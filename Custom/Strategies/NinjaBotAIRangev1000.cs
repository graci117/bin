using System;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;

namespace NinjaTrader.NinjaScript.Strategies.NinjaBotAI
{
    /// <summary>
    /// Range-bound trading strategy using swing detection and ATR filtering
    /// Enters on mean reversion signals within defined time windows
    /// </summary>
    public class NinjaBotAIRange_v1_0_0_0 : Strategy
    {
        #region Properties
        
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Position Size", Order = 1, GroupName = "1. Contracts")]
        public int contractSize { get; set; }

        [Range(0, 23)]
        [Display(Name = "Start Hour", Order = 1, GroupName = "2. Time")]
        [NinjaScriptProperty]
        public int StartHour { get; set; }

        [Display(Name = "Start Minute", Order = 2, GroupName = "2. Time")]
        [Range(0, 59)]
        [NinjaScriptProperty]
        public int StartMinute { get; set; }

        [Range(0, 23)]
        [Display(Name = "End Hour", Order = 3, GroupName = "2. Time")]
        [NinjaScriptProperty]
        public int EndHour { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "End Minute", Order = 4, GroupName = "2. Time")]
        [Range(0, 59)]
        public int EndMinute { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Swing Period", Order = 1, GroupName = "3. Parameters")]
        public int SwingPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Swing Multiplier", Order = 2, GroupName = "3. Parameters")]
        public double SwingMultiplier { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Filter ATR", Order = 3, GroupName = "3. Parameters")]
        public double FilterATR { get; set; }

        [Range(1, int.MaxValue)]
        [Display(Name = "ATR Period (TP/SL)", Order = 1, GroupName = "4. Risk Management")]
        [NinjaScriptProperty]
        public int ATRPeriod { get; set; }

        [Display(Name = "ATR Multiplier (TP)", Order = 2, GroupName = "4. Risk Management")]
        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        public double ATRMultiplierTP { get; set; }

        [Range(0.1, double.MaxValue)]
        [Display(Name = "ATR Multiplier (SL)", Order = 3, GroupName = "4. Risk Management")]
        [NinjaScriptProperty]
        public double ATRMultiplierSL { get; set; }

        #endregion

        #region Variables
        
        private Series<double> fastEMA;
        private Series<double> slowEMA;
        private Series<double> swingLine;
        private Series<double> trendDirection;
        private Series<int> signalState;
        private ATR atr;

        #endregion

        #region OnStateChange
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "NinjaBotAI - Range v1.0.0.0";
                Name = "NinjaBotAI - Range";
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
                StartHour = 0;
                StartMinute = 0;
                EndHour = 12;
                EndMinute = 45;
                ATRPeriod = 12;
                ATRMultiplierTP = 3.0;
                ATRMultiplierSL = 3.5;
                SwingPeriod = 15;
                SwingMultiplier = 4.0;
                FilterATR = 100.0;
            }
            else if (State == State.DataLoaded)
            {
                fastEMA = new Series<double>(this);
                slowEMA = new Series<double>(this);
                swingLine = new Series<double>(this);
                trendDirection = new Series<double>(this);
                signalState = new Series<int>(this);
                atr = ATR(ATRPeriod);
            }
        }

        #endregion

        #region OnBarUpdate
        
        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(SwingPeriod, 2))
                return;

            TimeSpan currentTime = Time[0].TimeOfDay;
            TimeSpan sessionStart = new TimeSpan(StartHour, StartMinute, 0);
            TimeSpan sessionEnd = new TimeSpan(EndHour, EndMinute, 0);

            // Calculate double-smoothed EMA for swing detection
            double priceChange = Math.Abs(Input[0] - Input[1]);
            double fastAlpha = 2.0 / (SwingPeriod + 1);
            fastEMA[0] = fastAlpha * priceChange + (1.0 - fastAlpha) * fastEMA[1];

            int slowPeriod = SwingPeriod * 2 - 1;
            double slowAlpha = 2.0 / (slowPeriod + 1);
            slowEMA[0] = slowAlpha * fastEMA[0] + (1.0 - slowAlpha) * slowEMA[1];

            // Calculate swing line with bands
            double bandwidth = slowEMA[0] * SwingMultiplier;
            double previousSwing = swingLine[1];
            double currentPrice = Input[0];
            double newSwing = previousSwing;

            if (currentPrice - bandwidth > previousSwing)
                newSwing = currentPrice - bandwidth;
            else if (currentPrice + bandwidth < previousSwing)
                newSwing = currentPrice + bandwidth;

            swingLine[0] = newSwing;

            // Determine trend direction
            double newDirection = trendDirection[1];
            if (swingLine[0] > swingLine[1])
                newDirection = 1.0;
            else if (swingLine[0] < swingLine[1])
                newDirection = -1.0;

            trendDirection[0] = newDirection;

            // Generate signals
            bool isUptrend = trendDirection[0] == 1.0;
            bool isDowntrend = trendDirection[0] == -1.0;
            bool priceAboveSwing = Input[0] > swingLine[0] && isUptrend;
            bool priceBelowSwing = Input[0] < swingLine[0] && isDowntrend;

            int previousSignal = signalState[1];
            
            if (priceAboveSwing)
                signalState[0] = 1;
            else if (priceBelowSwing)
                signalState[0] = -1;
            else
                signalState[0] = signalState[1];

            bool longSignal = priceAboveSwing && previousSignal == -1;
            bool shortSignal = priceBelowSwing && previousSignal == 1;

            // Execute trades within session time
            if (currentTime >= sessionStart && currentTime <= sessionEnd)
            {
                double currentATR = atr[0];

                if (longSignal)
                {
                    if (Position.MarketPosition == MarketPosition.Short)
                        ExitShort("Range_Short_ExitSignal", "Range_Short");
                }
                else if (shortSignal && Position.MarketPosition == MarketPosition.Long)
                {
                    ExitLong("Range_Long_ExitSignal", "Range_Long");
                }

                // Enter long if ATR filter passed
                if (longSignal && currentATR < FilterATR && 
                    PositionAccount.MarketPosition != MarketPosition.Short)
                {
                    EnterLong(contractSize, "Range_Long");
                }
                // Enter short if ATR filter passed
                else if (shortSignal && currentATR < FilterATR && 
                         PositionAccount.MarketPosition != MarketPosition.Long)
                {
                    EnterShort(contractSize, "Range_Short");
                }
            }
            else
            {
                // Exit all positions outside trading hours
                if (Position.MarketPosition == MarketPosition.Long)
                    ExitLong("Range_Long_ExitTime", "Range_Long");
                if (Position.MarketPosition == MarketPosition.Short)
                    ExitShort("Range_Short_ExitTime", "Range_Short");
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
                SetProfitTarget("Range_Long", CalculationMode.Ticks, profitTargetTicks, true);
                SetStopLoss("Range_Long", CalculationMode.Ticks, stopLossTicks, false);
            }
            else if (marketPosition == MarketPosition.Short)
            {
                SetProfitTarget("Range_Short", CalculationMode.Ticks, profitTargetTicks, true);
                SetStopLoss("Range_Short", CalculationMode.Ticks, stopLossTicks, false);
            }
        }

        #endregion
    }
}