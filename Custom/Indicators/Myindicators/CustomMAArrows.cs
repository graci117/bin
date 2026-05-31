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
namespace NinjaTrader.NinjaScript.Indicators.Myindicators
{
    public class CustomMAArrows : Indicator
    {
        [NinjaScriptProperty]
        [Range(1, int.MaxValue), Display(Name="Period 1", Order=2, GroupName="Parameters")]
        public int Period1 { get; set; }
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue), Display(Name="Period 2", Order=4, GroupName="Parameters")]
        public int Period2 { get; set; }
        
        private T3 MA1;
        private T3 MA2;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Indicator that plots arrows when two T3 moving averages cross and plots the moving averages on the chart.";
                Name = "CustomMAArrows";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                Period1 = 8;
                Period2 = 21;

                AddPlot(Brushes.Blue, "MA1"); // Plot for MA1
                AddPlot(Brushes.Red, "MA2");  // Plot for MA2
            }
            else if (State == State.Configure)
            {
                MA1 = T3(Period1, 3, 0.7);
                MA2 = T3(Period2, 3, 0.7);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(Period1, Period2)) return;

            // Plot the moving averages
            Values[0][0] = MA1[0];
            Values[1][0] = MA2[0];

            // Draw arrows for cross events
            if (CrossAbove(MA1, MA2, 1))
                Draw.ArrowUp(this, "ARROWUP" + CurrentBar, true, 0, Low[0] - TickSize, Brushes.Lime);

            if (CrossBelow(MA1, MA2, 1))
                Draw.ArrowDown(this, "ARROWDOWN" + CurrentBar, true, 0, High[0] + TickSize, Brushes.Pink);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Myindicators.CustomMAArrows[] cacheCustomMAArrows;
		public Myindicators.CustomMAArrows CustomMAArrows(int period1, int period2)
		{
			return CustomMAArrows(Input, period1, period2);
		}

		public Myindicators.CustomMAArrows CustomMAArrows(ISeries<double> input, int period1, int period2)
		{
			if (cacheCustomMAArrows != null)
				for (int idx = 0; idx < cacheCustomMAArrows.Length; idx++)
					if (cacheCustomMAArrows[idx] != null && cacheCustomMAArrows[idx].Period1 == period1 && cacheCustomMAArrows[idx].Period2 == period2 && cacheCustomMAArrows[idx].EqualsInput(input))
						return cacheCustomMAArrows[idx];
			return CacheIndicator<Myindicators.CustomMAArrows>(new Myindicators.CustomMAArrows(){ Period1 = period1, Period2 = period2 }, input, ref cacheCustomMAArrows);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Myindicators.CustomMAArrows CustomMAArrows(int period1, int period2)
		{
			return indicator.CustomMAArrows(Input, period1, period2);
		}

		public Indicators.Myindicators.CustomMAArrows CustomMAArrows(ISeries<double> input , int period1, int period2)
		{
			return indicator.CustomMAArrows(input, period1, period2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Myindicators.CustomMAArrows CustomMAArrows(int period1, int period2)
		{
			return indicator.CustomMAArrows(Input, period1, period2);
		}

		public Indicators.Myindicators.CustomMAArrows CustomMAArrows(ISeries<double> input , int period1, int period2)
		{
			return indicator.CustomMAArrows(input, period1, period2);
		}
	}
}

#endregion
