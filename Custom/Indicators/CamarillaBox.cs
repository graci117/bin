//
// Copyright (C) 2024, NinjaTrader LLC .
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//

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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
// **REMOVED: using SharpDX; - This was causing the Point ambiguity**
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.CamarillaBoxTypeConverter")]
    public class CamarillaBox : Indicator
    {
        private DateTime cacheMonthlyEndDate = Globals.MinDate;
        private DateTime cacheSessionDate = Globals.MinDate;
        private DateTime cacheSessionEnd = Globals.MinDate;
        private DateTime cacheTime;
        private DateTime cacheWeeklyEndDate = Globals.MinDate;
        private DateTime currentDate = Globals.MinDate;
        private DateTime currentMonth = Globals.MinDate;
        private DateTime currentWeek = Globals.MinDate;
        private DateTime sessionDateTmp = Globals.MinDate;
        private HLCCalculationMode priorDayHlc;
        private PivotRange pivotRangeType;
        private SessionIterator storedSession;
        private double currentClose;
        private double currentHigh = double.MinValue;
        private double currentLow = double.MaxValue;
        private double dailyBarClose = double.MinValue;
        private double dailyBarHigh = double.MinValue;
        private double dailyBarLow = double.MinValue;
        private double r1, r2, r3, r4, r5, r6;
        private double s1, s2, s3, s4, s5, s6;
        private double dp;
        private double userDefinedClose;
        private double userDefinedHigh;
        private double userDefinedLow;
        private int cacheBar;
        private int width = 20;
        private readonly List<int> newSessionBarIdxArr = new List<int>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Camarilla Pivots with upper and lower lines drawn from OnRender";
                Name = "CamarillaBox";
                Calculate = Calculate.OnBarClose;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                IsAutoScale = false;
                IsOverlay = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;

                AddPlot(Brushes.Gray, NinjaTrader.Custom.Resource.PivotsR1);
                AddPlot(Brushes.Gray, NinjaTrader.Custom.Resource.PivotsR2);
                AddPlot(Brushes.DarkRed, NinjaTrader.Custom.Resource.PivotsR3);
                AddPlot(Brushes.DarkGreen, NinjaTrader.Custom.Resource.PivotsR4);
                AddPlot(Brushes.Gray, "R5");
                AddPlot(Brushes.Gray, "R6");
                AddPlot(Brushes.Gray, NinjaTrader.Custom.Resource.PivotsS1);
                AddPlot(Brushes.Gray, NinjaTrader.Custom.Resource.PivotsS2);
                AddPlot(Brushes.DarkGreen, NinjaTrader.Custom.Resource.PivotsS3);
                AddPlot(Brushes.DarkRed, NinjaTrader.Custom.Resource.PivotsS4);
                AddPlot(Brushes.Gray, "S5");
                AddPlot(Brushes.Gray, "S6");
                AddPlot(Brushes.Black, "DP");
            }
            else if (State == State.Configure)
            {
                if (priorDayHlc == HLCCalculationMode.DailyBars)
                    AddDataSeries(BarsPeriodType.Day, 1);
            }
            else if (State == State.DataLoaded)
            {
                storedSession = new SessionIterator(Bars);
            }
            else if (State == State.Historical)
            {
                if (priorDayHlc == HLCCalculationMode.DailyBars && BarsArray[1].DayCount <= 0)
                {
                    Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsDailyDataError, TextPosition.BottomRight);
                    Log(NinjaTrader.Custom.Resource.PiviotsDailyDataError, LogLevel.Error);
                    return;
                }

                if (!Bars.BarsType.IsIntraday && BarsPeriod.BarsPeriodType != BarsPeriodType.Day && (BarsPeriod.BarsPeriodType != BarsPeriodType.HeikenAshi && BarsPeriod.BarsPeriodType != BarsPeriodType.Volumetric || BarsPeriod.BaseBarsPeriodType != BarsPeriodType.Day))
                {
                    Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsDailyBarsError, TextPosition.BottomRight);
                    Log(NinjaTrader.Custom.Resource.PiviotsDailyBarsError, LogLevel.Error);
                }

                if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && pivotRangeType == PivotRange.Daily)
                {
                    Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsWeeklyBarsError, TextPosition.BottomRight);
                    Log(NinjaTrader.Custom.Resource.PiviotsWeeklyBarsError, LogLevel.Error);
                }

                if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && BarsPeriod.Value > 1)
                {
                    Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsPeriodTypeError, TextPosition.BottomRight);
                    Log(NinjaTrader.Custom.Resource.PiviotsPeriodTypeError, LogLevel.Error);
                }

                if ((priorDayHlc == HLCCalculationMode.DailyBars &&
                    (pivotRangeType == PivotRange.Monthly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddMonths(-1)
                    || pivotRangeType == PivotRange.Weekly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-7)
                    || pivotRangeType == PivotRange.Daily && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-1)))
                    || pivotRangeType == PivotRange.Monthly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddMonths(-1)
                    || pivotRangeType == PivotRange.Weekly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-7)
                    || pivotRangeType == PivotRange.Daily && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-1))
                {
                    Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsInsufficentDataError, TextPosition.BottomRight);
                    Log(NinjaTrader.Custom.Resource.PiviotsInsufficentDataError, LogLevel.Error);
                }
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
                return;

            if ((priorDayHlc == HLCCalculationMode.DailyBars && BarsArray[1].DayCount <= 0)
                || (!Bars.BarsType.IsIntraday && BarsPeriod.BarsPeriodType != BarsPeriodType.Day && (BarsPeriod.BarsPeriodType != BarsPeriodType.HeikenAshi && BarsPeriod.BarsPeriodType != BarsPeriodType.Volumetric || BarsPeriod.BaseBarsPeriodType != BarsPeriodType.Day))
                || ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && pivotRangeType == PivotRange.Daily)
                || ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && BarsPeriod.Value > 1)
                || ((priorDayHlc == HLCCalculationMode.DailyBars && (pivotRangeType == PivotRange.Monthly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddMonths(-1)
                || pivotRangeType == PivotRange.Weekly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-7)
                || pivotRangeType == PivotRange.Daily && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-1)))
                || pivotRangeType == PivotRange.Monthly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddMonths(-1)
                || pivotRangeType == PivotRange.Weekly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-7)
                || pivotRangeType == PivotRange.Daily && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-1)))
                return;

            RemoveDrawObject("NinjaScriptInfo");

            if (PriorDayHlc == HLCCalculationMode.DailyBars && CurrentBars[1] >= 0)
            {
                if (cacheTime != Times[0][0])
                {
                    cacheTime = Times[0][0];
                    cacheBar = BarsArray[1].GetBar(Times[0][0]);
                }

                dailyBarHigh = BarsArray[1].GetHigh(cacheBar);
                dailyBarLow = BarsArray[1].GetLow(cacheBar);
                dailyBarClose = BarsArray[1].GetClose(cacheBar);
            }
            else
            {
                dailyBarHigh = double.MinValue;
                dailyBarLow = double.MinValue;
                dailyBarClose = double.MinValue;
            }

            double high = (dailyBarHigh == double.MinValue) ? Highs[0][0] : dailyBarHigh;
            double low = (dailyBarLow == double.MinValue) ? Lows[0][0] : dailyBarLow;
            double close = (dailyBarClose == double.MinValue) ? Closes[0][0] : dailyBarClose;

            DateTime lastBarTimeStamp = GetLastBarSessionDate(Times[0][0], pivotRangeType);

            if ((currentDate != Globals.MinDate && pivotRangeType == PivotRange.Daily && lastBarTimeStamp != currentDate)
                || (currentWeek != Globals.MinDate && pivotRangeType == PivotRange.Weekly && lastBarTimeStamp != currentWeek)
                || (currentMonth != Globals.MinDate && pivotRangeType == PivotRange.Monthly && lastBarTimeStamp != currentMonth))
            {
                s1 = currentClose - (currentHigh - currentLow) * 1.1 / 12;
                r1 = currentClose + (currentHigh - currentLow) * 1.1 / 12;
                s2 = currentClose - (currentHigh - currentLow) * 1.1 / 6;
                r2 = currentClose + (currentHigh - currentLow) * 1.1 / 6;
                s3 = currentClose - (currentHigh - currentLow) * 1.1 / 4;
                r3 = currentClose + (currentHigh - currentLow) * 1.1 / 4;
                s4 = currentClose - (currentHigh - currentLow) * 1.1 / 2;
                r4 = currentClose + (currentHigh - currentLow) * 1.1 / 2;
                s5 = s4 - 1.168 * (s3 - s4);
                r5 = r4 + 1.168 * (r4 - r3);
                r6 = (currentHigh / currentLow) * currentClose;
                s6 = currentClose - (r6 - currentClose);
                dp = (currentHigh + currentLow + currentClose) / 3;

                currentClose = (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedClose : close;
                currentHigh = (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedHigh : high;
                currentLow = (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedLow : low;
            }
            else
            {
                currentClose = (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedClose : close;
                currentHigh = (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedHigh : Math.Max(currentHigh, high);
                currentLow = (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedLow : Math.Min(currentLow, low);
            }

            if (pivotRangeType == PivotRange.Daily)
                currentDate = lastBarTimeStamp;
            if (pivotRangeType == PivotRange.Weekly)
                currentWeek = lastBarTimeStamp;
            if (pivotRangeType == PivotRange.Monthly)
                currentMonth = lastBarTimeStamp;

            if ((pivotRangeType == PivotRange.Daily && currentDate != Globals.MinDate)
                || (pivotRangeType == PivotRange.Weekly && currentWeek != Globals.MinDate)
                || (pivotRangeType == PivotRange.Monthly && currentMonth != Globals.MinDate))
            {
                R1[0] = r1;
                R2[0] = r2;
                R3[0] = r3;
                R4[0] = r4;
                R5[0] = r5;
                R6[0] = r6;
                S1[0] = s1;
                S2[0] = s2;
                S3[0] = s3;
                S4[0] = s4;
                S5[0] = s5;
                S6[0] = s6;
                DP[0] = dp;
            }
        }

        #region Misc
        private DateTime GetLastBarSessionDate(DateTime time, PivotRange pivotRange)
        {
            if (time > cacheSessionEnd)
            {
                if (Bars.BarsType.IsIntraday)
                {
                    storedSession.GetNextSession(time, true);
                    cacheSessionEnd = storedSession.ActualSessionEnd;
                    sessionDateTmp = TimeZoneInfo.ConvertTime(cacheSessionEnd.AddSeconds(-1), Globals.GeneralOptions.TimeZoneInfo, Bars.TradingHours.TimeZoneInfo).Date;
                }
                else
                    sessionDateTmp = time.Date;
            }

            if (pivotRange == PivotRange.Daily)
            {
                if (sessionDateTmp != cacheSessionDate)
                {
                    if (newSessionBarIdxArr.Count == 0 || newSessionBarIdxArr.Count > 0 && CurrentBar > newSessionBarIdxArr[newSessionBarIdxArr.Count - 1])
                        newSessionBarIdxArr.Add(CurrentBar);
                    cacheSessionDate = sessionDateTmp;
                }
                return sessionDateTmp;
            }

            DateTime tmpWeeklyEndDate = RoundUpTimeToPeriodTime(sessionDateTmp, PivotRange.Weekly);
            if (pivotRange == PivotRange.Weekly)
            {
                if (tmpWeeklyEndDate != cacheWeeklyEndDate)
                {
                    if (newSessionBarIdxArr.Count == 0 || newSessionBarIdxArr.Count > 0 && CurrentBar > newSessionBarIdxArr[newSessionBarIdxArr.Count - 1])
                        newSessionBarIdxArr.Add(CurrentBar);
                    cacheWeeklyEndDate = tmpWeeklyEndDate;
                }
                return tmpWeeklyEndDate;
            }

            DateTime tmpMonthlyEndDate = RoundUpTimeToPeriodTime(sessionDateTmp, PivotRange.Monthly);
            if (tmpMonthlyEndDate != cacheMonthlyEndDate)
            {
                if (newSessionBarIdxArr.Count == 0 || newSessionBarIdxArr.Count > 0 && CurrentBar > newSessionBarIdxArr[newSessionBarIdxArr.Count - 1])
                    newSessionBarIdxArr.Add(CurrentBar);
                cacheMonthlyEndDate = tmpMonthlyEndDate;
            }
            return tmpMonthlyEndDate;
        }

        private DateTime RoundUpTimeToPeriodTime(DateTime time, PivotRange pivotRange)
        {
            if (pivotRange == PivotRange.Weekly)
                return Gui.Tools.Extensions.GetEndOfWeekTime(time);
            if (pivotRange == PivotRange.Monthly)
                return Gui.Tools.Extensions.GetEndOfMonthTime(time);
            return time;
        }

		//3 lines version without labels
//        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
//{
//    // **FIXED: Simplified OnRender with single text label per line**
//    TextFormat textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();

//    for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
//    {
//        double y = -1;
//        double yUpper = -1;
//        double yLower = -1;
//        double startX = -1;
//        double endX = -1;
//        int firstBarIdxToPaint = -1;
//        int firstBarPainted = ChartBars.FromIndex;
//        int lastBarPainted = ChartBars.ToIndex;
//        Plot plot = Plots[seriesCount];
//        bool textDrawn = false; // **NEW: Flag to ensure text is drawn only once**

//        for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
//        {
//            int prevSessionBreakIdx = newSessionBarIdxArr[i];
//            if (prevSessionBreakIdx <= lastBarPainted)
//            {
//                firstBarIdxToPaint = prevSessionBreakIdx;
//                break;
//            }
//        }

//        for (int idx = lastBarPainted; idx >= Math.Max(firstBarPainted, lastBarPainted - width); idx--)
//        {
//            if (idx < firstBarIdxToPaint)
//                break;

//            startX = chartControl.GetXByBarIndex(ChartBars, idx);
//            endX = chartControl.GetXByBarIndex(ChartBars, lastBarPainted);
//            double val = Values[seriesCount].GetValueAt(idx);
//            y = chartScale.GetYByValue(val);

//            // Calculate Y coordinates for upper and lower lines
//            yUpper = chartScale.GetYByValue(val + 1);  // 1 point above
//            yLower = chartScale.GetYByValue(val - 1);  // 1 point below

//            // **FIXED: Use System.Windows.Point explicitly to avoid ambiguity**
//            System.Windows.Point startPoint = new System.Windows.Point(startX, y);
//            System.Windows.Point endPoint = new System.Windows.Point(endX, y);
            
//            // **Draw main pivot line (middle line with width 2)**
//            RenderTarget.DrawLine(startPoint.ToVector2(), endPoint.ToVector2(), plot.BrushDX, 2, plot.StrokeStyle);

//            // **Draw upper line (solid, thinner)**
//            System.Windows.Point upperStartPoint = new System.Windows.Point(startX, yUpper);
//            System.Windows.Point upperEndPoint = new System.Windows.Point(endX, yUpper);
//            RenderTarget.DrawLine(upperStartPoint.ToVector2(), upperEndPoint.ToVector2(), plot.BrushDX, 1, null);

//            // **Draw lower line (solid, thinner)**
//            System.Windows.Point lowerStartPoint = new System.Windows.Point(startX, yLower);
//            System.Windows.Point lowerEndPoint = new System.Windows.Point(endX, yLower);
//            RenderTarget.DrawLine(lowerStartPoint.ToVector2(), lowerEndPoint.ToVector2(), plot.BrushDX, 1, null);

//            // **KEY FIX: Draw text label only once at the start of the line**
//            if (!textDrawn)
//            {
//                TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, plot.Name, textFormat, ChartPanel.W, textFormat.FontSize);
//                RenderTarget.DrawTextLayout(startPoint.ToVector2(), textLayout, plot.BrushDX);
//                textLayout.Dispose();
//                textDrawn = true; // **Set flag to prevent drawing text again**
//            }
//        }
//    }
//    textFormat.Dispose();
//}
		
//		gradient rectangles
//		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
//{
//    // **FIXED: Proper brush to color conversion for rectangles**
//    TextFormat textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();

//    for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
//    {
//        double y = -1;
//        double yUpper = -1;
//        double yLower = -1;
//        double startX = -1;
//        double endX = -1;
//        int firstBarIdxToPaint = -1;
//        int firstBarPainted = ChartBars.FromIndex;
//        int lastBarPainted = ChartBars.ToIndex;
//        Plot plot = Plots[seriesCount];
//        bool textDrawn = false;

//        for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
//        {
//            int prevSessionBreakIdx = newSessionBarIdxArr[i];
//            if (prevSessionBreakIdx <= lastBarPainted)
//            {
//                firstBarIdxToPaint = prevSessionBreakIdx;
//                break;
//            }
//        }

//        for (int idx = lastBarPainted; idx >= Math.Max(firstBarPainted, lastBarPainted - width); idx--)
//        {
//            if (idx < firstBarIdxToPaint)
//                break;

//            startX = chartControl.GetXByBarIndex(ChartBars, idx);
//            endX = chartControl.GetXByBarIndex(ChartBars, lastBarPainted);
//            double val = Values[seriesCount].GetValueAt(idx);
//            y = chartScale.GetYByValue(val);
//            yUpper = chartScale.GetYByValue(val + 1);
//            yLower = chartScale.GetYByValue(val - 1);

//            // **FIXED: Create rectangle using proper coordinates**
//            SharpDX.RectangleF rect = new SharpDX.RectangleF(
//                (float)startX, (float)yUpper, 
//                (float)(endX - startX), (float)(yLower - yUpper)
//            );

//            // **CORRECTED: Get color from plot brush properly**
//            System.Windows.Media.SolidColorBrush plotBrush = plot.Brush as System.Windows.Media.SolidColorBrush;
//            SharpDX.Color4 fillColor;
            
//            if (plotBrush != null)
//            {
//                // Convert System.Windows.Media.Color to SharpDX.Color4
//                var color = plotBrush.Color;
//                fillColor = new SharpDX.Color4(
//                    color.R / 255f,
//                    color.G / 255f, 
//                    color.B / 255f,
//                    0.15f  // 15% opacity
//                );
//            }
//            else
//            {
//                // Fallback to a default color if conversion fails
//                fillColor = new SharpDX.Color4(0.5f, 0.5f, 0.5f, 0.15f); // Gray
//            }

//            // Create fill brush with proper color
//            var fillBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, fillColor);

//            // Draw the filled rectangle
//            RenderTarget.FillRectangle(rect, fillBrush);

//            // Draw rectangle outline using the plot's DX brush
//            RenderTarget.DrawRectangle(rect, plot.BrushDX, 1);

//            // Draw the middle pivot line on top of the rectangle
//            System.Windows.Point startPoint = new System.Windows.Point(startX, y);
//            System.Windows.Point endPoint = new System.Windows.Point(endX, y);
//            RenderTarget.DrawLine(startPoint.ToVector2(), endPoint.ToVector2(), plot.BrushDX, 2, plot.StrokeStyle);

//            // Draw text label only once
//            if (!textDrawn)
//            {
//                TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, plot.Name, textFormat, ChartPanel.W, textFormat.FontSize);
//                RenderTarget.DrawTextLayout(startPoint.ToVector2(), textLayout, plot.BrushDX);
//                textLayout.Dispose();
//                textDrawn = true;
//            }

//            // Clean up the fill brush
//            fillBrush?.Dispose();
//        }
//    }
//    textFormat.Dispose();
//}

//protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
//{
//    TextFormat textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();

//    for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
//    {
//        double y = -1;
//        double yUpper = -1;
//        double yLower = -1;
//        double startX = -1;
//        double endX = -1;
//        int firstBarIdxToPaint = -1;
//        int firstBarPainted = ChartBars.FromIndex;
//        int lastBarPainted = ChartBars.ToIndex;
//        Plot plot = Plots[seriesCount];

//        for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
//        {
//            int prevSessionBreakIdx = newSessionBarIdxArr[i];
//            if (prevSessionBreakIdx <= lastBarPainted)
//            {
//                firstBarIdxToPaint = prevSessionBreakIdx;
//                break;
//            }
//        }

//        for (int idx = lastBarPainted; idx >= Math.Max(firstBarPainted, lastBarPainted - width); idx--)
//        {
//            if (idx < firstBarIdxToPaint)
//                break;

//            startX = chartControl.GetXByBarIndex(ChartBars, idx);
//            endX = chartControl.GetXByBarIndex(ChartBars, lastBarPainted);
//            double val = Values[seriesCount].GetValueAt(idx);
//            y = chartScale.GetYByValue(val);

//            // Calculate Y coordinates for upper and lower lines
//            yUpper = chartScale.GetYByValue(val + 1);  // 1 point above
//            yLower = chartScale.GetYByValue(val - 1);  // 1 point below

//            System.Windows.Point startPoint = new System.Windows.Point(startX, y);
//            System.Windows.Point endPoint = new System.Windows.Point(endX, y);
            
//            // **Draw main pivot line (middle line with width 2)**
//            RenderTarget.DrawLine(startPoint.ToVector2(), endPoint.ToVector2(), plot.BrushDX, 2, plot.StrokeStyle);

//            // **Draw upper line (solid, thinner)**
//            System.Windows.Point upperStartPoint = new System.Windows.Point(startX, yUpper);
//            System.Windows.Point upperEndPoint = new System.Windows.Point(endX, yUpper);
//            RenderTarget.DrawLine(upperStartPoint.ToVector2(), upperEndPoint.ToVector2(), plot.BrushDX, 1, null);

//            // **Draw lower line (solid, thinner)**
//            System.Windows.Point lowerStartPoint = new System.Windows.Point(startX, yLower);
//            System.Windows.Point lowerEndPoint = new System.Windows.Point(endX, yLower);
//            RenderTarget.DrawLine(lowerStartPoint.ToVector2(), lowerEndPoint.ToVector2(), plot.BrushDX, 1, null);

//            // **KEY FIX: Draw text label exactly like Cams.cs - no conditions**
//            TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, plot.Name, textFormat, ChartPanel.W, textFormat.FontSize);
//            RenderTarget.DrawTextLayout(startPoint.ToVector2(), textLayout, plot.BrushDX);
//            textLayout.Dispose();
//        }
//    }
//    textFormat.Dispose();
//}


protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			// Set text to chart label color and font
			TextFormat	textFormat			= chartControl.Properties.LabelFont.ToDirectWriteTextFormat();

			// Loop through each Plot Values on the chart
			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
			{
				double	y					= -1;
				double	startX				= -1;
				double	endX				= -1;
				double yUpper = -1;
        		double yLower = -1;
				int		firstBarIdxToPaint	= -1;
				int		firstBarPainted		= ChartBars.FromIndex;
				int		lastBarPainted		= ChartBars.ToIndex;
				Plot	plot				= Plots[seriesCount];

				for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
				{
					int prevSessionBreakIdx = newSessionBarIdxArr[i];
					if (prevSessionBreakIdx <= lastBarPainted)
					{
						firstBarIdxToPaint = prevSessionBreakIdx;
						break;
					}
				}

				// Loop through visble bars to render plot values
				for (int idx = lastBarPainted; idx >= Math.Max(firstBarPainted, lastBarPainted - width); idx--)
				{
					if (idx < firstBarIdxToPaint)
						break;

					startX		= chartControl.GetXByBarIndex(ChartBars, idx);
					endX		= chartControl.GetXByBarIndex(ChartBars, lastBarPainted);
					double val	= Values[seriesCount].GetValueAt(idx);
					y			= chartScale.GetYByValue(val);
					   // Calculate Y coordinates for upper and lower lines
		            yUpper = chartScale.GetYByValue(val + 1);  // 1 point above
		            yLower = chartScale.GetYByValue(val - 1);  // 1 point below
				}

				         
				// Draw pivot lines
				Point startPoint	= new Point(startX, y);
				Point endPoint		= new Point(endX, y);
				RenderTarget.DrawLine(startPoint.ToVector2(), endPoint.ToVector2(), plot.BrushDX, plot.Width, plot.StrokeStyle);
				
					            // **Draw upper line (solid, thinner)**
	            System.Windows.Point upperStartPoint = new System.Windows.Point(startX, yUpper);
	            System.Windows.Point upperEndPoint = new System.Windows.Point(endX, yUpper);
	            RenderTarget.DrawLine(upperStartPoint.ToVector2(), upperEndPoint.ToVector2(), plot.BrushDX, 1, null);
	
	            // **Draw lower line (solid, thinner)**
	            System.Windows.Point lowerStartPoint = new System.Windows.Point(startX, yLower);
	            System.Windows.Point lowerEndPoint = new System.Windows.Point(endX, yLower);
	            RenderTarget.DrawLine(lowerStartPoint.ToVector2(), lowerEndPoint.ToVector2(), plot.BrushDX, 1, null);

				// Draw pivot text
				TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, plot.Name, textFormat, ChartPanel.W, textFormat.FontSize);
				RenderTarget.DrawTextLayout(startPoint.ToVector2(), textLayout, plot.BrushDX);
				textLayout.Dispose();
			}
			textFormat.Dispose();
		}




        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "PivotRange", GroupName = "NinjaScriptParameters", Order = 0)]
        public PivotRange PivotRangeType
        {
            get { return pivotRangeType; }
            set { pivotRangeType = value; }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "HLCCalculationMode", GroupName = "NinjaScriptParameters", Order = 1)]
        [RefreshProperties(RefreshProperties.All)]
        public HLCCalculationMode PriorDayHlc
        {
            get { return priorDayHlc; }
            set { priorDayHlc = value; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> R1
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> R2
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> R3
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> R4
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> R5
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> R6
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> S1
        {
            get { return Values[6]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> S2
        {
            get { return Values[7]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> S3
        {
            get { return Values[8]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> S4
        {
            get { return Values[9]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> S5
        {
            get { return Values[10]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> S6
        {
            get { return Values[11]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> DP
        {
            get { return Values[12]; }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "UserDefinedClose", GroupName = "NinjaScriptParameters", Order = 2)]
        public double UserDefinedClose
        {
            get { return userDefinedClose; }
            set { userDefinedClose = value; }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "UserDefinedHigh", GroupName = "NinjaScriptParameters", Order = 3)]
        public double UserDefinedHigh
        {
            get { return userDefinedHigh; }
            set { userDefinedHigh = value; }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "UserDefinedLow", GroupName = "NinjaScriptParameters", Order = 4)]
        public double UserDefinedLow
        {
            get { return userDefinedLow; }
            set { userDefinedLow = value; }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Width", GroupName = "NinjaScriptParameters", Order = 5)]
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        #endregion
    }

    public class CamarillaBoxTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);
            CamarillaBox thisPivotsInstance = (CamarillaBox)value;
            HLCCalculationMode selectedHLCCalculationMode = thisPivotsInstance.PriorDayHlc;

            if (selectedHLCCalculationMode == HLCCalculationMode.UserDefinedValues)
                return propertyDescriptorCollection;

            PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
            {
                if (thisDescriptor.Name == "UserDefinedClose" || thisDescriptor.Name == "UserDefinedHigh" || thisDescriptor.Name == "UserDefinedLow")
                    adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] { new BrowsableAttribute(false), }));
                else
                    adjusted.Add(thisDescriptor);
            }
            return adjusted;
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CamarillaBox[] cacheCamarillaBox;
		public CamarillaBox CamarillaBox(PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return CamarillaBox(Input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}

		public CamarillaBox CamarillaBox(ISeries<double> input, PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			if (cacheCamarillaBox != null)
				for (int idx = 0; idx < cacheCamarillaBox.Length; idx++)
					if (cacheCamarillaBox[idx] != null && cacheCamarillaBox[idx].PivotRangeType == pivotRangeType && cacheCamarillaBox[idx].PriorDayHlc == priorDayHlc && cacheCamarillaBox[idx].UserDefinedClose == userDefinedClose && cacheCamarillaBox[idx].UserDefinedHigh == userDefinedHigh && cacheCamarillaBox[idx].UserDefinedLow == userDefinedLow && cacheCamarillaBox[idx].Width == width && cacheCamarillaBox[idx].EqualsInput(input))
						return cacheCamarillaBox[idx];
			return CacheIndicator<CamarillaBox>(new CamarillaBox(){ PivotRangeType = pivotRangeType, PriorDayHlc = priorDayHlc, UserDefinedClose = userDefinedClose, UserDefinedHigh = userDefinedHigh, UserDefinedLow = userDefinedLow, Width = width }, input, ref cacheCamarillaBox);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CamarillaBox CamarillaBox(PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.CamarillaBox(Input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}

		public Indicators.CamarillaBox CamarillaBox(ISeries<double> input , PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.CamarillaBox(input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CamarillaBox CamarillaBox(PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.CamarillaBox(Input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}

		public Indicators.CamarillaBox CamarillaBox(ISeries<double> input , PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.CamarillaBox(input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}
	}
}

#endregion
