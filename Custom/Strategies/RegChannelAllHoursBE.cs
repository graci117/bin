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
	public class RegChannelAllHoursBE : Strategy
	{
		private int		breakEvenTicks		= 25;		// Default setting for ticks needed to acheive before stop moves to breakeven		
		private int		plusBreakEven		= 0; 		// Default setting for amount of ticks past breakeven to actually breakeven
		private int		profitTargetTicks	= 40;		// Default setting for how many Ticks away from AvgPrice is profit target
        private int		stopLossTicks		= 64;		// Default setting for stoploss. Ticks away from AvgPrice		
		private int		trailProfitTrigger	= 26;		// 8 Default Setting for trail trigger ie the number of ticks movede after break even befor activating TrailStep
		private int		trailFrequency		= 3;		// 2 Default setting for number of ticks advanced in the trails - take into consideration the barsize as is calculated/advanced next bar
		private int 	BarTraded 			= 0; 		// Default setting for Bar number that trade occurs	
		private int 	firstTarget 		= 11; 		// Default setting for Bar number that trade occurs	
		private int 	trailStopDistance 		= 40; 		// Default setting for Bar number that trade occurs	
		private ADX ADX1;
		
		
		
		private bool	showLines			= true;		// Turn on/off the profit targett, stoploss and trailing stop plots  // new for NT8
		
		private double	initialBreakEven	= 0; 		// Default setting for where you set the breakeven
		private double 	previousPrice		= 0;		// previous price used to calculate trailing stop
		private double 	newPrice			= 0;		// Default setting for new price used to calculate trailing stop
		private double	stopPlot			= 0;		// Value used to plot the stop level
		private double 	stopPrice			= 0;
		
		 private RegressionChannelExtended RegressionChannelExtended1;
		

		// 7/8/2020 - Changed from Calculate.OnBarClose to Calculate.OnPriceChange for correct stop placement
		// 7/8/2020 - Relocated entry logic to occur after Market position sequencing for "Best Practices"
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"ProfitTargetTrailingStop Version 1.01b. StopLoss, Trailing Stop and ProfitTarget With Controls. By Chris Long. alcamie@gmail.com";
				Name								= "RegChannelAllHoursBE";
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
				BarsRequiredToTrade					= 40;
				
				RegChanPeriod = 40;
				Width = 3.5;
				//TrailStop = 67;
				//ProfitTarget = 40;
				Contracts = 1;
				//FirstProfitTarget					= 11;
				ADXPeriod						= 6;
				ADXThreshold					= 21;

				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Hash, "ProfitTarget");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "StopLoss");

			}
			else if (State == State.Configure)
			{
				 RegressionChannelExtended1 = RegressionChannelExtended(Close, Convert.ToInt32(RegChanPeriod), Width);
				SetStopLoss(CalculationMode.Ticks, stopLossTicks);	
				ADX1									= ADX(Close, Convert.ToInt32(ADXPeriod));
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
					SetProfitTarget("Long1", CalculationMode.Ticks, profitTargetTicks);
					SetProfitTarget("Short1", CalculationMode.Ticks, profitTargetTicks);
					previousPrice = 0;
					stopPlot = 0;
                    break;
				
					   
                case MarketPosition.Long:
						
					if (previousPrice == 0)//breakeven has not hit
					{
						stopPlot = Position.AveragePrice - stopLossTicks * TickSize;  // initial stop plot level
					}
					
					// Once the price is greater than entry price+ breakEvenTicks ticks, set stop loss to plusBreakeven ticks
					
                    if (Close[0] > Position.AveragePrice + breakEvenTicks * TickSize  && previousPrice == 0 )
                    {

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
			
			
			
			if (IsFirstTickOfBar &&  ( Position.MarketPosition == MarketPosition.Flat  && CurrentBar > BarTraded) )
			{
				
				if ( Position.MarketPosition != MarketPosition.Short)
				{
				
					 if (
					     // Condition group 1
						 (
					     ((RegressionChannelExtended1.Middle[1] > RegressionChannelExtended1.Middle[2])
					      && (RegressionChannelExtended1.Middle[2] <= RegressionChannelExtended1.Middle[3]))
					      // Condition group 2
					      || ((RegressionChannelExtended1.Middle[0] > RegressionChannelExtended1.Middle[1])
					      && (Low[0] > Low[2])
					      && (Low[2] <= RegressionChannelExtended1.Lower[2]))
					      // Condition group 3
					      || (Low[0] > RegressionChannelExtended1.Lower[2]))
						  && (ADX1[0] > ADX1[2])
				 			&& (ADX1[0] > ADXThreshold)
						 )
					 {
						 FillLongEntry1();
						 newPrice = (Close[1] + (trailProfitTrigger * TickSize)) ;
					 }
				}
												
				if ( Position.MarketPosition != MarketPosition.Long)
				{		
					if (
					    // Condition group 1
						(
					    ((RegressionChannelExtended1.Middle[1] < RegressionChannelExtended1.Middle[2])
					     && (RegressionChannelExtended1.Middle[2] >= RegressionChannelExtended1.Middle[3]))
					     // Condition group 2
					     || ((RegressionChannelExtended1.Middle[0] < RegressionChannelExtended1.Middle[1])
					     && (High[0] < High[2])
					     && (High[2] >= RegressionChannelExtended1.Upper[2]))
					     // Condition group 3
					     || (High[0] < RegressionChannelExtended1.Upper[2]))
						 && (ADX1[0] > ADX1[2])
				 		&& (ADX1[0] > ADXThreshold)
						)
						{
							FillShortEntry1();
							newPrice = (Close[1] + (trailProfitTrigger * TickSize)) ;
						}
						
					}
			}
		}
		
		private void FillLongEntry1()
		{
			EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"Long1");
			//EnterLongLimit(Close[1] - (0*TickSize),"Long2");
			//EnterLong("Long1");
			//EnterLong("Long2");
			BarTraded = CurrentBar;  // save the current bar so only one entry per bar
		}
			
		private void FillShortEntry1()
		{
			EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(0), @"Short1");
			//EnterShortLimit(Close[1] + (0*TickSize),"Short2");
			//EnterShort("Short1");
			//EnterShort("Short2");
			BarTraded = CurrentBar;  // save the current bar so only one entry per bar
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
        [Display(Name = "RegChanPeriod", Order = 1, GroupName = "Parameters")]
        public int RegChanPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Width", Order = 2, GroupName = "Parameters")]
        public double Width
        { get; set; }
       

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Contracts", Order = 5, GroupName = "Parameters")]
        public int Contracts
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
