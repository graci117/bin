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
		
		private ninZaIntradayAdaptiveSR[] cacheninZaIntradayAdaptiveSR;

		
		public ninZaIntradayAdaptiveSR ninZaIntradayAdaptiveSR(int midPercentage)
		{
			return ninZaIntradayAdaptiveSR(Input, midPercentage);
		}


		
		public ninZaIntradayAdaptiveSR ninZaIntradayAdaptiveSR(ISeries<double> input, int midPercentage)
		{
			if (cacheninZaIntradayAdaptiveSR != null)
				for (int idx = 0; idx < cacheninZaIntradayAdaptiveSR.Length; idx++)
					if (cacheninZaIntradayAdaptiveSR[idx].MidPercentage == midPercentage && cacheninZaIntradayAdaptiveSR[idx].EqualsInput(input))
						return cacheninZaIntradayAdaptiveSR[idx];
			return CacheIndicator<ninZaIntradayAdaptiveSR>(new ninZaIntradayAdaptiveSR(){ MidPercentage = midPercentage }, input, ref cacheninZaIntradayAdaptiveSR);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaIntradayAdaptiveSR ninZaIntradayAdaptiveSR(int midPercentage)
		{
			return indicator.ninZaIntradayAdaptiveSR(Input, midPercentage);
		}


		
		public Indicators.ninZaIntradayAdaptiveSR ninZaIntradayAdaptiveSR(ISeries<double> input , int midPercentage)
		{
			return indicator.ninZaIntradayAdaptiveSR(input, midPercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaIntradayAdaptiveSR ninZaIntradayAdaptiveSR(int midPercentage)
		{
			return indicator.ninZaIntradayAdaptiveSR(Input, midPercentage);
		}


		
		public Indicators.ninZaIntradayAdaptiveSR ninZaIntradayAdaptiveSR(ISeries<double> input , int midPercentage)
		{
			return indicator.ninZaIntradayAdaptiveSR(input, midPercentage);
		}

	}
}

#endregion
