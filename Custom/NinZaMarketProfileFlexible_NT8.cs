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
		
		private ninZaMarketProfileFlexible[] cacheninZaMarketProfileFlexible;

		
		public ninZaMarketProfileFlexible ninZaMarketProfileFlexible()
		{
			return ninZaMarketProfileFlexible(Input);
		}


		
		public ninZaMarketProfileFlexible ninZaMarketProfileFlexible(ISeries<double> input)
		{
			if (cacheninZaMarketProfileFlexible != null)
				for (int idx = 0; idx < cacheninZaMarketProfileFlexible.Length; idx++)
					if ( cacheninZaMarketProfileFlexible[idx].EqualsInput(input))
						return cacheninZaMarketProfileFlexible[idx];
			return CacheIndicator<ninZaMarketProfileFlexible>(new ninZaMarketProfileFlexible(), input, ref cacheninZaMarketProfileFlexible);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMarketProfileFlexible ninZaMarketProfileFlexible()
		{
			return indicator.ninZaMarketProfileFlexible(Input);
		}


		
		public Indicators.ninZaMarketProfileFlexible ninZaMarketProfileFlexible(ISeries<double> input )
		{
			return indicator.ninZaMarketProfileFlexible(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMarketProfileFlexible ninZaMarketProfileFlexible()
		{
			return indicator.ninZaMarketProfileFlexible(Input);
		}


		
		public Indicators.ninZaMarketProfileFlexible ninZaMarketProfileFlexible(ISeries<double> input )
		{
			return indicator.ninZaMarketProfileFlexible(input);
		}

	}
}

#endregion
