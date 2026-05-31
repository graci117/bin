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
		
		private ninZaSuperiorStochastic[] cacheninZaSuperiorStochastic;

		
		public ninZaSuperiorStochastic ninZaSuperiorStochastic(int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaSuperiorStochastic(Input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaSuperiorStochastic ninZaSuperiorStochastic(ISeries<double> input, int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaSuperiorStochastic != null)
				for (int idx = 0; idx < cacheninZaSuperiorStochastic.Length; idx++)
					if (cacheninZaSuperiorStochastic[idx].KPeriod == kPeriod && cacheninZaSuperiorStochastic[idx].KSmoothingEnabled == kSmoothingEnabled && cacheninZaSuperiorStochastic[idx].KSmoothingMethod == kSmoothingMethod && cacheninZaSuperiorStochastic[idx].KSmoothingPeriod == kSmoothingPeriod && cacheninZaSuperiorStochastic[idx].DMAType == dMAType && cacheninZaSuperiorStochastic[idx].DPeriod == dPeriod && cacheninZaSuperiorStochastic[idx].ThresholdOverbought == thresholdOverbought && cacheninZaSuperiorStochastic[idx].ThresholdOversold == thresholdOversold && cacheninZaSuperiorStochastic[idx].EqualsInput(input))
						return cacheninZaSuperiorStochastic[idx];
			return CacheIndicator<ninZaSuperiorStochastic>(new ninZaSuperiorStochastic(){ KPeriod = kPeriod, KSmoothingEnabled = kSmoothingEnabled, KSmoothingMethod = kSmoothingMethod, KSmoothingPeriod = kSmoothingPeriod, DMAType = dMAType, DPeriod = dPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaSuperiorStochastic);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorStochastic ninZaSuperiorStochastic(int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorStochastic(Input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaSuperiorStochastic ninZaSuperiorStochastic(ISeries<double> input , int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorStochastic(input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorStochastic ninZaSuperiorStochastic(int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorStochastic(Input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaSuperiorStochastic ninZaSuperiorStochastic(ISeries<double> input , int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorStochastic(input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
