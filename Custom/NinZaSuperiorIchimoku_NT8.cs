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
		
		private ninZaSuperiorIchimoku[] cacheninZaSuperiorIchimoku;

		
		public ninZaSuperiorIchimoku ninZaSuperiorIchimoku(int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double stateToleranceMultipler, int stateToleranceATRPeriod)
		{
			return ninZaSuperiorIchimoku(Input, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, stateToleranceMultipler, stateToleranceATRPeriod);
		}


		
		public ninZaSuperiorIchimoku ninZaSuperiorIchimoku(ISeries<double> input, int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double stateToleranceMultipler, int stateToleranceATRPeriod)
		{
			if (cacheninZaSuperiorIchimoku != null)
				for (int idx = 0; idx < cacheninZaSuperiorIchimoku.Length; idx++)
					if (cacheninZaSuperiorIchimoku[idx].PeriodFast == periodFast && cacheninZaSuperiorIchimoku[idx].PeriodMedium == periodMedium && cacheninZaSuperiorIchimoku[idx].PeriodSlow == periodSlow && cacheninZaSuperiorIchimoku[idx].SmoothingEnabled == smoothingEnabled && cacheninZaSuperiorIchimoku[idx].SmoothingMethod == smoothingMethod && cacheninZaSuperiorIchimoku[idx].SmoothingPeriod == smoothingPeriod && cacheninZaSuperiorIchimoku[idx].StateToleranceMultipler == stateToleranceMultipler && cacheninZaSuperiorIchimoku[idx].StateToleranceATRPeriod == stateToleranceATRPeriod && cacheninZaSuperiorIchimoku[idx].EqualsInput(input))
						return cacheninZaSuperiorIchimoku[idx];
			return CacheIndicator<ninZaSuperiorIchimoku>(new ninZaSuperiorIchimoku(){ PeriodFast = periodFast, PeriodMedium = periodMedium, PeriodSlow = periodSlow, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, StateToleranceMultipler = stateToleranceMultipler, StateToleranceATRPeriod = stateToleranceATRPeriod }, input, ref cacheninZaSuperiorIchimoku);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorIchimoku ninZaSuperiorIchimoku(int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double stateToleranceMultipler, int stateToleranceATRPeriod)
		{
			return indicator.ninZaSuperiorIchimoku(Input, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, stateToleranceMultipler, stateToleranceATRPeriod);
		}


		
		public Indicators.ninZaSuperiorIchimoku ninZaSuperiorIchimoku(ISeries<double> input , int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double stateToleranceMultipler, int stateToleranceATRPeriod)
		{
			return indicator.ninZaSuperiorIchimoku(input, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, stateToleranceMultipler, stateToleranceATRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorIchimoku ninZaSuperiorIchimoku(int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double stateToleranceMultipler, int stateToleranceATRPeriod)
		{
			return indicator.ninZaSuperiorIchimoku(Input, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, stateToleranceMultipler, stateToleranceATRPeriod);
		}


		
		public Indicators.ninZaSuperiorIchimoku ninZaSuperiorIchimoku(ISeries<double> input , int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double stateToleranceMultipler, int stateToleranceATRPeriod)
		{
			return indicator.ninZaSuperiorIchimoku(input, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, stateToleranceMultipler, stateToleranceATRPeriod);
		}

	}
}

#endregion
