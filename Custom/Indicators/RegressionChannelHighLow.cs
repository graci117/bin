//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
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
    /// Linear regression is used to calculate a best fit line for the price data. In addition an upper and lower band is added by calculating the highest high and lowest low prices within the period.
    /// </summary>
    public class RegressionChannelHighLow : Indicator
    {
        private Series<double> interceptSeries;
        private Series<double> slopeSeries;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionRegressionChannel;
                Name                        = "RegressionChannelHighLow";
                Calculate                   = Calculate.OnPriceChange;
                IsAutoScale                 = false;
                IsOverlay                   = true;
                IsSuspendedWhileInactive    = true;
                Period                      = 30;
                Width                       = 3.5;

                AddPlot(Brushes.LightGray, NinjaTrader.Custom.Resource.NinjaScriptIndicatorMiddle);
                AddPlot(Brushes.Magenta, NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
                AddPlot(Brushes.Magenta, NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
            }
            else if (State == State.DataLoaded)
            {
                interceptSeries     = new Series<double>(this);
                slopeSeries         = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            // First we calculate the linear regression parameters

            double sumX = (double)Period * (Period - 1) * .5;
            double divisor = sumX * sumX -
                                (double)Period * Period * (Period - 1) * (2 * Period - 1) / 6;
            double sumXY = 0;
            double sumY = 0;
            int barCount = Math.Min(Period, CurrentBar);

            for (int count = 0; count < barCount; count++)
            {
                sumXY += count * Input[count];
                sumY += Input[count];
            }

            if (divisor.ApproxCompare(0) == 0 && Period == 0) return;

            double slope = (Period * sumXY - sumX * sumY) / divisor;
            double intercept = (sumY - slope * sumX) / Period;

            slopeSeries[0] = slope;
            interceptSeries[0] = intercept;

            // Calculate the highest high and lowest low within the period
            double highestHigh = double.MinValue;
            double lowestLow = double.MaxValue;

            for (int count = 0; count < barCount; count++)
            {
                highestHigh = Math.Max(highestHigh, High[count]);
                lowestLow = Math.Min(lowestLow, Low[count]);
            }

            double middle = intercept + slope * (Period - 1);
            Middle[0] = CurrentBar == 0 ? Input[0] : middle;
            Upper[0] = highestHigh;
            Lower[0] = lowestLow;
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
            int xPos = GetXPos(Period - 1 - Displacement);
            int yPos = GetYPos(intercept, chartScale);
            int xPos2 = GetXPos(0 - Displacement);
            int yPos2 = GetYPos(intercept + slope * (Period - 1), chartScale);
            Vector2 startVector = new Vector2(xPos, yPos);
            Vector2 endVector = new Vector2(xPos2, yPos2);

            // Middle
            RenderTarget.DrawLine(startVector, endVector, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);

            // Upper
            double upperValue = Upper.GetValueAt(idx);
            int yPosUpper = GetYPos(upperValue, chartScale);
            RenderTarget.DrawLine(new Vector2(startVector.X, yPosUpper), new Vector2(endVector.X, yPosUpper), Plots[1].BrushDX, Plots[1].Width, Plots[1].StrokeStyle);

            // Lower
            double lowerValue = Lower.GetValueAt(idx);
            int yPosLower = GetYPos(lowerValue, chartScale);
            RenderTarget.DrawLine(new Vector2(startVector.X, yPosLower), new Vector2(endVector.X, yPosLower), Plots[2].BrushDX, Plots[2].Width, Plots[2].StrokeStyle);

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
		private RegressionChannelHighLow[] cacheRegressionChannelHighLow;
		public RegressionChannelHighLow RegressionChannelHighLow(int period, double width)
		{
			return RegressionChannelHighLow(Input, period, width);
		}

		public RegressionChannelHighLow RegressionChannelHighLow(ISeries<double> input, int period, double width)
		{
			if (cacheRegressionChannelHighLow != null)
				for (int idx = 0; idx < cacheRegressionChannelHighLow.Length; idx++)
					if (cacheRegressionChannelHighLow[idx] != null && cacheRegressionChannelHighLow[idx].Period == period && cacheRegressionChannelHighLow[idx].Width == width && cacheRegressionChannelHighLow[idx].EqualsInput(input))
						return cacheRegressionChannelHighLow[idx];
			return CacheIndicator<RegressionChannelHighLow>(new RegressionChannelHighLow(){ Period = period, Width = width }, input, ref cacheRegressionChannelHighLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RegressionChannelHighLow RegressionChannelHighLow(int period, double width)
		{
			return indicator.RegressionChannelHighLow(Input, period, width);
		}

		public Indicators.RegressionChannelHighLow RegressionChannelHighLow(ISeries<double> input , int period, double width)
		{
			return indicator.RegressionChannelHighLow(input, period, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RegressionChannelHighLow RegressionChannelHighLow(int period, double width)
		{
			return indicator.RegressionChannelHighLow(Input, period, width);
		}

		public Indicators.RegressionChannelHighLow RegressionChannelHighLow(ISeries<double> input , int period, double width)
		{
			return indicator.RegressionChannelHighLow(input, period, width);
		}
	}
}

#endregion
