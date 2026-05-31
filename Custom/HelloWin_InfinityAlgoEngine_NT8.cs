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
		
		private HelloWin.HelloWin_InfinityAlgoEngine[] cacheHelloWin_InfinityAlgoEngine;

		
		public HelloWin.HelloWin_InfinityAlgoEngine HelloWin_InfinityAlgoEngine()
		{
			return HelloWin_InfinityAlgoEngine(Input);
		}


		
		public HelloWin.HelloWin_InfinityAlgoEngine HelloWin_InfinityAlgoEngine(ISeries<double> input)
		{
			if (cacheHelloWin_InfinityAlgoEngine != null)
				for (int idx = 0; idx < cacheHelloWin_InfinityAlgoEngine.Length; idx++)
					if ( cacheHelloWin_InfinityAlgoEngine[idx].EqualsInput(input))
						return cacheHelloWin_InfinityAlgoEngine[idx];
			return CacheIndicator<HelloWin.HelloWin_InfinityAlgoEngine>(new HelloWin.HelloWin_InfinityAlgoEngine(), input, ref cacheHelloWin_InfinityAlgoEngine);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.HelloWin.HelloWin_InfinityAlgoEngine HelloWin_InfinityAlgoEngine()
		{
			return indicator.HelloWin_InfinityAlgoEngine(Input);
		}


		
		public Indicators.HelloWin.HelloWin_InfinityAlgoEngine HelloWin_InfinityAlgoEngine(ISeries<double> input )
		{
			return indicator.HelloWin_InfinityAlgoEngine(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.HelloWin.HelloWin_InfinityAlgoEngine HelloWin_InfinityAlgoEngine()
		{
			return indicator.HelloWin_InfinityAlgoEngine(Input);
		}


		
		public Indicators.HelloWin.HelloWin_InfinityAlgoEngine HelloWin_InfinityAlgoEngine(ISeries<double> input )
		{
			return indicator.HelloWin_InfinityAlgoEngine(input);
		}

	}
}

#endregion
