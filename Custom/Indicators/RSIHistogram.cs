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
using System.Globalization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    [CategoryOrder("RSI Histogram", 1)]
    [CategoryOrder("Appearance", 2)]
    public class RSIHistogram : Indicator
    {
        public static RSIHistogramColorScheme DefaultColorScheme = RSIHistogramColorScheme.PreviousBar;
        public static Brush DefaultPositiveBarColor = Brushes.LimeGreen;
        public static Brush DefaultNegativeBarColor = Brushes.Red;

        public static double DefaultUpperLevel = 20.0;
        public static double DefaultLowerLevel = -20.0;

        public static bool DefaultShowLevels = true;
        public static Stroke DefaultLevelStroke = new Stroke(Brushes.Yellow, DashStyleHelper.Solid, 2);

        private RSI RSIValue;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Credit to @BestForexMethod on Twitter";
                Name = "RSI Histogram";

                Calculate = Calculate.OnPriceChange;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                ArePlotsConfigurable = false;
                AreLinesConfigurable = false;

                RSIPeriod = 8;
                RSISmoothing = 3;
                RSIModifier = 1.5;

                ColorScheme = DefaultColorScheme;
                PositiveBarColor = DefaultPositiveBarColor;
                NegativeBarColor = DefaultNegativeBarColor;

                ShowLevels = DefaultShowLevels;

                UpperLevel = DefaultUpperLevel;
                LowerLevel = DefaultLowerLevel;

                UpperLevelStroke = DefaultLevelStroke;
                LowerLevelStroke = DefaultLevelStroke;

                AddPlot(new Stroke(Brushes.Transparent, 0), PlotStyle.Bar, "Histogram");
            }
            else if (State == State.Configure)
            {
                if (ShowLevels)
                {
                    AddLine(UpperLevelStroke, UpperLevel, "Upper Level");
                    AddLine(LowerLevelStroke, LowerLevel, "Lower Level");
                }
            }
            else if (State == State.DataLoaded)
            {
                RSIValue = RSI(RSIPeriod, RSISmoothing);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 2)
                return;
            
            double rsiHistogramValue = (RSIValue[0] - 50) * RSIModifier;

            // If no change in value, use previous bar color, regardless of color scheme.
            if (rsiHistogramValue == Histogram[1])
                PlotBrushes[0][0] = PlotBrushes[0][1];
            else
            {
                if ((ColorScheme == RSIHistogramColorScheme.PositiveNegative && rsiHistogramValue > 0.0) ||
                (ColorScheme == RSIHistogramColorScheme.PreviousBar && rsiHistogramValue > Histogram[1]))
                    PlotBrushes[0][0] = PositiveBarColor;
                else
                    PlotBrushes[0][0] = NegativeBarColor;
            }

            Histogram[0] = rsiHistogramValue;
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            foreach (Plot plot in Plots)
            {
                plot.Width = (float)chartControl.GetBarPaintWidth(ChartBars);
            }
        }

        public override string DisplayName
        {
            get { return Name; }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "RSI Period", Description = "Period used in RSI calculation", Order = 1, GroupName = "RSI Histogram")]
        public int RSIPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "RSI Smoothing", Description = "Smoothing used in RSI calculation", Order = 2, GroupName = "RSI Histogram")]
        public int RSISmoothing
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "RSI Modifier", Description = "Modifier used to scale RSI calculation", Order = 3, GroupName = "RSI Histogram")]
        public double RSIModifier
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")]
        [TypeConverter(typeof(RSIHistogramColorSchemeConverter))]
        [Display(Name = "Color Scheme", Description = "RSI histogram coloring scheme", Order = 4, GroupName = "RSI Histogram")]
        public RSIHistogramColorScheme ColorScheme
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Positive Bar Color", Description = "Color of bars with a positive value", Order = 5, GroupName = "RSI Histogram")]
        public Brush PositiveBarColor
        { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Negative Bar Color", Description = "Color of bars with a negative value", Order = 6, GroupName = "RSI Histogram")]
        public Brush NegativeBarColor
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Levels", Order = 1, GroupName = "Levels")]
        public bool ShowLevels
        { get; set; }

        [NinjaScriptProperty]
        [Range(double.MinValue, double.MaxValue)]
        [Display(Name = "Upper Level", Description = "Upper level in histogram", Order = 2, GroupName = "Levels")]
        public double UpperLevel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Upper Level Style", Description = "Upper level displayed in histogram", Order = 3, GroupName = "Levels")]
        public Stroke UpperLevelStroke
        { get; set; }

        [NinjaScriptProperty]
        [Range(double.MinValue, double.MaxValue)]
        [Display(Name = "Lower Level", Description = "Lower level in histogram", Order = 4, GroupName = "Levels")]
        public double LowerLevel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Lower Level Style", Description = "Lower level displayed in histogram", Order = 5, GroupName = "Levels")]
        public Stroke LowerLevelStroke
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Histogram
        {
            get { return Values[0]; }
        }
        #endregion
    }
}

public enum RSIHistogramColorScheme
{
    PositiveNegative,
    PreviousBar
}

public class RSIHistogramColorSchemeConverter : TypeConverter
{
    private const string POSITIVE_NEGATIVE = "Above / Below 0";
    private const string PREVIOUS_BAR = "Based on Previous Bar";

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        List<string> values = new List<string>() { POSITIVE_NEGATIVE, PREVIOUS_BAR };
        return new StandardValuesCollection(values);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        switch (value.ToString())
        {
            case POSITIVE_NEGATIVE:
                return RSIHistogramColorScheme.PositiveNegative;
            case PREVIOUS_BAR:
                return RSIHistogramColorScheme.PreviousBar;
        }
        return RSIHistogramColorScheme.PreviousBar;
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        RSIHistogramColorScheme enumValue = (RSIHistogramColorScheme)Enum.Parse(typeof(RSIHistogramColorScheme), value.ToString());
        switch (enumValue)
        {
            case RSIHistogramColorScheme.PositiveNegative:
                return POSITIVE_NEGATIVE;
            case RSIHistogramColorScheme.PreviousBar:
                return PREVIOUS_BAR;
        }
        return PREVIOUS_BAR;
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

namespace NinjaTrader.NinjaScript.Strategies
{
    public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
    {
        public Indicators.RSIHistogram RSIHistogram(int rsiPeriod, int rsiSmoothing, double rsiModifier)
        {
            return indicator.RSIHistogram(
                Input,
                rsiPeriod,
                rsiSmoothing,
                rsiModifier,
                Indicators.RSIHistogram.DefaultColorScheme,
                Indicators.RSIHistogram.DefaultPositiveBarColor,
                Indicators.RSIHistogram.DefaultNegativeBarColor,
                Indicators.RSIHistogram.DefaultShowLevels,
                Indicators.RSIHistogram.DefaultUpperLevel,
                Indicators.RSIHistogram.DefaultLevelStroke,
                Indicators.RSIHistogram.DefaultLowerLevel,
                Indicators.RSIHistogram.DefaultLevelStroke
            );
        }
    }
}

namespace NinjaTrader.NinjaScript.Indicators
{
    public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
    {
        public RSIHistogram RSIHistogram(int rsiPeriod, int rsiSmoothing, double rsiModifier)
        {
            return RSIHistogram(
                Input,
                rsiPeriod,
                rsiSmoothing,
                rsiModifier,
                Indicators.RSIHistogram.DefaultColorScheme,
                Indicators.RSIHistogram.DefaultPositiveBarColor,
                Indicators.RSIHistogram.DefaultNegativeBarColor,
                Indicators.RSIHistogram.DefaultShowLevels,
                Indicators.RSIHistogram.DefaultUpperLevel,
                Indicators.RSIHistogram.DefaultLevelStroke,
                Indicators.RSIHistogram.DefaultLowerLevel,
                Indicators.RSIHistogram.DefaultLevelStroke
            );
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RSIHistogram[] cacheRSIHistogram;
		public RSIHistogram RSIHistogram(int rSIPeriod, int rSISmoothing, double rSIModifier, RSIHistogramColorScheme colorScheme, Brush positiveBarColor, Brush negativeBarColor, bool showLevels, double upperLevel, Stroke upperLevelStroke, double lowerLevel, Stroke lowerLevelStroke)
		{
			return RSIHistogram(Input, rSIPeriod, rSISmoothing, rSIModifier, colorScheme, positiveBarColor, negativeBarColor, showLevels, upperLevel, upperLevelStroke, lowerLevel, lowerLevelStroke);
		}

		public RSIHistogram RSIHistogram(ISeries<double> input, int rSIPeriod, int rSISmoothing, double rSIModifier, RSIHistogramColorScheme colorScheme, Brush positiveBarColor, Brush negativeBarColor, bool showLevels, double upperLevel, Stroke upperLevelStroke, double lowerLevel, Stroke lowerLevelStroke)
		{
			if (cacheRSIHistogram != null)
				for (int idx = 0; idx < cacheRSIHistogram.Length; idx++)
					if (cacheRSIHistogram[idx] != null && cacheRSIHistogram[idx].RSIPeriod == rSIPeriod && cacheRSIHistogram[idx].RSISmoothing == rSISmoothing && cacheRSIHistogram[idx].RSIModifier == rSIModifier && cacheRSIHistogram[idx].ColorScheme == colorScheme && cacheRSIHistogram[idx].PositiveBarColor == positiveBarColor && cacheRSIHistogram[idx].NegativeBarColor == negativeBarColor && cacheRSIHistogram[idx].ShowLevels == showLevels && cacheRSIHistogram[idx].UpperLevel == upperLevel && cacheRSIHistogram[idx].UpperLevelStroke == upperLevelStroke && cacheRSIHistogram[idx].LowerLevel == lowerLevel && cacheRSIHistogram[idx].LowerLevelStroke == lowerLevelStroke && cacheRSIHistogram[idx].EqualsInput(input))
						return cacheRSIHistogram[idx];
			return CacheIndicator<RSIHistogram>(new RSIHistogram(){ RSIPeriod = rSIPeriod, RSISmoothing = rSISmoothing, RSIModifier = rSIModifier, ColorScheme = colorScheme, PositiveBarColor = positiveBarColor, NegativeBarColor = negativeBarColor, ShowLevels = showLevels, UpperLevel = upperLevel, UpperLevelStroke = upperLevelStroke, LowerLevel = lowerLevel, LowerLevelStroke = lowerLevelStroke }, input, ref cacheRSIHistogram);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSIHistogram RSIHistogram(int rSIPeriod, int rSISmoothing, double rSIModifier, RSIHistogramColorScheme colorScheme, Brush positiveBarColor, Brush negativeBarColor, bool showLevels, double upperLevel, Stroke upperLevelStroke, double lowerLevel, Stroke lowerLevelStroke)
		{
			return indicator.RSIHistogram(Input, rSIPeriod, rSISmoothing, rSIModifier, colorScheme, positiveBarColor, negativeBarColor, showLevels, upperLevel, upperLevelStroke, lowerLevel, lowerLevelStroke);
		}

		public Indicators.RSIHistogram RSIHistogram(ISeries<double> input , int rSIPeriod, int rSISmoothing, double rSIModifier, RSIHistogramColorScheme colorScheme, Brush positiveBarColor, Brush negativeBarColor, bool showLevels, double upperLevel, Stroke upperLevelStroke, double lowerLevel, Stroke lowerLevelStroke)
		{
			return indicator.RSIHistogram(input, rSIPeriod, rSISmoothing, rSIModifier, colorScheme, positiveBarColor, negativeBarColor, showLevels, upperLevel, upperLevelStroke, lowerLevel, lowerLevelStroke);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSIHistogram RSIHistogram(int rSIPeriod, int rSISmoothing, double rSIModifier, RSIHistogramColorScheme colorScheme, Brush positiveBarColor, Brush negativeBarColor, bool showLevels, double upperLevel, Stroke upperLevelStroke, double lowerLevel, Stroke lowerLevelStroke)
		{
			return indicator.RSIHistogram(Input, rSIPeriod, rSISmoothing, rSIModifier, colorScheme, positiveBarColor, negativeBarColor, showLevels, upperLevel, upperLevelStroke, lowerLevel, lowerLevelStroke);
		}

		public Indicators.RSIHistogram RSIHistogram(ISeries<double> input , int rSIPeriod, int rSISmoothing, double rSIModifier, RSIHistogramColorScheme colorScheme, Brush positiveBarColor, Brush negativeBarColor, bool showLevels, double upperLevel, Stroke upperLevelStroke, double lowerLevel, Stroke lowerLevelStroke)
		{
			return indicator.RSIHistogram(input, rSIPeriod, rSISmoothing, rSIModifier, colorScheme, positiveBarColor, negativeBarColor, showLevels, upperLevel, upperLevelStroke, lowerLevel, lowerLevelStroke);
		}
	}
}

#endregion
