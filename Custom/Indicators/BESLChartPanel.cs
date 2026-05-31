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
	public class BESLChartPanel : Indicator
	{
		private System.Windows.Controls.Canvas containerCanvas;
		private System.Windows.Controls.Grid buttonPanel;
		private System.Windows.Controls.Border dragHandle;
		private System.Windows.Controls.TextBlock handleText;
		private System.Windows.Controls.Button[] slButtonsArray;
		private System.Windows.Controls.Button[] beButtonsArray;
		private System.Windows.Controls.Button flattenButton;
		private NinjaTrader.Gui.Tools.AccountSelector xAlselector;
		private NinjaTrader.Gui.Tools.InstrumentSelector xInSelector;
		private bool isCollapsed = false;
		private Point dragStartPoint;
		private bool isDragging = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Move SL and Breakeven buttons on chart overlay with draggable handle";
				Name = "BESLChartPanel";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = false;
				PaintPriceMarkers = false;
				
				// SL Button settings
				SLButton1Enable = true;
				SLButton1Ticks = 2;
				SLButton1Color = Brushes.IndianRed;
				SLButton2Enable = true;
				SLButton2Ticks = 4;
				SLButton2Color = Brushes.IndianRed;
				SLButton3Enable = true;
				SLButton3Ticks = 6;
				SLButton3Color = Brushes.IndianRed;
				SLButton4Enable = true;
				SLButton4Percent = 50;
				SLButton4Color = Brushes.IndianRed;
				
				// BE Button settings
				BEButton1Enable = true;
				BEButton1Ticks = 0;
				BEButton1Color = Brushes.DodgerBlue;
				BEButton2Enable = true;
				BEButton2Ticks = 2;
				BEButton2Color = Brushes.DodgerBlue;
				BEButton3Enable = false;
				BEButton3Ticks = 4;
				BEButton3Color = Brushes.DodgerBlue;
				BEButton4Enable = false;
				BEButton4Ticks = 6;
				BEButton4Color = Brushes.DodgerBlue;
				
				// Flatten button
				ShowFlattenButton = true;
				FlattenButtonColor = Brushes.HotPink;
				
				ButtonTextColor = Brushes.White;
				HandleColor = Brushes.Green;
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

		private void CreateButtonPanel()
		{
			if (UserControlCollection.Contains(containerCanvas))
				return;

			// Create a Canvas container to hold everything
			containerCanvas = new System.Windows.Controls.Canvas
			{
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Background = Brushes.Transparent
			};

			// Create the main button panel grid
			buttonPanel = new System.Windows.Controls.Grid
			{
				Name = "BESLPanel",
				Background = new SolidColorBrush(Color.FromArgb(200, 40, 40, 40))
			};

			// Always use 2 columns
			buttonPanel.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			buttonPanel.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			// Count total enabled buttons to determine rows
			int totalEnabledButtons = 0;
			if (SLButton1Enable) totalEnabledButtons++;
			if (SLButton2Enable) totalEnabledButtons++;
			if (SLButton3Enable) totalEnabledButtons++;
			if (SLButton4Enable) totalEnabledButtons++;
			if (BEButton1Enable) totalEnabledButtons++;
			if (BEButton2Enable) totalEnabledButtons++;
			if (BEButton3Enable) totalEnabledButtons++;
			if (BEButton4Enable) totalEnabledButtons++;

			// Calculate rows needed (2 buttons per row)
			int buttonRows = (totalEnabledButtons + 1) / 2;
			
			// Add rows for buttons
			for (int i = 0; i < buttonRows; i++)
				buttonPanel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			
			// Add one more row for Flatten button if enabled
			if (ShowFlattenButton)
				buttonPanel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());

			// Create all buttons in order (SL then BE)
			List<System.Windows.Controls.Button> allButtons = new List<System.Windows.Controls.Button>();

			// Create SL buttons
			slButtonsArray = new System.Windows.Controls.Button[4];
			bool[] slButtonEnabled = new bool[] { SLButton1Enable, SLButton2Enable, SLButton3Enable, SLButton4Enable };
			int[] slButtonTicks = new int[] { SLButton1Ticks, SLButton2Ticks, SLButton3Ticks, SLButton4Percent };
			Brush[] slButtonColors = new Brush[] { SLButton1Color, SLButton2Color, SLButton3Color, SLButton4Color };
			string[] slButtonLabels = new string[]
			{
				$"SL +{SLButton1Ticks}",
				$"SL +{SLButton2Ticks}",
				$"SL +{SLButton3Ticks}",
				$"SL {SLButton4Percent}%"
			};

			for (int i = 0; i < 4; i++)
			{
				if (!slButtonEnabled[i])
					continue;

				slButtonsArray[i] = new System.Windows.Controls.Button
				{
					Content = slButtonLabels[i],
					Height = 30,
					Margin = new Thickness(0),
					Padding = new Thickness(5, 0, 5, 0),
					Background = slButtonColors[i],
					Foreground = ButtonTextColor,
					BorderBrush = Brushes.Gray,
					BorderThickness = new Thickness(1),
					FontWeight = FontWeights.Bold,
					FontSize = 11
				};

				allButtons.Add(slButtonsArray[i]);
			}

			// Wire up SL button events
			if (slButtonsArray[0] != null) slButtonsArray[0].Click += (s, e) => MoveSL(SLButton1Ticks);
			if (slButtonsArray[1] != null) slButtonsArray[1].Click += (s, e) => MoveSL(SLButton2Ticks);
			if (slButtonsArray[2] != null) slButtonsArray[2].Click += (s, e) => MoveSL(SLButton3Ticks);
			if (slButtonsArray[3] != null) slButtonsArray[3].Click += (s, e) => MoveSLByPercent(SLButton4Percent);

			// Create BE buttons
			beButtonsArray = new System.Windows.Controls.Button[4];
			bool[] beButtonEnabled = new bool[] { BEButton1Enable, BEButton2Enable, BEButton3Enable, BEButton4Enable };
			int[] beButtonTicks = new int[] { BEButton1Ticks, BEButton2Ticks, BEButton3Ticks, BEButton4Ticks };
			Brush[] beButtonColors = new Brush[] { BEButton1Color, BEButton2Color, BEButton3Color, BEButton4Color };

			for (int i = 0; i < 4; i++)
			{
				if (!beButtonEnabled[i])
					continue;

				beButtonsArray[i] = new System.Windows.Controls.Button
				{
					Content = $"BE +{beButtonTicks[i]}",
					Height = 30,
					Margin = new Thickness(0),
					Padding = new Thickness(5, 0, 5, 0),
					Background = beButtonColors[i],
					Foreground = ButtonTextColor,
					BorderBrush = Brushes.Gray,
					BorderThickness = new Thickness(1),
					FontWeight = FontWeights.Bold,
					FontSize = 11
				};

				allButtons.Add(beButtonsArray[i]);

				int ticksToMove = beButtonTicks[i];
				beButtonsArray[i].Click += (s, e) => StopsToBreakeven(ticksToMove);
			}

			// Place all buttons in 2-column layout
			for (int i = 0; i < allButtons.Count; i++)
			{
				int row = i / 2;
				int col = i % 2;
				System.Windows.Controls.Grid.SetRow(allButtons[i], row);
				System.Windows.Controls.Grid.SetColumn(allButtons[i], col);
				buttonPanel.Children.Add(allButtons[i]);
			}

			// Create Flatten button at the bottom spanning 2 columns
			if (ShowFlattenButton)
			{
				flattenButton = new System.Windows.Controls.Button
				{
					Content = "Flatten",
					Height = 30,
					Margin = new Thickness(0),
					Padding = new Thickness(5, 0, 5, 0),
					Background = FlattenButtonColor,
					Foreground = ButtonTextColor,
					BorderBrush = Brushes.Gray,
					BorderThickness = new Thickness(1),
					FontWeight = FontWeights.Bold,
					FontSize = 11
				};

				System.Windows.Controls.Grid.SetRow(flattenButton, buttonRows);
				System.Windows.Controls.Grid.SetColumn(flattenButton, 0);
				System.Windows.Controls.Grid.SetColumnSpan(flattenButton, 2);

				buttonPanel.Children.Add(flattenButton);
				flattenButton.Click += FlattenButtonClick;
			}

			// Create the draggable handle
			dragHandle = new System.Windows.Controls.Border
			{
				Height = 20,
				Background = HandleColor,
				BorderBrush = Brushes.DarkGray,
				BorderThickness = new Thickness(1),
				Cursor = Cursors.SizeAll,
				Opacity = 0.8
			};

			// Add text to the handle
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

			// Handle events
			dragHandle.MouseLeftButtonDown += OnHandleMouseDown;
			dragHandle.MouseMove += OnHandleMouseMove;
			dragHandle.MouseLeftButtonUp += OnHandleMouseUp;

			// Add to canvas
			System.Windows.Controls.Canvas.SetLeft(dragHandle, 0);
			System.Windows.Controls.Canvas.SetTop(dragHandle, 0);
			System.Windows.Controls.Canvas.SetLeft(buttonPanel, 0);
			System.Windows.Controls.Canvas.SetTop(buttonPanel, 20);

			containerCanvas.Children.Add(dragHandle);
			containerCanvas.Children.Add(buttonPanel);

			// Set handle width to match button panel
			buttonPanel.Loaded += (s, e) =>
			{
				dragHandle.Width = buttonPanel.ActualWidth;
			};
			buttonPanel.SizeChanged += (s, e) =>
			{
				dragHandle.Width = buttonPanel.ActualWidth;
			};

			// Set initial position
			containerCanvas.Margin = new Thickness(10, 10, 0, 0);

			UserControlCollection.Add(containerCanvas);
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
					handleText.Text = "Move SL/BE";
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

		private void FlattenButtonClick(object sender, RoutedEventArgs e)
		{
			xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
			if (xAlselector == null) return;
			string currentAccount = xAlselector.SelectedAccount.ToString();
			
			Account Acct = Account.All.FirstOrDefault(x => currentAccount.Contains(x.Name));
			if (Acct == null) return;
			
			xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
			if (xInSelector == null) return;
			string currentInstrument = xInSelector.Instrument.ToString();
			
			List<Order> ordersToCancel = new List<Order>();
			foreach (Order order in Acct.Orders)
			{
				if (currentInstrument.Contains(order.Instrument.FullName) && 
				    order.OrderState != OrderState.Cancelled && 
				    order.OrderState != OrderState.Filled &&
				    order.OrderState != OrderState.Rejected)
				{
					ordersToCancel.Add(order);
				}
			}

			if (ordersToCancel.Count > 0)
			{
				Acct.Cancel(ordersToCancel.ToArray());
			}

			Position thisPosition = Acct.Positions.FirstOrDefault(x => currentInstrument.Contains(x.Instrument.FullName));
			if (thisPosition != null && thisPosition.MarketPosition != MarketPosition.Flat)
			{
				Acct.Flatten(new[] { thisPosition.Instrument });
			}
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

				if (flattenButton != null)
				{
					flattenButton.Click -= FlattenButtonClick;
				}

				if (slButtonsArray != null)
				{
					for (int i = 0; i < slButtonsArray.Length; i++)
					{
						if (slButtonsArray[i] != null)
							slButtonsArray[i].Click -= null;
					}
				}

				if (beButtonsArray != null)
				{
					for (int i = 0; i < beButtonsArray.Length; i++)
					{
						if (beButtonsArray[i] != null)
							beButtonsArray[i].Click -= null;
					}
				}
			}
		}

		private void MoveSL(int Ticks)
		{
			xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
			if (xAlselector == null) return;
			string currentAccount = xAlselector.SelectedAccount.ToString();
			
			Account Acct = Account.All.FirstOrDefault(x => currentAccount.Contains(x.Name));
			if (Acct == null) return;
			
			xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
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
								stopOrder.StopPriceChanged = order.StopPrice - Ticks * order.Instrument.MasterInstrument.TickSize;
								Acct.Change(new[] { stopOrder });
							}
							else if (thisPosition.MarketPosition == MarketPosition.Long)
							{
								stopOrder.StopPriceChanged = order.StopPrice + Ticks * order.Instrument.MasterInstrument.TickSize;
								Acct.Change(new[] { stopOrder });
							}
						}
					}
				}
			}
		}

		private void MoveSLByPercent(int Percent)
		{
			xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
			if (xAlselector == null) return;
			string currentAccount = xAlselector.SelectedAccount.ToString();
			
			Account Acct = Account.All.FirstOrDefault(x => currentAccount.Contains(x.Name));
			if (Acct == null) return;
			
			xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
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
							double percentMove = Percent / 100.0;
							
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

		private void StopsToBreakeven(int Ticks)
		{
			xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
			if (xAlselector == null) return;
			string currentAccount = xAlselector.SelectedAccount.ToString();
			
			Account Acct = Account.All.FirstOrDefault(x => currentAccount.Contains(x.Name));
			if (Acct == null) return;
			
			xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
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
								stopOrder.StopPriceChanged = thisPosition.AveragePrice - Ticks * order.Instrument.MasterInstrument.TickSize;
								Acct.Change(new[] { stopOrder });
							}
							else if (thisPosition.MarketPosition == MarketPosition.Long)
							{
								stopOrder.StopPriceChanged = thisPosition.AveragePrice + Ticks * order.Instrument.MasterInstrument.TickSize;
								Acct.Change(new[] { stopOrder });
							}
						}
					}
				}
			}
		}

		protected override void OnBarUpdate() { }

		#region Properties
		// SL Button Properties
		[NinjaScriptProperty]
		[Display(Name = "SL Button 1 Enable", Order = 1, GroupName = "SL Buttons")]
		public bool SLButton1Enable { get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "SL Button 1 Ticks", Order = 2, GroupName = "SL Buttons")]
		public int SLButton1Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 1 Color", Order = 3, GroupName = "SL Buttons")]
		public Brush SLButton1Color { get; set; }

		[Browsable(false)]
		public string SLButton1ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton1Color); }
			set { SLButton1Color = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "SL Button 2 Enable", Order = 4, GroupName = "SL Buttons")]
		public bool SLButton2Enable { get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "SL Button 2 Ticks", Order = 5, GroupName = "SL Buttons")]
		public int SLButton2Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 2 Color", Order = 6, GroupName = "SL Buttons")]
		public Brush SLButton2Color { get; set; }

		[Browsable(false)]
		public string SLButton2ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton2Color); }
			set { SLButton2Color = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "SL Button 3 Enable", Order = 7, GroupName = "SL Buttons")]
		public bool SLButton3Enable { get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "SL Button 3 Ticks", Order = 8, GroupName = "SL Buttons")]
		public int SLButton3Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 3 Color", Order = 9, GroupName = "SL Buttons")]
		public Brush SLButton3Color { get; set; }

		[Browsable(false)]
		public string SLButton3ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton3Color); }
			set { SLButton3Color = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "SL Button 4 Enable", Order = 10, GroupName = "SL Buttons")]
		public bool SLButton4Enable { get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name = "SL Button 4 Percent", Order = 11, GroupName = "SL Buttons")]
		public int SLButton4Percent { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "SL Button 4 Color", Order = 12, GroupName = "SL Buttons")]
		public Brush SLButton4Color { get; set; }

		[Browsable(false)]
		public string SLButton4ColorSerializable
		{
			get { return Serialize.BrushToString(SLButton4Color); }
			set { SLButton4Color = Serialize.StringToBrush(value); }
		}

		// BE Button Properties
		[NinjaScriptProperty]
		[Display(Name = "BE Button 1 Enable", Order = 1, GroupName = "BE Buttons")]
		public bool BEButton1Enable { get; set; }

		[NinjaScriptProperty]
		[Range(-20, int.MaxValue)]
		[Display(Name = "BE Button 1 Ticks", Order = 2, GroupName = "BE Buttons")]
		public int BEButton1Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "BE Button 1 Color", Order = 3, GroupName = "BE Buttons")]
		public Brush BEButton1Color { get; set; }

		[Browsable(false)]
		public string BEButton1ColorSerializable
		{
			get { return Serialize.BrushToString(BEButton1Color); }
			set { BEButton1Color = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "BE Button 2 Enable", Order = 4, GroupName = "BE Buttons")]
		public bool BEButton2Enable { get; set; }

		[NinjaScriptProperty]
		[Range(-20, int.MaxValue)]
		[Display(Name = "BE Button 2 Ticks", Order = 5, GroupName = "BE Buttons")]
		public int BEButton2Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "BE Button 2 Color", Order = 6, GroupName = "BE Buttons")]
		public Brush BEButton2Color { get; set; }

		[Browsable(false)]
		public string BEButton2ColorSerializable
		{
			get { return Serialize.BrushToString(BEButton2Color); }
			set { BEButton2Color = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "BE Button 3 Enable", Order = 7, GroupName = "BE Buttons")]
		public bool BEButton3Enable { get; set; }

		[NinjaScriptProperty]
		[Range(-20, int.MaxValue)]
		[Display(Name = "BE Button 3 Ticks", Order = 8, GroupName = "BE Buttons")]
		public int BEButton3Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "BE Button 3 Color", Order = 9, GroupName = "BE Buttons")]
		public Brush BEButton3Color { get; set; }

		[Browsable(false)]
		public string BEButton3ColorSerializable
		{
			get { return Serialize.BrushToString(BEButton3Color); }
			set { BEButton3Color = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "BE Button 4 Enable", Order = 10, GroupName = "BE Buttons")]
		public bool BEButton4Enable { get; set; }

		[NinjaScriptProperty]
		[Range(-20, int.MaxValue)]
		[Display(Name = "BE Button 4 Ticks", Order = 11, GroupName = "BE Buttons")]
		public int BEButton4Ticks { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "BE Button 4 Color", Order = 12, GroupName = "BE Buttons")]
		public Brush BEButton4Color { get; set; }

		[Browsable(false)]
		public string BEButton4ColorSerializable
		{
			get { return Serialize.BrushToString(BEButton4Color); }
			set { BEButton4Color = Serialize.StringToBrush(value); }
		}

		// Flatten Button Properties
		[NinjaScriptProperty]
		[Display(Name = "Show Flatten Button", Order = 1, GroupName = "Flatten Button")]
		public bool ShowFlattenButton { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Flatten Button Color", Order = 2, GroupName = "Flatten Button")]
		public Brush FlattenButtonColor { get; set; }

		[Browsable(false)]
		public string FlattenButtonColorSerializable
		{
			get { return Serialize.BrushToString(FlattenButtonColor); }
			set { FlattenButtonColor = Serialize.StringToBrush(value); }
		}

		// Appearance Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Button Text Color", Order = 1, GroupName = "Appearance")]
		public Brush ButtonTextColor { get; set; }

		[Browsable(false)]
		public string ButtonTextColorSerializable
		{
			get { return Serialize.BrushToString(ButtonTextColor); }
			set { ButtonTextColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Handle Color", Order = 2, GroupName = "Appearance")]
		public Brush HandleColor { get; set; }

		[Browsable(false)]
		public string HandleColorSerializable
		{
			get { return Serialize.BrushToString(HandleColor); }
			set { HandleColor = Serialize.StringToBrush(value); }
		}
		#endregion
	}
}


#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BESLChartPanel[] cacheBESLChartPanel;
		public BESLChartPanel BESLChartPanel(bool sLButton1Enable, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enable, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enable, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enable, int sLButton4Percent, Brush sLButton4Color, bool bEButton1Enable, int bEButton1Ticks, Brush bEButton1Color, bool bEButton2Enable, int bEButton2Ticks, Brush bEButton2Color, bool bEButton3Enable, int bEButton3Ticks, Brush bEButton3Color, bool bEButton4Enable, int bEButton4Ticks, Brush bEButton4Color, bool showFlattenButton, Brush flattenButtonColor, Brush buttonTextColor, Brush handleColor)
		{
			return BESLChartPanel(Input, sLButton1Enable, sLButton1Ticks, sLButton1Color, sLButton2Enable, sLButton2Ticks, sLButton2Color, sLButton3Enable, sLButton3Ticks, sLButton3Color, sLButton4Enable, sLButton4Percent, sLButton4Color, bEButton1Enable, bEButton1Ticks, bEButton1Color, bEButton2Enable, bEButton2Ticks, bEButton2Color, bEButton3Enable, bEButton3Ticks, bEButton3Color, bEButton4Enable, bEButton4Ticks, bEButton4Color, showFlattenButton, flattenButtonColor, buttonTextColor, handleColor);
		}

		public BESLChartPanel BESLChartPanel(ISeries<double> input, bool sLButton1Enable, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enable, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enable, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enable, int sLButton4Percent, Brush sLButton4Color, bool bEButton1Enable, int bEButton1Ticks, Brush bEButton1Color, bool bEButton2Enable, int bEButton2Ticks, Brush bEButton2Color, bool bEButton3Enable, int bEButton3Ticks, Brush bEButton3Color, bool bEButton4Enable, int bEButton4Ticks, Brush bEButton4Color, bool showFlattenButton, Brush flattenButtonColor, Brush buttonTextColor, Brush handleColor)
		{
			if (cacheBESLChartPanel != null)
				for (int idx = 0; idx < cacheBESLChartPanel.Length; idx++)
					if (cacheBESLChartPanel[idx] != null && cacheBESLChartPanel[idx].SLButton1Enable == sLButton1Enable && cacheBESLChartPanel[idx].SLButton1Ticks == sLButton1Ticks && cacheBESLChartPanel[idx].SLButton1Color == sLButton1Color && cacheBESLChartPanel[idx].SLButton2Enable == sLButton2Enable && cacheBESLChartPanel[idx].SLButton2Ticks == sLButton2Ticks && cacheBESLChartPanel[idx].SLButton2Color == sLButton2Color && cacheBESLChartPanel[idx].SLButton3Enable == sLButton3Enable && cacheBESLChartPanel[idx].SLButton3Ticks == sLButton3Ticks && cacheBESLChartPanel[idx].SLButton3Color == sLButton3Color && cacheBESLChartPanel[idx].SLButton4Enable == sLButton4Enable && cacheBESLChartPanel[idx].SLButton4Percent == sLButton4Percent && cacheBESLChartPanel[idx].SLButton4Color == sLButton4Color && cacheBESLChartPanel[idx].BEButton1Enable == bEButton1Enable && cacheBESLChartPanel[idx].BEButton1Ticks == bEButton1Ticks && cacheBESLChartPanel[idx].BEButton1Color == bEButton1Color && cacheBESLChartPanel[idx].BEButton2Enable == bEButton2Enable && cacheBESLChartPanel[idx].BEButton2Ticks == bEButton2Ticks && cacheBESLChartPanel[idx].BEButton2Color == bEButton2Color && cacheBESLChartPanel[idx].BEButton3Enable == bEButton3Enable && cacheBESLChartPanel[idx].BEButton3Ticks == bEButton3Ticks && cacheBESLChartPanel[idx].BEButton3Color == bEButton3Color && cacheBESLChartPanel[idx].BEButton4Enable == bEButton4Enable && cacheBESLChartPanel[idx].BEButton4Ticks == bEButton4Ticks && cacheBESLChartPanel[idx].BEButton4Color == bEButton4Color && cacheBESLChartPanel[idx].ShowFlattenButton == showFlattenButton && cacheBESLChartPanel[idx].FlattenButtonColor == flattenButtonColor && cacheBESLChartPanel[idx].ButtonTextColor == buttonTextColor && cacheBESLChartPanel[idx].HandleColor == handleColor && cacheBESLChartPanel[idx].EqualsInput(input))
						return cacheBESLChartPanel[idx];
			return CacheIndicator<BESLChartPanel>(new BESLChartPanel(){ SLButton1Enable = sLButton1Enable, SLButton1Ticks = sLButton1Ticks, SLButton1Color = sLButton1Color, SLButton2Enable = sLButton2Enable, SLButton2Ticks = sLButton2Ticks, SLButton2Color = sLButton2Color, SLButton3Enable = sLButton3Enable, SLButton3Ticks = sLButton3Ticks, SLButton3Color = sLButton3Color, SLButton4Enable = sLButton4Enable, SLButton4Percent = sLButton4Percent, SLButton4Color = sLButton4Color, BEButton1Enable = bEButton1Enable, BEButton1Ticks = bEButton1Ticks, BEButton1Color = bEButton1Color, BEButton2Enable = bEButton2Enable, BEButton2Ticks = bEButton2Ticks, BEButton2Color = bEButton2Color, BEButton3Enable = bEButton3Enable, BEButton3Ticks = bEButton3Ticks, BEButton3Color = bEButton3Color, BEButton4Enable = bEButton4Enable, BEButton4Ticks = bEButton4Ticks, BEButton4Color = bEButton4Color, ShowFlattenButton = showFlattenButton, FlattenButtonColor = flattenButtonColor, ButtonTextColor = buttonTextColor, HandleColor = handleColor }, input, ref cacheBESLChartPanel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BESLChartPanel BESLChartPanel(bool sLButton1Enable, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enable, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enable, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enable, int sLButton4Percent, Brush sLButton4Color, bool bEButton1Enable, int bEButton1Ticks, Brush bEButton1Color, bool bEButton2Enable, int bEButton2Ticks, Brush bEButton2Color, bool bEButton3Enable, int bEButton3Ticks, Brush bEButton3Color, bool bEButton4Enable, int bEButton4Ticks, Brush bEButton4Color, bool showFlattenButton, Brush flattenButtonColor, Brush buttonTextColor, Brush handleColor)
		{
			return indicator.BESLChartPanel(Input, sLButton1Enable, sLButton1Ticks, sLButton1Color, sLButton2Enable, sLButton2Ticks, sLButton2Color, sLButton3Enable, sLButton3Ticks, sLButton3Color, sLButton4Enable, sLButton4Percent, sLButton4Color, bEButton1Enable, bEButton1Ticks, bEButton1Color, bEButton2Enable, bEButton2Ticks, bEButton2Color, bEButton3Enable, bEButton3Ticks, bEButton3Color, bEButton4Enable, bEButton4Ticks, bEButton4Color, showFlattenButton, flattenButtonColor, buttonTextColor, handleColor);
		}

		public Indicators.BESLChartPanel BESLChartPanel(ISeries<double> input , bool sLButton1Enable, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enable, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enable, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enable, int sLButton4Percent, Brush sLButton4Color, bool bEButton1Enable, int bEButton1Ticks, Brush bEButton1Color, bool bEButton2Enable, int bEButton2Ticks, Brush bEButton2Color, bool bEButton3Enable, int bEButton3Ticks, Brush bEButton3Color, bool bEButton4Enable, int bEButton4Ticks, Brush bEButton4Color, bool showFlattenButton, Brush flattenButtonColor, Brush buttonTextColor, Brush handleColor)
		{
			return indicator.BESLChartPanel(input, sLButton1Enable, sLButton1Ticks, sLButton1Color, sLButton2Enable, sLButton2Ticks, sLButton2Color, sLButton3Enable, sLButton3Ticks, sLButton3Color, sLButton4Enable, sLButton4Percent, sLButton4Color, bEButton1Enable, bEButton1Ticks, bEButton1Color, bEButton2Enable, bEButton2Ticks, bEButton2Color, bEButton3Enable, bEButton3Ticks, bEButton3Color, bEButton4Enable, bEButton4Ticks, bEButton4Color, showFlattenButton, flattenButtonColor, buttonTextColor, handleColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BESLChartPanel BESLChartPanel(bool sLButton1Enable, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enable, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enable, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enable, int sLButton4Percent, Brush sLButton4Color, bool bEButton1Enable, int bEButton1Ticks, Brush bEButton1Color, bool bEButton2Enable, int bEButton2Ticks, Brush bEButton2Color, bool bEButton3Enable, int bEButton3Ticks, Brush bEButton3Color, bool bEButton4Enable, int bEButton4Ticks, Brush bEButton4Color, bool showFlattenButton, Brush flattenButtonColor, Brush buttonTextColor, Brush handleColor)
		{
			return indicator.BESLChartPanel(Input, sLButton1Enable, sLButton1Ticks, sLButton1Color, sLButton2Enable, sLButton2Ticks, sLButton2Color, sLButton3Enable, sLButton3Ticks, sLButton3Color, sLButton4Enable, sLButton4Percent, sLButton4Color, bEButton1Enable, bEButton1Ticks, bEButton1Color, bEButton2Enable, bEButton2Ticks, bEButton2Color, bEButton3Enable, bEButton3Ticks, bEButton3Color, bEButton4Enable, bEButton4Ticks, bEButton4Color, showFlattenButton, flattenButtonColor, buttonTextColor, handleColor);
		}

		public Indicators.BESLChartPanel BESLChartPanel(ISeries<double> input , bool sLButton1Enable, int sLButton1Ticks, Brush sLButton1Color, bool sLButton2Enable, int sLButton2Ticks, Brush sLButton2Color, bool sLButton3Enable, int sLButton3Ticks, Brush sLButton3Color, bool sLButton4Enable, int sLButton4Percent, Brush sLButton4Color, bool bEButton1Enable, int bEButton1Ticks, Brush bEButton1Color, bool bEButton2Enable, int bEButton2Ticks, Brush bEButton2Color, bool bEButton3Enable, int bEButton3Ticks, Brush bEButton3Color, bool bEButton4Enable, int bEButton4Ticks, Brush bEButton4Color, bool showFlattenButton, Brush flattenButtonColor, Brush buttonTextColor, Brush handleColor)
		{
			return indicator.BESLChartPanel(input, sLButton1Enable, sLButton1Ticks, sLButton1Color, sLButton2Enable, sLButton2Ticks, sLButton2Color, sLButton3Enable, sLButton3Ticks, sLButton3Color, sLButton4Enable, sLButton4Percent, sLButton4Color, bEButton1Enable, bEButton1Ticks, bEButton1Color, bEButton2Enable, bEButton2Ticks, bEButton2Color, bEButton3Enable, bEButton3Ticks, bEButton3Color, bEButton4Enable, bEButton4Ticks, bEButton4Color, showFlattenButton, flattenButtonColor, buttonTextColor, handleColor);
		}
	}
}

#endregion
