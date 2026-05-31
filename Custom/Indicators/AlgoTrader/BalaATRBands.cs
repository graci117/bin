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
    public class BalaATRBands : Indicator
    {
        // Bala Core Internals
        private ISeries<double> midlineSeries; 
        
        // ATR Band Internals
        private ATR atr;
        private Brush[] rainbowBrushes;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = @"Bala (EMA/SMMA) logic with step-based ATR volatility bands.";
                Name                        = "Bala ATR Bands";
                Calculate                   = Calculate.OnBarClose;
                IsOverlay                   = true;
                DisplayInDataBox            = true;

                // Bala Midline Parameters
                UseSMMA                     = true;
                EMAPeriod                   = 21;

                // ATR Band Parameters
                AtrPeriod                   = 100;
                AtrMultiplier               = 1.25;

                // Plots
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "BalaMidline");
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
                if (UseSMMA)
                    midlineSeries = SMMA(EMAPeriod);
                else
                    midlineSeries = EMA(EMAPeriod);
            }
            else if (State == State.DataLoaded)
            {
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
            if (CurrentBar < Math.Max(EMAPeriod, AtrPeriod)) return;

            // --- 1. Calculate Midline ---
            double mid = midlineSeries[0];
            double midPrev = midlineSeries[1];
            BalaValue[0] = mid;

            // Midline Slope Coloring
            if (mid > midPrev)
                PlotBrushes[0][0] = Brushes.Lime;
            else if (mid < midPrev)
                PlotBrushes[0][0] = Brushes.Red;
            else
                PlotBrushes[0][0] = Brushes.Gray;

            // --- 2. Calculate ATR Bands in steps ---
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
        [Display(Name = "Use SMMA Midline", GroupName = "1. Bala Parameters", Order = 1)]
        public bool UseSMMA { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Midline Period", GroupName = "1. Bala Parameters", Order = 2)]
        public int EMAPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Period", GroupName = "2. Band Parameters", Order = 3)]
        public int AtrPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 10.0)]
        [Display(Name = "ATR Step Multiplier", GroupName = "2. Band Parameters", Order = 4)]
        public double AtrMultiplier { get; set; }

        [Browsable(false)][XmlIgnore] public Series<double> BalaValue { get { return Values[0]; } }
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
		private AlgoTrader.BalaATRBands[] cacheBalaATRBands;
		public AlgoTrader.BalaATRBands BalaATRBands(bool useSMMA, int eMAPeriod, int atrPeriod, double atrMultiplier)
		{
			return BalaATRBands(Input, useSMMA, eMAPeriod, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.BalaATRBands BalaATRBands(ISeries<double> input, bool useSMMA, int eMAPeriod, int atrPeriod, double atrMultiplier)
		{
			if (cacheBalaATRBands != null)
				for (int idx = 0; idx < cacheBalaATRBands.Length; idx++)
					if (cacheBalaATRBands[idx] != null && cacheBalaATRBands[idx].UseSMMA == useSMMA && cacheBalaATRBands[idx].EMAPeriod == eMAPeriod && cacheBalaATRBands[idx].AtrPeriod == atrPeriod && cacheBalaATRBands[idx].AtrMultiplier == atrMultiplier && cacheBalaATRBands[idx].EqualsInput(input))
						return cacheBalaATRBands[idx];
			return CacheIndicator<AlgoTrader.BalaATRBands>(new AlgoTrader.BalaATRBands(){ UseSMMA = useSMMA, EMAPeriod = eMAPeriod, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheBalaATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.BalaATRBands BalaATRBands(bool useSMMA, int eMAPeriod, int atrPeriod, double atrMultiplier)
		{
			return indicator.BalaATRBands(Input, useSMMA, eMAPeriod, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.BalaATRBands BalaATRBands(ISeries<double> input , bool useSMMA, int eMAPeriod, int atrPeriod, double atrMultiplier)
		{
			return indicator.BalaATRBands(input, useSMMA, eMAPeriod, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.BalaATRBands BalaATRBands(bool useSMMA, int eMAPeriod, int atrPeriod, double atrMultiplier)
		{
			return indicator.BalaATRBands(Input, useSMMA, eMAPeriod, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.BalaATRBands BalaATRBands(ISeries<double> input , bool useSMMA, int eMAPeriod, int atrPeriod, double atrMultiplier)
		{
			return indicator.BalaATRBands(input, useSMMA, eMAPeriod, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
