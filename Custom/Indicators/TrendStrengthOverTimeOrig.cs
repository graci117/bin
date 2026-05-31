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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class TrendStrengthOverTimeOrig : Indicator
	{
		private Series<double> bullStrengthSeries;
		private Series<double> volumeStrengthSeries;
//		Percentile pct13;
//		Percentile pct13;
//		Percentile pct13;
//		Percentile pct13;
//		Percentile pct13;
		
//		Percentile pct13;
//		Percentile pct13;
//		Percentile pct13;
//		Percentile pct13;
//		Percentile pct13;
		
		Percentile pct;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TrendStrengthOverTimeOrig";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddLine(Brushes.Red, 3, "TrendStrength");
				AddLine(Brushes.PapayaWhip, 3, "VolumeStrength");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				bullStrengthSeries = new Series<double>(this);
				volumeStrengthSeries = new Series<double>(this);
				//pct = Percentile(Period
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 146)
				return;
			
				
			double percentile_13H = pct.GetPercentile(PriceType.High, 13, 75) ;
		
			double percentile_21H = pct.GetPercentile(PriceType.High, 21, 75) ;
			double percentile_34H = pct.GetPercentile(PriceType.High, 34, 75) ;
			double percentile_55H = pct.GetPercentile(PriceType.High, 55, 75) ;
			double percentile_89H = pct.GetPercentile(PriceType.High, 89, 75);

			// Calculate 25th percentile of  price for each length
			double percentile_13L =  pct.GetPercentile(PriceType.Low, 13, 25) ;
			double percentile_21L =  pct.GetPercentile(PriceType.Low, 21, 25) ;
			double percentile_34L =  pct.GetPercentile(PriceType.Low, 34, 25) ;
			double percentile_55L = pct.GetPercentile(PriceType.Low, 55, 25) ;
			double percentile_89L = pct.GetPercentile(PriceType.Low, 89, 25);

			// Calculate 75th and 25th for length 144 (longest length)
			double highest_high = pct.GetPercentile(PriceType.High, 144, 75) ;
			double lowest_low = pct.GetPercentile(PriceType.Low, 144, 25) ;

			// Calculate trend strength conditions
			bool trendBull1 = percentile_13H > highest_high;
			bool trendBull2 = percentile_21H > highest_high;
			bool trendBull3 = percentile_34H > highest_high;
			bool trendBull4 = percentile_55H > highest_high;
			bool trendBull5 = percentile_89H > highest_high;
			bool trendBull6 = percentile_13L > highest_high;
			bool trendBull7 = percentile_21L > highest_high;
			bool trendBull8 = percentile_34L > highest_high;
			bool trendBull9 = percentile_55L > highest_high;
			bool trendBull10 = percentile_89L > highest_high;

			bool trendBear1 = percentile_13H < lowest_low;
			bool trendBear2 = percentile_21H < lowest_low;
			bool trendBear3 = percentile_34H < lowest_low;
			bool trendBear4 = percentile_55H < lowest_low;
			bool trendBear5 = percentile_89H < lowest_low;
			bool trendBear6 = percentile_13L < lowest_low;
			bool trendBear7 = percentile_21L < lowest_low;
			bool trendBear8 = percentile_34L < lowest_low;
			bool trendBear9 = percentile_55L < lowest_low;
			bool trendBear10 = percentile_89L < lowest_low;

			int countBull =
				 (trendBull1 ? 1 : 0) +
				 (trendBull2 ? 1 : 0) +
				 (trendBull3 ? 1 : 0) +
				 (trendBull4 ? 1 : 0) +
				 (trendBull5 ? 1 : 0) +
				 (trendBull6 ? 1 : 0) +
				 (trendBull7 ? 1 : 0) +
				 (trendBull8 ? 1 : 0) +
				 (trendBull9 ? 1 : 0) +
				 (trendBull10 ? 1 : 0);

			int countBear =
				 (trendBear1 ? 1 : 0) +
				 (trendBear2 ? 1 : 0) +
				 (trendBear3 ? 1 : 0) +
				 (trendBear4 ? 1 : 0) +
				 (trendBear5 ? 1 : 0) +
				 (trendBear6 ? 1 : 0) +
				 (trendBear7 ? 1 : 0) +
				 (trendBear8 ? 1 : 0) +
				 (trendBear9 ? 1 : 0) +
				 (trendBear10 ? 1 : 0);

			// Calculate weak bull count
			int weakBullCount = 
				 (percentile_13L < highest_high && percentile_13L > lowest_low ? 1 : 0) +
				 (percentile_21L < highest_high && percentile_21L > lowest_low ? 1 : 0) +
				 (percentile_34L < highest_high && percentile_34L > lowest_low ? 1 : 0) +
				 (percentile_55L < highest_high && percentile_55L > lowest_low ? 1 : 0) +
				 (percentile_89L < highest_high && percentile_89L > lowest_low ? 1 : 0);

			// Calculate weak bear count
			int weakBearCount = 
				 (percentile_13H > lowest_low && percentile_13H < highest_high ? 1 : 0) +
				 (percentile_21H > lowest_low && percentile_21H < highest_high ? 1 : 0) +
				 (percentile_34H > lowest_low && percentile_34H < highest_high ? 1 : 0) +
				 (percentile_55H > lowest_low && percentile_55H < highest_high ? 1 : 0) +
				 (percentile_89H > lowest_low && percentile_89H < highest_high ? 1 : 0);
				 
			

            // Calculate bull strength and bear strength
            double bullStrength = 10 * (countBull + 0.5 * weakBullCount - 0.5 * weakBearCount - countBear);
            double bearStrength = 10 * (countBear + 0.5 * weakBearCount - 0.5 * weakBullCount - countBull);

            // Calculate the current trend
            double currentTrendValue = bullStrength - bearStrength;

            // Calulate volume strength
            double V13 = SMA(Volume, 13)[0] / SMA(Volume, 144)[0];
            double V21 = SMA(Volume, 21)[0] / SMA(Volume, 144)[0];
            double V34 = SMA(Volume, 34)[0] / SMA(Volume, 144)[0];
            double V55 = SMA(Volume, 55)[0] / SMA(Volume, 144)[0];
            double V89 = SMA(Volume, 89)[0] / SMA(Volume, 144)[0];
            double VCF = (0.5 * V13 + 0.25 * V21 + 0.125 * V34 + 0.08 * V55 + 0.045 * V89);

            // Update series values
            bullStrengthSeries[0] = currentTrendValue;
            volumeStrengthSeries[0] = VCF;

            // Set plot values
            Values[0][0] = bullStrengthSeries[0];
            Values[1][0] = volumeStrengthSeries[0];
		}
		
		


	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TrendStrengthOverTimeOrig[] cacheTrendStrengthOverTimeOrig;
		public TrendStrengthOverTimeOrig TrendStrengthOverTimeOrig()
		{
			return TrendStrengthOverTimeOrig(Input);
		}

		public TrendStrengthOverTimeOrig TrendStrengthOverTimeOrig(ISeries<double> input)
		{
			if (cacheTrendStrengthOverTimeOrig != null)
				for (int idx = 0; idx < cacheTrendStrengthOverTimeOrig.Length; idx++)
					if (cacheTrendStrengthOverTimeOrig[idx] != null &&  cacheTrendStrengthOverTimeOrig[idx].EqualsInput(input))
						return cacheTrendStrengthOverTimeOrig[idx];
			return CacheIndicator<TrendStrengthOverTimeOrig>(new TrendStrengthOverTimeOrig(), input, ref cacheTrendStrengthOverTimeOrig);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TrendStrengthOverTimeOrig TrendStrengthOverTimeOrig()
		{
			return indicator.TrendStrengthOverTimeOrig(Input);
		}

		public Indicators.TrendStrengthOverTimeOrig TrendStrengthOverTimeOrig(ISeries<double> input )
		{
			return indicator.TrendStrengthOverTimeOrig(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TrendStrengthOverTimeOrig TrendStrengthOverTimeOrig()
		{
			return indicator.TrendStrengthOverTimeOrig(Input);
		}

		public Indicators.TrendStrengthOverTimeOrig TrendStrengthOverTimeOrig(ISeries<double> input )
		{
			return indicator.TrendStrengthOverTimeOrig(input);
		}
	}
}

#endregion
