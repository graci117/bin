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
		
		private ninZaSidewayzZP[] cacheninZaSidewayzZP;

		
		public ninZaSidewayzZP ninZaSidewayzZP(int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return ninZaSidewayzZP(Input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}


		
		public ninZaSidewayzZP ninZaSidewayzZP(ISeries<double> input, int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			if (cacheninZaSidewayzZP != null)
				for (int idx = 0; idx < cacheninZaSidewayzZP.Length; idx++)
					if (cacheninZaSidewayzZP[idx].Lookback == lookback && cacheninZaSidewayzZP[idx].Threshold == threshold && cacheninZaSidewayzZP[idx].ThresholdMetSpan == thresholdMetSpan && cacheninZaSidewayzZP[idx].MultiplierMaxBaseRange == multiplierMaxBaseRange && cacheninZaSidewayzZP[idx].MultiplierBoundOffset == multiplierBoundOffset && cacheninZaSidewayzZP[idx].ATRPeriod == aTRPeriod && cacheninZaSidewayzZP[idx].BarBiasNeutralRangePercentage == barBiasNeutralRangePercentage && cacheninZaSidewayzZP[idx].BarBiasMinimumSpread == barBiasMinimumSpread && cacheninZaSidewayzZP[idx].EqualsInput(input))
						return cacheninZaSidewayzZP[idx];
			return CacheIndicator<ninZaSidewayzZP>(new ninZaSidewayzZP(){ Lookback = lookback, Threshold = threshold, ThresholdMetSpan = thresholdMetSpan, MultiplierMaxBaseRange = multiplierMaxBaseRange, MultiplierBoundOffset = multiplierBoundOffset, ATRPeriod = aTRPeriod, BarBiasNeutralRangePercentage = barBiasNeutralRangePercentage, BarBiasMinimumSpread = barBiasMinimumSpread }, input, ref cacheninZaSidewayzZP);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSidewayzZP ninZaSidewayzZP(int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzZP(Input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}


		
		public Indicators.ninZaSidewayzZP ninZaSidewayzZP(ISeries<double> input , int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzZP(input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSidewayzZP ninZaSidewayzZP(int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzZP(Input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}


		
		public Indicators.ninZaSidewayzZP ninZaSidewayzZP(ISeries<double> input , int lookback, int threshold, int thresholdMetSpan, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, int barBiasNeutralRangePercentage, int barBiasMinimumSpread)
		{
			return indicator.ninZaSidewayzZP(input, lookback, threshold, thresholdMetSpan, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, barBiasNeutralRangePercentage, barBiasMinimumSpread);
		}

	}
}

#endregion
