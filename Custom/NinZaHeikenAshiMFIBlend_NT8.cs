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
		
		private ninZaHeikenAshiMFIBlend[] cacheninZaHeikenAshiMFIBlend;

		
		public ninZaHeikenAshiMFIBlend ninZaHeikenAshiMFIBlend(ninZaHeikenAshiMFIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int mFIPeriod, bool mFISmoothingEnabled, ninZa_MAType mFISmoothingMethod, int mFISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaHeikenAshiMFIBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, mFIPeriod, mFISmoothingEnabled, mFISmoothingMethod, mFISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaHeikenAshiMFIBlend ninZaHeikenAshiMFIBlend(ISeries<double> input, ninZaHeikenAshiMFIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int mFIPeriod, bool mFISmoothingEnabled, ninZa_MAType mFISmoothingMethod, int mFISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaHeikenAshiMFIBlend != null)
				for (int idx = 0; idx < cacheninZaHeikenAshiMFIBlend.Length; idx++)
					if (cacheninZaHeikenAshiMFIBlend[idx].SignalMode == signalMode && cacheninZaHeikenAshiMFIBlend[idx].StrictModeEnabled == strictModeEnabled && cacheninZaHeikenAshiMFIBlend[idx].HAOpenWeight == hAOpenWeight && cacheninZaHeikenAshiMFIBlend[idx].MFIPeriod == mFIPeriod && cacheninZaHeikenAshiMFIBlend[idx].MFISmoothingEnabled == mFISmoothingEnabled && cacheninZaHeikenAshiMFIBlend[idx].MFISmoothingMethod == mFISmoothingMethod && cacheninZaHeikenAshiMFIBlend[idx].MFISmoothingPeriod == mFISmoothingPeriod && cacheninZaHeikenAshiMFIBlend[idx].ThresholdOverbought == thresholdOverbought && cacheninZaHeikenAshiMFIBlend[idx].ThresholdOversold == thresholdOversold && cacheninZaHeikenAshiMFIBlend[idx].EqualsInput(input))
						return cacheninZaHeikenAshiMFIBlend[idx];
			return CacheIndicator<ninZaHeikenAshiMFIBlend>(new ninZaHeikenAshiMFIBlend(){ SignalMode = signalMode, StrictModeEnabled = strictModeEnabled, HAOpenWeight = hAOpenWeight, MFIPeriod = mFIPeriod, MFISmoothingEnabled = mFISmoothingEnabled, MFISmoothingMethod = mFISmoothingMethod, MFISmoothingPeriod = mFISmoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaHeikenAshiMFIBlend);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHeikenAshiMFIBlend ninZaHeikenAshiMFIBlend(ninZaHeikenAshiMFIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int mFIPeriod, bool mFISmoothingEnabled, ninZa_MAType mFISmoothingMethod, int mFISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiMFIBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, mFIPeriod, mFISmoothingEnabled, mFISmoothingMethod, mFISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaHeikenAshiMFIBlend ninZaHeikenAshiMFIBlend(ISeries<double> input , ninZaHeikenAshiMFIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int mFIPeriod, bool mFISmoothingEnabled, ninZa_MAType mFISmoothingMethod, int mFISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiMFIBlend(input, signalMode, strictModeEnabled, hAOpenWeight, mFIPeriod, mFISmoothingEnabled, mFISmoothingMethod, mFISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHeikenAshiMFIBlend ninZaHeikenAshiMFIBlend(ninZaHeikenAshiMFIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int mFIPeriod, bool mFISmoothingEnabled, ninZa_MAType mFISmoothingMethod, int mFISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiMFIBlend(Input, signalMode, strictModeEnabled, hAOpenWeight, mFIPeriod, mFISmoothingEnabled, mFISmoothingMethod, mFISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaHeikenAshiMFIBlend ninZaHeikenAshiMFIBlend(ISeries<double> input , ninZaHeikenAshiMFIBlend_SignalMode signalMode, bool strictModeEnabled, int hAOpenWeight, int mFIPeriod, bool mFISmoothingEnabled, ninZa_MAType mFISmoothingMethod, int mFISmoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaHeikenAshiMFIBlend(input, signalMode, strictModeEnabled, hAOpenWeight, mFIPeriod, mFISmoothingEnabled, mFISmoothingMethod, mFISmoothingPeriod, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
