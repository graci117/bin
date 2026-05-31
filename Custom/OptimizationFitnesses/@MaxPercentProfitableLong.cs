// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxPercentProfitableLong : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy) =>
			Value = strategy.SystemPerformance.LongTrades.TradesCount == 0
				? 0 : (double)strategy.SystemPerformance.LongTrades.WinningTrades.TradesCount / strategy.SystemPerformance.LongTrades.TradesCount;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name = Custom.Resource.NinjaScriptOptimizationFitnessNameMaxPercentProfitableLong;
		}
	}
}
