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
using NinjaTrader.NinjaScript.Indicators.ZombiePack9;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class LEOTRENDV402 : Indicator
    {
        #region Variables
        private double chopValue = 0.0;
        private double highestVolume = 0.0;
        private double adxThreshold = 20.0;
        private bool isChoppy = false;
        private Brush chopColor = Brushes.Yellow;
        private Brush uptrendColor = Brushes.Green;
        private Brush downtrendColor = Brushes.Red;
        private Brush highVolumeUptrendColor = Brushes.Blue;
        private Brush highVolumeDowntrendColor = Brushes.Orange;
        private Brush significantHighLowColor = Brushes.Purple;
        private Series<double> volumeSeries;
        private ADX adx;
        private DM dm;
        private double highestHigh;
        private double lowestLow;
        private Series<bool> isGreenAndPurple;
        private Series<bool> isRedAndPurple;
        private Series<double> significantVolumeSeries;
        private Series<double> arrowSignalSeries;
        private MACD macd;
        private OBV obv;
        private VROC vroc;
        private ZiSchaffTrendCycle ziSchaffTrendCycle;
        private int significantHighLowLookback = 50;
        private double macdDiffChopThreshold = 0.67;
        private bool signalGenerated = false;
        private double obvThreshold = 100000;
        private double macdDiffUpperThreshold = 0.19;
        private double macdDiffLowerThreshold = -0.13;
        private int trend1 = 50;
        private int trend2 = 100;
        private Series<double> a1;
        private Series<double> b1;
        private Series<double> avg1;
        private Series<double> a2;
        private Series<double> b2;
        private Series<double> avg2;
        private bool isCountingRange = false;
        private double rangeStartValue = 0.0;
        private int chopRangePoints = 30;
        private int barsSinceRangeStart = 0;
        private double rangeHigh = 0.0;
        private double rangeLow = 0.0;
        private bool enableRangeFilter = true;
        private Zombie9Squeeze zombie9Squeeze;
        private VolumeOscillator volumeOscillator;
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Period", Description = "Period for calculating the chop indicator", Order = 0, GroupName = "Parameters")]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Chop Threshold", Description = "Threshold to identify chop conditions", Order = 1, GroupName = "Parameters")]
        public double ChopThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Volume Threshold", Description = "Threshold to identify high volume conditions", Order = 2, GroupName = "Parameters")]
        public double VolumeThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Volume Spike Threshold", Description = "Additional threshold for volume spikes", Order = 3, GroupName = "Parameters")]
        public double VolumeSpikeThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Paint Bars", Description = "Enable or disable painting bars based on chop conditions", Order = 4, GroupName = "Parameters")]
        public bool PaintBars { get; set; } 
       
        [NinjaScriptProperty]
        [Display(Name = "Significant High/Low Lookback", Description = "Lookback period for significant highs/lows", Order = 5, GroupName = "Parameters")]
        public int SignificantHighLowLookback { get; set; } 

        [NinjaScriptProperty]
        [Display(Name = "ADX Threshold", Description = "Threshold to identify ADX conditions", Order = 6, GroupName = "Parameters")]
        public double AdxThreshold
        {
            get { return adxThreshold; }
            set { adxThreshold = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Enable Trend Calculation", Description = "Enable or disable the trend calculation logic", Order = 7, GroupName = "Filters")]
        public bool EnableTrendCalculation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Chop Value", Description = "Enable or disable the chop value calculation logic", Order = 8, GroupName = "Filters")]
        public bool EnableChopValue { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Significant Volume Detection", Description = "Enable or disable the significant volume detection logic", Order = 9, GroupName = "Filters")]
        public bool EnableSignificantVolumeDetection { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable MACD Coloring", Description = "Enable or disable the MACD coloring logic", Order = 10, GroupName = "Filters")]
        public bool EnableMACDColoring { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable VROC Overlay", Description = "Enable or disable the VROC overlay logic", Order = 11, GroupName = "Filters")]
        public bool EnableVROCOverlay { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Zombie9 Squeeze Volume", Description = "Enable or disable the Zombie9 Squeeze volume logic", Order = 12, GroupName = "Filters")]
        public bool EnableZombie9SqueezeVolume { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Range Filter", Description = "Enable or disable the range filter logic", Order = 13, GroupName = "Filters")]
        public bool EnableRangeFilter { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ChopIndicator
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VolumeHistogram
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SignificantVolumeHistogram
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> GreenAndPurple
        {
            get { return isGreenAndPurple; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> RedAndPurple
        {
            get { return isRedAndPurple; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> GreenAndPurpleSeries
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RedAndPurpleSeries
        {
            get { return Values[4]; }
        }

        [XmlIgnore]
        [Display(Name = "Chop Color", Description = "Color for chop conditions", Order = 13, GroupName = "Plot Colors")]
        public Brush ChopColor
        {
            get { return chopColor; }
            set { chopColor = value; }
        }

        [Browsable(false)]
        public string ChopColorSerialize
        {
            get { return Serialize.BrushToString(chopColor); }
            set { chopColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Uptrend Color", Description = "Color for uptrend conditions", Order = 14, GroupName = "Plot Colors")]
        public Brush UptrendColor
        {
            get { return uptrendColor; }
            set { uptrendColor = value; }
        }

        [Browsable(false)]
        public string UptrendColorSerialize
        {
            get { return Serialize.BrushToString(uptrendColor); }
            set { uptrendColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Downtrend Color", Description = "Color for downtrend conditions", Order = 15, GroupName = "Plot Colors")]
        public Brush DowntrendColor
        {
            get { return downtrendColor; }
            set { downtrendColor = value; }
        }

        [Browsable(false)]
        public string DowntrendColorSerialize
        {
            get { return Serialize.BrushToString(downtrendColor); }
            set { downtrendColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "High Volume Uptrend Color", Description = "Color for high volume uptrend conditions", Order = 16, GroupName = "Plot Colors")]
        public Brush HighVolumeUptrendColor
        {
            get { return highVolumeUptrendColor; }
            set { highVolumeUptrendColor = value; }
        }

        [Browsable(false)]
        public string HighVolumeUptrendColorSerialize
        {
            get { return Serialize.BrushToString(highVolumeUptrendColor); }
            set { highVolumeUptrendColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "High Volume Downtrend Color", Description = "Color for high volume downtrend conditions", Order = 17, GroupName = "Plot Colors")]
        public Brush HighVolumeDowntrendColor
        {
            get { return highVolumeDowntrendColor; }
            set { highVolumeDowntrendColor = value; }
        }

        [Browsable(false)]
        public string HighVolumeDowntrendColorSerialize
        {
            get { return Serialize.BrushToString(highVolumeDowntrendColor); }
            set { highVolumeDowntrendColor = Serialize.StringToBrush(value); }
        }
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enhanced indicator to identify volatile market conditions and trade signals using Unirenko bars with volume consideration.";
                Name = "LEOTRENDV402";
                Calculate = Calculate.OnEachTick;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                Period = 14;  // Adjust this to suit your Unirenko bar size and trading style
                trend1 = 50;  // Example adjustment for Unirenko bars
                trend2 = 100; // Example adjustment for Unirenko bars
                ChopThreshold = 4; // Set to 4 as per your requirement
                AdxThreshold = 20;
                PaintBars = true;

                EnableTrendCalculation = true;
                EnableChopValue = true;
                EnableSignificantVolumeDetection = true;
                EnableMACDColoring = true;
                EnableVROCOverlay = true;
                EnableZombie9SqueezeVolume = true;
                EnableRangeFilter = true;

                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "ChopValue");
                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "VolumeHistogram");
                AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Bar, "SignificantVolumeHistogram");
                AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Bar, "GreenAndPurple");
                AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Bar, "RedAndPurple");
                AddLine(Brushes.Red, ChopThreshold, "Threshold");

                adx = ADX(14);
                dm = DM(14);
                macd = MACD(12, 26, 9);
                obv = OBV();
                vroc = VROC(14, 3);
                ziSchaffTrendCycle = ZiSchaffTrendCycle(12, 26, 9, 14, 0.5);
                zombie9Squeeze = Zombie9Squeeze("Zombie9Squeeze", true, 20, 0.38, 20, 0.38, Brushes.LightGreen, Brushes.ForestGreen, Brushes.Red, Brushes.LightCoral, Brushes.DeepSkyBlue, Brushes.Transparent);
                volumeOscillator = VolumeOscillator(14, 28);
            }
            else if (State == State.DataLoaded)
            {
                volumeSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                significantVolumeSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                arrowSignalSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                isGreenAndPurple = new Series<bool>(this, MaximumBarsLookBack.Infinite);
                isRedAndPurple = new Series<bool>(this, MaximumBarsLookBack.Infinite);
                a1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                b1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                avg1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                a2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                b2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                avg2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Period || CurrentBar < significantHighLowLookback)
            {
                Values[0][0] = 0;
                Values[1][0] = 0;
                Values[2][0] = 0;
                Values[3][0] = 0;
                Values[4][0] = 0;
                return;
            }

            if (EnableTrendCalculation)
            {
                // Initialize trend calculations
                a1[0] = Input[0];
                b1[0] = Input[0];
                a2[0] = Input[0];
                b2[0] = Input[0];

                // Update trend calculations
                if (Input[0] > a1[1])
                    a1[0] = Input[0];
                else
                    a1[0] = a1[1] - (a1[1] - b1[1]) / trend1;

                if (Input[0] < b1[1])
                    b1[0] = Input[0];
                else
                    b1[0] = b1[1] + (a1[1] - b1[1]) / trend1;

                avg1[0] = (a1[0] + b1[0]) / 2;

                if (Input[0] > a2[1])
                    a2[0] = Input[0];
                else
                    a2[0] = a2[1] - (a2[1] - b2[1]) / trend2;

                if (Input[0] < b2[1])
                    b2[0] = Input[0];
                else
                    b2[0] = b2[1] + (a2[1] - b2[1]) / trend2;

                avg2[0] = (a2[0] + b2[0]) / 2;

                // Determine trend direction
                bool isUptrend = avg1[0] > avg2[0];
                bool isDowntrend = avg1[0] < avg2[0];

                // Apply colors based on trend direction and volume
                if (isUptrend)
                {
                    PlotBrushes[0][0] = Volume[0] >= VolumeThreshold ? highVolumeUptrendColor : uptrendColor;
                    if (PaintBars)
                    {
                        BarBrush = PlotBrushes[0][0];
                        CandleOutlineBrush = PlotBrushes[0][0];
                    }
                }
                else if (isDowntrend)
                {
                    PlotBrushes[0][0] = Volume[0] >= VolumeThreshold ? highVolumeDowntrendColor : downtrendColor;
                    if (PaintBars)
                    {
                        BarBrush = PlotBrushes[0][0];
                        CandleOutlineBrush = PlotBrushes[0][0];
                    }
                }

                if (EnableZombie9SqueezeVolume && IsFirstTickOfBar)
                {
                    bool isVolumeDecreasing = zombie9Squeeze.VolumeHistogram[0] < zombie9Squeeze.VolumeHistogram[1];
                    bool isPriceMakingHigherHighs = High[0] > High[1];
                    bool isPriceMakingLowerLows = Low[0] < Low[1];

                    if ((isDowntrend && isPriceMakingLowerLows) || (isUptrend && isPriceMakingHigherHighs))
                    {
                        if (isVolumeDecreasing)
                        {
                            PlotBrushes[0][0] = isDowntrend ? (Volume[0] >= VolumeThreshold ? highVolumeDowntrendColor : downtrendColor) : (Volume[0] >= VolumeThreshold ? highVolumeUptrendColor : uptrendColor);
                            BarBrush = PlotBrushes[0][0];
                            CandleOutlineBrush = PlotBrushes[0][0];
                            Values[3][0] = 0;
                            return;
                        }
                    }
                }

                bool isFollowingTrend = (Close[0] > Open[0] && avg1[0] > avg2[0]) || (Close[0] < Open[0] && avg1[0] < avg2[0]);

                if (EnableRangeFilter && enableRangeFilter && !isCountingRange && Values[1][0] >= 68)
                {
                    isCountingRange = true;
                    rangeStartValue = Close[0];
                    rangeHigh = rangeStartValue + chopRangePoints * TickSize;
                    rangeLow = rangeStartValue - chopRangePoints * TickSize;
                    barsSinceRangeStart = 0;
                }

                if (EnableRangeFilter && enableRangeFilter && isCountingRange)
                {
                    barsSinceRangeStart++;

                    if (High[0] > rangeHigh || Low[0] < rangeLow)
                    {
                        isCountingRange = false;
                        signalGenerated = false;
                    }
                    else if (!isFollowingTrend)
                    {
                        if (PaintBars)
                        {
                            BarBrush = chopColor;
                            CandleOutlineBrush = chopColor;
                        }
                    }
                    else if (!isChoppy)
                    {
                        PlotBrushes[0][0] = PlotBrushes[1][0];
                        if (PaintBars)
                        {
                            BarBrush = PlotBrushes[1][0];
                            CandleOutlineBrush = PlotBrushes[1][0];
                        }
                    }
                }

                if (EnableMACDColoring)
                {
                    UpdateBarColorsBasedOnMACD();
                }

                if (EnableVROCOverlay && (PlotBrushes[1][0] == downtrendColor || PlotBrushes[1][0] == highVolumeDowntrendColor) && (vroc[0] < 33 && vroc[0] > -30))
                {
                    BarBrush = downtrendColor;
                    CandleOutlineBrush = downtrendColor;
                }
                else if (EnableVROCOverlay && (PlotBrushes[1][0] == uptrendColor || PlotBrushes[1][0] == highVolumeUptrendColor) && (vroc[0] < 33 && vroc[0] > -30))
                {
                    BarBrush = uptrendColor;
                    CandleOutlineBrush = uptrendColor;
                }

                if (!signalGenerated)
                {
                    if (CrossAbove(avg1, avg2, 1))
                    {
                        if (volumeOscillator[0] > 0 && High[0] > High[1])
                        {
                            Draw.ArrowUp(this, "TrendCrossUp" + CurrentBar, true, 0, Low[0] - 2 * TickSize, uptrendColor);
                            Values[3][0] = 1;
                            signalGenerated = true;
                        }
                    }
                    else if (CrossBelow(avg1, avg2, 1))
                    {
                        if (volumeOscillator[0] < 0 && Low[0] < Low[1])
                        {
                            Draw.ArrowDown(this, "TrendCrossDown" + CurrentBar, true, 0, High[0] + 2 * TickSize, downtrendColor);
                            Values[3][0] = -1;
                            signalGenerated = true;
                        }
                    }
                }
            }

            if (EnableChopValue)
            {
                double highestHigh = High[HighestBar(High, Period)];
                double lowestLow = Low[LowestBar(Low, Period)];
                double range = highestHigh - lowestLow;
                double totalMovement = 0.0;
                highestVolume = 0.0;

                for (int i = 0; i < Period; i++)
                {
                    totalMovement += Math.Abs(Close[i] - Close[i + 1]);
                    if (Volume[i] > highestVolume)
                    {
                        highestVolume = Volume[i];
                    }
                }

                chopValue = totalMovement / range;
                Values[0][0] = chopValue;
                Values[1][0] = Volume[0];
            }

            isChoppy = chopValue > ChopThreshold && adx[0] < AdxThreshold;

            if (EnableSignificantVolumeDetection)
            {
                double significantHighestHigh = High[HighestBar(High, significantHighLowLookback)];
                double significantLowestLow = Low[LowestBar(Low, significantHighLowLookback)];

                bool isSignificantHigh = High[0] >= significantHighestHigh;
                bool isSignificantLow = Low[0] <= significantLowestLow;

                if (isSignificantHigh || isSignificantLow)
                {
                    Values[2][0] = -Volume[0];
                    PlotBrushes[2][0] = significantHighLowColor;
                    if (PaintBars && !isChoppy)
                    {
                        BarBrush = significantHighLowColor;
                        CandleOutlineBrush = significantHighLowColor;
                    }
                }
                else
                {
                    Values[2][0] = 0;
                }
            }

            // Check for ADX and volume spike for reversal signals
            if (adx[0] > AdxThreshold && Volume[0] > VolumeSpikeThreshold)
            {
                if (CrossAbove(dm.DiPlus, dm.DiMinus, 1) && Close[0] > Open[0] && Close[1] < Open[1])
                {
                    Draw.ArrowUp(this, "LongSignal" + CurrentBar, true, 0, Low[0] - 2 * TickSize, Brushes.Lime);
                    BarBrushes[0] = Brushes.Yellow;
                    CandleOutlineBrushes[0] = Brushes.Yellow;
                }
                else if (CrossBelow(dm.DiPlus, dm.DiMinus, 1) && Close[0] < Open[0] && Close[1] > Open[1])
                {
                    Draw.ArrowDown(this, "ShortSignal" + CurrentBar, true, 0, High[0] + 2 * TickSize, Brushes.Red);
                    BarBrushes[0] = Brushes.Yellow;
                    CandleOutlineBrushes[0] = Brushes.Yellow;
                }
            }

            // Ensure the volume histogram above zero retains its color
            if (Close[0] > Open[0])
            {
                PlotBrushes[1][0] = uptrendColor;
            }
            else
            {
                PlotBrushes[1][0] = downtrendColor;
            }
        }

        private void UpdateBarColorsBasedOnMACD()
        {
            double macdDiff = macd[0] - macd.Avg[0];
            bool isMacdDiffInRange = macdDiff <= macdDiffUpperThreshold && macdDiff >= macdDiffLowerThreshold;

            if (isMacdDiffInRange)
            {
                if (Volume[0] >= VolumeThreshold)
                {
                    if (Close[0] > Open[0])
                    {
                        PlotBrushes[0][0] = highVolumeUptrendColor;
                        PlotBrushes[1][0] = highVolumeUptrendColor;
                        if (PaintBars)
                        {
                            BarBrushes[0] = highVolumeUptrendColor;
                            CandleOutlineBrushes[0] = highVolumeUptrendColor;
                        }
                    }
                    else
                    {
                        PlotBrushes[0][0] = highVolumeDowntrendColor;
                        PlotBrushes[1][0] = highVolumeDowntrendColor;
                        if (PaintBars)
                        {
                            BarBrushes[0] = highVolumeDowntrendColor;
                            CandleOutlineBrushes[0] = highVolumeDowntrendColor;
                        }
                    }
                }
                else
                {
                    if (Close[0] > Open[0])
                    {
                        PlotBrushes[0][0] = uptrendColor;
                        PlotBrushes[1][0] = uptrendColor;
                        if (PaintBars)
                        {
                            BarBrushes[0] = uptrendColor;
                            CandleOutlineBrushes[0] = uptrendColor;
                        }
                    }
                    else
                    {
                        PlotBrushes[0][0] = downtrendColor;
                        PlotBrushes[1][0] = downtrendColor;
                        if (PaintBars)
                        {
                            BarBrushes[0] = downtrendColor;
                            CandleOutlineBrushes[0] = downtrendColor;
                        }
                    }
                }
            }
            else
            {
                PlotBrushes[0][0] = chopColor;
                PlotBrushes[1][0] = chopColor;
                if (PaintBars)
                {
                    BarBrushes[0] = chopColor;
                    CandleOutlineBrushes[0] = chopColor;
                }
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LEOTRENDV402[] cacheLEOTRENDV402;
		public LEOTRENDV402 LEOTRENDV402(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double adxThreshold, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, bool enableRangeFilter)
		{
			return LEOTRENDV402(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, adxThreshold, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, enableRangeFilter);
		}

		public LEOTRENDV402 LEOTRENDV402(ISeries<double> input, int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double adxThreshold, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, bool enableRangeFilter)
		{
			if (cacheLEOTRENDV402 != null)
				for (int idx = 0; idx < cacheLEOTRENDV402.Length; idx++)
					if (cacheLEOTRENDV402[idx] != null && cacheLEOTRENDV402[idx].Period == period && cacheLEOTRENDV402[idx].ChopThreshold == chopThreshold && cacheLEOTRENDV402[idx].VolumeThreshold == volumeThreshold && cacheLEOTRENDV402[idx].VolumeSpikeThreshold == volumeSpikeThreshold && cacheLEOTRENDV402[idx].PaintBars == paintBars && cacheLEOTRENDV402[idx].SignificantHighLowLookback == significantHighLowLookback && cacheLEOTRENDV402[idx].AdxThreshold == adxThreshold && cacheLEOTRENDV402[idx].EnableTrendCalculation == enableTrendCalculation && cacheLEOTRENDV402[idx].EnableChopValue == enableChopValue && cacheLEOTRENDV402[idx].EnableSignificantVolumeDetection == enableSignificantVolumeDetection && cacheLEOTRENDV402[idx].EnableMACDColoring == enableMACDColoring && cacheLEOTRENDV402[idx].EnableVROCOverlay == enableVROCOverlay && cacheLEOTRENDV402[idx].EnableZombie9SqueezeVolume == enableZombie9SqueezeVolume && cacheLEOTRENDV402[idx].EnableRangeFilter == enableRangeFilter && cacheLEOTRENDV402[idx].EqualsInput(input))
						return cacheLEOTRENDV402[idx];
			return CacheIndicator<LEOTRENDV402>(new LEOTRENDV402(){ Period = period, ChopThreshold = chopThreshold, VolumeThreshold = volumeThreshold, VolumeSpikeThreshold = volumeSpikeThreshold, PaintBars = paintBars, SignificantHighLowLookback = significantHighLowLookback, AdxThreshold = adxThreshold, EnableTrendCalculation = enableTrendCalculation, EnableChopValue = enableChopValue, EnableSignificantVolumeDetection = enableSignificantVolumeDetection, EnableMACDColoring = enableMACDColoring, EnableVROCOverlay = enableVROCOverlay, EnableZombie9SqueezeVolume = enableZombie9SqueezeVolume, EnableRangeFilter = enableRangeFilter }, input, ref cacheLEOTRENDV402);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LEOTRENDV402 LEOTRENDV402(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double adxThreshold, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, bool enableRangeFilter)
		{
			return indicator.LEOTRENDV402(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, adxThreshold, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, enableRangeFilter);
		}

		public Indicators.LEOTRENDV402 LEOTRENDV402(ISeries<double> input , int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double adxThreshold, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, bool enableRangeFilter)
		{
			return indicator.LEOTRENDV402(input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, adxThreshold, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, enableRangeFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LEOTRENDV402 LEOTRENDV402(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double adxThreshold, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, bool enableRangeFilter)
		{
			return indicator.LEOTRENDV402(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, adxThreshold, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, enableRangeFilter);
		}

		public Indicators.LEOTRENDV402 LEOTRENDV402(ISeries<double> input , int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double adxThreshold, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, bool enableRangeFilter)
		{
			return indicator.LEOTRENDV402(input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, adxThreshold, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, enableRangeFilter);
		}
	}
}

#endregion
