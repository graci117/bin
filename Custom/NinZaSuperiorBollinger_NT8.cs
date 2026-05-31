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
		
		private ninZaSuperiorBollinger[] cacheninZaSuperiorBollinger;

		
		public ninZaSuperiorBollinger ninZaSuperiorBollinger(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return ninZaSuperiorBollinger(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}


		
		public ninZaSuperiorBollinger ninZaSuperiorBollinger(ISeries<double> input, ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			if (cacheninZaSuperiorBollinger != null)
				for (int idx = 0; idx < cacheninZaSuperiorBollinger.Length; idx++)
					if (cacheninZaSuperiorBollinger[idx].MAType == mAType && cacheninZaSuperiorBollinger[idx].Period == period && cacheninZaSuperiorBollinger[idx].SmoothingEnabled == smoothingEnabled && cacheninZaSuperiorBollinger[idx].SmoothingMethod == smoothingMethod && cacheninZaSuperiorBollinger[idx].SmoothingPeriod == smoothingPeriod && cacheninZaSuperiorBollinger[idx].StandardDeviation == standardDeviation && cacheninZaSuperiorBollinger[idx].EqualsInput(input))
						return cacheninZaSuperiorBollinger[idx];
			return CacheIndicator<ninZaSuperiorBollinger>(new ninZaSuperiorBollinger(){ MAType = mAType, Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, StandardDeviation = standardDeviation }, input, ref cacheninZaSuperiorBollinger);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorBollinger ninZaSuperiorBollinger(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaSuperiorBollinger(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}


		
		public Indicators.ninZaSuperiorBollinger ninZaSuperiorBollinger(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaSuperiorBollinger(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorBollinger ninZaSuperiorBollinger(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaSuperiorBollinger(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}


		
		public Indicators.ninZaSuperiorBollinger ninZaSuperiorBollinger(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaSuperiorBollinger(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}

	}
}

#endregion
