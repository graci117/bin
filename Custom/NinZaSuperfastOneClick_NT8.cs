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
		
		private ninZaSuperfastOneClick[] cacheninZaSuperfastOneClick;

		
		public ninZaSuperfastOneClick ninZaSuperfastOneClick()
		{
			return ninZaSuperfastOneClick(Input);
		}


		
		public ninZaSuperfastOneClick ninZaSuperfastOneClick(ISeries<double> input)
		{
			if (cacheninZaSuperfastOneClick != null)
				for (int idx = 0; idx < cacheninZaSuperfastOneClick.Length; idx++)
					if ( cacheninZaSuperfastOneClick[idx].EqualsInput(input))
						return cacheninZaSuperfastOneClick[idx];
			return CacheIndicator<ninZaSuperfastOneClick>(new ninZaSuperfastOneClick(), input, ref cacheninZaSuperfastOneClick);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperfastOneClick ninZaSuperfastOneClick()
		{
			return indicator.ninZaSuperfastOneClick(Input);
		}


		
		public Indicators.ninZaSuperfastOneClick ninZaSuperfastOneClick(ISeries<double> input )
		{
			return indicator.ninZaSuperfastOneClick(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperfastOneClick ninZaSuperfastOneClick()
		{
			return indicator.ninZaSuperfastOneClick(Input);
		}


		
		public Indicators.ninZaSuperfastOneClick ninZaSuperfastOneClick(ISeries<double> input )
		{
			return indicator.ninZaSuperfastOneClick(input);
		}

	}
}

#endregion
