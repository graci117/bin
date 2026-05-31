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



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private Qwave[] cacheQwave;

		
		public Qwave Qwave(int tMAPeriodBack, int aTRPeriodBack, double aTRMultiplier, double coeff, int aP, bool novolumedata, bool colorInsideBars, Brush barsInsideColor, bool alerts)
		{
			return Qwave(Input, tMAPeriodBack, aTRPeriodBack, aTRMultiplier, coeff, aP, novolumedata, colorInsideBars, barsInsideColor, alerts);
		}


		
		public Qwave Qwave(ISeries<double> input, int tMAPeriodBack, int aTRPeriodBack, double aTRMultiplier, double coeff, int aP, bool novolumedata, bool colorInsideBars, Brush barsInsideColor, bool alerts)
		{
			if (cacheQwave != null)
				for (int idx = 0; idx < cacheQwave.Length; idx++)
					if (cacheQwave[idx].TMAPeriodBack == tMAPeriodBack && cacheQwave[idx].ATRPeriodBack == aTRPeriodBack && cacheQwave[idx].ATRMultiplier == aTRMultiplier && cacheQwave[idx].Coeff == coeff && cacheQwave[idx].AP == aP && cacheQwave[idx].Novolumedata == novolumedata && cacheQwave[idx].ColorInsideBars == colorInsideBars && cacheQwave[idx].BarsInsideColor == barsInsideColor && cacheQwave[idx].Alerts == alerts && cacheQwave[idx].EqualsInput(input))
						return cacheQwave[idx];
			return CacheIndicator<Qwave>(new Qwave(){ TMAPeriodBack = tMAPeriodBack, ATRPeriodBack = aTRPeriodBack, ATRMultiplier = aTRMultiplier, Coeff = coeff, AP = aP, Novolumedata = novolumedata, ColorInsideBars = colorInsideBars, BarsInsideColor = barsInsideColor, Alerts = alerts }, input, ref cacheQwave);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.Qwave Qwave(int tMAPeriodBack, int aTRPeriodBack, double aTRMultiplier, double coeff, int aP, bool novolumedata, bool colorInsideBars, Brush barsInsideColor, bool alerts)
		{
			return indicator.Qwave(Input, tMAPeriodBack, aTRPeriodBack, aTRMultiplier, coeff, aP, novolumedata, colorInsideBars, barsInsideColor, alerts);
		}


		
		public Indicators.Qwave Qwave(ISeries<double> input , int tMAPeriodBack, int aTRPeriodBack, double aTRMultiplier, double coeff, int aP, bool novolumedata, bool colorInsideBars, Brush barsInsideColor, bool alerts)
		{
			return indicator.Qwave(input, tMAPeriodBack, aTRPeriodBack, aTRMultiplier, coeff, aP, novolumedata, colorInsideBars, barsInsideColor, alerts);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.Qwave Qwave(int tMAPeriodBack, int aTRPeriodBack, double aTRMultiplier, double coeff, int aP, bool novolumedata, bool colorInsideBars, Brush barsInsideColor, bool alerts)
		{
			return indicator.Qwave(Input, tMAPeriodBack, aTRPeriodBack, aTRMultiplier, coeff, aP, novolumedata, colorInsideBars, barsInsideColor, alerts);
		}


		
		public Indicators.Qwave Qwave(ISeries<double> input , int tMAPeriodBack, int aTRPeriodBack, double aTRMultiplier, double coeff, int aP, bool novolumedata, bool colorInsideBars, Brush barsInsideColor, bool alerts)
		{
			return indicator.Qwave(input, tMAPeriodBack, aTRPeriodBack, aTRMultiplier, coeff, aP, novolumedata, colorInsideBars, barsInsideColor, alerts);
		}

	}
}

#endregion
