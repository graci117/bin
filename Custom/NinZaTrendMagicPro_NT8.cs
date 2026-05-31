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
		
		private ninZaTrendMagicPro[] cacheninZaTrendMagicPro;

		
		public ninZaTrendMagicPro ninZaTrendMagicPro(int cCIPeriod, double cCIMultiplier, ninZa_MAType cCIMAType, bool cCIMASmoothingEnabled, ninZa_MAType cCIMASmoothingMethod, int cCIMASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double aTRMultiplier, int aTRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return ninZaTrendMagicPro(Input, cCIPeriod, cCIMultiplier, cCIMAType, cCIMASmoothingEnabled, cCIMASmoothingMethod, cCIMASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, aTRMultiplier, aTRPeriod, filterEnabled, filterMultiplier);
		}


		
		public ninZaTrendMagicPro ninZaTrendMagicPro(ISeries<double> input, int cCIPeriod, double cCIMultiplier, ninZa_MAType cCIMAType, bool cCIMASmoothingEnabled, ninZa_MAType cCIMASmoothingMethod, int cCIMASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double aTRMultiplier, int aTRPeriod, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaTrendMagicPro != null)
				for (int idx = 0; idx < cacheninZaTrendMagicPro.Length; idx++)
					if (cacheninZaTrendMagicPro[idx].CCIPeriod == cCIPeriod && cacheninZaTrendMagicPro[idx].CCIMultiplier == cCIMultiplier && cacheninZaTrendMagicPro[idx].CCIMAType == cCIMAType && cacheninZaTrendMagicPro[idx].CCIMASmoothingEnabled == cCIMASmoothingEnabled && cacheninZaTrendMagicPro[idx].CCIMASmoothingMethod == cCIMASmoothingMethod && cacheninZaTrendMagicPro[idx].CCIMASmoothingPeriod == cCIMASmoothingPeriod && cacheninZaTrendMagicPro[idx].CCISmoothingEnabled == cCISmoothingEnabled && cacheninZaTrendMagicPro[idx].CCISmoothingMethod == cCISmoothingMethod && cacheninZaTrendMagicPro[idx].CCISmoothingPeriod == cCISmoothingPeriod && cacheninZaTrendMagicPro[idx].ATRMultiplier == aTRMultiplier && cacheninZaTrendMagicPro[idx].ATRPeriod == aTRPeriod && cacheninZaTrendMagicPro[idx].FilterEnabled == filterEnabled && cacheninZaTrendMagicPro[idx].FilterMultiplier == filterMultiplier && cacheninZaTrendMagicPro[idx].EqualsInput(input))
						return cacheninZaTrendMagicPro[idx];
			return CacheIndicator<ninZaTrendMagicPro>(new ninZaTrendMagicPro(){ CCIPeriod = cCIPeriod, CCIMultiplier = cCIMultiplier, CCIMAType = cCIMAType, CCIMASmoothingEnabled = cCIMASmoothingEnabled, CCIMASmoothingMethod = cCIMASmoothingMethod, CCIMASmoothingPeriod = cCIMASmoothingPeriod, CCISmoothingEnabled = cCISmoothingEnabled, CCISmoothingMethod = cCISmoothingMethod, CCISmoothingPeriod = cCISmoothingPeriod, ATRMultiplier = aTRMultiplier, ATRPeriod = aTRPeriod, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaTrendMagicPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaTrendMagicPro ninZaTrendMagicPro(int cCIPeriod, double cCIMultiplier, ninZa_MAType cCIMAType, bool cCIMASmoothingEnabled, ninZa_MAType cCIMASmoothingMethod, int cCIMASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double aTRMultiplier, int aTRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTrendMagicPro(Input, cCIPeriod, cCIMultiplier, cCIMAType, cCIMASmoothingEnabled, cCIMASmoothingMethod, cCIMASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, aTRMultiplier, aTRPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaTrendMagicPro ninZaTrendMagicPro(ISeries<double> input , int cCIPeriod, double cCIMultiplier, ninZa_MAType cCIMAType, bool cCIMASmoothingEnabled, ninZa_MAType cCIMASmoothingMethod, int cCIMASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double aTRMultiplier, int aTRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTrendMagicPro(input, cCIPeriod, cCIMultiplier, cCIMAType, cCIMASmoothingEnabled, cCIMASmoothingMethod, cCIMASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, aTRMultiplier, aTRPeriod, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaTrendMagicPro ninZaTrendMagicPro(int cCIPeriod, double cCIMultiplier, ninZa_MAType cCIMAType, bool cCIMASmoothingEnabled, ninZa_MAType cCIMASmoothingMethod, int cCIMASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double aTRMultiplier, int aTRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTrendMagicPro(Input, cCIPeriod, cCIMultiplier, cCIMAType, cCIMASmoothingEnabled, cCIMASmoothingMethod, cCIMASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, aTRMultiplier, aTRPeriod, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaTrendMagicPro ninZaTrendMagicPro(ISeries<double> input , int cCIPeriod, double cCIMultiplier, ninZa_MAType cCIMAType, bool cCIMASmoothingEnabled, ninZa_MAType cCIMASmoothingMethod, int cCIMASmoothingPeriod, bool cCISmoothingEnabled, ninZa_MAType cCISmoothingMethod, int cCISmoothingPeriod, double aTRMultiplier, int aTRPeriod, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTrendMagicPro(input, cCIPeriod, cCIMultiplier, cCIMAType, cCIMASmoothingEnabled, cCIMASmoothingMethod, cCIMASmoothingPeriod, cCISmoothingEnabled, cCISmoothingMethod, cCISmoothingPeriod, aTRMultiplier, aTRPeriod, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
