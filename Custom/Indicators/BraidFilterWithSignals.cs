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
	public class BraidFilterWithSignals : Indicator
	{
		
		private bool LongDirection;
		private bool ShortDirection;
		private Series<double> ma01Series;
		private Series<double> ma02Series;
		private Series<double> ma03Series;
		private Series<double> difSeries;
		private Series<double> atrSeries;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BraidFilterWithSignals";
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
				MAType					= string.Empty;
				PipsMinSepPercent					= 40;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Bar, "Braid");
				//AddLine(Brushes.Orange, 1, "Filter");
				AddPlot(new Stroke(Brushes.Orange, 1), PlotStyle.Line, "Filter");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "isGreen");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "isRed");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "isGray");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Signal_Trade");
				//AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Signal_Trade");

				LongDirection					= false;
				ShortDirection					= false;
				LongFilterOn					= @"LongBraidOn";
				LongFilterOff					= @"LongBraidOff";
				ShortFilterOn					= @"ShortBraidOn";
				ShortFilterOff					= @"ShortBraidOff";
				ShowSignals						= true;
				isGreen							= false;
				isRed							= false;
				//Signal_Trade							= 0;
			}
			else if (State == State.Configure)
			{
				ma01Series = EMA(Close, Period1).Value;
				ma02Series = EMA(Open, Period2).Value;
				ma03Series = EMA(Close, Period3).Value;
				difSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				atrSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 40)
				return;
			
			double ma01 = ma01Series[0];
            double ma02 = ma02Series[0];
            double ma03 = ma03Series[0];
				
			double maxVal = Math.Max(Math.Max(ma01, ma02), ma03);
            double minVal = Math.Min(Math.Min(ma01, ma02), ma03);
            double dif = maxVal - minVal;
			difSeries[0] = dif;

            double atrVal = ATR(ATRLength)[0] * PipsMinSepPercent / 100;
			atrSeries[0] = atrVal;
			
			if ((ma01Series[0] > ma02Series[0] && difSeries[0] > atrSeries[0] )
				 && (LongDirection == false))
			{
				if (ShowSignals)
				{
//					if(isGreen )
//					{
//						if (difSeries[0] > difSeries[3])
//						{
//							Draw.Text(this, Convert.ToString(LongFilterOn) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Green );	
//							//Draw.Text(this, Convert.ToString(ShortFilterOff) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ShortOff", 0, (Low[0] + (-12 * TickSize)), Brushes.YellowGreen );	
//						}
							
//					}
//					else
//					{
//						Draw.Text(this, Convert.ToString(LongFilterOn) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Green );	
//						Draw.Text(this, Convert.ToString(ShortFilterOff) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ShortOff", 0, (Low[0] + (-12 * TickSize)), Brushes.Orange );	
//					}
					if (difSeries[0] > difSeries[1])
					{
						Draw.Text(this, Convert.ToString(LongFilterOn) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Green );	
						Draw.Text(this, Convert.ToString(ShortFilterOff) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ShortOff", 0, (Low[0] + (-12 * TickSize)), Brushes.Orange );	
						LongDirection = true;
						ShortDirection = false;
					}
				}
				
			}
			else if ((ma02Series[0] > ma01Series[0] && difSeries[0] > atrSeries[0] )
				 && (ShortDirection == false))
			{
				if (ShowSignals)
				{
//					if(isRed )
//					{
//						if (difSeries[0] > difSeries[3])
//						{
//							Draw.Text(this, Convert.ToString(ShortFilterOn) + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.DarkRed );
//							//Draw.Text(this, Convert.ToString(ShortFilterOff) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ShortOff", 0, (Low[0] + (-12 * TickSize)), Brushes.YellowGreen );	
//						}
							
//					}
//					else
//					{
									
//						Draw.Text(this, Convert.ToString(ShortFilterOn) + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.DarkRed );
//						Draw.Text(this, Convert.ToString(LongFilterOff) + Convert.ToString(CurrentBars[0]), "LongOff" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Purple );	
//					}
					if (difSeries[0] > difSeries[1])
					{
						Draw.Text(this, Convert.ToString(ShortFilterOn) + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.DarkRed );
						Draw.Text(this, Convert.ToString(LongFilterOff) + Convert.ToString(CurrentBars[0]), "LongOff" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Purple );	
						ShortDirection = true;
						LongDirection = false;
					}
		
					
				}
				
			}
			else
			{
				if  (LongDirection == true && (! (ma01Series[0] > ma02Series[0] && difSeries[0] > atrSeries[0]) || difSeries[0] < difSeries[3]))
				{
					LongDirection = false;
					if (ShowSignals)
					{
						Draw.Text(this, Convert.ToString(LongFilterOff) + Convert.ToString(CurrentBars[0]), "LongOff" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Magenta );		
					}
					//Draw.Text(this, Convert.ToString(LongFilterOff) + " " + Convert.ToString(CurrentBars[0]), @"LongOFF", 0, (Close[0] + (10 * TickSize)) );
				}
				else if (ShortDirection == true && (! (ma02Series[0] > ma01Series[0] && difSeries[0] > atrSeries[0]) || difSeries[0] < difSeries[3]))
				{
					ShortDirection = false;
					if (ShowSignals)
					{
						Draw.Text(this, Convert.ToString(ShortFilterOff) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ShortOff", 0, (Low[0] + (-12 * TickSize)), Brushes.YellowGreen );	
					}
				}
			}
			
			
			Values[0][0] = dif;
			Values[1][0] = atrVal;
			
			if  ( ma01Series[0] > ma02Series[0] && difSeries[0] > atrSeries[0])
			{
				isGreen = true;
				isRed = false;
				Values[5][0] = 1;
			}
			else if (ma02Series[0] >  ma01Series[0] && difSeries[0] > atrSeries[0] )
			{
				isGreen = true;
				isRed = false;
				Values[5][0] = -1;
			}
			else
			{
				isGray = true;
				Values[5][0] = 0;
			}
			
			 PlotBrushes[0][0] = ma01 > ma02 && dif > atrVal ? Brushes.Green :
                                ma02 > ma01 && dif > atrVal ? Brushes.Red : Brushes.Gray;
			
			
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
		public string MAType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, double.MaxValue)]
		[Display(Name="PipsMinSepPercent", Order=5, GroupName="Parameters")]
		public double PipsMinSepPercent
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="Show Signals?", Order=3, GroupName="Parameters")]
		public bool ShowSignals
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="LongFilterOn", Order=4, GroupName="Parameters")]
		public string LongFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongFilterOff", Order=5, GroupName="Parameters")]
		public string LongFilterOff
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOn", Order=6, GroupName="Parameters")]
		public string ShortFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOff", Order=7, GroupName="Parameters")]
		public string ShortFilterOff
		{ get; set; }
		
		public bool isGreen
		{get; set;}
		
		public bool isRed
		{get; set;}
		
		public bool isGray
		{get; set;}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal_Trade
		{
		    get { return Values[5]; }
		}

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
		private BraidFilterWithSignals[] cacheBraidFilterWithSignals;
		public BraidFilterWithSignals BraidFilterWithSignals(int period1, int period2, int period3, int aTRLength, string mAType, double pipsMinSepPercent, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return BraidFilterWithSignals(Input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public BraidFilterWithSignals BraidFilterWithSignals(ISeries<double> input, int period1, int period2, int period3, int aTRLength, string mAType, double pipsMinSepPercent, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			if (cacheBraidFilterWithSignals != null)
				for (int idx = 0; idx < cacheBraidFilterWithSignals.Length; idx++)
					if (cacheBraidFilterWithSignals[idx] != null && cacheBraidFilterWithSignals[idx].Period1 == period1 && cacheBraidFilterWithSignals[idx].Period2 == period2 && cacheBraidFilterWithSignals[idx].Period3 == period3 && cacheBraidFilterWithSignals[idx].ATRLength == aTRLength && cacheBraidFilterWithSignals[idx].MAType == mAType && cacheBraidFilterWithSignals[idx].PipsMinSepPercent == pipsMinSepPercent && cacheBraidFilterWithSignals[idx].ShowSignals == showSignals && cacheBraidFilterWithSignals[idx].LongFilterOn == longFilterOn && cacheBraidFilterWithSignals[idx].LongFilterOff == longFilterOff && cacheBraidFilterWithSignals[idx].ShortFilterOn == shortFilterOn && cacheBraidFilterWithSignals[idx].ShortFilterOff == shortFilterOff && cacheBraidFilterWithSignals[idx].EqualsInput(input))
						return cacheBraidFilterWithSignals[idx];
			return CacheIndicator<BraidFilterWithSignals>(new BraidFilterWithSignals(){ Period1 = period1, Period2 = period2, Period3 = period3, ATRLength = aTRLength, MAType = mAType, PipsMinSepPercent = pipsMinSepPercent, ShowSignals = showSignals, LongFilterOn = longFilterOn, LongFilterOff = longFilterOff, ShortFilterOn = shortFilterOn, ShortFilterOff = shortFilterOff }, input, ref cacheBraidFilterWithSignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BraidFilterWithSignals BraidFilterWithSignals(int period1, int period2, int period3, int aTRLength, string mAType, double pipsMinSepPercent, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.BraidFilterWithSignals(Input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public Indicators.BraidFilterWithSignals BraidFilterWithSignals(ISeries<double> input , int period1, int period2, int period3, int aTRLength, string mAType, double pipsMinSepPercent, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.BraidFilterWithSignals(input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BraidFilterWithSignals BraidFilterWithSignals(int period1, int period2, int period3, int aTRLength, string mAType, double pipsMinSepPercent, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.BraidFilterWithSignals(Input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public Indicators.BraidFilterWithSignals BraidFilterWithSignals(ISeries<double> input , int period1, int period2, int period3, int aTRLength, string mAType, double pipsMinSepPercent, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.BraidFilterWithSignals(input, period1, period2, period3, aTRLength, mAType, pipsMinSepPercent, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}
	}
}

#endregion
