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
		
		private ninZaTrioReversal[] cacheninZaTrioReversal;

		
		public ninZaTrioReversal ninZaTrioReversal(bool strictModeEnabled, bool minimumSignalBarEnabled, double minimumSignalBarMultiplier, bool minimumCenterBarEnabled, double minimumCenterBarMultiplier, int aTRPeriod)
		{
			return ninZaTrioReversal(Input, strictModeEnabled, minimumSignalBarEnabled, minimumSignalBarMultiplier, minimumCenterBarEnabled, minimumCenterBarMultiplier, aTRPeriod);
		}


		
		public ninZaTrioReversal ninZaTrioReversal(ISeries<double> input, bool strictModeEnabled, bool minimumSignalBarEnabled, double minimumSignalBarMultiplier, bool minimumCenterBarEnabled, double minimumCenterBarMultiplier, int aTRPeriod)
		{
			if (cacheninZaTrioReversal != null)
				for (int idx = 0; idx < cacheninZaTrioReversal.Length; idx++)
					if (cacheninZaTrioReversal[idx].StrictModeEnabled == strictModeEnabled && cacheninZaTrioReversal[idx].MinimumSignalBarEnabled == minimumSignalBarEnabled && cacheninZaTrioReversal[idx].MinimumSignalBarMultiplier == minimumSignalBarMultiplier && cacheninZaTrioReversal[idx].MinimumCenterBarEnabled == minimumCenterBarEnabled && cacheninZaTrioReversal[idx].MinimumCenterBarMultiplier == minimumCenterBarMultiplier && cacheninZaTrioReversal[idx].ATRPeriod == aTRPeriod && cacheninZaTrioReversal[idx].EqualsInput(input))
						return cacheninZaTrioReversal[idx];
			return CacheIndicator<ninZaTrioReversal>(new ninZaTrioReversal(){ StrictModeEnabled = strictModeEnabled, MinimumSignalBarEnabled = minimumSignalBarEnabled, MinimumSignalBarMultiplier = minimumSignalBarMultiplier, MinimumCenterBarEnabled = minimumCenterBarEnabled, MinimumCenterBarMultiplier = minimumCenterBarMultiplier, ATRPeriod = aTRPeriod }, input, ref cacheninZaTrioReversal);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaTrioReversal ninZaTrioReversal(bool strictModeEnabled, bool minimumSignalBarEnabled, double minimumSignalBarMultiplier, bool minimumCenterBarEnabled, double minimumCenterBarMultiplier, int aTRPeriod)
		{
			return indicator.ninZaTrioReversal(Input, strictModeEnabled, minimumSignalBarEnabled, minimumSignalBarMultiplier, minimumCenterBarEnabled, minimumCenterBarMultiplier, aTRPeriod);
		}


		
		public Indicators.ninZaTrioReversal ninZaTrioReversal(ISeries<double> input , bool strictModeEnabled, bool minimumSignalBarEnabled, double minimumSignalBarMultiplier, bool minimumCenterBarEnabled, double minimumCenterBarMultiplier, int aTRPeriod)
		{
			return indicator.ninZaTrioReversal(input, strictModeEnabled, minimumSignalBarEnabled, minimumSignalBarMultiplier, minimumCenterBarEnabled, minimumCenterBarMultiplier, aTRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaTrioReversal ninZaTrioReversal(bool strictModeEnabled, bool minimumSignalBarEnabled, double minimumSignalBarMultiplier, bool minimumCenterBarEnabled, double minimumCenterBarMultiplier, int aTRPeriod)
		{
			return indicator.ninZaTrioReversal(Input, strictModeEnabled, minimumSignalBarEnabled, minimumSignalBarMultiplier, minimumCenterBarEnabled, minimumCenterBarMultiplier, aTRPeriod);
		}


		
		public Indicators.ninZaTrioReversal ninZaTrioReversal(ISeries<double> input , bool strictModeEnabled, bool minimumSignalBarEnabled, double minimumSignalBarMultiplier, bool minimumCenterBarEnabled, double minimumCenterBarMultiplier, int aTRPeriod)
		{
			return indicator.ninZaTrioReversal(input, strictModeEnabled, minimumSignalBarEnabled, minimumSignalBarMultiplier, minimumCenterBarEnabled, minimumCenterBarMultiplier, aTRPeriod);
		}

	}
}

#endregion
