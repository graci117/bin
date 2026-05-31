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

#endregion



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private ninZaSidewayzRT[] cacheninZaSidewayzRT;

		
		public ninZaSidewayzRT ninZaSidewayzRT(int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return ninZaSidewayzRT(Input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}


		
		public ninZaSidewayzRT ninZaSidewayzRT(ISeries<double> input, int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			if (cacheninZaSidewayzRT != null)
				for (int idx = 0; idx < cacheninZaSidewayzRT.Length; idx++)
					if (cacheninZaSidewayzRT[idx].Lookback == lookback && cacheninZaSidewayzRT[idx].Threshold == threshold && cacheninZaSidewayzRT[idx].ThresholdMetSpan == thresholdMetSpan && cacheninZaSidewayzRT[idx].MultiplierMaxBaseRange == multiplierMaxBaseRange && cacheninZaSidewayzRT[idx].MultiplierBoundOffset == multiplierBoundOffset && cacheninZaSidewayzRT[idx].ATRPeriod == aTRPeriod && cacheninZaSidewayzRT[idx].BarBiasNeutralRangePercentage == barBiasNeutralRangePercentage && cacheninZaSidewayzRT[idx].BarBiasMinimumSpread == barBiasMinimumSpread && cacheninZaSidewayzRT[idx].EqualsInput(input))
						return cacheninZaSidewayzRT[idx];
			return CacheIndicator<ninZaSidewayzRT>(new ninZaSidewayzRT(){ Lookback = lookback, Threshold = threshold, ThresholdMetSpan = thresholdMetSpan, MultiplierMaxBaseRange = multiplierMaxBaseRange, MultiplierBoundOffset = multiplierBoundOffset, ATRPeriod = aTRPeriod, BarBiasNeutralRangePercentage = barBiasNeutralRangePercentage, BarBiasMinimumSpread = barBiasMinimumSpread }, input, ref cacheninZaSidewayzRT);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSidewayzRT ninZaSidewayzRT(int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzRT(Input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}


		
		public Indicators.ninZaSidewayzRT ninZaSidewayzRT(ISeries<double> input , int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzRT(input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSidewayzRT ninZaSidewayzRT(int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzRT(Input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}


		
		public Indicators.ninZaSidewayzRT ninZaSidewayzRT(ISeries<double> input , int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzRT(input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}

	}
}

#endregion
