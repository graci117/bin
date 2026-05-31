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
	public class DouceurDeLiberte : Indicator
	{
		
		private NinjaTrader.NinjaScript.Indicators.RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK1;
		private ninZaBollingerReversal ninZaBollingerReversal1;
		int barPrintedLong			=  0;
		int barPrintedShort			=  0;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DouceurDeLiberte";
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
				
				SWOffsetMultiTrend					= 30;
				SWMultiTrendStop					= 60;
				SWSlowdownScan					= 5;
				SWWeakSplitBars					= 10;
				SWPullbackEarly					= true;
				SWPullbackSplitBars					= 10;
				BBMAType					= ninZa_MAType.EMA;
				BBPeriod					= 30;
				BBSmoothingEnabled					= true;
				BBSmoothingMethod					= ninZa_MAType.WilderMA;
				BBSmoothingPeriod					= 6;
				BBMinProtrusion					= 1;
				BBSignalSplitBars					= 1;
				BBStdDeviation					= 2.7;
				LongSignal					= @"nLong";
				ShortSignal					= @"nShort";
				LongExitSignal					= @"nLExit";
				ShortExitSignal					= @"nSExit";
				IsLong					= false;
				IsShort					= false;
				this.BBMarkersEnabled = false;
				this.SWMarkersEnabled = false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				RenkoKings_SolarWindRK1				= RenkoKings_SolarWindRK(Close, SWOffsetMultiTrend, SWMultiTrendStop, Convert.ToInt32(SWSlowdownScan), Convert.ToInt32(SWWeakSplitBars), SWPullbackEarly, Convert.ToInt32(SWPullbackSplitBars));
				ninZaBollingerReversal1				= ninZaBollingerReversal(Close, BBMAType, Convert.ToInt32(BBPeriod), BBSmoothingEnabled, BBSmoothingMethod, Convert.ToInt32(BBSmoothingPeriod),BBStdDeviation, BBMinProtrusion, Convert.ToInt32(BBSignalSplitBars));
				RenkoKings_SolarWindRK1.Plots[0].Brush = Brushes.Crimson;
				RenkoKings_SolarWindRK1.Plots[1].Brush = Brushes.Yellow;
				RenkoKings_SolarWindRK1.Plots[2].Brush = Brushes.Lavender;
				RenkoKings_SolarWindRK1.Plots[3].Brush = Brushes.Goldenrod;
				RenkoKings_SolarWindRK1.Plots[4].Brush = Brushes.Brown;
				ninZaBollingerReversal1.Plots[0].Brush = Brushes.HotPink;
				ninZaBollingerReversal1.Plots[1].Brush = Brushes.Orange;
				ninZaBollingerReversal1.Plots[2].Brush = Brushes.DodgerBlue;
				ninZaBollingerReversal1.Plots[3].Brush = Brushes.Lavender;
				ninZaBollingerReversal1.Plots[4].Brush = Brushes.DarkGoldenrod;
				//ninZaBollingerReversal1.Plots[5].Brush = Brushes.DarkGoldenrod;
				//AddChartIndicator(RenkoKings_SolarWindRK1);
				//AddChartIndicator(ninZaBollingerReversal1);
				ninZaBollingerReversal1.MarkerEnabled = this.BBMarkersEnabled;
				RenkoKings_SolarWindRK1.MarkerEnabled = this.SWMarkersEnabled;
				ninZaBollingerReversal1.LogoEnabled = false;
				RenkoKings_SolarWindRK1.LogoEnabled = false;
				ninZaBollingerReversal1.InstructionEnabled = false;
				RenkoKings_SolarWindRK1.InstructionEnabled = false;
				RenkoKings_SolarWindRK1.BackgroundEnabled = false;
				//ninZaBollingerReversal1.RegionOpacity = 0;
				
				//ninZaBollingerReversal1.autosca .IsAutoScale = false;
				//ChartIndicators[1].IsAutoScale = false;

			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((RenkoKings_SolarWindRK1.Signal_Trend[0] > 0)
				 && (ninZaBollingerReversal1.Signal_Trade[0] > 0)
				 && ((IsLong == false) ) 
				
				)
			{
					Print("long---" + Time[0]);
					Draw.Text(this, Convert.ToString(LongSignal) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );				
					IsLong = true;
					//IsShort = false;
			}
			
			 // Set 2
			if ((RenkoKings_SolarWindRK1.Signal_Trend[0] < 0)
				 && (ninZaBollingerReversal1.Signal_Trade[0] < 0)
				 && ((IsShort == false)) 
				)
			{
					Print("short---" + Time[0]);
					Draw.Text(this, Convert.ToString(ShortSignal) + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );
					IsShort = true;
					//IsLong = true;

			}
			
						 // Set 3
			if ((ninZaBollingerReversal1.Signal_Trade[0] == 0)
				 && (IsLong == true))
			{
				Draw.Text(this, Convert.ToString(LongExitSignal) + Convert.ToString(CurrentBars[0]),  "LExit" + System.Environment.NewLine +  @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Purple );
				//Draw.Diamond(this, Convert.ToString(LongExitSignal) + CurrentBar + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (15 * TickSize)) , Brushes.Purple);
				IsLong = false;
				//barPrintedLong = 0; //reset it
			}
			
			 // Set 4
			if ((ninZaBollingerReversal1.Signal_Trade[0] == 0)
				 && (IsShort == true))
			{
				Draw.Text(this, Convert.ToString(ShortExitSignal) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "SExit", 0, (Low[0] + (-12 * TickSize)), Brushes.GreenYellow );
				//Draw.Diamond(this, Convert.ToString(ShortExitSignal) + CurrentBar + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] - (15 * TickSize)) , Brushes.GreenYellow);
				IsShort = false;
				//barPrintedShort = 0; //reset it
			}
		}
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SWOffsetMultiTrend", Description="Offset Multi Trend", Order=1, GroupName="SolarWind")]
		public int SWOffsetMultiTrend
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SWMultiTrendStop", Description="Multi Trend Stop", Order=2, GroupName="SolarWind")]
		public int SWMultiTrendStop
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SWSlowdownScan", Description="Slowdown Scan", Order=3, GroupName="SolarWind")]
		public int SWSlowdownScan
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SWWeakSplitBars", Description="Weak-Split Bars", Order=4, GroupName="SolarWind")]
		public int SWWeakSplitBars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SWPullbackEarly", Description="PullbackEarly", Order=5, GroupName="SolarWind")]
		public bool SWPullbackEarly
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SWPullbackSplitBars", Description="Pullback Split Bars", Order=6, GroupName="SolarWind")]
		public int SWPullbackSplitBars
		{ get; set; }

		[NinjaScriptProperty]		
		[Display(Name="MA Type", Order=7, GroupName="Bollinger Reversal Pro")]
		public ninZa_MAType BBMAType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBPeriod", Description="Period", Order=8, GroupName="Bollinger Reversal Pro")]
		public int BBPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BBSmoothingEnabled", Order=9, GroupName="Bollinger Reversal Pro")]
		public bool BBSmoothingEnabled
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBSmoothingMethod", Description="SmoothingMethod", Order=10, GroupName="Bollinger Reversal Pro")]
		public ninZa_MAType BBSmoothingMethod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBSmoothingPeriod", Order=11, GroupName="Bollinger Reversal Pro")]
		public int BBSmoothingPeriod
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Standard Deviation", Order=11, GroupName="Bollinger Reversal Pro")]
		public double BBStdDeviation
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBMinProtrusion", Description="Minimum Protrusion", Order=12, GroupName="Bollinger Reversal Pro")]
		public int BBMinProtrusion
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BBSignalSplitBars", Order=13, GroupName="Bollinger Reversal Pro")]
		public int BBSignalSplitBars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongSignal", Order=14, GroupName="Bollinger Reversal Pro")]
		public string LongSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortSignal", Order=15, GroupName="Bollinger Reversal Pro")]
		public string ShortSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongExitSignal", Order=16, GroupName="Bollinger Reversal Pro")]
		public string LongExitSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortExitSignal", Order=17, GroupName="Bollinger Reversal Pro")]
		public string ShortExitSignal
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Bollinger Markers Enabled", Order=18, GroupName="Parameters")]
		public bool BBMarkersEnabled
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="SolarWind Markers Enabled", Order=19, GroupName="Parameters")]
		public bool SWMarkersEnabled
		{ get; set; }



		[NinjaScriptProperty]
		//[Display(Name="IsLong", Order=18, GroupName="Parameters")]
		public bool IsLong
		{ get; set; }

		[NinjaScriptProperty]
		//[Display(Name="IsShort", Order=19, GroupName="Parameters")]
		public bool IsShort
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DouceurDeLiberte[] cacheDouceurDeLiberte;
		public DouceurDeLiberte DouceurDeLiberte(int sWOffsetMultiTrend, int sWMultiTrendStop, int sWSlowdownScan, int sWWeakSplitBars, bool sWPullbackEarly, int sWPullbackSplitBars, ninZa_MAType bBMAType, int bBPeriod, bool bBSmoothingEnabled, ninZa_MAType bBSmoothingMethod, int bBSmoothingPeriod, double bBStdDeviation, int bBMinProtrusion, int bBSignalSplitBars, string longSignal, string shortSignal, string longExitSignal, string shortExitSignal, bool bBMarkersEnabled, bool sWMarkersEnabled, bool isLong, bool isShort)
		{
			return DouceurDeLiberte(Input, sWOffsetMultiTrend, sWMultiTrendStop, sWSlowdownScan, sWWeakSplitBars, sWPullbackEarly, sWPullbackSplitBars, bBMAType, bBPeriod, bBSmoothingEnabled, bBSmoothingMethod, bBSmoothingPeriod, bBStdDeviation, bBMinProtrusion, bBSignalSplitBars, longSignal, shortSignal, longExitSignal, shortExitSignal, bBMarkersEnabled, sWMarkersEnabled, isLong, isShort);
		}

		public DouceurDeLiberte DouceurDeLiberte(ISeries<double> input, int sWOffsetMultiTrend, int sWMultiTrendStop, int sWSlowdownScan, int sWWeakSplitBars, bool sWPullbackEarly, int sWPullbackSplitBars, ninZa_MAType bBMAType, int bBPeriod, bool bBSmoothingEnabled, ninZa_MAType bBSmoothingMethod, int bBSmoothingPeriod, double bBStdDeviation, int bBMinProtrusion, int bBSignalSplitBars, string longSignal, string shortSignal, string longExitSignal, string shortExitSignal, bool bBMarkersEnabled, bool sWMarkersEnabled, bool isLong, bool isShort)
		{
			if (cacheDouceurDeLiberte != null)
				for (int idx = 0; idx < cacheDouceurDeLiberte.Length; idx++)
					if (cacheDouceurDeLiberte[idx] != null && cacheDouceurDeLiberte[idx].SWOffsetMultiTrend == sWOffsetMultiTrend && cacheDouceurDeLiberte[idx].SWMultiTrendStop == sWMultiTrendStop && cacheDouceurDeLiberte[idx].SWSlowdownScan == sWSlowdownScan && cacheDouceurDeLiberte[idx].SWWeakSplitBars == sWWeakSplitBars && cacheDouceurDeLiberte[idx].SWPullbackEarly == sWPullbackEarly && cacheDouceurDeLiberte[idx].SWPullbackSplitBars == sWPullbackSplitBars && cacheDouceurDeLiberte[idx].BBMAType == bBMAType && cacheDouceurDeLiberte[idx].BBPeriod == bBPeriod && cacheDouceurDeLiberte[idx].BBSmoothingEnabled == bBSmoothingEnabled && cacheDouceurDeLiberte[idx].BBSmoothingMethod == bBSmoothingMethod && cacheDouceurDeLiberte[idx].BBSmoothingPeriod == bBSmoothingPeriod && cacheDouceurDeLiberte[idx].BBStdDeviation == bBStdDeviation && cacheDouceurDeLiberte[idx].BBMinProtrusion == bBMinProtrusion && cacheDouceurDeLiberte[idx].BBSignalSplitBars == bBSignalSplitBars && cacheDouceurDeLiberte[idx].LongSignal == longSignal && cacheDouceurDeLiberte[idx].ShortSignal == shortSignal && cacheDouceurDeLiberte[idx].LongExitSignal == longExitSignal && cacheDouceurDeLiberte[idx].ShortExitSignal == shortExitSignal && cacheDouceurDeLiberte[idx].BBMarkersEnabled == bBMarkersEnabled && cacheDouceurDeLiberte[idx].SWMarkersEnabled == sWMarkersEnabled && cacheDouceurDeLiberte[idx].IsLong == isLong && cacheDouceurDeLiberte[idx].IsShort == isShort && cacheDouceurDeLiberte[idx].EqualsInput(input))
						return cacheDouceurDeLiberte[idx];
			return CacheIndicator<DouceurDeLiberte>(new DouceurDeLiberte(){ SWOffsetMultiTrend = sWOffsetMultiTrend, SWMultiTrendStop = sWMultiTrendStop, SWSlowdownScan = sWSlowdownScan, SWWeakSplitBars = sWWeakSplitBars, SWPullbackEarly = sWPullbackEarly, SWPullbackSplitBars = sWPullbackSplitBars, BBMAType = bBMAType, BBPeriod = bBPeriod, BBSmoothingEnabled = bBSmoothingEnabled, BBSmoothingMethod = bBSmoothingMethod, BBSmoothingPeriod = bBSmoothingPeriod, BBStdDeviation = bBStdDeviation, BBMinProtrusion = bBMinProtrusion, BBSignalSplitBars = bBSignalSplitBars, LongSignal = longSignal, ShortSignal = shortSignal, LongExitSignal = longExitSignal, ShortExitSignal = shortExitSignal, BBMarkersEnabled = bBMarkersEnabled, SWMarkersEnabled = sWMarkersEnabled, IsLong = isLong, IsShort = isShort }, input, ref cacheDouceurDeLiberte);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DouceurDeLiberte DouceurDeLiberte(int sWOffsetMultiTrend, int sWMultiTrendStop, int sWSlowdownScan, int sWWeakSplitBars, bool sWPullbackEarly, int sWPullbackSplitBars, ninZa_MAType bBMAType, int bBPeriod, bool bBSmoothingEnabled, ninZa_MAType bBSmoothingMethod, int bBSmoothingPeriod, double bBStdDeviation, int bBMinProtrusion, int bBSignalSplitBars, string longSignal, string shortSignal, string longExitSignal, string shortExitSignal, bool bBMarkersEnabled, bool sWMarkersEnabled, bool isLong, bool isShort)
		{
			return indicator.DouceurDeLiberte(Input, sWOffsetMultiTrend, sWMultiTrendStop, sWSlowdownScan, sWWeakSplitBars, sWPullbackEarly, sWPullbackSplitBars, bBMAType, bBPeriod, bBSmoothingEnabled, bBSmoothingMethod, bBSmoothingPeriod, bBStdDeviation, bBMinProtrusion, bBSignalSplitBars, longSignal, shortSignal, longExitSignal, shortExitSignal, bBMarkersEnabled, sWMarkersEnabled, isLong, isShort);
		}

		public Indicators.DouceurDeLiberte DouceurDeLiberte(ISeries<double> input , int sWOffsetMultiTrend, int sWMultiTrendStop, int sWSlowdownScan, int sWWeakSplitBars, bool sWPullbackEarly, int sWPullbackSplitBars, ninZa_MAType bBMAType, int bBPeriod, bool bBSmoothingEnabled, ninZa_MAType bBSmoothingMethod, int bBSmoothingPeriod, double bBStdDeviation, int bBMinProtrusion, int bBSignalSplitBars, string longSignal, string shortSignal, string longExitSignal, string shortExitSignal, bool bBMarkersEnabled, bool sWMarkersEnabled, bool isLong, bool isShort)
		{
			return indicator.DouceurDeLiberte(input, sWOffsetMultiTrend, sWMultiTrendStop, sWSlowdownScan, sWWeakSplitBars, sWPullbackEarly, sWPullbackSplitBars, bBMAType, bBPeriod, bBSmoothingEnabled, bBSmoothingMethod, bBSmoothingPeriod, bBStdDeviation, bBMinProtrusion, bBSignalSplitBars, longSignal, shortSignal, longExitSignal, shortExitSignal, bBMarkersEnabled, sWMarkersEnabled, isLong, isShort);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DouceurDeLiberte DouceurDeLiberte(int sWOffsetMultiTrend, int sWMultiTrendStop, int sWSlowdownScan, int sWWeakSplitBars, bool sWPullbackEarly, int sWPullbackSplitBars, ninZa_MAType bBMAType, int bBPeriod, bool bBSmoothingEnabled, ninZa_MAType bBSmoothingMethod, int bBSmoothingPeriod, double bBStdDeviation, int bBMinProtrusion, int bBSignalSplitBars, string longSignal, string shortSignal, string longExitSignal, string shortExitSignal, bool bBMarkersEnabled, bool sWMarkersEnabled, bool isLong, bool isShort)
		{
			return indicator.DouceurDeLiberte(Input, sWOffsetMultiTrend, sWMultiTrendStop, sWSlowdownScan, sWWeakSplitBars, sWPullbackEarly, sWPullbackSplitBars, bBMAType, bBPeriod, bBSmoothingEnabled, bBSmoothingMethod, bBSmoothingPeriod, bBStdDeviation, bBMinProtrusion, bBSignalSplitBars, longSignal, shortSignal, longExitSignal, shortExitSignal, bBMarkersEnabled, sWMarkersEnabled, isLong, isShort);
		}

		public Indicators.DouceurDeLiberte DouceurDeLiberte(ISeries<double> input , int sWOffsetMultiTrend, int sWMultiTrendStop, int sWSlowdownScan, int sWWeakSplitBars, bool sWPullbackEarly, int sWPullbackSplitBars, ninZa_MAType bBMAType, int bBPeriod, bool bBSmoothingEnabled, ninZa_MAType bBSmoothingMethod, int bBSmoothingPeriod, double bBStdDeviation, int bBMinProtrusion, int bBSignalSplitBars, string longSignal, string shortSignal, string longExitSignal, string shortExitSignal, bool bBMarkersEnabled, bool sWMarkersEnabled, bool isLong, bool isShort)
		{
			return indicator.DouceurDeLiberte(input, sWOffsetMultiTrend, sWMultiTrendStop, sWSlowdownScan, sWWeakSplitBars, sWPullbackEarly, sWPullbackSplitBars, bBMAType, bBPeriod, bBSmoothingEnabled, bBSmoothingMethod, bBSmoothingPeriod, bBStdDeviation, bBMinProtrusion, bBSignalSplitBars, longSignal, shortSignal, longExitSignal, shortExitSignal, bBMarkersEnabled, sWMarkersEnabled, isLong, isShort);
		}
	}
}

#endregion
