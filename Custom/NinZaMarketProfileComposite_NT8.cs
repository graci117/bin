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
		
		private ninZaMarketProfileComposite[] cacheninZaMarketProfileComposite;

		
		public ninZaMarketProfileComposite ninZaMarketProfileComposite()
		{
			return ninZaMarketProfileComposite(Input);
		}


		
		public ninZaMarketProfileComposite ninZaMarketProfileComposite(ISeries<double> input)
		{
			if (cacheninZaMarketProfileComposite != null)
				for (int idx = 0; idx < cacheninZaMarketProfileComposite.Length; idx++)
					if ( cacheninZaMarketProfileComposite[idx].EqualsInput(input))
						return cacheninZaMarketProfileComposite[idx];
			return CacheIndicator<ninZaMarketProfileComposite>(new ninZaMarketProfileComposite(), input, ref cacheninZaMarketProfileComposite);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMarketProfileComposite ninZaMarketProfileComposite()
		{
			return indicator.ninZaMarketProfileComposite(Input);
		}


		
		public Indicators.ninZaMarketProfileComposite ninZaMarketProfileComposite(ISeries<double> input )
		{
			return indicator.ninZaMarketProfileComposite(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMarketProfileComposite ninZaMarketProfileComposite()
		{
			return indicator.ninZaMarketProfileComposite(Input);
		}


		
		public Indicators.ninZaMarketProfileComposite ninZaMarketProfileComposite(ISeries<double> input )
		{
			return indicator.ninZaMarketProfileComposite(input);
		}

	}
}

#endregion
