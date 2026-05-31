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
	public class HMAHooksATRBands : Indicator
	{
		private HMA hma;
		private ATR atr;
		private Brush[] rainbowBrushes;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Hull Moving Average with step-based ATR volatility bands (Optimized for NQ Hooks).";
				Name										= "HMA ATR Bands";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				
				// Parameters
				Period										= 100;
				AtrPeriod									= 100;
				AtrMultiplier								= 1.25;

				// Midline Plot
				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "MainHMA");
				
				// Internal series for the Bot to access (Transparent for cleaner charts)
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
				hma = HMA(Period);
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

			// --- 1. Calculate HMA Midline ---
			double currentHma = hma[0];
			MainHMA[0] = currentHma;

			// Color midline based on slope
			if (currentHma > hma[1]) 
				PlotBrushes[0][0] = Brushes.Lime;
			else if (currentHma < hma[1])
				PlotBrushes[0][0] = Brushes.Red;
			else
				PlotBrushes[0][0] = Brushes.Gray;

			// --- 2. Calculate ATR Bands in steps ---
			double valAtr = atr[0];
			
			Upper1[0] = currentHma + (valAtr * AtrMultiplier * 1);
			Upper2[0] = currentHma + (valAtr * AtrMultiplier * 2);
			Upper3[0] = currentHma + (valAtr * AtrMultiplier * 3);
			Upper4[0] = currentHma + (valAtr * AtrMultiplier * 4);
			
			Lower1[0] = currentHma - (valAtr * AtrMultiplier * 1);
			Lower2[0] = currentHma - (valAtr * AtrMultiplier * 2);
			Lower3[0] = currentHma - (valAtr * AtrMultiplier * 3);
			Lower4[0] = currentHma - (valAtr * AtrMultiplier * 4);

			// --- 3. Visual Rainbow Lines & Cross Dots ---
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
		[Display(Name="HMA Period", GroupName="Parameters", Order=0)]
		public int Period { get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name="ATR Period", GroupName="Parameters", Order=1)]
		public int AtrPeriod { get; set; }

		[Range(0.1, 10.0), NinjaScriptProperty]
		[Display(Name="ATR Step Multiplier", GroupName="Parameters", Order=2)]
		public double AtrMultiplier { get; set; }

		[Browsable(false)][XmlIgnore] public Series<double> MainHMA { get { return Values[0]; } }
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
		private AlgoTrader.HMAHooksATRBands[] cacheHMAHooksATRBands;
		public AlgoTrader.HMAHooksATRBands HMAHooksATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return HMAHooksATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.HMAHooksATRBands HMAHooksATRBands(ISeries<double> input, int period, int atrPeriod, double atrMultiplier)
		{
			if (cacheHMAHooksATRBands != null)
				for (int idx = 0; idx < cacheHMAHooksATRBands.Length; idx++)
					if (cacheHMAHooksATRBands[idx] != null && cacheHMAHooksATRBands[idx].Period == period && cacheHMAHooksATRBands[idx].AtrPeriod == atrPeriod && cacheHMAHooksATRBands[idx].AtrMultiplier == atrMultiplier && cacheHMAHooksATRBands[idx].EqualsInput(input))
						return cacheHMAHooksATRBands[idx];
			return CacheIndicator<AlgoTrader.HMAHooksATRBands>(new AlgoTrader.HMAHooksATRBands(){ Period = period, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheHMAHooksATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.HMAHooksATRBands HMAHooksATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.HMAHooksATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.HMAHooksATRBands HMAHooksATRBands(ISeries<double> input , int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.HMAHooksATRBands(input, period, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.HMAHooksATRBands HMAHooksATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.HMAHooksATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.HMAHooksATRBands HMAHooksATRBands(ISeries<double> input , int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.HMAHooksATRBands(input, period, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
