/// <summary>
/// Dynamically plots the highest and lowest of the current hour.//
/// </summary>
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Data;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
#endregion
namespace NinjaTrader.NinjaScript.Indicators
{

	public class HourlyHL : Indicator
	{
			private int startOfHourBarIndex = -1;
			private int currentHour = -1;
	
			protected override void OnStateChange()
			{
				if (State == State.SetDefaults)
				{
				Description = "Plots the highest and lowest bars for each hour.";
				Name = "Hourly High Low";
				IsAutoScale = false;
				DrawOnPricePanel = false;
				IsOverlay = true;
				IsSuspendedWhileInactive = true;
				
				// AddPlot for Hourly High
				AddPlot(new Stroke(Brushes.SpringGreen, 2), PlotStyle.Square, "Hourly High");
				// AddPlot for Hourly Low
				AddPlot(new Stroke(Brushes.OrangeRed, 2), PlotStyle.Square, "Hourly Low");
				}
			}
	
	
			protected override void OnBarUpdate()
			{
					DateTime hourBeginning = Time[0].AddMinutes(-1 * Time[0].Minute).AddSeconds(-1 * Time[0].Second);
					int hour = hourBeginning.Hour;
					
					if (hour != currentHour)
					{
					// Start of a new hour, reset startOfHourBarIndex
					currentHour = hour;
					startOfHourBarIndex = CurrentBar;
					}
					
					// Calculate how many bars ago the current hour started
					int hourBeginBarsAgo = CurrentBar - startOfHourBarIndex;
					
					if (hourBeginBarsAgo >= 0)
					{
					double highestHigh = MAX(High, hourBeginBarsAgo + 1)[0];
					double lowestLow = MIN(Low, hourBeginBarsAgo + 1)[0];
					
					for (int index = 0; index <= hourBeginBarsAgo; index++)
					{
					Values[0][index] = highestHigh;
					Values[1][index] = lowestLow;
					}
			}
		}
	}	

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HourlyHL[] cacheHourlyHL;
		public HourlyHL HourlyHL()
		{
			return HourlyHL(Input);
		}

		public HourlyHL HourlyHL(ISeries<double> input)
		{
			if (cacheHourlyHL != null)
				for (int idx = 0; idx < cacheHourlyHL.Length; idx++)
					if (cacheHourlyHL[idx] != null &&  cacheHourlyHL[idx].EqualsInput(input))
						return cacheHourlyHL[idx];
			return CacheIndicator<HourlyHL>(new HourlyHL(), input, ref cacheHourlyHL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HourlyHL HourlyHL()
		{
			return indicator.HourlyHL(Input);
		}

		public Indicators.HourlyHL HourlyHL(ISeries<double> input )
		{
			return indicator.HourlyHL(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HourlyHL HourlyHL()
		{
			return indicator.HourlyHL(Input);
		}

		public Indicators.HourlyHL HourlyHL(ISeries<double> input )
		{
			return indicator.HourlyHL(input);
		}
	}
}

#endregion
