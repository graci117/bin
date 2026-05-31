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
		
		private RenkoKings.RenkoKings_ZoneGPTFromTheFuture[] cacheRenkoKings_ZoneGPTFromTheFuture;

		
		public RenkoKings.RenkoKings_ZoneGPTFromTheFuture RenkoKings_ZoneGPTFromTheFuture(bool sensitiveModeEnabled, int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int flatMinLength, int zone_Height, int signal_Period, int signal_QuatityPerFlat)
		{
			return RenkoKings_ZoneGPTFromTheFuture(Input, sensitiveModeEnabled, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, flatMinLength, zone_Height, signal_Period, signal_QuatityPerFlat);
		}


		
		public RenkoKings.RenkoKings_ZoneGPTFromTheFuture RenkoKings_ZoneGPTFromTheFuture(ISeries<double> input, bool sensitiveModeEnabled, int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int flatMinLength, int zone_Height, int signal_Period, int signal_QuatityPerFlat)
		{
			if (cacheRenkoKings_ZoneGPTFromTheFuture != null)
				for (int idx = 0; idx < cacheRenkoKings_ZoneGPTFromTheFuture.Length; idx++)
					if (cacheRenkoKings_ZoneGPTFromTheFuture[idx].SensitiveModeEnabled == sensitiveModeEnabled && cacheRenkoKings_ZoneGPTFromTheFuture[idx].PeriodFast == periodFast && cacheRenkoKings_ZoneGPTFromTheFuture[idx].PeriodMedium == periodMedium && cacheRenkoKings_ZoneGPTFromTheFuture[idx].PeriodSlow == periodSlow && cacheRenkoKings_ZoneGPTFromTheFuture[idx].SmoothingEnabled == smoothingEnabled && cacheRenkoKings_ZoneGPTFromTheFuture[idx].SmoothingMethod == smoothingMethod && cacheRenkoKings_ZoneGPTFromTheFuture[idx].SmoothingPeriod == smoothingPeriod && cacheRenkoKings_ZoneGPTFromTheFuture[idx].FlatMinLength == flatMinLength && cacheRenkoKings_ZoneGPTFromTheFuture[idx].Zone_Height == zone_Height && cacheRenkoKings_ZoneGPTFromTheFuture[idx].Signal_Period == signal_Period && cacheRenkoKings_ZoneGPTFromTheFuture[idx].Signal_QuatityPerFlat == signal_QuatityPerFlat && cacheRenkoKings_ZoneGPTFromTheFuture[idx].EqualsInput(input))
						return cacheRenkoKings_ZoneGPTFromTheFuture[idx];
			return CacheIndicator<RenkoKings.RenkoKings_ZoneGPTFromTheFuture>(new RenkoKings.RenkoKings_ZoneGPTFromTheFuture(){ SensitiveModeEnabled = sensitiveModeEnabled, PeriodFast = periodFast, PeriodMedium = periodMedium, PeriodSlow = periodSlow, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, FlatMinLength = flatMinLength, Zone_Height = zone_Height, Signal_Period = signal_Period, Signal_QuatityPerFlat = signal_QuatityPerFlat }, input, ref cacheRenkoKings_ZoneGPTFromTheFuture);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.RenkoKings.RenkoKings_ZoneGPTFromTheFuture RenkoKings_ZoneGPTFromTheFuture(bool sensitiveModeEnabled, int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int flatMinLength, int zone_Height, int signal_Period, int signal_QuatityPerFlat)
		{
			return indicator.RenkoKings_ZoneGPTFromTheFuture(Input, sensitiveModeEnabled, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, flatMinLength, zone_Height, signal_Period, signal_QuatityPerFlat);
		}


		
		public Indicators.RenkoKings.RenkoKings_ZoneGPTFromTheFuture RenkoKings_ZoneGPTFromTheFuture(ISeries<double> input , bool sensitiveModeEnabled, int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int flatMinLength, int zone_Height, int signal_Period, int signal_QuatityPerFlat)
		{
			return indicator.RenkoKings_ZoneGPTFromTheFuture(input, sensitiveModeEnabled, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, flatMinLength, zone_Height, signal_Period, signal_QuatityPerFlat);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.RenkoKings.RenkoKings_ZoneGPTFromTheFuture RenkoKings_ZoneGPTFromTheFuture(bool sensitiveModeEnabled, int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int flatMinLength, int zone_Height, int signal_Period, int signal_QuatityPerFlat)
		{
			return indicator.RenkoKings_ZoneGPTFromTheFuture(Input, sensitiveModeEnabled, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, flatMinLength, zone_Height, signal_Period, signal_QuatityPerFlat);
		}


		
		public Indicators.RenkoKings.RenkoKings_ZoneGPTFromTheFuture RenkoKings_ZoneGPTFromTheFuture(ISeries<double> input , bool sensitiveModeEnabled, int periodFast, int periodMedium, int periodSlow, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, int flatMinLength, int zone_Height, int signal_Period, int signal_QuatityPerFlat)
		{
			return indicator.RenkoKings_ZoneGPTFromTheFuture(input, sensitiveModeEnabled, periodFast, periodMedium, periodSlow, smoothingEnabled, smoothingMethod, smoothingPeriod, flatMinLength, zone_Height, signal_Period, signal_QuatityPerFlat);
		}

	}
}

#endregion
