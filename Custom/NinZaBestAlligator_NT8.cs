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
		
		private ninZaBestAlligator[] cacheninZaBestAlligator;

		
		public ninZaBestAlligator ninZaBestAlligator(int stateTolerance, int crossoverLookback, int aTRPeriod, double thresholdMultiplierAwakening, double thresholdMultiplierFeasting, ninZaBestAlligator_LargeBarMode largeBarMode, double largeBarMultiplier, int lipsShiftRight, ninZaBestAlligator_MAType lipsMAType, int lipsPeriod, bool lipsSmoothingEnabled, ninZa_MAType lipsSmoothingMethod, int lipsSmoothingPeriod, int teethShiftRight, ninZaBestAlligator_MAType teethMAType, int teethPeriod, bool teethSmoothingEnabled, ninZa_MAType teethSmoothingMethod, int teethSmoothingPeriod, int jawShiftRight, ninZaBestAlligator_MAType jawMAType, int jawPeriod, bool jawSmoothingEnabled, ninZa_MAType jawSmoothingMethod, int jawSmoothingPeriod)
		{
			return ninZaBestAlligator(Input, stateTolerance, crossoverLookback, aTRPeriod, thresholdMultiplierAwakening, thresholdMultiplierFeasting, largeBarMode, largeBarMultiplier, lipsShiftRight, lipsMAType, lipsPeriod, lipsSmoothingEnabled, lipsSmoothingMethod, lipsSmoothingPeriod, teethShiftRight, teethMAType, teethPeriod, teethSmoothingEnabled, teethSmoothingMethod, teethSmoothingPeriod, jawShiftRight, jawMAType, jawPeriod, jawSmoothingEnabled, jawSmoothingMethod, jawSmoothingPeriod);
		}


		
		public ninZaBestAlligator ninZaBestAlligator(ISeries<double> input, int stateTolerance, int crossoverLookback, int aTRPeriod, double thresholdMultiplierAwakening, double thresholdMultiplierFeasting, ninZaBestAlligator_LargeBarMode largeBarMode, double largeBarMultiplier, int lipsShiftRight, ninZaBestAlligator_MAType lipsMAType, int lipsPeriod, bool lipsSmoothingEnabled, ninZa_MAType lipsSmoothingMethod, int lipsSmoothingPeriod, int teethShiftRight, ninZaBestAlligator_MAType teethMAType, int teethPeriod, bool teethSmoothingEnabled, ninZa_MAType teethSmoothingMethod, int teethSmoothingPeriod, int jawShiftRight, ninZaBestAlligator_MAType jawMAType, int jawPeriod, bool jawSmoothingEnabled, ninZa_MAType jawSmoothingMethod, int jawSmoothingPeriod)
		{
			if (cacheninZaBestAlligator != null)
				for (int idx = 0; idx < cacheninZaBestAlligator.Length; idx++)
					if (cacheninZaBestAlligator[idx].StateTolerance == stateTolerance && cacheninZaBestAlligator[idx].CrossoverLookback == crossoverLookback && cacheninZaBestAlligator[idx].ATRPeriod == aTRPeriod && cacheninZaBestAlligator[idx].ThresholdMultiplierAwakening == thresholdMultiplierAwakening && cacheninZaBestAlligator[idx].ThresholdMultiplierFeasting == thresholdMultiplierFeasting && cacheninZaBestAlligator[idx].LargeBarMode == largeBarMode && cacheninZaBestAlligator[idx].LargeBarMultiplier == largeBarMultiplier && cacheninZaBestAlligator[idx].LipsShiftRight == lipsShiftRight && cacheninZaBestAlligator[idx].LipsMAType == lipsMAType && cacheninZaBestAlligator[idx].LipsPeriod == lipsPeriod && cacheninZaBestAlligator[idx].LipsSmoothingEnabled == lipsSmoothingEnabled && cacheninZaBestAlligator[idx].LipsSmoothingMethod == lipsSmoothingMethod && cacheninZaBestAlligator[idx].LipsSmoothingPeriod == lipsSmoothingPeriod && cacheninZaBestAlligator[idx].TeethShiftRight == teethShiftRight && cacheninZaBestAlligator[idx].TeethMAType == teethMAType && cacheninZaBestAlligator[idx].TeethPeriod == teethPeriod && cacheninZaBestAlligator[idx].TeethSmoothingEnabled == teethSmoothingEnabled && cacheninZaBestAlligator[idx].TeethSmoothingMethod == teethSmoothingMethod && cacheninZaBestAlligator[idx].TeethSmoothingPeriod == teethSmoothingPeriod && cacheninZaBestAlligator[idx].JawShiftRight == jawShiftRight && cacheninZaBestAlligator[idx].JawMAType == jawMAType && cacheninZaBestAlligator[idx].JawPeriod == jawPeriod && cacheninZaBestAlligator[idx].JawSmoothingEnabled == jawSmoothingEnabled && cacheninZaBestAlligator[idx].JawSmoothingMethod == jawSmoothingMethod && cacheninZaBestAlligator[idx].JawSmoothingPeriod == jawSmoothingPeriod && cacheninZaBestAlligator[idx].EqualsInput(input))
						return cacheninZaBestAlligator[idx];
			return CacheIndicator<ninZaBestAlligator>(new ninZaBestAlligator(){ StateTolerance = stateTolerance, CrossoverLookback = crossoverLookback, ATRPeriod = aTRPeriod, ThresholdMultiplierAwakening = thresholdMultiplierAwakening, ThresholdMultiplierFeasting = thresholdMultiplierFeasting, LargeBarMode = largeBarMode, LargeBarMultiplier = largeBarMultiplier, LipsShiftRight = lipsShiftRight, LipsMAType = lipsMAType, LipsPeriod = lipsPeriod, LipsSmoothingEnabled = lipsSmoothingEnabled, LipsSmoothingMethod = lipsSmoothingMethod, LipsSmoothingPeriod = lipsSmoothingPeriod, TeethShiftRight = teethShiftRight, TeethMAType = teethMAType, TeethPeriod = teethPeriod, TeethSmoothingEnabled = teethSmoothingEnabled, TeethSmoothingMethod = teethSmoothingMethod, TeethSmoothingPeriod = teethSmoothingPeriod, JawShiftRight = jawShiftRight, JawMAType = jawMAType, JawPeriod = jawPeriod, JawSmoothingEnabled = jawSmoothingEnabled, JawSmoothingMethod = jawSmoothingMethod, JawSmoothingPeriod = jawSmoothingPeriod }, input, ref cacheninZaBestAlligator);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaBestAlligator ninZaBestAlligator(int stateTolerance, int crossoverLookback, int aTRPeriod, double thresholdMultiplierAwakening, double thresholdMultiplierFeasting, ninZaBestAlligator_LargeBarMode largeBarMode, double largeBarMultiplier, int lipsShiftRight, ninZaBestAlligator_MAType lipsMAType, int lipsPeriod, bool lipsSmoothingEnabled, ninZa_MAType lipsSmoothingMethod, int lipsSmoothingPeriod, int teethShiftRight, ninZaBestAlligator_MAType teethMAType, int teethPeriod, bool teethSmoothingEnabled, ninZa_MAType teethSmoothingMethod, int teethSmoothingPeriod, int jawShiftRight, ninZaBestAlligator_MAType jawMAType, int jawPeriod, bool jawSmoothingEnabled, ninZa_MAType jawSmoothingMethod, int jawSmoothingPeriod)
		{
			return indicator.ninZaBestAlligator(Input, stateTolerance, crossoverLookback, aTRPeriod, thresholdMultiplierAwakening, thresholdMultiplierFeasting, largeBarMode, largeBarMultiplier, lipsShiftRight, lipsMAType, lipsPeriod, lipsSmoothingEnabled, lipsSmoothingMethod, lipsSmoothingPeriod, teethShiftRight, teethMAType, teethPeriod, teethSmoothingEnabled, teethSmoothingMethod, teethSmoothingPeriod, jawShiftRight, jawMAType, jawPeriod, jawSmoothingEnabled, jawSmoothingMethod, jawSmoothingPeriod);
		}


		
		public Indicators.ninZaBestAlligator ninZaBestAlligator(ISeries<double> input , int stateTolerance, int crossoverLookback, int aTRPeriod, double thresholdMultiplierAwakening, double thresholdMultiplierFeasting, ninZaBestAlligator_LargeBarMode largeBarMode, double largeBarMultiplier, int lipsShiftRight, ninZaBestAlligator_MAType lipsMAType, int lipsPeriod, bool lipsSmoothingEnabled, ninZa_MAType lipsSmoothingMethod, int lipsSmoothingPeriod, int teethShiftRight, ninZaBestAlligator_MAType teethMAType, int teethPeriod, bool teethSmoothingEnabled, ninZa_MAType teethSmoothingMethod, int teethSmoothingPeriod, int jawShiftRight, ninZaBestAlligator_MAType jawMAType, int jawPeriod, bool jawSmoothingEnabled, ninZa_MAType jawSmoothingMethod, int jawSmoothingPeriod)
		{
			return indicator.ninZaBestAlligator(input, stateTolerance, crossoverLookback, aTRPeriod, thresholdMultiplierAwakening, thresholdMultiplierFeasting, largeBarMode, largeBarMultiplier, lipsShiftRight, lipsMAType, lipsPeriod, lipsSmoothingEnabled, lipsSmoothingMethod, lipsSmoothingPeriod, teethShiftRight, teethMAType, teethPeriod, teethSmoothingEnabled, teethSmoothingMethod, teethSmoothingPeriod, jawShiftRight, jawMAType, jawPeriod, jawSmoothingEnabled, jawSmoothingMethod, jawSmoothingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaBestAlligator ninZaBestAlligator(int stateTolerance, int crossoverLookback, int aTRPeriod, double thresholdMultiplierAwakening, double thresholdMultiplierFeasting, ninZaBestAlligator_LargeBarMode largeBarMode, double largeBarMultiplier, int lipsShiftRight, ninZaBestAlligator_MAType lipsMAType, int lipsPeriod, bool lipsSmoothingEnabled, ninZa_MAType lipsSmoothingMethod, int lipsSmoothingPeriod, int teethShiftRight, ninZaBestAlligator_MAType teethMAType, int teethPeriod, bool teethSmoothingEnabled, ninZa_MAType teethSmoothingMethod, int teethSmoothingPeriod, int jawShiftRight, ninZaBestAlligator_MAType jawMAType, int jawPeriod, bool jawSmoothingEnabled, ninZa_MAType jawSmoothingMethod, int jawSmoothingPeriod)
		{
			return indicator.ninZaBestAlligator(Input, stateTolerance, crossoverLookback, aTRPeriod, thresholdMultiplierAwakening, thresholdMultiplierFeasting, largeBarMode, largeBarMultiplier, lipsShiftRight, lipsMAType, lipsPeriod, lipsSmoothingEnabled, lipsSmoothingMethod, lipsSmoothingPeriod, teethShiftRight, teethMAType, teethPeriod, teethSmoothingEnabled, teethSmoothingMethod, teethSmoothingPeriod, jawShiftRight, jawMAType, jawPeriod, jawSmoothingEnabled, jawSmoothingMethod, jawSmoothingPeriod);
		}


		
		public Indicators.ninZaBestAlligator ninZaBestAlligator(ISeries<double> input , int stateTolerance, int crossoverLookback, int aTRPeriod, double thresholdMultiplierAwakening, double thresholdMultiplierFeasting, ninZaBestAlligator_LargeBarMode largeBarMode, double largeBarMultiplier, int lipsShiftRight, ninZaBestAlligator_MAType lipsMAType, int lipsPeriod, bool lipsSmoothingEnabled, ninZa_MAType lipsSmoothingMethod, int lipsSmoothingPeriod, int teethShiftRight, ninZaBestAlligator_MAType teethMAType, int teethPeriod, bool teethSmoothingEnabled, ninZa_MAType teethSmoothingMethod, int teethSmoothingPeriod, int jawShiftRight, ninZaBestAlligator_MAType jawMAType, int jawPeriod, bool jawSmoothingEnabled, ninZa_MAType jawSmoothingMethod, int jawSmoothingPeriod)
		{
			return indicator.ninZaBestAlligator(input, stateTolerance, crossoverLookback, aTRPeriod, thresholdMultiplierAwakening, thresholdMultiplierFeasting, largeBarMode, largeBarMultiplier, lipsShiftRight, lipsMAType, lipsPeriod, lipsSmoothingEnabled, lipsSmoothingMethod, lipsSmoothingPeriod, teethShiftRight, teethMAType, teethPeriod, teethSmoothingEnabled, teethSmoothingMethod, teethSmoothingPeriod, jawShiftRight, jawMAType, jawPeriod, jawSmoothingEnabled, jawSmoothingMethod, jawSmoothingPeriod);
		}

	}
}

#endregion
