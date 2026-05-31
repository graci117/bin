#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class HiLoBands : Indicator
    {
		private LinReg LinReg1, LinReg2;
		private int LinRegPeriod;
		private int LinRegPeriod2;
		private bool linRegUp;
		private bool linRegDown;
		
		private RSI RSI1;
		private bool rsiUp;
		private bool rsiDown;
		
		private Momentum Momentum1;
		private int MomoUp;
		private int MomoDown;
		private bool momoUp;
		private bool momoDown;
		
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Draw highest high and lowest low within the last x number of bars.";
                Name = "HiLoBands";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                LookbackPeriod = 40; // Default lookback period
                LookbackPeriod2 = 100; // Default lookback period
                Width = 2;
				
				MomoUp = 0;
				MomoDown = 0;
				
				LinRegPeriod 	= 2;
				LinRegPeriod2	= 9;
				
                // Add plots for the highest high and lowest low
                AddPlot(Brushes.Cyan, "HighestHigh"); // fast high  line
                AddPlot(Brushes.Magenta, "LowestLow");     // slow low  line
				AddPlot(Brushes.Lime, "HighestHigh2");     // middle line
				AddPlot(Brushes.Red, "LowestLow2"); // slow high line
                AddPlot(Brushes.LightGray, "MiddleLine");     // slow low line
            }
            else if (State == State.Configure)
            {
                // Set the stroke thickness for the plots
                Plots[0].Width = Width; // Set thickness for HighestHigh plot
                Plots[1].Width = Width; // Set thickness for LowestLow plot
				Plots[2].Width = Width; // Set thickness for HighestHigh plot
                Plots[3].Width = Width; // Set thickness for LowestLow plot
				Plots[4].Width = Width; // Set thickness for LowestLow plot
            }
            else if (State == State.DataLoaded)
            {
                RSI1 		= RSI(Close, 14, 3);
				Momentum1	= Momentum(Close, 14);
				LinReg1 = LinReg(Close, LinRegPeriod);
				LinReg2 = LinReg(Close, LinRegPeriod2);
            }
        }

        protected override void OnBarUpdate()
        {
            // Ensure we have enough bars to calculate
            if (CurrentBar < LookbackPeriod)
                return;

            // Calculate the highest high and lowest low for the last x bars
            double highestHigh = MAX(High, LookbackPeriod)[0];
            double lowestLow = MIN(Low, LookbackPeriod)[0];
			double highestHigh2 = MAX(High, LookbackPeriod2)[0];
            double lowestLow2 = MIN(Low, LookbackPeriod2)[0];
			double midline = (highestHigh + lowestLow) / 2;

            // Assign the values to the plots
            Values[0][0] = highestHigh; // Assign to the first plot (HighestHigh)
            Values[1][0] = lowestLow;   // Assign to the second plot (LowestLow)
			Values[2][0] = highestHigh2; // Assign to the third plot (HighestHigh2)
            Values[3][0] = lowestLow2;   // Assign to the fouurth plot (LowestLow2)
			Values[4][0] = midline;   // Assign to the fifth plot (MiddleLine)
			
			momoUp = Momentum1[0] > MomoUp;
			momoDown = Momentum1[0] < MomoDown;
			
			rsiUp = RSI1[0] > RSI1[1] && RSI1[0] < 50;
			rsiDown = RSI1[0] < RSI1[1];
			
			linRegUp = LinReg2[0] > LinReg2[1];
			linRegDown = LinReg1[0] < LinReg1[1];
			
			// Long entry
//			if (((Low[0] > Low[1] && Low[1] == Values[3][1] && Values[3][1] < Values[3][2])
//				|| (Low[0] > Low[1] && Low[1] == Values[1][1] && Values[1][1] < Values[1][2])))
//				&& linRegUp)
//			{
//				Draw.ArrowUp(this, "LE " + Convert.ToString(CurrentBars[0]), false, 0, Low[0] - 10 * TickSize, Brushes.Cyan);				
//			}
			
			// Short entry
//			if (((High[0] < High[1] && High[1] ==  Values[2][1] && Values[2][1] > Values[2][2])
//				|| (High[0] < High[1] && High[1] ==  Values[0][1] && Values[0][1] > Values[0][2])))
//				&& linRegDown)
//			{
//				Draw.ArrowDown(this, "SE " + Convert.ToString(CurrentBars[0]), false, 0, High[0] + 10 * TickSize, Brushes.Yellow);
//			}
        }

        #region Properties
		
//        [NinjaScriptProperty]
//        [Display(Name = "Highest High", Order = 1, GroupName = "Parameters")]
//        public Series<double> HighestHigh { get; set; }

//        [NinjaScriptProperty]
//        [Display(Name = "Lowest Low", Order = 2, GroupName = "Parameters")]
//        public Series<double> LowestLow { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Fast Period", Description = "Number of bars to look back", Order = 1, GroupName = "Parameters")]
        public int LookbackPeriod { get; set; }
		
		
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Slow Period", Description = "Number of bars to look back", Order = 2, GroupName = "Parameters")]
        public int LookbackPeriod2 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Line Width", Order = 3, GroupName = "Parameters")]
        public int Width { get; set; }
		
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HiLoBands[] cacheHiLoBands;
		public HiLoBands HiLoBands(int lookbackPeriod, int lookbackPeriod2, int width)
		{
			return HiLoBands(Input, lookbackPeriod, lookbackPeriod2, width);
		}

		public HiLoBands HiLoBands(ISeries<double> input, int lookbackPeriod, int lookbackPeriod2, int width)
		{
			if (cacheHiLoBands != null)
				for (int idx = 0; idx < cacheHiLoBands.Length; idx++)
					if (cacheHiLoBands[idx] != null && cacheHiLoBands[idx].LookbackPeriod == lookbackPeriod && cacheHiLoBands[idx].LookbackPeriod2 == lookbackPeriod2 && cacheHiLoBands[idx].Width == width && cacheHiLoBands[idx].EqualsInput(input))
						return cacheHiLoBands[idx];
			return CacheIndicator<HiLoBands>(new HiLoBands(){ LookbackPeriod = lookbackPeriod, LookbackPeriod2 = lookbackPeriod2, Width = width }, input, ref cacheHiLoBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HiLoBands HiLoBands(int lookbackPeriod, int lookbackPeriod2, int width)
		{
			return indicator.HiLoBands(Input, lookbackPeriod, lookbackPeriod2, width);
		}

		public Indicators.HiLoBands HiLoBands(ISeries<double> input , int lookbackPeriod, int lookbackPeriod2, int width)
		{
			return indicator.HiLoBands(input, lookbackPeriod, lookbackPeriod2, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HiLoBands HiLoBands(int lookbackPeriod, int lookbackPeriod2, int width)
		{
			return indicator.HiLoBands(Input, lookbackPeriod, lookbackPeriod2, width);
		}

		public Indicators.HiLoBands HiLoBands(ISeries<double> input , int lookbackPeriod, int lookbackPeriod2, int width)
		{
			return indicator.HiLoBands(input, lookbackPeriod, lookbackPeriod2, width);
		}
	}
}

#endregion
