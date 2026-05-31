using System;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.NinjaScript.Strategies.NinjaBotAI
{
    /// <summary>
    /// Volatility-based momentum strategy with multiple filters
    /// Uses EMA crossovers, RSI, VWAP, and volatility filters for entry
    /// Multiple exit strategies: trailing stop, breakeven, time, reversal, drawdown
    /// </summary>
    public class NinjaBotAIVolatility_v1_0_0_0 : Strategy
    {
        #region Properties
        
        [NinjaScriptProperty]
        [Display(Name = "Position Size", Order = 1, GroupName = "1. Contracts")]
        [Range(1, int.MaxValue)]
        public int contractSize { get; set; }

        [NinjaScriptProperty]
        [Range(0, 23)]
        [Display(Name = "Start Hour", Order = 1, GroupName = "2. Time")]
        public int SessionStartHour { get; set; }

        [Display(Name = "Start Minute", Order = 2, GroupName = "2. Time")]
        [NinjaScriptProperty]
        [Range(0, 59)]
        public int SessionStartMinute { get; set; }

        [Display(Name = "Start Second", Order = 3, GroupName = "2. Time")]
        [NinjaScriptProperty]
        [Range(0, 59)]
        public int SessionStartSecond { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "End Hour", Order = 4, GroupName = "2. Time")]
        [Range(0, 23)]
        public int SessionEndHour { get; set; }

        [Range(0, 59)]
        [NinjaScriptProperty]
        [Display(Name = "End Minute", Order = 5, GroupName = "2. Time")]
        public int SessionEndMinute { get; set; }

        [Display(Name = "End Second", Order = 6, GroupName = "2. Time")]
        [NinjaScriptProperty]
        [Range(0, 59)]
        public int SessionEndSecond { get; set; }

        [Display(Name = "EMA1 Period", Order = 1, GroupName = "3. Parameters - Trend")]
        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        public int EMA1Period { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "EMA2 Period", Order = 2, GroupName = "3. Parameters - Trend")]
        [NinjaScriptProperty]
        public int EMA2Period { get; set; }

        [Display(Name = "RSI Period", Order = 3, GroupName = "3. Parameters - Trend")]
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        public int RSIPeriod { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "RSI Smooth", Order = 4, GroupName = "3. Parameters - Trend")]
        [NinjaScriptProperty]
        public int RSISmooth { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Period", Order = 5, GroupName = "3. Parameters - Trend")]
        [NinjaScriptProperty]
        public int ATRPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR SMA Period", Order = 6, GroupName = "3. Parameters - Volatility Filter")]
        public int ATRSMAPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Volume SMA Period", Order = 7, GroupName = "3. Parameters - Volatility Filter")]
        [Range(1, int.MaxValue)]
        public int SMAVolumePeriod { get; set; }

        [Display(Name = "Trail-stop Multiplier", Order = 8, GroupName = "4. Exit")]
        [NinjaScriptProperty]
        public double TrailStopMultiplier { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Breakeven Multiplier", Order = 9, GroupName = "4. Exit")]
        public double BreakEvenMultiplier { get; set; }

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Time-exit Bars", Order = 10, GroupName = "4. Exit")]
        public int TimeExitBars { get; set; }

        [Range(0, int.MaxValue)]
        [Display(Name = "Max Drawdown", Order = 11, GroupName = "4. Exit")]
        [NinjaScriptProperty]
        public int MaxDrawdown { get; set; }

        #endregion

        #region Variables
        
        private EMA fastEMA;
        private EMA slowEMA;
        private RSI rsi;
        private SMA atrSMA;
        private SMA volumeSMA;
        
        private double volatilityFilter;
        private double currentATR;
        private double trailStopDistance;
        private double breakEvenDistance;
        private int entryBarLong = -1;
        private int entryBarShort = -1;
        private double entryPriceLong;
        private double entryPriceShort;
        private double currentPnLPercent;
        private double peakPnLPercent;

        #endregion

        #region OnStateChange
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "NinjaBotAI - Volatility v1.0.0.0";
                Name = "NinjaBotAI - Volatility";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelCloseIgnoreRejects;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                IsInstantiatedOnEachOptimizationIteration = true;
                
                contractSize = 1;
                SessionStartHour = 9;
                SessionStartMinute = 30;
                SessionStartSecond = 0;
                SessionEndHour = 10;
                SessionEndMinute = 10;
                SessionEndSecond = 0;
                EMA1Period = 9;
                EMA2Period = 21;
                RSIPeriod = 14;
                RSISmooth = 3;
                ATRPeriod = 14;
                ATRSMAPeriod = 20;
                SMAVolumePeriod = 20;
                TrailStopMultiplier = 3.4;
                BreakEvenMultiplier = 0.4;
                TimeExitBars = 11;
                MaxDrawdown = 2;
            }
            else if (State == State.DataLoaded)
            {
                fastEMA = EMA(Close, EMA1Period);
                slowEMA = EMA(Close, EMA2Period);
                rsi = RSI(Close, RSIPeriod, RSISmooth);
                atrSMA = SMA(ATR(ATRPeriod), ATRSMAPeriod);
                volumeSMA = SMA(Volume, SMAVolumePeriod);
            }
        }

        #endregion

        #region OnBarUpdate
        
        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0 || CurrentBars[0] < 1)
                return;

            // Calculate volatility filter (1 = high volatility/volume, 0 = low)
            volatilityFilter = (ATR(ATRPeriod)[0] > atrSMA[0] && Volume[0] > volumeSMA[0]) ? 1.0 : 0.0;

            // Update ATR and calculate exit distances
            currentATR = ATR(ATRPeriod)[0];
            trailStopDistance = TrailStopMultiplier * currentATR;
            breakEvenDistance = BreakEvenMultiplier * currentATR;

            // Calculate current P&L percentage
            if (Position.MarketPosition == MarketPosition.Long)
            {
                currentPnLPercent = (Close[0] - Position.AveragePrice) / Position.AveragePrice * 100.0;
            }
            else if (Position.MarketPosition == MarketPosition.Short)
            {
                currentPnLPercent = (Position.AveragePrice - Close[0]) / Position.AveragePrice * 100.0;
            }

            // Track peak P&L for drawdown calculation
            if (currentPnLPercent > peakPnLPercent)
                peakPnLPercent = currentPnLPercent;

            // === ENTRY LOGIC ===
            
            // Long Entry Conditions
            if (ToTime(Time[0]) > ToTime(SessionStartHour, SessionStartMinute, SessionStartSecond) &&
                ToTime(Time[0]) < ToTime(SessionEndHour, SessionEndMinute, SessionEndSecond) &&
                Position.MarketPosition == MarketPosition.Flat &&
                Close[0] > fastEMA[0] &&           // Price above fast EMA
                fastEMA[0] > slowEMA[0] &&         // Fast EMA above slow EMA (uptrend)
                rsi.Avg[0] > 50.0 &&               // RSI above 50 (bullish momentum)
                volatilityFilter == 1.0)           // High volatility confirmed
            {
                if (PositionAccount.MarketPosition != MarketPosition.Short)
                {
                    EnterLong(Convert.ToInt32(contractSize), "Vol_Long");
                    entryBarLong = CurrentBar;
                    entryPriceLong = Close[0];
                }
            }

            // Short Entry Conditions
            if (ToTime(Time[0]) > ToTime(SessionStartHour, SessionStartMinute, SessionStartSecond) &&
                ToTime(Time[0]) < ToTime(SessionEndHour, SessionEndMinute, SessionEndSecond) &&
                Position.MarketPosition == MarketPosition.Flat &&
                Close[0] < fastEMA[0] &&           // Price below fast EMA
                fastEMA[0] < slowEMA[0] &&         // Fast EMA below slow EMA (downtrend)
                rsi.Avg[0] < 50.0 &&               // RSI below 50 (bearish momentum)
                volatilityFilter == 1.0)           // High volatility confirmed
            {
                if (PositionAccount.MarketPosition != MarketPosition.Long)
                {
                    EnterShort(Convert.ToInt32(contractSize), "Vol_Short");
                    entryBarShort = CurrentBar;
                    entryPriceShort = Close[0];
                }
            }

            // === EXIT LOGIC ===

            // Exit 1: Trailing Stop
            if (Position.MarketPosition == MarketPosition.Long && 
                Close[0] < entryPriceLong - trailStopDistance)
            {
                ExitLong(Convert.ToInt32(contractSize), "Exit_Vol_Long_Trail", "Vol_Long");
            }
            else if (Position.MarketPosition == MarketPosition.Short && 
                     Close[0] > entryPriceShort + trailStopDistance)
            {
                ExitShort(Convert.ToInt32(contractSize), "Exit_Vol_Short_Trail", "Vol_Short");
            }

            // Exit 2: Breakeven Exit (quick profit taking)
            if (Position.MarketPosition == MarketPosition.Long && 
                Close[0] > entryPriceLong + breakEvenDistance)
            {
                ExitLong(Convert.ToInt32(contractSize), "Exit_Vol_Long_Breakeven", "Vol_Long");
            }
            else if (Position.MarketPosition == MarketPosition.Short && 
                     Close[0] < entryPriceShort - breakEvenDistance)
            {
                ExitShort(Convert.ToInt32(contractSize), "Exit_Vol_Short_Breakeven", "Vol_Short");
            }

            // Exit 3: Time-based Exit
            if (Position.MarketPosition == MarketPosition.Long && 
                CurrentBar - entryBarLong >= TimeExitBars)
            {
                ExitLong(Convert.ToInt32(contractSize), "Exit_Vol_Long_Time", "Vol_Long");
            }
            else if (Position.MarketPosition == MarketPosition.Short && 
                     CurrentBar - entryBarShort >= TimeExitBars)
            {
                ExitShort(Convert.ToInt32(contractSize), "Exit_Vol_Short_Time", "Vol_Short");
            }

            // Exit 4: Trend Reversal Exit (EMAs cross against position)
            if (Position.MarketPosition == MarketPosition.Long && fastEMA[0] < slowEMA[0])
            {
                ExitLong(Convert.ToInt32(contractSize), "Exit_Vol_Long_Reversal", "Vol_Long");
            }
            else if (Position.MarketPosition == MarketPosition.Short && fastEMA[0] > slowEMA[0])
            {
                ExitShort(Convert.ToInt32(contractSize), "Exit_Vol_Short_Reversal", "Vol_Short");
            }

            // Exit 5: Maximum Drawdown Protection
            if (peakPnLPercent - currentPnLPercent >= MaxDrawdown)
            {
                if (Position.MarketPosition == MarketPosition.Long)
                {
                    ExitLong(Convert.ToInt32(contractSize), "Exit_Vol_Long_DD", "Vol_Long");
                }
                else if (Position.MarketPosition == MarketPosition.Short)
                {
                    ExitShort(Convert.ToInt32(contractSize), "Exit_Vol_Short_DD", "Vol_Short");
                }
            }
        }

        #endregion
    }
}