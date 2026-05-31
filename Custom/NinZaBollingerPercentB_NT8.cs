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
		
		private ninZaBollingerPercentB[] cacheninZaBollingerPercentB;

		
		public ninZaBollingerPercentB ninZaBollingerPercentB(ninZaBollingerPercentB_MAType mAType, int period, bool smoothingEnabled, ninZaBollingerPercentB_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return ninZaBollingerPercentB(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}


		
		public ninZaBollingerPercentB ninZaBollingerPercentB(ISeries<double> input, ninZaBollingerPercentB_MAType mAType, int period, bool smoothingEnabled, ninZaBollingerPercentB_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			if (cacheninZaBollingerPercentB != null)
				for (int idx = 0; idx < cacheninZaBollingerPercentB.Length; idx++)
					if (cacheninZaBollingerPercentB[idx].MAType == mAType && cacheninZaBollingerPercentB[idx].Period == period && cacheninZaBollingerPercentB[idx].SmoothingEnabled == smoothingEnabled && cacheninZaBollingerPercentB[idx].SmoothingMethod == smoothingMethod && cacheninZaBollingerPercentB[idx].SmoothingPeriod == smoothingPeriod && cacheninZaBollingerPercentB[idx].StandardDeviation == standardDeviation && cacheninZaBollingerPercentB[idx].EqualsInput(input))
						return cacheninZaBollingerPercentB[idx];
			return CacheIndicator<ninZaBollingerPercentB>(new ninZaBollingerPercentB(){ MAType = mAType, Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, StandardDeviation = standardDeviation }, input, ref cacheninZaBollingerPercentB);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaBollingerPercentB ninZaBollingerPercentB(ninZaBollingerPercentB_MAType mAType, int period, bool smoothingEnabled, ninZaBollingerPercentB_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaBollingerPercentB(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}


		
		public Indicators.ninZaBollingerPercentB ninZaBollingerPercentB(ISeries<double> input , ninZaBollingerPercentB_MAType mAType, int period, bool smoothingEnabled, ninZaBollingerPercentB_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaBollingerPercentB(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaBollingerPercentB ninZaBollingerPercentB(ninZaBollingerPercentB_MAType mAType, int period, bool smoothingEnabled, ninZaBollingerPercentB_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaBollingerPercentB(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}


		
		public Indicators.ninZaBollingerPercentB ninZaBollingerPercentB(ISeries<double> input , ninZaBollingerPercentB_MAType mAType, int period, bool smoothingEnabled, ninZaBollingerPercentB_MAType smoothingMethod, int smoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaBollingerPercentB(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, standardDeviation);
		}

	}
}

#endregion
