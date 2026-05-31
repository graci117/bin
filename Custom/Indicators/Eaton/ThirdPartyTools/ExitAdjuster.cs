// Coded by Nobbi. https://dev.to/nobbi
// Based on the forum post https://ninjatrader.com/support/forum/forum/ninjatrader-8/indicator-development/96376-modifications-to-chart-wpf-elements-and-tab-considerations by Chelsea Bell (chelsea.bell@ninjatrader.com)
// Additional reference: https://github.com/ImRobbieRobski/BreakEvenPlusMinus-for-Ninja-Trader
// 
// Modified (Eaton request):
// - Adds two button sets: SL BE +/- X and TP BE +/- X
// - User-selectable placement for each set (Above Instrument or Below ATM)
// - Adjusts existing stop-loss and profit-target exit orders on the selected Account/Instrument
// - Guards against invalid stop/limit placement vs market so repeated clicks won't throw platform errors
// - Includes TypeConverter to show/hide settings based on enabled toggles
//
// Version: 1.0.4

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
#endregion

// NOTE: ExitAdjusterPanelPlacement must be visible to NinjaTrader's auto-generated caching code.
// Keeping it in the global namespace avoids CS0246 issues if you later move this script
// into a custom namespace or folder.
public enum ExitAdjusterPanelPlacement {
    AboveInstrument,
    BelowATM
}

namespace NinjaTrader.NinjaScript.Indicators.Eaton.ThirdPartyTools {
    /// <summary>
    /// ChartTrader helper that adds SL/TP "move to breakeven +/- X ticks" buttons.
    /// </summary>
    //[TypeConverter(typeof(ExitAdjusterPropertyConverter))]
    public class ExitAdjuster: Indicator {

        #region Constants
        private
        const string ScriptVersion = "1.0.4";
        private
        const string IndicatorName = "ExitAdjuster";
        private
        const string SystemVersion = " v" + ScriptVersion;

        public override string DisplayName {
            get {
                return IndicatorName + SystemVersion;
            }
        }
        #endregion

        #region WPF / ChartTrader wiring

        private Gui.Chart.Chart chartWindow;
        private System.Windows.Threading.DispatcherTimer uiWatchTimer;
        private Gui.Chart.ChartTab chartTab;
        private System.Windows.Controls.TabItem tabItem;

        private System.Windows.Controls.Grid chartTraderGrid;
        private System.Windows.Controls.Grid chartTraderButtonsGrid;

        private System.Windows.Controls.RowDefinition addedRowSlAbove;
        private System.Windows.Controls.RowDefinition addedRowTpAbove;
        private System.Windows.Controls.RowDefinition addedRowSlBelow;
        private System.Windows.Controls.RowDefinition addedRowTpBelow;

        private System.Windows.Controls.Grid slPanel;
        private System.Windows.Controls.Grid tpPanel;

        private System.Windows.Controls.Button[] slButtons;
        private System.Windows.Controls.Button[] tpButtons;

        private bool panelActive;
        private bool tabHandlerHooked;

        #endregion

        #region Market Data(Bid / Ask)
        // --- Market data snapshot for validation ---
        private double lastBid = double.NaN;
        private double lastAsk = double.NaN;
        private double lastLast = double.NaN;

        #endregion

        #region Internal Types
        private enum AdjustKind {
            StopLoss,
            ProfitTarget
        }

        private class ButtonTag {
            public AdjustKind Kind;
            public int TicksSigned;
        }

        private int GetEffectiveTicks(Position pos, int ticksSigned) {
            // Interpret +/- as "profit direction" relative to the position.
            // Long: + moves price up, Short: + moves price down.
            return (pos != null && pos.MarketPosition == MarketPosition.Short) ? -ticksSigned : ticksSigned;
        }

        private bool IsBuyExitAction(OrderAction a) {
            return a == OrderAction.Buy || a == OrderAction.BuyToCover;
        }

        private bool IsSellExitAction(OrderAction a) {
            return a == OrderAction.Sell;
        }

        private bool IsOrderTypeByName(Order order, string orderTypeName) {
            if (order == null)
                return false;

            try {
                return string.Equals(order.OrderType.ToString(), orderTypeName, StringComparison.OrdinalIgnoreCase);
            } catch {
                return false;
            }
        }

        #endregion

        #region NinjaScript Overrides
        protected override void OnStateChange() {
            if (State == State.SetDefaults) {
                Description = DisplayName + " - Adds ChartTrader buttons to move existing SL/TP exit orders to Breakeven +/- X ticks.";
                // Keep the script Name stable (no version) so templates don't carry an old version string.
                Name = IndicatorName;
                Label = IndicatorName;
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = false;
                PaintPriceMarkers = false;

                // New behavior defaults
                EnableSLButtons = true;
                EnableTPButtons = true;
                SLPlacement = ExitAdjusterPanelPlacement.AboveInstrument;
                TPPlacement = ExitAdjusterPanelPlacement.BelowATM;
                SLMinDistanceTicks = 1;
                TPMinDistanceTicks = 1;
                ModifyAllMatchingOrders = true;
                PrintDebug = false;
                // SL Buttons (signed ticks)
                SLButton1Enabled = true;
                SLButton1Ticks = 0;
                SLButton1Color = Brushes.RoyalBlue;

                SLButton2Enabled = true;
                SLButton2Ticks = 5;
                SLButton2Color = Brushes.Green;

                SLButton3Enabled = true;
                SLButton3Ticks = -5;
                SLButton3Color = Brushes.Red;

                SLButton4Enabled = false;
                SLButton4Ticks = 15;
                SLButton4Color = Brushes.Green;

                SLButton5Enabled = false;
                SLButton5Ticks = 25;
                SLButton5Color = Brushes.Green;

                SLButton6Enabled = false;
                SLButton6Ticks = 50;
                SLButton6Color = Brushes.Green;

                // TP Buttons (signed ticks)
                TPButton1Enabled = true;
                TPButton1Ticks = 0;
                TPButton1Color = Brushes.RoyalBlue;

                TPButton2Enabled = true;
                TPButton2Ticks = 5;
                TPButton2Color = Brushes.Green;

                TPButton3Enabled = true;
                TPButton3Ticks = -5;
                TPButton3Color = Brushes.Red;

                TPButton4Enabled = false;
                TPButton4Ticks = 15;
                TPButton4Color = Brushes.Green;

                TPButton5Enabled = false;
                TPButton5Ticks = 25;
                TPButton5Color = Brushes.Green;

                TPButton6Enabled = false;
                TPButton6Ticks = 50;
                TPButton6Color = Brushes.Green;

                // Legacy properties (kept for generated region compatibility)
                Button1 = false;
                Button1Color = Brushes.DimGray;
                Button1Ticks = 1;
                Button2 = false;
                Button2Color = Brushes.DimGray;
                Button2Ticks = 1;
                Button3 = false;
                Button3Color = Brushes.DimGray;
                Button3Ticks = 1;
                Button4 = false;
                Button4Color = Brushes.DimGray;
                Button4Ticks = 1;
            } else if (State == State.Historical) {
                if (ChartControl != null) {
                    ChartControl.Dispatcher.InvokeAsync(() => {
                        CreateWPFControls();
                    });
                }
            } else if (State == State.Terminated) {
                if (ChartControl != null) {
                    ChartControl.Dispatcher.InvokeAsync(() => {
                        DisposeWPFControls();
                    });
                }
            }
        }

        protected override void OnMarketData(MarketDataEventArgs e) {
            // Keep a lightweight snapshot for click-time validation.
            if (e == null)
                return;

            switch (e.MarketDataType) {
            case MarketDataType.Bid:
                lastBid = e.Price;
                break;
            case MarketDataType.Ask:
                lastAsk = e.Price;
                break;
            case MarketDataType.Last:
                lastLast = e.Price;
                break;
            }
        }

        protected override void OnBarUpdate() {}

        public override string ToString() {
            // Keep chart label + Configured list short (no parameter dump)
            return DisplayName;
        }

        #endregion

        #region Properties
        // =====================
        // Properties (New)
        // =====================
        #region New Properties

            // ---- Setup ----
        //[NinjaScriptProperty]
        //[Display(Name = "Label", Order = 0, GroupName = "Setup")]
		[Browsable(false)]
        [XmlIgnore]
        public string Label {
            get;
            set;
        }

        // ---- Stop buttons (SL BE) ----
        [NinjaScriptProperty]
        [Display(Name = "Enable SL Buttons", Order = 0, GroupName = "Stop Buttons (SL BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool EnableSLButtons {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Placement", Order = 1, GroupName = "Stop Buttons (SL BE)")]
        public ExitAdjusterPanelPlacement SLPlacement {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Min Distance (ticks)", Order = 2, GroupName = "Stop Buttons (SL BE)")]
        public int SLMinDistanceTicks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 1 Enabled", Order = 10, GroupName = "Stop Buttons (SL BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool SLButton1Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 1 Ticks (signed)", Order = 11, GroupName = "Stop Buttons (SL BE)")]
        public int SLButton1Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "SL Button 1 Color", Order = 12, GroupName = "Stop Buttons (SL BE)")]
        public Brush SLButton1Color {
            get;
            set;
        }

        [Browsable(false)]
        public string SLButton1ColorSerializable {
            get {
                return Serialize.BrushToString(SLButton1Color);
            }
            set {
                SLButton1Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 2 Enabled", Order = 20, GroupName = "Stop Buttons (SL BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool SLButton2Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 2 Ticks (signed)", Order = 21, GroupName = "Stop Buttons (SL BE)")]
        public int SLButton2Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "SL Button 2 Color", Order = 22, GroupName = "Stop Buttons (SL BE)")]
        public Brush SLButton2Color {
            get;
            set;
        }

        [Browsable(false)]
        public string SLButton2ColorSerializable {
            get {
                return Serialize.BrushToString(SLButton2Color);
            }
            set {
                SLButton2Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 3 Enabled", Order = 30, GroupName = "Stop Buttons (SL BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool SLButton3Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 3 Ticks (signed)", Order = 31, GroupName = "Stop Buttons (SL BE)")]
        public int SLButton3Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "SL Button 3 Color", Order = 32, GroupName = "Stop Buttons (SL BE)")]
        public Brush SLButton3Color {
            get;
            set;
        }

        [Browsable(false)]
        public string SLButton3ColorSerializable {
            get {
                return Serialize.BrushToString(SLButton3Color);
            }
            set {
                SLButton3Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 4 Enabled", Order = 40, GroupName = "Stop Buttons (SL BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool SLButton4Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 4 Ticks (signed)", Order = 41, GroupName = "Stop Buttons (SL BE)")]
        public int SLButton4Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "SL Button 4 Color", Order = 42, GroupName = "Stop Buttons (SL BE)")]
        public Brush SLButton4Color {
            get;
            set;
        }

        [Browsable(false)]
        public string SLButton4ColorSerializable {
            get {
                return Serialize.BrushToString(SLButton4Color);
            }
            set {
                SLButton4Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 5 Enabled", Order = 50, GroupName = "Stop Buttons (SL BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool SLButton5Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 5 Ticks (signed)", Order = 51, GroupName = "Stop Buttons (SL BE)")]
        public int SLButton5Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "SL Button 5 Color", Order = 52, GroupName = "Stop Buttons (SL BE)")]
        public Brush SLButton5Color {
            get;
            set;
        }

        [Browsable(false)]
        public string SLButton5ColorSerializable {
            get {
                return Serialize.BrushToString(SLButton5Color);
            }
            set {
                SLButton5Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 6 Enabled", Order = 60, GroupName = "Stop Buttons (SL BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool SLButton6Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "SL Button 6 Ticks (signed)", Order = 61, GroupName = "Stop Buttons (SL BE)")]
        public int SLButton6Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "SL Button 6 Color", Order = 62, GroupName = "Stop Buttons (SL BE)")]
        public Brush SLButton6Color {
            get;
            set;
        }

        [Browsable(false)]
        public string SLButton6ColorSerializable {
            get {
                return Serialize.BrushToString(SLButton6Color);
            }
            set {
                SLButton6Color = Serialize.StringToBrush(value);
            }
        }

        // ---- Target buttons (TP BE) ----
        [NinjaScriptProperty]
        [Display(Name = "Enable TP Buttons", Order = 0, GroupName = "Target Buttons (TP BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool EnableTPButtons {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Placement", Order = 1, GroupName = "Target Buttons (TP BE)")]
        public ExitAdjusterPanelPlacement TPPlacement {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Min Distance (ticks)", Order = 2, GroupName = "Target Buttons (TP BE)")]
        public int TPMinDistanceTicks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 1 Enabled", Order = 10, GroupName = "Target Buttons (TP BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool TPButton1Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 1 Ticks (signed)", Order = 11, GroupName = "Target Buttons (TP BE)")]
        public int TPButton1Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "TP Button 1 Color", Order = 12, GroupName = "Target Buttons (TP BE)")]
        public Brush TPButton1Color {
            get;
            set;
        }

        [Browsable(false)]
        public string TPButton1ColorSerializable {
            get {
                return Serialize.BrushToString(TPButton1Color);
            }
            set {
                TPButton1Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 2 Enabled", Order = 20, GroupName = "Target Buttons (TP BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool TPButton2Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 2 Ticks (signed)", Order = 21, GroupName = "Target Buttons (TP BE)")]
        public int TPButton2Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "TP Button 2 Color", Order = 22, GroupName = "Target Buttons (TP BE)")]
        public Brush TPButton2Color {
            get;
            set;
        }

        [Browsable(false)]
        public string TPButton2ColorSerializable {
            get {
                return Serialize.BrushToString(TPButton2Color);
            }
            set {
                TPButton2Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 3 Enabled", Order = 30, GroupName = "Target Buttons (TP BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool TPButton3Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 3 Ticks (signed)", Order = 31, GroupName = "Target Buttons (TP BE)")]
        public int TPButton3Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "TP Button 3 Color", Order = 32, GroupName = "Target Buttons (TP BE)")]
        public Brush TPButton3Color {
            get;
            set;
        }

        [Browsable(false)]
        public string TPButton3ColorSerializable {
            get {
                return Serialize.BrushToString(TPButton3Color);
            }
            set {
                TPButton3Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 4 Enabled", Order = 40, GroupName = "Target Buttons (TP BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool TPButton4Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 4 Ticks (signed)", Order = 41, GroupName = "Target Buttons (TP BE)")]
        public int TPButton4Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "TP Button 4 Color", Order = 42, GroupName = "Target Buttons (TP BE)")]
        public Brush TPButton4Color {
            get;
            set;
        }

        [Browsable(false)]
        public string TPButton4ColorSerializable {
            get {
                return Serialize.BrushToString(TPButton4Color);
            }
            set {
                TPButton4Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 5 Enabled", Order = 50, GroupName = "Target Buttons (TP BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool TPButton5Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 5 Ticks (signed)", Order = 51, GroupName = "Target Buttons (TP BE)")]
        public int TPButton5Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "TP Button 5 Color", Order = 52, GroupName = "Target Buttons (TP BE)")]
        public Brush TPButton5Color {
            get;
            set;
        }

        [Browsable(false)]
        public string TPButton5ColorSerializable {
            get {
                return Serialize.BrushToString(TPButton5Color);
            }
            set {
                TPButton5Color = Serialize.StringToBrush(value);
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 6 Enabled", Order = 60, GroupName = "Target Buttons (TP BE)")]
        [RefreshProperties(RefreshProperties.All)]
        public bool TPButton6Enabled {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "TP Button 6 Ticks (signed)", Order = 61, GroupName = "Target Buttons (TP BE)")]
        public int TPButton6Ticks {
            get;
            set;
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "TP Button 6 Color", Order = 62, GroupName = "Target Buttons (TP BE)")]
        public Brush TPButton6Color {
            get;
            set;
        }

        [Browsable(false)]
        public string TPButton6ColorSerializable {
            get {
                return Serialize.BrushToString(TPButton6Color);
            }
            set {
                TPButton6Color = Serialize.StringToBrush(value);
            }
        }

        // ---- Behavior ----
        [NinjaScriptProperty]
        [Display(Name = "Modify ALL matching orders", Order = 0, GroupName = "Behavior")]
        public bool ModifyAllMatchingOrders {
            get;
            set;
        }

        [NinjaScriptProperty]
        [Display(Name = "Print debug", Order = 1, GroupName = "Behavior")]
        public bool PrintDebug {
            get;
            set;
        }

        #endregion

        // =====================
        // Legacy properties (kept so we do not have to edit the generated region at the bottom)
        // =====================
        #region Legacy Properties(Hidden)

        [Browsable(false)]
        public bool Button1 {
            get;
            set;
        }

        [Browsable(false)]
        [XmlIgnore]
        public Brush Button1Color {
            get;
            set;
        }

        [Browsable(false)]
        public string Button1ColorSerializable {
            get {
                return Serialize.BrushToString(Button1Color);
            }
            set {
                Button1Color = Serialize.StringToBrush(value);
            }
        }

        [Browsable(false)]
        public int Button1Ticks {
            get;
            set;
        }

        [Browsable(false)]
        public bool Button2 {
            get;
            set;
        }

        [Browsable(false)]
        [XmlIgnore]
        public Brush Button2Color {
            get;
            set;
        }

        [Browsable(false)]
        public string Button2ColorSerializable {
            get {
                return Serialize.BrushToString(Button2Color);
            }
            set {
                Button2Color = Serialize.StringToBrush(value);
            }
        }

        [Browsable(false)]
        public int Button2Ticks {
            get;
            set;
        }

        [Browsable(false)]
        public bool Button3 {
            get;
            set;
        }

        [Browsable(false)]
        [XmlIgnore]
        public Brush Button3Color {
            get;
            set;
        }

        [Browsable(false)]
        public string Button3ColorSerializable {
            get {
                return Serialize.BrushToString(Button3Color);
            }
            set {
                Button3Color = Serialize.StringToBrush(value);
            }
        }

        [Browsable(false)]
        public int Button3Ticks {
            get;
            set;
        }

        [Browsable(false)]
        public bool Button4 {
            get;
            set;
        }

        [Browsable(false)]
        [XmlIgnore]
        public Brush Button4Color {
            get;
            set;
        }

        [Browsable(false)]
        public string Button4ColorSerializable {
            get {
                return Serialize.BrushToString(Button4Color);
            }
            set {
                Button4Color = Serialize.StringToBrush(value);
            }
        }

        [Browsable(false)]
        public int Button4Ticks {
            get;
            set;
        }

        #endregion
        #endregion

        #region UI
        // =====================
        // WPF Controls
        // =====================
        private void CreateWPFControls() {
            if (!TryResolveChartTraderGrids())
                return;

            // Build panels + buttons
            slButtons = new System.Windows.Controls.Button[6];
            tpButtons = new System.Windows.Controls.Button[6];

            slPanel = BuildPanel(
                "SL",
                AdjustKind.StopLoss,
                new(bool enabled, int ticks, Brush color)[] {
                    (SLButton1Enabled, SLButton1Ticks, SLButton1Color),
                    (SLButton2Enabled, SLButton2Ticks, SLButton2Color),
                    (SLButton3Enabled, SLButton3Ticks, SLButton3Color),
                    (SLButton4Enabled, SLButton4Ticks, SLButton4Color),
                    (SLButton5Enabled, SLButton5Ticks, SLButton5Color),
                    (SLButton6Enabled, SLButton6Ticks, SLButton6Color),
                },
                slButtons);

            tpPanel = BuildPanel(
                "TP",
                AdjustKind.ProfitTarget,
                new(bool enabled, int ticks, Brush color)[] {
                    (TPButton1Enabled, TPButton1Ticks, TPButton1Color),
                    (TPButton2Enabled, TPButton2Ticks, TPButton2Color),
                    (TPButton3Enabled, TPButton3Ticks, TPButton3Color),
                    (TPButton4Enabled, TPButton4Ticks, TPButton4Color),
                    (TPButton5Enabled, TPButton5Ticks, TPButton5Color),
                    (TPButton6Enabled, TPButton6Ticks, TPButton6Color),
                },
                tpButtons);

            if (IsActiveChartTab())
                EnsurePanelsForActiveTab();

            if (!tabHandlerHooked) {
                chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
                tabHandlerHooked = true;
            }

            StartUiWatchdog();
        }

        private void DisposeWPFControls() {
            StopUiWatchdog();

            if (chartWindow != null && tabHandlerHooked) {
                chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
                tabHandlerHooked = false;
            }

            DetachButtonHandlers(slButtons);
            DetachButtonHandlers(tpButtons);

            RemoveWPFControls();

            slPanel = null;
            tpPanel = null;
            slButtons = null;
            tpButtons = null;
        }

        private void DetachButtonHandlers(System.Windows.Controls.Button[] arr) {
            if (arr == null)
                return;
            foreach(var b in arr) {
                if (b == null)
                    continue;
                b.Click -= OnAdjustButtonClick;
            }
        }

        private System.Windows.Controls.Grid BuildPanel(
            string prefix,
            AdjustKind kind,
            (bool enabled, int ticks, Brush color)[] specs,
            System.Windows.Controls.Button[] buttonOut) {
            var panel = new System.Windows.Controls.Grid();
            panel.Margin = new Thickness(0, 0, 0, 0);

            Style basicButtonStyle = Application.Current.FindResource("BasicEntryButton") as Style;

            // Keep output array aligned to spec indexes
            for (int i = 0; i < specs.Length; i++)
                buttonOut[i] = null;

            var enabledSpecs = specs.Select((s, i) => (s, i)).Where(x => x.s.enabled).ToList();
            int enabledCount = enabledSpecs.Count;

            // We want up to 3 buttons per row (2 rows max for 6 buttons)
            int cols = enabledCount <= 0 ? 1 : Math.Min(3, enabledCount);

            var uniform = new System.Windows.Controls.Primitives.UniformGrid();
            uniform.Columns = cols;
            uniform.HorizontalAlignment = HorizontalAlignment.Stretch;
            uniform.VerticalAlignment = VerticalAlignment.Stretch;

            if (enabledCount == 0) {
                // Nothing enabled: show a placeholder (disabled) so the panel still has a footprint.
                var placeholder = new System.Windows.Controls.Button() {
                    Content = prefix + " BE",
                        IsEnabled = false,
                        Height = 30,
                        Margin = new Thickness(1),
                        Padding = new Thickness(1),
                        Style = basicButtonStyle,
                        Background = Brushes.DimGray,
                        BorderBrush = Brushes.DimGray
                };
                uniform.Children.Add(placeholder);
            } else {
                foreach(var item in enabledSpecs) {
                    int i = item.i;
                    var s = item.s;
                    string label = FormatButtonLabel(prefix, s.ticks);

                    var btn = new System.Windows.Controls.Button() {
                        Content = label,
                            Height = 30,
                            Margin = new Thickness(1),
                            Padding = new Thickness(1),
                            Style = basicButtonStyle,
                            Background = s.color,
                            BorderBrush = Brushes.DimGray,
                            Tag = new ButtonTag {
                                Kind = kind, TicksSigned = s.ticks
                            }
                    };

                    btn.Click += OnAdjustButtonClick;
                    uniform.Children.Add(btn);
                    buttonOut[i] = btn;
                }
            }

            panel.Children.Add(uniform);
            System.Windows.Controls.Grid.SetColumnSpan(panel, 4);
            return panel;
        }

        private string FormatButtonLabel(string prefix, int ticksSigned) {
            string sign = ticksSigned >= 0 ? "+" : "-";
            int abs = Math.Abs(ticksSigned);
            return string.Format("{0} BE {1} {2}", prefix, sign, abs);
        }

        private void InsertWPFControls() {
            if (panelActive)
                return;

            // SL panel
            if (EnableSLButtons && slPanel != null) {
                InsertPanel(SLPlacement, slPanel, isStopLossPanel: true);
            }

            // TP panel
            if (EnableTPButtons && tpPanel != null) {
                InsertPanel(TPPlacement, tpPanel, isStopLossPanel: false);
            }

            panelActive = true;
        }

        private void InsertPanel(ExitAdjusterPanelPlacement placement, System.Windows.Controls.Grid panel, bool isStopLossPanel) {
            // Choose host grid based on placement
            if (panel == null)
                return;
            if (placement == ExitAdjusterPanelPlacement.AboveInstrument) {
                if (chartTraderButtonsGrid == null || chartTraderButtonsGrid.Children.Contains(panel))
                    return;
                var row = new System.Windows.Controls.RowDefinition() {
                    Height = GridLength.Auto
                };
                chartTraderButtonsGrid.RowDefinitions.Add(row);
                System.Windows.Controls.Grid.SetRow(panel, chartTraderButtonsGrid.RowDefinitions.Count - 1);
                System.Windows.Controls.Grid.SetColumnSpan(panel, 3);
                chartTraderButtonsGrid.Children.Add(panel);

                if (isStopLossPanel) addedRowSlAbove = row;
                else addedRowTpAbove = row;
            } else {
                if (chartTraderGrid == null || chartTraderGrid.Children.Contains(panel))
                    return;
                var row = new System.Windows.Controls.RowDefinition() {
                    Height = GridLength.Auto
                };
                chartTraderGrid.RowDefinitions.Add(row);
                System.Windows.Controls.Grid.SetRow(panel, chartTraderGrid.RowDefinitions.Count - 1);
                System.Windows.Controls.Grid.SetColumnSpan(panel, 4);
                chartTraderGrid.Children.Add(panel);

                if (isStopLossPanel) addedRowSlBelow = row;
                else addedRowTpBelow = row;
            }
        }

        private void RemoveWPFControls() {
            if (!panelActive)
                return;

            // Remove panels if they were inserted
            SafeRemovePanel(slPanel, ExitAdjusterPanelPlacement.AboveInstrument, addedRowSlAbove);
            SafeRemovePanel(tpPanel, ExitAdjusterPanelPlacement.AboveInstrument, addedRowTpAbove);
            SafeRemovePanel(slPanel, ExitAdjusterPanelPlacement.BelowATM, addedRowSlBelow);
            SafeRemovePanel(tpPanel, ExitAdjusterPanelPlacement.BelowATM, addedRowTpBelow);

            addedRowSlAbove = null;
            addedRowTpAbove = null;
            addedRowSlBelow = null;
            addedRowTpBelow = null;

            panelActive = false;
        }

        private void SafeRemovePanel(System.Windows.Controls.Grid panel, ExitAdjusterPanelPlacement placement, System.Windows.Controls.RowDefinition row) {
            if (panel == null || row == null)
                return;

            try {
                if (placement == ExitAdjusterPanelPlacement.AboveInstrument) {
                    if (chartTraderButtonsGrid != null) {
                        chartTraderButtonsGrid.Children.Remove(panel);
                        chartTraderButtonsGrid.RowDefinitions.Remove(row);
                    }
                } else {
                    if (chartTraderGrid != null) {
                        chartTraderGrid.Children.Remove(panel);
                        chartTraderGrid.RowDefinitions.Remove(row);
                    }
                }
            } catch {
                // ignore - UI may already be disposed
            }
        }

        // -----------------------------
        // Tab + ChartTrader resilience
        // -----------------------------
        private bool IsActiveChartTab() {
            if (chartWindow == null || ChartControl == null)
                return false;

            object selected = chartWindow.MainTabControl.SelectedItem;

            // NT builds differ: sometimes SelectedItem is TabItem, sometimes the ChartTab itself.
            var ti = selected as System.Windows.Controls.TabItem;
            if (ti != null) {
                var ct = ti.Content as Gui.Chart.ChartTab;
                return ct != null && ct.ChartControl == ChartControl;
            }

            var ct2 = selected as Gui.Chart.ChartTab;
            if (ct2 != null)
                return ct2.ChartControl == ChartControl;

            return false;
        }

        #region UI Watchdog(Tab / F5 resiliency)

        private void StartUiWatchdog() {
            if (uiWatchTimer != null || ChartControl == null)
                return;

            uiWatchTimer = new System.Windows.Threading.DispatcherTimer();
            uiWatchTimer.Interval = TimeSpan.FromMilliseconds(500);
            uiWatchTimer.Tick += UiWatchdogTick;
            uiWatchTimer.Start();
        }

        private void StopUiWatchdog() {
            try {
                if (uiWatchTimer != null) {
                    uiWatchTimer.Stop();
                    uiWatchTimer.Tick -= UiWatchdogTick;
                    uiWatchTimer = null;
                }
            } catch {}
        }

        private void UiWatchdogTick(object sender, EventArgs e) {
            try {
                if (ChartControl == null || chartWindow == null)
                    return;

                if (!IsActiveChartTab())
                    return;

                // ChartTrader can re-template on F5 / reload; re-resolve fresh grid refs
                if (!TryResolveChartTraderGrids())
                    return;

                bool needReinsert = false;

                if (EnableSLButtons && slPanel != null && !IsPanelPresent(SLPlacement, slPanel))
                    needReinsert = true;

                if (EnableTPButtons && tpPanel != null && !IsPanelPresent(TPPlacement, tpPanel))
                    needReinsert = true;

                if (needReinsert) {
                    panelActive = false;
                    addedRowSlAbove = addedRowTpAbove = addedRowSlBelow = addedRowTpBelow = null;
                    InsertWPFControls();
                }
            } catch {}
        }

        private bool IsPanelPresent(ExitAdjusterPanelPlacement placement, System.Windows.Controls.Grid panel) {
            if (placement == ExitAdjusterPanelPlacement.AboveInstrument)
                return chartTraderButtonsGrid != null && chartTraderButtonsGrid.Children.Contains(panel);

            return chartTraderGrid != null && chartTraderGrid.Children.Contains(panel);
        }

        #endregion

        private bool TryResolveChartTraderGrids() {
            // Chart window can change after reload/tab switches
            chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
            if (chartWindow == null)
                return false;

            var chartTrader = chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader;
            if (chartTrader == null)
                return false;

            var rootGrid = chartTrader.Content as System.Windows.Controls.Grid;
            if (rootGrid == null || rootGrid.Children.Count == 0)
                return false;

            var buttonsGrid = rootGrid.Children[0] as System.Windows.Controls.Grid;
            if (buttonsGrid == null)
                return false;

            chartTraderGrid = rootGrid;
            chartTraderButtonsGrid = buttonsGrid;
            return true;
        }

        private void EnsurePanelsForActiveTab() {
            if (!TryResolveChartTraderGrids())
                return;

            // If ChartTrader re-templated, our panels may have been detached but panelActive may still be true.
            // Reset by attempting a safe remove, then re-insert.
            RemoveWPFControls();
            InsertWPFControls();
        }

        private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (ChartControl == null)
                return;

            ChartControl.Dispatcher.BeginInvoke(new Action(() => {
                if (IsActiveChartTab())
                    EnsurePanelsForActiveTab();
                else
                    RemoveWPFControls();
            }));
        }

        // =====================
        // Button Click -> Order Adjust
        // =====================
        private void OnAdjustButtonClick(object sender, RoutedEventArgs e) {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null)
                return;

            var tag = btn.Tag as ButtonTag;
            if (tag == null)
                return;

            AdjustOrders(tag.Kind, tag.TicksSigned);
        }

        #endregion

        #region Business Logic
        private void AdjustOrders(AdjustKind kind, int ticksSigned) {
            try {
                if (ChartControl == null)
                    return;

                var acctSelector = Window.GetWindow(ChartControl.Parent)?.FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
                Account acct = acctSelector?.SelectedAccount;
                if (acct == null) {
                    if (PrintDebug) Print("ExitAdjuster: No selected account.");
                    return;
                }

                Instrument instr = Instrument;
                if (instr == null) {
                    if (PrintDebug) Print("ExitAdjuster: No instrument.");
                    return;
                }

                Position pos = acct.Positions.FirstOrDefault(p => p.Instrument != null && p.Instrument.FullName == instr.FullName);
                if (pos == null || pos.MarketPosition == MarketPosition.Flat) {
                    if (PrintDebug) Print("ExitAdjuster: No open position.");
                    return;
                }

                double tickSize = instr.MasterInstrument.TickSize;
                int effectiveTicks = GetEffectiveTicks(pos, ticksSigned);
                double newPrice = pos.AveragePrice + (effectiveTicks * tickSize);

                if (!TryGetBestBidAsk(out double bid, out double ask, out double last)) {
                    if (PrintDebug) Print("ExitAdjuster: No bid/ask/last available yet (skipping to avoid errors).");
                    return;
                }

                List < Order > matches = FindMatchingExitOrders(acct, instr, pos, kind);
                if (matches.Count == 0) {
                    if (PrintDebug) Print("ExitAdjuster: No matching exit orders found.");
                    return;
                }

                int minDistTicks = (kind == AdjustKind.StopLoss ? Math.Max(0, SLMinDistanceTicks) : Math.Max(0, TPMinDistanceTicks));
                double minDist = minDistTicks * tickSize;

                var toChange = new List < Order > ();
                foreach(var order in matches) {
                    if (!IsPriceValidForOrder(order, newPrice, bid, ask, last, minDist)) {
                        if (PrintDebug) Print(string.Format("ExitAdjuster: Skip {0} {1} -> {2} (invalid vs market)", order.OrderAction, order.OrderType, newPrice));
                        continue;
                    }

                    if (ApplyPriceChange(order, newPrice))
                        toChange.Add(order);

                    if (!ModifyAllMatchingOrders)
                        break;
                }

                if (toChange.Count == 0) {
                    if (PrintDebug) Print("ExitAdjuster: No orders eligible to change (all invalid vs market).");
                    return;
                }

                acct.Change(toChange.ToArray());
                if (PrintDebug) Print(string.Format("ExitAdjuster: Changed {0} order(s) to {1}", toChange.Count, newPrice));
            } catch (Exception ex) {
                // Avoid throwing UI popups - only print when debugging.
                if (PrintDebug) Print("ExitAdjuster: Exception: " + ex);
            }
        }

        // NT builds differ on whether OrderType includes MarketIfTouched / LimitIfTouched.
        // To stay compatible, avoid hard enum references and match the enum name when present.
        private static bool IsOrderTypeName(Order order, params string[] names) {
            if (order == null || names == null || names.Length == 0)
                return false;

            string s;
            try {
                s = order.OrderType.ToString();
            } catch {
                return false;
            }

            for (int i = 0; i < names.Length; i++) {
                if (string.Equals(s, names[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private List < Order > FindMatchingExitOrders(Account acct, Instrument instr, Position pos, AdjustKind kind) {
            var list = new List < Order > ();
            if (acct == null || instr == null)
                return list;

            foreach(Order o in acct.Orders) {
                if (o == null)
                    continue;
                if (o.Account != acct)
                    continue;
                if (o.Instrument == null || o.Instrument.FullName != instr.FullName)
                    continue;
                if (o.OrderState == OrderState.Cancelled || o.OrderState == OrderState.Filled)
                    continue;
                if (o.OrderState == OrderState.Rejected)
                    continue;
                // Only adjust exit-direction orders (allow Buy OR BuyToCover for shorts)
                bool isExitAction =
                    (pos.MarketPosition == MarketPosition.Long && IsSellExitAction(o.OrderAction)) ||
                    (pos.MarketPosition == MarketPosition.Short && IsBuyExitAction(o.OrderAction));
                if (!isExitAction)
                    continue;

                if (kind == AdjustKind.StopLoss) {
                    if (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
                        list.Add(o);
                } else {
                    // Profit target style orders.
                    // NT versions differ on whether MIT/LIT exist in OrderType; avoid hard refs and match by name if present.
                    if (o.OrderType == OrderType.Limit || IsOrderTypeName(o, "MarketIfTouched", "MIT") || IsOrderTypeName(o, "LimitIfTouched", "LIT"))
                        list.Add(o);
                }
            }

            return list;
        }

        private bool ApplyPriceChange(Order order, double newPrice) {
            if (order == null)
                return false;

            // Stop orders
            if (order.OrderType == OrderType.StopMarket) {
                order.StopPriceChanged = newPrice;
                return true;
            }

            if (order.OrderType == OrderType.StopLimit) {
                // Preserve stop->limit offset, if any
                double oldStop = order.StopPrice;
                double oldLimit = order.LimitPrice;
                double delta = newPrice - oldStop;
                order.StopPriceChanged = newPrice;
                if (oldLimit > 0)
                    order.LimitPriceChanged = oldLimit + delta;
                return true;
            }

            // Limit target
            if (order.OrderType == OrderType.Limit) {
                order.LimitPriceChanged = newPrice;
                return true;
            }

            // MIT / LIT (if supported by this NT build)
            if (IsOrderTypeName(order, "MarketIfTouched", "MIT")) {
                order.StopPriceChanged = newPrice;
                return true;
            }

            if (IsOrderTypeName(order, "LimitIfTouched", "LIT")) {
                order.StopPriceChanged = newPrice;
                order.LimitPriceChanged = newPrice;
                return true;
            }

            return false;
        }

        private bool TryGetBestBidAsk(out double bid, out double ask, out double last) {
            // Use last snapshot from OnMarketData (works in real-time + Market Replay).
            bid = lastBid;
            ask = lastAsk;
            last = lastLast;

            if (double.IsNaN(bid) || bid <= 0) bid = 0;
            if (double.IsNaN(ask) || ask <= 0) ask = 0;
            if (double.IsNaN(last) || last <= 0) last = 0;

            return (bid > 0 && ask > 0) || (last > 0);
        }

        private bool IsPriceValidForOrder(Order order, double newPrice, double bid, double ask, double last, double minDist) {
            if (order == null)
                return false;

            // Prefer bid/ask; fallback to last.
            double refBid = bid > 0 ? bid : last;
            double refAsk = ask > 0 ? ask : last;

            // If still no reference, skip (prevents platform error popups).
            if (refBid <= 0 && refAsk <= 0)
                return false;

            bool isStop = (order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit);
            bool isTarget = (order.OrderType == OrderType.Limit || IsOrderTypeName(order, "MarketIfTouched", "MIT") || IsOrderTypeName(order, "LimitIfTouched", "LIT"));

            if (isStop) {
                if (IsSellExitAction(order.OrderAction)) {
                    // Sell stop must be below market
                    return newPrice < (refBid - minDist);
                }
                if (IsBuyExitAction(order.OrderAction)) {
                    // Buy stop must be above market
                    return newPrice > (refAsk + minDist);
                }
            }

            if (isTarget) {
                // Pure limit orders are always valid (can be marketable and fill immediately).
                if (order.OrderType == OrderType.Limit)
                    return true;

                // For MIT/LIT, use conservative validation to avoid platform errors.
                if (IsSellExitAction(order.OrderAction))
                    return newPrice > (refAsk + minDist);
                if (IsBuyExitAction(order.OrderAction))
                    return newPrice < (refBid - minDist);
            }

            return false;
        }

        #endregion

    }

    /// <summary>
    /// Hides settings based on enabled toggles to keep the property grid clean.
    /// </summary>
    public class ExitAdjusterPropertyConverter: IndicatorBaseConverter {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs) {
            PropertyDescriptorCollection pdc = base.GetPropertiesSupported(context) ?
                base.GetProperties(context, component, attrs) :
                TypeDescriptor.GetProperties(component, attrs);

            ExitAdjuster ind = component as ExitAdjuster;
            if (ind == null || pdc == null)
                return pdc;

            var keep = new List < PropertyDescriptor > ();
            var allowedCategories = new HashSet < string > (StringComparer.OrdinalIgnoreCase) {
                "Behavior",
                "Stop Buttons (SL BE)",
                "Target Buttons (TP BE)",
                "Setup"
            };

            var allowedSetupNames = new HashSet < string > (StringComparer.OrdinalIgnoreCase) {
                "Name",
                "Label",
                "Calculate",
                "MaximumBarsLookBack"
            };

            foreach(PropertyDescriptor p in pdc) {
                string n = p.Name ?? string.Empty;
                string category = p.Category ?? string.Empty;

                if (!allowedCategories.Contains(category))
                    continue;

                if (category.Equals("Setup", StringComparison.OrdinalIgnoreCase) && !allowedSetupNames.Contains(n))
                    continue;

                // Always hide legacy settings (kept only for generated caching region compatibility)
                if (n.StartsWith("Button", StringComparison.Ordinal))
                    continue;

                // SL section: if disabled, keep only the master toggle
                if (!ind.EnableSLButtons && n.StartsWith("SL", StringComparison.Ordinal) && n != nameof(ExitAdjuster.EnableSLButtons))
                    continue;

                // TP section: if disabled, keep only the master toggle
                if (!ind.EnableTPButtons && n.StartsWith("TP", StringComparison.Ordinal) && n != nameof(ExitAdjuster.EnableTPButtons))
                    continue;

                // Per-button filtering (SL)
                if (n.StartsWith("SLButton", StringComparison.Ordinal)) {
                    if (n.StartsWith("SLButton1", StringComparison.Ordinal) && !ind.SLButton1Enabled && n != nameof(ExitAdjuster.SLButton1Enabled))
                        continue;
                    if (n.StartsWith("SLButton2", StringComparison.Ordinal) && !ind.SLButton2Enabled && n != nameof(ExitAdjuster.SLButton2Enabled))
                        continue;
                    if (n.StartsWith("SLButton3", StringComparison.Ordinal) && !ind.SLButton3Enabled && n != nameof(ExitAdjuster.SLButton3Enabled))
                        continue;
                    if (n.StartsWith("SLButton4", StringComparison.Ordinal) && !ind.SLButton4Enabled && n != nameof(ExitAdjuster.SLButton4Enabled))
                        continue;
                }

                // Per-button filtering (TP)
                if (n.StartsWith("TPButton", StringComparison.Ordinal)) {
                    if (n.StartsWith("TPButton1", StringComparison.Ordinal) && !ind.TPButton1Enabled && n != nameof(ExitAdjuster.TPButton1Enabled))
                        continue;
                    if (n.StartsWith("TPButton2", StringComparison.Ordinal) && !ind.TPButton2Enabled && n != nameof(ExitAdjuster.TPButton2Enabled))
                        continue;
                    if (n.StartsWith("TPButton3", StringComparison.Ordinal) && !ind.TPButton3Enabled && n != nameof(ExitAdjuster.TPButton3Enabled))
                        continue;
                    if (n.StartsWith("TPButton4", StringComparison.Ordinal) && !ind.TPButton4Enabled && n != nameof(ExitAdjuster.TPButton4Enabled))
                        continue;
                }

                keep.Add(p);
            }

            return new PropertyDescriptorCollection(keep.ToArray(), true);
        }
    }
}

//Clean up C# - https://codebeautify.org/csharpviewer

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Eaton.ThirdPartyTools.ExitAdjuster[] cacheExitAdjuster;
		public Eaton.ThirdPartyTools.ExitAdjuster ExitAdjuster(bool enableSLButtons, ExitAdjusterPanelPlacement sLPlacement, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool enableTPButtons, ExitAdjusterPanelPlacement tPPlacement, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return ExitAdjuster(Input, enableSLButtons, sLPlacement, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, enableTPButtons, tPPlacement, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, modifyAllMatchingOrders, printDebug);
		}

		public Eaton.ThirdPartyTools.ExitAdjuster ExitAdjuster(ISeries<double> input, bool enableSLButtons, ExitAdjusterPanelPlacement sLPlacement, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool enableTPButtons, ExitAdjusterPanelPlacement tPPlacement, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			if (cacheExitAdjuster != null)
				for (int idx = 0; idx < cacheExitAdjuster.Length; idx++)
					if (cacheExitAdjuster[idx] != null && cacheExitAdjuster[idx].EnableSLButtons == enableSLButtons && cacheExitAdjuster[idx].SLPlacement == sLPlacement && cacheExitAdjuster[idx].SLMinDistanceTicks == sLMinDistanceTicks && cacheExitAdjuster[idx].SLButton1Enabled == sLButton1Enabled && cacheExitAdjuster[idx].SLButton1Ticks == sLButton1Ticks && cacheExitAdjuster[idx].SLButton1Color == sLButton1Color && cacheExitAdjuster[idx].SLButton2Enabled == sLButton2Enabled && cacheExitAdjuster[idx].SLButton2Ticks == sLButton2Ticks && cacheExitAdjuster[idx].SLButton2Color == sLButton2Color && cacheExitAdjuster[idx].SLButton3Enabled == sLButton3Enabled && cacheExitAdjuster[idx].SLButton3Ticks == sLButton3Ticks && cacheExitAdjuster[idx].SLButton3Color == sLButton3Color && cacheExitAdjuster[idx].SLButton4Enabled == sLButton4Enabled && cacheExitAdjuster[idx].SLButton4Ticks == sLButton4Ticks && cacheExitAdjuster[idx].SLButton4Color == sLButton4Color && cacheExitAdjuster[idx].SLButton5Enabled == sLButton5Enabled && cacheExitAdjuster[idx].SLButton5Ticks == sLButton5Ticks && cacheExitAdjuster[idx].SLButton5Color == sLButton5Color && cacheExitAdjuster[idx].SLButton6Enabled == sLButton6Enabled && cacheExitAdjuster[idx].SLButton6Ticks == sLButton6Ticks && cacheExitAdjuster[idx].SLButton6Color == sLButton6Color && cacheExitAdjuster[idx].EnableTPButtons == enableTPButtons && cacheExitAdjuster[idx].TPPlacement == tPPlacement && cacheExitAdjuster[idx].TPMinDistanceTicks == tPMinDistanceTicks && cacheExitAdjuster[idx].TPButton1Enabled == tPButton1Enabled && cacheExitAdjuster[idx].TPButton1Ticks == tPButton1Ticks && cacheExitAdjuster[idx].TPButton1Color == tPButton1Color && cacheExitAdjuster[idx].TPButton2Enabled == tPButton2Enabled && cacheExitAdjuster[idx].TPButton2Ticks == tPButton2Ticks && cacheExitAdjuster[idx].TPButton2Color == tPButton2Color && cacheExitAdjuster[idx].TPButton3Enabled == tPButton3Enabled && cacheExitAdjuster[idx].TPButton3Ticks == tPButton3Ticks && cacheExitAdjuster[idx].TPButton3Color == tPButton3Color && cacheExitAdjuster[idx].TPButton4Enabled == tPButton4Enabled && cacheExitAdjuster[idx].TPButton4Ticks == tPButton4Ticks && cacheExitAdjuster[idx].TPButton4Color == tPButton4Color && cacheExitAdjuster[idx].TPButton5Enabled == tPButton5Enabled && cacheExitAdjuster[idx].TPButton5Ticks == tPButton5Ticks && cacheExitAdjuster[idx].TPButton5Color == tPButton5Color && cacheExitAdjuster[idx].TPButton6Enabled == tPButton6Enabled && cacheExitAdjuster[idx].TPButton6Ticks == tPButton6Ticks && cacheExitAdjuster[idx].TPButton6Color == tPButton6Color && cacheExitAdjuster[idx].ModifyAllMatchingOrders == modifyAllMatchingOrders && cacheExitAdjuster[idx].PrintDebug == printDebug && cacheExitAdjuster[idx].EqualsInput(input))
						return cacheExitAdjuster[idx];
			return CacheIndicator<Eaton.ThirdPartyTools.ExitAdjuster>(new Eaton.ThirdPartyTools.ExitAdjuster(){ EnableSLButtons = enableSLButtons, SLPlacement = sLPlacement, SLMinDistanceTicks = sLMinDistanceTicks, SLButton1Enabled = sLButton1Enabled, SLButton1Ticks = sLButton1Ticks, SLButton1Color = sLButton1Color, SLButton2Enabled = sLButton2Enabled, SLButton2Ticks = sLButton2Ticks, SLButton2Color = sLButton2Color, SLButton3Enabled = sLButton3Enabled, SLButton3Ticks = sLButton3Ticks, SLButton3Color = sLButton3Color, SLButton4Enabled = sLButton4Enabled, SLButton4Ticks = sLButton4Ticks, SLButton4Color = sLButton4Color, SLButton5Enabled = sLButton5Enabled, SLButton5Ticks = sLButton5Ticks, SLButton5Color = sLButton5Color, SLButton6Enabled = sLButton6Enabled, SLButton6Ticks = sLButton6Ticks, SLButton6Color = sLButton6Color, EnableTPButtons = enableTPButtons, TPPlacement = tPPlacement, TPMinDistanceTicks = tPMinDistanceTicks, TPButton1Enabled = tPButton1Enabled, TPButton1Ticks = tPButton1Ticks, TPButton1Color = tPButton1Color, TPButton2Enabled = tPButton2Enabled, TPButton2Ticks = tPButton2Ticks, TPButton2Color = tPButton2Color, TPButton3Enabled = tPButton3Enabled, TPButton3Ticks = tPButton3Ticks, TPButton3Color = tPButton3Color, TPButton4Enabled = tPButton4Enabled, TPButton4Ticks = tPButton4Ticks, TPButton4Color = tPButton4Color, TPButton5Enabled = tPButton5Enabled, TPButton5Ticks = tPButton5Ticks, TPButton5Color = tPButton5Color, TPButton6Enabled = tPButton6Enabled, TPButton6Ticks = tPButton6Ticks, TPButton6Color = tPButton6Color, ModifyAllMatchingOrders = modifyAllMatchingOrders, PrintDebug = printDebug }, input, ref cacheExitAdjuster);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Eaton.ThirdPartyTools.ExitAdjuster ExitAdjuster(bool enableSLButtons, ExitAdjusterPanelPlacement sLPlacement, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool enableTPButtons, ExitAdjusterPanelPlacement tPPlacement, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.ExitAdjuster(Input, enableSLButtons, sLPlacement, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, enableTPButtons, tPPlacement, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, modifyAllMatchingOrders, printDebug);
		}

		public Indicators.Eaton.ThirdPartyTools.ExitAdjuster ExitAdjuster(ISeries<double> input , bool enableSLButtons, ExitAdjusterPanelPlacement sLPlacement, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool enableTPButtons, ExitAdjusterPanelPlacement tPPlacement, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.ExitAdjuster(input, enableSLButtons, sLPlacement, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, enableTPButtons, tPPlacement, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, modifyAllMatchingOrders, printDebug);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Eaton.ThirdPartyTools.ExitAdjuster ExitAdjuster(bool enableSLButtons, ExitAdjusterPanelPlacement sLPlacement, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool enableTPButtons, ExitAdjusterPanelPlacement tPPlacement, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.ExitAdjuster(Input, enableSLButtons, sLPlacement, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, enableTPButtons, tPPlacement, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, modifyAllMatchingOrders, printDebug);
		}

		public Indicators.Eaton.ThirdPartyTools.ExitAdjuster ExitAdjuster(ISeries<double> input , bool enableSLButtons, ExitAdjusterPanelPlacement sLPlacement, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool enableTPButtons, ExitAdjusterPanelPlacement tPPlacement, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.ExitAdjuster(input, enableSLButtons, sLPlacement, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, enableTPButtons, tPPlacement, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, modifyAllMatchingOrders, printDebug);
		}
	}
}

#endregion
