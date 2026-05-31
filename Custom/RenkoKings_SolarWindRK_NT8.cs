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
		
		private RenkoKings.RenkoKings_SolarWindRK[] cacheRenkoKings_SolarWindRK;

		
		public RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK(double offsetMultiplierTrend, double offsetMultiplierStop, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return RenkoKings_SolarWindRK(Input, offsetMultiplierTrend, offsetMultiplierStop, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}


		
		public RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK(ISeries<double> input, double offsetMultiplierTrend, double offsetMultiplierStop, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			if (cacheRenkoKings_SolarWindRK != null)
				for (int idx = 0; idx < cacheRenkoKings_SolarWindRK.Length; idx++)
					if (cacheRenkoKings_SolarWindRK[idx].OffsetMultiplierTrend == offsetMultiplierTrend && cacheRenkoKings_SolarWindRK[idx].OffsetMultiplierStop == offsetMultiplierStop && cacheRenkoKings_SolarWindRK[idx].SlowdownScan == slowdownScan && cacheRenkoKings_SolarWindRK[idx].WeakWeakSplit == weakWeakSplit && cacheRenkoKings_SolarWindRK[idx].PullbackEarly == pullbackEarly && cacheRenkoKings_SolarWindRK[idx].PullbackSplit == pullbackSplit && cacheRenkoKings_SolarWindRK[idx].EqualsInput(input))
						return cacheRenkoKings_SolarWindRK[idx];
			return CacheIndicator<RenkoKings.RenkoKings_SolarWindRK>(new RenkoKings.RenkoKings_SolarWindRK(){ OffsetMultiplierTrend = offsetMultiplierTrend, OffsetMultiplierStop = offsetMultiplierStop, SlowdownScan = slowdownScan, WeakWeakSplit = weakWeakSplit, PullbackEarly = pullbackEarly, PullbackSplit = pullbackSplit }, input, ref cacheRenkoKings_SolarWindRK);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK(double offsetMultiplierTrend, double offsetMultiplierStop, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.RenkoKings_SolarWindRK(Input, offsetMultiplierTrend, offsetMultiplierStop, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}


		
		public Indicators.RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierStop, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.RenkoKings_SolarWindRK(input, offsetMultiplierTrend, offsetMultiplierStop, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK(double offsetMultiplierTrend, double offsetMultiplierStop, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.RenkoKings_SolarWindRK(Input, offsetMultiplierTrend, offsetMultiplierStop, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}


		
		public Indicators.RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierStop, int slowdownScan, int weakWeakSplit, bool pullbackEarly, int pullbackSplit)
		{
			return indicator.RenkoKings_SolarWindRK(input, offsetMultiplierTrend, offsetMultiplierStop, slowdownScan, weakWeakSplit, pullbackEarly, pullbackSplit);
		}

	}
}

#endregion
