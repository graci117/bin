//
// BetterRenkoBarsType
//
// written by aslan
//
// 20100807 - created BetterRenko to address issues with other types of Renko bars
// 20101118 - changed initial brick alignment to brick size
// 20150719 - DaleBru converted to NT8
// 20160508 - antonma fixed the SessionIterator compiler error NJ8 8.0.0.9
// 20220324 releasing under Better Ninja Tools, minor updates
//
#region Using declarations
using System;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
//	[Gui.CategoryOrder("Version", 8000000)]		// at end
//	[CategoryExpanded("Version", false)]
	public class BetterRenkoBarsType : BarsType
	{
		public const string version = "2.1.0 released 20220404";

//		[Display(Name = "Version", Description="Version and release date", Order = 1, GroupName = "Version")]
//		public string Version { get { return version; } set { } }
		
		private enum RenkoState { BarComplete, BarAccumulating };
		private RenkoState barRenkoState = RenkoState.BarComplete;

		private double renkoHigh;
		private double renkoLow;		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name				= "BetterRenko";
				Description			= @"BetterRenko by aslan";
				BarsPeriod			= new BarsPeriod { BarsPeriodType = (BarsPeriodType) 17, BarsPeriodTypeName = "BetterRenko(17)", Value = 1 };
				BuiltFrom			= BarsPeriodType.Tick;
				DaysToLoad			= 3;
				IsIntraday			= true;
				DefaultChartStyle	= (ChartStyleType) 88; // BetterBrick
//				DefaultChartStyle	= Gui.Chart.ChartStyleType.CandleStick; 			
			}
			else if (State == State.Configure)
			{
                Name = string.Format(Core.Globals.GeneralOptions.CurrentCulture, "{0} BetterRenko{1}", BarsPeriod.Value, (BarsPeriod.MarketDataType != MarketDataType.Last ? string.Format(" - {0}", Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)) : string.Empty));

                Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));

				SetPropertyName("Value", "BrickSize");
            }			
		}

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
		{
			return 3;
		}

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
            var brickSize = bars.Instrument.MasterInstrument.RoundToTickSize(bars.BarsPeriod.Value * bars.Instrument.MasterInstrument.TickSize);  // #ticks per brick * tickSize						
			
			// build a session iterator from the bars object being updated
  			if (SessionIterator == null)
    			SessionIterator = new SessionIterator(bars);

			bool newSession = SessionIterator.IsNewSession(time, isBar);
			
			if (newSession)
				SessionIterator.GetNextSession(time, isBar);
			
            if (bars.Count == 0 || bars.IsResetOnNewTradingDay && newSession)
            {
                if (bars.Count > 0)
                {
                    // Close out last bar in session and set open == close
                    double lastBarClose = bars.GetClose(bars.Count - 1);
                    DateTime lastBarTime = bars.GetTime(bars.Count - 1);
                    long lastBarVolume = bars.GetVolume(bars.Count - 1);
                    bars.RemoveLastBar();
                    AddBar(bars, lastBarClose, lastBarClose, lastBarClose, lastBarClose, lastBarTime, lastBarVolume);
                }
                barRenkoState = RenkoState.BarAccumulating;
                double mod = bars.Instrument.MasterInstrument.RoundToTickSize(close % brickSize);
                double mid = bars.Instrument.MasterInstrument.Compare(mod, brickSize) == 0 ? close : close - mod;
                renkoHigh = mid + brickSize;
                renkoLow = mid - brickSize;
                AddBar(bars, close, close, close, close, time, volume);
                bars.LastPrice = close;
                return;
            }
            if (barRenkoState == RenkoState.BarComplete)
            {
                // this tick creates a new bar
                AddBar(bars, close, close, close, close, time, volume);
                if (RangeExceeded(bars.Instrument.MasterInstrument, close))
                {
                    MoveLimits(bars.Instrument.MasterInstrument, close, brickSize);
                }
            }
            else
            {
                if (RangeExceeded(bars.Instrument.MasterInstrument, close))
                {
                    AddBar(bars, close, close, close, close, time, volume);
                    MoveLimits(bars.Instrument.MasterInstrument, close, brickSize);
                }
                else
                {
                    var barHigh = bars.GetHigh(bars.Count - 1);
                    var barLow = bars.GetLow(bars.Count - 1);
                    UpdateBar(bars, (close > barHigh ? close : barHigh), (close < barLow ? close : barLow), close, time, volume);
                }
            }
            CheckBarComplete(bars.Instrument.MasterInstrument, close, brickSize);
            bars.LastPrice = close;
        }

		public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
		{
		}

		public override void ApplyDefaultValue(BarsPeriod period)
		{
			period.BarsPeriodTypeName = "BetterRenko";
			period.Value = 2;
		}

		public override string ChartLabel(DateTime dateTime)
		{
			return dateTime.ToString("T", Core.Globals.GeneralOptions.CurrentCulture);
		}

		public override double GetPercentComplete(Bars bars, DateTime now)
		{
			return 1.0d;
		}

        private void MoveLimits(MasterInstrument masterInstrument, double price, double brickSize)
		{
			if (masterInstrument.Compare(price, renkoHigh) >= 0)
			{
				do
				{
					renkoHigh += brickSize;
					renkoLow  = renkoHigh - 3.0 * brickSize;
				} while (masterInstrument.Compare(price, renkoHigh) > 0);  // stops if price in range, including edge
			}
			else
			{
				do
				{
					renkoLow -= brickSize;
					renkoHigh = renkoLow + 3.0 * brickSize;
				} while (masterInstrument.Compare(price, renkoLow) < 0);
			}
		}

        private void CheckBarComplete(MasterInstrument masterInstrument, double price, double brickSize)
		{
			int cmpHi = masterInstrument.Compare(price, renkoHigh);
			int cmpLo = masterInstrument.Compare(price, renkoLow);

			if (cmpHi == 0 || cmpLo == 0)
			{
				barRenkoState = RenkoState.BarComplete;
				MoveLimits(masterInstrument, price, brickSize);  // will move limits once since equal
			}
			else barRenkoState = RenkoState.BarAccumulating;
		}

		private bool RangeExceeded(MasterInstrument masterInstrument, double price)
		{
			int cmpHi = masterInstrument.Compare(price, renkoHigh);
			int cmpLo = masterInstrument.Compare(price, renkoLow);

			return (cmpHi > 0 || cmpLo < 0);
		}
	}
}
