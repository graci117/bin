#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;  // ADD THIS LINE
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("General", 1)]
    [CategoryOrder("Secondary Pivots", 2)]
    [CategoryOrder("Weekly IB", 3)]
    [CategoryOrder("Price Line", 4)]
    [CategoryOrder("Fibonacci Pivots", 5)]
    [CategoryOrder("Current Day Levels", 6)]
    [CategoryOrder("Prior Day Levels", 7)]
    [CategoryOrder("Open Times & Styles", 8)]
    [CategoryOrder("Golden Levels", 9)]
    [CategoryOrder("Golden Level Labels", 10)]
    [CategoryOrder("General Label Settings", 11)]
    [CategoryOrder("Watermark", 12)]
    public class GraciKeyLevels : Indicator
    {
        #region Private Variables
        private SessionIterator sessionIterator;
        private DateTime currentDate = Core.Globals.MinDate;
        private double sessionOpenPrice;
        private double nyOpenPrice;
        private double londonOpenPrice;
        private double asiaOpenPrice;
        private DateTime sessionEndTime;
        private bool isNYOpenFoundToday;
        private bool isLondonOpenFoundToday;
        private bool isAsiaOpenFoundToday;
        private double goldenLevelBase;
        private double priorDayHigh;
        private double priorDayLow;
        private double currentHigh;
        private double currentLow;
        private double weeklyIBHigh;
        private double weeklyIBLow;
        private bool isIBPeriodActive;
        private int currentHighBar;
        private int currentLowBar;
        private int weeklyIBHighBar;
        private int weeklyIBLowBar;
        private int sessionOpenBar;
        private int nyOpenBar;
        private int londonOpenBar;
        private int asiaOpenBar;
        
        private List<PivotInfo> bearishPivots;
        private List<PivotInfo> bullishPivots;
        private List<PivotInfo> lastDrawnBearishPivots = new List<PivotInfo>();
        private List<PivotInfo> lastDrawnBullishPivots = new List<PivotInfo>();
        private List<double> drawnGoldenLevels = new List<double>();
        
        private double lastPrice;
        private double pp, s1, r1, s2, r2, s3, r3;
        
        private SimpleFont mainLabelFont;
        private SimpleFont goldenLabelFont;
        private TextFormat dxPivotTextFormat;
        private Dictionary<string, TextLayout> dxPivotTextLayouts;
        private Dictionary<string, SharpDX.Direct2D1.Brush> dxPivotBrushes;
        private SharpDX.Direct2D1.Brush ibFillBrush;
        
        private TextFormat dxWatermarkTextFormat;
        private TextLayout dxWatermarkTextLayout;
        private SharpDX.Direct2D1.Brush dxWatermarkBrush;
        private string lastWatermarkText;
		
		// Weekly levels
		private double currentWeekHigh;
		private double currentWeekLow;
		private double priorWeekHigh;
		private double priorWeekLow;
		private int currentWeekHighBar;
		private int currentWeekLowBar;

		// Monthly levels
		private double currentMonthHigh;
		private double currentMonthLow;
		private double priorMonthHigh;
		private double priorMonthLow;
		private int currentMonthHighBar;
		private int currentMonthLowBar;

		// Date tracking for week/month
		private DateTime currentWeek = Core.Globals.MinDate;
		private DateTime currentMonth = Core.Globals.MinDate;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Comprehensive key levels indicator showing pivots, daily levels, session opens, and golden levels.";
                Name = "GraciKeyLevels";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                IsSuspendedWhileInactive = true;
                BarsRequiredToPlot = 2;

                // General Settings
                ShowPriceLine = true;
                ShowFibonacciPivots = true;
                ShowSessionOpen = true;
                ShowCurrentDayHighLow = true;
                ShowPriorDayHighLow = true;
                ShowWeeklyIB = true;
                ShowSecondaryPivots = true;
                ShowNYOpen = true;
                ShowLondonOpen = false;
                ShowAsiaOpen = false;
                ShowGoldenLevels = false;

                // Secondary Pivots
                SecondaryPivotPeriodType = BarsPeriodType.Minute;
                SecondaryPivotPeriodValue = 60;
                KeepBrokenPivots = false;
                MaxSecondaryPivots = 3;
                BearishPivotLineStyle = new Stroke(Brushes.DimGray, DashStyleHelper.Dash, 2f);
                BearishPivotLabelColor = Brushes.DimGray;
                BearishPivotLabelText = "60min Pivot";
                BullishPivotLineStyle = new Stroke(Brushes.DimGray, DashStyleHelper.Dash, 2f);
                BullishPivotLabelColor = Brushes.DimGray;
                BullishPivotLabelText = "60min Pivot";

                // Weekly IB Settings
                IBHighLineStyle = new Stroke(Brushes.DodgerBlue, DashStyleHelper.Dash, 2f);
                IBHighLabelColor = Brushes.DodgerBlue;
                IBLowLineStyle = new Stroke(Brushes.OrangeRed, DashStyleHelper.Dash, 2f);
                IBLowLabelColor = Brushes.OrangeRed;
                FillWeeklyIB = true;
                IBFillColor = Brushes.Tan;
                IBFillOpacity = 20;
                IBHighLabelHorizontalOffsetBars = 2;
                IBHighLabelVerticalOffset = 15;
                IBLowLabelHorizontalOffsetBars = 2;
                IBLowLabelVerticalOffset = -15;

                // Price Line & Fibonacci Pivots
                PriceLineStroke = new Stroke(Brushes.Yellow, DashStyleHelper.Solid, 1f);
                PpStroke = new Stroke(Brushes.Goldenrod, DashStyleHelper.Solid, 1f);
                RStroke = new Stroke(Brushes.DodgerBlue, DashStyleHelper.Solid, 1f);
                SStroke = new Stroke(Brushes.Crimson, DashStyleHelper.Solid, 1f);
                PivotLineWidth = 20;
                PivotLabelHorizontalOffset = 5;

                // Current Day Levels
                CurrentDayHighLineStyle = new Stroke(Brushes.SeaGreen, DashStyleHelper.Dash, 2f);
                CurrentDayHighLabelColor = Brushes.SeaGreen;
                CurrentDayLowLineStyle = new Stroke(Brushes.Red, DashStyleHelper.Dash, 2f);
                CurrentDayLowLabelColor = Brushes.Red;
                CurrentDayHighLabelHorizontalOffsetBars = -4;
                CurrentDayHighLabelVerticalOffset = -7;
                CurrentDayLowLabelHorizontalOffsetBars = -4;
                CurrentDayLowLabelVerticalOffset = 7;

                // Prior Day Levels
                PriorDayHighLineStyle = new Stroke(Brushes.DarkCyan, DashStyleHelper.Solid, 2f);
                PriorDayHighLabelColor = Brushes.DarkCyan;
                PriorDayLowLineStyle = new Stroke(Brushes.Crimson, DashStyleHelper.Solid, 2f);
                PriorDayLowLabelColor = Brushes.Crimson;
                PriorDayHighLabelHorizontalOffsetBars = -4;
                PriorDayHighLabelVerticalOffset = 7;
                PriorDayLowLabelHorizontalOffsetBars = -4;
                PriorDayLowLabelVerticalOffset = -7;

                // Open Times
                NYOpenTime = DateTime.Parse("09:30");
                NYOpenLineStyle = new Stroke(Brushes.SeaGreen, DashStyleHelper.Dash, 2f);
                NYOpenLabelColor = Brushes.SeaGreen;
                LondonOpenTime = DateTime.Parse("03:00");
                LondonOpenLineStyle = new Stroke(Brushes.DodgerBlue, DashStyleHelper.Dash, 2f);
                LondonOpenLabelColor = Brushes.DodgerBlue;
                AsiaOpenTime = DateTime.Parse("20:00");
                AsiaOpenLineStyle = new Stroke(Brushes.Orchid, DashStyleHelper.Dash, 2f);
                AsiaOpenLabelColor = Brushes.Orchid;
                SessionOpenLineStyle = new Stroke(Brushes.Cyan, DashStyleHelper.Dash, 2f);
                SessionOpenLabelColor = Brushes.Cyan;

                // Golden Levels
                Level00 = 0;
                Level00Line = new Stroke(Brushes.Red, DashStyleHelper.Dot, 1f);
                Level26 = 26;
                Level26Line = new Stroke(Brushes.Gold, DashStyleHelper.Dot, 1f);
                Level50 = 50;
                Level50Line = new Stroke(Brushes.Red, DashStyleHelper.Dot, 1f);
                Level77 = 77;
                Level77Line = new Stroke(Brushes.Gold, DashStyleHelper.Dot, 1f);
                GoldenLabelHorizontalOffset = -4;
                GoldenLabelVerticalOffset = -7;
                GoldenLabelFontSize = 10;
                GoldenLabelFontBold = false;
                GoldenLevelLabelColor = Brushes.Gold;

                // General Label Settings
                LabelHorizontalOffset = -4;
                LabelVerticalOffset = 7;
                LabelFontSize = 10;
                LabelFontBold = true;

                // Watermark Settings
                ShowWatermark = true;
                WatermarkCustomText = string.Empty;
                WatermarkInstrumentDisplay = "Show";
                WatermarkPeriodDisplay = "Show";
                WatermarkColor = Brushes.Gray;
                WatermarkFontSize = 34;
                WatermarkOpacity = 15;
                WatermarkHAlign = "Left";
                WatermarkVAlign = "Center";
                WatermarkOffsetX = 0;
                WatermarkOffsetY = 0;
				
				// Additional toggles
				ShowCurrentWeekHighLow = false;
				ShowPriorWeekHighLow = false;
				ShowCurrentMonthHighLow = false;
				ShowPriorMonthHighLow = false;
				

				// Current Week
				CurrentWeekHighLineStyle = new Stroke(Brushes.LimeGreen, DashStyleHelper.Solid, 2f);
				CurrentWeekHighLabelColor = Brushes.LimeGreen;
				CurrentWeekLowLineStyle = new Stroke(Brushes.OrangeRed, DashStyleHelper.Solid, 2f);
				CurrentWeekLowLabelColor = Brushes.OrangeRed;

				// Prior Week
				PriorWeekHighLineStyle = new Stroke(Brushes.DarkGreen, DashStyleHelper.Dash, 2f);
				PriorWeekHighLabelColor = Brushes.DarkGreen;
				PriorWeekLowLineStyle = new Stroke(Brushes.DarkRed, DashStyleHelper.Dash, 2f);
				PriorWeekLowLabelColor = Brushes.DarkRed;

				// Current Month
				CurrentMonthHighLineStyle = new Stroke(Brushes.Cyan, DashStyleHelper.Solid, 2f);
				CurrentMonthHighLabelColor = Brushes.Cyan;
				CurrentMonthLowLineStyle = new Stroke(Brushes.Magenta, DashStyleHelper.Solid, 2f);
				CurrentMonthLowLabelColor = Brushes.Magenta;

				// Prior Month
				PriorMonthHighLineStyle = new Stroke(Brushes.DarkCyan, DashStyleHelper.Dash, 2f);
				PriorMonthHighLabelColor = Brushes.DarkCyan;
				PriorMonthLowLineStyle = new Stroke(Brushes.DarkMagenta, DashStyleHelper.Dash, 2f);
				PriorMonthLowLabelColor = Brushes.DarkMagenta;

			
            }
            else if (State == State.Configure)
            {
                AddDataSeries(BarsPeriodType.Minute, 1);
                AddDataSeries(SecondaryPivotPeriodType, SecondaryPivotPeriodValue);
                AddDataSeries(BarsPeriodType.Day, 1);  // ADD THIS LINE for daily volume profile
                bearishPivots = new List<PivotInfo>();
                bullishPivots = new List<PivotInfo>();
                currentHighBar = -1;
                currentLowBar = -1;
                weeklyIBHighBar = -1;
                weeklyIBLowBar = -1;
                sessionOpenBar = -1;
                nyOpenBar = -1;
                londonOpenBar = -1;
                asiaOpenBar = -1;
            }
            else if (State == State.DataLoaded)
            {
                sessionIterator = new SessionIterator(Bars);
                mainLabelFont = new SimpleFont() { Size = LabelFontSize, Bold = LabelFontBold };
                goldenLabelFont = new SimpleFont() { Size = GoldenLabelFontSize, Bold = GoldenLabelFontBold };
                if (ShowFibonacciPivots)
                {
                    dxPivotTextFormat = new TextFormat(Core.Globals.DirectWriteFactory, "Arial", 
                        LabelFontBold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal, 
                        SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, LabelFontSize);
                    
                    dxPivotTextLayouts = new Dictionary<string, TextLayout>();
                    string[] labels = new string[] { "PP", "R1", "R2", "R3", "S1", "S2", "S3" };
                    foreach (string label in labels)
                    {
                        dxPivotTextLayouts[label] = new TextLayout(Core.Globals.DirectWriteFactory, 
                            label, dxPivotTextFormat, float.MaxValue, float.MaxValue);
                    }
                }
            }
            else if (State == State.Terminated)
            {
                if (dxPivotTextFormat != null)
                    dxPivotTextFormat.Dispose();
                
                if (dxPivotTextLayouts != null)
                {
                    foreach (var layout in dxPivotTextLayouts.Values)
                        layout.Dispose();
                    dxPivotTextLayouts.Clear();
                }
                
                if (dxPivotBrushes != null)
                {
                    foreach (var brush in dxPivotBrushes.Values)
                        brush.Dispose();
                    dxPivotBrushes.Clear();
                }
                
                if (dxWatermarkTextFormat != null)
                    dxWatermarkTextFormat.Dispose();
                
                if (dxWatermarkTextLayout != null)
                    dxWatermarkTextLayout.Dispose();
                
                if (dxWatermarkBrush != null)
                    dxWatermarkBrush.Dispose();
                    
                if (ibFillBrush != null)
                    ibFillBrush.Dispose();
            }
        }
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToPlot)
                return;

            if (BarsInProgress == 0)
                ProcessPrimaryBar();
            else if (BarsInProgress == 1)
                ProcessSecondaryBar();
            else if (BarsInProgress == 2)
                CalculateSecondaryPivots();
        }
        #endregion

        #region Processing Methods
        private void ProcessPrimaryBar()
        {
            if (ShowSecondaryPivots)
            {
                CheckForBrokenPivots();
                DrawOrUpdateSecondaryPivots();
            }

            if (sessionIterator.GetTradingDay(Time[0]) != currentDate)
            {
                currentDate = sessionIterator.GetTradingDay(Time[0]);
                sessionIterator.GetNextSession(Time[0], true);
                sessionEndTime = sessionIterator.ActualSessionEnd;
                CalculateDailyLevels();
                DrawInitialDailyLines();
            }
            else
            {
                UpdateDynamicLevels();
            }

            UpdateTextLabels();
        }

        private void ProcessSecondaryBar()
        {
            if (CurrentBars[1] < 2)
                return;

            TimeSpan barTime = Times[1][0].TimeOfDay;
            int sessionEnd = GetSessionEndBarsAgo();

            if (ShowNYOpen && !isNYOpenFoundToday && barTime == NYOpenTime.TimeOfDay)
            {
                nyOpenPrice = Opens[1][0];
                nyOpenBar = CurrentBar;
                isNYOpenFoundToday = true;
                Draw.Line(this, "NYOpenLine", true, 0, nyOpenPrice, sessionEnd, nyOpenPrice, 
                    NYOpenLineStyle.Brush, NYOpenLineStyle.DashStyleHelper, (int)NYOpenLineStyle.Width);
            }

            if (ShowLondonOpen && !isLondonOpenFoundToday && barTime == LondonOpenTime.TimeOfDay)
            {
                londonOpenPrice = Opens[1][0];
                londonOpenBar = CurrentBar;
                isLondonOpenFoundToday = true;
                Draw.Line(this, "LondonOpenLine", true, 0, londonOpenPrice, sessionEnd, londonOpenPrice, 
                    LondonOpenLineStyle.Brush, LondonOpenLineStyle.DashStyleHelper, (int)LondonOpenLineStyle.Width);
            }

            if (ShowAsiaOpen && !isAsiaOpenFoundToday && barTime == AsiaOpenTime.TimeOfDay)
            {
                asiaOpenPrice = Opens[1][0];
                asiaOpenBar = CurrentBar;
                isAsiaOpenFoundToday = true;
                Draw.Line(this, "AsiaOpenLine", true, 0, asiaOpenPrice, sessionEnd, asiaOpenPrice, 
                    AsiaOpenLineStyle.Brush, AsiaOpenLineStyle.DashStyleHelper, (int)AsiaOpenLineStyle.Width);
            }
        }

        private void CalculateDailyLevels()
        {
						// Check for week change
			DateTime tradingDayWeek = GetWeekStart(currentDate);
			if (currentWeek != tradingDayWeek)
			{
				if (currentWeekHigh > 0)
					priorWeekHigh = currentWeekHigh;
				if (currentWeekLow > 0)
					priorWeekLow = currentWeekLow;
				
				currentWeekHigh = High[0];
				currentWeekLow = Low[0];
				currentWeekHighBar = CurrentBar;
				currentWeekLowBar = CurrentBar;
				currentWeek = tradingDayWeek;
			}

			// Check for month change
			DateTime tradingDayMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
			if (currentMonth != tradingDayMonth)
			{
				if (currentMonthHigh > 0)
					priorMonthHigh = currentMonthHigh;
				if (currentMonthLow > 0)
					priorMonthLow = currentMonthLow;
				
				currentMonthHigh = High[0];
				currentMonthLow = Low[0];
				currentMonthHighBar = CurrentBar;
				currentMonthLowBar = CurrentBar;
				currentMonth = tradingDayMonth;
			}

			
            RemoveDrawObject("NYOpenLine");
            RemoveDrawObject("NYOpenLabel");
            RemoveDrawObject("LondonOpenLine");
            RemoveDrawObject("LondonOpenLabel");
            RemoveDrawObject("AsiaOpenLine");
            RemoveDrawObject("AsiaOpenLabel");

            if (currentHigh > 0)
                priorDayHigh = currentHigh;
            if (currentLow > 0)
                priorDayLow = currentLow;

            double priorClose = Close[1];
            if (priorDayHigh > 0 && priorDayLow > 0)
            {
                pp = (priorDayHigh + priorDayLow + priorClose) / 3.0;
                double range = priorDayHigh - priorDayLow;
                s1 = pp - range * 0.382;
                r1 = pp + range * 0.382;
                s2 = pp - range * 0.618;
                r2 = pp + range * 0.618;
                s3 = pp - range * 1.0;
                r3 = pp + range * 1.0;
            }

            currentHigh = High[0];
            currentLow = Low[0];
            currentHighBar = CurrentBar;
            currentLowBar = CurrentBar;
            sessionOpenPrice = Open[0];
            sessionOpenBar = CurrentBar;

            if (currentDate.DayOfWeek == DayOfWeek.Monday)
            {
                weeklyIBHigh = High[0];
                weeklyIBLow = Low[0];
                weeklyIBHighBar = CurrentBar;
                weeklyIBLowBar = CurrentBar;
                isIBPeriodActive = true;
            }
            else if (currentDate.DayOfWeek == DayOfWeek.Wednesday)
            {
                isIBPeriodActive = false;
            }

            nyOpenPrice = 0;
            isNYOpenFoundToday = false;
            nyOpenBar = -1;
            londonOpenPrice = 0;
            isLondonOpenFoundToday = false;
            londonOpenBar = -1;
            asiaOpenPrice = 0;
            isAsiaOpenFoundToday = false;
            asiaOpenBar = -1;
            goldenLevelBase = 0;
        }
		
		private DateTime GetWeekStart(DateTime date)
		{
			int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
			return date.AddDays(-1 * diff).Date;
		}

        private void DrawInitialDailyLines()
        {
            int sessionEnd = GetSessionEndBarsAgo();

            if (ShowPriorDayHighLow && priorDayHigh > 0)
            {
                Draw.HorizontalLine(this, "PriorDayHighLine", false, priorDayHigh, 
                    PriorDayHighLineStyle.Brush, PriorDayHighLineStyle.DashStyleHelper, (int)PriorDayHighLineStyle.Width);
                Draw.HorizontalLine(this, "PriorDayLowLine", false, priorDayLow, 
                    PriorDayLowLineStyle.Brush, PriorDayLowLineStyle.DashStyleHelper, (int)PriorDayLowLineStyle.Width);
            }
			
			
			// Prior Week
			if (ShowPriorWeekHighLow && priorWeekHigh > 0)
			{
				Draw.HorizontalLine(this, "PriorWeekHighLine", false, priorWeekHigh, 
					PriorWeekHighLineStyle.Brush, PriorWeekHighLineStyle.DashStyleHelper, (int)PriorWeekHighLineStyle.Width);
				Draw.HorizontalLine(this, "PriorWeekLowLine", false, priorWeekLow, 
					PriorWeekLowLineStyle.Brush, PriorWeekLowLineStyle.DashStyleHelper, (int)PriorWeekLowLineStyle.Width);
			}

			// Current Week
			if (ShowCurrentWeekHighLow)
			{
				Draw.Line(this, "CurrentWeekHighLine", true, 0, currentWeekHigh, sessionEnd, currentWeekHigh, 
					CurrentWeekHighLineStyle.Brush, CurrentWeekHighLineStyle.DashStyleHelper, (int)CurrentWeekHighLineStyle.Width);
				Draw.Line(this, "CurrentWeekLowLine", true, 0, currentWeekLow, sessionEnd, currentWeekLow, 
					CurrentWeekLowLineStyle.Brush, CurrentWeekLowLineStyle.DashStyleHelper, (int)CurrentWeekLowLineStyle.Width);
			}

			// Prior Month
			if (ShowPriorMonthHighLow && priorMonthHigh > 0)
			{
				Draw.HorizontalLine(this, "PriorMonthHighLine", false, priorMonthHigh, 
					PriorMonthHighLineStyle.Brush, PriorMonthHighLineStyle.DashStyleHelper, (int)PriorMonthHighLineStyle.Width);
				Draw.HorizontalLine(this, "PriorMonthLowLine", false, priorMonthLow, 
					PriorMonthLowLineStyle.Brush, PriorMonthLowLineStyle.DashStyleHelper, (int)PriorMonthLowLineStyle.Width);
			}

			// Current Month
			if (ShowCurrentMonthHighLow)
			{
				Draw.Line(this, "CurrentMonthHighLine", true, 0, currentMonthHigh, sessionEnd, currentMonthHigh, 
					CurrentMonthHighLineStyle.Brush, CurrentMonthHighLineStyle.DashStyleHelper, (int)CurrentMonthHighLineStyle.Width);
				Draw.Line(this, "CurrentMonthLowLine", true, 0, currentMonthLow, sessionEnd, currentMonthLow, 
					CurrentMonthLowLineStyle.Brush, CurrentMonthLowLineStyle.DashStyleHelper, (int)CurrentMonthLowLineStyle.Width);
			}

			
            if (ShowCurrentDayHighLow)
            {
                Draw.Line(this, "CurrentDayHighLine", true, 0, currentHigh, sessionEnd, currentHigh, 
                    CurrentDayHighLineStyle.Brush, CurrentDayHighLineStyle.DashStyleHelper, (int)CurrentDayHighLineStyle.Width);
                Draw.Line(this, "CurrentDayLowLine", true, 0, currentLow, sessionEnd, currentLow, 
                    CurrentDayLowLineStyle.Brush, CurrentDayLowLineStyle.DashStyleHelper, (int)CurrentDayLowLineStyle.Width);
            }

            if (ShowSessionOpen)
            {
                Draw.Line(this, "SessionOpenLine", true, 0, sessionOpenPrice, sessionEnd, sessionOpenPrice, 
                    SessionOpenLineStyle.Brush, SessionOpenLineStyle.DashStyleHelper, (int)SessionOpenLineStyle.Width);
            }

            if (ShowWeeklyIB && isIBPeriodActive)
            {
                Draw.Line(this, "WeeklyIBHighLine", true, 0, weeklyIBHigh, 0, weeklyIBHigh, 
                    IBHighLineStyle.Brush, IBHighLineStyle.DashStyleHelper, (int)IBHighLineStyle.Width);
                Draw.Line(this, "WeeklyIBLowLine", true, 0, weeklyIBLow, 0, weeklyIBLow, 
                    IBLowLineStyle.Brush, IBLowLineStyle.DashStyleHelper, (int)IBLowLineStyle.Width);
                UpdateIBFill();
            }
        }

        private void UpdateDynamicLevels()
        {
            int sessionEnd = GetSessionEndBarsAgo();

            if (High[0] > currentHigh)
            {
                currentHigh = High[0];
                currentHighBar = CurrentBar;
                if (ShowCurrentDayHighLow)
                {
                    Draw.Line(this, "CurrentDayHighLine", true, 0, currentHigh, sessionEnd, currentHigh, 
                        CurrentDayHighLineStyle.Brush, CurrentDayHighLineStyle.DashStyleHelper, (int)CurrentDayHighLineStyle.Width);
                }
            }

            if (Low[0] < currentLow)
            {
                currentLow = Low[0];
                currentLowBar = CurrentBar;
                if (ShowCurrentDayHighLow)
                {
                    Draw.Line(this, "CurrentDayLowLine", true, 0, currentLow, sessionEnd, currentLow, 
                        CurrentDayLowLineStyle.Brush, CurrentDayLowLineStyle.DashStyleHelper, (int)CurrentDayLowLineStyle.Width);
                }
            }
			
			// Update current week levels
			if (High[0] > currentWeekHigh)
			{
				currentWeekHigh = High[0];
				currentWeekHighBar = CurrentBar;
			}
			if (Low[0] < currentWeekLow)
			{
				currentWeekLow = Low[0];
				currentWeekLowBar = CurrentBar;
			}

			// Update current month levels
			if (High[0] > currentMonthHigh)
			{
				currentMonthHigh = High[0];
				currentMonthHighBar = CurrentBar;
			}
			if (Low[0] < currentMonthLow)
			{
				currentMonthLow = Low[0];
				currentMonthLowBar = CurrentBar;
			}


            if (isIBPeriodActive)
            {
                bool ibUpdated = false;
                if (High[0] > weeklyIBHigh)
                {
                    weeklyIBHigh = High[0];
                    weeklyIBHighBar = CurrentBar;
                    if (ShowWeeklyIB)
                    {
                        Draw.Line(this, "WeeklyIBHighLine", true, 0, weeklyIBHigh, 0, weeklyIBHigh, 
                            IBHighLineStyle.Brush, IBHighLineStyle.DashStyleHelper, (int)IBHighLineStyle.Width);
                    }
                    ibUpdated = true;
                }

                if (Low[0] < weeklyIBLow)
                {
                    weeklyIBLow = Low[0];
                    weeklyIBLowBar = CurrentBar;
                    if (ShowWeeklyIB)
                    {
                        Draw.Line(this, "WeeklyIBLowLine", true, 0, weeklyIBLow, 0, weeklyIBLow, 
                            IBLowLineStyle.Brush, IBLowLineStyle.DashStyleHelper, (int)IBLowLineStyle.Width);
                    }
                    ibUpdated = true;
                }

                if (ibUpdated)
                    UpdateIBFill();
            }

            if (!IsFirstTickOfBar)
                return;

            UpdateIBFill();

            if (ShowCurrentDayHighLow)
            {
                Draw.Line(this, "CurrentDayHighLine", false, CurrentBar - currentHighBar, currentHigh, sessionEnd, currentHigh, 
                    CurrentDayHighLineStyle.Brush, CurrentDayHighLineStyle.DashStyleHelper, (int)CurrentDayHighLineStyle.Width);
                Draw.Line(this, "CurrentDayLowLine", false, CurrentBar - currentLowBar, currentLow, sessionEnd, currentLow, 
                    CurrentDayLowLineStyle.Brush, CurrentDayLowLineStyle.DashStyleHelper, (int)CurrentDayLowLineStyle.Width);
            }

            if (ShowWeeklyIB && weeklyIBHigh > 0)
            {
                Draw.Line(this, "WeeklyIBHighLine", false, CurrentBar - weeklyIBHighBar, weeklyIBHigh, 0, weeklyIBHigh, 
                    IBHighLineStyle.Brush, IBHighLineStyle.DashStyleHelper, (int)IBHighLineStyle.Width);
                Draw.Line(this, "WeeklyIBLowLine", false, CurrentBar - weeklyIBLowBar, weeklyIBLow, 0, weeklyIBLow, 
                    IBLowLineStyle.Brush, IBLowLineStyle.DashStyleHelper, (int)IBLowLineStyle.Width);
            }

            if (ShowSessionOpen && sessionOpenPrice > 0)
            {
                Draw.Line(this, "SessionOpenLine", false, CurrentBar - sessionOpenBar, sessionOpenPrice, sessionEnd, sessionOpenPrice, 
                    SessionOpenLineStyle.Brush, SessionOpenLineStyle.DashStyleHelper, (int)SessionOpenLineStyle.Width);
            }

            if (ShowNYOpen && nyOpenPrice > 0)
            {
                Draw.Line(this, "NYOpenLine", false, CurrentBar - nyOpenBar, nyOpenPrice, sessionEnd, nyOpenPrice, 
                    NYOpenLineStyle.Brush, NYOpenLineStyle.DashStyleHelper, (int)NYOpenLineStyle.Width);
            }

            if (ShowLondonOpen && londonOpenPrice > 0)
            {
                Draw.Line(this, "LondonOpenLine", false, CurrentBar - londonOpenBar, londonOpenPrice, sessionEnd, londonOpenPrice, 
                    LondonOpenLineStyle.Brush, LondonOpenLineStyle.DashStyleHelper, (int)LondonOpenLineStyle.Width);
            }

            if (ShowAsiaOpen && asiaOpenPrice > 0)
            {
                Draw.Line(this, "AsiaOpenLine", false, CurrentBar - asiaOpenBar, asiaOpenPrice, sessionEnd, asiaOpenPrice, 
                    AsiaOpenLineStyle.Brush, AsiaOpenLineStyle.DashStyleHelper, (int)AsiaOpenLineStyle.Width);
            }
        }

        private int GetSessionEndBarsAgo()
        {
            if (sessionEndTime == DateTime.MinValue)
                return 0;

            int bar = Bars.GetBar(sessionEndTime);
            return (bar > -1 && Time[0] >= sessionEndTime) ? CurrentBar - bar : 0;
        }

        private void UpdateIBFill()
        {
            if (IsVisible && ShowWeeklyIB && FillWeeklyIB && weeklyIBHigh > 0 && weeklyIBLow > 0)
            {
                Draw.Rectangle(this, "WeeklyIBFill", true, CurrentBar - Math.Min(weeklyIBHighBar, weeklyIBLowBar), 
                    weeklyIBHigh, 0, weeklyIBLow, Brushes.Transparent, IBFillColor, IBFillOpacity);
            }
            else
            {
                RemoveDrawObject("WeeklyIBFill");
            }
        }

        private void UpdateTextLabels()
        {
            if (ShowWeeklyIB && weeklyIBHigh > 0)
            {
                					
				Draw.Text(this, "WeeklyIBHighLabel", false, "IB High " + Instrument.MasterInstrument.FormatPrice(weeklyIBHigh), 
					IBHighLabelHorizontalOffsetBars, weeklyIBHigh, IBHighLabelVerticalOffset, IBHighLabelColor, 
					mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);

            }

            if (ShowWeeklyIB && weeklyIBLow > 0)
            {
                Draw.Text(this, "WeeklyIBLowLabel", false, "IB Low " + Instrument.MasterInstrument.FormatPrice(weeklyIBLow), 
                    IBLowLabelHorizontalOffsetBars, weeklyIBLow, IBLowLabelVerticalOffset, IBLowLabelColor, 
                    mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowCurrentDayHighLow && currentHigh > 0)
            {
                Draw.Text(this, "CurrentDayHighLabel", false, "Session High " + Instrument.MasterInstrument.FormatPrice(currentHigh), 
                    CurrentDayHighLabelHorizontalOffsetBars, currentHigh, CurrentDayHighLabelVerticalOffset, 
                    CurrentDayHighLabelColor, mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowCurrentDayHighLow && currentLow > 0)
            {
                Draw.Text(this, "CurrentDayLowLabel", false, "Session Low " + Instrument.MasterInstrument.FormatPrice(currentLow), 
                    CurrentDayLowLabelHorizontalOffsetBars, currentLow, CurrentDayLowLabelVerticalOffset, 
                    CurrentDayLowLabelColor, mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowPriorDayHighLow && priorDayHigh > 0)
            {
                Draw.Text(this, "PriorDayHighLabel", false, "PD High " + Instrument.MasterInstrument.FormatPrice(priorDayHigh), 
                    PriorDayHighLabelHorizontalOffsetBars, priorDayHigh, PriorDayHighLabelVerticalOffset, 
                    PriorDayHighLabelColor, mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowPriorDayHighLow && priorDayLow > 0)
            {
                Draw.Text(this, "PriorDayLowLabel", false, "PD Low " + Instrument.MasterInstrument.FormatPrice(priorDayLow), 
                    PriorDayLowLabelHorizontalOffsetBars, priorDayLow, PriorDayLowLabelVerticalOffset, 
                    PriorDayLowLabelColor, mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowSessionOpen && sessionOpenPrice > 0)
            {
                Draw.Text(this, "SessionOpenLabel", false, "Daily Open " + Instrument.MasterInstrument.FormatPrice(sessionOpenPrice), 
                    LabelHorizontalOffset, sessionOpenPrice, LabelVerticalOffset, SessionOpenLabelColor, 
                    mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowNYOpen && nyOpenPrice > 0)
            {
                Draw.Text(this, "NYOpenLabel", false, "NY Open " + Instrument.MasterInstrument.FormatPrice(nyOpenPrice), 
                    LabelHorizontalOffset, nyOpenPrice, LabelVerticalOffset, NYOpenLabelColor, 
                    mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowLondonOpen && londonOpenPrice > 0)
            {
                Draw.Text(this, "LondonOpenLabel", false, "LN Open " + Instrument.MasterInstrument.FormatPrice(londonOpenPrice), 
                    LabelHorizontalOffset, londonOpenPrice, LabelVerticalOffset, LondonOpenLabelColor, 
                    mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            if (ShowAsiaOpen && asiaOpenPrice > 0)
            {
                Draw.Text(this, "AsiaOpenLabel", false, "AS Open " + Instrument.MasterInstrument.FormatPrice(asiaOpenPrice), 
                    LabelHorizontalOffset, asiaOpenPrice, LabelVerticalOffset, AsiaOpenLabelColor, 
                    mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            // Golden Levels
            if (ShowGoldenLevels && IsFirstTickOfBar)
            {
                double baseLevel = Math.Floor(Close[0] / 100.0) * 100.0;
                if (goldenLevelBase != baseLevel)
                {
                    foreach (double level in drawnGoldenLevels)
                    {
                        RemoveDrawObject("GoldenLevel_" + level);
                        RemoveDrawObject("GoldenLabel_" + level);
                    }
                    drawnGoldenLevels.Clear();
                    goldenLevelBase = baseLevel;

                    DrawGoldenLevelsForBase(goldenLevelBase - 100);
                    DrawGoldenLevelsForBase(goldenLevelBase);
                    DrawGoldenLevelsForBase(goldenLevelBase + 100);
                }
            }
			
			// Prior Day Close

        }

        private void DrawGoldenLevelsForBase(double basePrice)
        {
            int[] offsets = new int[] { Level00, Level26, Level50, Level77 };
            Stroke[] strokes = new Stroke[] { Level00Line, Level26Line, Level50Line, Level77Line };

            for (int i = 0; i < offsets.Length; i++)
            {
                double price = basePrice + offsets[i];
                string tag = "GoldenLevel_" + price;
                string labelTag = "GoldenLabel_" + price;

                Draw.HorizontalLine(this, tag, false, price, strokes[i].Brush, 
                    strokes[i].DashStyleHelper, (int)strokes[i].Width);
                Draw.Text(this, labelTag, false, Instrument.MasterInstrument.FormatPrice(price), 
                    GoldenLabelHorizontalOffset, price, GoldenLabelVerticalOffset, GoldenLevelLabelColor, 
                    goldenLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);

                drawnGoldenLevels.Add(price);
            }
        }
        #endregion

        #region Secondary Pivots
        private void CalculateSecondaryPivots()
        {
            if (!ShowSecondaryPivots || CurrentBars[2] < 3)
                return;

            // Bearish Pivot (High)
            double pivotHigh = Highs[2][1];
            bool isBearishPivot = true;
            for (int i = 0; i <= 2; i++)
            {
                if (i != 1 && Highs[2][i] >= pivotHigh)
                {
                    isBearishPivot = false;
                    break;
                }
            }

            if (isBearishPivot)
            {
                DateTime pivotTime = Times[2][1];
                string tag = "bearishPivot" + pivotTime.Ticks;
                if (!bearishPivots.Any(p => p.Tag == tag))
                {
                    int bar = BarsArray[0].GetBar(pivotTime);
                    if (bar > -1)
                    {
                        bearishPivots.Add(new PivotInfo()
                        {
                            Tag = tag,
                            Price = pivotHigh,
                            StartBarIndex = bar
                        });
                    }
                }
            }

            // Bullish Pivot (Low)
            double pivotLow = Lows[2][1];
            bool isBullishPivot = true;
            for (int i = 0; i <= 2; i++)
            {
                if (i != 1 && Lows[2][i] <= pivotLow)
                {
                    isBullishPivot = false;
                    break;
                }
            }

            if (isBullishPivot)
            {
                DateTime pivotTime = Times[2][1];
                string tag = "bullishPivot" + pivotTime.Ticks;
                if (!bullishPivots.Any(p => p.Tag == tag))
                {
                    int bar = BarsArray[0].GetBar(pivotTime);
                    if (bar > -1)
                    {
                        bullishPivots.Add(new PivotInfo()
                        {
                            Tag = tag,
                            Price = pivotLow,
                            StartBarIndex = bar
                        });
                    }
                }
            }
        }

        private void CheckForBrokenPivots()
        {
            if (CurrentBar < 1)
                return;

            for (int i = bearishPivots.Count - 1; i >= 0; i--)
            {
                if (High[0] >= bearishPivots[i].Price)
                {
                    if (KeepBrokenPivots)
                    {
                        int startBarsAgo = CurrentBar - bearishPivots[i].StartBarIndex;
                        Draw.Line(this, "brokenBearish" + bearishPivots[i].Tag, false, startBarsAgo, 
                            bearishPivots[i].Price, 0, bearishPivots[i].Price, 
                            BearishPivotLineStyle.Brush, DashStyleHelper.Dot, 1);
                    }
                    bearishPivots.RemoveAt(i);
                }
            }

            for (int i = bullishPivots.Count - 1; i >= 0; i--)
            {
                if (Low[0] <= bullishPivots[i].Price)
                {
                    if (KeepBrokenPivots)
                    {
                        int startBarsAgo = CurrentBar - bullishPivots[i].StartBarIndex;
                        Draw.Line(this, "brokenBullish" + bullishPivots[i].Tag, false, startBarsAgo, 
                            bullishPivots[i].Price, 0, bullishPivots[i].Price, 
                            BullishPivotLineStyle.Brush, DashStyleHelper.Dot, 1);
                    }
                    bullishPivots.RemoveAt(i);
                }
            }
        }

        private void DrawOrUpdateSecondaryPivots()
        {
            if (!IsVisible || CurrentBar < 1)
                return;

            double currentPrice = Close[0];
            List<PivotInfo> drawableBearish = bearishPivots
                .OrderBy(p => Math.Abs(p.Price - currentPrice))
                .Take(MaxSecondaryPivots > 0 ? MaxSecondaryPivots : int.MaxValue)
                .ToList();

            List<PivotInfo> drawableBullish = bullishPivots
                .OrderBy(p => Math.Abs(p.Price - currentPrice))
                .Take(MaxSecondaryPivots > 0 ? MaxSecondaryPivots : int.MaxValue)
                .ToList();

            // Remove old pivots
            foreach (var oldPivot in lastDrawnBearishPivots.Where(old => !drawableBearish.Any(newP => newP.Tag == old.Tag)))
            {
                RemoveDrawObject(oldPivot.Tag);
                RemoveDrawObject("BearPivotLabel" + oldPivot.Tag);
            }

            foreach (var oldPivot in lastDrawnBullishPivots.Where(old => !drawableBullish.Any(newP => newP.Tag == old.Tag)))
            {
                RemoveDrawObject(oldPivot.Tag);
                RemoveDrawObject("BullPivotLabel" + oldPivot.Tag);
            }

            // Draw bearish pivots
            foreach (var pivot in drawableBearish)
            {
                int startBarsAgo = CurrentBar - pivot.StartBarIndex;
                Draw.Line(this, pivot.Tag, false, startBarsAgo, pivot.Price, 0, pivot.Price, 
                    BearishPivotLineStyle.Brush, BearishPivotLineStyle.DashStyleHelper, (int)BearishPivotLineStyle.Width);
                Draw.Text(this, "BearPivotLabel" + pivot.Tag, false, 
                    BearishPivotLabelText + " " + Instrument.MasterInstrument.FormatPrice(pivot.Price), 
                    LabelHorizontalOffset, pivot.Price, LabelVerticalOffset, BearishPivotLabelColor, 
                    mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            // Draw bullish pivots
            foreach (var pivot in drawableBullish)
            {
                int startBarsAgo = CurrentBar - pivot.StartBarIndex;
                Draw.Line(this, pivot.Tag, false, startBarsAgo, pivot.Price, 0, pivot.Price, 
                    BullishPivotLineStyle.Brush, BullishPivotLineStyle.DashStyleHelper, (int)BullishPivotLineStyle.Width);
                Draw.Text(this, "BullPivotLabel" + pivot.Tag, false, 
                    BullishPivotLabelText + " " + Instrument.MasterInstrument.FormatPrice(pivot.Price), 
                    LabelHorizontalOffset, pivot.Price, -LabelVerticalOffset, BullishPivotLabelColor, 
                    mainLabelFont, System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }

            lastDrawnBearishPivots = drawableBearish;
            lastDrawnBullishPivots = drawableBullish;
        }
        #endregion

        #region OnMarketData
        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            if (marketDataUpdate.MarketDataType == MarketDataType.Last)
                lastPrice = marketDataUpdate.Price;
        }
        #endregion

        #region OnRender
        public override void OnRenderTargetChanged()
        {
            base.OnRenderTargetChanged();

            if (PriceLineStroke != null)
                PriceLineStroke.RenderTarget = RenderTarget;
            if (PpStroke != null)
                PpStroke.RenderTarget = RenderTarget;
            if (RStroke != null)
                RStroke.RenderTarget = RenderTarget;
            if (SStroke != null)
                SStroke.RenderTarget = RenderTarget;

            if (dxPivotBrushes != null)
            {
                foreach (var brush in dxPivotBrushes.Values)
                    brush.Dispose();
                dxPivotBrushes.Clear();
            }

            if (RenderTarget != null && ShowFibonacciPivots)
            {
                dxPivotBrushes = new Dictionary<string, SharpDX.Direct2D1.Brush>
                {
                    { "PP", PpStroke.Brush.ToDxBrush(RenderTarget) },
                    { "R", RStroke.Brush.ToDxBrush(RenderTarget) },
                    { "S", SStroke.Brush.ToDxBrush(RenderTarget) }
                };
            }

            if (dxWatermarkBrush != null)
            {
                dxWatermarkBrush.Dispose();
                dxWatermarkBrush = null;
            }

            if (RenderTarget != null && ShowWatermark && WatermarkColor is System.Windows.Media.SolidColorBrush)
            {
                System.Windows.Media.SolidColorBrush wmColor = (System.Windows.Media.SolidColorBrush)WatermarkColor;
                byte alpha = (byte)(255 * (WatermarkOpacity / 100.0));
                dxWatermarkBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, 
                    new SharpDX.Color(wmColor.Color.R, wmColor.Color.G, wmColor.Color.B, alpha));
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            if (ChartBars == null || ChartBars.Count == 0)
                return;

            // Draw Price Line
            if (ShowPriceLine && lastPrice > 0)
            {
                ChartPanel panel = chartControl.ChartPanels[chartScale.PanelIndex];
                float xStart = (float)panel.X;
                float xEnd = (float)(panel.X + panel.W);
                float y = (float)chartScale.GetYByValue(lastPrice);

                RenderTarget.DrawLine(new SharpDX.Vector2(xStart, y), new SharpDX.Vector2(xEnd, y), 
                    PriceLineStroke.BrushDX, PriceLineStroke.Width, PriceLineStroke.StrokeStyle);
            }

            // Draw Fibonacci Pivots
            if (ShowFibonacciPivots && pp > 0 && dxPivotTextLayouts != null && dxPivotBrushes != null)
            {
                var pivots = new List<(string, double)>
                {
                    ("PP", pp), ("R1", r1), ("R2", r2), ("R3", r3), ("S1", s1), ("S2", s2), ("S3", s3)
                };

                foreach (var pivot in pivots)
                {
                    if (pivot.Item2 <= 0)
                        continue;

                    string brushKey = pivot.Item1.StartsWith("R") ? "R" : (pivot.Item1.StartsWith("S") ? "S" : "PP");
                    if (!dxPivotTextLayouts.ContainsKey(pivot.Item1) || !dxPivotBrushes.ContainsKey(brushKey))
                        continue;

                    float y = (float)chartScale.GetYByValue(pivot.Item2);
                    int toIndex = ChartBars.ToIndex;
                    int fromIndex = Math.Max(ChartBars.FromIndex, toIndex - PivotLineWidth);
                    float xStart = (float)chartControl.GetXByBarIndex(ChartBars, fromIndex);
                    float xEnd = (float)chartControl.GetXByBarIndex(ChartBars, toIndex);

                    TextLayout layout = dxPivotTextLayouts[pivot.Item1];
                    SharpDX.Direct2D1.Brush brush = dxPivotBrushes[brushKey];
                    Stroke stroke = (brushKey == "R") ? RStroke : ((brushKey == "S") ? SStroke : PpStroke);

                    RenderTarget.DrawLine(new SharpDX.Vector2(xStart, y), new SharpDX.Vector2(xEnd, y), 
                        brush, stroke.Width, stroke.StrokeStyle);

                    SharpDX.Vector2 textPos = new SharpDX.Vector2(
                        xStart - layout.Metrics.Width - PivotLabelHorizontalOffset, 
                        y - layout.Metrics.Height / 2f);
                    RenderTarget.DrawTextLayout(textPos, layout, brush);
                }
            }

            // Draw Watermark
            if (ShowWatermark && dxWatermarkBrush != null)
            {
                string watermarkText = GetWatermarkDisplayString();
                if (!string.IsNullOrEmpty(watermarkText))
                {
                    if (dxWatermarkTextFormat == null || dxWatermarkTextFormat.FontSize != WatermarkFontSize)
                    {
                        if (dxWatermarkTextFormat != null)
                            dxWatermarkTextFormat.Dispose();
                        dxWatermarkTextFormat = new TextFormat(Core.Globals.DirectWriteFactory, "Arial", 
                            SharpDX.DirectWrite.FontWeight.Bold, SharpDX.DirectWrite.FontStyle.Normal, WatermarkFontSize);
                    }

                    dxWatermarkTextFormat.TextAlignment = WatermarkHAlign == "Left" ? SharpDX.DirectWrite.TextAlignment.Leading : 
                        (WatermarkHAlign == "Right" ? SharpDX.DirectWrite.TextAlignment.Trailing : SharpDX.DirectWrite.TextAlignment.Center);

                    if (dxWatermarkTextLayout == null || lastWatermarkText != watermarkText)
                    {
                        if (dxWatermarkTextLayout != null)
                            dxWatermarkTextLayout.Dispose();
                        
                        ChartPanel panel = chartControl.ChartPanels[chartScale.PanelIndex];
                        dxWatermarkTextLayout = new TextLayout(Core.Globals.DirectWriteFactory, watermarkText, 
                            dxWatermarkTextFormat, (float)panel.W, (float)panel.H);
                        lastWatermarkText = watermarkText;
                    }

                    ChartPanel panel1 = chartControl.ChartPanels[chartScale.PanelIndex];
                    float xPos = (float)panel1.X + WatermarkOffsetX;
                    float yPos = WatermarkVAlign == "Top" ? (float)panel1.Y : 
                        (WatermarkVAlign == "Bottom" ? (float)(panel1.Y + panel1.H) - dxWatermarkTextLayout.Metrics.Height : 
                        (float)panel1.Y + (float)((panel1.H - dxWatermarkTextLayout.Metrics.Height) / 2.0));
                    yPos += WatermarkOffsetY;

                    RenderTarget.DrawTextLayout(new SharpDX.Vector2(xPos, yPos), dxWatermarkTextLayout, dxWatermarkBrush);
                }
            }
        }

        private string GetWatermarkDisplayString()
        {
            List<string> parts = new List<string>();
            
            if (!string.IsNullOrEmpty(WatermarkCustomText))
                parts.Add(WatermarkCustomText);
            
            if (WatermarkInstrumentDisplay == "Show")
                parts.Add(Instrument.MasterInstrument.Name);
            
            if (WatermarkPeriodDisplay == "Show")
                parts.Add("(" + GetPeriodString() + ")");
            
            return string.Join(Environment.NewLine, parts.Where(p => !string.IsNullOrEmpty(p)));
        }

        private string GetPeriodString()
        {
            switch (BarsArray[0].BarsPeriod.BarsPeriodType)
            {
                case BarsPeriodType.Tick: return BarsArray[0].BarsPeriod.Value + "t";
                case BarsPeriodType.Volume: return BarsArray[0].BarsPeriod.Value + "vol";
                case BarsPeriodType.Range: return BarsArray[0].BarsPeriod.Value + " range";
                case BarsPeriodType.Second: return BarsArray[0].BarsPeriod.Value + "s";
                case BarsPeriodType.Minute: return BarsArray[0].BarsPeriod.Value + "m";
                case BarsPeriodType.Day: return BarsArray[0].BarsPeriod.Value + "D";
                case BarsPeriodType.Week: return BarsArray[0].BarsPeriod.Value + "W";
                case BarsPeriodType.Month: return BarsArray[0].BarsPeriod.Value + "M";
                case BarsPeriodType.Year: return BarsArray[0].BarsPeriod.Value + "Y";
                default: return BarsArray[0].BarsPeriod.ToString();
            }
        }
        #endregion

        #region Properties
        // General
        [NinjaScriptProperty]
        [Display(Name = "Show Price Line", GroupName = "General", Order = 0)]
        public bool ShowPriceLine { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Fibonacci Pivots", GroupName = "General", Order = 1)]
        public bool ShowFibonacciPivots { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Daily Open", GroupName = "General", Order = 2)]
        public bool ShowSessionOpen { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Current Day High/Low", GroupName = "General", Order = 3)]
        public bool ShowCurrentDayHighLow { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Prior Day High/Low", GroupName = "General", Order = 4)]
        public bool ShowPriorDayHighLow { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Weekly IB", GroupName = "General", Order = 5)]
        public bool ShowWeeklyIB { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Secondary Pivots", GroupName = "General", Order = 6)]
        public bool ShowSecondaryPivots { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show NY Open", GroupName = "General", Order = 7)]
        public bool ShowNYOpen { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show London Open", GroupName = "General", Order = 8)]
        public bool ShowLondonOpen { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Asia Open", GroupName = "General", Order = 9)]
        public bool ShowAsiaOpen { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Golden Levels", GroupName = "General", Order = 10)]
        public bool ShowGoldenLevels { get; set; }

        // Secondary Pivots
        [NinjaScriptProperty]
        [Display(Name = "Period Type", GroupName = "Secondary Pivots", Order = 0)]
        public BarsPeriodType SecondaryPivotPeriodType { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period Value", GroupName = "Secondary Pivots", Order = 1)]
        public int SecondaryPivotPeriodValue { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Keep Broken Pivots", GroupName = "Secondary Pivots", Order = 2)]
        public bool KeepBrokenPivots { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Bearish Pivot Line", GroupName = "Secondary Pivots", Order = 3)]
        public Stroke BearishPivotLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bearish Pivot Label", GroupName = "Secondary Pivots", Order = 4)]
        public System.Windows.Media.Brush BearishPivotLabelColor { get; set; }

        [Browsable(false)]
        public string BearishPivotLabelColorSerializable
        {
            get { return Serialize.BrushToString(BearishPivotLabelColor); }
            set { BearishPivotLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Bearish Pivot Text", GroupName = "Secondary Pivots", Order = 5)]
        public string BearishPivotLabelText { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Bullish Pivot Line", GroupName = "Secondary Pivots", Order = 6)]
        public Stroke BullishPivotLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Bullish Pivot Label", GroupName = "Secondary Pivots", Order = 7)]
        public System.Windows.Media.Brush BullishPivotLabelColor { get; set; }

        [Browsable(false)]
        public string BullishPivotLabelColorSerializable
        {
            get { return Serialize.BrushToString(BullishPivotLabelColor); }
            set { BullishPivotLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Bullish Pivot Text", GroupName = "Secondary Pivots", Order = 8)]
        public string BullishPivotLabelText { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Max Pivots Per Side", GroupName = "Secondary Pivots", Order = 9)]
        [Range(0, int.MaxValue)]
        public int MaxSecondaryPivots { get; set; }

        // Weekly IB
        [NinjaScriptProperty]
        [Display(Name = "IB High Line", GroupName = "Weekly IB", Order = 0)]
        public Stroke IBHighLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "IB High Label", GroupName = "Weekly IB", Order = 1)]
        public System.Windows.Media.Brush IBHighLabelColor { get; set; }

        [Browsable(false)]
        public string IBHighLabelColorSerializable
        {
            get { return Serialize.BrushToString(IBHighLabelColor); }
            set { IBHighLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "IB Low Line", GroupName = "Weekly IB", Order = 2)]
        public Stroke IBLowLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "IB Low Label", GroupName = "Weekly IB", Order = 3)]
        public System.Windows.Media.Brush IBLowLabelColor { get; set; }

        [Browsable(false)]
        public string IBLowLabelColorSerializable
        {
            get { return Serialize.BrushToString(IBLowLabelColor); }
            set { IBLowLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Fill IB Range", GroupName = "Weekly IB", Order = 4)]
        public bool FillWeeklyIB { get; set; }

        [XmlIgnore]
        [Display(Name = "IB Fill Color", GroupName = "Weekly IB", Order = 5)]
        public System.Windows.Media.Brush IBFillColor { get; set; }

        [Browsable(false)]
        public string IBFillColorSerializable
        {
            get { return Serialize.BrushToString(IBFillColor); }
            set { IBFillColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Fill Opacity", GroupName = "Weekly IB", Order = 6)]
        [Range(0, 100)]
        public int IBFillOpacity { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "High Label Horiz. Offset (Bars)", GroupName = "Weekly IB", Order = 7)]
        [Range(-100, 100)]
        public int IBHighLabelHorizontalOffsetBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "High Label Vert. Offset", GroupName = "Weekly IB", Order = 8)]
        [Range(-100, 100)]
        public int IBHighLabelVerticalOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Low Label Horiz. Offset (Bars)", GroupName = "Weekly IB", Order = 9)]
        [Range(-100, 100)]
        public int IBLowLabelHorizontalOffsetBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Low Label Vert. Offset", GroupName = "Weekly IB", Order = 10)]
        [Range(-100, 100)]
        public int IBLowLabelVerticalOffset { get; set; }

        // Price Line & Fibonacci Pivots
        [NinjaScriptProperty]
        [Display(Name = "Price Line", GroupName = "Price Line", Order = 0)]
        public Stroke PriceLineStroke { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Pivot Point Style", GroupName = "Fibonacci Pivots", Order = 0)]
        public Stroke PpStroke { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Resistance Style", GroupName = "Fibonacci Pivots", Order = 1)]
        public Stroke RStroke { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Support Style", GroupName = "Fibonacci Pivots", Order = 2)]
        public Stroke SStroke { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Pivot Line Width (Bars)", GroupName = "Fibonacci Pivots", Order = 3)]
        [Range(1, int.MaxValue)]
        public int PivotLineWidth { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Label Horizontal Offset", GroupName = "Fibonacci Pivots", Order = 4)]
        [Range(0, 100)]
        public int PivotLabelHorizontalOffset { get; set; }

        // Current Day Levels
        [NinjaScriptProperty]
        [Display(Name = "Current Day High Line", GroupName = "Current Day Levels", Order = 0)]
        public Stroke CurrentDayHighLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Current Day High Label", GroupName = "Current Day Levels", Order = 1)]
        public System.Windows.Media.Brush CurrentDayHighLabelColor { get; set; }

        [Browsable(false)]
        public string CurrentDayHighLabelColorSerializable
        {
            get { return Serialize.BrushToString(CurrentDayHighLabelColor); }
            set { CurrentDayHighLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Current Day Low Line", GroupName = "Current Day Levels", Order = 2)]
        public Stroke CurrentDayLowLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Current Day Low Label", GroupName = "Current Day Levels", Order = 3)]
        public System.Windows.Media.Brush CurrentDayLowLabelColor { get; set; }

        [Browsable(false)]
        public string CurrentDayLowLabelColorSerializable
        {
            get { return Serialize.BrushToString(CurrentDayLowLabelColor); }
            set { CurrentDayLowLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "High Label Horiz. Offset (Bars)", GroupName = "Current Day Levels", Order = 4)]
        [Range(-100, 100)]
        public int CurrentDayHighLabelHorizontalOffsetBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "High Label Vert. Offset", GroupName = "Current Day Levels", Order = 5)]
        [Range(-100, 100)]
        public int CurrentDayHighLabelVerticalOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Low Label Horiz. Offset (Bars)", GroupName = "Current Day Levels", Order = 6)]
        [Range(-100, 100)]
        public int CurrentDayLowLabelHorizontalOffsetBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Low Label Vert. Offset", GroupName = "Current Day Levels", Order = 7)]
        [Range(-100, 100)]
        public int CurrentDayLowLabelVerticalOffset { get; set; }

        // Prior Day Levels
        [NinjaScriptProperty]
        [Display(Name = "Prior Day High Line", GroupName = "Prior Day Levels", Order = 0)]
        public Stroke PriorDayHighLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Prior Day High Label", GroupName = "Prior Day Levels", Order = 1)]
        public System.Windows.Media.Brush PriorDayHighLabelColor { get; set; }

        [Browsable(false)]
        public string PriorDayHighLabelColorSerializable
        {
            get { return Serialize.BrushToString(PriorDayHighLabelColor); }
            set { PriorDayHighLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Prior Day Low Line", GroupName = "Prior Day Levels", Order = 2)]
        public Stroke PriorDayLowLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Prior Day Low Label", GroupName = "Prior Day Levels", Order = 3)]
        public System.Windows.Media.Brush PriorDayLowLabelColor { get; set; }

        [Browsable(false)]
        public string PriorDayLowLabelColorSerializable
        {
            get { return Serialize.BrushToString(PriorDayLowLabelColor); }
            set { PriorDayLowLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "High Label Horiz. Offset (Bars)", GroupName = "Prior Day Levels", Order = 4)]
        [Range(-100, 100)]
        public int PriorDayHighLabelHorizontalOffsetBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "High Label Vert. Offset", GroupName = "Prior Day Levels", Order = 5)]
        [Range(-100, 100)]
        public int PriorDayHighLabelVerticalOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Low Label Horiz. Offset (Bars)", GroupName = "Prior Day Levels", Order = 6)]
        [Range(-100, 100)]
        public int PriorDayLowLabelHorizontalOffsetBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Low Label Vert. Offset", GroupName = "Prior Day Levels", Order = 7)]
        [Range(-100, 100)]
        public int PriorDayLowLabelVerticalOffset { get; set; }

        // Open Times
        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "NY Open Time", GroupName = "Open Times & Styles", Order = 0)]
        public DateTime NYOpenTime { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "NY Open Line", GroupName = "Open Times & Styles", Order = 1)]
        public Stroke NYOpenLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "NY Open Label", GroupName = "Open Times & Styles", Order = 2)]
        public System.Windows.Media.Brush NYOpenLabelColor { get; set; }

        [Browsable(false)]
        public string NYOpenLabelColorSerializable
        {
            get { return Serialize.BrushToString(NYOpenLabelColor); }
            set { NYOpenLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "London Open Time", GroupName = "Open Times & Styles", Order = 3)]
        public DateTime LondonOpenTime { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "London Open Line", GroupName = "Open Times & Styles", Order = 4)]
        public Stroke LondonOpenLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "London Open Label", GroupName = "Open Times & Styles", Order = 5)]
        public System.Windows.Media.Brush LondonOpenLabelColor { get; set; }

        [Browsable(false)]
        public string LondonOpenLabelColorSerializable
        {
            get { return Serialize.BrushToString(LondonOpenLabelColor); }
            set { LondonOpenLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Asia Open Time", GroupName = "Open Times & Styles", Order = 6)]
        public DateTime AsiaOpenTime { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Asia Open Line", GroupName = "Open Times & Styles", Order = 7)]
        public Stroke AsiaOpenLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Asia Open Label", GroupName = "Open Times & Styles", Order = 8)]
        public System.Windows.Media.Brush AsiaOpenLabelColor { get; set; }

        [Browsable(false)]
        public string AsiaOpenLabelColorSerializable
        {
            get { return Serialize.BrushToString(AsiaOpenLabelColor); }
            set { AsiaOpenLabelColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Daily Open Line", GroupName = "Open Times & Styles", Order = 9)]
        public Stroke SessionOpenLineStyle { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Daily Open Label", GroupName = "Open Times & Styles", Order = 10)]
        public System.Windows.Media.Brush SessionOpenLabelColor { get; set; }

        [Browsable(false)]
        public string SessionOpenLabelColorSerializable
        {
            get { return Serialize.BrushToString(SessionOpenLabelColor); }
            set { SessionOpenLabelColor = Serialize.StringToBrush(value); }
        }

        // Golden Levels
        [NinjaScriptProperty]
        [Display(Name = "Level 00 Offset", GroupName = "Golden Levels", Order = 0)]
        [Range(0, 99)]
        public int Level00 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Level 00 Line", GroupName = "Golden Levels", Order = 1)]
        public Stroke Level00Line { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Level 26 Offset", GroupName = "Golden Levels", Order = 2)]
        [Range(0, 99)]
        public int Level26 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Level 26 Line", GroupName = "Golden Levels", Order = 3)]
        public Stroke Level26Line { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Level 50 Offset", GroupName = "Golden Levels", Order = 4)]
        [Range(0, 99)]
        public int Level50 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Level 50 Line", GroupName = "Golden Levels", Order = 5)]
        public Stroke Level50Line { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Level 77 Offset", GroupName = "Golden Levels", Order = 6)]
        [Range(0, 99)]
        public int Level77 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Level 77 Line", GroupName = "Golden Levels", Order = 7)]
        public Stroke Level77Line { get; set; }

        // Golden Level Labels
        [NinjaScriptProperty]
        [Display(Name = "Horizontal Offset", GroupName = "Golden Level Labels", Order = 0)]
        public int GoldenLabelHorizontalOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Vertical Offset", GroupName = "Golden Level Labels", Order = 1)]
        public int GoldenLabelVerticalOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Font Size", GroupName = "Golden Level Labels", Order = 2)]
        public int GoldenLabelFontSize { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Font Bold", GroupName = "Golden Level Labels", Order = 3)]
        public bool GoldenLabelFontBold { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Label Color", GroupName = "Golden Level Labels", Order = 4)]
        public System.Windows.Media.Brush GoldenLevelLabelColor { get; set; }

        [Browsable(false)]
        public string GoldenLevelLabelColorSerializable
        {
            get { return Serialize.BrushToString(GoldenLevelLabelColor); }
            set { GoldenLevelLabelColor = Serialize.StringToBrush(value); }
        }

        // General Label Settings
        [NinjaScriptProperty]
        [Display(Name = "Horizontal Offset", GroupName = "General Label Settings", Order = 0)]
        public int LabelHorizontalOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Vertical Offset", GroupName = "General Label Settings", Order = 1)]
        public int LabelVerticalOffset { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Font Size", GroupName = "General Label Settings", Order = 2)]
        public int LabelFontSize { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Font Bold", GroupName = "General Label Settings", Order = 3)]
        public bool LabelFontBold { get; set; }

        // Watermark
        [NinjaScriptProperty]
        [Display(Name = "Show Watermark", GroupName = "Watermark", Order = 0)]
        public bool ShowWatermark { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Custom Text", GroupName = "Watermark", Order = 1)]
        public string WatermarkCustomText { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Instrument Display", GroupName = "Watermark", Order = 2)]
        public string WatermarkInstrumentDisplay { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Period Display", GroupName = "Watermark", Order = 3)]
        public string WatermarkPeriodDisplay { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Color", GroupName = "Watermark", Order = 4)]
        public System.Windows.Media.Brush WatermarkColor { get; set; }

        [Browsable(false)]
        public string WatermarkColorSerializable
        {
            get { return Serialize.BrushToString(WatermarkColor); }
            set { WatermarkColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Font Size", GroupName = "Watermark", Order = 5)]
        [Range(1, 200)]
        public int WatermarkFontSize { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opacity", GroupName = "Watermark", Order = 6)]
        [Range(0, 100)]
        public int WatermarkOpacity { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Horizontal Alignment", GroupName = "Watermark", Order = 7)]
        public string WatermarkHAlign { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Vertical Alignment", GroupName = "Watermark", Order = 8)]
        public string WatermarkVAlign { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Horizontal Offset", GroupName = "Watermark", Order = 9)]
        public int WatermarkOffsetX { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Vertical Offset", GroupName = "Watermark", Order = 10)]
        public int WatermarkOffsetY { get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name = "Prior Day Close Line", GroupName = "Prior Day Levels", Order = 8)]
		public Stroke PriorDayCloseLineStyle { get; set; }

		

		// Current Week Levels
		[NinjaScriptProperty]
		[Display(Name = "Show Current Week High/Low", GroupName = "General", Order = 12)]
		public bool ShowCurrentWeekHighLow { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Current Week High Line", GroupName = "Current Week Levels", Order = 0)]
		public Stroke CurrentWeekHighLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Current Week High Label", GroupName = "Current Week Levels", Order = 1)]
		public System.Windows.Media.Brush CurrentWeekHighLabelColor { get; set; }

		[Browsable(false)]
		public string CurrentWeekHighLabelColorSerializable
		{
			get { return Serialize.BrushToString(CurrentWeekHighLabelColor); }
			set { CurrentWeekHighLabelColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Current Week Low Line", GroupName = "Current Week Levels", Order = 2)]
		public Stroke CurrentWeekLowLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Current Week Low Label", GroupName = "Current Week Levels", Order = 3)]
		public System.Windows.Media.Brush CurrentWeekLowLabelColor { get; set; }

		[Browsable(false)]
		public string CurrentWeekLowLabelColorSerializable
		{
			get { return Serialize.BrushToString(CurrentWeekLowLabelColor); }
			set { CurrentWeekLowLabelColor = Serialize.StringToBrush(value); }
		}

		// Prior Week Levels
		[NinjaScriptProperty]
		[Display(Name = "Show Prior Week High/Low", GroupName = "General", Order = 13)]
		public bool ShowPriorWeekHighLow { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Prior Week High Line", GroupName = "Prior Week Levels", Order = 0)]
		public Stroke PriorWeekHighLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Prior Week High Label", GroupName = "Prior Week Levels", Order = 1)]
		public System.Windows.Media.Brush PriorWeekHighLabelColor { get; set; }

		[Browsable(false)]
		public string PriorWeekHighLabelColorSerializable
		{
			get { return Serialize.BrushToString(PriorWeekHighLabelColor); }
			set { PriorWeekHighLabelColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Prior Week Low Line", GroupName = "Prior Week Levels", Order = 2)]
		public Stroke PriorWeekLowLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Prior Week Low Label", GroupName = "Prior Week Levels", Order = 3)]
		public System.Windows.Media.Brush PriorWeekLowLabelColor { get; set; }

		[Browsable(false)]
		public string PriorWeekLowLabelColorSerializable
		{
			get { return Serialize.BrushToString(PriorWeekLowLabelColor); }
			set { PriorWeekLowLabelColor = Serialize.StringToBrush(value); }
		}

		// Current Month Levels
		[NinjaScriptProperty]
		[Display(Name = "Show Current Month High/Low", GroupName = "General", Order = 14)]
		public bool ShowCurrentMonthHighLow { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Current Month High Line", GroupName = "Current Month Levels", Order = 0)]
		public Stroke CurrentMonthHighLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Current Month High Label", GroupName = "Current Month Levels", Order = 1)]
		public System.Windows.Media.Brush CurrentMonthHighLabelColor { get; set; }

		[Browsable(false)]
		public string CurrentMonthHighLabelColorSerializable
		{
			get { return Serialize.BrushToString(CurrentMonthHighLabelColor); }
			set { CurrentMonthHighLabelColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Current Month Low Line", GroupName = "Current Month Levels", Order = 2)]
		public Stroke CurrentMonthLowLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Current Month Low Label", GroupName = "Current Month Levels", Order = 3)]
		public System.Windows.Media.Brush CurrentMonthLowLabelColor { get; set; }

		[Browsable(false)]
		public string CurrentMonthLowLabelColorSerializable
		{
			get { return Serialize.BrushToString(CurrentMonthLowLabelColor); }
			set { CurrentMonthLowLabelColor = Serialize.StringToBrush(value); }
		}

		// Prior Month Levels
		[NinjaScriptProperty]
		[Display(Name = "Show Prior Month High/Low", GroupName = "General", Order = 15)]
		public bool ShowPriorMonthHighLow { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Prior Month High Line", GroupName = "Prior Month Levels", Order = 0)]
		public Stroke PriorMonthHighLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Prior Month High Label", GroupName = "Prior Month Levels", Order = 1)]
		public System.Windows.Media.Brush PriorMonthHighLabelColor { get; set; }

		[Browsable(false)]
		public string PriorMonthHighLabelColorSerializable
		{
			get { return Serialize.BrushToString(PriorMonthHighLabelColor); }
			set { PriorMonthHighLabelColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Prior Month Low Line", GroupName = "Prior Month Levels", Order = 2)]
		public Stroke PriorMonthLowLineStyle { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Prior Month Low Label", GroupName = "Prior Month Levels", Order = 3)]
		public System.Windows.Media.Brush PriorMonthLowLabelColor { get; set; }

		[Browsable(false)]
		public string PriorMonthLowLabelColorSerializable
		{
			get { return Serialize.BrushToString(PriorMonthLowLabelColor); }
			set { PriorMonthLowLabelColor = Serialize.StringToBrush(value); }
		}

		

        #endregion
    }

    #region PivotInfo Class
    public class PivotInfo
    {
        public string Tag { get; set; }
        public double Price { get; set; }
        public int StartBarIndex { get; set; }
    }
    #endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GraciKeyLevels[] cacheGraciKeyLevels;
		public GraciKeyLevels GraciKeyLevels(bool showPriceLine, bool showFibonacciPivots, bool showSessionOpen, bool showCurrentDayHighLow, bool showPriorDayHighLow, bool showWeeklyIB, bool showSecondaryPivots, bool showNYOpen, bool showLondonOpen, bool showAsiaOpen, bool showGoldenLevels, BarsPeriodType secondaryPivotPeriodType, int secondaryPivotPeriodValue, bool keepBrokenPivots, Stroke bearishPivotLineStyle, System.Windows.Media.Brush bearishPivotLabelColor, string bearishPivotLabelText, Stroke bullishPivotLineStyle, System.Windows.Media.Brush bullishPivotLabelColor, string bullishPivotLabelText, int maxSecondaryPivots, Stroke iBHighLineStyle, System.Windows.Media.Brush iBHighLabelColor, Stroke iBLowLineStyle, System.Windows.Media.Brush iBLowLabelColor, bool fillWeeklyIB, int iBFillOpacity, int iBHighLabelHorizontalOffsetBars, int iBHighLabelVerticalOffset, int iBLowLabelHorizontalOffsetBars, int iBLowLabelVerticalOffset, Stroke priceLineStroke, Stroke ppStroke, Stroke rStroke, Stroke sStroke, int pivotLineWidth, int pivotLabelHorizontalOffset, Stroke currentDayHighLineStyle, System.Windows.Media.Brush currentDayHighLabelColor, Stroke currentDayLowLineStyle, System.Windows.Media.Brush currentDayLowLabelColor, int currentDayHighLabelHorizontalOffsetBars, int currentDayHighLabelVerticalOffset, int currentDayLowLabelHorizontalOffsetBars, int currentDayLowLabelVerticalOffset, Stroke priorDayHighLineStyle, System.Windows.Media.Brush priorDayHighLabelColor, Stroke priorDayLowLineStyle, System.Windows.Media.Brush priorDayLowLabelColor, int priorDayHighLabelHorizontalOffsetBars, int priorDayHighLabelVerticalOffset, int priorDayLowLabelHorizontalOffsetBars, int priorDayLowLabelVerticalOffset, DateTime nYOpenTime, Stroke nYOpenLineStyle, System.Windows.Media.Brush nYOpenLabelColor, DateTime londonOpenTime, Stroke londonOpenLineStyle, System.Windows.Media.Brush londonOpenLabelColor, DateTime asiaOpenTime, Stroke asiaOpenLineStyle, System.Windows.Media.Brush asiaOpenLabelColor, Stroke sessionOpenLineStyle, System.Windows.Media.Brush sessionOpenLabelColor, int level00, Stroke level00Line, int level26, Stroke level26Line, int level50, Stroke level50Line, int level77, Stroke level77Line, int goldenLabelHorizontalOffset, int goldenLabelVerticalOffset, int goldenLabelFontSize, bool goldenLabelFontBold, System.Windows.Media.Brush goldenLevelLabelColor, int labelHorizontalOffset, int labelVerticalOffset, int labelFontSize, bool labelFontBold, bool showWatermark, string watermarkCustomText, string watermarkInstrumentDisplay, string watermarkPeriodDisplay, System.Windows.Media.Brush watermarkColor, int watermarkFontSize, int watermarkOpacity, string watermarkHAlign, string watermarkVAlign, int watermarkOffsetX, int watermarkOffsetY, Stroke priorDayCloseLineStyle, bool showCurrentWeekHighLow, Stroke currentWeekHighLineStyle, System.Windows.Media.Brush currentWeekHighLabelColor, Stroke currentWeekLowLineStyle, System.Windows.Media.Brush currentWeekLowLabelColor, bool showPriorWeekHighLow, Stroke priorWeekHighLineStyle, System.Windows.Media.Brush priorWeekHighLabelColor, Stroke priorWeekLowLineStyle, System.Windows.Media.Brush priorWeekLowLabelColor, bool showCurrentMonthHighLow, Stroke currentMonthHighLineStyle, System.Windows.Media.Brush currentMonthHighLabelColor, Stroke currentMonthLowLineStyle, System.Windows.Media.Brush currentMonthLowLabelColor, bool showPriorMonthHighLow, Stroke priorMonthHighLineStyle, System.Windows.Media.Brush priorMonthHighLabelColor, Stroke priorMonthLowLineStyle, System.Windows.Media.Brush priorMonthLowLabelColor)
		{
			return GraciKeyLevels(Input, showPriceLine, showFibonacciPivots, showSessionOpen, showCurrentDayHighLow, showPriorDayHighLow, showWeeklyIB, showSecondaryPivots, showNYOpen, showLondonOpen, showAsiaOpen, showGoldenLevels, secondaryPivotPeriodType, secondaryPivotPeriodValue, keepBrokenPivots, bearishPivotLineStyle, bearishPivotLabelColor, bearishPivotLabelText, bullishPivotLineStyle, bullishPivotLabelColor, bullishPivotLabelText, maxSecondaryPivots, iBHighLineStyle, iBHighLabelColor, iBLowLineStyle, iBLowLabelColor, fillWeeklyIB, iBFillOpacity, iBHighLabelHorizontalOffsetBars, iBHighLabelVerticalOffset, iBLowLabelHorizontalOffsetBars, iBLowLabelVerticalOffset, priceLineStroke, ppStroke, rStroke, sStroke, pivotLineWidth, pivotLabelHorizontalOffset, currentDayHighLineStyle, currentDayHighLabelColor, currentDayLowLineStyle, currentDayLowLabelColor, currentDayHighLabelHorizontalOffsetBars, currentDayHighLabelVerticalOffset, currentDayLowLabelHorizontalOffsetBars, currentDayLowLabelVerticalOffset, priorDayHighLineStyle, priorDayHighLabelColor, priorDayLowLineStyle, priorDayLowLabelColor, priorDayHighLabelHorizontalOffsetBars, priorDayHighLabelVerticalOffset, priorDayLowLabelHorizontalOffsetBars, priorDayLowLabelVerticalOffset, nYOpenTime, nYOpenLineStyle, nYOpenLabelColor, londonOpenTime, londonOpenLineStyle, londonOpenLabelColor, asiaOpenTime, asiaOpenLineStyle, asiaOpenLabelColor, sessionOpenLineStyle, sessionOpenLabelColor, level00, level00Line, level26, level26Line, level50, level50Line, level77, level77Line, goldenLabelHorizontalOffset, goldenLabelVerticalOffset, goldenLabelFontSize, goldenLabelFontBold, goldenLevelLabelColor, labelHorizontalOffset, labelVerticalOffset, labelFontSize, labelFontBold, showWatermark, watermarkCustomText, watermarkInstrumentDisplay, watermarkPeriodDisplay, watermarkColor, watermarkFontSize, watermarkOpacity, watermarkHAlign, watermarkVAlign, watermarkOffsetX, watermarkOffsetY, priorDayCloseLineStyle, showCurrentWeekHighLow, currentWeekHighLineStyle, currentWeekHighLabelColor, currentWeekLowLineStyle, currentWeekLowLabelColor, showPriorWeekHighLow, priorWeekHighLineStyle, priorWeekHighLabelColor, priorWeekLowLineStyle, priorWeekLowLabelColor, showCurrentMonthHighLow, currentMonthHighLineStyle, currentMonthHighLabelColor, currentMonthLowLineStyle, currentMonthLowLabelColor, showPriorMonthHighLow, priorMonthHighLineStyle, priorMonthHighLabelColor, priorMonthLowLineStyle, priorMonthLowLabelColor);
		}

		public GraciKeyLevels GraciKeyLevels(ISeries<double> input, bool showPriceLine, bool showFibonacciPivots, bool showSessionOpen, bool showCurrentDayHighLow, bool showPriorDayHighLow, bool showWeeklyIB, bool showSecondaryPivots, bool showNYOpen, bool showLondonOpen, bool showAsiaOpen, bool showGoldenLevels, BarsPeriodType secondaryPivotPeriodType, int secondaryPivotPeriodValue, bool keepBrokenPivots, Stroke bearishPivotLineStyle, System.Windows.Media.Brush bearishPivotLabelColor, string bearishPivotLabelText, Stroke bullishPivotLineStyle, System.Windows.Media.Brush bullishPivotLabelColor, string bullishPivotLabelText, int maxSecondaryPivots, Stroke iBHighLineStyle, System.Windows.Media.Brush iBHighLabelColor, Stroke iBLowLineStyle, System.Windows.Media.Brush iBLowLabelColor, bool fillWeeklyIB, int iBFillOpacity, int iBHighLabelHorizontalOffsetBars, int iBHighLabelVerticalOffset, int iBLowLabelHorizontalOffsetBars, int iBLowLabelVerticalOffset, Stroke priceLineStroke, Stroke ppStroke, Stroke rStroke, Stroke sStroke, int pivotLineWidth, int pivotLabelHorizontalOffset, Stroke currentDayHighLineStyle, System.Windows.Media.Brush currentDayHighLabelColor, Stroke currentDayLowLineStyle, System.Windows.Media.Brush currentDayLowLabelColor, int currentDayHighLabelHorizontalOffsetBars, int currentDayHighLabelVerticalOffset, int currentDayLowLabelHorizontalOffsetBars, int currentDayLowLabelVerticalOffset, Stroke priorDayHighLineStyle, System.Windows.Media.Brush priorDayHighLabelColor, Stroke priorDayLowLineStyle, System.Windows.Media.Brush priorDayLowLabelColor, int priorDayHighLabelHorizontalOffsetBars, int priorDayHighLabelVerticalOffset, int priorDayLowLabelHorizontalOffsetBars, int priorDayLowLabelVerticalOffset, DateTime nYOpenTime, Stroke nYOpenLineStyle, System.Windows.Media.Brush nYOpenLabelColor, DateTime londonOpenTime, Stroke londonOpenLineStyle, System.Windows.Media.Brush londonOpenLabelColor, DateTime asiaOpenTime, Stroke asiaOpenLineStyle, System.Windows.Media.Brush asiaOpenLabelColor, Stroke sessionOpenLineStyle, System.Windows.Media.Brush sessionOpenLabelColor, int level00, Stroke level00Line, int level26, Stroke level26Line, int level50, Stroke level50Line, int level77, Stroke level77Line, int goldenLabelHorizontalOffset, int goldenLabelVerticalOffset, int goldenLabelFontSize, bool goldenLabelFontBold, System.Windows.Media.Brush goldenLevelLabelColor, int labelHorizontalOffset, int labelVerticalOffset, int labelFontSize, bool labelFontBold, bool showWatermark, string watermarkCustomText, string watermarkInstrumentDisplay, string watermarkPeriodDisplay, System.Windows.Media.Brush watermarkColor, int watermarkFontSize, int watermarkOpacity, string watermarkHAlign, string watermarkVAlign, int watermarkOffsetX, int watermarkOffsetY, Stroke priorDayCloseLineStyle, bool showCurrentWeekHighLow, Stroke currentWeekHighLineStyle, System.Windows.Media.Brush currentWeekHighLabelColor, Stroke currentWeekLowLineStyle, System.Windows.Media.Brush currentWeekLowLabelColor, bool showPriorWeekHighLow, Stroke priorWeekHighLineStyle, System.Windows.Media.Brush priorWeekHighLabelColor, Stroke priorWeekLowLineStyle, System.Windows.Media.Brush priorWeekLowLabelColor, bool showCurrentMonthHighLow, Stroke currentMonthHighLineStyle, System.Windows.Media.Brush currentMonthHighLabelColor, Stroke currentMonthLowLineStyle, System.Windows.Media.Brush currentMonthLowLabelColor, bool showPriorMonthHighLow, Stroke priorMonthHighLineStyle, System.Windows.Media.Brush priorMonthHighLabelColor, Stroke priorMonthLowLineStyle, System.Windows.Media.Brush priorMonthLowLabelColor)
		{
			if (cacheGraciKeyLevels != null)
				for (int idx = 0; idx < cacheGraciKeyLevels.Length; idx++)
					if (cacheGraciKeyLevels[idx] != null && cacheGraciKeyLevels[idx].ShowPriceLine == showPriceLine && cacheGraciKeyLevels[idx].ShowFibonacciPivots == showFibonacciPivots && cacheGraciKeyLevels[idx].ShowSessionOpen == showSessionOpen && cacheGraciKeyLevels[idx].ShowCurrentDayHighLow == showCurrentDayHighLow && cacheGraciKeyLevels[idx].ShowPriorDayHighLow == showPriorDayHighLow && cacheGraciKeyLevels[idx].ShowWeeklyIB == showWeeklyIB && cacheGraciKeyLevels[idx].ShowSecondaryPivots == showSecondaryPivots && cacheGraciKeyLevels[idx].ShowNYOpen == showNYOpen && cacheGraciKeyLevels[idx].ShowLondonOpen == showLondonOpen && cacheGraciKeyLevels[idx].ShowAsiaOpen == showAsiaOpen && cacheGraciKeyLevels[idx].ShowGoldenLevels == showGoldenLevels && cacheGraciKeyLevels[idx].SecondaryPivotPeriodType == secondaryPivotPeriodType && cacheGraciKeyLevels[idx].SecondaryPivotPeriodValue == secondaryPivotPeriodValue && cacheGraciKeyLevels[idx].KeepBrokenPivots == keepBrokenPivots && cacheGraciKeyLevels[idx].BearishPivotLineStyle == bearishPivotLineStyle && cacheGraciKeyLevels[idx].BearishPivotLabelColor == bearishPivotLabelColor && cacheGraciKeyLevels[idx].BearishPivotLabelText == bearishPivotLabelText && cacheGraciKeyLevels[idx].BullishPivotLineStyle == bullishPivotLineStyle && cacheGraciKeyLevels[idx].BullishPivotLabelColor == bullishPivotLabelColor && cacheGraciKeyLevels[idx].BullishPivotLabelText == bullishPivotLabelText && cacheGraciKeyLevels[idx].MaxSecondaryPivots == maxSecondaryPivots && cacheGraciKeyLevels[idx].IBHighLineStyle == iBHighLineStyle && cacheGraciKeyLevels[idx].IBHighLabelColor == iBHighLabelColor && cacheGraciKeyLevels[idx].IBLowLineStyle == iBLowLineStyle && cacheGraciKeyLevels[idx].IBLowLabelColor == iBLowLabelColor && cacheGraciKeyLevels[idx].FillWeeklyIB == fillWeeklyIB && cacheGraciKeyLevels[idx].IBFillOpacity == iBFillOpacity && cacheGraciKeyLevels[idx].IBHighLabelHorizontalOffsetBars == iBHighLabelHorizontalOffsetBars && cacheGraciKeyLevels[idx].IBHighLabelVerticalOffset == iBHighLabelVerticalOffset && cacheGraciKeyLevels[idx].IBLowLabelHorizontalOffsetBars == iBLowLabelHorizontalOffsetBars && cacheGraciKeyLevels[idx].IBLowLabelVerticalOffset == iBLowLabelVerticalOffset && cacheGraciKeyLevels[idx].PriceLineStroke == priceLineStroke && cacheGraciKeyLevels[idx].PpStroke == ppStroke && cacheGraciKeyLevels[idx].RStroke == rStroke && cacheGraciKeyLevels[idx].SStroke == sStroke && cacheGraciKeyLevels[idx].PivotLineWidth == pivotLineWidth && cacheGraciKeyLevels[idx].PivotLabelHorizontalOffset == pivotLabelHorizontalOffset && cacheGraciKeyLevels[idx].CurrentDayHighLineStyle == currentDayHighLineStyle && cacheGraciKeyLevels[idx].CurrentDayHighLabelColor == currentDayHighLabelColor && cacheGraciKeyLevels[idx].CurrentDayLowLineStyle == currentDayLowLineStyle && cacheGraciKeyLevels[idx].CurrentDayLowLabelColor == currentDayLowLabelColor && cacheGraciKeyLevels[idx].CurrentDayHighLabelHorizontalOffsetBars == currentDayHighLabelHorizontalOffsetBars && cacheGraciKeyLevels[idx].CurrentDayHighLabelVerticalOffset == currentDayHighLabelVerticalOffset && cacheGraciKeyLevels[idx].CurrentDayLowLabelHorizontalOffsetBars == currentDayLowLabelHorizontalOffsetBars && cacheGraciKeyLevels[idx].CurrentDayLowLabelVerticalOffset == currentDayLowLabelVerticalOffset && cacheGraciKeyLevels[idx].PriorDayHighLineStyle == priorDayHighLineStyle && cacheGraciKeyLevels[idx].PriorDayHighLabelColor == priorDayHighLabelColor && cacheGraciKeyLevels[idx].PriorDayLowLineStyle == priorDayLowLineStyle && cacheGraciKeyLevels[idx].PriorDayLowLabelColor == priorDayLowLabelColor && cacheGraciKeyLevels[idx].PriorDayHighLabelHorizontalOffsetBars == priorDayHighLabelHorizontalOffsetBars && cacheGraciKeyLevels[idx].PriorDayHighLabelVerticalOffset == priorDayHighLabelVerticalOffset && cacheGraciKeyLevels[idx].PriorDayLowLabelHorizontalOffsetBars == priorDayLowLabelHorizontalOffsetBars && cacheGraciKeyLevels[idx].PriorDayLowLabelVerticalOffset == priorDayLowLabelVerticalOffset && cacheGraciKeyLevels[idx].NYOpenTime == nYOpenTime && cacheGraciKeyLevels[idx].NYOpenLineStyle == nYOpenLineStyle && cacheGraciKeyLevels[idx].NYOpenLabelColor == nYOpenLabelColor && cacheGraciKeyLevels[idx].LondonOpenTime == londonOpenTime && cacheGraciKeyLevels[idx].LondonOpenLineStyle == londonOpenLineStyle && cacheGraciKeyLevels[idx].LondonOpenLabelColor == londonOpenLabelColor && cacheGraciKeyLevels[idx].AsiaOpenTime == asiaOpenTime && cacheGraciKeyLevels[idx].AsiaOpenLineStyle == asiaOpenLineStyle && cacheGraciKeyLevels[idx].AsiaOpenLabelColor == asiaOpenLabelColor && cacheGraciKeyLevels[idx].SessionOpenLineStyle == sessionOpenLineStyle && cacheGraciKeyLevels[idx].SessionOpenLabelColor == sessionOpenLabelColor && cacheGraciKeyLevels[idx].Level00 == level00 && cacheGraciKeyLevels[idx].Level00Line == level00Line && cacheGraciKeyLevels[idx].Level26 == level26 && cacheGraciKeyLevels[idx].Level26Line == level26Line && cacheGraciKeyLevels[idx].Level50 == level50 && cacheGraciKeyLevels[idx].Level50Line == level50Line && cacheGraciKeyLevels[idx].Level77 == level77 && cacheGraciKeyLevels[idx].Level77Line == level77Line && cacheGraciKeyLevels[idx].GoldenLabelHorizontalOffset == goldenLabelHorizontalOffset && cacheGraciKeyLevels[idx].GoldenLabelVerticalOffset == goldenLabelVerticalOffset && cacheGraciKeyLevels[idx].GoldenLabelFontSize == goldenLabelFontSize && cacheGraciKeyLevels[idx].GoldenLabelFontBold == goldenLabelFontBold && cacheGraciKeyLevels[idx].GoldenLevelLabelColor == goldenLevelLabelColor && cacheGraciKeyLevels[idx].LabelHorizontalOffset == labelHorizontalOffset && cacheGraciKeyLevels[idx].LabelVerticalOffset == labelVerticalOffset && cacheGraciKeyLevels[idx].LabelFontSize == labelFontSize && cacheGraciKeyLevels[idx].LabelFontBold == labelFontBold && cacheGraciKeyLevels[idx].ShowWatermark == showWatermark && cacheGraciKeyLevels[idx].WatermarkCustomText == watermarkCustomText && cacheGraciKeyLevels[idx].WatermarkInstrumentDisplay == watermarkInstrumentDisplay && cacheGraciKeyLevels[idx].WatermarkPeriodDisplay == watermarkPeriodDisplay && cacheGraciKeyLevels[idx].WatermarkColor == watermarkColor && cacheGraciKeyLevels[idx].WatermarkFontSize == watermarkFontSize && cacheGraciKeyLevels[idx].WatermarkOpacity == watermarkOpacity && cacheGraciKeyLevels[idx].WatermarkHAlign == watermarkHAlign && cacheGraciKeyLevels[idx].WatermarkVAlign == watermarkVAlign && cacheGraciKeyLevels[idx].WatermarkOffsetX == watermarkOffsetX && cacheGraciKeyLevels[idx].WatermarkOffsetY == watermarkOffsetY && cacheGraciKeyLevels[idx].PriorDayCloseLineStyle == priorDayCloseLineStyle && cacheGraciKeyLevels[idx].ShowCurrentWeekHighLow == showCurrentWeekHighLow && cacheGraciKeyLevels[idx].CurrentWeekHighLineStyle == currentWeekHighLineStyle && cacheGraciKeyLevels[idx].CurrentWeekHighLabelColor == currentWeekHighLabelColor && cacheGraciKeyLevels[idx].CurrentWeekLowLineStyle == currentWeekLowLineStyle && cacheGraciKeyLevels[idx].CurrentWeekLowLabelColor == currentWeekLowLabelColor && cacheGraciKeyLevels[idx].ShowPriorWeekHighLow == showPriorWeekHighLow && cacheGraciKeyLevels[idx].PriorWeekHighLineStyle == priorWeekHighLineStyle && cacheGraciKeyLevels[idx].PriorWeekHighLabelColor == priorWeekHighLabelColor && cacheGraciKeyLevels[idx].PriorWeekLowLineStyle == priorWeekLowLineStyle && cacheGraciKeyLevels[idx].PriorWeekLowLabelColor == priorWeekLowLabelColor && cacheGraciKeyLevels[idx].ShowCurrentMonthHighLow == showCurrentMonthHighLow && cacheGraciKeyLevels[idx].CurrentMonthHighLineStyle == currentMonthHighLineStyle && cacheGraciKeyLevels[idx].CurrentMonthHighLabelColor == currentMonthHighLabelColor && cacheGraciKeyLevels[idx].CurrentMonthLowLineStyle == currentMonthLowLineStyle && cacheGraciKeyLevels[idx].CurrentMonthLowLabelColor == currentMonthLowLabelColor && cacheGraciKeyLevels[idx].ShowPriorMonthHighLow == showPriorMonthHighLow && cacheGraciKeyLevels[idx].PriorMonthHighLineStyle == priorMonthHighLineStyle && cacheGraciKeyLevels[idx].PriorMonthHighLabelColor == priorMonthHighLabelColor && cacheGraciKeyLevels[idx].PriorMonthLowLineStyle == priorMonthLowLineStyle && cacheGraciKeyLevels[idx].PriorMonthLowLabelColor == priorMonthLowLabelColor && cacheGraciKeyLevels[idx].EqualsInput(input))
						return cacheGraciKeyLevels[idx];
			return CacheIndicator<GraciKeyLevels>(new GraciKeyLevels(){ ShowPriceLine = showPriceLine, ShowFibonacciPivots = showFibonacciPivots, ShowSessionOpen = showSessionOpen, ShowCurrentDayHighLow = showCurrentDayHighLow, ShowPriorDayHighLow = showPriorDayHighLow, ShowWeeklyIB = showWeeklyIB, ShowSecondaryPivots = showSecondaryPivots, ShowNYOpen = showNYOpen, ShowLondonOpen = showLondonOpen, ShowAsiaOpen = showAsiaOpen, ShowGoldenLevels = showGoldenLevels, SecondaryPivotPeriodType = secondaryPivotPeriodType, SecondaryPivotPeriodValue = secondaryPivotPeriodValue, KeepBrokenPivots = keepBrokenPivots, BearishPivotLineStyle = bearishPivotLineStyle, BearishPivotLabelColor = bearishPivotLabelColor, BearishPivotLabelText = bearishPivotLabelText, BullishPivotLineStyle = bullishPivotLineStyle, BullishPivotLabelColor = bullishPivotLabelColor, BullishPivotLabelText = bullishPivotLabelText, MaxSecondaryPivots = maxSecondaryPivots, IBHighLineStyle = iBHighLineStyle, IBHighLabelColor = iBHighLabelColor, IBLowLineStyle = iBLowLineStyle, IBLowLabelColor = iBLowLabelColor, FillWeeklyIB = fillWeeklyIB, IBFillOpacity = iBFillOpacity, IBHighLabelHorizontalOffsetBars = iBHighLabelHorizontalOffsetBars, IBHighLabelVerticalOffset = iBHighLabelVerticalOffset, IBLowLabelHorizontalOffsetBars = iBLowLabelHorizontalOffsetBars, IBLowLabelVerticalOffset = iBLowLabelVerticalOffset, PriceLineStroke = priceLineStroke, PpStroke = ppStroke, RStroke = rStroke, SStroke = sStroke, PivotLineWidth = pivotLineWidth, PivotLabelHorizontalOffset = pivotLabelHorizontalOffset, CurrentDayHighLineStyle = currentDayHighLineStyle, CurrentDayHighLabelColor = currentDayHighLabelColor, CurrentDayLowLineStyle = currentDayLowLineStyle, CurrentDayLowLabelColor = currentDayLowLabelColor, CurrentDayHighLabelHorizontalOffsetBars = currentDayHighLabelHorizontalOffsetBars, CurrentDayHighLabelVerticalOffset = currentDayHighLabelVerticalOffset, CurrentDayLowLabelHorizontalOffsetBars = currentDayLowLabelHorizontalOffsetBars, CurrentDayLowLabelVerticalOffset = currentDayLowLabelVerticalOffset, PriorDayHighLineStyle = priorDayHighLineStyle, PriorDayHighLabelColor = priorDayHighLabelColor, PriorDayLowLineStyle = priorDayLowLineStyle, PriorDayLowLabelColor = priorDayLowLabelColor, PriorDayHighLabelHorizontalOffsetBars = priorDayHighLabelHorizontalOffsetBars, PriorDayHighLabelVerticalOffset = priorDayHighLabelVerticalOffset, PriorDayLowLabelHorizontalOffsetBars = priorDayLowLabelHorizontalOffsetBars, PriorDayLowLabelVerticalOffset = priorDayLowLabelVerticalOffset, NYOpenTime = nYOpenTime, NYOpenLineStyle = nYOpenLineStyle, NYOpenLabelColor = nYOpenLabelColor, LondonOpenTime = londonOpenTime, LondonOpenLineStyle = londonOpenLineStyle, LondonOpenLabelColor = londonOpenLabelColor, AsiaOpenTime = asiaOpenTime, AsiaOpenLineStyle = asiaOpenLineStyle, AsiaOpenLabelColor = asiaOpenLabelColor, SessionOpenLineStyle = sessionOpenLineStyle, SessionOpenLabelColor = sessionOpenLabelColor, Level00 = level00, Level00Line = level00Line, Level26 = level26, Level26Line = level26Line, Level50 = level50, Level50Line = level50Line, Level77 = level77, Level77Line = level77Line, GoldenLabelHorizontalOffset = goldenLabelHorizontalOffset, GoldenLabelVerticalOffset = goldenLabelVerticalOffset, GoldenLabelFontSize = goldenLabelFontSize, GoldenLabelFontBold = goldenLabelFontBold, GoldenLevelLabelColor = goldenLevelLabelColor, LabelHorizontalOffset = labelHorizontalOffset, LabelVerticalOffset = labelVerticalOffset, LabelFontSize = labelFontSize, LabelFontBold = labelFontBold, ShowWatermark = showWatermark, WatermarkCustomText = watermarkCustomText, WatermarkInstrumentDisplay = watermarkInstrumentDisplay, WatermarkPeriodDisplay = watermarkPeriodDisplay, WatermarkColor = watermarkColor, WatermarkFontSize = watermarkFontSize, WatermarkOpacity = watermarkOpacity, WatermarkHAlign = watermarkHAlign, WatermarkVAlign = watermarkVAlign, WatermarkOffsetX = watermarkOffsetX, WatermarkOffsetY = watermarkOffsetY, PriorDayCloseLineStyle = priorDayCloseLineStyle, ShowCurrentWeekHighLow = showCurrentWeekHighLow, CurrentWeekHighLineStyle = currentWeekHighLineStyle, CurrentWeekHighLabelColor = currentWeekHighLabelColor, CurrentWeekLowLineStyle = currentWeekLowLineStyle, CurrentWeekLowLabelColor = currentWeekLowLabelColor, ShowPriorWeekHighLow = showPriorWeekHighLow, PriorWeekHighLineStyle = priorWeekHighLineStyle, PriorWeekHighLabelColor = priorWeekHighLabelColor, PriorWeekLowLineStyle = priorWeekLowLineStyle, PriorWeekLowLabelColor = priorWeekLowLabelColor, ShowCurrentMonthHighLow = showCurrentMonthHighLow, CurrentMonthHighLineStyle = currentMonthHighLineStyle, CurrentMonthHighLabelColor = currentMonthHighLabelColor, CurrentMonthLowLineStyle = currentMonthLowLineStyle, CurrentMonthLowLabelColor = currentMonthLowLabelColor, ShowPriorMonthHighLow = showPriorMonthHighLow, PriorMonthHighLineStyle = priorMonthHighLineStyle, PriorMonthHighLabelColor = priorMonthHighLabelColor, PriorMonthLowLineStyle = priorMonthLowLineStyle, PriorMonthLowLabelColor = priorMonthLowLabelColor }, input, ref cacheGraciKeyLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GraciKeyLevels GraciKeyLevels(bool showPriceLine, bool showFibonacciPivots, bool showSessionOpen, bool showCurrentDayHighLow, bool showPriorDayHighLow, bool showWeeklyIB, bool showSecondaryPivots, bool showNYOpen, bool showLondonOpen, bool showAsiaOpen, bool showGoldenLevels, BarsPeriodType secondaryPivotPeriodType, int secondaryPivotPeriodValue, bool keepBrokenPivots, Stroke bearishPivotLineStyle, System.Windows.Media.Brush bearishPivotLabelColor, string bearishPivotLabelText, Stroke bullishPivotLineStyle, System.Windows.Media.Brush bullishPivotLabelColor, string bullishPivotLabelText, int maxSecondaryPivots, Stroke iBHighLineStyle, System.Windows.Media.Brush iBHighLabelColor, Stroke iBLowLineStyle, System.Windows.Media.Brush iBLowLabelColor, bool fillWeeklyIB, int iBFillOpacity, int iBHighLabelHorizontalOffsetBars, int iBHighLabelVerticalOffset, int iBLowLabelHorizontalOffsetBars, int iBLowLabelVerticalOffset, Stroke priceLineStroke, Stroke ppStroke, Stroke rStroke, Stroke sStroke, int pivotLineWidth, int pivotLabelHorizontalOffset, Stroke currentDayHighLineStyle, System.Windows.Media.Brush currentDayHighLabelColor, Stroke currentDayLowLineStyle, System.Windows.Media.Brush currentDayLowLabelColor, int currentDayHighLabelHorizontalOffsetBars, int currentDayHighLabelVerticalOffset, int currentDayLowLabelHorizontalOffsetBars, int currentDayLowLabelVerticalOffset, Stroke priorDayHighLineStyle, System.Windows.Media.Brush priorDayHighLabelColor, Stroke priorDayLowLineStyle, System.Windows.Media.Brush priorDayLowLabelColor, int priorDayHighLabelHorizontalOffsetBars, int priorDayHighLabelVerticalOffset, int priorDayLowLabelHorizontalOffsetBars, int priorDayLowLabelVerticalOffset, DateTime nYOpenTime, Stroke nYOpenLineStyle, System.Windows.Media.Brush nYOpenLabelColor, DateTime londonOpenTime, Stroke londonOpenLineStyle, System.Windows.Media.Brush londonOpenLabelColor, DateTime asiaOpenTime, Stroke asiaOpenLineStyle, System.Windows.Media.Brush asiaOpenLabelColor, Stroke sessionOpenLineStyle, System.Windows.Media.Brush sessionOpenLabelColor, int level00, Stroke level00Line, int level26, Stroke level26Line, int level50, Stroke level50Line, int level77, Stroke level77Line, int goldenLabelHorizontalOffset, int goldenLabelVerticalOffset, int goldenLabelFontSize, bool goldenLabelFontBold, System.Windows.Media.Brush goldenLevelLabelColor, int labelHorizontalOffset, int labelVerticalOffset, int labelFontSize, bool labelFontBold, bool showWatermark, string watermarkCustomText, string watermarkInstrumentDisplay, string watermarkPeriodDisplay, System.Windows.Media.Brush watermarkColor, int watermarkFontSize, int watermarkOpacity, string watermarkHAlign, string watermarkVAlign, int watermarkOffsetX, int watermarkOffsetY, Stroke priorDayCloseLineStyle, bool showCurrentWeekHighLow, Stroke currentWeekHighLineStyle, System.Windows.Media.Brush currentWeekHighLabelColor, Stroke currentWeekLowLineStyle, System.Windows.Media.Brush currentWeekLowLabelColor, bool showPriorWeekHighLow, Stroke priorWeekHighLineStyle, System.Windows.Media.Brush priorWeekHighLabelColor, Stroke priorWeekLowLineStyle, System.Windows.Media.Brush priorWeekLowLabelColor, bool showCurrentMonthHighLow, Stroke currentMonthHighLineStyle, System.Windows.Media.Brush currentMonthHighLabelColor, Stroke currentMonthLowLineStyle, System.Windows.Media.Brush currentMonthLowLabelColor, bool showPriorMonthHighLow, Stroke priorMonthHighLineStyle, System.Windows.Media.Brush priorMonthHighLabelColor, Stroke priorMonthLowLineStyle, System.Windows.Media.Brush priorMonthLowLabelColor)
		{
			return indicator.GraciKeyLevels(Input, showPriceLine, showFibonacciPivots, showSessionOpen, showCurrentDayHighLow, showPriorDayHighLow, showWeeklyIB, showSecondaryPivots, showNYOpen, showLondonOpen, showAsiaOpen, showGoldenLevels, secondaryPivotPeriodType, secondaryPivotPeriodValue, keepBrokenPivots, bearishPivotLineStyle, bearishPivotLabelColor, bearishPivotLabelText, bullishPivotLineStyle, bullishPivotLabelColor, bullishPivotLabelText, maxSecondaryPivots, iBHighLineStyle, iBHighLabelColor, iBLowLineStyle, iBLowLabelColor, fillWeeklyIB, iBFillOpacity, iBHighLabelHorizontalOffsetBars, iBHighLabelVerticalOffset, iBLowLabelHorizontalOffsetBars, iBLowLabelVerticalOffset, priceLineStroke, ppStroke, rStroke, sStroke, pivotLineWidth, pivotLabelHorizontalOffset, currentDayHighLineStyle, currentDayHighLabelColor, currentDayLowLineStyle, currentDayLowLabelColor, currentDayHighLabelHorizontalOffsetBars, currentDayHighLabelVerticalOffset, currentDayLowLabelHorizontalOffsetBars, currentDayLowLabelVerticalOffset, priorDayHighLineStyle, priorDayHighLabelColor, priorDayLowLineStyle, priorDayLowLabelColor, priorDayHighLabelHorizontalOffsetBars, priorDayHighLabelVerticalOffset, priorDayLowLabelHorizontalOffsetBars, priorDayLowLabelVerticalOffset, nYOpenTime, nYOpenLineStyle, nYOpenLabelColor, londonOpenTime, londonOpenLineStyle, londonOpenLabelColor, asiaOpenTime, asiaOpenLineStyle, asiaOpenLabelColor, sessionOpenLineStyle, sessionOpenLabelColor, level00, level00Line, level26, level26Line, level50, level50Line, level77, level77Line, goldenLabelHorizontalOffset, goldenLabelVerticalOffset, goldenLabelFontSize, goldenLabelFontBold, goldenLevelLabelColor, labelHorizontalOffset, labelVerticalOffset, labelFontSize, labelFontBold, showWatermark, watermarkCustomText, watermarkInstrumentDisplay, watermarkPeriodDisplay, watermarkColor, watermarkFontSize, watermarkOpacity, watermarkHAlign, watermarkVAlign, watermarkOffsetX, watermarkOffsetY, priorDayCloseLineStyle, showCurrentWeekHighLow, currentWeekHighLineStyle, currentWeekHighLabelColor, currentWeekLowLineStyle, currentWeekLowLabelColor, showPriorWeekHighLow, priorWeekHighLineStyle, priorWeekHighLabelColor, priorWeekLowLineStyle, priorWeekLowLabelColor, showCurrentMonthHighLow, currentMonthHighLineStyle, currentMonthHighLabelColor, currentMonthLowLineStyle, currentMonthLowLabelColor, showPriorMonthHighLow, priorMonthHighLineStyle, priorMonthHighLabelColor, priorMonthLowLineStyle, priorMonthLowLabelColor);
		}

		public Indicators.GraciKeyLevels GraciKeyLevels(ISeries<double> input , bool showPriceLine, bool showFibonacciPivots, bool showSessionOpen, bool showCurrentDayHighLow, bool showPriorDayHighLow, bool showWeeklyIB, bool showSecondaryPivots, bool showNYOpen, bool showLondonOpen, bool showAsiaOpen, bool showGoldenLevels, BarsPeriodType secondaryPivotPeriodType, int secondaryPivotPeriodValue, bool keepBrokenPivots, Stroke bearishPivotLineStyle, System.Windows.Media.Brush bearishPivotLabelColor, string bearishPivotLabelText, Stroke bullishPivotLineStyle, System.Windows.Media.Brush bullishPivotLabelColor, string bullishPivotLabelText, int maxSecondaryPivots, Stroke iBHighLineStyle, System.Windows.Media.Brush iBHighLabelColor, Stroke iBLowLineStyle, System.Windows.Media.Brush iBLowLabelColor, bool fillWeeklyIB, int iBFillOpacity, int iBHighLabelHorizontalOffsetBars, int iBHighLabelVerticalOffset, int iBLowLabelHorizontalOffsetBars, int iBLowLabelVerticalOffset, Stroke priceLineStroke, Stroke ppStroke, Stroke rStroke, Stroke sStroke, int pivotLineWidth, int pivotLabelHorizontalOffset, Stroke currentDayHighLineStyle, System.Windows.Media.Brush currentDayHighLabelColor, Stroke currentDayLowLineStyle, System.Windows.Media.Brush currentDayLowLabelColor, int currentDayHighLabelHorizontalOffsetBars, int currentDayHighLabelVerticalOffset, int currentDayLowLabelHorizontalOffsetBars, int currentDayLowLabelVerticalOffset, Stroke priorDayHighLineStyle, System.Windows.Media.Brush priorDayHighLabelColor, Stroke priorDayLowLineStyle, System.Windows.Media.Brush priorDayLowLabelColor, int priorDayHighLabelHorizontalOffsetBars, int priorDayHighLabelVerticalOffset, int priorDayLowLabelHorizontalOffsetBars, int priorDayLowLabelVerticalOffset, DateTime nYOpenTime, Stroke nYOpenLineStyle, System.Windows.Media.Brush nYOpenLabelColor, DateTime londonOpenTime, Stroke londonOpenLineStyle, System.Windows.Media.Brush londonOpenLabelColor, DateTime asiaOpenTime, Stroke asiaOpenLineStyle, System.Windows.Media.Brush asiaOpenLabelColor, Stroke sessionOpenLineStyle, System.Windows.Media.Brush sessionOpenLabelColor, int level00, Stroke level00Line, int level26, Stroke level26Line, int level50, Stroke level50Line, int level77, Stroke level77Line, int goldenLabelHorizontalOffset, int goldenLabelVerticalOffset, int goldenLabelFontSize, bool goldenLabelFontBold, System.Windows.Media.Brush goldenLevelLabelColor, int labelHorizontalOffset, int labelVerticalOffset, int labelFontSize, bool labelFontBold, bool showWatermark, string watermarkCustomText, string watermarkInstrumentDisplay, string watermarkPeriodDisplay, System.Windows.Media.Brush watermarkColor, int watermarkFontSize, int watermarkOpacity, string watermarkHAlign, string watermarkVAlign, int watermarkOffsetX, int watermarkOffsetY, Stroke priorDayCloseLineStyle, bool showCurrentWeekHighLow, Stroke currentWeekHighLineStyle, System.Windows.Media.Brush currentWeekHighLabelColor, Stroke currentWeekLowLineStyle, System.Windows.Media.Brush currentWeekLowLabelColor, bool showPriorWeekHighLow, Stroke priorWeekHighLineStyle, System.Windows.Media.Brush priorWeekHighLabelColor, Stroke priorWeekLowLineStyle, System.Windows.Media.Brush priorWeekLowLabelColor, bool showCurrentMonthHighLow, Stroke currentMonthHighLineStyle, System.Windows.Media.Brush currentMonthHighLabelColor, Stroke currentMonthLowLineStyle, System.Windows.Media.Brush currentMonthLowLabelColor, bool showPriorMonthHighLow, Stroke priorMonthHighLineStyle, System.Windows.Media.Brush priorMonthHighLabelColor, Stroke priorMonthLowLineStyle, System.Windows.Media.Brush priorMonthLowLabelColor)
		{
			return indicator.GraciKeyLevels(input, showPriceLine, showFibonacciPivots, showSessionOpen, showCurrentDayHighLow, showPriorDayHighLow, showWeeklyIB, showSecondaryPivots, showNYOpen, showLondonOpen, showAsiaOpen, showGoldenLevels, secondaryPivotPeriodType, secondaryPivotPeriodValue, keepBrokenPivots, bearishPivotLineStyle, bearishPivotLabelColor, bearishPivotLabelText, bullishPivotLineStyle, bullishPivotLabelColor, bullishPivotLabelText, maxSecondaryPivots, iBHighLineStyle, iBHighLabelColor, iBLowLineStyle, iBLowLabelColor, fillWeeklyIB, iBFillOpacity, iBHighLabelHorizontalOffsetBars, iBHighLabelVerticalOffset, iBLowLabelHorizontalOffsetBars, iBLowLabelVerticalOffset, priceLineStroke, ppStroke, rStroke, sStroke, pivotLineWidth, pivotLabelHorizontalOffset, currentDayHighLineStyle, currentDayHighLabelColor, currentDayLowLineStyle, currentDayLowLabelColor, currentDayHighLabelHorizontalOffsetBars, currentDayHighLabelVerticalOffset, currentDayLowLabelHorizontalOffsetBars, currentDayLowLabelVerticalOffset, priorDayHighLineStyle, priorDayHighLabelColor, priorDayLowLineStyle, priorDayLowLabelColor, priorDayHighLabelHorizontalOffsetBars, priorDayHighLabelVerticalOffset, priorDayLowLabelHorizontalOffsetBars, priorDayLowLabelVerticalOffset, nYOpenTime, nYOpenLineStyle, nYOpenLabelColor, londonOpenTime, londonOpenLineStyle, londonOpenLabelColor, asiaOpenTime, asiaOpenLineStyle, asiaOpenLabelColor, sessionOpenLineStyle, sessionOpenLabelColor, level00, level00Line, level26, level26Line, level50, level50Line, level77, level77Line, goldenLabelHorizontalOffset, goldenLabelVerticalOffset, goldenLabelFontSize, goldenLabelFontBold, goldenLevelLabelColor, labelHorizontalOffset, labelVerticalOffset, labelFontSize, labelFontBold, showWatermark, watermarkCustomText, watermarkInstrumentDisplay, watermarkPeriodDisplay, watermarkColor, watermarkFontSize, watermarkOpacity, watermarkHAlign, watermarkVAlign, watermarkOffsetX, watermarkOffsetY, priorDayCloseLineStyle, showCurrentWeekHighLow, currentWeekHighLineStyle, currentWeekHighLabelColor, currentWeekLowLineStyle, currentWeekLowLabelColor, showPriorWeekHighLow, priorWeekHighLineStyle, priorWeekHighLabelColor, priorWeekLowLineStyle, priorWeekLowLabelColor, showCurrentMonthHighLow, currentMonthHighLineStyle, currentMonthHighLabelColor, currentMonthLowLineStyle, currentMonthLowLabelColor, showPriorMonthHighLow, priorMonthHighLineStyle, priorMonthHighLabelColor, priorMonthLowLineStyle, priorMonthLowLabelColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GraciKeyLevels GraciKeyLevels(bool showPriceLine, bool showFibonacciPivots, bool showSessionOpen, bool showCurrentDayHighLow, bool showPriorDayHighLow, bool showWeeklyIB, bool showSecondaryPivots, bool showNYOpen, bool showLondonOpen, bool showAsiaOpen, bool showGoldenLevels, BarsPeriodType secondaryPivotPeriodType, int secondaryPivotPeriodValue, bool keepBrokenPivots, Stroke bearishPivotLineStyle, System.Windows.Media.Brush bearishPivotLabelColor, string bearishPivotLabelText, Stroke bullishPivotLineStyle, System.Windows.Media.Brush bullishPivotLabelColor, string bullishPivotLabelText, int maxSecondaryPivots, Stroke iBHighLineStyle, System.Windows.Media.Brush iBHighLabelColor, Stroke iBLowLineStyle, System.Windows.Media.Brush iBLowLabelColor, bool fillWeeklyIB, int iBFillOpacity, int iBHighLabelHorizontalOffsetBars, int iBHighLabelVerticalOffset, int iBLowLabelHorizontalOffsetBars, int iBLowLabelVerticalOffset, Stroke priceLineStroke, Stroke ppStroke, Stroke rStroke, Stroke sStroke, int pivotLineWidth, int pivotLabelHorizontalOffset, Stroke currentDayHighLineStyle, System.Windows.Media.Brush currentDayHighLabelColor, Stroke currentDayLowLineStyle, System.Windows.Media.Brush currentDayLowLabelColor, int currentDayHighLabelHorizontalOffsetBars, int currentDayHighLabelVerticalOffset, int currentDayLowLabelHorizontalOffsetBars, int currentDayLowLabelVerticalOffset, Stroke priorDayHighLineStyle, System.Windows.Media.Brush priorDayHighLabelColor, Stroke priorDayLowLineStyle, System.Windows.Media.Brush priorDayLowLabelColor, int priorDayHighLabelHorizontalOffsetBars, int priorDayHighLabelVerticalOffset, int priorDayLowLabelHorizontalOffsetBars, int priorDayLowLabelVerticalOffset, DateTime nYOpenTime, Stroke nYOpenLineStyle, System.Windows.Media.Brush nYOpenLabelColor, DateTime londonOpenTime, Stroke londonOpenLineStyle, System.Windows.Media.Brush londonOpenLabelColor, DateTime asiaOpenTime, Stroke asiaOpenLineStyle, System.Windows.Media.Brush asiaOpenLabelColor, Stroke sessionOpenLineStyle, System.Windows.Media.Brush sessionOpenLabelColor, int level00, Stroke level00Line, int level26, Stroke level26Line, int level50, Stroke level50Line, int level77, Stroke level77Line, int goldenLabelHorizontalOffset, int goldenLabelVerticalOffset, int goldenLabelFontSize, bool goldenLabelFontBold, System.Windows.Media.Brush goldenLevelLabelColor, int labelHorizontalOffset, int labelVerticalOffset, int labelFontSize, bool labelFontBold, bool showWatermark, string watermarkCustomText, string watermarkInstrumentDisplay, string watermarkPeriodDisplay, System.Windows.Media.Brush watermarkColor, int watermarkFontSize, int watermarkOpacity, string watermarkHAlign, string watermarkVAlign, int watermarkOffsetX, int watermarkOffsetY, Stroke priorDayCloseLineStyle, bool showCurrentWeekHighLow, Stroke currentWeekHighLineStyle, System.Windows.Media.Brush currentWeekHighLabelColor, Stroke currentWeekLowLineStyle, System.Windows.Media.Brush currentWeekLowLabelColor, bool showPriorWeekHighLow, Stroke priorWeekHighLineStyle, System.Windows.Media.Brush priorWeekHighLabelColor, Stroke priorWeekLowLineStyle, System.Windows.Media.Brush priorWeekLowLabelColor, bool showCurrentMonthHighLow, Stroke currentMonthHighLineStyle, System.Windows.Media.Brush currentMonthHighLabelColor, Stroke currentMonthLowLineStyle, System.Windows.Media.Brush currentMonthLowLabelColor, bool showPriorMonthHighLow, Stroke priorMonthHighLineStyle, System.Windows.Media.Brush priorMonthHighLabelColor, Stroke priorMonthLowLineStyle, System.Windows.Media.Brush priorMonthLowLabelColor)
		{
			return indicator.GraciKeyLevels(Input, showPriceLine, showFibonacciPivots, showSessionOpen, showCurrentDayHighLow, showPriorDayHighLow, showWeeklyIB, showSecondaryPivots, showNYOpen, showLondonOpen, showAsiaOpen, showGoldenLevels, secondaryPivotPeriodType, secondaryPivotPeriodValue, keepBrokenPivots, bearishPivotLineStyle, bearishPivotLabelColor, bearishPivotLabelText, bullishPivotLineStyle, bullishPivotLabelColor, bullishPivotLabelText, maxSecondaryPivots, iBHighLineStyle, iBHighLabelColor, iBLowLineStyle, iBLowLabelColor, fillWeeklyIB, iBFillOpacity, iBHighLabelHorizontalOffsetBars, iBHighLabelVerticalOffset, iBLowLabelHorizontalOffsetBars, iBLowLabelVerticalOffset, priceLineStroke, ppStroke, rStroke, sStroke, pivotLineWidth, pivotLabelHorizontalOffset, currentDayHighLineStyle, currentDayHighLabelColor, currentDayLowLineStyle, currentDayLowLabelColor, currentDayHighLabelHorizontalOffsetBars, currentDayHighLabelVerticalOffset, currentDayLowLabelHorizontalOffsetBars, currentDayLowLabelVerticalOffset, priorDayHighLineStyle, priorDayHighLabelColor, priorDayLowLineStyle, priorDayLowLabelColor, priorDayHighLabelHorizontalOffsetBars, priorDayHighLabelVerticalOffset, priorDayLowLabelHorizontalOffsetBars, priorDayLowLabelVerticalOffset, nYOpenTime, nYOpenLineStyle, nYOpenLabelColor, londonOpenTime, londonOpenLineStyle, londonOpenLabelColor, asiaOpenTime, asiaOpenLineStyle, asiaOpenLabelColor, sessionOpenLineStyle, sessionOpenLabelColor, level00, level00Line, level26, level26Line, level50, level50Line, level77, level77Line, goldenLabelHorizontalOffset, goldenLabelVerticalOffset, goldenLabelFontSize, goldenLabelFontBold, goldenLevelLabelColor, labelHorizontalOffset, labelVerticalOffset, labelFontSize, labelFontBold, showWatermark, watermarkCustomText, watermarkInstrumentDisplay, watermarkPeriodDisplay, watermarkColor, watermarkFontSize, watermarkOpacity, watermarkHAlign, watermarkVAlign, watermarkOffsetX, watermarkOffsetY, priorDayCloseLineStyle, showCurrentWeekHighLow, currentWeekHighLineStyle, currentWeekHighLabelColor, currentWeekLowLineStyle, currentWeekLowLabelColor, showPriorWeekHighLow, priorWeekHighLineStyle, priorWeekHighLabelColor, priorWeekLowLineStyle, priorWeekLowLabelColor, showCurrentMonthHighLow, currentMonthHighLineStyle, currentMonthHighLabelColor, currentMonthLowLineStyle, currentMonthLowLabelColor, showPriorMonthHighLow, priorMonthHighLineStyle, priorMonthHighLabelColor, priorMonthLowLineStyle, priorMonthLowLabelColor);
		}

		public Indicators.GraciKeyLevels GraciKeyLevels(ISeries<double> input , bool showPriceLine, bool showFibonacciPivots, bool showSessionOpen, bool showCurrentDayHighLow, bool showPriorDayHighLow, bool showWeeklyIB, bool showSecondaryPivots, bool showNYOpen, bool showLondonOpen, bool showAsiaOpen, bool showGoldenLevels, BarsPeriodType secondaryPivotPeriodType, int secondaryPivotPeriodValue, bool keepBrokenPivots, Stroke bearishPivotLineStyle, System.Windows.Media.Brush bearishPivotLabelColor, string bearishPivotLabelText, Stroke bullishPivotLineStyle, System.Windows.Media.Brush bullishPivotLabelColor, string bullishPivotLabelText, int maxSecondaryPivots, Stroke iBHighLineStyle, System.Windows.Media.Brush iBHighLabelColor, Stroke iBLowLineStyle, System.Windows.Media.Brush iBLowLabelColor, bool fillWeeklyIB, int iBFillOpacity, int iBHighLabelHorizontalOffsetBars, int iBHighLabelVerticalOffset, int iBLowLabelHorizontalOffsetBars, int iBLowLabelVerticalOffset, Stroke priceLineStroke, Stroke ppStroke, Stroke rStroke, Stroke sStroke, int pivotLineWidth, int pivotLabelHorizontalOffset, Stroke currentDayHighLineStyle, System.Windows.Media.Brush currentDayHighLabelColor, Stroke currentDayLowLineStyle, System.Windows.Media.Brush currentDayLowLabelColor, int currentDayHighLabelHorizontalOffsetBars, int currentDayHighLabelVerticalOffset, int currentDayLowLabelHorizontalOffsetBars, int currentDayLowLabelVerticalOffset, Stroke priorDayHighLineStyle, System.Windows.Media.Brush priorDayHighLabelColor, Stroke priorDayLowLineStyle, System.Windows.Media.Brush priorDayLowLabelColor, int priorDayHighLabelHorizontalOffsetBars, int priorDayHighLabelVerticalOffset, int priorDayLowLabelHorizontalOffsetBars, int priorDayLowLabelVerticalOffset, DateTime nYOpenTime, Stroke nYOpenLineStyle, System.Windows.Media.Brush nYOpenLabelColor, DateTime londonOpenTime, Stroke londonOpenLineStyle, System.Windows.Media.Brush londonOpenLabelColor, DateTime asiaOpenTime, Stroke asiaOpenLineStyle, System.Windows.Media.Brush asiaOpenLabelColor, Stroke sessionOpenLineStyle, System.Windows.Media.Brush sessionOpenLabelColor, int level00, Stroke level00Line, int level26, Stroke level26Line, int level50, Stroke level50Line, int level77, Stroke level77Line, int goldenLabelHorizontalOffset, int goldenLabelVerticalOffset, int goldenLabelFontSize, bool goldenLabelFontBold, System.Windows.Media.Brush goldenLevelLabelColor, int labelHorizontalOffset, int labelVerticalOffset, int labelFontSize, bool labelFontBold, bool showWatermark, string watermarkCustomText, string watermarkInstrumentDisplay, string watermarkPeriodDisplay, System.Windows.Media.Brush watermarkColor, int watermarkFontSize, int watermarkOpacity, string watermarkHAlign, string watermarkVAlign, int watermarkOffsetX, int watermarkOffsetY, Stroke priorDayCloseLineStyle, bool showCurrentWeekHighLow, Stroke currentWeekHighLineStyle, System.Windows.Media.Brush currentWeekHighLabelColor, Stroke currentWeekLowLineStyle, System.Windows.Media.Brush currentWeekLowLabelColor, bool showPriorWeekHighLow, Stroke priorWeekHighLineStyle, System.Windows.Media.Brush priorWeekHighLabelColor, Stroke priorWeekLowLineStyle, System.Windows.Media.Brush priorWeekLowLabelColor, bool showCurrentMonthHighLow, Stroke currentMonthHighLineStyle, System.Windows.Media.Brush currentMonthHighLabelColor, Stroke currentMonthLowLineStyle, System.Windows.Media.Brush currentMonthLowLabelColor, bool showPriorMonthHighLow, Stroke priorMonthHighLineStyle, System.Windows.Media.Brush priorMonthHighLabelColor, Stroke priorMonthLowLineStyle, System.Windows.Media.Brush priorMonthLowLabelColor)
		{
			return indicator.GraciKeyLevels(input, showPriceLine, showFibonacciPivots, showSessionOpen, showCurrentDayHighLow, showPriorDayHighLow, showWeeklyIB, showSecondaryPivots, showNYOpen, showLondonOpen, showAsiaOpen, showGoldenLevels, secondaryPivotPeriodType, secondaryPivotPeriodValue, keepBrokenPivots, bearishPivotLineStyle, bearishPivotLabelColor, bearishPivotLabelText, bullishPivotLineStyle, bullishPivotLabelColor, bullishPivotLabelText, maxSecondaryPivots, iBHighLineStyle, iBHighLabelColor, iBLowLineStyle, iBLowLabelColor, fillWeeklyIB, iBFillOpacity, iBHighLabelHorizontalOffsetBars, iBHighLabelVerticalOffset, iBLowLabelHorizontalOffsetBars, iBLowLabelVerticalOffset, priceLineStroke, ppStroke, rStroke, sStroke, pivotLineWidth, pivotLabelHorizontalOffset, currentDayHighLineStyle, currentDayHighLabelColor, currentDayLowLineStyle, currentDayLowLabelColor, currentDayHighLabelHorizontalOffsetBars, currentDayHighLabelVerticalOffset, currentDayLowLabelHorizontalOffsetBars, currentDayLowLabelVerticalOffset, priorDayHighLineStyle, priorDayHighLabelColor, priorDayLowLineStyle, priorDayLowLabelColor, priorDayHighLabelHorizontalOffsetBars, priorDayHighLabelVerticalOffset, priorDayLowLabelHorizontalOffsetBars, priorDayLowLabelVerticalOffset, nYOpenTime, nYOpenLineStyle, nYOpenLabelColor, londonOpenTime, londonOpenLineStyle, londonOpenLabelColor, asiaOpenTime, asiaOpenLineStyle, asiaOpenLabelColor, sessionOpenLineStyle, sessionOpenLabelColor, level00, level00Line, level26, level26Line, level50, level50Line, level77, level77Line, goldenLabelHorizontalOffset, goldenLabelVerticalOffset, goldenLabelFontSize, goldenLabelFontBold, goldenLevelLabelColor, labelHorizontalOffset, labelVerticalOffset, labelFontSize, labelFontBold, showWatermark, watermarkCustomText, watermarkInstrumentDisplay, watermarkPeriodDisplay, watermarkColor, watermarkFontSize, watermarkOpacity, watermarkHAlign, watermarkVAlign, watermarkOffsetX, watermarkOffsetY, priorDayCloseLineStyle, showCurrentWeekHighLow, currentWeekHighLineStyle, currentWeekHighLabelColor, currentWeekLowLineStyle, currentWeekLowLabelColor, showPriorWeekHighLow, priorWeekHighLineStyle, priorWeekHighLabelColor, priorWeekLowLineStyle, priorWeekLowLabelColor, showCurrentMonthHighLow, currentMonthHighLineStyle, currentMonthHighLabelColor, currentMonthLowLineStyle, currentMonthLowLabelColor, showPriorMonthHighLow, priorMonthHighLineStyle, priorMonthHighLabelColor, priorMonthLowLineStyle, priorMonthLowLabelColor);
		}
	}
}

#endregion
