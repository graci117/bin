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
    public class KingKhanhv110 : Strategy, ICustomTypeDescriptor //
    {
		// Variables for ATM strategy
	
		private RegChannel RegChannel1;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		private LinReg LinReg1;
		private T3TrendFilter T3TrendFilter1;
		private ADX ADX1;
		private double Up;
		private double Down;
		
		private bool isBuySellMarketOrder;
		private bool tradesPerDirection;	
		private int counterLong;	
		private int counterShort;	
		private bool QuickLong;
		private bool QuickShort;
		private bool quickLongBtnActive;
		private bool quickShortBtnActive;

//		private bool isEnableTime1;
		private bool isEnableTime2;	
		private bool isEnableTime3;	
		private bool isEnableTime4;	
		private bool isEnableTime5;	
		private bool isEnableTime6;			

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
		private System.Windows.Controls.Button			quickLongBtn, quickShortBtn;
		private System.Windows.Controls.Button			add1Btn, close1Btn;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;
		private System.Windows.Controls.Grid 			myGrid;
		
		
		// KillAll 
		private Instrument inst;
		private Account chartTraderAccount;
		private AccountSelector accountSelector;
		private Order myEntryOrder = null;
		private Order myStopOrder = null;
		private Order myTargetOrder = null;
		private bool isAdded = false;
		private bool isAddedSetStop = false;
		private bool activeOrder = false;
		private double myStopPrice = 0;
		private double myLimitPrice = 0;
		private bool isBtnAdd1Enabled;
		private bool isBtnclose1Enabled;

		
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
		
		private bool syncPnl;
		private double historicalTimeTrades;//Sync  PnL
		private double dif;//To Calculate PNL sync
		private double cumProfit;//For real time pnl and pnl synchronization
		
		private bool restarPnL;
		
		
		
        #region Properties

		#region 0. Release notes
		
		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name="StrategyName", Order=1, GroupName="0. Release notes")]
		public string StrategyName
		{ get; set; }
		
		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name="Version", Order =2, GroupName="0. Release notes")]
		public string Version
		{ get; set; }

		
		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name="Credits", Order=3, GroupName="0. Release notes")]
		public string Credits
		{ get; set; }
		
		#endregion

		#region 01. Order Management
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
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Order=1, GroupName="01. Order Management")]
		public int Contracts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Initial Stop Ticks", Order=2, GroupName="01. Order Management")]
		public int InitialStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ProfitTarget", Order=3, GroupName="01. Order Management")]
		public int ProfitTarget
		{ get; set; }		

		[NinjaScriptProperty]
		[Display(Name="BarsSinceExit", Description = "Number of bars that have elapsed since the last specified exit. 0 == Not used. >1 == Use number of bars specified ", Order=4, GroupName="01. Order Management" )]
		public int iBarsSinceExit
		{ get; set; }			
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Daily Loss / Profit ", Description = "Enable / Disable Daily Loss & Profit control", Order = 5, GroupName = "01. Order Management")]
		[RefreshProperties(RefreshProperties.All)]
		public bool dailyLossProfit
		{ get; set; }
		
		//ProfitLimit and LossLimit
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Profit Limit ($)", Description="No positive or negative sign, just integer", Order=6, GroupName="01. Order Management")]
		public double DailyProfitLimit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Loss Limit ($)", Description="No positive or negative sign, just integer", Order=7, GroupName="01. Order Management")]
		public double DailyLossLimit
		{ get; set; }
		#endregion
		
		#region	02. Trades Per Direction	
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
		#endregion
		
		#region 03. Time Frames
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
		
		#region 04. Filters Settings
		[NinjaScriptProperty]
		[Display(Name="RegChanPeriod", Order=1, GroupName="04. Filters Settings")]
		public int RegChanPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LinRegPeriod", Order=2, GroupName="04. Filters Settings")]
		public int LinRegPeriod
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="ADXPeriod", Order=3, GroupName="04. Filters Settings")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXThreshold", Order=4, GroupName="04. Filters Settings")]
		public int ADXThreshold
		{ get; set; }	
		#endregion				
		
		#region STATUS PANEL
        [NinjaScriptProperty]
        [Display(GroupName = "STATUS PANEL", Name = "Show STATUS PANEL ?", Order = 0)]
        public bool showPnl { get; set; }		
		

		[NinjaScriptProperty]
		[Display(Name="STATUS PANEL Position", Description = "Status PNL Position", Order = 1, GroupName = "STATUS PANEL")]
		public TextPosition PositionPnl		
		{ get; set; }				
		
		[XmlIgnore()]
		[Display(Name = "STATUS PANEL Color", Order = 2, GroupName = "STATUS PANEL")]
		public Brush colorPnl
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string colorPnlSerialize
		{
			get { return Serialize.BrushToString(colorPnl); }
   			set { colorPnl = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Show Historical Trades", Description = "Show Historical Teorical Trades", Order=10, GroupName="STATUS PANEL")]
		public bool ShowHistorical
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(GroupName = "STATUS PANEL", Name = "Show Daily PNL Alert Position?", Order = 3)]
        public bool showDailyPnl { get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Daily PNL Alert Position", Description = "Daily PNL Alert Position", Order = 4, GroupName = "STATUS PANEL")]
		public TextPosition PositionDailyPNL
		{ get; set; }

		[XmlIgnore()]
		[Display(Name = "Daily Profit/Loss ALERT", Order = 5, GroupName = "STATUS PANEL")]
		public Brush colorDailyProfitLoss
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string colorDailyProfitLossSerialize
		{
			get { return Serialize.BrushToString(colorDailyProfitLoss); }
   			set { colorDailyProfitLoss = Serialize.StringToBrush(value); }
		}		
        #endregion
		
		#endregion
    
		
		protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
				Description 	= @"This is a strategy based on the linear regression channel.";
                Name 			= "KingKhanh v1.1.0";
				StrategyName 	= "KingKhanh v1.1.0";
				Version			= "Version 1.1.0 August 2024";
				Credits 		= "Strategy provided by Khanh Nguyen";

                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 2;					// This value should limit the number of contracts that the strategy can open per direction.
															// It has nothing to do with the parameter defining the entries per direction that we define in the strategy and are controlled by code.
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
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

				RegChanPeriod					= 30;
				LinRegPeriod					= 9;
				
				ADXPeriod						= 4;
				ADXThreshold					= 50;
				
				Contracts						= 1;
				InitialStop						= 37;   // 37
				ProfitTarget					= 16;	// 20
				
				tradesPerDirection				= false;
				longPerDirection				= 5;
				shortPerDirection				= 5;
				iBarsSinceExit					= 2;	
				
				QuickLong						= false;
				QuickShort						= false;
				
				counterLong						= 0;
				counterShort					= 0;
				
				Start							= DateTime.Parse("06:40", System.Globalization.CultureInfo.InvariantCulture);
				End								= DateTime.Parse("07:30", System.Globalization.CultureInfo.InvariantCulture);
				Start2							= DateTime.Parse("08:00", System.Globalization.CultureInfo.InvariantCulture);
				End2							= DateTime.Parse("08:30", System.Globalization.CultureInfo.InvariantCulture);
				Start3							= DateTime.Parse("09:00", System.Globalization.CultureInfo.InvariantCulture);
				End3							= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				Start4							= DateTime.Parse("12:30", System.Globalization.CultureInfo.InvariantCulture);
				End4							= DateTime.Parse("13:30", System.Globalization.CultureInfo.InvariantCulture);
				Start5							= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End5							= DateTime.Parse("06:29", System.Globalization.CultureInfo.InvariantCulture);
				Start6							= DateTime.Parse("15:00", System.Globalization.CultureInfo.InvariantCulture);
				End6							= DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
				
				// Panel Status
				showPnl							= true;
				PositionPnl						= TextPosition.TopRight;
				colorPnl 						= Brushes.AliceBlue; // Default value
				
				// PnL Daily Limits
				dailyLossProfit					= true;
				DailyProfitLimit				= 100000;
				DailyLossLimit					= 1500;
				
				ShowHistorical					= false;

				showDailyPnl					= true;
				colorDailyProfitLoss			= Brushes.SteelBlue; // Default value
				PositionDailyPNL				= TextPosition.BottomLeft;	
				
				
            }
            else if (State == State.Configure)
            {
                RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
            }
            else if (State == State.DataLoaded)
            {
				RegChannel1					= RegChannel(Close, RegChanPeriod, 0.9);
				RegressionChannelHighLow1	= RegressionChannelHighLow(Close, RegChanPeriod, 3.5);
				LinReg1						= LinReg(Close, LinRegPeriod);
				T3TrendFilter1				= T3TrendFilter(Close, 0.7, 8, 11, 14, 17, 20, false);
				ADX1						= ADX(Close, Convert.ToInt32(ADXPeriod));				

//				RegChannel1.Plots[0].Brush = Brushes.DarkGray;
//				RegChannel1.Plots[1].Brush = Brushes.Aqua;
//				RegChannel1.Plots[2].Brush = Brushes.Aqua;
//				LinReg1.Plots[0].Brush = Brushes.Yellow;
				
				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
				SetTrailStop(@"LE", CalculationMode.Ticks, InitialStop, true);
				SetTrailStop(@"SE", CalculationMode.Ticks, InitialStop, true);
				SetProfitTarget(@"Quick Long", CalculationMode.Ticks, ProfitTarget);
				SetProfitTarget(@"Quick Short", CalculationMode.Ticks, ProfitTarget);
				SetTrailStop(@"Quick Long", CalculationMode.Ticks, InitialStop, true);
				SetTrailStop(@"Quick Short", CalculationMode.Ticks, InitialStop, true);
				SetProfitTarget(@"Add 1", CalculationMode.Ticks, ProfitTarget);
//				SetProfitTarget(@"Add 1", CalculationMode.Ticks, ProfitTarget);
				SetTrailStop(@"Add 1", CalculationMode.Ticks, InitialStop, true);
//				SetTrailStop(@"Add 1", CalculationMode.Ticks, InitialStop, true);

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
			myButton.Background = Brushes.LimeGreen;
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

			addedRow	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(250) };
			
    		// SetButtons
    		SetButtonLocations();

    		// AddButtons
    		AddButtonsToGrid();			
				
			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;

		}
		#endregion
		
		#region CreateButtons
		private void CreateButtons()
		{	
					
			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= System.Windows.Application.Current.FindResource("BasicEntryButton") as Style;			
	
			useAutoBtn = new System.Windows.Controls.Button
			{		
				Content			= "\uD83D\uDD12 Strategy Enabled", Height = 25, Margin = new Thickness(1,0,1,0),	Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Enable (Green) / Disbled (Red) Strategy"
			};	
			if (isStrategyEnabled) DecoreEnabledButtons(useAutoBtn, "\uD83D\uDD12 Strategy Enabled");
			if (!isStrategyEnabled) DecoreDisabledButtons(useAutoBtn, "\uD83D\uDD13 Strategy Disabled");
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

			quickLongBtn = new System.Windows.Controls.Button
			{		
				Content			= "Quick LONG", Height = 25, Margin = new Thickness(1,0,1,0), Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Quick LONG Entry"
			};	
			DecoreEnabledButtons(quickLongBtn, "Quick LONG");
			quickLongBtn.Click += OnButtonClick;
			
			quickShortBtn = new System.Windows.Controls.Button
			{		
				Content			= "Quick SHORT", Height = 25, Margin	= new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Quick Short Entry"
			};	
			DecoreDisabledButtons(quickShortBtn, "Quick SHORT");	
			quickShortBtn.Click += OnButtonClick;		

			add1Btn = new System.Windows.Controls.Button
			{		
				Content			= "Add 1", Height = 25, Foreground = Brushes.Black, Background = Brushes.LightYellow, Margin = new Thickness(1,0,1,0), Padding	= new Thickness(0,0,0,0), Style	= basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Add 1 contract to open position"
			};	
			add1Btn.Click += OnButtonClick;
			
			close1Btn = new System.Windows.Controls.Button
			{		
				Content			= "Close 1", Height = 25, Foreground = Brushes.White, Background = Brushes.DarkBlue, Margin	= new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Close 1 contract from open position"
			};	
			close1Btn.Click += OnButtonClick;


			closeBtn = new System.Windows.Controls.Button
			{
				Name = "closeButton", Content = "Close All Positions", Foreground = Brushes.White, Background = Brushes.DarkRed, Height = 25, Margin = new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "Manual Close: CloseAllPosiions manually. Alert!!! Only works with the entries made by the strategy. Manual entries will not be closed from this option."
			};
        	closeBtn.Click += OnButtonClick; 

			panicBtn = new System.Windows.Controls.Button
			{
				Name = "PanicButton", Content = "\u2620 Panic Shut Down", Foreground = Brushes.Black, Background = Brushes.Goldenrod, Height = 25, Margin = new Thickness(1,0,1,0), Padding = new Thickness(0,0,0,0), Style = basicButtonStyle, BorderThickness = new Thickness(1.5), IsEnabled = true,
					ToolTip = "PanicBtn: CloseAllPosiions"
			};
        	panicBtn.Click += OnButtonClick;                     
			
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

    		// Row number
    		for (int i = 0; i <= 7; i++)
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
   			SetButtonLocation(quickLongBtn, 0, 3);
    		SetButtonLocation(quickShortBtn, 1, 3);    	
   			SetButtonLocation(add1Btn, 0, 4);
    		SetButtonLocation(close1Btn, 1, 4);    		
    		SetButtonLocation(closeBtn, 0, 5, 2);
			SetButtonLocation(panicBtn, 0, 6, 2);	
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
    		lowerButtonsGrid.Children.Add(quickLongBtn);
    		lowerButtonsGrid.Children.Add(quickShortBtn);
    		lowerButtonsGrid.Children.Add(add1Btn);
    		lowerButtonsGrid.Children.Add(close1Btn);
    		lowerButtonsGrid.Children.Add(closeBtn);
			lowerButtonsGrid.Children.Add(panicBtn);
		}			
		#endregion
		
		#region Buttons Clicks Events
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
								
			if (button == useAutoBtn)
			{	
				isStrategyEnabled = !isStrategyEnabled;
				if (isStrategyEnabled)
				{
					DecoreEnabledButtons(useAutoBtn, "\uD83D\uDD12 Strategy Enabled");	
					Print("Strategy: " + isStrategyEnabled);
				} 
				if (!isStrategyEnabled)
				{
					DecoreDisabledButtons(useAutoBtn, "\uD83D\uDD13 Strategy Disabled");
					Print("Strategy: " + isStrategyEnabled);					
				}  
				return;
			}
				
			if (button == useLongBtn)
			{	
				isLongEnabled = !isLongEnabled;
				if (isLongEnabled){
					DecoreEnabledButtons(useLongBtn, "LONG Enabled");
					Print("Long Enabled " + isLongEnabled);	
				} 
				if (!isLongEnabled)
				{
					DecoreDisabledButtons(useLongBtn, "LONG Disabled");	
					Print("Long Disabled " + isLongEnabled);
				}  
				return;
			}			

			if (button == useShortBtn)
			{	
				isShortEnabled = !isShortEnabled;
				if (isShortEnabled)
				{
					DecoreEnabledButtons(useShortBtn, "SHORT Enabled");	
					Print("Short Activated " + isShortEnabled);
				} 
		
				if (!isShortEnabled)
				{
					DecoreDisabledButtons(useShortBtn, "SHORT Disabled");	
					Print("Short Disabled " + isShortEnabled);
				}  
				return;
			}

			if (button == quickLongBtn)
			{	
			//	Code for QuickLong
				Print("State: " + QuickLong);
			
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
				if(QuickLong == false)
				{
					QuickLong = true;
					Print("Quick Long On  ");
					quickLongBtnActive = true;
				}
				EnterLong(Convert.ToInt32(Contracts), @"Quick Long");
				QuickLong		= false;
				

			//	ForceRefresh();
				return;
			}	

			if (button == quickShortBtn)
			{	
			//	Code for QuickShort
				Print("State: " + QuickShort);
				
				// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
				if(QuickShort == false)
				{
					QuickShort = true;
					Print("Quick Short On  ");
					quickShortBtnActive = true;
				}		
				EnterShort(Convert.ToInt32(Contracts), @"Quick Short");
				QuickShort		= false;
			//	ForceRefresh();	
				return;
			}			

			if (button == add1Btn)
			{	
			//	Code for Add 1	
				add1Entry();
			//	ForceRefresh();
				return;
			}

			if (button == close1Btn)
			{	
			//	Code for Close 1
				close1Exit();
			//	ForceRefresh();
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
		#endregion
		
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

			if (quickLongBtn != null)
				quickLongBtn.Click -= OnButtonClick;

			if (quickShortBtn != null)
				quickShortBtn.Click -= OnButtonClick;	

			if (add1Btn != null)
				add1Btn.Click -= OnButtonClick;

			if (close1Btn != null)
				close1Btn.Click -= OnButtonClick;				

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

		#region New functions implementation
		private void CloseActPositions()
		{
		//	Close actual position manually
        //	Check if there is an open position
			Print("Position Closing");
			
			if(Position.MarketPosition == MarketPosition.Long) {
				ExitLong("Manual Exit", @"LE");			
				ExitLong("Manual Exit", @"Quick Long");				
				
			}else if(Position.MarketPosition == MarketPosition.Short) {
				ExitShort("Manual Exit", @"SE");
				ExitShort("Manual Exit", @"Quick Short");
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

		private void add1Entry()
		{
		//	Code for add1Entry()
		//	Add 1  Check value of open Position and EntriesPerDirection
	    	int additionalContracts = 1; // Contracts to add
		//  Access the open position
        	Position openPosition = Position;
			double currentPosition = openPosition.Quantity;
        	if (openPosition != null && openPosition.MarketPosition != MarketPosition.Flat)
        	{
				currentPosition = Position.Quantity;
				if (currentPosition + additionalContracts <= EntriesPerDirection)
					AddContractToOpenPosition();
			}	

			return;	
		}	

		private void close1Exit()
		{
		//	Code for close1Exit()
		//	Close 1 Check value of open Position and EntriesPerDirection
			int additionalContracts = 1; // Contracts to close
        	Position openPosition = Position;
			double currentPosition = openPosition.Quantity;
        	if (openPosition != null && openPosition.MarketPosition != MarketPosition.Flat)
        	{
				currentPosition = Position.Quantity;
				if (currentPosition > additionalContracts)
					CloseOneContractFromPosition();
			}		

			return;	
		}	


		private void AddContractToOpenPosition()
		{   // Add 1
			int additionalContracts = 1;
		    try
		    {
				if(Position.MarketPosition == MarketPosition.Long) {
					if (!quickLongBtnActive)
					{	
						EnterLong(additionalContracts, @"LE");	
					}
					if (quickLongBtnActive)
					{	
						EnterLong(additionalContracts, @"Quick Long");
					}					
				
				}else if(Position.MarketPosition == MarketPosition.Short) {
					if (!quickShortBtnActive)
					{	
						EnterShort(additionalContracts, @"SE");
					//	if(isBuySellMarketOrder) EnterShort(additionalContracts, @"SE");
					//	if(!isBuySellMarketOrder) EnterShortLimit(additionalContracts, GetCurrentAsk(0), @"SE");	
					}	
					if (quickShortBtnActive)
					{	
						EnterShort(additionalContracts, @"Quick Short");
					//	if(isBuySellMarketOrder) EnterShort(additionalContracts, @"Quick Short");
					//	if(!isBuySellMarketOrder) EnterShortLimit(additionalContracts, GetCurrentAsk(0), @"Quick Short");
					}						
				}	
		        else {
		            Print("No open position to close contracts from.");
		        }
		    }
		    catch (Exception ex)
		    {
		        Print($"Failed to add contracts due to: {ex.Message}");
		    }
		}
		
/*
		
				if (!quickLongBtnActive)
				{	
					ExitLongStopMarket(0, true, Position.Quantity, (Position.AveragePrice - (InitialStop * TickSize)) , @"MoneyDone", @"LE");
					ExitLongLimit(0, true, Position.Quantity, (Position.AveragePrice + (ProfitTarget * TickSize)) , @"KaChing", @"LE");
				}
				if(quickLongBtnActive){
					ExitLongStopMarket(0, true, Position.Quantity, (Position.AveragePrice - (InitialStop * TickSize)) , @"Stop Quick Long", @"Quick Long");
					ExitLongLimit(0, true, Position.Quantity, (Position.AveragePrice + (ProfitTarget * TickSize)) , @"Target Quick Long", @"Quick Long");
				}		
		
*/		
		
		
		
		private void CloseOneContractFromPosition()
		{	// Close 1
		    int contractsToClose = 1; // Number of contracts to close && check  EntriesPerDirection
		    try
		    {
				checkOrder();
//				if (myStopOrder != null) myStopPrice = myStopOrder.StopPrice;
//				if (myTargetOrder != null) myLimitPrice = myTargetOrder.LimitPrice;
				if(Position.MarketPosition == MarketPosition.Long) {
					ExitLong(0, contractsToClose,  "Close1 Exit", @"LE");			
					ExitLong(0, contractsToClose,  "Close1 Exit", @"Quick Long");				
				
				}else if(Position.MarketPosition == MarketPosition.Short) {
					ExitShort(0, contractsToClose,  "Close1 Exit", @"SE");
					ExitShort(0, contractsToClose,  "Close1 Exit", @"Quick Short");
				}	
		        else {
		            Print("No open position to close contracts from.");
		        }
		    }
		    catch (Exception ex)
		    {
		        Print($"Failed to close contracts due to: {ex.Message}");
		    }
			Print($"{Times[0][0].TimeOfDay} Leaving Close 1.  StopPrice:  {myStopPrice}   LimitPrice  {myLimitPrice}    orderQuantity {Position.Quantity}");
		}


		
		private void checkPositions()
		{
		//	Detect unwanted Positions opened (possible rogue Order?)
	        double currentPosition = Position.Quantity; // Get current position quantity
		
			if (Position.MarketPosition == MarketPosition.Flat)
			{
		        foreach (var order in Orders)
		        {
		            if (order != null) CancelOrder(order);
		        }				
			}				
			
		}	
		
		private void checkOrder()
		{
		// Verify one active order and set myStopPrice and mylimitPrice to be use in changing orders when add or close 1 contracts to open positions
			activeOrder = false;
			
			if (Orders.Count != 0)
			{
				Print($"{Times[0][0].TimeOfDay} ACTIVE Orders Count:  {Orders.Count}");
				foreach (var order in Orders)
		        {
					string entrySignal = order.FromEntrySignal;
					Print($"{Times[0][0].TimeOfDay} myOrder NOT null {order.OrderId}  StopPrice:  {order.StopPrice}   LimitPrice  {order.LimitPrice}    orderQuantity {order.Quantity}   tiene el estado: {order.OrderState}  y es del tipo {order.OrderTypeString}    FROM EntrySignal {entrySignal}");
		            // Verificar el estado de cada orden
					if (order.OrderState == OrderState.Filled)
		            {
		                myEntryOrder = order;
						if (order.IsStopMarket && entrySignal != "Add 1")
						{
							myStopOrder = order;
							myStopPrice = myStopOrder.StopPrice;
						}	
						if (order.IsLimit &&  entrySignal != "Add 1") 
						{
							myLimitPrice = myEntryOrder.LimitPrice;
							
						}	
		            }					
					else if (order.OrderState == OrderState.TriggerPending && entrySignal != "Add 1")
		            {
		                if (order.IsStopMarket)
						{
							myStopOrder = order;
							myStopPrice = myStopOrder.StopPrice;
						}
		            }
					else if (order.OrderState == OrderState.Working && entrySignal != "Add 1")
		            {						
						if (order.IsLimit)
						{ 
							myTargetOrder = order;
							myLimitPrice = myTargetOrder.LimitPrice;	
						}	
		            }					
		            else
		            {
		                Print("La orden " + order.OrderId + " tiene el estado: " + order.OrderState);
		            }
//					if (order.IsStopMarket) myStopOrder = order;
//					if (order.IsLimit) myTargetOrder = order;
//					if (myStopOrder != null){
//						var test = myStopOrder.Quantity;
//						Print($"{Times[0][0].TimeOfDay} myStopOrder NOT null  StopPrice:  {myStopOrder.StopPrice}   LimitPrice  {myStopOrder.LimitPrice}    orderQuantity {myStopOrder.Quantity}");
//						myStopPrice = myStopOrder.StopPrice;
//					}	
//					if (myTargetOrder != null){
//						Print($"{Times[0][0].TimeOfDay} myTargetOrder NOT null  StopPrice:  {myTargetOrder.StopPrice}   LimitPrice  {myTargetOrder.LimitPrice}    orderQuantity {myTargetOrder.Quantity}");
//						myLimitPrice = myTargetOrder.LimitPrice; 				
//					}								
		        }
				Print($"{Times[0][0].TimeOfDay} myEntryOrder NOT null {myEntryOrder.OrderId}  StopPrice:  {myEntryOrder.StopPrice}   LimitPrice  {myEntryOrder.LimitPrice}    orderQuantity {myEntryOrder.Quantity}   tiene el estado: {myEntryOrder.OrderState}  y es del tipo {myEntryOrder.OrderTypeString}");
//				Print($"{Times[0][0].TimeOfDay} myStopOrder NOT null {myStopOrder.OrderId}  StopPrice:  {myStopOrder.StopPrice}   LimitPrice  {myStopOrder.LimitPrice}    orderQuantity {myStopOrder.Quantity}   tiene el estado: {myStopOrder.OrderState}  y es del tipo {myStopOrder.OrderTypeString}");
//				Print($"{Times[0][0].TimeOfDay} myTargetOrder NOT null {myTargetOrder.OrderId}  StopPrice:  {myTargetOrder.StopPrice}   LimitPrice  {myTargetOrder.LimitPrice}    orderQuantity {myTargetOrder.Quantity}   tiene el estado: {myTargetOrder.OrderState}  y es del tipo {myTargetOrder.OrderTypeString}");
				activeOrder = true;
				
			}
		}
		
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
		
		#endregion				
		
		#region DrawPnl
		private void showPNLStatus() {
									
			textLine0 = "ActiveTimer";
			textLine1 = GetActiveTimer();
			textLine2 = "longPerDirection";
			textLine3 = $"{counterLong} / {longPerDirection} | " + (TradesPerDirection ? "On" : "Off");
			textLine4 = "shortPerDirection";
			textLine5 = $"{counterShort} / {shortPerDirection} | " + (TradesPerDirection ? "On" : "Off");
			textLine6 = "BarsSinceExit ";
			textLine7 = $"{iBarsSinceExit}    |    " + (iBarsSinceExit > 1 ?  "On" : "Off");				
			string statusPnlText = textLine0 + "\t" + textLine1 + "\n" + textLine2 + "\t" + textLine3 + "\n" + textLine4 + "\t" + textLine5+ "\n" + textLine6 + "\t" + textLine7+ "\n";
			SimpleFont font = new SimpleFont("Arial", 15);
			
			Draw.TextFixed(this, "statusPnl", statusPnlText, PositionPnl, colorPnl, font, Brushes.Transparent, Brushes.Transparent, 0);
								
		}
		#endregion		
		
        #region OnBarUpdate
		protected override void OnBarUpdate()
        {
			if (!isStrategyEnabled) return;
			
			if (BarsInProgress != 0 || CurrentBars[0] < 5)
                return;
			
			if (!ShowHistorical)
			{
				if (State != State.Realtime)
					return;				
			}

            Up = T3TrendFilter1.Values[0][0];
            Down = T3TrendFilter1.Values[1][0];
						
			// at the start of a new session, reset the currentPnL for a new day of trading
			if (Bars.IsFirstBarOfSession){
				cumPnL 			= totalPnL; ///Double that copies the full session PnL (If trading multiple days). Is only calculated once per day.
				dailyPnL		= totalPnL - cumPnL; ///Subtract the copy of the full session by the full session PnL. This resets your daily PnL back to 0.
			}

//			if (IsFirstTickOfBar) checkPositions();    // Detect unwanted Positions opened (possible rogue Order?

			if (showPnl) showPNLStatus();		

            if (CheckLongEntryConditions() 
				&& (isStrategyEnabled)
				&& (isLongEnabled) 
				&& (checkTimers())
				&& ((dailyLossProfit ? dailyPnL > -DailyLossLimit : true))  //Loss remains 'above' limit 
				&& ((dailyLossProfit ? dailyPnL < DailyProfitLimit: true)) //Profit remains 'below' limit
				)
            {
				if (!TradesPerDirection || (TradesPerDirection && counterLong <= longPerDirection))
				{
					counterLong +=1;
					counterShort = 0;
				//	CreateAtmStrategy(OrderAction.Buy, "LongSignal", Low[0] + (-10 * TickSize), Brushes.Lime);

					if(isBuySellMarketOrder) EnterLong(Convert.ToInt32(Contracts), @"LE");
					if(!isBuySellMarketOrder) EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"LE");
					//	if(isBuySellMarketOrder ? EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"LE") : EnterLong(Convert.ToInt32(Contracts), @"LE"));
							
					Draw.ArrowUp(this, "LongSignal" + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] + (-10 * TickSize)) , Brushes.Lime);
			
				}
				else
				{
					Print("Limit long trades in a row");
				}
            }

            if (CheckShortEntryConditions() 
				&& (isStrategyEnabled)
				&& (isShortEnabled)
				&& (checkTimers())
				&& ((dailyLossProfit ? dailyPnL > -DailyLossLimit : true))  //Loss remains 'above' limit 
				&& ((dailyLossProfit ? dailyPnL < DailyProfitLimit: true)) //Profit remains 'below' limit
            	)
            {

				if (!TradesPerDirection || (TradesPerDirection && counterShort <= shortPerDirection))
				{				
					counterLong =0;
					counterShort +=1;
				//	CreateAtmStrategy(OrderAction.SellShort, "ShortSignal", High[0] + (10 * TickSize), Brushes.Red);					

					if(isBuySellMarketOrder) EnterShort(Contracts, @"SE");
					if(!isBuySellMarketOrder) EnterShortLimit(Contracts, GetCurrentAsk(0), @"SE");
				//	if(isBuySellMarketOrder ? EnterShortLimit(Contracts, GetCurrentAsk(0), @"SE") : EnterShort(Contracts, @"SE")); 				
					Draw.ArrowDown(this, "ShortSignal" + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (10 * TickSize)) , Brushes.Red);

				}
				else
				{
					Print("Limit short trades in a row");
				}
            }
			
			// Reset when Flat
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				
//				Print("Quick Long/Short Off flatten " + QuickLong);
				quickLongBtnActive = false;
				quickShortBtnActive = false;

				counterShort = 0;
				Print("Short RESET");

				counterLong = 0;
				Print("Long RESET");
			}

		//	checkPnL();

        }
		#endregion
		
		#region Entries functions
        private bool CheckLongEntryConditions()
        {
            return (Position.MarketPosition == MarketPosition.Flat &&
                   ((RegChannel1.Middle[0] > RegChannel1.Middle[1] && RegChannel1.Middle[1] <= RegChannel1.Middle[2] && LinReg1[0] > LinReg1[1] && Close[0] > Open[0] && Up >= 5 && Down == 0 && ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold) ||
                    (Low[0] > Low[1] && Low[1] <= RegChannel1.Lower[1] && LinReg1[0] > LinReg1[1] && Close[0] > Open[0] && Up >= 5 && Down == 0 && ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold) ||
                    (Low[0] > RegressionChannelHighLow1.Lower[1] && LinReg1[0] > LinReg1[1] && Close[0] > Open[0] && Up >= 5 && Down == 0 && ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))) &&
                   	((iBarsSinceExit > 0 ? BarsSinceExitExecution(0, "", 0) > 1: BarsSinceExitExecution(0, "", 0) > iBarsSinceExit) || BarsSinceExitExecution(0, "", 0) == -1);
/*
			return	((Position.MarketPosition == MarketPosition.Flat)
				&& (RegChannel1.Middle[0] > RegChannel1.Middle[1])
				&& (RegChannel1.Middle[1] <= RegChannel1.Middle[2])
				&& (LinReg1[0] > LinReg1[1])
				&& (Close[0] > Open[0])
				&& (!isEnableT3TrendFilter || (Up >= 5 && Down == 0))
				&& (!isEnableOBV_EMA || OBV()[0] > OBV_EMA[0])
				&& (!isEnableADX || (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))
				&& (!isEnableVOLMA || VOLMA1[0] > VolMovAvg))
				
				// Long entry group 2
				|| ((Position.MarketPosition == MarketPosition.Flat)
				&& (Low[0] > Low[1])
				&& (Low[1] <= RegChannel1.Lower[1])
				&& (LinReg1[0] > LinReg1[1])
				&& (Close[0] > Open[0])
				&& (!isEnableT3TrendFilter || (Up >= 5 && Down == 0))
				&& (!isEnableOBV_EMA || OBV()[0] > OBV_EMA[0])
				&& (!isEnableADX || (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))
				&& (!isEnableVOLMA || VOLMA1[0] > VolMovAvg))
				
				// Long entry group 3
				|| ((Position.MarketPosition == MarketPosition.Flat)
				&& (Low[0] > RegressionChannelHighLow1.Lower[1])
				&& (LinReg1[0] > LinReg1[1])
				&& (Close[0] > Open[0])
				&& (!isEnableT3TrendFilter || (Up >= 5 && Down == 0))
				&& (!isEnableOBV_EMA || OBV()[0] > OBV_EMA[0])
				&& (!isEnableADX || (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))
				&& (!isEnableVOLMA || VOLMA1[0] > VolMovAvg))
							 		
				 // Condition Position Market
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
			 	 && (Position.MarketPosition == MarketPosition.Flat);
*/



        }

        private bool CheckShortEntryConditions()
        {
            return (Position.MarketPosition == MarketPosition.Flat
				&& ((RegChannel1.Middle[0] < RegChannel1.Middle[1]
				&&	RegChannel1.Middle[1] >= RegChannel1.Middle[2]
				&&	LinReg1[0] < LinReg1[1]
				&&	Close[0] < Open[0]
				&&	Down <= -5 && Up == 0 
				&&	ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold) 
				||  (High[0] < High[1] 
				&&	High[1] >= RegChannel1.Upper[1] 
				&& LinReg1[0] < LinReg1[1] 
				&& Close[0] < Open[0] 
				&& Down <= -5 && Up == 0 
				&& ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold) 
				|| (High[0] < RegressionChannelHighLow1.Upper[1] 
				&& LinReg1[0] < LinReg1[1] 
				&& Close[0] < Open[0] 
				&& Down <= -5 && Up == 0 && ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))) 
				&& ((iBarsSinceExit > 0 ? BarsSinceExitExecution(0, "", 0) > 1: BarsSinceExitExecution(0, "", 0) > iBarsSinceExit) || BarsSinceExitExecution(0, "", 0) == -1);
/*			
			return	((Position.MarketPosition == MarketPosition.Flat)
				&& (RegChannel1.Middle[0] < RegChannel1.Middle[1])
				&& (RegChannel1.Middle[1] >= RegChannel1.Middle[2])
				&& (LinReg1[0] < LinReg1[1])
				&& (Close[0] < Open[0])
				&& (!isEnableT3TrendFilter || (Down <= -5 && Up == 0))
				&& (!isEnableOBV_EMA || OBV()[0] < OBV_EMA[0])
				&& (!isEnableADX || (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))
				&& (!isEnableVOLMA || VOLMA1[0] > VolMovAvg))
				
				// Short entry group 2
				|| ((Position.MarketPosition == MarketPosition.Flat)
				&& (High[0] < High[1])
				&& (High[1] >= RegChannel1.Upper[1])
				&& (LinReg1[0] < LinReg1[1])
				&& (Close[0] < Open[0])
				&& (!isEnableT3TrendFilter || (Down <= -5 && Up == 0))
				&& (!isEnableOBV_EMA || OBV()[0] < OBV_EMA[0])
				&& (!isEnableADX || (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))
				&& (!isEnableVOLMA || VOLMA1[0] > VolMovAvg))
				
				// Short entry group 3
				|| ((Position.MarketPosition == MarketPosition.Flat)
				&& (High[0] < RegressionChannelHighLow1.Upper[1])
				&& (LinReg1[0] < LinReg1[1])
				&& (Close[0] < Open[0])
				&& (!isEnableT3TrendFilter || (Down <= -5 && Up == 0))
				&& (!isEnableOBV_EMA || OBV()[0] < OBV_EMA[0])
				&& (!isEnableADX || (ADX1[0] > ADX1[2] && ADX1[0] > ADXThreshold))
				&& (!isEnableVOLMA || VOLMA1[0] > VolMovAvg))
				
				// Condition Position Market
				&& ((BarsSinceExitExecution(0, "", 0) > 1)
				|| (BarsSinceExitExecution(0, "", 0) == -1))
				&& (Position.MarketPosition == MarketPosition.Flat);
*/


        }
		#endregion
		
		#region Daily PNL
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
		
			
			if (Position.MarketPosition == MarketPosition.Flat && SystemPerformance.AllTrades.Count > 0)
			{
				
//				PositionPnl = TextPosition.BottomLeft;
//				totalPnL = 0; //backtest
			
				totalPnL = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit; ///Double that sets the total PnL 

				dailyPnL = (totalPnL) - (cumPnL); ///Your daily limit is the difference between these
				
				
				if (dailyPnL <= -DailyLossLimit) //Print this when daily Pnl is under Loss Limit
				{
					
					Print("Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + dailyPnL + " <<" +  Time[0]);
					
					Text myTextLoss = Draw.TextFixed(this, "loss_text", "Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + "$" + totalPnL + " <<", PositionDailyPNL, colorDailyProfitLoss, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextLoss.Font = new SimpleFont("Arial", 15) {Bold = true };

				}
				
				
				if (dailyPnL >= DailyProfitLimit) //Print this when daily Pnl is above Profit limit
				{
					
					Print("Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" +  dailyPnL + " <<" + Time[0]);
					
					Text myTextProfit = Draw.TextFixed(this, "profit_text", "Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" + "$" +  totalPnL + " <<", PositionDailyPNL, colorDailyProfitLoss, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextProfit.Font = new SimpleFont("Arial", 15) {Bold = true };
	
				}
			}	
			
			if (Position.MarketPosition == MarketPosition.Flat)	checkPositions(); // Detect unwanted Positions opened (possible rogue Order?)
						
		}
		#endregion
		
		//Draw pnl
		#region DrawStrategyPnl		
		private void DrawStrategyPnl(ChartControl chartControl) {
	
			if (!restarPnL) {
			    // Mode normal
			    if (syncPnl) {
					dif = historicalTimeTrades - getCumProfit();
			    } else {
			        cumProfit = getCumProfit() + dif;
			    }
			}else {
			    // Mode restarPnL
				dif = historicalTimeTrades - getCumProfit();
				if(getCumProfit() == 0){	//Reset starts negative so we start it at zero.
					cumProfit = 0;
				}else{
			    	cumProfit = getCumProfit() - dif;
				}
			}
			
			double unrealizedProfitLoss = Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
			string Total = (cumProfit + unrealizedProfitLoss).ToString("N0");
									
			string textLine0 = Account.Name + " | " + Account.Connection.Options.Name;
			string textLine1 = "Total PNL: ";
			string textLine2 = "$" + Total;
			string textLine3 = "Realized PNL: ";
			string textLine4 = "$" + cumProfit.ToString("N0");
			string textLine5 = "Unrealized PNL: ";
			
			string formattedPnL = unrealizedProfitLoss.ToString("N0");
			string textLine6 = "$" + formattedPnL;

			string realTimeTradeText = textLine0 + "\n" + textLine1 + "\t" + textLine2 + "\n" + textLine3 + "\t" + textLine4 + "\n" + textLine5+ "\t" + textLine6;
			SimpleFont font = new SimpleFont("Arial", 15);
			
			Draw.TextFixed(this, "realTimeTradeText", realTimeTradeText, PositionDailyPNL, colorPnl, font, Brushes.Transparent, Brushes.Transparent, 0);
								
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);	
			if (showDailyPnl) DrawStrategyPnl(chartControl);
		}
		
		private double getCumProfit() {
			TradeCollection realTimeTrades = SystemPerformance.RealTimeTrades;
			return realTimeTrades.TradesPerformance.Currency.CumProfit;
		}
		
		#endregion		
		//Fin draw PNL
		
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


/*
  // Only enter if at least 10 bars has passed since our last exit or if we have never traded yet
  if ((BarsSinceExitExecution() > iBarsSinceExit || BarsSinceExitExecution() == -1) && CrossAbove(SMA(10), SMA(20), 1))
      EnterLong();

*/