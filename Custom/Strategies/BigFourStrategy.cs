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
	public class BigFourStrategy : Strategy
	{
		private BigFour BigFour1;
		private BigFour BigFour2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BigFourStrategy";
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
				AKTrend1					= 3;
				AKTrend2					= 8;
				Amom_MomentumPeriod					= 10;
				Amom_SignalPeriod					= 8;
				Amom_SmoothingPeriod					= 7;
				Ehlers_Length					= 34;
				Strat_Confirm_Factor					= 4;
				TMO_Length					= 30;
				TMO_CalcLength					= 6;
				TMO_SmoothLength					= 6;
				ZScore_Length					= 20;
				ZScore_ZAvgLength					= 20;
				StopLoss					= 20;
				ProfitTrigger					= 20;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				BigFour1				= BigFour(Close, Convert.ToInt32(AKTrend1), Convert.ToInt32(AKTrend2), Convert.ToInt32(ZScore_Length), Convert.ToInt32(ZScore_ZAvgLength), Convert.ToInt32(Ehlers_Length), Convert.ToInt32(Amom_MomentumPeriod), Convert.ToInt32(Amom_SignalPeriod), false, Convert.ToInt32(Amom_SmoothingPeriod), Convert.ToInt32(TMO_Length), Convert.ToInt32(TMO_CalcLength), Convert.ToInt32(TMO_SmoothLength), Convert.ToInt32(Strat_Confirm_Factor), false, true, false, false);
				BigFour2				= BigFour(Close, Convert.ToInt32(AKTrend1), Convert.ToInt32(AKTrend2), Convert.ToInt32(ZScore_Length), Convert.ToInt32(ZScore_ZAvgLength), Convert.ToInt32(Ehlers_Length), Convert.ToInt32(Amom_MomentumPeriod), Convert.ToInt32(Amom_SignalPeriod), false, Convert.ToInt32(Amom_SmoothingPeriod), Convert.ToInt32(TMO_Length), Convert.ToInt32(TMO_CalcLength), Convert.ToInt32(TMO_SmoothLength), Convert.ToInt32(Strat_Confirm_Factor), false, true, false, false);
				SetStopLoss(@"Long", CalculationMode.Ticks, StopLoss, false);
				SetStopLoss(@"Short", CalculationMode.Ticks, StopLoss, false);
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

			 // Set 1
			if ((BigFour1[0] == 1)
				 && (BigFour1[1] != 1))
			{
				EnterLong(2, @"Long");
			}
			
			 // Set 2
			if ((BigFour1[0] == -1)
				 && (BigFour1[1] != 1))
			{
				EnterShort(2, @"Short");
			}
			
			 // Set 3
			if (
				 // Condition group 1
				((BigFour1[0] == 0)
				 || (BigFour1[0] == -1)))
			{
				ExitLong(2, @"ExitLong", @"Long");
			}
			
			 // Set 4
			if (
				 // Condition group 1
				((BigFour2[0] == 0)
				 || (BigFour1[0] == 1)))
			{
				ExitShort(Convert.ToInt32(DefaultQuantity), @"ExitShort", @"Short");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AKTrend1", Order=1, GroupName="Parameters")]
		public int AKTrend1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AKTrend2", Order=2, GroupName="Parameters")]
		public int AKTrend2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amom_MomentumPeriod", Order=3, GroupName="Parameters")]
		public int Amom_MomentumPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amom_SignalPeriod", Order=4, GroupName="Parameters")]
		public int Amom_SignalPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amom_SmoothingPeriod", Order=5, GroupName="Parameters")]
		public int Amom_SmoothingPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Ehlers_Length", Order=6, GroupName="Parameters")]
		public int Ehlers_Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Strat_Confirm_Factor", Order=7, GroupName="Parameters")]
		public int Strat_Confirm_Factor
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TMO_Length", Order=8, GroupName="Parameters")]
		public int TMO_Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TMO_CalcLength", Order=9, GroupName="Parameters")]
		public int TMO_CalcLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TMO_SmoothLength", Order=10, GroupName="Parameters")]
		public int TMO_SmoothLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ZScore_Length", Order=11, GroupName="Parameters")]
		public int ZScore_Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ZScore_ZAvgLength", Order=12, GroupName="Parameters")]
		public int ZScore_ZAvgLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopLoss", Order=13, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTrigger", Order=14, GroupName="Parameters")]
		public int ProfitTrigger
		{ get; set; }
		#endregion

	}
}
