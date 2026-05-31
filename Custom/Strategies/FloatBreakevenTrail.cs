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
	public class FloatBreakevenTrail : Strategy
	{
		private RSI RSI1;
		private EMA EMAFast;
		private EMA EMASlow;
		private FloatingLevels flLevels;
		private Series<double> flUpLevels;
		private Series<double> flDownLevels;
		private Series<double>  EMA1;
		private int barsToWait         =0;
		private EMA EMATrend;
		private int orderInTheBar = 0;
		private double	currentStop			= 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "FloatBreakevenTrail";
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
				ProfitTarget1				= 40;
				ProfitTarget2				= 100;
				StopLoss					= 40;
				BreakevenTrigger			= 40;
				TrailStep					= 40;
				TrailProfitTrigger			= 20;
				FloatingAvgPeriod			= 14;
				FloatingLevelsPeriod		= 14;
				LevelUpPercent				= 90;
				LevelDownPercent			= 10;
				RSILength					= 14;
				RSIMALength					= 50;
				BarsToWait					= 0;
				UseBreakEvenTrail			= false;
				BarsToWaitAfterMaxLoss      = 6;
				UseShorts					= true;
				EmaTrendLength				= 5;
				MyInput						= PriceType.Close;
				Ma0Type						= CDMAtype.EMA;
				ReverseTrade				= true;
				Quantity					= 2;
				FastEMA					= 9;
				SlowEMA					= 21;
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
			
				
				
				
				barsToWait = BarsToWait;
				flUpLevels = flLevels.LevelUp;
				flDownLevels = flLevels.LevelDown;
				EMATrend			= EMA(Close,EmaTrendLength);
				EMAFast			= EMA(Close,FastEMA);
				EMASlow			= EMA(Close,SlowEMA);
				SetStopLoss(@"Long", CalculationMode.Ticks, StopLoss, false);
				SetStopLoss(@"Short", CalculationMode.Ticks, StopLoss, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			
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
			 // Set 1
				 case MarketPosition.Flat:
					
					if (	
							//!(ToTime(Time[0]) > 140000 && ToTime(Time[0]) < 141800) &&
						!(ToTime(Time[0]) > 115200 && ToTime(Time[0]) < 130000) &&
							!(ToTime(Time[0]) > 154000) &&
							(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
							((BarsSinceExitExecution() > barsToWait) || (BarsSinceExitExecution() ==-1)) &&
							(	
								((CrossAbove(Close,flLevels.LevelUp,1)
								&& RSI1[0] > EMA1[0]
								&& RSI1[0] < 70 
								)								
									|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelUp[0])
								
								)	
							)
					)
					{
						EnterLong(Convert.ToInt32(Quantity), @"Long");
					}
			
					
					if 
						( 
							(
							((CrossBelow(Close,flLevels.LevelDown,1)
							&& RSI1.Avg[0] < EMA1[0])
							)
							&& (ToTime(Time[0])> 093800)
							&& RSI1[0] > 15
							&& !(ToTime(Time[0]) > 115200 && ToTime(Time[0]) < 130000)  
							&& !(ToTime(Time[0]) > 154000) 
						    // && !(ToTime(Time[0]) > 140000 && ToTime(Time[0]) < 141800) 
							&& ((BarsSinceExitExecution() > 5) || (BarsSinceExitExecution() ==-1))
							)
							&& UseShorts
							&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
							&& !(ToTime(Time[0]) > 120400 && ToTime(Time[0]) < 125800)  
						)
					{
						EnterShort(Convert.ToInt32(Quantity), @"Short");
					}

					break;
				 case MarketPosition.Long:
					 // Set 2
					if ((GetCurrentAsk(0) >= Position.AveragePrice +  BreakevenTrigger * TickSize)
						 && UseBreakEvenTrail)
					{
						SetStopLoss(@"Long", CalculationMode.Ticks, BreakevenTrigger, false);
					}
					
//					if ((GetCurrentAsk(0) >= Position.AveragePrice +  ProfitTarget1 * TickSize)
//						&& Position.Quantity > 1)
//					{
//						ExitLong(Position.Quantity - 1);
//					}
					
					if (
						 (GetCurrentAsk() >= Position.AveragePrice + ProfitTarget1 * TickSize)	)
					 {
						 if (Position.Quantity ==2)
						 	ExitLong(1);
					 }
					
//					if ((GetCurrentAsk(0) >= Position.AveragePrice + ProfitTarget2 * TickSize)
//						 )
//					{
//						ExitLong();
//					}
					
					if (
						  (CrossBelow(Close,flLevels.LevelDown,1)						 )
						 )
	                    {
							
							if 
								( 
									ReverseTrade &&								
									((CrossBelow(Close,flLevels.LevelDown,1)
									)
									&& (ToTime(Time[0])> 094500)
									&& RSI1[0] > 15
									&& !(ToTime(Time[0]) > 120400 && ToTime(Time[0]) < 125800)  
									&& !(ToTime(Time[0]) > 154000) 
									)
									&& UseShorts
									&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
									&& !(ToTime(Time[0]) > 120400 && ToTime(Time[0]) < 125800)  
								)
							{
								
								EnterShort(Convert.ToInt32(Quantity), @"Short");
							}
							else
							{	
								ExitLong(Position.Quantity);
							}	
						}
						else  if (CrossBelow(EMAFast,EMASlow,1) )							
						{
							ExitLong(Position.Quantity);
						}
					break;
				case MarketPosition.Short:
					 // Set 2
					if ((GetCurrentAsk(0) <= Position.AveragePrice +  BreakevenTrigger * TickSize)
						 && UseBreakEvenTrail)
					{
						SetStopLoss(@"Short", CalculationMode.Ticks, BreakevenTrigger, false);
					}
					
					if ((GetCurrentAsk(0) <= Position.AveragePrice -  ProfitTarget1 * TickSize)
						//&& Position.Quantity > 1
						)
					{
						//ExitShort(Position.Quantity - 1);
						ExitShort(Position.Quantity - 1,"EXitShortMore","Short");
						
					}
					
//					if ((GetCurrentAsk(0) <= Position.AveragePrice + ProfitTarget2 * TickSize)
//						 )
//					{
//						ExitShort();
//					}
					
					 if (
						   CrossAbove(Close,flUp,1))
                    {		
							
							if (
									ReverseTrade &&
										!(ToTime(Time[0]) > 114500 && ToTime(Time[0]) < 121400) &&
										!(ToTime(Time[0]) > 154000) &&							
									(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
									(								
										((CrossAbove(Close,flLevels.LevelUp,1)
										&& RSI1[0] > EMA1[0]
										&& RSI1[0] < 70 
										)
										
										|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelUp[0])
										)
										
									)
								)
							{
								
								EnterLong(Quantity,"Long");
								orderInTheBar = 1;
							}
							else
							{
								ExitShort(Position.Quantity);
							}
					}
					else  if (CrossAbove(EMAFast,EMASlow,1) )
								//&& Position.Quantity ==1)
							{
								ExitShort(Position.Quantity);
							}
								
					break;
			}
				
			if (SystemPerformance.AllTrades.Count > 1)

		    {

		        Trade lastTrade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - 1];
				
				if (lastTrade.ProfitCurrency <= -10 )
				{
				
					barsToWait = 1;
					if (lastTrade.ProfitCurrency <= -275 )
					{
						barsToWait = BarsToWaitAfterMaxLoss;
					}
					
				}			
				else
				{
					barsToWait = BarsToWait;
				}

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
		[Range(0, int.MaxValue)]
		[Display(Name="ProfitTarget1", Order=1, GroupName="Parameters")]
		public int ProfitTarget1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
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
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastEMA", Order=26, GroupName="Parameters")]
		public int FastEMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowEMA", Order=27, GroupName="Parameters")]
		public int SlowEMA
		{ get; set; }
		#endregion

	}
}
