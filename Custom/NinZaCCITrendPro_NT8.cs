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
		
		private ninZaCCITrendPro[] cacheninZaCCITrendPro;

		
		public ninZaCCITrendPro ninZaCCITrendPro(bool sensitiveModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return ninZaCCITrendPro(Input, sensitiveModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public ninZaCCITrendPro ninZaCCITrendPro(ISeries<double> input, bool sensitiveModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			if (cacheninZaCCITrendPro != null)
				for (int idx = 0; idx < cacheninZaCCITrendPro.Length; idx++)
					if (cacheninZaCCITrendPro[idx].SensitiveModeEnabled == sensitiveModeEnabled && cacheninZaCCITrendPro[idx].Period == period && cacheninZaCCITrendPro[idx].Multiplier == multiplier && cacheninZaCCITrendPro[idx].MAType == mAType && cacheninZaCCITrendPro[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaCCITrendPro[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaCCITrendPro[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaCCITrendPro[idx].CCISmoothingEnabled == cCISmoothingEnabled && cacheninZaCCITrendPro[idx].CCISmoothingMethod == cCISmoothingMethod && cacheninZaCCITrendPro[idx].CCISmoothingPeriod == cCISmoothingPeriod && cacheninZaCCITrendPro[idx].ThresholdHigh == thresholdHigh && cacheninZaCCITrendPro[idx].ThresholdLow == thresholdLow && cacheninZaCCITrendPro[idx].EqualsInput(input))
						return cacheninZaCCITrendPro[idx];
			return CacheIndicator<ninZaCCITrendPro>(new ninZaCCITrendPro(){ SensitiveModeEnabled = sensitiveModeEnabled, Period = period, Multiplier = multiplier, MAType = mAType, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, CCISmoothingEnabled = cCISmoothingEnabled, CCISmoothingMethod = cCISmoothingMethod, CCISmoothingPeriod = cCISmoothingPeriod, ThresholdHigh = thresholdHigh, ThresholdLow = thresholdLow }, input, ref cacheninZaCCITrendPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaCCITrendPro ninZaCCITrendPro(bool sensitiveModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCITrendPro(Input, sensitiveModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaCCITrendPro ninZaCCITrendPro(ISeries<double> input , bool sensitiveModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCITrendPro(input, sensitiveModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, thresholdHigh, thresholdLow);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaCCITrendPro ninZaCCITrendPro(bool sensitiveModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCITrendPro(Input, sensitiveModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaCCITrendPro ninZaCCITrendPro(ISeries<double> input , bool sensitiveModeEnabled, int period, double multiplier, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaCCITrendPro(input, sensitiveModeEnabled, period, multiplier, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, thresholdHigh, thresholdLow);
		}

	}
}

#endregion
