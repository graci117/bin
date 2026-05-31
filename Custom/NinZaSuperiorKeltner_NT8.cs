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
		
		private ninZaSuperiorKeltner[] cacheninZaSuperiorKeltner;

		
		public ninZaSuperiorKeltner ninZaSuperiorKeltner(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorKeltner_OffsetUnit offsetUnit, int offsetATRPeriod)
		{
			return ninZaSuperiorKeltner(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetATRPeriod);
		}


		
		public ninZaSuperiorKeltner ninZaSuperiorKeltner(ISeries<double> input, ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorKeltner_OffsetUnit offsetUnit, int offsetATRPeriod)
		{
			if (cacheninZaSuperiorKeltner != null)
				for (int idx = 0; idx < cacheninZaSuperiorKeltner.Length; idx++)
					if (cacheninZaSuperiorKeltner[idx].MAType == mAType && cacheninZaSuperiorKeltner[idx].Period == period && cacheninZaSuperiorKeltner[idx].SmoothingEnabled == smoothingEnabled && cacheninZaSuperiorKeltner[idx].SmoothingMethod == smoothingMethod && cacheninZaSuperiorKeltner[idx].SmoothingPeriod == smoothingPeriod && cacheninZaSuperiorKeltner[idx].OffsetMultiplier == offsetMultiplier && cacheninZaSuperiorKeltner[idx].OffsetUnit == offsetUnit && cacheninZaSuperiorKeltner[idx].OffsetATRPeriod == offsetATRPeriod && cacheninZaSuperiorKeltner[idx].EqualsInput(input))
						return cacheninZaSuperiorKeltner[idx];
			return CacheIndicator<ninZaSuperiorKeltner>(new ninZaSuperiorKeltner(){ MAType = mAType, Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, OffsetMultiplier = offsetMultiplier, OffsetUnit = offsetUnit, OffsetATRPeriod = offsetATRPeriod }, input, ref cacheninZaSuperiorKeltner);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorKeltner ninZaSuperiorKeltner(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorKeltner_OffsetUnit offsetUnit, int offsetATRPeriod)
		{
			return indicator.ninZaSuperiorKeltner(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetATRPeriod);
		}


		
		public Indicators.ninZaSuperiorKeltner ninZaSuperiorKeltner(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorKeltner_OffsetUnit offsetUnit, int offsetATRPeriod)
		{
			return indicator.ninZaSuperiorKeltner(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetATRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorKeltner ninZaSuperiorKeltner(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorKeltner_OffsetUnit offsetUnit, int offsetATRPeriod)
		{
			return indicator.ninZaSuperiorKeltner(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetATRPeriod);
		}


		
		public Indicators.ninZaSuperiorKeltner ninZaSuperiorKeltner(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorKeltner_OffsetUnit offsetUnit, int offsetATRPeriod)
		{
			return indicator.ninZaSuperiorKeltner(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetATRPeriod);
		}

	}
}

#endregion
