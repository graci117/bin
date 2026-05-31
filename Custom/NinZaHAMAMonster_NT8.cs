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
		
		private ninZaHAMAMonster[] cacheninZaHAMAMonster;

		
		public ninZaHAMAMonster ninZaHAMAMonster(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, int hAOpenWeight, int crossoverOffset, bool roundedToTickSize)
		{
			return ninZaHAMAMonster(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, hAOpenWeight, crossoverOffset, roundedToTickSize);
		}


		
		public ninZaHAMAMonster ninZaHAMAMonster(ISeries<double> input, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, int hAOpenWeight, int crossoverOffset, bool roundedToTickSize)
		{
			if (cacheninZaHAMAMonster != null)
				for (int idx = 0; idx < cacheninZaHAMAMonster.Length; idx++)
					if (cacheninZaHAMAMonster[idx].MAType == mAType && cacheninZaHAMAMonster[idx].MAPeriod == mAPeriod && cacheninZaHAMAMonster[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaHAMAMonster[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaHAMAMonster[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaHAMAMonster[idx].HAOpenWeight == hAOpenWeight && cacheninZaHAMAMonster[idx].CrossoverOffset == crossoverOffset && cacheninZaHAMAMonster[idx].RoundedToTickSize == roundedToTickSize && cacheninZaHAMAMonster[idx].EqualsInput(input))
						return cacheninZaHAMAMonster[idx];
			return CacheIndicator<ninZaHAMAMonster>(new ninZaHAMAMonster(){ MAType = mAType, MAPeriod = mAPeriod, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, HAOpenWeight = hAOpenWeight, CrossoverOffset = crossoverOffset, RoundedToTickSize = roundedToTickSize }, input, ref cacheninZaHAMAMonster);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHAMAMonster ninZaHAMAMonster(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, int hAOpenWeight, int crossoverOffset, bool roundedToTickSize)
		{
			return indicator.ninZaHAMAMonster(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, hAOpenWeight, crossoverOffset, roundedToTickSize);
		}


		
		public Indicators.ninZaHAMAMonster ninZaHAMAMonster(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, int hAOpenWeight, int crossoverOffset, bool roundedToTickSize)
		{
			return indicator.ninZaHAMAMonster(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, hAOpenWeight, crossoverOffset, roundedToTickSize);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHAMAMonster ninZaHAMAMonster(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, int hAOpenWeight, int crossoverOffset, bool roundedToTickSize)
		{
			return indicator.ninZaHAMAMonster(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, hAOpenWeight, crossoverOffset, roundedToTickSize);
		}


		
		public Indicators.ninZaHAMAMonster ninZaHAMAMonster(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, int hAOpenWeight, int crossoverOffset, bool roundedToTickSize)
		{
			return indicator.ninZaHAMAMonster(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, hAOpenWeight, crossoverOffset, roundedToTickSize);
		}

	}
}

#endregion
