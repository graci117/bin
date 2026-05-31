#region References

using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using System;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.BFT.CHIEF
{
    public enum HAOverlayColoringMode { ProfitWave, Trend }

    public class HAOverlay : Indicator
    {
        // ── Constants ────────────────────────────────────────────────────────
        private const int    PERIOD_EMA_SLOW = 21;
        private const int    PERIOD_ATR      = 8;
        private const double ATR_MULT        = 1.3;

        // ── HA series ────────────────────────────────────────────────────────
        private Series<double> _haOpen, _haHigh, _haLow, _haClose;

        // ── Calculation series ───────────────────────────────────────────────
        private Series<double> _emaSlowSeries;    // EMA(21) of HA close (coloring)
        private Series<double> _atrSeries;        // Wilder ATR(8)
        private Series<double> _trendUp, _trendDown;
        private Series<int>    _trendSwitch;
        private Series<int>    _candleColor;      // 1=bull, -1=bear, 0=neutral

        // ── DirectX brushes ──────────────────────────────────────────────────
        private SharpDX.Direct2D1.Brush _dxBull, _dxBear, _dxNeutral, _dxDot;

        // ── Plot indices ─────────────────────────────────────────────────────
        private const int IDX_FILTER_EMA   = 0;  // visible EMA line
        private const int IDX_HA_OPEN      = 1;  // transparent, data box only
        private const int IDX_HA_HIGH      = 2;
        private const int IDX_HA_LOW       = 3;
        private const int IDX_HA_CLOSE     = 4;
        private const int IDX_LONG_SIGNAL  = 5;
        private const int IDX_SHORT_SIGNAL = 6;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description              = "HA candles colored with ProfitWave / Trend rules, filter EMA, and trade alerts.";
                Name                     = "HA Overlay";
                Calculate                = Calculate.OnBarClose;
                IsOverlay                = true;
                DrawOnPricePanel         = true;
                MaximumBarsLookBack      = MaximumBarsLookBack.Infinite;
                IsSuspendedWhileInactive = true;
                DisplayInDataBox         = true;
                ShowTransparentPlotsInDataBox = true;

                // ── Defaults ─────────────────────────────────────────────────
                BullishColor    = new SolidColorBrush(Color.FromRgb(  8, 153, 129)); BullishColor.Freeze();
                BearishColor    = new SolidColorBrush(Color.FromRgb(242,  54,  69)); BearishColor.Freeze();
                ColoringMode    = HAOverlayColoringMode.ProfitWave;
                DotRadius       = 3f;
                DotColor        = new SolidColorBrush(Color.FromRgb(255, 255, 255)); DotColor.Freeze();
                FilterEmaPeriod = 200;
                FilterEmaColor  = new SolidColorBrush(Color.FromRgb(148,   0, 211)); FilterEmaColor.Freeze(); // DarkViolet
                FilterEmaWidth  = 2;
                LongArrowColor  = new SolidColorBrush(Color.FromRgb(  0, 200, 100)); LongArrowColor.Freeze();
                ShortArrowColor = new SolidColorBrush(Color.FromRgb(255,  60,  60)); ShortArrowColor.Freeze();

                // ── Plots ─────────────────────────────────────────────────────
                // IDX 0 – Filter EMA: NinjaTrader draws this as a clean line natively
                AddPlot(new Stroke(Brushes.DarkViolet, 2), PlotStyle.Line, "Filter EMA");

                // IDX 1-4 – HA OHLC: transparent, show in data box only
                AddPlot(Brushes.Transparent, "HA Open");
                AddPlot(Brushes.Transparent, "HA High");
                AddPlot(Brushes.Transparent, "HA Low");
                AddPlot(Brushes.Transparent, "HA Close");

                // IDX 5-6 – Signal flags: 1.0 = signal, NaN = none
                AddPlot(Brushes.Transparent, "Long Signal");
                AddPlot(Brushes.Transparent, "Short Signal");
            }
            else if (State == State.DataLoaded)
            {
                _haOpen        = new Series<double>(this);
                _haHigh        = new Series<double>(this);
                _haLow         = new Series<double>(this);
                _haClose       = new Series<double>(this);
                _emaSlowSeries = new Series<double>(this);
                _atrSeries     = new Series<double>(this);
                _trendUp       = new Series<double>(this);
                _trendDown     = new Series<double>(this);
                _trendSwitch   = new Series<int>(this);
                _candleColor   = new Series<int>(this);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void OnBarUpdate()
        {
            // ── 1. Heikin Ashi ───────────────────────────────────────────────
            _haClose[0] = (Open[0] + High[0] + Low[0] + Close[0]) * 0.25;

            if (CurrentBar == 0)
            {
                _haOpen[0] = Open[0];
                _haHigh[0] = High[0];
                _haLow[0]  = Low[0];
            }
            else
            {
                _haOpen[0] = (_haOpen[1] + _haClose[1]) * 0.5;
                _haHigh[0] = Math.Max(High[0], _haOpen[0]);
                _haLow[0]  = Math.Min(Low[0],  _haOpen[0]);
            }

            Values[IDX_HA_OPEN][0]  = _haOpen[0];
            Values[IDX_HA_HIGH][0]  = _haHigh[0];
            Values[IDX_HA_LOW][0]   = _haLow[0];
            Values[IDX_HA_CLOSE][0] = _haClose[0];

            // ── 2. EMA(21) of HA close (ProfitWave coloring) ─────────────────
            double k = 2.0 / (1 + PERIOD_EMA_SLOW);
            _emaSlowSeries[0] = (CurrentBar == 0)
                ? _haClose[0]
                : _haClose[0] * k + _emaSlowSeries[1] * (1 - k);

            // ── 3. ATR(8) Wilder ─────────────────────────────────────────────
            if (CurrentBar == 0)
            {
                _atrSeries[0] = _haHigh[0] - _haLow[0];
            }
            else
            {
                double tr = Math.Max(
                    Math.Max(_haHigh[0] - _haLow[0],
                             Math.Abs(_haHigh[0] - _haClose[1])),
                    Math.Abs(_haLow[0]  - _haClose[1]));
                int lookback = Math.Min(CurrentBar + 1, PERIOD_ATR);
                _atrSeries[0] = ((lookback - 1) * _atrSeries[1] + tr) / lookback;
            }

            // ── 4. TrendSwitch (Chandelier-style) ────────────────────────────
            double mid = (_haHigh[0] + _haLow[0]) / 2.0;
            double Up  = mid - ATR_MULT * _atrSeries[0];
            double Dn  = mid + ATR_MULT * _atrSeries[0];

            if (CurrentBar == 0)
            {
                _trendUp[0]     = Up;
                _trendDown[0]   = Dn;
                _trendSwitch[0] = 1;
            }
            else
            {
                _trendUp[0]   = (_haClose[1] > _trendUp[1]   ? Math.Max(Up, _trendUp[1])   : Up);
                _trendDown[0] = (_haClose[1] < _trendDown[1] ? Math.Min(Dn, _trendDown[1]) : Dn);
                _trendSwitch[0] = (_haClose[0] > _trendDown[1] ?  1 :
                                   _haClose[0] < _trendUp[1]   ? -1 :
                                   _trendSwitch[1]);
            }

            bool bullishTrend = (_trendSwitch[0] ==  1);
            bool bearishTrend = (_trendSwitch[0] == -1);

            // ── 5. Base candle color ─────────────────────────────────────────
            if      (_haClose[0] > _haOpen[0]) _candleColor[0] =  1;
            else if (_haClose[0] < _haOpen[0]) _candleColor[0] = -1;
            else if (CurrentBar > 0)           _candleColor[0] =  _candleColor[1];
            else                               _candleColor[0] =  0;

            // ── 6. Coloring mode override ────────────────────────────────────
            if (ColoringMode == HAOverlayColoringMode.Trend)
            {
                if      (bullishTrend)   _candleColor[0] =  1;
                else if (bearishTrend)   _candleColor[0] = -1;
                else if (CurrentBar > 0) _candleColor[0] =  _candleColor[1];
            }
            else // ProfitWave: HA close vs EMA(21)
            {
                if      (_haClose[0] > _emaSlowSeries[0]) _candleColor[0] =  1;
                else if (_haClose[0] < _emaSlowSeries[0]) _candleColor[0] = -1;
                else if (CurrentBar > 0)                   _candleColor[0] =  _candleColor[1];
            }

            // Hide real candles – only our custom HA render is visible
            BarBrushes[0]           = Brushes.Transparent;
            CandleOutlineBrushes[0] = Brushes.Transparent;

            // ── 7. Filter EMA(N) of real Close – pushed to plot ──────────────
            double kf = 2.0 / (1 + FilterEmaPeriod);
            Values[IDX_FILTER_EMA][0] = (CurrentBar == 0)
                ? Close[0]
                : Close[0] * kf + Values[IDX_FILTER_EMA][1] * (1 - kf);

            // Sync the plot stroke to the user-chosen color and width
            PlotBrushes[IDX_FILTER_EMA][0] = FilterEmaColor;
            Plots[IDX_FILTER_EMA].Width     = FilterEmaWidth;

            // ── 8. Signal detection ──────────────────────────────────────────
            if (CurrentBar < 2)
            {
                Values[IDX_LONG_SIGNAL][0]  = double.NaN;
                Values[IDX_SHORT_SIGNAL][0] = double.NaN;
                return;
            }

            int prevColor = _candleColor[1];
            int currColor = _candleColor[0];

            bool aboveEma = Close[0] > Values[IDX_FILTER_EMA][0];
            bool belowEma = Close[0] < Values[IDX_FILTER_EMA][0];

            bool longAlert  = (prevColor == -1 && currColor ==  1 && aboveEma);
            bool shortAlert = (prevColor ==  1 && currColor == -1 && belowEma);

            Values[IDX_LONG_SIGNAL][0]  = longAlert  ? 1.0 : double.NaN;
            Values[IDX_SHORT_SIGNAL][0] = shortAlert ? 1.0 : double.NaN;

            // ── 9. Draw / remove arrows ──────────────────────────────────────
            double offset = 2.0 * TickSize;

            if (longAlert)
                Draw.ArrowUp(this, "LongAlert_" + CurrentBar, false, 0,
                             _haLow[0] - offset, LongArrowColor);
            else
                RemoveDrawObject("LongAlert_" + CurrentBar);

            if (shortAlert)
                Draw.ArrowDown(this, "ShortAlert_" + CurrentBar, false, 0,
                               _haHigh[0] + offset, ShortArrowColor);
            else
                RemoveDrawObject("ShortAlert_" + CurrentBar);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // OnRenderTargetChanged – (re)create DirectX brushes
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public override void OnRenderTargetChanged()
        {
            base.OnRenderTargetChanged();

            if (_dxBull    != null) { _dxBull.Dispose();    _dxBull    = null; }
            if (_dxBear    != null) { _dxBear.Dispose();    _dxBear    = null; }
            if (_dxNeutral != null) { _dxNeutral.Dispose(); _dxNeutral = null; }
            if (_dxDot     != null) { _dxDot.Dispose();     _dxDot     = null; }

            if (RenderTarget == null) return;

            _dxBull    = BullishColor.ToDxBrush(RenderTarget);
            _dxBear    = BearishColor.ToDxBrush(RenderTarget);
            _dxNeutral = Brushes.Gray.ToDxBrush(RenderTarget);
            _dxDot     = DotColor.ToDxBrush(RenderTarget);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // OnRender – HA candles and close dot only
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        protected override void OnRender(ChartControl cc, ChartScale cs)
        {
            base.OnRender(cc, cs);

            if (_dxBull == null || _dxBear == null || _dxNeutral == null || _dxDot == null) return;

            int   barPaintWidth = cc.GetBarPaintWidth(cc.BarsArray[0]) - 1;
            float dotRadius     = Math.Max(2f, DotRadius);

            SharpDX.RectangleF rect = new SharpDX.RectangleF();
            SharpDX.Vector2    v1   = new SharpDX.Vector2();
            SharpDX.Vector2    v2   = new SharpDX.Vector2();

            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
                int adjIdx = idx - Displacement;
                if (adjIdx < 0 || adjIdx >= BarsArray[0].Count || adjIdx < BarsRequiredToPlot)
                    continue;

                double haO = _haOpen.GetValueAt(adjIdx);
                double haH = _haHigh.GetValueAt(adjIdx);
                double haL = _haLow.GetValueAt(adjIdx);
                double haC = _haClose.GetValueAt(adjIdx);

                if (double.IsNaN(haO) || double.IsNaN(haH) || double.IsNaN(haL) || double.IsNaN(haC))
                    continue;

                int col = _candleColor.GetValueAt(adjIdx);
                SharpDX.Direct2D1.Brush brush = (col ==  1 ? _dxBull :
                                                 col == -1 ? _dxBear : _dxNeutral);

                int barX     = cc.GetXByBarIndex(cc.BarsArray[0], idx);
                int barLeftX = barX - barPaintWidth / 2;

                int yOpen  = cs.GetYByValue(haO);
                int yHigh  = cs.GetYByValue(haH);
                int yLow   = cs.GetYByValue(haL);
                int yClose = cs.GetYByValue(haC);

                // Wick
                v1.X = barX; v1.Y = yHigh;
                v2.X = barX; v2.Y = yLow;
                RenderTarget.DrawLine(v1, v2, brush, 1);

                if (yClose == yOpen)
                {
                    // Doji
                    v1.X = barLeftX - 1;                 v1.Y = yOpen;
                    v2.X = barX + barPaintWidth / 2 - 1; v2.Y = yOpen;
                    RenderTarget.DrawLine(v1, v2, brush, 1);
                }
                else
                {
                    // Body fill
                    int bodyTop    = Math.Min(yOpen, yClose);
                    int bodyHeight = Math.Abs(yClose - yOpen);
                    rect.X = barLeftX; rect.Y = bodyTop; rect.Width = barPaintWidth - 1; rect.Height = bodyHeight;
                    RenderTarget.FillRectangle(rect, brush);

                    // Body outline
                    rect.X = barLeftX - (1 / 2); rect.Y = bodyTop; rect.Width = barPaintWidth - (1 / 2); rect.Height = bodyHeight;
                    RenderTarget.DrawRectangle(rect, brush, 1);
                }

                // Close dot – positioned at the REAL close price of the primary chart bar
                double realClose  = BarsArray[0].GetClose(adjIdx);
                int    yRealClose = cs.GetYByValue(realClose);
                var ellipse = new SharpDX.Direct2D1.Ellipse(
                    new SharpDX.Vector2(barX, yRealClose), dotRadius, dotRadius);
                RenderTarget.FillEllipse(ellipse, _dxDot);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Properties
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // ── Candle Settings ──────────────────────────────────────────────────

        [Display(Name = "Coloring Mode", Order = 1, GroupName = "Candle Settings",
                 Description = "ProfitWave: color by HA close vs EMA(21). Trend: color by TrendSwitch direction.")]
        public HAOverlayColoringMode ColoringMode { get; set; }

        [XmlIgnore]
        [Display(Name = "Bullish Candle Color", Order = 2, GroupName = "Candle Settings")]
        public Brush BullishColor { get; set; }

        [Browsable(false)]
        public string BullishColor_Serialize
        {
            get { return Serialize.BrushToString(BullishColor); }
            set { BullishColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Bearish Candle Color", Order = 3, GroupName = "Candle Settings")]
        public Brush BearishColor { get; set; }

        [Browsable(false)]
        public string BearishColor_Serialize
        {
            get { return Serialize.BrushToString(BearishColor); }
            set { BearishColor = Serialize.StringToBrush(value); }
        }

        [Display(Name = "Close Dot Radius", Order = 4, GroupName = "Candle Settings",
                 Description = "Radius in pixels of the dot at each HA close. Minimum enforced at 2px.")]
        public float DotRadius { get; set; }

        [XmlIgnore]
        [Display(Name = "Close Dot Color", Order = 5, GroupName = "Candle Settings")]
        public Brush DotColor { get; set; }

        [Browsable(false)]
        public string DotColor_Serialize
        {
            get { return Serialize.BrushToString(DotColor); }
            set { DotColor = Serialize.StringToBrush(value); }
        }

        // ── Filter EMA ───────────────────────────────────────────────────────

        [Display(Name = "Filter EMA Period", Order = 1, GroupName = "Filter EMA",
                 Description = "Period of the EMA filter (based on real Close). Default 200.")]
        public int FilterEmaPeriod { get; set; }

        [XmlIgnore]
        [Display(Name = "Filter EMA Color", Order = 2, GroupName = "Filter EMA",
                 Description = "Color of the EMA line. Change takes effect on next bar.")]
        public Brush FilterEmaColor { get; set; }

        [Browsable(false)]
        public string FilterEmaColor_Serialize
        {
            get { return Serialize.BrushToString(FilterEmaColor); }
            set { FilterEmaColor = Serialize.StringToBrush(value); }
        }

        [Display(Name = "Filter EMA Width", Order = 3, GroupName = "Filter EMA",
                 Description = "Stroke width in pixels of the EMA line. Default 2.")]
        public int FilterEmaWidth { get; set; }

        // ── Trade Alerts ─────────────────────────────────────────────────────

        [XmlIgnore]
        [Display(Name = "Long Arrow Color", Order = 1, GroupName = "Trade Alerts",
                 Description = "Up arrow: prev bar bearish → current bar bullish + price above filter EMA.")]
        public Brush LongArrowColor { get; set; }

        [Browsable(false)]
        public string LongArrowColor_Serialize
        {
            get { return Serialize.BrushToString(LongArrowColor); }
            set { LongArrowColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Short Arrow Color", Order = 2, GroupName = "Trade Alerts",
                 Description = "Down arrow: prev bar bullish → current bar bearish + price below filter EMA.")]
        public Brush ShortArrowColor { get; set; }

        [Browsable(false)]
        public string ShortArrowColor_Serialize
        {
            get { return Serialize.BrushToString(ShortArrowColor); }
            set { ShortArrowColor = Serialize.StringToBrush(value); }
        }

        // ── Public series accessors ──────────────────────────────────────────

        [Browsable(false)] [XmlIgnore]
        public Series<double> FilterEma   => Values[IDX_FILTER_EMA];

        [Browsable(false)] [XmlIgnore]
        public Series<double> HaOpen      => Values[IDX_HA_OPEN];

        [Browsable(false)] [XmlIgnore]
        public Series<double> HaHigh      => Values[IDX_HA_HIGH];

        [Browsable(false)] [XmlIgnore]
        public Series<double> HaLow       => Values[IDX_HA_LOW];

        [Browsable(false)] [XmlIgnore]
        public Series<double> HaClose     => Values[IDX_HA_CLOSE];

        [Browsable(false)] [XmlIgnore]
        public Series<double> LongSignal  => Values[IDX_LONG_SIGNAL];

        [Browsable(false)] [XmlIgnore]
        public Series<double> ShortSignal => Values[IDX_SHORT_SIGNAL];
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BFT.CHIEF.HAOverlay[] cacheHAOverlay;
		public BFT.CHIEF.HAOverlay HAOverlay()
		{
			return HAOverlay(Input);
		}

		public BFT.CHIEF.HAOverlay HAOverlay(ISeries<double> input)
		{
			if (cacheHAOverlay != null)
				for (int idx = 0; idx < cacheHAOverlay.Length; idx++)
					if (cacheHAOverlay[idx] != null &&  cacheHAOverlay[idx].EqualsInput(input))
						return cacheHAOverlay[idx];
			return CacheIndicator<BFT.CHIEF.HAOverlay>(new BFT.CHIEF.HAOverlay(), input, ref cacheHAOverlay);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BFT.CHIEF.HAOverlay HAOverlay()
		{
			return indicator.HAOverlay(Input);
		}

		public Indicators.BFT.CHIEF.HAOverlay HAOverlay(ISeries<double> input )
		{
			return indicator.HAOverlay(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BFT.CHIEF.HAOverlay HAOverlay()
		{
			return indicator.HAOverlay(Input);
		}

		public Indicators.BFT.CHIEF.HAOverlay HAOverlay(ISeries<double> input )
		{
			return indicator.HAOverlay(input);
		}
	}
}

#endregion
