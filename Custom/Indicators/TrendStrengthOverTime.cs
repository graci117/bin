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
	public class TrendStrengthOverTime : Indicator
	{
		private Series<double> bullStrengthSeries;
		private Series<double> volumeStrengthSeries;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TrendStrengthOverTime";
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
				
				
				AddPlot(new Stroke (Brushes.Green, 2), PlotStyle.Line, "TrendStrength");
				AddPlot(new Stroke (Brushes.Orange, 2), PlotStyle.Line, "VolumeStrength");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				bullStrengthSeries = new Series<double>(this);
				volumeStrengthSeries = new Series<double>(this);
				
			}
		}
		
		 // Custom function to calculate percentile nearest rank
        private double PercentileNearestRank(ISeries<double> values, int length, double percentile)
        {
            // Sort the values in ascending order
			
			List<double> valuesList = new List<double>();

            for (int i = 0; i < length; i++)
            {
                valuesList.Add(values[i]);
				
				
            }
			
			
			
            double[] sortedValues = valuesList.OrderBy(v => v).ToArray();
			
			
            // Calculate the index corresponding to the percentile
            int index = (int)Math.Round((percentile / 100.0) * length) - 1;
			
		
			
            // Ensure the index is within bounds
            index = Math.Max(0, Math.Min(index, sortedValues.Length - 1));
			
				
			
			
            // Return the value at the calculated index
            return sortedValues[index];
        }
		

		

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 145)
				return;
			

			
			 int length = 144; // Adjust this length as needed

            double highestHigh = PercentileNearestRank(High, length, 75);
			
			
			
            double lowestLow = PercentileNearestRank(Low, length, 25);			
			

            // Calculate trend strength conditions
            int countBull = 0;
            int countBear = 0;

            for (int i = 0; i < 10; i++)
            {
                bool trendBull = false;
                bool trendBear = false;

                if (i < 5)
                {
                    trendBull = PercentileNearestRank(High, new int[] { 13, 21, 34, 55, 89 }[i], 75) > highestHigh;
                    trendBear = PercentileNearestRank(High, new int[] { 13, 21, 34, 55, 89 }[i], 75) < lowestLow;					

                }
                else
                {
                    trendBull = PercentileNearestRank(Low, new int[] { 13, 21, 34, 55, 89 }[i - 5], 25) > highestHigh;
                    trendBear = PercentileNearestRank(Low, new int[] { 13, 21, 34, 55, 89 }[i - 5], 25) <  lowestLow;
                }				
                countBull += trendBull ? 1 : 0;
                countBear += trendBear ? 1 : 0;		
				
            }		
			
			 int weakBullCount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (PercentileNearestRank(Low, new int[] { 13, 21, 34, 55, 89 }[i], 25) < highestHigh &&
                    PercentileNearestRank(Low, new int[] { 13, 21, 34, 55, 89 }[i], 25) > lowestLow)
                {
                    weakBullCount++;
                }
            }

            // Calculate weak bear count
            int weakBearCount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (PercentileNearestRank(High, new int[] { 13, 21, 34, 55, 89 }[i], 75) > lowestLow &&
                    PercentileNearestRank(High, new int[] { 13, 21, 34, 55, 89 }[i], 75) < highestHigh)
                {
                    weakBearCount++;
                }
            }
			
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
            bullStrengthSeries[0] = bullStrength;
            volumeStrengthSeries[0] = VCF;
			

            // Set plot values
            TrendStrength[0] = bullStrengthSeries[0];
            VolumeStrength[1] = volumeStrengthSeries[0];
			
			if(currentTrendValue > 0) {PlotBrushes[0][0] = Brushes.Green;}
			else {PlotBrushes[0][0] = Brushes.Red;}
		}

			
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TrendStrength
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> VolumeStrength
		{
			get { return Values[1]; }
		}

	
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TrendStrengthOverTime[] cacheTrendStrengthOverTime;
		public TrendStrengthOverTime TrendStrengthOverTime()
		{
			return TrendStrengthOverTime(Input);
		}

		public TrendStrengthOverTime TrendStrengthOverTime(ISeries<double> input)
		{
			if (cacheTrendStrengthOverTime != null)
				for (int idx = 0; idx < cacheTrendStrengthOverTime.Length; idx++)
					if (cacheTrendStrengthOverTime[idx] != null &&  cacheTrendStrengthOverTime[idx].EqualsInput(input))
						return cacheTrendStrengthOverTime[idx];
			return CacheIndicator<TrendStrengthOverTime>(new TrendStrengthOverTime(), input, ref cacheTrendStrengthOverTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TrendStrengthOverTime TrendStrengthOverTime()
		{
			return indicator.TrendStrengthOverTime(Input);
		}

		public Indicators.TrendStrengthOverTime TrendStrengthOverTime(ISeries<double> input )
		{
			return indicator.TrendStrengthOverTime(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TrendStrengthOverTime TrendStrengthOverTime()
		{
			return indicator.TrendStrengthOverTime(Input);
		}

		public Indicators.TrendStrengthOverTime TrendStrengthOverTime(ISeries<double> input )
		{
			return indicator.TrendStrengthOverTime(input);
		}
	}
}

#endregion
