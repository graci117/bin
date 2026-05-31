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
	public class SampleDrawTextExample : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SampleDrawTextExample";
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
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			
			Draw.Text(this, "tag1", "Test", 0, Low[0] - 10 * TickSize, Brushes.DodgerBlue);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SampleDrawTextExample[] cacheSampleDrawTextExample;
		public SampleDrawTextExample SampleDrawTextExample()
		{
			return SampleDrawTextExample(Input);
		}

		public SampleDrawTextExample SampleDrawTextExample(ISeries<double> input)
		{
			if (cacheSampleDrawTextExample != null)
				for (int idx = 0; idx < cacheSampleDrawTextExample.Length; idx++)
					if (cacheSampleDrawTextExample[idx] != null &&  cacheSampleDrawTextExample[idx].EqualsInput(input))
						return cacheSampleDrawTextExample[idx];
			return CacheIndicator<SampleDrawTextExample>(new SampleDrawTextExample(), input, ref cacheSampleDrawTextExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SampleDrawTextExample SampleDrawTextExample()
		{
			return indicator.SampleDrawTextExample(Input);
		}

		public Indicators.SampleDrawTextExample SampleDrawTextExample(ISeries<double> input )
		{
			return indicator.SampleDrawTextExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SampleDrawTextExample SampleDrawTextExample()
		{
			return indicator.SampleDrawTextExample(Input);
		}

		public Indicators.SampleDrawTextExample SampleDrawTextExample(ISeries<double> input )
		{
			return indicator.SampleDrawTextExample(input);
		}
	}
}

#endregion
