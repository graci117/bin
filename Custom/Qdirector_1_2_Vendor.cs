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
		
		private Qdirector[] cacheQdirector;

		
		public Qdirector Qdirector(bool alerts, int period, PRICE_DIRECTOR_SOURCE price, double filter1, double filter2, int maxShelfSize)
		{
			return Qdirector(Input, alerts, period, price, filter1, filter2, maxShelfSize);
		}


		
		public Qdirector Qdirector(ISeries<double> input, bool alerts, int period, PRICE_DIRECTOR_SOURCE price, double filter1, double filter2, int maxShelfSize)
		{
			if (cacheQdirector != null)
				for (int idx = 0; idx < cacheQdirector.Length; idx++)
					if (cacheQdirector[idx].Alerts == alerts && cacheQdirector[idx].Period == period && cacheQdirector[idx].Price == price && cacheQdirector[idx].Filter1 == filter1 && cacheQdirector[idx].Filter2 == filter2 && cacheQdirector[idx].MaxShelfSize == maxShelfSize && cacheQdirector[idx].EqualsInput(input))
						return cacheQdirector[idx];
			return CacheIndicator<Qdirector>(new Qdirector(){ Alerts = alerts, Period = period, Price = price, Filter1 = filter1, Filter2 = filter2, MaxShelfSize = maxShelfSize }, input, ref cacheQdirector);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.Qdirector Qdirector(bool alerts, int period, PRICE_DIRECTOR_SOURCE price, double filter1, double filter2, int maxShelfSize)
		{
			return indicator.Qdirector(Input, alerts, period, price, filter1, filter2, maxShelfSize);
		}


		
		public Indicators.Qdirector Qdirector(ISeries<double> input , bool alerts, int period, PRICE_DIRECTOR_SOURCE price, double filter1, double filter2, int maxShelfSize)
		{
			return indicator.Qdirector(input, alerts, period, price, filter1, filter2, maxShelfSize);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.Qdirector Qdirector(bool alerts, int period, PRICE_DIRECTOR_SOURCE price, double filter1, double filter2, int maxShelfSize)
		{
			return indicator.Qdirector(Input, alerts, period, price, filter1, filter2, maxShelfSize);
		}


		
		public Indicators.Qdirector Qdirector(ISeries<double> input , bool alerts, int period, PRICE_DIRECTOR_SOURCE price, double filter1, double filter2, int maxShelfSize)
		{
			return indicator.Qdirector(input, alerts, period, price, filter1, filter2, maxShelfSize);
		}

	}
}

#endregion
