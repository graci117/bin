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
	public class RegChanAllHours2Pred : Strategy
	{
		private bool IsLong;
		private bool IsShort;

		private RegressionChannelExtended RegressionChannelExtended1;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "RegChanAllHours2Pred";
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
				RegChanPeriod					= 40;
				Width					= 3.5;
//				TrailStop					= 63;
//				ProfitTarget					= 40;
//				Contracts					= 1;
				LongSignal					= @"Long";
				ShortSignal					= @"Short";
				IsLong					= false;
				IsShort					= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				RegressionChannelExtended1				= RegressionChannelExtended(Close, Convert.ToInt32(RegChanPeriod), Width);
				RegressionChannelHighLow1				= RegressionChannelHighLow(Close, Convert.ToInt32(RegChanPeriod), Width);
//				SetTrailStop(@"LE", CalculationMode.Ticks, TrailStop, true);
//				SetTrailStop(@"SE", CalculationMode.Ticks, TrailStop, true);
//				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
//				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 3)
				return;

			 // Set 1
			if ( Close[1] > Open[1] && !IsLong &&
				 // Condition group 1
				(((RegressionChannelExtended1.Middle[1] > RegressionChannelExtended1.Middle[2])
				 && (RegressionChannelExtended1.Middle[2] <= RegressionChannelExtended1.Middle[3]))
				 // Condition group 2
				 || ((RegressionChannelExtended1.Middle[0] > RegressionChannelExtended1.Middle[1])
				 && (Low[0] > Low[2])
				 && (Low[2] <= RegressionChannelExtended1.Lower[2]))
				 // Condition group 3
				 || (Low[0] > RegressionChannelHighLow1.Lower[2])))
			{
				
				Draw.ArrowUp(this, Convert.ToString(LongSignal) + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] + (-10 * TickSize)) , Brushes.Lime);
				IsLong 	= true;
				IsShort = false;
			}
			
			 // Set 2
			if ( Close[1] < Open[1] && !IsShort &&
				 // Condition group 1
				(((RegressionChannelExtended1.Middle[1] < RegressionChannelExtended1.Middle[2])
				 && (RegressionChannelExtended1.Middle[2] >= RegressionChannelExtended1.Middle[3]))
				 // Condition group 2
				 || ((RegressionChannelExtended1.Middle[0] < RegressionChannelExtended1.Middle[1])
				 && (High[0] < High[2])
				 && (High[2] >= RegressionChannelExtended1.Upper[2]))
				 // Condition group 3
				 || (High[0] < RegressionChannelHighLow1.Upper[2])))
			{
				Draw.ArrowDown(this, Convert.ToString(ShortSignal) + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (10 * TickSize)) , Brushes.Red);
				IsLong 	= false;
				IsShort = true;
			}
			
			if (IsLong && Close[1] < Open[1])
				IsLong = false;
			
			if (IsShort && Close[1] > Open[1])
				IsShort = false;
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="RegChanPeriod", Order=1, GroupName="Parameters")]
		public int RegChanPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Width", Order=2, GroupName="Parameters")]
		public double Width
		{ get; set; }

//		[NinjaScriptProperty]
//		[Display(Name="TrailStop", Order=3, GroupName="Parameters")]
//		public int TrailStop
//		{ get; set; }

//		[NinjaScriptProperty]
//		[Display(Name="ProfitTarget", Order=4, GroupName="Parameters")]
//		public int ProfitTarget
//		{ get; set; }

//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="Contracts", Order=5, GroupName="Parameters")]
//		public int Contracts
//		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongSignal", Order=6, GroupName="Parameters")]
		public string LongSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortSignal", Order=7, GroupName="Parameters")]
		public string ShortSignal
		{ get; set; }
		#endregion

	}
}
