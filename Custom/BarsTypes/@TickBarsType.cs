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
	public class TickBarsType : BarsType
	{
		public override void ApplyDefaultBasePeriodValue(BarsPeriod period) {}

		public override void ApplyDefaultValue(BarsPeriod period) => period.Value = 150;

		public override string ChartLabel(DateTime time) => time.ToString("HH:mm:ss");

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack) => 1;

		public override double GetPercentComplete(Bars bars, DateTime now) => bars.TickCount / (double) bars.BarsPeriod.Value;

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			SessionIterator ??= new SessionIterator(bars);

			bool isNewSession = SessionIterator.IsNewSession(time, isBar);
			if (isNewSession)
				SessionIterator.GetNextSession(time, isBar);
			if (bars.BarsPeriod.Value == 1)
				AddBar(bars, open, high, low, close, time, volume, bid, ask);
			else if (bars.Count == 0)
				AddBar(bars, open, high, low, close, time, volume);
			else if (bars.Count > 0 && (!bars.IsResetOnNewTradingDay || !isNewSession) && bars.BarsPeriod.Value > 1 && bars.TickCount < bars.BarsPeriod.Value)
				UpdateBar(bars, high, low, close, time, volume);
			else
				AddBar(bars, open, high, low, close, time, volume); 
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptBarsTypeTick;
				BarsPeriod		= new BarsPeriod { BarsPeriodType = BarsPeriodType.Tick };
				BuiltFrom		= BarsPeriodType.Tick;
				DaysToLoad		= 3;
				IsIntraday		= true;
				IsTimeBased		= false;
			}
			else if (State == State.Configure)
			{
				Name = string.Format(Core.Globals.GeneralOptions.CurrentCulture, Custom.Resource.DataBarsTypeTick, BarsPeriod.Value, BarsPeriod.MarketDataType != MarketDataType.Last ? string.Format(" - {0}", Core.Globals.ToLocalizedObject(BarsPeriod.MarketDataType, Core.Globals.GeneralOptions.CurrentUICulture)) : string.Empty);

				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				Properties.Remove(Properties.Find("Value2",						true));
			}
		}
	}
}
