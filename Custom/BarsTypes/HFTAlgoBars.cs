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
	public class HFTAlgoBarsType : BarsType
	{
		double barOpen;
	    double barMax;
	    double barMin;
	    double fakeOpen;
	    int barDirection;
	    double openOffset;
	    double trendOffset;
	    double reversalOffset;
	    bool maxExceeded;
	    bool minExceeded;
	    double tickSize = 0.01;
	    int tmpCount;
	    double offset;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"HFT ALGO BARS";
				Name										= "HFTAlgoBars";
				BarsPeriod barsPeriod;
        		(barsPeriod = new BarsPeriod()).BarsPeriodType = (BarsPeriodType) 42069;
        		barsPeriod.BarsPeriodTypeName = "HFTBars";
        		barsPeriod.Value = 1;
        		BarsPeriod = barsPeriod;
        		BuiltFrom = BarsPeriodType.Tick;
        		DaysToLoad = 3;
        		IsIntraday = true;
			}
			else if (State == State.Configure)
			{
				Name = BarsPeriod.Value.ToString() + " HFTBars";
		        Properties.Remove(Properties.Find("BasePeriodType", true));
		        Properties.Remove(Properties.Find("BaseBarsPeriodType", true));
		        Properties.Remove(Properties.Find("BaseBarsPeriodValue", true));
		        Properties.Remove(Properties.Find("PointAndFigurePriceType", true));
		        Properties.Remove(Properties.Find("ReversalType", true));
		        Properties.Remove(Properties.Find("BasePeriodValue", true));
		        Properties.Remove(Properties.Find("Value2", true));
		        SetPropertyName("Value", "HFTBar Size");
			}
		}

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
	      if (SessionIterator == null)
	        SessionIterator = new SessionIterator(bars);
	      bool flag;
	      if (flag = SessionIterator.IsNewSession(time, isBar))
	        SessionIterator.CalculateTradingDay(time, isBar);
	      if (bars.Count != 0 && (!bars.IsResetOnNewTradingDay || !flag))
	      {
	        if (barMax == 0.0 || barMin == 0.0)
	        {
	          trendOffset = bars.BarsPeriod.Value * bars.Instrument.MasterInstrument.TickSize;
	          reversalOffset = (bars.BarsPeriod.Value * 2 + 1) * bars.Instrument.MasterInstrument.TickSize;
	          openOffset = Math.Ceiling(bars.BarsPeriod.Value * 1.0) * bars.Instrument.MasterInstrument.TickSize;
	          if (bars.Count == 1)
	          {
	            barMax = bars.GetOpen(bars.Count - 1) + trendOffset;
	            barMin = bars.GetOpen(bars.Count - 1) - trendOffset;
	          }
	          else if (bars.GetClose(bars.Count - 2) > bars.GetOpen(bars.Count - 2))
	          {
	            barMax = bars.GetClose(bars.Count - 2) + trendOffset;
	            barMin = bars.GetClose(bars.Count - 2) - trendOffset * 2.0;
	          }
	          else
	          {
	            barMax = bars.GetClose(bars.Count - 2) + trendOffset * 2.0;
	            barMin = bars.GetClose(bars.Count - 2) - trendOffset;
	          }
	        }
	        maxExceeded = bars.Instrument.MasterInstrument.Compare(close, barMax) > 0;
	        minExceeded = bars.Instrument.MasterInstrument.Compare(close, barMin) < 0;
	        if (!maxExceeded && !minExceeded)
	        {
	          UpdateBar(bars, close > bars.GetHigh(bars.Count - 1) ? close : bars.GetHigh(bars.Count - 1), close < bars.GetLow(bars.Count - 1) ? close : bars.GetLow(bars.Count - 1), close, time, volume);
	        }
	        else
	        {
	          double num = maxExceeded ? Math.Min(close, barMax) : (minExceeded ? Math.Max(close, barMin) : close);
	          barDirection = maxExceeded ? 1 : (minExceeded ? -1 : 0);
	          fakeOpen = num - openOffset * barDirection;
	          UpdateBar(bars, maxExceeded ? num : bars.GetHigh(bars.Count - 1), minExceeded ? num : bars.GetLow(bars.Count - 1), num, time, volume);
	          barOpen = close;
	          barMax = num + (barDirection > 0 ? trendOffset : reversalOffset);
	          barMin = num - (barDirection > 0 ? reversalOffset : trendOffset);
	          AddBar(bars, fakeOpen, maxExceeded ? num : fakeOpen, minExceeded ? num : fakeOpen, num, time, volume);
	        }
	      }
	      else
	      {
	        tickSize = bars.Instrument.MasterInstrument.TickSize;
	        if (bars.BarsPeriod.Value >= 1000000)
	        {
	          string str = bars.BarsPeriod.Value.ToString("000000000");
	          int result = 0;
	          int.TryParse(str.Substring(0, 3), out result);
	          bars.BarsPeriod.Value = result;
	          result = 0;
	          int.TryParse(str.Substring(3, 3), out result);
	          bars.BarsPeriod.Value2 = result;
	          result = 0;
	          int.TryParse(str.Substring(6, 3), out result);
	          bars.BarsPeriod.BaseBarsPeriodValue = result;
	        }
	        if (bars.Count != 0)
	        {
	          bars.GetClose(bars.Count - 1);
	          bars.GetTime(bars.Count - 1);
	          bars.GetVolume(bars.Count - 1);
	        }
	        trendOffset = bars.BarsPeriod.Value * bars.Instrument.MasterInstrument.TickSize;
	        reversalOffset = (bars.BarsPeriod.Value * 2 + 1) * bars.Instrument.MasterInstrument.TickSize;
	        openOffset = Math.Ceiling(bars.BarsPeriod.Value * 1.0) * bars.Instrument.MasterInstrument.TickSize;
	        barOpen = close;
	        barMax = barOpen + trendOffset * barDirection;
	        barMin = barOpen - trendOffset * barDirection;
	        AddBar(bars, barOpen, barOpen, barOpen, barOpen, time, volume);
	      }
	      bars.LastPrice = close;
	    }

	    public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
	    {
	      period.Value = 1;
	      period.Value2 = period.Value * 2 + 1;
	      period.BaseBarsPeriodValue = period.Value;
	    }

	    [Browsable(false)]
	    public override string ChartLabel(DateTime time) 
		{return time.ToString("T", (IFormatProvider) Core.Globals.GeneralOptions.CurrentCulture);}

	    [Browsable(false)]
	    public override void ApplyDefaultValue(BarsPeriod period) 
		{period.Value = 1;}

	    [Browsable(false)]
	    public override double GetPercentComplete(Bars bars, DateTime now) 
		{return 0.0;}

	    [Browsable(false)]
	    public override object Clone() 
		{return new HFTAlgoBarsType();}

	    [Browsable(false)]
	    public override int GetInitialLookBackDays(
	      BarsPeriod period,
	      TradingHours tradingHours,
	      int barsBack)
	    {
	      return 3;
	    }

	    [Browsable(false)]
	    public override string DisplayName
		{
		get {return " HFTBars";}
		}

	    [Browsable(false)]
	    public int GetPeriodValue(BarsPeriod period)
		{return period.Value;}

	    [Browsable(false)]
	    public string ToString(BarsPeriod period) 
		{return period.Value.ToString() + " HFTBars " + period.Value;}
  }
}
