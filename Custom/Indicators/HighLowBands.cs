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
    public class HighLowBands : Indicator
    {
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Draw highest high and lowest low within the last x number of bars.";
                Name = "HighLowBands";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                LookbackPeriod = 14; // Default lookback period
                Width = 2;

                // Add plots for the highest high and lowest low
                AddPlot(Brushes.Cyan, "HighestHigh"); // Green line
                AddPlot(Brushes.Magenta, "LowestLow");     // Red line
            }
            else if (State == State.Configure)
            {
                // Set the stroke thickness for the plots
                Plots[0].Width = Width; // Set thickness for HighestHigh plot
                Plots[1].Width = Width; // Set thickness for LowestLow plot
            }
        }

        protected override void OnBarUpdate()
        {
            // Ensure we have enough bars to calculate
            if (CurrentBar < LookbackPeriod)
                return;

            // Calculate the highest high and lowest low for the last x bars
            HighestHigh = MAX(High, LookbackPeriod)[0];
            LowestLow = MIN(Low, LookbackPeriod)[0];

            // Assign the values to the plots
            Values[0][0] = HighestHigh; // Assign to the first plot (HighestHigh)
            Values[1][0] = LowestLow;   // Assign to the second plot (LowestLow)
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Highest High", Order = 1, GroupName = "Filter Settings")]
        public double HighestHigh { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Lowest Low", Order = 1, GroupName = "Filter Settings")]
        public double LowestLow { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Lookback Period", Description = "Number of bars to look back", Order = 3, GroupName = "Parameters")]
        public int LookbackPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Line Width", Order = 4, GroupName = "Filter Settings")]
        public int Width { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HighLowBands[] cacheHighLowBands;
		public HighLowBands HighLowBands(double highestHigh, double lowestLow, int lookbackPeriod, int width)
		{
			return HighLowBands(Input, highestHigh, lowestLow, lookbackPeriod, width);
		}

		public HighLowBands HighLowBands(ISeries<double> input, double highestHigh, double lowestLow, int lookbackPeriod, int width)
		{
			if (cacheHighLowBands != null)
				for (int idx = 0; idx < cacheHighLowBands.Length; idx++)
					if (cacheHighLowBands[idx] != null && cacheHighLowBands[idx].HighestHigh == highestHigh && cacheHighLowBands[idx].LowestLow == lowestLow && cacheHighLowBands[idx].LookbackPeriod == lookbackPeriod && cacheHighLowBands[idx].Width == width && cacheHighLowBands[idx].EqualsInput(input))
						return cacheHighLowBands[idx];
			return CacheIndicator<HighLowBands>(new HighLowBands(){ HighestHigh = highestHigh, LowestLow = lowestLow, LookbackPeriod = lookbackPeriod, Width = width }, input, ref cacheHighLowBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HighLowBands HighLowBands(double highestHigh, double lowestLow, int lookbackPeriod, int width)
		{
			return indicator.HighLowBands(Input, highestHigh, lowestLow, lookbackPeriod, width);
		}

		public Indicators.HighLowBands HighLowBands(ISeries<double> input , double highestHigh, double lowestLow, int lookbackPeriod, int width)
		{
			return indicator.HighLowBands(input, highestHigh, lowestLow, lookbackPeriod, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HighLowBands HighLowBands(double highestHigh, double lowestLow, int lookbackPeriod, int width)
		{
			return indicator.HighLowBands(Input, highestHigh, lowestLow, lookbackPeriod, width);
		}

		public Indicators.HighLowBands HighLowBands(ISeries<double> input , double highestHigh, double lowestLow, int lookbackPeriod, int width)
		{
			return indicator.HighLowBands(input, highestHigh, lowestLow, lookbackPeriod, width);
		}
	}
}

#endregion
