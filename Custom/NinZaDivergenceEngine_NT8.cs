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
		
		private ninZaDivergenceEngine[] cacheninZaDivergenceEngine;

		
		public ninZaDivergenceEngine ninZaDivergenceEngine(int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return ninZaDivergenceEngine(Input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}


		
		public ninZaDivergenceEngine ninZaDivergenceEngine(ISeries<double> input, int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			if (cacheninZaDivergenceEngine != null)
				for (int idx = 0; idx < cacheninZaDivergenceEngine.Length; idx++)
					if (cacheninZaDivergenceEngine[idx].NeighborhoodPrice == neighborhoodPrice && cacheninZaDivergenceEngine[idx].NeighbourhoodPlot == neighbourhoodPlot && cacheninZaDivergenceEngine[idx].Vicinity == vicinity && cacheninZaDivergenceEngine[idx].LookbackMin == lookbackMin && cacheninZaDivergenceEngine[idx].LookbackMax == lookbackMax && cacheninZaDivergenceEngine[idx].EqualsInput(input))
						return cacheninZaDivergenceEngine[idx];
			return CacheIndicator<ninZaDivergenceEngine>(new ninZaDivergenceEngine(){ NeighborhoodPrice = neighborhoodPrice, NeighbourhoodPlot = neighbourhoodPlot, Vicinity = vicinity, LookbackMin = lookbackMin, LookbackMax = lookbackMax }, input, ref cacheninZaDivergenceEngine);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaDivergenceEngine ninZaDivergenceEngine(int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceEngine(Input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}


		
		public Indicators.ninZaDivergenceEngine ninZaDivergenceEngine(ISeries<double> input , int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceEngine(input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaDivergenceEngine ninZaDivergenceEngine(int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceEngine(Input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}


		
		public Indicators.ninZaDivergenceEngine ninZaDivergenceEngine(ISeries<double> input , int neighborhoodPrice, int neighbourhoodPlot, int vicinity, int lookbackMin, int lookbackMax)
		{
			return indicator.ninZaDivergenceEngine(input, neighborhoodPrice, neighbourhoodPlot, vicinity, lookbackMin, lookbackMax);
		}

	}
}

#endregion
