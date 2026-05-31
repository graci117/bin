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
		
		private ninZaWoodieZLRPro[] cacheninZaWoodieZLRPro;

		
		public ninZaWoodieZLRPro ninZaWoodieZLRPro(bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int rejectIntervalMin, int rejectIntervalMax, double thresholdHigh, double thresholdLow)
		{
			return ninZaWoodieZLRPro(Input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, rejectIntervalMin, rejectIntervalMax, thresholdHigh, thresholdLow);
		}


		
		public ninZaWoodieZLRPro ninZaWoodieZLRPro(ISeries<double> input, bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int rejectIntervalMin, int rejectIntervalMax, double thresholdHigh, double thresholdLow)
		{
			if (cacheninZaWoodieZLRPro != null)
				for (int idx = 0; idx < cacheninZaWoodieZLRPro.Length; idx++)
					if (cacheninZaWoodieZLRPro[idx].LateModeEnabled == lateModeEnabled && cacheninZaWoodieZLRPro[idx].Period == period && cacheninZaWoodieZLRPro[idx].Multiplier == multiplier && cacheninZaWoodieZLRPro[idx].MAType == mAType && cacheninZaWoodieZLRPro[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaWoodieZLRPro[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaWoodieZLRPro[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaWoodieZLRPro[idx].CCISmoothingEnabled == cCISmoothingEnabled && cacheninZaWoodieZLRPro[idx].CCISmoothingMethod == cCISmoothingMethod && cacheninZaWoodieZLRPro[idx].CCISmoothingPeriod == cCISmoothingPeriod && cacheninZaWoodieZLRPro[idx].RejectIntervalMin == rejectIntervalMin && cacheninZaWoodieZLRPro[idx].RejectIntervalMax == rejectIntervalMax && cacheninZaWoodieZLRPro[idx].ThresholdHigh == thresholdHigh && cacheninZaWoodieZLRPro[idx].ThresholdLow == thresholdLow && cacheninZaWoodieZLRPro[idx].EqualsInput(input))
						return cacheninZaWoodieZLRPro[idx];
			return CacheIndicator<ninZaWoodieZLRPro>(new ninZaWoodieZLRPro(){ LateModeEnabled = lateModeEnabled, Period = period, Multiplier = multiplier, MAType = mAType, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, CCISmoothingEnabled = cCISmoothingEnabled, CCISmoothingMethod = cCISmoothingMethod, CCISmoothingPeriod = cCISmoothingPeriod, RejectIntervalMin = rejectIntervalMin, RejectIntervalMax = rejectIntervalMax, ThresholdHigh = thresholdHigh, ThresholdLow = thresholdLow }, input, ref cacheninZaWoodieZLRPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaWoodieZLRPro ninZaWoodieZLRPro(bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int rejectIntervalMin, int rejectIntervalMax, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaWoodieZLRPro(Input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, rejectIntervalMin, rejectIntervalMax, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaWoodieZLRPro ninZaWoodieZLRPro(ISeries<double> input , bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int rejectIntervalMin, int rejectIntervalMax, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaWoodieZLRPro(input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, rejectIntervalMin, rejectIntervalMax, thresholdHigh, thresholdLow);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaWoodieZLRPro ninZaWoodieZLRPro(bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int rejectIntervalMin, int rejectIntervalMax, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaWoodieZLRPro(Input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, rejectIntervalMin, rejectIntervalMax, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaWoodieZLRPro ninZaWoodieZLRPro(ISeries<double> input , bool lateModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, int rejectIntervalMin, int rejectIntervalMax, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaWoodieZLRPro(input, lateModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, rejectIntervalMin, rejectIntervalMax, thresholdHigh, thresholdLow);
		}

	}
}

#endregion
