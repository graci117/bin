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
		
		private ninZaAAATrendSync[] cacheninZaAAATrendSync;

		
		public ninZaAAATrendSync ninZaAAATrendSync(ninZa_MAType fastType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType midType, int midPeriod, bool midSmoothingEnabled, ninZa_MAType midSmoothingMethod, int midSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, bool minimumFastMidSpreadEnabled, double minimumFastMidSpreadATRMultiplier, int minimumFastMidSpreadATRPeriod, bool syncSlope, bool roundToTickSize)
		{
			return ninZaAAATrendSync(Input, fastType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, midType, midPeriod, midSmoothingEnabled, midSmoothingMethod, midSmoothingPeriod, slowType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, minimumFastMidSpreadEnabled, minimumFastMidSpreadATRMultiplier, minimumFastMidSpreadATRPeriod, syncSlope, roundToTickSize);
		}


		
		public ninZaAAATrendSync ninZaAAATrendSync(ISeries<double> input, ninZa_MAType fastType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType midType, int midPeriod, bool midSmoothingEnabled, ninZa_MAType midSmoothingMethod, int midSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, bool minimumFastMidSpreadEnabled, double minimumFastMidSpreadATRMultiplier, int minimumFastMidSpreadATRPeriod, bool syncSlope, bool roundToTickSize)
		{
			if (cacheninZaAAATrendSync != null)
				for (int idx = 0; idx < cacheninZaAAATrendSync.Length; idx++)
					if (cacheninZaAAATrendSync[idx].FastType == fastType && cacheninZaAAATrendSync[idx].FastPeriod == fastPeriod && cacheninZaAAATrendSync[idx].FastSmoothingEnabled == fastSmoothingEnabled && cacheninZaAAATrendSync[idx].FastSmoothingMethod == fastSmoothingMethod && cacheninZaAAATrendSync[idx].FastSmoothingPeriod == fastSmoothingPeriod && cacheninZaAAATrendSync[idx].MidType == midType && cacheninZaAAATrendSync[idx].MidPeriod == midPeriod && cacheninZaAAATrendSync[idx].MidSmoothingEnabled == midSmoothingEnabled && cacheninZaAAATrendSync[idx].MidSmoothingMethod == midSmoothingMethod && cacheninZaAAATrendSync[idx].MidSmoothingPeriod == midSmoothingPeriod && cacheninZaAAATrendSync[idx].SlowType == slowType && cacheninZaAAATrendSync[idx].SlowPeriod == slowPeriod && cacheninZaAAATrendSync[idx].SlowSmoothingEnabled == slowSmoothingEnabled && cacheninZaAAATrendSync[idx].SlowSmoothingMethod == slowSmoothingMethod && cacheninZaAAATrendSync[idx].SlowSmoothingPeriod == slowSmoothingPeriod && cacheninZaAAATrendSync[idx].MinimumFastMidSpreadEnabled == minimumFastMidSpreadEnabled && cacheninZaAAATrendSync[idx].MinimumFastMidSpreadATRMultiplier == minimumFastMidSpreadATRMultiplier && cacheninZaAAATrendSync[idx].MinimumFastMidSpreadATRPeriod == minimumFastMidSpreadATRPeriod && cacheninZaAAATrendSync[idx].SyncSlope == syncSlope && cacheninZaAAATrendSync[idx].RoundToTickSize == roundToTickSize && cacheninZaAAATrendSync[idx].EqualsInput(input))
						return cacheninZaAAATrendSync[idx];
			return CacheIndicator<ninZaAAATrendSync>(new ninZaAAATrendSync(){ FastType = fastType, FastPeriod = fastPeriod, FastSmoothingEnabled = fastSmoothingEnabled, FastSmoothingMethod = fastSmoothingMethod, FastSmoothingPeriod = fastSmoothingPeriod, MidType = midType, MidPeriod = midPeriod, MidSmoothingEnabled = midSmoothingEnabled, MidSmoothingMethod = midSmoothingMethod, MidSmoothingPeriod = midSmoothingPeriod, SlowType = slowType, SlowPeriod = slowPeriod, SlowSmoothingEnabled = slowSmoothingEnabled, SlowSmoothingMethod = slowSmoothingMethod, SlowSmoothingPeriod = slowSmoothingPeriod, MinimumFastMidSpreadEnabled = minimumFastMidSpreadEnabled, MinimumFastMidSpreadATRMultiplier = minimumFastMidSpreadATRMultiplier, MinimumFastMidSpreadATRPeriod = minimumFastMidSpreadATRPeriod, SyncSlope = syncSlope, RoundToTickSize = roundToTickSize }, input, ref cacheninZaAAATrendSync);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaAAATrendSync ninZaAAATrendSync(ninZa_MAType fastType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType midType, int midPeriod, bool midSmoothingEnabled, ninZa_MAType midSmoothingMethod, int midSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, bool minimumFastMidSpreadEnabled, double minimumFastMidSpreadATRMultiplier, int minimumFastMidSpreadATRPeriod, bool syncSlope, bool roundToTickSize)
		{
			return indicator.ninZaAAATrendSync(Input, fastType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, midType, midPeriod, midSmoothingEnabled, midSmoothingMethod, midSmoothingPeriod, slowType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, minimumFastMidSpreadEnabled, minimumFastMidSpreadATRMultiplier, minimumFastMidSpreadATRPeriod, syncSlope, roundToTickSize);
		}


		
		public Indicators.ninZaAAATrendSync ninZaAAATrendSync(ISeries<double> input , ninZa_MAType fastType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType midType, int midPeriod, bool midSmoothingEnabled, ninZa_MAType midSmoothingMethod, int midSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, bool minimumFastMidSpreadEnabled, double minimumFastMidSpreadATRMultiplier, int minimumFastMidSpreadATRPeriod, bool syncSlope, bool roundToTickSize)
		{
			return indicator.ninZaAAATrendSync(input, fastType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, midType, midPeriod, midSmoothingEnabled, midSmoothingMethod, midSmoothingPeriod, slowType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, minimumFastMidSpreadEnabled, minimumFastMidSpreadATRMultiplier, minimumFastMidSpreadATRPeriod, syncSlope, roundToTickSize);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaAAATrendSync ninZaAAATrendSync(ninZa_MAType fastType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType midType, int midPeriod, bool midSmoothingEnabled, ninZa_MAType midSmoothingMethod, int midSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, bool minimumFastMidSpreadEnabled, double minimumFastMidSpreadATRMultiplier, int minimumFastMidSpreadATRPeriod, bool syncSlope, bool roundToTickSize)
		{
			return indicator.ninZaAAATrendSync(Input, fastType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, midType, midPeriod, midSmoothingEnabled, midSmoothingMethod, midSmoothingPeriod, slowType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, minimumFastMidSpreadEnabled, minimumFastMidSpreadATRMultiplier, minimumFastMidSpreadATRPeriod, syncSlope, roundToTickSize);
		}


		
		public Indicators.ninZaAAATrendSync ninZaAAATrendSync(ISeries<double> input , ninZa_MAType fastType, int fastPeriod, bool fastSmoothingEnabled, ninZa_MAType fastSmoothingMethod, int fastSmoothingPeriod, ninZa_MAType midType, int midPeriod, bool midSmoothingEnabled, ninZa_MAType midSmoothingMethod, int midSmoothingPeriod, ninZa_MAType slowType, int slowPeriod, bool slowSmoothingEnabled, ninZa_MAType slowSmoothingMethod, int slowSmoothingPeriod, bool minimumFastMidSpreadEnabled, double minimumFastMidSpreadATRMultiplier, int minimumFastMidSpreadATRPeriod, bool syncSlope, bool roundToTickSize)
		{
			return indicator.ninZaAAATrendSync(input, fastType, fastPeriod, fastSmoothingEnabled, fastSmoothingMethod, fastSmoothingPeriod, midType, midPeriod, midSmoothingEnabled, midSmoothingMethod, midSmoothingPeriod, slowType, slowPeriod, slowSmoothingEnabled, slowSmoothingMethod, slowSmoothingPeriod, minimumFastMidSpreadEnabled, minimumFastMidSpreadATRMultiplier, minimumFastMidSpreadATRPeriod, syncSlope, roundToTickSize);
		}

	}
}

#endregion
