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
	
	
	public class FollowLine : Indicator
	{
		Series<double> BBUpper;
		Series<double>  BBLower;
		Series<int>  BBSignal;
		Series<int> iTrend;
		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		private int savedLExitBar 		= 0;
		private int	savedSExitBar		= 0;
		private int cd = 0;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "FollowLine";
				Calculate									= Calculate.OnEachTick;
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
				BBPeriod					= 21;
				BBDeviations					= 0.3;
				UseAtrFilter					= true;
				AtrPeriod					= 5;
				HideArrows					= false;
				AddPlot(new Stroke(Brushes.DarkSalmon, 2), PlotStyle.Line, "TrendLine");
				AddPlot(Brushes.Transparent, 	"CrossDetect");
			}
			else if (State == State.Configure)
			{
				iTrend	= new Series<int>(this);
				BBUpper = new Series<double>(this);
				BBLower = new Series<double>(this);
				BBSignal = new Series<int>(this);
				//SMA(Close,BBPeriod) + StdDev(Close,BBPeriod)*BBDeviations;
				//BBUpper = SMA(Close,BBPeriod) - StdDev(Close,BBPeriod)*BBDeviations;
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
			if (CurrentBar < 1)
				return;
			
			
			CrossDetect[0] 	= 0;		
			
			BBUpper[0] = SMA(Close,BBPeriod)[0] + StdDev(Close,BBPeriod)[0]*BBDeviations;
			BBLower[0] = SMA(Close,BBPeriod)[0] - StdDev(Close,BBPeriod)[0]*BBDeviations;
			
			BBSignal[0] = Close[0] > BBUpper[0] ? 1 : (Close[0] < BBLower[0] ? -1 : 0);
			
			 if (BBSignal[0] == 1 && UseAtrFilter)
                TrendLine[0] = Math.Max(Low[0] - ATR(AtrPeriod)[0], TrendLine[1]);
            else if (BBSignal[0] == -1 && UseAtrFilter)
                TrendLine[0] = Math.Min(High[0] + ATR(AtrPeriod)[0], TrendLine[1]);
            else if (BBSignal[0] == 0 && UseAtrFilter)
                TrendLine[0] = TrendLine[1];
            else if (BBSignal[0] == 1 && !UseAtrFilter)
                TrendLine[0] = Math.Max(Low[0], TrendLine[1]);
            else if (BBSignal[0] == -1 && !UseAtrFilter)
                TrendLine[0] = Math.Min(High[0], TrendLine[1]);
            else if (BBSignal[0] == 0 && !UseAtrFilter)
                TrendLine[0] = TrendLine[1];
            else
                TrendLine[0] = TrendLine[1];
			
			//iTrend[0] = TrendLine[0] > TrendLine[1] ? 1 : (TrendLine[0] < TrendLine[1] ? -1 : iTrend[1]);
			iTrend[0] = TrendLine[0] > TrendLine[1] ? 1 : (TrendLine[0] < TrendLine[1] ? -1 : 0);
			
			
				if(iTrend[0] >0) {PlotBrushes[0][0] = Brushes.Blue;}
				else if (iTrend[0] <0) {PlotBrushes[0][0] = Brushes.Orchid;}
				else {PlotBrushes[0][0] = Brushes.Cyan ;}
			
			
			if(iTrend[0] >0
				&& CurrentBar != savedUBar
				&& (cd  == 0 || cd == 2 || cd == -2))
			{
				//PlotBrushes[0][0] = Brushes.Blue;
				savedUBar = CurrentBar; 
				CrossDetect[0] =  1;
				Print("Time" + ToTime(Time[0]) + "1-----" + CurrentBar);
				Draw.ArrowUp (this, "FLineLong"+CurrentBar, true, 0,  Low[0] - 5 * TickSize , Brushes.Green);
				cd = 1;
			}
			else if (iTrend[0] <0
				&& CurrentBar != savedDBar
				&& (cd  == 0 || cd == 2 || cd == -2)) 
			{
				//PlotBrushes[0][0] = Brushes.Orange ;
				savedDBar = CurrentBar; 
				CrossDetect[0] =  -1;
				Draw.ArrowDown (this, "FLineShort"+CurrentBar, true, 0, High[0] + 5 * TickSize, Brushes.Red);
				cd = -1;
			}
			else   if (iTrend[0] == 0){
				//PlotBrushes[0][0] = Brushes.Purple ;
				
			}
			
			
			if  (cd == 1 && CrossBelow(Close,TrendLine,1))
			{
				 CrossDetect[0] =  -2;
				savedDBar = CurrentBar;
				Draw.ArrowDown (this, "FLineLongExit"+CurrentBar, true, 0, High[0] + 5 * TickSize, Brushes.Purple);
				cd = -2;
			}
			
			if (cd == -1 && CrossAbove(Close,TrendLine,1))
			{
				CrossDetect[0] =  2;
				savedUBar = CurrentBar;
				Draw.ArrowUp (this, "FLineShortExit"+CurrentBar, true, 0,  Low[0] - 5 * TickSize , Brushes.Yellow);
				cd = 2;				
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBPeriod", Order=1, GroupName="Parameters")]
		public int BBPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="BBDeviations", Order=2, GroupName="Parameters")]
		public double BBDeviations
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseAtrFilter", Order=3, GroupName="Parameters")]
		public bool UseAtrFilter
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AtrPeriod", Order=4, GroupName="Parameters")]
		public int AtrPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="HideArrows", Order=5, GroupName="Parameters")]
		public bool HideArrows
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrendLine
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CrossDetect
		{
			get { return Values[1]; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FollowLine[] cacheFollowLine;
		public FollowLine FollowLine(int bBPeriod, double bBDeviations, bool useAtrFilter, int atrPeriod, bool hideArrows)
		{
			return FollowLine(Input, bBPeriod, bBDeviations, useAtrFilter, atrPeriod, hideArrows);
		}

		public FollowLine FollowLine(ISeries<double> input, int bBPeriod, double bBDeviations, bool useAtrFilter, int atrPeriod, bool hideArrows)
		{
			if (cacheFollowLine != null)
				for (int idx = 0; idx < cacheFollowLine.Length; idx++)
					if (cacheFollowLine[idx] != null && cacheFollowLine[idx].BBPeriod == bBPeriod && cacheFollowLine[idx].BBDeviations == bBDeviations && cacheFollowLine[idx].UseAtrFilter == useAtrFilter && cacheFollowLine[idx].AtrPeriod == atrPeriod && cacheFollowLine[idx].HideArrows == hideArrows && cacheFollowLine[idx].EqualsInput(input))
						return cacheFollowLine[idx];
			return CacheIndicator<FollowLine>(new FollowLine(){ BBPeriod = bBPeriod, BBDeviations = bBDeviations, UseAtrFilter = useAtrFilter, AtrPeriod = atrPeriod, HideArrows = hideArrows }, input, ref cacheFollowLine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FollowLine FollowLine(int bBPeriod, double bBDeviations, bool useAtrFilter, int atrPeriod, bool hideArrows)
		{
			return indicator.FollowLine(Input, bBPeriod, bBDeviations, useAtrFilter, atrPeriod, hideArrows);
		}

		public Indicators.FollowLine FollowLine(ISeries<double> input , int bBPeriod, double bBDeviations, bool useAtrFilter, int atrPeriod, bool hideArrows)
		{
			return indicator.FollowLine(input, bBPeriod, bBDeviations, useAtrFilter, atrPeriod, hideArrows);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FollowLine FollowLine(int bBPeriod, double bBDeviations, bool useAtrFilter, int atrPeriod, bool hideArrows)
		{
			return indicator.FollowLine(Input, bBPeriod, bBDeviations, useAtrFilter, atrPeriod, hideArrows);
		}

		public Indicators.FollowLine FollowLine(ISeries<double> input , int bBPeriod, double bBDeviations, bool useAtrFilter, int atrPeriod, bool hideArrows)
		{
			return indicator.FollowLine(input, bBPeriod, bBDeviations, useAtrFilter, atrPeriod, hideArrows);
		}
	}
}

#endregion
