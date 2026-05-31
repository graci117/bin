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
	public class LineBreakBarsType : BarsType
	{
		private double		anchorPrice			= double.MinValue;
		private bool		firstBarOfSession	= true;
		private	bool		newSession;
		private	int			newSessionIdx;
		private double		switchPrice			= double.MinValue;
		private int			tmpCount;
		private int			tmpDayCount;
		private	int			tmpTickCount;
		private	DateTime	tmpTime				= Core.Globals.MinDate;
		private long		tmpVolume;
		private bool		upTrend				= true;


		public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
		{
			switch (period.BaseBarsPeriodType)
			{
				case BarsPeriodType.Day		: period.BaseBarsPeriodValue = 1;		DaysToLoad = 365;	WeeksToLoad = 52;	MonthsToLoad = 1;	YearsToLoad = 0;	break;
				case BarsPeriodType.Minute	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 5;		WeeksToLoad = 1;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Month	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 5475;	WeeksToLoad = 780;	MonthsToLoad = 180;	YearsToLoad = 15;	break;
				case BarsPeriodType.Second	: period.BaseBarsPeriodValue = 30;		DaysToLoad = 3;		WeeksToLoad = 0;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Tick	: period.BaseBarsPeriodValue = 150;		DaysToLoad = 3;		WeeksToLoad = 0;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Volume	: period.BaseBarsPeriodValue = 1000;	DaysToLoad = 3;		WeeksToLoad = 0;	MonthsToLoad = 0;	YearsToLoad = 0;	break;
				case BarsPeriodType.Week	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 1825;	WeeksToLoad = 260;	MonthsToLoad = 60;	YearsToLoad = 5;	break;
				case BarsPeriodType.Year	: period.BaseBarsPeriodValue = 1;		DaysToLoad = 15000;	WeeksToLoad = 780;	MonthsToLoad = 180;	YearsToLoad = 15;	break;
			}
		}

		public override void ApplyDefaultValue(BarsPeriod period)
		{
			period.Value = 3;
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

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) =>
			BarsPeriod.BaseBarsPeriodType switch
			{
				BarsPeriodType.Day		=> new DayBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				BarsPeriodType.Minute	=> new MinuteBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				BarsPeriodType.Month	=> new MonthBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				BarsPeriodType.Second	=> new SecondBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				BarsPeriodType.Tick		=> new TickBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				BarsPeriodType.Volume	=> new VolumeBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				BarsPeriodType.Week		=> new WeekBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				BarsPeriodType.Year		=> new YearBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack),
				_						=> new MinuteBarsType().GetInitialLookBackDays(barsPeriod, tradingHours, barsBack)
			};

		public override double GetPercentComplete(Bars bars, DateTime now) { return 0; }

		public override bool IsRemoveLastBarSupported => true;

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			if (bars.Count == 0 && tmpTime != Core.Globals.MinDate) // Reset caching when live request trimmed existing bars
				tmpTime = Core.Globals.MinDate;

			bool endOfBar = true;
			if (tmpTime == Core.Globals.MinDate)
			{
				tmpTime			= time;
				tmpDayCount		= 1;
				tmpTickCount	= 1;
			}
			else if (bars.Count < tmpCount && bars.Count == 0) // Reset cache when bars are trimmed
			{
				tmpTime			= Core.Globals.MinDate;
				tmpVolume		= 0;
				tmpDayCount		= 0;
				tmpTickCount	= 0;
			}
			else if (bars.Count < tmpCount && bars.Count > 0) // Reset cache when bars are trimmed
			{
				tmpTime			= bars.GetTime(bars.Count - 1); 
				tmpVolume		= bars.GetVolume(bars.Count - 1);
				tmpTickCount	= bars.TickCount;
				tmpDayCount		= bars.DayCount;
			}

			switch (BarsPeriod.BaseBarsPeriodType)
			{
				case BarsPeriodType.Day:
					{
						if (bars.Count == 0 || bars.Count > 0 && (bars.LastBarTime.Month < time.Month || bars.LastBarTime.Year < time.Year))
						{
							tmpTime			= time.Date;
							bars.LastPrice	= close;
							newSession		= true;
						}
						else
						{
							tmpTime			= time.Date;
							tmpVolume		+= volume;
							bars.LastPrice	= close;
							tmpDayCount++;

							if (tmpDayCount < BarsPeriod.BaseBarsPeriodValue || bars.Count > 0 && bars.LastBarTime.Date == time.Date)
								endOfBar = false;
						}
						break;
					}
				case BarsPeriodType.Minute:
					{
						if (bars.Count == 0 || SessionIterator.IsNewSession(time, isBar) && bars.IsResetOnNewTradingDay)
						{
							tmpTime		= TimeToBarTimeMinute(bars, time, isBar);
							newSession	= true;
							tmpVolume	= 0;
						}
						else
						{
							if (!isBar && time < bars.LastBarTime || isBar && time <= bars.LastBarTime)
							{
								tmpTime		= bars.LastBarTime;
								endOfBar	= false;
							}
							else
								tmpTime		= TimeToBarTimeMinute(bars, time, isBar);

							tmpVolume		+= volume;
						}
						break;
					}
				case BarsPeriodType.Month:
					{
						if (tmpTime == Core.Globals.MinDate)
						{
							tmpTime		= TimeToBarTimeMonth(time, BarsPeriod.BaseBarsPeriodValue);

							if (bars.Count == 0)
								break;

							endOfBar = false;
						}
						else if (time.Month <= tmpTime.Month && time.Year == tmpTime.Year || time.Year < tmpTime.Year)
						{
							tmpVolume		+= volume;
							bars.LastPrice	= close;
							endOfBar		= false;
						}
						break;
					}
				case BarsPeriodType.Second:
					{
						if (SessionIterator.IsNewSession(time, isBar))
						{
							tmpTime = TimeToBarTimeSecond(bars, time, isBar);

							if (bars.Count == 0)
								break;

							endOfBar	= false;
							newSession	= true;
						}
						else if (time <= tmpTime)
						{
							tmpVolume		+= volume;
							bars.LastPrice	= close;
							endOfBar		= false;
						}
						else
							tmpTime = TimeToBarTimeSecond(bars, time, isBar);
						break;
					}
				case BarsPeriodType.Tick:
					{
						if (SessionIterator.IsNewSession(time, isBar))
						{
							SessionIterator.GetNextSession(time, isBar);
							newSession		= true;
							tmpTime			= time;
							tmpTickCount	= 1;

							if (bars.Count == 0)
								break;

							endOfBar = false;
						}
						else if (BarsPeriod.BaseBarsPeriodValue > 1 && tmpTickCount < BarsPeriod.BaseBarsPeriodValue)
						{
							tmpTime			= time;
							tmpVolume		+= volume;
							tmpTickCount++;
							bars.LastPrice	= close;
							endOfBar		= false;
						}
						else
							tmpTime = time; 
						break;
					}
				case BarsPeriodType.Volume:
					{
						if (SessionIterator.IsNewSession(time, isBar))
						{
							SessionIterator.GetNextSession(time, isBar);
							newSession = true;
						}
						else if (bars.Count == 0 && volume > 0)
							break;
						else
						{
							tmpVolume += volume;
							if (tmpVolume < BarsPeriod.BaseBarsPeriodValue)
							{
								bars.LastPrice	= close;
								endOfBar		= false;
							}
							else if (tmpVolume == 0)
								endOfBar = false;
						}

						tmpTime = time; 

						break;
					}
				case BarsPeriodType.Week:
					{
						if (tmpTime == Core.Globals.MinDate)
						{
							tmpTime = TimeToBarTimeWeek(time.Date, tmpTime.Date, BarsPeriod.BaseBarsPeriodValue);

							if (bars.Count == 0)
								break;

							endOfBar = false;
						}
						else if (time.Date <= tmpTime.Date)
						{
							tmpVolume		+= volume;
							bars.LastPrice	= close;
							endOfBar		= false;
						}
						break;
					}
				case BarsPeriodType.Year:
					{
						if (tmpTime == Core.Globals.MinDate)
						{
							tmpTime = TimeToBarTimeYear(time, BarsPeriod.BaseBarsPeriodValue);

							if (bars.Count == 0)
								break;

							endOfBar = false;
						}
						else if (time.Year <= tmpTime.Year)
						{
							tmpVolume		+= volume;
							bars.LastPrice	= close;
							endOfBar		= false;
						}
						break;
					}
			}

			if (bars.Count > 0 && tmpTime < bars.GetTime(bars.Count - 1) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Second)
				tmpTime = bars.GetTime(bars.Count - 1);

			if (bars.Count == 0 || newSession && IsIntraday)
			{
				AddBar(bars, open, close, close, close, tmpTime, volume);
				upTrend				= open < close;
				newSessionIdx		= bars.Count - 1;
				newSession			= false;
				firstBarOfSession	= true;
				anchorPrice			= close;
				switchPrice			= open;
			}
			else if (firstBarOfSession && endOfBar == false)
			{
				double prevOpen		= bars.GetOpen(bars.Count - 1);
				RemoveLastBar(bars);
				if (SessionIterator.IsNewSession(tmpTime, true))
					SessionIterator.GetNextSession(tmpTime, true);
				AddBar(bars, prevOpen, close, close, close, tmpTime, tmpVolume);
				upTrend				= prevOpen < close;
				anchorPrice			= close;
			}
			else
			{
				int		breakCount		= BarsPeriod.Value;
				double	breakMax		= double.MinValue;
				double	breakMin		= double.MaxValue;

				if (firstBarOfSession)
				{
					AddBar(bars, anchorPrice, close, close, close, tmpTime, volume);
					firstBarOfSession		= false;
					tmpVolume				= volume;
					tmpTime					= Core.Globals.MinDate;
					return;
				}

				if (bars.Count - newSessionIdx - 1 < breakCount)
					breakCount = bars.Count - (newSessionIdx + 1);

				for (int k = 1; k <= breakCount; k++)
				{
					breakMax		= Math.Max(breakMax, bars.GetOpen(bars.Count - k - 1));
					breakMax		= Math.Max(breakMax, bars.GetClose(bars.Count - k - 1));
					breakMin		= Math.Min(breakMin, bars.GetOpen(bars.Count - k - 1));
					breakMin		= Math.Min(breakMin, bars.GetClose(bars.Count - k - 1));
				}

				bars.LastPrice = close;

				if (upTrend)
					if (endOfBar)
					{
						bool adding = false;
						if (bars.Instrument.MasterInstrument.Compare(bars.GetClose(bars.Count - 1), anchorPrice) > 0)
						{
							anchorPrice		= bars.GetClose(bars.Count - 1);
							switchPrice		= bars.GetOpen(bars.Count - 1);
							tmpVolume		= volume;
							adding			= true;
						}
						else
							if (bars.Instrument.MasterInstrument.Compare(breakMin, bars.GetClose(bars.Count - 1)) > 0)
							{
								anchorPrice		= bars.GetClose(bars.Count - 1);
								switchPrice		= bars.GetOpen(bars.Count - 1);
								tmpVolume		= volume;
								upTrend			= false;
								adding			= true;
							}

						if (adding)
						{
							double tmpOpen = upTrend ? Math.Min(Math.Max(switchPrice, close), anchorPrice) : Math.Max(Math.Min(switchPrice, close), anchorPrice);
							AddBar(bars, tmpOpen, close, close, close, tmpTime, volume);
						}
						else
						{
							RemoveLastBar(bars);
							double tmpOpen = Math.Min(Math.Max(switchPrice, close), anchorPrice);
							if (SessionIterator.IsNewSession(tmpTime, true))
								SessionIterator.GetNextSession(tmpTime, true);
							AddBar(bars, tmpOpen, close, close, close, tmpTime, tmpVolume);
						}
					}
					else
					{
						RemoveLastBar(bars);
						double tmpOpen = Math.Min(Math.Max(switchPrice, close), anchorPrice);
						if (SessionIterator.IsNewSession(tmpTime, true))
							SessionIterator.GetNextSession(tmpTime, true);
						AddBar(bars, tmpOpen, close, close, close, tmpTime, tmpVolume);
					}
				else
					if (endOfBar)
					{
						bool adding = false;
						if (bars.Instrument.MasterInstrument.Compare(bars.GetClose(bars.Count - 1), anchorPrice) < 0)
						{
							anchorPrice		= bars.GetClose(bars.Count - 1);
							switchPrice		= bars.GetOpen(bars.Count - 1);
							tmpVolume		= volume;
							adding			= true;
						}
						else
							if (bars.Instrument.MasterInstrument.Compare(breakMax, bars.GetClose(bars.Count - 1)) < 0)
							{
								anchorPrice		= bars.GetClose(bars.Count - 1);
								switchPrice		= bars.GetOpen(bars.Count - 1);
								tmpVolume		= volume;
								upTrend			= true;
								adding			= true;
							}

						if (adding)
						{
							double tmpOpen = upTrend ? Math.Min(Math.Max(switchPrice, close), anchorPrice) : Math.Max(Math.Min(switchPrice, close), anchorPrice);
							AddBar(bars, tmpOpen, close, close, close, tmpTime, volume);
						}
						else
						{
							RemoveLastBar(bars);
							double tmpOpen = Math.Max(Math.Min(switchPrice, close), anchorPrice);
							if (SessionIterator.IsNewSession(tmpTime, true))
								SessionIterator.GetNextSession(tmpTime, true);
							AddBar(bars, tmpOpen, close, close, close, tmpTime, tmpVolume);
						}
					}
					else
					{
						RemoveLastBar(bars);
						double tmpOpen = Math.Max(Math.Min(switchPrice, close), anchorPrice);
						if (SessionIterator.IsNewSession(tmpTime, true))
							SessionIterator.GetNextSession(tmpTime, true);
						AddBar(bars, tmpOpen, close, close, close, tmpTime, tmpVolume);
					}
			}

			if (endOfBar)
				tmpTime = Core.Globals.MinDate;

			tmpCount = bars.Count;
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name						= Custom.Resource.NinjaScriptBarsTypeLineBreak;
				BarsPeriod					= new BarsPeriod { BarsPeriodType = BarsPeriodType.LineBreak };
				DaysToLoad					= 5;
				WeeksToLoad					= 1;
				DefaultChartStyle			= Gui.Chart.ChartStyleType.OpenClose;
			}
			else if (State == State.Configure)
			{
				switch (BarsPeriod.BaseBarsPeriodType)
				{
					case BarsPeriodType.Minute	: BuiltFrom = BarsPeriodType.Minute; IsIntraday = true; IsTimeBased = true; break;
					case BarsPeriodType.Second	: BuiltFrom = BarsPeriodType.Tick;	IsIntraday = true;	IsTimeBased = true; break;
					case BarsPeriodType.Tick	:
					case BarsPeriodType.Volume	: BuiltFrom = BarsPeriodType.Tick;	IsIntraday = true;	IsTimeBased = false; break;
					default						: BuiltFrom = BarsPeriodType.Day;	IsIntraday = false;	IsTimeBased = true; break;
				}

				switch (BarsPeriod.BaseBarsPeriodType)
				{
					case BarsPeriodType.Day		: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiDaily : Resource.GuiDay)} LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";		break;
					case BarsPeriodType.Minute	: Name = $"{BarsPeriod.BaseBarsPeriodValue} Min LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";																					break;
					case BarsPeriodType.Month	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiMonthly : Resource.GuiMonth)} LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
					case BarsPeriodType.Second	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiSecond : Resource.GuiSeconds)} LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
					case BarsPeriodType.Tick	: Name = $"{BarsPeriod.BaseBarsPeriodValue} Tick LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";																					break;
					case BarsPeriodType.Volume	: Name = $"{BarsPeriod.BaseBarsPeriodValue} Volume LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";																					break;
					case BarsPeriodType.Week	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiWeekly : Resource.GuiWeeks)} LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
					case BarsPeriodType.Year	: Name = $"{BarsPeriod.BaseBarsPeriodValue} {(BarsPeriod.BaseBarsPeriodValue == 1 ? Resource.GuiYearly : Resource.GuiYears)} LineBreak{(BarsPeriod.MarketDataType != MarketDataType.Last ? $" - {BarsPeriod.MarketDataType}" : string.Empty)}";	break;
				}

				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));

				SetPropertyName("Value", Custom.Resource.NinjaScriptBarsTypeLineBreakLineBreaks);
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
			=> periodStart.Date.AddDays(Math.Ceiling(Math.Ceiling(time.Date.Subtract(periodStart.Date).TotalDays) / (periodValue * 7)) * (periodValue * 7)).Date;

		private static DateTime TimeToBarTimeYear(DateTime time, int periodValue)
		{
			DateTime result = new(time.Year, 1, 1);
			for (int i = 0; i < periodValue; i++)
				result = result.AddYears(1);

			return result.AddDays(-1);
		}
	}
}
