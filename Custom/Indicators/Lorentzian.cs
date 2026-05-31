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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class Lorentzian : Indicator
    {
        //===== Parameters (match PineScript inputs) =====
        [NinjaScriptProperty] [Range(1,100)]        public int NeighborsCount     { get; set; } = 8;
        [NinjaScriptProperty] [Range(100,5000)]      public int MaxBarsBack        { get; set; } = 2000;
        [NinjaScriptProperty] [Range(2,5)]           public int FeatureCount       { get; set; } = 5;
        [NinjaScriptProperty] [Range(1,10)]          public int ColorCompression   { get; set; } = 1;
        [NinjaScriptProperty]                      public bool ShowBarColors     { get; set; } = true;
        [NinjaScriptProperty]                      public bool ShowBarPredictions{ get; set; } = true;
        [NinjaScriptProperty]                      public bool UseAtrOffset      { get; set; } = false;
        [NinjaScriptProperty] [Range(0,100)]        public double BarPredictionsOffset { get; set; } = 0;
        [NinjaScriptProperty]                      public bool UseEmaFilter      { get; set; } = false;
        [NinjaScriptProperty] [Range(1,500)]        public int EmaPeriod          { get; set; } = 200;
        [NinjaScriptProperty]                      public bool UseSmaFilter      { get; set; } = false;
        [NinjaScriptProperty] [Range(1,500)]        public int SmaPeriod          { get; set; } = 200;

        // Feature definitions
        [NinjaScriptProperty] public string F1_String { get; set; } = "RSI";
        [NinjaScriptProperty] [Range(1,50)] public int F1_ParamA { get; set; } = 14;
        [NinjaScriptProperty] [Range(1,50)] public int F1_ParamB { get; set; } = 1;
        [NinjaScriptProperty] public string F2_String { get; set; } = "WT";
        [NinjaScriptProperty] [Range(1,50)] public int F2_ParamA { get; set; } = 10;
        [NinjaScriptProperty] [Range(1,50)] public int F2_ParamB { get; set; } = 11;
        [NinjaScriptProperty] public string F3_String { get; set; } = "CCI";
        [NinjaScriptProperty] [Range(1,50)] public int F3_ParamA { get; set; } = 20;
        [NinjaScriptProperty] [Range(1,50)] public int F3_ParamB { get; set; } = 1;
        [NinjaScriptProperty] public string F4_String { get; set; } = "ADX";
        [NinjaScriptProperty] [Range(1,50)] public int F4_ParamA { get; set; } = 20;
        [NinjaScriptProperty] [Range(1,50)] public int F4_ParamB { get; set; } = 2;
        [NinjaScriptProperty] public string F5_String { get; set; } = "RSI";
        [NinjaScriptProperty] [Range(1,50)] public int F5_ParamA { get; set; } = 9;
        [NinjaScriptProperty] [Range(1,50)] public int F5_ParamB { get; set; } = 1;

        // Filters
        [NinjaScriptProperty] public bool UseVolatilityFilter{ get; set; } = true;
        [NinjaScriptProperty] public bool UseRegimeFilter    { get; set; } = true;
        [NinjaScriptProperty] public bool UseAdxFilter       { get; set; } = false;
        [NinjaScriptProperty] [Range(-10,10)] public double RegimeThreshold { get; set; } = -0.1;
        [NinjaScriptProperty] [Range(0,100)] public int AdxThreshold { get; set; } = 20;

        // Kernel regression
        [NinjaScriptProperty] public bool UseKernelFilter   { get; set; } = true;
        [NinjaScriptProperty] public bool ShowKernelEstimate{ get; set; } = true;
        [NinjaScriptProperty] public bool UseKernelSmoothing{ get; set; } = false;
        [NinjaScriptProperty] [Range(3,50)] public int H    { get; set; } = 8;
        [NinjaScriptProperty] [Range(0.25,25)] public double R { get; set; } = 8;
        [NinjaScriptProperty] [Range(2,25)] public int X    { get; set; } = 25;
        [NinjaScriptProperty] [Range(1,5)] public int Lag    { get; set; } = 2;

        // Series & state
        private List<double[]> featureArrays;
        private List<int>    yTrainArray;
        private List<double> distances, predictions;
        private double       lastDistance, kernelEstimate;
        private int          signal;
        private EMA          ema;
        private SMA          sma;
		private double prediction = 0;
		private const int LONG    =  1;
		private const int SHORT   = -1;
		private const int NEUTRAL =  0;
		private ATR atr;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description  = "Lorentzian Classification ML Indicator";
                Name         = "Lorentzian";
                Calculate    = Calculate.OnBarClose;
                IsOverlay    = true;
                AddPlot(new Stroke(Brushes.Orange, 4), PlotStyle.Line, "Kernel Regression");
            }
            else if (State == State.DataLoaded)
            {
                ema             = EMA(Close, EmaPeriod);
                sma             = SMA(Close, SmaPeriod);
                featureArrays   = new List<double[]>();
                yTrainArray     = new List<int>();
                distances       = new List<double>();
                predictions     = new List<double>();
                lastDistance    = -1;
                signal          = 0;
				atr = ATR(Close, 1);
            }
        }

       protected override void OnBarUpdate()
		{
			
		    // 1) Wait until we have enough bars for all features, lag, and 4-bar ahead label
		    int minBars = Math.Max(Math.Max(F1_ParamA, F2_ParamA),
		                   Math.Max(Math.Max(F3_ParamA, F4_ParamA),
		                            Math.Max(F5_ParamA, Math.Max(H, Lag)))) + 4;
		    if (CurrentBar < minBars)
		        return;
		
		    // 2) Maintain rolling maxBarsBackIndex
		    int maxBarsBackIndex = CurrentBar >= MaxBarsBack 
		                           ? CurrentBar - MaxBarsBack 
		                           : 0;
		
		    // 3) Extract features for this bar and store
		    double[] features = ExtractFeatures();
		    featureArrays.Add(features);
		
		    // 4) Compute label using close[4] as "future" vs. close[0] as "current"
		    //    PineScript: y_train_series = src[4] < src[0] ? short : src[4] > src[0] ? long : neutral[1]
		   int yTrainSeries = NEUTRAL;
			if (CurrentBar >= 4)
			{
			    double past    = Close[4];   // price 4 bars ago = PineScript src[4]
			    double current = Close[0];   // price now       = PineScript src[0]
			    yTrainSeries   = current > past  ? LONG
			                   : current < past  ? SHORT
			                                     : NEUTRAL;
			}
			yTrainArray.Add(yTrainSeries);
		
		    // 5) Perform Lorentzian ANN classification when we have sufficient history
		    //    (skipping every 4th bar for neighbor sampling)
		    PerformLorentzianClassification();
		
		    // 6) Kernel regression (Rational-Quadratic and Gaussian)
		    double yhat1 = CalculateRationalQuadraticKernel(Close, H, R, X);
		    double yhat2 = CalculateGaussianKernel(Close, H - Lag, X);
		    Values[0][0]   = ShowKernelEstimate ? yhat1 : double.NaN;
		    PlotBrushes[0][0] = UseKernelSmoothing 
		                       ? (yhat2 >= yhat1 ? Brushes.Green : Brushes.Red) 
		                       : (yhat1 > Values[0][1] ? Brushes.Green : Brushes.Red);
		
		    // 7) Apply filters
		    bool volF = UseVolatilityFilter ? CalculateVolatilityFilter() : true;
		    bool regF = UseRegimeFilter    ? CalculateRegimeFilter()    : true;
		    bool adxF = UseAdxFilter       ? CalculateAdxFilter()       : true;
		    bool allF = volF && regF && adxF;
		
		    // 8) Update signal based on predictions sum and filters
		    int prevSignal = signal;
		    signal = prediction > 0 && allF ? LONG 
		           : prediction < 0 && allF ? SHORT 
		                                    : signal;
		
		    // 9) Trend filters
		    bool emaUp   = UseEmaFilter ? Close[0] > ema[0] : true;
		    bool emaDown = UseEmaFilter ? Close[0] < ema[0] : true;
		    bool smaUp   = UseSmaFilter ? Close[0] > sma[0] : true;
		    bool smaDown = UseSmaFilter ? Close[0] < sma[0] : true;
		
		    // 10) Entry arrows (only once per bar)
		    bool newLong  = signal == LONG  && prevSignal != LONG  && emaUp   && smaUp;
		    bool newShort = signal == SHORT && prevSignal != SHORT && emaDown && smaDown;
		    if (newLong)
		        Draw.ArrowUp(this, "Buy"  + CurrentBar, false, 0, Low[0]  - TickSize*2, Brushes.Lime);
		    if (newShort)
		        Draw.ArrowDown(this, "Sell" + CurrentBar, false, 0, High[0] + TickSize*2, Brushes.Red);
		
		    // 11) Single draw of prediction text per bar
		   if (ShowBarPredictions)
		    {
				string predictionTag = "Prediction_" + CurrentBar;
		        double yVal = UseAtrOffset 
		            ? (prediction > 0 
		               ? High[0] + atr[0] 
		               : Low[0]  - atr[0])
		            : (prediction > 0 
		               ? High[0] + ((High[0]+Low[0])/2)*BarPredictionsOffset/20 
		               : Low[0]  - ((High[0]+Low[0])/2)*BarPredictionsOffset/30);
		
				 Brush predictionColor;
			    
			    if (prediction > 0)
			    {
			        // PineScript: color.from_gradient(prediction, 0, compressionFactor, #787b86, #009988)
			        predictionColor = Brushes.Lime;
			    }
			    else if (prediction <= 0)
			    {
			        // PineScript: color.from_gradient(prediction, -compressionFactor, 0, #CC3311, #787b86)
			        predictionColor = Brushes.Red;
			    }
			    else
			    {
			        predictionColor = Brushes.Gray;
			    }
		        Draw.Text(this, predictionTag, false, Math.Round(prediction, 0).ToString(), 0, yVal, 0, predictionColor,
			              new SimpleFont("Arial", 16), TextAlignment.Center, // Increased from 12 to 16
			              Brushes.Transparent, Brushes.Transparent, 0);
		    }
		
		    // 12) Bar coloring (one brush assignment per bar)
		    if (ShowBarColors)
		    {
		        double comp = (double)NeighborsCount / ColorCompression;
		        byte r = (byte)(prediction > 0
		            ? 120 + (0 - 120)*(prediction/comp)
		            : 120 + (204 - 120)*(Math.Abs(prediction)/comp));
		        byte g = (byte)(prediction > 0
		            ? 123 + (153 - 123)*(prediction/comp)
		            : 123 + ( 51 - 123)*(Math.Abs(prediction)/comp));
		        byte b = (byte)(prediction > 0
		            ? 134 + (136 - 134)*(prediction/comp)
		            : 134 + ( 17 - 134)*(Math.Abs(prediction)/comp));
		
		        BarBrush = new SolidColorBrush(Color.FromArgb(255, r, g, b));
		    }
		}


        //===== Core ML functions =====

        private double[] ExtractFeatures()
        {
            double hlc3 = (High[0]+ Low[0]+ Close[0])/3;
            var f = new double[FeatureCount];
            if (FeatureCount>=1) f[0] = SeriesFrom(F1_String, F1_ParamA, F1_ParamB, hlc3);
            if (FeatureCount>=2) f[1] = SeriesFrom(F2_String, F2_ParamA, F2_ParamB, hlc3);
            if (FeatureCount>=3) f[2] = SeriesFrom(F3_String, F3_ParamA, F3_ParamB, hlc3);
            if (FeatureCount>=4) f[3] = SeriesFrom(F4_String, F4_ParamA, F4_ParamB, hlc3);
            if (FeatureCount>=5) f[4] = SeriesFrom(F5_String, F5_ParamA, F5_ParamB, hlc3);
            featureArrays.Add(f);
            return f;
        }

        private double SeriesFrom(string type, int a, int b, double hlc3)
        {
            switch(type)
            {
                case "RSI": return RSI(Close, a, b)[0];
                case "WT":  return (WilliamsR(Close, a)[0] + 100)/2;
                case "CCI": return CCI(a)[0]/100;
                case "ADX": return ADX(Close, a)[0];
                default:    return hlc3;
            }
        }

        private void PerformLorentzianClassification()
        {
            distances.Clear();
            predictions.Clear();
            lastDistance = -1;

            var cur = featureArrays.Last();
            int sizeLoop = Math.Min(MaxBarsBack-1, yTrainArray.Count-1);

            for(int i=0; i<= sizeLoop; i++)
            {
                if (i % 4 == 0) continue;   // skip every 4th bar
                double d = GetLorentzianDistance(i, cur);
                if (d >= lastDistance)
                {
                    lastDistance = d;
                    distances.Add(d);
                    predictions.Add(yTrainArray[i]);
                    if (predictions.Count > NeighborsCount)
                    {
                        int idx = (int)Math.Round(NeighborsCount*3.0/4);
                        lastDistance = distances[Math.Min(idx, distances.Count-1)];
                        distances.RemoveAt(0);
                        predictions.RemoveAt(0);
                    }
                }
            }
            prediction = predictions.Sum();
        }

        private double GetLorentzianDistance(int i, double[] cur)
        {
            double sum=0;
            for(int f=0; f<FeatureCount; f++)
                sum += Math.Log(1 + Math.Abs(cur[f] - featureArrays[i][f]));
            return sum;
        }

        private double CalculateRationalQuadraticKernel(ISeries<double> src, int lookback, double rw, int rl)
        {
            if (lookback<=0) return src[0];
            double s=0, w=0;
            for(int i=0; i<lookback; i++)
            {
                double di = i*i/(2*rw*rw);
                double wi = Math.Pow(1+di, -rw);
                s += src[i]*wi; w+= wi;
            }
            return w>0 ? s/w : src[0];
        }

        private double CalculateGaussianKernel(ISeries<double> src, int lookback, int rl)
        {
            if (lookback<=0) return src[0];
            double s=0, w=0, bw = lookback/4.0;
            for(int i=0; i<lookback; i++)
            {
                double wi = Math.Exp(-0.5*Math.Pow(i/bw,2));
                s+= src[i]*wi; w+= wi;
            }
            return w>0 ? s/w : src[0];
        }

        private bool CalculateVolatilityFilter()
        {
            int len=10;
            if (CurrentBar<len) return true;
            var closes = Enumerable.Range(0,len).Select(i=>Close[i]).ToArray();
            double m = closes.Average();
            double sd = Math.Sqrt(closes.Average(d=>Math.Pow(d-m,2)));
            return sd < m*0.02;
        }

        private bool CalculateRegimeFilter()
        {
            if (CurrentBar<50) return true;
            double c0 = (Open[0]+High[0]+Low[0]+Close[0])/4;
            double c1 = (Open[50]+High[50]+Low[50]+Close[50])/4;
            return (c0-c1)/c1 > RegimeThreshold;
        }

        private bool CalculateAdxFilter()
        {
            if (CurrentBar<14) return true;
            return ADX(Close,14)[0] > AdxThreshold;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Lorentzian[] cacheLorentzian;
		public Lorentzian Lorentzian(int neighborsCount, int maxBarsBack, int featureCount, int colorCompression, bool showBarColors, bool showBarPredictions, bool useAtrOffset, double barPredictionsOffset, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, string f1_String, int f1_ParamA, int f1_ParamB, string f2_String, int f2_ParamA, int f2_ParamB, string f3_String, int f3_ParamA, int f3_ParamB, string f4_String, int f4_ParamA, int f4_ParamB, string f5_String, int f5_ParamA, int f5_ParamB, bool useVolatilityFilter, bool useRegimeFilter, bool useAdxFilter, double regimeThreshold, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, bool useKernelSmoothing, int h, double r, int x, int lag)
		{
			return Lorentzian(Input, neighborsCount, maxBarsBack, featureCount, colorCompression, showBarColors, showBarPredictions, useAtrOffset, barPredictionsOffset, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, f1_String, f1_ParamA, f1_ParamB, f2_String, f2_ParamA, f2_ParamB, f3_String, f3_ParamA, f3_ParamB, f4_String, f4_ParamA, f4_ParamB, f5_String, f5_ParamA, f5_ParamB, useVolatilityFilter, useRegimeFilter, useAdxFilter, regimeThreshold, adxThreshold, useKernelFilter, showKernelEstimate, useKernelSmoothing, h, r, x, lag);
		}

		public Lorentzian Lorentzian(ISeries<double> input, int neighborsCount, int maxBarsBack, int featureCount, int colorCompression, bool showBarColors, bool showBarPredictions, bool useAtrOffset, double barPredictionsOffset, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, string f1_String, int f1_ParamA, int f1_ParamB, string f2_String, int f2_ParamA, int f2_ParamB, string f3_String, int f3_ParamA, int f3_ParamB, string f4_String, int f4_ParamA, int f4_ParamB, string f5_String, int f5_ParamA, int f5_ParamB, bool useVolatilityFilter, bool useRegimeFilter, bool useAdxFilter, double regimeThreshold, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, bool useKernelSmoothing, int h, double r, int x, int lag)
		{
			if (cacheLorentzian != null)
				for (int idx = 0; idx < cacheLorentzian.Length; idx++)
					if (cacheLorentzian[idx] != null && cacheLorentzian[idx].NeighborsCount == neighborsCount && cacheLorentzian[idx].MaxBarsBack == maxBarsBack && cacheLorentzian[idx].FeatureCount == featureCount && cacheLorentzian[idx].ColorCompression == colorCompression && cacheLorentzian[idx].ShowBarColors == showBarColors && cacheLorentzian[idx].ShowBarPredictions == showBarPredictions && cacheLorentzian[idx].UseAtrOffset == useAtrOffset && cacheLorentzian[idx].BarPredictionsOffset == barPredictionsOffset && cacheLorentzian[idx].UseEmaFilter == useEmaFilter && cacheLorentzian[idx].EmaPeriod == emaPeriod && cacheLorentzian[idx].UseSmaFilter == useSmaFilter && cacheLorentzian[idx].SmaPeriod == smaPeriod && cacheLorentzian[idx].F1_String == f1_String && cacheLorentzian[idx].F1_ParamA == f1_ParamA && cacheLorentzian[idx].F1_ParamB == f1_ParamB && cacheLorentzian[idx].F2_String == f2_String && cacheLorentzian[idx].F2_ParamA == f2_ParamA && cacheLorentzian[idx].F2_ParamB == f2_ParamB && cacheLorentzian[idx].F3_String == f3_String && cacheLorentzian[idx].F3_ParamA == f3_ParamA && cacheLorentzian[idx].F3_ParamB == f3_ParamB && cacheLorentzian[idx].F4_String == f4_String && cacheLorentzian[idx].F4_ParamA == f4_ParamA && cacheLorentzian[idx].F4_ParamB == f4_ParamB && cacheLorentzian[idx].F5_String == f5_String && cacheLorentzian[idx].F5_ParamA == f5_ParamA && cacheLorentzian[idx].F5_ParamB == f5_ParamB && cacheLorentzian[idx].UseVolatilityFilter == useVolatilityFilter && cacheLorentzian[idx].UseRegimeFilter == useRegimeFilter && cacheLorentzian[idx].UseAdxFilter == useAdxFilter && cacheLorentzian[idx].RegimeThreshold == regimeThreshold && cacheLorentzian[idx].AdxThreshold == adxThreshold && cacheLorentzian[idx].UseKernelFilter == useKernelFilter && cacheLorentzian[idx].ShowKernelEstimate == showKernelEstimate && cacheLorentzian[idx].UseKernelSmoothing == useKernelSmoothing && cacheLorentzian[idx].H == h && cacheLorentzian[idx].R == r && cacheLorentzian[idx].X == x && cacheLorentzian[idx].Lag == lag && cacheLorentzian[idx].EqualsInput(input))
						return cacheLorentzian[idx];
			return CacheIndicator<Lorentzian>(new Lorentzian(){ NeighborsCount = neighborsCount, MaxBarsBack = maxBarsBack, FeatureCount = featureCount, ColorCompression = colorCompression, ShowBarColors = showBarColors, ShowBarPredictions = showBarPredictions, UseAtrOffset = useAtrOffset, BarPredictionsOffset = barPredictionsOffset, UseEmaFilter = useEmaFilter, EmaPeriod = emaPeriod, UseSmaFilter = useSmaFilter, SmaPeriod = smaPeriod, F1_String = f1_String, F1_ParamA = f1_ParamA, F1_ParamB = f1_ParamB, F2_String = f2_String, F2_ParamA = f2_ParamA, F2_ParamB = f2_ParamB, F3_String = f3_String, F3_ParamA = f3_ParamA, F3_ParamB = f3_ParamB, F4_String = f4_String, F4_ParamA = f4_ParamA, F4_ParamB = f4_ParamB, F5_String = f5_String, F5_ParamA = f5_ParamA, F5_ParamB = f5_ParamB, UseVolatilityFilter = useVolatilityFilter, UseRegimeFilter = useRegimeFilter, UseAdxFilter = useAdxFilter, RegimeThreshold = regimeThreshold, AdxThreshold = adxThreshold, UseKernelFilter = useKernelFilter, ShowKernelEstimate = showKernelEstimate, UseKernelSmoothing = useKernelSmoothing, H = h, R = r, X = x, Lag = lag }, input, ref cacheLorentzian);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Lorentzian Lorentzian(int neighborsCount, int maxBarsBack, int featureCount, int colorCompression, bool showBarColors, bool showBarPredictions, bool useAtrOffset, double barPredictionsOffset, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, string f1_String, int f1_ParamA, int f1_ParamB, string f2_String, int f2_ParamA, int f2_ParamB, string f3_String, int f3_ParamA, int f3_ParamB, string f4_String, int f4_ParamA, int f4_ParamB, string f5_String, int f5_ParamA, int f5_ParamB, bool useVolatilityFilter, bool useRegimeFilter, bool useAdxFilter, double regimeThreshold, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, bool useKernelSmoothing, int h, double r, int x, int lag)
		{
			return indicator.Lorentzian(Input, neighborsCount, maxBarsBack, featureCount, colorCompression, showBarColors, showBarPredictions, useAtrOffset, barPredictionsOffset, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, f1_String, f1_ParamA, f1_ParamB, f2_String, f2_ParamA, f2_ParamB, f3_String, f3_ParamA, f3_ParamB, f4_String, f4_ParamA, f4_ParamB, f5_String, f5_ParamA, f5_ParamB, useVolatilityFilter, useRegimeFilter, useAdxFilter, regimeThreshold, adxThreshold, useKernelFilter, showKernelEstimate, useKernelSmoothing, h, r, x, lag);
		}

		public Indicators.Lorentzian Lorentzian(ISeries<double> input , int neighborsCount, int maxBarsBack, int featureCount, int colorCompression, bool showBarColors, bool showBarPredictions, bool useAtrOffset, double barPredictionsOffset, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, string f1_String, int f1_ParamA, int f1_ParamB, string f2_String, int f2_ParamA, int f2_ParamB, string f3_String, int f3_ParamA, int f3_ParamB, string f4_String, int f4_ParamA, int f4_ParamB, string f5_String, int f5_ParamA, int f5_ParamB, bool useVolatilityFilter, bool useRegimeFilter, bool useAdxFilter, double regimeThreshold, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, bool useKernelSmoothing, int h, double r, int x, int lag)
		{
			return indicator.Lorentzian(input, neighborsCount, maxBarsBack, featureCount, colorCompression, showBarColors, showBarPredictions, useAtrOffset, barPredictionsOffset, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, f1_String, f1_ParamA, f1_ParamB, f2_String, f2_ParamA, f2_ParamB, f3_String, f3_ParamA, f3_ParamB, f4_String, f4_ParamA, f4_ParamB, f5_String, f5_ParamA, f5_ParamB, useVolatilityFilter, useRegimeFilter, useAdxFilter, regimeThreshold, adxThreshold, useKernelFilter, showKernelEstimate, useKernelSmoothing, h, r, x, lag);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Lorentzian Lorentzian(int neighborsCount, int maxBarsBack, int featureCount, int colorCompression, bool showBarColors, bool showBarPredictions, bool useAtrOffset, double barPredictionsOffset, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, string f1_String, int f1_ParamA, int f1_ParamB, string f2_String, int f2_ParamA, int f2_ParamB, string f3_String, int f3_ParamA, int f3_ParamB, string f4_String, int f4_ParamA, int f4_ParamB, string f5_String, int f5_ParamA, int f5_ParamB, bool useVolatilityFilter, bool useRegimeFilter, bool useAdxFilter, double regimeThreshold, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, bool useKernelSmoothing, int h, double r, int x, int lag)
		{
			return indicator.Lorentzian(Input, neighborsCount, maxBarsBack, featureCount, colorCompression, showBarColors, showBarPredictions, useAtrOffset, barPredictionsOffset, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, f1_String, f1_ParamA, f1_ParamB, f2_String, f2_ParamA, f2_ParamB, f3_String, f3_ParamA, f3_ParamB, f4_String, f4_ParamA, f4_ParamB, f5_String, f5_ParamA, f5_ParamB, useVolatilityFilter, useRegimeFilter, useAdxFilter, regimeThreshold, adxThreshold, useKernelFilter, showKernelEstimate, useKernelSmoothing, h, r, x, lag);
		}

		public Indicators.Lorentzian Lorentzian(ISeries<double> input , int neighborsCount, int maxBarsBack, int featureCount, int colorCompression, bool showBarColors, bool showBarPredictions, bool useAtrOffset, double barPredictionsOffset, bool useEmaFilter, int emaPeriod, bool useSmaFilter, int smaPeriod, string f1_String, int f1_ParamA, int f1_ParamB, string f2_String, int f2_ParamA, int f2_ParamB, string f3_String, int f3_ParamA, int f3_ParamB, string f4_String, int f4_ParamA, int f4_ParamB, string f5_String, int f5_ParamA, int f5_ParamB, bool useVolatilityFilter, bool useRegimeFilter, bool useAdxFilter, double regimeThreshold, int adxThreshold, bool useKernelFilter, bool showKernelEstimate, bool useKernelSmoothing, int h, double r, int x, int lag)
		{
			return indicator.Lorentzian(input, neighborsCount, maxBarsBack, featureCount, colorCompression, showBarColors, showBarPredictions, useAtrOffset, barPredictionsOffset, useEmaFilter, emaPeriod, useSmaFilter, smaPeriod, f1_String, f1_ParamA, f1_ParamB, f2_String, f2_ParamA, f2_ParamB, f3_String, f3_ParamA, f3_ParamB, f4_String, f4_ParamA, f4_ParamB, f5_String, f5_ParamA, f5_ParamB, useVolatilityFilter, useRegimeFilter, useAdxFilter, regimeThreshold, adxThreshold, useKernelFilter, showKernelEstimate, useKernelSmoothing, h, r, x, lag);
		}
	}
}

#endregion
