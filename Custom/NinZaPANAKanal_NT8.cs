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
		
		private ninZaPANAKanal[] cacheninZaPANAKanal;

		
		public ninZaPANAKanal ninZaPANAKanal(int period, double factor, int middlePeriod, int signalBreakSplit, int signalPullbackFindingPeriod)
		{
			return ninZaPANAKanal(Input, period, factor, middlePeriod, signalBreakSplit, signalPullbackFindingPeriod);
		}


		
		public ninZaPANAKanal ninZaPANAKanal(ISeries<double> input, int period, double factor, int middlePeriod, int signalBreakSplit, int signalPullbackFindingPeriod)
		{
			if (cacheninZaPANAKanal != null)
				for (int idx = 0; idx < cacheninZaPANAKanal.Length; idx++)
					if (cacheninZaPANAKanal[idx].Period == period && cacheninZaPANAKanal[idx].Factor == factor && cacheninZaPANAKanal[idx].MiddlePeriod == middlePeriod && cacheninZaPANAKanal[idx].SignalBreakSplit == signalBreakSplit && cacheninZaPANAKanal[idx].SignalPullbackFindingPeriod == signalPullbackFindingPeriod && cacheninZaPANAKanal[idx].EqualsInput(input))
						return cacheninZaPANAKanal[idx];
			return CacheIndicator<ninZaPANAKanal>(new ninZaPANAKanal(){ Period = period, Factor = factor, MiddlePeriod = middlePeriod, SignalBreakSplit = signalBreakSplit, SignalPullbackFindingPeriod = signalPullbackFindingPeriod }, input, ref cacheninZaPANAKanal);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaPANAKanal ninZaPANAKanal(int period, double factor, int middlePeriod, int signalBreakSplit, int signalPullbackFindingPeriod)
		{
			return indicator.ninZaPANAKanal(Input, period, factor, middlePeriod, signalBreakSplit, signalPullbackFindingPeriod);
		}


		
		public Indicators.ninZaPANAKanal ninZaPANAKanal(ISeries<double> input , int period, double factor, int middlePeriod, int signalBreakSplit, int signalPullbackFindingPeriod)
		{
			return indicator.ninZaPANAKanal(input, period, factor, middlePeriod, signalBreakSplit, signalPullbackFindingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaPANAKanal ninZaPANAKanal(int period, double factor, int middlePeriod, int signalBreakSplit, int signalPullbackFindingPeriod)
		{
			return indicator.ninZaPANAKanal(Input, period, factor, middlePeriod, signalBreakSplit, signalPullbackFindingPeriod);
		}


		
		public Indicators.ninZaPANAKanal ninZaPANAKanal(ISeries<double> input , int period, double factor, int middlePeriod, int signalBreakSplit, int signalPullbackFindingPeriod)
		{
			return indicator.ninZaPANAKanal(input, period, factor, middlePeriod, signalBreakSplit, signalPullbackFindingPeriod);
		}

	}
}

#endregion
