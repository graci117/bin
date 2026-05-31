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
		
		private ninZaRomantickPulse[] cacheninZaRomantickPulse;

		
		public ninZaRomantickPulse ninZaRomantickPulse(bool volumeFilterEnabled, int volumeFilterDeltaDiffMin, bool thresholdPOCEnabled, double thresholdPOCPercent, bool thresholdVolumeEnabled, double thresholdVolumePercent, bool thresholdBuySellEnabled, double thresholdBuySellPercent, int signalSplit)
		{
			return ninZaRomantickPulse(Input, volumeFilterEnabled, volumeFilterDeltaDiffMin, thresholdPOCEnabled, thresholdPOCPercent, thresholdVolumeEnabled, thresholdVolumePercent, thresholdBuySellEnabled, thresholdBuySellPercent, signalSplit);
		}


		
		public ninZaRomantickPulse ninZaRomantickPulse(ISeries<double> input, bool volumeFilterEnabled, int volumeFilterDeltaDiffMin, bool thresholdPOCEnabled, double thresholdPOCPercent, bool thresholdVolumeEnabled, double thresholdVolumePercent, bool thresholdBuySellEnabled, double thresholdBuySellPercent, int signalSplit)
		{
			if (cacheninZaRomantickPulse != null)
				for (int idx = 0; idx < cacheninZaRomantickPulse.Length; idx++)
					if (cacheninZaRomantickPulse[idx].VolumeFilterEnabled == volumeFilterEnabled && cacheninZaRomantickPulse[idx].VolumeFilterDeltaDiffMin == volumeFilterDeltaDiffMin && cacheninZaRomantickPulse[idx].ThresholdPOCEnabled == thresholdPOCEnabled && cacheninZaRomantickPulse[idx].ThresholdPOCPercent == thresholdPOCPercent && cacheninZaRomantickPulse[idx].ThresholdVolumeEnabled == thresholdVolumeEnabled && cacheninZaRomantickPulse[idx].ThresholdVolumePercent == thresholdVolumePercent && cacheninZaRomantickPulse[idx].ThresholdBuySellEnabled == thresholdBuySellEnabled && cacheninZaRomantickPulse[idx].ThresholdBuySellPercent == thresholdBuySellPercent && cacheninZaRomantickPulse[idx].SignalSplit == signalSplit && cacheninZaRomantickPulse[idx].EqualsInput(input))
						return cacheninZaRomantickPulse[idx];
			return CacheIndicator<ninZaRomantickPulse>(new ninZaRomantickPulse(){ VolumeFilterEnabled = volumeFilterEnabled, VolumeFilterDeltaDiffMin = volumeFilterDeltaDiffMin, ThresholdPOCEnabled = thresholdPOCEnabled, ThresholdPOCPercent = thresholdPOCPercent, ThresholdVolumeEnabled = thresholdVolumeEnabled, ThresholdVolumePercent = thresholdVolumePercent, ThresholdBuySellEnabled = thresholdBuySellEnabled, ThresholdBuySellPercent = thresholdBuySellPercent, SignalSplit = signalSplit }, input, ref cacheninZaRomantickPulse);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaRomantickPulse ninZaRomantickPulse(bool volumeFilterEnabled, int volumeFilterDeltaDiffMin, bool thresholdPOCEnabled, double thresholdPOCPercent, bool thresholdVolumeEnabled, double thresholdVolumePercent, bool thresholdBuySellEnabled, double thresholdBuySellPercent, int signalSplit)
		{
			return indicator.ninZaRomantickPulse(Input, volumeFilterEnabled, volumeFilterDeltaDiffMin, thresholdPOCEnabled, thresholdPOCPercent, thresholdVolumeEnabled, thresholdVolumePercent, thresholdBuySellEnabled, thresholdBuySellPercent, signalSplit);
		}


		
		public Indicators.ninZaRomantickPulse ninZaRomantickPulse(ISeries<double> input , bool volumeFilterEnabled, int volumeFilterDeltaDiffMin, bool thresholdPOCEnabled, double thresholdPOCPercent, bool thresholdVolumeEnabled, double thresholdVolumePercent, bool thresholdBuySellEnabled, double thresholdBuySellPercent, int signalSplit)
		{
			return indicator.ninZaRomantickPulse(input, volumeFilterEnabled, volumeFilterDeltaDiffMin, thresholdPOCEnabled, thresholdPOCPercent, thresholdVolumeEnabled, thresholdVolumePercent, thresholdBuySellEnabled, thresholdBuySellPercent, signalSplit);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaRomantickPulse ninZaRomantickPulse(bool volumeFilterEnabled, int volumeFilterDeltaDiffMin, bool thresholdPOCEnabled, double thresholdPOCPercent, bool thresholdVolumeEnabled, double thresholdVolumePercent, bool thresholdBuySellEnabled, double thresholdBuySellPercent, int signalSplit)
		{
			return indicator.ninZaRomantickPulse(Input, volumeFilterEnabled, volumeFilterDeltaDiffMin, thresholdPOCEnabled, thresholdPOCPercent, thresholdVolumeEnabled, thresholdVolumePercent, thresholdBuySellEnabled, thresholdBuySellPercent, signalSplit);
		}


		
		public Indicators.ninZaRomantickPulse ninZaRomantickPulse(ISeries<double> input , bool volumeFilterEnabled, int volumeFilterDeltaDiffMin, bool thresholdPOCEnabled, double thresholdPOCPercent, bool thresholdVolumeEnabled, double thresholdVolumePercent, bool thresholdBuySellEnabled, double thresholdBuySellPercent, int signalSplit)
		{
			return indicator.ninZaRomantickPulse(input, volumeFilterEnabled, volumeFilterDeltaDiffMin, thresholdPOCEnabled, thresholdPOCPercent, thresholdVolumeEnabled, thresholdVolumePercent, thresholdBuySellEnabled, thresholdBuySellPercent, signalSplit);
		}

	}
}

#endregion
