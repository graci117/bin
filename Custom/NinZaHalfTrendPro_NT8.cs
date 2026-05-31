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
		
		private ninZaHalfTrendPro[] cacheninZaHalfTrendPro;

		
		public ninZaHalfTrendPro ninZaHalfTrendPro(int lookback, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetThresholdMultiplier, int offsetATRPeriod)
		{
			return ninZaHalfTrendPro(Input, lookback, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetThresholdMultiplier, offsetATRPeriod);
		}


		
		public ninZaHalfTrendPro ninZaHalfTrendPro(ISeries<double> input, int lookback, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetThresholdMultiplier, int offsetATRPeriod)
		{
			if (cacheninZaHalfTrendPro != null)
				for (int idx = 0; idx < cacheninZaHalfTrendPro.Length; idx++)
					if (cacheninZaHalfTrendPro[idx].Lookback == lookback && cacheninZaHalfTrendPro[idx].MAType == mAType && cacheninZaHalfTrendPro[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaHalfTrendPro[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaHalfTrendPro[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaHalfTrendPro[idx].OffsetThresholdMultiplier == offsetThresholdMultiplier && cacheninZaHalfTrendPro[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaHalfTrendPro[idx].EqualsInput(input))
						return cacheninZaHalfTrendPro[idx];
			return CacheIndicator<ninZaHalfTrendPro>(new ninZaHalfTrendPro(){ Lookback = lookback, MAType = mAType, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, OffsetThresholdMultiplier = offsetThresholdMultiplier, OffsetATRPeriod = offsetATRPeriod }, input, ref cacheninZaHalfTrendPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHalfTrendPro ninZaHalfTrendPro(int lookback, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetThresholdMultiplier, int offsetATRPeriod)
		{
			return indicator.ninZaHalfTrendPro(Input, lookback, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetThresholdMultiplier, offsetATRPeriod);
		}


		
		public Indicators.ninZaHalfTrendPro ninZaHalfTrendPro(ISeries<double> input , int lookback, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetThresholdMultiplier, int offsetATRPeriod)
		{
			return indicator.ninZaHalfTrendPro(input, lookback, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetThresholdMultiplier, offsetATRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHalfTrendPro ninZaHalfTrendPro(int lookback, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetThresholdMultiplier, int offsetATRPeriod)
		{
			return indicator.ninZaHalfTrendPro(Input, lookback, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetThresholdMultiplier, offsetATRPeriod);
		}


		
		public Indicators.ninZaHalfTrendPro ninZaHalfTrendPro(ISeries<double> input , int lookback, ninZa_MAType mAType, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetThresholdMultiplier, int offsetATRPeriod)
		{
			return indicator.ninZaHalfTrendPro(input, lookback, mAType, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetThresholdMultiplier, offsetATRPeriod);
		}

	}
}

#endregion
