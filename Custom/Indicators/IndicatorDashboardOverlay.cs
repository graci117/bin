#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using System.Windows.Media;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class IndicatorDashboardOverlay : Indicator
    {
        private string overlayText = "Waiting for chart...";
        private int lastRefreshBar = -1;
        private const string TagName = "IndicatorDashboardOverlayTag";

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Uses reflection to inspect chart indicators and display all Values[] / plot labels for the current bar in an overlay.";
                Name = "IndicatorDashboardOverlay";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DrawOnPricePanel = true;
                DisplayInDataBox = false;
                PaintPriceMarkers = false;
                IsSuspendedWhileInactive = true;

                RefreshEveryTick = true;
                MaxLines = 60;
                FontSize = 12;
                BackgroundOpacity = 170;
                IncludeThisIndicator = false;
                ShowOnlyNumericValues = true;
                BarsAgo = 1;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 0)
                return;

            if (RefreshEveryTick || CurrentBar != lastRefreshBar)
            {
                lastRefreshBar = CurrentBar;
                overlayText = BuildOverlayText();
            }

            Draw.TextFixed(this, TagName, overlayText, TextPosition.TopLeft,
                Brushes.White, new SimpleFont("Consolas", FontSize),
                Brushes.White, Brushes.Black, BackgroundOpacity);
        }

        private string BuildOverlayText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Indicator Dashboard");
            sb.AppendLine("------------------------------");

            try
            {
                if (ChartControl == null)
                {
                    sb.AppendLine("ChartControl not ready.");
                    return sb.ToString();
                }

                object root = GetPropertyValue(ChartControl, "Indicators");
                if (root == null)
                {
                    sb.AppendLine("ChartControl.Indicators not available.");
                    return sb.ToString();
                }

                List<object> indicators = FlattenIndicatorObjects(root);
                if (!IncludeThisIndicator)
                    indicators.RemoveAll(o => object.ReferenceEquals(o, this));

                if (indicators.Count == 0)
                {
                    sb.AppendLine("No indicators found.");
                    return sb.ToString();
                }

                int lineCount = 2;
                foreach (object indicatorObj in indicators)
                {
                    if (indicatorObj == null)
                        continue;

                    string indicatorName = GetIndicatorName(indicatorObj);
                    IList values = GetPropertyAsList(indicatorObj, "Values");
                    IList plots = GetPropertyAsList(indicatorObj, "Plots");

                    int valuesCount = values != null ? values.Count : 0;
                    int plotsCount = plots != null ? plots.Count : 0;
                    int seriesCount = Math.Max(valuesCount, plotsCount);

                    if (seriesCount == 0)
                    {
                        if (lineCount < MaxLines)
                        {
                            sb.AppendLine(indicatorName + " | no Values/Plots");
                            lineCount++;
                        }
                        continue;
                    }

                    for (int i = 0; i < seriesCount; i++)
                    {
                        if (lineCount >= MaxLines)
                            break;

                        string label = GetPlotLabel(plots, i);
                        string valueText = GetSeriesValueText(values, i);

                        if (ShowOnlyNumericValues && valueText == "N/A")
                            continue;

                        sb.AppendLine(indicatorName + " | " + label + " = " + valueText);
                        lineCount++;
                    }

                    if (lineCount >= MaxLines)
                        break;
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine("Reflection error: " + ex.Message);
            }

            return sb.ToString();
        }

        private List<object> FlattenIndicatorObjects(object root)
        {
            List<object> result = new List<object>();
            HashSet<string> seen = new HashSet<string>();

            IEnumerable enumerable = root as IEnumerable;
            if (enumerable == null)
                return result;

            foreach (object obj in enumerable)
            {
                if (obj == null)
                    continue;

                object actual = ExtractIndicatorObject(obj);
                if (actual == null)
                    continue;

                string key = actual.GetType().FullName + "|" + SafeRuntimeId(actual);
                if (seen.Add(key))
                    result.Add(actual);
            }

            return result;
        }

        private object ExtractIndicatorObject(object obj)
        {
            if (obj == null)
                return null;

            Type t = obj.GetType();
            if (LooksLikeIndicator(t))
                return obj;

            foreach (string propName in new[] { "Indicator", "HostedIndicator", "NinjaScriptBase", "Script", "Tag", "Content" })
            {
                try
                {
                    PropertyInfo p = t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (p == null)
                        continue;

                    object val = p.GetValue(obj, null);
                    if (val == null)
                        continue;

                    if (LooksLikeIndicator(val.GetType()))
                        return val;
                }
                catch { }
            }

            return null;
        }

        private bool LooksLikeIndicator(Type t)
        {
            if (t == null)
                return false;

            Type cur = t;
            while (cur != null)
            {
                if (cur.FullName == "NinjaTrader.NinjaScript.Indicator" || cur.Name == "Indicator")
                    return true;
                cur = cur.BaseType;
            }

            return t.GetProperty("Values", BindingFlags.Public | BindingFlags.Instance) != null;
        }

        private object GetPropertyValue(object target, string propertyName)
        {
            try
            {
                if (target == null)
                    return null;
                PropertyInfo p = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                return p != null ? p.GetValue(target, null) : null;
            }
            catch { return null; }
        }

        private IList GetPropertyAsList(object target, string propertyName)
        {
            try
            {
                object value = GetPropertyValue(target, propertyName);
                return value as IList;
            }
            catch { return null; }
        }

        private string GetIndicatorName(object indicatorObj)
        {
            try
            {
                object nameObj = GetPropertyValue(indicatorObj, "Name");
                string name = nameObj as string;
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
            catch { }

            return indicatorObj.GetType().Name;
        }

        private string GetPlotLabel(IList plots, int index)
        {
            if (plots != null && index >= 0 && index < plots.Count && plots[index] != null)
            {
                try
                {
                    object plot = plots[index];
                    object nameObj = GetPropertyValue(plot, "Name");
                    string name = nameObj as string;
                    if (!string.IsNullOrWhiteSpace(name))
                        return name;
                }
                catch { }
            }

            return "Value" + index;
        }

        private string GetSeriesValueText(IList values, int index)
        {
            if (values == null || index < 0 || index >= values.Count || values[index] == null)
                return "N/A";

            try
            {
                object series = values[index];
                Type seriesType = series.GetType();

                int targetBarsAgo = Math.Max(0, BarsAgo);
                int targetBarIndex = CurrentBar - targetBarsAgo;
                if (targetBarIndex < 0)
                    return "N/A";

                // Prefer GetValueAt(int barIndex) with user-selected absolute bar index
                MethodInfo getValueAt = seriesType.GetMethod("GetValueAt",
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new Type[] { typeof(int) }, null);
                if (getValueAt != null)
                {
                    object rawAt = getValueAt.Invoke(series, new object[] { targetBarIndex });
                    if (rawAt != null)
                    {
                        double dAt = Convert.ToDouble(rawAt);
                        if (!double.IsNaN(dAt) && !double.IsInfinity(dAt))
                            return FormatValue(dAt);
                    }
                }

                // Fallback: Item[int barsAgo] indexer — [0] = current bar in NT8 BarsAgo convention
                // but only valid when the series Count > 0
                PropertyInfo countProp = seriesType.GetProperty("Count",
                    BindingFlags.Public | BindingFlags.Instance);
                int count = 0;
                if (countProp != null)
                    count = Convert.ToInt32(countProp.GetValue(series, null));

                if (count <= 0)
                    return "N/A";

                PropertyInfo indexer = seriesType.GetProperty("Item",
                    BindingFlags.Public | BindingFlags.Instance);
                if (indexer == null)
                    return "N/A";

                // [BarsAgo] in NT8 Series = user-selected barsAgo under barsAgo indexing
                object raw = indexer.GetValue(series, new object[] { targetBarsAgo });
                if (raw == null)
                    return "N/A";

                double value = Convert.ToDouble(raw);
                if (double.IsNaN(value) || double.IsInfinity(value))
                    return "N/A";

                return FormatValue(value);
            }
            catch
            {
                return "N/A";
            }
        }

        private string FormatValue(double value)
        {
            if (Math.Abs(value) >= 10000)
                return value.ToString("N0");
            if (Math.Abs(value) >= 100)
                return value.ToString("N2");
            if (Math.Abs(value) >= 1)
                return value.ToString("N4");
            return value.ToString("N6");
        }

        private string SafeRuntimeId(object obj)
        {
            try
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj).ToString();
            }
            catch
            {
                return obj.GetHashCode().ToString();
            }
        }

        [NinjaScriptProperty]
        [Display(Name = "Refresh every tick", Order = 1, GroupName = "Parameters")]
        public bool RefreshEveryTick { get; set; }

        [NinjaScriptProperty]
        [Range(5, 150)]
        [Display(Name = "Max lines", Order = 2, GroupName = "Parameters")]
        public int MaxLines { get; set; }

        [NinjaScriptProperty]
        [Range(8, 32)]
        [Display(Name = "Font size", Order = 3, GroupName = "Parameters")]
        public int FontSize { get; set; }

        [NinjaScriptProperty]
        [Range(0, 255)]
        [Display(Name = "Background opacity", Order = 4, GroupName = "Parameters")]
        public int BackgroundOpacity { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Include this indicator", Order = 5, GroupName = "Parameters")]
        public bool IncludeThisIndicator { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show only numeric values", Order = 6, GroupName = "Parameters")]
        public bool ShowOnlyNumericValues { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Bars ago", Order = 6, GroupName = "Parameters")]
        public int BarsAgo { get; set; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IndicatorDashboardOverlay[] cacheIndicatorDashboardOverlay;
		public IndicatorDashboardOverlay IndicatorDashboardOverlay(bool refreshEveryTick, int maxLines, int fontSize, int backgroundOpacity, bool includeThisIndicator, bool showOnlyNumericValues, int barsAgo)
		{
			return IndicatorDashboardOverlay(Input, refreshEveryTick, maxLines, fontSize, backgroundOpacity, includeThisIndicator, showOnlyNumericValues, barsAgo);
		}

		public IndicatorDashboardOverlay IndicatorDashboardOverlay(ISeries<double> input, bool refreshEveryTick, int maxLines, int fontSize, int backgroundOpacity, bool includeThisIndicator, bool showOnlyNumericValues, int barsAgo)
		{
			if (cacheIndicatorDashboardOverlay != null)
				for (int idx = 0; idx < cacheIndicatorDashboardOverlay.Length; idx++)
					if (cacheIndicatorDashboardOverlay[idx] != null && cacheIndicatorDashboardOverlay[idx].RefreshEveryTick == refreshEveryTick && cacheIndicatorDashboardOverlay[idx].MaxLines == maxLines && cacheIndicatorDashboardOverlay[idx].FontSize == fontSize && cacheIndicatorDashboardOverlay[idx].BackgroundOpacity == backgroundOpacity && cacheIndicatorDashboardOverlay[idx].IncludeThisIndicator == includeThisIndicator && cacheIndicatorDashboardOverlay[idx].ShowOnlyNumericValues == showOnlyNumericValues && cacheIndicatorDashboardOverlay[idx].BarsAgo == barsAgo && cacheIndicatorDashboardOverlay[idx].EqualsInput(input))
						return cacheIndicatorDashboardOverlay[idx];
			return CacheIndicator<IndicatorDashboardOverlay>(new IndicatorDashboardOverlay(){ RefreshEveryTick = refreshEveryTick, MaxLines = maxLines, FontSize = fontSize, BackgroundOpacity = backgroundOpacity, IncludeThisIndicator = includeThisIndicator, ShowOnlyNumericValues = showOnlyNumericValues, BarsAgo = barsAgo }, input, ref cacheIndicatorDashboardOverlay);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IndicatorDashboardOverlay IndicatorDashboardOverlay(bool refreshEveryTick, int maxLines, int fontSize, int backgroundOpacity, bool includeThisIndicator, bool showOnlyNumericValues, int barsAgo)
		{
			return indicator.IndicatorDashboardOverlay(Input, refreshEveryTick, maxLines, fontSize, backgroundOpacity, includeThisIndicator, showOnlyNumericValues, barsAgo);
		}

		public Indicators.IndicatorDashboardOverlay IndicatorDashboardOverlay(ISeries<double> input , bool refreshEveryTick, int maxLines, int fontSize, int backgroundOpacity, bool includeThisIndicator, bool showOnlyNumericValues, int barsAgo)
		{
			return indicator.IndicatorDashboardOverlay(input, refreshEveryTick, maxLines, fontSize, backgroundOpacity, includeThisIndicator, showOnlyNumericValues, barsAgo);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IndicatorDashboardOverlay IndicatorDashboardOverlay(bool refreshEveryTick, int maxLines, int fontSize, int backgroundOpacity, bool includeThisIndicator, bool showOnlyNumericValues, int barsAgo)
		{
			return indicator.IndicatorDashboardOverlay(Input, refreshEveryTick, maxLines, fontSize, backgroundOpacity, includeThisIndicator, showOnlyNumericValues, barsAgo);
		}

		public Indicators.IndicatorDashboardOverlay IndicatorDashboardOverlay(ISeries<double> input , bool refreshEveryTick, int maxLines, int fontSize, int backgroundOpacity, bool includeThisIndicator, bool showOnlyNumericValues, int barsAgo)
		{
			return indicator.IndicatorDashboardOverlay(input, refreshEveryTick, maxLines, fontSize, backgroundOpacity, includeThisIndicator, showOnlyNumericValues, barsAgo);
		}
	}
}

#endregion
