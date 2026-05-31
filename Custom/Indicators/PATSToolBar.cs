/*
*	PAT$ ToolBar Indicator made with ♡ by beo
* 	Last edit 03/03/2021
*	https://priceactiontradingsystem.com/link-to-forum/topic/pats-toolbar-custom-drawing-tools-why-this-is-better/
*/

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Automation;
using System.Windows.Data;
using System.Windows.Input;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using WindowsInput.Native; 
using WindowsInput;
#endregion

public enum ArrowIconStyleEnum { ColorFilled, ColorOutlined, Monochrome }

namespace NinjaTrader.NinjaScript.Indicators
{
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.CustomDrawingToolIndicatorTypeConverter")]
	public class PATSToolBar : Indicator
	{
		private const string assembly = "NinjaTrader.Custom";
		//private const string assembly = "Toolbar";
		private List<object> toolBarItems;
		private bool isToolBarAdded;
		private ArrowIconStyleEnum arrowIconStyle = ArrowIconStyleEnum.ColorFilled;
		private NinjaTrader.Gui.Chart.Chart cw; //chartWindow
		private System.Windows.Forms.Timer myTimer;
		private ATR myATR;
		private string path = NinjaTrader.Core.Globals.UserDataDir + @"bin\Custom\DrawingTools\notes.txt";
		private StreamReader sr;
		private Style systemMenuStyle = Application.Current.TryFindResource("SystemMenuStyle") as Style;
		private Style mainMenuItem = Application.Current.TryFindResource("MainMenuItem") as Style;
		private Brush textBrush = Application.Current.FindResource("FontActionBrush") as Brush ?? Brushes.Blue;
		private Label atrLabel, lagLabel;
		private double lastPrice;
		private InputSimulator inputSimulator;
		private bool simulatingPanning;
		private TimeZoneInfo marketTimeZone;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name						= "PATS ToolBar";
				Description					= @"Drawing tools horizontal toolbar showing selected tools only";
				IsOverlay					= true;
				IsChartOnly					= true;
				DisplayInDataBox			= false;
				PaintPriceMarkers			= false;
				IsSuspendedWhileInactive	= true;
				EnablePanning				= true;
				EnableZooming				= true;
				SelectedTypes				= new XElement("SelectedTypes");
				LeftMargin					= 32;
				GroupArrows					= true;
				ShowPlotExecutions			= true;
				ShowDrawnObjects			= true;
				ShowRemoveAll				= true;
				ShowATR						= true;
				ATRPeriod					= 21;
				ATRInTicks					= false;
				ShowClock					= true;
				ShowMarketTimeZone			= false;
				ShowLag						= true;
				LagWarning					= 0.3;
				LagAlert					= 1.0;
				ShowAngledLine				= true;
				ShowChannel					= true;
				ShowRange					= true;
				ShowSupportLine				= true;
				ShowMeasuredMove			= true;
				ShowArrows					= true;
				ShowNote					= true;
				ShowMeasure					= true;
				ShowHelp					= true;
				Annotation					= "";
			}
			else if (State == State.Configure)
			{
				isToolBarAdded = false;
				simulatingPanning = false;
			}
			else if (State == State.DataLoaded)
			{
				Name = "";
				marketTimeZone = Bars.TradingHours.TimeZoneInfo;
				if (ChartControl != null) ChartControl.Dispatcher.InvokeAsync((Action)(() => addToolBar()));
			}
			else if (State == State.Historical)
			{
				myATR = ATR(ATRPeriod);
			}
			else if(State == State.Terminated)
			{
				if (ChartControl != null) ChartControl.Dispatcher.InvokeAsync((Action)(() => removeToolBar()));
			}
		}

		private Button createButton(object tooltip, object icon, Style style, FontFamily fontFamily)
		{
			return new Button() {
				ToolTip = tooltip,
				Content = icon,
				Cursor = Cursors.Arrow,
				Style	= style,
				FontFamily = fontFamily,
				FontSize = 16,
				FontStyle	= FontStyles.Normal,
				VerticalAlignment = VerticalAlignment.Center
			};
		}

		private NTMenuItem createNTMenuItem(object icon, string header, Style style, string toolTip)
		{
			NTMenuItem item = new NTMenuItem(){ VerticalAlignment = VerticalAlignment.Center };
			if (icon != null) item.Icon = icon;
			if (header != null) item.Header = header;
			if (style != null) item.Style = style;
			if (toolTip != null) item.ToolTip = toolTip;
			return item;
		}

		private Label createLabel(string content)
		{
			return new Label() {
				Foreground = textBrush,
				Content = content,
				VerticalAlignment = VerticalAlignment.Center,
				Margin = new Thickness(24, 0, 0, 0)
			};
		}

		private bool addDrawingToolToToolBar(string name, string assembly, Style style, FontFamily fontFamily)
		{
			DrawingTools.DrawingTool dt = Core.Globals.AssemblyRegistry[assembly].CreateInstance(name) as DrawingTools.DrawingTool;
			if (dt == null) return false;
			string dtName = dt.DisplayName;
			if (GroupArrows  && (dtName == "Arrow up" || dtName == "Arrow down")) return true;
			Button b = createButton(dtName, dt.Icon ?? Gui.Tools.Icons.DrawPencil, style, fontFamily);
			b.Click += (sender, args) => { if (ChartControl != null) ChartControl.TryStartDrawing(dt.GetType().FullName); };
			if (dtName == "Arrow Up Green" || dtName == "Arrow Down Green" || dtName == "Arrow Up Blue" || dtName == "Arrow Down Red") b.Content = getArrowIcon(dtName, arrowIconStyle);
			toolBarItems.Add(b);
			return true;
		}

		private Object getArrowIcon(string name, ArrowIconStyleEnum style)
		{
			Grid icon = new Grid { Height = 16, Width = 16 };
			System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
			Brush b = name == "Arrow Up Blue" ? Brushes.Blue : name == "Arrow Down Red" ? Brushes.Red : Brushes.Green;
			p.Fill = style == ArrowIconStyleEnum.ColorFilled ? b : (style == ArrowIconStyleEnum.Monochrome && !name.Contains("Green")) ? textBrush : Brushes.Transparent;
			p.Stroke = style == ArrowIconStyleEnum.Monochrome ? textBrush : b; 
			p.Data = System.Windows.Media.Geometry.Parse(name.Contains("Down") ? "M 5.5 2 L 5.5 8 L 2 8 L 7.5 14 L 13 8 L 9.5 8 L 9.5 2 Z" : "M 7.5 2 L 2 8 L 5.5 8 L 5.5 14 L 9.5 14 L 9.5 8 L 13 8 Z");
			icon.Children.Add(p);
			return icon;
		}

		private void addRemoveSelectedDrawingTool(bool add, string name)
		{
			string toolName = "NinjaTrader.NinjaScript.DrawingTools." + name;
			XElement e = SelectedTypes.Element(toolName);
			if (add && e == null)
			{
				XElement el	= new XElement(toolName);
				el.Add(new XAttribute("Assembly", assembly));
				SelectedTypes.Add(el);
			}
			if (!add && e != null) e.Remove();
		}

		private Grid getToggleDrawingsIcon(bool hide)
		{
			Grid icon = new Grid { Height = 16, Width = 16 };
			System.Windows.Shapes.Path p1 = new System.Windows.Shapes.Path();
			System.Windows.Shapes.Path p2 = new System.Windows.Shapes.Path();
			System.Windows.Shapes.Path p3 = new System.Windows.Shapes.Path();
			p1.Fill = p2.Fill = p3.Fill = textBrush;
			p1.Data = System.Windows.Media.Geometry.Parse("m7.873 1.968c-3.519 0-6.672 2.309-7.847 5.746-.035.103-.035.215 0 .318 1.174 3.437 4.328 5.746 7.847 5.746s6.672-2.309 7.847-5.746c.035-.103.035-.215 0-.318-1.174-3.437-4.328-5.746-7.847-5.746zm0 10.826c-3.044 0-5.78-1.971-6.859-4.921 1.08-2.95 3.816-4.921 6.859-4.921 3.044 0 5.78 1.971 6.859 4.921-1.08 2.95-3.816 4.921-6.859 4.921z");
			p2.Data = System.Windows.Media.Geometry.Parse("m7.873 5.275c-1.433 0-2.599 1.166-2.599 2.599 0 1.433 1.166 2.599 2.599 2.599 1.433 0 2.599-1.166 2.599-2.599 0-1.433-1.166-2.599-2.599-2.599z");
			p3.Data = System.Windows.Media.Geometry.Parse("M 0 15 L 15 1 L 16 1 L 1 15 Z");
			icon.Children.Add(p1);
			icon.Children.Add(p2);
			if (hide) icon.Children.Add(p3);
			return icon;
		}

		private Grid getPanningIcon(bool enabled)
		{
			Grid icon = new Grid { Height = 16, Width = 16 };
			System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
			p.Fill = enabled ? Brushes.MediumSeaGreen : textBrush;
			p.Data = System.Windows.Media.Geometry.Parse("M 5 3 L 8 0 L 11 3 Z M 13 5 L 16 8 L 13 11 Z M 11 13 L 8 16 L 5 13 Z M 3 5 L 0 8 L 3 11 Z M 7.5 3 L 8.5 3 L 8.5 13 L 7.5 13 Z M 3 7.5 L 13 7.5 L 13 8.5 L 3 8.5 Z");
			icon.Children.Add(p);
			return icon;
		}

		private void drawNote(string note)
		{
			Annotation = note;
			DrawingTools.DrawingTool dt = Core.Globals.AssemblyRegistry[assembly].CreateInstance("NinjaTrader.NinjaScript.DrawingTools.Note") as DrawingTools.DrawingTool;
			if (dt == null || ChartControl == null) return;
			ChartControl.TryStartDrawing(dt.GetType().FullName);
		}

		protected void MouseRightButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (simulatingPanning)
			{
				inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
				inputSimulator.Mouse.LeftButtonUp();
				simulatingPanning = false;
				e.Handled = true;
			}
		}
		protected void MouseMove(object sender, MouseEventArgs e)
		{
			if (!simulatingPanning && Mouse.RightButton == MouseButtonState.Pressed)
			{
				simulatingPanning = true;
				inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
				inputSimulator.Mouse.LeftButtonDown();
			}
		}

		protected void MouseLeave(object sender, MouseEventArgs e)
		{
			if (simulatingPanning)
			{
				inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
				inputSimulator.Mouse.LeftButtonUp();
				simulatingPanning = false;
			}
		}

		protected void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) return;
			inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, e.Delta > 0 ? VirtualKeyCode.DOWN : VirtualKeyCode.UP);
			e.Handled = true;
		}

		private void addToolBar()
		{
			if (isToolBarAdded) return;
			cw = Window.GetWindow(ChartControl.Parent) as Chart; if (cw == null) return;
			toolBarItems = new List<object>();
			FontFamily fontFamily = Application.Current.Resources["IconsFamily"] as FontFamily;
			Style style = Application.Current.Resources["LinkButtonStyle"] as Style;

			if (EnablePanning || EnableZooming) inputSimulator = new InputSimulator();
			if (EnablePanning)
			{
				ChartControl.MouseRightButtonUp += MouseRightButtonUp;
				ChartControl.MouseMove += MouseMove;
				ChartControl.MouseLeave += MouseLeave;
			}
			if (EnableZooming) ChartControl.PreviewMouseWheel += PreviewMouseWheel;

			#region drawing tools
			addRemoveSelectedDrawingTool(ShowAngledLine, "AngledLine");
			addRemoveSelectedDrawingTool(ShowChannel, "Channel");
			addRemoveSelectedDrawingTool(ShowSupportLine, "SupportLine");
			addRemoveSelectedDrawingTool(ShowRange, "Range");
			addRemoveSelectedDrawingTool(ShowMeasuredMove, "MeasuredMove");
			addRemoveSelectedDrawingTool(ShowMeasure, "Measure");
			addRemoveSelectedDrawingTool(ShowArrows && !GroupArrows, "ArrowUpGreen");
			addRemoveSelectedDrawingTool(ShowArrows && !GroupArrows, "ArrowDownGreen");
			addRemoveSelectedDrawingTool(ShowArrows && !GroupArrows, "ArrowUpBlue");
			addRemoveSelectedDrawingTool(ShowArrows && !GroupArrows, "ArrowDownRed");

			List<XElement> elements = new List<XElement>();
			foreach (XElement element in XElement.Parse(SelectedTypes.ToString()).Elements()) elements.Add(element);
			int count = 0;

			Button empty = createButton("", null, style, fontFamily);
			empty.Margin = new Thickness(LeftMargin, 0, 0, 0);
			toolBarItems.Add(empty);
			
			while (count < elements.Count)
			{
				for (int j = 0; count < elements.Count; j++)
				{
					XElement element = elements[count];
					try
					{
						if (addDrawingToolToToolBar(element.Name.ToString(), element.Attribute("Assembly").Value, style, fontFamily)) count++;
						else { elements.RemoveAt(j); j--; }
					}
					catch (Exception e)
					{
						elements.RemoveAt(j);
						j--;
						Cbi.Log.Process(typeof(Custom.Resource), "NinjaScriptTileError", new object[] { element.Name.ToString(), e }, LogLevel.Error, LogCategories.NinjaScript);
					}
				}
			}
			#endregion

			if (ShowArrows && GroupArrows)
			{
				Menu arrowsMenu = new Menu { VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Style = systemMenuStyle };
				NTMenuItem arrowsMenuItem = createNTMenuItem(getArrowIcon("Arrow Up Green", ArrowIconStyleEnum.Monochrome), null, mainMenuItem, "Arrows");
				arrowsMenuItem.Margin = new Thickness(-4, 0, 0, 0);

				List<string> arrows = new List<string>(){ "ArrowUpGreen", "ArrowDownGreen", "ArrowUpBlue", "ArrowDownRed"};
				string dtprefix = "NinjaTrader.NinjaScript.DrawingTools.";
				if (elements.Exists(x => x.Name.ToString() == dtprefix + "ArrowUp")) arrows.Add("ArrowUp");
				if (elements.Exists(x => x.Name.ToString() == dtprefix + "ArrowDown")) arrows.Add("ArrowDown");
				foreach (string s in arrows)
				{
					DrawingTools.DrawingTool dt = Core.Globals.AssemblyRegistry[assembly].CreateInstance(dtprefix + s) as DrawingTools.DrawingTool;
					if (dt == null) break;
					string dtName = dt.DisplayName;
					NTMenuItem arrowItem = createNTMenuItem(dt.Icon, dtName, mainMenuItem, null);
					if (dtName == "Arrow Up Blue" || dtName == "Arrow Down Red" || dtName == "Arrow Up Green" || dtName == "Arrow Down Green") arrowItem.Icon = getArrowIcon(dtName, arrowIconStyle);
					arrowItem.Click += (sender, args) => { if (ChartControl != null) ChartControl.TryStartDrawing(dt.GetType().FullName); };
					arrowsMenuItem.Items.Add(arrowItem);
				}
				arrowsMenu.Items.Add(arrowsMenuItem);
				toolBarItems.Add(arrowsMenu);
			}

			if (ShowNote)
			{
				Menu theMenu = new Menu { VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Bottom, Style = systemMenuStyle };
				NTMenuItem topMenuItem = createNTMenuItem("𝐓", null, mainMenuItem, "Note");
				topMenuItem.Margin = new Thickness(-4, 0, 0, 0);
				NTMenuItem textItem = createNTMenuItem(NinjaTrader.Gui.Tools.Icons.DrawText, "Note", mainMenuItem, null);
				textItem.Click += (o, args) => drawNote("");
				topMenuItem.Items.Add(textItem);
				if (!File.Exists(path))
				{
					foreach (string s in new[] { "2EL", "2ES", "F2EL", "F2ES"})
					{
						NTMenuItem subItem = createNTMenuItem(null, s, mainMenuItem, null);
						subItem.Click += (o, args) => drawNote(s);
						topMenuItem.Items.Add(subItem);
					}
				}
				else
				{
					try
					{
						sr = new System.IO.StreamReader(path);
						string line;
						while ((line = sr.ReadLine()) != null) 
						{
							string trimmed = line.Trim();
							if (trimmed.Length == 0) continue;
							NTMenuItem subItem = createNTMenuItem(null, trimmed, mainMenuItem, null);
							subItem.Click += (o, args) => drawNote(trimmed);
							topMenuItem.Items.Add(subItem);
						}
						sr.Dispose(); sr = null;
					}
					catch (Exception e)
					{
						Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
					}
				}
				theMenu.Items.Add(topMenuItem);
				toolBarItems.Add(theMenu);
			}

			if (ShowPlotExecutions)
			{
				Grid icon = new Grid { Height = 16, Width = 16 };
				System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
				p.Fill = textBrush;
				p.Data = System.Windows.Media.Geometry.Parse("M 6 1 L 6 15 L 7 15 L 7 1 Z M 6 1 L 10 1 L 10 2 L 6 2 Z M 10 1 L 10 15 L 9 15 L 9 1 Z M 6 15 L 10 15 L 10 14 L 6 14 Z M 2 8 L 5 11 L 2 14 Z M 14 2 L 11 5 L 14 8 Z");
				icon.Children.Add(p);
				Button b = createButton("Plot Executions", icon, style, fontFamily);
				b.Click += (o, args) => System.Windows.Forms.SendKeys.SendWait("^e");
				toolBarItems.Add(b);
			}

			if (ShowDrawnObjects)
			{
				Button b = createButton("Hide Drawings", getToggleDrawingsIcon(true), style, fontFamily);
				b.Click += (o, args) => {
					Button bb = o as Button;
					bb.ToolTip = bb.ToolTip == "Hide Drawings" ? "Show Drawings" : "Hide Drawings";
					bb.Content = getToggleDrawingsIcon(bb.ToolTip == "Hide Drawings");
					foreach (var obj in cw.ActiveChartControl.ChartObjects)
					{
						var draw = obj as NinjaTrader.NinjaScript.DrawingTools.DrawingTool;
						if (draw == null || !draw.IsUserDrawn) continue;
						draw.IsVisible = !draw.IsVisible;
					}
					ForceRefresh();
				};
				toolBarItems.Add(b);
			}

			if (ShowRemoveAll)
			{
				NTMenuItem ntDrawingTools = ((Menu)cw.MainMenu[3]).Items[0] as NTMenuItem;
				foreach (var item in ntDrawingTools.Items)
				{
					if (item is NTMenuItem && ((NTMenuItem)item).Header != null)
					{
						NTMenuItem mnItem = item as NTMenuItem;
						if (mnItem.Header.ToString() == "Remove All Drawing Objects")
						{
							Button b = createButton(mnItem.Header, mnItem.Icon, style, fontFamily);
							b.Click += (o, args) => mnItem.Command.Execute(o);
							toolBarItems.Add(b);
						}
					}
				}
			}

			if (ShowHelp)
			{
				Menu helpMenu = new Menu { VerticalAlignment = VerticalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Bottom, Style = systemMenuStyle };
				NTMenuItem helpMenuItem = createNTMenuItem("?", null, mainMenuItem, "Help");
				helpMenuItem.Margin = new Thickness(-4, 0, 0, 0);
				NTMenuItem helpLinkItem = createNTMenuItem(null, "Help", mainMenuItem, null);
				NTMenuItem tipLinkItem = createNTMenuItem(null, "Tip Me ♡", mainMenuItem, null);
				NTMenuItem aboutItem = createNTMenuItem(null, "About", mainMenuItem, null);
				helpMenuItem.Items.Add(helpLinkItem);
				helpMenuItem.Items.Add(tipLinkItem);
				helpMenuItem.Items.Add(aboutItem);
				helpMenu.Items.Add(helpMenuItem);
				tipLinkItem.Click += ((o, args) => {
					try { System.Diagnostics.Process.Start("https://paypal.me/faurebastien"); }
					catch (System.Exception e) { MessageBox.Show(e.Message); }
				});
				helpLinkItem.Click += ((o, args) => {
					try { System.Diagnostics.Process.Start("https://www.notion.so/bastienfaure/PATS-ToolBar-57fcbaa98c46477ba7bcd801f4356e60"); }
					catch (System.Exception e) { MessageBox.Show(e.Message); }
				});
				aboutItem.Click += (o, args) => Core.Globals.RandomDispatcher.BeginInvoke(new Action(() => new AboutWindow().Show()));
				toolBarItems.Add(helpMenu);
			}

			if (ShowATR) { atrLabel = createLabel("ATR 0.00"); toolBarItems.Add(atrLabel); }
			if (ShowLag) { lagLabel = createLabel("LAG 0.000"); toolBarItems.Add(lagLabel); }

			if (ShowClock)
			{
				myTimer = new System.Windows.Forms.Timer();
				myTimer.Interval = 1000;
				myTimer.Enabled = true;
				Label clockLabel = createLabel(ShowMarketTimeZone ? TimeZoneInfo.ConvertTime(Core.Globals.Now, marketTimeZone).ToString() : Core.Globals.Now.ToString());
				toolBarItems.Add(clockLabel);
				myTimer.Tick += new EventHandler((o, args) => clockLabel.Content = ShowMarketTimeZone ? TimeZoneInfo.ConvertTime(Core.Globals.Now, marketTimeZone).ToString() : Core.Globals.Now.ToString());
			}

			ShowHideToolBar(TabSelected());
			cw.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private bool TabSelected()
		{
			return (ChartControl != null && ChartControl.ChartTab == ((cw.MainTabControl.Items.GetItemAt(cw.MainTabControl.SelectedIndex) as TabItem).Content as ChartTab));
		}

		private void ShowHideToolBar(bool show)
		{
			if (show && !isToolBarAdded)
			{
				foreach (object toolBarItem in toolBarItems) cw.MainMenu.Add(toolBarItem);
				isToolBarAdded = true;
			}
			else if (!show && isToolBarAdded)
			{
				foreach (object toolBarItem in toolBarItems) cw.MainMenu.Remove(toolBarItem);
				isToolBarAdded = false;
			}
		}

		protected void removeToolBar()
		{
			if (myTimer != null) { myTimer.Stop(); myTimer.Dispose(); }
			if (sr != null) { sr.Dispose(); sr = null; }
			ChartControl.MouseRightButtonUp -= MouseRightButtonUp;
			ChartControl.MouseMove -= MouseMove;
			ChartControl.MouseLeave -= MouseLeave;
			ChartControl.PreviewMouseWheel -= PreviewMouseWheel;
			if (cw == null) return;
			cw.MainTabControl.SelectionChanged -= TabChangedHandler;
			ShowHideToolBar(false);
		}

		private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0) return;
			TabItem tabItem = e.AddedItems[0] as TabItem; if (tabItem == null) return;
			ChartTab temp = tabItem.Content as ChartTab; if (temp == null) return;
			ShowHideToolBar(TabSelected());
		}

		private void updateATR (string atrValue) { ChartControl.Dispatcher.InvokeAsync((Action)(() => atrLabel.Content = "ATR " + atrValue)); }

		private void updateLAG (double price, double lag)
        {
			ChartControl.Dispatcher.InvokeAsync((Action)(() => {
				lagLabel.Content = "LAG " + lag.ToString("0.000");
				lagLabel.Foreground = (lag < -LagAlert) ? Brushes.Red : (lag < -LagWarning) ? Brushes.DarkOrange : textBrush;
			}));
			lastPrice = price;
		}

		protected override void OnBarUpdate() { if (ShowATR) updateATR(Math.Round(myATR[0] / (ATRInTicks ? TickSize : 1), 2).ToString()); }
		protected override void OnMarketData(MarketDataEventArgs e) { if (ShowLag && e.MarketDataType == MarketDataType.Last && lastPrice != e.Price) updateLAG(e.Price, (e.Time - Core.Globals.Now).TotalSeconds); }

		public override string DisplayName { get { return "PATS ToolBar"; } }

		#region Properties
		[Browsable(false)]
		public string Annotation { get; set; }

		public XElement SelectedTypes { get; set; }

		[Range(1, 1000)]
		[Display(Name = "Left Margin", Description = "Empty space before first drawing button", GroupName = "0. General Settings", Order = 0)]
		public int LeftMargin { get; set; }

		[Display(Name = "Arrows Icons Style", Description = "Color Filled, Color Outlined or Monochrome arrows icons", GroupName = "0. General Settings", Order = 1)]
		public ArrowIconStyleEnum ArrowIconStyle { get { return arrowIconStyle; } set { arrowIconStyle = value; } }

		[Display(Name = "Arrows Dropdown Menu", Description = "Group all arrows into dropdown menu", GroupName = "0. General Settings", Order = 2)]
		public bool GroupArrows { get; set; }

		[Display(Name = "Right Click Panning", Description = "Right Click Panning", GroupName = "0. General Settings", Order = 3)]
		public bool EnablePanning { get; set; }

		[Display(Name = "Scroll Wheel Zoom", Description = "Scroll Wheel Zoom", GroupName = "0. General Settings", Order = 4)]
		public bool EnableZooming { get; set; }

		[Display(Name = "Plot Executions", Description = "Toggle plot executions between none, markers only and markers + text", GroupName = "1. Buttons", Order = 1)]
		public bool ShowPlotExecutions { get; set; }

		[Display(Name = "Hide Drawings", Description = "Toggle between hide and show all drawings", GroupName = "1. Buttons", Order = 2)]
		public bool ShowDrawnObjects { get; set; }

		[Display(Name = "Remove All Drawing Objects", Description = "Remove all drawing objects", GroupName = "1. Buttons", Order = 3)]
		public bool ShowRemoveAll { get; set; }

		[Display(Name = "Help", Description = "Show help menu", GroupName = "1. Buttons", Order = 4)]
		public bool ShowHelp { get; set; }

		[RefreshProperties(RefreshProperties.All)]
		[Display(Name = "ATR", Description = "Display ATR value", GroupName = "2. Labels", Order = 1)]
		public bool ShowATR { get; set; }

		[Range(1, 1000)]
		[Display(Name = "ATR Period", Description = "Set ATR period", GroupName = "2. Labels", Order = 2)]
		public int ATRPeriod { get; set; }

		[Display(Name = "ATR Value in Ticks", Description = "Display ATR value in ticks", GroupName = "2. Labels", Order = 3)]
		public bool ATRInTicks { get; set; }

		[Display(Name = "Lag", Description = "Display chart lag", GroupName = "2. Labels", Order = 4)]
		public bool ShowLag { get; set; }

	    [Range(0, 100)]
	    [Display(Name = "Lag Warning Threshold", Description = "Lag warning threshold in seconds", GroupName = "2. Labels", Order = 5)]
	    public double LagWarning
	    { get; set; }

	    [Range(0, 100)]
	    [Display(Name = "Lag Alert Threshold", Description = "Lag alert threshold in seconds", GroupName = "2. Labels", Order = 6)]
	    public double LagAlert
	    { get; set; }

		[Display(Name = "Clock", Description = "Display date and time", GroupName = "2. Labels", Order = 7)]
		public bool ShowClock { get; set; }

		[Display(Name = "Clock With Market Time Zone", Description = "Display date and time using market trading hours time zone", GroupName = "2. Labels", Order = 8)]
		public bool ShowMarketTimeZone { get; set; }

		[Display(Name = "Angled Line", Description = "PATS angled line drawing tool", GroupName = "3. PATS Drawing Tools", Order = 0)]
		public bool ShowAngledLine { get; set; }

		[Display(Name = "Channel", Description = "PATS trend channel drawing tool", GroupName = "3. PATS Drawing Tools", Order = 1)]
		public bool ShowChannel { get; set; }

		[Display(Name = "Support Line", Description = "PATS support line drawing tool", GroupName = "3. PATS Drawing Tools", Order = 2)]
		public bool ShowSupportLine { get; set; }

		[Display(Name = "Range", Description = "PATS trading range drawing tool", GroupName = "3. PATS Drawing Tools", Order = 3)]
		public bool ShowRange { get; set; }

		[Display(Name = "Measured Move", Description = "PATS measured move drawing tool", GroupName = "3. PATS Drawing Tools", Order = 4)]
		public bool ShowMeasuredMove { get; set; }

		[Display(Name = "Measure", Description = "PATS measure drawing tool", GroupName = "3. PATS Drawing Tools", Order = 5)]
		public bool ShowMeasure { get; set; }

		[Display(Name = "Arrows", Description = "PATS arrows drawing tool", GroupName = "3. PATS Drawing Tools", Order = 6)]
		public bool ShowArrows { get; set; }

		[Display(Name = "Note", Description = "PATS note drawing tool", GroupName = "3. PATS Drawing Tools", Order = 7)]
		public bool ShowNote { get; set; }
		#endregion
	}

	public class CustomDrawingToolPropertyDescriptor : PropertyDescriptor
	{
		private readonly string		displayName;
		private readonly string		name;
		private readonly int		order;
		private readonly Type		type;

		public override AttributeCollection Attributes
		{
			get
			{
				Attribute[] attr	= new Attribute[1];
				attr[0]				= new DisplayAttribute { Name = DisplayName, GroupName = "4. Standard Drawing Tools", Order = order };
				return new AttributeCollection(attr);
			}
		}

		public CustomDrawingToolPropertyDescriptor(Type type, string displayName, int order) : base(type.FullName, null)
		{
			name					= type.FullName;
			this.displayName		= displayName;
			this.order				= order;
			this.type				= type;
		}

		public	override	Type	ComponentType							{ get { return typeof (PATSToolBar); } }
		public	override	string	DisplayName								{ get { return displayName; } }
		public	override	bool	IsReadOnly								{ get { return false; } }
		public	override	string	Name									{ get { return name; } }
		public	override	Type	PropertyType							{ get { return typeof (bool); } }

		public	override	bool	CanResetValue(object component)			{ return true; }
		public	override	bool	ShouldSerializeValue(object component)	{ return true; }

		public	override	object	GetValue(object component)
		{
			PATSToolBar c = component as PATSToolBar;
			return c != null && c.SelectedTypes.Element(Name) != null;
		}

		public override void ResetValue(object component) {}

		public override void SetValue(object component, object value)
		{
			PATSToolBar c = component as PATSToolBar;
			if (c == null) return;
			bool val = (bool) value;
			if (val && c.SelectedTypes.Element(Name) == null)
			{
				XElement toAdd = new XElement(Name);
				toAdd.Add(new XAttribute("Assembly", Core.Globals.AssemblyRegistry.IsNinjaTraderCustomAssembly(type) ? "NinjaTrader.Custom" : type.Assembly.GetName().Name));
				c.SelectedTypes.Add(toAdd);
			}
			else if(!val && c.SelectedTypes.Element(Name) != null) c.SelectedTypes.Element(Name).Remove();
		}
	}

	public class CustomDrawingToolIndicatorTypeConverter : TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			TypeConverter tc = component is IndicatorBase ? TypeDescriptor.GetConverter(typeof(IndicatorBase)) : TypeDescriptor.GetConverter(typeof(DrawingTools.DrawingTool));
			PropertyDescriptorCollection propertyDescriptorCollection = tc.GetProperties(context, component, attrs);
			if (propertyDescriptorCollection == null) return null;
			PropertyDescriptorCollection properties	= new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor pd in propertyDescriptorCollection)
			{
				if (!pd.IsBrowsable || pd.IsReadOnly || pd.Name == "IsAutoScale" || pd.Name == "DisplayInDataBox" || pd.Name == "MaximumBarsLookBack" || pd.Name == "Calculate"
					|| pd.Name == "Panel" || pd.Name == "IsVisible" || pd.Name == "PaintPriceMarkers" || pd.Name == "Displacement" || pd.Name == "ScaleJustification" || pd.Name == "Name")
					continue;

				if (pd.Name == "SelectedTypes")
				{
					int i = 1;
					foreach (Type type in Core.Globals.AssemblyRegistry.GetDerivedTypes(typeof(DrawingTools.DrawingTool)))
					{
						DrawingTools.DrawingTool tool = type.Assembly.CreateInstance(type.FullName) as DrawingTools.DrawingTool;
						if (tool == null || !tool.DisplayOnChartsMenus) continue;
						CustomDrawingToolPropertyDescriptor descriptor = new CustomDrawingToolPropertyDescriptor(type, tool.Name, i);
						properties.Add(descriptor);
						i++;
					}
					continue;
				}
				properties.Add(pd);
			}
			return properties;
		}
	}
}

public class AboutWindow : NTWindow
{
	public AboutWindow()
	{
		Caption = "About PATS ToolBar";
		Width = 400;
		Height = 200;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
		ScrollViewer viewer = new ScrollViewer();
		viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
		Brush textBrush = Application.Current.FindResource("FontActionBrush") as Brush ?? Brushes.Blue;
		TextBlock textBlock = new TextBlock(); textBlock.Text = "PATS ToolBar Indicator version 03/03/2021 made with ♡ by beo";
		TextBlock linkForum = new TextBlock(); linkForum.Text = "Download latest version and post any questions/suggestions here";
		linkForum.MouseLeftButtonDown += ((o, args) => {
			try { System.Diagnostics.Process.Start("https://priceactiontradingsystem.com/link-to-forum/topic/pats-toolbar-custom-drawing-tools-why-this-is-better/"); }
			catch (System.Exception e) { MessageBox.Show(e.Message); }
		});
		TextBlock tip = new TextBlock(); tip.Text = "If you like my work you can tip me here";
		linkForum.Cursor = tip.Cursor = Cursors.Hand;
		linkForum.TextDecorations = tip.TextDecorations = System.Windows.TextDecorations.Underline;
		tip.MouseLeftButtonDown  += ((o, args) => {
			try { System.Diagnostics.Process.Start("https://paypal.me/faurebastien"); }
			catch (System.Exception e) { MessageBox.Show(e.Message); }
		});
		TextBlock thumb = new TextBlock(); thumb.Text = "👍"; thumb.Margin = new Thickness(8, 0, 0, 0);
		StackPanel tipPanel = new StackPanel() { Orientation = Orientation.Horizontal };
		tipPanel.Children.Add(tip);
		tipPanel.Children.Add(thumb);
		TextBlock tippers = new TextBlock(); tippers.Text = "I want to say thank you to all tippers:\nBen, George, Scott, Alex, Meridee, Michael, Nathan, Suzu,\nMaria, Jeff and Jacob";
		textBlock.Foreground = linkForum.Foreground = tip.Foreground = thumb.Foreground = tippers.Foreground = textBrush;
		StackPanel myStackPanel = new StackPanel() { Orientation = Orientation.Vertical };
		myStackPanel.Children.Add(textBlock);
		myStackPanel.Children.Add(new TextBlock());
		myStackPanel.Children.Add(linkForum);
		myStackPanel.Children.Add(new TextBlock());
		myStackPanel.Children.Add(tipPanel);
		myStackPanel.Children.Add(new TextBlock());
		myStackPanel.Children.Add(tippers);
		Content = myStackPanel;
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PATSToolBar[] cachePATSToolBar;
		public PATSToolBar PATSToolBar()
		{
			return PATSToolBar(Input);
		}

		public PATSToolBar PATSToolBar(ISeries<double> input)
		{
			if (cachePATSToolBar != null)
				for (int idx = 0; idx < cachePATSToolBar.Length; idx++)
					if (cachePATSToolBar[idx] != null &&  cachePATSToolBar[idx].EqualsInput(input))
						return cachePATSToolBar[idx];
			return CacheIndicator<PATSToolBar>(new PATSToolBar(), input, ref cachePATSToolBar);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PATSToolBar PATSToolBar()
		{
			return indicator.PATSToolBar(Input);
		}

		public Indicators.PATSToolBar PATSToolBar(ISeries<double> input )
		{
			return indicator.PATSToolBar(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PATSToolBar PATSToolBar()
		{
			return indicator.PATSToolBar(Input);
		}

		public Indicators.PATSToolBar PATSToolBar(ISeries<double> input )
		{
			return indicator.PATSToolBar(input);
		}
	}
}

#endregion
