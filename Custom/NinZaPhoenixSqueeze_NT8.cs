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
		
		private ninZaPhoenixSqueeze[] cacheninZaPhoenixSqueeze;

		
		public ninZaPhoenixSqueeze ninZaPhoenixSqueeze(ninZa_MAType bollingerMAType, int bollingerMAPeriod, bool bollingerSmoothingEnabled, ninZa_MAType bollingerSmoothingMethod, int bollingerSmoothingPeriod, double bollingerOffset, ninZa_MAType keltnerMAType, int keltnerMAPeriod, bool keltnerSmoothingEnabled, ninZa_MAType keltnerSmoothingMethod, int keltnerSmoothingPeriod, double keltnerOffsetMultiplier, ninZaPhoenixSqueeze_OffsetUnit keltnerOffsetUnit, int keltnerOffsetATRPeriod, int momentumDonchianPeriod, ninZa_MAType momentumClosePriceMAType, int momentumClosePriceMAPeriod, ninZa_MAType momentumSmoothingMethod, int momentumSmoothingPeriod, double ratioContractionStrong, double ratioExpansionStrong, int safeExplosionPeriod, double thresholdUpper, double thresholdLower)
		{
			return ninZaPhoenixSqueeze(Input, bollingerMAType, bollingerMAPeriod, bollingerSmoothingEnabled, bollingerSmoothingMethod, bollingerSmoothingPeriod, bollingerOffset, keltnerMAType, keltnerMAPeriod, keltnerSmoothingEnabled, keltnerSmoothingMethod, keltnerSmoothingPeriod, keltnerOffsetMultiplier, keltnerOffsetUnit, keltnerOffsetATRPeriod, momentumDonchianPeriod, momentumClosePriceMAType, momentumClosePriceMAPeriod, momentumSmoothingMethod, momentumSmoothingPeriod, ratioContractionStrong, ratioExpansionStrong, safeExplosionPeriod, thresholdUpper, thresholdLower);
		}


		
		public ninZaPhoenixSqueeze ninZaPhoenixSqueeze(ISeries<double> input, ninZa_MAType bollingerMAType, int bollingerMAPeriod, bool bollingerSmoothingEnabled, ninZa_MAType bollingerSmoothingMethod, int bollingerSmoothingPeriod, double bollingerOffset, ninZa_MAType keltnerMAType, int keltnerMAPeriod, bool keltnerSmoothingEnabled, ninZa_MAType keltnerSmoothingMethod, int keltnerSmoothingPeriod, double keltnerOffsetMultiplier, ninZaPhoenixSqueeze_OffsetUnit keltnerOffsetUnit, int keltnerOffsetATRPeriod, int momentumDonchianPeriod, ninZa_MAType momentumClosePriceMAType, int momentumClosePriceMAPeriod, ninZa_MAType momentumSmoothingMethod, int momentumSmoothingPeriod, double ratioContractionStrong, double ratioExpansionStrong, int safeExplosionPeriod, double thresholdUpper, double thresholdLower)
		{
			if (cacheninZaPhoenixSqueeze != null)
				for (int idx = 0; idx < cacheninZaPhoenixSqueeze.Length; idx++)
					if (cacheninZaPhoenixSqueeze[idx].BollingerMAType == bollingerMAType && cacheninZaPhoenixSqueeze[idx].BollingerMAPeriod == bollingerMAPeriod && cacheninZaPhoenixSqueeze[idx].BollingerSmoothingEnabled == bollingerSmoothingEnabled && cacheninZaPhoenixSqueeze[idx].BollingerSmoothingMethod == bollingerSmoothingMethod && cacheninZaPhoenixSqueeze[idx].BollingerSmoothingPeriod == bollingerSmoothingPeriod && cacheninZaPhoenixSqueeze[idx].BollingerOffset == bollingerOffset && cacheninZaPhoenixSqueeze[idx].KeltnerMAType == keltnerMAType && cacheninZaPhoenixSqueeze[idx].KeltnerMAPeriod == keltnerMAPeriod && cacheninZaPhoenixSqueeze[idx].KeltnerSmoothingEnabled == keltnerSmoothingEnabled && cacheninZaPhoenixSqueeze[idx].KeltnerSmoothingMethod == keltnerSmoothingMethod && cacheninZaPhoenixSqueeze[idx].KeltnerSmoothingPeriod == keltnerSmoothingPeriod && cacheninZaPhoenixSqueeze[idx].KeltnerOffsetMultiplier == keltnerOffsetMultiplier && cacheninZaPhoenixSqueeze[idx].KeltnerOffsetUnit == keltnerOffsetUnit && cacheninZaPhoenixSqueeze[idx].KeltnerOffsetATRPeriod == keltnerOffsetATRPeriod && cacheninZaPhoenixSqueeze[idx].MomentumDonchianPeriod == momentumDonchianPeriod && cacheninZaPhoenixSqueeze[idx].MomentumClosePriceMAType == momentumClosePriceMAType && cacheninZaPhoenixSqueeze[idx].MomentumClosePriceMAPeriod == momentumClosePriceMAPeriod && cacheninZaPhoenixSqueeze[idx].MomentumSmoothingMethod == momentumSmoothingMethod && cacheninZaPhoenixSqueeze[idx].MomentumSmoothingPeriod == momentumSmoothingPeriod && cacheninZaPhoenixSqueeze[idx].RatioContractionStrong == ratioContractionStrong && cacheninZaPhoenixSqueeze[idx].RatioExpansionStrong == ratioExpansionStrong && cacheninZaPhoenixSqueeze[idx].SafeExplosionPeriod == safeExplosionPeriod && cacheninZaPhoenixSqueeze[idx].ThresholdUpper == thresholdUpper && cacheninZaPhoenixSqueeze[idx].ThresholdLower == thresholdLower && cacheninZaPhoenixSqueeze[idx].EqualsInput(input))
						return cacheninZaPhoenixSqueeze[idx];
			return CacheIndicator<ninZaPhoenixSqueeze>(new ninZaPhoenixSqueeze(){ BollingerMAType = bollingerMAType, BollingerMAPeriod = bollingerMAPeriod, BollingerSmoothingEnabled = bollingerSmoothingEnabled, BollingerSmoothingMethod = bollingerSmoothingMethod, BollingerSmoothingPeriod = bollingerSmoothingPeriod, BollingerOffset = bollingerOffset, KeltnerMAType = keltnerMAType, KeltnerMAPeriod = keltnerMAPeriod, KeltnerSmoothingEnabled = keltnerSmoothingEnabled, KeltnerSmoothingMethod = keltnerSmoothingMethod, KeltnerSmoothingPeriod = keltnerSmoothingPeriod, KeltnerOffsetMultiplier = keltnerOffsetMultiplier, KeltnerOffsetUnit = keltnerOffsetUnit, KeltnerOffsetATRPeriod = keltnerOffsetATRPeriod, MomentumDonchianPeriod = momentumDonchianPeriod, MomentumClosePriceMAType = momentumClosePriceMAType, MomentumClosePriceMAPeriod = momentumClosePriceMAPeriod, MomentumSmoothingMethod = momentumSmoothingMethod, MomentumSmoothingPeriod = momentumSmoothingPeriod, RatioContractionStrong = ratioContractionStrong, RatioExpansionStrong = ratioExpansionStrong, SafeExplosionPeriod = safeExplosionPeriod, ThresholdUpper = thresholdUpper, ThresholdLower = thresholdLower }, input, ref cacheninZaPhoenixSqueeze);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaPhoenixSqueeze ninZaPhoenixSqueeze(ninZa_MAType bollingerMAType, int bollingerMAPeriod, bool bollingerSmoothingEnabled, ninZa_MAType bollingerSmoothingMethod, int bollingerSmoothingPeriod, double bollingerOffset, ninZa_MAType keltnerMAType, int keltnerMAPeriod, bool keltnerSmoothingEnabled, ninZa_MAType keltnerSmoothingMethod, int keltnerSmoothingPeriod, double keltnerOffsetMultiplier, ninZaPhoenixSqueeze_OffsetUnit keltnerOffsetUnit, int keltnerOffsetATRPeriod, int momentumDonchianPeriod, ninZa_MAType momentumClosePriceMAType, int momentumClosePriceMAPeriod, ninZa_MAType momentumSmoothingMethod, int momentumSmoothingPeriod, double ratioContractionStrong, double ratioExpansionStrong, int safeExplosionPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaPhoenixSqueeze(Input, bollingerMAType, bollingerMAPeriod, bollingerSmoothingEnabled, bollingerSmoothingMethod, bollingerSmoothingPeriod, bollingerOffset, keltnerMAType, keltnerMAPeriod, keltnerSmoothingEnabled, keltnerSmoothingMethod, keltnerSmoothingPeriod, keltnerOffsetMultiplier, keltnerOffsetUnit, keltnerOffsetATRPeriod, momentumDonchianPeriod, momentumClosePriceMAType, momentumClosePriceMAPeriod, momentumSmoothingMethod, momentumSmoothingPeriod, ratioContractionStrong, ratioExpansionStrong, safeExplosionPeriod, thresholdUpper, thresholdLower);
		}


		
		public Indicators.ninZaPhoenixSqueeze ninZaPhoenixSqueeze(ISeries<double> input , ninZa_MAType bollingerMAType, int bollingerMAPeriod, bool bollingerSmoothingEnabled, ninZa_MAType bollingerSmoothingMethod, int bollingerSmoothingPeriod, double bollingerOffset, ninZa_MAType keltnerMAType, int keltnerMAPeriod, bool keltnerSmoothingEnabled, ninZa_MAType keltnerSmoothingMethod, int keltnerSmoothingPeriod, double keltnerOffsetMultiplier, ninZaPhoenixSqueeze_OffsetUnit keltnerOffsetUnit, int keltnerOffsetATRPeriod, int momentumDonchianPeriod, ninZa_MAType momentumClosePriceMAType, int momentumClosePriceMAPeriod, ninZa_MAType momentumSmoothingMethod, int momentumSmoothingPeriod, double ratioContractionStrong, double ratioExpansionStrong, int safeExplosionPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaPhoenixSqueeze(input, bollingerMAType, bollingerMAPeriod, bollingerSmoothingEnabled, bollingerSmoothingMethod, bollingerSmoothingPeriod, bollingerOffset, keltnerMAType, keltnerMAPeriod, keltnerSmoothingEnabled, keltnerSmoothingMethod, keltnerSmoothingPeriod, keltnerOffsetMultiplier, keltnerOffsetUnit, keltnerOffsetATRPeriod, momentumDonchianPeriod, momentumClosePriceMAType, momentumClosePriceMAPeriod, momentumSmoothingMethod, momentumSmoothingPeriod, ratioContractionStrong, ratioExpansionStrong, safeExplosionPeriod, thresholdUpper, thresholdLower);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaPhoenixSqueeze ninZaPhoenixSqueeze(ninZa_MAType bollingerMAType, int bollingerMAPeriod, bool bollingerSmoothingEnabled, ninZa_MAType bollingerSmoothingMethod, int bollingerSmoothingPeriod, double bollingerOffset, ninZa_MAType keltnerMAType, int keltnerMAPeriod, bool keltnerSmoothingEnabled, ninZa_MAType keltnerSmoothingMethod, int keltnerSmoothingPeriod, double keltnerOffsetMultiplier, ninZaPhoenixSqueeze_OffsetUnit keltnerOffsetUnit, int keltnerOffsetATRPeriod, int momentumDonchianPeriod, ninZa_MAType momentumClosePriceMAType, int momentumClosePriceMAPeriod, ninZa_MAType momentumSmoothingMethod, int momentumSmoothingPeriod, double ratioContractionStrong, double ratioExpansionStrong, int safeExplosionPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaPhoenixSqueeze(Input, bollingerMAType, bollingerMAPeriod, bollingerSmoothingEnabled, bollingerSmoothingMethod, bollingerSmoothingPeriod, bollingerOffset, keltnerMAType, keltnerMAPeriod, keltnerSmoothingEnabled, keltnerSmoothingMethod, keltnerSmoothingPeriod, keltnerOffsetMultiplier, keltnerOffsetUnit, keltnerOffsetATRPeriod, momentumDonchianPeriod, momentumClosePriceMAType, momentumClosePriceMAPeriod, momentumSmoothingMethod, momentumSmoothingPeriod, ratioContractionStrong, ratioExpansionStrong, safeExplosionPeriod, thresholdUpper, thresholdLower);
		}


		
		public Indicators.ninZaPhoenixSqueeze ninZaPhoenixSqueeze(ISeries<double> input , ninZa_MAType bollingerMAType, int bollingerMAPeriod, bool bollingerSmoothingEnabled, ninZa_MAType bollingerSmoothingMethod, int bollingerSmoothingPeriod, double bollingerOffset, ninZa_MAType keltnerMAType, int keltnerMAPeriod, bool keltnerSmoothingEnabled, ninZa_MAType keltnerSmoothingMethod, int keltnerSmoothingPeriod, double keltnerOffsetMultiplier, ninZaPhoenixSqueeze_OffsetUnit keltnerOffsetUnit, int keltnerOffsetATRPeriod, int momentumDonchianPeriod, ninZa_MAType momentumClosePriceMAType, int momentumClosePriceMAPeriod, ninZa_MAType momentumSmoothingMethod, int momentumSmoothingPeriod, double ratioContractionStrong, double ratioExpansionStrong, int safeExplosionPeriod, double thresholdUpper, double thresholdLower)
		{
			return indicator.ninZaPhoenixSqueeze(input, bollingerMAType, bollingerMAPeriod, bollingerSmoothingEnabled, bollingerSmoothingMethod, bollingerSmoothingPeriod, bollingerOffset, keltnerMAType, keltnerMAPeriod, keltnerSmoothingEnabled, keltnerSmoothingMethod, keltnerSmoothingPeriod, keltnerOffsetMultiplier, keltnerOffsetUnit, keltnerOffsetATRPeriod, momentumDonchianPeriod, momentumClosePriceMAType, momentumClosePriceMAPeriod, momentumSmoothingMethod, momentumSmoothingPeriod, ratioContractionStrong, ratioExpansionStrong, safeExplosionPeriod, thresholdUpper, thresholdLower);
		}

	}
}

#endregion
