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
    public class LEOTRENDV4 : Indicator
    {
        #region Variables
        private int period = 9;
        private double chopThreshold = 1;
        private double volumeThreshold = 100000;
        private double volumeSpikeThreshold = 300;
        private double adxThreshold = 20;
        private double chopValue = 0.0;
        private double highestVolume = 0.0;
        private Brush chopColor = Brushes.Yellow;
        private Brush uptrendColor = Brushes.Green;
        private Brush downtrendColor = Brushes.Red;
        private Brush highVolumeUptrendColor = Brushes.Blue;
        private Brush highVolumeDowntrendColor = Brushes.Orange;
        private Brush significantHighLowColor = Brushes.Purple;
        private bool paintBars = true;
        private Series<double> volumeSeries;
        private Series<double> significantVolumeSeries;
        private Series<double> arrowSignalSeries;
        private ADX adx;
        private DM dm;
        private MACD macd;
        private OBV obv;
        private VROC vroc;
        private int significantHighLowLookback = 50;
        private double macdDiffChopThreshold = 0.67;
        private bool signalGenerated = false;
        private double obvThreshold = 100000;
        private double macdDiffUpperThreshold = 0.19;
        private double macdDiffLowerThreshold = -0.13;
        private ZiSchaffTrendCycle ziSchaffTrendCycle;
        private int trend1 = 145;
        private int trend2 = 187;
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

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enhanced indicator to identify volatile market conditions and trade signals using Unirenko bars with volume consideration.";
                Name = "LEOTRENDV4";
                Calculate = Calculate.OnEachTick;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "ChopValue");
                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "VolumeHistogram");
                AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Bar, "SignificantVolume");
                AddPlot(new Stroke(Brushes.Black, 2), PlotStyle.Bar, "ArrowSignal");
                AddLine(Brushes.Red, chopThreshold, "Threshold");
            }
            else if (State == State.DataLoaded)
            {
                volumeSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                significantVolumeSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                arrowSignalSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                adx = ADX(14);
                dm = DM(14);
                macd = MACD(12, 26, 9);
                obv = OBV();
                vroc = VROC(14, 3);
                ziSchaffTrendCycle = ZiSchaffTrendCycle(12, 26, 9, 14, 0.5);
                a1 = new Series<double>(this);
                b1 = new Series<double>(this);
                avg1 = new Series<double>(this);
                a2 = new Series<double>(this);
                b2 = new Series<double>(this);
                avg2 = new Series<double>(this);
                zombie9Squeeze = Zombie9Squeeze("Zombie9Squeeze", true, 20, 0.38, 20, 0.38, Brushes.LightGreen, Brushes.ForestGreen, Brushes.Red, Brushes.LightCoral, Brushes.DeepSkyBlue, Brushes.Transparent);
                volumeOscillator = VolumeOscillator(14, 28); // Add Volume Oscillator
            }
        }

        protected override void OnBarUpdate()
        {
            if (!EnableTrendCalculation)
                return;

            if (CurrentBar < period || CurrentBar < significantHighLowLookback)
            {
                Values[0][0] = 0;
                Values[1][0] = 0;
                Values[2][0] = 0;
                Values[3][0] = 0;
                return;
            }

            a1[0] = Input[0];
            b1[0] = Input[0];
            a2[0] = Input[0];
            b2[0] = Input[0];

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

            double volumeOscValue = volumeOscillator[0];

            if (zombie9Squeeze.IsSqueezePlot[0] == 1)
            {
                if (paintBars)
                {
                    BarBrush = chopColor;
                    CandleOutlineBrush = chopColor;
                }
                Values[3][0] = 0;
                return;
            }

            bool isUptrend = avg1[0] > avg2[0];
            bool isDowntrend = avg1[0] < avg2[0];

            if (EnableZombie9SqueezeVolume && IsFirstTickOfBar)
            {
                bool isVolumeDecreasing = zombie9Squeeze.VolumeHistogram[0] < zombie9Squeeze.VolumeHistogram[1];
                bool isPriceMakingHigherHighs = High[0] > High[1];
                bool isPriceMakingLowerLows = Low[0] < Low[1];

                if ((isDowntrend && isPriceMakingLowerLows) || (isUptrend && isPriceMakingHigherHighs))
                {
                    if (isVolumeDecreasing)
                    {
                        PlotBrushes[0][0] = isDowntrend ? (Volume[0] >= volumeThreshold ? highVolumeDowntrendColor : downtrendColor) : (Volume[0] >= volumeThreshold ? highVolumeUptrendColor : uptrendColor);
                        BarBrush = PlotBrushes[0][0];
                        CandleOutlineBrush = PlotBrushes[0][0];
                        Values[3][0] = 0;
                        return;
                    }
                }
            }

            if (EnableTrendCalculation)
            {
                a1[0] = Input[0];
                b1[0] = Input[0];
                a2[0] = Input[0];
                b2[0] = Input[0];

                if (Input[0] > a1[1]) a1[0] = Input[0];
                else a1[0] = a1[1] - (a1[1] - b1[1]) / trend1;
                if (Input[0] < b1[1]) b1[0] = Input[0];
                else b1[0] = b1[1] + (a1[1] - b1[1]) / trend1;

                avg1[0] = (a1[0] + b1[0]) / 2;

                if (Input[0] > a2[1]) a2[0] = Input[0];
                else a2[0] = a2[1] - (a2[1] - b2[1]) / trend2;
                if (Input[0] < b2[1]) b2[0] = Input[0];
                else b2[0] = b2[1] + (a2[1] - b2[1]) / trend2;

                avg2[0] = (a2[0] + b2[0]) / 2;
            }

            if (EnableChopValue)
            {
                double highestHigh = High[HighestBar(High, period)];
                double lowestLow = Low[LowestBar(Low, period)];
                double range = highestHigh - lowestLow;
                double totalMovement = 0.0;
                highestVolume = 0.0;

                for (int i = 0; i < period; i++)
                {
                    totalMovement += Math.Abs(Close[i] - Close[i + 1]);
                    if (Volume[i] > highestVolume) highestVolume = Volume[i];
                }

                chopValue = totalMovement / range;
                Values[0][0] = chopValue;
                Values[1][0] = Volume[0];
            }

            bool isChoppy = chopValue > chopThreshold && adx[0] < adxThreshold;

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
                    if (paintBars && !isChoppy)
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
                bool isFollowingTrend = (Close[0] > Open[0] && avg1[0] > avg2[0]) || (Close[0] < Open[0] && avg1[0] < avg2[0]);

                if (High[0] > rangeHigh || Low[0] < rangeLow)
                {
                    isCountingRange = false;
                    signalGenerated = false;
                }
                else if (!isFollowingTrend)
                {
                    if (paintBars)
                    {
                        BarBrush = chopColor;
                        CandleOutlineBrush = chopColor;
                    }
                }
                else if (!isChoppy)
                {
                    PlotBrushes[0][0] = PlotBrushes[1][0];
                    if (paintBars)
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

            if (!signalGenerated && EnableTrendCalculation)
            {
                if (CrossAbove(avg1, avg2, 1))
                {
                    Print("CrossAbove condition met");
                    if (volumeOscValue > 0 && High[0] > High[1])
                    {
                        Print("Up signal validated");
                        Draw.ArrowUp(this, "TrendCrossUp" + CurrentBar, true, 0, Low[0] - 2 * TickSize, uptrendColor);
                        Values[3][0] = 1;
                        signalGenerated = true;
                    }
                }
                else if (CrossBelow(avg1, avg2, 1))
                {
                    Print("CrossBelow condition met");
                    if (volumeOscValue < 0 && Low[0] < Low[1])
                    {
                        Print("Down signal validated");
                        Draw.ArrowDown(this, "TrendCrossDown" + CurrentBar, true, 0, High[0] + 2 * TickSize, downtrendColor);
                        Values[3][0] = -1;
                        signalGenerated = true;
                    }
                }
            }

            if (macd[0] - macd.Avg[0] > macdDiffUpperThreshold || macd[0] - macd.Avg[0] < macdDiffLowerThreshold)
            {
                BarBrush = chopColor;
                CandleOutlineBrush = chopColor;
            }

            // Apply the purple color if significant volume is detected
            if (Values[2][0] < 0)
            {
                BarBrush = significantHighLowColor;
                CandleOutlineBrush = significantHighLowColor;
            }
        }

        private void UpdateBarColorsBasedOnMACD()
        {
            double macdDiff = macd[0] - macd.Avg[0];
            bool isMacdDiffInRange = macdDiff <= macdDiffUpperThreshold && macdDiff >= macdDiffLowerThreshold;

            if (isMacdDiffInRange)
            {
                if (Volume[0] >= volumeThreshold)
                {
                    if (Close[0] > Open[0])
                    {
                        PlotBrushes[0][0] = highVolumeUptrendColor;
                        PlotBrushes[1][0] = highVolumeUptrendColor;
                        if (paintBars)
                        {
                            BarBrush = highVolumeUptrendColor;
                            CandleOutlineBrush = highVolumeUptrendColor;
                        }
                    }
                    else
                    {
                        PlotBrushes[0][0] = highVolumeDowntrendColor;
                        PlotBrushes[1][0] = highVolumeDowntrendColor;
                        if (paintBars)
                        {
                            BarBrush = highVolumeDowntrendColor;
                            CandleOutlineBrush = highVolumeDowntrendColor;
                        }
                    }
                }
                else
                {
                    if (Close[0] > Open[0])
                    {
                        PlotBrushes[0][0] = uptrendColor;
                        PlotBrushes[1][0] = uptrendColor;
                        if (paintBars)
                        {
                            BarBrush = uptrendColor;
                            CandleOutlineBrush = uptrendColor;
                        }
                    }
                    else
                    {
                        PlotBrushes[0][0] = downtrendColor;
                        PlotBrushes[1][0] = downtrendColor;
                        if (paintBars)
                        {
                            BarBrush = downtrendColor;
                            CandleOutlineBrush = downtrendColor;
                        }
                    }
                }
            }
            else
            {
                PlotBrushes[0][0] = chopColor;
                PlotBrushes[1][0] = chopColor;
                if (paintBars)
                {
                    BarBrush = chopColor;
                    CandleOutlineBrush = chopColor;
                }
            }
        }

        #region Properties
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
        public Series<double> SignificantVolume
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ArrowSignal
        {
            get { return Values[3]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Period", Description = "Período para calcular o indicador de chop", Order = 0, GroupName = "Parâmetros")]
        public int Period
        {
            get { return period; }
            set { period = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Chop Threshold", Description = "Limite para identificar condições de chop", Order = 1, GroupName = "Parâmetros")]
        public double ChopThreshold
        {
            get { return chopThreshold; }
            set { chopThreshold = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Volume Threshold", Description = "Limite para identificar condições de volume alto", Order = 2, GroupName = "Parâmetros")]
        public double VolumeThreshold
        {
            get { return volumeThreshold; }
            set { volumeThreshold = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Volume Spike Threshold", Description = "Limite adicional para picos de volume", Order = 3, GroupName = "Parâmetros")]
        public double VolumeSpikeThreshold
        {
            get { return volumeSpikeThreshold; }
            set { volumeSpikeThreshold = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Paint Bars", Description = "Habilitar ou desabilitar a pintura das barras com base em condições de chop", Order = 4, GroupName = "Parâmetros")]
        public bool PaintBars
        {
            get { return paintBars; }
            set { paintBars = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Significant High/Low Lookback", Description = "Período de lookback para altas/baixas significativas", Order = 5, GroupName = "Parâmetros")]
        public int SignificantHighLowLookback
        {
            get { return significantHighLowLookback; }
            set { significantHighLowLookback = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "OBV Threshold", Description = "Limite do OBV para identificar sinais falsos", Order = 6, GroupName = "Parâmetros")]
        public double ObvThreshold
        {
            get { return obvThreshold; }
            set { obvThreshold = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Enable Range Filter", Description = "Habilitar ou desabilitar o filtro de range", Order = 14, GroupName = "Parâmetros")]
        public bool EnableRangeFilter
        {
            get { return enableRangeFilter; }
            set { enableRangeFilter = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Enable Trend Calculation", Description = "Ativa/Desativa a lógica de cálculo da tendência", Order = 15, GroupName = "Filtros")]
        public bool EnableTrendCalculation { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Chop Value", Description = "Ativa/Desativa a lógica de cálculo e uso do valor de chop", Order = 16, GroupName = "Filtros")]
        public bool EnableChopValue { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Significant Volume Detection", Description = "Ativa/Desativa a lógica de detecção de altos e baixos significativos e volume significativo", Order = 17, GroupName = "Filtros")]
        public bool EnableSignificantVolumeDetection { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable MACD Coloring", Description = "Ativa/Desativa a lógica de alteração da cor das barras com base na diferença do MACD", Order = 18, GroupName = "Filtros")]
        public bool EnableMACDColoring { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable VROC Overlay", Description = "Ativa/Desativa a lógica de sobreposição de volume e tendência para evitar sinais falsos", Order = 20, GroupName = "Filtros")]
        public bool EnableVROCOverlay { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Zombie9 Squeeze Volume", Description = "Ativa/Desativa a lógica de alteração da cor das barras quando o volume do Zombie9Squeeze está diminuindo", Order = 21, GroupName = "Filtros")]
        public bool EnableZombie9SqueezeVolume { get; set; }

        [XmlIgnore]
        [Display(Name = "Chop Color", Description = "Cor para condições de chop", Order = 7, GroupName = "Cores do Gráfico")]
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
        [Display(Name = "Uptrend Color", Description = "Cor para condições de tendência de alta", Order = 8, GroupName = "Cores do Gráfico")]
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
        [Display(Name = "Downtrend Color", Description = "Cor para condições de tendência de baixa", Order = 9, GroupName = "Cores do Gráfico")]
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
        [Display(Name = "High Volume Uptrend Color", Description = "Cor para condições de tendência de alta com volume alto", Order = 10, GroupName = "Cores do Gráfico")]
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
        [Display(Name = "High Volume Downtrend Color", Description = "Cor para condições de tendência de baixa com volume alto", Order = 11, GroupName = "Cores do Gráfico")]
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

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Trend 1", Order = 12, GroupName = "Parâmetros de Tendência")]
        public int Trend1
        {
            get { return trend1; }
            set { trend1 = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Trend 2", Order = 13, GroupName = "Parâmetros de Tendência")]
        public int Trend2
        {
            get { return trend2; }
            set { trend2 = Math.Max(1, value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LEOTRENDV4[] cacheLEOTRENDV4;
		public LEOTRENDV4 LEOTRENDV4(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double obvThreshold, bool enableRangeFilter, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, int trend1, int trend2)
		{
			return LEOTRENDV4(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, obvThreshold, enableRangeFilter, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, trend1, trend2);
		}

		public LEOTRENDV4 LEOTRENDV4(ISeries<double> input, int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double obvThreshold, bool enableRangeFilter, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, int trend1, int trend2)
		{
			if (cacheLEOTRENDV4 != null)
				for (int idx = 0; idx < cacheLEOTRENDV4.Length; idx++)
					if (cacheLEOTRENDV4[idx] != null && cacheLEOTRENDV4[idx].Period == period && cacheLEOTRENDV4[idx].ChopThreshold == chopThreshold && cacheLEOTRENDV4[idx].VolumeThreshold == volumeThreshold && cacheLEOTRENDV4[idx].VolumeSpikeThreshold == volumeSpikeThreshold && cacheLEOTRENDV4[idx].PaintBars == paintBars && cacheLEOTRENDV4[idx].SignificantHighLowLookback == significantHighLowLookback && cacheLEOTRENDV4[idx].ObvThreshold == obvThreshold && cacheLEOTRENDV4[idx].EnableRangeFilter == enableRangeFilter && cacheLEOTRENDV4[idx].EnableTrendCalculation == enableTrendCalculation && cacheLEOTRENDV4[idx].EnableChopValue == enableChopValue && cacheLEOTRENDV4[idx].EnableSignificantVolumeDetection == enableSignificantVolumeDetection && cacheLEOTRENDV4[idx].EnableMACDColoring == enableMACDColoring && cacheLEOTRENDV4[idx].EnableVROCOverlay == enableVROCOverlay && cacheLEOTRENDV4[idx].EnableZombie9SqueezeVolume == enableZombie9SqueezeVolume && cacheLEOTRENDV4[idx].Trend1 == trend1 && cacheLEOTRENDV4[idx].Trend2 == trend2 && cacheLEOTRENDV4[idx].EqualsInput(input))
						return cacheLEOTRENDV4[idx];
			return CacheIndicator<LEOTRENDV4>(new LEOTRENDV4(){ Period = period, ChopThreshold = chopThreshold, VolumeThreshold = volumeThreshold, VolumeSpikeThreshold = volumeSpikeThreshold, PaintBars = paintBars, SignificantHighLowLookback = significantHighLowLookback, ObvThreshold = obvThreshold, EnableRangeFilter = enableRangeFilter, EnableTrendCalculation = enableTrendCalculation, EnableChopValue = enableChopValue, EnableSignificantVolumeDetection = enableSignificantVolumeDetection, EnableMACDColoring = enableMACDColoring, EnableVROCOverlay = enableVROCOverlay, EnableZombie9SqueezeVolume = enableZombie9SqueezeVolume, Trend1 = trend1, Trend2 = trend2 }, input, ref cacheLEOTRENDV4);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LEOTRENDV4 LEOTRENDV4(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double obvThreshold, bool enableRangeFilter, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, int trend1, int trend2)
		{
			return indicator.LEOTRENDV4(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, obvThreshold, enableRangeFilter, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, trend1, trend2);
		}

		public Indicators.LEOTRENDV4 LEOTRENDV4(ISeries<double> input , int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double obvThreshold, bool enableRangeFilter, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, int trend1, int trend2)
		{
			return indicator.LEOTRENDV4(input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, obvThreshold, enableRangeFilter, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, trend1, trend2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LEOTRENDV4 LEOTRENDV4(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double obvThreshold, bool enableRangeFilter, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, int trend1, int trend2)
		{
			return indicator.LEOTRENDV4(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, obvThreshold, enableRangeFilter, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, trend1, trend2);
		}

		public Indicators.LEOTRENDV4 LEOTRENDV4(ISeries<double> input , int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars, int significantHighLowLookback, double obvThreshold, bool enableRangeFilter, bool enableTrendCalculation, bool enableChopValue, bool enableSignificantVolumeDetection, bool enableMACDColoring, bool enableVROCOverlay, bool enableZombie9SqueezeVolume, int trend1, int trend2)
		{
			return indicator.LEOTRENDV4(input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars, significantHighLowLookback, obvThreshold, enableRangeFilter, enableTrendCalculation, enableChopValue, enableSignificantVolumeDetection, enableMACDColoring, enableVROCOverlay, enableZombie9SqueezeVolume, trend1, trend2);
		}
	}
}

#endregion
