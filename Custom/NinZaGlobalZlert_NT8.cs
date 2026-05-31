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
		
		private ninZaGlobalZlert[] cacheninZaGlobalZlert;

		
		public ninZaGlobalZlert ninZaGlobalZlert()
		{
			return ninZaGlobalZlert(Input);
		}


		
		public ninZaGlobalZlert ninZaGlobalZlert(ISeries<double> input)
		{
			if (cacheninZaGlobalZlert != null)
				for (int idx = 0; idx < cacheninZaGlobalZlert.Length; idx++)
					if ( cacheninZaGlobalZlert[idx].EqualsInput(input))
						return cacheninZaGlobalZlert[idx];
			return CacheIndicator<ninZaGlobalZlert>(new ninZaGlobalZlert(), input, ref cacheninZaGlobalZlert);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaGlobalZlert ninZaGlobalZlert()
		{
			return indicator.ninZaGlobalZlert(Input);
		}


		
		public Indicators.ninZaGlobalZlert ninZaGlobalZlert(ISeries<double> input )
		{
			return indicator.ninZaGlobalZlert(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaGlobalZlert ninZaGlobalZlert()
		{
			return indicator.ninZaGlobalZlert(Input);
		}


		
		public Indicators.ninZaGlobalZlert ninZaGlobalZlert(ISeries<double> input )
		{
			return indicator.ninZaGlobalZlert(input);
		}

	}
}

#endregion
