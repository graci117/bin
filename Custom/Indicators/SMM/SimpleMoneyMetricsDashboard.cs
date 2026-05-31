//////////////////////////////////////////////////////////////////////////////////////////////////////
// 	Indicator	: 	SimpleMoneyMetricsDashboard														//
//	Description	:	Converted from Simple Money Metrics v4 indicator from TradingView.				//
//	Author		:	Adheer Pai (firstlanetech@gmail.com)											//
//	History		:																					//
//		15-Nov-2024		1.00	Initial version converted from Trading View.						//
//		19-Nov-20224	1.01	Bug-fix : Changed _currentMode from private variable to series		//
//								to fix refresh issues.												//
//		24-Nov-2024		1.02	Optimization and code cleanup.										//
//		25-Nov-2024		1.03	Code cleanup.														//
//		29-Nov-2024		1.04	Bug-fix of signal refresh.											//
//		04-Dec-2024		1.05	BUG-FIX : Buy/sell signal refresh issue. Reset Switch plot.			//
//		10-Feb-2025		1.06	Added Moving Average filter for trade signals.						//
//////////////////////////////////////////////////////////////////////////////////////////////////////

#region References

using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using System;

using SmmCustom = SimpleMoneyMetricsCommon;

#endregion

//////////////////////////////////////////////////////////////////////////////////////////////////////
// NinjaTrader.NinjaScript.Indicators
//////////////////////////////////////////////////////////////////////////////////////////////////////
namespace NinjaTrader.NinjaScript.Indicators
{
	[Gui.CategoryOrder("Signal Settings", 1)]
	[Gui.CategoryOrder("Profit Settings", 2)]
	[Gui.CategoryOrder("Background TrendSwitch Settings", 3)]
	[Gui.CategoryOrder("Candle Settings", 4)]
	[Gui.CategoryOrder("Support & Resistance Settings", 5)]
	[Gui.CategoryOrder("Real Price Settings", 6)]
	[Gui.CategoryOrder("Price Channel Settings", 7)]
	[Gui.CategoryOrder("Moving Average Filter", 8)]
	[Gui.CategoryOrder("Dashboard Settings", 9)]

	//////////////////////////////////////////////////////////////////////////////////////////////////////
	// SimpleMoneyMetricsDashboard indicator.
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	public class SimpleMoneyMetricsDashboard : Indicator
	{
		private const int PERIOD_FAST_EMA 	= 8;
		private const int PERIOD_SLOW_EMA 	= 21;
		private const int PERIOD_ATR		= 8;
		private const int PERIOD_MFI 		= 10;

		private SmmCustom.CustomPriceSeries _price = null, _ha = null, _src = null;
		private SmmCustom.ExponentialMA _fastEma = null, _slowEma = null;
		private SmmCustom.AverageTrueRange _atr = null;
		private Series<double> _trendUp = null, _trendDown = null;
		private Series<int> _trendSwitch = null;
		private Series<Brush> _backgroundTrendColor = null;
		private SmmCustom.MoneyFlowIndex _mfi = null;
		private Series<SmmCustom.EMode> _currentMode = null;

		private Brush _brushCenterDotsBullishColor = null, _brushCenterDotsBearishColor = null;

		// Graphics brushes.
		private SharpDX.Direct2D1.SolidColorBrush _brushAbove = null, _brushBelow = null;

		private const int PLOT_INDEX_MFI = 0;
		private const int PLOT_INDEX_CENTER = PLOT_INDEX_MFI + 1;
		private const int PLOT_INDEX_SIGNAL = PLOT_INDEX_CENTER + 1;

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// OnStateChange
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		protected override void OnStateChange()
		{
			if( State == State.SetDefaults )
			{
				Description									= @"The Simple Money Metrics (SMM) Dashboard indicator.";
				Name										= "SMM - Dashboard";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				MaximumBarsLookBack							= MaximumBarsLookBack.Infinite;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;

				//Disable this property if your indicator requires custom values that cumulate with each new market data event.
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				// Signal Settings defaults.
				SS_UseHeikenAshi = true;
				SS_EnableBuySellSignals = true;
				SS_EnableChopFilter = true;

				// Additional indicator settings.
				IN_MaEnabled = true;
				IN_MaPeriod = 10;
				IN_MaMethod = SmmCustom.EMovingAverageType.SMA;
				IN_MaSource = SmmCustom.EDataSource.Price;

				// Dashboard Settings
				DB_TrendDotsSize = 1;
				DB_CrossoverDotsSize = 3;
				DB_MfiBullishColor = Brushes.Lime;
				DB_MfiBearishColor = Brushes.Red;

				AddPlot(new Stroke(Brushes.Blue), PlotStyle.Square, "MFI");
				AddPlot(new Stroke(Brushes.Red), PlotStyle.Dot, "Center");
				AddPlot(new Stroke(Brushes.White, 4), PlotStyle.Dot, "Switch");
			}
			else if( State == State.Configure )
			{

			}
			else if( State == State.DataLoaded )
			{
				_price = new SmmCustom.CustomPriceSeries(this);
				if( SS_UseHeikenAshi ) _ha = new SmmCustom.CustomPriceSeries(this);
				_src = new SmmCustom.CustomPriceSeries(this);

				_fastEma = new SmmCustom.ExponentialMA(this, _src.Close, PERIOD_FAST_EMA);
				_slowEma = new SmmCustom.ExponentialMA(this, _src.Close, PERIOD_SLOW_EMA);

				_atr = new SmmCustom.AverageTrueRange(ref _src, PERIOD_ATR);

				_trendUp = new Series<double>(this);
				_trendDown = new Series<double>(this);
				_trendSwitch = new Series<int>(this);

				_backgroundTrendColor = new Series<Brush>(this);
				_mfi = new SmmCustom.MoneyFlowIndex(this, ref _src.HLC3, _price.Volume, PERIOD_MFI);
				_currentMode = new Series<SmmCustom.EMode>(this);

				_brushCenterDotsBullishColor = new SolidColorBrush(Color.FromRgb(102, 255, 0));
				_brushCenterDotsBullishColor.Freeze();
				_brushCenterDotsBearishColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
				_brushCenterDotsBearishColor.Freeze();

				Plots[PLOT_INDEX_CENTER].Width = DB_TrendDotsSize;
				Plots[PLOT_INDEX_SIGNAL].Width = DB_CrossoverDotsSize;
				Plots[PLOT_INDEX_SIGNAL].Brush = Brushes.Transparent;
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// OnBarUpdate
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		protected override void OnBarUpdate()
		{
			_price.Update(); 				// Update the base price (OHLC) candles.
			if( SS_UseHeikenAshi )
			{
				_ha.UpdateHeikinAshi(ref _price);		// Build Heikin Ashi candles from base price series.
				_src.Update(ref _ha);			// Copy Heikin Ashi data to source series.
			}
			else
				_src.Update(ref _price);		// Copy base price data to source series.

			double realClose = _price.Close[0];
			double profitWaveEmaFast = _fastEma.Calculate();
			double profitWaveEmaSlow = _slowEma.Calculate();

			double close1 = (CurrentBar > 0 ? _src.Close[1] : _src.Close[0]);
			double high1  = (CurrentBar > 0 ? _src.High[1]  : _src.High[0]);
			double low1   = (CurrentBar > 0 ? _src.Low[1]   : _src.Low[0]);

			double trueRange = Math.Max(Math.Max(_src.High[0] - _src.Low[0], Math.Abs(_src.High[0] - close1)), Math.Abs(_src.Low[0] - close1));
			double diPlusCalculation  = (_src.High[0] - high1 > low1 - _src.Low[0] ? Math.Max(_src.High[0] - high1, 0.0) : 0.0);
			double diMinusCalculation = (low1 - _src.Low[0] > _src.High[0] - high1 ? Math.Max(low1 - _src.Low[0], 0.0): 0.0);

			double smoothedTrueRange = trueRange;
			double smoothedDiPlus    = diPlusCalculation;
			double smoothedDiMinus   = diMinusCalculation;

			double diPlus  = (smoothedTrueRange != 0.0 ? smoothedDiPlus / smoothedTrueRange * 100.0 : 0.0);
			double diMinus = (smoothedTrueRange != 0.0 ? smoothedDiMinus / smoothedTrueRange * 100.0 : 0.0);

			_atr.Calculate();
			double Up = (_src.High[0] + _src.Low[0]) / 2.0 - (1.3 * _atr.ATR[0]);
			double Dn = (_src.High[0] + _src.Low[0]) / 2.0 + (1.3 * _atr.ATR[0]);

			_trendUp[0] = (CurrentBar > 0 ? (_src.Close[1] > _trendUp[1] ? Math.Max(Up, _trendUp[1]) : Up) : Up);
			_trendDown[0] = (CurrentBar > 0 ? (_src.Close[1] < _trendDown[1] ? Math.Min(Dn, _trendDown[1]) : Dn) : Dn);
			_trendSwitch[0] = (CurrentBar > 0 ? (_src.Close[0] > _trendDown[1] ? 1 : (_src.Close[0] < _trendUp[1] ? -1 : _trendSwitch[1])) : 1);

			bool bullishTrend = (_trendSwitch[0] == 1);
			bool bearishTrend = !bullishTrend;

			_backgroundTrendColor[0] = Brushes.White;
			if( bullishTrend )
			    _backgroundTrendColor[0] = Brushes.Lime;
			else if( bearishTrend )
			    _backgroundTrendColor[0] = Brushes.Red;
			else if( CurrentBar > 0 )
			    _backgroundTrendColor[0] = _backgroundTrendColor[1];

			bool buySignal = bullishTrend;
			bool sellSignal = bearishTrend;
			MoneyFlowIndex[0] = _mfi.Calculate();

			if( CurrentBar > 0 )
			{
				if( _backgroundTrendColor[0] != _backgroundTrendColor[1] )
					_currentMode[0] = SmmCustom.EMode.None;
				else
					_currentMode[0] = _currentMode[1];
			}
			else
				_currentMode[0] = SmmCustom.EMode.None;

			bool strongBullishCandle = (_src.Close[0] > _src.Open[0]) && (_src.Open[0] == _src.Low[0]) &&
									   (_price.Close[0] > profitWaveEmaFast) && (_price.Close[0] > profitWaveEmaSlow);
			bool strongBearishCandle = (_src.Close[0] < _src.Open[0]) && (_src.Open[0] == _src.High[0]) &&
									   (_price.Close[0] < profitWaveEmaFast) && (_price.Close[0] < profitWaveEmaSlow);

			bool canBuy = true, canSell = true;
			if( SS_EnableChopFilter )
			{
				canBuy  = (Math.Floor(diPlus)  > Math.Floor(diMinus)) && (Math.Floor(diPlus)  >= 45.0);
			    canSell = (Math.Floor(diMinus) > Math.Floor(diPlus))  && (Math.Floor(diMinus) >= 45.0);
			}

			bool mfiBuy = (SS_EnableChopFilter ? _mfi.MFI[0] > 52.0 : true);
			bool buy_con = (buySignal && bullishTrend && strongBullishCandle && (_currentMode[0] != SmmCustom.EMode.Buy) && mfiBuy && canBuy);
			if( buy_con && IN_MaEnabled )
			{
				double ma = (IN_MaMethod == SmmCustom.EMovingAverageType.EMA ?
								EMA(IN_MaSource == SmmCustom.EDataSource.HeikenAshi ? _ha.Close : _price.Close, IN_MaPeriod)[0] :
								SMA(IN_MaSource == SmmCustom.EDataSource.HeikenAshi ? _ha.Close : _price.Close, IN_MaPeriod)[0]);
				buy_con = Close[0] > ma;
			}
			if( buy_con && SS_EnableBuySellSignals ) _currentMode[0] = SmmCustom.EMode.Buy;

			bool mfiSell = (SS_EnableChopFilter ? _mfi.MFI[0] < 48.0 : true);
			bool sell_con = (sellSignal && bearishTrend && strongBearishCandle && (_currentMode[0] != SmmCustom.EMode.Sell) && mfiSell && canSell);
			if( sell_con && IN_MaEnabled )
			{
				double ma = (IN_MaMethod == SmmCustom.EMovingAverageType.EMA ?
								EMA(IN_MaSource == SmmCustom.EDataSource.HeikenAshi ? _ha.Close : _price.Close, IN_MaPeriod)[0] :
								SMA(IN_MaSource == SmmCustom.EDataSource.HeikenAshi ? _ha.Close : _price.Close, IN_MaPeriod)[0]);
				sell_con = Close[0] < ma;
			}
			if( sell_con && SS_EnableBuySellSignals ) _currentMode[0] = SmmCustom.EMode.Sell;

			if( (_currentMode[0] == SmmCustom.EMode.Buy) && (_price.Close[0] < profitWaveEmaSlow) && (_src.Close[0] < profitWaveEmaSlow) )
			    _currentMode[0] = SmmCustom.EMode.None;

			if( (_currentMode[0] == SmmCustom.EMode.Sell) && (_price.Close[0] > profitWaveEmaSlow) && (_src.Close[0] > profitWaveEmaSlow) )
			    _currentMode[0] = SmmCustom.EMode.None;

			PlotBrushes[PLOT_INDEX_MFI][0] = (_mfi.MFI[0] > 50.0 ? DB_MfiBullishColor : (_mfi.MFI[0] < 50.0 ? DB_MfiBearishColor : Brushes.White));
			Center[0] = 50.0;
			PlotBrushes[PLOT_INDEX_CENTER][0] = (bullishTrend ? _brushCenterDotsBullishColor : _brushCenterDotsBearishColor);

			if( buy_con || sell_con )
			{
				Switch[0] = 50.0;
				PlotBrushes[PLOT_INDEX_SIGNAL][0] = (buy_con ? _brushCenterDotsBullishColor : _brushCenterDotsBearishColor);
			}
			else
			{
				Switch.Reset(0);
				PlotBrushes[PLOT_INDEX_SIGNAL][0] = Brushes.Transparent;
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// OnRender
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		protected override void OnRender(ChartControl cc, ChartScale cs)
		{
			base.OnRender(cc,cs);

			if( cc == null || cs == null ) return;

			if( _brushBelow != null )
			{
				_brushBelow.Opacity = 0.5f;
				GfxDrawRectangle(ref cc, ref cs, _brushBelow, 10, 0);
				_brushBelow.Opacity = 0.25f;
				GfxDrawRectangle(ref cc, ref cs, _brushBelow, 20, 10);
			}

			if( _brushAbove != null )
			{
				_brushAbove.Opacity = 0.5f;
				GfxDrawRectangle(ref cc, ref cs, _brushAbove, 100, 90);
				_brushAbove.Opacity = 0.25f;
				GfxDrawRectangle(ref cc, ref cs, _brushAbove, 90, 80);
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// OnRenderTargetChanged
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		public override void OnRenderTargetChanged()
		{
			// Dispose the graphics brushes.
			if( _brushBelow != null ) _brushBelow.Dispose(); _brushBelow = null;
			if( _brushAbove != null ) _brushAbove.Dispose(); _brushAbove = null;

			if( RenderTarget == null ) return;

			// Reinitialize the graphics brushes.
			_brushBelow = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Green);
			_brushAbove = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// GfxDrawRectangle
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		private void GfxDrawRectangle(ref ChartControl cc, ref ChartScale cs, SharpDX.Direct2D1.SolidColorBrush brush,
									  double levelFrom, double levelUpto)
		{
			float xBegPos = cc.CanvasLeft;
			float xEndPos = cc.CanvasRight;
			float yBegPos = cs.GetYByValue(levelFrom);
			float yEndPos = cs.GetYByValue(levelUpto);
			float width = xEndPos - xBegPos;
			float height = Math.Abs( yEndPos - yBegPos);
			SetZOrder(-1); //Set object to be rendered behind the price bars
			RenderTarget.FillRectangle(new SharpDX.RectangleF((float) xBegPos, yBegPos, (float) width, height), brush);
		}


		//////////////////////////////////////////////////////////////////////////////////////////////////////
		// DisplayName
		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public override string DisplayName
		{
			get { return "SMM - Dashboard [v 1.06]"; }
		}

		#region "Series"

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MoneyFlowIndex
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Center
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Switch
		{
			get { return Values[2]; }
		}

		#endregion

		#region "Inputs"

		#region "Signal Settings"

		[NinjaScriptProperty]
		[Display(Name = "Use Heiken Ashi Chart", Order = 1, GroupName = "Signal Settings")]
        public bool SS_UseHeikenAshi
        {
			get; set;
        }

		[NinjaScriptProperty]
		[Display(Name = "Enable BUY/SELL Signals", Order = 2, GroupName = "Signal Settings")]
        public bool SS_EnableBuySellSignals
        {
			get; set;
        }

		[NinjaScriptProperty]
		[Display(Name = "Enable Chop Filter", Order = 3, GroupName = "Signal Settings")]
        public bool SS_EnableChopFilter
        {
			get; set;
        }

		#endregion

		#region Moving Average Filter

		[NinjaScriptProperty]
		[Display(Name = "Moving Average : Enabled", Order = 1, GroupName = "Moving Average Filter")]
        public bool IN_MaEnabled
        {
			get; set;
        }

		[NinjaScriptProperty]
		[Display(Name = "Moving Average : Period", Order = 2, GroupName = "Moving Average Filter")]
		[Range(2, 1000)]
        public int IN_MaPeriod
        {
			get; set;
        }

		[NinjaScriptProperty]
		[Display(Name = "Moving Average : Type", Order = 3, GroupName = "Moving Average Filter")]
        public SmmCustom.EMovingAverageType IN_MaMethod
        {
			get; set;
        }

		[NinjaScriptProperty]
		[Display(Name = "Moving Average : Data Source", Order = 4, GroupName = "Moving Average Filter")]
        public SmmCustom.EDataSource IN_MaSource
        {
			get; set;
        }

		#endregion

		#region "Dashboard Settings"

		[NinjaScriptProperty]
		[Display(Name = "TrendSwitch Dots Size", Order = 1, GroupName = "Dashboard Settings")]
		[Range(1, 10)]
        public int DB_TrendDotsSize
        {
			get; set;
        }

		[NinjaScriptProperty]
		[Display(Name = "Signal Dots Size", Order = 2, GroupName = "Dashboard Settings")]
		[Range(1, 10)]
        public int DB_CrossoverDotsSize
        {
			get; set;
        }

		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Money Flow Bullish Color", Order = 3, GroupName = "Dashboard Settings")]
		public Brush DB_MfiBullishColor
		{
			get; set;
		}
		[Browsable(false)]
		public string DB_MfiBullishColor_Serialize
		{
			get { return Serialize.BrushToString(this.DB_MfiBullishColor); }
			set { this.DB_MfiBullishColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Money Flow Bearish Color", Order = 4, GroupName = "Dashboard Settings")]
		public Brush DB_MfiBearishColor
		{
			get; set;
		}
		[Browsable(false)]
		public string DB_MfiBearishColor_Serialize
		{
			get { return Serialize.BrushToString(this.DB_MfiBearishColor); }
			set { this.DB_MfiBearishColor = Serialize.StringToBrush(value); }
		}

		#endregion

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SimpleMoneyMetricsDashboard[] cacheSimpleMoneyMetricsDashboard;
		public SimpleMoneyMetricsDashboard SimpleMoneyMetricsDashboard(bool sS_UseHeikenAshi, bool sS_EnableBuySellSignals, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod, SmmCustom.EDataSource iN_MaSource, int dB_TrendDotsSize, int dB_CrossoverDotsSize, Brush dB_MfiBullishColor, Brush dB_MfiBearishColor)
		{
			return SimpleMoneyMetricsDashboard(Input, sS_UseHeikenAshi, sS_EnableBuySellSignals, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod, iN_MaSource, dB_TrendDotsSize, dB_CrossoverDotsSize, dB_MfiBullishColor, dB_MfiBearishColor);
		}

		public SimpleMoneyMetricsDashboard SimpleMoneyMetricsDashboard(ISeries<double> input, bool sS_UseHeikenAshi, bool sS_EnableBuySellSignals, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod, SmmCustom.EDataSource iN_MaSource, int dB_TrendDotsSize, int dB_CrossoverDotsSize, Brush dB_MfiBullishColor, Brush dB_MfiBearishColor)
		{
			if (cacheSimpleMoneyMetricsDashboard != null)
				for (int idx = 0; idx < cacheSimpleMoneyMetricsDashboard.Length; idx++)
					if (cacheSimpleMoneyMetricsDashboard[idx] != null && cacheSimpleMoneyMetricsDashboard[idx].SS_UseHeikenAshi == sS_UseHeikenAshi && cacheSimpleMoneyMetricsDashboard[idx].SS_EnableBuySellSignals == sS_EnableBuySellSignals && cacheSimpleMoneyMetricsDashboard[idx].SS_EnableChopFilter == sS_EnableChopFilter && cacheSimpleMoneyMetricsDashboard[idx].IN_MaEnabled == iN_MaEnabled && cacheSimpleMoneyMetricsDashboard[idx].IN_MaPeriod == iN_MaPeriod && cacheSimpleMoneyMetricsDashboard[idx].IN_MaMethod == iN_MaMethod && cacheSimpleMoneyMetricsDashboard[idx].IN_MaSource == iN_MaSource && cacheSimpleMoneyMetricsDashboard[idx].DB_TrendDotsSize == dB_TrendDotsSize && cacheSimpleMoneyMetricsDashboard[idx].DB_CrossoverDotsSize == dB_CrossoverDotsSize && cacheSimpleMoneyMetricsDashboard[idx].DB_MfiBullishColor == dB_MfiBullishColor && cacheSimpleMoneyMetricsDashboard[idx].DB_MfiBearishColor == dB_MfiBearishColor && cacheSimpleMoneyMetricsDashboard[idx].EqualsInput(input))
						return cacheSimpleMoneyMetricsDashboard[idx];
			return CacheIndicator<SimpleMoneyMetricsDashboard>(new SimpleMoneyMetricsDashboard(){ SS_UseHeikenAshi = sS_UseHeikenAshi, SS_EnableBuySellSignals = sS_EnableBuySellSignals, SS_EnableChopFilter = sS_EnableChopFilter, IN_MaEnabled = iN_MaEnabled, IN_MaPeriod = iN_MaPeriod, IN_MaMethod = iN_MaMethod, IN_MaSource = iN_MaSource, DB_TrendDotsSize = dB_TrendDotsSize, DB_CrossoverDotsSize = dB_CrossoverDotsSize, DB_MfiBullishColor = dB_MfiBullishColor, DB_MfiBearishColor = dB_MfiBearishColor }, input, ref cacheSimpleMoneyMetricsDashboard);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleMoneyMetricsDashboard SimpleMoneyMetricsDashboard(bool sS_UseHeikenAshi, bool sS_EnableBuySellSignals, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod, SmmCustom.EDataSource iN_MaSource, int dB_TrendDotsSize, int dB_CrossoverDotsSize, Brush dB_MfiBullishColor, Brush dB_MfiBearishColor)
		{
			return indicator.SimpleMoneyMetricsDashboard(Input, sS_UseHeikenAshi, sS_EnableBuySellSignals, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod, iN_MaSource, dB_TrendDotsSize, dB_CrossoverDotsSize, dB_MfiBullishColor, dB_MfiBearishColor);
		}

		public Indicators.SimpleMoneyMetricsDashboard SimpleMoneyMetricsDashboard(ISeries<double> input , bool sS_UseHeikenAshi, bool sS_EnableBuySellSignals, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod, SmmCustom.EDataSource iN_MaSource, int dB_TrendDotsSize, int dB_CrossoverDotsSize, Brush dB_MfiBullishColor, Brush dB_MfiBearishColor)
		{
			return indicator.SimpleMoneyMetricsDashboard(input, sS_UseHeikenAshi, sS_EnableBuySellSignals, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod, iN_MaSource, dB_TrendDotsSize, dB_CrossoverDotsSize, dB_MfiBullishColor, dB_MfiBearishColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleMoneyMetricsDashboard SimpleMoneyMetricsDashboard(bool sS_UseHeikenAshi, bool sS_EnableBuySellSignals, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod, SmmCustom.EDataSource iN_MaSource, int dB_TrendDotsSize, int dB_CrossoverDotsSize, Brush dB_MfiBullishColor, Brush dB_MfiBearishColor)
		{
			return indicator.SimpleMoneyMetricsDashboard(Input, sS_UseHeikenAshi, sS_EnableBuySellSignals, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod, iN_MaSource, dB_TrendDotsSize, dB_CrossoverDotsSize, dB_MfiBullishColor, dB_MfiBearishColor);
		}

		public Indicators.SimpleMoneyMetricsDashboard SimpleMoneyMetricsDashboard(ISeries<double> input , bool sS_UseHeikenAshi, bool sS_EnableBuySellSignals, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod, SmmCustom.EDataSource iN_MaSource, int dB_TrendDotsSize, int dB_CrossoverDotsSize, Brush dB_MfiBullishColor, Brush dB_MfiBearishColor)
		{
			return indicator.SimpleMoneyMetricsDashboard(input, sS_UseHeikenAshi, sS_EnableBuySellSignals, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod, iN_MaSource, dB_TrendDotsSize, dB_CrossoverDotsSize, dB_MfiBullishColor, dB_MfiBearishColor);
		}
	}
}

#endregion
