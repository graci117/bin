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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class UnichoppyOFFICIAL : Indicator
    {
        #region Variables
        private double chopValue = 0.0;
        private double highestVolume = 0.0; // Highest volume in the current period
        private Brush chopColor = Brushes.Yellow;
        private Brush uptrendColor = Brushes.Green;
        private Brush downtrendColor = Brushes.Red;
        private Brush highVolumeUptrendColor = Brushes.Blue; // Color for high volume uptrend
        private Brush highVolumeDowntrendColor = Brushes.Orange; // Color for high volume downtrend
        private Brush significantHighLowColor = Brushes.Purple; // Color for significant high/low bars
        private Series<double> volumeSeries;
        private ADX adx;
        private DM dm;
        private double highestHigh;
        private double lowestLow;
        private int significantHighLowLookback = 50; 
        private double adxThreshold = 14;
        private Series<bool> isGreenAndPurple;
        private Series<bool> isRedAndPurple;
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
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enhanced Choppy Indicator tailored for Unirenko bars with volume consideration and trade signals.";
                Name = "UnichoppyOFFICIAL";
                Calculate = Calculate.OnEachTick;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true; // Plot chop value on price panel
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                Period = 9;
                ChopThreshold = 1.79;
                VolumeThreshold = 100000;
                VolumeSpikeThreshold = 500;
                PaintBars = true;

                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "ChopValue"); // Plotting chop value
                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "VolumeHistogram");
                AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Bar, "SignificantVolumeHistogram");
                AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Bar, "GreenAndPurple");
                AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Bar, "RedAndPurple");
                AddLine(Brushes.Red, ChopThreshold, "Threshold"); // Adding the threshold line for visual inspection
            }
            else if (State == State.DataLoaded)
            {
                volumeSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                adx = ADX(14);
                dm = DM(14);
                isGreenAndPurple = new Series<bool>(this, MaximumBarsLookBack.Infinite);
                isRedAndPurple = new Series<bool>(this, MaximumBarsLookBack.Infinite);
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

            Values[0][0] = chopValue; // Plotting chop value
            Values[1][0] = Volume[0]; // Update volume histogram values

            // Identify significant high/low in the high/low lookback period
            highestHigh = High[HighestBar(High, significantHighLowLookback)];
            lowestLow = Low[LowestBar(Low, significantHighLowLookback)];

            bool isSignificantHigh = High[0] >= highestHigh;
            bool isSignificantLow = Low[0] <= lowestLow;

            // Plot significant high/low below zero
            if (isSignificantHigh || isSignificantLow)
            {
                Values[2][0] = -Volume[0]; // Negative value to plot below zero
            }
            else
            {
                Values[2][0] = 0; // No significant high/low detected
            }

            // Determine if bar is green and purple or red and purple
            bool greenAndPurple = (Values[1][0] >= 300 && Values[2][0] <= -300);
            bool redAndPurple = (Values[1][0] >= 300 && Values[2][0] <= -300);
            isGreenAndPurple[0] = greenAndPurple;
            isRedAndPurple[0] = redAndPurple;
            Values[3][0] = greenAndPurple ? 1 : 0;
            Values[4][0] = redAndPurple ? 1 : 0;

            // Color bars based on chop value, volume, and trend conditions
            if (isSignificantHigh || isSignificantLow)
            {
                if (PaintBars)
                {
                    BarBrushes[0] = significantHighLowColor;
                    CandleOutlineBrushes[0] = significantHighLowColor;
                }
                PlotBrushes[2][0] = significantHighLowColor; // Set color to purple below zero
            }
            else if (chopValue > ChopThreshold)
            {
                PlotBrushes[0][0] = chopColor;
                if (PaintBars)
                {
                    BarBrushes[0] = chopColor;
                    CandleOutlineBrushes[0] = chopColor;
                }
            }
            else if (Volume[0] >= VolumeThreshold)
            {
                if (Close[0] > Open[0])
                {
                    PlotBrushes[0][0] = highVolumeUptrendColor;
                    if (PaintBars)
                    {
                        BarBrushes[0] = highVolumeUptrendColor;
                        CandleOutlineBrushes[0] = highVolumeUptrendColor;
                    }
                }
                else
                {
                    PlotBrushes[0][0] = highVolumeDowntrendColor;
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
                    if (PaintBars)
                    {
                        BarBrushes[0] = uptrendColor;
                        CandleOutlineBrushes[0] = uptrendColor;
                    }
                }
                else
                {
                    PlotBrushes[0][0] = downtrendColor;
                    if (PaintBars)
                    {
                        BarBrushes[0] = downtrendColor;
                        CandleOutlineBrushes[0] = downtrendColor;
                    }
                }
            }

            // Check for ADX and volume spike for reversal signals
            if (adx[0] > adxThreshold && Volume[0] > VolumeSpikeThreshold)
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
        [Display(Name = "Chop Color", Description = "Color for chop conditions", Order = 5, GroupName = "Plot Colors")]
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
        [Display(Name = "Uptrend Color", Description = "Color for uptrend conditions", Order = 6, GroupName = "Plot Colors")]
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
        [Display(Name = "Downtrend Color", Description = "Color for downtrend conditions", Order = 7, GroupName = "Plot Colors")]
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
        [Display(Name = "High Volume Uptrend Color", Description = "Color for high volume uptrend conditions", Order = 8, GroupName = "Plot Colors")]
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
        [Display(Name = "High Volume Downtrend Color", Description = "Color for high volume downtrend conditions", Order = 9, GroupName = "Plot Colors")]
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
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private UnichoppyOFFICIAL[] cacheUnichoppyOFFICIAL;
		public UnichoppyOFFICIAL UnichoppyOFFICIAL(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars)
		{
			return UnichoppyOFFICIAL(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars);
		}

		public UnichoppyOFFICIAL UnichoppyOFFICIAL(ISeries<double> input, int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars)
		{
			if (cacheUnichoppyOFFICIAL != null)
				for (int idx = 0; idx < cacheUnichoppyOFFICIAL.Length; idx++)
					if (cacheUnichoppyOFFICIAL[idx] != null && cacheUnichoppyOFFICIAL[idx].Period == period && cacheUnichoppyOFFICIAL[idx].ChopThreshold == chopThreshold && cacheUnichoppyOFFICIAL[idx].VolumeThreshold == volumeThreshold && cacheUnichoppyOFFICIAL[idx].VolumeSpikeThreshold == volumeSpikeThreshold && cacheUnichoppyOFFICIAL[idx].PaintBars == paintBars && cacheUnichoppyOFFICIAL[idx].EqualsInput(input))
						return cacheUnichoppyOFFICIAL[idx];
			return CacheIndicator<UnichoppyOFFICIAL>(new UnichoppyOFFICIAL(){ Period = period, ChopThreshold = chopThreshold, VolumeThreshold = volumeThreshold, VolumeSpikeThreshold = volumeSpikeThreshold, PaintBars = paintBars }, input, ref cacheUnichoppyOFFICIAL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.UnichoppyOFFICIAL UnichoppyOFFICIAL(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars)
		{
			return indicator.UnichoppyOFFICIAL(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars);
		}

		public Indicators.UnichoppyOFFICIAL UnichoppyOFFICIAL(ISeries<double> input , int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars)
		{
			return indicator.UnichoppyOFFICIAL(input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.UnichoppyOFFICIAL UnichoppyOFFICIAL(int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars)
		{
			return indicator.UnichoppyOFFICIAL(Input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars);
		}

		public Indicators.UnichoppyOFFICIAL UnichoppyOFFICIAL(ISeries<double> input , int period, double chopThreshold, double volumeThreshold, double volumeSpikeThreshold, bool paintBars)
		{
			return indicator.UnichoppyOFFICIAL(input, period, chopThreshold, volumeThreshold, volumeSpikeThreshold, paintBars);
		}
	}
}

#endregion
