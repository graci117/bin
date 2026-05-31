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
using NinjaTrader.NinjaScript.Indicators.GraciIndicators;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
    public class JBSignalATRBands : Indicator
    {
        // JBSignal Core Internals
        private MACD macd;
        private WilliamsREMA williamsREMA;
        private ALMA almaFast;
        private ALMA almaSlow;

        // ATR Band Internals
        private ATR atr;
        private Brush[] rainbowBrushes;
        
        // Signal Tracking
        private bool LastbuySignal = false;
        private bool LastsellSignal = false;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = @"JBSignal logic (MACD/WR/ALMA) with step-based ATR volatility bands.";
                Name                        = "JBSignal ATR Bands";
                Calculate                   = Calculate.OnBarClose;
                IsOverlay                   = true;
                DisplayInDataBox            = true;

                // 1. MACD Parameters
                Fast                        = 12;
                Slow                        = 26;
                Smooth                      = 9;

                // 2. WR EMA Parameters
                WilliamsRPeriod             = 21;
                WilliamsREMAPeriod          = 13;

                // 3. ALMA Parameters
                FastAlmaLength              = 19;
                SlowAlmaLength              = 20;

                // 4. ATR Band Parameters
                AtrPeriod                   = 100;
                AtrMultiplier               = 1.25;

                // Plots
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "MidlineSlowALMA");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper1");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper2");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper3");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper4");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower1");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower2");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower3");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower4");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "StrategySignal");
            }
            else if (State == State.DataLoaded)
            {
                macd            = MACD(Fast, Slow, Smooth);
                williamsREMA    = WilliamsREMA(WilliamsRPeriod, WilliamsREMAPeriod);
                almaFast        = ALMA(FastAlmaLength, 0.85, 6.0);
                almaSlow        = ALMA(SlowAlmaLength, 0.85, 6.0);
                atr             = ATR(AtrPeriod);

                rainbowBrushes = new Brush[] 
                { 
                    Brushes.DeepSkyBlue, Brushes.RoyalBlue, Brushes.BlueViolet, Brushes.Magenta, // Uppers
                    Brushes.LimeGreen, Brushes.Yellow, Brushes.Orange, Brushes.Red            // Lowers
                };
                foreach (var b in rainbowBrushes) b.Freeze();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(AtrPeriod, Math.Max(Slow, (int)SlowAlmaLength))) return;

            // --- 1. Core JBSignal Logic ---
            double macdDiff      = macd.Diff[0];
            double macdDiffPrev  = macd.Diff[1];
            double williamsValue = williamsREMA[0];
            double emaOfWR       = williamsREMA.Values[1][0];
            double fAlma         = almaFast[0];
            double sAlma         = almaSlow[0];

            Midline[0] = sAlma; 

            // Midline Slope Coloring
            if (sAlma > almaSlow[1])
                PlotBrushes[0][0] = Brushes.Lime;
            else if (sAlma < almaSlow[1])
                PlotBrushes[0][0] = Brushes.Red;
            else
                PlotBrushes[0][0] = Brushes.Gray;

            // Strategy Signal Calculation
            StrategySignal[0] = 0;
            bool buyCond  = williamsValue > emaOfWR && macdDiff > 0 && macdDiff > macdDiffPrev && Close[0] > fAlma && fAlma > sAlma && Close[0] > Open[0];
            bool sellCond = williamsValue < emaOfWR && macdDiff < 0 && macdDiff < macdDiffPrev && Close[0] < fAlma && fAlma < sAlma && Close[0] < Open[0];

            if (buyCond)
            {
                if (!LastbuySignal)
                {
                    Draw.Text(this, "BUY" + CurrentBar, "▲", 0, Low[0] - 3 * TickSize, Brushes.Lime);
                    StrategySignal[0] = 1;
                }
                LastbuySignal = true; LastsellSignal = false;
            }
            else if (sellCond)
            {
                if (!LastsellSignal)
                {
                    Draw.Text(this, "SELL" + CurrentBar, "▼", 0, High[0] + 5 * TickSize, Brushes.Orange);
                    StrategySignal[0] = -1;
                }
                LastbuySignal = false; LastsellSignal = true;
            }
            else
            {
                LastbuySignal = false; LastsellSignal = false;
            }

            // --- 2. Calculate ATR Bands in steps ---
            double valAtr = atr[0];
            Upper1[0] = sAlma + (valAtr * AtrMultiplier * 1);
            Upper2[0] = sAlma + (valAtr * AtrMultiplier * 2);
            Upper3[0] = sAlma + (valAtr * AtrMultiplier * 3);
            Upper4[0] = sAlma + (valAtr * AtrMultiplier * 4);
            Lower1[0] = sAlma - (valAtr * AtrMultiplier * 1);
            Lower2[0] = sAlma - (valAtr * AtrMultiplier * 2);
            Lower3[0] = sAlma - (valAtr * AtrMultiplier * 3);
            Lower4[0] = sAlma - (valAtr * AtrMultiplier * 4);

            // --- 3. Visual Rainbow Lines and Dots ---
            if (CurrentBar > 1)
            {
                double[] currentBands = { Upper1[0], Upper2[0], Upper3[0], Upper4[0], Lower1[0], Lower2[0], Lower3[0], Lower4[0] };
                double[] priorBands   = { Upper1[1], Upper2[1], Upper3[1], Upper4[1], Lower1[1], Lower2[1], Lower3[1], Lower4[1] };
                string[] tags         = { "U1", "U2", "U3", "U4", "L1", "L2", "L3", "L4" };

                for (int i = 0; i < 8; i++)
                {
                    Draw.Line(this, tags[i] + CurrentBar, false, 1, priorBands[i], 0, currentBands[i], rainbowBrushes[i], DashStyleHelper.Solid, 1);
                    
                    if (Close[1] <= currentBands[i] && Close[0] > currentBands[i])
                        Draw.Dot(this, "Up" + tags[i] + CurrentBar, false, 0, currentBands[i], Brushes.Cyan);
                    else if (Close[1] >= currentBands[i] && Close[0] < currentBands[i])
                        Draw.Dot(this, "Dn" + tags[i] + CurrentBar, false, 0, currentBands[i], Brushes.Yellow);
                }
            }
        }

        #region Properties
        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Fast", GroupName="1. MACD", Order=0)]
        public int Fast { get; set; }
        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Slow", GroupName="1. MACD", Order=1)]
        public int Slow { get; set; }
        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Smooth", GroupName="1. MACD", Order=2)]
        public int Smooth { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Williams %R Period", GroupName="2. WR EMA", Order=0)]
        public int WilliamsRPeriod { get; set; }
        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Williams %R Fast EMA Period", GroupName="2. WR EMA", Order=1)]
        public int WilliamsREMAPeriod { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Fast ALMA Length", GroupName="3. ALMA", Order=0)]
        public int FastAlmaLength { get; set; }
        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Slow ALMA Length", GroupName="3. ALMA", Order=1)]
        public int SlowAlmaLength { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="ATR Period", GroupName="4. ATR Bands", Order=0)]
        public int AtrPeriod { get; set; }

        [Range(0.1, 10.0), NinjaScriptProperty, Display(Name="ATR Step Multiplier", GroupName="4. ATR Bands", Order=1)]
        public double AtrMultiplier { get; set; }

        [Browsable(false)][XmlIgnore] public Series<double> Midline { get { return Values[0]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper1 { get { return Values[1]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper2 { get { return Values[2]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper3 { get { return Values[3]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper4 { get { return Values[4]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower1 { get { return Values[5]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower2 { get { return Values[6]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower3 { get { return Values[7]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower4 { get { return Values[8]; } }
        [Browsable(false)][XmlIgnore] public Series<double> StrategySignal { get { return Values[9]; } }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.JBSignalATRBands[] cacheJBSignalATRBands;
		public AlgoTrader.JBSignalATRBands JBSignalATRBands(int fast, int slow, int smooth, int williamsRPeriod, int williamsREMAPeriod, int fastAlmaLength, int slowAlmaLength, int atrPeriod, double atrMultiplier)
		{
			return JBSignalATRBands(Input, fast, slow, smooth, williamsRPeriod, williamsREMAPeriod, fastAlmaLength, slowAlmaLength, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.JBSignalATRBands JBSignalATRBands(ISeries<double> input, int fast, int slow, int smooth, int williamsRPeriod, int williamsREMAPeriod, int fastAlmaLength, int slowAlmaLength, int atrPeriod, double atrMultiplier)
		{
			if (cacheJBSignalATRBands != null)
				for (int idx = 0; idx < cacheJBSignalATRBands.Length; idx++)
					if (cacheJBSignalATRBands[idx] != null && cacheJBSignalATRBands[idx].Fast == fast && cacheJBSignalATRBands[idx].Slow == slow && cacheJBSignalATRBands[idx].Smooth == smooth && cacheJBSignalATRBands[idx].WilliamsRPeriod == williamsRPeriod && cacheJBSignalATRBands[idx].WilliamsREMAPeriod == williamsREMAPeriod && cacheJBSignalATRBands[idx].FastAlmaLength == fastAlmaLength && cacheJBSignalATRBands[idx].SlowAlmaLength == slowAlmaLength && cacheJBSignalATRBands[idx].AtrPeriod == atrPeriod && cacheJBSignalATRBands[idx].AtrMultiplier == atrMultiplier && cacheJBSignalATRBands[idx].EqualsInput(input))
						return cacheJBSignalATRBands[idx];
			return CacheIndicator<AlgoTrader.JBSignalATRBands>(new AlgoTrader.JBSignalATRBands(){ Fast = fast, Slow = slow, Smooth = smooth, WilliamsRPeriod = williamsRPeriod, WilliamsREMAPeriod = williamsREMAPeriod, FastAlmaLength = fastAlmaLength, SlowAlmaLength = slowAlmaLength, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheJBSignalATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.JBSignalATRBands JBSignalATRBands(int fast, int slow, int smooth, int williamsRPeriod, int williamsREMAPeriod, int fastAlmaLength, int slowAlmaLength, int atrPeriod, double atrMultiplier)
		{
			return indicator.JBSignalATRBands(Input, fast, slow, smooth, williamsRPeriod, williamsREMAPeriod, fastAlmaLength, slowAlmaLength, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.JBSignalATRBands JBSignalATRBands(ISeries<double> input , int fast, int slow, int smooth, int williamsRPeriod, int williamsREMAPeriod, int fastAlmaLength, int slowAlmaLength, int atrPeriod, double atrMultiplier)
		{
			return indicator.JBSignalATRBands(input, fast, slow, smooth, williamsRPeriod, williamsREMAPeriod, fastAlmaLength, slowAlmaLength, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.JBSignalATRBands JBSignalATRBands(int fast, int slow, int smooth, int williamsRPeriod, int williamsREMAPeriod, int fastAlmaLength, int slowAlmaLength, int atrPeriod, double atrMultiplier)
		{
			return indicator.JBSignalATRBands(Input, fast, slow, smooth, williamsRPeriod, williamsREMAPeriod, fastAlmaLength, slowAlmaLength, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.JBSignalATRBands JBSignalATRBands(ISeries<double> input , int fast, int slow, int smooth, int williamsRPeriod, int williamsREMAPeriod, int fastAlmaLength, int slowAlmaLength, int atrPeriod, double atrMultiplier)
		{
			return indicator.JBSignalATRBands(input, fast, slow, smooth, williamsRPeriod, williamsREMAPeriod, fastAlmaLength, slowAlmaLength, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
