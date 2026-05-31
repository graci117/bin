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
		
		private ninZaiTrendPro[] cacheninZaiTrendPro;

		
		public ninZaiTrendPro ninZaiTrendPro(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double crossoverTolerance)
		{
			return ninZaiTrendPro(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, crossoverTolerance);
		}


		
		public ninZaiTrendPro ninZaiTrendPro(ISeries<double> input, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double crossoverTolerance)
		{
			if (cacheninZaiTrendPro != null)
				for (int idx = 0; idx < cacheninZaiTrendPro.Length; idx++)
					if (cacheninZaiTrendPro[idx].Period == period && cacheninZaiTrendPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaiTrendPro[idx].SmoothingMethod == smoothingMethod && cacheninZaiTrendPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaiTrendPro[idx].CrossoverTolerance == crossoverTolerance && cacheninZaiTrendPro[idx].EqualsInput(input))
						return cacheninZaiTrendPro[idx];
			return CacheIndicator<ninZaiTrendPro>(new ninZaiTrendPro(){ Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, CrossoverTolerance = crossoverTolerance }, input, ref cacheninZaiTrendPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaiTrendPro ninZaiTrendPro(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double crossoverTolerance)
		{
			return indicator.ninZaiTrendPro(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, crossoverTolerance);
		}


		
		public Indicators.ninZaiTrendPro ninZaiTrendPro(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double crossoverTolerance)
		{
			return indicator.ninZaiTrendPro(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, crossoverTolerance);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaiTrendPro ninZaiTrendPro(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double crossoverTolerance)
		{
			return indicator.ninZaiTrendPro(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, crossoverTolerance);
		}


		
		public Indicators.ninZaiTrendPro ninZaiTrendPro(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double crossoverTolerance)
		{
			return indicator.ninZaiTrendPro(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, crossoverTolerance);
		}

	}
}

#endregion
