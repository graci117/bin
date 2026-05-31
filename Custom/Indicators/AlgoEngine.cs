#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Windows.Interop;
using System.Reflection;
using NinjaTrader.Gui.Tools;
#endregion

// Enums outside namespace to avoid conflicts with NinjaTrader.Data types
public enum ConditionOperator    { Greater, GreaterOrEqual, Less, LessOrEqual, Equals, CrossAbove, CrossBelow }
public enum ConditionSourceType  { Indicator, Price, StaticValue, SignalState }
public enum AEPriceType          { Close, Open, High, Low, Volume }

namespace NinjaTrader.NinjaScript.Indicators
{
    // ── Data structures ──────────────────────────────────────────────

    public class ChartIndicatorPlot
    {
        public string IndicatorName { get; set; }
        public int    PlotIndex     { get; set; }  // index in ns.Plots[]
        public int    ValuesIndex   { get; set; }  // index in ns.Values[]
        public string PlotName      { get; set; }
        public double CurrentValue  { get; set; }
    }

    public class ConditionItem
    {
        public ConditionSourceType LeftSourceType   { get; set; } = ConditionSourceType.Indicator;
        public string              LeftIndicator    { get; set; } = "";  // full ns.Name with params
        public string              LeftIndicatorShort { get; set; } = ""; // short name before '('
        public int                 LeftPlotIndex    { get; set; } = 0;
        public string              LeftPlotName     { get; set; } = "";
        public AEPriceType         LeftPriceType    { get; set; } = AEPriceType.Close;
        public double              LeftStaticValue  { get; set; } = 0;
        public int                 LeftBarsAgo      { get; set; } = 0;
        public string              LeftFieldName    { get; set; } = "";   // field name for SignalState
        public ConditionOperator   Operator         { get; set; } = ConditionOperator.Greater;
        public ConditionSourceType RightSourceType  { get; set; } = ConditionSourceType.Indicator;
        public string              RightIndicator   { get; set; } = "";  // full ns.Name
        public string              RightIndicatorShort { get; set; } = ""; // short name
        public int                 RightPlotIndex   { get; set; } = 0;
        public string              RightPlotName    { get; set; } = "";
        public AEPriceType         RightPriceType   { get; set; } = AEPriceType.Close;
        public double              RightStaticValue { get; set; } = 0;
        public int                 RightBarsAgo     { get; set; } = 0;
        public string              RightFieldName   { get; set; } = "";   // field name for SignalState
        public bool                ByOffset         { get; set; } = false;
        public bool                AtLeast          { get; set; } = false;
        public double              AtLeastVal       { get; set; } = 0;
        public bool                AtMost           { get; set; } = false;
        public double              AtMostVal        { get; set; } = 0;

        public override string ToString()
        {
            string lPlot  = !string.IsNullOrEmpty(LeftPlotName)  ? LeftPlotName  : LeftPlotIndex.ToString();
            string rPlot  = !string.IsNullOrEmpty(RightPlotName) ? RightPlotName : RightPlotIndex.ToString();
            string lAgo   = LeftBarsAgo  > 0 ? "[" + LeftBarsAgo  + "]" : "";
            string rAgo   = RightBarsAgo > 0 ? "[" + RightBarsAgo + "]" : "";
            string left  = LeftSourceType  == ConditionSourceType.StaticValue
                         ? LeftStaticValue.ToString("F4")
                         : LeftSourceType  == ConditionSourceType.Price
                         ? LeftPriceType.ToString() + lAgo
                         : LeftSourceType  == ConditionSourceType.SignalState
                         ? LeftIndicator + "." + LeftFieldName
                         : LeftIndicator + "." + lPlot + lAgo;
            string right = RightSourceType == ConditionSourceType.StaticValue
                         ? RightStaticValue.ToString("F4")
                         : RightSourceType == ConditionSourceType.Price
                         ? RightPriceType.ToString() + rAgo
                         : RightSourceType == ConditionSourceType.SignalState
                         ? RightIndicator + "." + RightFieldName
                         : RightIndicator + "." + rPlot + rAgo;
            return left + "  " + Operator + "  " + right;
        }
    }

    public class ConditionSet
    {
        public string              Name                { get; set; } = "Set 1";
        public bool                IsEnabled           { get; set; } = true;
        public List<ConditionItem> HitBarConditions    { get; set; } = new List<ConditionItem>();
        public List<ConditionItem> SignalBarConditions { get; set; } = new List<ConditionItem>();
        public string              EntryAction         { get; set; } = "None";
        public int                 Quantity            { get; set; } = 1;
        public string              AtmStrategy         { get; set; } = "None";
        public int                 BarMin              { get; set; } = 0;
        public int                 BarMax              { get; set; } = 0;
        public string              BarDirection        { get; set; } = "Any";
    }

    // ── Win32 helper: prevent our floating windows from stealing focus ──
    internal static class WindowHelper
    {
        private const int GWL_EXSTYLE      = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void MakeNoActivate(Window w)
        {
            w.SourceInitialized += (s, e) =>
            {
                try
                {
                    var handle = new WindowInteropHelper(w).Handle;
                    int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
                    SetWindowLong(handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
                }
                catch { }
            };
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  MAIN INDICATOR
    // ════════════════════════════════════════════════════════════════

    [Description("AlgoEngine — Condition builder with compact widget and floating builder window.")]
    public class AlgoEngine : Indicator
    {
        // ── indicator state ──────────────────────────────────────────
        private List<ChartIndicatorPlot> _registry = new List<ChartIndicatorPlot>();
        private List<ConditionSet>       _sets     = new List<ConditionSet>();
        private bool _longOn      = false;
        private bool _shortOn     = false;
        private bool _panelBuilt  = false;
        private bool _isDuplicate = false;
       
		private int _lastLongEntryBar = -1;
		private int _lastShortEntryBar = -1;

        // ── WPF windows ──────────────────────────────────────────────
        private Window     _compactWindow = null;   // unused — widget lives in chart canvas
        private Window     _builderWindow;   // full condition builder
        private TabControl _setTabs;
        private Button     _btnLong, _btnShort;
		
		private string _selectedAccountName = "Sim101";
		private int _selectedQty = 1;
		private string _selectedOrderType = "MKT";
		private int _lastEntryBar = -1;
		
		private int _lastOrderBar = -1;
		private DateTime _lastOrderTime = Core.Globals.MinDate;
		private string _lastOrderDirection = string.Empty;
		private ComboBox _accountSelector;
		 public  int  LastLongSignalBar  { get; private set; } = -1;
        public  int  LastShortSignalBar { get; private set; } = -1;
		private static ConditionItem _condClipboard = null;
		
		private double _dailyPnl        = 0.0;
		private DateTime _lastPnlDate   = DateTime.MinValue;

        // ════════════════════════════════════════════════════════════
        //  LIFECYCLE
        // ════════════════════════════════════════════════════════════

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description              = "AlgoEngine — Condition Builder";
                Name                     = "AlgoEngine";
                Calculate                = Calculate.OnBarClose;
                IsOverlay                = true;
                DisplayInDataBox         = false;
                DrawOnPricePanel         = true;
                IsSuspendedWhileInactive = true;

               
                WaitUntilFlat    = true;
                EntryCooldown    = 100;
                EnableMoneyMgmt  = false;
                MaxDailyProfit   = 1500;
                MaxDailyLoss     = 800;
                HitBarHighlight        = true;
                HitBarHighlightColor   = Brushes.Violet;
                HitBarHighlightOpacity = 50;
                BtnLongActiveColor     = Brushes.DodgerBlue;
                BtnLongInactiveColor   = Brushes.LightSkyBlue;
                BtnShortActiveColor    = Brushes.HotPink;
                BtnShortInactiveColor  = Brushes.Thistle;
                DragBarColor           = Brushes.LimeGreen;
                TitleTextColor         = Brushes.White;
                WinLeft                = 100;
                WinTop                 = 100;
                AlertPopupEnabled      = false;
                AlertSoundEnabled      = false;
                AlertMarkerEnabled     = true;
                MarkerColorBullish     = Brushes.DodgerBlue;
                MarkerColorBearish     = Brushes.HotPink;
				LongEnabledPersist = false;
				ShortEnabledPersist = false;
				
				// Within defaults
				WithinFilterCount = 1;
			
				
				// Skip defaults
				SkipFilterCount = 1;
				
				
				// Within defaults
				WithinStart1 = new TimeSpan(7, 30, 0);    // 07:30:00
				WithinEnd1   = new TimeSpan(16, 45, 0);   // 04:45 PM
				WithinStart2 = new TimeSpan(0, 1, 0);     // 12:01 AM
				WithinEnd2   = new TimeSpan(6, 45, 0);    // 06:45 AM
				WithinStart3 = new TimeSpan(0, 1, 0);
				WithinEnd3   = new TimeSpan(6, 45, 0);
				WithinStart4 = new TimeSpan(8, 0, 0);
				WithinEnd4   = new TimeSpan(16, 0, 0);
				
				// Skip defaults
				SkipStart1 = new TimeSpan(8, 28, 0);
				SkipEnd1   = new TimeSpan(8, 32, 0);
				SkipStart2 = new TimeSpan(15, 58, 0);
				SkipEnd2   = new TimeSpan(16, 2, 0);
				SkipStart3 = new TimeSpan(6, 28, 0);
				SkipEnd3   = new TimeSpan(6, 32, 0);
				SkipStart4 = new TimeSpan(8, 28, 0);
				SkipEnd4   = new TimeSpan(8, 32, 0);
				SkipStart5 = new TimeSpan(8, 28, 0);
				SkipEnd5   = new TimeSpan(8, 32, 0);
				SkipStart6 = new TimeSpan(8, 28, 0);
				SkipEnd6   = new TimeSpan(8, 32, 0);
            }
         else if (State == State.DataLoaded)
		{
		    // Restore conditions from serialized JSON
		    _sets.Clear();
		    if (!string.IsNullOrEmpty(SetsJson))
		    {
		        try
		        {
		            var restored = DeserializeSets(SetsJson);
		            if (restored != null && restored.Count > 0)
		                foreach (var s in restored) _sets.Add(s);
		        }
		        catch (Exception ex) { Print("AE deserialize error: " + ex.Message); }
		    }
		    if (_sets.Count == 0)
		        _sets.Add(new ConditionSet { Name = "Set 1" });
		    BuildIndicatorRegistry();
			_longOn = LongEnabledPersist;
			_shortOn = ShortEnabledPersist;
		    Print("AlgoEngine DataLoaded | sets=" + _sets.Count);
		}

         else if (State == State.Terminated)
			{
			    // Always persist current conditions before the indicator is torn down.
			    // This covers Edit dialog, F5 recompile, and chart close.
			    try { SaveSets(); } catch { }
			    DisposeWindows();
			}
			else if (State == State.Configure)
{
    // Re-hydrate sets as early as possible so they survive Edit→OK cycles
    // where DataLoaded may not be re-fired.
    if (_sets.Count == 0 && !string.IsNullOrEmpty(SetsJson))
    {
		_longOn = LongEnabledPersist;
			_shortOn = ShortEnabledPersist;
        try
        {
			
            var restored = DeserializeSets(SetsJson);
            if (restored != null && restored.Count > 0)
                foreach (var s in restored) _sets.Add(s);
        }
        catch { }
    }
}
        }

        protected override void OnBarUpdate()
        {
           // Duplicate detection — only flag as duplicate if another AlgoEngine has ALREADY built its panel.
			// On F5 recompile the old instance is still alive when the new one first runs, so we must NOT
			// count the old instance (which is about to be Terminated) against the new one.
			if (!_isDuplicate && !_panelBuilt && ChartControl != null)
			{
			    int activeCount = 0;
			    try {
			        foreach (NinjaScriptBase ns in ChartControl.Indicators)
			        {
			            if (ns == this) continue;
			            if (ns.GetType().Name != "AlgoEngine") continue;
			            // Only count instances that have successfully built their widget
			            var panelField = ns.GetType().GetField("_panelBuilt",
			                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			            bool otherBuilt = panelField != null && (bool)panelField.GetValue(ns);
			            if (otherBuilt) activeCount++;
			        }
			    } catch { }
			    if (activeCount > 0)
			    {
			        _isDuplicate = true;
			        Print("AlgoEngine: duplicate — skipping widget");
			    }
			}
			if (_isDuplicate) return;

            // Spawn compact widget once ChartControl is available
            if (!_panelBuilt && ChartControl != null)
            {
                _panelBuilt = true;
                Print("AlgoEngine spawning widget | ChartControl=" + ChartControl.GetType().Name);
                ChartControl.Dispatcher.BeginInvoke(
                    new Action(BuildCompactWidget),
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
			 HandleWithinFilterExits();
           if (!IsInTimeFilters()) return;
		   
		   // Update P&L then check limits
		UpdateDailyPnl();
		DrawDailyLimitMessage();
		if (IsDailyLimitHit()) return;
            BuildIndicatorRegistry();
            EvaluateSets();
        }

        // ════════════════════════════════════════════════════════════
        //  CHART INDICATOR SCANNER
        // ════════════════════════════════════════════════════════════

        private void BuildIndicatorRegistry()
        {
            _registry.Clear();
            if (ChartControl == null) return;
            try
            {
                foreach (NinjaScriptBase ns in ChartControl.Indicators)
                {
                    if (ns == this) continue;
                    int plotCount = ns.Plots != null ? ns.Plots.Length : 0;
                    for (int p = 0; p < plotCount; p++)
                    {
                        double val = double.NaN;
                        try { val = ns.Values[p][0]; } catch { }
                        // Find the Values[] index that matches this plot by name
                        int vIdx = p; // default: same as plot index
                        if (ns.Values != null)
                        {
                            for (int vi = 0; vi < ns.Values.Length; vi++)
                            {
                                // Values don't have names directly; use plot order as best guess
                                // For indicators where Values.Count > Plots.Count,
                                // the first Plots.Count values usually correspond 1:1
                                vIdx = p; // keep as-is; user must ensure plot order matches
                                break;
                            }
                        }
                        _registry.Add(new ChartIndicatorPlot
                        {
                            IndicatorName = ns.Name,
                            PlotIndex     = p,
                            ValuesIndex   = vIdx,
                            PlotName      = ns.Plots[p].Name,
                            CurrentValue  = val
                        });
                    }
                }
            }
            catch (Exception ex) { Print("AlgoEngine scanner: " + ex.Message); }
        }

        // ════════════════════════════════════════════════════════════
        //  REFLECTION HELPERS — dynamic field/property access
        //  SignalState source type uses these to let users pick from
        //  a live ComboBox of all numeric/bool fields on any indicator.
        //  No field names are ever hardcoded.
        // ════════════════════════════════════════════════════════════

        private static readonly BindingFlags _bfAll =
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Returns all readable field and property names on an indicator that
        /// produce a bool, int, long, float, double, or decimal value.
        /// Auto-property backing fields like &lt;Foo&gt;k__BackingField are unwrapped to "Foo".
        /// </summary>
        private List<string> GetIndicatorFieldNames(NinjaScriptBase ns)
        {
            var names = new List<string>();
            if (ns == null) return names;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var numTypes = new HashSet<Type>
            {
                typeof(bool), typeof(int), typeof(long),
                typeof(float), typeof(double), typeof(decimal)
            };
            try
            {
                Type t = ns.GetType();
                foreach (FieldInfo fi in t.GetFields(_bfAll))
                {
                    if (!numTypes.Contains(fi.FieldType)) continue;
                    string name = fi.Name;
                    if (name.StartsWith("<") && name.EndsWith(">k__BackingField"))
                        name = name.Substring(1, name.Length - ">k__BackingField".Length - 1);
                    if (name.Contains("<") || name.Contains(">")) continue;
                    if (seen.Add(name)) names.Add(name);
                }
                foreach (PropertyInfo pi in t.GetProperties(_bfAll))
                {
                    if (!pi.CanRead) continue;
                    if (!numTypes.Contains(pi.PropertyType)) continue;
                    if (seen.Add(pi.Name)) names.Add(pi.Name);
                }
            }
            catch { }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }
		
		
		private Account GetSelectedAccount()
		{
		    try
		    {
		        string wanted = string.IsNullOrWhiteSpace(_selectedAccountName) ? "Sim101" : _selectedAccountName;
		
		        foreach (Account acct in Account.All)
		        {
		            if (acct != null && string.Equals(acct.Name, wanted, StringComparison.OrdinalIgnoreCase))
		                return acct;
		        }
		    }
		    catch (Exception ex)
		    {
		        Print("AE GetSelectedAccount error: " + ex.Message);
		    }
		
		    return null;
		}
		
		private bool HasOpenPosition(Account account)
		{
		    try
		    {
		        if (account == null || Instrument == null)
		            return false;
		
		        foreach (Position p in account.Positions)
		        {
		            if (p == null || p.Instrument == null)
		                continue;
		
		            if (!string.Equals(p.Instrument.FullName, Instrument.FullName, StringComparison.OrdinalIgnoreCase))
		                continue;
		
		            if (p.MarketPosition != MarketPosition.Flat)
		                return true;
		        }
		    }
		    catch (Exception ex)
		    {
		        Print("AE HasOpenPosition error: " + ex.Message);
		    }
		
		    return false;
		}
		
		private bool HasWorkingOrder(Account account)
		{
		    try
		    {
		        if (account == null || Instrument == null)
		            return false;
		
		        foreach (Order o in account.Orders)
		        {
		            if (o == null || o.Instrument == null)
		                continue;
		
		            if (!string.Equals(o.Instrument.FullName, Instrument.FullName, StringComparison.OrdinalIgnoreCase))
		                continue;
		
		            if (o.OrderState == OrderState.Submitted
		                || o.OrderState == OrderState.Accepted
		                || o.OrderState == OrderState.Working
		                )
		                return true;
		        }
		    }
		    catch (Exception ex)
		    {
		        Print("AE HasWorkingOrder error: " + ex.Message);
		    }
		
		    return false;
		}
		
		private bool IsCooldownActive()
		{
		    if (EntryCooldown <= 0)
		        return false;
		
		    if (_lastOrderTime == Core.Globals.MinDate)
		        return false;
		
		    return (DateTime.Now - _lastOrderTime).TotalMilliseconds < EntryCooldown;
		}
		
		private bool CanSubmitOrder(string direction)
		{
		    if (_lastOrderBar == CurrentBar)
		    {
		        Print("AE BLOCKED | same bar | bar=" + CurrentBar);
		        return false;
		    }
		
		    if (IsCooldownActive())
		    {
		        Print("AE BLOCKED | cooldown | ms=" + EntryCooldown);
		        return false;
		    }
		
		    Account acct = GetSelectedAccount();
		    if (acct == null)
		    {
		        Print("AE BLOCKED | no account");
		        return false;
		    }
		
		    bool hasWorking = HasWorkingOrder(acct);
		    bool hasPosition = WaitUntilFlat && HasOpenPosition(acct);
		
		    Print("AE CanSubmit check | dir=" + direction
		        + " | hasWorking=" + hasWorking
		        + " | hasPosition=" + hasPosition
		        + " | acct=" + acct.Name);
		
		    if (hasWorking)
		    {
		        Print("AE BLOCKED | working order");
		        return false;
		    }
		
		    if (hasPosition)
		    {
		        Print("AE BLOCKED | not flat");
		        return false;
		    }
		
		    return true;
		}
		
		private void MarkOrderSubmitted(string direction)
		{
		    _lastOrderBar = CurrentBar;
		    _lastOrderTime = DateTime.Now;
		    _lastOrderDirection = direction;
		}

        /// <summary>
        /// Reads a named field or auto-property from obj via reflection.
        /// bool → 1.0/0.0 | numeric → double | not found → NaN
        /// </summary>
        private double TryResolveFieldValue(object obj, string fieldName)
        {
            if (obj == null || string.IsNullOrEmpty(fieldName)) return double.NaN;
            try
            {
                Type t = obj.GetType();
                FieldInfo fi = t.GetField(fieldName, _bfAll);
                if (fi == null)
                    fi = t.GetField("<" + fieldName + ">k__BackingField", _bfAll);
                if (fi != null) return FieldValueToDouble(fi.GetValue(obj));
                PropertyInfo pi = t.GetProperty(fieldName, _bfAll);
                if (pi != null && pi.CanRead) return FieldValueToDouble(pi.GetValue(obj, null));
            }
            catch (Exception ex)
            {
                Print("AE TryResolveFieldValue | field=" + fieldName + " ex=" + ex.Message);
            }
            return double.NaN;
        }
		
		private void UpdateDailyPnl()
		{
		    // Reset tracker at start of new day
		    if (Time[0].Date != _lastPnlDate)
		    {
		        _dailyPnl    = 0.0;
		        _lastPnlDate = Time[0].Date;
		        Print("AE DailyPnL reset for " + _lastPnlDate.ToShortDateString());
		    }
		
		    try
		    {
		        var account = NinjaTrader.Cbi.Account.All
		            .FirstOrDefault(a => a.Connection != null &&
		                a.Connection.Status == NinjaTrader.Cbi.ConnectionStatus.Connected);
		
		        if (account == null) return;
		
		        // Pull today's realized P&L directly from the account item
		        // This is the correct NT8 API from an indicator context
		        _dailyPnl = account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
		
		        Print("AE DailyPnL = " + _dailyPnl);
		    }
		    catch (Exception ex)
		    {
		        Print("AE DailyPnL error: " + ex.Message);
		    }
		}
		
		private bool IsDailyLimitHit()
		{
		    if (!EnableMoneyMgmt) return false;
		
		    if (_dailyPnl >= MaxDailyProfit)
		    {
		        Print("AE DailyPnL PROFIT LIMIT hit: " + _dailyPnl + " >= " + MaxDailyProfit);
		        return true;
		    }
		    if (_dailyPnl <= -Math.Abs(MaxDailyLoss))
		    {
		        Print("AE DailyPnL LOSS LIMIT hit: " + _dailyPnl + " <= -" + MaxDailyLoss);
		        return true;
		    }
		    return false;
		}
		
		private void DrawDailyLimitMessage()
		{
		    string msg   = null;
		    Brush  color = Brushes.White;
		
		    if (!EnableMoneyMgmt)
		    {
		        // Clear any existing message when disabled
		        RemoveDrawObject("DailyLimitMsg");
		        return;
		    }
		
		    if (_dailyPnl >= MaxDailyProfit)
		    {
		        msg   = "✓ DAILY PROFIT LIMIT HIT  +" + _dailyPnl.ToString("C");
		        color = Brushes.LimeGreen;
		    }
		    else if (_dailyPnl <= -Math.Abs(MaxDailyLoss))
		    {
		        msg   = "✗ DAILY LOSS LIMIT HIT  " + _dailyPnl.ToString("C");
		        color = Brushes.OrangeRed;
		    }
		    else
		    {
		        // No limit hit — clear old message if any
		        RemoveDrawObject("DailyLimitMsg");
		        return;
		    }
		
		    Draw.TextFixed(
		        this,
		        "DailyLimitMsg",
		        msg,
		        TextPosition.TopRight,
		        color,
		        new SimpleFont("Arial", 14) { Bold = true },
		        Brushes.Transparent,
		        Brushes.Transparent,
		        0
		    );
		}

        private double FieldValueToDouble(object v)
        {
            if (v == null)        return double.NaN;
            if (v is bool   b)    return b ? 1.0 : 0.0;
            if (v is int    i)    return (double)i;
            if (v is long   l)    return (double)l;
            if (v is float  f)    return (double)f;
            if (v is double d)    return d;
            if (v is decimal dc)  return (double)dc;
            return double.NaN;
        }
		
		private ConditionOperator ReverseOperator(ConditionOperator op)
		{
		    switch (op)
		    {
		        case ConditionOperator.Greater:        return ConditionOperator.Less;
		        case ConditionOperator.GreaterOrEqual: return ConditionOperator.LessOrEqual;
		        case ConditionOperator.Less:           return ConditionOperator.Greater;
		        case ConditionOperator.LessOrEqual:    return ConditionOperator.GreaterOrEqual;
		        case ConditionOperator.CrossAbove:     return ConditionOperator.CrossBelow;
		        case ConditionOperator.CrossBelow:     return ConditionOperator.CrossAbove;
		        default:                               return op; // Equals stays as-is
		    }
		}
		
		private void ForceRecolorConditionRows()
		{
		    if (_setTabs == null) { Print("AE Recolor | _setTabs null"); return; }
		
		    int currentSel = _setTabs.SelectedIndex;
		
		    for (int t = 0; t < _setTabs.Items.Count; t++)
		    {
		        // Select each tab to force WPF to render its content
		        _setTabs.SelectedIndex = t;
		        _setTabs.UpdateLayout();
		
		        var tab = _setTabs.Items[t] as TabItem;
		        if (tab == null) continue;
		
		        var panel = tab.Content as StackPanel;
		        if (panel == null) continue;
		
		        foreach (var child in GetAllBorders(panel))
		        {
		            string tt = child.ToolTip as string;
		
		            if (!string.IsNullOrEmpty(tt))
				{
				    // Invalid condition — force red border
				    child.ClearValue(Border.StyleProperty);
				    child.Background      = new SolidColorBrush(Color.FromArgb(255, 80, 20, 20));
				    child.BorderBrush     = new SolidColorBrush(Color.FromArgb(255, 180, 40, 40));
				    child.BorderThickness = new Thickness(1);
				    child.InvalidateVisual();
				
				    // Also color all TextBlocks inside this row red
				    foreach (var tb in GetAllTextBlocks(child))
				    {
				        tb.Foreground = Brushes.Tomato;
				        tb.InvalidateVisual();
				    }
				}
		        }
		    }
		
		    // Restore original tab selection
		    _setTabs.SelectedIndex = (currentSel >= 0 && currentSel < _setTabs.Items.Count)
		        ? currentSel : 0;
		}
		
		private IEnumerable<Border> GetAllBorders(DependencyObject parent)
		{
		    var results = new List<Border>();
		    int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
		    for (int i = 0; i < count; i++)
		    {
		        var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
		        if (child is Border b)
		            results.Add(b);
		        results.AddRange(GetAllBorders(child));
		    }
		    return results;
		}
		
		private IEnumerable<TextBlock> GetAllTextBlocks(DependencyObject parent)
		{
		    var results = new List<TextBlock>();
		    int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
		    for (int i = 0; i < count; i++)
		    {
		        var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
		        if (child is TextBlock tb)
		            results.Add(tb);
		        results.AddRange(GetAllTextBlocks(child));
		    }
		    return results;
		}
		
		private ConditionItem CloneCondition(ConditionItem src)
	{
	    return new ConditionItem
	    {
	        LeftSourceType     = src.LeftSourceType,
	        LeftIndicator      = src.LeftIndicator,
	        LeftIndicatorShort = src.LeftIndicatorShort,
	        LeftPlotIndex      = src.LeftPlotIndex,
	        LeftPlotName       = src.LeftPlotName,
	        LeftPriceType      = src.LeftPriceType,
	        LeftStaticValue    = src.LeftStaticValue,
	        LeftBarsAgo        = src.LeftBarsAgo,
	        LeftFieldName      = src.LeftFieldName,
	        Operator           = src.Operator,
	        RightSourceType    = src.RightSourceType,
	        RightIndicator     = src.RightIndicator,
	        RightIndicatorShort= src.RightIndicatorShort,
	        RightPlotIndex     = src.RightPlotIndex,
	        RightPlotName      = src.RightPlotName,
	        RightPriceType     = src.RightPriceType,
	        RightStaticValue   = src.RightStaticValue,
	        RightBarsAgo       = src.RightBarsAgo,
	        RightFieldName     = src.RightFieldName,
	        ByOffset           = src.ByOffset,
	        AtLeast            = src.AtLeast,
	        AtLeastVal         = src.AtLeastVal,
	        AtMost             = src.AtMost,
	        AtMostVal          = src.AtMostVal
	    };
	}
	
		private string GetConditionError(ConditionItem c)
		{
			
			
			Print("AE ValidCheck | registry count=" + _registry.Count);
    foreach (var r in _registry)
        Print("AE Registry entry | name=" + r.IndicatorName);

    if (c.LeftSourceType == ConditionSourceType.Indicator ||
        c.LeftSourceType == ConditionSourceType.SignalState)
    {
        Print("AE ValidCheck LEFT | indicator=" + c.LeftIndicator);
    }
    if (c.RightSourceType == ConditionSourceType.Indicator ||
        c.RightSourceType == ConditionSourceType.SignalState)
    {
        Print("AE ValidCheck RIGHT | indicator=" + c.RightIndicator);
    }
			
		    // Price and static values are always valid
		    if (c.LeftSourceType == ConditionSourceType.Price ||
		        c.LeftSourceType == ConditionSourceType.StaticValue)
		        goto checkRight;
		
		    // Check left indicator against registry
		    if (!string.IsNullOrEmpty(c.LeftIndicator))
		    {
		        string lShort = c.LeftIndicator.Contains("(")
		            ? c.LeftIndicator.Substring(0, c.LeftIndicator.IndexOf("(")).Trim()
		            : c.LeftIndicator;
		
		        bool found = _registry.Any(r =>
		        {
		            string rShort = r.IndicatorName.Contains("(")
		                ? r.IndicatorName.Substring(0, r.IndicatorName.IndexOf("(")).Trim()
		                : r.IndicatorName;
		            return string.Equals(r.IndicatorName, c.LeftIndicator, StringComparison.OrdinalIgnoreCase)
		                || string.Equals(rShort, lShort, StringComparison.OrdinalIgnoreCase);
		        });
		
		        if (!found)
		            return "Left indicator not on chart: " + lShort;
		    }
		
		    checkRight:
		    if (c.RightSourceType == ConditionSourceType.Price ||
		        c.RightSourceType == ConditionSourceType.StaticValue)
		        return null;
		
		    if (!string.IsNullOrEmpty(c.RightIndicator))
		    {
		        string rShort = c.RightIndicator.Contains("(")
		            ? c.RightIndicator.Substring(0, c.RightIndicator.IndexOf("(")).Trim()
		            : c.RightIndicator;
		
		        bool found = _registry.Any(r =>
		        {
		            string rrShort = r.IndicatorName.Contains("(")
		                ? r.IndicatorName.Substring(0, r.IndicatorName.IndexOf("(")).Trim()
		                : r.IndicatorName;
		            return string.Equals(r.IndicatorName, c.RightIndicator, StringComparison.OrdinalIgnoreCase)
		                || string.Equals(rrShort, rShort, StringComparison.OrdinalIgnoreCase);
		        });
		
		        if (!found)
		            return "Right indicator not on chart: " + rShort;
		    }
		
		    return null;
		}
	
	private List<string> GetAllConditionErrors()
	{
	    var errors = new List<string>();
	    foreach (var set in _sets)
	    {
	        foreach (var c in set.HitBarConditions)
	        {
	            string err = GetConditionError(c);
	            if (err != null)
	                errors.Add("[" + set.Name + " / Hit] " + err);
	        }
	        foreach (var c in set.SignalBarConditions)
	        {
	            string err = GetConditionError(c);
	            if (err != null)
	                errors.Add("[" + set.Name + " / Signal] " + err);
	        }
	    }
	    return errors;
	}
	
	private bool IsInTimeFilters()
{
    var now = Time[0].TimeOfDay;

    Print("AE TimeFilter check — Bar time: " + now 
        + " | WithinCount: " + WithinFilterCount 
        + " | SkipCount: " + SkipFilterCount);

    var withinStarts = new[] { WithinStart1, WithinStart2, WithinStart3, WithinStart4 };
    var withinEnds   = new[] { WithinEnd1,   WithinEnd2,   WithinEnd3,   WithinEnd4   };

    if (WithinFilterCount > 0)
    {
        bool inAny = false;
        for (int i = 0; i < WithinFilterCount && i < 4; i++)
        {
            var s = withinStarts[i];
            var e = withinEnds[i];
            bool inside = s <= e ? (now >= s && now <= e) : (now >= s || now <= e);
            Print("  Within[" + i + "] " + s + " -> " + e + " | inside: " + inside);
            if (inside) { inAny = true; break; }
        }
        if (!inAny)
        {
            Print("  BLOCKED — not inside any Within window");
            return false;
        }
    }

    var skipStarts = new[] { SkipStart1, SkipStart2, SkipStart3, SkipStart4, SkipStart5, SkipStart6 };
    var skipEnds   = new[] { SkipEnd1,   SkipEnd2,   SkipEnd3,   SkipEnd4,   SkipEnd5,   SkipEnd6   };

    for (int i = 0; i < SkipFilterCount && i < 6; i++)
    {
        var s = skipStarts[i];
        var e = skipEnds[i];
        bool inside = s <= e ? (now >= s && now <= e) : (now >= s || now <= e);
        Print("  Skip[" + i + "] " + s + " -> " + e + " | inside: " + inside);
        if (inside)
        {
            Print("  BLOCKED — inside skip window " + i);
            return false;
        }
    }

    Print("  PASSED — time filter OK");
    return true;
}
	
		private void HandleWithinFilterExits()
		{
		    var now        = Time[0].TimeOfDay;
		    var withinEnds = new[] { WithinEnd1, WithinEnd2, WithinEnd3, WithinEnd4 };
		    var exitFlags  = new[] { WithinExitOnEnd1, WithinExitOnEnd2, WithinExitOnEnd3, WithinExitOnEnd4 };
		
		    for (int i = 0; i < WithinFilterCount && i < 4; i++)
		    {
		        if (!exitFlags[i]) continue;
		        var endTime = withinEnds[i];
		        if (now >= endTime && now < endTime.Add(TimeSpan.FromSeconds(30)))
		        {
		            try
		            {
		                // Find the account — use first connected account
		                var account = NinjaTrader.Cbi.Account.All
		                    .FirstOrDefault(a => a.Connection != null &&
		                        a.Connection.Status == NinjaTrader.Cbi.ConnectionStatus.Connected);
		
		                if (account == null) return;
		
		                // Check if we have a position on this instrument
		                Position position = account.Positions
		                    .FirstOrDefault(p => p.Instrument == Instrument);
		
		                if (position == null || position.MarketPosition == MarketPosition.Flat)
		                    return;
		
		                // Submit a flat order
		                account.Flatten(new[] { Instrument });
		            }
		            catch (Exception ex)
		            {
		                Print("AlgoEngine TimeExit error: " + ex.Message);
		            }
		        }
		    }
		}
        // ════════════════════════════════════════════════════════════
        //  CONDITION EVALUATOR
        // ════════════════════════════════════════════════════════════

       private void EvaluateSets()
		{
		    int setIdx = 0;
		    foreach (var set in _sets)
		    {
		        if (!set.IsEnabled) { setIdx++; continue; }
		
		        // ── Hit Bar conditions ──────────────────────────────────────
		        // Exit Long / Exit Short sets skip hit bar evaluation entirely
		        bool isExitAction = set.EntryAction == "Exit Long" || set.EntryAction == "Exit Short";
		
		        bool hitOk = isExitAction
		            || set.HitBarConditions.Count == 0
		            || set.HitBarConditions.All(c => EvalCondition(c));
		
		        if (CurrentBar < 3)
		            Print($"AE EvalSets bar={CurrentBar} set={set.Name} hitConds={set.HitBarConditions.Count} hitOk={hitOk}");
		
		        // Draw hit bar highlight (skip for exit actions)
		        if (hitOk && CurrentBar > 0 && !isExitAction)
		        {
		            var hitColor  = (HitBarHighlightColor as SolidColorBrush)?.Color ?? Colors.Violet;
		            byte alpha    = (byte)(HitBarHighlightOpacity * 255 / 100);
		            var  colBrush = new SolidColorBrush(Color.FromArgb(alpha, hitColor.R, hitColor.G, hitColor.B));
		            string hitTag = "HitCol" + setIdx + "_" + CurrentBar;
		            double colHigh = 1e10;
		            double colLow  = -1e10;
		            Draw.Rectangle(this, hitTag, false, 1, colHigh, 0, colLow,
		                Brushes.Transparent, colBrush, (int)HitBarHighlightOpacity);
		        }
		
		        if (!hitOk) { setIdx++; continue; }
		
		        // ── Signal Bar conditions ───────────────────────────────────
		        bool sigOk = set.SignalBarConditions.Count == 0
		                  || set.SignalBarConditions.All(c => EvalCondition(c));
		
		        if (!sigOk) { setIdx++; continue; }
		
		        // ── Actions ─────────────────────────────────────────────────
		
		        // Buy / Long — requires longOn
		        if (_longOn && (set.EntryAction == "Buy" || set.EntryAction == "Long"))
		        {
		            LastLongSignalBar = CurrentBar;
		            if (AlertMarkerEnabled)
		                Draw.ArrowUp(this, "LongSig" + CurrentBar, false, 0,
		                    Low[0] - 2 * TickSize, MarkerColorBullish);
		            Print("AlgoEngine LONG " + set.Name + " " + Time[0]);
		        }
		        // Sell / Short — requires shortOn
		        else if (_shortOn && (set.EntryAction == "Sell" || set.EntryAction == "Short"))
		        {
		            LastShortSignalBar = CurrentBar;
		            if (AlertMarkerEnabled)
		                Draw.ArrowDown(this, "ShortSig" + CurrentBar, false, 0,
		                    High[0] + 2 * TickSize, MarkerColorBearish);
		            Print("AlgoEngine SHORT " + set.Name + " " + Time[0]);
		        }
		        // Exit Long — flatten long position, no longOn/shortOn required
		        else if (set.EntryAction == "Exit Long")
		        {
		            FlattenPosition(true);
		            if (AlertMarkerEnabled)
		                Draw.ArrowDown(this, "ExitLongSig" + CurrentBar, false, 0,
		                    High[0] + 2 * TickSize, Brushes.Yellow);
		            Print("AlgoEngine EXIT LONG " + set.Name + " " + Time[0]);
		        }
		        // Exit Short — flatten short position, no longOn/shortOn required
		        else if (set.EntryAction == "Exit Short")
		        {
		            FlattenPosition(false);
		            if (AlertMarkerEnabled)
		                Draw.ArrowUp(this, "ExitShortSig" + CurrentBar, false, 0,
		                    Low[0] - 2 * TickSize, Brushes.Yellow);
		            Print("AlgoEngine EXIT SHORT " + set.Name + " " + Time[0]);
		        }
		
		        setIdx++;
		    }
		}
		
		private Account ResolveSelectedAccount()
		{
		    try
		    {
		        foreach (Account acct in Account.All)
		        {
		            if (acct != null && acct.Name.Equals(_selectedAccountName, StringComparison.OrdinalIgnoreCase))
		                return acct;
		        }
		    }
		    catch (Exception ex)
		    {
		        Print("AE ResolveSelectedAccount error: " + ex.Message);
		    }
		    return null;
		}
		
		private void FlattenPosition(bool exitingLong)
		{
		    try
		    {
		        var account = NinjaTrader.Cbi.Account.All
		            .FirstOrDefault(a => a.Connection != null &&
		                a.Connection.Status == NinjaTrader.Cbi.ConnectionStatus.Connected);
		
		        if (account == null)
		        {
		            Print("AE FlattenPosition — no connected account found");
		            return;
		        }
		
		        Position position = account.Positions
		            .FirstOrDefault(p => p.Instrument == Instrument);
		
		        if (position == null || position.MarketPosition == MarketPosition.Flat)
		        {
		            Print("AE FlattenPosition — no open position to exit");
		            return;
		        }
		
		        // Only exit if position direction matches the action
		        if (exitingLong && position.MarketPosition != MarketPosition.Long)
		        {
		            Print("AE FlattenPosition — no long position to exit");
		            return;
		        }
		        if (!exitingLong && position.MarketPosition != MarketPosition.Short)
		        {
		            Print("AE FlattenPosition — no short position to exit");
		            return;
		        }
		
		        account.Flatten(new[] { Instrument });
		        Print("AE FlattenPosition — flattened " + (exitingLong ? "LONG" : "SHORT"));
		    }
		    catch (Exception ex)
		    {
		        Print("AE FlattenPosition error: " + ex.Message);
		    }
		}
		
		private int ResolveOrderQuantity(ConditionSet set)
		{
		    if (_selectedQty > 0)
		        return _selectedQty;
		
		    if (set != null && set.Quantity > 0)
		        return set.Quantity;
		
		    return 1;
		}
		
		private void PlaceAtmEntry(ConditionSet set, bool isLong)
{
    try
    {
        if (set == null)
            return;

        if (string.IsNullOrWhiteSpace(set.AtmStrategy) || set.AtmStrategy == "None")
        {
            Print("AE ATM skipped: no ATM strategy selected for set " + set.Name);
            return;
        }

        Account account = GetSelectedAccount();
        if (account == null)
        {
            Print("AE ATM skipped: account not found: " + _selectedAccountName);
            return;
        }

        int qty = set.Quantity > 0 ? set.Quantity : 1;

        OrderAction action    = isLong ? OrderAction.Buy : OrderAction.SellShort;
        OrderType   orderType = OrderType.Market;
        double      limitPrice = 0;
        double      stopPrice  = 0;

        if (_selectedOrderType == "LMT")
        {
            double px = isLong ? GetCurrentAsk() : GetCurrentBid();
            if (px <= 0 || double.IsNaN(px) || double.IsInfinity(px))
                px = Close[0];

            orderType  = OrderType.Limit;
            limitPrice = px;
        }
        else if (_selectedOrderType == "STP")
        {
            double px = isLong ? GetCurrentAsk() : GetCurrentBid();
            if (px <= 0 || double.IsNaN(px) || double.IsInfinity(px))
                px = Close[0];

            orderType = OrderType.StopMarket;
            stopPrice = px;
        }
		
		
		
		                            string orderId = Guid.NewGuid().ToString("N").Substring(0, 16);
                            string atmId   = Guid.NewGuid().ToString("N").Substring(0, 16);
		
	
                            if (account != null)
                            {
								if (isLong)
								{
                                Order order = account.CreateOrder(
                                    Instrument,
                                    OrderAction.Buy,
                                    OrderType.Market,
                                    OrderEntry.Manual,
                                    TimeInForce.Day,
                                    set.Quantity,
                                    0, 0, "", "Entry",
                                    NinjaTrader.Core.Globals.MaxDate, null);
                                AtmStrategy.StartAtmStrategy(set.AtmStrategy, order);
                                Print("AlgoEngine LONG ATM | " + set.AtmStrategy + " | " + set.Name + " | " + Time[0]);
								}
								else
								{
									 Order order = account.CreateOrder(
                                    Instrument,
                                    OrderAction.Sell,
                                    OrderType.Market,
                                    OrderEntry.Manual,
                                    TimeInForce.Day,
                                    set.Quantity,
                                    0, 0, "", "Entry",
                                    NinjaTrader.Core.Globals.MaxDate, null);
	                                AtmStrategy.StartAtmStrategy(set.AtmStrategy, order);
	                                Print("AlgoEngine LONG ATM | " + set.AtmStrategy + " | " + set.Name + " | " + Time[0]);
								}
						}
		
		
		
		
//        Order entryOrder = account.CreateOrder(
//            Instrument,
//            action,
//            orderType,
//            OrderEntry.Manual,
//            TimeInForce.Day,
//            qty,
//            limitPrice,
//            stopPrice,
//            "",
//            "AE_" + set.Name + "_" + (isLong ? "Long" : "Short"),
//            Core.Globals.MaxDate,
//            null);
		
		
//		Print ("atm:" + set.AtmStrategy);
//        // Attach ATM by name — no object lookup needed
//        NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy("BanksyNQ5b", entryOrder);

        //account.Submit(new[] { entryOrder });

        // Stamp the bar so guards stay in sync
        MarkOrderSubmitted(isLong ? "LONG" : "SHORT");

        Print("AE ATM " + (isLong ? "LONG" : "SHORT")
            + " submitted | set=" + set.Name
            + " | acct=" + account.Name
            + " | qty=" + qty
            + " | type=" + orderType
            + " | atm=" + set.AtmStrategy
            + " | bar=" + CurrentBar);
    }
    catch (Exception ex)
    {
        Print("AE PlaceAtmEntry error: " + ex.Message);
    }
}

        private bool EvalCondition(ConditionItem c)
        {
            double left  = Resolve(c.LeftSourceType,  c.LeftIndicator,  c.LeftPlotIndex,  c.LeftFieldName,  c.LeftPriceType,  c.LeftStaticValue,  c.LeftBarsAgo);
            double right = Resolve(c.RightSourceType, c.RightIndicator, c.RightPlotIndex, c.RightFieldName, c.RightPriceType, c.RightStaticValue, c.RightBarsAgo);

            Print("AE Eval | bar=" + CurrentBar
                + " leftSrc="  + c.LeftSourceType  + " ind=" + c.LeftIndicator
                + " plot="     + c.LeftPlotIndex   + " field=" + c.LeftFieldName
                + " barsAgo="  + c.LeftBarsAgo     + " => left=" + left
                + " | rightSrc=" + c.RightSourceType + " ind=" + c.RightIndicator
                + " plot="     + c.RightPlotIndex  + " field=" + c.RightFieldName
                + " barsAgo="  + c.RightBarsAgo    + " => right=" + right);

            if (double.IsNaN(left) || double.IsNaN(right)) return false;

            switch (c.Operator)
            {
                case ConditionOperator.Greater:        return left > right;
                case ConditionOperator.GreaterOrEqual: return left >= right;
                case ConditionOperator.Less:           return left < right;
                case ConditionOperator.LessOrEqual:    return left <= right;
                case ConditionOperator.Equals:         return Math.Abs(left - right) < 0.00001;
                case ConditionOperator.CrossAbove:
                    if (CurrentBar < 1) return false;
                    return left > right &&
                           Resolve(c.LeftSourceType,  c.LeftIndicator,  c.LeftPlotIndex,  c.LeftFieldName,  c.LeftPriceType,  c.LeftStaticValue,  c.LeftBarsAgo  + 1) <=
                           Resolve(c.RightSourceType, c.RightIndicator, c.RightPlotIndex, c.RightFieldName, c.RightPriceType, c.RightStaticValue, c.RightBarsAgo + 1);
                case ConditionOperator.CrossBelow:
                    if (CurrentBar < 1) return false;
                    return left < right &&
                           Resolve(c.LeftSourceType,  c.LeftIndicator,  c.LeftPlotIndex,  c.LeftFieldName,  c.LeftPriceType,  c.LeftStaticValue,  c.LeftBarsAgo  + 1) >=
                           Resolve(c.RightSourceType, c.RightIndicator, c.RightPlotIndex, c.RightFieldName, c.RightPriceType, c.RightStaticValue, c.RightBarsAgo + 1);
                default: return false;
            }
        }

        private double Resolve(ConditionSourceType src, string indName, int plotIdx, string fieldName,
    AEPriceType price, double staticVal, int barsAgo)
{
    if (barsAgo < 0 || CurrentBar < barsAgo)
        return double.NaN;

    if (src == ConditionSourceType.StaticValue)
        return staticVal;

    if (src == ConditionSourceType.Price)
    {
        switch (price)
        {
            case AEPriceType.Close:  return Close[barsAgo];
            case AEPriceType.Open:   return Open[barsAgo];
            case AEPriceType.High:   return High[barsAgo];
            case AEPriceType.Low:    return Low[barsAgo];
            case AEPriceType.Volume: return Volume[barsAgo];
            default:                 return double.NaN;
        }
    }

    if (ChartControl == null)
        return double.NaN;

    try
    {
        foreach (NinjaScriptBase ns in ChartControl.Indicators)
        {
            if (ns == this)
                continue;

            string nsShort = ns.Name.Contains("(")
                ? ns.Name.Substring(0, ns.Name.IndexOf("(")).Trim()
                : ns.Name;
            string indShort = indName.Contains("(")
                ? indName.Substring(0, indName.IndexOf("(")).Trim()
                : indName;

            bool nameMatch =
                ns.Name.Equals(indName, StringComparison.OrdinalIgnoreCase) ||
                nsShort.Equals(indShort, StringComparison.OrdinalIgnoreCase) ||
                ns.GetType().Name.Equals(indShort, StringComparison.OrdinalIgnoreCase);

            if (!nameMatch)
                continue;

            if (src == ConditionSourceType.SignalState)
            {
                if (string.IsNullOrEmpty(fieldName))
                {
                    Print("AE Resolve SignalState | no fieldName for ind=" + indName);
                    return double.NaN;
                }

                double fv = TryResolveFieldValue(ns, fieldName);
                Print("AE Resolve SignalState | " + ns.Name + " field=" + fieldName + " val=" + fv);
                return fv;
            }

            int vIdx = plotIdx;
            var reg = _registry.FirstOrDefault(r =>
            {
                string rs = r.IndicatorName.Contains("(")
                    ? r.IndicatorName.Substring(0, r.IndicatorName.IndexOf("(")).Trim()
                    : r.IndicatorName;
                return rs.Equals(indShort, StringComparison.OrdinalIgnoreCase)
                    && r.PlotIndex == plotIdx;
            });

            if (reg != null)
                vIdx = reg.ValuesIndex;

            if (ns.Values == null || vIdx < 0 || vIdx >= ns.Values.Length || ns.Values[vIdx] == null)
            {
                Print("AE Resolve NaN | bad Values index vIdx=" + vIdx
                    + " len=" + (ns.Values == null ? 0 : ns.Values.Length)
                    + " ind=" + ns.Name);
                return double.NaN;
            }

            object series = ns.Values[vIdx];
            Type st = series.GetType();
            int targetBarIndex = CurrentBar - barsAgo;

            if (targetBarIndex < 0)
                return double.NaN;

            try
            {
                MethodInfo getValueAt = st.GetMethod("GetValueAt",
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new Type[] { typeof(int) }, null);

                if (getValueAt != null)
                {
                    object rawAt = getValueAt.Invoke(series, new object[] { targetBarIndex });
                    if (rawAt != null)
                    {
                        double dAt = Convert.ToDouble(rawAt);
                        if (!double.IsNaN(dAt) && !double.IsInfinity(dAt))
                        {
                            Print("AE Resolve OK(GetValueAt) | " + ns.Name
                                + " vIdx=" + vIdx + " barIndex=" + targetBarIndex + " val=" + dAt);
                            return dAt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Print("AE Resolve GetValueAt fail | " + ns.Name + " vIdx=" + vIdx + " ex=" + ex.Message);
            }

            try
            {
                if (ns.Values[vIdx].Count <= barsAgo)
                {
                    Print("AE Resolve NaN | not enough bars Count=" + ns.Values[vIdx].Count
                        + " barsAgo=" + barsAgo + " ind=" + ns.Name);
                    return double.NaN;
                }

                double val = ((ISeries<double>)ns.Values[vIdx])[barsAgo];
                Print("AE Resolve OK(Indexer) | " + ns.Name + " vIdx=" + vIdx + " barsAgo=" + barsAgo + " val=" + val);
                return val;
            }
            catch (Exception ex)
            {
                Print("AE Resolve Indexer fail | " + ns.Name + " vIdx=" + vIdx + " ex=" + ex.Message);
                return double.NaN;
            }
        }

        Print("AE Resolve NO MATCH | indName=" + indName + " plotIdx=" + plotIdx);
    }
    catch (Exception ex)
    {
        Print("AE Resolve OUTER | " + ex.Message);
    }

    return double.NaN;
}

        // ════════════════════════════════════════════════════════════
        //  COMPACT WIDGET  (always on chart, single-click opens builder)
        // ════════════════════════════════════════════════════════════

        private Canvas _overlayCanvas;
        private Grid   _rootGrid;       // cached for safe removal on dispose
        private System.Windows.Window _chartWin; // cached chart window

        private void BuildCompactWidget()
        {
            Print("AlgoEngine BuildCompactWidget called | canvas=" + (_overlayCanvas != null ? "exists" : "null"));
            if (_overlayCanvas != null) return;
            if (ChartControl == null)   { Print("AlgoEngine ChartControl null in BuildCompactWidget"); return; }

            // Get the chart's WPF Window and inject a Canvas overlay into it.
            // This way the panel lives inside NT8's own visual tree and can
            // never be closed or hidden by Windows focus/minimize events.
            var chartWin = System.Windows.Window.GetWindow(ChartControl);
            if (chartWin == null) return;

            // Ensure root is a Grid so we can layer the canvas on top
            Grid rootGrid;
            if (chartWin.Content is Grid g)
            {
                rootGrid = g;
            }
            else
            {
                rootGrid = new Grid();
                var old = chartWin.Content as UIElement;
                chartWin.Content = rootGrid;
                if (old != null) rootGrid.Children.Add(old);
            }

            _rootGrid  = rootGrid;   // cache for dispose
            _chartWin  = chartWin;   // cache for dispose
            _overlayCanvas = new Canvas
            {
                Background          = null,
                IsHitTestVisible    = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment.Stretch
            };
            // Add canvas first, then set attached properties
            rootGrid.Children.Add(_overlayCanvas);
            Canvas.SetZIndex(_overlayCanvas, 9999);
            if (rootGrid.RowDefinitions.Count    > 0) Grid.SetRowSpan(_overlayCanvas,    rootGrid.RowDefinitions.Count);
            if (rootGrid.ColumnDefinitions.Count > 0) Grid.SetColumnSpan(_overlayCanvas, rootGrid.ColumnDefinitions.Count);

            // Build the compact widget UI as a Border inside the canvas
            var root = new Border
            {
                Background       = new SolidColorBrush(Color.FromArgb(245, 30, 30, 30)),
                BorderBrush      = new SolidColorBrush(Color.FromArgb(180, 80, 80, 80)),
                BorderThickness  = new Thickness(1),
                CornerRadius     = new CornerRadius(3),
                Width            = 210,
                IsHitTestVisible = true  // widget is clickable even though canvas is not
            };
            var stack = new StackPanel();
            root.Child = stack;

            // Position at saved location
            Canvas.SetLeft(root, WinLeft);
            Canvas.SetTop(root,  WinTop);
            _overlayCanvas.Children.Add(root);

            // Title bar — left-click opens builder, right-drag moves widget
            var titleBar = new Border
            {
                Background = DragBarColor ?? Brushes.LimeGreen,
                Padding    = new Thickness(8, 5, 8, 5),
                Cursor     = Cursors.Hand
            };
            titleBar.Child = new TextBlock
            {
                Text                = "AlgoEngine",
                Foreground          = TitleTextColor ?? Brushes.White,
                FontWeight          = FontWeights.Bold,
                FontSize            = 13,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            bool dragging   = false;
            bool hasMoved   = false;
            double dragOffX = 0, dragOffY = 0;
            Point  downPos  = new Point();
            const double DRAG_THRESHOLD = 4.0; // pixels before we call it a drag

            titleBar.PreviewMouseLeftButtonDown += (s, e) => {
                dragging  = true;
                hasMoved  = false;
                downPos   = e.GetPosition(_overlayCanvas);
                dragOffX  = downPos.X - Canvas.GetLeft(root);
                dragOffY  = downPos.Y - Canvas.GetTop(root);
                titleBar.CaptureMouse();
                e.Handled = true;
            };
            titleBar.PreviewMouseMove += (s, e) => {
                if (!dragging) return;
                var pos = e.GetPosition(_overlayCanvas);
                double dx = pos.X - downPos.X;
                double dy = pos.Y - downPos.Y;
                // Only start moving once past threshold
                if (!hasMoved && Math.Sqrt(dx*dx + dy*dy) < DRAG_THRESHOLD) return;
                hasMoved = true;
                Canvas.SetLeft(root, pos.X - dragOffX);
                Canvas.SetTop(root,  pos.Y - dragOffY);
                e.Handled = true;
            };
            titleBar.PreviewMouseLeftButtonUp += (s, e) => {
                bool didDrag = hasMoved;
                dragging = false;
                hasMoved = false;
                titleBar.ReleaseMouseCapture();
                if (didDrag)
                {
                    // Save position after drag
                    WinLeft   = Canvas.GetLeft(root);
                    WinTop    = Canvas.GetTop(root);
                    e.Handled = true;
                }
                else
                {
                    // Pure click — open condition builder
                    e.Handled = true;
                    OpenBuilderWindow();
                }
            };
            stack.Children.Add(titleBar);

            // CANCEL / CLOSE row
            var row1 = new Grid();
            row1.ColumnDefinitions.Add(new ColumnDefinition());
            row1.ColumnDefinitions.Add(new ColumnDefinition());
            var btnCancel = MakeWidgetBtn("CANCEL", new SolidColorBrush(Color.FromArgb(255, 200, 80, 0)));
            var btnClose  = MakeWidgetBtn("CLOSE",  new SolidColorBrush(Color.FromArgb(255, 80, 80, 80)));
            btnClose.Click += (s, e) => { root.Visibility = Visibility.Collapsed; e.Handled = true; };
            Grid.SetColumn(btnCancel, 0); Grid.SetColumn(btnClose, 1);
            row1.Children.Add(btnCancel); row1.Children.Add(btnClose);
            stack.Children.Add(row1);

            // LONG / SHORT row
            var row2 = new Grid();
            row2.ColumnDefinitions.Add(new ColumnDefinition());
            row2.ColumnDefinitions.Add(new ColumnDefinition());
            _btnLong  = MakeWidgetBtn("LONG",  BtnLongInactiveColor);
            _btnShort = MakeWidgetBtn("SHORT", BtnShortInactiveColor);
           _btnLong.Click += (s, e) =>
			{
			    _longOn = !_longOn;
			    LongEnabledPersist = _longOn;
			    RefreshToggles();
			    e.Handled = true;
			};
			
			_btnShort.Click += (s, e) =>
			{
			    _shortOn = !_shortOn;
			    ShortEnabledPersist = _shortOn;
			    RefreshToggles();
			    e.Handled = true;
			};
            Grid.SetColumn(_btnLong, 0); Grid.SetColumn(_btnShort, 1);
            row2.Children.Add(_btnLong); row2.Children.Add(_btnShort);
            stack.Children.Add(row2);

            // Reference price row
            var row3 = new Grid { Margin = new Thickness(2, 2, 2, 0) };
            row3.ColumnDefinitions.Add(new ColumnDefinition());
            row3.ColumnDefinitions.Add(new ColumnDefinition());
            var refBuy  = MakeDarkCombo(new[] { "Close_Sig...", "Open_Sig...", "Close_Hit...", "Open_Hit..." }, 100);
            var refSell = MakeDarkCombo(new[] { "Close_Sig...", "Open_Sig...", "Close_Hit...", "Open_Hit..." }, 100);
            refBuy.SelectedIndex = 0; refSell.SelectedIndex = 0;
            Grid.SetColumn(refBuy, 0); Grid.SetColumn(refSell, 1);
            row3.Children.Add(refBuy); row3.Children.Add(refSell);
            stack.Children.Add(row3);

            // Order type / Qty row
            var row4 = new Grid { Margin = new Thickness(2, 2, 2, 0) };
            row4.ColumnDefinitions.Add(new ColumnDefinition());
            row4.ColumnDefinitions.Add(new ColumnDefinition());
            var orderType = MakeDarkCombo(new[] { "MKT", "LMT", "STP" }, 100);
			orderType.SelectedIndex = Math.Max(0, new[] { "MKT", "LMT", "STP" }.ToList().IndexOf(_selectedOrderType));
			orderType.SelectionChanged += (s, e) =>
			{
			    if (orderType.SelectedItem != null)
			        _selectedOrderType = orderType.SelectedItem.ToString();
			};
            var qtyCtrl = new Grid();
            qtyCtrl.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            qtyCtrl.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
	          var qtyBox = new TextBox
			{
			    Text = _selectedQty.ToString(),
                Background = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80)),
                FontSize = 12, Height = 26,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding  = new Thickness(4, 0, 4, 0)
            };
            var qtySpinner = new StackPanel { Width = 16 };
            var btnUp   = new Button { Content = "▲", Height = 13, FontSize = 8, Background = new SolidColorBrush(Color.FromArgb(255,60,60,60)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Padding = new Thickness(0) };
            var btnDown = new Button { Content = "▼", Height = 13, FontSize = 8, Background = new SolidColorBrush(Color.FromArgb(255,60,60,60)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Padding = new Thickness(0) };
           btnUp.Click += (s, e) =>
			{
			    if (int.TryParse(qtyBox.Text, out int v))
			    {
			        v++;
			        qtyBox.Text = v.ToString();
			        _selectedQty = Math.Max(1, v);
			    }
			    e.Handled = true;
			};
			
			btnDown.Click += (s, e) =>
			{
			    if (int.TryParse(qtyBox.Text, out int v))
			    {
			        v = Math.Max(1, v - 1);
			        qtyBox.Text = v.ToString();
			        _selectedQty = v;
			    }
			    e.Handled = true;
			};
			
			qtyBox.TextChanged += (s, e) =>
			{
			    if (int.TryParse(qtyBox.Text, out int v) && v > 0)
			        _selectedQty = v;
			};
			  qtySpinner.Children.Add(btnUp); qtySpinner.Children.Add(btnDown);
            Grid.SetColumn(qtyBox, 0); Grid.SetColumn(qtySpinner, 1);
            qtyCtrl.Children.Add(qtyBox); qtyCtrl.Children.Add(qtySpinner);
            Grid.SetColumn(orderType, 0); Grid.SetColumn(qtyCtrl, 1);
            row4.Children.Add(orderType); row4.Children.Add(qtyCtrl);
            stack.Children.Add(row4);

            // Account dropdown — populated from NT8's live account list
            var accountNames = new System.Collections.Generic.List<string>();
            try
            {
                foreach (NinjaTrader.Cbi.Account acct in NinjaTrader.Cbi.Account.All)
                {
                    // Only show accounts that are actually connected (not Backtest/offline stubs)
                    if (acct.Connection != null &&
                        acct.Connection.Status == NinjaTrader.Cbi.ConnectionStatus.Connected)
                        accountNames.Add(acct.Name);
                }
            }
            catch { }
            if (accountNames.Count == 0) accountNames.Add("Sim101");
           
			_accountSelector = MakeDarkCombo(accountNames.ToArray(), 200);

			try
			{
			    string cur = NinjaTrader.Cbi.Account.All.Count > 0
			        ? NinjaTrader.Cbi.Account.All[0].Name
			        : "";
			
			    int ai = accountNames.IndexOf(cur);
			    if (ai >= 0)
			        _accountSelector.SelectedIndex = ai;
			    else
			        _accountSelector.SelectedIndex = 0;
			}
			catch
			{
			    _accountSelector.SelectedIndex = 0;
			}
			
			_selectedAccountName = _accountSelector.SelectedItem != null
			    ? _accountSelector.SelectedItem.ToString()
			    : "Sim101";

			_accountSelector.SelectionChanged += (s, e) =>
			{
			    _selectedAccountName = _accountSelector.SelectedItem != null
			        ? _accountSelector.SelectedItem.ToString()
			        : "Sim101";
			};
			
			stack.Children.Add(new Border
			{
			    Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
			    Padding = new Thickness(4, 2, 4, 2),
			    Child = _accountSelector
			});
						
          

            RefreshToggles();
        }


        // ════════════════════════════════════════════════════════════
        //  CONDITION BUILDER WINDOW  (opens on title click)
        // ════════════════════════════════════════════════════════════

		private void OpenBuilderWindow()
		{
		    if (_builderWindow != null && _builderWindow.IsVisible)
		    {
		        BuildIndicatorRegistry();
		        RebuildSetTabs();
		        _builderWindow.Activate();
		        // Force recolor AFTER full WPF layout is complete
		        _builderWindow.Dispatcher.BeginInvoke(
		            System.Windows.Threading.DispatcherPriority.Background,
		            new Action(() => ForceRecolorConditionRows()));
		        return;
		    }
		    BuildIndicatorRegistry();
		    BuildBuilderWindow();
		    if (_builderWindow != null)
		    {
		        _builderWindow.Dispatcher.BeginInvoke(
		            System.Windows.Threading.DispatcherPriority.Background,
		            new Action(() => ForceRecolorConditionRows()));
		    }
		}
		
        private void BuildBuilderWindow()
        {
            if (_builderWindow != null) try { _builderWindow.Close(); } catch { }

            _builderWindow = new Window
            {
                Width = 780,
    			MinWidth = 780,
                SizeToContent      = SizeToContent.Height,
                WindowStyle        = WindowStyle.None,
                AllowsTransparency = true,
                Background         = Brushes.Transparent,
                Topmost            = false,
                ShowInTaskbar      = false,
                Left               = _compactWindow != null ? _compactWindow.Left + 215 : WinLeft + 215,
                Top                = _compactWindow != null ? _compactWindow.Top        : WinTop,
                ResizeMode         = ResizeMode.NoResize,
                ShowActivated      = false
            };

            var root = new Border
            {
                Background      = new SolidColorBrush(Color.FromArgb(245, 30, 30, 30)),
                BorderBrush     = new SolidColorBrush(Color.FromArgb(200, 80, 80, 80)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(4)
            };
            var rootStack = new StackPanel();
            root.Child = rootStack;

            // Title bar with LONG / SHORT
           

            
			var titleBar  = new Border { Background = DragBarColor ?? Brushes.LimeGreen, Padding = new Thickness(8, 5, 8, 5), Cursor = Cursors.SizeAll };
			var titleGrid = new Grid();
			titleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			
			var titleTb = new TextBlock
			{
			    Text              = "AlgoEngine",
			    Foreground        = TitleTextColor ?? Brushes.White,
			    FontWeight        = FontWeights.Bold,
			    FontSize          = 13,
			    VerticalAlignment = VerticalAlignment.Center
			};
			titleGrid.Children.Add(titleTb);
			titleBar.Child = titleGrid;
			
			
            //titleGrid.Children.Add(bLong);
            //titleGrid.Children.Add(bShort);
           

            bool bdrag = false; double bdx = 0, bdy = 0, bwl = 0, bwt = 0;
            titleBar.MouseLeftButtonDown += (s, e) => { bdrag = true; titleBar.CaptureMouse(); var p = e.GetPosition(null); bdx = p.X; bdy = p.Y; bwl = _builderWindow.Left; bwt = _builderWindow.Top; };
            titleBar.MouseMove           += (s, e) => { if (!bdrag) return; var p = e.GetPosition(null); _builderWindow.Left = bwl + (p.X - bdx); _builderWindow.Top = bwt + (p.Y - bdy); };
            titleBar.MouseLeftButtonUp   += (s, e) => { bdrag = false; titleBar.ReleaseMouseCapture(); };
            rootStack.Children.Add(titleBar);

            // Set tabs
            _setTabs = new TabControl
            {
                Background  = new SolidColorBrush(Color.FromArgb(255, 28, 28, 28)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(80, 80, 80, 80)),
                Foreground  = Brushes.White,
                Margin      = new Thickness(4, 4, 4, 0)
            };
            RebuildSetTabs();
            rootStack.Children.Add(_setTabs);

            // Bottom buttons
            var btmBar = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
                Padding    = new Thickness(8, 6, 8, 6)
            };
            var btmRow = new WrapPanel { Orientation = Orientation.Horizontal };
            btmRow.Children.Add(MakeBottomBtn("SAVE ◆◆", () =>
            {
                if (_builderWindow != null) _builderWindow.Hide();
                SaveSets();
                ChartControl?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try { RedrawAllHitBars(); } catch(Exception ex) { Print("Save error: " + ex.Message); }
                    if (ChartControl != null) ChartControl.InvalidateVisual();
                }));
            }));
            btmRow.Children.Add(MakeBottomBtn("New +",     AddNewSet));
            btmRow.Children.Add(MakeBottomBtn("Save As",   DuplicateCurrentSet));
            btmRow.Children.Add(MakeBottomBtn("Cancel ✕",  () => _builderWindow.Hide()));
           btmRow.Children.Add(MakeBottomBtn("APPLY", () =>
			{
			    var errors = GetAllConditionErrors();
			    if (errors.Count > 0)
			    {
			        // Show error dialog — Cancel still works, Apply is blocked
			        var errDlg = new Window
			        {
			            Width           = 480,
			            SizeToContent   = SizeToContent.Height,
			            WindowStyle     = WindowStyle.None,
			            AllowsTransparency = true,
			            Background      = Brushes.Transparent,
			            Topmost         = true,
			            ShowInTaskbar   = false,
			            Left            = _builderWindow != null ? _builderWindow.Left + 40 : WinLeft + 40,
			            Top             = _builderWindow != null ? _builderWindow.Top  + 60 : WinTop  + 60,
			            ResizeMode      = ResizeMode.NoResize,
			            ShowActivated   = true
			        };
			
			        var errRoot = new Border
			        {
			            Background      = new SolidColorBrush(Color.FromArgb(245, 30, 30, 30)),
			            BorderBrush     = new SolidColorBrush(Color.FromArgb(200, 180, 40, 40)),
			            BorderThickness = new Thickness(1),
			            CornerRadius    = new CornerRadius(4),
			            Padding         = new Thickness(14)
			        };
			
			        var errStack = new StackPanel { Width = 452 };
			
			        errStack.Children.Add(new TextBlock
			        {
			            Text       = "⚠ Cannot Apply — Invalid Conditions",
			            Foreground = Brushes.Tomato,
			            FontWeight = FontWeights.Bold,
			            FontSize   = 13,
			            Margin     = new Thickness(0, 0, 0, 10)
			        });
			
			        errStack.Children.Add(new TextBlock
			        {
			            Text       = "The following conditions reference indicators that are not on the chart.\nFix or remove them before applying.",
			            Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
			            FontSize   = 11,
			            TextWrapping = TextWrapping.Wrap,
			            Margin     = new Thickness(0, 0, 0, 10)
			        });
			
			        foreach (var err in errors)
			        {
			            errStack.Children.Add(new Border
			            {
			                Background      = new SolidColorBrush(Color.FromArgb(255, 60, 20, 20)),
			                BorderBrush     = new SolidColorBrush(Color.FromArgb(120, 180, 60, 60)),
			                BorderThickness = new Thickness(1),
			                CornerRadius    = new CornerRadius(3),
			                Padding         = new Thickness(8, 4, 8, 4),
			                Margin          = new Thickness(0, 2, 0, 2),
			                Child           = new TextBlock
			                {
			                    Text         = err,
			                    Foreground   = Brushes.Tomato,
			                    FontSize     = 11,
			                    TextWrapping = TextWrapping.Wrap,
			                    FontFamily   = new System.Windows.Media.FontFamily("Consolas")
			                }
			            });
			        }
			
			        var errBtnRow = new StackPanel
			        {
			            Orientation         = Orientation.Horizontal,
			            HorizontalAlignment = HorizontalAlignment.Right,
			            Margin              = new Thickness(0, 12, 0, 0)
			        };
			        var btnOkErr = MakeDialogBtn("OK");
			        btnOkErr.Click += (s2, e2) => errDlg.Close();
			        errBtnRow.Children.Add(btnOkErr);
			        errStack.Children.Add(errBtnRow);
			
			        errRoot.Child    = errStack;
			        errDlg.Content   = errRoot;
			        SetNtOwner(errDlg);
			        errDlg.ShowDialog();
			        return; // block Apply
			    }
			
			    // No errors — proceed normally
			    if (_builderWindow != null) _builderWindow.Hide();
			    SaveSets();
			    ChartControl?.Dispatcher.BeginInvoke(new Action(() =>
			    {
			        try { RedrawAllHitBars(); } catch (Exception ex) { Print("Apply error: " + ex.Message); }
			        if (ChartControl != null) ChartControl.InvalidateVisual();
			    }));
			}));
            btmBar.Child = btmRow;
            rootStack.Children.Add(btmBar);

            // Footer tabs
            var footer = new StackPanel { Orientation = Orientation.Horizontal, Background = new SolidColorBrush(Color.FromArgb(255,25,25,25)), Height = 28 };
            footer.Children.Add(MakeFootTab("⚙ Setup"));
            footer.Children.Add(MakeFootTab("📋 Template Manager"));
            rootStack.Children.Add(footer);

            _builderWindow.Content = root;
            SetNtOwner(_builderWindow);
            WindowHelper.MakeNoActivate(_builderWindow);
            _builderWindow.Show();
        }

        // ════════════════════════════════════════════════════════════
        //  SET TABS
        // ════════════════════════════════════════════════════════════

		private void RebuildSetTabs()
		{
		    if (_setTabs == null) return;
		
		    // Preserve current tab selection across rebuilds
		    int prevSel = _setTabs.SelectedIndex;
		
		    _setTabs.Items.Clear();
		    for (int i = 0; i < _sets.Count; i++)
		    {
		        int idx = i;
		        var set = _sets[i];
		        var tab = new TabItem { Header = set.Name, Foreground = Brushes.White };
		        tab.Content = BuildSetPanel(idx);
		
		        var ctx = new ContextMenu
		        {
		            Background  = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
		            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80))
		        };
		
		        var miDuplicate = MakeMenuItem("Duplicate Set");
		        var miRename    = MakeMenuItem("Rename Set...");
		        var miRemove    = MakeMenuItem("Remove Set", isRed: true);
		
		        int capturedIdx = idx;
		
		        miDuplicate.Click += (s, e) =>
		        {
		            _setTabs.SelectedIndex = capturedIdx;
		            DuplicateCurrentSet();
		        };
		
		        miRename.Click += (s, e) =>
		        {
		            var dlg = new Window
		            {
		                Width              = 300,
		                SizeToContent      = SizeToContent.Height,
		                WindowStyle        = WindowStyle.None,
		                AllowsTransparency = true,
		                Background         = Brushes.Transparent,
		                ShowInTaskbar      = false,
		                ShowActivated      = true,
		                Left               = _builderWindow != null ? _builderWindow.Left + 80 : WinLeft + 80,
		                Top                = _builderWindow != null ? _builderWindow.Top  + 80 : WinTop  + 80
		            };
		            var dlgRoot = new Border
		            {
		                Background      = new SolidColorBrush(Color.FromArgb(245, 30, 30, 30)),
		                BorderBrush     = new SolidColorBrush(Color.FromArgb(200, 80, 80, 80)),
		                BorderThickness = new Thickness(1),
		                CornerRadius    = new CornerRadius(4),
		                Padding         = new Thickness(12)
		            };
		            var dlgStack = new StackPanel { Width = 276 };
		            dlgStack.Children.Add(new TextBlock
		            {
		                Text       = "Rename Set",
		                Foreground = Brushes.White,
		                FontWeight = FontWeights.Bold,
		                FontSize   = 13,
		                Margin     = new Thickness(0, 0, 0, 8)
		            });
		            var nameTxt = MakeDarkTxt(_sets[capturedIdx].Name, 276);
		            nameTxt.Width = 276;
		            dlgStack.Children.Add(nameTxt);
		            var btnRow2 = new StackPanel
		            {
		                Orientation         = Orientation.Horizontal,
		                HorizontalAlignment = HorizontalAlignment.Right,
		                Margin              = new Thickness(0, 10, 0, 0)
		            };
		            var btnOk = MakeDialogBtn("OK");
		            var btnCx = MakeDialogBtn("Cancel");
		            btnOk.Click += (s2, e2) =>
		            {
		                _sets[capturedIdx].Name = nameTxt.Text;
		                RebuildSetTabs();
		                if (_setTabs != null) _setTabs.SelectedIndex = capturedIdx;
		                dlg.Close();
		            };
		            btnCx.Click  += (s2, e2) => dlg.Close();
		            nameTxt.KeyDown += (s2, e2) =>
		            {
		                if (e2.Key == Key.Return)
		                    btnOk.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
		            };
		            btnRow2.Children.Add(btnOk);
		            btnRow2.Children.Add(new Border { Width = 8 });
		            btnRow2.Children.Add(btnCx);
		            dlgStack.Children.Add(btnRow2);
		            dlgRoot.Child = dlgStack;
		            dlg.Content   = dlgRoot;
		            dlg.ShowDialog();
		        };
		
		        miRemove.Click += (s, e) =>
		        {
		            if (_sets.Count <= 1) return;
		            _sets.RemoveAt(capturedIdx);
		            RebuildSetTabs();
		            if (_setTabs != null)
		                _setTabs.SelectedIndex = Math.Max(0, capturedIdx - 1);
		        };
		
		        ctx.Items.Add(miDuplicate);
		        ctx.Items.Add(new Separator { Background = new SolidColorBrush(Color.FromArgb(80, 120, 120, 120)) });
		        ctx.Items.Add(miRename);
		        ctx.Items.Add(new Separator { Background = new SolidColorBrush(Color.FromArgb(80, 120, 120, 120)) });
		        ctx.Items.Add(miRemove);
		        tab.ContextMenu = ctx;
		
		        _setTabs.Items.Add(tab);
		    }
		
		    // Restore previous selection, clamped to valid range
		    if (_sets.Count > 0)
		        _setTabs.SelectedIndex = (prevSel >= 0 && prevSel < _sets.Count) ? prevSel : 0;
		}

       
		private UIElement BuildSetPanel(int idx)
		{
		    var set   = _sets[idx];
		    var panel = new StackPanel { Background = new SolidColorBrush(Color.FromArgb(255, 28, 28, 28)) };
		
		    // ── Enabled row ──────────────────────────────────────────────
		    var enableRow = new DockPanel { Margin = new Thickness(6, 6, 6, 4), LastChildFill = false };
		    var chk = new CheckBox
		    {
		        IsChecked  = set.IsEnabled,
		        Content    = new TextBlock { Text = "Set enabled", Foreground = Brushes.White, FontSize = 12 },
		        Foreground = Brushes.White
		    };
		    chk.Checked   += (s, e) => set.IsEnabled = true;
		    chk.Unchecked += (s, e) => set.IsEnabled = false;
		    DockPanel.SetDock(chk, Dock.Left);
		
		    var links = new StackPanel { Orientation = Orientation.Horizontal };
		
		    var btnReverseSet = MakeLinkBtn("Reverse Set");
		    btnReverseSet.Click += (s, e) =>
		    {
		        foreach (var c in set.HitBarConditions)
		            c.Operator = ReverseOperator(c.Operator);
		        foreach (var c in set.SignalBarConditions)
		            c.Operator = ReverseOperator(c.Operator);
		
		        if (set.EntryAction == "Buy" || set.EntryAction == "Long")
		            set.EntryAction = "Sell";
		        else if (set.EntryAction == "Sell" || set.EntryAction == "Short")
		            set.EntryAction = "Buy";
		        // Exit Long / Exit Short are not reversed
		
		        int sel = _setTabs != null ? _setTabs.SelectedIndex : 0;
		        RebuildSetTabs();
		        if (_setTabs != null) _setTabs.SelectedIndex = sel;
		    };
		    links.Children.Add(btnReverseSet);
		    links.Children.Add(MakeLinkBtn("Apply to all sets"));
		    links.Children.Add(MakeLinkBtn("Presets"));
		    DockPanel.SetDock(links, Dock.Right);
		    enableRow.Children.Add(links);
		    enableRow.Children.Add(chk);
		    panel.Children.Add(enableRow);
		    panel.Children.Add(MakeSep());
		
		    // ── Determine if this is an exit action ──────────────────────
		    bool isExitAction = set.EntryAction == "Exit Long" || set.EntryAction == "Exit Short";
		
		    // ── Hit Bar section — hidden for exit actions ─────────────────
		    var hitSection = new StackPanel();
		    if (isExitAction)
		    {
		        hitSection.Children.Add(new Border
		        {
		            Background = new SolidColorBrush(Color.FromArgb(255, 38, 38, 38)),
		            Padding    = new Thickness(8, 6, 8, 6),
		            Child      = new TextBlock
		            {
		                Text       = "↳ Hit Bar conditions not used for Exit actions",
		                Foreground = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120)),
		                FontSize   = 11,
		                FontStyle  = FontStyles.Italic
		            }
		        });
		    }
		    else
		    {
		        hitSection.Children.Add(BuildCondSection("Hit Bar conditions", set.HitBarConditions, false, idx));
		    }
		    panel.Children.Add(hitSection);
		    panel.Children.Add(MakeSep());
		
		    // ── Signal Bar section — always shown ─────────────────────────
		    panel.Children.Add(BuildCondSection("Signal Bar conditions", set.SignalBarConditions, true, idx));
		    panel.Children.Add(MakeSep());
		
		    // ── Entry / Exit row ─────────────────────────────────────────
		    var eeRow = new Grid { Margin = new Thickness(6, 4, 6, 6) };
		    eeRow.ColumnDefinitions.Add(new ColumnDefinition());
		    eeRow.ColumnDefinitions.Add(new ColumnDefinition());
		
		    // Action combo
		    var entryPanel = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
		    entryPanel.Children.Add(new TextBlock
		    {
		        Text       = "Entry / Exit",
		        Foreground = Brushes.White,
		        FontWeight = FontWeights.SemiBold,
		        FontSize   = 11
		    });
		    entryPanel.Children.Add(new TextBlock
		    {
		        Text       = "Action",
		        Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
		        FontSize   = 10
		    });
		
		    var actionCb = MakeDarkCombo(new[] { "None", "Buy", "Sell", "Exit Long", "Exit Short" }, 140);
		    int ai = new[] { "None", "Buy", "Sell", "Exit Long", "Exit Short" }.ToList().IndexOf(set.EntryAction);
		    actionCb.SelectedIndex = ai >= 0 ? ai : 0;
		
		    // Rebuild panel when action type changes so hit bar / ATM sections update
		    actionCb.SelectionChanged += (s, e) =>
		    {
		        if (actionCb.SelectedItem == null) return;
		        set.EntryAction = actionCb.SelectedItem.ToString();
		        int sel = _setTabs != null ? _setTabs.SelectedIndex : idx;
		        RebuildSetTabs();
		        if (_setTabs != null) _setTabs.SelectedIndex = sel;
		    };
		    entryPanel.Children.Add(actionCb);
		    Grid.SetColumn(entryPanel, 0);
		
		    // ATM Strategy combo — hidden for exit actions
		    var exitPanel = new StackPanel();
		    exitPanel.Children.Add(new TextBlock
		    {
		        Text       = "Exit",
		        Foreground = Brushes.White,
		        FontWeight = FontWeights.SemiBold,
		        FontSize   = 11
		    });
		
		    if (isExitAction)
		    {
		        exitPanel.Children.Add(new TextBlock
		        {
		            Text       = "Flattens position via account",
		            Foreground = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120)),
		            FontSize   = 10,
		            FontStyle  = FontStyles.Italic,
		            Margin     = new Thickness(0, 4, 0, 0)
		        });
		    }
		    else
		    {
		        exitPanel.Children.Add(new TextBlock
		        {
		            Text       = "ATM Strategy",
		            Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
		            FontSize   = 10
		        });
		
		        var atmNames = new System.Collections.Generic.List<string> { "None" };
		        try
		        {
		            string ud = NinjaTrader.Core.Globals.UserDataDir;
		            string[] candidateDirs =
		            {
		                System.IO.Path.Combine(ud, "templates", "AtmStrategy"),
		                System.IO.Path.Combine(ud, "Templates",  "AtmStrategy"),
		                System.IO.Path.Combine(ud, "AtmStrategy"),
		            };
		            foreach (var atmDir in candidateDirs)
		            {
		                if (!System.IO.Directory.Exists(atmDir)) continue;
		                foreach (var f in System.IO.Directory.GetFiles(atmDir, "*.xml"))
		                    atmNames.Add(System.IO.Path.GetFileNameWithoutExtension(f));
		                if (atmNames.Count > 1) break;
		            }
		        }
		        catch (Exception ex) { Print("AlgoEngine ATM load error: " + ex.Message); }
		
		        var atmCb    = MakeDarkCombo(atmNames.ToArray(), 160);
		        int atmIdx   = atmNames.IndexOf(set.AtmStrategy);
		        atmCb.SelectedIndex = atmIdx >= 0 ? atmIdx : 0;
		        atmCb.SelectionChanged += (s, e) =>
		        {
		            if (atmCb.SelectedItem != null) set.AtmStrategy = atmCb.SelectedItem.ToString();
		        };
		        exitPanel.Children.Add(atmCb);
		    }
		
		    Grid.SetColumn(exitPanel, 1);
		    eeRow.Children.Add(entryPanel);
		    eeRow.Children.Add(exitPanel);
		    panel.Children.Add(eeRow);
		
		    return panel;
		}
		
        private UIElement BuildCondSection(string title, List<ConditionItem> conditions, bool showBarFields, int setIdx)
		{
		    var section = new StackPanel();
		
		    var header = new DockPanel
		    {
		        Background    = new SolidColorBrush(Color.FromArgb(255, 38, 38, 38)),
		        LastChildFill = false
		    };
		    var titleTb = new TextBlock
		    {
		        Text              = "▼ " + title,
		        Foreground        = Brushes.White,
		        FontWeight        = FontWeights.SemiBold,
		        FontSize          = 12,
		        Margin            = new Thickness(6, 5, 12, 5),
		        VerticalAlignment = VerticalAlignment.Center
		    };
		    DockPanel.SetDock(titleTb, Dock.Left);
		
		    var tools = new StackPanel { Orientation = Orientation.Horizontal };
		    int selectedCondIdx = -1;
		
		    tools.Children.Add(MakeToolBtn("Add", () => ShowAddCondDialog(conditions, setIdx)));
		
		    tools.Children.Add(MakeToolBtn("Duplicate", () =>
		    {
		        int src = selectedCondIdx >= 0 && selectedCondIdx < conditions.Count
		            ? selectedCondIdx : conditions.Count - 1;
		        if (src < 0 || src >= conditions.Count) return;
		        conditions.Insert(src + 1, CloneCondition(conditions[src]));
		        int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		        RebuildSetTabs();
		        if (_setTabs != null) _setTabs.SelectedIndex = sel;
		    }));
		
		    tools.Children.Add(MakeToolBtn("Edit", () =>
		    {
		        int editIdx = selectedCondIdx >= 0 && selectedCondIdx < conditions.Count
		            ? selectedCondIdx : conditions.Count - 1;
		        if (editIdx >= 0 && editIdx < conditions.Count)
		            ShowAddCondDialog(conditions, setIdx, editIdx);
		    }));
		
		    tools.Children.Add(MakeToolBtn("Copy", () =>
		    {
		        int ci = selectedCondIdx >= 0 && selectedCondIdx < conditions.Count
		            ? selectedCondIdx : conditions.Count - 1;
		        if (ci >= 0 && ci < conditions.Count)
		        {
		            _condClipboard = CloneCondition(conditions[ci]);
		            Print("AE Copied condition: " + _condClipboard);
		        }
		    }));
		
		    tools.Children.Add(MakeToolBtn("Paste", () =>
		    {
		        if (_condClipboard == null) return;
		        int insertAt = selectedCondIdx >= 0 && selectedCondIdx < conditions.Count
		            ? selectedCondIdx + 1 : conditions.Count;
		        conditions.Insert(insertAt, CloneCondition(_condClipboard));
		        int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		        RebuildSetTabs();
		        if (_setTabs != null) _setTabs.SelectedIndex = sel;
		    }));
		
		    tools.Children.Add(MakeToolBtn("Reverse", () =>
		    {
		        try
		        {
		            if (conditions == null || conditions.Count == 0) return;
		            int target = selectedCondIdx >= 0 && selectedCondIdx < conditions.Count
		                ? selectedCondIdx : conditions.Count - 1;
		            if (target < 0 || target >= conditions.Count) return;
		            conditions[target].Operator = ReverseOperator(conditions[target].Operator);
		            int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		            RebuildSetTabs();
		            if (_setTabs != null) _setTabs.SelectedIndex = sel;
		        }
		        catch (Exception ex) { Print("AE CondReverse error: " + ex.Message); }
		    }));
		
		    tools.Children.Add(MakeToolBtn("Remove", () =>
		    {
		        int removeIdx = selectedCondIdx >= 0 && selectedCondIdx < conditions.Count
		            ? selectedCondIdx : conditions.Count - 1;
		        if (removeIdx >= 0 && removeIdx < conditions.Count)
		        {
		            conditions.RemoveAt(removeIdx);
		            int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		            RebuildSetTabs();
		            if (_setTabs != null) _setTabs.SelectedIndex = sel;
		        }
		    }, isRed: true));
		
		    DockPanel.SetDock(tools, Dock.Right);
		    header.Children.Add(tools);
		    header.Children.Add(titleTb);
		    section.Children.Add(header);
		
		    if (showBarFields)
		    {
		        var barRow = new StackPanel
		        {
		            Orientation = Orientation.Horizontal,
		            Background  = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33))
		        };
		        barRow.Children.Add(BuildBarField("Bar min", "0"));
		        barRow.Children.Add(BuildBarField("Bar max", "0"));
		        barRow.Children.Add(BuildBarDirField());
		        section.Children.Add(barRow);
		    }
		
		    var listBorder = new Border
		    {
		        Background      = new SolidColorBrush(Color.FromArgb(255, 22, 22, 22)),
		        BorderBrush     = new SolidColorBrush(Color.FromArgb(120, 80, 80, 80)),
		        BorderThickness = new Thickness(1),
		        MinHeight       = 60,
		        Margin          = new Thickness(4, 2, 4, 4)
		    };
		    var listPanel = new StackPanel();
		    Border selectedRowBorder = null;
		
		    for (int ci = 0; ci < conditions.Count; ci++)
		    {
		        int capturedCi = ci;
		        var rowBorder = new Border
		        {
		            Background      = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)),
		            BorderThickness = new Thickness(0, 0, 0, 1),
		            BorderBrush     = new SolidColorBrush(Color.FromArgb(60, 100, 100, 100)),
		            Child           = MakeCondRow(conditions[ci], conditions, ci, setIdx),
		            Cursor          = Cursors.Hand
		        };
				
				// Tint red if condition references a missing indicator
				string condError = GetConditionError(conditions[ci]);
				bool condIsInvalid = condError != null;
				
				// Set initial color
				rowBorder.Background = condIsInvalid
				    ? new SolidColorBrush(Color.FromArgb(255, 80, 20, 20))
				    : new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
				
				if (condIsInvalid)
				    rowBorder.ToolTip = condError;
				
				// Re-apply after layout pass — WPF can reset background during measure/arrange
				rowBorder.Loaded += (s, e) =>
				{
				    rowBorder.Background = condIsInvalid
				        ? new SolidColorBrush(Color.FromArgb(255, 80, 20, 20))
				        : new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
				};
				

				
				if (condIsInvalid)
				    rowBorder.ToolTip = condError;
		
		       rowBorder.MouseLeftButtonDown += (s, e) =>
				{
				    // Deselect previous — restore its correct color
				    if (selectedRowBorder != null)
				    {
				        string prevErr = selectedRowBorder.ToolTip as string;
				        selectedRowBorder.Background = !string.IsNullOrEmpty(prevErr)
				            ? new SolidColorBrush(Color.FromArgb(255, 80, 20, 20))
				            : new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
				    }
				    // Highlight selected
				    rowBorder.Background = new SolidColorBrush(Color.FromArgb(255, 0, 80, 160));
				    selectedRowBorder    = rowBorder;
				    selectedCondIdx      = capturedCi;
				    e.Handled = true;
				};
		
		        rowBorder.MouseLeftButtonDown += (s, e) =>
		        {
		            if (e.ClickCount == 2)
		                ShowAddCondDialog(conditions, setIdx, capturedCi);
		        };
		
		        // ── Context menu — built INSIDE the loop so capturedCi is in scope ──
		        var condCtx = new ContextMenu
		        {
		            Background  = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
		            BorderBrush = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80))
		        };
		
		        var miEdit    = MakeMenuItem("Edit Condition");
		        var miDup     = MakeMenuItem("Duplicate Condition");
		        var miCopy    = MakeMenuItem("Copy Condition");
		        var miPaste   = MakeMenuItem("Paste Condition");
		        var miReverse = MakeMenuItem("Reverse Operator");
		        var miRemove  = MakeMenuItem("Remove Condition", isRed: true);
		
		        condCtx.Opened += (s, e) =>
		        {
		            miPaste.IsEnabled = _condClipboard != null;
		            miPaste.Opacity   = _condClipboard != null ? 1.0 : 0.4;
		        };
		
		        miEdit.Click += (s, e) =>
		        {
		            selectedCondIdx = capturedCi;
		            ShowAddCondDialog(conditions, setIdx, capturedCi);
		        };
		
		        miDup.Click += (s, e) =>
		        {
		            if (capturedCi < 0 || capturedCi >= conditions.Count) return;
		            conditions.Insert(capturedCi + 1, CloneCondition(conditions[capturedCi]));
		            int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		            RebuildSetTabs();
		            if (_setTabs != null) _setTabs.SelectedIndex = sel;
		        };
		
		        miCopy.Click += (s, e) =>
		        {
		            if (capturedCi >= 0 && capturedCi < conditions.Count)
		            {
		                _condClipboard = CloneCondition(conditions[capturedCi]);
		                Print("AE Copied condition: " + _condClipboard);
		            }
		        };
		
		        miPaste.Click += (s, e) =>
		        {
		            if (_condClipboard == null) return;
		            int insertAt = capturedCi >= 0 && capturedCi < conditions.Count
		                ? capturedCi + 1 : conditions.Count;
		            conditions.Insert(insertAt, CloneCondition(_condClipboard));
		            int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		            RebuildSetTabs();
		            if (_setTabs != null) _setTabs.SelectedIndex = sel;
		        };
		
		        miReverse.Click += (s, e) =>
		        {
		            if (capturedCi >= 0 && capturedCi < conditions.Count)
		                conditions[capturedCi].Operator = ReverseOperator(conditions[capturedCi].Operator);
		            int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		            RebuildSetTabs();
		            if (_setTabs != null) _setTabs.SelectedIndex = sel;
		        };
		
		        miRemove.Click += (s, e) =>
		        {
		            if (capturedCi >= 0 && capturedCi < conditions.Count)
		                conditions.RemoveAt(capturedCi);
		            int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
		            RebuildSetTabs();
		            if (_setTabs != null) _setTabs.SelectedIndex = sel;
		        };
		
		        condCtx.Items.Add(miEdit);
		        condCtx.Items.Add(miDup);
		        condCtx.Items.Add(new Separator { Background = new SolidColorBrush(Color.FromArgb(80, 120, 120, 120)) });
		        condCtx.Items.Add(miCopy);
		        condCtx.Items.Add(miPaste);
		        condCtx.Items.Add(new Separator { Background = new SolidColorBrush(Color.FromArgb(80, 120, 120, 120)) });
		        condCtx.Items.Add(miReverse);
		        condCtx.Items.Add(new Separator { Background = new SolidColorBrush(Color.FromArgb(80, 120, 120, 120)) });
		        condCtx.Items.Add(miRemove);
		
		        rowBorder.ContextMenu = condCtx;
		        listPanel.Children.Add(rowBorder);
		    }
		
		    listBorder.Child = listPanel;
		    section.Children.Add(listBorder);
		    return section;
		}
        // MakeCondRow now takes list+index so inline buttons can remove/edit
        private UIElement MakeCondRow(ConditionItem c, List<ConditionItem> list = null, int idx = -1, int setIdx = 0)
        {
            var dp = new DockPanel { LastChildFill = true };

            // Right-side action buttons (Edit ✎ and Remove ✕)
            var btnPanel = new StackPanel
            {
                Orientation       = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(4, 0, 2, 0)
            };

            var btnEdit = new Button
            {
                Content         = "✎",
                Width           = 22, Height = 22,
                FontSize        = 12,
                Background      = new SolidColorBrush(Color.FromArgb(255, 40, 80, 140)),
                Foreground      = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin          = new Thickness(0, 0, 3, 0),
                Padding         = new Thickness(0),
                ToolTip         = "Edit condition",
                Cursor          = Cursors.Hand
            };
            var btnRemove = new Button
            {
                Content         = "✕",
                Width           = 22, Height = 22,
                FontSize        = 11,
                Background      = new SolidColorBrush(Color.FromArgb(255, 140, 30, 30)),
                Foreground      = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding         = new Thickness(0),
                ToolTip         = "Remove condition",
                Cursor          = Cursors.Hand
            };

            btnEdit.Click += (s, e) =>
            {
                if (list != null && idx >= 0)
                    ShowAddCondDialog(list, setIdx, idx);
                e.Handled = true;
            };
            btnRemove.Click += (s, e) =>
            {
                if (list != null && idx >= 0 && idx < list.Count)
                {
                    list.RemoveAt(idx);
                    int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
                    RebuildSetTabs();
                    if (_setTabs != null) _setTabs.SelectedIndex = sel;
                }
                e.Handled = true;
            };

            btnPanel.Children.Add(btnEdit);
            btnPanel.Children.Add(btnRemove);
            DockPanel.SetDock(btnPanel, Dock.Right);
            dp.Children.Add(btnPanel);

            // Condition text
            dp.Children.Add(new TextBlock
            {
                Text              = c.ToString(),
                Foreground        = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
                FontSize          = 11,
                FontFamily        = new FontFamily("Consolas"),
                TextWrapping      = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Margin            = new Thickness(6, 3, 4, 3)
            });

            var row = new Border
            {
                Background      = new SolidColorBrush(Color.FromArgb(255, 45, 45, 45)),
                BorderBrush     = new SolidColorBrush(Color.FromArgb(60, 120, 120, 120)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child           = dp
            };
            row.MouseEnter += (s, e) => row.Background = new SolidColorBrush(Color.FromArgb(255, 58, 58, 58));
            row.MouseLeave += (s, e) => row.Background = new SolidColorBrush(Color.FromArgb(255, 45, 45, 45));
            return row;
        }

        // ════════════════════════════════════════════════════════════
        //  ADD CONDITION DIALOG
        // ════════════════════════════════════════════════════════════

        private void ShowAddCondDialog(List<ConditionItem> targetList, int setIdx, int editIndex = -1)
        {
            BuildIndicatorRegistry();
            // If editing, clone existing condition; otherwise start fresh
            var existing = (editIndex >= 0 && editIndex < targetList.Count) ? targetList[editIndex] : null;
            var newCond = existing != null ? new ConditionItem
{
    LeftSourceType = existing.LeftSourceType,
    LeftIndicator = existing.LeftIndicator,
    LeftIndicatorShort = existing.LeftIndicatorShort,
    LeftPlotIndex = existing.LeftPlotIndex,
    LeftPlotName = existing.LeftPlotName,
    LeftPriceType = existing.LeftPriceType,
    LeftStaticValue = existing.LeftStaticValue,
    LeftBarsAgo = existing.LeftBarsAgo,
    LeftFieldName = existing.LeftFieldName,

    Operator = existing.Operator,

    RightSourceType = existing.RightSourceType,
    RightIndicator = existing.RightIndicator,
    RightIndicatorShort = existing.RightIndicatorShort,
    RightPlotIndex = existing.RightPlotIndex,
    RightPlotName = existing.RightPlotName,
    RightPriceType = existing.RightPriceType,
    RightStaticValue = existing.RightStaticValue,
    RightBarsAgo = existing.RightBarsAgo,
    RightFieldName = existing.RightFieldName,

    ByOffset = existing.ByOffset,
    AtLeast = existing.AtLeast,
    AtLeastVal = existing.AtLeastVal,
    AtMost = existing.AtMost,
    AtMostVal = existing.AtMostVal
} : new ConditionItem();

            var dialog = new Window
            {
                Width              = 510,
                SizeToContent      = SizeToContent.Height,
                WindowStyle        = WindowStyle.ToolWindow,  // gives a thin title bar, enables focus
                AllowsTransparency = false,                   // transparency blocks keyboard in some WPF
                Background         = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)),
                Topmost            = true,
                ShowInTaskbar      = false,
                Left               = _builderWindow != null ? _builderWindow.Left + 30 : WinLeft + 30,
                Top                = _builderWindow != null ? _builderWindow.Top  + 60 : WinTop  + 60,
                ResizeMode         = ResizeMode.NoResize,
                ShowActivated      = true,
                Title              = "Add Condition"
            };

            var root = new Border
            {
                Background      = new SolidColorBrush(Color.FromArgb(245, 30, 30, 30)),
                BorderBrush     = new SolidColorBrush(Color.FromArgb(200, 80, 80, 80)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(4)
            };
            var dstack = new StackPanel();
            root.Child = dstack;

            // Orange title bar
            var dtitle = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 180, 60, 0)),
                Padding    = new Thickness(10, 5, 10, 5),
                Cursor     = Cursors.SizeAll
            };
            dtitle.Child = new TextBlock { Text = "Add Condition", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 13 };
            bool dd = false; double ddx = 0, ddy = 0, dwl = 0, dwt = 0;
            dtitle.MouseLeftButtonDown += (s, e) => { dd = true; dtitle.CaptureMouse(); var p = e.GetPosition(null); ddx = p.X; ddy = p.Y; dwl = dialog.Left; dwt = dialog.Top; };
            dtitle.MouseMove           += (s, e) => { if (!dd) return; var p = e.GetPosition(null); dialog.Left = dwl + (p.X - ddx); dialog.Top = dwt + (p.Y - ddy); };
            dtitle.MouseLeftButtonUp   += (s, e) => { dd = false; dtitle.ReleaseMouseCapture(); };
            dstack.Children.Add(dtitle);

            var body = new StackPanel { Margin = new Thickness(12, 10, 12, 10) };

            // Left source
            body.Children.Add(BuildSrcRow(newCond, isLeft: true));

            // Operator
            var opRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 6, 0, 6) };
            var opNames = new[] { "Greater", "GreaterOrEqual", "Less", "LessOrEqual", "Equals", "CrossAbove", "CrossBelow" };
			var opCb = MakeDarkCombo(opNames, 160);
			int opIdx = Array.IndexOf(opNames, newCond.Operator.ToString());
			opCb.SelectedIndex = opIdx >= 0 ? opIdx : 0;
            opCb.SelectionChanged += (s, e) => {
                if (opCb.SelectedItem != null && Enum.TryParse(opCb.SelectedItem.ToString(), out ConditionOperator op))
                    newCond.Operator = op;
            };
            opRow.Children.Add(opCb);
            body.Children.Add(opRow);

            // Right source
            body.Children.Add(BuildSrcRow(newCond, isLeft: false));

            // Offset row
            var offRow = new WrapPanel { Margin = new Thickness(0, 6, 0, 6) };
            var chkOff  = MakeDarkChk("By Offset");
            var chkAL   = MakeDarkChk("At least");
            var txAL    = MakeDarkTxt("0", 50);
            var chkAM   = MakeDarkChk("At most");
            var txAM    = MakeDarkTxt("0", 50);
            var unitCb  = MakeDarkCombo(new[] { "Tick", "Point", "%" }, 70);
            unitCb.SelectedIndex = 0;
            chkOff.Checked += (s,e) => newCond.ByOffset = true;  chkOff.Unchecked += (s,e) => newCond.ByOffset = false;
            chkAL.Checked  += (s,e) => newCond.AtLeast  = true;  chkAL.Unchecked  += (s,e) => newCond.AtLeast  = false;
            chkAM.Checked  += (s,e) => newCond.AtMost   = true;  chkAM.Unchecked  += (s,e) => newCond.AtMost   = false;
            txAL.TextChanged += (s,e) => { if (double.TryParse(txAL.Text, out double v)) newCond.AtLeastVal = v; };
            txAM.TextChanged += (s,e) => { if (double.TryParse(txAM.Text, out double v)) newCond.AtMostVal  = v; };
            offRow.Children.Add(chkOff); offRow.Children.Add(new Border { Width = 8 });
            offRow.Children.Add(chkAL);  offRow.Children.Add(txAL); offRow.Children.Add(new Border { Width = 8 });
            offRow.Children.Add(chkAM);  offRow.Children.Add(txAM); offRow.Children.Add(new Border { Width = 8 });
            offRow.Children.Add(unitCb);
            body.Children.Add(offRow);

            // Create / Cancel
            var btnRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 8, 0, 0) };
            var btnCreate = MakeDialogBtn("Create");
            var btnCncl   = MakeDialogBtn("Cancel");
            btnCreate.Click += (s, e) =>
            {
                if (editIndex >= 0 && editIndex < targetList.Count)
                    targetList[editIndex] = newCond;  // replace existing
                else
                    targetList.Add(newCond);          // add new
                int sel = _setTabs != null ? _setTabs.SelectedIndex : setIdx;
                RebuildSetTabs();
                if (_setTabs != null) _setTabs.SelectedIndex = sel;
                dialog.Close();
            };
            btnCncl.Click   += (s, e) => dialog.Close();
            btnRow.Children.Add(btnCreate); btnRow.Children.Add(new Border { Width = 8 }); btnRow.Children.Add(btnCncl);
            body.Children.Add(btnRow);

            dstack.Children.Add(body);
            dialog.Content = root;
            SetNtOwner(dialog);
            WindowHelper.MakeNoActivate(dialog);
            dialog.Show();
            dialog.Activate();  // ensure keyboard focus reaches TextBox controls
        }

        private UIElement BuildSrcRow(ConditionItem cond, bool isLeft)
{
    var row = new WrapPanel
    {
        Orientation = Orientation.Horizontal,
        Margin = new Thickness(0, 3, 0, 3)
    };

    bool restoring = true;

    // Source type combo
    var srcCb = MakeDarkCombo(new[] { "Indicator", "Price", "StaticValue", "SignalState" }, 125);
    srcCb.SelectedIndex = 0;
    row.Children.Add(srcCb);
    row.Children.Add(new Border { Width = 4 });

    // ── Indicator controls ────────────────────────────────────
    var indNames = _registry.Select(r => r.IndicatorName).Distinct().ToArray();
    if (indNames.Length == 0) indNames = new[] { "(none)" };

    var indCb = MakeDarkCombo(indNames, 200);
    indCb.SelectedIndex = 0;

    var firstPlots = _registry.Where(r => r.IndicatorName == indNames[0])
                              .Select(r => r.PlotName).ToArray();
    if (firstPlots.Length == 0) firstPlots = new[] { "Plot0" };

    var plotCb = MakeDarkCombo(firstPlots, 100);
    plotCb.SelectedIndex = 0;

    var barsAgoInd = MakeDarkTxt("0", 40);
    var baLabelInd = new TextBlock
    {
        Text = " bars ago",
        Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
        FontSize = 11,
        VerticalAlignment = VerticalAlignment.Center
    };
    var infoInd = MakeInfoBtn();

    // ── Price controls ────────────────────────────────────────
    var priceCb = MakeDarkCombo(new[] { "Close", "Open", "High", "Low", "Volume" }, 90);
    priceCb.SelectedIndex = 0;

    var barsAgoPrice = MakeDarkTxt("0", 40);
    var baLabelPrice = new TextBlock
    {
        Text = " bars ago",
        Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
        FontSize = 11,
        VerticalAlignment = VerticalAlignment.Center
    };
    var infoPrice = MakeInfoBtn();

    priceCb.Visibility = Visibility.Collapsed;
    barsAgoPrice.Visibility = Visibility.Collapsed;
    baLabelPrice.Visibility = Visibility.Collapsed;
    infoPrice.Visibility = Visibility.Collapsed;

    // ── Static value controls ─────────────────────────────────
    var staticTxt = MakeDarkTxt("0", 160);
    staticTxt.Width = 160;
    var infoStatic = MakeInfoBtn();

    staticTxt.Visibility = Visibility.Collapsed;
    infoStatic.Visibility = Visibility.Collapsed;

    // ── SignalState controls ──────────────────────────────────
    var sigIndCb = MakeDarkCombo(indNames, 200);
    sigIndCb.SelectedIndex = 0;

    var fieldCb = MakeDarkCombo(new string[0], 180);
    var fieldLabel = new TextBlock
    {
        Text = " field ",
        Foreground = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)),
        FontSize = 11,
        VerticalAlignment = VerticalAlignment.Center
    };

    sigIndCb.Visibility = Visibility.Collapsed;
    fieldLabel.Visibility = Visibility.Collapsed;
    fieldCb.Visibility = Visibility.Collapsed;

    Action refreshFieldCb = () =>
    {
        string selName = sigIndCb.SelectedItem?.ToString() ?? "";
        string selShort = selName.Contains("(") ? selName.Substring(0, selName.IndexOf("(")).Trim() : selName;
        string prevSel = fieldCb.SelectedItem?.ToString() ?? "";
        string savedField = isLeft ? cond.LeftFieldName : cond.RightFieldName;
        string desiredField = !string.IsNullOrEmpty(savedField) ? savedField : prevSel;

        fieldCb.Items.Clear();
        if (ChartControl == null) return;

        foreach (NinjaScriptBase nsf in ChartControl.Indicators)
        {
            if (nsf == this) continue;

            string nsShort2 = nsf.Name.Contains("(")
                ? nsf.Name.Substring(0, nsf.Name.IndexOf("(")).Trim()
                : nsf.Name;

            if (!nsShort2.Equals(selShort, StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (string fname in GetIndicatorFieldNames(nsf))
                fieldCb.Items.Add(fname);

            break;
        }

        bool restoredField = false;
        for (int fi = 0; fi < fieldCb.Items.Count; fi++)
        {
            if (string.Equals(fieldCb.Items[fi].ToString(), desiredField, StringComparison.OrdinalIgnoreCase))
            {
                fieldCb.SelectedIndex = fi;
                restoredField = true;
                break;
            }
        }

        if (!restoredField && fieldCb.Items.Count > 0)
            fieldCb.SelectedIndex = 0;
    };

    void ApplySourceVisibility(string sel)
    {
        bool isInd = sel == "Indicator";
        bool isPrice = sel == "Price";
        bool isStatic = sel == "StaticValue";
        bool isSig = sel == "SignalState";

        indCb.Visibility = isInd ? Visibility.Visible : Visibility.Collapsed;
        plotCb.Visibility = isInd ? Visibility.Visible : Visibility.Collapsed;
        barsAgoInd.Visibility = isInd ? Visibility.Visible : Visibility.Collapsed;
        baLabelInd.Visibility = isInd ? Visibility.Visible : Visibility.Collapsed;
        infoInd.Visibility = isInd ? Visibility.Visible : Visibility.Collapsed;

        priceCb.Visibility = isPrice ? Visibility.Visible : Visibility.Collapsed;
        barsAgoPrice.Visibility = isPrice ? Visibility.Visible : Visibility.Collapsed;
        baLabelPrice.Visibility = isPrice ? Visibility.Visible : Visibility.Collapsed;
        infoPrice.Visibility = isPrice ? Visibility.Visible : Visibility.Collapsed;

        staticTxt.Visibility = isStatic ? Visibility.Visible : Visibility.Collapsed;
        infoStatic.Visibility = isStatic ? Visibility.Visible : Visibility.Collapsed;

        sigIndCb.Visibility = isSig ? Visibility.Visible : Visibility.Collapsed;
        fieldLabel.Visibility = isSig ? Visibility.Visible : Visibility.Collapsed;
        fieldCb.Visibility = isSig ? Visibility.Visible : Visibility.Collapsed;
    }

    sigIndCb.SelectionChanged += (s, e) =>
    {
        if (restoring) return;

        string name = sigIndCb.SelectedItem?.ToString() ?? "";
        string sname = name.Contains("(") ? name.Substring(0, name.IndexOf("(")).Trim() : name;

        if (isLeft)
        {
            cond.LeftIndicator = name;
            cond.LeftIndicatorShort = sname;
        }
        else
        {
            cond.RightIndicator = name;
            cond.RightIndicatorShort = sname;
        }

        refreshFieldCb();
    };

    fieldCb.SelectionChanged += (s, e) =>
    {
        if (restoring) return;

        string fname = fieldCb.SelectedItem?.ToString() ?? "";
        if (isLeft) cond.LeftFieldName = fname;
        else cond.RightFieldName = fname;
    };

    // Add controls
    row.Children.Add(indCb);
    row.Children.Add(new Border { Width = 4 });
    row.Children.Add(plotCb);
    row.Children.Add(new Border { Width = 4 });
    row.Children.Add(barsAgoInd);
    row.Children.Add(baLabelInd);
    row.Children.Add(infoInd);

    row.Children.Add(priceCb);
    row.Children.Add(new Border { Width = 4 });
    row.Children.Add(barsAgoPrice);
    row.Children.Add(baLabelPrice);
    row.Children.Add(infoPrice);

    row.Children.Add(staticTxt);
    row.Children.Add(infoStatic);

    row.Children.Add(sigIndCb);
    row.Children.Add(fieldLabel);
    row.Children.Add(fieldCb);

    srcCb.SelectionChanged += (s, e) =>
    {
        string sel = srcCb.SelectedItem?.ToString() ?? "Indicator";

        if (Enum.TryParse(sel, out ConditionSourceType st))
        {
            if (isLeft) cond.LeftSourceType = st;
            else cond.RightSourceType = st;
        }

        ApplySourceVisibility(sel);

        if (restoring) return;

        if (sel == "SignalState")
            refreshFieldCb();
    };

    indCb.SelectionChanged += (s, e) =>
    {
        if (restoring) return;

        string name = indCb.SelectedItem?.ToString() ?? "";
        string sname = name.Contains("(") ? name.Substring(0, name.IndexOf("(")).Trim() : name;

        if (isLeft)
        {
            cond.LeftIndicator = name;
            cond.LeftIndicatorShort = sname;
        }
        else
        {
            cond.RightIndicator = name;
            cond.RightIndicatorShort = sname;
        }

        RefreshPlots(indCb, plotCb);
    };

    plotCb.SelectionChanged += (s, e) =>
    {
        if (restoring) return;

        int pidx = plotCb.SelectedIndex >= 0 ? plotCb.SelectedIndex : 0;
        string pname = plotCb.SelectedItem?.ToString() ?? "";

        if (isLeft)
        {
            cond.LeftPlotIndex = pidx;
            cond.LeftPlotName = pname;
        }
        else
        {
            cond.RightPlotIndex = pidx;
            cond.RightPlotName = pname;
        }
    };

    barsAgoInd.TextChanged += (s, e) =>
    {
        if (int.TryParse(barsAgoInd.Text, out int v))
        {
            if (isLeft) cond.LeftBarsAgo = v;
            else cond.RightBarsAgo = v;
        }
    };

    priceCb.SelectionChanged += (s, e) =>
    {
        if (Enum.TryParse(priceCb.SelectedItem?.ToString(), out AEPriceType pt))
        {
            if (isLeft) cond.LeftPriceType = pt;
            else cond.RightPriceType = pt;
        }
    };

    barsAgoPrice.TextChanged += (s, e) =>
    {
        if (int.TryParse(barsAgoPrice.Text, out int v))
        {
            if (isLeft) cond.LeftBarsAgo = v;
            else cond.RightBarsAgo = v;
        }
    };

    staticTxt.TextChanged += (s, e) =>
    {
        if (double.TryParse(staticTxt.Text,
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double v))
        {
            if (isLeft) cond.LeftStaticValue = v;
            else cond.RightStaticValue = v;
        }
    };

    // ── Restore saved values ─────────────────────────────────
    var savedSrcType = isLeft ? cond.LeftSourceType : cond.RightSourceType;
    var savedInd = isLeft ? cond.LeftIndicator : cond.RightIndicator;
    var savedIndShort = isLeft ? cond.LeftIndicatorShort : cond.RightIndicatorShort;
    var savedPlotName = isLeft ? cond.LeftPlotName : cond.RightPlotName;
    var savedPlotIdx = isLeft ? cond.LeftPlotIndex : cond.RightPlotIndex;
    var savedBarsAgo = isLeft ? cond.LeftBarsAgo : cond.RightBarsAgo;
    var savedField = isLeft ? cond.LeftFieldName : cond.RightFieldName;
    var savedPrice = isLeft ? cond.LeftPriceType : cond.RightPriceType;
    var savedStatic = isLeft ? cond.LeftStaticValue : cond.RightStaticValue;

    string savedSrcStr = savedSrcType.ToString();
    int srcIdx = Array.IndexOf(new[] { "Indicator", "Price", "StaticValue", "SignalState" }, savedSrcStr);
    srcCb.SelectedIndex = srcIdx >= 0 ? srcIdx : 0;
    ApplySourceVisibility(savedSrcStr);

    int indIdx = -1;
    if (!string.IsNullOrEmpty(savedInd))
        indIdx = Array.IndexOf(indNames, savedInd);

    if (indIdx < 0)
    {
        if (string.IsNullOrEmpty(savedIndShort) && !string.IsNullOrEmpty(savedInd))
            savedIndShort = savedInd.Contains("(") ? savedInd.Substring(0, savedInd.IndexOf("(")).Trim() : savedInd;

        if (!string.IsNullOrEmpty(savedIndShort))
        {
            indIdx = Array.FindIndex(indNames, n =>
            {
                string ns2 = n.Contains("(") ? n.Substring(0, n.IndexOf("(")).Trim() : n;
                return string.Equals(ns2, savedIndShort, StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    if (indIdx >= 0)
        indCb.SelectedIndex = indIdx;

    RefreshPlots(indCb, plotCb);

    int targetPlot = 0;
    if (!string.IsNullOrEmpty(savedPlotName))
    {
        for (int pi = 0; pi < plotCb.Items.Count; pi++)
        {
            if (string.Equals(plotCb.Items[pi].ToString(), savedPlotName, StringComparison.OrdinalIgnoreCase))
            {
                targetPlot = pi;
                break;
            }
        }
    }
    else if (savedPlotIdx >= 0 && savedPlotIdx < plotCb.Items.Count)
    {
        targetPlot = savedPlotIdx;
    }

    plotCb.SelectedIndex = targetPlot;
    barsAgoInd.Text = savedBarsAgo.ToString();

    int priceIdx = Array.IndexOf(new[] { "Close", "Open", "High", "Low", "Volume" }, savedPrice.ToString());
    if (priceIdx >= 0)
        priceCb.SelectedIndex = priceIdx;
    barsAgoPrice.Text = savedBarsAgo.ToString();

    staticTxt.Text = savedStatic.ToString(System.Globalization.CultureInfo.InvariantCulture);

    if (savedSrcType == ConditionSourceType.SignalState)
    {
        string sigSavedInd = savedInd;
        string sigSavedShort = sigSavedInd.Contains("(")
            ? sigSavedInd.Substring(0, sigSavedInd.IndexOf("(")).Trim()
            : sigSavedInd;

        int sigIdx = Array.FindIndex(indNames, n =>
        {
            string ns2 = n.Contains("(") ? n.Substring(0, n.IndexOf("(")).Trim() : n;
            return ns2.Equals(sigSavedShort, StringComparison.OrdinalIgnoreCase);
        });

        if (sigIdx >= 0)
            sigIndCb.SelectedIndex = sigIdx;

        refreshFieldCb();

        if (!string.IsNullOrEmpty(savedField))
        {
            for (int fi = 0; fi < fieldCb.Items.Count; fi++)
            {
                if (string.Equals(fieldCb.Items[fi].ToString(), savedField, StringComparison.OrdinalIgnoreCase))
                {
                    fieldCb.SelectedIndex = fi;
                    break;
                }
            }
        }
    }

    // Write restored values back once, after all event noise is suppressed
    if (isLeft)
    {
        cond.LeftSourceType = savedSrcType;
        cond.LeftBarsAgo = savedBarsAgo;
        cond.LeftPriceType = savedPrice;
        cond.LeftStaticValue = savedStatic;

        if (savedSrcType == ConditionSourceType.SignalState)
        {
            cond.LeftIndicator = sigIndCb.SelectedItem?.ToString() ?? savedInd;
            cond.LeftIndicatorShort = cond.LeftIndicator.Contains("(")
                ? cond.LeftIndicator.Substring(0, cond.LeftIndicator.IndexOf("(")).Trim()
                : cond.LeftIndicator;
            cond.LeftFieldName = fieldCb.SelectedItem?.ToString() ?? savedField;
        }
        else
        {
            cond.LeftIndicator = indCb.SelectedItem?.ToString() ?? savedInd;
            cond.LeftIndicatorShort = cond.LeftIndicator.Contains("(")
                ? cond.LeftIndicator.Substring(0, cond.LeftIndicator.IndexOf("(")).Trim()
                : cond.LeftIndicator;
            cond.LeftPlotIndex = plotCb.SelectedIndex >= 0 ? plotCb.SelectedIndex : savedPlotIdx;
            cond.LeftPlotName = plotCb.SelectedItem?.ToString() ?? savedPlotName;
        }
    }
    else
    {
        cond.RightSourceType = savedSrcType;
        cond.RightBarsAgo = savedBarsAgo;
        cond.RightPriceType = savedPrice;
        cond.RightStaticValue = savedStatic;

        if (savedSrcType == ConditionSourceType.SignalState)
        {
            cond.RightIndicator = sigIndCb.SelectedItem?.ToString() ?? savedInd;
            cond.RightIndicatorShort = cond.RightIndicator.Contains("(")
                ? cond.RightIndicator.Substring(0, cond.RightIndicator.IndexOf("(")).Trim()
                : cond.RightIndicator;
            cond.RightFieldName = fieldCb.SelectedItem?.ToString() ?? savedField;
        }
        else
        {
            cond.RightIndicator = indCb.SelectedItem?.ToString() ?? savedInd;
            cond.RightIndicatorShort = cond.RightIndicator.Contains("(")
                ? cond.RightIndicator.Substring(0, cond.RightIndicator.IndexOf("(")).Trim()
                : cond.RightIndicator;
            cond.RightPlotIndex = plotCb.SelectedIndex >= 0 ? plotCb.SelectedIndex : savedPlotIdx;
            cond.RightPlotName = plotCb.SelectedItem?.ToString() ?? savedPlotName;
        }
    }

    restoring = false;
    return row;
}


        private void RefreshPlots(ComboBox indCb, ComboBox plotCb)
        {
            plotCb.Items.Clear();
            var name  = indCb.SelectedItem?.ToString() ?? "";
            var plots = _registry.Where(r => r.IndicatorName == name).Select(r => r.PlotName).ToArray();
            foreach (var p in plots) plotCb.Items.Add(p);
            if (plotCb.Items.Count > 0) plotCb.SelectedIndex = 0;
        }

        // ════════════════════════════════════════════════════════════
        //  CONDITION HELPERS
        // ════════════════════════════════════════════════════════════

        private void AddNewSet()
        {
            _sets.Add(new ConditionSet { Name = "Set " + (_sets.Count + 1) });
            RebuildSetTabs();
            // Select the newly created tab
            if (_setTabs != null && _setTabs.Items.Count > 0)
                _setTabs.SelectedIndex = _setTabs.Items.Count - 1;
        }

        private void DuplicateCurrentSet()
        {
            if (_setTabs == null || _sets.Count == 0) return;
            int idx = _setTabs.SelectedIndex >= 0 ? _setTabs.SelectedIndex : 0;
            var src = _sets[idx];
            var copy = new ConditionSet
            {
                Name         = src.Name + " (copy)",
                IsEnabled    = src.IsEnabled,
                EntryAction  = src.EntryAction,
                Quantity     = src.Quantity,
                AtmStrategy  = src.AtmStrategy,
                BarMin       = src.BarMin,
                BarMax       = src.BarMax,
                BarDirection = src.BarDirection
            };
            foreach (var c in src.HitBarConditions)
    copy.HitBarConditions.Add(new ConditionItem
    {
        LeftSourceType = c.LeftSourceType,
        LeftIndicator = c.LeftIndicator,
        LeftIndicatorShort = c.LeftIndicatorShort,
        LeftPlotIndex = c.LeftPlotIndex,
        LeftPlotName = c.LeftPlotName,
        LeftPriceType = c.LeftPriceType,
        LeftStaticValue = c.LeftStaticValue,
        LeftBarsAgo = c.LeftBarsAgo,
        LeftFieldName = c.LeftFieldName,

        Operator = c.Operator,

        RightSourceType = c.RightSourceType,
        RightIndicator = c.RightIndicator,
        RightIndicatorShort = c.RightIndicatorShort,
        RightPlotIndex = c.RightPlotIndex,
        RightPlotName = c.RightPlotName,
        RightPriceType = c.RightPriceType,
        RightStaticValue = c.RightStaticValue,
        RightBarsAgo = c.RightBarsAgo,
        RightFieldName = c.RightFieldName,

        ByOffset = c.ByOffset,
        AtLeast = c.AtLeast,
        AtLeastVal = c.AtLeastVal,
        AtMost = c.AtMost,
        AtMostVal = c.AtMostVal
    });

		foreach (var c in src.SignalBarConditions)
		    copy.SignalBarConditions.Add(new ConditionItem
		    {
		        LeftSourceType = c.LeftSourceType,
		        LeftIndicator = c.LeftIndicator,
		        LeftIndicatorShort = c.LeftIndicatorShort,
		        LeftPlotIndex = c.LeftPlotIndex,
		        LeftPlotName = c.LeftPlotName,
		        LeftPriceType = c.LeftPriceType,
		        LeftStaticValue = c.LeftStaticValue,
		        LeftBarsAgo = c.LeftBarsAgo,
		        LeftFieldName = c.LeftFieldName,
		
		        Operator = c.Operator,
		
		        RightSourceType = c.RightSourceType,
		        RightIndicator = c.RightIndicator,
		        RightIndicatorShort = c.RightIndicatorShort,
		        RightPlotIndex = c.RightPlotIndex,
		        RightPlotName = c.RightPlotName,
		        RightPriceType = c.RightPriceType,
		        RightStaticValue = c.RightStaticValue,
		        RightBarsAgo = c.RightBarsAgo,
		        RightFieldName = c.RightFieldName,
		
		        ByOffset = c.ByOffset,
		        AtLeast = c.AtLeast,
		        AtLeastVal = c.AtLeastVal,
		        AtMost = c.AtMost,
		        AtMostVal = c.AtMostVal
		    });
            _sets.Add(copy);
            RebuildSetTabs();
            if (_setTabs != null)
                _setTabs.SelectedIndex = _setTabs.Items.Count - 1;
        }

        private void DupLastCond(List<ConditionItem> list)
        {
            if (list.Count == 0) return;
            var last = list.Last();
            list.Add(new ConditionItem
            {
                LeftSourceType   = last.LeftSourceType,  LeftIndicator   = last.LeftIndicator,
                LeftPlotIndex    = last.LeftPlotIndex,   LeftPlotName    = last.LeftPlotName,
                LeftIndicatorShort = last.LeftIndicatorShort,
                LeftPriceType    = last.LeftPriceType,   LeftStaticValue = last.LeftStaticValue,
                LeftBarsAgo      = last.LeftBarsAgo,     LeftFieldName   = last.LeftFieldName,
                Operator         = last.Operator,
                RightSourceType  = last.RightSourceType, RightIndicator  = last.RightIndicator,
                RightPlotIndex   = last.RightPlotIndex,  RightPlotName   = last.RightPlotName,
                RightIndicatorShort = last.RightIndicatorShort,
                RightPriceType   = last.RightPriceType,  RightStaticValue= last.RightStaticValue,
                RightBarsAgo     = last.RightBarsAgo,    RightFieldName  = last.RightFieldName,
                ByOffset = last.ByOffset, AtLeast = last.AtLeast, AtLeastVal = last.AtLeastVal,
                AtMost   = last.AtMost,   AtMostVal  = last.AtMostVal
            });
            RebuildSetTabs();
            // Keep selected tab
            if (_setTabs != null && _setTabs.SelectedIndex >= 0)
                _setTabs.SelectedIndex = _setTabs.SelectedIndex;
        }

        private void RemoveLastCond(List<ConditionItem> list)
        {
            if (list.Count == 0) return;
            list.RemoveAt(list.Count - 1);
            RebuildSetTabs();
        }

        // ════════════════════════════════════════════════════════════
        //  UI FACTORIES
        // ════════════════════════════════════════════════════════════

        private Button MakeWidgetBtn(string text, Brush bg)
        {
            return new Button
            {
                Content         = text,
                Height          = 30,
                FontSize        = 12,
                FontWeight      = FontWeights.Bold,
                Foreground      = Brushes.White,
                Background      = bg,
                BorderThickness = new Thickness(1),
                BorderBrush     = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                Cursor          = Cursors.Hand,
                Margin          = new Thickness(1)
            };
        }

        private Button MakeBottomBtn(string text, Action onClick)
        {
            var btn = new Button
            {
                Content         = text,
                Height          = 28,
                MinWidth        = 78,
                FontSize        = 12,
                FontWeight      = FontWeights.SemiBold,
                Background      = new SolidColorBrush(Color.FromArgb(255, 60, 60, 60)),
                Foreground      = Brushes.White,
                BorderBrush     = new SolidColorBrush(Color.FromArgb(255, 90, 90, 90)),
                BorderThickness = new Thickness(1),
                Cursor          = Cursors.Hand,
                Margin          = new Thickness(0, 0, 6, 0),
                Padding         = new Thickness(8, 0, 8, 0)
            };
            btn.Click      += (s, e) => onClick();
            btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(Color.FromArgb(255, 80, 80, 80));
            btn.MouseLeave += (s, e) => btn.Background = new SolidColorBrush(Color.FromArgb(255, 60, 60, 60));
            return btn;
        }

        private Button MakeToolBtn(string text, Action onClick, bool isRed = false)
        {
            var btn = new Button
            {
                Content         = text,
                Height          = 22,
                FontSize        = 11,
                Background      = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground      = isRed ? Brushes.Tomato : new SolidColorBrush(Color.FromArgb(255, 180, 180, 180)),
                Cursor          = Cursors.Hand,
                Margin          = new Thickness(4, 0, 0, 0),
                Padding         = new Thickness(2, 0, 2, 0)
            };
            btn.Click      += (s, e) => onClick();
            btn.MouseEnter += (s, e) => btn.Foreground = Brushes.White;
            btn.MouseLeave += (s, e) => btn.Foreground = isRed ? Brushes.Tomato : new SolidColorBrush(Color.FromArgb(255, 180, 180, 180));
            return btn;
        }

        private Button MakeLinkBtn(string text)
        {
            return new Button { Content = text, Height = 22, FontSize = 11, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Foreground = new SolidColorBrush(Color.FromArgb(255, 100, 160, 255)), Cursor = Cursors.Hand, Margin = new Thickness(0,0,8,0) };
        }

        private MenuItem MakeMenuItem(string text, bool isRed = false)
        {
            var mi = new MenuItem
            {
                Header     = text,
                Foreground = isRed ? Brushes.Tomato : Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40)),
                Padding    = new Thickness(8, 4, 8, 4)
            };
            mi.MouseEnter += (s, e) => mi.Background = new SolidColorBrush(Color.FromArgb(255, 60, 60, 60));
            mi.MouseLeave += (s, e) => mi.Background = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
            return mi;
        }

        private Button MakeInfoBtn()
        {
            var btn = new Button
            {
                Content         = "ℹ",
                Width           = 20, Height = 20,
                Margin          = new Thickness(2,0,0,0),
                Background      = new SolidColorBrush(Color.FromArgb(255,0,150,50)),
                Foreground      = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize        = 11,
                Padding         = new Thickness(0),
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip         = "Indicator info"
            };
            return btn;
        }

        private Button MakeDialogBtn(string text)
        {
            return new Button { Content = text, Width = 90, Height = 28, Background = new SolidColorBrush(Color.FromArgb(255,60,60,60)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromArgb(255,100,100,100)), BorderThickness = new Thickness(1) };
        }

        private Button MakeFootTab(string text)
        {
            return new Button { Content = text, Height = 28, FontSize = 11, Background = Brushes.Transparent, Foreground = new SolidColorBrush(Color.FromArgb(255,160,160,160)), BorderThickness = new Thickness(0), Padding = new Thickness(12,0,12,0), Cursor = Cursors.Hand };
        }

        private ComboBox MakeDarkCombo(string[] items, double width)
        {
            var cb = new ComboBox { Width = width, Height = 26, Background = new SolidColorBrush(Color.FromArgb(255,55,55,55)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromArgb(255,80,80,80)), FontSize = 11 };
            foreach (var item in items) cb.Items.Add(item);
            return cb;
        }

        private TextBox MakeDarkTxt(string text, double width)
        {
            return new TextBox { Text = text, Width = width, Height = 26, Background = new SolidColorBrush(Color.FromArgb(255,55,55,55)), Foreground = Brushes.White, BorderBrush = new SolidColorBrush(Color.FromArgb(255,80,80,80)), FontSize = 11, VerticalContentAlignment = VerticalAlignment.Center, Padding = new Thickness(4,0,4,0) };
        }

        private CheckBox MakeDarkChk(string label)
        {
            return new CheckBox { Content = new TextBlock { Text = label, Foreground = Brushes.White, FontSize = 11 }, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,0,4,0) };
        }

        private Border MakeSep()
        {
            return new Border { Height = 1, Background = new SolidColorBrush(Color.FromArgb(80,100,100,100)), Margin = new Thickness(0,2,0,2) };
        }

        private UIElement BuildBarField(string label, string def)
        {
            var sp = new StackPanel { Margin = new Thickness(6, 4, 12, 4) };
            sp.Children.Add(new TextBlock { Text = label, Foreground = new SolidColorBrush(Color.FromArgb(255,160,160,160)), FontSize = 10 });
            sp.Children.Add(MakeDarkTxt(def, 70));
            return sp;
        }

        private UIElement BuildBarDirField()
        {
            var sp = new StackPanel { Margin = new Thickness(6, 4, 6, 4) };
            sp.Children.Add(new TextBlock { Text = "Bar direction", Foreground = new SolidColorBrush(Color.FromArgb(255,160,160,160)), FontSize = 10 });
            var cb = MakeDarkCombo(new[] { "Any", "Bullish", "Bearish" }, 110);
            cb.SelectedIndex = 0;
            sp.Children.Add(cb);
            return sp;
        }

        private void RefreshToggles()
        {
            if (_btnLong  != null) _btnLong.Background  = _longOn  ? BtnLongActiveColor  : BtnLongInactiveColor;
            if (_btnShort != null) _btnShort.Background = _shortOn ? BtnShortActiveColor : BtnShortInactiveColor;
        }

        // ════════════════════════════════════════════════════════════
        //  DISPOSE
        // ════════════════════════════════════════════════════════════

        // Set NT8 main window as owner so our windows stay within NT8 and
        // never disappear behind other apps
        private void SetNtOwner(Window w)
        {
            // Do NOT set WPF Owner — that causes windows to hide when NT8
            // loses focus. We rely only on Win32 WS_EX_NOACTIVATE instead.
        }

        private void RedrawAllHitBars()
        {
            if (BarsArray == null || BarsArray.Length == 0) return;
            int totalBars = BarsArray[0].Count;
            if (totalBars == 0) return;

            // Remove all existing hit column drawings first
            for (int si = 0; si < _sets.Count; si++)
                for (int b = 0; b < totalBars; b++)
                    RemoveDrawObject("HitCol_" + si + "_" + b);

            // Rebuild registry fresh
            BuildIndicatorRegistry();

            var hitColor = (HitBarHighlightColor as SolidColorBrush)?.Color ?? Colors.DeepPink;
            byte alpha   = (byte)(HitBarHighlightOpacity * 255 / 100);
            var colBrush = new SolidColorBrush(Color.FromArgb(alpha, hitColor.R, hitColor.G, hitColor.B));

            // Evaluate each bar for each set
            for (int si = 0; si < _sets.Count; si++)
            {
                var set = _sets[si];
                if (!set.IsEnabled) continue;
                if (set.HitBarConditions.Count == 0) continue;

                int hitCount = 0;
                for (int b = 1; b < totalBars; b++)
                {
                    bool hitOk = set.HitBarConditions.All(c => EvalConditionAtBar(c, b));
                    if (!hitOk) continue;
                    hitCount++;

                    string tag = "HitCol_" + si + "_" + b;
                    int barsAgoFromCurrent = CurrentBar - b;
                    if (barsAgoFromCurrent < 0) continue;

                    double rectHigh = High[barsAgoFromCurrent] + 1e6;
                    double rectLow  = Low[barsAgoFromCurrent]  - 1e6;
                    Draw.Rectangle(this, tag, false,
                        barsAgoFromCurrent + 1, rectHigh,
                        barsAgoFromCurrent,    rectLow,
                        Brushes.Transparent, colBrush, (int)HitBarHighlightOpacity);
                }
                Print("AE set=" + set.Name + " | hitConds=" + set.HitBarConditions.Count
                    + " | barsMatched=" + hitCount);
            }
            Print("AE RedrawAllHitBars done | totalBars=" + totalBars
                + " | sets=" + _sets.Count + " | CurrentBar=" + CurrentBar);
        }

        // Evaluate a condition at a specific absolute bar index (not barsAgo)
        private bool EvalConditionAtBar(ConditionItem c, int barIdx)
        {
            int barsAgoL = CurrentBar - barIdx + c.LeftBarsAgo;
            int barsAgoR = CurrentBar - barIdx + c.RightBarsAgo;
            if (barsAgoL < 0 || barsAgoR < 0) return false;

            double left  = ResolveAtBarsAgo(c.LeftSourceType,  c.LeftIndicator,  c.LeftPlotIndex,  c.LeftFieldName,  c.LeftPriceType,  c.LeftStaticValue,  barsAgoL);
            double right = ResolveAtBarsAgo(c.RightSourceType, c.RightIndicator, c.RightPlotIndex, c.RightFieldName, c.RightPriceType, c.RightStaticValue, barsAgoR);

            if (double.IsNaN(left) || double.IsNaN(right)) return false;
            switch (c.Operator)
            {
                case ConditionOperator.Greater:        return left > right;
                case ConditionOperator.GreaterOrEqual: return left >= right;
                case ConditionOperator.Less:           return left < right;
                case ConditionOperator.LessOrEqual:    return left <= right;
                case ConditionOperator.Equals:         return Math.Abs(left - right) < 0.00001;
                case ConditionOperator.CrossAbove:
                    if (barIdx < 1) return false;
                    return left > right &&
                           ResolveAtBarsAgo(c.LeftSourceType,  c.LeftIndicator,  c.LeftPlotIndex,  c.LeftFieldName,  c.LeftPriceType,  c.LeftStaticValue,  barsAgoL+1) <=
                           ResolveAtBarsAgo(c.RightSourceType, c.RightIndicator, c.RightPlotIndex, c.RightFieldName, c.RightPriceType, c.RightStaticValue, barsAgoR+1);
                case ConditionOperator.CrossBelow:
                    if (barIdx < 1) return false;
                    return left < right &&
                           ResolveAtBarsAgo(c.LeftSourceType,  c.LeftIndicator,  c.LeftPlotIndex,  c.LeftFieldName,  c.LeftPriceType,  c.LeftStaticValue,  barsAgoL+1) >=
                           ResolveAtBarsAgo(c.RightSourceType, c.RightIndicator, c.RightPlotIndex, c.RightFieldName, c.RightPriceType, c.RightStaticValue, barsAgoR+1);
                default: return false;
            }
        }

        private double ResolveAtBarsAgo(ConditionSourceType src, string indName, int plotIdx, string fieldName,
    AEPriceType price, double staticVal, int barsAgo)
{
    if (barsAgo < 0 || CurrentBar < barsAgo)
        return double.NaN;

    if (src == ConditionSourceType.StaticValue)
        return staticVal;

    if (src == ConditionSourceType.Price)
    {
        switch (price)
        {
            case AEPriceType.Close:  return Close[barsAgo];
            case AEPriceType.Open:   return Open[barsAgo];
            case AEPriceType.High:   return High[barsAgo];
            case AEPriceType.Low:    return Low[barsAgo];
            case AEPriceType.Volume: return Volume[barsAgo];
            default:                 return double.NaN;
        }
    }

    if (ChartControl == null)
        return double.NaN;

    try
    {
        foreach (NinjaScriptBase ns in ChartControl.Indicators)
        {
            if (ns == this)
                continue;

            string nsShort = ns.Name.Contains("(")
                ? ns.Name.Substring(0, ns.Name.IndexOf("(")).Trim()
                : ns.Name;
            string indShort = indName.Contains("(")
                ? indName.Substring(0, indName.IndexOf("(")).Trim()
                : indName;

            bool nameMatch =
                ns.Name.Equals(indName, StringComparison.OrdinalIgnoreCase) ||
                nsShort.Equals(indShort, StringComparison.OrdinalIgnoreCase) ||
                ns.GetType().Name.Equals(indShort, StringComparison.OrdinalIgnoreCase);

            if (!nameMatch)
                continue;

            if (src == ConditionSourceType.SignalState)
            {
                if (string.IsNullOrEmpty(fieldName))
                    return double.NaN;

                return TryResolveFieldValue(ns, fieldName);
            }

            int vIdx = plotIdx;
            var reg = _registry.FirstOrDefault(r =>
            {
                string rs = r.IndicatorName.Contains("(")
                    ? r.IndicatorName.Substring(0, r.IndicatorName.IndexOf("(")).Trim()
                    : r.IndicatorName;
                return rs.Equals(indShort, StringComparison.OrdinalIgnoreCase)
                    && r.PlotIndex == plotIdx;
            });

            if (reg != null)
                vIdx = reg.ValuesIndex;

            if (ns.Values == null || vIdx < 0 || vIdx >= ns.Values.Length || ns.Values[vIdx] == null)
                return double.NaN;

            object series = ns.Values[vIdx];
            Type st = series.GetType();
            int targetBarIndex = CurrentBar - barsAgo;

            if (targetBarIndex < 0)
                return double.NaN;

            try
            {
                MethodInfo getValueAt = st.GetMethod("GetValueAt",
                    BindingFlags.Public | BindingFlags.Instance,
                    null, new Type[] { typeof(int) }, null);

                if (getValueAt != null)
                {
                    object rawAt = getValueAt.Invoke(series, new object[] { targetBarIndex });
                    if (rawAt != null)
                    {
                        double dAt = Convert.ToDouble(rawAt);
                        if (!double.IsNaN(dAt) && !double.IsInfinity(dAt))
                            return dAt;
                    }
                }
            }
            catch { }

            try
            {
                return ((ISeries<double>)ns.Values[vIdx])[barsAgo];
            }
            catch
            {
                return double.NaN;
            }
        }
    }
    catch
    {
    }

    return double.NaN;
}
        // Hand-rolled serialization — avoids assembly conflicts
        private string SerializeSets()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int si = 0; si < _sets.Count; si++)
            {
                if (si > 0) sb.Append("|SET|");
                var s = _sets[si];
                sb.Append(Esc(s.Name)).Append("~").Append(s.IsEnabled).Append("~")
                  .Append(Esc(s.EntryAction)).Append("~").Append(s.Quantity).Append("~")
                  .Append(Esc(s.AtmStrategy));
                sb.Append("|HIT|");
                sb.Append(SerializeConds(s.HitBarConditions));
                sb.Append("|SIG|");
                sb.Append(SerializeConds(s.SignalBarConditions));
            }
            sb.Append("]");
            return sb.ToString();
        }
        private string SerializeConds(List<ConditionItem> conds)
        {
            var sb = new System.Text.StringBuilder();
            for (int ci = 0; ci < conds.Count; ci++)
            {
                if (ci > 0) sb.Append("|COND|");
                var c = conds[ci];
                sb.Append((int)c.LeftSourceType).Append(",").Append(Esc(c.LeftIndicator)).Append(",")
                  .Append(c.LeftPlotIndex).Append(",").Append(Esc(c.LeftPlotName)).Append(",")
                  .Append(Esc(c.LeftIndicatorShort)).Append(",").Append((int)c.LeftPriceType).Append(",")
                  .Append(c.LeftStaticValue.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",").Append(c.LeftBarsAgo).Append(",").Append(Esc(c.LeftFieldName)).Append(";")
                  .Append((int)c.Operator).Append(";")
                  .Append((int)c.RightSourceType).Append(",").Append(Esc(c.RightIndicator)).Append(",")
                  .Append(c.RightPlotIndex).Append(",").Append(Esc(c.RightPlotName)).Append(",")
                  .Append(Esc(c.RightIndicatorShort)).Append(",").Append((int)c.RightPriceType).Append(",")
                  .Append(c.RightStaticValue.ToString(System.Globalization.CultureInfo.InvariantCulture)).Append(",").Append(c.RightBarsAgo).Append(",").Append(Esc(c.RightFieldName));
            }
            return sb.ToString();
        }
        private string Esc(string s) => (s ?? "").Replace("|", "XPIPEX").Replace("~", "XTILDEX").Replace(",", "XCOMMAX").Replace(";", "XSEMIX");
        private string Unesc(string s) => s.Replace("XPIPEX","|").Replace("XTILDEX","~").Replace("XCOMMAX",",").Replace("XSEMIX",";");

        private List<ConditionSet> DeserializeSets(string json)
        {
            var result = new List<ConditionSet>();
            if (string.IsNullOrEmpty(json) || json == "[]") return result;
            string inner = json.TrimStart('[').TrimEnd(']');
            foreach (var setPart in inner.Split(new string[]{"|SET|"}, StringSplitOptions.RemoveEmptyEntries))
            {
                var hitIdx  = setPart.IndexOf("|HIT|");
                var sigIdx  = setPart.IndexOf("|SIG|");
                if (hitIdx < 0 || sigIdx < 0) continue;
                var header  = setPart.Substring(0, hitIdx).Split('~');
                var hitPart = setPart.Substring(hitIdx + 5, sigIdx - hitIdx - 5);
                var sigPart = setPart.Substring(sigIdx + 5);
                if (header.Length < 5) continue;
                var set = new ConditionSet
                {
                    Name        = Unesc(header[0]),
                    IsEnabled   = header[1] == "True",
                    EntryAction = Unesc(header[2]),
                    Quantity    = int.TryParse(header[3], out int q) ? q : 1,
                    AtmStrategy = Unesc(header[4]),
                    HitBarConditions    = DeserializeConds(hitPart),
                    SignalBarConditions = DeserializeConds(sigPart)
                };
                result.Add(set);
            }
            return result;
        }
        private List<ConditionItem> DeserializeConds(string part)
        {
            var result = new List<ConditionItem>();
            if (string.IsNullOrEmpty(part)) return result;
            foreach (var cp in part.Split(new string[]{"|COND|"}, StringSplitOptions.RemoveEmptyEntries))
            {
                var halves = cp.Split(';');
                if (halves.Length < 3) continue;
                var L = halves[0].Split(',');
                var R = halves[2].Split(',');
                if (L.Length < 8 || R.Length < 8) continue;
                var c = new ConditionItem();
                c.LeftSourceType    = (ConditionSourceType)int.Parse(L[0]);
                c.LeftIndicator     = Unesc(L[1]);
                c.LeftPlotIndex     = int.Parse(L[2]);
                c.LeftPlotName      = Unesc(L[3]);
                c.LeftIndicatorShort= Unesc(L[4]);
                c.LeftPriceType     = (AEPriceType)int.Parse(L[5]);
                c.LeftStaticValue   = double.Parse(L[6], System.Globalization.CultureInfo.InvariantCulture);
                c.LeftBarsAgo       = int.Parse(L[7]);
                c.LeftFieldName     = L.Length > 8 ? Unesc(L[8]) : "";
                c.Operator          = (ConditionOperator)int.Parse(halves[1]);
                c.RightSourceType   = (ConditionSourceType)int.Parse(R[0]);
                c.RightIndicator    = Unesc(R[1]);
                c.RightPlotIndex    = int.Parse(R[2]);
                c.RightPlotName     = Unesc(R[3]);
                c.RightIndicatorShort=Unesc(R[4]);
                c.RightPriceType    = (AEPriceType)int.Parse(R[5]);
                c.RightStaticValue  = double.Parse(R[6], System.Globalization.CultureInfo.InvariantCulture);
                c.RightBarsAgo      = int.Parse(R[7]);
                c.RightFieldName    = R.Length > 8 ? Unesc(R[8]) : "";
                result.Add(c);
            }
            return result;
        }

        private void SaveSets()
        {
            try { SetsJson = SerializeSets(); }
            catch (Exception ex) { Print("AE serialize error: " + ex.Message); }
        }

        private void ForceRefresh()
        {
            // In NT8, drawn objects from OnBarUpdate persist by tag.
            // To force a redraw after settings change, we need to
            // trigger a full indicator recalculation.
            ChartControl?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    // Just invalidate visual — drawings persist by tag from OnBarUpdate
                    if (ChartControl != null) ChartControl.InvalidateVisual();
                }
                catch
                {
                    try { if (ChartControl != null) ChartControl.InvalidateVisual(); } catch { }
                }
            }));
        }

        private void DisposeWindows()
        {
            try
            {
                var disp = ChartControl?.Dispatcher ?? Application.Current?.Dispatcher;
                disp?.Invoke(() =>
                {
                    if (_builderWindow != null) { try { _builderWindow.Close(); } catch { } _builderWindow = null; }
                    // Remove canvas overlay — use cached references (ChartControl may be null)
                    if (_overlayCanvas != null)
                    {
                        try { _rootGrid?.Children.Remove(_overlayCanvas); } catch { }
                        try
                        {
                            // Also remove our wrapper grid if we created one
                            if (_rootGrid != null && _chartWin != null
                                && _chartWin.Content == _rootGrid
                                && _rootGrid.Children.Count == 1)
                            {
                                // Unwrap: restore original content
                                var orig = _rootGrid.Children[0] as UIElement;
                                if (orig != null) { _rootGrid.Children.Clear(); _chartWin.Content = orig; }
                            }
                        } catch { }
                        _overlayCanvas = null;
                        _rootGrid      = null;
                        _chartWin      = null;
                    }
                    _panelBuilt = false;
                });
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════
        //  TIME FILTER
        // ════════════════════════════════════════════════════════════

//        private bool IsInSession()
//        {
//            TimeSpan now   = Time[0].TimeOfDay;
//            TimeSpan start = SessionStart.TimeOfDay;
//            TimeSpan end   = SessionEnd.TimeOfDay;
//            if (start <= end) return now >= start && now <= end;
//            return now >= start || now <= end;
//        }

        // ════════════════════════════════════════════════════════════
        //  PUBLIC API
        // ════════════════════════════════════════════════════════════

        public IReadOnlyList<ChartIndicatorPlot> GetRegistry() => _registry.AsReadOnly();
        public double GetPlotValue(string indicatorName, int plotIndex = 0)
        {
            var match = _registry.FirstOrDefault(p => p.IndicatorName.Equals(indicatorName, StringComparison.OrdinalIgnoreCase) && p.PlotIndex == plotIndex);
            return match?.CurrentValue ?? double.NaN;
        }

        // ════════════════════════════════════════════════════════════
        //  PROPERTIES
        // ════════════════════════════════════════════════════════════

       
        #region Time Filter Properties

		// Within Filters
		[NinjaScriptProperty]
		[Display(Name = "Within Filter Count", Order = 200, GroupName = "Time Filters")]
		public int WithinFilterCount { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Start 1", Order = 201, GroupName = "Time Filters")]
		public TimeSpan WithinStart1 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within End 1", Order = 202, GroupName = "Time Filters")]
		public TimeSpan WithinEnd1 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Exit on End 1", Order = 203, GroupName = "Time Filters")]
		public bool WithinExitOnEnd1 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Start 2", Order = 204, GroupName = "Time Filters")]
		public TimeSpan WithinStart2 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within End 2", Order = 205, GroupName = "Time Filters")]
		public TimeSpan WithinEnd2 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Exit on End 2", Order = 206, GroupName = "Time Filters")]
		public bool WithinExitOnEnd2 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Start 3", Order = 207, GroupName = "Time Filters")]
		public TimeSpan WithinStart3 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within End 3", Order = 208, GroupName = "Time Filters")]
		public TimeSpan WithinEnd3 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Exit on End 3", Order = 209, GroupName = "Time Filters")]
		public bool WithinExitOnEnd3 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Start 4", Order = 210, GroupName = "Time Filters")]
		public TimeSpan WithinStart4 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within End 4", Order = 211, GroupName = "Time Filters")]
		public TimeSpan WithinEnd4 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Within Exit on End 4", Order = 212, GroupName = "Time Filters")]
		public bool WithinExitOnEnd4 { get; set; }
		
		// Skip Filters
		[NinjaScriptProperty]
		[Display(Name = "Skip Filter Count", Order = 220, GroupName = "Time Filters")]
		public int SkipFilterCount { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip Start 1", Order = 221, GroupName = "Time Filters")]
		public TimeSpan SkipStart1 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip End 1", Order = 222, GroupName = "Time Filters")]
		public TimeSpan SkipEnd1 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip Start 2", Order = 223, GroupName = "Time Filters")]
		public TimeSpan SkipStart2 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip End 2", Order = 224, GroupName = "Time Filters")]
		public TimeSpan SkipEnd2 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip Start 3", Order = 225, GroupName = "Time Filters")]
		public TimeSpan SkipStart3 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip End 3", Order = 226, GroupName = "Time Filters")]
		public TimeSpan SkipEnd3 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip Start 4", Order = 227, GroupName = "Time Filters")]
		public TimeSpan SkipStart4 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip End 4", Order = 228, GroupName = "Time Filters")]
		public TimeSpan SkipEnd4 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip Start 5", Order = 229, GroupName = "Time Filters")]
		public TimeSpan SkipStart5 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip End 5", Order = 230, GroupName = "Time Filters")]
		public TimeSpan SkipEnd5 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip Start 6", Order = 231, GroupName = "Time Filters")]
		public TimeSpan SkipStart6 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Skip End 6", Order = 232, GroupName = "Time Filters")]
		public TimeSpan SkipEnd6 { get; set; }


        #endregion

        #region Trade Behavior
        [NinjaScriptProperty][Display(Name="Wait Until Flat",Order=1,GroupName="Trade Behavior")]
        public bool WaitUntilFlat { get; set; }
        [NinjaScriptProperty][Display(Name="Entry Cooldown (ms)",Order=2,GroupName="Trade Behavior")]
        public int EntryCooldown { get; set; }
		
        #endregion

        #region Money Management
        [NinjaScriptProperty][Display(Name="Enabled",Order=1,GroupName="Money Management")]
        public bool EnableMoneyMgmt { get; set; }
        [NinjaScriptProperty][Display(Name="Max Daily Profit",Order=2,GroupName="Money Management")]
        public double MaxDailyProfit { get; set; }
        [NinjaScriptProperty][Display(Name="Max Daily Loss",Order=3,GroupName="Money Management")]
        public double MaxDailyLoss { get; set; }
        #endregion

        #region Graphics
        [NinjaScriptProperty][Display(Name="Hit Bar Highlight",Order=1,GroupName="Graphics")]
        public bool HitBarHighlight { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Hit Bar Color",Order=2,GroupName="Graphics")]
        public Brush HitBarHighlightColor { get; set; }
        [NinjaScriptProperty][Range(0,100)][Display(Name="Hit Bar Opacity %",Order=3,GroupName="Graphics")]
        public int HitBarHighlightOpacity { get; set; }
        #endregion

        #region Control Panel
        [NinjaScriptProperty][XmlIgnore][Display(Name="Btn Long Active",   Order=1,GroupName="Control Panel")] public Brush BtnLongActiveColor    { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Btn Long Inactive", Order=2,GroupName="Control Panel")] public Brush BtnLongInactiveColor  { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Btn Short Active",  Order=3,GroupName="Control Panel")] public Brush BtnShortActiveColor   { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Btn Short Inactive",Order=4,GroupName="Control Panel")] public Brush BtnShortInactiveColor { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Drag Bar Color",    Order=5,GroupName="Control Panel")] public Brush DragBarColor           { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Title Text Color",  Order=6,GroupName="Control Panel")] public Brush TitleTextColor         { get; set; }
        [NinjaScriptProperty][Display(Name="Window Left",Order=7,GroupName="Control Panel")] public double WinLeft { get; set; }
        [NinjaScriptProperty][Display(Name="Window Top", Order=8,GroupName="Control Panel")] public double WinTop  { get; set; }
		[NinjaScriptProperty]
		[Display(Name = "Long Enabled Persist", Order = 100, GroupName = "Advanced")]
		public bool LongEnabledPersist { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Short Enabled Persist", Order = 101, GroupName = "Advanced")]
		public bool ShortEnabledPersist { get; set; }
        #endregion

        #region Alerts
        [NinjaScriptProperty][Display(Name="Popup Enabled", Order=1,GroupName="Alerts")] public bool  AlertPopupEnabled  { get; set; }
        [NinjaScriptProperty][Display(Name="Sound Enabled", Order=2,GroupName="Alerts")] public bool  AlertSoundEnabled  { get; set; }
        [NinjaScriptProperty][Display(Name="Marker Enabled",Order=3,GroupName="Alerts")] public bool  AlertMarkerEnabled { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Marker Bullish",Order=4,GroupName="Alerts")] public Brush MarkerColorBullish { get; set; }
        [NinjaScriptProperty][XmlIgnore][Display(Name="Marker Bearish",Order=5,GroupName="Alerts")] public Brush MarkerColorBearish { get; set; }

        // Serialized conditions — survives F5 recompile and chart saves
        [NinjaScriptProperty]
        [Display(Name="Conditions JSON", Order=99, GroupName="Advanced")]
        public string SetsJson { get; set; } = "";
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoEngine[] cacheAlgoEngine;
		public AlgoEngine AlgoEngine(int withinFilterCount, TimeSpan withinStart1, TimeSpan withinEnd1, bool withinExitOnEnd1, TimeSpan withinStart2, TimeSpan withinEnd2, bool withinExitOnEnd2, TimeSpan withinStart3, TimeSpan withinEnd3, bool withinExitOnEnd3, TimeSpan withinStart4, TimeSpan withinEnd4, bool withinExitOnEnd4, int skipFilterCount, TimeSpan skipStart1, TimeSpan skipEnd1, TimeSpan skipStart2, TimeSpan skipEnd2, TimeSpan skipStart3, TimeSpan skipEnd3, TimeSpan skipStart4, TimeSpan skipEnd4, TimeSpan skipStart5, TimeSpan skipEnd5, TimeSpan skipStart6, TimeSpan skipEnd6, bool waitUntilFlat, int entryCooldown, bool enableMoneyMgmt, double maxDailyProfit, double maxDailyLoss, bool hitBarHighlight, Brush hitBarHighlightColor, int hitBarHighlightOpacity, Brush btnLongActiveColor, Brush btnLongInactiveColor, Brush btnShortActiveColor, Brush btnShortInactiveColor, Brush dragBarColor, Brush titleTextColor, double winLeft, double winTop, bool longEnabledPersist, bool shortEnabledPersist, bool alertPopupEnabled, bool alertSoundEnabled, bool alertMarkerEnabled, Brush markerColorBullish, Brush markerColorBearish, string setsJson)
		{
			return AlgoEngine(Input, withinFilterCount, withinStart1, withinEnd1, withinExitOnEnd1, withinStart2, withinEnd2, withinExitOnEnd2, withinStart3, withinEnd3, withinExitOnEnd3, withinStart4, withinEnd4, withinExitOnEnd4, skipFilterCount, skipStart1, skipEnd1, skipStart2, skipEnd2, skipStart3, skipEnd3, skipStart4, skipEnd4, skipStart5, skipEnd5, skipStart6, skipEnd6, waitUntilFlat, entryCooldown, enableMoneyMgmt, maxDailyProfit, maxDailyLoss, hitBarHighlight, hitBarHighlightColor, hitBarHighlightOpacity, btnLongActiveColor, btnLongInactiveColor, btnShortActiveColor, btnShortInactiveColor, dragBarColor, titleTextColor, winLeft, winTop, longEnabledPersist, shortEnabledPersist, alertPopupEnabled, alertSoundEnabled, alertMarkerEnabled, markerColorBullish, markerColorBearish, setsJson);
		}

		public AlgoEngine AlgoEngine(ISeries<double> input, int withinFilterCount, TimeSpan withinStart1, TimeSpan withinEnd1, bool withinExitOnEnd1, TimeSpan withinStart2, TimeSpan withinEnd2, bool withinExitOnEnd2, TimeSpan withinStart3, TimeSpan withinEnd3, bool withinExitOnEnd3, TimeSpan withinStart4, TimeSpan withinEnd4, bool withinExitOnEnd4, int skipFilterCount, TimeSpan skipStart1, TimeSpan skipEnd1, TimeSpan skipStart2, TimeSpan skipEnd2, TimeSpan skipStart3, TimeSpan skipEnd3, TimeSpan skipStart4, TimeSpan skipEnd4, TimeSpan skipStart5, TimeSpan skipEnd5, TimeSpan skipStart6, TimeSpan skipEnd6, bool waitUntilFlat, int entryCooldown, bool enableMoneyMgmt, double maxDailyProfit, double maxDailyLoss, bool hitBarHighlight, Brush hitBarHighlightColor, int hitBarHighlightOpacity, Brush btnLongActiveColor, Brush btnLongInactiveColor, Brush btnShortActiveColor, Brush btnShortInactiveColor, Brush dragBarColor, Brush titleTextColor, double winLeft, double winTop, bool longEnabledPersist, bool shortEnabledPersist, bool alertPopupEnabled, bool alertSoundEnabled, bool alertMarkerEnabled, Brush markerColorBullish, Brush markerColorBearish, string setsJson)
		{
			if (cacheAlgoEngine != null)
				for (int idx = 0; idx < cacheAlgoEngine.Length; idx++)
					if (cacheAlgoEngine[idx] != null && cacheAlgoEngine[idx].WithinFilterCount == withinFilterCount && cacheAlgoEngine[idx].WithinStart1 == withinStart1 && cacheAlgoEngine[idx].WithinEnd1 == withinEnd1 && cacheAlgoEngine[idx].WithinExitOnEnd1 == withinExitOnEnd1 && cacheAlgoEngine[idx].WithinStart2 == withinStart2 && cacheAlgoEngine[idx].WithinEnd2 == withinEnd2 && cacheAlgoEngine[idx].WithinExitOnEnd2 == withinExitOnEnd2 && cacheAlgoEngine[idx].WithinStart3 == withinStart3 && cacheAlgoEngine[idx].WithinEnd3 == withinEnd3 && cacheAlgoEngine[idx].WithinExitOnEnd3 == withinExitOnEnd3 && cacheAlgoEngine[idx].WithinStart4 == withinStart4 && cacheAlgoEngine[idx].WithinEnd4 == withinEnd4 && cacheAlgoEngine[idx].WithinExitOnEnd4 == withinExitOnEnd4 && cacheAlgoEngine[idx].SkipFilterCount == skipFilterCount && cacheAlgoEngine[idx].SkipStart1 == skipStart1 && cacheAlgoEngine[idx].SkipEnd1 == skipEnd1 && cacheAlgoEngine[idx].SkipStart2 == skipStart2 && cacheAlgoEngine[idx].SkipEnd2 == skipEnd2 && cacheAlgoEngine[idx].SkipStart3 == skipStart3 && cacheAlgoEngine[idx].SkipEnd3 == skipEnd3 && cacheAlgoEngine[idx].SkipStart4 == skipStart4 && cacheAlgoEngine[idx].SkipEnd4 == skipEnd4 && cacheAlgoEngine[idx].SkipStart5 == skipStart5 && cacheAlgoEngine[idx].SkipEnd5 == skipEnd5 && cacheAlgoEngine[idx].SkipStart6 == skipStart6 && cacheAlgoEngine[idx].SkipEnd6 == skipEnd6 && cacheAlgoEngine[idx].WaitUntilFlat == waitUntilFlat && cacheAlgoEngine[idx].EntryCooldown == entryCooldown && cacheAlgoEngine[idx].EnableMoneyMgmt == enableMoneyMgmt && cacheAlgoEngine[idx].MaxDailyProfit == maxDailyProfit && cacheAlgoEngine[idx].MaxDailyLoss == maxDailyLoss && cacheAlgoEngine[idx].HitBarHighlight == hitBarHighlight && cacheAlgoEngine[idx].HitBarHighlightColor == hitBarHighlightColor && cacheAlgoEngine[idx].HitBarHighlightOpacity == hitBarHighlightOpacity && cacheAlgoEngine[idx].BtnLongActiveColor == btnLongActiveColor && cacheAlgoEngine[idx].BtnLongInactiveColor == btnLongInactiveColor && cacheAlgoEngine[idx].BtnShortActiveColor == btnShortActiveColor && cacheAlgoEngine[idx].BtnShortInactiveColor == btnShortInactiveColor && cacheAlgoEngine[idx].DragBarColor == dragBarColor && cacheAlgoEngine[idx].TitleTextColor == titleTextColor && cacheAlgoEngine[idx].WinLeft == winLeft && cacheAlgoEngine[idx].WinTop == winTop && cacheAlgoEngine[idx].LongEnabledPersist == longEnabledPersist && cacheAlgoEngine[idx].ShortEnabledPersist == shortEnabledPersist && cacheAlgoEngine[idx].AlertPopupEnabled == alertPopupEnabled && cacheAlgoEngine[idx].AlertSoundEnabled == alertSoundEnabled && cacheAlgoEngine[idx].AlertMarkerEnabled == alertMarkerEnabled && cacheAlgoEngine[idx].MarkerColorBullish == markerColorBullish && cacheAlgoEngine[idx].MarkerColorBearish == markerColorBearish && cacheAlgoEngine[idx].SetsJson == setsJson && cacheAlgoEngine[idx].EqualsInput(input))
						return cacheAlgoEngine[idx];
			return CacheIndicator<AlgoEngine>(new AlgoEngine(){ WithinFilterCount = withinFilterCount, WithinStart1 = withinStart1, WithinEnd1 = withinEnd1, WithinExitOnEnd1 = withinExitOnEnd1, WithinStart2 = withinStart2, WithinEnd2 = withinEnd2, WithinExitOnEnd2 = withinExitOnEnd2, WithinStart3 = withinStart3, WithinEnd3 = withinEnd3, WithinExitOnEnd3 = withinExitOnEnd3, WithinStart4 = withinStart4, WithinEnd4 = withinEnd4, WithinExitOnEnd4 = withinExitOnEnd4, SkipFilterCount = skipFilterCount, SkipStart1 = skipStart1, SkipEnd1 = skipEnd1, SkipStart2 = skipStart2, SkipEnd2 = skipEnd2, SkipStart3 = skipStart3, SkipEnd3 = skipEnd3, SkipStart4 = skipStart4, SkipEnd4 = skipEnd4, SkipStart5 = skipStart5, SkipEnd5 = skipEnd5, SkipStart6 = skipStart6, SkipEnd6 = skipEnd6, WaitUntilFlat = waitUntilFlat, EntryCooldown = entryCooldown, EnableMoneyMgmt = enableMoneyMgmt, MaxDailyProfit = maxDailyProfit, MaxDailyLoss = maxDailyLoss, HitBarHighlight = hitBarHighlight, HitBarHighlightColor = hitBarHighlightColor, HitBarHighlightOpacity = hitBarHighlightOpacity, BtnLongActiveColor = btnLongActiveColor, BtnLongInactiveColor = btnLongInactiveColor, BtnShortActiveColor = btnShortActiveColor, BtnShortInactiveColor = btnShortInactiveColor, DragBarColor = dragBarColor, TitleTextColor = titleTextColor, WinLeft = winLeft, WinTop = winTop, LongEnabledPersist = longEnabledPersist, ShortEnabledPersist = shortEnabledPersist, AlertPopupEnabled = alertPopupEnabled, AlertSoundEnabled = alertSoundEnabled, AlertMarkerEnabled = alertMarkerEnabled, MarkerColorBullish = markerColorBullish, MarkerColorBearish = markerColorBearish, SetsJson = setsJson }, input, ref cacheAlgoEngine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoEngine AlgoEngine(int withinFilterCount, TimeSpan withinStart1, TimeSpan withinEnd1, bool withinExitOnEnd1, TimeSpan withinStart2, TimeSpan withinEnd2, bool withinExitOnEnd2, TimeSpan withinStart3, TimeSpan withinEnd3, bool withinExitOnEnd3, TimeSpan withinStart4, TimeSpan withinEnd4, bool withinExitOnEnd4, int skipFilterCount, TimeSpan skipStart1, TimeSpan skipEnd1, TimeSpan skipStart2, TimeSpan skipEnd2, TimeSpan skipStart3, TimeSpan skipEnd3, TimeSpan skipStart4, TimeSpan skipEnd4, TimeSpan skipStart5, TimeSpan skipEnd5, TimeSpan skipStart6, TimeSpan skipEnd6, bool waitUntilFlat, int entryCooldown, bool enableMoneyMgmt, double maxDailyProfit, double maxDailyLoss, bool hitBarHighlight, Brush hitBarHighlightColor, int hitBarHighlightOpacity, Brush btnLongActiveColor, Brush btnLongInactiveColor, Brush btnShortActiveColor, Brush btnShortInactiveColor, Brush dragBarColor, Brush titleTextColor, double winLeft, double winTop, bool longEnabledPersist, bool shortEnabledPersist, bool alertPopupEnabled, bool alertSoundEnabled, bool alertMarkerEnabled, Brush markerColorBullish, Brush markerColorBearish, string setsJson)
		{
			return indicator.AlgoEngine(Input, withinFilterCount, withinStart1, withinEnd1, withinExitOnEnd1, withinStart2, withinEnd2, withinExitOnEnd2, withinStart3, withinEnd3, withinExitOnEnd3, withinStart4, withinEnd4, withinExitOnEnd4, skipFilterCount, skipStart1, skipEnd1, skipStart2, skipEnd2, skipStart3, skipEnd3, skipStart4, skipEnd4, skipStart5, skipEnd5, skipStart6, skipEnd6, waitUntilFlat, entryCooldown, enableMoneyMgmt, maxDailyProfit, maxDailyLoss, hitBarHighlight, hitBarHighlightColor, hitBarHighlightOpacity, btnLongActiveColor, btnLongInactiveColor, btnShortActiveColor, btnShortInactiveColor, dragBarColor, titleTextColor, winLeft, winTop, longEnabledPersist, shortEnabledPersist, alertPopupEnabled, alertSoundEnabled, alertMarkerEnabled, markerColorBullish, markerColorBearish, setsJson);
		}

		public Indicators.AlgoEngine AlgoEngine(ISeries<double> input , int withinFilterCount, TimeSpan withinStart1, TimeSpan withinEnd1, bool withinExitOnEnd1, TimeSpan withinStart2, TimeSpan withinEnd2, bool withinExitOnEnd2, TimeSpan withinStart3, TimeSpan withinEnd3, bool withinExitOnEnd3, TimeSpan withinStart4, TimeSpan withinEnd4, bool withinExitOnEnd4, int skipFilterCount, TimeSpan skipStart1, TimeSpan skipEnd1, TimeSpan skipStart2, TimeSpan skipEnd2, TimeSpan skipStart3, TimeSpan skipEnd3, TimeSpan skipStart4, TimeSpan skipEnd4, TimeSpan skipStart5, TimeSpan skipEnd5, TimeSpan skipStart6, TimeSpan skipEnd6, bool waitUntilFlat, int entryCooldown, bool enableMoneyMgmt, double maxDailyProfit, double maxDailyLoss, bool hitBarHighlight, Brush hitBarHighlightColor, int hitBarHighlightOpacity, Brush btnLongActiveColor, Brush btnLongInactiveColor, Brush btnShortActiveColor, Brush btnShortInactiveColor, Brush dragBarColor, Brush titleTextColor, double winLeft, double winTop, bool longEnabledPersist, bool shortEnabledPersist, bool alertPopupEnabled, bool alertSoundEnabled, bool alertMarkerEnabled, Brush markerColorBullish, Brush markerColorBearish, string setsJson)
		{
			return indicator.AlgoEngine(input, withinFilterCount, withinStart1, withinEnd1, withinExitOnEnd1, withinStart2, withinEnd2, withinExitOnEnd2, withinStart3, withinEnd3, withinExitOnEnd3, withinStart4, withinEnd4, withinExitOnEnd4, skipFilterCount, skipStart1, skipEnd1, skipStart2, skipEnd2, skipStart3, skipEnd3, skipStart4, skipEnd4, skipStart5, skipEnd5, skipStart6, skipEnd6, waitUntilFlat, entryCooldown, enableMoneyMgmt, maxDailyProfit, maxDailyLoss, hitBarHighlight, hitBarHighlightColor, hitBarHighlightOpacity, btnLongActiveColor, btnLongInactiveColor, btnShortActiveColor, btnShortInactiveColor, dragBarColor, titleTextColor, winLeft, winTop, longEnabledPersist, shortEnabledPersist, alertPopupEnabled, alertSoundEnabled, alertMarkerEnabled, markerColorBullish, markerColorBearish, setsJson);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoEngine AlgoEngine(int withinFilterCount, TimeSpan withinStart1, TimeSpan withinEnd1, bool withinExitOnEnd1, TimeSpan withinStart2, TimeSpan withinEnd2, bool withinExitOnEnd2, TimeSpan withinStart3, TimeSpan withinEnd3, bool withinExitOnEnd3, TimeSpan withinStart4, TimeSpan withinEnd4, bool withinExitOnEnd4, int skipFilterCount, TimeSpan skipStart1, TimeSpan skipEnd1, TimeSpan skipStart2, TimeSpan skipEnd2, TimeSpan skipStart3, TimeSpan skipEnd3, TimeSpan skipStart4, TimeSpan skipEnd4, TimeSpan skipStart5, TimeSpan skipEnd5, TimeSpan skipStart6, TimeSpan skipEnd6, bool waitUntilFlat, int entryCooldown, bool enableMoneyMgmt, double maxDailyProfit, double maxDailyLoss, bool hitBarHighlight, Brush hitBarHighlightColor, int hitBarHighlightOpacity, Brush btnLongActiveColor, Brush btnLongInactiveColor, Brush btnShortActiveColor, Brush btnShortInactiveColor, Brush dragBarColor, Brush titleTextColor, double winLeft, double winTop, bool longEnabledPersist, bool shortEnabledPersist, bool alertPopupEnabled, bool alertSoundEnabled, bool alertMarkerEnabled, Brush markerColorBullish, Brush markerColorBearish, string setsJson)
		{
			return indicator.AlgoEngine(Input, withinFilterCount, withinStart1, withinEnd1, withinExitOnEnd1, withinStart2, withinEnd2, withinExitOnEnd2, withinStart3, withinEnd3, withinExitOnEnd3, withinStart4, withinEnd4, withinExitOnEnd4, skipFilterCount, skipStart1, skipEnd1, skipStart2, skipEnd2, skipStart3, skipEnd3, skipStart4, skipEnd4, skipStart5, skipEnd5, skipStart6, skipEnd6, waitUntilFlat, entryCooldown, enableMoneyMgmt, maxDailyProfit, maxDailyLoss, hitBarHighlight, hitBarHighlightColor, hitBarHighlightOpacity, btnLongActiveColor, btnLongInactiveColor, btnShortActiveColor, btnShortInactiveColor, dragBarColor, titleTextColor, winLeft, winTop, longEnabledPersist, shortEnabledPersist, alertPopupEnabled, alertSoundEnabled, alertMarkerEnabled, markerColorBullish, markerColorBearish, setsJson);
		}

		public Indicators.AlgoEngine AlgoEngine(ISeries<double> input , int withinFilterCount, TimeSpan withinStart1, TimeSpan withinEnd1, bool withinExitOnEnd1, TimeSpan withinStart2, TimeSpan withinEnd2, bool withinExitOnEnd2, TimeSpan withinStart3, TimeSpan withinEnd3, bool withinExitOnEnd3, TimeSpan withinStart4, TimeSpan withinEnd4, bool withinExitOnEnd4, int skipFilterCount, TimeSpan skipStart1, TimeSpan skipEnd1, TimeSpan skipStart2, TimeSpan skipEnd2, TimeSpan skipStart3, TimeSpan skipEnd3, TimeSpan skipStart4, TimeSpan skipEnd4, TimeSpan skipStart5, TimeSpan skipEnd5, TimeSpan skipStart6, TimeSpan skipEnd6, bool waitUntilFlat, int entryCooldown, bool enableMoneyMgmt, double maxDailyProfit, double maxDailyLoss, bool hitBarHighlight, Brush hitBarHighlightColor, int hitBarHighlightOpacity, Brush btnLongActiveColor, Brush btnLongInactiveColor, Brush btnShortActiveColor, Brush btnShortInactiveColor, Brush dragBarColor, Brush titleTextColor, double winLeft, double winTop, bool longEnabledPersist, bool shortEnabledPersist, bool alertPopupEnabled, bool alertSoundEnabled, bool alertMarkerEnabled, Brush markerColorBullish, Brush markerColorBearish, string setsJson)
		{
			return indicator.AlgoEngine(input, withinFilterCount, withinStart1, withinEnd1, withinExitOnEnd1, withinStart2, withinEnd2, withinExitOnEnd2, withinStart3, withinEnd3, withinExitOnEnd3, withinStart4, withinEnd4, withinExitOnEnd4, skipFilterCount, skipStart1, skipEnd1, skipStart2, skipEnd2, skipStart3, skipEnd3, skipStart4, skipEnd4, skipStart5, skipEnd5, skipStart6, skipEnd6, waitUntilFlat, entryCooldown, enableMoneyMgmt, maxDailyProfit, maxDailyLoss, hitBarHighlight, hitBarHighlightColor, hitBarHighlightOpacity, btnLongActiveColor, btnLongInactiveColor, btnShortActiveColor, btnShortInactiveColor, dragBarColor, titleTextColor, winLeft, winTop, longEnabledPersist, shortEnabledPersist, alertPopupEnabled, alertSoundEnabled, alertMarkerEnabled, markerColorBullish, markerColorBearish, setsJson);
		}
	}
}

#endregion
