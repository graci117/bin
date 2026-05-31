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
		
		private A_Plus.APlusNetChange[] cacheAPlusNetChange;
		private A_Plus.APlusNetChangeSignals[] cacheAPlusNetChangeSignals;

		
		public A_Plus.APlusNetChange APlusNetChange(DateTime endTime, bool showNQ, bool showES, bool showRTY, bool showGC, bool showSI, bool showEMAs)
		{
			return APlusNetChange(Input, endTime, showNQ, showES, showRTY, showGC, showSI, showEMAs);
		}

		public A_Plus.APlusNetChangeSignals APlusNetChangeSignals(bool enableJiggle, bool enableMullet, bool enableJiggleBarColoring, int arrowYOffset, bool mulletBarColor, System.Windows.Media.Brush barUpColor, System.Windows.Media.Brush barDnColor, SignalBandPreset bandPreset, double bandOffset, bool showCentreLine, System.Windows.Media.Brush upColor, System.Windows.Media.Brush dnColor, DashStyleHelper plot1Style, int plot1Width, bool showFastEMA, System.Windows.Media.Brush fastEMAUpColor, System.Windows.Media.Brush fastEMADnColor, DashStyleHelper fastEMAStyle, int fastEMAWidth, bool enableTradingShading, System.Windows.Media.Brush tradingTimeColor, bool tradingHours1, DateTime startTime1, DateTime endTime1, bool tradingHours2, DateTime startTime2, DateTime endTime2, bool tradingHours3, DateTime startTime3, DateTime endTime3, double tradeOpacity, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues, bool enableGannHLPanel, HTFSelection gannHTFTimeframe, int gannHPeriod, int gannLPeriod, int panelHeight, double panelOpacity)
		{
			return APlusNetChangeSignals(Input, enableJiggle, enableMullet, enableJiggleBarColoring, arrowYOffset, mulletBarColor, barUpColor, barDnColor, bandPreset, bandOffset, showCentreLine, upColor, dnColor, plot1Style, plot1Width, showFastEMA, fastEMAUpColor, fastEMADnColor, fastEMAStyle, fastEMAWidth, enableTradingShading, tradingTimeColor, tradingHours1, startTime1, endTime1, tradingHours2, startTime2, endTime2, tradingHours3, startTime3, endTime3, tradeOpacity, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues, enableGannHLPanel, gannHTFTimeframe, gannHPeriod, gannLPeriod, panelHeight, panelOpacity);
		}


		
		public A_Plus.APlusNetChange APlusNetChange(ISeries<double> input, DateTime endTime, bool showNQ, bool showES, bool showRTY, bool showGC, bool showSI, bool showEMAs)
		{
			if (cacheAPlusNetChange != null)
				for (int idx = 0; idx < cacheAPlusNetChange.Length; idx++)
					if (cacheAPlusNetChange[idx].EndTime == endTime && cacheAPlusNetChange[idx].ShowNQ == showNQ && cacheAPlusNetChange[idx].ShowES == showES && cacheAPlusNetChange[idx].ShowRTY == showRTY && cacheAPlusNetChange[idx].ShowGC == showGC && cacheAPlusNetChange[idx].ShowSI == showSI && cacheAPlusNetChange[idx].ShowEMAs == showEMAs && cacheAPlusNetChange[idx].EqualsInput(input))
						return cacheAPlusNetChange[idx];
			return CacheIndicator<A_Plus.APlusNetChange>(new A_Plus.APlusNetChange(){ EndTime = endTime, ShowNQ = showNQ, ShowES = showES, ShowRTY = showRTY, ShowGC = showGC, ShowSI = showSI, ShowEMAs = showEMAs }, input, ref cacheAPlusNetChange);
		}

		public A_Plus.APlusNetChangeSignals APlusNetChangeSignals(ISeries<double> input, bool enableJiggle, bool enableMullet, bool enableJiggleBarColoring, int arrowYOffset, bool mulletBarColor, System.Windows.Media.Brush barUpColor, System.Windows.Media.Brush barDnColor, SignalBandPreset bandPreset, double bandOffset, bool showCentreLine, System.Windows.Media.Brush upColor, System.Windows.Media.Brush dnColor, DashStyleHelper plot1Style, int plot1Width, bool showFastEMA, System.Windows.Media.Brush fastEMAUpColor, System.Windows.Media.Brush fastEMADnColor, DashStyleHelper fastEMAStyle, int fastEMAWidth, bool enableTradingShading, System.Windows.Media.Brush tradingTimeColor, bool tradingHours1, DateTime startTime1, DateTime endTime1, bool tradingHours2, DateTime startTime2, DateTime endTime2, bool tradingHours3, DateTime startTime3, DateTime endTime3, double tradeOpacity, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues, bool enableGannHLPanel, HTFSelection gannHTFTimeframe, int gannHPeriod, int gannLPeriod, int panelHeight, double panelOpacity)
		{
			if (cacheAPlusNetChangeSignals != null)
				for (int idx = 0; idx < cacheAPlusNetChangeSignals.Length; idx++)
					if (cacheAPlusNetChangeSignals[idx].EnableJiggle == enableJiggle && cacheAPlusNetChangeSignals[idx].EnableMullet == enableMullet && cacheAPlusNetChangeSignals[idx].EnableJiggleBarColoring == enableJiggleBarColoring && cacheAPlusNetChangeSignals[idx].ArrowYOffset == arrowYOffset && cacheAPlusNetChangeSignals[idx].MulletBarColor == mulletBarColor && cacheAPlusNetChangeSignals[idx].BarUpColor == barUpColor && cacheAPlusNetChangeSignals[idx].BarDnColor == barDnColor && cacheAPlusNetChangeSignals[idx].BandPreset == bandPreset && cacheAPlusNetChangeSignals[idx].BandOffset == bandOffset && cacheAPlusNetChangeSignals[idx].showCentreLine == showCentreLine && cacheAPlusNetChangeSignals[idx].UpColor == upColor && cacheAPlusNetChangeSignals[idx].DnColor == dnColor && cacheAPlusNetChangeSignals[idx].Plot1Style == plot1Style && cacheAPlusNetChangeSignals[idx].Plot1Width == plot1Width && cacheAPlusNetChangeSignals[idx].ShowFastEMA == showFastEMA && cacheAPlusNetChangeSignals[idx].FastEMAUpColor == fastEMAUpColor && cacheAPlusNetChangeSignals[idx].FastEMADnColor == fastEMADnColor && cacheAPlusNetChangeSignals[idx].FastEMAStyle == fastEMAStyle && cacheAPlusNetChangeSignals[idx].FastEMAWidth == fastEMAWidth && cacheAPlusNetChangeSignals[idx].EnableTradingShading == enableTradingShading && cacheAPlusNetChangeSignals[idx].TradingTimeColor == tradingTimeColor && cacheAPlusNetChangeSignals[idx].TradingHours1 == tradingHours1 && cacheAPlusNetChangeSignals[idx].StartTime1 == startTime1 && cacheAPlusNetChangeSignals[idx].EndTime1 == endTime1 && cacheAPlusNetChangeSignals[idx].TradingHours2 == tradingHours2 && cacheAPlusNetChangeSignals[idx].StartTime2 == startTime2 && cacheAPlusNetChangeSignals[idx].EndTime2 == endTime2 && cacheAPlusNetChangeSignals[idx].TradingHours3 == tradingHours3 && cacheAPlusNetChangeSignals[idx].StartTime3 == startTime3 && cacheAPlusNetChangeSignals[idx].EndTime3 == endTime3 && cacheAPlusNetChangeSignals[idx].TradeOpacity == tradeOpacity && cacheAPlusNetChangeSignals[idx].usePT == usePT && cacheAPlusNetChangeSignals[idx].ProfitTarget == profitTarget && cacheAPlusNetChangeSignals[idx].StopLoss == stopLoss && cacheAPlusNetChangeSignals[idx].TPLineColor == tPLineColor && cacheAPlusNetChangeSignals[idx].SLLineColor == sLLineColor && cacheAPlusNetChangeSignals[idx].LineWidth == lineWidth && cacheAPlusNetChangeSignals[idx].TPBuffer == tPBuffer && cacheAPlusNetChangeSignals[idx].ShowTickValues == showTickValues && cacheAPlusNetChangeSignals[idx].EnableGannHLPanel == enableGannHLPanel && cacheAPlusNetChangeSignals[idx].GannHTFTimeframe == gannHTFTimeframe && cacheAPlusNetChangeSignals[idx].GannHPeriod == gannHPeriod && cacheAPlusNetChangeSignals[idx].GannLPeriod == gannLPeriod && cacheAPlusNetChangeSignals[idx].PanelHeight == panelHeight && cacheAPlusNetChangeSignals[idx].PanelOpacity == panelOpacity && cacheAPlusNetChangeSignals[idx].EqualsInput(input))
						return cacheAPlusNetChangeSignals[idx];
			return CacheIndicator<A_Plus.APlusNetChangeSignals>(new A_Plus.APlusNetChangeSignals(){ EnableJiggle = enableJiggle, EnableMullet = enableMullet, EnableJiggleBarColoring = enableJiggleBarColoring, ArrowYOffset = arrowYOffset, MulletBarColor = mulletBarColor, BarUpColor = barUpColor, BarDnColor = barDnColor, BandPreset = bandPreset, BandOffset = bandOffset, showCentreLine = showCentreLine, UpColor = upColor, DnColor = dnColor, Plot1Style = plot1Style, Plot1Width = plot1Width, ShowFastEMA = showFastEMA, FastEMAUpColor = fastEMAUpColor, FastEMADnColor = fastEMADnColor, FastEMAStyle = fastEMAStyle, FastEMAWidth = fastEMAWidth, EnableTradingShading = enableTradingShading, TradingTimeColor = tradingTimeColor, TradingHours1 = tradingHours1, StartTime1 = startTime1, EndTime1 = endTime1, TradingHours2 = tradingHours2, StartTime2 = startTime2, EndTime2 = endTime2, TradingHours3 = tradingHours3, StartTime3 = startTime3, EndTime3 = endTime3, TradeOpacity = tradeOpacity, usePT = usePT, ProfitTarget = profitTarget, StopLoss = stopLoss, TPLineColor = tPLineColor, SLLineColor = sLLineColor, LineWidth = lineWidth, TPBuffer = tPBuffer, ShowTickValues = showTickValues, EnableGannHLPanel = enableGannHLPanel, GannHTFTimeframe = gannHTFTimeframe, GannHPeriod = gannHPeriod, GannLPeriod = gannLPeriod, PanelHeight = panelHeight, PanelOpacity = panelOpacity }, input, ref cacheAPlusNetChangeSignals);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.APlusNetChange APlusNetChange(DateTime endTime, bool showNQ, bool showES, bool showRTY, bool showGC, bool showSI, bool showEMAs)
		{
			return indicator.APlusNetChange(Input, endTime, showNQ, showES, showRTY, showGC, showSI, showEMAs);
		}

		public Indicators.A_Plus.APlusNetChangeSignals APlusNetChangeSignals(bool enableJiggle, bool enableMullet, bool enableJiggleBarColoring, int arrowYOffset, bool mulletBarColor, System.Windows.Media.Brush barUpColor, System.Windows.Media.Brush barDnColor, SignalBandPreset bandPreset, double bandOffset, bool showCentreLine, System.Windows.Media.Brush upColor, System.Windows.Media.Brush dnColor, DashStyleHelper plot1Style, int plot1Width, bool showFastEMA, System.Windows.Media.Brush fastEMAUpColor, System.Windows.Media.Brush fastEMADnColor, DashStyleHelper fastEMAStyle, int fastEMAWidth, bool enableTradingShading, System.Windows.Media.Brush tradingTimeColor, bool tradingHours1, DateTime startTime1, DateTime endTime1, bool tradingHours2, DateTime startTime2, DateTime endTime2, bool tradingHours3, DateTime startTime3, DateTime endTime3, double tradeOpacity, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues, bool enableGannHLPanel, HTFSelection gannHTFTimeframe, int gannHPeriod, int gannLPeriod, int panelHeight, double panelOpacity)
		{
			return indicator.APlusNetChangeSignals(Input, enableJiggle, enableMullet, enableJiggleBarColoring, arrowYOffset, mulletBarColor, barUpColor, barDnColor, bandPreset, bandOffset, showCentreLine, upColor, dnColor, plot1Style, plot1Width, showFastEMA, fastEMAUpColor, fastEMADnColor, fastEMAStyle, fastEMAWidth, enableTradingShading, tradingTimeColor, tradingHours1, startTime1, endTime1, tradingHours2, startTime2, endTime2, tradingHours3, startTime3, endTime3, tradeOpacity, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues, enableGannHLPanel, gannHTFTimeframe, gannHPeriod, gannLPeriod, panelHeight, panelOpacity);
		}


		
		public Indicators.A_Plus.APlusNetChange APlusNetChange(ISeries<double> input , DateTime endTime, bool showNQ, bool showES, bool showRTY, bool showGC, bool showSI, bool showEMAs)
		{
			return indicator.APlusNetChange(input, endTime, showNQ, showES, showRTY, showGC, showSI, showEMAs);
		}

		public Indicators.A_Plus.APlusNetChangeSignals APlusNetChangeSignals(ISeries<double> input , bool enableJiggle, bool enableMullet, bool enableJiggleBarColoring, int arrowYOffset, bool mulletBarColor, System.Windows.Media.Brush barUpColor, System.Windows.Media.Brush barDnColor, SignalBandPreset bandPreset, double bandOffset, bool showCentreLine, System.Windows.Media.Brush upColor, System.Windows.Media.Brush dnColor, DashStyleHelper plot1Style, int plot1Width, bool showFastEMA, System.Windows.Media.Brush fastEMAUpColor, System.Windows.Media.Brush fastEMADnColor, DashStyleHelper fastEMAStyle, int fastEMAWidth, bool enableTradingShading, System.Windows.Media.Brush tradingTimeColor, bool tradingHours1, DateTime startTime1, DateTime endTime1, bool tradingHours2, DateTime startTime2, DateTime endTime2, bool tradingHours3, DateTime startTime3, DateTime endTime3, double tradeOpacity, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues, bool enableGannHLPanel, HTFSelection gannHTFTimeframe, int gannHPeriod, int gannLPeriod, int panelHeight, double panelOpacity)
		{
			return indicator.APlusNetChangeSignals(input, enableJiggle, enableMullet, enableJiggleBarColoring, arrowYOffset, mulletBarColor, barUpColor, barDnColor, bandPreset, bandOffset, showCentreLine, upColor, dnColor, plot1Style, plot1Width, showFastEMA, fastEMAUpColor, fastEMADnColor, fastEMAStyle, fastEMAWidth, enableTradingShading, tradingTimeColor, tradingHours1, startTime1, endTime1, tradingHours2, startTime2, endTime2, tradingHours3, startTime3, endTime3, tradeOpacity, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues, enableGannHLPanel, gannHTFTimeframe, gannHPeriod, gannLPeriod, panelHeight, panelOpacity);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.APlusNetChange APlusNetChange(DateTime endTime, bool showNQ, bool showES, bool showRTY, bool showGC, bool showSI, bool showEMAs)
		{
			return indicator.APlusNetChange(Input, endTime, showNQ, showES, showRTY, showGC, showSI, showEMAs);
		}

		public Indicators.A_Plus.APlusNetChangeSignals APlusNetChangeSignals(bool enableJiggle, bool enableMullet, bool enableJiggleBarColoring, int arrowYOffset, bool mulletBarColor, System.Windows.Media.Brush barUpColor, System.Windows.Media.Brush barDnColor, SignalBandPreset bandPreset, double bandOffset, bool showCentreLine, System.Windows.Media.Brush upColor, System.Windows.Media.Brush dnColor, DashStyleHelper plot1Style, int plot1Width, bool showFastEMA, System.Windows.Media.Brush fastEMAUpColor, System.Windows.Media.Brush fastEMADnColor, DashStyleHelper fastEMAStyle, int fastEMAWidth, bool enableTradingShading, System.Windows.Media.Brush tradingTimeColor, bool tradingHours1, DateTime startTime1, DateTime endTime1, bool tradingHours2, DateTime startTime2, DateTime endTime2, bool tradingHours3, DateTime startTime3, DateTime endTime3, double tradeOpacity, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues, bool enableGannHLPanel, HTFSelection gannHTFTimeframe, int gannHPeriod, int gannLPeriod, int panelHeight, double panelOpacity)
		{
			return indicator.APlusNetChangeSignals(Input, enableJiggle, enableMullet, enableJiggleBarColoring, arrowYOffset, mulletBarColor, barUpColor, barDnColor, bandPreset, bandOffset, showCentreLine, upColor, dnColor, plot1Style, plot1Width, showFastEMA, fastEMAUpColor, fastEMADnColor, fastEMAStyle, fastEMAWidth, enableTradingShading, tradingTimeColor, tradingHours1, startTime1, endTime1, tradingHours2, startTime2, endTime2, tradingHours3, startTime3, endTime3, tradeOpacity, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues, enableGannHLPanel, gannHTFTimeframe, gannHPeriod, gannLPeriod, panelHeight, panelOpacity);
		}


		
		public Indicators.A_Plus.APlusNetChange APlusNetChange(ISeries<double> input , DateTime endTime, bool showNQ, bool showES, bool showRTY, bool showGC, bool showSI, bool showEMAs)
		{
			return indicator.APlusNetChange(input, endTime, showNQ, showES, showRTY, showGC, showSI, showEMAs);
		}

		public Indicators.A_Plus.APlusNetChangeSignals APlusNetChangeSignals(ISeries<double> input , bool enableJiggle, bool enableMullet, bool enableJiggleBarColoring, int arrowYOffset, bool mulletBarColor, System.Windows.Media.Brush barUpColor, System.Windows.Media.Brush barDnColor, SignalBandPreset bandPreset, double bandOffset, bool showCentreLine, System.Windows.Media.Brush upColor, System.Windows.Media.Brush dnColor, DashStyleHelper plot1Style, int plot1Width, bool showFastEMA, System.Windows.Media.Brush fastEMAUpColor, System.Windows.Media.Brush fastEMADnColor, DashStyleHelper fastEMAStyle, int fastEMAWidth, bool enableTradingShading, System.Windows.Media.Brush tradingTimeColor, bool tradingHours1, DateTime startTime1, DateTime endTime1, bool tradingHours2, DateTime startTime2, DateTime endTime2, bool tradingHours3, DateTime startTime3, DateTime endTime3, double tradeOpacity, bool usePT, int profitTarget, int stopLoss, Brush tPLineColor, Brush sLLineColor, int lineWidth, int tPBuffer, bool showTickValues, bool enableGannHLPanel, HTFSelection gannHTFTimeframe, int gannHPeriod, int gannLPeriod, int panelHeight, double panelOpacity)
		{
			return indicator.APlusNetChangeSignals(input, enableJiggle, enableMullet, enableJiggleBarColoring, arrowYOffset, mulletBarColor, barUpColor, barDnColor, bandPreset, bandOffset, showCentreLine, upColor, dnColor, plot1Style, plot1Width, showFastEMA, fastEMAUpColor, fastEMADnColor, fastEMAStyle, fastEMAWidth, enableTradingShading, tradingTimeColor, tradingHours1, startTime1, endTime1, tradingHours2, startTime2, endTime2, tradingHours3, startTime3, endTime3, tradeOpacity, usePT, profitTarget, stopLoss, tPLineColor, sLLineColor, lineWidth, tPBuffer, showTickValues, enableGannHLPanel, gannHTFTimeframe, gannHPeriod, gannLPeriod, panelHeight, panelOpacity);
		}

	}
}

#endregion
