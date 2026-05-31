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
using SharpDX;
using SharpDX.Direct2D1;
using WpfBrush = System.Windows.Media.Brush;
using DxBrush = SharpDX.Direct2D1.Brush;
using DxPathGeometry = SharpDX.Direct2D1.PathGeometry;
using WpfPathGeometry = System.Windows.Media.PathGeometry;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class DynamicLinearRegressionChannels : Indicator
    {
        private struct ChannelData
        {
            public int StartBar;
            public int EndBar;
            public double StartPrice;
            public double EndPrice;
            public double UpperStartPrice;
            public double UpperEndPrice;
            public double LowerStartPrice;
            public double LowerEndPrice;
        }

        private List<ChannelData> historicalChannels;
        private ChannelData currentChannel;
        private int startIndex = 0;
        
        private DxBrush upperBrush;
        private DxBrush lowerBrush;
        private DxBrush lineBrush;
        private SharpDX.DirectWrite.TextFormat textFormat;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Dynamic Linear Regression Channels";
                Name = "DynamicLinearRegressionChannels";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                UpperDeviation = 2.0;
                LowerDeviation = 2.0;
                UpperColor = Brushes.Blue;
                LowerColor = Brushes.Red;
                LineColor = Brushes.Gray;
                
                historicalChannels = new List<ChannelData>();
            }
            else if (State == State.DataLoaded)
            {
                startIndex = CurrentBar;
            }
            else if (State == State.Terminated)
            {
                if (upperBrush != null)
                    upperBrush.Dispose();
                if (lowerBrush != null)
                    lowerBrush.Dispose();
                if (lineBrush != null)
                    lineBrush.Dispose();
                if (textFormat != null)
                    textFormat.Dispose();
            }
        }

        protected override void OnBarUpdate()
		{
		    if (CurrentBar < 20) // Need minimum bars for meaningful calculation
		        return;
		
		    int lengthInput = CurrentBar - startIndex + 1;
		    
		    var slopeData = CalculateSlope(lengthInput);
		    if (double.IsNaN(slopeData.slope))
		        return;
		
		    double startPrice = slopeData.intercept + slopeData.slope * (lengthInput - 1);
		    double endPrice = slopeData.intercept;
		
		    var devData = CalculateDeviation(lengthInput, slopeData.slope, slopeData.average, slopeData.intercept);
		    if (double.IsNaN(devData.stdDev))
		        return;
		
		    double upperStartPrice = startPrice + UpperDeviation * devData.stdDev;
		    double upperEndPrice = endPrice + UpperDeviation * devData.stdDev;
		    double lowerStartPrice = startPrice - LowerDeviation * devData.stdDev;
		    double lowerEndPrice = endPrice - LowerDeviation * devData.stdDev;
		
		    // Check for breakout - simplified condition
		    bool breakout = Close[0] > upperEndPrice || Close[0] < lowerEndPrice;
		    
		    if (breakout && CurrentBar > startIndex + 5) // Allow minimum channel length
		    {
		        // Store the completed channel
		        historicalChannels.Add(new ChannelData
		        {
		            StartBar = startIndex,
		            EndBar = CurrentBar - 1,
		            StartPrice = startPrice,
		            EndPrice = endPrice,
		            UpperStartPrice = upperStartPrice,
		            UpperEndPrice = upperEndPrice,
		            LowerStartPrice = lowerStartPrice,
		            LowerEndPrice = lowerEndPrice
		        });
		        
		        // Start new channel
		        startIndex = CurrentBar;
		    }
		    
		    // Always update current channel
		    currentChannel = new ChannelData
		    {
		        StartBar = startIndex,
		        EndBar = CurrentBar,
		        StartPrice = startPrice,
		        EndPrice = endPrice,
		        UpperStartPrice = upperStartPrice,
		        UpperEndPrice = upperEndPrice,
		        LowerStartPrice = lowerStartPrice,
		        LowerEndPrice = lowerEndPrice
		    };
		}
		
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
		    if (Bars == null || chartControl == null || chartScale == null)
		        return;
		
		    // Initialize brushes
		    if (upperBrush == null)
		    {
		        upperBrush = UpperColor.ToDxBrush(RenderTarget);
		        upperBrush.Opacity = 0.15f;
		    }
		    if (lowerBrush == null)
		    {
		        lowerBrush = LowerColor.ToDxBrush(RenderTarget);
		        lowerBrush.Opacity = 0.15f;
		    }
		    if (lineBrush == null)
		    {
		        lineBrush = LineColor.ToDxBrush(RenderTarget);
		    }
		
		    // Draw historical channels
		    foreach (var channel in historicalChannels)
		    {
		        if (IsValidChannel(channel))
		            DrawChannel(chartControl, chartScale, channel);
		    }
		
		    // Draw current channel - removed StartBar > 0 condition
		    if (IsValidChannel(currentChannel))
		    {
		        DrawChannel(chartControl, chartScale, currentChannel);
		    }
		}
		
		private bool IsValidChannel(ChannelData channel)
		{
		    return channel.StartBar >= 0 && 
		           channel.EndBar > channel.StartBar && 
		           !double.IsNaN(channel.StartPrice) && 
		           !double.IsNaN(channel.EndPrice);
		}

        private void DrawChannel(ChartControl chartControl, ChartScale chartScale, ChannelData channel)
        {
            int startX = chartControl.GetXByBarIndex(ChartBars, channel.StartBar);
            int endX = chartControl.GetXByBarIndex(ChartBars, channel.EndBar);
            
            float startPriceY = chartScale.GetYByValue(channel.StartPrice);
            float endPriceY = chartScale.GetYByValue(channel.EndPrice);
            float upperStartY = chartScale.GetYByValue(channel.UpperStartPrice);
            float upperEndY = chartScale.GetYByValue(channel.UpperEndPrice);
            float lowerStartY = chartScale.GetYByValue(channel.LowerStartPrice);
            float lowerEndY = chartScale.GetYByValue(channel.LowerEndPrice);

            // Draw lines
            RenderTarget.DrawLine(new Vector2(startX, startPriceY), new Vector2(endX, endPriceY), lineBrush, 1);
            RenderTarget.DrawLine(new Vector2(startX, upperStartY), new Vector2(endX, upperEndY), lineBrush, 1);
            RenderTarget.DrawLine(new Vector2(startX, lowerStartY), new Vector2(endX, lowerEndY), lineBrush, 1);

            // Draw fills
            DrawChannelFill(startX, endX, startPriceY, endPriceY, upperStartY, upperEndY, upperBrush);
            DrawChannelFill(startX, endX, startPriceY, endPriceY, lowerStartY, lowerEndY, lowerBrush);
        }

        private void DrawChannelFill(int startX, int endX, float midStartY, float midEndY, float edgeStartY, float edgeEndY, DxBrush brush)
		{
		    var geometry = new DxPathGeometry(Core.Globals.D2DFactory);
		    var sink = geometry.Open();
		    
		    sink.BeginFigure(new Vector2(startX, midStartY), FigureBegin.Filled);
		    sink.AddLine(new Vector2(endX, midEndY));
		    sink.AddLine(new Vector2(endX, edgeEndY));
		    sink.AddLine(new Vector2(startX, edgeStartY));
		    sink.EndFigure(FigureEnd.Closed);
		    sink.Close();
		    
		    RenderTarget.FillGeometry(geometry, brush);
		    geometry.Dispose();
		    sink.Dispose();
		}
        private (double slope, double average, double intercept) CalculateSlope(int length)
        {
            if (length <= 1 || CurrentBar < length - 1)
                return (double.NaN, double.NaN, double.NaN);

            double sumX = 0.0;
            double sumY = 0.0;
            double sumXSqr = 0.0;
            double sumXY = 0.0;

            for (int i = 0; i < length; i++)
            {
                double val = Close[i];
                double per = i + 1.0;
                sumX += per;
                sumY += val;
                sumXSqr += per * per;
                sumXY += val * per;
            }

            double slope = (length * sumXY - sumX * sumY) / (length * sumXSqr - sumX * sumX);
            double average = sumY / length;
            double intercept = average - slope * sumX / length + slope;

            return (slope, average, intercept);
        }

        private (double stdDev, double pearsonR, double upDev, double dnDev) CalculateDeviation(int length, double slope, double average, double intercept)
        {
            if (length <= 1 || CurrentBar < length - 1)
                return (double.NaN, double.NaN, double.NaN, double.NaN);

            double upDev = 0.0;
            double dnDev = 0.0;
            double stdDevAcc = 0.0;
            double dsxx = 0.0;
            double dsyy = 0.0;
            double dsxy = 0.0;
            int periods = length - 1;
            double daY = intercept + slope * periods / 2;
            double val = intercept;

            for (int j = 0; j <= periods; j++)
            {
                double price = High[j] - val;
                if (price > upDev)
                    upDev = price;
                
                price = val - Low[j];
                if (price > dnDev)
                    dnDev = price;
                
                price = Close[j];
                double dxt = price - average;
                double dyt = val - daY;
                price -= val;
                stdDevAcc += price * price;
                dsxx += dxt * dxt;
                dsyy += dyt * dyt;
                dsxy += dxt * dyt;
                val += slope;
            }

            double stdDev = Math.Sqrt(stdDevAcc / (periods == 0 ? 1 : periods));
            double pearsonR = dsxx == 0 || dsyy == 0 ? 0 : dsxy / Math.Sqrt(dsxx * dsyy);

            return (stdDev, pearsonR, upDev, dnDev);
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Upper Deviation", Order = 1, GroupName = "Parameters")]
        public double UpperDeviation { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Lower Deviation", Order = 2, GroupName = "Parameters")]
        public double LowerDeviation { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Upper Color", Order = 3, GroupName = "Visual")]
        public WpfBrush UpperColor { get; set; }

        [Browsable(false)]
        public string UpperColorSerializable
        {
            get { return Serialize.BrushToString(UpperColor); }
            set { UpperColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Lower Color", Order = 4, GroupName = "Visual")]
        public WpfBrush LowerColor { get; set; }

        [Browsable(false)]
        public string LowerColorSerializable
        {
            get { return Serialize.BrushToString(LowerColor); }
            set { LowerColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Line Color", Order = 5, GroupName = "Visual")]
        public WpfBrush LineColor { get; set; }

        [Browsable(false)]
        public string LineColorSerializable
        {
            get { return Serialize.BrushToString(LineColor); }
            set { LineColor = Serialize.StringToBrush(value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DynamicLinearRegressionChannels[] cacheDynamicLinearRegressionChannels;
		public DynamicLinearRegressionChannels DynamicLinearRegressionChannels(double upperDeviation, double lowerDeviation, WpfBrush upperColor, WpfBrush lowerColor, WpfBrush lineColor)
		{
			return DynamicLinearRegressionChannels(Input, upperDeviation, lowerDeviation, upperColor, lowerColor, lineColor);
		}

		public DynamicLinearRegressionChannels DynamicLinearRegressionChannels(ISeries<double> input, double upperDeviation, double lowerDeviation, WpfBrush upperColor, WpfBrush lowerColor, WpfBrush lineColor)
		{
			if (cacheDynamicLinearRegressionChannels != null)
				for (int idx = 0; idx < cacheDynamicLinearRegressionChannels.Length; idx++)
					if (cacheDynamicLinearRegressionChannels[idx] != null && cacheDynamicLinearRegressionChannels[idx].UpperDeviation == upperDeviation && cacheDynamicLinearRegressionChannels[idx].LowerDeviation == lowerDeviation && cacheDynamicLinearRegressionChannels[idx].UpperColor == upperColor && cacheDynamicLinearRegressionChannels[idx].LowerColor == lowerColor && cacheDynamicLinearRegressionChannels[idx].LineColor == lineColor && cacheDynamicLinearRegressionChannels[idx].EqualsInput(input))
						return cacheDynamicLinearRegressionChannels[idx];
			return CacheIndicator<DynamicLinearRegressionChannels>(new DynamicLinearRegressionChannels(){ UpperDeviation = upperDeviation, LowerDeviation = lowerDeviation, UpperColor = upperColor, LowerColor = lowerColor, LineColor = lineColor }, input, ref cacheDynamicLinearRegressionChannels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DynamicLinearRegressionChannels DynamicLinearRegressionChannels(double upperDeviation, double lowerDeviation, WpfBrush upperColor, WpfBrush lowerColor, WpfBrush lineColor)
		{
			return indicator.DynamicLinearRegressionChannels(Input, upperDeviation, lowerDeviation, upperColor, lowerColor, lineColor);
		}

		public Indicators.DynamicLinearRegressionChannels DynamicLinearRegressionChannels(ISeries<double> input , double upperDeviation, double lowerDeviation, WpfBrush upperColor, WpfBrush lowerColor, WpfBrush lineColor)
		{
			return indicator.DynamicLinearRegressionChannels(input, upperDeviation, lowerDeviation, upperColor, lowerColor, lineColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DynamicLinearRegressionChannels DynamicLinearRegressionChannels(double upperDeviation, double lowerDeviation, WpfBrush upperColor, WpfBrush lowerColor, WpfBrush lineColor)
		{
			return indicator.DynamicLinearRegressionChannels(Input, upperDeviation, lowerDeviation, upperColor, lowerColor, lineColor);
		}

		public Indicators.DynamicLinearRegressionChannels DynamicLinearRegressionChannels(ISeries<double> input , double upperDeviation, double lowerDeviation, WpfBrush upperColor, WpfBrush lowerColor, WpfBrush lineColor)
		{
			return indicator.DynamicLinearRegressionChannels(input, upperDeviation, lowerDeviation, upperColor, lowerColor, lineColor);
		}
	}
}

#endregion
