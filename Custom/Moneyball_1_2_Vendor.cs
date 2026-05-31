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

#endregion



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private Moneyball[] cacheMoneyball;

		
		public Moneyball Moneyball(Brush upColor, Brush downColor, int nb_bars, int period, bool all_zero, double upperThreshold, double lowerThreshold, double sensitivity, MoneyballMode mode, bool alerts)
		{
			return Moneyball(Input, upColor, downColor, nb_bars, period, all_zero, upperThreshold, lowerThreshold, sensitivity, mode, alerts);
		}


		
		public Moneyball Moneyball(ISeries<double> input, Brush upColor, Brush downColor, int nb_bars, int period, bool all_zero, double upperThreshold, double lowerThreshold, double sensitivity, MoneyballMode mode, bool alerts)
		{
			if (cacheMoneyball != null)
				for (int idx = 0; idx < cacheMoneyball.Length; idx++)
					if (cacheMoneyball[idx].UpColor == upColor && cacheMoneyball[idx].DownColor == downColor && cacheMoneyball[idx].Nb_bars == nb_bars && cacheMoneyball[idx].Period == period && cacheMoneyball[idx].All_zero == all_zero && cacheMoneyball[idx].UpperThreshold == upperThreshold && cacheMoneyball[idx].LowerThreshold == lowerThreshold && cacheMoneyball[idx].Sensitivity == sensitivity && cacheMoneyball[idx].Mode == mode && cacheMoneyball[idx].Alerts == alerts && cacheMoneyball[idx].EqualsInput(input))
						return cacheMoneyball[idx];
			return CacheIndicator<Moneyball>(new Moneyball(){ UpColor = upColor, DownColor = downColor, Nb_bars = nb_bars, Period = period, All_zero = all_zero, UpperThreshold = upperThreshold, LowerThreshold = lowerThreshold, Sensitivity = sensitivity, Mode = mode, Alerts = alerts }, input, ref cacheMoneyball);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.Moneyball Moneyball(Brush upColor, Brush downColor, int nb_bars, int period, bool all_zero, double upperThreshold, double lowerThreshold, double sensitivity, MoneyballMode mode, bool alerts)
		{
			return indicator.Moneyball(Input, upColor, downColor, nb_bars, period, all_zero, upperThreshold, lowerThreshold, sensitivity, mode, alerts);
		}


		
		public Indicators.Moneyball Moneyball(ISeries<double> input , Brush upColor, Brush downColor, int nb_bars, int period, bool all_zero, double upperThreshold, double lowerThreshold, double sensitivity, MoneyballMode mode, bool alerts)
		{
			return indicator.Moneyball(input, upColor, downColor, nb_bars, period, all_zero, upperThreshold, lowerThreshold, sensitivity, mode, alerts);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.Moneyball Moneyball(Brush upColor, Brush downColor, int nb_bars, int period, bool all_zero, double upperThreshold, double lowerThreshold, double sensitivity, MoneyballMode mode, bool alerts)
		{
			return indicator.Moneyball(Input, upColor, downColor, nb_bars, period, all_zero, upperThreshold, lowerThreshold, sensitivity, mode, alerts);
		}


		
		public Indicators.Moneyball Moneyball(ISeries<double> input , Brush upColor, Brush downColor, int nb_bars, int period, bool all_zero, double upperThreshold, double lowerThreshold, double sensitivity, MoneyballMode mode, bool alerts)
		{
			return indicator.Moneyball(input, upColor, downColor, nb_bars, period, all_zero, upperThreshold, lowerThreshold, sensitivity, mode, alerts);
		}

	}
}

#endregion
