#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Strategies.AlgoTrader;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{
    /// <summary>
    /// DaMaster AT - Data Collection Edition
    /// 1. All Bots Enabled: Collecting performance metrics for every signal source.
    /// 2. Fully Dynamic: SL (MAE), TP (MFE), and BE (Min MFE of Losers - 1).
    /// 3. Wide Windows: Maximum session coverage for MNQ data gathering.
    /// </summary>
    public class DaMasterAT : AlgoBase
    {
        public override string DisplayName { get { return Name; } }

        protected override void OnStateChange()
        {
            base.OnStateChange();
            
            if (State == State.SetDefaults)
            {
                Description     = @"DaMaster AT - Data Collection. All bots active. Fully Dynamic SL/TP/BE.";
                Name            = "DaMaster AT MNQ";
                StrategyName    = "DaMaster AT MNQ";
		
				// --- SESSION TIMES (Broad coverage for maximum trade samples) ---
                Start = DateTime.Parse("09:30", System.Globalization.CultureInfo.InvariantCulture);
                End   = DateTime.Parse("16:00", System.Globalization.CultureInfo.InvariantCulture);
				Time6 = true; // Use session 6 for broad coverage if defined

                // --- POSITION SIZING ---
                Contracts = 4; 

                // --- FULLY ADAPTIVE RISK MANAGEMENT ---
                StopMode          = TradeManagementMode.Dynamic;     // Learns from MAE
                TargetMode        = TradeManagementMode.Dynamic;     // Learns from MFE
                BreakevenMode     = BreakevenManagementMode.Dynamic;  // Trigger = Min MFE of Losers - 1
                
                // Fallback / Start-up Values
                DynamicInitialSL  = 40; 
                DynamicInitialTP  = 112;
                DynamicInitialBE  = 35;
                DynamicSLPadding  = 2;
                BE_Offset         = 2;   

                // --- MASTER BOT CATEGORY CONTROLS ---
                EnableTrendBots    = true;
                EnableRangeBots    = true;
                EnableBreakoutBots = true;

                // --- ENABLE ALL INDIVIDUAL BOTS (For Data Collection) ---
                
                // Universal / Momentum Bots
                EnableKingKhanh                 = true;
                EnablePivotImpulseBot           = true;
                EnableDeviationTrendProfileBot  = true;
                EnableTMOBot                    = true;
                EnableRangeFilteredBot          = true;
                EnableMomentumExtremesBot       = true;
                EnableMomentumVmaBot            = true;
                EnableHiLoBandsBot              = true;
                EnableLinRegBandsBot            = true;
                EnableHooker                    = true;
                EnableSuperTrendBot             = true;
                EnableJohny5                    = true;
                EnableMagicTrendy               = true;
                EnableCoralBot                  = true;
                EnableBalaBot                   = true;
                EnableTrendSniperBot            = true;

                // Trend Focus Bots
                EnableMomo                      = true;
                EnableSwingStructureBot         = true;
                EnableTrendArchitectBot         = true;

                // Range Focus Bots
                EnableVolumeThrustBot           = true;
                EnableWilly                     = true;
                EnableSmartMoneyBot             = true;

                // Breakout Focus Bots
                EnableBollingerBot              = true;
                EnableKeltnerBot                = true;
                
                // --- FILTERS ---
                EnableAutoRegimeDetection       = true;
                EnableConfluenceScoring         = false; // Set to false to collect data on ALL individual bots rather than just the "best" one
                EnableBotPerformanceFilter      = false; // Set to false to ensure even underperforming bots are logged
            }
        }

        protected override void OnBarUpdate()
        {
		    // Safety check: ensure minimum data available
		    if (CurrentBar < 50 || State != State.Realtime && CurrentBar < BarsRequiredToTrade)
		        return;

            // Optional: Relax ADX veto for data collection to see how bots handle overextension
            if (regimeAdx != null && currentAdx > 50) 
            {
                a_vetoReason = "Extreme Overextension Veto (>50)";
                longSignal = false;
                shortSignal = false;
            }

            base.OnBarUpdate();
        }
    }
}