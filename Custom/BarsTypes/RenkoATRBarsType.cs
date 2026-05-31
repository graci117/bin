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


namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class RenkoATRBarsType : BarsType
	{
		private double					atr, offset, prevAtr, renkoHigh, renkoLow;
		private bool                    /*calculatedOnce, */isFirstTickOfMinBar, isNewSession, minPeriodElapsed;
		private int						synthMinBarCount;
		private DateTime				nextMinElapsedTime;
		private SynthBarPoints[]		synthMinBars;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"RenkoATR";
				Name								= "RenkoATR";
				BarsPeriod							= new BarsPeriod { BarsPeriodType = (BarsPeriodType) 2085, BarsPeriodTypeName = "RenkoATR(2085)", Value = 1 };
				BuiltFrom							= BarsPeriodType.Tick;
				DefaultChartStyle					= ChartStyleType.CandleStick;
				DaysToLoad							= 3;
				IsIntraday							= true;
				IsTimeBased							= false;
			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("BaseBarsPeriodType", true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType", true));
				Properties.Remove(Properties.Find("ReversalType", true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue", true));
				
				SetPropertyName("Value", "ATR period (bars)");
				SetPropertyName("Value2", "ATR base (minutes)");

				// TODO: should there be a multiplier?

				Name				= string.Format("{0} RenkoATR P{0}B{1}", BarsPeriod.Value, BarsPeriod.Value2);

				minPeriodElapsed	= true;
				//calculatedOnce		= false;
				isNewSession		= false;
				synthMinBars		= new SynthBarPoints[2];
				synthMinBarCount	= 0;
			}
		}

		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) { }

		public override void ApplyDefaultValue(BarsPeriod period)
		{
			period.Value				= 10;
			period.Value2				= 1;
		}

		public override string ChartLabel(DateTime dateTime)
		{
			//return "custom label logic here";
			// original
			// return time.ToString(chartControl.LabelFormatTick, Cbi.Globals.CurrentCulture);
			return dateTime.ToString("T", Core.Globals.GeneralOptions.CurrentCulture);
		}

		public override double GetPercentComplete(Bars bars, DateTime now)
		{ return 1.0d; }

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
		{ return 3; }

		public override bool IsRemoveLastBarSupported
		{ get { return true; } }

		private class SynthBarPoints
		{
			public DateTime Time;
			public double	Open, High, Low, Close;
			public long		Volume;
		}

		private void OnSynthMinBarsDataPoint(DateTime time, double close, long volume)
		{
			if (DateTime.Compare(nextMinElapsedTime, time) < 0)
				minPeriodElapsed = true;

			// minute ended, time to create a new bar
			if (minPeriodElapsed)
			{
				if (synthMinBarCount < BarsPeriod.Value)
					++synthMinBarCount;

				// calculate time until minute bar close
				nextMinElapsedTime	= new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0).AddMinutes(BarsPeriod.Value2);

				// for this specific script, as we will only ever need two bars of info, an array with the size of 2 is used instead of a list to conserve memory
				// set the previous bar to the current bar and reset the current bar
				synthMinBars[1]		= synthMinBars[0];

				// This will print out the previously closed minute bar if you would like to compare to a similar minute chart
				/*if (synthMinBarCount > 1)
				{
					SynthBarPoints barPrint = synthMinBars[1];
					Print(string.Format("{0} | Open: {1}, High: {2}, Low: {3}, Close: {4}, Volume: {5}", barPrint.Time, barPrint.Open, barPrint.High, barPrint.Low, barPrint.Close, barPrint.Volume));
				}*/
				
				synthMinBars[0]	= new SynthBarPoints()
				{
					Time	= nextMinElapsedTime,
					Close	= close,
					Open	= close,
					High	= close,
					Low		= close,
					Volume	= volume
				};

				minPeriodElapsed	= false;
				isFirstTickOfMinBar = true;
				return;
			}

			// else if not creating a new bar, update the existing bar
			synthMinBars[0].Volume	+= volume;
			synthMinBars[0].Close	= close;
			synthMinBars[0].High	= Math.Max(close, synthMinBars[0].High);
			synthMinBars[0].Low		= Math.Min(close, synthMinBars[0].Low);

			isFirstTickOfMinBar	= false;
		}

		// math and calculations from the ATR indicator
		private double CalculateATR(Bars bars, DateTime time)
		{			
			SynthBarPoints bar	= synthMinBars[0];

			double high0		= bar.High;
			double low0			= bar.Low;
			
			if (synthMinBarCount == 1)
				atr	= high0 - low0;

			else
			{
				// prevAtr is Value[1], minBarCount is CurrentBar + 1, BarsPeriod.Value is Period
				if (isFirstTickOfMinBar)
					prevAtr = atr;
				
				double close1		= synthMinBars[1].Close;
				double trueRange	= Math.Max(Math.Abs(low0 - close1), Math.Max(high0 - low0, Math.Abs(high0 - close1)));
				atr					= ((Math.Min(synthMinBarCount, BarsPeriod.Value) - 1) * prevAtr + trueRange) / Math.Min(synthMinBarCount, BarsPeriod.Value);
			}

			// this print will show the bar info and atr of the most recently closed bar if you would like to compare with a chart using the same minute series and the ATR indicator applied
			/*if (isFirstTickOfMinBar && synthMinBarCount > 1)
				Print(string.Format("{0:M/dd/yyyy hh:mm:ss:ffff} | high: {1}, low: {2}, close: {3}, atr: {4}, atr * TickSize: {5}", time, synthMinBars[1].High, synthMinBars[1].Low, synthMinBars[1].Close, prevAtr, bars.Instrument.MasterInstrument.RoundToTickSize(atr * bars.Instrument.MasterInstrument.TickSize) ));*/
			
			// TODO: confirm this should return a number of ticks and not points
			//return  Math.Max(bars.Instrument.MasterInstrument.TickSize, bars.Instrument.MasterInstrument.RoundToTickSize(atr * bars.Instrument.MasterInstrument.TickSize));
			return Math.Max(1, Convert.ToInt32(Math.Round(atr, 0)));
		}

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			if (SessionIterator == null)
				SessionIterator = new SessionIterator(bars);

			isNewSession = SessionIterator.IsNewSession(time, isBar);

			if (isNewSession)
				SessionIterator.GetNextSession(time, isBar);

			//if (!calculatedOnce)
			//{
			//	ClearOutputWindow();
			//	calculatedOnce = true;
			//}

			OnSynthMinBarsDataPoint(time, close, volume);
			
			// TODO: should this only calculate on first tick unless realtime?
			offset	= CalculateATR(bars, time);
			
			if (bars.Count == 0 || bars.IsResetOnNewTradingDay && isNewSession)
			{
				if (bars.Count > 0)
				{
					// Close out last bar in session and set open == close
					double		lastBarClose	= bars.GetClose(bars.Count - 1);
					DateTime	lastBarTime		= bars.GetTime(bars.Count - 1);
					long		lastBarVolume	= bars.GetVolume(bars.Count - 1);

					RemoveLastBar(bars);
					AddBar(bars, lastBarClose, lastBarClose, lastBarClose, lastBarClose, lastBarTime, lastBarVolume);
				}

				renkoHigh	= close + offset;
				renkoLow	= close - offset;

				isNewSession = SessionIterator.IsNewSession(time, isBar);
				if (isNewSession)
					SessionIterator.GetNextSession(time, isBar);

				AddBar(bars, close, close, close, close, time, volume);
				bars.LastPrice	= close;
				return;
			}
			
			double		barOpen		= bars.GetOpen(bars.Count - 1);
			double		barHigh		= bars.GetHigh(bars.Count - 1);
			double		barLow		= bars.GetLow(bars.Count - 1);
			long		barVolume	= bars.GetVolume(bars.Count - 1);
			DateTime	barTime		= bars.GetTime(bars.Count - 1);
			
			if (renkoHigh.ApproxCompare(0.0) == 0 || renkoLow.ApproxCompare(0.0) == 0)
			{
				if (bars.Count == 1)
				{
					renkoHigh	= barOpen + offset;
					renkoLow	= barOpen - offset;
				}
				else if (bars.GetClose(bars.Count - 2) > bars.GetOpen(bars.Count - 2))
				{
					renkoHigh	= bars.GetClose(bars.Count - 2) + offset;
					renkoLow	= bars.GetClose(bars.Count - 2) - offset * 2;
				}
				else
				{
					renkoHigh	= bars.GetClose(bars.Count - 2) + offset * 2;
					renkoLow	= bars.GetClose(bars.Count - 2) - offset;
				}
			}
			// !!! debugging - something below this point is causing a runaway memory when the offset is calculated atr
			if (close.ApproxCompare(renkoHigh) >= 0)
			{
				if (barOpen.ApproxCompare(renkoHigh - offset) != 0
					|| barHigh.ApproxCompare(Math.Max(renkoHigh - offset, renkoHigh)) != 0
					|| barLow.ApproxCompare(Math.Min(renkoHigh - offset, renkoHigh)) != 0)
				{	
					RemoveLastBar(bars);
					AddBar(bars, renkoHigh - offset, Math.Max(renkoHigh - offset, renkoHigh), Math.Min(renkoHigh - offset, renkoHigh), renkoHigh, barTime, barVolume);
				}

				renkoLow	= renkoHigh - 2.0 * offset;
				renkoHigh	= renkoHigh + offset;

				isNewSession = SessionIterator.IsNewSession(time, isBar);
				if (isNewSession)
					SessionIterator.GetNextSession(time, isBar);

				while (close.ApproxCompare(renkoHigh) >= 0)	// Add empty bars to fill gap if price jumps
				{
					AddBar(bars, renkoHigh - offset, Math.Max(renkoHigh - offset, renkoHigh), Math.Min(renkoHigh - offset, renkoHigh), renkoHigh, time, 0);
					renkoLow	= renkoHigh - 2.0 * offset;
					renkoHigh	= renkoHigh + offset;
				}

				// Add final partial bar
				AddBar(bars, renkoHigh - offset, Math.Max(renkoHigh - offset, close), Math.Min(renkoHigh - offset, close), close, time, volume);
			}

			else if (close.ApproxCompare(renkoLow) <= 0)
			{
				if (barOpen.ApproxCompare(renkoLow + offset) != 0
					|| barHigh.ApproxCompare(Math.Max(renkoLow + offset, renkoLow)) != 0
					|| barLow.ApproxCompare(Math.Min(renkoLow + offset, renkoLow)) != 0)
				{
					RemoveLastBar(bars);
					AddBar(bars, renkoLow + offset, Math.Max(renkoLow + offset, renkoLow), Math.Min(renkoLow + offset, renkoLow), renkoLow, barTime, barVolume);
				}

				renkoHigh	= renkoLow + 2.0 * offset;
				renkoLow	= renkoLow - offset;

				isNewSession = SessionIterator.IsNewSession(time, isBar);
				if (isNewSession)
					SessionIterator.GetNextSession(time, isBar);

				while (close.ApproxCompare(renkoLow) <= 0)	// Add empty bars to fill gap if price jumps
				{
					AddBar(bars, renkoLow + offset, Math.Max(renkoLow + offset, renkoLow), Math.Min(renkoLow + offset, renkoLow), renkoLow, time, 0);
					renkoHigh	= renkoLow + 2.0 * offset;
					renkoLow	= renkoLow - offset;
				}

				// Add final partial bar
				AddBar(bars, renkoLow + offset, Math.Max(renkoLow + offset, close), Math.Min(renkoLow + offset, close), close, time, volume);
			}
			else
				UpdateBar(bars, close, close, close, time, volume);

			bars.LastPrice = close;
		}
	}
}