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
	public class TrailLongShortBuilderExample : Strategy
	{
		private double CurrentTriggerPrice;
		private double CurrentStopPrice;
		private bool FlipDirection;
		private int ProgressState;

		
		private Series<double> MathSeries;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Demonstrates changing the price of a Stop Market Order similar to a trailing stop loss";
				Name										= "TrailLongShortBuilderExample";
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
				BarsRequiredToTrade							= 1;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				TrailFrequency					= 2;
				TrailStopDistance					= 8;
				CurrentTriggerPrice					= 0;
				CurrentStopPrice					= 0;
				FlipDirection					= false;
				ProgressState					= 0;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				MathSeries = new Series<double>(this);			
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			 // Set 1
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (ProgressState > 1))
			{
				CurrentStopPrice = 0;
				ProgressState = 0;
			}
			
			if (CurrentBars[0] < 1)
				return;

			 // Set 2
			if ((ProgressState == 0)
				 && (State == State.Realtime)
				 && (Position.MarketPosition == MarketPosition.Flat)
				 && (FlipDirection == false))
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), @"longEntry");
				CurrentTriggerPrice = (Close[0] + (TrailFrequency * TickSize)) ;
				MathSeries[0] = TrailStopDistance;
				MathSeries[0] = (MathSeries[0] * -1) ;
				CurrentStopPrice = (Close[0] + (MathSeries[0] * TickSize)) ;
				FlipDirection = true;
				ProgressState = 1;
			}
			
			 // Set 3
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] > CurrentTriggerPrice)
				 && (ProgressState < 3))
			{
				CurrentTriggerPrice = (Close[0] + (TrailFrequency * TickSize)) ;
				MathSeries[0] = TrailStopDistance;
				MathSeries[0] = (MathSeries[0] * -1) ;
				CurrentStopPrice = (Close[0] + (MathSeries[0] * TickSize)) ;
			}
			
			 // Set 4
			if ((ProgressState == 3)
				 && (Position.MarketPosition == MarketPosition.Long))
			{
				ExitLong(Convert.ToInt32(DefaultQuantity), @"longMarketExit", @"longEntry");
			}
			
			 // Set 5
			if ((ProgressState < 3)
				 && (Position.MarketPosition == MarketPosition.Long)
				 && (CurrentStopPrice >= GetCurrentBid(0)))
			{
				ProgressState = 3;
			}
			
			 // Set 6
			if ((ProgressState < 3)
				 && (Position.MarketPosition == MarketPosition.Long))
			{
				ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), CurrentStopPrice, @"longStopExit", @"longEntry");
				ProgressState = 2;
			}
			
			 // Set 7
			if ((ProgressState == 0)
				 && (State == State.Realtime)
				 && (Position.MarketPosition == MarketPosition.Flat)
				 && (FlipDirection == true))
			{
				EnterShort(Convert.ToInt32(DefaultQuantity), @"shortEntry");
				MathSeries[0] = TrailFrequency;
				MathSeries[0] = (MathSeries[0] * -1) ;
				CurrentTriggerPrice = (Close[0] + (MathSeries[0] * TickSize)) ;
				CurrentStopPrice = (Close[0] + (TrailStopDistance * TickSize)) ;
				FlipDirection = false;
				ProgressState = 1;
			}
			
			 // Set 8
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] < CurrentTriggerPrice)
				 && (ProgressState < 3))
			{
				MathSeries[0] = TrailFrequency;
				MathSeries[0] = (MathSeries[0] * -1) ;
				CurrentTriggerPrice = (Close[0] + (MathSeries[0] * TickSize)) ;
				CurrentStopPrice = (Close[0] + (TrailStopDistance * TickSize)) ;
			}
			
			 // Set 9
			if ((ProgressState == 3)
				 && (Position.MarketPosition == MarketPosition.Short))
			{
				ExitShort(Convert.ToInt32(DefaultQuantity), @"shortMarketExit", @"shortEntry");
			}
			
			 // Set 10
			if ((ProgressState < 3)
				 && (Position.MarketPosition == MarketPosition.Short)
				 && (CurrentStopPrice <= GetCurrentAsk(0)))
			{
				ProgressState = 3;
			}
			
			 // Set 11
			if ((ProgressState < 3)
				 && (Position.MarketPosition == MarketPosition.Short))
			{
				ExitShortStopMarket(Convert.ToInt32(DefaultQuantity), CurrentStopPrice, @"shortStopExit", @"shortEntry");
				ProgressState = 2;
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailFrequency", Description="This will be how often trail action triggers.", Order=1, GroupName="Parameters")]
		public int TrailFrequency
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailStopDistance", Description="Distance stop for exit order will be placed. This needs to be less than the TrailLimitDistance to exit a long position.", Order=2, GroupName="Parameters")]
		public int TrailStopDistance
		{ get; set; }
		#endregion

	}
}
