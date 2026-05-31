// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MinAvgMaeLong : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy) => Value = 1.0 - strategy.SystemPerformance.LongTrades.TradesPerformance.Percent.AverageMae;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name = Custom.Resource.NinjaScriptOptimizationFitnessNameMinAvgMaeLong;
		}
	}
}
