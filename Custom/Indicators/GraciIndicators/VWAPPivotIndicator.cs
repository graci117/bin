using System;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Indicators;
using System.Windows.Media;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class VWAPPivotIndicator : Indicator
    {
        private double runningPH = double.NaN;
        private double runningPL = double.NaN;
        private int pointLB = 5;
        private int pointRB = 5;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "VWAP and Pivot Indicator with Plotting";
                Name = "VWAPPivotIndicator";
                Calculate = Calculate.OnBarClose;
                AddPlot(Brushes.Red, "PivotHigh");
                AddPlot(Brushes.Blue, "PivotLow");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < 50) return; // Ensure enough bars are loaded

            // Calculate the pivot high and low
            double oh = Math.Max(Open[2], Math.Max(Open[1], Open[0]));
            double ol = Math.Min(Open[2], Math.Min(Open[1], Open[0]));

            double pointPH = PivotHigh(oh, pointLB, pointRB);
            double pointPL = PivotLow(ol, pointLB, pointRB);

            // Update running pivot high and low
            if (!double.IsNaN(pointPH))
                runningPH = pointPH;
            if (!double.IsNaN(pointPL))
                runningPL = pointPL;

            // Plot the running pivot high and low
            Values[0][0] = runningPH;
            Values[1][0] = runningPL;
        }

        private double PivotHigh(double value, int leftBars, int rightBars)
        {
            bool isPivotHigh = true;
            for (int i = 1; i <= leftBars; i++)
            {
                if (High[i] > value)
                {
                    isPivotHigh = false;
                    break;
                }
            }
            for (int i = 1; i <= rightBars; i++)
            {
                if (High[-i] > value)
                {
                    isPivotHigh = false;
                    break;
                }
            }
            return isPivotHigh ? value : double.NaN;
        }

        private double PivotLow(double value, int leftBars, int rightBars)
        {
            bool isPivotLow = true;
            for (int i = 1; i <= leftBars; i++)
            {
                if (Low[i] < value)
                {
                    isPivotLow = false;
                    break;
                }
            }
            for (int i = 1; i <= rightBars; i++)
            {
                if (Low[-i] < value)
                {
                    isPivotLow = false;
                    break;
                }
            }
            return isPivotLow ? value : double.NaN;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VWAPPivotIndicator[] cacheVWAPPivotIndicator;
		public VWAPPivotIndicator VWAPPivotIndicator()
		{
			return VWAPPivotIndicator(Input);
		}

		public VWAPPivotIndicator VWAPPivotIndicator(ISeries<double> input)
		{
			if (cacheVWAPPivotIndicator != null)
				for (int idx = 0; idx < cacheVWAPPivotIndicator.Length; idx++)
					if (cacheVWAPPivotIndicator[idx] != null &&  cacheVWAPPivotIndicator[idx].EqualsInput(input))
						return cacheVWAPPivotIndicator[idx];
			return CacheIndicator<VWAPPivotIndicator>(new VWAPPivotIndicator(), input, ref cacheVWAPPivotIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VWAPPivotIndicator VWAPPivotIndicator()
		{
			return indicator.VWAPPivotIndicator(Input);
		}

		public Indicators.VWAPPivotIndicator VWAPPivotIndicator(ISeries<double> input )
		{
			return indicator.VWAPPivotIndicator(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VWAPPivotIndicator VWAPPivotIndicator()
		{
			return indicator.VWAPPivotIndicator(Input);
		}

		public Indicators.VWAPPivotIndicator VWAPPivotIndicator(ISeries<double> input )
		{
			return indicator.VWAPPivotIndicator(input);
		}
	}
}

#endregion
