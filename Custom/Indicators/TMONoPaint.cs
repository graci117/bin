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
	public class TMONoPaint : Indicator
	{
		private Series<double> ma;
		private Series<double> data;
		private Series<double> main;	
		private Series<double> signal;
		private Series<double> obline, osline, upperline, lowerline;
		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		private Brush UpColor;
		private Brush DownColor;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TMONoPaint";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Length					= 14;
				CalcLength					= 5;
				SmoothLength					= 3;
				
				AddPlot(Brushes.Green, "Main");
				AddPlot(Brushes.Green, "Signal");
				AddPlot(Brushes.Transparent, "Cross");
				AddLine(Brushes.Gray,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				
				AddLine(Brushes.Gray,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				
				
				
				
			}
			else if (State == State.Configure)
			{
				AddLine(Brushes.Orange,	Math.Round(Length * 0.7),	"ob");
				AddLine(Brushes.Orange,	-(Math.Round(Length * 0.7)),	"os");
				AddLine(Brushes.Green,	Length,	"upper");
				AddLine(Brushes.Red,	-Length,	"lower");;
				
			}
			else if (State == State.DataLoaded)
            {
				ma = new Series<double>(this);
                data = new Series<double>(this);
				main = new Series<double>(this);
				signal = new Series<double>(this);
               	obline = new Series<double>(this, MaximumBarsLookBack.Infinite);
				osline = new Series<double>(this, MaximumBarsLookBack.Infinite);
				upperline = new Series<double>(this, MaximumBarsLookBack.Infinite);
				lowerline = new Series<double>(this, MaximumBarsLookBack.Infinite);
            }
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 35)
				return;
			
			
			double o = Open[0];
            double c = Close[0];
            int s = 0;
			Cross[0] 	= 0;				
			

            for (int i = 0; i <= Length; i++)
                s += c > Open[i] ? 1 : c < Open[i] ? -1 : 0;

            data[0] = s;			
			

            ma = EMA(data, CalcLength).Value;
            main= EMA(ma, SmoothLength).Value;
            signal = EMA(main, SmoothLength).Value;			
			
			
			Main[0] = main[0];
			Signal[0] = signal[0];
			
			
			if (CrossAbove (Main, Signal, 1) && CurrentBar != savedUBar )
			{
				savedUBar = CurrentBar;  		// once per bar only
				Cross[0] =  1;
				
			}
			
			
			if (CrossBelow (Main, Signal, 1) && CurrentBar != savedDBar )
			{
				savedDBar = CurrentBar;			// once per bar only
				Cross[0] = -1;
				//DoActions();		
			}
						
			
		
			
			//main			
			if(Main[0]>Signal[0]) {PlotBrushes[0][0] = Brushes.Green;}
			else {PlotBrushes[0][0] = Brushes.Red;}
			
			//signal
			if(Main[0]>Signal[0]) {PlotBrushes[1][0] = Brushes.Green;}
			else {PlotBrushes[1][0] = Brushes.Red;}
			
			
			
			upperline[0] = Length;
			lowerline[0] = -Length;
			obline[0] = Math.Round(Length * 0.7);
			osline[0] = -(Math.Round(Length * 0.7));
				
			Draw.Region(this, "obToUpper" + CurrentBar, CurrentBar, 0, upperline, obline, Brushes.LightGreen, Brushes.LightGreen, 10);
			Draw.Region(this, "osToLower" + CurrentBar, CurrentBar, 0, lowerline, osline, Brushes.IndianRed, Brushes.IndianRed, 10);
			
			if (Cross[0] == 1 || (savedUBar > savedDBar))
				//if (Cross[0] == 1 )
				{
					Draw.Region(this, "Up" + CurrentBar, CurrentBar - savedUBar + 1, 0, Values[0],Values[1], Brushes.DarkGreen, Brushes.DarkGreen, 70);
				
				}
	
			if (Cross[0] == -1 || (savedDBar > savedUBar))
				//if (Cross[0] == -1 )
				{
					Draw.Region(this, "Dwn" + CurrentBar, CurrentBar - savedDBar + 1, 0, Values[0],Values[1], Brushes.Maroon, Brushes.Maroon, 70);
					
						
				}	
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Description="Length", Order=1, GroupName="Parameters")]
		public int Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CalcLength", Description="Calc Length", Order=2, GroupName="Parameters")]
		public int CalcLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmoothLength", Description="Smooth Length", Order=3, GroupName="Parameters")]
		public int SmoothLength
		{ get; set; }

		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Main
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Cross
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
		private TMONoPaint[] cacheTMONoPaint;
		public TMONoPaint TMONoPaint(int length, int calcLength, int smoothLength)
		{
			return TMONoPaint(Input, length, calcLength, smoothLength);
		}

		public TMONoPaint TMONoPaint(ISeries<double> input, int length, int calcLength, int smoothLength)
		{
			if (cacheTMONoPaint != null)
				for (int idx = 0; idx < cacheTMONoPaint.Length; idx++)
					if (cacheTMONoPaint[idx] != null && cacheTMONoPaint[idx].Length == length && cacheTMONoPaint[idx].CalcLength == calcLength && cacheTMONoPaint[idx].SmoothLength == smoothLength && cacheTMONoPaint[idx].EqualsInput(input))
						return cacheTMONoPaint[idx];
			return CacheIndicator<TMONoPaint>(new TMONoPaint(){ Length = length, CalcLength = calcLength, SmoothLength = smoothLength }, input, ref cacheTMONoPaint);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TMONoPaint TMONoPaint(int length, int calcLength, int smoothLength)
		{
			return indicator.TMONoPaint(Input, length, calcLength, smoothLength);
		}

		public Indicators.TMONoPaint TMONoPaint(ISeries<double> input , int length, int calcLength, int smoothLength)
		{
			return indicator.TMONoPaint(input, length, calcLength, smoothLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TMONoPaint TMONoPaint(int length, int calcLength, int smoothLength)
		{
			return indicator.TMONoPaint(Input, length, calcLength, smoothLength);
		}

		public Indicators.TMONoPaint TMONoPaint(ISeries<double> input , int length, int calcLength, int smoothLength)
		{
			return indicator.TMONoPaint(input, length, calcLength, smoothLength);
		}
	}
}

#endregion
