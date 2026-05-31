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
		
		private ninZaFAMESuperPivot[] cacheninZaFAMESuperPivot;

		
		public ninZaFAMESuperPivot ninZaFAMESuperPivot(bool offsetAdjustmentEnabled, double offsetAdjustmentRatioThreshold)
		{
			return ninZaFAMESuperPivot(Input, offsetAdjustmentEnabled, offsetAdjustmentRatioThreshold);
		}


		
		public ninZaFAMESuperPivot ninZaFAMESuperPivot(ISeries<double> input, bool offsetAdjustmentEnabled, double offsetAdjustmentRatioThreshold)
		{
			if (cacheninZaFAMESuperPivot != null)
				for (int idx = 0; idx < cacheninZaFAMESuperPivot.Length; idx++)
					if (cacheninZaFAMESuperPivot[idx].OffsetAdjustmentEnabled == offsetAdjustmentEnabled && cacheninZaFAMESuperPivot[idx].OffsetAdjustmentRatioThreshold == offsetAdjustmentRatioThreshold && cacheninZaFAMESuperPivot[idx].EqualsInput(input))
						return cacheninZaFAMESuperPivot[idx];
			return CacheIndicator<ninZaFAMESuperPivot>(new ninZaFAMESuperPivot(){ OffsetAdjustmentEnabled = offsetAdjustmentEnabled, OffsetAdjustmentRatioThreshold = offsetAdjustmentRatioThreshold }, input, ref cacheninZaFAMESuperPivot);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaFAMESuperPivot ninZaFAMESuperPivot(bool offsetAdjustmentEnabled, double offsetAdjustmentRatioThreshold)
		{
			return indicator.ninZaFAMESuperPivot(Input, offsetAdjustmentEnabled, offsetAdjustmentRatioThreshold);
		}


		
		public Indicators.ninZaFAMESuperPivot ninZaFAMESuperPivot(ISeries<double> input , bool offsetAdjustmentEnabled, double offsetAdjustmentRatioThreshold)
		{
			return indicator.ninZaFAMESuperPivot(input, offsetAdjustmentEnabled, offsetAdjustmentRatioThreshold);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaFAMESuperPivot ninZaFAMESuperPivot(bool offsetAdjustmentEnabled, double offsetAdjustmentRatioThreshold)
		{
			return indicator.ninZaFAMESuperPivot(Input, offsetAdjustmentEnabled, offsetAdjustmentRatioThreshold);
		}


		
		public Indicators.ninZaFAMESuperPivot ninZaFAMESuperPivot(ISeries<double> input , bool offsetAdjustmentEnabled, double offsetAdjustmentRatioThreshold)
		{
			return indicator.ninZaFAMESuperPivot(input, offsetAdjustmentEnabled, offsetAdjustmentRatioThreshold);
		}

	}
}

#endregion
