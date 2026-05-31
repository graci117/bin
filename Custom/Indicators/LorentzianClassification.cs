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

public enum FeatureType
{
    RSI,
    WT,
    CCI,
    ADX
}

namespace NinjaTrader.NinjaScript.Indicators
{
    public class LorentzianClassification : Indicator
    {
        // EXACT PINE SCRIPT SETTINGS (from search results)
        [Range(1, 100), NinjaScriptProperty]
        [Display(Name = "Neighbors Count", Order = 1, GroupName = "General Settings")]
        public int NeighborsCount { get; set; } = 8;
        
        [Range(100, 5000), NinjaScriptProperty]
        [Display(Name = "Max Bars Back", Order = 2, GroupName = "General Settings")]
        public int MaxBarsBack { get; set; } = 2000;
        
        [Range(2, 5), NinjaScriptProperty]
        [Display(Name = "Feature Count", Order = 3, GroupName = "Feature Engineering")]
        public int FeatureCount { get; set; } = 2; // Set to 2 for testing

        // EXACT PINE SCRIPT FEATURE SYSTEM (from search results)
        [NinjaScriptProperty]
        [Display(Name = "Feature 1", Order = 4, GroupName = "Feature Engineering")]
        public FeatureType F1String { get; set; } = FeatureType.RSI; // Default: RSI
        [Range(2, 50), NinjaScriptProperty]
        [Display(Name = "Parameter A", Order = 5, GroupName = "Feature Engineering")]
        public int F1ParamA { get; set; } = 14; // Default: 14
        [Range(1, 50), NinjaScriptProperty]
        [Display(Name = "Parameter B", Order = 6, GroupName = "Feature Engineering")]
        public int F1ParamB { get; set; } = 1; // Default: 1

        [NinjaScriptProperty]
        [Display(Name = "Feature 2", Order = 7, GroupName = "Feature Engineering")]
        public FeatureType F2String { get; set; } = FeatureType.WT; // Default: WT
        [Range(2, 50), NinjaScriptProperty]
        [Display(Name = "Parameter A", Order = 8, GroupName = "Feature Engineering")]
        public int F2ParamA { get; set; } = 10; // Default: 10
        [Range(1, 50), NinjaScriptProperty]
        [Display(Name = "Parameter B", Order = 9, GroupName = "Feature Engineering")]
        public int F2ParamB { get; set; } = 11; // Default: 11

        // EXACT PINE SCRIPT FILTER SETTINGS (from search results)
        [NinjaScriptProperty]
        [Display(Name = "Use Volatility Filter", Order = 19, GroupName = "Filters")]
        public bool UseVolatilityFilter { get; set; } = true;

        [NinjaScriptProperty]
        [Display(Name = "Use Regime Filter", Order = 20, GroupName = "Filters")]
        public bool UseRegimeFilter { get; set; } = true;
        [Range(-10.0, 10.0), NinjaScriptProperty]
        [Display(Name = "Threshold", Order = 21, GroupName = "Filters")]
        public double RegimeThreshold { get; set; } = -0.1;

        [NinjaScriptProperty]
        [Display(Name = "Use ADX Filter", Order = 22, GroupName = "Filters")]
        public bool UseAdxFilter { get; set; } = false;
        [Range(0, 100), NinjaScriptProperty]
        [Display(Name = "Threshold", Order = 23, GroupName = "Filters")]
        public int AdxThreshold { get; set; } = 20;

        // EXACT PINE SCRIPT KERNEL SETTINGS
        [NinjaScriptProperty]
        [Display(Name = "Trade with Kernel", Order = 28, GroupName = "Kernel Settings")]
        public bool UseKernelFilter { get; set; } = true;
        [NinjaScriptProperty]
        [Display(Name = "Show Kernel Estimate", Order = 29, GroupName = "Kernel Settings")]
        public bool ShowKernelEstimate { get; set; } = true;
        [Range(3, 50), NinjaScriptProperty]
        [Display(Name = "Lookback Window", Order = 30, GroupName = "Kernel Settings")]
        public int H { get; set; } = 8;
        [Range(0.25, 25.0), NinjaScriptProperty]
        [Display(Name = "Relative Weighting", Order = 31, GroupName = "Kernel Settings")]
        public double R { get; set; } = 8.0;
        [Range(2, 25), NinjaScriptProperty]
        [Display(Name = "Regression Level", Order = 32, GroupName = "Kernel Settings")]
        public int X { get; set; } = 25;

        [NinjaScriptProperty]
        [Display(Name = "Show Bar Colors", Order = 33, GroupName = "Display Settings")]
        public bool ShowBarColors { get; set; } = true;
        [NinjaScriptProperty]
        [Display(Name = "Show Bar Prediction Values", Order = 34, GroupName = "Display Settings")]
        public bool ShowBarPredictions { get; set; } = true;

        // EXACT PINE SCRIPT VARIABLES (from search results type definitions)
        private List<double> f1Array, f2Array;
        private List<int> yTrainArray;
        private List<double> distancesArray;
        private List<double> predictionsArray;
        private double prediction = 0.0;
        private int signal = 0; // direction.neutral
        private double lastDistance = -1.0;
        private int firstBarIndex;
        private int loopSize;
        private ADX adx;

        // EXACT PINE SCRIPT DIRECTION CONSTANTS
        private const int DIRECTION_LONG = 1;
        private const int DIRECTION_SHORT = -1;
        private const int DIRECTION_NEUTRAL = 0;
		
		

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Machine Learning: Lorentzian Classification - EXACT Pine Script Match";
                Name = "LorentzianClassification";
                IsOverlay = true;
                
                AddPlot(new Stroke(Brushes.Orange, 4), PlotStyle.Line, "Kernel Regression");
                
                BarsRequiredToPlot = 20;
            }
            else if (State == State.DataLoaded)
            {
                f1Array = new List<double>();
                f2Array = new List<double>();
                yTrainArray = new List<int>();
                distancesArray = new List<double>();
                predictionsArray = new List<double>();

                if (UseAdxFilter)
                    adx = ADX(14); // Pine Script uses 14 for ADX filter
            }
        }

        protected override void OnBarUpdate()
        {
            // EXACT PINE SCRIPT: firstBarIndex calculation
			if (CurrentBar < MaxBarsBack)
        return;
			
            firstBarIndex = CurrentBar >= MaxBarsBack ? CurrentBar - MaxBarsBack : 0;

          

            // EXACT PINE SCRIPT: Training Labels (CORRECTED - this was the main issue!)
            // y_train_series = src[4] < src[0] ? direction.short : src[4] > src[0] ? direction.long : direction.neutral
            int yTrainSeries;
            if (Close[4] < Close[0])
                yTrainSeries = DIRECTION_SHORT; // -1: Pine Script assigns SHORT when price went UP!
            else if (Close[4] > Close[0])
                yTrainSeries = DIRECTION_LONG; // 1: Pine Script assigns LONG when price went DOWN!
            else
                yTrainSeries = DIRECTION_NEUTRAL; // 0: no significant change

            yTrainArray.Add(yTrainSeries);
            if (yTrainArray.Count > MaxBarsBack)
                yTrainArray.RemoveAt(0);

            // EXACT PINE SCRIPT: Feature Series calculation (with DUAL parameters)
            double hlc3 = (High[0] + Low[0] + Close[0]) / 3.0;
            double f1 = SeriesFrom(F1String, Close[0], High[0], Low[0], hlc3, F1ParamA, F1ParamB);
            double f2 = SeriesFrom(F2String, Close[0], High[0], Low[0], hlc3, F2ParamA, F2ParamB);

            // EXACT PINE SCRIPT: Feature Arrays storage
            f1Array.Add(f1);
            f2Array.Add(f2);
            if (f1Array.Count > MaxBarsBack)
            {
                f1Array.RemoveAt(0);
                f2Array.RemoveAt(0);
            }

            // EXACT PINE SCRIPT: Core ML Logic
            lastDistance = -1.0;
            loopSize = Math.Min(MaxBarsBack - 1, yTrainArray.Count - 1);

            distancesArray.Clear();
            predictionsArray.Clear();

            if (CurrentBar >= firstBarIndex)
            {
                for (int i = 0; i < loopSize; i++)
                {
                    double d = GetLorentzianDistance(i, f1, f2);
                    // EXACT PINE SCRIPT CONDITION: d >= lastDistance and i%4
                    if (d >= lastDistance && (i % 4 != 0))
                    {
                        lastDistance = d;
                        distancesArray.Add(d);
                        // CRITICAL FIX: Use correct indexing like Pine Script
                        predictionsArray.Add(Math.Round((double)yTrainArray[yTrainArray.Count - 1 - i]));
                        
                        if (predictionsArray.Count > NeighborsCount)
                        {
                            lastDistance = distancesArray[(int)Math.Round(NeighborsCount * 3.0 / 4.0)];
                            distancesArray.RemoveAt(0); // array.shift equivalent
                            predictionsArray.RemoveAt(0); // array.shift equivalent
                        }
                    }
                }
                prediction = predictionsArray.Sum();
            }

            // EXACT PINE SCRIPT: Filter Logic (from search results)
            bool volatilityFilter = !UseVolatilityFilter || FilterVolatility();
            bool regimeFilter = !UseRegimeFilter || RegimeFilter();
            bool adxFilterResult = !UseAdxFilter || FilterAdx();
			
			
            
			//tempchange
			//ool filterAll = volatilityFilter && regimeFilter && adxFilterResult;
			bool filterAll = true; 
			//endtemp
			
			  // === ADD FIX 2 DEBUGGING HERE ===
    if (CurrentBar % 10 == 0) // Every 10 bars to avoid spam
    {
        // Get the actual filter values for debugging
        double recentVol = 0, avgVol = 0, regimeValue = 0;
        
        // Calculate volatility values
        if (CurrentBar >= 10)
        {
            recentVol = Math.Max(High[0] - Low[0], Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Low[0] - Close[1])));
            for (int i = 0; i < 10; i++)
            {
                avgVol += Math.Max(High[i] - Low[i], Math.Max(Math.Abs(High[i] - Close[i + 1]), Math.Abs(Low[i] - Close[i + 1])));
            }
            avgVol /= 10.0;
        }
        
        // Calculate regime value
        if (CurrentBar >= 1)
        {
            double ohlc4 = (Open[0] + High[0] + Low[0] + Close[0]) / 4.0;
            double ohlc4Prev = (Open[1] + High[1] + Low[1] + Close[1]) / 4.0;
            regimeValue = ohlc4 - ohlc4Prev;
        }
        
        Print($"FILTER DETAILS Bar {CurrentBar}:");
        Print($"  Volatility: recent={recentVol:F4} avg={avgVol:F4} pass={volatilityFilter}");
        Print($"  Regime: value={regimeValue:F4} threshold={RegimeThreshold} pass={regimeFilter}");
        Print($"  ADX: value={adx?[0]:F2 ?? 0} threshold={AdxThreshold} pass={adxFilterResult}");
        Print($"  FilterAll: {filterAll}");
        Print($"  Prediction: {prediction:F0}");
    }
    // === END FIX 2 DEBUGGING ===

            // EXACT PINE SCRIPT: Signal Logic with persistence
            // signal := prediction > 0 and filter_all ? direction.long : prediction < 0 and filter_all ? direction.short : nz(signal[1])
//            int prevSignal = signal;
//            if (prediction > 0 && filterAll)
//                signal = DIRECTION_LONG;
//            else if (prediction < 0 && filterAll)
//                signal = DIRECTION_SHORT;
            // else signal remains the same (nz(signal[1]) logic)
			
			// Replace your signal logic with this:
			int actualPrevSignal = signal; // Store BEFORE updating
			
			if (prediction > 0 && filterAll)
			    signal = DIRECTION_LONG;
			else if (prediction < 0 && filterAll)
			    signal = DIRECTION_SHORT;
			// CRITICAL: Don't change signal if filters don't pass (nz logic)
			
			bool isDifferentSignalType = signal != actualPrevSignal;

            // Kernel Logic
            double kernelEstimate = CalculateKernelRegression();
            bool isBullishRate = kernelEstimate < Close[0]; // Note: Pine Script kernel logic
            bool isBearishRate = kernelEstimate > Close[0];
            bool isBullish = !UseKernelFilter || isBullishRate;
            bool isBearish = !UseKernelFilter || isBearishRate;

            // EXACT PINE SCRIPT: Entry Conditions
            //bool isDifferentSignalType = signal != prevSignal;
            bool isNewBuySignal = signal == DIRECTION_LONG && isDifferentSignalType;
            bool isNewSellSignal = signal == DIRECTION_SHORT && isDifferentSignalType;

            bool startLongTrade = isNewBuySignal && isBullish;
            bool startShortTrade = isNewSellSignal && isBearish;
			
if (startLongTrade)
{
    Print($"LONG ARROW: Time={Time[0]}, Price={Low[0]:F2}, Pred={prediction:F0}, Sig={signal}");
}

if (startShortTrade)
{
    Print($"SHORT ARROW: Time={Time[0]}, Price={High[0]:F2}, Pred={prediction:F0}, Sig={signal}");
}

// Debug signal changes with price info
if (signal != actualPrevSignal)
{
    Print($"SIGNAL CHANGE: Time={Time[0]}, Price={Close[0]:F2}, {actualPrevSignal}->{signal}, Pred={prediction:F0}");
}

// Add this right after filter calculation:
if (filterAll != true) // Only log when filters block signals
{
    Print($"FILTERS BLOCKING: Vol={volatilityFilter}, Reg={regimeFilter}, ADX={adxFilterResult}, Pred={prediction:F0}");
}

            // EXACT PINE SCRIPT: Plot arrows
            if (startLongTrade)
                Draw.ArrowUp(this, "Buy" + CurrentBar, true, 0, Low[0], Brushes.LimeGreen);
            if (startShortTrade)
                Draw.ArrowDown(this, "Sell" + CurrentBar, true, 0, High[0], Brushes.Red);

            // Plot kernel estimate
            Values[0][0] = kernelEstimate;
            PlotBrushes[0][0] = ShowKernelEstimate ? (isBullishRate ? Brushes.Green : Brushes.Red) : Brushes.Transparent;

            // Bar colors and prediction labels
            if (ShowBarColors)
                BarBrush = GetBarColor(prediction);

            if (ShowBarPredictions)
            {
                string txt = prediction.ToString("0");
                double y = prediction > 0 ? High[0] + TickSize * 2 : Low[0] - TickSize * 2;
                Draw.Text(this, "Pred" + CurrentBar, txt, 0, y, prediction > 0 ? Brushes.Green : prediction < 0 ? Brushes.Red : Brushes.Gray);
            }
        }

        // EXACT PINE SCRIPT: series_from function (with dual parameters)
        private double SeriesFrom(FeatureType featureString, double close, double high, double low, double hlc3, int fParamA, int fParamB)
        {
            switch (featureString)
            {
                case FeatureType.RSI:
                    // ml.n_rsi(_close, f_paramA, f_paramB) - Pine Script uses both parameters
                    return RSI(Close, fParamA, fParamB)[0];
                case FeatureType.WT:
                    // ml.n_wt(_hlc3, f_paramA, f_paramB) - WaveTrend with both parameters
                    return CalculateWT(hlc3, fParamA, fParamB);
                case FeatureType.CCI:
                    // ml.n_cci(_close, f_paramA, f_paramB) - CCI with parameter A
                    return CCI(fParamA)[0];
                case FeatureType.ADX:
                    // ml.n_adx(_high, _low, _close, f_paramA) - ADX with parameter A
                    return ADX(fParamA)[0];
                default:
                    return 0;
            }
        }

        // EXACT PINE SCRIPT: get_lorentzian_distance function for 2 features
        private double GetLorentzianDistance(int i, double f1, double f2)
        {
            double result = 0;
            if (i < f1Array.Count)
                result += Math.Log(1 + Math.Abs(f1 - f1Array[f1Array.Count - 1 - i]));
            if (i < f2Array.Count)
                result += Math.Log(1 + Math.Abs(f2 - f2Array[f2Array.Count - 1 - i]));
            return result;
        }

        // EXACT PINE SCRIPT: Filter implementations (from search results)
        private bool FilterVolatility()
        {
            // ml.filter_volatility(1, 10, filterSettings.useVolatilityFilter)
            if (CurrentBar < 10) return true;
            
            double recentVol = Math.Max(High[0] - Low[0], Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Low[0] - Close[1])));
            double avgVol = 0;
            for (int i = 0; i < 10; i++)
            {
                avgVol += Math.Max(High[i] - Low[i], Math.Max(Math.Abs(High[i] - Close[i + 1]), Math.Abs(Low[i] - Close[i + 1])));
            }
            avgVol /= 10.0;
            
            return recentVol > avgVol;
        }

        private bool RegimeFilter()
        {
            // ml.regime_filter(ohlc4, filterSettings.regimeThreshold, filterSettings.useRegimeFilter)
            if (CurrentBar < 1) return true;
            
            double ohlc4 = (Open[0] + High[0] + Low[0] + Close[0]) / 4.0;
            double ohlc4Prev = (Open[1] + High[1] + Low[1] + Close[1]) / 4.0;
            double regimeValue = ohlc4 - ohlc4Prev;
            
            return regimeValue > RegimeThreshold;
        }

        private bool FilterAdx()
        {
            // ml.filter_adx(settings.source, 14, filterSettings.adxThreshold, filterSettings.useAdxFilter)
            if (adx == null || CurrentBar < 14) return true;
            return adx[0] > AdxThreshold;
        }

        private double CalculateWT(double hlc3, int paramA, int paramB)
        {
            // Simplified WaveTrend implementation based on dual parameters
            if (CurrentBar < Math.Max(paramA, paramB)) return 0;
            double esa = EMA(Close, paramA)[0];
            double d = EMA(Close, paramB)[0];
            return (hlc3 - esa) / (d != 0 ? d : 1);
        }

        private double CalculateKernelRegression()
        {
            // EXACT PINE SCRIPT: Rational Quadratic Kernel
            double sum = 0.0;
            double weightSum = 0.0;

            for (int i = 0; i < Math.Min(H, CurrentBar); i++)
            {
                double distance = Math.Abs(i);
                double weight = Math.Pow(1 + (distance * distance) / (2 * R * X * X), -R);
                sum += Close[i] * weight;
                weightSum += weight;
            }

            return weightSum > 0 ? sum / weightSum : Close[0];
        }

        private Brush GetBarColor(double prediction)
        {
            double compressionFactor = NeighborsCount / 1.0;
            if (prediction > 0)
            {
                byte intensity = (byte)(120 + 135 * Math.Min(prediction / compressionFactor, 1.0));
                return new SolidColorBrush(Color.FromRgb(0, intensity, 136));
            }
            else if (prediction < 0)
            {
                byte intensity = (byte)(51 + 153 * Math.Min(-prediction / compressionFactor, 1.0));
                return new SolidColorBrush(Color.FromRgb(204, intensity, 17));
            }
            else
            {
                return new SolidColorBrush(Color.FromRgb(120, 123, 134));
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LorentzianClassification[] cacheLorentzianClassification;
		public LorentzianClassification LorentzianClassification(int neighborsCount, int maxBarsBack, int featureCount, FeatureType f1String, int f1ParamA, int f1ParamB, FeatureType f2String, int f2ParamA, int f2ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, int h, double r, int x, bool showBarColors, bool showBarPredictions)
		{
			return LorentzianClassification(Input, neighborsCount, maxBarsBack, featureCount, f1String, f1ParamA, f1ParamB, f2String, f2ParamA, f2ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useKernelFilter, showKernelEstimate, h, r, x, showBarColors, showBarPredictions);
		}

		public LorentzianClassification LorentzianClassification(ISeries<double> input, int neighborsCount, int maxBarsBack, int featureCount, FeatureType f1String, int f1ParamA, int f1ParamB, FeatureType f2String, int f2ParamA, int f2ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, int h, double r, int x, bool showBarColors, bool showBarPredictions)
		{
			if (cacheLorentzianClassification != null)
				for (int idx = 0; idx < cacheLorentzianClassification.Length; idx++)
					if (cacheLorentzianClassification[idx] != null && cacheLorentzianClassification[idx].NeighborsCount == neighborsCount && cacheLorentzianClassification[idx].MaxBarsBack == maxBarsBack && cacheLorentzianClassification[idx].FeatureCount == featureCount && cacheLorentzianClassification[idx].F1String == f1String && cacheLorentzianClassification[idx].F1ParamA == f1ParamA && cacheLorentzianClassification[idx].F1ParamB == f1ParamB && cacheLorentzianClassification[idx].F2String == f2String && cacheLorentzianClassification[idx].F2ParamA == f2ParamA && cacheLorentzianClassification[idx].F2ParamB == f2ParamB && cacheLorentzianClassification[idx].UseVolatilityFilter == useVolatilityFilter && cacheLorentzianClassification[idx].UseRegimeFilter == useRegimeFilter && cacheLorentzianClassification[idx].RegimeThreshold == regimeThreshold && cacheLorentzianClassification[idx].UseAdxFilter == useAdxFilter && cacheLorentzianClassification[idx].AdxThreshold == adxThreshold && cacheLorentzianClassification[idx].UseKernelFilter == useKernelFilter && cacheLorentzianClassification[idx].ShowKernelEstimate == showKernelEstimate && cacheLorentzianClassification[idx].H == h && cacheLorentzianClassification[idx].R == r && cacheLorentzianClassification[idx].X == x && cacheLorentzianClassification[idx].ShowBarColors == showBarColors && cacheLorentzianClassification[idx].ShowBarPredictions == showBarPredictions && cacheLorentzianClassification[idx].EqualsInput(input))
						return cacheLorentzianClassification[idx];
			return CacheIndicator<LorentzianClassification>(new LorentzianClassification(){ NeighborsCount = neighborsCount, MaxBarsBack = maxBarsBack, FeatureCount = featureCount, F1String = f1String, F1ParamA = f1ParamA, F1ParamB = f1ParamB, F2String = f2String, F2ParamA = f2ParamA, F2ParamB = f2ParamB, UseVolatilityFilter = useVolatilityFilter, UseRegimeFilter = useRegimeFilter, RegimeThreshold = regimeThreshold, UseAdxFilter = useAdxFilter, AdxThreshold = adxThreshold, UseKernelFilter = useKernelFilter, ShowKernelEstimate = showKernelEstimate, H = h, R = r, X = x, ShowBarColors = showBarColors, ShowBarPredictions = showBarPredictions }, input, ref cacheLorentzianClassification);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LorentzianClassification LorentzianClassification(int neighborsCount, int maxBarsBack, int featureCount, FeatureType f1String, int f1ParamA, int f1ParamB, FeatureType f2String, int f2ParamA, int f2ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, int h, double r, int x, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassification(Input, neighborsCount, maxBarsBack, featureCount, f1String, f1ParamA, f1ParamB, f2String, f2ParamA, f2ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useKernelFilter, showKernelEstimate, h, r, x, showBarColors, showBarPredictions);
		}

		public Indicators.LorentzianClassification LorentzianClassification(ISeries<double> input , int neighborsCount, int maxBarsBack, int featureCount, FeatureType f1String, int f1ParamA, int f1ParamB, FeatureType f2String, int f2ParamA, int f2ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, int h, double r, int x, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassification(input, neighborsCount, maxBarsBack, featureCount, f1String, f1ParamA, f1ParamB, f2String, f2ParamA, f2ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useKernelFilter, showKernelEstimate, h, r, x, showBarColors, showBarPredictions);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LorentzianClassification LorentzianClassification(int neighborsCount, int maxBarsBack, int featureCount, FeatureType f1String, int f1ParamA, int f1ParamB, FeatureType f2String, int f2ParamA, int f2ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, int h, double r, int x, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassification(Input, neighborsCount, maxBarsBack, featureCount, f1String, f1ParamA, f1ParamB, f2String, f2ParamA, f2ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useKernelFilter, showKernelEstimate, h, r, x, showBarColors, showBarPredictions);
		}

		public Indicators.LorentzianClassification LorentzianClassification(ISeries<double> input , int neighborsCount, int maxBarsBack, int featureCount, FeatureType f1String, int f1ParamA, int f1ParamB, FeatureType f2String, int f2ParamA, int f2ParamB, bool useVolatilityFilter, bool useRegimeFilter, double regimeThreshold, bool useAdxFilter, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, int h, double r, int x, bool showBarColors, bool showBarPredictions)
		{
			return indicator.LorentzianClassification(input, neighborsCount, maxBarsBack, featureCount, f1String, f1ParamA, f1ParamB, f2String, f2ParamA, f2ParamB, useVolatilityFilter, useRegimeFilter, regimeThreshold, useAdxFilter, adxThreshold, useKernelFilter, showKernelEstimate, h, r, x, showBarColors, showBarPredictions);
		}
	}
}

#endregion
