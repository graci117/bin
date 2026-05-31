#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Windows.Media;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class SMMDots : Indicator
    {
        private double fastEMA;
        private double slowEMA;

        #region === Inputs ===
        [Range(1, 200), NinjaScriptProperty]
        [Display(Name = "Fast Period", Order = 1, GroupName = "Parameters")]
        public int FastPeriod { get; set; }

        [Range(1, 200), NinjaScriptProperty]
        [Display(Name = "Slow Period", Order = 2, GroupName = "Parameters")]
        public int SlowPeriod { get; set; }
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "SMM dots";
                Description = "Generates colored dots (green/red/gray) to confirm SMM signals.";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = false;
                DrawOnPricePanel = false;
                DisplayInDataBox = true;
                IsSuspendedWhileInactive = true;

                FastPeriod = 10;
                SlowPeriod = 25;

                AddPlot(new Stroke(Brushes.Gray, 6), PlotStyle.Dot, "Dots");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(FastPeriod, SlowPeriod))
            {
                Values[0][0] = 0;
                PlotBrushes[0][0] = Brushes.Gray;
                return;
            }

            // --- Basic sample logic (you can replace with SMM-based confirmation) ---
            double fast = EMA(Close, FastPeriod)[0];
            double slow = EMA(Close, SlowPeriod)[0];

            int direction;
            if (fast > slow)
                direction = 1;     // bullish
            else if (fast < slow)
                direction = -1;    // bearish
            else
                direction = 0;     // neutral

            Values[0][0] = direction;

            // --- Color dots based on direction ---
            if (direction == 1)
                PlotBrushes[0][0] = Brushes.LimeGreen;
            else if (direction == -1)
                PlotBrushes[0][0] = Brushes.Red;
            else
                PlotBrushes[0][0] = Brushes.Gray;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SMMDots[] cacheSMMDots;
		public SMMDots SMMDots(int fastPeriod, int slowPeriod)
		{
			return SMMDots(Input, fastPeriod, slowPeriod);
		}

		public SMMDots SMMDots(ISeries<double> input, int fastPeriod, int slowPeriod)
		{
			if (cacheSMMDots != null)
				for (int idx = 0; idx < cacheSMMDots.Length; idx++)
					if (cacheSMMDots[idx] != null && cacheSMMDots[idx].FastPeriod == fastPeriod && cacheSMMDots[idx].SlowPeriod == slowPeriod && cacheSMMDots[idx].EqualsInput(input))
						return cacheSMMDots[idx];
			return CacheIndicator<SMMDots>(new SMMDots(){ FastPeriod = fastPeriod, SlowPeriod = slowPeriod }, input, ref cacheSMMDots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMMDots SMMDots(int fastPeriod, int slowPeriod)
		{
			return indicator.SMMDots(Input, fastPeriod, slowPeriod);
		}

		public Indicators.SMMDots SMMDots(ISeries<double> input , int fastPeriod, int slowPeriod)
		{
			return indicator.SMMDots(input, fastPeriod, slowPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMMDots SMMDots(int fastPeriod, int slowPeriod)
		{
			return indicator.SMMDots(Input, fastPeriod, slowPeriod);
		}

		public Indicators.SMMDots SMMDots(ISeries<double> input , int fastPeriod, int slowPeriod)
		{
			return indicator.SMMDots(input, fastPeriod, slowPeriod);
		}
	}
}

#endregion
