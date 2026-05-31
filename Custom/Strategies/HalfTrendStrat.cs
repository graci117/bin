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
	public class HalfTrendStrat : Strategy
	{
		private int ASIsma;

		private NinjaTrader.NinjaScript.Indicators.Infinity.HalfTrend HalfTrend1;
		private NinjaTrader.NinjaScript.Indicators.Infinity.AccumulationSwingIndex AccumulationSwingIndex1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "HalfTrendStrat";
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
				TradeSize					= 2;
				StopLoss					= 20;
				ProfitTarget					= 40;
				Amplitude					= 3;
				ASILimit					= 30;
				SmaLength					= 10;
				ASIsma					= 10;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				HalfTrend1				= HalfTrend(Close, Convert.ToInt32(Amplitude), 2, 100, true, true, 10);
				AccumulationSwingIndex1				= AccumulationSwingIndex(Close, Convert.ToInt32(SmaLength), ASILimit, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;

			 // Set 1
			if (HalfTrend1.Trend[0] == 0)
			{
				EnterLong(Convert.ToInt32(TradeSize), @"Long");
			}
			
			 // Set 2
			if (CrossBelow(AccumulationSwingIndex1.ASI, AccumulationSwingIndex1.SMAASI, 1))
			{
				ExitLong(Convert.ToInt32(TradeSize), @"ExitLong", @"Long");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TradeSize", Order=1, GroupName="Parameters")]
		public int TradeSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopLoss", Order=2, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTarget", Order=3, GroupName="Parameters")]
		public int ProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amplitude", Order=4, GroupName="Parameters")]
		public int Amplitude
		{ get; set; }

		[NinjaScriptProperty]
		[Range(10, int.MaxValue)]
		[Display(Name="ASILimit", Order=5, GroupName="Parameters")]
		public int ASILimit
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmaLength", Order=6, GroupName="Parameters")]
		public int SmaLength
		{ get; set; }
		#endregion

	}
}
