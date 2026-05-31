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
		
		private ninZaEOBOrdering[] cacheninZaEOBOrdering;

		
		public ninZaEOBOrdering ninZaEOBOrdering()
		{
			return ninZaEOBOrdering(Input);
		}


		
		public ninZaEOBOrdering ninZaEOBOrdering(ISeries<double> input)
		{
			if (cacheninZaEOBOrdering != null)
				for (int idx = 0; idx < cacheninZaEOBOrdering.Length; idx++)
					if ( cacheninZaEOBOrdering[idx].EqualsInput(input))
						return cacheninZaEOBOrdering[idx];
			return CacheIndicator<ninZaEOBOrdering>(new ninZaEOBOrdering(), input, ref cacheninZaEOBOrdering);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaEOBOrdering ninZaEOBOrdering()
		{
			return indicator.ninZaEOBOrdering(Input);
		}


		
		public Indicators.ninZaEOBOrdering ninZaEOBOrdering(ISeries<double> input )
		{
			return indicator.ninZaEOBOrdering(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaEOBOrdering ninZaEOBOrdering()
		{
			return indicator.ninZaEOBOrdering(Input);
		}


		
		public Indicators.ninZaEOBOrdering ninZaEOBOrdering(ISeries<double> input )
		{
			return indicator.ninZaEOBOrdering(input);
		}

	}
}

#endregion
