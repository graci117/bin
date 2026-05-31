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
	public class HighLowCandles30Secs : Indicator
	{
		EMA ema14;
		EMA ema10;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "HighLowCandles30Secs";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
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
				ema14 = EMA(Close, 28);
				ema10 = EMA(Close, 20);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 1)
            return;

        // Long condition
	        if (Close[1] < Open[1]            // Previous red
	            && Close[0] > Open[0]         // Current green
	            && Low[0] < Low[1]            // Lower low
	            && High[0] < High[1]         // Lower high
				&&  (
				ema14[0] > High[0] + (40*TickSize)
				|| Close[0] > ema14[0])
				&& Close[0] > ema10[0]
				)
				
	        {
//				Print("High[0] plus (10*TickSize)  --- " + (High[0]));
//				Print("High[0] plus (10*TickSize)  --- " + (High[0] + (40 * (TickSize))));
//				Print("ema14[0] --- " + ema14[0]);
	            Draw.ArrowUp(this, "Long"+CurrentBar, true, 0, Low[0] - TickSize, Brushes.Yellow);
	        }
	
	        // Short condition
	        if (Close[1] > Open[1]            // Previous green
	            && Close[0] < Open[0]         // Current red
	            && High[0] > High[1]          // Higher high
	            && Low[0] > Low[1]           // Higher low
				&&  (ema14[0] <  Low[0] -(40*TickSize)
					|| Close[0] < ema14[0])
				&& Close[0] < ema10[0]
				)
	        {
	            Draw.ArrowDown(this, "Short"+CurrentBar, true, 0, High[0] + TickSize, Brushes.Magenta);
	        }
   		 }
	}
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HighLowCandles30Secs[] cacheHighLowCandles30Secs;
		public HighLowCandles30Secs HighLowCandles30Secs()
		{
			return HighLowCandles30Secs(Input);
		}

		public HighLowCandles30Secs HighLowCandles30Secs(ISeries<double> input)
		{
			if (cacheHighLowCandles30Secs != null)
				for (int idx = 0; idx < cacheHighLowCandles30Secs.Length; idx++)
					if (cacheHighLowCandles30Secs[idx] != null &&  cacheHighLowCandles30Secs[idx].EqualsInput(input))
						return cacheHighLowCandles30Secs[idx];
			return CacheIndicator<HighLowCandles30Secs>(new HighLowCandles30Secs(), input, ref cacheHighLowCandles30Secs);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HighLowCandles30Secs HighLowCandles30Secs()
		{
			return indicator.HighLowCandles30Secs(Input);
		}

		public Indicators.HighLowCandles30Secs HighLowCandles30Secs(ISeries<double> input )
		{
			return indicator.HighLowCandles30Secs(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HighLowCandles30Secs HighLowCandles30Secs()
		{
			return indicator.HighLowCandles30Secs(Input);
		}

		public Indicators.HighLowCandles30Secs HighLowCandles30Secs(ISeries<double> input )
		{
			return indicator.HighLowCandles30Secs(input);
		}
	}
}

#endregion
