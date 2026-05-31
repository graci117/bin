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
	public class BreakEvenBuilderExample : Strategy
	{
		private double StopPrice;
		private double TriggerPrice;
		private int TriggerState;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "BreakEvenBuilderExample";
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
				BreakEvenTrigger					= 5;
				InitialStopDistance					= -10;
				StopPrice					= 0;
				TriggerPrice					= 0;
				TriggerState					= 0;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			 // Set 1
			if ((TriggerState >= 2)
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				TriggerState = 0;
			}
			
			if (CurrentBars[0] < 1)
				return;

			 // Set 2
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				TriggerState = 1;
				EnterLong(Convert.ToInt32(DefaultQuantity), @"entry");
			}
			
			 // Set 3
			if ((TriggerState == 1)
				 && (Position.MarketPosition == MarketPosition.Long))
			{
				TriggerState = 2;
				StopPrice = (Position.AveragePrice + (InitialStopDistance * TickSize)) ;
				TriggerPrice = (Position.AveragePrice + (BreakEvenTrigger * TickSize)) ;
			}
			
			 // Set 4
			if ((TriggerState == 2)
				 && (Close[0] >= TriggerPrice))
			{
				TriggerState = 3;
				StopPrice = Position.AveragePrice;
				Draw.Diamond(this, @"BreakEvenBuilderExample Diamond_1", true, 0, (High[0] + (2 * TickSize)) , Brushes.DarkCyan);
			}
			
			 // Set 5
			if (TriggerState >= 2)
			{
				ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), StopPrice, @"exit", @"entry");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BreakEvenTrigger", Description="Number of ticks above entry the breakeven movement trigger is set", Order=1, GroupName="Parameters")]
		public int BreakEvenTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-999, int.MaxValue)]
		[Display(Name="InitialStopDistance", Description="(use a negative) Number of ticks from entry the stop will initially be placed below", Order=2, GroupName="Parameters")]
		public int InitialStopDistance
		{ get; set; }
		#endregion

	}
}
