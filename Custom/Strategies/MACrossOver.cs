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
	public class MACrossOver : Strategy
	{
		private Series<double> EMAFast;
		private Series<double> EMASlow;
		private SMA SMA20;
		private Series<double>  MA1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MACrossOver";
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
				FastEMA					= 5;
				SlowEMA					= 8;
				SMALength					= 20;
				StopLoss					= 18;
				ProfitTrigger					= 30;
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
				SMA20			= SMA(Close,SMALength);
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
			
			if (SMALength == 1)
			{
				if (CrossAbove(EMAFast,EMASlow,1))
				{
					EnterLong(2, "Long");
				}
				else if (CrossBelow(EMAFast,EMASlow,1))
				{
					EnterShort(2,"Short");
				}
			}
			else
			{
				if (EMAFast[0] > EMASlow[0])
				{
					if (CrossAbove(SMA20,EMAFast,1))
					{
						EnterLong(2, "Long");
					}
					else if (CrossBelow(SMA20,EMAFast,1))
					{
						EnterShort(2,"Short");
					}
				}
				else if  (EMASlow[0] > EMAFast[0])
				{
					if (CrossAbove(SMA20,EMASlow,1))
					{
						EnterLong(2, "Long");
					}
					else if (CrossBelow(SMA20,EMASlow,1))
					{
						EnterShort(2,"Short");
					}
				}
					
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
		[Display(Name=" Slow MA Type", Description="RSI MA Type", Order=12)]
		public CDMAtype SlowMaType
        { get; set; }
		
		
		
		#endregion

	}
}
