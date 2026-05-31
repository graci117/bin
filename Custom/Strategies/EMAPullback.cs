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
	public class EMAPullback : Strategy
	{
		private bool IsLong;
		private bool IsShort;

		private T3TrendFilter T3TrendFilter1;
		private JurbolMultiMAColorSlope JurbolMultiMAColorSlope1;
		private EMA EMA1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "EMAPullback";
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
				EMALength					= 14;
				T3VolumeFactor					= 0.7;
				Period1					= 10;
				Period2					= 11;
				Period3					= 14;
				Period4					= 17;
				Period5					= 28;
				IsLong					= false;
				IsShort					= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				T3TrendFilter1				= T3TrendFilter(Close, T3VolumeFactor, Convert.ToInt32(Period1), Convert.ToInt32(Period2), Convert.ToInt32(Period3), Convert.ToInt32(Period4), Convert.ToInt32(Period5), false);
				JurbolMultiMAColorSlope1				= JurbolMultiMAColorSlope(JTSUniversalMovingAverage.EMA,14);
				EMA1				= EMA(Close, 14);
				T3TrendFilter1.Plots[0].Brush = Brushes.Green;
				T3TrendFilter1.Plots[1].Brush = Brushes.Red;
				AddChartIndicator(T3TrendFilter1);
				AddChartIndicator(JurbolMultiMAColorSlope1);
				SetProfitTarget("", CalculationMode.Ticks, 30);
				SetStopLoss("", CalculationMode.Ticks, 44, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			
			Print("T3TrendFilter1.Values[0][0] ---- " + T3TrendFilter1.Values[0][0] + "-----" + Time[0]);
			
			Print("T3TrendFilter1.Values[1][0] ---- " + T3TrendFilter1.Values[1][0] + "-----" + Time[0]);
			
			Print("JurbolMultiMAColorSlope1[1] ---- " + JurbolMultiMAColorSlope1[1] + "-----" + Time[0]);
			
			Print("(Low[0] - 1) ---- " + (Low[0] - 1)  + "-----" + Time[0]);
			
			Print("(EMA1[0]) ---- " + EMA1[0]  + "-----" + Time[0]);
			
			Print("(Close[0]) ---- " + Close[0] + "-----" + Time[0]);
			
			
			Print("(IsLong) ---- " + IsLong  + "-----" + Time[0]);
			
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				IsLong = false;
				IsShort = false;
			}
			
			 // Set 1
			if ((T3TrendFilter1.Values[0][0] > 0)
				 && (T3TrendFilter1.Values[1][0] == 0)
				 && (Close[1] > EMA1[1])
				 && ((Low[0] - 1)  <= EMA1[0])
				 && ((Close[0] + (3*TickSize)) > EMA1[0])
				 && (IsLong == false)
			     && IsRising(JurbolMultiMAColorSlope1)
				)
				 //&& (JurbolMultiMAColorSlope1[0].is
			{
				Print("(IsLong) --Test-- " + IsLong  + "-----" + Time[0]);
				EnterLongLimit(Convert.ToInt32(DefaultQuantity), (Close[0] -  (6*TickSize)) , @"PBLong");
				IsLong = true;
				IsShort = false;
				
			}
			
			 // Set 2
			if ((T3TrendFilter1.Values[1][0] < 0)
				 && (T3TrendFilter1.Values[0][0] == 0)
				 && (Close[1] < EMA1[1])
				 && ((High[0] + 1)  >= EMA1[0])
				 && ((Close[0] - (3*TickSize)) < EMA1[0])
				 && (IsShort == false)
				&& IsFalling(JurbolMultiMAColorSlope1)
				)
			{
				EnterShortLimit(Convert.ToInt32(DefaultQuantity), (Close[0] + (6*TickSize)) , "PBShort");
				IsLong = false;
				IsShort = true;
			}
			
			
			if (IsLong &&
				(IsFalling(JurbolMultiMAColorSlope1) ||
				(Close[0]  < EMA1[0]) )
				)
			{
				ExitLong();
			}
			
			if (IsShort &&
				(IsRising(JurbolMultiMAColorSlope1) ||
				(Close[0]  > EMA1[0]) )
				)
			{
				ExitShort();
				IsShort = false;
				IsLong = false;
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EMALength", Order=1, GroupName="Parameters")]
		public int EMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="T3VolumeFactor", Order=2, GroupName="Parameters")]
		public double T3VolumeFactor
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period1", Order=3, GroupName="Parameters")]
		public int Period1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period2", Order=4, GroupName="Parameters")]
		public int Period2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period3", Order=5, GroupName="Parameters")]
		public int Period3
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period4", Order=6, GroupName="Parameters")]
		public int Period4
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period5", Order=7, GroupName="Parameters")]
		public int Period5
		{ get; set; }
		#endregion

	}
}
