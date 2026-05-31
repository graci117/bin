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
    public class OR
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
    [TypeConverter("NinjaTrader.NinjaScript.Indicators.OpeningRangePropertyConverter")]
    public class OpeningRange : Indicator
    {
        public static int DefaultOpeningRangePeriod = 30;
        public static OpeningRangeBarType DefaultOpeningRangeType =
            OpeningRangeBarType.Minutes;

        public static OpeningRangeStartTime DefaultOpeningRangeStartTime =
            OpeningRangeStartTime.Default;
        public static DateTime DefaultCustomOpeningRangeStartTime =
            DateTime.Parse("09:30", CultureInfo.InvariantCulture);

        public static OpeningRangeColorScheme DefaultOpeningRangeColorScheme =
            OpeningRangeColorScheme.Default;

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
        public static OpeningRangeLabelPosition DefaultOpeningRangeLabelPosition =
            OpeningRangeLabelPosition.Above;

        private const int PrimaryBars = 0;
        private int OpeningRangeBars;
        private int RegularTradingHoursBars;

        private const string RegularTradingHours = "US Equities RTH";
        private TimeSpan RegularTradingHoursOpen;
        private TimeSpan SessionClose;

        private List<OR> OpeningRanges;
        private OR CurrentOpeningRange;

        private double OpeningRangeHigh;
        private double OpeningRangeLow;
        private double OpeningRangeMid;
        private double LastPrice;

        private const int LabelPadding = 5;
        private const string LevelFormatString = "{level}";

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Opening Range Indicator";
                Name = "Opening Range";
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
                StartTime = DefaultOpeningRangeStartTime;
                CustomOpeningRangeStartTime = DefaultCustomOpeningRangeStartTime;

                ColorScheme = DefaultOpeningRangeColorScheme;
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
                OpeningRangeHighLabelPosition = DefaultOpeningRangeLabelPosition;
                OpeningRangeLowLabel = DefaultOpeningRangeLowLabel;
                OpeningRangeLowLabelPosition = DefaultOpeningRangeLabelPosition;
                OpeningRangeMidLabel = DefaultOpeningRangeMidLabel;
                OpeningRangeMidLabelPosition = DefaultOpeningRangeLabelPosition;

                ArePlotsConfigurable = false;
                AddPlot(Brushes.Transparent, "ORH");
                AddPlot(Brushes.Transparent, "ORL");
                AddPlot(Brushes.Transparent, "ORM");
            }
            else if (State == State.Configure)
            {
                ResetOpeningRange(DateTime.MinValue);
                OpeningRanges = new List<OR>();

                AddDataSeries(Instrument.FullName, new BarsPeriod { 
                    BarsPeriodType = BarsPeriodType.Second, Value = 1 }, 
                        Instrument.MasterInstrument.TradingHours.Name);

                OpeningRangeBars = 1;

                RegularTradingHoursBars = 0;
                if (OpeningRangeStartTime.Default == StartTime && RegularTradingHours != Bars.TradingHours.Name)
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

            DateTime openingRangeStartTime = GetOpeningRangeStartTime(now);

            if (OpeningRangeBars == BarsInProgress)
            {
                if (now > openingRangeStartTime && now <= GetOpeningRangeEndTime(openingRangeStartTime))
                {
                    if (CurrentOpeningRange == null)
                    {
                        CurrentOpeningRange = new OR
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

                LastPrice = Close[0];

                UpdateOpeningRange(now);
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            SharpDX.Direct2D1.Brush openingRangeMidBrush = OpeningRangeMidStroke.Brush.ToDxBrush(RenderTarget);

            TextFormat textFormat = OpeningRangeFont.ToDirectWriteTextFormat();
            SharpDX.Direct2D1.Brush textBrush = OpeningRangeFontColor.ToDxBrush(RenderTarget);

            foreach (OR openingRange in OpeningRanges)
            {	
                int openingRangeStartX = chartControl.GetXByTime(openingRange.StartTime);

                int openingRangeEndX;
                if (openingRange.EndTime == default(DateTime))
                    openingRangeEndX = ChartPanel.X + ChartPanel.W;
                else
                    openingRangeEndX = chartControl.GetXByTime(openingRange.EndTime);

                if (openingRange.High > 0.0)
                {
                    double openingRangeHigh = openingRange.High;

                    float openingRangeHighEndX = openingRangeEndX;
                    if (ShowLabels && openingRange.EndTime == default(DateTime) 
                        && openingRangeStartX < ChartPanel.X + ChartPanel.W)
                    {
                        SharpDX.Vector2 labelOrigin = DrawLabel(OpeningRangeHighLabel, openingRangeHigh, 
                            OpeningRangeHighLabelPosition, textFormat, textBrush, chartScale);

                        if (OpeningRangeLabelPosition.Center == OpeningRangeHighLabelPosition)
                            openingRangeHighEndX = labelOrigin.X - LabelPadding;
                    }

                    int openingRangeHighY = chartScale.GetYByValue(openingRangeHigh);
                    if (openingRangeStartX < openingRangeHighEndX)
                    {
                        Stroke openingRangeHighStroke = GetStroke(openingRange, OpeningRangeHighStroke);
                        SharpDX.Direct2D1.Brush openingRangeHighBrush = openingRangeHighStroke.Brush.ToDxBrush(RenderTarget);
                        RenderTarget.DrawLine(new SharpDX.Vector2(openingRangeStartX, openingRangeHighY),
                            new SharpDX.Vector2(openingRangeHighEndX, openingRangeHighY),
                                openingRangeHighBrush, openingRangeHighStroke.Width, openingRangeHighStroke.StrokeStyle);
                        openingRangeHighBrush.Dispose();
                    }
                }

                if (openingRange.Low > 0.0)
                {
                    double openingRangeLow = openingRange.Low;

                    float openingRangeLowEndX = openingRangeEndX;
                    if (ShowLabels && openingRange.EndTime == default(DateTime) 
                        && openingRangeStartX < ChartPanel.X + ChartPanel.W)
                    {
                        SharpDX.Vector2 labelOrigin = DrawLabel(OpeningRangeLowLabel, openingRangeLow, 
                            OpeningRangeLowLabelPosition, textFormat, textBrush, chartScale);

                        if (OpeningRangeLabelPosition.Center == OpeningRangeLowLabelPosition)
                            openingRangeLowEndX = labelOrigin.X - LabelPadding;
                    }

                    int openingRangeLowY = chartScale.GetYByValue(openingRangeLow);
                    if (openingRangeStartX < openingRangeLowEndX)
                    {
                        Stroke openingRangeLowStroke = GetStroke(openingRange, OpeningRangeLowStroke);
                        SharpDX.Direct2D1.Brush openingRangeLowBrush = openingRangeLowStroke.Brush.ToDxBrush(RenderTarget);
                        RenderTarget.DrawLine(new SharpDX.Vector2(openingRangeStartX, openingRangeLowY),
                            new SharpDX.Vector2(openingRangeLowEndX, openingRangeLowY),
                                openingRangeLowBrush, openingRangeLowStroke.Width, openingRangeLowStroke.StrokeStyle);
                        openingRangeLowBrush.Dispose();
                    }
                }

                if (openingRange.Mid > 0.0)
                {
                    double openingRangeMid = openingRange.Mid;

                    float openingRangeMidEndX = openingRangeEndX;
                    if (ShowLabels && openingRange.EndTime == default(DateTime)
                        && openingRangeStartX < ChartPanel.X + ChartPanel.W)
                    {
                        SharpDX.Vector2 labelOrigin = DrawLabel(OpeningRangeMidLabel, openingRangeMid, 
                            OpeningRangeMidLabelPosition, textFormat, textBrush, chartScale);

                        if (OpeningRangeLabelPosition.Center == OpeningRangeMidLabelPosition)
                            openingRangeMidEndX = labelOrigin.X - LabelPadding;
                    }

                    int openingRangeMidY = chartScale.GetYByValue(openingRangeMid);
                    if (openingRangeStartX < openingRangeMidEndX)
                        RenderTarget.DrawLine(new SharpDX.Vector2(openingRangeStartX, openingRangeMidY),
                            new SharpDX.Vector2(openingRangeMidEndX, openingRangeMidY),
                                openingRangeMidBrush, OpeningRangeMidStroke.Width, OpeningRangeMidStroke.StrokeStyle);
                }
            }

            openingRangeMidBrush.Dispose();
            textFormat.Dispose();
            textBrush.Dispose();
        }

        private Stroke GetStroke(OR openingRange, Stroke defaultStroke)
        {
            if (ColorScheme == OpeningRangeColorScheme.PriceBased)
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

        private SharpDX.Vector2 DrawLabel(string label, double level, OpeningRangeLabelPosition position, 
            TextFormat textFormat, SharpDX.Direct2D1.Brush textBrush, ChartScale chartScale)
        {
            string labelText = label.Replace(LevelFormatString, level.ToString("F2"));

            TextLayout textLayout = new TextLayout(Core.Globals.DirectWriteFactory,
                labelText, textFormat, 500, textFormat.FontSize);

            int levelY = chartScale.GetYByValue(level);

            float labelY;
            switch (position)
            {
                case OpeningRangeLabelPosition.Below:
                    labelY = ChartPanel.Y + (float)levelY + LabelPadding;
                    break;
                case OpeningRangeLabelPosition.Center:
                    labelY = ChartPanel.Y + (float)levelY - (textLayout.Metrics.Height / 2.0f);
                    break;
                case OpeningRangeLabelPosition.Above:
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

        private DateTime GetOpeningRangeStartTime(DateTime now)
        {
            if (OpeningRangeStartTime.Custom == StartTime)
                return now.Date + CustomOpeningRangeStartTime.TimeOfDay;
            return now.Date + RegularTradingHoursOpen;
        }

        private DateTime GetOpeningRangeEndTime(DateTime openingRangeStartTime)
        {
            switch (OpeningRangeType)
            {
                case OpeningRangeBarType.Seconds:
                    return openingRangeStartTime.AddSeconds(OpeningRangePeriod);
                case OpeningRangeBarType.Hours:
                    return openingRangeStartTime.AddHours(OpeningRangePeriod);
                default:
                    return openingRangeStartTime.AddMinutes(OpeningRangePeriod);
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
                if (GetOpeningRangeStartTime(now) > sessionClose)
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
        public OpeningRangeBarType OpeningRangeType
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
        [TypeConverter(typeof(OpeningRangeStartTypeConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Start Time", Description = "Start time of opening range", Order = 3, GroupName = "Opening Range")]
        public OpeningRangeStartTime StartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.AutoCloseTimeEditorKey")]
        [Display(Name = "", Description = "Opening range start in local time", Order = 4, GroupName = "Opening Range")]
        public DateTime CustomOpeningRangeStartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
        [TypeConverter(typeof(OpeningRangeColorSchemeConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [Display(Name = "Color Scheme", Description = "Opening range coloring scheme", Order = 1, GroupName = "Appearance")]
        public OpeningRangeColorScheme ColorScheme
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
        public OpeningRangeLabelPosition OpeningRangeHighLabelPosition
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range Mid", Order = 6, GroupName = "Labels")]
        public string OpeningRangeMidLabel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    Label Position", Order = 7, GroupName = "Labels")]
        public OpeningRangeLabelPosition OpeningRangeMidLabelPosition
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Opening Range Low", Order = 8, GroupName = "Labels")]
        public string OpeningRangeLowLabel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    Label Position", Order = 9, GroupName = "Labels")]
        public OpeningRangeLabelPosition OpeningRangeLowLabelPosition
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

    public class OpeningRangePropertyConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            OpeningRange indicator = component as OpeningRange;

            PropertyDescriptorCollection properties = base.GetPropertiesSupported(context) ?
                base.GetProperties(context, component, attrs) : TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || properties == null)
                return properties;

            PropertyDescriptor customOpeningRangeStartTime = properties["CustomOpeningRangeStartTime"];

            properties.Remove(customOpeningRangeStartTime);

            if (indicator.StartTime == OpeningRangeStartTime.Custom)
                properties.Add(customOpeningRangeStartTime);

            PropertyDescriptor priceAboveStroke = properties["PriceAboveStroke"];
            PropertyDescriptor priceBelowStroke = properties["PriceBelowStroke"];
            PropertyDescriptor priceInsideStroke = properties["PriceInsideStroke"];

            properties.Remove(priceAboveStroke);
            properties.Remove(priceBelowStroke);
            properties.Remove(priceInsideStroke);

            if (indicator.ColorScheme == OpeningRangeColorScheme.PriceBased)
            {
                PropertyDescriptor openingRangeHighStroke = properties["OpeningRangeHighStroke"];
                PropertyDescriptor openingRangeLowStroke = properties["OpeningRangeLowStroke"];

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

public class OpeningRangeStartTypeConverter : TypeConverter
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
                return OpeningRangeStartTime.Default;
            case CUSTOM:
                return OpeningRangeStartTime.Custom;
        }
        return OpeningRangeStartTime.Default;
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        OpeningRangeStartTime enumValue = (OpeningRangeStartTime)Enum.Parse(typeof(OpeningRangeStartTime), value.ToString());
        switch (enumValue)
        {
            case OpeningRangeStartTime.Default:
                return DEFAULT;
            case OpeningRangeStartTime.Custom:
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

public class OpeningRangeColorSchemeConverter : TypeConverter
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
                return OpeningRangeColorScheme.Default;
            case PRICE_BASED:
                return OpeningRangeColorScheme.PriceBased;
        }
        return OpeningRangeColorScheme.Default;
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        OpeningRangeColorScheme enumValue = (OpeningRangeColorScheme)Enum.Parse(typeof(OpeningRangeColorScheme), value.ToString());
        switch (enumValue)
        {
            case OpeningRangeColorScheme.Default:
                return DEFAULT;
            case OpeningRangeColorScheme.PriceBased:
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

public enum OpeningRangeBarType
{
    Seconds,
    Minutes,
    Hours
}

public enum OpeningRangeStartTime
{
    Default,
    Custom
}

public enum OpeningRangeColorScheme
{
    Default,
    PriceBased
}

public enum OpeningRangeLabelPosition
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
        public Indicators.OpeningRange OpeningRange(int openingRangePeriod, OpeningRangeBarType openingRangeType)
        {
            return indicator.OpeningRange(
                Input, 
                openingRangePeriod, 
                openingRangeType,
                Indicators.OpeningRange.DefaultOpeningRangeStartTime,
                Indicators.OpeningRange.DefaultCustomOpeningRangeStartTime,
                Indicators.OpeningRange.DefaultOpeningRangeColorScheme,
                Indicators.OpeningRange.DefaultPriceAboveStroke,
                Indicators.OpeningRange.DefaultPriceBelowStroke,
                Indicators.OpeningRange.DefaultPriceInsideStroke,
                Indicators.OpeningRange.DefaultOpeningRangeStroke,
                Indicators.OpeningRange.DefaultOpeningRangeMidStroke,
                Indicators.OpeningRange.DefaultOpeningRangeStroke,
                Indicators.OpeningRange.DefaultShowLabels,
                Indicators.OpeningRange.DefaultOpeningRangeFont,
                Indicators.OpeningRange.DefaultOpeningRangeFontColor,
                Indicators.OpeningRange.DefaultOpeningRangeHighLabel,
                Indicators.OpeningRange.DefaultOpeningRangeLabelPosition,
                Indicators.OpeningRange.DefaultOpeningRangeMidLabel,
                Indicators.OpeningRange.DefaultOpeningRangeLabelPosition,
                Indicators.OpeningRange.DefaultOpeningRangeLowLabel, 
                Indicators.OpeningRange.DefaultOpeningRangeLabelPosition
            );
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OpeningRange[] cacheOpeningRange;
		public OpeningRange OpeningRange(int openingRangePeriod, OpeningRangeBarType openingRangeType, OpeningRangeStartTime startTime, DateTime customOpeningRangeStartTime, OpeningRangeColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeLabelPosition openingRangeLowLabelPosition)
		{
			return OpeningRange(Input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}

		public OpeningRange OpeningRange(ISeries<double> input, int openingRangePeriod, OpeningRangeBarType openingRangeType, OpeningRangeStartTime startTime, DateTime customOpeningRangeStartTime, OpeningRangeColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeLabelPosition openingRangeLowLabelPosition)
		{
			if (cacheOpeningRange != null)
				for (int idx = 0; idx < cacheOpeningRange.Length; idx++)
					if (cacheOpeningRange[idx] != null && cacheOpeningRange[idx].OpeningRangePeriod == openingRangePeriod && cacheOpeningRange[idx].OpeningRangeType == openingRangeType && cacheOpeningRange[idx].StartTime == startTime && cacheOpeningRange[idx].CustomOpeningRangeStartTime == customOpeningRangeStartTime && cacheOpeningRange[idx].ColorScheme == colorScheme && cacheOpeningRange[idx].PriceAboveStroke == priceAboveStroke && cacheOpeningRange[idx].PriceBelowStroke == priceBelowStroke && cacheOpeningRange[idx].PriceInsideStroke == priceInsideStroke && cacheOpeningRange[idx].OpeningRangeHighStroke == openingRangeHighStroke && cacheOpeningRange[idx].OpeningRangeMidStroke == openingRangeMidStroke && cacheOpeningRange[idx].OpeningRangeLowStroke == openingRangeLowStroke && cacheOpeningRange[idx].ShowLabels == showLabels && cacheOpeningRange[idx].OpeningRangeFont == openingRangeFont && cacheOpeningRange[idx].OpeningRangeFontColor == openingRangeFontColor && cacheOpeningRange[idx].OpeningRangeHighLabel == openingRangeHighLabel && cacheOpeningRange[idx].OpeningRangeHighLabelPosition == openingRangeHighLabelPosition && cacheOpeningRange[idx].OpeningRangeMidLabel == openingRangeMidLabel && cacheOpeningRange[idx].OpeningRangeMidLabelPosition == openingRangeMidLabelPosition && cacheOpeningRange[idx].OpeningRangeLowLabel == openingRangeLowLabel && cacheOpeningRange[idx].OpeningRangeLowLabelPosition == openingRangeLowLabelPosition && cacheOpeningRange[idx].EqualsInput(input))
						return cacheOpeningRange[idx];
			return CacheIndicator<OpeningRange>(new OpeningRange(){ OpeningRangePeriod = openingRangePeriod, OpeningRangeType = openingRangeType, StartTime = startTime, CustomOpeningRangeStartTime = customOpeningRangeStartTime, ColorScheme = colorScheme, PriceAboveStroke = priceAboveStroke, PriceBelowStroke = priceBelowStroke, PriceInsideStroke = priceInsideStroke, OpeningRangeHighStroke = openingRangeHighStroke, OpeningRangeMidStroke = openingRangeMidStroke, OpeningRangeLowStroke = openingRangeLowStroke, ShowLabels = showLabels, OpeningRangeFont = openingRangeFont, OpeningRangeFontColor = openingRangeFontColor, OpeningRangeHighLabel = openingRangeHighLabel, OpeningRangeHighLabelPosition = openingRangeHighLabelPosition, OpeningRangeMidLabel = openingRangeMidLabel, OpeningRangeMidLabelPosition = openingRangeMidLabelPosition, OpeningRangeLowLabel = openingRangeLowLabel, OpeningRangeLowLabelPosition = openingRangeLowLabelPosition }, input, ref cacheOpeningRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OpeningRange OpeningRange(int openingRangePeriod, OpeningRangeBarType openingRangeType, OpeningRangeStartTime startTime, DateTime customOpeningRangeStartTime, OpeningRangeColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRange(Input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}

		public Indicators.OpeningRange OpeningRange(ISeries<double> input , int openingRangePeriod, OpeningRangeBarType openingRangeType, OpeningRangeStartTime startTime, DateTime customOpeningRangeStartTime, OpeningRangeColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRange(input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OpeningRange OpeningRange(int openingRangePeriod, OpeningRangeBarType openingRangeType, OpeningRangeStartTime startTime, DateTime customOpeningRangeStartTime, OpeningRangeColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRange(Input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}

		public Indicators.OpeningRange OpeningRange(ISeries<double> input , int openingRangePeriod, OpeningRangeBarType openingRangeType, OpeningRangeStartTime startTime, DateTime customOpeningRangeStartTime, OpeningRangeColorScheme colorScheme, Stroke priceAboveStroke, Stroke priceBelowStroke, Stroke priceInsideStroke, Stroke openingRangeHighStroke, Stroke openingRangeMidStroke, Stroke openingRangeLowStroke, bool showLabels, SimpleFont openingRangeFont, Brush openingRangeFontColor, string openingRangeHighLabel, OpeningRangeLabelPosition openingRangeHighLabelPosition, string openingRangeMidLabel, OpeningRangeLabelPosition openingRangeMidLabelPosition, string openingRangeLowLabel, OpeningRangeLabelPosition openingRangeLowLabelPosition)
		{
			return indicator.OpeningRange(input, openingRangePeriod, openingRangeType, startTime, customOpeningRangeStartTime, colorScheme, priceAboveStroke, priceBelowStroke, priceInsideStroke, openingRangeHighStroke, openingRangeMidStroke, openingRangeLowStroke, showLabels, openingRangeFont, openingRangeFontColor, openingRangeHighLabel, openingRangeHighLabelPosition, openingRangeMidLabel, openingRangeMidLabelPosition, openingRangeLowLabel, openingRangeLowLabelPosition);
		}
	}
}

#endregion
