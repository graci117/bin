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
	public class LinePullback : Strategy
	{
		private bool IsLong;
		private bool IsShort;

		private EMA EMA1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "LinePullback";
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
				Length					= 14;
				UsePullback					= true;
				UseCrossover					= false;
				DrawOnlyArrows					= true;
				IsLong					= false;
				IsShort					= false;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				EMA1				= EMA(Close, Convert.ToInt32(Length));
				EMA1.Plots[0].Brush = Brushes.Goldenrod;
				AddChartIndicator(EMA1);
				SetProfitTarget(@"22", CalculationMode.Currency, 0);
				SetStopLoss(@"", CalculationMode.Ticks, 44, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			 // Set 1
			if ((Position.MarketPosition == MarketPosition.Flat)
				 && (DrawOnlyArrows == false))
			{
				IsLong = false;
				IsShort = false;
			}
			
			if (CurrentBars[0] < 3)
				return;

			 // Set 2
			if ((Low[3] > EMA1[0])
				 && (Low[2] > EMA1[0])
				 && (Low[1] > EMA1[0])
				 && (GetCurrentAsk(0) > EMA1[0])
				 && (UsePullback == true)
				 && (DrawOnlyArrows == true)
				 && (IsLong == false))
			{
				Print(@"2isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, "LE" + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
				IsLong = true;
				IsShort = false;
			}
			
			 // Set 3
			if ((Low[3] > EMA1[0])
				 && (Low[2] > EMA1[0])
				 && (Low[1] > EMA1[0])
				 && (GetCurrentAsk(0) > EMA1[0])
				 && (UsePullback == true)
				 && (DrawOnlyArrows == false)
				 && (IsLong == false)
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				Print(@"3isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, "LE" + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
				EnterLong(Convert.ToInt32(DefaultQuantity), @"LongPB");
				IsLong = true;
				IsShort = false;
			}
			
			 // Set 4
			if ((High[0] < EMA1[0])
				 && (High[0] < EMA1[0])
				 && (High[0] < EMA1[0])
				 && (GetCurrentAsk(0) == EMA1[0])
				 && (UsePullback == true)
				 && (DrawOnlyArrows == true)
				 && (IsShort == false))
			{
				Print(@"4isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, Convert.ToString("SE") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );		
				IsLong = false;
				IsShort = true;
			}
			
			 // Set 5
			if ((High[0] < EMA1[0])
				 && (High[0] < EMA1[0])
				 && (High[0] < EMA1[0])
				 && (GetCurrentAsk(0) == EMA1[0])
				 && (UsePullback == true)
				 && (DrawOnlyArrows == true)
				 && (IsShort == false)
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				Print(@"5isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, Convert.ToString("SE") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );		
				EnterShort(Convert.ToInt32(DefaultQuantity), @"ShortPB");
				IsLong = false;
				IsShort = true;
			}
			
			//Print(@"6isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
			 // Set 6
			if (
				(Close[3] < EMA1[0])
				 && (Close[2] < EMA1[0])
				 && (Close[1] < EMA1[0]) &&
				  ((Close[0]- 2)  > EMA1[0] )
				 && (UsePullback == false)
				 && (DrawOnlyArrows == true)
				&& (IsLong == false)
				)
			{
				
				Print(@"6isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, "LE" + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
				IsLong = true;
				IsShort = false;
			}
			
			
			 // Set 7
			if ((Close[3] < EMA1[0])
				 && (Close[2] < EMA1[0])
				 && (Close[1] < EMA1[0])
				 && ((Close[0] - 2)  > EMA1[0])
				 && (UsePullback == false)
				 && (DrawOnlyArrows == false)
				 && (IsLong == false)
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				Print(@"7isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, Convert.ToString("LE") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
				EnterLong(Convert.ToInt32(DefaultQuantity), @"LongCO");
				IsLong = true;
				IsShort = false;
			}
			
			
			
			
			
			 // Set 8
			if ((Close[3] > EMA1[0])
				 && (Close[2] > EMA1[0])
				 && (Close[1] > EMA1[0])
				 && ((Close[0] + 2)  < EMA1[0])
				 && (UsePullback == false)
				 && (DrawOnlyArrows == true)
				 && (IsShort == false))
			{
				Print(@"8isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, Convert.ToString("SE") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );		
				IsLong = false;
				IsShort = true;
				Print(@"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
			}
			
			
			 // Set 9
			if ((Close[3] > EMA1[0])
				 && (Close[2] > EMA1[0])
				 && (Close[1] > EMA1[0])
				 && ((Close[0] + 2)  < EMA1[0])
				 && (UsePullback == false)
				 && (DrawOnlyArrows == false)
				 && (IsShort == false)
				 && (Position.MarketPosition == MarketPosition.Flat))
			{
				Print(@"9isLong---- " + Convert.ToString(IsLong) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"isShort---- " + Convert.ToString(IsShort) + @" ---- " + Convert.ToString(Times[0][0]));
				Print(@"UsePB---- " + Convert.ToString(UsePullback) + @" ---- " + Convert.ToString(Times[0][0]));
				Draw.Text(this, Convert.ToString("SE") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );		
				EnterShort(Convert.ToInt32(DefaultQuantity), @"ShortCO");
				IsLong = false;
				IsShort = true;
			}
			
			 if ((IsLong && (Close[0] )  < EMA1[0]) || (IsShort && (Close[0] )  > EMA1[0]))
			{
				IsShort = false;
				IsLong=false;
			}
			
			
			
				
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Order=1, GroupName="Parameters")]
		public int Length
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UsePullback", Order=2, GroupName="Parameters")]
		public bool UsePullback
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseCrossover", Order=3, GroupName="Parameters")]
		public bool UseCrossover
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="DrawOnlyArrows", Order=4, GroupName="Parameters")]
		public bool DrawOnlyArrows
		{ get; set; }
		#endregion

	}
}
