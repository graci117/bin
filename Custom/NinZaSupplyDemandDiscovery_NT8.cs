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
		
		private ninZaSupplyDemandDiscovery[] cacheninZaSupplyDemandDiscovery;

		
		public ninZaSupplyDemandDiscovery ninZaSupplyDemandDiscovery(ninZaSupplyDemandDiscovery_LookbackType lookbackType, int lookbackValue, int aTRPeriod, double multiplierQualifyingMove, double multiplierQualifyingFirstBar, int intervalLimit, ninZaSupplyDemandDiscovery_PriceNearMode priceNearMode, bool priceFarOneBarExtensionEnabled, int priceFarOneBarExtensionMax, bool zoneHeightLimitEnabled, int zoneHeightLimitMax, int zoneHeightLimitMin, int moveSplit, int minimumDistanceBetweenZones)
		{
			return ninZaSupplyDemandDiscovery(Input, lookbackType, lookbackValue, aTRPeriod, multiplierQualifyingMove, multiplierQualifyingFirstBar, intervalLimit, priceNearMode, priceFarOneBarExtensionEnabled, priceFarOneBarExtensionMax, zoneHeightLimitEnabled, zoneHeightLimitMax, zoneHeightLimitMin, moveSplit, minimumDistanceBetweenZones);
		}


		
		public ninZaSupplyDemandDiscovery ninZaSupplyDemandDiscovery(ISeries<double> input, ninZaSupplyDemandDiscovery_LookbackType lookbackType, int lookbackValue, int aTRPeriod, double multiplierQualifyingMove, double multiplierQualifyingFirstBar, int intervalLimit, ninZaSupplyDemandDiscovery_PriceNearMode priceNearMode, bool priceFarOneBarExtensionEnabled, int priceFarOneBarExtensionMax, bool zoneHeightLimitEnabled, int zoneHeightLimitMax, int zoneHeightLimitMin, int moveSplit, int minimumDistanceBetweenZones)
		{
			if (cacheninZaSupplyDemandDiscovery != null)
				for (int idx = 0; idx < cacheninZaSupplyDemandDiscovery.Length; idx++)
					if (cacheninZaSupplyDemandDiscovery[idx].LookbackType == lookbackType && cacheninZaSupplyDemandDiscovery[idx].LookbackValue == lookbackValue && cacheninZaSupplyDemandDiscovery[idx].ATRPeriod == aTRPeriod && cacheninZaSupplyDemandDiscovery[idx].MultiplierQualifyingMove == multiplierQualifyingMove && cacheninZaSupplyDemandDiscovery[idx].MultiplierQualifyingFirstBar == multiplierQualifyingFirstBar && cacheninZaSupplyDemandDiscovery[idx].IntervalLimit == intervalLimit && cacheninZaSupplyDemandDiscovery[idx].PriceNearMode == priceNearMode && cacheninZaSupplyDemandDiscovery[idx].PriceFarOneBarExtensionEnabled == priceFarOneBarExtensionEnabled && cacheninZaSupplyDemandDiscovery[idx].PriceFarOneBarExtensionMax == priceFarOneBarExtensionMax && cacheninZaSupplyDemandDiscovery[idx].ZoneHeightLimitEnabled == zoneHeightLimitEnabled && cacheninZaSupplyDemandDiscovery[idx].ZoneHeightLimitMax == zoneHeightLimitMax && cacheninZaSupplyDemandDiscovery[idx].ZoneHeightLimitMin == zoneHeightLimitMin && cacheninZaSupplyDemandDiscovery[idx].MoveSplit == moveSplit && cacheninZaSupplyDemandDiscovery[idx].MinimumDistanceBetweenZones == minimumDistanceBetweenZones && cacheninZaSupplyDemandDiscovery[idx].EqualsInput(input))
						return cacheninZaSupplyDemandDiscovery[idx];
			return CacheIndicator<ninZaSupplyDemandDiscovery>(new ninZaSupplyDemandDiscovery(){ LookbackType = lookbackType, LookbackValue = lookbackValue, ATRPeriod = aTRPeriod, MultiplierQualifyingMove = multiplierQualifyingMove, MultiplierQualifyingFirstBar = multiplierQualifyingFirstBar, IntervalLimit = intervalLimit, PriceNearMode = priceNearMode, PriceFarOneBarExtensionEnabled = priceFarOneBarExtensionEnabled, PriceFarOneBarExtensionMax = priceFarOneBarExtensionMax, ZoneHeightLimitEnabled = zoneHeightLimitEnabled, ZoneHeightLimitMax = zoneHeightLimitMax, ZoneHeightLimitMin = zoneHeightLimitMin, MoveSplit = moveSplit, MinimumDistanceBetweenZones = minimumDistanceBetweenZones }, input, ref cacheninZaSupplyDemandDiscovery);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSupplyDemandDiscovery ninZaSupplyDemandDiscovery(ninZaSupplyDemandDiscovery_LookbackType lookbackType, int lookbackValue, int aTRPeriod, double multiplierQualifyingMove, double multiplierQualifyingFirstBar, int intervalLimit, ninZaSupplyDemandDiscovery_PriceNearMode priceNearMode, bool priceFarOneBarExtensionEnabled, int priceFarOneBarExtensionMax, bool zoneHeightLimitEnabled, int zoneHeightLimitMax, int zoneHeightLimitMin, int moveSplit, int minimumDistanceBetweenZones)
		{
			return indicator.ninZaSupplyDemandDiscovery(Input, lookbackType, lookbackValue, aTRPeriod, multiplierQualifyingMove, multiplierQualifyingFirstBar, intervalLimit, priceNearMode, priceFarOneBarExtensionEnabled, priceFarOneBarExtensionMax, zoneHeightLimitEnabled, zoneHeightLimitMax, zoneHeightLimitMin, moveSplit, minimumDistanceBetweenZones);
		}


		
		public Indicators.ninZaSupplyDemandDiscovery ninZaSupplyDemandDiscovery(ISeries<double> input , ninZaSupplyDemandDiscovery_LookbackType lookbackType, int lookbackValue, int aTRPeriod, double multiplierQualifyingMove, double multiplierQualifyingFirstBar, int intervalLimit, ninZaSupplyDemandDiscovery_PriceNearMode priceNearMode, bool priceFarOneBarExtensionEnabled, int priceFarOneBarExtensionMax, bool zoneHeightLimitEnabled, int zoneHeightLimitMax, int zoneHeightLimitMin, int moveSplit, int minimumDistanceBetweenZones)
		{
			return indicator.ninZaSupplyDemandDiscovery(input, lookbackType, lookbackValue, aTRPeriod, multiplierQualifyingMove, multiplierQualifyingFirstBar, intervalLimit, priceNearMode, priceFarOneBarExtensionEnabled, priceFarOneBarExtensionMax, zoneHeightLimitEnabled, zoneHeightLimitMax, zoneHeightLimitMin, moveSplit, minimumDistanceBetweenZones);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSupplyDemandDiscovery ninZaSupplyDemandDiscovery(ninZaSupplyDemandDiscovery_LookbackType lookbackType, int lookbackValue, int aTRPeriod, double multiplierQualifyingMove, double multiplierQualifyingFirstBar, int intervalLimit, ninZaSupplyDemandDiscovery_PriceNearMode priceNearMode, bool priceFarOneBarExtensionEnabled, int priceFarOneBarExtensionMax, bool zoneHeightLimitEnabled, int zoneHeightLimitMax, int zoneHeightLimitMin, int moveSplit, int minimumDistanceBetweenZones)
		{
			return indicator.ninZaSupplyDemandDiscovery(Input, lookbackType, lookbackValue, aTRPeriod, multiplierQualifyingMove, multiplierQualifyingFirstBar, intervalLimit, priceNearMode, priceFarOneBarExtensionEnabled, priceFarOneBarExtensionMax, zoneHeightLimitEnabled, zoneHeightLimitMax, zoneHeightLimitMin, moveSplit, minimumDistanceBetweenZones);
		}


		
		public Indicators.ninZaSupplyDemandDiscovery ninZaSupplyDemandDiscovery(ISeries<double> input , ninZaSupplyDemandDiscovery_LookbackType lookbackType, int lookbackValue, int aTRPeriod, double multiplierQualifyingMove, double multiplierQualifyingFirstBar, int intervalLimit, ninZaSupplyDemandDiscovery_PriceNearMode priceNearMode, bool priceFarOneBarExtensionEnabled, int priceFarOneBarExtensionMax, bool zoneHeightLimitEnabled, int zoneHeightLimitMax, int zoneHeightLimitMin, int moveSplit, int minimumDistanceBetweenZones)
		{
			return indicator.ninZaSupplyDemandDiscovery(input, lookbackType, lookbackValue, aTRPeriod, multiplierQualifyingMove, multiplierQualifyingFirstBar, intervalLimit, priceNearMode, priceFarOneBarExtensionEnabled, priceFarOneBarExtensionMax, zoneHeightLimitEnabled, zoneHeightLimitMax, zoneHeightLimitMin, moveSplit, minimumDistanceBetweenZones);
		}

	}
}

#endregion
