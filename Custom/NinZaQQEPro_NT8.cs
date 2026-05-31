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
		
		private ninZaQQEPro[] cacheninZaQQEPro;

		
		public ninZaQQEPro ninZaQQEPro(ninZaQQEPro_Mode trendMode, int rSIPeriod, bool qQESmoothingEnabled, ninZa_MAType qQESmoothingMethod, int qQESmoothingPeriod, double offsetFactor, bool offsetSmoothingEnabled, ninZa_MAType offsetSmoothingMethod, int offsetSmoothingPeriod, int thresholdUpper, int thresholdLower)
		{
			return ninZaQQEPro(Input, trendMode, rSIPeriod, qQESmoothingEnabled, qQESmoothingMethod, qQESmoothingPeriod, offsetFactor, offsetSmoothingEnabled, offsetSmoothingMethod, offsetSmoothingPeriod, thresholdUpper, thresholdLower);
		}


		
		public ninZaQQEPro ninZaQQEPro(ISeries<double> input, ninZaQQEPro_Mode trendMode, int rSIPeriod, bool qQESmoothingEnabled, ninZa_MAType qQESmoothingMethod, int qQESmoothingPeriod, double offsetFactor, bool offsetSmoothingEnabled, ninZa_MAType offsetSmoothingMethod, int offsetSmoothingPeriod, int thresholdUpper, int thresholdLower)
		{
			if (cacheninZaQQEPro != null)
				for (int idx = 0; idx < cacheninZaQQEPro.Length; idx++)
					if (cacheninZaQQEPro[idx].TrendMode == trendMode && cacheninZaQQEPro[idx].RSIPeriod == rSIPeriod && cacheninZaQQEPro[idx].QQESmoothingEnabled == qQESmoothingEnabled && cacheninZaQQEPro[idx].QQESmoothingMethod == qQESmoothingMethod && cacheninZaQQEPro[idx].QQESmoothingPeriod == qQESmoothingPeriod && cacheninZaQQEPro[idx].OffsetFactor == offsetFactor && cacheninZaQQEPro[idx].OffsetSmoothingEnabled == offsetSmoothingEnabled && cacheninZaQQEPro[idx].OffsetSmoothingMethod == offsetSmoothingMethod && cacheninZaQQEPro[idx].OffsetSmoothingPeriod == offsetSmoothingPeriod && cacheninZaQQEPro[idx].ThresholdUpper == thresholdUpper && cacheninZaQQEPro[idx].ThresholdLower == thresholdLower && cacheninZaQQEPro[idx].EqualsInput(input))
						return cacheninZaQQEPro[idx];
			return CacheIndicator<ninZaQQEPro>(new ninZaQQEPro(){ TrendMode = trendMode, RSIPeriod = rSIPeriod, QQESmoothingEnabled = qQESmoothingEnabled, QQESmoothingMethod = qQESmoothingMethod, QQESmoothingPeriod = qQESmoothingPeriod, OffsetFactor = offsetFactor, OffsetSmoothingEnabled = offsetSmoothingEnabled, OffsetSmoothingMethod = offsetSmoothingMethod, OffsetSmoothingPeriod = offsetSmoothingPeriod, ThresholdUpper = thresholdUpper, ThresholdLower = thresholdLower }, input, ref cacheninZaQQEPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaQQEPro ninZaQQEPro(ninZaQQEPro_Mode trendMode, int rSIPeriod, bool qQESmoothingEnabled, ninZa_MAType qQESmoothingMethod, int qQESmoothingPeriod, double offsetFactor, bool offsetSmoothingEnabled, ninZa_MAType offsetSmoothingMethod, int offsetSmoothingPeriod, int thresholdUpper, int thresholdLower)
		{
			return indicator.ninZaQQEPro(Input, trendMode, rSIPeriod, qQESmoothingEnabled, qQESmoothingMethod, qQESmoothingPeriod, offsetFactor, offsetSmoothingEnabled, offsetSmoothingMethod, offsetSmoothingPeriod, thresholdUpper, thresholdLower);
		}


		
		public Indicators.ninZaQQEPro ninZaQQEPro(ISeries<double> input , ninZaQQEPro_Mode trendMode, int rSIPeriod, bool qQESmoothingEnabled, ninZa_MAType qQESmoothingMethod, int qQESmoothingPeriod, double offsetFactor, bool offsetSmoothingEnabled, ninZa_MAType offsetSmoothingMethod, int offsetSmoothingPeriod, int thresholdUpper, int thresholdLower)
		{
			return indicator.ninZaQQEPro(input, trendMode, rSIPeriod, qQESmoothingEnabled, qQESmoothingMethod, qQESmoothingPeriod, offsetFactor, offsetSmoothingEnabled, offsetSmoothingMethod, offsetSmoothingPeriod, thresholdUpper, thresholdLower);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaQQEPro ninZaQQEPro(ninZaQQEPro_Mode trendMode, int rSIPeriod, bool qQESmoothingEnabled, ninZa_MAType qQESmoothingMethod, int qQESmoothingPeriod, double offsetFactor, bool offsetSmoothingEnabled, ninZa_MAType offsetSmoothingMethod, int offsetSmoothingPeriod, int thresholdUpper, int thresholdLower)
		{
			return indicator.ninZaQQEPro(Input, trendMode, rSIPeriod, qQESmoothingEnabled, qQESmoothingMethod, qQESmoothingPeriod, offsetFactor, offsetSmoothingEnabled, offsetSmoothingMethod, offsetSmoothingPeriod, thresholdUpper, thresholdLower);
		}


		
		public Indicators.ninZaQQEPro ninZaQQEPro(ISeries<double> input , ninZaQQEPro_Mode trendMode, int rSIPeriod, bool qQESmoothingEnabled, ninZa_MAType qQESmoothingMethod, int qQESmoothingPeriod, double offsetFactor, bool offsetSmoothingEnabled, ninZa_MAType offsetSmoothingMethod, int offsetSmoothingPeriod, int thresholdUpper, int thresholdLower)
		{
			return indicator.ninZaQQEPro(input, trendMode, rSIPeriod, qQESmoothingEnabled, qQESmoothingMethod, qQESmoothingPeriod, offsetFactor, offsetSmoothingEnabled, offsetSmoothingMethod, offsetSmoothingPeriod, thresholdUpper, thresholdLower);
		}

	}
}

#endregion
