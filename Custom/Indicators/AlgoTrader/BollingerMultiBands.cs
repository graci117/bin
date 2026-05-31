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
	public class BollingerMultiBands : Indicator
	{
		private SMA		sma;
		private StdDev	stdDev;
		private Brush[] rainbowBrushes;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Bollinger Bands with 4-stage step-based volatility levels.";
				Name										= "Bollinger Multi-Bands";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				
				// Parameters
				Period										= 20;
				StdDevMultiplier							= 1.0; // Base StdDev Multiplier (Old Mult1)

				// Midline Plot (Index 0)
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "MainSMA");
				
				// Internal series plots
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper2");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper3");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper4");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower2");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower3");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower4");
			}
			else if (State == State.DataLoaded)
			{
				sma		= SMA(Period);
				stdDev	= StdDev(Period);
				
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
			if (CurrentBar < Period) return;

			double sma0 = sma[0];
			MainSMA[0] = sma0;

			if (sma0 > sma[1]) 
				PlotBrushes[0][0] = Brushes.Lime;
			else if (sma0 < sma[1])
				PlotBrushes[0][0] = Brushes.Red;
			else
				PlotBrushes[0][0] = Brushes.Gray;

			double sd = stdDev[0];
			
			// Automated tier steps based on the exposed StdDevMultiplier
			Upper1[0] = sma0 + (sd * StdDevMultiplier);
			Upper2[0] = sma0 + (sd * StdDevMultiplier * 2.0);
			Upper3[0] = sma0 + (sd * StdDevMultiplier * 3.0);
			Upper4[0] = sma0 + (sd * StdDevMultiplier * 4.0);
			
			Lower1[0] = sma0 - (sd * StdDevMultiplier);
			Lower2[0] = sma0 - (sd * StdDevMultiplier * 2.0);
			Lower3[0] = sma0 - (sd * StdDevMultiplier * 3.0);
			Lower4[0] = sma0 - (sd * StdDevMultiplier * 4.0);

			if (CurrentBar > 1)
			{
				double[] currentValues = { Upper1[0], Upper2[0], Upper3[0], Upper4[0], Lower1[0], Lower2[0], Lower3[0], Lower4[0] };
				double[] priorValues   = { Upper1[1], Upper2[1], Upper3[1], Upper4[1], Lower1[1], Lower2[1], Lower3[1], Lower4[1] };
				string[] tags          = { "U1", "U2", "U3", "U4", "L1", "L2", "L3", "L4" };

				for (int i = 0; i < 8; i++)
				{
					Draw.Line(this, tags[i] + CurrentBar, false, 1, priorValues[i], 0, currentValues[i], rainbowBrushes[i], DashStyleHelper.Solid, 1);
					
					if (CrossAbove(Close, currentValues[i], 1))
						Draw.Diamond(this, "Up" + tags[i] + CurrentBar, false, 0, currentValues[i], Brushes.Cyan);
					else if (CrossBelow(Close, currentValues[i], 1))
						Draw.Diamond(this, "Dn" + tags[i] + CurrentBar, false, 0, currentValues[i], Brushes.Yellow);
				}
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name="SMA Period", GroupName="Parameters", Order=0)]
		public int Period { get; set; }

		[Range(0.1, 10.0), NinjaScriptProperty]
		[Display(Name="ATR Multiplier", GroupName="Parameters", Order=1)]
		public double StdDevMultiplier { get; set; }

		[Browsable(false)][XmlIgnore] public Series<double> MainSMA { get { return Values[0]; } }
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
		private AlgoTrader.BollingerMultiBands[] cacheBollingerMultiBands;
		public AlgoTrader.BollingerMultiBands BollingerMultiBands(int period, double stdDevMultiplier)
		{
			return BollingerMultiBands(Input, period, stdDevMultiplier);
		}

		public AlgoTrader.BollingerMultiBands BollingerMultiBands(ISeries<double> input, int period, double stdDevMultiplier)
		{
			if (cacheBollingerMultiBands != null)
				for (int idx = 0; idx < cacheBollingerMultiBands.Length; idx++)
					if (cacheBollingerMultiBands[idx] != null && cacheBollingerMultiBands[idx].Period == period && cacheBollingerMultiBands[idx].StdDevMultiplier == stdDevMultiplier && cacheBollingerMultiBands[idx].EqualsInput(input))
						return cacheBollingerMultiBands[idx];
			return CacheIndicator<AlgoTrader.BollingerMultiBands>(new AlgoTrader.BollingerMultiBands(){ Period = period, StdDevMultiplier = stdDevMultiplier }, input, ref cacheBollingerMultiBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.BollingerMultiBands BollingerMultiBands(int period, double stdDevMultiplier)
		{
			return indicator.BollingerMultiBands(Input, period, stdDevMultiplier);
		}

		public Indicators.AlgoTrader.BollingerMultiBands BollingerMultiBands(ISeries<double> input , int period, double stdDevMultiplier)
		{
			return indicator.BollingerMultiBands(input, period, stdDevMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.BollingerMultiBands BollingerMultiBands(int period, double stdDevMultiplier)
		{
			return indicator.BollingerMultiBands(Input, period, stdDevMultiplier);
		}

		public Indicators.AlgoTrader.BollingerMultiBands BollingerMultiBands(ISeries<double> input , int period, double stdDevMultiplier)
		{
			return indicator.BollingerMultiBands(input, period, stdDevMultiplier);
		}
	}
}

#endregion
