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
		
		private RenkoKings.RenkoKings_ThunderZilla[] cacheRenkoKings_ThunderZilla;

		
		public RenkoKings.RenkoKings_ThunderZilla RenkoKings_ThunderZilla(ninZa_MAType trendMAType, int trendPeriod, bool trendSmoothingEnabled, ninZa_MAType trendSmoothingMethod, int trendSmoothingPeriod, double stopOffsetMultiplierStop, int signalQuantityPerFlat, int signalQuantityPerTrend)
		{
			return RenkoKings_ThunderZilla(Input, trendMAType, trendPeriod, trendSmoothingEnabled, trendSmoothingMethod, trendSmoothingPeriod, stopOffsetMultiplierStop, signalQuantityPerFlat, signalQuantityPerTrend);
		}


		
		public RenkoKings.RenkoKings_ThunderZilla RenkoKings_ThunderZilla(ISeries<double> input, ninZa_MAType trendMAType, int trendPeriod, bool trendSmoothingEnabled, ninZa_MAType trendSmoothingMethod, int trendSmoothingPeriod, double stopOffsetMultiplierStop, int signalQuantityPerFlat, int signalQuantityPerTrend)
		{
			if (cacheRenkoKings_ThunderZilla != null)
				for (int idx = 0; idx < cacheRenkoKings_ThunderZilla.Length; idx++)
					if (cacheRenkoKings_ThunderZilla[idx].TrendMAType == trendMAType && cacheRenkoKings_ThunderZilla[idx].TrendPeriod == trendPeriod && cacheRenkoKings_ThunderZilla[idx].TrendSmoothingEnabled == trendSmoothingEnabled && cacheRenkoKings_ThunderZilla[idx].TrendSmoothingMethod == trendSmoothingMethod && cacheRenkoKings_ThunderZilla[idx].TrendSmoothingPeriod == trendSmoothingPeriod && cacheRenkoKings_ThunderZilla[idx].StopOffsetMultiplierStop == stopOffsetMultiplierStop && cacheRenkoKings_ThunderZilla[idx].SignalQuantityPerFlat == signalQuantityPerFlat && cacheRenkoKings_ThunderZilla[idx].SignalQuantityPerTrend == signalQuantityPerTrend && cacheRenkoKings_ThunderZilla[idx].EqualsInput(input))
						return cacheRenkoKings_ThunderZilla[idx];
			return CacheIndicator<RenkoKings.RenkoKings_ThunderZilla>(new RenkoKings.RenkoKings_ThunderZilla(){ TrendMAType = trendMAType, TrendPeriod = trendPeriod, TrendSmoothingEnabled = trendSmoothingEnabled, TrendSmoothingMethod = trendSmoothingMethod, TrendSmoothingPeriod = trendSmoothingPeriod, StopOffsetMultiplierStop = stopOffsetMultiplierStop, SignalQuantityPerFlat = signalQuantityPerFlat, SignalQuantityPerTrend = signalQuantityPerTrend }, input, ref cacheRenkoKings_ThunderZilla);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.RenkoKings.RenkoKings_ThunderZilla RenkoKings_ThunderZilla(ninZa_MAType trendMAType, int trendPeriod, bool trendSmoothingEnabled, ninZa_MAType trendSmoothingMethod, int trendSmoothingPeriod, double stopOffsetMultiplierStop, int signalQuantityPerFlat, int signalQuantityPerTrend)
		{
			return indicator.RenkoKings_ThunderZilla(Input, trendMAType, trendPeriod, trendSmoothingEnabled, trendSmoothingMethod, trendSmoothingPeriod, stopOffsetMultiplierStop, signalQuantityPerFlat, signalQuantityPerTrend);
		}


		
		public Indicators.RenkoKings.RenkoKings_ThunderZilla RenkoKings_ThunderZilla(ISeries<double> input , ninZa_MAType trendMAType, int trendPeriod, bool trendSmoothingEnabled, ninZa_MAType trendSmoothingMethod, int trendSmoothingPeriod, double stopOffsetMultiplierStop, int signalQuantityPerFlat, int signalQuantityPerTrend)
		{
			return indicator.RenkoKings_ThunderZilla(input, trendMAType, trendPeriod, trendSmoothingEnabled, trendSmoothingMethod, trendSmoothingPeriod, stopOffsetMultiplierStop, signalQuantityPerFlat, signalQuantityPerTrend);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.RenkoKings.RenkoKings_ThunderZilla RenkoKings_ThunderZilla(ninZa_MAType trendMAType, int trendPeriod, bool trendSmoothingEnabled, ninZa_MAType trendSmoothingMethod, int trendSmoothingPeriod, double stopOffsetMultiplierStop, int signalQuantityPerFlat, int signalQuantityPerTrend)
		{
			return indicator.RenkoKings_ThunderZilla(Input, trendMAType, trendPeriod, trendSmoothingEnabled, trendSmoothingMethod, trendSmoothingPeriod, stopOffsetMultiplierStop, signalQuantityPerFlat, signalQuantityPerTrend);
		}


		
		public Indicators.RenkoKings.RenkoKings_ThunderZilla RenkoKings_ThunderZilla(ISeries<double> input , ninZa_MAType trendMAType, int trendPeriod, bool trendSmoothingEnabled, ninZa_MAType trendSmoothingMethod, int trendSmoothingPeriod, double stopOffsetMultiplierStop, int signalQuantityPerFlat, int signalQuantityPerTrend)
		{
			return indicator.RenkoKings_ThunderZilla(input, trendMAType, trendPeriod, trendSmoothingEnabled, trendSmoothingMethod, trendSmoothingPeriod, stopOffsetMultiplierStop, signalQuantityPerFlat, signalQuantityPerTrend);
		}

	}
}

#endregion
