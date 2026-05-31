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
	public class WizTrail4 : Strategy
	{
		private double Stop_Price;
		private double Stop_Trigger;
		private double Target_Price;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Wiz Trailing 4";
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
				Target					= 200;
				Stop					= 40;
				Trail_Trigger					= 28;
				Trail_Stop_Distance					= 14;
				Trail_frequency					= 3;
				Stop_Price					= 0;
				Stop_Trigger					= 0;
				Target_Price					= 0;
			}
			else if (State == State.Configure)
			{
				SetProfitTarget("", CalculationMode.Ticks, 10);
				SetStopLoss("", CalculationMode.Ticks, 10, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if(State != State.Realtime ) return ;
			
			 // Set 1
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				Stop_Price = 0;
			}
			
			if (CurrentBars[0] < 2)
				return;

			 // Set 2
			if (( false )   // Long signal condition
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				Target_Price = (Close[0] + (Target * TickSize)) ;
				Stop_Price = (Close[0] - (Stop * TickSize)) ;
				SetProfitTarget("", CalculationMode.Price, Target_Price);
				SetStopLoss("", CalculationMode.Price, Stop_Price, false);
				EnterLong(Convert.ToInt32(DefaultQuantity), "");
				Stop_Trigger = (Close[0] + (Trail_Trigger * TickSize)) ;
			}
			
			 // Set 3
			if (( false )   // Short signal condition
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				Target_Price = (Close[0] - (Target * TickSize)) ;
				Stop_Price = (Close[0] + (Stop * TickSize)) ;
				SetProfitTarget("", CalculationMode.Price, Target_Price);
				SetStopLoss("", CalculationMode.Price, Stop_Price, false);
				EnterShort(Convert.ToInt32(DefaultQuantity), "");
				Stop_Trigger = (Close[0] - (Trail_Trigger * TickSize)) ;
			}
			
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
    	// Print some data to the Output window
    	if (marketDataUpdate.MarketDataType == MarketDataType.Last)
		{
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Stop_Price != 0)
				 && (marketDataUpdate.Price >= Stop_Trigger))
			{
				Stop_Price = (marketDataUpdate.Price - (Trail_Stop_Distance * TickSize)) ;
				Stop_Trigger = (marketDataUpdate.Price + (Trail_frequency * TickSize)) ;
				SetStopLoss("", CalculationMode.Price, Stop_Price, false);
				Print("Setting Stop Longs ="+Stop_Price);
			}
			
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Stop_Price != 0)
				 && (marketDataUpdate.Price <= Stop_Trigger))
			{
				Stop_Price = (marketDataUpdate.Price + (Trail_Stop_Distance * TickSize)) ;
				Stop_Trigger = (marketDataUpdate.Price - (Trail_frequency * TickSize)) ;
				SetStopLoss("", CalculationMode.Price, Stop_Price, false);
				Print("Setting Stop Shorts ="+Stop_Price);
			}
		}
		
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Target", Order=1, GroupName="Parameters")]
		public int Target
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Stop", Order=2, GroupName="Parameters")]
		public int Stop
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Trail_Trigger", Order=3, GroupName="Parameters")]
		public int Trail_Trigger
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trail_Stop_Distance", Order=4, GroupName="Parameters")]
		public int Trail_Stop_Distance
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Trail_frequency", Order=5, GroupName="Parameters")]
		public int Trail_frequency
		{ get; set; }
		#endregion

	}
}
