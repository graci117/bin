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
		
		private ninZaVolumeDelta[] cacheninZaVolumeDelta;

		
		public ninZaVolumeDelta ninZaVolumeDelta(ninZaVolumeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdPositiveModerate, int thresholdNegativeStrong, int thresholdNegativeModerate)
		{
			return ninZaVolumeDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdPositiveModerate, thresholdNegativeStrong, thresholdNegativeModerate);
		}


		
		public ninZaVolumeDelta ninZaVolumeDelta(ISeries<double> input, ninZaVolumeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdPositiveModerate, int thresholdNegativeStrong, int thresholdNegativeModerate)
		{
			if (cacheninZaVolumeDelta != null)
				for (int idx = 0; idx < cacheninZaVolumeDelta.Length; idx++)
					if (cacheninZaVolumeDelta[idx].VolumeBase == volumeBase && cacheninZaVolumeDelta[idx].VolumeFilterEnabled == volumeFilterEnabled && cacheninZaVolumeDelta[idx].VolumeFilterSizeMinimum == volumeFilterSizeMinimum && cacheninZaVolumeDelta[idx].VolumeFilterSizeMaximum == volumeFilterSizeMaximum && cacheninZaVolumeDelta[idx].ThresholdPositiveStrong == thresholdPositiveStrong && cacheninZaVolumeDelta[idx].ThresholdPositiveModerate == thresholdPositiveModerate && cacheninZaVolumeDelta[idx].ThresholdNegativeStrong == thresholdNegativeStrong && cacheninZaVolumeDelta[idx].ThresholdNegativeModerate == thresholdNegativeModerate && cacheninZaVolumeDelta[idx].EqualsInput(input))
						return cacheninZaVolumeDelta[idx];
			return CacheIndicator<ninZaVolumeDelta>(new ninZaVolumeDelta(){ VolumeBase = volumeBase, VolumeFilterEnabled = volumeFilterEnabled, VolumeFilterSizeMinimum = volumeFilterSizeMinimum, VolumeFilterSizeMaximum = volumeFilterSizeMaximum, ThresholdPositiveStrong = thresholdPositiveStrong, ThresholdPositiveModerate = thresholdPositiveModerate, ThresholdNegativeStrong = thresholdNegativeStrong, ThresholdNegativeModerate = thresholdNegativeModerate }, input, ref cacheninZaVolumeDelta);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaVolumeDelta ninZaVolumeDelta(ninZaVolumeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdPositiveModerate, int thresholdNegativeStrong, int thresholdNegativeModerate)
		{
			return indicator.ninZaVolumeDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdPositiveModerate, thresholdNegativeStrong, thresholdNegativeModerate);
		}


		
		public Indicators.ninZaVolumeDelta ninZaVolumeDelta(ISeries<double> input , ninZaVolumeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdPositiveModerate, int thresholdNegativeStrong, int thresholdNegativeModerate)
		{
			return indicator.ninZaVolumeDelta(input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdPositiveModerate, thresholdNegativeStrong, thresholdNegativeModerate);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaVolumeDelta ninZaVolumeDelta(ninZaVolumeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdPositiveModerate, int thresholdNegativeStrong, int thresholdNegativeModerate)
		{
			return indicator.ninZaVolumeDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdPositiveModerate, thresholdNegativeStrong, thresholdNegativeModerate);
		}


		
		public Indicators.ninZaVolumeDelta ninZaVolumeDelta(ISeries<double> input , ninZaVolumeDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int thresholdPositiveStrong, int thresholdPositiveModerate, int thresholdNegativeStrong, int thresholdNegativeModerate)
		{
			return indicator.ninZaVolumeDelta(input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, thresholdPositiveStrong, thresholdPositiveModerate, thresholdNegativeStrong, thresholdNegativeModerate);
		}

	}
}

#endregion
