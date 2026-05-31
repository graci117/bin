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
	public class AuSuperSmoothFilterATRBands : Indicator
	{
        #region Variables
        private double a1 = 0, b1 = 0, c1 = 0;
        private double coeff1 = 0, coeff2 = 0, coeff3 = 0, coeff4 = 0;
        private double recurrentPart = 0;

        // ATR Band Internals
        private ATR atr;
        private Brush[] rainbowBrushes;
        #endregion

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Super Smoother Filter (2/3 Pole) with ATR-based volatility bands calculated in steps.";
				Name										= "AuSuperSmoothFilter ATR Bands";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;

                // SS Parameters
                Poles                                       = 3;
                Period                                      = 40;

                // ATR Parameters
                AtrPeriod                                   = 100;
                AtrMultiplier                               = 1.25;

                // Midline Plot
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "SuperSmoother");

                // Band Plots
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper1");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper2");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper3");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper4");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower1");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower2");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower3");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower4");
                
                // Trend Plot
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Dot, "Trend");
            }
			else if (State == State.DataLoaded)
			{
                double pi = Math.PI;
                double sq2 = Math.Sqrt(2.0);
                double sq3 = Math.Sqrt(3.0);

                if (Poles == 2)
                {
                    a1 = Math.Exp(-sq2 * pi / Period);
                    b1 = 2 * a1 * Math.Cos(sq2 * pi / Period);
                    coeff2 = b1;
                    coeff3 = -a1 * a1;
                    coeff1 = 1 - coeff2 - coeff3;
                }
                else if (Poles == 3)
                {
                    a1 = Math.Exp(-pi / Period);
                    b1 = 2 * a1 * Math.Cos(sq3 * pi / Period);
                    c1 = a1 * a1;
                    coeff2 = b1 + c1;
                    coeff3 = -(c1 + b1 * c1);
                    coeff4 = c1 * c1;
                    coeff1 = 1 - coeff2 - coeff3 - coeff4;
                }

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
            if (CurrentBar < Math.Max(Period, AtrPeriod))
                return;

            // --- 1. Calculate Super Smoother (Midline) ---
            if (CurrentBar < Poles)
            {
                SuperSmoother[0] = Input[0];
            }
            else
            {
                if (Poles == 2)
                    recurrentPart = coeff2 * SuperSmoother[1] + coeff3 * SuperSmoother[2];
                else if (Poles == 3)
                    recurrentPart = coeff2 * SuperSmoother[1] + coeff3 * SuperSmoother[2] + coeff4 * SuperSmoother[3];

                SuperSmoother[0] = recurrentPart + coeff1 * Input[0];
            }

            // Midline Slope Coloring
            if (SuperSmoother[0] > SuperSmoother[1])
            {
                PlotBrushes[0][0] = Brushes.Lime;
                Trend[0] = 1;
            }
            else if (SuperSmoother[0] < SuperSmoother[1])
            {
                PlotBrushes[0][0] = Brushes.Red;
                Trend[0] = -1;
            }
            else
            {
                PlotBrushes[0][0] = Brushes.Gray;
                Trend[0] = 0;
            }

            // --- 2. Calculate ATR Bands in steps ---
            double mid = SuperSmoother[0];
            double valAtr = atr[0];

            Upper1[0] = mid + (valAtr * AtrMultiplier * 1);
            Upper2[0] = mid + (valAtr * AtrMultiplier * 2);
            Upper3[0] = mid + (valAtr * AtrMultiplier * 3);
            Upper4[0] = mid + (valAtr * AtrMultiplier * 4);
            Lower1[0] = mid - (valAtr * AtrMultiplier * 1);
            Lower2[0] = mid - (valAtr * AtrMultiplier * 2);
            Lower3[0] = mid - (valAtr * AtrMultiplier * 3);
            Lower4[0] = mid - (valAtr * AtrMultiplier * 4);

            // --- 3. Visual Rainbow Lines and Dots ---
            if (CurrentBar > 1)
            {
                double[] curBands = { Upper1[0], Upper2[0], Upper3[0], Upper4[0], Lower1[0], Lower2[0], Lower3[0], Lower4[0] };
                double[] priBands = { Upper1[1], Upper2[1], Upper3[1], Upper4[1], Lower1[1], Lower2[1], Lower3[1], Lower4[1] };
                string[] tags = { "U1", "U2", "U3", "U4", "L1", "L2", "L3", "L4" };

                for (int i = 0; i < 8; i++)
                {
                    Draw.Line(this, tags[i] + CurrentBar, false, 1, priBands[i], 0, curBands[i], rainbowBrushes[i], DashStyleHelper.Solid, 1);
                    
                    if (Close[1] <= priBands[i] && Close[0] > curBands[i])
                        Draw.Dot(this, "Up" + tags[i] + CurrentBar, false, 0, curBands[i], Brushes.Cyan);
                    else if (Close[1] >= priBands[i] && Close[0] < curBands[i])
                        Draw.Dot(this, "Dn" + tags[i] + CurrentBar, false, 0, curBands[i], Brushes.Yellow);
                }
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(2, 3)]
        [Display(Name = "# Poles", Order = 1, GroupName = "1. SS Parameters")]
        public int Poles { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Order = 2, GroupName = "1. SS Parameters")]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Period", Order = 3, GroupName = "2. Band Parameters")]
        public int AtrPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 10.0)]
        [Display(Name = "ATR Step Multiplier", Order = 4, GroupName = "2. Band Parameters")]
        public double AtrMultiplier { get; set; }

        [Browsable(false)][XmlIgnore] public Series<double> SuperSmoother { get { return Values[0]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper1 { get { return Values[1]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper2 { get { return Values[2]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper3 { get { return Values[3]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Upper4 { get { return Values[4]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower1 { get { return Values[5]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower2 { get { return Values[6]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower3 { get { return Values[7]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Lower4 { get { return Values[8]; } }
        [Browsable(false)][XmlIgnore] public Series<double> Trend { get { return Values[9]; } }
        #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.AuSuperSmoothFilterATRBands[] cacheAuSuperSmoothFilterATRBands;
		public AlgoTrader.AuSuperSmoothFilterATRBands AuSuperSmoothFilterATRBands(int poles, int period, int atrPeriod, double atrMultiplier)
		{
			return AuSuperSmoothFilterATRBands(Input, poles, period, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.AuSuperSmoothFilterATRBands AuSuperSmoothFilterATRBands(ISeries<double> input, int poles, int period, int atrPeriod, double atrMultiplier)
		{
			if (cacheAuSuperSmoothFilterATRBands != null)
				for (int idx = 0; idx < cacheAuSuperSmoothFilterATRBands.Length; idx++)
					if (cacheAuSuperSmoothFilterATRBands[idx] != null && cacheAuSuperSmoothFilterATRBands[idx].Poles == poles && cacheAuSuperSmoothFilterATRBands[idx].Period == period && cacheAuSuperSmoothFilterATRBands[idx].AtrPeriod == atrPeriod && cacheAuSuperSmoothFilterATRBands[idx].AtrMultiplier == atrMultiplier && cacheAuSuperSmoothFilterATRBands[idx].EqualsInput(input))
						return cacheAuSuperSmoothFilterATRBands[idx];
			return CacheIndicator<AlgoTrader.AuSuperSmoothFilterATRBands>(new AlgoTrader.AuSuperSmoothFilterATRBands(){ Poles = poles, Period = period, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheAuSuperSmoothFilterATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.AuSuperSmoothFilterATRBands AuSuperSmoothFilterATRBands(int poles, int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.AuSuperSmoothFilterATRBands(Input, poles, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.AuSuperSmoothFilterATRBands AuSuperSmoothFilterATRBands(ISeries<double> input , int poles, int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.AuSuperSmoothFilterATRBands(input, poles, period, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.AuSuperSmoothFilterATRBands AuSuperSmoothFilterATRBands(int poles, int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.AuSuperSmoothFilterATRBands(Input, poles, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.AuSuperSmoothFilterATRBands AuSuperSmoothFilterATRBands(ISeries<double> input , int poles, int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.AuSuperSmoothFilterATRBands(input, poles, period, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
