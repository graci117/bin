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
		
		private ninZaSpeedupZlert[] cacheninZaSpeedupZlert;

		
		public ninZaSpeedupZlert ninZaSpeedupZlert()
		{
			return ninZaSpeedupZlert(Input);
		}


		
		public ninZaSpeedupZlert ninZaSpeedupZlert(ISeries<double> input)
		{
			if (cacheninZaSpeedupZlert != null)
				for (int idx = 0; idx < cacheninZaSpeedupZlert.Length; idx++)
					if ( cacheninZaSpeedupZlert[idx].EqualsInput(input))
						return cacheninZaSpeedupZlert[idx];
			return CacheIndicator<ninZaSpeedupZlert>(new ninZaSpeedupZlert(), input, ref cacheninZaSpeedupZlert);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSpeedupZlert ninZaSpeedupZlert()
		{
			return indicator.ninZaSpeedupZlert(Input);
		}


		
		public Indicators.ninZaSpeedupZlert ninZaSpeedupZlert(ISeries<double> input )
		{
			return indicator.ninZaSpeedupZlert(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSpeedupZlert ninZaSpeedupZlert()
		{
			return indicator.ninZaSpeedupZlert(Input);
		}


		
		public Indicators.ninZaSpeedupZlert ninZaSpeedupZlert(ISeries<double> input )
		{
			return indicator.ninZaSpeedupZlert(input);
		}

	}
}

#endregion
