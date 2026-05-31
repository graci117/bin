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
		
		private HelloWin.HelloWin_CaptainOptimusStrong[] cacheHelloWin_CaptainOptimusStrong;

		
		public HelloWin.HelloWin_CaptainOptimusStrong HelloWin_CaptainOptimusStrong()
		{
			return HelloWin_CaptainOptimusStrong(Input);
		}


		
		public HelloWin.HelloWin_CaptainOptimusStrong HelloWin_CaptainOptimusStrong(ISeries<double> input)
		{
			if (cacheHelloWin_CaptainOptimusStrong != null)
				for (int idx = 0; idx < cacheHelloWin_CaptainOptimusStrong.Length; idx++)
					if ( cacheHelloWin_CaptainOptimusStrong[idx].EqualsInput(input))
						return cacheHelloWin_CaptainOptimusStrong[idx];
			return CacheIndicator<HelloWin.HelloWin_CaptainOptimusStrong>(new HelloWin.HelloWin_CaptainOptimusStrong(), input, ref cacheHelloWin_CaptainOptimusStrong);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.HelloWin.HelloWin_CaptainOptimusStrong HelloWin_CaptainOptimusStrong()
		{
			return indicator.HelloWin_CaptainOptimusStrong(Input);
		}


		
		public Indicators.HelloWin.HelloWin_CaptainOptimusStrong HelloWin_CaptainOptimusStrong(ISeries<double> input )
		{
			return indicator.HelloWin_CaptainOptimusStrong(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.HelloWin.HelloWin_CaptainOptimusStrong HelloWin_CaptainOptimusStrong()
		{
			return indicator.HelloWin_CaptainOptimusStrong(Input);
		}


		
		public Indicators.HelloWin.HelloWin_CaptainOptimusStrong HelloWin_CaptainOptimusStrong(ISeries<double> input )
		{
			return indicator.HelloWin_CaptainOptimusStrong(input);
		}

	}
}

#endregion
