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
		
		private ninZaATR[] cacheninZaATR;

		
		public ninZaATR ninZaATR(int period)
		{
			return ninZaATR(Input, period);
		}


		
		public ninZaATR ninZaATR(ISeries<double> input, int period)
		{
			if (cacheninZaATR != null)
				for (int idx = 0; idx < cacheninZaATR.Length; idx++)
					if (cacheninZaATR[idx].Period == period && cacheninZaATR[idx].EqualsInput(input))
						return cacheninZaATR[idx];
			return CacheIndicator<ninZaATR>(new ninZaATR(){ Period = period }, input, ref cacheninZaATR);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaATR ninZaATR(int period)
		{
			return indicator.ninZaATR(Input, period);
		}


		
		public Indicators.ninZaATR ninZaATR(ISeries<double> input , int period)
		{
			return indicator.ninZaATR(input, period);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaATR ninZaATR(int period)
		{
			return indicator.ninZaATR(Input, period);
		}


		
		public Indicators.ninZaATR ninZaATR(ISeries<double> input , int period)
		{
			return indicator.ninZaATR(input, period);
		}

	}
}

#endregion
