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
		
		private ninZaSuperiorADXDMI[] cacheninZaSuperiorADXDMI;

		
		public ninZaSuperiorADXDMI ninZaSuperiorADXDMI(int period, bool aDXSmoothingEnabled, ninZa_MAType aDXSmoothingMethod, int aDXSmoothingPeriod, bool dMISmoothingEnabled, ninZa_MAType dMISmoothingMethod, int dMISmoothingPeriod, int dMICrossoverTolerance, int thresholdHigh, int thresholdMid)
		{
			return ninZaSuperiorADXDMI(Input, period, aDXSmoothingEnabled, aDXSmoothingMethod, aDXSmoothingPeriod, dMISmoothingEnabled, dMISmoothingMethod, dMISmoothingPeriod, dMICrossoverTolerance, thresholdHigh, thresholdMid);
		}


		
		public ninZaSuperiorADXDMI ninZaSuperiorADXDMI(ISeries<double> input, int period, bool aDXSmoothingEnabled, ninZa_MAType aDXSmoothingMethod, int aDXSmoothingPeriod, bool dMISmoothingEnabled, ninZa_MAType dMISmoothingMethod, int dMISmoothingPeriod, int dMICrossoverTolerance, int thresholdHigh, int thresholdMid)
		{
			if (cacheninZaSuperiorADXDMI != null)
				for (int idx = 0; idx < cacheninZaSuperiorADXDMI.Length; idx++)
					if (cacheninZaSuperiorADXDMI[idx].Period == period && cacheninZaSuperiorADXDMI[idx].ADXSmoothingEnabled == aDXSmoothingEnabled && cacheninZaSuperiorADXDMI[idx].ADXSmoothingMethod == aDXSmoothingMethod && cacheninZaSuperiorADXDMI[idx].ADXSmoothingPeriod == aDXSmoothingPeriod && cacheninZaSuperiorADXDMI[idx].DMISmoothingEnabled == dMISmoothingEnabled && cacheninZaSuperiorADXDMI[idx].DMISmoothingMethod == dMISmoothingMethod && cacheninZaSuperiorADXDMI[idx].DMISmoothingPeriod == dMISmoothingPeriod && cacheninZaSuperiorADXDMI[idx].DMICrossoverTolerance == dMICrossoverTolerance && cacheninZaSuperiorADXDMI[idx].ThresholdHigh == thresholdHigh && cacheninZaSuperiorADXDMI[idx].ThresholdMid == thresholdMid && cacheninZaSuperiorADXDMI[idx].EqualsInput(input))
						return cacheninZaSuperiorADXDMI[idx];
			return CacheIndicator<ninZaSuperiorADXDMI>(new ninZaSuperiorADXDMI(){ Period = period, ADXSmoothingEnabled = aDXSmoothingEnabled, ADXSmoothingMethod = aDXSmoothingMethod, ADXSmoothingPeriod = aDXSmoothingPeriod, DMISmoothingEnabled = dMISmoothingEnabled, DMISmoothingMethod = dMISmoothingMethod, DMISmoothingPeriod = dMISmoothingPeriod, DMICrossoverTolerance = dMICrossoverTolerance, ThresholdHigh = thresholdHigh, ThresholdMid = thresholdMid }, input, ref cacheninZaSuperiorADXDMI);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorADXDMI ninZaSuperiorADXDMI(int period, bool aDXSmoothingEnabled, ninZa_MAType aDXSmoothingMethod, int aDXSmoothingPeriod, bool dMISmoothingEnabled, ninZa_MAType dMISmoothingMethod, int dMISmoothingPeriod, int dMICrossoverTolerance, int thresholdHigh, int thresholdMid)
		{
			return indicator.ninZaSuperiorADXDMI(Input, period, aDXSmoothingEnabled, aDXSmoothingMethod, aDXSmoothingPeriod, dMISmoothingEnabled, dMISmoothingMethod, dMISmoothingPeriod, dMICrossoverTolerance, thresholdHigh, thresholdMid);
		}


		
		public Indicators.ninZaSuperiorADXDMI ninZaSuperiorADXDMI(ISeries<double> input , int period, bool aDXSmoothingEnabled, ninZa_MAType aDXSmoothingMethod, int aDXSmoothingPeriod, bool dMISmoothingEnabled, ninZa_MAType dMISmoothingMethod, int dMISmoothingPeriod, int dMICrossoverTolerance, int thresholdHigh, int thresholdMid)
		{
			return indicator.ninZaSuperiorADXDMI(input, period, aDXSmoothingEnabled, aDXSmoothingMethod, aDXSmoothingPeriod, dMISmoothingEnabled, dMISmoothingMethod, dMISmoothingPeriod, dMICrossoverTolerance, thresholdHigh, thresholdMid);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorADXDMI ninZaSuperiorADXDMI(int period, bool aDXSmoothingEnabled, ninZa_MAType aDXSmoothingMethod, int aDXSmoothingPeriod, bool dMISmoothingEnabled, ninZa_MAType dMISmoothingMethod, int dMISmoothingPeriod, int dMICrossoverTolerance, int thresholdHigh, int thresholdMid)
		{
			return indicator.ninZaSuperiorADXDMI(Input, period, aDXSmoothingEnabled, aDXSmoothingMethod, aDXSmoothingPeriod, dMISmoothingEnabled, dMISmoothingMethod, dMISmoothingPeriod, dMICrossoverTolerance, thresholdHigh, thresholdMid);
		}


		
		public Indicators.ninZaSuperiorADXDMI ninZaSuperiorADXDMI(ISeries<double> input , int period, bool aDXSmoothingEnabled, ninZa_MAType aDXSmoothingMethod, int aDXSmoothingPeriod, bool dMISmoothingEnabled, ninZa_MAType dMISmoothingMethod, int dMISmoothingPeriod, int dMICrossoverTolerance, int thresholdHigh, int thresholdMid)
		{
			return indicator.ninZaSuperiorADXDMI(input, period, aDXSmoothingEnabled, aDXSmoothingMethod, aDXSmoothingPeriod, dMISmoothingEnabled, dMISmoothingMethod, dMISmoothingPeriod, dMICrossoverTolerance, thresholdHigh, thresholdMid);
		}

	}
}

#endregion
