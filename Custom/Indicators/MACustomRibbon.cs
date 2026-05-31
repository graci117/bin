#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using System.ComponentModel.DataAnnotations;
#endregion

// Define the enum in a separate namespace as recommended by NinjaTrader best practices
namespace CustomEnumNamespace
{
    public enum MAType
    {
        SMA,
        EMA,
        WMA,
        TRIMA,
        ZLEMA,
        DEMA,
        TEMA,
        HMA
    }
}

namespace NinjaTrader.NinjaScript.Indicators
{
    public class MACustomRibbon : Indicator
    {
        private Series<double> ma05;
        private Series<double> ma10;
        private Series<double> ma15;
        private Series<double> ma20;
        private Series<double> ma25;
        private Series<double> ma30;
        private Series<double> ma35;
        private Series<double> ma40;
        private Series<double> ma45;
        private Series<double> ma50;
        private Series<double> ma55;
        private Series<double> ma60;
        private Series<double> ma65;
        private Series<double> ma70;
        private Series<double> ma75;
        private Series<double> ma80;
        private Series<double> ma85;
        private Series<double> ma90;
        private Series<double> ma95;
        private Series<double> ma100;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Moving Average Ribbon with multiple MA types";
                Name = "MACustomRibbon";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                
                // Default Close values
                
                MATypeSelection = CustomEnumNamespace.MAType.EMA;
                
                // Default MA periods
                Period1 = 5;
                Period2 = 10;
                Period3 = 15;
                Period4 = 20;
                Period5 = 25;
                Period6 = 30;
                Period7 = 35;
                Period8 = 40;
                Period9 = 45;
                Period10 = 50;
                Period11 = 55;
                Period12 = 60;
                Period13 = 65;
                Period14 = 70;
                Period15 = 75;
                Period16 = 80;
                Period17 = 85;
                Period18 = 90;
                Period19 = 95;
                Period20 = 100;
            }
            else if (State == State.DataLoaded)
            {
                // Initialize series
                ma05 = new Series<double>(this);
                ma10 = new Series<double>(this);
                ma15 = new Series<double>(this);
                ma20 = new Series<double>(this);
                ma25 = new Series<double>(this);
                ma30 = new Series<double>(this);
                ma35 = new Series<double>(this);
                ma40 = new Series<double>(this);
                ma45 = new Series<double>(this);
                ma50 = new Series<double>(this);
                ma55 = new Series<double>(this);
                ma60 = new Series<double>(this);
                ma65 = new Series<double>(this);
                ma70 = new Series<double>(this);
                ma75 = new Series<double>(this);
                ma80 = new Series<double>(this);
                ma85 = new Series<double>(this);
                ma90 = new Series<double>(this);
                ma95 = new Series<double>(this);
                ma100 = new Series<double>(this);
				
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "ma05");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma10");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma15");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma20");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma25");
                AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "ma30");
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "ma35");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma40");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma45");
				 AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma50");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma55");
                AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "ma60");
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "ma65");
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "70");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma75");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma80");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma85");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma90");
                AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "ma95");					
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "ma100");
               
            }
        }

        protected override void OnBarUpdate()
        {
			
			 if (CurrentBar < 300)
        		return;
			 
            ma05[0] = CalculateMA(Period1);
            ma10[0] = CalculateMA(Period2);
            ma15[0] = CalculateMA(Period3);
            ma20[0] = CalculateMA(Period4);
            ma25[0] = CalculateMA(Period5);
            ma30[0] = CalculateMA(Period6);
            ma35[0] = CalculateMA(Period7);
            ma40[0] = CalculateMA(Period8);
            ma45[0] = CalculateMA(Period9);
            ma50[0] = CalculateMA(Period10);
            ma55[0] = CalculateMA(Period11);
            ma60[0] = CalculateMA(Period12);
            ma65[0] = CalculateMA(Period13);
            ma70[0] = CalculateMA(Period14);
            ma75[0] = CalculateMA(Period15);
            ma80[0] = CalculateMA(Period16);
            ma85[0] = CalculateMA(Period17);
            ma90[0] = CalculateMA(Period18);
            ma95[0] = CalculateMA(Period19);
            ma100[0] = CalculateMA(Period20);

               // Set colors for each MA plot based on conditions
		    PlotBrushes[0][0] = GetMAColor(ma05, ma100);
		    PlotBrushes[1][0] = GetMAColor(ma10, ma100);
		    PlotBrushes[2][0] = GetMAColor(ma15, ma100);
		    PlotBrushes[3][0] = GetMAColor(ma20, ma100);
		    PlotBrushes[4][0] = GetMAColor(ma25, ma100);
		    PlotBrushes[5][0] = GetMAColor(ma30, ma100);
		    PlotBrushes[6][0] = GetMAColor(ma35, ma100);
		    PlotBrushes[7][0] = GetMAColor(ma40, ma100);
		    PlotBrushes[8][0] = GetMAColor(ma45, ma100);
		    PlotBrushes[9][0] = GetMAColor(ma50, ma100);
		    PlotBrushes[10][0] = GetMAColor(ma55, ma100);
		    PlotBrushes[11][0] = GetMAColor(ma60, ma100);
		    PlotBrushes[12][0] = GetMAColor(ma65, ma100);
		    PlotBrushes[13][0] = GetMAColor(ma70, ma100);
		    PlotBrushes[14][0] = GetMAColor(ma75, ma100);
		    PlotBrushes[15][0] = GetMAColor(ma80, ma100);
		    PlotBrushes[16][0] = GetMAColor(ma85, ma100);
		    PlotBrushes[17][0] = GetMAColor(ma90, ma100);
		    PlotBrushes[18][0] = GetMAColor(ma95, ma100);
		    PlotBrushes[19][0] = GetMAColor(ma100, ma100);
		}
		
		private Brush GetMAColor(Series<double> ma, Series<double> ma100)
		{
			
			
		    if (CurrentBar < 101) // Need at least 2 bars for comparison
		        return Brushes.Gray;
			
		    // Determine color based on the original PineScript logic
		    bool isRising = ma[0] > ma[1];
		    bool isAboveMA100 = ma[0] > ma100[0];
		    
		    if (isRising && isAboveMA100)
		        return Brushes.Lime;
		    else if (!isRising && isAboveMA100)
		        return Brushes.Maroon;
		    else if (!isRising && !isAboveMA100)
		        return Brushes.Red;
		    else if (isRising && !isAboveMA100)
		        return Brushes.Green;
		    else
		        return Brushes.Gray;
		}

        private double CalculateMA(int period)
        {
			
			if (CurrentBar < 110) // Need at least 2 bars for comparison
        		return 0;
			
            switch (MATypeSelection)
            {
                case CustomEnumNamespace.MAType.SMA:
                    return SMA(Close, period)[0];
                case CustomEnumNamespace.MAType.EMA:					
					return EMA(Close, period)[0];
					Print("Error Here");
                case CustomEnumNamespace.MAType.WMA:
                    return WMA(Close, period)[0];
                case CustomEnumNamespace.MAType.TRIMA:
                    return TRIMA(period);
                case CustomEnumNamespace.MAType.ZLEMA:
                    return ZLEMA(period);
                case CustomEnumNamespace.MAType.DEMA:
                    return DEMA(period);
                case CustomEnumNamespace.MAType.TEMA:
                    return TEMA(period);
                case CustomEnumNamespace.MAType.HMA:
                    return HMA(period);
                default:
                    return SMA(Close, period)[0];
            }
			
        }

        private double TRIMA(int period)
        {
            // Triangular Moving Average implementation
            return SMA(SMA(Close, period), period)[0];
        }

        private double HMA(int period)
        {
            // Hull Moving Average implementation
            int halfPeriod = period / 2;
            int sqrtPeriod = (int)Math.Sqrt(period);
            
            double wma1 = WMA(Close, halfPeriod)[0];
            double wma2 = WMA(Close, period)[0];
            Series<double> diff = new Series<double>(this);
            diff[0] = 2 * wma1 - wma2;
            
            return WMA(diff, sqrtPeriod)[0];
        }

        private double DEMA(int period)
        {
            // Double Exponential Moving Average
            double ema1 = EMA(Close, period)[0];
            double ema2 = EMA(EMA(Close, period), period)[0];
            return 2 * ema1 - ema2;
        }

        private double TEMA(int period)
        {
            // Triple Exponential Moving Average
            double ema1 = EMA(Close, period)[0];
            double ema2 = EMA(EMA(Close, period), period)[0];
            double ema3 = EMA(EMA(EMA(Close, period), period), period)[0];
            return 3 * ema1 - 3 * ema2 + ema3;
        }

        private double ZLEMA(int period)
        {
            // Zero Lag Exponential Moving Average
            double ema1 = EMA(Close, period)[0];
            double ema2 = EMA(EMA(Close, period), period)[0];
            return ema1 + (ema1 - ema2);
        }

//        private void PlotMA(Series<double> ma, int plotIndex, int lineWidth, string plotName)
//        {
//            Brush color = GetMAColor(ma);
//            Plot(plotName, ma, color, PlotStyle.Line, lineWidth);
//        }

//        private Brush GetMAColor(Series<double> ma)
//        {
//            // Determine color based on the original PineScript logic
//            bool isRising = ma[0] > ma[1];
//            bool isAboveMA100 = ma[0] > ma100[0];
            
//            if (isRising && isAboveMA100)
//                return Brushes.Lime;
//            else if (!isRising && isAboveMA100)
//                return Brushes.Maroon;
//            else if (!isRising && !isAboveMA100)
//                return Brushes.Red;
//            else if (isRising && !isAboveMA100)
//                return Brushes.Green;
//            else
//                return Brushes.Gray;
//        }

        #region Properties
      


        [NinjaScriptProperty]
        [Display(Name = "MA Type", Description = "Type of Moving Average to use", Order = 2, GroupName = "Parameters")]
        public CustomEnumNamespace.MAType MATypeSelection { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 1", Description = "Period for MA 1", Order = 3, GroupName = "MA Periods")]
        public int Period1 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 2", Description = "Period for MA 2", Order = 4, GroupName = "MA Periods")]
        public int Period2 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 3", Description = "Period for MA 3", Order = 5, GroupName = "MA Periods")]
        public int Period3 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 4", Description = "Period for MA 4", Order = 6, GroupName = "MA Periods")]
        public int Period4 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 5", Description = "Period for MA 5", Order = 7, GroupName = "MA Periods")]
        public int Period5 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 6", Description = "Period for MA 6", Order = 8, GroupName = "MA Periods")]
        public int Period6 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 7", Description = "Period for MA 7", Order = 9, GroupName = "MA Periods")]
        public int Period7 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 8", Description = "Period for MA 8", Order = 10, GroupName = "MA Periods")]
        public int Period8 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 9", Description = "Period for MA 9", Order = 11, GroupName = "MA Periods")]
        public int Period9 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 10", Description = "Period for MA 10", Order = 12, GroupName = "MA Periods")]
        public int Period10 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 11", Description = "Period for MA 11", Order = 13, GroupName = "MA Periods")]
        public int Period11 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 12", Description = "Period for MA 12", Order = 14, GroupName = "MA Periods")]
        public int Period12 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 13", Description = "Period for MA 13", Order = 15, GroupName = "MA Periods")]
        public int Period13 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 14", Description = "Period for MA 14", Order = 16, GroupName = "MA Periods")]
        public int Period14 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 15", Description = "Period for MA 15", Order = 17, GroupName = "MA Periods")]
        public int Period15 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 16", Description = "Period for MA 16", Order = 18, GroupName = "MA Periods")]
        public int Period16 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 17", Description = "Period for MA 17", Order = 19, GroupName = "MA Periods")]
        public int Period17 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 18", Description = "Period for MA 18", Order = 20, GroupName = "MA Periods")]
        public int Period18 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 19", Description = "Period for MA 19", Order = 21, GroupName = "MA Periods")]
        public int Period19 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period 20", Description = "Period for MA 20", Order = 22, GroupName = "MA Periods")]
        public int Period20 { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACustomRibbon[] cacheMACustomRibbon;
		public MACustomRibbon MACustomRibbon(CustomEnumNamespace.MAType mATypeSelection, int period1, int period2, int period3, int period4, int period5, int period6, int period7, int period8, int period9, int period10, int period11, int period12, int period13, int period14, int period15, int period16, int period17, int period18, int period19, int period20)
		{
			return MACustomRibbon(Input, mATypeSelection, period1, period2, period3, period4, period5, period6, period7, period8, period9, period10, period11, period12, period13, period14, period15, period16, period17, period18, period19, period20);
		}

		public MACustomRibbon MACustomRibbon(ISeries<double> input, CustomEnumNamespace.MAType mATypeSelection, int period1, int period2, int period3, int period4, int period5, int period6, int period7, int period8, int period9, int period10, int period11, int period12, int period13, int period14, int period15, int period16, int period17, int period18, int period19, int period20)
		{
			if (cacheMACustomRibbon != null)
				for (int idx = 0; idx < cacheMACustomRibbon.Length; idx++)
					if (cacheMACustomRibbon[idx] != null && cacheMACustomRibbon[idx].MATypeSelection == mATypeSelection && cacheMACustomRibbon[idx].Period1 == period1 && cacheMACustomRibbon[idx].Period2 == period2 && cacheMACustomRibbon[idx].Period3 == period3 && cacheMACustomRibbon[idx].Period4 == period4 && cacheMACustomRibbon[idx].Period5 == period5 && cacheMACustomRibbon[idx].Period6 == period6 && cacheMACustomRibbon[idx].Period7 == period7 && cacheMACustomRibbon[idx].Period8 == period8 && cacheMACustomRibbon[idx].Period9 == period9 && cacheMACustomRibbon[idx].Period10 == period10 && cacheMACustomRibbon[idx].Period11 == period11 && cacheMACustomRibbon[idx].Period12 == period12 && cacheMACustomRibbon[idx].Period13 == period13 && cacheMACustomRibbon[idx].Period14 == period14 && cacheMACustomRibbon[idx].Period15 == period15 && cacheMACustomRibbon[idx].Period16 == period16 && cacheMACustomRibbon[idx].Period17 == period17 && cacheMACustomRibbon[idx].Period18 == period18 && cacheMACustomRibbon[idx].Period19 == period19 && cacheMACustomRibbon[idx].Period20 == period20 && cacheMACustomRibbon[idx].EqualsInput(input))
						return cacheMACustomRibbon[idx];
			return CacheIndicator<MACustomRibbon>(new MACustomRibbon(){ MATypeSelection = mATypeSelection, Period1 = period1, Period2 = period2, Period3 = period3, Period4 = period4, Period5 = period5, Period6 = period6, Period7 = period7, Period8 = period8, Period9 = period9, Period10 = period10, Period11 = period11, Period12 = period12, Period13 = period13, Period14 = period14, Period15 = period15, Period16 = period16, Period17 = period17, Period18 = period18, Period19 = period19, Period20 = period20 }, input, ref cacheMACustomRibbon);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACustomRibbon MACustomRibbon(CustomEnumNamespace.MAType mATypeSelection, int period1, int period2, int period3, int period4, int period5, int period6, int period7, int period8, int period9, int period10, int period11, int period12, int period13, int period14, int period15, int period16, int period17, int period18, int period19, int period20)
		{
			return indicator.MACustomRibbon(Input, mATypeSelection, period1, period2, period3, period4, period5, period6, period7, period8, period9, period10, period11, period12, period13, period14, period15, period16, period17, period18, period19, period20);
		}

		public Indicators.MACustomRibbon MACustomRibbon(ISeries<double> input , CustomEnumNamespace.MAType mATypeSelection, int period1, int period2, int period3, int period4, int period5, int period6, int period7, int period8, int period9, int period10, int period11, int period12, int period13, int period14, int period15, int period16, int period17, int period18, int period19, int period20)
		{
			return indicator.MACustomRibbon(input, mATypeSelection, period1, period2, period3, period4, period5, period6, period7, period8, period9, period10, period11, period12, period13, period14, period15, period16, period17, period18, period19, period20);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACustomRibbon MACustomRibbon(CustomEnumNamespace.MAType mATypeSelection, int period1, int period2, int period3, int period4, int period5, int period6, int period7, int period8, int period9, int period10, int period11, int period12, int period13, int period14, int period15, int period16, int period17, int period18, int period19, int period20)
		{
			return indicator.MACustomRibbon(Input, mATypeSelection, period1, period2, period3, period4, period5, period6, period7, period8, period9, period10, period11, period12, period13, period14, period15, period16, period17, period18, period19, period20);
		}

		public Indicators.MACustomRibbon MACustomRibbon(ISeries<double> input , CustomEnumNamespace.MAType mATypeSelection, int period1, int period2, int period3, int period4, int period5, int period6, int period7, int period8, int period9, int period10, int period11, int period12, int period13, int period14, int period15, int period16, int period17, int period18, int period19, int period20)
		{
			return indicator.MACustomRibbon(input, mATypeSelection, period1, period2, period3, period4, period5, period6, period7, period8, period9, period10, period11, period12, period13, period14, period15, period16, period17, period18, period19, period20);
		}
	}
}

#endregion
