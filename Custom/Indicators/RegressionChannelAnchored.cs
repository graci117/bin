//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;

#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Linear regression is used to calculate a best fit line for the price data. In addition an upper and lower band is added by calculating the standard deviation of prices from the regression line.
	/// </summary>
	public class RegressionChannelAnchored : Indicator
	{
		private Series<double> interceptSeries;
		private Series<double> slopeSeries;
		private Series<double> stdDeviationSeries;
		private Series<double> priceSeries;
		 private double slope;
        private double intercept;
        private double stdDeviation;
		int startIndex	;
			int endIndex;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionRegressionChannel;
				Name						= "RegressionChannelAnchored";
				IsAutoScale					= false;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 35;
				Width						= 2;
				 EndPlotTime = 1430;  // Default end plot time (2:30 pm)
				AddPlot(Brushes.DarkGray, "Middle");
                AddPlot(Brushes.DodgerBlue, "Upper");
                AddPlot(Brushes.DodgerBlue, "Lower");
			}
			else if (State == State.DataLoaded)
			{
				interceptSeries		= new Series<double>(this);
				slopeSeries			= new Series<double>(this);
				stdDeviationSeries	= new Series<double>(this);
				priceSeries	= new Series<double>(this);
				slope = 0;
                intercept = 0;
                stdDeviation = 0;
				
			}
		}

	     protected override void OnBarUpdate()
		{
			if (CurrentBar == 10)
				return;
			
			Period = GetRegressionPeriod();
			
			
			 if ( Period <= 0 || startIndex == 0) return;
			 
			 
			 
			int barCount		= Period + 1;
			double sumX			= barCount * (barCount - 1) * 0.5;
			double divisor		= sumX * sumX - barCount * barCount * (barCount - 1d) * (2d * barCount - 1) / 6d;

			// First we calculate the linear regression parameters

			double sumXY  = 0;

			double sumY  = 0;


			for (int count = 0; count < barCount; count++)
			{
				int idx						= startIndex + count;
				
				if (idx < Bars.Count)
				{
					double priceValue = GetBarPrice(Bars, idx);
					sumXY += count * priceValue;
					sumY += priceValue;
				}
			}

			if (divisor == 0 || Period == 0)
				return;

			double	slope	  = ((double) barCount * sumXY - sumX * sumY) / divisor;
			double	intercept = (sumY - slope * sumX) / barCount;

			slopeSeries[0] = slope;
			interceptSeries[0] = intercept;

			// Next we calculate the standard deviation of the 
			// residuals (vertical distances to the regression line).

			double sumResiduals = 0;

			for (int count = 0; count < barCount; count++) 
			{
				int idx						= startIndex + count;
				if (idx < Bars.Count)
				{
					double regressionValue	= Math.Abs(GetBarPrice(Bars, idx) - (intercept + slope * ((double)barCount - 1 - count)));
					sumResiduals += regressionValue;
				}
			}

			double avgResiduals = sumResiduals / barCount;

			sumResiduals = 0;
			
			for (int count = 0; count < barCount; count++)
			{
				int idx = startIndex + count;
				if (idx < Bars.Count)
				{
					double regressionValue	= Math.Abs(GetBarPrice(Bars, idx) - (intercept + slope * ((double)barCount - 1 - count)));
					sumResiduals += (regressionValue - avgResiduals) * (regressionValue - avgResiduals);
				}
			}

			double stdDeviation				= Math.Sqrt(sumResiduals / barCount);
			stdDeviationSeries[0] = stdDeviation;
			
			
	

  			double middle = intercept + slope * (barCount - 1);
  			Middle[0] = middle;
  			Upper[0] = middle + stdDeviation * Width;
  			Lower[0] = middle - stdDeviation * Width;
			
			//Print($"Plotting Middle: {Middle[0]}, Upper: {Upper[0]}, Lower: {Lower[0]}");
			
			//DrawLine("test", -50, middle, 0,middle, Color.Purple);
		}
		
		public double GetBarPrice(Bars barObject, int barIndex)
		{
			if (barObject == null || !barObject.IsValidDataPointAt(barIndex))
				return double.MinValue;

			return barObject.GetClose(barIndex);
			
		}
		
		private int GetRegressionPeriod()
        {
//            DateTime currentDateTime = Bars.LastBarTime; //DateTime.Now;;  // Get the current chart time
//			Print("---currentDateTime---- " + currentDateTime);
			
			
			
            
			
			
//			DateTime anchorStartTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 4, 0, 0);
//			DateTime anchorEndTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 9, 30, 0);
			
			
			if (Time[0].TimeOfDay >= new TimeSpan(4,0,0))
			{		
				
				DateTime currentDateTime = Time[0]; //DateTime.Now;;  // Get the current chart time
//				Print("---Time[0]---- " + Time[0].ToLongDateString());
				
//				Print("---currentDateTime---- " + currentDateTime);
				
				DateTime anchorStartTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 4, 0, 0);
				DateTime anchorEndTime = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 9, 30, 0);
				
				 startIndex			= Bars.GetBar(anchorStartTime);
				
				
				if (Time[0].TimeOfDay >= new TimeSpan(9,30,0))
				{
					 endIndex			= Bars.GetBar(anchorEndTime);
					
				}
				else
				{
					endIndex			= Bars.GetBar(DateTime.Now);
				}
//				Print("---anchorStartTime---- " + anchorStartTime);
				
//				Print("---startIndex---- " + startIndex);
//				Print(" ---endIndex----" + endIndex);
				
				return (endIndex - startIndex  > 0 ?endIndex - startIndex  : 0 );	
			}
			else
			{
				return 0;
			}
           
        }
		
		

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[0]; }
		}

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptGeneral", Order = 0)]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}

		[Range(1, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Width", GroupName = "NinjaScriptGeneral", Order = 1)]
		public double Width
		{ get; set; }
		
		[Range(1300, 1600), NinjaScriptProperty]
        public int EndPlotTime { get; set; }  // Property for end plot time
		
		#endregion

		#region Misc
		private int GetXPos(int barsBack)
		{
			return ChartControl.GetXByBarIndex(ChartBars,
				Math.Max(0, Bars.Count - 1 - barsBack - (Calculate == Calculate.OnBarClose ? 1 : 0)));
		}

		private int GetYPos(double price, ChartScale chartScale)
		{
			return chartScale.GetYByValue(price);
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			
		 if (Bars == null || chartControl == null) return;
		 
		 
		 DateTime timeToCheck = new DateTime(2024, 04, 25, 11, 0, 0);
 
  // Find the chart-canvas x-coordinate of the bar at the specified time
  			Print( chartControl.GetXByTime(timeToCheck));
		 
		 Print("---Time[0].TimeOfDay---- " + Time[0].TimeOfDay);
		 
		 //if (Time[0].TimeOfDay < new TimeSpan(4,0,0) || Time[0].TimeOfDay > new TimeSpan(9,30,0)) return;
		
				
		 
            RenderTarget.AntialiasMode = AntialiasMode.PerPrimitive;

            ChartPanel panel = chartControl.ChartPanels[ChartPanel.PanelIndex];

            // Ensure there's enough data to render without errors
            if (BarsArray[0].Count < Period) return;  // Validate that there are enough bars to work with

            // Retrieve regression parameters with safe index handling
            int idx = Period - (Calculate == Calculate.OnBarClose ? 1 : 0);
            if (idx < 0) return;  // Validate index before accessing

			if (Time[0].TimeOfDay >= new TimeSpan(4,0,0))
			{
	            double intercept = interceptSeries.GetValueAt(idx);
	            double slope = slopeSeries.GetValueAt(idx);
	            double stdDev = stdDeviationSeries.GetValueAt(idx);
	
	            if (slope == 0 || intercept == 0) return;
	
				
				
				DateTime anchorStartTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, 4, 0, 0);
				DateTime anchorEndTime = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, 9, 30, 0);
							
				
				
				Print("---Time[0].TimeOfDay---- " + Time[0].ToLongDateString());
				Print("---anchorStartTime---- " + anchorStartTime);
				
				Print("---anchorEndTime---- " + anchorEndTime);
	
	            // Calculate x-coordinates for anchor points
	            int startX = chartControl.GetXByTime(anchorStartTime);
	            int endX = chartControl.GetXByTime(anchorEndTime);
	
	            // Diagnostic print statements for verification
	            Print($"StartX: {startX}, EndX: {endX}");
	            //Print($"Chart range: {Times[0][0]} to {Times[0][Bars.Count - 1]}");
	
	            if (startX >= endX)
	            {
	                Print($"Warning: StartX and EndX are the same. Ensure the times are correct and within range.");
	                //return;
	            }
	
	            // Calculate pixel-based standard deviation
	            int stdDevPixels = (int) Math.Round(((stdDev * Width) / (chartScale.MaxValue - chartScale.MinValue)) * panel.H, 0);
	
	            // Calculate y-coordinates for middle regression line based on regression and x-coordinates
	            int startY = chartScale.GetYByValue(intercept + slope * (Period));
	            int endY = chartScale.GetYByValue(intercept + slope * 0);
	
	            // Define vectors for rendering
	            Vector2 startVector = new Vector2(startX, startY);
	            Vector2 endVector = new Vector2(endX, endY);
				
			
	
			// Middle
				RenderTarget.DrawLine(startVector, endVector, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);
				//RenderTarget.DrawLine(startVector2, endVector2, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);
	
				// Upper
				RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y - stdDevPixels), new Vector2(endVector.X, endVector.Y - stdDevPixels), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);
				//RenderTarget.DrawLine(new Vector2(startVector2.X, startVector2.Y - stdDevPixels), new Vector2(endVector2.X, endVector2.Y - stdDevPixels), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);
				
				// Lower
				RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y + stdDevPixels), new Vector2(endVector.X, endVector.Y + stdDevPixels), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);
				//RenderTarget.DrawLine(new Vector2(startVector2.X, startVector2.Y + stdDevPixels), new Vector2(endVector2.X, endVector2.Y + stdDevPixels), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);
	
				RenderTarget.AntialiasMode = AntialiasMode.Aliased;
			}
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RegressionChannelAnchored[] cacheRegressionChannelAnchored;
		public RegressionChannelAnchored RegressionChannelAnchored(int period, double width, int endPlotTime)
		{
			return RegressionChannelAnchored(Input, period, width, endPlotTime);
		}

		public RegressionChannelAnchored RegressionChannelAnchored(ISeries<double> input, int period, double width, int endPlotTime)
		{
			if (cacheRegressionChannelAnchored != null)
				for (int idx = 0; idx < cacheRegressionChannelAnchored.Length; idx++)
					if (cacheRegressionChannelAnchored[idx] != null && cacheRegressionChannelAnchored[idx].Period == period && cacheRegressionChannelAnchored[idx].Width == width && cacheRegressionChannelAnchored[idx].EndPlotTime == endPlotTime && cacheRegressionChannelAnchored[idx].EqualsInput(input))
						return cacheRegressionChannelAnchored[idx];
			return CacheIndicator<RegressionChannelAnchored>(new RegressionChannelAnchored(){ Period = period, Width = width, EndPlotTime = endPlotTime }, input, ref cacheRegressionChannelAnchored);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RegressionChannelAnchored RegressionChannelAnchored(int period, double width, int endPlotTime)
		{
			return indicator.RegressionChannelAnchored(Input, period, width, endPlotTime);
		}

		public Indicators.RegressionChannelAnchored RegressionChannelAnchored(ISeries<double> input , int period, double width, int endPlotTime)
		{
			return indicator.RegressionChannelAnchored(input, period, width, endPlotTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RegressionChannelAnchored RegressionChannelAnchored(int period, double width, int endPlotTime)
		{
			return indicator.RegressionChannelAnchored(Input, period, width, endPlotTime);
		}

		public Indicators.RegressionChannelAnchored RegressionChannelAnchored(ISeries<double> input , int period, double width, int endPlotTime)
		{
			return indicator.RegressionChannelAnchored(input, period, width, endPlotTime);
		}
	}
}

#endregion
