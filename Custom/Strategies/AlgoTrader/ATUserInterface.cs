#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{
    public abstract partial class AlgoBase : Strategy
    {
		#region UI Variables
        private Chart chartWindow;
		private Grid chartTraderGrid, lowerButtonsGrid;
		private ScrollViewer panelScrollViewer;
		
		private ComboBox accountSelector, stopTypeSelector, beTriggerModeSelector, ptCalculationModeSelector, orderTypeSelector, initialStopModeSelector, masterTrendFilterSelector, chopDetectionSelector;
		private QuantityUpDown quantitySelector;
		
		private TextBox limitOffsetTextBox, rrRatioTextBox, closePartialTextBox, initialStopTextBox, rangeFilteredStopLossBandTextBox, rangeFilteredTakeProfitBandTextBox, dtpStopLossBandTextBox, dtpTakeProfitBandTextBox;
		private TextBox profitTargetTextBox, manualMoveStopLookbackTextBox, beTriggerTextBox, beOffsetTextBox;
		private TextBox initialTrailTicksTextBox, finalTrailTriggerPercentTextBox, finalTrailTicksTextBox, highLowTrailLookbackTextBox, hybridTrailTriggerPercentTextBox, stagedTrailTriggerPercentTextBox;
        private TextBox scaleOutLevel1PctTextBox, scaleOutQty1TextBox, scaleOutLevel2PctTextBox, scaleOutQty2TextBox;

		private TextBox atrMultiplierTextBox, atrPeriodTextBox, stdDevMultiplierTextBox;

		private TextBlock beTriggerLabel, initialStopLabel, profitTargetLabel, rrRatioLabel, rangeFilteredStopLossBandLabel, rangeFilteredTakeProfitBandLabel, dtpStopLossBandLabel, dtpTakeProfitBandLabel;
		private TextBlock initialTrailTicksLabel, finalTrailTriggerPercentLabel, finalTrailTicksLabel, highLowTrailLookbackLabel, hybridTrailTriggerPercentLabel, stagedTrailTriggerPercentLabel;
        private TextBlock limitOffsetLabel;
		
		private Grid initialStopGrid, profitTargetGrid, rrRatioGrid, rangeFilteredStopLossBandGrid, rangeFilteredTakeProfitBandGrid, dtpStopLossBandGrid, dtpTakeProfitBandGrid;
		private Grid initialTrailTicksGrid, finalTrailTriggerPercentGrid, finalTrailTicksGrid, highLowTrailLookbackGrid, hybridTrailTriggerPercentGrid, stagedTrailTriggerPercentGrid;
        private Grid limitOffsetGrid;
		private Border trendAlertPanel;
		
		private Button limitOffsetUp, limitOffsetDown, rrRatioUp, rrRatioDown, closePartialUp, closePartialDown; 
		private Button initialStopUp, initialStopDown, profitTargetUp, profitTargetDown, manualMoveStopLookbackUp, manualMoveStopLookbackDown;
		private Button rangeFilteredStopLossBandUp, rangeFilteredStopLossBandDown, rangeFilteredTakeProfitBandUp, rangeFilteredTakeProfitBandDown, dtpStopLossBandUp, dtpStopLossBandDown, dtpTakeProfitBandUp, dtpTakeProfitBandDown;
		private Button beTriggerUp, beTriggerDown, beOffsetUp, beOffsetDown;
		private Button initialTrailTicksUp, initialTrailTicksDown, finalTrailTriggerPercentUp, finalTrailTriggerPercentDown, finalTrailTicksUp, finalTrailTicksDown;
		private Button highLowTrailLookbackUp, highLowTrailLookbackDown, hybridTrailTriggerPercentUp, hybridTrailTriggerPercentDown, stagedTrailTriggerPercentUp, stagedTrailTriggerPercentDown;
        private Button scaleOutLevel1PctUp, scaleOutLevel1PctDown, scaleOutQty1Up, scaleOutQty1Down, scaleOutLevel2PctUp, scaleOutLevel2PctDown, scaleOutQty2Up, scaleOutQty2Down;
		
		private Button atrMultiplierUp, atrMultiplierDown, atrPeriodUp, atrPeriodDown, stdDevMultiplierUp, stdDevMultiplierDown;

        private Button autoBtn, longBtn, shortBtn, quickLongBtn, quickShortBtn;
        private Button add1Btn, close1Btn, BEBtn, TSBtn, moveTSBtn, moveToBEBtn;
        private Button moveTS50PctBtn, closeBtn, panicBtn, donateBtn, errorResetBtn;		
		private Button trendBotsBtn, rangeBotsBtn, breakoutBotsBtn;
		private Button regimeFilterOnBtn, regimeFilterOffBtn;
		private Button staticSlBtn, dynamicSlBtn, staticTpBtn, dynamicTpBtn;
		private Button botPerfFilterBtn, staticBeBtn, dynamicBeBtn;
		private Button autoSizeBtn, scaleOutBtn, enableFilterBtn;
		private bool panelActive;
		#endregion
		
		#region UI Control Name Constants
		private const string TrendBotsButton = "TrendBotsBtn";
		private const string RangeBotsButton = "RangeBotsBtn";
		private const string BreakoutBotsButton = "BreakoutBotsBtn";
		private const string RegimeFilterOnButton = "RegimeFilterOnBtn";
		private const string RegimeFilterOffButton = "RegimeFilterOffBtn";
		private const string StaticSlButton = "StaticSlBtn";
		private const string DynamicSlButton = "DynamicSlBtn";
		private const string StaticTpButton = "StaticTpBtn";
		private const string DynamicTpButton = "DynamicTpBtn";
		private const string StaticBeButton = "StaticBeBtn";
		private const string DynamicBeButton = "DynamicBeBtn";
		private const string BotPerfFilterButton = "BotPerfFilterBtn";
		private const string AutoSizeButton = "AutoSizeBtn";
		private const string ScaleOutButton = "ScaleOutBtn";
		private const string ORDER_TYPE_SELECTOR_NAME = "orderTypeSelector";
		private const string PT_SELECTOR_NAME = "ptSelector";
		private const string STOP_SELECTOR_NAME = "stopSelector";
		private const string INITIAL_STOP_MODE_SELECTOR_NAME = "initialStopModeSelector";
		private const string BE_TRIGGER_MODE_SELECTOR_NAME = "beTriggerModeSelector";
		private const string MASTER_TREND_FILTER_SELECTOR_NAME = "masterTrendFilterSelector";
		private const string CHOP_DETECTION_SELECTOR_NAME = "chopDetectionSelector";
		private const string AutoButton = "AutoBtn";
		private const string LongButton = "LongBtn";
		private const string ShortButton = "ShortBtn";
		private const string QuickLongButton = "QuickLongBtn";
		private const string QuickShortButton = "QuickShortBtn";
		private const string AddOneButton = "Add1Btn";
		private const string CloseOneButton = "ClosePartialBtn";
		private const string BEButton = "BEBtn";
		private const string TSButton = "TSBtn";
		private const string MoveTSButton = "MoveTSBtn";
		private const string MoveTS50PctButton = "MoveTS50PctButton";
		private const string MoveToBeButton = "MoveToBeBtn";
		private const string CloseButton = "CloseBtn";
		private const string PanicButton = "PanicBtn";		
		private const string DonateButton = "DonateBtn";
		private const string ErrorResetButton = "ErrorResetBtn";
		private const string EnableFilterButton = "EnableFilterBtn";
		#endregion		
		
		public void SetBreakevenMode(BreakevenManagementMode newMode)
        {
            if (BreakevenMode == newMode) return;
            BreakevenMode = newMode;
            if (State >= State.Historical && ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() => UpdateBreakevenModeButtonStates());
            }
        }
		
        public void ToggleBreakevenMode()
        {
            SetBreakevenMode(BreakevenMode == BreakevenManagementMode.Static ? BreakevenManagementMode.Dynamic : BreakevenManagementMode.Static);
        }
		
		#region UI Creation and Management
		
		protected void CreateWPFControls()
	    {
	      chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
	      if (chartWindow == null) return;
		  
	      ChartTrader chartTrader = chartWindow.FindFirst("ChartWindowChartTraderControl") as ChartTrader;
	      if (chartTrader == null || chartTrader.Content == null) return;
	      
	      chartTraderGrid = chartTrader.Content as Grid;
	      if (chartTraderGrid == null) return;
		  
	      InitializeButtonDefinitions();
	      CreateAllControls();
		  UpdateManualAutoButtonStates();
		  UpdateRegimeFilterButtonStates();
		  UpdateStopModeButtonStates();
		  UpdateTargetModeButtonStates();
		  UpdateDirectionButtonStates();
		  UpdateBotPerformanceFilterButtonState();
		  UpdateBreakevenModeButtonStates();
		  UpdateAutoSizeButtonState();
		  UpdateScaleOutButtonState();
	      InitializeUIGrid();
		  UpdateTrendAlertPanel();
	
		  panelScrollViewer = new ScrollViewer()
			{
				Content = lowerButtonsGrid,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				MaxHeight = 600 
			};
	
	      InsertCustomPanel();
	      chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
	    }

		private void CreateAllControls()
		{						
			Style basicBtnStyle	= Application.Current.FindResource("BasicEntryButton") as Style;
			
			accountSelector = new ComboBox { Margin = new Thickness(2), Height = 28 };
			quantitySelector = new QuantityUpDown { Margin = new Thickness(2), Height = 28, VerticalAlignment = VerticalAlignment.Center };
			
			accountSelector.ItemsSource = Account.All.ToList();
			accountSelector.DisplayMemberPath = "Name";
			accountSelector.SelectedItem = Account.All.FirstOrDefault(a => a.Name == Account.Name);
			accountSelector.SelectionChanged += OnAccountSelectorChanged;
			
			quantitySelector.Value = this.Contracts;
			quantitySelector.ValueChanged += OnQuantitySelectorChanged;

			orderTypeSelector = new ComboBox { Name = ORDER_TYPE_SELECTOR_NAME, Margin = new Thickness(2), Height = 28 };
			orderTypeSelector.ItemsSource = Enum.GetValues(typeof(orderSelector));
			orderTypeSelector.SelectedItem = OrderSelector;
			orderTypeSelector.SelectionChanged += OnOrderTypeChanged;

			ptCalculationModeSelector = new ComboBox { Name = PT_SELECTOR_NAME, Margin = new Thickness(2), Height = 28 };
			ptCalculationModeSelector.ItemsSource = Enum.GetValues(typeof(ProfitTargetCalculationMode));
			ptCalculationModeSelector.SelectedItem = ProfitTargetMode;
			ptCalculationModeSelector.SelectionChanged += OnProfitTargetSelectorChanged;
			
			initialStopModeSelector = new ComboBox { Name = INITIAL_STOP_MODE_SELECTOR_NAME, Margin = new Thickness(2), Height = 28 };
			initialStopModeSelector.ItemsSource = Enum.GetValues(typeof(InitialStopCalculationMode));
			initialStopModeSelector.SelectedItem = InitialStopMode;
			initialStopModeSelector.SelectionChanged += OnInitialStopModeSelectorChanged;
			
			stopTypeSelector = new ComboBox { Name = STOP_SELECTOR_NAME, Margin = new Thickness(2), Height = 28 };
			stopTypeSelector.ItemsSource = Enum.GetValues(typeof(StopManagementType));
			stopTypeSelector.SelectedItem = StopType;
			stopTypeSelector.SelectionChanged += OnStopTypeSelectorChanged;

			beTriggerModeSelector = new ComboBox { Name = BE_TRIGGER_MODE_SELECTOR_NAME, Margin = new Thickness(2), Height = 28 };
			beTriggerModeSelector.ItemsSource = Enum.GetValues(typeof(BETriggerMode));
			beTriggerModeSelector.SelectedItem = BreakevenTriggerMode;
			beTriggerModeSelector.SelectionChanged += OnBETriggerModeChanged;
			
			masterTrendFilterSelector = new ComboBox { Name = MASTER_TREND_FILTER_SELECTOR_NAME, Margin = new Thickness(2), Height = 28 };
			masterTrendFilterSelector.ItemsSource = Enum.GetValues(typeof(MasterTrendFilterMode));
			masterTrendFilterSelector.SelectedItem = FilterMode;
			masterTrendFilterSelector.SelectionChanged += OnMasterTrendFilterChanged;

			chopDetectionSelector = new ComboBox { Name = CHOP_DETECTION_SELECTOR_NAME, Margin = new Thickness(2), Height = 28 };
			chopDetectionSelector.ItemsSource = Enum.GetValues(typeof(ChopDetectionMode));
			chopDetectionSelector.SelectedItem = ChopDetectionMethod;
			chopDetectionSelector.SelectionChanged += OnChopDetectionChanged;

			CreateNumericInput(nameof(initialStopTextBox), ref initialStopTextBox, ref initialStopUp, ref initialStopDown, InitialStop.ToString());
			CreateNumericInput(nameof(profitTargetTextBox), ref profitTargetTextBox, ref profitTargetUp, ref profitTargetDown, ProfitTarget.ToString());
			CreateNumericInput(nameof(limitOffsetTextBox), ref limitOffsetTextBox, ref limitOffsetUp, ref limitOffsetDown, LimitOffset.ToString());
			CreateNumericInput(nameof(rrRatioTextBox), ref rrRatioTextBox, ref rrRatioUp, ref rrRatioDown, RiskRewardRatio.ToString("F1"));
			CreateNumericInput(nameof(closePartialTextBox), ref closePartialTextBox, ref closePartialUp, ref closePartialDown, "1");
			CreateNumericInput(nameof(manualMoveStopLookbackTextBox), ref manualMoveStopLookbackTextBox, ref manualMoveStopLookbackUp, ref manualMoveStopLookbackDown, ManualMoveStopLookback.ToString());
			CreateNumericInput(nameof(rangeFilteredStopLossBandTextBox), ref rangeFilteredStopLossBandTextBox, ref rangeFilteredStopLossBandUp, ref rangeFilteredStopLossBandDown, RangeFilteredStopLossBand.ToString());
			CreateNumericInput(nameof(rangeFilteredTakeProfitBandTextBox), ref rangeFilteredTakeProfitBandTextBox, ref rangeFilteredTakeProfitBandUp, ref rangeFilteredTakeProfitBandDown, RangeFilteredTakeProfitBand.ToString());
			CreateNumericInput(nameof(dtpStopLossBandTextBox), ref dtpStopLossBandTextBox, ref dtpStopLossBandUp, ref dtpStopLossBandDown, DTPStopLossBand.ToString());
			CreateNumericInput(nameof(dtpTakeProfitBandTextBox), ref dtpTakeProfitBandTextBox, ref dtpTakeProfitBandUp, ref dtpTakeProfitBandDown, DTPTakeProfitBand.ToString());
			CreateNumericInput(nameof(initialTrailTicksTextBox), ref initialTrailTicksTextBox, ref initialTrailTicksUp, ref initialTrailTicksDown, InitialTrailTicks.ToString());
			CreateNumericInput(nameof(finalTrailTriggerPercentTextBox), ref finalTrailTriggerPercentTextBox, ref finalTrailTriggerPercentUp, ref finalTrailTriggerPercentDown, FinalTrailTriggerPercent.ToString());
			CreateNumericInput(nameof(finalTrailTicksTextBox), ref finalTrailTicksTextBox, ref finalTrailTicksUp, ref finalTrailTicksDown, FinalTrailTicks.ToString());
			CreateNumericInput(nameof(highLowTrailLookbackTextBox), ref highLowTrailLookbackTextBox, ref highLowTrailLookbackUp, ref highLowTrailLookbackDown, HighLowTrailInitialLookback.ToString());
			CreateNumericInput(nameof(hybridTrailTriggerPercentTextBox), ref hybridTrailTriggerPercentTextBox, ref hybridTrailTriggerPercentUp, ref hybridTrailTriggerPercentDown, HybridTrailTriggerPercent.ToString());
			CreateNumericInput(nameof(stagedTrailTriggerPercentTextBox), ref stagedTrailTriggerPercentTextBox, ref stagedTrailTriggerPercentUp, ref stagedTrailTriggerPercentDown, StagedTrailTriggerPercent.ToString());
			CreateNumericInput(nameof(beTriggerTextBox), ref beTriggerTextBox, ref beTriggerUp, ref beTriggerDown, BE_Trigger.ToString());
			CreateNumericInput(nameof(beOffsetTextBox), ref beOffsetTextBox, ref beOffsetUp, ref beOffsetDown, BE_Offset.ToString());
            CreateNumericInput(nameof(scaleOutLevel1PctTextBox), ref scaleOutLevel1PctTextBox, ref scaleOutLevel1PctUp, ref scaleOutLevel1PctDown, ScaleOutLevel1Percent.ToString());
            CreateNumericInput(nameof(scaleOutQty1TextBox), ref scaleOutQty1TextBox, ref scaleOutQty1Up, ref scaleOutQty1Down, ScaleOutQty1.ToString());
            CreateNumericInput(nameof(scaleOutLevel2PctTextBox), ref scaleOutLevel2PctTextBox, ref scaleOutLevel2PctUp, ref scaleOutLevel2PctDown, ScaleOutLevel2Percent.ToString());
            CreateNumericInput(nameof(scaleOutQty2TextBox), ref scaleOutQty2TextBox, ref scaleOutQty2Up, ref scaleOutQty2Down, ScaleOutQty2.ToString());

			// Updated ATR to F2 and added Std Dev Input
			CreateNumericInput(nameof(atrMultiplierTextBox), ref atrMultiplierTextBox, ref atrMultiplierUp, ref atrMultiplierDown, AtrMultiplier.ToString("F2"));
			CreateNumericInput(nameof(atrPeriodTextBox), ref atrPeriodTextBox, ref atrPeriodUp, ref atrPeriodDown, AtrPeriod.ToString());
			CreateNumericInput(nameof(stdDevMultiplierTextBox), ref stdDevMultiplierTextBox, ref stdDevMultiplierUp, ref stdDevMultiplierDown, StdDevMultiplier.ToString("F1"));

			autoBtn = CreateButton(AutoButton, basicBtnStyle);
			longBtn = CreateButton(LongButton, basicBtnStyle); shortBtn = CreateButton(ShortButton, basicBtnStyle);
			quickLongBtn = CreateButton(QuickLongButton, basicBtnStyle); quickShortBtn = CreateButton(QuickShortButton, basicBtnStyle);
			BEBtn = CreateButton(BEButton, basicBtnStyle); TSBtn = CreateButton(TSButton, basicBtnStyle);
			moveTSBtn = CreateButton(MoveTSButton, basicBtnStyle); moveTS50PctBtn = CreateButton(MoveTS50PctButton, basicBtnStyle);
			moveToBEBtn = CreateButton(MoveToBeButton, basicBtnStyle); add1Btn = CreateButton(AddOneButton, basicBtnStyle);
			close1Btn = CreateButton(CloseOneButton, basicBtnStyle); closeBtn = CreateButton(CloseButton, basicBtnStyle);
			panicBtn = CreateButton(PanicButton, basicBtnStyle); donateBtn = CreateButton(DonateButton, basicBtnStyle);
			errorResetBtn = CreateButton(ErrorResetButton, basicBtnStyle);			
			
			trendBotsBtn = CreateButton(TrendBotsButton, basicBtnStyle);
			rangeBotsBtn = CreateButton(RangeBotsButton, basicBtnStyle);
			breakoutBotsBtn = CreateButton(BreakoutBotsButton, basicBtnStyle);
			
			regimeFilterOnBtn = CreateButton(RegimeFilterOnButton, basicBtnStyle);
			regimeFilterOffBtn = CreateButton(RegimeFilterOffButton, basicBtnStyle);

			staticSlBtn = CreateButton(StaticSlButton, basicBtnStyle);
			dynamicSlBtn = CreateButton(DynamicSlButton, basicBtnStyle);
			staticTpBtn = CreateButton(StaticTpButton, basicBtnStyle);
			dynamicTpBtn = CreateButton(DynamicTpButton, basicBtnStyle);
			
			staticBeBtn = CreateButton(StaticBeButton, basicBtnStyle);
			dynamicBeBtn = CreateButton(DynamicBeButton, basicBtnStyle);
			botPerfFilterBtn = CreateButton(BotPerfFilterButton, basicBtnStyle);
			
			autoSizeBtn = CreateButton(AutoSizeButton, basicBtnStyle);
			scaleOutBtn = CreateButton(ScaleOutButton, basicBtnStyle);
			enableFilterBtn = CreateButton(EnableFilterButton, basicBtnStyle);
		}

	    private Button CreateButton(string name, Style style)
	    {
	      var def = buttonDefinitions.FirstOrDefault(b => b.Name == name);
	      if (def == null) return null;
	      var btn = new Button { Name = name, Content = def.Content, Height = 28, Margin = new Thickness(2), Style = style, IsEnabled = true, ToolTip = def.ToolTip, HorizontalAlignment = HorizontalAlignment.Stretch };
	      def.InitialDecoration?.Invoke(this, btn);
	      btn.Click += OnButtonClick;
	      return btn;
	    }
		
		private void CreateNumericInput(string name, ref TextBox textBox, ref Button upButton, ref Button downButton, string initialValue)
		{
			textBox = new TextBox
			{
				Name = name, Text = initialValue, Height = 28, Width = 40,
				VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center,
				BorderThickness = new Thickness(1), BorderBrush = Brushes.Gray,
				Background = Brushes.White, Foreground = Brushes.Black, FontWeight = FontWeights.Bold
			};
			textBox.PreviewKeyDown += OnNumericTextBoxPreviewKeyDown;
			textBox.LostFocus += OnNumericTextBoxLostFocus;
			
			upButton = new Button 
			{
				Name = $"{name}Up", Content = "  +  ", Width = 28, Height = 28, 
				Background = Brushes.DarkGreen, Foreground = Brushes.White, FontWeight = FontWeights.Bold,
				FontSize = 20, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center
			};
			upButton.Click += OnNumericUpDownClick;
			
			downButton = new Button 
			{ 
				Name = $"{name}Down", Content = "  -  ", Width = 28, Height = 28, 
				Background = Brushes.DarkRed, Foreground = Brushes.White, FontWeight = FontWeights.Bold,
				FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
			};
			downButton.Click += OnNumericUpDownClick;
		}
		
		private void InitializeUIGrid()
		{
		    lowerButtonsGrid = new Grid { Margin = new Thickness(2, 0, 2, 0) };
		    for (int i = 0; i < 8; i++)
		        lowerButtonsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			
			trendAlertPanel = new Border
			{
				Background = new SolidColorBrush(Color.FromArgb(255, 255, 248, 220)),
				BorderBrush = Brushes.DarkOrange, BorderThickness = new Thickness(2),
				CornerRadius = new CornerRadius(5), Margin = new Thickness(2, 5, 2, 5),
				Padding = new Thickness(5), Visibility = Visibility.Collapsed
			};
			var alertStack = new StackPanel();
			alertStack.Children.Add(new TextBlock { 
				Text = "Trend Detected. Activate Filter?", 
				FontWeight = FontWeights.Bold, 
				Foreground = Brushes.Black,
				HorizontalAlignment = HorizontalAlignment.Center
			});
			alertStack.Children.Add(enableFilterBtn);
			trendAlertPanel.Child = alertStack;
			Grid.SetRow(trendAlertPanel, 0);
			lowerButtonsGrid.Children.Add(trendAlertPanel);
		
		    var tradeExecExpander = new Expander { Header = "Trade Execution", IsExpanded = true, FontWeight = FontWeights.Bold };
		    var tradeExecGrid = new Grid();
		    tradeExecGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
		    tradeExecGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		    AddLabelAndControl(tradeExecGrid, "Account:", accountSelector);
		    AddLabelAndControl(tradeExecGrid, "Quantity:", quantitySelector);
		    tradeExecExpander.Content = tradeExecGrid;
		    Grid.SetRow(tradeExecExpander, 1);
		    lowerButtonsGrid.Children.Add(tradeExecExpander);
			
			var masterTrendExpander = new Expander { Header = "Master Trend", IsExpanded = true, FontWeight = FontWeights.Bold };
			var masterTrendGrid = new Grid();
			masterTrendGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
			masterTrendGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			AddLabelAndControl(masterTrendGrid, "Filter Mode:", masterTrendFilterSelector);
			AddLabelAndControl(masterTrendGrid, "Chop Filter:", chopDetectionSelector);
			masterTrendExpander.Content = masterTrendGrid;
			Grid.SetRow(masterTrendExpander, 2);
			lowerButtonsGrid.Children.Add(masterTrendExpander);
		    
		    var coreParamsExpander = new Expander { Header = "Core Parameters", IsExpanded = false, FontWeight = FontWeights.Bold };
		    var coreParamsGrid = new Grid();
		    coreParamsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
		    coreParamsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			AddLabelAndControl(coreParamsGrid, "Order Type:", orderTypeSelector);

			limitOffsetGrid = CreateNumericInputGrid(limitOffsetDown, limitOffsetTextBox, limitOffsetUp);
			limitOffsetLabel = AddLabelAndControl(coreParamsGrid, "Limit Offset (Ticks):", limitOffsetGrid);
            
			AddLabelAndControl(coreParamsGrid, "Stop Type:", initialStopModeSelector);
			initialStopGrid = CreateNumericInputGrid(initialStopDown, initialStopTextBox, initialStopUp);
			initialStopLabel = AddLabelAndControl(coreParamsGrid, "Initial Stop (Ticks):", initialStopGrid);
			dtpStopLossBandGrid = CreateNumericInputGrid(dtpStopLossBandDown, dtpStopLossBandTextBox, dtpStopLossBandUp);
			dtpStopLossBandLabel = AddLabelAndControl(coreParamsGrid, "DTP Stop Band:", dtpStopLossBandGrid);
			rangeFilteredStopLossBandGrid = CreateNumericInputGrid(rangeFilteredStopLossBandDown, rangeFilteredStopLossBandTextBox, rangeFilteredStopLossBandUp);
			rangeFilteredStopLossBandLabel = AddLabelAndControl(coreParamsGrid, "RangeFiltered Stop Band:", rangeFilteredStopLossBandGrid);

			AddLabelAndControl(coreParamsGrid, "Target Type:", ptCalculationModeSelector);
			profitTargetGrid = CreateNumericInputGrid(profitTargetDown, profitTargetTextBox, profitTargetUp);
			profitTargetLabel = AddLabelAndControl(coreParamsGrid, "Profit Target (Ticks):", profitTargetGrid);
			dtpTakeProfitBandGrid = CreateNumericInputGrid(dtpTakeProfitBandDown, dtpTakeProfitBandTextBox, dtpTakeProfitBandUp);
			dtpTakeProfitBandLabel = AddLabelAndControl(coreParamsGrid, "DTP Target Band:", dtpTakeProfitBandGrid);
			rangeFilteredTakeProfitBandGrid = CreateNumericInputGrid(rangeFilteredTakeProfitBandDown, rangeFilteredTakeProfitBandTextBox, rangeFilteredTakeProfitBandUp);
			rangeFilteredTakeProfitBandLabel = AddLabelAndControl(coreParamsGrid, "RangeFiltered Target Band:", rangeFilteredTakeProfitBandGrid); 
			rrRatioGrid = CreateNumericInputGrid(rrRatioDown, rrRatioTextBox, rrRatioUp);
			rrRatioLabel = AddLabelAndControl(coreParamsGrid, "R:R Ratio:", rrRatioGrid);

			AddLabelAndControl(coreParamsGrid, "In-Trade Stop Type:", stopTypeSelector);
			hybridTrailTriggerPercentGrid = CreateNumericInputGrid(hybridTrailTriggerPercentDown, hybridTrailTriggerPercentTextBox, hybridTrailTriggerPercentUp);
			hybridTrailTriggerPercentLabel = AddLabelAndControl(coreParamsGrid, "Hybrid Trigger %:", hybridTrailTriggerPercentGrid);
			highLowTrailLookbackGrid = CreateNumericInputGrid(highLowTrailLookbackDown, highLowTrailLookbackTextBox, highLowTrailLookbackUp);
			highLowTrailLookbackLabel = AddLabelAndControl(coreParamsGrid, "High/Low Trail Lookback:", highLowTrailLookbackGrid);
			stagedTrailTriggerPercentGrid = CreateNumericInputGrid(stagedTrailTriggerPercentDown, stagedTrailTriggerPercentTextBox, stagedTrailTriggerPercentUp);
			stagedTrailTriggerPercentLabel = AddLabelAndControl(coreParamsGrid, "Start Trail Trigger %:", stagedTrailTriggerPercentGrid);
			initialTrailTicksGrid = CreateNumericInputGrid(initialTrailTicksDown, initialTrailTicksTextBox, initialTrailTicksUp);
			initialTrailTicksLabel = AddLabelAndControl(coreParamsGrid, "Initial Trail (Ticks):", initialTrailTicksGrid);
			finalTrailTriggerPercentGrid = CreateNumericInputGrid(finalTrailTriggerPercentDown, finalTrailTriggerPercentTextBox, finalTrailTriggerPercentUp);
			finalTrailTriggerPercentLabel = AddLabelAndControl(coreParamsGrid, "Final Trail Trigger %:", finalTrailTriggerPercentGrid);
			finalTrailTicksGrid = CreateNumericInputGrid(finalTrailTicksDown, finalTrailTicksTextBox, finalTrailTicksUp);
			finalTrailTicksLabel = AddLabelAndControl(coreParamsGrid, "Final Trail (Ticks):", finalTrailTicksGrid);
			
			AddLabelAndControl(coreParamsGrid, "ATR Period:", CreateNumericInputGrid(atrPeriodDown, atrPeriodTextBox, atrPeriodUp));
			AddLabelAndControl(coreParamsGrid, "ATR Multiplier:", CreateNumericInputGrid(atrMultiplierDown, atrMultiplierTextBox, atrMultiplierUp));
			AddLabelAndControl(coreParamsGrid, "Std. Dev. Multiplier:", CreateNumericInputGrid(stdDevMultiplierDown, stdDevMultiplierTextBox, stdDevMultiplierUp));

			AddLabelAndControl(coreParamsGrid, "BE Trigger Mode:", beTriggerModeSelector);
			beTriggerLabel = AddLabelAndControl(coreParamsGrid, "BE Trigger (Ticks):", CreateNumericInputGrid(beTriggerDown, beTriggerTextBox, beTriggerUp));
		    AddLabelAndControl(coreParamsGrid, "BE Offset (Ticks):", CreateNumericInputGrid(beOffsetDown, beOffsetTextBox, beOffsetUp));
		    AddLabelAndControl(coreParamsGrid, "Move Stop Lookback:", CreateNumericInputGrid(manualMoveStopLookbackDown, manualMoveStopLookbackTextBox, manualMoveStopLookbackUp));
            AddLabelAndControl(coreParamsGrid, "Scale-Out Lvl 1 (%):", CreateNumericInputGrid(scaleOutLevel1PctDown, scaleOutLevel1PctTextBox, scaleOutLevel1PctUp));
            AddLabelAndControl(coreParamsGrid, "Scale-Out Qty 1:", CreateNumericInputGrid(scaleOutQty1Down, scaleOutQty1TextBox, scaleOutQty1Up));
            AddLabelAndControl(coreParamsGrid, "Scale-Out Lvl 2 (%):", CreateNumericInputGrid(scaleOutLevel2PctDown, scaleOutLevel2PctTextBox, scaleOutLevel2PctUp));
            AddLabelAndControl(coreParamsGrid, "Scale-Out Qty 2:", CreateNumericInputGrid(scaleOutQty2Down, scaleOutQty2TextBox, scaleOutQty2Up));
		    AddLabelAndControl(coreParamsGrid, "Close Quantity:", CreateNumericInputGrid(closePartialDown, closePartialTextBox, closePartialUp));
		    coreParamsExpander.Content = coreParamsGrid;
		    Grid.SetRow(coreParamsExpander, 3);
		    lowerButtonsGrid.Children.Add(coreParamsExpander);
		
		    var modeControlExpander = new Expander { Header = "Mode & Category Control", IsExpanded = true, FontWeight = FontWeights.Bold };
		    var modeControlGrid = new Grid();
		    for (int i = 0; i < 3; i++) modeControlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		    modeControlGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		    modeControlGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			modeControlGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
		    
			AddButtonToGrid(modeControlGrid, autoBtn, 0, 0);
		    AddButtonToGrid(modeControlGrid, longBtn, 0, 1);
			AddButtonToGrid(modeControlGrid, shortBtn, 0, 2);
		    
			AddButtonToGrid(modeControlGrid, trendBotsBtn, 1, 0);
			AddButtonToGrid(modeControlGrid, rangeBotsBtn, 1, 1);
		    AddButtonToGrid(modeControlGrid, breakoutBotsBtn, 1, 2);
			
			var regimeGrid = new Grid();
			regimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			regimeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			AddButtonToGrid(regimeGrid, regimeFilterOnBtn, 0, 0);
			AddButtonToGrid(regimeGrid, regimeFilterOffBtn, 0, 1);
			Grid.SetRow(regimeGrid, 2); Grid.SetColumn(regimeGrid, 0); Grid.SetColumnSpan(regimeGrid, 3);
			modeControlGrid.Children.Add(regimeGrid);
			
		    modeControlExpander.Content = modeControlGrid;
		    Grid.SetRow(modeControlExpander, 4);
		    lowerButtonsGrid.Children.Add(modeControlExpander);
		
		    var tradeMgmtExpander = new Expander { Header = "Trade Management Mode", IsExpanded = true, FontWeight = FontWeights.Bold };
		    var tradeMgmtGrid = new Grid();
		    tradeMgmtGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		    tradeMgmtGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			
			AddButtonToGrid(tradeMgmtGrid, staticSlBtn, 0, 0);
		    AddButtonToGrid(tradeMgmtGrid, dynamicSlBtn, 0, 1);
			AddButtonToGrid(tradeMgmtGrid, staticTpBtn, 1, 0);
		    AddButtonToGrid(tradeMgmtGrid, dynamicTpBtn, 1, 1);
			
			AddButtonToGrid(tradeMgmtGrid, BEBtn, 2, 0, 2); 
			AddButtonToGrid(tradeMgmtGrid, staticBeBtn, 3, 0);
			AddButtonToGrid(tradeMgmtGrid, dynamicBeBtn, 3, 1);

			AddButtonToGrid(tradeMgmtGrid, autoSizeBtn, 4, 0);
			AddButtonToGrid(tradeMgmtGrid, scaleOutBtn, 4, 1);

			AddButtonToGrid(tradeMgmtGrid, botPerfFilterBtn, 5, 0, 2);
		    tradeMgmtExpander.Content = tradeMgmtGrid;
		    Grid.SetRow(tradeMgmtExpander, 5);
		    lowerButtonsGrid.Children.Add(tradeMgmtExpander);
		    
		    var inTradeExpander = new Expander { Header = "In-Trade Management", IsExpanded = true, FontWeight = FontWeights.Bold };
		    var inTradeGrid = new Grid();
		    for (int i = 0; i < 2; i++) inTradeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		    AddButtonToGrid(inTradeGrid, quickLongBtn, 0, 0); AddButtonToGrid(inTradeGrid, quickShortBtn, 0, 1);
		    AddButtonToGrid(inTradeGrid, add1Btn, 1, 0); AddButtonToGrid(inTradeGrid, close1Btn, 1, 1);
		    AddButtonToGrid(inTradeGrid, moveToBEBtn, 2, 0); AddButtonToGrid(inTradeGrid, moveTS50PctBtn, 2, 1);
		    AddButtonToGrid(inTradeGrid, moveTSBtn, 3, 0);
		    AddButtonToGrid(inTradeGrid, closeBtn, 3, 1);
		    inTradeExpander.Content = inTradeGrid;
		    Grid.SetRow(inTradeExpander, 6);
		    lowerButtonsGrid.Children.Add(inTradeExpander);
		    
		    var systemExpander = new Expander { Header = "System", IsExpanded = true, FontWeight = FontWeights.Bold };
		    var systemGrid = new Grid();
		    for (int i = 0; i < 3; i++) systemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		    AddButtonToGrid(systemGrid, panicBtn, 0, 0);
		    AddButtonToGrid(systemGrid, errorResetBtn, 0, 1);
		    AddButtonToGrid(systemGrid, donateBtn, 0, 2);
		    systemExpander.Content = systemGrid;
		    Grid.SetRow(systemExpander, 7);
		    lowerButtonsGrid.Children.Add(systemExpander);

			UpdateBETriggerUI();
			UpdateParameterVisibility();
		}

		private Grid CreateNumericInputGrid(Button down, TextBox text, Button up)
		{
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
			
			Grid.SetColumn(down, 0); grid.Children.Add(down);
			Grid.SetColumn(text, 1); grid.Children.Add(text);
			Grid.SetColumn(up, 2); grid.Children.Add(up);
			return grid;
		}
		
		private TextBlock AddLabelAndControl(Grid grid, string labelText, UIElement control)
		{
			int newRowIndex = grid.RowDefinitions.Count;
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			var textBlock = new TextBlock { Text = labelText, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2,2,5,2) };
			Grid.SetRow(textBlock, newRowIndex); Grid.SetColumn(textBlock, 0);
			Grid.SetRow(control, newRowIndex); Grid.SetColumn(control, 1);
			grid.Children.Add(textBlock);
			grid.Children.Add(control);
			return textBlock;
		}
		
		private void AddButtonToGrid(Grid grid, Button button, int row, int column, int colSpan = 1)
		{
			if(grid.RowDefinitions.Count <= row) grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			Grid.SetRow(button, row);
			Grid.SetColumn(button, column);
			if (colSpan > 1) Grid.SetColumnSpan(button, colSpan);
			grid.Children.Add(button);
		}

		#endregion
		
		#region UI Event Handlers and State Management
		
		public void UpdateTrendAlertPanel()
		{
			if (trendAlertPanel != null)
			{
				trendAlertPanel.Visibility = isTrendOverrideSuggested ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		private void OnAccountSelectorChanged(object sender, SelectionChangedEventArgs e)
		{
			if (accountSelector.SelectedItem is Account selectedAccount && !IsInHitTest)
			{
				Account = selectedAccount;
			}
		}

		private void OnQuantitySelectorChanged(object sender, System.Windows.RoutedEventArgs e)
		{
			if(quantitySelector.Value > 0 && !IsInHitTest)
			{
				Contracts = quantitySelector.Value;
			}
		}
	
		public void UpdateStopModeButtonStates()
		{
		    if (staticSlBtn == null || dynamicSlBtn == null) return;
		    DecorateButton(staticSlBtn, StopMode == TradeManagementMode.Static ? ButtonState.Enabled : ButtonState.Disabled, "Static Stop", "Static Stop");
		    DecorateButton(dynamicSlBtn, StopMode == TradeManagementMode.Dynamic ? ButtonState.Enabled : ButtonState.Disabled, "Dynamic Stop", "Dynamic Stop");
		}
		
		public void UpdateTargetModeButtonStates()
		{
		    if (staticTpBtn == null || dynamicTpBtn == null) return;
		    DecorateButton(staticTpBtn, TargetMode == TradeManagementMode.Static ? ButtonState.Enabled : ButtonState.Disabled, "Static Target", "Static Target");
		    DecorateButton(dynamicTpBtn, TargetMode == TradeManagementMode.Dynamic ? ButtonState.Enabled : ButtonState.Disabled, "Dynamic Target", "Dynamic Target");
		}
		
		public void UpdateBreakevenModeButtonStates()
		{
			if (staticBeBtn == null || dynamicBeBtn == null || BEBtn == null) return;
			
			DecorateButton(BEBtn, BESetAuto ? ButtonState.Enabled : ButtonState.Disabled, "Auto BE ON", "Auto BE OFF");

			bool isEnabled = BESetAuto;
			staticBeBtn.IsEnabled = isEnabled;
			dynamicBeBtn.IsEnabled = isEnabled;

			if (isEnabled)
			{
				DecorateButton(staticBeBtn, BreakevenMode == BreakevenManagementMode.Static ? ButtonState.Enabled : ButtonState.Disabled, "Static BE", "Static BE");
				DecorateButton(dynamicBeBtn, BreakevenMode == BreakevenManagementMode.Dynamic ? ButtonState.Enabled : ButtonState.Disabled, "Dynamic BE", "Dynamic BE");
			}
			else
			{
				DecorateButton(staticBeBtn, ButtonState.Neutral, "Static BE", null, Brushes.White, Brushes.Gray);
				DecorateButton(dynamicBeBtn, ButtonState.Neutral, "Dynamic BE", null, Brushes.White, Brushes.Gray);
			}
		}
		
		public void UpdateBotPerformanceFilterButtonState()
		{
			if (botPerfFilterBtn == null) return;
			DecorateButton(botPerfFilterBtn, EnableBotPerformanceFilter ? ButtonState.Enabled : ButtonState.Disabled, "Bot Perf Filter ON", "Bot Perf Filter OFF");
		}
		
		public void UpdateAutoSizeButtonState()
		{
			if (autoSizeBtn == null) return;
			DecorateButton(autoSizeBtn, EnableDynamicSizing ? ButtonState.Enabled : ButtonState.Disabled, "Auto Sizing ON", "Auto Sizing OFF");
		}
		
		public void UpdateScaleOutButtonState()
		{
			if (scaleOutBtn == null) return;
			DecorateButton(scaleOutBtn, EnableScaleOutExecution ? ButtonState.Enabled : ButtonState.Disabled, "Scale-Out ON", "Scale-Out OFF");
		}
		
		private void ToggleRegimeFilter()
		{
			EnableAutoRegimeDetection = !EnableAutoRegimeDetection;
			UpdateRegimeFilterButtonStates();
			ForceRefresh();
		}

		private void OnNumericUpDownClick(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null) return;
			
			if (btn.Name == $"{nameof(limitOffsetTextBox)}Up") UpdateNumericValue(limitOffsetTextBox, 1);
			else if (btn.Name == $"{nameof(limitOffsetTextBox)}Down") UpdateNumericValue(limitOffsetTextBox, -1);
			else if (btn.Name == $"{nameof(rrRatioTextBox)}Up") UpdateNumericValue(rrRatioTextBox, 0.1, "F1");
			else if (btn.Name == $"{nameof(rrRatioTextBox)}Down") UpdateNumericValue(rrRatioTextBox, -0.1, "F1");
			else if (btn.Name == $"{nameof(initialStopTextBox)}Up") UpdateNumericValue(initialStopTextBox, 1);
			else if (btn.Name == $"{nameof(initialStopTextBox)}Down") UpdateNumericValue(initialStopTextBox, -1);
			else if (btn.Name == $"{nameof(profitTargetTextBox)}Up") UpdateNumericValue(profitTargetTextBox, 1);
			else if (btn.Name == $"{nameof(profitTargetTextBox)}Down") UpdateNumericValue(profitTargetTextBox, -1);
			else if (btn.Name == $"{nameof(rangeFilteredStopLossBandTextBox)}Up") UpdateNumericValue(rangeFilteredStopLossBandTextBox, 1, "F0");
			else if (btn.Name == $"{nameof(rangeFilteredStopLossBandTextBox)}Down") UpdateNumericValue(rangeFilteredStopLossBandTextBox, -1, "F0");
			else if (btn.Name == $"{nameof(rangeFilteredTakeProfitBandTextBox)}Up") UpdateNumericValue(rangeFilteredTakeProfitBandTextBox, 1, "F0");
			else if (btn.Name == $"{nameof(rangeFilteredTakeProfitBandTextBox)}Down") UpdateNumericValue(rangeFilteredTakeProfitBandTextBox, -1, "F0");
			else if (btn.Name == $"{nameof(dtpStopLossBandTextBox)}Up") UpdateNumericValue(dtpStopLossBandTextBox, 1, "F0");
			else if (btn.Name == $"{nameof(dtpStopLossBandTextBox)}Down") UpdateNumericValue(dtpStopLossBandTextBox, -1, "F0");
			else if (btn.Name == $"{nameof(dtpTakeProfitBandTextBox)}Up") UpdateNumericValue(dtpTakeProfitBandTextBox, 1, "F0");
			else if (btn.Name == $"{nameof(dtpTakeProfitBandTextBox)}Down") UpdateNumericValue(dtpTakeProfitBandTextBox, -1, "F0");
			else if (btn.Name == $"{nameof(closePartialTextBox)}Up") UpdateNumericValue(closePartialTextBox, 1);
			else if (btn.Name == $"{nameof(closePartialTextBox)}Down") UpdateNumericValue(closePartialTextBox, -1);
			else if (btn.Name == $"{nameof(manualMoveStopLookbackTextBox)}Up") UpdateNumericValue(manualMoveStopLookbackTextBox, 1);
			else if (btn.Name == $"{nameof(manualMoveStopLookbackTextBox)}Down") UpdateNumericValue(manualMoveStopLookbackTextBox, -1);
			else if (btn.Name == $"{nameof(initialTrailTicksTextBox)}Up") UpdateNumericValue(initialTrailTicksTextBox, 1);
			else if (btn.Name == $"{nameof(initialTrailTicksTextBox)}Down") UpdateNumericValue(initialTrailTicksTextBox, -1);
			else if (btn.Name == $"{nameof(finalTrailTriggerPercentTextBox)}Up") UpdateNumericValue(finalTrailTriggerPercentTextBox, 1);
			else if (btn.Name == $"{nameof(finalTrailTriggerPercentTextBox)}Down") UpdateNumericValue(finalTrailTriggerPercentTextBox, -1);
			else if (btn.Name == $"{nameof(finalTrailTicksTextBox)}Up") UpdateNumericValue(finalTrailTicksTextBox, 1);
			else if (btn.Name == $"{nameof(finalTrailTicksTextBox)}Down") UpdateNumericValue(finalTrailTicksTextBox, -1);
			else if (btn.Name == $"{nameof(highLowTrailLookbackTextBox)}Up") UpdateNumericValue(highLowTrailLookbackTextBox, 1);
			else if (btn.Name == $"{nameof(highLowTrailLookbackTextBox)}Down") UpdateNumericValue(highLowTrailLookbackTextBox, -1);
			else if (btn.Name == $"{nameof(hybridTrailTriggerPercentTextBox)}Up") UpdateNumericValue(hybridTrailTriggerPercentTextBox, 1);
			else if (btn.Name == $"{nameof(hybridTrailTriggerPercentTextBox)}Down") UpdateNumericValue(hybridTrailTriggerPercentTextBox, -1);
			else if (btn.Name == $"{nameof(stagedTrailTriggerPercentTextBox)}Up") UpdateNumericValue(stagedTrailTriggerPercentTextBox, 1);
			else if (btn.Name == $"{nameof(stagedTrailTriggerPercentTextBox)}Down") UpdateNumericValue(stagedTrailTriggerPercentTextBox, -1);
			else if (btn.Name == $"{nameof(beTriggerTextBox)}Up") UpdateNumericValue(beTriggerTextBox, 1);
			else if (btn.Name == $"{nameof(beTriggerTextBox)}Down") UpdateNumericValue(beTriggerTextBox, -1);
			else if (btn.Name == $"{nameof(beOffsetTextBox)}Up") UpdateNumericValue(beOffsetTextBox, 1);
			else if (btn.Name == $"{nameof(beOffsetTextBox)}Down") UpdateNumericValue(beOffsetTextBox, -1);
            else if (btn.Name == $"{nameof(scaleOutLevel1PctTextBox)}Up") UpdateNumericValue(scaleOutLevel1PctTextBox, 1);
            else if (btn.Name == $"{nameof(scaleOutLevel1PctTextBox)}Down") UpdateNumericValue(scaleOutLevel1PctTextBox, -1);
            else if (btn.Name == $"{nameof(scaleOutQty1TextBox)}Up") UpdateNumericValue(scaleOutQty1TextBox, 1);
            else if (btn.Name == $"{nameof(scaleOutQty1TextBox)}Down") UpdateNumericValue(scaleOutQty1TextBox, -1);
            else if (btn.Name == $"{nameof(scaleOutLevel2PctTextBox)}Up") UpdateNumericValue(scaleOutLevel2PctTextBox, 1);
            else if (btn.Name == $"{nameof(scaleOutLevel2PctTextBox)}Down") UpdateNumericValue(scaleOutLevel2PctTextBox, -1);
            else if (btn.Name == $"{nameof(scaleOutQty2TextBox)}Up") UpdateNumericValue(scaleOutQty2TextBox, 1);
            else if (btn.Name == $"{nameof(scaleOutQty2TextBox)}Down") UpdateNumericValue(scaleOutQty2TextBox, -1);
			
			else if (btn.Name == $"{nameof(atrMultiplierTextBox)}Up") UpdateNumericValue(atrMultiplierTextBox, 0.05, "F2");
			else if (btn.Name == $"{nameof(atrMultiplierTextBox)}Down") UpdateNumericValue(atrMultiplierTextBox, -0.05, "F2");
			else if (btn.Name == $"{nameof(atrPeriodTextBox)}Up") UpdateNumericValue(atrPeriodTextBox, 1);
			else if (btn.Name == $"{nameof(atrPeriodTextBox)}Down") UpdateNumericValue(atrPeriodTextBox, -1);
			else if (btn.Name == $"{nameof(stdDevMultiplierTextBox)}Up") UpdateNumericValue(stdDevMultiplierTextBox, 0.1, "F1");
			else if (btn.Name == $"{nameof(stdDevMultiplierTextBox)}Down") UpdateNumericValue(stdDevMultiplierTextBox, -0.1, "F1");
		}

		private void UpdateNumericValue(TextBox textBox, double change, string format = "F0")
		{
			if(double.TryParse(textBox.Text, out double currentValue))
			{
				double newValue = currentValue + change;
				
				if (textBox.Name == nameof(rangeFilteredStopLossBandTextBox) || textBox.Name == nameof(rangeFilteredTakeProfitBandTextBox) || textBox.Name == nameof(dtpStopLossBandTextBox) || textBox.Name == nameof(dtpTakeProfitBandTextBox))
				{
					if (newValue < 1) newValue = 1;
					if (newValue > 4) newValue = 4;
				}
				
				if (newValue < 0 && textBox != rrRatioTextBox) newValue = 0;
                if (newValue < 1 && (textBox == closePartialTextBox || textBox == atrPeriodTextBox)) newValue = 1;
				textBox.Text = newValue.ToString(format);
				ParseTextBoxAndUpdateProperty(textBox);
			}
		}
		
		private void OnNumericTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
		{
			Key[] allowedKeys = { Key.Back, Key.Delete, Key.Tab, Key.Left, Key.Right, Key.Home, Key.End, Key.Enter };
			if (allowedKeys.Contains(e.Key)) { if (e.Key == Key.Enter) { ParseTextBoxAndUpdateProperty(sender as TextBox); Keyboard.ClearFocus(); e.Handled = true; } else { e.Handled = false; } return; }
			
			bool isDecimalAllowed = (sender as TextBox)?.Name == nameof(rrRatioTextBox) || 
			                        (sender as TextBox)?.Name == nameof(atrMultiplierTextBox) ||
									(sender as TextBox)?.Name == nameof(stdDevMultiplierTextBox);
			
			bool isDecimalKey = (e.Key == Key.Decimal || e.Key == Key.OemPeriod);
			if (isDecimalKey && isDecimalAllowed) { e.Handled = false; return; }
			
			bool isNumber = (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
			e.Handled = !isNumber;
		}
		
		private void OnNumericTextBoxLostFocus(object sender, RoutedEventArgs e) { ParseTextBoxAndUpdateProperty(sender as TextBox); }

		private void ParseTextBoxAndUpdateProperty(TextBox textBox)
		{
			if (textBox == null) return;
			if (double.TryParse(textBox.Text, out double parsedValue))
			{
				if (textBox.Name == nameof(rangeFilteredStopLossBandTextBox) || textBox.Name == nameof(rangeFilteredTakeProfitBandTextBox) || textBox.Name == nameof(dtpStopLossBandTextBox) || textBox.Name == nameof(dtpTakeProfitBandTextBox))
				{
				    if (parsedValue > 4) parsedValue = 4;
				    if (parsedValue < 1) parsedValue = 1;
				    textBox.Text = parsedValue.ToString("F0");
				}
				
				if (textBox.Name == nameof(limitOffsetTextBox)) LimitOffset = (int)parsedValue;
				else if (textBox.Name == nameof(rrRatioTextBox)) RiskRewardRatio = parsedValue;
				else if (textBox.Name == nameof(initialStopTextBox)) InitialStop = (int)parsedValue; 
				else if (textBox.Name == nameof(profitTargetTextBox)) ProfitTarget = (int)parsedValue;
				else if (textBox.Name == nameof(rangeFilteredStopLossBandTextBox)) RangeFilteredStopLossBand = (int)parsedValue;
				else if (textBox.Name == nameof(rangeFilteredTakeProfitBandTextBox)) RangeFilteredTakeProfitBand = (int)parsedValue;
				else if (textBox.Name == nameof(dtpStopLossBandTextBox)) DTPStopLossBand = (int)parsedValue;
				else if (textBox.Name == nameof(dtpTakeProfitBandTextBox)) DTPTakeProfitBand = (int)parsedValue;
				else if (textBox.Name == nameof(manualMoveStopLookbackTextBox)) ManualMoveStopLookback = (int)parsedValue;
				else if (textBox.Name == nameof(initialTrailTicksTextBox)) InitialTrailTicks = (int)parsedValue;
				else if (textBox.Name == nameof(finalTrailTriggerPercentTextBox)) FinalTrailTriggerPercent = (int)parsedValue;
				else if (textBox.Name == nameof(finalTrailTicksTextBox)) FinalTrailTicks = (int)parsedValue;
				else if (textBox.Name == nameof(highLowTrailLookbackTextBox)) HighLowTrailInitialLookback = (int)parsedValue;
				else if (textBox.Name == nameof(hybridTrailTriggerPercentTextBox)) HybridTrailTriggerPercent = (int)parsedValue;
				else if (textBox.Name == nameof(stagedTrailTriggerPercentTextBox)) StagedTrailTriggerPercent = (int)parsedValue;
				else if (textBox.Name == nameof(beTriggerTextBox))
				{
				    if (BreakevenTriggerMode == BETriggerMode.FixedTicks) BE_Trigger = (int)parsedValue;
				    else BETriggerPercent = (int)parsedValue;
				}
				else if (textBox.Name == nameof(beOffsetTextBox)) BE_Offset = (int)parsedValue;
                else if (textBox.Name == nameof(scaleOutLevel1PctTextBox)) ScaleOutLevel1Percent = (int)parsedValue;
                else if (textBox.Name == nameof(scaleOutQty1TextBox)) ScaleOutQty1 = (int)parsedValue;
                else if (textBox.Name == nameof(scaleOutLevel2PctTextBox)) ScaleOutLevel2Percent = (int)parsedValue;
                else if (textBox.Name == nameof(scaleOutQty2TextBox)) ScaleOutQty2 = (int)parsedValue;
				else if (textBox.Name == nameof(atrMultiplierTextBox)) AtrMultiplier = parsedValue;
				else if (textBox.Name == nameof(atrPeriodTextBox)) AtrPeriod = (int)parsedValue;
				else if (textBox.Name == nameof(stdDevMultiplierTextBox)) StdDevMultiplier = parsedValue;
			}
		}

		private void OnOrderTypeChanged(object sender, SelectionChangedEventArgs e)
		{
			if (orderTypeSelector.SelectedItem == null || IsInHitTest) return;
			OrderSelector = (orderSelector)orderTypeSelector.SelectedItem;
            UpdateParameterVisibility(); 
			ForceRefresh();
		}
		
		private void OnProfitTargetSelectorChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ptCalculationModeSelector.SelectedItem == null || IsInHitTest) return;
			ProfitTargetMode = (ProfitTargetCalculationMode)ptCalculationModeSelector.SelectedItem;
			UpdateParameterVisibility();
			ForceRefresh();
		}
		
		private void OnInitialStopModeSelectorChanged(object sender, SelectionChangedEventArgs e)
		{
			if (initialStopModeSelector.SelectedItem == null || IsInHitTest) return;
			InitialStopMode = (InitialStopCalculationMode)initialStopModeSelector.SelectedItem;
			UpdateParameterVisibility();
			ForceRefresh();
		}

		private void OnStopTypeSelectorChanged(object sender, SelectionChangedEventArgs e)
		{
			if (stopTypeSelector.SelectedItem == null || IsInHitTest) return;
			StopType = (StopManagementType)stopTypeSelector.SelectedItem;
			UpdateParameterVisibility();
			ForceRefresh();
		}
		
		private void OnMasterTrendFilterChanged(object sender, SelectionChangedEventArgs e)
		{
			if (masterTrendFilterSelector.SelectedItem == null || IsInHitTest) return;
			FilterMode = (MasterTrendFilterMode)masterTrendFilterSelector.SelectedItem;
			UpdateParameterVisibility();
			ForceRefresh();
		}

		private void OnChopDetectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (chopDetectionSelector.SelectedItem == null || IsInHitTest) return;
			ChopDetectionMethod = (ChopDetectionMode)chopDetectionSelector.SelectedItem;
			UpdateParameterVisibility();
			ForceRefresh();
		}

		private void OnBETriggerModeChanged(object sender, SelectionChangedEventArgs e)
		{
			if (beTriggerModeSelector.SelectedItem == null || IsInHitTest) return;
			BreakevenTriggerMode = (BETriggerMode)beTriggerModeSelector.SelectedItem;
			UpdateBETriggerUI();
			ForceRefresh();
		}
		
		private void UpdateParameterVisibility()
		{
			if (orderTypeSelector == null || initialStopModeSelector == null || ptCalculationModeSelector == null || stopTypeSelector == null) return;
			
            bool isMarketOrder = (orderSelector)orderTypeSelector.SelectedItem == orderSelector.Market;
            if(limitOffsetLabel != null) limitOffsetLabel.Visibility = isMarketOrder ? Visibility.Collapsed : Visibility.Visible;
            if(limitOffsetGrid != null) limitOffsetGrid.Visibility = isMarketOrder ? Visibility.Collapsed : Visibility.Visible;

			var stopMode = (InitialStopCalculationMode)initialStopModeSelector.SelectedItem;
			bool showFixedStop = stopMode == InitialStopCalculationMode.FixedTicks;
			bool showDtpStop = stopMode == InitialStopCalculationMode.DeviationTrendProfile;
			bool showRangeFilteredStop = stopMode == InitialStopCalculationMode.RangeFiltered;
			
			if (initialStopLabel != null) initialStopLabel.Visibility = showFixedStop ? Visibility.Visible : Visibility.Collapsed;
			if (initialStopGrid != null) initialStopGrid.Visibility = showFixedStop ? Visibility.Visible : Visibility.Collapsed;
			if (rangeFilteredStopLossBandLabel != null) rangeFilteredStopLossBandLabel.Visibility = showRangeFilteredStop ? Visibility.Visible : Visibility.Collapsed;
			if (rangeFilteredStopLossBandGrid != null) rangeFilteredStopLossBandGrid.Visibility = showRangeFilteredStop ? Visibility.Visible : Visibility.Collapsed;
			if (dtpStopLossBandLabel != null) dtpStopLossBandLabel.Visibility = showDtpStop ? Visibility.Visible : Visibility.Collapsed;
			if (dtpStopLossBandGrid != null) dtpStopLossBandGrid.Visibility = showDtpStop ? Visibility.Visible : Visibility.Collapsed;
			
			var targetMode = (ProfitTargetCalculationMode)ptCalculationModeSelector.SelectedItem;
			bool showFixedTarget = targetMode == ProfitTargetCalculationMode.FixedTicks;
			bool showDtpTarget = targetMode == ProfitTargetCalculationMode.DeviationTrendProfile;
			bool showRangeFilteredTarget = targetMode == ProfitTargetCalculationMode.RangeFiltered;
			bool showRRTarget = targetMode == ProfitTargetCalculationMode.RiskRewardRatio;
			
			if (profitTargetLabel != null) profitTargetLabel.Visibility = showFixedTarget ? Visibility.Visible : Visibility.Collapsed;
			if (profitTargetGrid != null) profitTargetGrid.Visibility = showFixedTarget ? Visibility.Visible : Visibility.Collapsed;
			if (rangeFilteredTakeProfitBandLabel != null) rangeFilteredTakeProfitBandLabel.Visibility = showRangeFilteredTarget ? Visibility.Visible : Visibility.Collapsed;
			if (rangeFilteredTakeProfitBandGrid != null) rangeFilteredTakeProfitBandGrid.Visibility = showRangeFilteredTarget ? Visibility.Visible : Visibility.Collapsed;
			if (dtpTakeProfitBandLabel != null) dtpTakeProfitBandLabel.Visibility = showDtpTarget ? Visibility.Visible : Visibility.Collapsed;
			if (dtpTakeProfitBandGrid != null) dtpTakeProfitBandGrid.Visibility = showDtpTarget ? Visibility.Visible : Visibility.Collapsed;
			if (rrRatioLabel != null) rrRatioLabel.Visibility = showRRTarget ? Visibility.Visible : Visibility.Collapsed;
			if (rrRatioGrid != null) rrRatioGrid.Visibility = showRRTarget ? Visibility.Visible : Visibility.Collapsed;
			
			var inTradeStopType = (StopManagementType)stopTypeSelector.SelectedItem;
			bool showStaged = inTradeStopType == StopManagementType.StagedTrail;
			bool showHighLow = inTradeStopType == StopManagementType.HighLow;
			bool showHybrid = inTradeStopType == StopManagementType.HybridAtrPsar;

			if (stagedTrailTriggerPercentLabel != null) stagedTrailTriggerPercentLabel.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			if (stagedTrailTriggerPercentGrid != null) stagedTrailTriggerPercentGrid.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			if (initialTrailTicksLabel != null) initialTrailTicksLabel.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			if (initialTrailTicksGrid != null) initialTrailTicksGrid.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			if (finalTrailTriggerPercentLabel != null) finalTrailTriggerPercentLabel.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			if (finalTrailTriggerPercentGrid != null) finalTrailTriggerPercentGrid.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			if (finalTrailTicksLabel != null) finalTrailTicksLabel.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			if (finalTrailTicksGrid != null) finalTrailTicksGrid.Visibility = showStaged ? Visibility.Visible : Visibility.Collapsed;
			
			if (highLowTrailLookbackLabel != null) highLowTrailLookbackLabel.Visibility = showHighLow ? Visibility.Visible : Visibility.Collapsed;
			if (highLowTrailLookbackGrid != null) highLowTrailLookbackGrid.Visibility = showHighLow ? Visibility.Visible : Visibility.Collapsed;

			if (hybridTrailTriggerPercentLabel != null) hybridTrailTriggerPercentLabel.Visibility = showHybrid ? Visibility.Visible : Visibility.Collapsed;
			if (hybridTrailTriggerPercentGrid != null) hybridTrailTriggerPercentGrid.Visibility = showHybrid ? Visibility.Visible : Visibility.Collapsed;
		}

		private void UpdateBETriggerUI()
		{
		    if (beTriggerLabel == null || beTriggerTextBox == null) return;
		    if (BreakevenTriggerMode == BETriggerMode.FixedTicks)
		    {
		        beTriggerLabel.Text = "BE Trigger (Ticks):";
		        beTriggerTextBox.Text = BE_Trigger.ToString();
		    }
		    else
		    {
		        beTriggerLabel.Text = "BE Trigger (% of PT):";
		        beTriggerTextBox.Text = BETriggerPercent.ToString();
		    }
		}
		
		protected void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			if (!(sender is Button button)) return;
			buttonDefinitions.FirstOrDefault(b => b.Name == button.Name)?.ClickAction?.Invoke(this);
		}

		private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0) return;
			if (TabSelected()) InsertCustomPanel();
			else RemoveWPFControls();
		}		
		
		private bool TabSelected()
		{
			if (chartWindow == null) return false;
			foreach (TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab)?.ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem) return true;
			return false;
		}

		private void InsertCustomPanel()
		{
		    if (panelActive || chartTraderGrid == null) return;
			if (chartTraderGrid.Children.Contains(panelScrollViewer)) return;
		    chartTraderGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
		    Grid.SetRow(panelScrollViewer, chartTraderGrid.RowDefinitions.Count - 1);
		    if (chartTraderGrid.ColumnDefinitions.Count > 0) Grid.SetColumnSpan(panelScrollViewer, chartTraderGrid.ColumnDefinitions.Count);
		    chartTraderGrid.Children.Add(panelScrollViewer);
		    panelActive = true;
		}

		private void RemoveWPFControls()
		{
		    if (!panelActive || chartTraderGrid == null) return;
		    if (panelScrollViewer != null) chartTraderGrid.Children.Remove(panelScrollViewer);
		    if (chartTraderGrid.RowDefinitions.Count > 0) chartTraderGrid.RowDefinitions.RemoveAt(chartTraderGrid.RowDefinitions.Count - 1);
		    panelActive = false;
		}
		
		protected void DisposeWPFControls() 
		{
			if (chartWindow != null) chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			if (accountSelector != null) accountSelector.SelectionChanged -= OnAccountSelectorChanged;
			if (quantitySelector != null) quantitySelector.ValueChanged -= OnQuantitySelectorChanged;
			if (orderTypeSelector != null) orderTypeSelector.SelectionChanged -= OnOrderTypeChanged;
			if (ptCalculationModeSelector != null) ptCalculationModeSelector.SelectionChanged -= OnProfitTargetSelectorChanged;
			if (initialStopModeSelector != null) initialStopModeSelector.SelectionChanged -= OnInitialStopModeSelectorChanged;
			if (stopTypeSelector != null) stopTypeSelector.SelectionChanged -= OnStopTypeSelectorChanged;
			if (beTriggerModeSelector != null) beTriggerModeSelector.SelectionChanged -= OnBETriggerModeChanged;
			if (masterTrendFilterSelector != null) masterTrendFilterSelector.SelectionChanged -= OnMasterTrendFilterChanged;
			if (chopDetectionSelector != null) chopDetectionSelector.SelectionChanged -= OnChopDetectionChanged;

			if (atrMultiplierTextBox != null) { atrMultiplierTextBox.PreviewKeyDown -= OnNumericTextBoxPreviewKeyDown; atrMultiplierTextBox.LostFocus -= OnNumericTextBoxLostFocus; }
			if (atrPeriodTextBox != null) { atrPeriodTextBox.PreviewKeyDown -= OnNumericTextBoxPreviewKeyDown; atrPeriodTextBox.LostFocus -= OnNumericTextBoxLostFocus; }
			if (stdDevMultiplierTextBox != null) { stdDevMultiplierTextBox.PreviewKeyDown -= OnNumericTextBoxPreviewKeyDown; stdDevMultiplierTextBox.LostFocus -= OnNumericTextBoxLostFocus; }

			Action<Button> unsub = (b) => { if (b != null) b.Click -= OnButtonClick; };
			unsub(autoBtn); unsub(longBtn); unsub(shortBtn); unsub(quickLongBtn); unsub(quickShortBtn);
			unsub(add1Btn); unsub(close1Btn); unsub(BEBtn); unsub(TSBtn); unsub(moveTSBtn); unsub(moveTS50PctBtn);
			unsub(moveToBEBtn); unsub(closeBtn); unsub(panicBtn); unsub(donateBtn); unsub(errorResetBtn);
			unsub(regimeFilterOnBtn); unsub(regimeFilterOffBtn);
			unsub(botPerfFilterBtn); unsub(staticBeBtn); unsub(dynamicBeBtn);
			unsub(autoSizeBtn); unsub(scaleOutBtn);
			unsub(staticSlBtn); unsub(dynamicSlBtn); unsub(staticTpBtn); unsub(dynamicTpBtn);
			unsub(enableFilterBtn);
			RemoveWPFControls();
		}
		#endregion

		#region Button Definitions and Decorations
		private List<ButtonDefinition> buttonDefinitions;
		private class ButtonDefinition
		{
			public string Name { get; set; } public string Content { get; set; } public string ToolTip { get; set; }
			public Action<AlgoBase, Button> InitialDecoration { get; set; }
			public Action<AlgoBase> ClickAction { get; set; }
		}

		private void InitializeButtonDefinitions()
		{
			buttonDefinitions = new List<ButtonDefinition>
			{
				new ButtonDefinition { Name = AutoButton, Content = "Strategy Enabled", ToolTip = "Toggle Strategy ON/OFF",
					ClickAction = (s) => { s.SetTradingMode(s.CurrentMode == TradingMode.Auto ? TradingMode.Disabled : TradingMode.Auto); s.UpdateManualAutoButtonStates(); } },
				
				new ButtonDefinition { Name = LongButton, Content = "LONG", ToolTip = "Toggle Auto Longs",
					ClickAction = (s) => { s.isLongEnabled = !s.isLongEnabled; s.isLongManuallyEnabled = s.isLongEnabled; s.UpdateDirectionButtonStates(); } },
				new ButtonDefinition { Name = ShortButton, Content = "SHORT", ToolTip = "Toggle Auto Shorts",
					ClickAction = (s) => { s.isShortEnabled = !s.isShortEnabled; s.isShortManuallyEnabled = s.isShortEnabled; s.UpdateDirectionButtonStates(); } },
				
				new ButtonDefinition { Name = QuickLongButton, Content = "Buy", ToolTip = "Manual Market Buy",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Buy", null, Brushes.White, Brushes.DarkGreen),
					ClickAction = (s) => { if (s.Position.MarketPosition == MarketPosition.Flat) { s.SetTradingMode(TradingMode.Disabled); s.isManualTradeActive = true; s.EnterLongPosition(); } } },
				new ButtonDefinition { Name = QuickShortButton, Content = "Sell", ToolTip = "Manual Market Sell",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Sell", null, Brushes.White, Brushes.DarkRed),
					ClickAction = (s) => { if (s.Position.MarketPosition == MarketPosition.Flat) { s.SetTradingMode(TradingMode.Disabled); s.isManualTradeActive = true; s.EnterShortPosition(); } } },
				new ButtonDefinition { Name = AddOneButton, Content = "Add 1", ToolTip = "Add 1 contract",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Add 1", null, Brushes.White, Brushes.DarkGreen),
					ClickAction = (s) => s.AddOneEntry() },
				new ButtonDefinition { Name = CloseOneButton, Content = "Partial Close", ToolTip = "Partial Close contract (FIFO)",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Partial Close", null, Brushes.White, Brushes.DarkRed),
					ClickAction = (s) => { if(int.TryParse(s.closePartialTextBox.Text, out int qty)) { s.CloseOneExit(qty); } } },
				
				new ButtonDefinition { Name = BEButton, Content = "Auto BE", ToolTip = "Toggle Auto Breakeven",
					ClickAction = (s) => { s.BESetAuto = !s.BESetAuto; s.UpdateBreakevenModeButtonStates(); } },

				new ButtonDefinition { Name = TSButton, Content = "Trailstop On", ToolTip = "Toggle Auto Trailing Stop",
					InitialDecoration = (s, b) => DecorateButton(b, s.StopType != StopManagementType.Fixed ? ButtonState.Enabled : ButtonState.Disabled, "TS On", "TS Off"),
					ClickAction = (s) => { s.StopType = (s.StopType == StopManagementType.Fixed) ? StopManagementType.RegularTrail : StopManagementType.Fixed; DecorateButton(s.TSBtn, s.StopType != StopManagementType.Fixed ? ButtonState.Enabled : ButtonState.Disabled, "TS On", "TS Off"); s.ForceRefresh(); }},
				new ButtonDefinition { Name = MoveToBeButton, Content = "Breakeven", ToolTip = "Move stop to breakeven",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Breakeven", null, Brushes.Yellow, Brushes.DarkBlue),
					ClickAction = (s) => s.MoveToBreakeven() },
				new ButtonDefinition { Name = MoveTSButton, Content = "Move Trailstop", ToolTip = "Move stop to swing point",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Move Trailstop", null, Brushes.Yellow, Brushes.DarkBlue),
					ClickAction = (s) => s.MoveStopToSwingPoint() },
				new ButtonDefinition { Name = MoveTS50PctButton, Content = "Move TS 50%", ToolTip = "Move stop 50% closer to price",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Move TS 50%", null, Brushes.Yellow, Brushes.DarkBlue),
					ClickAction = (s) => s.MoveTrailingStopByPercentage(0.5) },
				new ButtonDefinition { Name = CloseButton, Content = "Close All", ToolTip = "Close all strategy positions",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Close All", null, Brushes.White, Brushes.DarkRed),
					ClickAction = (s) => s.CloseAllOperation(ClAll) },
				new ButtonDefinition { Name = PanicButton, Content = "Panic", ToolTip = "FLATTEN ALL positions for account",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Panic", null, Brushes.Yellow, Brushes.DarkRed),
					ClickAction = (s) => { s.FlattenAllPositions(); s.SetTradingMode(TradingMode.Disabled); s.UpdateManualAutoButtonStates(); } },
				new ButtonDefinition { Name = DonateButton, Content = "Pay", ToolTip = "Payment",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Pay", null, Brushes.Yellow, Brushes.DarkBlue),
					ClickAction = (s) => { if(!string.IsNullOrWhiteSpace(s.paypal)) System.Diagnostics.Process.Start(s.paypal); } },
				new ButtonDefinition { Name = ErrorResetButton, Content = "Reset Error", ToolTip = "Click to reset the 'Order Error' status.",
					InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Reset Error", null, Brushes.White, Brushes.Indigo),
					ClickAction = (s) => { s.orderErrorOccurred = false; } },
				new ButtonDefinition { Name = TrendBotsButton, Content = "Trend", ToolTip = "Toggle Trend Bots",
				    InitialDecoration = (s, b) => DecorateButton(b, s.EnableTrendBots ? ButtonState.Enabled : ButtonState.Disabled, "Trend", "Trend"),
				    ClickAction = (s) => { s.EnableTrendBots = !s.EnableTrendBots; DecorateButton(s.trendBotsBtn, s.EnableTrendBots ? ButtonState.Enabled : ButtonState.Disabled, "Trend", "Trend"); s.ForceRefresh(); } },				
				new ButtonDefinition { Name = RangeBotsButton, Content = "Range", ToolTip = "Toggle Range Bots",
				    InitialDecoration = (s, b) => DecorateButton(b, s.EnableRangeBots ? ButtonState.Enabled : ButtonState.Disabled, "Range", "Range"),
				    ClickAction = (s) => { s.EnableRangeBots = !s.EnableRangeBots; DecorateButton(s.rangeBotsBtn, s.EnableRangeBots ? ButtonState.Enabled : ButtonState.Disabled, "Range", "Range"); s.ForceRefresh(); } },				
				new ButtonDefinition { Name = BreakoutBotsButton, Content = "Breakout", ToolTip = "Toggle Breakout Bots",
				    InitialDecoration = (s, b) => DecorateButton(b, s.EnableBreakoutBots ? ButtonState.Enabled : ButtonState.Disabled, "Breakout", "Breakout"),
				    ClickAction = (s) => { s.EnableBreakoutBots = !s.EnableBreakoutBots; DecorateButton(s.breakoutBotsBtn, s.EnableBreakoutBots ? ButtonState.Enabled : ButtonState.Disabled, "Breakout", "Breakout"); s.ForceRefresh(); } },				
				new ButtonDefinition { Name = RegimeFilterOnButton, Content = "Regime Filter ON", ToolTip = "Toggle regime filtering.",
				    ClickAction = (s) => s.ToggleRegimeFilter() },
				new ButtonDefinition { Name = RegimeFilterOffButton, Content = "All Signals ON", ToolTip = "Toggle regime filtering.",
				    ClickAction = (s) => s.ToggleRegimeFilter() },
		        new ButtonDefinition { Name = StaticSlButton, Content = "Static Stop", ToolTip = "Toggle Static/Dynamic SL",
		            ClickAction = (s) => s.ToggleStopManagementMode() },         
		        new ButtonDefinition { Name = DynamicSlButton, Content = "Dynamic Stop", ToolTip = "Toggle Static/Dynamic SL",
		            ClickAction = (s) => s.ToggleStopManagementMode() },
		        new ButtonDefinition { Name = StaticTpButton, Content = "Static Target", ToolTip = "Toggle Static/Dynamic TP",
		            ClickAction = (s) => s.ToggleTargetManagementMode() },         
		        new ButtonDefinition { Name = DynamicTpButton, Content = "Dynamic Target", ToolTip = "Toggle Static/Dynamic TP",
		            ClickAction = (s) => s.ToggleTargetManagementMode() },
				new ButtonDefinition { Name = StaticBeButton, Content = "Static BE", ToolTip = "Toggle Static/Dynamic BE",
					ClickAction = (s) => { s.ToggleBreakevenMode(); } },
				new ButtonDefinition { Name = DynamicBeButton, Content = "Dynamic BE", ToolTip = "Toggle Static/Dynamic BE",
					ClickAction = (s) => { s.ToggleBreakevenMode(); } },
				new ButtonDefinition { Name = BotPerfFilterButton, Content = "Bot Performance Filter ON", ToolTip = "Toggle Performance Filter",
					ClickAction = (s) => { s.EnableBotPerformanceFilter = !s.EnableBotPerformanceFilter; s.UpdateBotPerformanceFilterButtonState(); } },
				new ButtonDefinition { Name = AutoSizeButton, Content = "Auto Sizing ON", ToolTip = "Toggle Dynamic Sizing",
				    ClickAction = (s) => { s.EnableDynamicSizing = !s.EnableDynamicSizing; s.UpdateAutoSizeButtonState(); s.ForceRefresh(); } },
				new ButtonDefinition { Name = ScaleOutButton, Content = "Scale-Out ON", ToolTip = "Toggle Scale-Out Execution",
				    ClickAction = (s) => { s.EnableScaleOutExecution = !s.EnableScaleOutExecution; s.UpdateScaleOutButtonState(); s.ForceRefresh(); } },
				new ButtonDefinition { Name = EnableFilterButton, Content = "Enable RangeFiltered Mode", ToolTip = "Click to enable RangeFiltered mode.",
				    InitialDecoration = (s, b) => DecorateButton(b, ButtonState.Neutral, "Enable RangeFiltered Mode", null, Brushes.White, Brushes.DarkGreen),
				    ClickAction = (s) => { s.FilterMode = MasterTrendFilterMode.RangeFiltered; s.isTrendOverrideSuggested = false; s.UpdateTrendAlertPanel(); s.ForceRefresh(); }}
			};
		}

		private enum ButtonState { Enabled, Disabled, Neutral }

        public void UpdateManualAutoButtonStates()
		{
		    if (autoBtn == null) return;
		    DecorateButton(autoBtn, CurrentMode == TradingMode.Auto ? ButtonState.Enabled : ButtonState.Disabled, "Strategy ON", "Strategy OFF");
		    if (CurrentMode == TradingMode.DisabledByChop) { autoBtn.Content = "AUTO OFF (CHOP)"; autoBtn.Background = Brushes.Gray; }
		}

		public void UpdateDirectionButtonStates()
		{
			if (longBtn == null || shortBtn == null) return;
			DecorateButton(longBtn, isLongEnabled ? ButtonState.Enabled : ButtonState.Disabled, "LONG", "LONG OFF");
			DecorateButton(shortBtn, isShortEnabled ? ButtonState.Enabled : ButtonState.Disabled, "SHORT", "SHORT OFF");
		}
		
		public void UpdateRegimeFilterButtonStates()
		{
		    if (regimeFilterOnBtn == null || regimeFilterOffBtn == null) return;
			if (EnableAutoRegimeDetection)
			{
				DecorateButton(regimeFilterOnBtn, ButtonState.Enabled, "Regime Filter ON");
				DecorateButton(regimeFilterOffBtn, ButtonState.Disabled, "All Signals OFF"); 
			}
			else
			{
				DecorateButton(regimeFilterOnBtn, ButtonState.Disabled, "Regime Filter OFF");
				DecorateButton(regimeFilterOffBtn, ButtonState.Enabled, "All Signals ON");
			}
		}
		
		private void DecorateButton(Button button, ButtonState state, string contentOn, string contentOff = null, Brush foreground = null, Brush background = null)
		{
		    if (button == null) return;
		
		    switch (state)
		    {
		        case ButtonState.Enabled:
		            button.Content = contentOn; button.Background = background ?? Brushes.DarkGreen; button.Foreground = foreground ?? Brushes.White;
		            break;
		        case ButtonState.Disabled:
		            button.Content = contentOff ?? contentOn; button.Background = background ?? Brushes.DarkRed; button.Foreground = foreground ?? Brushes.White;
		            break;
		        case ButtonState.Neutral:
		            button.Content = contentOn; button.Background = background ?? Brushes.DarkGray; button.Foreground = foreground ?? Brushes.Black;
		            break;
		    }
		}
		#endregion
    }
}