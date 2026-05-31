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
	public class TradeFromChart : Indicator
	{
		public override string DisplayName
		{
			get
			{
				return "Trade From Chart";
			}
		}
		private Account myAccount;
		private ChartScale MyChartScale;
		private Order myOrder;
		private int myQuantity; 
		
		private bool buyButton = false;
		private bool sellButton = false;
		
		private OrderPlacementHotKey buyKey = OrderPlacementHotKey.LeftShift;
		private OrderPlacementHotKey sellKey = OrderPlacementHotKey.LeftAlt;
		
		private Key kbuyKey;
		private Key ksellKey;
		
		private StopOrderType stopOrderType = StopOrderType.StopMarket;
		private OrderType orderType;
		
		private bool tradingFromChart = false;
		private ButtonLocationTypes buttonLocation = ButtonLocationTypes.ChartTrader;
		
		#region Chart Trader Buttons
		
		private System.Windows.Controls.RowDefinition	addedRow;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private System.Windows.Controls.Grid			chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid;
		private System.Windows.Controls.Button			activateButton1;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;
		
		private bool myButtonClicked = false;
		private bool isToolBarButtonAdded = false;
		
		#endregion

		protected override void OnStateChange()
		{

			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "TradeFromChart";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;

			}
		 	else if (State == State.DataLoaded)
  			{
				if (ChartControl != null)
				ChartControl.MouseLeftButtonDown += LeftMouseDown;
				
				if (ChartControl != null)
					ChartPanel.MouseMove += ChartControl_MouseMove;
				
				
				Dispatcher.Invoke((() =>
				{
					ChartPanel.PreviewKeyDown += ChartPanel_PreviewKeyDown;
					ChartPanel.PreviewKeyUp += ChartPanel_PreviewKeyUp;
				}));
				
				switch (buyKey)
				{
					case OrderPlacementHotKey.LeftShift:
						kbuyKey = Key.LeftShift;
						break;
					
					case OrderPlacementHotKey.LeftAlt:
						kbuyKey = Key.LeftAlt;
						break;
						
					case OrderPlacementHotKey.RightAlt:
						kbuyKey = Key.RightAlt;
						break;
						
					case OrderPlacementHotKey.RightShift:
						kbuyKey = Key.RightShift;
						break;
				}
				
				switch (sellKey)
				{
					case OrderPlacementHotKey.LeftShift:
						ksellKey = Key.LeftShift;
						break;
					
					case OrderPlacementHotKey.LeftAlt:
						ksellKey = Key.LeftAlt;
						break;
						
					case OrderPlacementHotKey.RightAlt:
						ksellKey = Key.RightAlt;
						break;
						
					case OrderPlacementHotKey.RightShift:
						ksellKey = Key.RightShift;
						break;
				}
				
				if (stopOrderType == StopOrderType.StopLimit)
				{
					orderType = OrderType.StopLimit;
				}
				else
				{
					orderType = OrderType.StopMarket;
				}
   		}

			else if (State == State.Historical)
			{
				#region Buttons Load
				
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
				
				#endregion
			}

			else if (State == State.Terminated)
			{
				if (ChartControl != null)
					ChartControl.MouseLeftButtonDown -= LeftMouseDown;
				
				if (ChartControl != null)
					ChartPanel.MouseMove -= ChartControl_MouseMove;
				
				if (ChartPanel != null)
				{
					ChartPanel.PreviewKeyDown -= ChartPanel_PreviewKeyDown;
					ChartPanel.PreviewKeyUp -= ChartPanel_PreviewKeyUp;
				}

				#region Terminate
				
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
				
				#endregion
			}
		}

		#region Button Click Events
		protected void Button1Click(object sender, RoutedEventArgs e)
		{
			if (myButtonClicked == false)
			{
				activateButton1.Background		= Brushes.Green;
				if (buttonLocation == ButtonLocationTypes.ChartTrader)
					activateButton1.Content = "Trading from chart on";
				else
					activateButton1.Content = "TFC ON";
				tradingFromChart = true;
				myButtonClicked = true;
			}
			else
			{
				activateButton1.Background		= Brushes.Red;
				if (buttonLocation == ButtonLocationTypes.ChartTrader)
					activateButton1.Content = "Trading from chart off";
				else
					activateButton1.Content = "TFC OFF";
				tradingFromChart = false;
				myButtonClicked = false;
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
			activateButton1 = new System.Windows.Controls.Button()
			{
				Content = "TFC OFF",
				Height = 20,
				Width = 50,
				Margin = new Thickness(2),
				Padding = new Thickness(2),
				Background = Brushes.Red,
				Foreground = Brushes.White,
				BorderBrush = Brushes.Black,
				BorderThickness = new Thickness(1.0),
				FontSize = 10
			};
			
			activateButton1.Click += Button1Click;
			
			// Add to MainMenu (toolbar)
			chartWindow.MainMenu.Add(activateButton1);
			
			isToolBarButtonAdded = true;
		}
		
		private void RemoveButtonFromToolbar()
		{
			if (!isToolBarButtonAdded || chartWindow == null || activateButton1 == null)
				return;
				
			activateButton1.Click -= Button1Click;
			chartWindow.MainMenu.Remove(activateButton1);
			isToolBarButtonAdded = false;
		}
		
		#endregion
		
		protected void CreateWPFControls()
		{
			#region CreateWPFControls
			
				#region Button Grid
			
			chartWindow				= Window.GetWindow(ChartControl.Parent) as Gui.Chart.Chart;
			
			// if not added to a chart, do nothing
			if (chartWindow == null)
				return;
			

			chartTraderGrid			= (chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader).Content as System.Windows.Controls.Grid;

			// this grid contains the existing chart trader buttons
			chartTraderButtonsGrid	= chartTraderGrid.Children[0] as System.Windows.Controls.Grid;
			

			// Lower Grid - (Row1)Upper
			lowerButtonsGrid = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid, 1);
	
			//Columns * 1
			lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());			
			
			
			addedRow	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
				

			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= Application.Current.FindResource("BasicEntryButton") as Style;
	
			
			#endregion
			
			
				#region Button Content
				
				
					activateButton1 = new System.Windows.Controls.Button()//1
					{		
						
						Content			= "Trading from chart off",
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
			
				#endregion
					
	
				#region Button Colors
					
					//Row1
					activateButton1.Background		= Brushes.Red;
					activateButton1.BorderBrush		= Brushes.Black;	
					activateButton1.Foreground    	= Brushes.White;	
					activateButton1.BorderThickness = new Thickness(2.0);
			#endregion	
					
		
				#region Button Click 
				
					activateButton1.Click += Button1Click;
				
				#endregion	
					
					
				#region Button Location
		
					//Row 1
					System.Windows.Controls.Grid.SetColumn(activateButton1, 0);				
					System.Windows.Controls.Grid.SetRow(activateButton1, 0);	
				
				#endregion	
					
					
				
				#region Add Buttons 1
			
					lowerButtonsGrid.Children.Add(activateButton1);
				
				#endregion
					
					
					
					
            if (totalGrids == 0) 
				totalGrids = chartTraderGrid.RowDefinitions.Count;


			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
			#endregion
			
		}
		
		static int totalGrids;

        public void DisposeWPFControls() 
		{
			#region Dispose
			
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			
			//Row 1
			if (activateButton1 != null)
				activateButton1.Click -= Button1Click;
			
			RemoveWPFControls();
			
			#endregion
		}
		
		public void InsertWPFControls()
		{
			#region Insert WPF
			
			if (panelActive)
				return;
			
			
			// add a new row (addedRow) for our lowerButtonsGrid below the ask and bid prices and pnl display			
			chartTraderGrid.RowDefinitions.Add(addedRow);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid, totalGrids); 
			chartTraderGrid.Children.Add(lowerButtonsGrid);
			
			panelActive = true;
			
			#endregion	
		}
		
		private bool TabSelected()
		{
			#region TabSelected 
			
			if (ChartControl == null || chartWindow == null || chartWindow.MainTabControl == null )
				return false;
			
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
				
			#endregion
		}
		
		protected void RemoveWPFControls()
		{
			#region Remove WPF
			
			if (!panelActive)
				return;

			if (chartTraderButtonsGrid != null || lowerButtonsGrid != null)
			{
				chartTraderGrid.Children.Remove(lowerButtonsGrid);
				
				chartTraderGrid.RowDefinitions.Remove(addedRow);
			}
			
			panelActive = false;
			
			#endregion
		}
		
		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{	
			#region TabHandler
			
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
			
			#endregion
		}


		protected override void OnBarUpdate()
		{
			// nothing to do here
		}
		
		private void ChartPanel_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if ( Keyboard.IsKeyDown(kbuyKey))
			{
				buyButton = true;
			}
			
			if ( Keyboard.IsKeyDown(ksellKey))
			{
				sellButton = true;
			}
		}
		
		private void ChartPanel_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyUp(kbuyKey))
			{
				buyButton = false;
			}
			
			if (Keyboard.IsKeyUp(ksellKey))
			{
				sellButton = false;
			}
			
		}
		
		protected void LeftMouseDown(object sender, MouseButtonEventArgs e)
		{
			if((buyButton == true || sellButton == true) && tradingFromChart == true)
			{
				TriggerCustomEvent(o =>
				{
					int Y = ChartingExtensions.ConvertToVerticalPixels(e.GetPosition(ChartControl as IInputElement).Y, ChartControl.PresentationSource);
					
					double priceClicked = MyChartScale.GetValueByY(Y);
				
					// get account, quantity, and ATM of chart trader
					NinjaTrader.Gui.Tools.QuantityUpDown quantitySelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlQuantitySelector") as NinjaTrader.Gui.Tools.QuantityUpDown);
					myQuantity = quantitySelector.Value;
					
					NinjaTrader.Gui.Tools.AccountSelector accountSelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector);
					myAccount = accountSelector.SelectedAccount;
					
					NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector atmSelector = (Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector);
					
					
					
					if (buyButton == true)
					{
						if (priceClicked > Close[0])
						{	
							myOrder = myAccount.CreateOrder(Instrument, OrderAction.Buy, orderType, OrderEntry.Manual, TimeInForce.Day, myQuantity, priceClicked, priceClicked, "", "Entry", DateTime.MaxValue, null);
						}
						else if (priceClicked < Close[0])
						{
							myOrder = myAccount.CreateOrder(Instrument, OrderAction.Buy, OrderType.Limit, OrderEntry.Manual, TimeInForce.Day, myQuantity, priceClicked, 0, "", "Entry", DateTime.MaxValue, null);
						}
					}
					
					if (sellButton == true)
					{
						if (priceClicked < Close[0])
						{
							myOrder = myAccount.CreateOrder(Instrument, OrderAction.Sell, orderType, OrderEntry.Manual, TimeInForce.Day, myQuantity, priceClicked, priceClicked, "", "Entry", DateTime.MaxValue, null);
						}
						else if (priceClicked > Close[0])
						{
							myOrder = myAccount.CreateOrder(Instrument, OrderAction.Sell, OrderType.Limit, OrderEntry.Manual, TimeInForce.Day, myQuantity, priceClicked, 0, "", "Entry", DateTime.MaxValue, null);
						}
					}
					
					if (atmSelector.SelectedAtmStrategy != null)
					{
						NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmSelector.SelectedAtmStrategy, myOrder);
					}
					
					myAccount.Submit(new[] { myOrder});
					
				}, null);

				
				e.Handled = true;
			}
		}
		
		
		#region MouseMove And Render
		void ChartControl_MouseMove (object sender, System.Windows.Input.MouseEventArgs e)//DO NOT REMOVE
		{

		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			MyChartScale = chartScale;
		}
		
		#endregion
		
		#region Properties
		[Display(Name="Buy hotkey", Order=1,GroupName = "Hotkeys", Description="Choose a key binding for buys")]
		public OrderPlacementHotKey BuyKey
		{
			get { return buyKey; }
			set { buyKey = value; }
		}
		
		[Display(Name="Sell hotkey", Order=2,GroupName = "Hotkeys", Description="Choose a key binding for sells")]
		public OrderPlacementHotKey SellKey
		{
			get { return sellKey; }
			set { sellKey = value; }
		}
		
		[Display(Name="Stop order type", Order=1,GroupName = "Order management", Description="Choose the type of stop order")]
		public StopOrderType StopOrderType
		{
			get { return stopOrderType; }
			set { stopOrderType = value; }
		}
		
		[Display(Name="Button location", Order=1,GroupName = "UI Settings", Description="Choose where to place the button")]
		public ButtonLocationTypes ButtonLocation
		{
			get { return buttonLocation; }
			set { buttonLocation = value; }
		}
		
		#endregion
	}
}

public enum OrderPlacementHotKey
{
	LeftAlt,
	LeftShift,
	RightAlt,
	RightShift
}

public enum StopOrderType
{
	StopLimit,
	StopMarket
}

public enum ButtonLocationTypes
{
	ChartTrader,
	Toolbar
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeFromChart[] cacheTradeFromChart;
		public TradeFromChart TradeFromChart()
		{
			return TradeFromChart(Input);
		}

		public TradeFromChart TradeFromChart(ISeries<double> input)
		{
			if (cacheTradeFromChart != null)
				for (int idx = 0; idx < cacheTradeFromChart.Length; idx++)
					if (cacheTradeFromChart[idx] != null &&  cacheTradeFromChart[idx].EqualsInput(input))
						return cacheTradeFromChart[idx];
			return CacheIndicator<TradeFromChart>(new TradeFromChart(), input, ref cacheTradeFromChart);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeFromChart TradeFromChart()
		{
			return indicator.TradeFromChart(Input);
		}

		public Indicators.TradeFromChart TradeFromChart(ISeries<double> input )
		{
			return indicator.TradeFromChart(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeFromChart TradeFromChart()
		{
			return indicator.TradeFromChart(Input);
		}

		public Indicators.TradeFromChart TradeFromChart(ISeries<double> input )
		{
			return indicator.TradeFromChart(input);
		}
	}
}

#endregion
