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
		
		private ninZaNonlagMAPro[] cacheninZaNonlagMAPro;

		
		public ninZaNonlagMAPro ninZaNonlagMAPro(int period, int cycle, double coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaNonlagMAPro_FilterUnit filterUnit, int filterATRPeriod)
		{
			return ninZaNonlagMAPro(Input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}


		
		public ninZaNonlagMAPro ninZaNonlagMAPro(ISeries<double> input, int period, int cycle, double coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaNonlagMAPro_FilterUnit filterUnit, int filterATRPeriod)
		{
			if (cacheninZaNonlagMAPro != null)
				for (int idx = 0; idx < cacheninZaNonlagMAPro.Length; idx++)
					if (cacheninZaNonlagMAPro[idx].Period == period && cacheninZaNonlagMAPro[idx].Cycle == cycle && cacheninZaNonlagMAPro[idx].Coefficient == coefficient && cacheninZaNonlagMAPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaNonlagMAPro[idx].SmoothingMethod == smoothingMethod && cacheninZaNonlagMAPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaNonlagMAPro[idx].FilterEnabled == filterEnabled && cacheninZaNonlagMAPro[idx].FilterAfterSmoothing == filterAfterSmoothing && cacheninZaNonlagMAPro[idx].FilterMultiplier == filterMultiplier && cacheninZaNonlagMAPro[idx].FilterUnit == filterUnit && cacheninZaNonlagMAPro[idx].FilterATRPeriod == filterATRPeriod && cacheninZaNonlagMAPro[idx].EqualsInput(input))
						return cacheninZaNonlagMAPro[idx];
			return CacheIndicator<ninZaNonlagMAPro>(new ninZaNonlagMAPro(){ Period = period, Cycle = cycle, Coefficient = coefficient, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, FilterEnabled = filterEnabled, FilterAfterSmoothing = filterAfterSmoothing, FilterMultiplier = filterMultiplier, FilterUnit = filterUnit, FilterATRPeriod = filterATRPeriod }, input, ref cacheninZaNonlagMAPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaNonlagMAPro ninZaNonlagMAPro(int period, int cycle, double coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaNonlagMAPro_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaNonlagMAPro(Input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}


		
		public Indicators.ninZaNonlagMAPro ninZaNonlagMAPro(ISeries<double> input , int period, int cycle, double coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaNonlagMAPro_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaNonlagMAPro(input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaNonlagMAPro ninZaNonlagMAPro(int period, int cycle, double coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaNonlagMAPro_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaNonlagMAPro(Input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}


		
		public Indicators.ninZaNonlagMAPro ninZaNonlagMAPro(ISeries<double> input , int period, int cycle, double coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, bool filterEnabled, bool filterAfterSmoothing, double filterMultiplier, ninZaNonlagMAPro_FilterUnit filterUnit, int filterATRPeriod)
		{
			return indicator.ninZaNonlagMAPro(input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, filterEnabled, filterAfterSmoothing, filterMultiplier, filterUnit, filterATRPeriod);
		}

	}
}

#endregion
