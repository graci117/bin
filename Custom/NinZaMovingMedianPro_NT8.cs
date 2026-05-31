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
		
		private ninZaMovingMedianPro[] cacheninZaMovingMedianPro;

		
		public ninZaMovingMedianPro ninZaMovingMedianPro(int period, bool inputSmoothingEnabled, ninZa_MAType inputSmoothingMethod, int inputSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return ninZaMovingMedianPro(Input, period, inputSmoothingEnabled, inputSmoothingMethod, inputSmoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public ninZaMovingMedianPro ninZaMovingMedianPro(ISeries<double> input, int period, bool inputSmoothingEnabled, ninZa_MAType inputSmoothingMethod, int inputSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaMovingMedianPro != null)
				for (int idx = 0; idx < cacheninZaMovingMedianPro.Length; idx++)
					if (cacheninZaMovingMedianPro[idx].Period == period && cacheninZaMovingMedianPro[idx].InputSmoothingEnabled == inputSmoothingEnabled && cacheninZaMovingMedianPro[idx].InputSmoothingMethod == inputSmoothingMethod && cacheninZaMovingMedianPro[idx].InputSmoothingPeriod == inputSmoothingPeriod && cacheninZaMovingMedianPro[idx].FilterEnabled == filterEnabled && cacheninZaMovingMedianPro[idx].FilterMultiplier == filterMultiplier && cacheninZaMovingMedianPro[idx].EqualsInput(input))
						return cacheninZaMovingMedianPro[idx];
			return CacheIndicator<ninZaMovingMedianPro>(new ninZaMovingMedianPro(){ Period = period, InputSmoothingEnabled = inputSmoothingEnabled, InputSmoothingMethod = inputSmoothingMethod, InputSmoothingPeriod = inputSmoothingPeriod, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaMovingMedianPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMovingMedianPro ninZaMovingMedianPro(int period, bool inputSmoothingEnabled, ninZa_MAType inputSmoothingMethod, int inputSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaMovingMedianPro(Input, period, inputSmoothingEnabled, inputSmoothingMethod, inputSmoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaMovingMedianPro ninZaMovingMedianPro(ISeries<double> input , int period, bool inputSmoothingEnabled, ninZa_MAType inputSmoothingMethod, int inputSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaMovingMedianPro(input, period, inputSmoothingEnabled, inputSmoothingMethod, inputSmoothingPeriod, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMovingMedianPro ninZaMovingMedianPro(int period, bool inputSmoothingEnabled, ninZa_MAType inputSmoothingMethod, int inputSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaMovingMedianPro(Input, period, inputSmoothingEnabled, inputSmoothingMethod, inputSmoothingPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaMovingMedianPro ninZaMovingMedianPro(ISeries<double> input , int period, bool inputSmoothingEnabled, ninZa_MAType inputSmoothingMethod, int inputSmoothingPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaMovingMedianPro(input, period, inputSmoothingEnabled, inputSmoothingMethod, inputSmoothingPeriod, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
