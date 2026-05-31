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
    public class neVsSignals_v1_1 : Indicator
    {
        #region Variables and Parameters
        // Williams %R Parameters
        [Range(1, int.MaxValue)]
        [Display(Name = "WPR Length", Order = 1)]
        public int WprLength { get; set; } = 21;

        [Range(1, int.MaxValue)]
        [Display(Name = "WPR EMA Length", Order = 2)]
        public int WprEmaLength { get; set; } = 13;

        [Display(Name = "WPR Overbought", Order = 3)]
        public double WprOverbought { get; set; } = -20;

        [Display(Name = "WPR Oversold", Order = 4)]
        public double WprOversold { get; set; } = -80;

        // MACD Parameters
        [Range(1, int.MaxValue)]
        [Display(Name = "MACD Fast", Order = 5)]
        public int FastLength { get; set; } = 5;

        [Range(1, int.MaxValue)]
        [Display(Name = "MACD Slow", Order = 6)]
        public int SlowLength { get; set; } = 8;

        [Range(1, int.MaxValue)]
        [Display(Name = "Signal Length", Order = 7)]
        public int SignalLength { get; set; } = 5;

        [Display(Name = "MACD Threshold", Order = 8)]
        public double MacdThreshold { get; set; } = 0.5;

        // Range Filter Parameters
        [Range(1, int.MaxValue)]
        [Display(Name = "RF Period", Order = 9)]
        public int RfPeriod { get; set; } = 10;

        [Display(Name = "RF Multiplier", Order = 10)]
        public double RfMultiplier { get; set; } = 1.0;

        // ALMA & TEMA Parameters
        [Range(1, int.MaxValue)]
        [Display(Name = "ALMA Window", Order = 11)]
        public int AlmaWindow { get; set; } = 34;

        [Display(Name = "ALMA Offset", Order = 12)]
        public double AlmaOffset { get; set; } = 0.85;

        [Display(Name = "ALMA Sigma", Order = 13)]
        public double AlmaSigma { get; set; } = 6;

        [Range(1, int.MaxValue)]
        [Display(Name = "TEMA Length", Order = 14)]
        public int TemaLength { get; set; } = 9;

        // Series for calculations
        private Series<double> almaSeries;
        private Series<double> temaSeries;
        private Series<double> wprSeries;
        private Series<double> wprEmaSeries;
        private Series<double> filtSeries;
        private Series<double> upwardSeries;
        private Series<double> downwardSeries;
        private Series<double> macdLineSeries;
        private Series<double> signalLineSeries;
        private Series<double> histSeries;
        private Series<double> absDiffSeries;
        private Series<bool> buySignalSeries;
        private Series<bool> sellSignalSeries;

        // Bool Signals to track the *previous* bar's signal state for drawing logic
        private bool LastBuySignalState = false; // Stores the state of buySignalSeries[1] after GenerateSignals()
        private bool LastSellSignalState = false; // Stores the state of sellSignalSeries[1] after GenerateSignals()

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Composite indicator combining ALMA, TEMA, WPR, MACD, and Range Filter";
                Name = "neVsSignals_v1_1"; // Renamed
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                IsSuspendedWhileInactive = false;

                AddPlot(new Stroke(Brushes.Cyan, 1), PlotStyle.Line, "ALMA");
                AddPlot(new Stroke(Brushes.Magenta, 1), PlotStyle.Line, "TEMA");
                AddPlot(new Stroke(Brushes.Blue, 1), PlotStyle.Line, "RangeFilter");
            }
            else if (State == State.DataLoaded)
            {
                almaSeries = new Series<double>(this);
                temaSeries = new Series<double>(this);
                wprSeries = new Series<double>(this);
                wprEmaSeries = new Series<double>(this);
                filtSeries = new Series<double>(this);
                upwardSeries = new Series<double>(this);
                downwardSeries = new Series<double>(this);
                macdLineSeries = new Series<double>(this);
                signalLineSeries = new Series<double>(this);
                histSeries = new Series<double>(this);
                absDiffSeries = new Series<double>(this);
                buySignalSeries = new Series<bool>(this);
                sellSignalSeries = new Series<bool>(this);
            }
        }

        #region Core Calculations
        protected override void OnBarUpdate()
        {
            int minBarsRequired = Math.Max(AlmaWindow,
                                    Math.Max(3 * TemaLength,
                                    Math.Max(WprLength,
                                    Math.Max(SlowLength + SignalLength,
                                    RfPeriod * 2))));

            if (CurrentBar < minBarsRequired)
            {
                if (State == State.Historical || State == State.Realtime)
                {
                    Values[0][0] = 0;
                    Values[1][0] = 0;
                    Values[2][0] = 0;
                }
                return;
            }

            almaSeries[0] = CalculateAlma();
            temaSeries[0] = TEMA(Close, TemaLength)[0];
            CalculateWilliamsR();
            CalculateMacd();
            CalculateRangeFilter();

            // Calculate the current bar's signals based on conditions
            bool currentLongCondition = filtSeries[0] > filtSeries[1] &&
                                        histSeries[0] > MacdThreshold &&
                                        wprSeries[0] > WprOverbought;

            bool currentShortCondition = filtSeries[0] < filtSeries[1] &&
                                         histSeries[0] < -MacdThreshold &&
                                         wprSeries[0] < WprOversold;

            // Only set buySignalSeries[0] if it's a new signal and not contradictory
            buySignalSeries[0] = currentLongCondition && !sellSignalSeries[1]; // A new buy signal if long condition is met and no sell signal on previous bar
            sellSignalSeries[0] = currentShortCondition && !buySignalSeries[1]; // A new sell signal if short condition is met and no buy signal on previous bar

            // Ensure mutual exclusivity for the current bar's calculated signals
            if (buySignalSeries[0] && sellSignalSeries[0])
            {
                // If both conditions are met, prioritize one or set both to false.
                // For now, let's say no signal if both conditions are met simultaneously (unlikely).
                buySignalSeries[0] = false;
                sellSignalSeries[0] = false;
            }
            else if (buySignalSeries[0])
            {
                sellSignalSeries[0] = false; // Ensure no sell signal if buy is active
            }
            else if (sellSignalSeries[0])
            {
                buySignalSeries[0] = false; // Ensure no buy signal if sell is active
            }

            UpdatePlots();
            DrawSignals();

            // After all calculations and drawing, update the "Last" state for the NEXT bar's calculations
            // This is crucial: LastBuySignalState should reflect buySignalSeries[0] *after* all logic for CurrentBar
            LastBuySignalState = buySignalSeries[0];
            LastSellSignalState = sellSignalSeries[0];
        }

        private double CalculateAlma()
        {
            double sum = 0.0;
            double norm = 0.0;
            int m = (int)(AlmaOffset * (AlmaWindow - 1));
            double s = AlmaWindow / (double)AlmaSigma;

            for (int i = 0; i < AlmaWindow; i++)
            {
                double weight = Math.Exp(-((i - m) * (i - m)) / (2 * s * s));
                sum += Close[AlmaWindow - 1 - i] * weight;
                norm += weight;
            }
            return sum / norm;
        }

        private void CalculateWilliamsR()
        {
            double highest = High[HighestBar(High, WprLength)];
            double lowest = Low[LowestBar(Low, WprLength)];

            if (Math.Abs(highest - lowest) < Double.Epsilon)
            {
                wprSeries[0] = wprSeries[1];
            }
            else
            {
                wprSeries[0] = (highest - Close[0]) / (highest - lowest) * -100;
            }
            wprEmaSeries[0] = EMA(wprSeries, WprEmaLength)[0];
        }

        private void CalculateMacd()
        {
            double fastMA = EMA(Close, FastLength)[0];
            double slowMA = EMA(Close, SlowLength)[0];

            macdLineSeries[0] = fastMA - slowMA;
            signalLineSeries[0] = EMA(macdLineSeries, SignalLength)[0];
            histSeries[0] = macdLineSeries[0] - signalLineSeries[0];
        }

        private void CalculateRangeFilter()
        {
            absDiffSeries[0] = Math.Abs(Close[0] - Close[1]);

            double smrng = RfMultiplier * EMA(EMA(absDiffSeries, RfPeriod), RfPeriod * 2 - 1)[0];

            if (CurrentBar == 0)
            {
                filtSeries[0] = Close[0];
            }
            else
            {
                if (Close[0] > filtSeries[1])
                    filtSeries[0] = Math.Max(Close[0] - smrng, filtSeries[1]);
                else if (Close[0] < filtSeries[1])
                    filtSeries[0] = Math.Min(Close[0] + smrng, filtSeries[1]);
                else
                    filtSeries[0] = filtSeries[1];
            }

            upwardSeries[0] = filtSeries[0] > filtSeries[1] ? upwardSeries[1] + 1 : 0;
            downwardSeries[0] = filtSeries[0] < filtSeries[1] ? downwardSeries[1] + 1 : 0;
        }
        #endregion

        #region Signal Generation (This method is now simplified as logic is in OnBarUpdate)
        private void GenerateSignals()
        {
            // The core signal logic is now directly in OnBarUpdate
            // This method can be removed or left empty if desired,
            // as its original purpose is now fulfilled by direct assignments in OnBarUpdate.
            // Keeping it for structure, but it doesn't contain the core conditional assignment anymore.
        }
        #endregion

        #region Visualization
        private void UpdatePlots()
        {
            Values[0][0] = almaSeries[0];
            Values[1][0] = temaSeries[0];
            Values[2][0] = filtSeries[0];
        }

        private void DrawSignals()
        {
            // Only draw a buy signal if it's TRUE on the current bar AND it was FALSE on the previous bar
            // (meaning, it's a new signal)
            if (buySignalSeries[0] && !LastBuySignalState)
            {
                Draw.ArrowUp(this, "BuySignalArrow_" + CurrentBar, true, 0, Low[0] - 5 * TickSize, Brushes.Lime);
                Print("Buy signal confirmed on bar close: " + Time[0]); // Optional: Print statement
            }

            // Only draw a sell signal if it's TRUE on the current bar AND it was FALSE on the previous bar
            if (sellSignalSeries[0] && !LastSellSignalState)
            {
                Draw.ArrowDown(this, "SellSignalArrow_" + CurrentBar, true, 0, High[0] + 5 * TickSize, Brushes.Orange);
                Print("Sell signal confirmed on bar close: " + Time[0]); // Optional: Print statement
            }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private neVsSignals_v1_1[] cacheneVsSignals_v1_1;
		public neVsSignals_v1_1 neVsSignals_v1_1()
		{
			return neVsSignals_v1_1(Input);
		}

		public neVsSignals_v1_1 neVsSignals_v1_1(ISeries<double> input)
		{
			if (cacheneVsSignals_v1_1 != null)
				for (int idx = 0; idx < cacheneVsSignals_v1_1.Length; idx++)
					if (cacheneVsSignals_v1_1[idx] != null &&  cacheneVsSignals_v1_1[idx].EqualsInput(input))
						return cacheneVsSignals_v1_1[idx];
			return CacheIndicator<neVsSignals_v1_1>(new neVsSignals_v1_1(), input, ref cacheneVsSignals_v1_1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.neVsSignals_v1_1 neVsSignals_v1_1()
		{
			return indicator.neVsSignals_v1_1(Input);
		}

		public Indicators.neVsSignals_v1_1 neVsSignals_v1_1(ISeries<double> input )
		{
			return indicator.neVsSignals_v1_1(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.neVsSignals_v1_1 neVsSignals_v1_1()
		{
			return indicator.neVsSignals_v1_1(Input);
		}

		public Indicators.neVsSignals_v1_1 neVsSignals_v1_1(ISeries<double> input )
		{
			return indicator.neVsSignals_v1_1(input);
		}
	}
}

#endregion
