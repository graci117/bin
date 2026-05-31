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
		
		private ninZaSmartZoom[] cacheninZaSmartZoom;

		
		public ninZaSmartZoom ninZaSmartZoom()
		{
			return ninZaSmartZoom(Input);
		}


		
		public ninZaSmartZoom ninZaSmartZoom(ISeries<double> input)
		{
			if (cacheninZaSmartZoom != null)
				for (int idx = 0; idx < cacheninZaSmartZoom.Length; idx++)
					if ( cacheninZaSmartZoom[idx].EqualsInput(input))
						return cacheninZaSmartZoom[idx];
			return CacheIndicator<ninZaSmartZoom>(new ninZaSmartZoom(), input, ref cacheninZaSmartZoom);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSmartZoom ninZaSmartZoom()
		{
			return indicator.ninZaSmartZoom(Input);
		}


		
		public Indicators.ninZaSmartZoom ninZaSmartZoom(ISeries<double> input )
		{
			return indicator.ninZaSmartZoom(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSmartZoom ninZaSmartZoom()
		{
			return indicator.ninZaSmartZoom(Input);
		}


		
		public Indicators.ninZaSmartZoom ninZaSmartZoom(ISeries<double> input )
		{
			return indicator.ninZaSmartZoom(input);
		}

	}
}

#endregion
