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
	public class ReversalStrat : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "ReversalStrat";
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
				TickOffset					= 1;
				PriceOffset					= 0.01;
				PercentOffset					= 0;
				PriceOffsetEntry					= 0.01;
				PercentOffsetEntry					= 0;
				TickOffsetEntry					= 1;
				PriceOffsetStop					= 0.01;
				PercentOffsetStop					= 0;
				TickOffsetStop					= 1;
				PriceOffsetTrail					= 0.01;
				PercentOffsetTrail					= 0;
				TickOffsetTrail					= 1;
				TrailTriggerAmount					= 20;
				BreakevenTriggerArea					= 10;
				DailyProfitLimit					= 1000;
				DailyLosstLimit					= -500;
				NumberOfTradesAllowed					= 5;
				StartTime						= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime						= DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
				PositionSize					= 2;
				ProfitTargetTicks					= 50;
				StopLoss					= true;
				ProfitTarget					= true;
				SetBreakeven					= true;
				SetTrail					= true;
				SystemPrint					= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			
		}

		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="TickOffset", Order=1, GroupName="Parameters")]
		public int TickOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PriceOffset", Order=2, GroupName="Parameters")]
		public double PriceOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PercentOffset", Order=3, GroupName="Parameters")]
		public double PercentOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PriceOffsetEntry", Order=4, GroupName="Parameters")]
		public double PriceOffsetEntry
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PercentOffsetEntry", Order=5, GroupName="Parameters")]
		public double PercentOffsetEntry
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TickOffsetEntry", Order=6, GroupName="Parameters")]
		public int TickOffsetEntry
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PriceOffsetStop", Order=7, GroupName="Parameters")]
		public double PriceOffsetStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PercentOffsetStop", Order=8, GroupName="Parameters")]
		public double PercentOffsetStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TickOffsetStop", Order=9, GroupName="Parameters")]
		public int TickOffsetStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PriceOffsetTrail", Order=10, GroupName="Parameters")]
		public double PriceOffsetTrail
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PercentOffsetTrail", Order=11, GroupName="Parameters")]
		public double PercentOffsetTrail
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TickOffsetTrail", Order=12, GroupName="Parameters")]
		public int TickOffsetTrail
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailTriggerAmount", Order=13, GroupName="Parameters")]
		public int TrailTriggerAmount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BreakevenTriggerArea", Order=14, GroupName="Parameters")]
		public int BreakevenTriggerArea
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="DailyProfitLimit", Order=15, GroupName="Parameters")]
		public double DailyProfitLimit
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="DailyLosstLimit", Order=16, GroupName="Parameters")]
		public double DailyLosstLimit
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="NumberOfTradesAllowed", Order=17, GroupName="Parameters")]
		public int NumberOfTradesAllowed
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="StartTime", Order=18, GroupName="Parameters")]
		public DateTime StartTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="EndTime", Order=19, GroupName="Parameters")]
		public DateTime EndTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PositionSize", Order=20, GroupName="Parameters")]
		public int PositionSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTargetTicks", Order=21, GroupName="Parameters")]
		public int ProfitTargetTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="StopLoss", Order=22, GroupName="Parameters")]
		public bool StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ProfitTarget", Order=23, GroupName="Parameters")]
		public bool ProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SetBreakeven", Order=24, GroupName="Parameters")]
		public bool SetBreakeven
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SetTrail", Order=25, GroupName="Parameters")]
		public bool SetTrail
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SystemPrint", Order=26, GroupName="Parameters")]
		public bool SystemPrint
		{ get; set; }
		#endregion

	}
}
