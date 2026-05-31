//05/23/23

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
	public class FloatingLevelsStratFiltersRTH2 : Strategy
	{
		private RSI RSI1;
		//private EMA EMA1;
		private Series<double>  EMA1;
		private FloatingLevels flLevels;
		private Series<double> flUpLevels;
		private Series<double> flDownLevels;
		private FloatingLevels fl14Levels;
		private double	stopPlot			= 0;		// Value used to plot the stop level
		private double 	previousPrice		= 0;		// previous price used to calculate trailing stop
		private double 	newPrice			= 0;		// Default setting for new price used to calculate trailing stop
		//private double	stopPlot			= 0;		// Value used to plot the stop level
		private double	initialBreakEven	= 0; 		// Default setting for where you set the breakeven
		private int		lastTradeOrderNumber = 0;
		private int barsToWait         =0;
		private EMA EMATrend;
		private int orderInTheBar = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "FloatingLevelsStratFiltersRTH2";
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
				ProfitTarget1					= 40;
				ProfitTarget2					= 100;
				StopLoss					= 40;
				BreakevenTrigger					= 40;
				TrailStep					= 40;
				TrailProfitTrigger					= 20;
				FloatingAvgPeriod					= 14;
				FloatingLevelsPeriod					= 14;
				LevelUpPercent					= 90;
				LevelDownPercent					= 10;
				RSILength					= 14;
				RSIMALength					= 50;
				BarsToWait					= 0;
				UseBreakEvenTrail			= false;
				BarsToWaitAfterMaxLoss      = 6;
				UseShorts					= true;
				EmaTrendLength				= 5;
				MyInput						= PriceType.Close;
				Ma0Type										= CDMAtype.EMA;
				ReverseTrade				= true;
				Quantity					= 2;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				RSI1				= RSI(Close, Convert.ToInt32(RSILength), 3);
				//EMA1				= SMA(RSI1.Avg, RSIMALength).Value;
				GetRSIMA();
				flLevels			= FloatingLevels(FloatingAvgPeriod,FloatingLevelsPeriod,LevelUpPercent,LevelDownPercent,false, MyInput);
				fl14Levels			= FloatingLevels(FloatingAvgPeriod,14,LevelUpPercent,LevelDownPercent,false, MyInput);
				SetStopLoss(@"Long", CalculationMode.Ticks, StopLoss, false);
				SetStopLoss(@"Short", CalculationMode.Ticks, StopLoss, false);
				
				barsToWait = BarsToWait;
				flUpLevels = flLevels.LevelUp;
				flDownLevels = flLevels.LevelDown;
				EMATrend			= EMA(Close,EmaTrendLength);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			
			//if (ToTime(Time[0]) < 093300)
			if (ToTime(Time[0]) < 093600  )
			{
				//maxLossHit = false;
				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
				barsToWait = BarsToWait;
				
			}
			
			if (ToTime(Time[0]) < 093000  || ToTime(Time[0]) > 160000)
			{
				//maxLossHit = false;
				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
				return;
				
			}
			
			if (ToTime(Time[0]) >= 155900)
			{
				if (Position.MarketPosition == MarketPosition.Long)
				ExitLong();
				if (Position.MarketPosition == MarketPosition.Short)
				ExitShort();
			}
			
			
			Series<double> flUp = flLevels.Values[1];
			Series<double> flDown = flLevels.Values[2];
			
			switch (Position.MarketPosition)
            {
				
				// Resets the stop loss to the original value when all positions are closed
                case MarketPosition.Flat:
			
//					if ( (
//							(
//							(CrossAbove(Close,flLevels.LevelUp,1)
//							&& RSI1.Avg[0] > EMA1[0])
//							|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelUp[0])
//							)
//							&& RSI1.Avg[0] < 70 
//							//&& ((BarsSinceExitExecution() > BarsToWait) || (BarsSinceExitExecution() ==-1))
//							)
//							||
//							(CrossAbove(Close,flLevels.LevelDown,1)
//							&& RSI1.Avg[0] < EMA1[0]
//							&& RSI1.Avg[0] > 25
//							//&& ((BarsSinceExitExecution() > BarsToWait) || (BarsSinceExitExecution() ==-1))
//							)
//							&& (ToTime(Time[0])> 095100)
//						)
//					{
//						EnterLong(Convert.ToInt32(2), @"Long");
						
//					}
					
	
					//orderInTheBar = 0;
					/////////////////////////////////////////////////////////////////
						if (
						//(
						//ToTime(Time[0]) < 153000 &&
							!(ToTime(Time[0]) > 114500 && ToTime(Time[0]) < 121700) &&
							!(ToTime(Time[0]) > 154000) &&
						//	((ToTime(Time[0]) > 094000 && ToTime(Time[0]) < 104500) 
						//	||(ToTime(Time[0]) > 141000 && ToTime(Time[0]) < 154500)) &&
							//)&&
							(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
							((BarsSinceExitExecution() > barsToWait) || (BarsSinceExitExecution() ==-1)) &&
							(								
								//(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
								((CrossAbove(Close,flLevels.LevelUp,1)
								&& RSI1[0] > EMA1[0]
								&& RSI1[0] < 75 //&& RSI1[0] > 12
								)
								
								|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelUp[0])
								//&& ToTime(Time[0])> 095100
								)
								//)
								
								//)
						
							)
							
						
					)
					{
						
						EnterLong(Convert.ToInt32(Quantity), @"Long");
						//EnterLongLimit(Close[0] - 0 * TickSize, "Long");
						//Print("Time: -------" + Time[0] + "----Bars Long----" + barsToWait + "----BarsSinceExitExecution()---" + BarsSinceExitExecution());
						//Print(flUp[0] + "---Crossabve--" + flLevels.MALowSeries[1] + "---RSI1.Avg[0]===" + RSI1.Avg[0] + "---EMA1------" + EMA1[0] +"---Time---" + Time[0]);
						//Print(CrossAbove(Close,flLevels.LevelUp,1) + "----Close---" + Close[0] + "----flup---" + flLevels.LevelUp + "-111--" + Time[0]);
						//orderInTheBar = 1;
						
					}
					
					///////////////
					
					
					
					

					
					
					if 
						( 
							//((ToTime(Time[0]) > 094000 && ToTime(Time[0]) < 104500) 
						 //||(ToTime(Time[0]) > 141000 && ToTime(Time[0]) < 154500)) &&
							(
							((CrossBelow(Close,flLevels.LevelDown,1)
							&& RSI1.Avg[0] < EMA1[0])
							||  ((CrossBelow(RSI1.Avg, EMA1, 1)) && Close[0] < flLevels.LevelDown[0])	
							//|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelUp[0])
							)
							&& (ToTime(Time[0])> 094500)
							&& RSI1[0] > 15
							&& !(ToTime(Time[0]) > 120100 && ToTime(Time[0]) < 125800)  
							&& !(ToTime(Time[0]) > 154000) 
							//&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
							&& ((BarsSinceExitExecution() > 5) || (BarsSinceExitExecution() ==-1))
							)
//							||
//							(CrossBelow(Close,flUp,1)
//							&& RSI1.Avg[0] > EMA1[0]
//							&& RSI1.Avg[0] < 70
//							&& (ToTime(Time[0])> 095500)
//							&& ((BarsSinceExitExecution() > barsToWait) || (BarsSinceExitExecution() ==-1))
							&& UseShorts
							&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
							&& !(ToTime(Time[0]) > 120400 && ToTime(Time[0]) < 125800)  
//							)
						)
					{
						EnterShort(Convert.ToInt32(Quantity), @"Short");
						//orderInTheBar = 1;
						//Print(CrossBelow(Close,flUp,1) + "---" + Time[0]);
					}
					//Print(CrossBelow(Close,flUp,1) + "---Out" + Time[0]);
					
//					if (
//							CrossAbove(Close,flLevels.LevelDown,1)
//							&& RSI1.Avg[0] < EMA1[0]
//							&& RSI1.Avg[0] > 25
//							&& ((BarsSinceExitExecution() > 20) || (BarsSinceExitExecution() ==-1))
//						)
//					{
//						EnterLong(Convert.ToInt32(2), @"Long");
//					}
					break;
				case MarketPosition.Long:
					//////////////////////////////////////////////////////////////////////////////////////////////////
//					if (CrossBelow(Close,flLevels.LevelDown,0)	)
//					{
//						ExitLong(Position.Quantity);
//					}
//					 if (
//						 (GetCurrentAsk() >= Position.AveragePrice + BreakevenTrigger * TickSize)
//						 && previousPrice == 0
//						// || (CrossBelow(Close,flLevels.LevelDown,1)						 )
//						 )
//                    {
//						if (UseBreakEvenTrail)
//						{
//							ExitLong(1);
//							previousPrice = initialBreakEven + BreakevenTrigger * TickSize;
//						}
//						else
//						{
//							ExitLong(Position.Quantity);
//							previousPrice = 0;
//						}
//						initialBreakEven = Position.AveragePrice;
//                        SetStopLoss(CalculationMode.Price, initialBreakEven);
						
//						stopPlot = initialBreakEven;
//					}
//					else if (previousPrice	!= 0 ////StopLoss is at breakeven
// 							&& GetCurrentAsk() > previousPrice + TrailProfitTrigger * TickSize && UseBreakEvenTrail)
//					{
//						newPrice = previousPrice + TrailStep * TickSize; 	// Calculate trail stop adjustment
//						SetStopLoss(CalculationMode.Price, newPrice);			// Readjust stoploss level		
//						previousPrice = previousPrice + TrailProfitTrigger * TickSize;				 				// save for price adjust on next candle
//						stopPlot = newPrice; 					 				// save to adjust plot line
//					}
					//////////////////////////////////////////////////////////////////////////////////////////////////
					/// 
					
					
					 if (
						 (GetCurrentAsk() >= Position.AveragePrice + BreakevenTrigger * TickSize)						
						// || (CrossBelow(Close,fl14Levels.Values[2],1)						 )
						  || (CrossBelow(Close,flLevels.LevelDown,1)						 )
						 //	|| ( orderInTheBar != 0 && (High[0] < Position.AveragePrice + 3 * TickSize))
						 )
	                    {
							
							if 
								( 
									ReverseTrade &&
									//((ToTime(Time[0]) > 094000 && ToTime(Time[0]) < 104500) 
								 //||(ToTime(Time[0]) > 141000 && ToTime(Time[0]) < 154500)) &&
									(
									((CrossBelow(Close,flLevels.LevelDown,1)
									//&& RSI1.Avg[0] < EMA1[0]
								)
									//||  ((CrossBelow(RSI1.Avg, EMA1, 1)) && Close[0] < flLevels.LevelDown[0])								
									)
									&& (ToTime(Time[0])> 094500)
									&& RSI1[0] > 15
									&& !(ToTime(Time[0]) > 120400 && ToTime(Time[0]) < 125800)  
									&& !(ToTime(Time[0]) > 154000) 
									)
									&& UseShorts
									&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
									&& !(ToTime(Time[0]) > 120400 && ToTime(Time[0]) < 125800)  
		//							)
								)
							{
								EnterShort(Convert.ToInt32(Quantity), @"Short");
								//orderInTheBar = 1;
								//Print(CrossBelow(Close,flUp,1) + "---" + Time[0]);
							}
							else
							{	
							   
							
								ExitLong(Position.Quantity);
							}
							//Print("flDown--" + flDown[1]);
							
							
							
						}
						if (orderInTheBar == 1)
							{
								if (Position.MarketPosition == MarketPosition.Long)
								{
									orderInTheBar = 0;
								}
							}
					break;
				case MarketPosition.Short:
					
					
						//////////////////////////////////////////////////////////////////////////////////////////////////\
					/// 
					 if (
						   (GetCurrentAsk() < Position.AveragePrice - (BreakevenTrigger) * TickSize)
						   || (CrossAbove(Close,flUp,1))
						 	//|| ( orderInTheBar != 0 && (Low[0] > Position.AveragePrice - 3 * TickSize))
						    //&& previousPrice == 0
						   )
		                    {		
									
									if (
											ReverseTrade &&
											!(ToTime(Time[0]) > 114500 && ToTime(Time[0]) < 121400) &&
											!(ToTime(Time[0]) > 154000) &&
											//((ToTime(Time[0]) > 094000 && ToTime(Time[0]) < 104500) 
											//||(ToTime(Time[0]) > 141000 && ToTime(Time[0]) < 154500))
											//&&
											(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
											//((BarsSinceExitExecution() > barsToWait) || (BarsSinceExitExecution() ==-1)) &&
											(								
												//(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
												((CrossAbove(Close,flLevels.LevelUp,1)
												&& RSI1[0] > EMA1[0]
												&& RSI1[0] < 70 //&& RSI1[0] > 12
												)
												
												|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelUp[0])
												//&& ToTime(Time[0])> 095100
												)
												//)
												
												//)
										
											)
										)
									{
										EnterLong(Quantity,"Long");
										orderInTheBar = 1;
										
										//Print(CrossAbove(Close,flLevels.LevelUp,1) + "-222--" + Time[0]);
									}
									else
									{
										ExitShort(Position.Quantity);
									}
									
							}
							if (orderInTheBar == 1)
							{
								if (Position.MarketPosition == MarketPosition.Short)
								{
									orderInTheBar = 0;
								}
							}
					
					break;
			}

			if (SystemPerformance.AllTrades.Count > 1)

		    {

		        Trade lastTrade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - 1];
				//Print("-------maxLossHit - Short - " + maxLossHit + "----------dateLossHit: " + dateLossHit + "-------Order  number" + lastTrade.TradeNumber + "---Today---" + ToDay(Time[0]));
		        //Trade firstTrade = Performance.AllTrades[0];
				//if (lastTrade.ProfitCurrency <= -10 && lastTradeOrderNumber != lastTrade.TradeNumber
				//if (lastTrade.ProfitCurrency <= -10 && dateLossHit == ToDay(Time[0]))
				if (lastTrade.ProfitCurrency <= -10 )
				{
					//maxLossHit = false;
					//dateLossHit = ToDay(Time[0]);
					lastTradeOrderNumber = lastTrade.TradeNumber;
					barsToWait = 1;
					
					//Print("-------maxLossHit - Short - " + maxLossHit + "----------dateLossHit: " + dateLossHit + "-------Order  number" + lastTrade.TradeNumber + "---Today---" + ToDay(Time[0]));
					if (lastTrade.ProfitCurrency <= -275 )
					{
						barsToWait = BarsToWaitAfterMaxLoss;
						//Print("Time: -------" + Time[0] + "----Bars----" + barsToWait);
					}
					
				}			
				else
				{
					barsToWait = BarsToWait;
				}
		 
				//Print("Time: -------" + Time[0] + "----Bars----" + barsToWait);
		        //Print("The last trade profit is " + );
		        //Print("The first trade profit is " + firstTrade.ProfitPercent);

		    }
		
			
			
		}

		
		private void GetRSIMA()
		{
			switch (Ma0Type)
				{
					case CDMAtype.DEMA:						
						
						EMA1 = DEMA(RSI1.Avg, RSIMALength).Value;
						
						break;
						
					case CDMAtype.EMA:
							EMA1 = EMA(RSI1.Avg, RSIMALength).Value;
						
					break;	
						
					case CDMAtype.HMA:
							EMA1 = HMA(RSI1.Avg, RSIMALength).Value;
					
					break;	
						
					case CDMAtype.LinReg:
							EMA1 = LinReg(RSI1.Avg, RSIMALength).Value;

					break;							
						
					case CDMAtype.SMA:
							EMA1 = SMA(RSI1.Avg, RSIMALength).Value;
					
					break;	
						
					case CDMAtype.TEMA:
							EMA1 = TEMA(RSI1.Avg, RSIMALength).Value;

					break;	
						
					case CDMAtype.TMA:	
							EMA1 = TMA(RSI1.Avg, RSIMALength).Value;
					
					break;	
					
					case CDMAtype.VWMA:
							EMA1 = VWMA(RSI1.Avg, RSIMALength).Value;

					break;	
						
					case CDMAtype.WMA:
							EMA1 = WMA(RSI1.Avg, RSIMALength).Value;
							
					break;
						
					case CDMAtype.ZLEMA:
							EMA1 = ZLEMA(RSI1.Avg, RSIMALength).Value;

					break;												
				}		
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(20, int.MaxValue)]
		[Display(Name="ProfitTarget1", Order=1, GroupName="Parameters")]
		public int ProfitTarget1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(40, int.MaxValue)]
		[Display(Name="ProfitTarget2", Order=2, GroupName="Parameters")]
		public int ProfitTarget2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="StopLoss", Order=3, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="BreakevenTrigger", Order=4, GroupName="Parameters")]
		public int BreakevenTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="TrailStep", Order=5, GroupName="Parameters")]
		public int TrailStep
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="TrailProfitTrigger", Order=6, GroupName="Parameters")]
		public int TrailProfitTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="FloatingAvgPeriod", Order=7, GroupName="Parameters")]
		public int FloatingAvgPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="FloatingLevelsPeriod", Order=8, GroupName="Parameters")]
		public int FloatingLevelsPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(70, double.MaxValue)]
		[Display(Name="LevelUpPercent", Order=9, GroupName="Parameters")]
		public int LevelUpPercent
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, double.MaxValue)]
		[Display(Name="LevelDownPercent", Order=10, GroupName="Parameters")]
		public int LevelDownPercent
		{ get; set; }

		[NinjaScriptProperty]
		[Range(7, int.MaxValue)]
		[Display(Name="RSILength", Order=11, GroupName="Parameters")]
		public int RSILength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="RSIMALength", Order=12, GroupName="Parameters")]
		public int RSIMALength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name=" RSI MA Type", Description="RSI MA Type", Order=13)]
		public CDMAtype Ma0Type
        { get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="BarsToWait", Order=18, GroupName="Parameters")]
		public int BarsToWait
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="UseBreakEvenTrail", Order=25, GroupName="Parameters")]
		public bool UseBreakEvenTrail
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="BarsToWaitAfterMaxLoss", Description="Number of ticks above entry the trail profit movement trigger is set", Order=19, GroupName="Parameters")]
		public int BarsToWaitAfterMaxLoss
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="UseShorts", Order=15, GroupName="Parameters")]
		public bool UseShorts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="EmaTrendLength", Description="Number of ticks above entry the trail profit movement trigger is set", Order=17, GroupName="Parameters")]
		public int EmaTrendLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="  MA PriceType", Description="Select price Type (Close, high, Low, etc.)", Order=99)]
		public PriceType MyInput
        { get; set; }	
		
		[NinjaScriptProperty]		
		[Display(Name="Reverse Trade", Order=22, GroupName="Parameters")]
		public bool ReverseTrade
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Quantity", Order=25, GroupName="Parameters")]
		public int Quantity
		{ get; set; }
		
		#endregion

	}
}