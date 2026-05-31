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
	public class FLevelsLower1130WithArrowsbak : Strategy
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
		private DateTime nextTradeTime;
		bool isLong = false;
		bool isShort = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"FLevels Until 11:30 Arrows.";
				Name										= "FLevelsLower1130WithArrowsbak";
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
				
				StopLoss					= 40;
				ProfitTarget					= 40;
				FloatingAvgPeriod					= 14;
				FloatingLevelsPeriod					= 14;
				LevelUpPercent					= 90;
				LevelDownPercent					= 10;
				RSILength					= 14;
				RSIMALength					= 50;
				BarsToWait					= 0;
				BarsToWaitAfterMaxLoss      = 6;
				UseShorts					= true;
				EmaTrendLength				= 5;
				MyInput						= PriceType.Close;
				Ma0Type										= CDMAtype.EMA;
				ReverseTrade				= true;
				Quantity					= 2;
				
				UseOnlyArrows			= true;
				
				
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
			
			if (Bars.IsFirstBarOfSession)
			{
				nextTradeTime			= Time[0];
			}
			
			//if (ToTime(Time[0]) < 093300)
			if (ToTime(Time[0]) < 093600  )
			{
				barsToWait = BarsToWait;
			}
			
			if (ToTime(Time[0]) < 093300  || ToTime(Time[0]) > 113000)
			{
				return;
				
			}
			
			if (ToDay(Time[0]) == 20240911)
				return;
			
			
			
			if (ToTime(Time[0]) >= 155900)
			{
				if (Position.MarketPosition == MarketPosition.Long)
				ExitLong();
				if (Position.MarketPosition == MarketPosition.Short)
				ExitShort();
			}
			
			
			//Print(nextTradeTime);
			
			Series<double> flUp = flLevels.Values[1];
			Series<double> flDown = flLevels.Values[2];
			
			if (UseOnlyArrows)
			{
				if (
					(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
					(								
						((CrossAbove(Close,flLevels.LevelDown,1)
						&& RSI1[0] > EMA1[0]
						&& RSI1[0] < 70 //&& RSI1[0] > 12
						)								
						|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelDown[0])
						)
					)
					&&	(isLong == false) 
				)
					{
						//EnterLong(Convert.ToInt32(Quantity), @"Long");
						Draw.Text(this, Convert.ToString("FLLong") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-45 * TickSize)), Brushes.DodgerBlue );	
						isShort = false;
						isLong = true;
					}
					
					if 
						( 
							(
							((CrossBelow(Close,flLevels.LevelUp,1)
							&& RSI1.Avg[0] < EMA1[0])
							)
							&& RSI1[0] > 15
							
							&& UseShorts
							&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
							&&	(isShort == false) 
							)
							|| ((CrossBelow(RSI1.Avg, EMA1, 1)) && Close[0] < flLevels.LevelUp[0])
						)
					{
						//EnterShort(Convert.ToInt32(Quantity), @"Short");
						Draw.Text(this, Convert.ToString("FLShort") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (45 * TickSize)), Brushes.HotPink );		
						isLong = false;
						isShort = true;
					}
					
					 if (
							 (CrossBelow(Close,flLevels.LevelUp,1) && isLong == true)		
						
						 )
	                    {								
							if 
							( 
								ReverseTrade &&		
								(
								((CrossBelow(Close,flLevels.LevelUp,1)
								))
								&& RSI1[0] > 15								
								&& UseShorts
								&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
								)
								|| ((CrossBelow(RSI1.Avg, EMA1, 1)) && Close[0] < flLevels.LevelUp[0])
							)
							{
								Draw.Text(this, Convert.ToString("FLShort") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (45 * TickSize)), Brushes.HotPink );		
								isLong = false;
								isShort = true;
							}
							else
							{	
								Draw.Text(this, Convert.ToString("FLShortExit") + Convert.ToString(CurrentBars[0]), "ExitL" + System.Environment.NewLine + @"🢃", 0, (High[0] + (45 * TickSize)), Brushes.White );		
								isLong = false;
								isShort = false;
							}							
						}
						
						
						 if (
							 (CrossAbove(Close,flLevels.LevelDown,1)	&& isShort == true)
						 )
	                    {								
							if (
										ReverseTrade &&//										
										(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
										(								
											((RSI1[0] > EMA1[0]
											&& RSI1[0] < 70 //&& RSI1[0] > 12
											)
											|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelDown[0])
										)
									
										)
									)
								{
									Draw.Text(this, Convert.ToString("FLLong") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-45 * TickSize)), Brushes.DodgerBlue );	
									isLong = true;
									isShort = false;
								}
								else
								{
									Draw.Text(this, Convert.ToString("FLShortExit") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ExitL", 0, (Low[0] + (-45 * TickSize)), Brushes.White );	
									isLong = false;
									isShort = false;
								}				
						}
						
						
			}
			else
			{
				switch (Position.MarketPosition)
	            {	// Resets the stop loss to the original value when all positions are closed
	                case MarketPosition.Flat:
						if (
							(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
							((BarsSinceExitExecution() > barsToWait) || (BarsSinceExitExecution() ==-1)) &&
							(								
								((CrossAbove(Close,flLevels.LevelDown,1)
								&& RSI1[0] > EMA1[0]
								&& RSI1[0] < 70 //&& RSI1[0] > 12
								)
								
								|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelDown[0])
								)
							)
						)
						{
							Draw.Text(this, Convert.ToString("FLLong") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-45 * TickSize)), Brushes.DodgerBlue );	
							EnterLong(Convert.ToInt32(Quantity), @"Long");
						}
						
						if 
							( 
								(
								((CrossBelow(Close,flLevels.LevelUp,1)
								&& RSI1.Avg[0] < EMA1[0])
								
								&& RSI1[0] > 15
								)
								&& UseShorts
								&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) 
								)
								|| ((CrossBelow(RSI1.Avg, EMA1, 1)) && Close[0] < flLevels.LevelUp[0])
							)
						{
							Draw.Text(this, Convert.ToString("FLShort") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (45 * TickSize)), Brushes.HotPink );		
							EnterShort(Convert.ToInt32(Quantity), @"Short");
						}
						break;
					case MarketPosition.Long:
						
						 if (
							 (GetCurrentAsk() >= Position.AveragePrice + ProfitTarget * TickSize)						
							  || (CrossBelow(Close,flLevels.LevelUp,1)						 )
							 )
		                    {								
								if 
								( 
									ReverseTrade &&
									(
									((CrossBelow(Close,flLevels.LevelUp,1)
									))
									&& RSI1[0] > 15
									
									&& UseShorts
									&&  (Close[0] < EMATrend[0] || EmaTrendLength == 1) )
									|| ((CrossBelow(RSI1.Avg, EMA1, 1)) && Close[0] < flLevels.LevelUp[0])
									
								)
								{
									Draw.Text(this, Convert.ToString("FLShort") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (45 * TickSize)), Brushes.HotPink );		
									EnterShort(Convert.ToInt32(Quantity), @"Short");
								}
								else
								{	
									Draw.Text(this, Convert.ToString("FLShortExit") + Convert.ToString(CurrentBars[0]), "ExitL" + System.Environment.NewLine + @"🢃", 0, (High[0] + (45 * TickSize)), Brushes.White );		
									ExitLong(Position.Quantity);
								}							
							}
							
						break;
					case MarketPosition.Short:
						 if (
							   (GetCurrentAsk() < Position.AveragePrice - (ProfitTarget) * TickSize)
							
							)
		                    {		
								if (
										ReverseTrade &&//										
										(Close[0] >= EMATrend[0] || EmaTrendLength == 1) && 
										(								
											((CrossAbove(Close,flLevels.LevelDown,1)
											&& RSI1[0] > EMA1[0]
											&& RSI1[0] < 70 //&& RSI1[0] > 12
											)
											
											|| ((CrossAbove(RSI1.Avg, EMA1, 1)) && Close[0] > flLevels.LevelDown[0])
										)
									
										)
									)
								{
									Draw.Text(this, Convert.ToString("FLLong") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-45 * TickSize)), Brushes.DodgerBlue );	
									EnterLong(Quantity,"Long");
								}
								else
								{
									Draw.Text(this, Convert.ToString("FLShortExit") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ExitL", 0, (Low[0] + (-45 * TickSize)), Brushes.White );	
									ExitShort(Position.Quantity);
								}
									
							}
				
							break;
					}
				if (SystemPerformance.AllTrades.Count > 1)

			    {
	
			        Trade lastTrade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - 1];
					
					if (lastTrade.ProfitCurrency <= -10 )
					{
						lastTradeOrderNumber = lastTrade.TradeNumber;
						barsToWait = 1;
						
						if (lastTrade.ProfitCurrency <= -250 )
						{
							
							nextTradeTime =  lastTrade.Entry.Time.AddMinutes(8);
							barsToWait = BarsToWaitAfterMaxLoss;
						}
						
					}			
					else
					{
						nextTradeTime = lastTrade.Entry.Time;
						barsToWait = BarsToWait;
					}
				
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
		[Range(4, int.MaxValue)]
		[Display(Name="StopLoss", Order=3, GroupName="Trades")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(4, int.MaxValue)]
		[Display(Name="ProfitTarget", Order=4, GroupName="Trades")]
		public int ProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Quantity", Order=5, GroupName="Trades")]
		public int Quantity
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="FloatingAvgPeriod", Order=1, GroupName="MainStrat")]
		public int FloatingAvgPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="FloatingLevelsPeriod", Order=2, GroupName="MainStrat")]
		public int FloatingLevelsPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(70, double.MaxValue)]
		[Display(Name="LevelUpPercent", Order=3, GroupName="MainStrat")]
		public int LevelUpPercent
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, double.MaxValue)]
		[Display(Name="LevelDownPercent", Order=4, GroupName="MainStrat")]
		public int LevelDownPercent
		{ get; set; }

		[NinjaScriptProperty]
		[Range(7, int.MaxValue)]
		[Display(Name="RSILength", Order=5, GroupName="MainStrat")]
		public int RSILength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="RSIMALength", Order=6, GroupName="MainStrat")]
		public int RSIMALength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name=" RSI MA Type", Description="RSI MA Type", Order=7, GroupName="MainStrat" )]
		public CDMAtype Ma0Type
        { get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="UseShorts", Order=8, GroupName="MainStrat")]
		public bool UseShorts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="EmaTrendLength", Description="Number of ticks above entry the trail profit movement trigger is set", Order=9, GroupName="MainStrat")]
		public int EmaTrendLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="  MA PriceType", Description="Select price Type (Close, high, Low, etc.)", Order=10, GroupName="MainStrat")]
		public PriceType MyInput
        { get; set; }	
		
		[NinjaScriptProperty]		
		[Display(Name="Reverse Trade", Order=11, GroupName="MainStrat")]
		public bool ReverseTrade
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="UseOnlyArrows", Order=12, GroupName="MainStrat")]
		public bool UseOnlyArrows
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="BarsToWait", Order=1, GroupName="TradeOnlyParameters")]
		public int BarsToWait
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="BarsToWaitAfterMaxLoss", Description="Number of ticks above entry the trail profit movement trigger is set", Order=2, GroupName="TradeOnlyParameters")]
		public int BarsToWaitAfterMaxLoss
		{ get; set; }
		
		
		
		
		
		
		
		
		#endregion

	}
}