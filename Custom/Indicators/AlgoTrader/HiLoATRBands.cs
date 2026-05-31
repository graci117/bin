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
using NinjaTrader.NinjaScript.Indicators.AlgoTrader;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
    public class HiLoATRBands : Indicator
    {
        private Series<double> rawHighestHigh;
        private Series<double> rawLowestLow;
        private Series<double> rawMiddleLine;
        private HMA hmaHighestHigh;
        private HMA hmaLowestLow;
        private HMA hmaMiddleLine;
		private ATR atr;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Draws smoothed highest high and lowest low bands with multiple ATR-based levels and slope-colored centerline.";
                Name = "HiLo ATR Bands";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
				
				// Parameters
                LookbackPeriod = 20;
                SmoothingPeriod = 6;
                Width = 2;
				AtrPeriod = 100;
				AtrMultiplier = 1.25;

                // Plot Indices: 
				// 0: Smoothed HH (Top), 1: Smoothed LL (Bottom), 2: Mid
                AddPlot(Brushes.Cyan, "SmoothedHighestHigh");
                AddPlot(Brushes.Magenta, "SmoothedLowestLow");
                AddPlot(Brushes.Gray, "SmoothedMiddleLine");

                // Add 3 upper lines (Indices 3, 4, 5)
                AddPlot(Brushes.LightSkyBlue, "UpperBand1");
                AddPlot(Brushes.DodgerBlue, "UpperBand2");
                AddPlot(Brushes.RoyalBlue, "UpperBand3");

                // Add 3 lower lines (Indices 6, 7, 8)
                AddPlot(Brushes.Violet, "LowerBand1");
                AddPlot(Brushes.Orchid, "LowerBand2");
                AddPlot(Brushes.MediumOrchid, "LowerBand3");
            }
            else if (State == State.Configure)
            {
                Plots[0].Name = "Upper Band 4 (HH)";
                Plots[0].Width = Width;
                Plots[1].Name = "Lower Band 4 (LL)";
                Plots[1].Width = Width;
                Plots[2].Name = "Smoothed Mid";
                Plots[2].Width = Width;

                Plots[3].Name = "Upper Band 1";
                Plots[4].Name = "Upper Band 2";
                Plots[5].Name = "Upper Band 3";

                Plots[6].Name = "Lower Band 1";
                Plots[7].Name = "Lower Band 2";
                Plots[8].Name = "Lower Band 3";
            }
            else if (State == State.DataLoaded)
            {
                rawHighestHigh = new Series<double>(this, MaximumBarsLookBack.Infinite);
                rawLowestLow = new Series<double>(this, MaximumBarsLookBack.Infinite);
                rawMiddleLine = new Series<double>(this, MaximumBarsLookBack.Infinite);
                
				int actualSmoothingPeriod = Math.Max(1, SmoothingPeriod);
                hmaHighestHigh = HMA(rawHighestHigh, actualSmoothingPeriod);
                hmaLowestLow = HMA(rawLowestLow, actualSmoothingPeriod);
                hmaMiddleLine = HMA(rawMiddleLine, actualSmoothingPeriod);
				atr = ATR(AtrPeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(LookbackPeriod, AtrPeriod))
                return;

            rawHighestHigh[0] = MAX(High, LookbackPeriod)[0];
            rawLowestLow[0] = MIN(Low, LookbackPeriod)[0];
            rawMiddleLine[0] = (rawHighestHigh[0] + rawLowestLow[0]) / 2;

            double smoothedMid = hmaMiddleLine[0];
			double curAtr = atr[0];

            Values[2][0] = smoothedMid;

			// Color the Middle Line based on slope (Pointing Up = Lime, Down = Red)
			if (Values[2][0] > Values[2][1])
				PlotBrushes[2][0] = Brushes.Lime;
			else if (Values[2][0] < Values[2][1])
				PlotBrushes[2][0] = Brushes.Red;

            // Calculate ATR bands (0.8 apart like previous indicators)
            Values[3][0] = smoothedMid + (curAtr * AtrMultiplier * 1); // Upper 1
            Values[4][0] = smoothedMid + (curAtr * AtrMultiplier * 2); // Upper 2
            Values[5][0] = smoothedMid + (curAtr * AtrMultiplier * 3); // Upper 3
			Values[0][0] = smoothedMid + (curAtr * AtrMultiplier * 4); // Upper 4 (Smoothed HH)

            Values[6][0] = smoothedMid - (curAtr * AtrMultiplier * 1); // Lower 1
            Values[7][0] = smoothedMid - (curAtr * AtrMultiplier * 2); // Lower 2
            Values[8][0] = smoothedMid - (curAtr * AtrMultiplier * 3); // Lower 3
			Values[1][0] = smoothedMid - (curAtr * AtrMultiplier * 4); // Lower 4 (Smoothed LL)

            // Check for crosses on all lines
            for (int i = 0; i < 9; i++)
            {
                // Draw a cyan diamond EXACTLY on the band for a cross above
                if (CrossAbove(Close, Values[i], 1))
                {
                    Draw.Diamond(this, "CrossAbove" + i + CurrentBar, true, 0, Values[i][0], Brushes.Cyan);
                }

                // Draw a yellow diamond EXACTLY on the band for a cross below
                if (CrossBelow(Close, Values[i], 1))
                {
                    Draw.Diamond(this, "CrossBelow" + i + CurrentBar, true, 0, Values[i][0], Brushes.Yellow);
                }
            }
        }

        #region Properties
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "HiLo Lookback Period", Description = "Number of bars for HighestHigh/LowestLow calculation", Order = 1, GroupName = "Parameters")]
        public int LookbackPeriod { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Smoothing Period", Description = "Period for HMA smoothing of bands", Order = 2, GroupName = "Parameters")]
        public int SmoothingPeriod { get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "ATR Period", Description = "Period for ATR volatility calculation", Order = 3, GroupName = "Parameters")]
        public int AtrPeriod { get; set; }

		[Range(0.1, double.MaxValue), NinjaScriptProperty]
        [Display(Name = "ATR Step Multiplier", Description = "Distance between each band (e.g. 0.8)", Order = 4, GroupName = "Parameters")]
        public double AtrMultiplier { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Line Width", Order = 5, GroupName = "Parameters")]
        public int Width { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.HiLoATRBands[] cacheHiLoATRBands;
		public AlgoTrader.HiLoATRBands HiLoATRBands(int lookbackPeriod, int smoothingPeriod, int atrPeriod, double atrMultiplier, int width)
		{
			return HiLoATRBands(Input, lookbackPeriod, smoothingPeriod, atrPeriod, atrMultiplier, width);
		}

		public AlgoTrader.HiLoATRBands HiLoATRBands(ISeries<double> input, int lookbackPeriod, int smoothingPeriod, int atrPeriod, double atrMultiplier, int width)
		{
			if (cacheHiLoATRBands != null)
				for (int idx = 0; idx < cacheHiLoATRBands.Length; idx++)
					if (cacheHiLoATRBands[idx] != null && cacheHiLoATRBands[idx].LookbackPeriod == lookbackPeriod && cacheHiLoATRBands[idx].SmoothingPeriod == smoothingPeriod && cacheHiLoATRBands[idx].AtrPeriod == atrPeriod && cacheHiLoATRBands[idx].AtrMultiplier == atrMultiplier && cacheHiLoATRBands[idx].Width == width && cacheHiLoATRBands[idx].EqualsInput(input))
						return cacheHiLoATRBands[idx];
			return CacheIndicator<AlgoTrader.HiLoATRBands>(new AlgoTrader.HiLoATRBands(){ LookbackPeriod = lookbackPeriod, SmoothingPeriod = smoothingPeriod, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier, Width = width }, input, ref cacheHiLoATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.HiLoATRBands HiLoATRBands(int lookbackPeriod, int smoothingPeriod, int atrPeriod, double atrMultiplier, int width)
		{
			return indicator.HiLoATRBands(Input, lookbackPeriod, smoothingPeriod, atrPeriod, atrMultiplier, width);
		}

		public Indicators.AlgoTrader.HiLoATRBands HiLoATRBands(ISeries<double> input , int lookbackPeriod, int smoothingPeriod, int atrPeriod, double atrMultiplier, int width)
		{
			return indicator.HiLoATRBands(input, lookbackPeriod, smoothingPeriod, atrPeriod, atrMultiplier, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.HiLoATRBands HiLoATRBands(int lookbackPeriod, int smoothingPeriod, int atrPeriod, double atrMultiplier, int width)
		{
			return indicator.HiLoATRBands(Input, lookbackPeriod, smoothingPeriod, atrPeriod, atrMultiplier, width);
		}

		public Indicators.AlgoTrader.HiLoATRBands HiLoATRBands(ISeries<double> input , int lookbackPeriod, int smoothingPeriod, int atrPeriod, double atrMultiplier, int width)
		{
			return indicator.HiLoATRBands(input, lookbackPeriod, smoothingPeriod, atrPeriod, atrMultiplier, width);
		}
	}
}

#endregion
