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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class LONGSHORTToolbarButtonsB9 : Strategy
	{
		private bool longButtonClicked;
		private bool shortButtonClicked;
		private System.Windows.Controls.Button longButton;
		private System.Windows.Controls.Button shortButton;
		private System.Windows.Controls.Grid myGrid;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"LONGSHORTToolbarButtonsB9";
				Name								= "LONGSHORTToolbarButtonsB9";
				Calculate							= Calculate.OnEachTick;
				EntriesPerDirection					= 1;
				EntryHandling						= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy		= true;
				ExitOnSessionCloseSeconds			= 30;
				IsFillLimitOnTouch					= false;
				MaximumBarsLookBack					= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution					= OrderFillResolution.Standard;
				Slippage							= 0;
				StartBehavior						= StartBehavior.WaitUntilFlat;
				TimeInForce							= TimeInForce.Gtc;
				TraceOrders							= false;
				RealtimeErrorHandling				= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling					= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade					= 20;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.Historical)
			{
				if (UserControlCollection.Contains(myGrid))
					return;
				
				Dispatcher.InvokeAsync((() =>
				{
					myGrid = new System.Windows.Controls.Grid
					{
						Name = "MyCustomGrid", HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top
					};
					
					System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
					
					myGrid.ColumnDefinitions.Add(column1);
					myGrid.ColumnDefinitions.Add(column2);
					
					longButton = new System.Windows.Controls.Button
					{
						Name = "LongButton", Content = "LONG", Foreground = Brushes.White, Background = Brushes.Green
					};
					
					shortButton = new System.Windows.Controls.Button
					{
						Name = "ShortButton", Content = "SHORT", Foreground = Brushes.Black, Background = Brushes.Red
					};
					
					longButton.Click += OnButtonClick;
					shortButton.Click += OnButtonClick;
					
					System.Windows.Controls.Grid.SetColumn(longButton, 0);
					System.Windows.Controls.Grid.SetColumn(shortButton, 1);
					
					myGrid.Children.Add(longButton);
					myGrid.Children.Add(shortButton);
					
					UserControlCollection.Add(myGrid);
				}));
			}
			else if (State == State.Terminated)
			{
				Dispatcher.InvokeAsync((() =>
				{
					if (myGrid != null)
					{
						if (longButton != null)
						{
							myGrid.Children.Remove(longButton);
							longButton.Click -= OnButtonClick;
							longButton = null;
						}
						if (shortButton != null)
						{
							myGrid.Children.Remove(shortButton);
							shortButton.Click -= OnButtonClick;
							shortButton = null;
						}
					}
				}));
			}
		}

		protected override void OnBarUpdate()
		{
			if (longButtonClicked
				&& Close[1] >= MAX(High, 2)[2] + 0 * TickSize
				&& High[1] < CurrentDayOHL().CurrentHigh[1]
				&& High[0] > High[1])
				EnterLong();
			
			if (shortButtonClicked
				&& Low[1] > CurrentDayOHL().CurrentLow[1]
				&& Low[0] < Low[1]
				&& Close[1] < MIN(Low, 2)[2] - 0 * TickSize)
				EnterShort();
						
			if (!longButtonClicked
				&& Position.MarketPosition == MarketPosition.Long)
				ExitLong();
				
			if (!shortButtonClicked
				&& Position.MarketPosition == MarketPosition.Short)
				ExitShort();
		}
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button == longButton && button.Name == "LongButton" && button.Content == "LONG")
			{
				button.Content = "Exit L";
				button.Name = "ExitLongButton";
				longButtonClicked = true;
				return;
			}
			
			if (button == shortButton && button.Name == "ShortButton" && button.Content == "SHORT")
			{
				button.Content = "Exit S";
				button.Name = "ExitShortButton";
				shortButtonClicked = true;
				return;
			}
			
			if (button == longButton && button.Name == "ExitLongButton" && button.Content == "Exit L")
			{
				button.Content = "LONG";
				button.Name = "LongButton";
				longButtonClicked = false;
				return;
			}
			
			if (button == shortButton && button.Name == "ExitShortButton" && button.Content == "Exit S")
			{
				button.Content = "SHORT";
				button.Name = "ShortButton";
				shortButtonClicked = false;
				return;
			}
		}
	}
}