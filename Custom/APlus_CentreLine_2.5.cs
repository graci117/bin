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
		
		private A_Plus.APlusCentreLine[] cacheAPlusCentreLine;

		
		public A_Plus.APlusCentreLine APlusCentreLine(int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, HTFTimeframe higherTimeframe, DashStyleHelper mTFLineStyle, int mTFLineWidth, int mTFTickValue, int mTFRangeValue, bool showLTFCenterLine, LTFTimeframe lowerTimeframe, DashStyleHelper lTFLineStyle, int lTFLineWidth, int lTFTickValue, int lTFRangeValue)
		{
			return APlusCentreLine(Input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, mTFLineStyle, mTFLineWidth, mTFTickValue, mTFRangeValue, showLTFCenterLine, lowerTimeframe, lTFLineStyle, lTFLineWidth, lTFTickValue, lTFRangeValue);
		}


		
		public A_Plus.APlusCentreLine APlusCentreLine(ISeries<double> input, int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, HTFTimeframe higherTimeframe, DashStyleHelper mTFLineStyle, int mTFLineWidth, int mTFTickValue, int mTFRangeValue, bool showLTFCenterLine, LTFTimeframe lowerTimeframe, DashStyleHelper lTFLineStyle, int lTFLineWidth, int lTFTickValue, int lTFRangeValue)
		{
			if (cacheAPlusCentreLine != null)
				for (int idx = 0; idx < cacheAPlusCentreLine.Length; idx++)
					if (cacheAPlusCentreLine[idx].HPeriod == hPeriod && cacheAPlusCentreLine[idx].LPeriod == lPeriod && cacheAPlusCentreLine[idx].BearishColor == bearishColor && cacheAPlusCentreLine[idx].BullishColor == bullishColor && cacheAPlusCentreLine[idx].ShowMTFCenterLine == showMTFCenterLine && cacheAPlusCentreLine[idx].HigherTimeframe == higherTimeframe && cacheAPlusCentreLine[idx].MTFLineStyle == mTFLineStyle && cacheAPlusCentreLine[idx].MTFLineWidth == mTFLineWidth && cacheAPlusCentreLine[idx].MTFTickValue == mTFTickValue && cacheAPlusCentreLine[idx].MTFRangeValue == mTFRangeValue && cacheAPlusCentreLine[idx].ShowLTFCenterLine == showLTFCenterLine && cacheAPlusCentreLine[idx].LowerTimeframe == lowerTimeframe && cacheAPlusCentreLine[idx].LTFLineStyle == lTFLineStyle && cacheAPlusCentreLine[idx].LTFLineWidth == lTFLineWidth && cacheAPlusCentreLine[idx].LTFTickValue == lTFTickValue && cacheAPlusCentreLine[idx].LTFRangeValue == lTFRangeValue && cacheAPlusCentreLine[idx].EqualsInput(input))
						return cacheAPlusCentreLine[idx];
			return CacheIndicator<A_Plus.APlusCentreLine>(new A_Plus.APlusCentreLine(){ HPeriod = hPeriod, LPeriod = lPeriod, BearishColor = bearishColor, BullishColor = bullishColor, ShowMTFCenterLine = showMTFCenterLine, HigherTimeframe = higherTimeframe, MTFLineStyle = mTFLineStyle, MTFLineWidth = mTFLineWidth, MTFTickValue = mTFTickValue, MTFRangeValue = mTFRangeValue, ShowLTFCenterLine = showLTFCenterLine, LowerTimeframe = lowerTimeframe, LTFLineStyle = lTFLineStyle, LTFLineWidth = lTFLineWidth, LTFTickValue = lTFTickValue, LTFRangeValue = lTFRangeValue }, input, ref cacheAPlusCentreLine);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.APlusCentreLine APlusCentreLine(int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, HTFTimeframe higherTimeframe, DashStyleHelper mTFLineStyle, int mTFLineWidth, int mTFTickValue, int mTFRangeValue, bool showLTFCenterLine, LTFTimeframe lowerTimeframe, DashStyleHelper lTFLineStyle, int lTFLineWidth, int lTFTickValue, int lTFRangeValue)
		{
			return indicator.APlusCentreLine(Input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, mTFLineStyle, mTFLineWidth, mTFTickValue, mTFRangeValue, showLTFCenterLine, lowerTimeframe, lTFLineStyle, lTFLineWidth, lTFTickValue, lTFRangeValue);
		}


		
		public Indicators.A_Plus.APlusCentreLine APlusCentreLine(ISeries<double> input , int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, HTFTimeframe higherTimeframe, DashStyleHelper mTFLineStyle, int mTFLineWidth, int mTFTickValue, int mTFRangeValue, bool showLTFCenterLine, LTFTimeframe lowerTimeframe, DashStyleHelper lTFLineStyle, int lTFLineWidth, int lTFTickValue, int lTFRangeValue)
		{
			return indicator.APlusCentreLine(input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, mTFLineStyle, mTFLineWidth, mTFTickValue, mTFRangeValue, showLTFCenterLine, lowerTimeframe, lTFLineStyle, lTFLineWidth, lTFTickValue, lTFRangeValue);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.APlusCentreLine APlusCentreLine(int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, HTFTimeframe higherTimeframe, DashStyleHelper mTFLineStyle, int mTFLineWidth, int mTFTickValue, int mTFRangeValue, bool showLTFCenterLine, LTFTimeframe lowerTimeframe, DashStyleHelper lTFLineStyle, int lTFLineWidth, int lTFTickValue, int lTFRangeValue)
		{
			return indicator.APlusCentreLine(Input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, mTFLineStyle, mTFLineWidth, mTFTickValue, mTFRangeValue, showLTFCenterLine, lowerTimeframe, lTFLineStyle, lTFLineWidth, lTFTickValue, lTFRangeValue);
		}


		
		public Indicators.A_Plus.APlusCentreLine APlusCentreLine(ISeries<double> input , int hPeriod, int lPeriod, Brush bearishColor, Brush bullishColor, bool showMTFCenterLine, HTFTimeframe higherTimeframe, DashStyleHelper mTFLineStyle, int mTFLineWidth, int mTFTickValue, int mTFRangeValue, bool showLTFCenterLine, LTFTimeframe lowerTimeframe, DashStyleHelper lTFLineStyle, int lTFLineWidth, int lTFTickValue, int lTFRangeValue)
		{
			return indicator.APlusCentreLine(input, hPeriod, lPeriod, bearishColor, bullishColor, showMTFCenterLine, higherTimeframe, mTFLineStyle, mTFLineWidth, mTFTickValue, mTFRangeValue, showLTFCenterLine, lowerTimeframe, lTFLineStyle, lTFLineWidth, lTFTickValue, lTFRangeValue);
		}

	}
}

#endregion
