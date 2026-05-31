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
		
		private ninZaMurreyMathOscillatorPro[] cacheninZaMurreyMathOscillatorPro;

		
		public ninZaMurreyMathOscillatorPro ninZaMurreyMathOscillatorPro(bool strictModeEnabled, int period, bool priceSmoothingEnabled, ninZa_MAType priceSmoothingMethod, int priceSmoothingPeriod, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod)
		{
			return ninZaMurreyMathOscillatorPro(Input, strictModeEnabled, period, priceSmoothingEnabled, priceSmoothingMethod, priceSmoothingPeriod, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod);
		}


		
		public ninZaMurreyMathOscillatorPro ninZaMurreyMathOscillatorPro(ISeries<double> input, bool strictModeEnabled, int period, bool priceSmoothingEnabled, ninZa_MAType priceSmoothingMethod, int priceSmoothingPeriod, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod)
		{
			if (cacheninZaMurreyMathOscillatorPro != null)
				for (int idx = 0; idx < cacheninZaMurreyMathOscillatorPro.Length; idx++)
					if (cacheninZaMurreyMathOscillatorPro[idx].StrictModeEnabled == strictModeEnabled && cacheninZaMurreyMathOscillatorPro[idx].Period == period && cacheninZaMurreyMathOscillatorPro[idx].PriceSmoothingEnabled == priceSmoothingEnabled && cacheninZaMurreyMathOscillatorPro[idx].PriceSmoothingMethod == priceSmoothingMethod && cacheninZaMurreyMathOscillatorPro[idx].PriceSmoothingPeriod == priceSmoothingPeriod && cacheninZaMurreyMathOscillatorPro[idx].MAType == mAType && cacheninZaMurreyMathOscillatorPro[idx].MAPeriod == mAPeriod && cacheninZaMurreyMathOscillatorPro[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaMurreyMathOscillatorPro[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaMurreyMathOscillatorPro[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaMurreyMathOscillatorPro[idx].EqualsInput(input))
						return cacheninZaMurreyMathOscillatorPro[idx];
			return CacheIndicator<ninZaMurreyMathOscillatorPro>(new ninZaMurreyMathOscillatorPro(){ StrictModeEnabled = strictModeEnabled, Period = period, PriceSmoothingEnabled = priceSmoothingEnabled, PriceSmoothingMethod = priceSmoothingMethod, PriceSmoothingPeriod = priceSmoothingPeriod, MAType = mAType, MAPeriod = mAPeriod, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod }, input, ref cacheninZaMurreyMathOscillatorPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMurreyMathOscillatorPro ninZaMurreyMathOscillatorPro(bool strictModeEnabled, int period, bool priceSmoothingEnabled, ninZa_MAType priceSmoothingMethod, int priceSmoothingPeriod, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod)
		{
			return indicator.ninZaMurreyMathOscillatorPro(Input, strictModeEnabled, period, priceSmoothingEnabled, priceSmoothingMethod, priceSmoothingPeriod, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod);
		}


		
		public Indicators.ninZaMurreyMathOscillatorPro ninZaMurreyMathOscillatorPro(ISeries<double> input , bool strictModeEnabled, int period, bool priceSmoothingEnabled, ninZa_MAType priceSmoothingMethod, int priceSmoothingPeriod, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod)
		{
			return indicator.ninZaMurreyMathOscillatorPro(input, strictModeEnabled, period, priceSmoothingEnabled, priceSmoothingMethod, priceSmoothingPeriod, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMurreyMathOscillatorPro ninZaMurreyMathOscillatorPro(bool strictModeEnabled, int period, bool priceSmoothingEnabled, ninZa_MAType priceSmoothingMethod, int priceSmoothingPeriod, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod)
		{
			return indicator.ninZaMurreyMathOscillatorPro(Input, strictModeEnabled, period, priceSmoothingEnabled, priceSmoothingMethod, priceSmoothingPeriod, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod);
		}


		
		public Indicators.ninZaMurreyMathOscillatorPro ninZaMurreyMathOscillatorPro(ISeries<double> input , bool strictModeEnabled, int period, bool priceSmoothingEnabled, ninZa_MAType priceSmoothingMethod, int priceSmoothingPeriod, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod)
		{
			return indicator.ninZaMurreyMathOscillatorPro(input, strictModeEnabled, period, priceSmoothingEnabled, priceSmoothingMethod, priceSmoothingPeriod, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod);
		}

	}
}

#endregion
