using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using NinjaTrader.Cbi;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class NinjaPassBot : Strategy
    {
        #region Properties
        
        [NinjaScriptProperty]
        [Display(Name = "Account Size", Order = 1, GroupName = "Parameters")]
        [TypeConverter(typeof(EnumConverter))]
        public AccountSize AccountSizeSelection { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Trade Direction", Order = 2, GroupName = "Parameters")]
        [TypeConverter(typeof(EnumConverter))]
        public TradeSide TradeDirection { get; set; }

        [Display(Name = "Event Date", Order = 3, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public DateTime EventDate { get; set; }

        [Display(Name = "Event Hour", Order = 4, GroupName = "Parameters")]
        [Range(0, 23)]
        [NinjaScriptProperty]
        public int EventHour { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Event Minute", Order = 5, GroupName = "Parameters")]
        [Range(0, 59)]
        public int EventMinute { get; set; }

        #endregion

        #region Variables
        
        private DateTime eventDateTime;
        private int contractQuantity;
        private int profitTargetTicks;
        private int stopLossTicks;
        private bool isProfitTargetSet;
        private bool isStopLossSet;

        #endregion

        #region Enums
        
        public enum AccountSize
        {
            Size25k,
            Size50k
        }

        public enum TradeSide
        {
            Account1_Buy,
            Account2_Sell
        }

        #endregion

        #region OnStateChange
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "NinjaPassBot 2.8 - Event-based trading strategy";
                Name = "NinjaPassBot";
                Calculate = Calculate.OnEachTick;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.UniqueEntries;
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
                
                // Default parameter values
                AccountSizeSelection = AccountSize.Size25k;
                TradeDirection = TradeSide.Account1_Buy;
                EventDate = DateTime.Today;
                EventHour = 9;
                EventMinute = 30;
            }
            else if (State == State.Configure)
            {
                // Create the event datetime from the configured parameters
                eventDateTime = new DateTime(
                    EventDate.Year,
                    EventDate.Month,
                    EventDate.Day,
                    EventHour,
                    EventMinute,
                    0
                );

                // Set trading parameters based on account size
                if (AccountSizeSelection == AccountSize.Size25k)
                {
                    contractQuantity = 3;
                    profitTargetTicks = 106;
                    stopLossTicks = 98;
                }
                else // Size50k
                {
                    contractQuantity = 6;
                    profitTargetTicks = 105;
                    stopLossTicks = 82;
                }
            }
        }

        #endregion

        #region OnBarUpdate
        
        protected override void OnBarUpdate()
        {
            // Only execute in real-time
            if (State == State.Historical)
                return;

            // Wait for minimum bars
            if (CurrentBar < BarsRequiredToTrade)
                return;

            // Check if current bar time matches event time and position is flat
            if (Time[0] == eventDateTime && Position.MarketPosition == MarketPosition.Flat)
            {
                if (TradeDirection == TradeSide.Account1_Buy)
                {
                    EnterLong(contractQuantity, "Buy");
                }
                else
                {
                    EnterShort(contractQuantity, "Sell");
                }
                
                // Set profit target and stop loss
                SetProfitTarget(CalculationMode.Ticks, profitTargetTicks);
                SetStopLoss(CalculationMode.Ticks, stopLossTicks);

                // Prevent re-entry by setting event to max value
                eventDateTime = DateTime.MaxValue;
            }
        }

        #endregion

        #region OnOrderUpdate
        
        protected override void OnOrderUpdate(
            Order order,
            double limitPrice,
            double stopPrice,
            int quantity,
            int filled,
            double averageFillPrice,
            OrderState orderState,
            DateTime time,
            ErrorCode error,
            string comment)
        {
            if (order == null)
                return;

            // Check if profit target and stop loss orders are working
            foreach (Order existingOrder in Orders)
            {
                if (existingOrder != null && 
                    (existingOrder.OrderState == OrderState.Working || 
                     existingOrder.OrderState == OrderState.Accepted))
                {
                    if (existingOrder.StopPrice > 0)
                        isStopLossSet = true;
                    
                    if (existingOrder.LimitPrice > 0)
                        isProfitTargetSet = true;
                }
            }

            // Ensure profit target and stop loss are set after order fill
            if (order.OrderState == OrderState.Filled)
            {
                if (!isProfitTargetSet)
                {
                    SetProfitTarget(CalculationMode.Ticks, profitTargetTicks);
                    isProfitTargetSet = true;
                }

                if (!isStopLossSet)
                {
                    SetStopLoss(CalculationMode.Ticks, stopLossTicks);
                    isStopLossSet = true;
                }
            }

            // Reset flags when position is flat
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                isProfitTargetSet = false;
                isStopLossSet = false;
            }
        }

        #endregion
    }
}