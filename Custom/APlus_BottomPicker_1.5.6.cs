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
		
		private A_Plus.APlusBottomPicker[] cacheAPlusBottomPicker;

		
		public A_Plus.APlusBottomPicker APlusBottomPicker(string customLabel)
		{
			return APlusBottomPicker(Input, customLabel);
		}


		
		public A_Plus.APlusBottomPicker APlusBottomPicker(ISeries<double> input, string customLabel)
		{
			if (cacheAPlusBottomPicker != null)
				for (int idx = 0; idx < cacheAPlusBottomPicker.Length; idx++)
					if (cacheAPlusBottomPicker[idx].CustomLabel == customLabel && cacheAPlusBottomPicker[idx].EqualsInput(input))
						return cacheAPlusBottomPicker[idx];
			return CacheIndicator<A_Plus.APlusBottomPicker>(new A_Plus.APlusBottomPicker(){ CustomLabel = customLabel }, input, ref cacheAPlusBottomPicker);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.APlusBottomPicker APlusBottomPicker(string customLabel)
		{
			return indicator.APlusBottomPicker(Input, customLabel);
		}


		
		public Indicators.A_Plus.APlusBottomPicker APlusBottomPicker(ISeries<double> input , string customLabel)
		{
			return indicator.APlusBottomPicker(input, customLabel);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.APlusBottomPicker APlusBottomPicker(string customLabel)
		{
			return indicator.APlusBottomPicker(Input, customLabel);
		}


		
		public Indicators.A_Plus.APlusBottomPicker APlusBottomPicker(ISeries<double> input , string customLabel)
		{
			return indicator.APlusBottomPicker(input, customLabel);
		}

	}
}

#endregion
