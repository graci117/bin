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
		
		private ninZaPrevdayCriticalSR[] cacheninZaPrevdayCriticalSR;

		
		public ninZaPrevdayCriticalSR ninZaPrevdayCriticalSR(int midPercentage)
		{
			return ninZaPrevdayCriticalSR(Input, midPercentage);
		}


		
		public ninZaPrevdayCriticalSR ninZaPrevdayCriticalSR(ISeries<double> input, int midPercentage)
		{
			if (cacheninZaPrevdayCriticalSR != null)
				for (int idx = 0; idx < cacheninZaPrevdayCriticalSR.Length; idx++)
					if (cacheninZaPrevdayCriticalSR[idx].MidPercentage == midPercentage && cacheninZaPrevdayCriticalSR[idx].EqualsInput(input))
						return cacheninZaPrevdayCriticalSR[idx];
			return CacheIndicator<ninZaPrevdayCriticalSR>(new ninZaPrevdayCriticalSR(){ MidPercentage = midPercentage }, input, ref cacheninZaPrevdayCriticalSR);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaPrevdayCriticalSR ninZaPrevdayCriticalSR(int midPercentage)
		{
			return indicator.ninZaPrevdayCriticalSR(Input, midPercentage);
		}


		
		public Indicators.ninZaPrevdayCriticalSR ninZaPrevdayCriticalSR(ISeries<double> input , int midPercentage)
		{
			return indicator.ninZaPrevdayCriticalSR(input, midPercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaPrevdayCriticalSR ninZaPrevdayCriticalSR(int midPercentage)
		{
			return indicator.ninZaPrevdayCriticalSR(Input, midPercentage);
		}


		
		public Indicators.ninZaPrevdayCriticalSR ninZaPrevdayCriticalSR(ISeries<double> input , int midPercentage)
		{
			return indicator.ninZaPrevdayCriticalSR(input, midPercentage);
		}

	}
}

#endregion
