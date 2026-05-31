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
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class AlightenDynamicLevelsV3 : Indicator
    {
        #region Class Variables

        private const string tagPrefix = "ZZ_line_";
        private string instanceId;

        // BIP: 0=primary, 1=60m, 2=Daily, 3=Weekly, 4=240m
        private List<int>[] pivotBars         = new List<int>[5];
        private List<double>[] pivotPrices    = new List<double>[5];
        private List<DateTime>[] pivotTimes   = new List<DateTime>[5];
        private List<bool>[] pivotIsHigh      = new List<bool>[5];
        private List<double>[] pivotGuidePrices = new List<double>[5];

        private const string anchorTag = "TrendMatrix_Anchor";

        // --- Store HTF levels for use in strategy
        private Series<double>[] HTFLevels = new Series<double>[60];

        // simplify array management (10 above + 10 below per timeframe)
        private const int DailyAboveOff    = 0;   // 0..9
        private const int DailyBelowOff    = 10;  // 10..19
        private const int WeeklyAboveOff   = 20;  // 20..29
        private const int WeeklyBelowOff   = 30;  // 30..39
        private const int FourHourAboveOff = 40;  // 40..49
        private const int FourHourBelowOff = 50;  // 50..59

        #endregion

        #region Public accessors for Strategies

        [Browsable(false), XmlIgnore]
        public Series<double>[] HTFKeyLevels => HTFLevels;

        #endregion

        #region Configurable Properties

        // --- Daily ---
        [NinjaScriptProperty]
        [Display(Name = "Enable Daily SnR Levels", GroupName = "Higher Time-Frame Settings", Order = 2)]
        public bool EnableSeries5SnRLevels { get; set; } = true;

        [XmlIgnore]
        [NinjaScriptProperty]
        [Display(Name = "Daily SnR Levels Line Color", GroupName = "Higher Time-Frame Settings", Order = 3)]
        public Brush Series5Color { get; set; } = Brushes.Cyan;
        [Browsable(false)]
        public string Series5ColorSerialize { get => Serialize.BrushToString(Series5Color); set => Series5Color = Serialize.StringToBrush(value); }

        [NinjaScriptProperty]
        [Display(Name = "Daily SnR Levels Line Thickness", GroupName = "Higher Time-Frame Settings", Order = 4)]
        public int Series5Thickness { get; set; } = 2;

        [NinjaScriptProperty]
        [Display(Name = "Daily SnR Levels Line Style", GroupName = "Higher Time-Frame Settings", Order = 5)]
        public DashStyleHelper Series5Style { get; set; } = DashStyleHelper.Dash;

        // --- Weekly ---
        [NinjaScriptProperty]
        [Display(Name = "Enable Weekly SnR Levels", GroupName = "Higher Time-Frame Settings", Order = 6)]
        public bool EnableSeries6SnRLevels { get; set; } = true;

        [XmlIgnore]
        [NinjaScriptProperty]
        [Display(Name = "Weekly SnR Levels Line Color", GroupName = "Higher Time-Frame Settings", Order = 7)]
        public Brush Series6Color { get; set; } = Brushes.Red;
        [Browsable(false)]
        public string Series6ColorSerialize { get => Serialize.BrushToString(Series6Color); set => Series6Color = Serialize.StringToBrush(value); }

        [NinjaScriptProperty]
        [Display(Name = "Weekly SnR Levels Line Thickness", GroupName = "Higher Time-Frame Settings", Order = 8)]
        public int Series6Thickness { get; set; } = 2;

        [NinjaScriptProperty]
        [Display(Name = "Weekly SnR Levels Line Style", GroupName = "Higher Time-Frame Settings", Order = 9)]
        public DashStyleHelper Series6Style { get; set; } = DashStyleHelper.Dash;

        // --- 240-minute (4h) ---
        [NinjaScriptProperty]
        [Display(Name = "Enable 240m SnR Levels", GroupName = "Higher Time-Frame Settings", Order = 10)]
        public bool EnableSeries4SnRLevels { get; set; } = true;

        [XmlIgnore]
        [NinjaScriptProperty]
        [Display(Name = "240m SnR Levels Line Color", GroupName = "Higher Time-Frame Settings", Order = 11)]
        public Brush Series4Color { get; set; } = Brushes.Goldenrod;
        [Browsable(false)]
        public string Series4ColorSerialize { get => Serialize.BrushToString(Series4Color); set => Series4Color = Serialize.StringToBrush(value); }

        [NinjaScriptProperty]
        [Display(Name = "240m SnR Levels Line Thickness", GroupName = "Higher Time-Frame Settings", Order = 12)]
        public int Series4Thickness { get; set; } = 2;

        [NinjaScriptProperty]
        [Display(Name = "240m SnR Levels Line Style", GroupName = "Higher Time-Frame Settings", Order = 13)]
        public DashStyleHelper Series4Style { get; set; } = DashStyleHelper.Dash;

        #endregion

        #region OnStateChange

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Multi-timeframe trend, support, and resistance indicator with native drawing objects.";
                Name = "Alighten DynamicLevels V3";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DrawOnPricePanel = true;
                DisplayInDataBox = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                for (int i = 0; i < 60; i++)
                {
                    AddPlot(Brushes.Transparent, $"HTF_Level_{i}");
                }
            }
            else if (State == State.Configure)
            {
                instanceId = $"ZZ_{Instrument.FullName}_{Guid.NewGuid()}";

                // BIP=1 (existing): 60-minute for coordination
                AddDataSeries(BarsPeriodType.Minute, 60);

                // Daily BarsPeriod
                var dailyPeriod = new BarsPeriod
                {
                    BarsPeriodType = BarsPeriodType.Day,
                    Value = 1
                };
                // Weekly BarsPeriod
                var weeklyPeriod = new BarsPeriod
                {
                    BarsPeriodType = BarsPeriodType.Week,
                    Value = 1
                };

                // BIP=2: Daily
                AddDataSeries(
                    Instrument.FullName,
                    dailyPeriod,
                    200,          // last 200 daily bars
                    string.Empty, // default session
                    false         // no fill forward
                );

                // BIP=3: Weekly
                AddDataSeries(
                    Instrument.FullName,
                    weeklyPeriod,
                    100,          // last 100 weekly bars
                    string.Empty,
                    false
                );

                // BIP=4: 240-minute (4h)
                AddDataSeries(BarsPeriodType.Minute, 240);
            }
            else if (State == State.DataLoaded)
            {
                for (int i = 1; i <= 4; i++)
                {
                    pivotBars[i] = new List<int>();
                    pivotPrices[i] = new List<double>();
                    pivotTimes[i] = new List<DateTime>();
                    pivotIsHigh[i] = new List<bool>();
                    pivotGuidePrices[i] = new List<double>();
                }

                // Init HTF array for use in strategy
                for (int i = 0; i < 60; i++)
                    HTFLevels[i] = new Series<double>(this);
            }
            else if (State == State.Terminated)
            {
                // cleanup if needed
            }
            else if (State == State.Historical)
            {
                // no-op
            }
        }

        #endregion

        #region OnBarUpdate

        protected override void OnBarUpdate()
        {
            try
            {
                // Build pivot sets from HTF series (Daily=2, Weekly=3, 240m=4)
                if (BarsInProgress >= 2 && BarsInProgress <= 4)
                {
                    if (CurrentBars[BarsInProgress] < 2)
                        return;

                    double h0 = Highs[BarsInProgress][0];
                    double h1 = Highs[BarsInProgress][1];
                    double l0 = Lows[BarsInProgress][0];
                    double l1 = Lows[BarsInProgress][1];
                    double o0 = Opens[BarsInProgress][0];
                    double o1 = Opens[BarsInProgress][1];
                    double c0 = Closes[BarsInProgress][0];
                    double c1 = Closes[BarsInProgress][1];

                    bool isHigh        = h0 > h1;
                    bool isLow         = l0 < l1;
                    bool previousGreen = c1 > o1;
                    bool previousRed   = c1 < o1;
                    bool currentGreen  = c0 > o0;
                    bool currentRed    = c0 < o0;

                    double highGuide = Math.Max(o0, c0);
                    double lowGuide  = Math.Min(o0, c0);
                    DateTime time    = Times[BarsInProgress][0];

                    // --- Pivot Detection (simple/robust) ---
                    if (previousGreen && currentGreen && isHigh)
                        ProcessPivotGeneric(BarsInProgress, h0, time, true, highGuide);
                    if (previousRed && currentRed && isLow)
                        ProcessPivotGeneric(BarsInProgress, l0, time, false, lowGuide);
                    if (previousGreen && currentRed && isHigh)
                        ProcessPivotGeneric(BarsInProgress, h0, time, true, highGuide);
                    if (previousRed && currentGreen && isLow)
                        ProcessPivotGeneric(BarsInProgress, l0, time, false, lowGuide);
                    if (previousRed && currentGreen && isHigh)
                        ProcessPivotGeneric(BarsInProgress, h0, time, true, highGuide);
                    if (previousGreen && currentRed && isLow)
                        ProcessPivotGeneric(BarsInProgress, l0, time, false, lowGuide);
                }

                // Refresh drawings while building history whenever any HTF stream updates
                if (State == State.Historical && (BarsInProgress == 1 || BarsInProgress == 2 || BarsInProgress == 3 || BarsInProgress == 4) && CurrentBars[BarsInProgress] > 0)
                    RefreshDrawings();

                // In realtime, refresh on primary stream ticks
                if (State == State.Realtime && BarsInProgress == 0 && CurrentBars[0] > 0)
                    RefreshDrawings();
            }
            catch (Exception ex)
            {
                Print($"[ERROR] Exception in OnBarUpdate @ BIP={BarsInProgress}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        #endregion

        #region Process Pivots

        private void ProcessPivotGeneric(int seriesIndex, double price, DateTime time, bool isHigh, double guidePrice)
        {
            if (BarsInProgress != seriesIndex)
                return;

            var bars        = pivotBars[seriesIndex];
            var prices      = pivotPrices[seriesIndex];
            var times       = pivotTimes[seriesIndex];
            var highs       = pivotIsHigh[seriesIndex];
            var guidePrices = pivotGuidePrices[seriesIndex];

            int barIndex = CurrentBars[seriesIndex];

            if (bars.Count == 0)
            {
                AddPivotGeneric(seriesIndex, barIndex, price, time, isHigh, guidePrice);
                return;
            }

            bool lastIsHigh = highs.Last();
            double lastPrice = prices.Last();

            if (isHigh == lastIsHigh)
            {
                if ((isHigh && price > lastPrice) || (!isHigh && price < lastPrice))
                {
                    prices[prices.Count - 1] = price;
                    bars[bars.Count - 1] = barIndex;
                    times[times.Count - 1] = time;
                    guidePrices[guidePrices.Count - 1] = guidePrice;
                    ForceRefresh();
                }
            }
            else
            {
                AddPivotGeneric(seriesIndex, barIndex, price, time, isHigh, guidePrice);
            }
        }

        private void AddPivotGeneric(int seriesIndex, int barIndex, double price, DateTime time, bool isHigh, double guidePrice)
        {
            pivotBars[seriesIndex].Add(barIndex);
            pivotPrices[seriesIndex].Add(price);
            pivotTimes[seriesIndex].Add(time);
            pivotIsHigh[seriesIndex].Add(isHigh);
            pivotGuidePrices[seriesIndex].Add(guidePrice);
        }

        #endregion

        #region Refresh Drawings

        private void RefreshDrawings()
        {
            RemoveDrawObjects();

            // Draw HTF guides by toggle
            if (EnableSeries5SnRLevels) DrawHTFLevels(2); // Daily
            if (EnableSeries6SnRLevels) DrawHTFLevels(3); // Weekly
            if (EnableSeries4SnRLevels) DrawHTFLevels(4); // 240m
        }

        private void DrawHTFLevels(int seriesIndex)
        {
            // 1) nothing to draw if no pivots
            if (pivotTimes[seriesIndex].Count == 0)
                return;

            // 2) pick your brush & timestamp for extending the lines
            var brush = GetBrushForSeries(seriesIndex);
            DateTime now = BarsArray[0].GetTime(Math.Max(0, BarsArray[0].Count - 1));

            // 3) get the last *completed* HTF bar's close
            var htfBars = BarsArray[seriesIndex];
            int barCount = htfBars.Count;
            if (barCount < 2)
                return;   // need at least two bars to have a closed one

            // second-to-last (closed) bar
            double close = htfBars.GetClose(barCount - 2);

            // 4) build a list of all pivot (price, time) pairs (using guide price: body extreme)
            var pts = pivotGuidePrices[seriesIndex]
                        .Select((price, idx) => new { Price = price, Time = pivotTimes[seriesIndex][idx] })
                        .ToList();

            // 5) pick the 10 nearest above & below that *closed* price
            var aboveList = pts
                .Where(p => p.Price >= close)
                .OrderBy(p => p.Price - close)
                .ThenByDescending(p => p.Time)
                .Take(10)
                .ToList();

            var belowList = pts
                .Where(p => p.Price <= close)
                .OrderBy(p => close - p.Price)
                .ThenByDescending(p => p.Time)
                .Take(10)
                .ToList();

            // 6) decide where in HTFLevels to write
            int aboveOff, belowOff;
            if (seriesIndex == 2)
            {
                aboveOff = DailyAboveOff;    // 0
                belowOff = DailyBelowOff;    // 10
            }
            else if (seriesIndex == 3)
            {
                aboveOff = WeeklyAboveOff;   // 20
                belowOff = WeeklyBelowOff;   // 30
            }
            else if (seriesIndex == 4)
            {
                aboveOff = FourHourAboveOff; // 40
                belowOff = FourHourBelowOff; // 50
            }
            else
                return; // only HTF

            // 7) draw & populate the 10 slots
            for (int j = 0; j < 10; j++)
            {
                double upVal   = j < aboveList.Count ? aboveList[j].Price : double.NaN;
                double downVal = j < belowList.Count ? belowList[j].Price : double.NaN;

                // populate public arrays & plots for strategy access and DataBox
                HTFLevels[aboveOff + j][0] = upVal;
                HTFLevels[belowOff + j][0] = downVal;

                Values[aboveOff + j][0] = upVal;
                Values[belowOff + j][0] = downVal;

                // draw above-level line if valid
                if (!double.IsNaN(upVal))
                {
                    var p = aboveList[j];
                    Draw.Line(this,
                              $"{tagPrefix}HTF_{seriesIndex}_A_{j}",
                              false,
                              p.Time,  p.Price,
                              now,     p.Price,
                              brush,
                              GetStyleForSeries(seriesIndex),
                              GetLineThicknessForSeries(seriesIndex));
                }

                // draw below-level line if valid
                if (!double.IsNaN(downVal))
                {
                    var p = belowList[j];
                    Draw.Line(this,
                              $"{tagPrefix}HTF_{seriesIndex}_B_{j}",
                              false,
                              p.Time,  p.Price,
                              now,     p.Price,
                              brush,
                              GetStyleForSeries(seriesIndex),
                              GetLineThicknessForSeries(seriesIndex));
                }
            }
        }

        #endregion

        #region Helpers
        private Brush GetBrushForSeries(int seriesIndex)
        {
            switch (seriesIndex)
            {
                case 2: return Series5Color; // Daily
                case 3: return Series6Color; // Weekly
                case 4: return Series4Color; // 240m
                default: return Brushes.Gray;
            }
        }
        private DashStyleHelper GetStyleForSeries(int seriesIndex)
        {
            switch (seriesIndex)
            {
                case 2: return Series5Style;
                case 3: return Series6Style;
                case 4: return Series4Style;
                default: return DashStyleHelper.Dash;
            }
        }
        private int GetLineThicknessForSeries(int seriesIndex)
        {
            switch (seriesIndex)
            {
                case 2: return Series5Thickness;
                case 3: return Series6Thickness;
                case 4: return Series4Thickness;
                default: return 2;
            }
        }

        private SharpDX.Color4 ConvertMediaBrushToColor4(System.Windows.Media.Brush mediaBrush)
        {
            var scb = mediaBrush as System.Windows.Media.SolidColorBrush;
            if (scb == null)
                return new SharpDX.Color4(0.2f, 0.2f, 0.2f, 1f);

            float a = (float)scb.Color.A / 255f;
            float r = (float)scb.Color.R / 255f;
            float g = (float)scb.Color.G / 255f;
            float b = (float)scb.Color.B / 255f;
            return new SharpDX.Color4(r, g, b, a);
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlightenDynamicLevelsV3[] cacheAlightenDynamicLevelsV3;
		public AlightenDynamicLevelsV3 AlightenDynamicLevelsV3(bool enableSeries5SnRLevels, Brush series5Color, int series5Thickness, DashStyleHelper series5Style, bool enableSeries6SnRLevels, Brush series6Color, int series6Thickness, DashStyleHelper series6Style, bool enableSeries4SnRLevels, Brush series4Color, int series4Thickness, DashStyleHelper series4Style)
		{
			return AlightenDynamicLevelsV3(Input, enableSeries5SnRLevels, series5Color, series5Thickness, series5Style, enableSeries6SnRLevels, series6Color, series6Thickness, series6Style, enableSeries4SnRLevels, series4Color, series4Thickness, series4Style);
		}

		public AlightenDynamicLevelsV3 AlightenDynamicLevelsV3(ISeries<double> input, bool enableSeries5SnRLevels, Brush series5Color, int series5Thickness, DashStyleHelper series5Style, bool enableSeries6SnRLevels, Brush series6Color, int series6Thickness, DashStyleHelper series6Style, bool enableSeries4SnRLevels, Brush series4Color, int series4Thickness, DashStyleHelper series4Style)
		{
			if (cacheAlightenDynamicLevelsV3 != null)
				for (int idx = 0; idx < cacheAlightenDynamicLevelsV3.Length; idx++)
					if (cacheAlightenDynamicLevelsV3[idx] != null && cacheAlightenDynamicLevelsV3[idx].EnableSeries5SnRLevels == enableSeries5SnRLevels && cacheAlightenDynamicLevelsV3[idx].Series5Color == series5Color && cacheAlightenDynamicLevelsV3[idx].Series5Thickness == series5Thickness && cacheAlightenDynamicLevelsV3[idx].Series5Style == series5Style && cacheAlightenDynamicLevelsV3[idx].EnableSeries6SnRLevels == enableSeries6SnRLevels && cacheAlightenDynamicLevelsV3[idx].Series6Color == series6Color && cacheAlightenDynamicLevelsV3[idx].Series6Thickness == series6Thickness && cacheAlightenDynamicLevelsV3[idx].Series6Style == series6Style && cacheAlightenDynamicLevelsV3[idx].EnableSeries4SnRLevels == enableSeries4SnRLevels && cacheAlightenDynamicLevelsV3[idx].Series4Color == series4Color && cacheAlightenDynamicLevelsV3[idx].Series4Thickness == series4Thickness && cacheAlightenDynamicLevelsV3[idx].Series4Style == series4Style && cacheAlightenDynamicLevelsV3[idx].EqualsInput(input))
						return cacheAlightenDynamicLevelsV3[idx];
			return CacheIndicator<AlightenDynamicLevelsV3>(new AlightenDynamicLevelsV3(){ EnableSeries5SnRLevels = enableSeries5SnRLevels, Series5Color = series5Color, Series5Thickness = series5Thickness, Series5Style = series5Style, EnableSeries6SnRLevels = enableSeries6SnRLevels, Series6Color = series6Color, Series6Thickness = series6Thickness, Series6Style = series6Style, EnableSeries4SnRLevels = enableSeries4SnRLevels, Series4Color = series4Color, Series4Thickness = series4Thickness, Series4Style = series4Style }, input, ref cacheAlightenDynamicLevelsV3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlightenDynamicLevelsV3 AlightenDynamicLevelsV3(bool enableSeries5SnRLevels, Brush series5Color, int series5Thickness, DashStyleHelper series5Style, bool enableSeries6SnRLevels, Brush series6Color, int series6Thickness, DashStyleHelper series6Style, bool enableSeries4SnRLevels, Brush series4Color, int series4Thickness, DashStyleHelper series4Style)
		{
			return indicator.AlightenDynamicLevelsV3(Input, enableSeries5SnRLevels, series5Color, series5Thickness, series5Style, enableSeries6SnRLevels, series6Color, series6Thickness, series6Style, enableSeries4SnRLevels, series4Color, series4Thickness, series4Style);
		}

		public Indicators.AlightenDynamicLevelsV3 AlightenDynamicLevelsV3(ISeries<double> input , bool enableSeries5SnRLevels, Brush series5Color, int series5Thickness, DashStyleHelper series5Style, bool enableSeries6SnRLevels, Brush series6Color, int series6Thickness, DashStyleHelper series6Style, bool enableSeries4SnRLevels, Brush series4Color, int series4Thickness, DashStyleHelper series4Style)
		{
			return indicator.AlightenDynamicLevelsV3(input, enableSeries5SnRLevels, series5Color, series5Thickness, series5Style, enableSeries6SnRLevels, series6Color, series6Thickness, series6Style, enableSeries4SnRLevels, series4Color, series4Thickness, series4Style);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlightenDynamicLevelsV3 AlightenDynamicLevelsV3(bool enableSeries5SnRLevels, Brush series5Color, int series5Thickness, DashStyleHelper series5Style, bool enableSeries6SnRLevels, Brush series6Color, int series6Thickness, DashStyleHelper series6Style, bool enableSeries4SnRLevels, Brush series4Color, int series4Thickness, DashStyleHelper series4Style)
		{
			return indicator.AlightenDynamicLevelsV3(Input, enableSeries5SnRLevels, series5Color, series5Thickness, series5Style, enableSeries6SnRLevels, series6Color, series6Thickness, series6Style, enableSeries4SnRLevels, series4Color, series4Thickness, series4Style);
		}

		public Indicators.AlightenDynamicLevelsV3 AlightenDynamicLevelsV3(ISeries<double> input , bool enableSeries5SnRLevels, Brush series5Color, int series5Thickness, DashStyleHelper series5Style, bool enableSeries6SnRLevels, Brush series6Color, int series6Thickness, DashStyleHelper series6Style, bool enableSeries4SnRLevels, Brush series4Color, int series4Thickness, DashStyleHelper series4Style)
		{
			return indicator.AlightenDynamicLevelsV3(input, enableSeries5SnRLevels, series5Color, series5Thickness, series5Style, enableSeries6SnRLevels, series6Color, series6Thickness, series6Style, enableSeries4SnRLevels, series4Color, series4Thickness, series4Style);
		}
	}
}

#endregion
