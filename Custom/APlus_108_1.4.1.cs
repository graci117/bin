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
		
		private A_Plus.APlus108[] cacheAPlus108;

		
		public A_Plus.APlus108 APlus108(bool st_108, bool st_108Panel, bool st_108216, bool johnWick, bool allSignals, bool showEMA1, int eMA1Period, DashStyleHelper plotEMA1Style, int plotEMA1Width, Brush eMA1Color, bool showEMA2, int eMA2Period, DashStyleHelper plotEMA2Style, int plotEMA2Width, Brush eMA2Color, bool useSlope, Brush upColor, Brush dnColor, bool colorBackground, double backgroundOpacity, Brush bullishBackgroundColor, Brush bearishBackgroundColor, bool useEMALin1, DashStyleHelper plot1Style, int plot1Width, bool showPanel, bool useEMALin2, DashStyleHelper plot2Style, int plot2Width, bool useEMALinBands, BandPreset108 bandPreset, double bandOffset, DashStyleHelper bandStyle, int bandWidth, Brush bandColor, bool barColor, Brush barUpColor, Brush barDnColor, bool sBBarColor, Brush sBColor, int offset, int buffer, Brush sigBullColor, Brush sigBearColor, Brush wickColor, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return APlus108(Input, st_108, st_108Panel, st_108216, johnWick, allSignals, showEMA1, eMA1Period, plotEMA1Style, plotEMA1Width, eMA1Color, showEMA2, eMA2Period, plotEMA2Style, plotEMA2Width, eMA2Color, useSlope, upColor, dnColor, colorBackground, backgroundOpacity, bullishBackgroundColor, bearishBackgroundColor, useEMALin1, plot1Style, plot1Width, showPanel, useEMALin2, plot2Style, plot2Width, useEMALinBands, bandPreset, bandOffset, bandStyle, bandWidth, bandColor, barColor, barUpColor, barDnColor, sBBarColor, sBColor, offset, buffer, sigBullColor, sigBearColor, wickColor, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}


		
		public A_Plus.APlus108 APlus108(ISeries<double> input, bool st_108, bool st_108Panel, bool st_108216, bool johnWick, bool allSignals, bool showEMA1, int eMA1Period, DashStyleHelper plotEMA1Style, int plotEMA1Width, Brush eMA1Color, bool showEMA2, int eMA2Period, DashStyleHelper plotEMA2Style, int plotEMA2Width, Brush eMA2Color, bool useSlope, Brush upColor, Brush dnColor, bool colorBackground, double backgroundOpacity, Brush bullishBackgroundColor, Brush bearishBackgroundColor, bool useEMALin1, DashStyleHelper plot1Style, int plot1Width, bool showPanel, bool useEMALin2, DashStyleHelper plot2Style, int plot2Width, bool useEMALinBands, BandPreset108 bandPreset, double bandOffset, DashStyleHelper bandStyle, int bandWidth, Brush bandColor, bool barColor, Brush barUpColor, Brush barDnColor, bool sBBarColor, Brush sBColor, int offset, int buffer, Brush sigBullColor, Brush sigBearColor, Brush wickColor, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			if (cacheAPlus108 != null)
				for (int idx = 0; idx < cacheAPlus108.Length; idx++)
					if (cacheAPlus108[idx].St_108 == st_108 && cacheAPlus108[idx].St_108Panel == st_108Panel && cacheAPlus108[idx].St_108216 == st_108216 && cacheAPlus108[idx].JohnWick == johnWick && cacheAPlus108[idx].AllSignals == allSignals && cacheAPlus108[idx].showEMA1 == showEMA1 && cacheAPlus108[idx].EMA1Period == eMA1Period && cacheAPlus108[idx].PlotEMA1Style == plotEMA1Style && cacheAPlus108[idx].PlotEMA1Width == plotEMA1Width && cacheAPlus108[idx].EMA1Color == eMA1Color && cacheAPlus108[idx].showEMA2 == showEMA2 && cacheAPlus108[idx].EMA2Period == eMA2Period && cacheAPlus108[idx].PlotEMA2Style == plotEMA2Style && cacheAPlus108[idx].PlotEMA2Width == plotEMA2Width && cacheAPlus108[idx].EMA2Color == eMA2Color && cacheAPlus108[idx].useSlope == useSlope && cacheAPlus108[idx].UpColor == upColor && cacheAPlus108[idx].DnColor == dnColor && cacheAPlus108[idx].colorBackground == colorBackground && cacheAPlus108[idx].BackgroundOpacity == backgroundOpacity && cacheAPlus108[idx].BullishBackgroundColor == bullishBackgroundColor && cacheAPlus108[idx].BearishBackgroundColor == bearishBackgroundColor && cacheAPlus108[idx].useEMALin1 == useEMALin1 && cacheAPlus108[idx].Plot1Style == plot1Style && cacheAPlus108[idx].Plot1Width == plot1Width && cacheAPlus108[idx].ShowPanel == showPanel && cacheAPlus108[idx].useEMALin2 == useEMALin2 && cacheAPlus108[idx].Plot2Style == plot2Style && cacheAPlus108[idx].Plot2Width == plot2Width && cacheAPlus108[idx].useEMALinBands == useEMALinBands && cacheAPlus108[idx].BandPreset == bandPreset && cacheAPlus108[idx].BandOffset == bandOffset && cacheAPlus108[idx].BandStyle == bandStyle && cacheAPlus108[idx].BandWidth == bandWidth && cacheAPlus108[idx].BandColor == bandColor && cacheAPlus108[idx].BarColor == barColor && cacheAPlus108[idx].BarUpColor == barUpColor && cacheAPlus108[idx].BarDnColor == barDnColor && cacheAPlus108[idx].SBBarColor == sBBarColor && cacheAPlus108[idx].SBColor == sBColor && cacheAPlus108[idx].Offset == offset && cacheAPlus108[idx].Buffer == buffer && cacheAPlus108[idx].SigBullColor == sigBullColor && cacheAPlus108[idx].SigBearColor == sigBearColor && cacheAPlus108[idx].WickColor == wickColor && cacheAPlus108[idx].usePT == usePT && cacheAPlus108[idx].ProfitTarget == profitTarget && cacheAPlus108[idx].StopLoss == stopLoss && cacheAPlus108[idx].TPLineColor == tPLineColor && cacheAPlus108[idx].SLLineColor == sLLineColor && cacheAPlus108[idx].LineWidth == lineWidth && cacheAPlus108[idx].TPBuffer == tPBuffer && cacheAPlus108[idx].ShowTickValues == showTickValues && cacheAPlus108[idx].EqualsInput(input))
						return cacheAPlus108[idx];
			return CacheIndicator<A_Plus.APlus108>(new A_Plus.APlus108(){ St_108 = st_108, St_108Panel = st_108Panel, St_108216 = st_108216, JohnWick = johnWick, AllSignals = allSignals, showEMA1 = showEMA1, EMA1Period = eMA1Period, PlotEMA1Style = plotEMA1Style, PlotEMA1Width = plotEMA1Width, EMA1Color = eMA1Color, showEMA2 = showEMA2, EMA2Period = eMA2Period, PlotEMA2Style = plotEMA2Style, PlotEMA2Width = plotEMA2Width, EMA2Color = eMA2Color, useSlope = useSlope, UpColor = upColor, DnColor = dnColor, colorBackground = colorBackground, BackgroundOpacity = backgroundOpacity, BullishBackgroundColor = bullishBackgroundColor, BearishBackgroundColor = bearishBackgroundColor, useEMALin1 = useEMALin1, Plot1Style = plot1Style, Plot1Width = plot1Width, ShowPanel = showPanel, useEMALin2 = useEMALin2, Plot2Style = plot2Style, Plot2Width = plot2Width, useEMALinBands = useEMALinBands, BandPreset = bandPreset, BandOffset = bandOffset, BandStyle = bandStyle, BandWidth = bandWidth, BandColor = bandColor, BarColor = barColor, BarUpColor = barUpColor, BarDnColor = barDnColor, SBBarColor = sBBarColor, SBColor = sBColor, Offset = offset, Buffer = buffer, SigBullColor = sigBullColor, SigBearColor = sigBearColor, WickColor = wickColor, usePT = usePT, ProfitTarget = profitTarget, StopLoss = stopLoss, TPLineColor = tPLineColor, SLLineColor = sLLineColor, LineWidth = lineWidth, TPBuffer = tPBuffer, ShowTickValues = showTickValues }, input, ref cacheAPlus108);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.APlus108 APlus108(bool st_108, bool st_108Panel, bool st_108216, bool johnWick, bool allSignals, bool showEMA1, int eMA1Period, DashStyleHelper plotEMA1Style, int plotEMA1Width, Brush eMA1Color, bool showEMA2, int eMA2Period, DashStyleHelper plotEMA2Style, int plotEMA2Width, Brush eMA2Color, bool useSlope, Brush upColor, Brush dnColor, bool colorBackground, double backgroundOpacity, Brush bullishBackgroundColor, Brush bearishBackgroundColor, bool useEMALin1, DashStyleHelper plot1Style, int plot1Width, bool showPanel, bool useEMALin2, DashStyleHelper plot2Style, int plot2Width, bool useEMALinBands, BandPreset108 bandPreset, double bandOffset, DashStyleHelper bandStyle, int bandWidth, Brush bandColor, bool barColor, Brush barUpColor, Brush barDnColor, bool sBBarColor, Brush sBColor, int offset, int buffer, Brush sigBullColor, Brush sigBearColor, Brush wickColor, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.APlus108(Input, st_108, st_108Panel, st_108216, johnWick, allSignals, showEMA1, eMA1Period, plotEMA1Style, plotEMA1Width, eMA1Color, showEMA2, eMA2Period, plotEMA2Style, plotEMA2Width, eMA2Color, useSlope, upColor, dnColor, colorBackground, backgroundOpacity, bullishBackgroundColor, bearishBackgroundColor, useEMALin1, plot1Style, plot1Width, showPanel, useEMALin2, plot2Style, plot2Width, useEMALinBands, bandPreset, bandOffset, bandStyle, bandWidth, bandColor, barColor, barUpColor, barDnColor, sBBarColor, sBColor, offset, buffer, sigBullColor, sigBearColor, wickColor, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}


		
		public Indicators.A_Plus.APlus108 APlus108(ISeries<double> input , bool st_108, bool st_108Panel, bool st_108216, bool johnWick, bool allSignals, bool showEMA1, int eMA1Period, DashStyleHelper plotEMA1Style, int plotEMA1Width, Brush eMA1Color, bool showEMA2, int eMA2Period, DashStyleHelper plotEMA2Style, int plotEMA2Width, Brush eMA2Color, bool useSlope, Brush upColor, Brush dnColor, bool colorBackground, double backgroundOpacity, Brush bullishBackgroundColor, Brush bearishBackgroundColor, bool useEMALin1, DashStyleHelper plot1Style, int plot1Width, bool showPanel, bool useEMALin2, DashStyleHelper plot2Style, int plot2Width, bool useEMALinBands, BandPreset108 bandPreset, double bandOffset, DashStyleHelper bandStyle, int bandWidth, Brush bandColor, bool barColor, Brush barUpColor, Brush barDnColor, bool sBBarColor, Brush sBColor, int offset, int buffer, Brush sigBullColor, Brush sigBearColor, Brush wickColor, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.APlus108(input, st_108, st_108Panel, st_108216, johnWick, allSignals, showEMA1, eMA1Period, plotEMA1Style, plotEMA1Width, eMA1Color, showEMA2, eMA2Period, plotEMA2Style, plotEMA2Width, eMA2Color, useSlope, upColor, dnColor, colorBackground, backgroundOpacity, bullishBackgroundColor, bearishBackgroundColor, useEMALin1, plot1Style, plot1Width, showPanel, useEMALin2, plot2Style, plot2Width, useEMALinBands, bandPreset, bandOffset, bandStyle, bandWidth, bandColor, barColor, barUpColor, barDnColor, sBBarColor, sBColor, offset, buffer, sigBullColor, sigBearColor, wickColor, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.APlus108 APlus108(bool st_108, bool st_108Panel, bool st_108216, bool johnWick, bool allSignals, bool showEMA1, int eMA1Period, DashStyleHelper plotEMA1Style, int plotEMA1Width, Brush eMA1Color, bool showEMA2, int eMA2Period, DashStyleHelper plotEMA2Style, int plotEMA2Width, Brush eMA2Color, bool useSlope, Brush upColor, Brush dnColor, bool colorBackground, double backgroundOpacity, Brush bullishBackgroundColor, Brush bearishBackgroundColor, bool useEMALin1, DashStyleHelper plot1Style, int plot1Width, bool showPanel, bool useEMALin2, DashStyleHelper plot2Style, int plot2Width, bool useEMALinBands, BandPreset108 bandPreset, double bandOffset, DashStyleHelper bandStyle, int bandWidth, Brush bandColor, bool barColor, Brush barUpColor, Brush barDnColor, bool sBBarColor, Brush sBColor, int offset, int buffer, Brush sigBullColor, Brush sigBearColor, Brush wickColor, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.APlus108(Input, st_108, st_108Panel, st_108216, johnWick, allSignals, showEMA1, eMA1Period, plotEMA1Style, plotEMA1Width, eMA1Color, showEMA2, eMA2Period, plotEMA2Style, plotEMA2Width, eMA2Color, useSlope, upColor, dnColor, colorBackground, backgroundOpacity, bullishBackgroundColor, bearishBackgroundColor, useEMALin1, plot1Style, plot1Width, showPanel, useEMALin2, plot2Style, plot2Width, useEMALinBands, bandPreset, bandOffset, bandStyle, bandWidth, bandColor, barColor, barUpColor, barDnColor, sBBarColor, sBColor, offset, buffer, sigBullColor, sigBearColor, wickColor, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}


		
		public Indicators.A_Plus.APlus108 APlus108(ISeries<double> input , bool st_108, bool st_108Panel, bool st_108216, bool johnWick, bool allSignals, bool showEMA1, int eMA1Period, DashStyleHelper plotEMA1Style, int plotEMA1Width, Brush eMA1Color, bool showEMA2, int eMA2Period, DashStyleHelper plotEMA2Style, int plotEMA2Width, Brush eMA2Color, bool useSlope, Brush upColor, Brush dnColor, bool colorBackground, double backgroundOpacity, Brush bullishBackgroundColor, Brush bearishBackgroundColor, bool useEMALin1, DashStyleHelper plot1Style, int plot1Width, bool showPanel, bool useEMALin2, DashStyleHelper plot2Style, int plot2Width, bool useEMALinBands, BandPreset108 bandPreset, double bandOffset, DashStyleHelper bandStyle, int bandWidth, Brush bandColor, bool barColor, Brush barUpColor, Brush barDnColor, bool sBBarColor, Brush sBColor, int offset, int buffer, Brush sigBullColor, Brush sigBearColor, Brush wickColor, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues)
		{
			return indicator.APlus108(input, st_108, st_108Panel, st_108216, johnWick, allSignals, showEMA1, eMA1Period, plotEMA1Style, plotEMA1Width, eMA1Color, showEMA2, eMA2Period, plotEMA2Style, plotEMA2Width, eMA2Color, useSlope, upColor, dnColor, colorBackground, backgroundOpacity, bullishBackgroundColor, bearishBackgroundColor, useEMALin1, plot1Style, plot1Width, showPanel, useEMALin2, plot2Style, plot2Width, useEMALinBands, bandPreset, bandOffset, bandStyle, bandWidth, bandColor, barColor, barUpColor, barDnColor, sBBarColor, sBColor, offset, buffer, sigBullColor, sigBearColor, wickColor, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues);
		}

	}
}

#endregion
