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
	public class FloatingLevelsUp : Indicator
	{
		private SMA avg;
		private SMA minAvg;
		private SMA maxAvg;
		
		private Series<double> maAvgSeries;
		private Series<double> maHighSeries;
		private Series<double> maLowSeries;	
		private Series<double> maHighSeriesTmp;
		private Series<double> maLowSeriesTmp;	
		private int savedUBar 		= 0;
		private int savedDBar 		= 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "FloatingLevels";
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
				AveragePeriod					= 14;
				LevelsPeriod					= 35;
				LevelsUpPercent					= 90;
				LevelsDownPercent					= 10;
				ShowSignals					= true;
				
				maAvgSeries	= new Series<double>(this);	
				maHighSeries = new Series<double>(this);
				maLowSeries = new Series<double>(this);	
				
				
				//AddPlot(Brushes.Yellow, "Average");
				AddPlot(Brushes.Green, 		"LevelUp");
				AddPlot(Brushes.Red, 	"LevelDown");
				//AddPlot(Brushes.Transparent, 	"CrossDetect");
			
				
				
				
				
//				AddPlot(Brushes.Orange, "Average");
//				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Dot, "LevelUp");
//				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "LevelDown");
			}
			else if (State == State.Configure)
			{
				Plots[0].Width 				= 1;
				Plots[0].PlotStyle			= PlotStyle.Dot;
				//Plots[0].DashStyleHelper	= DashStyle.;
				Plots[0].Brush				= Brushes.Green;
				
				
				Plots[1].Width 				= 1;
				Plots[1].PlotStyle			= PlotStyle.Dot;
				//Plots[1].DashStyleHelper	= MA1DashStyle;
				Plots[1].Brush 				= Brushes.Red;
				
			}
			else if (State == State.DataLoaded)
			{			
				maAvgSeries					= SMA(Close, Convert.ToInt32(AveragePeriod)).Value;
				maHighSeriesTmp				= MAX(maAvgSeries, Math.Max(LevelsPeriod,1)).Value;
				maLowSeriesTmp				= MIN(maAvgSeries, Math.Max(LevelsPeriod,1)).Value;
				maHighSeries				= new Series<double>(this);
				maLowSeries				= new Series<double>(this);
				//EMA(Close, Convert.ToInt32(5));
				//FloatingUp = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 1)
				return;
			//plot levelUp   = minAverage + (maxAverage - minAverage) * levelsUpPercent / 100.0;
			maHighSeries[0] = maLowSeriesTmp[0] + ((maHighSeriesTmp[0] - maLowSeriesTmp[0]) * LevelsUpPercent/100);
			maLowSeries[0] = maLowSeriesTmp[0] + ((maHighSeriesTmp[0] - maLowSeriesTmp[0]) * LevelsDownPercent/100);
			
			//Value[0]  		= maHighSeries[0];		// Plot the selected MA slow
			//Value[1]  		= maLowSeries[0];	
			
			LevelUp[0] 		= maHighSeries[0];		// Plot the selected MA slow
			LevelDown[0] 	= maLowSeries[0];
			
			//PlotBrushes[0][0] = IsRising(Average) ? MA0RisingColor : IsFalling(FstMA) ? MA0FallingColor : MA0FlatColor;

			
			PlotBrushes[0][0] = Brushes.Green;
			PlotBrushes[1][0] = Brushes.Red;
			
			
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="AveragePeriod", Order=1, GroupName="Parameters")]
		public int AveragePeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="LevelsPeriod", Order=2, GroupName="Parameters")]
		public int LevelsPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(60, int.MaxValue)]
		[Display(Name="LevelsUpPercent", Order=3, GroupName="Parameters")]
		public int LevelsUpPercent
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="LevelsDownPercent", Order=4, GroupName="Parameters")]
		public int LevelsDownPercent
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowSignals", Order=5, GroupName="Parameters")]
		public bool ShowSignals
		{ get; set; }

	
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LevelUp
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LevelDown
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
		private FloatingLevelsUp[] cacheFloatingLevelsUp;
		public FloatingLevelsUp FloatingLevelsUp(int averagePeriod, int levelsPeriod, int levelsUpPercent, int levelsDownPercent, bool showSignals)
		{
			return FloatingLevelsUp(Input, averagePeriod, levelsPeriod, levelsUpPercent, levelsDownPercent, showSignals);
		}

		public FloatingLevelsUp FloatingLevelsUp(ISeries<double> input, int averagePeriod, int levelsPeriod, int levelsUpPercent, int levelsDownPercent, bool showSignals)
		{
			if (cacheFloatingLevelsUp != null)
				for (int idx = 0; idx < cacheFloatingLevelsUp.Length; idx++)
					if (cacheFloatingLevelsUp[idx] != null && cacheFloatingLevelsUp[idx].AveragePeriod == averagePeriod && cacheFloatingLevelsUp[idx].LevelsPeriod == levelsPeriod && cacheFloatingLevelsUp[idx].LevelsUpPercent == levelsUpPercent && cacheFloatingLevelsUp[idx].LevelsDownPercent == levelsDownPercent && cacheFloatingLevelsUp[idx].ShowSignals == showSignals && cacheFloatingLevelsUp[idx].EqualsInput(input))
						return cacheFloatingLevelsUp[idx];
			return CacheIndicator<FloatingLevelsUp>(new FloatingLevelsUp(){ AveragePeriod = averagePeriod, LevelsPeriod = levelsPeriod, LevelsUpPercent = levelsUpPercent, LevelsDownPercent = levelsDownPercent, ShowSignals = showSignals }, input, ref cacheFloatingLevelsUp);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FloatingLevelsUp FloatingLevelsUp(int averagePeriod, int levelsPeriod, int levelsUpPercent, int levelsDownPercent, bool showSignals)
		{
			return indicator.FloatingLevelsUp(Input, averagePeriod, levelsPeriod, levelsUpPercent, levelsDownPercent, showSignals);
		}

		public Indicators.FloatingLevelsUp FloatingLevelsUp(ISeries<double> input , int averagePeriod, int levelsPeriod, int levelsUpPercent, int levelsDownPercent, bool showSignals)
		{
			return indicator.FloatingLevelsUp(input, averagePeriod, levelsPeriod, levelsUpPercent, levelsDownPercent, showSignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FloatingLevelsUp FloatingLevelsUp(int averagePeriod, int levelsPeriod, int levelsUpPercent, int levelsDownPercent, bool showSignals)
		{
			return indicator.FloatingLevelsUp(Input, averagePeriod, levelsPeriod, levelsUpPercent, levelsDownPercent, showSignals);
		}

		public Indicators.FloatingLevelsUp FloatingLevelsUp(ISeries<double> input , int averagePeriod, int levelsPeriod, int levelsUpPercent, int levelsDownPercent, bool showSignals)
		{
			return indicator.FloatingLevelsUp(input, averagePeriod, levelsPeriod, levelsUpPercent, levelsDownPercent, showSignals);
		}
	}
}

#endregion
