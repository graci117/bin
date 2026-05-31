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
		
		private ninZaTripleRangeRover[] cacheninZaTripleRangeRover;

		
		public ninZaTripleRangeRover ninZaTripleRangeRover(ninZa_MAType mAType, int fastPeriod, double fastRange, int mediumPeriod, double mediumRange, int slowPeriod, double slowRange, bool filterEnabled, double filterMultiplier)
		{
			return ninZaTripleRangeRover(Input, mAType, fastPeriod, fastRange, mediumPeriod, mediumRange, slowPeriod, slowRange, filterEnabled, filterMultiplier);
		}


		
		public ninZaTripleRangeRover ninZaTripleRangeRover(ISeries<double> input, ninZa_MAType mAType, int fastPeriod, double fastRange, int mediumPeriod, double mediumRange, int slowPeriod, double slowRange, bool filterEnabled, double filterMultiplier)
		{
			if (cacheninZaTripleRangeRover != null)
				for (int idx = 0; idx < cacheninZaTripleRangeRover.Length; idx++)
					if (cacheninZaTripleRangeRover[idx].MAType == mAType && cacheninZaTripleRangeRover[idx].FastPeriod == fastPeriod && cacheninZaTripleRangeRover[idx].FastRange == fastRange && cacheninZaTripleRangeRover[idx].MediumPeriod == mediumPeriod && cacheninZaTripleRangeRover[idx].MediumRange == mediumRange && cacheninZaTripleRangeRover[idx].SlowPeriod == slowPeriod && cacheninZaTripleRangeRover[idx].SlowRange == slowRange && cacheninZaTripleRangeRover[idx].FilterEnabled == filterEnabled && cacheninZaTripleRangeRover[idx].FilterMultiplier == filterMultiplier && cacheninZaTripleRangeRover[idx].EqualsInput(input))
						return cacheninZaTripleRangeRover[idx];
			return CacheIndicator<ninZaTripleRangeRover>(new ninZaTripleRangeRover(){ MAType = mAType, FastPeriod = fastPeriod, FastRange = fastRange, MediumPeriod = mediumPeriod, MediumRange = mediumRange, SlowPeriod = slowPeriod, SlowRange = slowRange, FilterEnabled = filterEnabled, FilterMultiplier = filterMultiplier }, input, ref cacheninZaTripleRangeRover);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaTripleRangeRover ninZaTripleRangeRover(ninZa_MAType mAType, int fastPeriod, double fastRange, int mediumPeriod, double mediumRange, int slowPeriod, double slowRange, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTripleRangeRover(Input, mAType, fastPeriod, fastRange, mediumPeriod, mediumRange, slowPeriod, slowRange, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaTripleRangeRover ninZaTripleRangeRover(ISeries<double> input , ninZa_MAType mAType, int fastPeriod, double fastRange, int mediumPeriod, double mediumRange, int slowPeriod, double slowRange, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTripleRangeRover(input, mAType, fastPeriod, fastRange, mediumPeriod, mediumRange, slowPeriod, slowRange, filterEnabled, filterMultiplier);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaTripleRangeRover ninZaTripleRangeRover(ninZa_MAType mAType, int fastPeriod, double fastRange, int mediumPeriod, double mediumRange, int slowPeriod, double slowRange, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTripleRangeRover(Input, mAType, fastPeriod, fastRange, mediumPeriod, mediumRange, slowPeriod, slowRange, filterEnabled, filterMultiplier);
		}


		
		public Indicators.ninZaTripleRangeRover ninZaTripleRangeRover(ISeries<double> input , ninZa_MAType mAType, int fastPeriod, double fastRange, int mediumPeriod, double mediumRange, int slowPeriod, double slowRange, bool filterEnabled, double filterMultiplier)
		{
			return indicator.ninZaTripleRangeRover(input, mAType, fastPeriod, fastRange, mediumPeriod, mediumRange, slowPeriod, slowRange, filterEnabled, filterMultiplier);
		}

	}
}

#endregion
