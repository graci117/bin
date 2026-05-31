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
	public class RegChannelBeTrailADXv2 : Strategy
	{
		
		private RegressionChannel2 RegressionChannel21;
		private RegressionChannelExtended RegressionChannelExtended1;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		private ADX ADX1;
		
		private string sVersion = "2.0.0.0"; // CURRENT VERSION 00.
        private Brush s1Color = Brushes.DimGray; // TRADE SESSION BACKGROUND COLOR 01. - 02.
    
		private double BEStoredTargetPrice;
		private double BEStoredActualPrice;
		
		private double TrailStoredTargetPrice;
		private double TrailStoredActualPrice;
		
		private bool StopSetBool;
		private bool BreakEvenBool;
		private bool TrailStopBool;

		#region OnStateChange
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Printing Money using UniRenko";
				Name										= "RegChannelBeTrailADXv2 " + sVersion;
				Calculate									= Calculate.OnPriceChange;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.High;
				Slippage									= 1;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				myVersion = "(c) 2024 by Kreative Collaborations, version " + sVersion; // 00. TEMPLATE VERSION INFORMATION
				
				EnableTimeSet = true; // 01. ENABLE TRADE TIME SESSION
				SessionOpacity	= 20; // 02. TRADE SESSION BACKGROUND OPACITY PRESET

				// 03. PRESET TRADE SESSION WINDOW
        		StartTime = DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
        		EndTime = DateTime.Parse("15:45", System.Globalization.CultureInfo.InvariantCulture);

				ProfitTarget					= 40;
				Contracts						= 1;
				
				InitialStop						= 60;
				
				BETargetTicks					= 32;	// How many ticks until BE Set
				BEOffset						= 4;
				
				TrailTargetTicks				= 33;	// How many ticks until Trail Set
				TrailStopDistance				= 10;	// How far back your stop will trail
			
				//Set at false from default
				StopSetBool						= false;
				BreakEvenBool					= false;
				TrailStopBool					= false;
				
				ShowHistorical					= true;
				
				ADXPeriod						= 4;
				ADXThreshold					= 50;
				
			}
			else if (State == State.Configure)
			{
//				SetTrailStop(@"", CalculationMode.Ticks, TrailTicks, false);
			}
			else if (State == State.DataLoaded)
			{				
				RegressionChannel21						= RegressionChannel2(Close, 40, 3.5);
				RegressionChannelExtended1				= RegressionChannelExtended(Close, 40, 3.5);
				RegressionChannelHighLow1				= RegressionChannelHighLow(Close, 40, 3.5);
				ADX1									= ADX(Close, Convert.ToInt32(ADXPeriod));
//				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
//				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
			}
		}
		#endregion

		#region OnBarUpdate
		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 5)
				return;
			
			if (!ShowHistorical)
			{
				if (State != State.Realtime)
					return;
			}
			
			 // Set 1 - Set Order
			if (
				 // RegChanLongGroup1
				(((RegressionChannelExtended1.Middle[1] > RegressionChannelExtended1.Middle[2])
				 && (RegressionChannelExtended1.Middle[2] <= RegressionChannelExtended1.Middle[3])
				 && (ADX1[0] > ADX1[2])				
				 && (ADX1[0] > ADXThreshold))

				 // RegChanLongGroup2
				 || ((RegressionChannelExtended1.Middle[0] > RegressionChannelExtended1.Middle[1])
				 && (Low[0] > Low[2])
				 && (Low[2] <= RegressionChannelExtended1.Lower[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				
				 // RegChanLongGroup3
				 || ((Low[0] > RegressionChannelHighLow1.Lower[2])
				 || (ADX1[0] > ADX1[2])
				 || (ADX1[0] > ADXThreshold)))
				
				 // Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				
				 && (Position.MarketPosition == MarketPosition.Flat)
				 && ((EnableTimeSet == true) // 03.
				 && (Times[0][0].TimeOfDay >= StartTime.TimeOfDay)
          		 && (Times[0][0].TimeOfDay < EndTime.TimeOfDay))
)
				
			{
				EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"GoingUp");
				BreakEvenBool 	= false;
				TrailStopBool	= false;
				StopSetBool		= false;
			}
			
			// Set 2 - Set Stop and BE/Trail Targets
			if ((Position.MarketPosition == MarketPosition.Long)
				 && !StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (InitialStop * TickSize)) , @"MoneyDone", @"GoingUp");
				ExitLongLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (ProfitTarget * TickSize)) , @"MoneyWon", @"GoingUp");
				
				BEStoredTargetPrice = (Position.AveragePrice + (BETargetTicks * TickSize)); //Store how far price needs to move before BE is set
				BEStoredActualPrice = (Position.AveragePrice + (BEOffset * TickSize)); //Store the actual BE Value for later use
				
				
				TrailStoredTargetPrice = (Position.AveragePrice + (TrailTargetTicks * TickSize)); // Store How far price needs to move before Trail Stop is set
				TrailStoredActualPrice = Close[0]; // Store a value for Trail -> Needs a check first but will be adjusted later on when its set
				
				StopSetBool = true;
			}
			
			 // Set 3 - Set Breakeven
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] >= BEStoredTargetPrice)
					&& GetCurrentAsk(0) > BEStoredActualPrice && GetCurrentBid(0) > BEStoredActualPrice
					&& StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"MoneyDone", @"GoingUp");
				BreakEvenBool = true;
			}
			
			 // Set 4 - Set Trail Stop
			if ((Position.MarketPosition == MarketPosition.Long)
				 && Close[0] >= TrailStoredTargetPrice
					&& StopSetBool && BreakEvenBool && !TrailStopBool
					&& Close[0] - (TrailStopDistance * TickSize) > BEStoredActualPrice
					&& Close[0] - (TrailStopDistance * TickSize) > TrailStoredActualPrice)
			{
				TrailStoredActualPrice = Close[0] - (TrailStopDistance * TickSize); //Update Trail Price before submitting order
				
				if (GetCurrentAsk(0) > TrailStoredActualPrice && GetCurrentBid(0) > TrailStoredActualPrice)
					ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"MoneyDone", @"GoingUp");
			}
			
			//======Short Sets======\\
			
			 // Set 6 - Set Order
			if (
				 // RegChanShortGroup1
				(((RegressionChannelExtended1.Middle[1] < RegressionChannelExtended1.Middle[2])
				 && (RegressionChannelExtended1.Middle[2] >= RegressionChannelExtended1.Middle[3])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				
				 // RegChanShortGroup2
				 || ((RegressionChannelExtended1.Middle[0] < RegressionChannelExtended1.Middle[1])
				 && (High[0] < High[2])
				 && (High[2] >= RegressionChannelExtended1.Upper[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				
				 // RegChanShortGroup3
				 || ((High[0] < RegressionChannelHighLow1.Upper[2])
				 || (ADX1[0] > ADX1[2])
				 || (ADX1[0] > ADXThreshold)))
				
				 // Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				
				 && (Position.MarketPosition == MarketPosition.Flat)
				 && ((EnableTimeSet == true)
				 && (Times[0][0].TimeOfDay >= StartTime.TimeOfDay) // 03.
          		 && (Times[0][0].TimeOfDay < EndTime.TimeOfDay))
				 )
			{
				EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(0), @"GoingDown");
				StopSetBool		= false;
				BreakEvenBool 	= false;
				TrailStopBool	= false;
			}
			
			
			// Set 2 - Set Stop and BE/Trail Targets
			if ((Position.MarketPosition == MarketPosition.Short)
				 && !StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (InitialStop * TickSize)) , @"MoneyDone", @"GoingDown");
				ExitShortLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (ProfitTarget * TickSize)) , @"MoneyWon", @"GoingDown");
				
				BEStoredTargetPrice = (Position.AveragePrice - (BETargetTicks * TickSize)); //Store how far price needs to move before BE is set
				BEStoredActualPrice = (Position.AveragePrice - (BEOffset * TickSize)); //Store the actual BE Value for later use
				
				
				TrailStoredTargetPrice = (Position.AveragePrice - (TrailTargetTicks * TickSize)); // Store How far price needs to move before Trail Stop is set
				TrailStoredActualPrice = Close[0]; // Store a value for Trail -> Needs a check first but will be adjusted later on when its set
				
				StopSetBool = true;
			}
			
			// Set 3 - Set Breakeven
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] <= BEStoredTargetPrice)
					&& GetCurrentAsk(0) < BEStoredActualPrice && GetCurrentBid(0) < BEStoredActualPrice
					&& StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"MoneyDone", @"GoingDown");
				BreakEvenBool = true;
			}
			
			// Set 4 - Set Trail Stop
			if ((Position.MarketPosition == MarketPosition.Short)
				 && Close[0] <= TrailStoredTargetPrice
					&& StopSetBool && BreakEvenBool && !TrailStopBool
					&& Close[0] + (TrailStopDistance * TickSize) < BEStoredActualPrice
					&& Close[0] + (TrailStopDistance * TickSize) < TrailStoredActualPrice)
			{
				TrailStoredActualPrice = Close[0] + (TrailStopDistance * TickSize); //Update Trail Price before submitting order
				
				if (GetCurrentAsk(0) < TrailStoredActualPrice && GetCurrentBid(0) < TrailStoredActualPrice)
					ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"MoneyDone", @"GoingDown");
			}
			
				
			// Reset when Flat
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				StopSetBool		= false;
				BreakEvenBool 	= false;
				TrailStopBool	= false;
			}
			
            #region 01. TRADE SESSION BACKGROUND COLORS
      		// Set Session Background Color
      		int year = Convert.ToInt16(Time[0].ToString("yyyy"));
      		int month = Convert.ToInt16(Time[0].ToString("MM"));
      		int day = Convert.ToInt16(Time[0].ToString("dd"));

      		if ((Times[0][0].TimeOfDay >= StartTime.TimeOfDay)
     		&& (Times[0][0].TimeOfDay <= EndTime.TimeOfDay))
      		{
      		int barsAgo = CurrentBar - Bars.GetBar(new DateTime(year, month, day, (ToTime(StartTime) / 10000), 0, 0));
      		Draw.Rectangle(this, "TradeSession" + Time[0].ToString("MM/dd/yyyy"), false, barsAgo, Highs[0][HighestBar(High, barsAgo)], 0, Lows[0][LowestBar(Low, barsAgo)], Brushes.Transparent, s1Color, SessionOpacity);
      		}
			#endregion

		}
        #endregion

		#region Properties

		#region 00. STRATEGY VERSION

        [NinjaScriptProperty]
        [Display(Name = "Signal Version", GroupName = "00. GENERAL", Order = 0)]
        public string myVersion { get; private set; }

        #endregion

        #region 01. - 03. TRADE SESSION TIME

        [NinjaScriptProperty]
        [Display(Name = "Trade Session On/Off Switch", Description = "Bool switch to display background", GroupName = "01. TRADE SESSION COLORS ON/OFF SWITCHES", Order = 0)]
        public bool EnableTimeSet
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trade Session Color", Description = "Color for Trade Session", GroupName = "02. TRADE SESSION COLOR", Order = 0)]
        public Brush S1Color
        {
            get { return s1Color; }
            set { s1Color = value; }
        }
        [Browsable(false)]
        public string S1ColorSerialize
        {
            get { return Serialize.BrushToString(S1Color); }
            set { S1Color = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Range(0, 100)]
        [Display(Name = "Trade Session Opacity", Description = "Sessions Opacity", GroupName = "02. TRADE SESSION COLOR", Order = 1)]
        public int SessionOpacity
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start Time", Description = "Strategy Start Time", GroupName = "03. TRADE SESSION ACTIVE TIMES", Order = 0)]
        public DateTime StartTime
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "End Time", Description = "Strategy End Time", GroupName = "03. TRADE SESSION ACTIVE TIMES", Order = 1)]
        public DateTime EndTime
        { get; set; }

        #endregion
		
		#region 04. ORDER MANAGEMENT
        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Order=1, GroupName="04. Order Management")]
		public int Contracts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Initial Stop Ticks", Order=2, GroupName="04. Order Management")]
		public int InitialStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ProfitTarget", Order=3, GroupName="04. Order Management")]
		public int ProfitTarget
		{ get; set; }
		#endregion
		
        #region 05. BREAKEVEN
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Breakeven Target", Order=2, GroupName="05. BreakEven")]
		public int BETargetTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Breakeven Tick Offset", Order=3, GroupName="05. BreakEven")]
		public int BEOffset
		{ get; set; }
		#endregion
		
		#region .06 TRAIL STOP
        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailStop Target", Order=2, GroupName="06. Trail Stop")]
		public int TrailTargetTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trail Stop Distance", Order=3, GroupName="06. Trail Stop")]
		public int TrailStopDistance
		{ get; set; }
		#endregion
		
		#region 07. ADDITIONAL SETTINGS
		[NinjaScriptProperty]
		[Display(Name="Show Historical Trades", Order=3, GroupName="07. Additional Settings")]
		public bool ShowHistorical
		{ get; set; }
        #endregion
		
		#region 08. ADX SETTINGS
        [NinjaScriptProperty]
		[Display(Name="ADXPeriod", Order=1, GroupName="08. ADX Settings")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXThreshold", Order=2, GroupName="08. ADX Settings")]
		public int ADXThreshold
		{ get; set; }
        #endregion

		#endregion

	}
}