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
		
		private ninZaRenkoStyleTrend[] cacheninZaRenkoStyleTrend;

		
		public ninZaRenkoStyleTrend ninZaRenkoStyleTrend(double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD)
		{
			return ninZaRenkoStyleTrend(Input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD);
		}


		
		public ninZaRenkoStyleTrend ninZaRenkoStyleTrend(ISeries<double> input, double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD)
		{
			if (cacheninZaRenkoStyleTrend != null)
				for (int idx = 0; idx < cacheninZaRenkoStyleTrend.Length; idx++)
					if (cacheninZaRenkoStyleTrend[idx].OffsetMultiplierTrend == offsetMultiplierTrend && cacheninZaRenkoStyleTrend[idx].OffsetMultiplierReversal == offsetMultiplierReversal && cacheninZaRenkoStyleTrend[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaRenkoStyleTrend[idx].BreakAtEOD == breakAtEOD && cacheninZaRenkoStyleTrend[idx].EqualsInput(input))
						return cacheninZaRenkoStyleTrend[idx];
			return CacheIndicator<ninZaRenkoStyleTrend>(new ninZaRenkoStyleTrend(){ OffsetMultiplierTrend = offsetMultiplierTrend, OffsetMultiplierReversal = offsetMultiplierReversal, OffsetATRPeriod = offsetATRPeriod, BreakAtEOD = breakAtEOD }, input, ref cacheninZaRenkoStyleTrend);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaRenkoStyleTrend ninZaRenkoStyleTrend(double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD)
		{
			return indicator.ninZaRenkoStyleTrend(Input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD);
		}


		
		public Indicators.ninZaRenkoStyleTrend ninZaRenkoStyleTrend(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD)
		{
			return indicator.ninZaRenkoStyleTrend(input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaRenkoStyleTrend ninZaRenkoStyleTrend(double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD)
		{
			return indicator.ninZaRenkoStyleTrend(Input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD);
		}


		
		public Indicators.ninZaRenkoStyleTrend ninZaRenkoStyleTrend(ISeries<double> input , double offsetMultiplierTrend, double offsetMultiplierReversal, int offsetATRPeriod, bool breakAtEOD)
		{
			return indicator.ninZaRenkoStyleTrend(input, offsetMultiplierTrend, offsetMultiplierReversal, offsetATRPeriod, breakAtEOD);
		}

	}
}

#endregion
