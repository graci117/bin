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
		
		private A_Plus.S007Levels[] cacheS007Levels;

		
		public A_Plus.S007Levels S007Levels(string indicatorName, string upperTargets, string lowerTargets, Brush upperLineColor, Brush lowerLineColor, bool showLabels, bool showPrice, string upperLabel, string lowerLabel, int labelFontSize, int labelRightMargin, int lineThickness, DashStyleHelper lineStyle, bool showMidLine, string midLineLabel, Brush midLineColor, bool showMidZone, Brush midZoneColor, double midZonePercent, int midZoneOpacity, bool enableReversalSignals, int minBarsBeforeReversal, int arrowOffset, Brush upperArrowBrush, Brush lowerArrowBrush, string upperSecondaryTargets, string lowerSecondaryTargets, Brush upperSecondaryLineColor, Brush lowerSecondaryLineColor, string upperSecondaryLabel, string lowerSecondaryLabel, int secondaryLineThickness, DashStyleHelper secondaryLineStyle, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return S007Levels(Input, indicatorName, upperTargets, lowerTargets, upperLineColor, lowerLineColor, showLabels, showPrice, upperLabel, lowerLabel, labelFontSize, labelRightMargin, lineThickness, lineStyle, showMidLine, midLineLabel, midLineColor, showMidZone, midZoneColor, midZonePercent, midZoneOpacity, enableReversalSignals, minBarsBeforeReversal, arrowOffset, upperArrowBrush, lowerArrowBrush, upperSecondaryTargets, lowerSecondaryTargets, upperSecondaryLineColor, lowerSecondaryLineColor, upperSecondaryLabel, lowerSecondaryLabel, secondaryLineThickness, secondaryLineStyle, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}


		
		public A_Plus.S007Levels S007Levels(ISeries<double> input, string indicatorName, string upperTargets, string lowerTargets, Brush upperLineColor, Brush lowerLineColor, bool showLabels, bool showPrice, string upperLabel, string lowerLabel, int labelFontSize, int labelRightMargin, int lineThickness, DashStyleHelper lineStyle, bool showMidLine, string midLineLabel, Brush midLineColor, bool showMidZone, Brush midZoneColor, double midZonePercent, int midZoneOpacity, bool enableReversalSignals, int minBarsBeforeReversal, int arrowOffset, Brush upperArrowBrush, Brush lowerArrowBrush, string upperSecondaryTargets, string lowerSecondaryTargets, Brush upperSecondaryLineColor, Brush lowerSecondaryLineColor, string upperSecondaryLabel, string lowerSecondaryLabel, int secondaryLineThickness, DashStyleHelper secondaryLineStyle, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			if (cacheS007Levels != null)
				for (int idx = 0; idx < cacheS007Levels.Length; idx++)
					if (cacheS007Levels[idx].IndicatorName == indicatorName && cacheS007Levels[idx].UpperTargets == upperTargets && cacheS007Levels[idx].LowerTargets == lowerTargets && cacheS007Levels[idx].UpperLineColor == upperLineColor && cacheS007Levels[idx].LowerLineColor == lowerLineColor && cacheS007Levels[idx].ShowLabels == showLabels && cacheS007Levels[idx].ShowPrice == showPrice && cacheS007Levels[idx].UpperLabel == upperLabel && cacheS007Levels[idx].LowerLabel == lowerLabel && cacheS007Levels[idx].LabelFontSize == labelFontSize && cacheS007Levels[idx].LabelRightMargin == labelRightMargin && cacheS007Levels[idx].LineThickness == lineThickness && cacheS007Levels[idx].LineStyle == lineStyle && cacheS007Levels[idx].ShowMidLine == showMidLine && cacheS007Levels[idx].MidLineLabel == midLineLabel && cacheS007Levels[idx].MidLineColor == midLineColor && cacheS007Levels[idx].ShowMidZone == showMidZone && cacheS007Levels[idx].MidZoneColor == midZoneColor && cacheS007Levels[idx].MidZonePercent == midZonePercent && cacheS007Levels[idx].MidZoneOpacity == midZoneOpacity && cacheS007Levels[idx].EnableReversalSignals == enableReversalSignals && cacheS007Levels[idx].MinBarsBeforeReversal == minBarsBeforeReversal && cacheS007Levels[idx].ArrowOffset == arrowOffset && cacheS007Levels[idx].UpperArrowBrush == upperArrowBrush && cacheS007Levels[idx].LowerArrowBrush == lowerArrowBrush && cacheS007Levels[idx].UpperSecondaryTargets == upperSecondaryTargets && cacheS007Levels[idx].LowerSecondaryTargets == lowerSecondaryTargets && cacheS007Levels[idx].UpperSecondaryLineColor == upperSecondaryLineColor && cacheS007Levels[idx].LowerSecondaryLineColor == lowerSecondaryLineColor && cacheS007Levels[idx].UpperSecondaryLabel == upperSecondaryLabel && cacheS007Levels[idx].LowerSecondaryLabel == lowerSecondaryLabel && cacheS007Levels[idx].SecondaryLineThickness == secondaryLineThickness && cacheS007Levels[idx].SecondaryLineStyle == secondaryLineStyle && cacheS007Levels[idx].usePT == usePT && cacheS007Levels[idx].ProfitTarget == profitTarget && cacheS007Levels[idx].StopLoss == stopLoss && cacheS007Levels[idx].TPLineColor == tPLineColor && cacheS007Levels[idx].SLLineColor == sLLineColor && cacheS007Levels[idx].LineWidth == lineWidth && cacheS007Levels[idx].TPBuffer == tPBuffer && cacheS007Levels[idx].ShowTickValues == showTickValues && cacheS007Levels[idx].EqualsInput(input))
						return cacheS007Levels[idx];
			return CacheIndicator<A_Plus.S007Levels>(new A_Plus.S007Levels(){ IndicatorName = indicatorName, UpperTargets = upperTargets, LowerTargets = lowerTargets, UpperLineColor = upperLineColor, LowerLineColor = lowerLineColor, ShowLabels = showLabels, ShowPrice = showPrice, UpperLabel = upperLabel, LowerLabel = lowerLabel, LabelFontSize = labelFontSize, LabelRightMargin = labelRightMargin, LineThickness = lineThickness, LineStyle = lineStyle, ShowMidLine = showMidLine, MidLineLabel = midLineLabel, MidLineColor = midLineColor, ShowMidZone = showMidZone, MidZoneColor = midZoneColor, MidZonePercent = midZonePercent, MidZoneOpacity = midZoneOpacity, EnableReversalSignals = enableReversalSignals, MinBarsBeforeReversal = minBarsBeforeReversal, ArrowOffset = arrowOffset, UpperArrowBrush = upperArrowBrush, LowerArrowBrush = lowerArrowBrush, UpperSecondaryTargets = upperSecondaryTargets, LowerSecondaryTargets = lowerSecondaryTargets, UpperSecondaryLineColor = upperSecondaryLineColor, LowerSecondaryLineColor = lowerSecondaryLineColor, UpperSecondaryLabel = upperSecondaryLabel, LowerSecondaryLabel = lowerSecondaryLabel, SecondaryLineThickness = secondaryLineThickness, SecondaryLineStyle = secondaryLineStyle, usePT = usePT, ProfitTarget = profitTarget, StopLoss = stopLoss, TPLineColor = tPLineColor, SLLineColor = sLLineColor, LineWidth = lineWidth, TPBuffer = tPBuffer, ShowTickValues = showTickValues }, input, ref cacheS007Levels);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.S007Levels S007Levels(string indicatorName, string upperTargets, string lowerTargets, Brush upperLineColor, Brush lowerLineColor, bool showLabels, bool showPrice, string upperLabel, string lowerLabel, int labelFontSize, int labelRightMargin, int lineThickness, DashStyleHelper lineStyle, bool showMidLine, string midLineLabel, Brush midLineColor, bool showMidZone, Brush midZoneColor, double midZonePercent, int midZoneOpacity, bool enableReversalSignals, int minBarsBeforeReversal, int arrowOffset, Brush upperArrowBrush, Brush lowerArrowBrush, string upperSecondaryTargets, string lowerSecondaryTargets, Brush upperSecondaryLineColor, Brush lowerSecondaryLineColor, string upperSecondaryLabel, string lowerSecondaryLabel, int secondaryLineThickness, DashStyleHelper secondaryLineStyle, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.S007Levels(Input, indicatorName, upperTargets, lowerTargets, upperLineColor, lowerLineColor, showLabels, showPrice, upperLabel, lowerLabel, labelFontSize, labelRightMargin, lineThickness, lineStyle, showMidLine, midLineLabel, midLineColor, showMidZone, midZoneColor, midZonePercent, midZoneOpacity, enableReversalSignals, minBarsBeforeReversal, arrowOffset, upperArrowBrush, lowerArrowBrush, upperSecondaryTargets, lowerSecondaryTargets, upperSecondaryLineColor, lowerSecondaryLineColor, upperSecondaryLabel, lowerSecondaryLabel, secondaryLineThickness, secondaryLineStyle, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}


		
		public Indicators.A_Plus.S007Levels S007Levels(ISeries<double> input , string indicatorName, string upperTargets, string lowerTargets, Brush upperLineColor, Brush lowerLineColor, bool showLabels, bool showPrice, string upperLabel, string lowerLabel, int labelFontSize, int labelRightMargin, int lineThickness, DashStyleHelper lineStyle, bool showMidLine, string midLineLabel, Brush midLineColor, bool showMidZone, Brush midZoneColor, double midZonePercent, int midZoneOpacity, bool enableReversalSignals, int minBarsBeforeReversal, int arrowOffset, Brush upperArrowBrush, Brush lowerArrowBrush, string upperSecondaryTargets, string lowerSecondaryTargets, Brush upperSecondaryLineColor, Brush lowerSecondaryLineColor, string upperSecondaryLabel, string lowerSecondaryLabel, int secondaryLineThickness, DashStyleHelper secondaryLineStyle, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.S007Levels(input, indicatorName, upperTargets, lowerTargets, upperLineColor, lowerLineColor, showLabels, showPrice, upperLabel, lowerLabel, labelFontSize, labelRightMargin, lineThickness, lineStyle, showMidLine, midLineLabel, midLineColor, showMidZone, midZoneColor, midZonePercent, midZoneOpacity, enableReversalSignals, minBarsBeforeReversal, arrowOffset, upperArrowBrush, lowerArrowBrush, upperSecondaryTargets, lowerSecondaryTargets, upperSecondaryLineColor, lowerSecondaryLineColor, upperSecondaryLabel, lowerSecondaryLabel, secondaryLineThickness, secondaryLineStyle, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.S007Levels S007Levels(string indicatorName, string upperTargets, string lowerTargets, Brush upperLineColor, Brush lowerLineColor, bool showLabels, bool showPrice, string upperLabel, string lowerLabel, int labelFontSize, int labelRightMargin, int lineThickness, DashStyleHelper lineStyle, bool showMidLine, string midLineLabel, Brush midLineColor, bool showMidZone, Brush midZoneColor, double midZonePercent, int midZoneOpacity, bool enableReversalSignals, int minBarsBeforeReversal, int arrowOffset, Brush upperArrowBrush, Brush lowerArrowBrush, string upperSecondaryTargets, string lowerSecondaryTargets, Brush upperSecondaryLineColor, Brush lowerSecondaryLineColor, string upperSecondaryLabel, string lowerSecondaryLabel, int secondaryLineThickness, DashStyleHelper secondaryLineStyle, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.S007Levels(Input, indicatorName, upperTargets, lowerTargets, upperLineColor, lowerLineColor, showLabels, showPrice, upperLabel, lowerLabel, labelFontSize, labelRightMargin, lineThickness, lineStyle, showMidLine, midLineLabel, midLineColor, showMidZone, midZoneColor, midZonePercent, midZoneOpacity, enableReversalSignals, minBarsBeforeReversal, arrowOffset, upperArrowBrush, lowerArrowBrush, upperSecondaryTargets, lowerSecondaryTargets, upperSecondaryLineColor, lowerSecondaryLineColor, upperSecondaryLabel, lowerSecondaryLabel, secondaryLineThickness, secondaryLineStyle, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}


		
		public Indicators.A_Plus.S007Levels S007Levels(ISeries<double> input , string indicatorName, string upperTargets, string lowerTargets, Brush upperLineColor, Brush lowerLineColor, bool showLabels, bool showPrice, string upperLabel, string lowerLabel, int labelFontSize, int labelRightMargin, int lineThickness, DashStyleHelper lineStyle, bool showMidLine, string midLineLabel, Brush midLineColor, bool showMidZone, Brush midZoneColor, double midZonePercent, int midZoneOpacity, bool enableReversalSignals, int minBarsBeforeReversal, int arrowOffset, Brush upperArrowBrush, Brush lowerArrowBrush, string upperSecondaryTargets, string lowerSecondaryTargets, Brush upperSecondaryLineColor, Brush lowerSecondaryLineColor, string upperSecondaryLabel, string lowerSecondaryLabel, int secondaryLineThickness, DashStyleHelper secondaryLineStyle, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.S007Levels(input, indicatorName, upperTargets, lowerTargets, upperLineColor, lowerLineColor, showLabels, showPrice, upperLabel, lowerLabel, labelFontSize, labelRightMargin, lineThickness, lineStyle, showMidLine, midLineLabel, midLineColor, showMidZone, midZoneColor, midZonePercent, midZoneOpacity, enableReversalSignals, minBarsBeforeReversal, arrowOffset, upperArrowBrush, lowerArrowBrush, upperSecondaryTargets, lowerSecondaryTargets, upperSecondaryLineColor, lowerSecondaryLineColor, upperSecondaryLabel, lowerSecondaryLabel, secondaryLineThickness, secondaryLineStyle, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}

	}
}

#endregion
