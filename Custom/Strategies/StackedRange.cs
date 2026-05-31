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
	public class StackedRange : Strategy
	{
		private SMA SMA1;
		private SMA SMA2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "StackedRange";
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
				RangeSize					= 28;
				FastMA					= 5;
				SlowMA					= 14;
				StopLoss					= 40;
				TakeProfit					= 40;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				SMA1				= SMA(Close, Convert.ToInt32(FastMA));
				SMA2				= SMA(Close, Convert.ToInt32(SlowMA));
				SetStopLoss(@"LongRange", CalculationMode.Ticks, StopLoss, false);
				SetStopLoss(@"ShortRange", CalculationMode.Ticks, StopLoss, false);
				SetProfitTarget(@"LongRange1", CalculationMode.Ticks, TakeProfit);
				SetProfitTarget(@"ShortRange1", CalculationMode.Ticks, TakeProfit);
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
			
			if (ToTime(Time[0]) >= 155900)
			{
				if (Position.MarketPosition == MarketPosition.Long)
				ExitLong();
				if (Position.MarketPosition == MarketPosition.Short)
				ExitShort();
			}

			 // Set 1
			if ((Close[0] > Open[0])
				 && (Close[1] > Open[1])
				 && (SMA1[0] > SMA2[0])
				 && (Position.Quantity == 0))
			{
				EnterLong(1, @"LongRange1");
				EnterLong(1, @"LongRange2");
			}
			
			 // Set 2
			if ((Close[0] < Open[0])
				 && (Close[1] < Open[1])
				 && (SMA1[0] < SMA2[0])
				 && (Position.Quantity == 0))
			{
				EnterShort(1, @"ShortRange1");
				EnterShort(1, @"ShortRang2");
			}
			
			if (Position.Quantity > 0)
			{
				if 	 (SMA1[0] < SMA2[0])
				{
					ExitLong();
				}
				if 	 (SMA1[0] > SMA2[0])
				{
					ExitShort();
				}
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="RangeSize", Order=1, GroupName="Parameters")]
		public int RangeSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(3, int.MaxValue)]
		[Display(Name="FastMA", Order=2, GroupName="Parameters")]
		public int FastMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="SlowMA", Order=3, GroupName="Parameters")]
		public int SlowMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(8, int.MaxValue)]
		[Display(Name="StopLoss", Order=4, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(8, int.MaxValue)]
		[Display(Name="TakeProfit", Order=5, GroupName="Parameters")]
		public int TakeProfit
		{ get; set; }
		#endregion

	}
}
