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
	/// The Momentum indicator measures the amount that a security's price has changed over a given time span.
	/// </summary>
	public class Momentum2 : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionMomentum;
				Name						= "Momentum2";
				Calculate					= Calculate.OnPriceChange;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Yellow,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameMomentum);
				AddLine(Brushes.DodgerBlue,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
			}
		}

		protected override void OnBarUpdate()
		{
			Value[0] = CurrentBar == 0 ? 0 : Input[0] - Input[Math.Min(CurrentBar, Period)];
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Momentum2[] cacheMomentum2;
		public Momentum2 Momentum2(int period)
		{
			return Momentum2(Input, period);
		}

		public Momentum2 Momentum2(ISeries<double> input, int period)
		{
			if (cacheMomentum2 != null)
				for (int idx = 0; idx < cacheMomentum2.Length; idx++)
					if (cacheMomentum2[idx] != null && cacheMomentum2[idx].Period == period && cacheMomentum2[idx].EqualsInput(input))
						return cacheMomentum2[idx];
			return CacheIndicator<Momentum2>(new Momentum2(){ Period = period }, input, ref cacheMomentum2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Momentum2 Momentum2(int period)
		{
			return indicator.Momentum2(Input, period);
		}

		public Indicators.Momentum2 Momentum2(ISeries<double> input , int period)
		{
			return indicator.Momentum2(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Momentum2 Momentum2(int period)
		{
			return indicator.Momentum2(Input, period);
		}

		public Indicators.Momentum2 Momentum2(ISeries<double> input , int period)
		{
			return indicator.Momentum2(input, period);
		}
	}
}

#endregion
