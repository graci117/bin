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
    public class TrendSniperATRBands : Indicator
    {
        // TrendSniper Internals
        private Series<double> smooth;
        private Series<double> resultSeries;
        private Series<double> boolSeries;
        private Series<double> traSeries;
        private Series<double> atrSmoothSeries;
        private Series<double> maAtrSeries;
        private double customEmaTr;
        private int countEmaTr;

        // ATR Band Internals
        private ATR atr;
        private Brush[] rainbowBrushes;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = @"Moving Average Trend Sniper with step-based ATR volatility bands.";
                Name                        = "Trend Sniper ATR Bands";
                Calculate                   = Calculate.OnBarClose;
                IsOverlay                   = true;
                DisplayInDataBox            = true;

                // TrendSniper Parameters
                Length                      = 30;
                UpColor                     = Brushes.Lime;
                DownColor                   = Brushes.Red;
                LineWidth                   = 3;

                // ATR Parameters
                AtrPeriod                   = 100;
                AtrMultiplier               = 1.25;

                // Midline Plot (Index 0)
                AddPlot(new Stroke(Brushes.Gray, LineWidth), PlotStyle.Line, "Midline");

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
                smooth          = new Series<double>(this);
                resultSeries    = new Series<double>(this);
                boolSeries      = new Series<double>(this);
                traSeries       = new Series<double>(this);
                atrSmoothSeries = new Series<double>(this);
                maAtrSeries     = new Series<double>(this);
            }
            else if (State == State.DataLoaded)
            {
                customEmaTr = 0;
                countEmaTr  = 0;
                atr         = ATR(AtrPeriod);

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
            if (CurrentBar < Math.Max(Length * 4, AtrPeriod))
            {
                if (CurrentBar > 0) Midline[0] = Close[0];
                return;
            }

            // --- 1. Core TrendSniper Calculation ---
            double tr = Math.Max(Math.Max(High[0] - Low[0], Math.Abs(High[0] - Close[1])), Math.Abs(Low[0] - Close[1]));
            countEmaTr++;
            customEmaTr = (1 - 2.0/(countEmaTr+1)) * customEmaTr + 2.0/(countEmaTr+1) * tr;

            double slope = (Close[0] - Close[Math.Min(CurrentBar, 10)]) / (customEmaTr * 10);
            double angleDeg = Math.Atan2(slope, 1) * 180 / Math.PI;
            double raw = angleDeg > 0 ? High[0] : Low[0];
            double rawPrev = angleDeg > 0 ? High[1] : Low[1];
            traSeries[0] = (raw + rawPrev) / 2.0;

            double highest = MAX(High, Length)[0];
            double prevHigh = MAX(High, Length)[1];
            double higherH = Math.Max(Math.Sign(highest - prevHigh), 0);
            
            double lowest = MIN(Low, Length)[0];
            double prevLow = MIN(Low, Length)[1];
            double lowerL = Math.Max(Math.Sign(prevLow - lowest), 0);
            
            boolSeries[0] = (higherH > 0 || lowerL > 0) ? 1.0 : 0.0;
            double tc = Math.Pow(SMA(boolSeries, Length)[0], 2);

            smooth[0] = smooth[1] + tc * (traSeries[0] - smooth[1]);

            int wilders = Length * 4 - 1;
            atrSmoothSeries[0] = Math.Abs(smooth[1] - smooth[0]);
            maAtrSeries[0] = EMA(atrSmoothSeries, wilders)[0];
            double deltaFast = EMA(maAtrSeries, wilders)[0] * Length * 0.4;

            if (smooth[0] > resultSeries[1])
                resultSeries[0] = Math.Max(resultSeries[1], smooth[0] - deltaFast);
            else
                resultSeries[0] = Math.Min(resultSeries[1], smooth[0] + deltaFast);

            double matsValue = resultSeries[0];
            Midline[0] = matsValue;

            double sma2 = (Close[0] + Close[1]) / 2.0;
            PlotBrushes[0][0] = sma2 > matsValue ? UpColor : DownColor;

            // --- 2. Calculate ATR Bands in steps ---
            double valAtr = atr[0];

            Upper1[0] = matsValue + (valAtr * AtrMultiplier * 1);
            Upper2[0] = matsValue + (valAtr * AtrMultiplier * 2);
            Upper3[0] = matsValue + (valAtr * AtrMultiplier * 3);
            Upper4[0] = matsValue + (valAtr * AtrMultiplier * 4);
            
            Lower1[0] = matsValue - (valAtr * AtrMultiplier * 1);
            Lower2[0] = matsValue - (valAtr * AtrMultiplier * 2);
            Lower3[0] = matsValue - (valAtr * AtrMultiplier * 3);
            Lower4[0] = matsValue - (valAtr * AtrMultiplier * 4);

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
        [Display(Name="Length", Order=1, GroupName="TrendSniper Parameters")]
        public int Length { get; set; }

        [XmlIgnore]
        [Display(Name="Up Color", Order=2, GroupName="TrendSniper Parameters")]
        public Brush UpColor { get; set; }

        [Browsable(false)]
        public string UpColorSerializable
        {
            get { return UpColor.ToString(); }
            set { UpColor = (Brush)new BrushConverter().ConvertFromString(value); }
        }

        [XmlIgnore]
        [Display(Name="Down Color", Order=3, GroupName="TrendSniper Parameters")]
        public Brush DownColor { get; set; }

        [Browsable(false)]
        public string DownColorSerializable
        {
            get { return DownColor.ToString(); }
            set { DownColor = (Brush)new BrushConverter().ConvertFromString(value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Line Width", Order=4, GroupName="Visual")]
        public int LineWidth { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name="ATR Period", GroupName="Band Parameters", Order=5)]
        public int AtrPeriod { get; set; }

        [Range(0.1, 10.0), NinjaScriptProperty]
        [Display(Name="ATR Step Multiplier", GroupName="Band Parameters", Order=6)]
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
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.TrendSniperATRBands[] cacheTrendSniperATRBands;
		public AlgoTrader.TrendSniperATRBands TrendSniperATRBands(int length, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return TrendSniperATRBands(Input, length, lineWidth, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.TrendSniperATRBands TrendSniperATRBands(ISeries<double> input, int length, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			if (cacheTrendSniperATRBands != null)
				for (int idx = 0; idx < cacheTrendSniperATRBands.Length; idx++)
					if (cacheTrendSniperATRBands[idx] != null && cacheTrendSniperATRBands[idx].Length == length && cacheTrendSniperATRBands[idx].LineWidth == lineWidth && cacheTrendSniperATRBands[idx].AtrPeriod == atrPeriod && cacheTrendSniperATRBands[idx].AtrMultiplier == atrMultiplier && cacheTrendSniperATRBands[idx].EqualsInput(input))
						return cacheTrendSniperATRBands[idx];
			return CacheIndicator<AlgoTrader.TrendSniperATRBands>(new AlgoTrader.TrendSniperATRBands(){ Length = length, LineWidth = lineWidth, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheTrendSniperATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.TrendSniperATRBands TrendSniperATRBands(int length, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.TrendSniperATRBands(Input, length, lineWidth, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.TrendSniperATRBands TrendSniperATRBands(ISeries<double> input , int length, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.TrendSniperATRBands(input, length, lineWidth, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.TrendSniperATRBands TrendSniperATRBands(int length, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.TrendSniperATRBands(Input, length, lineWidth, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.TrendSniperATRBands TrendSniperATRBands(ISeries<double> input , int length, int lineWidth, int atrPeriod, double atrMultiplier)
		{
			return indicator.TrendSniperATRBands(input, length, lineWidth, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
