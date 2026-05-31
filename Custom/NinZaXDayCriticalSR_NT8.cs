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
		
		private ninZaXDayCriticalSR[] cacheninZaXDayCriticalSR;

		
		public ninZaXDayCriticalSR ninZaXDayCriticalSR(int dayNum, int midPercentage)
		{
			return ninZaXDayCriticalSR(Input, dayNum, midPercentage);
		}


		
		public ninZaXDayCriticalSR ninZaXDayCriticalSR(ISeries<double> input, int dayNum, int midPercentage)
		{
			if (cacheninZaXDayCriticalSR != null)
				for (int idx = 0; idx < cacheninZaXDayCriticalSR.Length; idx++)
					if (cacheninZaXDayCriticalSR[idx].DayNum == dayNum && cacheninZaXDayCriticalSR[idx].MidPercentage == midPercentage && cacheninZaXDayCriticalSR[idx].EqualsInput(input))
						return cacheninZaXDayCriticalSR[idx];
			return CacheIndicator<ninZaXDayCriticalSR>(new ninZaXDayCriticalSR(){ DayNum = dayNum, MidPercentage = midPercentage }, input, ref cacheninZaXDayCriticalSR);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaXDayCriticalSR ninZaXDayCriticalSR(int dayNum, int midPercentage)
		{
			return indicator.ninZaXDayCriticalSR(Input, dayNum, midPercentage);
		}


		
		public Indicators.ninZaXDayCriticalSR ninZaXDayCriticalSR(ISeries<double> input , int dayNum, int midPercentage)
		{
			return indicator.ninZaXDayCriticalSR(input, dayNum, midPercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaXDayCriticalSR ninZaXDayCriticalSR(int dayNum, int midPercentage)
		{
			return indicator.ninZaXDayCriticalSR(Input, dayNum, midPercentage);
		}


		
		public Indicators.ninZaXDayCriticalSR ninZaXDayCriticalSR(ISeries<double> input , int dayNum, int midPercentage)
		{
			return indicator.ninZaXDayCriticalSR(input, dayNum, midPercentage);
		}

	}
}

#endregion
