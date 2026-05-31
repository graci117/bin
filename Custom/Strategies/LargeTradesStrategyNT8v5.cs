//            \\|//             +-+-+-+-+-+-+-+-+-+-+-+             \\|//            //
//           ( o o )            |L|a|r|g|e|T|r|a|d|e|s|            ( o o )           //
//    ~~~~oOOo~(_)~oOOo~~~~     +-+-+-+-+-+-+-+-+-+-+-+     ~~~~oOOo~(_)~oOOo~~~~    //

    /// <summary>
	/// Coded by bcomas July 2020. Email:  bcomasSoftware@gmail.com for improvements
    /// http://ninjatrader-programming-strategies.mozello.com/
    /// Trading the major transactions that have taken place during
    /// short period of time and at the same price (Limit buyer / seller)
    /// ***Updated September 2020
    /// </summary>

// Using declarations
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

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class LargeTradesStrategyNT8v5 : Strategy
	{
	    /// OrderFlowCumulativeDelta indicator
//		private OrderFlowCumulativeDelta OrderFlowCumulativeDelta1; 
//	    private OrderFlowCumulativeDelta OrderFlowCumulativeDelta2;
		
		/// LargeTrades indicator
		private bcomasLargeTradesV3 LargeTradesSignal1;
		private int vol                     = 150; 
		private int fix                     = 1;
		
		/// PriorDayOHLCsignal indicator
        private PriorDayOHLCBreakout PriorDayOHLCsignal1;
		
		// Daily Target 
		private int priorTradesCount        = 0;
		private double priorTradesCumProfit = 0;
		
		// Boosting 
		private bool isAtmStrategyCreated2 = false;
		private string atmStrategyId2      = string.Empty;
		private string orderId2            = string.Empty;
		
		// Email
		private string toEmailAddress       = @""; 
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{                                                 // 1 min heiken-ashi timeframe
				Description									= @"NT8 Strategy based on LargeTrades";
				Name										= "LargeTradesStrategyNT8v5";
				Calculate									= Calculate.OnBarClose;////
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= false;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.ImmediatelySubmit;
				TimeInForce									= TimeInForce.Day;
				TraceOrders									= false;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 100;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
			    // Keeps the strategy running as if no disconnect occurred	
                ConnectionLossHandling                      = ConnectionLossHandling.KeepRunning;
				
			    /// Orderflow
				UseOrderflow		                        = true;
				Delta                                       =  50;
				
				ATMName										= "EnterYourATMnameHere"; // 2 contracts ; SL 50 TP 100 BE 50 plus 0 Trailing 20 50 1
				BoostIfProfit2                              =  10000; // Boost if the account profit reach +(X) $
				
				// StopLoss & ProfitTarget
				UseProtectiveStops		                    = true;
				StopLoss				                    = 80;
				ProfitTarget			                    = 100;
				BreakEvenLong				                = 90;
				BreakEvenShort				                = 90;
				
				// Daily Target
				Contracts                                   =    2;
				DailyLossLimit                              = -200;
				DailyProfitLimit                            =  200;
				DailyTradesCount                            =  200;
				
				//Stop trading if the Account Cash Value Target is reached ($)
				UseOnlyOneStrategy                          = true;
                AccountCashValueTarget                      = 550000;
				AccountCashValueDrawDown                    = 2200;

				// Email alerts
				UseEmail	                                = false;
				EnableEntryAlert			                = false;
				EnableExitAlert				                = false;
				toEmailAddress                              = @"";
				
				//Days of the week to exclude
				ExcludeMonday                               = false;
				ExcludeTuesday                              = false;
				ExcludeWednesday                            = false;
				ExcludeThursday                             = false;
				ExcludeFriday                               = false;
				
				// Times
				Session1 = true;
				Session1Start =    0;  
				Session1End = 224500;  

				Session2 = false;
				Session2Start = 90000;
				Session2End = 110000;
				
				Session3 = false;
				Session3Start = 120000;
				Session3End = 125900;
				
			}
			else if (State == State.Configure)
			{ 
				{ 
                    RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
                }
				
				{ 
				if( UseProtectiveStops )
				SetProfitTarget(@"LongA", CalculationMode.Ticks, ProfitTarget);
				if( UseProtectiveStops )
				SetStopLoss(@"LongA", CalculationMode.Ticks, StopLoss, false);
                }

				{
					/// LargeTrades indicator
			        AddDataSeries(BarsPeriodType.Second,1);
					
					/// OrderFlowCumulativeDelta indicator
					//AddDataSeries(Data.BarsPeriodType.Tick, 1);
				}
			}
			else if (State == State.DataLoaded)
			{
				{
				if( UseProtectiveStops )
				SetProfitTarget(@"ShortA", CalculationMode.Ticks, ProfitTarget);
				if( UseProtectiveStops )
				SetStopLoss(@"ShortA", CalculationMode.Ticks, StopLoss, false);
			    }
				
                /// LargeTrades indicator
			    LargeTradesSignal1 = bcomasLargeTradesV3(Close, Volume_, fix, Brushes.Transparent, Brushes.White, false, false, false, "Alert4.wav");
				                         
			    {
			      // AddChartIndicator(LargeTradesSignal1);
			    }
				
//				/// OrderFlowCumulativeDelta indicator
//				OrderFlowCumulativeDelta1   = OrderFlowCumulativeDelta(CumulativeDeltaType.BidAsk, CumulativeDeltaPeriod.Bar, 0);// bar				
//				OrderFlowCumulativeDelta2   = OrderFlowCumulativeDelta(CumulativeDeltaType.BidAsk, CumulativeDeltaPeriod.Session, 0);// session
				
//                //AddChartIndicator(OrderFlowCumulativeDelta1);
//				AddChartIndicator(OrderFlowCumulativeDelta2);
				
				/// PriorDayOHLCsignal indicator
                PriorDayOHLCsignal1 = PriorDayOHLCBreakout(Close);

                AddChartIndicator(PriorDayOHLCsignal1);
				
			}
		}
		
		// Email Alerts
		
	    protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, int quantity, Cbi.MarketPosition marketPosition)
		{
			#region Alert for exits
			//Alert for exits
			if (Position.MarketPosition == MarketPosition.Flat && EnableExitAlert == true && SystemPerformance.RealTimeTrades.Count > 0) 
			{
		      // Check to make sure there is at least one trade in the collection
		      Trade lastTrade = SystemPerformance.RealTimeTrades[SystemPerformance.RealTimeTrades.Count - 1];
		 
		      // Calculate the PnL for the last completed real-time trade
		      double lastProfitCurrency = lastTrade.ProfitCurrency;
				
			  SendMail(toEmailAddress,"Position LargeTrades closed" + " " + Instrument.MasterInstrument.Description.ToString() + " " + this.Name,
				  "Date & Time: " + Times[0][0].ToString("dd/MM/yyyy") + " " + Times[0][0].ToString("HH:mm:ss") + "\n" +
				  "Instrument: " + Instrument.MasterInstrument.Description.ToString() + " " + Instrument.FullName.ToString() + "\n" +
				  "Account: " + Position.Account.Name.ToString() + "\n" +
				  "Exit name: " + lastTrade.Exit.Name.ToString() + "\n" +
				  "Exit price: " + lastTrade.Exit.Price.ToString() + "\n" + 
				  "Profit/Loss last trade: " + lastProfitCurrency.ToString("0.00"));
			}
			// Draw text TopRight - Last Profit / Loss
			if (Position.MarketPosition == MarketPosition.Flat && SystemPerformance.RealTimeTrades.Count > 0) 
			{
				Trade lastTrade = SystemPerformance.RealTimeTrades[SystemPerformance.RealTimeTrades.Count - 1];
		        double lastProfitCurrency = lastTrade.ProfitCurrency;
				Draw.TextFixed(this, "limitText", "Profit / Loss \nPnL: "+lastProfitCurrency.ToString("0.00")+"$", TextPosition.TopRight);
			}
			else
			    {
				RemoveDrawObject("limitText");
			    }
				
			#endregion
			
			#region Alert for entries
			if (EnableEntryAlert == true)
			{
				//Alert long entries
				if (Position.MarketPosition == MarketPosition.Long)
				{
					SendMail(toEmailAddress,"Long LargeTrades Entry: " + this.Name + " " + Instrument.MasterInstrument.Description.ToString(),
						"Date & Time: " + Times[0][0].ToString("dd/MM/yyyy") + " " + Times[0][0].ToString("HH:mm:ss") + "\n" +
						"Instrument: " + Instrument.MasterInstrument.Description.ToString() + " " + Instrument.FullName.ToString() + "\n" +
					 	"Account: " + Position.Account.Name.ToString() + "\n" +
						"Long Entry" + "\n" +
						"Quantity: " + Position.Quantity.ToString() + "\n" +
						"EntryPrice: " + Position.AveragePrice.ToString() + "\n");
				}
				
				//Alert short entries
				if (Position.MarketPosition == MarketPosition.Short)
				{
					SendMail(toEmailAddress,"Short LargeTrades Entry: " + this.Name + " " + Instrument.MasterInstrument.Description.ToString(),
						"Date & Time: " + Times[0][0].ToString("dd/MM/yyyy") + " " + Times[0][0].ToString("HH:mm:ss") + "\n" +
						"Instrument: " + Instrument.MasterInstrument.Description.ToString() + " " + Instrument.FullName.ToString() + "\n" +
					 	"Account: " + Position.Account.Name.ToString() + "\n" +
						"Short Entry" + "\n" +
						"Quantity: " + Position.Quantity.ToString() + "\n" +
						"EntryPrice: " + Position.AveragePrice.ToString() + "\n");
				}
			}
			#endregion
		}
		
	    // Boosting
	    private bool InPosition2 { get { return atmStrategyId2.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId2) != MarketPosition.Flat; } }
		
		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0 || CurrentBars[0] < 1 || CurrentBar < BarsRequiredToTrade) 
				// Halt further processing of our strategy 
				return;
			
			// Daily Target
			if (Bars.IsFirstBarOfSession)
				
			{
				// Store the strategy's prior cumulated realized profit and number of trades
				priorTradesCount = SystemPerformance.RealTimeTrades.Count;
				priorTradesCumProfit = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;
				/* NOTE: Using . " AllTrades " will include both historical virtual trades as well as real-time trades.
				For only count profits from real-time trades use . " RealTimeTrades "  */
			}
			if (SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit - priorTradesCumProfit >= DailyProfitLimit
				|| SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit - priorTradesCumProfit <= DailyLossLimit 
				|| SystemPerformance.RealTimeTrades.Count - priorTradesCount > DailyTradesCount)
			{
				Draw.TextFixed(this, "limitText", "Daily Loss / Profit reached"
				, TextPosition.BottomRight);
				
				// Halt further processing of our strategy 
				return;
			}	
			else
			    {
				RemoveDrawObject("limitText");
		    }
				
			/// BreakEven
		    // Resets the stop loss to the original value when all positions are closed
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				SetStopLoss(@"LongA", CalculationMode.Ticks, StopLoss, false);
				SetStopLoss(@"ShortA", CalculationMode.Ticks, StopLoss, false);
			}
			
			// If a long position is open, allow for stop loss modification to breakeven
			else if (Position.MarketPosition == MarketPosition.Long)
			{
				// Once the price is greater than entry price+BreakEvenLong ticks, set stop loss to breakeven
				if (Close[0] > Position.AveragePrice + BreakEvenLong * TickSize)
				{
				//Print("Move LongA to BE");
		        SetStopLoss(@"LongA",CalculationMode.Price, Position.AveragePrice,false);
				}
			}
			// If a Short position is open, allow for stop loss modification to breakeven
			else if (Position.MarketPosition == MarketPosition.Short)
			{
				// Once the price is greater than entry price-BreakEvenShort ticks, set stop loss to breakeven
				if (Close[0] < Position.AveragePrice - BreakEvenShort * TickSize)
				{
				//Print("Move ShortA to BE");
		        SetStopLoss(@"ShortA",CalculationMode.Price, Position.AveragePrice,false);
				}
			}			
            //////////////////////////////////////////////////////////////////////////////
		
			/// Evaluates to see if the account has more than the Account Cash Value Target ($)
            if (Account.Get(AccountItem.CashValue, Currency.UsDollar) > AccountCashValueTarget)
            {
            Draw.TextFixed(this, "limitText", "                                                              Account Cash Value Target reached - Withdraw funds", TextPosition.BottomLeft);
			Print("----------Time is: " +Time[0].ToString("dd/MM/yyyy") + "," + Time[0].ToString("HH:mm:ss"));
			Account.FlattenEverything();
            /// Halt further processing of our strategy - disabling

            }
			
			/// Max Monthly DrawDown permited
            if (Account.Get(AccountItem.CashValue, Currency.UsDollar) < AccountCashValueDrawDown)
            {
            Draw.TextFixed(this, "limitText", "                                                              Max Monthly DrawDown reached - STOP TRADING , range-bound days", TextPosition.BottomLeft);
			Print("----------Time is: " +Time[0].ToString("dd/MM/yyyy") + "," + Time[0].ToString("HH:mm:ss"));
			Account.FlattenEverything();
            /// Halt further processing of our strategy - disabling
            }
			
			///Days of the week strategy 
			if      ((ExcludeMonday == true) && (Time[0].DayOfWeek == DayOfWeek.Monday))
			return;
			// Halt further processing of our strategy
			else if ((ExcludeTuesday == true) && (Time[0].DayOfWeek == DayOfWeek.Tuesday))
			return;
			// Halt further processing of our strategy
			else if ((ExcludeWednesday == true) && (Time[0].DayOfWeek == DayOfWeek.Wednesday))
			return;
			// Halt further processing of our strategy
			else if ((ExcludeThursday == true) && (Time[0].DayOfWeek == DayOfWeek.Thursday))
			return;
			// Halt further processing of our strategy
			else if ((ExcludeFriday == true) && (Time[0].DayOfWeek == DayOfWeek.Friday))
			return;
			// Halt further processing of our strategy
				
			//Times  (Exit)
			/*if( !canTradeTime() ) {
				// Halt further processing of our strategy 
				return;
			}*/
				
			///Times  (Exit)
		    if (!canTradeTime()) 
			//if (OrderFlowCumulativeDelta1.DeltaClose[0] > 1) 
			if (Position.MarketPosition == MarketPosition.Short)
			{
				ExitShort(Convert.ToInt32(Contracts));					
			}
			if (!canTradeTime()) 
			//if (OrderFlowCumulativeDelta1.DeltaClose[0] < -1) 
			if (Position.MarketPosition == MarketPosition.Long)
			{
				ExitLong(Convert.ToInt32(Contracts));
			}
			
			if (UseOnlyOneStrategy == true && Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) > 0) 
				/// Halt further processing of our strategy
				return;
			
			/// Boosting
			
			if ((CurrentBar > 0)
			&& Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) >= BoostIfProfit2 
			&& canTradeTime())
			LookForTrade2();
            else if (!canTradeTime()) //Times  (Exit)	
			FlattenPosition2();
			
			///bullish	
		    // Condition set 1
			if (UseOrderflow == false && (canTradeTime()) 
			&& LargeTradesSignal1.Direction == 1)
				
			EnterLong(Convert.ToInt32(Contracts), @"LongA");
			
			///bearish
			// Condition set 2
			if (UseOrderflow == false && (canTradeTime())
			&& LargeTradesSignal1.Direction == -1)  
				
			EnterShort(Convert.ToInt32(Contracts), @"ShortA");
			
            ///bullish - UseOrderflow
            // Condition set 3
            if( UseOrderflow == true && canTradeTime() && LargeTradesSignal1.Direction == 1
			//&& OrderFlowCumulativeDelta1.DeltaClose[0] > Delta  // bar
            //&& OrderFlowCumulativeDelta2.DeltaClose[0] > Delta
				) // session

            EnterLong(Convert.ToInt32(Contracts), @"LongA");
 
            ///bearish - UseOrderflow
            // Condition set 4
            if( UseOrderflow == true && canTradeTime() && LargeTradesSignal1.Direction == -1
			//&& OrderFlowCumulativeDelta1.DeltaClose[0] < - Delta  // bar
           //&& OrderFlowCumulativeDelta2.DeltaClose[0] < - Delta
				) // session
    
            EnterShort(Convert.ToInt32(Contracts), @"ShortA");
			
	    }
		/////////////////////////////////////////////////////////////////////////////// // Boosting	
			private void LookForTrade2()
	        {
				
				/// Make sure this strategy does not execute against historical data
			    if(State == State.Historical)
				/// Halt further processing of our strategy 
				return;
				
            /// bullish boost
//		    if (UseOrderflow == true && canTradeTime() && OrderFlowCumulativeDelta1.DeltaClose[0] > 1) // bar
  
//			{
//				if (orderId2.Length == 0 && atmStrategyId2.Length == 0)
//			{
//				isAtmStrategyCreated2 = false;  // reset atm strategy created check to false
//				atmStrategyId2 = GetAtmStrategyUniqueId();
//				Print("----atmStrategyID is : " +atmStrategyId2.ToString());
//				orderId2 = GetAtmStrategyUniqueId();         
//				AtmStrategyCreate(OrderAction.Buy, OrderType.Market, Low[0], 0, TimeInForce.Day, orderId2, ATMName, atmStrategyId2, (atmCallbackErrorCode, atmCallBackId) => {
//					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
//					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId2)
//						isAtmStrategyCreated2 = true;
//				});
//			}
			
//			}
			
		   /// bearish boost
//           if (UseOrderflow == true && canTradeTime() && OrderFlowCumulativeDelta1.DeltaClose[0] < -1) // bar
 
//			{
//			if (orderId2.Length == 0 && atmStrategyId2.Length == 0)
//			{
//				isAtmStrategyCreated2 = false;  // reset atm strategy created check to false
//				atmStrategyId2 = GetAtmStrategyUniqueId();
//				Print("----atmStrategyID is : " +atmStrategyId2.ToString());
//				orderId2 = GetAtmStrategyUniqueId();
//				AtmStrategyCreate(OrderAction.Sell, OrderType.Market, High[0], 0, TimeInForce.Day, orderId2, ATMName, atmStrategyId2, (atmCallbackErrorCode, atmCallBackId) => {
//					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
//					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId2)
//						isAtmStrategyCreated2 = true;
//				});
//			}
			
//			}
			// Check That atm strategy was created before checking other properties
			if (!isAtmStrategyCreated2)
				return;

			// Check for a pending entry order
			if (orderId2.Length > 0)
			{
				string[] status = GetAtmStrategyEntryOrderStatus(orderId2);

				// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
				if (status.GetLength(0) > 0)
				{
					// Print out some information about the order to the output window
					Print("The entry order average fill price is: " + status[0]);
					Print("The entry order filled amount is: " + status[1]);
					Print("The entry order order state is: " + status[2]);
					

					// If the order state is terminal, reset the order id value
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderId2 = string.Empty;
				}
			} // If the strategy has terminated reset the strategy id
			else if (atmStrategyId2.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId2) == Cbi.MarketPosition.Flat)
				atmStrategyId2 = string.Empty;

			if (atmStrategyId2.Length > 0)
			{

			}
		}
		   
        // Flatten function
		private void FlattenPosition2()
		{
			if (InPosition2)
			{				
                foreach (Order order in Account.Orders) //First foreach loop, to get working orders
                {					
					if(order.OrderState == OrderState.Working || order.OrderState == OrderState.Accepted)  //This will print all working orders on the account
                     {             
						 
						if (order.Name == "Entry")  //Checks if the order name is Stop1, and if so, assigns it to an order object.
                        { 
							
						var orders = Account.Orders.Where( x => x.Instrument == Instrument && !Order.IsTerminalState(x.OrderState));
					    if(orders.Count()!= 0)
                        {
                        Print( Name +"********"+ Instrument.FullName +", "+ BarsArray[0].BarsPeriod +": active orders detected to cancel: " + orders.Count().ToString());
				        }
	
                            Account.Cancel(new[] { order });   // Cancel the Buy Stop / Sell Stop 
                         
                        }    
                    }					
                }

                Print("----Closing ATM for : " + atmStrategyId2.ToString());

                AtmStrategyClose(atmStrategyId2); 
				
				atmStrategyId2 = string.Empty;;
      
            }   
		}/////////////////////////////////////////////////////////////////////////////// End Boosting
		
		private bool canTradeTime()
		{
			int currentTime = ToTime( Time[0] );
			bool doTrade = false;
				
			if( (Session1
				&& (currentTime >= Session1Start && currentTime <= Session1End)) )
			{
				doTrade = true;
			}
			else if( (Session2
				&& (currentTime >= Session2Start && currentTime <= Session2End)) )
			{
				doTrade = true;
			}
			else if( (Session3
					&& (currentTime >= Session3Start && currentTime <= Session3End)) )	
			{
				doTrade = true;
			}

			return doTrade;
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ATM Name Boost", Description="Name of ATM strategy to attach", Order=0, GroupName="Boost")]
		public string ATMName
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Boost If Profit", Description = "Boost If Profit", Order = 1, GroupName = "Boost")]
		public double BoostIfProfit2
		{ get; set; }
	
		[Display(ResourceType = typeof(Custom.Resource), Name = "Use protective stops?", Order = 0, GroupName = "Stops & Targets")]
		public bool UseProtectiveStops
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Stop loss", Description="Stop loss (ticks)", Order=1, GroupName="Stops & Targets")]
		public int StopLoss
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Profit target", Description="Profit target (in ticks)", Order=2, GroupName="Stops & Targets")]
		public int ProfitTarget
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="BreakEven Long", Description="BreakEven Long", Order=3, GroupName="Stops & Targets")]
		public int BreakEvenLong
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="BreakEven Short", Description="BreakEven Short", Order=4, GroupName="Stops & Targets")]
		public int BreakEvenShort
		{ get; set; }
	
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Contracts", Description="The number of contracts to trade", Order=1, GroupName="Daily Target")]
		public int Contracts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "DailyLossLimit", Description = "Daily Loss Limit", Order = 2, GroupName = "Daily Target")]
		public double DailyLossLimit
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "DailyProfitLimit", Description = "Daily Profit Limit", Order = 3, GroupName = "Daily Target")]
		public double DailyProfitLimit
		{ get; set; }
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="DailyTradesCount", Description="The number of daily TradesCount", Order=4, GroupName="Daily Target")]
		public int DailyTradesCount
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Use Session 1 times?", Description="True to use Session 1 start/end times)", Order=1, GroupName="Times")]		
		public bool Session1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, 235959)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Session 1 start time", Description="Session 1 start time", Order=2, GroupName="Times")]		
		public int Session1Start
		{ get; set; }        
		
		[NinjaScriptProperty]
		[Range(0, 235959)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Session 1 end time", Description="Session 1 end time", Order=3, GroupName="Times")]		
		public int Session1End
		{ get; set; }        

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Use Session 2 times?", Description="True to use Session 3 start/end times)", Order=4, GroupName="Times")]		
		public bool Session2
		{ get; set; }        

		[NinjaScriptProperty]
		[Range(0, 235959)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Session 2 start time", Description="Session 2 start time", Order=5, GroupName="Times")]		
		public int Session2Start
		{ get; set; }        

		[NinjaScriptProperty]
		[Range(0, 235959)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Session 2 end time", Description="Session 1 end time", Order=6, GroupName="Times")]		
		public int Session2End
		{ get; set; }        

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Use Session 3 times?", Description="True to use Session 3 start/end times)", Order=7, GroupName="Times")]		
		public bool Session3
		{ get; set; }        

		[NinjaScriptProperty]
		[Range(0, 235959)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Session 3 start time", Description="Session 3 end time", Order=8, GroupName="Times")]		
		public int Session3Start
		{ get; set; }        

		[NinjaScriptProperty]
		[Range(0, 235959)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Session 3 end time", Description="Session 3 end time", Order=9, GroupName="Times")]		
		public int Session3End
		{ get; set; }   
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="UseEmail", Description="Use Email", Order=1, GroupName="Mail Share Service")]
		public bool UseEmail
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="EnableEntryAlert", Description="Enable email alert for Entries", Order=2, GroupName="Mail Share Service")]
		public bool EnableEntryAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="EnableExitAlert", Description="Enable email alert for Exits", Order=3, GroupName="Mail Share Service")]
		public bool EnableExitAlert
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Description = "To Email id", GroupName = "Mail Share Service", Order = 4)]
        public string ToEmailAddress
        {
            get { return toEmailAddress; }
            set 
			{ 
				System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"\b[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}\b");
				
				if (reg.IsMatch((string)value))
					toEmailAddress = value; 
			}
        }
		
		[NinjaScriptProperty]		
		[Display(Name = "Transaction volume", Description = "Transaction volume", GroupName = "Large Trades", Order = 1)]		
		public int Volume_
        {
            get { return vol; }
            set { vol = Math.Max(1, value); }
        }
        [NinjaScriptProperty]       
		[Display(Name = "Time of transation (seconds)", Description = "The time during which the transaction occurred", GroupName = "Large Trades", Order = 2)]       
		public int Time_
        {
            get { return fix; }
            set { fix = Math.Max(1, value); }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Use Orderflow?", Order = 0, GroupName = "OrderFlow")]
		public bool UseOrderflow
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Delta", Description = "Delta", Order = 1, GroupName = "OrderFlow")]
		public double Delta
		{ get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name="UseOnlyOneStrategy", Description= "If the account is in unrealized profit halt trading", Order = 0, GroupName = "Halt Trading")]
		public bool UseOnlyOneStrategy
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="AccountCashValueTarget", Description="Stop if the Account Cash Value Target reached", Order=1, GroupName="Halt Trading")]
        public double AccountCashValueTarget
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="AccountCashValueDrawDown", Description="Stop if Max DrawDown reached", Order=2, GroupName="Halt Trading")]
        public double AccountCashValueDrawDown
        { get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ExcludeMonday", Description="Exclude trading in Monday", Order=1, GroupName="Exclude days of the week")]
		public bool ExcludeMonday 
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ExcludeTuesday", Description="Exclude trading in Tuesday", Order=2, GroupName="Exclude days of the week")]
		public bool ExcludeTuesday 
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ExcludeWednesday", Description="Exclude trading in Wednesday", Order=3, GroupName="Exclude days of the week")]
		public bool ExcludeWednesday 
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ExcludeThursday", Description="Exclude trading in Thursday", Order=4, GroupName="Exclude days of the week")]
		public bool ExcludeThursday 
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="ExcludeFriday", Description="Exclude trading in Friday", Order=5, GroupName="Exclude days of the week")]
		public bool ExcludeFriday 
		{ get; set; }
		
	}
}


    