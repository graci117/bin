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
		
		private ninZaHelperMFI[] cacheninZaHelperMFI;

		
		public ninZaHelperMFI ninZaHelperMFI(int period)
		{
			return ninZaHelperMFI(Input, period);
		}


		
		public ninZaHelperMFI ninZaHelperMFI(ISeries<double> input, int period)
		{
			if (cacheninZaHelperMFI != null)
				for (int idx = 0; idx < cacheninZaHelperMFI.Length; idx++)
					if (cacheninZaHelperMFI[idx].Period == period && cacheninZaHelperMFI[idx].EqualsInput(input))
						return cacheninZaHelperMFI[idx];
			return CacheIndicator<ninZaHelperMFI>(new ninZaHelperMFI(){ Period = period }, input, ref cacheninZaHelperMFI);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHelperMFI ninZaHelperMFI(int period)
		{
			return indicator.ninZaHelperMFI(Input, period);
		}


		
		public Indicators.ninZaHelperMFI ninZaHelperMFI(ISeries<double> input , int period)
		{
			return indicator.ninZaHelperMFI(input, period);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHelperMFI ninZaHelperMFI(int period)
		{
			return indicator.ninZaHelperMFI(Input, period);
		}


		
		public Indicators.ninZaHelperMFI ninZaHelperMFI(ISeries<double> input , int period)
		{
			return indicator.ninZaHelperMFI(input, period);
		}

	}
}

#endregion
