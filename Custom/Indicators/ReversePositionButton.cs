#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class ReversePositionButton : Indicator
    {
        private System.Windows.Controls.RowDefinition addedRow;
        private Gui.Chart.ChartTab chartTab;
        private Gui.Chart.Chart chartWindow;
        private System.Windows.Controls.Grid chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid;
        private System.Windows.Controls.Button reverseButton;
        private bool panelActive;
        private System.Windows.Controls.TabItem tabItem;
        
        private Order closeOrder = null;
        private bool isProcessing = false;
        private Account currentAccount;
        
        // Store these explicitly to avoid any timing issues
        private bool shouldBuyOnReverse = false;
        private bool shouldSellOnReverse = false;
        
        static int totalGrids;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Reverse button in Chart Trader";
                Name = "ReversePositionButton";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = false;
                DrawOnPricePanel = true;
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

        protected void CreateWPFControls()
        {
            chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
            
            if (chartWindow == null)
            {
                Print("Error: chartWindow is null");
                return;
            }

            chartTraderGrid = (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;
            chartTraderButtonsGrid = chartTraderGrid.Children[0] as System.Windows.Controls.Grid;

            lowerButtonsGrid = new System.Windows.Controls.Grid();
            System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid, 1);
            lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
            
            addedRow = new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
            
            Style basicButtonStyle = Application.Current.FindResource("BasicEntryButton") as Style;

            reverseButton = new System.Windows.Controls.Button()
            {
                Content = "REVERSE",
                Height = 30,
                Margin = new Thickness(5, 0, 5, 0),
                Padding = new Thickness(0, 0, 0, 0),
                Style = basicButtonStyle
            };

            reverseButton.Background = new SolidColorBrush(Color.FromRgb(255, 140, 0));
            reverseButton.BorderBrush = Brushes.Black;
            reverseButton.Foreground = Brushes.White;
            reverseButton.BorderThickness = new Thickness(2.0);

            reverseButton.Click += OnReverseButtonClick;

            System.Windows.Controls.Grid.SetColumn(reverseButton, 0);
            System.Windows.Controls.Grid.SetRow(reverseButton, 0);

            lowerButtonsGrid.Children.Add(reverseButton);

            if (totalGrids == 0)
                totalGrids = chartTraderGrid.RowDefinitions.Count;

            if (TabSelected())
                InsertWPFControls();

            chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
        }

        public void DisposeWPFControls()
        {
            if (chartWindow != null)
                chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

            if (reverseButton != null)
                reverseButton.Click -= OnReverseButtonClick;

            RemoveWPFControls();
        }

        public void InsertWPFControls()
        {
            if (panelActive)
                return;

            chartTraderGrid.RowDefinitions.Add(addedRow);
            System.Windows.Controls.Grid.SetRow(lowerButtonsGrid, totalGrids);
            chartTraderGrid.Children.Add(lowerButtonsGrid);

            panelActive = true;
        }

        private bool TabSelected()
        {
            if (ChartControl == null || chartWindow == null || chartWindow.MainTabControl == null)
                return false;

            bool tabSelected = false;

            foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
                if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
                    tabSelected = true;

            return tabSelected;
        }

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

        private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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

                    // Cancel all working orders (old ATM brackets)
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

                    // Determine reverse direction BEFORE submitting close order
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

        protected override void OnBarUpdate()
        {
        }
    }
}


#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ReversePositionButton[] cacheReversePositionButton;
		public ReversePositionButton ReversePositionButton()
		{
			return ReversePositionButton(Input);
		}

		public ReversePositionButton ReversePositionButton(ISeries<double> input)
		{
			if (cacheReversePositionButton != null)
				for (int idx = 0; idx < cacheReversePositionButton.Length; idx++)
					if (cacheReversePositionButton[idx] != null &&  cacheReversePositionButton[idx].EqualsInput(input))
						return cacheReversePositionButton[idx];
			return CacheIndicator<ReversePositionButton>(new ReversePositionButton(), input, ref cacheReversePositionButton);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ReversePositionButton ReversePositionButton()
		{
			return indicator.ReversePositionButton(Input);
		}

		public Indicators.ReversePositionButton ReversePositionButton(ISeries<double> input )
		{
			return indicator.ReversePositionButton(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ReversePositionButton ReversePositionButton()
		{
			return indicator.ReversePositionButton(Input);
		}

		public Indicators.ReversePositionButton ReversePositionButton(ISeries<double> input )
		{
			return indicator.ReversePositionButton(input);
		}
	}
}

#endregion
