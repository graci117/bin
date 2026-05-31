//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
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

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// This indicator returns 1 when we have n of consecutive bars up, otherwise returns 0.
	/// An up bar is defined as a bar where the close is above the open and the bars makes a higher
	/// high and a higher low. You can adjust the specific requirements with the indicator options.
	/// </summary>
	public class NBarsUpCustom : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionNBarsUp;
				Name						= "NBarsUpCustom";
				BarCount					= 3;
				BarUp						= true;
				HigherHigh					= true;
				HigherLow					= true;
				IsSuspendedWhileInactive	= true;

				AddPlot(new Stroke(Brushes.DarkCyan, 2), PlotStyle.Bar, NinjaTrader.Custom.Resource.NinjaScriptIndicatorDiff);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarCount)
			{
				Value[0] = 0;
			}
			else
			{
				bool gotBars = false;

				for (int i = 0; i < BarCount + 1; i++)
				{
					if (i == BarCount)
					{
						gotBars = true;
						break;
					}

					if (!(Close[i] > Open[i]))
						break;

					if (BarUp && !(Close[i] > Open[i]))
						break;

					if (HigherHigh && !(High[i] > High[i + 1]))
						break;

					if (HigherLow && !(Low[i] > Low[i + 1]))
						break;
				}

				Value[0] = gotBars ? 1 : 0;
			}
		}


		#region Properties
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarCount", GroupName = "NinjaScriptParameters", Order = 0)]
		public int BarCount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarUp", GroupName = "NinjaScriptParameters", Order = 1)]
		public bool BarUp
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HigherHigh", GroupName = "NinjaScriptParameters", Order = 2)]
		public bool HigherHigh
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HigherLow", GroupName = "NinjaScriptParameters", Order = 3)]
		public bool HigherLow
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private NBarsUpCustom[] cacheNBarsUpCustom;
		public NBarsUpCustom NBarsUpCustom(int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return NBarsUpCustom(Input, barCount, barUp, higherHigh, higherLow);
		}

		public NBarsUpCustom NBarsUpCustom(ISeries<double> input, int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			if (cacheNBarsUpCustom != null)
				for (int idx = 0; idx < cacheNBarsUpCustom.Length; idx++)
					if (cacheNBarsUpCustom[idx] != null && cacheNBarsUpCustom[idx].BarCount == barCount && cacheNBarsUpCustom[idx].BarUp == barUp && cacheNBarsUpCustom[idx].HigherHigh == higherHigh && cacheNBarsUpCustom[idx].HigherLow == higherLow && cacheNBarsUpCustom[idx].EqualsInput(input))
						return cacheNBarsUpCustom[idx];
			return CacheIndicator<NBarsUpCustom>(new NBarsUpCustom(){ BarCount = barCount, BarUp = barUp, HigherHigh = higherHigh, HigherLow = higherLow }, input, ref cacheNBarsUpCustom);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.NBarsUpCustom NBarsUpCustom(int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.NBarsUpCustom(Input, barCount, barUp, higherHigh, higherLow);
		}

		public Indicators.NBarsUpCustom NBarsUpCustom(ISeries<double> input , int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.NBarsUpCustom(input, barCount, barUp, higherHigh, higherLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.NBarsUpCustom NBarsUpCustom(int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.NBarsUpCustom(Input, barCount, barUp, higherHigh, higherLow);
		}

		public Indicators.NBarsUpCustom NBarsUpCustom(ISeries<double> input , int barCount, bool barUp, bool higherHigh, bool higherLow)
		{
			return indicator.NBarsUpCustom(input, barCount, barUp, higherHigh, higherLow);
		}
	}
}

#endregion
