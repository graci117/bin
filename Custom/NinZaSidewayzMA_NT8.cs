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
		
		private ninZaSidewayzMA[] cacheninZaSidewayzMA;

		
		public ninZaSidewayzMA ninZaSidewayzMA(int monitoredCrossovers, double multiplierMaxCrossoverDeviation, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod)
		{
			return ninZaSidewayzMA(Input, monitoredCrossovers, multiplierMaxCrossoverDeviation, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod);
		}


		
		public ninZaSidewayzMA ninZaSidewayzMA(ISeries<double> input, int monitoredCrossovers, double multiplierMaxCrossoverDeviation, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod)
		{
			if (cacheninZaSidewayzMA != null)
				for (int idx = 0; idx < cacheninZaSidewayzMA.Length; idx++)
					if (cacheninZaSidewayzMA[idx].MonitoredCrossovers == monitoredCrossovers && cacheninZaSidewayzMA[idx].MultiplierMaxCrossoverDeviation == multiplierMaxCrossoverDeviation && cacheninZaSidewayzMA[idx].MultiplierMaxBaseRange == multiplierMaxBaseRange && cacheninZaSidewayzMA[idx].MultiplierBoundOffset == multiplierBoundOffset && cacheninZaSidewayzMA[idx].ATRPeriod == aTRPeriod && cacheninZaSidewayzMA[idx].FastType == fastType && cacheninZaSidewayzMA[idx].FastPeriod == fastPeriod && cacheninZaSidewayzMA[idx].FastInput == fastInput && cacheninZaSidewayzMA[idx].FastSmoothingEnabled == fastSmoothingEnabled && cacheninZaSidewayzMA[idx].FastSmoothingMethod == fastSmoothingMethod && cacheninZaSidewayzMA[idx].FastSmoothingPeriod == fastSmoothingPeriod && cacheninZaSidewayzMA[idx].SlowType == slowType && cacheninZaSidewayzMA[idx].SlowPeriod == slowPeriod && cacheninZaSidewayzMA[idx].SlowInput == slowInput && cacheninZaSidewayzMA[idx].SlowSmoothingEnabled == slowSmoothingEnabled && cacheninZaSidewayzMA[idx].SlowSmoothingMethod == slowSmoothingMethod && cacheninZaSidewayzMA[idx].SlowSmoothingPeriod == slowSmoothingPeriod && cacheninZaSidewayzMA[idx].EqualsInput(input))
						return cacheninZaSidewayzMA[idx];
			return CacheIndicator<ninZaSidewayzMA>(new ninZaSidewayzMA(){ MonitoredCrossovers = monitoredCrossovers, MultiplierMaxCrossoverDeviation = multiplierMaxCrossoverDeviation, MultiplierMaxBaseRange = multiplierMaxBaseRange, MultiplierBoundOffset = multiplierBoundOffset, ATRPeriod = aTRPeriod, FastType = fastType, FastPeriod = fastPeriod, FastInput = fastInput, FastSmoothingEnabled = fastSmoothingEnabled, FastSmoothingMethod = fastSmoothingMethod, FastSmoothingPeriod = fastSmoothingPeriod, SlowType = slowType, SlowPeriod = slowPeriod, SlowInput = slowInput, SlowSmoothingEnabled = slowSmoothingEnabled, SlowSmoothingMethod = slowSmoothingMethod, SlowSmoothingPeriod = slowSmoothingPeriod }, input, ref cacheninZaSidewayzMA);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSidewayzMA ninZaSidewayzMA(int monitoredCrossovers, double multiplierMaxCrossoverDeviation, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod)
		{
			return indicator.ninZaSidewayzMA(Input, monitoredCrossovers, multiplierMaxCrossoverDeviation, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod);
		}


		
		public Indicators.ninZaSidewayzMA ninZaSidewayzMA(ISeries<double> input , int monitoredCrossovers, double multiplierMaxCrossoverDeviation, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod)
		{
			return indicator.ninZaSidewayzMA(input, monitoredCrossovers, multiplierMaxCrossoverDeviation, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSidewayzMA ninZaSidewayzMA(int monitoredCrossovers, double multiplierMaxCrossoverDeviation, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod)
		{
			return indicator.ninZaSidewayzMA(Input, monitoredCrossovers, multiplierMaxCrossoverDeviation, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod);
		}


		
		public Indicators.ninZaSidewayzMA ninZaSidewayzMA(ISeries<double> input , int monitoredCrossovers, double multiplierMaxCrossoverDeviation, double multiplierMaxBaseRange, double multiplierBoundOffset, int aTRPeriod, ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod)
		{
			return indicator.ninZaSidewayzMA(input, monitoredCrossovers, multiplierMaxCrossoverDeviation, multiplierMaxBaseRange, multiplierBoundOffset, aTRPeriod, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod);
		}

	}
}

#endregion
