//////////////////////////////////////////////////////////////////////////////////////////////////////
//	Indicator	: 	MTFEmaFilter													//
//	Description	:	Multi-timeframe EMA filter for signal validation					//
//	Author		:	Your Name													//
//	History		:	21-Sep-2025		1.00	Initial version							//
//////////////////////////////////////////////////////////////////////////////////////////////////////

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


namespace NinjaTrader.NinjaScript.Indicators
{
    public class MTFEmaFilter : Indicator
    {
        #region Variables
        private EMA _ema1Min;
        private int _ema1MinIndex = -1;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Multi-timeframe EMA filter for signal validation";
                Name = "MTFEmaFilter";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                // Default parameters
                EmaPeriod = 14;
                TickThreshold = 30.0;
                FilterTimeframe = FilterTimeframeType.OneMinute;

                // Add plots for debugging/visualization (optional)
                AddPlot(Brushes.Blue, "BullishFilter");
                AddPlot(Brushes.Red, "BearishFilter");
                AddPlot(Brushes.Gray, "EMADistance");
            }
            else if (State == State.DataLoaded)
            {
                // Add the specified timeframe data series if not already present
                if (FilterTimeframe == FilterTimeframeType.OneMinute)
                {
                    if (BarsPeriod.BarsPeriodType != BarsPeriodType.Minute || BarsPeriod.Value != 1)
                    {
                        AddDataSeries(Data.BarsPeriodType.Minute, 1);
                        _ema1MinIndex = 1;
                    }
                    else
                    {
                        _ema1MinIndex = 0;
                    }
                }
                
                // Initialize the EMA
                _ema1Min = EMA(BarsArray[_ema1MinIndex], EmaPeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < EmaPeriod || _ema1Min == null)
            {
                Values[0][0] = 0; // BullishFilter
                Values[1][0] = 0; // BearishFilter
                Values[2][0] = 0; // EMADistance
                return;
            }

            try
            {
                // Get current close and EMA values from the filter timeframe
                double filterClose = Closes[_ema1MinIndex][0];
                double filterEMA = _ema1Min[0];
                
                // Calculate distance in ticks
                double distanceInTicks = Math.Abs(filterClose - filterEMA) / Instrument.MasterInstrument.TickSize;
                
                // Set plot values
                Values[0][0] = GetBullishFilterValue(filterClose, filterEMA, distanceInTicks) ? 1 : 0;
                Values[1][0] = GetBearishFilterValue(filterClose, filterEMA, distanceInTicks) ? 1 : 0;
                Values[2][0] = distanceInTicks;
            }
            catch (Exception ex)
            {
                Print("MTFEmaFilter" + ex.Message);
                Values[0][0] = 1; // Allow signals on error
                Values[1][0] = 1;
                Values[2][0] = 0;
            }
        }

        #region Public Methods
        /// <summary>
        /// Check if bullish signal should be allowed
        /// </summary>
        public bool IsBullishSignalAllowed()
        {
            if (CurrentBar < EmaPeriod || _ema1Min == null) return true;
            
            try
            {
                double filterClose = Closes[_ema1MinIndex][0];
                double filterEMA = _ema1Min[0];
                double distanceInTicks = Math.Abs(filterClose - filterEMA) / Instrument.MasterInstrument.TickSize;
                
                return GetBullishFilterValue(filterClose, filterEMA, distanceInTicks);
            }
            catch
            {
                return true; // Allow on error
            }
        }

        /// <summary>
        /// Check if bearish signal should be allowed
        /// </summary>
        public bool IsBearishSignalAllowed()
        {
            if (CurrentBar < EmaPeriod || _ema1Min == null) return true;
            
            try
            {
                double filterClose = Closes[_ema1MinIndex][0];
                double filterEMA = _ema1Min[0];
                double distanceInTicks = Math.Abs(filterClose - filterEMA) / Instrument.MasterInstrument.TickSize;
                
                return GetBearishFilterValue(filterClose, filterEMA, distanceInTicks);
            }
            catch
            {
                return true; // Allow on error
            }
        }

        /// <summary>
        /// Get current EMA distance in ticks
        /// </summary>
        public double GetEMADistanceInTicks()
        {
            if (CurrentBar < EmaPeriod || _ema1Min == null) return 0;
            
            try
            {
                double filterClose = Closes[_ema1MinIndex][0];
                double filterEMA = _ema1Min[0];
                return Math.Abs(filterClose - filterEMA) / Instrument.MasterInstrument.TickSize;
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region Private Methods
        private bool GetBullishFilterValue(double close, double ema, double distanceInTicks)
        {
            // If price closed above EMA, allow signal regardless of distance
            if (close > ema)
                return true;
            
            // If price is within threshold distance, block the signal
            if (distanceInTicks <= TickThreshold)
                return false;
            
            // If price is below EMA but outside threshold, allow signal
            return true;
        }

        private bool GetBearishFilterValue(double close, double ema, double distanceInTicks)
        {
            // If price closed below EMA, allow signal regardless of distance
            if (close < ema)
                return true;
            
            // If price is within threshold distance, block the signal
            if (distanceInTicks <= TickThreshold)
                return false;
            
            // If price is above EMA but outside threshold, allow signal
            return true;
        }
        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Range(1, 200)]
        [Display(Name = "EMA Period", Order = 1, GroupName = "Parameters")]
        public int EmaPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, 500)]
        [Display(Name = "Tick Threshold", Order = 2, GroupName = "Parameters")]
        public double TickThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Filter Timeframe", Order = 3, GroupName = "Parameters")]
        public FilterTimeframeType FilterTimeframe { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> BullishFilter => Values[0];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> BearishFilter => Values[1];

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EMADistance => Values[2];
        #endregion
    }


}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MTFEmaFilter[] cacheMTFEmaFilter;
		public MTFEmaFilter MTFEmaFilter(int emaPeriod, double tickThreshold, FilterTimeframeType filterTimeframe)
		{
			return MTFEmaFilter(Input, emaPeriod, tickThreshold, filterTimeframe);
		}

		public MTFEmaFilter MTFEmaFilter(ISeries<double> input, int emaPeriod, double tickThreshold, FilterTimeframeType filterTimeframe)
		{
			if (cacheMTFEmaFilter != null)
				for (int idx = 0; idx < cacheMTFEmaFilter.Length; idx++)
					if (cacheMTFEmaFilter[idx] != null && cacheMTFEmaFilter[idx].EmaPeriod == emaPeriod && cacheMTFEmaFilter[idx].TickThreshold == tickThreshold && cacheMTFEmaFilter[idx].FilterTimeframe == filterTimeframe && cacheMTFEmaFilter[idx].EqualsInput(input))
						return cacheMTFEmaFilter[idx];
			return CacheIndicator<MTFEmaFilter>(new MTFEmaFilter(){ EmaPeriod = emaPeriod, TickThreshold = tickThreshold, FilterTimeframe = filterTimeframe }, input, ref cacheMTFEmaFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MTFEmaFilter MTFEmaFilter(int emaPeriod, double tickThreshold, FilterTimeframeType filterTimeframe)
		{
			return indicator.MTFEmaFilter(Input, emaPeriod, tickThreshold, filterTimeframe);
		}

		public Indicators.MTFEmaFilter MTFEmaFilter(ISeries<double> input , int emaPeriod, double tickThreshold, FilterTimeframeType filterTimeframe)
		{
			return indicator.MTFEmaFilter(input, emaPeriod, tickThreshold, filterTimeframe);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MTFEmaFilter MTFEmaFilter(int emaPeriod, double tickThreshold, FilterTimeframeType filterTimeframe)
		{
			return indicator.MTFEmaFilter(Input, emaPeriod, tickThreshold, filterTimeframe);
		}

		public Indicators.MTFEmaFilter MTFEmaFilter(ISeries<double> input , int emaPeriod, double tickThreshold, FilterTimeframeType filterTimeframe)
		{
			return indicator.MTFEmaFilter(input, emaPeriod, tickThreshold, filterTimeframe);
		}
	}
}

#endregion
