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
	public class TrailBuilderExample : Strategy
	{
		private double CurrentTriggerPrice;
		private double CurrentStopPrice;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Demonstrates changing the price of a Stop Market Order similar to a trailing stop loss";
				Name										= "TrailBuilderExample";
				Calculate									= Calculate.OnEachTick;
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
				TrailFrequency					= 5;
				TrailStopDistance					= -5;
				CurrentTriggerPrice					= 0;
				CurrentStopPrice					= 0;
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
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				CurrentStopPrice = 0;
			}
			
			if (CurrentBars[0] < 1)
				return;

			 // Set 2
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (State == State.Realtime))
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), @"longEntry");
				CurrentTriggerPrice = (Close[0] + (TrailFrequency * TickSize)) ;
				CurrentStopPrice = (Close[0] + (TrailStopDistance * TickSize)) ;
			}
			
			 // Set 3
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] > CurrentTriggerPrice))
			{
				CurrentTriggerPrice = (Close[0] + (TrailFrequency * TickSize)) ;
				CurrentStopPrice = (Close[0] + (TrailStopDistance * TickSize)) ;
			}
			
			 // Set 4
			if (CurrentStopPrice != 0)
			{
				ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), CurrentStopPrice, @"", "");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailFrequency", Description="This will be how often trail action triggers.", Order=1, GroupName="Parameters")]
		public int TrailFrequency
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-9999, int.MaxValue)]
		[Display(Name="TrailStopDistance", Description="Distance stop for exit order will be placed. This needs to be less than the TrailLimitDistance to exit a long position.", Order=2, GroupName="Parameters")]
		public int TrailStopDistance
		{ get; set; }
		#endregion

	}
}
