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
	public class LinRegBot : Strategy
	{
		private LinReg LinReg1;
		private MACD MACD1;
		private ADX ADX1;
		private ADX ADX2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Trading strategy based on the Linear Regression indicator.";
				Name										= "LinRegBot";
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
				LinRegPeriod					= 3;
				Fast					= 7;
				Slow					= 31;
				Smooth					= 5;
				TrailStop					= 65;
				ProfitTarget					= 130;
				Contracts					= 1;
				ADXPeriod					= 4;
				ADXThreshold					= 75;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				LinReg1				= LinReg(Close, Convert.ToInt32(LinRegPeriod));
				MACD1				= MACD(Close, Convert.ToInt32(Fast), Convert.ToInt32(Slow), Convert.ToInt32(Smooth));
				ADX1				= ADX(Close, Convert.ToInt32(ADXPeriod));
				ADX2				= ADX(Close, 14);
				SetTrailStop(@"LE", CalculationMode.Ticks, TrailStop, true);
				SetTrailStop(@"SE", CalculationMode.Ticks, TrailStop, true);
				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 3)
				return;

			 // Set 1
			if ((LinReg1[1] > LinReg1[2])
				 && (LinReg1[2] <= LinReg1[3])
				 && (LinReg1[0] > LinReg1[1])
				 && (MACD1.Default[0] > MACD1.Avg[0])
				 && (ADX1[0] > ADX1[2])
				 && (ADX2[0] >= ADXThreshold))
			{
				EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"LE");
			}
			
			 // Set 2
			if ((LinReg1[1] < LinReg1[2])
				 && (LinReg1[2] >= LinReg1[3])
				 && (LinReg1[0] < LinReg1[1])
				 && (MACD1.Default[0] < MACD1.Avg[0])
				 && (ADX1[0] > ADX1[2])
				 && (ADX2[0] >= ADXThreshold))
			{
				EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(0), @"SE");
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="LinRegPeriod", Order=1, GroupName="Parameters")]
		public int LinRegPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Fast", Order=2, GroupName="Parameters")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Slow", Order=3, GroupName="Parameters")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Smooth", Order=4, GroupName="Parameters")]
		public int Smooth
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TrailStop", Order=5, GroupName="Parameters")]
		public int TrailStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ProfitTarget", Order=6, GroupName="Parameters")]
		public int ProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Order=7, GroupName="Parameters")]
		public int Contracts
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXPeriod", Order=8, GroupName="Parameters")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXThreshold", Order=9, GroupName="Parameters")]
		public int ADXThreshold
		{ get; set; }
		#endregion

	}
}
