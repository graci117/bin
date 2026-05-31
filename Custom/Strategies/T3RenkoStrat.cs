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
	public class T3RenkoStrat : Strategy
	{
		private bool IsLong;
		private bool IsShort;

		private TillsonT3 TillsonT31;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "T3RenkoStrat";
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
				T3Length					= 22;
				VolumeFactor					= 0.6;
				LongSignal					= @"T3Long";
				ShortSignal					= @"T3Short";
				ExitSignal					= @"T3Exit";
				IsLong					= false;
				IsShort					= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				TillsonT31				= TillsonT3(Close, Convert.ToInt32(T3Length), VolumeFactor);
				TillsonT31.Plots[0].Brush = Brushes.Yellow;
				AddChartIndicator(TillsonT31);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((Low[0] > TillsonT31[0])
				 && (High[0] > TillsonT31[0])
				 && (IsLong != true)
				)
			{
				//Draw.ArrowUp(this, Convert.ToString(LongSignal) + " " + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] - 20) , Brushes.Lime);
				Draw.Text(this, Convert.ToString(LongSignal) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
				IsLong = true;
			}
			
			 // Set 2
			if ((High[0] < TillsonT31[0])
				 && (Low[0] < TillsonT31[0])
				 && (IsShort != true)
				)
			{
				//Draw.ArrowDown(this, Convert.ToString(ShortSignal) + " " + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + 20) , Brushes.Red);
				Draw.Text(this, Convert.ToString(ShortSignal) + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.DarkRed );
						
				IsShort = true;
			}
			
			 // Set 3
			if (
				 // Condition group 1
				((High[0] > TillsonT31[0])
				 && (Low[0] < TillsonT31[0])
				 && (IsLong == true)))
			{
				//Draw.ArrowDown(this, Convert.ToString(ExitSignal) + " " + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + 20) , Brushes.Salmon);
				//Draw.Text(this, Convert.ToString(ExitSignal) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Exit", 0, (Low[0] + (-12 * TickSize)), Brushes.Salmon );	
				IsLong = false;
			}
			
			 // Set 4
			if ((Low[0] < TillsonT31[0])
				 && (High[0] > TillsonT31[0])
				 && (IsShort == true))
			{
				//Draw.ArrowUp(this, Convert.ToString(ExitSignal) + " " + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] - 20) , Brushes.MediumOrchid);
				//Draw.Text(this, Convert.ToString(ExitSignal) + Convert.ToString(CurrentBars[0]), "Exit" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.DarkCyan );
				IsShort = false;
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="T3Length", Order=1, GroupName="Parameters")]
		public int T3Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="VolumeFactor", Order=2, GroupName="Parameters")]
		public double VolumeFactor
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongSignal", Order=3, GroupName="Parameters")]
		public string LongSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortSignal", Order=4, GroupName="Parameters")]
		public string ShortSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ExitSignal", Order=5, GroupName="Parameters")]
		public string ExitSignal
		{ get; set; }
		#endregion

	}
}
