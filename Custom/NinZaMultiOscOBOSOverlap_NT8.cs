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
		
		private ninZaMultiOscOBOSOverlap[] cacheninZaMultiOscOBOSOverlap;

		
		public ninZaMultiOscOBOSOverlap ninZaMultiOscOBOSOverlap(int mFIPeriod, double mFIThresholdHigh, double mFIThresholdLow, int rSIPeriod, int rSISmooth, ninZaMultiOscOBOSOverlap_RSIPlot rSIPlot, double rSIThresholdHigh, double rSIThresholdLow, int stochPeriodD, int stochPeriodK, ninZa_MAType stochSmoothingMethod, int stochSmoothingPeriod, ninZaMultiOscOBOSOverlap_StochPlot stochPlot, double stochThresholdHigh, double stochThresholdLow, int safeReversalPeriod)
		{
			return ninZaMultiOscOBOSOverlap(Input, mFIPeriod, mFIThresholdHigh, mFIThresholdLow, rSIPeriod, rSISmooth, rSIPlot, rSIThresholdHigh, rSIThresholdLow, stochPeriodD, stochPeriodK, stochSmoothingMethod, stochSmoothingPeriod, stochPlot, stochThresholdHigh, stochThresholdLow, safeReversalPeriod);
		}


		
		public ninZaMultiOscOBOSOverlap ninZaMultiOscOBOSOverlap(ISeries<double> input, int mFIPeriod, double mFIThresholdHigh, double mFIThresholdLow, int rSIPeriod, int rSISmooth, ninZaMultiOscOBOSOverlap_RSIPlot rSIPlot, double rSIThresholdHigh, double rSIThresholdLow, int stochPeriodD, int stochPeriodK, ninZa_MAType stochSmoothingMethod, int stochSmoothingPeriod, ninZaMultiOscOBOSOverlap_StochPlot stochPlot, double stochThresholdHigh, double stochThresholdLow, int safeReversalPeriod)
		{
			if (cacheninZaMultiOscOBOSOverlap != null)
				for (int idx = 0; idx < cacheninZaMultiOscOBOSOverlap.Length; idx++)
					if (cacheninZaMultiOscOBOSOverlap[idx].MFIPeriod == mFIPeriod && cacheninZaMultiOscOBOSOverlap[idx].MFIThresholdHigh == mFIThresholdHigh && cacheninZaMultiOscOBOSOverlap[idx].MFIThresholdLow == mFIThresholdLow && cacheninZaMultiOscOBOSOverlap[idx].RSIPeriod == rSIPeriod && cacheninZaMultiOscOBOSOverlap[idx].RSISmooth == rSISmooth && cacheninZaMultiOscOBOSOverlap[idx].RSIPlot == rSIPlot && cacheninZaMultiOscOBOSOverlap[idx].RSIThresholdHigh == rSIThresholdHigh && cacheninZaMultiOscOBOSOverlap[idx].RSIThresholdLow == rSIThresholdLow && cacheninZaMultiOscOBOSOverlap[idx].StochPeriodD == stochPeriodD && cacheninZaMultiOscOBOSOverlap[idx].StochPeriodK == stochPeriodK && cacheninZaMultiOscOBOSOverlap[idx].StochSmoothingMethod == stochSmoothingMethod && cacheninZaMultiOscOBOSOverlap[idx].StochSmoothingPeriod == stochSmoothingPeriod && cacheninZaMultiOscOBOSOverlap[idx].StochPlot == stochPlot && cacheninZaMultiOscOBOSOverlap[idx].StochThresholdHigh == stochThresholdHigh && cacheninZaMultiOscOBOSOverlap[idx].StochThresholdLow == stochThresholdLow && cacheninZaMultiOscOBOSOverlap[idx].SafeReversalPeriod == safeReversalPeriod && cacheninZaMultiOscOBOSOverlap[idx].EqualsInput(input))
						return cacheninZaMultiOscOBOSOverlap[idx];
			return CacheIndicator<ninZaMultiOscOBOSOverlap>(new ninZaMultiOscOBOSOverlap(){ MFIPeriod = mFIPeriod, MFIThresholdHigh = mFIThresholdHigh, MFIThresholdLow = mFIThresholdLow, RSIPeriod = rSIPeriod, RSISmooth = rSISmooth, RSIPlot = rSIPlot, RSIThresholdHigh = rSIThresholdHigh, RSIThresholdLow = rSIThresholdLow, StochPeriodD = stochPeriodD, StochPeriodK = stochPeriodK, StochSmoothingMethod = stochSmoothingMethod, StochSmoothingPeriod = stochSmoothingPeriod, StochPlot = stochPlot, StochThresholdHigh = stochThresholdHigh, StochThresholdLow = stochThresholdLow, SafeReversalPeriod = safeReversalPeriod }, input, ref cacheninZaMultiOscOBOSOverlap);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMultiOscOBOSOverlap ninZaMultiOscOBOSOverlap(int mFIPeriod, double mFIThresholdHigh, double mFIThresholdLow, int rSIPeriod, int rSISmooth, ninZaMultiOscOBOSOverlap_RSIPlot rSIPlot, double rSIThresholdHigh, double rSIThresholdLow, int stochPeriodD, int stochPeriodK, ninZa_MAType stochSmoothingMethod, int stochSmoothingPeriod, ninZaMultiOscOBOSOverlap_StochPlot stochPlot, double stochThresholdHigh, double stochThresholdLow, int safeReversalPeriod)
		{
			return indicator.ninZaMultiOscOBOSOverlap(Input, mFIPeriod, mFIThresholdHigh, mFIThresholdLow, rSIPeriod, rSISmooth, rSIPlot, rSIThresholdHigh, rSIThresholdLow, stochPeriodD, stochPeriodK, stochSmoothingMethod, stochSmoothingPeriod, stochPlot, stochThresholdHigh, stochThresholdLow, safeReversalPeriod);
		}


		
		public Indicators.ninZaMultiOscOBOSOverlap ninZaMultiOscOBOSOverlap(ISeries<double> input , int mFIPeriod, double mFIThresholdHigh, double mFIThresholdLow, int rSIPeriod, int rSISmooth, ninZaMultiOscOBOSOverlap_RSIPlot rSIPlot, double rSIThresholdHigh, double rSIThresholdLow, int stochPeriodD, int stochPeriodK, ninZa_MAType stochSmoothingMethod, int stochSmoothingPeriod, ninZaMultiOscOBOSOverlap_StochPlot stochPlot, double stochThresholdHigh, double stochThresholdLow, int safeReversalPeriod)
		{
			return indicator.ninZaMultiOscOBOSOverlap(input, mFIPeriod, mFIThresholdHigh, mFIThresholdLow, rSIPeriod, rSISmooth, rSIPlot, rSIThresholdHigh, rSIThresholdLow, stochPeriodD, stochPeriodK, stochSmoothingMethod, stochSmoothingPeriod, stochPlot, stochThresholdHigh, stochThresholdLow, safeReversalPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMultiOscOBOSOverlap ninZaMultiOscOBOSOverlap(int mFIPeriod, double mFIThresholdHigh, double mFIThresholdLow, int rSIPeriod, int rSISmooth, ninZaMultiOscOBOSOverlap_RSIPlot rSIPlot, double rSIThresholdHigh, double rSIThresholdLow, int stochPeriodD, int stochPeriodK, ninZa_MAType stochSmoothingMethod, int stochSmoothingPeriod, ninZaMultiOscOBOSOverlap_StochPlot stochPlot, double stochThresholdHigh, double stochThresholdLow, int safeReversalPeriod)
		{
			return indicator.ninZaMultiOscOBOSOverlap(Input, mFIPeriod, mFIThresholdHigh, mFIThresholdLow, rSIPeriod, rSISmooth, rSIPlot, rSIThresholdHigh, rSIThresholdLow, stochPeriodD, stochPeriodK, stochSmoothingMethod, stochSmoothingPeriod, stochPlot, stochThresholdHigh, stochThresholdLow, safeReversalPeriod);
		}


		
		public Indicators.ninZaMultiOscOBOSOverlap ninZaMultiOscOBOSOverlap(ISeries<double> input , int mFIPeriod, double mFIThresholdHigh, double mFIThresholdLow, int rSIPeriod, int rSISmooth, ninZaMultiOscOBOSOverlap_RSIPlot rSIPlot, double rSIThresholdHigh, double rSIThresholdLow, int stochPeriodD, int stochPeriodK, ninZa_MAType stochSmoothingMethod, int stochSmoothingPeriod, ninZaMultiOscOBOSOverlap_StochPlot stochPlot, double stochThresholdHigh, double stochThresholdLow, int safeReversalPeriod)
		{
			return indicator.ninZaMultiOscOBOSOverlap(input, mFIPeriod, mFIThresholdHigh, mFIThresholdLow, rSIPeriod, rSISmooth, rSIPlot, rSIThresholdHigh, rSIThresholdLow, stochPeriodD, stochPeriodK, stochSmoothingMethod, stochSmoothingPeriod, stochPlot, stochThresholdHigh, stochThresholdLow, safeReversalPeriod);
		}

	}
}

#endregion
