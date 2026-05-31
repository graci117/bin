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
	public class RegressionATRBands : Indicator
	{
		private LinReg	linReg;
		private ATR		atr; 

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"A linear regression channel with step-based ATR volatility bands.";
				Name										= @"Regression ATR Bands"; 
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				Period										= 100;
				AtrPeriod 									= 100;
				AtrMultiplier								= 1.25;

				AddPlot(new Stroke(Brushes.Orange, 1), PlotStyle.Line, "Upper Band 1");
				AddPlot(new Stroke(Brushes.LightGreen, 1), PlotStyle.Line, "Lower Band 1");
				AddPlot(new Stroke(Brushes.Magenta, 1), PlotStyle.Line, "Upper Band 2");
				AddPlot(new Stroke(Brushes.Cyan, 1), PlotStyle.Line, "Lower Band 2");
				AddPlot(new Stroke(Brushes.White, 1), PlotStyle.Line, "Upper Band 3");
				AddPlot(new Stroke(Brushes.White, 1), PlotStyle.Line, "Lower Band 3");
				AddPlot(new Stroke(Brushes.Gold, 1), PlotStyle.Line, "Upper Band 4");
				AddPlot(new Stroke(Brushes.Gold, 1), PlotStyle.Line, "Lower Band 4");
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "Regression Line");
			}
			else if (State == State.DataLoaded)
			{
				linReg	= LinReg(Period);
				atr 	= ATR(AtrPeriod); 
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(Period, AtrPeriod))
				return;
			
			double regressionValue 	= linReg[0];
			double atrValue 		= atr[0];
			
			RegressionLine[0] = regressionValue;
			
			UpperBand1[0] = regressionValue + (AtrMultiplier * 1 * atrValue);
			LowerBand1[0] = regressionValue - (AtrMultiplier * 1 * atrValue);
			
			UpperBand2[0] = regressionValue + (AtrMultiplier * 2 * atrValue);
			LowerBand2[0] = regressionValue - (AtrMultiplier * 2 * atrValue);
			
			UpperBand3[0] = regressionValue + (AtrMultiplier * 3 * atrValue);
			LowerBand3[0] = regressionValue - (AtrMultiplier * 3 * atrValue);
			
			UpperBand4[0] = regressionValue + (AtrMultiplier * 4 * atrValue);
			LowerBand4[0] = regressionValue - (AtrMultiplier * 4 * atrValue);

			if (regressionValue > Values[8][1]) 
			{
				PlotBrushes[8][0] = Brushes.Lime;
			}
			else if (regressionValue < Values[8][1])
			{
				PlotBrushes[8][0] = Brushes.Red;
			}
			else
			{
				PlotBrushes[8][0] = Brushes.Gray;
			}
			
			Series<double>[] allLines = new Series<double>[]
			{
				UpperBand1, LowerBand1,
				UpperBand2, LowerBand2,
				UpperBand3, LowerBand3,
				UpperBand4, LowerBand4,
				RegressionLine
			};
			
			for(int i = 0; i < allLines.Length; i++)
			{
				if (CrossAbove(Close, allLines[i], 1))
				{
					Draw.Diamond(this, "CrossAbove" + i + CurrentBar, true, 0, allLines[i][0], Brushes.Cyan);
				}
				else if (CrossBelow(Close, allLines[i], 1))
				{
					Draw.Diamond(this, "CrossBelow" + i + CurrentBar, true, 0, allLines[i][0], Brushes.Yellow);
				}
			}
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "Period", GroupName = "Parameters", Order = 0)]
		public int Period { get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "ATR Period", GroupName = "Parameters", Order = 1)]
		public int AtrPeriod { get; set; }

		[Range(0.1, double.MaxValue), NinjaScriptProperty]
		[Display(Name = "ATR Step Multiplier", GroupName = "Parameters", Order = 2)]
		public double AtrMultiplier { get; set; }
		#endregion
		
		#region Plot Accessors
		[Browsable(false)] [XmlIgnore] public Series<double> UpperBand1 { get { return Values[0]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> LowerBand1 { get { return Values[1]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> UpperBand2 { get { return Values[2]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> LowerBand2 { get { return Values[3]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> UpperBand3 { get { return Values[4]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> LowerBand3 { get { return Values[5]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> UpperBand4 { get { return Values[6]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> LowerBand4 { get { return Values[7]; } }
		[Browsable(false)] [XmlIgnore] public Series<double> RegressionLine { get { return Values[8]; } }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.RegressionATRBands[] cacheRegressionATRBands;
		public AlgoTrader.RegressionATRBands RegressionATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return RegressionATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public AlgoTrader.RegressionATRBands RegressionATRBands(ISeries<double> input, int period, int atrPeriod, double atrMultiplier)
		{
			if (cacheRegressionATRBands != null)
				for (int idx = 0; idx < cacheRegressionATRBands.Length; idx++)
					if (cacheRegressionATRBands[idx] != null && cacheRegressionATRBands[idx].Period == period && cacheRegressionATRBands[idx].AtrPeriod == atrPeriod && cacheRegressionATRBands[idx].AtrMultiplier == atrMultiplier && cacheRegressionATRBands[idx].EqualsInput(input))
						return cacheRegressionATRBands[idx];
			return CacheIndicator<AlgoTrader.RegressionATRBands>(new AlgoTrader.RegressionATRBands(){ Period = period, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier }, input, ref cacheRegressionATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.RegressionATRBands RegressionATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.RegressionATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.RegressionATRBands RegressionATRBands(ISeries<double> input , int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.RegressionATRBands(input, period, atrPeriod, atrMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.RegressionATRBands RegressionATRBands(int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.RegressionATRBands(Input, period, atrPeriod, atrMultiplier);
		}

		public Indicators.AlgoTrader.RegressionATRBands RegressionATRBands(ISeries<double> input , int period, int atrPeriod, double atrMultiplier)
		{
			return indicator.RegressionATRBands(input, period, atrPeriod, atrMultiplier);
		}
	}
}

#endregion
