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
		
		private ninZaTDIPro[] cacheninZaTDIPro;

		
		public ninZaTDIPro ninZaTDIPro(ninZaTDIPro_Mode trendMode, int rSIPeriod, ninZa_MAType primaryType, int primaryPeriod, bool primarySmoothingEnabled, ninZa_MAType primarySmoothingMethod, int primarySmoothingPeriod, ninZa_MAType secondaryType, int secondaryPeriod, bool secondarySmoothingEnabled, ninZa_MAType secondarySmoothingMethod, int secondarySmoothingPeriod, ninZa_MAType bollingerMiddleType, int bollingerMiddlePeriod, bool bollingerMiddleSmoothingEnabled, ninZa_MAType bollingerMiddleSmoothingMethod, int bollingerMiddleSmoothingPeriod, double standardDeviation)
		{
			return ninZaTDIPro(Input, trendMode, rSIPeriod, primaryType, primaryPeriod, primarySmoothingEnabled, primarySmoothingMethod, primarySmoothingPeriod, secondaryType, secondaryPeriod, secondarySmoothingEnabled, secondarySmoothingMethod, secondarySmoothingPeriod, bollingerMiddleType, bollingerMiddlePeriod, bollingerMiddleSmoothingEnabled, bollingerMiddleSmoothingMethod, bollingerMiddleSmoothingPeriod, standardDeviation);
		}


		
		public ninZaTDIPro ninZaTDIPro(ISeries<double> input, ninZaTDIPro_Mode trendMode, int rSIPeriod, ninZa_MAType primaryType, int primaryPeriod, bool primarySmoothingEnabled, ninZa_MAType primarySmoothingMethod, int primarySmoothingPeriod, ninZa_MAType secondaryType, int secondaryPeriod, bool secondarySmoothingEnabled, ninZa_MAType secondarySmoothingMethod, int secondarySmoothingPeriod, ninZa_MAType bollingerMiddleType, int bollingerMiddlePeriod, bool bollingerMiddleSmoothingEnabled, ninZa_MAType bollingerMiddleSmoothingMethod, int bollingerMiddleSmoothingPeriod, double standardDeviation)
		{
			if (cacheninZaTDIPro != null)
				for (int idx = 0; idx < cacheninZaTDIPro.Length; idx++)
					if (cacheninZaTDIPro[idx].TrendMode == trendMode && cacheninZaTDIPro[idx].RSIPeriod == rSIPeriod && cacheninZaTDIPro[idx].PrimaryType == primaryType && cacheninZaTDIPro[idx].PrimaryPeriod == primaryPeriod && cacheninZaTDIPro[idx].PrimarySmoothingEnabled == primarySmoothingEnabled && cacheninZaTDIPro[idx].PrimarySmoothingMethod == primarySmoothingMethod && cacheninZaTDIPro[idx].PrimarySmoothingPeriod == primarySmoothingPeriod && cacheninZaTDIPro[idx].SecondaryType == secondaryType && cacheninZaTDIPro[idx].SecondaryPeriod == secondaryPeriod && cacheninZaTDIPro[idx].SecondarySmoothingEnabled == secondarySmoothingEnabled && cacheninZaTDIPro[idx].SecondarySmoothingMethod == secondarySmoothingMethod && cacheninZaTDIPro[idx].SecondarySmoothingPeriod == secondarySmoothingPeriod && cacheninZaTDIPro[idx].BollingerMiddleType == bollingerMiddleType && cacheninZaTDIPro[idx].BollingerMiddlePeriod == bollingerMiddlePeriod && cacheninZaTDIPro[idx].BollingerMiddleSmoothingEnabled == bollingerMiddleSmoothingEnabled && cacheninZaTDIPro[idx].BollingerMiddleSmoothingMethod == bollingerMiddleSmoothingMethod && cacheninZaTDIPro[idx].BollingerMiddleSmoothingPeriod == bollingerMiddleSmoothingPeriod && cacheninZaTDIPro[idx].StandardDeviation == standardDeviation && cacheninZaTDIPro[idx].EqualsInput(input))
						return cacheninZaTDIPro[idx];
			return CacheIndicator<ninZaTDIPro>(new ninZaTDIPro(){ TrendMode = trendMode, RSIPeriod = rSIPeriod, PrimaryType = primaryType, PrimaryPeriod = primaryPeriod, PrimarySmoothingEnabled = primarySmoothingEnabled, PrimarySmoothingMethod = primarySmoothingMethod, PrimarySmoothingPeriod = primarySmoothingPeriod, SecondaryType = secondaryType, SecondaryPeriod = secondaryPeriod, SecondarySmoothingEnabled = secondarySmoothingEnabled, SecondarySmoothingMethod = secondarySmoothingMethod, SecondarySmoothingPeriod = secondarySmoothingPeriod, BollingerMiddleType = bollingerMiddleType, BollingerMiddlePeriod = bollingerMiddlePeriod, BollingerMiddleSmoothingEnabled = bollingerMiddleSmoothingEnabled, BollingerMiddleSmoothingMethod = bollingerMiddleSmoothingMethod, BollingerMiddleSmoothingPeriod = bollingerMiddleSmoothingPeriod, StandardDeviation = standardDeviation }, input, ref cacheninZaTDIPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaTDIPro ninZaTDIPro(ninZaTDIPro_Mode trendMode, int rSIPeriod, ninZa_MAType primaryType, int primaryPeriod, bool primarySmoothingEnabled, ninZa_MAType primarySmoothingMethod, int primarySmoothingPeriod, ninZa_MAType secondaryType, int secondaryPeriod, bool secondarySmoothingEnabled, ninZa_MAType secondarySmoothingMethod, int secondarySmoothingPeriod, ninZa_MAType bollingerMiddleType, int bollingerMiddlePeriod, bool bollingerMiddleSmoothingEnabled, ninZa_MAType bollingerMiddleSmoothingMethod, int bollingerMiddleSmoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaTDIPro(Input, trendMode, rSIPeriod, primaryType, primaryPeriod, primarySmoothingEnabled, primarySmoothingMethod, primarySmoothingPeriod, secondaryType, secondaryPeriod, secondarySmoothingEnabled, secondarySmoothingMethod, secondarySmoothingPeriod, bollingerMiddleType, bollingerMiddlePeriod, bollingerMiddleSmoothingEnabled, bollingerMiddleSmoothingMethod, bollingerMiddleSmoothingPeriod, standardDeviation);
		}


		
		public Indicators.ninZaTDIPro ninZaTDIPro(ISeries<double> input , ninZaTDIPro_Mode trendMode, int rSIPeriod, ninZa_MAType primaryType, int primaryPeriod, bool primarySmoothingEnabled, ninZa_MAType primarySmoothingMethod, int primarySmoothingPeriod, ninZa_MAType secondaryType, int secondaryPeriod, bool secondarySmoothingEnabled, ninZa_MAType secondarySmoothingMethod, int secondarySmoothingPeriod, ninZa_MAType bollingerMiddleType, int bollingerMiddlePeriod, bool bollingerMiddleSmoothingEnabled, ninZa_MAType bollingerMiddleSmoothingMethod, int bollingerMiddleSmoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaTDIPro(input, trendMode, rSIPeriod, primaryType, primaryPeriod, primarySmoothingEnabled, primarySmoothingMethod, primarySmoothingPeriod, secondaryType, secondaryPeriod, secondarySmoothingEnabled, secondarySmoothingMethod, secondarySmoothingPeriod, bollingerMiddleType, bollingerMiddlePeriod, bollingerMiddleSmoothingEnabled, bollingerMiddleSmoothingMethod, bollingerMiddleSmoothingPeriod, standardDeviation);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaTDIPro ninZaTDIPro(ninZaTDIPro_Mode trendMode, int rSIPeriod, ninZa_MAType primaryType, int primaryPeriod, bool primarySmoothingEnabled, ninZa_MAType primarySmoothingMethod, int primarySmoothingPeriod, ninZa_MAType secondaryType, int secondaryPeriod, bool secondarySmoothingEnabled, ninZa_MAType secondarySmoothingMethod, int secondarySmoothingPeriod, ninZa_MAType bollingerMiddleType, int bollingerMiddlePeriod, bool bollingerMiddleSmoothingEnabled, ninZa_MAType bollingerMiddleSmoothingMethod, int bollingerMiddleSmoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaTDIPro(Input, trendMode, rSIPeriod, primaryType, primaryPeriod, primarySmoothingEnabled, primarySmoothingMethod, primarySmoothingPeriod, secondaryType, secondaryPeriod, secondarySmoothingEnabled, secondarySmoothingMethod, secondarySmoothingPeriod, bollingerMiddleType, bollingerMiddlePeriod, bollingerMiddleSmoothingEnabled, bollingerMiddleSmoothingMethod, bollingerMiddleSmoothingPeriod, standardDeviation);
		}


		
		public Indicators.ninZaTDIPro ninZaTDIPro(ISeries<double> input , ninZaTDIPro_Mode trendMode, int rSIPeriod, ninZa_MAType primaryType, int primaryPeriod, bool primarySmoothingEnabled, ninZa_MAType primarySmoothingMethod, int primarySmoothingPeriod, ninZa_MAType secondaryType, int secondaryPeriod, bool secondarySmoothingEnabled, ninZa_MAType secondarySmoothingMethod, int secondarySmoothingPeriod, ninZa_MAType bollingerMiddleType, int bollingerMiddlePeriod, bool bollingerMiddleSmoothingEnabled, ninZa_MAType bollingerMiddleSmoothingMethod, int bollingerMiddleSmoothingPeriod, double standardDeviation)
		{
			return indicator.ninZaTDIPro(input, trendMode, rSIPeriod, primaryType, primaryPeriod, primarySmoothingEnabled, primarySmoothingMethod, primarySmoothingPeriod, secondaryType, secondaryPeriod, secondarySmoothingEnabled, secondarySmoothingMethod, secondarySmoothingPeriod, bollingerMiddleType, bollingerMiddlePeriod, bollingerMiddleSmoothingEnabled, bollingerMiddleSmoothingMethod, bollingerMiddleSmoothingPeriod, standardDeviation);
		}

	}
}

#endregion
