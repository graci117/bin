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
		
		private ninZaInnovativeOrdering[] cacheninZaInnovativeOrdering;

		
		public ninZaInnovativeOrdering ninZaInnovativeOrdering()
		{
			return ninZaInnovativeOrdering(Input);
		}


		
		public ninZaInnovativeOrdering ninZaInnovativeOrdering(ISeries<double> input)
		{
			if (cacheninZaInnovativeOrdering != null)
				for (int idx = 0; idx < cacheninZaInnovativeOrdering.Length; idx++)
					if ( cacheninZaInnovativeOrdering[idx].EqualsInput(input))
						return cacheninZaInnovativeOrdering[idx];
			return CacheIndicator<ninZaInnovativeOrdering>(new ninZaInnovativeOrdering(), input, ref cacheninZaInnovativeOrdering);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaInnovativeOrdering ninZaInnovativeOrdering()
		{
			return indicator.ninZaInnovativeOrdering(Input);
		}


		
		public Indicators.ninZaInnovativeOrdering ninZaInnovativeOrdering(ISeries<double> input )
		{
			return indicator.ninZaInnovativeOrdering(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaInnovativeOrdering ninZaInnovativeOrdering()
		{
			return indicator.ninZaInnovativeOrdering(Input);
		}


		
		public Indicators.ninZaInnovativeOrdering ninZaInnovativeOrdering(ISeries<double> input )
		{
			return indicator.ninZaInnovativeOrdering(input);
		}

	}
}

#endregion
