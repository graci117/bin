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
		
		private ninZaEOBExit[] cacheninZaEOBExit;

		
		public ninZaEOBExit ninZaEOBExit()
		{
			return ninZaEOBExit(Input);
		}


		
		public ninZaEOBExit ninZaEOBExit(ISeries<double> input)
		{
			if (cacheninZaEOBExit != null)
				for (int idx = 0; idx < cacheninZaEOBExit.Length; idx++)
					if ( cacheninZaEOBExit[idx].EqualsInput(input))
						return cacheninZaEOBExit[idx];
			return CacheIndicator<ninZaEOBExit>(new ninZaEOBExit(), input, ref cacheninZaEOBExit);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaEOBExit ninZaEOBExit()
		{
			return indicator.ninZaEOBExit(Input);
		}


		
		public Indicators.ninZaEOBExit ninZaEOBExit(ISeries<double> input )
		{
			return indicator.ninZaEOBExit(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaEOBExit ninZaEOBExit()
		{
			return indicator.ninZaEOBExit(Input);
		}


		
		public Indicators.ninZaEOBExit ninZaEOBExit(ISeries<double> input )
		{
			return indicator.ninZaEOBExit(input);
		}

	}
}

#endregion
