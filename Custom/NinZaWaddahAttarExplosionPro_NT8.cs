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
		
		private ninZaWaddahAttarExplosionPro[] cacheninZaWaddahAttarExplosionPro;

		
		public ninZaWaddahAttarExplosionPro ninZaWaddahAttarExplosionPro(double sensitivity, ninZa_MAType mACDFastMAType, int mACDFastPeriod, bool mACDFastSmoothingEnabled, ninZa_MAType mACDFastSmoothingMethod, int mACDFastSmoothingPeriod, ninZa_MAType mACDSlowMAType, int mACDSlowPeriod, bool mACDSlowSmoothingEnabled, ninZa_MAType mACDSlowSmoothingMethod, int mACDSlowSmoothingPeriod, int bollingerPeriod, double bollingerStdDevMultiplier, bool bollingerStdDevSmoothingEnabled, ninZa_MAType bollingerStdDevSmoothingMethod, int bollingerStdDevSmoothingPeriod, bool deadZoneFilterEnabled, double zoneMultiplierTolerance, double zoneMultiplierDead, int zoneATRPeriod)
		{
			return ninZaWaddahAttarExplosionPro(Input, sensitivity, mACDFastMAType, mACDFastPeriod, mACDFastSmoothingEnabled, mACDFastSmoothingMethod, mACDFastSmoothingPeriod, mACDSlowMAType, mACDSlowPeriod, mACDSlowSmoothingEnabled, mACDSlowSmoothingMethod, mACDSlowSmoothingPeriod, bollingerPeriod, bollingerStdDevMultiplier, bollingerStdDevSmoothingEnabled, bollingerStdDevSmoothingMethod, bollingerStdDevSmoothingPeriod, deadZoneFilterEnabled, zoneMultiplierTolerance, zoneMultiplierDead, zoneATRPeriod);
		}


		
		public ninZaWaddahAttarExplosionPro ninZaWaddahAttarExplosionPro(ISeries<double> input, double sensitivity, ninZa_MAType mACDFastMAType, int mACDFastPeriod, bool mACDFastSmoothingEnabled, ninZa_MAType mACDFastSmoothingMethod, int mACDFastSmoothingPeriod, ninZa_MAType mACDSlowMAType, int mACDSlowPeriod, bool mACDSlowSmoothingEnabled, ninZa_MAType mACDSlowSmoothingMethod, int mACDSlowSmoothingPeriod, int bollingerPeriod, double bollingerStdDevMultiplier, bool bollingerStdDevSmoothingEnabled, ninZa_MAType bollingerStdDevSmoothingMethod, int bollingerStdDevSmoothingPeriod, bool deadZoneFilterEnabled, double zoneMultiplierTolerance, double zoneMultiplierDead, int zoneATRPeriod)
		{
			if (cacheninZaWaddahAttarExplosionPro != null)
				for (int idx = 0; idx < cacheninZaWaddahAttarExplosionPro.Length; idx++)
					if (cacheninZaWaddahAttarExplosionPro[idx].Sensitivity == sensitivity && cacheninZaWaddahAttarExplosionPro[idx].MACDFastMAType == mACDFastMAType && cacheninZaWaddahAttarExplosionPro[idx].MACDFastPeriod == mACDFastPeriod && cacheninZaWaddahAttarExplosionPro[idx].MACDFastSmoothingEnabled == mACDFastSmoothingEnabled && cacheninZaWaddahAttarExplosionPro[idx].MACDFastSmoothingMethod == mACDFastSmoothingMethod && cacheninZaWaddahAttarExplosionPro[idx].MACDFastSmoothingPeriod == mACDFastSmoothingPeriod && cacheninZaWaddahAttarExplosionPro[idx].MACDSlowMAType == mACDSlowMAType && cacheninZaWaddahAttarExplosionPro[idx].MACDSlowPeriod == mACDSlowPeriod && cacheninZaWaddahAttarExplosionPro[idx].MACDSlowSmoothingEnabled == mACDSlowSmoothingEnabled && cacheninZaWaddahAttarExplosionPro[idx].MACDSlowSmoothingMethod == mACDSlowSmoothingMethod && cacheninZaWaddahAttarExplosionPro[idx].MACDSlowSmoothingPeriod == mACDSlowSmoothingPeriod && cacheninZaWaddahAttarExplosionPro[idx].BollingerPeriod == bollingerPeriod && cacheninZaWaddahAttarExplosionPro[idx].BollingerStdDevMultiplier == bollingerStdDevMultiplier && cacheninZaWaddahAttarExplosionPro[idx].BollingerStdDevSmoothingEnabled == bollingerStdDevSmoothingEnabled && cacheninZaWaddahAttarExplosionPro[idx].BollingerStdDevSmoothingMethod == bollingerStdDevSmoothingMethod && cacheninZaWaddahAttarExplosionPro[idx].BollingerStdDevSmoothingPeriod == bollingerStdDevSmoothingPeriod && cacheninZaWaddahAttarExplosionPro[idx].DeadZoneFilterEnabled == deadZoneFilterEnabled && cacheninZaWaddahAttarExplosionPro[idx].ZoneMultiplierTolerance == zoneMultiplierTolerance && cacheninZaWaddahAttarExplosionPro[idx].ZoneMultiplierDead == zoneMultiplierDead && cacheninZaWaddahAttarExplosionPro[idx].ZoneATRPeriod == zoneATRPeriod && cacheninZaWaddahAttarExplosionPro[idx].EqualsInput(input))
						return cacheninZaWaddahAttarExplosionPro[idx];
			return CacheIndicator<ninZaWaddahAttarExplosionPro>(new ninZaWaddahAttarExplosionPro(){ Sensitivity = sensitivity, MACDFastMAType = mACDFastMAType, MACDFastPeriod = mACDFastPeriod, MACDFastSmoothingEnabled = mACDFastSmoothingEnabled, MACDFastSmoothingMethod = mACDFastSmoothingMethod, MACDFastSmoothingPeriod = mACDFastSmoothingPeriod, MACDSlowMAType = mACDSlowMAType, MACDSlowPeriod = mACDSlowPeriod, MACDSlowSmoothingEnabled = mACDSlowSmoothingEnabled, MACDSlowSmoothingMethod = mACDSlowSmoothingMethod, MACDSlowSmoothingPeriod = mACDSlowSmoothingPeriod, BollingerPeriod = bollingerPeriod, BollingerStdDevMultiplier = bollingerStdDevMultiplier, BollingerStdDevSmoothingEnabled = bollingerStdDevSmoothingEnabled, BollingerStdDevSmoothingMethod = bollingerStdDevSmoothingMethod, BollingerStdDevSmoothingPeriod = bollingerStdDevSmoothingPeriod, DeadZoneFilterEnabled = deadZoneFilterEnabled, ZoneMultiplierTolerance = zoneMultiplierTolerance, ZoneMultiplierDead = zoneMultiplierDead, ZoneATRPeriod = zoneATRPeriod }, input, ref cacheninZaWaddahAttarExplosionPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaWaddahAttarExplosionPro ninZaWaddahAttarExplosionPro(double sensitivity, ninZa_MAType mACDFastMAType, int mACDFastPeriod, bool mACDFastSmoothingEnabled, ninZa_MAType mACDFastSmoothingMethod, int mACDFastSmoothingPeriod, ninZa_MAType mACDSlowMAType, int mACDSlowPeriod, bool mACDSlowSmoothingEnabled, ninZa_MAType mACDSlowSmoothingMethod, int mACDSlowSmoothingPeriod, int bollingerPeriod, double bollingerStdDevMultiplier, bool bollingerStdDevSmoothingEnabled, ninZa_MAType bollingerStdDevSmoothingMethod, int bollingerStdDevSmoothingPeriod, bool deadZoneFilterEnabled, double zoneMultiplierTolerance, double zoneMultiplierDead, int zoneATRPeriod)
		{
			return indicator.ninZaWaddahAttarExplosionPro(Input, sensitivity, mACDFastMAType, mACDFastPeriod, mACDFastSmoothingEnabled, mACDFastSmoothingMethod, mACDFastSmoothingPeriod, mACDSlowMAType, mACDSlowPeriod, mACDSlowSmoothingEnabled, mACDSlowSmoothingMethod, mACDSlowSmoothingPeriod, bollingerPeriod, bollingerStdDevMultiplier, bollingerStdDevSmoothingEnabled, bollingerStdDevSmoothingMethod, bollingerStdDevSmoothingPeriod, deadZoneFilterEnabled, zoneMultiplierTolerance, zoneMultiplierDead, zoneATRPeriod);
		}


		
		public Indicators.ninZaWaddahAttarExplosionPro ninZaWaddahAttarExplosionPro(ISeries<double> input , double sensitivity, ninZa_MAType mACDFastMAType, int mACDFastPeriod, bool mACDFastSmoothingEnabled, ninZa_MAType mACDFastSmoothingMethod, int mACDFastSmoothingPeriod, ninZa_MAType mACDSlowMAType, int mACDSlowPeriod, bool mACDSlowSmoothingEnabled, ninZa_MAType mACDSlowSmoothingMethod, int mACDSlowSmoothingPeriod, int bollingerPeriod, double bollingerStdDevMultiplier, bool bollingerStdDevSmoothingEnabled, ninZa_MAType bollingerStdDevSmoothingMethod, int bollingerStdDevSmoothingPeriod, bool deadZoneFilterEnabled, double zoneMultiplierTolerance, double zoneMultiplierDead, int zoneATRPeriod)
		{
			return indicator.ninZaWaddahAttarExplosionPro(input, sensitivity, mACDFastMAType, mACDFastPeriod, mACDFastSmoothingEnabled, mACDFastSmoothingMethod, mACDFastSmoothingPeriod, mACDSlowMAType, mACDSlowPeriod, mACDSlowSmoothingEnabled, mACDSlowSmoothingMethod, mACDSlowSmoothingPeriod, bollingerPeriod, bollingerStdDevMultiplier, bollingerStdDevSmoothingEnabled, bollingerStdDevSmoothingMethod, bollingerStdDevSmoothingPeriod, deadZoneFilterEnabled, zoneMultiplierTolerance, zoneMultiplierDead, zoneATRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaWaddahAttarExplosionPro ninZaWaddahAttarExplosionPro(double sensitivity, ninZa_MAType mACDFastMAType, int mACDFastPeriod, bool mACDFastSmoothingEnabled, ninZa_MAType mACDFastSmoothingMethod, int mACDFastSmoothingPeriod, ninZa_MAType mACDSlowMAType, int mACDSlowPeriod, bool mACDSlowSmoothingEnabled, ninZa_MAType mACDSlowSmoothingMethod, int mACDSlowSmoothingPeriod, int bollingerPeriod, double bollingerStdDevMultiplier, bool bollingerStdDevSmoothingEnabled, ninZa_MAType bollingerStdDevSmoothingMethod, int bollingerStdDevSmoothingPeriod, bool deadZoneFilterEnabled, double zoneMultiplierTolerance, double zoneMultiplierDead, int zoneATRPeriod)
		{
			return indicator.ninZaWaddahAttarExplosionPro(Input, sensitivity, mACDFastMAType, mACDFastPeriod, mACDFastSmoothingEnabled, mACDFastSmoothingMethod, mACDFastSmoothingPeriod, mACDSlowMAType, mACDSlowPeriod, mACDSlowSmoothingEnabled, mACDSlowSmoothingMethod, mACDSlowSmoothingPeriod, bollingerPeriod, bollingerStdDevMultiplier, bollingerStdDevSmoothingEnabled, bollingerStdDevSmoothingMethod, bollingerStdDevSmoothingPeriod, deadZoneFilterEnabled, zoneMultiplierTolerance, zoneMultiplierDead, zoneATRPeriod);
		}


		
		public Indicators.ninZaWaddahAttarExplosionPro ninZaWaddahAttarExplosionPro(ISeries<double> input , double sensitivity, ninZa_MAType mACDFastMAType, int mACDFastPeriod, bool mACDFastSmoothingEnabled, ninZa_MAType mACDFastSmoothingMethod, int mACDFastSmoothingPeriod, ninZa_MAType mACDSlowMAType, int mACDSlowPeriod, bool mACDSlowSmoothingEnabled, ninZa_MAType mACDSlowSmoothingMethod, int mACDSlowSmoothingPeriod, int bollingerPeriod, double bollingerStdDevMultiplier, bool bollingerStdDevSmoothingEnabled, ninZa_MAType bollingerStdDevSmoothingMethod, int bollingerStdDevSmoothingPeriod, bool deadZoneFilterEnabled, double zoneMultiplierTolerance, double zoneMultiplierDead, int zoneATRPeriod)
		{
			return indicator.ninZaWaddahAttarExplosionPro(input, sensitivity, mACDFastMAType, mACDFastPeriod, mACDFastSmoothingEnabled, mACDFastSmoothingMethod, mACDFastSmoothingPeriod, mACDSlowMAType, mACDSlowPeriod, mACDSlowSmoothingEnabled, mACDSlowSmoothingMethod, mACDSlowSmoothingPeriod, bollingerPeriod, bollingerStdDevMultiplier, bollingerStdDevSmoothingEnabled, bollingerStdDevSmoothingMethod, bollingerStdDevSmoothingPeriod, deadZoneFilterEnabled, zoneMultiplierTolerance, zoneMultiplierDead, zoneATRPeriod);
		}

	}
}

#endregion
