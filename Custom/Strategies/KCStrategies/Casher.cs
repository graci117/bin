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
    public class Casher : KCAlgoBase
    {
        // Parameters
		private Momentum Momentum1;
		
		private HiLoBands HiLoBands1, HiLoBands2; 
        private Series<double> highestHigh;
        private Series<double> lowestLow;
		private Series<double> highestHigh2;
        private Series<double> lowestLow2;
		private Series<double> midline;
		
		private bool longSignal = false;
        private bool shortSignal = false;

		public override string DisplayName { get { return Name; } }
		
        protected override void OnStateChange()
        {
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = "Strategy based on HiLoBands indicator.";
                Name = "Casher v4.0";
                StrategyName = "Casher";
                Version = "4.0 Feb. 2025";
                Credits = "Strategy by Khanh Nguyen";
                ChartType = "TDU Renko Backtest 70-2, Range 30";
				
				FastPeriod		= 7;
				SlowPeriod		= 20;
				Width			= 2;			
				lookbackPeriod	= 10;
				showHighLow		= false;
				
                InitialStop		= 144;
				ProfitTarget	= 28;
				enableExit			= false;
				
                activeOrder 	= false;
            }
            else if (State == State.DataLoaded)
            {
				highestHigh = new Series<double> (this);
				lowestLow = new Series<double> (this);
				highestHigh2 = new Series<double> (this);
				lowestLow2 = new Series<double> (this);
				midline = new Series<double> (this);
				
                InitializeIndicators();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade)
                return;
            
			// Ensure there are enough bars to calculate the average range
		    if (CurrentBar < lookbackPeriod)
		        return;
		
		    // Calculate the average range of the last 10 bars
		    double sumRange = 0;
		
		    for (int i = 1; i <= lookbackPeriod; i++) // Start from 1 to exclude the current bar
		    {
		        sumRange += (High[i] - Low[i]);
		    }
		
		    double averageRange = sumRange / lookbackPeriod;
		
		    // Calculate the current range (current bar)
		    double currentRange = High[0] - Low[0];
	
			highestHigh[0] = HiLoBands1.Values[0][0];
			lowestLow[0] = HiLoBands1.Values[1][0];
			highestHigh2[0] = HiLoBands1.Values[2][0];
			lowestLow2[0] = HiLoBands1.Values[3][0];
			midline[0] = HiLoBands1.Values[4][0];
			
			longSignal = ((highestHigh[0] == highestHigh2[0] && highestHigh[1] != highestHigh2[1] && highestHigh[0] > highestHigh[1])
				|| (highestHigh2[0] > highestHigh2[1] && highestHigh2[1] == highestHigh2[2])
				|| (highestHigh[0] > highestHigh[1] && highestHigh[1] > highestHigh[2])
				|| (midline[0] > midline[1] && midline[1] < midline[2]))
//				|| (Low[0] > midline[0] && Low[1] < midline[1]))
//				|| (Low[0] > Low[1] && Low[1] == lowestLow[1]))
				&& (Close[0] > Close[1]);
//				&& (Close[0] > Open[0] && Momentum1[0] > Momentum1[1]);
			
            shortSignal = ((lowestLow[0] == lowestLow2[0] && lowestLow[1] != lowestLow2[1] && lowestLow[0] < lowestLow[1])
				|| (lowestLow2[0] < lowestLow2[1] && lowestLow2[1] == lowestLow2[2])
				|| (lowestLow[0] < lowestLow[1] && lowestLow[1] < lowestLow[2])
				|| (midline[0] < midline[1] && midline[1] > midline[2]))
//				|| (High[0] < midline[0] && High[1] > midline[1]))
//				|| (High[0] < High[1] && High[1] == highestHigh[1]))
				&& (Close[0] < Close[1]);	
//				&& (Close[0] < Open[0] && Momentum1[0] < Momentum1[1]);	
			
			exitLong = isLong && highestHigh[0] < highestHigh[1] && highestHigh[1] == highestHigh[2];
			exitShort = isShort && lowestLow[0] > lowestLow[1] && lowestLow[1] == lowestLow[2];
			
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
            return enableExit? true: false;
        }

        protected override bool ValidateExitShort()
        {
			// Logic for validating short exits
			return enableExit? true: false;
        }

        #region Indicators
        protected override void InitializeIndicators()
        {
			HiLoBands1				= HiLoBands(FastPeriod, SlowPeriod, Width);
			HiLoBands1.Plots[0].Brush = Brushes.Cyan;
			HiLoBands1.Plots[1].Brush = Brushes.Magenta;
			if (showHighLow) AddChartIndicator(HiLoBands1);
			
//			HiLoBands2				= HiLoBands(FastPeriod, SlowPeriod, Width);
//			HiLoBands2.Plots[0].Brush = Brushes.Lime;
//			HiLoBands2.Plots[1].Brush = Brushes.Red;
//			if (showHighLow2) AddChartIndicator(HiLoBands2);
			
			Momentum1			= Momentum(Close, 14);	
			Momentum1.Plots[0].Brush = Brushes.Yellow;
			Momentum1.Plots[0].Width = 2;
			if (showMomo) AddChartIndicator(Momentum1);
        }
        #endregion

        #region Properties
		
		[NinjaScriptProperty]
        [Display(Name = "Fast HiLo Period", Order = 1, GroupName="07. Strategy Settings")]
        public int FastPeriod { get; set; }		
		
		[NinjaScriptProperty]
        [Display(Name = "Slow HiLo Period", Order = 2, GroupName="07. Strategy Settings")]
        public int SlowPeriod { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Line Width", Order = 3, GroupName="07. Strategy Settings")]
        public int Width { get; set; }	
		
		[NinjaScriptProperty]
        [Display(Name = "Show High Low Bands", Order = 4, GroupName = "07. Strategy Settings")]
        public bool showHighLow { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Average Range Bars Lookback", Order = 5, GroupName="07. Strategy Settings")]
        public int lookbackPeriod { get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Momentum Up", Order = 6, GroupName="07. Strategy Settings")]
		public int MomoUp
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Momentum Down", Order = 7, GroupName="07. Strategy Settings")]
		public int MomoDown
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show Momentum", Order = 8, GroupName = "07. Strategy Settings")]
        public bool showMomo { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trail Stop Tick Offset", Order = 9, GroupName="07. Strategy Settings")]
		public int TrailOffset
		{ get; set; }

        #endregion
    }
}
