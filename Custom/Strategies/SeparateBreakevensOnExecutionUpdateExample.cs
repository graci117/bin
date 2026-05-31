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
	public class SeparateBreakevensOnExecutionUpdateExample : Strategy
	{
		private bool doOnce;
		private bool pos1Breakeven, pos2Breakeven;
		private double entry1price, entry2price;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "SeparateBreakevensOnExecutionUpdateExample";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 2;
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
			}
			else if (State == State.DataLoaded)
			{
				// Intialize the doOnce bool to true
				doOnce = true;
				// Initialize the Breakeven bool to false
				pos1Breakeven = pos2Breakeven = false;
			}
		}

		protected override void OnExecutionUpdate(Cbi.Execution execution, string executionId, double price, int quantity, 
			Cbi.MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (execution.Name == "Entry1")
			{
				ExitLongStopMarket(0, true, execution.Order.Filled, execution.Order.AverageFillPrice - 10 * TickSize, "StopForEntry1", "Entry1");
				ExitLongLimit(0, true, execution.Order.Filled, execution.Order.AverageFillPrice + 20 * TickSize, "TargetForEntry1", "Entry1");
				
				entry1price = execution.Order.AverageFillPrice;
			}
			else if (execution.Name == "Entry2")
			{
				ExitLongStopMarket(0, true, execution.Order.Filled, execution.Order.AverageFillPrice - 10 * TickSize, "StopForEntry2", "Entry2");
				ExitLongLimit(0, true, execution.Order.Filled, execution.Order.AverageFillPrice + 30 * TickSize, "TargetForEntry2", "Entry2");
				
				entry2price = execution.Order.AverageFillPrice;
			}
		}

		protected override void OnBarUpdate()
		{
			if (State == State.Historical)
				return;
			if (Position.MarketPosition == MarketPosition.Flat && doOnce)
			{
				EnterLongLimit(0, true, 1, Low[0] - 10 * TickSize, "Entry1");
				EnterLongLimit(0, true, 1, Low[0] - 20 * TickSize, "Entry2");
				
				// Make sure the entry logic is processed just once, so the entry orders do not move with new bar updates
				doOnce = false;
				
				// Make sure our Breakeven bools are set
				pos1Breakeven = pos2Breakeven = false;
			}
			
			if (Position.MarketPosition == MarketPosition.Long)
			{
				// Move entry1's stop to Breakeven
				if (Close[0] > entry1price + 5 * TickSize && !pos1Breakeven)
				{
					ExitLongStopMarket(0, true, 1, entry1price, "StopForEntry1", "Entry1");
					
					// Set our bool to note we are at the BE point for this entry
					pos1Breakeven = true;
				}
				
				// Move entry2's stop to Breakeven
				if (Close[0] > entry2price + 10 * TickSize && !pos2Breakeven)
				{
					ExitLongStopMarket(0, true, 1, entry2price, "StopForEntry2", "Entry2");
					
					// Set our bool to note we are at the BE point for this entry
					pos2Breakeven = true;
				}
				
				// Reset the bool so we can submit entry orderds once we go back to flat
				doOnce = true;
			}
			
		}
	}
}
