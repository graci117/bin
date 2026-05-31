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
    public class SuperBot2 : KCAlgoBase
    {
        // Parameters
       	private NinjaTrader.NinjaScript.Indicators.RegressionChannel RegressionChannel1, RegressionChannel2;
		private RegressionChannelHighLow RegressionChannelHighLow1;	
		private bool regChanUp;
		private bool regChanDown;
		
		private NinjaTrader.NinjaScript.Indicators.BlueZ.BlueZHMAHooks HMAHooks1;
		public bool hmaHooksUp;
		public bool hmaHooksDown;
		
		public Momentum Momentum1;
		public bool momoUp;
		public bool momoDown;
		
		private NinjaTrader.NinjaScript.Indicators.TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD WilliamsR1;
		private bool WillyUp;
		private bool WillyDown;
		
		private bool longSignal = false;
        private bool shortSignal = false;

		public override string DisplayName { get { return Name; } }
		
        protected override void OnStateChange()
        {
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = "Strategy based on the Linear Regression Channel and HMAHooks indicators.";
                Name = "SuperBot2 v4.3";
                StrategyName = "SuperBot2";
                Version = "4.3 Feb. 2025";
                Credits = "Strategy by Khanh Nguyen";
                ChartType = "Orenko 34-40-40";	
				
				HmaPeriod			= 8;
				enableHmaHooks		= true;
				showHmaHooks		= true;
				
				RegChanPeriod		= 40;
				RegChanWidth		= 4;
				RegChanWidth2		= 3;
				enableRegChan1		= true;
				enableRegChan2		= true;
				showRegChan1		= true;
				showRegChan2		= true;
				showRegChanHiLo		= true;
				
				MomoUp				= 5;
				MomoDown			= -5;
				enableMomo			= true;
				showMomo			= true;
				
				wrUp 				= -20;
				wrDown				= -80;
				wrPeriod			= 14;
				enableWilly			= true;
				showWilly			= false;
				
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
			
			momoUp = enableMomo ? Momentum1[0] > MomoUp && Momentum1[0] > Momentum1[1] : true;;
			momoDown = enableMomo ? Momentum1[0] < MomoDown && Momentum1[0] < Momentum1[1] : true;
			
			WillyUp = enableWilly ? WilliamsR1[1] >= wrUp && Close[0] > Close[1] && High[1] > High[2] : true;
            WillyDown = enableWilly ? WilliamsR1[1] <= wrDown && Close[0] < Close[1] && Low[1] < Low[2] : true;
			
			uptrend = aboveEMAHigh && RegressionChannel1.Middle[0] > RegressionChannel1.Middle[1] && Close[0] > Open[0];
			downtrend = belowEMALow && RegressionChannel1.Middle[0] < RegressionChannel1.Middle[1] && Close[0] < Open[0];
			
			longSignal = hmaHooksUp || regChanUp || WillyUp || momoUp;
            shortSignal = hmaHooksDown || regChanDown || WillyDown || momoDown; 
			
            base.OnBarUpdate();
        }

        protected override bool ValidateEntryLong()
        {
            // Logic for validating long entries
			if (longSignal) return true;
			else return false;
        }

        protected override bool ValidateEntryShort()
        {
            // Logic for validating short entries
			if (shortSignal) return true;
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
            RegressionChannel1			= RegressionChannel(Close, RegChanPeriod, RegChanWidth);
			if (showRegChan1) AddChartIndicator(RegressionChannel1);
			
            RegressionChannel2			= RegressionChannel(Close, RegChanPeriod, RegChanWidth2);
			if (showRegChan2) AddChartIndicator(RegressionChannel2);
			
			RegressionChannelHighLow1	= RegressionChannelHighLow(Close, RegChanPeriod, RegChanWidth);	
			if (showRegChanHiLo) AddChartIndicator(RegressionChannelHighLow1);
				
			HMAHooks1				= BlueZHMAHooks(Close, HmaPeriod, 0, false, false, true, Brushes.Lime, Brushes.Red);
			HMAHooks1.Plots[0].Brush = Brushes.White;
			HMAHooks1.Plots[0].Width = 2;
			if (showHmaHooks) AddChartIndicator(HMAHooks1);
			
			Momentum1			= Momentum(Close, 14);	
			Momentum1.Plots[0].Brush = Brushes.Yellow;
			Momentum1.Plots[0].Width = 2;
			if (showMomo) AddChartIndicator(Momentum1);
				
			WilliamsR1    = TOWilliamsTraderOracleSignalMOD(Close, 14, @"LongEntry", @"ShortEntry");
			WilliamsR1.Plots[0].Brush = Brushes.Yellow;
			WilliamsR1.Plots[0].Width = 1;
			if (showWilly) AddChartIndicator(WilliamsR1);			
        }
        #endregion

        #region Properties
		
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
        [Display(Name = "Show High Low", Order = 11, GroupName = "08. Strategy Settings")]
        public bool showRegChanHiLo { get; set; }        
        
		[NinjaScriptProperty]
        [Display(Name = "Enable Momo", Order = 12, GroupName = "08. Strategy Settings")]
        public bool enableMomo { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Momo Up", Order = 13, GroupName="08. Strategy Settings")]
		public int MomoUp
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Momo Down", Order = 14, GroupName="08. Strategy Settings")]
		public int MomoDown
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Momentum", Order = 15, GroupName = "08. Strategy Settings")]
        public bool showMomo { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable Willy", Order = 16, GroupName = "08. Strategy Settings")]
        public bool enableWilly { get; set; }
        
		[NinjaScriptProperty]
		[Display(Name="Willy Period", Order = 17, GroupName="08. Strategy Settings")]
		public int wrPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Willy Up", Order = 18, GroupName="08. Strategy Settings")]
		public int wrUp
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Willy Down", Order = 18, GroupName="08. Strategy Settings")]
		public int wrDown
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Willy", Order = 20, GroupName = "08. Strategy Settings")]
        public bool showWilly { get; set; }
		
        #endregion
    }
}
