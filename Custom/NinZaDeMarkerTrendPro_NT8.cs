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
		
		private ninZaDeMarkerTrendPro[] cacheninZaDeMarkerTrendPro;

		
		public ninZaDeMarkerTrendPro ninZaDeMarkerTrendPro(bool sensitiveModeEnabled, int period, ninZa_MAType mADeMax, ninZa_MAType mADeMin, bool highLowSmoothingEnabled, ninZa_MAType highLowSmoothingMethod, int highLowSmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return ninZaDeMarkerTrendPro(Input, sensitiveModeEnabled, period, mADeMax, mADeMin, highLowSmoothingEnabled, highLowSmoothingMethod, highLowSmoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public ninZaDeMarkerTrendPro ninZaDeMarkerTrendPro(ISeries<double> input, bool sensitiveModeEnabled, int period, ninZa_MAType mADeMax, ninZa_MAType mADeMin, bool highLowSmoothingEnabled, ninZa_MAType highLowSmoothingMethod, int highLowSmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			if (cacheninZaDeMarkerTrendPro != null)
				for (int idx = 0; idx < cacheninZaDeMarkerTrendPro.Length; idx++)
					if (cacheninZaDeMarkerTrendPro[idx].SensitiveModeEnabled == sensitiveModeEnabled && cacheninZaDeMarkerTrendPro[idx].Period == period && cacheninZaDeMarkerTrendPro[idx].MADeMax == mADeMax && cacheninZaDeMarkerTrendPro[idx].MADeMin == mADeMin && cacheninZaDeMarkerTrendPro[idx].HighLowSmoothingEnabled == highLowSmoothingEnabled && cacheninZaDeMarkerTrendPro[idx].HighLowSmoothingMethod == highLowSmoothingMethod && cacheninZaDeMarkerTrendPro[idx].HighLowSmoothingPeriod == highLowSmoothingPeriod && cacheninZaDeMarkerTrendPro[idx].ThresholdHigh == thresholdHigh && cacheninZaDeMarkerTrendPro[idx].ThresholdLow == thresholdLow && cacheninZaDeMarkerTrendPro[idx].EqualsInput(input))
						return cacheninZaDeMarkerTrendPro[idx];
			return CacheIndicator<ninZaDeMarkerTrendPro>(new ninZaDeMarkerTrendPro(){ SensitiveModeEnabled = sensitiveModeEnabled, Period = period, MADeMax = mADeMax, MADeMin = mADeMin, HighLowSmoothingEnabled = highLowSmoothingEnabled, HighLowSmoothingMethod = highLowSmoothingMethod, HighLowSmoothingPeriod = highLowSmoothingPeriod, ThresholdHigh = thresholdHigh, ThresholdLow = thresholdLow }, input, ref cacheninZaDeMarkerTrendPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaDeMarkerTrendPro ninZaDeMarkerTrendPro(bool sensitiveModeEnabled, int period, ninZa_MAType mADeMax, ninZa_MAType mADeMin, bool highLowSmoothingEnabled, ninZa_MAType highLowSmoothingMethod, int highLowSmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDeMarkerTrendPro(Input, sensitiveModeEnabled, period, mADeMax, mADeMin, highLowSmoothingEnabled, highLowSmoothingMethod, highLowSmoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaDeMarkerTrendPro ninZaDeMarkerTrendPro(ISeries<double> input , bool sensitiveModeEnabled, int period, ninZa_MAType mADeMax, ninZa_MAType mADeMin, bool highLowSmoothingEnabled, ninZa_MAType highLowSmoothingMethod, int highLowSmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDeMarkerTrendPro(input, sensitiveModeEnabled, period, mADeMax, mADeMin, highLowSmoothingEnabled, highLowSmoothingMethod, highLowSmoothingPeriod, thresholdHigh, thresholdLow);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaDeMarkerTrendPro ninZaDeMarkerTrendPro(bool sensitiveModeEnabled, int period, ninZa_MAType mADeMax, ninZa_MAType mADeMin, bool highLowSmoothingEnabled, ninZa_MAType highLowSmoothingMethod, int highLowSmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDeMarkerTrendPro(Input, sensitiveModeEnabled, period, mADeMax, mADeMin, highLowSmoothingEnabled, highLowSmoothingMethod, highLowSmoothingPeriod, thresholdHigh, thresholdLow);
		}


		
		public Indicators.ninZaDeMarkerTrendPro ninZaDeMarkerTrendPro(ISeries<double> input , bool sensitiveModeEnabled, int period, ninZa_MAType mADeMax, ninZa_MAType mADeMin, bool highLowSmoothingEnabled, ninZa_MAType highLowSmoothingMethod, int highLowSmoothingPeriod, double thresholdHigh, double thresholdLow)
		{
			return indicator.ninZaDeMarkerTrendPro(input, sensitiveModeEnabled, period, mADeMax, mADeMin, highLowSmoothingEnabled, highLowSmoothingMethod, highLowSmoothingPeriod, thresholdHigh, thresholdLow);
		}

	}
}

#endregion
