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
		
		private ninZaVoluCandlez[] cacheninZaVoluCandlez;

		
		public ninZaVoluCandlez ninZaVoluCandlez(ninZa_MAType volumeMAType, int volumeMAPeriod, bool volumeMASmoothingEnabled, ninZa_MAType volumeMASmoothingMethod, int volumeMASmoothingPeriod, long thresholdMinimumVolume, long thresholdHighVolume, ninZaVoluCandlez_BarBiasMode barBiasMode, int barBiasMinimumBody, int barBiasMinimumSpread, double barBiasNeutralRangePercentage)
		{
			return ninZaVoluCandlez(Input, volumeMAType, volumeMAPeriod, volumeMASmoothingEnabled, volumeMASmoothingMethod, volumeMASmoothingPeriod, thresholdMinimumVolume, thresholdHighVolume, barBiasMode, barBiasMinimumBody, barBiasMinimumSpread, barBiasNeutralRangePercentage);
		}


		
		public ninZaVoluCandlez ninZaVoluCandlez(ISeries<double> input, ninZa_MAType volumeMAType, int volumeMAPeriod, bool volumeMASmoothingEnabled, ninZa_MAType volumeMASmoothingMethod, int volumeMASmoothingPeriod, long thresholdMinimumVolume, long thresholdHighVolume, ninZaVoluCandlez_BarBiasMode barBiasMode, int barBiasMinimumBody, int barBiasMinimumSpread, double barBiasNeutralRangePercentage)
		{
			if (cacheninZaVoluCandlez != null)
				for (int idx = 0; idx < cacheninZaVoluCandlez.Length; idx++)
					if (cacheninZaVoluCandlez[idx].VolumeMAType == volumeMAType && cacheninZaVoluCandlez[idx].VolumeMAPeriod == volumeMAPeriod && cacheninZaVoluCandlez[idx].VolumeMASmoothingEnabled == volumeMASmoothingEnabled && cacheninZaVoluCandlez[idx].VolumeMASmoothingMethod == volumeMASmoothingMethod && cacheninZaVoluCandlez[idx].VolumeMASmoothingPeriod == volumeMASmoothingPeriod && cacheninZaVoluCandlez[idx].ThresholdMinimumVolume == thresholdMinimumVolume && cacheninZaVoluCandlez[idx].ThresholdHighVolume == thresholdHighVolume && cacheninZaVoluCandlez[idx].BarBiasMode == barBiasMode && cacheninZaVoluCandlez[idx].BarBiasMinimumBody == barBiasMinimumBody && cacheninZaVoluCandlez[idx].BarBiasMinimumSpread == barBiasMinimumSpread && cacheninZaVoluCandlez[idx].BarBiasNeutralRangePercentage == barBiasNeutralRangePercentage && cacheninZaVoluCandlez[idx].EqualsInput(input))
						return cacheninZaVoluCandlez[idx];
			return CacheIndicator<ninZaVoluCandlez>(new ninZaVoluCandlez(){ VolumeMAType = volumeMAType, VolumeMAPeriod = volumeMAPeriod, VolumeMASmoothingEnabled = volumeMASmoothingEnabled, VolumeMASmoothingMethod = volumeMASmoothingMethod, VolumeMASmoothingPeriod = volumeMASmoothingPeriod, ThresholdMinimumVolume = thresholdMinimumVolume, ThresholdHighVolume = thresholdHighVolume, BarBiasMode = barBiasMode, BarBiasMinimumBody = barBiasMinimumBody, BarBiasMinimumSpread = barBiasMinimumSpread, BarBiasNeutralRangePercentage = barBiasNeutralRangePercentage }, input, ref cacheninZaVoluCandlez);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaVoluCandlez ninZaVoluCandlez(ninZa_MAType volumeMAType, int volumeMAPeriod, bool volumeMASmoothingEnabled, ninZa_MAType volumeMASmoothingMethod, int volumeMASmoothingPeriod, long thresholdMinimumVolume, long thresholdHighVolume, ninZaVoluCandlez_BarBiasMode barBiasMode, int barBiasMinimumBody, int barBiasMinimumSpread, double barBiasNeutralRangePercentage)
		{
			return indicator.ninZaVoluCandlez(Input, volumeMAType, volumeMAPeriod, volumeMASmoothingEnabled, volumeMASmoothingMethod, volumeMASmoothingPeriod, thresholdMinimumVolume, thresholdHighVolume, barBiasMode, barBiasMinimumBody, barBiasMinimumSpread, barBiasNeutralRangePercentage);
		}


		
		public Indicators.ninZaVoluCandlez ninZaVoluCandlez(ISeries<double> input , ninZa_MAType volumeMAType, int volumeMAPeriod, bool volumeMASmoothingEnabled, ninZa_MAType volumeMASmoothingMethod, int volumeMASmoothingPeriod, long thresholdMinimumVolume, long thresholdHighVolume, ninZaVoluCandlez_BarBiasMode barBiasMode, int barBiasMinimumBody, int barBiasMinimumSpread, double barBiasNeutralRangePercentage)
		{
			return indicator.ninZaVoluCandlez(input, volumeMAType, volumeMAPeriod, volumeMASmoothingEnabled, volumeMASmoothingMethod, volumeMASmoothingPeriod, thresholdMinimumVolume, thresholdHighVolume, barBiasMode, barBiasMinimumBody, barBiasMinimumSpread, barBiasNeutralRangePercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaVoluCandlez ninZaVoluCandlez(ninZa_MAType volumeMAType, int volumeMAPeriod, bool volumeMASmoothingEnabled, ninZa_MAType volumeMASmoothingMethod, int volumeMASmoothingPeriod, long thresholdMinimumVolume, long thresholdHighVolume, ninZaVoluCandlez_BarBiasMode barBiasMode, int barBiasMinimumBody, int barBiasMinimumSpread, double barBiasNeutralRangePercentage)
		{
			return indicator.ninZaVoluCandlez(Input, volumeMAType, volumeMAPeriod, volumeMASmoothingEnabled, volumeMASmoothingMethod, volumeMASmoothingPeriod, thresholdMinimumVolume, thresholdHighVolume, barBiasMode, barBiasMinimumBody, barBiasMinimumSpread, barBiasNeutralRangePercentage);
		}


		
		public Indicators.ninZaVoluCandlez ninZaVoluCandlez(ISeries<double> input , ninZa_MAType volumeMAType, int volumeMAPeriod, bool volumeMASmoothingEnabled, ninZa_MAType volumeMASmoothingMethod, int volumeMASmoothingPeriod, long thresholdMinimumVolume, long thresholdHighVolume, ninZaVoluCandlez_BarBiasMode barBiasMode, int barBiasMinimumBody, int barBiasMinimumSpread, double barBiasNeutralRangePercentage)
		{
			return indicator.ninZaVoluCandlez(input, volumeMAType, volumeMAPeriod, volumeMASmoothingEnabled, volumeMASmoothingMethod, volumeMASmoothingPeriod, thresholdMinimumVolume, thresholdHighVolume, barBiasMode, barBiasMinimumBody, barBiasMinimumSpread, barBiasNeutralRangePercentage);
		}

	}
}

#endregion
