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
		
		private A_Plus.AplusGannHighLow[] cacheAplusGannHighLow;

		
		public A_Plus.AplusGannHighLow AplusGannHighLow(int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, MTFTimeframe higherTimeframe, int tickValue, DashStyleHelper mTFLineStyle, int mTFLineWidth)
		{
			return AplusGannHighLow(Input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, tickValue, mTFLineStyle, mTFLineWidth);
		}


		
		public A_Plus.AplusGannHighLow AplusGannHighLow(ISeries<double> input, int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, MTFTimeframe higherTimeframe, int tickValue, DashStyleHelper mTFLineStyle, int mTFLineWidth)
		{
			if (cacheAplusGannHighLow != null)
				for (int idx = 0; idx < cacheAplusGannHighLow.Length; idx++)
					if (cacheAplusGannHighLow[idx].HPeriod == hPeriod && cacheAplusGannHighLow[idx].LPeriod == lPeriod && cacheAplusGannHighLow[idx].BearishColor == bearishColor && cacheAplusGannHighLow[idx].BullishColor == bullishColor && cacheAplusGannHighLow[idx].ShowMTFCenterLine == showMTFCenterLine && cacheAplusGannHighLow[idx].HigherTimeframe == higherTimeframe && cacheAplusGannHighLow[idx].TickValue == tickValue && cacheAplusGannHighLow[idx].MTFLineStyle == mTFLineStyle && cacheAplusGannHighLow[idx].MTFLineWidth == mTFLineWidth && cacheAplusGannHighLow[idx].EqualsInput(input))
						return cacheAplusGannHighLow[idx];
			return CacheIndicator<A_Plus.AplusGannHighLow>(new A_Plus.AplusGannHighLow(){ HPeriod = hPeriod, LPeriod = lPeriod, BearishColor = bearishColor, BullishColor = bullishColor, ShowMTFCenterLine = showMTFCenterLine, HigherTimeframe = higherTimeframe, TickValue = tickValue, MTFLineStyle = mTFLineStyle, MTFLineWidth = mTFLineWidth }, input, ref cacheAplusGannHighLow);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.AplusGannHighLow AplusGannHighLow(int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, MTFTimeframe higherTimeframe, int tickValue, DashStyleHelper mTFLineStyle, int mTFLineWidth)
		{
			return indicator.AplusGannHighLow(Input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, tickValue, mTFLineStyle, mTFLineWidth);
		}


		
		public Indicators.A_Plus.AplusGannHighLow AplusGannHighLow(ISeries<double> input , int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, MTFTimeframe higherTimeframe, int tickValue, DashStyleHelper mTFLineStyle, int mTFLineWidth)
		{
			return indicator.AplusGannHighLow(input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, tickValue, mTFLineStyle, mTFLineWidth);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.AplusGannHighLow AplusGannHighLow(int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, MTFTimeframe higherTimeframe, int tickValue, DashStyleHelper mTFLineStyle, int mTFLineWidth)
		{
			return indicator.AplusGannHighLow(Input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, tickValue, mTFLineStyle, mTFLineWidth);
		}


		
		public Indicators.A_Plus.AplusGannHighLow AplusGannHighLow(ISeries<double> input , int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, MTFTimeframe higherTimeframe, int tickValue, DashStyleHelper mTFLineStyle, int mTFLineWidth)
		{
			return indicator.AplusGannHighLow(input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, tickValue, mTFLineStyle, mTFLineWidth);
		}

	}
}

#endregion
