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
		
		private ninZaDWINTrend[] cacheninZaDWINTrend;

		
		public ninZaDWINTrend ninZaDWINTrend(bool sensitiveModeEnabled, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return ninZaDWINTrend(Input, sensitiveModeEnabled, period, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public ninZaDWINTrend ninZaDWINTrend(ISeries<double> input, bool sensitiveModeEnabled, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			if (cacheninZaDWINTrend != null)
				for (int idx = 0; idx < cacheninZaDWINTrend.Length; idx++)
					if (cacheninZaDWINTrend[idx].SensitiveModeEnabled == sensitiveModeEnabled && cacheninZaDWINTrend[idx].Period == period && cacheninZaDWINTrend[idx].SmoothingEnabled == smoothingEnabled && cacheninZaDWINTrend[idx].SmoothingMethod == smoothingMethod && cacheninZaDWINTrend[idx].SmoothingPeriod == smoothingPeriod && cacheninZaDWINTrend[idx].ThresholdHigh == thresholdHigh && cacheninZaDWINTrend[idx].ThresholdLow == thresholdLow && cacheninZaDWINTrend[idx].EqualsInput(input))
						return cacheninZaDWINTrend[idx];
			return CacheIndicator<ninZaDWINTrend>(new ninZaDWINTrend(){ SensitiveModeEnabled = sensitiveModeEnabled, Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, ThresholdHigh = thresholdHigh, ThresholdLow = thresholdLow }, input, ref cacheninZaDWINTrend);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaDWINTrend ninZaDWINTrend(bool sensitiveModeEnabled, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDWINTrend(Input, sensitiveModeEnabled, period, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaDWINTrend ninZaDWINTrend(ISeries<double> input , bool sensitiveModeEnabled, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDWINTrend(input, sensitiveModeEnabled, period, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaDWINTrend ninZaDWINTrend(bool sensitiveModeEnabled, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDWINTrend(Input, sensitiveModeEnabled, period, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaDWINTrend ninZaDWINTrend(ISeries<double> input , bool sensitiveModeEnabled, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDWINTrend(input, sensitiveModeEnabled, period, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow);
		}

	}
}

#endregion
