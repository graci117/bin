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
		
		private ninZaSuperiorRSI[] cacheninZaSuperiorRSI;

		
		public ninZaSuperiorRSI ninZaSuperiorRSI(int rSIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaSuperiorRSI(Input, rSIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaSuperiorRSI ninZaSuperiorRSI(ISeries<double> input, int rSIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaSuperiorRSI != null)
				for (int idx = 0; idx < cacheninZaSuperiorRSI.Length; idx++)
					if (cacheninZaSuperiorRSI[idx].RSIPeriod == rSIPeriod && cacheninZaSuperiorRSI[idx].SmoothingEnabled == smoothingEnabled && cacheninZaSuperiorRSI[idx].SmoothingMethod == smoothingMethod && cacheninZaSuperiorRSI[idx].SmoothingPeriod == smoothingPeriod && cacheninZaSuperiorRSI[idx].ThresholdOverbought == thresholdOverbought && cacheninZaSuperiorRSI[idx].ThresholdOversold == thresholdOversold && cacheninZaSuperiorRSI[idx].EqualsInput(input))
						return cacheninZaSuperiorRSI[idx];
			return CacheIndicator<ninZaSuperiorRSI>(new ninZaSuperiorRSI(){ RSIPeriod = rSIPeriod, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaSuperiorRSI);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorRSI ninZaSuperiorRSI(int rSIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorRSI(Input, rSIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaSuperiorRSI ninZaSuperiorRSI(ISeries<double> input , int rSIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorRSI(input, rSIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorRSI ninZaSuperiorRSI(int rSIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorRSI(Input, rSIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaSuperiorRSI ninZaSuperiorRSI(ISeries<double> input , int rSIPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaSuperiorRSI(input, rSIPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
