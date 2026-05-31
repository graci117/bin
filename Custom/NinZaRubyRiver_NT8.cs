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
		
		private ninZaRubyRiver[] cacheninZaRubyRiver;

		
		public ninZaRubyRiver ninZaRubyRiver(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return ninZaRubyRiver(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}


		
		public ninZaRubyRiver ninZaRubyRiver(ISeries<double> input, ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			if (cacheninZaRubyRiver != null)
				for (int idx = 0; idx < cacheninZaRubyRiver.Length; idx++)
					if (cacheninZaRubyRiver[idx].MAType == mAType && cacheninZaRubyRiver[idx].MAPeriod == mAPeriod && cacheninZaRubyRiver[idx].MASmoothingEnabled == mASmoothingEnabled && cacheninZaRubyRiver[idx].MASmoothingMethod == mASmoothingMethod && cacheninZaRubyRiver[idx].MASmoothingPeriod == mASmoothingPeriod && cacheninZaRubyRiver[idx].OffsetMultiplier == offsetMultiplier && cacheninZaRubyRiver[idx].OffsetPeriod == offsetPeriod && cacheninZaRubyRiver[idx].EqualsInput(input))
						return cacheninZaRubyRiver[idx];
			return CacheIndicator<ninZaRubyRiver>(new ninZaRubyRiver(){ MAType = mAType, MAPeriod = mAPeriod, MASmoothingEnabled = mASmoothingEnabled, MASmoothingMethod = mASmoothingMethod, MASmoothingPeriod = mASmoothingPeriod, OffsetMultiplier = offsetMultiplier, OffsetPeriod = offsetPeriod }, input, ref cacheninZaRubyRiver);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaRubyRiver ninZaRubyRiver(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaRubyRiver(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}


		
		public Indicators.ninZaRubyRiver ninZaRubyRiver(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaRubyRiver(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaRubyRiver ninZaRubyRiver(ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaRubyRiver(Input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}


		
		public Indicators.ninZaRubyRiver ninZaRubyRiver(ISeries<double> input , ninZa_MAType mAType, int mAPeriod, bool mASmoothingEnabled, ninZa_MAType mASmoothingMethod, int mASmoothingPeriod, double offsetMultiplier, int offsetPeriod)
		{
			return indicator.ninZaRubyRiver(input, mAType, mAPeriod, mASmoothingEnabled, mASmoothingMethod, mASmoothingPeriod, offsetMultiplier, offsetPeriod);
		}

	}
}

#endregion
