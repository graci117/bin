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
		
		private RenkoKings.RenkoKings_RevOutScalping[] cacheRenkoKings_RevOutScalping;

		
		public RenkoKings.RenkoKings_RevOutScalping RenkoKings_RevOutScalping(int searchLimit, RenkoKings_RevOutScalping_SignalMode signalMode, int minimumOffset)
		{
			return RenkoKings_RevOutScalping(Input, searchLimit, signalMode, minimumOffset);
		}


		
		public RenkoKings.RenkoKings_RevOutScalping RenkoKings_RevOutScalping(ISeries<double> input, int searchLimit, RenkoKings_RevOutScalping_SignalMode signalMode, int minimumOffset)
		{
			if (cacheRenkoKings_RevOutScalping != null)
				for (int idx = 0; idx < cacheRenkoKings_RevOutScalping.Length; idx++)
					if (cacheRenkoKings_RevOutScalping[idx].SearchLimit == searchLimit && cacheRenkoKings_RevOutScalping[idx].SignalMode == signalMode && cacheRenkoKings_RevOutScalping[idx].MinimumOffset == minimumOffset && cacheRenkoKings_RevOutScalping[idx].EqualsInput(input))
						return cacheRenkoKings_RevOutScalping[idx];
			return CacheIndicator<RenkoKings.RenkoKings_RevOutScalping>(new RenkoKings.RenkoKings_RevOutScalping(){ SearchLimit = searchLimit, SignalMode = signalMode, MinimumOffset = minimumOffset }, input, ref cacheRenkoKings_RevOutScalping);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.RenkoKings.RenkoKings_RevOutScalping RenkoKings_RevOutScalping(int searchLimit, RenkoKings_RevOutScalping_SignalMode signalMode, int minimumOffset)
		{
			return indicator.RenkoKings_RevOutScalping(Input, searchLimit, signalMode, minimumOffset);
		}


		
		public Indicators.RenkoKings.RenkoKings_RevOutScalping RenkoKings_RevOutScalping(ISeries<double> input , int searchLimit, RenkoKings_RevOutScalping_SignalMode signalMode, int minimumOffset)
		{
			return indicator.RenkoKings_RevOutScalping(input, searchLimit, signalMode, minimumOffset);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.RenkoKings.RenkoKings_RevOutScalping RenkoKings_RevOutScalping(int searchLimit, RenkoKings_RevOutScalping_SignalMode signalMode, int minimumOffset)
		{
			return indicator.RenkoKings_RevOutScalping(Input, searchLimit, signalMode, minimumOffset);
		}


		
		public Indicators.RenkoKings.RenkoKings_RevOutScalping RenkoKings_RevOutScalping(ISeries<double> input , int searchLimit, RenkoKings_RevOutScalping_SignalMode signalMode, int minimumOffset)
		{
			return indicator.RenkoKings_RevOutScalping(input, searchLimit, signalMode, minimumOffset);
		}

	}
}

#endregion
