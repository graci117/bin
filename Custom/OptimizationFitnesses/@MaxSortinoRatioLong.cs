// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxSortinoRatioLong : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy) => Value = strategy.SystemPerformance.LongTrades.TradesPerformance.SortinoRatio;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name = Custom.Resource.NinjaScriptOptimizationFitnessNameMaxSortinoRatioLong;
		}
	}
}
