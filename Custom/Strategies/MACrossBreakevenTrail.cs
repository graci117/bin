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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class MACrossBreakevenTrail : Strategy
	{
		private int		breakEvenTicks		= 13;		// Default setting for ticks needed to acheive before stop moves to breakeven		
		private int		plusBreakEven		= 0; 		// Default setting for amount of ticks past breakeven to actually breakeven
		private int		profitTargetTicks	= 40;		// Default setting for how many Ticks away from AvgPrice is profit target
        private int		stopLossTicks		= 28;		// Default setting for stoploss. Ticks away from AvgPrice		
		private int		trailProfitTrigger	= 24;		// 8 Default Setting for trail trigger ie the number of ticks movede after break even befor activating TrailStep
		private int		trailFrequency		= 3;		// 2 Default setting for number of ticks advanced in the trails - take into consideration the barsize as is calculated/advanced next bar
		private int 	BarTraded 			= 0; 		// Default setting for Bar number that trade occurs	
		private int 	firstTarget 		= 11; 		// Default setting for Bar number that trade occurs	
		private int 	trailStopDistance 		= 22; 		// Default setting for Bar number that trade occurs	
		
		
		
		private bool	showLines			= true;		// Turn on/off the profit targett, stoploss and trailing stop plots  // new for NT8
		
		private double	initialBreakEven	= 0; 		// Default setting for where you set the breakeven
		private double 	previousPrice		= 0;		// previous price used to calculate trailing stop
		private double 	newPrice			= 0;		// Default setting for new price used to calculate trailing stop
		private double	stopPlot			= 0;		// Value used to plot the stop level
		private double 	stopPrice			= 0;
		
		private Series<double> EMAFast;
		private Series<double> EMASlow;
		//private SMA SMA20;
		private Series<double>  MA1;
		private Series<double> SMA20;
		

		// 7/8/2020 - Changed from Calculate.OnBarClose to Calculate.OnPriceChange for correct stop placement
		// 7/8/2020 - Relocated entry logic to occur after Market position sequencing for "Best Practices"
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"ProfitTargetTrailingStop Version 1.01b. StopLoss, Trailing Stop and ProfitTarget With Controls. By Chris Long. alcamie@gmail.com";
				Name								= "MACrossBreakevenTrail";
				Calculate							= Calculate.OnPriceChange;
				EntriesPerDirection					= 1;
				EntryHandling						= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy		= false;
				ExitOnSessionCloseSeconds			= 30;
				IsFillLimitOnTouch					= false;
				MaximumBarsLookBack					= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution					= OrderFillResolution.Standard;
				Slippage							= 0;
				StartBehavior						= StartBehavior.WaitUntilFlat;
				TimeInForce							= TimeInForce.Gtc;
				TraceOrders							= false;
				RealtimeErrorHandling				= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling					= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade					= 20;
				//FirstProfitTarget					= 11;

				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Hash, "ProfitTarget");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "StopLoss");

			}
			else if (State == State.Configure)
			{
				EMAFast 		= GetMA(true);
				EMASlow 		= GetMA(false);
				//SMA20			= SMA(Close,SMALength);
				
				SMA20 		= AuLLMA(SMALength,0).Value;
				SetStopLoss(CalculationMode.Ticks, stopLossTicks);
				SetProfitTarget("Long2", CalculationMode.Ticks, profitTargetTicks);	
				SetProfitTarget("Short2", CalculationMode.Ticks, profitTargetTicks);	
			}
		}
		

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade) return;	
			
			if (ToTime(Time[0]) < 093000  || ToTime(Time[0]) > 160000)
			{
				//maxLossHit = false;
				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
				return;
				
			}

			// keep the below code intact for use with a fixed stop, a break even stop and a profit trailing stop =================			
			switch (Position.MarketPosition)
            {
				// Resets the stop loss to the original value when all positions are closed
                case MarketPosition.Flat:
                    SetStopLoss(CalculationMode.Ticks, stopLossTicks);
					SetProfitTarget("Long1", CalculationMode.Ticks, firstTarget);
					SetProfitTarget("Short1", CalculationMode.Ticks, firstTarget);
					previousPrice = 0;
					stopPlot = 0;
                    break;
				
					   
                case MarketPosition.Long:
						
					if (previousPrice == 0)//breakeven has not hit
					{
						stopPlot = Position.AveragePrice - stopLossTicks * TickSize;  // initial stop plot level
					}
					
					// Once the price is greater than entry price+ breakEvenTicks ticks, set stop loss to plusBreakeven ticks
					
					//this is assuming that the 1st position needs to be out at breakeven price if quantity is more than 1
                    if (Close[0] > Position.AveragePrice + breakEvenTicks * TickSize  && previousPrice == 0 )
                    {
//						if(Position.Quantity > 1)
//						{
//							ExitLong("Long1");
//						}
//						else //I think this should come outside of the if
//						{
							
//						}
						initialBreakEven = Position.AveragePrice + plusBreakEven * TickSize;
	                        SetStopLoss(CalculationMode.Price, initialBreakEven);
							previousPrice = Position.AveragePrice;
							stopPlot = initialBreakEven;
							stopPrice = previousPrice;
						
                    }
					
                   
					// Once at breakeven wait till trailProfitTrigger is reached before advancing stoploss by trailFrequency size step
					else if (previousPrice	!= 0 ////StopLoss is at breakeven or more
 							&& GetCurrentAsk() > newPrice )//newprice means the stop distance has reached
					{
						//Print("previousPrice0 - " + previousPrice);
						stopPrice = newPrice - (trailStopDistance * TickSize); 	// Calculate trail stop adjustment
						
						if (newPrice - (trailStopDistance * TickSize) >initialBreakEven )
						{
						
							SetStopLoss(CalculationMode.Price, stopPrice);			// Readjust stoploss level	
							stopPlot = stopPrice; 		
						}
						else
						{
							SetStopLoss(CalculationMode.Price, initialBreakEven);	
							stopPlot = initialBreakEven; 	
						}
						previousPrice = stopPrice;				 				// save for price adjust on next candle
									 				// save to adjust plot line
						newPrice = (GetCurrentAsk() + (trailFrequency * TickSize));
					//	Print("stopPrice - " + stopPrice);
					//	Print("previousPrice - " + previousPrice);
					//	Print("newPrice - " + newPrice);
						//Print("stopPrice - " + stopPrice);
					}
					
					// Plot the profit/stop lines
					if (showLines)
					{
						ProfitTarget[0] = Position.AveragePrice + profitTargetTicks * TickSize;
						StopLoss[0] 	= stopPlot;
					}
                    break;
					
					
                case MarketPosition.Short:
					
					if (previousPrice == 0) 
					{
						stopPlot = Position.AveragePrice + stopLossTicks * TickSize;  // initial stop plot level
					}
					
                    // Once the price is Less than entry price - breakEvenTicks ticks, set stop loss to breakeven
                    if (Close[0] < Position.AveragePrice - breakEvenTicks * TickSize && previousPrice == 0)
                    {
						initialBreakEven = Position.AveragePrice - plusBreakEven * TickSize;
                        SetStopLoss(CalculationMode.Price, initialBreakEven);
						previousPrice = Position.AveragePrice;
						stopPlot = initialBreakEven;
						stopPrice = previousPrice;
                    }
					// Once at breakeven wait till trailProfitTrigger is reached before advancing stoploss by trailFrequency size step
					else if (previousPrice	!= 0 ////StopLoss is at breakeven
 							&& GetCurrentAsk() < newPrice )
					{
						stopPrice = newPrice + (trailStopDistance * TickSize);
						
						if (newPrice + (trailStopDistance * TickSize) <initialBreakEven )
						{
							SetStopLoss(CalculationMode.Price, stopPrice);
							stopPlot = stopPrice;
						}
						else
						{
							SetStopLoss(CalculationMode.Price, initialBreakEven);
							stopPlot = initialBreakEven;
						}
						previousPrice = stopPrice;
						
						newPrice = (GetCurrentAsk() - (trailFrequency * TickSize));
					}
					
					if (showLines)
					{
						ProfitTarget[0] = Position.AveragePrice - profitTargetTicks * TickSize;
						StopLoss[0] 	= stopPlot;
					}					

                    break;
                default:
                    break;
			}	
			
			// Begin the Entry Logic section *********
	
			/* The idea here is that you would create your own entry logic and replace what is show below
			You will want to make it like:
			
			if (Position.MarketPosition != MarketPosition.Short && Your various conditions for Long entry)
			{
			 FillLongEntry1();  // must call this to enter long
			}
			if (Position.MarketPosition != MarketPosition.Long && Your various conditions for short entry)
			{
			 FillShortEntry1(); // must call this to enter short
			}		
			*/
			if (IsFirstTickOfBar)
			{
				
			}
			
			if (IsFirstTickOfBar &&  ( Position.MarketPosition == MarketPosition.Flat  && CurrentBar > BarTraded) )
			{
				Print("CrossBelowwww(EMAFast,EMASlow,1)---" + CrossBelow(EMAFast,EMASlow,1));
				Print("CrossAboveeee(EMAFast,EMASlow,1)---" + CrossAbove(EMAFast,EMASlow,1));
				if ( Position.MarketPosition != MarketPosition.Short)
				{
					if (SMALength == 1)
					{
						if (CrossAbove(EMAFast,EMASlow,1))
						{
							
							FillLongEntry1();
							newPrice = (Close[1] + (trailProfitTrigger * TickSize)) ;
						}
						
					}
					else
					{
						if (EMAFast[1] > EMASlow[1] && EMAFast[0] > EMASlow[0])
						{
								if (ToDay(Time[0]) > 20240124 &&  ToTime(Time[0]) > 102200)
								{
									Print(ToTime(Time[0]) + "" + "--out----" + CrossBelow(SMA20,EMASlow,1));
								}
							if (CrossBelow(SMA20,EMASlow,1))
							{
								if (ToDay(Time[0]) > 20240124 &&  ToTime(Time[0]) > 102200)
								{
									Print(ToTime(Time[0]) + "" + "--in----" + CrossBelow(SMA20,EMASlow,1));
								}
								FillLongEntry1();
								newPrice = (Close[1] + (trailProfitTrigger * TickSize)) ;
							}
							
						}
						if (Position.MarketPosition == MarketPosition.Flat)
						{
							if (CrossAbove(EMAFast,EMASlow,1))
							{
								if  (SMA20[0] < EMASlow[0])
								{
									FillLongEntry1();
									newPrice = (Close[1] + (trailProfitTrigger * TickSize)) ;
								}
							}
						}

							
					}
				}
				
				if  ( Position.MarketPosition != MarketPosition.Long)
				{
					
					if (SMALength == 1)
					{
						if (CrossBelow(EMAFast,EMASlow,1))
						{
							FillShortEntry1();
							newPrice = (Close[1] - (trailProfitTrigger * TickSize)) ;
						}
					}
					else
					{

						Print("EMASlow1 gt EMAFast1---" + (EMASlow[1] > EMAFast[1]));
						Print("EMASlow0 > EMAFast0---" + (EMASlow[0] > EMAFast[0]));
						Print("CrossAbove(SMA20,EMASlow,1)---" + CrossAbove(SMA20,EMASlow,1));
						Print("CrossBelow(EMAFast,EMASlow,1)---" + CrossBelow(EMAFast,EMASlow,1));
						Print("SMA20 > EMASlow0---" + (SMA20[0] > EMASlow[0]));
						
						if  (EMASlow[1] > EMAFast[1] && EMASlow[0] > EMAFast[0])
						{
							
							if (CrossAbove(SMA20,EMASlow,1))
							{
								FillShortEntry1();
								newPrice = (Close[1] - (trailProfitTrigger * TickSize)) ;
							}
						}
						if (Position.MarketPosition == MarketPosition.Flat)
						{
							if (CrossBelow(EMAFast,EMASlow,1))
							{
								if  (SMA20[0] > EMASlow[0])
								{
									FillShortEntry1();
									newPrice = (Close[1] + (trailProfitTrigger * TickSize)) ;
								}
							}
						}
							
					}
					
				}
				
			}
		
			
          
		}
		
		private void FillLongEntry1()
		{
			EnterLongLimit(Close[1] - (0*TickSize),"Long1");
			EnterLongLimit(Close[1] - (0*TickSize),"Long2");
			//EnterLong("Long1");
			//EnterLong("Long2");
			BarTraded = CurrentBar;  // save the current bar so only one entry per bar
		}
			
		private void FillShortEntry1()
		{
			EnterShortLimit(Close[1] + (0*TickSize),"Short1");
			EnterShortLimit(Close[1] + (0*TickSize),"Short2");
			//EnterShort("Short1");
			//EnterShort("Short2");
			BarTraded = CurrentBar;  // save the current bar so only one entry per bar
		}		
		
		private Series<double> GetMA(bool isFast)
		{
			CDMAtype Ma0Type;
			int MALength ;
			if (isFast)
			{
				Ma0Type = FastMaType;
				MALength = FastEMA;
			}
			else
			{
				Ma0Type = SlowMaType;
				MALength = SlowEMA;
			}
			
			switch (Ma0Type)
				{
					case CDMAtype.DEMA:						
						
						MA1 = DEMA(Close, MALength).Value;
						
						break;
						
					case CDMAtype.EMA:
							MA1 = EMA(Close, MALength).Value;
						
					break;	
						
					case CDMAtype.HMA:
							MA1 = HMA(Close, MALength).Value;
					
					break;	
						
					case CDMAtype.LinReg:
							MA1 = LinReg(Close, MALength).Value;

					break;							
						
					case CDMAtype.SMA:
							MA1 = SMA(Close, MALength).Value;
					
					break;	
						
					case CDMAtype.TEMA:
							MA1 = TEMA(Close, MALength).Value;

					break;	
						
					case CDMAtype.TMA:	
							MA1 = TMA(Close, MALength).Value;
					
					break;	
					
					case CDMAtype.VWMA:
							MA1 = VWMA(Close, MALength).Value;

					break;	
						
					case CDMAtype.WMA:
							MA1 = WMA(Close, MALength).Value;
							
					break;
						
					case CDMAtype.ZLEMA:
							MA1 = ZLEMA(Close, MALength).Value;

					break;												
				}	
				return MA1;
		}

		
		#region Properties
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Profit Target Ticks", Description="Number of ticks away from entry price for the Profit Target order", Order=21, GroupName="RiskParameters")]
		public int ProfitTargetTicks
		{
			get { return profitTargetTicks; }
			set { profitTargetTicks = value; }
		}

		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Stop Loss Ticks", Description="Numbers of ticks away from entry price for the Stop Loss order", Order=23, GroupName="RiskParameters")]
		public int StopLossTicks
		{
			get { return stopLossTicks; }
			set { stopLossTicks = value; }
		}

		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BreakEven Ticks Trigger", Description="Number of ticks in Profit to trigger stop to move to Plus Breakeven ticks level", Order=25, GroupName="RiskParameters")]
		public int BreakEvenTicks
		{
			get {return breakEvenTicks;}
			set {breakEvenTicks = value;}
		}

		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BreakEven Ticks Offset", Description="Number of ticks past breakeven for breakeven stop (can be zero)", Order=27, GroupName="RiskParameters")]
		public int PlusBreakEven
		{
			get { return plusBreakEven; }
			set { plusBreakEven = value; }
		}

		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Trail Profit Trigger", Description="Number of ticks in profit to trigger trail stop action", Order=29, GroupName="RiskParameters")]
		public int TrailProfitTrigger
		{
			get {return trailProfitTrigger;}
			set {trailProfitTrigger = value;}
		}
		
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Trail Step Ticks", Description="Number of ticks to step for each adjustment of trail stop", Order=31, GroupName="RiskParameters")]
		public int TrailFrequency
		{
			get {return trailFrequency;}
			set {trailFrequency = value;}
		}
		
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Trail Stop Distance", Description="Price behind current Price to set Stop", Order=33, GroupName="RiskParameters")]
		public int TrailStopDistance
		{
			get {return trailStopDistance;}
			set {trailStopDistance = value;}
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Show Lines", Description="Plot profit and stop lines on chart", Order = 35, GroupName = "RiskParameters")]
		public bool ShowLines
		{
			get { return showLines; } 
			set { showLines = value; }
		}		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ProfitTarget
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> StopLoss
		{
			get { return Values[1]; }
		}

		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastEMA", Order=1, GroupName="MA")]
		public int FastEMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowEMA", Order=3, GroupName="MA")]
		public int SlowEMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SMALength", Order=5, GroupName="MA")]
		public int SMALength
		{ get; set; }

		
		[NinjaScriptProperty]
		[Display(Name=" Fast MA Type", Description="RSI MA Type", Order=7, GroupName="MA")]
		public CDMAtype FastMaType
        { get; set; }
		
			[NinjaScriptProperty]
		[Display(Name=" Slow MA Type", Description="RSI MA Type", Order=9, GroupName="MA")]
		public CDMAtype SlowMaType
        { get; set; }

		#endregion

	}
}
