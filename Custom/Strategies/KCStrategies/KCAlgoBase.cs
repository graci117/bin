#region Using declarations
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Web.Script.Serialization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.KCStrategies
{
    abstract public class KCAlgoBase : Strategy, ICustomTypeDescriptor //
    {
		#region Variables
		private DateTime lastEntryTime;
		
		private VMA VMA1;
		public bool volMaUp;
		public bool volMaDown;
		
		public NTSvePivots pivots;
		public double pivotPoint, s1, s2, s3, r1, r2, r3, s1m, s2m, s3m, r1m, r2m, r3m;
		
		private ADX ADX1;
		public bool adxUp;
		
		private ATR ATR1;
		public bool atrUp;
		
		public bool aboveEMAHigh;
		public bool belowEMALow;
		
		public bool uptrend;
		public bool downtrend;
		
		private double lastStopLevel = 0;  // Tracks the last stop level
        private bool stopUpdated = false;  // To ensure stop is moved only when favorable
		
//		private bool activeTrade;
//		private int barsSinceActiveTrade = 0; // Tracks the number of bars since activeTrade became true
//		private int activeTradeStartBar = -1; // Stores the bar number when activeTrade became true

		public bool isLong;
		public bool isShort;
		public bool isFlat;
		public bool exitLong;
		public bool exitShort;
		
        // Progress tracking
        private double actualPnL;		
		private double trailStop;
		private bool _beRealized;
		private bool enableFixedStopLoss = false;
		private bool threeStepTrail;
		private bool trailingDrawdownReached = false;
        private int ProgressState;
		
		private double entryPrice;
		private double currentPrice;	
		private bool additionalContractExists;
		
		private bool isBuySellMarketOrder;
		private bool tradesPerDirection;	
		private int counterLong;	
		private int counterShort;	
		private bool QuickLong;
		private bool QuickShort;
		private bool quickLongBtnActive;
		private bool quickShortBtnActive;

//		private bool isEnableTime1;
		private bool isEnableTime2;	
		private bool isEnableTime3;	
		private bool isEnableTime4;	
		private bool isEnableTime5;	
		private bool isEnableTime6;			

		private bool isStrategyEnabled;
		private bool isLongEnabled;
		private bool isShortEnabled;
		
//		Chart Trader Buttons
		private System.Windows.Controls.RowDefinition	addedRow;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private System.Windows.Controls.Grid			chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid;
		
//		New Toggle Buttons
		private System.Windows.Controls.Button			strategyBtn, longBtn, shortBtn, quickLongBtn, quickShortBtn;
		private System.Windows.Controls.Button			add1Btn, close1Btn, BEBtn, TSBtn, moveTSBtn, moveToBEBtn;
		private System.Windows.Controls.Button			moveTS50PctBtn, closeBtn, panicBtn;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;
		private System.Windows.Controls.Grid 			myGrid;
		
		// KillAll 
		private Instrument inst;
		private Account chartTraderAccount;
		private AccountSelector accountSelector;
		private Order myEntryOrder = null;
		private Order myStopOrder = null;
		private Order myTargetOrder = null;
		private bool isAdded = false;
		private bool isAddedSetStop = false;
		public bool activeOrder = false;
		private double myStopPrice = 0;
		private double myLimitPrice = 0;
		private bool isBtnAdd1Enabled;
		private bool isBtnClose1Enabled;
		
//		Status Panel
		private string textLine0;	
		private string textLine1;
		private string textLine2;
		private string textLine3;
		private string textLine4;
		private string textLine5;
		private string textLine6;
		private string textLine7;
		
//		PnL
		private double totalPnL;
		private double cumPnL;
		private double dailyPnL;		
		private bool canTradeOK = true;
		private bool runOnce = false;
		
		private bool syncPnl;
		private double historicalTimeTrades;//Sync  PnL
		private double dif;//To Calculate PNL sync
		private double cumProfit;//For real time pnl and pnl synchronization
		
		private bool restartPnL;
		
		private bool beSetAuto;
		private bool showctrlBESetAuto;
		private bool atrTrailSetAuto;
		private bool showAtrTrailSetAuto;		
		private bool enableTrail;
		private bool showTrailOptions;
		public  bool tickTrail;
		
		private TrailStopTypeKC trailStopType;
		private bool showTickTrailOption;
		private bool showAtrTrailOptions;
		private bool showThreeStepTrailOptions;		

		private bool enableDynamicProfit = false;				
		private bool enableFixedProfit	 = true;
		private bool showctrlEnableDynamicProfit = false;
		private bool showctrlEnableFixedProfit = true;
		
		// Error Handling
		private readonly object orderLock = new object(); // Critical for thread safety
		private Dictionary<string, Order> activeOrders = new Dictionary<string, Order>(); // Track active orders with labels.
		private DateTime lastOrderActionTime = DateTime.MinValue;
		private readonly TimeSpan minOrderActionInterval = TimeSpan.FromSeconds(1); // Prevent rapid order submissions.
		private bool orderErrorOccurred = false; // Flag to halt trading after an order error.

		// Rogue Order Detection
		private DateTime lastAccountReconciliationTime = DateTime.MinValue;
		private readonly TimeSpan accountReconciliationInterval = TimeSpan.FromMinutes(5); // Check for rogue orders every 5 minutes
		#endregion

		#region Order Label Constants (Highly Recommended)

		// Define your order labels as constants.  This prevents typos and ensures consistency.
		private const string LongEntryLabel = "LE";
		private const string ShortEntryLabel = "SE";
		private const string QuickLongEntryLabel = "QLE";
		private const string QuickShortEntryLabel = "QSE";
		private const string Add1LongEntryLabel = "Add1LE";
		private const string Add1ShortEntryLabel = "Add1SE";
		// Add constants for other order labels as needed (e.g., "LE2", "SE2", "TrailingStop")

		#endregion
		
		#region TradeToDiscord
		
		private ClientWebSocket clientWebSocket;
		private List<dynamic> signalHistory = new List<dynamic>();
		private DateTime lastDiscordMessageTime = DateTime.MinValue;
		private readonly TimeSpan discordRateLimitInterval = TimeSpan.FromSeconds(30); // Adjust the interval as needed

		private string lastSignalType = "N/A";
		private double lastEntryPrice = 0.0;
		private double lastStopLoss = 0.0;
		private double lastProfitTarget = 0.0;
		private DateTime lastSignalTime = DateTime.MinValue;		
		
		#endregion
		
		#region OnStateChange
		protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {				
				Description									= @"Base Strategy with OEB v.5.0.2 TradeSaber(Dre). and ArchReactor for KC (Khanh Nguyen)";
				Name										= "KCAlgoBase";
				BaseAlgoVersion								= "KCAlgoBase v4.3";
				Author										= "indiVGA, Khanh Nguyen, Oshi, based on ArchReactor";
				Version										= "Version 4.3 Feb. 2025";
				Credits										= "";
				StrategyName 								= "";
				ChartType									= "";

                EntriesPerDirection = 10;					// This value should limit the number of contracts that the strategy can open per direction.
															// It has nothing to do with the parameter defining the entries per direction that we define in the strategy and are controlled by code.
                Calculate									= Calculate.OnPriceChange;
				EntryHandling 								= EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy 				= true;
                ExitOnSessionCloseSeconds 					= 30;
                IsFillLimitOnTouch 							= false;
                MaximumBarsLookBack 						= MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution 						= OrderFillResolution.High;
                Slippage 									= 0;
                StartBehavior 								= StartBehavior.WaitUntilFlat;
                TimeInForce 								= TimeInForce.Gtc;
                TraceOrders 								= false;
                RealtimeErrorHandling 						= RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling 							= StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade 						= 20;
				RealtimeErrorHandling 						= RealtimeErrorHandling.StopCancelClose; // important to manage errors on rogue orders
                IsInstantiatedOnEachOptimizationIteration 	= false;

				
                // Default Parameters
				isStrategyEnabled 				= true;
				isLongEnabled					= true;
				isShortEnabled					= true;
				canTradeOK 						= true;
				runOnce 						= false;
				
				OrderType						= OrderType.Limit;
				
				enableVMA						= true;
				showVMA							= true;
				
				emaLength						= 110;
				enableEMAFilter 				= false;
				showEMA							= false;
				
				adxPeriod						= 7;
				adxThreshold					= 25;
				adxThreshold2					= 50;
				adxExitThreshold				= 45;
				enableADX						= false;
				showAdx							= false;
				
				atrPeriod						= 14;
				atrThreshold					= 1.5;
				enableVolatility				= false;
				
				showPivots						= false;
				
				enableExit						= false;
				
				TickMove						= 4;
				
				Contracts						= 1;
				Contracts2 						= 1;
				Contracts3 					    = 1;
				Contracts4						= 1;
				
				InitialStop						= 92;
				
				ProfitTarget					= 48;
				ProfitTarget2					= 60;
				ProfitTarget3					= 80;
				ProfitTarget4					= 100;
				
				EnableProfitTarget2				= false;
				EnableProfitTarget3				= false;
				EnableProfitTarget4				= false;				
								
				EnableFixedProfit				= true;
				EnableDynamicProfit				= false;
				
			//	Set BE Stop
				BESetAuto						= true;
				beSetAuto						= true;
				showctrlBESetAuto				= true;
				BE_Trigger						= 36;
				BE_Offset						= 4;
				_beRealized						= false;

			//	Trailing Stops
				enableTrail 					= true;
				tickTrail						= false;
				showTrailOptions 				= true;	
				trailStopType 					= TrailStopTypeKC.TickTrail;
				
			//	ATR Trail
				atrTrailSetAuto					= false;
				showAtrTrailSetAuto				= false;
				showAtrTrailOptions 			= false;
				enableAtrProfitTarget			= false;
				atrMultiplier					= 2;
				RiskRewardRatio					= 3;
//				Trail_frequency					= 4;
				
			//	3 Step Trail	
				showThreeStepTrailOptions 		= false;
				threeStepTrail					= false;
				step1ProfitTrigger 				= 1;	// Set your step 1 profit trigger
                step1StopLoss 					= 93;	// Set your step 1 stop loss
                step2ProfitTrigger 				= 40;	// Set your step 2 profit trigger
                step2StopLoss 					= 36;	// Set your step 2 stop loss
				step3ProfitTrigger 				= 80;	// Set your step 3 profit trigger
				step3StopLoss 					= 30;	// Set your step 3 stop loss
//				step1Frequency					= 4;
//				step2Frequency					= 4;
//				step3Frequency					= 2;
				ProgressState 					= 0;
				
				tradesPerDirection				= false;
				longPerDirection				= 5;
				shortPerDirection				= 5;	
				iBarsSinceExit					= 0;				
				SecsSinceEntry					= 0;
				
				QuickLong						= false;
				QuickShort						= false;
				
				counterLong						= 0;
				counterShort					= 0;
				
				Start							= DateTime.Parse("06:30", System.Globalization.CultureInfo.InvariantCulture);
				End								= DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
				Start2							= DateTime.Parse("11:00", System.Globalization.CultureInfo.InvariantCulture);
				End2							= DateTime.Parse("11:20", System.Globalization.CultureInfo.InvariantCulture);
				Start3							= DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
				End3							= DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
				Start4							= DateTime.Parse("17:00", System.Globalization.CultureInfo.InvariantCulture);
				End4							= DateTime.Parse("17:20", System.Globalization.CultureInfo.InvariantCulture);
				Start5							= DateTime.Parse("06:30", System.Globalization.CultureInfo.InvariantCulture);
				End5							= DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
				Start6							= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End6							= DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
				
				// Panel Status
				showDailyPnl					= true;
				PositionDailyPNL				= TextPosition.TopLeft;	
				colorDailyProfitLoss			= Brushes.Cyan; // Default value
				
				showPnl							= false;
				PositionPnl						= TextPosition.BottomLeft;
				colorPnl 						= Brushes.Yellow; // Default value
				
				// PnL Daily Limits
				dailyLossProfit					= true;
				DailyProfitLimit				= 100000;
				DailyLossLimit					= 1000;
				TrailingDrawdown				= 1000;
				StartTrailingDD					= 3000;
				
				ShowHistorical					= true;
				
				useWebHook						= false;
				DiscordWebhooks					= "https://discord.com/channels/963493404988289124/1343311936736989194";
				
            }
            else if (State == State.Configure)
            {
				// Ensure RealtimeErrorHandling is set
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
				
				clientWebSocket = new ClientWebSocket();			
            }
            else if (State == State.DataLoaded)
            {	
				VMA1				= VMA(Close, 9, 9);
				VMA1.Plots[0].Brush = Brushes.SkyBlue;
				VMA1.Plots[0].Width = 3;
				if (showVMA) AddChartIndicator(VMA1);			
				
				ATR1 	= ATR(atrPeriod);
				
				if(showEMA) 
				{
					AddChartIndicator(EMA(High, emaLength));
					AddChartIndicator(EMA(Low, emaLength));
				}
					
				ADX1				= ADX(Close, adxPeriod);
				ADX1.Plots[0].Brush = Brushes.Yellow;
				ADX1.Plots[0].Width = 2;
				if (showAdx) AddChartIndicator(ADX1);
				
				pivots = NTSvePivots(Close, false, NTSvePivotRange.Daily, NTSveHLCCalculationMode.CalcFromIntradayData, 0, 0, 0, 250);
				pivots.Plots[0].Width = 4;
				if (showPivots) AddChartIndicator(pivots);
				
				if (additionalContractExists)
			    {
			        string quickProfitTargetLabel = isLong ? "QLE" : "QSE";  // QLE = Quick Long Entry, QSE = Quick Short Entry
			        SetProfitTarget(quickProfitTargetLabel, CalculationMode.Ticks, ProfitTarget);
			    } 	
            }
			else if (State == State.Historical)
			{
				// Chart Trader Buttons Load	
				Dispatcher.InvokeAsync((() => {	CreateWPFControls();	}));
				
			}
			else if (State == State.Terminated)
			{
				// Chart Trader Buttons dispose
				ChartControl?.Dispatcher.InvokeAsync(() =>	{	DisposeWPFControls();	});
				
				clientWebSocket?.Dispose();	
				
				// Log any remaining active orders
				lock (orderLock)
				{
					if (activeOrders.Count > 0)
					{
						Print (string.Format("{0}: Strategy terminated with active orders. Investigate:", Time[0]));
						foreach (var kvp in activeOrders)
						{
							Print (string.Format("{0}: Order Label: {1}, Order ID: {2}", Time[0], kvp.Key, kvp.Value.OrderId));
							// Consider attempting to cancel the order.  Do this ONLY if you have
							// carefully considered the implications (e.g., potential for slippage)
							CancelOrder(kvp.Value); // IMPORTANT: Cancel rogue orders before terminating.
						}
					}
				}				
			}
        }
		#endregion	
		
        #region OnBarUpdate
		protected override void OnBarUpdate()
        {
			if (BarsInProgress != 0 || CurrentBars[0] < 5 || !isStrategyEnabled || orderErrorOccurred)
                return;
			
			// *** Account Reconciliation ***
			if (DateTime.Now - lastAccountReconciliationTime > accountReconciliationInterval)
			{
				ReconcileAccountOrders();
				lastAccountReconciliationTime = DateTime.Now;
			}

			if (!ShowHistorical)
			{
				if (State != State.Realtime)
					return;				
			}	
	
			// Get pivot points and support/resistance levels
            pivotPoint = pivots.Pp[0];
            s1 = pivots.S1[0];
            s2 = pivots.S2[0];
            s3 = pivots.S3[0];
            r1 = pivots.R1[0];
            r2 = pivots.R2[0];
            r3 = pivots.R3[0];
            s1m = pivots.S1M[0];
            s2m = pivots.S2M[0];
			s3m = pivots.S3M[0];
            r1m = pivots.R1M[0];
            r2m = pivots.R2M[0];
			r3m = pivots.R3M[0];
			
			atrUp = enableVolatility ? ATR1[0] > atrThreshold : true;
			
			adxUp = enableADX ? ADX1[0] > adxThreshold && ADX1[0] < adxThreshold2 : true;
			
			volMaUp = enableVMA ? Close[0] > VMA1[0] : true;
			volMaDown = enableVMA ? Close[0] < VMA1[0] : true;
			
			aboveEMAHigh = enableEMAFilter ? Open[1] > EMA(High, emaLength)[1] : true;
			belowEMALow = enableEMAFilter ? Open[1] < EMA(Low, emaLength)[1] : true;
			
			uptrend = aboveEMAHigh && volMaUp  && adxUp && atrUp;
            downtrend = belowEMALow && volMaDown && adxUp && atrUp;
			
			entryPrice = Position.AveragePrice;
			currentPrice = Close[0];
					    
			// Define long, short and flat
		    isLong = Position.MarketPosition == MarketPosition.Long;
			isShort = Position.MarketPosition == MarketPosition.Short;
			isFlat = Position.MarketPosition == MarketPosition.Flat;
					
//			if (activeTrade)
//			{
//				barsSinceActiveTrade = CurrentBar - activeTradeStartBar; // Calculate the number of bars since activeTrade became true
				
//				if (barsSinceActiveTrade > 1) // Check if the number of bars exceeds x
//				{
//					activeTrade = false; // Reset activeTrade
//					activeTradeStartBar = -1; // Reset the start bar counter
//				}
//			}
			
			// Logic to check if additional contracts exist (i.e., more than one contract is held)
		    additionalContractExists = Position.Quantity > 1;
			
			// Calculate Profit Target based on ATR
		    if (enableAtrProfitTarget) ProfitTarget = ATR1[0] * RiskRewardRatio;
			
			if (EnableFixedProfit)
			{
				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
				SetProfitTarget(@"QLE", CalculationMode.Ticks, ProfitTarget);
				SetProfitTarget(@"QSE", CalculationMode.Ticks, ProfitTarget);
				
				setMultipleProfitTargets();
			}
			else if (EnableDynamicProfit)
			{
				// Set profit target at each pivot level
				if (isLong && Close[0] > r3 && Low[0] <= r3)
					SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
				else
					SetProfitTargetBasedOnLongConditions();
				
				if (isShort && Close[0] < s3 && High[0] >= s3)
					SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
				else 
					SetProfitTargetBasedOnShortConditions();
			}
			
			// at the start of a new session, reset the currentPnL for a new day of trading
			if (Bars.IsFirstBarOfSession)
			{
				cumPnL 			= totalPnL; ///Double that copies the full session PnL (If trading multiple days). Is only calculated once per day.
				dailyPnL		= totalPnL - cumPnL; ///Subtract the copy of the full session by the full session PnL. This resets your daily PnL back to 0.
			}

//			if (IsFirstTickOfBar) checkPositions();    // Detect unwanted Positions opened (possible rogue Order?
			
			if (showPnl) showPNLStatus();
			
			#region Long Entry
			
			if (ValidateEntryLong() 
				&& (isStrategyEnabled)
				&& (isLongEnabled) 
				&& (checkTimers())
				&& ((dailyLossProfit ? dailyPnL > -DailyLossLimit : true))  //Loss remains 'above' limit 
				&& ((dailyLossProfit ? dailyPnL < DailyProfitLimit: true)) //Profit remains 'below' limit
				&& (isFlat)
				&& (uptrend)
				&& (!trailingDrawdownReached)
				&& ((iBarsSinceExit > 0 ? BarsSinceExitExecution(0, "", 0) > iBarsSinceExit: BarsSinceExitExecution(0, "", 0) > 1) || BarsSinceExitExecution(0, "", 0) == -1)
				&& (canTradeOK)
//				&& (!activeTrade)
				)
            {
				if (!TradesPerDirection || (TradesPerDirection && counterLong < longPerDirection))
				{
//					activeTradeStartBar = CurrentBar;
//					activeTrade = true;
					counterLong +=1;
					counterShort = 0;
					runOnce = true;
					
					if (State == State.Realtime) 
					{
						double _entryPrice = Close[0];
	        			double _stopLoss = _entryPrice - InitialStop * TickSize;
	        			double _profitTarget = _entryPrice + ProfitTarget * TickSize;
					//	Update last signal details
					    lastSignalType = "LONG";
					    lastEntryPrice = _entryPrice;
					    lastStopLoss = _stopLoss;
					    lastProfitTarget = _profitTarget;
					    lastSignalTime = Time[0];
					//	Send Entry Signal to Discord
					    _ = SendSignalToDiscordAsync(lastSignalType, lastEntryPrice, lastStopLoss, lastProfitTarget, lastSignalTime);
						
					}	
					
					SubmitEntryOrder(LongEntryLabel, OrderType, Contracts);
					
					Draw.Dot(this, "LE " + Convert.ToString(CurrentBars[0]), false, 0, (Close[0]) , Brushes.Cyan);
				}
				else
				{
					Print("Limit long trades in a row");
				}
				
				// Ensure stop and target are set right after the entry
	 			setStopLossAndProfitTargetForEntry(LongEntryLabel);
				enterMultipleLongsTargets(false);
				ManageProfitTargets();
            }
			
			#endregion
			
			#region Short Entry
			
            if (ValidateEntryShort() 
				&& (isStrategyEnabled)
				&& (isShortEnabled)
				&& (checkTimers())
				&& ((dailyLossProfit ? dailyPnL > -DailyLossLimit : true))  //Loss remains 'above' limit 
				&& ((dailyLossProfit ? dailyPnL < DailyProfitLimit: true)) //Profit remains 'below' limit
				&& (isFlat)
				&& (downtrend)
				&& (!trailingDrawdownReached)
				&& ((iBarsSinceExit > 0 ? BarsSinceExitExecution(0, "", 0) > iBarsSinceExit : BarsSinceExitExecution(0, "", 0) > 1) || BarsSinceExitExecution(0, "", 0) == -1)
				&& (canTradeOK)
//				&& (!activeTrade)
				)
            {

				if (!TradesPerDirection || (TradesPerDirection && counterShort < shortPerDirection))
				{			
//					activeTradeStartBar = CurrentBar;
//					activeTrade = true;	
					counterLong =0;
					counterShort +=1;
					runOnce = true;					

					if (State == State.Realtime) 
					{					
				        double _entryPrice = Close[0];
				        double _stopLoss = _entryPrice + InitialStop * TickSize;
				        double _profitTarget = _entryPrice - ProfitTarget * TickSize;					
						
					//	Update last signal details
					    lastSignalType = "SHORT";
					    lastEntryPrice = _entryPrice;
					    lastStopLoss = _stopLoss;
					    lastProfitTarget = _profitTarget;
					    lastSignalTime = Time[0];				
						
					//	Send Entry Signal to Discord
					    _ = SendSignalToDiscordAsync(lastSignalType, lastEntryPrice, lastStopLoss, lastProfitTarget, lastSignalTime);					
					}

					SubmitEntryOrder(ShortEntryLabel, OrderType, Contracts);
					
					Draw.Dot(this, "SE " + Convert.ToString(CurrentBars[0]), false, 0, (Close[0]) , Brushes.Yellow);
				}
				else
				{
					Print("Limit short trades in a row");
				}
				
				// Ensure stop and target are set right after the entry
		        setStopLossAndProfitTargetForEntry(ShortEntryLabel);
		        enterMultipleShortTargets(false);
				ManageProfitTargets();
            }	
			
			#endregion
			
			#region Reset Trades Per Direction
            if (TradesPerDirection){
                if (counterLong != 0 && Close[1] < Open[1])
                    counterLong = 0;
                if (counterShort != 0 && Close[1] > Open[1])
                    counterShort = 0;
            }
            #endregion	
			
			#region Breakeven Stop
			
			if (!isFlat && beSetAuto)				
			{
				// Calculate the actual profit/loss in ticks
				actualPnL = Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks, Close[0]);
			
				// Determine if breakeven conditions are met
				if (actualPnL >= BE_Trigger) 
				{
					// Set the trail stop in ticks
					trailStop = BE_Trigger - BE_Offset;
			
			        // Create the order tags array based on whether additional contracts exist
			        string[] orderTags = additionalContractExists ? new[] 
						{ "LE", "LE2", "LE3", "LE4", "SE", "SE2", "SE3", "SE4", "QLE", "QLE2", "QLE3", "QLE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "LE", "SE" };
			
			        // Apply the trailing stop to all relevant tags
			        foreach (string tag in orderTags)
			        {
//						if (enableTrail)
//						{
							SetTrailStop(tag, CalculationMode.Ticks, trailStop, true);
//							setMultipleStopLoss(trailStop);
//						}
//						if (enableFixedStopLoss)
//						{
//							ApplyFixedStopLoss(trailStop);
////							setMultipleStopLoss(trailStop);
//						}
			        }
			
					// Mark breakeven as realized
					_beRealized = true;
				}
			}	
			
			#endregion

			#region Set Fixed Stop Loss
			if (!isFlat && enableFixedStopLoss)
			{
				// Calculate the actual profit/loss in ticks
//				actualPnL = Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks, Close[0]);
			
//				// Determine if breakeven conditions are met
//				if (actualPnL >= BE_Trigger + BE_Offset && beSetAuto) 
//				{
//					// Set the trail stop in ticks
//					double newStopPrice = BE_Trigger - BE_Offset;
					
//					ApplyFixedStopLoss(newStopPrice);
//				}
//				else
					ApplyFixedStopLoss(InitialStop);
			}			
			#endregion
			
			#region Three-step, ATR Trail and Tick Trail
			
			if (!isFlat && enableTrail)
		    {
				// Debug: Displaying the current stopValueTicks
		        Print($"[DEBUG] Default trailing stop value in ticks: {trailStop}.");
		
		        if (threeStepTrail)
		        {
					 // Cache the unrealized PnL value for the current close price
				    actualPnL = Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks, Close[0]);
					
		            // Adjust for three-step trail logic
		            switch (ProgressState)
		            {
		                case 0:
		                    if (actualPnL >= step1ProfitTrigger)
		                    {
		                        ProgressState = 1;
		                        trailStop = step1StopLoss;
		                        Print($"[DEBUG] Transition to ProgressState 1: stopValueTicks = {trailStop}.");
		                    }
		                    break;
		                case 1:
		                    if (actualPnL >= step2ProfitTrigger)
		                    {
		                        ProgressState = 2;
		                        trailStop = step2StopLoss;
		                        Print($"[DEBUG] Transition to ProgressState 2: stopValueTicks = {trailStop}.");
		                    }
		                    break;
		                case 2:
		                    if (actualPnL >= step3ProfitTrigger)
		                    {
		                        trailStop = step3StopLoss;
		                        Print($"[DEBUG] Transition to ProgressState 3: stopValueTicks = {trailStop}.");
		                    }
		                    break;
		            }
		        }
		        
				else if (atrTrailSetAuto)
		        {
		            // ATR trailing stop logic
		            trailStop = Math.Abs((ATR1[0] * atrMultiplier - entryPrice) / TickSize);
		            Print($"[DEBUG] ATR trailing stop: stopValueTicks = {trailStop}.");
		        }
				
				else if (tickTrail)
				{
				    trailStop = InitialStop;
				}
				
		        // Apply the trailing stop
		        ApplyTrailStop(trailStop);
		    }
			
			else if (isFlat)
			{
				trailStop = InitialStop;
			}
			
			#endregion
			
			#region Reset Stop Loss
			
			// Reset when Flat
			if (isFlat)
			{
				// Reset quick order buttons
			    quickLongBtnActive = false;
			    quickShortBtnActive = false;
			
			    // Reset counters and progress state
			    ProgressState = 0;
		        lastStopLevel = InitialStop;
			    _beRealized = false;
			
			    // Reset trailing stops to InitialStop
			    if (enableTrail) ApplyTrailStop(InitialStop);
				if (enableFixedStopLoss) ApplyFixedStopLoss(InitialStop);
				
				//	We start the waiting sequence of x seconds to be able to enter another operation
				if (runOnce){
					lastEntryTime = Time[0];
					Print(Time[0] + " Timer activated");
					canTradeOK = false;		
					runOnce = false;
				}
				
				// Clear active orders on flatten. CRITICAL for ghost order prevention.
				lock (orderLock)
				{
					activeOrders.Clear();
				}
			}
			
			if (!canTradeOK)
			{
                if (Time[0] >= lastEntryTime.AddSeconds(SecsSinceEntry))
                {
					Print(Time[0] + " Timer de-activated");
                    canTradeOK = true;
				//	runOnce = true;
                }				
			}	
			
			#endregion
			
			if (ValidateExitLong()) 
			{
				// Create the order labels array based on whether additional contracts exist
				string[] orderLabels = additionalContractExists ? new[] { "LE", "LE2", "LE3", "LE4", "QLE", "QLE2", "QLE3", "QLE4" } : new[] { "LE" };
				
				// Apply the initial stop for all relevant orders
				foreach (string label in orderLabels)
				{		              
					ExitLong(label);
				}
			}
			
			if (ValidateExitShort())
			{
				// Create the order labels array based on whether additional contracts exist
				string[] orderLabels = additionalContractExists ? new[] { "SE", "SE2", "SE3", "SE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "SE" };
				
				// Apply the initial stop for all relevant orders
				foreach (string label in orderLabels)
				{		              
					ExitShort(label);
				}	
			}
			
			KillSwitch();
        }
		#endregion
		
		#region Order Submission Helpers

		// This method encapsulates all order submissions and error handling.
		private Order SubmitEntryOrder(string orderLabel, OrderType orderType, int contracts)
		{
			Order submittedOrder = null;

			lock (orderLock)
			{
				if (!CanSubmitOrder())
				{
					Print (string.Format("{0}: Cannot submit {1} order: Minimum order interval not met.", Time[0], orderLabel));
					return null; // Or throw an exception if order submission is absolutely critical
				}

				try
				{
					switch (orderType)
					{
						case OrderType.Market:
							if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
								submittedOrder = EnterLong(contracts, orderLabel);
							else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
								submittedOrder = EnterShort(contracts, orderLabel);
							else
								throw new ArgumentException("Invalid order label for Market order.");
							break;
						case OrderType.Limit:
							if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
								submittedOrder = EnterLongLimit(contracts, GetCurrentBid(), orderLabel);
							else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
								submittedOrder = EnterShortLimit(contracts, GetCurrentAsk(), orderLabel);
							else
								throw new ArgumentException("Invalid order label for Limit order.");
							break;
						case OrderType.MIT:
							if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
								submittedOrder = EnterLongMIT(contracts, GetCurrentBid(), orderLabel);
							else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
								submittedOrder = EnterShortMIT(contracts, GetCurrentAsk(), orderLabel);
							else
								throw new ArgumentException("Invalid order label for MIT order.");
							break;
						case OrderType.StopLimit:
							if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
								submittedOrder = EnterLongLimit(contracts, GetCurrentBid(), orderLabel);
							else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
								submittedOrder = EnterShortLimit(contracts, GetCurrentAsk(), orderLabel);
							else
								throw new ArgumentException("Invalid order label for StopLimit order.");
							break;
						case OrderType.StopMarket:
							if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
								submittedOrder = EnterLong(contracts, orderLabel);
							else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
								submittedOrder = EnterShort(contracts, orderLabel);
							else
								throw new ArgumentException("Invalid order label for StopMarket order.");
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(orderType), orderType, "Unsupported order type");
					}

					if (submittedOrder != null)
					{
						activeOrders[orderLabel] = submittedOrder;  // TRACK THE ORDER!
						lastOrderActionTime = DateTime.Now;
						Print (string.Format("{0}: Submitted {1} order with OrderId: {2}", Time[0], orderLabel, submittedOrder.OrderId));
					}
					else
					{
						Print (string.Format("{0}: Error: {1} Entry order was null after submission.", Time[0], orderLabel));
						orderErrorOccurred = true;
					}
				}
				catch (Exception ex)
				{
					Print (string.Format("{0}: Error submitting {1} entry order: {2}", Time[0], orderLabel, ex.Message));
					orderErrorOccurred = true;
				}
			}

			return submittedOrder;
		}

		private void SubmitExitOrder(string orderLabel)
		{
			lock(orderLock)
			{
				try
				{
					if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel || orderLabel == Add1LongEntryLabel) {
						ExitLong(orderLabel);
					} else if(orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel || orderLabel == Add1ShortEntryLabel){
						ExitShort(orderLabel);
					} else {
						Print ($"Error: invalid order label {orderLabel}");
					}
					
					if(!activeOrders.ContainsKey(orderLabel))
						Print ($"Cannot cancel order that does not exist");
					
					if(activeOrders.TryGetValue(orderLabel, out Order orderToCancel)) {
						CancelOrder(orderToCancel);
						activeOrders.Remove(orderLabel);
					}
				} catch(Exception ex) {
					Print ($"Error submitting Exit order: {ex.Message}");
					orderErrorOccurred = true;
				}
			}
		}

		#endregion
		
		#region Rogue Order Detection

		private void ReconcileAccountOrders()
		{
		    lock (orderLock)
		    {
		        try
		        {
		            // Get all accounts
		            var accounts = Account.All;
		
		            if (accounts == null || accounts.Count == 0)
		            {
		                Print(string.Format("{0}: No accounts found.", Time[0]));
		                return;
		            }
		
		            // Iterate through the accounts and reconcile orders
		            foreach (Account account in accounts)
		            {
		                // Get the list of all orders associated with each instrument in that account
		                List<Order> accountOrders = new List<Order>();

		                try
		                {
		                    foreach (Position position in account.Positions)
		                    {
		                        Instrument instrument = position.Instrument;
		                        foreach (Order order in Orders)
		                        {									
		                            if (order.Instrument == instrument && order.Account == account)
		                            {
		                                accountOrders.Add(order);
		                            }
		                        }
		                    }
		                }
		                catch (Exception ex)
		                {
		                    Print(string.Format("{0}: Error getting orders for account {1}: {2}", Time[0], account.Name, ex.Message));
		                    continue; // Move to the next account. Don't halt the entire strategy if one account fails.
		                }
		
		
		                // Check for nulls and validity of account orders
		                if (accountOrders == null || accountOrders.Count == 0)
		                {
		                    Print(string.Format("{0}: No orders found in account {1}.", Time[0], account.Name));
		                    continue; //Move to the next account
		                }

		                // Create a list of order IDs from activeOrders
		                HashSet<string> strategyOrderIds = new HashSet<string>(activeOrders.Values.Select(o => o.OrderId));
		
		                // Iterate through the account orders and check if they are tracked by the strategy
		                foreach (Order accountOrder in accountOrders)
		                {
		                    if (!strategyOrderIds.Contains(accountOrder.OrderId))
		                    {
		                        // This is a rogue order!
		                        Print(string.Format("{0}: Rogue order detected! Account: {6} OrderId: {1}, OrderType: {2}, OrderStatus: {3}, Quantity: {4}, AveragePrice: {5}",
		                            Time[0], accountOrder.OrderId, accountOrder.OrderType, accountOrder.OrderState, accountOrder.Quantity, accountOrder.AverageFillPrice, account.Name));
		
		                        // You can either attempt to manage it:
		
		                        // Attempt to cancel the rogue order.  If it's a manual order, you might want to skip this step and just log it.
		                        try
		                        {
		                            CancelOrder(accountOrder);
		                            Print(string.Format("{0}: Attempted to cancel rogue order: {1}", Time[0], accountOrder.OrderId));
		                        }
		                        catch(Exception ex)
		                        {
		                            Print(string.Format("{0}: Failed to Cancel rogue order. Account: {6} OrderId: {1}, OrderType: {2}, OrderStatus: {3}, Quantity: {4}, AveragePrice: {5}, Reason: {7}",
										Time[0], accountOrder.OrderId, accountOrder.OrderType, accountOrder.OrderState, accountOrder.Quantity, accountOrder.AverageFillPrice, account.Name, ex.Message));

		                        }
		                        /*
		                        // Or, if you are brave and know what you are doing:
		                        // Adopt the order by adding it to activeOrders with a generated label.
		                        string rogueLabel = "Rogue_" + Guid.NewGuid().ToString();
		                        activeOrders[rogueLabel] = accountOrder;
		
		                        Print(string.Format("{0}: Adopted rogue order {1} with label {2}", Time[0], accountOrder.OrderId, rogueLabel));
		                        */
		                    }
		                }
		            } // End of account iteration
		        }
		        catch (Exception ex)
		        {
		            Print(string.Format("{0}: Error during account reconciliation: {1}", Time[0], ex.Message));
		            orderErrorOccurred = true;  // Consider whether to halt trading
		        }
		    }
		}

		#endregion
		
		#region Helper Methods

		// Method to check the minimum interval between order submissions
		private bool CanSubmitOrder()
		{
			return (DateTime.Now - lastOrderActionTime) >= minOrderActionInterval;
		}
	
		#endregion
	
		#region OnExecutionUpdate
		
		protected virtual void OnExecutionUpdate(Execution execution, string executionId, double price,
										   int quantity, MarketPosition marketPosition, string orderId,
										   DateTime time)
		{
		    if (execution.Order.Name == "FixedStop")
		    {
		        if (isLong)
		        {
		            // Update FixedStopLossTicks for long positions
		            InitialStop = (int)((Position.AveragePrice - price) / TickSize);
		            Print($"Long Stop Loss adjusted. New FixedStopLossTicks: {InitialStop}");
		        }
		        else if (isShort)
		        {
		            // Update FixedStopLossTicks for short positions
		            InitialStop = (int)((price - Position.AveragePrice) / TickSize);
		            Print($"Short Stop Loss adjusted. New FixedStopLossTicks: {InitialStop}");
		        }
		    }

			// *** CRITICAL: Track order fills, modifications, and cancellations ***
			lock (orderLock)
			{
				// Find the order in our activeOrders dictionary
				string orderLabel = activeOrders.FirstOrDefault(x => x.Value.OrderId == orderId).Key;

				if (!string.IsNullOrEmpty(orderLabel))
				{
					switch (execution.Order.OrderState)
					{
						case OrderState.Filled:
							Print (string.Format("{0}: Order {1} with label {2} filled.", Time[0], orderId, orderLabel));
							activeOrders.Remove(orderLabel); // Remove the order when it's filled.
							break;

						case OrderState.Cancelled:
							Print (string.Format("{0}: Order {1} with label {2} cancelled.", Time[0], orderId, orderLabel));
							activeOrders.Remove(orderLabel); // Remove cancelled orders
							break;

						case OrderState.Rejected:
							Print (string.Format("{0}: Order {1} with label {2} rejected.", Time[0], orderId, orderLabel));
							activeOrders.Remove(orderLabel); // Remove rejected orders
							break;

//						case OrderState.PartiallyFilled:
//							Print (string.Format("{0}: Order {1} with label {2} partially filled. Quantity: {3}", Time[0], orderId, orderLabel, quantity));
//							// Handle partial fills if your strategy logic requires it.
//							break;

						default:
							Print (string.Format("{0}: Order {1} with label {2} updated to state: {3}", Time[0], orderId, orderLabel, execution.Order.OrderState));
							break;
					}
				}
				else
				{
					// This could indicate a rogue order or an order not tracked by the strategy.
					Print (string.Format("{0}: Execution update for order {1}, but order is not tracked by the strategy.", Time[0], orderId));

					//Attempt to Cancel the Rogue Order
					try {
						CancelOrder(execution.Order);
						Print (string.Format("{0}: Successfully Canceled the Rogue Order: {1}.", Time[0], orderId));

					} catch(Exception ex) {
						Print (string.Format("{0}: Could not Cancel the Rogue Order: {1}. {2}", Time[0], orderId, ex.Message));
						orderErrorOccurred = true;  // Consider whether to halt trading

					}
				}
			}
		}
		
		#endregion
		
		#region Pivot Profit Targets

        private void SetProfitTargetBasedOnLongConditions()
        {
            if (Close[0] > s3 && Low[0] <= s3)
				SetProfitTarget("LE", CalculationMode.Price, s3m);
			else if (Close[0] > s3m && Low[0] <= s3m)
				SetProfitTarget("LE", CalculationMode.Price, s2);
			else if (Close[0] > s2 && Low[0] <= s2)
				SetProfitTarget("LE", CalculationMode.Price, s2m);
			else if (Close[0] > s2m && Low[0] <= s2m)	
				SetProfitTarget("LE", CalculationMode.Price, s1);
			else if (Close[0] > s1 && Low[0] <= s1)
				SetProfitTarget("LE", CalculationMode.Price, s1m);
			else if (Close[0] > s1m && Low[0] <= s1m)
				SetProfitTarget("LE", CalculationMode.Price, pivotPoint);
			else if (Close[0] > pivotPoint && Low[0] <= pivotPoint)
				SetProfitTarget("LE", CalculationMode.Price, r1m);
			else if (Close[0] > r1m && Low[0] <= r1m)
				SetProfitTarget("LE", CalculationMode.Price, r1);
			else if (Close[0] > r1 && Low[0] <= r1)
				SetProfitTarget("LE", CalculationMode.Price, r2m);
			else if (Close[0] > r2m && Low[0] <= r2m)
				SetProfitTarget("LE", CalculationMode.Price, r2);
			else if (Close[0] > r2 && Low[0] <= r2)
				SetProfitTarget("LE", CalculationMode.Price, r3m);
			else if (Close[0] > r3m && Low[0] <= r3m)
				SetProfitTarget("LE", CalculationMode.Price, r3);
			else if (Close[0] > r3 && Low[0] <= r3)
				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
        }

        private void SetProfitTargetBasedOnShortConditions()
        {
            if (Close[0] < r3 && High[0] >= r3)
				SetProfitTarget("SE", CalculationMode.Price, r3m);
			else if (Close[0] < r3m && High[0] >= r3m)
				SetProfitTarget("SE", CalculationMode.Price, r2);
			else if (Close[0] < r2 && High[0] >= r2)
				SetProfitTarget("SE", CalculationMode.Price, r2m);
			else if (Close[0] < r2m && High[0] >= r2m)
				SetProfitTarget("SE", CalculationMode.Price, r1);
			else if (Close[0] < r1 && High[0] >= r1)
				SetProfitTarget("SE", CalculationMode.Price, r1m);
			else if (Close[0] < r1m && High[0] >= r1m)
				SetProfitTarget("SE", CalculationMode.Price, pivotPoint);
			else if (Close[0] < pivotPoint && High[0] >= pivotPoint)
				SetProfitTarget("SE", CalculationMode.Price, s1m);
			else if (Close[0] < s1m && High[0] >= s1m)
				SetProfitTarget("SE", CalculationMode.Price, s1);
			else if (Close[0] < s1 && High[0] >= s1)
				SetProfitTarget("SE", CalculationMode.Price, s2m);
			else if (Close[0] < s2m && High[0] >= s2m)
				SetProfitTarget("SE", CalculationMode.Price, s2);
			else if (Close[0] < s2 && High[0] >= s2)
				SetProfitTarget("SE", CalculationMode.Price, s3m);
			else if (Close[0] < s3m && High[0] >= s3m)
				SetProfitTarget("SE", CalculationMode.Price, s3);
			else if (Close[0] < s3 && High[0] >= s3)
				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
        	}
		
		private void setStopLossAndProfitTargetForEntry(string entryOrderLabel)
		{
		  if (enableTrail)
		  {
		      SetTrailStop(entryOrderLabel, CalculationMode.Ticks, InitialStop, true);
		      setMultipleStopLoss(InitialStop);
		  }
		  else if (enableFixedStopLoss)
		  {
		      ApplyFixedStopLoss(InitialStop);
		      setMultipleStopLoss(InitialStop);
		  }
		}
		
		private void enterMultipleLongsTargets(bool isManual) {
			if (enableFixedProfit) {
				if(isManual) {
					enterEnabledOrder(true, EnableProfitTarget2, @"QLE2", Contracts2);
					enterEnabledOrder(true, EnableProfitTarget3,  @"QLE3", Contracts3);
					enterEnabledOrder(true, EnableProfitTarget4,  @"QLE4", Contracts4);
				} else {
					enterEnabledOrder(true, EnableProfitTarget2, @"LE2", Contracts2);
					enterEnabledOrder(true, EnableProfitTarget3,  @"LE3", Contracts3);
					enterEnabledOrder(true, EnableProfitTarget4,  @"LE4", Contracts4);
				}
			}
		}
		
		private void enterMultipleShortTargets(bool isManual) {
			if (enableFixedProfit) {
				if(isManual) {
					enterEnabledOrder(false, EnableProfitTarget2, @"QSE2", Contracts2);
					enterEnabledOrder(false, EnableProfitTarget3,  @"QSE3", Contracts3);
					enterEnabledOrder(false, EnableProfitTarget4,  @"QSE4", Contracts4);
				} else {
					enterEnabledOrder(false, EnableProfitTarget2, @"SE2", Contracts2);
					enterEnabledOrder(false, EnableProfitTarget3,  @"SE3", Contracts3);
					enterEnabledOrder(false, EnableProfitTarget4,  @"SE4", Contracts4);
				}
			}
		}
		
		private void setMultipleProfitTargets() {
			if (enableFixedProfit) {
				if (EnableProfitTarget2) {
						SetProfitTarget(@"LE2", CalculationMode.Ticks, ProfitTarget2);
						SetProfitTarget(@"SE2", CalculationMode.Ticks, ProfitTarget2);
						SetProfitTarget(@"QLE2", CalculationMode.Ticks, ProfitTarget2);
						SetProfitTarget(@"QSE2", CalculationMode.Ticks, ProfitTarget2);
					}
					
					if (EnableProfitTarget3) {
						SetProfitTarget(@"LE3", CalculationMode.Ticks, ProfitTarget3);
						SetProfitTarget(@"SE3", CalculationMode.Ticks, ProfitTarget3);
						SetProfitTarget(@"QLE3", CalculationMode.Ticks, ProfitTarget3);
						SetProfitTarget(@"QSE3", CalculationMode.Ticks, ProfitTarget3);
					}
					
					if (EnableProfitTarget4) {
						SetProfitTarget(@"LE4", CalculationMode.Ticks, ProfitTarget4);
						SetProfitTarget(@"SE4", CalculationMode.Ticks, ProfitTarget4);
						SetProfitTarget(@"QLE4", CalculationMode.Ticks, ProfitTarget4);
						SetProfitTarget(@"QSE4", CalculationMode.Ticks, ProfitTarget4);
					}
			}
		}
		
		private void setMultipleStopLoss(double stopLoss) {
			if (enableFixedProfit) {
				if (EnableProfitTarget2) {
						SetTrailStop(@"LE2", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"SE2", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"QLE2", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"QSE2", CalculationMode.Ticks, stopLoss, true);
					}
					
					if (EnableProfitTarget3) {
						SetTrailStop(@"LE3", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"SE3", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"QLE3", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"QSE3", CalculationMode.Ticks, stopLoss, true);
					}
					
					if (EnableProfitTarget4) {
						SetTrailStop(@"LE4", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"SE4", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"QLE4", CalculationMode.Ticks, stopLoss, true);
						SetTrailStop(@"QSE4", CalculationMode.Ticks, stopLoss, true);
					}
			}
		}
		
		private void enterEnabledOrder(bool isLong, bool isEnableTarget, string signalName, int contracts) {
			if (isEnableTarget) {
				if (isLong) {
					if(OrderType == OrderType.Market) EnterLong(Convert.ToInt32(contracts), signalName); 				
					else if (OrderType == OrderType.Limit) EnterLongLimit(Convert.ToInt32(contracts), GetCurrentBid(), signalName);
					else if (OrderType == OrderType.MIT) EnterLongMIT(Convert.ToInt32(contracts), GetCurrentBid(), signalName);
					else if (OrderType == OrderType.StopLimit) EnterLongLimit(Convert.ToInt32(contracts), GetCurrentBid(), signalName);
					else if (OrderType == OrderType.StopMarket) EnterLong(Convert.ToInt32(contracts), signalName);
				}
				else {
					if (OrderType == OrderType.Market) EnterShort(Convert.ToInt32(contracts), signalName);
					else if (OrderType == OrderType.Limit) EnterShortLimit(Convert.ToInt32(contracts), GetCurrentAsk(), signalName);			
					else if (OrderType == OrderType.MIT) EnterShortMIT(Convert.ToInt32(contracts), GetCurrentAsk(), signalName);
					else if (OrderType == OrderType.StopLimit) EnterShortLimit(Convert.ToInt32(contracts), GetCurrentAsk(), signalName);
					else if (OrderType == OrderType.StopMarket) EnterShort(Convert.ToInt32(contracts), signalName);
				}
			}
		}
		
		#endregion		
		
		#region Adjust Stop Loss

		protected void AdjustStopLoss(int tickAdjustment)
		{
		    if (isFlat)
		    {
		        Print("No active position to adjust trailing stop.");
		        return;
		    }
		
		    double entryPrice = Position.AveragePrice;
		    bool isLong = Position.MarketPosition == MarketPosition.Long; // Ensure correct isLong value
			double currentPrice = Close[0];
		
		    // Get all active stop orders
		    List<Order> stopOrders = new List<Order>();
		    foreach (var order in Orders)
		    {
		        if ((order.OrderState == OrderState.Working || order.OrderState == OrderState.TriggerPending) && order.OrderTypeString == "Stop Market" && order.StopPrice > 0)
		        {
		            stopOrders.Add(order);
		        }
		    }

		    if (stopOrders.Count == 0)
		    {
		        // No stop orders found, apply adjustment to the initial stop
		        double currentTrailingStopPrice = isLong
		            ? entryPrice - InitialStop * TickSize
		            : entryPrice + InitialStop * TickSize;
		
		        // Calculate the new trailing stop price based on the adjustment
		        double newTrailingStopPrice = isLong
		            ? currentTrailingStopPrice + tickAdjustment * TickSize  // Increase stop price for longs
		            : currentTrailingStopPrice - tickAdjustment * TickSize;  // Decrease stop price for shorts
		
		        // Calculate the tick offset from the *entry price* (important for SetTrailStop)
		        double tickOffset = isLong
		            ? (entryPrice - newTrailingStopPrice) / TickSize
		            : (newTrailingStopPrice - entryPrice) / TickSize;
		
		        // Apply the new trailing stop
		        ApplyTrailStop(tickOffset);
		        return;
		    }
		
		    // Adjust existing stop orders
		    foreach (var order in stopOrders)
		    {
		        double currentTrailingStopPrice = order.StopPrice;
		
		        // Calculate the *desired* new trailing stop price
		        double newTrailingStopPrice = isLong
		            ? currentTrailingStopPrice + tickAdjustment * TickSize  // Increase stop price for longs
		            : currentTrailingStopPrice - tickAdjustment * TickSize;  // Decrease stop price for shorts
		
		        // Prevent moving the stop beyond the current price
		        if ((isLong && newTrailingStopPrice >= currentPrice) || (isShort && newTrailingStopPrice <= currentPrice))
		        {
		            Print("Cannot move stop loss: Beyond current price");
		            continue; // Skip this order
		        }

		        // Calculate the *tick offset from the entry price* for SetTrailStop
		        double tickOffset = isLong
		            ? (entryPrice - newTrailingStopPrice) / TickSize
		            : (newTrailingStopPrice - entryPrice) / TickSize;
		
		
		        // Apply the new trailing stop to ALL relevant orders
		        string[] orderLabels = additionalContractExists ? new[] { "LE", "LE2", "LE3", "LE4", "QLE", "QLE2", "QLE3", "QLE4", "SE", "SE2", "SE3", "SE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "LE", "SE" }; //Corrected
		        foreach (string label in orderLabels)
		        {
		            SetTrailStop(label, CalculationMode.Ticks, tickOffset, true);
		        }
		    }
		}
		
        #endregion
		
		#region Manage Profit Targets
		
		private void ManageProfitTargets()
	    {
	        if (EnableFixedProfit)
	        {
	            SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
	            SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
	            SetProfitTarget(@"QLE", CalculationMode.Ticks, ProfitTarget);
	            SetProfitTarget(@"QSE", CalculationMode.Ticks, ProfitTarget);
	
	            setMultipleProfitTargets();
	        }
	        else if (EnableDynamicProfit)
	        {
	            // Set profit target at each pivot level
	            if (isLong)
	            {
	                if (Close[0] > r3 && Low[0] <= r3)
	                    SetProfitTarget(@"LE", CalculationMode.Price, r3m);
	                else
	                    SetProfitTargetBasedOnLongConditions();
	            }
	
	            if (isShort)
	            {
	                if (Close[0] < s3 && High[0] >= s3)
	                    SetProfitTarget(@"SE", CalculationMode.Price, s3m);
	                else
	                    SetProfitTargetBasedOnShortConditions();
	            }
	        }
	    }
		
		#endregion
		
		#region Appy Trail Stop
		// Method to apply trailing stop
	    private void ApplyTrailStop(double stopValueTicks)
	    {
	        // Create the order labels array based on whether additional contracts exist
	        string[] orderLabels = additionalContractExists ? 
					new[] { "LE", "LE2", "LE3", "LE4", "SE", "SE2", "SE3", "SE4", "QLE", "QLE2", "QLE3", "QLE4",  "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "LE", "SE" };
	
	        // Apply the initial trailing stop for all relevant orders
	        foreach (string label in orderLabels)
	        {
	            SetTrailStop(label, CalculationMode.Ticks, stopValueTicks, true);
//	            setMultipleStopLoss(stopValueTicks);
	        }
	    }
		
		#endregion
		
		#region Apply Fixed Stop
		
		protected void ApplyFixedStopLoss(double stopLoss)
		{
		    if (!isFlat && enableFixedStopLoss)
		    {
		        double stopPrice;
		        string[] orderLabels;
		
		        if (isLong)
		        {
		            // Calculate stop price for long positions in price
		            stopPrice = entryPrice - (stopLoss * TickSize);
		
		            // Create the order labels array based on whether additional contracts exist
		            orderLabels = additionalContractExists ? new[] { "LE", "LE2", "LE3", "LE4", "QLE", "QLE2", "QLE3", "QLE4" } : new[] { "LE" };
		
		            // Apply the initial stop for all relevant orders
		            foreach (string label in orderLabels)
		            {
		                SetStopLoss(label, CalculationMode.Price, stopPrice, true);
		                Print($"{Time[0]}: Set Long Fixed Stop Loss for {label} to {stopPrice}");  // Debug
		                //ExitLongStopMarket(stopPrice, label);  // Remove the ExitLongStopMarket
		            }
		        }
		        else if (isShort)
		        {
		            // Calculate stop price for short positions in price
		            stopPrice = entryPrice + (stopLoss * TickSize);
		
		            // Create the order labels array based on whether additional contracts exist
		            orderLabels = additionalContractExists ? new[] { "SE", "SE2", "SE3", "SE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "SE" };
		
		            // Apply the initial stop for all relevant orders
		            foreach (string label in orderLabels)
		            {
		                SetStopLoss(label, CalculationMode.Price, stopPrice, true);
		                Print($"{Time[0]}: Set Short Fixed Stop Loss for {label} to {stopPrice}");  // Debug
		                //ExitShortStopMarket(stopPrice, label);  // Remove the ExitShortStopMarket
		            }
		        }
		    }
		}
		
		#endregion

		#region DecorateButtons
		
		protected void DecoreDisabledButtons(System.Windows.Controls.Button myButton, string stringButton)
		{
			myButton.Content = stringButton;
			myButton.Background = Brushes.DarkRed;
			myButton.BorderBrush = Brushes.Black;
			myButton.Foreground = Brushes.White;
			return;
		}

		protected void DecorateEnabledButtons(System.Windows.Controls.Button myButton, string stringButton)
		{
			myButton.Content = stringButton;
			myButton.Background = Brushes.DarkGreen;
			myButton.BorderBrush = Brushes.Black;
			myButton.Foreground = Brushes.White;
			return;
		}

		protected void DecorateNeutralButtons(System.Windows.Controls.Button myButton, string stringButton)
		{
			myButton.Content = stringButton;
			myButton.Background = Brushes.LightGray;
			myButton.BorderBrush = Brushes.Black;
			myButton.Foreground = Brushes.Black;
			return;
		}

		protected void DecorateGrayButtons(System.Windows.Controls.Button myButton, string stringButton)
		{
			myButton.Content = stringButton;
			myButton.Background = Brushes.DarkGray;
			myButton.BorderBrush = Brushes.Black;
			myButton.Foreground = Brushes.Black;
			return;
		}		
		
		#endregion		
		
		#region Create WPF Controls
		protected void CreateWPFControls()
		{
			//	ChartWindow
			chartWindow	= System.Windows.Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
			
			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;

			// this is the entire chart trader area grid
			chartTraderGrid			= (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;
			
			// this grid contains the existing chart trader buttons
			chartTraderButtonsGrid	= chartTraderGrid.Children[0] as System.Windows.Controls.Grid;
			
			CreateButtons();

			// this grid is to organize stuff below
			lowerButtonsGrid = new System.Windows.Controls.Grid();
			
			// Initialize
    		InitializeButtonGrid();

			addedRow	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(250) };
			
    		// SetButtons
    		SetButtonLocations();

    		// AddButtons
    		AddButtonsToGrid();			
				
			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;

		}
		#endregion
		
		#region Create Buttons
		protected void CreateButtons()
		{	
					
			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= System.Windows.Application.Current.FindResource("BasicEntryButton") as Style;			
	
			strategyBtn = new System.Windows.Controls.Button
			{		
				Content			= "\uD83D\uDD12 Strategy On", Height = 25, Margin = new Thickness(1,0,1,0),	Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Strategy"
			};	
			if (isStrategyEnabled) DecorateEnabledButtons(strategyBtn, "\uD83D\uDD12 Strategy On");
			if (!isStrategyEnabled) DecoreDisabledButtons(strategyBtn, "\uD83D\uDD13 Strategy Off");
			strategyBtn.Click +=  OnButtonClick;
			
			longBtn = new System.Windows.Controls.Button
			{		
				Content			= "LONG", Height = 25, Margin = new Thickness(1,0,1,0), Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Auto Long Entry"
			};	
			if (isLongEnabled) DecorateEnabledButtons(longBtn, "LONG");
			if (!isLongEnabled) DecoreDisabledButtons(longBtn, "LONG Off");	
			longBtn.Click += OnButtonClick;
			
			shortBtn = new System.Windows.Controls.Button
			{		
				Content			= "SHORT", Height = 25, Margin	= new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Auto Short Entry"
			};	
			if (isShortEnabled) DecorateEnabledButtons(shortBtn, "SHORT");
			if (!isShortEnabled) 	DecoreDisabledButtons(shortBtn, "SHORT Off");	
			shortBtn.Click += OnButtonClick;			

			quickLongBtn = new System.Windows.Controls.Button
			{		
				Content			= "Buy", Height = 25, Margin = new Thickness(1,0,1,0), Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Buy Marktt Entry"
			};	
			DecorateEnabledButtons(quickLongBtn, "Buy");
			quickLongBtn.Click += OnButtonClick;
			
			quickShortBtn = new System.Windows.Controls.Button
			{		
				Content			= "Sell", Height = 25, Margin	= new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Sell Market Entry"
			};	
			DecoreDisabledButtons(quickShortBtn, "Sell");	
			quickShortBtn.Click += OnButtonClick;		

			BEBtn = new System.Windows.Controls.Button
			{		
				Content			= "\uD83D\uDD12 BE On", Height = 25, Margin = new Thickness(1,0,1,0),	Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Auto BE"
			};	
			if (beSetAuto) DecorateEnabledButtons(BEBtn, "\uD83D\uDD12 BE On");
			if (!beSetAuto) DecoreDisabledButtons(BEBtn, "\uD83D\uDD13 BE Off");
			BEBtn.Click +=  OnButtonClick;

			TSBtn = new System.Windows.Controls.Button
			{		
				Content			= "\uD83D\uDD12 TS On", Height = 25, Margin = new Thickness(1,0,1,0),	Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Auto TS"
			};	
			if (enableTrail) DecorateEnabledButtons(TSBtn, "\uD83D\uDD12 TS On");
			if (enableFixedStopLoss) DecoreDisabledButtons(TSBtn, "\uD83D\uDD13 TS Off");
			TSBtn.Click +=  OnButtonClick;	
		
		    // Add Move TS Button
		    moveTSBtn = new System.Windows.Controls.Button
		    {
		        Content = "Move TS",
		        Height = 25,
		        Margin = new Thickness(1, 0, 1, 0),
		        Padding = new Thickness(0, 0, 0, 0),
		        Style = basicButtonStyle,
		        BorderThickness = new Thickness(1.5),
		        IsEnabled = true,
		        Background = Brushes.DarkBlue,
		        Foreground = Brushes.Yellow,
		        ToolTip = "Increase trailing stop"
		    };
		    moveTSBtn.Click += OnButtonClick;
	
			moveTS50PctBtn = new System.Windows.Controls.Button
			{
			    Content = "Move TS 50%", // Update label
			    Height = 25,
			    Margin = new Thickness(1, 0, 1, 0),
			    Padding = new Thickness(0, 0, 0, 0),
			    Style = basicButtonStyle,
			    BorderThickness = new Thickness(1.5),
			    IsEnabled = true,
			    ToolTip = "Move trailing stop 50% closer to the current price",
			    Background = Brushes.DarkBlue, // Background color
			    Foreground = Brushes.Yellow   // Text color
			};
			moveTS50PctBtn.Click += OnButtonClick;				
			
			moveToBEBtn = new System.Windows.Controls.Button
			{
			    Content = "Breakeven", // Update label
			    Height = 25,
			    Margin = new Thickness(1, 0, 1, 0),
			    Padding = new Thickness(0, 0, 0, 0),
			    Style = basicButtonStyle,
			    BorderThickness = new Thickness(1.5),
			    IsEnabled = true,
			    ToolTip = "Move stop to breakeven if in profit",
			    Background = Brushes.DarkBlue, // Background color
			    Foreground = Brushes.White // Adjust text color for contrast
			};
			moveToBEBtn.Click += OnButtonClick;

			add1Btn = new System.Windows.Controls.Button
			{		
				Content			= "Add 1", Height = 25, Foreground = Brushes.White, Background = Brushes.DarkGreen, Margin = new Thickness(1,0,1,0), Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Add 1 contract to open position"
			};	
			add1Btn.Click += OnButtonClick;
			
			close1Btn = new System.Windows.Controls.Button
			{		
				Content			= "Close 1", Height = 25, Foreground = Brushes.White, Background = Brushes.DarkRed, Margin	= new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Close 1 contract from open position"
			};	
			close1Btn.Click += OnButtonClick;


			closeBtn = new System.Windows.Controls.Button
			{
				Name = "closeButton", Content = "Close All Positions", Foreground = Brushes.White, Background = Brushes.DarkRed, Height = 25, Margin = new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Manual Close: CloseAllPosiions manually. Alert!!! Only works with the entries made by the strategy. Manual entries will not be closed from this option."
			};
        	closeBtn.Click += OnButtonClick; 

			panicBtn = new System.Windows.Controls.Button
			{
				Name = "PanicButton", Content = "\u2620 Panic Shutdown", Foreground = Brushes.Yellow, Background = Brushes.DarkRed, Height = 25, Margin = new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "PanicBtn: CloseAllPosiions"
			};
        	panicBtn.Click += OnButtonClick;                     
			
		}	
		
		protected void InitializeButtonGrid()
		{
    		// Create new grid
    		lowerButtonsGrid = new System.Windows.Controls.Grid();

    		// Columns number
    		for (int i = 0; i < 2; i++)
    		{
        		lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
    		}

    		// Row number
    		for (int i = 0; i <= 9; i++)
    		{
        		lowerButtonsGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
    		}
		}				

		protected void SetButtonLocations()
		{
			// Btn, Column, Row, Column span
			
    		SetButtonLocation(strategyBtn, 0, 1, 2);    // Column 0 2 pos
    		SetButtonLocation(longBtn, 0, 2);
    		SetButtonLocation(shortBtn, 1, 2);
   			SetButtonLocation(quickLongBtn, 0, 3);
    		SetButtonLocation(quickShortBtn, 1, 3);    	
   			SetButtonLocation(add1Btn, 0, 4);
    		SetButtonLocation(close1Btn, 1, 4);    		
   			SetButtonLocation(BEBtn, 0, 5);
    		SetButtonLocation(TSBtn, 1, 5); 
		    SetButtonLocation(moveTSBtn, 0, 6);
    		SetButtonLocation(moveTS50PctBtn, 1, 6);  
   			SetButtonLocation(moveToBEBtn, 0, 7, 2);
			SetButtonLocation(closeBtn, 0, 8, 2);
			SetButtonLocation(panicBtn, 0, 9, 2);	
		}		
		
		protected void SetButtonLocation(System.Windows.Controls.Button button, int column, int row, int columnSpan = 1)
		{
    		System.Windows.Controls.Grid.SetColumn(button, column);
    		System.Windows.Controls.Grid.SetRow(button, row);
    
   			if (columnSpan > 1)
        		System.Windows.Controls.Grid.SetColumnSpan(button, columnSpan);
		}		
		
		protected void AddButtonsToGrid()
		{
    		// Add Buttons to grid
    		lowerButtonsGrid.Children.Add(strategyBtn);
    		lowerButtonsGrid.Children.Add(longBtn);
    		lowerButtonsGrid.Children.Add(shortBtn);
    		lowerButtonsGrid.Children.Add(quickLongBtn);
    		lowerButtonsGrid.Children.Add(quickShortBtn);
    		lowerButtonsGrid.Children.Add(add1Btn);
    		lowerButtonsGrid.Children.Add(close1Btn);
    		lowerButtonsGrid.Children.Add(BEBtn);
    		lowerButtonsGrid.Children.Add(TSBtn);  
    		lowerButtonsGrid.Children.Add(moveTSBtn);  
    		lowerButtonsGrid.Children.Add(moveTS50PctBtn);  
    		lowerButtonsGrid.Children.Add(moveToBEBtn);    
			lowerButtonsGrid.Children.Add(closeBtn);
			lowerButtonsGrid.Children.Add(panicBtn);
		}			
		#endregion
		
		#region Buttons Click Events
		
		protected void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
								
			if (button == strategyBtn)
			{	
				isStrategyEnabled = !isStrategyEnabled;
				if (isStrategyEnabled)
				{
					DecorateEnabledButtons(strategyBtn, "\uD83D\uDD12 Strategy On");	
					Print("Strategy: " + isStrategyEnabled);
				} 
				if (!isStrategyEnabled)
				{
					DecoreDisabledButtons(strategyBtn, "\uD83D\uDD13 Strategy Off");
					Print("Strategy: " + isStrategyEnabled);					
				}  
				return;
			}
				
			if (button == longBtn)
			{	
				isLongEnabled = !isLongEnabled;
				if (isLongEnabled){
					DecorateEnabledButtons(longBtn, "LONG");
					Print("Long Enabled " + isLongEnabled);	
				} 
				if (!isLongEnabled)
				{
					DecoreDisabledButtons(longBtn, "LONG Off");	
					Print("Long Disabled " + isLongEnabled);
				}  
				return;
			}			

			if (button == shortBtn)
			{	
				isShortEnabled = !isShortEnabled;
				if (isShortEnabled)
				{
					DecorateEnabledButtons(shortBtn, "SHORT");	
					Print("Short Activated " + isShortEnabled);
				} 
		
				if (!isShortEnabled)
				{
					DecoreDisabledButtons(shortBtn, "SHORT Off");	
					Print("Short Disabled " + isShortEnabled);
				}  
				return;
			}

			if (button == quickLongBtn)
			{	
			//	Code for QuickLong
				Print("State: " + QuickLong);
			
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
				if(QuickLong == false)
				{
					QuickLong = true;
					Print("Buy Market On  ");
					quickLongBtnActive = true;
				}
				
				if(OrderType == OrderType.Market) EnterLong(Convert.ToInt32(Contracts), @"QLE");
				else if (OrderType == OrderType.Limit) EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(), @"QLE");
				else if (OrderType == OrderType.MIT) EnterLongMIT(Convert.ToInt32(Contracts), GetCurrentBid(), @"QLE");
				else if (OrderType == OrderType.StopLimit) EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(), @"QLE");
				else if (OrderType == OrderType.StopMarket) EnterLong(Convert.ToInt32(Contracts), @"QLE");
				enterMultipleLongsTargets(true);
				
				if (enableTrail)
				{
					SetTrailStop(@"QLE", CalculationMode.Ticks, InitialStop, true);
					setMultipleStopLoss(InitialStop);
				}
				if (enableFixedStopLoss)
				{
					SetStopLoss(@"QLE", CalculationMode.Ticks, InitialStop, true);
					setMultipleStopLoss(InitialStop);
				}
				
			//	The following method variation is for experienced programmers who fully understand Advanced Order Handling concepts:
			//	EnterLongLimit(int barsInProgressIndex, bool isLiveUntilCancelled, int quantity, double limitPrice, string signalName)
				
				
				QuickLong		= false;
				runOnce = true;
			//	ForceRefresh();
				return;
			}	

			if (button == quickShortBtn)
			{	
			//	Code for QuickShort
				Print("State: " + QuickShort);
				
				// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
				if(QuickShort == false)
				{
					QuickShort = true;
					Print("Sell Market On  ");
					quickShortBtnActive = true;
				}		
				if (OrderType == OrderType.Market) EnterShort(Convert.ToInt32(Contracts), @"QSE");
				else if (OrderType == OrderType.Limit) EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(), @"QSE");			
				else if (OrderType == OrderType.MIT) EnterShortMIT(Convert.ToInt32(Contracts), GetCurrentAsk(), @"QSE");
				else if (OrderType == OrderType.StopLimit) EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(), @"QSE");
				else if (OrderType == OrderType.StopMarket) EnterShort(Convert.ToInt32(Contracts), @"QSE");
				enterMultipleShortTargets(true);
				
				if (enableTrail)
				{
					SetTrailStop(@"QSE", CalculationMode.Ticks, InitialStop, true);	
					setMultipleStopLoss(InitialStop);
				}
				if (enableFixedStopLoss)
				{
					SetStopLoss(@"QSE", CalculationMode.Ticks, InitialStop, true);
					setMultipleStopLoss(InitialStop);
				}
				
				QuickShort		= false;
				runOnce = true;
			//	ForceRefresh();	
				return;
			}			

			if (button == add1Btn)
			{	
			//	Code for Add 1	
				add1Entry();
			//	ForceRefresh();
				return;
			}

			if (button == close1Btn)
			{	
			//	Code for Close 1
				close1Exit();
			//	ForceRefresh();
				return;
			}
			
			#region BEButton
			if (button == BEBtn)
			{	
				beSetAuto = !beSetAuto;
				if (beSetAuto)
				{
					DecorateEnabledButtons(BEBtn, "\uD83D\uDD12 BE On");	
					//Print("BreakEven: " + beSetAuto);
				} 
				if (!beSetAuto)
				{
					DecoreDisabledButtons(BEBtn, "\uD83D\uDD13 BE Off");
					//Print("BreakEven: " + beSetAuto);					
				}  
				return;
			}
			#endregion		
	
			#region TSButton
			if (button == TSBtn)
			{	
				enableTrail = !enableTrail;
				if (enableTrail)
				{
					DecorateEnabledButtons(TSBtn, "\uD83D\uDD12 TS On");	
					//Print("Trailing Stop: " + enableTrail);
				} 
				if (enableFixedStopLoss)
				{
					DecoreDisabledButtons(TSBtn, "\uD83D\uDD13 TS Off");
					//Print("Trailing Stop: " + enableTrail);					
				}  
				return;
			}
			#endregion	
			
			#region Move Trailing Stop Button
			if (button == moveTSBtn)
		    {
		        AdjustStopLoss(TickMove); 
				ForceRefresh();
		    }
			#endregion
	
			#region Move Trailing Stop 50% Button
			if (button == moveTS50PctBtn)
			{
				MoveTrailingStopByPercentage(0.5);
				ForceRefresh();
			}
			#endregion	
			
			#region Move To Breakeven Button
			
			if (button == moveToBEBtn)
			{
				MoveToBreakeven();
				ForceRefresh();
			    return;
			}
			#endregion
			
			if (button == closeBtn)
			{	
				CloseAllPositions();
				ForceRefresh();
				return;
			}
				
			if (button == panicBtn)
			{	
				FlattenAllPositions();
				ForceRefresh();
				return;
			}
		}
		
		#endregion
		
		#region MoveToBreakeven
		protected void MoveToBreakeven()
		{
		    //'Ensure an active position exists
		    if (!isFlat)
		    {
		        // Calculate the actual profit/loss in ticks
				actualPnL = Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks, Close[0]);
			
				// Determine if breakeven conditions are met
				if (actualPnL >= BE_Offset) 
				{
					// Set the trail stop in ticks
					trailStop = entryPrice / TickSize + (isLong? 1 : -1) * BE_Offset;
			
			        // Create the order tags array based on whether additional contracts exist
			        string[] orderTags = additionalContractExists ? new[] 
						{ "LE", "LE2", "LE3", "LE4", "SE", "SE2", "SE3", "SE4", "QLE", "QLE2", "QLE3", "QLE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "LE", "SE" };
			
			        // Apply the trailing stop to all relevant tags
			        foreach (string tag in orderTags)
			        {
						SetTrailStop(tag, CalculationMode.Ticks, trailStop, true);
			        }
			
					// Mark breakeven as realized
					_beRealized = true;
				}
		    }
		}
		#endregion
	
		#region Move Trailing Stop 50% 
		protected void MoveTrailingStopByPercentage(double percentage)
		{
		    Print("Move TS 50% button clicked.");
		    if (isFlat)
		    {
		        Print("No active position to move trailing stop.");
		        return;
		    }
		
		    double entryPrice = Position.AveragePrice;
		    bool isLong = Position.MarketPosition == MarketPosition.Long;
		    double currentPrice = Close[0];
		
		    // Get all active stop orders
		    List<Order> stopOrders = new List<Order>();
		    foreach (var order in Orders)
		    {
		        if ((order.OrderState == OrderState.Working || order.OrderState == OrderState.TriggerPending) && order.OrderTypeString == "Stop Market" && order.StopPrice > 0)
		        {
		            stopOrders.Add(order);
		        }
		    }

		    if (stopOrders.Count == 0)
		    {
		        // No stop orders found, calculate new stop based on INITIAL stop and move that percentage closer to the current price
		        double currentTrailingStopPrice = isLong
		            ? entryPrice - InitialStop * TickSize
		            : entryPrice + InitialStop * TickSize;
		
		        // Calculate the distance between the current price and the current stop
		        double distance = Math.Abs(currentPrice - currentTrailingStopPrice);
		
		        // Calculate the amount to move the stop
		        double moveAmount = distance * percentage;
		
		        // Calculate the new trailing stop price
		        double newTrailingStopPrice = isLong
		            ? currentTrailingStopPrice + moveAmount
		            : currentTrailingStopPrice - moveAmount;
		
		        // Check if the new stop price exceeds current price
		        if ((isLong && newTrailingStopPrice >= currentPrice) || (isShort && newTrailingStopPrice <= currentPrice))
		        {
		            Print("Cannot move stop loss: New stop price exceeds current price.");
		            return;
		        }

		        // Calculate the tick offset from the entry price
		        double tickOffset = isLong
		            ? (entryPrice - newTrailingStopPrice) / TickSize
		            : (newTrailingStopPrice - entryPrice) / TickSize;
		
		        // Apply the new trailing stop using the calculated tick offset
		        ApplyTrailStop(tickOffset);
		        return;
		    }
		
		    // Adjust existing stop orders, moving the stop percentage closer to current price
		    foreach (var order in stopOrders)
		    {
		        double currentTrailingStopPrice = order.StopPrice;
		
		        // Calculate the distance between the current price and the current stop
		        double distance = Math.Abs(currentPrice - currentTrailingStopPrice);
		
		        // Calculate the amount to move the stop
		        double moveAmount = distance * percentage;
		
		        // Calculate the new trailing stop price
		        double newTrailingStopPrice = isLong
		            ? currentTrailingStopPrice + moveAmount
		            : currentTrailingStopPrice - moveAmount;

		        // Check if the new stop price exceeds current price
		        if ((isLong && newTrailingStopPrice >= currentPrice) || (isShort && newTrailingStopPrice <= currentPrice))
		        {
		            Print("Cannot move stop loss: New stop price exceeds current price.");
		            continue;  // Skip this order
		        }
		
		        // Calculate the tick offset from the entry price
		        double tickOffset = isLong
		            ? (entryPrice - newTrailingStopPrice) / TickSize
		            : (newTrailingStopPrice - entryPrice) / TickSize;
		
		        // Apply the new trailing stop to all relevant orders using calculated tick offset
		        string[] orderLabels = additionalContractExists ? new[] { "LE", "LE2", "LE3", "LE4", "QLE", "QLE2", "QLE3", "QLE4", "SE", "SE2", "SE3", "SE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "LE", "SE" };
		        foreach (string label in orderLabels)
		        {
		            SetTrailStop(label, CalculationMode.Ticks, tickOffset, true);
		        }
		    }
		}
		#endregion
		
		#region Dispose
		protected void DisposeWPFControls() 
		{
			
			
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			if (strategyBtn != null)
				strategyBtn.Click -= OnButtonClick;
						
			if (longBtn != null)
				longBtn.Click -= OnButtonClick;

			if (shortBtn != null)
				shortBtn.Click -= OnButtonClick;			

			if (quickLongBtn != null)
				quickLongBtn.Click -= OnButtonClick;

			if (quickShortBtn != null)
				quickShortBtn.Click -= OnButtonClick;	

			if (add1Btn != null)
				add1Btn.Click -= OnButtonClick;

			if (close1Btn != null)
				close1Btn.Click -= OnButtonClick;				

			if (BEBtn != null)
				BEBtn.Click -= OnButtonClick;

			if (TSBtn != null)
				TSBtn.Click -= OnButtonClick;
			
			if (moveTSBtn != null)
				moveTSBtn.Click -= OnButtonClick;	

			if (moveTS50PctBtn != null)
				moveTS50PctBtn.Click -= OnButtonClick;
			
			if (moveToBEBtn != null)
				moveToBEBtn.Click -= OnButtonClick;
			
			if (closeBtn != null)
				closeBtn.Click -= OnButtonClick;
			
			if (panicBtn != null)
				panicBtn.Click -= OnButtonClick;	
			
			RemoveWPFControls();
			
			
		}
		#endregion
		
		#region Insert WPF
		public void InsertWPFControls()
		{
			if (panelActive)
				return;
			
			// add a new row (addedRow) for our lowerButtonsGrid below the ask and bid prices and pnl display			
			chartTraderGrid.RowDefinitions.Add(addedRow);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid, (chartTraderGrid.RowDefinitions.Count - 1));
			chartTraderGrid.Children.Add(lowerButtonsGrid);

			panelActive = true;
		}
		#endregion
		
		#region Remove WPF
		protected void RemoveWPFControls()
		{
			if (!panelActive)
				return;
			
			if (chartTraderButtonsGrid != null || lowerButtonsGrid != null)
			{
				chartTraderGrid.Children.Remove(lowerButtonsGrid);
				chartTraderGrid.RowDefinitions.Remove(addedRow);
			}

			panelActive = false;
		}
		#endregion
		
		#region TabSelcected 
		protected bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}
		#endregion
		
		#region TabHandler
		protected void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}		
		#endregion	

		#region Close All Positions
		protected void CloseAllPositions()
		{
		//	Close actual position manually
        //	Check if there is an open position
			Print("Position Closing");
			
			if(isLong) 
			{
				// Create the order labels array based on whether additional contracts exist
		        string[] orderLabels = additionalContractExists ? new[] { "LE", "LE2", "LE3", "LE4", "QLE", "QLE2", "QLE3", "QLE4" } : new[] { "LE" };
		
		        // Apply the initial trailing stop for all relevant orders
		        foreach (string label in orderLabels)
		        {
		            ExitLong("Manual Exit", label);
		        }
			}
			else if(isShort) 
			{
				// Create the order labels array based on whether additional contracts exist
		        string[] orderLabels = additionalContractExists ? new[] { "SE", "SE2", "SE3", "SE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "SE" };
		
		        // Apply the initial trailing stop for all relevant orders
		        foreach (string label in orderLabels)
		        {
		            ExitShort("Manual Exit", label);
		        }
			}		
		}	
		
        protected void FlattenAllPositions()
        {
			//  Access the open position
        	Position openPosition = Position;
			Account myAccount;
			AccountSelector accountSelector = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlAccountSelector") as AccountSelector;
			this.chartTraderAccount = ((accountSelector != null) ? accountSelector.SelectedAccount : null);
			this.accountSelector = ((accountSelector != null) ? accountSelector : null);
			
			// Get the account (replace "Sim101" with your actual account name)
            myAccount = Account.All.FirstOrDefault((Account a) => a.Name == this.chartTraderAccount.DisplayName);
			Print("Account selected: " + this.chartTraderAccount.DisplayName);
            if (myAccount == null) Print("Account selected: " + this.chartTraderAccount.DisplayName + " Account not found !!!");
			if (myAccount == null)
			     throw new Exception("Account not found.");
			
        	if (openPosition != null && openPosition.MarketPosition != MarketPosition.Flat)
        	{
			// Less drastic method, we make a Flatten All to the account used in the strategy and to the instrument that we have loaded on the chart
				List<Instrument> instrumentNames = new List<Instrument>();
				foreach (Position position in this.chartTraderAccount.Positions)
	            {
	              Instrument instrument = position.Instrument;
	              if (!instrumentNames.Contains(instrument))
	                instrumentNames.Add(instrument);
	            }
	            this.chartTraderAccount.Flatten((ICollection<Instrument>) instrumentNames);
        	}		
		}

		protected void add1Entry()
		{
		//	Code for add1Entry()
		//	Add 1  Check value of open Position and EntriesPerDirection
	    	int additionalContracts = 1; // Contracts to add
		//  Access the open position
        	Position openPosition = Position;
			double currentPosition = openPosition.Quantity;
        	if (openPosition != null && openPosition.MarketPosition != MarketPosition.Flat)
        	{
				currentPosition = Position.Quantity;
				if (currentPosition + additionalContracts <= EntriesPerDirection)
					AddContractToOpenPosition();
			}	

			return;	
		}	

		protected void close1Exit()
		{
		//	Code for close1Exit()
		//	Close 1 Check value of open Position and EntriesPerDirection
			int additionalContracts = 1; // Contracts to close
        	Position openPosition = Position;
			double currentPosition = openPosition.Quantity;
        	if (openPosition != null && openPosition.MarketPosition != MarketPosition.Flat)
        	{
				currentPosition = Position.Quantity;
				if (currentPosition > additionalContracts)
					CloseOneContractFromPosition();
			}		

			return;	
		}	
		
		protected void AddContractToOpenPosition()
		{   // Add 1
			int additionalContracts = 1;
		    try
		    {
				if(isLong) {
					if (!quickLongBtnActive)
					{	
						EnterLong(additionalContracts, @"LE");	
//						enterMultipleLongsTargets(false);
					}
					if (quickLongBtnActive)
					{	
						EnterLong(additionalContracts, @"QLE");
//						enterMultipleLongsTargets(true);
						
					}					
				
				}else if(isShort) {
					if (!quickShortBtnActive)
					{	
						EnterShort(additionalContracts, @"SE");
//						enterMultipleShortTargets(false);
						
					//	if(OrderType == OrderType.Market) EnterShort(additionalContracts, @"SE");
					//	if(!OrderType == OrderType.Market) EnterShortLimit(additionalContracts, GetCurrentAsk(0), @"SE");	
					}	
					if (quickShortBtnActive)
					{	
						EnterShort(additionalContracts, @"QSE");
//						enterMultipleShortTargets(true);
						
					//	if(OrderType == OrderType.Market) EnterShort(additionalContracts, @"QSE");
					//	if(!OrderType == OrderType.Market) EnterShortLimit(additionalContracts, GetCurrentAsk(0), @"QSE");
					}						
				}	
		        else {
		            Print("No open position to close contracts from.");
		        }
		    }
		    catch (Exception ex)
		    {
		        Print($"Failed to add contracts due to: {ex.Message}");
		    }
		}
		
		protected void CloseOneContractFromPosition()
		{	// Close 1
		    int contractsToClose = 1; // Number of contracts to close && check  EntriesPerDirection
		    try
		    {
				checkOrder();
//				if (myStopOrder != null) myStopPrice = myStopOrder.StopPrice;
//				if (myTargetOrder != null) myLimitPrice = myTargetOrder.LimitPrice;
				if(isLong) 
				{
					// Create the order labels array based on whether additional contracts exist
			        string[] orderLabels = additionalContractExists ? 
							new[] { "LE", "LE2", "LE3", "LE4", "QLE", "QLE2", "QLE3", "QLE4" } : new[] { "LE" };
			
			        // Apply the initial trailing stop for all relevant orders
			        foreach (string label in orderLabels)
			        {
			            ExitLong(0, contractsToClose,  "Close1 Exit", label);	
			        }				
				
				}else if(isShort) 
				{
					// Create the order labels array based on whether additional contracts exist
			        string[] orderLabels = additionalContractExists ? 
							new[] { "SE", "SE2", "SE3", "SE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "SE" };
			
			        // Apply the initial trailing stop for all relevant orders
			        foreach (string label in orderLabels)
			        {
			            ExitShort(0, contractsToClose,  "Close1 Exit", label);
			        }
				}	
		        else {
		            Print("No open position to close contracts from.");
		        }
		    }
		    catch (Exception ex)
		    {
		        Print($"Failed to close contracts due to: {ex.Message}");
		    }
			Print($"{Times[0][0].TimeOfDay} Leaving Close 1.  StopPrice:  {myStopPrice}   LimitPrice  {myLimitPrice}    orderQuantity {Position.Quantity}");
		}


		
		protected void checkPositions()
		{
		//	Detect unwanted Positions opened (possible rogue Order?)
	        double currentPosition = Position.Quantity; // Get current position quantity
		
			if (isFlat)
			{
		        foreach (var order in Orders)
		        {
		            if (order != null) CancelOrder(order);
		        }				
			}
		}	
		
		protected void checkOrder()
		{
		// Verify one active order and set myStopPrice and mylimitPrice to be use in changing orders when add or close 1 contracts to open positions
			activeOrder = false;
			
			if (Orders.Count != 0)
			{
				Print($"{Times[0][0].TimeOfDay} ACTIVE Orders Count:  {Orders.Count}");
				foreach (var order in Orders)
		        {
					string entrySignal = order.FromEntrySignal;
					Print($"{Times[0][0].TimeOfDay} myOrder NOT null {order.OrderId}  StopPrice:  {order.StopPrice}   LimitPrice  {order.LimitPrice}    orderQuantity {order.Quantity}   tiene el estado: {order.OrderState}  y es del tipo {order.OrderTypeString}    FROM EntrySignal {entrySignal}");
		            // Verificar el estado de cada orden
					if (order.OrderState == OrderState.Filled)
		            {
		                myEntryOrder = order;
						if (order.IsStopMarket && entrySignal != "Add 1")
						{
							myStopOrder = order;
							myStopPrice = myStopOrder.StopPrice;
						}	
						if (order.IsLimit &&  entrySignal != "Add 1") 
						{
							myLimitPrice = myEntryOrder.LimitPrice;
							
						}	
		            }					
					else if (order.OrderState == OrderState.TriggerPending && entrySignal != "Add 1")
		            {
		                if (order.IsStopMarket)
						{
							myStopOrder = order;
							myStopPrice = myStopOrder.StopPrice;
						}
		            }
					else if (order.OrderState == OrderState.Working && entrySignal != "Add 1")
		            {						
						if (order.IsLimit)
						{ 
							myTargetOrder = order;
							myLimitPrice = myTargetOrder.LimitPrice;	
						}	
		            }					
		            else
		            {
		                Print("La orden " + order.OrderId + " tiene el estado: " + order.OrderState);
		            }							
		        }
				Print($"{Times[0][0].TimeOfDay} myEntryOrder NOT null {myEntryOrder.OrderId}  StopPrice:  {myEntryOrder.StopPrice}   LimitPrice  {myEntryOrder.LimitPrice}    orderQuantity {myEntryOrder.Quantity}   tiene el estado: {myEntryOrder.OrderState}  y es del tipo {myEntryOrder.OrderTypeString}");
				activeOrder = true;
			}
		}
		
		protected bool checkTimers()
		{
		//	check we are in timer	
			if((Times[0][0].TimeOfDay >= Start.TimeOfDay) && (Times[0][0].TimeOfDay < End.TimeOfDay) 
					|| (Time2 && Times[0][0].TimeOfDay >= Start2.TimeOfDay && Times[0][0].TimeOfDay <= End2.TimeOfDay)
					|| (Time3 && Times[0][0].TimeOfDay >= Start3.TimeOfDay && Times[0][0].TimeOfDay <= End3.TimeOfDay)
					|| (Time4 && Times[0][0].TimeOfDay >= Start4.TimeOfDay && Times[0][0].TimeOfDay <= End4.TimeOfDay)
					|| (Time5 && Times[0][0].TimeOfDay >= Start5.TimeOfDay && Times[0][0].TimeOfDay <= End5.TimeOfDay)
					|| (Time6 && Times[0][0].TimeOfDay >= Start6.TimeOfDay && Times[0][0].TimeOfDay <= End6.TimeOfDay)
			)
			{
				return true;
			}
			else
			{
				return false;
			}			
		}
		
		protected string GetActiveTimer()
		{
		//	check active timer	
		    TimeSpan currentTime = Times[0][0].TimeOfDay;
		
		    if ((Times[0][0].TimeOfDay >= Start.TimeOfDay) && (Times[0][0].TimeOfDay < End.TimeOfDay))
		    {
		        return $"{Start:HH\\:mm} - {End:HH\\:mm}";
		    }
		    else if (Time2 && Times[0][0].TimeOfDay >= Start2.TimeOfDay && Times[0][0].TimeOfDay <= End2.TimeOfDay)
		    {
		        return $"{Start2:HH\\:mm} - {End2:HH\\:mm}";
		    }
		    else if (Time3 && Times[0][0].TimeOfDay >= Start3.TimeOfDay && Times[0][0].TimeOfDay <= End3.TimeOfDay)
		    {
		        return $"{Start3:HH\\:mm} - {End3:HH\\:mm}";
		    }
		    else if (Time4 && Times[0][0].TimeOfDay >= Start4.TimeOfDay && Times[0][0].TimeOfDay <= End4.TimeOfDay)
		    {
		        return $"{Start4:HH\\:mm} - {End4:HH\\:mm}";
		    }
		    else if (Time5 && Times[0][0].TimeOfDay >= Start5.TimeOfDay && Times[0][0].TimeOfDay <= End5.TimeOfDay)
		    {
		        return $"{Start5:HH\\:mm} - {End5:HH\\:mm}";
		    }
		    else if (Time6 && Times[0][0].TimeOfDay >= Start6.TimeOfDay && Times[0][0].TimeOfDay <= End6.TimeOfDay)
		    {
		        return $"{Start6:HH\\:mm} - {End6:HH\\:mm}";
		    }
		
		    return "No active timer";
		}
		
		#endregion				
		
		#region DrawPnl
		protected void showPNLStatus() {
			textLine0 = "Active Timer";
			textLine1 = GetActiveTimer();
			textLine2 = "Long Per Direction";
			textLine3 = $"{counterLong} / {longPerDirection} | " + (TradesPerDirection ? "On" : "Off");
			textLine4 = "Short Per Direction";
			textLine5 = $"{counterShort} / {shortPerDirection} | " + (TradesPerDirection ? "On" : "Off");
			textLine6 = "Bars Since Exit ";
			textLine7 = $"{iBarsSinceExit}    |    " + (iBarsSinceExit > 1 ?  "On" : "Off");
			string statusPnlText = textLine0 + "\t" + textLine1 + "\n" + textLine2 + "  " + textLine3 + "\n" + textLine4 + "  " + textLine5+ "\n" + textLine6 + "\t";
			SimpleFont font = new SimpleFont("Arial", 18);
			
			Draw.TextFixed(this, "statusPnl", statusPnlText, PositionPnl, colorPnl, font, Brushes.Transparent, Brushes.Transparent, 0);
								
		}
		#endregion			
		
		#region Discord Signal
		private async Task SendSignalToDiscordAsync(string direction, double entryPrice, double stopLoss, double profitTarget, DateTime entryTime)
		{
		    try
		    {
		        // Check rate limit
		        if (DateTime.Now - lastDiscordMessageTime < discordRateLimitInterval)
		        {
		            Print("Skipping Discord signal due to rate limit.");
		            return;
		        }
		
		        // Update the last sent time
		        lastDiscordMessageTime = DateTime.Now;
		
		        // Create the embed message for Discord
		        var fields = new List<object>
		        {
		            new { name = "Direction", value = direction, inline = true },
		            new { name = "Entry Price", value = entryPrice.ToString("F2"), inline = true },
		            new { name = "Stop Loss", value = stopLoss.ToString("F2"), inline = true },
		            new { name = "Profit Target", value = profitTarget.ToString("F2"), inline = true },
		            new { name = "Time", value = entryTime.ToString("HH:mm:ss"), inline = false }
		        };
		
		        var embed = new
		        {
		            title = $"Trade Signal: {direction}",
		            color = direction.Contains("LONG") ? 3066993 : 15158332, // Green for long, Red for short
		            fields = fields
		        };
		
		        using (var client = new HttpClient())
		        {
		            var payload = new { username = "Trading Bot", embeds = new[] { embed } };
		            var json = new JavaScriptSerializer().Serialize(payload);
		            var content = new StringContent(json, Encoding.UTF8, "application/json");
		
		            var webhookUrl = DiscordWebhooks;
		
		            var response = await client.PostAsync(webhookUrl, content);
		
		            if (response.IsSuccessStatusCode)
		            {
		                Print($"Discord Signal sent: {direction} - Time: {entryTime:HH:mm:ss}");
		            }
		            else
		            {
		                Print($"Discord Signal failed: {response.StatusCode} {response.ReasonPhrase}");
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        Print($"Error sending Discord Signal: {ex.Message}");
		    }
		}		
		#endregion		
		
		#region Entry Signals & Inits
		
		protected abstract bool ValidateEntryLong(); 
        	
		// protected abstract bool CheckLongEntryConditions();	
		
        protected abstract bool ValidateEntryShort();

		// protected abstract bool CheckShortEntryConditions();	
		
        protected virtual bool ValidateExitLong() {
			return false;
		}

        protected virtual bool ValidateExitShort() {
			return false;
		}
		
		protected abstract void InitializeIndicators();		
		
		protected virtual void addDataSeries() {}
		
		#endregion
		
		#region Daily PNL
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			if (isFlat && SystemPerformance.AllTrades.Count > 0)
			{
//				PositionPnl = TextPosition.BottomLeft;
//				totalPnL = 0; //backtest
			
				totalPnL = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit; ///Double that sets the total PnL 

				dailyPnL = (totalPnL) - (cumPnL); ///Your daily limit is the difference between these
				
				
				if (dailyPnL <= -DailyLossLimit) //Print this when daily Pnl is under Loss Limit
				{
					
					Print("Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + dailyPnL + " <<" +  Time[0]);
					
					Text myTextLoss = Draw.TextFixed(this, "loss_text", "Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + "$" + totalPnL + " <<", PositionDailyPNL, colorDailyProfitLoss, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextLoss.Font = new SimpleFont("Arial", 18) {Bold = true };

				}				
				
				if (dailyPnL >= DailyProfitLimit) //Print this when daily Pnl is above Profit limit
				{
					
					Print("Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" +  dailyPnL + " <<" + Time[0]);
					
					Text myTextProfit = Draw.TextFixed(this, "profit_text", "Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" + "$" +  totalPnL + " <<", PositionDailyPNL, colorDailyProfitLoss, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextProfit.Font = new SimpleFont("Arial", 18) {Bold = true };	
				}
			}	
			
			if (isFlat)	checkPositions(); // Detect unwanted Positions opened (possible rogue Order?)						
		}
		
		#endregion		
		
		#region Draw Strategy Pnl
		//Draw pnl
		protected void DrawStrategyPnl(ChartControl chartControl) {
	
			if (!restartPnL) {
			    // Mode normal
			    if (syncPnl) {
					dif = historicalTimeTrades - getCumProfit();
			    } else {
			        cumProfit = getCumProfit() + dif;
			    }
			}else {
			    // Mode restartPnL
				dif = historicalTimeTrades - getCumProfit();
				if(getCumProfit() == 0){	//Reset starts negative so we start it at zero.
					cumProfit = 0;
				}else{
			    	cumProfit = getCumProfit() - dif;
				}
			}
			
			double unrealizedProfitLoss = Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
			string Total = (cumProfit + unrealizedProfitLoss).ToString("N0");
			
			colorDailyProfitLoss = totalPnL == 0 ? Brushes.Cyan: totalPnL > 0 ? Brushes.Lime : Brushes.Red;
															
			string textLine0 = Account.Name + " | " + Account.Connection.Options.Name;
			string textLine1 = "Total PNL: ";
			string textLine2 = "$" + Total;
			string textLine3 = "Realized PNL: ";
			string textLine4 = "$" + cumProfit.ToString("N0");
			string textLine5 = "Unrealized PNL: ";
			
			string formattedPnL = unrealizedProfitLoss.ToString("N0");
			string textLine6 = "$" + formattedPnL;

			string realTimeTradeText = textLine0 + "\n" + textLine1 + "\t" + textLine2 + "\n" + textLine3 + "\t" + textLine4 + "\n" + textLine5+ "\t" + textLine6;
			SimpleFont font = new SimpleFont("Arial", 18);
			
			Draw.TextFixed(this, "realTimeTradeText", realTimeTradeText, PositionDailyPNL, colorDailyProfitLoss, font, Brushes.Transparent, Brushes.Transparent, 0);					
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);	
			if (showDailyPnl) DrawStrategyPnl(chartControl);
		}
		
		protected double getCumProfit() {
			TradeCollection realTimeTrades = SystemPerformance.RealTimeTrades;
			return realTimeTrades.TradesPerformance.Currency.CumProfit;
		}
		
		#endregion	
				
		#region KillSwitch
		protected void KillSwitch() {
			
			totalPnL = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit; 
			dailyPnL = totalPnL + Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
			
			double maxProfit = Math.Max(-10000, totalPnL);
			
			// Create the order labels array based on whether additional contracts exist
	        string[] longOrderLabels = additionalContractExists ? 
					new[] { "LE", "LE2", "LE3", "LE4", "QLE", "QLE2", "QLE3", "QLE4" } : new[] { "LE" };
	
			string[] shortOrderLabels = additionalContractExists ? 
					new[] { "SE", "SE2", "SE3", "SE4", "QSE", "QSE2", "QSE3", "QSE4" } : new[] { "SE" };
			
			if (totalPnL > StartTrailingDD && maxProfit - totalPnL >= TrailingDrawdown)
			{
				foreach (string label in longOrderLabels)
		        {
		            ExitLong(Convert.ToInt32(Position.Quantity), @"LongExitMaxDD", label);
		        }
			
				foreach (string label in shortOrderLabels)
		        {
		            ExitShort(Convert.ToInt32(Position.Quantity), @"ShortExitMaxDD", label);
		        }
				
				trailingDrawdownReached = true;
			
				isStrategyEnabled = false;
			}
			
			if (dailyPnL <= -DailyLossLimit)
			{
				foreach (string label in longOrderLabels)
		        {
		            ExitLong(Convert.ToInt32(Position.Quantity), @"LongExitMaxSL", label);
		        }
			
				foreach (string label in shortOrderLabels)
		        {
		            ExitShort(Convert.ToInt32(Position.Quantity), @"ShortExitMaxSL", label);
		        }
			
				isStrategyEnabled = false;
			}				
			
			if (dailyPnL >= DailyProfitLimit)
			{
				foreach (string label in longOrderLabels)
		        {
		            ExitLong(Convert.ToInt32(Position.Quantity), @"LongExitMaxTP", label);
		        }
			
				foreach (string label in shortOrderLabels)
		        {
		            ExitShort(Convert.ToInt32(Position.Quantity), @"ShortExitMaxTP", label);
		        }
			
				isStrategyEnabled = false;
			}
		}
		#endregion
		
		#region Custom Property Manipulation	
		
		public void ModifyProperties(PropertyDescriptorCollection col)
        {
			if (TradesPerDirection == false)
            {
				col.Remove(col.Find("longPerDirection", true));
				col.Remove(col.Find("shortPerDirection", true));
            }
			if (Time2 == false)
            {
				col.Remove(col.Find("Start2", true));
				col.Remove(col.Find("End2", true));
            }
			if (Time3 == false)
            {
				col.Remove(col.Find("Start3", true));
				col.Remove(col.Find("End3", true));
            }
			if (Time4 == false)
            {
				col.Remove(col.Find("Start4", true));
				col.Remove(col.Find("End4", true));
            }
			if (Time5 == false)
            {
				col.Remove(col.Find("Start5", true));
				col.Remove(col.Find("End5", true));
            }
			if (Time6 == false)
            {
				col.Remove(col.Find("Start6", true));
				col.Remove(col.Find("End6", true));
            }
		}
		
		public void ModifyBESetAutoProperties(PropertyDescriptorCollection col) {
			if (showctrlBESetAuto == false) {
				col.Remove(col.Find("BE_Trigger", true));
				col.Remove(col.Find("BE_Offset", true));
			}
		}		

		public void ModifyEnableTypeProfitProperties(PropertyDescriptorCollection col) {
			if (showctrlEnableDynamicProfit) {	
				col.Remove(col.Find("EnableFixedProfit", true));
				col.Remove(col.Find("EnableProfitTarget2", true));
				col.Remove(col.Find("ProfitTarget2", true));
				col.Remove(col.Find("EnableProfitTarget3", true));
				col.Remove(col.Find("ProfitTarget3", true));
				col.Remove(col.Find("EnableProfitTarget3", true));
				col.Remove(col.Find("ProfitTarget4", true));
				col.Remove(col.Find("EnableProfitTarget4", true));
				col.Remove(col.Find("Contracts2", true));
				col.Remove(col.Find("Contracts3", true));
				col.Remove(col.Find("Contracts4", true));
			
			}
			if (showctrlEnableFixedProfit) { col.Remove(col.Find("EnableDynamicProfit", true));}
		
		}	
		
		public void ModifyTrailProperties(PropertyDescriptorCollection col) {
			if (showTrailOptions == false) {
				col.Remove(col.Find("TrailSetAuto", true));
				col.Remove(col.Find("atrPeriod", true));
				col.Remove(col.Find("atrMultiplier", true));
				col.Remove(col.Find("RiskRewardRatio", true));
				col.Remove(col.Find("Trail_Frequency", true));
				col.Remove(col.Find("TrailByThreeStep", true));
				col.Remove(col.Find("threeStepTrail", true));
				col.Remove(col.Find("step1ProfitTrigger", true));
				col.Remove(col.Find("step2ProfitTrigger", true));
				col.Remove(col.Find("step3ProfitTrigger", true));
				col.Remove(col.Find("step1StopLoss", true));
				col.Remove(col.Find("step2StopLoss", true));
				col.Remove(col.Find("step3StopLoss", true));
				col.Remove(col.Find("step1Frequency", true));
				col.Remove(col.Find("step2Frequency", true));
				col.Remove(col.Find("step3Frequency", true));				
			}
		}	
		
		public void ModifyTrailStopTypeProperties(PropertyDescriptorCollection col) {
		//	if (SystemPrint) Print("showAtrTrailOptions "+showAtrTrailOptions);
		//	if (SystemPrint) Print("showATRTrailOptions "+ showATRTrailOptions);
		//	if (SystemPrint) Print("showThreeStepTrailOptions "+showThreeStepTrailOptions);
			if (showAtrTrailOptions == false) {
				col.Remove(col.Find("TrailSetAuto", true));
				col.Remove(col.Find("atrPeriod", true));
				col.Remove(col.Find("atrMultiplier", true));
				col.Remove(col.Find("RiskRewardRatio", true));
				col.Remove(col.Find("Trail_Frequency", true));
			} 
			if (showThreeStepTrailOptions == false) {
			//	if (SystemPrint) Print("Remove Trail By ThreeStep");
				col.Remove(col.Find("threeStepTrail", true));
				col.Remove(col.Find("step1ProfitTrigger", true));
				col.Remove(col.Find("step2ProfitTrigger", true));
				col.Remove(col.Find("step3ProfitTrigger", true));
				col.Remove(col.Find("step1StopLoss", true));
				col.Remove(col.Find("step2StopLoss", true));
				col.Remove(col.Find("step3StopLoss", true));				
				col.Remove(col.Find("step1Frequency", true));
				col.Remove(col.Find("step2Frequency", true));
				col.Remove(col.Find("step3Frequency", true));				
			}
		}
		
		public void ModifyTrailSetAutoProperties(PropertyDescriptorCollection col) {
			if (showAtrTrailSetAuto == false) {
				col.Remove(col.Find("atrPeriod", true));
				col.Remove(col.Find("atrMultiplier", true));
				col.Remove(col.Find("RiskRewardRatio", true));
				col.Remove(col.Find("Trail_frequency", true));
			}
		}			

		public void ModifyThreeStepTrailSetAutoProperties(PropertyDescriptorCollection col) {
			if (threeStepTrail == false) {
				col.Remove(col.Find("step1ProfitTrigger", true));
				col.Remove(col.Find("step2ProfitTrigger", true));
				col.Remove(col.Find("step3ProfitTrigger", true));
				col.Remove(col.Find("step1StopLoss", true));
				col.Remove(col.Find("step2StopLoss", true));
				col.Remove(col.Find("step3StopLoss", true));				
				col.Remove(col.Find("step1Frequency", true));
				col.Remove(col.Find("step2Frequency", true));
				col.Remove(col.Find("step3Frequency", true));				
			}
		}
		
		#endregion
		
		#region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(GetType());
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(GetType());
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(GetType());
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(GetType());
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(GetType());
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(GetType());
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(GetType(), editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(GetType(), attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(GetType());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection orig = TypeDescriptor.GetProperties(GetType(), attributes);
            PropertyDescriptor[] arr = new PropertyDescriptor[orig.Count];
            orig.CopyTo(arr, 0);
            PropertyDescriptorCollection col = new PropertyDescriptorCollection(arr);

            ModifyProperties(col);
			ModifyBESetAutoProperties(col);

			ModifyTrailProperties(col);
			ModifyTrailStopTypeProperties(col);
			ModifyTrailSetAutoProperties(col);
			ModifyEnableTypeProfitProperties(col);	
			ModifyThreeStepTrailSetAutoProperties(col);			
			
            return col;

        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(GetType());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
		#endregion		
        
		#region Properties

		#region 01. Release Notes
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name="BaseAlgoVersion", Order=1, GroupName="01. Release Notes")]
		public string BaseAlgoVersion
		{ get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name="Author", Order=2, GroupName="01. Release Notes")]
		public string Author
		{ get; set; }		
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
//		[ReadOnly(true)]
		[Display(Name="StrategyName", Order=3, GroupName="01. Release Notes")]
		public string StrategyName
		{ get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
//		[ReadOnly(true)]
		[Display(Name="Version", Order =4, GroupName="01. Release Notes")]
		public string Version
		{ get; set; }
		
		[ReadOnly(true)]
		[NinjaScriptProperty]
//		[ReadOnly(true)]
		[Display(Name="Credits", Order=5, GroupName="01. Release Notes")]
		public string Credits
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Chart Type", Order=6, GroupName="01. Release Notes")]
		public string ChartType
		{ get; set; }
		
		#endregion
		
		#region 02. Order Settings	
		
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)]	
		[Display(Name="Enable Fixed Profit", Order=1, GroupName="02. Order Settings")]
		public bool EnableFixedProfit
		{ 	get{
				return enableFixedProfit;
			} 
			set {
				enableFixedProfit = value;
				
				if (enableFixedProfit == true) {
					showctrlEnableDynamicProfit = false;
					showctrlEnableFixedProfit = true;
					enableDynamicProfit = false;
				} else {
					showctrlEnableDynamicProfit = true;
					showctrlEnableFixedProfit = false;
					enableDynamicProfit = true;
				}
			}
		}
		
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)]	
		[Display(Name="Enable Dynamic Profit", Order= 1, GroupName="02. Order Settings")]
		public bool EnableDynamicProfit
		{ 	get{
				return enableDynamicProfit;
			} 
			set {
				enableDynamicProfit = value;
				
				if (enableDynamicProfit == true) {
					showctrlEnableDynamicProfit = true;
					showctrlEnableFixedProfit = false;
					enableFixedProfit = false;
				} else {
					showctrlEnableDynamicProfit = false;
					showctrlEnableFixedProfit = true;
					enableFixedProfit = true;
				}
			}
		}
		
		[NinjaScriptProperty]
        [Display(Name = "Order Type (Market/Limit)", Order = 2, GroupName = "02. Order Settings")]
        public OrderType OrderType { get; set; } 
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Order= 3, GroupName="02. Order Settings")]
		public int Contracts
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Tick Move (Button Click)", Order= 4, GroupName="02. Order Settings")]
		public int TickMove
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Initial Stop (Ticks)", Order= 5, GroupName="02. Order Settings")]
		public int InitialStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Profit Target", Order=6, GroupName="02. Order Settings")]
		public double ProfitTarget
		{ get; set; }
		
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)]	
		[Display(Name="Enable Profit Target 2", Order= 7, GroupName="02. Order Settings")]
		public bool EnableProfitTarget2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contract 2", Order= 8, GroupName="02. Order Settings")]
		public int Contracts2
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Profit Target 2", Order=9, GroupName="02. Order Settings")]
		public double ProfitTarget2
		{ get; set; }
		
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)]	
		[Display(Name="Enable Profit Target 3", Order= 10, GroupName="02. Order Settings")]
		public bool EnableProfitTarget3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contract 3", Order= 11, GroupName="02. Order Settings")]
		public int Contracts3
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Profit Target3", Order=12, GroupName="02. Order Settings")]
		public double ProfitTarget3
		{ get; set; }
		
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)]	
		[Display(Name="Enable Profit Target 4", Order= 13, GroupName="02. Order Settings")]
		public bool EnableProfitTarget4
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contract 4", Order= 14, GroupName="02. Order Settings")]
		public int Contracts4
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Profit Target4", Order=15, GroupName="02. Order Settings")]
		public double ProfitTarget4
		{ get; set; }	
		
		#endregion	
		
		#region 03. Order Management
		
//		[NinjaScriptProperty]
//        [Display(Name = "Enable Fixed or Trailing Stop", Order = 0, GroupName = "03. Order Management")]
//        [RefreshProperties(RefreshProperties.All)]
//		public bool enableTrail
//        { 
//			get{
//				return enableTrail;
//			} 
//			set {
//				enableTrail = value;
				
//				if (enableTrail == true) {
//					showTrailOptions = true;
//					tickTrail = true;
//					showAtrTrailOptions = true;
//					showThreeStepTrailOptions = true;
//					enableFixedStopLoss = false;
//				} else {
//					enableFixedStopLoss = true;
//					showTrailOptions = false;
//					tickTrail = false;
//					showAtrTrailOptions = false;
//					showThreeStepTrailOptions = false;
//				}
//			}
//		}
				
		[NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss Type", Description= "Type of Trail Stop", GroupName = "03. Order Management", Order = 1)]
        [RefreshProperties(RefreshProperties.All)]
		public TrailStopTypeKC TrailStopType
        { 
			get { return trailStopType; } 
			set { 
				trailStopType = value; 
				if (trailStopType == TrailStopTypeKC.TickTrail) {
					tickTrail = true;
					enableFixedStopLoss = false;
					atrTrailSetAuto = false;
					showAtrTrailSetAuto = false;					
					showAtrTrailOptions = false;
					threeStepTrail = false;
					showThreeStepTrailOptions = false;
				}
				else if (trailStopType == TrailStopTypeKC.FixedStop) {
					enableFixedStopLoss = true;
					atrTrailSetAuto = false;
					showAtrTrailSetAuto = false;					
					showAtrTrailOptions = false;
					tickTrail = false;
					threeStepTrail = false;
					showThreeStepTrailOptions = false;
				}
				else if (trailStopType == TrailStopTypeKC.ATR_Trail) {
					enableFixedStopLoss = false;
					atrTrailSetAuto = true;
					showAtrTrailSetAuto = true;					
					showAtrTrailOptions = true;
					tickTrail = false;
					threeStepTrail = false;
					showThreeStepTrailOptions = false;
				} else if (trailStopType == TrailStopTypeKC.ThreeStepTrail) {
//					TrailSetAuto = false;
					enableFixedStopLoss = false;
					threeStepTrail = true;
					showThreeStepTrailOptions = true;	
					showAtrTrailOptions = false;				
					atrTrailSetAuto = false;
					showAtrTrailSetAuto = false;	
					tickTrail = false;
				}
			}
		}

		[NinjaScriptProperty]
		[Display(Name="ATR Period", Order= 2, GroupName="03. Order Management")]
		public int atrPeriod	
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ATR Trailing Multiplier", Order= 3, GroupName="03. Order Management")]
		public double atrMultiplier
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Risk To Reward Ratio", Order= 4, GroupName="03. Order Management")]
		public double RiskRewardRatio
		{ get; set; }

//		[NinjaScriptProperty]
//		[Display(Name="Trail Frecuency (Ticks)", Order=6, GroupName="03. Order Management - 1. Tick")]
//		public int Trail_frequency
//		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name = "Enable ATR Profit Target", Description = "Enable  Profit Target based on TrendMagic", Order = 5, GroupName = "03. Order Management")]
		[RefreshProperties(RefreshProperties.All)]
		public bool enableAtrProfitTarget			
		{ get; set; }
		
		//Breakeven Actual				
		[NinjaScriptProperty]
		[RefreshProperties(RefreshProperties.All)]	
		[Display(Name="Enable Breakeven", Order= 6, GroupName="03. Order Management")]	
		public bool BESetAuto
		{ 	get{
				return beSetAuto;
			} 
			set {
				beSetAuto = value;
				
				if (beSetAuto == true) {
					showctrlBESetAuto = true;
				} else {
					showctrlBESetAuto = false;
				}
			}
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Breakeven Trigger", Order = 7, Description="In Ticks", GroupName="03. Order Management")]
		public int BE_Trigger
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Breakeven Offset", Order = 8, Description="In Ticks", GroupName="03. Order Management")]
		public int BE_Offset
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Exit", Description = "Enable Exit", Order = 9, GroupName = "03. Order Management")]
		[RefreshProperties(RefreshProperties.All)]
		public bool enableExit
		{ get; set; }
		
		
		#endregion			

		#region 04. Three-step Trailing Stop
		
		[NinjaScriptProperty]
		[Display(Name="Profit Trigger Step 1", Order = 1, GroupName="04. Three-step Trailing Stop")]
		public int step1ProfitTrigger
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name="Stop Loss Step 1", Order = 2, GroupName="04. Three-step Trailing Stop")]
		public int step1StopLoss
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Profit Trigger Step 2", Order = 3, GroupName="04. Three-step Trailing Stop")]
		public int step2ProfitTrigger
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Stop Loss Step 2", Order = 4, GroupName="04. Three-step Trailing Stop")]
		public int step2StopLoss
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Profit Trigger Step 3", Order = 5, GroupName="04. Three-step Trailing Stop")]
		public int step3ProfitTrigger
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name="Stop Loss Step 3", Order = 6, GroupName="04. Three-step Trailing Stop")]
		public int step3StopLoss
		{ get; set; }		
		
//		[NinjaScriptProperty]
//		[Display(Name="Step1Frequency", Order=7, GroupName="04. Three-step Trailing Stop")]
//		public int step1Frequency
//		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Display(Name="Step2Frequency", Order=8, GroupName="04. Three-step Trailing Stop")]
//		public int step2Frequency
//		{ get; set; }			
		
//		[NinjaScriptProperty]
//		[Display(Name="Step 3 Frequency", Order=9, GroupName="04. Three-step Trailing Stop")]
//		public int step3Frequency
//		{ get; set; }	
		
		#endregion	
		
		#region 05. Profit/Loss Limit	
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Daily Loss / Profit ", Description = "Enable / Disable Daily Loss & Profit control", Order =1, GroupName = "05. Profit/Loss Limit	")]
		[RefreshProperties(RefreshProperties.All)]
		public bool dailyLossProfit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Profit Limit ($)", Description="No positive or negative sign, just integer", Order=2, GroupName="05. Profit/Loss Limit	")]
		public double DailyProfitLimit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Loss Limit ($)", Description="No positive or negative sign, just integer", Order=3, GroupName="05. Profit/Loss Limit	")]
		public double DailyLossLimit
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Trailing Drawdown ($)", Description="No positive or negative sign, just integer", Order=4, GroupName="05. Profit/Loss Limit	")]
		public double TrailingDrawdown
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Start Trailing Drawdown ($)", Description="No positive or negative sign, just integer", Order=5, GroupName="05. Profit/Loss Limit	")]
		public double StartTrailingDD
		{ get; set; }	
		
		#endregion

		#region	06. Trades Per Direction	
		[NinjaScriptProperty]
		[Display(Name = "Enable Trades Per Direction", Description = "Switch off Historical Trades to use this option.", Order = 0, GroupName = "06. Trades Per Direction")]
		[RefreshProperties(RefreshProperties.All)]
		public bool TradesPerDirection 
		{
		 	get{return tradesPerDirection;} 
			set{tradesPerDirection = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name="Long Per Direction", Description = "Number of long in a row", Order = 1, GroupName = "06. Trades Per Direction")]
		public int longPerDirection
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Short Per Direction", Description = "Number of short in a row", Order = 2, GroupName = "06. Trades Per Direction")]
		public int shortPerDirection
		{ get; set; }

		#endregion
		
		#region 07. Other Trade Controls
		
		[NinjaScriptProperty]
		[Display(Name="Seconds Since Entry", Description = "Time between orders i seconds", Order = 3, GroupName = "07. Other Trade Controls")]
		public int SecsSinceEntry
		{ get; set; }				
		
		[NinjaScriptProperty]
		[Display(Name="Bars Since Exit", Description = "Number of bars that have elapsed since the last specified exit. 0 == Not used. >1 == Use number of bars specified ", Order=4, GroupName="07. Other Trade Controls" )]
		public int iBarsSinceExit
		{ get; set; }
		
		#endregion
		
		#region 08. Default Settings			
		
		[NinjaScriptProperty]
        [Display(Name = "Enable VMA", Order = 1, GroupName = "08. Default Settings")]
        public bool enableVMA { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show VMA", Order = 2, GroupName = "08. Default Settings")]
        public bool showVMA { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable EMA Filter", Order = 3, GroupName = "08. Default Settings")]
        public bool enableEMAFilter { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="EMA Length", Order = 4, GroupName="08. Default Settings")]
		public int emaLength
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show EMA", Order = 5, GroupName = "08. Default Settings")]
        public bool showEMA { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable ADX", Order = 6, GroupName = "08. Default Settings")]
        public bool enableADX { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ADX Period", Order = 7, GroupName = "08. Default Settings")]
        public int adxPeriod { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ADX Threshold 1", Order = 6, GroupName = "08. Default Settings")]
        public int adxThreshold { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ADX Threshold 2", Order = 8, GroupName = "08. Default Settings")]
        public int adxThreshold2 { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ADX Exit Threshold", Order = 9, GroupName = "08. Default Settings")]
        public int adxExitThreshold { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show ADX", Order = 10, GroupName = "08. Default Settings")]
        public bool showAdx { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Enable Volatility", Order = 11, GroupName = "08. Default Settings")]
        public bool enableVolatility { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="Volatility Threshold", Order = 12, GroupName="08. Default Settings")]
        public double atrThreshold { get; set; }		
		
		[NinjaScriptProperty]
        [Display(Name = "Show Pivots", Order = 13, GroupName = "08. Default Settings")]
        public bool showPivots { get; set; }
		
		#endregion	
		
		#region 10. Timeframes
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Trades", Order=1, GroupName="10. Timeframes")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Trades", Order=2, GroupName="10. Timeframes")]
		public DateTime End
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 2", Description = "Enable 2 times.", Order=3, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time2
		{
		 	get{return isEnableTime2;} 
			set{isEnableTime2 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 2", Order=4, GroupName="10. Timeframes")]
		public DateTime Start2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 2", Order=5, GroupName="10. Timeframes")]
		public DateTime End2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 3", Description = "Enable 3 times.", Order=6, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time3
		{
		 	get{return isEnableTime3;} 
			set{isEnableTime3 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 3", Order=7, GroupName="10. Timeframes")]
		public DateTime Start3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 3", Order=8, GroupName="10. Timeframes")]
		public DateTime End3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 4", Description = "Enable 4 times.", Order=9, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time4
		{
		 	get{return isEnableTime4;} 
			set{isEnableTime4 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 4", Order=10, GroupName="10. Timeframes")]
		public DateTime Start4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 4", Order=11, GroupName="10. Timeframes")]
		public DateTime End4
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 5", Description = "Enable 5 times.", Order=12, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time5
		{
		 	get{return isEnableTime5;} 
			set{isEnableTime5 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 5", Order=13, GroupName="10. Timeframes")]
		public DateTime Start5
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 5", Order=14, GroupName="10. Timeframes")]
		public DateTime End5
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 6", Description = "Enable 6 times.", Order =15, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time6
		{
		 	get{return isEnableTime6;} 
			set{isEnableTime6 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 6", Order=16, GroupName="10. Timeframes")]
		public DateTime Start6
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 6", Order=17, GroupName="10. Timeframes")]
		public DateTime End6
		{ get; set; }
		
		#endregion
		
		#region 11. Status Panel	
		
		[NinjaScriptProperty]
        [Display(Name = "Show Daily PnL", Order = 1, GroupName = "11. Status Panel")]
        public bool showDailyPnl { get; set; }			
		
		[XmlIgnore()]
		[Display(Name = "Daily PnL Color", Order = 2, GroupName = "11. Status Panel")]
		public Brush colorDailyProfitLoss
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Daily PnL Position", Description = "Daily PNL Alert Position", Order = 3, GroupName = "11. Status Panel")]
		public TextPosition PositionDailyPNL
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string colorDailyProfitLossSerialize
		{
			get { return Serialize.BrushToString(colorDailyProfitLoss); }
   			set { colorDailyProfitLoss = Serialize.StringToBrush(value); }
		}
		
        [NinjaScriptProperty]
        [Display(Name = "Show STATUS PANEL", Order = 4, GroupName = "11. Status Panel")]
        public bool showPnl { get; set; }		

		[XmlIgnore()]
		[Display(Name = "STATUS PANEL Color", Order = 5, GroupName = "11. Status Panel")]
		public Brush colorPnl
		{ get; set; }				
		
		[NinjaScriptProperty]
		[Display(Name="STATUS PANEL Position", Description = "Status PNL Position", Order = 6, GroupName = "11. Status Panel")]
		public TextPosition PositionPnl		
		{ get; set; }	
		
		// Serialize our Color object
		[Browsable(false)]
		public string colorPnlSerialize
		{
			get { return Serialize.BrushToString(colorPnl); }
   			set { colorPnl = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Show Historical Trades", Description = "Show Historical Teorical Trades", Order= 7, GroupName="11. Status Panel")]
		public bool ShowHistorical
		{ get; set; }
		
        #endregion
		
		#region 12. WebHook

		[NinjaScriptProperty]
		[Display(Name="Activate Discord webhooks", Description="Activate One or more Discord webhooks", GroupName="11. Webhook", Order = 0)]
		public bool useWebHook { get; set; }		
		
//		[NinjaScriptProperty]
//		[Display(Name="Discord webhooks", Description="One or more Discord webhooks, separated by comma.", GroupName="11. Webhook", Order = 1)]
//		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
//		public string AccountName { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Discord webhooks", Description="One or more Discord webhooks, separated by comma.", GroupName="11. Webhook", Order = 2)]
		public string DiscordWebhooks
		{ get; set; }
		
		#endregion	
		
		#region Trailing Stop Type
		// Stop Loss Type
		public enum TrailStopTypeKC
		{
			TickTrail,
			FixedStop,
			ATR_Trail,
			ThreeStepTrail
		}
		#endregion
		
		#endregion
    }
}

/*
  // Only enter if at least 10 bars has passed since our last exit or if we have never traded yet
  if ((BarsSinceExitExecution() > iBarsSinceExit || BarsSinceExitExecution() == -1) && CrossAbove(SMA(10), SMA(20), 1))
      EnterLong();

*/