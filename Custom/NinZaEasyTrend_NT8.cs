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
		
		private ninZaEasyTrend[] cacheninZaEasyTrend;

		
		public ninZaEasyTrend ninZaEasyTrend(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaEasyTrend_FilterUnit filterUnit, int filterATRPeriod)
		{
			return ninZaEasyTrend(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}


		
		public ninZaEasyTrend ninZaEasyTrend(ISeries<double> input, ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaEasyTrend_FilterUnit filterUnit, int filterATRPeriod)
		{
			if (cacheninZaEasyTrend != null)
				for (int idx = 0; idx < cacheninZaEasyTrend.Length; idx++)
					if (cacheninZaEasyTrend[idx].MAType == mAType && cacheninZaEasyTrend[idx].Period == period && cacheninZaEasyTrend[idx].SmoothingEnabled == smoothingEnabled && cacheninZaEasyTrend[idx].SmoothingMethod == smoothingMethod && cacheninZaEasyTrend[idx].SmoothingPeriod == smoothingPeriod && cacheninZaEasyTrend[idx].FilterEnabled == filterEnabled && cacheninZaEasyTrend[idx].FilterAfterSmoothing == filterAfterSmoothing && cacheninZaEasyTrend[idx].FilterMultiplier == filterMultiplier && cacheninZaEasyTrend[idx].FilterUnit == filterUnit && cacheninZaEasyTrend[idx].FilterATRPeriod == filterATRPeriod && cacheninZaEasyTrend[idx].EqualsInput(input))
						return cacheninZaEasyTrend[idx];
			return CacheIndicator<ninZaEasyTrend>(new ninZaEasyTrend(){ MAType = mAType, Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, FilterEnabled = filterEnabled, FilterAfterSmoothing = filterAfterSmoothing, FilterMultiplier = filterMultiplier, FilterUnit = filterUnit, FilterATRPeriod = filterATRPeriod }, input, ref cacheninZaEasyTrend);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaEasyTrend ninZaEasyTrend(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaEasyTrend_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaEasyTrend(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}


		
		public Indicators.ninZaEasyTrend ninZaEasyTrend(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaEasyTrend_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaEasyTrend(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaEasyTrend ninZaEasyTrend(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaEasyTrend_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaEasyTrend(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}


		
		public Indicators.ninZaEasyTrend ninZaEasyTrend(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaEasyTrend_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaEasyTrend(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}

	}
}

#endregion
