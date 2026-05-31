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
		
		private ninZaSuperiorMFI[] cacheninZaSuperiorMFI;

		
		public ninZaSuperiorMFI ninZaSuperiorMFI(int mFIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaSuperiorMFI(Input, mFIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaSuperiorMFI ninZaSuperiorMFI(ISeries<double> input, int mFIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaSuperiorMFI != null)
				for (int idx = 0; idx < cacheninZaSuperiorMFI.Length; idx++)
					if (cacheninZaSuperiorMFI[idx].MFIPeriod == mFIPeriod && cacheninZaSuperiorMFI[idx].SmoothingEnabled == smoothingEnabled && cacheninZaSuperiorMFI[idx].SmoothingMethod == smoothingMethod && cacheninZaSuperiorMFI[idx].SmoothingPeriod == smoothingPeriod && cacheninZaSuperiorMFI[idx].ThresholdOverbought == thresholdOverbought && cacheninZaSuperiorMFI[idx].ThresholdOversold == thresholdOversold && cacheninZaSuperiorMFI[idx].EqualsInput(input))
						return cacheninZaSuperiorMFI[idx];
			return CacheIndicator<ninZaSuperiorMFI>(new ninZaSuperiorMFI(){ MFIPeriod = mFIPeriod, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaSuperiorMFI);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorMFI ninZaSuperiorMFI(int mFIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorMFI(Input, mFIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaSuperiorMFI ninZaSuperiorMFI(ISeries<double> input , int mFIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorMFI(input, mFIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorMFI ninZaSuperiorMFI(int mFIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorMFI(Input, mFIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaSuperiorMFI ninZaSuperiorMFI(ISeries<double> input , int mFIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorMFI(input, mFIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
