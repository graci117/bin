#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
  

    // ─────────────────────────────────────────────────────────────────
    //  MAIN INDICATOR
    // ─────────────────────────────────────────────────────────────────

    [Description("Algo Engine Foundation — Step 1: Chart indicator scanner + time filter + on-chart panel.")]
    public class AlgoEngineShell : Indicator
    {
        // ── private state ────────────────────────────────────────────
        private List<ChartIndicatorPlot> _registry = new List<ChartIndicatorPlot>();
        private string _panelText = "";

        // ─────────────────────────────────────────────────────────────
        //  LIFECYCLE
        // ─────────────────────────────────────────────────────────────

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description             = "Algo Engine — Foundation Shell";
                Name                    = "AlgoEngineShell";
                Calculate               = Calculate.OnBarClose;
                IsOverlay               = true;
                DisplayInDataBox        = false;
                DrawOnPricePanel        = true;
                ScaleJustification      = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                // ── Time Filter defaults ──────────────────────────────
                EnableTimeFilter = false;
                SessionStart     = DateTime.Parse("09:30");
                SessionEnd       = DateTime.Parse("16:00");

                // ── Panel defaults ────────────────────────────────────
                ShowInfoPanel    = true;
                PanelFontSize    = 12;
                PanelOpacity     = 180;   // 0-255
            }
            else if (State == State.Configure)
            {
                // Nothing to configure in step 1 — placeholder for future
                // AddDataSeries, AddLine, etc. go here later
            }
            else if (State == State.DataLoaded)
            {
                // Initial registry build once data is ready
                BuildIndicatorRegistry();
            }
        }

        protected override void OnBarUpdate()
        {
            // ── Time filter gate ─────────────────────────────────────
            if (EnableTimeFilter && !IsInSession())
                return;

            // ── Refresh registry every bar so new indicators are picked up ──
            BuildIndicatorRegistry();

            // ── Build panel text ─────────────────────────────────────
            if (ShowInfoPanel)
                RenderInfoPanel();
        }

        // ─────────────────────────────────────────────────────────────
        //  CHART INDICATOR SCANNER
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Walks every indicator attached to the current chart and snapshots
        /// each plot name + current value into _registry.
        /// </summary>
        private void BuildIndicatorRegistry()
        {
            _registry.Clear();

            if (ChartControl == null) return;

            try
            {
                foreach (NinjaScriptBase ns in ChartControl.Indicators)
                {
                    // Skip ourselves to avoid infinite loops
                    if (ns == this) continue;
					Print("AE Registry: Found indicator name='" + ns.Name + "' plots=" + 
                  (ns.Plots != null ? ns.Plots.Length : 0));

                    string indName = ns.Name;

                    for (int p = 0; p < ns.Plots.Length ; p++)
                    {
                        double val = double.NaN;
                        try { val = ns.Values[p][0]; }
                        catch { /* plot may not be ready on first bar */ }

                        string plotName = (ns.Plots != null && p < ns.Plots.Length)
                            ? ns.Plots[p].Name
                            : $"Plot{p}";

                        _registry.Add(new ChartIndicatorPlot
                        {
                            IndicatorName = indName,
                            PlotIndex     = p,
                            PlotName      = plotName,
                            CurrentValue  = val
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Silently swallow — chart may not be fully ready
                Print($"AlgoEngineShell Registry Error: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────
        //  TIME FILTER HELPER
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns true if current bar time falls within [SessionStart, SessionEnd].
        /// Handles overnight sessions (end < start) correctly.
        /// </summary>
        private bool IsInSession()
        {
            TimeSpan now   = Time[0].TimeOfDay;
            TimeSpan start = SessionStart.TimeOfDay;
            TimeSpan end   = SessionEnd.TimeOfDay;

            if (start <= end)
                return now >= start && now <= end;
            else
                // overnight: e.g. 18:00 → 08:00
                return now >= start || now <= end;
        }

        // ─────────────────────────────────────────────────────────────
        //  ON-CHART INFO PANEL
        // ─────────────────────────────────────────────────────────────

        private void RenderInfoPanel()
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine("── AlgoEngineShell ──────────────────");

            // Time filter status
            if (EnableTimeFilter)
            {
                bool inSession = IsInSession();
                lines.AppendLine($"Session: {SessionStart:HH:mm} → {SessionEnd:HH:mm}  [{(inSession ? "ACTIVE" : "CLOSED")}]");
            }
            else
            {
                lines.AppendLine("Session Filter: OFF");
            }

            lines.AppendLine($"Detected Indicators: {(ChartControl != null && ChartControl.Indicators != null ? ChartControl.Indicators.Count - 1 : 0)}");
            lines.AppendLine("─────────────────────────────────");

            // List each discovered plot
            if (_registry.Count == 0)
            {
                lines.AppendLine("  (no indicators on chart)");
            }
            else
            {
                string currentInd = "";
                foreach (var plot in _registry)
                {
                    if (plot.IndicatorName != currentInd)
                    {
                        currentInd = plot.IndicatorName;
                        lines.AppendLine($"  [{currentInd}]");
                    }
                    string valStr = double.IsNaN(plot.CurrentValue)
                        ? "n/a"
                        : plot.CurrentValue.ToString("F4");
                    lines.AppendLine($"    {plot.PlotName}: {valStr}");
                }
            }

            lines.AppendLine("─────────────────────────────────");
            lines.AppendLine("CONDITIONS: (none yet — Step 2)");

            Draw.TextFixed(this, "AlgoEngineShellPanel", lines.ToString(),
                TextPosition.TopLeft,
                Brushes.WhiteSmoke,
                new Gui.Tools.SimpleFont("Consolas", PanelFontSize),
                Brushes.Transparent,
                Brushes.Black,
                PanelOpacity);
        }

        // ─────────────────────────────────────────────────────────────
        //  PUBLIC API (for future extension / external callers)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns a snapshot of all detected indicator plots.
        /// Call this from a strategy or another indicator to read the registry.
        /// </summary>
        public IReadOnlyList<ChartIndicatorPlot> GetRegistry() => _registry.AsReadOnly();

        /// <summary>
        /// Gets the current value of a specific indicator plot by name + plot index.
        /// Returns double.NaN if not found.
        /// </summary>
        public double GetPlotValue(string indicatorName, int plotIndex = 0)
        {
            var match = _registry.FirstOrDefault(p =>
                p.IndicatorName.Equals(indicatorName, StringComparison.OrdinalIgnoreCase)
                && p.PlotIndex == plotIndex);
            return match?.CurrentValue ?? double.NaN;
        }

        // ─────────────────────────────────────────────────────────────
        //  PROPERTIES  (appear in NT8 Properties panel)
        // ─────────────────────────────────────────────────────────────

        #region Time Filter

        [NinjaScriptProperty]
        [Display(Name = "Enable Time Filter",
                 Description = "Only evaluate conditions within the session window.",
                 Order = 1, GroupName = "Time Filter")]
        public bool EnableTimeFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Session Start",
                 Description = "Start of the allowed trading session.",
                 Order = 2, GroupName = "Time Filter")]
        [PropertyEditor("NinjaTrader.Gui.Design.TimeEditorKey")]
        public DateTime SessionStart { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Session End",
                 Description = "End of the allowed trading session.",
                 Order = 3, GroupName = "Time Filter")]
        [PropertyEditor("NinjaTrader.Gui.Design.TimeEditorKey")]
        public DateTime SessionEnd { get; set; }

        #endregion

        #region Panel

        [NinjaScriptProperty]
        [Display(Name = "Show Info Panel",
                 Description = "Show the on-chart indicator registry panel.",
                 Order = 1, GroupName = "Panel")]
        public bool ShowInfoPanel { get; set; }

        [NinjaScriptProperty]
        [Range(6, 24)]
        [Display(Name = "Panel Font Size",
                 Order = 2, GroupName = "Panel")]
        public int PanelFontSize { get; set; }

        [NinjaScriptProperty]
        [Range(0, 255)]
        [Display(Name = "Panel Opacity (0-255)",
                 Description = "Background opacity of the info panel.",
                 Order = 3, GroupName = "Panel")]
        public int PanelOpacity { get; set; }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoEngineShell[] cacheAlgoEngineShell;
		public AlgoEngineShell AlgoEngineShell(bool enableTimeFilter, DateTime sessionStart, DateTime sessionEnd, bool showInfoPanel, int panelFontSize, int panelOpacity)
		{
			return AlgoEngineShell(Input, enableTimeFilter, sessionStart, sessionEnd, showInfoPanel, panelFontSize, panelOpacity);
		}

		public AlgoEngineShell AlgoEngineShell(ISeries<double> input, bool enableTimeFilter, DateTime sessionStart, DateTime sessionEnd, bool showInfoPanel, int panelFontSize, int panelOpacity)
		{
			if (cacheAlgoEngineShell != null)
				for (int idx = 0; idx < cacheAlgoEngineShell.Length; idx++)
					if (cacheAlgoEngineShell[idx] != null && cacheAlgoEngineShell[idx].EnableTimeFilter == enableTimeFilter && cacheAlgoEngineShell[idx].SessionStart == sessionStart && cacheAlgoEngineShell[idx].SessionEnd == sessionEnd && cacheAlgoEngineShell[idx].ShowInfoPanel == showInfoPanel && cacheAlgoEngineShell[idx].PanelFontSize == panelFontSize && cacheAlgoEngineShell[idx].PanelOpacity == panelOpacity && cacheAlgoEngineShell[idx].EqualsInput(input))
						return cacheAlgoEngineShell[idx];
			return CacheIndicator<AlgoEngineShell>(new AlgoEngineShell(){ EnableTimeFilter = enableTimeFilter, SessionStart = sessionStart, SessionEnd = sessionEnd, ShowInfoPanel = showInfoPanel, PanelFontSize = panelFontSize, PanelOpacity = panelOpacity }, input, ref cacheAlgoEngineShell);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoEngineShell AlgoEngineShell(bool enableTimeFilter, DateTime sessionStart, DateTime sessionEnd, bool showInfoPanel, int panelFontSize, int panelOpacity)
		{
			return indicator.AlgoEngineShell(Input, enableTimeFilter, sessionStart, sessionEnd, showInfoPanel, panelFontSize, panelOpacity);
		}

		public Indicators.AlgoEngineShell AlgoEngineShell(ISeries<double> input , bool enableTimeFilter, DateTime sessionStart, DateTime sessionEnd, bool showInfoPanel, int panelFontSize, int panelOpacity)
		{
			return indicator.AlgoEngineShell(input, enableTimeFilter, sessionStart, sessionEnd, showInfoPanel, panelFontSize, panelOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoEngineShell AlgoEngineShell(bool enableTimeFilter, DateTime sessionStart, DateTime sessionEnd, bool showInfoPanel, int panelFontSize, int panelOpacity)
		{
			return indicator.AlgoEngineShell(Input, enableTimeFilter, sessionStart, sessionEnd, showInfoPanel, panelFontSize, panelOpacity);
		}

		public Indicators.AlgoEngineShell AlgoEngineShell(ISeries<double> input , bool enableTimeFilter, DateTime sessionStart, DateTime sessionEnd, bool showInfoPanel, int panelFontSize, int panelOpacity)
		{
			return indicator.AlgoEngineShell(input, enableTimeFilter, sessionStart, sessionEnd, showInfoPanel, panelFontSize, panelOpacity);
		}
	}
}

#endregion
