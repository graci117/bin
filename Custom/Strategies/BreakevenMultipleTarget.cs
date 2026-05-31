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
	public class BreakevenMultipleTarget : Strategy
	{
		private double BreakevenTargetPriceStore;
		private bool BreakevenBool;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BreakevenMultipleTarget";
				Calculate									= Calculate.OnPriceChange;
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
				InitialStopLong1					= -30;
				InitialStopShort1					= 30;
				TargetLong1					= 50;
				TargetShort1					= -50;
				InitialStopLong2					= -30;
				InitialStopShort2					= 30;
				TargetLong2					= 75;
				TargetShort2					= -75;
				InitialStopLong3					= -30;
				InitialStopShort3					= 30;
				TargetLong3					= 100;
				TargetShort3					= -100;
				QTY1					= 1;
				QTY2					= 1;
				QTY3					= 1;
				BreakevenDistanceLong					= 50;
				BreakevenDistanceShort					= -50;
				BreakevenTickOffsetLong					= 0;
				BreakevenTickOffsetShort					= 0;
				BreakevenTargetPriceStore					= 1;
				BreakevenBool					= false;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (State == State.Realtime)
				 && (Close[0] >= High[1]))
			{
				EnterLong(Convert.ToInt32(QTY1), @"Entry1");
				EnterLong(Convert.ToInt32(QTY2), @"Entry2");
				EnterLong(Convert.ToInt32(QTY3), @"Entry3");
				BreakevenBool = false;
			}
			
			 // Set 2
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (BreakevenBool == false))
			{
				ExitLongStopMarket(Convert.ToInt32(QTY2), (Position.AveragePrice + (InitialStopLong2 * TickSize)) , @"Stop2", @"Entry2");
				ExitLongStopMarket(Convert.ToInt32(QTY1), (Position.AveragePrice + (InitialStopLong1 * TickSize)) , @"Stop1", @"Entry1");
				ExitLongStopMarket(Convert.ToInt32(QTY3), (Position.AveragePrice + (InitialStopLong3 * TickSize)) , @"Stop3", @"Entry3");
				ExitLongLimit(Convert.ToInt32(QTY1), (Position.AveragePrice + (TargetLong1 * TickSize)) , @"Target1", @"Entry1");
				ExitLongLimit(Convert.ToInt32(QTY3), (Position.AveragePrice + (TargetLong3 * TickSize)) , @"Target3", @"Entry3");
				ExitLongLimit(Convert.ToInt32(QTY2), (Position.AveragePrice + (TargetLong2 * TickSize)) , @"Target2", @"Entry2");
				BreakevenTargetPriceStore = (Position.AveragePrice + (BreakevenDistanceLong * TickSize)) ;
			}
			
			 // Set 3
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] >= BreakevenTargetPriceStore))
			{
				BreakevenBool = true;
			}
			
			 // Set 4
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (BreakevenBool == true))
			{
				ExitLongStopMarket(Convert.ToInt32(QTY2), (Position.AveragePrice + (BreakevenTickOffsetLong * TickSize)) , @"Stop2", @"Entry2");
				ExitLongStopMarket(Convert.ToInt32(QTY3), (Position.AveragePrice + (BreakevenTickOffsetLong * TickSize)) , @"Stop3", @"Entry3");
				ExitLongStopMarket(Convert.ToInt32(QTY1), (Position.AveragePrice + (BreakevenTickOffsetLong * TickSize)) , @"Stop1", @"Entry1");
				ExitLongLimit(Convert.ToInt32(QTY1), (Position.AveragePrice + (TargetLong1 * TickSize)) , @"Target1", @"Entry1");
				ExitLongLimit(Convert.ToInt32(QTY2), (Position.AveragePrice + (TargetLong2 * TickSize)) , @"Target2", @"Entry2");
				ExitLongLimit(Convert.ToInt32(QTY3), (Position.AveragePrice + (TargetLong3 * TickSize)) , @"Target3", @"Entry3");
			}
			
			 // Set 5
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (State == State.Realtime)
				 && (Close[0] <= Low[1]))
			{
				EnterShort(Convert.ToInt32(QTY2), @"Entry2");
				EnterShort(Convert.ToInt32(QTY3), @"Entry3");
				EnterShort(Convert.ToInt32(QTY1), @"Entry1");
				BreakevenBool = false;
			}
			
			 // Set 6
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (BreakevenBool == false))
			{
				ExitShortStopMarket(Convert.ToInt32(QTY2), (Position.AveragePrice + (InitialStopShort2 * TickSize)) , @"Stop2", @"Entry2");
				ExitShortStopMarket(Convert.ToInt32(QTY3), (Position.AveragePrice + (InitialStopShort3 * TickSize)) , @"Stop3", @"Entry3");
				ExitShortStopMarket(Convert.ToInt32(QTY1), (Position.AveragePrice + (InitialStopShort1 * TickSize)) , @"Stop1", @"Entry1");
				ExitShortLimit(Convert.ToInt32(QTY2), (Position.AveragePrice + (TargetShort2 * TickSize)) , @"Target2", @"Entry2");
				ExitShortLimit(Convert.ToInt32(QTY3), (Position.AveragePrice + (TargetShort3 * TickSize)) , @"Target3", @"Entry3");
				ExitShortLimit(Convert.ToInt32(QTY1), (Position.AveragePrice + (TargetShort1 * TickSize)) , @"Target1", @"Entry1");
				BreakevenTargetPriceStore = (Position.AveragePrice + (BreakevenDistanceShort * TickSize)) ;
			}
			
			 // Set 7
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] <= BreakevenTargetPriceStore))
			{
				BreakevenBool = true;
			}
			
			 // Set 8
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (BreakevenBool == true))
			{
				ExitShortStopMarket(Convert.ToInt32(QTY3), (Position.AveragePrice + (BreakevenTickOffsetShort * TickSize)) , @"Stop3", @"Entry3");
				ExitShortStopMarket(Convert.ToInt32(QTY1), (Position.AveragePrice + (BreakevenTickOffsetShort * TickSize)) , @"Stop1", @"Entry1");
				ExitShortStopMarket(Convert.ToInt32(QTY2), (Position.AveragePrice + (BreakevenTickOffsetShort * TickSize)) , @"Stop2", @"Entry2");
				ExitShortLimit(Convert.ToInt32(QTY2), (Position.AveragePrice + (TargetShort2 * TickSize)) , @"Target2", @"Entry2");
				ExitShortLimit(Convert.ToInt32(QTY1), (Position.AveragePrice + (TargetShort1 * TickSize)) , @"Target1", @"Entry1");
				ExitShortLimit(Convert.ToInt32(QTY3), (Position.AveragePrice + (TargetShort3 * TickSize)) , @"Target3", @"Entry3");
			}
			
			 // Set 9
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				BreakevenBool = false;
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="InitialStopLong1", Order=1, GroupName="Parameters")]
		public int InitialStopLong1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="InitialStopShort1", Order=2, GroupName="Parameters")]
		public int InitialStopShort1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TargetLong1", Order=3, GroupName="Parameters")]
		public int TargetLong1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TargetShort1", Order=4, GroupName="Parameters")]
		public int TargetShort1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="InitialStopLong2", Order=5, GroupName="Parameters")]
		public int InitialStopLong2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="InitialStopShort2", Order=6, GroupName="Parameters")]
		public int InitialStopShort2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TargetLong2", Order=7, GroupName="Parameters")]
		public int TargetLong2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TargetShort2", Order=8, GroupName="Parameters")]
		public int TargetShort2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="InitialStopLong3", Order=9, GroupName="Parameters")]
		public int InitialStopLong3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="InitialStopShort3", Order=10, GroupName="Parameters")]
		public int InitialStopShort3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TargetLong3", Order=11, GroupName="Parameters")]
		public int TargetLong3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TargetShort3", Order=12, GroupName="Parameters")]
		public int TargetShort3
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="QTY1", Order=13, GroupName="Parameters")]
		public int QTY1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="QTY2", Order=14, GroupName="Parameters")]
		public int QTY2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="QTY3", Order=15, GroupName="Parameters")]
		public int QTY3
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BreakevenDistanceLong", Order=16, GroupName="Parameters")]
		public int BreakevenDistanceLong
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BreakevenDistanceShort", Order=17, GroupName="Parameters")]
		public int BreakevenDistanceShort
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BreakevenTickOffsetLong", Order=18, GroupName="Parameters")]
		public int BreakevenTickOffsetLong
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BreakevenTickOffsetShort", Order=19, GroupName="Parameters")]
		public int BreakevenTickOffsetShort
		{ get; set; }
		#endregion

	}
}
