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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    public class FibonacciAlerts : Indicator
    {
        private int lookbackPeriod = 50;
        private double fibonacciBuffer = 0.002; // Currently not used in calculations
        
        // Fibonacci levels
        private double fib0, fib100, fib382, fib500, fib618, fib236, fib764;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                                    = "Calculates Fibonacci retracement levels based on a lookback period and issues buy/sell signals when price crosses key Fibonacci levels.";
                Name                                           = "FibonacciAlerts";
                Calculate                                      = Calculate.OnBarClose;
                IsOverlay                                      = true;  // Overlay indicator on price chart
                PaintPriceMarkers                              = false;
                IsSuspendedWhileInactive                       = true;

                // Define the plots for the Fibonacci levels
                AddPlot(Brushes.Blue,  "Fib0");      // 0%
				AddPlot(Brushes.Cyan,  "Fib23.6");      // 23.6
                AddPlot(Brushes.Green,  "Fib38.2");  // 38.2%
                AddPlot(Brushes.Orange, "Fib50");   // 50%
                AddPlot(Brushes.Red, "Fib61.8");    // 61.8%
				AddPlot(Brushes.Pink,  "Fib76.4");      // 0%
                AddPlot(Brushes.Purple, "Fib100");  // 100%
				
//				AddPlot(new Stroke(Brushes.Blue, 1), PlotStyle.Line,  "Fib0");      // 0%
//                AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line,  "Fib38.2");  // 38.2%
//                AddPlot(new Stroke(Brushes.Orange, 1), "Fib50");   // 50%
//                AddPlot(new Stroke(Brushes.Red, 1), "Fib61.8");    // 61.8%
//                AddPlot(new Stroke(Brushes.Purple, 1), "Fib100");  // 100%
				//AddPlot(Brushes.Green, "ADX");
				
				//AddPlot(new Stroke(Brushes.Yellow, 3), PlotStyle.Line, "T3Plot");
            }
            else if (State == State.Configure)
            {
                // Any configuration code if needed
            }
        }

        protected override void OnBarUpdate()
        {
            // Ensure there are enough bars to look back
            if (CurrentBar < lookbackPeriod)
                return;

            // Calculate the highest high and lowest low within the lookback period
            double highLevel = MAX(High, lookbackPeriod)[0];
            double lowLevel = MIN(Low, lookbackPeriod)[0];

            // Calculate Fibonacci levels
            fib0   = lowLevel;
            fib100 = highLevel;
			fib236 = fib0 + 0.236 * (fib100 - fib0);
            fib382 = fib0 + 0.382 * (fib100 - fib0);
            fib500 = fib0 + 0.5   * (fib100 - fib0);
            fib618 = fib0 + 0.618 * (fib100 - fib0);
			fib764 = fib0 + 0.764 * (fib100 - fib0);
			

            // Assign values to plots so they appear on the chart
            Values[0][0] = fib0;
			Values[1][0] = fib236;
            Values[2][0] = fib382;
            Values[3][0] = fib500;
            Values[4][0] = fib618;
			Values[5][0] = fib764;
            Values[6][0] = fib100;

            // Generate Buy/Sell signals:
            // Buy signal when the close on the current bar crosses above the 61.8% retracement level
            // Sell signal when the close on the current bar crosses below the 38.2% retracement level.
            bool buySignal = (Close[0] >= fib618) && (CurrentBar > 0 && Close[1] < fib618);
            bool sellSignal = (Close[0] <= fib382) && (CurrentBar > 0 && Close[1] > fib382);

            // Draw arrow signals on the chart. The tag ensures each drawn object has a unique name.
            if (buySignal)
            {
                //Draw.ArrowUp(this, "BuySignal" + CurrentBar, false, 0, Low[0] - (TickSize * 2), Brushes.Green);
				
				Draw.Text(this, Convert.ToString("BuySignal") + Convert.ToString(CurrentBars[0]), @"🢁", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
            }
            if (sellSignal)
            {
                //Draw.ArrowDown(this, "SellSignal" + CurrentBar, false, 0, High[0] + (TickSize * 2), Brushes.Red);
				Draw.Text(this, Convert.ToString("SellSignal") + Convert.ToString(CurrentBars[0]),  @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );
            }

            // In Pine Script the background is highlighted when the price is above/below certain levels.
            // NT8 does not support background highlighting in the same way; you might use Draw.Region or similar
            // if you need such visualization.
        }

        #region Properties
        [Display(Name="Lookback Period", Order=1, GroupName="Parameters")]
        [Range(1, int.MaxValue)]
        public int LookbackPeriod
        {
            get { return lookbackPeriod; }
            set { lookbackPeriod = value; }
        }

          [Display(Name="Fibonacci Buffer (%)", Order=1, GroupName="Parameters")]
        [Range(double.Epsilon, double.MaxValue)]
        public double FibonacciBuffer
        {
            get { return fibonacciBuffer; }
            set { fibonacciBuffer = value; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FibonacciAlerts[] cacheFibonacciAlerts;
		public FibonacciAlerts FibonacciAlerts()
		{
			return FibonacciAlerts(Input);
		}

		public FibonacciAlerts FibonacciAlerts(ISeries<double> input)
		{
			if (cacheFibonacciAlerts != null)
				for (int idx = 0; idx < cacheFibonacciAlerts.Length; idx++)
					if (cacheFibonacciAlerts[idx] != null &&  cacheFibonacciAlerts[idx].EqualsInput(input))
						return cacheFibonacciAlerts[idx];
			return CacheIndicator<FibonacciAlerts>(new FibonacciAlerts(), input, ref cacheFibonacciAlerts);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FibonacciAlerts FibonacciAlerts()
		{
			return indicator.FibonacciAlerts(Input);
		}

		public Indicators.FibonacciAlerts FibonacciAlerts(ISeries<double> input )
		{
			return indicator.FibonacciAlerts(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FibonacciAlerts FibonacciAlerts()
		{
			return indicator.FibonacciAlerts(Input);
		}

		public Indicators.FibonacciAlerts FibonacciAlerts(ISeries<double> input )
		{
			return indicator.FibonacciAlerts(input);
		}
	}
}

#endregion
