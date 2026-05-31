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
	public class MoveSLChartPanel : Indicator
	{
		private System.Windows.Controls.Grid buttonPanel;
		private System.Windows.Controls.Button[] buttonsArray;
		private NinjaTrader.Gui.Tools.AccountSelector xAlselector;
		private NinjaTrader.Gui.Tools.InstrumentSelector xInSelector;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Move SL buttons on chart overlay";
				Name = "Move SL Chart Panel";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = false;
				PaintPriceMarkers = false;
				
				Button1Ticks = 2;
				Button2Ticks = 4;
				Button3Ticks = 6;
				Button4Percent = 50;
				
				ButtonBackground = Brushes.DarkSlateGray;
				ButtonTextColor = Brushes.White;
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
			// Check if panel already exists
			if (UserControlCollection.Contains(buttonPanel))
				return;

			// Create the main grid panel
			buttonPanel = new System.Windows.Controls.Grid
			{
				Name = "MoveSLPanel",
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(10, 10, 0, 0),
				Background = new SolidColorBrush(Color.FromArgb(200, 40, 40, 40))
			};

			// Define 2 rows and 2 columns for 4 buttons
			buttonPanel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			buttonPanel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			buttonPanel.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
			buttonPanel.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

			// Create 4 buttons
			buttonsArray = new System.Windows.Controls.Button[4];

			string[] buttonLabels = new string[]
			{
				$"SL +{Button1Ticks}",
				$"SL +{Button2Ticks}",
				$"SL +{Button3Ticks}",
				$"SL {Button4Percent}%"
			};

			for (int i = 0; i < 4; i++)
			{
				buttonsArray[i] = new System.Windows.Controls.Button
				{
					Content = buttonLabels[i],
					Width = 80,
					Height = 30,
					Margin = new Thickness(5),
					Background = ButtonBackground,
					Foreground = ButtonTextColor,
					BorderBrush = Brushes.Gray,
					BorderThickness = new Thickness(1),
					FontWeight = FontWeights.Bold,
					FontSize = 11
				};

				// Set button position in grid
				int row = i / 2;
				int col = i % 2;
				System.Windows.Controls.Grid.SetRow(buttonsArray[i], row);
				System.Windows.Controls.Grid.SetColumn(buttonsArray[i], col);

				// Add button to panel
				buttonPanel.Children.Add(buttonsArray[i]);
			}

			// Wire up click events
			buttonsArray[0].Click += (s, e) => MoveSL(Button1Ticks);
			buttonsArray[1].Click += (s, e) => MoveSL(Button2Ticks);
			buttonsArray[2].Click += (s, e) => MoveSL(Button3Ticks);
			buttonsArray[3].Click += (s, e) => MoveSLByPercent(Button4Percent);

			// Add panel to chart
			UserControlCollection.Add(buttonPanel);
		}

		private void RemoveButtonPanel()
		{
			if (buttonPanel != null)
			{
				if (UserControlCollection.Contains(buttonPanel))
					UserControlCollection.Remove(buttonPanel);

				// Unsubscribe from events
				if (buttonsArray != null)
				{
					for (int i = 0; i < buttonsArray.Length; i++)
					{
						if (buttonsArray[i] != null)
							buttonsArray[i].Click -= null;
					}
				}
			}
		}

		private void MoveSL(int Ticks)
		{
			xAlselector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
			string currentAccount = xAlselector.SelectedAccount.ToString();
			
			Account Acct = Account.All.FirstOrDefault(x => currentAccount.Contains(x.Name));
			
			xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
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
			string currentAccount = xAlselector.SelectedAccount.ToString();
			
			Account Acct = Account.All.FirstOrDefault(x => currentAccount.Contains(x.Name));
			
			xInSelector = Window.GetWindow(ChartControl.OwnerChart).FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
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

		protected override void OnBarUpdate() { }

		#region Properties
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Button 1 Ticks", Order = 1, GroupName = "Parameters")]
		public int Button1Ticks { get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Button 2 Ticks", Order = 2, GroupName = "Parameters")]
		public int Button2Ticks { get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name = "Button 3 Ticks", Order = 3, GroupName = "Parameters")]
		public int Button3Ticks { get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name = "Button 4 Percent", Order = 4, GroupName = "Parameters")]
		public int Button4Percent { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Button Background", Order = 5, GroupName = "Appearance")]
		public Brush ButtonBackground { get; set; }

		[Browsable(false)]
		public string ButtonBackgroundSerializable
		{
			get { return Serialize.BrushToString(ButtonBackground); }
			set { ButtonBackground = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name = "Button Text Color", Order = 6, GroupName = "Appearance")]
		public Brush ButtonTextColor { get; set; }

		[Browsable(false)]
		public string ButtonTextColorSerializable
		{
			get { return Serialize.BrushToString(ButtonTextColor); }
			set { ButtonTextColor = Serialize.StringToBrush(value); }
		}
		#endregion
	}
}


#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MoveSLChartPanel[] cacheMoveSLChartPanel;
		public MoveSLChartPanel MoveSLChartPanel(int button1Ticks, int button2Ticks, int button3Ticks, int button4Percent, Brush buttonBackground, Brush buttonTextColor)
		{
			return MoveSLChartPanel(Input, button1Ticks, button2Ticks, button3Ticks, button4Percent, buttonBackground, buttonTextColor);
		}

		public MoveSLChartPanel MoveSLChartPanel(ISeries<double> input, int button1Ticks, int button2Ticks, int button3Ticks, int button4Percent, Brush buttonBackground, Brush buttonTextColor)
		{
			if (cacheMoveSLChartPanel != null)
				for (int idx = 0; idx < cacheMoveSLChartPanel.Length; idx++)
					if (cacheMoveSLChartPanel[idx] != null && cacheMoveSLChartPanel[idx].Button1Ticks == button1Ticks && cacheMoveSLChartPanel[idx].Button2Ticks == button2Ticks && cacheMoveSLChartPanel[idx].Button3Ticks == button3Ticks && cacheMoveSLChartPanel[idx].Button4Percent == button4Percent && cacheMoveSLChartPanel[idx].ButtonBackground == buttonBackground && cacheMoveSLChartPanel[idx].ButtonTextColor == buttonTextColor && cacheMoveSLChartPanel[idx].EqualsInput(input))
						return cacheMoveSLChartPanel[idx];
			return CacheIndicator<MoveSLChartPanel>(new MoveSLChartPanel(){ Button1Ticks = button1Ticks, Button2Ticks = button2Ticks, Button3Ticks = button3Ticks, Button4Percent = button4Percent, ButtonBackground = buttonBackground, ButtonTextColor = buttonTextColor }, input, ref cacheMoveSLChartPanel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MoveSLChartPanel MoveSLChartPanel(int button1Ticks, int button2Ticks, int button3Ticks, int button4Percent, Brush buttonBackground, Brush buttonTextColor)
		{
			return indicator.MoveSLChartPanel(Input, button1Ticks, button2Ticks, button3Ticks, button4Percent, buttonBackground, buttonTextColor);
		}

		public Indicators.MoveSLChartPanel MoveSLChartPanel(ISeries<double> input , int button1Ticks, int button2Ticks, int button3Ticks, int button4Percent, Brush buttonBackground, Brush buttonTextColor)
		{
			return indicator.MoveSLChartPanel(input, button1Ticks, button2Ticks, button3Ticks, button4Percent, buttonBackground, buttonTextColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MoveSLChartPanel MoveSLChartPanel(int button1Ticks, int button2Ticks, int button3Ticks, int button4Percent, Brush buttonBackground, Brush buttonTextColor)
		{
			return indicator.MoveSLChartPanel(Input, button1Ticks, button2Ticks, button3Ticks, button4Percent, buttonBackground, buttonTextColor);
		}

		public Indicators.MoveSLChartPanel MoveSLChartPanel(ISeries<double> input , int button1Ticks, int button2Ticks, int button3Ticks, int button4Percent, Brush buttonBackground, Brush buttonTextColor)
		{
			return indicator.MoveSLChartPanel(input, button1Ticks, button2Ticks, button3Ticks, button4Percent, buttonBackground, buttonTextColor);
		}
	}
}

#endregion
