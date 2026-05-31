#region Using declarations
using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class SimpleMoneyMetricsMain : Indicator
	{
		private Series<int> _signal;
		private Series<double> _profitWaveSlow;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "SimpleMoneyMetricsMain";
				Description = "Lightweight version exposing Signal and ProfitWaveSlow for SMMCombinedSignalPro.";
				IsOverlay = true;
				Calculate = Calculate.OnBarClose;
				DisplayInDataBox = true;
				AddPlot(Brushes.Orange, "ProfitWaveSlow");
			}
			else if (State == State.DataLoaded)
			{
				_signal = new Series<int>(this);
				_profitWaveSlow = new Series<double>(this);
			}
		}

	protected override void OnBarUpdate()
{
	if (CurrentBar < 20)
	{
		_signal[0] = 0;
		_profitWaveSlow[0] = 0;
		return;
	}

	// --- Simple logic: 10 vs 20 EMA cross ---
	double fast = EMA(Close, 10)[0];
	double slow = EMA(Close, 20)[0];

	// Output +1 or -1 explicitly
	if (fast > slow)
		_signal[0] = 1;
	else if (fast < slow)
		_signal[0] = -1;
	else
		_signal[0] = _signal[1];  // carry previous state

	_profitWaveSlow[0] = slow;
	Values[0][0] = slow;
}


		#region === Exposed Properties ===
		[Browsable(false), XmlIgnore]
		public Series<int> Signal => _signal;

		[Browsable(false), XmlIgnore]
		public Series<double> ProfitWaveSlow => _profitWaveSlow;
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SimpleMoneyMetricsMain[] cacheSimpleMoneyMetricsMain;
		public SimpleMoneyMetricsMain SimpleMoneyMetricsMain()
		{
			return SimpleMoneyMetricsMain(Input);
		}

		public SimpleMoneyMetricsMain SimpleMoneyMetricsMain(ISeries<double> input)
		{
			if (cacheSimpleMoneyMetricsMain != null)
				for (int idx = 0; idx < cacheSimpleMoneyMetricsMain.Length; idx++)
					if (cacheSimpleMoneyMetricsMain[idx] != null &&  cacheSimpleMoneyMetricsMain[idx].EqualsInput(input))
						return cacheSimpleMoneyMetricsMain[idx];
			return CacheIndicator<SimpleMoneyMetricsMain>(new SimpleMoneyMetricsMain(), input, ref cacheSimpleMoneyMetricsMain);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleMoneyMetricsMain SimpleMoneyMetricsMain()
		{
			return indicator.SimpleMoneyMetricsMain(Input);
		}

		public Indicators.SimpleMoneyMetricsMain SimpleMoneyMetricsMain(ISeries<double> input )
		{
			return indicator.SimpleMoneyMetricsMain(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleMoneyMetricsMain SimpleMoneyMetricsMain()
		{
			return indicator.SimpleMoneyMetricsMain(Input);
		}

		public Indicators.SimpleMoneyMetricsMain SimpleMoneyMetricsMain(ISeries<double> input )
		{
			return indicator.SimpleMoneyMetricsMain(input);
		}
	}
}

#endregion
