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
	public class LinRegATRBands : Indicator
	{
		private SUM	sum;
		private ATR atr;
		private Brush[] rainbowBrushes;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Linear Regression with step-based ATR volatility bands (Optimized for NQ).";
				Name										= "LinReg Bands";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				
				// Parameters
				Period										= 100;
				AtrPeriod									= 100;
				AtrMultiplier								= 1.25;

				// Midline Plot
				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Line, "MainLinReg");
				
				// Create internal series for the Bot to access (Transparent by default)
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
				sum = SUM(Inputs[0], Period);
				atr = ATR(AtrPeriod);
				
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
			if (CurrentBar < Period || CurrentBar < AtrPeriod) return;

			// --- 1. Calculate Linear Regression Midline ---
			double x = (double)Period * (Period - 1) * 0.5;
			double d = x * x - (double)Period * Period * (Period - 1) * (2 * Period - 1) / 6;
			double xy = 0;
			for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
				xy += count * Input[count];

			double sl	= (Period * xy - x * sum[0]) / d;
			double itrc	= (sum[0] - sl * x) / Period;
			double currentLinReg = itrc + sl * (Period - 1);
			
			MainLinReg[0] = currentLinReg;

			// Color midline based on slope
			if (MainLinReg[0] > MainLinReg[1]) PlotBrushes[0][0] = Brushes.Lime;
			else PlotBrushes[0][0] = Brushes.Red;

			// --- 2. Calculate ATR Bands in steps ---
			double valAtr = atr[0];
			
			Upper1[0] = currentLinReg + (valAtr * AtrMultiplier * 1);
			Upper2[0] = currentLinReg + (valAtr * AtrMultiplier * 2);
			Upper3[0] = currentLinReg + (valAtr * AtrMultiplier * 3);
			Upper4[0] = currentLinReg + (valAtr * AtrMultiplier * 4);
			
			Lower1[0] = currentLinReg - (valAtr * AtrMultiplier * 1);
			Lower2[0] = currentLinReg - (valAtr * AtrMultiplier * 2);
			Lower3[0] = currentLinReg - (valAtr * AtrMultiplier * 3);
			Lower4[0] = currentLinReg - (valAtr * AtrMultiplier * 4);

			// --- 3. Visual Rainbow Lines (Continuous) ---
			if (CurrentBar > 1)
			{
				double[] currentValues = { Upper1[0], Upper2[0], Upper3[0], Upper4[0], Lower1[0], Lower2[0], Lower3[0], Lower4[0] };
				double[] priorValues   = { Upper1[1], Upper2[1], Upper3[1], Upper4[1], Lower1[1], Lower2[1], Lower3[1], Lower4[1] };
				string[] tags          = { "U1", "U2", "U3", "U4", "L1", "L2", "L3", "L4" };

				for (int i = 0; i < 8; i++)
				{
					// Draw smooth continuous lines for the bands
					Draw.Line(this, tags[i] + CurrentBar, false, 1, priorValues[i], 0, currentValues[i], rainbowBrushes[i], DashStyleHelper.Solid, 1);
					
					// Cross Dots
					if (Close[1] <= currentValues[i] && Close[0] > currentValues[i])
						Draw.Dot(this, "Up" + tags[i] + CurrentBar, false, 0, currentValues[i], Brushes.Cyan);
					else if (Close[1] >= currentValues[i] && Close[0] < currentValues[i])
						Draw.Dot(this, "Dn" + tags[i] + CurrentBar, false, 0, currentValues[i], Brushes.Yellow);
				}
			}
		}

		#region Properties
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(Name="LinReg Period", GroupName="Parameters", Order=0)]
		public int Period { get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name="ATR Period", GroupName="Parameters", Order=1)]
		public int AtrPeriod { get; set; }

		[Range(0.1, 10.0), NinjaScriptProperty]
		[Display(Name="ATR Step Multiplier", GroupName="Parameters", Order=2)]
		public double AtrMultiplier { get; set; }

		[Browsable(false)][XmlIgnore] public Series<double> MainLinReg { get { return Values[0]; } }
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
		private AlgoTrader.LinRegATRBands[] cacheLinRegATRBands;
		public AlgoTrader.LinRegATRBands LinRegATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return LinRegATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.LinRegATRBands LinRegATRBands(ISeries<double> input, int period, int atrPeriod, double atrMultiplier)
		{
			if (cacheLinRegATRBands != null)
				for (int idx = 0; idx < cacheLinRegATRBands.Length; idx++)
					if (cacheLinRegATRBands[idx] != null && cacheLinRegATRBands[idx].Period == period && cacheLinRegATRBands[idx].AtrPeriod == atrPeriod && cacheLinRegATRBands[idx].AtrMultiplier == atrMultiplier && cacheLinRegATRBands[idx].EqualsInput(input))
						return cacheLinRegATRBands[idx];
			return CacheIndicator<AlgoTrader.LinRegATRBands>(new AlgoTrader.LinRegATRBands(){ Period = period, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheLinRegATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.LinRegATRBands LinRegATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.LinRegATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.LinRegATRBands LinRegATRBands(ISeries<double> input , int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.LinRegATRBands(input, period, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.LinRegATRBands LinRegATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.LinRegATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.LinRegATRBands LinRegATRBands(ISeries<double> input , int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.LinRegATRBands(input, period, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
