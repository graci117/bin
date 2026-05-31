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
	public class RenkoReaper : Strategy
	{
		private ADXMA ADXMA1;

		
		private double BEStoredTargetPrice;
		private double BEStoredActualPrice;
		
		private double TrailStoredTargetPrice;
		private double TrailStoredActualPrice;
		
		private bool StopSetBool;
		private bool BreakEvenBool;
		private bool TrailStopBool;
		
		private double totalPnL;
		private double cumPnL;
		private double dailyPnL;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Printing Money using UniRenko";
				Name										= "RenkoReaper";
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
				ProfitTarget					= 60;
				Contracts						= 1;
				
				InitialStop						= 50;
				
				BETargetTicks					= 20;	// How many ticks until BE Set
				BEOffset						= 4;
				
				TrailTargetTicks				= 40;	// How many ticks until Trail Set
				TrailStopDistance				= 10;	// How far back your stop will trail
			
				//Set at false from default
				StopSetBool						= false;
				BreakEvenBool					= false;
				TrailStopBool					= false;
				
				ShowHistorical					= true;
				
				ADXPeriod					= 4;
				ADXThreshold					= 75;
				MAType						= MAtypeADX.HMA;
				
				Start						= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End							= DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
				
				//Daily Limits
				DailyProfitLimit							= 4500;
				DailyLossLimit								= 1500;
				
			}
			else if (State == State.Configure)
			{
//				SetTrailStop(@"", CalculationMode.Ticks, TrailTicks, false);
			}
			else if (State == State.DataLoaded)
			{				
				
				ADXMA1									= ADXMA(Convert.ToInt32(ADXPeriod),MAType);
				
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
			// at the start of a new session, reset the currentPnL for a new day of trading
			if (Bars.IsFirstBarOfSession){
				cumPnL 			= totalPnL; ///Double that copies the full session PnL (If trading multiple days). Is only calculated once per day.
				dailyPnL		= totalPnL - cumPnL; ///Subtract the copy of the full session by the full session PnL. This resets your daily PnL back to 0.
			}
			
			 // Set 1 - Set Order
			if (
				 // RegChanLongGroup1
					(Low[1] > Low[2])
				 && (Low[2] < Low[3])
				 && (ADXMA1[0] > ADXMA1[2])
				 && (ADXMA1[0] > ADXThreshold)
				
				 // Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				// Enter Time
				 && ((Times[0][0].TimeOfDay >= Start.TimeOfDay)
				 && (Times[0][0].TimeOfDay < End.TimeOfDay))
			 	 && (Position.MarketPosition == MarketPosition.Flat)
					)
				
			{
				// if flat and below the loss limit of the day enter long
				if (
					(dailyPnL > -DailyLossLimit) //Loss remains 'above' limit 
					&& (dailyPnL < DailyProfitLimit) //Profit remains 'below' limit
					)
				{
					EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"GoingUp");
					BreakEvenBool 	= false;
					TrailStopBool	= false;
					StopSetBool		= false;
				}
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
				(High[1] < High[2])
				 && (High[2] > High[3]
				 && (ADXMA1[0] > ADXMA1[2])
				 && (ADXMA1[0] > ADXThreshold))
				// Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				// Enter Time
				 && ((Times[0][0].TimeOfDay >= Start.TimeOfDay)
				 && (Times[0][0].TimeOfDay < End.TimeOfDay))
				
				&& (Position.MarketPosition == MarketPosition.Flat)
				)
			{
				
				// if flat and below the loss limit of the day enter short
				if (
					(dailyPnL > -DailyLossLimit) //Loss remains 'above' limit 
					&& (dailyPnL < DailyProfitLimit) //Profit remains 'below' limit
					)
				{
					EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(0), @"GoingDown");
					StopSetBool		= false;
					BreakEvenBool 	= false;
					TrailStopBool	= false;
				}
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
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			#region Daily PNL
			
			if (Position.MarketPosition == MarketPosition.Flat && SystemPerformance.AllTrades.Count > 0)
			{
				
//				totalPnL = 0; //backtest
			
				totalPnL = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit; ///Double that sets the total PnL 

				dailyPnL = (totalPnL) - (cumPnL); ///Your daily limit is the difference between these
				
				
				if (dailyPnL <= -DailyLossLimit) //Print this when daily Pnl is under Loss Limit
				{
					
					Print("Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + dailyPnL + " <<" +  Time[0]);
					
					Text myTextLoss = Draw.TextFixed(this, "loss_text", "Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + "$" + totalPnL + " <<", TextPosition.BottomLeft, Brushes.Black, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextLoss.Font = new SimpleFont("Arial", 15) {Bold = true };

				}
				
				
				if (dailyPnL >= DailyProfitLimit) //Print this when daily Pnl is above Profit limit
				{
					
					Print("Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" +  dailyPnL + " <<" + Time[0]);
					
					Text myTextProfit = Draw.TextFixed(this, "profit_text", "Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" + "$" +  totalPnL + " <<", TextPosition.BottomLeft, Brushes.Black, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextProfit.Font = new SimpleFont("Arial", 15) {Bold = true };
	
				}
			}	
			
			#endregion
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
		
		///ProfitLimit and LossLimit
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Profit Limit", Description="No positive or negative sign, just integer", Order=4, GroupName="01. Order Management")]
		public double DailyProfitLimit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Loss Limit", Description="No positive or negative sign, just integer", Order=5, GroupName="01. Order Management")]
		public double DailyLossLimit
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
		
		[NinjaScriptProperty]
		[Display(Name="MA Type", Order=2, GroupName="05. ADX Settings")]
		public MAtypeADX MAType
		{ get; set; }
		
		
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Trades", Order=1, GroupName="06. Time Frame")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Trades", Order=2, GroupName="06. Time Frame")]
		public DateTime End
		{ get; set; }

		#endregion

	}
}

