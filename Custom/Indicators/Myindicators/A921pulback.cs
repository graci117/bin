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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Myindicators
{
	public class A921pulback : Indicator
	{
		private EMA EMA1;
		private EMA EMA2;
		private EMA EntryEMA;

		// User-configurable properties for EMA periods
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Fast EMA Period", Order = 1, GroupName = "Parameters")]
		public int FastEmaPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Slow EMA Period", Order = 2, GroupName = "Parameters")]
		public int SlowEmaPeriod { get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Entry EMA Period", Order = 3, GroupName = "Parameters")]
		public int EntryEmaPeriod { get; set; }

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Enter the description for your new custom Strategy here.";
				Name = "a921pulback";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				DisplayInDataBox = true;
				DrawOnPricePanel = true;
				DrawHorizontalGridLines = true;
				DrawVerticalGridLines = true;
				PaintPriceMarkers = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;

				// Default values for the EMA periods
				FastEmaPeriod = 9;
				SlowEmaPeriod = 21;
				EntryEmaPeriod = 21;

				AddPlot(Brushes.Magenta, "EMA1"); // Plot for EMA1 (fast)
				AddPlot(Brushes.Cyan, "EMA2");    // Plot for EMA2 (slow)
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				EMA1 = EMA(Close, FastEmaPeriod);
				EMA2 = EMA(Close, SlowEmaPeriod);
				EntryEMA = EMA(Close, EntryEmaPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 5)
				return;

			// Assign values to the plots
			Values[0][0] = EMA1[0];
			Values[1][0] = EMA2[0];

			// Up arrow condition
			if ((EMA1[0] > EMA2[0]) && (CrossAbove(Close, EntryEMA, 1)))
			{
				Draw.ArrowUp(this, "ARROWUP" + CurrentBar, true, 0, Low[0] - TickSize, Brushes.Lime);
			}

			// Down arrow condition
			if ((EMA1[0] < EMA2[0]) && (CrossBelow(Close, EntryEMA, 1)))
			{
				Draw.ArrowDown(this, "ARROWDOWN" + CurrentBar, true, 0, High[0] + TickSize, Brushes.Red);
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Myindicators.A921pulback[] cacheA921pulback;
		public Myindicators.A921pulback A921pulback(int fastEmaPeriod, int slowEmaPeriod, int entryEmaPeriod)
		{
			return A921pulback(Input, fastEmaPeriod, slowEmaPeriod, entryEmaPeriod);
		}

		public Myindicators.A921pulback A921pulback(ISeries<double> input, int fastEmaPeriod, int slowEmaPeriod, int entryEmaPeriod)
		{
			if (cacheA921pulback != null)
				for (int idx = 0; idx < cacheA921pulback.Length; idx++)
					if (cacheA921pulback[idx] != null && cacheA921pulback[idx].FastEmaPeriod == fastEmaPeriod && cacheA921pulback[idx].SlowEmaPeriod == slowEmaPeriod && cacheA921pulback[idx].EntryEmaPeriod == entryEmaPeriod && cacheA921pulback[idx].EqualsInput(input))
						return cacheA921pulback[idx];
			return CacheIndicator<Myindicators.A921pulback>(new Myindicators.A921pulback(){ FastEmaPeriod = fastEmaPeriod, SlowEmaPeriod = slowEmaPeriod, EntryEmaPeriod = entryEmaPeriod }, input, ref cacheA921pulback);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Myindicators.A921pulback A921pulback(int fastEmaPeriod, int slowEmaPeriod, int entryEmaPeriod)
		{
			return indicator.A921pulback(Input, fastEmaPeriod, slowEmaPeriod, entryEmaPeriod);
		}

		public Indicators.Myindicators.A921pulback A921pulback(ISeries<double> input , int fastEmaPeriod, int slowEmaPeriod, int entryEmaPeriod)
		{
			return indicator.A921pulback(input, fastEmaPeriod, slowEmaPeriod, entryEmaPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Myindicators.A921pulback A921pulback(int fastEmaPeriod, int slowEmaPeriod, int entryEmaPeriod)
		{
			return indicator.A921pulback(Input, fastEmaPeriod, slowEmaPeriod, entryEmaPeriod);
		}

		public Indicators.Myindicators.A921pulback A921pulback(ISeries<double> input , int fastEmaPeriod, int slowEmaPeriod, int entryEmaPeriod)
		{
			return indicator.A921pulback(input, fastEmaPeriod, slowEmaPeriod, entryEmaPeriod);
		}
	}
}

#endregion
