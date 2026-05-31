#region Using declarations
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Core;
using BlueZ = NinjaTrader.NinjaScript.Indicators.BlueZ; // Alias for better readability
using RegressionChannel = NinjaTrader.NinjaScript.Indicators.RegressionChannel;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.KCStrategies
{
    public class ManualTraderATM : Strategy, ICustomTypeDescriptor //
    {
		#region Variables		
		
		// ATM Strategy Variables
		private string atmStrategyId = string.Empty;
		private string orderId = string.Empty;
		private bool isAtmStrategyCreated = false;
		private DateTime lastEntryTime;

		// Indicator Variables
		private BlueZ.BlueZHMAHooks hullMAHooks;
		public bool hmaHooksUp;
		public bool hmaHooksDown;

       	private RegressionChannel RegressionChannel1, RegressionChannel2;
		private RegressionChannelHighLow RegressionChannelHighLow1;	
		private bool regChanUp;
		private bool regChanDown;
		
		private VMA vmaIndicator;
		public bool volMaUp;
		public bool volMaDown;
		
		private Momentum momentumIndicator;	
		public bool momoUp;
		public bool momoDown;
		
		private ADX adxIndicator;	
		
		// Trend Variables
		public bool uptrend;
		public bool downtrend;

		// Signal Variables
		public bool longSignal;
		public bool shortSignal;
		
		// Position Variables
		public bool isLong;
		public bool isShort;
		public bool isFlat;

		// Progress Tracking
		private double actualPnL;
		private bool trailingDrawdownReached = false;

		private double entryPrice;
		private double currentPrice;	
		
		// Trade Direction Management
		private bool tradesPerDirection;
		private int counterLong;
		private int counterShort;
		
		// Quick Order Buttons
		private bool QuickLong;
		private bool QuickShort;
		private bool quickLongBtnActive;
		private bool quickShortBtnActive;

		// Time Management
		private bool isEnableTime2;
		private bool isEnableTime3;
		private bool isEnableTime4;
		private bool isEnableTime5;
		private bool isEnableTime6;

		// Strategy Enablement
		private bool isStrategyEnabled = true; // Default to enabled
		private bool isLongEnabled = true; // Default to enabled
		private bool isShortEnabled = true; // Default to enabled

		// WPF Control Variables
		private RowDefinition addedRow;
		private ChartTab chartTab;
		private Chart chartWindow;
		private Grid chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid;

		private Button strategyBtn, longBtn, shortBtn, quickLongBtn, quickShortBtn;
		private Button moveTSBtn, moveToBEBtn;
		private Button moveTS50PctBtn, closeBtn, panicBtn;
		private bool panelActive;
		private TabItem tabItem;
		private Grid myGrid;

		// P&L Variables
		private double totalPnL;
		private double cumPnL;
		private double dailyPnL;
		private bool canTradeOK = true;
		private bool canTradeToday;
		private bool runOnce = false;

		private bool syncPnl;
		private double historicalTimeTrades; // Sync P&L
		private double dif; // To Calculate PNL sync
		private double cumProfit; // For real time pnl and pnl synchronization

		private bool restartPnL;
		
		// Error Handling
		private readonly object orderLock = new object(); // Critical for thread safety
		private Dictionary<string, Order> activeOrders = new Dictionary<string, Order>(); // Track active orders with labels.
		private DateTime lastOrderActionTime = DateTime.MinValue;
		private readonly TimeSpan minOrderActionInterval = TimeSpan.FromSeconds(1); // Prevent rapid order submissions.
		private bool orderErrorOccurred = false; // Flag to halt trading after an order error.

		// Rogue Order Detection
		private DateTime lastAccountReconciliationTime = DateTime.MinValue;
		private readonly TimeSpan accountReconciliationInterval = TimeSpan.FromMinutes(5); // Check for rogue orders every 5 minutes

		// Trailing Drawdown variables
		private double maxProfit;  // Stores the highest profit achieved

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

//		// KillAll 
		private Account chartTraderAccount;

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
		
		#endregion
		
		public override string DisplayName { get { return Name; } }
		
		#region OnStateChange
		protected override void OnStateChange()
        {
			switch (State)
			{
				case State.SetDefaults:
					ConfigureStrategyDefaults();
					break;
				case State.Configure:
					ConfigureStrategy();
					break;
				case State.DataLoaded:
					InitializeIndicators();
					LoadChartTraderButtons();
					maxProfit = totalPnL;
					break;
				case State.Historical:
					break;
				case State.Terminated:
					CleanUpStrategy();
					break;
			}
		}
			
        private void ConfigureStrategyDefaults()
		{
			Description = @"Base Strategy with OEB v.5.0.2 TradeSaber(Dre). and ArchReactor for KC (Khanh Nguyen)";
			Name = "Manual Trader ATM";
			BaseAlgoVersion = "Manual Trader ATM v4.7";
			Author = "indiVGA, Khanh Nguyen, Oshi, based on ArchReactor";
			Version = "Version 4.7 Mar. 2025";
			Credits = "";
			StrategyName = "";
			ChartType = "Orenko 34-40-40"; // TODO: Document Magic Numbers

			EntriesPerDirection = 10;
			Calculate = Calculate.OnPriceChange;
			EntryHandling = EntryHandling.AllEntries;
			IsExitOnSessionCloseStrategy = true;
			ExitOnSessionCloseSeconds = 30;
			IsFillLimitOnTouch = false;
			MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
			OrderFillResolution = OrderFillResolution.High;
			Slippage = 0;
			StartBehavior = StartBehavior.WaitUntilFlat;
			TimeInForce = TimeInForce.Gtc;
			TraceOrders = false;
			RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
			StopTargetHandling = StopTargetHandling.PerEntryExecution;
			BarsRequiredToTrade = 20;
			IsInstantiatedOnEachOptimizationIteration = false;

			// Default Parameters
			isStrategyEnabled = true;
			isLongEnabled = true;
			isShortEnabled = true;
			canTradeOK = true;
			runOnce = false;

			OrderType = OrderType.Limit;
			ATMStrategyTemplate = "ATM";

			HmaPeriod = 12;
			enableHmaHooks = true;
			showHmaHooks = true;

			RegChanPeriod = 40;
			RegChanWidth = 4;
			RegChanWidth2 = 3;
			enableRegChan1 = false;
			enableRegChan2 = false;
			showRegChan1 = true;
			showRegChan2 = true;
			showRegChanHiLo = true;

			enableVMA = false;
			showVMA = true;

			MomoUp = 1;
			MomoDown = -1;
			enableMomo = false;
			showMomo = true;

			showAdx = false;
			adxPeriod = 7;

			TickMove = 4;
			BreakevenOffset = 4;
			
			tradesPerDirection = false;
			longPerDirection = 5;
			shortPerDirection = 5;

			QuickLong = false;
			QuickShort = false;

			counterLong = 0;
			counterShort = 0;

			Start = DateTime.Parse("06:30", System.Globalization.CultureInfo.InvariantCulture);
			End = DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
			Start2 = DateTime.Parse("07:31", System.Globalization.CultureInfo.InvariantCulture);
			End2 = DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
			Start3 = DateTime.Parse("08:01", System.Globalization.CultureInfo.InvariantCulture);
			End3 = DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
			Start4 = DateTime.Parse("12:01", System.Globalization.CultureInfo.InvariantCulture);
			End4 = DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
			Start5 = DateTime.Parse("06:30", System.Globalization.CultureInfo.InvariantCulture);
			End5 = DateTime.Parse("13:00", System.Globalization.CultureInfo.InvariantCulture);
			Start6 = DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
			End6 = DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);

			// Panel Status
			showDailyPnl = true;
			PositionDailyPNL = TextPosition.TopLeft;
			colorDailyProfitLoss = Brushes.Cyan;

			showPnl = false;
			PositionPnl = TextPosition.BottomLeft;
			colorPnl = Brushes.Yellow;

			// PnL Daily Limits
			dailyLossProfit = true;
			DailyProfitLimit = 100000;
			DailyLossLimit = 1000;
			TrailingDrawdown = 1000;
			StartTrailingDD = 3000;
			maxProfit = double.MinValue;
			enableTrailingDD = true;
		}
			
        private void ConfigureStrategy()
		{
			RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
		}
		
		private void InitializeIndicators()
		{
			hullMAHooks = BlueZHMAHooks(Close, HmaPeriod, 0, false, false, true, Brushes.Lime, Brushes.Red);
			hullMAHooks.Plots[0].Brush = Brushes.White;
			hullMAHooks.Plots[0].Width = 2;
			if (showHmaHooks) AddChartIndicator(hullMAHooks);

			RegressionChannel1 = RegressionChannel(Close, RegChanPeriod, RegChanWidth);
			if (showRegChan1) AddChartIndicator(RegressionChannel1);

			RegressionChannel2 = RegressionChannel(Close, RegChanPeriod, RegChanWidth2);
			if (showRegChan2) AddChartIndicator(RegressionChannel2);

			RegressionChannelHighLow1 = RegressionChannelHighLow(Close, RegChanPeriod, RegChanWidth);
			if (showRegChanHiLo) AddChartIndicator(RegressionChannelHighLow1);

			vmaIndicator = VMA(Close, 9, 9);
			vmaIndicator.Plots[0].Brush = Brushes.SkyBlue;
			vmaIndicator.Plots[0].Width = 3;
			if (showVMA) AddChartIndicator(vmaIndicator);

			momentumIndicator = Momentum(Close, 14);
			momentumIndicator.Plots[0].Brush = Brushes.Yellow;
			momentumIndicator.Plots[0].Width = 2;
			if (showMomo) AddChartIndicator(momentumIndicator);

			adxIndicator = ADX(Close, adxPeriod);
			adxIndicator.Plots[0].Brush = Brushes.Cyan;
			adxIndicator.Plots[0].Width = 2;
			if (showAdx) AddChartIndicator(adxIndicator);

			maxProfit = totalPnL;
		}
			
		private void LoadChartTraderButtons()
		{
			Dispatcher.InvokeAsync(() => { CreateWPFControls(); });
		}

		private void CleanUpStrategy()
		{
			ChartControl?.Dispatcher.InvokeAsync(() => { DisposeWPFControls(); });

			clientWebSocket?.Dispose();

			lock (orderLock)
			{
				if (activeOrders.Count > 0)
				{
					Print($"{Time[0]}: Strategy terminated with active orders. Investigate:");
					foreach (var kvp in activeOrders)
					{
						Print($"{Time[0]}: Order Label: {kvp.Key}, Order ID: {kvp.Value.OrderId}");
						CancelOrder(kvp.Value);
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
	
			if (Bars.IsFirstBarOfSession)
			{
			    canTradeToday = true;
				cumPnL 			= totalPnL; ///Double that copies the full session PnL (If trading multiple days). Is only calculated once per day.
				dailyPnL		= totalPnL - cumPnL; ///Subtract the copy of the full session by the full session PnL. This resets your daily PnL back to 0.
				Print ($"{Time[0]} //On Bar Update//// IsFirst Bar of SessioncumPnL: {cumPnL}, dailyPnL: {dailyPnL}, totalPnL: {totalPnL}, CumProfit is {SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit}");

			}
			
			if (!canTradeToday || State == State.Historical) return;

			//Track the Highest Profit Achieved
			if (totalPnL > maxProfit)
			{
				maxProfit = totalPnL;
                Print ($"{Time[0]} ///On Bar Update//// Updated maxProfit: {maxProfit}");

			}

			dailyPnL = (totalPnL) - (cumPnL); ///Your daily limit is the difference between these

			// Account Reconciliation
			if (DateTime.Now - lastAccountReconciliationTime > accountReconciliationInterval)
			{
				ReconcileAccountOrders();
				lastAccountReconciliationTime = DateTime.Now;
			}

			regChanUp = !enableRegChan1 || RegressionChannel1.Middle[0] > RegressionChannel1.Middle[2];
			regChanDown = !enableRegChan1 || RegressionChannel1.Middle[0] < RegressionChannel1.Middle[2];

			hmaHooksUp = !enableHmaHooks || hullMAHooks[0] > hullMAHooks[2];
			hmaHooksDown = !enableHmaHooks || hullMAHooks[0] < hullMAHooks[2];

			volMaUp = !enableVMA || Close[0] > vmaIndicator[0];
			volMaDown = !enableVMA || Close[0] < vmaIndicator[0];

			momoUp = !enableMomo || (momentumIndicator[0] > MomoUp && momentumIndicator[0] > momentumIndicator[1]);
			momoDown = !enableMomo || (momentumIndicator[0] < MomoDown && momentumIndicator[0] < momentumIndicator[1]);

			uptrend = volMaUp && hmaHooksUp && regChanUp && momoUp;
			downtrend = volMaDown && hmaHooksDown && regChanDown && momoDown;

			entryPrice = Position.AveragePrice;
			currentPrice = Close[0];

			UpdatePositionState();

			if (Bars.IsFirstBarOfSession)
			{
				cumPnL = totalPnL;
				dailyPnL = totalPnL - cumPnL;
			}

			if (showPnl) ShowPNLStatus();

			ProcessLongEntry();
			ProcessShortEntry();

			if (!isAtmStrategyCreated)
				return;

			UpdateAtmStrategyStatus();

			if (atmStrategyId.Length > 0)
			{
				UpdateStopPrice();
				PrintAtmStrategyInfo();
			}

			ResetTradesPerDirection();
			ResetStopLoss();
			KillSwitch();
        }
		
		#endregion
		
		private void UpdatePositionState()
		{
			isLong = Position.MarketPosition == MarketPosition.Long;
			isShort = Position.MarketPosition == MarketPosition.Short;
			isFlat = Position.MarketPosition == MarketPosition.Flat;
		}

		private bool AtmStrategyNotActive()
        {
            return orderId.Length == 0 && atmStrategyId.Length == 0;
        }
		
		private void ProcessLongEntry()
		{
			if (IsLongEntryConditionMet())
			{
				if (!tradesPerDirection || (tradesPerDirection && counterLong < longPerDirection))
				{
					counterLong++;
					counterShort = 0;
					runOnce = true;

					CreateAtmStrategy(OrderAction.Buy, LongEntryLabel, Brushes.Cyan);
				}
				else
				{
					Print("Limit long trades in a row");
				}
			}
		}

		private void ProcessShortEntry()
		{
			if (IsShortEntryConditionMet())
			{
				if (!tradesPerDirection || (tradesPerDirection && counterShort < shortPerDirection))
				{
					counterLong = 0;
					counterShort++;
					runOnce = true;

					CreateAtmStrategy(OrderAction.SellShort, ShortEntryLabel, Brushes.Yellow);
				}
				else
				{
					Print("Limit short trades in a row");
				}
			}
		}

		private bool IsLongEntryConditionMet()
		{
			return isLong
				   && AtmStrategyNotActive()
				   && (isStrategyEnabled)
				   && (isLongEnabled)
				   && (checkTimers())
				   && ((dailyLossProfit ? dailyPnL > -DailyLossLimit : true))
				   && ((dailyLossProfit ? dailyPnL < DailyProfitLimit : true))
				   && (isFlat)
				   && (uptrend)
				   && (!trailingDrawdownReached)
				   && (canTradeOK)
				   && (canTradeToday);
		}

		private bool IsShortEntryConditionMet()
		{
			return isShort
				   && AtmStrategyNotActive()
				   && (isStrategyEnabled)
				   && (isShortEnabled)
				   && (checkTimers())
				   && ((dailyLossProfit ? dailyPnL > -DailyLossLimit : true))
				   && ((dailyLossProfit ? dailyPnL < DailyProfitLimit : true))
				   && (isFlat)
				   && (downtrend)
				   && (!trailingDrawdownReached)
				   && (canTradeOK)
				   && (canTradeToday);
		}

		private void ResetTradesPerDirection()
		{
			if (tradesPerDirection)
			{
				if (counterLong != 0 && Close[1] < Open[1])
					counterLong = 0;
				if (counterShort != 0 && Close[1] > Open[1])
					counterShort = 0;
			}
		}

		private void ResetStopLoss()
		{
			if (isFlat)
			{
				quickLongBtnActive = false;
				quickShortBtnActive = false;

				longSignal = false;
				shortSignal = false;

				if (runOnce)
				{
					lastEntryTime = Time[0];
					Print($"{Time[0]} Timer activated");
					canTradeOK = false;
					runOnce = false;
				}

				lock (orderLock)
				{
					activeOrders.Clear();
				}
			}
		}
		
		#region ATM Strategy Methods

		private void CreateAtmStrategy(OrderAction orderAction, string signalName, Brush signalBrush)
		{
			isAtmStrategyCreated = false;
		    atmStrategyId = GetAtmStrategyUniqueId();
		    orderId = GetAtmStrategyUniqueId();

			Print($"Your atmStrategyId is: {atmStrategyId} OrderId is: {orderId}");

			OrderType orderType = (OrderType == OrderType.Market) ? OrderType.Market : OrderType.Limit;
			double orderPrice = (orderType == OrderType.Limit) ? (orderAction == OrderAction.Buy ? GetCurrentBid() : GetCurrentAsk()) : 0;

			AtmStrategyCreate(orderAction, orderType, orderPrice, 0, TimeInForce.Gtc, orderId, ATMStrategyTemplate, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) =>
			{
				if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
					isAtmStrategyCreated = true;
			});

			DrawArrow(signalName, orderPrice, signalBrush);
		}

		private void DrawArrow(string signalName, double signalPrice, Brush signalBrush)
		{
			if (signalName == LongEntryLabel)
				Draw.ArrowUp(this, signalName + CurrentBars[0], false, 0, signalPrice, signalBrush);
			else if (signalName == ShortEntryLabel)
				Draw.ArrowDown(this, signalName + CurrentBars[0], false, 0, signalPrice, signalBrush);
		}

		private void UpdateAtmStrategyStatus()
		{
			if (orderId.Length > 0)
			{
				string[] status = GetAtmStrategyEntryOrderStatus(orderId);

				if (status.Length > 0)
				{
					PrintOrderStatus(status);
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderId = string.Empty;
				}
			}
			else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == MarketPosition.Flat)
			{
				atmStrategyId = string.Empty;
			}
		}

		private void PrintOrderStatus(string[] status)
		{
			Print($"The entry order average fill price is: {status[0]}");
			Print($"The entry order filled amount is: {status[1]}");
			Print($"The entry order order state is: {status[2]}");
		}

		private void PrintAtmStrategyInfo()
		{
			Print($"The current ATM Strategy market position is: {GetAtmStrategyMarketPosition(atmStrategyId)}");
			Print($"The current ATM Strategy position quantity is: {GetAtmStrategyPositionQuantity(atmStrategyId)}");
			Print($"The current ATM Strategy average price is: {GetAtmStrategyPositionAveragePrice(atmStrategyId)}");
			Print($"The current ATM Strategy Unrealized PnL is: {GetAtmStrategyUnrealizedProfitLoss(atmStrategyId)}");
		}
		#endregion

        private void UpdateStopPrice()
        {
            MarketPosition marketPosition = GetAtmStrategyMarketPosition(atmStrategyId);
            double newStopPrice = 0;

            if (marketPosition == MarketPosition.Long)
            {
                newStopPrice = Low[0] - 3 * TickSize;
                if (newStopPrice < GetCurrentBid())
                {
                    AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
                }
            }
            else if (marketPosition == MarketPosition.Short)
            {
                newStopPrice = High[0] + 3 * TickSize;
                if (newStopPrice > GetCurrentAsk())
                {
                    AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
                }
            }
        }

		#region Order Submission Helpers

		// This method encapsulates all order submissions and error handling.
		private Order SubmitEntryOrder(string orderLabel, OrderType orderType, int contracts)
		{
			Order submittedOrder = null;

			lock (orderLock)
			{
				if (!CanSubmitOrder())
				{
					Print($"{Time[0]}: Cannot submit {orderLabel} order: Minimum order interval not met.");
					return null;
				}

				try
				{
					submittedOrder = SubmitOrder(orderLabel, orderType, contracts);

					if (submittedOrder != null)
					{
						activeOrders[orderLabel] = submittedOrder;
						lastOrderActionTime = DateTime.Now;
						Print($"{Time[0]}: Submitted {orderLabel} order with OrderId: {submittedOrder.OrderId}");
					}
					else
					{
						Print($"{Time[0]}: Error: {orderLabel} Entry order was null after submission.");
						orderErrorOccurred = true;
					}
				}
				catch (Exception ex)
				{
					Print($"{Time[0]}: Error submitting {orderLabel} entry order: {ex.Message}");
					orderErrorOccurred = true;
				}
			}

			return submittedOrder;
		}
		
		private Order SubmitOrder(string orderLabel, OrderType orderType, int contracts)
		{
			switch (orderType)
			{
				case OrderType.Market:
					if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
						return EnterLong(contracts, orderLabel);
					else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
						return EnterShort(contracts, orderLabel);
					break;
				case OrderType.Limit:
					if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
						return EnterLongLimit(contracts, GetCurrentBid(), orderLabel);
					else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
						return EnterShortLimit(contracts, GetCurrentAsk(), orderLabel);
					break;
				case OrderType.MIT:
					if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
						return EnterLongMIT(contracts, GetCurrentBid(), orderLabel);
					else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
						return EnterShortMIT(contracts, GetCurrentAsk(), orderLabel);
					break;
				case OrderType.StopLimit:
					if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
						return EnterLongLimit(contracts, GetCurrentBid(), orderLabel);
					else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
						return EnterShortLimit(contracts, GetCurrentAsk(), orderLabel);
					break;
				case OrderType.StopMarket:
					if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel)
						return EnterLong(contracts, orderLabel);
					else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel)
						return EnterShort(contracts, orderLabel);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(orderType), orderType, "Unsupported order type");
			}
			return null;
		}

		private void SubmitExitOrder(string orderLabel)
		{
			lock (orderLock)
			{
				try
				{
					if (orderLabel == LongEntryLabel || orderLabel == QuickLongEntryLabel || orderLabel == Add1LongEntryLabel)
					{
						ExitLong(orderLabel);
					}
					else if (orderLabel == ShortEntryLabel || orderLabel == QuickShortEntryLabel || orderLabel == Add1ShortEntryLabel)
					{
						ExitShort(orderLabel);
					}
					else
					{
						Print($"Error: invalid order label {orderLabel}");
					}

					if (!activeOrders.ContainsKey(orderLabel))
						Print($"Cannot cancel order that does not exist");

					if (activeOrders.TryGetValue(orderLabel, out Order orderToCancel))
					{
						CancelOrder(orderToCancel);
						activeOrders.Remove(orderLabel);
					}
				}
				catch (Exception ex)
				{
					Print($"Error submitting Exit order: {ex.Message}");
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
					var accounts = Account.All;

					if (accounts == null || accounts.Count == 0)
					{
						Print($"{Time[0]}: No accounts found.");
						return;
					}

					foreach (Account account in accounts)
					{
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
							Print($"{Time[0]}: Error getting orders for account {account.Name}: {ex.Message}");
							continue;
						}

						if (accountOrders == null || accountOrders.Count == 0)
						{
							Print($"{Time[0]}: No orders found in account {account.Name}.");
							continue;
						}

						HashSet<string> strategyOrderIds = new HashSet<string>(activeOrders.Values.Select(o => o.OrderId));

						foreach (Order accountOrder in accountOrders)
						{
							if (!strategyOrderIds.Contains(accountOrder.OrderId))
							{
								Print($"{Time[0]}: Rogue order detected! Account: {accountOrder.Account.Name} OrderId: {accountOrder.OrderId}, OrderType: {accountOrder.OrderType}, OrderStatus: {accountOrder.OrderState}, Quantity: {accountOrder.Quantity}, AveragePrice: {accountOrder.AverageFillPrice}");

								try
								{
									CancelOrder(accountOrder);
									Print($"{Time[0]}: Attempted to cancel rogue order: {accountOrder.OrderId}");
								}
								catch (Exception ex)
								{
									Print($"{Time[0]}: Failed to Cancel rogue order. Account: {accountOrder.Account.Name} OrderId: {accountOrder.OrderId}, OrderType: {accountOrder.OrderType}, OrderStatus: {accountOrder.OrderState}, Quantity: {accountOrder.Quantity}, AveragePrice: {accountOrder.AverageFillPrice}, Reason: {ex.Message}");
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Print($"{Time[0]}: Error during account reconciliation: {ex.Message}");
					orderErrorOccurred = true;
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
							Print($"{Time[0]}: Order {orderId} with label {orderLabel} filled.");
							activeOrders.Remove(orderLabel); // Remove the order when it's filled.

							if (execution.Order.OrderState == OrderState.Filled && isFlat)
							{
								if (execution.Order.Name.StartsWith("LE") || execution.Order.Name.StartsWith("QLE") || execution.Order.Name.StartsWith("Add1LE"))
								{
									counterLong = 0;
								}
								else if (execution.Order.Name.StartsWith("SE") || execution.Order.Name.StartsWith("QSE") || execution.Order.Name.StartsWith("Add1SE"))
								{
									counterShort = 0;
								}
							}

							break;

						case OrderState.Cancelled:
							Print($"{Time[0]}: Order {orderId} with label {orderLabel} cancelled.");
							activeOrders.Remove(orderLabel); // Remove cancelled orders
							break;

						case OrderState.Rejected:
							Print($"{Time[0]}: Order {orderId} with label {orderLabel} rejected.");
							activeOrders.Remove(orderLabel); // Remove rejected orders
							break;

						default:
							Print($"{Time[0]}: Order {orderId} with label {orderLabel} updated to state: {execution.Order.OrderState}");
							break;
					}
				}
				else
				{
					// This could indicate a rogue order or an order not tracked by the strategy.
					Print($"{Time[0]}: Execution update for order {orderId}, but order is not tracked by the strategy.");

					// Attempt to Cancel the Rogue Order
					try
					{
						CancelOrder(execution.Order);
						Print($"{Time[0]}: Successfully Canceled the Rogue Order: {orderId}.");
					}
					catch (Exception ex)
					{
						Print($"{Time[0]}: Could not Cancel the Rogue Order: {orderId}. {ex.Message}");
						orderErrorOccurred = true;  // Consider whether to halt trading
					}
				}
			}
		}

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
				
				// Re-enable the strategy if it was disabled by the DD and totalPnL increases
				if (enableTrailingDD && trailingDrawdownReached && totalPnL > maxProfit - TrailingDrawdown)
	            {
	                trailingDrawdownReached = false;
					isStrategyEnabled = true;
					Print("Trailing Drawdown Lifted. Strategy Re-Enabled!");
				}
	
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
		
		#endregion	
		
		#region Chart Trader Button Handling
		protected void DecorateButton(Button button, string content, Brush background, Brush borderBrush, Brush foreground)
		{
			button.Content = content;
			button.Background = background;
			button.BorderBrush = borderBrush;
			button.Foreground = foreground;
		}

		protected void DecoreDisabledButtons(Button myButton, string stringButton)
		{
			DecorateButton(myButton, stringButton, Brushes.DarkRed, Brushes.Black, Brushes.White);
		}

		protected void DecorateEnabledButtons(Button myButton, string stringButton)
		{
			DecorateButton(myButton, stringButton, Brushes.DarkGreen, Brushes.Black, Brushes.White);
		}

		protected void DecorateNeutralButtons(Button myButton, string stringButton)
		{
			DecorateButton(myButton, stringButton, Brushes.LightGray, Brushes.Black, Brushes.Black);
		}

		protected void DecorateGrayButtons(Button myButton, string stringButton)
		{
			DecorateButton(myButton, stringButton, Brushes.DarkGray, Brushes.Black, Brushes.Black);
		}

		protected void CreateWPFControls()
		{
			chartWindow = System.Windows.Window.GetWindow(ChartControl.Parent) as Chart;

			if (chartWindow == null)
				return;

			chartTraderGrid = (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as Grid;
			chartTraderButtonsGrid = chartTraderGrid.Children[0] as Grid;

		    InitializeButtonGrid(); // Call InitializeButtonGrid FIRST
		    CreateButtons(); // Call CreateButtons BEFORE SetButtonLocations and AddButtonsToGrid

			addedRow = new RowDefinition() { Height = new GridLength(250) };

			SetButtonLocations();
			AddButtonsToGrid();
	
			if (TabSelected())
				InsertWPFControls();
	
			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		protected void CreateButtons()
		{
			Style basicButtonStyle = System.Windows.Application.Current.FindResource("BasicEntryButton") as Style;
	
			strategyBtn = CreateButton("\uD83D\uDD12 Manual ATM On", basicButtonStyle, "Enable (Green) / Disbled (Red) Strategy", OnButtonClick);
			if (isStrategyEnabled) DecorateEnabledButtons(strategyBtn, "\uD83D\uDD12 Manual ATM On");
			else DecoreDisabledButtons(strategyBtn, "\uD83D\uDD13 Manual ATM Off");
	
			longBtn = CreateButton("LONG", basicButtonStyle, "Enable (Green) / Disbled (Red) Auto Long Entry", OnButtonClick);
			if (isLongEnabled) DecorateEnabledButtons(longBtn, "LONG");
			else DecoreDisabledButtons(longBtn, "LONG Off");
	
			shortBtn = CreateButton("SHORT", basicButtonStyle, "Enable (Green) / Disbled (Red) Auto Short Entry", OnButtonClick);
			if (isShortEnabled) DecorateEnabledButtons(shortBtn, "SHORT");
			else DecoreDisabledButtons(shortBtn, "SHORT Off");
	
			quickLongBtn = CreateButton("Buy", basicButtonStyle, "Buy Market Entry", OnButtonClick);
			DecorateEnabledButtons(quickLongBtn, "Buy");
	
			quickShortBtn = CreateButton("Sell", basicButtonStyle, "Sell Market Entry", OnButtonClick);
			DecoreDisabledButtons(quickShortBtn, "Sell");
	
			moveTSBtn = CreateButton("Move TS", basicButtonStyle, "Increase trailing stop", OnButtonClick, Brushes.DarkBlue, Brushes.Yellow);
			moveTS50PctBtn = CreateButton("Move TS 50%", basicButtonStyle, "Move trailing stop 50% closer to the current price", OnButtonClick, Brushes.DarkBlue, Brushes.Yellow);
			moveToBEBtn = CreateButton("Breakeven", basicButtonStyle, "Move stop to breakeven if in profit", OnButtonClick, Brushes.DarkBlue, Brushes.White);
	
			closeBtn = CreateButton("Close All Positions", basicButtonStyle, "Manual Close: CloseAllPosiions manually", OnButtonClick, Brushes.DarkRed, Brushes.White);
			panicBtn = CreateButton("\u2620 Panic Shutdown", basicButtonStyle, "PanicBtn: CloseAllPosiions", OnButtonClick, Brushes.DarkRed, Brushes.Yellow);
		}

		private Button CreateButton(string content, Style style, string toolTip, RoutedEventHandler clickHandler, Brush background = null, Brush foreground = null)
		{
			Button button = new Button
			{
				Content = content,
				Height = 25,
				Margin = new Thickness(1, 0, 1, 0),
				Padding = new Thickness(0, 0, 0, 0),
				Style = style,
				BorderThickness = new Thickness(1.5),
				IsEnabled = true,
				ToolTip = toolTip
			};
	
			if (background != null) button.Background = background;
			if (foreground != null) button.Foreground = foreground;
	
			button.Click += clickHandler;
	
			return button;
		}

		protected void InitializeButtonGrid()
		{
			lowerButtonsGrid = new Grid();
	
			for (int i = 0; i < 2; i++)
			{
				lowerButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
			}
	
			for (int i = 0; i <= 9; i++)
			{
				lowerButtonsGrid.RowDefinitions.Add(new RowDefinition());
			}
		}

		protected void SetButtonLocations()
		{
			SetButtonLocation(strategyBtn, 0, 1, 2);
			SetButtonLocation(longBtn, 0, 2);
			SetButtonLocation(shortBtn, 1, 2);
			SetButtonLocation(quickLongBtn, 0, 3);
			SetButtonLocation(quickShortBtn, 1, 3);
			SetButtonLocation(moveTSBtn, 0, 4);
			SetButtonLocation(moveTS50PctBtn, 1, 4);
			SetButtonLocation(moveToBEBtn, 0, 5, 2);
			SetButtonLocation(closeBtn, 0, 6, 2);
			SetButtonLocation(panicBtn, 0, 7, 2);
		}

		protected void SetButtonLocation(Button button, int column, int row, int columnSpan = 1)
		{
			Grid.SetColumn(button, column);
			Grid.SetRow(button, row);
	
			if (columnSpan > 1)
				Grid.SetColumnSpan(button, columnSpan);
		}

		protected void AddButtonsToGrid()
		{
			lowerButtonsGrid.Children.Add(strategyBtn);
			lowerButtonsGrid.Children.Add(longBtn);
			lowerButtonsGrid.Children.Add(shortBtn);
			lowerButtonsGrid.Children.Add(quickLongBtn);
			lowerButtonsGrid.Children.Add(quickShortBtn);
			lowerButtonsGrid.Children.Add(moveTSBtn);
			lowerButtonsGrid.Children.Add(moveTS50PctBtn);
			lowerButtonsGrid.Children.Add(moveToBEBtn);
			lowerButtonsGrid.Children.Add(closeBtn);
			lowerButtonsGrid.Children.Add(panicBtn);
		}

		protected void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			Button button = sender as Button;
	
			if (button == strategyBtn)
			{
				isStrategyEnabled = !isStrategyEnabled;
				if (isStrategyEnabled)
					DecorateEnabledButtons(strategyBtn, "\uD83D\uDD12 Manual ATM On");
				else
					DecoreDisabledButtons(strategyBtn, "\uD83D\uDD13 Manual ATM Off");
				Print($"Strategy: {isStrategyEnabled}");
				return;
			}

			if (button == longBtn)
			{
				isLongEnabled = !isLongEnabled;
				if (isLongEnabled)
					DecorateEnabledButtons(longBtn, "LONG");
				else
					DecoreDisabledButtons(longBtn, "LONG Off");
				Print($"Long Enabled: {isLongEnabled}");
				return;
			}
	
			if (button == shortBtn)
			{
				isShortEnabled = !isShortEnabled;
				if (isShortEnabled)
					DecorateEnabledButtons(shortBtn, "SHORT");
				else
					DecoreDisabledButtons(shortBtn, "SHORT Off");
				Print($"Short Enabled: {isShortEnabled}");
				return;
			}

			if (button == quickLongBtn && isStrategyEnabled && isLongEnabled && !longSignal && uptrend)
			{
				longSignal = true;
				QuickLong = !QuickLong;
				Print($"Buy Market On: {QuickLong}");
				quickLongBtnActive = true;
	
				CreateAtmStrategy(OrderAction.Buy, LongEntryLabel, Brushes.Cyan);
	
				QuickLong = false;
				runOnce = true;
				return;
			}

			if (button == quickShortBtn && isStrategyEnabled && isShortEnabled && !shortSignal && downtrend)
			{
				shortSignal = true;
				QuickShort = !QuickShort;
				Print($"Sell Market On: {QuickShort}");
				quickShortBtnActive = true;
	
				CreateAtmStrategy(OrderAction.SellShort, ShortEntryLabel, Brushes.Yellow);
	
				QuickShort = false;
				runOnce = true;
				return;
			}
	
			#region Move Trailing Stop Button
			if (button == moveTSBtn)
			{
				if (!string.IsNullOrEmpty(atmStrategyId))
				{
					MoveTrailingStop(TickMove);
					ForceRefresh();
				}
				else
					Print("Not moving target, invalid state of atmStrategyId");
				return;
			}
			#endregion

			#region Move Trailing Stop 50% Button
			if (button == moveTS50PctBtn)
			{
				if (!string.IsNullOrEmpty(atmStrategyId))
				{
					MoveTrailingStopPercent(0.5); // 50%
					ForceRefresh();
				}
				else
					Print("Not moving target, invalid state of atmStrategyId");
				return;
			}
			#endregion

			#region Move To Breakeven Button

			if (button == moveToBEBtn)
			{
				if (!string.IsNullOrEmpty(atmStrategyId))
				{
					MoveToBreakeven();
					ForceRefresh();
				}
				else
					Print("Not moving target, invalid state of atmStrategyId");
				return;
			}
			#endregion

			if (button == closeBtn) { CloseAllPositions(); ForceRefresh(); return; }
			if (button == panicBtn) { FlattenAllPositions(); ForceRefresh(); return; }
		}
		
		#region Move Trailing Stop Methods
		private void MoveTrailingStop(int tickMove)
		{
			if (string.IsNullOrEmpty(atmStrategyId))
			{
				Print("No ATM strategy active to move the stop.");
				return;
			}
	
			MarketPosition marketPosition = GetAtmStrategyMarketPosition(atmStrategyId);
	
			string[,] stopTargetInfo = GetAtmStrategyStopTargetOrderStatus("", atmStrategyId);
	
			if (stopTargetInfo == null || stopTargetInfo.GetLength(0) == 0)
			{
				Print("Could not retrieve stop target order status. Check ATM strategy configuration.");
				return;
			}
	
			if (marketPosition == MarketPosition.Long)
			{
				if (double.TryParse(stopTargetInfo[0, 0], out double currentStopPrice))
				{
					double newStopPrice = currentStopPrice + tickMove * TickSize;
					AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
					Print($"Moving Long Stop to {newStopPrice}");
				}
				else
				{
					Print("Could not parse long stop price from ATM strategy. Check ATM Strategy configuration.");
				}
			}
			else if (marketPosition == MarketPosition.Short)
			{
				if (double.TryParse(stopTargetInfo[0, 0], out double currentStopPrice))
				{
					double newStopPrice = currentStopPrice - tickMove * TickSize;
					AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
					Print($"Moving Short Stop to {newStopPrice}");
				}
				else
				{
					Print("Could not parse short stop price from ATM strategy. Check ATM Strategy configuration.");
				}
			}
			else
			{
				Print("No open position to move the stop.");
			}
		}

		private void MoveTrailingStopPercent(double percent)
		{
			if (string.IsNullOrEmpty(atmStrategyId))
			{
				Print("No ATM strategy active to move the stop.");
				return;
			}
	
			MarketPosition marketPosition = GetAtmStrategyMarketPosition(atmStrategyId);
	
			string[,] stopTargetInfo = GetAtmStrategyStopTargetOrderStatus("", atmStrategyId);
	
			if (stopTargetInfo == null || stopTargetInfo.GetLength(0) < 2) // Check if both stop and target order info is available.
			{
				Print("Could not retrieve stop target order status or target is missing. Check ATM strategy configuration or ensure multiple targets");
				return;
			}
			double currentStopPrice = 0;
			double profitTarget = 0;
			if (marketPosition == MarketPosition.Long)
			{
				if (double.TryParse(stopTargetInfo[0, 0], out currentStopPrice) && double.TryParse(stopTargetInfo[1, 0], out profitTarget))
				{
					double distanceToTarget = profitTarget;
					double moveAmount = percent * (distanceToTarget - currentStopPrice);
					double newStopPrice = currentStopPrice + moveAmount;
					AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
					Print($"Moving Long Stop by {percent * 100}% to {newStopPrice}");
				}
				else
				{
					Print("Could not parse long stop or target price from ATM strategy. Check ATM Strategy configuration.");
				}
			}
			else if (marketPosition == MarketPosition.Short)
			{
	
				if (double.TryParse(stopTargetInfo[0, 0], out currentStopPrice) && double.TryParse(stopTargetInfo[1, 0], out profitTarget))
				{
					double distanceToTarget = profitTarget;
					double moveAmount = percent * (currentStopPrice - distanceToTarget);
					double newStopPrice = currentStopPrice - moveAmount;
					AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
					Print($"Moving Short Stop by {percent * 100}% to {newStopPrice}");
				}
				else
				{
					Print("Could not parse short stop or target price from ATM strategy. Check ATM Strategy configuration.");
				}
			}
			else
			{
				Print("No open position to move the stop.");
			}
		}


		private void MoveToBreakeven()
		{
			if (string.IsNullOrEmpty(atmStrategyId))
			{
				Print("No ATM strategy active to move to breakeven.");
				return;
			}
	
			MarketPosition marketPosition = GetAtmStrategyMarketPosition(atmStrategyId);
			double entryPrice = GetAtmStrategyPositionAveragePrice(atmStrategyId);
	
			string[,] stopTargetInfo = GetAtmStrategyStopTargetOrderStatus("", atmStrategyId);
	
			if (stopTargetInfo == null || stopTargetInfo.GetLength(0) == 0)
			{
				Print("Could not retrieve stop target order status. Check ATM strategy configuration.");
				return;
			}

			if (double.TryParse(stopTargetInfo[0, 0], out double currentStopPrice))
			{
				if (marketPosition == MarketPosition.Long)
				{
					if (Close[0] > entryPrice + BreakevenOffset * TickSize)
					{
						double newStopPrice = entryPrice + BreakevenOffset * TickSize;
						AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
						Print($"Moving Long Stop to Breakeven + {BreakevenOffset} ticks: {newStopPrice}");
					}
					else
					{
						Print("Long position not profitable enough to move to breakeven.");
					}
				}
				else if (marketPosition == MarketPosition.Short)
				{
					if (Close[0] < entryPrice - BreakevenOffset * TickSize)
					{
						double newStopPrice = entryPrice - BreakevenOffset * TickSize;
						AtmStrategyChangeStopTarget(0, newStopPrice, GetAtmStrategyUniqueId(), atmStrategyId);
						Print($"Moving Short Stop to Breakeven - {BreakevenOffset} ticks: {newStopPrice}");
					}
					else
					{
						Print("Short position not profitable enough to move to breakeven.");
					}
				}
				else
				{
					Print("No open position to move to breakeven.");
				}
			}
			else
			{
				Print("Could not parse current stop price. Check ATM Strategy configuration.");
			}
		}

		//Helper method to retrieve stop price
		private double GetAtmStrategyStopPrice(string strategyId)
		{
			double stopPrice = 0;
			string[,] stopTargetInfo = GetAtmStrategyStopTargetOrderStatus("", atmStrategyId);
			if (stopTargetInfo != null && stopTargetInfo.GetLength(0) > 0)
			{
				if (double.TryParse(stopTargetInfo[0, 0], out stopPrice))
					return stopPrice;
			}

			return 0;
		}

		//Helper method to retrieve profit target
		private double GetAtmStrategyProfitTarget(string strategyId)
		{
			double profitTarget = 0;
			string[,] stopTargetInfo = GetAtmStrategyStopTargetOrderStatus("", atmStrategyId);
			if (stopTargetInfo != null && stopTargetInfo.GetLength(0) > 1)
			{
				if (double.TryParse(stopTargetInfo[1, 0], out profitTarget))
					return profitTarget;
			}
			return 0;
		}

		#endregion
		
		#region Dispose
		protected void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			strategyBtn.Click -= OnButtonClick;
			longBtn.Click -= OnButtonClick;
			shortBtn.Click -= OnButtonClick;
			quickLongBtn.Click -= OnButtonClick;
			quickShortBtn.Click -= OnButtonClick;
			moveTSBtn.Click -= OnButtonClick;
			moveTS50PctBtn.Click -= OnButtonClick;
			moveToBEBtn.Click -= OnButtonClick;
			closeBtn.Click -= OnButtonClick;
			panicBtn.Click -= OnButtonClick;

			RemoveWPFControls();
		}
		#endregion

		#region Insert WPF
		public void InsertWPFControls()
		{
			if (panelActive)
				return;

			chartTraderGrid.RowDefinitions.Add(addedRow);
			Grid.SetRow(lowerButtonsGrid, (chartTraderGrid.RowDefinitions.Count - 1));
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

		#region Tab Selected
		protected bool TabSelected()
		{
			foreach (TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					return true;

			return false;
		}

		protected void TabChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as ChartTab;
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
			if (!string.IsNullOrEmpty(atmStrategyId))
			{
				Print("Closing open position for ATM strategy.");
				AtmStrategyClose(atmStrategyId);
			}
			else
			{
				Print("No active ATM strategy to close.");
			}
		}

		protected void FlattenAllPositions()
		{
			Position openPosition = Position;
			Account myAccount = Account.All.FirstOrDefault(a => a.Name == chartTraderAccount.DisplayName);

			if (myAccount == null)
				throw new Exception("Account not found.");

			if (openPosition != null && openPosition.MarketPosition != MarketPosition.Flat)
			{
				List<Instrument> instrumentNames = new List<Instrument>();
				foreach (Position position in chartTraderAccount.Positions)
				{
					Instrument instrument = position.Instrument;
					if (!instrumentNames.Contains(instrument))
						instrumentNames.Add(instrument);
				}
				chartTraderAccount.Flatten((ICollection<Instrument>)instrumentNames);
			}
		}
		#endregion

		protected bool checkTimers()
		{
			if ((Times[0][0].TimeOfDay >= Start.TimeOfDay) && (Times[0][0].TimeOfDay < End.TimeOfDay)
					|| (isEnableTime2 && Times[0][0].TimeOfDay >= Start2.TimeOfDay && Times[0][0].TimeOfDay <= End2.TimeOfDay)
					|| (isEnableTime3 && Times[0][0].TimeOfDay >= Start3.TimeOfDay && Times[0][0].TimeOfDay <= End3.TimeOfDay)
					|| (isEnableTime4 && Times[0][0].TimeOfDay >= Start4.TimeOfDay && Times[0][0].TimeOfDay <= End4.TimeOfDay)
					|| (isEnableTime5 && Times[0][0].TimeOfDay >= Start5.TimeOfDay && Times[0][0].TimeOfDay <= End5.TimeOfDay)
					|| (isEnableTime6 && Times[0][0].TimeOfDay >= Start6.TimeOfDay && Times[0][0].TimeOfDay <= End6.TimeOfDay)
			)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		private string GetActiveTimer()
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

		protected void ShowPNLStatus()
		{
			string textLine1 = GetActiveTimer();
			string textLine3 = $"{counterLong} / {longPerDirection} | " + (tradesPerDirection ? "On" : "Off");
			string textLine5 = $"{counterShort} / {shortPerDirection} | " + (tradesPerDirection ? "On" : "Off");

			string statusPnlText = $"Active Timer:\t{textLine1}\nLong Per Direction:\t{textLine3}\nShort Per Direction:\t{textLine5}";
			SimpleFont font = new SimpleFont("Arial", 18);

			Draw.TextFixed(this, "statusPnl", statusPnlText, PositionPnl, colorPnl, font, Brushes.Transparent, Brushes.Transparent, 0);
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			if (showDailyPnl) DrawStrategyPnl(chartControl);
		}

		protected void DrawStrategyPnl(ChartControl chartControl)
		{	
			double realizedPnL = Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
			double unrealizedProfitLoss = Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
			cumProfit = syncPnl ? historicalTimeTrades + realizedPnL : realizedPnL + dif;
			double totalPnL = cumProfit + unrealizedProfitLoss;
//			string total = totalPnL.ToString("N0");

			// Track the Highest Profit Achieved
			if (totalPnL > maxProfit)
			{
				maxProfit = totalPnL;
			}
			
			string direction = hmaHooksUp && regChanUp? "Up" : "Down";
			string realTimeTradeText = $"{Account.Name} | {Account.Connection.Options.Name}\nRealized PnL:\t${realizedPnL:F2}\nUnrealized PnL:\t${unrealizedProfitLoss:F2}\nTotal PnL:\t${totalPnL:F2}\nMax Profit:\t${maxProfit:F2}\nTrend Direction:\t{direction}";
			SimpleFont font = new SimpleFont("Arial", 18);

			colorDailyProfitLoss = totalPnL == 0 ? Brushes.Cyan: totalPnL > 0 ? Brushes.Lime : Brushes.Red;
			
			Draw.TextFixed(this, "realTimeTradeText", realTimeTradeText, PositionDailyPNL, colorDailyProfitLoss, font, Brushes.Transparent, Brushes.Transparent, 0);
		}

		#region KillSwitch
		protected void KillSwitch()
		{
			totalPnL = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit;
			dailyPnL = totalPnL + Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);

			// Determine all relevant order labels
			List<string> longOrderLabels = new List<string> { LongEntryLabel }; // Base Labels for Longs
			List<string> shortOrderLabels = new List<string> { ShortEntryLabel }; // Base Labels for Shorts

		    // Common Action: Close all Positions and Disable the Strategy
		    Action closeAllPositionsAndDisableStrategy = () =>
		    {
		        foreach (string label in longOrderLabels)
		        {
		            ExitLong(Convert.ToInt32(Position.Quantity), @"LongExitKillSwitch", label);
		        }
		
		        foreach (string label in shortOrderLabels)
		        {
		            ExitShort(Convert.ToInt32(Position.Quantity), @"ShortExitKillSwitch", label);
		        }
		
		        isStrategyEnabled = false;
		        Print("Kill Switch Activated: Strategy Disabled!");
		    };

		    if (dailyLossProfit && enableTrailingDD) //Check both the enableDailyLossLimit and enableTrailingDD
		    {
		        if (totalPnL >= StartTrailingDD && (maxProfit - totalPnL) >= TrailingDrawdown && Position.Quantity > 0)
		        {
		            closeAllPositionsAndDisableStrategy();
		            trailingDrawdownReached = true;
					Print("Max drawdown has been reached!  No more trading for the day.");
		        }
		    }

			if (dailyLossProfit && enableTrailingDD) //Check both the enableDailyLossLimit and enableTrailingDD
			{
				if (totalPnL >= StartTrailingDD && (maxProfit - totalPnL) >= TrailingDrawdown)
				{
					closeAllPositionsAndDisableStrategy();
					trailingDrawdownReached = true;
				}
			}

			if (dailyPnL <= -DailyLossLimit)
			{
				closeAllPositionsAndDisableStrategy();
			}

			if (dailyPnL >= DailyProfitLimit)
			{
				closeAllPositionsAndDisableStrategy();
			}

			if (!isStrategyEnabled)
				Print("Kill Switch Activated!");
		}
		#endregion

		#region Custom Property Manipulation

		public void ModifyProperties(PropertyDescriptorCollection col)
		{
			if (!TradesPerDirection)
			{
				col.Remove(col.Find(nameof(longPerDirection), true));
				col.Remove(col.Find(nameof(shortPerDirection), true));
			}
			if (!isEnableTime2)
			{
				col.Remove(col.Find(nameof(Start2), true));
				col.Remove(col.Find(nameof(End2), true));
			}
			if (!isEnableTime3)
			{
				col.Remove(col.Find(nameof(Start3), true));
				col.Remove(col.Find(nameof(End3), true));
			}
			if (!isEnableTime4)
			{
				col.Remove(col.Find(nameof(Start4), true));
				col.Remove(col.Find(nameof(End4), true));
			}
			if (!isEnableTime5)
			{
				col.Remove(col.Find(nameof(Start5), true));
				col.Remove(col.Find(nameof(End5), true));
			}
			if (!isEnableTime6)
			{
				col.Remove(col.Find(nameof(Start6), true));
				col.Remove(col.Find(nameof(End6), true));
			}
		}
		#endregion

		#region ICustomTypeDescriptor Members

		public AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(GetType()); }
		public string GetClassName() { return TypeDescriptor.GetClassName(GetType()); }
		public string GetComponentName() { return TypeDescriptor.GetComponentName(GetType()); }
		public TypeConverter GetConverter() { return TypeDescriptor.GetConverter(GetType()); }
		public EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(GetType()); }
		public PropertyDescriptor GetDefaultProperty() { return TypeDescriptor.GetDefaultProperty(GetType()); }
		public object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(GetType(), editorBaseType); }
		public EventDescriptorCollection GetEvents(Attribute[] attributes) { return TypeDescriptor.GetEvents(GetType(), attributes); }
		public EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(GetType()); }
		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			PropertyDescriptorCollection orig = TypeDescriptor.GetProperties(GetType(), attributes);
			PropertyDescriptor[] arr = new PropertyDescriptor[orig.Count];
			orig.CopyTo(arr, 0);
			PropertyDescriptorCollection col = new PropertyDescriptorCollection(arr);

			ModifyProperties(col);

			return col;
		}
		public PropertyDescriptorCollection GetProperties() { return TypeDescriptor.GetProperties(GetType()); }
		public object GetPropertyOwner(PropertyDescriptor pd) { return this; }

		#endregion
		
		#region Properties - Release Notes
	
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Base Algo Version", Order = 1, GroupName = "01. Release Notes")]
		public string BaseAlgoVersion { get; set; }
	
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Author", Order = 2, GroupName = "01. Release Notes")]
		public string Author { get; set; }
	
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Strategy Name", Order = 3, GroupName = "01. Release Notes")]
		public string StrategyName { get; set; }
	
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Version", Order = 4, GroupName = "01. Release Notes")]
		public string Version { get; set; }
	
		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Display(Name = "Credits", Order = 5, GroupName = "01. Release Notes")]
		public string Credits { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Chart Type", Order = 6, GroupName = "01. Release Notes")]
		public string ChartType { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "ATM Strategy Template", Order = 7, GroupName = "01. Release Notes")]
		public string ATMStrategyTemplate { get; set; }
	
		#endregion

		#region Properties - Order Settings
	
		[NinjaScriptProperty]
		[Display(Name = "Order Type", Order = 1, GroupName = "02. Order Settings")]
		public OrderType OrderType { get; set; }
	
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Breakeven Offset", Order = 2, GroupName = "02. Order Settings")]
		public int BreakevenOffset { get; set; }
	
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Tick Move", Order = 3, GroupName = "02. Order Settings")]
		public int TickMove { get; set; }
	
		#endregion
	
		#region Properties - Profit/Loss Limit
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Daily Loss / Profit ", Description = "Enable / Disable Daily Loss & Profit control", Order = 1, GroupName = "05. Profit/Loss Limit	")]
		[RefreshProperties(RefreshProperties.All)]
		public bool dailyLossProfit
		{ get; set; }
	
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Daily Profit Limit ($)", Description = "No positive or negative sign, just integer", Order = 2, GroupName = "05. Profit/Loss Limit	")]
		public double DailyProfitLimit { get; set; }
	
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Daily Loss Limit ($)", Description = "No positive or negative sign, just integer", Order = 3, GroupName = "05. Profit/Loss Limit	")]
		public double DailyLossLimit { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Trailing Drawdown", Description = "Enable / Disable trailing drawdown", Order = 4, GroupName = "05. Profit/Loss Limit	")]
		[RefreshProperties(RefreshProperties.All)]
		public bool enableTrailingDD { get; set; }
	
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trailing Drawdown ($)", Description = "No positive or negative sign, just integer", Order = 5, GroupName = "05. Profit/Loss Limit	")]
		public double TrailingDrawdown { get; set; }
	
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Trailing Drawdown ($)", Description = "No positive or negative sign, just integer", Order = 6, GroupName = "05. Profit/Loss Limit	")]
		public double StartTrailingDD { get; set; }
	
		#endregion
	
		#region Properties - Trades Per Direction
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Trades Per Direction", Description = "Switch off Historical Trades to use this option.", Order = 1, GroupName = "06. Trades Per Direction")]
		[RefreshProperties(RefreshProperties.All)]
		public bool TradesPerDirection
		{
			get { return tradesPerDirection; }
			set { tradesPerDirection = (value); }
		}
	
		[NinjaScriptProperty]
		[Display(Name = "Long Per Direction", Description = "Number of long in a row", Order = 2, GroupName = "06. Trades Per Direction")]
		public int longPerDirection { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Short Per Direction", Description = "Number of short in a row", Order = 3, GroupName = "06. Trades Per Direction")]
		public int shortPerDirection { get; set; }
	
		#endregion
	
		#region Properties - Indicator Settings
	
		[NinjaScriptProperty]
		[Display(Name = "Enable VMA", Order = 1, GroupName = "08. Indicator Settings")]
		public bool enableVMA { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Show VMA", Order = 2, GroupName = "08. Indicator Settings")]
		public bool showVMA { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable HMA Hooks", Order = 3, GroupName = "08. Indicator Settings")]
		public bool enableHmaHooks { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Show HMA Hooks", Order = 4, GroupName = "08. Indicator Settings")]
		public bool showHmaHooks { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "HMA Period", Order = 5, GroupName = "08. Indicator Settings")]
		public int HmaPeriod { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Regression Channel", Order = 6, GroupName = "08. Indicator Settings")]
		public bool enableRegChan1 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Inner Regression Channel", Order = 7, GroupName = "08. Indicator Settings")]
		public bool enableRegChan2 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Show Outer Regression Channel", Order = 8, GroupName = "08. Indicator Settings")]
		public bool showRegChan1 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Show Inner Regression Channel", Order = 9, GroupName = "08. Indicator Settings")]
		public bool showRegChan2 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Show High and Low Lines", Order = 10, GroupName = "08. Indicator Settings")]
		public bool showRegChanHiLo { get; set; }

		[NinjaScriptProperty]
		[Display(Name="Regression Channel Period", Order = 11, GroupName="08. Indicator Settings")]
		public int RegChanPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Outer Regression Channel Width", Order = 12, GroupName="08. Indicator Settings")]
		public double RegChanWidth
		{ get; set; }
			
		[NinjaScriptProperty]
		[Display(Name = "Inner Regression Channel Width", Order = 13, GroupName = "08. Indicator Settings")]
		public double RegChanWidth2 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Momentum", Order = 14, GroupName = "08. Indicator Settings")]
		public bool enableMomo { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Show Momentum", Order = 15, GroupName = "08. Indicator Settings")]
		public bool showMomo { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Momentum Up", Order = 16, GroupName = "08. Indicator Settings")]
		public int MomoUp { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Momentum Down", Order = 17, GroupName = "08. Indicator Settings")]
		public int MomoDown { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Show ADX", Order = 18, GroupName = "08. Indicator Settings")]
		public bool showAdx { get; set; }
	
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "ADX Period", Order = 19, GroupName = "08. Indicator Settings")]
		public int adxPeriod { get; set; }
	
		#endregion

		#region Properties - Timeframes
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Start Trades", Order = 1, GroupName = "10. Timeframes")]
		public DateTime Start { get; set; }
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "End Trades", Order = 2, GroupName = "10. Timeframes")]
		public DateTime End { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 2", Description = "Enable 2 times.", Order = 3, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time2
		{
			get { return isEnableTime2; }
			set { isEnableTime2 = (value); }
		}
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Start Time 2", Order = 4, GroupName = "10. Timeframes")]
		public DateTime Start2 { get; set; }
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "End Time 2", Order = 5, GroupName = "10. Timeframes")]
		public DateTime End2 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 3", Description = "Enable 3 times.", Order = 6, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time3
		{
			get { return isEnableTime3; }
			set { isEnableTime3 = (value); }
		}
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Start Time 3", Order = 7, GroupName = "10. Timeframes")]
		public DateTime Start3 { get; set; }
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "End Time 3", Order = 8, GroupName = "10. Timeframes")]
		public DateTime End3 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 4", Description = "Enable 4 times.", Order = 9, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time4
		{
			get { return isEnableTime4; }
			set { isEnableTime4 = (value); }
		}
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Start Time 4", Order = 10, GroupName = "10. Timeframes")]
		public DateTime Start4 { get; set; }
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "End Time 4", Order = 11, GroupName = "10. Timeframes")]
		public DateTime End4 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 5", Description = "Enable 5 times.", Order = 12, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time5
		{
			get { return isEnableTime5; }
			set { isEnableTime5 = (value); }
		}
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Start Time 5", Order = 13, GroupName = "10. Timeframes")]
		public DateTime Start5 { get; set; }
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "End Time 5", Order = 14, GroupName = "10. Timeframes")]
		public DateTime End5 { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 6", Description = "Enable 6 times.", Order = 15, GroupName = "10. Timeframes")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time6
		{
			get { return isEnableTime6; }
			set { isEnableTime6 = (value); }
		}
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "Start Time 6", Order = 16, GroupName = "10. Timeframes")]
		public DateTime Start6 { get; set; }
	
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name = "End Time 6", Order = 17, GroupName = "10. Timeframes")]
		public DateTime End6 { get; set; }
	
		#endregion
	
		#region Properties - Status Panel
	
		[NinjaScriptProperty]
		[Display(Name = "Show Daily PnL", Order = 1, GroupName = "11. Status Panel")]
		public bool showDailyPnl { get; set; }
	
		[XmlIgnore()]
		[Display(Name = "Daily PnL Color", Order = 2, GroupName = "11. Status Panel")]
		public Brush colorDailyProfitLoss { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "Daily PnL Position", Description = "Daily PNL Alert Position", Order = 3, GroupName = "11. Status Panel")]
		public TextPosition PositionDailyPNL { get; set; }
	
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
		public Brush colorPnl { get; set; }
	
		[NinjaScriptProperty]
		[Display(Name = "STATUS PANEL Position", Description = "Status PNL Position", Order = 6, GroupName = "11. Status Panel")]
		public TextPosition PositionPnl { get; set; }
	
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
		
		[NinjaScriptProperty]
		[Display(Name="Discord webhooks", Description="One or more Discord webhooks, separated by comma.", GroupName="11. Webhook", Order = 2)]
		public string DiscordWebhooks
		{ get; set; }
		
		#endregion	
		
		#endregion
    }
}