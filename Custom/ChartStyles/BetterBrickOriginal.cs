//
// BetterBrickStyle - for use with BetterRenkoBarType
//
// written by aslan
//
// 20100807 - created to go with BetterRenko
// 20100812 - chnaged to paint OHLC bar for live bar
// 20150719 - DaleBru converted to NT8
// 20220324 releasing under Better Ninja Tools, minor updates, changed Class name
// 20220331 fix issue with no last bar when wicks are transparent
// 20220402 fix leaking brush 
// 20220412 added Original to bar name
//
#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
//    [Gui.CategoryOrder("Version", 8000000)]     // at end
//    [CategoryExpanded("Version", false)]
    public class BetterBrick : ChartStyle
	{
        public const string version = "2.1.0 released 20220404";

//        [Display(Name = "Version", Description = "Version and release date", Order = 1, GroupName = "Version")]
//        public string Version { get { return version; } set { } }

        protected override void OnStateChange()
	    {
	        if (State == State.SetDefaults)
	        {
//                Name = "BetterBrick";
                Name = "BetterBrick Original";
                Description = @"ChartStyle that goes with original BetterRenko";
	            ChartStyleType = (ChartStyleType) 88;
	            BarWidth = 2;
	        }
	        else if (State == State.Configure)
	        {
                SetPropertyName("BarWidth", Custom.Resource.NinjaScriptChartStyleBarWidth);
	            SetPropertyName("DownBrush", Custom.Resource.NinjaScriptChartStyleCandleDownBarsColor);
	            SetPropertyName("UpBrush", Custom.Resource.NinjaScriptChartStyleCandleUpBarsColor);
	            SetPropertyName("Stroke", Custom.Resource.NinjaScriptChartStyleCandleOutline);
                SetPropertyName("Stroke2", Custom.Resource.NinjaScriptChartStyleCandleWick);
            
                Properties.Remove(Properties.Find("Name", true));
            }
        }

	    public override int GetBarPaintWidth(int barWidth)
		{
            // middle line + 2 * half of the body width + 2 * border line
            return 1 + 2 * (barWidth) + (int)Math.Round(Stroke.Width);
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
            var bars = chartBars.Bars;
            float barWidth = GetBarPaintWidth(BarWidthUI);
            Vector2 point0 = new Vector2();
            Vector2 point1 = new Vector2();
            RectangleF rect = new RectangleF();
            
            double brickSize = bars.BarsPeriod.Value * bars.Instrument.MasterInstrument.TickSize;  // #ticks per brick * tickSize
            int lastBarIdx = bars.Count - 1;
            for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
            {
                var overriddenBarBrush = chartControl.GetBarOverrideBrush(chartBars, idx);
                var overriddenOutlineBrush = chartControl.GetCandleOutlineOverrideBrush(chartBars, idx);
                
                var outlineBrush = overriddenOutlineBrush ?? Stroke.BrushDX;
                var wickBrush    = overriddenOutlineBrush ?? Stroke2.BrushDX;

                var closeValue = bars.GetClose(idx);
                var close = chartScale.GetYByValue(closeValue);
                var high = chartScale.GetYByValue(bars.GetHigh(idx));
                var low = chartScale.GetYByValue(bars.GetLow(idx));
                var openValue = bars.GetOpen(idx);
                var open = chartScale.GetYByValue(openValue);
                var x = chartControl.GetXByBarIndex(chartBars, idx);
                var renkoOpen = open;

                // change renkoOpen if necessary - so brick is not too big
                if (bars.Instrument.MasterInstrument.Compare(Math.Abs(openValue - closeValue), brickSize) > 0)
                {
                    renkoOpen = close < open ? chartScale.GetYByValue(closeValue - brickSize) : chartScale.GetYByValue(closeValue + brickSize);
                }
                if (idx != lastBarIdx)
                {
                    if (Math.Abs(renkoOpen - close) < 0.0000001)
                    {
                        // Line 
                        point0.X = x - barWidth * 0.5f;
                        point0.Y = close;
                        point1.X = x + barWidth * 0.5f;
                        point1.Y = close;
                        if (!(outlineBrush is SolidColorBrush))
                            TransformBrush(outlineBrush, new RectangleF(point0.X, point0.Y - Stroke.Width, barWidth, Stroke.Width));
                        RenderTarget.DrawLine(point0, point1, outlineBrush, Stroke.Width, Stroke.StrokeStyle);
                    }
                    else
                    {
                        // Candle
                        var upBar = close < open;
                        var prevOpenVal = (idx != 0 ? bars.GetOpen(idx - 1) : openValue);
                        var prevCloseVal = (idx != 0 ? bars.GetClose(idx - 1) : closeValue);
                        if ((idx != lastBarIdx && bars.Instrument.MasterInstrument.Compare(Math.Abs(prevCloseVal - closeValue), brickSize) < 0) ||
                            (idx == lastBarIdx && close == open))
                        {
                            upBar = prevCloseVal > prevOpenVal;  // use prev color, since we are still inside prev bricks
                        }
                        var bodyBrush = overriddenBarBrush ?? (upBar ? UpBrushDX : DownBrushDX);
                        rect.X = x - barWidth * 0.5f + 0.5f;
                        rect.Y = Math.Min(close, renkoOpen);
                        rect.Width = barWidth - 1;
                        rect.Height = Math.Max(renkoOpen, close) - Math.Min(close, renkoOpen);
                        if (!(bodyBrush is SolidColorBrush))
                            TransformBrush(bodyBrush, rect);
                        if (!(outlineBrush is SolidColorBrush))
                            TransformBrush(outlineBrush, rect);
                        RenderTarget.FillRectangle(rect, bodyBrush);
                        RenderTarget.DrawRectangle(rect, outlineBrush, Stroke.Width, Stroke.StrokeStyle);
                    }

                    // High wick
                    if (high < Math.Min(renkoOpen, close))
                    {
                        point0.X = x;
                        point0.Y = high;
                        point1.X = x;
                        point1.Y = Math.Min(renkoOpen, close);
                        if (!(wickBrush is SolidColorBrush))
                            TransformBrush(wickBrush, new RectangleF(point0.X - Stroke2.Width, point0.Y, Stroke2.Width, point1.Y - point0.Y));
                        RenderTarget.DrawLine(point0, point1, wickBrush, Stroke2.Width, Stroke2.StrokeStyle);
                    }

                    // Low wick
                    if (low > Math.Max(renkoOpen, close))
                    {
                        point0.X = x;
                        point0.Y = low;
                        point1.X = x;
                        point1.Y = Math.Max(renkoOpen, close);
                        if (!(wickBrush is SolidColorBrush))
                            TransformBrush(wickBrush, new RectangleF(point1.X - Stroke2.Width, point1.Y, Stroke2.Width, point0.Y - point1.Y));
                        RenderTarget.DrawLine(point0, point1, wickBrush, Stroke2.Width, Stroke2.StrokeStyle);
                    }
                }
                else
                {
                    // for last bar, just draw the OHLC, make sure rendered if no wicks
                    var brush = overriddenOutlineBrush ?? Stroke2.BrushDX;
					
					Brush allocatedBrush = null;
                    
					if (brush is SolidColorBrush && (brush as SolidColorBrush).Color.Alpha == 0)
					{
                        allocatedBrush = chartControl.Properties.ChartText.ToDxBrush(RenderTarget);
						brush = allocatedBrush;
					}
					
                    // Vertical bar
                    point0.X = x;
                    point0.Y = high;
                    point1.X = x;
                    point1.Y = low;
                    if (!(brush is SolidColorBrush))
                        TransformBrush(brush, new RectangleF(point1.X - Stroke2.Width, point1.Y, Stroke2.Width, point0.Y - point1.Y));
                    RenderTarget.DrawLine(point0, point1, brush, Stroke2.Width, Stroke2.StrokeStyle);

                    // Close horizontal
                    point0.X = x + barWidth * 0.5f;
                    point0.Y = close;
                    point1.X = x;
                    point1.Y = close;
                    RenderTarget.DrawLine(point0, point1, brush, Stroke2.Width, Stroke2.StrokeStyle);

                    // Open horizontal
                    point0.X = x - barWidth * 0.5f;
                    point0.Y = open;
                    point1.X = x;
                    point1.Y = open;

                    RenderTarget.DrawLine(point0, point1, brush, Stroke2.Width, Stroke2.StrokeStyle);
					
					if (allocatedBrush != null)
					{
						allocatedBrush.Dispose();
						allocatedBrush = null;
					}
                }
            }
		}
	}
}
