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
	public class T3TrendFilter : Indicator
	{
		
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
				Name										= "T3TrendFilter";
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
				VolumeFactor			= 0.5;
				Period1					= 1;
				Period2					= 1;
				Period3					= 1;
				Period4					= 1;
				Period5					= 9;
				ColorBars					= false;
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Bar, "Histou");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Histod");
				MaximumBarsLookBack					= MaximumBarsLookBack.Infinite;
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

//            ma1[0] = CalculateT3(Input[0], Period1, t30, t31, t32, t33, t34, t35);
//            ma2[0] = CalculateT3(Input[0], Period2, t30, t31, t32, t33, t34, t35);
//            ma3[0] = CalculateT3(Input[0], Period3, t30, t31, t32, t33, t34, t35);
//            ma4[0] = CalculateT3(Input[0], Period4, t30, t31, t32, t33, t34, t35);
//            ma5[0] = CalculateT3(Input[0], Period5, t30, t31, t32, t33, t34, t35);
			
			
			
			
			Print("Time---" + Time[0] );
			//Print("period---" + period );			
			Print("ma1" + ma1[0]);
			Print("ma1" + ma1[1]);
			
			Print("ma2" + ma2[0]);
			Print("ma2" + ma2[1]);
			
			Print("ma3" + ma3[0]);
			Print("ma3" + ma3[1]);
			
			Print("ma4" + ma4[0]);
			Print("ma4" + ma4[1]);
			
			Print("ma5" + ma5[0]);
			Print("ma5" + ma5[1]);
			

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
			
//			Print("ma1[0]--" + ma1[0] + "Time" + Time[0]);
//			Print("ma1[1]--" + ma1[1] + "Time" + Time[0]);
			
			//double trend = histou[0] > 0 && histod[0] == 0 ? 1 : histod[0] < 0 && histou[0] == 0 ? -1 : 0;
           	PlotBrushes[0][0] = Brushes.Green;
			PlotBrushes[1][0] = Brushes.Red;
			
			
			
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
		[Display(Name="ColorBars", Description="Color Bars?", Order=7, GroupName="Parameters")]
		public bool ColorBars
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private T3TrendFilter[] cacheT3TrendFilter;
		public T3TrendFilter T3TrendFilter(double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool colorBars)
		{
			return T3TrendFilter(Input, volumeFactor, period1, period2, period3, period4, period5, colorBars);
		}

		public T3TrendFilter T3TrendFilter(ISeries<double> input, double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool colorBars)
		{
			if (cacheT3TrendFilter != null)
				for (int idx = 0; idx < cacheT3TrendFilter.Length; idx++)
					if (cacheT3TrendFilter[idx] != null && cacheT3TrendFilter[idx].VolumeFactor == volumeFactor && cacheT3TrendFilter[idx].Period1 == period1 && cacheT3TrendFilter[idx].Period2 == period2 && cacheT3TrendFilter[idx].Period3 == period3 && cacheT3TrendFilter[idx].Period4 == period4 && cacheT3TrendFilter[idx].Period5 == period5 && cacheT3TrendFilter[idx].ColorBars == colorBars && cacheT3TrendFilter[idx].EqualsInput(input))
						return cacheT3TrendFilter[idx];
			return CacheIndicator<T3TrendFilter>(new T3TrendFilter(){ VolumeFactor = volumeFactor, Period1 = period1, Period2 = period2, Period3 = period3, Period4 = period4, Period5 = period5, ColorBars = colorBars }, input, ref cacheT3TrendFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.T3TrendFilter T3TrendFilter(double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool colorBars)
		{
			return indicator.T3TrendFilter(Input, volumeFactor, period1, period2, period3, period4, period5, colorBars);
		}

		public Indicators.T3TrendFilter T3TrendFilter(ISeries<double> input , double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool colorBars)
		{
			return indicator.T3TrendFilter(input, volumeFactor, period1, period2, period3, period4, period5, colorBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.T3TrendFilter T3TrendFilter(double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool colorBars)
		{
			return indicator.T3TrendFilter(Input, volumeFactor, period1, period2, period3, period4, period5, colorBars);
		}

		public Indicators.T3TrendFilter T3TrendFilter(ISeries<double> input , double volumeFactor, int period1, int period2, int period3, int period4, int period5, bool colorBars)
		{
			return indicator.T3TrendFilter(input, volumeFactor, period1, period2, period3, period4, period5, colorBars);
		}
	}
}

#endregion
