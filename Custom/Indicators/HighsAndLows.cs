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
    public class HighsAndLows : Indicator
    {
        private static string HIGHER_HIGH = "HH";
        private static string HIGHER_LOW = "HL";
        private static string LOWER_HIGH = "LH";
        private static string LOWER_LOW = "LL";

        private double PivotHigh = double.MinValue;
        private int PivotHighCandle = int.MinValue;
        private string PivotHighLabel = string.Empty;

        private double PivotLow = double.MaxValue;
        private int PivotLowCandle = int.MinValue;
        private string PivotLowLabel = string.Empty;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Plots higher highs, lower highs, higher lows, and lower lows";
                Name = "Highs and Lows";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                BarsToLookBack = 10;
                BarsToLookAhead = 10;
                TextColor = Brushes.Yellow;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < (BarsToLookBack + BarsToLookAhead) + 1)
                return;

            double currentHigh = High[BarsToLookAhead];

            bool isPivotHigh = true;
            for (int i = 0; i < BarsToLookAhead; i++)
            {
                if (High[i] > currentHigh)
                {
                    isPivotHigh = false;
                    break;
                }
            }

            if (isPivotHigh)
            {
                for (int i = 0; i < BarsToLookBack; i++)
                {
                    if (High[BarsToLookAhead + (i + 1)] > currentHigh)
                    {
                        isPivotHigh = false;
                        break;
                    }
                }
            }

            if (isPivotHigh)
            {
                if ((CurrentBar - PivotHighCandle) < 5 && currentHigh == PivotHigh)
                {
                    RemoveDrawObject(PivotHighLabel + "@" + PivotHighCandle);
                }

                if (currentHigh > PivotHigh)
                {
                    PivotHighLabel = HIGHER_HIGH;
                }
                else if (currentHigh < PivotHigh)
                {
                    PivotHighLabel = LOWER_HIGH;
                }

                PivotHigh = currentHigh;
                PivotHighCandle = CurrentBar;

                Draw.Text(this, PivotHighLabel + "@" + PivotHighCandle, PivotHighLabel, BarsToLookAhead, PivotHigh + TickSize, TextColor);
            }

            double currentLow = Low[BarsToLookAhead];

            bool isPivotLow = true;
            for (int i = 0; i < BarsToLookAhead; i++)
            {
                if (Low[i] < currentLow)
                {
                    isPivotLow = false;
                    break;
                }
            }

            if (isPivotLow)
            {
                for (int i = 0; i < BarsToLookBack; i++)
                {
                    if (Low[BarsToLookAhead + (i + 1)] < currentLow)
                    {
                        isPivotLow = false;
                        break;
                    }
                }
            }

            if (isPivotLow)
            {
                if ((CurrentBar - PivotLowCandle) < 5 && currentLow == PivotLow)
                {
                    RemoveDrawObject(PivotLowLabel + "@" + PivotLowCandle);
                }
                
                if (currentLow < PivotLow)
                {
                    PivotLowLabel = LOWER_LOW;
                }
                else if (currentLow > PivotLow)
                {
                    PivotLowLabel = HIGHER_LOW;
                }

                PivotLow = currentLow;
                PivotLowCandle = CurrentBar;

                Draw.Text(this, PivotLowLabel + "@" + PivotLowCandle, PivotLowLabel, BarsToLookAhead, PivotLow - TickSize, TextColor);
            }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Look Back Period", Description = "Number of bars to look backward when determining pivots", Order = 1, GroupName = "Parameters")]
        public int BarsToLookBack
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Look Ahead Period", Description = "Number of bars to look forward when determining pivots", Order = 2, GroupName = "Parameters")]
        public int BarsToLookAhead
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Text Color", Description = "Color of the text used to label the pivots", Order = 3, GroupName = "Parameters")]
        public Brush TextColor
        { get; set; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HighsAndLows[] cacheHighsAndLows;
		public HighsAndLows HighsAndLows(int barsToLookBack, int barsToLookAhead, Brush textColor)
		{
			return HighsAndLows(Input, barsToLookBack, barsToLookAhead, textColor);
		}

		public HighsAndLows HighsAndLows(ISeries<double> input, int barsToLookBack, int barsToLookAhead, Brush textColor)
		{
			if (cacheHighsAndLows != null)
				for (int idx = 0; idx < cacheHighsAndLows.Length; idx++)
					if (cacheHighsAndLows[idx] != null && cacheHighsAndLows[idx].BarsToLookBack == barsToLookBack && cacheHighsAndLows[idx].BarsToLookAhead == barsToLookAhead && cacheHighsAndLows[idx].TextColor == textColor && cacheHighsAndLows[idx].EqualsInput(input))
						return cacheHighsAndLows[idx];
			return CacheIndicator<HighsAndLows>(new HighsAndLows(){ BarsToLookBack = barsToLookBack, BarsToLookAhead = barsToLookAhead, TextColor = textColor }, input, ref cacheHighsAndLows);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HighsAndLows HighsAndLows(int barsToLookBack, int barsToLookAhead, Brush textColor)
		{
			return indicator.HighsAndLows(Input, barsToLookBack, barsToLookAhead, textColor);
		}

		public Indicators.HighsAndLows HighsAndLows(ISeries<double> input , int barsToLookBack, int barsToLookAhead, Brush textColor)
		{
			return indicator.HighsAndLows(input, barsToLookBack, barsToLookAhead, textColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HighsAndLows HighsAndLows(int barsToLookBack, int barsToLookAhead, Brush textColor)
		{
			return indicator.HighsAndLows(Input, barsToLookBack, barsToLookAhead, textColor);
		}

		public Indicators.HighsAndLows HighsAndLows(ISeries<double> input , int barsToLookBack, int barsToLookAhead, Brush textColor)
		{
			return indicator.HighsAndLows(input, barsToLookBack, barsToLookAhead, textColor);
		}
	}
}

#endregion
