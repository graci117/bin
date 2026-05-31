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
		
		private ninZaMarketExtremes[] cacheninZaMarketExtremes;

		
		public ninZaMarketExtremes ninZaMarketExtremes(ninZaMarketExtremes_LookbackType lookbackType, int lookbackValue, int extremeNeighborhoodLeft, int extremeNeighborhoodRight, int qualifyingLevelAge)
		{
			return ninZaMarketExtremes(Input, lookbackType, lookbackValue, extremeNeighborhoodLeft, extremeNeighborhoodRight, qualifyingLevelAge);
		}


		
		public ninZaMarketExtremes ninZaMarketExtremes(ISeries<double> input, ninZaMarketExtremes_LookbackType lookbackType, int lookbackValue, int extremeNeighborhoodLeft, int extremeNeighborhoodRight, int qualifyingLevelAge)
		{
			if (cacheninZaMarketExtremes != null)
				for (int idx = 0; idx < cacheninZaMarketExtremes.Length; idx++)
					if (cacheninZaMarketExtremes[idx].LookbackType == lookbackType && cacheninZaMarketExtremes[idx].LookbackValue == lookbackValue && cacheninZaMarketExtremes[idx].ExtremeNeighborhoodLeft == extremeNeighborhoodLeft && cacheninZaMarketExtremes[idx].ExtremeNeighborhoodRight == extremeNeighborhoodRight && cacheninZaMarketExtremes[idx].QualifyingLevelAge == qualifyingLevelAge && cacheninZaMarketExtremes[idx].EqualsInput(input))
						return cacheninZaMarketExtremes[idx];
			return CacheIndicator<ninZaMarketExtremes>(new ninZaMarketExtremes(){ LookbackType = lookbackType, LookbackValue = lookbackValue, ExtremeNeighborhoodLeft = extremeNeighborhoodLeft, ExtremeNeighborhoodRight = extremeNeighborhoodRight, QualifyingLevelAge = qualifyingLevelAge }, input, ref cacheninZaMarketExtremes);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMarketExtremes ninZaMarketExtremes(ninZaMarketExtremes_LookbackType lookbackType, int lookbackValue, int extremeNeighborhoodLeft, int extremeNeighborhoodRight, int qualifyingLevelAge)
		{
			return indicator.ninZaMarketExtremes(Input, lookbackType, lookbackValue, extremeNeighborhoodLeft, extremeNeighborhoodRight, qualifyingLevelAge);
		}


		
		public Indicators.ninZaMarketExtremes ninZaMarketExtremes(ISeries<double> input , ninZaMarketExtremes_LookbackType lookbackType, int lookbackValue, int extremeNeighborhoodLeft, int extremeNeighborhoodRight, int qualifyingLevelAge)
		{
			return indicator.ninZaMarketExtremes(input, lookbackType, lookbackValue, extremeNeighborhoodLeft, extremeNeighborhoodRight, qualifyingLevelAge);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMarketExtremes ninZaMarketExtremes(ninZaMarketExtremes_LookbackType lookbackType, int lookbackValue, int extremeNeighborhoodLeft, int extremeNeighborhoodRight, int qualifyingLevelAge)
		{
			return indicator.ninZaMarketExtremes(Input, lookbackType, lookbackValue, extremeNeighborhoodLeft, extremeNeighborhoodRight, qualifyingLevelAge);
		}


		
		public Indicators.ninZaMarketExtremes ninZaMarketExtremes(ISeries<double> input , ninZaMarketExtremes_LookbackType lookbackType, int lookbackValue, int extremeNeighborhoodLeft, int extremeNeighborhoodRight, int qualifyingLevelAge)
		{
			return indicator.ninZaMarketExtremes(input, lookbackType, lookbackValue, extremeNeighborhoodLeft, extremeNeighborhoodRight, qualifyingLevelAge);
		}

	}
}

#endregion
