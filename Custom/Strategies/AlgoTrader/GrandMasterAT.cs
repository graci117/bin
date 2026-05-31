#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies.AlgoTrader;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{
    /// <summary>
    /// GrandMaster AT - Adaptive MNQ Sniper
    /// 1. Only top-tier bots enabled.
    /// 2. Fully Dynamic SL, TP, and BE (Min MFE of Losers - 1).
    /// </summary>
    public class GrandMasterAT : AlgoBase
    {
        public override string DisplayName { get { return Name; } }

        protected override void OnStateChange()
        {
            base.OnStateChange();
            
            if (State == State.SetDefaults)
            {
                Description     = @"GrandMaster AT - Adaptive MNQ Sniper Edition.";
                Name            = "GrandMaster AT MNQ";
                StrategyName    = "GrandMaster AT MNQ";
                
                Contracts = 4; 

                // --- FULLY DYNAMIC MODES ---
                StopMode          = TradeManagementMode.Dynamic;    // Learns from MAE
                TargetMode        = TradeManagementMode.Dynamic;    // Learns from MFE
                BreakevenMode     = BreakevenManagementMode.Dynamic; // Trigger = Min MFE of Losers - 1
                
                DynamicInitialSL  = 40; 
                DynamicInitialTP  = 112;
                DynamicInitialBE  = 35;
                DynamicSLPadding  = 4; 
                BE_Offset         = 4;   

                // --- SESSION CONTROLS ---
                Start = DateTime.Parse("09:32", System.Globalization.CultureInfo.InvariantCulture);
                End   = DateTime.Parse("11:15", System.Globalization.CultureInfo.InvariantCulture);
                Time2 = true;
                Start2 = DateTime.Parse("14:15", System.Globalization.CultureInfo.InvariantCulture); 
                End2   = DateTime.Parse("15:30", System.Globalization.CultureInfo.InvariantCulture);
                Time3 = true;
                Start3 = DateTime.Parse("03:02", System.Globalization.CultureInfo.InvariantCulture); 
                End2   = DateTime.Parse("05:00", System.Globalization.CultureInfo.InvariantCulture);
                Time6 = false; 

                // --- TOP TIER (Enabled) ---
				EnableKingKhanh = true;
				EnableDeviationTrendProfileBot = true;
				EnableSwingStructureBot = true;
				
				// --- MID TIER (Secondary/Optional) ---
				EnableTMOBot = true;
				EnableMomentumVmaBot = true;
				
				// --- LOW TIER / DISABLED (Reduces Churn) ---
				EnableHiLoBandsBot = false;
				EnablePivotImpulseBot = false; // Until more data is collected
				EnableVolumeThrustBot = false;
            }
        }

        protected override void OnBarUpdate()
        {
		    if (CurrentBar < 50 || State != State.Realtime && CurrentBar < BarsRequiredToTrade)
		        return;

            // ADX Filter: Veto if trend is exhausted
            if (regimeAdx != null && currentAdx > 45)
            {
                a_vetoReason = "Trend Overextended (ADX > 45)";
                longSignal = false;
                shortSignal = false;
            }

            // Regime Veto: GrandMaster only trades clear trends or breakouts
            if (currentRegime == MarketRegime.Ranging)
            {
                a_vetoReason = "Range Sniper Veto";
                longSignal = false;
                shortSignal = false;
            }

            base.OnBarUpdate();
        }
    }
}