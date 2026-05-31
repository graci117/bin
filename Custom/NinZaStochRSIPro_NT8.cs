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
		
		private ninZaStochRSIPro[] cacheninZaStochRSIPro;

		
		public ninZaStochRSIPro ninZaStochRSIPro(int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int rSIPeriod, int rSISmooth, ninZaStochRSIPro_RSIPlot rSIPlot, int thresholdOverbought, int thresholdOversold)
		{
			return ninZaStochRSIPro(Input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, rSIPeriod, rSISmooth, rSIPlot, thresholdOverbought, thresholdOversold);
		}


		
		public ninZaStochRSIPro ninZaStochRSIPro(ISeries<double> input, int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int rSIPeriod, int rSISmooth, ninZaStochRSIPro_RSIPlot rSIPlot, int thresholdOverbought, int thresholdOversold)
		{
			if (cacheninZaStochRSIPro != null)
				for (int idx = 0; idx < cacheninZaStochRSIPro.Length; idx++)
					if (cacheninZaStochRSIPro[idx].KPeriod == kPeriod && cacheninZaStochRSIPro[idx].KSmoothingEnabled == kSmoothingEnabled && cacheninZaStochRSIPro[idx].KSmoothingMethod == kSmoothingMethod && cacheninZaStochRSIPro[idx].KSmoothingPeriod == kSmoothingPeriod && cacheninZaStochRSIPro[idx].DMAType == dMAType && cacheninZaStochRSIPro[idx].DPeriod == dPeriod && cacheninZaStochRSIPro[idx].RSIPeriod == rSIPeriod && cacheninZaStochRSIPro[idx].RSISmooth == rSISmooth && cacheninZaStochRSIPro[idx].RSIPlot == rSIPlot && cacheninZaStochRSIPro[idx].ThresholdOverbought == thresholdOverbought && cacheninZaStochRSIPro[idx].ThresholdOversold == thresholdOversold && cacheninZaStochRSIPro[idx].EqualsInput(input))
						return cacheninZaStochRSIPro[idx];
			return CacheIndicator<ninZaStochRSIPro>(new ninZaStochRSIPro(){ KPeriod = kPeriod, KSmoothingEnabled = kSmoothingEnabled, KSmoothingMethod = kSmoothingMethod, KSmoothingPeriod = kSmoothingPeriod, DMAType = dMAType, DPeriod = dPeriod, RSIPeriod = rSIPeriod, RSISmooth = rSISmooth, RSIPlot = rSIPlot, ThresholdOverbought = thresholdOverbought, ThresholdOversold = thresholdOversold }, input, ref cacheninZaStochRSIPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaStochRSIPro ninZaStochRSIPro(int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int rSIPeriod, int rSISmooth, ninZaStochRSIPro_RSIPlot rSIPlot, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaStochRSIPro(Input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, rSIPeriod, rSISmooth, rSIPlot, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaStochRSIPro ninZaStochRSIPro(ISeries<double> input , int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int rSIPeriod, int rSISmooth, ninZaStochRSIPro_RSIPlot rSIPlot, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaStochRSIPro(input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, rSIPeriod, rSISmooth, rSIPlot, thresholdOverbought, thresholdOversold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaStochRSIPro ninZaStochRSIPro(int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int rSIPeriod, int rSISmooth, ninZaStochRSIPro_RSIPlot rSIPlot, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaStochRSIPro(Input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, rSIPeriod, rSISmooth, rSIPlot, thresholdOverbought, thresholdOversold);
		}


		
		public Indicators.ninZaStochRSIPro ninZaStochRSIPro(ISeries<double> input , int kPeriod, bool kSmoothingEnabled, ninZa_MAType kSmoothingMethod, int kSmoothingPeriod, ninZa_MAType dMAType, int dPeriod, int rSIPeriod, int rSISmooth, ninZaStochRSIPro_RSIPlot rSIPlot, int thresholdOverbought, int thresholdOversold)
		{
			return indicator.ninZaStochRSIPro(input, kPeriod, kSmoothingEnabled, kSmoothingMethod, kSmoothingPeriod, dMAType, dPeriod, rSIPeriod, rSISmooth, rSIPlot, thresholdOverbought, thresholdOversold);
		}

	}
}

#endregion
