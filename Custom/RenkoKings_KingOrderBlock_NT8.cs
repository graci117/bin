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
		
		private RenkoKings.RenkoKings_KingOrderBlock[] cacheRenkoKings_KingOrderBlock;

		
		public RenkoKings.RenkoKings_KingOrderBlock RenkoKings_KingOrderBlock(int swingPointNeighborhood, int imbalanceQualifying, int orderBlockFindingBosChochPeriod,int orderBlockAge,int orderBlocksSameDirectionOffset, int orderBlocksDifferenceDirectionOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return RenkoKings_KingOrderBlock(Input, swingPointNeighborhood, imbalanceQualifying, orderBlockFindingBosChochPeriod, orderBlockAge, orderBlocksSameDirectionOffset, orderBlocksDifferenceDirectionOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}


		
		public RenkoKings.RenkoKings_KingOrderBlock RenkoKings_KingOrderBlock(ISeries<double> input, int swingPointNeighborhood, int imbalanceQualifying, int orderBlockFindingBosChochPeriod,int orderBlockAge,int orderBlocksSameDirectionOffset, int orderBlocksDifferenceDirectionOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			if (cacheRenkoKings_KingOrderBlock != null)
				for (int idx = 0; idx < cacheRenkoKings_KingOrderBlock.Length; idx++)
					if (cacheRenkoKings_KingOrderBlock[idx].SwingPointNeighborhood == swingPointNeighborhood && cacheRenkoKings_KingOrderBlock[idx].ImbalanceQualifying == imbalanceQualifying && cacheRenkoKings_KingOrderBlock[idx].OrderBlockFindingBosChochPeriod == orderBlockFindingBosChochPeriod && cacheRenkoKings_KingOrderBlock[idx].OrderBlockAge == orderBlockAge && cacheRenkoKings_KingOrderBlock[idx].OrderBlocksSameDirectionOffset == orderBlocksSameDirectionOffset && cacheRenkoKings_KingOrderBlock[idx].OrderBlocksDifferenceDirectionOffset == orderBlocksDifferenceDirectionOffset && cacheRenkoKings_KingOrderBlock[idx].SignalQuantityPerOrderBlock == signalQuantityPerOrderBlock && cacheRenkoKings_KingOrderBlock[idx].SignalSplitBars == signalSplitBars && cacheRenkoKings_KingOrderBlock[idx].EqualsInput(input))
						return cacheRenkoKings_KingOrderBlock[idx];
			return CacheIndicator<RenkoKings.RenkoKings_KingOrderBlock>(new RenkoKings.RenkoKings_KingOrderBlock(){ SwingPointNeighborhood = swingPointNeighborhood, ImbalanceQualifying = imbalanceQualifying, OrderBlockFindingBosChochPeriod = orderBlockFindingBosChochPeriod, OrderBlockAge = orderBlockAge, OrderBlocksSameDirectionOffset = orderBlocksSameDirectionOffset, OrderBlocksDifferenceDirectionOffset = orderBlocksDifferenceDirectionOffset, SignalQuantityPerOrderBlock = signalQuantityPerOrderBlock, SignalSplitBars = signalSplitBars }, input, ref cacheRenkoKings_KingOrderBlock);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.RenkoKings.RenkoKings_KingOrderBlock RenkoKings_KingOrderBlock(int swingPointNeighborhood, int imbalanceQualifying, int orderBlockFindingBosChochPeriod,int orderBlockAge,int orderBlocksSameDirectionOffset, int orderBlocksDifferenceDirectionOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.RenkoKings_KingOrderBlock(Input, swingPointNeighborhood, imbalanceQualifying, orderBlockFindingBosChochPeriod, orderBlockAge, orderBlocksSameDirectionOffset, orderBlocksDifferenceDirectionOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}


		
		public Indicators.RenkoKings.RenkoKings_KingOrderBlock RenkoKings_KingOrderBlock(ISeries<double> input , int swingPointNeighborhood, int imbalanceQualifying, int orderBlockFindingBosChochPeriod,int orderBlockAge,int orderBlocksSameDirectionOffset, int orderBlocksDifferenceDirectionOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.RenkoKings_KingOrderBlock(input, swingPointNeighborhood, imbalanceQualifying, orderBlockFindingBosChochPeriod, orderBlockAge, orderBlocksSameDirectionOffset, orderBlocksDifferenceDirectionOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.RenkoKings.RenkoKings_KingOrderBlock RenkoKings_KingOrderBlock(int swingPointNeighborhood, int imbalanceQualifying, int orderBlockFindingBosChochPeriod,int orderBlockAge,int orderBlocksSameDirectionOffset, int orderBlocksDifferenceDirectionOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.RenkoKings_KingOrderBlock(Input, swingPointNeighborhood, imbalanceQualifying, orderBlockFindingBosChochPeriod, orderBlockAge, orderBlocksSameDirectionOffset, orderBlocksDifferenceDirectionOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}


		
		public Indicators.RenkoKings.RenkoKings_KingOrderBlock RenkoKings_KingOrderBlock(ISeries<double> input , int swingPointNeighborhood, int imbalanceQualifying, int orderBlockFindingBosChochPeriod,int orderBlockAge,int orderBlocksSameDirectionOffset, int orderBlocksDifferenceDirectionOffset, int signalQuantityPerOrderBlock, int signalSplitBars)
		{
			return indicator.RenkoKings_KingOrderBlock(input, swingPointNeighborhood, imbalanceQualifying, orderBlockFindingBosChochPeriod, orderBlockAge, orderBlocksSameDirectionOffset, orderBlocksDifferenceDirectionOffset, signalQuantityPerOrderBlock, signalSplitBars);
		}

	}
}


#endregion
