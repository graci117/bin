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
	public class AuZLHMACrossStrat : Strategy
	{
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaDHMA amaDHMA1;
		private NinjaTrader.NinjaScript.Indicators.LizardIndicators.amaDHMA amaDHMA2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "AuZLHMACrossStrat";
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
				Slow					= 8;
				Fast					= 5;
				SlowSmooth					= 5;
				FastSmooth					= 5;
				SlowMultiplier					= 0.3;
				FastMultiplier					= 0.3;
				SlowStdDev					= 5;
				FastStdDev					= 5;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				amaDHMA1				= amaDHMA(Close, Convert.ToInt32(Fast), Convert.ToInt32(FastSmooth), FastMultiplier, Convert.ToInt32(FastStdDev));
				amaDHMA2				= amaDHMA(Close, Convert.ToInt32(Slow), Convert.ToInt32(SlowSmooth), SlowMultiplier, Convert.ToInt32(SlowStdDev));
				amaDHMA1.Plots[0].Brush = Brushes.DarkOrange;
				amaDHMA2.Plots[0].Brush = Brushes.DarkOrange;
				AddChartIndicator(amaDHMA1);
				AddChartIndicator(amaDHMA2);
				SetStopLoss("", CalculationMode.Ticks, 50, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if (CrossAbove(amaDHMA1, amaDHMA2, 1))
			{
				EnterLong(1, @"LongDHMA");
			}
			
			 // Set 2
			if (CrossBelow(amaDHMA1, amaDHMA2, 1))
			{
				EnterShort(1, @"ShortDHMA");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow", Order=1, GroupName="Parameters")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast", Order=2, GroupName="Parameters")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowSmooth", Order=3, GroupName="Parameters")]
		public int SlowSmooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastSmooth", Order=4, GroupName="Parameters")]
		public int FastSmooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="SlowMultiplier", Order=5, GroupName="Parameters")]
		public double SlowMultiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="FastMultiplier", Order=6, GroupName="Parameters")]
		public double FastMultiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowStdDev", Order=7, GroupName="Parameters")]
		public int SlowStdDev
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastStdDev", Order=8, GroupName="Parameters")]
		public int FastStdDev
		{ get; set; }
		#endregion

	}
}
