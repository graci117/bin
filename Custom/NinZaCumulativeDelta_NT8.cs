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
		
		private ninZaCumulativeDelta[] cacheninZaCumulativeDelta;

		
		public ninZaCumulativeDelta ninZaCumulativeDelta(ninZaCumulativeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdNegativeStrong, int movingMedianPeriod, int cumDeltaLookback)
		{
			return ninZaCumulativeDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdNegativeStrong, movingMedianPeriod, cumDeltaLookback);
		}


		
		public ninZaCumulativeDelta ninZaCumulativeDelta(ISeries<double> input, ninZaCumulativeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdNegativeStrong, int movingMedianPeriod, int cumDeltaLookback)
		{
			if (cacheninZaCumulativeDelta != null)
				for (int idx = 0; idx < cacheninZaCumulativeDelta.Length; idx++)
					if (cacheninZaCumulativeDelta[idx].VolumeBase == volumeBase && cacheninZaCumulativeDelta[idx].VolumeFilterEnabled == volumeFilterEnabled && cacheninZaCumulativeDelta[idx].VolumeFilterSizeMinimum == volumeFilterSizeMinimum && cacheninZaCumulativeDelta[idx].VolumeFilterSizeMaximum == volumeFilterSizeMaximum && cacheninZaCumulativeDelta[idx].ThresholdPositiveStrong == thresholdPositiveStrong && cacheninZaCumulativeDelta[idx].ThresholdNegativeStrong == thresholdNegativeStrong && cacheninZaCumulativeDelta[idx].MovingMedianPeriod == movingMedianPeriod && cacheninZaCumulativeDelta[idx].CumDeltaLookback == cumDeltaLookback && cacheninZaCumulativeDelta[idx].EqualsInput(input))
						return cacheninZaCumulativeDelta[idx];
			return CacheIndicator<ninZaCumulativeDelta>(new ninZaCumulativeDelta(){ VolumeBase = volumeBase, VolumeFilterEnabled = volumeFilterEnabled, VolumeFilterSizeMinimum = volumeFilterSizeMinimum, VolumeFilterSizeMaximum = volumeFilterSizeMaximum, ThresholdPositiveStrong = thresholdPositiveStrong, ThresholdNegativeStrong = thresholdNegativeStrong, MovingMedianPeriod = movingMedianPeriod, CumDeltaLookback = cumDeltaLookback }, input, ref cacheninZaCumulativeDelta);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaCumulativeDelta ninZaCumulativeDelta(ninZaCumulativeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdNegativeStrong, int movingMedianPeriod, int cumDeltaLookback)
		{
			return indicator.ninZaCumulativeDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdNegativeStrong, movingMedianPeriod, cumDeltaLookback);
		}


		
		public Indicators.ninZaCumulativeDelta ninZaCumulativeDelta(ISeries<double> input , ninZaCumulativeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdNegativeStrong, int movingMedianPeriod, int cumDeltaLookback)
		{
			return indicator.ninZaCumulativeDelta(input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdNegativeStrong, movingMedianPeriod, cumDeltaLookback);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaCumulativeDelta ninZaCumulativeDelta(ninZaCumulativeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdNegativeStrong, int movingMedianPeriod, int cumDeltaLookback)
		{
			return indicator.ninZaCumulativeDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdNegativeStrong, movingMedianPeriod, cumDeltaLookback);
		}


		
		public Indicators.ninZaCumulativeDelta ninZaCumulativeDelta(ISeries<double> input , ninZaCumulativeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdNegativeStrong, int movingMedianPeriod, int cumDeltaLookback)
		{
			return indicator.ninZaCumulativeDelta(input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdNegativeStrong, movingMedianPeriod, cumDeltaLookback);
		}

	}
}

#endregion
