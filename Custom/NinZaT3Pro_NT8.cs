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
		
		private ninZaT3Pro[] cacheninZaT3Pro;

		
		public ninZaT3Pro ninZaT3Pro(ninZa_MAType mAType, int period, int tCount, double vFactor, bool chaosSmoothingEnabled, ninZaT3Pro_ChaosSmoothingMethod chaosSmoothingMethod, int chaosSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return ninZaT3Pro(Input, mAType, period, tCount, vFactor, chaosSmoothingEnabled, chaosSmoothingMethod, chaosSmoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public ninZaT3Pro ninZaT3Pro(ISeries<double> input, ninZa_MAType mAType, int period, int tCount, double vFactor, bool chaosSmoothingEnabled, ninZaT3Pro_ChaosSmoothingMethod chaosSmoothingMethod, int chaosSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaT3Pro != null)
				for (int idx = 0; idx < cacheninZaT3Pro.Length; idx++)
					if (cacheninZaT3Pro[idx].MAType == mAType && cacheninZaT3Pro[idx].Period == period && cacheninZaT3Pro[idx].TCount == tCount && cacheninZaT3Pro[idx].VFactor == vFactor && cacheninZaT3Pro[idx].ChaosSmoothingEnabled == chaosSmoothingEnabled && cacheninZaT3Pro[idx].ChaosSmoothingMethod == chaosSmoothingMethod && cacheninZaT3Pro[idx].ChaosSmoothingPeriod == chaosSmoothingPeriod && cacheninZaT3Pro[idx].FilterEnabled == filterEnabled && cacheninZaT3Pro[idx].FilterMultiplier == filterMultiplier && cacheninZaT3Pro[idx].EqualsInput(input))
						return cacheninZaT3Pro[idx];
			return CacheIndicator<ninZaT3Pro>(new ninZaT3Pro(){ MAType = mAType, Period = period, TCount = tCount, VFactor = vFactor, ChaosSmoothingEnabled = chaosSmoothingEnabled, ChaosSmoothingMethod = chaosSmoothingMethod, ChaosSmoothingPeriod = chaosSmoothingPeriod, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaT3Pro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaT3Pro ninZaT3Pro(ninZa_MAType mAType, int period, int tCount, double vFactor, bool chaosSmoothingEnabled, ninZaT3Pro_ChaosSmoothingMethod chaosSmoothingMethod, int chaosSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaT3Pro(Input, mAType, period, tCount, vFactor, chaosSmoothingEnabled, chaosSmoothingMethod, chaosSmoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaT3Pro ninZaT3Pro(ISeries<double> input , ninZa_MAType mAType, int period, int tCount, double vFactor, bool chaosSmoothingEnabled, ninZaT3Pro_ChaosSmoothingMethod chaosSmoothingMethod, int chaosSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaT3Pro(input, mAType, period, tCount, vFactor, chaosSmoothingEnabled, chaosSmoothingMethod, chaosSmoothingPeriod, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaT3Pro ninZaT3Pro(ninZa_MAType mAType, int period, int tCount, double vFactor, bool chaosSmoothingEnabled, ninZaT3Pro_ChaosSmoothingMethod chaosSmoothingMethod, int chaosSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaT3Pro(Input, mAType, period, tCount, vFactor, chaosSmoothingEnabled, chaosSmoothingMethod, chaosSmoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaT3Pro ninZaT3Pro(ISeries<double> input , ninZa_MAType mAType, int period, int tCount, double vFactor, bool chaosSmoothingEnabled, ninZaT3Pro_ChaosSmoothingMethod chaosSmoothingMethod, int chaosSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaT3Pro(input, mAType, period, tCount, vFactor, chaosSmoothingEnabled, chaosSmoothingMethod, chaosSmoothingPeriod, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
