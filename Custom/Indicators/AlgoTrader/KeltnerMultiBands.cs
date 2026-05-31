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
	public class KeltnerMultiBands : Indicator
	{
		private Series<double>	diff;
		private	SMA				smaDiff;
		private	SMA				smaTypical;
		private Brush[]         rainbowBrushes;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Keltner Channel with step-based volatility levels.";
				Name										= "Keltner Multi-Bands";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				
				Period										= 10;
				StdDevMultiplier							= 1.0; // Base Multiplier 

				AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "MainMidline");
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
				diff				= new Series<double>(this);
				smaDiff				= SMA(diff, Period);
				smaTypical			= SMA(Typical, Period);
				
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

			diff[0] = High[0] - Low[0];
			double middle = smaTypical[0];
			MainMidline[0] = middle;

			if (middle > MainMidline[1]) 
				PlotBrushes[0][0] = Brushes.Lime;
			else if (middle < MainMidline[1])
				PlotBrushes[0][0] = Brushes.Red;
			else
				PlotBrushes[0][0] = Brushes.Gray;

			double baseOffset = smaDiff[0];
			
			// Automated steps based on the exposed StdDevMultiplier
			Upper1[0] = middle + (baseOffset * StdDevMultiplier);
			Upper2[0] = middle + (baseOffset * StdDevMultiplier * 1.5);
			Upper3[0] = middle + (baseOffset * StdDevMultiplier * 2.0);
			Upper4[0] = middle + (baseOffset * StdDevMultiplier * 2.5);
			
			Lower1[0] = middle - (baseOffset * StdDevMultiplier);
			Lower2[0] = middle - (baseOffset * StdDevMultiplier * 1.5);
			Lower3[0] = middle - (baseOffset * StdDevMultiplier * 2.0);
			Lower4[0] = middle - (baseOffset * StdDevMultiplier * 2.5);

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

		[Range(0.01, 10.0), NinjaScriptProperty]
		[Display(Name="ATR Multiplier", GroupName="Parameters", Order=1)]
		public double StdDevMultiplier { get; set; }

		[Browsable(false)][XmlIgnore] public Series<double> MainMidline { get { return Values[0]; } }
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
		private AlgoTrader.KeltnerMultiBands[] cacheKeltnerMultiBands;
		public AlgoTrader.KeltnerMultiBands KeltnerMultiBands(int period, double stdDevMultiplier)
		{
			return KeltnerMultiBands(Input, period, stdDevMultiplier);
		}

		public AlgoTrader.KeltnerMultiBands KeltnerMultiBands(ISeries<double> input, int period, double stdDevMultiplier)
		{
			if (cacheKeltnerMultiBands != null)
				for (int idx = 0; idx < cacheKeltnerMultiBands.Length; idx++)
					if (cacheKeltnerMultiBands[idx] != null && cacheKeltnerMultiBands[idx].Period == period && cacheKeltnerMultiBands[idx].StdDevMultiplier == stdDevMultiplier && cacheKeltnerMultiBands[idx].EqualsInput(input))
						return cacheKeltnerMultiBands[idx];
			return CacheIndicator<AlgoTrader.KeltnerMultiBands>(new AlgoTrader.KeltnerMultiBands(){ Period = period, StdDevMultiplier = stdDevMultiplier }, input, ref cacheKeltnerMultiBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.KeltnerMultiBands KeltnerMultiBands(int period, double stdDevMultiplier)
		{
			return indicator.KeltnerMultiBands(Input, period, stdDevMultiplier);
		}

		public Indicators.AlgoTrader.KeltnerMultiBands KeltnerMultiBands(ISeries<double> input , int period, double stdDevMultiplier)
		{
			return indicator.KeltnerMultiBands(input, period, stdDevMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.KeltnerMultiBands KeltnerMultiBands(int period, double stdDevMultiplier)
		{
			return indicator.KeltnerMultiBands(Input, period, stdDevMultiplier);
		}

		public Indicators.AlgoTrader.KeltnerMultiBands KeltnerMultiBands(ISeries<double> input , int period, double stdDevMultiplier)
		{
			return indicator.KeltnerMultiBands(input, period, stdDevMultiplier);
		}
	}
}

#endregion
