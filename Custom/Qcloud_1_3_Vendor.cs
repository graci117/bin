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
		
		private Qcloud[] cacheQcloud;

		
		public Qcloud Qcloud(Brush bearColor, Brush bullColor, int inpPeriod1, int inpPeriod2, int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool alerts)
		{
			return Qcloud(Input, bearColor, bullColor, inpPeriod1, inpPeriod2, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, alerts);
		}


		
		public Qcloud Qcloud(ISeries<double> input, Brush bearColor, Brush bullColor, int inpPeriod1, int inpPeriod2, int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool alerts)
		{
			if (cacheQcloud != null)
				for (int idx = 0; idx < cacheQcloud.Length; idx++)
					if (cacheQcloud[idx].BearColor == bearColor && cacheQcloud[idx].BullColor == bullColor && cacheQcloud[idx].InpPeriod1 == inpPeriod1 && cacheQcloud[idx].InpPeriod2 == inpPeriod2 && cacheQcloud[idx].InpPeriod3 == inpPeriod3 && cacheQcloud[idx].InpPeriod4 == inpPeriod4 && cacheQcloud[idx].InpPeriod5 == inpPeriod5 && cacheQcloud[idx].InpPeriod6 == inpPeriod6 && cacheQcloud[idx].Alerts == alerts && cacheQcloud[idx].EqualsInput(input))
						return cacheQcloud[idx];
			return CacheIndicator<Qcloud>(new Qcloud(){ BearColor = bearColor, BullColor = bullColor, InpPeriod1 = inpPeriod1, InpPeriod2 = inpPeriod2, InpPeriod3 = inpPeriod3, InpPeriod4 = inpPeriod4, InpPeriod5 = inpPeriod5, InpPeriod6 = inpPeriod6, Alerts = alerts }, input, ref cacheQcloud);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.Qcloud Qcloud(Brush bearColor, Brush bullColor, int inpPeriod1, int inpPeriod2, int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool alerts)
		{
			return indicator.Qcloud(Input, bearColor, bullColor, inpPeriod1, inpPeriod2, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, alerts);
		}


		
		public Indicators.Qcloud Qcloud(ISeries<double> input , Brush bearColor, Brush bullColor, int inpPeriod1, int inpPeriod2, int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool alerts)
		{
			return indicator.Qcloud(input, bearColor, bullColor, inpPeriod1, inpPeriod2, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, alerts);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.Qcloud Qcloud(Brush bearColor, Brush bullColor, int inpPeriod1, int inpPeriod2, int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool alerts)
		{
			return indicator.Qcloud(Input, bearColor, bullColor, inpPeriod1, inpPeriod2, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, alerts);
		}


		
		public Indicators.Qcloud Qcloud(ISeries<double> input , Brush bearColor, Brush bullColor, int inpPeriod1, int inpPeriod2, int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool alerts)
		{
			return indicator.Qcloud(input, bearColor, bullColor, inpPeriod1, inpPeriod2, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, alerts);
		}

	}
}

#endregion
