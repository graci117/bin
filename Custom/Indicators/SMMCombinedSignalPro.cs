#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Windows.Media;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class SMMCombinedSignalPro : Indicator
    {
        private const int    MIN_BARS_REQUIRED  = 21;
        private const double ARMED_STATE_HEIGHT = 0.5;

        private SimpleMoneyMetricsMain smmMain;
        private SMMDots                smmDots;

        private Series<double> confidenceSeries;
        private double currentConfidence;
        private int    armedSignal;
        private int    lastSignalBar = -1000;

        private bool   inVirtualTrade   = false;
        private int    virtualDirection = 0;
        private double virtualEntryPrice = 0;
        
        // Anti-repaint tracking
        private int    signalLockedBar = -1;
        private int    lastDrawnSignal = 0;

        #region === Inputs ===

        [Range(1,100)]
        [NinjaScriptProperty, Display(Name="Signal Sensitivity (50=Normal)", Order=1, GroupName="Signal Settings")]
        public int SignalSensitivity { get; set; }

        [Range(0,100)]
        [NinjaScriptProperty, Display(Name="Min Confidence %", Order=2, GroupName="Signal Settings")]
        public int MinConfidenceRequired { get; set; }

        [NinjaScriptProperty, Display(Name="Require Trend Alignment", Order=3, GroupName="Signal Settings")]
        public bool RequireTrendAlignment { get; set; }

        [NinjaScriptProperty, Display(Name="Reset Armed on Reverse", Order=4, GroupName="Signal Settings")]
        public bool ResetOnReverseTrend { get; set; }

        [Range(0,20)]
        [NinjaScriptProperty, Display(Name="Min Bars Between Signals", Order=5, GroupName="Signal Settings")]
        public int MinBarsBetweenSignals { get; set; }

        [NinjaScriptProperty, Range(0.1,3.0)]
        [Display(Name="Chop Slope Threshold", Order=10, GroupName="Signal Settings")]
        public double ChopSlopeThreshold { get; set; }
        
        [NinjaScriptProperty, Display(Name="Prevent Repaint (Lock Signals)", Order=11, GroupName="Signal Settings")]
        public bool PreventRepaint { get; set; }

        // Dots params
        [Range(1, 200)]
        [NinjaScriptProperty, Display(Name="Dots Fast", Order=20, GroupName="Dots Settings")]
        public int DotsFast { get; set; }

        [Range(1, 200)]
        [NinjaScriptProperty, Display(Name="Dots Slow", Order=21, GroupName="Dots Settings")]
        public int DotsSlow { get; set; }

        #region Virtual Trade Settings
        [NinjaScriptProperty, Range(1,200)]
        [Display(Name="Target (Ticks)", Order=1, GroupName="Virtual Trade Settings")]
        public int VirtualTargetTicks { get; set; }

        [NinjaScriptProperty, Range(1,200)]
        [Display(Name="Stop (Ticks)", Order=2, GroupName="Virtual Trade Settings")]
        public int VirtualStopTicks { get; set; }
        #endregion

        #region Visual Settings
        [XmlIgnore, Display(Name="Bull Color", Order=1, GroupName="Visual Settings")]
        public Brush BullColor { get; set; }

        [XmlIgnore, Display(Name="Bear Color", Order=2, GroupName="Visual Settings")]
        public Brush BearColor { get; set; }

        [XmlIgnore, Display(Name="Confidence Color", Order=3, GroupName="Visual Settings")]
        public Brush ConfidenceColor { get; set; }

        [Display(Name="Plot Width", Order=4, GroupName="Visual Settings"), Range(1,8)]
        public int PlotWidth { get; set; }

        [Display(Name="Show Arrows", Order=5, GroupName="Visual Settings")]
        public bool ShowArrows { get; set; }

        [Display(Name="Show Confidence Text", Order=6, GroupName="Visual Settings")]
        public bool ShowConfidenceText { get; set; }

        [XmlIgnore, Display(Name="Chop Background Color", Order=7, GroupName="Visual Settings")]
        public Brush ChopBackgroundColor { get; set; }
        #endregion
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name                     = "SMM Combined Signal Pro";
                Description              = "SMM + Dots confirmation + Virtual re-arm + Chop filter + Confidence.";
                IsOverlay                = false;
                DrawOnPricePanel         = true;
                Calculate                = Calculate.OnPriceChange;
                DisplayInDataBox         = true;
                IsSuspendedWhileInactive = true;

                SignalSensitivity     = 50;
                MinConfidenceRequired = 60;
                RequireTrendAlignment = true;
                ResetOnReverseTrend   = true;
                MinBarsBetweenSignals = 1;
                ChopSlopeThreshold    = 0.6;
                PreventRepaint        = false;

                DotsFast = 10;
                DotsSlow = 25;

                VirtualTargetTicks = 40;
                VirtualStopTicks   = 80;

                BullColor           = Brushes.LimeGreen;
                BearColor           = Brushes.Red;
                ConfidenceColor     = Brushes.DodgerBlue;
                ChopBackgroundColor = Brushes.DimGray;
                PlotWidth           = 3;
                ShowArrows          = true;
                ShowConfidenceText  = true;

                AddPlot(new Stroke(Brushes.Goldenrod, PlotWidth), PlotStyle.Line, "CombinedSignal");
                AddPlot(new Stroke(ConfidenceColor, 2),             PlotStyle.Line, "Confidence");
                AddPlot(new Stroke(Brushes.Gray, 1),                 PlotStyle.Line, "ChopSlope");
                AddPlot(new Stroke(Brushes.Orange, 2),               PlotStyle.Square, "ObjectSignal");
            }
            else if (State == State.DataLoaded)
            {
                smmMain = SimpleMoneyMetricsMain(Close);
                smmDots = SMMDots(DotsFast, DotsSlow);
                confidenceSeries = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(MIN_BARS_REQUIRED, 2))
                return;

            int mainSignal = smmMain.Signal[0];

            int trendDir = (smmMain.ProfitWaveSlow[0] > smmMain.ProfitWaveSlow[1]) ? 1 :
                           (smmMain.ProfitWaveSlow[0] < smmMain.ProfitWaveSlow[1]) ? -1 : 0;

            double rawDot = smmDots[0];
            int dotsDir   = rawDot >  0.5 ?  1 :
                            rawDot < -0.5 ? -1 : 0;

            // --- Chop slope ---
            double slopeTicks = Math.Abs(smmMain.ProfitWaveSlow[0] - smmMain.ProfitWaveSlow[1]) / TickSize;
            Values[2][0]      = slopeTicks;
            PlotBrushes[2][0] = (slopeTicks < ChopSlopeThreshold) ? Brushes.DarkGray : Brushes.LightGreen;

            // Chop filter hard block
            if (slopeTicks < ChopSlopeThreshold)
            {
                var c = ((SolidColorBrush)ChopBackgroundColor).Color;
                BackBrushes[0] = new SolidColorBrush(Color.FromArgb(40, c.R, c.G, c.B));
                Value[0]       = double.NaN;
                Values[1][0]   = currentConfidence = 0;
                Values[3][0]   = double.NaN;
                armedSignal    = 0;
                
                // Remove current bar visuals if repainting
                if (!PreventRepaint)
                    RemoveCurrentBarVisuals();
                    
                return;
            }
            else
            {
                BackBrushes[0] = Brushes.Transparent;
            }

            // Confidence 0..100
            currentConfidence   = CalculateConfidence(mainSignal, trendDir, dotsDir);
            confidenceSeries[0] = currentConfidence;
            Values[1][0]        = currentConfidence;
            PlotBrushes[1][0]   = ConfidenceColor;

            double sensitivityMultiplier = SignalSensitivity / 50.0;
            double adjustedMinConfidence = MinConfidenceRequired * (2.0 - sensitivityMultiplier);
            adjustedMinConfidence        = Math.Max(0, Math.Min(100, adjustedMinConfidence));

            // Arm logic
            if (mainSignal == 1 && (!RequireTrendAlignment || trendDir == 1))
                armedSignal = 1;
            else if (mainSignal == -1 && (!RequireTrendAlignment || trendDir == -1))
                armedSignal = -1;

            if (ResetOnReverseTrend)
            {
                if (armedSignal == 1 && trendDir != 1)  armedSignal = 0;
                if (armedSignal == -1 && trendDir != -1) armedSignal = 0;
            }

            // Virtual closure => allow rearm
            if (inVirtualTrade)
            {
                double pnlTicks = (Close[0] - virtualEntryPrice) / TickSize * virtualDirection;
                if (pnlTicks >= VirtualTargetTicks || pnlTicks <= -VirtualStopTicks)
                {
                    inVirtualTrade = false;
                    armedSignal    = 0;
                    lastSignalBar  = CurrentBar - MinBarsBetweenSignals;
                }
            }

            // Anti-repaint check: if signal already locked on this bar, skip retriggering
            if (PreventRepaint && CurrentBar == signalLockedBar)
            {
                return;
            }

            // Check for full signal trigger
            bool triggerFullSignal = false;
            int  signalDirection   = 0;

            if (!inVirtualTrade && CurrentBar - lastSignalBar >= MinBarsBetweenSignals)
            {
                if (armedSignal == 1 && dotsDir == 1 && currentConfidence >= adjustedMinConfidence)
                { 
                    triggerFullSignal = true; 
                    signalDirection = 1; 
                }
                else if (armedSignal == -1 && dotsDir == -1 && currentConfidence >= adjustedMinConfidence)
                { 
                    triggerFullSignal = true; 
                    signalDirection = -1; 
                }
            }

            // Handle signal state changes - remove visuals if signal changed on current bar
            if (!PreventRepaint && CurrentBar == lastSignalBar)
            {
                // Check if signal changed from what was drawn
                if (lastDrawnSignal != 0 && !triggerFullSignal)
                {
                    // Signal disappeared - remove visuals
                    RemoveCurrentBarVisuals();
                    lastDrawnSignal = 0;
                }
                else if (triggerFullSignal && signalDirection != lastDrawnSignal)
                {
                    // Signal changed direction - remove old visuals
                    RemoveCurrentBarVisuals();
                }
            }

            // Draw full signal visuals
            if (triggerFullSignal)
            {
                Value[0]          = signalDirection;
                PlotBrushes[0][0] = (signalDirection == 1) ? BullColor : BearColor;

                lastSignalBar     = CurrentBar;
                armedSignal       = 0;
                inVirtualTrade    = true;
                virtualDirection  = signalDirection;
                virtualEntryPrice = Close[0];
                lastDrawnSignal   = signalDirection;

                if (PreventRepaint)
                {
                    signalLockedBar = CurrentBar;
                }

//                if (ShowArrows)
//                {
//                    string tag = "SMMCONF_" + CurrentBar;
//                    if (signalDirection == 1)
//                        Draw.ArrowUp(this, tag, true, 0, Low[0]  - 2 * TickSize, BullColor);
//                    else
//                        Draw.ArrowDown(this, tag, true, 0, High[0] + 2 * TickSize, BearColor);
//                }
                if (ShowArrows)
{
    string tag;
    if (signalDirection == 1)
    {
        tag = "SMMCONF_LONG_" + CurrentBar;    // ✅ Long tag for Predator ObjectMode
        Draw.ArrowUp(this, tag, false, 0, Low[0] - 2 * TickSize, BullColor);
    }
    else
    {
        tag = "SMMCONF_SHORT_" + CurrentBar;   // ✅ Short tag for Predator ObjectMode
        Draw.ArrowDown(this, tag, false, 0, High[0] + 2 * TickSize, BearColor);
    }
}

				
                // Set ObjectSignal plot (visible in Data Box)
                Values[3][0]      = signalDirection;
                PlotBrushes[3][0] = (signalDirection == 1) ? BullColor : BearColor;

                if (ShowConfidenceText)
                {
                    double atr      = ATR(14)[0];
                    double crTicks  = Math.Max(6.0, (High[0] - Low[0]) / TickSize);
                    double atrTicks = Math.Max(6.0, atr / TickSize);
                    double dynTicks = Math.Max(10.0, Math.Max(crTicks * 0.35, atrTicks * 0.60));
                    double y        = (signalDirection == 1) ? Low[0]  - dynTicks * TickSize
                                                 : High[0] + dynTicks * TickSize;

                    Brush txtBrush  = currentConfidence >= 80 ? Brushes.LimeGreen :
                                      currentConfidence >= 60 ? Brushes.Gold : Brushes.Silver;
                    var   font      = new SimpleFont("Segoe UI", 14) { Bold = true };

                    Draw.Text(this, "SMMCONF_TXT_" + CurrentBar, true,
                              string.Format("{0:F0}%", currentConfidence),
                              0, y, 0, txtBrush, font,
                              System.Windows.TextAlignment.Center,
                              Brushes.Transparent, Brushes.Transparent, 0);
                }
            }
            else if (armedSignal != 0)
            {
                Value[0]          = armedSignal * ARMED_STATE_HEIGHT;
                PlotBrushes[0][0] = (armedSignal == 1) ? Brushes.DarkGreen : Brushes.DarkRed;
                Values[3][0]      = armedSignal * ARMED_STATE_HEIGHT;
                PlotBrushes[3][0] = (armedSignal == 1) ? Brushes.DarkGreen : Brushes.DarkRed;
            }
            else
            {
                Value[0]          = double.NaN;
                PlotBrushes[0][0] = Brushes.Transparent;
                Values[3][0]      = double.NaN;
                PlotBrushes[3][0] = Brushes.Transparent;
            }
        }

//        private void RemoveCurrentBarVisuals()
//        {
//            string arrowTag = "SMMCONF_" + CurrentBar;
//            string textTag  = "SMMCONF_TXT_" + CurrentBar;
            
//            RemoveDrawObject(arrowTag);
//            RemoveDrawObject(textTag);
//        }
		private void RemoveCurrentBarVisuals()
{
    string[] tags = { 
        "SMMCONF_LONG_" + CurrentBar, 
        "SMMCONF_SHORT_" + CurrentBar, 
        "SMMCONF_TXT_" + CurrentBar 
    };

    foreach (var tag in tags)
        RemoveDrawObject(tag);
}


        private double CalculateConfidence(int sig, int trendDir, int dotsDir)
        {
            double slopeTicks = Math.Abs(smmMain.ProfitWaveSlow[0] - smmMain.ProfitWaveSlow[1]) / TickSize;
            double slopeScore = Math.Min(slopeTicks * 2.0, 40.0);

            double dotsScore  = ((sig == 1 && dotsDir == 1) || (sig == -1 && dotsDir == -1)) ? 40.0 : 0.0;

            double atr       = ATR(14)[0];
            double range     = High[0] - Low[0];
            double volScore  = Math.Min(range / Math.Max(atr, TickSize), 1.0) * 20.0;

            return Math.Min(slopeScore + dotsScore + volScore, 100.0);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SMMCombinedSignalPro[] cacheSMMCombinedSignalPro;
		public SMMCombinedSignalPro SMMCombinedSignalPro(int signalSensitivity, int minConfidenceRequired, bool requireTrendAlignment, bool resetOnReverseTrend, int minBarsBetweenSignals, double chopSlopeThreshold, bool preventRepaint, int dotsFast, int dotsSlow, int virtualTargetTicks, int virtualStopTicks)
		{
			return SMMCombinedSignalPro(Input, signalSensitivity, minConfidenceRequired, requireTrendAlignment, resetOnReverseTrend, minBarsBetweenSignals, chopSlopeThreshold, preventRepaint, dotsFast, dotsSlow, virtualTargetTicks, virtualStopTicks);
		}

		public SMMCombinedSignalPro SMMCombinedSignalPro(ISeries<double> input, int signalSensitivity, int minConfidenceRequired, bool requireTrendAlignment, bool resetOnReverseTrend, int minBarsBetweenSignals, double chopSlopeThreshold, bool preventRepaint, int dotsFast, int dotsSlow, int virtualTargetTicks, int virtualStopTicks)
		{
			if (cacheSMMCombinedSignalPro != null)
				for (int idx = 0; idx < cacheSMMCombinedSignalPro.Length; idx++)
					if (cacheSMMCombinedSignalPro[idx] != null && cacheSMMCombinedSignalPro[idx].SignalSensitivity == signalSensitivity && cacheSMMCombinedSignalPro[idx].MinConfidenceRequired == minConfidenceRequired && cacheSMMCombinedSignalPro[idx].RequireTrendAlignment == requireTrendAlignment && cacheSMMCombinedSignalPro[idx].ResetOnReverseTrend == resetOnReverseTrend && cacheSMMCombinedSignalPro[idx].MinBarsBetweenSignals == minBarsBetweenSignals && cacheSMMCombinedSignalPro[idx].ChopSlopeThreshold == chopSlopeThreshold && cacheSMMCombinedSignalPro[idx].PreventRepaint == preventRepaint && cacheSMMCombinedSignalPro[idx].DotsFast == dotsFast && cacheSMMCombinedSignalPro[idx].DotsSlow == dotsSlow && cacheSMMCombinedSignalPro[idx].VirtualTargetTicks == virtualTargetTicks && cacheSMMCombinedSignalPro[idx].VirtualStopTicks == virtualStopTicks && cacheSMMCombinedSignalPro[idx].EqualsInput(input))
						return cacheSMMCombinedSignalPro[idx];
			return CacheIndicator<SMMCombinedSignalPro>(new SMMCombinedSignalPro(){ SignalSensitivity = signalSensitivity, MinConfidenceRequired = minConfidenceRequired, RequireTrendAlignment = requireTrendAlignment, ResetOnReverseTrend = resetOnReverseTrend, MinBarsBetweenSignals = minBarsBetweenSignals, ChopSlopeThreshold = chopSlopeThreshold, PreventRepaint = preventRepaint, DotsFast = dotsFast, DotsSlow = dotsSlow, VirtualTargetTicks = virtualTargetTicks, VirtualStopTicks = virtualStopTicks }, input, ref cacheSMMCombinedSignalPro);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMMCombinedSignalPro SMMCombinedSignalPro(int signalSensitivity, int minConfidenceRequired, bool requireTrendAlignment, bool resetOnReverseTrend, int minBarsBetweenSignals, double chopSlopeThreshold, bool preventRepaint, int dotsFast, int dotsSlow, int virtualTargetTicks, int virtualStopTicks)
		{
			return indicator.SMMCombinedSignalPro(Input, signalSensitivity, minConfidenceRequired, requireTrendAlignment, resetOnReverseTrend, minBarsBetweenSignals, chopSlopeThreshold, preventRepaint, dotsFast, dotsSlow, virtualTargetTicks, virtualStopTicks);
		}

		public Indicators.SMMCombinedSignalPro SMMCombinedSignalPro(ISeries<double> input , int signalSensitivity, int minConfidenceRequired, bool requireTrendAlignment, bool resetOnReverseTrend, int minBarsBetweenSignals, double chopSlopeThreshold, bool preventRepaint, int dotsFast, int dotsSlow, int virtualTargetTicks, int virtualStopTicks)
		{
			return indicator.SMMCombinedSignalPro(input, signalSensitivity, minConfidenceRequired, requireTrendAlignment, resetOnReverseTrend, minBarsBetweenSignals, chopSlopeThreshold, preventRepaint, dotsFast, dotsSlow, virtualTargetTicks, virtualStopTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMMCombinedSignalPro SMMCombinedSignalPro(int signalSensitivity, int minConfidenceRequired, bool requireTrendAlignment, bool resetOnReverseTrend, int minBarsBetweenSignals, double chopSlopeThreshold, bool preventRepaint, int dotsFast, int dotsSlow, int virtualTargetTicks, int virtualStopTicks)
		{
			return indicator.SMMCombinedSignalPro(Input, signalSensitivity, minConfidenceRequired, requireTrendAlignment, resetOnReverseTrend, minBarsBetweenSignals, chopSlopeThreshold, preventRepaint, dotsFast, dotsSlow, virtualTargetTicks, virtualStopTicks);
		}

		public Indicators.SMMCombinedSignalPro SMMCombinedSignalPro(ISeries<double> input , int signalSensitivity, int minConfidenceRequired, bool requireTrendAlignment, bool resetOnReverseTrend, int minBarsBetweenSignals, double chopSlopeThreshold, bool preventRepaint, int dotsFast, int dotsSlow, int virtualTargetTicks, int virtualStopTicks)
		{
			return indicator.SMMCombinedSignalPro(input, signalSensitivity, minConfidenceRequired, requireTrendAlignment, resetOnReverseTrend, minBarsBetweenSignals, chopSlopeThreshold, preventRepaint, dotsFast, dotsSlow, virtualTargetTicks, virtualStopTicks);
		}
	}
}

#endregion
