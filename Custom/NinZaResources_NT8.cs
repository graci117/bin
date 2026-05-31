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
		
		private ninZaResources[] cacheninZaResources;

		
		public ninZaResources ninZaResources()
		{
			return ninZaResources(Input);
		}


		
		public ninZaResources ninZaResources(ISeries<double> input)
		{
			if (cacheninZaResources != null)
				for (int idx = 0; idx < cacheninZaResources.Length; idx++)
					if ( cacheninZaResources[idx].EqualsInput(input))
						return cacheninZaResources[idx];
			return CacheIndicator<ninZaResources>(new ninZaResources(), input, ref cacheninZaResources);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaResources ninZaResources()
		{
			return indicator.ninZaResources(Input);
		}


		
		public Indicators.ninZaResources ninZaResources(ISeries<double> input )
		{
			return indicator.ninZaResources(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaResources ninZaResources()
		{
			return indicator.ninZaResources(Input);
		}


		
		public Indicators.ninZaResources ninZaResources(ISeries<double> input )
		{
			return indicator.ninZaResources(input);
		}

	}
}

#endregion
