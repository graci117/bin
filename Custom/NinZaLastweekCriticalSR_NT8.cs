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
		
		private ninZaLastweekCriticalSR[] cacheninZaLastweekCriticalSR;

		
		public ninZaLastweekCriticalSR ninZaLastweekCriticalSR(ninZaLastweekCriticalSR_WeekStart weekStart, int midPercentage)
		{
			return ninZaLastweekCriticalSR(Input, weekStart, midPercentage);
		}


		
		public ninZaLastweekCriticalSR ninZaLastweekCriticalSR(ISeries<double> input, ninZaLastweekCriticalSR_WeekStart weekStart, int midPercentage)
		{
			if (cacheninZaLastweekCriticalSR != null)
				for (int idx = 0; idx < cacheninZaLastweekCriticalSR.Length; idx++)
					if (cacheninZaLastweekCriticalSR[idx].WeekStart == weekStart && cacheninZaLastweekCriticalSR[idx].MidPercentage == midPercentage && cacheninZaLastweekCriticalSR[idx].EqualsInput(input))
						return cacheninZaLastweekCriticalSR[idx];
			return CacheIndicator<ninZaLastweekCriticalSR>(new ninZaLastweekCriticalSR(){ WeekStart = weekStart, MidPercentage = midPercentage }, input, ref cacheninZaLastweekCriticalSR);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaLastweekCriticalSR ninZaLastweekCriticalSR(ninZaLastweekCriticalSR_WeekStart weekStart, int midPercentage)
		{
			return indicator.ninZaLastweekCriticalSR(Input, weekStart, midPercentage);
		}


		
		public Indicators.ninZaLastweekCriticalSR ninZaLastweekCriticalSR(ISeries<double> input , ninZaLastweekCriticalSR_WeekStart weekStart, int midPercentage)
		{
			return indicator.ninZaLastweekCriticalSR(input, weekStart, midPercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaLastweekCriticalSR ninZaLastweekCriticalSR(ninZaLastweekCriticalSR_WeekStart weekStart, int midPercentage)
		{
			return indicator.ninZaLastweekCriticalSR(Input, weekStart, midPercentage);
		}


		
		public Indicators.ninZaLastweekCriticalSR ninZaLastweekCriticalSR(ISeries<double> input , ninZaLastweekCriticalSR_WeekStart weekStart, int midPercentage)
		{
			return indicator.ninZaLastweekCriticalSR(input, weekStart, midPercentage);
		}

	}
}

#endregion
