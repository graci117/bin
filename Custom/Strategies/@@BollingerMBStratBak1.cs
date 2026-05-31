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
	public class BollingerMBStratBak1 : Strategy
	{
		private ninZaBollingerReversal ninZaBollingerReversal1;
		int barPrintedLong			=  0;
		int barPrintedShort			=  0;
		bool rising					= false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BollingerMBStratBak1";
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
				BarsRequiredToTrade							= 30;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				BBMAType					= ninZa_MAType.SMA;
				BBPeriod					= 13;
				BBSmoothingEnabled					= true;
				BBSmoothingMethod					= ninZa_MAType.EMA;
				BBSmoothingPeriod					= 5;
				BBMinProtrusion					= 1;
				BBSignalSplitBars					= 1;
				BBStdDeviation					= 2.3;
				LongSignal					= @"nLong";
				ShortSignal					= @"nShort";
				LongExitSignal					= @"nLExit";
				ShortExitSignal					= @"nSExit";
				IsLong					= false;
				IsShort					= false;
				this.BBMarkersEnabled = false;
				
			}
			else if (State == State.Configure)
			{
				//ninZaBollingerReversal1.MarkerEnabled = false;
				//RenkoKings_SolarWindRK1.MarkerEnabled = false;
			}
			else if (State == State.DataLoaded)
			{				
				ninZaBollingerReversal1				= ninZaBollingerReversal(Close, BBMAType, Convert.ToInt32(BBPeriod), BBSmoothingEnabled, BBSmoothingMethod, Convert.ToInt32(BBSmoothingPeriod),BBStdDeviation, BBMinProtrusion, Convert.ToInt32(BBSignalSplitBars));
				
				ninZaBollingerReversal1.Plots[0].Brush = Brushes.HotPink;
				ninZaBollingerReversal1.Plots[1].Brush = Brushes.Orange;
				ninZaBollingerReversal1.Plots[2].Brush = Brushes.DodgerBlue;
				ninZaBollingerReversal1.Plots[3].Brush = Brushes.Lavender;
				ninZaBollingerReversal1.Plots[4].Brush = Brushes.DarkGoldenrod;
				//ninZaBollingerReversal1.Plots[5].Brush = Brushes.DarkGoldenrod;
				AddChartIndicator(ninZaBollingerReversal1);
				ninZaBollingerReversal1.MarkerEnabled = this.BBMarkersEnabled;
				ninZaBollingerReversal1.LogoEnabled = false;
				ninZaBollingerReversal1.InstructionEnabled = false;
						//ninZaBollingerReversal1.RegionOpacity = 0;
				
				
				ChartIndicators[0].IsAutoScale = false;

			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 30)
				return;
			
			
			 rising = ninZaBollingerReversal1.IsRising(ninZaBollingerReversal1.MiddleBand);
			//ninZaBollingerReversal1.
			 
			if (
				CrossAbove(Close, ninZaBollingerReversal1.MiddleBand, 1) &&	(rising)
				&&	((IsLong == false) ) 
				)
			{
				Draw.Text(this, Convert.ToString(LongSignal) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );				
					IsLong = true;
					IsShort = false;
				
				
			}
			
			 // Set 2
			if (

				CrossBelow(Close, ninZaBollingerReversal1.MiddleBand, 1)	&&	(!rising)		&&
				 ((IsShort == false)) 
				)
			{
					//Print(ninZaBollingerReversal1.IsRising.ToString());
					
					Draw.Text(this, Convert.ToString(ShortSignal) + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );		
				
					IsShort = true;
					IsLong = false;

			}
			
					//	  Set 3
			if (CrossBelow(Close, ninZaBollingerReversal1.MiddleBand, 1) 				
				 && (IsLong == true))
			{
				Draw.Text(this, Convert.ToString(LongExitSignal) + Convert.ToString(CurrentBars[0]),  "LExit" + System.Environment.NewLine +  @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Pink );
				//Draw.Diamond(this, Convert.ToString(LongExitSignal) + CurrentBar + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (15 * TickSize)) , Brushes.Purple);
				IsLong = false;
				//barPrintedLong = 0; //reset it
			}
			
			 // Set 4
			if ( CrossAbove(Close, ninZaBollingerReversal1.MiddleBand, 1) 			
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
