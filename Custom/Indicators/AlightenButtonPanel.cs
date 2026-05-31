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
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AlightenButtonPanel : Indicator
	{
		
		private ChartTrader chartTrader;
		private System.Windows.Controls.Grid chartTraderGrid;
		private System.Windows.Controls.Grid chartTraderButtonsGrid;
		private System.Windows.Controls.RowDefinition addedRow;
		private System.Windows.Controls.Grid panelGrid;
		
		private NinjaTrader.Gui.Tools.AccountSelector    xAcSelector;
		private NinjaTrader.Gui.Tools.InstrumentSelector xInSelector;
	
	    private Button btnBE, btnBEPlus, btnBracket, btnAddStop, btnHalf, btnDouble;
		
		private double lastClose = 0;
		private string lastOcoId;
		private double lastBracketTargetBase;
		private bool   lastBracketIsLong;
		private int    lastBracketQty;
	
	    [NinjaScriptProperty]
	    [Display(Name = "Breakeven1 + X Ticks", Order = 0, GroupName = "Parameters")]
	    public int Breakeven1PlusTicks { get; set; } = 3;
		
		[NinjaScriptProperty]
	    [Display(Name = "Breakeven2 + X Ticks", Order = 0, GroupName = "Parameters")]
	    public int Breakeven2PlusTicks { get; set; } = 6;
	
	    [NinjaScriptProperty]
	    [Display(Name = "Bracket Stop (Ticks)", Order = 1, GroupName = "Parameters")]
	    public int BracketStopTicks { get; set; } = 40;
	
	    [NinjaScriptProperty]
	    [Display(Name = "Bracket Profit (Ticks)", Order = 2, GroupName = "Parameters")]
	    public int BracketProfitTicks { get; set; } = 40;
		
		#region Button Color Properties

		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button1 Color", GroupName = "Button Color Settings", Order = 1)]
		public Brush Button1Color { get; set; } = Brushes.Blue;
		[Browsable(false)]
		public string Button1ColorSerialize
		{
		    get => Serialize.BrushToString(Button1Color);
		    set => Button1Color = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button2 Color", GroupName = "Button Color Settings", Order = 2)]
		public Brush Button2Color { get; set; } = Brushes.Blue;
		[Browsable(false)]
		public string Button2ColorSerialize
		{
		    get => Serialize.BrushToString(Button2Color);
		    set => Button2Color = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button3 Color", GroupName = "Button Color Settings", Order = 3)]
		public Brush Button3Color { get; set; } = Brushes.Blue;
		[Browsable(false)]
		public string Button3ColorSerialize
		{
		    get => Serialize.BrushToString(Button3Color);
		    set => Button3Color = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button4 Color", GroupName = "Button Color Settings", Order = 4)]
		public Brush Button4Color { get; set; } = Brushes.Blue;
		[Browsable(false)]
		public string Button4ColorSerialize
		{
		    get => Serialize.BrushToString(Button4Color);
		    set => Button4Color = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button5 Color", GroupName = "Button Color Settings", Order = 5)]
		public Brush Button5Color { get; set; } = Brushes.DimGray;
		[Browsable(false)]
		public string Button5ColorSerialize
		{
		    get => Serialize.BrushToString(Button5Color);
		    set => Button5Color = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button6 Color", GroupName = "Button Color Settings", Order = 6)]
		public Brush Button6Color { get; set; } = Brushes.DimGray;
		[Browsable(false)]
		public string Button6ColorSerialize
		{
		    get => Serialize.BrushToString(Button6Color);
		    set => Button6Color = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button7 Color", GroupName = "Button Color Settings", Order = 7)]
		public Brush Button7Color { get; set; } = Brushes.DimGray;
		[Browsable(false)]
		public string Button7ColorSerialize
		{
		    get => Serialize.BrushToString(Button7Color);
		    set => Button7Color = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "Button8 Color", GroupName = "Button Color Settings", Order = 8)]
		public Brush Button8Color { get; set; } = Brushes.DimGray;
		[Browsable(false)]
		public string Button8ColorSerialize
		{
		    get => Serialize.BrushToString(Button8Color);
		    set => Button8Color = Serialize.StringToBrush(value);
		}
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AlightenButtonPanelB1";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
	        {
	            // Defer UI creation until chart is ready
	            ChartControl.Dispatcher.BeginInvoke(new Action(CreateWPFControls));
				
				
	        }
	        else if (State == State.Terminated)
	        {
	            // Clean up when indicator is removed
	            ChartControl?.Dispatcher.BeginInvoke(new Action(RemoveWPFControls));
				
	        }
		}

		private void CreateWPFControls()
		{
		    // 1) grab the ChartTrader
		    chartTrader = ChartControl.OwnerChart.ChartTrader;
		    if (chartTrader == null || panelGrid != null)
		        return;
		
		    // 2) find its root grid and the built-in buttons grid
		    chartTraderGrid = chartTrader.Content as Grid;
		    if (chartTraderGrid == null) return;
		    chartTraderButtonsGrid = chartTraderGrid.Children[0] as Grid;
		    if (chartTraderButtonsGrid == null) return;
		
		    // 3) build our 4×2 panel
		    panelGrid = new Grid { Margin = new Thickness(4) };
		    // four rows
		    for (int row = 0; row < 4; row++)
		        panelGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
		    // two columns
		    for (int col = 0; col < 2; col++)
		        panelGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		
		    // 4) create each button, passing in its configured Brush
		    var bePlus1 = CreateButton(
		        $"BE + {Breakeven1PlusTicks}", 
		        Button1Color,    // Blue by default
		        BtnBEPlus1_Click
		    );
		    var bePlus2 = CreateButton(
		        $"BE + {Breakeven2PlusTicks}", 
		        Button2Color,    // Blue
		        BtnBEPlus2_Click
		    );
		    var bracket = CreateButton(
		        "Bracket", 
		        Button3Color,    // Blue
		        BtnBracket_Click
		    );
		    var addStop = CreateButton(
		        "Add Stop", 
		        Button4Color,    // Blue
		        BtnAddStop_Click
		    );
		    var half    = CreateButton("Half",   Button5Color, BtnHalf_Click);   // DimGray
		    var dbl     = CreateButton("Double", Button6Color, BtnDouble_Click); // DimGray
		    var naked   = CreateButton("Naked",  Button7Color, BtnNaked_Click);  // DimGray
		    var split   = CreateButton("Split",  Button8Color, BtnSplit_Click);  // DimGray
		
		    // 5) position them in the 4×2 grid
		    Grid.SetRow(bePlus1, 0); Grid.SetColumn(bePlus1, 0);
		    Grid.SetRow(bePlus2, 0); Grid.SetColumn(bePlus2, 1);
		    Grid.SetRow(bracket, 1); Grid.SetColumn(bracket, 0);
		    Grid.SetRow(addStop, 1); Grid.SetColumn(addStop, 1);
		    Grid.SetRow(half,    2); Grid.SetColumn(half,    0);
		    Grid.SetRow(dbl,     2); Grid.SetColumn(dbl,     1);
		    Grid.SetRow(naked,   3); Grid.SetColumn(naked,   0);
		    Grid.SetRow(split,   3); Grid.SetColumn(split,   1);
		
		    // 6) add to our panel
		    panelGrid.Children.Add(bePlus1);
		    panelGrid.Children.Add(bePlus2);
		    panelGrid.Children.Add(bracket);
		    panelGrid.Children.Add(addStop);
		    panelGrid.Children.Add(half);
		    panelGrid.Children.Add(dbl);
		    panelGrid.Children.Add(naked);
		    panelGrid.Children.Add(split);
		
		    // 7) inject into the ChartTrader buttons grid
		    addedRow = new RowDefinition { Height = GridLength.Auto };
		    chartTraderButtonsGrid.RowDefinitions.Add(addedRow);
		    Grid.SetRow(panelGrid, chartTraderButtonsGrid.RowDefinitions.Count - 1);
		    Grid.SetColumnSpan(panelGrid, chartTraderButtonsGrid.ColumnDefinitions.Count);
		    chartTraderButtonsGrid.Children.Add(panelGrid);
		}
		
		private Button CreateButton(string text, Brush background, RoutedEventHandler handler)
		{
		    var b = new Button
		    {
		        Content    = text,
		        Background = background,
		        Foreground = Brushes.White,           // white text
		        FontSize   = 14,                      // larger font
		        FontWeight = FontWeights.Normal,
		        Height     = 30,                      // taller
		        MinWidth   = 80,                      // ensure wide enough
		        Margin     = new Thickness(2),
		        Padding    = new Thickness(4,2,4,2),
		        Style      = Application.Current.TryFindResource("Button") as Style
		    };
		    b.Click += handler;
		    return b;
		}


		private void RemoveWPFControls()
		{
		    if (chartTraderButtonsGrid == null || panelGrid == null || addedRow == null)
		        return; 
		
		    // remove the grid and its row
		    chartTraderButtonsGrid.Children.Remove(panelGrid);
		    chartTraderButtonsGrid.RowDefinitions.Remove(addedRow);
		
		    // clear references
		    panelGrid              = null;
		    addedRow               = null;
		    chartTraderGrid        = null;
		    chartTraderButtonsGrid = null;
		}

	    private void BtnBEPlus1_Click(object sender, RoutedEventArgs e)
	    {
		    StopsToBreakeven(Breakeven1PlusTicks);
	    }
	
	    private void BtnBEPlus2_Click(object sender, RoutedEventArgs e)
	    {
	        StopsToBreakeven(Breakeven2PlusTicks);
	    }
	
	    private void BtnBracket_Click(object sender, RoutedEventArgs e)
	    {
	        BracketOrder(BracketStopTicks, BracketProfitTicks);
	    }
	
	    private void BtnAddStop_Click(object sender, RoutedEventArgs e)
	    {
	       	AddStopOrder(BracketStopTicks);
	    }
	
	    private void BtnHalf_Click(object sender, RoutedEventArgs e)
		{
		    RemoveHalfPosition();
		}

	    private void BtnDouble_Click(object sender, RoutedEventArgs e)
	    {
	        DoublePosition();
	    }
		
		private void BtnNaked_Click(object sender, RoutedEventArgs e)
		{
		    RemoveStopsAndTargets();
		}
		
		private void BtnSplit_Click(object sender, RoutedEventArgs e)
		{
			SplitStopsAndTargets();
		}
		
		private void SplitStopsAndTargets()
		{
		    // 1) resolve account & instrument (unchanged)
		    xAcSelector = Window
		      .GetWindow(ChartControl.Parent)
		      .FindFirst("ChartTraderControlAccountSelector")
		        as NinjaTrader.Gui.Tools.AccountSelector;
		    if (xAcSelector == null) return;
		    var acct = NinjaTrader.Cbi.Account.All
		        .FirstOrDefault(a => xAcSelector.SelectedAccount.ToString().Contains(a.Name));
		    if (acct == null) return;
		    xInSelector = Window
		      .GetWindow(ChartControl.OwnerChart)
		      .FindFirst("ChartWindowInstrumentSelector")
		        as NinjaTrader.Gui.Tools.InstrumentSelector;
		    if (xInSelector == null) return;
		    var instr = xInSelector.Instrument;
		    if (instr == null) return;
		
		    // 2) pull all stop & limit legs that belong to any OCO
		    var bracketLegs = acct.Orders
		        .Where(o =>
		            o.Instrument.FullName == instr.FullName &&
		            !string.IsNullOrEmpty(o.Oco) &&
		           (o.OrderState == OrderState.Working   || o.OrderState == OrderState.Accepted) &&
		           (o.OrderType == OrderType.Limit       ||
		            o.OrderType == OrderType.StopMarket ||
		            o.OrderType == OrderType.StopLimit))
		        .ToList();
		    if (bracketLegs.Count == 0)
		    {
		        Print("Split ▶ no bracket exits to split");
		        return;
		    }
		
		    // 3) group by OCO to inspect quantities
		    var groups = bracketLegs.GroupBy(o => o.Oco).ToList();
		
		    // 4) handle "multi-ATM" scenario: several 1-contract OCOs
		    bool allSingle = groups.All(g => g.All(o => o.Quantity == 1));
		    if (allSingle && groups.Count > 1)
		    {
		        int totalQty = groups.Count;
		        // base prices from first group
		        var first = groups[0].ToList();
		        var stopLeg  = first.First(o => o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit);
		        var tgtLeg   = first.First(o => o.OrderType == OrderType.Limit);
		        double baseStop   = stopLeg.StopPrice;
		        double baseTarget = tgtLeg.LimitPrice;
		        bool   isLong     = stopLeg.OrderAction == OrderAction.Sell;
		        double tickSz     = instr.MasterInstrument.TickSize;
		
		        // cancel every ATM exit
		        acct.Cancel(bracketLegs.ToArray());
		        Print($"Split ▶ cancelled {bracketLegs.Count} legs from {groups.Count} ATM brackets");
		
		        // rebuild per-contract stop+target stepping out by ticks
		        var newOrders = new List<NinjaTrader.Cbi.Order>(totalQty * 2);
		        for (int i = 0; i < totalQty; i++)
		        {
		            string legOco = Guid.NewGuid().ToString();
		
		            // stop market for 1 contract
		            newOrders.Add(acct.CreateOrder(
		                instr,
		                isLong ? OrderAction.Sell : OrderAction.BuyToCover,
		                OrderType.StopMarket,
		                OrderEntry.Automated,
		                TimeInForce.Gtc,
		                1,
		                0,
		                baseStop,
		                legOco,
		                Name + "_Stop"  + (i+1),
		                Core.Globals.MaxDate,
		                null
		            ));
		
		            // limit target one tick further each time
		            double stepped = isLong
		                ? baseTarget + i * tickSz
		                : baseTarget - i * tickSz;
		
		            newOrders.Add(acct.CreateOrder(
		                instr,
		                isLong ? OrderAction.Sell : OrderAction.BuyToCover,
		                OrderType.Limit,
		                OrderEntry.Automated,
		                TimeInForce.Gtc,
		                1,
		                stepped,
		                0,
		                legOco,
		                Name + "_Split" + (i+1),
		                Core.Globals.MaxDate,
		                null
		            ));
		        }
		
		        acct.Submit(newOrders.ToArray());
		        Print($"Split ▶ created {totalQty} stop+target pairs across combined ATMs");
		        return;
		    }
		
		    // 5) fallback to your existing “aggregated” logic
		    foreach (var group in groups)
		    {
		        var legs = group.ToList();
		        string oco = group.Key;
		
		        bool isAggregated = legs.Any(o => o.Quantity > 1);
		        if (!isAggregated)
		        {
		            Print($"Split ▶ OCO={oco} is already per-contract, skipping");
		            continue;
		        }
		
		        // your original cancel & rebuild logic for multi-contract brackets...
		        // (unchanged)
		        var stopLeg  = legs.First(o => o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit);
		        var limitLeg = legs.First(o => o.OrderType == OrderType.Limit);
		        int    qty             = limitLeg.Quantity;
		        double baseStopPrice   = stopLeg.StopPrice;
		        double baseTargetPrice = limitLeg.LimitPrice;
		        bool   isLong2         = stopLeg.OrderAction == OrderAction.Sell;
		        double tickSz2         = instr.MasterInstrument.TickSize;
		
		        acct.Cancel(legs.ToArray());
		        Print($"Split ▶ cancelled {legs.Count} aggregated legs for OCO={oco}");
		
		        var newOrders = new List<NinjaTrader.Cbi.Order>(qty * 2);
		        for (int i = 0; i < qty; i++)
		        {
		            string legOco2 = Guid.NewGuid().ToString();
		
		            newOrders.Add(acct.CreateOrder(
		                instr,
		                isLong2 ? OrderAction.Sell : OrderAction.BuyToCover,
		                OrderType.StopMarket,
		                OrderEntry.Automated,
		                TimeInForce.Gtc,
		                1,
		                0,
		                baseStopPrice,
		                legOco2,
		                Name + "_Stop"  + (i+1),
		                Core.Globals.MaxDate,
		                null
		            ));
		
		            double tgtPrice = isLong2
		                ? baseTargetPrice + i * tickSz2
		                : baseTargetPrice - i * tickSz2;
		
		            newOrders.Add(acct.CreateOrder(
		                instr,
		                isLong2 ? OrderAction.Sell : OrderAction.BuyToCover,
		                OrderType.Limit,
		                OrderEntry.Automated,
		                TimeInForce.Gtc,
		                1,
		                tgtPrice,
		                0,
		                legOco2,
		                Name + "_Split" + (i+1),
		                Core.Globals.MaxDate,
		                null
		            ));
		        }
		
		        acct.Submit(newOrders.ToArray());
		        Print($"Split ▶ created {qty} stop+target pairs for OCO={oco}");
		    }
		}


		private void RemoveStopsAndTargets()
		{
			// 1) resolve account
		    xAcSelector = Window
		      .GetWindow(ChartControl.Parent)
		      .FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
		    if (xAcSelector == null) return;
		    string acctName = xAcSelector.SelectedAccount?.ToString();
		    if (string.IsNullOrEmpty(acctName)) return;
		    var acct = NinjaTrader.Cbi.Account.All.FirstOrDefault(a => acctName.Contains(a.Name));
		    if (acct == null) return;
		
		    // 2) resolve instrument
		    xInSelector = Window
		      .GetWindow(ChartControl.OwnerChart)
		      .FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
		    if (xInSelector == null) return;
		    var instr = xInSelector.Instrument;
		    if (instr == null) return;
		
		    // 3) find all working or accepted stops & limits
		    var exitOrders = acct.Orders
		        .Where(o =>
		            o.Instrument.FullName == instr.FullName &&
		           (o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.Limit) &&
		           (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted))
		        .ToArray();
		
		    if (exitOrders.Length == 0)
		    {
		        Print("Naked ▶ no exit orders to remove");
		        return;
		    }
		
		    // 4) cancel them
		    acct.Cancel(exitOrders);
		    Print($"Naked ▶ removed {exitOrders.Length} stops/targets");	
		}
		
		
		private void DoublePosition()
		{
		    // 1) resolve account & instrument
		    xAcSelector = Window
		      .GetWindow(ChartControl.Parent)
		      .FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
		    if (xAcSelector == null) return;
		    string acctName = xAcSelector.SelectedAccount?.ToString() ?? "";
			var acct = NinjaTrader.Cbi.Account.All
			    .FirstOrDefault(a => acctName.Contains(a.Name));
			if (acct == null) return;
		
		    xInSelector = Window
		      .GetWindow(ChartControl.OwnerChart)
		      .FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
		    if (xInSelector == null) return;
		    var instr = xInSelector.Instrument;
		
		    // 2) pull your current filled position
		    var pos = acct.Positions
		        .FirstOrDefault(p => p.Instrument.FullName == instr.FullName);
		    if (pos == null || pos.Quantity <= 0)
		    {
		        Print("Double ▶ no open position");
		        return;
		    }
		
		    bool isLong      = pos.MarketPosition == MarketPosition.Long;
		    int  currentQty  = pos.Quantity;
		
		    // 3) send one market order to add currentQty (doubling total)
		    var dblOrder = acct.CreateOrder(
		        instr,
		        isLong ? OrderAction.Buy : OrderAction.SellShort,
		        OrderType.Market,
		        OrderEntry.Automated,
		        TimeInForce.Gtc,
		        currentQty,
		        0, 0,
		        "",                      // no OCO on the market leg
		        Name + "_Double",
		        Core.Globals.MaxDate,
		        null
		    );
		    acct.Submit(new[] { dblOrder });
		    Print($"Double ▶ submitted market for {currentQty}");
		
		    // 4) collect **all** exit orders in any OCO group
		    var bracketOrders = acct.Orders
		        .Where(o =>
		            o.Instrument.FullName == instr.FullName &&
		            !string.IsNullOrEmpty(o.Oco) &&
		           (o.OrderState == OrderState.Accepted || o.OrderState == OrderState.Working) &&
		           (o.OrderType == OrderType.Limit ||
		            o.OrderType == OrderType.StopMarket ||
		            o.OrderType == OrderType.StopLimit))
		        .ToList();
		
		    if (bracketOrders.Count == 0)
		    {
		        Print("Double ▶ no bracket exits to resize");
		        return;
		    }
		
		    // 5) group by OCO and double each group
		    foreach (var group in bracketOrders.GroupBy(o => o.Oco))
		    {
		        // what each leg was sized at before doubling
		        int preQty = group.Min(o => o.Quantity);
		        int newQty = preQty * 2;
		
		        foreach (var o in group)
		            o.QuantityChanged = newQty;
		
		        acct.Change(group.ToArray());
		        Print($"Double ▶ resized OCO={group.Key} from {preQty} to {newQty}");
		    }
		}


		private void RemoveHalfPosition()
		{
		    // 1) resolve account & instrument
		    xAcSelector = Window
		      .GetWindow(ChartControl.Parent)
		      .FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
		    if (xAcSelector == null) return;
		    string acctName = xAcSelector.SelectedAccount?.ToString() ?? "";
			var acct = NinjaTrader.Cbi.Account.All
			    .FirstOrDefault(a => acctName.Contains(a.Name));
			if (acct == null) return;
		    xInSelector = Window
		      .GetWindow(ChartControl.OwnerChart)
		      .FindFirst("ChartWindowInstrumentSelector") as NinjaTrader.Gui.Tools.InstrumentSelector;
		    if (xInSelector == null) return;
		    var instr = xInSelector.Instrument;
		
		    // 2) collect all exit‐orders in any OCO
		    var exitOrders = acct.Orders
		        .Where(o =>
		            o.Instrument.FullName == instr.FullName &&
		            !string.IsNullOrEmpty(o.Oco) &&
		            (o.OrderState == OrderState.Working || o.OrderState == OrderState.Accepted) &&
		            (o.OrderType == OrderType.Limit ||
		             o.OrderType == OrderType.StopMarket ||
		             o.OrderType == OrderType.StopLimit))
		        .ToList();
		    if (!exitOrders.Any())
		    {
		        Print("Half ▶ no bracket orders to halve");
		        return;
		    }
		
		    // 3) group by OCO and figure out #contracts per group
		    var groups = exitOrders
		        .GroupBy(o => o.Oco)
		        .Select(g =>
		        {
		            bool isAgg = g.Any(o => o.Quantity > 1);
		            int  contracts = isAgg 
		                ? g.Min(o => o.Quantity)    // multi‐contract bracket
		                : g.Count() / 2;           // 2 orders per 1‐contract ATM
		            return new { Oco = g.Key, Legs = g.ToList(), IsAggregated = isAgg, Contracts = contracts };
		        })
		        .ToList();
		
		    // total contracts open, and need to remove half (floor)
		    int totalContracts   = groups.Sum(g => g.Contracts);
		    int toRemoveContracts = totalContracts / 2;
		    if (toRemoveContracts == 0)
		    {
		        Print($"Half ▶ only {totalContracts} contract(s), nothing to remove");
		        return;
		    }
		
		    // 4) remove via one market order
		    var pos = acct.Positions.FirstOrDefault(p => p.Instrument.FullName == instr.FullName);
		    if (pos == null || pos.Quantity < toRemoveContracts)
		    {
		        Print("Half ▶ position mismatch");
		        return;
		    }
		    var mktAction = pos.MarketPosition == MarketPosition.Long
		                  ? OrderAction.Sell
		                  : OrderAction.BuyToCover;
		    var mkt = acct.CreateOrder(
		        instr,
		        mktAction,
		        OrderType.Market,
		        OrderEntry.Automated,
		        TimeInForce.Gtc,
		        toRemoveContracts,
		        0, 0,
		        "",
		        Name + "_Half",
		        Core.Globals.MaxDate,
		        null
		    );
		    acct.Submit(new[] { mkt });
		    Print($"Half ▶ market remove {toRemoveContracts} of {totalContracts}");
		
		    // 5) now adjust each OCO to match the new sizes
		    int remainingToRemove = toRemoveContracts;
		    foreach (var grp in groups)
		    {
		        if (remainingToRemove == 0)
		            break;
		
		        int removeHere = Math.Min(grp.Contracts, remainingToRemove);
		        int keepHere   = grp.Contracts - removeHere;
		
		        if (grp.IsAggregated)
		        {
		            // multi‐contract bracket
		            if (keepHere > 0)
		            {
		                // shrink quantities
		                foreach (var o in grp.Legs)
		                    o.QuantityChanged = keepHere;
		                acct.Change(grp.Legs.ToArray());
		                Print($"Half ▶ OCO={grp.Oco} resized from {grp.Contracts} to {keepHere}");
		            }
		            else
		            {
		                // remove entire bracket
		                acct.Cancel(grp.Legs.ToArray());
		                Print($"Half ▶ OCO={grp.Oco} fully removed");
		            }
		        }
		        else
		        {
		            // ATM‐style one‐contract OCO
		            // cancel 'removeHere' stops & targets
		            var stops   = grp.Legs.Where(o => o.OrderType == OrderType.StopMarket || o.OrderType == OrderType.StopLimit);
		            var targets = grp.Legs.Where(o => o.OrderType == OrderType.Limit);
		
		            var toCancel = new List<NinjaTrader.Cbi.Order>();
		            if (pos.MarketPosition == MarketPosition.Long)
		            {
		                toCancel.AddRange(stops.OrderByDescending(o => o.StopPrice).Take(removeHere));
		                toCancel.AddRange(targets.OrderBy(o => o.LimitPrice).Take(removeHere));
		            }
		            else
		            {
		                toCancel.AddRange(stops.OrderBy(o => o.StopPrice).Take(removeHere));
		                toCancel.AddRange(targets.OrderByDescending(o => o.LimitPrice).Take(removeHere));
		            }
		
		            if (toCancel.Any())
		            {
		                acct.Cancel(toCancel.ToArray());
		                Print($"Half ▶ OCO={grp.Oco} cancelled {toCancel.Count} ATM legs");
		            }
		        }
		
		        remainingToRemove -= removeHere;
		    }
		}


		private void AddStopOrder(int stopTicks)
		{
		    // 1) find the ChartTrader’s account-selector
		    xAcSelector = Window
		      .GetWindow(ChartControl.Parent)
		      .FindFirst("ChartTraderControlAccountSelector")
		        as NinjaTrader.Gui.Tools.AccountSelector;
		    if (xAcSelector == null) return;
		    string acctName = xAcSelector.SelectedAccount?.ToString();
		    if (string.IsNullOrEmpty(acctName)) return;
		
		    // 2) look up the real Cbi.Account
		    var acct = NinjaTrader.Cbi.Account.All
		               .FirstOrDefault(a => acctName.Contains(a.Name));
		    if (acct == null) return;
		
		    // 3) find the instrument selector on the Chart window
		    xInSelector = Window
		      .GetWindow(ChartControl.OwnerChart)
		      .FindFirst("ChartWindowInstrumentSelector")
		        as NinjaTrader.Gui.Tools.InstrumentSelector;
		    if (xInSelector == null) return;
		    var instr = xInSelector.Instrument;
		    if (instr == null) return;
		
		    // 4) pull your filled market position
		    var pos = acct.Positions
		                  .FirstOrDefault(p => p.Instrument.FullName == instr.FullName);
		    if (pos == null || pos.Quantity == 0)
		    {
		        Print("No open position to add a stop");
		        return;
		    }
		
		    // — NEW: skip if you already have a StopMarket working on this instrument
		    bool hasStop = acct.Orders.Any(o =>
		        o.Instrument.FullName == instr.FullName &&
		        o.OrderType     == OrderType.StopMarket &&
		       (o.OrderState    == OrderState.Accepted || o.OrderState == OrderState.Working));
		    if (hasStop)
		    {
		        Print("A stop already exists—skipping Add Stop");
		        return;
		    }
		
		    // 5) compute stop price off lastClose…
		    double basePrice  = instr.MasterInstrument.RoundToTickSize(lastClose);
		    double tickSz     = instr.MasterInstrument.TickSize;
		    bool   isLong     = pos.MarketPosition == MarketPosition.Long;
		    int    qty        = pos.Quantity;
		    double stopPx     = isLong
		                        ? basePrice - stopTicks * tickSz
		                        : basePrice + stopTicks * tickSz;
		
		    // 6) create & submit the stop
		    var stopOrder = acct.CreateOrder(
		        instr,
		        isLong ? OrderAction.Sell : OrderAction.BuyToCover,
		        OrderType.StopMarket,
		        OrderEntry.Automated,
		        TimeInForce.Gtc,
		        qty,
		        0,
		        stopPx,
		        "",                    // no OCO
		        Name + "_AddStop",
		        Core.Globals.MaxDate,
		        null
		    );
		    acct.Submit(new[] { stopOrder });
		    Print($"Add Stop placed ▶ Stop={stopPx}");
		}
		
		private void BracketOrder(int stopTicks, int profitTicks)
		{
		    // 1) grab the selected account name
		    xAcSelector = Window
		      .GetWindow(ChartControl.Parent)
		      .FindFirst("ChartTraderControlAccountSelector")
		        as NinjaTrader.Gui.Tools.AccountSelector;
		    if (xAcSelector == null) return;
		    string acctName = xAcSelector.SelectedAccount?.ToString();
		    if (string.IsNullOrEmpty(acctName)) return;
		
		    // 2) look up the real Cbi.Account
		    var acct = NinjaTrader.Cbi.Account.All
		               .FirstOrDefault(a => acctName.Contains(a.Name));
		    if (acct == null) return;
		
		    // 3) grab the chart’s instrument
		    xInSelector = Window
		      .GetWindow(ChartControl.OwnerChart)
		      .FindFirst("ChartWindowInstrumentSelector")
		        as NinjaTrader.Gui.Tools.InstrumentSelector;
		    if (xInSelector == null) return;
		    var instr = xInSelector.Instrument;
		    if (instr == null) return;
		
		    // 4) guard: don’t bracket if you already have any OCO’d exit legs
		    bool hasBracket = acct.Orders.Any(o =>
		        o.Instrument.FullName == instr.FullName &&
		        !string.IsNullOrEmpty(o.Oco) &&
		       (o.OrderState == OrderState.Accepted || o.OrderState == OrderState.Working));
		    if (hasBracket)
		    {
		        Print("Bracket already exists—skipping");
		        return;
		    }
		
		    // 5) pull your filled market position
		    var pos = acct.Positions
		                  .FirstOrDefault(p => p.Instrument.FullName == instr.FullName);
		    if (pos == null || pos.Quantity == 0)
		    {
		        Print("No open position to bracket");
		        return;
		    }
		
		    // 6) compute basePrice off pos.AveragePrice
		    double basePrice = pos.AveragePrice;
		    double tickSz    = instr.MasterInstrument.TickSize;
		    bool   isLong    = pos.MarketPosition == MarketPosition.Long;
		    int    qty       = pos.Quantity;
		
		    double stopPx   = isLong
		                     ? basePrice - stopTicks   * tickSz
		                     : basePrice + stopTicks   * tickSz;
		    double targetPx = isLong
		                     ? basePrice + profitTicks * tickSz
		                     : basePrice - profitTicks * tickSz;
		
		    // 7) assign OCO and submit two legs
		    string ocoId = Guid.NewGuid().ToString();
		
		    var stopOrder = acct.CreateOrder(
		        instr,
		        isLong ? OrderAction.Sell : OrderAction.BuyToCover,
		        OrderType.StopMarket,
		        OrderEntry.Automated,
		        TimeInForce.Gtc,
		        qty,
		        0,
		        stopPx,
		        ocoId,
		        Name + "_SL",
		        Core.Globals.MaxDate,
		        null
		    );
		
		    var targetOrder = acct.CreateOrder(
		        instr,
		        isLong ? OrderAction.Sell : OrderAction.BuyToCover,
		        OrderType.Limit,
		        OrderEntry.Automated,
		        TimeInForce.Gtc,
		        qty,
		        targetPx,
		        0,
		        ocoId,
		        Name + "_TP",
		        Core.Globals.MaxDate,
		        null
		    );
		
		    acct.Submit(new[] { stopOrder, targetOrder });
		    Print($"Bracket placed ▶ Entry={basePrice}, SL={stopPx}, TP={targetPx}, OCO={ocoId}");
		}

		
		private void StopsToBreakeven(int ticks)
		{
		    // 1) find the ChartTrader’s account-selector and grab the selected name
		    xAcSelector = Window
		      .GetWindow(ChartControl.Parent)
		      .FindFirst("ChartTraderControlAccountSelector") 
		        as NinjaTrader.Gui.Tools.AccountSelector;
		    if (xAcSelector == null) return;
		    string acctName = xAcSelector.SelectedAccount?.ToString();
		    if (string.IsNullOrEmpty(acctName)) return;
		
		    // 2) look up the real Cbi.Account object
		    var acct = NinjaTrader.Cbi.Account.All
		               .FirstOrDefault(a => acctName.Contains(a.Name));
		    if (acct == null) return;
		
		    // 3) find the instrument selector on the Chart window
		    xInSelector = Window
		      .GetWindow(ChartControl.OwnerChart)
		      .FindFirst("ChartWindowInstrumentSelector")
		        as NinjaTrader.Gui.Tools.InstrumentSelector;
		    if (xInSelector == null) return;
		    var instr = xInSelector.Instrument;
		    if (instr == null) return;
		
		    // 4) get your live position for this instrument
		    var pos = acct.Positions
		              .FirstOrDefault(p => p.Instrument.FullName == instr.FullName);
		    if (pos == null || pos.Quantity == 0) return;
		
		    // 5) loop every working stop (market or limit) for this instrument
		    foreach (var order in acct.Orders
		      .Where(o =>
		         o.Instrument.FullName == instr.FullName &&
		        (o.OrderType == OrderType.StopMarket ||
		         o.OrderType == OrderType.StopLimit) &&
		         o.OrderState != OrderState.Cancelled &&
		         o.OrderState != OrderState.Filled))
		    {
		        // compute new stop price
		        double newStop = pos.AveragePrice 
		                       + (pos.MarketPosition == MarketPosition.Long 
		                          ? +ticks 
		                          : -ticks)
		                         * instr.MasterInstrument.TickSize;
		
		        order.StopPriceChanged = newStop;
		        acct.Change(new[] { order });
		    }
		}
		
		protected override void OnBarUpdate()
		{
			lastClose = Close[0];
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlightenButtonPanel[] cacheAlightenButtonPanel;
		public AlightenButtonPanel AlightenButtonPanel(int breakeven1PlusTicks, int breakeven2PlusTicks, int bracketStopTicks, int bracketProfitTicks, Brush button1Color, Brush button2Color, Brush button3Color, Brush button4Color, Brush button5Color, Brush button6Color, Brush button7Color, Brush button8Color)
		{
			return AlightenButtonPanel(Input, breakeven1PlusTicks, breakeven2PlusTicks, bracketStopTicks, bracketProfitTicks, button1Color, button2Color, button3Color, button4Color, button5Color, button6Color, button7Color, button8Color);
		}

		public AlightenButtonPanel AlightenButtonPanel(ISeries<double> input, int breakeven1PlusTicks, int breakeven2PlusTicks, int bracketStopTicks, int bracketProfitTicks, Brush button1Color, Brush button2Color, Brush button3Color, Brush button4Color, Brush button5Color, Brush button6Color, Brush button7Color, Brush button8Color)
		{
			if (cacheAlightenButtonPanel != null)
				for (int idx = 0; idx < cacheAlightenButtonPanel.Length; idx++)
					if (cacheAlightenButtonPanel[idx] != null && cacheAlightenButtonPanel[idx].Breakeven1PlusTicks == breakeven1PlusTicks && cacheAlightenButtonPanel[idx].Breakeven2PlusTicks == breakeven2PlusTicks && cacheAlightenButtonPanel[idx].BracketStopTicks == bracketStopTicks && cacheAlightenButtonPanel[idx].BracketProfitTicks == bracketProfitTicks && cacheAlightenButtonPanel[idx].Button1Color == button1Color && cacheAlightenButtonPanel[idx].Button2Color == button2Color && cacheAlightenButtonPanel[idx].Button3Color == button3Color && cacheAlightenButtonPanel[idx].Button4Color == button4Color && cacheAlightenButtonPanel[idx].Button5Color == button5Color && cacheAlightenButtonPanel[idx].Button6Color == button6Color && cacheAlightenButtonPanel[idx].Button7Color == button7Color && cacheAlightenButtonPanel[idx].Button8Color == button8Color && cacheAlightenButtonPanel[idx].EqualsInput(input))
						return cacheAlightenButtonPanel[idx];
			return CacheIndicator<AlightenButtonPanel>(new AlightenButtonPanel(){ Breakeven1PlusTicks = breakeven1PlusTicks, Breakeven2PlusTicks = breakeven2PlusTicks, BracketStopTicks = bracketStopTicks, BracketProfitTicks = bracketProfitTicks, Button1Color = button1Color, Button2Color = button2Color, Button3Color = button3Color, Button4Color = button4Color, Button5Color = button5Color, Button6Color = button6Color, Button7Color = button7Color, Button8Color = button8Color }, input, ref cacheAlightenButtonPanel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlightenButtonPanel AlightenButtonPanel(int breakeven1PlusTicks, int breakeven2PlusTicks, int bracketStopTicks, int bracketProfitTicks, Brush button1Color, Brush button2Color, Brush button3Color, Brush button4Color, Brush button5Color, Brush button6Color, Brush button7Color, Brush button8Color)
		{
			return indicator.AlightenButtonPanel(Input, breakeven1PlusTicks, breakeven2PlusTicks, bracketStopTicks, bracketProfitTicks, button1Color, button2Color, button3Color, button4Color, button5Color, button6Color, button7Color, button8Color);
		}

		public Indicators.AlightenButtonPanel AlightenButtonPanel(ISeries<double> input , int breakeven1PlusTicks, int breakeven2PlusTicks, int bracketStopTicks, int bracketProfitTicks, Brush button1Color, Brush button2Color, Brush button3Color, Brush button4Color, Brush button5Color, Brush button6Color, Brush button7Color, Brush button8Color)
		{
			return indicator.AlightenButtonPanel(input, breakeven1PlusTicks, breakeven2PlusTicks, bracketStopTicks, bracketProfitTicks, button1Color, button2Color, button3Color, button4Color, button5Color, button6Color, button7Color, button8Color);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlightenButtonPanel AlightenButtonPanel(int breakeven1PlusTicks, int breakeven2PlusTicks, int bracketStopTicks, int bracketProfitTicks, Brush button1Color, Brush button2Color, Brush button3Color, Brush button4Color, Brush button5Color, Brush button6Color, Brush button7Color, Brush button8Color)
		{
			return indicator.AlightenButtonPanel(Input, breakeven1PlusTicks, breakeven2PlusTicks, bracketStopTicks, bracketProfitTicks, button1Color, button2Color, button3Color, button4Color, button5Color, button6Color, button7Color, button8Color);
		}

		public Indicators.AlightenButtonPanel AlightenButtonPanel(ISeries<double> input , int breakeven1PlusTicks, int breakeven2PlusTicks, int bracketStopTicks, int bracketProfitTicks, Brush button1Color, Brush button2Color, Brush button3Color, Brush button4Color, Brush button5Color, Brush button6Color, Brush button7Color, Brush button8Color)
		{
			return indicator.AlightenButtonPanel(input, breakeven1PlusTicks, breakeven2PlusTicks, bracketStopTicks, bracketProfitTicks, button1Color, button2Color, button3Color, button4Color, button5Color, button6Color, button7Color, button8Color);
		}
	}
}

#endregion
