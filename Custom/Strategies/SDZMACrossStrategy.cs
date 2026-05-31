#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Xml.Serialization;

using NinjaTrader.Gui.Tools;



#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class SDZMACrossStrategy : Strategy
    {
        // ── Strategy's own confirmed signal state ─────────────────────────────
        [Browsable(false)]
[XmlIgnore]
public int SignalState { get; private set; }


        private SupplyDemandZones _sdz;
        private MACrossBuilder    _maCross;
        private MACD              _macd;
        private int               _lastSignalBar = -1;
		

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                  = "SDZ + MACD + MA Cross Strategy with ATM";
                Name                         = "SDZ_MACross_Strategy";
                Calculate                    = Calculate.OnBarClose;
                EntriesPerDirection          = 1;
                EntryHandling                = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds    = 30;

                // SDZ params — should match what you set on the indicator
                SDZ_ConsecutiveBars        = 3;
                SDZ_BreakTicks             = 10;
                SDZ_ReversalBars           = 1;
                SDZ_OppositeZoneClearTicks = 1;

                // MA Cross
                MAC_FastPeriod             = 5;
                MAC_SlowPeriod             = 14;

                // MACD
                MACD_Fast                  = 12;
                MACD_Slow                  = 26;
                MACD_Signal                = 9;

                // ATM
                AtmStrategyName            = "MyAtmStrategy";

                // Visuals — different colors from SDZ so you can tell them apart
                BuyArrowColor              = Brushes.Lime;
                SellArrowColor             = Brushes.Red;
                ArrowOffsetTicks           = 6;   // offset more so arrows don't overlap
				SDZ_SwingStrength = 3;
            }
            else if (State == State.Configure)
            {
                BarsRequiredToPlot = 30;
            }
          else if (State == State.DataLoaded)
{
    SignalState = 0;

  _sdz = SupplyDemandZones(
    SDZ_ConsecutiveBars,
    SDZ_BreakTicks,
    SDZ_ReversalBars,
    SDZ_OppositeZoneClearTicks,
    true,
    Brushes.LimeGreen,
    Brushes.Red,
    Brushes.Cyan,
    Brushes.Magenta,
    40,
    SDZ_SwingStrength);

    _macd = MACD(Close, MACD_Fast, MACD_Slow, MACD_Signal);

    _maCross = MACrossBuilder(
        CDMAtype.EMA, NinjaTrader.Data.PriceType.Close, MAC_FastPeriod,
        CDMAtype.EMA, NinjaTrader.Data.PriceType.Close, MAC_SlowPeriod,
        1, true, true);
}

        }

        protected override void OnBarUpdate()
        {
			if (_sdz == null || _macd == null || _maCross == null)
    			return;
            if (CurrentBar < 30)
                return;
			
			if (_sdz != null)
				Print (" yay   " + _sdz.SignalState);

            // Start from SDZ signal
            SignalState = _sdz.SignalState;

            // Layer on MACD + MA Cross confirmation
            ApplyConfirmation();

            // Act on confirmed signal
            if (CurrentBar != _lastSignalBar)
                ProcessEntry();
        }

        private void ApplyConfirmation()
        {
            if (SignalState == 0)
                return;

            double macdHist = _macd.Diff[0];
            double maCross  = _maCross.CrossDetect[0];

            bool confirmedLong  = SignalState == 1  && macdHist > 0 && maCross == 1;
            bool confirmedShort = SignalState == -1 && macdHist < 0 && maCross == -1;

            if (!confirmedLong && !confirmedShort)
                SignalState = 0;
        }

        private void ProcessEntry()
        {
            if (SignalState == 0)
                return;

            if (SignalState == 1 && Position.MarketPosition != MarketPosition.Long)
            {
                _lastSignalBar = CurrentBar;

                // Confirmed arrow — different color/offset from SDZ arrow
                Draw.ArrowUp(this, "CONF_BUY_" + CurrentBar, false, 0,
                    Low[0] - ArrowOffsetTicks * TickSize, BuyArrowColor);

                string atmId   = GetAtmStrategyUniqueId();
                string orderId = GetAtmStrategyUniqueId();

                AtmStrategyCreate(OrderAction.Buy,
                    OrderType.Market, 0, 0,
                    TimeInForce.Day,
                    orderId,
                    AtmStrategyName,
                    atmId,
                    (err, id) =>
                    {
                        if (err == ErrorCode.NoError && id == atmId)
                            Print("ATM LONG confirmed @ " + Time[0]);
                    });
            }
            else if (SignalState == -1 && Position.MarketPosition != MarketPosition.Short)
            {
                _lastSignalBar = CurrentBar;

                Draw.ArrowDown(this, "CONF_SELL_" + CurrentBar, false, 0,
                    High[0] + ArrowOffsetTicks * TickSize, SellArrowColor);

                string atmId   = GetAtmStrategyUniqueId();
                string orderId = GetAtmStrategyUniqueId();

                AtmStrategyCreate(OrderAction.SellShort,
                    OrderType.Market, 0, 0,
                    TimeInForce.Day,
                    orderId,
                    AtmStrategyName,
                    atmId,
                    (err, id) =>
                    {
                        if (err == ErrorCode.NoError && id == atmId)
                            Print("ATM SHORT confirmed @ " + Time[0]);
                    });
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "ATM Strategy Name",
            Description = "Must exactly match ATM name saved in NinjaTrader",
            Order = 1, GroupName = "ATM Settings")]
        public string AtmStrategyName { get; set; }

        [NinjaScriptProperty]
        [Range(2, 20)]
        [Display(Name = "SDZ Consecutive Bars", Order = 1, GroupName = "Zone Settings")]
        public int SDZ_ConsecutiveBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, 500)]
        [Display(Name = "SDZ Break Ticks", Order = 2, GroupName = "Zone Settings")]
        public int SDZ_BreakTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, 20)]
        [Display(Name = "SDZ Reversal Bars", Order = 3, GroupName = "Zone Settings")]
        public int SDZ_ReversalBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, 500)]
        [Display(Name = "SDZ Opposite Zone Clear Ticks", Order = 4, GroupName = "Zone Settings")]
        public int SDZ_OppositeZoneClearTicks { get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, 20)]
		[Display(Name = "SDZ Swing Strength", Order = 5, GroupName = "Zone Settings")]
		public int SDZ_SwingStrength { get; set; }


        [NinjaScriptProperty]
        [Range(1, 200)]
        [Display(Name = "Fast MA Period", Order = 1, GroupName = "MA Cross")]
        public int MAC_FastPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, 200)]
        [Display(Name = "Slow MA Period", Order = 2, GroupName = "MA Cross")]
        public int MAC_SlowPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, 200)]
        [Display(Name = "MACD Fast Period", Order = 1, GroupName = "MACD")]
        public int MACD_Fast { get; set; }

        [NinjaScriptProperty]
        [Range(1, 200)]
        [Display(Name = "MACD Slow Period", Order = 2, GroupName = "MACD")]
        public int MACD_Slow { get; set; }
		
		public SupplyDemandZones SupplyDemandZones(int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength)
		{
			return SupplyDemandZones(Input, consecutiveBars, breakTicks, reversalBars, oppositeZoneClearTicks, drawZoneArrows, demandColor, supplyColor, buySignalColor, sellSignalColor, zoneOpacity, swingStrength);
		}

		public SupplyDemandZones SupplyDemandZones(ISeries<double> input , int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength)
		{
			return SupplyDemandZones(input, consecutiveBars, breakTicks, reversalBars, oppositeZoneClearTicks, drawZoneArrows, demandColor, supplyColor, buySignalColor, sellSignalColor, zoneOpacity, swingStrength);
		}

        [NinjaScriptProperty]
        [Range(1, 200)]
        [Display(Name = "MACD Signal Period", Order = 3, GroupName = "MACD")]
        public int MACD_Signal { get; set; }

		 [XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Buy Arrow Color", Order = 1, GroupName = "Visuals")]
		public Brush BuyArrowColor { get; set; }
		
		[Browsable(false)]
		public string BuyArrowColorSerializable
		{
//		    get { return Serialize.BrushToString(BuyArrowColor); }
		   // set { BuyArrowColor = Serialize.StringToBrush(value); }
			
			get { return NinjaTrader.Gui.Serialize.BrushToString(BuyArrowColor); }
set { BuyArrowColor = NinjaTrader.Gui.Serialize.StringToBrush(value); }

		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Sell Arrow Color", Order = 2, GroupName = "Visuals")]
		public Brush SellArrowColor { get; set; }
		
		[Browsable(false)]
		public string SellArrowColorSerializable
		{
		    get { return NinjaTrader.Gui.Serialize.BrushToString(SellArrowColor); }
		    set { SellArrowColor = NinjaTrader.Gui.Serialize.StringToBrush(value); }
		}


        [NinjaScriptProperty]
        [Range(1, 20)]
        [Display(Name = "Arrow Offset Ticks", Order = 3, GroupName = "Visuals")]
        public int ArrowOffsetTicks { get; set; }

        #endregion
    }
}
