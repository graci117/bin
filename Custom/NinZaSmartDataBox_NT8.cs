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
		
		private ninZaSmartDataBox[] cacheninZaSmartDataBox;

		
		public ninZaSmartDataBox ninZaSmartDataBox()
		{
			return ninZaSmartDataBox(Input);
		}


		
		public ninZaSmartDataBox ninZaSmartDataBox(ISeries<double> input)
		{
			if (cacheninZaSmartDataBox != null)
				for (int idx = 0; idx < cacheninZaSmartDataBox.Length; idx++)
					if ( cacheninZaSmartDataBox[idx].EqualsInput(input))
						return cacheninZaSmartDataBox[idx];
			return CacheIndicator<ninZaSmartDataBox>(new ninZaSmartDataBox(), input, ref cacheninZaSmartDataBox);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSmartDataBox ninZaSmartDataBox()
		{
			return indicator.ninZaSmartDataBox(Input);
		}


		
		public Indicators.ninZaSmartDataBox ninZaSmartDataBox(ISeries<double> input )
		{
			return indicator.ninZaSmartDataBox(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSmartDataBox ninZaSmartDataBox()
		{
			return indicator.ninZaSmartDataBox(Input);
		}


		
		public Indicators.ninZaSmartDataBox ninZaSmartDataBox(ISeries<double> input )
		{
			return indicator.ninZaSmartDataBox(input);
		}

	}
}

#endregion
