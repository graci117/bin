#region Using declarations
using System;
using System.Windows.Media;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.Indicators;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;
using System.Globalization;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class HourlyFibsSlow : Indicator
    {
        // Internal fields that retain the capture period values and time boundaries
        private double periodHigh = double.MinValue;
        private double periodLow  = double.MaxValue;
        private DateTime captureStart = DateTime.MinValue;
        private DateTime captureEnd   = DateTime.MinValue;  // captureStart + 20 minutes (:10)
        private DateTime extensionEnd = DateTime.MinValue;  // captureStart + 1 hour (:50)
        private bool inExtension = false; // flag: true when we are in extension period

        // Computed levels (assigned during extension period)
        private double midline    = 0.0;
        private double upperExt   = 0.0;
        private double lowerExt   = 0.0;
		
		private SharpDX.Direct2D1.SolidColorBrush cachedSolidBrush;
private SharpDX.Direct2D1.StrokeStyle cachedDashStyle;

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Line Color", Order = 1, GroupName = "Appearance")]
        public System.Windows.Media.Brush LineColor { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Line Width", Order = 2, GroupName = "Appearance")]
        public int LineWidth { get; set; }

        // You could add additional properties here for further customization.
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Plots hourly Fibonacci levels. This indicator calculates the high and low of a capture period defined from :50 to :10 (next hour) and then displays horizontal lines for the high, low, midline and 25% extension levels. The lines extend until :50 of the same session.";
                Name = "HourlyFibsSlow";
                IsOverlay = true;
                Calculate = Calculate.OnEachTick;
                IsSuspendedWhileInactive = true;
                BarsRequiredToPlot = 1;

                // Default appearance properties
                LineColor = Brushes.White;
                LineWidth = 2;

                // Add five plots corresponding to:
                // Plot0: Hourly high (solid); Plot1: Hourly low (solid);
                // Plot2: Midline (solid); Plot3: Upper extension (dashed); Plot4: Lower extension (dashed)
                AddPlot(new Stroke(LineColor, LineWidth), PlotStyle.Line, "HourHigh");
                AddPlot(new Stroke(LineColor, LineWidth), PlotStyle.Line, "HourLow");
                AddPlot(new Stroke(LineColor, LineWidth), PlotStyle.Line, "Midline");
                AddPlot(new Stroke(LineColor, DashStyleHelper.Dash , LineWidth), PlotStyle.Line, "UpperExt");
                AddPlot(new Stroke(LineColor, DashStyleHelper.Dash, LineWidth) , PlotStyle.Line, "LowerExt");
            }
            else if (State == State.Configure)
            {
                // Nothing additional needed in Configure.
            }
            else if (State == State.DataLoaded)
            {
                // Initialize variables once data is available
                periodHigh = double.MinValue;
                periodLow = double.MaxValue;
                captureStart = DateTime.MinValue;
                captureEnd = DateTime.MinValue;
                extensionEnd = DateTime.MinValue;
                inExtension = false;
            }
        }

        protected override void OnBarUpdate()
        {
            // Ensure we have enough bars
            if (CurrentBar < 1)
                return;

            DateTime dt = Time[0];

            // Determine capture period boundaries:
            // Capture period: from HH:50 to (HH+1):10.
            // If the current bar’s minute is >=50, then use the current hour’s 50; otherwise use previous hour.
            DateTime newCaptureStart;
            if (dt.Minute >= 50)
                newCaptureStart = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 50, 0);
            else
            {
                DateTime prev = dt.AddHours(-1);
                newCaptureStart = new DateTime(prev.Year, prev.Month, prev.Day, prev.Hour, 50, 0);
            }
            DateTime newCaptureEnd = newCaptureStart.AddMinutes(20);  // ends at :10 (next hour)
            DateTime newExtensionEnd = newCaptureStart.AddHours(1);     // extends until :50 (next hour)

            // If a new capture period has begun, reset the period high/low values.
            if (newCaptureStart != captureStart)
            {
                captureStart = newCaptureStart;
                captureEnd = newCaptureEnd;
                extensionEnd = newExtensionEnd;
                periodHigh = double.MinValue;
                periodLow = double.MaxValue;
                inExtension = false;
            }
			
			

            // During the capture period [captureStart, captureEnd)
            if (dt >= captureStart && dt < captureEnd)
            {
                periodHigh = Math.Max(periodHigh, High[0]);
                periodLow = Math.Min(periodLow, Low[0]);
                inExtension = false;
                // Clear the plot values so nothing is drawn during capture
                Values[0][0] = double.NaN; // HourHigh
                Values[1][0] = double.NaN; // HourLow
                Values[2][0] = double.NaN; // Midline
                Values[3][0] = double.NaN; // UpperExt
                Values[4][0] = double.NaN; // LowerExt
            }
            // During the extension period [captureEnd, extensionEnd)
            else if (dt >= captureEnd && dt < extensionEnd)
            {
				Print("settinginextension");
                inExtension = true;
                // Calculate the midline and the 25% Fibonacci extension levels
                midline = (periodHigh + periodLow) / 2.0;
                double range = periodHigh - periodLow;
                upperExt = periodHigh + range * 0.25;
                lowerExt = periodLow - range * 0.25;

                // Update the plot series so that the built‑in plots show horizontal lines.
                Values[0][0] = periodHigh;
                Values[1][0] = periodLow;
                Values[2][0] = midline;
                Values[3][0] = upperExt;
                Values[4][0] = lowerExt;
            }
            else
            {
                inExtension = false;
                // Outside both periods clear the plot values.
                Values[0][0] = double.NaN;
                Values[1][0] = double.NaN;
                Values[2][0] = double.NaN;
                Values[3][0] = double.NaN;
                Values[4][0] = double.NaN;
            }
        }

//        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
//        {
//            // Only draw our custom render objects when in the extension period.
//			//Print("OnRender: xStart=");
//			Print("inExtension  --" + inExtension);
//            if (!inExtension)
//                return;
//			//Print("OnRender: xStart=");
//            // Convert the time boundaries (captureEnd and extensionEnd) into bar indices.
//            int idxStart = Bars.GetBar(captureEnd);
//            int idxEnd = Bars.GetBar(extensionEnd);
//            if (idxStart < 0 || idxEnd < 0)
//                return;

//            float xStart = chartControl.GetXByBarIndex(ChartBars, idxStart);
//            float xEnd = chartControl.GetXByBarIndex(ChartBars, idxEnd);
//            if (xEnd <= xStart)
//                return;  // nothing to render if xEnd comes before xStart

//            // Convert the computed price levels to Y coordinates.
//            float yHigh = chartScale.GetYByValue(periodHigh);
//            float yLow = chartScale.GetYByValue(periodLow);
//            float yMid = chartScale.GetYByValue(midline);
//            float yUpper = chartScale.GetYByValue(upperExt);
//            float yLower = chartScale.GetYByValue(lowerExt);

//            // Create a solid brush using our LineColor.
//            var solidBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.White);
//            // Note: The brush color will follow the property LineColor; you can modify this to use a conversion from System.Windows.Media.Brush if desired.
//            // Create a dashed stroke style for the Fibonacci extension lines.
//            var dashProps = new StrokeStyleProperties { DashStyle = SharpDX.Direct2D1.DashStyle.Dash };
//            var dashStyle = new SharpDX.Direct2D1.StrokeStyle(RenderTarget.Factory, dashProps);
//            float lw = LineWidth;

//            // Draw the solid horizontal lines: HourHigh, HourLow, and Midline.
//            RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yHigh), new SharpDX.Vector2(xEnd, yHigh), solidBrush, lw);
//            RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yLow), new SharpDX.Vector2(xEnd, yLow), solidBrush, lw);
//            RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yMid), new SharpDX.Vector2(xEnd, yMid), solidBrush, lw);

//            // Draw the dashed lines: Upper Extension and Lower Extension.
//            RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yUpper), new SharpDX.Vector2(xEnd, yUpper), solidBrush, lw, dashStyle);
//            RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yLower), new SharpDX.Vector2(xEnd, yLower), solidBrush, lw, dashStyle);

//            solidBrush.Dispose();
//            dashStyle.Dispose();

//            base.OnRender(chartControl, chartScale);
//        }
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
{
    // Only render during the extension period.
    if (!inExtension)
        return;

    // Determine the bar indices for captureEnd and extensionEnd.
    int idxStart = Bars.GetBar(captureEnd);
    int idxEnd = Bars.GetBar(extensionEnd);
    if (idxStart < 0 || idxEnd < 0)
        return;

    float xStart = chartControl.GetXByBarIndex(ChartBars, idxStart);
    float xEnd = chartControl.GetXByBarIndex(ChartBars, idxEnd);
    if (xEnd <= xStart)
        return;

    // Convert price levels to Y coordinates.
    float yHigh = chartScale.GetYByValue(periodHigh);
    float yLow = chartScale.GetYByValue(periodLow);
    float yMid = chartScale.GetYByValue(midline);
    float yUpper = chartScale.GetYByValue(upperExt);
    float yLower = chartScale.GetYByValue(lowerExt);

    // Use a cached solid brush and dash style.
    if (cachedSolidBrush == null)
    {
        cachedSolidBrush = (SharpDX.Direct2D1.SolidColorBrush)LineColor.ToDxBrush(RenderTarget);
    }
    if (cachedDashStyle == null)
    {
       //var dashProps = new StrokeStyleProperties { DashStyle = NinjaTrader.NinjaScript.DrawingTools.DashStyleHelper.Dash };
		var dashProps = new StrokeStyleProperties { DashStyle = SharpDX.Direct2D1.DashStyle.Dash };
        cachedDashStyle = new SharpDX.Direct2D1.StrokeStyle(RenderTarget.Factory, dashProps);
    }
    float lw = LineWidth;

    // Draw solid horizontal lines: HourHigh, HourLow, and Midline.
    RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yHigh), new SharpDX.Vector2(xEnd, yHigh), cachedSolidBrush, lw);
    RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yLow), new SharpDX.Vector2(xEnd, yLow), cachedSolidBrush, lw);
    RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yMid), new SharpDX.Vector2(xEnd, yMid), cachedSolidBrush, lw);

    // Draw dashed lines: Upper Extension and Lower Extension.
    RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yUpper), new SharpDX.Vector2(xEnd, yUpper), cachedSolidBrush, lw, cachedDashStyle);
    RenderTarget.DrawLine(new SharpDX.Vector2(xStart, yLower), new SharpDX.Vector2(xEnd, yLower), cachedSolidBrush, lw, cachedDashStyle);

    // Call base OnRender (if desired).
    base.OnRender(chartControl, chartScale);
}

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HourlyFibsSlow[] cacheHourlyFibsSlow;
		public HourlyFibsSlow HourlyFibsSlow(System.Windows.Media.Brush lineColor, int lineWidth)
		{
			return HourlyFibsSlow(Input, lineColor, lineWidth);
		}

		public HourlyFibsSlow HourlyFibsSlow(ISeries<double> input, System.Windows.Media.Brush lineColor, int lineWidth)
		{
			if (cacheHourlyFibsSlow != null)
				for (int idx = 0; idx < cacheHourlyFibsSlow.Length; idx++)
					if (cacheHourlyFibsSlow[idx] != null && cacheHourlyFibsSlow[idx].LineColor == lineColor && cacheHourlyFibsSlow[idx].LineWidth == lineWidth && cacheHourlyFibsSlow[idx].EqualsInput(input))
						return cacheHourlyFibsSlow[idx];
			return CacheIndicator<HourlyFibsSlow>(new HourlyFibsSlow(){ LineColor = lineColor, LineWidth = lineWidth }, input, ref cacheHourlyFibsSlow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HourlyFibsSlow HourlyFibsSlow(System.Windows.Media.Brush lineColor, int lineWidth)
		{
			return indicator.HourlyFibsSlow(Input, lineColor, lineWidth);
		}

		public Indicators.HourlyFibsSlow HourlyFibsSlow(ISeries<double> input , System.Windows.Media.Brush lineColor, int lineWidth)
		{
			return indicator.HourlyFibsSlow(input, lineColor, lineWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HourlyFibsSlow HourlyFibsSlow(System.Windows.Media.Brush lineColor, int lineWidth)
		{
			return indicator.HourlyFibsSlow(Input, lineColor, lineWidth);
		}

		public Indicators.HourlyFibsSlow HourlyFibsSlow(ISeries<double> input , System.Windows.Media.Brush lineColor, int lineWidth)
		{
			return indicator.HourlyFibsSlow(input, lineColor, lineWidth);
		}
	}
}

#endregion
