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
		
		private ninZaFakeyPro[] cacheninZaFakeyPro;

		
		public ninZaFakeyPro ninZaFakeyPro(int maxBars, ninZaFakeyPro_FilterUnit fakeyFilterUnit, int fakeyFilterBreakout, int fakeyFilterReturn, bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaFakeyPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int rangeOffset)
		{
			return ninZaFakeyPro(Input, maxBars, fakeyFilterUnit, fakeyFilterBreakout, fakeyFilterReturn, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, rangeOffset);
		}


		
		public ninZaFakeyPro ninZaFakeyPro(ISeries<double> input, int maxBars, ninZaFakeyPro_FilterUnit fakeyFilterUnit, int fakeyFilterBreakout, int fakeyFilterReturn, bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaFakeyPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int rangeOffset)
		{
			if (cacheninZaFakeyPro != null)
				for (int idx = 0; idx < cacheninZaFakeyPro.Length; idx++)
					if (cacheninZaFakeyPro[idx].MaxBars == maxBars && cacheninZaFakeyPro[idx].FakeyFilterUnit == fakeyFilterUnit && cacheninZaFakeyPro[idx].FakeyFilterBreakout == fakeyFilterBreakout && cacheninZaFakeyPro[idx].FakeyFilterReturn == fakeyFilterReturn && cacheninZaFakeyPro[idx].StrictMode == strictMode && cacheninZaFakeyPro[idx].MinimumMotherBarEnabled == minimumMotherBarEnabled && cacheninZaFakeyPro[idx].MinimumMotherBarMultiplier == minimumMotherBarMultiplier && cacheninZaFakeyPro[idx].MinimumMotherBarUnit == minimumMotherBarUnit && cacheninZaFakeyPro[idx].MinimumMotherBarATRPeriod == minimumMotherBarATRPeriod && cacheninZaFakeyPro[idx].RangeOffset == rangeOffset && cacheninZaFakeyPro[idx].EqualsInput(input))
						return cacheninZaFakeyPro[idx];
			return CacheIndicator<ninZaFakeyPro>(new ninZaFakeyPro(){ MaxBars = maxBars, FakeyFilterUnit = fakeyFilterUnit, FakeyFilterBreakout = fakeyFilterBreakout, FakeyFilterReturn = fakeyFilterReturn, StrictMode = strictMode, MinimumMotherBarEnabled = minimumMotherBarEnabled, MinimumMotherBarMultiplier = minimumMotherBarMultiplier, MinimumMotherBarUnit = minimumMotherBarUnit, MinimumMotherBarATRPeriod = minimumMotherBarATRPeriod, RangeOffset = rangeOffset }, input, ref cacheninZaFakeyPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaFakeyPro ninZaFakeyPro(int maxBars, ninZaFakeyPro_FilterUnit fakeyFilterUnit, int fakeyFilterBreakout, int fakeyFilterReturn, bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaFakeyPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int rangeOffset)
		{
			return indicator.ninZaFakeyPro(Input, maxBars, fakeyFilterUnit, fakeyFilterBreakout, fakeyFilterReturn, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, rangeOffset);
		}


		
		public Indicators.ninZaFakeyPro ninZaFakeyPro(ISeries<double> input , int maxBars, ninZaFakeyPro_FilterUnit fakeyFilterUnit, int fakeyFilterBreakout, int fakeyFilterReturn, bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaFakeyPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int rangeOffset)
		{
			return indicator.ninZaFakeyPro(input, maxBars, fakeyFilterUnit, fakeyFilterBreakout, fakeyFilterReturn, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, rangeOffset);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaFakeyPro ninZaFakeyPro(int maxBars, ninZaFakeyPro_FilterUnit fakeyFilterUnit, int fakeyFilterBreakout, int fakeyFilterReturn, bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaFakeyPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int rangeOffset)
		{
			return indicator.ninZaFakeyPro(Input, maxBars, fakeyFilterUnit, fakeyFilterBreakout, fakeyFilterReturn, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, rangeOffset);
		}


		
		public Indicators.ninZaFakeyPro ninZaFakeyPro(ISeries<double> input , int maxBars, ninZaFakeyPro_FilterUnit fakeyFilterUnit, int fakeyFilterBreakout, int fakeyFilterReturn, bool strictMode, bool minimumMotherBarEnabled, double minimumMotherBarMultiplier, ninZaFakeyPro_Unit minimumMotherBarUnit, int minimumMotherBarATRPeriod, int rangeOffset)
		{
			return indicator.ninZaFakeyPro(input, maxBars, fakeyFilterUnit, fakeyFilterBreakout, fakeyFilterReturn, strictMode, minimumMotherBarEnabled, minimumMotherBarMultiplier, minimumMotherBarUnit, minimumMotherBarATRPeriod, rangeOffset);
		}

	}
}

#endregion
