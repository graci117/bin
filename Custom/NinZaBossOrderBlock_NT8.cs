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
		
		private ninZaBossOrderBlock[] cacheninZaBossOrderBlock;

		
		public ninZaBossOrderBlock ninZaBossOrderBlock(int swingPointNeighborhood, int imbalanceMinHigh, int orderBlockMinHigh, int orderBlockFindingBosChochPeriod, int orderBlocksOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return ninZaBossOrderBlock(Input, swingPointNeighborhood, imbalanceMinHigh, orderBlockMinHigh, orderBlockFindingBosChochPeriod, orderBlocksOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}


		
		public ninZaBossOrderBlock ninZaBossOrderBlock(ISeries<double> input, int swingPointNeighborhood, int imbalanceMinHigh, int orderBlockMinHigh, int orderBlockFindingBosChochPeriod, int orderBlocksOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			if (cacheninZaBossOrderBlock != null)
				for (int idx = 0; idx < cacheninZaBossOrderBlock.Length; idx++)
					if (cacheninZaBossOrderBlock[idx].SwingPointNeighborhood == swingPointNeighborhood && cacheninZaBossOrderBlock[idx].ImbalanceMinHigh == imbalanceMinHigh && cacheninZaBossOrderBlock[idx].OrderBlockMinHigh == orderBlockMinHigh && cacheninZaBossOrderBlock[idx].OrderBlockFindingBosChochPeriod == orderBlockFindingBosChochPeriod && cacheninZaBossOrderBlock[idx].OrderBlocksOffset == orderBlocksOffset && cacheninZaBossOrderBlock[idx].SignalQuantityPerOrderBlock == signalQuantityPerOrderBlock && cacheninZaBossOrderBlock[idx].SignalSplitBars == signalSplitBars && cacheninZaBossOrderBlock[idx].EqualsInput(input))
						return cacheninZaBossOrderBlock[idx];
			return CacheIndicator<ninZaBossOrderBlock>(new ninZaBossOrderBlock(){ SwingPointNeighborhood = swingPointNeighborhood, ImbalanceMinHigh = imbalanceMinHigh, OrderBlockMinHigh = orderBlockMinHigh, OrderBlockFindingBosChochPeriod = orderBlockFindingBosChochPeriod, OrderBlocksOffset = orderBlocksOffset, SignalQuantityPerOrderBlock = signalQuantityPerOrderBlock, SignalSplitBars = signalSplitBars }, input, ref cacheninZaBossOrderBlock);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaBossOrderBlock ninZaBossOrderBlock(int swingPointNeighborhood, int imbalanceMinHigh, int orderBlockMinHigh, int orderBlockFindingBosChochPeriod, int orderBlocksOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.ninZaBossOrderBlock(Input, swingPointNeighborhood, imbalanceMinHigh, orderBlockMinHigh, orderBlockFindingBosChochPeriod, orderBlocksOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}


		
		public Indicators.ninZaBossOrderBlock ninZaBossOrderBlock(ISeries<double> input , int swingPointNeighborhood, int imbalanceMinHigh, int orderBlockMinHigh, int orderBlockFindingBosChochPeriod, int orderBlocksOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.ninZaBossOrderBlock(input, swingPointNeighborhood, imbalanceMinHigh, orderBlockMinHigh, orderBlockFindingBosChochPeriod, orderBlocksOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaBossOrderBlock ninZaBossOrderBlock(int swingPointNeighborhood, int imbalanceMinHigh, int orderBlockMinHigh, int orderBlockFindingBosChochPeriod, int orderBlocksOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.ninZaBossOrderBlock(Input, swingPointNeighborhood, imbalanceMinHigh, orderBlockMinHigh, orderBlockFindingBosChochPeriod, orderBlocksOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}


		
		public Indicators.ninZaBossOrderBlock ninZaBossOrderBlock(ISeries<double> input , int swingPointNeighborhood, int imbalanceMinHigh, int orderBlockMinHigh, int orderBlockFindingBosChochPeriod, int orderBlocksOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.ninZaBossOrderBlock(input, swingPointNeighborhood, imbalanceMinHigh, orderBlockMinHigh, orderBlockFindingBosChochPeriod, orderBlocksOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}

	}
}

#endregion
