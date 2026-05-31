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
		
		private ninZaSineWMAPro[] cacheninZaSineWMAPro;

		
		public ninZaSineWMAPro ninZaSineWMAPro(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return ninZaSineWMAPro(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public ninZaSineWMAPro ninZaSineWMAPro(ISeries<double> input, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaSineWMAPro != null)
				for (int idx = 0; idx < cacheninZaSineWMAPro.Length; idx++)
					if (cacheninZaSineWMAPro[idx].Period == period && cacheninZaSineWMAPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaSineWMAPro[idx].SmoothingMethod == smoothingMethod && cacheninZaSineWMAPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaSineWMAPro[idx].FilterEnabled == filterEnabled && cacheninZaSineWMAPro[idx].FilterMultiplier == filterMultiplier && cacheninZaSineWMAPro[idx].EqualsInput(input))
						return cacheninZaSineWMAPro[idx];
			return CacheIndicator<ninZaSineWMAPro>(new ninZaSineWMAPro(){ Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaSineWMAPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSineWMAPro ninZaSineWMAPro(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaSineWMAPro(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaSineWMAPro ninZaSineWMAPro(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaSineWMAPro(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSineWMAPro ninZaSineWMAPro(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaSineWMAPro(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaSineWMAPro ninZaSineWMAPro(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaSineWMAPro(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
