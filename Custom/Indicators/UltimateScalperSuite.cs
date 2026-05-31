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
		
		private UltimateAI[] cacheUltimateAI;
		private UltimateBackdoor1v2[] cacheUltimateBackdoor1v2;
		private WiZigZagHighLow_TOS[] cacheWiZigZagHighLow_TOS;
		private StochasticFull_TOS[] cacheStochasticFull_TOS;
		private WilderMA[] cacheWilderMA;

		
		public UltimateAI UltimateAI()
		{
			return UltimateAI(Input);
		}

		public UltimateBackdoor1v2 UltimateBackdoor1v2(int breakout_lines_max_bax_bars_look_back, int number_of_cons_highs_lows, int high_buffer_ticks, int low_buffer_ticks, bool pmEnableMACDSoundAlert, bool pmEnableSoundAlert, bool pmEnablePopUpAlert, string pmAlertSoundFile)
		{
			return UltimateBackdoor1v2(Input, breakout_lines_max_bax_bars_look_back, number_of_cons_highs_lows, high_buffer_ticks, low_buffer_ticks, pmEnableMACDSoundAlert, pmEnableSoundAlert, pmEnablePopUpAlert, pmAlertSoundFile);
		}

		public WiZigZagHighLow_TOS WiZigZagHighLow_TOS(WiZigZagHighLow_TOS_PriceTypeHL priceH, WiZigZagHighLow_TOS_PriceTypeHL priceL, double percentageReversal, double absoluteReversal, int atrLength, double atrReversal, int tickReversal)
		{
			return WiZigZagHighLow_TOS(Input, priceH, priceL, percentageReversal, absoluteReversal, atrLength, atrReversal, tickReversal);
		}

		public StochasticFull_TOS StochasticFull_TOS(int overBought, int overSold, int kPeriod, int dPeriod, StochasticFull_Price priceH, StochasticFull_Price priceL, StochasticFull_Price priceC, int slowingPeriod, StochasticFull_AverageType averageType)
		{
			return StochasticFull_TOS(Input, overBought, overSold, kPeriod, dPeriod, priceH, priceL, priceC, slowingPeriod, averageType);
		}

		public WilderMA WilderMA(int period)
		{
			return WilderMA(Input, period);
		}


		
		public UltimateAI UltimateAI(ISeries<double> input)
		{
			if (cacheUltimateAI != null)
				for (int idx = 0; idx < cacheUltimateAI.Length; idx++)
					if ( cacheUltimateAI[idx].EqualsInput(input))
						return cacheUltimateAI[idx];
			return CacheIndicator<UltimateAI>(new UltimateAI(), input, ref cacheUltimateAI);
		}

		public UltimateBackdoor1v2 UltimateBackdoor1v2(ISeries<double> input, int breakout_lines_max_bax_bars_look_back, int number_of_cons_highs_lows, int high_buffer_ticks, int low_buffer_ticks, bool pmEnableMACDSoundAlert, bool pmEnableSoundAlert, bool pmEnablePopUpAlert, string pmAlertSoundFile)
		{
			if (cacheUltimateBackdoor1v2 != null)
				for (int idx = 0; idx < cacheUltimateBackdoor1v2.Length; idx++)
					if (cacheUltimateBackdoor1v2[idx].breakout_lines_max_bax_bars_look_back == breakout_lines_max_bax_bars_look_back && cacheUltimateBackdoor1v2[idx].number_of_cons_highs_lows == number_of_cons_highs_lows && cacheUltimateBackdoor1v2[idx].high_buffer_ticks == high_buffer_ticks && cacheUltimateBackdoor1v2[idx].low_buffer_ticks == low_buffer_ticks && cacheUltimateBackdoor1v2[idx].pmEnableMACDSoundAlert == pmEnableMACDSoundAlert && cacheUltimateBackdoor1v2[idx].pmEnableSoundAlert == pmEnableSoundAlert && cacheUltimateBackdoor1v2[idx].pmEnablePopUpAlert == pmEnablePopUpAlert && cacheUltimateBackdoor1v2[idx].pmAlertSoundFile == pmAlertSoundFile && cacheUltimateBackdoor1v2[idx].EqualsInput(input))
						return cacheUltimateBackdoor1v2[idx];
			return CacheIndicator<UltimateBackdoor1v2>(new UltimateBackdoor1v2(){ breakout_lines_max_bax_bars_look_back = breakout_lines_max_bax_bars_look_back, number_of_cons_highs_lows = number_of_cons_highs_lows, high_buffer_ticks = high_buffer_ticks, low_buffer_ticks = low_buffer_ticks, pmEnableMACDSoundAlert = pmEnableMACDSoundAlert, pmEnableSoundAlert = pmEnableSoundAlert, pmEnablePopUpAlert = pmEnablePopUpAlert, pmAlertSoundFile = pmAlertSoundFile }, input, ref cacheUltimateBackdoor1v2);
		}

		public WiZigZagHighLow_TOS WiZigZagHighLow_TOS(ISeries<double> input, WiZigZagHighLow_TOS_PriceTypeHL priceH, WiZigZagHighLow_TOS_PriceTypeHL priceL, double percentageReversal, double absoluteReversal, int atrLength, double atrReversal, int tickReversal)
		{
			if (cacheWiZigZagHighLow_TOS != null)
				for (int idx = 0; idx < cacheWiZigZagHighLow_TOS.Length; idx++)
					if (cacheWiZigZagHighLow_TOS[idx].PriceH == priceH && cacheWiZigZagHighLow_TOS[idx].PriceL == priceL && cacheWiZigZagHighLow_TOS[idx].percentageReversal == percentageReversal && cacheWiZigZagHighLow_TOS[idx].absoluteReversal == absoluteReversal && cacheWiZigZagHighLow_TOS[idx].atrLength == atrLength && cacheWiZigZagHighLow_TOS[idx].atrReversal == atrReversal && cacheWiZigZagHighLow_TOS[idx].tickReversal == tickReversal && cacheWiZigZagHighLow_TOS[idx].EqualsInput(input))
						return cacheWiZigZagHighLow_TOS[idx];
			return CacheIndicator<WiZigZagHighLow_TOS>(new WiZigZagHighLow_TOS(){ PriceH = priceH, PriceL = priceL, percentageReversal = percentageReversal, absoluteReversal = absoluteReversal, atrLength = atrLength, atrReversal = atrReversal, tickReversal = tickReversal }, input, ref cacheWiZigZagHighLow_TOS);
		}

		public StochasticFull_TOS StochasticFull_TOS(ISeries<double> input, int overBought, int overSold, int kPeriod, int dPeriod, StochasticFull_Price priceH, StochasticFull_Price priceL, StochasticFull_Price priceC, int slowingPeriod, StochasticFull_AverageType averageType)
		{
			if (cacheStochasticFull_TOS != null)
				for (int idx = 0; idx < cacheStochasticFull_TOS.Length; idx++)
					if (cacheStochasticFull_TOS[idx].OverBought == overBought && cacheStochasticFull_TOS[idx].OverSold == overSold && cacheStochasticFull_TOS[idx].KPeriod == kPeriod && cacheStochasticFull_TOS[idx].DPeriod == dPeriod && cacheStochasticFull_TOS[idx].priceH == priceH && cacheStochasticFull_TOS[idx].priceL == priceL && cacheStochasticFull_TOS[idx].priceC == priceC && cacheStochasticFull_TOS[idx].SlowingPeriod == slowingPeriod && cacheStochasticFull_TOS[idx].averageType == averageType && cacheStochasticFull_TOS[idx].EqualsInput(input))
						return cacheStochasticFull_TOS[idx];
			return CacheIndicator<StochasticFull_TOS>(new StochasticFull_TOS(){ OverBought = overBought, OverSold = overSold, KPeriod = kPeriod, DPeriod = dPeriod, priceH = priceH, priceL = priceL, priceC = priceC, SlowingPeriod = slowingPeriod, averageType = averageType }, input, ref cacheStochasticFull_TOS);
		}

		public WilderMA WilderMA(ISeries<double> input, int period)
		{
			if (cacheWilderMA != null)
				for (int idx = 0; idx < cacheWilderMA.Length; idx++)
					if (cacheWilderMA[idx].Period == period && cacheWilderMA[idx].EqualsInput(input))
						return cacheWilderMA[idx];
			return CacheIndicator<WilderMA>(new WilderMA(){ Period = period }, input, ref cacheWilderMA);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.UltimateAI UltimateAI()
		{
			return indicator.UltimateAI(Input);
		}

		public Indicators.UltimateBackdoor1v2 UltimateBackdoor1v2(int breakout_lines_max_bax_bars_look_back, int number_of_cons_highs_lows, int high_buffer_ticks, int low_buffer_ticks, bool pmEnableMACDSoundAlert, bool pmEnableSoundAlert, bool pmEnablePopUpAlert, string pmAlertSoundFile)
		{
			return indicator.UltimateBackdoor1v2(Input, breakout_lines_max_bax_bars_look_back, number_of_cons_highs_lows, high_buffer_ticks, low_buffer_ticks, pmEnableMACDSoundAlert, pmEnableSoundAlert, pmEnablePopUpAlert, pmAlertSoundFile);
		}

		public Indicators.WiZigZagHighLow_TOS WiZigZagHighLow_TOS(WiZigZagHighLow_TOS_PriceTypeHL priceH, WiZigZagHighLow_TOS_PriceTypeHL priceL, double percentageReversal, double absoluteReversal, int atrLength, double atrReversal, int tickReversal)
		{
			return indicator.WiZigZagHighLow_TOS(Input, priceH, priceL, percentageReversal, absoluteReversal, atrLength, atrReversal, tickReversal);
		}

		public Indicators.StochasticFull_TOS StochasticFull_TOS(int overBought, int overSold, int kPeriod, int dPeriod, StochasticFull_Price priceH, StochasticFull_Price priceL, StochasticFull_Price priceC, int slowingPeriod, StochasticFull_AverageType averageType)
		{
			return indicator.StochasticFull_TOS(Input, overBought, overSold, kPeriod, dPeriod, priceH, priceL, priceC, slowingPeriod, averageType);
		}

		public Indicators.WilderMA WilderMA(int period)
		{
			return indicator.WilderMA(Input, period);
		}


		
		public Indicators.UltimateAI UltimateAI(ISeries<double> input )
		{
			return indicator.UltimateAI(input);
		}

		public Indicators.UltimateBackdoor1v2 UltimateBackdoor1v2(ISeries<double> input , int breakout_lines_max_bax_bars_look_back, int number_of_cons_highs_lows, int high_buffer_ticks, int low_buffer_ticks, bool pmEnableMACDSoundAlert, bool pmEnableSoundAlert, bool pmEnablePopUpAlert, string pmAlertSoundFile)
		{
			return indicator.UltimateBackdoor1v2(input, breakout_lines_max_bax_bars_look_back, number_of_cons_highs_lows, high_buffer_ticks, low_buffer_ticks, pmEnableMACDSoundAlert, pmEnableSoundAlert, pmEnablePopUpAlert, pmAlertSoundFile);
		}

		public Indicators.WiZigZagHighLow_TOS WiZigZagHighLow_TOS(ISeries<double> input , WiZigZagHighLow_TOS_PriceTypeHL priceH, WiZigZagHighLow_TOS_PriceTypeHL priceL, double percentageReversal, double absoluteReversal, int atrLength, double atrReversal, int tickReversal)
		{
			return indicator.WiZigZagHighLow_TOS(input, priceH, priceL, percentageReversal, absoluteReversal, atrLength, atrReversal, tickReversal);
		}

		public Indicators.StochasticFull_TOS StochasticFull_TOS(ISeries<double> input , int overBought, int overSold, int kPeriod, int dPeriod, StochasticFull_Price priceH, StochasticFull_Price priceL, StochasticFull_Price priceC, int slowingPeriod, StochasticFull_AverageType averageType)
		{
			return indicator.StochasticFull_TOS(input, overBought, overSold, kPeriod, dPeriod, priceH, priceL, priceC, slowingPeriod, averageType);
		}

		public Indicators.WilderMA WilderMA(ISeries<double> input , int period)
		{
			return indicator.WilderMA(input, period);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.UltimateAI UltimateAI()
		{
			return indicator.UltimateAI(Input);
		}

		public Indicators.UltimateBackdoor1v2 UltimateBackdoor1v2(int breakout_lines_max_bax_bars_look_back, int number_of_cons_highs_lows, int high_buffer_ticks, int low_buffer_ticks, bool pmEnableMACDSoundAlert, bool pmEnableSoundAlert, bool pmEnablePopUpAlert, string pmAlertSoundFile)
		{
			return indicator.UltimateBackdoor1v2(Input, breakout_lines_max_bax_bars_look_back, number_of_cons_highs_lows, high_buffer_ticks, low_buffer_ticks, pmEnableMACDSoundAlert, pmEnableSoundAlert, pmEnablePopUpAlert, pmAlertSoundFile);
		}

		public Indicators.WiZigZagHighLow_TOS WiZigZagHighLow_TOS(WiZigZagHighLow_TOS_PriceTypeHL priceH, WiZigZagHighLow_TOS_PriceTypeHL priceL, double percentageReversal, double absoluteReversal, int atrLength, double atrReversal, int tickReversal)
		{
			return indicator.WiZigZagHighLow_TOS(Input, priceH, priceL, percentageReversal, absoluteReversal, atrLength, atrReversal, tickReversal);
		}

		public Indicators.StochasticFull_TOS StochasticFull_TOS(int overBought, int overSold, int kPeriod, int dPeriod, StochasticFull_Price priceH, StochasticFull_Price priceL, StochasticFull_Price priceC, int slowingPeriod, StochasticFull_AverageType averageType)
		{
			return indicator.StochasticFull_TOS(Input, overBought, overSold, kPeriod, dPeriod, priceH, priceL, priceC, slowingPeriod, averageType);
		}

		public Indicators.WilderMA WilderMA(int period)
		{
			return indicator.WilderMA(Input, period);
		}


		
		public Indicators.UltimateAI UltimateAI(ISeries<double> input )
		{
			return indicator.UltimateAI(input);
		}

		public Indicators.UltimateBackdoor1v2 UltimateBackdoor1v2(ISeries<double> input , int breakout_lines_max_bax_bars_look_back, int number_of_cons_highs_lows, int high_buffer_ticks, int low_buffer_ticks, bool pmEnableMACDSoundAlert, bool pmEnableSoundAlert, bool pmEnablePopUpAlert, string pmAlertSoundFile)
		{
			return indicator.UltimateBackdoor1v2(input, breakout_lines_max_bax_bars_look_back, number_of_cons_highs_lows, high_buffer_ticks, low_buffer_ticks, pmEnableMACDSoundAlert, pmEnableSoundAlert, pmEnablePopUpAlert, pmAlertSoundFile);
		}

		public Indicators.WiZigZagHighLow_TOS WiZigZagHighLow_TOS(ISeries<double> input , WiZigZagHighLow_TOS_PriceTypeHL priceH, WiZigZagHighLow_TOS_PriceTypeHL priceL, double percentageReversal, double absoluteReversal, int atrLength, double atrReversal, int tickReversal)
		{
			return indicator.WiZigZagHighLow_TOS(input, priceH, priceL, percentageReversal, absoluteReversal, atrLength, atrReversal, tickReversal);
		}

		public Indicators.StochasticFull_TOS StochasticFull_TOS(ISeries<double> input , int overBought, int overSold, int kPeriod, int dPeriod, StochasticFull_Price priceH, StochasticFull_Price priceL, StochasticFull_Price priceC, int slowingPeriod, StochasticFull_AverageType averageType)
		{
			return indicator.StochasticFull_TOS(input, overBought, overSold, kPeriod, dPeriod, priceH, priceL, priceC, slowingPeriod, averageType);
		}

		public Indicators.WilderMA WilderMA(ISeries<double> input , int period)
		{
			return indicator.WilderMA(input, period);
		}

	}
}

#endregion
