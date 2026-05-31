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
	public class T3TrendFilterWithSignals : Indicator
	{
		
		private bool LongDirection;
		private bool ShortDirection;
		private Series<double> t30, t31, t32, t33, t34, t35;
        private Series<double> ma1, ma2, ma3, ma4, ma5;
		private Series<double> histou;
        private Series<double> histod;
		private double Trend;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "T3TrendFilterWithSignals";
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
				VolumeFactor			= 0.7;
				Period1					= 8;
				Period2					= 11;
				Period3					= 14;
				Period4					= 17;
				Period5					= 20;
				ShowSignals					= false;
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Bar, "Histou");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Histod");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "Signal_Trade");
				MaximumBarsLookBack					= MaximumBarsLookBack.Infinite;
				LongDirection					= false;
				ShortDirection					= false;
				LongFilterOn					= @"LongT3On";
				LongFilterOff					= @"LongT3Off";
				ShortFilterOn					= @"ShortT3On";
				ShortFilterOff					= @"ShortT3Off";
			}
			else if (State == State.Configure)
			{
				 ma1 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                ma2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                ma3 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                ma4 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                ma5 = new Series<double>(this, MaximumBarsLookBack.Infinite);

                t30 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                t31 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                t32 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                t33 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                t34 = new Series<double>(this, MaximumBarsLookBack.Infinite);
                t35 = new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
			else if (State == State.DataLoaded)
			{				
				ma1			= TillsonT3(Close, Period1, VolumeFactor).Value;
				ma2				= TillsonT3(Close, Period2, VolumeFactor).Value;
				ma3				= TillsonT3(Close, Period3, VolumeFactor).Value;
				ma4				= TillsonT3(Close, Period4, VolumeFactor).Value;
				ma5				= TillsonT3(Close, Period5, VolumeFactor).Value;
			}
		}

		protected override void OnBarUpdate()
		{
			
			
			
//			if (CurrentBar < 30)
//				return;
			
			
			if (CurrentBar == 0)
            {
                t30[0] = Input[0];
                t31[0] = Input[0];
                t32[0] = Input[0];
                t33[0] = Input[0];
                t34[0] = Input[0];
                t35[0] = Input[0];
                return;
            }


			

            double histou = 0;
            double histod = 0;

            if (ma1[0] > ma1[1]) histou++;
            if (ma1[0] < ma1[1]) histod--;
            if (ma2[0] > ma2[1]) histou++;
            if (ma2[0] < ma2[1]) histod--;
            if (ma3[0] > ma3[1]) histou++;
            if (ma3[0] < ma3[1]) histod--;
            if (ma4[0] > ma4[1]) histou++;
            if (ma4[0] < ma4[1]) histod--;
            if (ma5[0] > ma5[1]) histou++;
            if (ma5[0] < ma5[1]) histod--;

            double trend = histou > 0 && histod == 0 ? 1 : histod < 0 && histou == 0 ? -1 : 0;
			Trend = trend;
			
            PlotBrushes[0][0] = trend == 1 ? Brushes.Green : trend == -1 ? Brushes.Red : Brushes.Gray;
            Values[0][0] = histou;
            Values[1][0] = histod;

           	PlotBrushes[0][0] = Brushes.Green;
			PlotBrushes[1][0] = Brushes.Red;
			
			if ((histou > 0 && histod == 0))
				Values [2][0] = 1;
			else if ((histou == 0 && histod < 0))
				Values [2][0] = -1;
			else
				Values [2][0] = 0;
			
			if ((histou > 0 && histod == 0)
				 && (LongDirection == false))
			{
				if (ShowSignals)
					Draw.Text(this, Convert.ToString(LongFilterOn) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "T3Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Green );	
				LongDirection = true;
			}
			else if ((histou == 0 && histod < 0)
				 && (ShortDirection == false))
			{
				if (ShowSignals)					
					Draw.Text(this, Convert.ToString(ShortFilterOn) + Convert.ToString(CurrentBars[0]), "T3Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.DarkRed );
				ShortDirection = true;
			}
			else
			{
				if  (LongDirection == true && histod < 0)
				{
					LongDirection = false;
					if (ShowSignals)
						Draw.Text(this, Convert.ToString(LongFilterOff) + Convert.ToString(CurrentBars[0]), "T3LongOff" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Magenta );		
					
					//Draw.Text(this, Convert.ToString(LongFilterOff) + " " + Convert.ToString(CurrentBars[0]), @"LongOFF", 0, (Close[0] + (10 * TickSize)) );
				}
				else if (ShortDirection == true && histou > 0)
				{
					ShortDirection = false;
					if (ShowSignals)
						Draw.Text(this, Convert.ToString(ShortFilterOff) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "T3ShortOff", 0, (Low[0] + (-12 * TickSize)), Brushes.YellowGreen );	
					
				}
			}
			
			
			
			//PlotBrushes[0][0] = Brushes.Fuchsia;
			
		}
	
		
		#region Properties
		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="VolumeFactor", Description="T3 Volume Factor", Order=1, GroupName="Parameters")]
		public double VolumeFactor
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period1", Description="Period 1", Order=2, GroupName="Parameters")]
		public int Period1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period2", Description="Period 2", Order=3, GroupName="Parameters")]
		public int Period2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period3", Description="Period 3", Order=4, GroupName="Parameters")]
		public int Period3
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period4", Description="Period 4", Order=5, GroupName="Parameters")]
		public int Period4
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period5", Description="Period 5", Order=6, GroupName="Parameters")]
		public int Period5
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowSignals", Description="Show Signals?", Order=7, GroupName="Parameters")]
		public bool ShowSignals
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="LongFilterOn", Order=4, GroupName="Signals")]
		public string LongFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongFilterOff", Order=5, GroupName="Signals")]
		public string LongFilterOff
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOn", Order=6, GroupName="Signals")]
		public string ShortFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOff", Order=7, GroupName="Signals")]
		public string ShortFilterOff
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal_Trade
		{
		    get { return Values[2]; }
		}
		
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private T3TrendFilterWithSignals[] cacheT3TrendFilterWithSignals;
		public T3TrendFilterWithSignals T3TrendFilterWithSignals(double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return T3TrendFilterWithSignals(Input, volumeFactor, period1, period2, period3, period4, period5, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public T3TrendFilterWithSignals T3TrendFilterWithSignals(ISeries<double> input, double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			if (cacheT3TrendFilterWithSignals != null)
				for (int idx = 0; idx < cacheT3TrendFilterWithSignals.Length; idx++)
					if (cacheT3TrendFilterWithSignals[idx] != null && cacheT3TrendFilterWithSignals[idx].VolumeFactor == volumeFactor && cacheT3TrendFilterWithSignals[idx].Period1 == period1 && cacheT3TrendFilterWithSignals[idx].Period2 == period2 && cacheT3TrendFilterWithSignals[idx].Period3 == period3 && cacheT3TrendFilterWithSignals[idx].Period4 == period4 && cacheT3TrendFilterWithSignals[idx].Period5 == period5 && cacheT3TrendFilterWithSignals[idx].ShowSignals == showSignals && cacheT3TrendFilterWithSignals[idx].LongFilterOn == longFilterOn && cacheT3TrendFilterWithSignals[idx].LongFilterOff == longFilterOff && cacheT3TrendFilterWithSignals[idx].ShortFilterOn == shortFilterOn && cacheT3TrendFilterWithSignals[idx].ShortFilterOff == shortFilterOff && cacheT3TrendFilterWithSignals[idx].EqualsInput(input))
						return cacheT3TrendFilterWithSignals[idx];
			return CacheIndicator<T3TrendFilterWithSignals>(new T3TrendFilterWithSignals(){ VolumeFactor = volumeFactor, Period1 = period1, Period2 = period2, Period3 = period3, Period4 = period4, Period5 = period5, ShowSignals = showSignals, LongFilterOn = longFilterOn, LongFilterOff = longFilterOff, ShortFilterOn = shortFilterOn, ShortFilterOff = shortFilterOff }, input, ref cacheT3TrendFilterWithSignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.T3TrendFilterWithSignals T3TrendFilterWithSignals(double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.T3TrendFilterWithSignals(Input, volumeFactor, period1, period2, period3, period4, period5, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public Indicators.T3TrendFilterWithSignals T3TrendFilterWithSignals(ISeries<double> input , double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.T3TrendFilterWithSignals(input, volumeFactor, period1, period2, period3, period4, period5, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.T3TrendFilterWithSignals T3TrendFilterWithSignals(double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.T3TrendFilterWithSignals(Input, volumeFactor, period1, period2, period3, period4, period5, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public Indicators.T3TrendFilterWithSignals T3TrendFilterWithSignals(ISeries<double> input , double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool showSignals, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.T3TrendFilterWithSignals(input, volumeFactor, period1, period2, period3, period4, period5, showSignals, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}
	}
}

#endregion
