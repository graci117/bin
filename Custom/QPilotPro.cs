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
		
		private QuantVue.QPilotPro[] cacheQPilotPro;

		
		public QuantVue.QPilotPro QPilotPro()
		{
			return QPilotPro(Input);
		}


		
		public QuantVue.QPilotPro QPilotPro(ISeries<double> input)
		{
			if (cacheQPilotPro != null)
				for (int idx = 0; idx < cacheQPilotPro.Length; idx++)
					if ( cacheQPilotPro[idx].EqualsInput(input))
						return cacheQPilotPro[idx];
			return CacheIndicator<QuantVue.QPilotPro>(new QuantVue.QPilotPro(), input, ref cacheQPilotPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.QuantVue.QPilotPro QPilotPro()
		{
			return indicator.QPilotPro(Input);
		}


		
		public Indicators.QuantVue.QPilotPro QPilotPro(ISeries<double> input )
		{
			return indicator.QPilotPro(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.QuantVue.QPilotPro QPilotPro()
		{
			return indicator.QPilotPro(Input);
		}


		
		public Indicators.QuantVue.QPilotPro QPilotPro(ISeries<double> input )
		{
			return indicator.QPilotPro(input);
		}

	}
}

#endregion
