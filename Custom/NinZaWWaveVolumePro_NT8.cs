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
		
		private ninZaWWaveVolumePro[] cacheninZaWWaveVolumePro;

		
		public ninZaWWaveVolumePro ninZaWWaveVolumePro(double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD, ninZaWWaveVolumePro_VolumeMode volumeMode, int volumeFilter)
		{
			return ninZaWWaveVolumePro(Input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD, volumeMode, volumeFilter);
		}


		
		public ninZaWWaveVolumePro ninZaWWaveVolumePro(ISeries<double> input, double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD, ninZaWWaveVolumePro_VolumeMode volumeMode, int volumeFilter)
		{
			if (cacheninZaWWaveVolumePro != null)
				for (int idx = 0; idx < cacheninZaWWaveVolumePro.Length; idx++)
					if (cacheninZaWWaveVolumePro[idx].OffsetMultiplierTrend == offsetMultiplierTrend && cacheninZaWWaveVolumePro[idx].OffsetMultiplierReversal == offsetMultiplierReversal && cacheninZaWWaveVolumePro[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaWWaveVolumePro[idx].BreakAtEOD == breakAtEOD && cacheninZaWWaveVolumePro[idx].VolumeMode == volumeMode && cacheninZaWWaveVolumePro[idx].VolumeFilter == volumeFilter && cacheninZaWWaveVolumePro[idx].EqualsInput(input))
						return cacheninZaWWaveVolumePro[idx];
			return CacheIndicator<ninZaWWaveVolumePro>(new ninZaWWaveVolumePro(){ OffsetMultiplierTrend = offsetMultiplierTrend, OffsetMultiplierReversal = offsetMultiplierReversal, OffsetATRPeriod = offsetATRPeriod, BreakAtEOD = breakAtEOD, VolumeMode = volumeMode, VolumeFilter = volumeFilter }, input, ref cacheninZaWWaveVolumePro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaWWaveVolumePro ninZaWWaveVolumePro(double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD, ninZaWWaveVolumePro_VolumeMode volumeMode, int volumeFilter)
		{
			return indicator.ninZaWWaveVolumePro(Input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD, volumeMode, volumeFilter);
		}


		
		public Indicators.ninZaWWaveVolumePro ninZaWWaveVolumePro(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD, ninZaWWaveVolumePro_VolumeMode volumeMode, int volumeFilter)
		{
			return indicator.ninZaWWaveVolumePro(input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD, volumeMode, volumeFilter);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaWWaveVolumePro ninZaWWaveVolumePro(double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD, ninZaWWaveVolumePro_VolumeMode volumeMode, int volumeFilter)
		{
			return indicator.ninZaWWaveVolumePro(Input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD, volumeMode, volumeFilter);
		}


		
		public Indicators.ninZaWWaveVolumePro ninZaWWaveVolumePro(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD, ninZaWWaveVolumePro_VolumeMode volumeMode, int volumeFilter)
		{
			return indicator.ninZaWWaveVolumePro(input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD, volumeMode, volumeFilter);
		}

	}
}

#endregion
