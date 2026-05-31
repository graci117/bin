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
		
		private ninZaMarketSpeed[] cacheninZaMarketSpeed;

		
		public ninZaMarketSpeed ninZaMarketSpeed(ninZaMarketSpeed_Mode mode, ninZaMarketSpeed_TimeUnit timeUnit, ninZaMarketSpeed_DisplacementUnit displacementUnit, bool displacementAbsolute, int extremeSpeedCapping, ninZa_MAType averageMethod, int averagePeriod)
		{
			return ninZaMarketSpeed(Input, mode, timeUnit, displacementUnit, displacementAbsolute, extremeSpeedCapping, averageMethod, averagePeriod);
		}


		
		public ninZaMarketSpeed ninZaMarketSpeed(ISeries<double> input, ninZaMarketSpeed_Mode mode, ninZaMarketSpeed_TimeUnit timeUnit, ninZaMarketSpeed_DisplacementUnit displacementUnit, bool displacementAbsolute, int extremeSpeedCapping, ninZa_MAType averageMethod, int averagePeriod)
		{
			if (cacheninZaMarketSpeed != null)
				for (int idx = 0; idx < cacheninZaMarketSpeed.Length; idx++)
					if (cacheninZaMarketSpeed[idx].Mode == mode && cacheninZaMarketSpeed[idx].TimeUnit == timeUnit && cacheninZaMarketSpeed[idx].DisplacementUnit == displacementUnit && cacheninZaMarketSpeed[idx].DisplacementAbsolute == displacementAbsolute && cacheninZaMarketSpeed[idx].ExtremeSpeedCapping == extremeSpeedCapping && cacheninZaMarketSpeed[idx].AverageMethod == averageMethod && cacheninZaMarketSpeed[idx].AveragePeriod == averagePeriod && cacheninZaMarketSpeed[idx].EqualsInput(input))
						return cacheninZaMarketSpeed[idx];
			return CacheIndicator<ninZaMarketSpeed>(new ninZaMarketSpeed(){ Mode = mode, TimeUnit = timeUnit, DisplacementUnit = displacementUnit, DisplacementAbsolute = displacementAbsolute, ExtremeSpeedCapping = extremeSpeedCapping, AverageMethod = averageMethod, AveragePeriod = averagePeriod }, input, ref cacheninZaMarketSpeed);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMarketSpeed ninZaMarketSpeed(ninZaMarketSpeed_Mode mode, ninZaMarketSpeed_TimeUnit timeUnit, ninZaMarketSpeed_DisplacementUnit displacementUnit, bool displacementAbsolute, int extremeSpeedCapping, ninZa_MAType averageMethod, int averagePeriod)
		{
			return indicator.ninZaMarketSpeed(Input, mode, timeUnit, displacementUnit, displacementAbsolute, extremeSpeedCapping, averageMethod, averagePeriod);
		}


		
		public Indicators.ninZaMarketSpeed ninZaMarketSpeed(ISeries<double> input , ninZaMarketSpeed_Mode mode, ninZaMarketSpeed_TimeUnit timeUnit, ninZaMarketSpeed_DisplacementUnit displacementUnit, bool displacementAbsolute, int extremeSpeedCapping, ninZa_MAType averageMethod, int averagePeriod)
		{
			return indicator.ninZaMarketSpeed(input, mode, timeUnit, displacementUnit, displacementAbsolute, extremeSpeedCapping, averageMethod, averagePeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMarketSpeed ninZaMarketSpeed(ninZaMarketSpeed_Mode mode, ninZaMarketSpeed_TimeUnit timeUnit, ninZaMarketSpeed_DisplacementUnit displacementUnit, bool displacementAbsolute, int extremeSpeedCapping, ninZa_MAType averageMethod, int averagePeriod)
		{
			return indicator.ninZaMarketSpeed(Input, mode, timeUnit, displacementUnit, displacementAbsolute, extremeSpeedCapping, averageMethod, averagePeriod);
		}


		
		public Indicators.ninZaMarketSpeed ninZaMarketSpeed(ISeries<double> input , ninZaMarketSpeed_Mode mode, ninZaMarketSpeed_TimeUnit timeUnit, ninZaMarketSpeed_DisplacementUnit displacementUnit, bool displacementAbsolute, int extremeSpeedCapping, ninZa_MAType averageMethod, int averagePeriod)
		{
			return indicator.ninZaMarketSpeed(input, mode, timeUnit, displacementUnit, displacementAbsolute, extremeSpeedCapping, averageMethod, averagePeriod);
		}

	}
}

#endregion
