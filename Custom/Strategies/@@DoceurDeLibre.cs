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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class DoceurDeLibre : Strategy
	{
		private NinjaTrader.NinjaScript.Indicators.RenkoKings.RenkoKings_SolarWindRK RenkoKings_SolarWindRK1;
		private ninZaBollingerReversal ninZaBollingerReversal1;
		int barPrintedLong			=  0;
		int barPrintedShort			=  0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "DoceurDeLibre";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
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
				//ninZaBollingerReversal1.MarkerEnabled = false;
				//RenkoKings_SolarWindRK1.MarkerEnabled = false;
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
				AddChartIndicator(RenkoKings_SolarWindRK1);
				AddChartIndicator(ninZaBollingerReversal1);
				ninZaBollingerReversal1.MarkerEnabled = this.BBMarkersEnabled;
				RenkoKings_SolarWindRK1.MarkerEnabled = this.SWMarkersEnabled;
				ninZaBollingerReversal1.LogoEnabled = false;
				RenkoKings_SolarWindRK1.LogoEnabled = false;
				ninZaBollingerReversal1.InstructionEnabled = false;
				RenkoKings_SolarWindRK1.InstructionEnabled = false;
				RenkoKings_SolarWindRK1.BackgroundEnabled = false;
				//ninZaBollingerReversal1.RegionOpacity = 0;
				
				ChartIndicators[0].IsAutoScale = false;
				ChartIndicators[1].IsAutoScale = false;

			}
		}

		protected override void OnBarUpdate()
		{
//			if (BarsInProgress != 30) 
//				return;

			if (CurrentBars[0] < 30)
				return;
			
			Print(ninZaBollingerReversal1.IsRising(ninZaBollingerReversal1.MiddleBand).ToString() + "-----"+ Time[0]);
			
			 // Set 1
			if ((RenkoKings_SolarWindRK1.Signal_Trend[0] > 0)
				 && (ninZaBollingerReversal1.Signal_Trade[0] > 0)
				 && ((IsLong == false) ) 
				
				)
			{
					//Print("long---" + Time[0]);
				//Print(ninZaBollingerReversal1.IsRising.ToString());
				//ninZaBollingerReversal1.
					Print (ninZaBollingerReversal1.Plots[1].Brush.ToString());//   .ToString() + "------" + Time[0]);
					
				
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
					//Print(ninZaBollingerReversal1.IsRising.ToString());
					Print("test---" + Time[0]);
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
