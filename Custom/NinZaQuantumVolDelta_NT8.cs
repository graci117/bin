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
		
		private ninZaQuantumVolDelta[] cacheninZaQuantumVolDelta;

		
		public ninZaQuantumVolDelta ninZaQuantumVolDelta(ninZaQuantumVolDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int volumePeriod, int deltaPeriod, double thresholdRatioStrongModerate, bool signalFilterEnabled)
		{
			return ninZaQuantumVolDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, volumePeriod, deltaPeriod, thresholdRatioStrongModerate, signalFilterEnabled);
		}


		
		public ninZaQuantumVolDelta ninZaQuantumVolDelta(ISeries<double> input, ninZaQuantumVolDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int volumePeriod, int deltaPeriod, double thresholdRatioStrongModerate, bool signalFilterEnabled)
		{
			if (cacheninZaQuantumVolDelta != null)
				for (int idx = 0; idx < cacheninZaQuantumVolDelta.Length; idx++)
					if (cacheninZaQuantumVolDelta[idx].VolumeBase == volumeBase && cacheninZaQuantumVolDelta[idx].VolumeFilterEnabled == volumeFilterEnabled && cacheninZaQuantumVolDelta[idx].VolumeFilterSizeMinimum == volumeFilterSizeMinimum && cacheninZaQuantumVolDelta[idx].VolumeFilterSizeMaximum == volumeFilterSizeMaximum && cacheninZaQuantumVolDelta[idx].VolumePeriod == volumePeriod && cacheninZaQuantumVolDelta[idx].DeltaPeriod == deltaPeriod && cacheninZaQuantumVolDelta[idx].ThresholdRatioStrongModerate == thresholdRatioStrongModerate && cacheninZaQuantumVolDelta[idx].SignalFilterEnabled == signalFilterEnabled && cacheninZaQuantumVolDelta[idx].EqualsInput(input))
						return cacheninZaQuantumVolDelta[idx];
			return CacheIndicator<ninZaQuantumVolDelta>(new ninZaQuantumVolDelta(){ VolumeBase = volumeBase, VolumeFilterEnabled = volumeFilterEnabled, VolumeFilterSizeMinimum = volumeFilterSizeMinimum, VolumeFilterSizeMaximum = volumeFilterSizeMaximum, VolumePeriod = volumePeriod, DeltaPeriod = deltaPeriod, ThresholdRatioStrongModerate = thresholdRatioStrongModerate, SignalFilterEnabled = signalFilterEnabled }, input, ref cacheninZaQuantumVolDelta);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaQuantumVolDelta ninZaQuantumVolDelta(ninZaQuantumVolDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int volumePeriod, int deltaPeriod, double thresholdRatioStrongModerate, bool signalFilterEnabled)
		{
			return indicator.ninZaQuantumVolDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, volumePeriod, deltaPeriod, thresholdRatioStrongModerate, signalFilterEnabled);
		}


		
		public Indicators.ninZaQuantumVolDelta ninZaQuantumVolDelta(ISeries<double> input , ninZaQuantumVolDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int volumePeriod, int deltaPeriod, double thresholdRatioStrongModerate, bool signalFilterEnabled)
		{
			return indicator.ninZaQuantumVolDelta(input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, volumePeriod, deltaPeriod, thresholdRatioStrongModerate, signalFilterEnabled);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaQuantumVolDelta ninZaQuantumVolDelta(ninZaQuantumVolDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int volumePeriod, int deltaPeriod, double thresholdRatioStrongModerate, bool signalFilterEnabled)
		{
			return indicator.ninZaQuantumVolDelta(Input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, volumePeriod, deltaPeriod, thresholdRatioStrongModerate, signalFilterEnabled);
		}


		
		public Indicators.ninZaQuantumVolDelta ninZaQuantumVolDelta(ISeries<double> input , ninZaQuantumVolDelta_VolumeBase volumeBase, bool volumeFilterEnabled, int volumeFilterSizeMinimum, int volumeFilterSizeMaximum, int volumePeriod, int deltaPeriod, double thresholdRatioStrongModerate, bool signalFilterEnabled)
		{
			return indicator.ninZaQuantumVolDelta(input, volumeBase, volumeFilterEnabled, volumeFilterSizeMinimum, volumeFilterSizeMaximum, volumePeriod, deltaPeriod, thresholdRatioStrongModerate, signalFilterEnabled);
		}

	}
}

#endregion
