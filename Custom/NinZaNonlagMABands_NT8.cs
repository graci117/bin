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
		
		private ninZaNonlagMABands[] cacheninZaNonlagMABands;

		
		public ninZaNonlagMABands ninZaNonlagMABands(int period, int cycle, int coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, int offsetATRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return ninZaNonlagMABands(Input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetATRPeriod, filterEnabled, filterMultiplier);
		}


		
		public ninZaNonlagMABands ninZaNonlagMABands(ISeries<double> input, int period, int cycle, int coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, int offsetATRPeriod, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaNonlagMABands != null)
				for (int idx = 0; idx < cacheninZaNonlagMABands.Length; idx++)
					if (cacheninZaNonlagMABands[idx].Period == period && cacheninZaNonlagMABands[idx].Cycle == cycle && cacheninZaNonlagMABands[idx].Coefficient == coefficient && cacheninZaNonlagMABands[idx].SmoothingEnabled == smoothingEnabled && cacheninZaNonlagMABands[idx].SmoothingMethod == smoothingMethod && cacheninZaNonlagMABands[idx].SmoothingPeriod == smoothingPeriod && cacheninZaNonlagMABands[idx].OffsetMultiplier == offsetMultiplier && cacheninZaNonlagMABands[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaNonlagMABands[idx].FilterEnabled == filterEnabled && cacheninZaNonlagMABands[idx].FilterMultiplier == filterMultiplier && cacheninZaNonlagMABands[idx].EqualsInput(input))
						return cacheninZaNonlagMABands[idx];
			return CacheIndicator<ninZaNonlagMABands>(new ninZaNonlagMABands(){ Period = period, Cycle = cycle, Coefficient = coefficient, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, OffsetMultiplier = offsetMultiplier, OffsetATRPeriod = offsetATRPeriod, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaNonlagMABands);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaNonlagMABands ninZaNonlagMABands(int period, int cycle, int coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, int offsetATRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaNonlagMABands(Input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetATRPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaNonlagMABands ninZaNonlagMABands(ISeries<double> input , int period, int cycle, int coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, int offsetATRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaNonlagMABands(input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetATRPeriod, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaNonlagMABands ninZaNonlagMABands(int period, int cycle, int coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, int offsetATRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaNonlagMABands(Input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetATRPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaNonlagMABands ninZaNonlagMABands(ISeries<double> input , int period, int cycle, int coefficient, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, int offsetATRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaNonlagMABands(input, period, cycle, coefficient, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetATRPeriod, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
