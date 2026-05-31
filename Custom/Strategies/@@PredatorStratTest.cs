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

public enum MovingAvgType
	{
		DEMA,
		EMA,
		HMA,
		LinReg,
		SMA,
		TEMA,
		T3,
		TMA,
		VWMA,
		WMA,
		ZLEMA	,
		ZLHATema,
		ZLTema
	}	


//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class PredatorStratTest : Strategy
	{
		private Series<double> EMAFast;
		private Series<double> EMASlow;
		private SMA SMA20;
		private Series<double>  MA1;
		private bool maCrossLongCondtion;
		private bool maCrossShortCondtion;
		
		
		private double SuperTrendLong;
		private double SuperTrendShort;

		private TSSuperTrend TSSuperTrend1;
		private bool stLongCondition;
		private bool stShortCondition;
		
		private Series<double> TrendMA;
		private bool trendLongCondition;
		private bool trendShortCondition;
		
		private bool timeCondition;
		private bool isEnableTime2;	
		private bool isEnableTime3;	
		private bool isEnableTime4;	
		
		private double	initialBreakEven	= 0; 
		
		private Order entryOrderLong1 = null;
		private Order entryOrderLong2 = null;
		
		private Order entryOrderShort1 = null;
		private Order entryOrderShort2 = null;
		
		private double entry1price = 0;
		private double entry2price = 0;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "PredatorStratTest";
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
				
				Order1Quantity				= 1;
				Order2Quantity				= 0;
				
				StopLoss1					= 34;
				ProfitTrigger1					= 10;
				
				StopLoss2					= 34;
				ProfitTrigger2					= 10;
				
				FastEMA					= 8;
				SlowEMA					= 34;
				maCrossLongCondtion 	= false;
				maCrossShortCondtion 	= false;
				
				SMALength					= 20;
				
				FastMaType										= MovingAvgType.EMA;
				SlowMaType										= MovingAvgType.EMA;
				VFactor		= 0.7;
				TCount 		= 3;
				
				UseSuperTrend					= false;
				STPeriod					= 67;
				STMultiplier					= 3.17;
				STSmooth					= 75;
				STMAType					= MovingAverageType.HMA;
				stLongCondition			= false;				
				stShortCondition			= false;
				
				
				UseTrendMA					= false;
				TrendPeriod						= 151;
				TrendMAType					=  MovingAvgType.ZLHATema;
				trendLongCondition			= false;
				trendShortCondition			= false;
				
				timeCondition					= true;
				isEnableTime2					= false;
				isEnableTime3					= false;
				isEnableTime4					= false;
				
						
				Start							= DateTime.Parse("06:40", System.Globalization.CultureInfo.InvariantCulture);
				End								= DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
				Start2							= DateTime.Parse("09:00", System.Globalization.CultureInfo.InvariantCulture);
				End2							= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				Start3							= DateTime.Parse("11:00", System.Globalization.CultureInfo.InvariantCulture);
				End3							= DateTime.Parse("11:30", System.Globalization.CultureInfo.InvariantCulture);
				Start4							= DateTime.Parse("12:30", System.Globalization.CultureInfo.InvariantCulture);
				End4							= DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
				
				
				
				
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				
				//SetStopLoss(CalculationMode.Ticks, StopLoss);
				
				
				
//				SetStopLoss("EntryLong1",CalculationMode.Ticks, StopLoss1);
//				SetProfitTarget("EntryLong2",CalculationMode.Ticks, ProfitTrigger1);
				
//				SetStopLoss("EntryShort1",CalculationMode.Ticks, StopLoss1);
//				SetProfitTarget("EntryShort2",CalculationMode.Ticks, ProfitTrigger1);
				
				
//				EMAFast			= SMA(Close,FastEMA);
//				EMASlow			= EMA(Close,SlowEMA);
				EMAFast 		= GetMA(FastEMA, FastMaType);
				EMASlow 		= GetMA(SlowEMA,SlowMaType);
				SMA20			= SMA(Close,SMALength);
				
				if (UseSuperTrend)
				{
					TSSuperTrend1				= TSSuperTrend(Close, SuperTrendMode.ATR, STPeriod, STMultiplier, STMAType, STSmooth, false, false, false);
					TSSuperTrend1.Plots[0].Brush = Brushes.Green;
					TSSuperTrend1.Plots[1].Brush = Brushes.Red;
					AddChartIndicator(TSSuperTrend1);
				}
				
				if (UseTrendMA)
				{
					TrendMA 		= GetMA(TrendPeriod,TrendMAType);
				}
				
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
			
			if (CrossAbove(EMAFast,EMASlow,1))						
			{
				maCrossLongCondtion = true;
				//EnterLong(2, "Long");
			}
			else if (CrossBelow(EMAFast,EMASlow,1))
			{
				maCrossShortCondtion = true;
				//EnterShort(2,"Short");
			}
			else
			{
				maCrossLongCondtion = false;
				maCrossShortCondtion = false;
			}
				
			//Supertrend
			if (UseSuperTrend)
			{
				if (
					(Close[0] >= TSSuperTrend1.UpTrend[0])
					 && (TSSuperTrend1.UpTrend[0] != 0)
					 && (TSSuperTrend1.DownTrend[0] == 0)
					 && (TSSuperTrend1.DownTrend[1] != 0)
					)
				{
					stLongCondition = true;
					//EnterLong(Convert.ToInt32(PositionSize), @"EntryLong");				
					stShortCondition = false;
				}
				
				 // Set 4
				if ((Close[0] <= TSSuperTrend1.DownTrend[0])
					 && (TSSuperTrend1.DownTrend[0] != 0)
					 && (TSSuperTrend1.UpTrend[0] == 0)
					 && (TSSuperTrend1.UpTrend[1] != 0))
				{
					stShortCondition = true;
					stLongCondition = false;
					//EnterShort(Convert.ToInt32(PositionSize), @"EntryShort");				
				}
			}
			
			if (UseTrendMA)
			{
				if (Close[0] > TrendMA[0])
				{
					trendLongCondition = true;
					trendShortCondition = false;
				}
				
				if (Close[0] < TrendMA[0])
				{
					trendShortCondition = true;
					trendLongCondition = false;
				}
			}
			
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
			
			if (timeCondition &&
				maCrossLongCondtion &&
				(!UseTrendMA  || trendLongCondition) &&
				(!UseSuperTrend || 	stLongCondition)
				)
			{
				Print("TrendMA---" + UseTrendMA + Time[0]);
				EnterLong(Order1Quantity, "EntryLong1");
				EnterLong(Order2Quantity, "EntryLong2");
			}
			
			if (timeCondition &&
				maCrossShortCondtion &&
				(!UseTrendMA  || trendShortCondition) &&
				(!UseSuperTrend || 	stShortCondition)
				)
			{
				EnterShort(Order1Quantity, "EntryShort1");
				EnterShort(Order2Quantity, "EntryShort2");
			}
			
			//entryOrder1.
			
			if (SetBreakeven1 && entryOrderLong1 != null)
			{
				
				if (Close[0] > entryOrderLong1.AverageFillPrice + BE1Trigger * TickSize)
	            {
						//Position.Account.
					initialBreakEven = entryOrderLong1.AverageFillPrice + BE1Offset * TickSize;
	                ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), initialBreakEven, @"ExitLong1", @"EntryLong1");					
					
	            }
			}
			
			
			
			//entryOrder2.
			
			if (SetBreakeven2 && Order2Quantity >0 && entryOrderLong2 != null)
			{
				
				if (Close[0] > entryOrderLong2.AverageFillPrice + BE2Trigger * TickSize)
	            {
						//Position.Account.
					initialBreakEven = entryOrderLong2.AverageFillPrice + BE2Offset * TickSize;
	                ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), initialBreakEven, @"ExitLong2", @"EntryLong2");					
					
	            }
			}
			
			
			//entryOrder1.
			
			if (SetBreakeven1 && entryOrderShort1 != null)
			{
				
				if (Close[0] < entryOrderShort1.AverageFillPrice - BE1Trigger * TickSize)
	            {
						//Position.Account.
					initialBreakEven = entryOrderShort1.AverageFillPrice - BE1Offset * TickSize;
	                ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), initialBreakEven, @"ExitShort1", @"EntryShort1");					
					
	            }
			}
			
			
			
			//entryOrder2.
			
			if (SetBreakeven2 && Order2Quantity >0 && entryOrderShort2 != null)
			{
				
				if (Close[0] < entryOrderShort2.AverageFillPrice - BE2Trigger * TickSize)
	            {
						//Position.Account.
					initialBreakEven = entryOrderShort2.AverageFillPrice - BE2Offset * TickSize;
	                ExitLongStopMarket(Convert.ToInt32(DefaultQuantity), initialBreakEven, @"ExitShort2", @"EntryShort2");					
					
	            }
			}
			
			
			//initialBreakEven = Position.AveragePrice + plusBreakEven * TickSize;

		}
		
		protected override void OnExecutionUpdate(Cbi.Execution execution, string executionId, double price, int quantity, 
			Cbi.MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (execution.Name == "EntryLong1"  && execution.Order.OrderState == OrderState.Filled)
			{
				ExitLongStopMarket(0, true, execution.Order.Filled, execution.Order.AverageFillPrice - StopLoss1 * TickSize, "StopForEntryLong1", "EntryLong1");
				ExitLongLimit(0, true, execution.Order.Filled, execution.Order.AverageFillPrice + ProfitTrigger1 * TickSize, "TargetForEntryLong1", "EntryLong1");
				entryOrderLong1 = execution.Order;
				entry1price = execution.Order.AverageFillPrice;
			}
			else if (execution.Name == "EntryLong2"  && execution.Order.OrderState == OrderState.Filled)
			{
				ExitLongStopMarket(0, true, execution.Order.Filled, execution.Order.AverageFillPrice - StopLoss2 * TickSize, "StopForEntryLong2", "EntryLong2");
				ExitLongLimit(0, true, execution.Order.Filled, execution.Order.AverageFillPrice + ProfitTrigger2 * TickSize, "TargetForEntryLong2", "EntryLong2");
				entryOrderLong2 = execution.Order;
				entry2price = execution.Order.AverageFillPrice;
			}
			
			if (execution.Name == "EntryShort1"  && execution.Order.OrderState == OrderState.Filled)
			{
				ExitShortStopMarket(0, true, execution.Order.Filled, execution.Order.AverageFillPrice + StopLoss1 * TickSize, "StopForEntryShort2", "EntryShort1");
				ExitLongLimit(0, true, execution.Order.Filled, execution.Order.AverageFillPrice - ProfitTrigger1 * TickSize, "TargetForEntryShort2", "EntryShort1");
				entryOrderShort1 = execution.Order;
				entry1price = execution.Order.AverageFillPrice;
			}
			else if (execution.Name == "EntryShort2"  && execution.Order.OrderState == OrderState.Filled)
			{
				ExitLongStopMarket(0, true, execution.Order.Filled, execution.Order.AverageFillPrice - StopLoss2 * TickSize, "StopForEntryShort2", "EntryShort2");
				ExitLongLimit(0, true, execution.Order.Filled, execution.Order.AverageFillPrice + ProfitTrigger2 * TickSize, "TargetForEntryShort2", "EntryShort2");
				entryOrderLong2 = execution.Order;
				entry2price = execution.Order.AverageFillPrice;
			}
		}
		
		private Series<double> GetMA( int MALength, MovingAvgType Ma0Type)
		{
						
			switch (Ma0Type)
				{
					case MovingAvgType.DEMA:						
						
						MA1 = DEMA(Close, MALength).Value;
						
						break;
						
					case MovingAvgType.EMA:
							MA1 = EMA(Close, MALength).Value;
						
					break;	
						
					case MovingAvgType.HMA:
							MA1 = HMA(Close, MALength).Value;
					
					break;	
						
					case MovingAvgType.LinReg:
							MA1 = LinReg(Close, MALength).Value;

					break;							
						
					case MovingAvgType.SMA:
							MA1 = SMA(Close, MALength).Value;
					
					break;	
					
					case  MovingAvgType.T3:
							MA1 = T3(Close, MALength, TCount, VFactor).Value;
					
					break;
						
					case MovingAvgType.TEMA:
							MA1 = TEMA(Close, MALength).Value;

					break;	
						
					case MovingAvgType.TMA:	
							MA1 = TMA(Close, MALength).Value;
					
					break;	
					
					case MovingAvgType.VWMA:
							MA1 = VWMA(Close, MALength).Value;

					break;	
						
					case MovingAvgType.WMA:
							MA1 = WMA(Close, MALength).Value;
							
					break;
						
					case MovingAvgType.ZLEMA:
							MA1 = ZLEMA(Close, MALength).Value;
					break;
					
				case MovingAvgType.ZLHATema:
					
					MA1 = ZeroLagHATEMAmodLT(Close, MALength).Value;
					break;
					
				case MovingAvgType.ZLTema:
					
					MA1 = ZLTEMAmodLT(Close, MALength).Value;
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
		[Display(Name=" Fast MA Type", Description="RSI MA Type", Order=12)]
		public MovingAvgType FastMaType
        { get; set; }
		
			[NinjaScriptProperty]
		[Display(Name=" Slow MA Type", Description="RSI MA Type", Order=12)]
		public MovingAvgType SlowMaType
        { get; set; }
		
		//For T3
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TCount - T3 Only", GroupName = "NinjaScriptParameters", Order = 1)]
		public int TCount
		{ get; set; }

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "VFactor - T3 Only ", GroupName = "NinjaScriptParameters", Order = 2)]
		public double VFactor
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use SuperTrend", Order=3, GroupName="04. Additional Settings")]
		public bool UseSuperTrend
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=40, GroupName="SuperTrend")]
		public int STPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.5, int.MaxValue)]
		[Display(Name="Multiplier", Order=50, GroupName="SuperTrend")]
		public double STMultiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smooth", Order=60, GroupName="SuperTrend")]
		public int STSmooth
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="MAType", Order=70, GroupName="SuperTrend")]
		public MovingAverageType STMAType
		{ get; set; }
	
		[NinjaScriptProperty]
		[Display(Name="Use Trend Moving Avergage", Order=3, GroupName="04. Additional Settings")]
		public bool UseTrendMA
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=40, GroupName="Trend Moving Average")]
		public int TrendPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="MAType", Order=70, GroupName="Trend Moving Average")]
		public MovingAvgType TrendMAType
		{ get; set; }
		
		
		#region 6. Time Valid
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Trades", Order=1, GroupName="06. Time Frames")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Trades", Order=2, GroupName="06. Time Frames")]
		public DateTime End
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 2", Description = "Enable 2 times.", Order=3, GroupName = "06. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time2
		{
		 	get{return isEnableTime2;} 
			set{isEnableTime2 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 2", Order=4, GroupName="06. Time Frames")]
		public DateTime Start2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 2", Order=5, GroupName="06. Time Frames")]
		public DateTime End2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 3", Description = "Enable 3 times.", Order=6, GroupName = "06. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time3
		{
		 	get{return isEnableTime3;} 
			set{isEnableTime3 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 3", Order=7, GroupName="06. Time Frames")]
		public DateTime Start3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 3", Order=8, GroupName="06. Time Frames")]
		public DateTime End3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 4", Description = "Enable 4 times.", Order=9, GroupName = "06. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time4
		{
		 	get{return isEnableTime4;} 
			set{isEnableTime4 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 4", Order=10, GroupName="06. Time Frames")]
		public DateTime Start4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 4", Order=11, GroupName="06. Time Frames")]
		public DateTime End4
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Order 1 Quantity", Order=60, GroupName="Trade Management1")]
		public int Order1Quantity
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopLoss 1", Order=6, GroupName="Parameters")]
		public int StopLoss1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTrigger1", Order=7, GroupName="Parameters")]
		public int ProfitTrigger1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Set Breakeven 1", Description = "Set Breakeven 1", Order=9, GroupName = "Trade Management1")]
		[RefreshProperties(RefreshProperties.All)]
		public bool SetBreakeven1
		{
		 	get;
			set;
		}		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Breakeven 1 Trigger", Order=60, GroupName="Trade Management1")]
		public int BE1Trigger
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-100, int.MaxValue)]
		[Display(Name="Breakeven 1 Trigger Offset", Order=60, GroupName="Trade Management1")]
		public int BE1Offset
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Order 2 Quantity", Order=60, GroupName="Trade Management1")]
		public int Order2Quantity
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopLoss 2", Order=6, GroupName="Parameters")]
		public int StopLoss2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ProfitTrigger2", Order=7, GroupName="Parameters")]
		public int ProfitTrigger2
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name = "Set Breakeven 2", Description = "Set Breakeven 2", Order=9, GroupName = "Trade Management2")]
		[RefreshProperties(RefreshProperties.All)]
		public bool SetBreakeven2
		{
		 	get;
			set;
		}		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Breakeven 2 Trigger", Order=60, GroupName="Trade Management2")]
		public int BE2Trigger
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-100, int.MaxValue)]
		[Display(Name="Breakeven 2 Trigger Offset", Order=60, GroupName="Trade Management2")]
		public int BE2Offset
		{ get; set; }
		
				
		#endregion
	
		
		
		#endregion

	}
}
