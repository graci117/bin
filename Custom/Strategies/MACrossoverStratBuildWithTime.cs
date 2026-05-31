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
	public class MACrossoverStratBuildWithTime : Strategy
	{
		private Series<double> EMAFast;
		private Series<double> EMASlow;
		private SMA SMA20;
		private Series<double>  MA1;
		private bool timeCondition;
		private bool longCondition;
		private bool shortCondition;
		
		
		private bool isEnableTime2;	
		private bool isEnableTime3;	
		private bool isEnableTime4;	
		private bool isEnableTime;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MACrossoverStratBuildWithTime";
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
				
				timeCondition					= true;
				UseTimeFilters					= true;
				isEnableTime2					= false;
				isEnableTime3					= false;
				isEnableTime4					= false;
				isEnableTime					= true;
				
				Start							= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				End								= DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
				Start2							= DateTime.Parse("09:00", System.Globalization.CultureInfo.InvariantCulture);
				End2							= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				Start3							= DateTime.Parse("11:00", System.Globalization.CultureInfo.InvariantCulture);
				End3							= DateTime.Parse("11:30", System.Globalization.CultureInfo.InvariantCulture);
				Start4							= DateTime.Parse("12:30", System.Globalization.CultureInfo.InvariantCulture);
				End4							= DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
				
				longCondition					= false;
				shortCondition					= false;
				
				
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
//				EMASlow			= EMA(Close,SlowEMA);;
				EMAFast 		= GetMA(true);
				EMASlow 		= GetMA(false);
				SMA20			= SMA(Close,SMALength);
				
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
			
//			if (ToTime(Time[0]) < 093000  || ToTime(Time[0]) > 160000)
//			{
//				//maxLossHit = false;
//				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
//				return;
				
//			}
			
			if (SMALength == 1)
			{
				if (CrossAbove(EMAFast,EMASlow,1))
				{
					longCondition = true;
					shortCondition = false;
					//EnterLong(2, "Long");
				}
				else if (CrossBelow(EMAFast,EMASlow,1))
				{
					shortCondition = true;
					//EnterShort(2,"Short");
					longCondition = false;
				}
				else
				{
					longCondition = false;
					shortCondition = false;
				} 
			}
			else
			{
				if (EMAFast[0] > EMASlow[0])
				{
					if (CrossAbove(SMA20,EMAFast,1))
					{
						longCondition = true;
						shortCondition = false;
						//EnterLong(2, "Long");
					}
					else if (CrossBelow(SMA20,EMAFast,1))
					{
						shortCondition = true;
						longCondition = false;
						//EnterShort(2,"Short");
					}
					else
					{
						longCondition = false;
						shortCondition = false;
					} 
				}
				else if  (EMASlow[0] > EMAFast[0])
				{
					if (CrossAbove(SMA20,EMASlow,1))
					{
						longCondition = true;
						shortCondition = false;
						//EnterLong(2, "Long");
					}
					else if (CrossBelow(SMA20,EMASlow,1))
					{
						shortCondition = true;
						longCondition = false;
						//EnterShort(2,"Short");
					}
					else
					{
						longCondition = false;
						shortCondition = false;
					} 
				}
				
			}
			
			if (UseTimeFilters)
			{
				if((Times[0][0].TimeOfDay >= Start.TimeOfDay) && (Times[0][0].TimeOfDay < End.TimeOfDay) 
						|| (Time2 && Times[0][0].TimeOfDay >= Start2.TimeOfDay && Times[0][0].TimeOfDay <= End2.TimeOfDay)
						|| (Time3 && Times[0][0].TimeOfDay >= Start3.TimeOfDay && Times[0][0].TimeOfDay <= End3.TimeOfDay)
						|| (Time4 && Times[0][0].TimeOfDay >= Start4.TimeOfDay && Times[0][0].TimeOfDay <= End4.TimeOfDay)
						)
				{
					timeCondition = true;
				}
				else
				{
					timeCondition = false;
				}
			
			}
			
			if (longCondition && timeCondition)
			{
				//EnterLong(2, "Long");
				EnterLongLimit(2,Close[0],"Long");
			}
			else if (shortCondition && timeCondition)
			{
				//EnterShort(2,"Short");
				EnterShortLimit(2,Close[0],"Short");
			}
//			else 
//			{
//				if (Position.MarketPosition == MarketPosition.Long)
//				{
//					 if (EMASlow[0] > EMAFast[0])
//					 {
//						 ExitLong();
//					 }
//				}
				
//				if (Position.MarketPosition == MarketPosition.Short)
//				{
//					 if (EMASlow[0] < EMAFast[0])
//					 {
//						 ExitShort();
//					 }
//				}
//			}
			
//			if (longCondition)
//			{
//				EnterLong(2, "Long");
//			}
			
//			if (shortCondition )
//			{
//				EnterShort(2,"Short");
//			}
			

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
		[Display(Name=" Slow MA Type", Description="RSI MA Type", Order=13)]
		public CDMAtype SlowMaType
        { get; set; }
		
		//--------------------------------------------------------------------------------------------
		
		
	
		
		[NinjaScriptProperty]
		[Display(Name = "UseTimeFilters", Description = "UseTimeFilters", Order=1, GroupName = "06. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool UseTimeFilters
		{
		 	get{return isEnableTime;} 
			set{isEnableTime = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Trades", Order=2, GroupName="06. Time Frames")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Trades", Order=3, GroupName="06. Time Frames")]
		public DateTime End
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 2", Description = "Enable 2 times.", Order=4, GroupName = "06. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time2
		{
		 	get{return isEnableTime2;} 
			set{isEnableTime2 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 2", Order=5, GroupName="06. Time Frames")]
		public DateTime Start2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 2", Order=6, GroupName="06. Time Frames")]
		public DateTime End2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 3", Description = "Enable 3 times.", Order=7, GroupName = "06. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time3
		{
		 	get{return isEnableTime3;} 
			set{isEnableTime3 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 3", Order=8, GroupName="06. Time Frames")]
		public DateTime Start3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 3", Order=9, GroupName="06. Time Frames")]
		public DateTime End3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 4", Description = "Enable 4 times.", Order=10, GroupName = "06. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time4
		{
		 	get{return isEnableTime4;} 
			set{isEnableTime4 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 4", Order=11, GroupName="06. Time Frames")]
		public DateTime Start4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 4", Order=12, GroupName="06. Time Frames")]
		public DateTime End4
		{ get; set; }
		
		
		
		#endregion

	}
}
