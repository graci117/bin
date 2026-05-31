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
		
		private ninZaRegularizedEMAPro[] cacheninZaRegularizedEMAPro;

		
		public ninZaRegularizedEMAPro ninZaRegularizedEMAPro(int period, double lambda, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return ninZaRegularizedEMAPro(Input, period, lambda, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public ninZaRegularizedEMAPro ninZaRegularizedEMAPro(ISeries<double> input, int period, double lambda, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaRegularizedEMAPro != null)
				for (int idx = 0; idx < cacheninZaRegularizedEMAPro.Length; idx++)
					if (cacheninZaRegularizedEMAPro[idx].Period == period && cacheninZaRegularizedEMAPro[idx].Lambda == lambda && cacheninZaRegularizedEMAPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaRegularizedEMAPro[idx].SmoothingMethod == smoothingMethod && cacheninZaRegularizedEMAPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaRegularizedEMAPro[idx].FilterEnabled == filterEnabled && cacheninZaRegularizedEMAPro[idx].FilterMultiplier == filterMultiplier && cacheninZaRegularizedEMAPro[idx].EqualsInput(input))
						return cacheninZaRegularizedEMAPro[idx];
			return CacheIndicator<ninZaRegularizedEMAPro>(new ninZaRegularizedEMAPro(){ Period = period, Lambda = lambda, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaRegularizedEMAPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaRegularizedEMAPro ninZaRegularizedEMAPro(int period, double lambda, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaRegularizedEMAPro(Input, period, lambda, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaRegularizedEMAPro ninZaRegularizedEMAPro(ISeries<double> input , int period, double lambda, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaRegularizedEMAPro(input, period, lambda, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaRegularizedEMAPro ninZaRegularizedEMAPro(int period, double lambda, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaRegularizedEMAPro(Input, period, lambda, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaRegularizedEMAPro ninZaRegularizedEMAPro(ISeries<double> input , int period, double lambda, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaRegularizedEMAPro(input, period, lambda, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
