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

public enum MovingAverageType
{
	Simple,
	Exponential,
	Hull,
	Variable
}

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class DeviationTrendProfile : Indicator
	{
		private SMA sma;
		private EMA ema;
		private HMA hma;
		private VMA vma;
		private ATR atr;
		private Series<double> avg;
		private Series<double> trend;
		
		private Series<double> avgDiffSeries;
		private Series<double> avgColNormSeries;
		
		private double upThreshold = 0.55;
		private double downThreshold = 0.45;

		private Brush[] rainbowBrushes;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Deviation Trend Profile with rainbow bands and unified ATR steps.";
				Name										= "Deviation Trend Profile";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;

				ColorBar									= true;
				Length										= 77;
				VolatilityPeriod							= 14;
				AtrMultiplier								= 1.25; 
			    AtrPeriod             						= 100; 
				NormalizationLookback						= 200;
				ShowTrendSignals							= true;
				AverageType									= MovingAverageType.Exponential;

				RangingColor								= Brushes.Gray;
				UpColor 									= Brushes.Lime;
				DownColor 									= Brushes.Red;

				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Stdv1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Stdv2");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Stdv3");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "Stdv4");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Stdv_1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Stdv_2");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Stdv_3");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "Stdv_4");
				AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Line, "Avg");
			}
			else if (State == State.Configure)
			{
				avg 				= new Series<double>(this);
				trend 				= new Series<double>(this);
				avgDiffSeries 		= new Series<double>(this);
				avgColNormSeries 	= new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{
				switch (AverageType)
				{
					case MovingAverageType.Simple: 		sma = SMA(Length); 					break;
					case MovingAverageType.Exponential: ema = EMA(Length); 					break;
					case MovingAverageType.Hull: 		hma = HMA(Length); 					break;
					case MovingAverageType.Variable: 	vma = VMA(Length, VolatilityPeriod);break;
				}
				atr = ATR(AtrPeriod);

				rainbowBrushes = new Brush[]
				{
					Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.LimeGreen,
					Brushes.DeepSkyBlue, Brushes.LightBlue, Brushes.Violet, Brushes.Magenta
				};
				
				foreach (Brush brush in rainbowBrushes)
					brush.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < (Math.Max(Length, AtrPeriod) + NormalizationLookback + 5))
			{
				trend[0] = 0;
				return;
			}

			switch (AverageType)
			{
				case MovingAverageType.Simple: 		avg[0] = sma[0]; break;
				case MovingAverageType.Exponential: avg[0] = ema[0]; break;
				case MovingAverageType.Hull: 		avg[0] = hma[0]; break;
				case MovingAverageType.Variable: 	avg[0] = vma[0]; break;
				default: 							avg[0] = sma[0]; break;
			}

			double atrValue = atr[0];

			Stdv1[0]  = avg[0] + (atrValue * AtrMultiplier * 1);
			Stdv2[0]  = avg[0] + (atrValue * AtrMultiplier * 2);
			Stdv3[0]  = avg[0] + (atrValue * AtrMultiplier * 3);
			Stdv4[0]  = avg[0] + (atrValue * AtrMultiplier * 4);
			Stdv_1[0] = avg[0] - (atrValue * AtrMultiplier * 1);
			Stdv_2[0] = avg[0] - (atrValue * AtrMultiplier * 2);
			Stdv_3[0] = avg[0] - (atrValue * AtrMultiplier * 3);
			Stdv_4[0] = avg[0] - (atrValue * AtrMultiplier * 4);
			Avg[0]    = avg[0];
			
			avgDiffSeries[0] = avg[0] - avg[5];
			double maxSlope = MAX(avgDiffSeries, NormalizationLookback)[0];
			double minSlope = MIN(avgDiffSeries, NormalizationLookback)[0];
			avgColNormSeries[0] = (maxSlope != minSlope) ? (avgDiffSeries[0] - minSlope) / (maxSlope - minSlope) : 0.5;

			if (avgColNormSeries[0] > upThreshold)
				PlotBrushes[8][0] = UpColor;
			else if (avgColNormSeries[0] < downThreshold)
				PlotBrushes[8][0] = DownColor;
			else
				PlotBrushes[8][0] = RangingColor;

			PlotBrushes[7][0] = rainbowBrushes[0]; 
			PlotBrushes[6][0] = rainbowBrushes[1]; 
			PlotBrushes[5][0] = rainbowBrushes[2]; 
			PlotBrushes[4][0] = rainbowBrushes[3]; 
			PlotBrushes[0][0] = rainbowBrushes[4]; 
			PlotBrushes[1][0] = rainbowBrushes[5]; 
			PlotBrushes[2][0] = rainbowBrushes[6]; 
			PlotBrushes[3][0] = rainbowBrushes[7]; 

			trend[0] = trend[1]; 
			if(CrossAbove(avgColNormSeries, upThreshold, 1))
				trend[0] = 1;
			else if(CrossBelow(avgColNormSeries, downThreshold, 1))
				trend[0] = -1;

			if (ShowTrendSignals)
			{
				if (trend[0] == 1 && trend[0] != trend[1])
					Draw.Text(this, "UpTrend"+CurrentBar, "UpT", 0, avg[0], UpColor);
				if (trend[0] == -1 && trend[0] != trend[1])
					Draw.Text(this, "DownTrend"+CurrentBar, "DnT", 0, avg[0], DownColor);
			}

			if (ColorBar)
			{
				if (Low[0] < Stdv_4[0]) BarBrush = Brushes.Cyan;
				else if (High[0] > Stdv4[0]) BarBrush = Brushes.Magenta;
				else if (Close[0] > Stdv_2[0] && Close[0] < Stdv_1[0]) BarBrush = Brushes.LightGreen;
				else if (Close[0] > Stdv_3[0] && Close[0] < Stdv_2[0]) BarBrush = Brushes.Green;
				else if (Close[0] > Stdv_4[0] && Close[0] < Stdv_3[0]) BarBrush = Brushes.DarkGreen;
				else if (Close[0] < Stdv4[0] && Close[0] > Stdv3[0]) BarBrush = Brushes.DarkRed;
				else if (Close[0] < Stdv3[0] && Close[0] > Stdv2[0]) BarBrush = Brushes.Red;
				else if (Close[0] > Stdv1[0] && Close[0] < Stdv2[0]) BarBrush = Brushes.DarkOrange;
				else BarBrush = Brushes.DarkGray;
			}
			
			Series<double>[] allLines = new Series<double>[] { Stdv1, Stdv2, Stdv3, Stdv4, Stdv_1, Stdv_2, Stdv_3, Stdv_4, Avg };

			for (int i = 0; i < allLines.Length; i++)
			{
				if (CrossAbove(Close, allLines[i], 1))
					Draw.Diamond(this, "DTPCrossAbove" + i + CurrentBar, true, 0, allLines[i][0], Brushes.Cyan);
				else if (CrossBelow(Close, allLines[i], 1))
					Draw.Diamond(this, "DTPCrossBelow" + i + CurrentBar, true, 0, allLines[i][0], Brushes.Yellow);
			}
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ColorBar", Order=1, GroupName="Parameters")]
		public bool ColorBar { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Order=2, GroupName="Parameters")]
		public int Length { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Volatility Period", Order=3, GroupName="Parameters")]
		public int VolatilityPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="ATR Step Multiplier", Order=4, GroupName="Parameters")]
		public double AtrMultiplier { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATR Period", Order=5, GroupName="Parameters")]
		public int AtrPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 250)]
		[Display(Name="Normalization Lookback", Order=6, GroupName="Parameters")]
		public int NormalizationLookback { get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowTrendSignals", Order=7, GroupName="Parameters")]
		public bool ShowTrendSignals { get; set; }

		[NinjaScriptProperty]
		[Display(Name="AverageType", Order=8, GroupName="Parameters")]
		public MovingAverageType AverageType { get; set; }

		[XmlIgnore]
        [Display(Name = "Ranging Color", GroupName = "Colors", Order = 1)]
        public Brush RangingColor { get; set; }

        [Browsable(false)]
        public string RangingColorSerializable
        {
            get { return Serialize.BrushToString(RangingColor); }
            set { RangingColor = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
        [Display(Name = "Up Color", GroupName = "Colors", Order = 2)]
        public Brush UpColor { get; set; }

        [Browsable(false)]
        public string UpColorSerializable
        {
            get { return Serialize.BrushToString(UpColor); }
            set { UpColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Down Color", GroupName = "Colors", Order = 3)]
        public Brush DownColor { get; set; }

        [Browsable(false)]
        public string DownColorSerializable
        {
            get { return Serialize.BrushToString(DownColor); }
            set { DownColor = Serialize.StringToBrush(value); }
        }

		[Browsable(false)][XmlIgnore] public Series<double> Stdv1 { get { return Values[0]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Stdv2 { get { return Values[1]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Stdv3 { get { return Values[2]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Stdv4 { get { return Values[3]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Stdv_1 { get { return Values[4]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Stdv_2 { get { return Values[5]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Stdv_3 { get { return Values[6]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Stdv_4 { get { return Values[7]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Avg { get { return Values[8]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Trend { get { return trend; } }
		[Browsable(false)][XmlIgnore] public Series<double> AvgColNorm { get { return avgColNormSeries; } }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.DeviationTrendProfile[] cacheDeviationTrendProfile;
		public AlgoTrader.DeviationTrendProfile DeviationTrendProfile(bool colorBar, int length, int volatilityPeriod, double atrMultiplier, int atrPeriod, int normalizationLookback, bool showTrendSignals, MovingAverageType averageType)
		{
			return DeviationTrendProfile(Input, colorBar, length, volatilityPeriod, atrMultiplier, atrPeriod, normalizationLookback, showTrendSignals, averageType);
		}

		public AlgoTrader.DeviationTrendProfile DeviationTrendProfile(ISeries<double> input, bool colorBar, int length, int volatilityPeriod, double atrMultiplier, int atrPeriod, int normalizationLookback, bool showTrendSignals, MovingAverageType averageType)
		{
			if (cacheDeviationTrendProfile != null)
				for (int idx = 0; idx < cacheDeviationTrendProfile.Length; idx++)
					if (cacheDeviationTrendProfile[idx] != null && cacheDeviationTrendProfile[idx].ColorBar == colorBar && cacheDeviationTrendProfile[idx].Length == length && cacheDeviationTrendProfile[idx].VolatilityPeriod == volatilityPeriod && cacheDeviationTrendProfile[idx].AtrMultiplier == atrMultiplier && cacheDeviationTrendProfile[idx].AtrPeriod == atrPeriod && cacheDeviationTrendProfile[idx].NormalizationLookback == normalizationLookback && cacheDeviationTrendProfile[idx].ShowTrendSignals == showTrendSignals && cacheDeviationTrendProfile[idx].AverageType == averageType && cacheDeviationTrendProfile[idx].EqualsInput(input))
						return cacheDeviationTrendProfile[idx];
			return CacheIndicator<AlgoTrader.DeviationTrendProfile>(new AlgoTrader.DeviationTrendProfile(){ ColorBar = colorBar, Length = length, VolatilityPeriod = volatilityPeriod, AtrMultiplier = atrMultiplier, AtrPeriod = atrPeriod, NormalizationLookback = normalizationLookback, ShowTrendSignals = showTrendSignals, AverageType = averageType }, input, ref cacheDeviationTrendProfile);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.DeviationTrendProfile DeviationTrendProfile(bool colorBar, int length, int volatilityPeriod, double atrMultiplier, int atrPeriod, int normalizationLookback, bool showTrendSignals, MovingAverageType averageType)
		{
			return indicator.DeviationTrendProfile(Input, colorBar, length, volatilityPeriod, atrMultiplier, atrPeriod, normalizationLookback, showTrendSignals, averageType);
		}

		public Indicators.AlgoTrader.DeviationTrendProfile DeviationTrendProfile(ISeries<double> input , bool colorBar, int length, int volatilityPeriod, double atrMultiplier, int atrPeriod, int normalizationLookback, bool showTrendSignals, MovingAverageType averageType)
		{
			return indicator.DeviationTrendProfile(input, colorBar, length, volatilityPeriod, atrMultiplier, atrPeriod, normalizationLookback, showTrendSignals, averageType);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.DeviationTrendProfile DeviationTrendProfile(bool colorBar, int length, int volatilityPeriod, double atrMultiplier, int atrPeriod, int normalizationLookback, bool showTrendSignals, MovingAverageType averageType)
		{
			return indicator.DeviationTrendProfile(Input, colorBar, length, volatilityPeriod, atrMultiplier, atrPeriod, normalizationLookback, showTrendSignals, averageType);
		}

		public Indicators.AlgoTrader.DeviationTrendProfile DeviationTrendProfile(ISeries<double> input , bool colorBar, int length, int volatilityPeriod, double atrMultiplier, int atrPeriod, int normalizationLookback, bool showTrendSignals, MovingAverageType averageType)
		{
			return indicator.DeviationTrendProfile(input, colorBar, length, volatilityPeriod, atrMultiplier, atrPeriod, normalizationLookback, showTrendSignals, averageType);
		}
	}
}

#endregion
