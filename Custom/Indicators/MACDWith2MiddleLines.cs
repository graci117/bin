//
// Copyright (C) 2024, NinjaTrader LLC <www.ninjatrader.com>.
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
	/// The MACD (Moving Average Convergence/Divergence) is a trend following momentum indicator
	/// that shows the relationship between two moving averages of prices.
	/// </summary>
	public class MACDWith2MiddleLines : Indicator
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
				Description					= Custom.Resource.NinjaScriptIndicatorDescriptionMACD;
				Name						= "MACDWith2MiddleLines";
				Fast						= 2;
				IsSuspendedWhileInactive	= true;
				Slow						= 20;
				Smooth						= 20;

				AddPlot(Brushes.DarkCyan,									Custom.Resource.NinjaScriptIndicatorNameMACD);
				AddPlot(Brushes.Crimson,									Custom.Resource.NinjaScriptIndicatorAvg);
				AddPlot(new Stroke(Brushes.Transparent, 2),	PlotStyle.Bar,	Custom.Resource.NinjaScriptIndicatorDiff);
				//AddLine(Brushes.DarkGray,					0,				Custom.Resource.NinjaScriptIndicatorZeroLine);
				
				Stroke myStroke = new Stroke(Brushes.Chartreuse,3);
				
				AddLine(myStroke,					-5,				Custom.Resource.NinjaScriptIndicatorLower);
				
				AddLine(myStroke,					5,				Custom.Resource.NinjaScriptIndicatorUpper);
				//Plots[4].Width = 3;
			}
			else if (State == State.Configure)
			{
				constant1	= 2.0 / (1 + Fast);
				constant2	= 1 - (2.0 / (1 + Fast));
				constant3	= 2.0 / (1 + Slow);
				constant4	= 1 - (2.0 / (1 + Slow));
				constant5	= 2.0 / (1 + Smooth);
				constant6	= 1 - (2.0 / (1 + Smooth));
			}
			else if (State == State.DataLoaded)
			{
				fastEma = new Series<double>(this);
				slowEma = new Series<double>(this);
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
		private MACDWith2MiddleLines[] cacheMACDWith2MiddleLines;
		public MACDWith2MiddleLines MACDWith2MiddleLines(int fast, int slow, int smooth)
		{
			return MACDWith2MiddleLines(Input, fast, slow, smooth);
		}

		public MACDWith2MiddleLines MACDWith2MiddleLines(ISeries<double> input, int fast, int slow, int smooth)
		{
			if (cacheMACDWith2MiddleLines != null)
				for (int idx = 0; idx < cacheMACDWith2MiddleLines.Length; idx++)
					if (cacheMACDWith2MiddleLines[idx] != null && cacheMACDWith2MiddleLines[idx].Fast == fast && cacheMACDWith2MiddleLines[idx].Slow == slow && cacheMACDWith2MiddleLines[idx].Smooth == smooth && cacheMACDWith2MiddleLines[idx].EqualsInput(input))
						return cacheMACDWith2MiddleLines[idx];
			return CacheIndicator<MACDWith2MiddleLines>(new MACDWith2MiddleLines(){ Fast = fast, Slow = slow, Smooth = smooth }, input, ref cacheMACDWith2MiddleLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDWith2MiddleLines MACDWith2MiddleLines(int fast, int slow, int smooth)
		{
			return indicator.MACDWith2MiddleLines(Input, fast, slow, smooth);
		}

		public Indicators.MACDWith2MiddleLines MACDWith2MiddleLines(ISeries<double> input , int fast, int slow, int smooth)
		{
			return indicator.MACDWith2MiddleLines(input, fast, slow, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDWith2MiddleLines MACDWith2MiddleLines(int fast, int slow, int smooth)
		{
			return indicator.MACDWith2MiddleLines(Input, fast, slow, smooth);
		}

		public Indicators.MACDWith2MiddleLines MACDWith2MiddleLines(ISeries<double> input , int fast, int slow, int smooth)
		{
			return indicator.MACDWith2MiddleLines(input, fast, slow, smooth);
		}
	}
}

#endregion
