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
		
		private ninZaSupportResistanceRadar[] cacheninZaSupportResistanceRadar;

		
		public ninZaSupportResistanceRadar ninZaSupportResistanceRadar()
		{
			return ninZaSupportResistanceRadar(Input);
		}


		
		public ninZaSupportResistanceRadar ninZaSupportResistanceRadar(ISeries<double> input)
		{
			if (cacheninZaSupportResistanceRadar != null)
				for (int idx = 0; idx < cacheninZaSupportResistanceRadar.Length; idx++)
					if ( cacheninZaSupportResistanceRadar[idx].EqualsInput(input))
						return cacheninZaSupportResistanceRadar[idx];
			return CacheIndicator<ninZaSupportResistanceRadar>(new ninZaSupportResistanceRadar(), input, ref cacheninZaSupportResistanceRadar);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaSupportResistanceRadar ninZaSupportResistanceRadar()
		{
			return indicator.ninZaSupportResistanceRadar(Input);
		}


		
		public Indicators.ninZaSupportResistanceRadar ninZaSupportResistanceRadar(ISeries<double> input )
		{
			return indicator.ninZaSupportResistanceRadar(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaSupportResistanceRadar ninZaSupportResistanceRadar()
		{
			return indicator.ninZaSupportResistanceRadar(Input);
		}


		
		public Indicators.ninZaSupportResistanceRadar ninZaSupportResistanceRadar(ISeries<double> input )
		{
			return indicator.ninZaSupportResistanceRadar(input);
		}

	}
}

#endregion
