// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.OptimizationFitnesses
{
	public class MaxNetProfitLong : OptimizationFitness
	{
		protected override void OnCalculatePerformanceValue(StrategyBase strategy) => Value = strategy.SystemPerformance.LongTrades.TradesPerformance.GrossProfit + strategy.SystemPerformance.LongTrades.TradesPerformance.GrossLoss;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
				Name = Custom.Resource.NinjaScriptOptimizationFitnessNameMaxNetProfitLong;
		}
	}
}
