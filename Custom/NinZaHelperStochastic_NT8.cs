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
		
		private ninZaHelperStochastic[] cacheninZaHelperStochastic;

		
		public ninZaHelperStochastic ninZaHelperStochastic(int periodD, int periodK, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return ninZaHelperStochastic(Input, periodD, periodK, smoothingMethod, smoothingPeriod);
		}


		
		public ninZaHelperStochastic ninZaHelperStochastic(ISeries<double> input, int periodD, int periodK, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			if (cacheninZaHelperStochastic != null)
				for (int idx = 0; idx < cacheninZaHelperStochastic.Length; idx++)
					if (cacheninZaHelperStochastic[idx].PeriodD == periodD && cacheninZaHelperStochastic[idx].PeriodK == periodK && cacheninZaHelperStochastic[idx].SmoothingMethod == smoothingMethod && cacheninZaHelperStochastic[idx].SmoothingPeriod == smoothingPeriod && cacheninZaHelperStochastic[idx].EqualsInput(input))
						return cacheninZaHelperStochastic[idx];
			return CacheIndicator<ninZaHelperStochastic>(new ninZaHelperStochastic(){ PeriodD = periodD, PeriodK = periodK, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod }, input, ref cacheninZaHelperStochastic);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHelperStochastic ninZaHelperStochastic(int periodD, int periodK, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaHelperStochastic(Input, periodD, periodK, smoothingMethod, smoothingPeriod);
		}


		
		public Indicators.ninZaHelperStochastic ninZaHelperStochastic(ISeries<double> input , int periodD, int periodK, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaHelperStochastic(input, periodD, periodK, smoothingMethod, smoothingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHelperStochastic ninZaHelperStochastic(int periodD, int periodK, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaHelperStochastic(Input, periodD, periodK, smoothingMethod, smoothingPeriod);
		}


		
		public Indicators.ninZaHelperStochastic ninZaHelperStochastic(ISeries<double> input , int periodD, int periodK, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaHelperStochastic(input, periodD, periodK, smoothingMethod, smoothingPeriod);
		}

	}
}

#endregion
