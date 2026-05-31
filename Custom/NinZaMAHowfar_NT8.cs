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
		
		private ninZaMAHowfar[] cacheninZaMAHowfar;

		
		public ninZaMAHowfar ninZaMAHowfar(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool howfarSmoothingEnabled, ninZa_MAType howfarSmoothingMethod, int howfarSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return ninZaMAHowfar(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, howfarSmoothingEnabled, howfarSmoothingMethod, howfarSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}


		
		public ninZaMAHowfar ninZaMAHowfar(ISeries<double> input, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool howfarSmoothingEnabled, ninZa_MAType howfarSmoothingMethod, int howfarSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			if (cacheninZaMAHowfar != null)
				for (int idx = 0; idx < cacheninZaMAHowfar.Length; idx++)
					if (cacheninZaMAHowfar[idx].MAType == mAType && cacheninZaMAHowfar[idx].MAPeriod == mAPeriod && cacheninZaMAHowfar[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaMAHowfar[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaMAHowfar[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaMAHowfar[idx].HowfarSmoothingEnabled == howfarSmoothingEnabled && cacheninZaMAHowfar[idx].HowfarSmoothingMethod == howfarSmoothingMethod && cacheninZaMAHowfar[idx].HowfarSmoothingPeriod == howfarSmoothingPeriod && cacheninZaMAHowfar[idx].SlowdownScan == slowdownScan && cacheninZaMAHowfar[idx].ThresholdMultipler == thresholdMultipler && cacheninZaMAHowfar[idx].ThresholdATRPeriod == thresholdATRPeriod && cacheninZaMAHowfar[idx].ThresholdMinimum == thresholdMinimum && cacheninZaMAHowfar[idx].EqualsInput(input))
						return cacheninZaMAHowfar[idx];
			return CacheIndicator<ninZaMAHowfar>(new ninZaMAHowfar(){ MAType = mAType, MAPeriod = mAPeriod, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, HowfarSmoothingEnabled = howfarSmoothingEnabled, HowfarSmoothingMethod = howfarSmoothingMethod, HowfarSmoothingPeriod = howfarSmoothingPeriod, SlowdownScan = slowdownScan, ThresholdMultipler = thresholdMultipler, ThresholdATRPeriod = thresholdATRPeriod, ThresholdMinimum = thresholdMinimum }, input, ref cacheninZaMAHowfar);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMAHowfar ninZaMAHowfar(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool howfarSmoothingEnabled, ninZa_MAType howfarSmoothingMethod, int howfarSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaMAHowfar(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, howfarSmoothingEnabled, howfarSmoothingMethod, howfarSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}


		
		public Indicators.ninZaMAHowfar ninZaMAHowfar(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool howfarSmoothingEnabled, ninZa_MAType howfarSmoothingMethod, int howfarSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaMAHowfar(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, howfarSmoothingEnabled, howfarSmoothingMethod, howfarSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMAHowfar ninZaMAHowfar(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool howfarSmoothingEnabled, ninZa_MAType howfarSmoothingMethod, int howfarSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaMAHowfar(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, howfarSmoothingEnabled, howfarSmoothingMethod, howfarSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}


		
		public Indicators.ninZaMAHowfar ninZaMAHowfar(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, bool howfarSmoothingEnabled, ninZa_MAType howfarSmoothingMethod, int howfarSmoothingPeriod, int slowdownScan, double thresholdMultipler, int thresholdATRPeriod, int thresholdMinimum)
		{
			return indicator.ninZaMAHowfar(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, howfarSmoothingEnabled, howfarSmoothingMethod, howfarSmoothingPeriod, slowdownScan, thresholdMultipler, thresholdATRPeriod, thresholdMinimum);
		}

	}
}

#endregion
