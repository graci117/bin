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
		
		private RenkoKings.RenkoKings_SumoPullback[] cacheRenkoKings_SumoPullback;

		
		public RenkoKings.RenkoKings_SumoPullback RenkoKings_SumoPullback(ninZa_MAType slowMAType, int slowMAPeriod, bool slowMASmoothingEnabled, ninZa_MAType slowMASmoothingMethod, int slowMASmoothingPeriod, ninZa_MAType fastMA1Type, int fastMA1Period, bool fastMA1SmoothingEnabled, ninZa_MAType fastMA1SmoothingMethod, int fastMA1SmoothingPeriod, ninZa_MAType fastMA2Type, int fastMA2Period, bool fastMA2SmoothingEnabled, ninZa_MAType fastMA2SmoothingMethod, int fastMA2SmoothingPeriod, ninZa_MAType fastMA3Type, int fastMA3Period, bool fastMA3SmoothingEnabled, ninZa_MAType fastMA3SmoothingMethod, int fastMA3SmoothingPeriod, int signalSplitFirst, int signalSplitSecond)
		{
			return RenkoKings_SumoPullback(Input, slowMAType, slowMAPeriod, slowMASmoothingEnabled, slowMASmoothingMethod, slowMASmoothingPeriod, fastMA1Type, fastMA1Period, fastMA1SmoothingEnabled, fastMA1SmoothingMethod, fastMA1SmoothingPeriod, fastMA2Type, fastMA2Period, fastMA2SmoothingEnabled, fastMA2SmoothingMethod, fastMA2SmoothingPeriod, fastMA3Type, fastMA3Period, fastMA3SmoothingEnabled, fastMA3SmoothingMethod, fastMA3SmoothingPeriod, signalSplitFirst, signalSplitSecond);
		}


		
		public RenkoKings.RenkoKings_SumoPullback RenkoKings_SumoPullback(ISeries<double> input, ninZa_MAType slowMAType, int slowMAPeriod, bool slowMASmoothingEnabled, ninZa_MAType slowMASmoothingMethod, int slowMASmoothingPeriod, ninZa_MAType fastMA1Type, int fastMA1Period, bool fastMA1SmoothingEnabled, ninZa_MAType fastMA1SmoothingMethod, int fastMA1SmoothingPeriod, ninZa_MAType fastMA2Type, int fastMA2Period, bool fastMA2SmoothingEnabled, ninZa_MAType fastMA2SmoothingMethod, int fastMA2SmoothingPeriod, ninZa_MAType fastMA3Type, int fastMA3Period, bool fastMA3SmoothingEnabled, ninZa_MAType fastMA3SmoothingMethod, int fastMA3SmoothingPeriod, int signalSplitFirst, int signalSplitSecond)
		{
			if (cacheRenkoKings_SumoPullback != null)
				for (int idx = 0; idx < cacheRenkoKings_SumoPullback.Length; idx++)
					if (cacheRenkoKings_SumoPullback[idx].SlowMAType == slowMAType && cacheRenkoKings_SumoPullback[idx].SlowMAPeriod == slowMAPeriod && cacheRenkoKings_SumoPullback[idx].SlowMASmoothingEnabled == slowMASmoothingEnabled && cacheRenkoKings_SumoPullback[idx].SlowMASmoothingMethod == slowMASmoothingMethod && cacheRenkoKings_SumoPullback[idx].SlowMASmoothingPeriod == slowMASmoothingPeriod && cacheRenkoKings_SumoPullback[idx].FastMA1Type == fastMA1Type && cacheRenkoKings_SumoPullback[idx].FastMA1Period == fastMA1Period && cacheRenkoKings_SumoPullback[idx].FastMA1SmoothingEnabled == fastMA1SmoothingEnabled && cacheRenkoKings_SumoPullback[idx].FastMA1SmoothingMethod == fastMA1SmoothingMethod && cacheRenkoKings_SumoPullback[idx].FastMA1SmoothingPeriod == fastMA1SmoothingPeriod && cacheRenkoKings_SumoPullback[idx].FastMA2Type == fastMA2Type && cacheRenkoKings_SumoPullback[idx].FastMA2Period == fastMA2Period && cacheRenkoKings_SumoPullback[idx].FastMA2SmoothingEnabled == fastMA2SmoothingEnabled && cacheRenkoKings_SumoPullback[idx].FastMA2SmoothingMethod == fastMA2SmoothingMethod && cacheRenkoKings_SumoPullback[idx].FastMA2SmoothingPeriod == fastMA2SmoothingPeriod && cacheRenkoKings_SumoPullback[idx].FastMA3Type == fastMA3Type && cacheRenkoKings_SumoPullback[idx].FastMA3Period == fastMA3Period && cacheRenkoKings_SumoPullback[idx].FastMA3SmoothingEnabled == fastMA3SmoothingEnabled && cacheRenkoKings_SumoPullback[idx].FastMA3SmoothingMethod == fastMA3SmoothingMethod && cacheRenkoKings_SumoPullback[idx].FastMA3SmoothingPeriod == fastMA3SmoothingPeriod && cacheRenkoKings_SumoPullback[idx].SignalSplitFirst == signalSplitFirst && cacheRenkoKings_SumoPullback[idx].SignalSplitSecond == signalSplitSecond && cacheRenkoKings_SumoPullback[idx].EqualsInput(input))
						return cacheRenkoKings_SumoPullback[idx];
			return CacheIndicator<RenkoKings.RenkoKings_SumoPullback>(new RenkoKings.RenkoKings_SumoPullback(){ SlowMAType = slowMAType, SlowMAPeriod = slowMAPeriod, SlowMASmoothingEnabled = slowMASmoothingEnabled, SlowMASmoothingMethod = slowMASmoothingMethod, SlowMASmoothingPeriod = slowMASmoothingPeriod, FastMA1Type = fastMA1Type, FastMA1Period = fastMA1Period, FastMA1SmoothingEnabled = fastMA1SmoothingEnabled, FastMA1SmoothingMethod = fastMA1SmoothingMethod, FastMA1SmoothingPeriod = fastMA1SmoothingPeriod, FastMA2Type = fastMA2Type, FastMA2Period = fastMA2Period, FastMA2SmoothingEnabled = fastMA2SmoothingEnabled, FastMA2SmoothingMethod = fastMA2SmoothingMethod, FastMA2SmoothingPeriod = fastMA2SmoothingPeriod, FastMA3Type = fastMA3Type, FastMA3Period = fastMA3Period, FastMA3SmoothingEnabled = fastMA3SmoothingEnabled, FastMA3SmoothingMethod = fastMA3SmoothingMethod, FastMA3SmoothingPeriod = fastMA3SmoothingPeriod, SignalSplitFirst = signalSplitFirst, SignalSplitSecond = signalSplitSecond }, input, ref cacheRenkoKings_SumoPullback);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.RenkoKings.RenkoKings_SumoPullback RenkoKings_SumoPullback(ninZa_MAType slowMAType, int slowMAPeriod, bool slowMASmoothingEnabled, ninZa_MAType slowMASmoothingMethod, int slowMASmoothingPeriod, ninZa_MAType fastMA1Type, int fastMA1Period, bool fastMA1SmoothingEnabled, ninZa_MAType fastMA1SmoothingMethod, int fastMA1SmoothingPeriod, ninZa_MAType fastMA2Type, int fastMA2Period, bool fastMA2SmoothingEnabled, ninZa_MAType fastMA2SmoothingMethod, int fastMA2SmoothingPeriod, ninZa_MAType fastMA3Type, int fastMA3Period, bool fastMA3SmoothingEnabled, ninZa_MAType fastMA3SmoothingMethod, int fastMA3SmoothingPeriod, int signalSplitFirst, int signalSplitSecond)
		{
			return indicator.RenkoKings_SumoPullback(Input, slowMAType, slowMAPeriod, slowMASmoothingEnabled, slowMASmoothingMethod, slowMASmoothingPeriod, fastMA1Type, fastMA1Period, fastMA1SmoothingEnabled, fastMA1SmoothingMethod, fastMA1SmoothingPeriod, fastMA2Type, fastMA2Period, fastMA2SmoothingEnabled, fastMA2SmoothingMethod, fastMA2SmoothingPeriod, fastMA3Type, fastMA3Period, fastMA3SmoothingEnabled, fastMA3SmoothingMethod, fastMA3SmoothingPeriod, signalSplitFirst, signalSplitSecond);
		}


		
		public Indicators.RenkoKings.RenkoKings_SumoPullback RenkoKings_SumoPullback(ISeries<double> input , ninZa_MAType slowMAType, int slowMAPeriod, bool slowMASmoothingEnabled, ninZa_MAType slowMASmoothingMethod, int slowMASmoothingPeriod, ninZa_MAType fastMA1Type, int fastMA1Period, bool fastMA1SmoothingEnabled, ninZa_MAType fastMA1SmoothingMethod, int fastMA1SmoothingPeriod, ninZa_MAType fastMA2Type, int fastMA2Period, bool fastMA2SmoothingEnabled, ninZa_MAType fastMA2SmoothingMethod, int fastMA2SmoothingPeriod, ninZa_MAType fastMA3Type, int fastMA3Period, bool fastMA3SmoothingEnabled, ninZa_MAType fastMA3SmoothingMethod, int fastMA3SmoothingPeriod, int signalSplitFirst, int signalSplitSecond)
		{
			return indicator.RenkoKings_SumoPullback(input, slowMAType, slowMAPeriod, slowMASmoothingEnabled, slowMASmoothingMethod, slowMASmoothingPeriod, fastMA1Type, fastMA1Period, fastMA1SmoothingEnabled, fastMA1SmoothingMethod, fastMA1SmoothingPeriod, fastMA2Type, fastMA2Period, fastMA2SmoothingEnabled, fastMA2SmoothingMethod, fastMA2SmoothingPeriod, fastMA3Type, fastMA3Period, fastMA3SmoothingEnabled, fastMA3SmoothingMethod, fastMA3SmoothingPeriod, signalSplitFirst, signalSplitSecond);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.RenkoKings.RenkoKings_SumoPullback RenkoKings_SumoPullback(ninZa_MAType slowMAType, int slowMAPeriod, bool slowMASmoothingEnabled, ninZa_MAType slowMASmoothingMethod, int slowMASmoothingPeriod, ninZa_MAType fastMA1Type, int fastMA1Period, bool fastMA1SmoothingEnabled, ninZa_MAType fastMA1SmoothingMethod, int fastMA1SmoothingPeriod, ninZa_MAType fastMA2Type, int fastMA2Period, bool fastMA2SmoothingEnabled, ninZa_MAType fastMA2SmoothingMethod, int fastMA2SmoothingPeriod, ninZa_MAType fastMA3Type, int fastMA3Period, bool fastMA3SmoothingEnabled, ninZa_MAType fastMA3SmoothingMethod, int fastMA3SmoothingPeriod, int signalSplitFirst, int signalSplitSecond)
		{
			return indicator.RenkoKings_SumoPullback(Input, slowMAType, slowMAPeriod, slowMASmoothingEnabled, slowMASmoothingMethod, slowMASmoothingPeriod, fastMA1Type, fastMA1Period, fastMA1SmoothingEnabled, fastMA1SmoothingMethod, fastMA1SmoothingPeriod, fastMA2Type, fastMA2Period, fastMA2SmoothingEnabled, fastMA2SmoothingMethod, fastMA2SmoothingPeriod, fastMA3Type, fastMA3Period, fastMA3SmoothingEnabled, fastMA3SmoothingMethod, fastMA3SmoothingPeriod, signalSplitFirst, signalSplitSecond);
		}


		
		public Indicators.RenkoKings.RenkoKings_SumoPullback RenkoKings_SumoPullback(ISeries<double> input , ninZa_MAType slowMAType, int slowMAPeriod, bool slowMASmoothingEnabled, ninZa_MAType slowMASmoothingMethod, int slowMASmoothingPeriod, ninZa_MAType fastMA1Type, int fastMA1Period, bool fastMA1SmoothingEnabled, ninZa_MAType fastMA1SmoothingMethod, int fastMA1SmoothingPeriod, ninZa_MAType fastMA2Type, int fastMA2Period, bool fastMA2SmoothingEnabled, ninZa_MAType fastMA2SmoothingMethod, int fastMA2SmoothingPeriod, ninZa_MAType fastMA3Type, int fastMA3Period, bool fastMA3SmoothingEnabled, ninZa_MAType fastMA3SmoothingMethod, int fastMA3SmoothingPeriod, int signalSplitFirst, int signalSplitSecond)
		{
			return indicator.RenkoKings_SumoPullback(input, slowMAType, slowMAPeriod, slowMASmoothingEnabled, slowMASmoothingMethod, slowMASmoothingPeriod, fastMA1Type, fastMA1Period, fastMA1SmoothingEnabled, fastMA1SmoothingMethod, fastMA1SmoothingPeriod, fastMA2Type, fastMA2Period, fastMA2SmoothingEnabled, fastMA2SmoothingMethod, fastMA2SmoothingPeriod, fastMA3Type, fastMA3Period, fastMA3SmoothingEnabled, fastMA3SmoothingMethod, fastMA3SmoothingPeriod, signalSplitFirst, signalSplitSecond);
		}

	}
}

#endregion
