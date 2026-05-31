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
	public class BallsDeepStrat : Strategy
	{
		private bool IsLong;
		private bool IsShort;

		private BluezMACDBB BluezMACDBB1;
		private AuSuperTrendU11 AuSuperTrendU111;
		private ninZaSuperTrend4U ninZaSuperTrend4U1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BallsDeepStrat";
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
				MACDBBFast					= 7;
				MACDBBSlow					= 24;
				MACDBBSmooth					= 12;
				MACDBBBandPeriod					= 12;
				AUSTSmoothingMAType					= 1;
				AUSTOffsetType					= 1;
				AUSTBaselinePeriod					= 2;
				AUSTOffsetMultiplier					= 1;
				AUSTOffsetPeriod					= 5;
				NinZAMAType					= 1;
				NinZaMAPeriod					= 5;
				NinZaOffsetMultiplier					= 1;
				NinZaATRPeriod					= 200;
				IsLong					= false;
				IsShort					= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				
				BluezMACDBB1				= BluezMACDBB(Close, Convert.ToInt32(MACDBBFast), Convert.ToInt32(MACDBBSlow), Convert.ToInt32(MACDBBSmooth), Convert.ToInt32(MACDBBBandPeriod), 0, Brushes.Green, Brushes.Red);
				AuSuperTrendU111				= AuSuperTrendU11(Close, AuSuperTrendU11BaseType.HMA, AuSuperTrendU11OffsetType.Default, AuSuperTrendU11VolaType.True_Range, false, Convert.ToInt32(AUSTBaselinePeriod), AUSTOffsetMultiplier, Convert.ToInt32(AUSTOffsetPeriod));
				ninZaSuperTrend4U1				= ninZaSuperTrend4U(Close, ninZaSuperTrend4U_MAType.EMA, Convert.ToInt32(NinZaMAPeriod), Convert.ToInt32(NinZaOffsetMultiplier), Convert.ToInt32(NinZaATRPeriod));
				AuSuperTrendU111.Plots[0].Brush = Brushes.Gray;
				AuSuperTrendU111.Plots[1].Brush = Brushes.Gray;
				AuSuperTrendU111.Plots[2].Brush = Brushes.Transparent;
				ninZaSuperTrend4U1.Plots[0].Brush = Brushes.Orange;
				ninZaSuperTrend4U1.Plots[1].Brush = Brushes.Fuchsia;
				AddChartIndicator(AuSuperTrendU111);
				AddChartIndicator(ninZaSuperTrend4U1);
				AddChartIndicator(BluezMACDBB1);
				
				ninZaSuperTrend4U1.InstructionEnabled = false;
				ninZaSuperTrend4U1.IsAutoScale = false;
				ninZaSuperTrend4U1.LogoEnabled = false;
				
				AuSuperTrendU111.IsAutoScale = false;
				
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 0)
				return;

			 // Set 1
			if (
				 // Long
				((BluezMACDBB1.MACDPlot[0] > BluezMACDBB1.Average[0])
				 && (AuSuperTrendU111.Trend[0] > 0)
				 && (ninZaSuperTrend4U1.Signal_Trend[0] > 0)
				 && (IsLong == false))
				 && Math.Abs(BluezMACDBB1.Upper[0] - BluezMACDBB1.Lower[0]) > 1.75
				)
			{
				Draw.Text(this, Convert.ToString("Long") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );			
				IsLong = true;
			}
			
			 // Set 2
			if (
				 // ExitLong
				((BluezMACDBB1.MACDPlot[0] < BluezMACDBB1.Average[0])
				 || (AuSuperTrendU111.Trend[0] <= 0)
				 || (ninZaSuperTrend4U1.Signal_Trend[0] <= 0))
				 && (IsLong == true))
			{
				Draw.Text(this, Convert.ToString("LongExit") + Convert.ToString(CurrentBars[0]), "LExit" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Crimson );
				IsLong = false;
				IsShort = false;
			}
			
			 // Set 3
			if (
				 // Short
				((BluezMACDBB1.MACDPlot[0] < BluezMACDBB1.Average[0])
				 && (AuSuperTrendU111.Trend[0] < 0)
				 && (ninZaSuperTrend4U1.Signal_Trend[0] < 0)
				 && (IsShort == false))
				 && Math.Abs(BluezMACDBB1.Upper[0] - BluezMACDBB1.Lower[0]) > 1.75
				)				
			{
				Draw.Text(this, Convert.ToString("Short") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );
				IsShort = true;
			}
			
			 // Set 4
			if (
				 // ShortExit
				((BluezMACDBB1.MACDPlot[0] >= BluezMACDBB1.Average[0])
				 || (AuSuperTrendU111.Trend[0] >= 0)
				 || (ninZaSuperTrend4U1.Signal_Trend[0] >= 0))
				 && (IsShort == true))
			{
				Draw.Text(this, Convert.ToString("ShortExit") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "SExit", 0, (Low[0] + (-12 * TickSize)), Brushes.DarkGreen );			
				IsShort = false;
				IsLong = false;
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACDBBFast", Order=1, GroupName="Parameters")]
		public int MACDBBFast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACDBBSlow", Order=2, GroupName="Parameters")]
		public int MACDBBSlow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACDBBSmooth", Order=3, GroupName="Parameters")]
		public int MACDBBSmooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACDBBBandPeriod", Order=4, GroupName="Parameters")]
		public int MACDBBBandPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AUSTSmoothingMAType", Order=5, GroupName="Parameters")]
		public int AUSTSmoothingMAType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AUSTOffsetType", Order=6, GroupName="Parameters")]
		public int AUSTOffsetType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AUSTBaselinePeriod", Order=7, GroupName="Parameters")]
		public int AUSTBaselinePeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AUSTOffsetMultiplier", Order=8, GroupName="Parameters")]
		public int AUSTOffsetMultiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AUSTOffsetPeriod", Order=9, GroupName="Parameters")]
		public int AUSTOffsetPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NinZAMAType", Order=10, GroupName="Parameters")]
		public int NinZAMAType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NinZaMAPeriod", Order=11, GroupName="Parameters")]
		public int NinZaMAPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NinZaOffsetMultiplier", Order=12, GroupName="Parameters")]
		public int NinZaOffsetMultiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NinZaATRPeriod", Order=13, GroupName="Parameters")]
		public int NinZaATRPeriod
		{ get; set; }
		#endregion

	}
}
