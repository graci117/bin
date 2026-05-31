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
	public class BallsDeepPopeMod : Strategy
	{
		private int SETUP;

		private AuSuperTrendU11 AuSuperTrendU111;
		private BluezMACDBB BluezMACDBB1;
		private ninZaSuperTrend4U ninZaSuperTrend4U1;
		private EMA EMA1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BallsDeepPopeMod";
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
				TP					= 6;
				SL					= 40;
				UseEMAfilter					= true;
				EMAperiod					= 35;
				Contracts					= 1;
				SETUP					= 0;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				AuSuperTrendU111				= AuSuperTrendU11(Close, AuSuperTrendU11BaseType.HMA, AuSuperTrendU11OffsetType.Default, AuSuperTrendU11VolaType.True_Range, false, 2, 1, 5);
				BluezMACDBB1				= BluezMACDBB(Close, 7, 24, 12, 12, 0, Brushes.Green, Brushes.Red);
				ninZaSuperTrend4U1				= ninZaSuperTrend4U(Close, ninZaSuperTrend4U_MAType.SMA, 5, 1, 200);
				EMA1				= EMA(Close, Convert.ToInt32(EMAperiod));
				SetProfitTarget("", CalculationMode.Ticks, TP);
				SetStopLoss("", CalculationMode.Ticks, SL, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			 // Set 1
			if (true == true)
			{
				SETUP = 0;
			}
			
			if (CurrentBars[0] < 2)
				return;

			 // Set 2
			if ((AuSuperTrendU111.Trend[0] > 0)
				 && (AuSuperTrendU111.Trend[2] < 0)
				 && (AuSuperTrendU111.Trend[1] > 0)
				 && (BluezMACDBB1.MACDPlot[0] > BluezMACDBB1.Upper[0])
				 && (ninZaSuperTrend4U1.SuperTrend[0] < Open[0]))
			{
				SETUP = 1;
				//EnterLong(Convert.ToInt32(Contracts), @"Long");
				Draw.Text(this, Convert.ToString("Long") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );		
			}
			
			 // Set 3
			if ((AuSuperTrendU111.Trend[0] < 0)
				 && (AuSuperTrendU111.Trend[2] > 0)
				 && (AuSuperTrendU111.Trend[1] < 0)
				 && (BluezMACDBB1.MACDPlot[0] < BluezMACDBB1.Lower[0])
				 && (ninZaSuperTrend4U1.SuperTrend[0] > Open[0]))
			{
				SETUP = -1;
				//EnterShort(Convert.ToInt32(Contracts), @"Short");
				Draw.Text(this, Convert.ToString("Short") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Crimson );
			}
			
			 // Set 4
			if ((SETUP == 1)
				 && (UseEMAfilter == true)
				 && (EMA1[0] > Close[0]))
			{
				SETUP = 0;
			}
			
			 // Set 5
			if ((SETUP == -1)
				 && (UseEMAfilter == true)
				 && (EMA1[0] < Close[0]))
			{
				SETUP = 0;
			}
			
			 // Set 6
			if (SETUP == 1)
			{
				//EnterLong(Convert.ToInt32(DefaultQuantity), "");
				Draw.Text(this, Convert.ToString("Long") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );		
				Draw.VerticalLine(this, @"BallsDeep Vertical line_1 " + Convert.ToString(CurrentBars[0]), 0, Brushes.Lime, DashStyleHelper.Dash, 1);
				//EnterLong(Convert.ToInt32(Contracts), @"Long");
			}
			
			 // Set 7
			if (SETUP == -1)
			{
				//EnterShort(Convert.ToInt32(DefaultQuantity), "");
				Draw.VerticalLine(this, @"BallsDeep Vertical line_1 " + Convert.ToString(CurrentBars[0]), 0, Brushes.Firebrick, DashStyleHelper.Dash, 1);
				//EnterShort(Convert.ToInt32(Contracts), @"Short");
				Draw.Text(this, Convert.ToString("Short") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Crimson );
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TP", Order=1, GroupName="Parameters")]
		public int TP
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SL", Order=2, GroupName="Parameters")]
		public int SL
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseEMAfilter", Order=3, GroupName="Parameters")]
		public bool UseEMAfilter
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EMAperiod", Order=4, GroupName="Parameters")]
		public int EMAperiod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Description="Number of Contracts", Order=5, GroupName="Parameters")]
		public int Contracts
		{ get; set; }
		#endregion

	}
}
