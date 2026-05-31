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
		
		private ninZaOrderFlowPresentation[] cacheninZaOrderFlowPresentation;

		
		public ninZaOrderFlowPresentation ninZaOrderFlowPresentation()
		{
			return ninZaOrderFlowPresentation(Input);
		}


		
		public ninZaOrderFlowPresentation ninZaOrderFlowPresentation(ISeries<double> input)
		{
			if (cacheninZaOrderFlowPresentation != null)
				for (int idx = 0; idx < cacheninZaOrderFlowPresentation.Length; idx++)
					if ( cacheninZaOrderFlowPresentation[idx].EqualsInput(input))
						return cacheninZaOrderFlowPresentation[idx];
			return CacheIndicator<ninZaOrderFlowPresentation>(new ninZaOrderFlowPresentation(), input, ref cacheninZaOrderFlowPresentation);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaOrderFlowPresentation ninZaOrderFlowPresentation()
		{
			return indicator.ninZaOrderFlowPresentation(Input);
		}


		
		public Indicators.ninZaOrderFlowPresentation ninZaOrderFlowPresentation(ISeries<double> input )
		{
			return indicator.ninZaOrderFlowPresentation(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaOrderFlowPresentation ninZaOrderFlowPresentation()
		{
			return indicator.ninZaOrderFlowPresentation(Input);
		}


		
		public Indicators.ninZaOrderFlowPresentation ninZaOrderFlowPresentation(ISeries<double> input )
		{
			return indicator.ninZaOrderFlowPresentation(input);
		}

	}
}

#endregion
