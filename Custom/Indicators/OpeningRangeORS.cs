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
using SharpDX.DirectWrite;
using NinjaTrader.NinjaScript.Indicators;
using System.Globalization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class ORORS
    {
        public double High { get; set; }

        public double Low { get; set; }

        public double Mid { get; set; }

        public double LatestPrice { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    [CategoryOrder("Opening Range", 1)]
    [CategoryOrder("Appearance", 2)]
    [CategoryOrder("Labels", 3)]
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.OpeningRangeORSPropertyConverter")]
    public class OpeningRangeORS : Indicator
    {
        public static int DefaultOpeningRangePeriod = 30;
        public static OpeningRangeORSBarType DefaultOpeningRangeType =
            OpeningRangeORSBarType.Minutes;

        public static OpeningRangeORSStartTime DefaultOpeningRangeORSStartTime =
            OpeningRangeORSStartTime.Default;
        public static DateTime DefaultCustomOpeningRangeORSStartTime =
            DateTime.Parse("09:30", CultureInfo.InvariantCulture);

        public static OpeningRangeORSColorScheme DefaultOpeningRangeORSColorScheme =
            OpeningRangeORSColorScheme.Default;

        public static Stroke DefaultOpeningRangeStroke =
            new Stroke(Brushes.Yellow, DashStyleHelper.Solid, 3);
        public static Stroke DefaultOpeningRangeMidStroke =
            new Stroke(Brushes.Gray, DashStyleHelper.Dash, 2);

        public static Stroke DefaultPriceAboveStroke =
            new Stroke(Brushes.LimeGreen, DashStyleHelper.Solid, 3);
        public static Stroke DefaultPriceBelowStroke =
            new Stroke(Brushes.Red, DashStyleHelper.Solid, 3);
        public static Stroke DefaultPriceInsideStroke =
            new Stroke(Brushes.Yellow, DashStyleHelper.Solid, 3);

        public static SimpleFont DefaultOpeningRangeFont =
            new SimpleFont("Arial", 12);
        public static Brush DefaultOpeningRangeFontColor = Brushes.LightGray;

        public static bool DefaultShowLabels = true;
        public static string DefaultOpeningRangeHighLabel = "ORH @ {level}";
        public static string DefaultOpeningRangeLowLabel = "ORL @ {level}";
        public static string DefaultOpeningRangeMidLabel = "ORM @ {level}";
        public static OpeningRangeORSLabelPosition DefaultOpeningRangeORSLabelPosition =
            OpeningRangeORSLabelPosition.Above;

        private const int PrimaryBars = 0;
        private int OpeningRangeBars;
        private int RegularTradingHoursBars;

        private const string RegularTradingHours = "US Equities RTH";
        private TimeSpan RegularTradingHoursOpen;
        private TimeSpan SessionClose;

        private List<ORORS> OpeningRanges;
        private ORORS CurrentOpeningRange;

        private double OpeningRangeHigh;
        private double OpeningRangeLow;
        private double OpeningRangeMid;
        private double LastPrice;

        private const int LabelPadding = 5;
        private const string LevelFormatString = "{level}";
		
		private double dUpperQuarterPrice = 0.0;
		private double dLowerQuarterPrice = 0.0;
		private double[] upperRangeLines = new double[20];
		private double[] lowerRangeLines = new double[20];

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Opening Range ORS Indicator";
                Name = "Opening Range ORS";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                OpeningRangePeriod = DefaultOpeningRangePeriod;
                OpeningRangeType = DefaultOpeningRangeType;
                StartTime = DefaultOpeningRangeORSStartTime;
                CustomOpeningRangeORSStartTime = DefaultCustomOpeningRangeORSStartTime;

                ColorScheme = DefaultOpeningRangeORSColorScheme;
                OpeningRangeHighStroke = DefaultOpeningRangeStroke;
                OpeningRangeLowStroke = DefaultOpeningRangeStroke;
                OpeningRangeMidStroke = DefaultOpeningRangeMidStroke;
                PriceAboveStroke = DefaultPriceAboveStroke;
                PriceBelowStroke = DefaultPriceBelowStroke;
                PriceInsideStroke = DefaultPriceInsideStroke;

                ShowLabels = DefaultShowLabels;
                OpeningRangeFont = DefaultOpeningRangeFont;
                OpeningRangeFontColor = DefaultOpeningRangeFontColor;
                OpeningRangeHighLabel = DefaultOpeningRangeHighLabel;
                OpeningRangeHighLabelPosition = DefaultOpeningRangeORSLabelPosition;
                OpeningRangeLowLabel = DefaultOpeningRangeLowLabel;
                OpeningRangeLowLabelPosition = DefaultOpeningRangeORSLabelPosition;
                OpeningRangeMidLabel = DefaultOpeningRangeMidLabel;
                OpeningRangeMidLabelPosition = DefaultOpeningRangeORSLabelPosition;

                ArePlotsConfigurable = false;
                AddPlot(Brushes.Transparent, "ORH");
                AddPlot(Brushes.Transparent, "ORL");
                AddPlot(Brushes.Transparent, "ORM");
            }
            else if (State == State.Configure)
            {
                ResetOpeningRange(DateTime.MinValue);
                OpeningRanges = new List<ORORS>();

                AddDataSeries(Instrument.FullName, new BarsPeriod { 
                    BarsPeriodType = BarsPeriodType.Second, Value = 1 }, 
                        Instrument.MasterInstrument.TradingHours.Name);

                OpeningRangeBars = 1;

                RegularTradingHoursBars = 0;
                if (OpeningRangeORSStartTime.Default == StartTime && RegularTradingHours != Bars.TradingHours.Name)
                {
                    AddDataSeries(Instrument.FullName, new BarsPeriod { 
                        BarsPeriodType = BarsPeriodType.Second, Value = 1 }, RegularTradingHours);
                    RegularTradingHoursBars = 2;
                    OpeningRangeBars = 2;
                }
            }
            else if (State == State.DataLoaded)
            {
                SessionIterator regularTradingHoursSession = new SessionIterator(BarsArray[RegularTradingHoursBars]);
                RegularTradingHoursOpen = regularTradingHoursSession
                    .GetTradingDayBeginLocal(regularTradingHoursSession.ActualTradingDayExchange).TimeOfDay;

                SessionIterator chartSession = new SessionIterator(BarsArray[PrimaryBars]);
                SessionClose = chartSession.GetTradingDayEndLocal(chartSession.ActualTradingDayExchange).TimeOfDay;
            }
            else if (State == State.Historical)
            {
                SetZOrder(-1); // Display behind bars on chart.
            }
        }

        protected override void OnBarUpdate()
        {   
            DateTime now = Times[BarsInProgress][0];

            DateTime OpeningRangeORSStartTime = GetOpeningRangeORSStartTime(now);

            if (OpeningRangeBars == BarsInProgress)
            {
                if (now > OpeningRangeORSStartTime && now <= GetOpeningRangeEndTime(OpeningRangeORSStartTime))
                {
                    if (CurrentOpeningRange == null)
                    {
                        CurrentOpeningRange = new ORORS
                        {
                            High = OpeningRangeHigh,
                            Low = OpeningRangeLow,
                            Mid = OpeningRangeMid,
                            LatestPrice = LastPrice,
                            StartTime = now
                        };
                        OpeningRanges.Add(CurrentOpeningRange);
                    }

                    if (Highs[BarsInProgress][0] > OpeningRangeHigh || OpeningRangeHigh == 0.0)
                        OpeningRangeHigh = Highs[BarsInProgress][0];

                    if (Lows[BarsInProgress][0] < OpeningRangeLow || OpeningRangeLow == 0.0)
                        OpeningRangeLow = Lows[BarsInProgress][0];
                }
            }

            if (PrimaryBars == BarsInProgress)
			{
			    if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
			        ResetOpeningRange(now);
			
			    ORH[0] = OpeningRangeHigh;
			    ORL[0] = OpeningRangeLow;
			
			    OpeningRangeMid = Instrument.MasterInstrument
			        .RoundToTickSize((OpeningRangeLow + OpeningRangeHigh) / 2.0);
			    ORM[0] = OpeningRangeMid;
			
			    // Calculate quarter prices
			    double rangeSize = OpeningRangeHigh - OpeningRangeLow;
			    dUpperQuarterPrice = OpeningRangeMid + (rangeSize / 4);
			    dLowerQuarterPrice = OpeningRangeMid - (rangeSize / 4);
			    
			    // Calculate additional range lines
			    double quarterSize = rangeSize / 4;
			    for (int i = 0; i < 20; i++)
			    {
			        upperRangeLines[i] = OpeningRangeHigh + (quarterSize * (i + 1));
			        lowerRangeLines[i] = OpeningRangeLow - (quarterSize * (i + 1));
			    }
			
			    LastPrice = Close[0];
			    UpdateOpeningRange(now);
			}
        }

//        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
//		{
//		    base.OnRender(chartControl, chartScale);
		
//		    SharpDX.Direct2D1.Brush openingRangeMidBrush = OpeningRangeMidStroke.Brush.ToDxBrush(RenderTarget);
//		    TextFormat textFormat = OpeningRangeFont.ToDirectWriteTextFormat();
//		    SharpDX.Direct2D1.Brush textBrush = OpeningRangeFontColor.ToDxBrush(RenderTarget);
		
//		    foreach (ORORS openingRange in OpeningRanges)
//		    {   
//		        int openingRangeStartX = chartControl.GetXByTime(openingRange.StartTime);
//		        int openingRangeEndX = openingRange.EndTime == default(DateTime) 
//		            ? ChartPanel.X + ChartPanel.W 
//		            : chartControl.GetXByTime(openingRange.EndTime);
		
//		        if (openingRange.High > 0.0)
//		        {
//		            double rangeSize = openingRange.High - openingRange.Low;
//		            double upperQuarter = openingRange.Mid + (rangeSize / 4);
//		            double lowerQuarter = openingRange.Mid - (rangeSize / 4);
		            
//		            // Draw quarter lines
//		            int upperQuarterY = chartScale.GetYByValue(upperQuarter);
//		            int lowerQuarterY = chartScale.GetYByValue(lowerQuarter);
		            
//		            float quarterEndX = openingRangeEndX;
//		            if (ShowLabels && openingRange.EndTime == default(DateTime) 
//		                && openingRangeStartX < ChartPanel.X + ChartPanel.W)
//		            {
//		                SharpDX.Vector2 labelOrigin = DrawLabel("Q3 @ {level}", upperQuarter, 
//		                    OpeningRangeORSLabelPosition.Above, textFormat, textBrush, chartScale);
		                
//		                if (OpeningRangeORSLabelPosition.Center == OpeningRangeORSLabelPosition.Center)
//		                    quarterEndX = labelOrigin.X - LabelPadding;
//		            }
		
//		            RenderTarget.DrawLine(
//		                new SharpDX.Vector2(openingRangeStartX, upperQuarterY),
//		                new SharpDX.Vector2(quarterEndX, upperQuarterY),
//		                openingRangeMidBrush, OpeningRangeMidStroke.Width, OpeningRangeMidStroke.StrokeStyle);
		                
//		            RenderTarget.DrawLine(
//		                new SharpDX.Vector2(openingRangeStartX, lowerQuarterY),
//		                new SharpDX.Vector2(quarterEndX, lowerQuarterY),
//		                openingRangeMidBrush, OpeningRangeMidStroke.Width, OpeningRangeMidStroke.StrokeStyle);
		
//		            // Draw additional range lines
//		            double quarterSize = rangeSize / 4;
//		            for (int i = 0; i < 20; i++)
//		            {
//		                double upperLine = openingRange.High + (quarterSize * (i + 1));
//		                double lowerLine = openingRange.Low - (quarterSize * (i + 1));
		                
//		                int upperY = chartScale.GetYByValue(upperLine);
//		                int lowerY = chartScale.GetYByValue(lowerLine);
		                
//		                float rangeEndX = openingRangeEndX;
//		                if (ShowLabels && openingRange.EndTime == default(DateTime))
//		                {
//		                    SharpDX.Vector2 upperLabelOrigin = DrawLabel($"R{i+1} @ {{level}}", upperLine, 
//		                        OpeningRangeORSLabelPosition.Above, textFormat, textBrush, chartScale);
//		                    SharpDX.Vector2 lowerLabelOrigin = DrawLabel($"S{i+1} @ {{level}}", lowerLine, 
//		                        OpeningRangeORSLabelPosition.Below, textFormat, textBrush, chartScale);
		                        
//		                    if (OpeningRangeORSLabelPosition.Center == OpeningRangeORSLabelPosition.Center)
//		                        rangeEndX = Math.Min(upperLabelOrigin.X, lowerLabelOrigin.X) - LabelPadding;
//		                }
		                
//		                RenderTarget.DrawLine(
//		                    new SharpDX.Vector2(openingRangeStartX, upperY),
//		                    new SharpDX.Vector2(rangeEndX, upperY),
//		                    openingRangeMidBrush, OpeningRangeMidStroke.Width, OpeningRangeMidStroke.StrokeStyle);
		                    
//		                RenderTarget.DrawLine(
//		                    new SharpDX.Vector2(openingRangeStartX, lowerY),
//		                    new SharpDX.Vector2(rangeEndX, lowerY),
//		                    openingRangeMidBrush, OpeningRangeMidStroke.Width, OpeningRangeMidStroke.StrokeStyle);
//		            }
//		        }
//		    }
		
//		    openingRangeMidBrush.Dispose();
//		    textFormat.Dispose();
//		    textBrush.Dispose();
//		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
{
    base.OnRender(chartControl, chartScale);

    SharpDX.Direct2D1.Brush openingRangeMidBrush = OpeningRangeMidStroke.Brush.ToDxBrush(RenderTarget);
    TextFormat textFormat = OpeningRangeFont.ToDirectWriteTextFormat();
    SharpDX.Direct2D1.Brush textBrush = OpeningRangeFontColor.ToDxBrush(RenderTarget);

    foreach (ORORS openingRange in OpeningRanges)
    {   
        int openingRangeStartX = chartControl.GetXByTime(openingRange.StartTime);
        int openingRangeEndX = openingRange.EndTime == default(DateTime) 
            ? ChartPanel.X + ChartPanel.W 
            : chartControl.GetXByTime(openingRange.EndTime);

        if (openingRange.High > 0.0)
        {
            // Draw Opening Range High Line
            double openingRangeHigh = openingRange.High;
            float openingRangeHighEndX = openingRangeEndX;
            
            if (ShowLabels && openingRange.EndTime == default(DateTime) 
                && openingRangeStartX < ChartPanel.X + ChartPanel.W)
            {
                SharpDX.Vector2 labelOrigin = DrawLabel(OpeningRangeHighLabel, openingRangeHigh, 
                    OpeningRangeHighLabelPosition, textFormat, textBrush, chartScale);

                if (OpeningRangeORSLabelPosition.Center == OpeningRangeHighLabelPosition)
                    openingRangeHighEndX = labelOrigin.X - LabelPadding;
            }

            int openingRangeHighY = chartScale.GetYByValue(openingRangeHigh);
            if (openingRangeStartX < openingRangeHighEndX)
            {
                Stroke openingRangeHighStroke = GetStroke(openingRange, OpeningRangeHighStroke);
                SharpDX.Direct2D1.Brush openingRangeHighBrush = openingRangeHighStroke.Brush.ToDxBrush(RenderTarget);
                RenderTarget.DrawLine(
                    new SharpDX.Vector2(openingRangeStartX, openingRangeHighY),
                    new SharpDX.Vector2(openingRangeHighEndX, openingRangeHighY),
                    openingRangeHighBrush, openingRangeHighStroke.Width, openingRangeHighStroke.StrokeStyle);
                openingRangeHighBrush.Dispose();
            }

            // Draw Opening Range Low Line
            if (openingRange.Low > 0.0)
            {
                double openingRangeLow = openingRange.Low;
                float openingRangeLowEndX = openingRangeEndX;
                
                if (ShowLabels && openingRange.EndTime == default(DateTime) 
                    && openingRangeStartX < ChartPanel.X + ChartPanel.W)
                {
                    SharpDX.Vector2 labelOrigin = DrawLabel(OpeningRangeLowLabel, openingRangeLow, 
                        OpeningRangeLowLabelPosition, textFormat, textBrush, chartScale);

                    if (OpeningRangeORSLabelPosition.Center == OpeningRangeLowLabelPosition)
                        openingRangeLowEndX = labelOrigin.X - LabelPadding;
                }

                int openingRangeLowY = chartScale.GetYByValue(openingRangeLow);
                if (openingRangeStartX < openingRangeLowEndX)
                {
                    Stroke openingRangeLowStroke = GetStroke(openingRange, OpeningRangeLowStroke);
                    SharpDX.Direct2D1.Brush openingRangeLowBrush = openingRangeLowStroke.Brush.ToDxBrush(RenderTarget);
                    RenderTarget.DrawLine(
                        new SharpDX.Vector2(openingRangeStartX, openingRangeLowY),
                        new SharpDX.Vector2(openingRangeLowEndX, openingRangeLowY),
                        openingRangeLowBrush, openingRangeLowStroke.Width, openingRangeLowStroke.StrokeStyle);
                    openingRangeLowBrush.Dispose();
                }
            }

            // Calculate and draw quarter lines and additional range lines
            double rangeSize = openingRange.High - openingRange.Low;
            double upperQuarter = openingRange.Mid + (rangeSize / 4);
            double lowerQuarter = openingRange.Mid - (rangeSize / 4);
            
            // Draw quarter lines
            DrawRangeLine(upperQuarter, openingRangeStartX, openingRangeEndX, 
                "Q3", OpeningRangeORSLabelPosition.Above, openingRangeMidBrush, textFormat, textBrush, chartScale);
            DrawRangeLine(lowerQuarter, openingRangeStartX, openingRangeEndX, 
                "Q1", OpeningRangeORSLabelPosition.Above, openingRangeMidBrush, textFormat, textBrush, chartScale);
            
            // Draw additional range lines
            double quarterSize = rangeSize / 4;
            for (int i = 0; i < 20; i++)
            {
                double upperLine = openingRange.High + (quarterSize * (i + 1));
                double lowerLine = openingRange.Low - (quarterSize * (i + 1));
                
                DrawRangeLine(upperLine, openingRangeStartX, openingRangeEndX, 
                    $"R{i+1}", OpeningRangeORSLabelPosition.Above, openingRangeMidBrush, textFormat, textBrush, chartScale);
                DrawRangeLine(lowerLine, openingRangeStartX, openingRangeEndX, 
                    $"S{i+1}", OpeningRangeORSLabelPosition.Below, openingRangeMidBrush, textFormat, textBrush, chartScale);
            }

            // Draw Mid Line
            if (openingRange.Mid > 0.0)
            {
                double openingRangeMid = openingRange.Mid;
                float openingRangeMidEndX = openingRangeEndX;
                
                if (ShowLabels && openingRange.EndTime == default(DateTime)
                    && openingRangeStartX < ChartPanel.X + ChartPanel.W)
                {
                    SharpDX.Vector2 labelOrigin = DrawLabel(OpeningRangeMidLabel, openingRangeMid, 
                        OpeningRangeMidLabelPosition, textFormat, textBrush, chartScale);

                    if (OpeningRangeORSLabelPosition.Center == OpeningRangeMidLabelPosition)
                        openingRangeMidEndX = labelOrigin.X - LabelPadding;
                }

                int openingRangeMidY = chartScale.GetYByValue(openingRangeMid);
                if (openingRangeStartX < openingRangeMidEndX)
                    RenderTarget.DrawLine(
                        new SharpDX.Vector2(openingRangeStartX, openingRangeMidY),
                        new SharpDX.Vector2(openingRangeMidEndX, openingRangeMidY),
                        openingRangeMidBrush, OpeningRangeMidStroke.Width, OpeningRangeMidStroke.StrokeStyle);
            }
        }
    }

    openingRangeMidBrush.Dispose();
    textFormat.Dispose();
    textBrush.Dispose();
}


		private void DrawRangeLine(double level, int startX, int endX, string label, 
		    OpeningRangeORSLabelPosition position, SharpDX.Direct2D1.Brush brush,
		    TextFormat textFormat, SharpDX.Direct2D1.Brush textBrush, ChartScale chartScale)
		{
		    float lineEndX = endX;
		    int levelY = chartScale.GetYByValue(level);
		
		    if (ShowLabels && startX < ChartPanel.X + ChartPanel.W)
		    {
		        string labelText = label.Replace("{level}", level.ToString("F2"));
		        TextLayout textLayout = new TextLayout(Core.Globals.DirectWriteFactory,
		            labelText, textFormat, 500, textFormat.FontSize);
		
		        float labelY;
		        switch (position)
		        {
		            case OpeningRangeORSLabelPosition.Below:
		                labelY = ChartPanel.Y + (float)levelY + LabelPadding;
		                break;
		            case OpeningRangeORSLabelPosition.Center:
		                labelY = ChartPanel.Y + (float)levelY - (textLayout.Metrics.Height / 2.0f);
		                break;
		            default:
		                labelY = ChartPanel.Y + (float)levelY - textLayout.Metrics.Height - LabelPadding;
		                break;
		        }
		
		        SharpDX.Vector2 textOrigin = new SharpDX.Vector2(
		            ChartPanel.W - textLayout.Metrics.Width - LabelPadding, labelY);
		
		        if (OpeningRangeORSLabelPosition.Center == position)
		            lineEndX = textOrigin.X - LabelPadding;
		
		        RenderTarget.DrawTextLayout(textOrigin, textLayout, textBrush, 
		            SharpDX.Direct2D1.DrawTextOptions.NoSnap);
		
		        textLayout.Dispose();
		    }
		
		    if (startX < lineEndX)
		    {
		        RenderTarget.DrawLine(
		            new SharpDX.Vector2(startX, levelY),
		            new SharpDX.Vector2(lineEndX, levelY),
		            brush, OpeningRangeMidStroke.Width, OpeningRangeMidStroke.StrokeStyle);
		    }
		}

        private Stroke GetStroke(ORORS openingRange, Stroke defaultStroke)
        {
            if (ColorScheme == OpeningRangeORSColorScheme.PriceBased)
            {
                if (openingRange.LatestPrice > openingRange.High)
                    return PriceAboveStroke;
                else if (openingRange.LatestPrice < openingRange.Low)
                    return PriceBelowStroke;
                else
                    return PriceInsideStroke;
            }
            return defaultStroke;
        }

        private SharpDX.Vector2 DrawLabel(string label, double level, OpeningRangeORSLabelPosition position, 
            TextFormat textFormat, SharpDX.Direct2D1.Brush textBrush, ChartScale chartScale)
        {
            string labelText = label.Replace(LevelFormatString, level.ToString("F2"));

            TextLayout textLayout = new TextLayout(Core.Globals.DirectWriteFactory,
                labelText, textFormat, 500, textFormat.FontSize);

            int levelY = chartScale.GetYByValue(level);

            float labelY;
            switch (position)
            {
                case OpeningRangeORSLabelPosition.Below:
                    labelY = ChartPanel.Y + (float)levelY + LabelPadding;
                    break;
                case OpeningRangeORSLabelPosition.Center:
                    labelY = ChartPanel.Y + (float)levelY - (textLayout.Metrics.Height / 2.0f);
                    break;
                case OpeningRangeORSLabelPosition.Above:
                default:
                    labelY = ChartPanel.Y + (float)levelY - textLayout.Metrics.Height - LabelPadding;
                    break;
            }

            SharpDX.Vector2 textOrigin = new SharpDX.Vector2(
                ChartPanel.W - textLayout.Metrics.Width - LabelPadding, labelY);

            RenderTarget.DrawTextLayout(textOrigin, textLayout, textBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

            textLayout.Dispose();

            return textOrigin;
        }

        private DateTime GetOpeningRangeORSStartTime(DateTime now)
        {
            if (OpeningRangeORSStartTime.Custom == StartTime)
                return now.Date + CustomOpeningRangeORSStartTime.TimeOfDay;
            return now.Date + RegularTradingHoursOpen;
        }

        private DateTime GetOpeningRangeEndTime(DateTime OpeningRangeORSStartTime)
        {
            switch (OpeningRangeType)
            {
                case OpeningRangeORSBarType.Seconds:
                    return OpeningRangeORSStartTime.AddSeconds(OpeningRangePeriod);
                case OpeningRangeORSBarType.Hours:
                    return OpeningRangeORSStartTime.AddHours(OpeningRangePeriod);
                default:
                    return OpeningRangeORSStartTime.AddMinutes(OpeningRangePeriod);
            }
        }

        private void UpdateOpeningRange(DateTime now)
        {
            if (CurrentOpeningRange != null)
            {
                CurrentOpeningRange.High = OpeningRangeHigh;
                CurrentOpeningRange.Low = OpeningRangeLow;
                CurrentOpeningRange.Mid = OpeningRangeMid;

                DateTime sessionClose = now.Date + SessionClose;
                if (GetOpeningRangeORSStartTime(now) > sessionClose)
                    sessionClose = sessionClose.AddDays(1);

                if (CurrentOpeningRange.EndTime == default(DateTime))
                {
                    CurrentOpeningRange.LatestPrice = LastPrice;

                    if (now >= sessionClose)
                        CurrentOpeningRange.EndTime = sessionClose;
                }
            }
        }

        private void ResetOpeningRange(DateTime now)
        {
            OpeningRangeHigh = 0.0;
            OpeningRangeLow = 0.0;
            OpeningRangeMid = 0.0;
            LastPrice = 0.0;

            if (CurrentOpeningRange != null && CurrentOpeningRange.EndTime == default(DateTime))
                CurrentOpeningRange.EndTime = now;

            CurrentOpeningRange = null;
        }

        public override string DisplayName
        {
            get { return Name; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Description = "Opening range period", Order = 1, GroupName = "Opening Range")]
        public int OpeningRangePeriod
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Type", Description = "Type of opening range being calculated", Order = 2, GroupName = "Opening Range")]
        public OpeningRangeORSBarType OpeningRangeType
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
        [TypeConverter(typeof(OpeningRangeORSStartTypeConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Start Time", Description = "Start time of opening range", Order = 3, GroupName = "Opening Range")]
        public OpeningRangeORSStartTime StartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.AutoCloseTimeEditorKey")]
        [Display(Name = "", Description = "Opening range start in local time", Order = 4, GroupName = "Opening Range")]
        public DateTime CustomOpeningRangeORSStartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
        [TypeConverter(typeof(OpeningRangeORSColorSchemeConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Color Scheme", Description = "Opening range coloring scheme", Order = 1, GroupName = "Appearance")]
        public OpeningRangeORSColorScheme ColorScheme
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Price Above", Description = "Opening range lines drawn on chart when price is > ORH", Order = 2, GroupName = "Appearance")]
        public Stroke PriceAboveStroke
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Price Below", Description = "Opening range lines drawn on chart when price is < ORL", Order = 3, GroupName = "Appearance")]
        public Stroke PriceBelowStroke
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Price Inside", Description = "Opening range lines drawn on chart when price is inside OR", Order = 4, GroupName = "Appearance")]
        public Stroke PriceInsideStroke
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range High", Description = "Opening range high line drawn on chart", Order = 5, GroupName = "Appearance")]
        public Stroke OpeningRangeHighStroke
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range Mid", Description = "Opening range mid line drawn on chart", Order = 6, GroupName = "Appearance")]
        public Stroke OpeningRangeMidStroke
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range Low", Description = "Opening range low line drawn on chart", Order = 7, GroupName = "Appearance")]
        public Stroke OpeningRangeLowStroke
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Labels", Order = 1, GroupName = "Labels")]
        public bool ShowLabels
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Font", Description = "Font used to display the opening range labels", Order = 2, GroupName = "Labels")]
        public SimpleFont OpeningRangeFont
        { get; set; }

        [XmlIgnore]
        [NinjaScriptProperty]
        [Display(Name = "Font Color", Description = "Color of the text used to label the opening range levels", Order = 3, GroupName = "Labels")]
        public Brush OpeningRangeFontColor
        { get; set; }

        [Browsable(false)]
        public string OpeningRangeFontColorSerialization
        {
            get { return Serialize.BrushToString(OpeningRangeFontColor); }
            set { OpeningRangeFontColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range High", Order = 4, GroupName = "Labels")]
        public string OpeningRangeHighLabel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    Label Position", Order = 5, GroupName = "Labels")]
        public OpeningRangeORSLabelPosition OpeningRangeHighLabelPosition
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range Mid", Order = 6, GroupName = "Labels")]
        public string OpeningRangeMidLabel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    Label Position", Order = 7, GroupName = "Labels")]
        public OpeningRangeORSLabelPosition OpeningRangeMidLabelPosition
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range Low", Order = 8, GroupName = "Labels")]
        public string OpeningRangeLowLabel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    Label Position", Order = 9, GroupName = "Labels")]
        public OpeningRangeORSLabelPosition OpeningRangeLowLabelPosition
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORH
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORL
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORM
        {
            get { return Values[2]; }
        }
    }

    public class OpeningRangeORSPropertyConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            OpeningRangeORS indicator = component as OpeningRangeORS;

            PropertyDescriptorCollection properties = base.GetPropertiesSupported(context) ?
                base.GetProperties(context, component, attrs) : TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || properties == null)
                return properties;

            PropertyDescriptor customOpeningRangeORSStartTime = properties["CustomOpeningRangeORSStartTime"];

            properties.Remove(customOpeningRangeORSStartTime);

            if (indicator.StartTime == OpeningRangeORSStartTime.Custom)
                properties.Add(customOpeningRangeORSStartTime);

            PropertyDescriptor priceAboveStroke = properties["PriceAboveStroke"];
            PropertyDescriptor priceBelowStroke = properties["PriceBelowStroke"];
            PropertyDescriptor priceInsideStroke = properties["PriceInsideStroke"];

            properties.Remove(priceAboveStroke);
            properties.Remove(priceBelowStroke);
            properties.Remove(priceInsideStroke);

            if (indicator.ColorScheme == OpeningRangeORSColorScheme.PriceBased)
            {
                PropertyDescriptor openingRangeHighStroke = properties["OpeningRangeORS"];
                PropertyDescriptor openingRangeLowStroke = properties["OpeningRangeORS"];

                properties.Remove(openingRangeHighStroke);
                properties.Remove(openingRangeLowStroke);

                properties.Add(priceAboveStroke);
                properties.Add(priceBelowStroke);
                properties.Add(priceInsideStroke);
            }

            return properties;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        { return true; }
    }
}

public class OpeningRangeORSStartTypeConverter : TypeConverter
{
    private const string DEFAULT = "US RTH Open";
    private const string CUSTOM = "Custom";

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        List<string> values = new List<string>() { DEFAULT, CUSTOM };
        return new StandardValuesCollection(values);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        switch (value.ToString())
        {
            case DEFAULT:
                return OpeningRangeORSStartTime.Default;
            case CUSTOM:
                return OpeningRangeORSStartTime.Custom;
        }
        return OpeningRangeORSStartTime.Default;
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        OpeningRangeORSStartTime enumValue = (OpeningRangeORSStartTime)Enum.Parse(typeof(OpeningRangeORSStartTime), value.ToString());
        switch (enumValue)
        {
            case OpeningRangeORSStartTime.Default:
                return DEFAULT;
            case OpeningRangeORSStartTime.Custom:
                return CUSTOM;
        }
        return DEFAULT;
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    { return true; }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    { return true; }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    { return true; }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    { return true; }
}

public class OpeningRangeORSColorSchemeConverter : TypeConverter
{
    private const string DEFAULT = "Default";
    private const string PRICE_BASED = "Price Based";

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        List<string> values = new List<string>() { DEFAULT, PRICE_BASED };
        return new StandardValuesCollection(values);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        switch (value.ToString())
        {
            case DEFAULT:
                return OpeningRangeORSColorScheme.Default;
            case PRICE_BASED:
                return OpeningRangeORSColorScheme.PriceBased;
        }
        return OpeningRangeORSColorScheme.Default;
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        OpeningRangeORSColorScheme enumValue = (OpeningRangeORSColorScheme)Enum.Parse(typeof(OpeningRangeORSColorScheme), value.ToString());
        switch (enumValue)
        {
            case OpeningRangeORSColorScheme.Default:
                return DEFAULT;
            case OpeningRangeORSColorScheme.PriceBased:
                return PRICE_BASED;
        }
        return DEFAULT;
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    { return true; }

    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    { return true; }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    { return true; }

    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    { return true; }
}

public enum OpeningRangeORSBarType
{
    Seconds,
    Minutes,
    Hours
}

public enum OpeningRangeORSStartTime
{
    Default,
    Custom
}

public enum OpeningRangeORSColorScheme
{
    Default,
    PriceBased
}

public enum OpeningRangeORSLabelPosition
{
    Above,
    Below,
    Center
}

// Creates an easy to use constructor for use in strategies.
namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        public Indicators.OpeningRangeORS OpeningRangeORS(int openingRangePeriod, OpeningRangeORSBarType openingRangeType)
        {
            return indicator.OpeningRangeORS(
                Input, 
                openingRangePeriod, 
                openingRangeType,
                Indicators.OpeningRangeORS.DefaultOpeningRangeORSStartTime,
                Indicators.OpeningRangeORS.DefaultCustomOpeningRangeORSStartTime,
                Indicators.OpeningRangeORS.DefaultOpeningRangeORSColorScheme,
                Indicators.OpeningRangeORS.DefaultPriceAboveStroke,
                Indicators.OpeningRangeORS.DefaultPriceBelowStroke,
                Indicators.OpeningRangeORS.DefaultPriceInsideStroke,
                Indicators.OpeningRangeORS.DefaultOpeningRangeStroke,
                Indicators.OpeningRangeORS.DefaultOpeningRangeMidStroke,
                Indicators.OpeningRangeORS.DefaultOpeningRangeStroke,
                Indicators.OpeningRangeORS.DefaultShowLabels,
                Indicators.OpeningRangeORS.DefaultOpeningRangeFont,
                Indicators.OpeningRangeORS.DefaultOpeningRangeFontColor,
                Indicators.OpeningRangeORS.DefaultOpeningRangeHighLabel,
                Indicators.OpeningRangeORS.DefaultOpeningRangeORSLabelPosition,
                Indicators.OpeningRangeORS.DefaultOpeningRangeMidLabel,
                Indicators.OpeningRangeORS.DefaultOpeningRangeORSLabelPosition,
                Indicators.OpeningRangeORS.DefaultOpeningRangeLowLabel, 
                Indicators.OpeningRangeORS.DefaultOpeningRangeORSLabelPosition
            );
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OpeningRangeORS[] cacheOpeningRangeORS;
		public OpeningRangeORS OpeningRangeORS(int openingRangePeriod, OpeningRangeORSBarType openingRangeType, OpeningRangeORSStartTime startTime, DateTime customOpeningRangeORSStartTime, OpeningRangeORSColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeORSLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeORSLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeORSLabelPosition openingRangeLowLabelPosition)
		{
			return OpeningRangeORS(Input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeORSStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}

		public OpeningRangeORS OpeningRangeORS(ISeries<double> input, int openingRangePeriod, OpeningRangeORSBarType openingRangeType, OpeningRangeORSStartTime startTime, DateTime customOpeningRangeORSStartTime, OpeningRangeORSColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeORSLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeORSLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeORSLabelPosition openingRangeLowLabelPosition)
		{
			if (cacheOpeningRangeORS != null)
				for (int idx = 0; idx < cacheOpeningRangeORS.Length; idx++)
					if (cacheOpeningRangeORS[idx] != null && cacheOpeningRangeORS[idx].OpeningRangePeriod == openingRangePeriod && cacheOpeningRangeORS[idx].OpeningRangeType == openingRangeType && cacheOpeningRangeORS[idx].StartTime == startTime && cacheOpeningRangeORS[idx].CustomOpeningRangeORSStartTime == customOpeningRangeORSStartTime && cacheOpeningRangeORS[idx].ColorScheme == colorScheme && cacheOpeningRangeORS[idx].PriceAboveStroke == priceAboveStroke && cacheOpeningRangeORS[idx].PriceBelowStroke == priceBelowStroke && cacheOpeningRangeORS[idx].PriceInsideStroke == priceInsideStroke && cacheOpeningRangeORS[idx].OpeningRangeHighStroke == openingRangeHighStroke && cacheOpeningRangeORS[idx].OpeningRangeMidStroke == openingRangeMidStroke && cacheOpeningRangeORS[idx].OpeningRangeLowStroke == openingRangeLowStroke && cacheOpeningRangeORS[idx].ShowLabels == showLabels && cacheOpeningRangeORS[idx].OpeningRangeFont == openingRangeFont && cacheOpeningRangeORS[idx].OpeningRangeFontColor == openingRangeFontColor && cacheOpeningRangeORS[idx].OpeningRangeHighLabel == openingRangeHighLabel && cacheOpeningRangeORS[idx].OpeningRangeHighLabelPosition == openingRangeHighLabelPosition && cacheOpeningRangeORS[idx].OpeningRangeMidLabel == openingRangeMidLabel && cacheOpeningRangeORS[idx].OpeningRangeMidLabelPosition == openingRangeMidLabelPosition && cacheOpeningRangeORS[idx].OpeningRangeLowLabel == openingRangeLowLabel && cacheOpeningRangeORS[idx].OpeningRangeLowLabelPosition == openingRangeLowLabelPosition && cacheOpeningRangeORS[idx].EqualsInput(input))
						return cacheOpeningRangeORS[idx];
			return CacheIndicator<OpeningRangeORS>(new OpeningRangeORS(){ OpeningRangePeriod = openingRangePeriod, OpeningRangeType = openingRangeType, StartTime = startTime, CustomOpeningRangeORSStartTime = customOpeningRangeORSStartTime, ColorScheme = colorScheme, PriceAboveStroke = priceAboveStroke, PriceBelowStroke = priceBelowStroke, PriceInsideStroke = priceInsideStroke, OpeningRangeHighStroke = openingRangeHighStroke, OpeningRangeMidStroke = openingRangeMidStroke, OpeningRangeLowStroke = openingRangeLowStroke, ShowLabels = showLabels, OpeningRangeFont = openingRangeFont, OpeningRangeFontColor = openingRangeFontColor, OpeningRangeHighLabel = openingRangeHighLabel, OpeningRangeHighLabelPosition = openingRangeHighLabelPosition, OpeningRangeMidLabel = openingRangeMidLabel, OpeningRangeMidLabelPosition = openingRangeMidLabelPosition, OpeningRangeLowLabel = openingRangeLowLabel, OpeningRangeLowLabelPosition = openingRangeLowLabelPosition }, input, ref cacheOpeningRangeORS);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OpeningRangeORS OpeningRangeORS(int openingRangePeriod, OpeningRangeORSBarType openingRangeType, OpeningRangeORSStartTime startTime, DateTime customOpeningRangeORSStartTime, OpeningRangeORSColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeORSLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeORSLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeORSLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRangeORS(Input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeORSStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}

		public Indicators.OpeningRangeORS OpeningRangeORS(ISeries<double> input , int openingRangePeriod, OpeningRangeORSBarType openingRangeType, OpeningRangeORSStartTime startTime, DateTime customOpeningRangeORSStartTime, OpeningRangeORSColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeORSLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeORSLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeORSLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRangeORS(input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeORSStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OpeningRangeORS OpeningRangeORS(int openingRangePeriod, OpeningRangeORSBarType openingRangeType, OpeningRangeORSStartTime startTime, DateTime customOpeningRangeORSStartTime, OpeningRangeORSColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeORSLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeORSLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeORSLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRangeORS(Input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeORSStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}

		public Indicators.OpeningRangeORS OpeningRangeORS(ISeries<double> input , int openingRangePeriod, OpeningRangeORSBarType openingRangeType, OpeningRangeORSStartTime startTime, DateTime customOpeningRangeORSStartTime, OpeningRangeORSColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeORSLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeORSLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeORSLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRangeORS(input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeORSStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}
	}
}

#endregion
