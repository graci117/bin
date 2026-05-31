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
		
		private ninZaSmartRuler[] cacheninZaSmartRuler;

		
		public ninZaSmartRuler ninZaSmartRuler()
		{
			return ninZaSmartRuler(Input);
		}


		
		public ninZaSmartRuler ninZaSmartRuler(ISeries<double> input)
		{
			if (cacheninZaSmartRuler != null)
				for (int idx = 0; idx < cacheninZaSmartRuler.Length; idx++)
					if ( cacheninZaSmartRuler[idx].EqualsInput(input))
						return cacheninZaSmartRuler[idx];
			return CacheIndicator<ninZaSmartRuler>(new ninZaSmartRuler(), input, ref cacheninZaSmartRuler);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSmartRuler ninZaSmartRuler()
		{
			return indicator.ninZaSmartRuler(Input);
		}


		
		public Indicators.ninZaSmartRuler ninZaSmartRuler(ISeries<double> input )
		{
			return indicator.ninZaSmartRuler(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSmartRuler ninZaSmartRuler()
		{
			return indicator.ninZaSmartRuler(Input);
		}


		
		public Indicators.ninZaSmartRuler ninZaSmartRuler(ISeries<double> input )
		{
			return indicator.ninZaSmartRuler(input);
		}

	}
}

#endregion
