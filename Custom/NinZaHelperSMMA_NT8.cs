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
		
		private ninZaHelperSMMA[] cacheninZaHelperSMMA;

		
		public ninZaHelperSMMA ninZaHelperSMMA(int period)
		{
			return ninZaHelperSMMA(Input, period);
		}


		
		public ninZaHelperSMMA ninZaHelperSMMA(ISeries<double> input, int period)
		{
			if (cacheninZaHelperSMMA != null)
				for (int idx = 0; idx < cacheninZaHelperSMMA.Length; idx++)
					if (cacheninZaHelperSMMA[idx].Period == period && cacheninZaHelperSMMA[idx].EqualsInput(input))
						return cacheninZaHelperSMMA[idx];
			return CacheIndicator<ninZaHelperSMMA>(new ninZaHelperSMMA(){ Period = period }, input, ref cacheninZaHelperSMMA);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHelperSMMA ninZaHelperSMMA(int period)
		{
			return indicator.ninZaHelperSMMA(Input, period);
		}


		
		public Indicators.ninZaHelperSMMA ninZaHelperSMMA(ISeries<double> input , int period)
		{
			return indicator.ninZaHelperSMMA(input, period);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHelperSMMA ninZaHelperSMMA(int period)
		{
			return indicator.ninZaHelperSMMA(Input, period);
		}


		
		public Indicators.ninZaHelperSMMA ninZaHelperSMMA(ISeries<double> input , int period)
		{
			return indicator.ninZaHelperSMMA(input, period);
		}

	}
}

#endregion
