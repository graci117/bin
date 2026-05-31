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

	public enum MAtypeBraid
	{
		DEMA,
		EMA,
		HMA,
		LinReg,
		SMA,
		TEMA,		
		TMA,
		VWMA,
		WMA,
		ZLEMA	
	}	

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BraidFilter : Indicator
	{
		
		private bool LongDirection;
		private bool ShortDirection;
		private Series<double>  MA1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BraidFilter";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period1					= 3;
				Period2					= 7;
				Period3					= 14;
				ATRLength				= 14;
				MAType					= MAtypeBraid.EMA;
				PipsMinSepPercent					= 40;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Bar, "Braid");
				//AddLine(Brushes.Orange, 1, "Filter");
				AddPlot(new Stroke(Brushes.Orange, 1), PlotStyle.Line, "Filter");
				
				AddPlot(new Stroke(Brushes.Black, 1), PlotStyle.Line, "isGreen");
				AddPlot(new Stroke(Brushes.Black, 1), PlotStyle.Line, "isRed");
				AddPlot(new Stroke(Brushes.Black, 1), PlotStyle.Line, "isGray");
				
//				AddPlot(Brushes.Transparent, 	"isGreen");
//				AddPlot(Brushes.Transparent, 	"isRed");
//				AddPlot(Brushes.Transparent, 	"isGray");
				LongDirection					= false;
				ShortDirection					= false;
				
			}
			 else if (State == State.DataLoaded)
			{
				MA1 = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 1)
				return;
			
			double ma01 = GetMA(Close, Period1)[0];
            double ma02 = GetMA(Open, Period2)[0];
            double ma03 = GetMA(Close, Period3)[0];
				
			double maxVal = Math.Max(Math.Max(ma01, ma02), ma03);
            double minVal = Math.Min(Math.Min(ma01, ma02), ma03);
            double dif = maxVal - minVal;

            double atrVal = ATR(ATRLength)[0] * PipsMinSepPercent / 100;
			
			if ((ma01 > ma02 && dif > atrVal)
				 && (LongDirection == false))
			{
				//Draw.Text(this, Convert.ToString(LongFilterOn) + " " + Convert.ToString(CurrentBars[0]), @"LongON", 0, (Close[0] + (-10 * TickSize)) );
				LongDirection = true;
				ShortDirection = false;
			}
			else if ((ma02 > ma01 && dif > atrVal)
				 && (ShortDirection == false))
			{
				//Draw.Text(this, Convert.ToString(ShortFilterOn) + " " + Convert.ToString(CurrentBars[0]), @"ShortON", 0, (Close[0] + (10 * TickSize)) );
				ShortDirection = true;
				LongDirection = false;
			}
			else
			{
				if  (LongDirection == true && ! (ma01 > ma02 && dif > atrVal))
				{
					LongDirection = false;
					//Draw.Text(this, Convert.ToString(LongFilterOff) + " " + Convert.ToString(CurrentBars[0]), @"LongOFF", 0, (Close[0] + (10 * TickSize)) );
				}
				else if (ShortDirection == true && ! (ma02 > ma01 && dif > atrVal))
				{
					ShortDirection = false;
					//Draw.Text(this, Convert.ToString(ShortFilterOff) + " " + Convert.ToString(CurrentBars[0]), @"ShortOFF", 0, (Close[0] + (-10 * TickSize)) );
				}
			}
			
			
			Values[0][0] = dif;
			Values[1][0] = atrVal;
			
			if  (ma01 > ma02 && dif > atrVal)
			{
				Values[2][0] = 1;//green
				Values[3][0] = 0;//red
				Values[4][0] = 0;
			}
			else if (ma02 > ma01 && dif > atrVal )
			{
				Values[2][0] = 0;//green
				Values[3][0] = 1;//red
				Values[4][0] = 0;
			}
			else
			{
				Values[4][0] = 1;
				Values[2][0] = 0;//green
				Values[3][0] = 0;//red
			}
			
//			Print("isGreen--" + isGreen + "---" + Time[0]);
//			Print("isRed--" + isRed + "---" + Time[0]);
			
			 PlotBrushes[0][0] = ma01 > ma02 && dif > atrVal ? Brushes.Green :
                                ma02 > ma01 && dif > atrVal ? Brushes.Red : Brushes.Gray;
			
			
		}
		
		private Series<double> GetMA(ISeries<double> myInput, int period)
		{
			
			
			
			switch (MAType)
				{
					case MAtypeBraid.DEMA:						
						
						MA1 = DEMA(myInput, period).Value;
						
						break;
						
					case MAtypeBraid.EMA:
							MA1 = EMA(myInput, period).Value;
						
					break;	
						
					case MAtypeBraid.HMA:
							MA1 = HMA(myInput, period).Value;
					
					break;	
						
					case MAtypeBraid.LinReg:
							MA1 = LinReg(myInput, period).Value;

					break;							
						
					case MAtypeBraid.SMA:
							MA1 = SMA(myInput, period).Value;
					
					break;	
						
					case MAtypeBraid.TEMA:
							MA1 = TEMA(myInput, period).Value;

					break;	
						
					case MAtypeBraid.TMA:	
							MA1 = TMA(myInput, period).Value;
					
					break;	
					
					case MAtypeBraid.VWMA:
							MA1 = VWMA(myInput, period).Value;

					break;	
						
					case MAtypeBraid.WMA:
							MA1 = WMA(myInput, period).Value;
							
					break;
						
					case MAtypeBraid.ZLEMA:
							MA1 = ZLEMA(myInput, period).Value;
					break;
																
				}	
				return MA1;
		}
		

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period1", Order=1, GroupName="Parameters")]
		public int Period1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Period2", Order=2, GroupName="Parameters")]
		public int Period2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(8, int.MaxValue)]
		[Display(Name="Period3", Order=3, GroupName="Parameters")]
		public int Period3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(8, int.MaxValue)]
		[Display(Name="ATRLength", Order=4, GroupName="Parameters")]
		public int ATRLength
		{ get; set; }


		[NinjaScriptProperty]
		[Display(Name="MAType", Order=4, GroupName="Parameters")]
		public MAtypeBraid MAType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, double.MaxValue)]
		[Display(Name="PipsMinSepPercent", Order=5, GroupName="Parameters")]
		public double PipsMinSepPercent
		{ get; set; }
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> isGreen
//		{
//			get { return Values[2]; }
//		}
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> isRed
//		{
//			get { return Values[3]; }
//		}
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> isGray
//		{
//			get { return Values[4]; }
//		}
		

//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> Braid
//		{
//			get { return Values[0]; }
//		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BraidFilter[] cacheBraidFilter;
		public BraidFilter BraidFilter(int period1, int period2, int period3, int aTRLength, MAtypeBraid mAType, double pipsMinSepPercent)
		{
			return BraidFilter(Input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent);
		}

		public BraidFilter BraidFilter(ISeries<double> input, int period1, int period2, int period3, int aTRLength, MAtypeBraid mAType, double pipsMinSepPercent)
		{
			if (cacheBraidFilter != null)
				for (int idx = 0; idx < cacheBraidFilter.Length; idx++)
					if (cacheBraidFilter[idx] != null && cacheBraidFilter[idx].Period1 == period1 && cacheBraidFilter[idx].Period2 == period2 && cacheBraidFilter[idx].Period3 == period3 && cacheBraidFilter[idx].ATRLength == aTRLength && cacheBraidFilter[idx].MAType == mAType && cacheBraidFilter[idx].PipsMinSepPercent == pipsMinSepPercent && cacheBraidFilter[idx].EqualsInput(input))
						return cacheBraidFilter[idx];
			return CacheIndicator<BraidFilter>(new BraidFilter(){ Period1 = period1, Period2 = period2, Period3 = period3, ATRLength = aTRLength, MAType = mAType, PipsMinSepPercent = pipsMinSepPercent }, input, ref cacheBraidFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BraidFilter BraidFilter(int period1, int period2, int period3, int aTRLength, MAtypeBraid mAType, double pipsMinSepPercent)
		{
			return indicator.BraidFilter(Input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent);
		}

		public Indicators.BraidFilter BraidFilter(ISeries<double> input , int period1, int period2, int period3, int aTRLength, MAtypeBraid mAType, double pipsMinSepPercent)
		{
			return indicator.BraidFilter(input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BraidFilter BraidFilter(int period1, int period2, int period3, int aTRLength, MAtypeBraid mAType, double pipsMinSepPercent)
		{
			return indicator.BraidFilter(Input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent);
		}

		public Indicators.BraidFilter BraidFilter(ISeries<double> input , int period1, int period2, int period3, int aTRLength, MAtypeBraid mAType, double pipsMinSepPercent)
		{
			return indicator.BraidFilter(input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent);
		}
	}
}

#endregion
