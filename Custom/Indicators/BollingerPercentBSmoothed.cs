#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

public enum BollBSmoothMAType
{
    [Description("EMA")]  EMA,
    [Description("SMA")]  SMA,
    [Description("WMA")]  WMA,
    [Description("HMA")]  HMA,
    [Description("DEMA")] DEMA,
    [Description("TEMA")] TEMA
}

public enum BollBSmoothOffsetUnit
{
    [Description("StdDev")]  StdDev,
    [Description("ATR")]     ATR,
    [Description("Percent")] Percent
}

namespace NinjaTrader.NinjaScript.Indicators
{
    public class BollingerPercentBSmoothed : Indicator
    {
        private Series<double> rawPercentB;
        private Series<int>    trendStateHistory;
        private Brush          lastBrush = null;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description              = "Bollinger %B Smoothed with BPB/R signals, background and bar coloring.";
                Name                     = "Bollinger %B Smoothed";
                Calculate                = Calculate.OnBarClose;
                IsOverlay                = false;
                DisplayInDataBox         = true;
                DrawOnPricePanel         = true;
                ScaleJustification       = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                MAType                 = BollBSmoothMAType.EMA;
                Period                 = 40;
                OffsetUnit             = BollBSmoothOffsetUnit.StdDev;
                OffsetStdDevMultiplier = 2.8;
                OffsetStdDevPeriod     = 20;
                SmoothingEnabled       = true;
                SmoothingMethod        = BollBSmoothMAType.EMA;
                SmoothingPeriod        = 10;
                ThresholdOverbought    = 75;
                ThresholdOversold      = 30;
                SignalResumeSplitBars  = 3;

                BarUptrendStrong   = Brushes.LimeGreen;
                BarUptrendWeak     = Brushes.LightGreen;
                BarDowntrendStrong = Brushes.HotPink;
                BarDowntrendWeak   = Brushes.DarkRed;

                AddPlot(new Stroke(Brushes.Transparent, 0), PlotStyle.Line,   "RawPercentB");
                AddPlot(new Stroke(Brushes.DodgerBlue,  2), PlotStyle.Square, "PercentB");

                AddLine(new Stroke(Brushes.Red,  DashStyleHelper.Dash, 1), 80, "Overbought");
                AddLine(new Stroke(Brushes.Lime, DashStyleHelper.Dash, 1), 20, "Oversold");
            }
            else if (State == State.Configure)
            {
                Plots[0].AutoWidth = false;
            }
            else if (State == State.DataLoaded)
            {
                rawPercentB       = new Series<double>(this);
                trendStateHistory = new Series<int>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            int minBars = Math.Max(Period, OffsetStdDevPeriod) + SmoothingPeriod + 2;
            if (CurrentBar < minBars)
            {
                rawPercentB[0]       = 50;
                Values[0][0]         = 50;
                Values[1][0]         = 50;
                trendStateHistory[0] = 0;
                return;
            }

            // ── Basis MA ──────────────────────────────────────────────────────
            double basis = GetMA(Closes[0], MAType, Period, 0);

            // ── Band offset ───────────────────────────────────────────────────
            double offset;
            switch (OffsetUnit)
            {
                case BollBSmoothOffsetUnit.ATR:
                    offset = ATR(OffsetStdDevPeriod)[0] * OffsetStdDevMultiplier;
                    break;
                case BollBSmoothOffsetUnit.Percent:
                    offset = basis * (OffsetStdDevMultiplier / 100.0);
                    break;
                default:
                    offset = StdDev(Closes[0], OffsetStdDevPeriod)[0] * OffsetStdDevMultiplier;
                    break;
            }

            double upper = basis + offset;
            double lower = basis - offset;
            double bw    = upper - lower;

            double raw     = bw > 1e-10 ? ((Close[0] - lower) / bw) * 100.0 : 50.0;
            rawPercentB[0] = raw;
            Values[0][0]   = raw;

            Values[1][0] = SmoothingEnabled
                ? GetMA(Values[0], SmoothingMethod, SmoothingPeriod, 0)
                : raw;

            double val  = Values[1][0];
            double prev = Values[1][1];

            // ── Line coloring ─────────────────────────────────────────────────
            bool isAboveOB = val >= ThresholdOverbought;
            bool isBelowOS = val <= ThresholdOversold;
            bool isRising  = val >= prev;

            Brush newBrush;
            if (isAboveOB)
                newBrush = isRising ? BarUptrendStrong : Brushes.DarkGreen;
            else if (isBelowOS)
                newBrush = isRising ? Brushes.DarkRed  : BarDowntrendStrong;
            else
                newBrush = isRising ? BarUptrendWeak   : BarDowntrendWeak;

            if (!ReferenceEquals(newBrush, lastBrush))
            {
                if (newBrush.CanFreeze) newBrush.Freeze();
                lastBrush = newBrush;
            }
            PlotBrushes[1][0] = lastBrush;

            Lines[0].Value = ThresholdOverbought;
            Lines[1].Value = ThresholdOversold;

            // ── State machine ─────────────────────────────────────────────────
            // prevState: what zone were we tracking before this bar
            int prevState = trendStateHistory[1];

            bool prevAboveOB = prev >= ThresholdOverbought;
            bool prevBelowOS = prev <= ThresholdOversold;

            // Carry state forward by default
            int newState = prevState;

            // Only reset state when crossing into the OPPOSITE zone, not on neutral
            // e.g. if we were in uptrend (1) and now cross below OS → reset to -1 (BPB down)
            // Neutral dips keep prevState intact so R fires correctly on re-entry

            if (isAboveOB && !prevAboveOB)
            {
                // Crossing up into OB:
                // R  = we were already tracking uptrend (state==1), dipped neutral, came back
                // BPB = fresh: either from neutral-never-triggered (0) or from downtrend (-1)
                string label = (prevState == 1) ? "R" : "BPB";
                newState = 1;
                Draw.ArrowUp(this, "SIG_" + CurrentBar, true, 0,
                    Low[0] - 10 * TickSize, Brushes.Cyan);
                Draw.Text(this, "TXT_" + CurrentBar, true, label, 0,
                    Low[0] -35 * TickSize, 0,
                    Brushes.Cyan, new SimpleFont("Arial", 16), TextAlignment.Center,
                    Brushes.Transparent, Brushes.Transparent, 0);
            }
            else if (isBelowOS && !prevBelowOS)
            {
                // Crossing down into OS:
                // R  = we were already tracking downtrend (state==-1), bounced neutral, came back
                // BPB = fresh: from neutral (0) or from uptrend (1)
                string label = (prevState == -1) ? "R" : "BPB";
                newState = -1;
                Draw.ArrowDown(this, "SIG_" + CurrentBar, true, 0,
                    High[0] + 10 * TickSize, Brushes.HotPink);
                Draw.Text(this, "TXT_" + CurrentBar, true, label, 0,
                    High[0] + 35 * TickSize, 0,
                    Brushes.HotPink, new SimpleFont("Arial", 16), TextAlignment.Center,
                    Brushes.Transparent, Brushes.Transparent, 0);
            }
            else if (!isAboveOB && prevAboveOB)
            {
                // Left OB zone going DOWN — keep state==1 so next OB re-entry fires R
                // newState stays 1 (already set from carry-forward)
            }
            else if (!isBelowOS && prevBelowOS)
            {
                // Left OS zone going UP — keep state==-1 so next OS re-entry fires R
                // newState stays -1 (already set from carry-forward)
            }

            // Reset state only when crossing all the way to the opposite extreme
            // (handled naturally: crossing OB when state==-1 → BPB and sets state=1,
            //  crossing OS when state==1 → BPB and sets state=-1)

            trendStateHistory[0] = newState;

            // ── Chart background ──────────────────────────────────────────────
            if (newState == 1)
                BackBrushesAll[0] = new SolidColorBrush(Color.FromArgb(60, 50, 205, 50));
            else if (newState == -1)
                BackBrushesAll[0] = new SolidColorBrush(Color.FromArgb(60, 255, 105, 180));
            else
                BackBrushesAll[0] = null;
        }

        private double GetMA(ISeries<double> src, BollBSmoothMAType t, int p, int b)
        {
            switch (t)
            {
                case BollBSmoothMAType.SMA:  return SMA(src, p)[b];
                case BollBSmoothMAType.WMA:  return WMA(src, p)[b];
                case BollBSmoothMAType.HMA:  return HMA(src, p)[b];
                case BollBSmoothMAType.DEMA: return DEMA(src, p)[b];
                case BollBSmoothMAType.TEMA: return TEMA(src, p)[b];
                default:                     return EMA(src, p)[b];
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "MA Type", Order = 1, GroupName = "Parameters")]
        public BollBSmoothMAType MAType { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Order = 2, GroupName = "Parameters")]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Offset: Unit", Order = 3, GroupName = "Parameters")]
        public BollBSmoothOffsetUnit OffsetUnit { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, 20.0)]
        [Display(Name = "Offset: StdDev Multiplier", Order = 4, GroupName = "Parameters")]
        public double OffsetStdDevMultiplier { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Offset: StdDev Period", Order = 5, GroupName = "Parameters")]
        public int OffsetStdDevPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Smoothing: Enabled", Order = 6, GroupName = "Parameters")]
        public bool SmoothingEnabled { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Smoothing: Method", Order = 7, GroupName = "Parameters")]
        public BollBSmoothMAType SmoothingMethod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Smoothing: Period", Order = 8, GroupName = "Parameters")]
        public int SmoothingPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(50, 100)]
        [Display(Name = "Threshold: Overbought", Order = 9, GroupName = "Parameters")]
        public double ThresholdOverbought { get; set; }

        [NinjaScriptProperty]
        [Range(0, 50)]
        [Display(Name = "Threshold: Oversold", Order = 10, GroupName = "Parameters")]
        public double ThresholdOversold { get; set; }

        [NinjaScriptProperty]
        [Range(1, 50)]
        [Display(Name = "Signal Resume Split (Bars)", Order = 11, GroupName = "Parameters")]
        public int SignalResumeSplitBars { get; set; }

        [XmlIgnore]
        [Display(Name = "Plot: Uptrend Strong", Order = 1, GroupName = "Graphics")]
        public Brush BarUptrendStrong { get; set; }
        [Browsable(false)]
        public string BarUptrendStrongSerializable
        {
            get { return Serialize.BrushToString(BarUptrendStrong); }
            set { BarUptrendStrong = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Plot: Uptrend Weak", Order = 2, GroupName = "Graphics")]
        public Brush BarUptrendWeak { get; set; }
        [Browsable(false)]
        public string BarUptrendWeakSerializable
        {
            get { return Serialize.BrushToString(BarUptrendWeak); }
            set { BarUptrendWeak = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Plot: Downtrend Strong", Order = 3, GroupName = "Graphics")]
        public Brush BarDowntrendStrong { get; set; }
        [Browsable(false)]
        public string BarDowntrendStrongSerializable
        {
            get { return Serialize.BrushToString(BarDowntrendStrong); }
            set { BarDowntrendStrong = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Plot: Downtrend Weak", Order = 4, GroupName = "Graphics")]
        public Brush BarDowntrendWeak { get; set; }
        [Browsable(false)]
        public string BarDowntrendWeakSerializable
        {
            get { return Serialize.BrushToString(BarDowntrendWeak); }
            set { BarDowntrendWeak = Serialize.StringToBrush(value); }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerPercentBSmoothed[] cacheBollingerPercentBSmoothed;
		public BollingerPercentBSmoothed BollingerPercentBSmoothed(BollBSmoothMAType mAType, int period, BollBSmoothOffsetUnit offsetUnit, double offsetStdDevMultiplier, int offsetStdDevPeriod, bool smoothingEnabled, BollBSmoothMAType smoothingMethod, int smoothingPeriod, double thresholdOverbought, double thresholdOversold, int signalResumeSplitBars)
		{
			return BollingerPercentBSmoothed(Input, mAType, period, offsetUnit, offsetStdDevMultiplier, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplitBars);
		}

		public BollingerPercentBSmoothed BollingerPercentBSmoothed(ISeries<double> input, BollBSmoothMAType mAType, int period, BollBSmoothOffsetUnit offsetUnit, double offsetStdDevMultiplier, int offsetStdDevPeriod, bool smoothingEnabled, BollBSmoothMAType smoothingMethod, int smoothingPeriod, double thresholdOverbought, double thresholdOversold, int signalResumeSplitBars)
		{
			if (cacheBollingerPercentBSmoothed != null)
				for (int idx = 0; idx < cacheBollingerPercentBSmoothed.Length; idx++)
					if (cacheBollingerPercentBSmoothed[idx] != null && cacheBollingerPercentBSmoothed[idx].MAType == mAType && cacheBollingerPercentBSmoothed[idx].Period == period && cacheBollingerPercentBSmoothed[idx].OffsetUnit == offsetUnit && cacheBollingerPercentBSmoothed[idx].OffsetStdDevMultiplier == offsetStdDevMultiplier && cacheBollingerPercentBSmoothed[idx].OffsetStdDevPeriod == offsetStdDevPeriod && cacheBollingerPercentBSmoothed[idx].SmoothingEnabled == smoothingEnabled && cacheBollingerPercentBSmoothed[idx].SmoothingMethod == smoothingMethod && cacheBollingerPercentBSmoothed[idx].SmoothingPeriod == smoothingPeriod && cacheBollingerPercentBSmoothed[idx].ThresholdOverbought == thresholdOverbought && cacheBollingerPercentBSmoothed[idx].ThresholdOversold == thresholdOversold && cacheBollingerPercentBSmoothed[idx].SignalResumeSplitBars == signalResumeSplitBars && cacheBollingerPercentBSmoothed[idx].EqualsInput(input))
						return cacheBollingerPercentBSmoothed[idx];
			return CacheIndicator<BollingerPercentBSmoothed>(new BollingerPercentBSmoothed(){ MAType = mAType, Period = period, OffsetUnit = offsetUnit, OffsetStdDevMultiplier = offsetStdDevMultiplier, OffsetStdDevPeriod = offsetStdDevPeriod, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold, SignalResumeSplitBars = signalResumeSplitBars }, input, ref cacheBollingerPercentBSmoothed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerPercentBSmoothed BollingerPercentBSmoothed(BollBSmoothMAType mAType, int period, BollBSmoothOffsetUnit offsetUnit, double offsetStdDevMultiplier, int offsetStdDevPeriod, bool smoothingEnabled, BollBSmoothMAType smoothingMethod, int smoothingPeriod, double thresholdOverbought, double thresholdOversold, int signalResumeSplitBars)
		{
			return indicator.BollingerPercentBSmoothed(Input, mAType, period, offsetUnit, offsetStdDevMultiplier, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplitBars);
		}

		public Indicators.BollingerPercentBSmoothed BollingerPercentBSmoothed(ISeries<double> input , BollBSmoothMAType mAType, int period, BollBSmoothOffsetUnit offsetUnit, double offsetStdDevMultiplier, int offsetStdDevPeriod, bool smoothingEnabled, BollBSmoothMAType smoothingMethod, int smoothingPeriod, double thresholdOverbought, double thresholdOversold, int signalResumeSplitBars)
		{
			return indicator.BollingerPercentBSmoothed(input, mAType, period, offsetUnit, offsetStdDevMultiplier, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplitBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerPercentBSmoothed BollingerPercentBSmoothed(BollBSmoothMAType mAType, int period, BollBSmoothOffsetUnit offsetUnit, double offsetStdDevMultiplier, int offsetStdDevPeriod, bool smoothingEnabled, BollBSmoothMAType smoothingMethod, int smoothingPeriod, double thresholdOverbought, double thresholdOversold, int signalResumeSplitBars)
		{
			return indicator.BollingerPercentBSmoothed(Input, mAType, period, offsetUnit, offsetStdDevMultiplier, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplitBars);
		}

		public Indicators.BollingerPercentBSmoothed BollingerPercentBSmoothed(ISeries<double> input , BollBSmoothMAType mAType, int period, BollBSmoothOffsetUnit offsetUnit, double offsetStdDevMultiplier, int offsetStdDevPeriod, bool smoothingEnabled, BollBSmoothMAType smoothingMethod, int smoothingPeriod, double thresholdOverbought, double thresholdOversold, int signalResumeSplitBars)
		{
			return indicator.BollingerPercentBSmoothed(input, mAType, period, offsetUnit, offsetStdDevMultiplier, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplitBars);
		}
	}
}

#endregion
