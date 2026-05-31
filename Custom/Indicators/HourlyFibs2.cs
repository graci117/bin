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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators.LizardIndicators;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;

#endregion
namespace NinjaTrader.NinjaScript.Indicators
{
    public class HourlyFibs2 : Indicator
    {
        private Dictionary<string, DateTime> endTimes = new Dictionary<string, DateTime>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Hourly Fibonacci Levels with Historical Support";
                Name = "HourlyFibs2";
                IsOverlay = true;
                Calculate = Calculate.OnBarClose;
            }
        }

        protected override void OnBarUpdate()
        {
            if (State == State.Historical)
                ProcessHistoricalData();
            else
                ProcessRealtimeData();
        }

        private void ProcessHistoricalData()
        {
            DateTime currentTime = Time[0];
            if (currentTime.Minute != 10) return;

            DateTime calculationStart = GetCalculationStartTime(currentTime);
            DateTime lineStart = calculationStart;
            DateTime lineEnd = calculationStart.AddHours(1);

            if (lineStart.Date != currentTime.Date && currentTime.Hour == 0)
                lineStart = lineStart.AddDays(-1);

            double high = CalculateHigh(calculationStart, currentTime);
            double low = CalculateLow(calculationStart, currentTime);
            
            if (high == double.MinValue || low == double.MaxValue) return;

            DrawFibLevels(high, low, lineStart, lineEnd);
        }

        private void ProcessRealtimeData()
        {
            CleanupExpiredLines(Time[0]);
            
            DateTime currentTime = Time[0];
            if (currentTime.Minute != 10) return;

            DateTime lineStart = GetCalculationStartTime(currentTime);
            DateTime lineEnd = lineStart.AddHours(1);

            double high = MAX(High, 20)[0];
            double low = MIN(Low, 20)[0];

            DrawFibLevels(high, low, lineStart, lineEnd);
            RecordExpirationTimes(lineStart, lineEnd);
        }

        private DateTime GetCalculationStartTime(DateTime currentTime)
        {
            if (currentTime.Hour == 0)
                return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day - 1, 23, 50, 0);
            
            return currentTime.AddHours(-1).AddMinutes(-10).AddMinutes(50);
        }

       private double CalculateHigh(DateTime startTime, DateTime endTime)
		{
		    double high = double.MinValue;
		    for (int i = 0; i < CurrentBar; i++)
		    {
		        DateTime barTime = Bars.GetTime(i); // Get actual bar timestamp
		        if (barTime >= startTime && barTime <= endTime)
		            high = Math.Max(high, High[i]);
		    }
		    return high;
		}

        private double CalculateLow(DateTime startTime, DateTime endTime)
        {
            double low = double.MaxValue;
            for (int i = 0; i < CurrentBar; i++)
            {
				 DateTime barTime = Bars.GetTime(i); //
                if (barTime >= startTime && barTime <= endTime)
                    low = Math.Min(low, Low[i]);
            }
            return low;
        }

        private void DrawFibLevels(double high, double low, DateTime startTime, DateTime endTime)
        {
            string tagBase = startTime.ToString("yyyyMMddHHmm");
            double mid = (high + low) / 2;
            double range = high - low;

            DrawLineWithTag(tagBase + "_HIGH", startTime, endTime, high, DashStyleHelper.Solid, 2, Brushes.White);
            DrawLineWithTag(tagBase + "_LOW", startTime, endTime, low, DashStyleHelper.Solid, 2, Brushes.White);
            DrawLineWithTag(tagBase + "_MID", startTime, endTime, mid, DashStyleHelper.Dash, 3, Brushes.Yellow);
            DrawLineWithTag(tagBase + "_UPPER", startTime, endTime, high + range * 0.25, DashStyleHelper.DashDot, 1, Brushes.Magenta);
            DrawLineWithTag(tagBase + "_LOWER", startTime, endTime, low - range * 0.25, DashStyleHelper.Dash, 1, Brushes.Magenta);
        }

        private void DrawLineWithTag(string tag, DateTime start, DateTime end, double price, DashStyleHelper style, int width, Brush brush)
        {
            Draw.Line(
                this,
                tag,
                false,          // isAutoScale
                start,          // startTime
                price,          // startY
                end,            // endTime
                price,          // endY
                brush,
                style,
                width
            );
        }

        private void RecordExpirationTimes(DateTime lineStart, DateTime lineEnd)
        {
            string tagBase = lineStart.ToString("yyyyMMddHHmm");
            foreach (string suffix in new[] { "_HIGH", "_LOW", "_MID", "_UPPER", "_LOWER" })
                endTimes[tagBase + suffix] = lineEnd;
        }

        private void CleanupExpiredLines(DateTime currentTime)
        {
            if (State == State.Historical) return;

            List<string> toRemove = new List<string>();
            foreach (var kvp in endTimes)
            {
                if (currentTime >= kvp.Value)
                {
                    RemoveDrawObject(kvp.Key);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (string key in toRemove)
                endTimes.Remove(key);
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HourlyFibs2[] cacheHourlyFibs2;
		public HourlyFibs2 HourlyFibs2()
		{
			return HourlyFibs2(Input);
		}

		public HourlyFibs2 HourlyFibs2(ISeries<double> input)
		{
			if (cacheHourlyFibs2 != null)
				for (int idx = 0; idx < cacheHourlyFibs2.Length; idx++)
					if (cacheHourlyFibs2[idx] != null &&  cacheHourlyFibs2[idx].EqualsInput(input))
						return cacheHourlyFibs2[idx];
			return CacheIndicator<HourlyFibs2>(new HourlyFibs2(), input, ref cacheHourlyFibs2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HourlyFibs2 HourlyFibs2()
		{
			return indicator.HourlyFibs2(Input);
		}

		public Indicators.HourlyFibs2 HourlyFibs2(ISeries<double> input )
		{
			return indicator.HourlyFibs2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HourlyFibs2 HourlyFibs2()
		{
			return indicator.HourlyFibs2(Input);
		}

		public Indicators.HourlyFibs2 HourlyFibs2(ISeries<double> input )
		{
			return indicator.HourlyFibs2(input);
		}
	}
}

#endregion
