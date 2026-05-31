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
		
		private ninZaSuperJumpBoost[] cacheninZaSuperJumpBoost;

		
		public ninZaSuperJumpBoost ninZaSuperJumpBoost(bool sensitiveModeEnabled, double offsetLevel1, double offsetLevel2, double offsetLevel3, double offsetLevel4, double offsetBase, int referencePricePeriod, int lineLevelsOffset, int extremeNeighborhood, int signalCloseThreshold, int signalQuantityPerZone, int signalSplit)
		{
			return ninZaSuperJumpBoost(Input, sensitiveModeEnabled, offsetLevel1, offsetLevel2, offsetLevel3, offsetLevel4, offsetBase, referencePricePeriod, lineLevelsOffset, extremeNeighborhood, signalCloseThreshold, signalQuantityPerZone, signalSplit);
		}


		
		public ninZaSuperJumpBoost ninZaSuperJumpBoost(ISeries<double> input, bool sensitiveModeEnabled, double offsetLevel1, double offsetLevel2, double offsetLevel3, double offsetLevel4, double offsetBase, int referencePricePeriod, int lineLevelsOffset, int extremeNeighborhood, int signalCloseThreshold, int signalQuantityPerZone, int signalSplit)
		{
			if (cacheninZaSuperJumpBoost != null)
				for (int idx = 0; idx < cacheninZaSuperJumpBoost.Length; idx++)
					if (cacheninZaSuperJumpBoost[idx].SensitiveModeEnabled == sensitiveModeEnabled && cacheninZaSuperJumpBoost[idx].OffsetLevel1 == offsetLevel1 && cacheninZaSuperJumpBoost[idx].OffsetLevel2 == offsetLevel2 && cacheninZaSuperJumpBoost[idx].OffsetLevel3 == offsetLevel3 && cacheninZaSuperJumpBoost[idx].OffsetLevel4 == offsetLevel4 && cacheninZaSuperJumpBoost[idx].OffsetBase == offsetBase && cacheninZaSuperJumpBoost[idx].ReferencePricePeriod == referencePricePeriod && cacheninZaSuperJumpBoost[idx].LineLevelsOffset == lineLevelsOffset && cacheninZaSuperJumpBoost[idx].ExtremeNeighborhood == extremeNeighborhood && cacheninZaSuperJumpBoost[idx].SignalCloseThreshold == signalCloseThreshold && cacheninZaSuperJumpBoost[idx].SignalQuantityPerZone == signalQuantityPerZone && cacheninZaSuperJumpBoost[idx].SignalSplit == signalSplit && cacheninZaSuperJumpBoost[idx].EqualsInput(input))
						return cacheninZaSuperJumpBoost[idx];
			return CacheIndicator<ninZaSuperJumpBoost>(new ninZaSuperJumpBoost(){ SensitiveModeEnabled = sensitiveModeEnabled, OffsetLevel1 = offsetLevel1, OffsetLevel2 = offsetLevel2, OffsetLevel3 = offsetLevel3, OffsetLevel4 = offsetLevel4, OffsetBase = offsetBase, ReferencePricePeriod = referencePricePeriod, LineLevelsOffset = lineLevelsOffset, ExtremeNeighborhood = extremeNeighborhood, SignalCloseThreshold = signalCloseThreshold, SignalQuantityPerZone = signalQuantityPerZone, SignalSplit = signalSplit }, input, ref cacheninZaSuperJumpBoost);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperJumpBoost ninZaSuperJumpBoost(bool sensitiveModeEnabled, double offsetLevel1, double offsetLevel2, double offsetLevel3, double offsetLevel4, double offsetBase, int referencePricePeriod, int lineLevelsOffset, int extremeNeighborhood, int signalCloseThreshold, int signalQuantityPerZone, int signalSplit)
		{
			return indicator.ninZaSuperJumpBoost(Input, sensitiveModeEnabled, offsetLevel1, offsetLevel2, offsetLevel3, offsetLevel4, offsetBase, referencePricePeriod, lineLevelsOffset, extremeNeighborhood, signalCloseThreshold, signalQuantityPerZone, signalSplit);
		}


		
		public Indicators.ninZaSuperJumpBoost ninZaSuperJumpBoost(ISeries<double> input , bool sensitiveModeEnabled, double offsetLevel1, double offsetLevel2, double offsetLevel3, double offsetLevel4, double offsetBase, int referencePricePeriod, int lineLevelsOffset, int extremeNeighborhood, int signalCloseThreshold, int signalQuantityPerZone, int signalSplit)
		{
			return indicator.ninZaSuperJumpBoost(input, sensitiveModeEnabled, offsetLevel1, offsetLevel2, offsetLevel3, offsetLevel4, offsetBase, referencePricePeriod, lineLevelsOffset, extremeNeighborhood, signalCloseThreshold, signalQuantityPerZone, signalSplit);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperJumpBoost ninZaSuperJumpBoost(bool sensitiveModeEnabled, double offsetLevel1, double offsetLevel2, double offsetLevel3, double offsetLevel4, double offsetBase, int referencePricePeriod, int lineLevelsOffset, int extremeNeighborhood, int signalCloseThreshold, int signalQuantityPerZone, int signalSplit)
		{
			return indicator.ninZaSuperJumpBoost(Input, sensitiveModeEnabled, offsetLevel1, offsetLevel2, offsetLevel3, offsetLevel4, offsetBase, referencePricePeriod, lineLevelsOffset, extremeNeighborhood, signalCloseThreshold, signalQuantityPerZone, signalSplit);
		}


		
		public Indicators.ninZaSuperJumpBoost ninZaSuperJumpBoost(ISeries<double> input , bool sensitiveModeEnabled, double offsetLevel1, double offsetLevel2, double offsetLevel3, double offsetLevel4, double offsetBase, int referencePricePeriod, int lineLevelsOffset, int extremeNeighborhood, int signalCloseThreshold, int signalQuantityPerZone, int signalSplit)
		{
			return indicator.ninZaSuperJumpBoost(input, sensitiveModeEnabled, offsetLevel1, offsetLevel2, offsetLevel3, offsetLevel4, offsetBase, referencePricePeriod, lineLevelsOffset, extremeNeighborhood, signalCloseThreshold, signalQuantityPerZone, signalSplit);
		}

	}
}

#endregion
