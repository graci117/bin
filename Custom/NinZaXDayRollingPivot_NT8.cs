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
		
		private ninZaXDayRollingPivot[] cacheninZaXDayRollingPivot;

		
		public ninZaXDayRollingPivot ninZaXDayRollingPivot(int dayNum, double minimumDiffPercentage, double factorS1R1, double factorS2R2, double factorS3R3, double factorS4R4)
		{
			return ninZaXDayRollingPivot(Input, dayNum, minimumDiffPercentage, factorS1R1, factorS2R2, factorS3R3, factorS4R4);
		}


		
		public ninZaXDayRollingPivot ninZaXDayRollingPivot(ISeries<double> input, int dayNum, double minimumDiffPercentage, double factorS1R1, double factorS2R2, double factorS3R3, double factorS4R4)
		{
			if (cacheninZaXDayRollingPivot != null)
				for (int idx = 0; idx < cacheninZaXDayRollingPivot.Length; idx++)
					if (cacheninZaXDayRollingPivot[idx].DayNum == dayNum && cacheninZaXDayRollingPivot[idx].MinimumDiffPercentage == minimumDiffPercentage && cacheninZaXDayRollingPivot[idx].FactorS1R1 == factorS1R1 && cacheninZaXDayRollingPivot[idx].FactorS2R2 == factorS2R2 && cacheninZaXDayRollingPivot[idx].FactorS3R3 == factorS3R3 && cacheninZaXDayRollingPivot[idx].FactorS4R4 == factorS4R4 && cacheninZaXDayRollingPivot[idx].EqualsInput(input))
						return cacheninZaXDayRollingPivot[idx];
			return CacheIndicator<ninZaXDayRollingPivot>(new ninZaXDayRollingPivot(){ DayNum = dayNum, MinimumDiffPercentage = minimumDiffPercentage, FactorS1R1 = factorS1R1, FactorS2R2 = factorS2R2, FactorS3R3 = factorS3R3, FactorS4R4 = factorS4R4 }, input, ref cacheninZaXDayRollingPivot);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaXDayRollingPivot ninZaXDayRollingPivot(int dayNum, double minimumDiffPercentage, double factorS1R1, double factorS2R2, double factorS3R3, double factorS4R4)
		{
			return indicator.ninZaXDayRollingPivot(Input, dayNum, minimumDiffPercentage, factorS1R1, factorS2R2, factorS3R3, factorS4R4);
		}


		
		public Indicators.ninZaXDayRollingPivot ninZaXDayRollingPivot(ISeries<double> input , int dayNum, double minimumDiffPercentage, double factorS1R1, double factorS2R2, double factorS3R3, double factorS4R4)
		{
			return indicator.ninZaXDayRollingPivot(input, dayNum, minimumDiffPercentage, factorS1R1, factorS2R2, factorS3R3, factorS4R4);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaXDayRollingPivot ninZaXDayRollingPivot(int dayNum, double minimumDiffPercentage, double factorS1R1, double factorS2R2, double factorS3R3, double factorS4R4)
		{
			return indicator.ninZaXDayRollingPivot(Input, dayNum, minimumDiffPercentage, factorS1R1, factorS2R2, factorS3R3, factorS4R4);
		}


		
		public Indicators.ninZaXDayRollingPivot ninZaXDayRollingPivot(ISeries<double> input , int dayNum, double minimumDiffPercentage, double factorS1R1, double factorS2R2, double factorS3R3, double factorS4R4)
		{
			return indicator.ninZaXDayRollingPivot(input, dayNum, minimumDiffPercentage, factorS1R1, factorS2R2, factorS3R3, factorS4R4);
		}

	}
}

#endregion
