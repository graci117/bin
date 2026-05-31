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
		
		private ninZaSuperiorVolume[] cacheninZaSuperiorVolume;

		
		public ninZaSuperiorVolume ninZaSuperiorVolume(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double highMultiplier, long highMinimum, double mediumMultiplier, long mediumMinimum, int barMinimumSpread, double barNeutralRangePercentage)
		{
			return ninZaSuperiorVolume(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, highMultiplier, highMinimum, mediumMultiplier, mediumMinimum, barMinimumSpread, barNeutralRangePercentage);
		}


		
		public ninZaSuperiorVolume ninZaSuperiorVolume(ISeries<double> input, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double highMultiplier, long highMinimum, double mediumMultiplier, long mediumMinimum, int barMinimumSpread, double barNeutralRangePercentage)
		{
			if (cacheninZaSuperiorVolume != null)
				for (int idx = 0; idx < cacheninZaSuperiorVolume.Length; idx++)
					if (cacheninZaSuperiorVolume[idx].MAType == mAType && cacheninZaSuperiorVolume[idx].MAPeriod == mAPeriod && cacheninZaSuperiorVolume[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaSuperiorVolume[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaSuperiorVolume[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaSuperiorVolume[idx].HighMultiplier == highMultiplier && cacheninZaSuperiorVolume[idx].HighMinimum == highMinimum && cacheninZaSuperiorVolume[idx].MediumMultiplier == mediumMultiplier && cacheninZaSuperiorVolume[idx].MediumMinimum == mediumMinimum && cacheninZaSuperiorVolume[idx].BarMinimumSpread == barMinimumSpread && cacheninZaSuperiorVolume[idx].BarNeutralRangePercentage == barNeutralRangePercentage && cacheninZaSuperiorVolume[idx].EqualsInput(input))
						return cacheninZaSuperiorVolume[idx];
			return CacheIndicator<ninZaSuperiorVolume>(new ninZaSuperiorVolume(){ MAType = mAType, MAPeriod = mAPeriod, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, HighMultiplier = highMultiplier, HighMinimum = highMinimum, MediumMultiplier = mediumMultiplier, MediumMinimum = mediumMinimum, BarMinimumSpread = barMinimumSpread, BarNeutralRangePercentage = barNeutralRangePercentage }, input, ref cacheninZaSuperiorVolume);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorVolume ninZaSuperiorVolume(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double highMultiplier, long highMinimum, double mediumMultiplier, long mediumMinimum, int barMinimumSpread, double barNeutralRangePercentage)
		{
			return indicator.ninZaSuperiorVolume(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, highMultiplier, highMinimum, mediumMultiplier, mediumMinimum, barMinimumSpread, barNeutralRangePercentage);
		}


		
		public Indicators.ninZaSuperiorVolume ninZaSuperiorVolume(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double highMultiplier, long highMinimum, double mediumMultiplier, long mediumMinimum, int barMinimumSpread, double barNeutralRangePercentage)
		{
			return indicator.ninZaSuperiorVolume(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, highMultiplier, highMinimum, mediumMultiplier, mediumMinimum, barMinimumSpread, barNeutralRangePercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorVolume ninZaSuperiorVolume(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double highMultiplier, long highMinimum, double mediumMultiplier, long mediumMinimum, int barMinimumSpread, double barNeutralRangePercentage)
		{
			return indicator.ninZaSuperiorVolume(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, highMultiplier, highMinimum, mediumMultiplier, mediumMinimum, barMinimumSpread, barNeutralRangePercentage);
		}


		
		public Indicators.ninZaSuperiorVolume ninZaSuperiorVolume(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double highMultiplier, long highMinimum, double mediumMultiplier, long mediumMinimum, int barMinimumSpread, double barNeutralRangePercentage)
		{
			return indicator.ninZaSuperiorVolume(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, highMultiplier, highMinimum, mediumMultiplier, mediumMinimum, barMinimumSpread, barNeutralRangePercentage);
		}

	}
}

#endregion
