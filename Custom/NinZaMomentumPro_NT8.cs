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
		
		private ninZaMomentumPro[] cacheninZaMomentumPro;

		
		public ninZaMomentumPro ninZaMomentumPro(int period, ninZaMomentumPro_OffsetType offsetType, double offsetMultiplier, int offsetPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return ninZaMomentumPro(Input, period, offsetType, offsetMultiplier, offsetPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}


		
		public ninZaMomentumPro ninZaMomentumPro(ISeries<double> input, int period, ninZaMomentumPro_OffsetType offsetType, double offsetMultiplier, int offsetPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			if (cacheninZaMomentumPro != null)
				for (int idx = 0; idx < cacheninZaMomentumPro.Length; idx++)
					if (cacheninZaMomentumPro[idx].Period == period && cacheninZaMomentumPro[idx].OffsetType == offsetType && cacheninZaMomentumPro[idx].OffsetMultiplier == offsetMultiplier && cacheninZaMomentumPro[idx].OffsetPeriod == offsetPeriod && cacheninZaMomentumPro[idx].SmoothingEnabled == smoothingEnabled && cacheninZaMomentumPro[idx].SmoothingMethod == smoothingMethod && cacheninZaMomentumPro[idx].SmoothingPeriod == smoothingPeriod && cacheninZaMomentumPro[idx].EqualsInput(input))
						return cacheninZaMomentumPro[idx];
			return CacheIndicator<ninZaMomentumPro>(new ninZaMomentumPro(){ Period = period, OffsetType = offsetType, OffsetMultiplier = offsetMultiplier, OffsetPeriod = offsetPeriod, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod }, input, ref cacheninZaMomentumPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaMomentumPro ninZaMomentumPro(int period, ninZaMomentumPro_OffsetType offsetType, double offsetMultiplier, int offsetPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaMomentumPro(Input, period, offsetType, offsetMultiplier, offsetPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}


		
		public Indicators.ninZaMomentumPro ninZaMomentumPro(ISeries<double> input , int period, ninZaMomentumPro_OffsetType offsetType, double offsetMultiplier, int offsetPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaMomentumPro(input, period, offsetType, offsetMultiplier, offsetPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaMomentumPro ninZaMomentumPro(int period, ninZaMomentumPro_OffsetType offsetType, double offsetMultiplier, int offsetPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaMomentumPro(Input, period, offsetType, offsetMultiplier, offsetPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}


		
		public Indicators.ninZaMomentumPro ninZaMomentumPro(ISeries<double> input , int period, ninZaMomentumPro_OffsetType offsetType, double offsetMultiplier, int offsetPeriod, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod)
		{
			return indicator.ninZaMomentumPro(input, period, offsetType, offsetMultiplier, offsetPeriod, smoothingEnabled, smoothingMethod, smoothingPeriod);
		}

	}
}

#endregion
