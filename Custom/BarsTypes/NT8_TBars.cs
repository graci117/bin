

#region Using declarations
using System;
using System.ComponentModel;
using NinjaTrader;
using NinjaTrader.Cbi;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.BarsTypes
{
  public class TBars : BarsType
  {
    private double barOpen;
    private double barMax;
    private double barMin;
    private double fakeOpen;
    private int barDirection;
    private double openOffset;
    private double trendOffset;
    private double reversalOffset;
    private bool maxExceeded;
    private bool minExceeded;
    private double tickSize = 0.01;
    private bool hasInitialized;
//    private string moduleName = "NT8_TBars2";

    protected override void OnStateChange()
    {
	      if (State == State.SetDefaults)
	      {
				Description					= @"NT8 TBars";
				Name						= "TBars";				
				BarsPeriod					= new BarsPeriod { BarsPeriodType = (BarsPeriodType) 2015, BarsPeriodTypeName = "TBars(2015)"};			
		        BuiltFrom = BarsPeriodType.Tick;
		        DaysToLoad = 2;
				DefaultChartStyle			= Gui.Chart.ChartStyleType.CandleStick;
		        IsIntraday = true;
	      }
	      else
	      {
		        if (State != State.Configure)
		          return;
		        Properties.Remove(Properties.Find("BaseBarsPeriodType", true));
		        Properties.Remove(Properties.Find("PointAndFigurePriceType", true));
		        Properties.Remove(Properties.Find("ReversalType", true));
		        Properties.Remove(Properties.Find("Value", true));
		        Properties.Remove(Properties.Find("Value2", true));
		        SetPropertyName("BaseBarsPeriodValue", "Speed Settings");
		        BarsPeriod.Value = BarsPeriod.BaseBarsPeriodValue / 2;
		        BarsPeriod.Value2 = BarsPeriod.BaseBarsPeriodValue * 2;
	      }
    }

    public override int GetInitialLookBackDays(
      BarsPeriod barsPeriod,
      TradingHours tradingHours,
      int barsBack)
    {
      return 3;
    }

    protected override void OnDataPoint(
      Bars bars,
      double open,
      double high,
      double low,
      double close,
      DateTime time,
      long volume,
      bool isBar,
      double bid,
      double ask)
    {
      try
      {
          if (SessionIterator == null)
            SessionIterator = new SessionIterator(bars);
          bool flag;
          if (flag = SessionIterator.IsNewSession(time, isBar))
            SessionIterator.CalculateTradingDay(time, isBar);
          if (bars.Count != 0 && (!bars.IsResetOnNewTradingDay || !flag))
          {
            maxExceeded = bars.Instrument.MasterInstrument.Compare(close, barMax) > 0;
            minExceeded = bars.Instrument.MasterInstrument.Compare(close, barMin) < 0;
            if (!maxExceeded && !minExceeded)
            {
            //Print("else");
              int num = bars.Count - 1;
              double high1 = close > bars.GetHigh(num) ? close : bars.GetHigh(num);
              double low1 = close < bars.GetLow(num) ? close : bars.GetLow(num);
              double heikinAshiClose = GetHeikinAshiClose(bars.GetOpen(num), high1, low1, close);
              UpdateBar(bars, high1, low1, heikinAshiClose, time, volume);
            }
            else
            {
            //Print("maxExceeded || minExceeded");
              double close1 = maxExceeded ? Math.Min(close, barMax) : (minExceeded ? Math.Max(close, barMin) : close);
              barDirection = maxExceeded ? 1 : (minExceeded ? -1 : 0);
              fakeOpen = close1 - openOffset * (double) barDirection;
              int num = bars.Count - 1;
              double high2 = maxExceeded ? close1 : bars.GetHigh(num);
              double low2 = minExceeded ? close1 : bars.GetLow(num);
              double heikinAshiClose1 = GetHeikinAshiClose(bars.GetOpen(num), high2, low2, close1);
              UpdateBar(bars, high2, low2, heikinAshiClose1, time, volume);
              barOpen = close;
              barMax = close1 + (barDirection > 0 ? trendOffset : reversalOffset);
              barMin = close1 - (barDirection > 0 ? reversalOffset : trendOffset);
              double heikinAshiOpen = GetHeikinAshiOpen(fakeOpen, bars.GetClose(num));
              double high3 = maxExceeded ? close1 : fakeOpen;
              double low3 = minExceeded ? close1 : fakeOpen;
              double heikinAshiClose2 = GetHeikinAshiClose(heikinAshiOpen, high3, low3, close1);
              AddBar(bars, heikinAshiOpen, high3, low3, heikinAshiClose2, time, volume);
            }
          }
          else
          {
            tickSize = bars.Instrument.MasterInstrument.TickSize;
            trendOffset = (double) bars.BarsPeriod.Value * tickSize;
            reversalOffset = (double) bars.BarsPeriod.Value2 * tickSize;
            openOffset = (double) bars.BarsPeriod.BaseBarsPeriodValue * tickSize;
            barOpen = open;
            barMax = barOpen + trendOffset * (double) barDirection;
            barMin = barOpen - trendOffset * (double) barDirection;
            AddBar(bars, barOpen, high, low, GetHeikinAshiClose(open, high, low, close), time, volume);
          }
          bars.LastPrice = close;
      }
      catch (Exception ex)
      {
        Print((object) ex.ToString());
      }
    }

    public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
    {
    }

    public override void ApplyDefaultValue(BarsPeriod period)
    {
      period.Value = 1;
      period.Value2 = 4;
      period.BaseBarsPeriodValue = 2;
    }

    public override string ChartLabel(DateTime dateTime) => Name;

    public override double GetPercentComplete(Bars bars, DateTime now) => 0.0;

    private double GetHeikinAshiOpen(double open, double close) => (open + close) * 0.5;

    private double GetHeikinAshiHigh(double open, double high) => Math.Max(open, high);

    private double GetHeikinAshiLow(double open, double low) => Math.Min(open, low);

    private double GetHeikinAshiClose(double open, double high, double low, double close) => (open + high + low + close) * 0.25;
  }
}
