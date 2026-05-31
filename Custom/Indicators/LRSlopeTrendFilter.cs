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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class LRSlopeTrendFilter : Indicator
    {
        private int currentTrend;
        private int previousTrend;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Linear Regression Slope Trend Filter - Plots 1 for uptrend, -1 for downtrend, 0 for neutral";
                Name = "LRSlopeTrendFilter";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                Period = 14;
                SlopeThreshold = 0.15;
                ShowArrows = true;
                ShowNeutral = true;
                
                AddPlot(Brushes.Transparent, "TrendValue");
            }
            else if (State == State.DataLoaded)
            {
                currentTrend = 0;
                previousTrend = 0;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Period)
            {
                Values[0][0] = 0;
                return;
            }
            
            // Calculate Linear Regression Slope
            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumX2 = 0;
            
            for (int i = 0; i < Period; i++)
            {
                double x = i;
                double y = Close[Period - 1 - i];
                
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }
            
            double slope = (Period * sumXY - sumX * sumY) / (Period * sumX2 - sumX * sumX);
            
            // Normalize slope (multiply by 100 for easier threshold comparison)
            double normalizedSlope = slope * 100;
            
            // Store previous trend
            previousTrend = currentTrend;
            
            // Determine trend based on slope threshold
            if (normalizedSlope > SlopeThreshold)
            {
                currentTrend = 1;  // Uptrend
            }
            else if (normalizedSlope < -SlopeThreshold)
            {
                currentTrend = -1;  // Downtrend
            }
            else
            {
                if (ShowNeutral)
                    currentTrend = 0;  // Neutral/Ranging
                // else keep previous trend
            }
            
            // Set plot value
            Values[0][0] = currentTrend;
            
            // Draw arrows on trend change
            if (ShowArrows && CurrentBar > Period)
            {
                if (currentTrend == 1 && previousTrend != 1)
                {
                    // Uptrend started
                    Draw.ArrowUp(this, "UpArrow" + CurrentBar, true, 0, Low[0] - (3 * TickSize), Brushes.Lime);
                    
                    // Optional: Draw text showing slope value
                    Draw.Text(this, "SlopeUp" + CurrentBar, true, 
                        string.Format("▲ {0:F2}", normalizedSlope), 
                        0, Low[0] - (6 * TickSize), 0, Brushes.Lime, 
                        new SimpleFont("Arial", 9), TextAlignment.Center, 
                        Brushes.Transparent, Brushes.Transparent, 0);
                }
                else if (currentTrend == -1 && previousTrend != -1)
                {
                    // Downtrend started
                    Draw.ArrowDown(this, "DownArrow" + CurrentBar, true, 0, High[0] + (3 * TickSize), Brushes.Red);
                    
                    // Optional: Draw text showing slope value
                    Draw.Text(this, "SlopeDown" + CurrentBar, true, 
                        string.Format("▼ {0:F2}", normalizedSlope), 
                        0, High[0] + (6 * TickSize), 0, Brushes.Red, 
                        new SimpleFont("Arial", 9), TextAlignment.Center, 
                        Brushes.Transparent, Brushes.Transparent, 0);
                }
                else if (ShowNeutral && currentTrend == 0 && previousTrend != 0)
                {
                    // Neutral started
                    Draw.Diamond(this, "Neutral" + CurrentBar, true, 0, Close[0], Brushes.Yellow);
                }
            }
            
            // Color code the background slightly based on trend (optional)
            if (currentTrend == 1)
            {
                BackBrushes[0] = new SolidColorBrush(Color.FromArgb(5, 0, 255, 0));
            }
            else if (currentTrend == -1)
            {
                BackBrushes[0] = new SolidColorBrush(Color.FromArgb(5, 255, 0, 0));
            }
            else
            {
                BackBrushes[0] = Brushes.Transparent;
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(2, int.MaxValue)]
        [Display(Name = "Period", Description = "Period for Linear Regression calculation", Order = 1, GroupName = "Parameters")]
        public int Period { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Slope Threshold", Description = "Minimum slope to trigger trend signal (normalized)", Order = 2, GroupName = "Parameters")]
        public double SlopeThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Arrows", Description = "Show arrows on trend change", Order = 3, GroupName = "Display")]
        public bool ShowArrows { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Neutral State", Description = "Show neutral state (0) or keep previous trend", Order = 4, GroupName = "Display")]
        public bool ShowNeutral { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> TrendValue
        {
            get { return Values[0]; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LRSlopeTrendFilter[] cacheLRSlopeTrendFilter;
		public LRSlopeTrendFilter LRSlopeTrendFilter(int period, double slopeThreshold, bool showArrows, bool showNeutral)
		{
			return LRSlopeTrendFilter(Input, period, slopeThreshold, showArrows, showNeutral);
		}

		public LRSlopeTrendFilter LRSlopeTrendFilter(ISeries<double> input, int period, double slopeThreshold, bool showArrows, bool showNeutral)
		{
			if (cacheLRSlopeTrendFilter != null)
				for (int idx = 0; idx < cacheLRSlopeTrendFilter.Length; idx++)
					if (cacheLRSlopeTrendFilter[idx] != null && cacheLRSlopeTrendFilter[idx].Period == period && cacheLRSlopeTrendFilter[idx].SlopeThreshold == slopeThreshold && cacheLRSlopeTrendFilter[idx].ShowArrows == showArrows && cacheLRSlopeTrendFilter[idx].ShowNeutral == showNeutral && cacheLRSlopeTrendFilter[idx].EqualsInput(input))
						return cacheLRSlopeTrendFilter[idx];
			return CacheIndicator<LRSlopeTrendFilter>(new LRSlopeTrendFilter(){ Period = period, SlopeThreshold = slopeThreshold, ShowArrows = showArrows, ShowNeutral = showNeutral }, input, ref cacheLRSlopeTrendFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LRSlopeTrendFilter LRSlopeTrendFilter(int period, double slopeThreshold, bool showArrows, bool showNeutral)
		{
			return indicator.LRSlopeTrendFilter(Input, period, slopeThreshold, showArrows, showNeutral);
		}

		public Indicators.LRSlopeTrendFilter LRSlopeTrendFilter(ISeries<double> input , int period, double slopeThreshold, bool showArrows, bool showNeutral)
		{
			return indicator.LRSlopeTrendFilter(input, period, slopeThreshold, showArrows, showNeutral);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LRSlopeTrendFilter LRSlopeTrendFilter(int period, double slopeThreshold, bool showArrows, bool showNeutral)
		{
			return indicator.LRSlopeTrendFilter(Input, period, slopeThreshold, showArrows, showNeutral);
		}

		public Indicators.LRSlopeTrendFilter LRSlopeTrendFilter(ISeries<double> input , int period, double slopeThreshold, bool showArrows, bool showNeutral)
		{
			return indicator.LRSlopeTrendFilter(input, period, slopeThreshold, showArrows, showNeutral);
		}
	}
}

#endregion