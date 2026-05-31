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
		
		private ninZaSmartOCOAssistant[] cacheninZaSmartOCOAssistant;

		
		public ninZaSmartOCOAssistant ninZaSmartOCOAssistant()
		{
			return ninZaSmartOCOAssistant(Input);
		}


		
		public ninZaSmartOCOAssistant ninZaSmartOCOAssistant(ISeries<double> input)
		{
			if (cacheninZaSmartOCOAssistant != null)
				for (int idx = 0; idx < cacheninZaSmartOCOAssistant.Length; idx++)
					if ( cacheninZaSmartOCOAssistant[idx].EqualsInput(input))
						return cacheninZaSmartOCOAssistant[idx];
			return CacheIndicator<ninZaSmartOCOAssistant>(new ninZaSmartOCOAssistant(), input, ref cacheninZaSmartOCOAssistant);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSmartOCOAssistant ninZaSmartOCOAssistant()
		{
			return indicator.ninZaSmartOCOAssistant(Input);
		}


		
		public Indicators.ninZaSmartOCOAssistant ninZaSmartOCOAssistant(ISeries<double> input )
		{
			return indicator.ninZaSmartOCOAssistant(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSmartOCOAssistant ninZaSmartOCOAssistant()
		{
			return indicator.ninZaSmartOCOAssistant(Input);
		}


		
		public Indicators.ninZaSmartOCOAssistant ninZaSmartOCOAssistant(ISeries<double> input )
		{
			return indicator.ninZaSmartOCOAssistant(input);
		}

	}
}

#endregion
