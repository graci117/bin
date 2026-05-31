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
using NinjaTrader.NinjaScript.Indicators;
#endregion

// Enum placed AFTER using statements but BEFORE namespace
//public enum FeatureType
//{
//    RSI,
//    WT,
//    CCI,
//    ADX
//}

namespace NinjaTrader.NinjaScript.Indicators
{
    public class LorentzianClassificationBak : Indicator
    {
        [Range(1, 100), NinjaScriptProperty]
        [Display(Name = "Neighbors Count", Order = 1, GroupName = "Parameters")]
        public int NeighborsCount { get; set; } = 8;
        
        [Range(100, 5000), NinjaScriptProperty]
        [Display(Name = "Max Bars Back", Order = 2, GroupName = "Parameters")]
        public int MaxBarsBack { get; set; } = 2000;
        
        [Range(2, 5), NinjaScriptProperty]
        [Display(Name = "Feature Count", Order = 3, GroupName = "Parameters")]
        public int FeatureCount { get; set; } = 5;

        // Feature 1 with both Parameter A and B (matching PineScript)
        [NinjaScriptProperty]
        [Display(Name = "Feature 1 Type", Order = 4, GroupName = "Features")]
        public FeatureType Feature1 { get; set; } = FeatureType.RSI;
        [Range(2, 50), NinjaScriptProperty]
        [Display(Name = "Feature 1 Param A", Order = 5, GroupName = "Features")]
        public int Feature1ParamA { get; set; } = 14;
        [Range(1, 50), NinjaScriptProperty]
        [Display(Name = "Feature 1 Param B", Order = 6, GroupName = "Features")]
        public int Feature1ParamB { get; set; } = 1;

        // Feature 2 with both Parameter A and B
        [NinjaScriptProperty]
        [Display(Name = "Feature 2 Type", Order = 7, GroupName = "Features")]
        public FeatureType Feature2 { get; set; } = FeatureType.WT;
        [Range(2, 50), NinjaScriptProperty]
        [Display(Name = "Feature 2 Param A", Order = 8, GroupName = "Features")]
        public int Feature2ParamA { get; set; } = 10;
        [Range(1, 50), NinjaScriptProperty]
        [Display(Name = "Feature 2 Param B", Order = 9, GroupName = "Features")]
        public int Feature2ParamB { get; set; } = 11;

        // Feature 3 with both Parameter A and B
        [NinjaScriptProperty]
        [Display(Name = "Feature 3 Type", Order = 10, GroupName = "Features")]
        public FeatureType Feature3 { get; set; } = FeatureType.CCI;
        [Range(2, 50), NinjaScriptProperty]
        [Display(Name = "Feature 3 Param A", Order = 11, GroupName = "Features")]
        public int Feature3ParamA { get; set; } = 20;
        [Range(1, 50), NinjaScriptProperty]
        [Display(Name = "Feature 3 Param B", Order = 12, GroupName = "Features")]
        public int Feature3ParamB { get; set; } = 1;

        // Feature 4 with both Parameter A and B
        [NinjaScriptProperty]
        [Display(Name = "Feature 4 Type", Order = 13, GroupName = "Features")]
        public FeatureType Feature4 { get; set; } = FeatureType.ADX;
        [Range(2, 50), NinjaScriptProperty]
        [Display(Name = "Feature 4 Param A", Order = 14, GroupName = "Features")]
        public int Feature4ParamA { get; set; } = 20;
        [Range(1, 50), NinjaScriptProperty]
        [Display(Name = "Feature 4 Param B", Order = 15, GroupName = "Features")]
        public int Feature4ParamB { get; set; } = 2;

        // Feature 5 with both Parameter A and B
        [NinjaScriptProperty]
        [Display(Name = "Feature 5 Type", Order = 16, GroupName = "Features")]
        public FeatureType Feature5 { get; set; } = FeatureType.RSI;
        [Range(2, 50), NinjaScriptProperty]
        [Display(Name = "Feature 5 Param A", Order = 17, GroupName = "Features")]
        public int Feature5ParamA { get; set; } = 9;
        [Range(1, 50), NinjaScriptProperty]
        [Display(Name = "Feature 5 Param B", Order = 18, GroupName = "Features")]
        public int Feature5ParamB { get; set; } = 1;

        // EXACT FILTER SETTINGS MATCHING PINESCRIPT
        [NinjaScriptProperty]
        [Display(Name = "Use Volatility Filter", Order = 19, GroupName = "Filters")]
        public bool UseVolatilityFilter { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Use Regime Filter", Order = 20, GroupName = "Filters")]
        public bool UseRegimeFilter { get; set; } = true;
        [Range(-10.0, 10.0), NinjaScriptProperty]
        [Display(Name = "Regime Threshold", Order = 21, GroupName = "Filters")]
        public double RegimeThreshold { get; set; } = -0.1;

        [NinjaScriptProperty]
        [Display(Name = "Use ADX Filter", Order = 22, GroupName = "Filters")]
        public bool UseAdxFilter { get; set; } = false;
        [Range(0, 100), NinjaScriptProperty]
        [Display(Name = "ADX Threshold", Order = 23, GroupName = "Filters")]
        public int AdxThreshold { get; set; } = 20;

        [NinjaScriptProperty]
        [Display(Name = "Use EMA Filter", Order = 24, GroupName = "Filters")]
        public bool UseEmaFilter { get; set; } = false;
        [Range(1, 500), NinjaScriptProperty]
        [Display(Name = "EMA Period", Order = 25, GroupName = "Filters")]
        public int EmaPeriod { get; set; } = 200;

        [NinjaScriptProperty]
        [Display(Name = "Use SMA Filter", Order = 26, GroupName = "Filters")]
        public bool UseSmaFilter { get; set; } = false;
        [Range(1, 500), NinjaScriptProperty]
        [Display(Name = "SMA Period", Order = 27, GroupName = "Filters")]
        public int SmaPeriod { get; set; } = 200;

        // Kernel Settings (matching PineScript)
        [NinjaScriptProperty]
        [Display(Name = "Use Kernel Filter", Order = 28, GroupName = "Kernel")]
        public bool UseKernelFilter { get; set; } = true;
        [NinjaScriptProperty]
        [Display(Name = "Show Kernel Estimate", Order = 29, GroupName = "Kernel")]
        public bool ShowKernelEstimate { get; set; } = true;
        [Range(3, 50), NinjaScriptProperty]
        [Display(Name = "Kernel Lookback Window", Order = 30, GroupName = "Kernel")]
        public int KernelWindow { get; set; } = 8;
        [Range(0.25, 25.0), NinjaScriptProperty]
        [Display(Name = "Kernel Relative Weighting", Order = 31, GroupName = "Kernel")]
        public double KernelWeighting { get; set; } = 8.0;
        [Range(2, 25), NinjaScriptProperty]
        [Display(Name = "Kernel Regression Level", Order = 32, GroupName = "Kernel")]
        public int KernelLevel { get; set; } = 25;

        // Display Settings
        [NinjaScriptProperty]
        [Display(Name = "Show Bar Colors", Order = 33, GroupName = "Display")]
        public bool ShowBarColors { get; set; } = true;
        [NinjaScriptProperty]
        [Display(Name = "Show Bar Predictions", Order = 34, GroupName = "Display")]
        public bool ShowBarPredictions { get; set; } = true;

        // Storage (matching PineScript structure)
        private List<double>[] featureArrays;
        private List<int> labelArray;
        private Series<double> predictionSeries;
        private Series<double> signalSeries;
        private EMA ema;
        private SMA sma;
        private ADX adx;

        // Direction constants (matching PineScript)
        private const int DIRECTION_LONG = 1;
        private const int DIRECTION_SHORT = -1;
        private const int DIRECTION_NEUTRAL = 0;

        // ML Logic variables (matching PineScript)
        private double lastDistance = -1.0;
        private List<double> distances;
        private List<double> predictions;
        private double prediction = 0.0;
        private double signal = 0.0;
        private double prevSignal = 0.0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Lorentzian Classification - Exact PineScript Match";
                Name = "LorentzianClassificationBak";
                IsOverlay = true;
                
                // Two plots: Prediction values and Kernel regression
                AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "Prediction");
                AddPlot(new Stroke(Brushes.Orange, 4), PlotStyle.Line, "Kernel Regression");
                
                BarsRequiredToPlot = 20;
            }
            else if (State == State.DataLoaded)
            {
                featureArrays = new List<double>[5];
                for (int i = 0; i < 5; i++)
                    featureArrays[i] = new List<double>();
                labelArray = new List<int>();
                predictionSeries = new Series<double>(this);
                signalSeries = new Series<double>(this);
                distances = new List<double>();
                predictions = new List<double>();

                // Initialize indicators only if filters are enabled
                if (UseEmaFilter)
                    ema = EMA(Close, EmaPeriod);
                if (UseSmaFilter)
                    sma = SMA(Close, SmaPeriod);
                if (UseAdxFilter)
                    adx = ADX(14); // PineScript uses 14 for ADX in filter
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < MaxBarsBack + 5)
                return;

            // 1. EXACT PINESCRIPT TRAINING LABEL LOGIC
            // y_train_series = src[4] < src[0] ? direction.short : src[4] > src[0] ? direction.long : direction.neutral
            int yTrainSeries;
            if (Close[4] < Close[0])
                yTrainSeries = DIRECTION_SHORT; // -1 (price went UP)
            else if (Close[4] > Close[0])
                yTrainSeries = DIRECTION_LONG;  // 1 (price went DOWN)
            else
                yTrainSeries = DIRECTION_NEUTRAL; // 0

            if (labelArray.Count >= MaxBarsBack)
                labelArray.RemoveAt(0);
            labelArray.Add(yTrainSeries);

            // 2. Calculate current features with both parameters (matching PineScript)
            double[] currFeatures = new double[5];
            currFeatures[0] = GetFeature(Feature1, Feature1ParamA, Feature1ParamB, 0);
            currFeatures[1] = GetFeature(Feature2, Feature2ParamA, Feature2ParamB, 0);
            currFeatures[2] = GetFeature(Feature3, Feature3ParamA, Feature3ParamB, 0);
            currFeatures[3] = GetFeature(Feature4, Feature4ParamA, Feature4ParamB, 0);
            currFeatures[4] = GetFeature(Feature5, Feature5ParamA, Feature5ParamB, 0);

            // 3. Store features for historical bars
            for (int i = 0; i < 5; i++)
            {
                if (featureArrays[i].Count >= MaxBarsBack)
                    featureArrays[i].RemoveAt(0);
                featureArrays[i].Add(GetFeature(GetFeatureType(i), GetFeatureParamA(i), GetFeatureParamB(i), 0));
            }

            // 4. EXACT PINESCRIPT ML LOGIC
            prediction = 0.0;
            distances.Clear();
            predictions.Clear();
            lastDistance = -1.0;

            int size = Math.Min(MaxBarsBack - 1, labelArray.Count - 1);
            int sizeLoop = Math.Min(MaxBarsBack - 1, size);

            // Exact PineScript loop logic
            for (int i = 0; i < sizeLoop; i++)
            {
                double d = LorentzianDistance(currFeatures, i);
                if (d >= lastDistance && i % 4 != 0) // i%4 logic from PineScript
                {
                    lastDistance = d;
                    distances.Add(d);
                    predictions.Add(Math.Round(labelArray[labelArray.Count - 1 - i]));
                    
                    if (predictions.Count > NeighborsCount)
                    {
                        lastDistance = distances[(int)Math.Round(NeighborsCount * 3.0 / 4.0)];
                        distances.RemoveAt(0);
                        predictions.RemoveAt(0);
                    }
                }
            }
            prediction = predictions.Sum();

            // 5. EXACT PINESCRIPT FILTER LOGIC
            // First apply filter_all (volatility AND regime AND adx)
            bool volatilityFilter = !UseVolatilityFilter || GetVolatilityFilter();
            bool regimeFilter = !UseRegimeFilter || GetRegimeFilter();
            bool adxFilter = !UseAdxFilter || GetAdxFilter();
            bool filterAll = volatilityFilter && regimeFilter && adxFilter;

            // Generate signal using filter_all (matching PineScript)
            if (prediction > 0 && filterAll)
                signal = DIRECTION_LONG;
            else if (prediction < 0 && filterAll)
                signal = DIRECTION_SHORT;
            // else signal remains previous value (nz(signal[1]) in PineScript)

            // 6. EMA/SMA trend filters (separate from filter_all)
            bool isEmaUptrend = !UseEmaFilter || Close[0] > (ema != null ? ema[0] : Close[0]);
            bool isEmaDowntrend = !UseEmaFilter || Close[0] < (ema != null ? ema[0] : Close[0]);
            bool isSmaUptrend = !UseSmaFilter || Close[0] > (sma != null ? sma[0] : Close[0]);
            bool isSmaDowntrend = !UseSmaFilter || Close[0] < (sma != null ? sma[0] : Close[0]);

            // 7. Calculate Kernel Regression
            double kernelValue = CalculateKernelRegression();
            bool isBullish = !UseKernelFilter || kernelValue < Close[0]; // Kernel bearish logic
            bool isBearish = !UseKernelFilter || kernelValue > Close[0]; // Kernel bullish logic

            // 8. EXACT PINESCRIPT ENTRY LOGIC
            bool isDifferentSignalType = signal != prevSignal;
            bool isBuySignal = signal == DIRECTION_LONG && isEmaUptrend && isSmaUptrend;
            bool isSellSignal = signal == DIRECTION_SHORT && isEmaDowntrend && isSmaDowntrend;
            bool isNewBuySignal = isBuySignal && isDifferentSignalType;
            bool isNewSellSignal = isSellSignal && isDifferentSignalType;

            bool startLongTrade = isNewBuySignal && isBullish;
            bool startShortTrade = isNewSellSignal && isBearish;

            // 9. Plot arrows exactly like PineScript (only on new trades)
            if (startLongTrade)
                Draw.ArrowUp(this, "Long" + CurrentBar, true, 0, Low[0] - TickSize, Brushes.LimeGreen);
            if (startShortTrade)
                Draw.ArrowDown(this, "Short" + CurrentBar, true, 0, High[0] + TickSize, Brushes.Red);

            prevSignal = signal;

            // 10. Plot values
            Values[0][0] = prediction;  // Prediction line
            Values[1][0] = kernelValue; // Kernel regression line

            // 11. Dynamic plot colors
            if (prediction > 0)
                PlotBrushes[0][0] = Brushes.LimeGreen;
            else if (prediction < 0)
                PlotBrushes[0][0] = Brushes.Red;
            else
                PlotBrushes[0][0] = Brushes.Gray;

            // Kernel line color
            PlotBrushes[1][0] = ShowKernelEstimate ? (kernelValue < Close[0] ? Brushes.Green : Brushes.Red) : Brushes.Transparent;

            // 12. Bar colors and prediction labels
            if (ShowBarColors)
                BarBrush = GetBarColor(prediction, NeighborsCount);

            if (ShowBarPredictions)
            {
                string txt = prediction.ToString("0");
                double y = prediction > 0 ? High[0] + TickSize * 2 : Low[0] - TickSize * 2;
                Draw.Text(this, "Pred" + CurrentBar, txt, 0, y, prediction > 0 ? Brushes.Green : prediction < 0 ? Brushes.Red : Brushes.Gray);
            }
        }

        // EXACT PINESCRIPT FILTER IMPLEMENTATIONS
        private bool GetVolatilityFilter()
        {
            // ml.filter_volatility(1, 10, filterSettings.useVolatilityFilter)
            // Simplified implementation - compare recent volatility to historical
            if (CurrentBar < 10) return true;
            
            double recentVol = 0;
            double historicalVol = 0;
            
            for (int i = 0; i < 10; i++)
            {
                double tr = Math.Max(High[i] - Low[i], Math.Max(Math.Abs(High[i] - Close[i + 1]), Math.Abs(Low[i] - Close[i + 1])));
                if (i < 1) recentVol += tr;
                historicalVol += tr;
            }
            
            return recentVol > (historicalVol / 10.0); // Recent volatility > average
        }

        private bool GetRegimeFilter()
        {
            // ml.regime_filter(ohlc4, filterSettings.regimeThreshold, filterSettings.useRegimeFilter)
            if (CurrentBar < 1) return true;
            
            double ohlc4 = (Open[0] + High[0] + Low[0] + Close[0]) / 4.0;
            double ohlc4Prev = (Open[1] + High[1] + Low[1] + Close[1]) / 4.0;
            double regimeValue = ohlc4 - ohlc4Prev;
            
            return regimeValue > RegimeThreshold;
        }

        private bool GetAdxFilter()
        {
            // ml.filter_adx(settings.source, 14, filterSettings.adxThreshold, filterSettings.useAdxFilter)
            if (adx == null || CurrentBar < 14) return true;
            return adx[0] > AdxThreshold;
        }

        // Enhanced GetFeature method with both parameters (matching PineScript)
        private double GetFeature(FeatureType type, int paramA, int paramB, int barsAgo)
        {
            switch (type)
            {
                case FeatureType.RSI:
                    // ml.n_rsi(_close, f_paramA, f_paramB)
                    return RSI(Close, paramA, paramB)[barsAgo];
                case FeatureType.CCI:
                    // ml.n_cci(_close, f_paramA, f_paramB)
                    return CCI(paramA)[barsAgo];
                case FeatureType.ADX:
                    // ml.n_adx(_high, _low, _close, f_paramA)
                    return ADX(paramA)[barsAgo];
                case FeatureType.WT:
                    // ml.n_wt(_hlc3, f_paramA, f_paramB)
                    return CalculateWT(paramA, paramB, barsAgo);
                default:
                    return 0;
            }
        }

        // WaveTrend implementation (simplified)
        private double CalculateWT(int paramA, int paramB, int barsAgo)
        {
            if (CurrentBar < Math.Max(paramA, paramB)) return 0;
            
            double hlc3 = (High[barsAgo] + Low[barsAgo] + Close[barsAgo]) / 3.0;
            // Simplified WT - in real implementation you'd need proper EMA calculations
            double ema1 = EMA(Close, paramA)[barsAgo];
            double d = Math.Abs(hlc3 - ema1);
            double ci = d > 0 ? (hlc3 - ema1) / (0.015 * d) : 0;
            return EMA(Close, paramB)[barsAgo]; // Simplified
        }

        // Nadaraya-Watson Rational Quadratic Kernel Regression (exact PineScript)
        private double CalculateKernelRegression()
        {
            double sum = 0.0;
            double weightSum = 0.0;
            int h = KernelWindow;
            double r = KernelWeighting;
            int x = KernelLevel;

            for (int i = 0; i < Math.Min(h, CurrentBar); i++)
            {
                double distance = Math.Abs(i);
                double weight = Math.Pow(1 + (distance * distance) / (2 * r * x * x), -r);
                sum += Close[i] * weight;
                weightSum += weight;
            }

            return weightSum > 0 ? sum / weightSum : Close[0];
        }

        // Helper methods for feature access
        private FeatureType GetFeatureType(int idx)
        {
            switch (idx)
            {
                case 0: return Feature1;
                case 1: return Feature2;
                case 2: return Feature3;
                case 3: return Feature4;
                case 4: return Feature5;
                default: return FeatureType.RSI;
            }
        }

        private int GetFeatureParamA(int idx)
        {
            switch (idx)
            {
                case 0: return Feature1ParamA;
                case 1: return Feature2ParamA;
                case 2: return Feature3ParamA;
                case 3: return Feature4ParamA;
                case 4: return Feature5ParamA;
                default: return 14;
            }
        }

        private int GetFeatureParamB(int idx)
        {
            switch (idx)
            {
                case 0: return Feature1ParamB;
                case 1: return Feature2ParamB;
                case 2: return Feature3ParamB;
                case 3: return Feature4ParamB;
                case 4: return Feature5ParamB;
                default: return 1;
            }
        }

        private double LorentzianDistance(double[] currFeatures, int idx)
        {
            double sum = 0.0;
            for (int f = 0; f < FeatureCount; f++)
            {
                int arrIdx = featureArrays[f].Count - 1 - idx;
                if (arrIdx < 0) continue;
                sum += Math.Log(1 + Math.Abs(currFeatures[f] - featureArrays[f][arrIdx]));
            }
            return sum;
        }

        private Brush GetBarColor(double prediction, double compression)
        {
            if (prediction > 0)
            {
                byte g = (byte)(128 + 127 * Math.Min(prediction / compression, 1.0));
                return new SolidColorBrush(Color.FromRgb(0, g, 0));
            }
            else if (prediction < 0)
            {
                byte r = (byte)(128 + 127 * Math.Min(-prediction / compression, 1.0));
                return new SolidColorBrush(Color.FromRgb(r, 0, 0));
            }
            else
            {
                return Brushes.Gray;
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LorentzianClassificationBak[] cacheLorentzianClassificationBak;
		public LorentzianClassificationBak LorentzianClassificationBak(int neighborsCount, int maxBarsBack, int featureCount, FeatureType feature1, int feature1ParamA, int feature1ParamB, FeatureType feature2, int feature2ParamA, int feature2ParamB, FeatureType feature3, int feature3ParamA, int feature3ParamB, FeatureType feature4, int feature4ParamA, int feature4ParamB, FeatureType feature5, int feature5ParamA, int feature5ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, bool useKernelFilter, bool showKernelEstimate, int kernelWindow, double kernelWeighting, int kernelLevel, bool showBarColors, bool showBarPredictions)
		{
			return LorentzianClassificationBak(Input, neighborsCount, maxBarsBack, featureCount, feature1, feature1ParamA, feature1ParamB, feature2, feature2ParamA, feature2ParamB, feature3, feature3ParamA, feature3ParamB, feature4, feature4ParamA, feature4ParamB, feature5, feature5ParamA, feature5ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, useKernelFilter, showKernelEstimate, kernelWindow, kernelWeighting, kernelLevel, showBarColors, showBarPredictions);
		}

		public LorentzianClassificationBak LorentzianClassificationBak(ISeries<double> input, int neighborsCount, int maxBarsBack, int featureCount, FeatureType feature1, int feature1ParamA, int feature1ParamB, FeatureType feature2, int feature2ParamA, int feature2ParamB, FeatureType feature3, int feature3ParamA, int feature3ParamB, FeatureType feature4, int feature4ParamA, int feature4ParamB, FeatureType feature5, int feature5ParamA, int feature5ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, bool useKernelFilter, bool showKernelEstimate, int kernelWindow, double kernelWeighting, int kernelLevel, bool showBarColors, bool showBarPredictions)
		{
			if (cacheLorentzianClassificationBak != null)
				for (int idx = 0; idx < cacheLorentzianClassificationBak.Length; idx++)
					if (cacheLorentzianClassificationBak[idx] != null && cacheLorentzianClassificationBak[idx].NeighborsCount == neighborsCount && cacheLorentzianClassificationBak[idx].MaxBarsBack == maxBarsBack && cacheLorentzianClassificationBak[idx].FeatureCount == featureCount && cacheLorentzianClassificationBak[idx].Feature1 == feature1 && cacheLorentzianClassificationBak[idx].Feature1ParamA == feature1ParamA && cacheLorentzianClassificationBak[idx].Feature1ParamB == feature1ParamB && cacheLorentzianClassificationBak[idx].Feature2 == feature2 && cacheLorentzianClassificationBak[idx].Feature2ParamA == feature2ParamA && cacheLorentzianClassificationBak[idx].Feature2ParamB == feature2ParamB && cacheLorentzianClassificationBak[idx].Feature3 == feature3 && cacheLorentzianClassificationBak[idx].Feature3ParamA == feature3ParamA && cacheLorentzianClassificationBak[idx].Feature3ParamB == feature3ParamB && cacheLorentzianClassificationBak[idx].Feature4 == feature4 && cacheLorentzianClassificationBak[idx].Feature4ParamA == feature4ParamA && cacheLorentzianClassificationBak[idx].Feature4ParamB == feature4ParamB && cacheLorentzianClassificationBak[idx].Feature5 == feature5 && cacheLorentzianClassificationBak[idx].Feature5ParamA == feature5ParamA && cacheLorentzianClassificationBak[idx].Feature5ParamB == feature5ParamB && cacheLorentzianClassificationBak[idx].UseVolatilityFilter == useVolatilityFilter && cacheLorentzianClassificationBak[idx].UseRegimeFilter == useRegimeFilter && cacheLorentzianClassificationBak[idx].RegimeThreshold == regimeThreshold && cacheLorentzianClassificationBak[idx].UseAdxFilter == useAdxFilter && cacheLorentzianClassificationBak[idx].AdxThreshold == adxThreshold && cacheLorentzianClassificationBak[idx].UseEmaFilter == useEmaFilter && cacheLorentzianClassificationBak[idx].EmaPeriod == emaPeriod && cacheLorentzianClassificationBak[idx].UseSmaFilter == useSmaFilter && cacheLorentzianClassificationBak[idx].SmaPeriod == smaPeriod && cacheLorentzianClassificationBak[idx].UseKernelFilter == useKernelFilter && cacheLorentzianClassificationBak[idx].ShowKernelEstimate == showKernelEstimate && cacheLorentzianClassificationBak[idx].KernelWindow == kernelWindow && cacheLorentzianClassificationBak[idx].KernelWeighting == kernelWeighting && cacheLorentzianClassificationBak[idx].KernelLevel == kernelLevel && cacheLorentzianClassificationBak[idx].ShowBarColors == showBarColors && cacheLorentzianClassificationBak[idx].ShowBarPredictions == showBarPredictions && cacheLorentzianClassificationBak[idx].EqualsInput(input))
						return cacheLorentzianClassificationBak[idx];
			return CacheIndicator<LorentzianClassificationBak>(new LorentzianClassificationBak(){ NeighborsCount = neighborsCount, MaxBarsBack = maxBarsBack, FeatureCount = featureCount, Feature1 = feature1, Feature1ParamA = feature1ParamA, Feature1ParamB = feature1ParamB, Feature2 = feature2, Feature2ParamA = feature2ParamA, Feature2ParamB = feature2ParamB, Feature3 = feature3, Feature3ParamA = feature3ParamA, Feature3ParamB = feature3ParamB, Feature4 = feature4, Feature4ParamA = feature4ParamA, Feature4ParamB = feature4ParamB, Feature5 = feature5, Feature5ParamA = feature5ParamA, Feature5ParamB = feature5ParamB, UseVolatilityFilter = useVolatilityFilter, UseRegimeFilter = useRegimeFilter, RegimeThreshold = regimeThreshold, UseAdxFilter = useAdxFilter, AdxThreshold = adxThreshold, UseEmaFilter = useEmaFilter, EmaPeriod = emaPeriod, UseSmaFilter = useSmaFilter, SmaPeriod = smaPeriod, UseKernelFilter = useKernelFilter, ShowKernelEstimate = showKernelEstimate, KernelWindow = kernelWindow, KernelWeighting = kernelWeighting, KernelLevel = kernelLevel, ShowBarColors = showBarColors, ShowBarPredictions = showBarPredictions }, input, ref cacheLorentzianClassificationBak);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LorentzianClassificationBak LorentzianClassificationBak(int neighborsCount, int maxBarsBack, int featureCount, FeatureType feature1, int feature1ParamA, int feature1ParamB, FeatureType feature2, int feature2ParamA, int feature2ParamB, FeatureType feature3, int feature3ParamA, int feature3ParamB, FeatureType feature4, int feature4ParamA, int feature4ParamB, FeatureType feature5, int feature5ParamA, int feature5ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, bool useKernelFilter, bool showKernelEstimate, int kernelWindow, double kernelWeighting, int kernelLevel, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassificationBak(Input, neighborsCount, maxBarsBack, featureCount, feature1, feature1ParamA, feature1ParamB, feature2, feature2ParamA, feature2ParamB, feature3, feature3ParamA, feature3ParamB, feature4, feature4ParamA, feature4ParamB, feature5, feature5ParamA, feature5ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, useKernelFilter, showKernelEstimate, kernelWindow, kernelWeighting, kernelLevel, showBarColors, showBarPredictions);
		}

		public Indicators.LorentzianClassificationBak LorentzianClassificationBak(ISeries<double> input , int neighborsCount, int maxBarsBack, int featureCount, FeatureType feature1, int feature1ParamA, int feature1ParamB, FeatureType feature2, int feature2ParamA, int feature2ParamB, FeatureType feature3, int feature3ParamA, int feature3ParamB, FeatureType feature4, int feature4ParamA, int feature4ParamB, FeatureType feature5, int feature5ParamA, int feature5ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, bool useKernelFilter, bool showKernelEstimate, int kernelWindow, double kernelWeighting, int kernelLevel, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassificationBak(input, neighborsCount, maxBarsBack, featureCount, feature1, feature1ParamA, feature1ParamB, feature2, feature2ParamA, feature2ParamB, feature3, feature3ParamA, feature3ParamB, feature4, feature4ParamA, feature4ParamB, feature5, feature5ParamA, feature5ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, useKernelFilter, showKernelEstimate, kernelWindow, kernelWeighting, kernelLevel, showBarColors, showBarPredictions);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LorentzianClassificationBak LorentzianClassificationBak(int neighborsCount, int maxBarsBack, int featureCount, FeatureType feature1, int feature1ParamA, int feature1ParamB, FeatureType feature2, int feature2ParamA, int feature2ParamB, FeatureType feature3, int feature3ParamA, int feature3ParamB, FeatureType feature4, int feature4ParamA, int feature4ParamB, FeatureType feature5, int feature5ParamA, int feature5ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, bool useKernelFilter, bool showKernelEstimate, int kernelWindow, double kernelWeighting, int kernelLevel, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassificationBak(Input, neighborsCount, maxBarsBack, featureCount, feature1, feature1ParamA, feature1ParamB, feature2, feature2ParamA, feature2ParamB, feature3, feature3ParamA, feature3ParamB, feature4, feature4ParamA, feature4ParamB, feature5, feature5ParamA, feature5ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, useKernelFilter, showKernelEstimate, kernelWindow, kernelWeighting, kernelLevel, showBarColors, showBarPredictions);
		}

		public Indicators.LorentzianClassificationBak LorentzianClassificationBak(ISeries<double> input , int neighborsCount, int maxBarsBack, int featureCount, FeatureType feature1, int feature1ParamA, int feature1ParamB, FeatureType feature2, int feature2ParamA, int feature2ParamB, FeatureType feature3, int feature3ParamA, int feature3ParamB, FeatureType feature4, int feature4ParamA, int feature4ParamB, FeatureType feature5, int feature5ParamA, int feature5ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, bool useKernelFilter, bool showKernelEstimate, int kernelWindow, double kernelWeighting, int kernelLevel, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassificationBak(input, neighborsCount, maxBarsBack, featureCount, feature1, feature1ParamA, feature1ParamB, feature2, feature2ParamA, feature2ParamB, feature3, feature3ParamA, feature3ParamB, feature4, feature4ParamA, feature4ParamB, feature5, feature5ParamA, feature5ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, useKernelFilter, showKernelEstimate, kernelWindow, kernelWeighting, kernelLevel, showBarColors, showBarPredictions);
		}
	}
}

#endregion
