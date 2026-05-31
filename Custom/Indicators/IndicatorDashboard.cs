#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class IndicatorDashboard : Indicator
    {
        // ─── Configurable parameters ───────────────────────────────────────
        private int    _panelHeight    = 0;   // auto-sized
        private double _fontSize       = 12;
        private double _rowHeight      = 18;
        private double _colWidth       = 220;
        private double _padding        = 6;

        // ─── Brushes ───────────────────────────────────────────────────────
        private SolidColorBrush _bgBrush;
        private SolidColorBrush _headerBrush;
        private SolidColorBrush _textBrush;
        private SolidColorBrush _altRowBrush;
        private SolidColorBrush _borderBrush;
        private SolidColorBrush _naTextBrush;

        // ─── Cached reflection data refreshed each render ─────────────────
        private struct PlotEntry
        {
            public string IndicatorName;
            public string PlotName;
            public Brush  PlotBrush;
            public string Value;
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Scans all indicators on the chart via reflection and displays their plot names and current-bar values in a floating panel.";
                Name        = "Indicator Dashboard";
                Calculate   = Calculate.OnBarClose;
                IsOverlay   = false;
                DisplayInDataBox = false;
                DrawOnPricePanel = false;
                IsSuspendedWhileInactive = true;
            }
            else if (State == State.Configure)
            {
                // force a dedicated panel
                Panel = 2;
            }
            else if (State == State.Historical || State == State.Realtime)
            {
                InitBrushes();
            }
        }

        private void InitBrushes()
        {
            _bgBrush     = new SolidColorBrush(Color.FromArgb(230, 15,  15,  20 )); _bgBrush.Freeze();
            _headerBrush = new SolidColorBrush(Color.FromArgb(255, 30,  30,  45 )); _headerBrush.Freeze();
            _altRowBrush = new SolidColorBrush(Color.FromArgb(40,  255, 255, 255)); _altRowBrush.Freeze();
            _borderBrush = new SolidColorBrush(Color.FromArgb(120, 80,  130, 200)); _borderBrush.Freeze();
            _textBrush   = new SolidColorBrush(Colors.WhiteSmoke); _textBrush.Freeze();
            _naTextBrush = new SolidColorBrush(Color.FromArgb(160, 180, 180, 180)); _naTextBrush.Freeze();
        }

        protected override void OnBarUpdate() { }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);
            if (chartControl == null || ChartPanel == null) return;

            var entries = CollectPlotEntries(chartControl);

            // ── Layout ────────────────────────────────────────────────────
            double x       = _padding;
            double y       = _padding;
            double panelW  = ChartPanel.W;
            double colW    = Math.Max(_colWidth, (panelW - _padding * 2) / Math.Max(1, GetColumnCount(entries)));

            double totalH  = _padding * 2 + _rowHeight + entries.Count * _rowHeight + 4;
            double panelH  = Math.Max(totalH, 60);

            // ── Background ────────────────────────────────────────────────
            RenderTarget.FillRectangle(
                new SharpDX.RectangleF((float)x, (float)y, (float)(panelW - _padding * 2), (float)panelH),
                _bgBrush.ToDxBrush(RenderTarget));

            // ── Border ────────────────────────────────────────────────────
            RenderTarget.DrawRectangle(
                new SharpDX.RectangleF((float)x, (float)y, (float)(panelW - _padding * 2), (float)panelH),
                _borderBrush.ToDxBrush(RenderTarget), 1.5f);

            // ── Header row ────────────────────────────────────────────────
            RenderTarget.FillRectangle(
                new SharpDX.RectangleF((float)x, (float)y, (float)(panelW - _padding * 2), (float)_rowHeight),
                _headerBrush.ToDxBrush(RenderTarget));

            DrawText(chartControl, "Indicator",  x + _padding, y + 2, _textBrush, true);
            DrawText(chartControl, "Plot",       x + _padding + colW * 0.4, y + 2, _textBrush, true);
            DrawText(chartControl, "Value",      x + _padding + colW * 0.75, y + 2, _textBrush, true);

            y += _rowHeight + 2;

            // ── Data rows ─────────────────────────────────────────────────
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];

                // alternating row shading
                if (i % 2 == 1)
                {
                    RenderTarget.FillRectangle(
                        new SharpDX.RectangleF((float)x, (float)y, (float)(panelW - _padding * 2), (float)_rowHeight),
                        _altRowBrush.ToDxBrush(RenderTarget));
                }

                // colored indicator name
                var nameBrush = e.PlotBrush as SolidColorBrush ?? _textBrush;
                DrawText(chartControl, TruncateStr(e.IndicatorName, 18), x + _padding, y + 1, nameBrush, false);
                DrawText(chartControl, TruncateStr(e.PlotName, 14),      x + _padding + colW * 0.42, y + 1, _textBrush, false);

                var valueBrush = e.Value == "N/A" ? _naTextBrush : _textBrush;
                DrawText(chartControl, e.Value, x + _padding + colW * 0.77, y + 1, valueBrush, false);

                y += _rowHeight;
            }

            if (entries.Count == 0)
                DrawText(chartControl, "No indicators with plots found on this chart.", x + _padding * 2, y, _naTextBrush, false);
        }

        // ─── Reflection core ──────────────────────────────────────────────
        private List<PlotEntry> CollectPlotEntries(ChartControl chartControl)
        {
            var result = new List<PlotEntry>();

            try
            {
                // ChartControl.Indicators returns IEnumerable of NinjaScriptBase
                var indProp = chartControl.GetType()
                    .GetProperty("Indicators", BindingFlags.Public | BindingFlags.Instance);
                if (indProp == null) return result;

                var indicators = indProp.GetValue(chartControl) as System.Collections.IEnumerable;
                if (indicators == null) return result;

                foreach (var ind in indicators)
                {
                    if (ind == null) continue;

                    // Skip ourselves
                    if (ind is IndicatorDashboard) continue;

                    Type indType   = ind.GetType();
                    string indName = GetIndicatorDisplayName(ind, indType);

                    // Try to get Plots property (Plot[] or IList<Plot>)
                    var plotsProp = indType.GetProperty("Plots",
                        BindingFlags.Public | BindingFlags.Instance);

                    // Also try Values (Series<double>[])
                    var valuesProp = indType.GetProperty("Values",
                        BindingFlags.Public | BindingFlags.Instance);

                    object[] plotsArr = null;
                    if (plotsProp != null)
                    {
                        try
                        {
                            var raw = plotsProp.GetValue(ind) as System.Collections.IList;
                            if (raw != null)
                            {
                                plotsArr = new object[raw.Count];
                                for (int i = 0; i < raw.Count; i++) plotsArr[i] = raw[i];
                            }
                        }
                        catch { }
                    }

                    object[] valuesArr = null;
                    if (valuesProp != null)
                    {
                        try
                        {
                            var raw = valuesProp.GetValue(ind) as System.Collections.IList;
                            if (raw != null)
                            {
                                valuesArr = new object[raw.Count];
                                for (int i = 0; i < raw.Count; i++) valuesArr[i] = raw[i];
                            }
                        }
                        catch { }
                    }

                    // If no Plots collection found, try iterating public Series<double> properties
                    if ((plotsArr == null || plotsArr.Length == 0))
                    {
                        CollectSeriesProperties(ind, indType, indName, result);
                        continue;
                    }

                    int plotCount = plotsArr?.Length ?? 0;

                    for (int pi = 0; pi < plotCount; pi++)
                    {
                        var plot     = plotsArr[pi];
                        string pName = GetPlotName(plot, pi);
                        Brush  pBrush = GetPlotBrush(plot);
                        string val   = GetSeriesValue(valuesArr, pi);

                        result.Add(new PlotEntry
                        {
                            IndicatorName = indName,
                            PlotName      = pName,
                            PlotBrush     = pBrush,
                            Value         = val
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add(new PlotEntry
                {
                    IndicatorName = "Reflection Error",
                    PlotName      = "",
                    PlotBrush     = Brushes.OrangeRed,
                    Value         = ex.Message.Length > 30 ? ex.Message.Substring(0, 30) : ex.Message
                });
            }

            return result;
        }

        // Fallback: enumerate public properties that look like Series<double>
        private void CollectSeriesProperties(object ind, Type indType, string indName, List<PlotEntry> result)
        {
            try
            {
                var props = indType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    // Look for ISeries<double> or Series<double>
                    var pType = prop.PropertyType;
                    bool isSeries = IsSeriesDouble(pType);
                    if (!isSeries) continue;

                    try
                    {
                        var series = prop.GetValue(ind);
                        if (series == null) continue;
                        string val = GetSeriesCurrentValue(series);

                        result.Add(new PlotEntry
                        {
                            IndicatorName = indName,
                            PlotName      = prop.Name,
                            PlotBrush     = Brushes.CornflowerBlue,
                            Value         = val
                        });
                    }
                    catch { }
                }
            }
            catch { }
        }

        private bool IsSeriesDouble(Type t)
        {
            if (t == null) return false;
            if (t.Name.Contains("Series") && (t.Name.Contains("Double") || t.Name.Contains("double"))) return true;
            if (t.IsGenericType)
            {
                var args = t.GetGenericArguments();
                if (args.Length == 1 && args[0] == typeof(double)) return true;
            }
            // Check interfaces
            foreach (var iface in t.GetInterfaces())
            {
                if (iface.IsGenericType)
                {
                    var args = iface.GetGenericArguments();
                    if (args.Length == 1 && args[0] == typeof(double)) return true;
                }
            }
            return false;
        }

        // ─── Reflection helpers ───────────────────────────────────────────
        private string GetIndicatorDisplayName(object ind, Type indType)
        {
            try
            {
                var nameProp = indType.GetProperty("Name",
                    BindingFlags.Public | BindingFlags.Instance);
                if (nameProp != null)
                {
                    var val = nameProp.GetValue(ind) as string;
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            catch { }
            return indType.Name;
        }

        private string GetPlotName(object plot, int index)
        {
            if (plot == null) return "Plot " + index;
            try
            {
                var nameProp = plot.GetType().GetProperty("Name",
                    BindingFlags.Public | BindingFlags.Instance);
                if (nameProp != null)
                {
                    var val = nameProp.GetValue(plot) as string;
                    if (!string.IsNullOrEmpty(val)) return val;
                }
            }
            catch { }
            return "Plot " + index;
        }

        private Brush GetPlotBrush(object plot)
        {
            if (plot == null) return _textBrush;
            try
            {
                // Try Brush property first
                var brushProp = plot.GetType().GetProperty("Brush",
                    BindingFlags.Public | BindingFlags.Instance);
                if (brushProp != null)
                {
                    var b = brushProp.GetValue(plot) as Brush;
                    if (b != null) return b;
                }
                // Fallback: BrushDX or PlotBrush
                foreach (string pname in new[] { "BrushDX", "PlotBrush", "Color" })
                {
                    var pp = plot.GetType().GetProperty(pname,
                        BindingFlags.Public | BindingFlags.Instance);
                    if (pp == null) continue;
                    var b = pp.GetValue(plot) as Brush;
                    if (b != null) return b;
                }
            }
            catch { }
            return _textBrush;
        }

        private string GetSeriesValue(object[] valuesArr, int index)
        {
            if (valuesArr == null || index >= valuesArr.Length) return "N/A";
            return GetSeriesCurrentValue(valuesArr[index]);
        }

        private string GetSeriesCurrentValue(object series)
        {
            if (series == null) return "N/A";
            try
            {
                // Series<double> indexer: series[0] = current bar value
                var indexer = series.GetType().GetProperty("Item",
                    BindingFlags.Public | BindingFlags.Instance);
                if (indexer != null)
                {
                    var val = indexer.GetValue(series, new object[] { 0 });
                    if (val is double d)
                    {
                        if (double.IsNaN(d) || double.IsInfinity(d)) return "N/A";
                        return FormatValue(d);
                    }
                }
                // Try Count to verify series is populated
                var countProp = series.GetType().GetProperty("Count",
                    BindingFlags.Public | BindingFlags.Instance);
                if (countProp != null)
                {
                    int cnt = (int)countProp.GetValue(series);
                    if (cnt == 0) return "N/A";
                }
            }
            catch { }
            return "N/A";
        }

        private string FormatValue(double val)
        {
            if (Math.Abs(val) >= 10000)     return val.ToString("N0");
            if (Math.Abs(val) >= 100)       return val.ToString("N2");
            if (Math.Abs(val) >= 1)         return val.ToString("N4");
            return val.ToString("N6");
        }

        private int GetColumnCount(List<PlotEntry> entries) => 1;

        private string TruncateStr(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }

        // ─── DirectX text helper ──────────────────────────────────────────
        private void DrawText(ChartControl cc, string text, double x, double y, Brush brush, bool bold)
        {
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                var tf = new SharpDX.DirectWrite.TextFormat(
                    Core.Globals.DirectWriteFactory,
                    "Consolas",
                    bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal,
                    SharpDX.DirectWrite.FontStyle.Normal,
                    (float)_fontSize);

                var layout = new SharpDX.DirectWrite.TextLayout(
                    Core.Globals.DirectWriteFactory,
                    text, tf,
                    400, (float)_rowHeight);

                SharpDX.Direct2D1.Brush dxBrush;
                if (brush is SolidColorBrush scb)
                {
                    var c = scb.Color;
                    dxBrush = new SharpDX.Direct2D1.SolidColorBrush(
                        RenderTarget,
                        new SharpDX.Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f));
                }
                else
                {
                    dxBrush = new SharpDX.Direct2D1.SolidColorBrush(
                        RenderTarget,
                        new SharpDX.Color4(1f, 1f, 1f, 1f));
                }

                RenderTarget.DrawTextLayout(
                    new SharpDX.Vector2((float)x, (float)y),
                    layout, dxBrush);

                tf.Dispose();
                layout.Dispose();
                dxBrush.Dispose();
            }
            catch { }
        }

        // ─── Properties ───────────────────────────────────────────────────
        [NinjaScriptProperty]
        [Display(Name = "Font Size", Order = 1, GroupName = "Dashboard Settings")]
        public double FontSize
        {
            get => _fontSize;
            set => _fontSize = Math.Max(8, value);
        }

        [NinjaScriptProperty]
        [Display(Name = "Row Height (px)", Order = 2, GroupName = "Dashboard Settings")]
        public double RowHeight
        {
            get => _rowHeight;
            set => _rowHeight = Math.Max(14, value);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IndicatorDashboard[] cacheIndicatorDashboard;
		public IndicatorDashboard IndicatorDashboard(double fontSize, double rowHeight)
		{
			return IndicatorDashboard(Input, fontSize, rowHeight);
		}

		public IndicatorDashboard IndicatorDashboard(ISeries<double> input, double fontSize, double rowHeight)
		{
			if (cacheIndicatorDashboard != null)
				for (int idx = 0; idx < cacheIndicatorDashboard.Length; idx++)
					if (cacheIndicatorDashboard[idx] != null && cacheIndicatorDashboard[idx].FontSize == fontSize && cacheIndicatorDashboard[idx].RowHeight == rowHeight && cacheIndicatorDashboard[idx].EqualsInput(input))
						return cacheIndicatorDashboard[idx];
			return CacheIndicator<IndicatorDashboard>(new IndicatorDashboard(){ FontSize = fontSize, RowHeight = rowHeight }, input, ref cacheIndicatorDashboard);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IndicatorDashboard IndicatorDashboard(double fontSize, double rowHeight)
		{
			return indicator.IndicatorDashboard(Input, fontSize, rowHeight);
		}

		public Indicators.IndicatorDashboard IndicatorDashboard(ISeries<double> input , double fontSize, double rowHeight)
		{
			return indicator.IndicatorDashboard(input, fontSize, rowHeight);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IndicatorDashboard IndicatorDashboard(double fontSize, double rowHeight)
		{
			return indicator.IndicatorDashboard(Input, fontSize, rowHeight);
		}

		public Indicators.IndicatorDashboard IndicatorDashboard(ISeries<double> input , double fontSize, double rowHeight)
		{
			return indicator.IndicatorDashboard(input, fontSize, rowHeight);
		}
	}
}

#endregion
