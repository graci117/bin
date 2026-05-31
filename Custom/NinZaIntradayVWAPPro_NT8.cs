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
		
		private ninZaIntradayVWAPPro[] cacheninZaIntradayVWAPPro;

		
		public ninZaIntradayVWAPPro ninZaIntradayVWAPPro(double volumePower, PriceType inputPrice, bool vWAPSmoothingEnabled, int vWAPSmoothingPeriod, ninZaIntradayVWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4)
		{
			return ninZaIntradayVWAPPro(Input, volumePower, inputPrice, vWAPSmoothingEnabled, vWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4);
		}


		
		public ninZaIntradayVWAPPro ninZaIntradayVWAPPro(ISeries<double> input, double volumePower, PriceType inputPrice, bool vWAPSmoothingEnabled, int vWAPSmoothingPeriod, ninZaIntradayVWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4)
		{
			if (cacheninZaIntradayVWAPPro != null)
				for (int idx = 0; idx < cacheninZaIntradayVWAPPro.Length; idx++)
					if (cacheninZaIntradayVWAPPro[idx].VolumePower == volumePower && cacheninZaIntradayVWAPPro[idx].InputPrice == inputPrice && cacheninZaIntradayVWAPPro[idx].VWAPSmoothingEnabled == vWAPSmoothingEnabled && cacheninZaIntradayVWAPPro[idx].VWAPSmoothingPeriod == vWAPSmoothingPeriod && cacheninZaIntradayVWAPPro[idx].OffsetType == offsetType && cacheninZaIntradayVWAPPro[idx].StdDevMultiplerS1R1 == stdDevMultiplerS1R1 && cacheninZaIntradayVWAPPro[idx].StdDevMultiplerS2R2 == stdDevMultiplerS2R2 && cacheninZaIntradayVWAPPro[idx].StdDevMultiplerS3R3 == stdDevMultiplerS3R3 && cacheninZaIntradayVWAPPro[idx].StdDevMultiplerS4R4 == stdDevMultiplerS4R4 && cacheninZaIntradayVWAPPro[idx].DayMultiplerS1R1 == dayMultiplerS1R1 && cacheninZaIntradayVWAPPro[idx].DayMultiplerS2R2 == dayMultiplerS2R2 && cacheninZaIntradayVWAPPro[idx].DayMultiplerS3R3 == dayMultiplerS3R3 && cacheninZaIntradayVWAPPro[idx].DayMultiplerS4R4 == dayMultiplerS4R4 && cacheninZaIntradayVWAPPro[idx].BarMultiplerS1R1 == barMultiplerS1R1 && cacheninZaIntradayVWAPPro[idx].BarMultiplerS2R2 == barMultiplerS2R2 && cacheninZaIntradayVWAPPro[idx].BarMultiplerS3R3 == barMultiplerS3R3 && cacheninZaIntradayVWAPPro[idx].BarMultiplerS4R4 == barMultiplerS4R4 && cacheninZaIntradayVWAPPro[idx].EqualsInput(input))
						return cacheninZaIntradayVWAPPro[idx];
			return CacheIndicator<ninZaIntradayVWAPPro>(new ninZaIntradayVWAPPro(){ VolumePower = volumePower, InputPrice = inputPrice, VWAPSmoothingEnabled = vWAPSmoothingEnabled, VWAPSmoothingPeriod = vWAPSmoothingPeriod, OffsetType = offsetType, StdDevMultiplerS1R1 = stdDevMultiplerS1R1, StdDevMultiplerS2R2 = stdDevMultiplerS2R2, StdDevMultiplerS3R3 = stdDevMultiplerS3R3, StdDevMultiplerS4R4 = stdDevMultiplerS4R4, DayMultiplerS1R1 = dayMultiplerS1R1, DayMultiplerS2R2 = dayMultiplerS2R2, DayMultiplerS3R3 = dayMultiplerS3R3, DayMultiplerS4R4 = dayMultiplerS4R4, BarMultiplerS1R1 = barMultiplerS1R1, BarMultiplerS2R2 = barMultiplerS2R2, BarMultiplerS3R3 = barMultiplerS3R3, BarMultiplerS4R4 = barMultiplerS4R4 }, input, ref cacheninZaIntradayVWAPPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaIntradayVWAPPro ninZaIntradayVWAPPro(double volumePower, PriceType inputPrice, bool vWAPSmoothingEnabled, int vWAPSmoothingPeriod, ninZaIntradayVWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4)
		{
			return indicator.ninZaIntradayVWAPPro(Input, volumePower, inputPrice, vWAPSmoothingEnabled, vWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4);
		}


		
		public Indicators.ninZaIntradayVWAPPro ninZaIntradayVWAPPro(ISeries<double> input , double volumePower, PriceType inputPrice, bool vWAPSmoothingEnabled, int vWAPSmoothingPeriod, ninZaIntradayVWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4)
		{
			return indicator.ninZaIntradayVWAPPro(input, volumePower, inputPrice, vWAPSmoothingEnabled, vWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaIntradayVWAPPro ninZaIntradayVWAPPro(double volumePower, PriceType inputPrice, bool vWAPSmoothingEnabled, int vWAPSmoothingPeriod, ninZaIntradayVWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4)
		{
			return indicator.ninZaIntradayVWAPPro(Input, volumePower, inputPrice, vWAPSmoothingEnabled, vWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4);
		}


		
		public Indicators.ninZaIntradayVWAPPro ninZaIntradayVWAPPro(ISeries<double> input , double volumePower, PriceType inputPrice, bool vWAPSmoothingEnabled, int vWAPSmoothingPeriod, ninZaIntradayVWAPPro_OffsetType offsetType, double stdDevMultiplerS1R1, double stdDevMultiplerS2R2, double stdDevMultiplerS3R3, double stdDevMultiplerS4R4, double dayMultiplerS1R1, double dayMultiplerS2R2, double dayMultiplerS3R3, double dayMultiplerS4R4, double barMultiplerS1R1, double barMultiplerS2R2, double barMultiplerS3R3, double barMultiplerS4R4)
		{
			return indicator.ninZaIntradayVWAPPro(input, volumePower, inputPrice, vWAPSmoothingEnabled, vWAPSmoothingPeriod, offsetType, stdDevMultiplerS1R1, stdDevMultiplerS2R2, stdDevMultiplerS3R3, stdDevMultiplerS4R4, dayMultiplerS1R1, dayMultiplerS2R2, dayMultiplerS3R3, dayMultiplerS4R4, barMultiplerS1R1, barMultiplerS2R2, barMultiplerS3R3, barMultiplerS4R4);
		}

	}
}

#endregion
