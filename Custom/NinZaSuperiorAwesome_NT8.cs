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
		
		private ninZaSuperiorAwesome[] cacheninZaSuperiorAwesome;

		
		public ninZaSuperiorAwesome ninZaSuperiorAwesome(ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int neighborhood)
		{
			return ninZaSuperiorAwesome(Input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, neighborhood);
		}


		
		public ninZaSuperiorAwesome ninZaSuperiorAwesome(ISeries<double> input, ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int neighborhood)
		{
			if (cacheninZaSuperiorAwesome != null)
				for (int idx = 0; idx < cacheninZaSuperiorAwesome.Length; idx++)
					if (cacheninZaSuperiorAwesome[idx].FastType == fastType && cacheninZaSuperiorAwesome[idx].FastPeriod == fastPeriod && cacheninZaSuperiorAwesome[idx].FastInput == fastInput && cacheninZaSuperiorAwesome[idx].FastSmoothingEnabled == fastSmoothingEnabled && cacheninZaSuperiorAwesome[idx].FastSmoothingMethod == fastSmoothingMethod && cacheninZaSuperiorAwesome[idx].FastSmoothingPeriod == fastSmoothingPeriod && cacheninZaSuperiorAwesome[idx].SlowType == slowType && cacheninZaSuperiorAwesome[idx].SlowPeriod == slowPeriod && cacheninZaSuperiorAwesome[idx].SlowInput == slowInput && cacheninZaSuperiorAwesome[idx].SlowSmoothingEnabled == slowSmoothingEnabled && cacheninZaSuperiorAwesome[idx].SlowSmoothingMethod == slowSmoothingMethod && cacheninZaSuperiorAwesome[idx].SlowSmoothingPeriod == slowSmoothingPeriod && cacheninZaSuperiorAwesome[idx].Neighborhood == neighborhood && cacheninZaSuperiorAwesome[idx].EqualsInput(input))
						return cacheninZaSuperiorAwesome[idx];
			return CacheIndicator<ninZaSuperiorAwesome>(new ninZaSuperiorAwesome(){ FastType = fastType, FastPeriod = fastPeriod, FastInput = fastInput, FastSmoothingEnabled = fastSmoothingEnabled, FastSmoothingMethod = fastSmoothingMethod, FastSmoothingPeriod = fastSmoothingPeriod, SlowType = slowType, SlowPeriod = slowPeriod, SlowInput = slowInput, SlowSmoothingEnabled = slowSmoothingEnabled, SlowSmoothingMethod = slowSmoothingMethod, SlowSmoothingPeriod = slowSmoothingPeriod, Neighborhood = neighborhood }, input, ref cacheninZaSuperiorAwesome);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorAwesome ninZaSuperiorAwesome(ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int neighborhood)
		{
			return indicator.ninZaSuperiorAwesome(Input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, neighborhood);
		}


		
		public Indicators.ninZaSuperiorAwesome ninZaSuperiorAwesome(ISeries<double> input , ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int neighborhood)
		{
			return indicator.ninZaSuperiorAwesome(input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, neighborhood);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorAwesome ninZaSuperiorAwesome(ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int neighborhood)
		{
			return indicator.ninZaSuperiorAwesome(Input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, neighborhood);
		}


		
		public Indicators.ninZaSuperiorAwesome ninZaSuperiorAwesome(ISeries<double> input , ninZa_MAType fastType, int fastPeriod, PriceType fastInput, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, PriceType slowInput, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, int neighborhood)
		{
			return indicator.ninZaSuperiorAwesome(input, fastType, fastPeriod, fastInput, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, slowType, slowPeriod, slowInput, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, neighborhood);
		}

	}
}

#endregion
