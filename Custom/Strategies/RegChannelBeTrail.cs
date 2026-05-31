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
	public class RegChannelBeTrail : Strategy
	{
		private RegressionChannel2 RegressionChannel21;
		private RegressionChannelExtended RegressionChannelExtended1;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		private ADX ADX1;
		
		private double BEStoredTargetPrice;
		private double BEStoredActualPrice;
		
		private double TrailStoredTargetPrice;
		private double TrailStoredActualPrice;
		
		private bool StopSetBool;
		private bool BreakEvenBool;
		private bool TrailStopBool;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "RegChannelBeTrail";
				Calculate									= Calculate.OnPriceChange;
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
				ProfitTarget					= 40;
				Contracts						= 1;
				
				InitialStop						= 61;
				
				BETargetTicks					= 27;	// How many ticks until BE Set
				BEOffset						= 4;
				
				TrailTargetTicks				= 30;	// How many ticks until Trail Set
				TrailStopDistance				= 10;	// How far back your stop will trail
			
				//Set at false from default
				StopSetBool						= false;
				BreakEvenBool					= false;
				TrailStopBool					= false;
				
				ShowHistorical					= true;
				
				ADXPeriod						= 4;
				ADXThreshold					= 50;
				
			}
			else if (State == State.Configure)
			{
//				SetTrailStop(@"", CalculationMode.Ticks, TrailTicks, false);
			}
			else if (State == State.DataLoaded)
			{				
				RegressionChannel21						= RegressionChannel2(Close, 40, 3.5);
				RegressionChannelExtended1				= RegressionChannelExtended(Close, 40, 3.5);
				RegressionChannelHighLow1				= RegressionChannelHighLow(Close, 40, 3.5);
				ADX1									= ADX(Close, Convert.ToInt32(ADXPeriod));
//				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
//				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 5)
				return;
			
			if (!ShowHistorical)
			{
				if (State != State.Realtime)
					return;
			}
			
			 // Set 1 - Set Order
			if (
				 // RegChanLongGroup1
				(((RegressionChannel21.Middle[1] > RegressionChannel21.Middle[2])
				 && (RegressionChannel21.Middle[2] <= RegressionChannel21.Middle[3])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanLongGroup2
				 || ((RegressionChannel21.Middle[0] > RegressionChannel21.Middle[1])
				 && (Low[0] > Low[2])
				 && (Low[2] <= RegressionChannel21.Lower[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanLongGroup2
				 || (ADX1[0] > ADXThreshold)
				&& ((Low[0] > RegressionChannelHighLow1.Lower[2])
				 && (ADX1[0] > ADX1[2])
				&& (ADX1[0] > ADXThreshold))
				)
				// Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				
				&& (Position.MarketPosition == MarketPosition.Flat)
)
				
			{
				EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"GoingUp");
				BreakEvenBool 	= false;
				TrailStopBool	= false;
				StopSetBool		= false;
			}
			
			// Set 2 - Set Stop and BE/Trail Targets
			if ((Position.MarketPosition == MarketPosition.Long)
				 && !StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (InitialStop * TickSize)) , @"MoneyDone", @"GoingUp");
				ExitLongLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (ProfitTarget * TickSize)) , @"MoneyWon", @"GoingUp");
				
				BEStoredTargetPrice = (Position.AveragePrice + (BETargetTicks * TickSize)); //Store how far price needs to move before BE is set
				BEStoredActualPrice = (Position.AveragePrice + (BEOffset * TickSize)); //Store the actual BE Value for later use
				
				
				TrailStoredTargetPrice = (Position.AveragePrice + (TrailTargetTicks * TickSize)); // Store How far price needs to move before Trail Stop is set
				TrailStoredActualPrice = Close[0]; // Store a value for Trail -> Needs a check first but will be adjusted later on when its set
				
				StopSetBool = true;
			}
			
			 // Set 3 - Set Breakeven
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] >= BEStoredTargetPrice)
					&& GetCurrentAsk(0) > BEStoredActualPrice && GetCurrentBid(0) > BEStoredActualPrice
					&& StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"MoneyDone", @"GoingUp");
				BreakEvenBool = true;
			}
			
			 // Set 4 - Set Trail Stop
			if ((Position.MarketPosition == MarketPosition.Long)
				 && Close[0] >= TrailStoredTargetPrice
					&& StopSetBool && BreakEvenBool && !TrailStopBool
					&& Close[0] - (TrailStopDistance * TickSize) > BEStoredActualPrice
					&& Close[0] - (TrailStopDistance * TickSize) > TrailStoredActualPrice)
			{
				TrailStoredActualPrice = Close[0] - (TrailStopDistance * TickSize); //Update Trail Price before submitting order
				
				if (GetCurrentAsk(0) > TrailStoredActualPrice && GetCurrentBid(0) > TrailStoredActualPrice)
					ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"MoneyDone", @"GoingUp");
			}
			
			//======Short Sets======\\
			
			 // Set 6 - Set Order
			if (
				 // RegChanShortGroup1
				(((RegressionChannelExtended1.Middle[1] < RegressionChannelExtended1.Middle[2])
				 && (RegressionChannelExtended1.Middle[2] >= RegressionChannelExtended1.Middle[3])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanShortGroup2
				 || ((RegressionChannelExtended1.Middle[0] > RegressionChannelExtended1.Middle[1])
				 && (High[0] < High[2])
				 && (High[2] >= RegressionChannelExtended1.Lower[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanShortGroup3
				 || ((High[0] < RegressionChannelHighLow1.Upper[2])
				 && (ADX1[0] > ADX1[5])
				 && (ADX1[0] > ADXThreshold)))
				// Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				
				&& (Position.MarketPosition == MarketPosition.Flat)
				)
			{
				EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(0), @"GoingDown");
				StopSetBool		= false;
				BreakEvenBool 	= false;
				TrailStopBool	= false;
			}
			
			
			// Set 2 - Set Stop and BE/Trail Targets
			if ((Position.MarketPosition == MarketPosition.Short)
				 && !StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (InitialStop * TickSize)) , @"MoneyDone", @"GoingDown");
				ExitShortLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (ProfitTarget * TickSize)) , @"MoneyWon", @"GoingDown");
				
				BEStoredTargetPrice = (Position.AveragePrice - (BETargetTicks * TickSize)); //Store how far price needs to move before BE is set
				BEStoredActualPrice = (Position.AveragePrice - (BEOffset * TickSize)); //Store the actual BE Value for later use
				
				
				TrailStoredTargetPrice = (Position.AveragePrice - (TrailTargetTicks * TickSize)); // Store How far price needs to move before Trail Stop is set
				TrailStoredActualPrice = Close[0]; // Store a value for Trail -> Needs a check first but will be adjusted later on when its set
				
				StopSetBool = true;
			}
			
			// Set 3 - Set Breakeven
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] <= BEStoredTargetPrice)
					&& GetCurrentAsk(0) < BEStoredActualPrice && GetCurrentBid(0) < BEStoredActualPrice
					&& StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"MoneyDone", @"GoingDown");
				BreakEvenBool = true;
			}
			
			// Set 4 - Set Trail Stop
			if ((Position.MarketPosition == MarketPosition.Short)
				 && Close[0] <= TrailStoredTargetPrice
					&& StopSetBool && BreakEvenBool && !TrailStopBool
					&& Close[0] + (TrailStopDistance * TickSize) < BEStoredActualPrice
					&& Close[0] + (TrailStopDistance * TickSize) < TrailStoredActualPrice)
			{
				TrailStoredActualPrice = Close[0] + (TrailStopDistance * TickSize); //Update Trail Price before submitting order
				
				if (GetCurrentAsk(0) < TrailStoredActualPrice && GetCurrentBid(0) < TrailStoredActualPrice)
					ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"MoneyDone", @"GoingDown");
			}
			
				
			// Reset when Flat
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				StopSetBool		= false;
				BreakEvenBool 	= false;
				TrailStopBool	= false;
			}

		}

		#region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Order=1, GroupName="01. Order Management")]
		public int Contracts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Initial Stop Ticks", Order=2, GroupName="01. Order Management")]
		public int InitialStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ProfitTarget", Order=3, GroupName="01. Order Management")]
		public int ProfitTarget
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Breakeven Target", Order=2, GroupName="02. BreakEven")]
		public int BETargetTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Breakeven Tick Offset", Order=3, GroupName="02. BreakEven")]
		public int BEOffset
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailStop Target", Order=2, GroupName="03. Trail Stop")]
		public int TrailTargetTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trail Stop Distance", Order=3, GroupName="03. Trail Stop")]
		public int TrailStopDistance
		{ get; set; }
		
		
		
		[NinjaScriptProperty]
		[Display(Name="Show Historical Trades", Order=3, GroupName="04. Additional Settings")]
		public bool ShowHistorical
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ADXPeriod", Order=1, GroupName="05. ADX Settings")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXThreshold", Order=2, GroupName="05. ADX Settings")]
		public int ADXThreshold
		{ get; set; }

		#endregion

	}
}

