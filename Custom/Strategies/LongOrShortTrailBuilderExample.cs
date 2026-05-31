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
	public class LongOrShortTrailBuilderExample : Strategy
	{
		private double CurrentLongTriggerPrice;
		private double CurrentLongStopPrice;
		private double CurrentShortTriggerPrice;
		private double CurrentShortStopPrice;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "LongOrShortTrailBuilderExample";
				Calculate									= Calculate.OnEachTick;
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
				LongTrailFrequency					= 5;
				LongTrailStopDistance					= -5;
				ShortTrailFrequency					= -5;
				ShortTrailStopDistance					= 5;
				LongProfitTargetTicks					= 20;
				ShortProfitTargetTicks					= -20;
				CurrentLongTriggerPrice					= 0;
				CurrentLongStopPrice					= 0;
				CurrentShortTriggerPrice					= 0;
				CurrentShortStopPrice					= 0;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			 // Set 1
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				CurrentLongStopPrice = 0;
				CurrentShortStopPrice = 0;
			}
			
			if (CurrentBars[0] < 1)
				return;

			 // Set 2
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (State == State.Realtime)
				 && (Close[0] > Open[0]))
			{
				CurrentLongTriggerPrice = (Close[0] + (LongTrailFrequency * TickSize)) ;
				CurrentLongStopPrice = (Close[0] + (LongTrailStopDistance * TickSize)) ;
				EnterLong(Convert.ToInt32(DefaultQuantity), "");
			}
			
			 // Set 3
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (State == State.Realtime)
				 && (Close[0] < Open[0]))
			{
				CurrentShortTriggerPrice = (Close[0] + (ShortTrailFrequency * TickSize)) ;
				CurrentShortStopPrice = (Close[0] + (ShortTrailStopDistance * TickSize)) ;
				EnterShort(Convert.ToInt32(DefaultQuantity), "");
			}
			
			 // Set 4
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] > CurrentLongTriggerPrice))
			{
				CurrentLongTriggerPrice = (Close[0] + (LongTrailFrequency * TickSize)) ;
				CurrentLongStopPrice = (Close[0] + (LongTrailStopDistance * TickSize)) ;
			}
			
			 // Set 5
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] < CurrentShortTriggerPrice))
			{
				CurrentShortTriggerPrice = (Close[0] + (ShortTrailFrequency * TickSize)) ;
				CurrentShortStopPrice = (Close[0] + (ShortTrailStopDistance * TickSize)) ;
			}
			
			 // Set 6
			if (CurrentLongStopPrice != 0)
			{
				ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), CurrentLongStopPrice, "", "");
			}
			
			 // Set 7
			if (CurrentShortStopPrice != 0)
			{
				ExitShortStopMarket(Convert.ToInt32(DefaultQuantity), CurrentShortStopPrice, "", "");
			}
			
			 // Set 8
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] >= (Position.AveragePrice + (LongProfitTargetTicks * TickSize)) ))
			{
				ExitLong(Convert.ToInt32(DefaultQuantity), @"Profit Long", "");
			}
			
			 // Set 9
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] <= (Position.AveragePrice + (ShortProfitTargetTicks * TickSize)) ))
			{
				ExitShort(Convert.ToInt32(DefaultQuantity), @"ProfitShort", "");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongTrailFrequency", Order=1, GroupName="Parameters")]
		public int LongTrailFrequency
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-9999, int.MaxValue)]
		[Display(Name="LongTrailStopDistance", Order=2, GroupName="Parameters")]
		public int LongTrailStopDistance
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-9999, int.MaxValue)]
		[Display(Name="ShortTrailFrequency", Order=3, GroupName="Parameters")]
		public int ShortTrailFrequency
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortTrailStopDistance", Order=4, GroupName="Parameters")]
		public int ShortTrailStopDistance
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongProfitTargetTicks", Order=5, GroupName="Parameters")]
		public int LongProfitTargetTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-9999, int.MaxValue)]
		[Display(Name="ShortProfitTargetTicks", Order=6, GroupName="Parameters")]
		public int ShortProfitTargetTicks
		{ get; set; }
		#endregion

	}
}
