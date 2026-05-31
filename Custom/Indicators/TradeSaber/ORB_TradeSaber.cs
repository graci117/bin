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
#endregion

namespace NinjaTrader.NinjaScript.Indicators.TradeSaber
{
    public class ORB_TradeSaber : Indicator
    {
        private double sessionHigh;
        private double sessionLow;
        private bool isInRange;
        private DateTime adjustedStartTime;
        private DateTime adjustedEndTime;
        private Series<double> highSeries;
        private Series<double> lowSeries;
        private Series<double> middleSeries;
        private Series<double> target1Series;
        private Series<double> target2Series;
        private Series<double> targetMinus1Series;
        private Series<double> targetMinus2Series;
        private Series<double> targeLowerQuarterSeries;
        private Series<double> targetUpperQuarterSeries;
        private DateTime lastBarDate;

        #region Properties
		[NinjaScriptProperty]
		[Display(Name = "Highlight Range", Description = "Highlight the Opening Range period", Order = 9, GroupName = "Display")]
		public bool HighlightRange { get; set; }
		
		[Range(0, 1)]
		[NinjaScriptProperty]
		[Display(Name = "Highlight Opacity", Description = "Opacity of the highlight (0 to 1)", Order = 10, GroupName = "Display")]
		public double HighlightOpacity { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Opening Range Start Time", Description = "Start time of the Opening Range", Order = 0, GroupName = "Opening Range")]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		public DateTime StartTime { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Opening Range End Time", Description = "End time of the Opening Range", Order = 1, GroupName = "Opening Range")]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		public DateTime EndTime { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Time Zone", Description = "Select the time zone for the Opening Range", Order = 2, GroupName = "Opening Range")]
		[TypeConverter(typeof(TimeZoneConverter))]
		public string TimeZoneSelection { get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Time Zone Note", Description = "", Order = 3, GroupName = "Opening Range")]
		public string TZNote { get; set; }
		
		// 1. Above Upper and Below Lower (+1/-1)
		[NinjaScriptProperty]
		[Display(Name = "Show Outer Arrows (+1/-1)", Description = "Show arrows for crossing above upper or below lower", Order = 1, GroupName = "Predator X Signals")]
		public bool ShowOuterArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Upper Tag", Description = "Tag for arrow when crossing above upper range", Order = 2, GroupName = "Predator X Signals")]
		public string AboveUpperTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Lower Tag", Description = "Tag for arrow when crossing below lower range", Order = 3, GroupName = "Predator X Signals")]
		public string BelowLowerTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Upper Color", Description = "Color for arrow when crossing above upper range", Order = 4, GroupName = "Predator X Signals")]
		public Brush AboveUpperColor { get; set; }
		
		[Browsable(false)]
		public string AboveUpperColorSerializable
		{
		    get { return Serialize.BrushToString(AboveUpperColor); }
		    set { AboveUpperColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Lower Color", Description = "Color for arrow when crossing below lower range", Order = 5, GroupName = "Predator X Signals")]
		public Brush BelowLowerColor { get; set; }
		
		[Browsable(false)]
		public string BelowLowerColorSerializable
		{
		    get { return Serialize.BrushToString(BelowLowerColor); }
		    set { BelowLowerColor = Serialize.StringToBrush(value); }
		}
		
		// 2. Below Upper and Above Lower (+2/-2)
		[NinjaScriptProperty]
		[Display(Name = "Show Inner Arrows (+2/-2)", Description = "Show arrows for crossing above lower or below upper", Order = 6, GroupName = "Predator X Signals")]
		public bool ShowInnerArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Lower Tag", Description = "Tag for arrow when crossing above lower range", Order = 7, GroupName = "Predator X Signals")]
		public string AboveLowerTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Upper Tag", Description = "Tag for arrow when crossing below upper range", Order = 8, GroupName = "Predator X Signals")]
		public string BelowUpperTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Lower Color", Description = "Color for arrow when crossing above lower range", Order = 9, GroupName = "Predator X Signals")]
		public Brush AboveLowerColor { get; set; }
		
		[Browsable(false)]
		public string AboveLowerColorSerializable
		{
		    get { return Serialize.BrushToString(AboveLowerColor); }
		    set { AboveLowerColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Upper Color", Description = "Color for arrow when crossing below upper range", Order = 10, GroupName = "Predator X Signals")]
		public Brush BelowUpperColor { get; set; }
		
		[Browsable(false)]
		public string BelowUpperColorSerializable
		{
		    get { return Serialize.BrushToString(BelowUpperColor); }
		    set { BelowUpperColor = Serialize.StringToBrush(value); }
		}
		
		// 3. Midline (+3/-3)
		[NinjaScriptProperty]
		[Display(Name = "Show Midline Arrows (+3/-3)", Description = "Show arrows for crossing above or below midline", Order = 11, GroupName = "Predator X Signals")]
		public bool ShowMidlineArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Midline Tag", Description = "Tag for arrow when crossing above midline", Order = 12, GroupName = "Predator X Signals")]
		public string AboveMidlineTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Midline Tag", Description = "Tag for arrow when crossing below midline", Order = 13, GroupName = "Predator X Signals")]
		public string BelowMidlineTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Midline Color", Description = "Color for arrow when crossing above midline", Order = 14, GroupName = "Predator X Signals")]
		public Brush AboveMidlineColor { get; set; }
		
		[Browsable(false)]
		public string AboveMidlineColorSerializable
		{
		    get { return Serialize.BrushToString(AboveMidlineColor); }
		    set { AboveMidlineColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Midline Color", Description = "Color for arrow when crossing below midline", Order = 15, GroupName = "Predator X Signals")]
		public Brush BelowMidlineColor { get; set; }
		
		[Browsable(false)]
		public string BelowMidlineColorSerializable
		{
		    get { return Serialize.BrushToString(BelowMidlineColor); }
		    set { BelowMidlineColor = Serialize.StringToBrush(value); }
		}
		
		// 4. Upper Quarter Above/Below (+4/-4)
		[NinjaScriptProperty]
		[Display(Name = "Show Upper Quarter Arrows (+4/-4)", Description = "Show arrows for crossing above or below upper quarter", Order = 16, GroupName = "Predator X Signals")]
		public bool ShowUpperQuarterArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Upper Quarter Tag", Description = "Tag for arrow when crossing above upper quarter", Order = 17, GroupName = "Predator X Signals")]
		public string AboveUpperQuarterTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Upper Quarter Tag", Description = "Tag for arrow when crossing below upper quarter", Order = 18, GroupName = "Predator X Signals")]
		public string BelowUpperQuarterTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Upper Quarter Color", Description = "Color for arrow when crossing above upper quarter", Order = 19, GroupName = "Predator X Signals")]
		public Brush AboveUpperQuarterColor { get; set; }
		
		[Browsable(false)]
		public string AboveUpperQuarterColorSerializable
		{
		    get { return Serialize.BrushToString(AboveUpperQuarterColor); }
		    set { AboveUpperQuarterColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Upper Quarter Color", Description = "Color for arrow when crossing below upper quarter", Order = 20, GroupName = "Predator X Signals")]
		public Brush BelowUpperQuarterColor { get; set; }
		
		[Browsable(false)]
		public string BelowUpperQuarterColorSerializable
		{
		    get { return Serialize.BrushToString(BelowUpperQuarterColor); }
		    set { BelowUpperQuarterColor = Serialize.StringToBrush(value); }
		}
		
		// 5. Lower Quarter (+5/-5)
		[NinjaScriptProperty]
		[Display(Name = "Show Lower Quarter Arrows (+5/-5)", Description = "Show arrows for crossing above or below lower quarter", Order = 21, GroupName = "Predator X Signals")]
		public bool ShowLowerQuarterArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Lower Quarter Tag", Description = "Tag for arrow when crossing above lower quarter", Order = 22, GroupName = "Predator X Signals")]
		public string AboveLowerQuarterTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Lower Quarter Tag", Description = "Tag for arrow when crossing below lower quarter", Order = 23, GroupName = "Predator X Signals")]
		public string BelowLowerQuarterTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Lower Quarter Color", Description = "Color for arrow when crossing above lower quarter", Order = 24, GroupName = "Predator X Signals")]
		public Brush AboveLowerQuarterColor { get; set; }
		
		[Browsable(false)]
		public string AboveLowerQuarterColorSerializable
		{
		    get { return Serialize.BrushToString(AboveLowerQuarterColor); }
		    set { AboveLowerQuarterColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Lower Quarter Color", Description = "Color for arrow when crossing below lower quarter", Order = 25, GroupName = "Predator X Signals")]
		public Brush BelowLowerQuarterColor { get; set; }
		
		[Browsable(false)]
		public string BelowLowerQuarterColorSerializable
		{
		    get { return Serialize.BrushToString(BelowLowerQuarterColor); }
		    set { BelowLowerQuarterColor = Serialize.StringToBrush(value); }
		}
		
		// 6. Above/Below Upper Target 1 (+6/-6)
		[NinjaScriptProperty]
		[Display(Name = "Show Target 1 Upper Arrows (+6/-6)", Description = "Show arrows for crossing above or below upper target 1", Order = 26, GroupName = "Predator X Signals")]
		public bool ShowTarget1UpperArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Target 1 Upper Tag", Description = "Tag for arrow when crossing above upper target 1", Order = 27, GroupName = "Predator X Signals")]
		public string AboveTarget1UpperTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Target 1 Upper Tag", Description = "Tag for arrow when crossing below upper target 1", Order = 28, GroupName = "Predator X Signals")]
		public string BelowTarget1UpperTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Target 1 Upper Color", Description = "Color for arrow when crossing above upper target 1", Order = 29, GroupName = "Predator X Signals")]
		public Brush AboveTarget1UpperColor { get; set; }
		
		[Browsable(false)]
		public string AboveTarget1UpperColorSerializable
		{
		    get { return Serialize.BrushToString(AboveTarget1UpperColor); }
		    set { AboveTarget1UpperColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Target 1 Upper Color", Description = "Color for arrow when crossing below upper target 1", Order = 30, GroupName = "Predator X Signals")]
		public Brush BelowTarget1UpperColor { get; set; }
		
		[Browsable(false)]
		public string BelowTarget1UpperColorSerializable
		{
		    get { return Serialize.BrushToString(BelowTarget1UpperColor); }
		    set { BelowTarget1UpperColor = Serialize.StringToBrush(value); }
		}
		
		// 7. Below/Above Lower Target 1 (+7/-7)
		[NinjaScriptProperty]
		[Display(Name = "Show Target 1 Lower Arrows (+7/-7)", Description = "Show arrows for crossing below or above lower target 1", Order = 31, GroupName = "Predator X Signals")]
		public bool ShowTarget1LowerArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Target 1 Lower Tag", Description = "Tag for arrow when crossing below lower target 1", Order = 32, GroupName = "Predator X Signals")]
		public string BelowTarget1LowerTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Target 1 Lower Tag", Description = "Tag for arrow when crossing above lower target 1", Order = 33, GroupName = "Predator X Signals")]
		public string AboveTarget1LowerTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Target 1 Lower Color", Description = "Color for arrow when crossing below lower target 1", Order = 34, GroupName = "Predator X Signals")]
		public Brush BelowTarget1LowerColor { get; set; }
		
		[Browsable(false)]
		public string BelowTarget1LowerColorSerializable
		{
		    get { return Serialize.BrushToString(BelowTarget1LowerColor); }
		    set { BelowTarget1LowerColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Target 1 Lower Color", Description = "Color for arrow when crossing above lower target 1", Order = 35, GroupName = "Predator X Signals")]
		public Brush AboveTarget1LowerColor { get; set; }
		
		[Browsable(false)]
		public string AboveTarget1LowerColorSerializable
		{
		    get { return Serialize.BrushToString(AboveTarget1LowerColor); }
		    set { AboveTarget1LowerColor = Serialize.StringToBrush(value); }
		}
		
		// 8. Above/Below Upper Target 2 (+8/-8)
		[NinjaScriptProperty]
		[Display(Name = "Show Target 2 Upper Arrows (+8/-8)", Description = "Show arrows for crossing above or below upper target 2", Order = 36, GroupName = "Predator X Signals")]
		public bool ShowTarget2UpperArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Target 2 Upper Tag", Description = "Tag for arrow when crossing above upper target 2", Order = 37, GroupName = "Predator X Signals")]
		public string AboveTarget2UpperTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Target 2 Upper Tag", Description = "Tag for arrow when crossing below upper target 2", Order = 38, GroupName = "Predator X Signals")]
		public string BelowTarget2UpperTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Target 2 Upper Color", Description = "Color for arrow when crossing above upper target 2", Order = 39, GroupName = "Predator X Signals")]
		public Brush AboveTarget2UpperColor { get; set; }
		
		[Browsable(false)]
		public string AboveTarget2UpperColorSerializable
		{
		    get { return Serialize.BrushToString(AboveTarget2UpperColor); }
		    set { AboveTarget2UpperColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Target 2 Upper Color", Description = "Color for arrow when crossing below upper target 2", Order = 40, GroupName = "Predator X Signals")]
		public Brush BelowTarget2UpperColor { get; set; }
		
		[Browsable(false)]
		public string BelowTarget2UpperColorSerializable
		{
		    get { return Serialize.BrushToString(BelowTarget2UpperColor); }
		    set { BelowTarget2UpperColor = Serialize.StringToBrush(value); }
		}
		
		// 9. Above/Below Lower Target 2 (+9/-9)
		[NinjaScriptProperty]
		[Display(Name = "Show Target 2 Lower Arrows (+9/-9)", Description = "Show arrows for crossing above or below lower target 2", Order = 41, GroupName = "Predator X Signals")]
		public bool ShowTarget2LowerArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Above Target 2 Lower Tag", Description = "Tag for arrow when crossing above lower target 2", Order = 42, GroupName = "Predator X Signals")]
		public string AboveTarget2LowerTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Below Target 2 Lower Tag", Description = "Tag for arrow when crossing below lower target 2", Order = 43, GroupName = "Predator X Signals")]
		public string BelowTarget2LowerTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Above Target 2 Lower Color", Description = "Color for arrow when crossing above lower target 2", Order = 44, GroupName = "Predator X Signals")]
		public Brush AboveTarget2LowerColor { get; set; }
		
		[Browsable(false)]
		public string AboveTarget2LowerColorSerializable
		{
		    get { return Serialize.BrushToString(AboveTarget2LowerColor); }
		    set { AboveTarget2LowerColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Below Target 2 Lower Color", Description = "Color for arrow when crossing below lower target 2", Order = 45, GroupName = "Predator X Signals")]
		public Brush BelowTarget2LowerColor { get; set; }
		
		[Browsable(false)]
		public string BelowTarget2LowerColorSerializable
		{
		    get { return Serialize.BrushToString(BelowTarget2LowerColor); }
		    set { BelowTarget2LowerColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Show Day Start Arrows", Description = "Show arrows at midnight/new day", Order = 46, GroupName = "Predator X Signals")]
		public bool ShowDayStartArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Range Start Arrows", Description = "Show arrows at start of ORB range", Order = 47, GroupName = "Predator X Signals")]
		public bool ShowRangeStartArrows { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Day/Range Start Up Arrow Tag", Description = "Tag for up arrow at session start", Order = 48, GroupName = "Predator X Signals")]
		public string SessionStartUpTag { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Day/Range Start Down Arrow Tag", Description = "Tag for down arrow at session start", Order = 49, GroupName = "Predator X Signals")]
		public string SessionStartDownTag { get; set; }
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Session Start Up Color", Description = "Color for up arrow at session/day start", Order = 50, GroupName = "Predator X Signals")]
		public Brush SessionStartUpColor { get; set; }
		
		[Browsable(false)]
		public string SessionStartUpColorSerializable
		{
		    get { return Serialize.BrushToString(SessionStartUpColor); }
		    set { SessionStartUpColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Session Start Down Color", Description = "Color for down arrow at session/day start", Order = 51, GroupName = "Predator X Signals")]
		public Brush SessionStartDownColor { get; set; }
		
		[Browsable(false)]
		public string SessionStartDownColorSerializable
		{
		    get { return Serialize.BrushToString(SessionStartDownColor); }
		    set { SessionStartDownColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Show Mid Range", Description = "Show middle range plot", Order = 0, GroupName = "Extended Ranges")]
		public bool ShowMid { get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Middle Range Multiplier", Description = "Multiplier for middle range (default 0.5 for 50%)", Order = 1, GroupName = "Extended Ranges")]
		public double MiddleRangeMultiplier { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Quarters", Description = "Show quarter range plots (at multiplier and 1 - multiplier within range)", Order = 2, GroupName = "Extended Ranges")]
		public bool ShowQuarters { get; set; }
		
		[NinjaScriptProperty]
		[Range(0, 1)]
		[Display(Name = "Quarter Range Multiplier", Description = "Multiplier for quarter range (default 0.25 for 25% and 75%)", Order = 3, GroupName = "Extended Ranges")]
		public double QuarterRangeMultiplier { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Targets 1", Description = "Show first target above and below range", Order = 4, GroupName = "Extended Ranges")]
		public bool ShowTarget1 { get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Target 1 Multiplier", Description = "Value for first target (applies as + and -)", Order = 5, GroupName = "Extended Ranges")]
		public double Target1Multiplier { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Targets 2", Description = "Show second target above and below range", Order = 6, GroupName = "Extended Ranges")]
		public bool ShowTarget2 { get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name = "Target 2 Multiplier", Description = "Value for second target (applies as + and -)", Order = 7, GroupName = "Extended Ranges")]
		public double Target2Multiplier { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Social Media Buttons", Description = "", Order = 1, GroupName = "TradeSaber Socials")]
		public bool ShowSocials { get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Author", Description = "", Order = 2, GroupName = "TradeSaber Socials")]
		public string Author { get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Version", Description = "", Order = 3, GroupName = "TradeSaber Socials")]
		public string Version { get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "TradeSaber Link", Description = "", Order = 4, GroupName = "TradeSaber Socials")]
		public string TradeSaber { get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Discord", Description = "", Order = 5, GroupName = "TradeSaber Socials")]
		public string Discord { get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Youtube", Description = "", Order = 6, GroupName = "TradeSaber Socials")]
		public string YouTube { get; set; }
		#endregion

        #region Exposed Plots for Strategy Builder
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORHigh
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLow
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Signal
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORMiddle
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORTarget1
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORTarget2
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORTargetMinus1
        {
            get { return Values[6]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORTargetMinus2
        {
            get { return Values[7]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORLowerTargetQuarter
        {
            get { return Values[8]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ORUpperTargetQuarter
        {
            get { return Values[9]; }
        }
        #endregion

        #region Time Zone Converter
        private class TimeZoneConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { 
                    "Local", "EST", "CST", "MST", "PST", "UTC", 
                    "GMT", "CET", "EET", "JST", "AEST" 
                });
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                if (value is string str)
                    return str;
                return base.ConvertFrom(context, culture, value);
            }
        }
        #endregion

        #region TradeSaber Social
        private Brush axisColor;
        private bool YouTubeButtonClicked;
        private bool DiscordButtonClicked;
        private bool tradeSaberButtonClicked;
        private System.Windows.Controls.Button YouTubeButton;
        private System.Windows.Controls.Button DiscordButton;
        private System.Windows.Controls.Button tradeSaberButton;
        private System.Windows.Controls.Button closeSocialsButton;
        private System.Windows.Controls.Grid myGrid29;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description             = "Plots the Opening Range High and Low with signals and customizable options.";
                Name                   = "ORB_TradeSaber";
                Calculate              = Calculate.OnBarClose;
                IsOverlay              = true;
                DisplayInDataBox       = true;

                StartTime              = new DateTime(2000, 1, 1, 9, 30, 0);
                EndTime                = new DateTime(2000, 1, 1, 9, 45, 0);
                TimeZoneSelection      = "Local";
                TZNote                 = "Recommended that PC and \nNinjaTrader clocks match";
                ShowOuterArrows        = true;
                AboveUpperTag          = "AboveUpper";
                BelowLowerTag          = "BelowLower";
                AboveUpperColor        = Brushes.SteelBlue;
                BelowLowerColor        = Brushes.DarkOrange;
                ShowInnerArrows        = false;
                AboveLowerTag          = "AboveLower";
                BelowUpperTag          = "BelowUpper";
                AboveLowerColor        = Brushes.LightGreen;
                BelowUpperColor        = Brushes.Pink;
                ShowMidlineArrows      = false;
                AboveMidlineTag        = "AboveMidline";
                BelowMidlineTag        = "BelowMidline";
                AboveMidlineColor      = Brushes.Cyan;
                BelowMidlineColor      = Brushes.Magenta;
                ShowUpperQuarterArrows = false;
                AboveUpperQuarterTag   = "AboveUpperQuarter";
                BelowUpperQuarterTag   = "BelowUpperQuarter";
                AboveUpperQuarterColor = Brushes.Aqua;
                BelowUpperQuarterColor = Brushes.Crimson;
                ShowLowerQuarterArrows = false;
                AboveLowerQuarterTag   = "AboveLowerQuarter";
                BelowLowerQuarterTag   = "BelowLowerQuarter";
                AboveLowerQuarterColor = Brushes.Lime;
                BelowLowerQuarterColor = Brushes.OrangeRed;
                ShowTarget1UpperArrows = false;
                AboveTarget1UpperTag   = "AboveTarget1Upper";
                BelowTarget1UpperTag   = "BelowTarget1Upper";
                AboveTarget1UpperColor = Brushes.DodgerBlue;
                BelowTarget1UpperColor = Brushes.Tomato;
                ShowTarget1LowerArrows = false;
                BelowTarget1LowerTag   = "BelowTarget1Lower";
                AboveTarget1LowerTag   = "AboveTarget1Lower";
                BelowTarget1LowerColor = Brushes.Tomato;
                AboveTarget1LowerColor = Brushes.DodgerBlue;
                ShowTarget2UpperArrows = false;
                AboveTarget2UpperTag   = "AboveTarget2Upper";
                BelowTarget2UpperTag   = "BelowTarget2Upper";
                AboveTarget2UpperColor = Brushes.DeepSkyBlue;
                BelowTarget2UpperColor = Brushes.DarkRed;
                ShowTarget2LowerArrows = false;
                AboveTarget2LowerTag   = "AboveTarget2Lower";
                BelowTarget2LowerTag   = "BelowTarget2Lower";
                AboveTarget2LowerColor = Brushes.DeepSkyBlue;
                BelowTarget2LowerColor = Brushes.DarkRed;
                HighlightRange         = true;
                HighlightOpacity       = 0.2;
                ShowDayStartArrows     = false;
                ShowRangeStartArrows   = false;
                SessionStartUpTag      = "SessionStartUp";
                SessionStartDownTag    = "SessionStartDown";
                SessionStartUpColor    = Brushes.LightGreen;
                SessionStartDownColor  = Brushes.Pink;
                ShowMid                = false;
                MiddleRangeMultiplier  = 0.5;
                ShowQuarters           = false;
                QuarterRangeMultiplier = 0.25;
                ShowTarget1            = false;
                Target1Multiplier      = 1.0;
                ShowTarget2            = false;
                Target2Multiplier      = 2.0;

                ShowSocials = true;
                Author = "TradeSaber - Built With Grok";
                Version = "Version 1.3 // April 2025";
                YouTube = "https://youtu.be/jUYT-Erzc_8";
                Discord = "https://Discord.gg/2YU9GDme8j";
                TradeSaber = "https://tradesaber.com/predator-guide/";

                AddPlot(new Stroke(Brushes.SteelBlue), PlotStyle.Line, "ORHigh");
                AddPlot(new Stroke(Brushes.DarkOrange), PlotStyle.Line, "ORLow");
                AddPlot(new Stroke(Brushes.Transparent), PlotStyle.Dot, "Signal");
                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Line, "ORMiddle");
                AddPlot(new Stroke(Brushes.LightBlue), PlotStyle.Line, "ORTarget1");
                AddPlot(new Stroke(Brushes.Cyan), PlotStyle.Line, "ORTarget2");
                AddPlot(new Stroke(Brushes.LightSalmon), PlotStyle.Line, "ORTargetMinus1");
                AddPlot(new Stroke(Brushes.Salmon), PlotStyle.Line, "ORTargetMinus2");
                AddPlot(new Stroke(Brushes.DimGray), PlotStyle.Line, "ORLowerTargetQuarter");
                AddPlot(new Stroke(Brushes.DimGray), PlotStyle.Line, "ORUpperTargetQuarter");
            }
            else if (State == State.Configure)
            {
                sessionHigh = double.MinValue;
                sessionLow = double.MaxValue;
                isInRange = false;
                highSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                lowSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                middleSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                target1Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
                target2Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
                targetMinus1Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
                targetMinus2Series = new Series<double>(this, MaximumBarsLookBack.Infinite);
                targeLowerQuarterSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                targetUpperQuarterSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                lastBarDate = DateTime.MinValue;
            }
            else if (State == State.DataLoaded)
            {
                if (ChartControl != null)
                {
                    axisColor = ChartControl.Properties.AxisPen.Brush;
                }
            }
            else if (State == State.Historical)
            {
                #region TradeSaber Socials
                if (ShowSocials)
                {
                    if (UserControlCollection.Contains(myGrid29))
                        return;

                    Dispatcher.InvokeAsync(() =>
                    {
                        myGrid29 = new System.Windows.Controls.Grid
                        {
                            Name = "MyCustomGrid",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Bottom
                        };

                        myGrid29.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
                        myGrid29.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
                        myGrid29.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
                        myGrid29.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

                        YouTubeButton = new System.Windows.Controls.Button
                        {
                            Name = "YoutubeButton",
                            Content = "Youtube",
                            Foreground = Brushes.White,
                            Background = Brushes.Red,
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            FontSize = 12,
                            Height = 18,
                            Width = 45
                        };

                        DiscordButton = new System.Windows.Controls.Button
                        {
                            Name = "DiscordButton",
                            Content = "Discord",
                            Foreground = Brushes.White,
                            Background = Brushes.RoyalBlue,
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            FontSize = 12,
                            Height = 18,
                            Width = 45
                        };

                        tradeSaberButton = new System.Windows.Controls.Button
                        {
                            Name = "TradeSaberButton",
                            Content = "TradeSaber",
                            Foreground = Brushes.White,
                            Background = Brushes.DarkOrange,
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            FontSize = 12,
                            Height = 18,
                            Width = 45
                        };

                        closeSocialsButton = new System.Windows.Controls.Button
                        {
                            Name = "CloseSocialsButton",
                            Content = "❌",
                            Foreground = axisColor,
                            Background = Brushes.Transparent,
                            BorderBrush = Brushes.Transparent,
                            HorizontalContentAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(0),
                            Padding = new Thickness(0),
                            FontSize = 12,
                            Height = 18,
                            Width = 15
                        };

                        YouTubeButton.Click += OnButtonClick;
                        DiscordButton.Click += OnButtonClick;
                        tradeSaberButton.Click += OnButtonClick;
                        closeSocialsButton.Click += OnButtonClick;

                        System.Windows.Controls.Grid.SetColumn(YouTubeButton, 0);
                        System.Windows.Controls.Grid.SetColumn(DiscordButton, 1);
                        System.Windows.Controls.Grid.SetColumn(tradeSaberButton, 2);
                        System.Windows.Controls.Grid.SetColumn(closeSocialsButton, 3);

                        myGrid29.Children.Add(YouTubeButton);
                        myGrid29.Children.Add(DiscordButton);
                        myGrid29.Children.Add(tradeSaberButton);
                        myGrid29.Children.Add(closeSocialsButton);

                        UserControlCollection.Add(myGrid29);
                    });
                }
                #endregion
            }
            else if (State == State.Terminated)
            {
                #region TradeSaber Socials
                if (ShowSocials)
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        if (myGrid29 != null)
                        {
                            if (YouTubeButton != null)
                            {
                                myGrid29.Children.Remove(YouTubeButton);
                                YouTubeButton.Click -= OnButtonClick;
                                YouTubeButton = null;
                            }

                            if (DiscordButton != null)
                            {
                                myGrid29.Children.Remove(DiscordButton);
                                DiscordButton.Click -= OnButtonClick;
                                DiscordButton = null;
                            }

                            if (tradeSaberButton != null)
                            {
                                myGrid29.Children.Remove(tradeSaberButton);
                                tradeSaberButton.Click -= OnButtonClick;
                                tradeSaberButton = null;
                            }

                            if (closeSocialsButton != null)
                            {
                                myGrid29.Children.Remove(closeSocialsButton);
                                closeSocialsButton.Click -= OnButtonClick;
                                closeSocialsButton = null;
                            }
                        }
                    });
                }
                #endregion
            }
        }

        #region tradeSaber Socials
        private void OnButtonClick(object sender, RoutedEventArgs rea)
        {
            if (sender is System.Windows.Controls.Button button && ShowSocials)
            {
                if (button == YouTubeButton && button.Name == "YoutubeButton" && button.Content == "Youtube")
                {
                    System.Diagnostics.Process.Start(YouTube);
                }
                else if (button == DiscordButton && button.Name == "DiscordButton" && button.Content == "Discord")
                {
                    System.Diagnostics.Process.Start(Discord);
                }
                else if (button == tradeSaberButton && button.Name == "TradeSaberButton" && button.Content == "TradeSaber")
                {
                    System.Diagnostics.Process.Start(TradeSaber);
                }
                else if (button == closeSocialsButton && button.Name == "CloseSocialsButton" && button.Content == "❌")
                {
                    YouTubeButton.Visibility = Visibility.Collapsed;
                    DiscordButton.Visibility = Visibility.Collapsed;
                    tradeSaberButton.Visibility = Visibility.Collapsed;
                    closeSocialsButton.Visibility = Visibility.Collapsed;
                }
            }
        }
        #endregion

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return Name;
                else
                    return "ORB - TradeSaber";
            }
        }

        protected override void OnBarUpdate()
        {
            TimeZoneInfo selectedTimeZone;
            try
            {
                switch (TimeZoneSelection)
                {
                    case "Local": selectedTimeZone = TimeZoneInfo.Local; break;
                    case "EST": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); break;
                    case "CST": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); break;
                    case "MST": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time"); break;
                    case "PST": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"); break;
                    case "UTC": selectedTimeZone = TimeZoneInfo.Utc; break;
                    case "GMT": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"); break;
                    case "CET": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); break;
                    case "EET": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"); break;
                    case "JST": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"); break;
                    case "AEST": selectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"); break;
                    default: selectedTimeZone = TimeZoneInfo.Local; break;
                }
            }
            catch (TimeZoneNotFoundException)
            {
                selectedTimeZone = TimeZoneInfo.Local;
                Print("Warning: Selected time zone not found. Defaulting to local system timezone.");
            }

            DateTime barTime = Time[0];
            DateTime adjustedTime = TimeZoneInfo.ConvertTime(barTime, TimeZoneInfo.Local, selectedTimeZone);

            adjustedStartTime = new DateTime(adjustedTime.Year, adjustedTime.Month, adjustedTime.Day, StartTime.Hour, StartTime.Minute, StartTime.Second);
            adjustedEndTime = new DateTime(adjustedTime.Year, adjustedTime.Month, adjustedTime.Day, EndTime.Hour, EndTime.Minute, EndTime.Second);
            DateTime endOfDay = adjustedTime.Date.AddDays(1);

            if (CurrentBar > 0)
            {
                DateTime previousAdjustedTime = TimeZoneInfo.ConvertTime(Time[1], TimeZoneInfo.Local, selectedTimeZone);
                if (adjustedTime.Date > previousAdjustedTime.Date)
                {
                    sessionHigh = double.MinValue;
                    sessionLow = double.MaxValue;
                    isInRange = false;

                    if (ShowDayStartArrows)
                    {
                        Draw.ArrowUp(this, SessionStartUpTag + CurrentBar, false, 0, Low[0], SessionStartUpColor);
                        Draw.ArrowDown(this, SessionStartDownTag + CurrentBar, false, 0, High[0], SessionStartDownColor);
                    }
                }
            }

            if (adjustedTime == adjustedStartTime && ShowRangeStartArrows)
            {
                Draw.ArrowUp(this, SessionStartUpTag + CurrentBar, false, 0, Low[0], SessionStartUpColor);
                Draw.ArrowDown(this, SessionStartDownTag + CurrentBar, false, 0, High[0], SessionStartDownColor);
            }

            if (adjustedTime >= adjustedStartTime && adjustedTime <= adjustedEndTime)
            {
                isInRange = true;
                sessionHigh = Math.Max(sessionHigh, High[0]);
                sessionLow = Math.Min(sessionLow, Low[0]);
                highSeries[0] = sessionHigh;
                lowSeries[0] = sessionLow;

                if (HighlightRange && sessionHigh != double.MinValue && sessionLow != double.MaxValue)
                {
                    Draw.Region(this, "ORHighlight" + CurrentBar, adjustedStartTime, Time[0], highSeries, lowSeries, null, Brushes.Gray, (int)(HighlightOpacity * 100));
                }
            }
            else if (adjustedTime > adjustedEndTime && isInRange)
            {
                isInRange = false;
                if (HighlightRange && sessionHigh != double.MinValue && sessionLow != double.MaxValue)
                {
                    Draw.Region(this, "ORHighlight" + CurrentBar, adjustedStartTime, adjustedEndTime, highSeries, lowSeries, null, Brushes.Gray, (int)(HighlightOpacity * 100));
                }
            }

            if (adjustedTime >= adjustedStartTime && adjustedTime < endOfDay && sessionHigh != double.MinValue && sessionLow != double.MaxValue)
            {
                Values[0][0] = sessionHigh;
                Values[1][0] = sessionLow;

                double range = sessionHigh - sessionLow;

                if (ShowMid)
                {
                    Values[3][0] = sessionLow + (range * MiddleRangeMultiplier);
                    middleSeries[0] = Values[3][0];
                }

                if (ShowTarget1)
                {
                    Values[4][0] = sessionHigh + (range * Target1Multiplier);
                    Values[6][0] = sessionLow - (range * Target1Multiplier);
                    target1Series[0] = Values[4][0];
                    targetMinus1Series[0] = Values[6][0];
                }

                if (ShowTarget2)
                {
                    Values[5][0] = sessionHigh + (range * Target2Multiplier);
                    Values[7][0] = sessionLow - (range * Target2Multiplier);
                    target2Series[0] = Values[5][0];
                    targetMinus2Series[0] = Values[7][0];
                }

                if (ShowQuarters)
                {
                    Values[8][0] = sessionLow + (range * QuarterRangeMultiplier);
                    Values[9][0] = sessionLow + (range * (1 - QuarterRangeMultiplier));
                    targeLowerQuarterSeries[0] = Values[8][0];
                    targetUpperQuarterSeries[0] = Values[9][0];
                }
            }

            Values[2][0] = 0;

            if (CurrentBar >= 1)
            {
                double priorClose = Closes[0][1];
                double currentClose = Close[0];

                if (!isInRange)
                {
                    // 1. Above Upper and Below Lower (+1/-1)
                    if (priorClose <= sessionHigh && currentClose > sessionHigh)
                    {
                        Values[2][0] = 1;
                        if (ShowOuterArrows)
                            Draw.ArrowUp(this, AboveUpperTag + CurrentBar, false, 0, Low[0], AboveUpperColor);
                    }
                    else if (priorClose >= sessionLow && currentClose < sessionLow)
                    {
                        Values[2][0] = -1;
                        if (ShowOuterArrows)
                            Draw.ArrowDown(this, BelowLowerTag + CurrentBar, false, 0, High[0], BelowLowerColor);
                    }

                    // 2. Below Upper and Above Lower (+2/-2)
                    if (priorClose <= sessionLow && currentClose > sessionLow && currentClose < sessionHigh)
                    {
                        Values[2][0] = 2;
                        if (ShowInnerArrows)
                            Draw.ArrowUp(this, AboveLowerTag + CurrentBar, false, 0, Low[0], AboveLowerColor);
                    }
                    else if (priorClose >= sessionHigh && currentClose < sessionHigh && currentClose > sessionLow)
                    {
                        Values[2][0] = -2;
                        if (ShowInnerArrows)
                            Draw.ArrowDown(this, BelowUpperTag + CurrentBar, false, 0, High[0], BelowUpperColor);
                    }

                    // 3. Midline (+3/-3)
                    if (ShowMid && Values[3][0] != 0)
                    {
                        if (priorClose <= Values[3][0] && currentClose > Values[3][0])
                        {
                            Values[2][0] = 3;
                            if (ShowMidlineArrows)
                                Draw.ArrowUp(this, AboveMidlineTag + CurrentBar, false, 0, Low[0], AboveMidlineColor);
                        }
                        else if (priorClose >= Values[3][0] && currentClose < Values[3][0])
                        {
                            Values[2][0] = -3;
                            if (ShowMidlineArrows)
                                Draw.ArrowDown(this, BelowMidlineTag + CurrentBar, false, 0, High[0], BelowMidlineColor);
                        }
                    }

                    // 4. Upper Quarter (+4/-4)
                    if (ShowQuarters && Values[9][0] != 0)
                    {
                        if (priorClose <= Values[9][0] && currentClose > Values[9][0])
                        {
                            Values[2][0] = 4;
                            if (ShowUpperQuarterArrows)
                                Draw.ArrowUp(this, AboveUpperQuarterTag + CurrentBar, false, 0, Low[0], AboveUpperQuarterColor);
                        }
                        else if (priorClose >= Values[9][0] && currentClose < Values[9][0])
                        {
                            Values[2][0] = -4;
                            if (ShowUpperQuarterArrows)
                                Draw.ArrowDown(this, BelowUpperQuarterTag + CurrentBar, false, 0, High[0], BelowUpperQuarterColor);
                        }
                    }

                    // 5. Lower Quarter (+5/-5)
                    if (ShowQuarters && Values[8][0] != 0)
                    {
                        if (priorClose <= Values[8][0] && currentClose > Values[8][0])
                        {
                            Values[2][0] = 5;
                            if (ShowLowerQuarterArrows)
                                Draw.ArrowUp(this, AboveLowerQuarterTag + CurrentBar, false, 0, Low[0], AboveLowerQuarterColor);
                        }
                        else if (priorClose >= Values[8][0] && currentClose < Values[8][0])
                        {
                            Values[2][0] = -5;
                            if (ShowLowerQuarterArrows)
                                Draw.ArrowDown(this, BelowLowerQuarterTag + CurrentBar, false, 0, High[0], BelowLowerQuarterColor);
                        }
                    }

                    // 6. Above/Below Upper Target 1 (+6/-6)
                    if (ShowTarget1 && Values[4][0] != 0)
                    {
                        if (priorClose <= Values[4][0] && currentClose > Values[4][0])
                        {
                            Values[2][0] = 6;
                            if (ShowTarget1UpperArrows)
                                Draw.ArrowUp(this, AboveTarget1UpperTag + CurrentBar, false, 0, Low[0], AboveTarget1UpperColor);
                        }
                        else if (priorClose >= Values[4][0] && currentClose < Values[4][0])
                        {
                            Values[2][0] = -6;
                            if (ShowTarget1UpperArrows)
                                Draw.ArrowDown(this, BelowTarget1UpperTag + CurrentBar, false, 0, High[0], BelowTarget1UpperColor);
                        }
                    }

                    // 7. Below/Above Lower Target 1 (+7/-7)
                    if (ShowTarget1 && Values[6][0] != 0)
                    {
                        if (priorClose >= Values[6][0] && currentClose < Values[6][0])
                        {
                            Values[2][0] = -7;
                            if (ShowTarget1LowerArrows)
                                Draw.ArrowDown(this, BelowTarget1LowerTag + CurrentBar, false, 0, High[0], BelowTarget1LowerColor);
                        }
                        else if (priorClose <= Values[6][0] && currentClose > Values[6][0])
                        {
                            Values[2][0] = 7;
                            if (ShowTarget1LowerArrows)
                                Draw.ArrowUp(this, AboveTarget1LowerTag + CurrentBar, false, 0, Low[0], AboveTarget1LowerColor);
                        }
                    }

                    // 8. Above/Below Upper Target 2 (+8/-8)
                    if (ShowTarget2 && Values[5][0] != 0)
                    {
                        if (priorClose <= Values[5][0] && currentClose > Values[5][0])
                        {
                            Values[2][0] = 8;
                            if (ShowTarget2UpperArrows)
                                Draw.ArrowUp(this, AboveTarget2UpperTag + CurrentBar, false, 0, Low[0], AboveTarget2UpperColor);
                        }
                        else if (priorClose >= Values[5][0] && currentClose < Values[5][0])
                        {
                            Values[2][0] = -8;
                            if (ShowTarget2UpperArrows)
                                Draw.ArrowDown(this, BelowTarget2UpperTag + CurrentBar, false, 0, High[0], BelowTarget2UpperColor);
                        }
                    }

                    // 9. Above/Below Lower Target 2 (+9/-9)
                    if (ShowTarget2 && Values[7][0] != 0)
                    {
                        if (priorClose <= Values[7][0] && currentClose > Values[7][0])
                        {
                            Values[2][0] = 9;
                            if (ShowTarget2LowerArrows)
                                Draw.ArrowUp(this, AboveTarget2LowerTag + CurrentBar, false, 0, Low[0], AboveTarget2LowerColor);
                        }
                        else if (priorClose >= Values[7][0] && currentClose < Values[7][0])
                        {
                            Values[2][0] = -9;
                            if (ShowTarget2LowerArrows)
                                Draw.ArrowDown(this, BelowTarget2LowerTag + CurrentBar, false, 0, High[0], BelowTarget2LowerColor);
                        }
                    }
                }
            }

            lastBarDate = adjustedTime.Date;
        }
    }
}

//Draw.RegionHighlightY(this, "ORHighlight" + CurrentBar, true, sessionHigh, sessionLow, Brushes.Gray, Brushes.Gray, 20);

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber.ORB_TradeSaber[] cacheORB_TradeSaber;
		public TradeSaber.ORB_TradeSaber ORB_TradeSaber(bool highlightRange, double highlightOpacity, DateTime startTime, DateTime endTime, string timeZoneSelection, string tZNote, bool showOuterArrows, string aboveUpperTag, string belowLowerTag, Brush aboveUpperColor, Brush belowLowerColor, bool showInnerArrows, string aboveLowerTag, string belowUpperTag, Brush aboveLowerColor, Brush belowUpperColor, bool showMidlineArrows, string aboveMidlineTag, string belowMidlineTag, Brush aboveMidlineColor, Brush belowMidlineColor, bool showUpperQuarterArrows, string aboveUpperQuarterTag, string belowUpperQuarterTag, Brush aboveUpperQuarterColor, Brush belowUpperQuarterColor, bool showLowerQuarterArrows, string aboveLowerQuarterTag, string belowLowerQuarterTag, Brush aboveLowerQuarterColor, Brush belowLowerQuarterColor, bool showTarget1UpperArrows, string aboveTarget1UpperTag, string belowTarget1UpperTag, Brush aboveTarget1UpperColor, Brush belowTarget1UpperColor, bool showTarget1LowerArrows, string belowTarget1LowerTag, string aboveTarget1LowerTag, Brush belowTarget1LowerColor, Brush aboveTarget1LowerColor, bool showTarget2UpperArrows, string aboveTarget2UpperTag, string belowTarget2UpperTag, Brush aboveTarget2UpperColor, Brush belowTarget2UpperColor, bool showTarget2LowerArrows, string aboveTarget2LowerTag, string belowTarget2LowerTag, Brush aboveTarget2LowerColor, Brush belowTarget2LowerColor, bool showDayStartArrows, bool showRangeStartArrows, string sessionStartUpTag, string sessionStartDownTag, Brush sessionStartUpColor, Brush sessionStartDownColor, bool showMid, double middleRangeMultiplier, bool showQuarters, double quarterRangeMultiplier, bool showTarget1, double target1Multiplier, bool showTarget2, double target2Multiplier, bool showSocials, string author, string version, string tradeSaber, string discord, string youTube)
		{
			return ORB_TradeSaber(Input, highlightRange, highlightOpacity, startTime, endTime, timeZoneSelection, tZNote, showOuterArrows, aboveUpperTag, belowLowerTag, aboveUpperColor, belowLowerColor, showInnerArrows, aboveLowerTag, belowUpperTag, aboveLowerColor, belowUpperColor, showMidlineArrows, aboveMidlineTag, belowMidlineTag, aboveMidlineColor, belowMidlineColor, showUpperQuarterArrows, aboveUpperQuarterTag, belowUpperQuarterTag, aboveUpperQuarterColor, belowUpperQuarterColor, showLowerQuarterArrows, aboveLowerQuarterTag, belowLowerQuarterTag, aboveLowerQuarterColor, belowLowerQuarterColor, showTarget1UpperArrows, aboveTarget1UpperTag, belowTarget1UpperTag, aboveTarget1UpperColor, belowTarget1UpperColor, showTarget1LowerArrows, belowTarget1LowerTag, aboveTarget1LowerTag, belowTarget1LowerColor, aboveTarget1LowerColor, showTarget2UpperArrows, aboveTarget2UpperTag, belowTarget2UpperTag, aboveTarget2UpperColor, belowTarget2UpperColor, showTarget2LowerArrows, aboveTarget2LowerTag, belowTarget2LowerTag, aboveTarget2LowerColor, belowTarget2LowerColor, showDayStartArrows, showRangeStartArrows, sessionStartUpTag, sessionStartDownTag, sessionStartUpColor, sessionStartDownColor, showMid, middleRangeMultiplier, showQuarters, quarterRangeMultiplier, showTarget1, target1Multiplier, showTarget2, target2Multiplier, showSocials, author, version, tradeSaber, discord, youTube);
		}

		public TradeSaber.ORB_TradeSaber ORB_TradeSaber(ISeries<double> input, bool highlightRange, double highlightOpacity, DateTime startTime, DateTime endTime, string timeZoneSelection, string tZNote, bool showOuterArrows, string aboveUpperTag, string belowLowerTag, Brush aboveUpperColor, Brush belowLowerColor, bool showInnerArrows, string aboveLowerTag, string belowUpperTag, Brush aboveLowerColor, Brush belowUpperColor, bool showMidlineArrows, string aboveMidlineTag, string belowMidlineTag, Brush aboveMidlineColor, Brush belowMidlineColor, bool showUpperQuarterArrows, string aboveUpperQuarterTag, string belowUpperQuarterTag, Brush aboveUpperQuarterColor, Brush belowUpperQuarterColor, bool showLowerQuarterArrows, string aboveLowerQuarterTag, string belowLowerQuarterTag, Brush aboveLowerQuarterColor, Brush belowLowerQuarterColor, bool showTarget1UpperArrows, string aboveTarget1UpperTag, string belowTarget1UpperTag, Brush aboveTarget1UpperColor, Brush belowTarget1UpperColor, bool showTarget1LowerArrows, string belowTarget1LowerTag, string aboveTarget1LowerTag, Brush belowTarget1LowerColor, Brush aboveTarget1LowerColor, bool showTarget2UpperArrows, string aboveTarget2UpperTag, string belowTarget2UpperTag, Brush aboveTarget2UpperColor, Brush belowTarget2UpperColor, bool showTarget2LowerArrows, string aboveTarget2LowerTag, string belowTarget2LowerTag, Brush aboveTarget2LowerColor, Brush belowTarget2LowerColor, bool showDayStartArrows, bool showRangeStartArrows, string sessionStartUpTag, string sessionStartDownTag, Brush sessionStartUpColor, Brush sessionStartDownColor, bool showMid, double middleRangeMultiplier, bool showQuarters, double quarterRangeMultiplier, bool showTarget1, double target1Multiplier, bool showTarget2, double target2Multiplier, bool showSocials, string author, string version, string tradeSaber, string discord, string youTube)
		{
			if (cacheORB_TradeSaber != null)
				for (int idx = 0; idx < cacheORB_TradeSaber.Length; idx++)
					if (cacheORB_TradeSaber[idx] != null && cacheORB_TradeSaber[idx].HighlightRange == highlightRange && cacheORB_TradeSaber[idx].HighlightOpacity == highlightOpacity && cacheORB_TradeSaber[idx].StartTime == startTime && cacheORB_TradeSaber[idx].EndTime == endTime && cacheORB_TradeSaber[idx].TimeZoneSelection == timeZoneSelection && cacheORB_TradeSaber[idx].TZNote == tZNote && cacheORB_TradeSaber[idx].ShowOuterArrows == showOuterArrows && cacheORB_TradeSaber[idx].AboveUpperTag == aboveUpperTag && cacheORB_TradeSaber[idx].BelowLowerTag == belowLowerTag && cacheORB_TradeSaber[idx].AboveUpperColor == aboveUpperColor && cacheORB_TradeSaber[idx].BelowLowerColor == belowLowerColor && cacheORB_TradeSaber[idx].ShowInnerArrows == showInnerArrows && cacheORB_TradeSaber[idx].AboveLowerTag == aboveLowerTag && cacheORB_TradeSaber[idx].BelowUpperTag == belowUpperTag && cacheORB_TradeSaber[idx].AboveLowerColor == aboveLowerColor && cacheORB_TradeSaber[idx].BelowUpperColor == belowUpperColor && cacheORB_TradeSaber[idx].ShowMidlineArrows == showMidlineArrows && cacheORB_TradeSaber[idx].AboveMidlineTag == aboveMidlineTag && cacheORB_TradeSaber[idx].BelowMidlineTag == belowMidlineTag && cacheORB_TradeSaber[idx].AboveMidlineColor == aboveMidlineColor && cacheORB_TradeSaber[idx].BelowMidlineColor == belowMidlineColor && cacheORB_TradeSaber[idx].ShowUpperQuarterArrows == showUpperQuarterArrows && cacheORB_TradeSaber[idx].AboveUpperQuarterTag == aboveUpperQuarterTag && cacheORB_TradeSaber[idx].BelowUpperQuarterTag == belowUpperQuarterTag && cacheORB_TradeSaber[idx].AboveUpperQuarterColor == aboveUpperQuarterColor && cacheORB_TradeSaber[idx].BelowUpperQuarterColor == belowUpperQuarterColor && cacheORB_TradeSaber[idx].ShowLowerQuarterArrows == showLowerQuarterArrows && cacheORB_TradeSaber[idx].AboveLowerQuarterTag == aboveLowerQuarterTag && cacheORB_TradeSaber[idx].BelowLowerQuarterTag == belowLowerQuarterTag && cacheORB_TradeSaber[idx].AboveLowerQuarterColor == aboveLowerQuarterColor && cacheORB_TradeSaber[idx].BelowLowerQuarterColor == belowLowerQuarterColor && cacheORB_TradeSaber[idx].ShowTarget1UpperArrows == showTarget1UpperArrows && cacheORB_TradeSaber[idx].AboveTarget1UpperTag == aboveTarget1UpperTag && cacheORB_TradeSaber[idx].BelowTarget1UpperTag == belowTarget1UpperTag && cacheORB_TradeSaber[idx].AboveTarget1UpperColor == aboveTarget1UpperColor && cacheORB_TradeSaber[idx].BelowTarget1UpperColor == belowTarget1UpperColor && cacheORB_TradeSaber[idx].ShowTarget1LowerArrows == showTarget1LowerArrows && cacheORB_TradeSaber[idx].BelowTarget1LowerTag == belowTarget1LowerTag && cacheORB_TradeSaber[idx].AboveTarget1LowerTag == aboveTarget1LowerTag && cacheORB_TradeSaber[idx].BelowTarget1LowerColor == belowTarget1LowerColor && cacheORB_TradeSaber[idx].AboveTarget1LowerColor == aboveTarget1LowerColor && cacheORB_TradeSaber[idx].ShowTarget2UpperArrows == showTarget2UpperArrows && cacheORB_TradeSaber[idx].AboveTarget2UpperTag == aboveTarget2UpperTag && cacheORB_TradeSaber[idx].BelowTarget2UpperTag == belowTarget2UpperTag && cacheORB_TradeSaber[idx].AboveTarget2UpperColor == aboveTarget2UpperColor && cacheORB_TradeSaber[idx].BelowTarget2UpperColor == belowTarget2UpperColor && cacheORB_TradeSaber[idx].ShowTarget2LowerArrows == showTarget2LowerArrows && cacheORB_TradeSaber[idx].AboveTarget2LowerTag == aboveTarget2LowerTag && cacheORB_TradeSaber[idx].BelowTarget2LowerTag == belowTarget2LowerTag && cacheORB_TradeSaber[idx].AboveTarget2LowerColor == aboveTarget2LowerColor && cacheORB_TradeSaber[idx].BelowTarget2LowerColor == belowTarget2LowerColor && cacheORB_TradeSaber[idx].ShowDayStartArrows == showDayStartArrows && cacheORB_TradeSaber[idx].ShowRangeStartArrows == showRangeStartArrows && cacheORB_TradeSaber[idx].SessionStartUpTag == sessionStartUpTag && cacheORB_TradeSaber[idx].SessionStartDownTag == sessionStartDownTag && cacheORB_TradeSaber[idx].SessionStartUpColor == sessionStartUpColor && cacheORB_TradeSaber[idx].SessionStartDownColor == sessionStartDownColor && cacheORB_TradeSaber[idx].ShowMid == showMid && cacheORB_TradeSaber[idx].MiddleRangeMultiplier == middleRangeMultiplier && cacheORB_TradeSaber[idx].ShowQuarters == showQuarters && cacheORB_TradeSaber[idx].QuarterRangeMultiplier == quarterRangeMultiplier && cacheORB_TradeSaber[idx].ShowTarget1 == showTarget1 && cacheORB_TradeSaber[idx].Target1Multiplier == target1Multiplier && cacheORB_TradeSaber[idx].ShowTarget2 == showTarget2 && cacheORB_TradeSaber[idx].Target2Multiplier == target2Multiplier && cacheORB_TradeSaber[idx].ShowSocials == showSocials && cacheORB_TradeSaber[idx].Author == author && cacheORB_TradeSaber[idx].Version == version && cacheORB_TradeSaber[idx].TradeSaber == tradeSaber && cacheORB_TradeSaber[idx].Discord == discord && cacheORB_TradeSaber[idx].YouTube == youTube && cacheORB_TradeSaber[idx].EqualsInput(input))
						return cacheORB_TradeSaber[idx];
			return CacheIndicator<TradeSaber.ORB_TradeSaber>(new TradeSaber.ORB_TradeSaber(){ HighlightRange = highlightRange, HighlightOpacity = highlightOpacity, StartTime = startTime, EndTime = endTime, TimeZoneSelection = timeZoneSelection, TZNote = tZNote, ShowOuterArrows = showOuterArrows, AboveUpperTag = aboveUpperTag, BelowLowerTag = belowLowerTag, AboveUpperColor = aboveUpperColor, BelowLowerColor = belowLowerColor, ShowInnerArrows = showInnerArrows, AboveLowerTag = aboveLowerTag, BelowUpperTag = belowUpperTag, AboveLowerColor = aboveLowerColor, BelowUpperColor = belowUpperColor, ShowMidlineArrows = showMidlineArrows, AboveMidlineTag = aboveMidlineTag, BelowMidlineTag = belowMidlineTag, AboveMidlineColor = aboveMidlineColor, BelowMidlineColor = belowMidlineColor, ShowUpperQuarterArrows = showUpperQuarterArrows, AboveUpperQuarterTag = aboveUpperQuarterTag, BelowUpperQuarterTag = belowUpperQuarterTag, AboveUpperQuarterColor = aboveUpperQuarterColor, BelowUpperQuarterColor = belowUpperQuarterColor, ShowLowerQuarterArrows = showLowerQuarterArrows, AboveLowerQuarterTag = aboveLowerQuarterTag, BelowLowerQuarterTag = belowLowerQuarterTag, AboveLowerQuarterColor = aboveLowerQuarterColor, BelowLowerQuarterColor = belowLowerQuarterColor, ShowTarget1UpperArrows = showTarget1UpperArrows, AboveTarget1UpperTag = aboveTarget1UpperTag, BelowTarget1UpperTag = belowTarget1UpperTag, AboveTarget1UpperColor = aboveTarget1UpperColor, BelowTarget1UpperColor = belowTarget1UpperColor, ShowTarget1LowerArrows = showTarget1LowerArrows, BelowTarget1LowerTag = belowTarget1LowerTag, AboveTarget1LowerTag = aboveTarget1LowerTag, BelowTarget1LowerColor = belowTarget1LowerColor, AboveTarget1LowerColor = aboveTarget1LowerColor, ShowTarget2UpperArrows = showTarget2UpperArrows, AboveTarget2UpperTag = aboveTarget2UpperTag, BelowTarget2UpperTag = belowTarget2UpperTag, AboveTarget2UpperColor = aboveTarget2UpperColor, BelowTarget2UpperColor = belowTarget2UpperColor, ShowTarget2LowerArrows = showTarget2LowerArrows, AboveTarget2LowerTag = aboveTarget2LowerTag, BelowTarget2LowerTag = belowTarget2LowerTag, AboveTarget2LowerColor = aboveTarget2LowerColor, BelowTarget2LowerColor = belowTarget2LowerColor, ShowDayStartArrows = showDayStartArrows, ShowRangeStartArrows = showRangeStartArrows, SessionStartUpTag = sessionStartUpTag, SessionStartDownTag = sessionStartDownTag, SessionStartUpColor = sessionStartUpColor, SessionStartDownColor = sessionStartDownColor, ShowMid = showMid, MiddleRangeMultiplier = middleRangeMultiplier, ShowQuarters = showQuarters, QuarterRangeMultiplier = quarterRangeMultiplier, ShowTarget1 = showTarget1, Target1Multiplier = target1Multiplier, ShowTarget2 = showTarget2, Target2Multiplier = target2Multiplier, ShowSocials = showSocials, Author = author, Version = version, TradeSaber = tradeSaber, Discord = discord, YouTube = youTube }, input, ref cacheORB_TradeSaber);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.ORB_TradeSaber ORB_TradeSaber(bool highlightRange, double highlightOpacity, DateTime startTime, DateTime endTime, string timeZoneSelection, string tZNote, bool showOuterArrows, string aboveUpperTag, string belowLowerTag, Brush aboveUpperColor, Brush belowLowerColor, bool showInnerArrows, string aboveLowerTag, string belowUpperTag, Brush aboveLowerColor, Brush belowUpperColor, bool showMidlineArrows, string aboveMidlineTag, string belowMidlineTag, Brush aboveMidlineColor, Brush belowMidlineColor, bool showUpperQuarterArrows, string aboveUpperQuarterTag, string belowUpperQuarterTag, Brush aboveUpperQuarterColor, Brush belowUpperQuarterColor, bool showLowerQuarterArrows, string aboveLowerQuarterTag, string belowLowerQuarterTag, Brush aboveLowerQuarterColor, Brush belowLowerQuarterColor, bool showTarget1UpperArrows, string aboveTarget1UpperTag, string belowTarget1UpperTag, Brush aboveTarget1UpperColor, Brush belowTarget1UpperColor, bool showTarget1LowerArrows, string belowTarget1LowerTag, string aboveTarget1LowerTag, Brush belowTarget1LowerColor, Brush aboveTarget1LowerColor, bool showTarget2UpperArrows, string aboveTarget2UpperTag, string belowTarget2UpperTag, Brush aboveTarget2UpperColor, Brush belowTarget2UpperColor, bool showTarget2LowerArrows, string aboveTarget2LowerTag, string belowTarget2LowerTag, Brush aboveTarget2LowerColor, Brush belowTarget2LowerColor, bool showDayStartArrows, bool showRangeStartArrows, string sessionStartUpTag, string sessionStartDownTag, Brush sessionStartUpColor, Brush sessionStartDownColor, bool showMid, double middleRangeMultiplier, bool showQuarters, double quarterRangeMultiplier, bool showTarget1, double target1Multiplier, bool showTarget2, double target2Multiplier, bool showSocials, string author, string version, string tradeSaber, string discord, string youTube)
		{
			return indicator.ORB_TradeSaber(Input, highlightRange, highlightOpacity, startTime, endTime, timeZoneSelection, tZNote, showOuterArrows, aboveUpperTag, belowLowerTag, aboveUpperColor, belowLowerColor, showInnerArrows, aboveLowerTag, belowUpperTag, aboveLowerColor, belowUpperColor, showMidlineArrows, aboveMidlineTag, belowMidlineTag, aboveMidlineColor, belowMidlineColor, showUpperQuarterArrows, aboveUpperQuarterTag, belowUpperQuarterTag, aboveUpperQuarterColor, belowUpperQuarterColor, showLowerQuarterArrows, aboveLowerQuarterTag, belowLowerQuarterTag, aboveLowerQuarterColor, belowLowerQuarterColor, showTarget1UpperArrows, aboveTarget1UpperTag, belowTarget1UpperTag, aboveTarget1UpperColor, belowTarget1UpperColor, showTarget1LowerArrows, belowTarget1LowerTag, aboveTarget1LowerTag, belowTarget1LowerColor, aboveTarget1LowerColor, showTarget2UpperArrows, aboveTarget2UpperTag, belowTarget2UpperTag, aboveTarget2UpperColor, belowTarget2UpperColor, showTarget2LowerArrows, aboveTarget2LowerTag, belowTarget2LowerTag, aboveTarget2LowerColor, belowTarget2LowerColor, showDayStartArrows, showRangeStartArrows, sessionStartUpTag, sessionStartDownTag, sessionStartUpColor, sessionStartDownColor, showMid, middleRangeMultiplier, showQuarters, quarterRangeMultiplier, showTarget1, target1Multiplier, showTarget2, target2Multiplier, showSocials, author, version, tradeSaber, discord, youTube);
		}

		public Indicators.TradeSaber.ORB_TradeSaber ORB_TradeSaber(ISeries<double> input , bool highlightRange, double highlightOpacity, DateTime startTime, DateTime endTime, string timeZoneSelection, string tZNote, bool showOuterArrows, string aboveUpperTag, string belowLowerTag, Brush aboveUpperColor, Brush belowLowerColor, bool showInnerArrows, string aboveLowerTag, string belowUpperTag, Brush aboveLowerColor, Brush belowUpperColor, bool showMidlineArrows, string aboveMidlineTag, string belowMidlineTag, Brush aboveMidlineColor, Brush belowMidlineColor, bool showUpperQuarterArrows, string aboveUpperQuarterTag, string belowUpperQuarterTag, Brush aboveUpperQuarterColor, Brush belowUpperQuarterColor, bool showLowerQuarterArrows, string aboveLowerQuarterTag, string belowLowerQuarterTag, Brush aboveLowerQuarterColor, Brush belowLowerQuarterColor, bool showTarget1UpperArrows, string aboveTarget1UpperTag, string belowTarget1UpperTag, Brush aboveTarget1UpperColor, Brush belowTarget1UpperColor, bool showTarget1LowerArrows, string belowTarget1LowerTag, string aboveTarget1LowerTag, Brush belowTarget1LowerColor, Brush aboveTarget1LowerColor, bool showTarget2UpperArrows, string aboveTarget2UpperTag, string belowTarget2UpperTag, Brush aboveTarget2UpperColor, Brush belowTarget2UpperColor, bool showTarget2LowerArrows, string aboveTarget2LowerTag, string belowTarget2LowerTag, Brush aboveTarget2LowerColor, Brush belowTarget2LowerColor, bool showDayStartArrows, bool showRangeStartArrows, string sessionStartUpTag, string sessionStartDownTag, Brush sessionStartUpColor, Brush sessionStartDownColor, bool showMid, double middleRangeMultiplier, bool showQuarters, double quarterRangeMultiplier, bool showTarget1, double target1Multiplier, bool showTarget2, double target2Multiplier, bool showSocials, string author, string version, string tradeSaber, string discord, string youTube)
		{
			return indicator.ORB_TradeSaber(input, highlightRange, highlightOpacity, startTime, endTime, timeZoneSelection, tZNote, showOuterArrows, aboveUpperTag, belowLowerTag, aboveUpperColor, belowLowerColor, showInnerArrows, aboveLowerTag, belowUpperTag, aboveLowerColor, belowUpperColor, showMidlineArrows, aboveMidlineTag, belowMidlineTag, aboveMidlineColor, belowMidlineColor, showUpperQuarterArrows, aboveUpperQuarterTag, belowUpperQuarterTag, aboveUpperQuarterColor, belowUpperQuarterColor, showLowerQuarterArrows, aboveLowerQuarterTag, belowLowerQuarterTag, aboveLowerQuarterColor, belowLowerQuarterColor, showTarget1UpperArrows, aboveTarget1UpperTag, belowTarget1UpperTag, aboveTarget1UpperColor, belowTarget1UpperColor, showTarget1LowerArrows, belowTarget1LowerTag, aboveTarget1LowerTag, belowTarget1LowerColor, aboveTarget1LowerColor, showTarget2UpperArrows, aboveTarget2UpperTag, belowTarget2UpperTag, aboveTarget2UpperColor, belowTarget2UpperColor, showTarget2LowerArrows, aboveTarget2LowerTag, belowTarget2LowerTag, aboveTarget2LowerColor, belowTarget2LowerColor, showDayStartArrows, showRangeStartArrows, sessionStartUpTag, sessionStartDownTag, sessionStartUpColor, sessionStartDownColor, showMid, middleRangeMultiplier, showQuarters, quarterRangeMultiplier, showTarget1, target1Multiplier, showTarget2, target2Multiplier, showSocials, author, version, tradeSaber, discord, youTube);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.ORB_TradeSaber ORB_TradeSaber(bool highlightRange, double highlightOpacity, DateTime startTime, DateTime endTime, string timeZoneSelection, string tZNote, bool showOuterArrows, string aboveUpperTag, string belowLowerTag, Brush aboveUpperColor, Brush belowLowerColor, bool showInnerArrows, string aboveLowerTag, string belowUpperTag, Brush aboveLowerColor, Brush belowUpperColor, bool showMidlineArrows, string aboveMidlineTag, string belowMidlineTag, Brush aboveMidlineColor, Brush belowMidlineColor, bool showUpperQuarterArrows, string aboveUpperQuarterTag, string belowUpperQuarterTag, Brush aboveUpperQuarterColor, Brush belowUpperQuarterColor, bool showLowerQuarterArrows, string aboveLowerQuarterTag, string belowLowerQuarterTag, Brush aboveLowerQuarterColor, Brush belowLowerQuarterColor, bool showTarget1UpperArrows, string aboveTarget1UpperTag, string belowTarget1UpperTag, Brush aboveTarget1UpperColor, Brush belowTarget1UpperColor, bool showTarget1LowerArrows, string belowTarget1LowerTag, string aboveTarget1LowerTag, Brush belowTarget1LowerColor, Brush aboveTarget1LowerColor, bool showTarget2UpperArrows, string aboveTarget2UpperTag, string belowTarget2UpperTag, Brush aboveTarget2UpperColor, Brush belowTarget2UpperColor, bool showTarget2LowerArrows, string aboveTarget2LowerTag, string belowTarget2LowerTag, Brush aboveTarget2LowerColor, Brush belowTarget2LowerColor, bool showDayStartArrows, bool showRangeStartArrows, string sessionStartUpTag, string sessionStartDownTag, Brush sessionStartUpColor, Brush sessionStartDownColor, bool showMid, double middleRangeMultiplier, bool showQuarters, double quarterRangeMultiplier, bool showTarget1, double target1Multiplier, bool showTarget2, double target2Multiplier, bool showSocials, string author, string version, string tradeSaber, string discord, string youTube)
		{
			return indicator.ORB_TradeSaber(Input, highlightRange, highlightOpacity, startTime, endTime, timeZoneSelection, tZNote, showOuterArrows, aboveUpperTag, belowLowerTag, aboveUpperColor, belowLowerColor, showInnerArrows, aboveLowerTag, belowUpperTag, aboveLowerColor, belowUpperColor, showMidlineArrows, aboveMidlineTag, belowMidlineTag, aboveMidlineColor, belowMidlineColor, showUpperQuarterArrows, aboveUpperQuarterTag, belowUpperQuarterTag, aboveUpperQuarterColor, belowUpperQuarterColor, showLowerQuarterArrows, aboveLowerQuarterTag, belowLowerQuarterTag, aboveLowerQuarterColor, belowLowerQuarterColor, showTarget1UpperArrows, aboveTarget1UpperTag, belowTarget1UpperTag, aboveTarget1UpperColor, belowTarget1UpperColor, showTarget1LowerArrows, belowTarget1LowerTag, aboveTarget1LowerTag, belowTarget1LowerColor, aboveTarget1LowerColor, showTarget2UpperArrows, aboveTarget2UpperTag, belowTarget2UpperTag, aboveTarget2UpperColor, belowTarget2UpperColor, showTarget2LowerArrows, aboveTarget2LowerTag, belowTarget2LowerTag, aboveTarget2LowerColor, belowTarget2LowerColor, showDayStartArrows, showRangeStartArrows, sessionStartUpTag, sessionStartDownTag, sessionStartUpColor, sessionStartDownColor, showMid, middleRangeMultiplier, showQuarters, quarterRangeMultiplier, showTarget1, target1Multiplier, showTarget2, target2Multiplier, showSocials, author, version, tradeSaber, discord, youTube);
		}

		public Indicators.TradeSaber.ORB_TradeSaber ORB_TradeSaber(ISeries<double> input , bool highlightRange, double highlightOpacity, DateTime startTime, DateTime endTime, string timeZoneSelection, string tZNote, bool showOuterArrows, string aboveUpperTag, string belowLowerTag, Brush aboveUpperColor, Brush belowLowerColor, bool showInnerArrows, string aboveLowerTag, string belowUpperTag, Brush aboveLowerColor, Brush belowUpperColor, bool showMidlineArrows, string aboveMidlineTag, string belowMidlineTag, Brush aboveMidlineColor, Brush belowMidlineColor, bool showUpperQuarterArrows, string aboveUpperQuarterTag, string belowUpperQuarterTag, Brush aboveUpperQuarterColor, Brush belowUpperQuarterColor, bool showLowerQuarterArrows, string aboveLowerQuarterTag, string belowLowerQuarterTag, Brush aboveLowerQuarterColor, Brush belowLowerQuarterColor, bool showTarget1UpperArrows, string aboveTarget1UpperTag, string belowTarget1UpperTag, Brush aboveTarget1UpperColor, Brush belowTarget1UpperColor, bool showTarget1LowerArrows, string belowTarget1LowerTag, string aboveTarget1LowerTag, Brush belowTarget1LowerColor, Brush aboveTarget1LowerColor, bool showTarget2UpperArrows, string aboveTarget2UpperTag, string belowTarget2UpperTag, Brush aboveTarget2UpperColor, Brush belowTarget2UpperColor, bool showTarget2LowerArrows, string aboveTarget2LowerTag, string belowTarget2LowerTag, Brush aboveTarget2LowerColor, Brush belowTarget2LowerColor, bool showDayStartArrows, bool showRangeStartArrows, string sessionStartUpTag, string sessionStartDownTag, Brush sessionStartUpColor, Brush sessionStartDownColor, bool showMid, double middleRangeMultiplier, bool showQuarters, double quarterRangeMultiplier, bool showTarget1, double target1Multiplier, bool showTarget2, double target2Multiplier, bool showSocials, string author, string version, string tradeSaber, string discord, string youTube)
		{
			return indicator.ORB_TradeSaber(input, highlightRange, highlightOpacity, startTime, endTime, timeZoneSelection, tZNote, showOuterArrows, aboveUpperTag, belowLowerTag, aboveUpperColor, belowLowerColor, showInnerArrows, aboveLowerTag, belowUpperTag, aboveLowerColor, belowUpperColor, showMidlineArrows, aboveMidlineTag, belowMidlineTag, aboveMidlineColor, belowMidlineColor, showUpperQuarterArrows, aboveUpperQuarterTag, belowUpperQuarterTag, aboveUpperQuarterColor, belowUpperQuarterColor, showLowerQuarterArrows, aboveLowerQuarterTag, belowLowerQuarterTag, aboveLowerQuarterColor, belowLowerQuarterColor, showTarget1UpperArrows, aboveTarget1UpperTag, belowTarget1UpperTag, aboveTarget1UpperColor, belowTarget1UpperColor, showTarget1LowerArrows, belowTarget1LowerTag, aboveTarget1LowerTag, belowTarget1LowerColor, aboveTarget1LowerColor, showTarget2UpperArrows, aboveTarget2UpperTag, belowTarget2UpperTag, aboveTarget2UpperColor, belowTarget2UpperColor, showTarget2LowerArrows, aboveTarget2LowerTag, belowTarget2LowerTag, aboveTarget2LowerColor, belowTarget2LowerColor, showDayStartArrows, showRangeStartArrows, sessionStartUpTag, sessionStartDownTag, sessionStartUpColor, sessionStartDownColor, showMid, middleRangeMultiplier, showQuarters, quarterRangeMultiplier, showTarget1, target1Multiplier, showTarget2, target2Multiplier, showSocials, author, version, tradeSaber, discord, youTube);
		}
	}
}

#endregion
