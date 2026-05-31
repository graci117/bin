// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxPercentProfitable : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy) =>
			Value = strategy.SystemPerformance.AllTrades.TradesCount == 0
				? 0 : (double)strategy.SystemPerformance.AllTrades.WinningTrades.TradesCount / strategy.SystemPerformance.AllTrades.TradesCount;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name = Custom.Resource.NinjaScriptOptimizationFitnessNameMaxPercentProfitable;
		}
	}
}
