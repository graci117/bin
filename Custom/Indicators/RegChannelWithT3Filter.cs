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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RegChannelWithT3Filter : Indicator
	{
		
		private RegChannel RegChannel1;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		private LinReg LinReg1;
		private T3TrendFilter T3TrendFilter1;
		private ADX ADX1;
		private double t3Up;
		private double t3Down;
		private int longSplitNum;
		private int shortSplitNum;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RegChannelWithT3Filter";				
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				RegChannelPeriod					= 30;
				RegChannelWidth					= 0.9;
				RegChannelHighLowWidth			= 3.5;
				
				VolumeFactor					= 0.7;
				Period1					= 8;
				Period2					= 11;
				Period3					= 14;
				Period4					= 17;
				Period5					= 20;
				
				ADXPeriod						= 4;
				ADXThreshold					= 50;
				
				LinRegPeriod					= 9;
				SplitArrowDistance				= 6;
				
				longSplitNum					= 0;
				shortSplitNum					= 0;
				//this.MaximumBarsLookBack 		= MaximumBarsLookBack.TwoHundredFiftySix;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
            {
				RegChannel1					= RegChannel(Close, RegChannelPeriod, RegChannelWidth);
				RegressionChannelHighLow1	= RegressionChannelHighLow(Close, RegChannelPeriod, RegChannelHighLowWidth);
				LinReg1						= LinReg(Close, LinRegPeriod);
				T3TrendFilter1				= T3TrendFilter(Close, VolumeFactor, Period1, Period2, Period3, Period4, Period5, false);
				ADX1						= ADX(Close, Convert.ToInt32(ADXPeriod));
			}

		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 5)
				return;
			

            t3Up = T3TrendFilter1.Values[0][0];
            t3Down = T3TrendFilter1.Values[1][0];
			
			if (SplitArrowDistance == 0)
			{
				SplitArrowDistance = 300;
			}
			
			 if (CheckLongEntryConditions() )
			 {
				 
				 if (longSplitNum % SplitArrowDistance == 0)
				 {
					 Draw.ArrowUp(this, "LongSignal" + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] + (-10 * TickSize)) , Brushes.Lime);
					
				 }
				  longSplitNum ++;
			
			}
			 else{
				 longSplitNum = 0;
			 }
			
           

            if (CheckShortEntryConditions() )
            {
				if (shortSplitNum % SplitArrowDistance == 0)
				 {
					Draw.ArrowDown(this, "ShortSignal" + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (10 * TickSize)) , Brushes.Red);
					
				 }
				 shortSplitNum ++;
			 }
		
			 else
			 {
				 shortSplitNum = 0;
			 }
			


		}
		
		private bool CheckLongEntryConditions()
        {
			Print("RegChannel1.Middle[0] gt RegChannel1.Middle[1] -" + (RegChannel1.Middle[0] > RegChannel1.Middle[1])  + "---" + Time[0]);
			Print("RegChannel1.Middle[1] lte RegChannel1.Middle[2] -" + (RegChannel1.Middle[1] <= RegChannel1.Middle[2] )   + "---" + Time[0]);
			Print("LinReg1[0] > LinReg1[1]  -" + (LinReg1[0] > LinReg1[1])  + "---" + Time[0]);
			Print("t3Up" + t3Up  + "---" + Time[0]);
			Print("t3Down" + t3Down  + "---" + Time[0]);
			Print("ADX1[0] gt ADX1[2]-" +  (ADX1[0] > ADX1[2]) + "---" + Time[0]);
			Print("ADX1[0] gt ADXThreshold -" + (ADX1[0] > ADXThreshold)  + "---" + Time[0]);
			Print("Low[0]  gt Low[1]  -" + (Low[0] > Low[1])  + "---" + Time[0]);
			Print("Low[0] gt RegressionChannelHighLow1.Lower[1] -" + (Low[0] > RegressionChannelHighLow1.Lower[1])   + "---" + Time[0]);
			Print("Close[0] gt Open[0] -" + (Close[0] > Open[0])  + "---" + Time[0]);
            return 
                   ((
					(RegChannel1.Middle[0] > RegChannel1.Middle[1] 
					&& RegChannel1.Middle[1] <= RegChannel1.Middle[2] 
						&& LinReg1[0] > LinReg1[1] 
						&& Close[0] > Open[0] 
						&& t3Up >= 5 
						&& t3Down == 0 
						&& ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold
					) 
					||
	                   	(Low[0] > Low[1] && Low[1] <= RegChannel1.Lower[1] 
						//&& LinReg1[0] > LinReg1[1] 
						&& Close[0] > Open[0] 
						&& t3Up >= 5 
						&& t3Down == 0 
						&& ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold
					) 
					||
                    (Low[0] > RegressionChannelHighLow1.Lower[1] &&
						LinReg1[0] > LinReg1[1] && Close[0] > Open[0] &&
						Low[0] > RegChannel1.Lower[1] &&
						t3Up >= 5 
						&& t3Down == 0 
						&& ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold
					)));

        }
		

        private bool CheckShortEntryConditions()
        {
            return 
				( ((RegChannel1.Middle[0] < RegChannel1.Middle[1]
				&&	RegChannel1.Middle[1] >= RegChannel1.Middle[2]
				&&	LinReg1[0] < LinReg1[1]
				&&	Close[0] < Open[0]
				&&	t3Down <= -5 && t3Up == 0 
				&&	ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold
				) 
				||  (High[0] < High[1] 
				&&	High[1] >= RegChannel1.Upper[1] 
				&& LinReg1[0] < LinReg1[1] 
				&& Close[0] < Open[0] 
				&& t3Down <= -5 && t3Up == 0 
				&& ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold
				) 
				||
				(High[0] < RegressionChannelHighLow1.Upper[1] 
				&& LinReg1[0] < LinReg1[1] 
				&& Close[0] < Open[0] 
				&& t3Down <= -5 && t3Up == 0 
				&& ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold
				))
				
				);
			}

		
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RegChannelPeriod", Description="Reg Channel Period", Order=1, GroupName="RegChannel")]
		public int RegChannelPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Reg Channel Width", Order=2, GroupName="RegChannel")]
		public double RegChannelWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Reg Channel High/Low Width", Order=2, GroupName="RegChannel")]
		public double RegChannelHighLowWidth
		{ get; set; }
		
		
		
		
		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="VolumeFactor", Description="T3 Volume Factor", Order=1, GroupName="T3 Trend Filter")]
		public double VolumeFactor
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period1", Description="Period 1", Order=2, GroupName="T3 Trend Filter")]
		public int Period1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period2", Description="Period 2", Order=3, GroupName="T3 Trend Filter")]
		public int Period2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period3", Description="Period 3", Order=4, GroupName="T3 Trend Filter")]
		public int Period3
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period4", Description="Period 4", Order=5, GroupName="T3 Trend Filter")]
		public int Period4
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period5", Description="Period 5", Order=6, GroupName="T3 Trend Filter")]
		public int Period5
		{ get; set; }
		
	
		[NinjaScriptProperty]
		[Display(Name="ADXPeriod", Order=3, GroupName="ADX")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXThreshold", Order=4, GroupName="ADX")]
		public int ADXThreshold
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="LinRegPeriod", Order=2, GroupName="Lin Reg")]
		public int LinRegPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="SplitArrowDistance", Order=2, GroupName="Split Arrow Distance")]
		public int SplitArrowDistance
		{ get; set; }
		

		[NinjaScriptProperty]
		[Display(Name="LongSignal", Order=6, GroupName="Parameters")]
		public string LongSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortSignal", Order=7, GroupName="Parameters")]
		public string ShortSignal
		{ get; set; }
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RegChannelWithT3Filter[] cacheRegChannelWithT3Filter;
		public RegChannelWithT3Filter RegChannelWithT3Filter(int regChannelPeriod, double regChannelWidth, double regChannelHighLowWidth, double volumeFactor, int period1, int period2, int period3, int period4, int period5, int aDXPeriod, int aDXThreshold, int linRegPeriod, int splitArrowDistance, string longSignal, string shortSignal)
		{
			return RegChannelWithT3Filter(Input, regChannelPeriod, regChannelWidth, regChannelHighLowWidth, volumeFactor, period1, period2, period3, period4, period5, aDXPeriod, aDXThreshold, linRegPeriod, splitArrowDistance, longSignal, shortSignal);
		}

		public RegChannelWithT3Filter RegChannelWithT3Filter(ISeries<double> input, int regChannelPeriod, double regChannelWidth, double regChannelHighLowWidth, double volumeFactor, int period1, int period2, int period3, int period4, int period5, int aDXPeriod, int aDXThreshold, int linRegPeriod, int splitArrowDistance, string longSignal, string shortSignal)
		{
			if (cacheRegChannelWithT3Filter != null)
				for (int idx = 0; idx < cacheRegChannelWithT3Filter.Length; idx++)
					if (cacheRegChannelWithT3Filter[idx] != null && cacheRegChannelWithT3Filter[idx].RegChannelPeriod == regChannelPeriod && cacheRegChannelWithT3Filter[idx].RegChannelWidth == regChannelWidth && cacheRegChannelWithT3Filter[idx].RegChannelHighLowWidth == regChannelHighLowWidth && cacheRegChannelWithT3Filter[idx].VolumeFactor == volumeFactor && cacheRegChannelWithT3Filter[idx].Period1 == period1 && cacheRegChannelWithT3Filter[idx].Period2 == period2 && cacheRegChannelWithT3Filter[idx].Period3 == period3 && cacheRegChannelWithT3Filter[idx].Period4 == period4 && cacheRegChannelWithT3Filter[idx].Period5 == period5 && cacheRegChannelWithT3Filter[idx].ADXPeriod == aDXPeriod && cacheRegChannelWithT3Filter[idx].ADXThreshold == aDXThreshold && cacheRegChannelWithT3Filter[idx].LinRegPeriod == linRegPeriod && cacheRegChannelWithT3Filter[idx].SplitArrowDistance == splitArrowDistance && cacheRegChannelWithT3Filter[idx].LongSignal == longSignal && cacheRegChannelWithT3Filter[idx].ShortSignal == shortSignal && cacheRegChannelWithT3Filter[idx].EqualsInput(input))
						return cacheRegChannelWithT3Filter[idx];
			return CacheIndicator<RegChannelWithT3Filter>(new RegChannelWithT3Filter(){ RegChannelPeriod = regChannelPeriod, RegChannelWidth = regChannelWidth, RegChannelHighLowWidth = regChannelHighLowWidth, VolumeFactor = volumeFactor, Period1 = period1, Period2 = period2, Period3 = period3, Period4 = period4, Period5 = period5, ADXPeriod = aDXPeriod, ADXThreshold = aDXThreshold, LinRegPeriod = linRegPeriod, SplitArrowDistance = splitArrowDistance, LongSignal = longSignal, ShortSignal = shortSignal }, input, ref cacheRegChannelWithT3Filter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RegChannelWithT3Filter RegChannelWithT3Filter(int regChannelPeriod, double regChannelWidth, double regChannelHighLowWidth, double volumeFactor, int period1, int period2, int period3, int period4, int period5, int aDXPeriod, int aDXThreshold, int linRegPeriod, int splitArrowDistance, string longSignal, string shortSignal)
		{
			return indicator.RegChannelWithT3Filter(Input, regChannelPeriod, regChannelWidth, regChannelHighLowWidth, volumeFactor, period1, period2, period3, period4, period5, aDXPeriod, aDXThreshold, linRegPeriod, splitArrowDistance, longSignal, shortSignal);
		}

		public Indicators.RegChannelWithT3Filter RegChannelWithT3Filter(ISeries<double> input , int regChannelPeriod, double regChannelWidth, double regChannelHighLowWidth, double volumeFactor, int period1, int period2, int period3, int period4, int period5, int aDXPeriod, int aDXThreshold, int linRegPeriod, int splitArrowDistance, string longSignal, string shortSignal)
		{
			return indicator.RegChannelWithT3Filter(input, regChannelPeriod, regChannelWidth, regChannelHighLowWidth, volumeFactor, period1, period2, period3, period4, period5, aDXPeriod, aDXThreshold, linRegPeriod, splitArrowDistance, longSignal, shortSignal);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RegChannelWithT3Filter RegChannelWithT3Filter(int regChannelPeriod, double regChannelWidth, double regChannelHighLowWidth, double volumeFactor, int period1, int period2, int period3, int period4, int period5, int aDXPeriod, int aDXThreshold, int linRegPeriod, int splitArrowDistance, string longSignal, string shortSignal)
		{
			return indicator.RegChannelWithT3Filter(Input, regChannelPeriod, regChannelWidth, regChannelHighLowWidth, volumeFactor, period1, period2, period3, period4, period5, aDXPeriod, aDXThreshold, linRegPeriod, splitArrowDistance, longSignal, shortSignal);
		}

		public Indicators.RegChannelWithT3Filter RegChannelWithT3Filter(ISeries<double> input , int regChannelPeriod, double regChannelWidth, double regChannelHighLowWidth, double volumeFactor, int period1, int period2, int period3, int period4, int period5, int aDXPeriod, int aDXThreshold, int linRegPeriod, int splitArrowDistance, string longSignal, string shortSignal)
		{
			return indicator.RegChannelWithT3Filter(input, regChannelPeriod, regChannelWidth, regChannelHighLowWidth, volumeFactor, period1, period2, period3, period4, period5, aDXPeriod, aDXThreshold, linRegPeriod, splitArrowDistance, longSignal, shortSignal);
		}
	}
}

#endregion
