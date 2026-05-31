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
	public class RegChannelTest1 : Indicator
	{
		private Series<double> interceptSeries;
		private Series<double> slopeSeries;
		private Series<double> stdDeviationSeries;
		private Series<double> upperChannelSeries;
		private Series<double> lowerChannelSeries;
		private Series<double> upperChannelSeriestmp;
		private Series<double> lowerChannelSeriestmp;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionRegressionChannel;
				Name						= "RegChannelTest1";
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
				upperChannelSeries	= new Series<double>(this);
				lowerChannelSeries	= new Series<double>(this);
				upperChannelSeriestmp	= new Series<double>(this);
				lowerChannelSeriestmp	= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			// First we calculate the linear regression parameters

			double sumX = (double) Period*(Period - 1)*.5;
			double divisor = sumX*sumX -
								(double) Period*Period*(Period - 1)*(2*Period - 1)/6;
			double sumXY = 0;
			double sumY = 0;
			int barCount = Math.Min(Period, CurrentBar);

			for (int count = 0; count < barCount; count++)
			{
				sumXY += count*Input[count];
				sumY += Input[count];
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

			double middle = intercept + slope * (Period - 1);
			Middle[0] = CurrentBar == 0 ? Input[0] : middle;
			
			lowerChannelSeriestmp[0] =  Math.Abs(Middle[0] - Low[0]);
			upperChannelSeriestmp[0] =  Math.Abs(High[0] - Middle[0]);
			
			lowerChannelSeries[0] = MAX(lowerChannelSeriestmp, 30)[0];
			upperChannelSeries[0] = MAX(upperChannelSeriestmp, 30)[0];
			
			
			Upper[0] = upperChannelSeries[0].ApproxCompare(0) == 0 || Double.IsInfinity(lowerChannelSeries[0]) ? Input[0] : Middle[0]  + upperChannelSeries[0] * Width;
			Lower[0] = lowerChannelSeries[0].ApproxCompare(0) == 0 || Double.IsInfinity(upperChannelSeries[0] ) ? Input[0] : Middle[0]  - lowerChannelSeries[0]  * Width;
			
			
//			Upper[0] = stdDeviation.ApproxCompare(0) == 0 || Double.IsInfinity(stdDeviation) ? Input[0] : middle + stdDeviation * Width;
//			Lower[0] = stdDeviation.ApproxCompare(0) == 0 || Double.IsInfinity(stdDeviation) ? Input[0] : middle - stdDeviation * Width;
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

		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptGeneral", Order = 0)]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}

		[Range(0, double.MaxValue), NinjaScriptProperty]
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
			//double stdDev = stdDeviationSeries.GetValueAt(idx);
			double stdDevUp = upperChannelSeries.GetValueAt(idx);
			double stdDevLow = lowerChannelSeries.GetValueAt(idx);
			
			//int stdDevPixels = (int) Math.Round(((stdDev*Width)/(chartScale.MaxValue - chartScale.MinValue))*panel.H, 0);
			
			int stdDevPixelsUp = (int) Math.Round(((stdDevUp*Width)/(chartScale.MaxValue - chartScale.MinValue))*panel.H, 0);
			int stdDevPixelsLow = (int) Math.Round(((stdDevLow*Width)/(chartScale.MaxValue - chartScale.MinValue))*panel.H, 0);
			
			int xPos = GetXPos(Period - 1 - Displacement);
			int yPos = GetYPos(intercept, chartScale);
			int xPos2 = GetXPos(0 - Displacement);
			int yPos2 = GetYPos(intercept + slope*(Period - 1), chartScale);
			Vector2 startVector = new Vector2(xPos, yPos);
			Vector2 endVector = new Vector2(xPos2, yPos2);

			// Middle
			RenderTarget.DrawLine(startVector, endVector, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);

			// Upper
			RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y - stdDevPixelsUp), new Vector2(endVector.X, endVector.Y - stdDevPixelsUp), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);

			// Lower
			RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y + stdDevPixelsLow), new Vector2(endVector.X, endVector.Y + stdDevPixelsLow), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);

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
		private RegChannelTest1[] cacheRegChannelTest1;
		public RegChannelTest1 RegChannelTest1(int period, double width)
		{
			return RegChannelTest1(Input, period, width);
		}

		public RegChannelTest1 RegChannelTest1(ISeries<double> input, int period, double width)
		{
			if (cacheRegChannelTest1 != null)
				for (int idx = 0; idx < cacheRegChannelTest1.Length; idx++)
					if (cacheRegChannelTest1[idx] != null && cacheRegChannelTest1[idx].Period == period && cacheRegChannelTest1[idx].Width == width && cacheRegChannelTest1[idx].EqualsInput(input))
						return cacheRegChannelTest1[idx];
			return CacheIndicator<RegChannelTest1>(new RegChannelTest1(){ Period = period, Width = width }, input, ref cacheRegChannelTest1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RegChannelTest1 RegChannelTest1(int period, double width)
		{
			return indicator.RegChannelTest1(Input, period, width);
		}

		public Indicators.RegChannelTest1 RegChannelTest1(ISeries<double> input , int period, double width)
		{
			return indicator.RegChannelTest1(input, period, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RegChannelTest1 RegChannelTest1(int period, double width)
		{
			return indicator.RegChannelTest1(Input, period, width);
		}

		public Indicators.RegChannelTest1 RegChannelTest1(ISeries<double> input , int period, double width)
		{
			return indicator.RegChannelTest1(input, period, width);
		}
	}
}

#endregion
