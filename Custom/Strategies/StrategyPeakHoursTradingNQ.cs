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

namespace NinjaTrader.NinjaScript.Strategies
{
    public class StrategyPeakHoursTradingNQ : Strategy
    {
        private EMA EMA1;
        private EMA EMA2;
        private RSI RSI1;
        private VWAP8 VWAP81;
        private SMA SMA1;
        private SMA SMA2;
        private ATR STR1;
        private double volatilityFilter;
        private double ATR1;
        private double trailStop;
        private double breakEvenTrigger;
        private int longEntryBar = -1;
        private int shortEntryBar = -1;
        private double longEntryPrice = 0.0;
        private double shortEntryPrice = 0.0;
        private int timeExitBars = 20;
        private int maxDrawdown = 20;
        private double maxDrawdownPercent = 0.0;
        private double highestProfit = 0.0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                // Set your default parameters here
            }
            else if (State == State.DataLoaded)
            {
                EMA1 = EMA(Close, 9);
                EMA2 = EMA(Close, 21);
                RSI1 = RSI(Close, 14, 3);
                VWAP81 = VWAP8(Close);
                SMA1 = SMA(ATR(14), 20);
                SMA2 = SMA(Volume, 20);
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 0)
                return;

            if (CurrentBars[0] < 1)
                return;

            volatilityFilter = ATR(14)[0] > SMA1[0] && Volume[0] > SMA2[0] ? 1 : 0;
            ATR1 = ATR(14)[0];
            trailStop = 1.5 * ATR1;
            breakEvenTrigger = 1.5 * ATR1;

            if (Position.MarketPosition == MarketPosition.Long)
                maxDrawdownPercent = ((Close[0] - Position.AveragePrice) / Position.AveragePrice) * 100;

            if (Position.MarketPosition == MarketPosition.Short)
                maxDrawdownPercent = ((Position.AveragePrice - Close[0]) / Position.AveragePrice) * 100;

            if (maxDrawdownPercent > highestProfit)
                highestProfit = maxDrawdownPercent;

            // Set 1
            if ((ToTime(Time[0]) > ToTime(12, 30, 0))
                 && (ToTime(Time[0]) < ToTime(16, 30, 0))
                 && (Position.MarketPosition == MarketPosition.Flat)
                 && (Close[0] > EMA1[0])
                 && (EMA1[0] > EMA2[0])
                 && (RSI1.Avg[0] > 50)
                 && (Close[0] > VWAP81[0])
                 && volatilityFilter == 1)
            {
                EnterLong(Convert.ToInt32(DefaultQuantity), "");
                longEntryBar = CurrentBar;
                longEntryPrice = Close[0];
            }

            // Set 2
            if ((Close[0] < VWAP81[0])
                 && (ToTime(Time[0]) > ToTime(12, 30, 0))
                 && (ToTime(Time[0]) < ToTime(16, 30, 0))
                 && (Position.MarketPosition == MarketPosition.Flat)
                 && (Close[0] < EMA1[0])
                 && (EMA1[0] < EMA2[0])
                 && (RSI1.Avg[0] < 50)
                 && (Close[0] < VWAP81[0])
                 && volatilityFilter == 1)
            {
                EnterShort(Convert.ToInt32(DefaultQuantity), "");
                shortEntryBar = CurrentBar;
                shortEntryPrice = Close[0];
            }

            // Trailing Stop
            if (Position.MarketPosition == MarketPosition.Long && Close[0] < (longEntryPrice - trailStop))
            {
                ExitLong();
            }
            else if (Position.MarketPosition == MarketPosition.Short && Close[0] > (shortEntryPrice + trailStop))
            {
                ExitShort();
            }

            // Break-even Exit
            if (Position.MarketPosition == MarketPosition.Long && Close[0] > (longEntryPrice + breakEvenTrigger))
            {
                ExitLong();
            }
            else if (Position.MarketPosition == MarketPosition.Short && Close[0] < (shortEntryPrice - breakEvenTrigger))
            {
                ExitShort();
            }

            // Time-based Exit
            if (Position.MarketPosition == MarketPosition.Long && CurrentBar - longEntryBar >= timeExitBars)
            {
                ExitLong();
            }
            else if (Position.MarketPosition == MarketPosition.Short && CurrentBar - shortEntryBar >= timeExitBars)
            {
                ExitShort();
            }

            // Trend Reversal Exit
            if (Position.MarketPosition == MarketPosition.Long && (EMA1[0] < EMA2[0]))
            {
                ExitLong();
            }
            else if (Position.MarketPosition == MarketPosition.Short && (EMA1[0] > EMA2[0]))
            {
                ExitShort();
            }

            // Max Drawdown Exit
				if (highestProfit - maxDrawdownPercent >= maxDrawdown)
				{
				    if (Position.MarketPosition == MarketPosition.Long)
				    {
				        ExitLong();
				    }
				    else if (Position.MarketPosition == MarketPosition.Short)
				    {
				        ExitShort();
				    }
				}


        }
    }
}
