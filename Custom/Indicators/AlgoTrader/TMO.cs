//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
//
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

// TMO by indiVGA, based on TMO Plus by dart966.
// Original concept and TMO ((T)rue (M)omentum (O)scilator) by Mobius.
// This is a simplified version that only calculates the price-based TMO, with EMA smoothing.

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class TMO : Indicator
	{
		private Series<double> tmoData;
		private Indicator tmoSmoother1;
		private Indicator tmoMainSmoother;
		private Indicator tmoSignalSmoother;

		// Brushes for clouds
		private Brush tmoUpBrush, tmoDownBrush;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"TMO: A simplified version of TMO Plus that calculates momentum using the delta of price, with EMA smoothing.";
				Name										= "TMO";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				// Default Input Parameters
				Length					= 14;
				CalcLength				= 5;
				SmoothLength			= 3;
			}
			else if (State == State.Configure)
			{
				// --- Add Plots ---
				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.Line, "TMO Main");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "TMO Signal");
				
				AddPlot(new Stroke(Brushes.Cyan, 2), PlotStyle.Line, "Zero Line");
				AddPlot(new Stroke(Brushes.Yellow, DashStyleHelper.Dot, 2), PlotStyle.Line, "OverBought Level");
				AddPlot(new Stroke(Brushes.Yellow, DashStyleHelper.Dot, 2), PlotStyle.Line, "OverSold Level");
			}
			else if (State == State.DataLoaded)
			{
				tmoData 	= new Series<double>(this);

				tmoSmoother1 	 = EMA(tmoData, CalcLength);
				tmoMainSmoother  = EMA(tmoSmoother1, SmoothLength);
				tmoSignalSmoother = EMA(tmoMainSmoother, SmoothLength);

				tmoUpBrush = new SolidColorBrush(Colors.Green) { Opacity = 0.1 }; tmoUpBrush.Freeze();
				tmoDownBrush = new SolidColorBrush(Colors.Red) { Opacity = 0.1 }; tmoDownBrush.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Length)
				return;

			// --- 1. TMO (True Momentum Oscillator) on Price ---
			double tmoSum = 0;
			for (int i = 0; i < Length; i++)
			{
				if (Close[0] > Open[i])
					tmoSum += 1;
				else if (Close[0] < Open[i])
					tmoSum -= 1;
			}
			tmoData[0] = tmoSum;

			Main[0] = tmoMainSmoother.Values[0][0];
			Signal[0] = tmoSignalSmoother.Values[0][0];
			
			if (Main[0] > Signal[0])
			{
				PlotBrushes[0][0] = Brushes.Lime;
				PlotBrushes[1][0] = Brushes.Green;
				Draw.Region(this, "TMOCloudUp" + CurrentBar, 0, 1, Main, Signal, null, tmoUpBrush, 10);
			}
			else
			{
				PlotBrushes[0][0] = Brushes.Red;
				PlotBrushes[1][0] = Brushes.Orange;
				Draw.Region(this, "TMOCloudDown" + CurrentBar, 0, 1, Signal, Main, null, tmoDownBrush, 10);
			}

			// --- 2. Horizontal Lines ---
			ZeroLine[0] = 0;
			OverBought[0] = Math.Round(Length * 0.7);
			OverSold[0] = -Math.Round(Length * 0.7);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Lookback Length", Description="Main lookback period for the momentum calculation.", Order=1, GroupName="Parameters")]
		public int Length { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Calculation Length", Description="Period for the first smoothing.", Order=2, GroupName="Parameters")]
		public int CalcLength { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smoothing Length", Description="Period for the second (Main) and third (Signal) smoothing.", Order=3, GroupName="Parameters")]
		public int SmoothLength { get; set; }
		#endregion

		#region Output Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Main
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ZeroLine
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OverBought
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OverSold
		{
			get { return Values[4]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.TMO[] cacheTMO;
		public AlgoTrader.TMO TMO(int length, int calcLength, int smoothLength)
		{
			return TMO(Input, length, calcLength, smoothLength);
		}

		public AlgoTrader.TMO TMO(ISeries<double> input, int length, int calcLength, int smoothLength)
		{
			if (cacheTMO != null)
				for (int idx = 0; idx < cacheTMO.Length; idx++)
					if (cacheTMO[idx] != null && cacheTMO[idx].Length == length && cacheTMO[idx].CalcLength == calcLength && cacheTMO[idx].SmoothLength == smoothLength && cacheTMO[idx].EqualsInput(input))
						return cacheTMO[idx];
			return CacheIndicator<AlgoTrader.TMO>(new AlgoTrader.TMO(){ Length = length, CalcLength = calcLength, SmoothLength = smoothLength }, input, ref cacheTMO);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.TMO TMO(int length, int calcLength, int smoothLength)
		{
			return indicator.TMO(Input, length, calcLength, smoothLength);
		}

		public Indicators.AlgoTrader.TMO TMO(ISeries<double> input , int length, int calcLength, int smoothLength)
		{
			return indicator.TMO(input, length, calcLength, smoothLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.TMO TMO(int length, int calcLength, int smoothLength)
		{
			return indicator.TMO(Input, length, calcLength, smoothLength);
		}

		public Indicators.AlgoTrader.TMO TMO(ISeries<double> input , int length, int calcLength, int smoothLength)
		{
			return indicator.TMO(input, length, calcLength, smoothLength);
		}
	}
}

#endregion
