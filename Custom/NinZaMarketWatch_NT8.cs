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
		
		private ninZaMarketWatch[] cacheninZaMarketWatch;

		
		public ninZaMarketWatch ninZaMarketWatch()
		{
			return ninZaMarketWatch(Input);
		}


		
		public ninZaMarketWatch ninZaMarketWatch(ISeries<double> input)
		{
			if (cacheninZaMarketWatch != null)
				for (int idx = 0; idx < cacheninZaMarketWatch.Length; idx++)
					if ( cacheninZaMarketWatch[idx].EqualsInput(input))
						return cacheninZaMarketWatch[idx];
			return CacheIndicator<ninZaMarketWatch>(new ninZaMarketWatch(), input, ref cacheninZaMarketWatch);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMarketWatch ninZaMarketWatch()
		{
			return indicator.ninZaMarketWatch(Input);
		}


		
		public Indicators.ninZaMarketWatch ninZaMarketWatch(ISeries<double> input )
		{
			return indicator.ninZaMarketWatch(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMarketWatch ninZaMarketWatch()
		{
			return indicator.ninZaMarketWatch(Input);
		}


		
		public Indicators.ninZaMarketWatch ninZaMarketWatch(ISeries<double> input )
		{
			return indicator.ninZaMarketWatch(input);
		}

	}
}

#endregion
