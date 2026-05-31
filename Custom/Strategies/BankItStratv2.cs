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
using NinjaTrader.NinjaScript.Indicators.TradeSaber;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class BankItStratv2 : Strategy
	{
		private BankItSystem BankItSystem1;
		private EMA EMA1;
		private VisEMA vEMA1;
		private string  atmStrategyId			= string.Empty;
		private string  orderId					= string.Empty;
		private bool	isAtmStrategyCreated	= false;
		private bool isLong = false;
		private bool isShort = false;
		private ReversalTS revTS;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BankItStratv2";
				
				
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;
				BankMAType = BankItType.ALMA;
				AllowTrades = true;
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 1);
			}
			else if (State == State.DataLoaded)
			{				
				// BankItSystem1				= BankItSystem(Close, @"BankItSystem - v1.0", BankItType.Tillson, 6, 0.1, 8, 0.5, 6, 0.35, 5, 0.3, 4, 0.3, 42, 14, 10);
				BankItSystem1					= BankItSystem(@"BankItSystem - v1.0",BankItType.ALMA, 6, 0.1, 8, 0.5, 6, 0.35, 5, 0.3, 4, 0.3, 4,0.3
																										,21,6,0.95, 31,6.35,0.95,34,6.35,0.95,37,6.35,0.95,42,6.35,0.95,55,6.35,0.95
																										,42, 14, 10);
				vEMA1 = VisEMA(1,14);
				EMA1				= EMA(Closes[1], 14);
				revTS								= ReversalTS(0, 0, 0, false, Brushes.AliceBlue, Brushes.AntiqueWhite,false, Brushes.AntiqueWhite, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;

			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1
			|| CurrentBars[1] < 0)
				return;
			
			// Make sure this strategy does not execute against historical data
			if(State == State.Historical)
				return;
			
	
					
//					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
//					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
//					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
				
//			Print ("Close[0] less Close[1]" + (Close[0] < Close[1]).ToString() + "-----" + Time[0]);
//			Print ("Closes[1][0] less EMA1[0])" + (Closes[1][0] < EMA1[0]).ToString() + "-----" + Time[0]);
//			Print ("CrossBelow Close, BankItSystem1.LowestValue" + (CrossBelow(Close, BankItSystem1.LowestValue, 1)).ToString() + "-----" + Time[0]);
//			Print ("BankItSystem1.ZombieMeanChangeValues greater	BankItSystem1.HighestValue	" + (BankItSystem1.ZombieMeanChangeValues[0] >	BankItSystem1.HighestValue[0]).ToString()	+ "-----" + Time[0]);
			

			 // Set 1
			if (
				 orderId.Length == 0 && atmStrategyId.Length == 0 &&
				(BankItSystem1.CloudBullish[0] == 1)
				 && (Close[0] > Close[1])
				 && ((Closes[1][0] > EMA1[0]) 
					|| (Math.Abs((Close[0] - vEMA1[0])) > 40))
				 && (CrossAbove(Close, BankItSystem1.HighestValue, 2)
						|| CrossAbove(Close, BankItSystem1.ZombieMeanChangeValues, 1))
				 && 	BankItSystem1.ZombieMeanChangeValues[0] <	BankItSystem1.LowestValue[0]					 
				
				)
			{
				
				if (AllowTrades)
				{
					isAtmStrategyCreated = false;  // reset atm strategy created check to false
					atmStrategyId = GetAtmStrategyUniqueId();
					orderId = GetAtmStrategyUniqueId();
					AtmStrategyCreate(OrderAction.Buy, OrderType.Limit, Low[0], 0, TimeInForce.Day, orderId, "BanksyNQ5b", atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
						//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
						if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
							isAtmStrategyCreated = true;
						
					});
					isLong = true;
					isShort = false;
				}
				else
				{
					Draw.Text(this, Convert.ToString("Long") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
					isLong = true;
					isShort = false;
				}
			}
			
//			Print ("BankItSystem1 CloudBearish" + BankItSystem1.CloudBearish[0] + "-----" + Time[0]);
//			Print ("Close[0] less Close[1]" + (Close[0] < Close[1]).ToString() + "-----" + Time[0]);
//			Print ("Closes[1][0] less EMA1[0])" + (Closes[1][0] < EMA1[0]).ToString() + "-----" + Time[0]);
//			Print ("CrossBelow Close, BankItSystem1.LowestValue" + (CrossBelow(Close, BankItSystem1.LowestValue, 1)).ToString() + "-----" + Time[0]);
//			Print ("BankItSystem1.ZombieMeanChangeValues greater	BankItSystem1.HighestValue	" + (BankItSystem1.ZombieMeanChangeValues[0] >	BankItSystem1.HighestValue[0]).ToString()	+ "-----" + Time[0]);
//			Print ("orderId.Length	" + orderId.Length.ToString()	+ "-----" + Time[0]);
//			Print ("atmStrategyId.Length	" + atmStrategyId.Length.ToString()	+ "-----" + Time[0]);
			
			if (isLong
				&& revTS.CurrentReversalBar[0] == -1 
			 && BankItSystem1.CloudBullish[0] != 1)
			{
				Draw.Text(this, Convert.ToString("LongExit") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Purple );
				isLong = false;
			}
			
			
			if (
				  orderId.Length == 0 && atmStrategyId.Length == 0 
				 && (BankItSystem1.CloudBearish[0] == 1)
				 && (Close[0] < Close[1])
				 && ((Closes[1][0] < EMA1[0]) 
					|| (Math.Abs((Close[0] - vEMA1[0])) > 40))
				 && (CrossBelow(Close, BankItSystem1.LowestValue, 2)
						|| CrossBelow(Close, BankItSystem1.ZombieMeanChangeValues, 1))
				 && 	BankItSystem1.ZombieMeanChangeValues[0] >	BankItSystem1.HighestValue[0]		
				
				)
			{
				if (AllowTrades)
				{
					isAtmStrategyCreated = false;  // reset atm strategy created check to false
					atmStrategyId = GetAtmStrategyUniqueId();
					orderId = GetAtmStrategyUniqueId();
					AtmStrategyCreate(OrderAction.Sell, OrderType.Limit, Low[0], 0, TimeInForce.Day, orderId, "BanksyNQ5b", atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
						//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
						if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
							isAtmStrategyCreated = true;
					});
				}
				else
				{
					Draw.Text(this, Convert.ToString("Short") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );
					Print ("BankItSystem1 CloudBearish" + BankItSystem1.CloudBearish[0] + "-----" + Time[0]);
					
				}
				isLong = false;
				isShort = true;
			}
			
			
			if (isShort
				&& revTS.CurrentReversalBar[0] == 1 
			 && BankItSystem1.CloudBearish[0] != 1)
			{
				Draw.Text(this, Convert.ToString("ShortExit") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Purple );
				isShort = false;
			}
			
			
			if (AllowTrades)
			{
			
				if (!isAtmStrategyCreated)
					return;
				
				// Check for a pending entry order
				if (orderId.Length > 0)
				{
					string[] status = GetAtmStrategyEntryOrderStatus(orderId);
	
					// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
					if (status.GetLength(0) > 0)
					{
						// Print out some information about the order to the output window
						Print("The entry order average fill price is: " + status[0]);
						Print("The entry order filled amount is: " + status[1]);
						Print("The entry order order state is: " + status[2]);
	
						// If the order state is terminal, reset the order id value
						if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
							orderId = string.Empty;
					}
				} // If the strategy has terminated reset the strategy id
				else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat)
					atmStrategyId = string.Empty;
	
				if (atmStrategyId.Length > 0)
				{
					// You can change the stop price
					if (GetAtmStrategyMarketPosition(atmStrategyId) != MarketPosition.Flat)
						AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP1", atmStrategyId);
	
					// Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
					// the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
					Print("The current ATM Strategy market position is: " + GetAtmStrategyMarketPosition(atmStrategyId));
					Print("The current ATM Strategy position quantity is: " + GetAtmStrategyPositionQuantity(atmStrategyId));
					Print("The current ATM Strategy average price is: " + GetAtmStrategyPositionAveragePrice(atmStrategyId));
					Print("The current ATM Strategy Unrealized PnL is: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyId));
				}
			}
			
		}
		
		 #region Properties
		
		[NinjaScriptProperty]
		[Display(Name="MA Type", Description="MA Type", Order=1)]
		public BankItType BankMAType
        { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name=" AllowTrades", Description="AllowTrades", Order=2)]
		public bool AllowTrades
        { get; set; }
		
			
		 #endregion;
	}
}
