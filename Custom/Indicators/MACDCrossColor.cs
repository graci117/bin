// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The MACDCrossColor (Moving Average Convergence/Divergence) is a trend following momentum indicator 
	/// that shows the relationship between two moving averages of prices.
	/// </summary>
	public class MACDCrossColor : Indicator
	{
		private	Series<double>		fastEma;
		private	Series<double>		slowEma;
		private double				constant1;
		private double				constant2;
		private double				constant3;
		private double				constant4;
		private double				constant5;
		private double				constant6;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionMACD;
				Name						= "MACDCrossColor";
				Fast						= 12;
				IsSuspendedWhileInactive	= true;
				Slow						= 26;
				Smooth						= 9;

				AddPlot(Brushes.Green,									NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameMACD);
				AddPlot(Brushes.DarkViolet,								NinjaTrader.Custom.Resource.NinjaScriptIndicatorAvg);
				AddPlot(new Stroke(Brushes.Navy, 2),	PlotStyle.Bar,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorDiff);
				AddLine(Brushes.DarkGray,				0,				NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
			}
			else if (State == State.Configure)
			{
				constant1	= 2.0 / (1 + Fast);
				constant2	= (1 - (2.0 / (1 + Fast)));
				constant3	= 2.0 / (1 + Slow);
				constant4	= (1 - (2.0 / (1 + Slow)));
				constant5	= 2.0 / (1 + Smooth);
				constant6	= (1 - (2.0 / (1 + Smooth)));
				fastEma		= new Series<double>(this);
				slowEma		= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			double input0	= Input[0];

			if (CurrentBar == 0)
			{	
				fastEma[0]		= input0;
				slowEma[0]		= input0;
				Value[0]		= 0;
				Avg[0]			= 0;
				Diff[0]			= 0;
			}
			else
			{
				double fastEma0	= constant1 * input0 + constant2 * fastEma[1];
				double slowEma0	= constant3 * input0 + constant4 * slowEma[1];
				double macd		= fastEma0 - slowEma0;
				double macdAvg	= constant5 * macd + constant6 * Avg[1];

				fastEma[0]		= fastEma0;
				slowEma[0]		= slowEma0;
				Value[0]		= macd;
				Avg[0]			= macdAvg;
				Diff[0]			= macd - macdAvg;
			}
			
			if (Diff[0] >= 0)
			{
				PlotBrushes[2][0] = Brushes.Green;
			}
			else PlotBrushes[2][0] = Brushes.Red;
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Default
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Diff
		{
			get { return Values[2]; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Slow
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "NinjaScriptParameters", Order = 2)]
		public int Smooth
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDCrossColor[] cacheMACDCrossColor;
		public MACDCrossColor MACDCrossColor(int fast, int slow, int smooth)
		{
			return MACDCrossColor(Input, fast, slow, smooth);
		}

		public MACDCrossColor MACDCrossColor(ISeries<double> input, int fast, int slow, int smooth)
		{
			if (cacheMACDCrossColor != null)
				for (int idx = 0; idx < cacheMACDCrossColor.Length; idx++)
					if (cacheMACDCrossColor[idx] != null && cacheMACDCrossColor[idx].Fast == fast && cacheMACDCrossColor[idx].Slow == slow && cacheMACDCrossColor[idx].Smooth == smooth && cacheMACDCrossColor[idx].EqualsInput(input))
						return cacheMACDCrossColor[idx];
			return CacheIndicator<MACDCrossColor>(new MACDCrossColor(){ Fast = fast, Slow = slow, Smooth = smooth }, input, ref cacheMACDCrossColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDCrossColor MACDCrossColor(int fast, int slow, int smooth)
		{
			return indicator.MACDCrossColor(Input, fast, slow, smooth);
		}

		public Indicators.MACDCrossColor MACDCrossColor(ISeries<double> input , int fast, int slow, int smooth)
		{
			return indicator.MACDCrossColor(input, fast, slow, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDCrossColor MACDCrossColor(int fast, int slow, int smooth)
		{
			return indicator.MACDCrossColor(Input, fast, slow, smooth);
		}

		public Indicators.MACDCrossColor MACDCrossColor(ISeries<double> input , int fast, int slow, int smooth)
		{
			return indicator.MACDCrossColor(input, fast, slow, smooth);
		}
	}
}

#endregion
