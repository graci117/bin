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
		
		private ninZaConnorsRSIPro[] cacheninZaConnorsRSIPro;

		
		public ninZaConnorsRSIPro ninZaConnorsRSIPro(int rSIPeriod, int streakRSIPeriod, int percentRankLookback, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaConnorsRSIPro(Input, rSIPeriod, streakRSIPeriod, percentRankLookback, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaConnorsRSIPro ninZaConnorsRSIPro(ISeries<double> input, int rSIPeriod, int streakRSIPeriod, int percentRankLookback, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaConnorsRSIPro != null)
				for (int idx = 0; idx < cacheninZaConnorsRSIPro.Length; idx++)
					if (cacheninZaConnorsRSIPro[idx].RSIPeriod == rSIPeriod && cacheninZaConnorsRSIPro[idx].StreakRSIPeriod == streakRSIPeriod && cacheninZaConnorsRSIPro[idx].PercentRankLookback == percentRankLookback && cacheninZaConnorsRSIPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaConnorsRSIPro[idx].SmoothingMethod == smoothingMethod && cacheninZaConnorsRSIPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaConnorsRSIPro[idx].ThresholdOverbought == thresholdOverbought && cacheninZaConnorsRSIPro[idx].ThresholdOversold == thresholdOversold && cacheninZaConnorsRSIPro[idx].EqualsInput(input))
						return cacheninZaConnorsRSIPro[idx];
			return CacheIndicator<ninZaConnorsRSIPro>(new ninZaConnorsRSIPro(){ RSIPeriod = rSIPeriod, StreakRSIPeriod = streakRSIPeriod, PercentRankLookback = percentRankLookback, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaConnorsRSIPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaConnorsRSIPro ninZaConnorsRSIPro(int rSIPeriod, int streakRSIPeriod, int percentRankLookback, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaConnorsRSIPro(Input, rSIPeriod, streakRSIPeriod, percentRankLookback, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaConnorsRSIPro ninZaConnorsRSIPro(ISeries<double> input , int rSIPeriod, int streakRSIPeriod, int percentRankLookback, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaConnorsRSIPro(input, rSIPeriod, streakRSIPeriod, percentRankLookback, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaConnorsRSIPro ninZaConnorsRSIPro(int rSIPeriod, int streakRSIPeriod, int percentRankLookback, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaConnorsRSIPro(Input, rSIPeriod, streakRSIPeriod, percentRankLookback, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaConnorsRSIPro ninZaConnorsRSIPro(ISeries<double> input , int rSIPeriod, int streakRSIPeriod, int percentRankLookback, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaConnorsRSIPro(input, rSIPeriod, streakRSIPeriod, percentRankLookback, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
