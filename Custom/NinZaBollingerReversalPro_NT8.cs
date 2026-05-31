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
		
		private ninZaBollingerReversal[] cacheninZaBollingerReversal;

		
		public ninZaBollingerReversal ninZaBollingerReversal(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offset, double minProtrusion, int signalSplit)
		{
			return ninZaBollingerReversal(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offset, minProtrusion, signalSplit);
		}


		
		public ninZaBollingerReversal ninZaBollingerReversal(ISeries<double> input, ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offset, double minProtrusion, int signalSplit)
		{
			if (cacheninZaBollingerReversal != null)
				for (int idx = 0; idx < cacheninZaBollingerReversal.Length; idx++)
					if (cacheninZaBollingerReversal[idx].MAType == mAType && cacheninZaBollingerReversal[idx].Period == period && cacheninZaBollingerReversal[idx].SmoothingEnabled == smoothingEnabled && cacheninZaBollingerReversal[idx].SmoothingMethod == smoothingMethod && cacheninZaBollingerReversal[idx].SmoothingPeriod == smoothingPeriod && cacheninZaBollingerReversal[idx].Offset == offset && cacheninZaBollingerReversal[idx].MinProtrusion == minProtrusion && cacheninZaBollingerReversal[idx].SignalSplit == signalSplit && cacheninZaBollingerReversal[idx].EqualsInput(input))
						return cacheninZaBollingerReversal[idx];
			return CacheIndicator<ninZaBollingerReversal>(new ninZaBollingerReversal(){ MAType = mAType, Period = period, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, Offset = offset, MinProtrusion = minProtrusion, SignalSplit = signalSplit }, input, ref cacheninZaBollingerReversal);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaBollingerReversal ninZaBollingerReversal(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offset, double minProtrusion, int signalSplit)
		{
			return indicator.ninZaBollingerReversal(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offset, minProtrusion, signalSplit);
		}


		
		public Indicators.ninZaBollingerReversal ninZaBollingerReversal(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offset, double minProtrusion, int signalSplit)
		{
			return indicator.ninZaBollingerReversal(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offset, minProtrusion, signalSplit);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaBollingerReversal ninZaBollingerReversal(ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offset, double minProtrusion, int signalSplit)
		{
			return indicator.ninZaBollingerReversal(Input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offset, minProtrusion, signalSplit);
		}


		
		public Indicators.ninZaBollingerReversal ninZaBollingerReversal(ISeries<double> input , ninZa_MAType mAType, int period, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double offset, double minProtrusion, int signalSplit)
		{
			return indicator.ninZaBollingerReversal(input, mAType, period, smoothingEnabled, smoothingMethod, smoothingPeriod, offset, minProtrusion, signalSplit);
		}

	}
}

#endregion
