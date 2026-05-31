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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ClickLimitOrderIndicatorExpanded : Indicator
	{
		private Account myAccount;
		private ChartScale MyChartScale;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ClickLimitOrderIndicatorExpanded";
				Calculate									= Calculate.OnPriceChange;
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
				AccountName 								= "Sim101";
			}
		 	else if (State == State.DataLoaded)
  			{
   				if (ChartControl != null)
    				ChartControl.MouseLeftButtonDown += LeftMouseDown;
				
				if (ChartControl != null)
					ChartPanel.MouseMove += ChartControl_MouseMove;
				
				// Find our account
				lock (Account.All)
					myAccount = Account.All.FirstOrDefault(a => a.Name == AccountName);
				
				// Subscribe to account item and order updates
		        if (myAccount != null)
				{
					myAccount.OrderUpdate 		+= OnOrderUpdate;
					myAccount.ExecutionUpdate 	+= OnExecutionUpdate;
					myAccount.PositionUpdate	+= OnPositionUpdate;
				}
   			}
   			else if (State == State.Terminated)
   			{
	    		if (ChartControl != null)
	     			ChartControl.MouseLeftButtonDown -= LeftMouseDown;
				
				if (ChartControl != null)
					ChartPanel.MouseMove -= ChartControl_MouseMove;
				
				if (myAccount != null)
				{
					myAccount.OrderUpdate 		-= OnOrderUpdate;
					myAccount.ExecutionUpdate 	-= OnExecutionUpdate;
					myAccount.PositionUpdate	-= OnPositionUpdate;
				}
   			}
		}
		
		protected override void OnBarUpdate()
		{
			
		}
		
		private void OnOrderUpdate(object sender, OrderEventArgs e)
	    {
			NinjaTrader.Code.Output.Process("", PrintTo.OutputTab1);
			NinjaTrader.Code.Output.Process("OrderUpdate", PrintTo.OutputTab1);
	        // Output the order
	        NinjaTrader.Code.Output.Process(e.Order.ToString(), PrintTo.OutputTab1);
	    }
		
		private void OnExecutionUpdate(object sender, ExecutionEventArgs e)
	    {
			NinjaTrader.Code.Output.Process("", PrintTo.OutputTab1);
			NinjaTrader.Code.Output.Process("ExecutionUpdate", PrintTo.OutputTab1);
	        // Output the execution
	        NinjaTrader.Code.Output.Process(string.Format("Instrument: {0} Quantity: {1} Price: {2}",
	              e.Execution.Instrument.FullName, e.Quantity, e.Price), PrintTo.OutputTab1);
	    }
		private void OnPositionUpdate(object sender, PositionEventArgs e)
	    {
			NinjaTrader.Code.Output.Process("", PrintTo.OutputTab1);
			NinjaTrader.Code.Output.Process("PositionUpdate", PrintTo.OutputTab1);
	        // Output the order
	        NinjaTrader.Code.Output.Process(e.Position.ToString(), PrintTo.OutputTab1);
	    }
		
		protected void LeftMouseDown(object sender, MouseButtonEventArgs e)
		{
			TriggerCustomEvent(o =>
			{
				int Y = ChartingExtensions.ConvertToVerticalPixels(e.GetPosition(ChartControl as IInputElement).Y, ChartControl.PresentationSource);
				
				double priceClicked = MyChartScale.GetValueByY(Y);
			
				Order limitOrder = null;
				Order stopOrder = null;

				if (priceClicked > Close[0])
				{
					limitOrder = myAccount.CreateOrder(Instrument, OrderAction.Sell, OrderType.Limit, OrderEntry.Manual, TimeInForce.Day, 1, priceClicked, 0, "", "limitOrder"+DateTime.Now.ToString(), DateTime.MaxValue, null);
					stopOrder  = myAccount.CreateOrder(Instrument, OrderAction.Buy, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, 1, 0, priceClicked, "", "stopOrder"+DateTime.Now.ToString(), DateTime.MaxValue, null);
				}
				else
				{
					limitOrder = myAccount.CreateOrder(Instrument, OrderAction.Buy, OrderType.Limit, OrderEntry.Manual, TimeInForce.Day, 1, priceClicked, 0, "", "limitOrder"+DateTime.Now.ToString(), DateTime.MaxValue, null);
					stopOrder  = myAccount.CreateOrder(Instrument, OrderAction.Sell, OrderType.StopMarket, OrderEntry.Manual, TimeInForce.Day, 1, 0, priceClicked, "", "stopOrder"+DateTime.Now.ToString(), DateTime.MaxValue, null);
				}
				
				myAccount.Submit(new[] { limitOrder, stopOrder });
			}, null);
			e.Handled = true;
		}
		
		void ChartControl_MouseMove (object sender, System.Windows.Input.MouseEventArgs e)
		{

		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			MyChartScale = chartScale;
		}
		
		[TypeConverter(typeof(NinjaTrader.NinjaScript.AccountNameConverter))]
		public string AccountName { get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ClickLimitOrderIndicatorExpanded[] cacheClickLimitOrderIndicatorExpanded;
		public ClickLimitOrderIndicatorExpanded ClickLimitOrderIndicatorExpanded()
		{
			return ClickLimitOrderIndicatorExpanded(Input);
		}

		public ClickLimitOrderIndicatorExpanded ClickLimitOrderIndicatorExpanded(ISeries<double> input)
		{
			if (cacheClickLimitOrderIndicatorExpanded != null)
				for (int idx = 0; idx < cacheClickLimitOrderIndicatorExpanded.Length; idx++)
					if (cacheClickLimitOrderIndicatorExpanded[idx] != null &&  cacheClickLimitOrderIndicatorExpanded[idx].EqualsInput(input))
						return cacheClickLimitOrderIndicatorExpanded[idx];
			return CacheIndicator<ClickLimitOrderIndicatorExpanded>(new ClickLimitOrderIndicatorExpanded(), input, ref cacheClickLimitOrderIndicatorExpanded);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ClickLimitOrderIndicatorExpanded ClickLimitOrderIndicatorExpanded()
		{
			return indicator.ClickLimitOrderIndicatorExpanded(Input);
		}

		public Indicators.ClickLimitOrderIndicatorExpanded ClickLimitOrderIndicatorExpanded(ISeries<double> input )
		{
			return indicator.ClickLimitOrderIndicatorExpanded(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ClickLimitOrderIndicatorExpanded ClickLimitOrderIndicatorExpanded()
		{
			return indicator.ClickLimitOrderIndicatorExpanded(Input);
		}

		public Indicators.ClickLimitOrderIndicatorExpanded ClickLimitOrderIndicatorExpanded(ISeries<double> input )
		{
			return indicator.ClickLimitOrderIndicatorExpanded(input);
		}
	}
}

#endregion
