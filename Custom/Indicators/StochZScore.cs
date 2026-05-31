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
    public class StochasticZScore : Indicator
    {
        private SMA sma;
        private StdDev stdDev;
        
        private Series<double> zscore;
        private Series<double> stochZ;
        private Series<double> scaledSZ;
        private Series<double> smoothedScaled;
        private Series<double> ltm;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Stochastic Z-Score Oscillator with Long-Term Momentum";
                Name = "Stochastic Z-Score";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = true;
                ArePlotsConfigurable = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                Length = 21;
                ShowHistogram = false;
                BullishColor = Brushes.LimeGreen;
                BearishColor = Brushes.Red;
                
                AddPlot(new Stroke(Brushes.Transparent, 0), PlotStyle.Dot, "ZScore");
                AddPlot(new Stroke(Brushes.Gray, 3), PlotStyle.Line, "Oscillator");
                AddPlot(new Stroke(Brushes.Transparent, 0), PlotStyle.Dot, "OscillatorPrev");
                AddPlot(new Stroke(Brushes.Transparent, 0), PlotStyle.Dot, "Momentum");
                AddPlot(new Stroke(Brushes.Transparent, 0), PlotStyle.Dot, "Zero");
                AddPlot(new Stroke(Brushes.Orange, 3), PlotStyle.Line, "Upper");
                AddPlot(new Stroke(Brushes.DodgerBlue, 3), PlotStyle.Line, "Lower");
                AddPlot(new Stroke(Brushes.DarkRed, 3), PlotStyle.Line, "UpperOuter");
                AddPlot(new Stroke(Brushes.DarkGreen, 3), PlotStyle.Line, "LowerOuter");
            }
            else if (State == State.Configure)
            {
                zscore = new Series<double>(this);
                stochZ = new Series<double>(this);
                scaledSZ = new Series<double>(this);
                smoothedScaled = new Series<double>(this);
                ltm = new Series<double>(this);
            }
            else if (State == State.DataLoaded)
            {
                sma = SMA(Typical, Length);
                stdDev = StdDev(Typical, Length);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Length)
                return;
            
            // Calculate Z-Score
            double basis = sma[0];
            double stdev = stdDev[0];
            double zsc = stdev != 0 ? (Typical[0] - basis) / stdev : 0;
            zscore[0] = zsc;
            
            // Calculate Stochastic of Z-Score
            double highest = double.MinValue;
            double lowest = double.MaxValue;
            
            for (int i = 0; i < Length; i++)
            {
                if (CurrentBar >= i)
                {
                    if (zscore[i] > highest) highest = zscore[i];
                    if (zscore[i] < lowest) lowest = zscore[i];
                }
            }
            
            double stoch = (highest - lowest) != 0 ? 100 * (zscore[0] - lowest) / (highest - lowest) : 50;
            stochZ[0] = stoch;
            
            // Scale the stochastic
            scaledSZ[0] = stochZ[0] / 25 - 2;
            
            // Smooth with HMA
            if (CurrentBar >= Length)
            {
                smoothedScaled[0] = CalculateHMA(scaledSZ, Length);
            }
            
            // Calculate long-term momentum with ALMA
            ltm[0] = CalculateALMA(zscore, Length, 0, 0.1);
            
            // Plot values
            Values[0][0] = ShowHistogram ? zscore[0] : double.NaN;
            Values[1][0] = smoothedScaled[0];
            Values[2][0] = CurrentBar > 0 ? smoothedScaled[1] : smoothedScaled[0];
            Values[3][0] = ltm[0];
            Values[4][0] = 0;
            Values[5][0] = 2;
            Values[6][0] = -2;
            Values[7][0] = 2.5;
            Values[8][0] = -2.5;
            
            // Color the Z-Score histogram
            if (ShowHistogram)
            {
                double t = Math.Max(-2, Math.Min(2, zscore[0]));
                PlotBrushes[0][0] = GetGradientBrush(t, -2, 2, BearishColor, BullishColor);
            }
            
            // Color the smoothed oscillator
            bool isRising = CurrentBar > 0 && smoothedScaled[0] > smoothedScaled[1];
            PlotBrushes[1][0] = isRising ? Brushes.Gray : Brushes.DarkGray;
            
            // Color the bounds
            double upperGrad = Math.Max(0, smoothedScaled[0]);
            double lowerGrad = Math.Min(0, smoothedScaled[0]);
            
            PlotBrushes[5][0] = GetGradientBrush(upperGrad, 0, 2, Brushes.Gray, BearishColor);
            PlotBrushes[6][0] = GetGradientBrush(lowerGrad, -2, 0, BullishColor, Brushes.Gray);
            PlotBrushes[7][0] = GetGradientBrush(upperGrad, 0, 2.5, Brushes.Gray, BearishColor);
            PlotBrushes[8][0] = GetGradientBrush(lowerGrad, -2.5, 0, BullishColor, Brushes.Gray);
            
            // Draw signals
            if (CurrentBar < 1) return;
            
            bool momentumShift = (smoothedScaled[0] > smoothedScaled[1] && smoothedScaled[1] <= smoothedScaled[2]) ||
                                (smoothedScaled[0] < smoothedScaled[1] && smoothedScaled[1] >= smoothedScaled[2]);
            
            bool bullishReversal = smoothedScaled[0] > smoothedScaled[1] && smoothedScaled[1] <= smoothedScaled[2] &&
                                  ltm[0] > 0 && smoothedScaled[1] < -2;
            
            bool bearishReversal = smoothedScaled[0] < smoothedScaled[1] && smoothedScaled[1] >= smoothedScaled[2] &&
                                  ltm[0] < 0 && smoothedScaled[1] > 2;
            
            if (momentumShift)
            {
                Draw.Dot(this, "MomentumShift" + CurrentBar, false, 0, smoothedScaled[1], Brushes.White);
            }
            
            if (bullishReversal)
            {
                Draw.TriangleUp(this, "BullUp" + CurrentBar, false, 0, -3, BullishColor);
                Draw.ArrowUp(this, "BullArrow" + CurrentBar, false, 0, Low[0] - TickSize * 10, BullishColor);
            }
            
            if (bearishReversal)
            {
                Draw.TriangleDown(this, "BearDown" + CurrentBar, false, 0, 3, BearishColor);
                Draw.ArrowDown(this, "BearArrow" + CurrentBar, false, 0, High[0] + TickSize * 10, BearishColor);
            }
        }
        
        private double CalculateHMA(Series<double> series, int period)
        {
            if (CurrentBar < period) return 0;
            
            int halfPeriod = period / 2;
            int sqrtPeriod = (int)Math.Sqrt(period);
            
            // WMA of half period
            double wma1 = CalculateWMA(series, halfPeriod);
            // WMA of full period
            double wma2 = CalculateWMA(series, period);
            // 2 * wma1 - wma2
            double diff = 2 * wma1 - wma2;
            
            // Store in temp series for final WMA
            if (!hmaDiff.ContainsKey(CurrentBar))
                hmaDiff[CurrentBar] = diff;
            
            return CalculateWMAFromDict(hmaDiff, sqrtPeriod);
        }
        
        private Dictionary<int, double> hmaDiff = new Dictionary<int, double>();
        
        private double CalculateWMA(Series<double> series, int period)
        {
            double sum = 0;
            double weightSum = 0;
            
            for (int i = 0; i < period && CurrentBar >= i; i++)
            {
                double weight = period - i;
                sum += series[i] * weight;
                weightSum += weight;
            }
            
            return weightSum != 0 ? sum / weightSum : 0;
        }
        
        private double CalculateWMAFromDict(Dictionary<int, double> dict, int period)
        {
            double sum = 0;
            double weightSum = 0;
            
            for (int i = 0; i < period && CurrentBar >= i; i++)
            {
                int bar = CurrentBar - i;
                if (dict.ContainsKey(bar))
                {
                    double weight = period - i;
                    sum += dict[bar] * weight;
                    weightSum += weight;
                }
            }
            
            return weightSum != 0 ? sum / weightSum : 0;
        }
        
        private double CalculateALMA(Series<double> series, int period, double offset, double sigma)
        {
            if (CurrentBar < period) return 0;
            
            double m = Math.Floor(offset * (period - 1));
            double s = period / sigma;
            double norm = 0;
            double sum = 0;
            
            for (int i = 0; i < period && CurrentBar >= i; i++)
            {
                double weight = Math.Exp(-1 * Math.Pow(i - m, 2) / (2 * Math.Pow(s, 2)));
                norm += weight;
                sum += series[i] * weight;
            }
            
            return norm != 0 ? sum / norm : 0;
        }
        
        private Brush GetGradientBrush(double value, double min, double max, Brush minBrush, Brush maxBrush)
        {
            double normalized = (value - min) / (max - min);
            normalized = Math.Max(0, Math.Min(1, normalized));
            
            Color minColor = ((SolidColorBrush)minBrush).Color;
            Color maxColor = ((SolidColorBrush)maxBrush).Color;
            
            byte r = (byte)(minColor.R + (maxColor.R - minColor.R) * normalized);
            byte g = (byte)(minColor.G + (maxColor.G - minColor.G) * normalized);
            byte b = (byte)(minColor.B + (maxColor.B - minColor.B) * normalized);
            
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        
        #region Properties
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Length", Description = "Look-back period", Order = 1, GroupName = "Parameters")]
        public int Length { get; set; }
        
        [NinjaScriptProperty]
        [Display(Name = "Show Histogram", Description = "Display raw Z-Score histogram", Order = 2, GroupName = "Parameters")]
        public bool ShowHistogram { get; set; }
        
        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish Color", Description = "Color for bullish signals", Order = 3, GroupName = "Parameters")]
        public Brush BullishColor { get; set; }
        
        [Browsable(false)]
        public string BullishColorSerializable
        {
            get { return Serialize.BrushToString(BullishColor); }
            set { BullishColor = Serialize.StringToBrush(value); }
        }
        
        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish Color", Description = "Color for bearish signals", Order = 4, GroupName = "Parameters")]
        public Brush BearishColor { get; set; }
        
        [Browsable(false)]
        public string BearishColorSerializable
        {
            get { return Serialize.BrushToString(BearishColor); }
            set { BearishColor = Serialize.StringToBrush(value); }
        }
        
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private StochasticZScore[] cacheStochasticZScore;
		public StochasticZScore StochasticZScore(int length, bool showHistogram, Brush bullishColor, Brush bearishColor)
		{
			return StochasticZScore(Input, length, showHistogram, bullishColor, bearishColor);
		}

		public StochasticZScore StochasticZScore(ISeries<double> input, int length, bool showHistogram, Brush bullishColor, Brush bearishColor)
		{
			if (cacheStochasticZScore != null)
				for (int idx = 0; idx < cacheStochasticZScore.Length; idx++)
					if (cacheStochasticZScore[idx] != null && cacheStochasticZScore[idx].Length == length && cacheStochasticZScore[idx].ShowHistogram == showHistogram && cacheStochasticZScore[idx].BullishColor == bullishColor && cacheStochasticZScore[idx].BearishColor == bearishColor && cacheStochasticZScore[idx].EqualsInput(input))
						return cacheStochasticZScore[idx];
			return CacheIndicator<StochasticZScore>(new StochasticZScore(){ Length = length, ShowHistogram = showHistogram, BullishColor = bullishColor, BearishColor = bearishColor }, input, ref cacheStochasticZScore);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.StochasticZScore StochasticZScore(int length, bool showHistogram, Brush bullishColor, Brush bearishColor)
		{
			return indicator.StochasticZScore(Input, length, showHistogram, bullishColor, bearishColor);
		}

		public Indicators.StochasticZScore StochasticZScore(ISeries<double> input , int length, bool showHistogram, Brush bullishColor, Brush bearishColor)
		{
			return indicator.StochasticZScore(input, length, showHistogram, bullishColor, bearishColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.StochasticZScore StochasticZScore(int length, bool showHistogram, Brush bullishColor, Brush bearishColor)
		{
			return indicator.StochasticZScore(Input, length, showHistogram, bullishColor, bearishColor);
		}

		public Indicators.StochasticZScore StochasticZScore(ISeries<double> input , int length, bool showHistogram, Brush bullishColor, Brush bearishColor)
		{
			return indicator.StochasticZScore(input, length, showHistogram, bullishColor, bearishColor);
		}
	}
}

#endregion
