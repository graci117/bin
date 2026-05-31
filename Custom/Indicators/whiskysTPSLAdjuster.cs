// whisky's TP & SL Adjuster
// Floating draggable WPF panel with SL/TP breakeven adjustment buttons,
// Account selector, ATM selector, FLATTEN and CANCEL ALL controls.
//
// Based on the original ChartTrader injection approach by Nobbi (https://dev.to/nobbi)
// Floating panel approach inspired by HelloWin_CaptainOptimusStrong (NinZa)
//
// Version: 1.3.0

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript.AtmStrategy;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.Whisky
{
	/// <summary>
	/// Floating draggable panel with SL/TP breakeven adjustment buttons,
	/// Account selector, ATM selector, FLATTEN and CANCEL ALL controls.
	/// Up to 12 buttons per set; each individually enabled. Max 3 per row.
	/// </summary>
	[TypeConverter(typeof(WhiskysTPSLAdjusterConverter))]
	public class WhiskysTPSLAdjuster : Indicator
	{
		#region Constants
		private const string ScriptVersion      = "1.3.0";
		private const string ScriptVersionLabel = "1.30";
		private const string IndicatorName      = "Whisky's TP & SL Adjuster";
		private const string SystemVersion      = " v" + ScriptVersionLabel;
		private const int    MaxButtons         = 12;
		private const int    ButtonsPerRow      = 3;

		public override string DisplayName
		{
			get { return IndicatorName + SystemVersion; }
		}
		#endregion

		#region WPF / Floating Panel

		private Gui.Chart.Chart                  chartWindow;
		private Gui.Chart.ChartTab               chartTab;
		private System.Windows.Controls.TabItem  tabItem;

		private System.Windows.Controls.Border   floatingPanel;
		private System.Windows.Controls.ComboBox accountCombo;
		private AtmStrategySelector              atmSelector;

		private System.Windows.Point dragStartMouse;
		private bool                 isDragging    = false;
		private int                  sizeStepIndex = 0;
		private static readonly double[] PanelScales = { 1.0, 0.8 };

		private List<System.Windows.Controls.Button> slButtons;
		private List<System.Windows.Controls.Button> tpButtons;

		#endregion

		#region Market Data (Bid/Ask)
		private double lastBid  = double.NaN;
		private double lastAsk  = double.NaN;
		private double lastLast = double.NaN;
		#endregion

		#region Internal Types
		private enum AdjustKind { StopLoss, ProfitTarget }

		private class ButtonTag
		{
			public AdjustKind Kind;
			public int TicksSigned;
		}

		private int GetEffectiveTicks(Position pos, int ticksSigned)
		{
			return (pos != null && pos.MarketPosition == MarketPosition.Short) ? -ticksSigned : ticksSigned;
		}

		private bool IsBuyExitAction(OrderAction a)
		{
			return a == OrderAction.Buy || a == OrderAction.BuyToCover;
		}

		private bool IsSellExitAction(OrderAction a)
		{
			return a == OrderAction.Sell;
		}

		private bool IsOrderTypeName(Order order, params string[] names)
		{
			if (order == null || names == null || names.Length == 0)
				return false;
			string s;
			try { s = order.OrderType.ToString(); }
			catch { return false; }
			for (int i = 0; i < names.Length; i++)
				if (string.Equals(s, names[i], StringComparison.OrdinalIgnoreCase))
					return true;
			return false;
		}
		#endregion

		#region NinjaScript Overrides
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description             = DisplayName + " - Floating panel with SL/TP breakeven adjustment buttons, Account/ATM selectors, Flatten and Cancel All.";
				Name                    = IndicatorName;
				Label                   = IndicatorName;
				Calculate               = Calculate.OnPriceChange;
				IsOverlay               = true;
				DisplayInDataBox        = false;
				PaintPriceMarkers       = false;

				PanelLeft               = 10;
				PanelTop                = 10;
				PanelMaxWidth           = 300;
				EnableSLButtons         = true;
				EnableTPButtons         = true;
				SLMinDistanceTicks      = 1;
				TPMinDistanceTicks      = 1;
				ModifyAllMatchingOrders = true;
				PrintDebug              = false;

				// SL buttons — only button 1 enabled by default
				SLButton1Enabled  = true;  SLButton1Ticks  = 0;   SLButton1Color  = Brushes.DimGray;
				SLButton2Enabled  = false; SLButton2Ticks  = 5;   SLButton2Color  = Brushes.DimGray;
				SLButton3Enabled  = false; SLButton3Ticks  = -5;  SLButton3Color  = Brushes.DimGray;
				SLButton4Enabled  = false; SLButton4Ticks  = 10;  SLButton4Color  = Brushes.DimGray;
				SLButton5Enabled  = false; SLButton5Ticks  = -10; SLButton5Color  = Brushes.DimGray;
				SLButton6Enabled  = false; SLButton6Ticks  = 15;  SLButton6Color  = Brushes.DimGray;
				SLButton7Enabled  = false; SLButton7Ticks  = -15; SLButton7Color  = Brushes.DimGray;
				SLButton8Enabled  = false; SLButton8Ticks  = 20;  SLButton8Color  = Brushes.DimGray;
				SLButton9Enabled  = false; SLButton9Ticks  = -20; SLButton9Color  = Brushes.DimGray;
				SLButton10Enabled = false; SLButton10Ticks = 25;  SLButton10Color = Brushes.DimGray;
				SLButton11Enabled = false; SLButton11Ticks = -25; SLButton11Color = Brushes.DimGray;
				SLButton12Enabled = false; SLButton12Ticks = 30;  SLButton12Color = Brushes.DimGray;

				// TP buttons — only button 1 enabled by default
				TPButton1Enabled  = true;  TPButton1Ticks  = 0;   TPButton1Color  = Brushes.DimGray;
				TPButton2Enabled  = false; TPButton2Ticks  = 5;   TPButton2Color  = Brushes.DimGray;
				TPButton3Enabled  = false; TPButton3Ticks  = -5;  TPButton3Color  = Brushes.DimGray;
				TPButton4Enabled  = false; TPButton4Ticks  = 10;  TPButton4Color  = Brushes.DimGray;
				TPButton5Enabled  = false; TPButton5Ticks  = -10; TPButton5Color  = Brushes.DimGray;
				TPButton6Enabled  = false; TPButton6Ticks  = 15;  TPButton6Color  = Brushes.DimGray;
				TPButton7Enabled  = false; TPButton7Ticks  = -15; TPButton7Color  = Brushes.DimGray;
				TPButton8Enabled  = false; TPButton8Ticks  = 20;  TPButton8Color  = Brushes.DimGray;
				TPButton9Enabled  = false; TPButton9Ticks  = -20; TPButton9Color  = Brushes.DimGray;
				TPButton10Enabled = false; TPButton10Ticks = 25;  TPButton10Color = Brushes.DimGray;
				TPButton11Enabled = false; TPButton11Ticks = -25; TPButton11Color = Brushes.DimGray;
				TPButton12Enabled = false; TPButton12Ticks = 30;  TPButton12Color = Brushes.DimGray;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
					ChartControl.Dispatcher.InvokeAsync(() => CreateWPFControls());
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
					ChartControl.Dispatcher.InvokeAsync(() => DisposeWPFControls());
			}
		}

		protected override void OnMarketData(MarketDataEventArgs e)
		{
			if (e == null) return;
			switch (e.MarketDataType)
			{
				case MarketDataType.Bid:  lastBid  = e.Price; break;
				case MarketDataType.Ask:  lastAsk  = e.Price; break;
				case MarketDataType.Last: lastLast = e.Price; break;
			}
		}

		protected override void OnBarUpdate() { }

		public override string ToString() { return DisplayName; }
		#endregion

		#region Properties

		[NinjaScriptProperty]
		[Display(Name = "Label", Order = 0, GroupName = "Setup")]
		public string Label { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Panel Left (px)", Order = 1, GroupName = "Setup")]
		public double PanelLeft { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Panel Top (px)", Order = 2, GroupName = "Setup")]
		public double PanelTop { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Panel Max Width (px)", Order = 3, GroupName = "Setup")]
		public double PanelMaxWidth { get; set; }

		// =====================================================================
		// Stop Buttons (SL BE)
		// =====================================================================

		[NinjaScriptProperty]
		[Display(Name = "Enable SL Buttons", Order = 0, GroupName = "Stop Buttons (SL BE)")]
		[RefreshProperties(RefreshProperties.All)]
		public bool EnableSLButtons { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Min Distance (ticks)", Order = 1, GroupName = "Stop Buttons (SL BE)")]
		public int SLMinDistanceTicks { get; set; }

		// SL Button 1
		[NinjaScriptProperty]
		[Display(Name = "SL Button 1 Enabled", Order = 10, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton1Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 1 Ticks (signed)", Order = 11, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton1Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 1 Color", Order = 12, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton1Color { get; set; }

		[Browsable(false)]
		public string SLButton1ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton1Color); }
			set { SLButton1Color = Serialize.StringToBrush(value); }
		}

		// SL Button 2
		[NinjaScriptProperty]
		[Display(Name = "SL Button 2 Enabled", Order = 20, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton2Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 2 Ticks (signed)", Order = 21, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton2Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 2 Color", Order = 22, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton2Color { get; set; }

		[Browsable(false)]
		public string SLButton2ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton2Color); }
			set { SLButton2Color = Serialize.StringToBrush(value); }
		}

		// SL Button 3
		[NinjaScriptProperty]
		[Display(Name = "SL Button 3 Enabled", Order = 30, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton3Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 3 Ticks (signed)", Order = 31, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton3Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 3 Color", Order = 32, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton3Color { get; set; }

		[Browsable(false)]
		public string SLButton3ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton3Color); }
			set { SLButton3Color = Serialize.StringToBrush(value); }
		}

		// SL Button 4
		[NinjaScriptProperty]
		[Display(Name = "SL Button 4 Enabled", Order = 40, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton4Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 4 Ticks (signed)", Order = 41, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton4Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 4 Color", Order = 42, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton4Color { get; set; }

		[Browsable(false)]
		public string SLButton4ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton4Color); }
			set { SLButton4Color = Serialize.StringToBrush(value); }
		}

		// SL Button 5
		[NinjaScriptProperty]
		[Display(Name = "SL Button 5 Enabled", Order = 50, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton5Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 5 Ticks (signed)", Order = 51, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton5Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 5 Color", Order = 52, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton5Color { get; set; }

		[Browsable(false)]
		public string SLButton5ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton5Color); }
			set { SLButton5Color = Serialize.StringToBrush(value); }
		}

		// SL Button 6
		[NinjaScriptProperty]
		[Display(Name = "SL Button 6 Enabled", Order = 60, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton6Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 6 Ticks (signed)", Order = 61, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton6Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 6 Color", Order = 62, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton6Color { get; set; }

		[Browsable(false)]
		public string SLButton6ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton6Color); }
			set { SLButton6Color = Serialize.StringToBrush(value); }
		}

		// SL Button 7
		[NinjaScriptProperty]
		[Display(Name = "SL Button 7 Enabled", Order = 70, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton7Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 7 Ticks (signed)", Order = 71, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton7Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 7 Color", Order = 72, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton7Color { get; set; }

		[Browsable(false)]
		public string SLButton7ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton7Color); }
			set { SLButton7Color = Serialize.StringToBrush(value); }
		}

		// SL Button 8
		[NinjaScriptProperty]
		[Display(Name = "SL Button 8 Enabled", Order = 80, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton8Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 8 Ticks (signed)", Order = 81, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton8Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 8 Color", Order = 82, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton8Color { get; set; }

		[Browsable(false)]
		public string SLButton8ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton8Color); }
			set { SLButton8Color = Serialize.StringToBrush(value); }
		}

		// SL Button 9
		[NinjaScriptProperty]
		[Display(Name = "SL Button 9 Enabled", Order = 90, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton9Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 9 Ticks (signed)", Order = 91, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton9Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 9 Color", Order = 92, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton9Color { get; set; }

		[Browsable(false)]
		public string SLButton9ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton9Color); }
			set { SLButton9Color = Serialize.StringToBrush(value); }
		}

		// SL Button 10
		[NinjaScriptProperty]
		[Display(Name = "SL Button 10 Enabled", Order = 100, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton10Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 10 Ticks (signed)", Order = 101, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton10Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 10 Color", Order = 102, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton10Color { get; set; }

		[Browsable(false)]
		public string SLButton10ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton10Color); }
			set { SLButton10Color = Serialize.StringToBrush(value); }
		}

		// SL Button 11
		[NinjaScriptProperty]
		[Display(Name = "SL Button 11 Enabled", Order = 110, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton11Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 11 Ticks (signed)", Order = 111, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton11Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 11 Color", Order = 112, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton11Color { get; set; }

		[Browsable(false)]
		public string SLButton11ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton11Color); }
			set { SLButton11Color = Serialize.StringToBrush(value); }
		}

		// SL Button 12
		[NinjaScriptProperty]
		[Display(Name = "SL Button 12 Enabled", Order = 120, GroupName = "Stop Buttons (SL BE)")]
		public bool SLButton12Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "SL Button 12 Ticks (signed)", Order = 121, GroupName = "Stop Buttons (SL BE)")]
		public int SLButton12Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 12 Color", Order = 122, GroupName = "Stop Buttons (SL BE)")]
		public Brush SLButton12Color { get; set; }

		[Browsable(false)]
		public string SLButton12ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton12Color); }
			set { SLButton12Color = Serialize.StringToBrush(value); }
		}

		// =====================================================================
		// Target Buttons (TP BE)
		// =====================================================================

		[NinjaScriptProperty]
		[Display(Name = "Enable TP Buttons", Order = 0, GroupName = "Target Buttons (TP BE)")]
		[RefreshProperties(RefreshProperties.All)]
		public bool EnableTPButtons { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Min Distance (ticks)", Order = 1, GroupName = "Target Buttons (TP BE)")]
		public int TPMinDistanceTicks { get; set; }

		// TP Button 1
		[NinjaScriptProperty]
		[Display(Name = "TP Button 1 Enabled", Order = 10, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton1Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 1 Ticks (signed)", Order = 11, GroupName = "Target Buttons (TP BE)")]
		public int TPButton1Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 1 Color", Order = 12, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton1Color { get; set; }

		[Browsable(false)]
		public string TPButton1ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton1Color); }
			set { TPButton1Color = Serialize.StringToBrush(value); }
		}

		// TP Button 2
		[NinjaScriptProperty]
		[Display(Name = "TP Button 2 Enabled", Order = 20, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton2Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 2 Ticks (signed)", Order = 21, GroupName = "Target Buttons (TP BE)")]
		public int TPButton2Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 2 Color", Order = 22, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton2Color { get; set; }

		[Browsable(false)]
		public string TPButton2ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton2Color); }
			set { TPButton2Color = Serialize.StringToBrush(value); }
		}

		// TP Button 3
		[NinjaScriptProperty]
		[Display(Name = "TP Button 3 Enabled", Order = 30, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton3Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 3 Ticks (signed)", Order = 31, GroupName = "Target Buttons (TP BE)")]
		public int TPButton3Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 3 Color", Order = 32, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton3Color { get; set; }

		[Browsable(false)]
		public string TPButton3ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton3Color); }
			set { TPButton3Color = Serialize.StringToBrush(value); }
		}

		// TP Button 4
		[NinjaScriptProperty]
		[Display(Name = "TP Button 4 Enabled", Order = 40, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton4Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 4 Ticks (signed)", Order = 41, GroupName = "Target Buttons (TP BE)")]
		public int TPButton4Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 4 Color", Order = 42, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton4Color { get; set; }

		[Browsable(false)]
		public string TPButton4ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton4Color); }
			set { TPButton4Color = Serialize.StringToBrush(value); }
		}

		// TP Button 5
		[NinjaScriptProperty]
		[Display(Name = "TP Button 5 Enabled", Order = 50, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton5Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 5 Ticks (signed)", Order = 51, GroupName = "Target Buttons (TP BE)")]
		public int TPButton5Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 5 Color", Order = 52, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton5Color { get; set; }

		[Browsable(false)]
		public string TPButton5ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton5Color); }
			set { TPButton5Color = Serialize.StringToBrush(value); }
		}

		// TP Button 6
		[NinjaScriptProperty]
		[Display(Name = "TP Button 6 Enabled", Order = 60, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton6Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 6 Ticks (signed)", Order = 61, GroupName = "Target Buttons (TP BE)")]
		public int TPButton6Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 6 Color", Order = 62, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton6Color { get; set; }

		[Browsable(false)]
		public string TPButton6ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton6Color); }
			set { TPButton6Color = Serialize.StringToBrush(value); }
		}

		// TP Button 7
		[NinjaScriptProperty]
		[Display(Name = "TP Button 7 Enabled", Order = 70, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton7Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 7 Ticks (signed)", Order = 71, GroupName = "Target Buttons (TP BE)")]
		public int TPButton7Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 7 Color", Order = 72, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton7Color { get; set; }

		[Browsable(false)]
		public string TPButton7ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton7Color); }
			set { TPButton7Color = Serialize.StringToBrush(value); }
		}

		// TP Button 8
		[NinjaScriptProperty]
		[Display(Name = "TP Button 8 Enabled", Order = 80, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton8Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 8 Ticks (signed)", Order = 81, GroupName = "Target Buttons (TP BE)")]
		public int TPButton8Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 8 Color", Order = 82, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton8Color { get; set; }

		[Browsable(false)]
		public string TPButton8ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton8Color); }
			set { TPButton8Color = Serialize.StringToBrush(value); }
		}

		// TP Button 9
		[NinjaScriptProperty]
		[Display(Name = "TP Button 9 Enabled", Order = 90, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton9Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 9 Ticks (signed)", Order = 91, GroupName = "Target Buttons (TP BE)")]
		public int TPButton9Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 9 Color", Order = 92, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton9Color { get; set; }

		[Browsable(false)]
		public string TPButton9ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton9Color); }
			set { TPButton9Color = Serialize.StringToBrush(value); }
		}

		// TP Button 10
		[NinjaScriptProperty]
		[Display(Name = "TP Button 10 Enabled", Order = 100, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton10Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 10 Ticks (signed)", Order = 101, GroupName = "Target Buttons (TP BE)")]
		public int TPButton10Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 10 Color", Order = 102, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton10Color { get; set; }

		[Browsable(false)]
		public string TPButton10ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton10Color); }
			set { TPButton10Color = Serialize.StringToBrush(value); }
		}

		// TP Button 11
		[NinjaScriptProperty]
		[Display(Name = "TP Button 11 Enabled", Order = 110, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton11Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 11 Ticks (signed)", Order = 111, GroupName = "Target Buttons (TP BE)")]
		public int TPButton11Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 11 Color", Order = 112, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton11Color { get; set; }

		[Browsable(false)]
		public string TPButton11ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton11Color); }
			set { TPButton11Color = Serialize.StringToBrush(value); }
		}

		// TP Button 12
		[NinjaScriptProperty]
		[Display(Name = "TP Button 12 Enabled", Order = 120, GroupName = "Target Buttons (TP BE)")]
		public bool TPButton12Enabled { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "TP Button 12 Ticks (signed)", Order = 121, GroupName = "Target Buttons (TP BE)")]
		public int TPButton12Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "TP Button 12 Color", Order = 122, GroupName = "Target Buttons (TP BE)")]
		public Brush TPButton12Color { get; set; }

		[Browsable(false)]
		public string TPButton12ColorSerializable
		{
			get { return Serialize.BrushToString(TPButton12Color); }
			set { TPButton12Color = Serialize.StringToBrush(value); }
		}

		// ---- Behavior ----
		[NinjaScriptProperty]
		[Display(Name = "Modify ALL matching orders", Order = 0, GroupName = "Behavior")]
		public bool ModifyAllMatchingOrders { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Print debug", Order = 1, GroupName = "Behavior")]
		public bool PrintDebug { get; set; }

		#endregion

		#region UI

		private (int ticks, Brush color)[] GetButtonSpecs(AdjustKind kind)
		{
			if (kind == AdjustKind.StopLoss)
			{
				var all = new (bool enabled, int ticks, Brush color)[]
				{
					(SLButton1Enabled,  SLButton1Ticks,  SLButton1Color),
					(SLButton2Enabled,  SLButton2Ticks,  SLButton2Color),
					(SLButton3Enabled,  SLButton3Ticks,  SLButton3Color),
					(SLButton4Enabled,  SLButton4Ticks,  SLButton4Color),
					(SLButton5Enabled,  SLButton5Ticks,  SLButton5Color),
					(SLButton6Enabled,  SLButton6Ticks,  SLButton6Color),
					(SLButton7Enabled,  SLButton7Ticks,  SLButton7Color),
					(SLButton8Enabled,  SLButton8Ticks,  SLButton8Color),
					(SLButton9Enabled,  SLButton9Ticks,  SLButton9Color),
					(SLButton10Enabled, SLButton10Ticks, SLButton10Color),
					(SLButton11Enabled, SLButton11Ticks, SLButton11Color),
					(SLButton12Enabled, SLButton12Ticks, SLButton12Color),
				};
				return all.Where(s => s.enabled).Select(s => (s.ticks, s.color)).ToArray();
			}
			else
			{
				var all = new (bool enabled, int ticks, Brush color)[]
				{
					(TPButton1Enabled,  TPButton1Ticks,  TPButton1Color),
					(TPButton2Enabled,  TPButton2Ticks,  TPButton2Color),
					(TPButton3Enabled,  TPButton3Ticks,  TPButton3Color),
					(TPButton4Enabled,  TPButton4Ticks,  TPButton4Color),
					(TPButton5Enabled,  TPButton5Ticks,  TPButton5Color),
					(TPButton6Enabled,  TPButton6Ticks,  TPButton6Color),
					(TPButton7Enabled,  TPButton7Ticks,  TPButton7Color),
					(TPButton8Enabled,  TPButton8Ticks,  TPButton8Color),
					(TPButton9Enabled,  TPButton9Ticks,  TPButton9Color),
					(TPButton10Enabled, TPButton10Ticks, TPButton10Color),
					(TPButton11Enabled, TPButton11Ticks, TPButton11Color),
					(TPButton12Enabled, TPButton12Ticks, TPButton12Color),
				};
				return all.Where(s => s.enabled).Select(s => (s.ticks, s.color)).ToArray();
			}
		}

		private void CreateWPFControls()
		{
			chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
			if (chartWindow == null) return;

			slButtons     = new List<System.Windows.Controls.Button>();
			tpButtons     = new List<System.Windows.Controls.Button>();
			floatingPanel = BuildFloatingPanel();

			floatingPanel.Visibility = TabSelected() ? Visibility.Visible : Visibility.Collapsed;
			UserControlCollection.Add(floatingPanel);

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			DetachButtonHandlers(slButtons);
			DetachButtonHandlers(tpButtons);

			if (floatingPanel != null)
			{
				try { UserControlCollection.Remove(floatingPanel); } catch { }
				floatingPanel = null;
			}

			accountCombo = null;
			atmSelector  = null;
		}

		private void DetachButtonHandlers(List<System.Windows.Controls.Button> list)
		{
			if (list == null) return;
			foreach (var b in list)
				if (b != null) b.Click -= OnAdjustButtonClick;
		}

		private System.Windows.Controls.Border BuildFloatingPanel()
		{
			var titleColor = new SolidColorBrush(Color.FromRgb(45, 80, 130));

			var panel = new System.Windows.Controls.Border()
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment   = VerticalAlignment.Top,
				Margin              = new Thickness(PanelLeft, PanelTop, 0, 0),
				MaxWidth            = PanelMaxWidth,
				Background          = new SolidColorBrush(Color.FromArgb(230, 25, 25, 25)),
				BorderBrush         = titleColor,
				BorderThickness     = new Thickness(1),
				Cursor              = System.Windows.Input.Cursors.Arrow
			};
			var inner = new System.Windows.Controls.Grid();

			// ── Title / drag bar ─────────────────────────────────────────────
			var titleBar = new System.Windows.Controls.Border()
			{
				Background = titleColor,
				Height     = 24,
				Cursor     = System.Windows.Input.Cursors.SizeAll
			};
			var titleText = new System.Windows.Controls.TextBlock()
			{
				Text                = IndicatorName + SystemVersion,
				Foreground          = Brushes.White,
				FontSize            = 10,
				FontWeight          = FontWeights.SemiBold,
				VerticalAlignment   = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Center,
				Margin              = new Thickness(4, 0, 4, 0)
			};
			titleBar.Child = titleText;
			titleBar.MouseLeftButtonDown += OnTitleMouseDown;
			titleBar.MouseMove           += OnTitleMouseMove;
			titleBar.MouseLeftButtonUp   += OnTitleMouseUp;

			// ── Account selector ─────────────────────────────────────────────
			var activeAccounts = Account.All
				.Where(a => a.ConnectionStatus == ConnectionStatus.Connected)
				.ToList();
			accountCombo = new System.Windows.Controls.ComboBox()
			{
				Margin            = new Thickness(2, 0, 2, 1),
				DisplayMemberPath = "DisplayName",
				MinWidth          = 120
			};
			accountCombo.ItemsSource = activeAccounts;
			if (activeAccounts.Count > 0) accountCombo.SelectedIndex = 0;

			// ── ATM selector ─────────────────────────────────────────────────
			atmSelector = new AtmStrategySelector()
			{
				Margin   = new Thickness(2, 0, 2, 2),
				MinWidth = 0
			};

			// ── SL buttons ───────────────────────────────────────────────────
			var slLabel = MakeSectionLabel("Stop Loss (BE)");
			var slSpecs = GetButtonSpecs(AdjustKind.StopLoss);
			var slContent = BuildButtonSection(slSpecs, AdjustKind.StopLoss, slButtons);

			// ── TP buttons ───────────────────────────────────────────────────
			var tpLabel = MakeSectionLabel("Take Profit (BE)");
			var tpSpecs = GetButtonSpecs(AdjustKind.ProfitTarget);
			var tpContent = BuildButtonSection(tpSpecs, AdjustKind.ProfitTarget, tpButtons);

			// ── Move SL row ──────────────────────────────────────────────────
			var slMoveRow = new System.Windows.Controls.Grid() { Margin = new Thickness(2, 0, 2, 1) };
			slMoveRow.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			slMoveRow.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			slMoveRow.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			slMoveRow.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

			var slMinus10 = MakeMoveSLButton("▼ 10", -10);
			var slMinus1  = MakeMoveSLButton("▼ 1",  -1);
			var slPlus1   = MakeMoveSLButton("▲ 1",  +1);
			var slPlus10  = MakeMoveSLButton("▲ 10", +10);

			System.Windows.Controls.Grid.SetColumn(slMinus10, 0);
			System.Windows.Controls.Grid.SetColumn(slMinus1,  1);
			System.Windows.Controls.Grid.SetColumn(slPlus1,   2);
			System.Windows.Controls.Grid.SetColumn(slPlus10,  3);

			slMoveRow.Children.Add(slMinus10);
			slMoveRow.Children.Add(slMinus1);
			slMoveRow.Children.Add(slPlus1);
			slMoveRow.Children.Add(slPlus10);

			// ── FLATTEN / CANCEL ALL row ──────────────────────────────────────
			var actionRow = new System.Windows.Controls.Grid() { Margin = new Thickness(2, 2, 2, 2) };
			actionRow.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			actionRow.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

			var flatBtn = new System.Windows.Controls.Button()
			{
				Content     = "FLATTEN",
				Height      = 24,
				FontSize    = 11,
				Margin      = new Thickness(1),
				Foreground  = Brushes.White,
				Background  = new SolidColorBrush(Color.FromRgb(170, 50, 50)),
				BorderBrush = new SolidColorBrush(Color.FromRgb(210, 70, 70)),
				FontWeight  = FontWeights.Bold
			};
			flatBtn.Click += OnFlattenClick;
			System.Windows.Controls.Grid.SetColumn(flatBtn, 0);

			var cancelBtn = new System.Windows.Controls.Button()
			{
				Content     = "CANCEL ALL",
				Height      = 24,
				FontSize    = 11,
				Margin      = new Thickness(1),
				Foreground  = Brushes.White,
				Background  = new SolidColorBrush(Color.FromRgb(140, 95, 20)),
				BorderBrush = new SolidColorBrush(Color.FromRgb(190, 135, 30)),
				FontWeight  = FontWeights.Bold
			};
			cancelBtn.Click += OnCancelAllClick;
			System.Windows.Controls.Grid.SetColumn(cancelBtn, 1);

			actionRow.Children.Add(flatBtn);
			actionRow.Children.Add(cancelBtn);

			// ── Stack everything ──────────────────────────────────────────────
			var stack = new System.Windows.Controls.StackPanel() { Margin = new Thickness(0, 0, 0, 1) };
			stack.Children.Add(titleBar);
			stack.Children.Add(MakeSectionLabel("Account"));
			stack.Children.Add(accountCombo);
			stack.Children.Add(MakeSectionLabel("ATM Strategy"));
			stack.Children.Add(atmSelector);
			if (EnableSLButtons)
			{
				stack.Children.Add(slLabel);
				stack.Children.Add(slContent);
			}
			if (EnableTPButtons)
			{
				stack.Children.Add(tpLabel);
				stack.Children.Add(tpContent);
			}
			stack.Children.Add(MakeSectionLabel("Move Stop Loss"));
			stack.Children.Add(slMoveRow);
			stack.Children.Add(actionRow);

			inner.Children.Add(stack);
			panel.Child = inner;

			double s = PanelScales[sizeStepIndex];
			panel.LayoutTransform = new ScaleTransform(s, s);

			return panel;
		}

		private System.Windows.Controls.TextBlock MakeSectionLabel(string text)
		{
			return new System.Windows.Controls.TextBlock()
			{
				Text                = text,
				Foreground          = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
				FontSize            = 9,
				FontWeight          = FontWeights.SemiBold,
				HorizontalAlignment = HorizontalAlignment.Center,
				Margin              = new Thickness(2, 3, 2, 0)
			};
		}

		private System.Windows.UIElement BuildButtonSection(
			(int ticks, Brush color)[] specs,
			AdjustKind kind,
			List<System.Windows.Controls.Button> buttonList)
		{
			if (specs.Length == 0)
			{
				return new System.Windows.Controls.TextBlock()
				{
					Text                = "(none enabled)",
					Foreground          = Brushes.Gray,
					FontSize            = 10,
					HorizontalAlignment = HorizontalAlignment.Center,
					Margin              = new Thickness(4, 2, 4, 2)
				};
			}

			string prefix = kind == AdjustKind.StopLoss ? "SL" : "TP";
			var grid = new System.Windows.Controls.Primitives.UniformGrid()
			{
				Columns = Math.Min(ButtonsPerRow, specs.Length),
				Margin  = new Thickness(2, 0, 2, 0)
			};
			foreach (var s in specs)
			{
				var btn = new System.Windows.Controls.Button()
				{
					Content     = FormatButtonLabel(prefix, s.ticks),
					Height      = 22,
					Margin      = new Thickness(1),
					Padding     = new Thickness(0),
					Background  = s.color,
					BorderBrush = Brushes.DimGray,
					Foreground  = Brushes.White,
					Tag         = new ButtonTag { Kind = kind, TicksSigned = s.ticks }
				};
				btn.Click += OnAdjustButtonClick;
				grid.Children.Add(btn);
				buttonList.Add(btn);
			}
			return grid;
		}

		private string FormatButtonLabel(string prefix, int ticksSigned)
		{
			string sign = ticksSigned >= 0 ? "+" : "-";
			return string.Format("{0} BE {1}{2}", prefix, sign, Math.Abs(ticksSigned));
		}

		private bool TabSelected()
		{
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					return true;
			return false;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0) return;
			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null) return;
			chartTab = tabItem.Content as Gui.Chart.ChartTab;
			if (chartTab == null) return;

			if (floatingPanel != null)
				floatingPanel.Visibility = TabSelected() ? Visibility.Visible : Visibility.Collapsed;
		}

		private void OnTitleMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				sizeStepIndex = (sizeStepIndex + 1) % PanelScales.Length;
				double s = PanelScales[sizeStepIndex];
				if (floatingPanel != null)
					floatingPanel.LayoutTransform = new ScaleTransform(s, s);
				e.Handled = true;
				return;
			}

			var el = sender as UIElement;
			if (el == null) return;
			isDragging     = true;
			dragStartMouse = e.GetPosition(null);
			el.CaptureMouse();
		}

		private void OnTitleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (!isDragging || floatingPanel == null) return;
			var current = e.GetPosition(null);
			double dx = current.X - dragStartMouse.X;
			double dy = current.Y - dragStartMouse.Y;
			dragStartMouse = current;
			double newLeft = Math.Max(0, floatingPanel.Margin.Left + dx);
			double newTop  = Math.Max(0, floatingPanel.Margin.Top  + dy);
			floatingPanel.Margin = new Thickness(newLeft, newTop, 0, 0);
		}

		private void OnTitleMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			isDragging = false;
			var el = sender as UIElement;
			if (el != null) el.ReleaseMouseCapture();
		}

		private void OnFlattenClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var acct  = accountCombo?.SelectedItem as Account;
				var instr = Instrument;
				if (acct == null || instr == null) return;
				acct.Flatten(new[] { instr });
				if (PrintDebug) Print(IndicatorName + ": Flatten submitted.");
			}
			catch (Exception ex)
			{
				if (PrintDebug) Print(IndicatorName + ": Flatten exception: " + ex);
			}
		}

		private void OnCancelAllClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var acct  = accountCombo?.SelectedItem as Account;
				var instr = Instrument;
				if (acct == null || instr == null) return;

				var toCancel = acct.Orders
					.Where(o => o != null
						&& o.Instrument != null && o.Instrument.FullName == instr.FullName
						&& o.OrderState != OrderState.Cancelled
						&& o.OrderState != OrderState.Filled
						&& o.OrderState != OrderState.Rejected)
					.ToList();

				if (toCancel.Count > 0)
					acct.Cancel(toCancel.ToArray());

				if (PrintDebug) Print(IndicatorName + ": Cancelled " + toCancel.Count + " order(s).");
			}
			catch (Exception ex)
			{
				if (PrintDebug) Print(IndicatorName + ": CancelAll exception: " + ex);
			}
		}

		private void OnAdjustButtonClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as System.Windows.Controls.Button;
			if (btn == null) return;
			var tag = btn.Tag as ButtonTag;
			if (tag == null) return;
			AdjustOrders(tag.Kind, tag.TicksSigned);
		}

		private System.Windows.Controls.Button MakeMoveSLButton(string label, int deltaTicks)
		{
			var btn = new System.Windows.Controls.Button()
			{
				Content     = label,
				Height      = 22,
				Margin      = new Thickness(1),
				Padding     = new Thickness(0),
				Foreground  = Brushes.White,
				Background  = deltaTicks > 0
					? new SolidColorBrush(Color.FromRgb(50, 100, 50))
					: new SolidColorBrush(Color.FromRgb(110, 50, 50)),
				BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
				Tag         = deltaTicks
			};
			btn.Click += OnMoveSLClick;
			return btn;
		}

		private void OnMoveSLClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as System.Windows.Controls.Button;
			if (btn == null || !(btn.Tag is int delta)) return;
			AdjustSLByTicks(delta);
		}

		#endregion

		#region Business Logic
		private void AdjustOrders(AdjustKind kind, int ticksSigned)
		{
			try
			{
				if (ChartControl == null)
					return;

				Account acct = accountCombo?.SelectedItem as Account;
				if (acct == null)
				{
					if (PrintDebug) Print(IndicatorName + ": No account selected.");
					return;
				}

				Instrument instr = Instrument;
				if (instr == null)
				{
					if (PrintDebug) Print(IndicatorName + ": No instrument.");
					return;
				}

				Position pos = acct.Positions.FirstOrDefault(p => p.Instrument != null && p.Instrument.FullName == instr.FullName);
				if (pos == null || pos.MarketPosition == MarketPosition.Flat)
				{
					if (PrintDebug) Print("ExitAdjuster: No open position.");
					return;
				}

				double tickSize    = instr.MasterInstrument.TickSize;
				int effectiveTicks = GetEffectiveTicks(pos, ticksSigned);
				double newPrice    = pos.AveragePrice + (effectiveTicks * tickSize);

				if (!TryGetBestBidAsk(out double bid, out double ask, out double last))
				{
					if (PrintDebug) Print("ExitAdjuster: No bid/ask/last available yet (skipping to avoid errors).");
					return;
				}

				List<Order> matches = FindMatchingExitOrders(acct, instr, pos, kind);
				if (matches.Count == 0)
				{
					if (PrintDebug) Print("ExitAdjuster: No matching exit orders found.");
					return;
				}

				int minDistTicks = kind == AdjustKind.StopLoss ? Math.Max(0, SLMinDistanceTicks) : Math.Max(0, TPMinDistanceTicks);
				double minDist   = minDistTicks * tickSize;

				var toChange = new List<Order>();
				foreach (var order in matches)
				{
					if (!IsPriceValidForOrder(order, newPrice, bid, ask, last, minDist))
					{
						if (PrintDebug) Print(string.Format("ExitAdjuster: Skip {0} {1} -> {2} (invalid vs market)", order.OrderAction, order.OrderType, newPrice));
						continue;
					}

					if (ApplyPriceChange(order, newPrice))
						toChange.Add(order);

					if (!ModifyAllMatchingOrders)
						break;
				}

				if (toChange.Count == 0)
				{
					if (PrintDebug) Print("ExitAdjuster: No orders eligible to change (all invalid vs market).");
					return;
				}

				acct.Change(toChange.ToArray());
				if (PrintDebug) Print(string.Format("ExitAdjuster: Changed {0} order(s) to {1}", toChange.Count, newPrice));
			}
			catch (Exception ex)
			{
				if (PrintDebug) Print("ExitAdjuster: Exception: " + ex);
			}
		}

		private void AdjustSLByTicks(int deltaTicks)
		{
			try
			{
				Account acct = accountCombo?.SelectedItem as Account;
				if (acct == null)
				{
					if (PrintDebug) Print(IndicatorName + ": AdjustSLByTicks: No account selected.");
					return;
				}

				Instrument instr = Instrument;
				if (instr == null)
				{
					if (PrintDebug) Print(IndicatorName + ": AdjustSLByTicks: No instrument.");
					return;
				}

				Position pos = acct.Positions.FirstOrDefault(p => p.Instrument != null && p.Instrument.FullName == instr.FullName);
				if (pos == null || pos.MarketPosition == MarketPosition.Flat)
				{
					if (PrintDebug) Print(IndicatorName + ": AdjustSLByTicks: No open position.");
					return;
				}

				double tickSize = instr.MasterInstrument.TickSize;

				if (!TryGetBestBidAsk(out double bid, out double ask, out double last))
				{
					if (PrintDebug) Print(IndicatorName + ": AdjustSLByTicks: No bid/ask/last available.");
					return;
				}

				double minDist = Math.Max(0, SLMinDistanceTicks) * tickSize;

				List<Order> matches = FindMatchingExitOrders(acct, instr, pos, AdjustKind.StopLoss);
				if (matches.Count == 0)
				{
					if (PrintDebug) Print(IndicatorName + ": AdjustSLByTicks: No SL orders found.");
					return;
				}

				var toChange = new List<Order>();
				foreach (var order in matches)
				{
					double newPrice = order.StopPrice + (deltaTicks * tickSize);

					if (!IsPriceValidForOrder(order, newPrice, bid, ask, last, minDist))
					{
						if (PrintDebug) Print(string.Format("{0}: AdjustSLByTicks: Skip {1} -> {2} (invalid vs market)", IndicatorName, order.StopPrice, newPrice));
						continue;
					}

					if (ApplyPriceChange(order, newPrice))
						toChange.Add(order);

					if (!ModifyAllMatchingOrders)
						break;
				}

				if (toChange.Count == 0)
				{
					if (PrintDebug) Print(IndicatorName + ": AdjustSLByTicks: No orders eligible to change.");
					return;
				}

				acct.Change(toChange.ToArray());
				if (PrintDebug) Print(string.Format("{0}: AdjustSLByTicks: Moved SL by {1} tick(s) on {2} order(s).", IndicatorName, deltaTicks, toChange.Count));
			}
			catch (Exception ex)
			{
				if (PrintDebug) Print(IndicatorName + ": AdjustSLByTicks exception: " + ex);
			}
		}

		private List<Order> FindMatchingExitOrders(Account acct, Instrument instr, Position pos, AdjustKind kind)
		{
			var list = new List<Order>();
			if (acct == null || instr == null)
				return list;

			foreach (Order o in acct.Orders)
			{
				if (o == null) continue;
				if (o.Account != acct) continue;
				if (o.Instrument == null || o.Instrument.FullName != instr.FullName) continue;
				if (o.OrderState == OrderState.Cancelled || o.OrderState == OrderState.Filled || o.OrderState == OrderState.Rejected) continue;

				bool isExitAction =
					(pos.MarketPosition == MarketPosition.Long  && IsSellExitAction(o.OrderAction))
				 || (pos.MarketPosition == MarketPosition.Short && IsBuyExitAction(o.OrderAction));
				if (!isExitAction) continue;

				if (kind == AdjustKind.StopLoss)
				{
					if (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit)
						list.Add(o);
				}
				else
				{
					if (o.OrderType == OrderType.Limit || IsOrderTypeName(o, "MarketIfTouched", "MIT") || IsOrderTypeName(o, "LimitIfTouched", "LIT"))
						list.Add(o);
				}
			}

			return list;
		}

		private bool ApplyPriceChange(Order order, double newPrice)
		{
			if (order == null) return false;

			if (order.OrderType == OrderType.StopMarket)
			{
				order.StopPriceChanged = newPrice;
				return true;
			}

			if (order.OrderType == OrderType.StopLimit)
			{
				double delta = newPrice - order.StopPrice;
				order.StopPriceChanged = newPrice;
				if (order.LimitPrice > 0)
					order.LimitPriceChanged = order.LimitPrice + delta;
				return true;
			}

			if (order.OrderType == OrderType.Limit)
			{
				order.LimitPriceChanged = newPrice;
				return true;
			}

			if (IsOrderTypeName(order, "MarketIfTouched", "MIT"))
			{
				order.StopPriceChanged = newPrice;
				return true;
			}

			if (IsOrderTypeName(order, "LimitIfTouched", "LIT"))
			{
				order.StopPriceChanged  = newPrice;
				order.LimitPriceChanged = newPrice;
				return true;
			}

			return false;
		}

		private bool TryGetBestBidAsk(out double bid, out double ask, out double last)
		{
			bid  = double.IsNaN(lastBid)  || lastBid  <= 0 ? 0 : lastBid;
			ask  = double.IsNaN(lastAsk)  || lastAsk  <= 0 ? 0 : lastAsk;
			last = double.IsNaN(lastLast) || lastLast <= 0 ? 0 : lastLast;
			return (bid > 0 && ask > 0) || last > 0;
		}

		private bool IsPriceValidForOrder(Order order, double newPrice, double bid, double ask, double last, double minDist)
		{
			if (order == null) return false;

			double refBid = bid > 0 ? bid : last;
			double refAsk = ask > 0 ? ask : last;

			if (refBid <= 0 && refAsk <= 0)
				return false;

			bool isStop   = order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit;
			bool isTarget = order.OrderType == OrderType.Limit || IsOrderTypeName(order, "MarketIfTouched", "MIT") || IsOrderTypeName(order, "LimitIfTouched", "LIT");

			if (isStop)
			{
				if (IsSellExitAction(order.OrderAction)) return newPrice < (refBid - minDist);
				if (IsBuyExitAction(order.OrderAction))  return newPrice > (refAsk + minDist);
			}

			if (isTarget)
			{
				if (order.OrderType == OrderType.Limit) return true;
				if (IsSellExitAction(order.OrderAction)) return newPrice > (refAsk + minDist);
				if (IsBuyExitAction(order.OrderAction))  return newPrice < (refBid - minDist);
			}

			return false;
		}

		#endregion

	}

	/// <summary>
	/// Hides Ticks/Color for disabled buttons and collapses entire SL/TP sections when master toggle is off.
	/// </summary>
	public class WhiskysTPSLAdjusterConverter : IndicatorBaseConverter
	{
		// Built once from the type — names of every property declared directly on WhiskysTPSLAdjuster.
		// Filtering by name is the only approach that survives NinjaTrader's descriptor wrapping.
		private static readonly HashSet<string> OwnPropertyNames = new HashSet<string>(
			typeof(WhiskysTPSLAdjuster)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
				.Select(pi => pi.Name),
			StringComparer.Ordinal
		);

		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			PropertyDescriptorCollection pdc = base.GetPropertiesSupported(context)
				? base.GetProperties(context, component, attrs)
				: TypeDescriptor.GetProperties(component, attrs);

			WhiskysTPSLAdjuster ind = component as WhiskysTPSLAdjuster;
			if (ind == null || pdc == null)
				return pdc;

			var keep = new List<PropertyDescriptor>();

			foreach (PropertyDescriptor p in pdc)
			{
				// Hide everything not declared on our class (Input Plot, Timeframe, Misc, etc.)
				if (!OwnPropertyNames.Contains(p.Name))
					continue;

				string n = p.Name;

				// Collapse entire SL section (except master toggle) when SL is disabled
				if (!ind.EnableSLButtons && n.StartsWith("SL", StringComparison.Ordinal) && n != nameof(WhiskysTPSLAdjuster.EnableSLButtons))
					continue;

				// Collapse entire TP section (except master toggle) when TP is disabled
				if (!ind.EnableTPButtons && n.StartsWith("TP", StringComparison.Ordinal) && n != nameof(WhiskysTPSLAdjuster.EnableTPButtons))
					continue;

				keep.Add(p);
			}

			return new PropertyDescriptorCollection(keep.ToArray());
		}

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Whisky.WhiskysTPSLAdjuster[] cacheWhiskysTPSLAdjuster;
		public Whisky.WhiskysTPSLAdjuster WhiskysTPSLAdjuster(string label, double panelLeft, double panelTop, double panelMaxWidth, bool enableSLButtons, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool sLButton7Enabled, int sLButton7Ticks, Brush sLButton7Color, bool sLButton8Enabled, int sLButton8Ticks, Brush sLButton8Color, bool sLButton9Enabled, int sLButton9Ticks, Brush sLButton9Color, bool sLButton10Enabled, int sLButton10Ticks, Brush sLButton10Color, bool sLButton11Enabled, int sLButton11Ticks, Brush sLButton11Color, bool sLButton12Enabled, int sLButton12Ticks, Brush sLButton12Color, bool enableTPButtons, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool tPButton7Enabled, int tPButton7Ticks, Brush tPButton7Color, bool tPButton8Enabled, int tPButton8Ticks, Brush tPButton8Color, bool tPButton9Enabled, int tPButton9Ticks, Brush tPButton9Color, bool tPButton10Enabled, int tPButton10Ticks, Brush tPButton10Color, bool tPButton11Enabled, int tPButton11Ticks, Brush tPButton11Color, bool tPButton12Enabled, int tPButton12Ticks, Brush tPButton12Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return WhiskysTPSLAdjuster(Input, label, panelLeft, panelTop, panelMaxWidth, enableSLButtons, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, sLButton7Enabled, sLButton7Ticks, sLButton7Color, sLButton8Enabled, sLButton8Ticks, sLButton8Color, sLButton9Enabled, sLButton9Ticks, sLButton9Color, sLButton10Enabled, sLButton10Ticks, sLButton10Color, sLButton11Enabled, sLButton11Ticks, sLButton11Color, sLButton12Enabled, sLButton12Ticks, sLButton12Color, enableTPButtons, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, tPButton7Enabled, tPButton7Ticks, tPButton7Color, tPButton8Enabled, tPButton8Ticks, tPButton8Color, tPButton9Enabled, tPButton9Ticks, tPButton9Color, tPButton10Enabled, tPButton10Ticks, tPButton10Color, tPButton11Enabled, tPButton11Ticks, tPButton11Color, tPButton12Enabled, tPButton12Ticks, tPButton12Color, modifyAllMatchingOrders, printDebug);
		}

		public Whisky.WhiskysTPSLAdjuster WhiskysTPSLAdjuster(ISeries<double> input, string label, double panelLeft, double panelTop, double panelMaxWidth, bool enableSLButtons, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool sLButton7Enabled, int sLButton7Ticks, Brush sLButton7Color, bool sLButton8Enabled, int sLButton8Ticks, Brush sLButton8Color, bool sLButton9Enabled, int sLButton9Ticks, Brush sLButton9Color, bool sLButton10Enabled, int sLButton10Ticks, Brush sLButton10Color, bool sLButton11Enabled, int sLButton11Ticks, Brush sLButton11Color, bool sLButton12Enabled, int sLButton12Ticks, Brush sLButton12Color, bool enableTPButtons, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool tPButton7Enabled, int tPButton7Ticks, Brush tPButton7Color, bool tPButton8Enabled, int tPButton8Ticks, Brush tPButton8Color, bool tPButton9Enabled, int tPButton9Ticks, Brush tPButton9Color, bool tPButton10Enabled, int tPButton10Ticks, Brush tPButton10Color, bool tPButton11Enabled, int tPButton11Ticks, Brush tPButton11Color, bool tPButton12Enabled, int tPButton12Ticks, Brush tPButton12Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			if (cacheWhiskysTPSLAdjuster != null)
				for (int idx = 0; idx < cacheWhiskysTPSLAdjuster.Length; idx++)
					if (cacheWhiskysTPSLAdjuster[idx] != null && cacheWhiskysTPSLAdjuster[idx].Label == label && cacheWhiskysTPSLAdjuster[idx].PanelLeft == panelLeft && cacheWhiskysTPSLAdjuster[idx].PanelTop == panelTop && cacheWhiskysTPSLAdjuster[idx].PanelMaxWidth == panelMaxWidth && cacheWhiskysTPSLAdjuster[idx].EnableSLButtons == enableSLButtons && cacheWhiskysTPSLAdjuster[idx].SLMinDistanceTicks == sLMinDistanceTicks && cacheWhiskysTPSLAdjuster[idx].SLButton1Enabled == sLButton1Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton1Ticks == sLButton1Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton1Color == sLButton1Color && cacheWhiskysTPSLAdjuster[idx].SLButton2Enabled == sLButton2Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton2Ticks == sLButton2Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton2Color == sLButton2Color && cacheWhiskysTPSLAdjuster[idx].SLButton3Enabled == sLButton3Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton3Ticks == sLButton3Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton3Color == sLButton3Color && cacheWhiskysTPSLAdjuster[idx].SLButton4Enabled == sLButton4Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton4Ticks == sLButton4Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton4Color == sLButton4Color && cacheWhiskysTPSLAdjuster[idx].SLButton5Enabled == sLButton5Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton5Ticks == sLButton5Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton5Color == sLButton5Color && cacheWhiskysTPSLAdjuster[idx].SLButton6Enabled == sLButton6Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton6Ticks == sLButton6Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton6Color == sLButton6Color && cacheWhiskysTPSLAdjuster[idx].SLButton7Enabled == sLButton7Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton7Ticks == sLButton7Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton7Color == sLButton7Color && cacheWhiskysTPSLAdjuster[idx].SLButton8Enabled == sLButton8Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton8Ticks == sLButton8Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton8Color == sLButton8Color && cacheWhiskysTPSLAdjuster[idx].SLButton9Enabled == sLButton9Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton9Ticks == sLButton9Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton9Color == sLButton9Color && cacheWhiskysTPSLAdjuster[idx].SLButton10Enabled == sLButton10Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton10Ticks == sLButton10Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton10Color == sLButton10Color && cacheWhiskysTPSLAdjuster[idx].SLButton11Enabled == sLButton11Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton11Ticks == sLButton11Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton11Color == sLButton11Color && cacheWhiskysTPSLAdjuster[idx].SLButton12Enabled == sLButton12Enabled && cacheWhiskysTPSLAdjuster[idx].SLButton12Ticks == sLButton12Ticks && cacheWhiskysTPSLAdjuster[idx].SLButton12Color == sLButton12Color && cacheWhiskysTPSLAdjuster[idx].EnableTPButtons == enableTPButtons && cacheWhiskysTPSLAdjuster[idx].TPMinDistanceTicks == tPMinDistanceTicks && cacheWhiskysTPSLAdjuster[idx].TPButton1Enabled == tPButton1Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton1Ticks == tPButton1Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton1Color == tPButton1Color && cacheWhiskysTPSLAdjuster[idx].TPButton2Enabled == tPButton2Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton2Ticks == tPButton2Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton2Color == tPButton2Color && cacheWhiskysTPSLAdjuster[idx].TPButton3Enabled == tPButton3Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton3Ticks == tPButton3Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton3Color == tPButton3Color && cacheWhiskysTPSLAdjuster[idx].TPButton4Enabled == tPButton4Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton4Ticks == tPButton4Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton4Color == tPButton4Color && cacheWhiskysTPSLAdjuster[idx].TPButton5Enabled == tPButton5Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton5Ticks == tPButton5Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton5Color == tPButton5Color && cacheWhiskysTPSLAdjuster[idx].TPButton6Enabled == tPButton6Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton6Ticks == tPButton6Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton6Color == tPButton6Color && cacheWhiskysTPSLAdjuster[idx].TPButton7Enabled == tPButton7Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton7Ticks == tPButton7Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton7Color == tPButton7Color && cacheWhiskysTPSLAdjuster[idx].TPButton8Enabled == tPButton8Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton8Ticks == tPButton8Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton8Color == tPButton8Color && cacheWhiskysTPSLAdjuster[idx].TPButton9Enabled == tPButton9Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton9Ticks == tPButton9Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton9Color == tPButton9Color && cacheWhiskysTPSLAdjuster[idx].TPButton10Enabled == tPButton10Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton10Ticks == tPButton10Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton10Color == tPButton10Color && cacheWhiskysTPSLAdjuster[idx].TPButton11Enabled == tPButton11Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton11Ticks == tPButton11Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton11Color == tPButton11Color && cacheWhiskysTPSLAdjuster[idx].TPButton12Enabled == tPButton12Enabled && cacheWhiskysTPSLAdjuster[idx].TPButton12Ticks == tPButton12Ticks && cacheWhiskysTPSLAdjuster[idx].TPButton12Color == tPButton12Color && cacheWhiskysTPSLAdjuster[idx].ModifyAllMatchingOrders == modifyAllMatchingOrders && cacheWhiskysTPSLAdjuster[idx].PrintDebug == printDebug && cacheWhiskysTPSLAdjuster[idx].EqualsInput(input))
						return cacheWhiskysTPSLAdjuster[idx];
			return CacheIndicator<Whisky.WhiskysTPSLAdjuster>(new Whisky.WhiskysTPSLAdjuster(){ Label = label, PanelLeft = panelLeft, PanelTop = panelTop, PanelMaxWidth = panelMaxWidth, EnableSLButtons = enableSLButtons, SLMinDistanceTicks = sLMinDistanceTicks, SLButton1Enabled = sLButton1Enabled, SLButton1Ticks = sLButton1Ticks, SLButton1Color = sLButton1Color, SLButton2Enabled = sLButton2Enabled, SLButton2Ticks = sLButton2Ticks, SLButton2Color = sLButton2Color, SLButton3Enabled = sLButton3Enabled, SLButton3Ticks = sLButton3Ticks, SLButton3Color = sLButton3Color, SLButton4Enabled = sLButton4Enabled, SLButton4Ticks = sLButton4Ticks, SLButton4Color = sLButton4Color, SLButton5Enabled = sLButton5Enabled, SLButton5Ticks = sLButton5Ticks, SLButton5Color = sLButton5Color, SLButton6Enabled = sLButton6Enabled, SLButton6Ticks = sLButton6Ticks, SLButton6Color = sLButton6Color, SLButton7Enabled = sLButton7Enabled, SLButton7Ticks = sLButton7Ticks, SLButton7Color = sLButton7Color, SLButton8Enabled = sLButton8Enabled, SLButton8Ticks = sLButton8Ticks, SLButton8Color = sLButton8Color, SLButton9Enabled = sLButton9Enabled, SLButton9Ticks = sLButton9Ticks, SLButton9Color = sLButton9Color, SLButton10Enabled = sLButton10Enabled, SLButton10Ticks = sLButton10Ticks, SLButton10Color = sLButton10Color, SLButton11Enabled = sLButton11Enabled, SLButton11Ticks = sLButton11Ticks, SLButton11Color = sLButton11Color, SLButton12Enabled = sLButton12Enabled, SLButton12Ticks = sLButton12Ticks, SLButton12Color = sLButton12Color, EnableTPButtons = enableTPButtons, TPMinDistanceTicks = tPMinDistanceTicks, TPButton1Enabled = tPButton1Enabled, TPButton1Ticks = tPButton1Ticks, TPButton1Color = tPButton1Color, TPButton2Enabled = tPButton2Enabled, TPButton2Ticks = tPButton2Ticks, TPButton2Color = tPButton2Color, TPButton3Enabled = tPButton3Enabled, TPButton3Ticks = tPButton3Ticks, TPButton3Color = tPButton3Color, TPButton4Enabled = tPButton4Enabled, TPButton4Ticks = tPButton4Ticks, TPButton4Color = tPButton4Color, TPButton5Enabled = tPButton5Enabled, TPButton5Ticks = tPButton5Ticks, TPButton5Color = tPButton5Color, TPButton6Enabled = tPButton6Enabled, TPButton6Ticks = tPButton6Ticks, TPButton6Color = tPButton6Color, TPButton7Enabled = tPButton7Enabled, TPButton7Ticks = tPButton7Ticks, TPButton7Color = tPButton7Color, TPButton8Enabled = tPButton8Enabled, TPButton8Ticks = tPButton8Ticks, TPButton8Color = tPButton8Color, TPButton9Enabled = tPButton9Enabled, TPButton9Ticks = tPButton9Ticks, TPButton9Color = tPButton9Color, TPButton10Enabled = tPButton10Enabled, TPButton10Ticks = tPButton10Ticks, TPButton10Color = tPButton10Color, TPButton11Enabled = tPButton11Enabled, TPButton11Ticks = tPButton11Ticks, TPButton11Color = tPButton11Color, TPButton12Enabled = tPButton12Enabled, TPButton12Ticks = tPButton12Ticks, TPButton12Color = tPButton12Color, ModifyAllMatchingOrders = modifyAllMatchingOrders, PrintDebug = printDebug }, input, ref cacheWhiskysTPSLAdjuster);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Whisky.WhiskysTPSLAdjuster WhiskysTPSLAdjuster(string label, double panelLeft, double panelTop, double panelMaxWidth, bool enableSLButtons, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool sLButton7Enabled, int sLButton7Ticks, Brush sLButton7Color, bool sLButton8Enabled, int sLButton8Ticks, Brush sLButton8Color, bool sLButton9Enabled, int sLButton9Ticks, Brush sLButton9Color, bool sLButton10Enabled, int sLButton10Ticks, Brush sLButton10Color, bool sLButton11Enabled, int sLButton11Ticks, Brush sLButton11Color, bool sLButton12Enabled, int sLButton12Ticks, Brush sLButton12Color, bool enableTPButtons, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool tPButton7Enabled, int tPButton7Ticks, Brush tPButton7Color, bool tPButton8Enabled, int tPButton8Ticks, Brush tPButton8Color, bool tPButton9Enabled, int tPButton9Ticks, Brush tPButton9Color, bool tPButton10Enabled, int tPButton10Ticks, Brush tPButton10Color, bool tPButton11Enabled, int tPButton11Ticks, Brush tPButton11Color, bool tPButton12Enabled, int tPButton12Ticks, Brush tPButton12Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.WhiskysTPSLAdjuster(Input, label, panelLeft, panelTop, panelMaxWidth, enableSLButtons, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, sLButton7Enabled, sLButton7Ticks, sLButton7Color, sLButton8Enabled, sLButton8Ticks, sLButton8Color, sLButton9Enabled, sLButton9Ticks, sLButton9Color, sLButton10Enabled, sLButton10Ticks, sLButton10Color, sLButton11Enabled, sLButton11Ticks, sLButton11Color, sLButton12Enabled, sLButton12Ticks, sLButton12Color, enableTPButtons, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, tPButton7Enabled, tPButton7Ticks, tPButton7Color, tPButton8Enabled, tPButton8Ticks, tPButton8Color, tPButton9Enabled, tPButton9Ticks, tPButton9Color, tPButton10Enabled, tPButton10Ticks, tPButton10Color, tPButton11Enabled, tPButton11Ticks, tPButton11Color, tPButton12Enabled, tPButton12Ticks, tPButton12Color, modifyAllMatchingOrders, printDebug);
		}

		public Indicators.Whisky.WhiskysTPSLAdjuster WhiskysTPSLAdjuster(ISeries<double> input , string label, double panelLeft, double panelTop, double panelMaxWidth, bool enableSLButtons, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool sLButton7Enabled, int sLButton7Ticks, Brush sLButton7Color, bool sLButton8Enabled, int sLButton8Ticks, Brush sLButton8Color, bool sLButton9Enabled, int sLButton9Ticks, Brush sLButton9Color, bool sLButton10Enabled, int sLButton10Ticks, Brush sLButton10Color, bool sLButton11Enabled, int sLButton11Ticks, Brush sLButton11Color, bool sLButton12Enabled, int sLButton12Ticks, Brush sLButton12Color, bool enableTPButtons, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool tPButton7Enabled, int tPButton7Ticks, Brush tPButton7Color, bool tPButton8Enabled, int tPButton8Ticks, Brush tPButton8Color, bool tPButton9Enabled, int tPButton9Ticks, Brush tPButton9Color, bool tPButton10Enabled, int tPButton10Ticks, Brush tPButton10Color, bool tPButton11Enabled, int tPButton11Ticks, Brush tPButton11Color, bool tPButton12Enabled, int tPButton12Ticks, Brush tPButton12Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.WhiskysTPSLAdjuster(input, label, panelLeft, panelTop, panelMaxWidth, enableSLButtons, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, sLButton7Enabled, sLButton7Ticks, sLButton7Color, sLButton8Enabled, sLButton8Ticks, sLButton8Color, sLButton9Enabled, sLButton9Ticks, sLButton9Color, sLButton10Enabled, sLButton10Ticks, sLButton10Color, sLButton11Enabled, sLButton11Ticks, sLButton11Color, sLButton12Enabled, sLButton12Ticks, sLButton12Color, enableTPButtons, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, tPButton7Enabled, tPButton7Ticks, tPButton7Color, tPButton8Enabled, tPButton8Ticks, tPButton8Color, tPButton9Enabled, tPButton9Ticks, tPButton9Color, tPButton10Enabled, tPButton10Ticks, tPButton10Color, tPButton11Enabled, tPButton11Ticks, tPButton11Color, tPButton12Enabled, tPButton12Ticks, tPButton12Color, modifyAllMatchingOrders, printDebug);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Whisky.WhiskysTPSLAdjuster WhiskysTPSLAdjuster(string label, double panelLeft, double panelTop, double panelMaxWidth, bool enableSLButtons, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool sLButton7Enabled, int sLButton7Ticks, Brush sLButton7Color, bool sLButton8Enabled, int sLButton8Ticks, Brush sLButton8Color, bool sLButton9Enabled, int sLButton9Ticks, Brush sLButton9Color, bool sLButton10Enabled, int sLButton10Ticks, Brush sLButton10Color, bool sLButton11Enabled, int sLButton11Ticks, Brush sLButton11Color, bool sLButton12Enabled, int sLButton12Ticks, Brush sLButton12Color, bool enableTPButtons, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool tPButton7Enabled, int tPButton7Ticks, Brush tPButton7Color, bool tPButton8Enabled, int tPButton8Ticks, Brush tPButton8Color, bool tPButton9Enabled, int tPButton9Ticks, Brush tPButton9Color, bool tPButton10Enabled, int tPButton10Ticks, Brush tPButton10Color, bool tPButton11Enabled, int tPButton11Ticks, Brush tPButton11Color, bool tPButton12Enabled, int tPButton12Ticks, Brush tPButton12Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.WhiskysTPSLAdjuster(Input, label, panelLeft, panelTop, panelMaxWidth, enableSLButtons, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, sLButton7Enabled, sLButton7Ticks, sLButton7Color, sLButton8Enabled, sLButton8Ticks, sLButton8Color, sLButton9Enabled, sLButton9Ticks, sLButton9Color, sLButton10Enabled, sLButton10Ticks, sLButton10Color, sLButton11Enabled, sLButton11Ticks, sLButton11Color, sLButton12Enabled, sLButton12Ticks, sLButton12Color, enableTPButtons, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, tPButton7Enabled, tPButton7Ticks, tPButton7Color, tPButton8Enabled, tPButton8Ticks, tPButton8Color, tPButton9Enabled, tPButton9Ticks, tPButton9Color, tPButton10Enabled, tPButton10Ticks, tPButton10Color, tPButton11Enabled, tPButton11Ticks, tPButton11Color, tPButton12Enabled, tPButton12Ticks, tPButton12Color, modifyAllMatchingOrders, printDebug);
		}

		public Indicators.Whisky.WhiskysTPSLAdjuster WhiskysTPSLAdjuster(ISeries<double> input , string label, double panelLeft, double panelTop, double panelMaxWidth, bool enableSLButtons, int sLMinDistanceTicks, bool sLButton1Enabled, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enabled, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enabled, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enabled, int sLButton4Ticks, Brush sLButton4Color, bool sLButton5Enabled, int sLButton5Ticks, Brush sLButton5Color, bool sLButton6Enabled, int sLButton6Ticks, Brush sLButton6Color, bool sLButton7Enabled, int sLButton7Ticks, Brush sLButton7Color, bool sLButton8Enabled, int sLButton8Ticks, Brush sLButton8Color, bool sLButton9Enabled, int sLButton9Ticks, Brush sLButton9Color, bool sLButton10Enabled, int sLButton10Ticks, Brush sLButton10Color, bool sLButton11Enabled, int sLButton11Ticks, Brush sLButton11Color, bool sLButton12Enabled, int sLButton12Ticks, Brush sLButton12Color, bool enableTPButtons, int tPMinDistanceTicks, bool tPButton1Enabled, int tPButton1Ticks, Brush tPButton1Color, bool tPButton2Enabled, int tPButton2Ticks, Brush tPButton2Color, bool tPButton3Enabled, int tPButton3Ticks, Brush tPButton3Color, bool tPButton4Enabled, int tPButton4Ticks, Brush tPButton4Color, bool tPButton5Enabled, int tPButton5Ticks, Brush tPButton5Color, bool tPButton6Enabled, int tPButton6Ticks, Brush tPButton6Color, bool tPButton7Enabled, int tPButton7Ticks, Brush tPButton7Color, bool tPButton8Enabled, int tPButton8Ticks, Brush tPButton8Color, bool tPButton9Enabled, int tPButton9Ticks, Brush tPButton9Color, bool tPButton10Enabled, int tPButton10Ticks, Brush tPButton10Color, bool tPButton11Enabled, int tPButton11Ticks, Brush tPButton11Color, bool tPButton12Enabled, int tPButton12Ticks, Brush tPButton12Color, bool modifyAllMatchingOrders, bool printDebug)
		{
			return indicator.WhiskysTPSLAdjuster(input, label, panelLeft, panelTop, panelMaxWidth, enableSLButtons, sLMinDistanceTicks, sLButton1Enabled, sLButton1Ticks, sLButton1Color, sLButton2Enabled, sLButton2Ticks, sLButton2Color, sLButton3Enabled, sLButton3Ticks, sLButton3Color, sLButton4Enabled, sLButton4Ticks, sLButton4Color, sLButton5Enabled, sLButton5Ticks, sLButton5Color, sLButton6Enabled, sLButton6Ticks, sLButton6Color, sLButton7Enabled, sLButton7Ticks, sLButton7Color, sLButton8Enabled, sLButton8Ticks, sLButton8Color, sLButton9Enabled, sLButton9Ticks, sLButton9Color, sLButton10Enabled, sLButton10Ticks, sLButton10Color, sLButton11Enabled, sLButton11Ticks, sLButton11Color, sLButton12Enabled, sLButton12Ticks, sLButton12Color, enableTPButtons, tPMinDistanceTicks, tPButton1Enabled, tPButton1Ticks, tPButton1Color, tPButton2Enabled, tPButton2Ticks, tPButton2Color, tPButton3Enabled, tPButton3Ticks, tPButton3Color, tPButton4Enabled, tPButton4Ticks, tPButton4Color, tPButton5Enabled, tPButton5Ticks, tPButton5Color, tPButton6Enabled, tPButton6Ticks, tPButton6Color, tPButton7Enabled, tPButton7Ticks, tPButton7Color, tPButton8Enabled, tPButton8Ticks, tPButton8Color, tPButton9Enabled, tPButton9Ticks, tPButton9Color, tPButton10Enabled, tPButton10Ticks, tPButton10Color, tPButton11Enabled, tPButton11Ticks, tPButton11Color, tPButton12Enabled, tPButton12Ticks, tPButton12Color, modifyAllMatchingOrders, printDebug);
		}
	}
}

#endregion
