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
using NinjaTrader.NinjaScript.Indicators.AlgoTrader;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class MomentumExtremesATRBands : Indicator
	{
		private Series<double> rawPriceSeries;
		private HMA hma;
		private MAX highestHighs;
		private MIN lowestLows;
		private ATR atr;
		private Brush downBrush;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Momentum Extremes Driver centerline with step-based ATR volatility bands.";
				Name										= "Momentum Extremes Bands";
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				IsSuspendedWhileInactive					= true;

				// Parameters
				HmaPeriod									= 100;
				ExtremesLookback							= 20;
				OffsetTicks									= 0;
				DriverWidth							 		= 2;
				AtrPeriod									= 100;
				AtrMultiplier								= 1.25;
				UpColor										= Brushes.Lime;
				DownColor									= Brushes.Red;
				
				// Plots
				AddPlot(new Stroke(Brushes.DarkOrange, 1), PlotStyle.Line, "Upper Band 1");
				AddPlot(new Stroke(Brushes.LightGreen, 1), PlotStyle.Line, "Lower Band 1");
				AddPlot(new Stroke(Brushes.Magenta, 1), PlotStyle.Line, "Upper Band 2");
				AddPlot(new Stroke(Brushes.Cyan, 1), PlotStyle.Line, "Lower Band 2");
				AddPlot(new Stroke(Brushes.White, 1), PlotStyle.Line, "Upper Band 3");
				AddPlot(new Stroke(Brushes.White, 1), PlotStyle.Line, "Lower Band 3");
				AddPlot(new Stroke(Brushes.Gold, 1), PlotStyle.Line, "Upper Band 4");
				AddPlot(new Stroke(Brushes.Gold, 1), PlotStyle.Line, "Lower Band 4");
				AddPlot(new Stroke(UpColor, DriverWidth), PlotStyle.Line, "MED Line");
			}
			else if (State == State.Configure)
			{
				rawPriceSeries = new Series<double>(this);
				highestHighs = MAX(High, ExtremesLookback);
				lowestLows   = MIN(Low, ExtremesLookback);
				hma = HMA(rawPriceSeries, HmaPeriod);
				atr = ATR(AtrPeriod);
				
				downBrush = DownColor;
				if (downBrush.CanFreeze)
					downBrush.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(HmaPeriod, ExtremesLookback))
				return;
			
			double rawValue;
			
			// --- Core Logic for raw value ---
			bool newLowestLow  = lowestLows[0] < lowestLows[1];
			bool newHighestHigh = highestHighs[0] > highestHighs[1];

			if (newLowestLow || newHighestHigh)
				rawValue = High[0] + (OffsetTicks * TickSize);
			else
				rawValue = Low[0];
			
			rawPriceSeries[0] = rawValue;

			// --- Calculate final MED centerline and ATR bands in steps ---
			double medValue = hma[0];
			double atrValue = atr[0];
			
			MEDLine[0] = medValue;
			
			UpperBand1[0] = medValue + (atrValue * AtrMultiplier * 1);
			LowerBand1[0] = medValue - (atrValue * AtrMultiplier * 1);
			
			UpperBand2[0] = medValue + (atrValue * AtrMultiplier * 2);
			LowerBand2[0] = medValue - (atrValue * AtrMultiplier * 2);
			
			UpperBand3[0] = medValue + (atrValue * AtrMultiplier * 3);
			LowerBand3[0] = medValue - (atrValue * AtrMultiplier * 3);
			
			UpperBand4[0] = medValue + (atrValue * AtrMultiplier * 4);
			LowerBand4[0] = medValue - (atrValue * AtrMultiplier * 4);
			
			// Color the HMA line based on its slope
			if(MEDLine[0] > MEDLine[1])
				PlotBrushes[8][0] = UpColor; 
			else if (MEDLine[0] < MEDLine[1])
				PlotBrushes[8][0] = downBrush;
			
			// Cross checks
			if (CrossAbove(Close, MEDLine, 1))
				Draw.Diamond(this, "MEDLineCross" + CurrentBar, true, 0, MEDLine[0], Brushes.Cyan);
			else if (CrossBelow(Close, MEDLine, 1))
				Draw.Diamond(this, "MEDLineCross" + CurrentBar, true, 0, MEDLine[0], Brushes.Yellow);

			Series<double>[] allBands = new Series<double>[] 
			{ 
				UpperBand1, LowerBand1, UpperBand2, LowerBand2, 
				UpperBand3, LowerBand3, UpperBand4, LowerBand4 
			};

			for (int i = 0; i < allBands.Length; i++)
			{
				if (CrossAbove(Close, allBands[i], 1))
					Draw.Diamond(this, "ATRBANDCross" + i + CurrentBar, true, 0, allBands[i][0], Brushes.Cyan);
				else if (CrossBelow(Close, allBands[i], 1))
					Draw.Diamond(this, "ATRBANDCross" + i + CurrentBar, true, 0, allBands[i][0], Brushes.Yellow);
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="HMA Period", Order=1, GroupName="Parameters")]
		public int HmaPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="Extremes Lookback", Order=2, GroupName="Parameters")]
		public int ExtremesLookback { get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Offset Ticks", Order=3, GroupName="Parameters")]
		public int OffsetTicks { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "ATR Period", GroupName = "Parameters", Order = 4)]
		public int AtrPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name = "ATR Step Multiplier", GroupName = "Parameters", Order = 5)]
		public double AtrMultiplier { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Up Color", Order=6, GroupName="Visuals")]
		public Brush UpColor { get; set; }

		[Browsable(false)]
		public string UpColorSerializable
		{
			get { return Serialize.BrushToString(UpColor); }
			set { UpColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Down Color", Order=7, GroupName="Visuals")]
		public Brush DownColor { get; set; }

		[Browsable(false)]
		public string DownColorSerializable
		{
			get { return Serialize.BrushToString(DownColor); }
			set { DownColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(1, 20)]
		[Display(Name="Driver Width", Order=8, GroupName="Visuals")]
		public int DriverWidth { get; set; }
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
		[Browsable(false)] [XmlIgnore] public Series<double> MEDLine { get { return Values[8]; } }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.MomentumExtremesATRBands[] cacheMomentumExtremesATRBands;
		public AlgoTrader.MomentumExtremesATRBands MomentumExtremesATRBands(int hmaPeriod, int extremesLookback, int offsetTicks, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return MomentumExtremesATRBands(Input, hmaPeriod, extremesLookback, offsetTicks, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}

		public AlgoTrader.MomentumExtremesATRBands MomentumExtremesATRBands(ISeries<double> input, int hmaPeriod, int extremesLookback, int offsetTicks, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			if (cacheMomentumExtremesATRBands != null)
				for (int idx = 0; idx < cacheMomentumExtremesATRBands.Length; idx++)
					if (cacheMomentumExtremesATRBands[idx] != null && cacheMomentumExtremesATRBands[idx].HmaPeriod == hmaPeriod && cacheMomentumExtremesATRBands[idx].ExtremesLookback == extremesLookback && cacheMomentumExtremesATRBands[idx].OffsetTicks == offsetTicks && cacheMomentumExtremesATRBands[idx].AtrPeriod == atrPeriod && cacheMomentumExtremesATRBands[idx].AtrMultiplier == atrMultiplier && cacheMomentumExtremesATRBands[idx].UpColor == upColor && cacheMomentumExtremesATRBands[idx].DownColor == downColor && cacheMomentumExtremesATRBands[idx].DriverWidth == driverWidth && cacheMomentumExtremesATRBands[idx].EqualsInput(input))
						return cacheMomentumExtremesATRBands[idx];
			return CacheIndicator<AlgoTrader.MomentumExtremesATRBands>(new AlgoTrader.MomentumExtremesATRBands(){ HmaPeriod = hmaPeriod, ExtremesLookback = extremesLookback, OffsetTicks = offsetTicks, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier, UpColor = upColor, DownColor = downColor, DriverWidth = driverWidth }, input, ref cacheMomentumExtremesATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.MomentumExtremesATRBands MomentumExtremesATRBands(int hmaPeriod, int extremesLookback, int offsetTicks, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.MomentumExtremesATRBands(Input, hmaPeriod, extremesLookback, offsetTicks, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}

		public Indicators.AlgoTrader.MomentumExtremesATRBands MomentumExtremesATRBands(ISeries<double> input , int hmaPeriod, int extremesLookback, int offsetTicks, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.MomentumExtremesATRBands(input, hmaPeriod, extremesLookback, offsetTicks, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.MomentumExtremesATRBands MomentumExtremesATRBands(int hmaPeriod, int extremesLookback, int offsetTicks, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.MomentumExtremesATRBands(Input, hmaPeriod, extremesLookback, offsetTicks, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}

		public Indicators.AlgoTrader.MomentumExtremesATRBands MomentumExtremesATRBands(ISeries<double> input , int hmaPeriod, int extremesLookback, int offsetTicks, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.MomentumExtremesATRBands(input, hmaPeriod, extremesLookback, offsetTicks, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}
	}
}

#endregion
