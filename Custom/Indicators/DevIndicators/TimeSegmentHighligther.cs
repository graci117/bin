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

// This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.DevIndicators
{
    public class TimeSegmentHighligther : Indicator
    {
        #region Variables
        private int segmentDuration;
        private int highlightStartMinutes;
        private int highlightEndMinutes;
        private Brush highlightColor;
        private string lastRectangleTag;
        private double segmentMax;
        private double segmentMin;
		private int iDataSeries = 1;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Enter the description for your new custom Indicator here.";
                Name = "TimeSegmentHighligther";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                // Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                // See Help Guide for additional information.
                IsSuspendedWhileInactive = true;

                // Configurable parameters
                SegmentDuration = 30;            // Duration of each segment in minutes
                HighlightStartMinutes = 3;       // Minutes at the start of the segment to highlight
                HighlightEndMinutes = 5;         // Minutes at the end of the segment to highlight
                HighlightColor = Brushes.Gray;    // Default color for highlighting candles

				useAditionalDS1M = false;
                isPaintBars = true;
                tshTemplate = "TSH";    
                isGlobalObj = true;
            }
            else if (State == State.Configure)
            {
                // AddDataSeries("NQ 03-25", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);        
				if (useAditionalDS1M) AddDataSeries(Data.BarsPeriodType.Minute, 1);      
            }
			else if (State == State.DataLoaded)
            {
            //	Check if the current chart's bar type matches the additional data series type			
				iDataSeries = 0;
				if (useAditionalDS1M) iDataSeries = 1;  
				
            }
			
        }

        protected override void OnBarUpdate()
        {
        //	Add your custom indicator logic here.
            
            if (CurrentBars[0] < HighlightStartMinutes || (useAditionalDS1M ? CurrentBars[1] < HighlightStartMinutes : false))
                return;

            // Get the time of the current bar
            DateTime barTime = (useAditionalDS1M ? Times[iDataSeries][0] : Time[0]);
            int segmentStartMinute = (barTime.Minute / SegmentDuration) * SegmentDuration;

            // Determine the start and end of the current segment
            DateTime segmentStart = new DateTime(barTime.Year, barTime.Month, barTime.Day, barTime.Hour, segmentStartMinute + 1, 0);
            DateTime rectangleStart = new DateTime(barTime.Year, barTime.Month, barTime.Day, barTime.Hour, segmentStartMinute + 1, 0);
            DateTime segmentEnd = segmentStart.AddMinutes(SegmentDuration);
            DateTime rectangleEnd = segmentStart.AddMinutes(SegmentDuration - HighlightEndMinutes);
            
            // Check if the bar is within the highlighted start or end minutes
            bool isInHighlightStart = barTime >= segmentStart && barTime < segmentStart.AddMinutes(HighlightStartMinutes);
            bool isInHighlightEnd = barTime >= segmentEnd.AddMinutes(-HighlightEndMinutes) && barTime < segmentEnd;

            // Apply the color if it is in the highlighted periods
            if (isInHighlightStart || isInHighlightEnd)
            {
                if (isPaintBars) CandleOutlineBrush = HighlightColor;
                if (isPaintBars) BarBrush = HighlightColor;
            }			

            // Calculate the maximum and minimum of the candles in the first minutes of the segment
            if (isInHighlightStart)
            {
                segmentMax = Math.Max(segmentMax, useAditionalDS1M ? Highs[iDataSeries][0] : High[0] );
                segmentMin = Math.Min(segmentMin, useAditionalDS1M ? Lows[iDataSeries][0] : Low[0] );
            }

            // Draw the rectangle after the first minutes of the segment
            if (barTime == segmentStart.AddMinutes(HighlightStartMinutes))
            {
                // Unique tag for the rectangle
                lastRectangleTag = $"Rectangle_{segmentStart.Ticks}";
                Print($"{Times[iDataSeries][0]} -->  BarTime  --> {barTime}   RectangleStart --> {rectangleStart}    RectangleEnd --> {rectangleEnd}     iDataSeries   -->   {iDataSeries}");	
                // Draw rectangle
                Draw.Rectangle(this, lastRectangleTag, rectangleStart, segmentMax, rectangleEnd, segmentMin, isGlobalObj, tshTemplate);
                // Reset segment values
                segmentMax = double.MinValue;
                segmentMin = double.MaxValue;
            }		
        }
		
        #region Properties
        
        [NinjaScriptProperty]		
        [Display(Name = "Use aditional 1 min dataseries?", Description = "Use aditional 1 min dataseries", GroupName = "Parameters", Order = 0)]		
        public bool useAditionalDS1M
        {
            get; set;
        }		
		
		[NinjaScriptProperty]
        [Range(1, 120)]
        [Display(Name = "Segment Duration (minutes)", Order = 1, GroupName = "Parameters")]
        public int SegmentDuration
        {
            get { return segmentDuration; }
            set { segmentDuration = value; }
        }

        [NinjaScriptProperty]
        [Range(1, 15)]
        [Display(Name = "Highlight Start Minutes", Order = 2, GroupName = "Parameters")]
        public int HighlightStartMinutes
        {
            get { return highlightStartMinutes; }
            set { highlightStartMinutes = value; }
        }

        [NinjaScriptProperty]
        [Range(1, 15)]
        [Display(Name = "Highlight End Minutes", Order = 3, GroupName = "Parameters")]
        public int HighlightEndMinutes
        {
            get { return highlightEndMinutes; }
            set { highlightEndMinutes = value; }
        }

        [NinjaScriptProperty]		
        [Display(Name = "Paint Bars?", Description = "Paint Bars ", GroupName = "Parameters", Order = 4)]		
        public bool isPaintBars
        {
            get; set;
        }
		
        [NinjaScriptProperty]
        [XmlIgnore()]
        [Display(Name = "Highlight Color", Order = 5, GroupName = "Parameters")]
        public Brush HighlightColor
        {
            get { return highlightColor; }
            set { highlightColor = value; }
        }		

        [NinjaScriptProperty]		
        [Display(Name = "Is Global Object?", Description = "Is Global rectangle object ", GroupName = "Parameters", Order = 6)]		
        public bool isGlobalObj
        {
            get; set;
        }		
		
        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(GroupName = "Parameters", Name = "Template for Rectangle", Description="Enter template for rectangle file path/name", Order = 7)]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="XML Files (*.xml)|*.xml")]
		public string tshTemplate { get; set; }			
			
        #endregion		
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DevIndicators.TimeSegmentHighligther[] cacheTimeSegmentHighligther;
		public DevIndicators.TimeSegmentHighligther TimeSegmentHighligther(bool useAditionalDS1M, int segmentDuration, int highlightStartMinutes, int highlightEndMinutes, bool isPaintBars, Brush highlightColor, bool isGlobalObj, string tshTemplate)
		{
			return TimeSegmentHighligther(Input, useAditionalDS1M, segmentDuration, highlightStartMinutes, highlightEndMinutes, isPaintBars, highlightColor, isGlobalObj, tshTemplate);
		}

		public DevIndicators.TimeSegmentHighligther TimeSegmentHighligther(ISeries<double> input, bool useAditionalDS1M, int segmentDuration, int highlightStartMinutes, int highlightEndMinutes, bool isPaintBars, Brush highlightColor, bool isGlobalObj, string tshTemplate)
		{
			if (cacheTimeSegmentHighligther != null)
				for (int idx = 0; idx < cacheTimeSegmentHighligther.Length; idx++)
					if (cacheTimeSegmentHighligther[idx] != null && cacheTimeSegmentHighligther[idx].useAditionalDS1M == useAditionalDS1M && cacheTimeSegmentHighligther[idx].SegmentDuration == segmentDuration && cacheTimeSegmentHighligther[idx].HighlightStartMinutes == highlightStartMinutes && cacheTimeSegmentHighligther[idx].HighlightEndMinutes == highlightEndMinutes && cacheTimeSegmentHighligther[idx].isPaintBars == isPaintBars && cacheTimeSegmentHighligther[idx].HighlightColor == highlightColor && cacheTimeSegmentHighligther[idx].isGlobalObj == isGlobalObj && cacheTimeSegmentHighligther[idx].tshTemplate == tshTemplate && cacheTimeSegmentHighligther[idx].EqualsInput(input))
						return cacheTimeSegmentHighligther[idx];
			return CacheIndicator<DevIndicators.TimeSegmentHighligther>(new DevIndicators.TimeSegmentHighligther(){ useAditionalDS1M = useAditionalDS1M, SegmentDuration = segmentDuration, HighlightStartMinutes = highlightStartMinutes, HighlightEndMinutes = highlightEndMinutes, isPaintBars = isPaintBars, HighlightColor = highlightColor, isGlobalObj = isGlobalObj, tshTemplate = tshTemplate }, input, ref cacheTimeSegmentHighligther);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DevIndicators.TimeSegmentHighligther TimeSegmentHighligther(bool useAditionalDS1M, int segmentDuration, int highlightStartMinutes, int highlightEndMinutes, bool isPaintBars, Brush highlightColor, bool isGlobalObj, string tshTemplate)
		{
			return indicator.TimeSegmentHighligther(Input, useAditionalDS1M, segmentDuration, highlightStartMinutes, highlightEndMinutes, isPaintBars, highlightColor, isGlobalObj, tshTemplate);
		}

		public Indicators.DevIndicators.TimeSegmentHighligther TimeSegmentHighligther(ISeries<double> input , bool useAditionalDS1M, int segmentDuration, int highlightStartMinutes, int highlightEndMinutes, bool isPaintBars, Brush highlightColor, bool isGlobalObj, string tshTemplate)
		{
			return indicator.TimeSegmentHighligther(input, useAditionalDS1M, segmentDuration, highlightStartMinutes, highlightEndMinutes, isPaintBars, highlightColor, isGlobalObj, tshTemplate);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DevIndicators.TimeSegmentHighligther TimeSegmentHighligther(bool useAditionalDS1M, int segmentDuration, int highlightStartMinutes, int highlightEndMinutes, bool isPaintBars, Brush highlightColor, bool isGlobalObj, string tshTemplate)
		{
			return indicator.TimeSegmentHighligther(Input, useAditionalDS1M, segmentDuration, highlightStartMinutes, highlightEndMinutes, isPaintBars, highlightColor, isGlobalObj, tshTemplate);
		}

		public Indicators.DevIndicators.TimeSegmentHighligther TimeSegmentHighligther(ISeries<double> input , bool useAditionalDS1M, int segmentDuration, int highlightStartMinutes, int highlightEndMinutes, bool isPaintBars, Brush highlightColor, bool isGlobalObj, string tshTemplate)
		{
			return indicator.TimeSegmentHighligther(input, useAditionalDS1M, segmentDuration, highlightStartMinutes, highlightEndMinutes, isPaintBars, highlightColor, isGlobalObj, tshTemplate);
		}
	}
}

#endregion
