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
		
		private ninZaCCIReversalPro[] cacheninZaCCIReversalPro;

		
		public ninZaCCIReversalPro ninZaCCIReversalPro(bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int highLowStay, double thresholdHigh, double thresholdLow)
		{
			return ninZaCCIReversalPro(Input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, highLowStay, thresholdHigh, thresholdLow);
		}


		
		public ninZaCCIReversalPro ninZaCCIReversalPro(ISeries<double> input, bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int highLowStay, double thresholdHigh, double thresholdLow)
		{
			if (cacheninZaCCIReversalPro != null)
				for (int idx = 0; idx < cacheninZaCCIReversalPro.Length; idx++)
					if (cacheninZaCCIReversalPro[idx].LateModeEnabled == lateModeEnabled && cacheninZaCCIReversalPro[idx].Period == period && cacheninZaCCIReversalPro[idx].Multiplier == multiplier && cacheninZaCCIReversalPro[idx].MAType == mAType && cacheninZaCCIReversalPro[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaCCIReversalPro[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaCCIReversalPro[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaCCIReversalPro[idx].CCISmoothingEnabled == cCISmoothingEnabled && cacheninZaCCIReversalPro[idx].CCISmoothingMethod == cCISmoothingMethod && cacheninZaCCIReversalPro[idx].CCISmoothingPeriod == cCISmoothingPeriod && cacheninZaCCIReversalPro[idx].HighLowStay == highLowStay && cacheninZaCCIReversalPro[idx].ThresholdHigh == thresholdHigh && cacheninZaCCIReversalPro[idx].ThresholdLow == thresholdLow && cacheninZaCCIReversalPro[idx].EqualsInput(input))
						return cacheninZaCCIReversalPro[idx];
			return CacheIndicator<ninZaCCIReversalPro>(new ninZaCCIReversalPro(){ LateModeEnabled = lateModeEnabled, Period = period, Multiplier = multiplier, MAType = mAType, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, CCISmoothingEnabled = cCISmoothingEnabled, CCISmoothingMethod = cCISmoothingMethod, CCISmoothingPeriod = cCISmoothingPeriod, HighLowStay = highLowStay, ThresholdHigh = thresholdHigh, ThresholdLow = thresholdLow }, input, ref cacheninZaCCIReversalPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaCCIReversalPro ninZaCCIReversalPro(bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int highLowStay, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCIReversalPro(Input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, highLowStay, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaCCIReversalPro ninZaCCIReversalPro(ISeries<double> input , bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int highLowStay, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCIReversalPro(input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, highLowStay, thresholdHigh, thresholdLow);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaCCIReversalPro ninZaCCIReversalPro(bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int highLowStay, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCIReversalPro(Input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, highLowStay, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaCCIReversalPro ninZaCCIReversalPro(ISeries<double> input , bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int highLowStay, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCIReversalPro(input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, highLowStay, thresholdHigh, thresholdLow);
		}

	}
}

#endregion
