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
	public class VolatilityBreakoutPattern : Strategy
	{
		#region Variables
			private double trailStop, StopLoss, prevValue;
			private bool justEntered;
		#endregion
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Identify stocks in periods of extremes of low volatility which usually followed by big moves.";
				Name						= "VolatilityBreakoutPattern";
				Calculate					= Calculate.OnBarClose;
				EntriesPerDirection			= 1;
				EntryHandling				= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy		= true;
				ExitOnSessionCloseSeconds			= 30;
				IsFillLimitOnTouch			= false;
				MaximumBarsLookBack			= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution			= OrderFillResolution.Standard;
				Slippage					= 0;
				StartBehavior				= StartBehavior.WaitUntilFlat;
				TimeInForce					= TimeInForce.Gtc;
				TraceOrders					= false;
				RealtimeErrorHandling		= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling			= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade			= 20;
				Period						= 20;
				KCDeviation					= 1.5;
				BBDeviation					= 2;
				MomentumPeriod				= 12;
				BBWidth						= 999;
				StopLossPct					= 0.020;
				TrailingStopPct				= 0.020;
				trailStop = 0;
				StopLoss = 0;
				prevValue 					= 0;
				justEntered 				= false;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
            // Condition set 1
			
			if (VolatilityBreakout(Period, KCDeviation, BBDeviation, MomentumPeriod).Plot0[0] == 2 &&
				BollingerBandWidth(Period, BBDeviation)[0] < BBWidth &&
				Position.MarketPosition == MarketPosition.Flat)
            {
                EnterLong(100, "Long");
				justEntered = true;
            }
			
			if (Position.MarketPosition == MarketPosition.Long && justEntered == true)
			{
				trailStop = Position.AveragePrice - (TrailingStopPct * Position.AveragePrice);
				StopLoss = Position.AveragePrice - (StopLossPct * Position.AveragePrice);
    			prevValue = Position.AveragePrice;
    			justEntered = false;
    			Print(" TRAIL STOP TRACKING: " + trailStop + " symbol " + Instrument.FullName);
			}			
			if (Position.MarketPosition == MarketPosition.Long)
			{
				if (High[0] > Position.AveragePrice && High[0] > prevValue)
				{
					trailStop = trailStop + (High[0] - prevValue);
       				prevValue = High[0];
       				Print(" TRAIL STOP RAISED: " + trailStop + " PrevValue " + prevValue + " symbol " + Instrument.FullName);
				}
				Print(Time[0] + " High " + High[0] + " PrevValue " + prevValue + " symbol " + Instrument.FullName);
			}
			if (Low[0] <= trailStop && Position.MarketPosition == MarketPosition.Long && Position.AveragePrice < trailStop)
			{
				Print(" TRAIL STOP HIT: " + trailStop + " " + Close[0] + " symbol " + Instrument.FullName);
    			// Trailing stop has been hit; do whatever you want here
				ExitLong(100,"Trailing Stop" ,"Long");
   				trailStop = 0;
				StopLoss = 0;
    			prevValue = 0;
			}
			else if (Low[0] <= StopLoss && Position.MarketPosition == MarketPosition.Long && Position.AveragePrice > StopLoss)
			{
				Print(" STOP LOSS HIT: " + StopLoss + " " + Close[0] + " symbol " + Instrument.FullName);
				// Trailing stop has been hit; do whatever you want here
				ExitLong(100,"Stop Loss" ,"Long");
				trailStop = 0;
				StopLoss = 0;
				prevValue = 0;					
			}
			
            // Condition set 2
            if (VolatilityBreakout(Period, KCDeviation, BBDeviation, MomentumPeriod).Plot0[0] == -2 &&
				BollingerBandWidth(Period, BBDeviation)[0] < BBWidth &&
				Position.MarketPosition == MarketPosition.Flat)
            {
                EnterShort(100, "Short");
				justEntered = true;
            }
			if (Position.MarketPosition == MarketPosition.Short && justEntered == true)
			{
				trailStop = Position.AveragePrice + (TrailingStopPct * Position.AveragePrice);
				StopLoss = Position.AveragePrice + (StopLossPct * Position.AveragePrice);
    			prevValue = Position.AveragePrice;
    			justEntered = false;
    			Print(" TRAIL STOP TRACKING: " + trailStop + " symbol " + Instrument.FullName);
			}
			if (Position.MarketPosition == MarketPosition.Short)
			{
				if (Low[0] < Position.AveragePrice && Low[0] < prevValue)
				{
					trailStop = trailStop - (prevValue - Low[0]);
       				prevValue = Low[0];
       				Print(" TRAIL STOP Lowered: " + trailStop + " PrevValue " + prevValue + " symbol " + Instrument.FullName);
				}
				Print(Time[0] + " Low " + Low[0] + " PrevValue " + prevValue + " symbol " + Instrument.FullName);
			}
			if (High[0] >= trailStop && Position.MarketPosition == MarketPosition.Short && Position.AveragePrice > trailStop)
			{
				Print(" TRAIL STOP HIT: " + trailStop + " " + Close[0] + " symbol " + Instrument.FullName);
    			// Trailing stop has been hit; do whatever you want here
				ExitShort(100,"Trailing Stop" ,"Short");
   				trailStop = 0;
				StopLoss = 0;
    			prevValue = 0;
			}
			else if (High[0] >= StopLoss && Position.MarketPosition == MarketPosition.Short && Position.AveragePrice < StopLoss)
			{
				Print(" STOP LOSS HIT: " + StopLoss + " " + Close[0] + " symbol " + Instrument.FullName);
				// Trailing stop has been hit; do whatever you want here
				ExitShort(100,"Stop Loss" ,"Short");
				trailStop = 0;
				StopLoss = 0;
				prevValue = 0;					
			}			
			
		}

		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Description="# of Price Bars used to calculate band/channel", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Range(1, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="KCDeviation", Description="Used to calculate Keltner Channel", Order=2, GroupName="Parameters")]
		public double KCDeviation
		{ get; set; }

		[Range(1, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BBDeviation", Description="Used to calculate Bollinger Band", Order=3, GroupName="Parameters")]
		public double BBDeviation
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="MomentumPeriod", Description="Used to calculate Momentum", Order=4, GroupName="Parameters")]
		public int MomentumPeriod
		{ get; set; }

		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BBWidth", Description="Used to limit results, lower = Low Volatility & 999 to ignore", Order=5, GroupName="Parameters")]
		public double BBWidth
		{ get; set; }

		[Range(0.010, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="StopLossPct", Description="Define stop loss percent", Order=6, GroupName="Parameters")]
		public double StopLossPct
		{ get; set; }
		
		[Range(0.010, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="TrailingStopPct", Description="Define trailing stop percent", Order=7, GroupName="Parameters")]
		public double TrailingStopPct
		{ get; set; }		
		#endregion
	}
}
