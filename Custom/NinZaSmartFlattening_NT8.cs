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
		
		private ninZaSmartFlattening[] cacheninZaSmartFlattening;

		
		public ninZaSmartFlattening ninZaSmartFlattening()
		{
			return ninZaSmartFlattening(Input);
		}


		
		public ninZaSmartFlattening ninZaSmartFlattening(ISeries<double> input)
		{
			if (cacheninZaSmartFlattening != null)
				for (int idx = 0; idx < cacheninZaSmartFlattening.Length; idx++)
					if ( cacheninZaSmartFlattening[idx].EqualsInput(input))
						return cacheninZaSmartFlattening[idx];
			return CacheIndicator<ninZaSmartFlattening>(new ninZaSmartFlattening(), input, ref cacheninZaSmartFlattening);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSmartFlattening ninZaSmartFlattening()
		{
			return indicator.ninZaSmartFlattening(Input);
		}


		
		public Indicators.ninZaSmartFlattening ninZaSmartFlattening(ISeries<double> input )
		{
			return indicator.ninZaSmartFlattening(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSmartFlattening ninZaSmartFlattening()
		{
			return indicator.ninZaSmartFlattening(Input);
		}


		
		public Indicators.ninZaSmartFlattening ninZaSmartFlattening(ISeries<double> input )
		{
			return indicator.ninZaSmartFlattening(input);
		}

	}
}

#endregion
