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
		
		private ninZaHeikenAshiStochasticBlend[] cacheninZaHeikenAshiStochasticBlend;

		
		public ninZaHeikenAshiStochasticBlend ninZaHeikenAshiStochasticBlend(ninZaHeikenAshiStochasticBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, ninZaHeikenAshiStochasticBlend_StochasticPlot stochasticPlot, int stochasticPeriodD, int stochasticPeriodK, ninZa_MAType stochasticSmoothingMethod, int stochasticSmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaHeikenAshiStochasticBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, stochasticPlot, stochasticPeriodD, stochasticPeriodK, stochasticSmoothingMethod, stochasticSmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaHeikenAshiStochasticBlend ninZaHeikenAshiStochasticBlend(ISeries<double> input, ninZaHeikenAshiStochasticBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, ninZaHeikenAshiStochasticBlend_StochasticPlot stochasticPlot, int stochasticPeriodD, int stochasticPeriodK, ninZa_MAType stochasticSmoothingMethod, int stochasticSmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaHeikenAshiStochasticBlend != null)
				for (int idx = 0; idx < cacheninZaHeikenAshiStochasticBlend.Length; idx++)
					if (cacheninZaHeikenAshiStochasticBlend[idx].SignalMode == signalMode && cacheninZaHeikenAshiStochasticBlend[idx].StrictModeEnabled == strictModeEnabled && cacheninZaHeikenAshiStochasticBlend[idx].HAOpenWeight == hAOpenWeight && cacheninZaHeikenAshiStochasticBlend[idx].StochasticPlot == stochasticPlot && cacheninZaHeikenAshiStochasticBlend[idx].StochasticPeriodD == stochasticPeriodD && cacheninZaHeikenAshiStochasticBlend[idx].StochasticPeriodK == stochasticPeriodK && cacheninZaHeikenAshiStochasticBlend[idx].StochasticSmoothingMethod == stochasticSmoothingMethod && cacheninZaHeikenAshiStochasticBlend[idx].StochasticSmoothingPeriod == stochasticSmoothingPeriod && cacheninZaHeikenAshiStochasticBlend[idx].ThresholdOverbought == thresholdOverbought && cacheninZaHeikenAshiStochasticBlend[idx].ThresholdOversold == thresholdOversold && cacheninZaHeikenAshiStochasticBlend[idx].EqualsInput(input))
						return cacheninZaHeikenAshiStochasticBlend[idx];
			return CacheIndicator<ninZaHeikenAshiStochasticBlend>(new ninZaHeikenAshiStochasticBlend(){ SignalMode = signalMode, StrictModeEnabled = strictModeEnabled, HAOpenWeight = hAOpenWeight, StochasticPlot = stochasticPlot, StochasticPeriodD = stochasticPeriodD, StochasticPeriodK = stochasticPeriodK, StochasticSmoothingMethod = stochasticSmoothingMethod, StochasticSmoothingPeriod = stochasticSmoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaHeikenAshiStochasticBlend);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHeikenAshiStochasticBlend ninZaHeikenAshiStochasticBlend(ninZaHeikenAshiStochasticBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, ninZaHeikenAshiStochasticBlend_StochasticPlot stochasticPlot, int stochasticPeriodD, int stochasticPeriodK, ninZa_MAType stochasticSmoothingMethod, int stochasticSmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiStochasticBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, stochasticPlot, stochasticPeriodD, stochasticPeriodK, stochasticSmoothingMethod, stochasticSmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaHeikenAshiStochasticBlend ninZaHeikenAshiStochasticBlend(ISeries<double> input , ninZaHeikenAshiStochasticBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, ninZaHeikenAshiStochasticBlend_StochasticPlot stochasticPlot, int stochasticPeriodD, int stochasticPeriodK, ninZa_MAType stochasticSmoothingMethod, int stochasticSmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiStochasticBlend(input, signalMode, strictModeEnabled, hAOpenWeight, stochasticPlot, stochasticPeriodD, stochasticPeriodK, stochasticSmoothingMethod, stochasticSmoothingPeriod, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHeikenAshiStochasticBlend ninZaHeikenAshiStochasticBlend(ninZaHeikenAshiStochasticBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, ninZaHeikenAshiStochasticBlend_StochasticPlot stochasticPlot, int stochasticPeriodD, int stochasticPeriodK, ninZa_MAType stochasticSmoothingMethod, int stochasticSmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiStochasticBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, stochasticPlot, stochasticPeriodD, stochasticPeriodK, stochasticSmoothingMethod, stochasticSmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaHeikenAshiStochasticBlend ninZaHeikenAshiStochasticBlend(ISeries<double> input , ninZaHeikenAshiStochasticBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, ninZaHeikenAshiStochasticBlend_StochasticPlot stochasticPlot, int stochasticPeriodD, int stochasticPeriodK, ninZa_MAType stochasticSmoothingMethod, int stochasticSmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiStochasticBlend(input, signalMode, strictModeEnabled, hAOpenWeight, stochasticPlot, stochasticPeriodD, stochasticPeriodK, stochasticSmoothingMethod, stochasticSmoothingPeriod, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
