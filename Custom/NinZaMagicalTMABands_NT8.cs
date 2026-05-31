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
		
		private ninZaMagicalTMABands[] cacheninZaMagicalTMABands;

		
		public ninZaMagicalTMABands ninZaMagicalTMABands(int period, double aTRMultiplier, int aTRPeriod, double midPercentage)
		{
			return ninZaMagicalTMABands(Input, period, aTRMultiplier, aTRPeriod, midPercentage);
		}


		
		public ninZaMagicalTMABands ninZaMagicalTMABands(ISeries<double> input, int period, double aTRMultiplier, int aTRPeriod, double midPercentage)
		{
			if (cacheninZaMagicalTMABands != null)
				for (int idx = 0; idx < cacheninZaMagicalTMABands.Length; idx++)
					if (cacheninZaMagicalTMABands[idx].Period == period && cacheninZaMagicalTMABands[idx].ATRMultiplier == aTRMultiplier && cacheninZaMagicalTMABands[idx].ATRPeriod == aTRPeriod && cacheninZaMagicalTMABands[idx].MidPercentage == midPercentage && cacheninZaMagicalTMABands[idx].EqualsInput(input))
						return cacheninZaMagicalTMABands[idx];
			return CacheIndicator<ninZaMagicalTMABands>(new ninZaMagicalTMABands(){ Period = period, ATRMultiplier = aTRMultiplier, ATRPeriod = aTRPeriod, MidPercentage = midPercentage }, input, ref cacheninZaMagicalTMABands);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMagicalTMABands ninZaMagicalTMABands(int period, double aTRMultiplier, int aTRPeriod, double midPercentage)
		{
			return indicator.ninZaMagicalTMABands(Input, period, aTRMultiplier, aTRPeriod, midPercentage);
		}


		
		public Indicators.ninZaMagicalTMABands ninZaMagicalTMABands(ISeries<double> input , int period, double aTRMultiplier, int aTRPeriod, double midPercentage)
		{
			return indicator.ninZaMagicalTMABands(input, period, aTRMultiplier, aTRPeriod, midPercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMagicalTMABands ninZaMagicalTMABands(int period, double aTRMultiplier, int aTRPeriod, double midPercentage)
		{
			return indicator.ninZaMagicalTMABands(Input, period, aTRMultiplier, aTRPeriod, midPercentage);
		}


		
		public Indicators.ninZaMagicalTMABands ninZaMagicalTMABands(ISeries<double> input , int period, double aTRMultiplier, int aTRPeriod, double midPercentage)
		{
			return indicator.ninZaMagicalTMABands(input, period, aTRMultiplier, aTRPeriod, midPercentage);
		}

	}
}

#endregion
