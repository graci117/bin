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
	public class BenStrat : Strategy
	{
		private Series<double> EMAFast;
		private Series<double> EMASlow;
		private Series<double> SMA20;
		private Series<double>  MA1;
		private ADX adx1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BenStrat";
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
				FastEMA					= 4;
				SlowEMA					= 34;
				SMALength					= 4;
				StopLoss					= 50;
				ProfitTrigger					= 60;
				FastMaType										= CDMAtype.EMA;
				SlowMaType										= CDMAtype.EMA;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				
				//SetStopLoss(CalculationMode.Ticks, StopLoss);
				SetStopLoss(CalculationMode.Ticks, StopLoss);
				SetProfitTarget(CalculationMode.Ticks, ProfitTrigger);
				
				
//				EMAFast			= SMA(Close,FastEMA);
//				EMASlow			= EMA(Close,SlowEMA);
				EMAFast 		= GetMA(true);
				EMASlow 		= GetMA(false);
				SMA20			= EMA(Close,SMALength).Value;
				adx1			= ADX(14);
				//qc = Qcloud(Brushes.Red,Brushes.Green,19,29,39,49,59,99,false);
				//EMAFast.Plots[0].Brush = Brushes.Cyan;
				//EMASlow.Plots[0].Brush = Brushes.Green;
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 0)
				return;
			
			if (ToTime(Time[0]) < 093000  || ToTime(Time[0]) > 160000)
			{
				//maxLossHit = false;
				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
				return;
				
			}
			
			if (ToTime(Time[0]) < 093300  || ToTime(Time[0]) > 113000)
			{
				//maxLossHit = false;
				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
				return;
				
			}
			
			if ((EMAFast[0] > EMASlow[0]) && (CrossAbove(Close, SMA20, 1)) && adx1[0] > 19 &&  adx1[0] < 75)
			{
				EnterLong(1, "Long");
				Draw.ArrowUp(this, "ARROWUP" + CurrentBar, true, 0, Low[0] - TickSize, Brushes.Lime);
			}

			// Down arrow condition
			if ((EMAFast[0] < EMASlow[0]) && (CrossBelow(Close, SMA20, 1)) && adx1[0] > 19 &&  adx1[0] < 75)
			{
				EnterShort(1, "Short");
				Draw.ArrowDown(this, "ARROWDOWN" + CurrentBar, true, 0, High[0] + TickSize, Brushes.Red);
			}
			

		}
		
		private Series<double> GetMA(bool isFast)
		{
			CDMAtype Ma0Type;
			int MALength ;
			if (isFast)
			{
				Ma0Type = FastMaType;
				MALength = FastEMA;
			}
			else
			{
				Ma0Type = SlowMaType;
				MALength = SlowEMA;
			}
			
			switch (Ma0Type)
				{
					case CDMAtype.DEMA:						
						
						MA1 = DEMA(Close, MALength).Value;
						
						break;
						
					case CDMAtype.EMA:
							MA1 = EMA(Close, MALength).Value;
						
					break;	
						
					case CDMAtype.HMA:
							MA1 = HMA(Close, MALength).Value;
					
					break;	
						
					case CDMAtype.LinReg:
							MA1 = LinReg(Close, MALength).Value;

					break;							
						
					case CDMAtype.SMA:
							MA1 = SMA(Close, MALength).Value;
					
					break;	
						
					case CDMAtype.TEMA:
							MA1 = TEMA(Close, MALength).Value;

					break;	
						
					case CDMAtype.TMA:	
							MA1 = TMA(Close, MALength).Value;
					
					break;	
					
					case CDMAtype.VWMA:
							MA1 = VWMA(Close, MALength).Value;

					break;	
						
					case CDMAtype.WMA:
							MA1 = WMA(Close, MALength).Value;
							
					break;
						
					case CDMAtype.ZLEMA:
							MA1 = ZLEMA(Close, MALength).Value;

					break;												
				}	
				return MA1;
				//qc.
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastEMA", Order=1, GroupName="Parameters")]
		public int FastEMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowEMA", Order=3, GroupName="Parameters")]
		public int SlowEMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SMALength", Order=5, GroupName="Parameters")]
		public int SMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopLoss", Order=6, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTrigger", Order=7, GroupName="Parameters")]
		public int ProfitTrigger
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name=" Fast MA Type", Description="RSI MA Type", Order=12)]
		public CDMAtype FastMaType
        { get; set; }
		
			[NinjaScriptProperty]
		[Display(Name=" Slow MA Type", Description="RSI MA Type", Order=13)]
		public CDMAtype SlowMaType
        { get; set; }
		
		
		
		#endregion

	}
}
