#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
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

namespace NinjaTrader.NinjaScript.Strategies
{
    public class KingKhanhATMStrategyUI : Strategy, ICustomTypeDescriptor //
    {
		// Variables for ATM strategy
		private string  atmStrategyId			= string.Empty;
		private string  orderId					= string.Empty;
		private bool	isAtmStrategyCreated	= false;
		
        private RegChannel RegChannel1;
        private RegressionChannelHighLow RegressionChannelHighLow1;
        private LinReg LinReg1;
        private T3TrendFilter T3TrendFilter1;
        private ADX ADX1;
        private double Up;
        private double Down;
		
		private bool isBuySellMarketOrder;
		private bool tradesPerDirection;	
		private bool isInLong;
		private bool isInShort;
		private int counterLong;	
		private int counterShort;	

		private bool isEnableTime1;
		private bool isEnableTime2;	
		private bool isEnableTime3;	
		private bool isEnableTime4;	
		private bool isEnableTime5;	
		private bool isEnableTime6;			
		private bool isTimeInFirstRange;
		private bool isTimeInSecondRange;
		private bool isTimeInThirdRange ;
		private bool isTimeInFourthRange;
		private bool isTimeInFifthRange;
		private bool isTimeInSixthRange;
		private bool isStrategyEnabled;
		private bool isLongEnabled;
		private bool isShortEnabled;
		
		
//		Chart Trader Buttons
		private System.Windows.Controls.RowDefinition	addedRow;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private System.Windows.Controls.Grid			chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid;
		
//		New Toggle Buttons
		private System.Windows.Controls.Button			useAutoBtn;
		private System.Windows.Controls.Button			useLongBtn, useShortBtn;
		private System.Windows.Controls.Button			closeBtn;
		private System.Windows.Controls.Button			panicBtn;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;
		private System.Windows.Controls.Grid 			myGrid;
		
		
//		KillAll 
		private Instrument inst;
		private Account chartTraderAccount;
		private AccountSelector accountSelector;
		
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
		
        #region Properties
		
		[Display(Name = "Order Selector", Description= "Select order type to enter the strategy", Order = 4, GroupName = "01. Order Management")]
		[RefreshProperties(RefreshProperties.All)]
		public orderSelector OrderSelector
		{
			get { return showOrder; }
			set
			{
				showOrder = value;
				if (showOrder == orderSelector.Limit_Order)
				{
					isBuySellMarketOrder = false;
//					Print("Limit Order active");
				}
				else if (showOrder == orderSelector.Market_Order)
				{
					isBuySellMarketOrder  = true;
//					Print("Market Order active");
				}
			}			
		}
		
		//ProfitLimit and LossLimit
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Profit Limit ($)", Description="No positive or negative sign, just integer", Order=5, GroupName="01. Order Management")]
		public double DailyProfitLimit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Loss Limit ($)", Description="No positive or negative sign, just integer", Order=6, GroupName="01. Order Management")]
		public double DailyLossLimit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Trades Per Direction", Description = "Switch off Historical Trades to use this option.", Order = 0, GroupName = "02. Trades Per Direction")]
		[RefreshProperties(RefreshProperties.All)]
		public bool TradesPerDirection 
		{
		 	get{return tradesPerDirection;} 
			set{tradesPerDirection = (value);} 
		}
		
		[NinjaScriptProperty]
		[Display(Name="Long Per Direction", Description = "Number of long in a row", Order = 1, GroupName = "02. Trades Per Direction")]
		public int longPerDirection
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Short Per Direction", Description = "Number of short in a row", Order = 2, GroupName = "02. Trades Per Direction")]
		public int shortPerDirection
		{ get; set; }	
		
		#region 6. Time Valid
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Trades", Order=1, GroupName="03. Time Frames")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Trades", Order=2, GroupName="03. Time Frames")]
		public DateTime End
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 2", Description = "Enable 2 times.", Order=3, GroupName = "03. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time2
		{
		 	get{return isEnableTime2;} 
			set{isEnableTime2 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 2", Order=4, GroupName="03. Time Frames")]
		public DateTime Start2
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 2", Order=5, GroupName="03. Time Frames")]
		public DateTime End2
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 3", Description = "Enable 3 times.", Order=6, GroupName = "03. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time3
		{
		 	get{return isEnableTime3;} 
			set{isEnableTime3 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 3", Order=7, GroupName="03. Time Frames")]
		public DateTime Start3
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 3", Order=8, GroupName="03. Time Frames")]
		public DateTime End3
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 4", Description = "Enable 4 times.", Order=9, GroupName = "03. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time4
		{
		 	get{return isEnableTime4;} 
			set{isEnableTime4 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 4", Order=10, GroupName="03. Time Frames")]
		public DateTime Start4
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 4", Order=11, GroupName="03. Time Frames")]
		public DateTime End4
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 5", Description = "Enable 5 times.", Order=12, GroupName = "03. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time5
		{
		 	get{return isEnableTime5;} 
			set{isEnableTime5 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 5", Order=13, GroupName="03. Time Frames")]
		public DateTime Start5
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 5", Order=14, GroupName="03. Time Frames")]
		public DateTime End5
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Time 6", Description = "Enable 6 times.", Order =15, GroupName = "03. Time Frames")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Time6
		{
		 	get{return isEnableTime6;} 
			set{isEnableTime6 = (value);} 
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time 6", Order=16, GroupName="03. Time Frames")]
		public DateTime Start6
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time 6", Order=17, GroupName="03. Time Frames")]
		public DateTime End6
		{ get; set; }
		
		#endregion
		
		[NinjaScriptProperty]
		[Display(Name="ADXPeriod", Order=3, GroupName="04. Filters Settings")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXThreshold", Order=4, GroupName="04. Filters Settings")]
		public int ADXThreshold
		{ get; set; }
				
		
		
		#region 8. Custom Colors
		
        [NinjaScriptProperty]
        [Display(GroupName = "STATUS PANEL", Name = "Show STATUS PANEL ?", Order = 0)]
        public bool showPnl { get; set; }		
		

		[NinjaScriptProperty]
		[Display(Name="STATUS PANEl Position", Description = "PNL Position", Order = 1, GroupName = "STATUS PANEL")]
		public TextPosition PositionPnl		
		{ get; set; }				
		
		[XmlIgnore()]
		[Display(Name = "STATUS PANEl Color", Order = 1, GroupName = "STATUS PANEL")]
		public Brush colorPnl
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string colorPnlSerialize
		{
			get { return Serialize.BrushToString(colorPnl); }
   			set { colorPnl = Serialize.StringToBrush(value); }
		}
		#endregion		
		
		[NinjaScriptProperty]
        [Display(Name = "ATMStrategyTemplate", Order = 1, GroupName = "Parameters")]
        public string ATMStrategyTemplate { get; set; }
		
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"This is a strategy based on the linear regression channel.";
                Name = "KingKhanhATMStrategyUI";
                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 2;					// This value should limit the number of contracts that the strategy can open per direction.
															// It has nothing to do with the parameter defining the entries per direction that we define in the strategy and are controlled by code.
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
				isStrategyEnabled 				= true;
				isLongEnabled					= true;
				isShortEnabled					= true;
				
				ATMStrategyTemplate				= "KingKhanh";
				ADXPeriod						= 4;
                ADXThreshold			 		= 50;
				
				longPerDirection				= 2;
				shortPerDirection				= 2;
				
				counterLong						= 0;
				counterShort					= 0;
				
				Start							= DateTime.Parse("06:40", System.Globalization.CultureInfo.InvariantCulture);
				End								= DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
				Start2							= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				End2							= DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
				Start3							= DateTime.Parse("09:00", System.Globalization.CultureInfo.InvariantCulture);
				End3							= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				Start4							= DateTime.Parse("12:15", System.Globalization.CultureInfo.InvariantCulture);
				End4							= DateTime.Parse("13:30", System.Globalization.CultureInfo.InvariantCulture);
				Start5							= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End5							= DateTime.Parse("01:30", System.Globalization.CultureInfo.InvariantCulture);
				Start6							= DateTime.Parse("15:00", System.Globalization.CultureInfo.InvariantCulture);
				End6							= DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
				
				// Panel Status
				showPnl							= true;
				PositionPnl						= TextPosition.TopRight;
				colorPnl 						= Brushes.AliceBlue; // Default value
				// PnL Daily Limits
				DailyProfitLimit				= 10000;
				DailyLossLimit					= 1500;
				
            }
            else if (State == State.Configure)
            {
                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
            }
            else if (State == State.DataLoaded)
            {
                RegChannel1 = RegChannel(Close, 30, 0.9);
                RegressionChannelHighLow1 = RegressionChannelHighLow(Close, 30, 3.5);
                LinReg1 = LinReg(Close, 9);
                T3TrendFilter1 = T3TrendFilter(Close, 0.7, 8, 11, 14, 17, 20, false);
                ADX1 = ADX(Close, ADXPeriod);
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
			}
        }
		
		#region DecoreButton
		
		protected void DecoreDisabledButtons(System.Windows.Controls.Button myButton, string stringButton)
		{
			myButton.Content = stringButton;
			myButton.Background = Brushes.Firebrick;
			myButton.BorderBrush = Brushes.Black;
			myButton.Foreground = Brushes.White;
			return;
		}

		protected void DecoreEnabledButtons(System.Windows.Controls.Button myButton, string stringButton)
		{
			myButton.Content = stringButton;
			myButton.Background = Brushes.ForestGreen;
			myButton.BorderBrush = Brushes.Black;
			myButton.Foreground = Brushes.Black;
			return;
		}

		protected void DecoreNeutralButtons(System.Windows.Controls.Button myButton, string stringButton)
		{
			myButton.Content = stringButton;
			myButton.Background = Brushes.LightGray;
			myButton.BorderBrush = Brushes.Black;
			myButton.Foreground = Brushes.Black;
			return;
		}

		protected void DecoreGrayButtons(System.Windows.Controls.Button myButton, string stringButton)
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

			addedRow	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(165) };
			
    		// SetButtons
    		SetButtonLocations();

    		// AddButtons
    		AddButtonsToGrid();			
				
			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;

		}
		#endregion
		
		private void CreateButtons()
		{	
					
			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= System.Windows.Application.Current.FindResource("BasicEntryButton") as Style;			
	
			useAutoBtn = new System.Windows.Controls.Button
			{		
				Content			= "\uD83D\uDD12 Strat Enabled", Height = 25, Margin = new Thickness(1,0,1,0),	Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Strategy"
			};	
			if (isStrategyEnabled) DecoreEnabledButtons(useAutoBtn, "\uD83D\uDD12 Strat Enabled");
			if (!isStrategyEnabled) DecoreDisabledButtons(useAutoBtn, "\uD83D\uDD13 Strat Disabled");
			useAutoBtn.Click +=  OnButtonClick;
			
			useLongBtn = new System.Windows.Controls.Button
			{		
				Content			= "LONG Enabled", Height = 25, Margin = new Thickness(1,0,1,0), Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Auto Long Entry"
			};	
			if (isLongEnabled) DecoreEnabledButtons(useLongBtn, "LONG Enabled");
			if (!isLongEnabled) DecoreDisabledButtons(useLongBtn, "LONG Disabled");	
			useLongBtn.Click += OnButtonClick;
			
			useShortBtn = new System.Windows.Controls.Button
			{		
				Content			= "SHORT Enabled", Height = 25, Margin	= new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Auto Short Entry"
			};	
			if (isShortEnabled) DecoreEnabledButtons(useShortBtn, "SHORT Enabled");
			if (!isShortEnabled) 	DecoreDisabledButtons(useShortBtn, "SHORT Disabled");	
			useShortBtn.Click += OnButtonClick;			
			
			panicBtn = new System.Windows.Controls.Button
			{
				Name = "PanicButton", Content = "\u2620 Panic Btn", Foreground = Brushes.Black, Background = Brushes.Goldenrod, Height = 25, Margin = new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "PanicBtn: CloseAllPosiions"
			};
        	panicBtn.Click += OnButtonClick;                     

			closeBtn = new System.Windows.Controls.Button
			{
				Name = "closeButton", Content = "Manual close", Foreground = Brushes.Black, Background = Brushes.Crimson, Height = 25, Margin = new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Manual Close: CloseAllPosiions manually. Alert!!! Only works with the entries made by the strategy. Manual entries will not be closed from this option."
			};
        	closeBtn.Click += OnButtonClick;                      			
			
			
		}	
		
		private void InitializeButtonGrid()
		{
    		// Create new grid
    		lowerButtonsGrid = new System.Windows.Controls.Grid();

    		// Columns number
    		for (int i = 0; i < 2; i++)
    		{
        		lowerButtonsGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
    		}

    		// Row naumber
    		for (int i = 0; i <= 5; i++)
    		{
        		lowerButtonsGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
    		}
		}				

		private void SetButtonLocations()
		{
			// Btn, Column, Row, Column span
			
    		SetButtonLocation(useAutoBtn, 0, 1, 2);    // Column 0 2 pos
    		SetButtonLocation(useLongBtn, 0, 2);
    		SetButtonLocation(useShortBtn, 1, 2);
    		SetButtonLocation(closeBtn, 0, 3, 2);
			SetButtonLocation(panicBtn, 0, 4, 2);	
		}		
		
		private void SetButtonLocation(System.Windows.Controls.Button button, int column, int row, int columnSpan = 1)
		{
    		System.Windows.Controls.Grid.SetColumn(button, column);
    		System.Windows.Controls.Grid.SetRow(button, row);
    
   			if (columnSpan > 1)
        		System.Windows.Controls.Grid.SetColumnSpan(button, columnSpan);
		}		
		
		private void AddButtonsToGrid()
		{
    		// Add Buttons to grid
    		lowerButtonsGrid.Children.Add(useAutoBtn);
    		lowerButtonsGrid.Children.Add(useLongBtn);
    		lowerButtonsGrid.Children.Add(useShortBtn);
    		lowerButtonsGrid.Children.Add(closeBtn);
			lowerButtonsGrid.Children.Add(panicBtn);
		}			
		
		#region ToggleButton Click Events
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
								
			if (button == useAutoBtn)
			{	
				isStrategyEnabled = !isStrategyEnabled;
				if (isStrategyEnabled) DecoreEnabledButtons(useAutoBtn, "\uD83D\uDD12 Strat Enabled");
				if (!isStrategyEnabled) DecoreDisabledButtons(useAutoBtn, "\uD83D\uDD13 Strat Disabeld"); 
				return;
			}
				
			if (button == useLongBtn)
			{	
				isLongEnabled = !isLongEnabled;
				if (isLongEnabled) DecoreEnabledButtons(useLongBtn, "LONG Enabeld");
				if (!isLongEnabled) DecoreDisabledButtons(useLongBtn, "LONG Disabled"); 
				return;
			}			

			if (button == useShortBtn)
			{	
				isShortEnabled = !isShortEnabled;
				if (isShortEnabled) DecoreEnabledButtons(useShortBtn, "SHORT Enabled");
				if (!isShortEnabled) DecoreDisabledButtons(useShortBtn, "SHORT Disabled"); 
				return;
			}
			
			if (button == closeBtn)
			{	
				CloseActPositions();
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
		
		#region Dispose
		public void DisposeWPFControls() 
		{
			
			
			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			if (useAutoBtn != null)
				useAutoBtn.Click -= OnButtonClick;
						
			if (useLongBtn != null)
				useLongBtn.Click -= OnButtonClick;

			if (useShortBtn != null)
				useShortBtn.Click -= OnButtonClick;			

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
		private bool TabSelected()
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
		
		private void CloseActPositions()
		{
		//	Close actual position manually
        //	Check if there is an open ATM strategy
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
		
        private void FlattenAllPositions()
        {
			
			//  Access the open position
        	Position openPosition = Position;
			Account myAccount;
			AccountSelector accountSelector = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlAccountSelector") as AccountSelector;
			this.chartTraderAccount = ((accountSelector != null) ? accountSelector.SelectedAccount : null);
			this.accountSelector = ((accountSelector != null) ? accountSelector : null);
			
			
			
			// Get the account (replace "Sim101" with your actual account name)
            myAccount = Account.All.FirstOrDefault((Account a) => a.Name == this.chartTraderAccount.DisplayName);
			Print("Account selectd: " + this.chartTraderAccount.DisplayName);
            if (myAccount == null) Print("Account selectd: " + this.chartTraderAccount.DisplayName + " Account not found !!!");
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
		#endregion		
		
		private bool checkTimers()
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
		
		#region DrawPnl
		private void showPNLStatus() {
									
			textLine0 = "ActiveTimer";
			textLine1 = GetActiveTimer();
			textLine2 = "longPerDirection";
			textLine3 = $"{counterLong} / {longPerDirection}";
			textLine4 = "shortPerDirection";
			textLine5 = $"{counterShort} / {shortPerDirection}";
			textLine6 = "ACCT PnL ";
			textLine7 = $"{dailyPnL:F2}";		// Pending
			textLine7 = $"   N/A   ";			// Pending
			string statusPnlText = textLine0 + "\t" + textLine1 + "\n" + textLine2 + "\t" + textLine3 + "\n" + textLine4 + "\t" + textLine5+ "\n" + textLine6 + "\t" + textLine7+ "\n";
			SimpleFont font = new SimpleFont("Arial", 15);
			
			Draw.TextFixed(this, "statusPnl", statusPnlText, PositionPnl, colorPnl, font, Brushes.Transparent, Brushes.Transparent, 0);
								
		}
		#endregion		
		
        protected override void OnBarUpdate()
        {
			if (!isStrategyEnabled) return;
			
			if (BarsInProgress != 0 || CurrentBars[0] < 5)
                return;
			
			// Make sure this strategy does not execute against historical data
			if(State == State.Historical)
				return;

            Up = T3TrendFilter1.Values[0][0];
            Down = T3TrendFilter1.Values[1][0];
						
			if (showPnl) showPNLStatus();			

            #region Long Trade

            // Long entry conditions
            if ((orderId.Length == 0 && atmStrategyId.Length == 0)
				// Long entry group 1
				&& (((Position.MarketPosition == MarketPosition.Flat)
                && (RegChannel1.Middle[0] > RegChannel1.Middle[1])
                && (RegChannel1.Middle[1] <= RegChannel1.Middle[2])
                && (LinReg1[0] > LinReg1[1])
                && (Close[0] > Open[0])
                && (Up >= 5 && Down == 0)
                && (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))

                // Long entry group 2
                || ((Position.MarketPosition == MarketPosition.Flat)
                && (Low[0] > Low[1])
                && (Low[1] <= RegChannel1.Lower[1])
                && (LinReg1[0] > LinReg1[1])
                && (Close[0] > Open[0])
                && (Up >= 5 && Down == 0)
                && (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))

                // Long entry group 3
                || ((Position.MarketPosition == MarketPosition.Flat)
                && (Low[0] > RegressionChannelHighLow1.Lower[1])
                && (LinReg1[0] > LinReg1[1])
                && (Close[0] > Open[0])
                && (Up >= 5 && Down == 0)
                && (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold)))
				&& (isLongEnabled) 
				&& (checkTimers())
				)
            {
				if (!TradesPerDirection || (TradesPerDirection && counterLong < longPerDirection))
				{
                	isAtmStrategyCreated = false;  // reset atm strategy created check to false
					atmStrategyId = GetAtmStrategyUniqueId();
					orderId = GetAtmStrategyUniqueId();
					AtmStrategyCreate(OrderAction.Buy, (isBuySellMarketOrder? OrderType.Market : OrderType.Limit), Close[0], 0, TimeInForce.Gtc, orderId, ATMStrategyTemplate, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						isAtmStrategyCreated = true;
					});
					counterLong +=1;
					counterShort = 0;
				}
				else
				{
					Print("Limit long trades in a row");
				}
				Draw.ArrowUp(this, "LongSignal" + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] + (-10 * TickSize)), Brushes.Lime);
			}

			// Check that atm strategy was created before checking other properties
			if (!isAtmStrategyCreated)
				return;

			// Check for a pending entry order
			if (orderId.Length > 0)
			{
				string[] status = GetAtmStrategyEntryOrderStatus(orderId);

				// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
				if (status.GetLength(0) > 0)
				{
					// Print out some information about the order to the output window
					Print("The entry order average fill price is: " + status[0]);
					Print("The entry order filled amount is: " + status[1]);
					Print("The entry order order state is: " + status[2]);

					// If the order state is terminal, reset the order id value
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderId = string.Empty;
				}
			} // If the strategy has terminated reset the strategy id
			else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat)
				atmStrategyId = string.Empty;

			if (atmStrategyId.Length > 0)
			{
				// You can change the stop price
				if (GetAtmStrategyMarketPosition(atmStrategyId) != MarketPosition.Flat)
					AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP6", atmStrategyId);

				// Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
				// the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
				Print("The current ATM Strategy market position is: " + GetAtmStrategyMarketPosition(atmStrategyId));
				Print("The current ATM Strategy position quantity is: " + GetAtmStrategyPositionQuantity(atmStrategyId));
				Print("The current ATM Strategy average price is: " + GetAtmStrategyPositionAveragePrice(atmStrategyId));
				Print("The current ATM Strategy Unrealized PnL is: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyId));
			}

            #endregion

            #region Short Trade

            // Short entry conditions
            if  ((orderId.Length == 0 && atmStrategyId.Length == 0)
				// Short entry group 1
                && (((Position.MarketPosition == MarketPosition.Flat)
                && (RegChannel1.Middle[0] < RegChannel1.Middle[1])
                && (RegChannel1.Middle[1] >= RegChannel1.Middle[2])
                && (LinReg1[0] < LinReg1[1])
                && (Close[0] < Open[0])
                && (Down <= -5 && Up == 0)
                && (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))

                // Short entry group 2
                || ((Position.MarketPosition == MarketPosition.Flat)
                && (High[0] < High[1])
                && (High[1] >= RegChannel1.Upper[1])
                && (LinReg1[0] < LinReg1[1])
                && (Close[0] < Open[0])
                && (Down <= -5 && Up == 0)
                && (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))

                // Short entry group 3
                || ((Position.MarketPosition == MarketPosition.Flat)
                && (High[0] < RegressionChannelHighLow1.Upper[1])
                && (LinReg1[0] < LinReg1[1])
                && (Close[0] < Open[0])
                && (Down <= -5 && Up == 0)
                && (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold)))
				&& (isShortEnabled)
				&& (checkTimers())
				)
            {
				if (!TradesPerDirection || (TradesPerDirection && counterShort < shortPerDirection))
				{				
                	isAtmStrategyCreated = false;  // reset atm strategy created check to false
					atmStrategyId = GetAtmStrategyUniqueId();
					orderId = GetAtmStrategyUniqueId();
					AtmStrategyCreate(OrderAction.SellShort, (isBuySellMarketOrder? OrderType.Market : OrderType.Limit), Close[0], 0, TimeInForce.Gtc, orderId, ATMStrategyTemplate, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						isAtmStrategyCreated = true;
					});
					counterLong =0;
					counterShort +=1;					
				}
				else
				{
					Print("Limit short trades in a row");
				}
				Draw.ArrowDown(this, "ShortSignal" + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (10 * TickSize)), Brushes.Red);
			}

			// Check that atm strategy was created before checking other properties
			if (!isAtmStrategyCreated)
				return;

			// Check for a pending entry order
			if (orderId.Length > 0)
			{
				string[] status = GetAtmStrategyEntryOrderStatus(orderId);

				// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
				if (status.GetLength(0) > 0)
				{
					// Print out some information about the order to the output window
					Print("The entry order average fill price is: " + status[0]);
					Print("The entry order filled amount is: " + status[1]);
					Print("The entry order order state is: " + status[2]);

					// If the order state is terminal, reset the order id value
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderId = string.Empty;
				}
			} // If the strategy has terminated reset the strategy id
			else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat)
				atmStrategyId = string.Empty;

			if (atmStrategyId.Length > 0)
			{
				// You can change the stop price
				if (GetAtmStrategyMarketPosition(atmStrategyId) != MarketPosition.Flat)
					AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP6", atmStrategyId);

				// Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
				// the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
				Print("The current ATM Strategy market position is: " + GetAtmStrategyMarketPosition(atmStrategyId));
				Print("The current ATM Strategy position quantity is: " + GetAtmStrategyPositionQuantity(atmStrategyId));
				Print("The current ATM Strategy average price is: " + GetAtmStrategyPositionAveragePrice(atmStrategyId));
				Print("The current ATM Strategy Unrealized PnL is: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyId));
			}
			
		//	checkPnL();
        }
		#endregion		

		#region Daily PNL		
		
// Pending to review		
//		protected void checkPnL()
//		{

			
//			if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat && SystemPerformance.AllTrades.Count > 0)
//			{
				
//				canTradeOK = true;
				
//				//  Access the open position
//	        	Position openPosition = Position;
//				Account myAccount;
//				AccountSelector accountSelector = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlAccountSelector") as AccountSelector;
//				this.chartTraderAccount = ((accountSelector != null) ? accountSelector.SelectedAccount : null);
//				this.accountSelector = ((accountSelector != null) ? accountSelector : null);
				
				
				
//				// Get the account (replace "Sim101" with your actual account name)
//	            myAccount = Account.All.FirstOrDefault((Account a) => a.Name == this.chartTraderAccount.DisplayName);
//				Print("Account selectd: " + this.chartTraderAccount.DisplayName);
//	            if (myAccount == null) Print("Account selectd: " + this.chartTraderAccount.DisplayName + " Account not found !!!");
//				if (myAccount == null)
//				     throw new Exception("Account not found.");
				
//	        	if (openPosition != null && openPosition.MarketPosition != MarketPosition.Flat)
//	        	{
//				// Less drastic method, we make a Flatten All to the account used in the strategy and to the instrument that we have loaded on the chart
//					List<Instrument> instrumentNames = new List<Instrument>();
//					foreach (Position position in this.chartTraderAccount.Positions)
//		            {
//		              Instrument instrument = position.Instrument;
//		              if (!instrumentNames.Contains(instrument))
//		                instrumentNames.Add(instrument);
//					  	long id = instrument.Id;
//		            }					
//				}	
				
//				try
//				{
//					lock (Account.All)
//					{
//						myAccount = Account.All.FirstOrDefault((Account a) => a.Name == this.chartTraderAccount.DisplayName);
//					}
//					openPosition = myAccount.GetPosition(id);
//					dailyPnL = myAccount.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);   // Added
//				}				
//				catch (Exception)
//				{
				
//				}

//				Print($"dailyPnL = {dailyPnL}");       				
				
//				if (dailyPnL <= -DailyLossLimit) //Print this when daily Pnl is under Loss Limit
//				{
					
//					Print("Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + dailyPnL + " <<" +  Time[0]);
					
//				//	Text myTextLoss = Draw.TextFixed(this, "loss_text", "Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + "$" + totalPnL + " <<", PositionDailyPNL, colorDailyProfitLoss, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
//				//	myTextLoss.Font = new SimpleFont("Arial", 15) {Bold = true };
//					canTradeOK = false;	

//				}
				
				
//				if (dailyPnL >= DailyProfitLimit) //Print this when daily Pnl is above Profit limit
//				{
					
//					Print("Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" +  dailyPnL + " <<" + Time[0]);
					
//				//	Text myTextProfit = Draw.TextFixed(this, "profit_text", "Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" + "$" +  totalPnL + " <<", PositionDailyPNL, colorDailyProfitLoss, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
//				//	myTextProfit.Font = new SimpleFont("Arial", 15) {Bold = true };
//					canTradeOK = false;
					
//				}
//			}	
//		}		
		
		#endregion
		
		#region Custom Property Manipulation
		private void ModifyProperties(PropertyDescriptorCollection col)
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
		#endregion
		
		#region Custom Enum Selector
		public enum orderSelector
		{
			Limit_Order = 0,
			Market_Order = 1
		};
		
		orderSelector showOrder;
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
		
    }
	
	
}
