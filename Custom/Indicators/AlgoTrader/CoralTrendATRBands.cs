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

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
    public class CoralTrendATRBands : Indicator
    {
        // Coral Trend Internals
        private Series<double> i1;
        private Series<double> i2;
        private Series<double> i3;
        private Series<double> i4;
        private Series<double> i5;
        private Series<double> i6;
        private Series<double> bfr;
        private double c1, c2, c3, c4, c5, cd_pow2, cd_pow3;

        // ATR Band Internals
        private ATR atr;
        private Brush[] rainbowBrushes;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = @"Coral Trend with step-based ATR volatility bands.";
                Name                        = "Coral Trend Bands";
                Calculate                   = Calculate.OnBarClose;
                IsOverlay                   = true;
                DisplayInDataBox            = true;

                // Coral Parameters
                SmoothingPeriod             = 20;
                ConstantD                   = 1.0;
                LineWidth                   = 3;

                // ATR Parameters
                AtrPeriod                   = 100;
                AtrMultiplier               = 1.25;

                // Midline Plot
                AddPlot(new Stroke(Brushes.Gray, LineWidth), PlotStyle.Line, "CoralTrend");

                // Band Plots (Transparent for data access)
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper1");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper2");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper3");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper4");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower1");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower2");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower3");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower4");
            }
            else if (State == State.Configure)
            {
                i1 = new Series<double>(this);
                i2 = new Series<double>(this);
                i3 = new Series<double>(this);
                i4 = new Series<double>(this);
                i5 = new Series<double>(this);
                i6 = new Series<double>(this);
                bfr = new Series<double>(this);
            }
            else if (State == State.DataLoaded)
            {
                // Coral Constants
                double di = (SmoothingPeriod - 1.0) / 2.0 + 1.0;
                c1 = 2.0 / (di + 1.0);
                c2 = 1.0 - c1;
                cd_pow2 = ConstantD * ConstantD;
                cd_pow3 = cd_pow2 * ConstantD;
                c3 = 3.0 * (cd_pow2 + cd_pow3);
                c4 = -3.0 * (2.0 * cd_pow2 + ConstantD + cd_pow3);
                c5 = 3.0 * ConstantD + 1.0 + cd_pow3 + 3.0 * cd_pow2;

                // ATR and Brushes
                atr = ATR(AtrPeriod);
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
            if (CurrentBar < Math.Max(SmoothingPeriod * 2, AtrPeriod)) return;

            // --- 1. Calculate Coral Trend (Midline) ---
            i1[0] = c1 * Input[0] + c2 * i1[1];
            i2[0] = c1 * i1[0]    + c2 * i2[1];
            i3[0] = c1 * i2[0]    + c2 * i3[1];
            i4[0] = c1 * i3[0]    + c2 * i4[1];
            i5[0] = c1 * i4[0]    + c2 * i5[1];
            i6[0] = c1 * i5[0]    + c2 * i6[1];

            bfr[0] = -cd_pow3 * i6[0] + c3 * i5[0] + c4 * i4[0] + c5 * i3[0];
            CoralTrendValue[0] = bfr[0];

            if (bfr[0] > bfr[1]) PlotBrushes[0][0] = Brushes.Lime;
            else if (bfr[0] < bfr[1]) PlotBrushes[0][0] = Brushes.Red;
            else PlotBrushes[0][0] = Brushes.Blue;

            // --- 2. Calculate ATR Bands in steps ---
            double valAtr = atr[0];
            double currentMid = bfr[0];

            Upper1[0] = currentMid + (valAtr * AtrMultiplier * 1);
            Upper2[0] = currentMid + (valAtr * AtrMultiplier * 2);
            Upper3[0] = currentMid + (valAtr * AtrMultiplier * 3);
            Upper4[0] = currentMid + (valAtr * AtrMultiplier * 4);
            
            Lower1[0] = currentMid - (valAtr * AtrMultiplier * 1);
            Lower2[0] = currentMid - (valAtr * AtrMultiplier * 2);
            Lower3[0] = currentMid - (valAtr * AtrMultiplier * 3);
            Lower4[0] = currentMid - (valAtr * AtrMultiplier * 4);

            // --- 3. Visual Rainbow Lines and Dots ---
            if (CurrentBar > 1)
            {
                double[] currentValues = { Upper1[0], Upper2[0], Upper3[0], Upper4[0], Lower1[0], Lower2[0], Lower3[0], Lower4[0] };
                double[] priorValues   = { Upper1[1], Upper2[1], Upper3[1], Upper4[1], Lower1[1], Lower2[1], Lower3[1], Lower4[1] };
                string[] tags          = { "U1", "U2", "U3", "U4", "L1", "L2", "L3", "L4" };

                for (int i = 0; i < 8; i++)
                {
                    Draw.Line(this, tags[i] + CurrentBar, false, 1, priorValues[i], 0, currentValues[i], rainbowBrushes[i], DashStyleHelper.Solid, 1);
                    
                    if (Close[1] <= currentValues[i] && Close[0] > currentValues[i])
                        Draw.Dot(this, "Up" + tags[i] + CurrentBar, false, 0, currentValues[i], Brushes.Cyan);
                    else if (Close[1] >= currentValues[i] && Close[0] < currentValues[i])
                        Draw.Dot(this, "Dn" + tags[i] + CurrentBar, false, 0, currentValues[i], Brushes.Yellow);
                }
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Smoothing Period", Order=1, GroupName="Coral Parameters")]
        public int SmoothingPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.0001, double.MaxValue)]
        [Display(Name="Constant D", Order=2, GroupName="Coral Parameters")]
        public double ConstantD { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Line Width", Order=3, GroupName="Visual")]
        public int LineWidth { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name="ATR Period", GroupName="Band Parameters", Order=4)]
        public int AtrPeriod { get; set; }

        [Range(0.1, 10.0), NinjaScriptProperty]
        [Display(Name="ATR Step Multiplier", GroupName="Band Parameters", Order=5)]
        public double AtrMultiplier { get; set; }

        [Browsable(false)][XmlIgnore] public Series<double> CoralTrendValue { get { return Values[0]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper1 { get { return Values[1]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper2 { get { return Values[2]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper3 { get { return Values[3]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper4 { get { return Values[4]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower1 { get { return Values[5]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower2 { get { return Values[6]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower3 { get { return Values[7]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower4 { get { return Values[8]; } }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.CoralTrendATRBands[] cacheCoralTrendATRBands;
		public AlgoTrader.CoralTrendATRBands CoralTrendATRBands(int smoothingPeriod, double constantD, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return CoralTrendATRBands(Input, smoothingPeriod, constantD, lineWidth, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.CoralTrendATRBands CoralTrendATRBands(ISeries<double> input, int smoothingPeriod, double constantD, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			if (cacheCoralTrendATRBands != null)
				for (int idx = 0; idx < cacheCoralTrendATRBands.Length; idx++)
					if (cacheCoralTrendATRBands[idx] != null && cacheCoralTrendATRBands[idx].SmoothingPeriod == smoothingPeriod && cacheCoralTrendATRBands[idx].ConstantD == constantD && cacheCoralTrendATRBands[idx].LineWidth == lineWidth && cacheCoralTrendATRBands[idx].AtrPeriod == atrPeriod && cacheCoralTrendATRBands[idx].AtrMultiplier == atrMultiplier && cacheCoralTrendATRBands[idx].EqualsInput(input))
						return cacheCoralTrendATRBands[idx];
			return CacheIndicator<AlgoTrader.CoralTrendATRBands>(new AlgoTrader.CoralTrendATRBands(){ SmoothingPeriod = smoothingPeriod, ConstantD = constantD, LineWidth = lineWidth, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheCoralTrendATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.CoralTrendATRBands CoralTrendATRBands(int smoothingPeriod, double constantD, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.CoralTrendATRBands(Input, smoothingPeriod, constantD, lineWidth, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.CoralTrendATRBands CoralTrendATRBands(ISeries<double> input , int smoothingPeriod, double constantD, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.CoralTrendATRBands(input, smoothingPeriod, constantD, lineWidth, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.CoralTrendATRBands CoralTrendATRBands(int smoothingPeriod, double constantD, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.CoralTrendATRBands(Input, smoothingPeriod, constantD, lineWidth, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.CoralTrendATRBands CoralTrendATRBands(ISeries<double> input , int smoothingPeriod, double constantD, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.CoralTrendATRBands(input, smoothingPeriod, constantD, lineWidth, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
