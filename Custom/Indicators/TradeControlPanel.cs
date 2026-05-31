#region Using declarations

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class TradeControlPanel : Indicator
    {
        #region Variables - UI Components

        private Chart chartWindow;
        private Grid chartTraderGrid, lowerButtonsGrid;
        private RowDefinition addedRow;
        private Expander orderManagementExpander;
        private Grid buttonPanel;
        private ChartScale chartScale;
		private const double FixedButtonWidth   = 90.0;
private const double DoubleButtonWidth = 180.0;

        #endregion

        #region Variables - State Tracking

        private bool panelActive = false;
        private int totalGrids = 0;
        private bool isTabChangeSubscribed = false;
        private bool isChartPanelKeySubscribed = false;
        private bool isChartControlMouseSubscribed = false;

        #endregion

        #region Variables - Offset Control

        private QuantityUpDown offsetControl;
        private TextBlock offsetDisplay;
        private Button offsetUpBtn, offsetDownBtn;
        private TextBox offsetTextBox;

        #endregion

        #region Variables - Entry Buttons

        private Button buyChartButton, sellChartButton;
        private Button buyUpButton, sellDownButton, buyDownButton, sellUpButton;
        private Button buyMarketButton, sellMarketButton;
        private bool buyChartActive = false;
        private bool sellChartActive = false;

        #endregion

        #region Variables - Position Management Buttons

        private Button cancelAllButton, reverseButton, flattenButton;
        private Button exitOnReverseButton, bracketButton;

        #endregion

        #region Variables - Stop Loss and Breakeven Buttons

        private Button slButton1, slButton2, slButton3, slPercent50Button;
        private Button beButton1, beButton2, beButton3, beButton4;
		private Button tpButton1, tpButton2, tpButton3, tpButton4;
		//private int TP1Value = 2, TP2Value = 4, TP3Value = 6, TP4Value = 8;  // User-

        #endregion

        #region Variables - Colors

        private Brush BuyButtonColor = Brushes.MediumSeaGreen;
        private Brush SellButtonColor = Brushes.HotPink;
        private Brush NeutralButtonColor = Brushes.DodgerBlue;
		private Brush NeutralButtonColor2 = Brushes.HotPink;
        private Brush AccentColor = Brushes.MediumSeaGreen;

        #endregion

        #region Variables - Directional Order Tracking

        private bool buyUpActive = false;
        private bool sellDownActive = false;
        private bool buyDownActive = false;
        private bool sellUpActive = false;

        private int buyUpActivatedBar = -1;
        private int sellDownActivatedBar = -1;
        private int buyDownActivatedBar = -1;
        private int sellUpActivatedBar = -1;

        private Order buyUpOrder = null;
        private Order sellDownOrder = null;
        private Order buyDownOrder = null;
        private Order sellUpOrder = null;

        private int buyUpOrderBar = -1;
        private int sellDownOrderBar = -1;
        private int buyDownOrderBar = -1;
        private int sellUpOrderBar = -1;

        private bool sellDownShouldBeActive = false;
        private bool buyUpShouldBeActive = false;
        private bool buyDownShouldBeActive = false;
        private bool sellUpShouldBeActive = false;

        #endregion

        #region Variables - Bracket and Exit on Reverse

        private bool exitOnReverseActive = false;
        private bool bracketEnabled = false;
        private bool bracketActive = false;
        private int reversalCount = 0;
        private int lastReversalBar = -1;

        #endregion

        #region Variables - Account and Order Management

        private AccountSelector xAlselector;
        private Account monitoredAccount;
        private Account cachedAccount = null;
        private Account currentAccount = null;
        private Account reverseWorkingAccount = null;

        private bool orderActionInProgress = false;
        private bool reverseInProgress = false;
        private bool isReverseProcessing = false;
        private bool isProcessing = false;

        private Order closeOrder = null;
        private bool shouldBuyOnReverse = false;
        private bool shouldSellOnReverse = false;
		//private bool orderActionInProgress = false;  // Prevent multiple simultaneous actions

        #endregion

        #region Variables - Keyboard

        private Key buyKey, sellKey;

        #endregion

        #region State Management

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Comprehensive trading control panel for Chart Trader";
                Name = "TradeControlPanel";
                IsChartOnly = true;
                IsOverlay = true;
                Calculate = Calculate.OnBarClose;

                Offset = 0;
                BracketStopLossTicks = 40;
                BracketTakeProfitTicks = 40;

                SL1Value = 2;
                SL2Value = 4;
                SL3Value = 6;

                BE1Value = 0;
                BE2Value = 2;
                BE3Value = 4;
                BE4Value = -12;
				
				TP1Value = 2;
				TP2Value = 6;
				TP3Value = 12;
				TP4Value = -8;

                ExitReverseBars = 2;
                BuyChartKey = KeyDesired.LeftShift;
                SellChartKey = KeyDesired.LeftAlt;
            }
            else if (State == State.DataLoaded)
            {
                // Subscribe to account execution events for OCO
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                        var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
                        if (xAlselector?.SelectedAccount != null)
                        {
                            monitoredAccount = xAlselector.SelectedAccount;
                            monitoredAccount.ExecutionUpdate += OnAccountExecutionUpdate;
                            Print("OCO: Subscribed to account execution events");
                        }
                    });
                }
            }
            else if (State == State.Historical)
            {
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        CreateOrderManagementPanel();

                        try
                        {
                            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                            if (chartWindow != null)
                            {
                                var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
                                if (xAlselector?.SelectedAccount != null)
                                {
                                    // Unsubscribe from old account if exists
                                    if (monitoredAccount != null)
                                    {
                                        monitoredAccount.ExecutionUpdate -= OnAccountExecutionUpdate;
                                        Print("OCO: Unsubscribed from previous account");
                                    }

                                    monitoredAccount = xAlselector.SelectedAccount;
                                    cachedAccount = xAlselector.SelectedAccount;
                                    monitoredAccount.ExecutionUpdate += OnAccountExecutionUpdate;
                                    Print("OCO: Subscribed to account execution events");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Print($"Warning: Could not subscribe to account events: {ex.Message}");
                        }
                    });
                }
            }
            else if (State == State.Terminated)
			{
				Print("═══ State.Terminated: Starting cleanup ═══");
				
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						try
						{
							// Unsubscribe from events first
							if (monitoredAccount != null)
							{
								monitoredAccount.ExecutionUpdate -= OnAccountExecutionUpdate;
								Print("✓ Unsubscribed from ExecutionUpdate");
							}
							
							if (currentAccount != null)
							{
								currentAccount.OrderUpdate -= OnOrderUpdate;
								Print("✓ Unsubscribed from OrderUpdate");
							}
							
							if (chartWindow != null && chartWindow.MainTabControl != null)
							{
								chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
								Print("✓ Unsubscribed from TabChanged");
							}
							
							if (ChartControl != null)
							{
								ChartControl.MouseLeftButtonDown -= ChartControl_MouseLeftButtonDown;
								Print("✓ Unsubscribed from MouseClick");
							}
							
							// Remove UI panel
							RemoveWPFControls();
							
							// Clear all references
							orderManagementExpander = null;
							buttonPanel = null;
							reverseButton = null;
							buyChartButton = null;
							sellChartButton = null;
							buyUpButton = null;
							sellDownButton = null;
							buyDownButton = null;
							sellUpButton = null;
							buyMarketButton = null;
							sellMarketButton = null;
							flattenButton = null;
							cancelAllButton = null;
							bracketButton = null;
							exitOnReverseButton = null;
							
							monitoredAccount = null;
							currentAccount = null;
							cachedAccount = null;
							
							Print("═══ Panel cleanup COMPLETE ═══");
						}
						catch (Exception ex)
						{
							Print($"Error in Terminated cleanup: {ex.Message}");
						}
					});
				}
			}
        }

        #endregion

        #region Bar Update and Rendering

        protected override void OnBarUpdate()
        {
            if (CurrentBar % 50 == 0)
            {
                Print($"OnBarUpdate RUNNING: Bar {CurrentBar}, exitOnReverseActive={exitOnReverseActive}");
            }

            if (CurrentBar < 1)
                return;

            // Skip if no buttons are active
            if (!buyUpActive && !sellDownActive && !buyDownActive && !sellUpActive && !bracketActive && !exitOnReverseActive)
            {
                return;
            }

            try
            {
                bool isGreenBar = Close[0] > Open[0];
                bool isRedBar = Close[0] < Open[0];

                // Buy Up: wait for GREEN bar
                if (buyUpActive && isGreenBar)
                {
                    double orderPrice = Close[0] - (Offset * TickSize);
                    Print($"Buy Up TRIGGERED on GREEN bar @ {orderPrice}");
                    buyUpOrder = PlaceDirectionalOrder(OrderAction.Buy, orderPrice);
                    buyUpActive = false;

                    // OCO: Cancel the opposite waiting order
                    if (sellDownActive)
                    {
                        sellDownActive = false;
                        Print("OCO: Sell Down cancelled - Buy Up won the race");
                    }
                    UpdateButtonStates();
                }

                // Sell Down: wait for RED bar
                if (sellDownActive && isRedBar)
                {
                    double orderPrice = Close[0] + (Offset * TickSize);
                    Print($"Sell Down TRIGGERED on RED bar @ {orderPrice}");
                    sellDownOrder = PlaceDirectionalOrder(OrderAction.Sell, orderPrice);
                    sellDownActive = false;

                    // OCO: Cancel the opposite waiting order
                    if (buyUpActive)
                    {
                        buyUpActive = false;
                        Print("OCO: Buy Up cancelled - Sell Down won the race");
                    }
                    UpdateButtonStates();
                }

                // Buy Down: wait for RED bar
                if (buyDownActive && isRedBar)
                {
                    double orderPrice = Close[0] - (Offset * TickSize);
                    Print($"Buy Down TRIGGERED on RED bar @ {orderPrice}");
                    buyDownOrder = PlaceDirectionalOrder(OrderAction.Buy, orderPrice);
                    buyDownActive = false;

                    // OCO: Cancel the opposite waiting order
                    if (sellUpActive)
                    {
                        sellUpActive = false;
                        Print("OCO: Sell Up cancelled - Buy Down won the race");
                    }
                    UpdateButtonStates();
                }

                // Sell Up: wait for GREEN bar
                if (sellUpActive && isGreenBar)
                {
                    double orderPrice = Close[0] + (Offset * TickSize);
                    Print($"Sell Up TRIGGERED on GREEN bar @ {orderPrice}");
                    sellUpOrder = PlaceDirectionalOrder(OrderAction.Sell, orderPrice);
                    sellUpActive = false;

                    // OCO: Cancel the opposite waiting order
                    if (buyDownActive)
                    {
                        buyDownActive = false;
                        Print("OCO: Buy Down cancelled - Sell Up won the race");
                    }
                    UpdateButtonStates();
                }

                // Check for bracket orders
                if (bracketActive)
                {
                    AddMissingBracketOrders();
                }

                // Check for exit on reversal
                if (exitOnReverseActive)
                {
                    MarketPosition currentPosition = GetCurrentPositionFromAccount();

                    // DEBUG: Print position every 10 bars
                    if (CurrentBar % 10 == 0)
                    {
                        Print($"Exit on Reverse: Monitoring - Position={currentPosition}, Bar={CurrentBar}, LastReversal={lastReversalBar}");
                    }

                    if (currentPosition != MarketPosition.Flat)
                    {
                        bool reversalDetected = false;
                        bool isBearishReversal = IsBearishReversal();
                        bool isBullishReversal = IsBullishReversal();

                        // DEBUG: Print reversal checks for each position
                        if (currentPosition == MarketPosition.Long)
                        {
                            Print($"Long Position Check: Close[1]={Close[1]:F2}, Open[1]={Open[1]:F2}, Close[0]={Close[0]:F2}, Open[0]={Open[0]:F2}, IsBearishReversal={isBearishReversal}");
                        }
                        else if (currentPosition == MarketPosition.Short)
                        {
                            Print($"Short Position Check: Close[1]={Close[1]:F2}, Open[1]={Open[1]:F2}, Close[0]={Close[0]:F2}, Open[0]={Open[0]:F2}, IsBullishReversal={isBullishReversal}");
                        }

                        // Check for reversal based on position
                        if (currentPosition == MarketPosition.Long && isBearishReversal)
                        {
                            reversalDetected = true;
                            Print($"⚠️ Exit on Reverse: Bearish reversal detected - EXITING LONG");
                        }
                        else if (currentPosition == MarketPosition.Short && isBullishReversal)
                        {
                            reversalDetected = true;
                            Print($"⚠️ Exit on Reverse: Bullish reversal detected - EXITING SHORT");
                        }

                        if (reversalDetected && CurrentBar != lastReversalBar)
                        {
                            lastReversalBar = CurrentBar;
                            reversalCount++;
                            Print($"Exit on Reverse: Reversal #{reversalCount} at bar {CurrentBar}");

                            // Exit immediately on first reversal
                            if (reversalCount >= 1)
                            {
                                Print($"Exit on Reverse: EXECUTING EXIT NOW");
                                ExitPositionOnReversal();
                                reversalCount = 0;
                                exitOnReverseActive = false;

                                ChartControl.Dispatcher.InvokeAsync(() =>
                                {
                                    exitOnReverseButton.Background = Brushes.Orange;
                                    exitOnReverseButton.Content = "Exit Rev";
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Print($"CRITICAL ERROR in OnBarUpdate: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale cs)
        {
            base.OnRender(chartControl, cs);
            chartScale = cs;
        }

        #endregion
        
        #region Account Execution Update Handler

        private void OnAccountExecutionUpdate(object sender, ExecutionEventArgs e)
        {
            if (e.Execution == null || e.Execution.Order == null)
                return;
            if (e.Execution.Order.Instrument != Instrument)
                return;

            try
            {
                if (e.Execution.Order.OrderState == OrderState.Filled)
                {
                    Print($"═══════════════════════════════════════════");
                    Print($"OCO: Order filled - {e.Execution.Order.Name}");
                    Print($" Type: {e.Execution.Order.OrderType}");
                    Print($" Action: {e.Execution.Order.OrderAction}");
                    Print($" Quantity: {e.Execution.Order.Quantity}");
                    Print($" Price: {e.Execution.Price}");
                    Print($"═══════════════════════════════════════════");

                    // Check if this is a bracket order
                    bool isBracketOrder = (e.Execution.Order.Name == "Bracket SL" ||
                                         e.Execution.Order.Name == "Bracket TP");

                    if (isBracketOrder)
                    {
                        Print($"🎯 OCO: BRACKET ORDER DETECTED");
                        Print($" bracketActive flag: {bracketActive}");

                        if (bracketActive)
                        {
                            Print($"OCO: Cancelling opposite bracket order...");
                            List<Order> ordersToCancel = new List<Order>();

                            // Debug: List ALL orders
                            Print("═══ ALL ORDERS FOR THIS INSTRUMENT ═══");
                            foreach (Order order in monitoredAccount.Orders)
                            {
                                if (order.Instrument == Instrument)
                                {
                                    Print($" Order: {order.Name} | State: {order.OrderState} | Type: {order.OrderType} | Action: {order.OrderAction} | Qty: {order.Quantity}");

                                    if (order.OrderState != OrderState.Cancelled &&
                                        order.OrderState != OrderState.Filled &&
                                        order.OrderState != OrderState.Rejected &&
                                        (order.Name == "Bracket SL" || order.Name == "Bracket TP"))
                                    {
                                        ordersToCancel.Add(order);
                                        Print($" ✓ MARKED FOR CANCELLATION");
                                    }
                                }
                            }
                            Print("═══════════════════════════════════════");

                            if (ordersToCancel.Count > 0)
                            {
                                monitoredAccount.Cancel(ordersToCancel.ToArray());
                                Print($"✅ OCO: Cancelled {ordersToCancel.Count} bracket order(s)");
                            }
                            else
                            {
                                Print("⚠️ OCO: NO BRACKET ORDERS FOUND TO CANCEL!");
                            }

                            // Reset bracket
                            bracketActive = false;
                            ChartControl.Dispatcher.InvokeAsync(() =>
                            {
                                if (bracketButton != null)
                                {
                                    bracketButton.Content = "Bracket Off";
                                    bracketButton.Background = Brushes.Gray;
                                }
                            });
                        }
                        else
                        {
                            Print("⚠️ OCO: Bracket order filled but bracketActive is FALSE!");
                        }
                    }

                    // Check position
                    Position currentPos = null;
                    foreach (Position pos in monitoredAccount.Positions)
                    {
                        if (pos.Instrument == Instrument)
                            currentPos = pos;
                    }

                    if (currentPos != null)
                        Print($"📊 Position after fill: {currentPos.MarketPosition} {currentPos.Quantity} @ {currentPos.AveragePrice}");
                    else
                        Print("📊 Position after fill: FLAT");
                }
            }
            catch (Exception ex)
            {
                Print($"ERROR in OCO handler: {ex.Message}\n{ex.StackTrace}");
            }
        }

        #endregion

        #region Keyboard Input Handlers

        private void SetKeyBindings()
        {
            buyKey = KeyDesiredToKey(BuyChartKey);
            sellKey = KeyDesiredToKey(SellChartKey);
        }

        private Key KeyDesiredToKey(KeyDesired kd)
        {
            switch (kd)
            {
                case KeyDesired.LeftShift: return Key.LeftShift;
                case KeyDesired.RightShift: return Key.RightShift;
                case KeyDesired.LeftAlt: return Key.LeftAlt;
                case KeyDesired.RightAlt: return Key.RightAlt;
                default: return Key.None;
            }
        }

        private void ChartPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            return;
        }

        private void ChartPanel_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            return;
        }

        #endregion

	

        #region UI Panel Creation and Management
private void CreateOrderManagementPanel()
{
    try
    {
        Print("Creating TradeControlPanel...");

        chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
        if (chartWindow == null)
        {
            Print("ERROR: Chart window not found");
            return;
        }

        ChartTrader chartTrader = chartWindow.FindFirst("ChartWindowChartTraderControl") as ChartTrader;
        if (chartTrader == null)
        {
            Print("ERROR: Chart Trader not found");
            return;
        }

        chartTraderGrid = chartTrader.Content as Grid;
        if (chartTraderGrid == null)
        {
            Print("ERROR: Chart Trader grid not found");
            return;
        }

        // Compact button factory
       Button CreateButton(string content, Brush bgColor)
{
    return new Button
    {
        Content = content,
        Width = 90,
        Height = 24,
        Padding = new Thickness(2, 0, 2, 0),
        Margin = new Thickness(0),
        Foreground = Brushes.White,
        Background = bgColor,
        BorderBrush = Brushes.Black,
        BorderThickness = new Thickness(1.0),
        FontSize = 10.5,  // ADD THIS LINE - matches other buttons
        FontWeight = FontWeights.Normal
    };
}


        lowerButtonsGrid = new Grid();
        Grid.SetColumnSpan(lowerButtonsGrid, 1);
        lowerButtonsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        lowerButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        addedRow = new RowDefinition { Height = GridLength.Auto };

        StackPanel mainStackPanel = new StackPanel { Background = Brushes.DarkGray, Margin = new Thickness(0) };

        // ===== ENTRY ORDERS (EXACTLY 5 ROWS - NO GAP) =====
        Expander entryExpander = new Expander
        {
            IsExpanded = true,
            Background = Brushes.SlateGray,
            FontWeight = FontWeights.Bold,
            Header = "ENTRY ORDERS",
            Margin = new Thickness(0)
        };

        Grid entryGrid = new Grid { Background = Brushes.DarkGray };
        for (int i = 0; i < 5; i++) // EXACTLY 5 ROWS
            entryGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
        entryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        entryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        int entryRow = 0;

        // Row 0: Buy/Sell Chart
        buyChartButton = CreateButton("Buy Chart", BuyButtonColor);
        Grid.SetRow(buyChartButton, entryRow); Grid.SetColumn(buyChartButton, 0); entryGrid.Children.Add(buyChartButton);
        buyChartButton.Click += BuyChartClick;

        sellChartButton = CreateButton("Sell Chart", SellButtonColor);
        Grid.SetRow(sellChartButton, entryRow); Grid.SetColumn(sellChartButton, 1); entryGrid.Children.Add(sellChartButton);
        sellChartButton.Click += SellChartClick;
        entryRow++;

        // Row 1: Buy Up / Sell Down
        buyUpButton = CreateButton("Buy Up", BuyButtonColor);
        Grid.SetRow(buyUpButton, entryRow); Grid.SetColumn(buyUpButton, 0); entryGrid.Children.Add(buyUpButton);
        buyUpButton.Click += BuyUpClick;

        sellDownButton = CreateButton("Sell Down", SellButtonColor);
        Grid.SetRow(sellDownButton, entryRow); Grid.SetColumn(sellDownButton, 1); entryGrid.Children.Add(sellDownButton);
        sellDownButton.Click += SellDownClick;
        entryRow++;

        // Row 2: Buy Down / Sell Up
        buyDownButton = CreateButton("Buy Down", BuyButtonColor);
        Grid.SetRow(buyDownButton, entryRow); Grid.SetColumn(buyDownButton, 0); entryGrid.Children.Add(buyDownButton);
        buyDownButton.Click += BuyDownClick;

        sellUpButton = CreateButton("Sell Up", SellButtonColor);
        Grid.SetRow(sellUpButton, entryRow); Grid.SetColumn(sellUpButton, 1); entryGrid.Children.Add(sellUpButton);
        sellUpButton.Click += SellUpClick;
        entryRow++;

        // Row 3: Buy/Sell Market
        buyMarketButton = CreateButton("Buy Market", BuyButtonColor);
        Grid.SetRow(buyMarketButton, entryRow); Grid.SetColumn(buyMarketButton, 0); entryGrid.Children.Add(buyMarketButton);
        buyMarketButton.Click += BuyMarketClick;

        sellMarketButton = CreateButton("Sell Market", SellButtonColor);
        Grid.SetRow(sellMarketButton, entryRow); Grid.SetColumn(sellMarketButton, 1); entryGrid.Children.Add(sellMarketButton);
        sellMarketButton.Click += SellMarketClick;
        entryRow++;

        // Row 4: Offset (FINAL ROW)
        Grid offsetGrid = CreateOffsetControl();
        offsetGrid.Height = 24;
        offsetGrid.MaxHeight = 24;
        Grid.SetRow(offsetGrid, entryRow);
        Grid.SetColumnSpan(offsetGrid, 2);
        entryGrid.Children.Add(offsetGrid);

        entryExpander.Content = entryGrid;
        mainStackPanel.Children.Add(entryExpander);

        // ===== POSITION MANAGEMENT (EXACTLY 2 ROWS) =====
        Expander positionExpander = new Expander
        {
            IsExpanded = true,
            Background = Brushes.SlateGray,
            FontWeight = FontWeights.Bold,
            Header = "POSITION MANAGEMENT",
            Margin = new Thickness(0)
        };

        Grid positionGrid = new Grid { Background = Brushes.DarkGray };
        positionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
        positionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
        positionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        positionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        int posRow = 0;

        // Row 0
        reverseButton = CreateButton("Reverse", AccentColor);
        Grid.SetRow(reverseButton, posRow); Grid.SetColumn(reverseButton, 0); positionGrid.Children.Add(reverseButton);
        reverseButton.Click += OnReverseButtonClick;

        exitOnReverseButton = CreateButton("Exit Rev", AccentColor);
        Grid.SetRow(exitOnReverseButton, posRow); Grid.SetColumn(exitOnReverseButton, 1); positionGrid.Children.Add(exitOnReverseButton);
        exitOnReverseButton.Click += ExitOnReverseClick;
        posRow++;

        // Row 1
        cancelAllButton = CreateButton("Cancel All", AccentColor);
        Grid.SetRow(cancelAllButton, posRow); Grid.SetColumn(cancelAllButton, 0); positionGrid.Children.Add(cancelAllButton);
        cancelAllButton.Click += CancelAllClick;

        flattenButton = CreateButton("FLATTEN", Brushes.DarkRed);
        Grid.SetRow(flattenButton, posRow); Grid.SetColumn(flattenButton, 1); positionGrid.Children.Add(flattenButton);
        flattenButton.Click += FlattenClick;

        positionExpander.Content = positionGrid;
        mainStackPanel.Children.Add(positionExpander);

       
    	// ===== SL / TP / BE (10 ROWS - TP BEFORE BE) =====
Expander slTpBeExpander = new Expander
{
    IsExpanded = true,
    Background = Brushes.SlateGray,
    FontWeight = FontWeights.Bold,
    Header = "SL/TP/BE",
    Margin = new Thickness(0)
	
};

Grid slTpBeGrid = new Grid { Background = Brushes.DarkGray };
// 10 ROWS: SL(3) + TP(3) + BE(4)
for (int i = 0; i < 6; i++)
    slTpBeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
slTpBeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
slTpBeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

int row = 0;

// SL ROWS 0-2 (TOP)
slButton1 = CreateButton($"SL{SL1Value}", NeutralButtonColor);
Grid.SetRow(slButton1, row); Grid.SetColumn(slButton1, 0); slTpBeGrid.Children.Add(slButton1);
slButton1.Click += (s, e) => MoveSLByTicks(SL1Value);

slButton2 = CreateButton($"SL{SL2Value}", NeutralButtonColor);
Grid.SetRow(slButton2, row); Grid.SetColumn(slButton2, 1); slTpBeGrid.Children.Add(slButton2);
slButton2.Click += (s, e) => MoveSLByTicks(SL2Value);
row++;

slButton3 = CreateButton($"SL{SL3Value}", NeutralButtonColor);
Grid.SetRow(slButton3, row); Grid.SetColumn(slButton3, 0); slTpBeGrid.Children.Add(slButton3);
slButton3.Click += (s, e) => MoveSLByTicks(SL3Value);

slPercent50Button = CreateButton("SL 50%", NeutralButtonColor);
Grid.SetRow(slPercent50Button, row); Grid.SetColumn(slPercent50Button, 1); slTpBeGrid.Children.Add(slPercent50Button);
slPercent50Button.Click += MoveSLPercent50Click;
row++;

// TP ROWS 3-5 (MIDDLE - BEFORE BE)
tpButton1 = CreateButton($"TP{TP1Value}", Brushes.Orange);
Grid.SetRow(tpButton1, row); Grid.SetColumn(tpButton1, 0); slTpBeGrid.Children.Add(tpButton1);
tpButton1.Click += (s, e) => MoveTPByTicks(TP1Value);

tpButton2 = CreateButton($"TP{TP2Value}", Brushes.Orange);
Grid.SetRow(tpButton2, row); Grid.SetColumn(tpButton2, 1); slTpBeGrid.Children.Add(tpButton2);
tpButton2.Click += (s, e) => MoveTPByTicks(TP2Value);
row++;

tpButton3 = CreateButton($"TP{TP3Value}", Brushes.Orange);
Grid.SetRow(tpButton3, row); Grid.SetColumn(tpButton3, 0); slTpBeGrid.Children.Add(tpButton3);
tpButton3.Click += (s, e) => MoveTPByTicks(TP3Value);

tpButton4 = CreateButton($"TP{TP4Value}", Brushes.Orange);
Grid.SetRow(tpButton4, row); Grid.SetColumn(tpButton4, 1); slTpBeGrid.Children.Add(tpButton4);
tpButton4.Click += (s, e) => MoveTPByTicks(TP4Value);
row++;

// BE ROWS 6-9 (BOTTOM)
beButton1 = CreateButton($"BE{BE1Value}", NeutralButtonColor2);
Grid.SetRow(beButton1, row); Grid.SetColumn(beButton1, 0); slTpBeGrid.Children.Add(beButton1);
beButton1.Click += (s, e) => MoveToBreakeven(BE1Value);

beButton2 = CreateButton($"BE{BE2Value}", NeutralButtonColor2);
Grid.SetRow(beButton2, row); Grid.SetColumn(beButton2, 1); slTpBeGrid.Children.Add(beButton2);
beButton2.Click += (s, e) => MoveToBreakeven(BE2Value);
row++;

beButton3 = CreateButton($"BE{BE3Value}", NeutralButtonColor2);
Grid.SetRow(beButton3, row); Grid.SetColumn(beButton3, 0); slTpBeGrid.Children.Add(beButton3);
beButton3.Click += (s, e) => MoveToBreakeven(BE3Value);

beButton4 = CreateButton($"BE{BE4Value}", NeutralButtonColor2);
Grid.SetRow(beButton4, row); Grid.SetColumn(beButton4, 1); slTpBeGrid.Children.Add(beButton4);
beButton4.Click += (s, e) => MoveToBreakeven(BE4Value);

slTpBeExpander.Content = slTpBeGrid;
mainStackPanel.Children.Add(slTpBeExpander);


      // ===== BRACKET (ADD THIS BACK) =====
Expander bracketExpander = new Expander
{
    IsExpanded = true,
    Background = Brushes.SlateGray,
    FontWeight = FontWeights.Bold,
    Header = "BRACKET",
    Margin = new Thickness(0)
};

Grid bracketGrid = new Grid { Background = Brushes.DarkGray };
bracketGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
bracketGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

bracketButton = CreateButton("Bracket OFF", Brushes.Gray);
Grid.SetRow(bracketButton, 0);
Grid.SetColumn(bracketButton, 0);
bracketGrid.Children.Add(bracketButton);
bracketButton.Click += BracketClick;

bracketExpander.Content = bracketGrid;
mainStackPanel.Children.Add(bracketExpander);  // THIS LINE ADDS IT BACK


        // Add to container
        Grid.SetRow(mainStackPanel, 0);
        Grid.SetColumn(mainStackPanel, 0);
        lowerButtonsGrid.Children.Add(mainStackPanel);

        if (totalGrids == 0)
            totalGrids = chartTraderGrid.RowDefinitions.Count;

        if (chartWindow.MainTabControl != null)
            chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;

        if (TabSelected())
            InsertWPFControls();

        if (ChartControl != null)
        {
            ChartControl.MouseLeftButtonDown += ChartControl_MouseLeftButtonDown;
            isChartControlMouseSubscribed = true;
        }
		Print($"DEBUG: Added {mainStackPanel.Children.Count} items to mainStackPanel. BracketButton: {bracketButton != null}");


        Print("✓ TradeControlPanel created - NO GAPS");
    }
    catch (Exception ex)
    {
        Print("ERROR: " + ex.Message + "\n" + ex.StackTrace);
    }
}


        private bool TabSelected()
        {
            if (ChartControl == null || chartWindow == null || chartWindow.MainTabControl == null)
                return false;

            bool tabSelected = false;
            foreach (TabItem tab in chartWindow.MainTabControl.Items)
            {
                ChartTab ct = tab.Content as ChartTab;
                if (ct != null && ct.ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
                {
                    tabSelected = true;
                    break;
                }
            }
            return tabSelected;
        }

        private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (TabSelected())
                    InsertWPFControls();
                else
                    RemoveWPFControls();
            }
            catch { }
        }

        public void InsertWPFControls()
		{
			if (panelActive)
			{
				Print("Panel already active, skipping insert");
				return;
			}
			
			if (chartTraderGrid == null || lowerButtonsGrid == null)
			{
				Print("Cannot insert: grid references are null");
				return;
			}
			
			try
			{
				// Only add row if it doesn't already exist
				if (!chartTraderGrid.RowDefinitions.Contains(addedRow))
				{
					chartTraderGrid.RowDefinitions.Add(addedRow);
				}
				
				// Only add panel if it's not already in the grid
				if (!chartTraderGrid.Children.Contains(lowerButtonsGrid))
				{
					Grid.SetRow(lowerButtonsGrid, totalGrids);
					chartTraderGrid.Children.Add(lowerButtonsGrid);
				}
				
				panelActive = true;
				Print("Panel inserted successfully");
			}
			catch (Exception ex)
			{
				Print($"Error in InsertWPFControls: {ex.Message}");
			}
		}


        public void RemoveWPFControls()
		{
			if (!panelActive)
				return;
			
			try
			{
				Print("Removing WPF controls...");
				
				if (chartTraderGrid != null && lowerButtonsGrid != null)
				{
					// Remove the grid containing your panel
					if (chartTraderGrid.Children.Contains(lowerButtonsGrid))
					{
						chartTraderGrid.Children.Remove(lowerButtonsGrid);
						Print("✓ Removed lowerButtonsGrid from ChartTrader");
					}
					
					// Remove the row definition we added
					if (addedRow != null && chartTraderGrid.RowDefinitions.Contains(addedRow))
					{
						chartTraderGrid.RowDefinitions.Remove(addedRow);
						Print("✓ Removed row definition");
					}
				}
				
				panelActive = false;
				Print("✓ Panel marked inactive");
			}
			catch (Exception ex)
			{
				Print($"Error in RemoveWPFControls: {ex.Message}");
			}
		}



        private void RemoveOrderManagementPanel()
        {
            try
            {
                offsetControl = null;
                RemoveWPFControls();
                offsetTextBox = null;
            }
            catch { }
        }

        private Grid CreateOffsetControl()
        {
            Grid offsetGrid = new Grid();
            offsetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            offsetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock offsetLabel = new TextBlock 
            { 
                Text = "Offset:", 
                VerticalAlignment = VerticalAlignment.Center, 
                Margin = new Thickness(5), 
                Foreground = Brushes.Black, 
                FontSize = 12, 
                FontWeight = FontWeights.Bold 
            };
            Grid.SetColumn(offsetLabel, 0);
            offsetGrid.Children.Add(offsetLabel);

            offsetControl = new QuantityUpDown();
            offsetControl.Value = Offset;
            offsetControl.Minimum = 0;
            offsetControl.Maximum = 100;
            offsetControl.Margin = new Thickness(5);
			
			offsetControl.ValueChanged += (s, e) => 
			{
			    Offset = (int)offsetControl.Value;  // CAST TO INT
			    Print($"Offset updated to {Offset}");
			};


            Grid.SetColumn(offsetControl, 1);
            offsetGrid.Children.Add(offsetControl);

            return offsetGrid;
        }

        private void AddSectionLabel(string text, int row)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                Margin = new Thickness(5, 8, 0, 3),
                FontSize = 11
            };
            Grid.SetRow(label, row);
            Grid.SetColumnSpan(label, 2);
            buttonPanel.Children.Add(label);
        }

        private Button CreateButton(string content, Brush background)
        {
            return new Button
            {
                Content = content,
                Background = background,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Height = 28,
                Margin = new Thickness(2),
                Padding = new Thickness(2),
                FontSize = 11
            };
        }

        #endregion
        
        #region Entry Order Button Click Handlers

        private void BuyChartClick(object sender, RoutedEventArgs e)
        {
            buyChartActive = !buyChartActive;
            buyChartButton.Background = buyChartActive ? Brushes.Yellow : BuyButtonColor;
            buyChartButton.Content = buyChartActive ? "Buy Chart (WAITING)" : "Buy Chart";
            Print($"Buy Chart: {(buyChartActive ? "ACTIVE - click on chart" : "INACTIVE")}");
        }

        private void SellChartClick(object sender, RoutedEventArgs e)
        {
            sellChartActive = !sellChartActive;
            sellChartButton.Background = sellChartActive ? Brushes.Yellow : SellButtonColor;
            sellChartButton.Content = sellChartActive ? "Sell Chart (WAITING)" : "Sell Chart";
            Print($"Sell Chart: {(sellChartActive ? "ACTIVE - click on chart" : "INACTIVE")}");
        }

        private void ChartControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!buyChartActive && !sellChartActive)
                return;

            try
            {
                int Y = ChartingExtensions.ConvertToVerticalPixels(e.GetPosition(ChartControl as IInputElement).Y, ChartControl.PresentationSource);
                if (chartScale == null)
                {
                    Print("Chart scale not ready");
                    return;
                }

                double priceClicked = chartScale.GetValueByY(Y);
                var window = Window.GetWindow(ChartControl.Parent) as Chart;
                xAlselector = window.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
                var quantitySelector = window.FindFirst("ChartTraderControlQuantitySelector") as NinjaTrader.Gui.Tools.QuantityUpDown;
                var atmSelector = window.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;

                if (xAlselector?.SelectedAccount == null)
                {
                    Print("No account selected");
                    return;
                }

                int quantity = quantitySelector?.Value ?? 1;
                Account account = xAlselector.SelectedAccount;
                OrderAction action;
                OrderType orderType;
                double limitPrice = 0;
                double stopPrice = 0;

                if (buyChartActive)
                {
                    action = OrderAction.Buy;
                    double currentAsk = GetCurrentAsk();
                    Print($"DEBUG Buy: Clicked={priceClicked}, Ask={currentAsk}, Below={priceClicked < currentAsk}");

                    if (priceClicked < currentAsk)
                    {
                        orderType = OrderType.Limit;
                        limitPrice = priceClicked;
                        stopPrice = 0;
                    }
                    else
                    {
                        orderType = OrderType.StopMarket;
                        limitPrice = 0;
                        stopPrice = priceClicked;
                    }
                }
                else // sellChartActive
                {
                    action = OrderAction.Sell;
                    double currentBid = GetCurrentBid();
                    Print($"DEBUG Sell: Clicked={priceClicked}, Bid={currentBid}, Above={priceClicked > currentBid}");

                    if (priceClicked > currentBid)
                    {
                        orderType = OrderType.Limit;
                        limitPrice = priceClicked;
                        stopPrice = 0;
                    }
                    else
                    {
                        orderType = OrderType.StopMarket;
                        limitPrice = 0;
                        stopPrice = priceClicked;
                    }
                }

                TriggerCustomEvent(o =>
                {
                    Order order = account.CreateOrder(Instrument, action, orderType, OrderEntry.Manual, TimeInForce.Day, quantity, limitPrice, stopPrice, "", "Entry", Core.Globals.MaxDate, null);

                    if (atmSelector?.SelectedAtmStrategy != null)
                    {
                        NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmSelector.SelectedAtmStrategy, order);
                    }

                    account.Submit(new[] { order });
                }, null);

                if (buyChartActive)
                {
                    buyChartActive = false;
                    buyChartButton.Background = BuyButtonColor;
                    buyChartButton.Content = "Buy Chart";
                    Print($"{orderType} Buy order placed @ limit:{limitPrice} / stop:{stopPrice} for {quantity} contracts");
                }

                if (sellChartActive)
                {
                    sellChartActive = false;
                    sellChartButton.Background = SellButtonColor;
                    sellChartButton.Content = "Sell Chart";
                    Print($"{orderType} Sell order placed @ limit:{limitPrice} / stop:{stopPrice} for {quantity} contracts");
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Print($"Error placing order: {ex.Message}");
                buyChartActive = false;
                sellChartActive = false;
            }
        }
                

private Account GetSelectedAccount()
{
    var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
    var accountSelector = chartWindow?.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
    return accountSelector?.SelectedAccount;
}

private NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector GetAtmStrategySelector()
{
    var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
    return chartWindow?.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;
}


//Here is the original version and this works
private void BuyMarketClick(object sender, RoutedEventArgs e)
{
    ChartControl.Dispatcher.InvokeAsync(() =>
    {
        try
        {
            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
            var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
            var quantitySelector = chartWindow.FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown;
            var atmSelector = chartWindow.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;

            if (xAlselector?.SelectedAccount == null)
                return;

            Account account = xAlselector.SelectedAccount;
            int quantity = quantitySelector?.Value ?? 1;
            
            double askPrice = GetCurrentAsk();
            int offset = Offset;  // Use the Offset property directly
            double orderPrice = askPrice - (offset * Instrument.MasterInstrument.TickSize);

            Order buyOrder = account.CreateOrder(Instrument, OrderAction.Buy, OrderType.Limit, 
                OrderEntry.Manual, TimeInForce.Day, quantity, orderPrice, 0, 
                "", "Entry", Core.Globals.MaxDate, null);

            // Attach ATM BEFORE submitting
            if (atmSelector?.SelectedAtmStrategy != null)
            {
                var atmId = NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(
                    atmSelector.SelectedAtmStrategy, buyOrder);
                Print($"✅ Buy Market with ATM: {atmSelector.SelectedAtmStrategy.Name}");
				 Print($"Buy Limit @ {orderPrice} (ask={askPrice}, offset={offset}) - NO ATMeeeeeee");
            }
            else
            {
                Print($"Buy Limit @ {orderPrice} (ask={askPrice}, offset={offset}) - NO ATM");
            }

            account.Submit(new[] { buyOrder });
        }
        catch (Exception ex)
        {
            Print($"Error in BuyMarketClick: {ex.Message}");
        }
    });
}
private void SellMarketClick(object sender, RoutedEventArgs e)
{
    ChartControl.Dispatcher.InvokeAsync(() =>
    {
        try
        {
            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
            var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
            var quantitySelector = chartWindow.FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown;
            var atmSelector = chartWindow.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;

            if (xAlselector?.SelectedAccount == null)
            {
                Print("Sell Market Error: No account selected");
                return;
            }

            Account account = xAlselector.SelectedAccount;
            int quantity = quantitySelector?.Value ?? 1;
            
            double bidPrice = GetCurrentBid();
            int offset = Offset;  // Use the Offset property directly
            double orderPrice = bidPrice + (offset * Instrument.MasterInstrument.TickSize);

            Order sellOrder = account.CreateOrder(Instrument, OrderAction.Sell, OrderType.Limit, 
                OrderEntry.Manual, TimeInForce.Day, quantity, orderPrice, 0, 
                "", "Entry", Core.Globals.MaxDate, null);

            // Attach ATM BEFORE submitting
            if (atmSelector?.SelectedAtmStrategy != null)
            {
                var atmId = NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(
                    atmSelector.SelectedAtmStrategy, sellOrder);
                Print($"✅ Sell Marketttt with ATM: {atmSelector.SelectedAtmStrategy.Name}");
				//Print($"✅ Buy Market with ATM: {atmSelector.SelectedAtmStrategy.Name}");
				 Print($"Buy Limit @ {orderPrice} (ask={bidPrice}, offset={offset}) - NO ATMeeeeeee");
            
            }
            else
            {
                Print($"Sell Limit @ {orderPrice} (bid={bidPrice}, offset={offset}) - NO ATM");
            }

            account.Submit(new[] { sellOrder });
        }
        catch (Exception ex)
        {
            Print($"Error in SellMarketClick: {ex.Message}");
        }
    });
}

//private void BuyMarketClick(object sender, RoutedEventArgs e)
//{
//    ChartControl.Dispatcher.InvokeAsync(() =>
//    {
//        try
//        {
//            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
//            var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
//            var quantitySelector = chartWindow.FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown;
//            var atmSelector = chartWindow.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;

//            if (xAlselector?.SelectedAccount == null)
//                return;

//            Account account = xAlselector.SelectedAccount;
//            int quantity = quantitySelector?.Value ?? 1;
//            if (quantity <= 0) quantity = 1;

//            double askPrice = GetCurrentAsk();
//            int offset = Offset;
//            double orderPrice = askPrice - (offset * Instrument.MasterInstrument.TickSize);

//            Order buyOrder = account.CreateOrder(
//                Instrument,
//                OrderAction.Buy,
//                OrderType.Limit,
//                OrderEntry.Manual,
//                TimeInForce.Day,
//                quantity,
//                orderPrice,
//                0,
//                "", "Entry", Core.Globals.MaxDate, null);

//            if (buyOrder != null)
//            {
//                var atmStrategy = atmSelector?.SelectedAtmStrategy;

//                if (atmStrategy != null)
//                {
//                    var atmId = NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, buyOrder);
//                    Print($"✅ Buy Market (Base) with ATM: {atmStrategy.Name} @ {orderPrice}");
//                }
//                else
//                {
//                    Print($"Buy Limit @ {orderPrice} (ask={askPrice}, offset={offset}) - NO ATM");
//                }
//                account.Submit(new[] { buyOrder });
//            }
//        }
//        catch (Exception ex)
//        {
//            Print($"Error in BuyMarketClick_Base: {ex.Message}");
//        }
//    });
//}


//private void SellMarketClick(object sender, RoutedEventArgs e)
//{
//    ChartControl.Dispatcher.InvokeAsync(() =>
//    {
//        try
//        {
//            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
//            var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
//            var quantitySelector = chartWindow.FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown;
//            var atmSelector = chartWindow.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;

//            if (xAlselector?.SelectedAccount == null)
//            {
//                Print("Sell Market (Base) Error: No account selected");
//                return;
//            }

//            Account account = xAlselector.SelectedAccount;
//            int quantity = quantitySelector?.Value ?? 1;
//            if (quantity <= 0) quantity = 1;

//            double bidPrice = GetCurrentBid();
//            int offset = Offset;
//            double orderPrice = bidPrice + (offset * Instrument.MasterInstrument.TickSize);

//            Order sellOrder = account.CreateOrder(
//                Instrument,
//                OrderAction.Sell,
//                OrderType.Limit,
//                OrderEntry.Manual,
//                TimeInForce.Day,
//                quantity,
//                orderPrice,
//                0,
//                "", "Entry", Core.Globals.MaxDate, null);

//            if (sellOrder != null)
//            {
//                var atmStrategy = atmSelector?.SelectedAtmStrategy;

//                if (atmStrategy != null)
//                {
//                    var atmId = NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, sellOrder);
//                    Print($"✅ Sell Market (Base) with ATM: {atmStrategy.Name} @ {orderPrice}");
//                }
//                else
//                {
//                    Print($"Sell Limit @ {orderPrice} (bid={bidPrice}, offset={offset}) - NO ATM");
//                }
//                account.Submit(new[] { sellOrder });
//            }
//        }
//        catch (Exception ex)
//        {
//            Print($"Error in SellMarketClick_Base: {ex.Message}");
//        }
//    });
//}














        private void BuyUpClick(object sender, RoutedEventArgs e)
        {
            buyUpActive = !buyUpActive;
            if (buyUpActive)
            {
                Print($"Buy Up activated - Waiting for GREEN bar (Close > Open)");
            }
            UpdateButtonStates();
        }

        private void SellDownClick(object sender, RoutedEventArgs e)
        {
            sellDownActive = !sellDownActive;
            if (sellDownActive)
            {
                Print($"Sell Down activated - Waiting for RED bar (Close < Open)");
            }
            UpdateButtonStates();
        }

        private void BuyDownClick(object sender, RoutedEventArgs e)
        {
            buyDownActive = !buyDownActive;
            if (buyDownActive)
            {
                Print($"Buy Down activated - Waiting for RED bar (Close < Open)");
            }
            UpdateButtonStates();
        }

        private void SellUpClick(object sender, RoutedEventArgs e)
        {
            sellUpActive = !sellUpActive;
            if (sellUpActive)
            {
                Print($"Sell Up activated - Waiting for GREEN bar (Close > Open)");
            }
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            if (ChartControl == null)
                return;

            ChartControl.Dispatcher.InvokeAsync(() =>
            {
                // Buy Up Button
                if (buyUpActive)
                {
                    buyUpButton.Content = "Buy Up ●";
                    buyUpButton.Opacity = 0.4;
                }
                else
                {
                    buyUpButton.Content = "Buy Up";
                    buyUpButton.Opacity = 1.0;
                }

                // Sell Down Button
                if (sellDownActive)
                {
                    sellDownButton.Content = "Sell Down ●";
                    sellDownButton.Opacity = 0.4;
                }
                else
                {
                    sellDownButton.Content = "Sell Down";
                    sellDownButton.Opacity = 1.0;
                }

                // Buy Down Button
                if (buyDownActive)
                {
                    buyDownButton.Content = "Buy Down ●";
                    buyDownButton.Opacity = 0.4;
                }
                else
                {
                    buyDownButton.Content = "Buy Down";
                    buyDownButton.Opacity = 1.0;
                }

                // Sell Up Button
                if (sellUpActive)
                {
                    sellUpButton.Content = "Sell Up ●";
                    sellUpButton.Opacity = 0.4;
                }
                else
                {
                    sellUpButton.Content = "Sell Up";
                    sellUpButton.Opacity = 1.0;
                }
            });
        }

        #endregion

        #region Position Management Button Click Handlers

        private void FlattenClick(object sender, RoutedEventArgs e)
        {
            if (orderActionInProgress)
            {
                Print("⚠️ Flatten: Another action in progress, please wait...");
                return;
            }

            orderActionInProgress = true;
            ChartControl.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                    var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;

                    if (xAlselector?.SelectedAccount == null)
                    {
                        Print("Flatten Error: No account selected");
                        orderActionInProgress = false;
                        return;
                    }

                    Account account = xAlselector.SelectedAccount;
                    Print("Flatten: Starting...");

                    // Step 1: Cancel all pending orders for this instrument
                    List<Order> ordersToCancel = new List<Order>();
                    foreach (Order order in account.Orders)
                    {
                        if (order.Instrument == Instrument &&
                            order.OrderState != OrderState.Cancelled &&
                            order.OrderState != OrderState.Filled &&
                            order.OrderState != OrderState.Rejected)
                        {
                            ordersToCancel.Add(order);
                        }
                    }

                    if (ordersToCancel.Count > 0)
                    {
                        account.Cancel(ordersToCancel.ToArray());
                        Print($"Flatten: Cancelled {ordersToCancel.Count} orders");
                    }

                    // Step 2: Use NT's built-in Flatten method
                    account.Flatten(new[] { Instrument });
                    Print("✅ Flatten: Position closed");

                    // Clear directional order references
                    buyUpOrder = null;
                    sellDownOrder = null;
                    buyDownOrder = null;
                    sellUpOrder = null;

                    // Reset directional flags
                    buyUpActive = false;
                    sellDownActive = false;
                    buyDownActive = false;
                    sellUpActive = false;

                    UpdateButtonStates();

                    // Reset flag after 1 second
                    System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
                    {
                        orderActionInProgress = false;
                    });
                }
                catch (Exception ex)
                {
                    Print($"Flatten Error: {ex.Message}");
                    orderActionInProgress = false;
                }
            });
        }

        private void BracketClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
                if (xAlselector == null) return;

                string currentAccountStr = xAlselector.SelectedAccount.ToString();
                Account Acct = Account.All.FirstOrDefault(x => currentAccountStr.Contains(x.Name));
                if (Acct == null) return;

                var xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
                if (xInSelector == null) return;

                string currentInstrument = xInSelector.Instrument.ToString();
                Position thisPosition = Acct.Positions.FirstOrDefault(x => currentInstrument.Contains(x.Instrument.FullName));

                if (thisPosition != null && thisPosition.MarketPosition != MarketPosition.Flat)
                {
                    // MODE 2: Already in position - immediately add missing orders
                    Print("Bracket: Checking for missing orders...");
                    AddMissingBracketOrders();
                }
                else
                {
                    // MODE 1: No position - toggle flag for next position
                    bracketActive = !bracketActive;

                    if (bracketActive)
                    {
                        bracketButton.Background = Brushes.Yellow;
                        bracketButton.Content = "Bracket ●";
                        Print("Bracket: ACTIVE - Will add missing SL/TP on next position");
                    }
                    else
                    {
                        bracketButton.Background = Brushes.Orange;
                        bracketButton.Content = "Bracket";
                        Print("Bracket: Deactivated");
                    }
                }
            }
            catch (Exception ex)
            {
                Print($"Error in BracketClick: {ex.Message}");
            }
        }

        private void ExitOnReverseClick(object sender, RoutedEventArgs e)
        {
            exitOnReverseActive = !exitOnReverseActive;

            if (exitOnReverseActive)
            {
                exitOnReverseButton.Background = Brushes.Yellow;
                exitOnReverseButton.Content = "Exit Rev ●";
                Print("Exit on Reverse: ACTIVE - Will exit on reversal candle");
                reversalCount = 0;
            }
            else
            {
                exitOnReverseButton.Background = Brushes.Orange;
                exitOnReverseButton.Content = "Exit Rev";
                Print("Exit on Reverse: Deactivated");
                reversalCount = 0;
            }
        }

        private void CancelAllClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cachedAccount == null)
                {
                    Print("Cancel All Error: No account cached");
                    return;
                }

                Print("Cancel All: Starting...");
                List<Order> ordersToCancel = new List<Order>();

                // Get all working orders for this instrument
                lock (cachedAccount.Orders)
                {
                    foreach (Order order in cachedAccount.Orders)
                    {
                        if (order.Instrument == Instrument &&
                            order.OrderState != OrderState.Cancelled &&
                            order.OrderState != OrderState.Filled &&
                            order.OrderState != OrderState.Rejected)
                        {
                            ordersToCancel.Add(order);
                        }
                    }
                }

                if (ordersToCancel.Count > 0)
                {
                    cachedAccount.Cancel(ordersToCancel.ToArray());
                    Print($"✅ Cancel All: Cancelled {ordersToCancel.Count} order(s)");

                    // Reset directional order references
                    buyUpOrder = null;
                    sellDownOrder = null;
                    buyDownOrder = null;
                    sellUpOrder = null;

                    // Reset directional flags
                    buyUpActive = false;
                    sellDownActive = false;
                    buyDownActive = false;
                    sellUpActive = false;

                    UpdateButtonStates();
                }
                else
                {
                    Print("Cancel All: No pending orders to cancel");
                }
            }
            catch (Exception ex)
            {
                Print($"Cancel All Error: {ex.Message}");
            }
        }

        #endregion
        
        #region Stop Loss Management

        private void MoveSLByTicks(int ticks)
        {
            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
            var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
            if (xAlselector == null) return;

            string currentAccountStr = xAlselector.SelectedAccount.ToString();
            Account Acct = Account.All.FirstOrDefault(x => currentAccountStr.Contains(x.Name));
            if (Acct == null) return;

            var xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
            if (xInSelector == null) return;

            string currentInstrument = xInSelector.Instrument.ToString();
            Position thisPosition = Acct.Positions.FirstOrDefault(x => currentInstrument.Contains(x.Instrument.FullName));

            if (thisPosition == null || thisPosition.MarketPosition == MarketPosition.Flat)
                return;

            foreach (Order order in Acct.Orders)
            {
                if (thisPosition.Account == order.Account && thisPosition.Instrument == order.Instrument)
                {
                    if (order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit)
                    {
                        if (order.OrderState != OrderState.Cancelled && order.OrderState != OrderState.Filled)
                        {
                            Order stopOrder = order;

                            if (thisPosition.MarketPosition == MarketPosition.Short)
                            {
                                stopOrder.StopPriceChanged = order.StopPrice - ticks * order.Instrument.MasterInstrument.TickSize;
                                Acct.Change(new[] { stopOrder });
                            }
                            else if (thisPosition.MarketPosition == MarketPosition.Long)
                            {
                                stopOrder.StopPriceChanged = order.StopPrice + ticks * order.Instrument.MasterInstrument.TickSize;
                                Acct.Change(new[] { stopOrder });
                            }
                        }
                    }
                }
            }
        }

        private void MoveSLPercent50Click(object sender, RoutedEventArgs e)
        {
            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
            var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
            if (xAlselector == null) return;

            string currentAccountStr = xAlselector.SelectedAccount.ToString();
            Account Acct = Account.All.FirstOrDefault(x => currentAccountStr.Contains(x.Name));
            if (Acct == null) return;

            var xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
            if (xInSelector == null) return;

            string currentInstrument = xInSelector.Instrument.ToString();
            Position thisPosition = Acct.Positions.FirstOrDefault(x => currentInstrument.Contains(x.Instrument.FullName));

            if (thisPosition == null || thisPosition.MarketPosition == MarketPosition.Flat)
                return;

            foreach (Order order in Acct.Orders)
            {
                if (thisPosition.Account == order.Account && thisPosition.Instrument == order.Instrument)
                {
                    if (order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit)
                    {
                        if (order.OrderState != OrderState.Cancelled && order.OrderState != OrderState.Filled)
                        {
                            Order stopOrder = order;
                            double currentStopPrice = order.StopPrice;
                            double percentMove = 50 / 100.0;

                            if (thisPosition.MarketPosition == MarketPosition.Short)
                            {
                                double currentAsk = GetCurrentAsk();
                                double distance = currentStopPrice - currentAsk;

                                if (distance > 0)
                                {
                                    double moveAmount = distance * percentMove;
                                    double newStopPrice = currentStopPrice - moveAmount;
                                    double minStopPrice = currentAsk + order.Instrument.MasterInstrument.TickSize;

                                    if (newStopPrice < minStopPrice)
                                        newStopPrice = minStopPrice;

                                    stopOrder.StopPriceChanged = newStopPrice;
                                    Acct.Change(new[] { stopOrder });
                                }
                            }
                            else if (thisPosition.MarketPosition == MarketPosition.Long)
                            {
                                double currentBid = GetCurrentBid();
                                double distance = currentBid - currentStopPrice;

                                if (distance > 0)
                                {
                                    double moveAmount = distance * percentMove;
                                    double newStopPrice = currentStopPrice + moveAmount;
                                    double maxStopPrice = currentBid - order.Instrument.MasterInstrument.TickSize;

                                    if (newStopPrice > maxStopPrice)
                                        newStopPrice = maxStopPrice;

                                    stopOrder.StopPriceChanged = newStopPrice;
                                    Acct.Change(new[] { stopOrder });
                                }
                            }
                        }
                    }
                }
            }
        }
		
	// ============================================================
// COMPLETE FIXED MoveTPByTicks method
// "Value cannot be null" fix — replace your existing method
// ============================================================
private void MoveTPByTicks(int ticks)
{
    try
    {
        var cw = Window.GetWindow(ChartControl.Parent) as Chart;
        if (cw == null) { Print("MoveTP: chartWindow null"); return; }

        var acctSel = cw.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
        if (acctSel?.SelectedAccount == null) { Print("MoveTP: no account selected"); return; }

        Account acct = acctSel.SelectedAccount;

        Position pos = acct.Positions.FirstOrDefault(p => p.Instrument == Instrument);
        if (pos == null || pos.MarketPosition == MarketPosition.Flat)
        {
            Print("MoveTP: No open position");
            return;
        }

        List<Order> tpOrders = new List<Order>();

        foreach (Order order in acct.Orders)
        {
            if (order.Instrument != Instrument) continue;
            if (order.OrderState == OrderState.Cancelled ||
                order.OrderState == OrderState.Filled ||
                order.OrderState == OrderState.Rejected) continue;
            if (order.OrderType != OrderType.Limit) continue;

            bool isTP = false;
            if (pos.MarketPosition == MarketPosition.Long &&
               (order.OrderAction == OrderAction.Sell || order.OrderAction == OrderAction.SellShort))
                isTP = true;
            else if (pos.MarketPosition == MarketPosition.Short &&
                    (order.OrderAction == OrderAction.Buy || order.OrderAction == OrderAction.BuyToCover))
                isTP = true;

            if (isTP) tpOrders.Add(order);
        }

        if (tpOrders.Count == 0)
        {
            Print("MoveTP: No TP (limit) orders found");
            return;
        }

        foreach (Order tpOrder in tpOrders)
        {
            double newPrice;
            if (pos.MarketPosition == MarketPosition.Long)
                newPrice = tpOrder.LimitPrice + (ticks * Instrument.MasterInstrument.TickSize);
            else
                newPrice = tpOrder.LimitPrice - (ticks * Instrument.MasterInstrument.TickSize);

            newPrice = Instrument.MasterInstrument.RoundToTickSize(newPrice);

            Order changed = tpOrder;
            changed = acct.CreateOrder(
                Instrument,
                tpOrder.OrderAction,
                OrderType.Limit,
                OrderEntry.Manual,
                TimeInForce.Day,
                tpOrder.Quantity,
                newPrice,
                0,
                "",
                tpOrder.Name,
                Core.Globals.MaxDate,
                null);

            acct.Cancel(new[] { tpOrder });
            acct.Submit(new[] { changed });

            Print($"MoveTP: moved {tpOrder.Name} from {tpOrder.LimitPrice} to {newPrice} ({ticks} ticks)");
        }
    }
    catch (Exception ex)
    {
        Print($"MoveTPByTicks ERROR: {ex.Message}\n{ex.StackTrace}");
    }
}

        #endregion

        #region Breakeven Management

        private void MoveToBreakeven(int offset)
        {
            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
            var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
            if (xAlselector == null) return;

            string currentAccountStr = xAlselector.SelectedAccount.ToString();
            Account Acct = Account.All.FirstOrDefault(x => currentAccountStr.Contains(x.Name));
            if (Acct == null) return;

            var xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
            if (xInSelector == null) return;

            string currentInstrument = xInSelector.Instrument.ToString();
            Position thisPosition = Acct.Positions.FirstOrDefault(x => currentInstrument.Contains(x.Instrument.FullName));

            if (thisPosition == null || thisPosition.MarketPosition == MarketPosition.Flat)
                return;

            foreach (Order order in Acct.Orders)
            {
                if (thisPosition.Account == order.Account && thisPosition.Instrument == order.Instrument)
                {
                    if (order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit)
                    {
                        if (order.OrderState != OrderState.Cancelled && order.OrderState != OrderState.Filled)
                        {
                            Order stopOrder = order;

                            if (thisPosition.MarketPosition == MarketPosition.Short)
                            {
                                stopOrder.StopPriceChanged = thisPosition.AveragePrice - offset * order.Instrument.MasterInstrument.TickSize;
                                Acct.Change(new[] { stopOrder });
                            }
                            else if (thisPosition.MarketPosition == MarketPosition.Long)
                            {
                                stopOrder.StopPriceChanged = thisPosition.AveragePrice + offset * order.Instrument.MasterInstrument.TickSize;
                                Acct.Change(new[] { stopOrder });
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Bracket Order Management

        private void AddMissingBracketOrders()
        {
            try
            {
                var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                var xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
                if (xAlselector == null)
                {
                    Print("Bracket: No account selector found");
                    return;
                }

                string currentAccountStr = xAlselector.SelectedAccount.ToString();
                Account Acct = Account.All.FirstOrDefault(x => currentAccountStr.Contains(x.Name));
                if (Acct == null)
                {
                    Print("Bracket: No account found");
                    return;
                }

                var xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as InstrumentSelector;
                if (xInSelector == null)
                {
                    Print("Bracket: No instrument selector found");
                    return;
                }

                string currentInstrument = xInSelector.Instrument.ToString();
                Position thisPosition = Acct.Positions.FirstOrDefault(x => currentInstrument.Contains(x.Instrument.FullName));

                if (thisPosition == null || thisPosition.MarketPosition == MarketPosition.Flat)
                {
                    Print("Bracket: No position found");
                    return;
                }

                // Check what orders already exist
                bool hasStopLoss = false;
                bool hasTakeProfit = false;

                foreach (Order order in Acct.Orders)
                {
                    if (order.Instrument == Instrument &&
                        order.OrderState != OrderState.Cancelled &&
                        order.OrderState != OrderState.Filled &&
                        order.OrderState != OrderState.Rejected)
                    {
                        if (order.OrderType == OrderType.StopMarket || order.OrderType == OrderType.StopLimit)
                        {
                            hasStopLoss = true;
                            Print($"Bracket: Found existing SL @ {order.StopPrice}");
                        }
                        if (order.OrderType == OrderType.Limit && order.IsBacktestOrder == false)
                        {
                            hasTakeProfit = true;
                            Print($"Bracket: Found existing TP @ {order.LimitPrice}");
                        }
                    }
                }

                if (hasStopLoss && hasTakeProfit)
                {
                    Print("Bracket: Both SL and TP already exist - nothing to add");
                    return;
                }

                double entryPrice = thisPosition.AveragePrice;
                int positionSize = Math.Abs(thisPosition.Quantity);
                Print($"Position: {thisPosition.MarketPosition} {positionSize} @ {entryPrice}");

                // Add missing Stop Loss
                if (!hasStopLoss)
                {
                    double stopPrice = 0;
                    double intendedStopPrice = 0;

                    if (thisPosition.MarketPosition == MarketPosition.Long)
                    {
                        intendedStopPrice = entryPrice - (BracketStopLossTicks * Instrument.MasterInstrument.TickSize);
                        double currentBid = GetCurrentBid();

                        if (currentBid <= intendedStopPrice)
                        {
                            stopPrice = currentBid - (20 * Instrument.MasterInstrument.TickSize);
                            Print($"⚠️ Bracket: Price ({currentBid}) already past intended SL ({intendedStopPrice})!");
                            Print($"🛡️ Bracket: Smart placement - placing emergency SL @ {stopPrice} (20 ticks below market)");
                        }
                        else
                        {
                            stopPrice = intendedStopPrice;
                        }
                    }
                    else if (thisPosition.MarketPosition == MarketPosition.Short)
                    {
                        intendedStopPrice = entryPrice + (BracketStopLossTicks * Instrument.MasterInstrument.TickSize);
                        double currentAsk = GetCurrentAsk();

                        if (currentAsk >= intendedStopPrice)
                        {
                            stopPrice = currentAsk + (20 * Instrument.MasterInstrument.TickSize);
                            Print($"⚠️ Bracket: Price ({currentAsk}) already past intended SL ({intendedStopPrice})!");
                            Print($"🛡️ Bracket: Smart placement - placing emergency SL @ {stopPrice} (20 ticks above market)");
                        }
                        else
                        {
                            stopPrice = intendedStopPrice;
                        }
                    }

                    OrderAction stopAction = thisPosition.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.Buy;
                    Order stopOrder = Acct.CreateOrder(Instrument, stopAction, OrderType.StopMarket, OrderEntry.Manual,
                        TimeInForce.Day, positionSize, 0, stopPrice, "", "Bracket SL", Core.Globals.MaxDate, null);

                    Acct.Submit(new[] { stopOrder });
                    Print($"✅ Bracket: Added Stop Loss @ {stopPrice}");
                }

                // Add missing Take Profit
                if (!hasTakeProfit)
                {
                    double targetPrice = 0;

                    if (thisPosition.MarketPosition == MarketPosition.Long)
                    {
                        targetPrice = entryPrice + (BracketTakeProfitTicks * Instrument.MasterInstrument.TickSize);
                        double currentBid = GetCurrentBid();

                        if (currentBid >= targetPrice)
                        {
                            Print($"Bracket: Price already at/past TP target - not placing TP order");
                            return;
                        }
                    }
                    else if (thisPosition.MarketPosition == MarketPosition.Short)
                    {
                        targetPrice = entryPrice - (BracketTakeProfitTicks * Instrument.MasterInstrument.TickSize);
                        double currentAsk = GetCurrentAsk();

                        if (currentAsk <= targetPrice)
                        {
                            Print($"Bracket: Price already at/past TP target - not placing TP order");
                            return;
                        }
                    }

                    OrderAction targetAction = thisPosition.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.Buy;
                    Order targetOrder = Acct.CreateOrder(Instrument, targetAction, OrderType.Limit, OrderEntry.Manual,
                        TimeInForce.Day, positionSize, targetPrice, 0, "", "Bracket TP", Core.Globals.MaxDate, null);

                    Acct.Submit(new[] { targetOrder });
                    Print($"Bracket: Added Take Profit @ {targetPrice}");
                }

                // Activate bracket flag after adding orders
                bracketActive = true;
                Print($"✅ Bracket: Active flag set to TRUE");

                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    bracketButton.Background = Brushes.Yellow;
                    bracketButton.Content = "Bracket ON";
                });
            }
            catch (Exception ex)
            {
                Print($"Error in AddMissingBracketOrders: {ex.Message}");
            }
        }

        #endregion

        #region Reverse Position Management

        private void OnReverseButtonClick(object sender, RoutedEventArgs e)
        {
            if (isProcessing)
            {
                Print("Already processing a reversal. Please wait.");
                return;
            }

            Print("=== REVERSE BUTTON CLICKED ===");

            TriggerCustomEvent(o =>
            {
                try
                {
                    isProcessing = true;
                    shouldBuyOnReverse = false;
                    shouldSellOnReverse = false;

                    if (chartWindow == null)
                    {
                        Print("Error: chartWindow is null");
                        isProcessing = false;
                        return;
                    }

                    var accountSelector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
                    if (accountSelector == null)
                    {
                        Print("Error: Could not find account selector");
                        isProcessing = false;
                        return;
                    }

                    Account account = accountSelector.SelectedAccount;
                    if (account == null)
                    {
                        Print("Error: No account selected");
                        isProcessing = false;
                        return;
                    }

                    Position position = account.Positions.FirstOrDefault(p => p.Instrument == Instrument);
                    if (position == null || position.MarketPosition == MarketPosition.Flat)
                    {
                        Print("No open position to reverse.");
                        isProcessing = false;
                        return;
                    }

                    int currentQuantity = Math.Abs(position.Quantity);
                    MarketPosition currentPosition = position.MarketPosition;
                    Print($"Step 1: Current position is {currentPosition}, Quantity: {currentQuantity}");

                    // Cancel all working orders
                    var workingOrders = account.Orders.Where(o =>
                        o.Instrument == Instrument &&
                        (o.OrderState == OrderState.Working ||
                         o.OrderState == OrderState.Accepted ||
                         o.OrderState == OrderState.Submitted)).ToList();

                    if (workingOrders.Count > 0)
                    {
                        Print($"Step 2: Cancelling {workingOrders.Count} working orders...");
                        foreach (var order in workingOrders)
                        {
                            try
                            {
                                account.Cancel(new[] { order });
                            }
                            catch { }
                        }
                    }

                    // Determine reverse direction
                    if (currentPosition == MarketPosition.Long)
                    {
                        shouldSellOnReverse = true;
                        Print("Step 3: Currently LONG, will reverse to SHORT (SELL)");
                    }
                    else
                    {
                        shouldBuyOnReverse = true;
                        Print("Step 3: Currently SHORT, will reverse to LONG (BUY)");
                    }

                    // Submit closing order
                    OrderAction closeAction = currentPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.Buy;
                    closeOrder = account.CreateOrder(Instrument, closeAction, OrderType.Market,
                        OrderEntry.Manual, TimeInForce.Day, currentQuantity, 0, 0,
                        string.Empty, "ReverseClose", Core.Globals.MaxDate, null);

                    currentAccount = account;
                    currentAccount.OrderUpdate += OnOrderUpdate;
                    currentAccount.Submit(new[] { closeOrder });

                    Print($"Step 4: Submitted close order with action {closeAction}");
                }
                catch (Exception ex)
                {
                    Print($"Error in OnReverseButtonClick: {ex.Message}");
                    isProcessing = false;
                }
            }, null);
        }

        private void OnOrderUpdate(object sender, OrderEventArgs e)
{
    if (closeOrder == null || e.Order != closeOrder)
        return;

    if (e.OrderState == OrderState.Filled)
    {
        Print("Step 5: Close order FILLED. Opening reverse position...");

        // Use ChartControl.Dispatcher to safely access UI thread
        ChartControl.Dispatcher.InvokeAsync(() =>
        {
            TriggerCustomEvent(o =>
            {
                try
                {
                    if (chartWindow == null)
                    {
                        Print("Error: chartWindow is null");
                        isProcessing = false;
                        return;
                    }

                    var atmSelector = chartWindow.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;
                    if (atmSelector == null || atmSelector.SelectedAtmStrategy == null)
                    {
                        Print("Error: No ATM strategy selected");
                        closeOrder = null;
                        if (currentAccount != null)
                            currentAccount.OrderUpdate -= OnOrderUpdate;
                        isProcessing = false;
                        return;
                    }

                    var quantitySelector = chartWindow.FindFirst("ChartTraderControlQuantitySelector") as NinjaTrader.Gui.Tools.QuantityUpDown;
                    int quantity = quantitySelector.Value;

                    // Determine entry action based on flags
                    OrderAction entryAction;
                    string directionName;

                    if (shouldBuyOnReverse)
                    {
                        entryAction = OrderAction.Buy;
                        directionName = "LONG";
                        Print($"Step 6: Creating BUY order for LONG position");
                    }
                    else if (shouldSellOnReverse)
                    {
                        entryAction = OrderAction.Sell;
                        directionName = "SHORT";
                        Print($"Step 6: Creating SELL order for SHORT position");
                    }
                    else
                    {
                        Print("ERROR: Neither buy nor sell flag was set!");
                        isProcessing = false;
                        return;
                    }

                    Order entryOrder = currentAccount.CreateOrder(
                        Instrument,
                        entryAction,
                        OrderType.Market,
                        OrderEntry.Manual,
                        TimeInForce.Day,
                        quantity,
                        0,
                        0,
                        string.Empty,
                        "Entry",
                        DateTime.MaxValue,
                        null
                    );

                    if (entryOrder != null)
                    {
                        Print($"Step 7: Order created with action {entryAction}, calling StartAtmStrategy...");
                        NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmSelector.SelectedAtmStrategy, entryOrder);
                        Print($"=== REVERSED TO {directionName} WITH ATM: {atmSelector.SelectedAtmStrategy.Template} ===");
                    }
                    else
                    {
                        Print("ERROR: Entry order was null!");
                    }

                    closeOrder = null;
                    currentAccount.OrderUpdate -= OnOrderUpdate;
                    shouldBuyOnReverse = false;
                    shouldSellOnReverse = false;
                    isProcessing = false;
                }
                catch (Exception ex)
                {
                    Print($"Error opening reverse position: {ex.Message}");
                    isProcessing = false;
                }
            }, null);
        });
    }
    else if (e.OrderState == OrderState.Rejected || e.OrderState == OrderState.Cancelled)
    {
        Print($"Close order was {e.OrderState}. Resetting.");
        closeOrder = null;
        if (currentAccount != null)
            currentAccount.OrderUpdate -= OnOrderUpdate;
        isProcessing = false;
    }
}


        private void CleanReverseState()
        {
            if (reverseWorkingAccount != null)
                reverseWorkingAccount.OrderUpdate -= OnOrderUpdate;

            closeOrder = null;
            reverseWorkingAccount = null;
            reverseInProgress = false;
        }

        #endregion
        
        #region Order Placement Helpers

        private Order PlaceDirectionalOrder(OrderAction action, double price)
        {
            Order placedOrder = null;

            try
            {
                // Use cached account - NO UI access
                if (cachedAccount == null)
                {
                    Print("Error: No cached account for directional order");
                    return null;
                }

                int quantity = 1;
                NinjaTrader.NinjaScript.AtmStrategy selectedAtmStrategy = null;

                // Get quantity and ATM from ChartTrader on UI thread
                try
                {
                    if (ChartControl != null)
                    {
                        ChartControl.Dispatcher.Invoke(() =>
                        {
                            var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;

                            // Get quantity
                            var quantitySelector = chartWindow?.FindFirst("ChartTraderControlQuantitySelector") as QuantityUpDown;
                            if (quantitySelector != null)
                                quantity = quantitySelector.Value;

                            // Get ATM strategy
                            var atmSelector = chartWindow?.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;
                            if (atmSelector != null)
                                selectedAtmStrategy = atmSelector.SelectedAtmStrategy;
                        });
                    }
                }
                catch (Exception ex)
                {
                    Print($"Warning: Could not access ChartTrader controls: {ex.Message}");
                }

                double currentPrice = (action == OrderAction.Buy) ? GetCurrentAsk() : GetCurrentBid();

                // Determine order type
                OrderType orderType;
                double limitPrice = 0;
                double stopPrice = 0;

                if (action == OrderAction.Buy)
                {
                    if (price < currentPrice)
                    {
                        orderType = OrderType.Limit;
                        limitPrice = price;
                    }
                    else
                    {
                        orderType = OrderType.StopMarket;
                        stopPrice = price;
                    }
                }
                else // Sell
                {
                    if (price > currentPrice)
                    {
                        orderType = OrderType.Limit;
                        limitPrice = price;
                    }
                    else
                    {
                        orderType = OrderType.StopMarket;
                        stopPrice = price;
                    }
                }

                // Create order using cached account
                placedOrder = cachedAccount.CreateOrder(
                    Instrument,
                    action,
                    orderType,
                    OrderEntry.Manual,
                    TimeInForce.Day,
                    quantity,
                    limitPrice,
                    stopPrice,
                    "",
                    "Entry",
                    Core.Globals.MaxDate,
                    null);

                if (placedOrder != null)
                {
                    // Attach ATM BEFORE submitting
                    if (selectedAtmStrategy != null)
                    {
                        var atmId = NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(
                            selectedAtmStrategy,
                            placedOrder);
                        Print($"✅ Directional order with ATM: {selectedAtmStrategy.Name} - {action} {quantity} at {price}");
                    }
                    else
                    {
                        Print($"Directional order WITHOUT ATM - {action} {quantity} {orderType} at {price}");
                    }

                    // Submit order
                    cachedAccount.Submit(new[] { placedOrder });
                }
            }
            catch (Exception ex)
            {
                Print($"Exception in PlaceDirectionalOrder: {ex.Message}");
            }

            return placedOrder;
        }

        private void PlaceOrder(Account account, OrderAction action, double price)
        {
            try
            {
                var window = Window.GetWindow(ChartControl.Parent) as Chart;
                var atmSelector = window.FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;

                TriggerCustomEvent(o =>
                {
                    Order order = account.CreateOrder(Instrument, action, OrderType.Limit, OrderEntry.Manual, TimeInForce.Day, 1, price, 0, "", "Entry", Core.Globals.MaxDate, null);

                    if (atmSelector?.SelectedAtmStrategy != null)
                    {
                        NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmSelector.SelectedAtmStrategy, order);
                        Print($"Order submitted with ATM: {atmSelector.SelectedAtmStrategy}");
                    }
                    else
                    {
                        account.Submit(new[] { order });
                        Print($"Order submitted - {action} @ {price}");
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Print($"Error: {ex.Message}");
            }
        }

        #endregion

        #region Reversal Detection Helpers

        private bool IsBullishReversal()
        {
            if (CurrentBar < 1)
            {
                Print("IsBullishReversal: CurrentBar < 1, returning false");
                return false;
            }

            bool previousBearish = Close[1] < Open[1];
            bool currentBullish = Close[0] > Open[0];
            bool lowExtended = Low[0] <= Low[1];
            bool isReversal = previousBearish && currentBullish && lowExtended;

            if (isReversal)
            {
                Print($"✅ BULLISH REVERSAL: Prev Red={previousBearish}, Curr Green={currentBullish}, LowExtended={lowExtended}");
            }

            return isReversal;
        }

        private bool IsBearishReversal()
        {
            if (CurrentBar < 1)
            {
                Print("IsBearishReversal: CurrentBar < 1, returning false");
                return false;
            }

            bool previousBullish = Close[1] > Open[1];
            bool currentBearish = Close[0] < Open[0];
            bool highExtended = High[0] >= High[1];
            bool isReversal = previousBullish && currentBearish && highExtended;

            if (isReversal)
            {
                Print($"✅ BEARISH REVERSAL: Prev Green={previousBullish}, Curr Red={currentBearish}, HighExtended={highExtended}");
            }

            return isReversal;
        }

        #endregion

        #region Position and Market Data Helpers

        private MarketPosition GetCurrentPositionFromAccount()
        {
            try
            {
                // Use CACHED account to avoid cross-thread access
                if (cachedAccount == null)
                {
                    // Try to get it from monitoredAccount as fallback
                    if (monitoredAccount != null)
                    {
                        cachedAccount = monitoredAccount;
                    }
                    else
                    {
                        return MarketPosition.Flat;
                    }
                }

                // Access account directly - safe from any thread
                lock (cachedAccount.Positions)
                {
                    foreach (var pos in cachedAccount.Positions)
                    {
                        if (pos.Instrument == Instrument)
                        {
                            return pos.MarketPosition;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Print($"Error getting position: {ex.Message}");
            }

            return MarketPosition.Flat;
        }

        private void ExitPositionOnReversal()
        {
            try
            {
                // Use cached account - NO UI access needed
                if (cachedAccount == null)
                {
                    // Fallback to monitoredAccount
                    if (monitoredAccount != null)
                    {
                        cachedAccount = monitoredAccount;
                    }
                    else
                    {
                        Print("Exit on Reverse Error: No account cached");
                        return;
                    }
                }

                // Direct account access - safe from any thread
                cachedAccount.Flatten(new[] { Instrument });
                Print("✅ Exit on Reverse: Position flattened");
            }
            catch (Exception ex)
            {
                Print($"Exit on Reverse Error: {ex.Message}");
            }
        }

        private string GetSelectedAtmTemplate()
        {
            string template = null;

            ChartControl.Dispatcher.Invoke(() =>
            {
                var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
                var comboBox = chartWindow?.FindFirst("ChartTraderControlATMStrategySelector") as System.Windows.Controls.ComboBox;

                if (comboBox?.SelectedItem != null)
                {
                    var itemString = comboBox.SelectedItem.ToString();
                    var idx = itemString.IndexOf("Template: ");

                    if (idx >= 0)
                    {
                        template = itemString.Substring(idx + "Template: ".Length).Trim();
                    }
                    else
                    {
                        template = itemString;
                    }
                }
            });

            return template;
        }

        private int GetOffsetValue()
        {
            try
            {
                if (offsetTextBox != null && !string.IsNullOrEmpty(offsetTextBox.Text))
                {
                    if (int.TryParse(offsetTextBox.Text, out int offset))
                    {
                        return offset;
                    }
                }
            }
            catch (Exception ex)
            {
                Print($"Error getting offset: {ex.Message}");
            }

            return 1; // Default offset
        }

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Range(-20, 100)]
        [Display(Name = "Offset", Order = 1, GroupName = "Entry")]
        public int Offset { get; set; }

        [NinjaScriptProperty]
        [Range(-20, int.MaxValue)]
        [Display(Name = "SL Button 1", Order = 2, GroupName = "SL/BE")]
        public int SL1Value { get; set; }

        [NinjaScriptProperty]
        [Range(-20, int.MaxValue)]
        [Display(Name = "SL Button 2", Order = 3, GroupName = "SL/BE")]
        public int SL2Value { get; set; }

        [NinjaScriptProperty]
        [Range(-20, int.MaxValue)]
        [Display(Name = "SL Button 3", Order = 4, GroupName = "SL/BE")]
        public int SL3Value { get; set; }

        [NinjaScriptProperty]
        [Range(-20, 100)]
        [Display(Name = "BE Button 1", Order = 5, GroupName = "SL/BE")]
        public int BE1Value { get; set; }

        [NinjaScriptProperty]
        [Range(-20, 100)]
        [Display(Name = "BE Button 2", Order = 6, GroupName = "SL/BE")]
        public int BE2Value { get; set; }

        [NinjaScriptProperty]
        [Range(-20, 100)]
        [Display(Name = "BE Button 3", Order = 7, GroupName = "SL/BE")]
        public int BE3Value { get; set; }

        [NinjaScriptProperty]
        [Range(-20, 100)]
        [Display(Name = "BE Button 4", Order = 8, GroupName = "SL/BE")]
        public int BE4Value { get; set; }
		
		// ADD THESE EXACTLY LIKE YOUR SL/BE PROPERTIES
		[Range(-100, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "TP1Value", Description = "TP1 Ticks", Order = 15, GroupName = "TP Settings")]
		public int TP1Value { get; set; }
		
		[Range(-100, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "TP2Value", Description = "TP2 Ticks", Order = 16, GroupName = "TP Settings")]
		public int TP2Value { get; set; }
		
		[Range(-100, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "TP3Value", Description = "TP3 Ticks", Order = 17, GroupName = "TP Settings")]
		public int TP3Value { get; set; }
		
		[Range(-100, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "TP4Value", Description = "TP4 Ticks", Order = 18, GroupName = "TP Settings")]
		public int TP4Value { get; set; }


        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Bracket SL Ticks", Order = 9, GroupName = "Bracket")]
        public int BracketStopLossTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Bracket TP Ticks", Order = 10, GroupName = "Bracket")]
        public int BracketTakeProfitTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, 10)]
        [Display(Name = "Exit Reverse Bars", Order = 11, GroupName = "Position Mgmt")]
        public int ExitReverseBars { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Buy Chart Hotkey", Order = 12, GroupName = "Hotkeys")]
        public KeyDesired BuyChartKey { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Sell Chart Hotkey", Order = 13, GroupName = "Hotkeys")]
        public KeyDesired SellChartKey { get; set; }

        #endregion
    }

  
}

  public enum KeyDesired
    {
        LeftAlt,
        LeftShift,
        RightAlt,
        RightShift
    }

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeControlPanel[] cacheTradeControlPanel;
		public TradeControlPanel TradeControlPanel(int offset, int sL1Value, int sL2Value, int sL3Value, int bE1Value, int bE2Value, int bE3Value, int bE4Value, int tP1Value, int tP2Value, int tP3Value, int tP4Value, int bracketStopLossTicks, int bracketTakeProfitTicks, int exitReverseBars, KeyDesired buyChartKey, KeyDesired sellChartKey)
		{
			return TradeControlPanel(Input, offset, sL1Value, sL2Value, sL3Value, bE1Value, bE2Value, bE3Value, bE4Value, tP1Value, tP2Value, tP3Value, tP4Value, bracketStopLossTicks, bracketTakeProfitTicks, exitReverseBars, buyChartKey, sellChartKey);
		}

		public TradeControlPanel TradeControlPanel(ISeries<double> input, int offset, int sL1Value, int sL2Value, int sL3Value, int bE1Value, int bE2Value, int bE3Value, int bE4Value, int tP1Value, int tP2Value, int tP3Value, int tP4Value, int bracketStopLossTicks, int bracketTakeProfitTicks, int exitReverseBars, KeyDesired buyChartKey, KeyDesired sellChartKey)
		{
			if (cacheTradeControlPanel != null)
				for (int idx = 0; idx < cacheTradeControlPanel.Length; idx++)
					if (cacheTradeControlPanel[idx] != null && cacheTradeControlPanel[idx].Offset == offset && cacheTradeControlPanel[idx].SL1Value == sL1Value && cacheTradeControlPanel[idx].SL2Value == sL2Value && cacheTradeControlPanel[idx].SL3Value == sL3Value && cacheTradeControlPanel[idx].BE1Value == bE1Value && cacheTradeControlPanel[idx].BE2Value == bE2Value && cacheTradeControlPanel[idx].BE3Value == bE3Value && cacheTradeControlPanel[idx].BE4Value == bE4Value && cacheTradeControlPanel[idx].TP1Value == tP1Value && cacheTradeControlPanel[idx].TP2Value == tP2Value && cacheTradeControlPanel[idx].TP3Value == tP3Value && cacheTradeControlPanel[idx].TP4Value == tP4Value && cacheTradeControlPanel[idx].BracketStopLossTicks == bracketStopLossTicks && cacheTradeControlPanel[idx].BracketTakeProfitTicks == bracketTakeProfitTicks && cacheTradeControlPanel[idx].ExitReverseBars == exitReverseBars && cacheTradeControlPanel[idx].BuyChartKey == buyChartKey && cacheTradeControlPanel[idx].SellChartKey == sellChartKey && cacheTradeControlPanel[idx].EqualsInput(input))
						return cacheTradeControlPanel[idx];
			return CacheIndicator<TradeControlPanel>(new TradeControlPanel(){ Offset = offset, SL1Value = sL1Value, SL2Value = sL2Value, SL3Value = sL3Value, BE1Value = bE1Value, BE2Value = bE2Value, BE3Value = bE3Value, BE4Value = bE4Value, TP1Value = tP1Value, TP2Value = tP2Value, TP3Value = tP3Value, TP4Value = tP4Value, BracketStopLossTicks = bracketStopLossTicks, BracketTakeProfitTicks = bracketTakeProfitTicks, ExitReverseBars = exitReverseBars, BuyChartKey = buyChartKey, SellChartKey = sellChartKey }, input, ref cacheTradeControlPanel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeControlPanel TradeControlPanel(int offset, int sL1Value, int sL2Value, int sL3Value, int bE1Value, int bE2Value, int bE3Value, int bE4Value, int tP1Value, int tP2Value, int tP3Value, int tP4Value, int bracketStopLossTicks, int bracketTakeProfitTicks, int exitReverseBars, KeyDesired buyChartKey, KeyDesired sellChartKey)
		{
			return indicator.TradeControlPanel(Input, offset, sL1Value, sL2Value, sL3Value, bE1Value, bE2Value, bE3Value, bE4Value, tP1Value, tP2Value, tP3Value, tP4Value, bracketStopLossTicks, bracketTakeProfitTicks, exitReverseBars, buyChartKey, sellChartKey);
		}

		public Indicators.TradeControlPanel TradeControlPanel(ISeries<double> input , int offset, int sL1Value, int sL2Value, int sL3Value, int bE1Value, int bE2Value, int bE3Value, int bE4Value, int tP1Value, int tP2Value, int tP3Value, int tP4Value, int bracketStopLossTicks, int bracketTakeProfitTicks, int exitReverseBars, KeyDesired buyChartKey, KeyDesired sellChartKey)
		{
			return indicator.TradeControlPanel(input, offset, sL1Value, sL2Value, sL3Value, bE1Value, bE2Value, bE3Value, bE4Value, tP1Value, tP2Value, tP3Value, tP4Value, bracketStopLossTicks, bracketTakeProfitTicks, exitReverseBars, buyChartKey, sellChartKey);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeControlPanel TradeControlPanel(int offset, int sL1Value, int sL2Value, int sL3Value, int bE1Value, int bE2Value, int bE3Value, int bE4Value, int tP1Value, int tP2Value, int tP3Value, int tP4Value, int bracketStopLossTicks, int bracketTakeProfitTicks, int exitReverseBars, KeyDesired buyChartKey, KeyDesired sellChartKey)
		{
			return indicator.TradeControlPanel(Input, offset, sL1Value, sL2Value, sL3Value, bE1Value, bE2Value, bE3Value, bE4Value, tP1Value, tP2Value, tP3Value, tP4Value, bracketStopLossTicks, bracketTakeProfitTicks, exitReverseBars, buyChartKey, sellChartKey);
		}

		public Indicators.TradeControlPanel TradeControlPanel(ISeries<double> input , int offset, int sL1Value, int sL2Value, int sL3Value, int bE1Value, int bE2Value, int bE3Value, int bE4Value, int tP1Value, int tP2Value, int tP3Value, int tP4Value, int bracketStopLossTicks, int bracketTakeProfitTicks, int exitReverseBars, KeyDesired buyChartKey, KeyDesired sellChartKey)
		{
			return indicator.TradeControlPanel(input, offset, sL1Value, sL2Value, sL3Value, bE1Value, bE2Value, bE3Value, bE4Value, tP1Value, tP2Value, tP3Value, tP4Value, bracketStopLossTicks, bracketTakeProfitTicks, exitReverseBars, buyChartKey, sellChartKey);
		}
	}
}

#endregion
