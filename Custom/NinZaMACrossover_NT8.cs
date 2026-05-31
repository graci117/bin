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
		
		private ninZaMACrossover[] cacheninZaMACrossover;

		
		public ninZaMACrossover ninZaMACrossover(ninZa_MAType fastType, int fastPeriod, ninZaMACrossover_PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, ninZaMACrossover_PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int crossoverOffset, bool roundToTickSizeEnabled)
		{
			return ninZaMACrossover(Input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, crossoverOffset, roundToTickSizeEnabled);
		}


		
		public ninZaMACrossover ninZaMACrossover(ISeries<double> input, ninZa_MAType fastType, int fastPeriod, ninZaMACrossover_PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, ninZaMACrossover_PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int crossoverOffset, bool roundToTickSizeEnabled)
		{
			if (cacheninZaMACrossover != null)
				for (int idx = 0; idx < cacheninZaMACrossover.Length; idx++)
					if (cacheninZaMACrossover[idx].FastType == fastType && cacheninZaMACrossover[idx].FastPeriod == fastPeriod && cacheninZaMACrossover[idx].FastInput == fastInput && cacheninZaMACrossover[idx].FastSmoothingEnabled == fastSmoothingEnabled && cacheninZaMACrossover[idx].FastSmoothingMethod == fastSmoothingMethod && cacheninZaMACrossover[idx].FastSmoothingPeriod == fastSmoothingPeriod && cacheninZaMACrossover[idx].SlowType == slowType && cacheninZaMACrossover[idx].SlowPeriod == slowPeriod && cacheninZaMACrossover[idx].SlowInput == slowInput && cacheninZaMACrossover[idx].SlowSmoothingEnabled == slowSmoothingEnabled && cacheninZaMACrossover[idx].SlowSmoothingMethod == slowSmoothingMethod && cacheninZaMACrossover[idx].SlowSmoothingPeriod == slowSmoothingPeriod && cacheninZaMACrossover[idx].CrossoverOffset == crossoverOffset && cacheninZaMACrossover[idx].RoundToTickSizeEnabled == roundToTickSizeEnabled && cacheninZaMACrossover[idx].EqualsInput(input))
						return cacheninZaMACrossover[idx];
			return CacheIndicator<ninZaMACrossover>(new ninZaMACrossover(){ FastType = fastType, FastPeriod = fastPeriod, FastInput = fastInput, FastSmoothingEnabled = fastSmoothingEnabled, FastSmoothingMethod = fastSmoothingMethod, FastSmoothingPeriod = fastSmoothingPeriod, SlowType = slowType, SlowPeriod = slowPeriod, SlowInput = slowInput, SlowSmoothingEnabled = slowSmoothingEnabled, SlowSmoothingMethod = slowSmoothingMethod, SlowSmoothingPeriod = slowSmoothingPeriod, CrossoverOffset = crossoverOffset, RoundToTickSizeEnabled = roundToTickSizeEnabled }, input, ref cacheninZaMACrossover);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMACrossover ninZaMACrossover(ninZa_MAType fastType, int fastPeriod, ninZaMACrossover_PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, ninZaMACrossover_PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int crossoverOffset, bool roundToTickSizeEnabled)
		{
			return indicator.ninZaMACrossover(Input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, crossoverOffset, roundToTickSizeEnabled);
		}


		
		public Indicators.ninZaMACrossover ninZaMACrossover(ISeries<double> input , ninZa_MAType fastType, int fastPeriod, ninZaMACrossover_PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, ninZaMACrossover_PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int crossoverOffset, bool roundToTickSizeEnabled)
		{
			return indicator.ninZaMACrossover(input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, crossoverOffset, roundToTickSizeEnabled);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMACrossover ninZaMACrossover(ninZa_MAType fastType, int fastPeriod, ninZaMACrossover_PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, ninZaMACrossover_PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int crossoverOffset, bool roundToTickSizeEnabled)
		{
			return indicator.ninZaMACrossover(Input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, crossoverOffset, roundToTickSizeEnabled);
		}


		
		public Indicators.ninZaMACrossover ninZaMACrossover(ISeries<double> input , ninZa_MAType fastType, int fastPeriod, ninZaMACrossover_PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, ninZaMACrossover_PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int crossoverOffset, bool roundToTickSizeEnabled)
		{
			return indicator.ninZaMACrossover(input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, crossoverOffset, roundToTickSizeEnabled);
		}

	}
}

#endregion
