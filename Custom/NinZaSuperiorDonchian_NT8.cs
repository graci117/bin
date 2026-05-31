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
		
		private ninZaSuperiorDonchian[] cacheninZaSuperiorDonchian;

		
		public ninZaSuperiorDonchian ninZaSuperiorDonchian(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorDonchian_OffsetMethod offsetUnit, int offsetPeriod)
		{
			return ninZaSuperiorDonchian(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetPeriod);
		}


		
		public ninZaSuperiorDonchian ninZaSuperiorDonchian(ISeries<double> input, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorDonchian_OffsetMethod offsetUnit, int offsetPeriod)
		{
			if (cacheninZaSuperiorDonchian != null)
				for (int idx = 0; idx < cacheninZaSuperiorDonchian.Length; idx++)
					if (cacheninZaSuperiorDonchian[idx].Period == period && cacheninZaSuperiorDonchian[idx].SmoothingEnabled == smoothingEnabled && cacheninZaSuperiorDonchian[idx].SmoothingMethod == smoothingMethod && cacheninZaSuperiorDonchian[idx].SmoothingPeriod == smoothingPeriod && cacheninZaSuperiorDonchian[idx].OffsetMultiplier == offsetMultiplier && cacheninZaSuperiorDonchian[idx].OffsetUnit == offsetUnit && cacheninZaSuperiorDonchian[idx].OffsetPeriod == offsetPeriod && cacheninZaSuperiorDonchian[idx].EqualsInput(input))
						return cacheninZaSuperiorDonchian[idx];
			return CacheIndicator<ninZaSuperiorDonchian>(new ninZaSuperiorDonchian(){ Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, OffsetMultiplier = offsetMultiplier, OffsetUnit = offsetUnit, OffsetPeriod = offsetPeriod }, input, ref cacheninZaSuperiorDonchian);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSuperiorDonchian ninZaSuperiorDonchian(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorDonchian_OffsetMethod offsetUnit, int offsetPeriod)
		{
			return indicator.ninZaSuperiorDonchian(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetPeriod);
		}


		
		public Indicators.ninZaSuperiorDonchian ninZaSuperiorDonchian(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorDonchian_OffsetMethod offsetUnit, int offsetPeriod)
		{
			return indicator.ninZaSuperiorDonchian(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSuperiorDonchian ninZaSuperiorDonchian(int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorDonchian_OffsetMethod offsetUnit, int offsetPeriod)
		{
			return indicator.ninZaSuperiorDonchian(Input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetPeriod);
		}


		
		public Indicators.ninZaSuperiorDonchian ninZaSuperiorDonchian(ISeries<double> input , int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offsetMultiplier, ninZaSuperiorDonchian_OffsetMethod offsetUnit, int offsetPeriod)
		{
			return indicator.ninZaSuperiorDonchian(input, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offsetMultiplier, offsetUnit, offsetPeriod);
		}

	}
}

#endregion
