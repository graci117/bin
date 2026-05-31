#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class TradingPanel_EN : Indicator
    {
        #region Private Variables

        // ChartTrader WPF elements
        private Chart chartWindow;
        private ChartTrader chartTrader;
        private Grid chartTraderGrid;
        private bool panelActive;
        private AccountSelector accountSelector;
        private InstrumentSelector instrumentSelector;

        // Custom UI elements
        private Grid mainPanelGrid;
        private RowDefinition panelRow;

        // Buttons - main buttons
        private Button btnCloseHalf;
        private Button btnCloseThird;
        private Button btnCloseAll;
        private Button btnClosePosition;
        private Button btnCancelWorkingOrders;

        // Risk Control
        private Button btnProfitClose;
        private TextBox txtProfitAmount;
        private Button btnLossClose;
        private TextBox txtLossAmount;

        // Account Info Display
        private Button btnAccountBalance;
        private Button btnAccountEquity;
        private Button btnDailyPnL;

        // State variables
        private bool isProfitCloseEnabled = false;
        private bool isLossCloseEnabled = false;

        // Account reference
        private Account selectedAccount;

        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Trading Panel EN - Trading panel with partial close and risk controls.";
                Name = "TradingPanel_EN";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = false;
                DrawVerticalGridLines = false;
                PaintPriceMarkers = false;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = false;

                // Default values
                ProfitCloseAmount = 1000;
                LossCloseAmount = 1000;
            }
            else if (State == State.Historical)
            {
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        CreateWPFControls();
                    });
                }
            }
            else if (State == State.Terminated)
            {
                if (ChartControl != null)
                {
                    ChartControl.Dispatcher.InvokeAsync(() =>
                    {
                        DisposeWPFControls();
                    });
                }
            }
        }

        #region WPF Controls Creation

        private void CreateWPFControls()
        {
            chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
            if (chartWindow == null) return;

            chartTrader = chartWindow.FindFirst("ChartWindowChartTraderControl") as ChartTrader;
            if (chartTrader == null) return;

            chartTraderGrid = chartTrader.Content as Grid;
            if (chartTraderGrid == null) return;

            // Get account selector
            accountSelector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
            
            // Get instrument selector (controls which instrument ChartTrader is operating on)
            // Note: If this returns null (depending on NT version/template), we fall back to this script's Instrument.
            instrumentSelector = chartWindow.FindFirst("ChartTraderControlInstrumentSelector") as InstrumentSelector;

            InsertWPFControls();
        }

        private void InsertWPFControls()
        {
            if (panelActive) return;

            // Add new row to ChartTrader grid
            panelRow = new RowDefinition();
            panelRow.Height = new GridLength(260);
            chartTraderGrid.RowDefinitions.Add(panelRow);

            // Create main panel grid
            mainPanelGrid = new Grid();
            mainPanelGrid.Name = "TradingPanelGrid_EN";
            mainPanelGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            mainPanelGrid.VerticalAlignment = VerticalAlignment.Top;
            mainPanelGrid.Margin = new Thickness(2, 5, 2, 0);

            // Define columns (2 columns: 1:1 ratio)
            mainPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainPanelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Define rows
            for (int i = 0; i < 12; i++)
            {
                mainPanelGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            int row = 0;

            // === Row 0: Close All (full width) ===
            btnCloseAll = CreateButton("Close All", 0, row, Brushes.Crimson);
            Grid.SetColumnSpan(btnCloseAll, 2);
            btnCloseAll.Click += OnCloseAllClick;
            mainPanelGrid.Children.Add(btnCloseAll);
            row++;

            // === Row 1: Close Positions / Cancel Working Orders ===
            btnClosePosition = CreateButton("Close Pos", 0, row, Brushes.Brown);
            btnClosePosition.Click += OnClosePositionClick;
            mainPanelGrid.Children.Add(btnClosePosition);

            btnCancelWorkingOrders = CreateButton("Cancel Pending", 1, row, Brushes.Brown);
            btnCancelWorkingOrders.Click += OnCancelWorkingOrdersClick;
            mainPanelGrid.Children.Add(btnCancelWorkingOrders);
            row++;

            // === Row 2: Close Half / Close Third ===
            btnCloseHalf = CreateButton("Close 1/2", 0, row, Brushes.DarkOrange);
            btnCloseHalf.Click += OnCloseHalfClick;
            mainPanelGrid.Children.Add(btnCloseHalf);

            btnCloseThird = CreateButton("Close 1/3", 1, row, Brushes.DarkOrange);
            btnCloseThird.Click += OnCloseThirdClick;
            mainPanelGrid.Children.Add(btnCloseThird);
            row++;

            // === Row 3: Profit Close ===
            btnProfitClose = CreateToggleButton("PnL Target", 0, row, 1);
            btnProfitClose.Click += OnProfitCloseToggle;
            mainPanelGrid.Children.Add(btnProfitClose);

            Grid profitPanel = new Grid();
            profitPanel.Margin = new Thickness(2);
            profitPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            profitPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            profitPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            txtProfitAmount = CreateTextBox(ProfitCloseAmount.ToString(), -1, -1);
            txtProfitAmount.Margin = new Thickness(0);
            txtProfitAmount.Width = double.NaN;
            txtProfitAmount.HorizontalAlignment = HorizontalAlignment.Stretch;
            txtProfitAmount.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(txtProfitAmount, 0);
            Grid.SetRow(txtProfitAmount, 0);
            profitPanel.Children.Add(txtProfitAmount);

            Button btnProfitIcon = CreateIconButton(txtProfitAmount, OnProfitAmountChanged);
            Grid.SetColumn(btnProfitIcon, 1);
            Grid.SetRow(btnProfitIcon, 0);
            profitPanel.Children.Add(btnProfitIcon);

            Grid.SetColumn(profitPanel, 1);
            Grid.SetRow(profitPanel, row);
            mainPanelGrid.Children.Add(profitPanel);
            row++;

            // === Row 4: Loss Close ===
            btnLossClose = CreateToggleButton("PnL Stop", 0, row, 1);
            btnLossClose.Click += OnLossCloseToggle;
            mainPanelGrid.Children.Add(btnLossClose);

            Grid lossPanel = new Grid();
            lossPanel.Margin = new Thickness(2);
            lossPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            lossPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            lossPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            txtLossAmount = CreateTextBox(LossCloseAmount.ToString(), -1, -1);
            txtLossAmount.Margin = new Thickness(0);
            txtLossAmount.Width = double.NaN;
            txtLossAmount.HorizontalAlignment = HorizontalAlignment.Stretch;
            txtLossAmount.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(txtLossAmount, 0);
            Grid.SetRow(txtLossAmount, 0);
            lossPanel.Children.Add(txtLossAmount);

            Button btnLossIcon = CreateIconButton(txtLossAmount, OnLossAmountChanged);
            Grid.SetColumn(btnLossIcon, 1);
            Grid.SetRow(btnLossIcon, 0);
            lossPanel.Children.Add(btnLossIcon);

            Grid.SetColumn(lossPanel, 1);
            Grid.SetRow(lossPanel, row);
            mainPanelGrid.Children.Add(lossPanel);
            row++;

            // === Row 5: Account Info Title ===
            Button accountInfoTitle = CreateReadOnlyButton("Acct Info", 0, row, 2);
            accountInfoTitle.Background = Brushes.DimGray;
            accountInfoTitle.Foreground = Brushes.White;
            Grid.SetColumnSpan(accountInfoTitle, 2);
            mainPanelGrid.Children.Add(accountInfoTitle);
            row++;

            // === Row 6: Account Balance ===
            Button btnBalanceLabel = CreateReadOnlyButton("Cash Value:", 0, row, 1);
            mainPanelGrid.Children.Add(btnBalanceLabel);

            btnAccountBalance = CreateReadOnlyButton("$0.00", 1, row, 1);
            mainPanelGrid.Children.Add(btnAccountBalance);
            row++;

            // === Row 7: Account Equity ===
            Button btnEquityLabel = CreateReadOnlyButton("Equity:", 0, row, 1);
            mainPanelGrid.Children.Add(btnEquityLabel);

            btnAccountEquity = CreateReadOnlyButton("$0.00", 1, row, 1);
            mainPanelGrid.Children.Add(btnAccountEquity);
            row++;

            // === Row 8: Daily PnL ===
            Button btnPnLLabel = CreateReadOnlyButton("Daily PnL:", 0, row, 1);
            mainPanelGrid.Children.Add(btnPnLLabel);

            btnDailyPnL = CreateReadOnlyButton("$0.00", 1, row, 1);
            mainPanelGrid.Children.Add(btnDailyPnL);
            row++;

            // Add to ChartTrader grid
            chartTraderGrid.Children.Add(mainPanelGrid);
            Grid.SetRow(mainPanelGrid, chartTraderGrid.RowDefinitions.Count - 1);

            panelActive = true;

            // Subscribe to account changes
            if (accountSelector != null)
            {
                accountSelector.SelectionChanged += OnAccountSelectionChanged;
                selectedAccount = accountSelector.SelectedAccount;
                SubscribeToAccount();
            }

            Print("[TradingPanel_EN] Panel initialized successfully");
        }

        private Button CreateButton(string text, int col, int row, Brush background)
        {
            Button btn = new Button();
            btn.Content = text;
            btn.Foreground = Brushes.White;
            btn.Background = background;
            btn.FontWeight = FontWeights.Bold;
            btn.FontSize = 11;
            btn.Height = 25;
            btn.Margin = new Thickness(2);
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn.VerticalContentAlignment = VerticalAlignment.Center;
            Grid.SetColumn(btn, col);
            Grid.SetRow(btn, row);
            return btn;
        }

        private Button CreateToggleButton(string text, int col, int row, int colSpan)
        {
            Button btn = new Button();
            btn.Content = text;
            btn.Foreground = Brushes.Black;
            btn.Background = Brushes.LightGray;
            btn.FontSize = 11;
            btn.Height = 25;
            btn.Margin = new Thickness(2);
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn.VerticalContentAlignment = VerticalAlignment.Center;
            Grid.SetColumn(btn, col);
            Grid.SetRow(btn, row);
            Grid.SetColumnSpan(btn, colSpan);
            return btn;
        }

        private Button CreateReadOnlyButton(string text, int col, int row, int colSpan)
        {
            Button btn = new Button();
            btn.Content = text;
            btn.Foreground = Brushes.Black;
            btn.Background = Brushes.LightGray;
            btn.FontSize = 11;
            btn.Height = 25;
            btn.Margin = new Thickness(2);
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn.VerticalContentAlignment = VerticalAlignment.Center;
            btn.IsEnabled = false;
            btn.IsHitTestVisible = false;
            Grid.SetColumn(btn, col);
            Grid.SetRow(btn, row);
            Grid.SetColumnSpan(btn, colSpan);
            return btn;
        }

        private string ShowInputDialog(string title, string currentValue)
        {
            string result = currentValue;

            if (ChartControl == null) return result;

            // Use Invoke to ensure we're on the UI thread for ShowDialog
            ChartControl.Dispatcher.Invoke(() =>
            {
                Window inputWindow = new Window();
                inputWindow.Title = title;
                inputWindow.Width = 300;
                inputWindow.Height = 150;
                inputWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                inputWindow.ResizeMode = ResizeMode.NoResize;
                inputWindow.WindowStyle = WindowStyle.SingleBorderWindow;

                Grid grid = new Grid();
                grid.Margin = new Thickness(10);

                // Input TextBox
                TextBox inputBox = new TextBox();
                inputBox.Text = currentValue;
                inputBox.FontSize = 14;
                inputBox.Height = 30;
                inputBox.Margin = new Thickness(0, 0, 0, 10);
                inputBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                inputBox.VerticalAlignment = VerticalAlignment.Top;
                inputBox.TextAlignment = TextAlignment.Center;
                Grid.SetRow(inputBox, 0);
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.Children.Add(inputBox);

                // Buttons
                StackPanel buttonPanel = new StackPanel();
                buttonPanel.Orientation = Orientation.Horizontal;
                buttonPanel.HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetRow(buttonPanel, 1);
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                Button btnOK = new Button();
                btnOK.Content = "OK";
                btnOK.Width = 80;
                btnOK.Height = 30;
                btnOK.Margin = new Thickness(5, 0, 5, 0);
                btnOK.Click += (s, e) =>
                {
                    result = inputBox.Text;
                    inputWindow.DialogResult = true;
                    inputWindow.Close();
                };

                Button btnCancel = new Button();
                btnCancel.Content = "Cancel";
                btnCancel.Width = 80;
                btnCancel.Height = 30;
                btnCancel.Margin = new Thickness(5, 0, 5, 0);
                btnCancel.Click += (s, e) =>
                {
                    inputWindow.DialogResult = false;
                    inputWindow.Close();
                };

                buttonPanel.Children.Add(btnOK);
                buttonPanel.Children.Add(btnCancel);
                grid.Children.Add(buttonPanel);

                inputWindow.Content = grid;

                // Select all text and focus
                inputBox.SelectAll();
                inputBox.Focus();

                // Handle Enter / Escape
                inputBox.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        result = inputBox.Text;
                        inputWindow.DialogResult = true;
                        inputWindow.Close();
                    }
                    else if (e.Key == Key.Escape)
                    {
                        inputWindow.DialogResult = false;
                        inputWindow.Close();
                    }
                };

                bool? dialogResult = inputWindow.ShowDialog();
                if (dialogResult != true)
                {
                    result = currentValue; // Keep original value if cancelled
                }
            });

            return result;
        }

        private Button CreateIconButton(TextBox targetTextBox, Action<string> onValueChanged)
        {
            Button btn = new Button();
            btn.Content = "...";
            btn.FontSize = 11;
            btn.Width = double.NaN;
            btn.MinWidth = 25;
            btn.Height = 22;
            btn.Margin = new Thickness(2, 0, 0, 0);
            btn.Padding = new Thickness(1);
            btn.HorizontalContentAlignment = HorizontalAlignment.Center;
            btn.VerticalContentAlignment = VerticalAlignment.Center;
            btn.HorizontalAlignment = HorizontalAlignment.Stretch;
            btn.Background = Brushes.LightGray;
            btn.Foreground = Brushes.Black;
            btn.BorderBrush = Brushes.Gray;
            btn.BorderThickness = new Thickness(1);

            // Click to show input dialog
            btn.Click += (s, e) =>
            {
                string newValue = ShowInputDialog("Enter value", targetTextBox.Text);
                if (newValue != targetTextBox.Text)
                {
                    targetTextBox.Text = newValue;
                    if (onValueChanged != null)
                    {
                        onValueChanged(newValue);
                    }
                }
            };

            return btn;
        }

        private TextBox CreateTextBox(string text, int col, int row)
        {
            TextBox txt = new TextBox();
            txt.Text = text;
            txt.FontSize = 11;
            txt.Height = 22;
            txt.Width = double.NaN;
            txt.Margin = new Thickness(0);
            txt.TextAlignment = TextAlignment.Center;
            txt.VerticalContentAlignment = VerticalAlignment.Center;

            // Set as read-only to avoid focus issues
            txt.IsReadOnly = true;
            txt.IsEnabled = true;
            txt.IsHitTestVisible = false;
            txt.Background = Brushes.White;
            txt.Foreground = Brushes.Black;

            // Only set Grid properties if col and row are valid (not used in StackPanel)
            if (col >= 0 && row >= 0)
            {
                Grid.SetColumn(txt, col);
                Grid.SetRow(txt, row);
            }
            return txt;
        }

        private void DisposeWPFControls()
        {
            if (!panelActive) return;

            // Unsubscribe events
            if (accountSelector != null)
                accountSelector.SelectionChanged -= OnAccountSelectionChanged;

            UnsubscribeFromAccount();

            // Remove click handlers
            if (btnCloseHalf != null) btnCloseHalf.Click -= OnCloseHalfClick;
            if (btnCloseThird != null) btnCloseThird.Click -= OnCloseThirdClick;
            if (btnCloseAll != null) btnCloseAll.Click -= OnCloseAllClick;
            if (btnProfitClose != null) btnProfitClose.Click -= OnProfitCloseToggle;
            if (btnLossClose != null) btnLossClose.Click -= OnLossCloseToggle;
            if (btnClosePosition != null) btnClosePosition.Click -= OnClosePositionClick;
            if (btnCancelWorkingOrders != null) btnCancelWorkingOrders.Click -= OnCancelWorkingOrdersClick;

            // Remove from grid
            if (chartTraderGrid != null && mainPanelGrid != null)
                chartTraderGrid.Children.Remove(mainPanelGrid);

            if (chartTraderGrid != null && panelRow != null)
                chartTraderGrid.RowDefinitions.Remove(panelRow);

            mainPanelGrid = null;
            panelRow = null;
            panelActive = false;
        }

        #endregion

        #region Account Management

        private Instrument GetActiveInstrument()
        {
            // Always try to get fresh reference to chartWindow and chartTrader
            if (chartWindow == null && ChartControl != null)
            {
                chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
            }
            
            if (chartTrader == null && chartWindow != null)
            {
                chartTrader = chartWindow.FindFirst("ChartWindowChartTraderControl") as ChartTrader;
            }
            
            try
            {
                // Get from current active chart panel in the window (most reliable)
                if (chartWindow != null)
                {
                    var chartControlProp = chartWindow.GetType().GetProperty("ActiveChartControl");
                    if (chartControlProp != null)
                    {
                        var chartControl = chartControlProp.GetValue(chartWindow);
                        if (chartControl != null)
                        {
                            var instrumentProp = chartControl.GetType().GetProperty("Instrument");
                            if (instrumentProp != null)
                            {
                                var inst = instrumentProp.GetValue(chartControl) as Instrument;
                                if (inst != null) return inst;
                            }
                        }
                    }
                    
                    var activePanelProp = chartWindow.GetType().GetProperty("ActiveChartPanel");
                    if (activePanelProp != null)
                    {
                        var activePanel = activePanelProp.GetValue(chartWindow);
                        if (activePanel != null)
                        {
                            var instrumentProp = activePanel.GetType().GetProperty("Instrument");
                            if (instrumentProp != null)
                            {
                                var inst = instrumentProp.GetValue(activePanel) as Instrument;
                                if (inst != null) return inst;
                            }
                        }
                    }
                }
            }
            catch { }
            
            // All methods failed, return null
            return null;
        }

        private void OnAccountSelectionChanged(object sender, EventArgs e)
        {
            UnsubscribeFromAccount();
            if (accountSelector != null)
                selectedAccount = accountSelector.SelectedAccount;
            SubscribeToAccount();
        }

        private void SubscribeToAccount()
        {
            if (selectedAccount != null)
            {
                selectedAccount.PositionUpdate += OnPositionUpdate;
                selectedAccount.OrderUpdate += OnOrderUpdate;
                selectedAccount.AccountItemUpdate += OnAccountItemUpdate;
                // Initial update
                if (ChartControl != null)
                    ChartControl.Dispatcher.InvokeAsync(() => UpdateAccountInfo());
            }
        }

        private void UnsubscribeFromAccount()
        {
            if (selectedAccount != null)
            {
                selectedAccount.PositionUpdate -= OnPositionUpdate;
                selectedAccount.OrderUpdate -= OnOrderUpdate;
                selectedAccount.AccountItemUpdate -= OnAccountItemUpdate;
            }
        }

        private void OnPositionUpdate(object sender, PositionEventArgs e)
        {
            // We may be controlling a different instrument than the chart this script is attached to,
            // so always re-check automation on any position update.
            if (ChartControl != null)
                ChartControl.Dispatcher.InvokeAsync(() => CheckAutomation());
        }

        private void OnOrderUpdate(object sender, OrderEventArgs e)
        {
            // Order changed
        }

        private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)
        {
            // Update account info display when account items change
            if (ChartControl != null)
                ChartControl.Dispatcher.InvokeAsync(() => UpdateAccountInfo());
        }

        private void UpdateAccountInfo()
        {
            if (selectedAccount == null || btnAccountBalance == null) return;

            try
            {
                // Get account values using GetAccountItem
                double balance = selectedAccount.GetAccountItem(AccountItem.CashValue, Currency.UsDollar).Value;
                double unrealizedPnL = selectedAccount.GetAccountItem(AccountItem.UnrealizedProfitLoss, Currency.UsDollar).Value;
                double realizedPnL = selectedAccount.GetAccountItem(AccountItem.RealizedProfitLoss, Currency.UsDollar).Value;

                // Calculate equity: balance + unrealized PnL
                double equity = balance + unrealizedPnL;

                // Update display - Button Content
                btnAccountBalance.Content = "$" + balance.ToString("N2");
                btnAccountEquity.Content = "$" + equity.ToString("N2");

                // Daily PnL with sign and color
                TextBlock pnlText = new TextBlock();
                if (realizedPnL > 0)
                {
                    pnlText.Text = "+$" + realizedPnL.ToString("N2");
                    pnlText.Foreground = Brushes.Green;
                }
                else if (realizedPnL < 0)
                {
                    pnlText.Text = "-$" + Math.Abs(realizedPnL).ToString("N2");
                    pnlText.Foreground = Brushes.Red;
                }
                else
                {
                    pnlText.Text = "$0.00";
                    pnlText.Foreground = Brushes.Black;
                }
                pnlText.HorizontalAlignment = HorizontalAlignment.Center;
                pnlText.VerticalAlignment = VerticalAlignment.Center;
                btnDailyPnL.Content = pnlText;
            }
            catch (Exception ex)
            {
                Print("[TradingPanel_EN] Error updating account info: " + ex.Message);
            }
        }

        #endregion

        #region Button Click Handlers

        private void OnCloseHalfClick(object sender, RoutedEventArgs e)
        {
            ClosePartialPosition(0.5);
        }

        private void OnCloseThirdClick(object sender, RoutedEventArgs e)
        {
            ClosePartialPosition(1.0 / 3.0);
        }

        private void OnCloseAllClick(object sender, RoutedEventArgs e)
        {
            CloseAllPositions();
        }

        private void OnClosePositionClick(object sender, RoutedEventArgs e)
        {
            CloseOnlyPositions();
        }

        private void OnCancelWorkingOrdersClick(object sender, RoutedEventArgs e)
        {
            CancelWorkingOrders();
        }

        private void OnProfitCloseToggle(object sender, RoutedEventArgs e)
        {
            isProfitCloseEnabled = !isProfitCloseEnabled;
            UpdateToggleButtonState(btnProfitClose, isProfitCloseEnabled);
            Print("[TradingPanel_EN] Profit close: " + (isProfitCloseEnabled ? "ON" : "OFF"));
        }

        private void OnLossCloseToggle(object sender, RoutedEventArgs e)
        {
            isLossCloseEnabled = !isLossCloseEnabled;
            UpdateToggleButtonState(btnLossClose, isLossCloseEnabled);
            Print("[TradingPanel_EN] Loss close: " + (isLossCloseEnabled ? "ON" : "OFF"));
        }

        private void UpdateToggleButtonState(Button btn, bool isEnabled)
        {
            if (btn == null) return;
            btn.Background = isEnabled ? Brushes.LimeGreen : Brushes.LightGray;
            btn.Foreground = isEnabled ? Brushes.White : Brushes.Black;
        }

        #endregion

        #region TextBox Change Handlers

        private void OnProfitAmountChanged(string newValue)
        {
            double val;
            if (double.TryParse(newValue, out val) && val > 0)
            {
                ProfitCloseAmount = val;
                txtProfitAmount.Text = val.ToString();
            }
            else
            {
                txtProfitAmount.Text = ProfitCloseAmount.ToString();
            }
        }

        private void OnLossAmountChanged(string newValue)
        {
            double val;
            if (double.TryParse(newValue, out val) && val > 0)
            {
                LossCloseAmount = val;
                txtLossAmount.Text = val.ToString();
            }
            else
            {
                txtLossAmount.Text = LossCloseAmount.ToString();
            }
        }

        #endregion

        #region Trading Operations

        private Position GetCurrentPosition()
        {
            if (selectedAccount == null) return null;
            
            Instrument activeInstrument = GetActiveInstrument();
            if (activeInstrument == null) return null;

            foreach (Position pos in selectedAccount.Positions)
            {
                if (pos.Instrument == activeInstrument && pos.MarketPosition != MarketPosition.Flat)
                    return pos;
            }
            return null;
        }

        private void CloseAllPositions()
        {
            if (selectedAccount == null) return;

            Position position = GetCurrentPosition();
            if (position == null) return;

            try
            {
                Instrument activeInstrument = GetActiveInstrument();
                if (activeInstrument == null) return;
                
                selectedAccount.Flatten(new[] { activeInstrument });
                Print("[TradingPanel_EN] Closed all positions");
            }
            catch (Exception ex)
            {
                Print("[TradingPanel_EN] Error closing all: " + ex.Message);
            }
        }

        private void CloseOnlyPositions()
        {
            if (selectedAccount == null) return;

            Position position = GetCurrentPosition();
            if (position == null) return;

            try
            {
                int closeQty = position.Quantity;
                if (closeQty < 1) return;

                OrderAction action = position.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

                Instrument activeInstrument = GetActiveInstrument();
                if (activeInstrument == null) return;

                Order order = selectedAccount.CreateOrder(
                    activeInstrument,
                    action,
                    OrderType.Market,
                    OrderEntry.Manual,
                    TimeInForce.Day,
                    closeQty,
                    0, 0,
                    string.Empty,
                    "TP_EN_ClosePos_" + DateTime.Now.Ticks.ToString(),
                    Core.Globals.MaxDate,
                    null
                );

                selectedAccount.Submit(new[] { order });
                Print("[TradingPanel_EN] Closed position only: " + closeQty.ToString() + " contracts");
            }
            catch (Exception ex)
            {
                Print("[TradingPanel_EN] Error closing position only: " + ex.Message);
            }
        }

        private void ClosePartialPosition(double fraction)
        {
            if (selectedAccount == null) return;

            Position position = GetCurrentPosition();
            if (position == null) return;

            int closeQty = (int)Math.Floor(position.Quantity * fraction);
            if (closeQty < 1) return;

            try
            {
                OrderAction action = position.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

                Instrument activeInstrument = GetActiveInstrument();
                if (activeInstrument == null) return;

                Order order = selectedAccount.CreateOrder(
                    activeInstrument,
                    action,
                    OrderType.Market,
                    OrderEntry.Manual,
                    TimeInForce.Day,
                    closeQty,
                    0, 0,
                    string.Empty,
                    "TP_EN_Partial_" + DateTime.Now.Ticks.ToString(),
                    Core.Globals.MaxDate,
                    null
                );

                selectedAccount.Submit(new[] { order });
                Print("[TradingPanel_EN] Partial close: " + closeQty.ToString() + " contracts (" + (fraction * 100).ToString("0") + "%)");
            }
            catch (Exception ex)
            {
                Print("[TradingPanel_EN] Error partial close: " + ex.Message);
            }
        }

        private void ModifyStopLoss(double newSlPrice)
        {
            if (selectedAccount == null) return;
            
            Instrument activeInstrument = GetActiveInstrument();
            if (activeInstrument == null) return;

            foreach (Order order in selectedAccount.Orders)
            {
                if (order.Instrument == activeInstrument &&
                    order.OrderState == OrderState.Working &&
                    order.OrderType == OrderType.StopMarket)
                {
                    try
                    {
                        // Cancel existing order and create new one with updated stop price
                        selectedAccount.Cancel(new[] { order });

                        Position position = GetCurrentPosition();
                        if (position == null) return;

                        OrderAction action = position.MarketPosition == MarketPosition.Long ? OrderAction.Sell : OrderAction.BuyToCover;

                        Order newOrder = selectedAccount.CreateOrder(
                            activeInstrument,
                            action,
                            OrderType.StopMarket,
                            OrderEntry.Manual,
                            TimeInForce.Gtc,
                            order.Quantity,
                            0,
                            newSlPrice,
                            string.Empty,
                            "TP_EN_SL_" + DateTime.Now.Ticks.ToString(),
                            Core.Globals.MaxDate,
                            null
                        );

                        selectedAccount.Submit(new[] { newOrder });
                        Print("[TradingPanel_EN] Modified SL to " + newSlPrice.ToString());
                    }
                    catch (Exception ex)
                    {
                        Print("[TradingPanel_EN] Error modifying SL: " + ex.Message);
                    }
                }
            }
        }

        private void CancelWorkingOrders()
        {
            if (selectedAccount == null) return;

            try
            {
                Instrument activeInstrument = GetActiveInstrument();
                if (activeInstrument == null) return;
                
                List<Order> workingOrders = new List<Order>();

                foreach (Order order in selectedAccount.Orders)
                {
                    if (order.Instrument == activeInstrument &&
                        order.OrderState == OrderState.Working)
                    {
                        workingOrders.Add(order);
                    }
                }

                if (workingOrders.Count > 0)
                {
                    selectedAccount.Cancel(workingOrders.ToArray());
                    Print("[TradingPanel_EN] Canceled " + workingOrders.Count.ToString() + " working orders");
                }
            }
            catch (Exception ex)
            {
                Print("[TradingPanel_EN] Error canceling working orders: " + ex.Message);
            }
        }

        #endregion

        #region Automation Logic

        private void CheckAutomation()
        {
            Position position = GetCurrentPosition();
            if (position == null) return;

            double pnl = position.GetUnrealizedProfitLoss(PerformanceUnit.Currency);

            // Check profit close
            if (isProfitCloseEnabled && pnl >= ProfitCloseAmount)
            {
                CloseAllPositions();
                Print("[TradingPanel_EN] Auto profit close triggered: " + pnl.ToString("C2"));
                return;
            }

            // Check loss close
            if (isLossCloseEnabled && pnl <= -LossCloseAmount)
            {
                CloseAllPositions();
                Print("[TradingPanel_EN] Auto loss close triggered: " + pnl.ToString("C2"));
                return;
            }
        }

        #endregion

        protected override void OnBarUpdate()
        {
            if (State != State.Realtime) return;

            if (ChartControl != null)
                ChartControl.Dispatcher.InvokeAsync(() => CheckAutomation());
        }

        protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
        {
            if (State != State.Realtime) return;
            if (marketDataUpdate.MarketDataType != MarketDataType.Last) return;

            if (ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() =>
                {
                    CheckAutomation();
                    UpdateAccountInfo(); // Update account info in real-time
                });
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(1, double.MaxValue)]
        [Display(Name = "Profit Close Amount", Order = 1, GroupName = "1. Risk Settings")]
        public double ProfitCloseAmount { get; set; }

        [NinjaScriptProperty]
        [Range(1, double.MaxValue)]
        [Display(Name = "Loss Close Amount", Order = 2, GroupName = "1. Risk Settings")]
        public double LossCloseAmount { get; set; }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradingPanel_EN[] cacheTradingPanel_EN;
		public TradingPanel_EN TradingPanel_EN(double profitCloseAmount, double lossCloseAmount)
		{
			return TradingPanel_EN(Input, profitCloseAmount, lossCloseAmount);
		}

		public TradingPanel_EN TradingPanel_EN(ISeries<double> input, double profitCloseAmount, double lossCloseAmount)
		{
			if (cacheTradingPanel_EN != null)
				for (int idx = 0; idx < cacheTradingPanel_EN.Length; idx++)
					if (cacheTradingPanel_EN[idx] != null && cacheTradingPanel_EN[idx].ProfitCloseAmount == profitCloseAmount && cacheTradingPanel_EN[idx].LossCloseAmount == lossCloseAmount && cacheTradingPanel_EN[idx].EqualsInput(input))
						return cacheTradingPanel_EN[idx];
			return CacheIndicator<TradingPanel_EN>(new TradingPanel_EN(){ ProfitCloseAmount = profitCloseAmount, LossCloseAmount = lossCloseAmount }, input, ref cacheTradingPanel_EN);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradingPanel_EN TradingPanel_EN(double profitCloseAmount, double lossCloseAmount)
		{
			return indicator.TradingPanel_EN(Input, profitCloseAmount, lossCloseAmount);
		}

		public Indicators.TradingPanel_EN TradingPanel_EN(ISeries<double> input , double profitCloseAmount, double lossCloseAmount)
		{
			return indicator.TradingPanel_EN(input, profitCloseAmount, lossCloseAmount);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradingPanel_EN TradingPanel_EN(double profitCloseAmount, double lossCloseAmount)
		{
			return indicator.TradingPanel_EN(Input, profitCloseAmount, lossCloseAmount);
		}

		public Indicators.TradingPanel_EN TradingPanel_EN(ISeries<double> input , double profitCloseAmount, double lossCloseAmount)
		{
			return indicator.TradingPanel_EN(input, profitCloseAmount, lossCloseAmount);
		}
	}
}

#endregion
