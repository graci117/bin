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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class ReverseExit : Indicator
	{
		#region Enums
		public enum ButtonLocationTypes
		{
			ChartTrader,
			Toolbar
		}
		#endregion
		
		public override string DisplayName
		{
			get
			{
				return "Reverse Exit";
			}
		}
		
		private Account myAccount;
		private int consecutiveReversalBars = 1;
		private int currentReversalCount = 0;
		private int reversalTickOffset = 0;
		private bool enableReverseExit = false;
		private ButtonLocationTypes buttonLocation = ButtonLocationTypes.Toolbar;
		
		// Drawing
		private bool showReversalArrows = true;
		private Brush bullishArrowColor = Brushes.Lime;
		private Brush bearishArrowColor = Brushes.Red;
		
		// Chart Trader Button
		private System.Windows.Controls.RowDefinition addedRow;
		private Gui.Chart.ChartTab chartTab;
		private Gui.Chart.Chart chartWindow;
		private System.Windows.Controls.Grid chartTraderGrid, lowerButtonsGrid;
		private System.Windows.Controls.Button toggleButton;
		private bool panelActive;
		private System.Windows.Controls.TabItem tabItem;
		private bool buttonClicked = false;
		private bool isToolBarButtonAdded = false;
		
		// Position tracking
		private MarketPosition lastKnownPosition = MarketPosition.Flat;
		private int lastReversalBar = -1;
		
		// Enhanced debugging
		private bool enableDebugOutput = true;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Monitors reversal candles and exits positions after specified consecutive reversals";
				Name = "ReverseExit";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = true;
				DrawOnPricePanel = true;
				PaintPriceMarkers = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						if (buttonLocation == ButtonLocationTypes.ChartTrader)
							CreateWPFControls();
						else
							AddButtonToToolbar();
					});
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						if (buttonLocation == ButtonLocationTypes.ChartTrader)
							DisposeWPFControls();
						else
							RemoveButtonFromToolbar();
					});
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2 || !enableReverseExit)
				return;
			
			// Get current position
			MarketPosition currentPosition = GetCurrentPosition();
			int positionSize = GetPositionSize();
			
			if (enableDebugOutput && CurrentBar % 50 == 0) // Debug every 50 bars
			{
				Print(string.Format("Bar {0}: Position={1}, Size={2}, Monitoring={3}", 
					CurrentBar, currentPosition, positionSize, enableReverseExit));
			}
			
			// Check for reversal candles based on current position
			bool reversalDetected = false;
			
			if (currentPosition == MarketPosition.Long && positionSize > 0)
			{
				// Look for bearish reversal to exit long
				if (IsBearishReversalCandle())
				{
					reversalDetected = true;
					Print(string.Format("*** BEARISH REVERSAL DETECTED at bar {0} (Long position detected: {1} contracts)", CurrentBar, positionSize));
					
					if (showReversalArrows)
						Draw.ArrowDown(this, "BearRev" + CurrentBar, true, 0, High[0] + (2 * TickSize), bearishArrowColor);
				}
			}
			else if (currentPosition == MarketPosition.Short && positionSize > 0)
			{
				// Look for bullish reversal to exit short
				if (IsBullishReversalCandle())
				{
					reversalDetected = true;
					Print(string.Format("*** BULLISH REVERSAL DETECTED at bar {0} (Short position detected: {1} contracts)", CurrentBar, positionSize));
					
					if (showReversalArrows)
						Draw.ArrowUp(this, "BullRev" + CurrentBar, true, 0, Low[0] - (2 * TickSize), bullishArrowColor);
				}
			}
			else if (positionSize == 0 && enableDebugOutput && CurrentBar % 100 == 0)
			{
				Print(string.Format("No position detected at bar {0}", CurrentBar));
			}
			
			// Track consecutive reversals
			if (reversalDetected && lastReversalBar != CurrentBar)
			{
				currentReversalCount++;
				lastReversalBar = CurrentBar;
				
				Print(string.Format("Reversal #{0} detected at bar {1} (need {2} total)", currentReversalCount, CurrentBar, consecutiveReversalBars));
				
				// Exit position if we hit the required count
				if (currentReversalCount >= consecutiveReversalBars)
				{
					Print(string.Format("*** REVERSAL COUNT REACHED - ATTEMPTING TO EXIT POSITION ***"));
					ExitPosition(currentPosition, positionSize);
					currentReversalCount = 0; // Reset counter after exit
				}
			}
			else if (!reversalDetected && CurrentBar != lastReversalBar + 1)
			{
				// Reset counter if non-reversal bar appears (not immediately after reversal)
				if (currentReversalCount > 0)
				{
					Print(string.Format("Reversal counter reset at bar {0}", CurrentBar));
					currentReversalCount = 0;
				}
			}
			
			lastKnownPosition = currentPosition;
		}
		
		#region Reversal Detection Methods
		
		private bool IsBullishReversalCandle()
		{
			if (CurrentBar < 1)
				return false;
			
			// Simplified Bullish Reversal:
			// 1. Previous candle is red (bearish)
			// 2. Current candle is green (bullish)
			// 3. Low went below previous low by at least the tick offset
			
			bool previousBearish = Close[1] < Open[1];
			bool currentBullish = Close[0] > Open[0];
			
			double tickSize = TickSize;
			double offsetPrice = reversalTickOffset * tickSize;
			
			bool lowExtendedBelowPrevious = Low[0] <= (Low[1] - offsetPrice);
			
			bool isReversal = previousBearish && currentBullish && lowExtendedBelowPrevious;
			
			if (enableDebugOutput && isReversal)
			{
				Print(string.Format("Bullish Reversal: Prev Red={0}, Curr Green={1}, Low[0]={2} <= Low[1]-offset={3}", 
					previousBearish, currentBullish, Low[0], Low[1] - offsetPrice));
			}
			
			return isReversal;
		}
		
		private bool IsBearishReversalCandle()
		{
			if (CurrentBar < 1)
				return false;
			
			// Simplified Bearish Reversal:
			// 1. Previous candle is green (bullish)
			// 2. Current candle is red (bearish)
			// 3. High went above previous high by at least the tick offset
			
			bool previousBullish = Close[1] > Open[1];
			bool currentBearish = Close[0] < Open[0];
			
			double tickSize = TickSize;
			double offsetPrice = reversalTickOffset * tickSize;
			
			bool highExtendedAbovePrevious = High[0] >= (High[1] + offsetPrice);
			
			bool isReversal = previousBullish && currentBearish && highExtendedAbovePrevious;
			
			if (enableDebugOutput && isReversal)
			{
				Print(string.Format("Bearish Reversal: Prev Green={0}, Curr Red={1}, High[0]={2} >= High[1]+offset={3}", 
					previousBullish, currentBearish, High[0], High[1] + offsetPrice));
			}
			
			return isReversal;
		}
		
		#endregion
		
		#region Position Management
		
		private MarketPosition GetCurrentPosition()
		{
			try
			{
				// Ensure account is set
				if (myAccount == null)
					GetAccount();
				
				if (myAccount != null)
				{
					lock (myAccount.Positions)
					{
						foreach (var pos in myAccount.Positions)
						{
							if (pos.Instrument == Instrument)
								return pos.MarketPosition;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Print("Error getting position: " + ex.Message);
			}
			
			return MarketPosition.Flat;
		}
		
		private int GetPositionSize()
		{
			try
			{
				// Ensure account is set
				if (myAccount == null)
					GetAccount();
				
				if (myAccount != null)
				{
					lock (myAccount.Positions)
					{
						foreach (var pos in myAccount.Positions)
						{
							if (pos.Instrument == Instrument)
								return pos.Quantity;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Print("Error getting position size: " + ex.Message);
			}
			
			return 0;
		}
		
		private void GetAccount()
		{
			if (ChartControl == null)
				return;
			
			ChartControl.Dispatcher.InvokeAsync(() =>
			{
				try
				{
					NinjaTrader.Gui.Tools.AccountSelector accountSelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector);
					
					if (accountSelector != null && accountSelector.SelectedAccount != null)
					{
						myAccount = accountSelector.SelectedAccount;
						if (enableDebugOutput)
							Print(string.Format("ReverseExit: Connected to account: {0}", myAccount.Name));
					}
					else
					{
						if (enableDebugOutput)
							Print("ReverseExit: WARNING - Could not find account selector");
					}
				}
				catch (Exception ex)
				{
					Print("Error getting account: " + ex.Message);
				}
			});
		}
		
		private void ExitPosition(MarketPosition position, int quantity)
		{
			if (quantity == 0)
			{
				Print("ERROR: Cannot exit - position quantity is 0");
				return;
			}
			
			// Ensure account is available
			if (myAccount == null)
			{
				Print("ERROR: Account not available, attempting to reconnect...");
				GetAccount();
				return;
			}
			
			try
			{
				Print(string.Format("Attempting to exit {0} position of {1} contracts on account {2}", 
					position, quantity, myAccount.Name));
				
				// First, cancel all orders associated with this instrument to clean up stop/profit orders
				CancelAllOrders();
				
				Order exitOrder = null;
				
				if (position == MarketPosition.Long)
				{
					// Create SELL order without ATM strategy
					exitOrder = myAccount.CreateOrder(Instrument, OrderAction.Sell, OrderType.Market, OrderEntry.Manual, TimeInForce.Day, quantity, 0, 0, "", "Reversal Exit", DateTime.MaxValue, null);
					Print(string.Format("Created SELL order to exit LONG position of {0} contracts after {1} reversal bar(s)", quantity, consecutiveReversalBars));
				}
				else if (position == MarketPosition.Short)
				{
					// Create BUY TO COVER order without ATM strategy
					exitOrder = myAccount.CreateOrder(Instrument, OrderAction.BuyToCover, OrderType.Market, OrderEntry.Manual, TimeInForce.Day, quantity, 0, 0, "", "Reversal Exit", DateTime.MaxValue, null);
					Print(string.Format("Created BUY TO COVER order to exit SHORT position of {0} contracts after {1} reversal bar(s)", quantity, consecutiveReversalBars));
				}
				
				if (exitOrder != null)
				{
					// Submit order WITHOUT triggering custom event to avoid ATM strategy attachment
					myAccount.Submit(new[] { exitOrder });
					Print("*** EXIT ORDER SUBMITTED SUCCESSFULLY - ALL PENDING ORDERS CANCELLED ***");
					
					// Visual confirmation
					if (position == MarketPosition.Long)
						Draw.Text(this, "Exit" + CurrentBar, "EXIT LONG", 0, High[0] + (4 * TickSize), Brushes.Yellow);
					else
						Draw.Text(this, "Exit" + CurrentBar, "EXIT SHORT", 0, Low[0] - (4 * TickSize), Brushes.Yellow);
				}
				else
				{
					Print("ERROR: Failed to create exit order");
				}
			}
			catch (Exception ex)
			{
				Print("ERROR exiting position: " + ex.Message);
				Print("Stack trace: " + ex.StackTrace);
			}
		}
		
		private void CancelAllOrders()
		{
			try
			{
				if (myAccount == null)
					return;
				
				// Get all orders for this instrument
				lock (myAccount.Orders)
				{
					// Create a list to avoid modifying collection while iterating
					List<Order> ordersToCancel = new List<Order>();
					
					foreach (var order in myAccount.Orders)
					{
						// Only cancel orders for this instrument that are not filled/cancelled already
						if (order.Instrument == Instrument && 
							order.OrderState != OrderState.Filled && 
							order.OrderState != OrderState.Cancelled && 
							order.OrderState != OrderState.Rejected)
						{
							ordersToCancel.Add(order);
						}
					}
					
					// Cancel all identified orders
					foreach (var order in ordersToCancel)
					{
						try
						{
							myAccount.Cancel(new[] { order });
							Print(string.Format("Cancelled order: {0} - {1} {2} @ {3}", 
								order.Name, order.OrderAction, order.Quantity, order.LimitPrice > 0 ? order.LimitPrice.ToString() : "Market"));
						}
						catch (Exception ex)
						{
							Print(string.Format("Error cancelling order {0}: {1}", order.Name, ex.Message));
						}
					}
				}
			}
			catch (Exception ex)
			{
				Print("Error cancelling all orders: " + ex.Message);
			}
		}

		
		#endregion
		
		#region Toolbar Button Methods
		
		private void AddButtonToToolbar()
		{
			if (isToolBarButtonAdded)
				return;
			
			chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;
			
			if (chartWindow == null)
				return;
			
			// Create the toolbar button
			toggleButton = new System.Windows.Controls.Button()
			{
				Content = "REX OFF",
				Height = 20,
				Width = 55,
				Margin = new Thickness(2),
				Padding = new Thickness(2),
				Background = Brushes.Red,
				Foreground = Brushes.White,
				BorderBrush = Brushes.Black,
				BorderThickness = new Thickness(1.0),
				FontSize = 10
			};
			
			toggleButton.Click += ToggleButtonClick;
			
			// Add to MainMenu (toolbar)
			chartWindow.MainMenu.Add(toggleButton);
			
			isToolBarButtonAdded = true;
		}
		
		private void RemoveButtonFromToolbar()
		{
			if (!isToolBarButtonAdded || chartWindow == null || toggleButton == null)
				return;
			
			toggleButton.Click -= ToggleButtonClick;
			chartWindow.MainMenu.Remove(toggleButton);
			isToolBarButtonAdded = false;
		}
		
		#endregion
		
		#region Button Management
		
		protected void ToggleButtonClick(object sender, RoutedEventArgs e)
		{
			if (buttonClicked == false)
			{
				// Get account when enabling
				GetAccount();
				
				toggleButton.Background = Brushes.Green;
				if (buttonLocation == ButtonLocationTypes.ChartTrader)
					toggleButton.Content = "Reverse Exit ON";
				else
					toggleButton.Content = "REX ON";
				enableReverseExit = true;
				buttonClicked = true;
				currentReversalCount = 0;
				
				// Get position info for debug
				MarketPosition pos = GetCurrentPosition();
				int size = GetPositionSize();
				Print(string.Format("=== Reverse Exit ENABLED === Current Position: {0}, Size: {1}", pos, size));
			}
			else
			{
				toggleButton.Background = Brushes.Red;
				if (buttonLocation == ButtonLocationTypes.ChartTrader)
					toggleButton.Content = "Reverse Exit OFF";
				else
					toggleButton.Content = "REX OFF";
				enableReverseExit = false;
				buttonClicked = false;
				currentReversalCount = 0;
				Print("=== Reverse Exit DISABLED ===");
			}
		}
		
		protected void CreateWPFControls()
		{
			chartWindow = Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
			
			if (chartWindow == null)
				return;
			
			chartTraderGrid = (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;
			
			lowerButtonsGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid, 1);
			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			
			addedRow = new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
			
			Style basicButtonStyle = Application.Current.FindResource("BasicEntryButton") as Style;
			
			toggleButton = new System.Windows.Controls.Button()
			{
				Content = "Reverse Exit OFF",
				Height = 25,
				Margin = new Thickness(5, 0, 5, 0),
				Padding = new Thickness(0, 0, 0, 0),
				Style = basicButtonStyle,
				Background = Brushes.Red,
				BorderBrush = Brushes.Black,
				Foreground = Brushes.White,
				BorderThickness = new Thickness(2.0)
			};
			
			toggleButton.Click += ToggleButtonClick;
			
			System.Windows.Controls.Grid.SetColumn(toggleButton, 0);
			System.Windows.Controls.Grid.SetRow(toggleButton, 0);
			
			lowerButtonsGrid.Children.Add(toggleButton);
			
			if (totalGrids == 0)
				totalGrids = chartTraderGrid.RowDefinitions.Count;
			
			if (TabSelected())
				InsertWPFControls();
			
			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}
		
		static int totalGrids;
		
		public void DisposeWPFControls()
		{
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			
			if (toggleButton != null)
				toggleButton.Click -= ToggleButtonClick;
			
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
			
			if (lowerButtonsGrid != null)
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
		
		#endregion
		
		#region Properties
		
		[Range(1, 10)]
		[Display(Name = "Consecutive reversal bars", Order = 1, GroupName = "Exit Settings", Description = "Number of consecutive reversal bars required to exit position")]
		public int ConsecutiveReversalBars
		{
			get { return consecutiveReversalBars; }
			set { consecutiveReversalBars = Math.Max(1, value); }
		}
		
		[Range(0, 100)]
		[Display(Name = "Reversal tick offset", Order = 2, GroupName = "Exit Settings", Description = "Minimum tick offset for price to extend beyond previous candle (0 = no requirement)")]
		public int ReversalTickOffset
		{
			get { return reversalTickOffset; }
			set { reversalTickOffset = Math.Max(0, value); }
		}
		
		[Display(Name = "Button location", Order = 1, GroupName = "UI Settings", Description = "Choose where to place the button")]
		public ButtonLocationTypes ButtonLocation
		{
			get { return buttonLocation; }
			set { buttonLocation = value; }
		}
		
		[Display(Name = "Enable debug output", Order = 2, GroupName = "UI Settings", Description = "Show detailed debug messages in Output window")]
		public bool EnableDebugOutput
		{
			get { return enableDebugOutput; }
			set { enableDebugOutput = value; }
		}
		
		[Display(Name = "Show reversal arrows", Order = 1, GroupName = "Visual Settings", Description = "Display arrows when reversal candles are detected")]
		public bool ShowReversalArrows
		{
			get { return showReversalArrows; }
			set { showReversalArrows = value; }
		}
		
		[XmlIgnore]
		[Display(Name = "Bullish arrow color", Order = 2, GroupName = "Visual Settings", Description = "Color for bullish reversal arrows")]
		public Brush BullishArrowColor
		{
			get { return bullishArrowColor; }
			set { bullishArrowColor = value; }
		}
		
		[Browsable(false)]
		public string BullishArrowColorSerialize
		{
			get { return Serialize.BrushToString(bullishArrowColor); }
			set { bullishArrowColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Bearish arrow color", Order = 3, GroupName = "Visual Settings", Description = "Color for bearish reversal arrows")]
		public Brush BearishArrowColor
		{
			get { return bearishArrowColor; }
			set { bearishArrowColor = value; }
		}
		
		[Browsable(false)]
		public string BearishArrowColorSerialize
		{
			get { return Serialize.BrushToString(bearishArrowColor); }
			set { bearishArrowColor = Serialize.StringToBrush(value); }
		}
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ReverseExit[] cacheReverseExit;
		public ReverseExit ReverseExit()
		{
			return ReverseExit(Input);
		}

		public ReverseExit ReverseExit(ISeries<double> input)
		{
			if (cacheReverseExit != null)
				for (int idx = 0; idx < cacheReverseExit.Length; idx++)
					if (cacheReverseExit[idx] != null &&  cacheReverseExit[idx].EqualsInput(input))
						return cacheReverseExit[idx];
			return CacheIndicator<ReverseExit>(new ReverseExit(), input, ref cacheReverseExit);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ReverseExit ReverseExit()
		{
			return indicator.ReverseExit(Input);
		}

		public Indicators.ReverseExit ReverseExit(ISeries<double> input )
		{
			return indicator.ReverseExit(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ReverseExit ReverseExit()
		{
			return indicator.ReverseExit(Input);
		}

		public Indicators.ReverseExit ReverseExit(ISeries<double> input )
		{
			return indicator.ReverseExit(input);
		}
	}
}

#endregion
