#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.AB
{
	public class ABToolbarButtons : Indicator
	{
		
		private bool drawSwitch;
		private bool indiSwitch;
				
	    // Define a Chart object to refer to the chart on which the indicator resides
		private Chart chartWindow;
	 
	    // Define a Button
	    private new System.Windows.Controls.Button btnDrawObjs;
		private new System.Windows.Controls.Button btnIndicators;
		private new System.Windows.Controls.Button btnShowTrades;
		private new System.Windows.Controls.Button btnHideWicks;
		
		private bool IsToolBarButtonAdded;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ABToolbarButtons";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsAutoScale									= true;
				IsSuspendedWhileInactive					= true;
				indiSwitch									= true;
				colorActiveCursor							= true;
				ActiveCursorColor							= Brushes.DarkGreen;
				InactiveCursorColor							= Brushes.DimGray;
				
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.Realtime)
			{
				//Call the custom method in State.Historical or State.Realtime to ensure it is only done when applied to a chart not when loaded in the Indicators window				
				if (ChartControl != null && !IsToolBarButtonAdded)
				{
				    ChartControl.Dispatcher.InvokeAsync((Action)(() => // Use this.Dispatcher to ensure code is executed on the proper thread
				    {
						AddButtonToToolbar();
					}));
				}
			}
			else if (State == State.Terminated)
			{
				if (chartWindow != null)
				{
			        ChartControl.Dispatcher.InvokeAsync((Action)(() => //Dispatcher used to Assure Executed on UI Thread
			        {	
						DisposeCleanUp();
					}));
				}
			}
		}

		protected override void OnBarUpdate()
		{
		}
		
		private void AddButtonToToolbar()
		{
				//Obtain the Chart on which the indicator is configured
				chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;
		        if (chartWindow == null)
		        {
		            Print("chartWindow == null");
		            return;
		        }
				
				// subscribe chartwindow to keypress events
				if (chartWindow != null)
				{
					chartWindow.KeyUp += OnKeyUp;
					chartWindow.MouseLeftButtonDown += OnMouseLeftDown;					
					chartWindow.PreviewMouseWheel += OnMouseWheel;
					chartWindow.MouseEnter += OnMouseEnter;
					chartWindow.MouseLeave += OnMouseLeave;
				}
				
				// Create a style to apply to the button
		        Style btnStyle = new Style();
		        btnStyle.TargetType = typeof(System.Windows.Controls.Button);
				
		        btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontSizeProperty, 11.0));
		        btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontFamilyProperty, new FontFamily("Arial")));
		        btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontWeightProperty, FontWeights.Bold));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.MarginProperty, new Thickness(2, 0, 2, 0)));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.PaddingProperty, new Thickness(4, 2, 4, 2)));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, Brushes.WhiteSmoke));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.BackgroundProperty, Brushes.DimGray));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.IsEnabledProperty, true));
				btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.HorizontalAlignmentProperty, HorizontalAlignment.Center));
				
		        // Instantiate the buttons
		        btnDrawObjs = new System.Windows.Controls.Button();
				btnIndicators = new System.Windows.Controls.Button();
				btnShowTrades = new System.Windows.Controls.Button();
				btnHideWicks = new System.Windows.Controls.Button();
				
				// Set button names
				btnDrawObjs.Content = "Toggle Draw";
				btnIndicators.Content = "Toggle Indicators";
				btnShowTrades.Content = "Toggle Trades";
				btnHideWicks.Content = "Toggle Wicks";
								
		        // Set Button style            
		        btnDrawObjs.Style = btnStyle;
				btnIndicators.Style = btnStyle;
				btnShowTrades.Style = btnStyle;
				btnHideWicks.Style = btnStyle;
				
				// Add the Buttons to the chart's toolbar
				chartWindow.MainMenu.Add(btnDrawObjs);
				chartWindow.MainMenu.Add(btnIndicators);
				chartWindow.MainMenu.Add(btnShowTrades);
				chartWindow.MainMenu.Add(btnHideWicks);
				
				// Set button visibility
				btnDrawObjs.Visibility = Visibility.Visible;
				btnIndicators.Visibility = Visibility.Visible;
				btnShowTrades.Visibility = Visibility.Visible;
				btnHideWicks.Visibility = Visibility.Visible;
				
				// Subscribe to click events
				btnDrawObjs.Click += btnDrawObjsClick;
				btnIndicators.Click += btnIndicatorsClick;
				btnShowTrades.Click += btnShowTradesClick;
		 		btnHideWicks.Click += btnHideWicksClick;
				
				// Set this value to true so it doesn't add the
				// toolbar multiple times if NS code is refreshed
		        IsToolBarButtonAdded = true;
		}		
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
		}
		
		private void btnDrawObjsClick(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button != null)
			{								
				hideDrawsFunc();
			}
		}		
		
		private void btnIndicatorsClick(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button != null)
			{								
				// toggle all indicators
				foreach (var obj in chartWindow.ActiveChartControl.ChartObjects)
				{
				    var indi = obj as Indicator;
				    if (indi != null)
				    {
						if (indiSwitch)
						{
							if (indi.Name !="ABMarketOpenLine")
							{
								indi.IsVisible = false;
							}
						}
						else if (!indiSwitch)
						{
							if (indi.Name !="ABMarketOpenLine")
							{
								indi.IsVisible = true;
							}
						}
				    }
				}				
				indiSwitch = !indiSwitch;
				chartWindow.ActiveChartControl.InvalidateVisual();
				ForceRefresh();
			}
		}
				
		private void btnShowTradesClick(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button != null)
			{
				// toggle trades				
				foreach (var obj in chartWindow.ActiveChartControl.ChartObjects)
				{
			    var trades = obj as ChartBars;
				    if (trades != null)
				    {
						switch (trades.Properties.PlotExecutions)
						{
							case ChartExecutionStyle.DoNotPlot:
							{
								trades.Properties.PlotExecutions = ChartExecutionStyle.MarkersOnly;
								break;
							}							
							case ChartExecutionStyle.MarkersOnly:
							{
								trades.Properties.PlotExecutions = ChartExecutionStyle.DoNotPlot;
								break;
							}							
						}
					}
				}
									
				chartWindow.ActiveChartControl.InvalidateVisual();
				ForceRefresh();
			}
		}
		
		private void btnHideWicksClick(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			if (button != null)
			{
				hideWicksFunc();
			}				
		}
				
		private void hideDrawsFunc()
		{			
			// toggle all drawing objects
			// turns off historical drawings but future drawings will show until hidden
						
			// Sets drawSwitch based on whether there are any drawings on the chart
			foreach (var obj in chartWindow.ActiveChartControl.ChartObjects)
			{
			    var draw = obj as DrawingTool;
			    if (draw != null)
			    {
			        if (draw.IsVisible && draw.IsUserDrawn)
					{
						drawSwitch = true;
						break;
					}
					else
					{
						drawSwitch = false;
					}
			    }
			}
			
			foreach (var obj in chartWindow.ActiveChartControl.ChartObjects)
			{
			    var draw = obj as DrawingTool;
			    if (draw != null)
			    {
			        if (draw.IsUserDrawn)
			        {						
						if (drawSwitch)
						{
							draw.IsVisible = false;
							// Print(draw.Name + " '" + draw.Tag + "' is hidden.");
						}
						else if (!drawSwitch)
						{
							draw.IsVisible = true;
							// Print(draw.Name + " '" + draw.Tag + "'  is visible.");
						}
			        }
			    }
			}
			
			drawSwitch = !drawSwitch;
			chartWindow.ActiveChartControl.InvalidateVisual();
			ForceRefresh();
		}
				
		private void hideSelectedDrawFunc()
		{
			// turns off historical drawings but future drawings will show until hidden
			foreach (var obj in chartWindow.ActiveChartControl.ChartObjects)
			{
			    var draw = obj as DrawingTool;
			    if (draw != null)
			    {
			        if (draw.IsUserDrawn && draw.IsSelected)
			        {
						draw.IsVisible = false;  // hide selected drawing
			        }
			    }
			}
						
			chartWindow.ActiveChartControl.InvalidateVisual();
			ForceRefresh();
		}
		
		private void hideWicksFunc()
		{
			// toggle wicks
			foreach (var obj in chartWindow.ActiveChartControl.ChartObjects)
			{
		    var wicks = obj as ChartBars;
			    if (wicks != null)
			    {
					switch (wicks.Properties.ChartStyle.Stroke2.Brush.ToString())
					{
						case "#FF000000":
						{
							wicks.Properties.ChartStyle.Stroke2.Brush = Brushes.Transparent;
							break;
						}
						
						case "#00FFFFFF":
						{
							wicks.Properties.ChartStyle.Stroke2.Brush = Brushes.Black;
							break;
						}
						
						default:
						{						
							wicks.Properties.ChartStyle.Stroke2.Brush = Brushes.Black;
							break;
						}
					}
				}
			}
								
			chartWindow.ActiveChartControl.InvalidateVisual();
			ForceRefresh();
		}
		
		public void OnKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Oem3 && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) hideWicksFunc();
			if (e.Key == Key.D && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) hideDrawsFunc();
			if (e.Key == Key.H && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) hideSelectedDrawFunc();
		}
		
		public void OnMouseLeftDown(object sender, MouseEventArgs e)
		{	
			// Hide selected drawing if CTRL + SHIFT + CLICK on a drawing
			if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))))
			{
				foreach (var obj in chartWindow.ActiveChartControl.ChartObjects)
				{
				    var draw = obj as DrawingTool;
				    if (draw != null)
				    {
				        if (draw.IsUserDrawn && draw.IsSelected)
				        {
							draw.IsVisible = false;  // hide selected drawing
				        }
				    }
				}
				
				chartWindow.ActiveChartControl.InvalidateVisual();
				ForceRefresh();
			}
		}
				
		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
		    if (chartWindow.ActiveChartControl != null && ChartBars != null)
			{
				if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
				{
					if (e.Delta < 0)
					{
						chartWindow.ActiveChartControl.Properties.BarDistance = (float)(chartWindow.ActiveChartControl.Properties.BarDistance * 0.9);
						chartWindow.ActiveChartControl.BarWidth = chartWindow.ActiveChartControl.BarWidth * 0.9;
					}
					else if (e.Delta > 0)
					{
						chartWindow.ActiveChartControl.Properties.BarDistance = (float)(chartWindow.ActiveChartControl.Properties.BarDistance / 0.9);
						chartWindow.ActiveChartControl.BarWidth = chartWindow.ActiveChartControl.BarWidth / 0.9;
					}			
					e.Handled = true;
					chartWindow.ActiveChartControl.InvalidateVisual();
					ForceRefresh();
				}
			}
		}
		
		private void OnMouseEnter(object sender, MouseEventArgs e)
		{
			if (chartWindow != null && chartWindow.ActiveChartControl.GetType() == typeof(ChartControl) && colorActiveCursor)
			{
				chartWindow.ActiveChartControl.Properties.CrosshairLabelBackground = ActiveCursorColor;
			}
		}
		
		private void OnMouseLeave(object sender, MouseEventArgs e)
		{
			if (chartWindow != null && chartWindow.ActiveChartControl.GetType() == typeof(ChartControl) && colorActiveCursor)
			{
				chartWindow.ActiveChartControl.Properties.CrosshairLabelBackground = InactiveCursorColor;
			}
		}
		
		private void DisposeCleanUp()
		{
		    // remove toolbar items and unsubscribe from events
			chartWindow.KeyUp -= OnKeyUp;
			chartWindow.MouseLeftButtonDown -= OnMouseLeftDown;
			chartWindow.PreviewMouseWheel -= OnMouseWheel;
			chartWindow.MouseEnter -= OnMouseEnter;
			chartWindow.MouseLeave -= OnMouseLeave;
						
            if (btnDrawObjs != null) chartWindow.MainMenu.Remove(btnDrawObjs);
				btnDrawObjs.Click -= btnDrawObjsClick;
			if (btnIndicators != null) chartWindow.MainMenu.Remove(btnIndicators);
				btnIndicators.Click -= btnIndicatorsClick;
			if (btnShowTrades != null) chartWindow.MainMenu.Remove(btnShowTrades);
				btnShowTrades.Click -= btnShowTradesClick;
			if (btnHideWicks != null) chartWindow.MainMenu.Remove(btnHideWicks);
				btnHideWicks.Click -= btnHideWicksClick;
		}
		
		
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "Color active cursor labels?", Order = 0, GroupName = "Other Settings")]
		public bool colorActiveCursor
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Active Cursor Color", Order=1, GroupName="Other Settings")]
		public Brush ActiveCursorColor
		{ get; set; }
			
		[Browsable(false)]
		public string ActiveCursorColorSerializable
		{
			get { return Serialize.BrushToString(ActiveCursorColor); }
			set { ActiveCursorColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Inctive Cursor Color", Order=2, GroupName="Other Settings")]
		public Brush InactiveCursorColor
		{ get; set; }
			
		[Browsable(false)]
		public string InactiveCursorColorSerializable
		{
			get { return Serialize.BrushToString(InactiveCursorColor); }
			set { InactiveCursorColor = Serialize.StringToBrush(value); }
		}
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AB.ABToolbarButtons[] cacheABToolbarButtons;
		public AB.ABToolbarButtons ABToolbarButtons(bool colorActiveCursor)
		{
			return ABToolbarButtons(Input, colorActiveCursor);
		}

		public AB.ABToolbarButtons ABToolbarButtons(ISeries<double> input, bool colorActiveCursor)
		{
			if (cacheABToolbarButtons != null)
				for (int idx = 0; idx < cacheABToolbarButtons.Length; idx++)
					if (cacheABToolbarButtons[idx] != null && cacheABToolbarButtons[idx].colorActiveCursor == colorActiveCursor && cacheABToolbarButtons[idx].EqualsInput(input))
						return cacheABToolbarButtons[idx];
			return CacheIndicator<AB.ABToolbarButtons>(new AB.ABToolbarButtons(){ colorActiveCursor = colorActiveCursor }, input, ref cacheABToolbarButtons);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AB.ABToolbarButtons ABToolbarButtons(bool colorActiveCursor)
		{
			return indicator.ABToolbarButtons(Input, colorActiveCursor);
		}

		public Indicators.AB.ABToolbarButtons ABToolbarButtons(ISeries<double> input , bool colorActiveCursor)
		{
			return indicator.ABToolbarButtons(input, colorActiveCursor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AB.ABToolbarButtons ABToolbarButtons(bool colorActiveCursor)
		{
			return indicator.ABToolbarButtons(Input, colorActiveCursor);
		}

		public Indicators.AB.ABToolbarButtons ABToolbarButtons(ISeries<double> input , bool colorActiveCursor)
		{
			return indicator.ABToolbarButtons(input, colorActiveCursor);
		}
	}
}

#endregion
