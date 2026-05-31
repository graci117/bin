// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxUlcerRatioShort : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy) => Value = strategy.SystemPerformance.ShortTrades.TradesPerformance.MonthlyUlcer;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name = Custom.Resource.NinjaScriptOptimizationFitnessNameMaxUlcerRatioShort;
		}
	}
}
