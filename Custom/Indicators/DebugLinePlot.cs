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
	public class DebugLinePlot : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DebugLinePlot";
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
				 AddPlot(Brushes.DarkGray, "Middle");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1) return;

            // A simple constant line at a fixed price level (like Close[0] or some constant value)
            Middle[0] = Close[0];  // Use closing price for simplicity

            // Diagnostic print statements to verify key information
            Print($"CurrentBar: {CurrentBar}, Close[0]: {Close[0]}");
		}
		
		[Browsable(false)]
        [XmlIgnore()]
        public Series<double> Middle
        {
            get { return Values[0]; }
        }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DebugLinePlot[] cacheDebugLinePlot;
		public DebugLinePlot DebugLinePlot()
		{
			return DebugLinePlot(Input);
		}

		public DebugLinePlot DebugLinePlot(ISeries<double> input)
		{
			if (cacheDebugLinePlot != null)
				for (int idx = 0; idx < cacheDebugLinePlot.Length; idx++)
					if (cacheDebugLinePlot[idx] != null &&  cacheDebugLinePlot[idx].EqualsInput(input))
						return cacheDebugLinePlot[idx];
			return CacheIndicator<DebugLinePlot>(new DebugLinePlot(), input, ref cacheDebugLinePlot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DebugLinePlot DebugLinePlot()
		{
			return indicator.DebugLinePlot(Input);
		}

		public Indicators.DebugLinePlot DebugLinePlot(ISeries<double> input )
		{
			return indicator.DebugLinePlot(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DebugLinePlot DebugLinePlot()
		{
			return indicator.DebugLinePlot(Input);
		}

		public Indicators.DebugLinePlot DebugLinePlot(ISeries<double> input )
		{
			return indicator.DebugLinePlot(input);
		}
	}
}

#endregion
