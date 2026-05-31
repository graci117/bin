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
		
		private ninZaDivergenceHidden[] cacheninZaDivergenceHidden;

		
		public ninZaDivergenceHidden ninZaDivergenceHidden(int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return ninZaDivergenceHidden(Input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}


		
		public ninZaDivergenceHidden ninZaDivergenceHidden(ISeries<double> input, int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			if (cacheninZaDivergenceHidden != null)
				for (int idx = 0; idx < cacheninZaDivergenceHidden.Length; idx++)
					if (cacheninZaDivergenceHidden[idx].NeighborhoodPrice == neighborhoodPrice && cacheninZaDivergenceHidden[idx].NeighbourhoodPlot == neighbourhoodPlot && cacheninZaDivergenceHidden[idx].Vicinity == vicinity && cacheninZaDivergenceHidden[idx].LookbackMin == lookbackMin && cacheninZaDivergenceHidden[idx].LookbackMax == lookbackMax && cacheninZaDivergenceHidden[idx].EqualsInput(input))
						return cacheninZaDivergenceHidden[idx];
			return CacheIndicator<ninZaDivergenceHidden>(new ninZaDivergenceHidden(){ NeighborhoodPrice = neighborhoodPrice, NeighbourhoodPlot = neighbourhoodPlot, Vicinity = vicinity, LookbackMin = lookbackMin, LookbackMax = lookbackMax }, input, ref cacheninZaDivergenceHidden);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaDivergenceHidden ninZaDivergenceHidden(int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceHidden(Input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}


		
		public Indicators.ninZaDivergenceHidden ninZaDivergenceHidden(ISeries<double> input , int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceHidden(input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaDivergenceHidden ninZaDivergenceHidden(int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceHidden(Input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}


		
		public Indicators.ninZaDivergenceHidden ninZaDivergenceHidden(ISeries<double> input , int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceHidden(input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}

	}
}

#endregion
