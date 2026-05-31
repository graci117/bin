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
		
		private ninZaHeikenAshiRSIBlend[] cacheninZaHeikenAshiRSIBlend;

		
		public ninZaHeikenAshiRSIBlend ninZaHeikenAshiRSIBlend(ninZaHeikenAshiRSIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int rSIPeriod, bool rSISmoothingEnabled, int rSISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaHeikenAshiRSIBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, rSIPeriod, rSISmoothingEnabled, rSISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaHeikenAshiRSIBlend ninZaHeikenAshiRSIBlend(ISeries<double> input, ninZaHeikenAshiRSIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int rSIPeriod, bool rSISmoothingEnabled, int rSISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaHeikenAshiRSIBlend != null)
				for (int idx = 0; idx < cacheninZaHeikenAshiRSIBlend.Length; idx++)
					if (cacheninZaHeikenAshiRSIBlend[idx].SignalMode == signalMode && cacheninZaHeikenAshiRSIBlend[idx].StrictModeEnabled == strictModeEnabled && cacheninZaHeikenAshiRSIBlend[idx].HAOpenWeight == hAOpenWeight && cacheninZaHeikenAshiRSIBlend[idx].RSIPeriod == rSIPeriod && cacheninZaHeikenAshiRSIBlend[idx].RSISmoothingEnabled == rSISmoothingEnabled && cacheninZaHeikenAshiRSIBlend[idx].RSISmoothingPeriod == rSISmoothingPeriod && cacheninZaHeikenAshiRSIBlend[idx].ThresholdOverbought == thresholdOverbought && cacheninZaHeikenAshiRSIBlend[idx].ThresholdOversold == thresholdOversold && cacheninZaHeikenAshiRSIBlend[idx].EqualsInput(input))
						return cacheninZaHeikenAshiRSIBlend[idx];
			return CacheIndicator<ninZaHeikenAshiRSIBlend>(new ninZaHeikenAshiRSIBlend(){ SignalMode = signalMode, StrictModeEnabled = strictModeEnabled, HAOpenWeight = hAOpenWeight, RSIPeriod = rSIPeriod, RSISmoothingEnabled = rSISmoothingEnabled, RSISmoothingPeriod = rSISmoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaHeikenAshiRSIBlend);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHeikenAshiRSIBlend ninZaHeikenAshiRSIBlend(ninZaHeikenAshiRSIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int rSIPeriod, bool rSISmoothingEnabled, int rSISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiRSIBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, rSIPeriod, rSISmoothingEnabled, rSISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaHeikenAshiRSIBlend ninZaHeikenAshiRSIBlend(ISeries<double> input , ninZaHeikenAshiRSIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int rSIPeriod, bool rSISmoothingEnabled, int rSISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiRSIBlend(input, signalMode, strictModeEnabled, hAOpenWeight, rSIPeriod, rSISmoothingEnabled, rSISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHeikenAshiRSIBlend ninZaHeikenAshiRSIBlend(ninZaHeikenAshiRSIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int rSIPeriod, bool rSISmoothingEnabled, int rSISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiRSIBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, rSIPeriod, rSISmoothingEnabled, rSISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaHeikenAshiRSIBlend ninZaHeikenAshiRSIBlend(ISeries<double> input , ninZaHeikenAshiRSIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int rSIPeriod, bool rSISmoothingEnabled, int rSISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiRSIBlend(input, signalMode, strictModeEnabled, hAOpenWeight, rSIPeriod, rSISmoothingEnabled, rSISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
