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
	public class RSIMAStrat : Strategy
	{
		private RSI RSI1;
		private EMA EMA1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "RSIMAStrat";
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
				ProfitTarget1					= 40;
				ProfitTarget2					= 100;
				StopLoss					= 40;
				BreakevenTrigger					= 40;
				TrailStep					= 40;
				TrailProfitTrigger					= 20;
				
				RSILength					= 14;
				RSIMALength					= 50;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				RSI1				= RSI(Close, Convert.ToInt32(RSILength), 3);
				EMA1				= EMA(RSI1.Avg, 50);
				SetStopLoss(@"Long", CalculationMode.Ticks, StopLoss, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if (RSI1.Avg[0] > EMA1[0])
			{
				EnterLong(Convert.ToInt32(DefaultQuantity), @"Long");
			}
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(20, int.MaxValue)]
		[Display(Name="ProfitTarget1", Order=1, GroupName="Parameters")]
		public int ProfitTarget1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(40, int.MaxValue)]
		[Display(Name="ProfitTarget2", Order=2, GroupName="Parameters")]
		public int ProfitTarget2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(20, int.MaxValue)]
		[Display(Name="StopLoss", Order=3, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(20, int.MaxValue)]
		[Display(Name="BreakevenTrigger", Order=4, GroupName="Parameters")]
		public int BreakevenTrigger
		{ get; set; }

		[NinjaScriptProperty]
		[Range(20, int.MaxValue)]
		[Display(Name="TrailStep", Order=5, GroupName="Parameters")]
		public int TrailStep
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="TrailProfitTrigger", Order=6, GroupName="Parameters")]
		public int TrailProfitTrigger
		{ get; set; }

		
		[NinjaScriptProperty]
		[Range(7, int.MaxValue)]
		[Display(Name="RSILength", Order=11, GroupName="Parameters")]
		public int RSILength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(40, int.MaxValue)]
		[Display(Name="RSIMALength", Order=12, GroupName="Parameters")]
		public int RSIMALength
		{ get; set; }
		#endregion

	}
}
