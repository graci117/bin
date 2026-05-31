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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BlueZTradeZone : Indicator
	{
		private MIN MinLow;
		private MAX MaxHigh;
		private Series<double> RelDiff, Diff, SMISeries;
		private EMA EMA0, EMA1, AvgRel, AvgDiff, SMIEMA;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"The Stochastic Momentum Index indicator as published in the October 2020 Stocks and Commodities article titled “Swing Trade With The Gann Hi-Lo Activator” by Barbara Star, PhD";
				Name										= "BlueZTradeZone";
				Calculate									= Calculate.OnPriceChange;//OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				
				OverBought					= 55;//40;
				OverSold					= -55;//-40;
				PercentDLength					= 3;
				PercentKLength					= 1;//8;
//				AddPlot(Brushes.LimeGreen, "BlueZTradeZonePlot");//SMIPlot");
				AddPlot(new Stroke(Brushes.LimeGreen, 4), PlotStyle.Line,  "BlueZTradeZonePlot");//0
//				AddPlot(Brushes.Gray, "BlueZTradeZoneAVGPlot");//SMIAVGPlot");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line,  "BlueZTradeZoneAVGPlot");//1
				PosColor = Brushes.LimeGreen;
				NegColor = Brushes.Red;
				
			}
			else if (State == State.Configure)
			{
				AddLine(Brushes.Transparent, 0, "ZeroLine");
				AddLine(Brushes.Blue, OverBought, "OverBoughtLine");
				AddLine(Brushes.HotPink, OverSold, "OverSoldLine");
			}
			else if (State == State.DataLoaded)
			{
				MinLow = MIN(Low, PercentKLength);
				MaxHigh = MAX(High, PercentKLength);
				RelDiff = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Diff = new Series<double>(this, MaximumBarsLookBack.Infinite);
				SMISeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				EMA0 = EMA(RelDiff, PercentDLength);
				AvgRel = EMA(EMA0, PercentDLength);
				EMA1 = EMA(Diff, PercentDLength);
				AvgDiff = EMA(EMA1, PercentDLength);
				SMIEMA = EMA(SMISeries, PercentDLength);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < Math.Max(PercentKLength, PercentDLength)) return;
			
			RelDiff[0] = Close[0] - (MaxHigh[0] + MinLow[0])/2;
			Diff[0] = MaxHigh[0] - MinLow[0];
			
			if(AvgDiff[0] != 0)
			{
				SMISeries[0] = AvgRel[0]/(AvgDiff[0]/2)*100;
				if(SMISeries[0] > 0)
				{
					PlotBrushes[0][0] = PosColor;
				}
				else
				{
					PlotBrushes[0][0] = NegColor;
				}
				BlueZTradeZonePlot[0] = SMISeries[0];
			}
			else
			{
				SMISeries[0] = 0;
				BlueZTradeZonePlot[0] = SMISeries[0];
			}
			
			BlueZTradeZoneAVGPlot[0] = SMIEMA[0];
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BlueZTradeZonePlot
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BlueZTradeZoneAVGPlot
		{
			get { return Values[1]; }
		}
		
		[NinjaScriptProperty]
		[Range(double.MinValue, double.MaxValue)]
		[Display(Name="OverBought", Order=1, GroupName="Parameters")]
		public double OverBought
		{ get; set; }

		[NinjaScriptProperty]
		[Range(double.MinValue, double.MaxValue)]
		[Display(Name="OverSold", Order=2, GroupName="Parameters")]
		public double OverSold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PercentDLength", Order=3, GroupName="Parameters")]
		public int PercentDLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PercentKLength", Order=4, GroupName="Parameters")]
		public int PercentKLength
		{ get; set; }
		
		[XmlIgnore]
		public Brush PosColor { get; set; }
		
		[Browsable(false)]
		public string PosSerialize
		{
		  get { return Serialize.BrushToString(PosColor); }
		  set { PosColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		public Brush NegColor { get; set; }
		
		[Browsable(false)]
		public string NegSerialize
		{
		  get { return Serialize.BrushToString(NegColor); }
		  set { 
			  NegColor = Serialize.StringToBrush(value); }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BlueZTradeZone[] cacheBlueZTradeZone;
		public BlueZTradeZone BlueZTradeZone(double overBought, double overSold, int percentDLength, int percentKLength)
		{
			return BlueZTradeZone(Input, overBought, overSold, percentDLength, percentKLength);
		}

		public BlueZTradeZone BlueZTradeZone(ISeries<double> input, double overBought, double overSold, int percentDLength, int percentKLength)
		{
			if (cacheBlueZTradeZone != null)
				for (int idx = 0; idx < cacheBlueZTradeZone.Length; idx++)
					if (cacheBlueZTradeZone[idx] != null && cacheBlueZTradeZone[idx].OverBought == overBought && cacheBlueZTradeZone[idx].OverSold == overSold && cacheBlueZTradeZone[idx].PercentDLength == percentDLength && cacheBlueZTradeZone[idx].PercentKLength == percentKLength && cacheBlueZTradeZone[idx].EqualsInput(input))
						return cacheBlueZTradeZone[idx];
			return CacheIndicator<BlueZTradeZone>(new BlueZTradeZone(){ OverBought = overBought, OverSold = overSold, PercentDLength = percentDLength, PercentKLength = percentKLength }, input, ref cacheBlueZTradeZone);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BlueZTradeZone BlueZTradeZone(double overBought, double overSold, int percentDLength, int percentKLength)
		{
			return indicator.BlueZTradeZone(Input, overBought, overSold, percentDLength, percentKLength);
		}

		public Indicators.BlueZTradeZone BlueZTradeZone(ISeries<double> input , double overBought, double overSold, int percentDLength, int percentKLength)
		{
			return indicator.BlueZTradeZone(input, overBought, overSold, percentDLength, percentKLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BlueZTradeZone BlueZTradeZone(double overBought, double overSold, int percentDLength, int percentKLength)
		{
			return indicator.BlueZTradeZone(Input, overBought, overSold, percentDLength, percentKLength);
		}

		public Indicators.BlueZTradeZone BlueZTradeZone(ISeries<double> input , double overBought, double overSold, int percentDLength, int percentKLength)
		{
			return indicator.BlueZTradeZone(input, overBought, overSold, percentDLength, percentKLength);
		}
	}
}

#endregion
