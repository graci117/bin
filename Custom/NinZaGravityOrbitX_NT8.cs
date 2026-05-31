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
		
		private ninZaGravityOrbitX[] cacheninZaGravityOrbitX;

		
		public ninZaGravityOrbitX ninZaGravityOrbitX(ninZaGravityOrbitX_OutsideRegionMode mode, int neighborhood, double flatPeriod, int offsetMinVAHVAL, int consolidationPeriod, int consolidationThresholdMid, ninZaGravityOrbitX_ProfileResolution profileResolution, int profileValueAreaPercentage, int signalSplitBars)
		{
			return ninZaGravityOrbitX(Input, mode, neighborhood, flatPeriod, offsetMinVAHVAL, consolidationPeriod, consolidationThresholdMid, profileResolution, profileValueAreaPercentage, signalSplitBars);
		}


		
		public ninZaGravityOrbitX ninZaGravityOrbitX(ISeries<double> input, ninZaGravityOrbitX_OutsideRegionMode mode, int neighborhood, double flatPeriod, int offsetMinVAHVAL, int consolidationPeriod, int consolidationThresholdMid, ninZaGravityOrbitX_ProfileResolution profileResolution, int profileValueAreaPercentage, int signalSplitBars)
		{
			if (cacheninZaGravityOrbitX != null)
				for (int idx = 0; idx < cacheninZaGravityOrbitX.Length; idx++)
					if (cacheninZaGravityOrbitX[idx].Mode == mode && cacheninZaGravityOrbitX[idx].Neighborhood == neighborhood && cacheninZaGravityOrbitX[idx].FlatPeriod == flatPeriod && cacheninZaGravityOrbitX[idx].OffsetMinVAHVAL == offsetMinVAHVAL && cacheninZaGravityOrbitX[idx].ConsolidationPeriod == consolidationPeriod && cacheninZaGravityOrbitX[idx].ConsolidationThresholdMid == consolidationThresholdMid && cacheninZaGravityOrbitX[idx].ProfileResolution == profileResolution && cacheninZaGravityOrbitX[idx].ProfileValueAreaPercentage == profileValueAreaPercentage && cacheninZaGravityOrbitX[idx].SignalSplitBars == signalSplitBars && cacheninZaGravityOrbitX[idx].EqualsInput(input))
						return cacheninZaGravityOrbitX[idx];
			return CacheIndicator<ninZaGravityOrbitX>(new ninZaGravityOrbitX(){ Mode = mode, Neighborhood = neighborhood, FlatPeriod = flatPeriod, OffsetMinVAHVAL = offsetMinVAHVAL, ConsolidationPeriod = consolidationPeriod, ConsolidationThresholdMid = consolidationThresholdMid, ProfileResolution = profileResolution, ProfileValueAreaPercentage = profileValueAreaPercentage, SignalSplitBars = signalSplitBars }, input, ref cacheninZaGravityOrbitX);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaGravityOrbitX ninZaGravityOrbitX(ninZaGravityOrbitX_OutsideRegionMode mode, int neighborhood, double flatPeriod, int offsetMinVAHVAL, int consolidationPeriod, int consolidationThresholdMid, ninZaGravityOrbitX_ProfileResolution profileResolution, int profileValueAreaPercentage, int signalSplitBars)
		{
			return indicator.ninZaGravityOrbitX(Input, mode, neighborhood, flatPeriod, offsetMinVAHVAL, consolidationPeriod, consolidationThresholdMid, profileResolution, profileValueAreaPercentage, signalSplitBars);
		}


		
		public Indicators.ninZaGravityOrbitX ninZaGravityOrbitX(ISeries<double> input , ninZaGravityOrbitX_OutsideRegionMode mode, int neighborhood, double flatPeriod, int offsetMinVAHVAL, int consolidationPeriod, int consolidationThresholdMid, ninZaGravityOrbitX_ProfileResolution profileResolution, int profileValueAreaPercentage, int signalSplitBars)
		{
			return indicator.ninZaGravityOrbitX(input, mode, neighborhood, flatPeriod, offsetMinVAHVAL, consolidationPeriod, consolidationThresholdMid, profileResolution, profileValueAreaPercentage, signalSplitBars);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaGravityOrbitX ninZaGravityOrbitX(ninZaGravityOrbitX_OutsideRegionMode mode, int neighborhood, double flatPeriod, int offsetMinVAHVAL, int consolidationPeriod, int consolidationThresholdMid, ninZaGravityOrbitX_ProfileResolution profileResolution, int profileValueAreaPercentage, int signalSplitBars)
		{
			return indicator.ninZaGravityOrbitX(Input, mode, neighborhood, flatPeriod, offsetMinVAHVAL, consolidationPeriod, consolidationThresholdMid, profileResolution, profileValueAreaPercentage, signalSplitBars);
		}


		
		public Indicators.ninZaGravityOrbitX ninZaGravityOrbitX(ISeries<double> input , ninZaGravityOrbitX_OutsideRegionMode mode, int neighborhood, double flatPeriod, int offsetMinVAHVAL, int consolidationPeriod, int consolidationThresholdMid, ninZaGravityOrbitX_ProfileResolution profileResolution, int profileValueAreaPercentage, int signalSplitBars)
		{
			return indicator.ninZaGravityOrbitX(input, mode, neighborhood, flatPeriod, offsetMinVAHVAL, consolidationPeriod, consolidationThresholdMid, profileResolution, profileValueAreaPercentage, signalSplitBars);
		}

	}
}

#endregion
