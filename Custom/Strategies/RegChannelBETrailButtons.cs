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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class RegChannelBeTrailButtons : Strategy
	{
		private RegressionChannel2 RegressionChannel21;
		private RegressionChannelExtended RegressionChannelExtended1;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		private ADX ADX1;
		
		private double BEStoredTargetPrice;
		private double BEStoredActualPrice;
		
		private double TrailStoredTargetPrice;
		private double TrailStoredActualPrice;
		
		private bool StopSetBool;
		private bool BreakEvenBool;
		private bool TrailStopBool;
		
		private double totalPnL;
		private double cumPnL;
		private double dailyPnL;
		
		#region Chart Trader Buttons
		
		private System.Windows.Controls.RowDefinition	addedRow, addedRow2, addedRow3;
		private Gui.Chart.ChartTab						chartTab;
		private Gui.Chart.Chart							chartWindow;
		private System.Windows.Controls.Grid			chartTraderGrid, chartTraderButtonsGrid, lowerButtonsGrid, lowerButtonsGrid2, lowerButtonsGrid3;
		private System.Windows.Controls.Button			activateButton1, activateButton2, activateButton3, activateButton4, activateButton5, activateButton6;
		private bool									panelActive;
		private System.Windows.Controls.TabItem			tabItem;
		
		//
		private bool button1active;
		private bool button2active;
		private bool button3active;
		private bool button4active;
		private bool button5active;
		private bool button6active;
		
		private bool QuickLong;
		private bool QuickShort;
		private bool StrategyEnabled;

		
		private bool syncPnl;
		private double historicalTimeTrades;//Sync  PnL
		private double dif;//To Calculate PNL sync
		private double cumProfit;//For real time pnl and pnl synchronization
		
		private bool restarPnL;
		
		#endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Printing Money using UniRenko";
				Name										= "RegChannelBeTrailButtons";
				Calculate									= Calculate.OnPriceChange;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				//Release notes
				Version							= "Version 1.0.0 // June 2024";
				
				ProfitTarget					= 60;
				Contracts						= 1;
				
				InitialStop						= 50;
				
				BETargetTicks					= 20;	// How many ticks until BE Set
				BEOffset						= 4;
				
				TrailTargetTicks				= 40;	// How many ticks until Trail Set
				TrailStopDistance				= 10;	// How far back your stop will trail
			
				//Set at false from default
				StopSetBool						= false;
				BreakEvenBool					= false;
				TrailStopBool					= false;
				
				ShowHistorical					= true;
				
				ADXPeriod						= 4;
				ADXThreshold					= 75;
				
				Start						= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End							= DateTime.Parse("04:00", System.Globalization.CultureInfo.InvariantCulture);
				
				//Daily Limits
				DailyProfitLimit							= 4500;
				DailyLossLimit								= 1500;
				
				QuickLong				= false;
				QuickShort				= false;
				StrategyEnabled			= true;
				
//				DisplayStrategyPnL		= true;  //For PNL plot
				
			}
			else if (State == State.Configure)
			{
//				SetTrailStop(@"", CalculationMode.Ticks, TrailTicks, false);
			}
			else if (State == State.DataLoaded)
			{				
				RegressionChannel21						= RegressionChannel2(Close, 40, 3.5);
				RegressionChannelExtended1				= RegressionChannelExtended(Close, 40, 3.5);
				RegressionChannelHighLow1				= RegressionChannelHighLow(Close, 40, 3.5);
				ADX1									= ADX(Close, Convert.ToInt32(ADXPeriod));
				
//				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
//				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);	
				
			}
			else if (State == State.Historical)
			{
				#region Chart Trader Buttons Load
				
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						CreateWPFControls();
						//boton  toolbar
//						AddButtonToToolbar();
					});
				}
				
				#endregion
			}
			
			else if (State == State.Terminated)
			{
				#region Chart Trader Termninate
				
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						DisposeWPFControls();
						//borra botones toolbar
//						DisposeCleanUp();
					});
				}
				
				#endregion
			}
		}
		
		/////////////////////////////////////
		//Funciones de botones desde abajo//
		////////////////////////////////////
		
		#region Button Click Events
		
			#region Button 1
		
		protected void Button1Click(object sender, RoutedEventArgs e)
		{
//			Draw.TextFixed(this, "infobox", "Button 1 Clicked", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			
			Print("Position Closing");
			
			if(Position.MarketPosition == MarketPosition.Long) {
				ExitLong("Manual Exit", @"Quick Long");
				ExitLong("Manual Exit", @"GoingUp");			
				
				
			}else if(Position.MarketPosition == MarketPosition.Short) {
				ExitShort("Manual Exit", @"GoingDown");
				ExitShort("Manual Exit", @"Quick Short");

			}		
			ForceRefresh();
			
		}
		
		#endregion
		
			#region Button 2
		
		protected void Button2Click(object sender, RoutedEventArgs e)
		{
			Print("State: " + QuickLong);
			
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			if(QuickLong == false)
			{
				QuickLong = true;
				Print("Quick Long On  ");
				button2active = true;
			}
						
			ForceRefresh();
		}		
		
		#endregion
		
			#region Button 3
		
		protected void Button3Click(object sender, RoutedEventArgs e)
		{
			Print("State: " + QuickShort);
			
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			if(QuickShort == false)
			{
				QuickShort = true;
				Print("Quick Short On  ");
				button3active = true;
			}
			
			ForceRefresh();
		}		
		
		#endregion
		
			#region Button 4
		
		protected void Button4Click(object sender, RoutedEventArgs e)
		{

//			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			
			ForceRefresh();
		}		
		
		#endregion
		
			#region Button 5
		
		protected void Button5Click(object sender, RoutedEventArgs e)
		{
//			Draw.TextFixed(this, "infobox", "Button 5 Clicked", TextPosition.BottomLeft, Brushes.Orange, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			
			
			ForceRefresh();
		}		
		
		#endregion
		
			#region Button 6
		
		protected void Button6Click(object sender, RoutedEventArgs e)
		{
//			Draw.TextFixed(this, "infobox", "Button 6 Clicked", TextPosition.BottomLeft, Brushes.Orange, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// refresh the chart so that the text box will appear on the next render pass even if there is no incoming data
			
			//🗘
			if(StrategyEnabled == true)
			{
				activateButton6.Background = new SolidColorBrush(Colors.White) {Opacity = 0.25};
				activateButton6.Content = "Off";
				button6active = false;
				StrategyEnabled = false;
				Print("Strategy: " + StrategyEnabled);
			}else
			{
				activateButton6.Background = Brushes.White;	
				activateButton6.Content = "ON";
				button6active = true;
				StrategyEnabled = true;
				Print("Strategy: " + StrategyEnabled);
			}
//			CloseStrategy("ReversalStrat_Noylan");
			ForceRefresh();
		}		
		
		#endregion
		

		#endregion
		
		protected void CreateWPFControls()
		{
			
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
			
			
		
			// Lower Grid - (Row2)Middle
			lowerButtonsGrid2 = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid2, 2);
			
			//Columns * 2
			lowerButtonsGrid2.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			lowerButtonsGrid2.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			
			
			
			// Lower Grid - (Row3)Lower
			lowerButtonsGrid3 = new System.Windows.Controls.Grid();
			System.Windows.Controls.Grid.SetColumnSpan(lowerButtonsGrid3, 3);
			
			//Columns * 3
			lowerButtonsGrid3.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			lowerButtonsGrid3.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			lowerButtonsGrid3.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			
			
			
			addedRow	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
			addedRow2	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
			addedRow3	= new System.Windows.Controls.RowDefinition() { Height = new GridLength(40) };
				

			// this style (provided by NinjaTrader_MichaelM) gives the correct default minwidth (and colors) to make buttons appear like chart trader buttons
			Style basicButtonStyle	= Application.Current.FindResource("BasicEntryButton") as Style;
	
			
			#endregion
			
			
				#region Button Content
				
				
					activateButton1 = new System.Windows.Controls.Button()//1
					{		
						
						Content			= "Flaten All",
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
				
				
			
					activateButton2 = new System.Windows.Controls.Button()//2
					{		
						
						Content			= "Quick Long",
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
			
				
					activateButton3 = new System.Windows.Controls.Button()//3
					{		
						
						Content			= "Quick Short",
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
					
					activateButton4 = new System.Windows.Controls.Button()//3
					{		
						
						Content			= "1",
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
					
					activateButton5 = new System.Windows.Controls.Button()//3
					{		
						
						Content			= "2",
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
					
					activateButton6 = new System.Windows.Controls.Button()//3
					{		
						
						Content			= "ON",
						Height			= 25, 
						Margin			= new Thickness(5,0,5,0),
						Padding			= new Thickness(0,0,0,0),
						Style			= basicButtonStyle
					};		
			
			
				#endregion
					
	
				#region Button Colors
					
					//Row1
					activateButton1.Background		= Brushes.Yellow;	//background
					activateButton1.BorderBrush		= Brushes.Black;	//borders
					activateButton1.Foreground    	= Brushes.Black;	//letter
					activateButton1.BorderThickness = new Thickness(2.0);

					//Row2
					activateButton2.Background		= Brushes.PaleGreen;
					activateButton2.BorderBrush		= Brushes.Black;	
					activateButton2.Foreground    	= Brushes.Black;	
					activateButton2.BorderThickness = new Thickness(2.0);

					activateButton3.Background		= Brushes.Pink;
					activateButton3.BorderBrush		= Brushes.Black;	
					activateButton3.Foreground    	= Brushes.Black;		
					activateButton3.BorderThickness = new Thickness(2.0);
					
					//Row3
					activateButton4.Background		= Brushes.White;
					activateButton4.BorderBrush		= Brushes.Black;	
					activateButton4.Foreground    	= Brushes.Black;		
					activateButton4.BorderThickness = new Thickness(2.0);
					
					activateButton5.Background		= Brushes.White;
					activateButton5.BorderBrush		= Brushes.Black;	
					activateButton5.Foreground    	= Brushes.Black;		
					activateButton5.BorderThickness = new Thickness(2.0);
					
					activateButton6.Background		= Brushes.White;
					activateButton6.BorderBrush		= Brushes.Black;	
					activateButton6.Foreground    	= Brushes.Black;		
					activateButton6.BorderThickness = new Thickness(2.0);
				
			
			#endregion	
					
		
				#region Button Click 
				
					activateButton1.Click += Button1Click;
					activateButton2.Click += Button2Click;
					activateButton3.Click += Button3Click;
					activateButton4.Click += Button4Click;
					activateButton5.Click += Button5Click;
					activateButton6.Click += Button6Click;
				
				#endregion	
					
					
				#region Button Location
		
					//activateButton1 (Row 1)
					System.Windows.Controls.Grid.SetColumn(activateButton1, 0);				
					System.Windows.Controls.Grid.SetRow(activateButton1, 0);	
				
					
					//New Grid - Start at Row 0. But we have 2 columns here (Row 2)
					System.Windows.Controls.Grid.SetColumn(activateButton2, 0);				
					System.Windows.Controls.Grid.SetRow(activateButton2, 0);
					
					System.Windows.Controls.Grid.SetColumn(activateButton3, 1);				
					System.Windows.Controls.Grid.SetRow(activateButton3, 0);	
					
					
					//New Grid - Start at Row 0. But we have 3 columns here (Row 3)
					System.Windows.Controls.Grid.SetColumn(activateButton4, 0);				
					System.Windows.Controls.Grid.SetRow(activateButton4, 0);
					
					System.Windows.Controls.Grid.SetColumn(activateButton5, 1);				
					System.Windows.Controls.Grid.SetRow(activateButton5, 0);
					
					System.Windows.Controls.Grid.SetColumn(activateButton6, 2);				
					System.Windows.Controls.Grid.SetRow(activateButton6, 0);
					
				
				#endregion	
					
							
				#region Add Buttons 1
			
					lowerButtonsGrid.Children.Add(activateButton1);
								
				#endregion
					
				#region Add Buttons 2-3
					
					lowerButtonsGrid2.Children.Add(activateButton2);
					lowerButtonsGrid2.Children.Add(activateButton3);
						
				#endregion	
					
				#region Add Buttons 4-6
					
					lowerButtonsGrid3.Children.Add(activateButton4);
					lowerButtonsGrid3.Children.Add(activateButton5);
					lowerButtonsGrid3.Children.Add(activateButton6);
						
				#endregion		
					
            if (totalGrids == 0) 
				totalGrids = chartTraderGrid.RowDefinitions.Count;


			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
			
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
			
			
			//Row 2
			if (activateButton2 != null)
				activateButton2.Click -= Button2Click;
			
			if (activateButton3 != null)
				activateButton3.Click -= Button3Click;

			
			//Row 3
			if (activateButton4 != null)
				activateButton4.Click -= Button4Click;
			
			if (activateButton5 != null)
				activateButton5.Click -= Button5Click;
			
			if (activateButton6 != null)
				activateButton6.Click -= Button6Click;
			
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
			
			
			chartTraderGrid.RowDefinitions.Add(addedRow2);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid2, totalGrids + 1); //Add 1 Grid
			chartTraderGrid.Children.Add(lowerButtonsGrid2);
			
			
			chartTraderGrid.RowDefinitions.Add(addedRow3);
			System.Windows.Controls.Grid.SetRow(lowerButtonsGrid3, totalGrids + 2); //Add 2 Grids 
			chartTraderGrid.Children.Add(lowerButtonsGrid3);
			
			
			panelActive = true;
			
			#endregion	
		}
		
		//////////////////////////////
		

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 5)
				return;
			
			if (!ShowHistorical)
			{
				if (State != State.Realtime)
					return;
				
			}
			// at the start of a new session, reset the currentPnL for a new day of trading
			if (Bars.IsFirstBarOfSession){
				cumPnL 			= totalPnL; ///Double that copies the full session PnL (If trading multiple days). Is only calculated once per day.
				dailyPnL		= totalPnL - cumPnL; ///Subtract the copy of the full session by the full session PnL. This resets your daily PnL back to 0.
			}
			
			#region Long Trade
			
			 // Set 1 - Set Order
			if (
				 // RegChanLongGroup1
				(((RegressionChannel21.Middle[1] > RegressionChannel21.Middle[2])
				 && (RegressionChannel21.Middle[2] <= RegressionChannel21.Middle[3])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanLongGroup2
				 || ((RegressionChannel21.Middle[0] > RegressionChannel21.Middle[1])
				 && (Low[0] > Low[2])
				 && (Low[2] <= RegressionChannel21.Lower[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanLongGroup3
				 || ((Low[0] > RegressionChannelHighLow1.Lower[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold)))
				 // RegChanLongGroup4
				 
				
				 // Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				// Enter Time
				 && ((Times[0][0].TimeOfDay >= Start.TimeOfDay)
				 && (Times[0][0].TimeOfDay < End.TimeOfDay))
			 	 && (Position.MarketPosition == MarketPosition.Flat)
					)
				
			{
				// if flat and below the loss limit of the day enter long
				if (
					(dailyPnL > -DailyLossLimit) //Loss remains 'above' limit 
					&& (dailyPnL < DailyProfitLimit) //Profit remains 'below' limit
					&& (StrategyEnabled == true)
					)
				{
					EnterLongLimit(Convert.ToInt32(Contracts), GetCurrentBid(0), @"GoingUp");
					BreakEvenBool 	= false;
					TrailStopBool	= false;
					StopSetBool		= false;
				}
			}
			
			
			// Set 2 - Set Stop and BE/Trail Targets
			if ((Position.MarketPosition == MarketPosition.Long)
				 && !StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (InitialStop * TickSize)) , @"MoneyDone", @"GoingUp");
				ExitLongLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (ProfitTarget * TickSize)) , @"MoneyWon", @"GoingUp");
				
				if(button2active){
					ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (InitialStop * TickSize)) , @"Stop Quick Long", @"Quick Long");
					ExitLongLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (ProfitTarget * TickSize)) , @"Target Quick Long", @"Quick Long");
				}
				
				BEStoredTargetPrice = (Position.AveragePrice + (BETargetTicks * TickSize)); //Store how far price needs to move before BE is set
				BEStoredActualPrice = (Position.AveragePrice + (BEOffset * TickSize)); //Store the actual BE Value for later use
				
				
				TrailStoredTargetPrice = (Position.AveragePrice + (TrailTargetTicks * TickSize)); // Store How far price needs to move before Trail Stop is set
				TrailStoredActualPrice = Close[0]; // Store a value for Trail -> Needs a check first but will be adjusted later on when its set
				
				
				StopSetBool = true;		
			}
			
			 // Set 3 - Set Breakeven
			if ((Position.MarketPosition == MarketPosition.Long)
				 && (Close[0] >= BEStoredTargetPrice)
					&& GetCurrentAsk(0) > BEStoredActualPrice && GetCurrentBid(0) > BEStoredActualPrice
					&& StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				
				if(button2active){
					ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"Stop Quick Long", @"Quick Long");
				}else{	
					ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"MoneyDone", @"GoingUp");
				}
				
				BreakEvenBool = true;
			}
			
			 // Set 4 - Set Trail Stop
			if ((Position.MarketPosition == MarketPosition.Long)
				 && Close[0] >= TrailStoredTargetPrice
					&& StopSetBool && BreakEvenBool && !TrailStopBool
					&& Close[0] - (TrailStopDistance * TickSize) > BEStoredActualPrice
					&& Close[0] - (TrailStopDistance * TickSize) > TrailStoredActualPrice)
			{
				TrailStoredActualPrice = Close[0] - (TrailStopDistance * TickSize); //Update Trail Price before submitting order
				
				if (GetCurrentAsk(0) > TrailStoredActualPrice && GetCurrentBid(0) > TrailStoredActualPrice)
					ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"MoneyDone", @"GoingUp");
					
					if(button2active){
							ExitLongStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"Stop Quick Long", @"Quick Long");
						}
					
					
			}
			
			#endregion
			
			#region Short Trade
			//======Short Sets======\\
			
			 // Set 6 - Set Order
			if (
				 // RegChanShortGroup1
				(((RegressionChannelExtended1.Middle[1] < RegressionChannelExtended1.Middle[2])
				 && (RegressionChannelExtended1.Middle[2] >= RegressionChannelExtended1.Middle[3])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanShortGroup2
				 || ((RegressionChannelExtended1.Middle[0] < RegressionChannelExtended1.Middle[1])
				 && (High[0] < High[2])
				 && (High[2] >= RegressionChannelExtended1.Lower[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				 // RegChanShortGroup3
				 || (High[0] < RegressionChannelHighLow1.Upper[2])
				 && (ADX1[0] > ADX1[2])
				 && (ADX1[0] > ADXThreshold))
				// Condition group 1
				 && ((BarsSinceExitExecution(0, "", 0) > 1)
				 || (BarsSinceExitExecution(0, "", 0) == -1))
				// Enter Time
				 && ((Times[0][0].TimeOfDay >= Start.TimeOfDay)
				 && (Times[0][0].TimeOfDay < End.TimeOfDay))
				
				&& (Position.MarketPosition == MarketPosition.Flat)
				)
			{
				
				// if flat and below the loss limit of the day enter short
				if (
					(dailyPnL > -DailyLossLimit) //Loss remains 'above' limit 
					&& (dailyPnL < DailyProfitLimit) //Profit remains 'below' limit
					&& (StrategyEnabled == true)
					)
				{
					EnterShortLimit(Convert.ToInt32(Contracts), GetCurrentAsk(0), @"GoingDown");
					StopSetBool		= false;
					BreakEvenBool 	= false;
					TrailStopBool	= false;
				}
			}
				
			
			// Set 2 - Set Stop and BE/Trail Targets
			if ((Position.MarketPosition == MarketPosition.Short)
				 && !StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (InitialStop * TickSize)) , @"MoneyDone", @"GoingDown");
				ExitShortLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (ProfitTarget * TickSize)) , @"MoneyWon", @"GoingDown");
				
				if(button3active){
					ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice + (InitialStop * TickSize)) , @"Stop Quick Short", @"Quick Short");
					ExitShortLimit(0, true, Convert.ToInt32(Contracts), (Position.AveragePrice - (ProfitTarget * TickSize)) , @"Target Quick Short", @"Quick Short");
				}
				
				BEStoredTargetPrice = (Position.AveragePrice - (BETargetTicks * TickSize)); //Store how far price needs to move before BE is set
				BEStoredActualPrice = (Position.AveragePrice - (BEOffset * TickSize)); //Store the actual BE Value for later use
				
				
				TrailStoredTargetPrice = (Position.AveragePrice - (TrailTargetTicks * TickSize)); // Store How far price needs to move before Trail Stop is set
				TrailStoredActualPrice = Close[0]; // Store a value for Trail -> Needs a check first but will be adjusted later on when its set
				
				StopSetBool = true;	
			}
			
			// Set 3 - Set Breakeven
			if ((Position.MarketPosition == MarketPosition.Short)
				 && (Close[0] <= BEStoredTargetPrice)
					&& GetCurrentAsk(0) < BEStoredActualPrice && GetCurrentBid(0) < BEStoredActualPrice
					&& StopSetBool && !BreakEvenBool && !TrailStopBool)
			{
				ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"MoneyDone", @"GoingDown");
				if(button3active){
					ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (BEStoredActualPrice) , @"Stop Quick Short", @"Quick Short");
				}
				
				BreakEvenBool = true;
			}
			
			// Set 4 - Set Trail Stop
			if ((Position.MarketPosition == MarketPosition.Short)
				 && Close[0] <= TrailStoredTargetPrice
					&& StopSetBool && BreakEvenBool && !TrailStopBool
					&& Close[0] + (TrailStopDistance * TickSize) < BEStoredActualPrice
					&& Close[0] + (TrailStopDistance * TickSize) < TrailStoredActualPrice)
			{
				TrailStoredActualPrice = Close[0] + (TrailStopDistance * TickSize); //Update Trail Price before submitting order
				
				if (GetCurrentAsk(0) < TrailStoredActualPrice && GetCurrentBid(0) < TrailStoredActualPrice)
					ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"MoneyDone", @"GoingDown");
				
					if(button3active){
						ExitShortStopMarket(0, true, Convert.ToInt32(Contracts), (TrailStoredActualPrice) , @"Stop Quick Short", @"Quick Short");
					}
			}
			
			#endregion
			
			#region Quick Long
			if(QuickLong)
			{
				EnterLong(Convert.ToInt32(Contracts), @"Quick Long");
		
				BreakEvenBool 	= false;
				TrailStopBool	= false;
				StopSetBool		= false;
				
				QuickLong		= false;
				
			}
			
			#endregion
			
			#region Quick Short
			if(QuickShort)
			{
				EnterShort(Convert.ToInt32(Contracts), @"Quick Short");
		
				StopSetBool		= false;
				BreakEvenBool 	= false;
				TrailStopBool	= false;
				
				QuickShort		= false;
				
			}
			
			#endregion
			
			// Reset when Flat
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				StopSetBool		= false;
				BreakEvenBool 	= false;
				TrailStopBool	= false;
				
//				Print("Quick Long/Short Off flatten " + QuickLong);
				button2active = false;
				button3active = false;
			}
			
		}
		
		protected override void OnPositionUpdate(Cbi.Position position, double averagePrice, 
			int quantity, Cbi.MarketPosition marketPosition)
		{
			#region Daily PNL
			
			if (Position.MarketPosition == MarketPosition.Flat && SystemPerformance.AllTrades.Count > 0)
			{
				
//				totalPnL = 0; //backtest
			
				totalPnL = SystemPerformance.RealTimeTrades.TradesPerformance.Currency.CumProfit; ///Double that sets the total PnL 

				dailyPnL = (totalPnL) - (cumPnL); ///Your daily limit is the difference between these
				
				
				if (dailyPnL <= -DailyLossLimit) //Print this when daily Pnl is under Loss Limit
				{
					
					Print("Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + dailyPnL + " <<" +  Time[0]);
					
					Text myTextLoss = Draw.TextFixed(this, "loss_text", "Daily Loss of " + DailyLossLimit +  " has been hit. No More Entries! Daily PnL >> " + "$" + totalPnL + " <<", TextPosition.BottomLeft, Brushes.Black, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextLoss.Font = new SimpleFont("Arial", 15) {Bold = true };

				}
				
				
				if (dailyPnL >= DailyProfitLimit) //Print this when daily Pnl is above Profit limit
				{
					
					Print("Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" +  dailyPnL + " <<" + Time[0]);
					
					Text myTextProfit = Draw.TextFixed(this, "profit_text", "Daily Profit of " + DailyProfitLimit +  " has been hit. No more Entries! Daily PnL >>" + "$" +  totalPnL + " <<", TextPosition.BottomLeft, Brushes.Black, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 100);
					myTextProfit.Font = new SimpleFont("Arial", 15) {Bold = true };
	
				}
			}	
			
			#endregion
		}
		
		//Botones
		protected void RemoveWPFControls()
		{
			#region Remove WPF
			
			if (!panelActive)
				return;

			if (chartTraderButtonsGrid != null || (lowerButtonsGrid != null && lowerButtonsGrid2 != null && lowerButtonsGrid3 != null))
			{
				chartTraderGrid.Children.Remove(lowerButtonsGrid);
				chartTraderGrid.Children.Remove(lowerButtonsGrid2);
				chartTraderGrid.Children.Remove(lowerButtonsGrid3);
				
				chartTraderGrid.RowDefinitions.Remove(addedRow);
				chartTraderGrid.RowDefinitions.Remove(addedRow2);
				chartTraderGrid.RowDefinitions.Remove(addedRow3);
			}
			
			panelActive = false;
			
			#endregion
		}
		
		
		private bool TabSelected()
		{
			#region TabSelected 
			
			if (ChartControl == null || chartWindow == null || chartWindow.MainTabControl == null)
				return false;
			
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as Gui.Chart.ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
				
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

			if (TabSelected()){
				InsertWPFControls();	
				//boton  toolbar
//				AddButtonToToolbar();
			}
			else{
				RemoveWPFControls();
				//borra botones toolbar
//				DisposeCleanUp();
			}
			
			#endregion
		}
	
		private double getCumProfit() {
			TradeCollection realTimeTrades = SystemPerformance.RealTimeTrades;
			return realTimeTrades.TradesPerformance.Currency.CumProfit;
		}
		
		//Draw pnl
		#region DrawStrategyPnl
		private void DrawStrategyPnl(ChartControl chartControl) {
	
			if (!restarPnL) {
			    // Modo normal
			    if (syncPnl) {
					dif = historicalTimeTrades - getCumProfit();
			    } else {
			        cumProfit = getCumProfit() + dif;
			    }
			}else {
			    // Modo restarPnL
				dif = historicalTimeTrades - getCumProfit();
				if(getCumProfit() == 0){//Reset arranca negativo por lo cual lo iniciamos en cero
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
			
			Draw.TextFixed(this, "realTimeTradeText", realTimeTradeText, TextPosition.BottomRight, Brushes.Black, font, Brushes.Transparent, Brushes.Transparent, 0);
								
		}
		#endregion
		
		#region onRender
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			DrawStrategyPnl(chartControl);


		}
		#endregion
		//Fin draw PN

		#region Properties
		
		#region 0. Release notes


		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name="Version", Order = 0, GroupName="0. Release notes")]
		public string Version
		{ get; set; }
			
		#endregion
		
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
		
		///ProfitLimit and LossLimit
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Profit Limit", Description="No positive or negative sign, just integer", Order=4, GroupName="01. Order Management")]
		public double DailyProfitLimit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Daily Loss Limit", Description="No positive or negative sign, just integer", Order=5, GroupName="01. Order Management")]
		public double DailyLossLimit
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Breakeven Target", Order=2, GroupName="02. BreakEven")]
		public int BETargetTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Breakeven Tick Offset", Order=3, GroupName="02. BreakEven")]
		public int BEOffset
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TrailStop Target", Order=2, GroupName="03. Trail Stop")]
		public int TrailTargetTicks
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trail Stop Distance", Order=3, GroupName="03. Trail Stop")]
		public int TrailStopDistance
		{ get; set; }
			
		[NinjaScriptProperty]
		[Display(Name="Show Historical Trades", Order=3, GroupName="04. Additional Settings")]
		public bool ShowHistorical
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ADXPeriod", Order=1, GroupName="05. ADX Settings")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ADXThreshold", Order=2, GroupName="05. ADX Settings")]
		public int ADXThreshold
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Trades", Order=1, GroupName="06. Time Frame")]
		public DateTime Start
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Trades", Order=2, GroupName="06. Time Frame")]
		public DateTime End
		{ get; set; }

		#endregion

	}
}

