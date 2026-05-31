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
/// EXTRA
using System.IO;
using System.Net;
using System.Globalization;
using System.Diagnostics;
///
#endregion

namespace TraderOracle.SkyFire_v2_0_4
{
    public class PropertyGroup
    {
        public const string General = "General";
        public const string Trading_Window = "Trading Window(s)";
        public const string Contracts = "Contracts";
        public const string Limit_Orders = "Limit Orders";
		public const string TradeSettings_NonATM = "Trade Settings -> Non-ATM";
        public const string TradeSettings_ATM = "Trade Settings -> ATM";
		public const string PnL = "Profit and Loss";
        public const string TrailingStop = "Trailing Stop (Non-ATM)";
        public const string Buy_Sell_Filters = "Buy/Sell Filters";
		public const string Trend_Filter = "Trend Filter";
        public const string Trade_Filters = "Trade Filters";
        public const string Exit_Conditions = "Exit Conditions (Non-ATM)";
        public const string Telegram = "Telegram Notification";
        public const string Other = "Other";
    }
}

namespace NinjaTrader.NinjaScript.Strategies
{
    // Set order of Property Groups based on class in custom namespace
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.General, 0)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, 5)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Contracts, 10)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Limit_Orders, 15)]
	[Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, 20)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, 23)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, 25)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.TrailingStop, 30)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, 35)]
	[Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trend_Filter, 36)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, 37)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Exit_Conditions, 40)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Telegram, 45)]
    [Gui.CategoryOrder(TraderOracle.SkyFire_v2_0_4.PropertyGroup.Other, 50)]

    public class SkyFireATM_v2_0_4 : Strategy
    {

        #region GLOBAL VARIABLES

        // Atm Strategy Initialisers -> start
        private string atmStrategyId = string.Empty;
        private string orderId = string.Empty;
		private bool atmStrategyCreated = false;
        private string selectedATMStrategy;
		private bool atmStoplossChanged = false;
		private bool atmClosed = false;
        // Atm Strategy Initialisers -> end

        private Stopwatch clock = new Stopwatch();
        private DateTime dtStart = DateTime.Now;
        private String sLastTrade = String.Empty;

        private SMA SMAS;
        private SMA SMAF;
        private Series<double> MACD1;
        private Series<double> LMACD;
        private Series<double> sqzData;
        private Series<double> SqueezeDef;
        private Series<double> AO;

        private int iOpenPositions = 0;
		private int iOpenPositionSize = 0;
        private double dailyPnL = 0;

        //-----------------------------------------------------------//

        private bool sessionDone = false;

        private string instrumentName = "";
        private const string stars = "****************************************";

        private DateTime lastCheckedTime;
        private DateTime currentTradingDay;

        private int tradeCount = 0;
        private int winningTradeCount = 0;
        private int losingTradeCount = 0;

        private int lastProcessedTradeIndex = -1;
        private bool updateDisplayMessage = true;

        private Order limitOrder = null;
        private int barsSinceEntry = 0;

        private bool breakevenSet = false;
        private double shadowStoplossPrice = 0;
        private string shadowStoplossTag = "ShadowTrailingStop";

        private string start = "N/A";
        private string end = "N/A";

        private TimeSpan windowStart;
		private TimeSpan windowEnd;
        private bool resetPnl = false;

        private bool usingTradingWindows = false;
        private int tradeWindow = 0;
        private string tradingWindows = string.Empty;

        private int positionSize = 0;

        public enum SlopeDirection { Flat, Up, Down }

        private bool kamaBuyFilterOk = true;
        private bool kamaSellFilterOk = true;

        private bool rsiBuyFilterOk = true;
        private bool rsiSellFilterOk = true;
		
		private bool volumeBuyFilterOk = true;
        private bool volumeSellFilterOk = true;

        private TextPosition textPosition = TextPosition.BottomRight;
		
		private int secondaryBarTimeframe = int.MinValue;
		private int atrPeriod = int.MinValue;
		
		private bool suspendFlag1 = false;
		private bool suspendFlag2 = false;
		
        private string stopTag = "StopPrice";
		
		private bool emaBuyFilterOk = true;
		private bool emaSellFilterOk = true;
		
		private bool priceAboveEmaFilterOk = true;
		private bool priceBelowEmaFilterOk = true;
		
		private double strategyRealizedPnl = 0;
		private double strategyUnrealizedPnl = 0;
		private double strategyCombinedPnl = 0;
		
		private double accountRealizedPnl = 0;
		private double accountUnrealizedPnl = 0;
		private double accountCombinedPnl = 0;
		
		private double previousDayStrategyRealizedPnL = 0.0;
		private double previousDayStrategyUnrealizedPnL = 0.0;
		private double previousDayAccountRealizedPnL = 0.0;
		private double previousDayAccountUnrealizedPnL = 0.0;
		
		private bool tradeExited = false;

        private bool tradeWindowExpired = false;
		private int expiredTradingWindow = 0;
		
		private bool macdPsarInitialized = false;
		
		private int atmStopTargets = 0;
        private int atmStopQuantity = 0;
		private List<string> atmStops = new List<string>();
        private Dictionary<string, double> atmStopsDictionary = new Dictionary<string, double>();
		
		private bool candleWickFilterOk = true;
		private bool candleSizeFilterOk = true;

		//---Order objects---//
		private Order entryOrder = null; 		// This variable holds an object representing our entry order
		private Order stoplossOrder = null; 	// This variable holds an object representing our stop loss order
		private Order profitTargetOrder = null; // This variable holds an object representing our profit target order
		
		private string entryOrderName = string.Empty;
		private string stoplossOrderName = string.Empty;
		private string profitTargetOrderName = string.Empty;
		
		private string exitOCOString = string.Empty;
		private bool scaleInOrder = false;
		
		//---Protective Exit (in case of order placement failures---//)
		private bool protectiveExit = false;
		private double protectiveStoploss = 0;
		private double protectiveProfitTarget = 0;
		
		//---Total trades and Profit factor---//
		private double grossProfit = 0.0;
		private double grossLoss = 0.0;
		
		public enum TypeOfOrder { Buy, Sell }
		
		//---Strategy and Account exit---//
		private bool strategyProfitReached = false;
		private bool accountProfitReached = false;
		private bool dailyFloatingLossReached = false;

		//--Starting capital--//
		private double startingCapital = 0.0;
		
		private bool updateAtmPnLCalled = false;
		private bool partialFillChecked = false;
        private bool floatingOrderClosed = false;
		
        //-----------------------------------------------------------//

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                #region DEFAULTS

                Description = @"Automated trading bot (c) 2024 TraderOracle";
                Name = "SkyFireATM_v2_0_4";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;

				IsUnmanaged = true; // set to Unmanaged for more control over order objects
				IsAdoptAccountPositionAware = true;
				
                Slippage = 2;
//                IncludeCommission = true;

                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                // RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelCloseIgnoreRejects;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 1;
                IsInstantiatedOnEachOptimizationIteration = true;

                /// General
                iWaddahIntense = 150;

                /// PnL settings
                useDailyProfitLimit = true;
                DailyProfitLimit = 750;
				dailyProfitLimitAccount = 1200;

                useDailyLossLimit = true;
                DailyLossLimit = -900;
				
				bExitFloatingProfit = false;
				bExitFloatingLoss = false;

                /// Contracts
                positionSizeInitial = 1;
				
				useScaleIn = true;
                positionSizeScaleIn = 1;
                scaleIntoProfitablePosition = false;
                ticksInProfit = 2;
                iMaxContracts = 9;

                /// Trailing Stop
                useTrailingStop = false;
                breakevenTicks = 60;            // Breakeven at this number of ticks in profit
                breakevenOffsetTicks = 40;      // Breakeven offset, can be negative as well
                trailStartTicks = 70;           // Start trailing at this number of ticks in profit
                trailTicks = 20;                // Trail by this number of ticks

                /// BUY/SELL Filters
                bVolImbalance = true;           // Volume Imbalances
                bUseMACDPSARArrow = false;		// MACD/PSAR Big Arrows
                bUseWaddah = true;              // Waddah Intensity
                bUsePSAR = true;                // Parabolic SAR
                bUseAO = false;                 // Awesome Oscillator
                bUseSqueeze = false;            // Squeeze Momentum
                bUseMACD = true;                // Linda MACD
                bUseHMA = false;                // Hull Moving Average
                bUseSuperTrend = true;			// SuperTrend
                bUseT3 = true;                  // T3
                bUseFisher = true;				// Fisher Transform
                bWaddahAboveE1 = true;          // When Waddah Exploding
                bUseMinADX = true;
                iMinADX = 14;                   // Minimum ADX
                bUseWaddahThreshold = true;
                iWaddahThreshold = 110;         // Minimum Waddah
                bUseADXVMA = true;              // Lizard ADXVMA Plus

				/// Trend Filter
				useEmaTrendFilter = false;
				emaPeriodFast = 9;
				emaPeriodMedium = 21;
				emaPeriodSlow = 50;
				
				priceAboveBelowEma = false;
				priceEmaPeriod = 50;
				
                /// Trade Filters
                useKamaFilter = false;          // KAMA (+Slope) filter
				kamaPeriod = 9;
                kamaSlopeThreshold = 0.005;     // "Flat" threshold - increase via user interface to check for a bigger KAMA slope

                useRsiFilter = false;           // RSI filter
                rsiPeriod = 14;                 // RSI period
                rsiOverbought = 70;             // RSI overbought level
                rsiOversold = 30;               // RSI oversold level
                rsiCandleLookback = 3;          // Number of previous candles to check if they were in the overbought/oversold region

				checkCandleWick = false;
				candleWickThreshold = 30;		// top (BUY) and bottom (SELL) candle wicks must be less than this percentage;
				
				checkCandleSize = true;
				candleSizeThreshold = 150;		// Tick Size
				
				useVolumeFilter = false;		// ROC (Rate of Change) filter
				rocPeriod = 12;					// ROC period
				rocThreshold = 0.02;			// Threshold the ROC needs to satisfy to enable a trade
				volumeLookbackPeriod = 1;		// Bar lookback period to determine if volume increased on the trigger bar
				
                /// Exit Conditions
                bExitKama = false;
                kamaCrossType = CrossType.Close_AboveOrBelow_KAMA;

                bExitWaddah = false;
                bExitPSAR = false;

                /// Initialize ATM strategy-related properties
                SelectedATMStrategy = string.Empty;
                atmStrategyId = string.Empty;
                orderId = string.Empty;
                atmStrategyCreated = false;

                /// Trade Windows
                // morning
                useTradeWindow = true;
                tradeStart = DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
                tradeEnd = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
				flattenAtExpiry = Expiry.No;
                resetPnlAfterTradeWindow = false;
                // after lunch
                useTradeWindow2 = true;
                tradeStart2 = DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
                tradeEnd2 = DateTime.Parse("15:30", System.Globalization.CultureInfo.InvariantCulture);
				flattenAtExpiry2 = Expiry.No;
                resetPnlAfterTradeWindow2 = false;
                // after market
                useTradeWindow3 = false;
                tradeStart3 = DateTime.Parse("18:00", System.Globalization.CultureInfo.InvariantCulture);
                tradeEnd3 = DateTime.Parse("23:30", System.Globalization.CultureInfo.InvariantCulture);
				flattenAtExpiry3 = Expiry.No;
                resetPnlAfterTradeWindow3 = false;

                stopAfterInitialWinningTrades = false;
                initialWinningTrades = 3;

                stopAfterConsecutiveLosses = false;
                consecutiveLosses = 3;

                /// Limit orders
                useLimitOrders = false;
                entryTicks = 2; 	// above/below the ASK/BID
                cancelBars = 3;   	// number of bars to wait before cancelling the limit order

				/// Trade Settings -> Non-ATM
				profitTargetTicks = 50;
				stoplossTicks = 50;
				
				stoplossType = StoplossType.Fixed_Stoploss;
				
				atrTimeframe = 5;
				atrLookbackPeriod = 14;
				atrMultiplier = 1;
				
				swingLookbackPeriod = 14;
				swingTickBuffer = 2;
				
				entryCandleTickBuffer = 2;
				
				/// Trade Settings -> ATM
				UseAtmStrategy = false;
				
				atmStoplossType = StoplossType.Fixed_Stoploss;
				
				atmAtrTimeframe = 5;
				atmAtrLookbackPeriod = 14;
				atmAtrMultiplier = 1;
				
				atmSwingLookbackPeriod = 14;
				atmSwingTickBuffer = 2;
				
				atmEntryCandleTickBuffer = 2;
				
                /// Other
                displayOutputMessages = false;
                printoTab2 = false;

                displayTradingWindowPanel = true;
                tradingWindowPanelLocation = DisplayLocation.BottomRight;

                /// Telegram notification
                useTelegram = false;
                telegramToken = string.Empty;
                telegramChannel = string.Empty;

                #endregion
            }
            else if (State == State.Configure)
            {
                MACD1 = new Series<double>(this);
                LMACD = new Series<double>(this);
                sqzData = new Series<double>(this);
                SqueezeDef = new Series<double>(this);
                AO = new Series<double>(this);

                /// make sure we can trade up to maximum amount of contracts specified by the user
                if (UseAtmStrategy)
					EntriesPerDirection = 1;
				else
                    EntriesPerDirection = iMaxContracts;

                PrintTo = (printoTab2) ? PrintTo.OutputTab2 : PrintTo.OutputTab1;
                instrumentName = Instrument.MasterInstrument.Name;
				
                /// Trading Window(s)
                usingTradingWindows = (useTradeWindow || useTradeWindow2 || useTradeWindow);

                if (usingTradingWindows)
                {
                    tradingWindows = (useTradeWindow  ? "1" : "1 - Not used") +
                                     (useTradeWindow2 ? ", 2" : ", 2 - Not used") +
                                     (useTradeWindow3 ? ", 3" : ", 3 - Not used");
                }
                else
                {
                    tradingWindows = "Not used";
                }

                /// Trading Window display location
                if (tradingWindowPanelLocation == DisplayLocation.TopLeft)
                    textPosition = TextPosition.TopLeft;
                else if (tradingWindowPanelLocation == DisplayLocation.TopRight)
                    textPosition = TextPosition.TopRight;
                else if (tradingWindowPanelLocation == DisplayLocation.BottomRight)
                    textPosition = TextPosition.BottomRight;
                else if (tradingWindowPanelLocation == DisplayLocation.Center)
                    textPosition = TextPosition.Center;
				
				/// add secondary data series
				if (UseAtmStrategy)
				{
					secondaryBarTimeframe = (atmStoplossType == StoplossType.ATR) ? atmAtrTimeframe : 5;
					atrPeriod = (atmStoplossType == StoplossType.ATR) ? atmAtrLookbackPeriod : 14;
				}
				else // non-ATM
				{
					secondaryBarTimeframe = (stoplossType == StoplossType.ATR) ? atrTimeframe : 5;
					atrPeriod = (stoplossType == StoplossType.ATR) ? atrLookbackPeriod : 14;
				}
				
				AddDataSeries(Data.BarsPeriodType.Minute, secondaryBarTimeframe);
            }
            else if (State == State.DataLoaded)
            {
                SMAF = SMA(3);
                SMAS = SMA(10);

                InitializeVariables();
				currentTradingDay = GetTradingDay(Now);
				
				if (UseAtmStrategy)
					SetAtmStopTargets();
            }
        }

		/// method to get the number of stop/targets for an atm strategy
		private void SetAtmStopTargets()
		{
			if (SelectedATMStrategy == string.Empty)
            {
				string message = "'Use ATM Strategy' is enabled, but no strategy has been selected. Please select a valid ATM strategy.";
                PrintMessage(message);
				MessageBox.Show(message, "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseStrategy("ATM Strategy Not Selected!");
            }
			else
			{
				string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				string atmFilePath = System.IO.Path.Combine(documentsFolder, "NinjaTrader 8", "templates", "AtmStrategy", $"{SelectedATMStrategy}.xml");
				
				// Get the count of Bracket elements
		        try
		        {
		            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
		            xmlDoc.Load(atmFilePath);
		            System.Xml.XmlNodeList bracketNodes = xmlDoc.SelectNodes("//Bracket");

		            atmStopTargets = bracketNodes.Count; // Number of stops

                    for (int i = 0; i < atmStopTargets; i++)
		            {
		                string stopName = "Stop" + (i + 1).ToString();
		                double stopLoss = Convert.ToDouble(bracketNodes[i].SelectSingleNode("StopLoss").InnerText);
						atmStopQuantity += Convert.ToInt16(bracketNodes[i].SelectSingleNode("Quantity").InnerText);
		                atmStopsDictionary[stopName] = stopLoss;
		            }
		        }
		        catch (Exception ex)
		        {
		            Print($"Error reading XML file ({atmFilePath}): " + ex.Message);
		        }
			}
		}
		
        private void InitializeVariables(string whenCalled = "Start of Session")
        {
            /// initialize for the new trading day
            sessionDone = false;
            iOpenPositions = 0;
			iOpenPositionSize = 0;

            /// PnL
            dailyPnL = 0;

            updateDisplayMessage = true;
            breakevenSet = false;
            shadowStoplossPrice = 0;

            /// trade window
            tradeWindow = 0;

            /// trade counts
            tradeCount = 0;
            winningTradeCount = 0;

            /// time variables
            lastCheckedTime = Now;
			
			/// update the trading status panel
			if (displayTradingWindowPanel)
                UpdateTradingWindowPanel();

			macdPsarInitialized = false;
            
            /// Strategy and Account profit
			strategyProfitReached = false;
			accountProfitReached = false;
			dailyFloatingLossReached = false;
			
			/// starting capital
			startingCapital = Account.Get(AccountItem.CashValue, Currency.UsDollar);
			
            /// initialize variables for the new trading day
            PrintMessage(string.Format(
				"[ {0}: {1} ] InitializeVariables(): {2}, {3}, Starting Equity = {4}, dailyPnL = {5}\n{6}", 
				Instrument.MasterInstrument.Name, whenCalled, Now.ToString("yyyy-MM-dd"), Now.TimeOfDay,
                Account.Get(AccountItem.CashValue, Currency.UsDollar), dailyPnL, stars
            ));
        }

        /// Cancel limit orders if they are not filled within a certain number of bars
        private void CancelOrderIfNotFilled(string forceString = "")
        {
            // NORMAL entries
			if (!UseAtmStrategy && limitOrder != null)
            {
                barsSinceEntry++;

				if (barsSinceEntry >= cancelBars || forceString != "")
				{
					PrintMessage($"Cancelling LIMIT order!\n{stars}");
	                CancelOrder(limitOrder);
					
					// reset variables
					limitOrder = null;
					barsSinceEntry = 0;
					
					// reset the entry order object
					entryOrder = null;
					
					return;
				}
            }
			
			// ATM entries
			if (UseAtmStrategy && AtmIsFlat() && orderId.Length > 0)
			{
                barsSinceEntry++;

				if (barsSinceEntry >= cancelBars || forceString != "")
				{
					string[] entryOrder = GetAtmStrategyEntryOrderStatus(orderId);
		
					// checks if the entry order exists and the order state is what we expect
				  	if (entryOrder.Length > 0 && (entryOrder[2] == "Working" || entryOrder[2] == "Accepted"))
				  	{
						PrintMessage($"Cancelling ATM LIMIT order!\n{stars}");
		                AtmStrategyCancelEntryOrder(orderId);
						
						// reset ATM strategy variables
						atmStrategyCreated = false;
						atmStrategyId = string.Empty;
		    			orderId = string.Empty;
						
						barsSinceEntry = 0;
						
						return;
		            }
				}
			}
        }

        /// Profit check
        private bool DailyProfitReached(bool flat)
        {
            if (useDailyProfitLimit)
            {
				bool strategyProfitReached = (dailyPnL >= DailyProfitLimit);
				bool accountProfitReached = (accountCombinedPnl >= dailyProfitLimitAccount);
					
				if (flat && (strategyProfitReached || accountProfitReached))
				{
					sessionDone = true;
					
					if (strategyProfitReached)
						PrintMessage("PROFIT", "Exit");
					else
						PrintMessage("ACCOUNT_PROTECTION", "Exit");
					
					return true;
				}
            }

            return false;
        }

        /// Loss check
        private bool DailyStopLossReached(bool flat)
        {
            if (useDailyLossLimit)
            {
				bool dailyStopLossReached = (dailyPnL <= DailyLossLimit || accountRealizedPnl <= DailyLossLimit);

                if (flat && dailyStopLossReached)
                {
                    sessionDone = true;
                    PrintMessage("LOSS", "Exit");
                    return true;
                }
            }

            return false;
        }

        /// check if we need to reset the daily PnL
        private void CheckResetPnl()
        {
            if (tradeWindow == 1)
            {
                windowStart = tradeStart.TimeOfDay;
				windowEnd = tradeEnd.TimeOfDay;
                resetPnl = resetPnlAfterTradeWindow;
            }
            else if (tradeWindow == 2)
            {
                windowStart = tradeStart2.TimeOfDay;
				windowEnd = tradeEnd2.TimeOfDay;
                resetPnl = resetPnlAfterTradeWindow2;
            }
            else if (tradeWindow == 3)
            {
                windowStart = tradeStart3.TimeOfDay;
				windowEnd = tradeEnd3.TimeOfDay;
                resetPnl = resetPnlAfterTradeWindow3;
            }
        }

        /// Trading Window check
        private bool TradingWindowExpired()
        {
            if (usingTradingWindows)
            {
                CheckResetPnl();
				
				bool windowExpired = (windowStart < windowEnd)
									 ? Now.TimeOfDay > windowEnd // normal window
									 : Now.TimeOfDay > windowEnd && Now.TimeOfDay < windowStart; // overnight window
	
				if (windowExpired)	
                {
					tradeWindowExpired = true;
					expiredTradingWindow = tradeWindow;

                    // cancel any unactivated limit orders
					if (useLimitOrders)
                		CancelOrderIfNotFilled("FORCE");
					
                    // check if we should flatten the position for this trading window
					if (usingTradingWindows)
						FlattenPosition();
					
                    PrintMessage("TRADE_WINDOW_EXPIRED", "Exit");

                    // only set sessionDone if all the trading Windows have expired
                    //NOTE: this assumes your trade windows are set chronologically
                    sessionDone = (tradeWindow == 1 && !useTradeWindow2 && !useTradeWindow3) ||
                                  (tradeWindow == 2 && !useTradeWindow3) ||
                                  (tradeWindow == 3);

                    tradeWindow = 0;

                    if (resetPnl)
                        dailyPnL = 0;

                    return true;
                }
            }

            return false;
        }

		private void FlattenPosition()
		{
			if (iOpenPositions > 0)
			{
				bool canFlatten = false;
				
				// check if flatten properties are set on the trading window(s)
				if (tradeWindow == 1)
					canFlatten = (flattenAtExpiry == Expiry.Yes) || (flattenAtExpiry == Expiry.Only_If_In_Profit && accountUnrealizedPnl > 0);
				else if (tradeWindow == 2)
					canFlatten = (flattenAtExpiry2 == Expiry.Yes) || (flattenAtExpiry2 == Expiry.Only_If_In_Profit && accountUnrealizedPnl > 0);
				else if (tradeWindow == 3)
					canFlatten = (flattenAtExpiry3 == Expiry.Yes) || (flattenAtExpiry3 == Expiry.Only_If_In_Profit && accountUnrealizedPnl > 0);
				
				// now check if can still go ahead and flatten the position
				if (canFlatten)
				{
					if (UseAtmStrategy)
					{
						AtmStrategyClose(atmStrategyId);
						UpdateAtmPnL("FLATTEN");
					}
					else // non-ATM
					{
						if (Position.MarketPosition == MarketPosition.Long)
							ExitPosition(TypeOfOrder.Buy, "ExitLong_FlattenPosition");
						else
							ExitPosition(TypeOfOrder.Sell, "ExitShort_FlattenPosition");
					}
				}
			}
		}
		
		private void ExitPosition(TypeOfOrder typeOfOrder, string exitSignal)
		{
			OrderAction orderAction = (typeOfOrder == TypeOfOrder.Buy) ? OrderAction.Sell : OrderAction.BuyToCover;
			// manually exit the position
//			Print($"Exiting position! Position.Quantity = {Position.Quantity}, iOpenPositionSize = {iOpenPositionSize}");
			SubmitOrderUnmanaged(0, orderAction, OrderType.Market, Position.Quantity, 0, 0, "", exitSignal);
		}
		
        #region Trailing Stoploss

        /// update the shadow stoploss
		private void SetStoplossPrice(double newStopPrice)
        {
			// physical stoploss
			ChangeOrder(stoplossOrder, Position.Quantity, 0, newStopPrice);
			// shadow stoploss (in case we get trade placement errors)
			shadowStoplossPrice = newStopPrice;
//            RemoveDrawObject(shadowStoplossTag);
//            Draw.HorizontalLine(this, shadowStoplossTag, shadowStoplossPrice, Brushes.CadetBlue);
        }
		
		/// trailing stoploss
		private void AdjustStoploss()
		{
			bool inLongPosition = (Position.MarketPosition == MarketPosition.Long);
			bool inShortPosition = (Position.MarketPosition == MarketPosition.Short);
            
			if (inLongPosition || inShortPosition)
			{
				double currentProfitTicks = Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks);
				
				// set breakeven (if used)
                if (breakevenTicks > 0 && currentProfitTicks >= breakevenTicks && !breakevenSet)
                {
                  	double newStopPrice = Position.AveragePrice + (inLongPosition ? breakevenOffsetTicks : -breakevenOffsetTicks) * TickSize;
					SetStoplossPrice(newStopPrice);
					breakevenSet = true;
                }

				// start trailing
                bool canStartTrail = (breakevenTicks == 0 || breakevenSet == true) && currentProfitTicks >= trailStartTicks;
				
                if (canStartTrail)
                {
					double newStopPrice = (inLongPosition) 
										  ? GetCurrentBid() - (trailTicks * TickSize) 
										  : GetCurrentAsk() + (trailTicks * TickSize);
					
					if (shadowStoplossPrice == 0)
					{
						SetStoplossPrice(newStopPrice);
					}
					else
					{					
						double currentTrailTicks = (inLongPosition) 
												   ? (GetCurrentBid() - shadowStoplossPrice) / TickSize
												   : (shadowStoplossPrice - GetCurrentAsk()) / TickSize;
						
						if (currentTrailTicks > trailTicks)
						{
							SetStoplossPrice(newStopPrice);
						}
					}
                }
/*				
				// check the shadow stoploss
				if (shadowStoplossPrice != 0)	
				{
					// check shadow stoploss price against current price
					bool shadowStoplossReached = (inLongPosition) ? shadowStoplossPrice >= GetCurrentBid() : shadowStoplossPrice <= GetCurrentAsk();
					
					if (shadowStoplossReached)
					{
						if (inLongPosition)
							ExitLong();
						else if (inShortPosition)
							ExitShort();
						
						string message = $"Exit = Shadow Trailing Stop Reached! (Profit Ticks = {currentProfitTicks})";
							
	                    PrintMessage(message);
	                    sLastTrade = message;

						breakevenSet = false;
						shadowStoplossPrice = 0;
					}
				}
*/				
            }
		}

        #endregion

        /// method to check if we are within the trade window
        private bool WithinTradeWindow(int window)
        {
			DateTime startTime = DateTime.MinValue;
            DateTime endTime = DateTime.MinValue;

            if (window == 1)
            {
                startTime = tradeStart;
                endTime = tradeEnd;
            }
            else if (window == 2)
            {
                startTime = tradeStart2;
                endTime = tradeEnd2;
            }
            else if (window == 3)
            {
                startTime = tradeStart3;
                endTime = tradeEnd3;
            }

            int currentTime = ToHHMMSS(lastCheckedTime);
            int theStart = ToHHMMSS(startTime);
            int theEnd = ToHHMMSS(endTime);

			if (startTime < endTime)
            	return (currentTime >= theStart && currentTime <= theEnd); // normal trade window
			else
				return (currentTime >= theStart || currentTime <= theEnd); // overnight trade window
        }

        private void SetTradingWindow()
        {
            if (usingTradingWindows)
            {
                //check which trade window are we in
				if (useTradeWindow && WithinTradeWindow(1) && expiredTradingWindow != 1)
                    tradeWindow = 1;
				else if (useTradeWindow2 && WithinTradeWindow(2) && expiredTradingWindow != 2)
                    tradeWindow = 2;
				else if (useTradeWindow3 && WithinTradeWindow(3) && expiredTradingWindow != 3)
                    tradeWindow = 3;
				
				if (tradeWindow != 0 && tradeWindowExpired)
				{
					tradeWindowExpired = false;
					expiredTradingWindow = 0;
				}
            }
        }

        /// Main method
        protected override void OnBarUpdate()
        {
            if (State == State.Historical)
                return;
			
			if (Bars.IsFirstBarOfSession)
			{
		        // Store the realized and unrealized Strategy and Account PnL at the start of the new session
				// NOTE: this is done in case the strategy is run non-stop (24/7) i.e. the user does not shut it down every day
				previousDayStrategyRealizedPnL = (SystemPerformance.AllTrades.Count > 0) ? SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit : 0;
            	previousDayStrategyUnrealizedPnL = (SystemPerformance.AllTrades.Count > 0) ? Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency) : 0;
				
		        previousDayAccountRealizedPnL = (Account != null) ? Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) : 0;
	            previousDayAccountUnrealizedPnL = (Account != null) ? Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) : 0;
				
				CalculateAccountPnl();
				CalculateStrategyPnl();
			}
			
            // ensure we only trade on the primary bars object
			if (BarsInProgress != 0)
				return;
			
            if (Bars.BarsSinceNewTradingDay < 1)
                return;

		    if (CurrentBars[0] < 2)	
                return; 

            if (!clock.IsRunning)
                clock.Start();

			// check if we need to reset for the day
            DailyReset();
			
			// MACD/PSAR strategy sync with indicator
			if (bUseMACDPSARArrow && !macdPsarInitialized)
			{
				InitializeMacdPsar();
				macdPsarInitialized = true;
			}

            // do we need to cancel any non-activated limit orders
            if (useLimitOrders)
                CancelOrderIfNotFilled();

			// this statement placed here to ensure strategy signals stays in sync with OptimusNinja indicator signals
			string signal = CheckForNewSignal();
			
			// check exits
            if (!UseAtmStrategy)
                CheckExits();
			
            if (!sessionDone && (!usingTradingWindows || tradeWindow != 0))
            {
                bool flat = (UseAtmStrategy) ? AtmNotSet() : IsFlat();

                if (DailyProfitReached(flat)) // profit check
                    return;

                if (DailyStopLossReached(flat)) // loss check
                    return;

                if (TradingWindowExpired()) // trading window check
                    return;

                // No Signal
                if (signal == "NO_SIGNAL")
                    return;

				// if we're scaling in, check if we're at the maximum number of contracts
				if (!UseAtmStrategy && useScaleIn && iOpenPositionSize >= iMaxContracts)	
	                return;
				
				// if we're NOT scaling in, only allow one position
				if (!UseAtmStrategy && !useScaleIn && iOpenPositions == 1)	
					return;
				
                bool buySignal = (UseAtmStrategy)
                                 ? (signal.Contains("[ BUY ]") && flat)
                                 : (signal.Contains("[ BUY ]") && Position.MarketPosition != MarketPosition.Short);

                bool sellSignal = (UseAtmStrategy)
                                  ? (signal.Contains("[ SELL ]") && flat)
                                  : (signal.Contains("[ SELL ]") && Position.MarketPosition != MarketPosition.Long);

                if (buySignal || sellSignal)
                {
                    double limitPrice = 0;

                    if (useLimitOrders)
						limitPrice = (buySignal) ? GetCurrentAsk() + (entryTicks * TickSize) : GetCurrentBid() - (entryTicks * TickSize);

                    bool canPlaceOrder = true;

                    if (UseAtmStrategy)
                    {
                        OrderAction orderAction = (buySignal) ? OrderAction.Buy : OrderAction.SellShort;
                        OrderType orderType = (useLimitOrders) ? OrderType.StopMarket : OrderType.Market;

                        // validate that the limitPrice is where it should be before placing the order
                        if ((buySignal && useLimitOrders && limitPrice <= GetCurrentAsk()) || (sellSignal && useLimitOrders && limitPrice >= GetCurrentBid()))
                            canPlaceOrder = false;

                        if (canPlaceOrder)
                        {
                            atmStrategyId = GetAtmStrategyUniqueId();
                            orderId = GetAtmStrategyUniqueId();
								
                            AtmStrategyCreate(orderAction, orderType, 0, limitPrice, TimeInForce.Day, orderId, SelectedATMStrategy, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) =>
                            {
                                if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
			                    	atmStrategyCreated = true;
                            });
                        }

                        barsSinceEntry = 0;

                    }
                    else // non-ATM entries
                    {
						positionSize = (iOpenPositions == 0) ? positionSizeInitial : positionSizeScaleIn;
						scaleInOrder = (iOpenPositions == 0) ? false : true;
						
						if (CanExecuteNextTrade())
						{
	                        canPlaceOrder = (buySignal) ? PlaceOrder("BUY", limitPrice) : PlaceOrder("SELL", limitPrice);
	                        barsSinceEntry = 0;
						}
						else
						{
							canPlaceOrder = false;
						}
                    }

                    if (canPlaceOrder)
                    {
                        PrintMessage(string.Format("{0} activated at {1} {2}", signal, Now.ToString("yyyy-MM-dd"), Now.TimeOfDay));
                        sLastTrade = $"{signal} at {Position.AveragePrice}";
                    }
                }
            }
        }

		#region Order Management
		
		private bool CanExecuteNextTrade()
        {
            bool returnValue = true;

            if (scaleIntoProfitablePosition && iOpenPositions > 0)
                returnValue = (Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks) >= ticksInProfit);

            return returnValue;
        }
		
		private bool PlaceOrder(string orderType, double limitPrice)
		{
			bool canPlaceOrder = true;
			
			OrderAction theOrderAction = (orderType == "BUY") ? OrderAction.Buy : OrderAction.SellShort;
			OrderType theOrderType = (useLimitOrders) ? OrderType.StopMarket : OrderType.Market;
			
			if (useLimitOrders)
	        {
				if (limitOrder == null)
					// check if the price is still where we want it
					canPlaceOrder = (orderType == "BUY") ? limitPrice > GetCurrentAsk() : limitPrice < GetCurrentBid();
				else
					canPlaceOrder = false;
			}
			
			if (canPlaceOrder)
			{
				try
				{
					entryOrderName = $"{instrumentName}_EntryOrder_{orderType}";
					stoplossOrderName = $"{instrumentName}_StoplossOrder_{orderType}";
					profitTargetOrderName = $"{instrumentName}_ProfitTargetOrder_{orderType}";
					
					SubmitOrderUnmanaged(0, theOrderAction, theOrderType, positionSize, 0, limitPrice, "", entryOrderName);
				}
				catch (Exception ex)
				{
					canPlaceOrder = false;
					Print($"Error placing {orderType} order! " + ex.Message);
				}
			}
			
			return canPlaceOrder;
		}

		private void PlaceBuyExitOrders()
		{
			double ptPrice = 0, slPrice = 0;
			double atrValue = ATR(BarsArray[1], atrPeriod)[0];

			// calculate Profit Target price
			ptPrice = entryOrder.AverageFillPrice + (profitTargetTicks * TickSize);
			
			// calculate Stoploss price
			if (stoplossType == StoplossType.Fixed_Stoploss)
				slPrice = entryOrder.AverageFillPrice - (stoplossTicks * TickSize);
			else if (stoplossType == StoplossType.ATR)
				slPrice = entryOrder.AverageFillPrice - (atrValue * atrMultiplier);
			else if (stoplossType == StoplossType.EntryCandle_HighLow)
				slPrice = Low[0] - (entryCandleTickBuffer * TickSize);
			else if (stoplossType == StoplossType.RecentSwing_HighLow)
	            slPrice = Low[LowestBar(Low, swingLookbackPeriod)] - (swingTickBuffer * TickSize);

			exitOCOString = instrumentName + "_" + Guid.NewGuid().ToString();
			
			try
			{
				if (scaleInOrder)
				{
					PrintMessage("--> Scaling into existing position! <--");
					ChangeOrder(stoplossOrder, iOpenPositionSize, 0, stoplossOrder.StopPrice);
				    ChangeOrder(profitTargetOrder, iOpenPositionSize, profitTargetOrder.LimitPrice, 0);
					PrintMessage("--> Scaling Done! <--");
				}
				else
				{
					// create stoploss amd profit target orders using Position.Quantity to ensure we cover scale ins
					stoplossOrder = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, entryOrder.Quantity, 0, slPrice, exitOCOString, stoplossOrderName);
	                profitTargetOrder = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, entryOrder.Quantity, ptPrice, 0, exitOCOString, profitTargetOrderName);
				}
			}
			catch (Exception ex)
			{
				protectiveExit = true;
				
				// set protective exits in case we get stoploss and profit target order errors
				protectiveStoploss = entryOrder.AverageFillPrice - ((stoplossTicks + 8) * TickSize);
				protectiveProfitTarget = entryOrder.AverageFillPrice + ((profitTargetTicks + 8) * TickSize);
				
				PrintMessage($"[ PlaceBuyExitOrders ] EXCEPTION! Setting Protective TP and SL: protectiveProfitTarget = {protectiveProfitTarget}, protectiveStoploss = {protectiveStoploss}, ex = {ex.Message}");
				
//				DrawExitOrderLines(protectiveStoploss, protectiveProfitTarget);
			}
		}
		
		private void PlaceSellExitOrders()
		{
			double ptPrice = 0, slPrice = 0;
			double atrValue = ATR(BarsArray[1], atrPeriod)[0];

			// calculate Profit Target price
            ptPrice = entryOrder.AverageFillPrice - (profitTargetTicks * TickSize);
			
			// calculate Stoploss price
			if (stoplossType == StoplossType.Fixed_Stoploss)
				slPrice = entryOrder.AverageFillPrice + (stoplossTicks * TickSize);
			else if (stoplossType == StoplossType.ATR)
				slPrice = entryOrder.AverageFillPrice + (atrValue * atrMultiplier);
            else if (stoplossType == StoplossType.EntryCandle_HighLow) // Entry Candle Higg/Low
                slPrice = High[0] + (entryCandleTickBuffer * TickSize);
            else if (stoplossType == StoplossType.RecentSwing_HighLow) // Recent Swing High/Low
                slPrice = High[HighestBar(High, swingLookbackPeriod)] + (swingTickBuffer * TickSize);

			exitOCOString = instrumentName + "_" + Guid.NewGuid().ToString();
			
			try
			{
				if (scaleInOrder)
				{
					PrintMessage("--> Scaling into existing position! <--");
					ChangeOrder(stoplossOrder, iOpenPositionSize, 0, stoplossOrder.StopPrice);
				    ChangeOrder(profitTargetOrder, iOpenPositionSize, profitTargetOrder.LimitPrice, 0);
					PrintMessage("--> Scaling Done! <--");
				}
				else
				{				
					// create stoploss amd profit target orders using Position.Quantity to ensure we cover scale ins
					stoplossOrder = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, entryOrder.Quantity, 0, slPrice, exitOCOString, stoplossOrderName);
	                profitTargetOrder = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, entryOrder.Quantity, ptPrice, 0, exitOCOString, profitTargetOrderName);
				}
			}
			catch (Exception ex)
			{
				protectiveExit = true;
				
				// set protective exits in case we get stoploss and profit target order errors
	            protectiveStoploss = entryOrder.AverageFillPrice + ((stoplossTicks + 8) * TickSize);
	            protectiveProfitTarget = entryOrder.AverageFillPrice - ((profitTargetTicks + 8) * TickSize);
				
				PrintMessage($"[ PlaceSellExitOrders ] EXCEPTION! Setting Protective TP and SL: protectiveProfitTarget = {protectiveProfitTarget}, protectiveStoploss = {protectiveStoploss}, ex = {ex.Message}");
				
//	            DrawExitOrderLines(protectiveStoploss, protectiveProfitTarget);
			}
		}

		#endregion
		
		private void UpdateAtmPnL(string typeOfExit = null)
		{
			// Delayed update: Use Dispatcher to execute code after trade processing
			Dispatcher.InvokeAsync(() => 
			{
                double tradePnL = 0;
				Position position = Account.GetPosition(Instrument.Id);
	
				if (position != null && position.MarketPosition != MarketPosition.Flat)
				{
					PrintMessage("ATM Position not closed properly. Calling ('AccountExtensions').ClosePosition()...");
					Order order = ClosePosition(); // flatten position on account for this instrument
					double realizedPnl = (Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) - previousDayAccountRealizedPnL);
					tradePnL = realizedPnl - dailyPnL;
				}
				else
				{
					tradePnL = Truncate(GetAtmStrategyRealizedProfitLoss(atmStrategyId));
				}

				dailyPnL += tradePnL;
				
				// analyze trade performance
	            AnalyzeTrade(tradePnL);
					
				if (typeOfExit == "EXIT")
				{
					string message = string.Empty;
					
					if (strategyProfitReached)
					{
						message = $"Exit = ATM Floating Profit >= Strategy Daily Profit Limit ({DailyProfitLimit})";
						PrintMessage("PROFIT", "Exit");
					}
					else if (accountProfitReached)
					{
						message = $"Exit = ATM Floating Profit >= Account Daily Profit Limit ({dailyProfitLimitAccount})";
						PrintMessage("ACCOUNT_PROTECTION", "Exit");
					}
					else if (dailyFloatingLossReached)
					{
						message = $"Exit = ATM Floating Loss <= Daily Loss Limit ({DailyLossLimit})";
						PrintMessage("LOSS", "Exit");
					}
					
					PrintMessage(message);
                    sLastTrade = message;
				}
			});
		}
		
        private void ManageAtmPosition()
        {
            // Check that atm strategy was created before checking other properties
			if (!atmStrategyCreated)
				return;

            if (!suspendFlag1) // because this is called on each tick within the OnMarketData event
			{
				suspendFlag1 = true;
                
                // Check for a pending entry order
                if (orderId.Length > 0)
                {
                    string[] status = GetAtmStrategyEntryOrderStatus(orderId);

                    // If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
                    if (status.GetLength(0) > 0)
                    {
                        // If the order state is terminal, reset the order id value
                        if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
                            orderId = string.Empty;
                    }
                } // If the strategy has terminated reset the strategy id after doing some profit/loss calculations.
                else if (AtmIsFlat() && !updateAtmPnLCalled)
                {
                    updateAtmPnLCalled = true;
                    UpdateAtmPnL();
                }
                
                // Change the stop price(s) if necessary
                if (atmStoplossType != StoplossType.Fixed_Stoploss && !AtmIsFlat() && !atmStoplossChanged)
                {
                    double slPrice = 0;
                    bool longPosition = (GetAtmStrategyMarketPosition(atmStrategyId) == MarketPosition.Long);
                    
                    if (atmStoplossType == StoplossType.EntryCandle_HighLow)
                    {
                        slPrice = (longPosition) ? Low[0] - (atmEntryCandleTickBuffer * TickSize) : High[0] + (atmEntryCandleTickBuffer * TickSize);
                    }
                    else if (atmStoplossType == StoplossType.RecentSwing_HighLow)
                    {
                        slPrice = (longPosition) 
                                ? Low[LowestBar(Low, atmSwingLookbackPeriod)] - (atmSwingTickBuffer * TickSize)
                                : High[HighestBar(High, atmSwingLookbackPeriod)] + (atmSwingTickBuffer * TickSize);
                    }
                    else if (atmStoplossType == StoplossType.ATR)
                    {
                        double atrValue = ATR(BarsArray[1], atrPeriod)[0];
                        slPrice = (longPosition) ? GetCurrentAsk() - (atrValue * atmAtrMultiplier) : GetCurrentBid() + (atrValue * atmAtrMultiplier);
                    }
                    
                    // Loop through potential targets
                    double bid = GetCurrentBid();
                    double ask = GetCurrentAsk();
                    
                    for (int i = 1; i <= atmStopTargets; i++)
                    {
                        string atmStopOrderName = "Stop" + i.ToString();
        
                        // check if we've processed this stop already
                        if (!atmStops.Contains(atmStopOrderName))
                        {
                            try
                            {
                                bool success = AtmStrategyChangeStopTarget(double.MaxValue, slPrice, atmStopOrderName, atmStrategyId);
                                
                                if (success)
                                    atmStops.Add(atmStopOrderName);
                                else
                                    Print(string.Format("Could not change stoploss for stop order {0}\nBID = {1}, ASK = {2}, slPrice = {3}", atmStopOrderName, bid, ask, slPrice));
                            }
                            catch (Exception ex)
                            {
                                Print(string.Format(
                                    "Could not change stoploss for stop order {0}\nBID = {1}, ASK = {2}, slPrice = {3}\nError: {4}",
                                    atmStopOrderName, bid, ask, slPrice, ex.Message));
                                break;
                            }
                        }
                    }
                    
                    if (atmStops.Count == atmStopTargets)
                    {
                        DrawStopLine(slPrice);
                        atmStoplossChanged = true;
                        atmStops.Clear();
                    }
                }
                
				// Daily Profit Limit Reached
                if (useDailyProfitLimit && !atmClosed && !AtmIsFlat())
                {
                    double runningProfitStrategy = (GetAtmStrategyUnrealizedProfitLoss(atmStrategyId) + dailyPnL);
                    double runningProfitAccount = accountCombinedPnl;
                    
                    strategyProfitReached = (runningProfitStrategy >= DailyProfitLimit);
                    accountProfitReached = (runningProfitAccount >= dailyProfitLimitAccount);
                    
                    if (strategyProfitReached || accountProfitReached)
                    {
                        atmClosed = true;
						sessionDone = true;
						updateAtmPnLCalled = true;

                        AtmStrategyClose(atmStrategyId);
                        UpdateAtmPnL("EXIT");
                    }
                }
				
				// Daily Loss Limit reached
				if (useDailyLossLimit && !atmClosed && !AtmIsFlat())
                {
                    dailyFloatingLossReached = (GetAtmStrategyUnrealizedProfitLoss(atmStrategyId) + dailyPnL) <= DailyLossLimit;

                    if (dailyFloatingLossReached)
                    {
                        atmClosed = true;
						sessionDone = true;
						updateAtmPnLCalled = true;

						AtmStrategyClose(atmStrategyId);
                        UpdateAtmPnL("EXIT");
                    }
                }

                // This method will check for floating orders and close them if needed
				// NOTE: in fast moving markets the entry order could get placed and not enough time to place the stoploss order and profit target order
				//       So let's just close the position if we could not set a SL and TP fast enough
				if (!floatingOrderClosed)
				{
					Position position = Account.GetPosition(Instrument.Id);
	
					if (position != null && position.MarketPosition != MarketPosition.Flat)
					{
						if (position.Quantity == atmStopQuantity)
						{
							floatingOrderClosed = true;
						}
						else
						{
							floatingOrderClosed = true;
							updateAtmPnLCalled = true;
							
							PrintMessage("Found NAKED position! Closing all orders for this position!");

							AtmStrategyClose(atmStrategyId);
							UpdateAtmPnL();
						}
					}
				}

                suspendFlag1 = false;
            }
        }

		/// draw a thin blue horizontal line at the the current shadowStoplossPrice
        private void DrawStopLine(double stopPrice)
        {
            RemoveDrawObject(stopTag);
            Draw.HorizontalLine(this, stopTag, stopPrice, Brushes.Red, DashStyleHelper.Solid, 3);
        }
		
        /// helper function to truncate values to 2 decimal places
        public double Truncate(double value)
        {
            /* Multiply the original value by 100 to shift the decimal point two places to the right.
			   Use Math.Truncate to remove the fractional part beyond the decimal point.
			   Divide by 100 to shift the decimal point back to its original position. */
            return Math.Truncate(value * 100) / 100;
        }

		private void ResetForNextOrder()
		{
			breakevenSet = false;
            
            shadowStoplossPrice = 0;
            RemoveDrawObject(shadowStoplossTag);

            iOpenPositions = 0;
			iOpenPositionSize = 0;

            protectiveStoploss = 0;
			protectiveProfitTarget = 0;
			
			atmStoplossChanged = false;
			atmClosed = false;
			
			barsSinceEntry = 0;

            atmStrategyCreated = false;
			atmStrategyId = string.Empty;
		    orderId = string.Empty;
			
			limitOrder = null;
			
			RemoveDrawObject(stopTag);
			
			tradeExited = false;

            updateAtmPnLCalled = false;
			partialFillChecked = false;
            floatingOrderClosed = false;
			
			//---Protective Exit---//
			protectiveExit = false;
			
			// Entry Order object
			entryOrder = null;
		}
		
        /// analyze trade, i.e. was it a winner or a loser
        private void AnalyzeTrade(double realizedProfitLoss)
        {
			// check if we need to cancel any "Working" limit orders
			if (useLimitOrders)
                CancelOrderIfNotFilled("FORCE");
			
			// update the strategy pnl (only for non-ATM)
			if (!UseAtmStrategy)
				CalculateStrategyPnl();
			
            // Increment trade count
            tradeCount++;

            // exit time
            string exitAt = Now.ToString("yyyy-MM-dd") + ", " + Now.TimeOfDay.ToString();
            string message = $"realizedProfitLoss = {Truncate(realizedProfitLoss)}, dailyPnL = {Truncate(dailyPnL)}\nExit trade at {exitAt}\n{stars}";

            if (realizedProfitLoss > 0)
            {
				PrintMessage($"WINNER! {message}");
                winningTradeCount++;

                // Reset consecutive losses on a winning trade
                losingTradeCount = 0;
				grossProfit += realizedProfitLoss;
            }
            else
            {
				PrintMessage($"LOSER! {message}");
                losingTradeCount++;
				grossLoss += Math.Abs(realizedProfitLoss);
            }

            // Stop the strategy if all of the "initialWinningTrades" were winners
            if (stopAfterInitialWinningTrades && tradeCount == initialWinningTrades && winningTradeCount == initialWinningTrades)
            {
                sessionDone = true;
                PrintMessage("INITIAL", "Exit");
            }

            // Stop the strategy if we had <number> of consecutive losses
            if (stopAfterConsecutiveLosses && losingTradeCount >= consecutiveLosses)
            {
                sessionDone = true;
                PrintMessage("CONSECUTIVE_LOSSES", "Exit");
            }

            // re-initialize some variables for next order
            ResetForNextOrder();
        }

        /// check if ATM variables are set
		private bool AtmNotSet()
		{
			return (atmStrategyCreated == false);
		}
		
		/// check if ATM variables are set
		private bool AtmSet()
		{
			return (atmStrategyCreated == true);
		}
		
		/// check if ATM strategy is flat
		private bool AtmIsFlat()
		{
			if (atmStrategyId == null || !atmStrategyCreated)
		    {
		        return true;
		    }
		
		    return GetAtmStrategyMarketPosition(atmStrategyId) == MarketPosition.Flat;
		}
		
		/// check if "Normal" strategy is flat
		private bool IsFlat()
		{
			return (Position.MarketPosition == MarketPosition.Flat);
		}

        /// check if "Normal" strategy is NOT flat
		private bool IsNotFlat()
		{
			return (Position.MarketPosition != MarketPosition.Flat);
		}

        /// Custom helper method to convert DateTime to HHMMSS format
        private int ToHHMMSS(DateTime time)
        {
            return time.Hour * 10000 + time.Minute * 100 + time.Second;
        }

        private void SetStartAndEnd()
        {
            if (usingTradingWindows)
            {
                if (tradeWindow == 1)
                {
                    start = tradeStart.ToString("h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
                    end = tradeEnd.ToString("h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (tradeWindow == 2)
                {
                    start = tradeStart2.ToString("h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
                    end = tradeEnd2.ToString("h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (tradeWindow == 3)
                {
                    start = tradeStart3.ToString("h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
                    end = tradeEnd3.ToString("h:mmtt", System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }

        /// Messages printed to Output Tab (if enabled)
        private async void PrintMessage(string msg, string exitMessage = "")
        {
			string barsTimeframe = $"{BarsArray[0].BarsPeriod.Value} {BarsArray[0].BarsPeriod.BarsPeriodType}";
			
            // regular message
            if (displayOutputMessages && exitMessage == "")
			{
				Print($"{Account.Name} - {instrumentName} ({barsTimeframe}) - {msg}");
				return;
			}

			// exit message
			if ((displayOutputMessages || useTelegram) && exitMessage != "")
			{
                // exit message
                SetStartAndEnd();
                string tradingWindowMessage = (start == "N/A") ? "N/A" : start + " - " + end;
    
                string message = string.Format("[ {0}: {1} Timeframe ]", strategyName, barsTimeframe) + Environment.NewLine;
                message += "Account: " + Account.Name + Environment.NewLine;
                message += "Instrument: " + instrumentName + Environment.NewLine;
                message += "Trading Day: " + Now.ToString("yyyy-MM-dd") + Environment.NewLine;
                message += "Trading Window: " + tradingWindowMessage + Environment.NewLine;
                message += "Daily Profit Limit (Strategy): " + DailyProfitLimit.ToString("C")  + Environment.NewLine;
                message += "Daily Profit Limit (Account): " + dailyProfitLimitAccount.ToString("C")  + Environment.NewLine;
                message += "Daily Loss Limit: " + DailyLossLimit.ToString("C") + Environment.NewLine;
    
                string outcomeMessage = "";
    
                if (msg == "TRADE_WINDOW_EXPIRED")
                    outcomeMessage = "Trading Window EXPIRED!";
                if (msg == "LOSS")
                    outcomeMessage = "Daily Loss Limit Reached at " + Now.ToShortTimeString();
                if (msg == "PROFIT")
                    outcomeMessage = "STRATEGY Daily Profit Limit Reached at " + Now.ToShortTimeString();
                if (msg == "ACCOUNT_PROTECTION")
                    outcomeMessage = "ACCOUNT Daily Profit Limit Reached at " + Now.ToShortTimeString();
                if (msg == "INITIAL")
                    outcomeMessage = "Initial " + initialWinningTrades + " trades were winners. Trading stopped at " + Now.ToShortTimeString();
                if (msg == "CONSECUTIVE_LOSSES")
                    outcomeMessage = consecutiveLosses + " consecutive losses! Trading stopped at " + Now.ToShortTimeString();
    
                message += "Outcome: " + outcomeMessage + Environment.NewLine;
                message += "Daily PnL (Strategy): " + dailyPnL.ToString("C") + Environment.NewLine;
                message += "Account Balance: " + Account.Get(AccountItem.CashValue, Currency.UsDollar).ToString("C");
    
                // print message
				if (displayOutputMessages)
				{
					Print("--------------------------------------------------");
					Print(message);
					Print("--------------------------------------------------");
				}
				
				// send Telegram notification
				if (useTelegram)
				{
					SendTelegramMessage(message);
				}
				
				// re-initialize some global variables to prevent unnecessary errors
				// e.g. 'GetAtmStrategyEntryOrderStatus' method error: Missing order ID parameter
				ResetForNextOrder();
			}
        }

        /// Telegram Notifications
        private string SendTelegramMessage(string message)
        {
            string returnValue = "";

            if (telegramChannel == string.Empty || telegramToken == string.Empty)
            {
                Print("Please provide a Telegram Channel (ID) and Telegram Token to receive Telegram Notifications");
                return returnValue;
            }

            string urlString = "https://api.telegram.org/bot" + telegramToken + "/sendMessage?chat_id=" + telegramChannel + "&text=" + message;

            try
            {
                WebClient webclient = new WebClient();
                returnValue = webclient.DownloadString(urlString);
            }
            catch (Exception ex)
            {
                Print("Error sending Telegram Notification! " + ex.Message);
            }

            return returnValue;
        }

        /// Helper method to determine the trading day dynamically
        private DateTime GetTradingDay(DateTime time)
        {
			// Use the session iterator to get session start and end times
			SessionIterator sessionIterator = new SessionIterator(Bars);
            // Move to the session that includes the current time
            sessionIterator.GetNextSession(time, false);
            // Get the session start time
            DateTime sessionBegin = sessionIterator.ActualSessionBegin;

            // If the current time is before the session start time, the trading day is the previous day
            if (time < sessionBegin)
            {
                sessionIterator.GetNextSession(time.AddDays(-1), false);
                sessionBegin = sessionIterator.ActualSessionBegin;
            }

            return sessionBegin.Date;
        }

        /// check if it's time to reset for the day
        private void DailyReset()
        {
			DateTime newTradingDay = GetTradingDay(Now);

            if (newTradingDay != currentTradingDay)
            {
                // initialize for the new trading day
                currentTradingDay = newTradingDay;
                InitializeVariables("Daily RESET!");
            }
        }

        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
			if (State == State.Historical)
				return;
			
			// only run for the primary bars object
			if (BarsInProgress == 0)
			{
	            // Get the current market time
	            DateTime currentTime = marketDataUpdate.Time;
				
	            // Check if at least one second has passed since the last check
	            if ((currentTime - lastCheckedTime).TotalSeconds >= 1)
	            {
	                lastCheckedTime = currentTime;
	                SetTradingWindow();
					
					// update the trading status panel
					if (displayTradingWindowPanel)
		                UpdateTradingWindowPanel();
	
					if (!sessionDone && tradeWindow != 0)
	                {
						if (UseAtmStrategy)
	                    {
	                        // check for any open ATM positions
	                        ManageAtmPosition();
	                    }
	                    else // non-ATM strategies
	                    {
	                        //Check PnL
	                        bool flat = IsFlat();
	
	                        DailyProfitReached(flat);
	                        DailyStopLossReached(flat);
	                    }
						// check if trading window expired
						TradingWindowExpired();
	                }
	            }
	
	            // check exits
	            if (!UseAtmStrategy)
	                CheckExits();
	
	            // trailing stoploss
	            if (!UseAtmStrategy && useTrailingStop)
	                AdjustStoploss();
			}
        }

        private void RecalculateExitIndicators()
        {
            // KAMA
            kama = KAMA(2, kamaPeriod, 109);

            // WADDAH
            double Trend1 = (MACD(20, 40, 9)[0] - MACD(20, 40, 9)[1]) * iWaddahIntense;
            wadaUp = Trend1 >= 0 ? true : false;

            // PSAR
            ParabolicSAR sar = ParabolicSAR(0.02, 0.2, 0.02);
            psarUp = sar.Value[0] < Low[0] ? true : false;
        }

        /// only used by non-ATM strategies
        private void CheckExits()
        {
			if (!suspendFlag2) // because this is called on each tick within the OnMarketData event
			{
				suspendFlag2 = true;
				
	            bool inLongPosition = (Position.MarketPosition == MarketPosition.Long);
	            bool inShortPosition = (Position.MarketPosition == MarketPosition.Short);
	
	            if (inLongPosition || inShortPosition)
	            {
					// Protective stoploss and profit target
					if (protectiveExit && !tradeExited)
					{
						bool profitReached = (inLongPosition) ? GetCurrentBid() > protectiveProfitTarget : GetCurrentAsk() < protectiveProfitTarget;
						bool lossReached = (inLongPosition) ? GetCurrentBid() < protectiveStoploss : GetCurrentAsk() > protectiveStoploss;
						
						if (profitReached || lossReached)
						{
							tradeExited = true;
							
							if (inLongPosition)
								ExitPosition(TypeOfOrder.Buy, "ExitLong_ProtectiveExit");
							else if (inShortPosition)
								ExitPosition(TypeOfOrder.Sell, "ExitShort_ProtectiveExit");
							
							string message = string.Empty;
							
							if (profitReached)	
								message = $"Exit = Protective Exit! Profit target reached";
							else if (lossReached)	
								message = $"Exit = Protective Exit! Stoploss reached";
							
		                    PrintMessage(message);
		                    sLastTrade = message;
						}
					}
					
					// Floating Profit Exit
					if (bExitFloatingProfit && useDailyProfitLimit && !tradeExited)
					{
						double runningProfitStrategy = (Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency) + dailyPnL);
						double runningProfitAccount = accountCombinedPnl;
						
						strategyProfitReached = (runningProfitStrategy >= DailyProfitLimit);
						accountProfitReached = (runningProfitAccount >= dailyProfitLimitAccount);
	    				
						if (strategyProfitReached || accountProfitReached)
						{
							tradeExited = true;
                            sessionDone = true;
							
							if (inLongPosition)
				            	ExitPosition(TypeOfOrder.Buy, "ExitLong_FloatingProfitExit");
							else if (inShortPosition)
								ExitPosition(TypeOfOrder.Sell, "ExitShort_FloatingProfitExit");
							
							string message = (strategyProfitReached)
											 ? $"Exit = Floating Profit >= Strategy Daily Profit Limit ({DailyProfitLimit})"
											 : $"Exit = Floating Profit >= Account Daily Profit Limit ({dailyProfitLimitAccount}";
						
							PrintMessage(message);
		                    sLastTrade = message;
							
							// NOTE: strategy/account profit exit determined in OnExecutionUpdate event
						}
					}
					
					// Floating Loss Exit
					if (bExitFloatingLoss && useDailyLossLimit && !tradeExited)
					{
						dailyFloatingLossReached = (Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency) + dailyPnL) <= DailyLossLimit;
						
						if (dailyFloatingLossReached)
						{
							tradeExited = true;
                            sessionDone = true;
							
							if (inLongPosition)
				            	ExitPosition(TypeOfOrder.Buy, "ExitLong_FloatingLossExit");
							else if (inShortPosition)
								ExitPosition(TypeOfOrder.Sell, "ExitShort_FloatingLossExit");
							
							string message = $"Exit = Floating Loss <= Daily Loss Limit ({DailyLossLimit}";
						
							PrintMessage(message);
		                    sLastTrade = message;
							
							// NOTE: floating loss exit determined in OnExecutionUpdate event
						}
					}
	
	                // Other exits - LONG
	                // recalcucate the exit indicators for cases where the trade window expires and we're still in a trade
	                if (usingTradingWindows && tradeWindow == 0)
	                {
	                    RecalculateExitIndicators();
	                }
	
	                if (inLongPosition)
	                {
						double kamaValue = kama.Value[0], open = Open[0], close = Close[0];
						bool redCandle = close < open;
						
	                    if (redCandle && bExitKama && !tradeExited)
	                    {
	                        bool canExit = (kamaCrossType == CrossType.Close_AboveOrBelow_KAMA)
	                                       ? close < kamaValue
	                                       : open < kamaValue && close < kamaValue;
	
	                        if (canExit)
	                        {
								tradeExited = true;
								
	                            ExitPosition(TypeOfOrder.Buy, "ExitLong_KAMA_Reversal");
								
	                            PrintMessage("Exit = Priced crossed KAMA");
	                            sLastTrade = "Exit = Priced crossed KAMA";
	                        }
	                    }
	
	                    if (!wadaUp && bExitWaddah && !tradeExited)
	                    {
							tradeExited = true;
							
	                        ExitPosition(TypeOfOrder.Buy, "ExitLong_WADDAH_Reversal");
							
	                        PrintMessage("Exit = Waddah Reversed");
	                        sLastTrade = "Exit = Waddah Reversed";
	                    }
	
	                    if (!psarUp && bExitPSAR && !tradeExited)
	                    {
							tradeExited = true;
							
	                        ExitPosition(TypeOfOrder.Buy, "ExitLong_PSAR_Reversal");
							
	                        PrintMessage("Exit = PSAR Reversed");
	                        sLastTrade = "Exit = PSAR Reversed";
	                    }
	                }
	
	                // Other exits - SHORT
	                if (inShortPosition)
	                {
						double kamaValue = kama.Value[0], open = Open[0], close = Close[0];
						bool greenCandle = close > open;
						
	                    if (greenCandle && bExitKama && !tradeExited)
	                    {
	                        bool canExit = (kamaCrossType == CrossType.Close_AboveOrBelow_KAMA)
	                                       ? close > kamaValue
	                                       : open > kamaValue && close > kamaValue;
	
	                        if (canExit)
	                        {
								tradeExited = true;
								
	                            ExitPosition(TypeOfOrder.Sell, "ExitShort_KAMA_Reversal");
								
	                            PrintMessage("Exit = Priced crossed KAMA");
	                            sLastTrade = "Exit = Priced crossed KAMA";
	                        }
	                    }
	
	                    if (wadaUp && bExitWaddah && !tradeExited)
	                    {
							tradeExited = true;
							
	                        ExitPosition(TypeOfOrder.Sell, "ExitShort_WADDAH_Reversal");
							
	                        PrintMessage("Exit = Waddah Reversed");
	                        sLastTrade = "Exit = Waddah Reversed";
	                    }
	
	                    if (psarUp && bExitPSAR && !tradeExited)
	                    {
							tradeExited = true;
							
	                        ExitPosition(TypeOfOrder.Sell, "ExitShort_PSAR_Reversal");
							
	                        PrintMessage("Exit = PSAR Reversed");
	                        sLastTrade = "Exit = PSAR Reversed";
	                    }
	                }
	            }
				
				suspendFlag2 = false;
			}
        }

		/// method to be run on strategy startup to determine if the MACD/PSAR arrow is currently green or red
		private void InitializeMacdPsar()
		{
			double Trend1, Explo1, psar;
			bool psarUp = false;
			
			for (int i = 0; i < 256; i++)
			{
				Trend1 = (MACD(20, 40, 9)[i] - MACD(20, 40, 9)[i+1]) * iWaddahIntense;
				Explo1 = Bollinger(2, 20).Upper[i] - Bollinger(2, 20).Lower[i];
				psar = ParabolicSAR(0.02, 0.2, 0.02)[i];
				psarUp = psar < Low[i] ? true : false;
							
				// MACD/PSAR Big Arrow
				if (Trend1 > Explo1 && psarUp)
				{
					bBigArrowUp = true;
					break;
				}
				else if (Trend1 < 0 && Math.Abs(Trend1) > Explo1 && !psarUp)
				{
					bBigArrowUp = false;
					break;
				}
			}
		}
		
        // global variables used in both the CheckForNewSignal() and CheckExits() methods
        private KAMA kama;
        private bool green = false, red = false;
        private bool wadaUp = false, psarUp = false;
        private bool bBigArrowUp = false;
        private bool bReveralSquareUp = false;

        private string CheckForNewSignal()
        {
            #region INDICATOR CALCULATIONS

            // Awesome Oscillator
            bool bAOGreen = false;
            var ao = SMA(Median, 5)[0] - SMA(Median, 34)[0];
            if (AO[0] > AO[1])
                bAOGreen = true;

            // SQUEEZE 
            double bbt = Bollinger(2, 20).Upper[0];
            double bbb = Bollinger(2, 20).Lower[0];
            double kct = KeltnerChannel(2, 20).Upper[0];
            double kcb = KeltnerChannel(2, 20).Lower[0];

            bool sqzOn = (bbb > kcb) && (bbt < kct);
            bool sqzOff = (bbb < kcb) && (bbt > kct);
            bool noSqz = (sqzOn == false) && (sqzOff == false);

            double h = High[HighestBar(High, 20)];
            double l = Low[LowestBar(Low, 20)];

            double avg = (h + l) / 2;
            avg = (avg + (kct + kcb) / 2) / 2;

            sqzData[0] = Close[0] - avg;
            SqueezeDef[0] = LinReg(sqzData, 20)[0];

            bool sqeezeUp = false;
            if (SqueezeDef[0] > 0)
                sqeezeUp = true;

            // Linda MACD
            MACD1[0] = SMAF[0] - SMAS[0];
            bool macdUp = MACD1[0] - SMA(MACD1, 16)[0] > 0;

            // Waddah Explosion
            double Trend1, Trend2, Explo1, Explo2, Dead;
            Trend1 = (MACD(20, 40, 9)[0] - MACD(20, 40, 9)[1]) * iWaddahIntense;
            Trend2 = (MACD(20, 40, 9)[2] - MACD(20, 40, 9)[3]) * iWaddahIntense;
            Explo1 = Bollinger(2, 20).Upper[0] - Bollinger(2, 20).Lower[0];
            Explo2 = Bollinger(2, 20).Upper[1] - Bollinger(2, 20).Lower[1];
            Dead = TickSize * 30;
            wadaUp = Trend1 >= 0 ? true : false;

            Supertrend st = Supertrend(2, 11);
            bool superUp = st.Value[0] < Low[0] ? true : false;

            FisherTransform ft = FisherTransform(10);
            bool fisherUp = ft.Value[0] > ft.Value[1] ? true : false;

            ParabolicSAR sar = ParabolicSAR(0.02, 0.2, 0.02);
            psarUp = sar.Value[0] < Low[0] ? true : false;
			
            Bollinger bb = Bollinger(2, 20);
            double bb_top = bb.Values[0][0];
            double bb_bottom = bb.Values[2][0];

            HMA hma = HMA(14);
            bool hullUp = hma.Value[0] > hma.Value[1];

            T3 t3 = T3(10, 2, 0.7);
            bool t3Up = Close[0] > t3.Value[0];

            ADX x = ADX(10);
            kama = KAMA(2, kamaPeriod, 109);
            RSI rsi = RSI(14, 1);

            // Contribution from smitty4728 on Discord
            var amaADXVMAPlus1 = amaADXVMAPlus(Close, false, 8, 8, 8);
            bool adxvmaUP = amaADXVMAPlus1.Trend[0] == 1;
            bool adxvmaDN = amaADXVMAPlus1.Trend[0] == -1;

            #endregion

            #region CANDLE CALCULATIONS

            bool bShowDown = true;
            bool bShowUp = true;

            red = Close[0] < Open[0];
            green = Close[0] > Open[0];

            var c0G = Open[0] < Close[0];
            var c0R = Open[0] > Close[0];
            var c1G = Open[1] < Close[1];
            var c1R = Open[1] > Close[1];
            var c2G = Open[2] < Close[2];
            var c2R = Open[2] > Close[2];
            var c3G = Open[3] < Close[3];
            var c3R = Open[3] > Close[3];
            var c4G = Open[4] < Close[4];
            var c4R = Open[4] > Close[4];

            var c0Body = Math.Abs(Close[0] - Open[0]);
            var c1Body = Math.Abs(Close[1] - Open[1]);
            var c2Body = Math.Abs(Close[2] - Open[2]);
            var c3Body = Math.Abs(Close[3] - Open[3]);
            var c4Body = Math.Abs(Close[4] - Open[4]);

            var upWickLarger = c0R && Math.Abs(High[0] - Open[0]) > Math.Abs(Low[0] - Close[0]);
            var downWickLarger = c0G && Math.Abs(Low[0] - Open[0]) > Math.Abs(Close[0] - High[0]);
            var ThreeOutUp = c2R && c1G && c0G && Open[1] < Close[2] && Open[2] < Close[1] && Math.Abs(Open[1] - Close[1]) > Math.Abs(Open[2] - Close[2]) && Close[0] > Low[1];
            var ThreeOutDown = c2G && c1R && c0R && Open[1] > Close[2] && Open[2] > Close[1] && Math.Abs(Open[1] - Close[1]) > Math.Abs(Open[2] - Close[2]) && Close[0] < Low[1];
            var eqHigh = c0R && c1R && c2G && c3G && (High[1] > bb_top || High[2] > bb_top) && Close[0] < Close[1] && (Open[1] == Close[2] || Open[1] == Close[2] + TickSize || Open[1] + TickSize == Close[2]);
            var eqLow = c0G && c1G && c2R && c3R && (Low[1] < bb_bottom || Low[2] < bb_bottom) && Close[0] > Close[1] && (Open[1] == Close[2] || Open[1] == Close[2] + TickSize || Open[1] + TickSize == Close[2]);

            #endregion

            #region DISPLAY BUY / SELL <----- OLD (Not used)

            //            // VOLUME IMBALANCE
            //            if (green && c1G && Open[0] > Close[1] && bVolImbalance)
            //                return "+1 Volume Imbalance";

            //            if (red && c1R && Open[0] < Close[1] && bVolImbalance)
            //                return "-1 Volume Imbalance";

            //            // ========================    UP CONDITIONS    ===========================

            //            if ((!macdUp && bUseMACD) || (!psarUp && bUsePSAR) || (!fisherUp && bUseFisher) || 
            //				(!t3Up && bUseT3) || (!wadaUp && bUseWaddah) || (!superUp && bUseSuperTrend) || 
            //				(!sqeezeUp && bUseSqueeze) || x.Value[0] < iMinADX || (bUseHMA && !hullUp) || (bUseAO && !bAOGreen))
            //                bShowUp = false;

            //            if (green && bShowUp)
            //                return "+1 Standard Buy";

            //            // ========================    DOWN CONDITIONS    =========================

            //            if ((macdUp && bUseMACD) || (psarUp && bUsePSAR) || (fisherUp && bUseFisher) || 
            //				(t3Up && bUseT3) || (wadaUp && bUseWaddah) || (superUp && bUseSuperTrend) || 
            //				(sqeezeUp && bUseSqueeze) || x.Value[0] < iMinADX || (bUseHMA && hullUp) || (bUseAO && bAOGreen))
            //                bShowDown = false;

            //            if (red && bShowDown)
            //                return "-1 Standard Sell";

            //          return "0000";

            #endregion

            #region BUY / SELL Signal <----- NEW (same as original SkyFire v1.7 and more)

			CheckEMATrendFilter(); 				// check if we're using the 3-EMA trend filter
            CheckKAMAFilter("MACD/PSAR"); 		// check if we're using KAMA trade filter [specifically for the MACD/PSAR Big Arrows]
            CheckRSIFilter();  					// check if we're using RSI trade filter
			CheckPriceAboveBelowEMAFilter();	// check if we're using the price above the ema for BUY and price below the ema for SELL trades
			CheckCandleWick();					// check if the candle wick is within the defined threshold
			CheckCandleSize();					// ensure that we do not trigger on big candles
			CheckVolumeFilter();  				// check if we're using Volume trade filter

            // MACD/PSAR Big Arrow
            if (bUseMACDPSARArrow && Trend1 > Explo1 && psarUp && !bBigArrowUp)
            {
                bBigArrowUp = true;
                if (kamaBuyFilterOk && emaBuyFilterOk && candleWickFilterOk && priceAboveEmaFilterOk && candleSizeFilterOk && volumeBuyFilterOk)
				{
                    return "[ BUY ] MACD/PSAR Big Arrow";
				}
            }

            if (bUseMACDPSARArrow && Trend1 < 0 && Math.Abs(Trend1) > Explo1 && !psarUp && bBigArrowUp)
            {
                bBigArrowUp = false;
                if (kamaSellFilterOk && emaSellFilterOk && candleWickFilterOk && priceBelowEmaFilterOk && candleSizeFilterOk && volumeSellFilterOk)
				{
                    return "[ SELL ] MACD/PSAR Big Arrow";
				}
            }

            CheckKAMAFilter(); // check if we're using KAMA trade filter [second call for non-MACD/PSAR Big Arrows]

            // VOLUME IMBALANCE
            if (green && c1G && Open[0] > Close[1] && bVolImbalance)
			{
//				PrintMessage(string.Format("BUY Volume Imbalance signal at {1} {2}", slope, Now.ToString("yyyy-MM-dd"), Now.TimeOfDay));
//				PrintMessage($"slope = {slope}, kamaBuyFilterOk = {kamaBuyFilterOk}, rsiBuyFilterOk = {rsiBuyFilterOk}, emaBuyFilterOk = {emaBuyFilterOk}");
				if (kamaBuyFilterOk && rsiBuyFilterOk && emaBuyFilterOk && candleWickFilterOk && priceAboveEmaFilterOk && candleSizeFilterOk)
	                return "[ BUY ] Volume Imbalance";
			}
			
            if (red && c1R && Open[0] < Close[1] && bVolImbalance)
			{
//				PrintMessage(string.Format("[ slope = {0} ] SELL Volume Imbalance signal at {1} {2}", slope, Now.ToString("yyyy-MM-dd"), Now.TimeOfDay));
//				PrintMessage($"slope = {slope}, kamaSellFilterOk = {kamaSellFilterOk}, rsiSellFilterOk = {rsiSellFilterOk}, emaSellFilterOk = {emaSellFilterOk}");
				if (kamaSellFilterOk && rsiSellFilterOk && emaSellFilterOk && candleWickFilterOk && priceBelowEmaFilterOk && candleSizeFilterOk)
                	return "[ SELL ] Volume Imbalance";
			}

            // check if at least one of the remaining BUY/SELL filters is checked
            if (bUseMACD || bUsePSAR || bUseFisher || bUseT3 || bUseWaddah || bUseSuperTrend || bUseSqueeze ||
                bUseMinADX || bUseWaddahThreshold || bWaddahAboveE1 || bUseHMA || bUseADXVMA || bUseAO)
            {
//				Print("Some BUY/SELL filters were enabled");
                // ========================    UP CONDITIONS    ===========================
                if ((!macdUp && bUseMACD) ||
                    (!psarUp && bUsePSAR) ||
                    (!fisherUp && bUseFisher) ||
                    (!t3Up && bUseT3) ||
                    (!wadaUp && bUseWaddah) ||
                    (!superUp && bUseSuperTrend) ||
                    (!sqeezeUp && bUseSqueeze) ||
                    (x.Value[0] < iMinADX && bUseMinADX) ||
                    (Math.Abs(Trend1) < iWaddahThreshold && bUseWaddahThreshold) ||
                    (Math.Abs(Trend1) < Explo1 && bWaddahAboveE1) ||
                    (!hullUp && bUseHMA) ||
                    (!adxvmaUP && bUseADXVMA) ||
                    (!bAOGreen && bUseAO))
				{
                    bShowUp = false;
				}
				
                if (green && bShowUp)
				{
//					PrintMessage(string.Format("Standard BUY signal at {1} {2}", slope, Now.ToString("yyyy-MM-dd"), Now.TimeOfDay));
//					PrintMessage($"slope = {slope}, kamaBuyFilterOk = {kamaBuyFilterOk}, rsiBuyFilterOk = {rsiBuyFilterOk}, emaBuyFilterOk = {emaBuyFilterOk}, candleWickFilterOk = {candleWickFilterOk}");
					if (kamaBuyFilterOk && rsiBuyFilterOk && emaBuyFilterOk && candleWickFilterOk && priceAboveEmaFilterOk && candleSizeFilterOk && volumeBuyFilterOk)
                    	return "[ BUY ] Standard Buy";
				}

                // ========================    DOWN CONDITIONS    =========================

                if ((macdUp && bUseMACD) ||
                    (psarUp && bUsePSAR) ||
                    (fisherUp && bUseFisher) ||
                    (t3Up && bUseT3) ||
                    (wadaUp && bUseWaddah) ||
                    (superUp && bUseSuperTrend) ||
                    (sqeezeUp && bUseSqueeze) ||
                    (x.Value[0] < iMinADX && bUseMinADX) ||
                    (Math.Abs(Trend1) < iWaddahThreshold && bUseWaddahThreshold) ||
                    (Math.Abs(Trend1) < Explo1 && bWaddahAboveE1) ||
                    (hullUp && bUseHMA) ||
                    (!adxvmaDN && bUseADXVMA) ||
                    (bAOGreen && bUseAO))
				{
                    bShowDown = false;
				}

                if (red && bShowDown)
				{
//					PrintMessage(string.Format("Standard SELL signal at {1} {2}", slope, Now.ToString("yyyy-MM-dd"), Now.TimeOfDay));
//					PrintMessage($"slope = {slope}, kamaSellFilterOk = {kamaSellFilterOk}, rsiSellFilterOk = {rsiSellFilterOk}, emaSellFilterOk = {emaSellFilterOk}, candleWickFilterOk = {candleWickFilterOk}");
					if (kamaSellFilterOk && rsiSellFilterOk && emaSellFilterOk && candleWickFilterOk && priceBelowEmaFilterOk && candleSizeFilterOk && volumeSellFilterOk)
                    	return "[ SELL ] Standard Sell";
				}
            }

            #endregion

            return "NO_SIGNAL";
        }

		/// Trend Filter: Fast/Medium/Slow EMA 
		private void CheckEMATrendFilter()
		{
			if (useEmaTrendFilter)
			{
				double fastEMA = EMA(Close, emaPeriodFast)[0];
				double mediumEMA = EMA(Close, emaPeriodMedium)[0];
				double slowEMA = EMA(Close, emaPeriodSlow)[0];
				
				emaBuyFilterOk = (fastEMA > mediumEMA && mediumEMA > slowEMA);
				emaSellFilterOk = (fastEMA < mediumEMA && mediumEMA < slowEMA);
			}
		}
		
		/// Trend Filter: Price Above/Below EMA 
		private void CheckPriceAboveBelowEMAFilter()
		{
			if (priceAboveBelowEma)
			{
				double emaValue = EMA(Close, priceEmaPeriod)[0];
				
				priceAboveEmaFilterOk = (Open[0] > emaValue && Close[0] > emaValue);
				priceBelowEmaFilterOk = (Open[0] < emaValue && Close[0] < emaValue);
			}
		}
		
        /// Trade Filter: KAMA (+Slope) [Buy above KAMA, Sell below KAMA]
		private double slope = 0;
		
        private void CheckKAMAFilter(string filter = "")
        {
            if (useKamaFilter)
            {
                if (filter == "")
                {
                    bool candleAbove = (Open[0] > kama.Value[0] && Close[0] > kama.Value[0]);
                    bool candleBelow = (Open[0] < kama.Value[0] && Close[0] < kama.Value[0]);

                    // Calculate the slope of the KAMA
                    slope = Slope(kama, kamaPeriod, 0);

                    // Determine the slope direction with a threshold for flat slope
                    SlopeDirection currentSlope;

                    if (slope > kamaSlopeThreshold)
                        currentSlope = SlopeDirection.Up;
                    else if (slope < -kamaSlopeThreshold)
                        currentSlope = SlopeDirection.Down;
                    else
                        currentSlope = SlopeDirection.Flat;

                    kamaBuyFilterOk  = (candleAbove && currentSlope == SlopeDirection.Up);
                    kamaSellFilterOk = (candleBelow && currentSlope == SlopeDirection.Down);
                }
                else
                {
                    kamaBuyFilterOk  = (Close[0] > kama.Value[0]); 
                    kamaSellFilterOk = (Close[0] < kama.Value[0]); 
                }
            }
        }

        ///  Trade Filter: RSI [Buy below overbought, Sell above oversold] 
        private void CheckRSIFilter()
        {
            if (useRsiFilter)
            {
                double[] rsiValues = new double[rsiCandleLookback];

                // Calculate RSI for a number of candles including the one that just closed
                for (int i = 0; i < rsiCandleLookback; i++)
                {
                    rsiValues[i] = RSI(Close, rsiPeriod, 3)[i];
                }

                rsiBuyFilterOk = rsiValues.All(rsi => rsi < rsiOverbought);
                rsiSellFilterOk = rsiValues.All(rsi => rsi > rsiOversold);
            }
        }
		
		///  Trade Filter: Volume
        private void CheckVolumeFilter()
        {
            if (useVolumeFilter)
            {
				double averageVolume = 0;
				double rocValue = ROC(rocPeriod)[0];
				
				for (int i = 1; i <= volumeLookbackPeriod; i++)
				{
				    averageVolume += Bars.GetVolume(i);
				}
				
				averageVolume /= volumeLookbackPeriod;  // Calculate the average volume (even if the lookback period is 1)
				
				volumeBuyFilterOk = (Bars.GetVolume(0) > averageVolume && rocValue >= rocThreshold);  // volume increasing and rate of change is positive and above threshold
		        volumeSellFilterOk = (Bars.GetVolume(0) > averageVolume && rocValue <= -rocThreshold); // volume increasing and rate of change is negative and below threshold
			}
		}

		/// Trade Filter: Check Candle Wicks
		private void CheckCandleWick()
		{
			if (checkCandleWick)
			{
				double candleSize = High[0] - Low[0];
				double candleWickSize = 0;
				
				if (green)
					candleWickSize = High[0] - Close[0];
				else if (red)
					candleWickSize = Close[0] - Low[0];
				else // probably a doji
				{
					candleWickFilterOk = false;
					return;
				}
				
				candleWickFilterOk = (candleWickSize / candleSize) * 100 < candleWickThreshold;
			}
		}
		
		/// Trade Filter: Check Candle Wicks
		private void CheckCandleSize()
		{
			if (checkCandleSize)
			{
				double candleSize = (High[0] - Low[0]) / TickSize;
				candleSizeFilterOk = (candleSize <= candleSizeThreshold);
			}
		}
		
        /// Display Strategy Performance
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			// call the base.OnRender() to ensure standard Plots work as designed
            base.OnRender(chartControl, chartScale);

            if (updateDisplayMessage)
            {
                SimpleFont sf = new SimpleFont();
                bool flat = (UseAtmStrategy) ? AtmNotSet() : IsFlat();

                string txt = $"{strategy} ({version})";
                TimeSpan t = TimeSpan.FromMilliseconds(clock.ElapsedMilliseconds);
				txt += "\nATM Strategy: " + (UseAtmStrategy ? SelectedATMStrategy : "N/A");
                txt += "\n" + $"ACTIVE since " + dtStart.ToString() + " (" + string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds) + ")";

                double profitFactor = (grossLoss > 0) ? (grossProfit / grossLoss) : double.PositiveInfinity; // No losses yet, profit factor is technically infinite

                txt += "\n" + tradeCount + " trades with profit factor " + profitFactor.ToString("0.##") + " PNL = " + dailyPnL;
                txt += "\nLast Trade: " + sLastTrade;

                if (flat)
                {
                    string message = string.Empty;

                    if (useDailyLossLimit && dailyPnL <= DailyLossLimit)
                    {
                        updateDisplayMessage = false;
                        message = "Daily Loss of " + DailyLossLimit + " has been hit. No More Entries! Daily PnL >> " + dailyPnL + " <<" + Now;
                    }
                    else if (useDailyProfitLimit && dailyPnL >= DailyProfitLimit)
                    {
                        updateDisplayMessage = false;
                        message = "Daily Profit of " + DailyProfitLimit + " has been hit. No more Entries! Daily PnL >>" + dailyPnL + " <<" + Now;
                    }

                    if (message != string.Empty)
                    {
                        txt += "\n" + message;
                        PrintMessage(message);
                    }
                }

                Draw.TextFixed(this, "xdf", txt, TextPosition.BottomLeft, Brushes.White, sf, Brushes.CadetBlue, Brushes.Black, 100);
            }
        }

        /// Update the display panel in the bottom right corner
        private void UpdateTradingWindowPanel()
        {
            SetStartAndEnd();

            string tradingWindowMessage = (start == "N/A") ? "N/A" : start + " - " + end;
            string currentTradingWindow = $"Current Trading Window: {tradingWindowMessage}";
			
            if (sessionDone || (usingTradingWindows && tradeWindow == 0))
                Draw.TextFixed(this, "Session", $"{strategy} ({version}) IS NOT Trading!\nTrading Windows: {tradingWindows}\n{currentTradingWindow}", textPosition, Brushes.Red,
                               new SimpleFont("Arial", 12), Brushes.Crimson, Brushes.Black, 100);
            else if (!sessionDone && (!usingTradingWindows || tradeWindow != 0))
                Draw.TextFixed(this, "Session", $"{strategy} ({version}) IS Trading!\nTrading Windows: {tradingWindows}\n{currentTradingWindow}", textPosition, Brushes.Lime,
                               new SimpleFont("Arial", 12), Brushes.Crimson, Brushes.Black, 100);
        }

		/// Only used for non-ATM strategies
        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity,
                                                  MarketPosition marketPosition, string orderId, DateTime time)
        {
			// only execute this for non-ATM entries
			if (!UseAtmStrategy)
			{
			    if (execution.Order != null && execution.Order.OrderState == OrderState.Filled)
			    {
			        bool exit = (execution.Order.OrderAction == OrderAction.BuyToCover || execution.Order.OrderAction == OrderAction.Sell);
					
					// Check if the order is an exit order
					if (exit)
			        {
						if (IsFlat())	
			            {
							// since we're doing a manual exit, make sure to cancel the OCO orders
							if (stoplossOrder != null)
								CancelOrder(stoplossOrder);
							if (profitTargetOrder != null)
								CancelOrder(profitTargetOrder);
							
							UpdatePnL();
						}
					}
			    }
			}
        }

        private void UpdatePnL()
        {
            // Delayed update: Use Dispatcher to execute code after trade processing
            Dispatcher.InvokeAsync(() => 
            {
                double cumulativePnL = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
                // Calculate PnL for the latest trade based on the difference between the previous and current PnL
                double tradePnL = cumulativePnL - dailyPnL;
                dailyPnL = cumulativePnL; // Update daily PnL
                    
                AnalyzeTrade(tradePnL);
                    
                if (strategyProfitReached)
                    PrintMessage("PROFIT", "Exit");
                else if (accountProfitReached)
                    PrintMessage("ACCOUNT_PROTECTION", "Exit");
                else if (dailyFloatingLossReached)
                    PrintMessage("LOSS", "Exit");
            });

        }

		/// Only for non-ATM strategies making use of limit orders
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled,
											  double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
            if (!UseAtmStrategy && order.Name == entryOrderName)
            {
				if (!scaleInOrder)
                	entryOrder = order; // assign order

                if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.Cancelled)
                {
					if (useLimitOrders)
					{
						limitOrder = null;
						barsSinceEntry = 0;
					}
					
					if (order.OrderState == OrderState.Filled)	
					{
						iOpenPositions++;
						iOpenPositionSize += positionSize;
						
						PrintMessage($"Open Positions = {iOpenPositions} | Position Size = {iOpenPositionSize}");
						
						// TIMING ISSUE: next line added because an existing order could be exiting at the same time we're scaling into that order
						if (entryOrder == null)
						{
							Print("TIMING ISSUE: Scaling into non-existing order!\nChanging scale-in order to initial order...");
							entryOrder = order;
							scaleInOrder = false;
						}
						
	                    if (order.OrderAction == OrderAction.Buy) 
							PlaceBuyExitOrders();
						else
							PlaceSellExitOrders();
					}
                }
				else if (useLimitOrders && order.OrderState == OrderState.Accepted)
				{
                    limitOrder = order;
				}
				else if (order.OrderState == OrderState.Rejected) // Rejections
				{
					if (order.OrderType == OrderType.StopMarket) // Limit Order failed, try again as Market Order
					{
						// reset the order and try one more time at market entry
						Print($"entryOrder (LIMIT ORDER) rejected! Trying once more with MARKET ORDER");
						
						limitOrder = null;
						barsSinceEntry = 0;
						
	                    entryOrder = null;
						SubmitOrderUnmanaged(0, order.OrderAction, OrderType.Market, order.Quantity, 0, 0, "", entryOrderName);
					}
					else // Market Order failed, prepare for next order
					{
						ResetForNextOrder();
					}
				}
            }
        }

		/// Strategy PnL
		private void CalculateStrategyPnl()
		{
            if (SystemPerformance.AllTrades.Count > 0)
            {
                strategyRealizedPnl = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit - previousDayStrategyRealizedPnL;
                strategyUnrealizedPnl = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency) - previousDayStrategyUnrealizedPnL;
                strategyCombinedPnl = (strategyRealizedPnl + strategyUnrealizedPnl);
            }
		}
		
		/// Account PnL
		private void CalculateAccountPnl()
		{
            if (Account != null)
            {
                accountRealizedPnl = Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) - previousDayAccountRealizedPnL;
                accountUnrealizedPnl = Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) - previousDayAccountUnrealizedPnL;
                accountCombinedPnl = (accountRealizedPnl + accountUnrealizedPnl);
            }
		}
		
		protected override void OnAccountItemUpdate(Account account, AccountItem accountItem, double value)
        {
            // update the account pnl
            CalculateAccountPnl();
            
            // update the strategy pnl (only for non-ATM)
            if (!UseAtmStrategy)
                CalculateStrategyPnl();
        }
		
		#region Account Extension
		
		private Order ClosePosition(bool cancelAllOrders = true)	
	    {
	        Position position = Account.GetPosition(Instrument.Id);
	
	        Order closePositionOrder = null;
	
	        switch (position.MarketPosition)
	        {
	            case MarketPosition.Long:
	                closePositionOrder = ExitWithMarketOrder("LONG", position.Quantity);
	                break;
	            case MarketPosition.Short:
	                closePositionOrder = ExitWithMarketOrder("SHORT", position.Quantity);
	                break;
	            case MarketPosition.Flat:
	                break;
	            default:
	                throw new ArgumentOutOfRangeException();
	        }
	
	        if (cancelAllOrders)
	        {
				// cancel all working orders for this instrument
				foreach (Order order in Account.Orders)
			    {
					// make sure to not cancel the order we just submitted above
			        if (order.Instrument.MasterInstrument.Name == Instrument.MasterInstrument.Name && 
						order.Name != "AccExt_ClosePosition") // &&order.OrderState == OrderState.Working)
					{
			            Account.Cancel(new[] { order });
					}
			    }
	        }
	
	        return closePositionOrder;
	    }
	
		/// Exit with a market order
	    private Order ExitWithMarketOrder(string orderType, int quantity, string signalName = "AccExt_ClosePosition", string oco = "", int barsInProgress = 0)
	    {
			OrderAction orderAction = (orderType == "LONG") ? OrderAction.Sell : OrderAction.BuyToCover;
	        Order order = Account.CreateOrder(Instrument, orderAction, OrderType.Market, OrderEntry.Manual,
	            							  TimeInForce.Day, quantity, 0, 0, oco, signalName, Core.Globals.MaxDate, null);
			
	        Account.Submit(new[] { order });
	        return order;
	    }
		
		#endregion
		
		/// helper function to get the current time
		private DateTime Now
		{
          get 
			{ 
				DateTime now = (Bars.Instrument.GetMarketDataConnection().Options.Provider == Provider.Playback ? Bars.Instrument.GetMarketDataConnection().Now : DateTime.Now); 

				if (now.Millisecond > 0)
					now = Core.Globals.MinDate.AddSeconds((long) System.Math.Floor(now.Subtract(Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}
		
        //=======================================================================================		
        #region PROPERTY GROUP: General

        //START: General
		[NinjaScriptProperty]
		[Browsable(false)] // hide propery
	    [Display(Name = "Strategy", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.General, Order = 0)]
	    public string strategy
		{ get { return "SkyFireATM"; } }
		
		[NinjaScriptProperty]
        [Browsable(false)] // hide propery
        [Display(Name = "Version", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.General, Order = 1)]
        public string version
        { get { return "v2.0.4"; } }
		
		[NinjaScriptProperty]
		[ReadOnly(true)]
	    [Display(Name = "Strategy", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.General, Order = 2)]
	    public string strategyName
		{ get { return $"{strategy} {version}"; } }
		
		[NinjaScriptProperty]
		[ReadOnly(true)]
	    [Display(Name = "Developer", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.General, Order = 3)]
	    public string strategyDeveloper
		{ get { return "v1.1 (c) 2024 by TraderOracle, v1.2+ by VirtualMadness"; } }
        //END: General

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Trading Windows

        // START: Trading Window
        [NinjaScriptProperty]
        [Display(Name = "Use Trade Window", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 0)]
        public bool useTradeWindow
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "    --> Start Time", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 1)]
        public DateTime tradeStart
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "    --> End Time", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 2)]
        public DateTime tradeEnd
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Flatten position at expiry", Description = "Close all open positions at trade window exiry", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 3)]
        public Expiry flattenAtExpiry
        { get; set; }
		
		public enum Expiry
		{
			[Description("No")] No,
			[Description("Yes")] Yes,
			[Description("Only if in profit")] Only_If_In_Profit
		}
		
		[NinjaScriptProperty]
        [Display(Name = "    --> Reset Daily PnL (if not reached)", Description = "Daily PnL will be reset at window expiry", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 5)]
        public bool resetPnlAfterTradeWindow
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Trade Window 2", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 10)]
        public bool useTradeWindow2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "    --> Start Time", Description = "Start Time", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 11)]
        public DateTime tradeStart2
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "    --> End Time", Description = "End Time", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 12)]
        public DateTime tradeEnd2
        { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "    --> Flatten position at expiry", Description = "Close all open positions at trade window exiry", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 13)]
        public Expiry flattenAtExpiry2
        { get; set; }
		
        [NinjaScriptProperty]
        [Display(Name = "    --> Reset Daily PnL (if not reached)", Description = "Daily PnL will be reset at window expiry", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 15)]
        public bool resetPnlAfterTradeWindow2
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Trade Window 3", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 20)]
        public bool useTradeWindow3
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "    --> Start Time", Description = "Start Time", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 21)]
        public DateTime tradeStart3
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "    --> End Time", Description = "End Time", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 22)]
        public DateTime tradeEnd3
        { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "    --> Flatten position at expiry", Description = "Close all open positions at trade window exiry", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 23)]
        public Expiry flattenAtExpiry3
        { get; set; }
		
        [NinjaScriptProperty]
        [Display(Name = "    --> Reset Daily PnL (if not reached)", Description = "Daily PnL will be reset at window expiry", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 25)]
        public bool resetPnlAfterTradeWindow3
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Stop after initial winning trades (i.e. first trades of session)", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 30)]
        public bool stopAfterInitialWinningTrades
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> Number of initial winning trades", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 31)]
        public int initialWinningTrades
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Stop after consecutive losses", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 40)]
        public bool stopAfterConsecutiveLosses
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> Number of consecutive losses", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trading_Window, Order = 41)]
        public int consecutiveLosses
        { get; set; }
        // END: Trading Window

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Contracts

        //START: Contracts
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Position Size -> Initial", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Contracts, Order = 0)]
        public int positionSizeInitial
	    { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Scale In", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Contracts, Order = 5)]
        public bool useScaleIn
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
	    [Display(Name = "    --> Scale In Position Size", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Contracts, Order = 6)]
        public int positionSizeScaleIn
        { get; set; }

        [NinjaScriptProperty]
		[Display(Name = "    --> Only scale into profitable position", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Contracts, Order = 7)]
        public bool scaleIntoProfitablePosition
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
	    [Display(Name = "            --> Ticks in profit", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Contracts, Order = 8)]
        public int ticksInProfit
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
	    [Display(Name = "Max Contracts to Open", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Contracts, Order = 10)]
        public int iMaxContracts
        { get; set; }
        //END: Contracts

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Limit Orders

        //START: Limit Orders
        [NinjaScriptProperty]
        [Display(Name = "Use Limit Orders", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Limit_Orders, Order = 0)]
        public bool useLimitOrders
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "    --> Limit Entry Ticks", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Limit_Orders, Order = 5)]
        public int entryTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> Number of bars to wait before cancelling limit order", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Limit_Orders, Order = 10)]
        public int cancelBars
        { get; set; }
        //END: Limit Orders

        #endregion
        //=======================================================================================
		#region PROPERTY GROUP: Trade Settings -> Non-ATM
		
		//START: Trade Settings -> Non-ATM
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Profit Target TICKS", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 0)]
		public int profitTargetTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Stoploss TICKS", Description="Used when Stoploss Type = Fixed", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 5)]
		public int stoplossTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Stoploss Type", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 10)]
        public StoplossType stoplossType
		{ get; set; }
		
		public enum StoplossType 
		{
			[Description("Fixed Stoploss")] Fixed_Stoploss,
			[Description("ATR Stoploss")] ATR,
			[Description("Recent swing High/Low")] RecentSwing_HighLow,
			[Description("Entry candle High/Low")] EntryCandle_HighLow
		}

		[NinjaScriptProperty]
		[Range(1, 240)]
	    [Display(Name = "    --> ATR: Timeframe (minutes)", Description="Used when Stoploss Type = ATR", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 11)]
	    public int atrTimeframe
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> ATR: Lookback Period", Description="Used when Stoploss Type = ATR", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 12)]
	    public int atrLookbackPeriod
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> ATR: Multiplier", Description="Used when Stoploss Type = ATR", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 13)]
	    public int atrMultiplier
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> Recent Swing High/Low: Lookback Period", Description="Used when Stoploss Type = RecentSwing_HighLow", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 14)]
	    public int swingLookbackPeriod
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> Recent Swing High/Low: Tick buffer", Description="Used when Stoploss Type = RecentSwing_HighLow. Number of ticks below Swing LOW or above Swing HIGH to set Stoploss", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 15)]
	    public int swingTickBuffer
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> Entry Candle High/Low: Tick buffer", Description="Used when Stoploss Type = EntryCandle_HighLow. Number of ticks below Entry Candle LOW or above Entry Candle HIGH to set Stoploss", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_NonATM, Order = 16)]
	    public int entryCandleTickBuffer
	    { get; set; }
		//END: Trade Settings -> Non-ATM
		
		#endregion
		//=======================================================================================
        #region PROPERTY GROUP: Trade Settings -> ATM

        //START: ATM
        [NinjaScriptProperty]
        [Display(Name = "Use Ninja ATM", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 0)]
        public bool UseAtmStrategy { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> ATM Strategy", Order = 1, GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM)]
        [TypeConverter(typeof(ATMStrategyConverter))]
        public string SelectedATMStrategy
        {
            get { return selectedATMStrategy; }
            set { selectedATMStrategy = value; }
        }

        // ATM Strategy Select List
        public class ATMStrategyConverter : TypeConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<string> atmStrategies = new List<string>();

                // Define the path to the ATM strategy templates
                string atmTemplatesPath = System.IO.Path.Combine(Core.Globals.UserDataDir, "templates", "AtmStrategy");

                if (Directory.Exists(atmTemplatesPath))
                {
                    foreach (var file in Directory.GetFiles(atmTemplatesPath, "*.xml"))
                    {
                        atmStrategies.Add(System.IO.Path.GetFileNameWithoutExtension(file));
                    }
                }

                return new StandardValuesCollection(atmStrategies);
            }
        }
		
		[NinjaScriptProperty]
		[Display(Name = "Stoploss Type", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 5)]
        public StoplossType atmStoplossType
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 240)]
	    [Display(Name = "    --> ATR: Timeframe (minutes)", Description="Used when Stoploss Type = ATR", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 6)]
	    public int atmAtrTimeframe
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> ATR: Lookback Period", Description="Used when Stoploss Type = ATR", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 7)]
	    public int atmAtrLookbackPeriod
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> ATR: Multiplier", Description="Used when Stoploss Type = ATR", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 8)]
	    public int atmAtrMultiplier
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> Recent Swing High/Low: Lookback Period", Description="Used when Stoploss Type = RecentSwing_HighLow", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 9)]
	    public int atmSwingLookbackPeriod
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> Recent Swing High/Low: Tick buffer", Description="Used when Stoploss Type = RecentSwing_HighLow. Number of ticks below Swing LOW or above Swing HIGH to set Stoploss", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 10)]
	    public int atmSwingTickBuffer
	    { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
	    [Display(Name = "    --> Entry Candle High/Low: Tick buffer", Description="Used when Stoploss Type = EntryCandle_HighLow. Number of ticks below Entry Candle LOW or above Entry Candle HIGH to set Stoploss", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TradeSettings_ATM, Order = 11)]
	    public int atmEntryCandleTickBuffer
	    { get; set; }
        //END: ATM

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: PnL

        //START: PnL
        [NinjaScriptProperty]
        [Display(Name = "Use Daily Profit Limit", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, Order = 0)]
        public bool useDailyProfitLimit
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Daily Profit Limit: Strategy", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, Order = 1)]
        public double DailyProfitLimit
        { get; set; }
		
		[NinjaScriptProperty]
	    [Display(Name = "    --> Daily Profit Limit: Account", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, Order = 2)]
	    public double dailyProfitLimitAccount
	    { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Daily Loss Limit", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, Order = 5)]
        public bool useDailyLossLimit
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Daily Loss Limit (negative amount, eg. -1000)", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, Order = 6)]
        public double DailyLossLimit
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Close position if floating profit >= Daily Profit Limit", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, Order = 10)]
        public bool bExitFloatingProfit
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Close position if floating loss >= Daily Loss Limit", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.PnL, Order = 15)]
        public bool bExitFloatingLoss
		{ get; set; }
        //END: PnL

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Trailing Stop (Non-ATM)

        //START: Trailing Stop
        [NinjaScriptProperty]
        [Display(Name = "Use Trailing Stop", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TrailingStop, Order = 0)]
        public bool useTrailingStop
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "    --> Breakeven TICKS (set to 0 if not using)", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TrailingStop, Order = 5)]
        public int breakevenTicks
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Breakeven Offset TICKS", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TrailingStop, Order = 10)]
        public int breakevenOffsetTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> Trail Start TICKS", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TrailingStop, Order = 15)]
        public int trailStartTicks
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> Trail TICKS", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.TrailingStop, Order = 20)]
        public int trailTicks
        { get; set; }
        //END: Trailing Stop

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Buy/Sell Filters

        // START: Buy/Sell Filters
        [NinjaScriptProperty]
        [Display(Name = "Trade All Volume Imbalances", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 0)]
        public bool bVolImbalance { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trade All MACD/PSAR Big Arrows", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 3)]
        public bool bUseMACDPSARArrow { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Waddah Explosion", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 9)]
        public bool bUseWaddah { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Awesome Oscillator", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 12)]
        public bool bUseAO { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Parabolic SAR", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 15)]
        public bool bUsePSAR { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Squeeze Momentum", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 18)]
        public bool bUseSqueeze { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Linda MACD", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 21)]
        public bool bUseMACD { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Hull Moving Avg", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 24)]
        public bool bUseHMA { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "SuperTrend", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 27)]
        public bool bUseSuperTrend { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "T3", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 30)]
        public bool bUseT3 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Fisher Transform", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 33)]
        public bool bUseFisher { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "When Waddah Exploding", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 36, Description = "Only trade when Waddah Explosion is above the explosion line")]
        public bool bWaddahAboveE1 { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trade Minimum ADX", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 39)]
        public bool bUseMinADX { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Minimum ADX", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 42)]
        public int iMinADX { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trade Minimum Waddah", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 45)]
        public bool bUseWaddahThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Minimum Waddah", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 48, Description = "Waddah Explosion must be at this minimum level to trade")]
        public int iWaddahThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Lizard ADXVMA Plus", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Buy_Sell_Filters, Order = 51)]
        public bool bUseADXVMA { get; set; }
        // END: Buy/Sell Filters

        #endregion
        //=======================================================================================
		#region PROPERTY GROUP: Trend Filter
		
		// START: Trend Filter
		[NinjaScriptProperty]
		[Display(Name="Use 3-EMA Trend Filter (BUY above, SELL below)", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trend_Filter, Order = 0)]
		public bool useEmaTrendFilter
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="    --> EMA Period: Fast", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trend_Filter, Order = 1)]
        public int emaPeriodFast
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="    --> EMA Period: Medium", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trend_Filter, Order = 2)]
        public int emaPeriodMedium
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="    --> EMA Period: Slow", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trend_Filter, Order = 3)]
        public int emaPeriodSlow
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Price Open and Close above/below EMA", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trend_Filter, Order = 5)]
		public bool priceAboveBelowEma
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="    --> EMA Period", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trend_Filter, Order = 6)]
        public int priceEmaPeriod
		{ get; set; }
		// END: Trend Filter
		
		#endregion
		//=======================================================================================
        #region PROPERTY GROUP: Trade Filters

        // START: Trade Filters
		[NinjaScriptProperty]
        [Display(Name = "Waddah Intensity", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 0)]
        public int iWaddahIntense { get; set; }
		
        [NinjaScriptProperty]
        [Display(Name = "Use KAMA (+Slope) Filter", Description = "Buy above KAMA, Sell below KAMA", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 1)]
        public bool useKamaFilter
        { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> KAMA Period", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 2)]
        public int kamaPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Slope Threshold (increase for steeper slope)", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 3)]
        public double kamaSlopeThreshold
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use RSI Filter", Description = "Buy below overbought region, Sell above oversold region", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 5)]
        public bool useRsiFilter
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> RSI Period", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 6)]
        public int rsiPeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, double.MaxValue)]
        [Display(Name = "    --> RSI Overbought", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 7)]
        public double rsiOverbought
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, double.MaxValue)]
        [Display(Name = "    --> RSI Oversold", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 8)]
        public double rsiOversold
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "    --> Overbought/Oversold Candle Lookback", Description = "Number of previous candles to check if they were in the overbought/oversold region", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 9)]
        public int rsiCandleLookback
        { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Check candle wicks", Description = "Check top candle wick for BUY, bottom candle wick for SELL", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 15)]
        public bool checkCandleWick
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="    --> Candle wick threshold (percent)", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 16)]
        public double candleWickThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Check candle size", Description = "Check tick size of candle", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 20)]
        public bool checkCandleSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="    --> Candle size threshold", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 21)]
        public double candleSizeThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Use VOLUME Filter", Description = "Uses Rate Of Change (ROC) and Bar Volume over a lookback period", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 25)]
        public bool useVolumeFilter
        { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="    --> ROC Period", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 26)]
        public int rocPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="    --> ROC Threshold", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 27)]
        public double rocThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="    --> Volume Lookback Period", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Trade_Filters, Order = 29)]
        public int volumeLookbackPeriod
		{ get; set; }
        // END: Trade Filters

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Exit Conditions (Non-ATM)

        // START: Exit Conditions
        [NinjaScriptProperty]
        [Display(Name = "KAMA Cross", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Exit_Conditions, Order = 0)]
        public bool bExitKama { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Type of cross", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Exit_Conditions, Order = 1)]
        public CrossType kamaCrossType { get; set; }

        public enum CrossType
        {
            [Description("Close Above/Below KAMA")] Close_AboveOrBelow_KAMA,
            [Description("Open and Close Above/Below KAMA")] OpenAndClose_AboveOrBelow_KAMA
        }

        [NinjaScriptProperty]
        [Display(Name = "Waddah reversal", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Exit_Conditions, Order = 2)]
        public bool bExitWaddah { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Parabolic SAR reversal", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Exit_Conditions, Order = 3)]
        public bool bExitPSAR { get; set; }
		// END: Exit Conditions
		
        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Telegram

        // START: Telegram
        [NinjaScriptProperty]
        [Display(Name = "Use Telegram Notifications", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Telegram, Order = 0)]
        public bool useTelegram
        { get; set; }

        [Display(Name = "    --> Telegram Channel (ID)", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Telegram, Order = 5)]
        public string telegramChannel
        { get; set; }

        [Display(Name = "    --> Telegram Token", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Telegram, Order = 10)]
        public string telegramToken
        { get; set; }
        // END: Telegram

        #endregion
        //=======================================================================================
        #region PROPERTY GROUP: Other

        // START: Other
        [NinjaScriptProperty]
        [Display(Name = "Display Ouput Messages", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Other, Order = 0)]
        public bool displayOutputMessages
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Print to 2nd Ouput tab", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Other, Order = 1)]
        public bool printoTab2
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Display Trading Window Panel", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Other, Order = 5)]
        public bool displayTradingWindowPanel
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "    --> Display Location", GroupName = TraderOracle.SkyFire_v2_0_4.PropertyGroup.Other, Order = 6)]
        public DisplayLocation tradingWindowPanelLocation { get; set; }

        public enum DisplayLocation
        {
            [Description("Top Left")] TopLeft,
            [Description("Top Right")] TopRight,
            [Description("Bottom Right")] BottomRight,
            [Description("Center")] Center
        }
        // START: Other

        #endregion
        //=======================================================================================

    }

}