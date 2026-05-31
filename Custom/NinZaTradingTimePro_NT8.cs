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
		
		private ninZaTradingTimePro[] cacheninZaTradingTimePro;

		
		public ninZaTradingTimePro ninZaTradingTimePro(bool time1Enabled, int time1Start, int time1Duration, bool time2Enabled, int time2Start, int time2Duration, bool time3Enabled, int time3Start, int time3Duration)
		{
			return ninZaTradingTimePro(Input, time1Enabled, time1Start, time1Duration, time2Enabled, time2Start, time2Duration, time3Enabled, time3Start, time3Duration);
		}


		
		public ninZaTradingTimePro ninZaTradingTimePro(ISeries<double> input, bool time1Enabled, int time1Start, int time1Duration, bool time2Enabled, int time2Start, int time2Duration, bool time3Enabled, int time3Start, int time3Duration)
		{
			if (cacheninZaTradingTimePro != null)
				for (int idx = 0; idx < cacheninZaTradingTimePro.Length; idx++)
					if (cacheninZaTradingTimePro[idx].Time1Enabled == time1Enabled && cacheninZaTradingTimePro[idx].Time1Start == time1Start && cacheninZaTradingTimePro[idx].Time1Duration == time1Duration && cacheninZaTradingTimePro[idx].Time2Enabled == time2Enabled && cacheninZaTradingTimePro[idx].Time2Start == time2Start && cacheninZaTradingTimePro[idx].Time2Duration == time2Duration && cacheninZaTradingTimePro[idx].Time3Enabled == time3Enabled && cacheninZaTradingTimePro[idx].Time3Start == time3Start && cacheninZaTradingTimePro[idx].Time3Duration == time3Duration && cacheninZaTradingTimePro[idx].EqualsInput(input))
						return cacheninZaTradingTimePro[idx];
			return CacheIndicator<ninZaTradingTimePro>(new ninZaTradingTimePro(){ Time1Enabled = time1Enabled, Time1Start = time1Start, Time1Duration = time1Duration, Time2Enabled = time2Enabled, Time2Start = time2Start, Time2Duration = time2Duration, Time3Enabled = time3Enabled, Time3Start = time3Start, Time3Duration = time3Duration }, input, ref cacheninZaTradingTimePro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaTradingTimePro ninZaTradingTimePro(bool time1Enabled, int time1Start, int time1Duration, bool time2Enabled, int time2Start, int time2Duration, bool time3Enabled, int time3Start, int time3Duration)
		{
			return indicator.ninZaTradingTimePro(Input, time1Enabled, time1Start, time1Duration, time2Enabled, time2Start, time2Duration, time3Enabled, time3Start, time3Duration);
		}


		
		public Indicators.ninZaTradingTimePro ninZaTradingTimePro(ISeries<double> input , bool time1Enabled, int time1Start, int time1Duration, bool time2Enabled, int time2Start, int time2Duration, bool time3Enabled, int time3Start, int time3Duration)
		{
			return indicator.ninZaTradingTimePro(input, time1Enabled, time1Start, time1Duration, time2Enabled, time2Start, time2Duration, time3Enabled, time3Start, time3Duration);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaTradingTimePro ninZaTradingTimePro(bool time1Enabled, int time1Start, int time1Duration, bool time2Enabled, int time2Start, int time2Duration, bool time3Enabled, int time3Start, int time3Duration)
		{
			return indicator.ninZaTradingTimePro(Input, time1Enabled, time1Start, time1Duration, time2Enabled, time2Start, time2Duration, time3Enabled, time3Start, time3Duration);
		}


		
		public Indicators.ninZaTradingTimePro ninZaTradingTimePro(ISeries<double> input , bool time1Enabled, int time1Start, int time1Duration, bool time2Enabled, int time2Start, int time2Duration, bool time3Enabled, int time3Start, int time3Duration)
		{
			return indicator.ninZaTradingTimePro(input, time1Enabled, time1Start, time1Duration, time2Enabled, time2Start, time2Duration, time3Enabled, time3Start, time3Duration);
		}

	}
}

#endregion
