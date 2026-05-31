using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Gui.Tools;
using System.Windows;

#region Enums

    public enum LWOttMaTypes
    {
        SMA,
        EMA,
        WMA,
        DEMA,
        TMA,
        VAR,
        WWMA,
        ZLEMA,
        TSF,
        HULL
    }

    public enum lwSmoothingModes
    {
        Kaufman,
        None
    }

    public enum lwSourceModes
    {
        Close,
        Open,
        High,
        Low,
        Median,
        Typical,
        Weighted,
        HAB_Close
    }

    public enum lwFilterOptions
    {
        None,
        Price,
        GFilter,
        Both
    }

    #endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class LWScalp : Indicator
    {
        // Core calculation series
        private Series<double>[] f_src_series;
        private Series<double>[] f_tr_series;
        private Series<double> manualTrueRange;
        private double alpha_ehlers;
        private int lag_ehlers;
        private Series<double> rawGaussianOutput;
        private Series<double> priceFiltHistory;
        private Series<double> finalFilteredOutput;
        private Series<int> contsw;
        private NinjaTrader.NinjaScript.Indicators.KAMA kama;
        private Series<double> habOpen;
        private Series<double> habClose;
        private double[,] coeffs;
        private NinjaTrader.NinjaScript.Indicators.ADX adx;
        private SimpleFont scalpTextFont;
        private SimpleFont scalpSymbolFont;

        // Range Filter (OTT) series
        private Series<double> ottHighChannel;
        private Series<double> ottLowChannel;
        private Series<double> ottLongStopH;
        private Series<double> ottShortStopH;
        private Series<double> ottLongStopL;
        private Series<double> ottShortStopL;
        private Series<int> ottDirH;
        private Series<int> ottDirL;
        private Series<double> ottVarHigh;
        private Series<double> ottVarLow;
        private Series<double> ottWwmaHigh;
        private Series<double> ottWwmaLow;
        private Series<double> ottZlemah;
        private Series<double> ottZlemal;

        // VWAP calculation
        private double iCumVolume;
        private double iCumTypicalVolume;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "LWScalp - Scalp Signal Indicator";
                Name = "LWScalp";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = false;
                PaintPriceMarkers = false;

                // Scalp Signal Settings
                EnableScalpSignals = true;
                ScalpBuySignalColor = Brushes.Cyan;
                ScalpSellSignalColor = Brushes.Khaki;
                ScalpSymbolFontSize = 22;
                ScalpTextFontSize = 12;
                ScalpSignalPixelOffset = 80;
                ScalpSignalVerticalSpacing = 14;

                // Filter Settings
                UseADXFilter = true;
                AdxPeriod = 18;
                AdxThreshold = 20;
                ShowRangeFilteredSignals = false;
                VWAPFilterSignals = false;
                ShowVWAP = true;
                TrendMagicFilterSignals = true;
                CciPeriod = 30;
                AtrPeriod = 12;
                AtrMult = 1.5;

                // Range Filter (OTT) Settings
                ShowRangeFilter = false;
                OttPeriod = 2;
                OttCoeff = 0.6;
                OttHighLowLength = 10;
                OttMaType = LWOttMaTypes.VAR;

                // Ehlers Gaussian Settings
                N = 4;
                Per = 144;
                Mult = 1.414;
                ModeLag = false;
                ModeFast = true;

                // lw Trend Tracker Settings
                SmoothingType = lwSmoothingModes.Kaufman;
                SourceOption = lwSourceModes.Close;
                Period = 11;
                Order = 2;
                FilterOption = lwFilterOptions.GFilter;
                FilterDeviations = 1.0;
                FilterPeriod = 10;
                KamaPeriod = 10;
                KamaFast = 2;
                KamaSlow = 30;
				
				Showlw0 = true;
				lwUpColor = Brushes.Lime;
				lwDownColor = Brushes.Red;
				
				// Bar Coloring Properties  
				ColorBars = true;
				BarUp2Color = Brushes.DodgerBlue;        // Bullish Breakout
				BarUp1Color = Brushes.Cyan;              // Bullish Trend
				BarUp3Color = Brushes.Transparent;       // Bullish Pullback
				BarDown2Color = Brushes.Yellow;          // Bearish Breakout
				BarDown1Color = Brushes.Khaki;           // Bearish Trend
				BarDown3Color = Brushes.Transparent;     // Bearish Pullback
				BarNeutralColor = Brushes.LightGray;
				
				TrendMagicUpColor = Brushes.Cyan;
				TrendMagicDownColor = Brushes.Yellow;

                // Add plots for internal calculations
               AddPlot(new Stroke(Brushes.Lime, 2f), PlotStyle.Line, "lw0");
				AddPlot(new Stroke(Brushes.Cyan, 2f), PlotStyle.Line, "RangeHigh");
				AddPlot(new Stroke(Brushes.Yellow, 2f), PlotStyle.Line, "RangeLow");
				AddPlot(new Stroke(Brushes.DodgerBlue, 3f), PlotStyle.Line, "VWAP");
				AddPlot(new Stroke(Brushes.Cyan, 3f), PlotStyle.Line, "lwLine");
            }
            else if (State == State.Configure)
            {
                if (EnableScalpSignals)
                {
                    scalpTextFont = new SimpleFont("Impact", ScalpTextFontSize);
                    scalpSymbolFont = new SimpleFont("Impact", ScalpSymbolFontSize);
                }
                iCumVolume = 0.0;
                iCumTypicalVolume = 0.0;
            }
            else if (State == State.DataLoaded)
            {
                if (UseADXFilter)
                    adx = ADX(AdxPeriod);

                f_src_series = new Series<double>[N > 9 ? 9 : N];
                f_tr_series = new Series<double>[N > 9 ? 9 : N];
                for (int i = 0; i < (N <= 9 ? N : 9); i++)
                {
                    f_src_series[i] = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    f_tr_series[i] = new Series<double>(this, MaximumBarsLookBack.Infinite);
                }

                manualTrueRange = new Series<double>(this, MaximumBarsLookBack.Infinite);
                
                double x = (1.0 - Math.Cos(4.0 * Math.Asin(1.0) / Per)) / (Math.Pow(1.414, 2.0 / N) - 1.0);
                alpha_ehlers = -x + Math.Sqrt(Math.Pow(x, 2.0) + 2.0 * x);
                lag_ehlers = Math.Max(1, (int)Math.Floor((Per - 1.0) / (2.0 * N)));

                priceFiltHistory = new Series<double>(this, MaximumBarsLookBack.Infinite);
                rawGaussianOutput = new Series<double>(this, MaximumBarsLookBack.Infinite);
                finalFilteredOutput = new Series<double>(this, MaximumBarsLookBack.Infinite);
                contsw = new Series<int>(this, MaximumBarsLookBack.Infinite);
                habOpen = new Series<double>(this, MaximumBarsLookBack.Infinite);
                habClose = new Series<double>(this, MaximumBarsLookBack.Infinite);

                if (Order > 0)
                    coeffs = MakeCoeffs(Period, Order);

                if (ShowRangeFilter)
                {
                    ottHighChannel = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottLowChannel = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottLongStopH = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottShortStopH = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottDirH = new Series<int>(this, MaximumBarsLookBack.Infinite);
                    ottLongStopL = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottShortStopL = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottDirL = new Series<int>(this, MaximumBarsLookBack.Infinite);
                    ottVarHigh = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottVarLow = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottWwmaHigh = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottWwmaLow = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottZlemah = new Series<double>(this, MaximumBarsLookBack.Infinite);
                    ottZlemal = new Series<double>(this, MaximumBarsLookBack.Infinite);
                }
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(CciPeriod, AtrPeriod))
            {
                lwLine[0] = 0.0;
                return;
            }

            // Calculate Trend Magic (lw line)
            double cci = CCI(Close, CciPeriod)[0];
            double atr = ATR(Close, AtrPeriod)[0];
            double longStop = Low[0] - atr * AtrMult;
            double shortStop = High[0] + atr * AtrMult;
            lwLine[0] = cci < 0.0 ? (shortStop <= lwLine[1] ? shortStop : lwLine[1]) : (longStop >= lwLine[1] ? longStop : lwLine[1]);

            // Calculate VWAP
            if (ShowVWAP)
            {
                if (Bars.IsFirstBarOfSession)
                {
                    iCumVolume = Volume[0];
                    iCumTypicalVolume = Volume[0] * ((High[0] + Low[0] + Close[0]) / 3.0);
                }
                else
                {
                    iCumVolume += Volume[0];
                    iCumTypicalVolume += Volume[0] * ((High[0] + Low[0] + Close[0]) / 3.0);
                }

                if (iCumVolume > 0.0)
                    VWAP[0] = iCumTypicalVolume / iCumVolume;
            }
            else
                VWAP[0] = double.NaN;

            if (CurrentBar <= 1)
                return;

            // Calculate Ehlers values
            double filtVal, hbandVal, lbandVal, prevLw0;
            CalculateEhlersValues(out filtVal, out hbandVal, out lbandVal);

            // Calculate lw0 (main trend line)
            double finalOut;
            CalculateStdValues(out finalOut);
            lw0[0] = finalOut;

            // Calculate Range Filter if enabled
            if (ShowRangeFilter)
                CalculateOttValues();
            else
            {
                RangeHigh[0] = double.NaN;
                RangeLow[0] = double.NaN;
            }
			
			// Update lw0 plot with dynamic coloring
			if (Showlw0 && CurrentBar > 1)
			{
			    prevLw0 = lw0[1];
			    if (lw0[0] > prevLw0)
			        PlotBrushes[0][0] = lwUpColor;
			    else if (lw0[0] < prevLw0)
			        PlotBrushes[0][0] = lwDownColor;
			    else if (CurrentBar > 0)
			        PlotBrushes[0][0] = PlotBrushes[0][1];
			}
			else if (!Showlw0)
			{
			    lw0[0] = double.NaN;
			}
			
			// Update VWAP plot with dynamic coloring
			if (ShowVWAP && VWAP.IsValidDataPoint(0))
			{
			    if (Close[0] > VWAP[0])
			        PlotBrushes[3][0] = VWAPUpColor;
			    else if (Close[0] < VWAP[0])
			        PlotBrushes[3][0] = VWAPDownColor;
			    else if (CurrentBar > 0)
			        PlotBrushes[3][0] = PlotBrushes[3][1];
			}
			
			// Update lwLine (Trend Magic) plot with dynamic coloring
			if (lwLine.IsValidDataPoint(0) && CurrentBar > 0)
			{
			    if (lwLine[0] > lwLine[1])
			        PlotBrushes[4][0] = TrendMagicUpColor;
			    else if (lwLine[0] < lwLine[1])
			        PlotBrushes[4][0] = TrendMagicDownColor;
			    else
			        PlotBrushes[4][0] = PlotBrushes[4][1];
			}
			
			// Bar Coloring Logic
			if (ColorBars && CurrentBar > 1)
			{
			    //double filtVal, hbandVal, lbandVal;
			    CalculateEhlersValues(out filtVal, out hbandVal, out lbandVal);
			    
			    if (Input[0] > Input[1] && Input[0] >= hbandVal)
			        BarBrush = BarUp2Color;
			    else if (Input[0] > Input[1] && Input[0] > filtVal && Input[0] < hbandVal)
			        BarBrush = BarUp1Color;
			    else if (Input[0] <= Input[1] && Input[0] > filtVal)
			        BarBrush = BarUp3Color;
			    else if (Input[0] < Input[1] && Input[0] <= lbandVal)
			        BarBrush = BarDown2Color;
			    else if (Input[0] < Input[1] && Input[0] < filtVal && Input[0] > lbandVal)
			        BarBrush = BarDown1Color;
			    else if (Input[0] >= Input[1] && Input[0] < filtVal)
			        BarBrush = BarDown3Color;
			    else
			        BarBrush = BarNeutralColor;
			}


            if (CurrentBar <= 2 || !EnableScalpSignals)
                return;

            // Scalp Signal Logic
             prevLw0 = lw0[1];
            bool bullishReversal = lw0[0] > prevLw0 && lw0.IsValidDataPoint(1) && lw0[1] <= lw0[2];
            bool bearishReversal = lw0[0] < prevLw0 && lw0.IsValidDataPoint(1) && lw0[1] >= lw0[2];

            contsw[0] = bullishReversal ? 1 : (bearishReversal ? -1 : contsw[1]);

            bool scalpLong = bullishReversal && contsw[1] == -1;
            bool scalpShort = bearishReversal && contsw[1] == 1;

            // Bar condition checks
            bool strongBullBar = Input[0] > Input[1] && Input[0] >= hbandVal;
            bool moderateBullBar = Input[0] > Input[1] && Input[0] > filtVal && Input[0] < hbandVal;

            if (scalpLong && !(strongBullBar || moderateBullBar))
                scalpLong = false;

            bool strongBearBar = Input[0] < Input[1] && Input[0] <= lbandVal;
            bool moderateBearBar = Input[0] < Input[1] && Input[0] < filtVal && Input[0] > lbandVal;

            if (scalpShort && !(strongBearBar || moderateBearBar))
                scalpShort = false;

            // Apply ADX Filter
            if (UseADXFilter && adx != null && adx.IsValidDataPoint(0) && adx.Value[0] < AdxThreshold)
            {
                scalpLong = false;
                scalpShort = false;
            }

            // Apply Range Filter
            if (ShowRangeFilteredSignals)
            {
                if (ShowRangeFilter && RangeHigh.IsValidDataPoint(2) && RangeLow.IsValidDataPoint(2))
                {
                    if (scalpLong && Close[0] < RangeHigh[2])
                        scalpLong = false;
                    if (scalpShort && Close[0] > RangeLow[2])
                        scalpShort = false;
                }
                else
                {
                    scalpLong = false;
                    scalpShort = false;
                }
            }

            // Apply VWAP Filter
            if (VWAPFilterSignals)
            {
                if (ShowVWAP && VWAP.IsValidDataPoint(0))
                {
                    if (scalpLong && Close[0] < VWAP[0])
                        scalpLong = false;
                    if (scalpShort && Close[0] > VWAP[0])
                        scalpShort = false;
                }
                else
                {
                    scalpLong = false;
                    scalpShort = false;
                }
            }

            // Apply Trend Magic Filter
            if (TrendMagicFilterSignals)
            {
                if (lwLine.IsValidDataPoint(0))
                {
                    if (scalpLong && Close[0] < lwLine[0])
                        scalpLong = false;
                    if (scalpShort && Close[0] > lwLine[0])
                        scalpShort = false;
                }
                else
                {
                    scalpLong = false;
                    scalpShort = false;
                }
            }

            // Draw scalp signals
            if (scalpLong)
            {
                Draw.Text(this, "ScalpLongSymbol" + CurrentBar.ToString(), true, "\uD83D\uDDF2", 0, Low[0], 
                    -ScalpSignalPixelOffset, ScalpBuySignalColor, scalpSymbolFont, 
                    TextAlignment.Center, null, null, 0);
                Draw.Text(this, "ScalpLongText" + CurrentBar.ToString(), true, "SCALP", 0, Low[0], 
                    -ScalpSignalPixelOffset - 5 - ScalpSignalVerticalSpacing, ScalpBuySignalColor, 
                    scalpTextFont, TextAlignment.Center, null, null, 0);
            }

            if (scalpShort)
            {
                Draw.Text(this, "ScalpShortText" + CurrentBar.ToString(), true, "SCALP", 0, High[0], 
                    ScalpSignalPixelOffset, ScalpSellSignalColor, scalpTextFont, 
                    TextAlignment.Center, null, null, 0);
                Draw.Text(this, "ScalpShortSymbol" + CurrentBar.ToString(), true, "\uD83D\uDDF2", 0, High[0], 
                    ScalpSignalPixelOffset - ScalpSignalVerticalSpacing, ScalpSellSignalColor, 
                    scalpSymbolFont, TextAlignment.Center, null, null, 0);
            }
        }

        #region Helper Methods

        private void CalculateEhlersValues(out double filtVal, out double hbandVal, out double lbandVal)
        {
            filtVal = double.NaN;
            hbandVal = double.NaN;
            lbandVal = double.NaN;

            if (CurrentBar < Math.Max(lag_ehlers, N))
                return;

            double src = Input[0];
            double srcLag = Input[lag_ehlers];

            manualTrueRange[0] = Math.Max(High[0] - Low[0], 
                Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Low[0] - Close[1])));

            double tr = manualTrueRange[0];
            double _s1 = ModeLag ? src + (src - srcLag) : src;
            double _s2 = ModeLag ? tr + (tr - manualTrueRange[lag_ehlers]) : tr;

            double[] filteredSrc = new double[N];
            double[] filteredTr = new double[N];

            for (int i = 1; i <= N; i++)
            {
                double filterSrc = CalculateFilter(alpha_ehlers, _s1, i, f_src_series[i - 1]);
                f_src_series[i - 1][0] = filterSrc;
                filteredSrc[i - 1] = filterSrc;

                double filterTr = CalculateFilter(alpha_ehlers, _s2, i, f_tr_series[i - 1]);
                f_tr_series[i - 1][0] = filterTr;
                filteredTr[i - 1] = filterTr;
            }

            double finalSrc = filteredSrc[N - 1];
            double firstSrc = filteredSrc[0];
            double finalTr = filteredTr[N - 1];
            double firstTr = filteredTr[0];

            double avgSrc = ModeFast ? (finalSrc + firstSrc) / 2.0 : finalSrc;
            double avgTr = ModeFast ? (finalTr + firstTr) / 2.0 : finalTr;

            filtVal = avgSrc;
            hbandVal = avgSrc + avgTr * Mult;
            lbandVal = avgSrc - avgTr * Mult;
        }

        private void CalculateStdValues(out double finalOut)
        {
            finalOut = Input[0];

            if (kama == null)
                kama = KAMA(Input, KamaFast, KamaPeriod, KamaSlow);

            if (CurrentBar < Period)
                return;

            double src1 = GetSourceValue();

            if ((FilterOption == lwFilterOptions.Both || FilterOption == lwFilterOptions.Price) && 
                FilterDeviations > 0.0 && FilterPeriod > 0)
                src1 = StdFilt(src1, priceFiltHistory, StdDev(Inputs[0], FilterPeriod)[0]);

            priceFiltHistory[0] = src1;

            double src2 = NpoleGF(src1, rawGaussianOutput);
            rawGaussianOutput[0] = src2;

            double result = src2;
            if ((FilterOption == lwFilterOptions.Both || FilterOption == lwFilterOptions.GFilter) && 
                FilterDeviations > 0.0 && FilterPeriod > 0)
                result = StdFilt(src2, finalFilteredOutput, StdDev(rawGaussianOutput, FilterPeriod)[0]);

            finalFilteredOutput[0] = result;
            finalOut = result;
        }

        private double CalculateFilter(double _a, double _s, int _i, Series<double> _f)
        {
            if (CurrentBar < _i)
                return _s;

            double x = 1.0 - _a;
            double c1 = 0.0, c2 = 0.0, c3 = 0.0, c4 = 0.0, c5 = 0.0, c6 = 0.0, c7 = 0.0, c8 = 0.0;

            if (_i >= 2)
                c1 = _i == 9 ? 36.0 : (_i == 8 ? 28.0 : (_i == 7 ? 21.0 : (_i == 6 ? 15.0 : 
                     (_i == 5 ? 10.0 : (_i == 4 ? 6.0 : (_i == 3 ? 3.0 : (_i == 2 ? 1 : 0)))))));
            if (_i >= 3)
                c2 = _i == 9 ? 84.0 : (_i == 8 ? 56.0 : (_i == 7 ? 35.0 : (_i == 6 ? 20.0 : 
                     (_i == 5 ? 10.0 : (_i == 4 ? 4.0 : (_i == 3 ? 1 : 0))))));
            if (_i >= 4)
                c3 = _i == 9 ? 126.0 : (_i == 8 ? 70.0 : (_i == 7 ? 35.0 : (_i == 6 ? 15.0 : 
                     (_i == 5 ? 5.0 : (_i == 4 ? 1 : 0)))));
            if (_i >= 5)
                c4 = _i == 9 ? 126.0 : (_i == 8 ? 56.0 : (_i == 7 ? 21.0 : (_i == 6 ? 6.0 : 
                     (_i == 5 ? 1 : 0))));
            if (_i >= 6)
                c5 = _i == 9 ? 84.0 : (_i == 8 ? 28.0 : (_i == 7 ? 7.0 : (_i == 6 ? 1 : 0)));
            if (_i >= 7)
                c6 = _i == 9 ? 36.0 : (_i == 8 ? 8.0 : (_i == 7 ? 1 : 0));
            if (_i >= 8)
                c7 = _i == 9 ? 9.0 : (_i == 8 ? 1 : 0);
            if (_i == 9)
                c8 = 1.0;

            return Math.Pow(_a, _i) * _s + _i * x * _f[1] - 
                   (_i >= 2 ? c1 * Math.Pow(x, 2.0) * _f[2] : 0.0) + 
                   (_i >= 3 ? c2 * Math.Pow(x, 3.0) * _f[3] : 0.0) - 
                   (_i >= 4 ? c3 * Math.Pow(x, 4.0) * _f[4] : 0.0) + 
                   (_i >= 5 ? c4 * Math.Pow(x, 5.0) * _f[5] : 0.0) - 
                   (_i >= 6 ? c5 * Math.Pow(x, 6.0) * _f[6] : 0.0) + 
                   (_i >= 7 ? c6 * Math.Pow(x, 7.0) * _f[7] : 0.0) + 
                   (_i >= 8 ? c7 * Math.Pow(x, 8.0) * _f[8] : 0.0) + 
                   (_i == 9 ? c8 * Math.Pow(x, 9.0) * _f[9] : 0.0);
        }

        private HabValues CalculateHAB()
        {
            if (kama != null && CurrentBar >= 1)
            {
                double close, open, high, low;
                if (SmoothingType == lwSmoothingModes.Kaufman)
                {
                    close = kama[0];
                    open = (kama[0] + kama[1]) / 2.0;
                    high = Math.Max(High[0], Math.Max(open, close));
                    low = Math.Min(Low[0], Math.Min(open, close));
                }
                else
                {
                    close = Close[0];
                    open = Open[0];
                    high = High[0];
                    low = Low[0];
                }

                habClose[0] = (open + high + low + close) / 4.0;
                habOpen[0] = (habOpen[1] + habClose[1]) / 2.0;
                double habHigh = Math.Max(high, Math.Max(habOpen[0], habClose[0]));
                double habLow = Math.Min(low, Math.Min(habOpen[0], habClose[0]));

                return new HabValues { Open = habOpen[0], High = habHigh, Low = habLow, Close = habClose[0] };
            }
            return new HabValues { Open = Open[0], High = High[0], Low = Low[0], Close = Close[0] };
        }

        private double GetSourceValue()
        {
            switch (SourceOption)
            {
                case lwSourceModes.Close: return Close[0];
                case lwSourceModes.Open: return Open[0];
                case lwSourceModes.High: return High[0];
                case lwSourceModes.Low: return Low[0];
                case lwSourceModes.Median: return Median[0];
                case lwSourceModes.Typical: return Typical[0];
                case lwSourceModes.Weighted: return Weighted[0];
                case lwSourceModes.HAB_Close: return CalculateHAB().Close;
                default: return Close[0];
            }
        }

        private double Factorial(int n)
        {
            if (n < 0) return 0.0;
            double result = 1.0;
            for (int i = 1; i <= n; i++)
                result *= i;
            return result;
        }

        private double Std_CalculateAlpha(int period, int poles)
        {
            if (period < 1 || poles < 1) return 0.0;
            double b = (1.0 - Math.Cos(2.0 * Math.PI / period)) / (Math.Pow(1.414, 2.0 / poles) - 1.0);
            return -b + Math.Sqrt(b * b + 2.0 * b);
        }

        private double[,] MakeCoeffs(int period, int order)
        {
            double[,] coeffs = new double[order + 1, 3];
            double alpha = Std_CalculateAlpha(period, order);

            for (int i = 0; i <= order; i++)
            {
                double numerator = Factorial(order);
                double denom1 = Factorial(order - i);
                double denom2 = Factorial(i);

                if (denom1 != 0.0 && denom2 != 0.0)
                {
                    coeffs[i, 0] = numerator / (denom1 * denom2);
                    coeffs[i, 1] = Math.Pow(alpha, i);
                    coeffs[i, 2] = Math.Pow(1.0 - alpha, i);
                }
            }
            return coeffs;
        }

        private double NpoleGF(double src, Series<double> history)
        {
            if (coeffs == null || Order < 1 || history.Count <= Order)
                return src;

            double result = src * coeffs[Order, 1];
            int sign = 1;

            for (int i = 1; i <= Order; i++)
            {
                result += sign * coeffs[i, 0] * coeffs[i, 2] * history[i];
                sign *= -1;
            }
            return result;
        }

        private double StdFilt(double src, Series<double> history, double stdDevValue)
        {
            return FilterPeriod > 0 && stdDevValue != 0.0 && 
                   Math.Abs(src - history[1]) < FilterDeviations * stdDevValue ? history[1] : src;
        }

        private void CalculateOttValues()
        {
            if (CurrentBar < OttHighLowLength)
                return;

            ottHighChannel[0] = MAX(High, OttHighLowLength)[0];
            ottLowChannel[0] = MIN(Low, OttHighLowLength)[0];

            double maHigh = OttCalculateMA(ottHighChannel, OttMaType, OttPeriod, "H");
            double maLow = OttCalculateMA(ottLowChannel, OttMaType, OttPeriod, "L");

            double offset = maHigh * OttCoeff * 0.01;
            double longStop = maHigh - offset;
            ottLongStopH[0] = maHigh > ottLongStopH[1] ? Math.Max(longStop, ottLongStopH[1]) : longStop;

            double shortStop = maHigh + offset;
            ottShortStopH[0] = maHigh < ottShortStopH[1] ? Math.Min(shortStop, ottShortStopH[1]) : shortStop;

            ottDirH[0] = ottDirH[1] == 0 ? 1 : ottDirH[1];
            if (ottDirH[1] == -1 && maHigh > ottShortStopH[1])
                ottDirH[0] = 1;
            else if (ottDirH[1] == 1 && maHigh < ottLongStopH[1])
                ottDirH[0] = -1;

            double mthigh = ottDirH[0] == 1 ? ottLongStopH[0] : ottShortStopH[0];
            double ottHigh = maHigh > mthigh ? mthigh * (200.0 + OttCoeff) / 200.0 : 
                                                mthigh * (200.0 - OttCoeff) / 200.0;

            double offsetLow = maLow * OttCoeff * 0.01;
            double longStopLow = maLow - offsetLow;
            ottLongStopL[0] = maLow > ottLongStopL[1] ? Math.Max(longStopLow, ottLongStopL[1]) : longStopLow;

            double shortStopLow = maLow + offsetLow;
            ottShortStopL[0] = maLow < ottShortStopL[1] ? Math.Min(shortStopLow, ottShortStopL[1]) : shortStopLow;

            ottDirL[0] = ottDirL[1] == 0 ? 1 : ottDirL[1];
            if (ottDirL[1] == -1 && maLow > ottShortStopL[1])
                ottDirL[0] = 1;
            else if (ottDirL[1] == 1 && maLow < ottLongStopL[1])
                ottDirL[0] = -1;

            double mtlow = ottDirL[0] == 1 ? ottLongStopL[0] : ottShortStopL[0];
            double ottLow = maLow > mtlow ? mtlow * (200.0 + OttCoeff) / 200.0 : 
                                            mtlow * (200.0 - OttCoeff) / 200.0;

            RangeHigh[0] = ottHigh;
            RangeLow[0] = ottLow;
        }

        private double OttCalculateMA(ISeries<double> source, LWOttMaTypes type, int period, string channel)
        {
            switch (type)
            {
                case LWOttMaTypes.SMA: return SMA(source, period)[0];
                case LWOttMaTypes.EMA: return EMA(source, period)[0];
                case LWOttMaTypes.WMA: return WMA(source, period)[0];
                case LWOttMaTypes.DEMA: return DEMA(source, period)[0];
                case LWOttMaTypes.TMA: return OttTMA_Manual(source, period);
                case LWOttMaTypes.VAR: return OttVAR_Manual(source, period, channel == "H" ? ottVarHigh : ottVarLow);
                case LWOttMaTypes.WWMA: return OttWWMA_Manual(source, period, channel == "H" ? ottWwmaHigh : ottWwmaLow);
                case LWOttMaTypes.ZLEMA: return OttZLEMA_Manual(source, period, channel == "H" ? ottZlemah : ottZlemal);
                case LWOttMaTypes.TSF: return OttTSF_Manual(source, period);
                case LWOttMaTypes.HULL: return HMA(source, period)[0];
                default: return SMA(source, period)[0];
            }
        }

        private double OttTSF_Manual(ISeries<double> source, int period)
        {
            if (CurrentBar < period) return source[0];
            NinjaTrader.NinjaScript.Indicators.LinReg linReg = LinReg(source, period);
            double slope = linReg[0] - linReg[1];
            return linReg[0] + slope;
        }

        private double OttTMA_Manual(ISeries<double> source, int period)
        {
            int period1 = (int)Math.Ceiling(period / 2.0);
            int period2 = (int)Math.Floor(period / 2.0) + 1;
            return SMA(SMA(source, period1), period2)[0];
        }

        private double OttWWMA_Manual(ISeries<double> source, int period, Series<double> resultSeries)
        {
            if (CurrentBar == 0)
            {
                resultSeries[0] = source[0];
                return resultSeries[0];
            }
            double weight = 1.0 / period;
            resultSeries[0] = weight * source[0] + (1.0 - weight) * resultSeries[1];
            return resultSeries[0];
        }

        private double OttZLEMA_Manual(ISeries<double> source, int period, Series<double> resultSeries)
        {
            int lag = (period - 1) / 2;
            if (CurrentBar < lag) return source[0];

            double adjusted = source[0] + (source[0] - source[lag]);

            if (CurrentBar == lag)
            {
                resultSeries[0] = adjusted;
                return resultSeries[0];
            }

            double weight = 2.0 / (period + 1);
            resultSeries[0] = weight * adjusted + (1.0 - weight) * resultSeries[1];
            return resultSeries[0];
        }

        private double OttVAR_Manual(ISeries<double> source, int period, Series<double> resultSeries)
        {
            if (CurrentBar < 9) return source[0];

            double upSum = 0.0;
            double downSum = 0.0;

            for (int i = 0; i < 9; i++)
            {
                upSum += Math.Max(0.0, source[i] - source[i + 1]);
                downSum += Math.Max(0.0, source[i + 1] - source[i]);
            }

            double cmo = upSum + downSum == 0.0 ? 0.0 : (upSum - downSum) / (upSum + downSum);
            double weight = 2.0 / (period + 1) * Math.Abs(cmo);

            if (CurrentBar == 9)
            {
                resultSeries[0] = source[0];
                return resultSeries[0];
            }

            resultSeries[0] = weight * source[0] + (1.0 - weight) * resultSeries[1];
            return resultSeries[0];
        }

        private struct HabValues
        {
            public double Open;
            public double High;
            public double Low;
            public double Close;
        }

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Enable Scalp Signals", Order = 1, GroupName = "Scalp Signals")]
        public bool EnableScalpSignals { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Scalp Buy Color", Order = 2, GroupName = "Scalp Signals")]
        [XmlIgnore]
        public Brush ScalpBuySignalColor { get; set; }

        [Browsable(false)]
        public string ScalpBuySignalColorSerializable
        {
            get => Serialize.BrushToString(ScalpBuySignalColor);
            set => ScalpBuySignalColor = Serialize.StringToBrush(value);
        }

        [NinjaScriptProperty]
        [Display(Name = "Scalp Sell Color", Order = 3, GroupName = "Scalp Signals")]
        [XmlIgnore]
        public Brush ScalpSellSignalColor { get; set; }

        [Browsable(false)]
        public string ScalpSellSignalColorSerializable
        {
            get => Serialize.BrushToString(ScalpSellSignalColor);
            set => ScalpSellSignalColor = Serialize.StringToBrush(value);
        }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Scalp Symbol Font Size", Order = 4, GroupName = "Scalp Signals")]
        public int ScalpSymbolFontSize { get; set; }

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Scalp Text Font Size", Order = 5, GroupName = "Scalp Signals")]
        public int ScalpTextFontSize { get; set; }

        [NinjaScriptProperty]
        [Range(0, 200)]
        [Display(Name = "Scalp Signal Pixel Offset", Order = 6, GroupName = "Scalp Signals")]
        public int ScalpSignalPixelOffset { get; set; }

        [NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "Scalp Signal Vertical Spacing", Order = 7, GroupName = "Scalp Signals")]
        public int ScalpSignalVerticalSpacing { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use ADX Filter", Order = 1, GroupName = "Filters")]
        public bool UseADXFilter { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ADX Period", Order = 2, GroupName = "Filters")]
        public int AdxPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "ADX Threshold", Order = 3, GroupName = "Filters")]
        public int AdxThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Range Filtered Signals", Order = 4, GroupName = "Filters")]
        public bool ShowRangeFilteredSignals { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Range Filter", Order = 5, GroupName = "Filters")]
        public bool ShowRangeFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "VWAP Filter Signals", Order = 6, GroupName = "Filters")]
        public bool VWAPFilterSignals { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show VWAP", Order = 7, GroupName = "Filters")]
        public bool ShowVWAP { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trend Magic Filter Signals", Order = 8, GroupName = "Filters")]
        public bool TrendMagicFilterSignals { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "CCI Period", Order = 9, GroupName = "Filters")]
        public int CciPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Period", Order = 10, GroupName = "Filters")]
        public int AtrPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.00001, double.MaxValue)]
        [Display(Name = "ATR Multiplier", Order = 11, GroupName = "Filters")]
        public double AtrMult { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "OTT Period", Order = 12, GroupName = "Filters")]
        public int OttPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "OTT Coeff", Order = 13, GroupName = "Filters")]
        public double OttCoeff { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "OTT High/Low Length", Order = 14, GroupName = "Filters")]
        public int OttHighLowLength { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "OTT MA Type", Order = 15, GroupName = "Filters")]
        public LWOttMaTypes OttMaType { get; set; }

        [NinjaScriptProperty]
        [Range(1, 9)]
        [Display(Name = "Poles (N)", Order = 1, GroupName = "Calculation")]
        public int N { get; set; }

        [NinjaScriptProperty]
        [Range(2, int.MaxValue)]
        [Display(Name = "Sampling Period", Order = 2, GroupName = "Calculation")]
        public int Per { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, double.MaxValue)]
        [Display(Name = "Multiplier", Order = 3, GroupName = "Calculation")]
        public double Mult { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Mode Lag", Order = 4, GroupName = "Calculation")]
        public bool ModeLag { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Mode Fast", Order = 5, GroupName = "Calculation")]
        public bool ModeFast { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Smoothing Type", Order = 6, GroupName = "Calculation")]
        public lwSmoothingModes SmoothingType { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Source Option", Order = 7, GroupName = "Calculation")]
        public lwSourceModes SourceOption { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Order = 8, GroupName = "Calculation")]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Order", Order = 9, GroupName = "Calculation")]
        public int Order { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Filter Option", Order = 10, GroupName = "Calculation")]
        public lwFilterOptions FilterOption { get; set; }

        [NinjaScriptProperty]
        [Range(0.0, double.MaxValue)]
        [Display(Name = "Filter Deviations", Order = 11, GroupName = "Calculation")]
        public double FilterDeviations { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Filter Period", Order = 12, GroupName = "Calculation")]
        public int FilterPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(5, int.MaxValue)]
        [Display(Name = "KAMA Period", Order = 13, GroupName = "Calculation")]
        public int KamaPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "KAMA Fast", Order = 14, GroupName = "Calculation")]
        public int KamaFast { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "KAMA Slow", Order = 15, GroupName = "Calculation")]
        public int KamaSlow { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> lw0 => Values[0];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RangeHigh => Values[1];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RangeLow => Values[2];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VWAP => Values[3];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> lwLine => Values[4];
		
		[NinjaScriptProperty]
		[Display(Name = "Show lw0 Line", Order = 1, GroupName = "Line Display")]
		public bool Showlw0 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "lw0 Up Color", Order = 2, GroupName = "Line Display")]
		[XmlIgnore]
		public Brush lwUpColor { get; set; }
		
		[Browsable(false)]
		public string lwUpColorSerializable
		{
		    get => Serialize.BrushToString(lwUpColor);
		    set => lwUpColor = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "lw0 Down Color", Order = 3, GroupName = "Line Display")]
		[XmlIgnore]
		public Brush lwDownColor { get; set; }
		
		[Browsable(false)]
		public string lwDownColorSerializable
		{
		    get => Serialize.BrushToString(lwDownColor);
		    set => lwDownColor = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "VWAP Up Color", Order = 4, GroupName = "Line Display")]
		[XmlIgnore]
		public Brush VWAPUpColor { get; set; }
		
		[Browsable(false)]
		public string VWAPUpColorSerializable
		{
		    get => Serialize.BrushToString(VWAPUpColor);
		    set => VWAPUpColor = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "VWAP Down Color", Order = 5, GroupName = "Line Display")]
		[XmlIgnore]
		public Brush VWAPDownColor { get; set; }
		
		[Browsable(false)]
		public string VWAPDownColorSerializable
		{
		    get => Serialize.BrushToString(VWAPDownColor);
		    set => VWAPDownColor = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Color Bars", Order = 1, GroupName = "Bar Coloring")]
		public bool ColorBars { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Bullish - Breakout", Order = 2, GroupName = "Bar Coloring")]
		[XmlIgnore]
		public Brush BarUp2Color { get; set; }
		
		[Browsable(false)]
		public string BarUp2ColorSerializable
		{
		    get => Serialize.BrushToString(BarUp2Color);
		    set => BarUp2Color = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Bullish - Trend", Order = 3, GroupName = "Bar Coloring")]
		[XmlIgnore]
		public Brush BarUp1Color { get; set; }
		
		[Browsable(false)]
		public string BarUp1ColorSerializable
		{
		    get => Serialize.BrushToString(BarUp1Color);
		    set => BarUp1Color = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Bullish - Pullback", Order = 4, GroupName = "Bar Coloring")]
		[XmlIgnore]
		public Brush BarUp3Color { get; set; }
		
		[Browsable(false)]
		public string BarUp3ColorSerializable
		{
		    get => Serialize.BrushToString(BarUp3Color);
		    set => BarUp3Color = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Bearish - Breakout", Order = 5, GroupName = "Bar Coloring")]
		[XmlIgnore]
		public Brush BarDown2Color { get; set; }
		
		[Browsable(false)]
		public string BarDown2ColorSerializable
		{
		    get => Serialize.BrushToString(BarDown2Color);
		    set => BarDown2Color = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Bearish - Trend", Order = 6, GroupName = "Bar Coloring")]
		[XmlIgnore]
		public Brush BarDown1Color { get; set; }
		
		[Browsable(false)]
		public string BarDown1ColorSerializable
		{
		    get => Serialize.BrushToString(BarDown1Color);
		    set => BarDown1Color = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Bearish - Pullback", Order = 7, GroupName = "Bar Coloring")]
		[XmlIgnore]
		public Brush BarDown3Color { get; set; }
		
		[Browsable(false)]
		public string BarDown3ColorSerializable
		{
		    get => Serialize.BrushToString(BarDown3Color);
		    set => BarDown3Color = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Neutral", Order = 8, GroupName = "Bar Coloring")]
		[XmlIgnore]
		public Brush BarNeutralColor { get; set; }
		
		[Browsable(false)]
		public string BarNeutralColorSerializable
		{
		    get => Serialize.BrushToString(BarNeutralColor);
		    set => BarNeutralColor = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Trend Magic Up Color", Order = 6, GroupName = "Line Display")]
		[XmlIgnore]
		public Brush TrendMagicUpColor { get; set; }
		
		[Browsable(false)]
		public string TrendMagicUpColorSerializable
		{
		    get => Serialize.BrushToString(TrendMagicUpColor);
		    set => TrendMagicUpColor = Serialize.StringToBrush(value);
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Trend Magic Down Color", Order = 7, GroupName = "Line Display")]
		[XmlIgnore]
		public Brush TrendMagicDownColor { get; set; }
		
		[Browsable(false)]
		public string TrendMagicDownColorSerializable
		{
		    get => Serialize.BrushToString(TrendMagicDownColor);
		    set => TrendMagicDownColor = Serialize.StringToBrush(value);
		}



        #endregion
    }

    
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LWScalp[] cacheLWScalp;
		public LWScalp LWScalp(bool enableScalpSignals, Brush scalpBuySignalColor, Brush scalpSellSignalColor, int scalpSymbolFontSize, int scalpTextFontSize, int scalpSignalPixelOffset, int scalpSignalVerticalSpacing, bool useADXFilter, int adxPeriod, int adxThreshold, bool showRangeFilteredSignals, bool showRangeFilter, bool vWAPFilterSignals, bool showVWAP, bool trendMagicFilterSignals, int cciPeriod, int atrPeriod, double atrMult, int ottPeriod, double ottCoeff, int ottHighLowLength, LWOttMaTypes ottMaType, int n, int per, double mult, bool modeLag, bool modeFast, lwSmoothingModes smoothingType, lwSourceModes sourceOption, int period, int order, lwFilterOptions filterOption, double filterDeviations, int filterPeriod, int kamaPeriod, int kamaFast, int kamaSlow, bool showlw0, Brush lwUpColor, Brush lwDownColor, Brush vWAPUpColor, Brush vWAPDownColor, bool colorBars, Brush barUp2Color, Brush barUp1Color, Brush barUp3Color, Brush barDown2Color, Brush barDown1Color, Brush barDown3Color, Brush barNeutralColor, Brush trendMagicUpColor, Brush trendMagicDownColor)
		{
			return LWScalp(Input, enableScalpSignals, scalpBuySignalColor, scalpSellSignalColor, scalpSymbolFontSize, scalpTextFontSize, scalpSignalPixelOffset, scalpSignalVerticalSpacing, useADXFilter, adxPeriod, adxThreshold, showRangeFilteredSignals, showRangeFilter, vWAPFilterSignals, showVWAP, trendMagicFilterSignals, cciPeriod, atrPeriod, atrMult, ottPeriod, ottCoeff, ottHighLowLength, ottMaType, n, per, mult, modeLag, modeFast, smoothingType, sourceOption, period, order, filterOption, filterDeviations, filterPeriod, kamaPeriod, kamaFast, kamaSlow, showlw0, lwUpColor, lwDownColor, vWAPUpColor, vWAPDownColor, colorBars, barUp2Color, barUp1Color, barUp3Color, barDown2Color, barDown1Color, barDown3Color, barNeutralColor, trendMagicUpColor, trendMagicDownColor);
		}

		public LWScalp LWScalp(ISeries<double> input, bool enableScalpSignals, Brush scalpBuySignalColor, Brush scalpSellSignalColor, int scalpSymbolFontSize, int scalpTextFontSize, int scalpSignalPixelOffset, int scalpSignalVerticalSpacing, bool useADXFilter, int adxPeriod, int adxThreshold, bool showRangeFilteredSignals, bool showRangeFilter, bool vWAPFilterSignals, bool showVWAP, bool trendMagicFilterSignals, int cciPeriod, int atrPeriod, double atrMult, int ottPeriod, double ottCoeff, int ottHighLowLength, LWOttMaTypes ottMaType, int n, int per, double mult, bool modeLag, bool modeFast, lwSmoothingModes smoothingType, lwSourceModes sourceOption, int period, int order, lwFilterOptions filterOption, double filterDeviations, int filterPeriod, int kamaPeriod, int kamaFast, int kamaSlow, bool showlw0, Brush lwUpColor, Brush lwDownColor, Brush vWAPUpColor, Brush vWAPDownColor, bool colorBars, Brush barUp2Color, Brush barUp1Color, Brush barUp3Color, Brush barDown2Color, Brush barDown1Color, Brush barDown3Color, Brush barNeutralColor, Brush trendMagicUpColor, Brush trendMagicDownColor)
		{
			if (cacheLWScalp != null)
				for (int idx = 0; idx < cacheLWScalp.Length; idx++)
					if (cacheLWScalp[idx] != null && cacheLWScalp[idx].EnableScalpSignals == enableScalpSignals && cacheLWScalp[idx].ScalpBuySignalColor == scalpBuySignalColor && cacheLWScalp[idx].ScalpSellSignalColor == scalpSellSignalColor && cacheLWScalp[idx].ScalpSymbolFontSize == scalpSymbolFontSize && cacheLWScalp[idx].ScalpTextFontSize == scalpTextFontSize && cacheLWScalp[idx].ScalpSignalPixelOffset == scalpSignalPixelOffset && cacheLWScalp[idx].ScalpSignalVerticalSpacing == scalpSignalVerticalSpacing && cacheLWScalp[idx].UseADXFilter == useADXFilter && cacheLWScalp[idx].AdxPeriod == adxPeriod && cacheLWScalp[idx].AdxThreshold == adxThreshold && cacheLWScalp[idx].ShowRangeFilteredSignals == showRangeFilteredSignals && cacheLWScalp[idx].ShowRangeFilter == showRangeFilter && cacheLWScalp[idx].VWAPFilterSignals == vWAPFilterSignals && cacheLWScalp[idx].ShowVWAP == showVWAP && cacheLWScalp[idx].TrendMagicFilterSignals == trendMagicFilterSignals && cacheLWScalp[idx].CciPeriod == cciPeriod && cacheLWScalp[idx].AtrPeriod == atrPeriod && cacheLWScalp[idx].AtrMult == atrMult && cacheLWScalp[idx].OttPeriod == ottPeriod && cacheLWScalp[idx].OttCoeff == ottCoeff && cacheLWScalp[idx].OttHighLowLength == ottHighLowLength && cacheLWScalp[idx].OttMaType == ottMaType && cacheLWScalp[idx].N == n && cacheLWScalp[idx].Per == per && cacheLWScalp[idx].Mult == mult && cacheLWScalp[idx].ModeLag == modeLag && cacheLWScalp[idx].ModeFast == modeFast && cacheLWScalp[idx].SmoothingType == smoothingType && cacheLWScalp[idx].SourceOption == sourceOption && cacheLWScalp[idx].Period == period && cacheLWScalp[idx].Order == order && cacheLWScalp[idx].FilterOption == filterOption && cacheLWScalp[idx].FilterDeviations == filterDeviations && cacheLWScalp[idx].FilterPeriod == filterPeriod && cacheLWScalp[idx].KamaPeriod == kamaPeriod && cacheLWScalp[idx].KamaFast == kamaFast && cacheLWScalp[idx].KamaSlow == kamaSlow && cacheLWScalp[idx].Showlw0 == showlw0 && cacheLWScalp[idx].lwUpColor == lwUpColor && cacheLWScalp[idx].lwDownColor == lwDownColor && cacheLWScalp[idx].VWAPUpColor == vWAPUpColor && cacheLWScalp[idx].VWAPDownColor == vWAPDownColor && cacheLWScalp[idx].ColorBars == colorBars && cacheLWScalp[idx].BarUp2Color == barUp2Color && cacheLWScalp[idx].BarUp1Color == barUp1Color && cacheLWScalp[idx].BarUp3Color == barUp3Color && cacheLWScalp[idx].BarDown2Color == barDown2Color && cacheLWScalp[idx].BarDown1Color == barDown1Color && cacheLWScalp[idx].BarDown3Color == barDown3Color && cacheLWScalp[idx].BarNeutralColor == barNeutralColor && cacheLWScalp[idx].TrendMagicUpColor == trendMagicUpColor && cacheLWScalp[idx].TrendMagicDownColor == trendMagicDownColor && cacheLWScalp[idx].EqualsInput(input))
						return cacheLWScalp[idx];
			return CacheIndicator<LWScalp>(new LWScalp(){ EnableScalpSignals = enableScalpSignals, ScalpBuySignalColor = scalpBuySignalColor, ScalpSellSignalColor = scalpSellSignalColor, ScalpSymbolFontSize = scalpSymbolFontSize, ScalpTextFontSize = scalpTextFontSize, ScalpSignalPixelOffset = scalpSignalPixelOffset, ScalpSignalVerticalSpacing = scalpSignalVerticalSpacing, UseADXFilter = useADXFilter, AdxPeriod = adxPeriod, AdxThreshold = adxThreshold, ShowRangeFilteredSignals = showRangeFilteredSignals, ShowRangeFilter = showRangeFilter, VWAPFilterSignals = vWAPFilterSignals, ShowVWAP = showVWAP, TrendMagicFilterSignals = trendMagicFilterSignals, CciPeriod = cciPeriod, AtrPeriod = atrPeriod, AtrMult = atrMult, OttPeriod = ottPeriod, OttCoeff = ottCoeff, OttHighLowLength = ottHighLowLength, OttMaType = ottMaType, N = n, Per = per, Mult = mult, ModeLag = modeLag, ModeFast = modeFast, SmoothingType = smoothingType, SourceOption = sourceOption, Period = period, Order = order, FilterOption = filterOption, FilterDeviations = filterDeviations, FilterPeriod = filterPeriod, KamaPeriod = kamaPeriod, KamaFast = kamaFast, KamaSlow = kamaSlow, Showlw0 = showlw0, lwUpColor = lwUpColor, lwDownColor = lwDownColor, VWAPUpColor = vWAPUpColor, VWAPDownColor = vWAPDownColor, ColorBars = colorBars, BarUp2Color = barUp2Color, BarUp1Color = barUp1Color, BarUp3Color = barUp3Color, BarDown2Color = barDown2Color, BarDown1Color = barDown1Color, BarDown3Color = barDown3Color, BarNeutralColor = barNeutralColor, TrendMagicUpColor = trendMagicUpColor, TrendMagicDownColor = trendMagicDownColor }, input, ref cacheLWScalp);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LWScalp LWScalp(bool enableScalpSignals, Brush scalpBuySignalColor, Brush scalpSellSignalColor, int scalpSymbolFontSize, int scalpTextFontSize, int scalpSignalPixelOffset, int scalpSignalVerticalSpacing, bool useADXFilter, int adxPeriod, int adxThreshold, bool showRangeFilteredSignals, bool showRangeFilter, bool vWAPFilterSignals, bool showVWAP, bool trendMagicFilterSignals, int cciPeriod, int atrPeriod, double atrMult, int ottPeriod, double ottCoeff, int ottHighLowLength, LWOttMaTypes ottMaType, int n, int per, double mult, bool modeLag, bool modeFast, lwSmoothingModes smoothingType, lwSourceModes sourceOption, int period, int order, lwFilterOptions filterOption, double filterDeviations, int filterPeriod, int kamaPeriod, int kamaFast, int kamaSlow, bool showlw0, Brush lwUpColor, Brush lwDownColor, Brush vWAPUpColor, Brush vWAPDownColor, bool colorBars, Brush barUp2Color, Brush barUp1Color, Brush barUp3Color, Brush barDown2Color, Brush barDown1Color, Brush barDown3Color, Brush barNeutralColor, Brush trendMagicUpColor, Brush trendMagicDownColor)
		{
			return indicator.LWScalp(Input, enableScalpSignals, scalpBuySignalColor, scalpSellSignalColor, scalpSymbolFontSize, scalpTextFontSize, scalpSignalPixelOffset, scalpSignalVerticalSpacing, useADXFilter, adxPeriod, adxThreshold, showRangeFilteredSignals, showRangeFilter, vWAPFilterSignals, showVWAP, trendMagicFilterSignals, cciPeriod, atrPeriod, atrMult, ottPeriod, ottCoeff, ottHighLowLength, ottMaType, n, per, mult, modeLag, modeFast, smoothingType, sourceOption, period, order, filterOption, filterDeviations, filterPeriod, kamaPeriod, kamaFast, kamaSlow, showlw0, lwUpColor, lwDownColor, vWAPUpColor, vWAPDownColor, colorBars, barUp2Color, barUp1Color, barUp3Color, barDown2Color, barDown1Color, barDown3Color, barNeutralColor, trendMagicUpColor, trendMagicDownColor);
		}

		public Indicators.LWScalp LWScalp(ISeries<double> input , bool enableScalpSignals, Brush scalpBuySignalColor, Brush scalpSellSignalColor, int scalpSymbolFontSize, int scalpTextFontSize, int scalpSignalPixelOffset, int scalpSignalVerticalSpacing, bool useADXFilter, int adxPeriod, int adxThreshold, bool showRangeFilteredSignals, bool showRangeFilter, bool vWAPFilterSignals, bool showVWAP, bool trendMagicFilterSignals, int cciPeriod, int atrPeriod, double atrMult, int ottPeriod, double ottCoeff, int ottHighLowLength, LWOttMaTypes ottMaType, int n, int per, double mult, bool modeLag, bool modeFast, lwSmoothingModes smoothingType, lwSourceModes sourceOption, int period, int order, lwFilterOptions filterOption, double filterDeviations, int filterPeriod, int kamaPeriod, int kamaFast, int kamaSlow, bool showlw0, Brush lwUpColor, Brush lwDownColor, Brush vWAPUpColor, Brush vWAPDownColor, bool colorBars, Brush barUp2Color, Brush barUp1Color, Brush barUp3Color, Brush barDown2Color, Brush barDown1Color, Brush barDown3Color, Brush barNeutralColor, Brush trendMagicUpColor, Brush trendMagicDownColor)
		{
			return indicator.LWScalp(input, enableScalpSignals, scalpBuySignalColor, scalpSellSignalColor, scalpSymbolFontSize, scalpTextFontSize, scalpSignalPixelOffset, scalpSignalVerticalSpacing, useADXFilter, adxPeriod, adxThreshold, showRangeFilteredSignals, showRangeFilter, vWAPFilterSignals, showVWAP, trendMagicFilterSignals, cciPeriod, atrPeriod, atrMult, ottPeriod, ottCoeff, ottHighLowLength, ottMaType, n, per, mult, modeLag, modeFast, smoothingType, sourceOption, period, order, filterOption, filterDeviations, filterPeriod, kamaPeriod, kamaFast, kamaSlow, showlw0, lwUpColor, lwDownColor, vWAPUpColor, vWAPDownColor, colorBars, barUp2Color, barUp1Color, barUp3Color, barDown2Color, barDown1Color, barDown3Color, barNeutralColor, trendMagicUpColor, trendMagicDownColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LWScalp LWScalp(bool enableScalpSignals, Brush scalpBuySignalColor, Brush scalpSellSignalColor, int scalpSymbolFontSize, int scalpTextFontSize, int scalpSignalPixelOffset, int scalpSignalVerticalSpacing, bool useADXFilter, int adxPeriod, int adxThreshold, bool showRangeFilteredSignals, bool showRangeFilter, bool vWAPFilterSignals, bool showVWAP, bool trendMagicFilterSignals, int cciPeriod, int atrPeriod, double atrMult, int ottPeriod, double ottCoeff, int ottHighLowLength, LWOttMaTypes ottMaType, int n, int per, double mult, bool modeLag, bool modeFast, lwSmoothingModes smoothingType, lwSourceModes sourceOption, int period, int order, lwFilterOptions filterOption, double filterDeviations, int filterPeriod, int kamaPeriod, int kamaFast, int kamaSlow, bool showlw0, Brush lwUpColor, Brush lwDownColor, Brush vWAPUpColor, Brush vWAPDownColor, bool colorBars, Brush barUp2Color, Brush barUp1Color, Brush barUp3Color, Brush barDown2Color, Brush barDown1Color, Brush barDown3Color, Brush barNeutralColor, Brush trendMagicUpColor, Brush trendMagicDownColor)
		{
			return indicator.LWScalp(Input, enableScalpSignals, scalpBuySignalColor, scalpSellSignalColor, scalpSymbolFontSize, scalpTextFontSize, scalpSignalPixelOffset, scalpSignalVerticalSpacing, useADXFilter, adxPeriod, adxThreshold, showRangeFilteredSignals, showRangeFilter, vWAPFilterSignals, showVWAP, trendMagicFilterSignals, cciPeriod, atrPeriod, atrMult, ottPeriod, ottCoeff, ottHighLowLength, ottMaType, n, per, mult, modeLag, modeFast, smoothingType, sourceOption, period, order, filterOption, filterDeviations, filterPeriod, kamaPeriod, kamaFast, kamaSlow, showlw0, lwUpColor, lwDownColor, vWAPUpColor, vWAPDownColor, colorBars, barUp2Color, barUp1Color, barUp3Color, barDown2Color, barDown1Color, barDown3Color, barNeutralColor, trendMagicUpColor, trendMagicDownColor);
		}

		public Indicators.LWScalp LWScalp(ISeries<double> input , bool enableScalpSignals, Brush scalpBuySignalColor, Brush scalpSellSignalColor, int scalpSymbolFontSize, int scalpTextFontSize, int scalpSignalPixelOffset, int scalpSignalVerticalSpacing, bool useADXFilter, int adxPeriod, int adxThreshold, bool showRangeFilteredSignals, bool showRangeFilter, bool vWAPFilterSignals, bool showVWAP, bool trendMagicFilterSignals, int cciPeriod, int atrPeriod, double atrMult, int ottPeriod, double ottCoeff, int ottHighLowLength, LWOttMaTypes ottMaType, int n, int per, double mult, bool modeLag, bool modeFast, lwSmoothingModes smoothingType, lwSourceModes sourceOption, int period, int order, lwFilterOptions filterOption, double filterDeviations, int filterPeriod, int kamaPeriod, int kamaFast, int kamaSlow, bool showlw0, Brush lwUpColor, Brush lwDownColor, Brush vWAPUpColor, Brush vWAPDownColor, bool colorBars, Brush barUp2Color, Brush barUp1Color, Brush barUp3Color, Brush barDown2Color, Brush barDown1Color, Brush barDown3Color, Brush barNeutralColor, Brush trendMagicUpColor, Brush trendMagicDownColor)
		{
			return indicator.LWScalp(input, enableScalpSignals, scalpBuySignalColor, scalpSellSignalColor, scalpSymbolFontSize, scalpTextFontSize, scalpSignalPixelOffset, scalpSignalVerticalSpacing, useADXFilter, adxPeriod, adxThreshold, showRangeFilteredSignals, showRangeFilter, vWAPFilterSignals, showVWAP, trendMagicFilterSignals, cciPeriod, atrPeriod, atrMult, ottPeriod, ottCoeff, ottHighLowLength, ottMaType, n, per, mult, modeLag, modeFast, smoothingType, sourceOption, period, order, filterOption, filterDeviations, filterPeriod, kamaPeriod, kamaFast, kamaSlow, showlw0, lwUpColor, lwDownColor, vWAPUpColor, vWAPDownColor, colorBars, barUp2Color, barUp1Color, barUp3Color, barDown2Color, barDown1Color, barDown3Color, barNeutralColor, trendMagicUpColor, trendMagicDownColor);
		}
	}
}

#endregion
