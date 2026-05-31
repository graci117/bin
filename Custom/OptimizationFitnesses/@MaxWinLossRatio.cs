// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxWinLossRatio : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy) =>
			Value = strategy.SystemPerformance.AllTrades.LosingTrades.TradesPerformance.Percent.AverageProfit == 0
				? 1
				: strategy.SystemPerformance.AllTrades.WinningTrades.TradesPerformance.Percent.AverageProfit /
				  System.Math.Abs(strategy.SystemPerformance.AllTrades.LosingTrades.TradesPerformance.Percent.AverageProfit);

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name = Custom.Resource.NinjaScriptOptimizationFitnessNameMaxWinLossRatio;
		}
	}
}
