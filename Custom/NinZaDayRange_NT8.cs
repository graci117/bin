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
		
		private ninZaDayRange[] cacheninZaDayRange;

		
		public ninZaDayRange ninZaDayRange(ninZaDayRange_DayStartDefinition dayStartDefinition, int dayStartCustomTime, int lookback1, int lookback2, int lookback3, int lookback4, int lookback5, ninZaDayRange_LookbackReference lookbackReference)
		{
			return ninZaDayRange(Input, dayStartDefinition, dayStartCustomTime, lookback1, lookback2, lookback3, lookback4, lookback5, lookbackReference);
		}


		
		public ninZaDayRange ninZaDayRange(ISeries<double> input, ninZaDayRange_DayStartDefinition dayStartDefinition, int dayStartCustomTime, int lookback1, int lookback2, int lookback3, int lookback4, int lookback5, ninZaDayRange_LookbackReference lookbackReference)
		{
			if (cacheninZaDayRange != null)
				for (int idx = 0; idx < cacheninZaDayRange.Length; idx++)
					if (cacheninZaDayRange[idx].DayStartDefinition == dayStartDefinition && cacheninZaDayRange[idx].DayStartCustomTime == dayStartCustomTime && cacheninZaDayRange[idx].Lookback1 == lookback1 && cacheninZaDayRange[idx].Lookback2 == lookback2 && cacheninZaDayRange[idx].Lookback3 == lookback3 && cacheninZaDayRange[idx].Lookback4 == lookback4 && cacheninZaDayRange[idx].Lookback5 == lookback5 && cacheninZaDayRange[idx].LookbackReference == lookbackReference && cacheninZaDayRange[idx].EqualsInput(input))
						return cacheninZaDayRange[idx];
			return CacheIndicator<ninZaDayRange>(new ninZaDayRange(){ DayStartDefinition = dayStartDefinition, DayStartCustomTime = dayStartCustomTime, Lookback1 = lookback1, Lookback2 = lookback2, Lookback3 = lookback3, Lookback4 = lookback4, Lookback5 = lookback5, LookbackReference = lookbackReference }, input, ref cacheninZaDayRange);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaDayRange ninZaDayRange(ninZaDayRange_DayStartDefinition dayStartDefinition, int dayStartCustomTime, int lookback1, int lookback2, int lookback3, int lookback4, int lookback5, ninZaDayRange_LookbackReference lookbackReference)
		{
			return indicator.ninZaDayRange(Input, dayStartDefinition, dayStartCustomTime, lookback1, lookback2, lookback3, lookback4, lookback5, lookbackReference);
		}


		
		public Indicators.ninZaDayRange ninZaDayRange(ISeries<double> input , ninZaDayRange_DayStartDefinition dayStartDefinition, int dayStartCustomTime, int lookback1, int lookback2, int lookback3, int lookback4, int lookback5, ninZaDayRange_LookbackReference lookbackReference)
		{
			return indicator.ninZaDayRange(input, dayStartDefinition, dayStartCustomTime, lookback1, lookback2, lookback3, lookback4, lookback5, lookbackReference);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaDayRange ninZaDayRange(ninZaDayRange_DayStartDefinition dayStartDefinition, int dayStartCustomTime, int lookback1, int lookback2, int lookback3, int lookback4, int lookback5, ninZaDayRange_LookbackReference lookbackReference)
		{
			return indicator.ninZaDayRange(Input, dayStartDefinition, dayStartCustomTime, lookback1, lookback2, lookback3, lookback4, lookback5, lookbackReference);
		}


		
		public Indicators.ninZaDayRange ninZaDayRange(ISeries<double> input , ninZaDayRange_DayStartDefinition dayStartDefinition, int dayStartCustomTime, int lookback1, int lookback2, int lookback3, int lookback4, int lookback5, ninZaDayRange_LookbackReference lookbackReference)
		{
			return indicator.ninZaDayRange(input, dayStartDefinition, dayStartCustomTime, lookback1, lookback2, lookback3, lookback4, lookback5, lookbackReference);
		}

	}
}

#endregion
