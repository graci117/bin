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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RegressionChannelExtended : Indicator
	{
		private int				period	= 40;
		private double			width	= 3.5;

		private Series<double>		interceptSeries;
		private Series<double>		slopeSeries;
		private Series<double>		stdDeviationSeries;
		private	Series<double>		y;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Linear regression is used to calculate a best fit line for the price data. In addition an upper and lower band is added by calculating the standard deviation of prices from the regression line.";
				Name										= "RegressionChannelExtended";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period						= 40;
				Width						= 3.5;
			}
			else if (State == State.Configure)
			{
				AddPlot(Brushes.DarkGray,	"Middle");
				AddPlot(Brushes.Cyan,	"Upper");
				AddPlot(Brushes.Cyan,	"Lower");
			}
			else if (State == State.DataLoaded)
			{
				interceptSeries		= new Series<double>(this);
				slopeSeries			= new Series<double>(this);
				stdDeviationSeries	= new Series<double>(this);
				y					= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
				return;

			// First we calculate the linear regression parameters

			double sumX	= (double) Period * (Period - 1) * 0.5;
			double divisor = sumX * sumX - 
				(double) Period * Period * (Period - 1) * (2 * Period - 1) / 6;
			double sumXY = 0;
			double sumY  = 0;

			int barCount = Math.Min(Period, CurrentBar);

			for (int count = 0; count < barCount; count++)
			{
				sumXY += count * Input[count];
				sumY  += Input[count];
			}

			if (divisor == 0 || Period == 0)
				return;

			double	slope	  = ((double) Period * sumXY - sumX * sumY) / divisor;
			double	intercept = (sumY - slope * sumX) / Period;

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
				double residual        = Math.Abs(Input[count] - regressionValue);

				sumResiduals += (residual - avgResiduals) * (residual - avgResiduals);
			}

			double stdDeviation = Math.Sqrt(sumResiduals / Math.Min(CurrentBar + 1, Period));
			stdDeviationSeries[0] = stdDeviation;

  			double middle = intercept + slope * (Period - 1);
  			Middle[0] = middle;
  			Upper[0] = middle + stdDeviation * Width;
  			Lower[0] = middle - stdDeviation * Width;
			
			//DrawLine("test", -50, middle, 0,middle, Color.Purple);
		}
		
		private int GetXPos(int barsBack)
		{
			return ChartControl.GetXByBarIndex(ChartControl.BarsArray[0], Math.Max(0, Bars.Count - 1 - barsBack - (Calculate == Calculate.OnBarClose ? 1 : 0)));
		}

		private int GetYPos(double price, ChartScale chartScale)
		{
			return chartScale.GetYByValue(price);
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (Bars == null || ChartControl == null)
				return;
			
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
			
			int xPos3 = GetXPos(-Period);
			int yPos3 = GetYPos(intercept + slope * (Period - 1)*2, chartScale);

			Vector2 startVector = new Vector2(xPos, yPos);
			Vector2 endVector = new Vector2(xPos2, yPos2);
			
			Vector2 startVector2 = new Vector2(xPos2, yPos2);
			Vector2 endVector2 = new Vector2(xPos3, yPos3);

		// Middle
			RenderTarget.DrawLine(startVector, endVector, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);
			RenderTarget.DrawLine(startVector2, endVector2, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);

			// Upper
			RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y - stdDevPixels), new Vector2(endVector.X, endVector.Y - stdDevPixels), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);
			RenderTarget.DrawLine(new Vector2(startVector2.X, startVector2.Y - stdDevPixels), new Vector2(endVector2.X, endVector2.Y - stdDevPixels), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);
			
			// Lower
			RenderTarget.DrawLine(new Vector2(startVector.X, startVector.Y + stdDevPixels), new Vector2(endVector.X, endVector.Y + stdDevPixels), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);
			RenderTarget.DrawLine(new Vector2(startVector2.X, startVector2.Y + stdDevPixels), new Vector2(endVector2.X, endVector2.Y + stdDevPixels), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);

			RenderTarget.AntialiasMode = AntialiasMode.Aliased;
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

		[Range(1, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Width", GroupName = "NinjaScriptGeneral", Order = 1)]
		public double Width
		{ get; set; }
		#endregion
	}
	

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RegressionChannelExtended[] cacheRegressionChannelExtended;
		public RegressionChannelExtended RegressionChannelExtended(int period, double width)
		{
			return RegressionChannelExtended(Input, period, width);
		}

		public RegressionChannelExtended RegressionChannelExtended(ISeries<double> input, int period, double width)
		{
			if (cacheRegressionChannelExtended != null)
				for (int idx = 0; idx < cacheRegressionChannelExtended.Length; idx++)
					if (cacheRegressionChannelExtended[idx] != null && cacheRegressionChannelExtended[idx].Period == period && cacheRegressionChannelExtended[idx].Width == width && cacheRegressionChannelExtended[idx].EqualsInput(input))
						return cacheRegressionChannelExtended[idx];
			return CacheIndicator<RegressionChannelExtended>(new RegressionChannelExtended(){ Period = period, Width = width }, input, ref cacheRegressionChannelExtended);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RegressionChannelExtended RegressionChannelExtended(int period, double width)
		{
			return indicator.RegressionChannelExtended(Input, period, width);
		}

		public Indicators.RegressionChannelExtended RegressionChannelExtended(ISeries<double> input , int period, double width)
		{
			return indicator.RegressionChannelExtended(input, period, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RegressionChannelExtended RegressionChannelExtended(int period, double width)
		{
			return indicator.RegressionChannelExtended(Input, period, width);
		}

		public Indicators.RegressionChannelExtended RegressionChannelExtended(ISeries<double> input , int period, double width)
		{
			return indicator.RegressionChannelExtended(input, period, width);
		}
	}
}

#endregion
