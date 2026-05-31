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
		
		private ninZaTotalAlert[] cacheninZaTotalAlert;

		
		public ninZaTotalAlert ninZaTotalAlert()
		{
			return ninZaTotalAlert(Input);
		}


		
		public ninZaTotalAlert ninZaTotalAlert(ISeries<double> input)
		{
			if (cacheninZaTotalAlert != null)
				for (int idx = 0; idx < cacheninZaTotalAlert.Length; idx++)
					if ( cacheninZaTotalAlert[idx].EqualsInput(input))
						return cacheninZaTotalAlert[idx];
			return CacheIndicator<ninZaTotalAlert>(new ninZaTotalAlert(), input, ref cacheninZaTotalAlert);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaTotalAlert ninZaTotalAlert()
		{
			return indicator.ninZaTotalAlert(Input);
		}


		
		public Indicators.ninZaTotalAlert ninZaTotalAlert(ISeries<double> input )
		{
			return indicator.ninZaTotalAlert(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaTotalAlert ninZaTotalAlert()
		{
			return indicator.ninZaTotalAlert(Input);
		}


		
		public Indicators.ninZaTotalAlert ninZaTotalAlert(ISeries<double> input )
		{
			return indicator.ninZaTotalAlert(input);
		}

	}
}

#endregion
