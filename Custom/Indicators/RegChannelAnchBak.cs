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
	public class RegChannelAnchBak : Indicator
	{
		private Series<double> interceptSeries;
		private Series<double> slopeSeries;
		private Series<double> stdDeviationSeries;
		private Series<double> priceSeries;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionRegressionChannel;
				Name						= "RegChannelAnchBak";
				IsAutoScale					= false;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 35;
				Width						= 2;

				AddPlot(Brushes.DarkGray, NinjaTrader.Custom.Resource.NinjaScriptIndicatorMiddle);
				AddPlot(Brushes.DodgerBlue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddPlot(Brushes.DodgerBlue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
			}
			else if (State == State.DataLoaded)
			{
				interceptSeries		= new Series<double>(this);
				slopeSeries			= new Series<double>(this);
				stdDeviationSeries	= new Series<double>(this);
				priceSeries	= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			// First we calculate the linear regression parameters
			TimeSpan currentTime = Time[0].TimeOfDay;
			TimeSpan StartTime = new TimeSpan(4, 0, 0);
			TimeSpan EndLRtime = new TimeSpan(09, 29, 0);
			TimeSpan EndPlottime = new TimeSpan(14, 29, 0);
			//if (currentTime < StartTime || currentTime > EndLRtime) return;
			Period = GetPeriod();
			if (Period == 0 || Period > 165) return;
			double sumX = (double) Period*(Period - 1)*.5;
			double divisor = sumX*sumX -
								(double) Period*Period*(Period - 1)*(2*Period - 1)/6;
			double sumXY = 0;
			double sumY = 0;
			
			int barCount = Math.Min(Period, CurrentBar);

			
			TimeSpan diffTimeEnd = currentTime.Subtract(EndLRtime);
			priceSeries[0] = diffTimeEnd.TotalSeconds >= 0 ?  priceSeries[1] : Close[0];
			
			
			
			for (int count = 0; count < barCount; count++)
			{
				sumXY += count*priceSeries[count];
				
				sumY += priceSeries[count];
			}

			
			
			if (divisor.ApproxCompare(0) == 0 && Period == 0) return;

			double slope = (Period*sumXY - sumX*sumY)/divisor;
			double intercept = (sumY - slope*sumX)/Period;

			slopeSeries[0] = slope;
			interceptSeries[0] = intercept;

			// Next we calculate the standard deviation of the
			// residuals (vertical distances to the regression line).

			double sumResiduals = 0;

			for (int count = 0; count < barCount; count++)
			{
				double regressionValue = intercept + slope * (Period - 1 - count);
				double residual = Math.Abs(Input[count] - regressionValue);
				sumResiduals += residual;
			}

			double avgResiduals = sumResiduals / Math.Min(CurrentBar - 1, Period);

			sumResiduals = 0;
			for (int count = 0; count < barCount; count++)
			{
				double regressionValue = intercept + slope * (Period - 1 - count);
				double residual = Math.Abs(Input[count] - regressionValue);
				sumResiduals += (residual - avgResiduals) * (residual - avgResiduals);
			}

			double stdDeviation = Math.Sqrt(sumResiduals / Math.Min(CurrentBar + 1, Period));
			stdDeviationSeries[0] = stdDeviation;

			TimeSpan diffTimeEndPlot = currentTime.Subtract(EndPlottime);
			double middle = intercept + slope * (Period - 1);
			Middle[0] = CurrentBar == 0 || diffTimeEndPlot.TotalSeconds > 0  ? Input[0] : middle;
			Upper[0] = stdDeviation.ApproxCompare(0) == 0 || Double.IsInfinity(stdDeviation) ? Input[0] : middle + stdDeviation * Width;
			Lower[0] = stdDeviation.ApproxCompare(0) == 0 || Double.IsInfinity(stdDeviation) ? Input[0] : middle - stdDeviation * Width;
		}
		
		private int GetPeriod()
		{
			 TimeSpan currentTime = Time[0].TimeOfDay;
			 TimeSpan stTime = new TimeSpan(4, 0, 0); // 1429
			
			 TimeSpan diffTime = currentTime.Subtract(stTime);
			
			 int totalMinutes = Convert.ToInt32(diffTime.TotalMinutes);
			
		     if (totalMinutes >=2)
			 {
				 return totalMinutes/2;
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
			if (Bars == null || ChartControl == null) return;

			RenderTarget.AntialiasMode = AntialiasMode.PerPrimitive;

			ChartPanel panel = chartControl.ChartPanels[ChartPanel.PanelIndex];

			int idx = BarsArray[0].Count - 1 - (Calculate == Calculate.OnBarClose ? 1 : 0);
			double intercept = interceptSeries.GetValueAt(idx);
			double slope = slopeSeries.GetValueAt(idx);
			double stdDev = stdDeviationSeries.GetValueAt(idx);
			
			int stdDevPixels = (int) Math.Round(((stdDev*Width)/(chartScale.MaxValue - chartScale.MinValue))*panel.H, 0);
			
			int xPos = GetXPos(Period - 1 - Displacement);
			int yPos = GetYPos(intercept, chartScale);
			
			int xPos2 = GetXPos(0 - Displacement);
			int yPos2 = GetYPos(intercept + slope*(Period - 1), chartScale);
			
			Vector2 startVector = new Vector2(xPos, yPos);
			Vector2 endVector = new Vector2(xPos2, yPos2);

			// Middle
			RenderTarget.DrawLine(startVector, endVector, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);

			// Upper
			RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y - stdDevPixels), new Vector2(endVector.X, endVector.Y - stdDevPixels), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);

			// Lower
			RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y + stdDevPixels), new Vector2(endVector.X, endVector.Y + stdDevPixels), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);

			RenderTarget.AntialiasMode = AntialiasMode.Aliased;
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RegChannelAnchBak[] cacheRegChannelAnchBak;
		public RegChannelAnchBak RegChannelAnchBak(int period, double width)
		{
			return RegChannelAnchBak(Input, period, width);
		}

		public RegChannelAnchBak RegChannelAnchBak(ISeries<double> input, int period, double width)
		{
			if (cacheRegChannelAnchBak != null)
				for (int idx = 0; idx < cacheRegChannelAnchBak.Length; idx++)
					if (cacheRegChannelAnchBak[idx] != null && cacheRegChannelAnchBak[idx].Period == period && cacheRegChannelAnchBak[idx].Width == width && cacheRegChannelAnchBak[idx].EqualsInput(input))
						return cacheRegChannelAnchBak[idx];
			return CacheIndicator<RegChannelAnchBak>(new RegChannelAnchBak(){ Period = period, Width = width }, input, ref cacheRegChannelAnchBak);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RegChannelAnchBak RegChannelAnchBak(int period, double width)
		{
			return indicator.RegChannelAnchBak(Input, period, width);
		}

		public Indicators.RegChannelAnchBak RegChannelAnchBak(ISeries<double> input , int period, double width)
		{
			return indicator.RegChannelAnchBak(input, period, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RegChannelAnchBak RegChannelAnchBak(int period, double width)
		{
			return indicator.RegChannelAnchBak(Input, period, width);
		}

		public Indicators.RegChannelAnchBak RegChannelAnchBak(ISeries<double> input , int period, double width)
		{
			return indicator.RegChannelAnchBak(input, period, width);
		}
	}
}

#endregion
