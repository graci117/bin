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
		
		private ninZaTStop[] cacheninZaTStop;

		
		public ninZaTStop ninZaTStop(double offsetMultiplier, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight)
		{
			return ninZaTStop(Input, offsetMultiplier, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight);
		}


		
		public ninZaTStop ninZaTStop(ISeries<double> input, double offsetMultiplier, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight)
		{
			if (cacheninZaTStop != null)
				for (int idx = 0; idx < cacheninZaTStop.Length; idx++)
					if (cacheninZaTStop[idx].OffsetMultiplier == offsetMultiplier && cacheninZaTStop[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaTStop[idx].ReferencePricePeriod == referencePricePeriod && cacheninZaTStop[idx].ReferencePriceCloseWeight == referencePriceCloseWeight && cacheninZaTStop[idx].EqualsInput(input))
						return cacheninZaTStop[idx];
			return CacheIndicator<ninZaTStop>(new ninZaTStop(){ OffsetMultiplier = offsetMultiplier, OffsetATRPeriod = offsetATRPeriod, ReferencePricePeriod = referencePricePeriod, ReferencePriceCloseWeight = referencePriceCloseWeight }, input, ref cacheninZaTStop);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaTStop ninZaTStop(double offsetMultiplier, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight)
		{
			return indicator.ninZaTStop(Input, offsetMultiplier, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight);
		}


		
		public Indicators.ninZaTStop ninZaTStop(ISeries<double> input , double offsetMultiplier, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight)
		{
			return indicator.ninZaTStop(input, offsetMultiplier, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaTStop ninZaTStop(double offsetMultiplier, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight)
		{
			return indicator.ninZaTStop(Input, offsetMultiplier, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight);
		}


		
		public Indicators.ninZaTStop ninZaTStop(ISeries<double> input , double offsetMultiplier, int offsetATRPeriod, int referencePricePeriod, double referencePriceCloseWeight)
		{
			return indicator.ninZaTStop(input, offsetMultiplier, offsetATRPeriod, referencePricePeriod, referencePriceCloseWeight);
		}

	}
}

#endregion
