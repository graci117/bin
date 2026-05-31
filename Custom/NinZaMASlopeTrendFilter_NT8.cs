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
		
		private ninZaMASlopeTrendFilter[] cacheninZaMASlopeTrendFilter;

		
		public ninZaMASlopeTrendFilter ninZaMASlopeTrendFilter(bool noSideways, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool mAFilterEnabled, bool mAFilterAfterSmoothing, double mAFilterMultiplier, ninZaMASlopeTrendFilter_FilterUnit mAFilterUnit, int slopeLookback, bool slopeSmoothingEnabled, ninZa_MAType slopeSmoothingMethod, int slopeSmoothingPeriod, int thresholdUptrendStart, int thresholdUptrendEnd, int thresholdDowntrendStart, int thresholdDowntrendEnd, int resumingSlowdownSplit, int aTRPeriod)
		{
			return ninZaMASlopeTrendFilter(Input, noSideways, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, mAFilterEnabled, mAFilterAfterSmoothing, mAFilterMultiplier, mAFilterUnit, slopeLookback, slopeSmoothingEnabled, slopeSmoothingMethod, slopeSmoothingPeriod, thresholdUptrendStart, thresholdUptrendEnd, thresholdDowntrendStart, thresholdDowntrendEnd, resumingSlowdownSplit, aTRPeriod);
		}


		
		public ninZaMASlopeTrendFilter ninZaMASlopeTrendFilter(ISeries<double> input, bool noSideways, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool mAFilterEnabled, bool mAFilterAfterSmoothing, double mAFilterMultiplier, ninZaMASlopeTrendFilter_FilterUnit mAFilterUnit, int slopeLookback, bool slopeSmoothingEnabled, ninZa_MAType slopeSmoothingMethod, int slopeSmoothingPeriod, int thresholdUptrendStart, int thresholdUptrendEnd, int thresholdDowntrendStart, int thresholdDowntrendEnd, int resumingSlowdownSplit, int aTRPeriod)
		{
			if (cacheninZaMASlopeTrendFilter != null)
				for (int idx = 0; idx < cacheninZaMASlopeTrendFilter.Length; idx++)
					if (cacheninZaMASlopeTrendFilter[idx].NoSideways == noSideways && cacheninZaMASlopeTrendFilter[idx].MAType == mAType && cacheninZaMASlopeTrendFilter[idx].MAPeriod == mAPeriod && cacheninZaMASlopeTrendFilter[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaMASlopeTrendFilter[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaMASlopeTrendFilter[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaMASlopeTrendFilter[idx].MAFilterEnabled == mAFilterEnabled && cacheninZaMASlopeTrendFilter[idx].MAFilterAfterSmoothing == mAFilterAfterSmoothing && cacheninZaMASlopeTrendFilter[idx].MAFilterMultiplier == mAFilterMultiplier && cacheninZaMASlopeTrendFilter[idx].MAFilterUnit == mAFilterUnit && cacheninZaMASlopeTrendFilter[idx].SlopeLookback == slopeLookback && cacheninZaMASlopeTrendFilter[idx].SlopeSmoothingEnabled == slopeSmoothingEnabled && cacheninZaMASlopeTrendFilter[idx].SlopeSmoothingMethod == slopeSmoothingMethod && cacheninZaMASlopeTrendFilter[idx].SlopeSmoothingPeriod == slopeSmoothingPeriod && cacheninZaMASlopeTrendFilter[idx].ThresholdUptrendStart == thresholdUptrendStart && cacheninZaMASlopeTrendFilter[idx].ThresholdUptrendEnd == thresholdUptrendEnd && cacheninZaMASlopeTrendFilter[idx].ThresholdDowntrendStart == thresholdDowntrendStart && cacheninZaMASlopeTrendFilter[idx].ThresholdDowntrendEnd == thresholdDowntrendEnd && cacheninZaMASlopeTrendFilter[idx].ResumingSlowdownSplit == resumingSlowdownSplit && cacheninZaMASlopeTrendFilter[idx].ATRPeriod == aTRPeriod && cacheninZaMASlopeTrendFilter[idx].EqualsInput(input))
						return cacheninZaMASlopeTrendFilter[idx];
			return CacheIndicator<ninZaMASlopeTrendFilter>(new ninZaMASlopeTrendFilter(){ NoSideways = noSideways, MAType = mAType, MAPeriod = mAPeriod, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, MAFilterEnabled = mAFilterEnabled, MAFilterAfterSmoothing = mAFilterAfterSmoothing, MAFilterMultiplier = mAFilterMultiplier, MAFilterUnit = mAFilterUnit, SlopeLookback = slopeLookback, SlopeSmoothingEnabled = slopeSmoothingEnabled, SlopeSmoothingMethod = slopeSmoothingMethod, SlopeSmoothingPeriod = slopeSmoothingPeriod, ThresholdUptrendStart = thresholdUptrendStart, ThresholdUptrendEnd = thresholdUptrendEnd, ThresholdDowntrendStart = thresholdDowntrendStart, ThresholdDowntrendEnd = thresholdDowntrendEnd, ResumingSlowdownSplit = resumingSlowdownSplit, ATRPeriod = aTRPeriod }, input, ref cacheninZaMASlopeTrendFilter);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMASlopeTrendFilter ninZaMASlopeTrendFilter(bool noSideways, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool mAFilterEnabled, bool mAFilterAfterSmoothing, double mAFilterMultiplier, ninZaMASlopeTrendFilter_FilterUnit mAFilterUnit, int slopeLookback, bool slopeSmoothingEnabled, ninZa_MAType slopeSmoothingMethod, int slopeSmoothingPeriod, int thresholdUptrendStart, int thresholdUptrendEnd, int thresholdDowntrendStart, int thresholdDowntrendEnd, int resumingSlowdownSplit, int aTRPeriod)
		{
			return indicator.ninZaMASlopeTrendFilter(Input, noSideways, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, mAFilterEnabled, mAFilterAfterSmoothing, mAFilterMultiplier, mAFilterUnit, slopeLookback, slopeSmoothingEnabled, slopeSmoothingMethod, slopeSmoothingPeriod, thresholdUptrendStart, thresholdUptrendEnd, thresholdDowntrendStart, thresholdDowntrendEnd, resumingSlowdownSplit, aTRPeriod);
		}


		
		public Indicators.ninZaMASlopeTrendFilter ninZaMASlopeTrendFilter(ISeries<double> input , bool noSideways, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool mAFilterEnabled, bool mAFilterAfterSmoothing, double mAFilterMultiplier, ninZaMASlopeTrendFilter_FilterUnit mAFilterUnit, int slopeLookback, bool slopeSmoothingEnabled, ninZa_MAType slopeSmoothingMethod, int slopeSmoothingPeriod, int thresholdUptrendStart, int thresholdUptrendEnd, int thresholdDowntrendStart, int thresholdDowntrendEnd, int resumingSlowdownSplit, int aTRPeriod)
		{
			return indicator.ninZaMASlopeTrendFilter(input, noSideways, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, mAFilterEnabled, mAFilterAfterSmoothing, mAFilterMultiplier, mAFilterUnit, slopeLookback, slopeSmoothingEnabled, slopeSmoothingMethod, slopeSmoothingPeriod, thresholdUptrendStart, thresholdUptrendEnd, thresholdDowntrendStart, thresholdDowntrendEnd, resumingSlowdownSplit, aTRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMASlopeTrendFilter ninZaMASlopeTrendFilter(bool noSideways, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool mAFilterEnabled, bool mAFilterAfterSmoothing, double mAFilterMultiplier, ninZaMASlopeTrendFilter_FilterUnit mAFilterUnit, int slopeLookback, bool slopeSmoothingEnabled, ninZa_MAType slopeSmoothingMethod, int slopeSmoothingPeriod, int thresholdUptrendStart, int thresholdUptrendEnd, int thresholdDowntrendStart, int thresholdDowntrendEnd, int resumingSlowdownSplit, int aTRPeriod)
		{
			return indicator.ninZaMASlopeTrendFilter(Input, noSideways, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, mAFilterEnabled, mAFilterAfterSmoothing, mAFilterMultiplier, mAFilterUnit, slopeLookback, slopeSmoothingEnabled, slopeSmoothingMethod, slopeSmoothingPeriod, thresholdUptrendStart, thresholdUptrendEnd, thresholdDowntrendStart, thresholdDowntrendEnd, resumingSlowdownSplit, aTRPeriod);
		}


		
		public Indicators.ninZaMASlopeTrendFilter ninZaMASlopeTrendFilter(ISeries<double> input , bool noSideways, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool mAFilterEnabled, bool mAFilterAfterSmoothing, double mAFilterMultiplier, ninZaMASlopeTrendFilter_FilterUnit mAFilterUnit, int slopeLookback, bool slopeSmoothingEnabled, ninZa_MAType slopeSmoothingMethod, int slopeSmoothingPeriod, int thresholdUptrendStart, int thresholdUptrendEnd, int thresholdDowntrendStart, int thresholdDowntrendEnd, int resumingSlowdownSplit, int aTRPeriod)
		{
			return indicator.ninZaMASlopeTrendFilter(input, noSideways, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, mAFilterEnabled, mAFilterAfterSmoothing, mAFilterMultiplier, mAFilterUnit, slopeLookback, slopeSmoothingEnabled, slopeSmoothingMethod, slopeSmoothingPeriod, thresholdUptrendStart, thresholdUptrendEnd, thresholdDowntrendStart, thresholdDowntrendEnd, resumingSlowdownSplit, aTRPeriod);
		}

	}
}

#endregion
