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
		
		private ninZaIntradayRWAPPro[] cacheninZaIntradayRWAPPro;

		
		public ninZaIntradayRWAPPro ninZaIntradayRWAPPro(double rangePower, PriceType inputPrice, bool rWAPSmoothingEnabled, int rWAPSmoothingPeriod, ninZaIntradayRWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4, bool time1Enabled, int time1Start, int time1Duration, int time1WeightPercentage, bool time2Enabled, int time2Start, int time2Duration, int time2WeightPercentage)
		{
			return ninZaIntradayRWAPPro(Input, rangePower, inputPrice, rWAPSmoothingEnabled, rWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4, time1Enabled, time1Start, time1Duration, time1WeightPercentage, time2Enabled, time2Start, time2Duration, time2WeightPercentage);
		}


		
		public ninZaIntradayRWAPPro ninZaIntradayRWAPPro(ISeries<double> input, double rangePower, PriceType inputPrice, bool rWAPSmoothingEnabled, int rWAPSmoothingPeriod, ninZaIntradayRWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4, bool time1Enabled, int time1Start, int time1Duration, int time1WeightPercentage, bool time2Enabled, int time2Start, int time2Duration, int time2WeightPercentage)
		{
			if (cacheninZaIntradayRWAPPro != null)
				for (int idx = 0; idx < cacheninZaIntradayRWAPPro.Length; idx++)
					if (cacheninZaIntradayRWAPPro[idx].RangePower == rangePower && cacheninZaIntradayRWAPPro[idx].InputPrice == inputPrice && cacheninZaIntradayRWAPPro[idx].RWAPSmoothingEnabled == rWAPSmoothingEnabled && cacheninZaIntradayRWAPPro[idx].RWAPSmoothingPeriod == rWAPSmoothingPeriod && cacheninZaIntradayRWAPPro[idx].OffsetType == offsetType && cacheninZaIntradayRWAPPro[idx].StdDevMultiplerS1R1 == stdDevMultiplerS1R1 && cacheninZaIntradayRWAPPro[idx].StdDevMultiplerS2R2 == stdDevMultiplerS2R2 && cacheninZaIntradayRWAPPro[idx].StdDevMultiplerS3R3 == stdDevMultiplerS3R3 && cacheninZaIntradayRWAPPro[idx].StdDevMultiplerS4R4 == stdDevMultiplerS4R4 && cacheninZaIntradayRWAPPro[idx].DayMultiplerS1R1 == dayMultiplerS1R1 && cacheninZaIntradayRWAPPro[idx].DayMultiplerS2R2 == dayMultiplerS2R2 && cacheninZaIntradayRWAPPro[idx].DayMultiplerS3R3 == dayMultiplerS3R3 && cacheninZaIntradayRWAPPro[idx].DayMultiplerS4R4 == dayMultiplerS4R4 && cacheninZaIntradayRWAPPro[idx].BarMultiplerS1R1 == barMultiplerS1R1 && cacheninZaIntradayRWAPPro[idx].BarMultiplerS2R2 == barMultiplerS2R2 && cacheninZaIntradayRWAPPro[idx].BarMultiplerS3R3 == barMultiplerS3R3 && cacheninZaIntradayRWAPPro[idx].BarMultiplerS4R4 == barMultiplerS4R4 && cacheninZaIntradayRWAPPro[idx].Time1Enabled == time1Enabled && cacheninZaIntradayRWAPPro[idx].Time1Start == time1Start && cacheninZaIntradayRWAPPro[idx].Time1Duration == time1Duration && cacheninZaIntradayRWAPPro[idx].Time1WeightPercentage == time1WeightPercentage && cacheninZaIntradayRWAPPro[idx].Time2Enabled == time2Enabled && cacheninZaIntradayRWAPPro[idx].Time2Start == time2Start && cacheninZaIntradayRWAPPro[idx].Time2Duration == time2Duration && cacheninZaIntradayRWAPPro[idx].Time2WeightPercentage == time2WeightPercentage && cacheninZaIntradayRWAPPro[idx].EqualsInput(input))
						return cacheninZaIntradayRWAPPro[idx];
			return CacheIndicator<ninZaIntradayRWAPPro>(new ninZaIntradayRWAPPro(){ RangePower = rangePower, InputPrice = inputPrice, RWAPSmoothingEnabled = rWAPSmoothingEnabled, RWAPSmoothingPeriod = rWAPSmoothingPeriod, OffsetType = offsetType, StdDevMultiplerS1R1 = stdDevMultiplerS1R1, StdDevMultiplerS2R2 = stdDevMultiplerS2R2, StdDevMultiplerS3R3 = stdDevMultiplerS3R3, StdDevMultiplerS4R4 = stdDevMultiplerS4R4, DayMultiplerS1R1 = dayMultiplerS1R1, DayMultiplerS2R2 = dayMultiplerS2R2, DayMultiplerS3R3 = dayMultiplerS3R3, DayMultiplerS4R4 = dayMultiplerS4R4, BarMultiplerS1R1 = barMultiplerS1R1, BarMultiplerS2R2 = barMultiplerS2R2, BarMultiplerS3R3 = barMultiplerS3R3, BarMultiplerS4R4 = barMultiplerS4R4, Time1Enabled = time1Enabled, Time1Start = time1Start, Time1Duration = time1Duration, Time1WeightPercentage = time1WeightPercentage, Time2Enabled = time2Enabled, Time2Start = time2Start, Time2Duration = time2Duration, Time2WeightPercentage = time2WeightPercentage }, input, ref cacheninZaIntradayRWAPPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaIntradayRWAPPro ninZaIntradayRWAPPro(double rangePower, PriceType inputPrice, bool rWAPSmoothingEnabled, int rWAPSmoothingPeriod, ninZaIntradayRWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4, bool time1Enabled, int time1Start, int time1Duration, int time1WeightPercentage, bool time2Enabled, int time2Start, int time2Duration, int time2WeightPercentage)
		{
			return indicator.ninZaIntradayRWAPPro(Input, rangePower, inputPrice, rWAPSmoothingEnabled, rWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4, time1Enabled, time1Start, time1Duration, time1WeightPercentage, time2Enabled, time2Start, time2Duration, time2WeightPercentage);
		}


		
		public Indicators.ninZaIntradayRWAPPro ninZaIntradayRWAPPro(ISeries<double> input , double rangePower, PriceType inputPrice, bool rWAPSmoothingEnabled, int rWAPSmoothingPeriod, ninZaIntradayRWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4, bool time1Enabled, int time1Start, int time1Duration, int time1WeightPercentage, bool time2Enabled, int time2Start, int time2Duration, int time2WeightPercentage)
		{
			return indicator.ninZaIntradayRWAPPro(input, rangePower, inputPrice, rWAPSmoothingEnabled, rWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4, time1Enabled, time1Start, time1Duration, time1WeightPercentage, time2Enabled, time2Start, time2Duration, time2WeightPercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaIntradayRWAPPro ninZaIntradayRWAPPro(double rangePower, PriceType inputPrice, bool rWAPSmoothingEnabled, int rWAPSmoothingPeriod, ninZaIntradayRWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4, bool time1Enabled, int time1Start, int time1Duration, int time1WeightPercentage, bool time2Enabled, int time2Start, int time2Duration, int time2WeightPercentage)
		{
			return indicator.ninZaIntradayRWAPPro(Input, rangePower, inputPrice, rWAPSmoothingEnabled, rWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4, time1Enabled, time1Start, time1Duration, time1WeightPercentage, time2Enabled, time2Start, time2Duration, time2WeightPercentage);
		}


		
		public Indicators.ninZaIntradayRWAPPro ninZaIntradayRWAPPro(ISeries<double> input , double rangePower, PriceType inputPrice, bool rWAPSmoothingEnabled, int rWAPSmoothingPeriod, ninZaIntradayRWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4, bool time1Enabled, int time1Start, int time1Duration, int time1WeightPercentage, bool time2Enabled, int time2Start, int time2Duration, int time2WeightPercentage)
		{
			return indicator.ninZaIntradayRWAPPro(input, rangePower, inputPrice, rWAPSmoothingEnabled, rWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4, time1Enabled, time1Start, time1Duration, time1WeightPercentage, time2Enabled, time2Start, time2Duration, time2WeightPercentage);
		}

	}
}

#endregion
