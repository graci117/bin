using System;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.NinjaScript.Strategies.NinjaBotAI
{
    /// <summary>
    /// Trend following strategy using SuperTrend-style indicator
    /// Enters on trend changes with ATR-based stops and targets
    /// </summary>
    public class NinjaBotAITrend_v1_0_0_0 : Strategy
    {
        #region Properties
        
        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Position Size", Order = 1, GroupName = "1. Contracts")]
        public int contractSize { get; set; }

        [NinjaScriptProperty]
        [Range(0, 23)]
        [Display(Name = "Start Hour", Order = 1, GroupName = "2. Time")]
        public int StartHour { get; set; }

        [NinjaScriptProperty]
        [Range(0, 59)]
        [Display(Name = "Start Minute", Order = 2, GroupName = "2. Time")]
        public int StartMinute { get; set; }

        [Range(0, 23)]
        [Display(Name = "End Hour", Order = 3, GroupName = "2. Time")]
        [NinjaScriptProperty]
        public int EndHour { get; set; }

        [Display(Name = "End Minute", Order = 4, GroupName = "2. Time")]
        [Range(0, 59)]
        [NinjaScriptProperty]
        public int EndMinute { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Period", Order = 1, GroupName = "3. Parameters")]
        public int Periods { get; set; }

        [Display(Name = "ATR Multiplier", Order = 2, GroupName = "3. Parameters")]
        [Range(0.1, double.MaxValue)]
        [NinjaScriptProperty]
        public double Multiplier { get; set; }

        [Display(Name = "Change ATR Calculation Method?", Order = 3, GroupName = "3. Parameters")]
        [NinjaScriptProperty]
        public bool ChangeATR { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Period (TP/SL)", Order = 1, GroupName = "4. Risk Management")]
        public int ATRPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "ATR Multiplier (TP)", Order = 2, GroupName = "4. Risk Management")]
        public double ATRMultiplierTP { get; set; }

        [Range(0.1, double.MaxValue)]
        [Display(Name = "ATR Multiplier (SL)", Order = 3, GroupName = "4. Risk Management")]
        [NinjaScriptProperty]
        public double ATRMultiplierSL { get; set; }

        #endregion

        #region Variables
        
        private Series<double> lowerBand;
        private Series<double> upperBand;
        private Series<int> trendDirection;
        private Series<double> trueRange;
        private SMA trueRangeSMA;
        private ATR atr;

        #endregion

        #region OnStateChange
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "NinjaBotAI - Trend v1.0.0.0";
                Name = "NinjaBotAI - Trend";
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
                IsInstantiatedOnEachOptimizationIteration = false;
                
                contractSize = 1;
                Periods = 10;
                Multiplier = 2.0;
                ChangeATR = true;
                ATRPeriod = 10;
                ATRMultiplierTP = 2.3;
                ATRMultiplierSL = 2.2;
                StartHour = 9;
                StartMinute = 30;
                EndHour = 14;
                EndMinute = 35;
            }
            else if (State == State.DataLoaded)
            {
                lowerBand = new Series<double>(this);
                upperBand = new Series<double>(this);
                trendDirection = new Series<int>(this);
                trueRange = new Series<double>(this);
                trueRangeSMA = SMA(trueRange, Periods);
                atr = ATR(ATRPeriod);
            }
        }

        #endregion

        #region OnBarUpdate
        
        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1)
            {
                trendDirection[0] = 1;
                trueRange[0] = High[0] - Low[0];
                return;
            }

            TimeSpan currentTime = Time[0].TimeOfDay;
            TimeSpan sessionStart = new TimeSpan(StartHour, StartMinute, 0);
            TimeSpan sessionEnd = new TimeSpan(EndHour, EndMinute, 0);

            // Calculate True Range
            double tr = Math.Max(High[0] - Low[0], 
                        Math.Max(Math.Abs(High[0] - Close[1]), 
                                Math.Abs(Low[0] - Close[1])));
            trueRange[0] = tr;

            // Get ATR value (choose between standard ATR or SMA of TR)
            double atrValue = ATR(Periods)[0];
            double smaValue = trueRangeSMA[0];
            double volatility = ChangeATR ? atrValue : smaValue;

            // Calculate basic bands (SuperTrend style)
            double hl2 = (High[0] + Low[0]) / 2.0;
            double basicLower = hl2 - Multiplier * volatility;
            double basicUpper = hl2 + Multiplier * volatility;

            // Apply band trailing logic
            double prevLower = lowerBand[1];
            double prevUpper = upperBand[1];

            if (Close[1] > prevLower)
                basicLower = Math.Max(basicLower, prevLower);
            
            if (Close[1] < prevUpper)
                basicUpper = Math.Min(basicUpper, prevUpper);

            lowerBand[0] = basicLower;
            upperBand[0] = basicUpper;

            // Determine trend direction
            int prevTrend = trendDirection[1];
            int newTrend = prevTrend;

            if (prevTrend == -1 && Close[0] > prevUpper)
                newTrend = 1;
            else if (prevTrend == 1 && Close[0] < prevLower)
                newTrend = -1;

            trendDirection[0] = newTrend;

            // Detect trend changes
            bool bullishCross = newTrend == 1 && prevTrend == -1;
            bool bearishCross = newTrend == -1 && prevTrend == 1;

            // Execute trades within session time
            if (currentTime >= sessionStart && currentTime <= sessionEnd)
            {
                if (bullishCross)
                {
                    if (Position.MarketPosition == MarketPosition.Short)
                        ExitShort("Trend_ExitSignal", "Trend_Short");
                    
                    if (PositionAccount.MarketPosition != MarketPosition.Short)
                        EnterLong(contractSize, "Trend_Long");
                }

                if (bearishCross)
                {
                    if (Position.MarketPosition == MarketPosition.Long)
                        ExitLong("Trend_ExitSignal", "Trend_Long");
                    
                    if (PositionAccount.MarketPosition != MarketPosition.Long)
                        EnterShort(contractSize, "Trend_Short");
                }
            }
            else
            {
                // Exit all positions outside trading hours
                if (Position.MarketPosition == MarketPosition.Long)
                    ExitLong("Trend_Long_ExitTime", "Trend_Long");
                if (Position.MarketPosition == MarketPosition.Short)
                    ExitShort("Trend_Long_ExitTime", "Trend_Short");
            }
        }

        #endregion

        #region OnExecutionUpdate
        
        protected override void OnExecutionUpdate(Execution execution, string executionId, 
            double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            if (execution.Order == null || execution.Order.OrderState != OrderState.Filled)
                return;

            double currentATR = atr[0];
            int profitTargetTicks = (int)Math.Round(
                ATRMultiplierTP * currentATR / Instrument.MasterInstrument.TickSize, 
                MidpointRounding.AwayFromZero);
            int stopLossTicks = (int)Math.Round(
                ATRMultiplierSL * currentATR / Instrument.MasterInstrument.TickSize, 
                MidpointRounding.AwayFromZero);

            if (marketPosition == MarketPosition.Long)
            {
                SetProfitTarget("Trend_Long", CalculationMode.Ticks, profitTargetTicks, true);
                SetStopLoss("Trend_Long", CalculationMode.Ticks, stopLossTicks, false);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                SetProfitTarget("Trend_Short", CalculationMode.Ticks, profitTargetTicks, true);
                SetStopLoss("Trend_Short", CalculationMode.Ticks, stopLossTicks, false);
            }
        }

        #endregion
    }
}