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
		
		private ninZaDragonTrend[] cacheninZaDragonTrend;

		
		public ninZaDragonTrend ninZaDragonTrend(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return ninZaDragonTrend(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}


		
		public ninZaDragonTrend ninZaDragonTrend(ISeries<double> input, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			if (cacheninZaDragonTrend != null)
				for (int idx = 0; idx < cacheninZaDragonTrend.Length; idx++)
					if (cacheninZaDragonTrend[idx].Period == period && cacheninZaDragonTrend[idx].SmoothingEnabled == smoothingEnabled && cacheninZaDragonTrend[idx].SmoothingMethod == smoothingMethod && cacheninZaDragonTrend[idx].SmoothingPeriod == smoothingPeriod && cacheninZaDragonTrend[idx].EqualsInput(input))
						return cacheninZaDragonTrend[idx];
			return CacheIndicator<ninZaDragonTrend>(new ninZaDragonTrend(){ Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod }, input, ref cacheninZaDragonTrend);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaDragonTrend ninZaDragonTrend(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaDragonTrend(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}


		
		public Indicators.ninZaDragonTrend ninZaDragonTrend(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaDragonTrend(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaDragonTrend ninZaDragonTrend(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaDragonTrend(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}


		
		public Indicators.ninZaDragonTrend ninZaDragonTrend(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaDragonTrend(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}

	}
}

#endregion
