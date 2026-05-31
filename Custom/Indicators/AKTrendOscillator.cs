//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
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
	/// The Price Oscillator indicator shows the variation among two moving averages for the price of a security.
	/// </summary>
	public class AKTrendOscillator : Indicator
	{
		private	EMA					emaAktrendInput1;
		private EMA					emaAktrendInput2;
		private	Series<double>		aktrend_bspread;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionPriceOscillator;
				Name						= "AKTrendOscillator";
				aktrend_input1						= 3;
				IsSuspendedWhileInactive	= true;
				aktrend_input2						= 8;

				AddLine(Brushes.DarkGray,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddPlot(Brushes.Goldenrod,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorNamePriceOscillator);
			}
			else if (State == State.DataLoaded)
			{
				aktrend_bspread	= new Series<double>(this);
				emaAktrendInput1		= EMA(aktrend_input1);
				emaAktrendInput2		= EMA(aktrend_input2);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 35)
				return;
			aktrend_bspread[0]	= (emaAktrendInput1[0] - emaAktrendInput2[0]) *1.001;
			Value[0]		= aktrend_bspread[0];
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "aktrend_input1", GroupName = "NinjaScriptParameters", Order = 0)]
		public int aktrend_input1
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "aktrend_input2", GroupName = "NinjaScriptParameters", Order = 1)]
		public int aktrend_input2
		{ get; set; }

	
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AKTrendOscillator[] cacheAKTrendOscillator;
		public AKTrendOscillator AKTrendOscillator(int aktrend_input1, int aktrend_input2)
		{
			return AKTrendOscillator(Input, aktrend_input1, aktrend_input2);
		}

		public AKTrendOscillator AKTrendOscillator(ISeries<double> input, int aktrend_input1, int aktrend_input2)
		{
			if (cacheAKTrendOscillator != null)
				for (int idx = 0; idx < cacheAKTrendOscillator.Length; idx++)
					if (cacheAKTrendOscillator[idx] != null && cacheAKTrendOscillator[idx].aktrend_input1 == aktrend_input1 && cacheAKTrendOscillator[idx].aktrend_input2 == aktrend_input2 && cacheAKTrendOscillator[idx].EqualsInput(input))
						return cacheAKTrendOscillator[idx];
			return CacheIndicator<AKTrendOscillator>(new AKTrendOscillator(){ aktrend_input1 = aktrend_input1, aktrend_input2 = aktrend_input2 }, input, ref cacheAKTrendOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AKTrendOscillator AKTrendOscillator(int aktrend_input1, int aktrend_input2)
		{
			return indicator.AKTrendOscillator(Input, aktrend_input1, aktrend_input2);
		}

		public Indicators.AKTrendOscillator AKTrendOscillator(ISeries<double> input , int aktrend_input1, int aktrend_input2)
		{
			return indicator.AKTrendOscillator(input, aktrend_input1, aktrend_input2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AKTrendOscillator AKTrendOscillator(int aktrend_input1, int aktrend_input2)
		{
			return indicator.AKTrendOscillator(Input, aktrend_input1, aktrend_input2);
		}

		public Indicators.AKTrendOscillator AKTrendOscillator(ISeries<double> input , int aktrend_input1, int aktrend_input2)
		{
			return indicator.AKTrendOscillator(input, aktrend_input1, aktrend_input2);
		}
	}
}

#endregion
