#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.KCStrategies
{
    public class SuperBot : KCAlgoBase
    {
        // Parameters
		
		public NinjaTrader.NinjaScript.Indicators.BlueZ.BlueZHMAHooks HMAHooks1;
		public bool hmaHooksUp;
		public bool hmaHooksDown;
		
		private LinReg2 LinReg1;
		private bool linRegUp;
		private bool linRegDown;
		
		private TrendMagic TrendMagic1;
		private int cciPeriod;
		private int atrPeriod;
		private bool trendMagicUp;
		private bool trendMagicDown;
		
       	private NinjaTrader.NinjaScript.Indicators.RegressionChannel RegressionChannel1, RegressionChannel2;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		private bool regChanUp;
		private bool regChanDown;
		
		private NinjaTrader.NinjaScript.Indicators.TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD WilliamsR1;
		private bool WillyUp;
		private bool WillyDown;
		
		public Momentum Momentum1;
		public bool momoUp;
		public bool momoDown;
		
		private CMO CMO1;
		private bool cmoUp;
		private bool cmoDown;
		
        private T3TrendFilter T3TrendFilter1;
        private double TrendyUp;
        private double TrendyDown;
		private bool trendyUp;
		private bool trendyDown;
		
		private bool longSignal = false;
        private bool shortSignal = false;

		public override string DisplayName { get { return Name; } }
		
        protected override void OnStateChange()
        {
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = "SuperBot is a combination of many strategies and indicators, including HMA Hooks, Linear Regression curve, Linear Regression Channel, TrendMagic, T3TrendFilter, CMO, Momentum, Market Structure, and William R.";
                Name = "SuperBot v4.3";
                StrategyName = "SuperBot";
                Version = "4.3 Jan. 2025";
                Credits = "Strategy by Khanh Nguyen";
                ChartType = "Orenko 34-40-40";	
				
				HmaPeriod			= 8;
				enableHmaHooks		= true;
				showHmaHooks		= true;
				
				LinRegPeriod		= 9;
				enableLinReg		= false;
				showLinReg			= false;
				
				RegChanPeriod		= 40;
				RegChanWidth		= 4;
				RegChanWidth2		= 3;
				enableRegChan1		= true;
				enableRegChan2		= true;
				showRegChan1		= true;
				showRegChan2		= true;
				showRegChanHiLo		= true;
				
				cciPeriod			= 20;
				atrPeriod			= 14;
				atrMult				= 0.1;
				enableTrendMagic	= false;
				showTrendMagic		= false;
				
				wrUp 				= -20;
				wrDown				= -80;
				wrPeriod			= 14;
				enableWilly			= true;
				showWilly			= false;
				
				MomoUp				= 5;
				MomoDown			= -5;
				enableMomo			= true;
				showMomo			= true;
				
				CmoUp				= 10;
				CmoDown				= -10;
				enableSuperRex		= true;
				showCMO				= false;
				
                // T3 Trend Filter settings
                Factor 				= 0.5;
                Period1 			= 1;
                Period2 			= 1;
                Period3 			= 1;
                Period4 			= 1;
                Period5 			= 9;
				enableTrendy		= false;
				showTrendy			= false;
				
                InitialStop			= 92;
				ProfitTarget		= 48;
				
                activeOrder 	= false;
            }
            else if (State == State.DataLoaded)
            {
                InitializeIndicators();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade)
                return;
			
            bool channelSlopeUp = (RegressionChannel1.Middle[1] > RegressionChannel1.Middle[2]) && (RegressionChannel1.Middle[2] <= RegressionChannel1.Middle[3]) 
				|| (RegressionChannel1.Middle[0] > RegressionChannel1.Middle[1] && Low[0] > Low[2] && Low[2] <= RegressionChannel1.Lower[2]);
    		bool priceNearLowerChannel = (Low[0] > RegressionChannelHighLow1.Lower[2]);

			bool channelSlopeDown = (RegressionChannel1.Middle[1] < RegressionChannel1.Middle[2]) && (RegressionChannel1.Middle[2] >= RegressionChannel1.Middle[3])
				|| (RegressionChannel1.Middle[0] < RegressionChannel1.Middle[1] && High[0] < High[2] && High[2] >= RegressionChannel1.Upper[2]);
    		bool priceNearUpperChannel = (High[0] < RegressionChannelHighLow1.Upper[2]);

            regChanUp = enableRegChan1 ? channelSlopeUp || priceNearLowerChannel : true;
            regChanDown = enableRegChan1 ? channelSlopeDown || priceNearUpperChannel : true;
			
			hmaHooksUp = enableHmaHooks ? (Close[0] > HMAHooks1[0] && HMAHooks1.trend[0] == 1 && HMAHooks1.trend[1] == -1) 
				|| (HMAHooks1[0] > HMAHooks1[2]) : true;
			
			hmaHooksDown = enableHmaHooks ? (Close[0] < HMAHooks1[0] && HMAHooks1.trend[0] == -1 && HMAHooks1.trend[1] == 1)  
				|| (HMAHooks1[0] < HMAHooks1[2]) : true;
			
			linRegUp = enableLinReg ? LinReg1[1] > LinReg1[2] && LinReg1[2] > LinReg1[3] : true;
			linRegDown = enableLinReg ? LinReg1[1] < LinReg1[2] && LinReg1[2] < LinReg1[3] : true;
			
			WillyUp = enableWilly ? WilliamsR1[1] >= wrUp && Close[0] > Close[1] && High[1] > High[2] : true;
            WillyDown = enableWilly ? WilliamsR1[1] <= wrDown && Close[0] < Close[1] && Low[1] < Low[2] : true;
			
			momoUp = enableMomo ? Momentum1[0] > MomoUp && Momentum1[0] > Momentum1[1] : true;;
			momoDown = enableMomo ? Momentum1[0] < MomoDown && Momentum1[0] < Momentum1[1] : true;
			
			cmoUp = enableSuperRex ? CMO1[0] >= CmoUp : true;
            cmoDown = enableSuperRex ? CMO1[0] <= CmoDown : true;
			
			trendMagicUp = TrendMagic1.Trend[1] > TrendMagic1.Trend[2];
            trendMagicDown = TrendMagic1.Trend[1] < TrendMagic1.Trend[2];	
			
            TrendyUp = T3TrendFilter1.Values[0][0];
            TrendyDown = T3TrendFilter1.Values[1][0];

			trendyUp = enableTrendy ? TrendyUp >= 5 && TrendyDown == 0 : true;
            trendyUp = enableTrendy ? TrendyDown <= -5 && TrendyUp == 0 : true;	
			
			uptrend = aboveEMAHigh && RegressionChannel1.Middle[0] > RegressionChannel1.Middle[1] && Close[0] > Open[0];
			downtrend = belowEMALow && RegressionChannel1.Middle[0] < RegressionChannel1.Middle[1] && Close[0] < Open[0];
			
			longSignal = (hmaHooksUp || regChanUp || WillyUp || linRegUp || trendMagicUp || momoUp || cmoUp || trendyUp);
            shortSignal = (hmaHooksDown || regChanDown || WillyDown || linRegDown || trendMagicDown || momoDown || cmoDown || trendyDown); 
			
			exitLong = isLong && (Close[0] < HMAHooks1[0] && HMAHooks1.trend[0] == -1 && HMAHooks1.trend[1] == 1);
			exitShort = isShort && (Close[0] > HMAHooks1[0] && HMAHooks1.trend[0] == 1 && HMAHooks1.trend[1] == -1);
			base.OnBarUpdate();
        }

        protected override bool ValidateEntryLong()
        {
            // Logic for validating long entries
			if (longSignal && uptrend) return true;
			else return false;
        }

        protected override bool ValidateEntryShort()
        {
            // Logic for validating short entries
			if (shortSignal && downtrend) return true;
            else return false;
        }

       	protected override bool ValidateExitLong()
        {
            // Logic for validating long exits
            return enableExit? true : false;
        }

        protected override bool ValidateExitShort()
        {
			// Logic for validating short exits
			return enableExit? true : false;
        }

        #region Indicators
        protected override void InitializeIndicators()
    {
			HMAHooks1	= BlueZHMAHooks(Close, HmaPeriod, 0, false, false, true, Brushes.Lime, Brushes.Red);
			HMAHooks1.Plots[0].Brush = Brushes.White;
			HMAHooks1.Plots[0].Width = 2;
			if (showHmaHooks) AddChartIndicator(HMAHooks1);
				
            RegressionChannel1			= RegressionChannel(Close, RegChanPeriod, RegChanWidth);
			if (showRegChan1) AddChartIndicator(RegressionChannel1);
			
            RegressionChannel2			= RegressionChannel(Close, RegChanPeriod, RegChanWidth2);
			if (showRegChan2) AddChartIndicator(RegressionChannel2);
			
			RegressionChannelHighLow1	= RegressionChannelHighLow(Close, RegChanPeriod, RegChanWidth);	
			if (showRegChanHiLo) AddChartIndicator(RegressionChannelHighLow1);
						
			TrendMagic1		 	= TrendMagic(cciPeriod, atrPeriod, atrMult, false);
            if (showTrendMagic) AddChartIndicator(TrendMagic1);
			
			WilliamsR1    = TOWilliamsTraderOracleSignalMOD(Close, 14, @"LongEntry", @"ShortEntry");
			WilliamsR1.Plots[0].Brush = Brushes.Yellow;
			WilliamsR1.Plots[0].Width = 1;
			if (showWilly) AddChartIndicator(WilliamsR1);
			
            LinReg1 	= LinReg2(Close, LinRegPeriod);
			LinReg1.Plots[0].Width = 2;
			if (showLinReg) AddChartIndicator(LinReg1);
			
			Momentum1			= Momentum(Close, 14);	
			Momentum1.Plots[0].Brush = Brushes.Yellow;
			Momentum1.Plots[0].Width = 2;
			if (showMomo) AddChartIndicator(Momentum1);
				
            CMO1				= CMO(Close, 14);
			CMO1.Plots[0].Brush = Brushes.Yellow;
			CMO1.Plots[0].Width = 2;
			if (showCMO) AddChartIndicator(CMO1);
				  
			T3TrendFilter1 = T3TrendFilter(Close, Factor, Period1, Period2, Period3, Period4, Period5, false);
			if (showTrendy) AddChartIndicator(T3TrendFilter1);
        }
        #endregion

		#region 08. Strategy Settings
		
		[NinjaScriptProperty]
        [Display(Name = "Enable Hooker", Order = 1, GroupName = "08. Strategy Settings")]
        public bool enableHmaHooks { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="HMA Period", Order = 2, GroupName="08. Strategy Settings")]
		public int HmaPeriod
		{ get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show HMA Hooks", Order = 3, GroupName = "08. Strategy Settings")]
        public bool showHmaHooks { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable KingKhanh", Order = 4, GroupName = "08. Strategy Settings")]
        public bool enableRegChan1 { get; set; }
        
		[NinjaScriptProperty]
        [Display(Name = "Enable Inner Regression Channel", Order = 5, GroupName = "08. Strategy Settings")]
        public bool enableRegChan2 { get; set; }
        
		[NinjaScriptProperty]
		[Display(Name="Regression Channel Period", Order = 6, GroupName="08. Strategy Settings")]
		public int RegChanPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Outer Regression Channel Width", Order = 7, GroupName="08. Strategy Settings")]
		public double RegChanWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Inner Regression Channel Width", Order = 8, GroupName="08. Strategy Settings")]
		public double RegChanWidth2
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Outer Regression Channel", Order = 9, GroupName = "08. Strategy Settings")]
        public bool showRegChan1 { get; set; }
        
		[NinjaScriptProperty]
        [Display(Name = "Show Inner Regression Channel", Order = 10, GroupName = "08. Strategy Settings")]
        public bool showRegChan2 { get; set; }
        
		[NinjaScriptProperty]
        [Display(Name = "Show High and Low Lines", Order = 11, GroupName = "08. Strategy Settings")]
        public bool showRegChanHiLo { get; set; }
        
		[NinjaScriptProperty]
        [Display(Name = "Enable Willy", Order = 12, GroupName = "08. Strategy Settings")]
        public bool enableWilly { get; set; }
        
		[NinjaScriptProperty]
		[Display(Name="Willy Period", Order = 13, GroupName="08. Strategy Settings")]
		public int wrPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Willy Up", Order = 14, GroupName="08. Strategy Settings")]
		public int wrUp
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Willy Down", Order = 15, GroupName="08. Strategy Settings")]
		public int wrDown
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Willy", Order = 16, GroupName = "08. Strategy Settings")]
        public bool showWilly { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable Momo", Order = 17, GroupName = "08. Strategy Settings")]
        public bool enableMomo { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Momo Up", Order = 18, GroupName="08. Strategy Settings")]
		public int MomoUp
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Momo Down", Order = 19, GroupName="08. Strategy Settings")]
		public int MomoDown
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Momentum", Order = 20, GroupName = "08. Strategy Settings")]
        public bool showMomo { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable Chaser", Order = 21, GroupName = "08. Strategy Settings")]
        public bool enableLinReg { get; set; }
        
		[NinjaScriptProperty]
		[Display(Name="Linear Regression Period", Order = 22, GroupName="08. Strategy Settings")]
		public int LinRegPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Linear Regression Curve", Order = 23, GroupName = "08. Strategy Settings")]
        public bool showLinReg { get; set; }
        
		[NinjaScriptProperty]
        [Display(Name = "Enable MagicTrendy (TrendMagic)", Order = 24, GroupName = "08. Strategy Settings")]
        public bool enableTrendMagic { get; set; }
		
        [NinjaScriptProperty]
		[Display(Name="TrendMagic ATR Multiplier", Order = 25, GroupName="08. Strategy Settings")]
		public double atrMult
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show MagicTrendy", Order = 26, GroupName = "08. Strategy Settings")]
        public bool showTrendMagic { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable SuperRex", Order = 27, GroupName = "08. Strategy Settings")]
        public bool enableSuperRex { get; set; }
        
		[NinjaScriptProperty]
		[Display(Name="CMO Up", Order = 28, GroupName="08. Strategy Settings")]
		public int CmoUp
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="CMO Down", Order = 29, GroupName="08. Strategy Settings")]
		public int CmoDown
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show CMO", Order = 30, GroupName = "08. Strategy Settings")]
        public bool showCMO { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable Trendy", Order = 31, GroupName = "08. Strategy Settings")]
        public bool enableTrendy { get; set; }
        
        [NinjaScriptProperty]
        [Display(Name = "Factor", Order = 32, GroupName = "08. Strategy Settings")]
        public double Factor { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period 1", Order = 33, GroupName = "08. Strategy Settings")]
        public int Period1 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period 2", Order = 34, GroupName = "08. Strategy Settings")]
        public int Period2 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period 3", Order = 35, GroupName = "08. Strategy Settings")]
        public int Period3 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period 4", Order = 36, GroupName = "08. Strategy Settings")]
        public int Period4 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period 5", Order = 37, GroupName = "08. Strategy Settings")]
        public int Period5 { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show T3 Trend Filter", Order = 38, GroupName = "08. Strategy Settings")]
        public bool showTrendy { get; set; }

		#endregion
    }
}