// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using NinjaTrader.Data;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class WeekBarsType : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}

		public override void ApplyDefaultValue(BarsPeriod period) => period.Value = 1;

		public override string ChartLabel(DateTime time)
			=> time.ToString(System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthDayPattern);

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
			=> barsPeriod.Value * barsBack * 7;

		public override double GetPercentComplete(Bars bars, DateTime now)
			=> now.Date <= bars.LastBarTime.Date ? (7 - bars.LastBarTime.AddDays(1).Subtract(now).TotalDays / bars.BarsPeriod.Value) / 7 : 1;

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			if (bars.Count == 0)
			{
				if (isBar || bars.TradingHours.Sessions.Count == 0)
					AddBar(bars, open, high, low, close, TimeToBarTime(time, time.AddDays(6 - ((int)time.DayOfWeek + 1) % 7 + (bars.BarsPeriod.Value - 1) * 7), bars.BarsPeriod.Value), volume);
				else
				{
					SessionIterator.CalculateTradingDay(time, false);
					DateTime barDate = SessionIterator.ActualTradingDayExchange;
					AddBar(bars, open, high, low, close, TimeToBarTime(barDate, barDate.AddDays(6 - ((int)barDate.DayOfWeek + 1) % 7 + (bars.BarsPeriod.Value - 1) * 7), bars.BarsPeriod.Value), volume);
				}
			}
			else
			{
				DateTime barTime;
				if (isBar)
					barTime = time.Date;
				else
				{
					if (SessionIterator.IsNewSession(time, false))
					{
						SessionIterator.CalculateTradingDay(time, false);
						barTime = SessionIterator.ActualTradingDayExchange;
						if (barTime < bars.LastBarTime.Date)
							barTime = bars.LastBarTime.Date; // Make sure timestamps are ascending
					}
					else
						barTime = bars.LastBarTime.Date; // Make sure timestamps are ascending
				}

				if (barTime <= bars.LastBarTime.Date)
					UpdateBar(bars, high, low, close, bars.LastBarTime, volume);
				else
					AddBar(bars, open, high, low, close, TimeToBarTime(barTime, bars.LastBarTime.Date, bars.BarsPeriod.Value), volume);
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptBarsTypeWeek;
				BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Week };
				BuiltFrom		= BarsPeriodType.Day;
				DaysToLoad		= 1825;
				WeeksToLoad		= 620;
				MonthsToLoad	= 60;
				YearsToLoad		= 5;
				IsIntraday		= false;
				IsTimeBased		= true;
			}
			else if (State == State.Configure)
			{
				Name = $"{(BarsPeriod.Value == 1 ? Custom.Resource.DataBarsTypeWeekly : string.Format(Core.Globals.GeneralOptions.CurrentCulture, Custom.Resource.DataBarsTypeWeek, BarsPeriod.Value))}{(BarsPeriod.MarketDataType != MarketDataType.Last ? string.Format(" - {0}", Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)) : string.Empty)}";

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}

		private DateTime TimeToBarTime(DateTime time, DateTime periodStart, int periodValue)
		{
			return periodStart.Date.AddDays(Math.Ceiling(Math.Ceiling(time.Date.Subtract(periodStart.Date).TotalDays) / (periodValue * 7)) * (periodValue * 7)).Date;
		}
	}
}
