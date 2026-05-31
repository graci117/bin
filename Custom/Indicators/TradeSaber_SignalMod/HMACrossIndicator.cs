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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber_SignalMod
{
	public class HMACrossIndicator : Indicator
	{
		private HMA HMA1;
		private HMA HMA2;
		private SMA SMA1;
		private ADX ADX1;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"HMA Crossover indicator with condition plots and signal line";
				Name										= "HMACrossIndicator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				ShowTransparentPlotsInDataBox				= true;
				
				// Parameters
				StartTime						= DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
				EndTime							= DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
				FastPeriodHMA					= 9;
				SlowPeriodHMA					= 21;
				EnableSmaFilter					= true;
				SmaPeriod						= 200;
				EnableAdxFilter					= true;
				AdxMin							= 15;
				AdxPeriod						= 14;
				
				// Add plots
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "Signal");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "TimeFilter");
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Dot, "HMACross");
				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Dot, "SMAFilter");
				AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Dot, "ADXFilter");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				HMA1 = HMA(Close, Convert.ToInt32(FastPeriodHMA));
				HMA2 = HMA(Close, Convert.ToInt32(SlowPeriodHMA));
				SMA1 = SMA(Close, Convert.ToInt32(SmaPeriod));
				ADX1 = ADX(Close, Convert.ToInt32(AdxPeriod));
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 1)
				return;

			// Check individual conditions
			bool timeOk = (Times[0][0].TimeOfDay > StartTime.TimeOfDay) && (Times[0][0].TimeOfDay < EndTime.TimeOfDay);
			bool hmaCrossUp = CrossAbove(HMA1, HMA2, 1);
			bool hmaCrossDown = CrossBelow(HMA1, HMA2, 1);
			bool priceAboveSMA = GetCurrentAsk(0) > SMA1[0];
			bool priceBelowSMA = GetCurrentAsk(0) < SMA1[0];
			bool adxOk = ADX1[0] > AdxMin;
			
			// Plot 1: Time Filter (row 1)
			if (timeOk)
				Values[1][0] = 1;
			else
				Values[1][0] = 1;
			
			PlotBrushes[1][0] = timeOk ? Brushes.Lime : Brushes.Red;
			
			// Plot 2: HMA Cross (row 2)
			if (hmaCrossUp)
			{
				Values[2][0] = 2;
				PlotBrushes[2][0] = Brushes.Lime;
			}
			else if (hmaCrossDown)
			{
				Values[2][0] = 2;
				PlotBrushes[2][0] = Brushes.Red;
			}
			else
			{
				Values[2][0] = 2;
				PlotBrushes[2][0] = Brushes.Gray;
			}
			
			// Plot 3: SMA Filter (row 3)
			if (!EnableSmaFilter)
			{
				Values[3][0] = 3;
				PlotBrushes[3][0] = Brushes.Gray; // Gray when disabled
			}
			else if (priceAboveSMA)
			{
				Values[3][0] = 3;
				PlotBrushes[3][0] = Brushes.Lime;
			}
			else if (priceBelowSMA)
			{
				Values[3][0] = 3;
				PlotBrushes[3][0] = Brushes.Red;
			}
			else
			{
				Values[3][0] = 3;
				PlotBrushes[3][0] = Brushes.Gray;
			}
			
			// Plot 4: ADX Filter (row 4)
			if (!EnableAdxFilter)
			{
				Values[4][0] = 4;
				PlotBrushes[4][0] = Brushes.Gray; // Gray when disabled
			}
			else if (adxOk)
			{
				Values[4][0] = 4;
				PlotBrushes[4][0] = Brushes.Lime;
			}
			else
			{
				Values[4][0] = 4;
				PlotBrushes[4][0] = Brushes.Red;
			}
			
			// Plot 0: Signal Line (+1 for long, -1 for short, 0 otherwise)
			Values[0][0] = 0;
			
			// Check all conditions for long signal
			bool longSignal = timeOk && hmaCrossUp &&
				(!EnableSmaFilter || priceAboveSMA) &&
				(!EnableAdxFilter || adxOk);
			
			// Check all conditions for short signal
			bool shortSignal = timeOk && hmaCrossDown &&
				(!EnableSmaFilter || priceBelowSMA) &&
				(!EnableAdxFilter || adxOk);
			
			if (longSignal)
				Values[0][0] = 1;
			else if (shortSignal)
				Values[0][0] = -1;
		}

		#region Properties
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="StartTime", Order=1, GroupName="Parameters")]
		public DateTime StartTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="EndTime", Order=2, GroupName="Parameters")]
		public DateTime EndTime
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastPeriodHMA", Order=3, GroupName="Parameters")]
		public int FastPeriodHMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowPeriodHMA", Order=4, GroupName="Parameters")]
		public int SlowPeriodHMA
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="EnableSmaFilter", Order=5, GroupName="Parameters")]
		public bool EnableSmaFilter
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmaPeriod", Order=6, GroupName="Parameters")]
		public int SmaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="EnableAdxFilter", Order=7, GroupName="Parameters")]
		public bool EnableAdxFilter
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AdxMin", Order=8, GroupName="Parameters")]
		public int AdxMin
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AdxPeriod", Order=9, GroupName="Parameters")]
		public int AdxPeriod
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TimeFilter
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HMACross
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SMAFilter
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ADXFilter
		{
			get { return Values[4]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber_SignalMod.HMACrossIndicator[] cacheHMACrossIndicator;
		public TradeSaber_SignalMod.HMACrossIndicator HMACrossIndicator(DateTime startTime, DateTime endTime, int fastPeriodHMA, int slowPeriodHMA, bool enableSmaFilter, int smaPeriod, bool enableAdxFilter, int adxMin, int adxPeriod)
		{
			return HMACrossIndicator(Input, startTime, endTime, fastPeriodHMA, slowPeriodHMA, enableSmaFilter, smaPeriod, enableAdxFilter, adxMin, adxPeriod);
		}

		public TradeSaber_SignalMod.HMACrossIndicator HMACrossIndicator(ISeries<double> input, DateTime startTime, DateTime endTime, int fastPeriodHMA, int slowPeriodHMA, bool enableSmaFilter, int smaPeriod, bool enableAdxFilter, int adxMin, int adxPeriod)
		{
			if (cacheHMACrossIndicator != null)
				for (int idx = 0; idx < cacheHMACrossIndicator.Length; idx++)
					if (cacheHMACrossIndicator[idx] != null && cacheHMACrossIndicator[idx].StartTime == startTime && cacheHMACrossIndicator[idx].EndTime == endTime && cacheHMACrossIndicator[idx].FastPeriodHMA == fastPeriodHMA && cacheHMACrossIndicator[idx].SlowPeriodHMA == slowPeriodHMA && cacheHMACrossIndicator[idx].EnableSmaFilter == enableSmaFilter && cacheHMACrossIndicator[idx].SmaPeriod == smaPeriod && cacheHMACrossIndicator[idx].EnableAdxFilter == enableAdxFilter && cacheHMACrossIndicator[idx].AdxMin == adxMin && cacheHMACrossIndicator[idx].AdxPeriod == adxPeriod && cacheHMACrossIndicator[idx].EqualsInput(input))
						return cacheHMACrossIndicator[idx];
			return CacheIndicator<TradeSaber_SignalMod.HMACrossIndicator>(new TradeSaber_SignalMod.HMACrossIndicator(){ StartTime = startTime, EndTime = endTime, FastPeriodHMA = fastPeriodHMA, SlowPeriodHMA = slowPeriodHMA, EnableSmaFilter = enableSmaFilter, SmaPeriod = smaPeriod, EnableAdxFilter = enableAdxFilter, AdxMin = adxMin, AdxPeriod = adxPeriod }, input, ref cacheHMACrossIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber_SignalMod.HMACrossIndicator HMACrossIndicator(DateTime startTime, DateTime endTime, int fastPeriodHMA, int slowPeriodHMA, bool enableSmaFilter, int smaPeriod, bool enableAdxFilter, int adxMin, int adxPeriod)
		{
			return indicator.HMACrossIndicator(Input, startTime, endTime, fastPeriodHMA, slowPeriodHMA, enableSmaFilter, smaPeriod, enableAdxFilter, adxMin, adxPeriod);
		}

		public Indicators.TradeSaber_SignalMod.HMACrossIndicator HMACrossIndicator(ISeries<double> input , DateTime startTime, DateTime endTime, int fastPeriodHMA, int slowPeriodHMA, bool enableSmaFilter, int smaPeriod, bool enableAdxFilter, int adxMin, int adxPeriod)
		{
			return indicator.HMACrossIndicator(input, startTime, endTime, fastPeriodHMA, slowPeriodHMA, enableSmaFilter, smaPeriod, enableAdxFilter, adxMin, adxPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber_SignalMod.HMACrossIndicator HMACrossIndicator(DateTime startTime, DateTime endTime, int fastPeriodHMA, int slowPeriodHMA, bool enableSmaFilter, int smaPeriod, bool enableAdxFilter, int adxMin, int adxPeriod)
		{
			return indicator.HMACrossIndicator(Input, startTime, endTime, fastPeriodHMA, slowPeriodHMA, enableSmaFilter, smaPeriod, enableAdxFilter, adxMin, adxPeriod);
		}

		public Indicators.TradeSaber_SignalMod.HMACrossIndicator HMACrossIndicator(ISeries<double> input , DateTime startTime, DateTime endTime, int fastPeriodHMA, int slowPeriodHMA, bool enableSmaFilter, int smaPeriod, bool enableAdxFilter, int adxMin, int adxPeriod)
		{
			return indicator.HMACrossIndicator(input, startTime, endTime, fastPeriodHMA, slowPeriodHMA, enableSmaFilter, smaPeriod, enableAdxFilter, adxMin, adxPeriod);
		}
	}
}

#endregion
