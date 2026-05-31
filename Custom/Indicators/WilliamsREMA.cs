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

namespace NinjaTrader.NinjaScript.Indicators
{
	public class WilliamsREMA : Indicator
	{
		private MAX max;
		private MIN min;
		private EMA ema;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "WilliamsR + EMA";
				Name						= "WilliamsREMA";
				IsSuspendedWhileInactive	= true;

				AddLine(Brushes.Black,	-20,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddLine(Brushes.Black,	-80,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddPlot(Brushes.Yellow,			NinjaTrader.Custom.Resource.WilliamsPercentR);
				AddPlot(Brushes.Cyan,		"EMA");
			}
			else if (State == State.DataLoaded)
			{
				max = MAX(High, Period);
				min	= MIN(Low, Period);
				ema = EMA(Value, EMAPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			double max0	= max[0];
			double min0	= min[0];
			Value[0]	= -100 * (max0 - Close[0]) / (max0 - min0 == 0 ? 1 : max0 - min0);
			Values[1][0] = ema[0];
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; } = 21;

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMAPeriod", GroupName = "NinjaScriptParameters", Order = 1)]
		public int EMAPeriod
		{ get; set; } = 13;
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WilliamsREMA[] cacheWilliamsREMA;
		public WilliamsREMA WilliamsREMA(int period, int eMAPeriod)
		{
			return WilliamsREMA(Input, period, eMAPeriod);
		}

		public WilliamsREMA WilliamsREMA(ISeries<double> input, int period, int eMAPeriod)
		{
			if (cacheWilliamsREMA != null)
				for (int idx = 0; idx < cacheWilliamsREMA.Length; idx++)
					if (cacheWilliamsREMA[idx] != null && cacheWilliamsREMA[idx].Period == period && cacheWilliamsREMA[idx].EMAPeriod == eMAPeriod && cacheWilliamsREMA[idx].EqualsInput(input))
						return cacheWilliamsREMA[idx];
			return CacheIndicator<WilliamsREMA>(new WilliamsREMA(){ Period = period, EMAPeriod = eMAPeriod }, input, ref cacheWilliamsREMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WilliamsREMA WilliamsREMA(int period, int eMAPeriod)
		{
			return indicator.WilliamsREMA(Input, period, eMAPeriod);
		}

		public Indicators.WilliamsREMA WilliamsREMA(ISeries<double> input , int period, int eMAPeriod)
		{
			return indicator.WilliamsREMA(input, period, eMAPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WilliamsREMA WilliamsREMA(int period, int eMAPeriod)
		{
			return indicator.WilliamsREMA(Input, period, eMAPeriod);
		}

		public Indicators.WilliamsREMA WilliamsREMA(ISeries<double> input , int period, int eMAPeriod)
		{
			return indicator.WilliamsREMA(input, period, eMAPeriod);
		}
	}
}

#endregion
