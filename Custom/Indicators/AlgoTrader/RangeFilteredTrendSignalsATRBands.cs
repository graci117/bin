#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class RangeFilteredTrendSignalsATRBands : Indicator
	{
		private Series<double> kalmanV1;
		private Series<int> supertrendDir;
		private Series<double> upperBand;
        private Series<double> lowerBand;
		private ATR atr;
		private WMA vola;
		private Series<double> highLowRange;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Range Filtered Trend Signals with 6 ATR Rainbow Bands, Small Envelope Dots, and Large Entry Dots.";
				Name										= "Range Filtered Trend Signals ATR Bands";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				
				TrendState = new Series<double>(this, MaximumBarsLookBack.Infinite);
				IsRanging = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				BandsEnabled 		= true;
				KalmanAlpha 		= 0.01;
				KalmanBeta  		= 0.1;
				KalmanPeriod 		= 77;
				Dev 				= 1.2;
				SupertrendFactor 	= 0.7;
				SupertrendAtrPeriod = 7;
				AtrMultiplier 		= 1.25; 
				AtrLength 			= 100;

				// --- SIGNAL PLOTS (0-7) ---
				AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "Kalman"); // 0
				AddPlot(new Stroke(Brushes.Gray, 3), PlotStyle.Line, "KalmanLine"); // 1
				
				// Small Envelope Dots (2-5)
				AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Dot, "UpperBandPlot"); // 2
				AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Dot, "UpperBandPlot2"); // 3
				AddPlot(new Stroke(Brushes.LimeGreen, 3), PlotStyle.Dot, "LowerBandPlot"); // 4
				AddPlot(new Stroke(Brushes.LimeGreen, 3), PlotStyle.Dot, "LowerBandPlot2"); // 5
				
				// Large Entry Dots on Midline (6-7)
				AddPlot(new Stroke(Brushes.Lime, 8), PlotStyle.Dot, "LongDot"); // 6
				AddPlot(new Stroke(Brushes.Red, 8), PlotStyle.Dot, "ShortDot"); // 7
				
				// --- ATR BANDS (8-19) - RAINBOW GRADIENT TOP TO BOTTOM ---
				AddPlot(new Stroke(Brushes.SpringGreen, 1), PlotStyle.Line, "Up1"); // 8
				AddPlot(new Stroke(Brushes.DeepSkyBlue, 1), PlotStyle.Line, "Dn1"); // 9
				AddPlot(new Stroke(Brushes.YellowGreen, 1), PlotStyle.Line, "Up2"); // 10
				AddPlot(new Stroke(Brushes.RoyalBlue, 1),   PlotStyle.Line, "Dn2"); // 11
				AddPlot(new Stroke(Brushes.Yellow, 1),      PlotStyle.Line, "Up3"); // 12
				AddPlot(new Stroke(Brushes.Blue, 1),        PlotStyle.Line, "Dn3"); // 13
				AddPlot(new Stroke(Brushes.Orange, 2),      PlotStyle.Line, "Up4"); // 14
				AddPlot(new Stroke(Brushes.BlueViolet, 2),  PlotStyle.Line, "Dn4"); // 15
				AddPlot(new Stroke(Brushes.OrangeRed, 2),   PlotStyle.Line, "Up5"); // 16
				AddPlot(new Stroke(Brushes.Purple, 2),      PlotStyle.Line, "Dn5"); // 17
				AddPlot(new Stroke(Brushes.Red, 3),         PlotStyle.Line, "Up6"); // 18 - Top
				AddPlot(new Stroke(Brushes.DarkMagenta, 3), PlotStyle.Line, "Dn6"); // 19 - Bottom
			}
			else if (State == State.Configure)
			{
				kalmanV1 		= new Series<double>(this);
				supertrendDir 	= new Series<int>(this);
				upperBand 		= new Series<double>(this);
                lowerBand 		= new Series<double>(this);
				highLowRange 	= new Series<double>(this);
			}
			else if(State == State.DataLoaded)
			{
				atr = ATR(SupertrendAtrPeriod);
				vola = WMA(highLowRange, 200);
			}
		}

		protected override void OnBarUpdate()
		{
			// Ensure enough bars for calculations (Wait for Vola and Kalman)
			if (CurrentBar < 201) { 
				kalmanV1[0] = Close[0]; 
				supertrendDir[0] = 1; 
				highLowRange[0] = High[0] - Low[0];
				return; 
			}
			
			highLowRange[0] = High[0] - Low[0];
			Values[0][0] = kalmanV1[0] = kalmanV1[1] + KalmanAlpha * (Close[0] - kalmanV1[1]);
			
			upperBand[0] = kalmanV1[0] + atr[0] * SupertrendFactor;
			lowerBand[0] = kalmanV1[0] - atr[0] * SupertrendFactor;
			
			if (Close[0] > upperBand[1]) supertrendDir[0] = 1;
			else if (Close[0] < lowerBand[1]) supertrendDir[0] = -1;
			else supertrendDir[0] = supertrendDir[1];
			
			double upperVola = kalmanV1[0] + vola[0] * Dev;
			double lowerVola = kalmanV1[0] - vola[0] * Dev;
			
			int trend = 0;
			if (Close[0] > upperVola) trend = 1;
			else if (Close[0] < lowerVola) trend = -1;
			
			int ktrend = (supertrendDir[0] < 0) ? 1 : -1;
			int ktimes = ktrend * trend;
			bool isRanging = (ktimes == 1) || (trend == 0);

			// Midline Kalman Color
			if (isRanging) PlotBrushes[0][0] = Brushes.Gray;
			else PlotBrushes[0][0] = (trend == 1) ? Brushes.Lime : Brushes.Red;

			// --- SMALL ENVELOPE DOTS (Mirroring Logic of Reference file) ---
			// Plot 1 (KalmanLine)
			if (ktimes == 1) Values[1][0] = kalmanV1[0]; else Values[1][0] = double.NaN;
			
			// Top Small Red Dots (Plot 2 & 3)
			Values[2][0] = (trend == 0 || ktimes == 1) ? upperVola : double.NaN;
			Values[3][0] = (trend == -1 || ktimes == 1) ? upperVola : double.NaN;
			
			// Bottom Small Lime Dots (Plot 4 & 5)
			Values[4][0] = (trend == 0 || ktimes == 1) ? lowerVola : double.NaN;
			Values[5][0] = (trend == 1 || ktimes == 1) ? lowerVola : double.NaN;

			// --- LARGE ENTRY DOTS ---
			// These appear on the midline when a transition occurs
			bool longTrigger = Double.IsNaN(Values[5][1]) && !Double.IsNaN(Values[5][0]);
			bool shortTrigger = Double.IsNaN(Values[3][1]) && !Double.IsNaN(Values[3][0]);

			Values[6][0] = (longTrigger) ? kalmanV1[0] : double.NaN;
			Values[7][0] = (shortTrigger) ? kalmanV1[0] : double.NaN;

			// --- ATR BANDS (8-19) ---
			if(BandsEnabled)
			{
				double atrB = ATR(AtrLength)[0];
				Values[8][0]  = kalmanV1[0] + (atrB * AtrMultiplier * 1.0);       
				Values[9][0]  = kalmanV1[0] - (atrB * AtrMultiplier * 1.0);       
				Values[10][0] = kalmanV1[0] + (atrB * AtrMultiplier * 2.0); 
				Values[11][0] = kalmanV1[0] - (atrB * AtrMultiplier * 2.0); 
				Values[12][0] = kalmanV1[0] + (atrB * AtrMultiplier * 3.0); 
				Values[13][0] = kalmanV1[0] - (atrB * AtrMultiplier * 3.0); 
				Values[14][0] = kalmanV1[0] + (atrB * AtrMultiplier * 4.0); 
				Values[15][0] = kalmanV1[0] - (atrB * AtrMultiplier * 4.0); 
				Values[16][0] = kalmanV1[0] + (atrB * AtrMultiplier * 5.0); 
				Values[17][0] = kalmanV1[0] - (atrB * AtrMultiplier * 5.0); 
				Values[18][0] = kalmanV1[0] + (atrB * AtrMultiplier * 6.0); 
				Values[19][0] = kalmanV1[0] - (atrB * AtrMultiplier * 6.0); 
			}
			
			TrendState[0] = trend;
			IsRanging[0] = isRanging ? 1 : 0;
			
			// --- DIAMOND CROSS MARKERS FOR BANDS ---
			int[] indices = { 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };
			foreach (int i in indices)
			{
				if (CrossAbove(Close, Values[i], 1))
					Draw.Diamond(this, "CrossUp" + i + CurrentBar, true, 0, Values[i][0], Brushes.Cyan);
				else if (CrossBelow(Close, Values[i], 1))
					Draw.Diamond(this, "CrossDn" + i + CurrentBar, true, 0, Values[i][0], Brushes.Yellow);
			}
		}

		#region Properties
		[NinjaScriptProperty] [Display(Name="Show Bands", GroupName="Parameters", Order=1)] public bool BandsEnabled { get; set; }
		[NinjaScriptProperty] [Display(Name="Kalman Alpha", GroupName="Parameters", Order=2)] public double KalmanAlpha { get; set; }
		[NinjaScriptProperty] [Display(Name="Kalman Beta", GroupName="Parameters", Order=3)] public double KalmanBeta { get; set; }
		[NinjaScriptProperty] [Display(Name="Kalman Period", GroupName="Parameters", Order=4)] public int KalmanPeriod { get; set; }
		[NinjaScriptProperty] [Display(Name="Dev Multiplier", GroupName="Parameters", Order=5)] public double Dev { get; set; }
		[NinjaScriptProperty] [Display(Name="Supertrend Factor", GroupName="Parameters", Order=6)] public double SupertrendFactor { get; set; }
		[NinjaScriptProperty] [Display(Name="Supertrend ATR", GroupName="Parameters", Order=7)] public int SupertrendAtrPeriod { get; set; }
		
		[NinjaScriptProperty] [Display(Name="ATR Multiplier", GroupName="ATR Bands")] public double AtrMultiplier { get; set; }
		[NinjaScriptProperty] [Display(Name="ATR Length", GroupName="ATR Bands")] public int AtrLength { get; set; }
		
		[Browsable(false)] [XmlIgnore] public Series<double> TrendState { get; private set; }
		[Browsable(false)] [XmlIgnore] public Series<double> IsRanging { get; private set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.RangeFilteredTrendSignalsATRBands[] cacheRangeFilteredTrendSignalsATRBands;
		public AlgoTrader.RangeFilteredTrendSignalsATRBands RangeFilteredTrendSignalsATRBands(bool bandsEnabled, double kalmanAlpha, double kalmanBeta, int kalmanPeriod, double dev, double supertrendFactor, int supertrendAtrPeriod, double atrMultiplier, int atrLength)
		{
			return RangeFilteredTrendSignalsATRBands(Input, bandsEnabled, kalmanAlpha, kalmanBeta, kalmanPeriod, dev, supertrendFactor, supertrendAtrPeriod, atrMultiplier, atrLength);
		}

		public AlgoTrader.RangeFilteredTrendSignalsATRBands RangeFilteredTrendSignalsATRBands(ISeries<double> input, bool bandsEnabled, double kalmanAlpha, double kalmanBeta, int kalmanPeriod, double dev, double supertrendFactor, int supertrendAtrPeriod, double atrMultiplier, int atrLength)
		{
			if (cacheRangeFilteredTrendSignalsATRBands != null)
				for (int idx = 0; idx < cacheRangeFilteredTrendSignalsATRBands.Length; idx++)
					if (cacheRangeFilteredTrendSignalsATRBands[idx] != null && cacheRangeFilteredTrendSignalsATRBands[idx].BandsEnabled == bandsEnabled && cacheRangeFilteredTrendSignalsATRBands[idx].KalmanAlpha == kalmanAlpha && cacheRangeFilteredTrendSignalsATRBands[idx].KalmanBeta == kalmanBeta && cacheRangeFilteredTrendSignalsATRBands[idx].KalmanPeriod == kalmanPeriod && cacheRangeFilteredTrendSignalsATRBands[idx].Dev == dev && cacheRangeFilteredTrendSignalsATRBands[idx].SupertrendFactor == supertrendFactor && cacheRangeFilteredTrendSignalsATRBands[idx].SupertrendAtrPeriod == supertrendAtrPeriod && cacheRangeFilteredTrendSignalsATRBands[idx].AtrMultiplier == atrMultiplier && cacheRangeFilteredTrendSignalsATRBands[idx].AtrLength == atrLength && cacheRangeFilteredTrendSignalsATRBands[idx].EqualsInput(input))
						return cacheRangeFilteredTrendSignalsATRBands[idx];
			return CacheIndicator<AlgoTrader.RangeFilteredTrendSignalsATRBands>(new AlgoTrader.RangeFilteredTrendSignalsATRBands(){ BandsEnabled = bandsEnabled, KalmanAlpha = kalmanAlpha, KalmanBeta = kalmanBeta, KalmanPeriod = kalmanPeriod, Dev = dev, SupertrendFactor = supertrendFactor, SupertrendAtrPeriod = supertrendAtrPeriod, AtrMultiplier = atrMultiplier, AtrLength = atrLength }, input, ref cacheRangeFilteredTrendSignalsATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.RangeFilteredTrendSignalsATRBands RangeFilteredTrendSignalsATRBands(bool bandsEnabled, double kalmanAlpha, double kalmanBeta, int kalmanPeriod, double dev, double supertrendFactor, int supertrendAtrPeriod, double atrMultiplier, int atrLength)
		{
			return indicator.RangeFilteredTrendSignalsATRBands(Input, bandsEnabled, kalmanAlpha, kalmanBeta, kalmanPeriod, dev, supertrendFactor, supertrendAtrPeriod, atrMultiplier, atrLength);
		}

		public Indicators.AlgoTrader.RangeFilteredTrendSignalsATRBands RangeFilteredTrendSignalsATRBands(ISeries<double> input , bool bandsEnabled, double kalmanAlpha, double kalmanBeta, int kalmanPeriod, double dev, double supertrendFactor, int supertrendAtrPeriod, double atrMultiplier, int atrLength)
		{
			return indicator.RangeFilteredTrendSignalsATRBands(input, bandsEnabled, kalmanAlpha, kalmanBeta, kalmanPeriod, dev, supertrendFactor, supertrendAtrPeriod, atrMultiplier, atrLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.RangeFilteredTrendSignalsATRBands RangeFilteredTrendSignalsATRBands(bool bandsEnabled, double kalmanAlpha, double kalmanBeta, int kalmanPeriod, double dev, double supertrendFactor, int supertrendAtrPeriod, double atrMultiplier, int atrLength)
		{
			return indicator.RangeFilteredTrendSignalsATRBands(Input, bandsEnabled, kalmanAlpha, kalmanBeta, kalmanPeriod, dev, supertrendFactor, supertrendAtrPeriod, atrMultiplier, atrLength);
		}

		public Indicators.AlgoTrader.RangeFilteredTrendSignalsATRBands RangeFilteredTrendSignalsATRBands(ISeries<double> input , bool bandsEnabled, double kalmanAlpha, double kalmanBeta, int kalmanPeriod, double dev, double supertrendFactor, int supertrendAtrPeriod, double atrMultiplier, int atrLength)
		{
			return indicator.RangeFilteredTrendSignalsATRBands(input, bandsEnabled, kalmanAlpha, kalmanBeta, kalmanPeriod, dev, supertrendFactor, supertrendAtrPeriod, atrMultiplier, atrLength);
		}
	}
}

#endregion
