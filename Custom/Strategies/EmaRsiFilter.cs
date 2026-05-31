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
	public class EmaRsiFilter : Strategy
	{
		private bool LongDirection;
		private bool ShortDirection;

		private RSI RSI1;
		private EMA EMA1;
		private EMA EMA2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "EmaRsiFilter";
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
				EmaPeriod					= 89;
				RsiPeriod					= 8;
				RsiSmooth					= 3;
				LongFilterOn					= @"LFilterOn";
				LongFilterOff					= @"LFilterOff";
				ShortFilterOn					= @"SFilterOn";
				ShortFilterOff					= @"SFilterOff";
				LongDirection					= false;
				ShortDirection					= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				RSI1				= RSI(Close, Convert.ToInt32(RsiPeriod), Convert.ToInt32(RsiSmooth));
				EMA1				= EMA(RSI1.Avg, 14);
				EMA2				= EMA(RSI1.Avg, Convert.ToInt32(EmaPeriod));
				RSI1.Plots[0].Brush = Brushes.DodgerBlue;
				RSI1.Plots[1].Brush = Brushes.Goldenrod;
				AddChartIndicator(RSI1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if ((RSI1.Default[0] > EMA1[0])
				 && (LongDirection == false))
			{
				Draw.Text(this, Convert.ToString(LongFilterOn) + " " + Convert.ToString(CurrentBars[0]), @"LongON", 0, (Close[0] + (-25 * TickSize)) );
				LongDirection = true;
			}
			
			 // Set 2
			if (
				 // Condition group 1
				(CrossBelow(RSI1.Default, EMA2, 1))
				 && (LongDirection == true))
			{
				Draw.Text(this, Convert.ToString(LongFilterOff) + " " + Convert.ToString(CurrentBars[0]), @"LongOFF", 0, (Close[0] + (25 * TickSize)) );
				LongDirection = false;
			}
			
			 // Set 3
			if ((RSI1.Default[0] < EMA2[0])
				 && (ShortDirection == false))
			{
				Draw.Text(this, Convert.ToString(ShortFilterOn) + " " + Convert.ToString(CurrentBars[0]), @"ShortON", 0, (Close[0] + (25 * TickSize)) );
				ShortDirection = true;
			}
			
			 // Set 4
			if (
				 // Condition group 1
				(CrossAbove(RSI1.Default, EMA2, 1))
				 && (ShortDirection == true))
			{
				Draw.Text(this, Convert.ToString(ShortFilterOff) + " " + Convert.ToString(CurrentBars[0]), @"ShortOFF", 0, (Close[0] + (-25 * TickSize)) );
				ShortDirection = false;
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EmaPeriod", Order=1, GroupName="Parameters")]
		public int EmaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RsiPeriod", Order=2, GroupName="Parameters")]
		public int RsiPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RsiSmooth", Order=3, GroupName="Parameters")]
		public int RsiSmooth
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongFilterOn", Order=4, GroupName="Parameters")]
		public string LongFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongFilterOff", Order=5, GroupName="Parameters")]
		public string LongFilterOff
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOn", Order=6, GroupName="Parameters")]
		public string ShortFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOff", Order=7, GroupName="Parameters")]
		public string ShortFilterOff
		{ get; set; }
		#endregion

	}
}
