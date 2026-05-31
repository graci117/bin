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
		
		private ninZaHeikenAshiSmartDelta[] cacheninZaHeikenAshiSmartDelta;

		
		public ninZaHeikenAshiSmartDelta ninZaHeikenAshiSmartDelta(bool hASmoothingEnabled, ninZa_MAType hASmoothingMethod, int hASmoothingPeriod, int hAOpenWeight, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceFrom, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceTo, ninZa_MAType deltaMAType, int deltaMAPeriod, bool deltaMASmoothingEnabled, ninZa_MAType deltaMASmoothingMethod, int deltaMASmoothingPeriod, double thresholdUpper, double thresholdLower)
		{
			return ninZaHeikenAshiSmartDelta(Input, hASmoothingEnabled, hASmoothingMethod, hASmoothingPeriod, hAOpenWeight, deltaPriceFrom, deltaPriceTo, deltaMAType, deltaMAPeriod, deltaMASmoothingEnabled, deltaMASmoothingMethod, deltaMASmoothingPeriod, thresholdUpper, thresholdLower);
		}


		
		public ninZaHeikenAshiSmartDelta ninZaHeikenAshiSmartDelta(ISeries<double> input, bool hASmoothingEnabled, ninZa_MAType hASmoothingMethod, int hASmoothingPeriod, int hAOpenWeight, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceFrom, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceTo, ninZa_MAType deltaMAType, int deltaMAPeriod, bool deltaMASmoothingEnabled, ninZa_MAType deltaMASmoothingMethod, int deltaMASmoothingPeriod, double thresholdUpper, double thresholdLower)
		{
			if (cacheninZaHeikenAshiSmartDelta != null)
				for (int idx = 0; idx < cacheninZaHeikenAshiSmartDelta.Length; idx++)
					if (cacheninZaHeikenAshiSmartDelta[idx].HASmoothingEnabled == hASmoothingEnabled && cacheninZaHeikenAshiSmartDelta[idx].HASmoothingMethod == hASmoothingMethod && cacheninZaHeikenAshiSmartDelta[idx].HASmoothingPeriod == hASmoothingPeriod && cacheninZaHeikenAshiSmartDelta[idx].HAOpenWeight == hAOpenWeight && cacheninZaHeikenAshiSmartDelta[idx].DeltaPriceFrom == deltaPriceFrom && cacheninZaHeikenAshiSmartDelta[idx].DeltaPriceTo == deltaPriceTo && cacheninZaHeikenAshiSmartDelta[idx].DeltaMAType == deltaMAType && cacheninZaHeikenAshiSmartDelta[idx].DeltaMAPeriod == deltaMAPeriod && cacheninZaHeikenAshiSmartDelta[idx].DeltaMASmoothingEnabled == deltaMASmoothingEnabled && cacheninZaHeikenAshiSmartDelta[idx].DeltaMASmoothingMethod == deltaMASmoothingMethod && cacheninZaHeikenAshiSmartDelta[idx].DeltaMASmoothingPeriod == deltaMASmoothingPeriod && cacheninZaHeikenAshiSmartDelta[idx].ThresholdUpper == thresholdUpper && cacheninZaHeikenAshiSmartDelta[idx].ThresholdLower == thresholdLower && cacheninZaHeikenAshiSmartDelta[idx].EqualsInput(input))
						return cacheninZaHeikenAshiSmartDelta[idx];
			return CacheIndicator<ninZaHeikenAshiSmartDelta>(new ninZaHeikenAshiSmartDelta(){ HASmoothingEnabled = hASmoothingEnabled, HASmoothingMethod = hASmoothingMethod, HASmoothingPeriod = hASmoothingPeriod, HAOpenWeight = hAOpenWeight, DeltaPriceFrom = deltaPriceFrom, DeltaPriceTo = deltaPriceTo, DeltaMAType = deltaMAType, DeltaMAPeriod = deltaMAPeriod, DeltaMASmoothingEnabled = deltaMASmoothingEnabled, DeltaMASmoothingMethod = deltaMASmoothingMethod, DeltaMASmoothingPeriod = deltaMASmoothingPeriod, ThresholdUpper = thresholdUpper, ThresholdLower = thresholdLower }, input, ref cacheninZaHeikenAshiSmartDelta);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHeikenAshiSmartDelta ninZaHeikenAshiSmartDelta(bool hASmoothingEnabled, ninZa_MAType hASmoothingMethod, int hASmoothingPeriod, int hAOpenWeight, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceFrom, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceTo, ninZa_MAType deltaMAType, int deltaMAPeriod, bool deltaMASmoothingEnabled, ninZa_MAType deltaMASmoothingMethod, int deltaMASmoothingPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaHeikenAshiSmartDelta(Input, hASmoothingEnabled, hASmoothingMethod, hASmoothingPeriod, hAOpenWeight, deltaPriceFrom, deltaPriceTo, deltaMAType, deltaMAPeriod, deltaMASmoothingEnabled, deltaMASmoothingMethod, deltaMASmoothingPeriod, thresholdUpper, thresholdLower);
		}


		
		public Indicators.ninZaHeikenAshiSmartDelta ninZaHeikenAshiSmartDelta(ISeries<double> input , bool hASmoothingEnabled, ninZa_MAType hASmoothingMethod, int hASmoothingPeriod, int hAOpenWeight, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceFrom, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceTo, ninZa_MAType deltaMAType, int deltaMAPeriod, bool deltaMASmoothingEnabled, ninZa_MAType deltaMASmoothingMethod, int deltaMASmoothingPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaHeikenAshiSmartDelta(input, hASmoothingEnabled, hASmoothingMethod, hASmoothingPeriod, hAOpenWeight, deltaPriceFrom, deltaPriceTo, deltaMAType, deltaMAPeriod, deltaMASmoothingEnabled, deltaMASmoothingMethod, deltaMASmoothingPeriod, thresholdUpper, thresholdLower);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHeikenAshiSmartDelta ninZaHeikenAshiSmartDelta(bool hASmoothingEnabled, ninZa_MAType hASmoothingMethod, int hASmoothingPeriod, int hAOpenWeight, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceFrom, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceTo, ninZa_MAType deltaMAType, int deltaMAPeriod, bool deltaMASmoothingEnabled, ninZa_MAType deltaMASmoothingMethod, int deltaMASmoothingPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaHeikenAshiSmartDelta(Input, hASmoothingEnabled, hASmoothingMethod, hASmoothingPeriod, hAOpenWeight, deltaPriceFrom, deltaPriceTo, deltaMAType, deltaMAPeriod, deltaMASmoothingEnabled, deltaMASmoothingMethod, deltaMASmoothingPeriod, thresholdUpper, thresholdLower);
		}


		
		public Indicators.ninZaHeikenAshiSmartDelta ninZaHeikenAshiSmartDelta(ISeries<double> input , bool hASmoothingEnabled, ninZa_MAType hASmoothingMethod, int hASmoothingPeriod, int hAOpenWeight, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceFrom, ninZaHeikenAshiSmartDelta_HAPriceType deltaPriceTo, ninZa_MAType deltaMAType, int deltaMAPeriod, bool deltaMASmoothingEnabled, ninZa_MAType deltaMASmoothingMethod, int deltaMASmoothingPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaHeikenAshiSmartDelta(input, hASmoothingEnabled, hASmoothingMethod, hASmoothingPeriod, hAOpenWeight, deltaPriceFrom, deltaPriceTo, deltaMAType, deltaMAPeriod, deltaMASmoothingEnabled, deltaMASmoothingMethod, deltaMASmoothingPeriod, thresholdUpper, thresholdLower);
		}

	}
}

#endregion
