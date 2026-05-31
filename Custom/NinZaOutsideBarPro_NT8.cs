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
		
		private ninZaOutsideBarPro[] cacheninZaOutsideBarPro;

		
		public ninZaOutsideBarPro ninZaOutsideBarPro(bool strictMode, bool minimumOutsideBarEnabled, double minimumOutsideBarMultiplier, ninZaOutsideBarPro_Unit minimumOutsideBarUnit, int minimumOutsideBarATRPeriod, ninZaOutsideBarPro_DirectionFilter directionFilter)
		{
			return ninZaOutsideBarPro(Input, strictMode, minimumOutsideBarEnabled, minimumOutsideBarMultiplier, minimumOutsideBarUnit, minimumOutsideBarATRPeriod, directionFilter);
		}


		
		public ninZaOutsideBarPro ninZaOutsideBarPro(ISeries<double> input, bool strictMode, bool minimumOutsideBarEnabled, double minimumOutsideBarMultiplier, ninZaOutsideBarPro_Unit minimumOutsideBarUnit, int minimumOutsideBarATRPeriod, ninZaOutsideBarPro_DirectionFilter directionFilter)
		{
			if (cacheninZaOutsideBarPro != null)
				for (int idx = 0; idx < cacheninZaOutsideBarPro.Length; idx++)
					if (cacheninZaOutsideBarPro[idx].StrictMode == strictMode && cacheninZaOutsideBarPro[idx].MinimumOutsideBarEnabled == minimumOutsideBarEnabled && cacheninZaOutsideBarPro[idx].MinimumOutsideBarMultiplier == minimumOutsideBarMultiplier && cacheninZaOutsideBarPro[idx].MinimumOutsideBarUnit == minimumOutsideBarUnit && cacheninZaOutsideBarPro[idx].MinimumOutsideBarATRPeriod == minimumOutsideBarATRPeriod && cacheninZaOutsideBarPro[idx].DirectionFilter == directionFilter && cacheninZaOutsideBarPro[idx].EqualsInput(input))
						return cacheninZaOutsideBarPro[idx];
			return CacheIndicator<ninZaOutsideBarPro>(new ninZaOutsideBarPro(){ StrictMode = strictMode, MinimumOutsideBarEnabled = minimumOutsideBarEnabled, MinimumOutsideBarMultiplier = minimumOutsideBarMultiplier, MinimumOutsideBarUnit = minimumOutsideBarUnit, MinimumOutsideBarATRPeriod = minimumOutsideBarATRPeriod, DirectionFilter = directionFilter }, input, ref cacheninZaOutsideBarPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaOutsideBarPro ninZaOutsideBarPro(bool strictMode, bool minimumOutsideBarEnabled, double minimumOutsideBarMultiplier, ninZaOutsideBarPro_Unit minimumOutsideBarUnit, int minimumOutsideBarATRPeriod, ninZaOutsideBarPro_DirectionFilter directionFilter)
		{
			return indicator.ninZaOutsideBarPro(Input, strictMode, minimumOutsideBarEnabled, minimumOutsideBarMultiplier, minimumOutsideBarUnit, minimumOutsideBarATRPeriod, directionFilter);
		}


		
		public Indicators.ninZaOutsideBarPro ninZaOutsideBarPro(ISeries<double> input , bool strictMode, bool minimumOutsideBarEnabled, double minimumOutsideBarMultiplier, ninZaOutsideBarPro_Unit minimumOutsideBarUnit, int minimumOutsideBarATRPeriod, ninZaOutsideBarPro_DirectionFilter directionFilter)
		{
			return indicator.ninZaOutsideBarPro(input, strictMode, minimumOutsideBarEnabled, minimumOutsideBarMultiplier, minimumOutsideBarUnit, minimumOutsideBarATRPeriod, directionFilter);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaOutsideBarPro ninZaOutsideBarPro(bool strictMode, bool minimumOutsideBarEnabled, double minimumOutsideBarMultiplier, ninZaOutsideBarPro_Unit minimumOutsideBarUnit, int minimumOutsideBarATRPeriod, ninZaOutsideBarPro_DirectionFilter directionFilter)
		{
			return indicator.ninZaOutsideBarPro(Input, strictMode, minimumOutsideBarEnabled, minimumOutsideBarMultiplier, minimumOutsideBarUnit, minimumOutsideBarATRPeriod, directionFilter);
		}


		
		public Indicators.ninZaOutsideBarPro ninZaOutsideBarPro(ISeries<double> input , bool strictMode, bool minimumOutsideBarEnabled, double minimumOutsideBarMultiplier, ninZaOutsideBarPro_Unit minimumOutsideBarUnit, int minimumOutsideBarATRPeriod, ninZaOutsideBarPro_DirectionFilter directionFilter)
		{
			return indicator.ninZaOutsideBarPro(input, strictMode, minimumOutsideBarEnabled, minimumOutsideBarMultiplier, minimumOutsideBarUnit, minimumOutsideBarATRPeriod, directionFilter);
		}

	}
}

#endregion
