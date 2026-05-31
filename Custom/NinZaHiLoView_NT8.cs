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
		
		private ninZaHiLoView[] cacheninZaHiLoView;

		
		public ninZaHiLoView ninZaHiLoView()
		{
			return ninZaHiLoView(Input);
		}


		
		public ninZaHiLoView ninZaHiLoView(ISeries<double> input)
		{
			if (cacheninZaHiLoView != null)
				for (int idx = 0; idx < cacheninZaHiLoView.Length; idx++)
					if ( cacheninZaHiLoView[idx].EqualsInput(input))
						return cacheninZaHiLoView[idx];
			return CacheIndicator<ninZaHiLoView>(new ninZaHiLoView(), input, ref cacheninZaHiLoView);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaHiLoView ninZaHiLoView()
		{
			return indicator.ninZaHiLoView(Input);
		}


		
		public Indicators.ninZaHiLoView ninZaHiLoView(ISeries<double> input )
		{
			return indicator.ninZaHiLoView(input);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaHiLoView ninZaHiLoView()
		{
			return indicator.ninZaHiLoView(Input);
		}


		
		public Indicators.ninZaHiLoView ninZaHiLoView(ISeries<double> input )
		{
			return indicator.ninZaHiLoView(input);
		}

	}
}

#endregion
