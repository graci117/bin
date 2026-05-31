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
	public class LineCross2 : Strategy
	{
		private double T3Strat;

		private TillsonT3 TillsonT31;
		private EMA EMA1;
		private EMA EMA2;
		private EMA EMA3;
		private double StopPrice;
		private double TriggerPrice;
		private int TriggerState;
		private MACrossBuilder  MACrossBuilder1;
		private MACD mcd;
		
				
		private double	initialBreakEven	= 0; 		// Default setting for where you set the breakeven
		private double 	previousPrice		= 0;		// previous price used to calculate trailing stop
		private double 	newPrice			= 0;		// Default setting for new price used to calculate trailing stop
		private double	stopPlot			= 0;		// Value used to plot the stop level
		private int 	BarTraded 			= 0; 		// Default setting for Bar number that trade occurs	
		private bool	maxLossHit			= false;
		private int		dateLossHit 		= 0;
		private int		lastTradeOrderNumber = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "LineCross2";
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
				MAShort					= 5;
				MALong					= 8;
				StartTime						= DateTime.Parse("09:50", System.Globalization.CultureInfo.InvariantCulture);
				T3Strat					= 1;
				StopLoss			= 14;
				ProfitTarget			= 14;
				BarsToWait				= 10;
				BreakEvenTrigger					= 8;
				InitialStopDistance					= -10;
				StopPrice					= 0;
				TriggerPrice					= 0;
				TriggerState					= 0;
				TrailProfitTrigger 				= 10;
				TrailStep						= 5;
				UseShorts						= true;
					FastEMA					= 5;
				SlowEMA					= 8;

			}
			else if (State == State.Configure)
			{
				//AddDataSeries("ES JUN23", Data.BarsPeriodType.Minute, 1, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{				
				TillsonT31				= TillsonT3(Close, 8, 0.7);
				EMA1				= EMA(Close, FastEMA);
				EMA2				= EMA(Close, SlowEMA);
				EMA3				= EMA(Close, 5);
				MACrossBuilder1 	= MACrossBuilder(CDMAtype.EMA, PriceType.Close, 5,CDMAtype.EMA,PriceType.Close,8,1,true,true);
				
				TillsonT31.Plots[0].Brush = Brushes.Yellow;
				AddChartIndicator(TillsonT31);		
				mcd = MACD(9,13,5);
				
				SetProfitTarget(@"T3CrossLong", CalculationMode.Ticks, ProfitTarget);
				SetStopLoss(@"T3CrossLong", CalculationMode.Ticks, StopLoss, false);
				SetProfitTarget(@"T3CrossShort", CalculationMode.Ticks, ProfitTarget);
				SetStopLoss(@"T3CrossShort", CalculationMode.Ticks, StopLoss, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;			
			
			
	
			
			
			if (ToTime(Time[0]) < 093000  || ToTime(Time[0]) > 160000)
			{
				//maxLossHit = false;
				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
				return;
				
			}
			
			switch (Position.MarketPosition)
            {
				
				// Resets the stop loss to the original value when all positions are closed
                case MarketPosition.Flat:
                    SetStopLoss(CalculationMode.Ticks, StopLoss);
					previousPrice = 0;
					stopPlot = 0;
					
					
					
			
					if ((((CrossAbove(Close, TillsonT31, 1)) 	
						&& Close[0] > EMA2[0]
						) 		
					
						)					 						
							
								&&  Close[0] > EMA(ProfitTarget)[0]
							
								)
					{
					
					
						EnterLong(2, @"T3CrossLong");
						
						
					}
					
					
					
					//				 // Set 1
					if (((((CrossBelow(Close, TillsonT31, 1)) 
						&& Close[0] < EMA2[0]
						))
				
						)						
						
						)
					{
						//Print("BarsSinceExecution - " + BarsSinceExitExecution().ToString() + "-----Time - " + Time[0] + "-------BarsToWait - " + BarsToWait );
						
						Print("-------maxLossHit - Short - " + maxLossHit + "----------Date: " + Time[0]);						
						EnterShort(2, @"T3CrossShort");
						
					}
                    break;
			    case MarketPosition.Long:
						
					
					
					if ((CrossBelow(Close,TillsonT31,1) ))
					
					{
						ExitLong(Position.Quantity);
					}
					
					
//					
					
//						if ((
//						CrossBelow(Close, EMA(ProfitTarget), 0)
//						//&& CurrentBar != BarTraded && Position.Quantity >0
//							)
//							)
//							ExitLong(Position.Quantity);
								
				
                    break;
				case MarketPosition.Short:
				
					
	
					
						if ((
							CrossAbove(Close, TillsonT31, 1))
							//&& CurrentBar != BarTraded  && Position.Quantity >0
							)
							ExitShort(Position.Quantity);
					
					
							
					break;
				default:
					break;
			}			
			
		
			

		
		}
		
		

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MAShort", Order=1, GroupName="Parameters")]
		public int MAShort
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MALong", Order=2, GroupName="Parameters")]
		public int MALong
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="StartTime", Order=3, GroupName="Parameters")]
		public DateTime StartTime
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTarget", Order=4, GroupName="Parameters")]
		public int ProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stop Loss", Order=5, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }
		
			[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="BarsToWait", Order=6, GroupName="Parameters")]
		public int BarsToWait
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BreakEvenTrigger", Description="Number of ticks above entry the breakeven movement trigger is set", Order=7, GroupName="Parameters")]
		public int BreakEvenTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-999, int.MaxValue)]
		[Display(Name="InitialStopDistance", Description="(use a negative) Number of ticks from entry the stop will initially be placed below", Order=2, GroupName="Parameters")]
		public int InitialStopDistance
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="TrailProfitTrigger", Description="Number of ticks above entry the trail profit movement trigger is set", Order=1, GroupName="Parameters")]
		public int TrailProfitTrigger
		{ get; set; }
		
			[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="TrailStep", Description="Number of ticks above entry the trail profit movement trigger is set", Order=1, GroupName="Parameters")]
		public int TrailStep
		{ get; set; }
		
				[NinjaScriptProperty]		
		[Display(Name="UseShorts", Order=15, GroupName="Parameters")]
		public bool UseShorts
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
