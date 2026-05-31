// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using NinjaTrader.Data;
using NinjaTrader.Core.FloatingPoint;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
	public class HeikinAshiBarsType : BarsType
	{
		public override void ApplyDefaultValue(BarsPeriod period) { }

		public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
		{
			switch (period.BaseBarsPeriodType)
			{
				case BarsPeriodType.Day		: period.BaseBarsPeriodValue = 1;		DaysToLoad = 365;	WeeksToLoad = 52;	MonthsToLoad = 12;	YearsToLoad = 1;	break;
				case BarsPeriodType.Minute	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 5;		WeeksToLoad = 1;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Month	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 5475;	WeeksToLoad = 780;	MonthsToLoad = 180;	YearsToLoad = 15;	break;
				case BarsPeriodType.Second	: period.BaseBarsPeriodValue = 30;		DaysToLoad = 3;		WeeksToLoad = 0;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Tick	: period.BaseBarsPeriodValue = 150;		DaysToLoad = 3;		WeeksToLoad = 0;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Volume	: period.BaseBarsPeriodValue = 1000;	DaysToLoad = 3;		WeeksToLoad = 0;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Week	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 1825;	WeeksToLoad = 260;	MonthsToLoad = 60;	YearsToLoad = 5;	break;
				case BarsPeriodType.Year	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 15000; WeeksToLoad = 780;	MonthsToLoad = 180;	YearsToLoad = 15; break;
			}
		}

		public override string ChartLabel(DateTime time) =>
			BarsPeriod.BaseBarsPeriodType switch
			{
				BarsPeriodType.Day		=> BarsTypeDay.ChartLabel(time),
				BarsPeriodType.Minute	=> BarsTypeMinute.ChartLabel(time),
				BarsPeriodType.Month	=> BarsTypeMonth.ChartLabel(time),
				BarsPeriodType.Second	=> BarsTypeSecond.ChartLabel(time),
				BarsPeriodType.Tick		=> BarsTypeTick.ChartLabel(time),
				BarsPeriodType.Volume	=> BarsTypeTick.ChartLabel(time),
				BarsPeriodType.Week		=> BarsTypeDay.ChartLabel(time),
				BarsPeriodType.Year		=> BarsTypeYear.ChartLabel(time),
				_						=> BarsTypeDay.ChartLabel(time)
			};

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
		{
			switch (BarsPeriod.BaseBarsPeriodType)
			{
				case BarsPeriodType.Day		:	return (int) Math.Ceiling(barsPeriod.BaseBarsPeriodValue * barsBack * 7.0 / 4.5);
				case BarsPeriodType.Minute	:
					int minutesPerWeek = 0;
					lock (tradingHours.Sessions)
					{
						foreach (Session session in tradingHours.Sessions)
						{
							int beginDay = (int)session.BeginDay;
							int endDay = (int)session.EndDay;
							if (beginDay > endDay)
								endDay += 7;

							minutesPerWeek += (endDay - beginDay) * 1440 + session.EndTime / 100 * 60 + session.EndTime % 100 - (session.BeginTime / 100 * 60 + session.BeginTime % 100);
						}
					}

					return (int)Math.Max(1, Math.Ceiling(barsBack / Math.Max(1, minutesPerWeek / 7.0 / barsPeriod.BaseBarsPeriodValue) * 1.05));
				case BarsPeriodType.Month	:	return barsPeriod.BaseBarsPeriodValue * barsBack * 31;
				case BarsPeriodType.Second	:	return (int) Math.Max(1, Math.Ceiling(barsBack / Math.Max(1, 8.0 * 60 * 60 / barsPeriod.BaseBarsPeriodValue)) * 7.0 / 5.0);	// 8 hours
				case BarsPeriodType.Tick	:	return 1;
				case BarsPeriodType.Volume	:	return 1;
				case BarsPeriodType.Week	:	return barsPeriod.BaseBarsPeriodValue * barsBack * 7;
				case BarsPeriodType.Year	:	return barsPeriod.BaseBarsPeriodValue * barsBack * 365;
				default						:	return 1;	
			}
		}

		public override double GetPercentComplete(Bars bars, DateTime now)
		{
			switch (BarsPeriod.BaseBarsPeriodType)
			{
				case BarsPeriodType.Day		:	return now.Date <= bars.LastBarTime.Date
															? 1.0 - bars.LastBarTime.AddDays(1).Subtract(now).TotalDays / bars.BarsPeriod.BaseBarsPeriodValue
															: 1;
				case BarsPeriodType.Minute	:	return now <= bars.LastBarTime ? 1.0 - bars.LastBarTime.Subtract(now).TotalMinutes / bars.BarsPeriod.BaseBarsPeriodValue : 1;
				case BarsPeriodType.Month	: 
					if (now.Date <= bars.LastBarTime.Date)
					{
						int month = now.Month;
						int daysInMonth = month == 2 ? DateTime.IsLeapYear(now.Year) ? 29 : 28 : month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12 ? 31 : 30;
						return (daysInMonth - bars.LastBarTime.Date.AddDays(1).Subtract(now).TotalDays / bars.BarsPeriod.BaseBarsPeriodValue) / daysInMonth;
					}
					return 1;
				case BarsPeriodType.Second	:	return now <= bars.LastBarTime ? 1.0 - bars.LastBarTime.Subtract(now).TotalSeconds / bars.BarsPeriod.BaseBarsPeriodValue : 1;
				case BarsPeriodType.Tick	:	return (double) bars.TickCount / bars.BarsPeriod.BaseBarsPeriodValue;
				case BarsPeriodType.Volume	:	return bars.Count == 0 ? 0 : (double) bars.GetVolume(bars.Count - 1) / bars.BarsPeriod.BaseBarsPeriodValue;
				case BarsPeriodType.Week	:	return now.Date <= bars.LastBarTime.Date ? (7 - bars.LastBarTime.AddDays(1).Subtract(now).TotalDays / bars.BarsPeriod.BaseBarsPeriodValue) / 7 : 1;
				case BarsPeriodType.Year	: 
					if (now.Date <= bars.LastBarTime.Date)
					{							
						double daysInYear = DateTime.IsLeapYear(now.Year) ? 366 : 365;
						return (daysInYear - bars.LastBarTime.Date.AddDays(1).Subtract(now).TotalDays / bars.BarsPeriod.BaseBarsPeriodValue) / daysInYear;
					}
					return 1;
				default						: return 1;
			}
		}

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			double	haClose		= 0.0;
			double	haHigh;
			double	haLow;
			double	haOpen		= 0.0;

			switch (BarsPeriod.BaseBarsPeriodType)
			{
				case BarsPeriodType.Day:
					{
						if (bars.Count == 0)
						{
							if (isBar || bars.TradingHours.Sessions.Count == 0)
								AddBar(bars, open, high, low, close, time.Date, volume);
							else
							{
								SessionIterator.CalculateTradingDay(time, false);
								AddBar(bars, open, high, low, close, SessionIterator.ActualTradingDayExchange, volume);
							}
						}
						else
						{
							DateTime barTime;
							if (isBar)
								barTime = time.Date;
							else
							{
								if (bars.TradingHours.Sessions.Count > 0 && SessionIterator.IsNewSession(time, false))
								{
									SessionIterator.CalculateTradingDay(time, false);
									barTime = SessionIterator.ActualTradingDayExchange;
									if (barTime < bars.LastBarTime.Date)
										barTime = bars.LastBarTime.Date; // Make sure timestamps are ascending
								}
								else
									barTime = bars.LastBarTime.Date; // Make sure timestamps are ascending
							}

							if (bars.DayCount < bars.BarsPeriod.BaseBarsPeriodValue
								|| isBar && bars.Count > 0 && barTime == bars.LastBarTime.Date
								|| !isBar && bars.Count > 0 && barTime <= bars.LastBarTime.Date)
							{
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
								UpdateBar(bars, haHigh, haLow, haClose, barTime, volume);
							}
							else
							{
								haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
								AddBar(bars, haOpen, haHigh, haLow, haClose, barTime, volume);
							}
						}

						break;
					}
				case BarsPeriodType.Minute:
					{
						if (bars.Count == 0)
							AddBar(bars, open, high, low, close, TimeToBarTimeMinute(bars, time, isBar), volume);
						else if (!isBar && time < bars.LastBarTime)
						{
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
							UpdateBar(bars, haHigh, haLow, haClose, bars.LastBarTime, volume);
						}
						else if (isBar && time <= bars.LastBarTime)
						{
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
							UpdateBar(bars, haHigh, haLow, haClose, bars.LastBarTime, volume);
						}
						else
						{
							haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
							time		= TimeToBarTimeMinute(bars, time, isBar);
							AddBar(bars, haOpen, haHigh, haLow, haClose, time, volume);
						}

						break;
					}
				case BarsPeriodType.Month:
					{
						if (bars.Count == 0)
							AddBar(bars, open, high, low, close, TimeToBarTimeMonth(time, bars.BarsPeriod.BaseBarsPeriodValue), volume);
						else if (time.Month <= bars.LastBarTime.Month && time.Year == bars.LastBarTime.Year || time.Year < bars.LastBarTime.Year)
						{
							if (high.ApproxCompare(bars.GetHigh(bars.Count - 1)) != 0 || low.ApproxCompare(bars.GetLow(bars.Count - 1)) != 0 || close.ApproxCompare(bars.GetClose(bars.Count - 1)) != 0 || volume > 0)
							{
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
								UpdateBar(bars, haHigh, haLow, haClose, bars.LastBarTime, volume);
							}
						}
						else
						{
							haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
							AddBar(bars, haOpen, haHigh, haLow, haClose, TimeToBarTimeMonth(time, bars.BarsPeriod.BaseBarsPeriodValue), volume);
						}
						break;
					}
				case BarsPeriodType.Second:
					{
						if (bars.Count == 0)
						{
							DateTime barTime = TimeToBarTimeSecond(bars, time, isBar);
							AddBar(bars, open, high, low, close, barTime, volume);
						}
						else
						{
							if (bars.BarsPeriod.BaseBarsPeriodValue > 1 && time < bars.LastBarTime || bars.BarsPeriod.BaseBarsPeriodValue == 1 && time <= bars.LastBarTime)
							{
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
								UpdateBar(bars, haHigh, haLow, haClose, bars.LastBarTime, volume);
							}
							else
							{
								haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
								time		= TimeToBarTimeSecond(bars, time, isBar);
								AddBar(bars, haOpen, haHigh, haLow, haClose, time, volume);
							}
						}
						break;
					}
				case BarsPeriodType.Tick:
					{
						bool isNewSession = SessionIterator.IsNewSession(time, isBar);
						if (isNewSession)
							SessionIterator.GetNextSession(time, isBar);

						if (bars.BarsPeriod.BaseBarsPeriodValue == 1)
						{
							haOpen		= haOpen.ApproxCompare(0.0) == 0 ? open : (haOpen + haClose) / 2.0;
							haClose		= haClose.ApproxCompare(0.0) == 0 ? close : bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
							AddBar(bars, haOpen, haHigh, haLow, haClose, time, volume);
						}
						else if (bars.Count == 0)
							AddBar(bars, open, high, low, close, time, volume);
						else if (bars.Count > 0 && (!isNewSession || !bars.IsResetOnNewTradingDay) && bars.BarsPeriod.BaseBarsPeriodValue > 1 && bars.TickCount < bars.BarsPeriod.BaseBarsPeriodValue)
						{
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
							UpdateBar(bars, haHigh, haLow, haClose, time, volume);
						}
						else
						{
							haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
							AddBar(bars, haOpen, haHigh, haLow, haClose, time, volume);
						}
						break;
					}
				case BarsPeriodType.Volume:
					{
						if (bars.Count == 0)
						{
							while (volume > bars.BarsPeriod.BaseBarsPeriodValue)
							{
								haOpen		= haOpen.ApproxCompare(0.0) == 0 ? open : (haOpen + haClose) / 2.0;
								haClose		= haClose.ApproxCompare(0) == 0 ? close : bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
								AddBar(bars, haOpen, haHigh, haLow, haClose, time, bars.BarsPeriod.BaseBarsPeriodValue);
								volume -= bars.BarsPeriod.BaseBarsPeriodValue;
							}
							if (volume > 0)
							{
								haOpen		= haOpen.ApproxCompare(0.0) == 0 ? open : bars.Instrument.MasterInstrument.RoundToTickSize((haOpen + haClose) / 2.0);
								haClose		= haClose.ApproxCompare(0.0) == 0 ? close : bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
								AddBar(bars, haOpen, haHigh, haLow, haClose, time, volume);
							}
						}
						else
						{
							long volumeTmp		= 0;
							bool isNewSession	= SessionIterator.IsNewSession(time, isBar);
							if (!bars.IsResetOnNewTradingDay || !isNewSession)
							{
								volumeTmp = Math.Min(bars.BarsPeriod.BaseBarsPeriodValue - bars.GetVolume(bars.Count - 1), volume);
								if (volumeTmp > 0)
								{
									haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
									haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
									haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
									UpdateBar(bars, haHigh, haLow, haClose, time, volumeTmp);
								}
							}

							if (isNewSession)
								SessionIterator.GetNextSession(time, isBar);

							volumeTmp = volume - volumeTmp;
							while (volumeTmp > 0)
							{
								haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
								AddBar(bars, haOpen, haHigh, haLow, haClose, time, Math.Min(volumeTmp, bars.BarsPeriod.BaseBarsPeriodValue));
								volumeTmp -= bars.BarsPeriod.BaseBarsPeriodValue;
							}
						}

						break;
					}
				case BarsPeriodType.Week:
					{
						if (bars.Count == 0)
						{
							AddBar(bars, open, high, low, close, TimeToBarTimeWeek(time, time.AddDays(6 - ((int)time.DayOfWeek + 1) % 7 + (bars.BarsPeriod.BaseBarsPeriodValue - 1) * 7), bars.BarsPeriod.BaseBarsPeriodValue), volume);
						}
						else if (time.Date <= bars.LastBarTime.Date)
						{
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
							UpdateBar(bars, haHigh, haLow, haClose, bars.LastBarTime, volume);
						}
						else
						{
							haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
							haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
							haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
							haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
							AddBar(bars, haOpen, haHigh, haLow, haClose, TimeToBarTimeWeek(time.Date, bars.LastBarTime.Date, bars.BarsPeriod.BaseBarsPeriodValue), volume);
						}
						
						break;
					}
				case BarsPeriodType.Year:
					{
						if (bars.Count == 0)
						{
							AddBar(bars, open, high, low, close, TimeToBarTimeYear(time, bars.BarsPeriod.BaseBarsPeriodValue), volume);
						}
						else
						{
							if (time.Year <= bars.LastBarTime.Year)
							{
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, bars.GetOpen(bars.Count - 1)));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, bars.GetOpen(bars.Count - 1)));
								UpdateBar(bars, haHigh, haLow, haClose, bars.LastBarTime, volume);
							}
							else
							{
								haOpen		= bars.Instrument.MasterInstrument.RoundToTickSize((bars.GetOpen(bars.Count - 1) + bars.GetClose(bars.Count - 1)) / 2.0);
								haClose		= bars.Instrument.MasterInstrument.RoundToTickSize((open + high + low + close) / 4.0);
								haHigh		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Max(high, haOpen));
								haLow		= bars.Instrument.MasterInstrument.RoundToTickSize(Math.Min(low, haOpen));
								AddBar(bars, haOpen, haHigh, haLow, haClose, TimeToBarTimeYear(time.Date, bars.BarsPeriod.BaseBarsPeriodValue), volume);
							}
						}

						break;
					}
			}

			bars.LastPrice = haClose;
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name						= Custom.Resource.NinjaScriptBarsTypeHeikenAshi;
				BarsPeriod					= new BarsPeriod { BarsPeriodType = BarsPeriodType.HeikenAshi }; 
				DaysToLoad					= 3;
			}
			else if (State == State.Configure)
			{
				switch (BarsPeriod.BaseBarsPeriodType)
				{
					case BarsPeriodType.Minute	: BuiltFrom = BarsPeriodType.Minute; IsIntraday = true; IsTimeBased = true; break;
					case BarsPeriodType.Second	: BuiltFrom = BarsPeriodType.Tick;   IsIntraday = true;	IsTimeBased = true; break;
					case BarsPeriodType.Tick	:
					case BarsPeriodType.Volume	: BuiltFrom = BarsPeriodType.Tick;	IsIntraday = true;	IsTimeBased = false; break;
					default						: BuiltFrom = BarsPeriodType.Day;	IsIntraday = false;	IsTimeBased = true; break;
				}

				switch (BarsPeriod.BaseBarsPeriodType)
				{
					case BarsPeriodType.Day		: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiDaily : Resource.GuiDay)} Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
					case BarsPeriodType.Minute	: Name = $"{BarsPeriod.BaseBarsPeriodValue} Min Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";																						break;
					case BarsPeriodType.Month	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiMonthly : Resource.GuiMonth)} Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
					case BarsPeriodType.Second	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiSecond : Resource.GuiSeconds)} Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
					case BarsPeriodType.Tick	: Name = $"{BarsPeriod.BaseBarsPeriodValue} Tick Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";																						break;
					case BarsPeriodType.Volume	: Name = $"{BarsPeriod.BaseBarsPeriodValue} Volume Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";																						break;
					case BarsPeriodType.Week	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiWeekly : Resource.GuiWeeks)} Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
					case BarsPeriodType.Year	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiYearly : Resource.GuiYears)} Heiken-Ashi{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
				}

				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value",						true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}

		private DateTime TimeToBarTimeMinute(Bars bars, DateTime time, bool isBar)
		{
			if (SessionIterator.IsNewSession(time, isBar))
				SessionIterator.GetNextSession(time, isBar);

			if (bars.IsResetOnNewTradingDay || !bars.IsResetOnNewTradingDay && bars.Count == 0)
			{
				DateTime barTimeStamp = isBar
					? SessionIterator.ActualSessionBegin.AddMinutes(Math.Ceiling(Math.Ceiling(Math.Max(0, time.Subtract(SessionIterator.ActualSessionBegin).TotalMinutes)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue)
					: SessionIterator.ActualSessionBegin.AddMinutes(bars.BarsPeriod.BaseBarsPeriodValue + Math.Floor(Math.Floor(Math.Max(0, time.Subtract(SessionIterator.ActualSessionBegin).TotalMinutes)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue);
				if (bars.TradingHours.Sessions.Count > 0 && barTimeStamp > SessionIterator.ActualSessionEnd) // Cut last bar in session down to session end on odd session end time
					barTimeStamp = SessionIterator.ActualSessionEnd;
				return barTimeStamp;
			}
			else
			{
				DateTime lastBarTime	= bars.GetTime(bars.Count - 1);
				DateTime barTimeStamp	= isBar
					? lastBarTime.AddMinutes(Math.Ceiling(Math.Ceiling(Math.Max(0, time.Subtract(lastBarTime).TotalMinutes)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue)
					: lastBarTime.AddMinutes(bars.BarsPeriod.BaseBarsPeriodValue + Math.Floor(Math.Floor(Math.Max(0, time.Subtract(lastBarTime).TotalMinutes)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue);
				if (bars.TradingHours.Sessions.Count > 0 && barTimeStamp > SessionIterator.ActualSessionEnd)
				{
					DateTime saveActualSessionEnd = SessionIterator.ActualSessionEnd;
					SessionIterator.GetNextSession(SessionIterator.ActualSessionEnd.AddSeconds(1), isBar);
					barTimeStamp = SessionIterator.ActualSessionBegin.AddMinutes((int) barTimeStamp.Subtract(saveActualSessionEnd).TotalMinutes);
				}
				return barTimeStamp;
			}
		}

		private static DateTime TimeToBarTimeMonth(DateTime time, int periodValue)
		{
			DateTime result = new(time.Year, time.Month, 1);
			for (int i = 0; i < periodValue; i++)
				result = result.AddMonths(1);

			return result.AddDays(-1);
		}

		private DateTime TimeToBarTimeSecond(Bars bars, DateTime time, bool isBar)
		{
			if (SessionIterator.IsNewSession(time, isBar))
				SessionIterator.GetNextSession(time, isBar);

			if (bars.IsResetOnNewTradingDay || !bars.IsResetOnNewTradingDay && bars.Count == 0)
			{
				DateTime barTimeStamp = isBar
					? SessionIterator.ActualSessionBegin.AddSeconds(Math.Ceiling(Math.Ceiling(Math.Max(0, time.Subtract(SessionIterator.ActualSessionBegin).TotalSeconds)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue)
					: SessionIterator.ActualSessionBegin.AddSeconds(bars.BarsPeriod.BaseBarsPeriodValue + Math.Floor(Math.Floor(Math.Max(0, time.Subtract(SessionIterator.ActualSessionBegin).TotalSeconds)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue);
				if (bars.TradingHours.Sessions.Count > 0 && barTimeStamp > SessionIterator.ActualSessionEnd) // Cut last bar in session down to session end on odd session end time
					barTimeStamp = SessionIterator.ActualSessionEnd;
				return barTimeStamp;
			}
			else
			{
				DateTime lastBarTime	= bars.GetTime(bars.Count - 1);
				DateTime barTimeStamp	= isBar
					? lastBarTime.AddSeconds(Math.Ceiling(Math.Ceiling(Math.Max(0, time.Subtract(lastBarTime).TotalSeconds)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue)
					: lastBarTime.AddSeconds(bars.BarsPeriod.BaseBarsPeriodValue + Math.Floor(Math.Floor(Math.Max(0, time.Subtract(lastBarTime).TotalSeconds)) / bars.BarsPeriod.BaseBarsPeriodValue) * bars.BarsPeriod.BaseBarsPeriodValue);
				if (bars.TradingHours.Sessions.Count > 0 && barTimeStamp > SessionIterator.ActualSessionEnd)
				{
					DateTime saveActualSessionEnd = SessionIterator.ActualSessionEnd;
					SessionIterator.GetNextSession(SessionIterator.ActualSessionEnd.AddSeconds(1), isBar);
					barTimeStamp = SessionIterator.ActualSessionBegin.AddSeconds((int) barTimeStamp.Subtract(saveActualSessionEnd).TotalSeconds);
				}
				return barTimeStamp;
			}
		}

		private static DateTime TimeToBarTimeWeek(DateTime time, DateTime periodStart, int periodValue)
		{
			return periodStart.Date.AddDays(Math.Ceiling(Math.Ceiling(time.Date.Subtract(periodStart.Date).TotalDays) / (periodValue * 7)) * (periodValue * 7)).Date;
		}

		private static DateTime TimeToBarTimeYear(DateTime time, int periodValue)
		{
			DateTime result = new(time.Year, 1, 1);
			for (int i = 0; i < periodValue; i++)
				result = result.AddYears(1);

			return result.AddDays(-1);
		}
	}
}
