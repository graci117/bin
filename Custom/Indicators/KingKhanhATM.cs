using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Core;
using NinjaTrader.Custom;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript.AtmStrategy;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.NinjaScript.Indicators
{
	public class KingKhanhATM : Indicator
	{
		NinjaTrader.Gui.Tools.SimpleFont title = 
		new NinjaTrader.Gui.Tools.SimpleFont("Agency Fb", 16) { Size = 20, Bold = true };
		NinjaTrader.Gui.Tools.SimpleFont title2 = 
		new NinjaTrader.Gui.Tools.SimpleFont("Agency Fb", 16) { Size = 15, Bold = true };
		NinjaTrader.Gui.Tools.SimpleFont TextFont = 
		new NinjaTrader.Gui.Tools.SimpleFont("Agency Fb", 16) { Size = 15, Bold = false };
		
		double realizedPnL = 0;
		double TriggerBar1 = 0;
		double BarsSince1 = 0;
		double TriggerBar2 = 0;
		double BarsSince2 = 0;
		bool sigLock1 = false;
		bool sigLock2 = false;
		bool playOnce = false;
		
		private SimpleFont textFont = new SimpleFont("Arial", 10)
		{
			Bold = true
		};

		#region Box Vars
		private Border moveB;
		private Border hideB;
		private Border tileHolder;
		private Grid grid;
		private Thickness margin;
		private bool subscribedToSize;
		private System.Windows.Point startPoint;
		private Button ARMLONGbtn;
		private Button ARMSHORTbtn;
		private Button ARMOPTIONbtn;
		private Button Closebtn;
		private Button AUTOArmbtn;
		private Button resetQuantVal;
		private Button increaseQuant;
		private Button decreaseQuant;
		private Label ATMLabel;
		private Label ACCOUNTLabel;
		private Label selectedATMStrat;
		private Label accountSelected;
		private Label quantityLabel;
		private TextBox quantityLabelVal;
		private ComboBox combo;
		private bool armLongBtnToggle;
		private bool armShortBtnToggle;
		private bool armOptionBtnToggle;
		private bool autoArmBtnToggle;
		private int quantityValue = 1;
		private int quantityATM = 1;
		private bool closeRunning;
		private bool handlerSet;
		private Chart chartWindow;
		private Grid chartTraderGrid;
		private Grid chartTraderButtonsGrid;
		private Grid lowerButtonsGrid;
		private Grid upperButtonsGrid;
		private AtmStrategySelector atmSelector;
		private AccountSelector accountSelector;
		private bool connected;
		private Instrument inst;
		private bool onceTriggered;
		private bool inPosition;
		private int posDirection;
		private Account chartTraderAccount;
		private Position positions;
		private Timer _posTimer;
		private Timer _acctTimer;
		private FrameworkElement addedGrid;
		#endregion
		
		#region Indicators
		private RegChannel RegChannel1;
		private LinReg LinReg1;
		private RegChannel RegChannel2;
		private LinReg LinReg2;
		private RegressionChannelHighLow RegressionChannelHighLow1;
		#endregion



		protected override void OnStateChange()
		{
			if (State == (State)1)
			{
				Description = "Custom Visualization for Strategy Execution Operations via ALGO.";
				Name = "KingKhanhATM";
				Calculate = Calculate.OnEachTick;
				BarsRequiredToPlot = 100;
				IsOverlay = true;
				DisplayInDataBox = false;
				ShowTransparentPlotsInDataBox = true;
				DrawOnPricePanel = true;
				PaintPriceMarkers = false;
				IsSuspendedWhileInactive = false;
				this.SelectedTypes = new XElement("SelectedTypes");
				ArePlotsConfigurable = false;
				AreLinesConfigurable = false;
				this.Left = 5.0;
				this.Top = -1.0;
				this.NumberOfRows = 5;
				realizedProfit = 0;
				realizedLoss = 0;
				startTime = 073000;
				endTime = 130000;
				econNumber1 = 074500;
				econNumber2 = 080000;
				openTime = 0;

				#region Scalper4	
				Contracts					= 1;
				TrailStop					= 45;
				ProfitTarget					= 36;
				LinRegPeriod					= 9;
				RegChanPeriod					= 30;
				Upper					= 0.9;
				Lower					= 0.9;
				#endregion

				return;
			}
			if (State == (State)2)
			{
				RegChannel1					= RegChannel(Close, Convert.ToInt32(RegChanPeriod), Lower);
				LinReg1						= LinReg(Close, Convert.ToInt32(LinRegPeriod));
				RegChannel2					= RegChannel(Close, Convert.ToInt32(RegChanPeriod), Upper);
				LinReg2						= LinReg(Close, Convert.ToInt32(LinRegPeriod));
				RegressionChannelHighLow1	= RegressionChannelHighLow(Close, 30, 3.5);
				RegChannel1.Plots[0].Brush = Brushes.DarkGray;
				RegChannel1.Plots[1].Brush = Brushes.Aqua;
				RegChannel1.Plots[2].Brush = Brushes.Aqua;
				LinReg1.Plots[0].Brush = Brushes.Yellow;


			}
			if (State != (State)2)
			{
				if (State == (State)4)
				{
					Draw.TextFixed(this, "hftName", "KingKhanhATM", TextPosition.TopRight, Brushes.Magenta, title, Brushes.Black,Brushes.Black,50);
					
					#region Control Panel
					
					try
					{
						Dispatcher.InvokeAsync(delegate()
						{
							QuantityUpDown quantityUpDown = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlQuantitySelector") as QuantityUpDown;
							this.quantityATM = quantityUpDown.Value;
						});
					}
					catch (Exception)
					{
					}
					try
					{
						if (ChartControl != null)
						{
							if (this.Top < 0.0)
							{
								this.Top = 25.0;
							}
							Dispatcher.Invoke(delegate()
							{
								this.addedGrid = this.CreateControl();
								UserControlCollection.Add(this.addedGrid);
							});
						}
						return;
					}
					catch (Exception)
					{
						return;
					}
					#endregion
				}
				if (State == (State)8)
				{
					#region Control Panel
					this.grid = null;
					this.tileHolder = null;
					this.moveB = null;
					this.hideB = null;
					Dispatcher.Invoke(delegate()
					{
						UserControlCollection.Remove(this.addedGrid);
						this.addedGrid = null;
					});
					try
					{
						if (this._posTimer != null)
						{
							this._posTimer.Change(-1, 0);
							this._posTimer.Dispose();
						}
						if (this._acctTimer != null)
						{
							this._acctTimer.Change(-1, 0);
							this._acctTimer.Dispose();
						}
						if (this.addedGrid != null)
						{
							UserControlCollection.Remove(this.addedGrid);
							this.addedGrid = null;
						}
						if (this.moveB != null)
						{
							this.moveB = null;
						}
						if (this.hideB != null)
						{
							this.hideB = null;
						}
						if (this.grid != null)
						{
							this.grid = null;
						}
						if (this.tileHolder != null)
						{
							this.tileHolder = null;
						}
						if (this.ARMLONGbtn != null)
						{
							this.ARMLONGbtn.Click -= this.ARMLONGbtn_Click;
							this.ARMLONGbtn = null;
						}
						if (this.ARMSHORTbtn != null)
						{
							this.ARMSHORTbtn.Click -= this.ARMSHORTbtn_Click;
							this.ARMSHORTbtn = null;
						}
						if (this.ARMOPTIONbtn != null)
						{
							this.ARMOPTIONbtn.Click -= this.ARMOPTIONbtn_Click;
							this.ARMOPTIONbtn = null;
						}
						if (this.Closebtn != null)
						{
							this.Closebtn.Click -= this.Closebtn_Click;
							this.Closebtn = null;
						}
						if (this.AUTOArmbtn != null)
						{
							this.AUTOArmbtn.Click -= this.AUTOArmbtn_Click;
							this.AUTOArmbtn = null;
						}
						if (this.resetQuantVal != null)
						{
							this.resetQuantVal.Click -= new RoutedEventHandler(this.resetQuantVal_Click);
							this.resetQuantVal = null;
						}
						if (this.increaseQuant != null)
						{
							this.increaseQuant.Click -= new RoutedEventHandler(this.increaseQuant_Click);
							this.increaseQuant = null;
						}
						if (this.decreaseQuant != null)
						{
							this.decreaseQuant.Click -= new RoutedEventHandler(this.decreaseQuant_Click);
							this.decreaseQuant = null;
						}
						if (this.atmSelector != null)
						{
							this.atmSelector.SelectionChanged -= new SelectionChangedEventHandler(this.ATMSelection_Changed);
							this.atmSelector = null;
						}
						if (this.quantityLabelVal != null)
						{
							this.quantityLabelVal.PreviewKeyDown -= this.TextBox_PreviewKeyDown;
							this.quantityLabelVal = null;
						}
						if (this.accountSelector != null)
						{
							this.accountSelector.SelectionChanged -= new SelectionChangedEventHandler(this.ACCOUNTSelection_Changed);
							this.accountSelector = null;
						}
					}
					catch (Exception)
					{
					}
					#endregion
				}
				return;
			}
		}

		protected override void OnBarUpdate()
		{
			try
			{
				if (!this.subscribedToSize && ChartPanel != null)
				{
					this.subscribedToSize = true;
					ChartPanel.SizeChanged += delegate(object sender, SizeChangedEventArgs e)
					{
						if (this.grid != null && ChartPanel != null)
						{
							if (this.grid.Margin.Left + this.grid.ActualWidth > ChartPanel.ActualWidth || this.grid.Margin.Top + this.grid.ActualHeight > ChartPanel.ActualHeight)
							{
								double left = Math.Max(0.0, Math.Min(this.grid.Margin.Left, ChartPanel.ActualWidth - this.grid.ActualWidth));
								double top = Math.Max(0.0, Math.Min(this.grid.Margin.Top, ChartPanel.ActualHeight - this.grid.ActualHeight));
								this.grid.Margin = new Thickness(left, top, 0.0, 0.0);
								this.Left = left;
								this.Top = top;
							}
							return;
						}
					};
				}
				try
				{
					Draw.TextFixed(this, "pnlstat", "\n\n"+"ACC PNL $"+realizedPnL.ToString("F2"), TextPosition.TopRight);
					if(realizedProfit != 0 && realizedPnL >= realizedProfit)
					{
						Draw.TextFixed(this, "PT STAT", "Stopped - Profit Target Reached"+ "   Realized P&L: "+ realizedPnL.ToString("F2"), TextPosition.Center, Brushes.LightGreen, title, Brushes.Black,Brushes.Black,50);
						return;
					}
					if(realizedLoss !=0 && realizedPnL <= realizedLoss)
					{
						Draw.TextFixed(this, "SL STAT", "Stopped - Total Stop Loss Reached"+ "   Realized P&L: "+ realizedPnL.ToString("F2"), TextPosition.Center, Brushes.Red, title, Brushes.Black,Brushes.Black,50);
						return;
					}
					
					/// Avoid Econ
					if(econNumber1 != 0 || econNumber2 != 0)
					{
						DateTime ecNumber = Time[0];
						ecNumber.AddMinutes(5);
						//Print((ToTime(ecNumber.AddMinutes(5))-ToTime(Time[0])).ToString());
						
						if((ToTime(Time[0]) >= econNumber1 - (ToTime(ecNumber.AddMinutes(5))-ToTime(Time[0])) && ToTime(Time[0]) < econNumber1 + (ToTime(ecNumber.AddMinutes(5))-ToTime(Time[0])))) // Trade Time
						{
							Draw.TextFixed(this, "Econ", "\n\n\n\n"+"No Trades Econ Data Release +- 5min", TextPosition.TopRight, Brushes.Red, title2, Brushes.Transparent,Brushes.Black,0);
							return;
						}
						else if((ToTime(Time[0]) >= econNumber2 - (ToTime(ecNumber.AddMinutes(5))-ToTime(Time[0])) && ToTime(Time[0]) < econNumber2 + (ToTime(ecNumber.AddMinutes(5))-ToTime(Time[0])))) // Trade Time
						{
							Draw.TextFixed(this, "Econ", "\n\n\n\n"+"No Trades Econ Data Release +- 5min", TextPosition.TopRight, Brushes.Red, title2, Brushes.Transparent,Brushes.Black,0);
							return;
						}
						else if((ToTime(Time[0]) >= openTime - (ToTime(ecNumber.AddMinutes(5))-ToTime(Time[0])) && ToTime(Time[0]) < openTime + (ToTime(ecNumber.AddMinutes(5))-ToTime(Time[0])))) // Trade Time
						{
							Draw.TextFixed(this, "Econ", "\n\n\n\n"+"No Trades During Market Open +- 5min", TextPosition.TopRight, Brushes.Red, title2, Brushes.Transparent,Brushes.Black,0);
							return;
						}
						else
						{
							Draw.TextFixed(this, "Econ", "\n\n\n\n"+"Skipping Econ @ "+econNumber1.ToString()+" and "+econNumber2.ToString(), TextPosition.TopRight, Brushes.LightGreen, title2, Brushes.Transparent,Brushes.Black,0);
						}
					}
					///Trade Time
					if((ToTime(Time[0]) >= startTime && ToTime(Time[0]) < endTime))
					{
						Draw.TextFixed(this, "timeStat", "\n\n\n"+"Trade Time: Trading " +startTime.ToString() + " to " +endTime.ToString(), TextPosition.TopRight);
					}
					else if(startTime == 0 && endTime == 0)
					{
						Draw.TextFixed(this, "timeStat", "\n\n\n"+"Trade Time: Trading All Day", TextPosition.TopRight);
					}
					else
					{
						Draw.TextFixed(this, "timeStat", "\n\n\n"+"Trade Time: Not Trading", TextPosition.TopRight);
						return;
					}
					
				}
				catch
				{
				}
				try
				{	
					if (CurrentBars[0] < 20)
					{
						return;
					}
					if (BarsInProgress != 0) 
						return;

					if(State == State.Realtime && this.getInstrumentPosition() == null)
					{
						Draw.TextFixed(this, "pos", "\n\n\n\n"+"No Position", TextPosition.TopRight);
					}
					if(State == State.Realtime && this.getInstrumentPosition() != null)
					{
						Draw.TextFixed(this, "pos", "\n\n\n\n"+"In Position", TextPosition.TopRight);
					}
					if(CurrentBar > TriggerBar1)
					{
						BarsSince1 = CurrentBar - TriggerBar1;
						if(BarsSince1 >= 1 && IsFirstTickOfBar)
						{
							TriggerBar1 = 0;
							sigLock1 = false;
							playOnce = false;
						}
					}
					if(CurrentBar > TriggerBar2 )
					{
						BarsSince2 = CurrentBar - TriggerBar2;
						if(BarsSince2 >= 1 && IsFirstTickOfBar)
						{
							TriggerBar2 = 0;
							sigLock2 = false;
							playOnce = false;
						}
					}
					
					if (
						// Long entry group 1
						((RegChannel1.Middle[0] > RegChannel1.Middle[1])
						&& (RegChannel1.Middle[1] <= RegChannel1.Middle[2])
						&& (LinReg1[0] > LinReg1[1])
						&& (Close[0] > Open[0]))
				
						// Long entry group 2
						|| ((Low[0] > Low[1])
						&& (Low[1] <= RegChannel1.Lower[1])
						&& (LinReg1[0] > LinReg1[1])
						&& (Close[0] > Open[0]))
				
						// Long entry group 3
						|| ((Low[0] > RegressionChannelHighLow1.Lower[1])
						&& (LinReg1[0] > LinReg1[1])
						&& (Close[0] > Open[0]))
						)
					{

					//	Draw.Line(this, "Long Entry Line" + CurrentBar, false, 1, High[1], -1, High[1], Brushes.DeepSkyBlue, 0, 3);
						Draw.Dot(this, "NQLong1"+CurrentBar, true, 0, Low[0]-1, Brushes.LightGreen);
						Draw.Line(this, "15 Target Line" + CurrentBar, false, 1, High[1] + ProfitTarget * TickSize, -1, High[1] + 15.0 * TickSize, Brushes.LightGreen, 0, 2);
						if(sigLock1 == false)
						{
							takeTrade(1);
							sigLock1 = true;
							TriggerBar1 = CurrentBar;
						}
							

					}

					if (
						// Short entry grop 1
					((RegChannel1.Middle[0] < RegChannel1.Middle[1])
					&& (RegChannel1.Middle[1] >= RegChannel1.Middle[2])
					&& (LinReg1[0] < LinReg1[1])
					&& (Close[0] < Open[0]))
				
					// Short entry group 2
					|| ((High[0] < High[1])
					&& (High[1] >= RegChannel1.Upper[1])
					&& (LinReg1[0] < LinReg1[1])
					&& (Close[0] < Open[0]))
				
					// Short entry group 3
					|| ((High[0] < RegressionChannelHighLow1.Upper[1])
					&& (LinReg1[0] < LinReg1[1])
					&& (Close[0] < Open[0]))
						)
					{

					//	Draw.Line(this, "Short Entry Line" + CurrentBar, false, 1, Low[1], -1, Low[1], Brushes.Magenta, 0, 3);
						Draw.Dot(this, "NQShort1"+CurrentBar, true, 0, High[0]+1, Brushes.Red);
						Draw.Line(this, "15 Target Line" + CurrentBar, false, 1, Low[1] - ProfitTarget * TickSize, -1, Low[1] - 15.0 * TickSize, Brushes.LightGreen, 0, 2);
						if(sigLock2 == false)
						{
							takeTrade(-1);
							sigLock2 = true;
		  					TriggerBar2 = CurrentBar;
						}

					}
					
				}
				catch (Exception)
				{
				}
				
				
			}
			catch (Exception)
			{
			}
		}

		private FrameworkElement CreateControl()
		{
			try
			{
				this.chartWindow = (Window.GetWindow(ChartControl.Parent) as Chart);
				this.chartTraderGrid = ((Extensions.FindFirst(this.chartWindow, "ChartWindowChartTraderControl") as ChartTrader).Content as Grid);
				if (this.chartTraderGrid != null)
				{
					try
					{
						this.chartTraderButtonsGrid = (this.chartTraderGrid.Children[0] as Grid);
						if (this.chartTraderButtonsGrid != null)
						{
							Grid grid = (Grid)this.chartTraderGrid.Children[2];
							if (grid != null)
							{
								this.atmSelector = (AtmStrategySelector)grid.Children[1];
								this.atmSelector.SelectionChanged += new SelectionChangedEventHandler(this.ATMSelection_Changed);
							}
							Grid grid2 = (Grid)this.chartTraderGrid.Children[1];
							if (grid2 != null)
							{
								ChartControl.Dispatcher.Invoke(delegate()
								{
									AccountSelector accountSelector = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlAccountSelector") as AccountSelector;
									this.chartTraderAccount = ((accountSelector != null) ? accountSelector.SelectedAccount : null);
									this.accountSelector = ((accountSelector != null) ? accountSelector : null);
								});
								if (this.accountSelector != null && !this.handlerSet)
								{
									this.handlerSet = true;
									this.accountSelector.SelectionChanged += new SelectionChangedEventHandler(this.ACCOUNTSelection_Changed);
								}
								this.combo = (ComboBox)grid2.Children[4];
								foreach (Instrument instrument in Instrument.All)
								{
									if (instrument.FullName == this.combo.SelectedValue.ToString())
									{
										this.inst = instrument;
									}
								}
							}
						}
						
					}
					catch (Exception)
					{
						
					}
				}
				this._posTimer = new Timer(new TimerCallback(this.posCallback), null, 0, 200);
				this._acctTimer = new Timer(new TimerCallback(this.acctCallback), null, 0, 200);
				if (this.grid != null)
				{
					return this.grid;
				}
				this.grid = new Grid
				{
					VerticalAlignment = VerticalAlignment.Top,
					HorizontalAlignment = HorizontalAlignment.Left,
					Margin = new Thickness(this.Left, this.Top, 0.0, 0.0)
				};
				this.grid.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = default(GridLength)
				});
				this.grid.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = default(GridLength)
				});
				this.grid.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				System.Windows.Media.Brush background = (Application.Current.FindResource("BackgroundMainWindow") as System.Windows.Media.Brush) ?? Brushes.White;
				System.Windows.Media.Brush borderBrush = (Application.Current.FindResource("BorderThinBrush") as System.Windows.Media.Brush) ?? Brushes.Black;
				Grid grid3 = new Grid();
				grid3.RowDefinitions.Add(new RowDefinition
				{
					Height = new GridLength(2.0, GridUnitType.Star)
				});
				grid3.RowDefinitions.Add(new RowDefinition
				{
					Height = new GridLength(1.0, GridUnitType.Star)
				});
				grid3.RowDefinitions.Add(new RowDefinition
				{
					Height = new GridLength(2.0, GridUnitType.Star)
				});
				Grid grid4 = new Grid();
				grid4.RowDefinitions.Add(new RowDefinition
				{
					Height = new GridLength(3.0, GridUnitType.Star)
				});
				for (int i = 0; i < grid3.RowDefinitions.Count; i++)
				{
					System.Windows.Shapes.Ellipse element = new System.Windows.Shapes.Ellipse
					{
						Width = 3.0,
						Height = 3.0,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center,
						Fill = Brushes.DarkGray
					};
					Grid.SetRow(element, i);
					grid3.Children.Add(element);
				}
				System.Windows.Shapes.Polygon element2 = new System.Windows.Shapes.Polygon
				{
					Width = 12.0,
					Height = 12.0,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Fill = Brushes.DarkGray,
					Points = new PointCollection
					{
						new System.Windows.Point(1.0, 5.0),
						new System.Windows.Point(8.0, 10.0),
						new System.Windows.Point(8.0, 0.0)
					}
				};
				Grid.SetRow(element2, 0);
				grid4.Children.Add(element2);
				this.moveB = new Border
				{
					VerticalAlignment = VerticalAlignment.Top,
					BorderThickness = new Thickness(0.0, 1.0, 1.0, 1.0),
					BorderBrush = borderBrush,
					Background = background,
					Width = 12.0,
					Height = 24.0,
					Cursor = Cursors.Hand,
					Child = grid3
				};
				this.hideB = new Border
				{
					VerticalAlignment = VerticalAlignment.Bottom,
					BorderThickness = new Thickness(0.0, 1.0, 1.0, 1.0),
					BorderBrush = borderBrush,
					Background = background,
					Width = 14.0,
					Height = 24.0,
					Cursor = Cursors.Hand,
					Child = grid4
				};
				 this.moveB.MouseDown += (MouseButtonEventHandler) ((sender, e) =>
		        {
		          this.startPoint = e.GetPosition((IInputElement) this.ChartPanel);
		          this.margin = this.grid.Margin;
		          if (e.ClickCount > 1)
		          {
		            this.moveB.ReleaseMouseCapture();
		            this.ChartControl.OnIndicatorsHotKey((object) this, (KeyEventArgs) null);
		          }
		          else
		            this.moveB.CaptureMouse();
		        });
				this.moveB.MouseUp += delegate(object sender, MouseButtonEventArgs e)
				{
					this.moveB.ReleaseMouseCapture();
				};
				this.moveB.MouseMove += delegate(object sender, MouseEventArgs e)
				{
					if (this.moveB.IsMouseCaptured && this.grid != null && ChartPanel != null)
					{
						System.Windows.Point position = e.GetPosition(ChartPanel);
						this.grid.Margin = new Thickness
						{
							Left = Math.Max(0.0, Math.Min(this.margin.Left + (position.X - this.startPoint.X), ChartPanel.ActualWidth - this.grid.ActualWidth)),
							Top = Math.Max(0.0, Math.Min(this.margin.Top + (position.Y - this.startPoint.Y), ChartPanel.ActualHeight - this.grid.ActualHeight))
						};
						this.Left = this.grid.Margin.Left;
						this.Top = this.grid.Margin.Top;
						return;
					}
				};
				Grid.SetColumn(this.moveB, 1);
				Grid.SetColumn(this.hideB, 2);
				this.grid.Children.Add(this.moveB);
				Grid grid5 = new Grid();
				grid5.Background = background;
				grid5.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = default(GridLength)
				});
				grid5.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = default(GridLength)
				});
				grid5.ColumnDefinitions.Add(new ColumnDefinition
				{
					Width = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				grid5.RowDefinitions.Add(new RowDefinition
				{
					Height = default(GridLength)
				});
				Style style = Application.Current.FindResource("BasicEntryButton") as Style;
				Border border = new Border
				{
					Background = Brushes.DodgerBlue,
					VerticalAlignment = VerticalAlignment.Stretch,
					HorizontalAlignment = HorizontalAlignment.Stretch
				};
				Label label = new Label
				{
					Content = string.Format("KingKhanhATM BOT Panel", 0),
					Height = 20.0,
					Margin = new Thickness(4.0, 0.0, 4.0, 0.0),
					Padding = new Thickness(0.0, 2.0, 0.0, 2.0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				label.Foreground = Brushes.White;
				label.Background = Brushes.DodgerBlue;
				label.BorderBrush = Brushes.Transparent;
				label.HorizontalAlignment = HorizontalAlignment.Center;
				Grid.SetColumnSpan(label, 3);
				border.Child = label;
				Grid.SetRow(border, 0);
				Grid.SetColumnSpan(border, 3);
				grid5.Children.Add(border);
				this.ATMLabel = new Label
				{
					Content = string.Format("ATM Strategy: ", 0),
					Height = 20.0,
					Margin = new Thickness(4.0, 0.0, 4.0, 0.0),
					Padding = new Thickness(0.0, 2.0, 0.0, 2.0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.ATMLabel.Foreground = Brushes.White;
				this.ATMLabel.Background = Brushes.Transparent;
				this.ATMLabel.BorderBrush = Brushes.Transparent;
				this.ATMLabel.HorizontalAlignment = HorizontalAlignment.Center;
				Grid.SetColumnSpan(this.ATMLabel, 1);
				Grid.SetRow(this.ATMLabel, 1);
				grid5.Children.Add(this.ATMLabel);
				string format = string.Empty;
				if (this.atmSelector != null)
				{
					try
					{
						format = this.atmSelector.SelectedAtmStrategy.DisplayName;
					}
					catch (Exception)
					{
						format = "None";
					}
				}
				this.selectedATMStrat = new Label
				{
					Content = string.Format(format, 0),
					Height = 20.0,
					Margin = new Thickness(4.0, 0.0, 4.0, 0.0),
					Padding = new Thickness(0.0, 2.0, 0.0, 2.0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.selectedATMStrat.Foreground = Brushes.White;
				this.selectedATMStrat.Background = Brushes.Transparent;
				this.selectedATMStrat.BorderBrush = Brushes.Transparent;
				this.selectedATMStrat.HorizontalAlignment = HorizontalAlignment.Center;
				Grid.SetColumnSpan(this.selectedATMStrat, 2);
				Grid.SetColumn(this.selectedATMStrat, 1);
				Grid.SetRow(this.selectedATMStrat, 1);
				grid5.Children.Add(this.selectedATMStrat);
				this.ACCOUNTLabel = new Label
				{
					Content = string.Format("Account: ", 0),
					Height = 20.0,
					Margin = new Thickness(4.0, 0.0, 4.0, 0.0),
					Padding = new Thickness(0.0, 2.0, 0.0, 2.0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.ACCOUNTLabel.Foreground = Brushes.White;
				this.ACCOUNTLabel.Background = Brushes.Transparent;
				this.ACCOUNTLabel.BorderBrush = Brushes.Transparent;
				this.ACCOUNTLabel.HorizontalAlignment = HorizontalAlignment.Center;
				Grid.SetColumnSpan(this.ACCOUNTLabel, 1);
				Grid.SetRow(this.ACCOUNTLabel, 2);
				grid5.Children.Add(this.ACCOUNTLabel);
				string format2 = string.Empty;
				lock (Connection.Connections)
				{
					foreach (Connection connection in Connection.Connections)
					{
						if (connection.Status == (ConnectionStatus)3)
						{
							this.connected = true;
							break;
						}
					}
				}
				if (!this.connected)
				{
					format2 = "Disconnected";
				}
				else
				{
					try
					{
						if (this.accountSelector.SelectedIndex.ToString() == string.Empty || this.accountSelector.SelectedAccount.Name == string.Empty || this.accountSelector.SelectedAccount.Name == null || this.accountSelector == null)
						{
							format2 = "Select Account";
						}
						format2 = NinjaTrader.NinjaScript.Indicators.KingKhanhATM.CensorString(this.accountSelector.SelectedAccount.DisplayName);
					}
					catch (Exception)
					{
						format2 = "Select Account";
					}
				}
				this.accountSelected = new Label
				{
					Content = string.Format(format2, 0),
					Height = 20.0,
					Margin = new Thickness(4.0, 0.0, 4.0, 0.0),
					Padding = new Thickness(0.0, 2.0, 0.0, 2.0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.accountSelected.Foreground = Brushes.White;
				this.accountSelected.Background = Brushes.Transparent;
				this.accountSelected.BorderBrush = Brushes.Transparent;
				this.accountSelected.HorizontalAlignment = HorizontalAlignment.Center;
				Grid.SetColumnSpan(this.accountSelected, 2);
				Grid.SetColumn(this.accountSelected, 1);
				Grid.SetRow(this.accountSelected, 2);
				grid5.Children.Add(this.accountSelected);
				this.quantityLabel = new Label
				{
					Content = string.Format("QTY", 0),
					Height = 20.0,
					Margin = new Thickness(4.0, 0.0, 4.0, 0.0),
					Padding = new Thickness(0.0, 2.0, 0.0, 2.0),
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.quantityLabel.Foreground = Brushes.White;
				this.quantityLabel.Background = Brushes.Transparent;
				this.quantityLabel.BorderBrush = Brushes.Transparent;
				this.quantityLabel.HorizontalAlignment = HorizontalAlignment.Center;
				Grid.SetColumnSpan(this.quantityLabel, 3);
				Grid.SetRow(this.quantityLabel, 3);
				grid5.Children.Add(this.quantityLabel);
				this.decreaseQuant = new Button
				{
					Content = string.Format("-", 1),
					Name = "decreaseQuantity",
					Height = 25.0,
					Width = 25.0,
					Margin = new Thickness(0.0, 0.0, 25.0, 0.0),
					Style = style
				};
				this.decreaseQuant.Background = Brushes.DarkRed;
				this.decreaseQuant.BorderBrush = Brushes.DimGray;
				this.decreaseQuant.Foreground = Brushes.White;
				this.decreaseQuant.HorizontalAlignment = HorizontalAlignment.Right;
				this.decreaseQuant.Click += new RoutedEventHandler(this.decreaseQuant_Click);
				Grid.SetColumn(this.decreaseQuant, 0);
				Grid.SetRow(this.decreaseQuant, 4);
				grid5.Children.Add(this.decreaseQuant);
				this.increaseQuant = new Button
				{
					Content = string.Format("+", 1),
					Name = "increaseQuantity",
					Height = 25.0,
					Width = 25.0,
					Margin = new Thickness(25.0, 0.0, 0.0, 0.0),
					Style = style
				};
				this.increaseQuant.Background = Brushes.DarkGreen;
				this.increaseQuant.BorderBrush = Brushes.DimGray;
				this.increaseQuant.Foreground = Brushes.White;
				this.increaseQuant.HorizontalAlignment = HorizontalAlignment.Left;
				this.increaseQuant.Click += new RoutedEventHandler(this.increaseQuant_Click);
				Grid.SetColumn(this.increaseQuant, 2);
				Grid.SetRow(this.increaseQuant, 4);
				grid5.Children.Add(this.increaseQuant);
				this.quantityLabelVal = new TextBox
				{
					Text = string.Format(this.quantityValue.ToString(), 0),
					Height = 20.0,
					Width = 60.0,
					FontSize = 14.0,
					HorizontalAlignment = HorizontalAlignment.Center
				};
				this.quantityLabelVal.Foreground = Brushes.White;
				this.quantityLabelVal.Background = Brushes.Transparent;
				this.quantityLabelVal.BorderBrush = Brushes.Transparent;
				this.quantityLabelVal.HorizontalAlignment = HorizontalAlignment.Center;
				this.quantityLabelVal.VerticalAlignment = VerticalAlignment.Center;
				this.quantityLabelVal.TextAlignment = TextAlignment.Center;
				Grid.SetColumnSpan(this.quantityLabelVal, 3);
				Grid.SetRow(this.quantityLabelVal, 4);
				this.quantityLabelVal.PreviewKeyDown += this.TextBox_PreviewKeyDown;
				this.quantityLabelVal.LostFocus += new RoutedEventHandler(this.TextBox_LostFocus);
				this.quantityLabelVal.LostMouseCapture += new MouseEventHandler(this.TextBox_LostFocus);
				grid5.Children.Add(this.quantityLabelVal);
				this.resetQuantVal = new Button
				{
					Content = string.Format("RESET", 1),
					Name = "resetQuantVal",
					Height = 20.0,
					Width = 60.0,
					FontSize = 12.0,
					Margin = new Thickness(0.0, 4.0, 0.0, 4.0),
					Padding = new Thickness(0.0, 2.0, 0.0, 0.0),
					Style = style
				};
				this.resetQuantVal.Background = Brushes.DarkBlue;
				this.resetQuantVal.BorderBrush = Brushes.DimGray;
				this.resetQuantVal.Foreground = Brushes.White;
				this.resetQuantVal.HorizontalAlignment = HorizontalAlignment.Center;
				this.resetQuantVal.Click += new RoutedEventHandler(this.resetQuantVal_Click);
				Grid.SetColumnSpan(this.resetQuantVal, 3);
				Grid.SetRow(this.resetQuantVal, 5);
				grid5.Children.Add(this.resetQuantVal);
				this.ARMLONGbtn = new Button
				{
					Content = string.Format("LONG", 1),
					Name = "LongArmButton",
					Height = 30.0,
					Width = 125.0,
					Margin = new Thickness(4.0, 4.0, 0.0, 0.0),
					Padding = new Thickness(4.0, 0.0, 4.0, 0.0),
					Style = style
				};
				this.ARMLONGbtn.Background = Brushes.LightGray;
				this.ARMLONGbtn.BorderBrush = Brushes.DimGray;
				this.ARMLONGbtn.Foreground = Brushes.Black;
				this.ARMLONGbtn.Click += this.ARMLONGbtn_Click;
				Grid.SetColumn(this.ARMLONGbtn, 0);
				Grid.SetRow(this.ARMLONGbtn, 6);
				grid5.Children.Add(this.ARMLONGbtn);
				this.ARMSHORTbtn = new Button
				{
					Content = string.Format("SHORT", 1),
					Name = "ShortArmButton",
					Height = 30.0,
					Width = 125.0,
					Margin = new Thickness(0.0, 4.0, 4.0, 0.0),
					Padding = new Thickness(4.0, 0.0, 4.0, 0.0),
					Style = style
				};
				this.ARMSHORTbtn.Background = Brushes.LightGray;
				this.ARMSHORTbtn.BorderBrush = Brushes.DimGray;
				this.ARMSHORTbtn.Foreground = Brushes.Black;
				this.ARMSHORTbtn.Click += this.ARMSHORTbtn_Click;
				Grid.SetColumn(this.ARMSHORTbtn, 2);
				Grid.SetRow(this.ARMSHORTbtn, 6);
				grid5.Children.Add(this.ARMSHORTbtn);
				this.ARMOPTIONbtn = new Button
				{
					Content = string.Format("NEXT ENTRY", 1),
					Name = "ARMOptionButton",
					Height = 30.0,
					Margin = new Thickness(4.0, 4.0, 4.0, 0.0),
					Padding = new Thickness(4.0, 0.0, 4.0, 0.0),
					Style = style
				};
				this.ARMOPTIONbtn.Background = Brushes.LightGray;
				this.ARMOPTIONbtn.BorderBrush = Brushes.DimGray;
				this.ARMOPTIONbtn.Foreground = Brushes.Black;
				this.ARMOPTIONbtn.Click += this.ARMOPTIONbtn_Click;
				Grid.SetColumnSpan(this.ARMOPTIONbtn, 3);
				Grid.SetRow(this.ARMOPTIONbtn, 7);
				grid5.Children.Add(this.ARMOPTIONbtn);
				this.Closebtn = new Button
				{
					Content = string.Format("FLATTEN ALL", 1),
					Name = "Flatten",
					Height = 30.0,
					Margin = new Thickness(4.0, 4.0, 4.0, 0.0),
					Padding = new Thickness(4.0, 0.0, 4.0, 0.0),
					Style = style
				};
				this.Closebtn.Background = Brushes.Red;
				this.Closebtn.BorderBrush = Brushes.DimGray;
				this.Closebtn.Foreground = Brushes.White;
				this.Closebtn.Click += this.Closebtn_Click;
				this.Closebtn.PreviewMouseDown += new MouseButtonEventHandler(this.Closebtn_MouseDown);
				this.Closebtn.PreviewMouseUp += new MouseButtonEventHandler(this.Closebtn_MouseUp);
				this.Closebtn.MouseLeave += new MouseEventHandler(this.Closebtn_MouseLeave);
				Grid.SetColumnSpan(this.Closebtn, 3);
				Grid.SetRow(this.Closebtn, 9);
				grid5.Children.Add(this.Closebtn);
				this.AUTOArmbtn = new Button
				{
					Content = string.Format("CONTINUOUS ENTRY", 1),
					Name = "Continuous",
					Height = 30.0,
					Margin = new Thickness(4.0, 4.0, 4.0, 0.0),
					Padding = new Thickness(4.0, 0.0, 4.0, 0.0),
					Style = style
				};
				this.AUTOArmbtn.Background = Brushes.LightGray;
				this.AUTOArmbtn.BorderBrush = Brushes.DimGray;
				this.AUTOArmbtn.Foreground = Brushes.Black;
				this.AUTOArmbtn.Click += this.AUTOArmbtn_Click;
				Grid.SetColumnSpan(this.AUTOArmbtn, 3);
				Grid.SetRow(this.AUTOArmbtn, 8);
				grid5.Children.Add(this.AUTOArmbtn);
				this.tileHolder = new Border
				{
					Cursor = Cursors.Arrow,
					Background = (Application.Current.FindResource("BackgroundMainWindow") as System.Windows.Media.Brush),
					BorderThickness = new Thickness((double)(Application.Current.FindResource("BorderThinThickness") ?? 1)),
					BorderBrush = (Application.Current.FindResource("BorderThinBrush") as System.Windows.Media.Brush),
					Child = grid5
				};
				this.grid.Children.Add(this.tileHolder);
				this.hideB.MouseUp += delegate(object sender, MouseButtonEventArgs e)
				{
					if (this.ARMOPTIONbtn.Visibility == Visibility.Visible)
					{
						this.ARMOPTIONbtn.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.ARMOPTIONbtn.Visibility = Visibility.Visible;
					}
					if (this.ARMSHORTbtn.Visibility == Visibility.Visible)
					{
						this.ARMSHORTbtn.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.ARMSHORTbtn.Visibility = Visibility.Visible;
					}
					if (this.ARMLONGbtn.Visibility == Visibility.Visible)
					{
						this.ARMLONGbtn.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.ARMLONGbtn.Visibility = Visibility.Visible;
					}
					if (this.Closebtn.Visibility == Visibility.Visible)
					{
						this.Closebtn.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.Closebtn.Visibility = Visibility.Visible;
					}
					if (this.AUTOArmbtn.Visibility == Visibility.Visible)
					{
						this.AUTOArmbtn.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.AUTOArmbtn.Visibility = Visibility.Visible;
					}
					if (this.selectedATMStrat.Visibility == Visibility.Visible)
					{
						this.selectedATMStrat.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.selectedATMStrat.Visibility = Visibility.Visible;
					}
					if (this.ATMLabel.Visibility == Visibility.Visible)
					{
						this.ATMLabel.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.ATMLabel.Visibility = Visibility.Visible;
					}
					if (this.accountSelected.Visibility == Visibility.Visible)
					{
						this.accountSelected.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.accountSelected.Visibility = Visibility.Visible;
					}
					if (this.ACCOUNTLabel.Visibility == Visibility.Visible)
					{
						this.ACCOUNTLabel.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.ACCOUNTLabel.Visibility = Visibility.Visible;
					}
					if (this.quantityLabel.Visibility == Visibility.Visible)
					{
						this.quantityLabel.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.quantityLabel.Visibility = Visibility.Visible;
					}
					if (this.quantityLabelVal.Visibility == Visibility.Visible)
					{
						this.quantityLabelVal.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.quantityLabelVal.Visibility = Visibility.Visible;
					}
					if (this.increaseQuant.Visibility == Visibility.Visible)
					{
						this.increaseQuant.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.increaseQuant.Visibility = Visibility.Visible;
					}
					if (this.decreaseQuant.Visibility == Visibility.Visible)
					{
						this.decreaseQuant.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.decreaseQuant.Visibility = Visibility.Visible;
					}
					if (this.moveB.Visibility == Visibility.Visible)
					{
						this.moveB.Visibility = Visibility.Collapsed;
					}
					else
					{
						this.moveB.Visibility = Visibility.Visible;
					}
					if (this.resetQuantVal.Visibility == Visibility.Visible)
					{
						this.resetQuantVal.Visibility = Visibility.Collapsed;
						return;
					}
					this.resetQuantVal.Visibility = Visibility.Visible;
				};
				this.grid.Children.Add(this.hideB);
				if (this.IsVisibleOnlyFocused)
				{
					Binding binding = new Binding("IsActive")
					{
						Source = ChartControl.OwnerChart,
						Converter = (Application.Current.FindResource("BoolToVisConverter") as IValueConverter)
					};
					this.grid.SetBinding(UIElement.VisibilityProperty, binding);
					return this.grid;
				}
			}
			catch (Exception)
			{
			}
			return this.grid;
		}

		private static string CensorString(string input)
		{
			if (input != null && input.Length >= 3)
			{
				if (input.Length > 10)
				{
					input = input.Substring(0, Math.Min(input.Length, 10));
				}
				else
				{
					input = input.PadRight(10);
				}
				int count = input.Length - 2;
				string str = new string('*', count);
				return input.Substring(0, 2) + str;
			}
			return input;
		}

		private void resetQuantVal_Click(object sender, EventArgs e)
		{
			int num = 1;
			this.quantityLabelVal.Text = num.ToString();
			this.quantityValue = num;
			this.armOptionBtnToggle = false;
			this.ARMOPTIONbtn.Background = Brushes.LightGray;
			this.ARMOPTIONbtn.Foreground = Brushes.Black;
			this.ARMOPTIONbtn.Content = "NEXT ENTRY";
			this.autoArmBtnToggle = false;
			this.AUTOArmbtn.Background = Brushes.LightGray;
			this.AUTOArmbtn.Foreground = Brushes.Black;
			this.AUTOArmbtn.Content = "CONTINUOUS ENTRY - OFF";
			this.armShortBtnToggle = false;
			this.ARMSHORTbtn.Background = Brushes.DarkGray;
			this.ARMSHORTbtn.Foreground = Brushes.Black;
			this.armLongBtnToggle = false;
			this.ARMLONGbtn.Background = Brushes.DarkGray;
			this.ARMLONGbtn.Foreground = Brushes.Black;
		}

		private void increaseQuant_Click(object sender, EventArgs e)
		{
			int num;
			if ((num = this.quantityValue + 1) < 1)
			{
				num = 1;
			}
			else if (num > 50)
			{
				num = 50;
			}
			this.quantityLabelVal.Text = num.ToString();
			this.quantityValue = num;
		}

		private void decreaseQuant_Click(object sender, EventArgs e)
		{
			int num;
			if ((num = this.quantityValue - 1) < 1)
			{
				num = 1;
			}
			else if (num > 50)
			{
				num = 50;
			}
			this.quantityLabelVal.Text = num.ToString();
			this.quantityValue = num;
		}

		private void ACCOUNTSelection_Changed(object sender, EventArgs e)
		{
			bool flag = false;
			lock (Connection.Connections)
			{
				foreach (Connection connection in Connection.Connections)
				{
					if (connection.Status == (ConnectionStatus)3)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				this.accountSelected.Content = "Disconnected";
				return;
			}
			if (this.accountSelector != null)
			{
				if (this.accountSelector.SelectedIndex.ToString() == string.Empty || this.accountSelector.SelectedAccount.Name == string.Empty || this.accountSelector.SelectedAccount.Name == null || this.accountSelector == null)
				{
					this.accountSelected.Content = "Select Account";
				}
				this.accountSelected.Content = NinjaTrader.NinjaScript.Indicators.KingKhanhATM.CensorString(this.accountSelector.SelectedAccount.DisplayName);
			}
		}

		private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			TextBox textBox = sender as TextBox;
			if (textBox != null)
			{
				if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
				{
					int num = (e.Key >= Key.NumPad0) ? (e.Key - Key.NumPad0) : (e.Key - Key.D0);
					textBox.Text += num.ToString();
					e.Handled = true;
					return;
				}
				if (e.Key != Key.Back && e.Key != Key.Delete)
				{
					if (e.Key != Key.Tab)
					{
						if (e.Key == Key.Return)
						{
							textBox.Text = textBox.Text;
							this.quantityValue = int.Parse(textBox.Text);
							e.Handled = false;
							return;
						}
						e.Handled = true;
						return;
					}
				}
				e.Handled = false;
				return;
			}
		}

		private void TextBox_LostFocus(object sender, EventArgs e)
		{
			TextBox textBox = sender as TextBox;
			if (textBox != null)
			{
				textBox.Text = textBox.Text;
				this.quantityValue = int.Parse(textBox.Text);
			}
		}

		private void ATMSelection_Changed(object sender, EventArgs e)
		{
			if (this.selectedATMStrat == null || this.atmSelector.SelectedAtmStrategy == null)
			{
				this.selectedATMStrat.Content = "None";
				return;
			}
			if (!(this.atmSelector.SelectedItem.ToString() != "None"))
			{
				this.selectedATMStrat.Content = "None";
				return;
			}
			Dispatcher.InvokeAsync(delegate()
			{
				QuantityUpDown quantityUpDown = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlQuantitySelector") as QuantityUpDown;
				this.quantityATM = quantityUpDown.Value;
			});
			if (this.atmSelector.SelectedAtmStrategy.DisplayName.Contains("-"))
			{
				this.selectedATMStrat.Content = this.atmSelector.SelectedAtmStrategy.DisplayName;
				return;
			}
			this.selectedATMStrat.Content = this.atmSelector.SelectedAtmStrategy.DisplayName;
		}
		
		private void ARMLONGbtn_Click(object sender, RoutedEventArgs e)
		{
			if (!this.armLongBtnToggle)
			{
				this.armLongBtnToggle = true;
				this.onceTriggered = false;
				this.ARMLONGbtn.Background = Brushes.LightGreen;
				this.ARMLONGbtn.Foreground = Brushes.Black;
				return;
			}
			this.armLongBtnToggle = false;
			this.ARMLONGbtn.Background = Brushes.LightGray;
			this.ARMLONGbtn.Foreground = Brushes.Black;
		}

		private void onceTriggeredARMLONGbtn()
		{
			this.armLongBtnToggle = false;
			this.ARMLONGbtn.Background = Brushes.LightGray;
			this.ARMLONGbtn.Foreground = Brushes.Black;
		}

		private void ARMSHORTbtn_Click(object sender, RoutedEventArgs e)
		{
			if (!this.armShortBtnToggle)
			{
				this.armShortBtnToggle = true;
				this.onceTriggered = false;
				this.ARMSHORTbtn.Background = Brushes.Red;
				this.ARMSHORTbtn.Foreground = Brushes.White;
				return;
			}
			this.armShortBtnToggle = false;
			this.ARMSHORTbtn.Background = Brushes.LightGray;
			this.ARMSHORTbtn.Foreground = Brushes.Black;
		}

		private void potentialAUTOLong()
		{
			this.ARMLONGbtn.Background = Brushes.Yellow;
			this.ARMLONGbtn.Foreground = Brushes.Black;
		}

		private void potentialAUTOShort()
		{
			this.ARMSHORTbtn.Background = Brushes.Yellow;
			this.ARMSHORTbtn.Foreground = Brushes.Black;
		}

		private void onceTriggeredARMSHORTbtn()
		{
			this.armShortBtnToggle = false;
			this.ARMSHORTbtn.Background = Brushes.DarkGray;
			this.ARMSHORTbtn.Foreground = Brushes.Black;
		}

		private void ARMOPTIONbtn_Click(object sender, RoutedEventArgs e)
		{
			if (!this.armOptionBtnToggle)
			{
				this.armOptionBtnToggle = true;
				this.ARMOPTIONbtn.Background = Brushes.Chartreuse;
				this.ARMOPTIONbtn.Foreground = Brushes.Black;
				this.ARMOPTIONbtn.Content = "NEXT ENTRY - ON";
				this.autoArmBtnToggle = false;
				this.AUTOArmbtn.Background = Brushes.LightGray;
				this.AUTOArmbtn.Foreground = Brushes.Black;
				this.AUTOArmbtn.Content = "CONTINUOUS ENTRY - OFF";
				return;
			}
			this.armOptionBtnToggle = false;
			this.ARMOPTIONbtn.Background = Brushes.LightGray;
			this.ARMOPTIONbtn.Foreground = Brushes.Black;
			this.ARMOPTIONbtn.Content = "NEXT ENTRY";
		}

		private void Closebtn_Click(object sender, RoutedEventArgs e)
		{
			this.CloseTrades(true);
			this.autoArmBtnToggle = false;
			this.AUTOArmbtn.Background = Brushes.LightGray;
			this.AUTOArmbtn.Foreground = Brushes.Black;
			this.AUTOArmbtn.Content = "CONTINOUS ENTRY";
			this.armOptionBtnToggle = false;
			this.ARMOPTIONbtn.Background = Brushes.LightGray;
			this.ARMOPTIONbtn.Foreground = Brushes.Black;
			this.ARMOPTIONbtn.Content = "NEXT ENTRY";
			this.armShortBtnToggle = false;
			this.ARMSHORTbtn.Background = Brushes.LightGray;
			this.ARMSHORTbtn.Foreground = Brushes.Black;
			this.armLongBtnToggle = false;
			this.ARMLONGbtn.Background = Brushes.LightGray;
			this.ARMLONGbtn.Foreground = Brushes.Black;
		}
		private void Closebtn_MouseDown(object sender, RoutedEventArgs e)
		{
			if (this.Closebtn.IsMouseOver)
			{
				this.Closebtn.Background = Brushes.Maroon;
			}
			this.Closebtn.Background = Brushes.Maroon;
			this.Closebtn.FontStyle = FontStyles.Oblique;
		}

		private void Closebtn_MouseUp(object sender, RoutedEventArgs e)
		{
			this.Closebtn.Background = Brushes.Red;
			this.Closebtn.FontStyle = FontStyles.Normal;
		}

		private void Closebtn_MouseLeave(object sender, RoutedEventArgs e)
		{
			this.Closebtn.Background = Brushes.Red;
		}

		private void AUTOArmbtn_Click(object sender, RoutedEventArgs e)
		{
			if (!this.autoArmBtnToggle)
			{
				this.autoArmBtnToggle = true;
				this.AUTOArmbtn.Background = Brushes.Chartreuse;
				this.AUTOArmbtn.Foreground = Brushes.Black;
				this.AUTOArmbtn.Content = "CONTINUOUS ENTRY - ON";
				this.armOptionBtnToggle = false;
				this.ARMOPTIONbtn.Background = Brushes.LightGray;
				this.ARMOPTIONbtn.Foreground = Brushes.Black;
				this.ARMOPTIONbtn.Content = "NEXT ENTRY - OFF";
				return;
			}
			this.autoArmBtnToggle = false;
			this.AUTOArmbtn.Background = Brushes.LightGray;
			this.AUTOArmbtn.Foreground = Brushes.Black;
			this.AUTOArmbtn.Content = "CONTINUOUS ENTRY";
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
		}

		public override void OnRenderTargetChanged()
		{
		}

		 private void CloseTrades(bool yn)
	    {
	     if (yn)
			{
				List<Instrument> instrumentNames = new List<Instrument>();
				Dispatcher.BeginInvoke(new Action(delegate()
	        {
	          try
	          {
	            if (this.chartTraderAccount.Positions.Count == 0)
	              return;
	            foreach (Position position in this.chartTraderAccount.Positions)
	            {
	              Instrument instrument = position.Instrument;
	              if (!instrumentNames.Contains(instrument))
	                instrumentNames.Add(instrument);
	            }
	            this.chartTraderAccount.Flatten((ICollection<Instrument>) instrumentNames);
	            Thread.Sleep(250);
	          }
	          catch (Exception ex)
	          {
	          }
	        }));
	      }
	      else
	      {
	        if (this.inPosition && this.posDirection == -1)
	        {
	          int quantity = this.positions.Quantity;
	          int num = 0;
	          this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	          {
	            this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 1, (OrderType) 1, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "Entry", (CustomOrder) null)
	          });
	          foreach (Order order in this.chartTraderAccount.Orders)
	          {
	            if (order.Instrument == this.inst && order.OrderType == null && order.OrderState == (OrderState)10 && num < quantity)
	            {
	              ++num;
	              this.chartTraderAccount.Cancel((IEnumerable<Order>) new Order[1]
	              {
	                order
	              });
	            }
	          }
	        }
	        if (!this.inPosition || this.posDirection != 1)
	          return;
	        int quantity1 = this.positions.Quantity;
	        int num1 = 0;
	        this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	        {
	          this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (TimeInForce) 1, quantity1, 0.0, 0.0, string.Empty, "Entry", (CustomOrder) null)
	        });
	        foreach (Order order in this.chartTraderAccount.Orders)
	        {
	          if (order.Instrument == this.inst && order.OrderType == null && order.OrderState == (OrderState)10 && num1 < quantity1)
	          {
	            ++num1;
	            this.chartTraderAccount.Cancel((IEnumerable<Order>) new Order[1]
	            {
	              order
	            });
	          }
	        }
	      }
	    }

		private void takeTrade(int direction)
	    {
			if (State != State.Realtime || !connected || this.getInstrumentPosition() != null)
	        return;
	      if (!this.armLongBtnToggle)
	      {
	        if (!this.armShortBtnToggle)
	          return;
	      }
	      try
				{
					AtmStrategy atmStrategy = null;
					object atmSelChoice = null;
					ChartControl.Dispatcher.Invoke(delegate()
					{
						AtmStrategySelector atmStrategySelector = Extensions.FindFirst(Window.GetWindow(this.ChartControl.Parent), "ChartTraderControlATMStrategySelector") as AtmStrategySelector;
						atmSelChoice = ((atmStrategySelector != null) ? atmStrategySelector.SelectedItem : null);
					});
					ChartControl.Dispatcher.Invoke(delegate()
					{
						QuantityUpDown quantityUpDown = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlQuantitySelector") as QuantityUpDown;
						this.quantityATM = quantityUpDown.Value;
					});
	        int quantity = this.quantityValue;
	        this.Dispatcher.InvokeAsync((Action) (() => quantity = this.quantityValue));
	        if (this.chartTraderAccount == null || atmSelChoice == null)
	          return;
	        if (atmSelChoice.ToString() != "None")
	          atmStrategy = (NinjaTrader.NinjaScript.AtmStrategy) atmSelChoice;
	        if (!this.armOptionBtnToggle && this.autoArmBtnToggle && (this.armLongBtnToggle || this.armShortBtnToggle))
	        {
	          if (!this.inPosition)
	          {
	            if (direction > 0 && this.armLongBtnToggle)
	            {
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  try
	                  {
	                    Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                  }
	                  catch (Exception ex)
	                  {
	                  }
	                }
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	                return;
	              }
	            }
	            else if (direction < 0 && this.armShortBtnToggle)
	            {
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  try
	                  {
	                    Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                  }
	                  catch (Exception ex)
	                  {
	                  }
	                }
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	                return;
	              }
	            }
	          }
	          if (this.inPosition && this.posDirection == -1)
	          {
	            if (direction > 0 && this.armLongBtnToggle)
	            {
	              this.CloseTrades(false);
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  try
	                  {
	                    Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                  }
	                  catch (Exception ex)
	                  {
	                  }
	                }
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	                return;
	              }
	            }
	            else if (this.armShortBtnToggle && direction < 0)
	            {
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                  NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                }
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	                return;
	              }
	            }
	          }
	          if (!this.inPosition || this.posDirection != 1)
	            return;
	          if (direction < 0 && this.armShortBtnToggle)
	          {
	            this.CloseTrades(false);
	            if (atmStrategy != null && atmSelChoice.ToString() != "None")
	            {
	              for (int index = 0; index < quantity; ++index)
	              {
	                try
	                {
	                  Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                  NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                }
	                catch (Exception ex)
	                {
	                }
	              }
	            }
	            else
	            {
	              if (!(atmSelChoice.ToString() == "None") || atmStrategy != null)
	                return;
	              this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	              {
	                this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	              });
	            }
	          }
	          else
	          {
	            if (!this.armLongBtnToggle || direction <= 0)
	              return;
	            if (atmStrategy != null && atmSelChoice.ToString() != "None")
	            {
	              for (int index = 0; index < quantity; ++index)
	              {
	                Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	              }
	            }
	            else
	            {
	              if (!(atmSelChoice.ToString() == "None") || atmStrategy != null)
	                return;
	              this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	              {
	                this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	              });
	            }
	          }
	        }
	        else
	        {
	          if (this.autoArmBtnToggle || !this.armOptionBtnToggle || this.onceTriggered || !this.armLongBtnToggle && !this.armShortBtnToggle)
	            return;
	          if (!this.inPosition)
	          {
	            if (direction > 0 && this.armLongBtnToggle)
	            {
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  try
	                  {
	                    Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                  }
	                  catch (Exception ex)
	                  {
	                  }
	                }
	                ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	               ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	            }
	            else if (direction < 0 && this.armShortBtnToggle)
	            {
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  try
	                  {
	                    Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                  }
	                  catch (Exception ex)
	                  {
	                  }
	                }
	                ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	                ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	            }
	          }
	          if (this.inPosition && this.posDirection == -1)
	          {
	            if (direction > 0 && this.armLongBtnToggle)
	            {
	              this.CloseTrades(false);
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  try
	                  {
	                    Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                    NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                  }
	                  catch (Exception ex)
	                  {
	                  }
	                }
	                ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	               ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	            }
	            else if (this.armShortBtnToggle && direction < 0)
	            {
	              if (atmStrategy != null && atmSelChoice.ToString() != "None")
	              {
	                for (int index = 0; index < quantity; ++index)
	                {
	                  Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                  NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                }
	                ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	              if (atmSelChoice.ToString() == "None" && atmStrategy == null)
	              {
	                this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	                {
	                  this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	                });
	                ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	                return;
	              }
	            }
	          }
	          if (!this.inPosition || this.posDirection != 1)
	            return;
	          if (direction < 0 && this.armShortBtnToggle)
	          {
	            this.CloseTrades(false);
	            if (atmStrategy != null && atmSelChoice.ToString() != "None")
	            {
	              for (int index = 0; index < quantity; ++index)
	              {
	                try
	                {
	                  Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                  NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	                }
	                catch (Exception ex)
	                {
	                }
	              }
	              ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	            }
	            else
	            {
	              if (!(atmSelChoice.ToString() == "None") || atmStrategy != null)
	                return;
	              this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	              {
	                this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 2, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	              });
	              ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	            }
	          }
	          else
	          {
	            if (!this.armLongBtnToggle || direction <= 0)
	              return;
	            if (atmStrategy != null && atmSelChoice.ToString() != "None")
	            {
	              for (int index = 0; index < quantity; ++index)
	              {
	                Order order = this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, this.quantityATM, 0.0, 0.0, string.Empty, "Entry", Globals.MaxDate, (CustomOrder) null);
	                NinjaTrader.NinjaScript.AtmStrategy.StartAtmStrategy(atmStrategy, order);
	              }
	              ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	            }
	            else
	            {
	              if (!(atmSelChoice.ToString() == "None") || atmStrategy != null)
	                return;
	              this.chartTraderAccount.Submit((IEnumerable<Order>) new Order[1]
	              {
	                this.chartTraderAccount.CreateOrder(this.inst, (OrderAction) 0, (OrderType) 1, (OrderEntry) 0, (TimeInForce) 1, quantity, 0.0, 0.0, string.Empty, "EntryNaked", Globals.MaxDate, (CustomOrder) null)
	              });
	              ChartControl.Dispatcher.InvokeAsync(delegate()
											{
												this.onceTriggeredARMLONGbtn();
												this.onceTriggeredARMSHORTbtn();
												this.onceTriggered = true;
											});
	            }
	          }
	        }
	      }
	      catch (Exception ex)
	      {
	      }
	    }

		private Position getInstrumentPosition()
		{
			long instrumentId = this.inst.Id;
			Position pos = null;
			Account myAccount = null;
			try
			{
				ChartControl.Dispatcher.Invoke(delegate()
				{
					lock (Account.All)
					{
						myAccount = Account.All.FirstOrDefault((Account a) => a.Name == this.accountSelector.SelectedAccount.DisplayName);
					}
					pos = myAccount.GetPosition(instrumentId);
				});
				if (myAccount == null)
				{
					return null;
				}
				if (pos != null)
				{
					return pos;
				}
			}
			catch (Exception)
			{
			}
			return pos;
		}

		private void acctCallback(object o)
		{
			try
			{
				ChartControl.Dispatcher.Invoke(delegate()
				{
					AccountSelector accountSelector = Extensions.FindFirst(Window.GetWindow(ChartControl.Parent), "ChartTraderControlAccountSelector") as AccountSelector;
					this.chartTraderAccount = ((accountSelector != null) ? accountSelector.SelectedAccount : null);
					this.accountSelector = ((accountSelector != null) ? accountSelector : null);
				});
				if (this.chartTraderAccount != null && !this.handlerSet)
				{
					this.handlerSet = true;
					this.accountSelector.SelectionChanged += new SelectionChangedEventHandler(this.ACCOUNTSelection_Changed);
				}
			}
			catch (Exception)
			{
			}
		}

		private void posCallback(object o)
		{
			long id = this.inst.Id;
			Position position = null;
			Account account = null;
			try
			{
				lock (Account.All)
				{
					account = Account.All.FirstOrDefault((Account a) => a.Name == this.chartTraderAccount.DisplayName);
				}
				position = account.GetPosition(id);
				realizedPnL = account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar); /// Added
			}
			catch (Exception)
			{
				
			}
			if (account == null)
			{
				this.inPosition = false;
				this.posDirection = 0;
				this.positions = null;
				return;
			}
			if (position == null)
			{
				this.inPosition = false;
				this.posDirection = 0;
				this.positions = null;
				return;
			}
			if (position != null)
			{
				this.inPosition = true;
				this.positions = position;
				if (position.MarketPosition == null)
				{
					this.posDirection = 1;
					return;
				}
				if (position.MarketPosition == (MarketPosition)1)
				{
					this.posDirection = -1;
				}
			}
		}

		private void UpdateRect(ref RectangleF updateRectangle, float x, float y, float width, float height)
		{
			updateRectangle.X = x;
			updateRectangle.Y = y;
			updateRectangle.Width = width;
			updateRectangle.Height = height;
		}

		private void UpdateRect(ref RectangleF rectangle, int x, int y, int width, int height)
		{
			this.UpdateRect(ref rectangle, (float)x, (float)y, (float)width, (float)height);
		}

		[Browsable(false)]
		public double Top { get; set; }

		[Browsable(false)]
		public double Left { get; set; }

		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptIsVisibleOnlyFocused", GroupName = "NinjaScriptIndicatorVisualGroup", Order = 499)]
		[Browsable(false)]
		public bool IsVisibleOnlyFocused { get; set; }

		[Browsable(false)]
		public XElement SelectedTypes { get; set; }

		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptNumberOfRows", GroupName = "NinjaScriptParameters", Order = 0)]
		[Range(1, 2147483647)]
		[Browsable(false)]
		public int NumberOfRows { get; set; }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target", GroupName = "Total Strategy Profit Target / StopLoss", Order = 0)]
		public int realizedProfit
		{ get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StopLoss (Negative Number)", GroupName = "Total Strategy Profit Target / StopLoss", Order = 0)]
		public int realizedLoss
		{ get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Start Time", GroupName = "Time Settings HHMMSS Format", Order = 2)]
		public int startTime
		{ get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "End Time", GroupName = "Time Settings HHMMSS Format", Order = 3)]
		public int endTime
		{ get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Econ1 Time", GroupName = "Time Settings HHMMSS Format", Order = 4)]
		public int econNumber1
		{ get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Econ2 Time", GroupName = "Time Settings HHMMSS Format", Order = 5)]
		public int econNumber2
		{ get; set; }
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Market Open Time", GroupName = "Time Settings HHMMSS Format", Order = 1)]
		public int openTime
		{ get; set; }

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Order=1, GroupName="Parameters")]
		public int Contracts
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TrailStop", Order=2, GroupName="Parameters")]
		public int TrailStop
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ProfitTarget", Order=3, GroupName="Parameters")]
		public int ProfitTarget
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LinRegPeriod", Order=4, GroupName="Parameters")]
		public int LinRegPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="RegChanPeriod", Order=5, GroupName="Parameters")]
		public int RegChanPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Upper", Order=6, GroupName="Parameters")]
		public double Upper
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Lower", Order=7, GroupName="Parameters")]
		public double Lower
		{ get; set; }
		#endregion


		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private KingKhanhATM[] cacheKingKhanhATM;
		public KingKhanhATM KingKhanhATM(int realizedProfit, int realizedLoss, int startTime, int endTime, int econNumber1, int econNumber2, int openTime, int contracts, int trailStop, int profitTarget, int linRegPeriod, int regChanPeriod, double upper, double lower)
		{
			return KingKhanhATM(Input, realizedProfit, realizedLoss, startTime, endTime, econNumber1, econNumber2, openTime, contracts, trailStop, profitTarget, linRegPeriod, regChanPeriod, upper, lower);
		}

		public KingKhanhATM KingKhanhATM(ISeries<double> input, int realizedProfit, int realizedLoss, int startTime, int endTime, int econNumber1, int econNumber2, int openTime, int contracts, int trailStop, int profitTarget, int linRegPeriod, int regChanPeriod, double upper, double lower)
		{
			if (cacheKingKhanhATM != null)
				for (int idx = 0; idx < cacheKingKhanhATM.Length; idx++)
					if (cacheKingKhanhATM[idx] != null && cacheKingKhanhATM[idx].realizedProfit == realizedProfit && cacheKingKhanhATM[idx].realizedLoss == realizedLoss && cacheKingKhanhATM[idx].startTime == startTime && cacheKingKhanhATM[idx].endTime == endTime && cacheKingKhanhATM[idx].econNumber1 == econNumber1 && cacheKingKhanhATM[idx].econNumber2 == econNumber2 && cacheKingKhanhATM[idx].openTime == openTime && cacheKingKhanhATM[idx].Contracts == contracts && cacheKingKhanhATM[idx].TrailStop == trailStop && cacheKingKhanhATM[idx].ProfitTarget == profitTarget && cacheKingKhanhATM[idx].LinRegPeriod == linRegPeriod && cacheKingKhanhATM[idx].RegChanPeriod == regChanPeriod && cacheKingKhanhATM[idx].Upper == upper && cacheKingKhanhATM[idx].Lower == lower && cacheKingKhanhATM[idx].EqualsInput(input))
						return cacheKingKhanhATM[idx];
			return CacheIndicator<KingKhanhATM>(new KingKhanhATM(){ realizedProfit = realizedProfit, realizedLoss = realizedLoss, startTime = startTime, endTime = endTime, econNumber1 = econNumber1, econNumber2 = econNumber2, openTime = openTime, Contracts = contracts, TrailStop = trailStop, ProfitTarget = profitTarget, LinRegPeriod = linRegPeriod, RegChanPeriod = regChanPeriod, Upper = upper, Lower = lower }, input, ref cacheKingKhanhATM);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.KingKhanhATM KingKhanhATM(int realizedProfit, int realizedLoss, int startTime, int endTime, int econNumber1, int econNumber2, int openTime, int contracts, int trailStop, int profitTarget, int linRegPeriod, int regChanPeriod, double upper, double lower)
		{
			return indicator.KingKhanhATM(Input, realizedProfit, realizedLoss, startTime, endTime, econNumber1, econNumber2, openTime, contracts, trailStop, profitTarget, linRegPeriod, regChanPeriod, upper, lower);
		}

		public Indicators.KingKhanhATM KingKhanhATM(ISeries<double> input , int realizedProfit, int realizedLoss, int startTime, int endTime, int econNumber1, int econNumber2, int openTime, int contracts, int trailStop, int profitTarget, int linRegPeriod, int regChanPeriod, double upper, double lower)
		{
			return indicator.KingKhanhATM(input, realizedProfit, realizedLoss, startTime, endTime, econNumber1, econNumber2, openTime, contracts, trailStop, profitTarget, linRegPeriod, regChanPeriod, upper, lower);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.KingKhanhATM KingKhanhATM(int realizedProfit, int realizedLoss, int startTime, int endTime, int econNumber1, int econNumber2, int openTime, int contracts, int trailStop, int profitTarget, int linRegPeriod, int regChanPeriod, double upper, double lower)
		{
			return indicator.KingKhanhATM(Input, realizedProfit, realizedLoss, startTime, endTime, econNumber1, econNumber2, openTime, contracts, trailStop, profitTarget, linRegPeriod, regChanPeriod, upper, lower);
		}

		public Indicators.KingKhanhATM KingKhanhATM(ISeries<double> input , int realizedProfit, int realizedLoss, int startTime, int endTime, int econNumber1, int econNumber2, int openTime, int contracts, int trailStop, int profitTarget, int linRegPeriod, int regChanPeriod, double upper, double lower)
		{
			return indicator.KingKhanhATM(input, realizedProfit, realizedLoss, startTime, endTime, econNumber1, econNumber2, openTime, contracts, trailStop, profitTarget, linRegPeriod, regChanPeriod, upper, lower);
		}
	}
}

#endregion
