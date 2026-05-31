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
		
		private ninZaVIDYAPro[] cacheninZaVIDYAPro;

		
		public ninZaVIDYAPro ninZaVIDYAPro(int period, int volatilityPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return ninZaVIDYAPro(Input, period, volatilityPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public ninZaVIDYAPro ninZaVIDYAPro(ISeries<double> input, int period, int volatilityPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaVIDYAPro != null)
				for (int idx = 0; idx < cacheninZaVIDYAPro.Length; idx++)
					if (cacheninZaVIDYAPro[idx].Period == period && cacheninZaVIDYAPro[idx].VolatilityPeriod == volatilityPeriod && cacheninZaVIDYAPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaVIDYAPro[idx].SmoothingMethod == smoothingMethod && cacheninZaVIDYAPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaVIDYAPro[idx].FilterEnabled == filterEnabled && cacheninZaVIDYAPro[idx].FilterMultiplier == filterMultiplier && cacheninZaVIDYAPro[idx].EqualsInput(input))
						return cacheninZaVIDYAPro[idx];
			return CacheIndicator<ninZaVIDYAPro>(new ninZaVIDYAPro(){ Period = period, VolatilityPeriod = volatilityPeriod, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaVIDYAPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaVIDYAPro ninZaVIDYAPro(int period, int volatilityPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaVIDYAPro(Input, period, volatilityPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaVIDYAPro ninZaVIDYAPro(ISeries<double> input , int period, int volatilityPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaVIDYAPro(input, period, volatilityPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaVIDYAPro ninZaVIDYAPro(int period, int volatilityPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaVIDYAPro(Input, period, volatilityPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaVIDYAPro ninZaVIDYAPro(ISeries<double> input , int period, int volatilityPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaVIDYAPro(input, period, volatilityPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
