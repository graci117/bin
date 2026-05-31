// Coded by NinjaTrader_Jim, NinjaTrader_Kate, NinjaTrader_ChelseaB
#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifierTypeConverter")]
	public class AtmStrategyIdentifier : Indicator
	{
		private AccountSelector							accountSelector;
		private ChartScale								chartScaleAccess;
		private Account									chartTraderAccount;
		private string									chartTraderInstrumentSymbol;
		private double									lowestPrice, pointValue, tickSize;
		private ComboBox								instrumentComboBox;

		private Dictionary<long, AtmOrders>				atmDictionary;
		private Dictionary<double, List<DrawOrder>>		stopOrders, targetOrders;
		private Dictionary<double, string>				renderPriceLevels;

		private Collection<AtmStrategyIdentifierTools.BrushWrapper>	brushCollection;
		private List<AtmStrategyIdentifierTools.BrushWrapper>		brushCollectionDefaults;
		private List<SharpDX.Direct2D1.Brush>						dxBrushList;
		private SharpDX.DirectWrite.TextFormat						textFormat;

		public class DrawOrder
		{
			public Order	Order;
			public string	AtmName;
		}

		public class AtmOrders
		{
			public long			Id;
			public string		Name;
			public List<Order>	Stops, Targets, Entry;
			
			public AtmOrders()
			{
				Stops	= new List<Order>();
				Targets	= new List<Order>();
				Entry	= new List<Order>();
			}
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= @"This indicator will add labels to found ATM strategies on the chart. You may add colors to the Brush Collection, and each found ATM strategy will rotate through these colors.";
				Name	   				= "Atm Strategy Identifier";
				IsChartOnly				= true;
				IsOverlay  				= true;

				IncludeAutoScale		= true;
				AutoScalePadding		= 7;
				PnLDisplay				= AtmStrategyIdentifierTools.PnLDisplay.Currency;
				Font 	   				= new SimpleFont("Arial", 14);

				brushCollectionDefaults = new List<AtmStrategyIdentifierTools.BrushWrapper>() { new AtmStrategyIdentifierTools.BrushWrapper(Brushes.Orange) { IsDefault = true } };
				BrushCollection 		= new Collection<AtmStrategyIdentifierTools.BrushWrapper>(brushCollectionDefaults);
			}
			else if (State == State.Configure)
			{
				IsAutoScale				= IncludeAutoScale;
			}
			else if (State == State.DataLoaded)
			{
				atmDictionary 			= new Dictionary<long, AtmOrders>();
				lowestPrice				= double.MaxValue;
				pointValue				= Instrument.MasterInstrument.PointValue;
				renderPriceLevels		= new Dictionary<double, string>();
				stopOrders				= new Dictionary<double, List<DrawOrder>>();
				targetOrders			= new Dictionary<double, List<DrawOrder>>();
				textFormat 				= Font.ToDirectWriteTextFormat();
				tickSize				= Instrument.MasterInstrument.TickSize;

				CreateWPFControls();
					
				SetZOrder(int.MaxValue);
			}
			else if(State == State.Terminated)
			{
				if (textFormat != null)
					textFormat.Dispose();

				if (chartTraderAccount != null)
					chartTraderAccount.OrderUpdate -= Account_OrderUpdate;
				
				if (accountSelector != null)
					accountSelector.SelectionChanged -= AccountSelector_SelectionChange;
					
				if (instrumentComboBox != null)
					instrumentComboBox.SelectionChanged -= InstrumentComboBox_InstrumentChanged;
			}
		}

		private void CreateWPFControls()
		{
			if (ChartControl == null)
				return;

			ChartControl.Dispatcher.InvokeAsync(new Action(() =>
			{
				if (BrushCollection == null || BrushCollection.Count == 0)
				{
					string message = "Atm Strategy Identifier: No brushes were detected in Brush Collection. Atm Strategy Labels will not be drawn.";					
					Print(message);
					Log(message, LogLevel.Warning);
					this.Dispatcher.InvokeAsync(() =>
					{
						Draw.TextFixed(this, "warning", message, TextPosition.BottomLeft);
					});
					return;
				}

				accountSelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as AccountSelector;
				
				if (accountSelector != null && accountSelector.SelectedAccount != null)
				{
			        chartTraderAccount					= accountSelector.SelectedAccount;
					chartTraderAccount.OrderUpdate		+= Account_OrderUpdate;
					accountSelector.SelectionChanged	+= AccountSelector_SelectionChange;
				}
				
				instrumentComboBox = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlInstrumentSelector") as ComboBox;
				
				if (instrumentComboBox != null && instrumentComboBox.SelectedValue != null)
				{
					chartTraderInstrumentSymbol			= instrumentComboBox.SelectedValue.ToString();
					instrumentComboBox.SelectionChanged	+= InstrumentComboBox_InstrumentChanged;
				}

				Dispatcher.InvokeAsync(() =>
				{
					CheckActiveAtmStrategyOrders();
				});				
			}));
		}

		private void CheckActiveAtmStrategyOrders()
		{
			lowestPrice	= double.MaxValue;			

			lock (atmDictionary)
				atmDictionary.Clear();
			lock (stopOrders)
				stopOrders.Clear();
			lock (targetOrders)
				targetOrders.Clear();
			lock (renderPriceLevels)
				renderPriceLevels.Clear();

			if (chartTraderAccount == null)
				return;

			Position position	= chartTraderAccount.Positions.Where(pos => pos.Instrument == Instrument).FirstOrDefault();

			if (position == null || position.MarketPosition == MarketPosition.Flat || Instrument.FullName != chartTraderInstrumentSymbol)
				return;

			double entryPrice	= position.AveragePrice;

			// load all atm instances and sort the orders to entry, target, and stop lists
			lock (atmDictionary)
			{
				foreach (Order order in chartTraderAccount.Orders.ToList())
				{
					if (order.GetOwnerStrategy() != null && order.Instrument == Instrument)
					{
						AtmStrategy atm = order.GetOwnerStrategy() as AtmStrategy;
							
						if (!atmDictionary.ContainsKey(atm.Id))
						{
							AtmOrders atmOrders		= new AtmOrders();
							atmOrders.Id			= atm.Id;
							atmOrders.Name			= atm.DisplayName;

							atmDictionary.Add(atm.Id, atmOrders);
						}
							
						foreach (KeyValuePair<long, AtmOrders> atmDict in atmDictionary)
						{
							if (atmDict.Key == atm.Id)
							{
								// sort into arrays of stopOrders and targetOrders
								if (order.Name.Contains("Stop") && !atmDict.Value.Stops.Contains(order) && !Order.IsTerminalState(order.OrderState))
									atmDict.Value.Stops.Add(order);

								if (order.Name.Contains("Target") && !atmDict.Value.Targets.Contains(order) && !Order.IsTerminalState(order.OrderState))
									atmDict.Value.Targets.Add(order);

								if(!order.Name.Contains("Stop") && !order.Name.Contains("Target") && !atmDict.Value.Entry.Contains(order))
									atmDict.Value.Entry.Add(order);
							}
						}
					}
				}
			}

			// sort into dictionaries of each price level of stopOrders and each price level with targetOrders from all active atms
			foreach (KeyValuePair<long, AtmOrders> atmDict in atmDictionary)
			{
				(atmDict.Value as AtmOrders).Stops.ForEach(atmOrder =>
				{
					lowestPrice = Math.Min(lowestPrice, atmOrder.StopPrice);

					if (!stopOrders.ContainsKey(atmOrder.StopPrice))
						stopOrders.Add(atmOrder.StopPrice, new List<DrawOrder> { new DrawOrder() { AtmName = atmDict.Value.Name, Order = atmOrder } });
					else
						stopOrders[atmOrder.StopPrice].Add(new DrawOrder() { AtmName = atmDict.Value.Name, Order = atmOrder });
				});

				(atmDict.Value as AtmOrders).Targets.ForEach(atmOrder =>
				{
					lowestPrice = Math.Min(lowestPrice, atmOrder.LimitPrice);

					if (!targetOrders.ContainsKey(atmOrder.LimitPrice))
						targetOrders.Add(atmOrder.LimitPrice, new List<DrawOrder> { new DrawOrder() { AtmName = atmDict.Value.Name, Order = atmOrder } });
					else
						targetOrders[atmOrder.LimitPrice].Add(new DrawOrder() { AtmName = atmDict.Value.Name, Order = atmOrder });
				});
			}

			// for each unique price level with a resting order, combine the orders atm strategy names and pnl into a single string
			foreach (KeyValuePair<double, List<DrawOrder>> stopLevels in stopOrders)
			{
				// join the atm names with commas
				string atmNames			= string.Join(", ", stopLevels.Value.Select(stopLevel => stopLevel.AtmName).Distinct().ToList());
				double totalQuantity	= stopLevels.Value.Select(stopLevel => stopLevel.Order.Quantity).Sum();
				string possiblePnL		= ((stopLevels.Key - entryPrice) * totalQuantity).ToString();

				// create strings for each PnLDisplay unit type
				if (PnLDisplay == AtmStrategyIdentifierTools.PnLDisplay.Currency)
					possiblePnL = ((stopLevels.Key - entryPrice) * totalQuantity * pointValue).ToString("C");
				else if (PnLDisplay == AtmStrategyIdentifierTools.PnLDisplay.Ticks)
					possiblePnL = ((stopLevels.Key - entryPrice) / tickSize).ToString();
				else if (PnLDisplay == AtmStrategyIdentifierTools.PnLDisplay.Percent)
					possiblePnL = string.Format("{0}%", ((stopLevels.Key - entryPrice) * totalQuantity / 100));

				string message = string.Format("{0} | PnL: {1}", atmNames, possiblePnL);
				Print(message);

				// add this to a list of price level labels to be rendered
				renderPriceLevels.Add(stopLevels.Key, message);
			}

			// do the same for target limit orders
			foreach (KeyValuePair<double, List<DrawOrder>> limitLevels in targetOrders)
			{
				string atmNames			= string.Join(", ", limitLevels.Value.Select(limitLevel => limitLevel.AtmName).Distinct().ToList());
				double totalQuantity	= limitLevels.Value.Select(limitLevel => limitLevel.Order.Quantity).Sum();
				string possiblePnL		= ((limitLevels.Key - entryPrice) * totalQuantity).ToString();

				if (PnLDisplay == AtmStrategyIdentifierTools.PnLDisplay.Currency)
					possiblePnL = ((limitLevels.Key - entryPrice) * totalQuantity * pointValue).ToString("C");
				else if (PnLDisplay == AtmStrategyIdentifierTools.PnLDisplay.Ticks)
					possiblePnL = ((limitLevels.Key - entryPrice) / tickSize).ToString();
				else if (PnLDisplay == AtmStrategyIdentifierTools.PnLDisplay.Percent)
					possiblePnL = string.Format("{0}%", ((limitLevels.Key - entryPrice) * totalQuantity / 100));

				string message = string.Format("{0} | PnL: {1}", atmNames, possiblePnL);
				renderPriceLevels.Add(limitLevels.Key, message);
			}

			ForceRefresh();
		}
		
		private void Account_OrderUpdate(object sender, OrderEventArgs e)
		{
			if (e.Order.Instrument == Instrument && (e.Order.OrderState == OrderState.Accepted || e.Order.OrderState == OrderState.Working || e.Order.OrderState == OrderState.Filled || e.Order.OrderState == OrderState.PartFilled || e.Order.OrderState == OrderState.Cancelled))
				CheckActiveAtmStrategyOrders();
		}
		
		private void AccountSelector_SelectionChange(object sender, EventArgs e)
		{
			if (accountSelector.SelectedAccount != null)
			{	
				if (chartTraderAccount != null)
					chartTraderAccount.OrderUpdate -= Account_OrderUpdate;

				chartTraderAccount					= accountSelector.SelectedAccount;
				chartTraderAccount.OrderUpdate		+= Account_OrderUpdate;

				CheckActiveAtmStrategyOrders();
			}
		}
		
		private void InstrumentComboBox_InstrumentChanged(object sender, EventArgs e)
		{
			if (instrumentComboBox.SelectedValue != null)
				chartTraderInstrumentSymbol = instrumentComboBox.SelectedValue.ToString();

			CheckActiveAtmStrategyOrders();
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{						
			if (dxBrushList.IsNullOrEmpty() || ChartControl.Properties.ChartTraderVisibility == ChartTraderVisibility.Collapsed)
				return;
			
			if (IsInHitTest || renderPriceLevels == null)
				return;

			chartScaleAccess	= chartScale;

			lock (renderPriceLevels)
			{
				int j = -1;
				foreach (KeyValuePair<double, string> renderLevel in renderPriceLevels)
				{
					j = (j >= dxBrushList.Count - 2) ? 0 : j + 1;

					SharpDX.Direct2D1.Brush textBrushDx	= dxBrushList[j];

					SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, renderLevel.Value, textFormat, ChartPanel.X + ChartPanel.W,textFormat.FontSize);

					float x	= (float)(ChartPanel.W - textLayout.Metrics.Width - ChartControl.OwnerChart.ChartTrader.Properties.OrderDisplayBarLength);
					float y	= chartScale.GetYByValue(renderLevel.Key) + 7;

					SharpDX.Vector2 textPoint = new SharpDX.Vector2(x, y);
					RenderTarget.DrawTextLayout(textPoint, textLayout, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					textLayout.Dispose();
				}
			}
		}
		
		public override void OnRenderTargetChanged()
		{
			if (dxBrushList.IsNullOrEmpty())
				dxBrushList = new List<SharpDX.Direct2D1.Brush>();
			
			foreach(SharpDX.Direct2D1.Brush dxBrush in dxBrushList)
				if (dxBrush != null)
					dxBrush.Dispose();

			dxBrushList.Clear();
			
			if (RenderTarget == null)
				return;
			
			for (int i = 0; i < BrushCollection.Count; i++)
			{
				Brush brush = (SolidColorBrush)new BrushConverter().ConvertFrom(BrushCollection[i].ToString());
				dxBrushList.Add(brush.ToDxBrush(RenderTarget));
			}
		}

		public override void OnCalculateMinMax()
		{
			base.OnCalculateMinMax();

			if (lowestPrice == double.MaxValue)	
				return;
			
			MaxValue = lowestPrice;
			MinValue = lowestPrice - AutoScalePadding * TickSize;
		}

		#region Properties
		[RefreshProperties(RefreshProperties.All)]
		[NinjaScriptProperty]
		[Display(Name = "Auto scale", GroupName = "Visual", Order = 0, Description = "Number of ticks of padding below lowest visible label when AutoScale is enabled")]
		public bool IncludeAutoScale
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "AutoScale padding", GroupName = "Parameters", Order = 1, Description = "Number of ticks of padding below lowest visible label when AutoScale is enabled")]
		public int AutoScalePadding
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Font", GroupName = "Parameters", Order = 3)]
		public SimpleFont Font
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "PnL display unit", GroupName = "Parameters", Order = 0)]
		public AtmStrategyIdentifierTools.PnLDisplay PnLDisplay
		{ get; set; }

		[XmlIgnore]
		[Gui.PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")]
		[Display(Name="Brush Collection", GroupName = "Parameters", Order = 2, Prompt = "1 Brush|{0} Brushes|Add Brush...|Edit Brush...|Edit Brushes...")]
		[SkipOnCopyTo(true)]
		public Collection<AtmStrategyIdentifierTools.BrushWrapper> BrushCollection
        {
			get { return brushCollection; }
			set	{ brushCollection = new Collection<AtmStrategyIdentifierTools.BrushWrapper>(value.ToList());	}
		}
		
        [Browsable(false)]
        public Collection<AtmStrategyIdentifierTools.BrushWrapper> BrushCollectionSerialize
        {
			get
			{
				foreach(AtmStrategyIdentifierTools.BrushWrapper bw in brushCollectionDefaults.ToList())
				{
					AtmStrategyIdentifierTools.BrushWrapper temp = BrushCollection.FirstOrDefault(p => p.BrushValue == bw.BrushValue && p.IsDefault == true);
					if(temp != null)
						BrushCollection.Remove(temp);
				}
				
				BrushCollection.All(p => p.IsDefault = false);

				return BrushCollection;
			}
			set
			{
				BrushCollection = value;
			}
        }
		#endregion
		
		#region Reflection to copy Collections to new assembly
		public override void CopyTo(NinjaScript ninjaScript)
		{
			base.CopyTo(ninjaScript);
			Type			newInstType				= ninjaScript.GetType();
			
			PropertyInfo	brushCollectionPropertyInfo	= newInstType.GetProperty("BrushCollection");
			Collection<AtmStrategyIdentifierTools.BrushWrapper> CopyToBrushCollection;
			
			CopyToBrushCollection = new Collection<AtmStrategyIdentifierTools.BrushWrapper>(BrushCollection);
			
			if (brushCollectionPropertyInfo != null)
			{
				IList newInstBrushCollection = brushCollectionPropertyInfo.GetValue(ninjaScript) as IList;
				if (newInstBrushCollection != null)
				{
					newInstBrushCollection.Clear();
					foreach (AtmStrategyIdentifierTools.BrushWrapper oldBrushWrapper in CopyToBrushCollection)
					{
						try
						{
							object newInstance = oldBrushWrapper.AssemblyClone(Core.Globals.AssemblyRegistry.GetType(typeof(AtmStrategyIdentifierTools.BrushWrapper).FullName));
							if (newInstance == null)
								continue;
							
							newInstBrushCollection.Add(newInstance);
						}
						catch { }
					}
				}
			}	
		}
		#endregion
	}
	
	#region TypeConverter to remove unneeded properties from UI Property Grid
	public class AtmStrategyIdentifierTypeConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            AtmStrategyIdentifier indicator = component as AtmStrategyIdentifier;

            PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
                                                                        ? base.GetProperties(context, component, attrs)
                                                                        : TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || propertyDescriptorCollection == null)
                return propertyDescriptorCollection;

			PropertyDescriptor toggleValue1 = propertyDescriptorCollection["AutoScalePadding"];
			PropertyDescriptor toggleValue2 = propertyDescriptorCollection["IncludeAutoScale"];
			PropertyDescriptor toggleValue3 = propertyDescriptorCollection["BrushCollection"];
			PropertyDescriptor toggleValue4 = propertyDescriptorCollection["Font"];
			PropertyDescriptor toggleValue5 = propertyDescriptorCollection["PDEX_InputUI"];
			PropertyDescriptor toggleValue6 = propertyDescriptorCollection["PnLDisplay"];
			PropertyDescriptor toggleValue7 = propertyDescriptorCollection["Name"];

			foreach (PropertyDescriptor pDesc in propertyDescriptorCollection)
				propertyDescriptorCollection.Remove(pDesc);

			propertyDescriptorCollection.Add(new AtmStrategyIdentifierTools.ReadOnlyDescriptor(indicator, toggleValue1));
			propertyDescriptorCollection.Add(toggleValue2);
			propertyDescriptorCollection.Add(toggleValue3);
			propertyDescriptorCollection.Add(toggleValue4);
			propertyDescriptorCollection.Add(toggleValue5);
			propertyDescriptorCollection.Add(toggleValue6);
			propertyDescriptorCollection.Add(toggleValue7);

			return propertyDescriptorCollection;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        { return true; }
    }
	#endregion
}


namespace AtmStrategyIdentifierTools
{
	public enum PnLDisplay
	{
		Currency,
		Points,
		Ticks,
		Percent
	}

	#region Hide/Show AutoScalePadding tools
	// This is a custom PropertyDescriptor class which will handle setting our desired properties to read only
	public class ReadOnlyDescriptor : PropertyDescriptor
    {
        // Need the instance on the property grid to check the show/hide toggle value
        private NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifier indicatorInstance;

        private PropertyDescriptor property;

        // The base instance constructor helps store the default Name and Attributes (Such as DisplayAttribute.Name, .GroupName, .Order)
        // Otherwise those details would be lost when we converted the PropertyDescriptor to the new custom ReadOnlyDescriptor
        public ReadOnlyDescriptor(NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifier indicator, PropertyDescriptor propertyDescriptor) : base(propertyDescriptor.Name, propertyDescriptor.Attributes.OfType<Attribute>().ToArray())
        {
            indicatorInstance	= indicator;
            property			= propertyDescriptor;
        }

        // Stores the current value of the property on the indicator
        public override object GetValue(object component)
        {
			NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifier targetInstance = component as NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifier;
            
			if (targetInstance == null)
                return null;
			
            switch (property.Name)
            {
                case "AutoScalePadding":
                	return targetInstance.AutoScalePadding;
            }
            return null;
        }

        // Updates the current value of the property on the indicator
        public override void SetValue(object component, object value)
        {
			NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifier targetInstance = component as NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifier;
            
			if (targetInstance == null)
                return;

            switch (property.Name)
            {
                case "AutoScalePadding":
                    targetInstance.AutoScalePadding = (int) value;
                    break;
            }
        }

        // set the PropertyDescriptor to "read only" based on the indicator instance input
        public override bool IsReadOnly
        { get { return !indicatorInstance.IncludeAutoScale; } }

        // IsReadOnly is the relevant interface member we need to use to obtain our desired custom behavior
        // but applying a custom property descriptor requires having to handle a bunch of other operations as well.
        // I.e., the below methods and properties are required to be implemented, otherwise it won't compile.
        public override bool CanResetValue(object component)
        { return true; }

        public override Type ComponentType
        { get { return typeof(NinjaTrader.NinjaScript.Indicators.AtmStrategyIdentifier); } }

        public override Type PropertyType
        { get { return typeof(int); } }

        public override void ResetValue(object component)
        { }

        public override bool ShouldSerializeValue(object component)
        { return true; }
    }
	#endregion

	#region BrushWrapper
	[CategoryDefaultExpanded(true)]
	public class BrushWrapper : NotifyPropertyChangedBase, ICloneable
	{
		public BrushWrapper() : this(Brushes.White)
		{
		}

		public BrushWrapper(Brush value)
		{
			BrushValue = value;
		}

		[XmlIgnore]
		[Display(Name = "Brushes", GroupName = "Brushes")]
		public Brush BrushValue
		{ get; set; }
		
		[Browsable(false)]
		public string BrushValueSerializable
		{
			get { return Serialize.BrushToString(BrushValue); }
			set { BrushValue = Serialize.StringToBrush(value); }
		}

		public object Clone()
		{
			BrushWrapper p	= new BrushWrapper();
			p.BrushValue	= BrushValue;
			return p;
		}
		
		[Browsable(false)]
		public bool IsDefault { get; set; }
		
		public override string ToString()
		{ return BrushValue.ToString(); }
		
		public object AssemblyClone(Type t)
		{
			Assembly a 				= t.Assembly;
			object brushCollection 	= a.CreateInstance(t.FullName);
			
			foreach (PropertyInfo p in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
				if (p.CanWrite)
					p.SetValue(brushCollection, this.GetType().GetProperty(p.Name).GetValue(this), null);
			
			return brushCollection;
		}
	}
	#endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AtmStrategyIdentifier[] cacheAtmStrategyIdentifier;
		public AtmStrategyIdentifier AtmStrategyIdentifier(bool includeAutoScale, int autoScalePadding, SimpleFont font, AtmStrategyIdentifierTools.PnLDisplay pnLDisplay)
		{
			return AtmStrategyIdentifier(Input, includeAutoScale, autoScalePadding, font, pnLDisplay);
		}

		public AtmStrategyIdentifier AtmStrategyIdentifier(ISeries<double> input, bool includeAutoScale, int autoScalePadding, SimpleFont font, AtmStrategyIdentifierTools.PnLDisplay pnLDisplay)
		{
			if (cacheAtmStrategyIdentifier != null)
				for (int idx = 0; idx < cacheAtmStrategyIdentifier.Length; idx++)
					if (cacheAtmStrategyIdentifier[idx] != null && cacheAtmStrategyIdentifier[idx].IncludeAutoScale == includeAutoScale && cacheAtmStrategyIdentifier[idx].AutoScalePadding == autoScalePadding && cacheAtmStrategyIdentifier[idx].Font == font && cacheAtmStrategyIdentifier[idx].PnLDisplay == pnLDisplay && cacheAtmStrategyIdentifier[idx].EqualsInput(input))
						return cacheAtmStrategyIdentifier[idx];
			return CacheIndicator<AtmStrategyIdentifier>(new AtmStrategyIdentifier(){ IncludeAutoScale = includeAutoScale, AutoScalePadding = autoScalePadding, Font = font, PnLDisplay = pnLDisplay }, input, ref cacheAtmStrategyIdentifier);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AtmStrategyIdentifier AtmStrategyIdentifier(bool includeAutoScale, int autoScalePadding, SimpleFont font, AtmStrategyIdentifierTools.PnLDisplay pnLDisplay)
		{
			return indicator.AtmStrategyIdentifier(Input, includeAutoScale, autoScalePadding, font, pnLDisplay);
		}

		public Indicators.AtmStrategyIdentifier AtmStrategyIdentifier(ISeries<double> input , bool includeAutoScale, int autoScalePadding, SimpleFont font, AtmStrategyIdentifierTools.PnLDisplay pnLDisplay)
		{
			return indicator.AtmStrategyIdentifier(input, includeAutoScale, autoScalePadding, font, pnLDisplay);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AtmStrategyIdentifier AtmStrategyIdentifier(bool includeAutoScale, int autoScalePadding, SimpleFont font, AtmStrategyIdentifierTools.PnLDisplay pnLDisplay)
		{
			return indicator.AtmStrategyIdentifier(Input, includeAutoScale, autoScalePadding, font, pnLDisplay);
		}

		public Indicators.AtmStrategyIdentifier AtmStrategyIdentifier(ISeries<double> input , bool includeAutoScale, int autoScalePadding, SimpleFont font, AtmStrategyIdentifierTools.PnLDisplay pnLDisplay)
		{
			return indicator.AtmStrategyIdentifier(input, includeAutoScale, autoScalePadding, font, pnLDisplay);
		}
	}
}

#endregion
