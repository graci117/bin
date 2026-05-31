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
		
		private A_Plus.APlusBPPanel[] cacheAPlusBPPanel;

		
		public A_Plus.APlusBPPanel APlusBPPanel()
		{
			return APlusBPPanel(Input);
		}


		
		public A_Plus.APlusBPPanel APlusBPPanel(ISeries<double> input)
		{
			if (cacheAPlusBPPanel != null)
				for (int idx = 0; idx < cacheAPlusBPPanel.Length; idx++)
					if ( cacheAPlusBPPanel[idx].EqualsInput(input))
						return cacheAPlusBPPanel[idx];
			return CacheIndicator<A_Plus.APlusBPPanel>(new A_Plus.APlusBPPanel(), input, ref cacheAPlusBPPanel);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.APlusBPPanel APlusBPPanel()
		{
			return indicator.APlusBPPanel(Input);
		}


		
		public Indicators.A_Plus.APlusBPPanel APlusBPPanel(ISeries<double> input )
		{
			return indicator.APlusBPPanel(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.APlusBPPanel APlusBPPanel()
		{
			return indicator.APlusBPPanel(Input);
		}


		
		public Indicators.A_Plus.APlusBPPanel APlusBPPanel(ISeries<double> input )
		{
			return indicator.APlusBPPanel(input);
		}

	}
}

#endregion
