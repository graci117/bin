#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Xml.Serialization;
#endregion

 public enum CustomMAType
    {
        DEMA,
        EMA,
        HMA,
        LinReg,
        SMA,
        TEMA,
        TMA,
        VWMA,
        WMA,
        WilderMA,
        ZLEMA
    }

namespace NinjaTrader.NinjaScript.Indicators
{
   

    public class BollingerBandsCustom : Indicator
    {
        private Series<double> sourceMA;
        private Series<double> smoothedUpper;
        private Series<double> smoothedLower;
        private Series<double> smoothedMiddle;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                 = "Bollinger Bands with configurable MA type and optional smoothing.";
                Name                        = "BollingerBandsCustom";
                Calculate                   = Calculate.OnBarClose;
                IsOverlay                   = true;
                DisplayInDataBox            = true;
                DrawOnPricePanel            = true;
                IsSuspendedWhileInactive    = true;

                MaType                      = CustomMAType.EMA;
                Period                      = 14;
                StdDevOffset                = 2.0;
                MinProtrusion               = 1.0;

                SmoothingEnabled            = true;
                SmoothingMaType             = CustomMAType.EMA;
                SmoothingPeriod             = 5;

                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "MiddleBand");
                AddPlot(new Stroke(Brushes.Red, 1),        PlotStyle.Line, "UpperBand");
                AddPlot(new Stroke(Brushes.Red, 1),        PlotStyle.Line, "LowerBand");
            }
            else if (State == State.DataLoaded)
            {
                sourceMA       = new Series<double>(this);
                smoothedUpper  = new Series<double>(this);
                smoothedLower  = new Series<double>(this);
                smoothedMiddle = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Period - 1)
                return;

            // --- Step 1: base MA ---
            double maValue = CalculateCustomMA(MaType, Period, Close);
            sourceMA[0] = maValue;

            // --- Step 2: StdDev ---
            double sumSq = 0;
            for (int i = 0; i < Period; i++)
                sumSq += Math.Pow(Close[i] - maValue, 2);
            double stdDev = Math.Sqrt(sumSq / Period);

            double rawUpper  = maValue + StdDevOffset * stdDev;
            double rawLower  = maValue - StdDevOffset * stdDev;
            double rawMiddle = maValue;

            smoothedUpper[0]  = rawUpper;
            smoothedLower[0]  = rawLower;
            smoothedMiddle[0] = rawMiddle;

            // --- Step 3: optional smoothing ---
            if (SmoothingEnabled && CurrentBar >= Period + SmoothingPeriod - 2)
            {
                double sUpper  = CalculateMAOnSeries(SmoothingMaType, SmoothingPeriod, smoothedUpper);
                double sLower  = CalculateMAOnSeries(SmoothingMaType, SmoothingPeriod, smoothedLower);
                double sMiddle = CalculateMAOnSeries(SmoothingMaType, SmoothingPeriod, smoothedMiddle);

                double bandwidth = Math.Abs(sUpper - sLower);
                if (MinProtrusion == 0 || bandwidth >= MinProtrusion * stdDev * 2)
                {
                    UpperBand[0]  = sUpper;
                    LowerBand[0]  = sLower;
                    MiddleBand[0] = sMiddle;
                }
                else
                {
                    UpperBand[0]  = rawUpper;
                    LowerBand[0]  = rawLower;
                    MiddleBand[0] = rawMiddle;
                }
            }
            else
            {
                UpperBand[0]  = rawUpper;
                LowerBand[0]  = rawLower;
                MiddleBand[0] = rawMiddle;
            }
        }

        #region MA Helpers

        private double CalculateCustomMA(CustomMAType type, int period, ISeries<double> input)
        {
            switch (type)
            {
                case CustomMAType.DEMA:     return DEMA(input, period)[0];
                case CustomMAType.EMA:      return EMA(input, period)[0];
                case CustomMAType.HMA:      return HMA(input, period)[0];
                case CustomMAType.LinReg:   return LinReg(input, period)[0];
                case CustomMAType.SMA:      return SMA(input, period)[0];
                case CustomMAType.TEMA:     return TEMA(input, period)[0];
                case CustomMAType.TMA:      return TMA(input, period)[0];
                case CustomMAType.VWMA:     return VWMA(input, period)[0];
                case CustomMAType.WMA:      return WMA(input, period)[0];
                case CustomMAType.WilderMA: return EMA(input, 2 * period - 1)[0];
                case CustomMAType.ZLEMA:    return ZLEMA(input, period)[0];
                default:                    return EMA(input, period)[0];
            }
        }

        private double CalculateMAOnSeries(CustomMAType type, int period, Series<double> input)
        {
            switch (type)
            {
                case CustomMAType.DEMA:     return DEMA(input, period)[0];
                case CustomMAType.EMA:      return EMA(input, period)[0];
                case CustomMAType.HMA:      return HMA(input, period)[0];
                case CustomMAType.LinReg:   return LinReg(input, period)[0];
                case CustomMAType.SMA:      return SMA(input, period)[0];
                case CustomMAType.TEMA:     return TEMA(input, period)[0];
                case CustomMAType.TMA:      return TMA(input, period)[0];
                case CustomMAType.VWMA:     return VWMA(input, period)[0];
                case CustomMAType.WMA:      return WMA(input, period)[0];
                case CustomMAType.WilderMA: return EMA(input, 2 * period - 1)[0];
                case CustomMAType.ZLEMA:    return ZLEMA(input, period)[0];
                default:                    return EMA(input, period)[0];
            }
        }

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "MA Type", Order = 1, GroupName = "Parameters")]
        public CustomMAType MaType { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Order = 2, GroupName = "Parameters")]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Smoothing: Enabled", Order = 3, GroupName = "Parameters")]
        public bool SmoothingEnabled { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Smoothing: Method", Order = 4, GroupName = "Parameters")]
        public CustomMAType SmoothingMaType { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Smoothing: Period", Order = 5, GroupName = "Parameters")]
        public int SmoothingPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Offset (StdDev)", Order = 6, GroupName = "Parameters")]
        public double StdDevOffset { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Minimum Protrusion", Order = 7, GroupName = "Parameters")]
        public double MinProtrusion { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MiddleBand => Values[0];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> UpperBand  => Values[1];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> LowerBand  => Values[2];

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerBandsCustom[] cacheBollingerBandsCustom;
		public BollingerBandsCustom BollingerBandsCustom(CustomMAType maType, int period, bool smoothingEnabled, CustomMAType smoothingMaType, int smoothingPeriod, double stdDevOffset, double minProtrusion)
		{
			return BollingerBandsCustom(Input, maType, period, smoothingEnabled, smoothingMaType, smoothingPeriod, stdDevOffset, minProtrusion);
		}

		public BollingerBandsCustom BollingerBandsCustom(ISeries<double> input, CustomMAType maType, int period, bool smoothingEnabled, CustomMAType smoothingMaType, int smoothingPeriod, double stdDevOffset, double minProtrusion)
		{
			if (cacheBollingerBandsCustom != null)
				for (int idx = 0; idx < cacheBollingerBandsCustom.Length; idx++)
					if (cacheBollingerBandsCustom[idx] != null && cacheBollingerBandsCustom[idx].MaType == maType && cacheBollingerBandsCustom[idx].Period == period && cacheBollingerBandsCustom[idx].SmoothingEnabled == smoothingEnabled && cacheBollingerBandsCustom[idx].SmoothingMaType == smoothingMaType && cacheBollingerBandsCustom[idx].SmoothingPeriod == smoothingPeriod && cacheBollingerBandsCustom[idx].StdDevOffset == stdDevOffset && cacheBollingerBandsCustom[idx].MinProtrusion == minProtrusion && cacheBollingerBandsCustom[idx].EqualsInput(input))
						return cacheBollingerBandsCustom[idx];
			return CacheIndicator<BollingerBandsCustom>(new BollingerBandsCustom(){ MaType = maType, Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMaType = smoothingMaType, SmoothingPeriod = smoothingPeriod, StdDevOffset = stdDevOffset, MinProtrusion = minProtrusion }, input, ref cacheBollingerBandsCustom);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerBandsCustom BollingerBandsCustom(CustomMAType maType, int period, bool smoothingEnabled, CustomMAType smoothingMaType, int smoothingPeriod, double stdDevOffset, double minProtrusion)
		{
			return indicator.BollingerBandsCustom(Input, maType, period, smoothingEnabled, smoothingMaType, smoothingPeriod, stdDevOffset, minProtrusion);
		}

		public Indicators.BollingerBandsCustom BollingerBandsCustom(ISeries<double> input , CustomMAType maType, int period, bool smoothingEnabled, CustomMAType smoothingMaType, int smoothingPeriod, double stdDevOffset, double minProtrusion)
		{
			return indicator.BollingerBandsCustom(input, maType, period, smoothingEnabled, smoothingMaType, smoothingPeriod, stdDevOffset, minProtrusion);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerBandsCustom BollingerBandsCustom(CustomMAType maType, int period, bool smoothingEnabled, CustomMAType smoothingMaType, int smoothingPeriod, double stdDevOffset, double minProtrusion)
		{
			return indicator.BollingerBandsCustom(Input, maType, period, smoothingEnabled, smoothingMaType, smoothingPeriod, stdDevOffset, minProtrusion);
		}

		public Indicators.BollingerBandsCustom BollingerBandsCustom(ISeries<double> input , CustomMAType maType, int period, bool smoothingEnabled, CustomMAType smoothingMaType, int smoothingPeriod, double stdDevOffset, double minProtrusion)
		{
			return indicator.BollingerBandsCustom(input, maType, period, smoothingEnabled, smoothingMaType, smoothingPeriod, stdDevOffset, minProtrusion);
		}
	}
}

#endregion
