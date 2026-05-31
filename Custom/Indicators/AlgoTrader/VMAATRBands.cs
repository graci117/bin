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

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class VMAATRBands : Indicator
	{
		private Series<double> rawPriceSeries;
		private VMA 		vma;
		private MAX 		highestHighs;
		private MIN 		lowestLows;
		private ATR 		atr; 
		private Brush 		downBrush;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"A Momentum Extremes VMA Driver centerline with step-based ATR volatility bands.";
				Name										= "VMA ATR Bands";
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				IsSuspendedWhileInactive					= true;

				// Parameters
				VmaPeriod									= 30;
				VmaVolatilityPeriod							= 30;
				ExtremesLookback							= 14;
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
				AddPlot(new Stroke(UpColor, DriverWidth), PlotStyle.Line, "VMA Line");
			}
			else if (State == State.Configure)
			{
				rawPriceSeries = new Series<double>(this);
				highestHighs = MAX(High, ExtremesLookback);
				lowestLows   = MIN(Low, ExtremesLookback);
				vma = VMA(rawPriceSeries, VmaPeriod, VmaVolatilityPeriod);
				atr = ATR(AtrPeriod); 
				
				downBrush = DownColor;
				if (downBrush.CanFreeze)
					downBrush.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(VmaPeriod, ExtremesLookback))
				return;
			
			double rawValue;
			bool newLowestLow  = lowestLows[0] < lowestLows[1];
			bool newHighestHigh = highestHighs[0] > highestHighs[1];

			if (newLowestLow || newHighestHigh)
				rawValue = High[0];
			else
				rawValue = Low[0];
			
			rawPriceSeries[0] = rawValue;

			double vmaValue = vma[0];
			double atrValue = atr[0];
			
			VmaLine[0] = vmaValue;
			
			UpperBand1[0] = vmaValue + (AtrMultiplier * 1 * atrValue);
			LowerBand1[0] = vmaValue - (AtrMultiplier * 1 * atrValue);
			
			UpperBand2[0] = vmaValue + (AtrMultiplier * 2 * atrValue);
			LowerBand2[0] = vmaValue - (AtrMultiplier * 2 * atrValue);
			
			UpperBand3[0] = vmaValue + (AtrMultiplier * 3 * atrValue);
			LowerBand3[0] = vmaValue - (AtrMultiplier * 3 * atrValue);
			
			UpperBand4[0] = vmaValue + (AtrMultiplier * 4 * atrValue);
			LowerBand4[0] = vmaValue - (AtrMultiplier * 4 * atrValue);
			
			if(VmaLine[0] > VmaLine[1])
				PlotBrushes[8][0] = UpColor; 
			else if (VmaLine[0] < VmaLine[1])
				PlotBrushes[8][0] = downBrush;
			
			for (int i = 0; i < Values.Length; i++)
			{
				if (CrossAbove(Close, Values[i], 1))
					Draw.Diamond(this, "CrossAbove" + i + CurrentBar, true, 0, Values[i][0], Brushes.Cyan);

				if (CrossBelow(Close, Values[i], 1))
					Draw.Diamond(this, "CrossBelow" + i + CurrentBar, true, 0, Values[i][0], Brushes.Yellow);
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="VMA Period", Order=1, GroupName="Parameters")]
		public int VmaPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="VMA Volatility Period", Order=2, GroupName="Parameters")]
		public int VmaVolatilityPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="Extremes Lookback", Order=3, GroupName="Parameters")]
		public int ExtremesLookback { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "ATR Period", Order = 4, GroupName = "Parameters")]
		public int AtrPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name = "ATR Step Multiplier", Order = 5, GroupName = "Parameters")]
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
		[Browsable(false)] [XmlIgnore] public Series<double> VmaLine { get { return Values[8]; } }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.VMAATRBands[] cacheVMAATRBands;
		public AlgoTrader.VMAATRBands VMAATRBands(int vmaPeriod, int vmaVolatilityPeriod, int extremesLookback, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return VMAATRBands(Input, vmaPeriod, vmaVolatilityPeriod, extremesLookback, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}

		public AlgoTrader.VMAATRBands VMAATRBands(ISeries<double> input, int vmaPeriod, int vmaVolatilityPeriod, int extremesLookback, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			if (cacheVMAATRBands != null)
				for (int idx = 0; idx < cacheVMAATRBands.Length; idx++)
					if (cacheVMAATRBands[idx] != null && cacheVMAATRBands[idx].VmaPeriod == vmaPeriod && cacheVMAATRBands[idx].VmaVolatilityPeriod == vmaVolatilityPeriod && cacheVMAATRBands[idx].ExtremesLookback == extremesLookback && cacheVMAATRBands[idx].AtrPeriod == atrPeriod && cacheVMAATRBands[idx].AtrMultiplier == atrMultiplier && cacheVMAATRBands[idx].UpColor == upColor && cacheVMAATRBands[idx].DownColor == downColor && cacheVMAATRBands[idx].DriverWidth == driverWidth && cacheVMAATRBands[idx].EqualsInput(input))
						return cacheVMAATRBands[idx];
			return CacheIndicator<AlgoTrader.VMAATRBands>(new AlgoTrader.VMAATRBands(){ VmaPeriod = vmaPeriod, VmaVolatilityPeriod = vmaVolatilityPeriod, ExtremesLookback = extremesLookback, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier, UpColor = upColor, DownColor = downColor, DriverWidth = driverWidth }, input, ref cacheVMAATRBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.VMAATRBands VMAATRBands(int vmaPeriod, int vmaVolatilityPeriod, int extremesLookback, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.VMAATRBands(Input, vmaPeriod, vmaVolatilityPeriod, extremesLookback, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}

		public Indicators.AlgoTrader.VMAATRBands VMAATRBands(ISeries<double> input , int vmaPeriod, int vmaVolatilityPeriod, int extremesLookback, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.VMAATRBands(input, vmaPeriod, vmaVolatilityPeriod, extremesLookback, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.VMAATRBands VMAATRBands(int vmaPeriod, int vmaVolatilityPeriod, int extremesLookback, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.VMAATRBands(Input, vmaPeriod, vmaVolatilityPeriod, extremesLookback, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}

		public Indicators.AlgoTrader.VMAATRBands VMAATRBands(ISeries<double> input , int vmaPeriod, int vmaVolatilityPeriod, int extremesLookback, int atrPeriod, double atrMultiplier, Brush upColor, Brush downColor, int driverWidth)
		{
			return indicator.VMAATRBands(input, vmaPeriod, vmaVolatilityPeriod, extremesLookback, atrPeriod, atrMultiplier, upColor, downColor, driverWidth);
		}
	}
}

#endregion
