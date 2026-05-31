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
		
		private ninZaSolarWind[] cacheninZaSolarWind;

		
		public ninZaSolarWind ninZaSolarWind(double offsetMultiplierTrend, double offsetMultiplierStop, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return ninZaSolarWind(Input, offsetMultiplierTrend, offsetMultiplierStop, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}


		
		public ninZaSolarWind ninZaSolarWind(ISeries<double> input, double offsetMultiplierTrend, double offsetMultiplierStop, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			if (cacheninZaSolarWind != null)
				for (int idx = 0; idx < cacheninZaSolarWind.Length; idx++)
					if (cacheninZaSolarWind[idx].OffsetMultiplierTrend == offsetMultiplierTrend && cacheninZaSolarWind[idx].OffsetMultiplierStop == offsetMultiplierStop && cacheninZaSolarWind[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaSolarWind[idx].ReferencePricePeriod == referencePricePeriod && cacheninZaSolarWind[idx].ReferencePriceCloseWeight == referencePriceCloseWeight && cacheninZaSolarWind[idx].SlowdownScan == slowdownScan && cacheninZaSolarWind[idx].WeakWeakSplit == weakWeakSplit && cacheninZaSolarWind[idx].PullbackEarly == pullbackEarly && cacheninZaSolarWind[idx].PullbackSplit == pullbackSplit && cacheninZaSolarWind[idx].EqualsInput(input))
						return cacheninZaSolarWind[idx];
			return CacheIndicator<ninZaSolarWind>(new ninZaSolarWind(){ OffsetMultiplierTrend = offsetMultiplierTrend, OffsetMultiplierStop = offsetMultiplierStop, OffsetATRPeriod = offsetATRPeriod, ReferencePricePeriod = referencePricePeriod, ReferencePriceCloseWeight = referencePriceCloseWeight, SlowdownScan = slowdownScan, WeakWeakSplit = weakWeakSplit, PullbackEarly = pullbackEarly, PullbackSplit = pullbackSplit }, input, ref cacheninZaSolarWind);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSolarWind ninZaSolarWind(double offsetMultiplierTrend, double offsetMultiplierStop, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.ninZaSolarWind(Input, offsetMultiplierTrend, offsetMultiplierStop, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}


		
		public Indicators.ninZaSolarWind ninZaSolarWind(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierStop, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.ninZaSolarWind(input, offsetMultiplierTrend, offsetMultiplierStop, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSolarWind ninZaSolarWind(double offsetMultiplierTrend, double offsetMultiplierStop, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.ninZaSolarWind(Input, offsetMultiplierTrend, offsetMultiplierStop, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}


		
		public Indicators.ninZaSolarWind ninZaSolarWind(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierStop, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.ninZaSolarWind(input, offsetMultiplierTrend, offsetMultiplierStop, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}

	}
}

#endregion
