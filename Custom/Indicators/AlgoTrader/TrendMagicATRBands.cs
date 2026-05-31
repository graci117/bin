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
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class TrendMagicATRBands : Indicator
	{
		// TrendMagic Internals
		private double cciVal 					= 0.0;
		private double atrValTM 				= 0.0;
		private double upTrend 					= 0.0;
		private double downTrend 				= 0.0;
		private Series<double> lineColor;

		// ATR Band Internals
		private ATR atrBands;
		private Brush[] rainbowBrushes;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description						= @"TrendMagic logic with step-based ATR volatility bands.";
				Name							= "TrendMagic ATR Bands";
				Calculate						= Calculate.OnBarClose;
				IsOverlay						= true; 
				IsSuspendedWhileInactive		= true;
				
				// TrendMagic Core Parameters
				CciPeriod 						= 20;
				AtrPeriodTM 					= 14;
				AtrMultTM						= 1.0;
				
				// Visual Band Parameters
				BandAtrPeriod					= 100;
				AtrMultiplier					= 1.25;

				// Midline Plot
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "TrendMagicMidline");

				// Band Plots (Transparent for data access)
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper2");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper3");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper4");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower2");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower3");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower4");
			}
			else if (State == State.Configure)
			{
				lineColor = new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{
				atrBands = ATR(BandAtrPeriod);
				
				rainbowBrushes = new Brush[] 
                { 
                    Brushes.DeepSkyBlue, Brushes.RoyalBlue, Brushes.BlueViolet, Brushes.Magenta, // Uppers
                    Brushes.LimeGreen, Brushes.Yellow, Brushes.Orange, Brushes.Red            // Lowers
                };
                foreach (var b in rainbowBrushes) b.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(CciPeriod, Math.Max(AtrPeriodTM, BandAtrPeriod)))
				return;
			
			// --- 1. Calculate TrendMagic (Midline) ---
			cciVal = CCI(CciPeriod)[0];
			atrValTM = ATR(AtrPeriodTM)[0];
			
			upTrend = Low[0] - atrValTM * AtrMultTM;
			downTrend = High[0] + atrValTM * AtrMultTM;
							
			if (cciVal >= 0)
			{
				if (upTrend < TrendMagicValue[1])
					TrendMagicValue[0] = TrendMagicValue[1];
				else
					TrendMagicValue[0] = upTrend;
			}
			else
			{
				if (downTrend > TrendMagicValue[1])
					TrendMagicValue[0] = TrendMagicValue[1];
				else
					TrendMagicValue[0] = downTrend;
			}
			
			// Midline Slope Coloring
			if (TrendMagicValue[0] > TrendMagicValue[1])
				PlotBrushes[0][0] = Brushes.Lime;
			else if (TrendMagicValue[0] < TrendMagicValue[1])
				PlotBrushes[0][0] = Brushes.Red;
			else
				PlotBrushes[0][0] = PlotBrushes[0][1];

			// --- 2. Calculate ATR Bands in steps ---
			double mid = TrendMagicValue[0];
			double bAtr = atrBands[0];

			Upper1[0] = mid + (bAtr * AtrMultiplier * 1);
			Upper2[0] = mid + (bAtr * AtrMultiplier * 2);
			Upper3[0] = mid + (bAtr * AtrMultiplier * 3);
			Upper4[0] = mid + (bAtr * AtrMultiplier * 4);
			Lower1[0] = mid - (bAtr * AtrMultiplier * 1);
			Lower2[0] = mid - (bAtr * AtrMultiplier * 2);
			Lower3[0] = mid - (bAtr * AtrMultiplier * 3);
			Lower4[0] = mid - (bAtr * AtrMultiplier * 4);

			// --- 3. Visual Rainbow Lines and Dots ---
			if (CurrentBar > 1)
			{
				double[] curBands = { Upper1[0], Upper2[0], Upper3[0], Upper4[0], Lower1[0], Lower2[0], Lower3[0], Lower4[0] };
				double[] priBands = { Upper1[1], Upper2[1], Upper3[1], Upper4[1], Lower1[1], Lower2[1], Lower3[1], Lower4[1] };
				string[] tags = { "U1", "U2", "U3", "U4", "L1", "L2", "L3", "L4" };

				for (int i = 0; i < 8; i++)
				{
					Draw.Line(this, tags[i] + CurrentBar, false, 1, priBands[i], 0, curBands[i], rainbowBrushes[i], DashStyleHelper.Solid, 1);
					
					if (Close[1] <= curBands[i] && Close[0] > curBands[i])
						Draw.Dot(this, "UpDot" + tags[i] + CurrentBar, false, 0, curBands[i], Brushes.Cyan);
					else if (Close[1] >= curBands[i] && Close[0] < curBands[i])
						Draw.Dot(this, "DnDot" + tags[i] + CurrentBar, false, 0, curBands[i], Brushes.Yellow);
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CCI Period", Order=1, GroupName="1. TrendMagic Parameters")]
		public int CciPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TM ATR Period", Order=2, GroupName="1. TrendMagic Parameters")]
		public int AtrPeriodTM { get; set; }
		
		[NinjaScriptProperty]
		[Range(0.00001, double.MaxValue)]
		[Display(Name="TM ATR Multiplier", Order=3, GroupName="1. TrendMagic Parameters")]
		public double AtrMultTM { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Band ATR Period", Order=4, GroupName="2. Band Parameters")]
		public int BandAtrPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(0.1, 10.0)]
		[Display(Name="ATR Step Multiplier", Order=5, GroupName="2. Band Parameters")]
		public double AtrMultiplier { get; set; }

		[Browsable(false)][XmlIgnore] public Series<double> TrendMagicValue { get { return Values[0]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Upper1 { get { return Values[1]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Upper2 { get { return Values[2]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Upper3 { get { return Values[3]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Upper4 { get { return Values[4]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Lower1 { get { return Values[5]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Lower2 { get { return Values[6]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Lower3 { get { return Values[7]; } }
		[Browsable(false)][XmlIgnore] public Series<double> Lower4 { get { return Values[8]; } }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.TrendMagicATRBands[] cacheTrendMagicATRBands;
		public AlgoTrader.TrendMagicATRBands TrendMagicATRBands(int cciPeriod, int atrPeriodTM, double atrMultTM, int bandAtrPeriod, double atrMultiplier)
		{
			return TrendMagicATRBands(Input, cciPeriod, atrPeriodTM, atrMultTM, bandAtrPeriod, atrMultiplier);
		}

		public AlgoTrader.TrendMagicATRBands TrendMagicATRBands(ISeries<double> input, int cciPeriod, int atrPeriodTM, double atrMultTM, int bandAtrPeriod, double atrMultiplier)
		{
			if (cacheTrendMagicATRBands != null)
				for (int idx = 0; idx < cacheTrendMagicATRBands.Length; idx++)
					if (cacheTrendMagicATRBands[idx] != null && cacheTrendMagicATRBands[idx].CciPeriod == cciPeriod && cacheTrendMagicATRBands[idx].AtrPeriodTM == atrPeriodTM && cacheTrendMagicATRBands[idx].AtrMultTM == atrMultTM && cacheTrendMagicATRBands[idx].BandAtrPeriod == bandAtrPeriod && cacheTrendMagicATRBands[idx].AtrMultiplier == atrMultiplier && cacheTrendMagicATRBands[idx].EqualsInput(input))
						return cacheTrendMagicATRBands[idx];
			return CacheIndicator<AlgoTrader.TrendMagicATRBands>(new AlgoTrader.TrendMagicATRBands(){ CciPeriod = cciPeriod, AtrPeriodTM = atrPeriodTM, AtrMultTM = atrMultTM, BandAtrPeriod = bandAtrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheTrendMagicATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.TrendMagicATRBands TrendMagicATRBands(int cciPeriod, int atrPeriodTM, double atrMultTM, int bandAtrPeriod, double atrMultiplier)
		{
			return indicator.TrendMagicATRBands(Input, cciPeriod, atrPeriodTM, atrMultTM, bandAtrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.TrendMagicATRBands TrendMagicATRBands(ISeries<double> input , int cciPeriod, int atrPeriodTM, double atrMultTM, int bandAtrPeriod, double atrMultiplier)
		{
			return indicator.TrendMagicATRBands(input, cciPeriod, atrPeriodTM, atrMultTM, bandAtrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.TrendMagicATRBands TrendMagicATRBands(int cciPeriod, int atrPeriodTM, double atrMultTM, int bandAtrPeriod, double atrMultiplier)
		{
			return indicator.TrendMagicATRBands(Input, cciPeriod, atrPeriodTM, atrMultTM, bandAtrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.TrendMagicATRBands TrendMagicATRBands(ISeries<double> input , int cciPeriod, int atrPeriodTM, double atrMultTM, int bandAtrPeriod, double atrMultiplier)
		{
			return indicator.TrendMagicATRBands(input, cciPeriod, atrPeriodTM, atrMultTM, bandAtrPeriod, atrMultiplier);
		}
	}
}

#endregion
