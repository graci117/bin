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
		
		private ninZaGannHiLoActivatorPro[] cacheninZaGannHiLoActivatorPro;

		
		public ninZaGannHiLoActivatorPro ninZaGannHiLoActivatorPro(int period, ninZaGannHiLoActivatorPro_CalculationMethod calculationMethod, ninZa_MAType calculationMAType, double hiLoOffsetMultiplier, bool hiLoSmoothingEnabled, ninZa_MAType hiLoSmoothingMethod, int hiLoSmoothingPeriod)
		{
			return ninZaGannHiLoActivatorPro(Input, period, calculationMethod, calculationMAType, hiLoOffsetMultiplier, hiLoSmoothingEnabled, hiLoSmoothingMethod, hiLoSmoothingPeriod);
		}


		
		public ninZaGannHiLoActivatorPro ninZaGannHiLoActivatorPro(ISeries<double> input, int period, ninZaGannHiLoActivatorPro_CalculationMethod calculationMethod, ninZa_MAType calculationMAType, double hiLoOffsetMultiplier, bool hiLoSmoothingEnabled, ninZa_MAType hiLoSmoothingMethod, int hiLoSmoothingPeriod)
		{
			if (cacheninZaGannHiLoActivatorPro != null)
				for (int idx = 0; idx < cacheninZaGannHiLoActivatorPro.Length; idx++)
					if (cacheninZaGannHiLoActivatorPro[idx].Period == period && cacheninZaGannHiLoActivatorPro[idx].CalculationMethod == calculationMethod && cacheninZaGannHiLoActivatorPro[idx].CalculationMAType == calculationMAType && cacheninZaGannHiLoActivatorPro[idx].HiLoOffsetMultiplier == hiLoOffsetMultiplier && cacheninZaGannHiLoActivatorPro[idx].HiLoSmoothingEnabled == hiLoSmoothingEnabled && cacheninZaGannHiLoActivatorPro[idx].HiLoSmoothingMethod == hiLoSmoothingMethod && cacheninZaGannHiLoActivatorPro[idx].HiLoSmoothingPeriod == hiLoSmoothingPeriod && cacheninZaGannHiLoActivatorPro[idx].EqualsInput(input))
						return cacheninZaGannHiLoActivatorPro[idx];
			return CacheIndicator<ninZaGannHiLoActivatorPro>(new ninZaGannHiLoActivatorPro(){ Period = period, CalculationMethod = calculationMethod, CalculationMAType = calculationMAType, HiLoOffsetMultiplier = hiLoOffsetMultiplier, HiLoSmoothingEnabled = hiLoSmoothingEnabled, HiLoSmoothingMethod = hiLoSmoothingMethod, HiLoSmoothingPeriod = hiLoSmoothingPeriod }, input, ref cacheninZaGannHiLoActivatorPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaGannHiLoActivatorPro ninZaGannHiLoActivatorPro(int period, ninZaGannHiLoActivatorPro_CalculationMethod calculationMethod, ninZa_MAType calculationMAType, double hiLoOffsetMultiplier, bool hiLoSmoothingEnabled, ninZa_MAType hiLoSmoothingMethod, int hiLoSmoothingPeriod)
		{
			return indicator.ninZaGannHiLoActivatorPro(Input, period, calculationMethod, calculationMAType, hiLoOffsetMultiplier, hiLoSmoothingEnabled, hiLoSmoothingMethod, hiLoSmoothingPeriod);
		}


		
		public Indicators.ninZaGannHiLoActivatorPro ninZaGannHiLoActivatorPro(ISeries<double> input , int period, ninZaGannHiLoActivatorPro_CalculationMethod calculationMethod, ninZa_MAType calculationMAType, double hiLoOffsetMultiplier, bool hiLoSmoothingEnabled, ninZa_MAType hiLoSmoothingMethod, int hiLoSmoothingPeriod)
		{
			return indicator.ninZaGannHiLoActivatorPro(input, period, calculationMethod, calculationMAType, hiLoOffsetMultiplier, hiLoSmoothingEnabled, hiLoSmoothingMethod, hiLoSmoothingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaGannHiLoActivatorPro ninZaGannHiLoActivatorPro(int period, ninZaGannHiLoActivatorPro_CalculationMethod calculationMethod, ninZa_MAType calculationMAType, double hiLoOffsetMultiplier, bool hiLoSmoothingEnabled, ninZa_MAType hiLoSmoothingMethod, int hiLoSmoothingPeriod)
		{
			return indicator.ninZaGannHiLoActivatorPro(Input, period, calculationMethod, calculationMAType, hiLoOffsetMultiplier, hiLoSmoothingEnabled, hiLoSmoothingMethod, hiLoSmoothingPeriod);
		}


		
		public Indicators.ninZaGannHiLoActivatorPro ninZaGannHiLoActivatorPro(ISeries<double> input , int period, ninZaGannHiLoActivatorPro_CalculationMethod calculationMethod, ninZa_MAType calculationMAType, double hiLoOffsetMultiplier, bool hiLoSmoothingEnabled, ninZa_MAType hiLoSmoothingMethod, int hiLoSmoothingPeriod)
		{
			return indicator.ninZaGannHiLoActivatorPro(input, period, calculationMethod, calculationMAType, hiLoOffsetMultiplier, hiLoSmoothingEnabled, hiLoSmoothingMethod, hiLoSmoothingPeriod);
		}

	}
}

#endregion
