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
		
		private ninZaSuperiorMACD[] cacheninZaSuperiorMACD;

		
		public ninZaSuperiorMACD ninZaSuperiorMACD(ninZa_MAType fastMAType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowMAType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, ninZa_MAType signalMAType, int signalPeriod, bool signalSmoothingEnabled, ninZa_MAType signalSmoothingMethod, int signalSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return ninZaSuperiorMACD(Input, fastMAType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowMAType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, signalMAType, signalPeriod, signalSmoothingEnabled, signalSmoothingMethod, signalSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}


		
		public ninZaSuperiorMACD ninZaSuperiorMACD(ISeries<double> input, ninZa_MAType fastMAType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowMAType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, ninZa_MAType signalMAType, int signalPeriod, bool signalSmoothingEnabled, ninZa_MAType signalSmoothingMethod, int signalSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			if (cacheninZaSuperiorMACD != null)
				for (int idx = 0; idx < cacheninZaSuperiorMACD.Length; idx++)
					if (cacheninZaSuperiorMACD[idx].FastMAType == fastMAType && cacheninZaSuperiorMACD[idx].FastPeriod == fastPeriod && cacheninZaSuperiorMACD[idx].FastSmoothingEnabled == fastSmoothingEnabled && cacheninZaSuperiorMACD[idx].FastSmoothingMethod == fastSmoothingMethod && cacheninZaSuperiorMACD[idx].FastSmoothingPeriod == fastSmoothingPeriod && cacheninZaSuperiorMACD[idx].SlowMAType == slowMAType && cacheninZaSuperiorMACD[idx].SlowPeriod == slowPeriod && cacheninZaSuperiorMACD[idx].SlowSmoothingEnabled == slowSmoothingEnabled && cacheninZaSuperiorMACD[idx].SlowSmoothingMethod == slowSmoothingMethod && cacheninZaSuperiorMACD[idx].SlowSmoothingPeriod == slowSmoothingPeriod && cacheninZaSuperiorMACD[idx].SignalMAType == signalMAType && cacheninZaSuperiorMACD[idx].SignalPeriod == signalPeriod && cacheninZaSuperiorMACD[idx].SignalSmoothingEnabled == signalSmoothingEnabled && cacheninZaSuperiorMACD[idx].SignalSmoothingMethod == signalSmoothingMethod && cacheninZaSuperiorMACD[idx].SignalSmoothingPeriod == signalSmoothingPeriod && cacheninZaSuperiorMACD[idx].SlowdownScan == slowdownScan && cacheninZaSuperiorMACD[idx].ThresholdMultipler == thresholdMultipler && cacheninZaSuperiorMACD[idx].ThresholdATRPeriod == thresholdATRPeriod && cacheninZaSuperiorMACD[idx].ThresholdMinimum == thresholdMinimum && cacheninZaSuperiorMACD[idx].EqualsInput(input))
						return cacheninZaSuperiorMACD[idx];
			return CacheIndicator<ninZaSuperiorMACD>(new ninZaSuperiorMACD(){ FastMAType = fastMAType, FastPeriod = fastPeriod, FastSmoothingEnabled = fastSmoothingEnabled, FastSmoothingMethod = fastSmoothingMethod, FastSmoothingPeriod = fastSmoothingPeriod, SlowMAType = slowMAType, SlowPeriod = slowPeriod, SlowSmoothingEnabled = slowSmoothingEnabled, SlowSmoothingMethod = slowSmoothingMethod, SlowSmoothingPeriod = slowSmoothingPeriod, SignalMAType = signalMAType, SignalPeriod = signalPeriod, SignalSmoothingEnabled = signalSmoothingEnabled, SignalSmoothingMethod = signalSmoothingMethod, SignalSmoothingPeriod = signalSmoothingPeriod, SlowdownScan = slowdownScan, ThresholdMultipler = thresholdMultipler, ThresholdATRPeriod = thresholdATRPeriod, ThresholdMinimum = thresholdMinimum }, input, ref cacheninZaSuperiorMACD);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorMACD ninZaSuperiorMACD(ninZa_MAType fastMAType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowMAType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, ninZa_MAType signalMAType, int signalPeriod, bool signalSmoothingEnabled, ninZa_MAType signalSmoothingMethod, int signalSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaSuperiorMACD(Input, fastMAType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowMAType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, signalMAType, signalPeriod, signalSmoothingEnabled, signalSmoothingMethod, signalSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}


		
		public Indicators.ninZaSuperiorMACD ninZaSuperiorMACD(ISeries<double> input , ninZa_MAType fastMAType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowMAType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, ninZa_MAType signalMAType, int signalPeriod, bool signalSmoothingEnabled, ninZa_MAType signalSmoothingMethod, int signalSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaSuperiorMACD(input, fastMAType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowMAType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, signalMAType, signalPeriod, signalSmoothingEnabled, signalSmoothingMethod, signalSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorMACD ninZaSuperiorMACD(ninZa_MAType fastMAType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowMAType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, ninZa_MAType signalMAType, int signalPeriod, bool signalSmoothingEnabled, ninZa_MAType signalSmoothingMethod, int signalSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaSuperiorMACD(Input, fastMAType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowMAType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, signalMAType, signalPeriod, signalSmoothingEnabled, signalSmoothingMethod, signalSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}


		
		public Indicators.ninZaSuperiorMACD ninZaSuperiorMACD(ISeries<double> input , ninZa_MAType fastMAType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowMAType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, ninZa_MAType signalMAType, int signalPeriod, bool signalSmoothingEnabled, ninZa_MAType signalSmoothingMethod, int signalSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaSuperiorMACD(input, fastMAType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowMAType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, signalMAType, signalPeriod, signalSmoothingEnabled, signalSmoothingMethod, signalSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}

	}
}

#endregion
