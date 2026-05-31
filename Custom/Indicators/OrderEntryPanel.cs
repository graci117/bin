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
using System.Windows.Controls.Primitives;
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
	public class OrderEntryPanel : Indicator
	{
		private System.Windows.Controls.Canvas containerCanvas;
		private System.Windows.Controls.Grid buttonPanel;
		private System.Windows.Controls.Border dragHandle;
		private System.Windows.Controls.TextBlock handleText;
		private System.Windows.Controls.Button buyChartButton;
		private System.Windows.Controls.Button sellChartButton;
		private System.Windows.Controls.Button buyUpButton;
		private System.Windows.Controls.Button sellDownButton;
		private System.Windows.Controls.Button buyDownButton;
		private System.Windows.Controls.Button sellUpButton;
		private System.Windows.Controls.Button buyMarketButton;
		private System.Windows.Controls.Button sellMarketButton;
		private System.Windows.Controls.TextBox offsetTextBox;
		private System.Windows.Controls.Button offsetUpButton;
		private System.Windows.Controls.Button offsetDownButton;
		private System.Windows.Controls.TextBlock offsetLabel;
		
		private NinjaTrader.Gui.Tools.AccountSelector xAlselector;
		private NinjaTrader.Gui.Tools.InstrumentSelector xInSelector;
		private ChartScale chartScale;
		
		private bool isCollapsed = false;
		private Point dragStartPoint;
		private bool isDragging = false;
		
		// Chart click mode tracking
		private bool buyChartMode = false;
		private bool sellChartMode = false;
		
		// Directional order tracking
		private bool buyUpActive = false;
		private bool sellDownActive = false;
		private bool buyDownActive = false;
		private bool sellUpActive = false;
		private double buyUpTriggerPrice = 0;
		private double sellDownTriggerPrice = 0;
		private double buyDownTriggerPrice = 0;
		private double sellUpTriggerPrice = 0;
		
		// Order tracking for cancellation
		private Order buyUpOrder = null;
		private Order sellDownOrder = null;
		private Order buyDownOrder = null;
		private Order sellUpOrder = null;
		private int buyUpOrderBar = -1;
		private int sellDownOrderBar = -1;
		private int buyDownOrderBar = -1;
		private int sellUpOrderBar = -1;
		private string atmStrategyTemplate = string.Empty;
		
		private int buyUpActivatedBar = -1;
		private int sellDownActivatedBar = -1;
		private int buyDownActivatedBar = -1;
		private int sellUpActivatedBar = -1;
		//private Order buyUpOrder = null;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Advanced order entry panel with chart click and directional entries";
				Name = "OrderEntryPanel";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = false;
				PaintPriceMarkers = false;
				IsSuspendedWhileInactive = false;
				
				// Matching colors for buy/sell groups
				BuyButtonColor = Brushes.DodgerBlue;
				SellButtonColor = Brushes.IndianRed;
				ButtonTextColor = Brushes.White;
				HandleColor = Brushes.Purple;
				DefaultOffset = 0;
				AtmStrategyTemplate = string.Empty;
				UseAtmForChartOrders = true;
				//AtmStrategyTemplate = string.Empty;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						CreateButtonPanel();
					});
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						RemoveButtonPanel();
					});
				}
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			this.chartScale = chartScale;
			base.OnRender(chartControl, chartScale);
		}

		private void CreateButtonPanel()
		{
			if (UserControlCollection.Contains(containerCanvas))
				return;

			containerCanvas = new System.Windows.Controls.Canvas
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Background = Brushes.Transparent
			};

			buttonPanel = new System.Windows.Controls.Grid
			{
				Name = "OrderEntryPanel",
				Background = new SolidColorBrush(Color.FromArgb(200, 40, 40, 40))
			};

			buttonPanel.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			buttonPanel.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			for (int i = 0; i < 5; i++)
				buttonPanel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());

			// Row 0: Buy Chart | Sell Chart
			buyChartButton = CreateButton("Buy Chart\n(Shift+Click)", BuyButtonColor, 35);
			System.Windows.Controls.Grid.SetRow(buyChartButton, 0);
			System.Windows.Controls.Grid.SetColumn(buyChartButton, 0);
			buttonPanel.Children.Add(buyChartButton);
			buyChartButton.Click += BuyChartClick;

			sellChartButton = CreateButton("Sell Chart\n(Alt+Click)", SellButtonColor, 35);
			System.Windows.Controls.Grid.SetRow(sellChartButton, 0);
			System.Windows.Controls.Grid.SetColumn(sellChartButton, 1);
			buttonPanel.Children.Add(sellChartButton);
			sellChartButton.Click += SellChartClick;

			// Row 1: Buy Up | Sell Down
			buyUpButton = CreateButton("Buy Up", BuyButtonColor, 30);
			System.Windows.Controls.Grid.SetRow(buyUpButton, 1);
			System.Windows.Controls.Grid.SetColumn(buyUpButton, 0);
			buttonPanel.Children.Add(buyUpButton);
			buyUpButton.Click += BuyUpClick;

			sellDownButton = CreateButton("Sell Down", SellButtonColor, 30);
			System.Windows.Controls.Grid.SetRow(sellDownButton, 1);
			System.Windows.Controls.Grid.SetColumn(sellDownButton, 1);
			buttonPanel.Children.Add(sellDownButton);
			sellDownButton.Click += SellDownClick;

			// Row 2: Buy Down | Sell Up
			buyDownButton = CreateButton("Buy Down", BuyButtonColor, 30);
			System.Windows.Controls.Grid.SetRow(buyDownButton, 2);
			System.Windows.Controls.Grid.SetColumn(buyDownButton, 0);
			buttonPanel.Children.Add(buyDownButton);
			buyDownButton.Click += BuyDownClick;

			sellUpButton = CreateButton("Sell Up", SellButtonColor, 30);
			System.Windows.Controls.Grid.SetRow(sellUpButton, 2);
			System.Windows.Controls.Grid.SetColumn(sellUpButton, 1);
			buttonPanel.Children.Add(sellUpButton);
			sellUpButton.Click += SellUpClick;
			
			// Row 3: Buy Market | Sell Market
			buyMarketButton = CreateButton("Buy Market", BuyButtonColor, 30);
			System.Windows.Controls.Grid.SetRow(buyMarketButton, 3);
			System.Windows.Controls.Grid.SetColumn(buyMarketButton, 0);
			buttonPanel.Children.Add(buyMarketButton);
			buyMarketButton.Click += BuyMarketClick;
			
			sellMarketButton = CreateButton("Sell Market", SellButtonColor, 30);
			System.Windows.Controls.Grid.SetRow(sellMarketButton, 3);
			System.Windows.Controls.Grid.SetColumn(sellMarketButton, 1);
			buttonPanel.Children.Add(sellMarketButton);
			sellMarketButton.Click += SellMarketClick;

			// Row 4: Offset control
			CreateOffsetControl();

			// Create draggable handle
			CreateDragHandle();

			UserControlCollection.Add(containerCanvas);
			
			if (ChartPanel != null)
			{
				ChartPanel.MouseLeftButtonDown += OnChartMouseDown;
			}
		}

		private System.Windows.Controls.Button CreateButton(string content, Brush color, double height)
		{
			return new System.Windows.Controls.Button
			{
				Content = content,
				Height = height,
				Margin = new Thickness(0),
				Padding = new Thickness(5, 0, 5, 0),
				Background = color,
				Foreground = ButtonTextColor,
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1),
				FontWeight = FontWeights.Bold,
				FontSize = height > 30 ? 10 : 11,
				Opacity = 1.0
			};
		}

		private void CreateOffsetControl()
		{
			System.Windows.Controls.Grid offsetGrid = new System.Windows.Controls.Grid();
			offsetGrid.Background = Brushes.HotPink;
			offsetGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(50) });
			offsetGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			offsetGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(20) });

			offsetLabel = new System.Windows.Controls.TextBlock
			{
				Text = "Offset:",
				Foreground = Brushes.White,
				FontWeight = FontWeights.Bold,
				FontSize = 11,
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Right,
				Margin = new Thickness(0, 0, 5, 0)
			};
			System.Windows.Controls.Grid.SetColumn(offsetLabel, 0);
			offsetGrid.Children.Add(offsetLabel);

			offsetTextBox = new System.Windows.Controls.TextBox
			{
				Text = DefaultOffset.ToString(),
				Height = 25,
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				FontSize = 11,
				FontWeight = FontWeights.Bold,
				Background = new SolidColorBrush(Color.FromArgb(200, 40, 40, 40)),
				Foreground = Brushes.White,
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1),
				Margin = new Thickness(0, 2, 0, 2)
			};
			System.Windows.Controls.Grid.SetColumn(offsetTextBox, 1);
			offsetGrid.Children.Add(offsetTextBox);

			System.Windows.Controls.StackPanel upDownStack = new System.Windows.Controls.StackPanel
			{
				Orientation = System.Windows.Controls.Orientation.Vertical,
				VerticalAlignment = VerticalAlignment.Center
			};

			offsetUpButton = new System.Windows.Controls.Button
			{
				//Content = "▲",
				Content = "U",
				Height = 12,
				Width = 20,
				Padding = new Thickness(0),
				Margin = new Thickness(0),
				FontSize = 6,
				Background = Brushes.DarkGray,
				Foreground = Brushes.White,
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1)
			};
			offsetUpButton.Click += (s, e) => AdjustOffset(1);
			upDownStack.Children.Add(offsetUpButton);

			offsetDownButton = new System.Windows.Controls.Button
			{
				Content = "▼",
				Height = 12,
				Width = 20,
				Padding = new Thickness(0),
				Margin = new Thickness(0, 1, 0, 0),
				FontSize = 6,
				Background = Brushes.DarkGray,
				Foreground = Brushes.White,
				BorderBrush = Brushes.Gray,
				BorderThickness = new Thickness(1)
			};
			offsetDownButton.Click += (s, e) => AdjustOffset(-1);
			upDownStack.Children.Add(offsetDownButton);

			System.Windows.Controls.Grid.SetColumn(upDownStack, 2);
			offsetGrid.Children.Add(upDownStack);

			System.Windows.Controls.Grid.SetRow(offsetGrid, 4);
			System.Windows.Controls.Grid.SetColumn(offsetGrid, 0);
			System.Windows.Controls.Grid.SetColumnSpan(offsetGrid, 2);
			buttonPanel.Children.Add(offsetGrid);
		}

		private void CreateDragHandle()
		{
			dragHandle = new System.Windows.Controls.Border
			{
				Height = 20,
				Background = HandleColor,
				BorderBrush = Brushes.DarkGray,
				BorderThickness = new Thickness(1),
				Cursor = Cursors.SizeAll,
				Opacity = 0.8
			};

			handleText = new System.Windows.Controls.TextBlock
			{
				Text = "≡≡≡",
				Foreground = Brushes.White,
				FontWeight = FontWeights.Bold,
				FontSize = 14,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				TextAlignment = TextAlignment.Center
			};

			dragHandle.Child = handleText;
			dragHandle.MouseLeftButtonDown += OnHandleMouseDown;
			dragHandle.MouseMove += OnHandleMouseMove;
			dragHandle.MouseLeftButtonUp += OnHandleMouseUp;

			System.Windows.Controls.Canvas.SetLeft(dragHandle, 0);
			System.Windows.Controls.Canvas.SetTop(dragHandle, 0);
			System.Windows.Controls.Canvas.SetLeft(buttonPanel, 0);
			System.Windows.Controls.Canvas.SetTop(buttonPanel, 20);

			containerCanvas.Children.Add(dragHandle);
			containerCanvas.Children.Add(buttonPanel);

			buttonPanel.Loaded += (s, e) => { dragHandle.Width = buttonPanel.ActualWidth; };
			buttonPanel.SizeChanged += (s, e) => { dragHandle.Width = buttonPanel.ActualWidth; };

			containerCanvas.Margin = new Thickness(10, 10, 0, 0);
		}

		private void AdjustOffset(int increment)
		{
			if (int.TryParse(offsetTextBox.Text, out int currentValue))
			{
				offsetTextBox.Text = (currentValue + increment).ToString();
			}
		}

		private void OnHandleMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				isCollapsed = !isCollapsed;
				if (isCollapsed)
				{
					buttonPanel.Visibility = Visibility.Collapsed;
					dragHandle.Opacity = 0.5;
					handleText.Text = "Order Entry";
				}
				else
				{
					buttonPanel.Visibility = Visibility.Visible;
					dragHandle.Opacity = 0.8;
					handleText.Text = "≡≡≡";
				}
			}
			else if (e.ClickCount == 1)
			{
				isDragging = true;
				dragStartPoint = e.GetPosition(null);
				dragHandle.CaptureMouse();
			}
		}

		private void OnHandleMouseMove(object sender, MouseEventArgs e)
		{
			if (isDragging && e.LeftButton == MouseButtonState.Pressed)
			{
				Point currentPosition = e.GetPosition(null);
				Vector offset = currentPosition - dragStartPoint;

				Thickness margin = containerCanvas.Margin;
				margin.Left += offset.X;
				margin.Top += offset.Y;

				if (margin.Left < 0) margin.Left = 0;
				if (margin.Top < 0) margin.Top = 0;

				containerCanvas.Margin = margin;
				dragStartPoint = currentPosition;
			}
		}

		private void OnHandleMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (isDragging)
			{
				isDragging = false;
				dragHandle.ReleaseMouseCapture();
			}
		}

		private void BuyChartClick(object sender, RoutedEventArgs e)
		{
			CancelAllDirectionalModes();
			
			buyChartMode = !buyChartMode;
			sellChartMode = false;
			
			UpdateButtonStates();
			
			
		}

		private void SellChartClick(object sender, RoutedEventArgs e)
		{
			CancelAllDirectionalModes();
			
			sellChartMode = !sellChartMode;
			buyChartMode = false;
			
			UpdateButtonStates();
			
			
		}

		private void BuyUpClick(object sender, RoutedEventArgs e)
		{
			if (buyDownActive || sellUpActive)
			{
				//Print("Cannot activate Buy Up while Buy Down or Sell Up are active");
				return;
			}
			
			CancelChartModes();
			
			buyUpActive = !buyUpActive;
			
			if (buyUpActive)
			{
				buyUpActivatedBar = CurrentBar;
				//Print($"Buy Up activated at bar {CurrentBar} - Waiting for GREEN bar (Close > Open)");
			}
			
			
			UpdateButtonStates();
		}
		
		private void SellDownClick(object sender, RoutedEventArgs e)
		{
			if (buyDownActive || sellUpActive)
			{
				//Print("Cannot activate Sell Down while Buy Down or Sell Up are active");
				return;
			}
			
			CancelChartModes();
			
			sellDownActive = !sellDownActive;
			
			if (sellDownActive)
			{
				sellDownActivatedBar = CurrentBar;
				//Print($"Sell Down activated at bar {CurrentBar} - Waiting for RED bar (Close < Open)");
			}
			
			
			UpdateButtonStates();
		}
		
		private void BuyDownClick(object sender, RoutedEventArgs e)
		{
			if (buyUpActive || sellDownActive)
			{
				//Print("Cannot activate Buy Down while Buy Up or Sell Down are active");
				return;
			}
			
			CancelChartModes();
			
			buyDownActive = !buyDownActive;
			
			if (buyDownActive)
			{
				buyDownActivatedBar = CurrentBar;
				//Print($"Buy Down activated at bar {CurrentBar} - Waiting for RED bar (Close < Open)");
			}
			
			
			UpdateButtonStates();
		}
		
		private void SellUpClick(object sender, RoutedEventArgs e)
		{
			if (buyUpActive || sellDownActive)
			{
				//Print("Cannot activate Sell Up while Buy Up or Sell Down are active");
				return;
			}
			
			CancelChartModes();
			
			sellUpActive = !sellUpActive;
			
			if (sellUpActive)
			{
				sellUpActivatedBar = CurrentBar;
				//Print($"Sell Up activated at bar {CurrentBar} - Waiting for GREEN bar (Close > Open)");
			}
			
			
			UpdateButtonStates();
		}
		
		private void BuyMarketClick(object sender, RoutedEventArgs e)
		{
			try
			{
			Print("buy click");
				double currentAsk = GetCurrentAsk();
				int offset = GetOffsetTicks();
				double orderPrice = currentAsk - (offset * TickSize);
				
				PlaceOrder(OrderAction.Buy, orderPrice);
				//Print($"Buy Market clicked: Placed limit order at {orderPrice} (Ask {currentAsk} - {offset} ticks)");
			}
			catch (Exception ex)
			{
				Print($"Error in BuyMarketClick: {ex.Message}");
			}
		}
		
		private void SellMarketClick(object sender, RoutedEventArgs e)
		{
			try
			{
				double currentBid = GetCurrentBid();
				int offset = GetOffsetTicks();
				double orderPrice = currentBid + (offset * TickSize);
				
				PlaceOrder(OrderAction.Sell, orderPrice);
				//Print($"Sell Market clicked: Placed limit order at {orderPrice} (Bid {currentBid} + {offset} ticks)");
			}
			catch (Exception ex)
			{
				Print($"Error in SellMarketClick: {ex.Message}");
			}
		}




		private void CancelChartModes()
		{
			buyChartMode = false;
			sellChartMode = false;
		}

		private void CancelAllDirectionalModes()
		{
			buyUpActive = false;
			sellDownActive = false;
			buyDownActive = false;
			sellUpActive = false;
			
			CancelPendingOrders();
		}

		private void CancelPendingOrders()
		{
			ChartControl.Dispatcher.InvokeAsync(() =>
			{
				try
				{
					var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
					if (chartWindow == null) return;
					
					xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
					if (xAlselector == null) return;
					
					Account account = xAlselector.SelectedAccount;
					if (account == null) return;

					List<Order> ordersToCancel = new List<Order>();
					
					if (buyUpOrder != null) ordersToCancel.Add(buyUpOrder);
					if (sellDownOrder != null) ordersToCancel.Add(sellDownOrder);
					if (buyDownOrder != null) ordersToCancel.Add(buyDownOrder);
					if (sellUpOrder != null) ordersToCancel.Add(sellUpOrder);
					
					if (ordersToCancel.Count > 0)
					{
						account.Cancel(ordersToCancel.ToArray());
						//Print($"Cancelled {ordersToCancel.Count} pending orders");
					}
					
					buyUpOrder = null;
					sellDownOrder = null;
					buyDownOrder = null;
					sellUpOrder = null;
				}
				catch (Exception ex)
				{
					Print($"Error cancelling orders: {ex.Message}");
				}
			});
		}

		private void UpdateButtonStates()
		{
			if (ChartControl == null) return;
			
			ChartControl.Dispatcher.InvokeAsync(() =>
			{
				// Buy Chart Button
				if (buyChartMode)
				{
					buyChartButton.Content = "Buy Chart ✓\n(Shift+Click)";
					buyChartButton.Opacity = 0.6;
				}
				else
				{
					buyChartButton.Content = "Buy Chart\n(Shift+Click)";
					buyChartButton.Opacity = 1.0;
				}
				
				// Sell Chart Button
				if (sellChartMode)
				{
					sellChartButton.Content = "Sell Chart ✓\n(Alt+Click)";
					sellChartButton.Opacity = 0.6;
				}
				else
				{
					sellChartButton.Content = "Sell Chart\n(Alt+Click)";
					sellChartButton.Opacity = 1.0;
				}
				
				// Buy Up Button
				if (buyUpActive)
				{
					buyUpButton.Content = "Buy Up ✓";
					buyUpButton.Opacity = 0.6;
					buyDownButton.IsEnabled = false;  // Disable conflicting
					buyDownButton.Opacity = 0.3;
					sellUpButton.IsEnabled = false;   // Disable conflicting
					sellUpButton.Opacity = 0.3;
				}
				else if (!buyDownActive && !sellUpActive)
				{
					buyUpButton.Content = "Buy Up";
					buyUpButton.Opacity = 1.0;
					buyUpButton.IsEnabled = true;
				}
				
				// Sell Down Button
				if (sellDownActive)
				{
					sellDownButton.Content = "Sell Down ✓";
					sellDownButton.Opacity = 0.6;
					buyDownButton.IsEnabled = false;  // Disable conflicting
					buyDownButton.Opacity = 0.3;
					sellUpButton.IsEnabled = false;   // Disable conflicting
					sellUpButton.Opacity = 0.3;
				}
				else if (!buyDownActive && !sellUpActive)
				{
					sellDownButton.Content = "Sell Down";
					sellDownButton.Opacity = 1.0;
					sellDownButton.IsEnabled = true;
				}
				
				// Buy Down Button
				if (buyDownActive)
				{
					buyDownButton.Content = "Buy Down ✓";
					buyDownButton.Opacity = 0.6;
					buyUpButton.IsEnabled = false;    // Disable conflicting
					buyUpButton.Opacity = 0.3;
					sellDownButton.IsEnabled = false; // Disable conflicting
					sellDownButton.Opacity = 0.3;
				}
				else if (!buyUpActive && !sellDownActive)
				{
					buyDownButton.Content = "Buy Down";
					buyDownButton.Opacity = 1.0;
					buyDownButton.IsEnabled = true;
				}
				
				// Sell Up Button
				if (sellUpActive)
				{
					sellUpButton.Content = "Sell Up ✓";
					sellUpButton.Opacity = 0.6;
					buyUpButton.IsEnabled = false;    // Disable conflicting
					buyUpButton.Opacity = 0.3;
					sellDownButton.IsEnabled = false; // Disable conflicting
					sellDownButton.Opacity = 0.3;
				}
				else if (!buyUpActive && !sellDownActive)
				{
					sellUpButton.Content = "Sell Up";
					sellUpButton.Opacity = 1.0;
					sellUpButton.IsEnabled = true;
				}
			});
		}


		private void OnChartMouseDown(object sender, MouseButtonEventArgs e)
		{
			bool isShiftClick = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
			bool isAltClick = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
			
			bool shouldPlaceBuy = isShiftClick || buyChartMode;
			bool shouldPlaceSell = isAltClick || sellChartMode;
			
			if (!shouldPlaceBuy && !shouldPlaceSell)
				return;

			try
			{
				if (chartScale == null)
				{
					//Print("Error: Chart scale not initialized");
					return;
				}
				
				Point clickPoint = e.GetPosition(ChartControl as IInputElement);
				double yPixel = ChartingExtensions.ConvertToVerticalPixels(clickPoint.Y, ChartControl.PresentationSource);
				double clickedPrice = chartScale.GetValueByY((float)yPixel);
				
				int offset = GetOffsetTicks();
				double orderPrice = clickedPrice + (offset * TickSize);

				if (shouldPlaceBuy)
				{
					PlaceOrder(OrderAction.Buy, orderPrice); // Allow ATM
					buyChartMode = false;
					UpdateButtonStates();
					//Print($"Buy order placed at {orderPrice}");
				}
				else if (shouldPlaceSell)
				{
					PlaceOrder(OrderAction.Sell, orderPrice); // Allow ATM
					sellChartMode = false;
					UpdateButtonStates();
					//Print($"Sell order placed at {orderPrice}");
				}

			}
			catch (Exception ex)
			{
				Print($"Exception in OnChartMouseDown: {ex.Message}");
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
		
			// Check for unfilled orders and cancel them after 1 bar
			CheckAndCancelUnfilledOrders();
		
			// Cancel if no trigger within 1 bar of activation
			if (buyUpActive && buyUpActivatedBar > 0 && CurrentBar > buyUpActivatedBar + 1)
			{
				//Print($"Buy Up cancelled - no trigger within 1 bar");
				buyUpActive = false;
				UpdateButtonStates();
			}
			
			if (sellDownActive && sellDownActivatedBar > 0 && CurrentBar > sellDownActivatedBar + 1)
			{
				//Print($"Sell Down cancelled - no trigger within 1 bar");
				sellDownActive = false;
				UpdateButtonStates();
			}
			
			if (buyDownActive && buyDownActivatedBar > 0 && CurrentBar > buyDownActivatedBar + 1)
			{
				//Print($"Buy Down cancelled - no trigger within 1 bar");
				buyDownActive = false;
				UpdateButtonStates();
			}
			
			if (sellUpActive && sellUpActivatedBar > 0 && CurrentBar > sellUpActivatedBar + 1)
			{
				//Print($"Sell Up cancelled - no trigger within 1 bar");
				sellUpActive = false;
				UpdateButtonStates();
			}
		
			int offset = GetOffsetTicks();
			bool isGreenBar = Close[0] > Open[0];
			bool isRedBar = Close[0] < Open[0];
			
			// Buy Up: Wait for GREEN bar (bullish), then BUY
			if (buyUpActive && CurrentBar > buyUpActivatedBar)
			{
				//Print($"=== Buy Up Check (Bar {CurrentBar}) ===");
				//Print($"Open: {Open[0]}, Close: {Close[0]}");
				//Print($"Is GREEN bar (Close > Open)? {isGreenBar}");
				
				if (isGreenBar)
				{
					double orderPrice = Close[0] + (offset * TickSize);
					buyUpOrder = PlaceOrder(OrderAction.Buy, orderPrice);
					buyUpOrderBar = CurrentBar;
					
					// Cancel opposite order if both were active
					if (sellDownActive)
					{
						if (sellDownOrder != null) CancelOrder(sellDownOrder);
						sellDownActive = false;
					}
					
					buyUpActive = false;
					buyUpActivatedBar = -1;
					UpdateButtonStates();
					//Print($"✓ Buy Up TRIGGERED on GREEN bar, Close={Close[0]}, order at {orderPrice}");
				}
			}
			
			// Sell Down: Wait for RED bar (bearish), then SELL
			if (sellDownActive && CurrentBar > sellDownActivatedBar)
			{
				//Print($"=== Sell Down Check (Bar {CurrentBar}) ===");
				//Print($"Open: {Open[0]}, Close: {Close[0]}");
				//Print($"Is RED bar (Close < Open)? {isRedBar}");
				
				if (isRedBar)
				{
					double orderPrice = Close[0] + (offset * TickSize);
					sellDownOrder = PlaceOrder(OrderAction.Sell, orderPrice);
					sellDownOrderBar = CurrentBar;
					
					if (buyUpActive)
					{
						if (buyUpOrder != null) CancelOrder(buyUpOrder);
						buyUpActive = false;
					}
					
					sellDownActive = false;
					sellDownActivatedBar = -1;
					UpdateButtonStates();
					//Print($"✓ Sell Down TRIGGERED on RED bar, Close={Close[0]}, order at {orderPrice}");
				}
			}
			
			// Buy Down: Wait for RED bar (bearish), then BUY
			if (buyDownActive && CurrentBar > buyDownActivatedBar)
			{
				//Print($"=== Buy Down Check (Bar {CurrentBar}) ===");
				//Print($"Open: {Open[0]}, Close: {Close[0]}");
				//Print($"Is RED bar (Close < Open)? {isRedBar}");
				
				if (isRedBar)
				{
					double orderPrice = Close[0] + (offset * TickSize);
					buyDownOrder = PlaceOrder(OrderAction.Buy, orderPrice);
					buyDownOrderBar = CurrentBar;
					
					if (sellUpActive)
					{
						if (sellUpOrder != null) CancelOrder(sellUpOrder);
						sellUpActive = false;
					}
					
					buyDownActive = false;
					buyDownActivatedBar = -1;
					UpdateButtonStates();
					//Print($"✓ Buy Down TRIGGERED on RED bar, Close={Close[0]}, order at {orderPrice}");
				}
			}
			
			// Sell Up: Wait for GREEN bar (bullish), then SELL
			if (sellUpActive && CurrentBar > sellUpActivatedBar)
			{
				//Print($"=== Sell Up Check (Bar {CurrentBar}) ===");
				//Print($"Open: {Open[0]}, Close: {Close[0]}");
				//Print($"Is GREEN bar (Close > Open)? {isGreenBar}");
				
				if (isGreenBar)
				{
					double orderPrice = Close[0] + (offset * TickSize);
					sellUpOrder = PlaceOrder(OrderAction.Sell, orderPrice);
					sellUpOrderBar = CurrentBar;
					
					if (buyDownActive)
					{
						if (buyDownOrder != null) CancelOrder(buyDownOrder);
						buyDownActive = false;
					}
					
					sellUpActive = false;
					sellUpActivatedBar = -1;
					UpdateButtonStates();
					//Print($"✓ Sell Up TRIGGERED on GREEN bar, Close={Close[0]}, order at {orderPrice}");
				}
			}
		}



		private void CheckAndCancelUnfilledOrders()
		{
			// Cancel Buy Up order if unfilled after 1 bar AND still working
			if (buyUpOrder != null && buyUpOrderBar > 0 && CurrentBar > buyUpOrderBar)
			{
				// Check if order is still working (not filled or cancelled)
				if (buyUpOrder.OrderState == OrderState.Working || buyUpOrder.OrderState == OrderState.Accepted)
				{
					CancelOrder(buyUpOrder);
					//Print("Buy Up order cancelled (unfilled after 1 bar)");
				}
				buyUpOrder = null;
				buyUpOrderBar = -1;
			}
			
			if (sellDownOrder != null && sellDownOrderBar > 0 && CurrentBar > sellDownOrderBar)
			{
				if (sellDownOrder.OrderState == OrderState.Working || sellDownOrder.OrderState == OrderState.Accepted)
				{
					CancelOrder(sellDownOrder);
					//Print("Sell Down order cancelled (unfilled after 1 bar)");
				}
				sellDownOrder = null;
				sellDownOrderBar = -1;
			}
			
			if (buyDownOrder != null && buyDownOrderBar > 0 && CurrentBar > buyDownOrderBar)
			{
				if (buyDownOrder.OrderState == OrderState.Working || buyDownOrder.OrderState == OrderState.Accepted)
				{
					CancelOrder(buyDownOrder);
					//Print("Buy Down order cancelled (unfilled after 1 bar)");
				}
				buyDownOrder = null;
				buyDownOrderBar = -1;
			}
			
			if (sellUpOrder != null && sellUpOrderBar > 0 && CurrentBar > sellUpOrderBar)
			{
				if (sellUpOrder.OrderState == OrderState.Working || sellUpOrder.OrderState == OrderState.Accepted)
				{
					CancelOrder(sellUpOrder);
					//Print("Sell Up order cancelled (unfilled after 1 bar)");
				}
				sellUpOrder = null;
				sellUpOrderBar = -1;
			}
		}


		private void CancelOrder(Order order)
		{
			ChartControl.Dispatcher.InvokeAsync(() =>
			{
				try
				{
					var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
					if (chartWindow == null) return;
					
					xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
					if (xAlselector == null) return;
					
					Account account = xAlselector.SelectedAccount;
					if (account == null) return;

					account.Cancel(new[] { order });
				}
				catch (Exception ex)
				{
					Print($"Error cancelling order: {ex.Message}");
				}
			});
		}
		
	


		private Order PlaceOrder(OrderAction action, double price)
		{
			Order placedOrder = null;
			
			ChartControl.Dispatcher.Invoke(() =>
			{
				try
				{
					var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
					if (chartWindow == null)
					{
						//Print("Error: Could not get chart window");
						return;
					}
					
					xAlselector = chartWindow.FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
					if (xAlselector == null)
					{
						//Print("Error: Could not find account selector");
						return;
					}
					
					Account account = xAlselector.SelectedAccount;
					if (account == null)
					{
						//Print("Error: No account selected");
						return;
					}
		
					double currentPrice = (action == OrderAction.Buy) ? GetCurrentAsk() : GetCurrentBid();
					OrderType orderType;
					
					if (action == OrderAction.Buy)
					{
						orderType = (price > currentPrice) ? OrderType.StopMarket : OrderType.Limit;
					}
					else
					{
						orderType = (price < currentPrice) ? OrderType.StopMarket : OrderType.Limit;
					}
		
					// Create the order (but don't submit yet)
					placedOrder = account.CreateOrder(
						Instrument,
						action,
						orderType,
						OrderEntry.Manual,
						TimeInForce.Day,
						1,
						(orderType == OrderType.Limit ? price : 0),
						(orderType == OrderType.StopMarket ? price : 0),
						string.Empty,
						"Entry",  // Name it "Entry" for ATM
						Core.Globals.MaxDate,
						null
					);
					
					if (placedOrder != null)
					{
						// Get selected ATM from Chart Trader
						string selectedAtm = GetSelectedAtmStrategy();
						
						if (!string.IsNullOrEmpty(selectedAtm) && selectedAtm != "<None>")
						{
							// StartAtmStrategy submits the order AND attaches ATM
							NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(selectedAtm, placedOrder);
							//Print($"✓ Order submitted with ATM: {action} {orderType} at {price}");
							//Print($"✓ ATM Strategy '{selectedAtm}' attached!");
						}
						else
						{
							// No ATM selected, just submit normally
							account.Submit(new[] { placedOrder });
							//Print($"✓ Order submitted: {action} {orderType} at {price} (No ATM - select one in Chart Trader)");
						}
					}
				}
				catch (Exception ex)
				{
					Print($"Exception in PlaceOrder: {ex.Message}");
					Print($"Stack: {ex.StackTrace}");
				}
			});
			
			return placedOrder;
		}
		
		private string GetSelectedAtmStrategy()
		{
			try
			{
				//Print("Looking for ATM selector...");
				var chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
				if (chartWindow == null)
				{
					//Print("Chart window is null");
					return null;
				}
				
				// Try to find ATM strategy selector
				var atmSelector = chartWindow.FindFirst("ChartTraderControlATMStrategySelector") as System.Windows.Controls.ComboBox;
				
				if (atmSelector == null)
				{
					//Print("ATM selector control not found");
					return null;
				}
				
				//Print($"ATM selector found. SelectedItem: {atmSelector.SelectedItem}");
				
				if (atmSelector.SelectedItem != null)
				{
					string fullText = atmSelector.SelectedItem.ToString();
					//Print($"Full ATM text: '{fullText}'");
					
					// Extract just the template name from "name='AtmStrategy' id=-1; Template: NQ-LargeTrades-1Con"
					if (fullText.Contains("Template:"))
					{
						string[] parts = fullText.Split(new string[] { "Template:" }, StringSplitOptions.None);
						if (parts.Length > 1)
						{
							string atmName = parts[1].Trim();
							//Print($"Extracted ATM name: '{atmName}'");
							return atmName;
						}
					}
					
					// Fallback: if no "Template:" found, return as is
					//Print($"Could not extract template name, using full text");
					return fullText;
				}
				
				//Print("ATM SelectedItem is null");
			}
			catch (Exception ex)
			{
				Print($"Error in GetSelectedAtmStrategy: {ex.Message}");
				Print($"Stack: {ex.StackTrace}");
			}
			return null;
		}












		private int GetOffsetTicks()
		{
			int offset = 0;
			try
			{
				if (ChartControl != null && offsetTextBox != null)
				{
					ChartControl.Dispatcher.Invoke(() =>
					{
						if (offsetTextBox != null && int.TryParse(offsetTextBox.Text, out int result))
							offset = result;
					});
				}
				else
				{
					offset = DefaultOffset;
				}
			}
			catch
			{
				offset = DefaultOffset;
			}
			return offset;
		}

		private void RemoveButtonPanel()
		{
			if (containerCanvas != null)
			{
				if (UserControlCollection.Contains(containerCanvas))
					UserControlCollection.Remove(containerCanvas);

				if (dragHandle != null)
				{
					dragHandle.MouseLeftButtonDown -= OnHandleMouseDown;
					dragHandle.MouseMove -= OnHandleMouseMove;
					dragHandle.MouseLeftButtonUp -= OnHandleMouseUp;
				}

				if (ChartPanel != null)
				{
					ChartPanel.MouseLeftButtonDown -= OnChartMouseDown;
				}

				if (buyChartButton != null) buyChartButton.Click -= BuyChartClick;
				if (sellChartButton != null) sellChartButton.Click -= SellChartClick;
				if (buyUpButton != null) buyUpButton.Click -= BuyUpClick;
				if (sellDownButton != null) sellDownButton.Click -= SellDownClick;
				if (buyDownButton != null) buyDownButton.Click -= BuyDownClick;
				if (sellUpButton != null) sellUpButton.Click -= SellUpClick;
				if (buyMarketButton != null) buyMarketButton.Click -= BuyMarketClick;
				if (sellMarketButton != null) sellMarketButton.Click -= SellMarketClick;

			}
		}

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Buy Button Color", Order = 1, GroupName = "Button Colors")]
		public Brush BuyButtonColor { get; set; }

		[Browsable(false)]
		public string BuyButtonColorSerializable
		{
			get { return Serialize.BrushToString(BuyButtonColor); }
			set { BuyButtonColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Sell Button Color", Order = 2, GroupName = "Button Colors")]
		public Brush SellButtonColor { get; set; }

		[Browsable(false)]
		public string SellButtonColorSerializable
		{
			get { return Serialize.BrushToString(SellButtonColor); }
			set { SellButtonColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Button Text Color", Order = 3, GroupName = "Button Colors")]
		public Brush ButtonTextColor { get; set; }

		[Browsable(false)]
		public string ButtonTextColorSerializable
		{
			get { return Serialize.BrushToString(ButtonTextColor); }
			set { ButtonTextColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Handle Color", Order = 4, GroupName = "Button Colors")]
		public Brush HandleColor { get; set; }

		[Browsable(false)]
		public string HandleColorSerializable
		{
			get { return Serialize.BrushToString(HandleColor); }
			set { HandleColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(-100, 100)]
		[Display(Name = "Default Offset (Ticks)", Order = 1, GroupName = "Order Settings")]
		public int DefaultOffset { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Use ATM for Chart Orders", Order = 2, GroupName = "Order Settings")]
		public bool UseAtmForChartOrders { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "ATM Strategy Template", Order = 3, GroupName = "Order Settings")]
		public string AtmStrategyTemplate { get; set; }
		
		

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OrderEntryPanel[] cacheOrderEntryPanel;
		public OrderEntryPanel OrderEntryPanel(Brush buyButtonColor, Brush sellButtonColor, Brush buttonTextColor, Brush handleColor, int defaultOffset, bool useAtmForChartOrders, string atmStrategyTemplate)
		{
			return OrderEntryPanel(Input, buyButtonColor, sellButtonColor, buttonTextColor, handleColor, defaultOffset, useAtmForChartOrders, atmStrategyTemplate);
		}

		public OrderEntryPanel OrderEntryPanel(ISeries<double> input, Brush buyButtonColor, Brush sellButtonColor, Brush buttonTextColor, Brush handleColor, int defaultOffset, bool useAtmForChartOrders, string atmStrategyTemplate)
		{
			if (cacheOrderEntryPanel != null)
				for (int idx = 0; idx < cacheOrderEntryPanel.Length; idx++)
					if (cacheOrderEntryPanel[idx] != null && cacheOrderEntryPanel[idx].BuyButtonColor == buyButtonColor && cacheOrderEntryPanel[idx].SellButtonColor == sellButtonColor && cacheOrderEntryPanel[idx].ButtonTextColor == buttonTextColor && cacheOrderEntryPanel[idx].HandleColor == handleColor && cacheOrderEntryPanel[idx].DefaultOffset == defaultOffset && cacheOrderEntryPanel[idx].UseAtmForChartOrders == useAtmForChartOrders && cacheOrderEntryPanel[idx].AtmStrategyTemplate == atmStrategyTemplate && cacheOrderEntryPanel[idx].EqualsInput(input))
						return cacheOrderEntryPanel[idx];
			return CacheIndicator<OrderEntryPanel>(new OrderEntryPanel(){ BuyButtonColor = buyButtonColor, SellButtonColor = sellButtonColor, ButtonTextColor = buttonTextColor, HandleColor = handleColor, DefaultOffset = defaultOffset, UseAtmForChartOrders = useAtmForChartOrders, AtmStrategyTemplate = atmStrategyTemplate }, input, ref cacheOrderEntryPanel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OrderEntryPanel OrderEntryPanel(Brush buyButtonColor, Brush sellButtonColor, Brush buttonTextColor, Brush handleColor, int defaultOffset, bool useAtmForChartOrders, string atmStrategyTemplate)
		{
			return indicator.OrderEntryPanel(Input, buyButtonColor, sellButtonColor, buttonTextColor, handleColor, defaultOffset, useAtmForChartOrders, atmStrategyTemplate);
		}

		public Indicators.OrderEntryPanel OrderEntryPanel(ISeries<double> input , Brush buyButtonColor, Brush sellButtonColor, Brush buttonTextColor, Brush handleColor, int defaultOffset, bool useAtmForChartOrders, string atmStrategyTemplate)
		{
			return indicator.OrderEntryPanel(input, buyButtonColor, sellButtonColor, buttonTextColor, handleColor, defaultOffset, useAtmForChartOrders, atmStrategyTemplate);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OrderEntryPanel OrderEntryPanel(Brush buyButtonColor, Brush sellButtonColor, Brush buttonTextColor, Brush handleColor, int defaultOffset, bool useAtmForChartOrders, string atmStrategyTemplate)
		{
			return indicator.OrderEntryPanel(Input, buyButtonColor, sellButtonColor, buttonTextColor, handleColor, defaultOffset, useAtmForChartOrders, atmStrategyTemplate);
		}

		public Indicators.OrderEntryPanel OrderEntryPanel(ISeries<double> input , Brush buyButtonColor, Brush sellButtonColor, Brush buttonTextColor, Brush handleColor, int defaultOffset, bool useAtmForChartOrders, string atmStrategyTemplate)
		{
			return indicator.OrderEntryPanel(input, buyButtonColor, sellButtonColor, buttonTextColor, handleColor, defaultOffset, useAtmForChartOrders, atmStrategyTemplate);
		}
	}
}

#endregion
