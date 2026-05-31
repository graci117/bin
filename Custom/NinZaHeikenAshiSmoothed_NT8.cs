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
		
		private ninZaHeikenAshiSmoothed[] cacheninZaHeikenAshiSmoothed;

		
		public ninZaHeikenAshiSmoothed ninZaHeikenAshiSmoothed(bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int hAOpenWeight, bool hAFilterEnabled, double hAFilterMinimumBody)
		{
			return ninZaHeikenAshiSmoothed(Input, smoothingEnabled, smoothingMethod, smoothingPeriod, hAOpenWeight, hAFilterEnabled, hAFilterMinimumBody);
		}


		
		public ninZaHeikenAshiSmoothed ninZaHeikenAshiSmoothed(ISeries<double> input, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int hAOpenWeight, bool hAFilterEnabled, double hAFilterMinimumBody)
		{
			if (cacheninZaHeikenAshiSmoothed != null)
				for (int idx = 0; idx < cacheninZaHeikenAshiSmoothed.Length; idx++)
					if (cacheninZaHeikenAshiSmoothed[idx].SmoothingEnabled == smoothingEnabled && cacheninZaHeikenAshiSmoothed[idx].SmoothingMethod == smoothingMethod && cacheninZaHeikenAshiSmoothed[idx].SmoothingPeriod == smoothingPeriod && cacheninZaHeikenAshiSmoothed[idx].HAOpenWeight == hAOpenWeight && cacheninZaHeikenAshiSmoothed[idx].HAFilterEnabled == hAFilterEnabled && cacheninZaHeikenAshiSmoothed[idx].HAFilterMinimumBody == hAFilterMinimumBody && cacheninZaHeikenAshiSmoothed[idx].EqualsInput(input))
						return cacheninZaHeikenAshiSmoothed[idx];
			return CacheIndicator<ninZaHeikenAshiSmoothed>(new ninZaHeikenAshiSmoothed(){ SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, HAOpenWeight = hAOpenWeight, HAFilterEnabled = hAFilterEnabled, HAFilterMinimumBody = hAFilterMinimumBody }, input, ref cacheninZaHeikenAshiSmoothed);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHeikenAshiSmoothed ninZaHeikenAshiSmoothed(bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int hAOpenWeight, bool hAFilterEnabled, double hAFilterMinimumBody)
		{
			return indicator.ninZaHeikenAshiSmoothed(Input, smoothingEnabled, smoothingMethod, smoothingPeriod, hAOpenWeight, hAFilterEnabled, hAFilterMinimumBody);
		}


		
		public Indicators.ninZaHeikenAshiSmoothed ninZaHeikenAshiSmoothed(ISeries<double> input , bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int hAOpenWeight, bool hAFilterEnabled, double hAFilterMinimumBody)
		{
			return indicator.ninZaHeikenAshiSmoothed(input, smoothingEnabled, smoothingMethod, smoothingPeriod, hAOpenWeight, hAFilterEnabled, hAFilterMinimumBody);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHeikenAshiSmoothed ninZaHeikenAshiSmoothed(bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int hAOpenWeight, bool hAFilterEnabled, double hAFilterMinimumBody)
		{
			return indicator.ninZaHeikenAshiSmoothed(Input, smoothingEnabled, smoothingMethod, smoothingPeriod, hAOpenWeight, hAFilterEnabled, hAFilterMinimumBody);
		}


		
		public Indicators.ninZaHeikenAshiSmoothed ninZaHeikenAshiSmoothed(ISeries<double> input , bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int hAOpenWeight, bool hAFilterEnabled, double hAFilterMinimumBody)
		{
			return indicator.ninZaHeikenAshiSmoothed(input, smoothingEnabled, smoothingMethod, smoothingPeriod, hAOpenWeight, hAFilterEnabled, hAFilterMinimumBody);
		}

	}
}

#endregion
