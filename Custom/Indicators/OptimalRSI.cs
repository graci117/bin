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

// Define enums outside the class
public enum OptimalRSIType
{
    AllCrossings,
    ExtremityCrossings
}

public enum AdjustType
{
    Auto,
    Manual
}

public enum MLType
{
    KNNAverage,
    KNNExponentialAverage,
    SimpleAverage,
    None
}

public enum KNNDistanceType
{
    Both,
    Max,
    Min
}

namespace NinjaTrader.NinjaScript.Indicators
{
    public class OptimalRSI : Indicator
    {
        private Series<double> optimalRSI;
        private Series<double> rsiMA;
        private Series<double> upperInner;
        private Series<double> lowerInner;
        private Series<double> upperOuter;
        private Series<double> lowerOuter;
        
        private List<double> crossPercents;
        private List<double> rsiData;
        private List<double> rsiFastData;
        private List<double> rsiSlowData;
        
        private int currentOptimalLength = 14;
        private double bestPercent = 0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Machine Learning: Optimal RSI [YinYangAlgorithms] - NT8 Version";
                Name = "OptimalRSI";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                // Default parameters
                ShowSignals = true;
                ShowTables = true;
                ShowNewSettings = true;
                ShowBollingerBands = true;
                OptimalType = OptimalRSIType.AllCrossings;
                AIAdjust = AdjustType.Auto;
                OptimalLength = 200;
                RSICount = 30;
                RSIMinLength = 4;
                MALength = 14;
                BackupLength = 14;
                UseRationalQuadratics = true;
                OnlyUseSimilarMA = false;
                MachineLearningType = MLType.SimpleAverage;
                DistanceType = KNNDistanceType.Both;
                MLLength = 10;
                KNNLength = 3;
                FastLength = 1;
                SlowLength = 5;
                
                AddPlot(Brushes.Purple, "RSI");
                AddPlot(Brushes.Yellow, "RSI_MA");
               AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Dot, "BullSignal");
				AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Dot, "BearSignal");
                AddPlot(Brushes.Green, "UpperInner");
                AddPlot(Brushes.Green, "LowerInner");
                AddPlot(Brushes.Green, "UpperOuter");
                AddPlot(Brushes.Red, "LowerOuter");
                
                AddLine(new Stroke(Brushes.Gray, 1), 70, "UpperBand");
                AddLine(new Stroke(Brushes.Gray, 1), 50, "MiddleBand");
                AddLine(new Stroke(Brushes.Gray, 1), 30, "LowerBand");
            }
            else if (State == State.DataLoaded)
            {
                optimalRSI = new Series<double>(this);
                rsiMA = new Series<double>(this);
                upperInner = new Series<double>(this);
                lowerInner = new Series<double>(this);
                upperOuter = new Series<double>(this);
                lowerOuter = new Series<double>(this);
                
                crossPercents = new List<double>();
                rsiData = new List<double>();
                rsiFastData = new List<double>();
                rsiSlowData = new List<double>();
            }
        }

        protected override void OnBarUpdate()
		{
		    if (CurrentBar < Math.Max(OptimalLength, MLLength))
		        return;
		
		    // Auto-adjust parameters if needed
		    if (AIAdjust == AdjustType.Auto)
		    {
		        var adjusted = AutoAdjust();
		        OptimalLength = adjusted.Item1;
		        RSICount = adjusted.Item2;
		    }
		
		    // Calculate optimal RSI length
		    var optimal = GetOptimalRSILength();
		    currentOptimalLength = optimal.Item1;
		    bestPercent = optimal.Item2;
		
		    // Calculate RSI with optimal length
		    double rsiValue = CalculateRSI(Close, currentOptimalLength, 0);
		    optimalRSI[0] = rsiValue;
		
		    // Apply Machine Learning if selected BEFORE calculating RSI MA
		    if (MachineLearningType != MLType.None)
		    {
		        // Calculate temp MA for ML filtering
		        double tempMA = CalculateRSIMA(0); // Calculate MA from current RSI values
		        rsiValue = ApplyMachineLearning(rsiValue, tempMA);
		        optimalRSI[0] = rsiValue;
		    }
		
		    // Calculate RSI MA AFTER ML adjustments
		    rsiMA[0] = CalculateRSIMA(0);
		
		    // Calculate Bollinger Bands
		    if (ShowBollingerBands)
		    {
		        CalculateBollingerBands();
		    }
		
		    // Set plot values
		    Values[0][0] = optimalRSI[0];
		    Values[1][0] = rsiMA[0];
			
//			Values[2][0] = double.NaN;
//    		Values[3][0] = double.NaN;

		
		    // Check for crossovers
		    // Only plot signals at actual crossover points
		   if (CurrentBar > 1 && ShowSignals)
    {
        bool bullCross = optimalRSI[0] > rsiMA[0] && optimalRSI[1] <= rsiMA[1];
        bool bearCross = optimalRSI[0] < rsiMA[0] && optimalRSI[1] >= rsiMA[1];

        if (bullCross)
        {
            Draw.Dot(this, "BullSignal" + CurrentBar, false, 0, optimalRSI[0], Brushes.Green);
        }
        
        if (bearCross)
        {
            Draw.Dot(this, "BearSignal" + CurrentBar, false, 0, optimalRSI[0], Brushes.Red);
        }
    }
				
		    // Set Bollinger Band values
		    if (ShowBollingerBands)
		    {
		        Values[4][0] = upperInner[0];
		        Values[5][0] = lowerInner[0];
		        Values[6][0] = upperOuter[0];
		        Values[7][0] = lowerOuter[0];
		    }
		    else
		    {
		        Values[4][0] = double.NaN;
		        Values[5][0] = double.NaN;
		        Values[6][0] = double.NaN;
		        Values[7][0] = double.NaN;
		    }
		}
		
		// New method to properly calculate RSI MA
		private double CalculateRSIMA(int barsAgo)
		{
		    if (CurrentBar < MALength + barsAgo)
		        return 50;
		
		    double sum = 0;
		    for (int i = 0; i < MALength; i++)
		    {
		        if (CurrentBar >= i + barsAgo)
		            sum += optimalRSI[i + barsAgo];
		    }
		    return sum / MALength;
		}

        private (int, int) AutoAdjust()
		{
		    int newLength = 30;  // More conservative default
		    int newCount = 10;   // Reduced count
		
		    if (CurrentBar <= 5000)
		    {
		        newLength = 50;
		        newCount = 15;
		    }
		    else if (CurrentBar <= 10000)
		    {
		        newLength = 40;
		        newCount = 12;
		    }
		    else if (CurrentBar <= 20000)
		    {
		        newLength = 35;
		        newCount = 10;
		    }
		    else
		    {
		        newLength = 30;
		        newCount = 8;
		    }
		
		    return (newLength, newCount);
		}


        private (int, double) GetOptimalRSILength()
		{
		    // Early exit if not enough bars
		    if (CurrentBar < OptimalLength)
		        return (BackupLength, 0);
		
		    crossPercents.Clear();
		    
		    // Limit the actual calculation length based on available bars
		    int actualOptimalLength = Math.Min(OptimalLength, CurrentBar - 1);
		    
		    for (int i = 0; i < RSICount; i++)
		    {
		        int len = i + RSIMinLength;
		        double crossPercent = 0;
		        int crossCount = 0;
		        int crossType = 0;
		        double crossClose = 0;
		        bool inExtremity = false;
		
		        // Process bars in smaller chunks to prevent timeout
		        for (int a = 0; a < actualOptimalLength; a++)
		        {
		            // Skip if not enough data for this calculation
		            if (CurrentBar < len + a + 1)
		                continue;
		
		            double currentRSI = CalculateRSI(Close, len, a);
		            double currentMA = CalculateSMA(currentRSI, MALength, len, a);
		
		            bool crossOver = a > 0 && currentRSI > currentMA && 
		                           CalculateRSI(Close, len, a + 1) <= CalculateSMA(CalculateRSI(Close, len, a + 1), MALength, len, a + 1);
		            bool crossUnder = a > 0 && currentRSI < currentMA && 
		                            CalculateRSI(Close, len, a + 1) >= CalculateSMA(CalculateRSI(Close, len, a + 1), MALength, len, a + 1);
		
		            bool currentOver = OptimalType == OptimalRSIType.AllCrossings || inExtremity ? 
		                             crossOver : currentRSI <= 40 && crossOver;
		            bool currentUnder = OptimalType == OptimalRSIType.AllCrossings || inExtremity ? 
		                              crossUnder : currentRSI >= 60 && crossUnder;
		
		            if (currentOver)
		            {
		                if (crossType != 0 && crossClose > 0)
		                {
		                    crossPercent += crossClose / Close[a];
		                    crossCount++;
		                }
		                crossClose = Close[a];
		                crossType = 1;
		                inExtremity = !inExtremity;
		            }
		            else if (currentUnder)
		            {
		                if (crossType != 0 && crossClose > 0)
		                {
		                    crossPercent += Close[a] / crossClose;
		                    crossCount++;
		                }
		                crossClose = Close[a];
		                crossType = -1;
		                inExtremity = !inExtremity;
		            }
		        }
		
		        crossPercents.Add(crossCount > 0 ? crossPercent / crossCount : 0);
		    }
		
		    double bestPercent = -100000;
		    int bestIndex = 0;
		    for (int p = 0; p < crossPercents.Count; p++)
		    {
		        if (crossPercents[p] > bestPercent)
		        {
		            bestPercent = crossPercents[p];
		            bestIndex = p;
		        }
		    }
		
		    int optimal = bestPercent != -100000 ? bestIndex + RSIMinLength : BackupLength;
		    return (optimal, bestPercent);
		}


        private double CalculateRSI(ISeries<double> source, int period, int barsAgo)
        {
            if (CurrentBar < period + barsAgo)
                return 50;

            double gain = 0;
            double loss = 0;

            for (int i = 1; i <= period; i++)
            {
                double change = source[barsAgo + i - 1] - source[barsAgo + i];
                if (change > 0)
                    gain += change;
                else
                    loss -= change;
            }

            if (loss == 0)
                return 100;

            double rs = (gain / period) / (loss / period);
            return 100 - (100 / (1 + rs));
        }

        private double CalculateSMA(double rsiValue, int period, int rsiPeriod, int barsAgo)
        {
            double sum = 0;
            for (int i = 0; i < period; i++)
            {
                sum += CalculateRSI(Close, rsiPeriod, barsAgo + i);
            }
            return sum / period;
        }

        private double ApplyMachineLearning(double rsiValue, double tempMA)
        {
            if (MachineLearningType == MLType.SimpleAverage)
            {
                rsiData.Clear();
                bool rsiBull = rsiValue >= tempMA;

                for (int i = 0; i < MLLength && i < CurrentBar; i++)
                {
                    double pastRSI = CalculateRSI(Close, currentOptimalLength, i);
                    double pastMA = CalculateSMA(pastRSI, MALength, currentOptimalLength, i);
                    bool pastBull = pastRSI > pastMA;

                    if (!OnlyUseSimilarMA || pastBull == rsiBull)
                    {
                        rsiData.Add(pastRSI);
                    }
                }

                return rsiData.Count > 0 ? rsiData.Average() : rsiValue;
            }
            else if (MachineLearningType == MLType.KNNAverage || MachineLearningType == MLType.KNNExponentialAverage)
            {
                return CalculateKNNAverage(rsiValue);
            }

            return rsiValue;
        }

        private double CalculateKNNAverage(double rsiValue)
        {
            rsiFastData.Clear();
            rsiSlowData.Clear();

            for (int i = 0; i < MLLength && i < CurrentBar; i++)
            {
                double pastRSI = CalculateRSI(Close, currentOptimalLength, i);
                rsiFastData.Add(CalculateSMA(pastRSI, FastLength, currentOptimalLength, i));
                rsiSlowData.Add(CalculateSMA(pastRSI, SlowLength, currentOptimalLength, i));
            }

            if (rsiFastData.Count == 0 || rsiSlowData.Count == 0)
                return rsiValue;

            List<double> distances = new List<double>();
            for (int i = 0; i < Math.Min(rsiFastData.Count, rsiSlowData.Count); i++)
            {
                distances.Add(rsiSlowData[i] - rsiFastData[i]);
            }

            distances.Sort();
            int knnCount = Math.Min(KNNLength, distances.Count);
            double maxDist = distances.Take(knnCount).Max();
            double minDist = distances.Take(knnCount).Min();

            List<double> validDistances = new List<double>();
            for (int i = 0; i < Math.Min(rsiFastData.Count, rsiSlowData.Count); i++)
            {
                double dist = rsiSlowData[i] - rsiFastData[i];
                bool validMax = DistanceType != KNNDistanceType.Max || dist <= maxDist;
                bool validMin = DistanceType != KNNDistanceType.Min || dist >= minDist;
                
                if (validMax && validMin)
                {
                    validDistances.Add((rsiSlowData[i] + rsiFastData[i]) / 2);
                }
            }

            return validDistances.Count > 0 ? validDistances.Average() : rsiValue;
        }

        private void CalculateBollingerBands()
        {
            if (CurrentBar < OptimalLength)
                return;

            double basis = SMA(optimalRSI, OptimalLength)[0];
            double deviation = StdDev(optimalRSI, OptimalLength)[0];
            
            upperInner[0] = basis + (1.6185 * deviation);
            lowerInner[0] = basis - (1.6185 * deviation);
            upperOuter[0] = basis + (2.0 * deviation);
            lowerOuter[0] = basis - (2.0 * deviation);
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Show Signals", Order = 1, GroupName = "RSI Settings")]
        public bool ShowSignals { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Tables", Order = 2, GroupName = "RSI Settings")]
        public bool ShowTables { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show New Settings", Order = 3, GroupName = "RSI Settings")]
        public bool ShowNewSettings { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Bollinger Bands", Order = 4, GroupName = "RSI Settings")]
        public bool ShowBollingerBands { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Optimal RSI Type", Order = 5, GroupName = "RSI Settings")]
        public OptimalRSIType OptimalType { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "AI Adjust", Order = 6, GroupName = "RSI Settings")]
        public AdjustType AIAdjust { get; set; }

        [NinjaScriptProperty]
        [Range(10, 500)]
        [Display(Name = "Optimal Length", Order = 7, GroupName = "RSI Settings")]
        public int OptimalLength { get; set; }

        [NinjaScriptProperty]
        [Range(5, 50)]
        [Display(Name = "RSI Count", Order = 8, GroupName = "RSI Settings")]
        public int RSICount { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "RSI Min Length", Order = 9, GroupName = "RSI Settings")]
        public int RSIMinLength { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MA Length", Order = 10, GroupName = "RSI Settings")]
        public int MALength { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Backup Length", Order = 11, GroupName = "RSI Settings")]
        public int BackupLength { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Rational Quadratics", Order = 12, GroupName = "Machine Learning")]
        public bool UseRationalQuadratics { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Only Use Similar MA", Order = 13, GroupName = "Machine Learning")]
        public bool OnlyUseSimilarMA { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Machine Learning Type", Order = 14, GroupName = "Machine Learning")]
        public MLType MachineLearningType { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Distance Type", Order = 15, GroupName = "Machine Learning")]
        public KNNDistanceType DistanceType { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ML Length", Order = 16, GroupName = "Machine Learning")]
        public int MLLength { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "KNN Length", Order = 17, GroupName = "Machine Learning")]
        public int KNNLength { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast Length", Order = 18, GroupName = "Machine Learning")]
        public int FastLength { get; set; }

        [NinjaScriptProperty]
        [Range(2, int.MaxValue)]
        [Display(Name = "Slow Length", Order = 19, GroupName = "Machine Learning")]
        public int SlowLength { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OptimalRSI[] cacheOptimalRSI;
		public OptimalRSI OptimalRSI(bool showSignals, bool showTables, bool showNewSettings, bool showBollingerBands, OptimalRSIType optimalType, AdjustType aIAdjust, int optimalLength, int rSICount, int rSIMinLength, int mALength, int backupLength, bool useRationalQuadratics, bool onlyUseSimilarMA, MLType machineLearningType, KNNDistanceType distanceType, int mLLength, int kNNLength, int fastLength, int slowLength)
		{
			return OptimalRSI(Input, showSignals, showTables, showNewSettings, showBollingerBands, optimalType, aIAdjust, optimalLength, rSICount, rSIMinLength, mALength, backupLength, useRationalQuadratics, onlyUseSimilarMA, machineLearningType, distanceType, mLLength, kNNLength, fastLength, slowLength);
		}

		public OptimalRSI OptimalRSI(ISeries<double> input, bool showSignals, bool showTables, bool showNewSettings, bool showBollingerBands, OptimalRSIType optimalType, AdjustType aIAdjust, int optimalLength, int rSICount, int rSIMinLength, int mALength, int backupLength, bool useRationalQuadratics, bool onlyUseSimilarMA, MLType machineLearningType, KNNDistanceType distanceType, int mLLength, int kNNLength, int fastLength, int slowLength)
		{
			if (cacheOptimalRSI != null)
				for (int idx = 0; idx < cacheOptimalRSI.Length; idx++)
					if (cacheOptimalRSI[idx] != null && cacheOptimalRSI[idx].ShowSignals == showSignals && cacheOptimalRSI[idx].ShowTables == showTables && cacheOptimalRSI[idx].ShowNewSettings == showNewSettings && cacheOptimalRSI[idx].ShowBollingerBands == showBollingerBands && cacheOptimalRSI[idx].OptimalType == optimalType && cacheOptimalRSI[idx].AIAdjust == aIAdjust && cacheOptimalRSI[idx].OptimalLength == optimalLength && cacheOptimalRSI[idx].RSICount == rSICount && cacheOptimalRSI[idx].RSIMinLength == rSIMinLength && cacheOptimalRSI[idx].MALength == mALength && cacheOptimalRSI[idx].BackupLength == backupLength && cacheOptimalRSI[idx].UseRationalQuadratics == useRationalQuadratics && cacheOptimalRSI[idx].OnlyUseSimilarMA == onlyUseSimilarMA && cacheOptimalRSI[idx].MachineLearningType == machineLearningType && cacheOptimalRSI[idx].DistanceType == distanceType && cacheOptimalRSI[idx].MLLength == mLLength && cacheOptimalRSI[idx].KNNLength == kNNLength && cacheOptimalRSI[idx].FastLength == fastLength && cacheOptimalRSI[idx].SlowLength == slowLength && cacheOptimalRSI[idx].EqualsInput(input))
						return cacheOptimalRSI[idx];
			return CacheIndicator<OptimalRSI>(new OptimalRSI(){ ShowSignals = showSignals, ShowTables = showTables, ShowNewSettings = showNewSettings, ShowBollingerBands = showBollingerBands, OptimalType = optimalType, AIAdjust = aIAdjust, OptimalLength = optimalLength, RSICount = rSICount, RSIMinLength = rSIMinLength, MALength = mALength, BackupLength = backupLength, UseRationalQuadratics = useRationalQuadratics, OnlyUseSimilarMA = onlyUseSimilarMA, MachineLearningType = machineLearningType, DistanceType = distanceType, MLLength = mLLength, KNNLength = kNNLength, FastLength = fastLength, SlowLength = slowLength }, input, ref cacheOptimalRSI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OptimalRSI OptimalRSI(bool showSignals, bool showTables, bool showNewSettings, bool showBollingerBands, OptimalRSIType optimalType, AdjustType aIAdjust, int optimalLength, int rSICount, int rSIMinLength, int mALength, int backupLength, bool useRationalQuadratics, bool onlyUseSimilarMA, MLType machineLearningType, KNNDistanceType distanceType, int mLLength, int kNNLength, int fastLength, int slowLength)
		{
			return indicator.OptimalRSI(Input, showSignals, showTables, showNewSettings, showBollingerBands, optimalType, aIAdjust, optimalLength, rSICount, rSIMinLength, mALength, backupLength, useRationalQuadratics, onlyUseSimilarMA, machineLearningType, distanceType, mLLength, kNNLength, fastLength, slowLength);
		}

		public Indicators.OptimalRSI OptimalRSI(ISeries<double> input , bool showSignals, bool showTables, bool showNewSettings, bool showBollingerBands, OptimalRSIType optimalType, AdjustType aIAdjust, int optimalLength, int rSICount, int rSIMinLength, int mALength, int backupLength, bool useRationalQuadratics, bool onlyUseSimilarMA, MLType machineLearningType, KNNDistanceType distanceType, int mLLength, int kNNLength, int fastLength, int slowLength)
		{
			return indicator.OptimalRSI(input, showSignals, showTables, showNewSettings, showBollingerBands, optimalType, aIAdjust, optimalLength, rSICount, rSIMinLength, mALength, backupLength, useRationalQuadratics, onlyUseSimilarMA, machineLearningType, distanceType, mLLength, kNNLength, fastLength, slowLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OptimalRSI OptimalRSI(bool showSignals, bool showTables, bool showNewSettings, bool showBollingerBands, OptimalRSIType optimalType, AdjustType aIAdjust, int optimalLength, int rSICount, int rSIMinLength, int mALength, int backupLength, bool useRationalQuadratics, bool onlyUseSimilarMA, MLType machineLearningType, KNNDistanceType distanceType, int mLLength, int kNNLength, int fastLength, int slowLength)
		{
			return indicator.OptimalRSI(Input, showSignals, showTables, showNewSettings, showBollingerBands, optimalType, aIAdjust, optimalLength, rSICount, rSIMinLength, mALength, backupLength, useRationalQuadratics, onlyUseSimilarMA, machineLearningType, distanceType, mLLength, kNNLength, fastLength, slowLength);
		}

		public Indicators.OptimalRSI OptimalRSI(ISeries<double> input , bool showSignals, bool showTables, bool showNewSettings, bool showBollingerBands, OptimalRSIType optimalType, AdjustType aIAdjust, int optimalLength, int rSICount, int rSIMinLength, int mALength, int backupLength, bool useRationalQuadratics, bool onlyUseSimilarMA, MLType machineLearningType, KNNDistanceType distanceType, int mLLength, int kNNLength, int fastLength, int slowLength)
		{
			return indicator.OptimalRSI(input, showSignals, showTables, showNewSettings, showBollingerBands, optimalType, aIAdjust, optimalLength, rSICount, rSIMinLength, mALength, backupLength, useRationalQuadratics, onlyUseSimilarMA, machineLearningType, distanceType, mLLength, kNNLength, fastLength, slowLength);
		}
	}
}

#endregion
