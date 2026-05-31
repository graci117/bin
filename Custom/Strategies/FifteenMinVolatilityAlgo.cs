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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
namespace NinjaTrader.NinjaScript.Strategies.Profitable
{
	public class FifteenMinVolatilityAlgo : Strategy
	{
		SessionIterator sessionIterator;
		private DateTime endTime;
		private EMA emaFast, emaSlow;
		private int trades, entries;
		private double entryPrice, highOfDay, lowOfDay, ssl, lsl, secondHigh, secondLow;
		protected override void OnStateChange()
		{
			if(State == State.Historical)
				sessionIterator = new SessionIterator(Bars);
			else if(State == State.SetDefaults)
			{
				Name = "FifteenMinVolatilityAlgo";
				Description = "Designed to run on 15 Minute MNQ on 8:30AM to 3:10PM CST template. Possibly more profitable in the short term but less stable long term.";
				Calculate = Calculate.OnBarClose;
				IsExitOnSessionCloseStrategy = true;
				ExitOnSessionCloseSeconds = 30;
				StartBehavior = StartBehavior.WaitUntilFlat;
				RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
				ConnectionLossHandling = ConnectionLossHandling.KeepRunning;
				IsInstantiatedOnEachOptimizationIteration = true;
				SetOrderQuantity = SetOrderQuantity.Strategy;
				EntriesPerDirection = 4;
				DefaultQuantity = 3;
				Display = true;
				MaxTrades = 1;
				StopLoss = 600;
				TD = 950;
				ProfitTarget = 1800;
				Fast = 3;
				Slow = 67;
				ATRPeriod = 40;
				ATRMultiple = 3.2;
				DistanceMultiple = 2.2;
				SizingPoints = 90;
				IQ = 1;
				Cutoff = 23;
			}
			else if(State == State.DataLoaded && Display)
			{
				emaFast = EMA(Fast);
				emaSlow = EMA(Slow);
				emaFast.PaintPriceMarkers = false;
				emaSlow.PaintPriceMarkers = false;
				emaFast.Plots[0].Brush = Brushes.LimeGreen;
				emaSlow.Plots[0].Brush = Brushes.Red;
				AddChartIndicator(emaFast);
				AddChartIndicator(emaSlow);
				AddChartIndicator(ATR(ATRPeriod));
			}
		}
		protected override void OnBarUpdate()
		{
			if(CurrentBar < 60)
				return;
			if(Bars.IsFirstBarOfSession)
			{
    			sessionIterator.GetNextSession(Time[0], true);
				endTime = sessionIterator.ActualSessionEnd;
			}
			if(Bars.IsLastBarOfSession)
				trades = 0;
			if(Position.MarketPosition == MarketPosition.Flat)
			{
				highOfDay = 0;
				lowOfDay = double.MaxValue;
				ssl = double.MaxValue;
				lsl = 0;
			}
			if((ToTime(Time[0]) == ToTime(endTime) - 1000 && ToTime(endTime) == 151000) || (ToTime(Time[0]) == ToTime(endTime) - 1500 && ToTime(endTime) != 151000))
			{
				if((Close[0] > Open[0] && Position.MarketPosition == MarketPosition.Long) || (Close[0] < Open[0] && Position.MarketPosition == MarketPosition.Short))
				{
					ExitLong();
					ExitShort();
					Account.CancelAllOrders(Bars.Instrument);
				}
			}
			if(ToTime(Time[0]) < ToTime(endTime) - 1500)
			{
				if(Bars.BarsSinceNewTradingDay < Cutoff)
				{
					double[] highs = new double[Bars.BarsSinceNewTradingDay + 1];
					double[] lows = new double[Bars.BarsSinceNewTradingDay + 1];
					for(int i = 0; i < Bars.BarsSinceNewTradingDay + 1; i++)
					{
						highs[i] = High[i];
						lows[i] = Low[i];
					}
					Array.Sort(highs);
					Array.Sort(lows);
					if(highs.Count() > 2 && lows.Count() > 2)
					{
						secondHigh = highs[Bars.BarsSinceNewTradingDay - 1];
						secondLow = lows[1];
					}
				}
				if(Bars.BarsSinceNewTradingDay == 0 && Math.Abs(Open[0] - Close[1]) > ATR(ATRPeriod)[0] * DistanceMultiple)
					trades = MaxTrades;
				if(Bars.BarsSinceNewTradingDay >= Cutoff && entries > 1)
				{
					if(Position.MarketPosition == MarketPosition.Long && Close[0] < secondHigh)
						SetProfitTarget(CalculationMode.Price, secondHigh);
					if(Position.MarketPosition == MarketPosition.Short && Close[0] > secondLow)
						SetProfitTarget(CalculationMode.Price, secondLow);
				}
				if(CrossAbove(EMA(Fast), EMA(Slow), 1) && trades < MaxTrades)
				{
					lsl = Math.Max(Close[0] - ATRMultiple * ATR(ATRPeriod)[0], Close[0] - StopLoss / Bars.Instrument.MasterInstrument.PointValue);
					SetStopLoss(CalculationMode.Price, lsl);
					SetProfitTarget(CalculationMode.Price, Close[0] + ProfitTarget / Bars.Instrument.MasterInstrument.PointValue);
					EnterLong(IQ);
					entryPrice = Close[0];
					entries = 1;
					trades++;
				}
				else if(CrossBelow(EMA(Fast), EMA(Slow), 1) && trades < MaxTrades)
				{
					ssl = Math.Min(Close[0] + ATRMultiple * ATR(ATRPeriod)[0], Close[0] + StopLoss / Bars.Instrument.MasterInstrument.PointValue);
					SetStopLoss(CalculationMode.Price, ssl);
					SetProfitTarget(CalculationMode.Price, Close[0] - ProfitTarget / Bars.Instrument.MasterInstrument.PointValue);
					EnterShort(IQ);
					entryPrice = Close[0];
					entries = 1;
					trades++;
				}
				if(Position.MarketPosition == MarketPosition.Long && Close[0] - entryPrice > .9 * ATR(ATRPeriod)[0] && Close[0] - entryPrice < 2.5 * ATR(ATRPeriod)[0] && Close[0] > Open[0] && entries < EntriesPerDirection)
				{
					int pq = (int) Math.Min(10 * IQ - Position.Quantity, Math.Floor(SizingPoints * IQ / ATR(ATRPeriod)[0]));
					lsl = (Close[0] * pq + Position.AveragePrice * Position.Quantity) / (Position.Quantity + pq) - StopLoss * IQ / ((Position.Quantity + pq) * Bars.Instrument.MasterInstrument.PointValue);
					SetStopLoss(CalculationMode.Price, lsl);
					SetProfitTarget(CalculationMode.Price, (Close[0] * pq + Position.AveragePrice * Position.Quantity) / (Position.Quantity + pq) + ProfitTarget * IQ / ((Position.Quantity + pq) * Bars.Instrument.MasterInstrument.PointValue));
					EnterLong(pq);
					entryPrice = Close[0];
					entries++;
				}
				if(Position.MarketPosition == MarketPosition.Short && entryPrice - Close[0] > .9 * ATR(ATRPeriod)[0] && entryPrice - Close[0] < 2.5 * ATR(ATRPeriod)[0] && Close[0] < Open[0] && entries < EntriesPerDirection)
				{
					int pq = (int) Math.Min(10 * IQ - Position.Quantity, Math.Floor(SizingPoints * IQ / ATR(ATRPeriod)[0]));
					ssl = (Close[0] * pq + Position.AveragePrice * Position.Quantity) / (Position.Quantity + pq) + StopLoss * IQ / ((Position.Quantity + pq) * Bars.Instrument.MasterInstrument.PointValue);
					SetStopLoss(CalculationMode.Price, ssl);
					SetProfitTarget(CalculationMode.Price, (Close[0] * pq + Position.AveragePrice * Position.Quantity) / (Position.Quantity + pq) - ProfitTarget * IQ / ((Position.Quantity + pq) * Bars.Instrument.MasterInstrument.PointValue));
					EnterShort(pq);
					entryPrice = Close[0];
					entries++;
				}
				if(Position.MarketPosition == MarketPosition.Long)
				{
					if(High[0] > highOfDay)
						highOfDay = High[0];
					if(highOfDay - TD * IQ / (Position.Quantity * Bars.Instrument.MasterInstrument.PointValue) > lsl && highOfDay - TD * IQ / (Position.Quantity * Bars.Instrument.MasterInstrument.PointValue) < Close[0])
						SetStopLoss(CalculationMode.Price, highOfDay - TD * IQ / (Position.Quantity * Bars.Instrument.MasterInstrument.PointValue));
				}
				if(Position.MarketPosition == MarketPosition.Short)
				{
					if(Low[0] < lowOfDay)
						lowOfDay = Low[0];
					if(lowOfDay + TD * IQ / (Position.Quantity * Bars.Instrument.MasterInstrument.PointValue) < ssl && lowOfDay + TD * IQ / (Position.Quantity * Bars.Instrument.MasterInstrument.PointValue) > Close[0])
						SetStopLoss(CalculationMode.Price, lowOfDay + TD * IQ / (Position.Quantity * Bars.Instrument.MasterInstrument.PointValue));
				}
			}
		}
		protected override void OnAccountItemUpdate(Account account, AccountItem accountItem, double value)
		{
			double totalPNL = Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) + Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar);
			if((totalPNL < -StopLoss * IQ - 40 || totalPNL > ProfitTarget * IQ + 40 || ToTime(DateTime.Now) > ToTime(endTime) - 60) && PositionAccount.MarketPosition != MarketPosition.Flat)
			{
				ExitLong();
				ExitShort();
				Account.CancelAllOrders(Bars.Instrument);
				trades = MaxTrades;
				entries = EntriesPerDirection;
			}
		}
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Display Indicators", GroupName = "Strategy Parameters", Order = 0)]
		public bool Display
		{get; set;}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Minimum Initial Contracts", GroupName = "Strategy Parameters", Order = 0)]
		public int IQ
		{get; set;}
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Max Daily Trades", GroupName = "Strategy Parameters", Order = 0)]
		public int MaxTrades
		{get; set;}
		[Range(.01, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stop Loss Currency Per Contract", GroupName = "Strategy Parameters", Order = 0)]
		public double StopLoss
		{get; set;}
		[Range(.01, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Trailing Drawdown Currency Per Contract", GroupName = "Strategy Parameters", Order = 0)]
		public double TD
		{get; set;}
		[Range(.01, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Profit Target Currency Per Contract", GroupName = "Strategy Parameters", Order = 0)]
		public double ProfitTarget
		{get; set;}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMA Fast Period", GroupName = "Strategy Parameters", Order = 0)]
		public int Fast
		{get; set;}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMA Slow Period", GroupName = "Strategy Parameters", Order = 0)]
		public int Slow
		{get; set;}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR Period", GroupName =  "Strategy Parameters", Order = 0)]
		public int ATRPeriod
		{get; set;}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Sizing Points Per Contract", GroupName =  "Strategy Parameters", Order = 0)]
		public int SizingPoints
		{get; set;}
		[Range(.01, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR SL Multiple", GroupName =  "Strategy Parameters", Order = 0)]
		public double ATRMultiple
		{get; set;}
		[Range(.01, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATR Gap Distance Multiple", GroupName =  "Strategy Parameters", Order = 0)]
		public double DistanceMultiple
		{get; set;}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Range Cutoff", GroupName =  "Strategy Parameters", Order = 0)]
		public int Cutoff
		{get; set;}
	}
}
