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
		
		private ninZaSuperTrendPro[] cacheninZaSuperTrendPro;

		
		public ninZaSuperTrendPro ninZaSuperTrendPro(ninZa_MAType mAType, PriceType mAInputUptrend, PriceType mAInputDowntrend, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return ninZaSuperTrendPro(Input, mAType, mAInputUptrend, mAInputDowntrend, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}


		
		public ninZaSuperTrendPro ninZaSuperTrendPro(ISeries<double> input, ninZa_MAType mAType, PriceType mAInputUptrend, PriceType mAInputDowntrend, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			if (cacheninZaSuperTrendPro != null)
				for (int idx = 0; idx < cacheninZaSuperTrendPro.Length; idx++)
					if (cacheninZaSuperTrendPro[idx].MAType == mAType && cacheninZaSuperTrendPro[idx].MAInputUptrend == mAInputUptrend && cacheninZaSuperTrendPro[idx].MAInputDowntrend == mAInputDowntrend && cacheninZaSuperTrendPro[idx].MAPeriod == mAPeriod && cacheninZaSuperTrendPro[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaSuperTrendPro[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaSuperTrendPro[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaSuperTrendPro[idx].OffsetMultiplier == offsetMultiplier && cacheninZaSuperTrendPro[idx].OffsetPeriod == offsetPeriod && cacheninZaSuperTrendPro[idx].EqualsInput(input))
						return cacheninZaSuperTrendPro[idx];
			return CacheIndicator<ninZaSuperTrendPro>(new ninZaSuperTrendPro(){ MAType = mAType, MAInputUptrend = mAInputUptrend, MAInputDowntrend = mAInputDowntrend, MAPeriod = mAPeriod, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, OffsetMultiplier = offsetMultiplier, OffsetPeriod = offsetPeriod }, input, ref cacheninZaSuperTrendPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperTrendPro ninZaSuperTrendPro(ninZa_MAType mAType, PriceType mAInputUptrend, PriceType mAInputDowntrend, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaSuperTrendPro(Input, mAType, mAInputUptrend, mAInputDowntrend, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}


		
		public Indicators.ninZaSuperTrendPro ninZaSuperTrendPro(ISeries<double> input , ninZa_MAType mAType, PriceType mAInputUptrend, PriceType mAInputDowntrend, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaSuperTrendPro(input, mAType, mAInputUptrend, mAInputDowntrend, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperTrendPro ninZaSuperTrendPro(ninZa_MAType mAType, PriceType mAInputUptrend, PriceType mAInputDowntrend, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaSuperTrendPro(Input, mAType, mAInputUptrend, mAInputDowntrend, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}


		
		public Indicators.ninZaSuperTrendPro ninZaSuperTrendPro(ISeries<double> input , ninZa_MAType mAType, PriceType mAInputUptrend, PriceType mAInputDowntrend, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaSuperTrendPro(input, mAType, mAInputUptrend, mAInputDowntrend, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}

	}
}

#endregion
