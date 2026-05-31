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
		
		private ninZaBollingerPercentBPro[] cacheninZaBollingerPercentBPro;

		
		public ninZaBollingerPercentBPro ninZaBollingerPercentBPro(ninZa_MAType mAType, int period, ninZaBollingerPercentBPro_OffsetUnit offsetUnit, double offsetMultiplierTicks, double offsetMultiplierATR, int offsetATRPeriod, double offsetMultiplierStdDev, int offsetStdDevPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold, int signalResumeSplit)
		{
			return ninZaBollingerPercentBPro(Input, mAType, period, offsetUnit, offsetMultiplierTicks, offsetMultiplierATR, offsetATRPeriod, offsetMultiplierStdDev, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplit);
		}


		
		public ninZaBollingerPercentBPro ninZaBollingerPercentBPro(ISeries<double> input, ninZa_MAType mAType, int period, ninZaBollingerPercentBPro_OffsetUnit offsetUnit, double offsetMultiplierTicks, double offsetMultiplierATR, int offsetATRPeriod, double offsetMultiplierStdDev, int offsetStdDevPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold, int signalResumeSplit)
		{
			if (cacheninZaBollingerPercentBPro != null)
				for (int idx = 0; idx < cacheninZaBollingerPercentBPro.Length; idx++)
					if (cacheninZaBollingerPercentBPro[idx].MAType == mAType && cacheninZaBollingerPercentBPro[idx].Period == period && cacheninZaBollingerPercentBPro[idx].OffsetUnit == offsetUnit && cacheninZaBollingerPercentBPro[idx].OffsetMultiplierTicks == offsetMultiplierTicks && cacheninZaBollingerPercentBPro[idx].OffsetMultiplierATR == offsetMultiplierATR && cacheninZaBollingerPercentBPro[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaBollingerPercentBPro[idx].OffsetMultiplierStdDev == offsetMultiplierStdDev && cacheninZaBollingerPercentBPro[idx].OffsetStdDevPeriod == offsetStdDevPeriod && cacheninZaBollingerPercentBPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaBollingerPercentBPro[idx].SmoothingMethod == smoothingMethod && cacheninZaBollingerPercentBPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaBollingerPercentBPro[idx].ThresholdOverbought == thresholdOverbought && cacheninZaBollingerPercentBPro[idx].ThresholdOversold == thresholdOversold && cacheninZaBollingerPercentBPro[idx].SignalResumeSplit == signalResumeSplit && cacheninZaBollingerPercentBPro[idx].EqualsInput(input))
						return cacheninZaBollingerPercentBPro[idx];
			return CacheIndicator<ninZaBollingerPercentBPro>(new ninZaBollingerPercentBPro(){ MAType = mAType, Period = period, OffsetUnit = offsetUnit, OffsetMultiplierTicks = offsetMultiplierTicks, OffsetMultiplierATR = offsetMultiplierATR, OffsetATRPeriod = offsetATRPeriod, OffsetMultiplierStdDev = offsetMultiplierStdDev, OffsetStdDevPeriod = offsetStdDevPeriod, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold, SignalResumeSplit = signalResumeSplit }, input, ref cacheninZaBollingerPercentBPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaBollingerPercentBPro ninZaBollingerPercentBPro(ninZa_MAType mAType, int period, ninZaBollingerPercentBPro_OffsetUnit offsetUnit, double offsetMultiplierTicks, double offsetMultiplierATR, int offsetATRPeriod, double offsetMultiplierStdDev, int offsetStdDevPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold, int signalResumeSplit)
		{
			return indicator.ninZaBollingerPercentBPro(Input, mAType, period, offsetUnit, offsetMultiplierTicks, offsetMultiplierATR, offsetATRPeriod, offsetMultiplierStdDev, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplit);
		}


		
		public Indicators.ninZaBollingerPercentBPro ninZaBollingerPercentBPro(ISeries<double> input , ninZa_MAType mAType, int period, ninZaBollingerPercentBPro_OffsetUnit offsetUnit, double offsetMultiplierTicks, double offsetMultiplierATR, int offsetATRPeriod, double offsetMultiplierStdDev, int offsetStdDevPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold, int signalResumeSplit)
		{
			return indicator.ninZaBollingerPercentBPro(input, mAType, period, offsetUnit, offsetMultiplierTicks, offsetMultiplierATR, offsetATRPeriod, offsetMultiplierStdDev, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplit);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaBollingerPercentBPro ninZaBollingerPercentBPro(ninZa_MAType mAType, int period, ninZaBollingerPercentBPro_OffsetUnit offsetUnit, double offsetMultiplierTicks, double offsetMultiplierATR, int offsetATRPeriod, double offsetMultiplierStdDev, int offsetStdDevPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold, int signalResumeSplit)
		{
			return indicator.ninZaBollingerPercentBPro(Input, mAType, period, offsetUnit, offsetMultiplierTicks, offsetMultiplierATR, offsetATRPeriod, offsetMultiplierStdDev, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplit);
		}


		
		public Indicators.ninZaBollingerPercentBPro ninZaBollingerPercentBPro(ISeries<double> input , ninZa_MAType mAType, int period, ninZaBollingerPercentBPro_OffsetUnit offsetUnit, double offsetMultiplierTicks, double offsetMultiplierATR, int offsetATRPeriod, double offsetMultiplierStdDev, int offsetStdDevPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold, int signalResumeSplit)
		{
			return indicator.ninZaBollingerPercentBPro(input, mAType, period, offsetUnit, offsetMultiplierTicks, offsetMultiplierATR, offsetATRPeriod, offsetMultiplierStdDev, offsetStdDevPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold, signalResumeSplit);
		}

	}
}

#endregion
