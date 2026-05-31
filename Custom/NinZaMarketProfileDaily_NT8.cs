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
		
		private ninZaMarketProfileDaily[] cacheninZaMarketProfileDaily;

		
		public ninZaMarketProfileDaily ninZaMarketProfileDaily()
		{
			return ninZaMarketProfileDaily(Input);
		}


		
		public ninZaMarketProfileDaily ninZaMarketProfileDaily(ISeries<double> input)
		{
			if (cacheninZaMarketProfileDaily != null)
				for (int idx = 0; idx < cacheninZaMarketProfileDaily.Length; idx++)
					if ( cacheninZaMarketProfileDaily[idx].EqualsInput(input))
						return cacheninZaMarketProfileDaily[idx];
			return CacheIndicator<ninZaMarketProfileDaily>(new ninZaMarketProfileDaily(), input, ref cacheninZaMarketProfileDaily);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMarketProfileDaily ninZaMarketProfileDaily()
		{
			return indicator.ninZaMarketProfileDaily(Input);
		}


		
		public Indicators.ninZaMarketProfileDaily ninZaMarketProfileDaily(ISeries<double> input )
		{
			return indicator.ninZaMarketProfileDaily(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMarketProfileDaily ninZaMarketProfileDaily()
		{
			return indicator.ninZaMarketProfileDaily(Input);
		}


		
		public Indicators.ninZaMarketProfileDaily ninZaMarketProfileDaily(ISeries<double> input )
		{
			return indicator.ninZaMarketProfileDaily(input);
		}

	}
}

#endregion
