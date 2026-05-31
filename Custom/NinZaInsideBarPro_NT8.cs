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
		
		private ninZaInsideBarPro[] cacheninZaInsideBarPro;

		
		public ninZaInsideBarPro ninZaInsideBarPro(bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaInsideBarPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int offset, int midPercentage)
		{
			return ninZaInsideBarPro(Input, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, offset, midPercentage);
		}


		
		public ninZaInsideBarPro ninZaInsideBarPro(ISeries<double> input, bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaInsideBarPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int offset, int midPercentage)
		{
			if (cacheninZaInsideBarPro != null)
				for (int idx = 0; idx < cacheninZaInsideBarPro.Length; idx++)
					if (cacheninZaInsideBarPro[idx].StrictMode == strictMode && cacheninZaInsideBarPro[idx].MinimumMotherBarEnabled == minimumMotherBarEnabled && cacheninZaInsideBarPro[idx].MinimumMotherBarMultiplier == minimumMotherBarMultiplier && cacheninZaInsideBarPro[idx].MinimumMotherBarUnit == minimumMotherBarUnit && cacheninZaInsideBarPro[idx].MinimumMotherBarATRPeriod == minimumMotherBarATRPeriod && cacheninZaInsideBarPro[idx].Offset == offset && cacheninZaInsideBarPro[idx].MidPercentage == midPercentage && cacheninZaInsideBarPro[idx].EqualsInput(input))
						return cacheninZaInsideBarPro[idx];
			return CacheIndicator<ninZaInsideBarPro>(new ninZaInsideBarPro(){ StrictMode = strictMode, MinimumMotherBarEnabled = minimumMotherBarEnabled, MinimumMotherBarMultiplier = minimumMotherBarMultiplier, MinimumMotherBarUnit = minimumMotherBarUnit, MinimumMotherBarATRPeriod = minimumMotherBarATRPeriod, Offset = offset, MidPercentage = midPercentage }, input, ref cacheninZaInsideBarPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaInsideBarPro ninZaInsideBarPro(bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaInsideBarPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int offset, int midPercentage)
		{
			return indicator.ninZaInsideBarPro(Input, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, offset, midPercentage);
		}


		
		public Indicators.ninZaInsideBarPro ninZaInsideBarPro(ISeries<double> input , bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaInsideBarPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int offset, int midPercentage)
		{
			return indicator.ninZaInsideBarPro(input, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, offset, midPercentage);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaInsideBarPro ninZaInsideBarPro(bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaInsideBarPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int offset, int midPercentage)
		{
			return indicator.ninZaInsideBarPro(Input, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, offset, midPercentage);
		}


		
		public Indicators.ninZaInsideBarPro ninZaInsideBarPro(ISeries<double> input , bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaInsideBarPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int offset, int midPercentage)
		{
			return indicator.ninZaInsideBarPro(input, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, offset, midPercentage);
		}

	}
}

#endregion
