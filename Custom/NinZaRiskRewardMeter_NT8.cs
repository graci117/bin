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
		
		private ninZaRiskRewardMeter[] cacheninZaRiskRewardMeter;

		
		public ninZaRiskRewardMeter ninZaRiskRewardMeter()
		{
			return ninZaRiskRewardMeter(Input);
		}


		
		public ninZaRiskRewardMeter ninZaRiskRewardMeter(ISeries<double> input)
		{
			if (cacheninZaRiskRewardMeter != null)
				for (int idx = 0; idx < cacheninZaRiskRewardMeter.Length; idx++)
					if ( cacheninZaRiskRewardMeter[idx].EqualsInput(input))
						return cacheninZaRiskRewardMeter[idx];
			return CacheIndicator<ninZaRiskRewardMeter>(new ninZaRiskRewardMeter(), input, ref cacheninZaRiskRewardMeter);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaRiskRewardMeter ninZaRiskRewardMeter()
		{
			return indicator.ninZaRiskRewardMeter(Input);
		}


		
		public Indicators.ninZaRiskRewardMeter ninZaRiskRewardMeter(ISeries<double> input )
		{
			return indicator.ninZaRiskRewardMeter(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaRiskRewardMeter ninZaRiskRewardMeter()
		{
			return indicator.ninZaRiskRewardMeter(Input);
		}


		
		public Indicators.ninZaRiskRewardMeter ninZaRiskRewardMeter(ISeries<double> input )
		{
			return indicator.ninZaRiskRewardMeter(input);
		}

	}
}

#endregion
