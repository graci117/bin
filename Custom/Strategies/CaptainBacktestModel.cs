#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
    public class CaptainBacktestModel : Strategy
    {
        private const int LongBias = 1;
        private const int NeutralBias = 0;
        private const int ShortBias = -1;

        private Series<double> RangeHigh;
        private Series<double> RangeLow;

        private Series<int> Bias;

        private Series<bool> OppositeCandleClose;
        private Series<bool> NewHighOrLow;

        private Series<bool> IsLong;
        private Series<bool> IsShort;

        private Series<bool> InPriceRangeWindow;
        private Series<bool> InBiasWindow;
        private Series<bool> InTradeWindow;

        private int NumberOfTradesAtSessionStart = 0;

        private class PriceRange
        {
            public double High { get; set; }
            public double Low { get; set; }
        }

        private Dictionary<DateTime, PriceRange> PriceRanges = new Dictionary<DateTime, PriceRange>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "Captain Backtest Model";
                Description = @"Credit to @tradeforopp";

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
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;
                IsInstantiatedOnEachOptimizationIteration = true;

                PriceRangeWindowStart = 030000;
                PriceRangeWindowEnd = 070000;
                BiasWindowStart = 070000;
                BiasWindowEnd = 081500;
                TradeWindowStart = 070000;
                TradeWindowEnd = 130000;

                WaitForOppositeCandleClose = true;
                WaitForNewHighOrLow = true;

                UseStopOrders = false;
                UseFixedRiskReward = true;
                Risk = 25.0;
                Reward = 75.0;

                PriceRangeHigh = new Stroke(Brushes.LimeGreen, DashStyleHelper.Solid, 3);
                PriceRangeLow = new Stroke(Brushes.Red, DashStyleHelper.Solid, 3);
            }
            else if (State == State.Configure)
            {
                RangeHigh = new Series<double>(this);
                RangeLow = new Series<double>(this);

                Bias = new Series<int>(this);

                OppositeCandleClose = new Series<bool>(this);
                NewHighOrLow = new Series<bool>(this);

                IsLong = new Series<bool>(this);
                IsShort = new Series<bool>(this);

                InPriceRangeWindow = new Series<bool>(this);
                InBiasWindow = new Series<bool>(this);
                InTradeWindow = new Series<bool>(this);
            }
            else if (State == State.Historical)
            {
                SetZOrder(-1); // Display behind bars on chart.
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 3)
                return;

            SetProfitTarget(Name, CalculationMode.Ticks, Reward / TickSize);
            SetStopLoss(Name, CalculationMode.Ticks, Risk / TickSize, false);

            RangeHigh[0] = RangeHigh[1];
            RangeLow[0] = RangeLow[1];
            Bias[0] = Bias[1];
            OppositeCandleClose[0] = OppositeCandleClose[1];
            NewHighOrLow[0] = NewHighOrLow[1];
            IsLong[0] = IsLong[1];
            IsShort[0] = IsShort[1];

            InPriceRangeWindow[0] = CurrentlyInWindow(PriceRangeWindowStart, PriceRangeWindowEnd);
            if (InPriceRangeWindow[0])
                ProcessPriceRangeWindow();
            else if (InPriceRangeWindow[1])
                ResetTradeConditions(); // On exit of price range window.

            InBiasWindow[0] = CurrentlyInWindow(BiasWindowStart, BiasWindowEnd);
            if (InBiasWindow[0])
                ProcessBiasWindow();
            else if (InBiasWindow[1])
            {
                if (NeutralBias == Bias[0])
                    Draw.Text(this, CurrentBar + " No Trade", "No Trade Taken", 0, High[0]);
            }

            InTradeWindow[0] = CurrentlyInWindow(TradeWindowStart, TradeWindowEnd);
            if (!TradeTaken() && InTradeWindow[0])
                ProcessTradeWindow();
            else if (!InTradeWindow[0] && InTradeWindow[1])
            {
                ExitLong();
                ExitShort();
            }
        }

        private bool CurrentlyInWindow(int windowStart, int windowEnd)
        {
            int currentTime = ToTime(Time[0]);
            if (windowStart > windowEnd)
                return (currentTime >= windowStart) || (currentTime <= windowEnd);
            else
                return (currentTime >= windowStart) && (currentTime <= windowEnd);
        }

        private bool TradeTaken()
        {
            int totalTradesTaken = SystemPerformance.AllTrades.Count;

            if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
                NumberOfTradesAtSessionStart = totalTradesTaken;

            // Have any trades been taken since the start of the session?
            if (totalTradesTaken - NumberOfTradesAtSessionStart > 0)
                return true;

            return false;
        }

        private void ProcessPriceRangeWindow()
        {
            if (CurrentBar > 3)
            {
                DateTime currentDate = Time[0].Date;

                if (!InPriceRangeWindow[1])
                {
                    // First bar in price range window.
                    RangeHigh[0] = High[0];
                    RangeLow[0] = Low[0];

                    PriceRanges[currentDate] = new PriceRange();
                }
                else
                {
                    RangeHigh[0] = Math.Max(RangeHigh[1], High[0]);
                    RangeLow[0] = Math.Min(RangeLow[1], Low[0]);
                }

                PriceRanges[currentDate].High = RangeHigh[0];
                PriceRanges[currentDate].Low = RangeLow[0];
            }
        }

        private void ResetTradeConditions()
        {
            if (CurrentBar > 3)
            {
                Bias[0] = NeutralBias;
                IsLong[0] = false;
                IsShort[0] = false;
                OppositeCandleClose[0] = false;
                NewHighOrLow[0] = false;
            }
        }

        private void ProcessBiasWindow()
        {
            if (CurrentBar > 3)
            {
                if ((High[0] > RangeHigh[0]) && (NeutralBias == Bias[0]))
                {
                    Bias[0] = LongBias;
                    Draw.ArrowUp(this, CurrentBar + " Long Bias", true, 0, Low[0], Brushes.LimeGreen);
                }

                if ((Low[0] < RangeLow[0]) && (NeutralBias == Bias[0]))
                {
                    Bias[0] = ShortBias;
                    Draw.ArrowDown(this, CurrentBar + " Short Bias", true, 0, High[0], Brushes.Red);
                }
            }
        }

        private void ProcessTradeWindow()
        {
            if (WaitForOppositeCandleClose)
            {
                if ((LongBias == Bias[0]) && (Close[0] < Open[0]))
                    OppositeCandleClose[0] = true;

                if ((ShortBias == Bias[0]) && (Close[0] > Open[0]))
                    OppositeCandleClose[0] = true;
            }
            else
            {
                OppositeCandleClose[0] = true;
            }

            if (WaitForNewHighOrLow)
            {
                if ((LongBias == Bias[0]) && (Low[0] < Low[1]))
                    NewHighOrLow[0] = true;

                if ((ShortBias == Bias[0]) && (High[0] > High[1]))
                    NewHighOrLow[0] = true;
            }
            else
            {
                NewHighOrLow[0] = true;
            }

            if (CurrentBar > 3)
            {
                if ((LongBias == Bias[0]) && (Close[0] > High[1]) && OppositeCandleClose[0] && NewHighOrLow[0] && !IsLong[1])
                {
                    IsLong[0] = true;

                    if (UseStopOrders)
                        EnterLongStopMarket(DefaultQuantity, High[0], Name);
                    else
                        EnterLong(DefaultQuantity, Name);
                }

                if ((ShortBias == Bias[0]) && (Close[0] < Low[1]) && OppositeCandleClose[0] && NewHighOrLow[0] && !IsShort[1])
                {
                    IsShort[0] = true;

                    if (UseStopOrders)
                        EnterShortStopMarket(DefaultQuantity, Low[0], Name);
                    else
                        EnterShort(DefaultQuantity, Name);
                }
            }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            foreach (var priceRange in PriceRanges)
            {
                if (priceRange.Value.High == double.MinValue || priceRange.Value.Low == double.MinValue)
                    continue;

                TimeSpan rangeStartTime = ConvertIntToTimeSpan(PriceRangeWindowStart);
                TimeSpan rangeEndTime = ConvertIntToTimeSpan(BiasWindowEnd);

                int startX = chartControl.GetXByTime(priceRange.Key + rangeStartTime);
                int endX = chartControl.GetXByTime(priceRange.Key + rangeEndTime);

                SharpDX.Direct2D1.Brush priceRangeHighBrush = PriceRangeHigh.Brush.ToDxBrush(RenderTarget);
                int highY = chartScale.GetYByValue(priceRange.Value.High);
                RenderTarget.DrawLine(new SharpDX.Vector2(startX, highY), new SharpDX.Vector2(endX, highY),
                    priceRangeHighBrush, PriceRangeHigh.Width, PriceRangeHigh.StrokeStyle);
                priceRangeHighBrush.Dispose();

                SharpDX.Direct2D1.Brush priceRangeLowBrush = PriceRangeLow.Brush.ToDxBrush(RenderTarget);
                int lowY = chartScale.GetYByValue(priceRange.Value.Low);
                RenderTarget.DrawLine(new SharpDX.Vector2(startX, lowY), new SharpDX.Vector2(endX, lowY),
                    priceRangeLowBrush, PriceRangeLow.Width, PriceRangeLow.StrokeStyle);
                priceRangeLowBrush.Dispose();
            }
        }

        private TimeSpan ConvertIntToTimeSpan(int time)
        {
            int hours = time / 10000;
            int minutes = (time % 10000) / 100;
            int seconds = time % 100;

            return new TimeSpan(hours, minutes, seconds);
        }

        public override string DisplayName
        {
            get { return Name; }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Price Range Window Start", GroupName = "Time", Order = 1)]
        public int PriceRangeWindowStart
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Price Range Window End", GroupName = "Time", Order = 2)]
        public int PriceRangeWindowEnd
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Bias Window Start", GroupName = "Time", Order = 3)]
        public int BiasWindowStart
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Bias Window End", GroupName = "Time", Order = 4)]
        public int BiasWindowEnd
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Trade Window Start", GroupName = "Time", Order = 5)]
        public int TradeWindowStart
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, int.MaxValue)]
        [Display(Name = "Trade Window End", GroupName = "Time", Order = 5)]
        public int TradeWindowEnd
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Wait for Opposite Candle Close", GroupName = "Retracement", Order = 1)]
        public bool WaitForOppositeCandleClose
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Wait for New High / Low", GroupName = "Retracement", Order = 2)]
        public bool WaitForNewHighOrLow
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Stop Orders", GroupName = "Risk Management", Order = 1)]
        public bool UseStopOrders
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Use Fixed R:R", GroupName = "Risk Management", Order = 2)]
        public bool UseFixedRiskReward
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Risk (Points)", GroupName = "Risk Management", Order = 3)]
        public double Risk
        { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Reward (Points)", GroupName = "Risk Management", Order = 4)]
        public double Reward
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Range High", GroupName = "Price Range Window", Order = 1)]
        public Stroke PriceRangeHigh
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Range Low", GroupName = "Price Range Window", Order = 2)]
        public Stroke PriceRangeLow
        { get; set; }

        #endregion
    }
}