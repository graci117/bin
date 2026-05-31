#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript;
using System.Windows.Media;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{
    public abstract partial class AlgoBase : Strategy
    {
        #region Order Label Constants
        protected const string LE = "LE";
        protected const string SE = "SE";
        protected const string QLE = "QLE";
        protected const string QSE = "QSE";
		protected const string Add1 = "Add1";
		protected const string CP1C = "CP1C";
		protected const string ClAll = "ClAll";
        #endregion
		
		private bool isHybridPsarActive = false;

        #region Entry Submission & Execution
		
        private void EnterLongPosition()
		{
		    counterLong++; counterShort = 0;
		    currentTradeSignalSource = a_lastSignalSource; 
            currentTradeStopType = StopType.ToString();
            currentTradeProfitType = ProfitTargetMode.ToString(); 
            
            // Capture trigger for next trade (Static or Learned Dynamic)
            currentTradeBETriggerFixedTicks = (BreakevenMode == BreakevenManagementMode.Static) ? BE_Trigger : dynamicBreakevenTicks;
		    
            int contractsToTrade = CalculatePositionSize();
		    if (contractsToTrade == 0) return;

            scaleOut1Triggered = false;
            scaleOut2Triggered = false;
		
		    currentTradeEntryAdx = currentAdx;
		    currentTradeEntryMomentum = currentMomentum;
			currentTradeRegime = currentRegime;

			if (botVolumeUpDown != null && botVolumeUpDown.IsValidDataPoint(0))
			{
				currentTradeEntryUpVolume = botVolumeUpDown.UpVolume[0];
				currentTradeEntryDownVolume = botVolumeUpDown.DownVolume[0];
			}

			InitializeOperation();
			highLowTrailCurrentLookback = HighLowTrailInitialLookback;
			
			SubmitLongOrder(contractsToTrade, LE, (isBuySellMarketOrder ? "Market" : "Limit"));
			activeLE = true;
			activeOrder = true;
			orderBarNumber = CurrentBar;
		    
		    lastEntryTime = Time[0];
			isInLong = true;
			isInShort = false;
		}
		
		private void EnterShortPosition()
		{
		    counterLong = 0; counterShort++;
		    currentTradeSignalSource = a_lastSignalSource;
            currentTradeStopType = StopType.ToString();
            currentTradeProfitType = ProfitTargetMode.ToString();
            
            currentTradeBETriggerFixedTicks = (BreakevenMode == BreakevenManagementMode.Static) ? BE_Trigger : dynamicBreakevenTicks;
		    
            int contractsToTrade = CalculatePositionSize();
		    if (contractsToTrade == 0) return;
		
            scaleOut1Triggered = false;
            scaleOut2Triggered = false;

		    currentTradeEntryAdx = currentAdx;
		    currentTradeEntryMomentum = currentMomentum;
			currentTradeRegime = currentRegime;

			if (botVolumeUpDown != null && botVolumeUpDown.IsValidDataPoint(0))
			{
				currentTradeEntryUpVolume = botVolumeUpDown.UpVolume[0];
				currentTradeEntryDownVolume = botVolumeUpDown.DownVolume[0];
			}

			InitializeOperation();
			highLowTrailCurrentLookback = HighLowTrailInitialLookback;
			
			SubmitShortOrder(contractsToTrade, SE, (isBuySellMarketOrder ? "Market" : "Limit"));
			activeSE = true;
			activeOrder = true;
			orderBarNumber = CurrentBar;
		    
		    lastEntryTime = Time[0];
			isInLong = false;
			isInShort = true;
		}
        #endregion

		#region Unmanaged OnOrderUpdate
        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
            if (BarsInProgress != 0) return;

		    if ((order.Name.Contains(LE) || order.Name.Contains(SE) || order.Name.Contains(QLE) || order.Name.Contains(QSE)) && orderState != OrderState.Filled)
		    {
		      entryOrder = order;
			}
		
		    if (entryOrder != null && entryOrder == order)
		    {
		        if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
		        {
				    entryOrder = null;
				    if (isInLong) counterLong -= 1;
				    if (isInShort) counterShort -= 1;
				    if (counterLong < 0) counterLong = 0;
				    if (counterShort < 0) counterShort = 0;
					ResetOperation();
			    }  
		        if (order.OrderState == OrderState.Filled)
			    {	  
		            entryOrder = null;
				    if (isInLong) counterLong += 1;
				    if (isInShort) counterShort += 1;				  
			    }		  
		    }
	
			if (stopPrice != 0 && currStopPrice != stopPrice) currStopPrice = stopPrice;
			if (limitPrice != 0 && limitPrice != currTargetPrice) currTargetPrice = limitPrice;
			
			string stopSignal = "";
			string targetSignal = "";	
			
			switch (order.Name)
			{
				case LE:		
				case QLE:
                	entryOrder = order;
					stopSignal = "StLE";
					targetSignal = "TgLE";
					
					if (entryOrder != null && entryOrder == order)
					{		
                		if (order.OrderState == OrderState.Cancelled && order.Filled == 0) { entryOrder = null; }
						if (order.OrderState == OrderState.Filled)
                		{
                    		entryOrder = null;							
					        highSinceEntry = averageFillPrice;
					        lowSinceEntry = averageFillPrice;
					        entryBarNumberOfTrade = CurrentBar;
							
							if (stopOrder == null && targetOrder == null)
               				{
                  				if (State == State.Historical) oco = DateTime.Now.ToString() + CurrentBar + "LExits";
                   				else oco = GetAtmStrategyUniqueId() +  "LExits";
								
								Target_Price = GetProfitTargetPrice(OrderAction.Buy, averageFillPrice);
								BE_Price = Stop_Price = GetInitialStopPrice(OrderAction.Buy, averageFillPrice);

								if (chkLongPositionStop(Stop_Price))
								{
									currentTradeInitialTPTicks = (Target_Price > 0) ? (Target_Price - averageFillPrice) / TickSize : 0;
									currentTradeInitialSLTicks = (Stop_Price > 0) ? (averageFillPrice - Stop_Price) / TickSize : 0;

									Stop_Trigger	= averageFillPrice + (Trail_Trigger * TickSize);
									
									double beTriggerTicks = (BreakevenMode == BreakevenManagementMode.Dynamic) ? dynamicBreakevenTicks : BE_Trigger;
									bePriceTrigger	= averageFillPrice + (beTriggerTicks * TickSize);
									
									stopOrder = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, quantity, 0, Stop_Price, oco, stopSignal);
									if (chkLongPositionProfit(Target_Price)) 
										targetOrder = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, quantity, Target_Price, 0, oco, targetSignal);
								}
								else { FlattenAllPositions(); orderErrorOccurred = true; }
							}
                		}
					}	
    				break;
								
				case SE:		
            	case QSE:
            	    entryOrder = order;
					stopSignal = "StSE";
					targetSignal = "TgSE";
					
					if (entryOrder != null && entryOrder == order)
					{					
                		if (order.OrderState == OrderState.Cancelled && order.Filled == 0) { entryOrder = null; }
                		if (order.OrderState == OrderState.Filled)
                		{
                    		entryOrder = null;
					        highSinceEntry = averageFillPrice;
					        lowSinceEntry = averageFillPrice;
					        entryBarNumberOfTrade = CurrentBar;
							
						   	if (stopOrder == null && targetOrder == null)
               				{
                   				if (State == State.Historical) oco = DateTime.Now.ToString() + CurrentBar +  "SExits";
                   				else oco = GetAtmStrategyUniqueId() +  "SExits";
							
								Target_Price = GetProfitTargetPrice(OrderAction.Sell, averageFillPrice);
								BE_Price = Stop_Price = GetInitialStopPrice(OrderAction.Sell, averageFillPrice);

								if (chkShortPositionStop(Stop_Price))
								{
									currentTradeInitialTPTicks = (Target_Price > 0) ? (averageFillPrice - Target_Price) / TickSize : 0;
									currentTradeInitialSLTicks = (Stop_Price > 0) ? (Stop_Price - averageFillPrice) / TickSize : 0;
									
									Stop_Trigger	= averageFillPrice - (Trail_Trigger * TickSize);
									
									double beTriggerTicks = (BreakevenMode == BreakevenManagementMode.Dynamic) ? dynamicBreakevenTicks : BE_Trigger;
									bePriceTrigger	= averageFillPrice - (beTriggerTicks * TickSize);
								
									stopOrder = SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.StopMarket, quantity, 0, Stop_Price, oco, stopSignal);
									if (chkShortPositionProfit(Target_Price)) 
										targetOrder = SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, quantity, Target_Price, 0, oco, targetSignal);
								}
								else { FlattenAllPositions(); orderErrorOccurred = true; }
               				}	
                		}	
					}	
					break;
								
				case "StLE" : case "StSE" :
					stopOrder = order;
					if (order.OrderState == OrderState.Filled) { stopOrder = targetOrder = null; }	
					break;				
					
				case "TgLE" : case "TgSE" :
					targetOrder = order;				
					if (order.OrderState == OrderState.Filled) { stopOrder = targetOrder = null; }				
					break;			

				case CP1C : case ClAll :
            	    entryOrder = order;
					if (entryOrder != null && entryOrder == order)
					{					
                		if (order.OrderState == OrderState.Cancelled || order.OrderState == OrderState.Filled)
                		{
                    		entryOrder = null;
							if (order.Name == ClAll) { if(stopOrder != null) CancelOrder(stopOrder); if(targetOrder != null) CancelOrder(targetOrder); stopOrder = targetOrder = null; }
                		}
					}	
					break;					
            }
        }	
		#endregion

		#region Unmanaged OnExecutionUpdate
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			if (BarsInProgress != 0) return;
	
            if (execution.Name == Add1 || execution.Name == CP1C || execution.Name.StartsWith("Scale Out"))
			{	
				if (stopOrder != null && targetOrder != null && Position.Quantity > 0)
				{
					ChangeOrder(stopOrder, Position.Quantity, 0, stopOrder.StopPrice);
					ChangeOrder(targetOrder, Position.Quantity, targetOrder.LimitPrice, 0);
				}
			}
		}
		#endregion		
		
		#region OnPositionUpdate
		protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
		{
		    if (BarsInProgress != 0) return;
		
		    if (position.MarketPosition == MarketPosition.Flat)
		    {
		        Stop_Price = 0; Stop_Trigger = 0; Target_Price = 0;
		        BE_Price = 0; bePriceTrigger = 0;
		        startTrail = false;
		        isHybridPsarActive = false;
		        scaleOut1Triggered = false;
		        scaleOut2Triggered = false;
		        isFinalTrailActive = false;
		        
		        checkPositions();

		        if (SystemPerformance.AllTrades.Count > 0)
		        {
		            var lastTrade = SystemPerformance.AllTrades.Last();
		            if (lastTrade.ProfitCurrency > 0) sessionWins++; else sessionLosses++;
		
		            double tradeEntryPrice = lastTrade.Entry.Price;
		            double trueMfeTicks = 0;
		            double trueMaeTicks = 0;
		
		            if (highSinceEntry > 0 && lowSinceEntry > 0)
		            {
		                if (lastTrade.Entry.MarketPosition == MarketPosition.Long)
		                {
		                    trueMfeTicks = (highSinceEntry - tradeEntryPrice) / TickSize;
		                    trueMaeTicks = (tradeEntryPrice - lowSinceEntry) / TickSize;
		                }
		                else
		                {
		                    trueMfeTicks = (tradeEntryPrice - lowSinceEntry) / TickSize;
		                    trueMaeTicks = (highSinceEntry - tradeEntryPrice) / TickSize;
		                }
		            }
		
		            trueMfeTicks = Math.Max(0, trueMfeTicks);
		            trueMaeTicks = Math.Max(0, trueMaeTicks);
		
		            if (currentTradeRegime == MarketRegime.Ranging)
		            {
		                RangeMaxMAE = Math.Max(RangeMaxMAE, trueMaeTicks);
		                RangeMaxMFE = Math.Max(RangeMaxMFE, trueMfeTicks);
		            }
		            else if (currentTradeRegime == MarketRegime.Trending)
		            {
		                TrendMaxMAE = Math.Max(TrendMaxMAE, trueMaeTicks);
		                TrendMaxMFE = Math.Max(TrendMaxMFE, trueMfeTicks);
		            }
		
		            if (!string.IsNullOrEmpty(currentTradeSignalSource) && botPerformanceTrackers.ContainsKey(currentTradeSignalSource))
		                botPerformanceTrackers[currentTradeSignalSource].AddTrade(lastTrade.ProfitCurrency > 0);
		
		            if (recentMaeTicksByRegime.ContainsKey(currentTradeRegime))
		            {
		                recentMaeTicksByRegime[currentTradeRegime].Add(trueMaeTicks);
		                if (trueMfeTicks > 10) recentMfeTicksByRegime[currentTradeRegime].Add(trueMfeTicks);
		                if (recentMaeTicksByRegime[currentTradeRegime].Count > DynamicAvgLookback) recentMaeTicksByRegime[currentTradeRegime].RemoveAt(0);
		                if (recentMfeTicksByRegime[currentTradeRegime].Count > DynamicAvgLookback) recentMfeTicksByRegime[currentTradeRegime].RemoveAt(0);
		            }

                    // --- NEW LOGIC: CAPTURE MFE OF LOSING TRADES FOR BE TRIGGER ---
                    if (lastTrade.ProfitCurrency < 0)
                    {
                        if (recentMfeOfLosersByRegime.ContainsKey(currentTradeRegime))
                        {
                            recentMfeOfLosersByRegime[currentTradeRegime].Add(trueMfeTicks);
                            if (recentMfeOfLosersByRegime[currentTradeRegime].Count > DynamicAvgLookback)
                                recentMfeOfLosersByRegime[currentTradeRegime].RemoveAt(0);
                        }
                    }

                    // --- NEW LOGIC: DYNAMIC BE CALCULATION (MIN LOSER MFE - 1) ---
                    if (BreakevenMode == BreakevenManagementMode.Dynamic)
                    {
                        var loserMfeList = recentMfeOfLosersByRegime[currentTradeRegime];
                        if (loserMfeList.Count > 0)
                        {
                            // Trigger is the minimum excursion seen on a loser minus 1 tick
                            dynamicBreakevenTicks = loserMfeList.Min() - 1;
                            
                            // Safety Clamps: Ensure at least 2 ticks, no more than 50% of the target
                            dynamicBreakevenTicks = Math.Max(2, Math.Min(dynamicBreakevenTicks, ProfitTarget * 0.5));
                        }
                        else
                        {
                            dynamicBreakevenTicks = DynamicInitialBE; // Fallback
                        }
                    }

                    // --- CALMAR CALCULATION AT EXIT ---
                    double currentCalmar = CalculateCalmar();

		            if (EnableTradeLogging && tradeLogger != null)
		            {
		                tradeLogger.LogTrade(Name, lastTrade, trueMfeTicks, trueMaeTicks,
		                    currentTradeRegime.ToString(), currentTradeSignalSource, FilterMode.ToString(), currentTradeStopType, currentTradeProfitType,
		                    lastEntryConfluenceScore, currentTradeEntryAdx, currentTradeEntryMomentum, currentAdx, currentAtr, currentMomentum,
		                    barsInCurrentTrade, currentTradeInitialSLTicks, currentTradeInitialTPTicks, currentTradeBETriggerFixedTicks, dynamicBreakevenTicks, currentTradeSlippageTicks,
		                    currentTradeEntryUpVolume, currentTradeEntryDownVolume, currentCalmar, // ADDED CALMAR
		                    out string errorMsg);
		            }

                    if (EnableJsonLogging && tradeJsonLogger != null)
                    {
                        tradeJsonLogger.LogTrade(Name, lastTrade, trueMfeTicks, trueMaeTicks,
                            currentTradeRegime.ToString(), currentTradeSignalSource, FilterMode.ToString(), currentTradeStopType, currentTradeProfitType,
                            lastEntryConfluenceScore, currentTradeEntryAdx, currentTradeEntryMomentum, currentAdx, currentAtr, currentMomentum,
                            barsInCurrentTrade, currentTradeInitialSLTicks, currentTradeInitialTPTicks, currentTradeBETriggerFixedTicks, dynamicBreakevenTicks, currentTradeSlippageTicks,
                            currentTradeEntryUpVolume, currentTradeEntryDownVolume, currentCalmar, // ADDED CALMAR
                            out string jsonErrorMsg);
                    }
		        }
		
		        // Metadata Reset
		        lastEntryConfluenceScore = 0; currentTradeEntryAdx = 0; currentTradeEntryMomentum = 0;
		        currentTradeSignalSource = "---"; currentTradeStopType = "---"; currentTradeProfitType = "---";
		        currentTradeRegime = MarketRegime.Undefined; currentTradeBETriggerFixedTicks = 0; 
                currentTradeSlippageTicks = 0; a_lastSignalSource = "---";
		        currentTradeEntryUpVolume = 0; currentTradeEntryDownVolume = 0;
		        BE_Realized = false; highSinceEntry = 0; lowSinceEntry = 0;
		        
		        ResetOperation();
		    }
		    else
		    {
		        iCurrentPosition = quantity;
		        if (highSinceEntry == 0) highSinceEntry = Close[0];
		        if (lowSinceEntry == 0) lowSinceEntry = Close[0];
		    }
		}
		#endregion
		
		protected override void OnMarketData(MarketDataEventArgs e)
		{
		    if (e.MarketDataType == MarketDataType.Last)
		    {
		        if (Position.MarketPosition != MarketPosition.Flat && Position.AveragePrice > 0)
		        {
		            if (highSinceEntry == 0) highSinceEntry = e.Price;
		            if (lowSinceEntry == 0) lowSinceEntry = e.Price;
		
		            highSinceEntry = Math.Max(highSinceEntry, e.Price);
		            lowSinceEntry = Math.Min(lowSinceEntry, e.Price);
		
		            double currentMFE = (Position.MarketPosition == MarketPosition.Long) ? (highSinceEntry - Position.AveragePrice) / TickSize : (Position.AveragePrice - lowSinceEntry) / TickSize;
		            double currentMAE = (Position.MarketPosition == MarketPosition.Long) ? (Position.AveragePrice - lowSinceEntry) / TickSize : (highSinceEntry - Position.AveragePrice) / TickSize;
		
		            if (currentRegime == MarketRegime.Ranging)
		            {
		                RangeMaxMAE = Math.Max(RangeMaxMAE, Math.Max(0, currentMAE));
		                RangeMaxMFE = Math.Max(RangeMaxMFE, Math.Max(0, currentMFE));
		            }
		            else if (currentRegime == MarketRegime.Trending)
		            {
		                TrendMaxMAE = Math.Max(TrendMaxMAE, Math.Max(0, currentMAE));
		                TrendMaxMFE = Math.Max(TrendMaxMFE, Math.Max(0, currentMFE));
		            }
		        }
		
		        ManageInTradeStops_OnMarketData(e.Price);
		        ManageScaleOuts(e.Price);
		    }
		}

        private void ManageScaleOuts(double currentPrice)
        {
            if (!EnableScaleOutExecution || isFlat || targetOrder == null || Position.AveragePrice <= 0 || targetOrder.LimitPrice <= 0)
                return;

            double progressPercent = (Position.MarketPosition == MarketPosition.Long) 
                ? (currentPrice - Position.AveragePrice) / (targetOrder.LimitPrice - Position.AveragePrice) * 100 
                : (Position.AveragePrice - currentPrice) / (Position.AveragePrice - targetOrder.LimitPrice) * 100;

            if (progressPercent >= ScaleOutLevel1Percent && !scaleOut1Triggered && Position.Quantity >= ScaleOutQty1)
            {
                scaleOut1Triggered = true;
                if (Position.MarketPosition == MarketPosition.Long) SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, ScaleOutQty1, 0, 0, "", "Scale Out 1");
                else SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, ScaleOutQty1, 0, 0, "", "Scale Out 1");
            }

            if (progressPercent >= ScaleOutLevel2Percent && !scaleOut2Triggered && Position.Quantity >= ScaleOutQty2)
            {
                scaleOut2Triggered = true;
                if (Position.MarketPosition == MarketPosition.Long) SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, ScaleOutQty2, 0, 0, "", "Scale Out 2");
                else SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, ScaleOutQty2, 0, 0, "", "Scale Out 2");
            }
        }

		#region Stop & Target Calculation

		private double GetInitialStopInTicks()
		{
			if (StopMode == TradeManagementMode.Dynamic) return InitialStop;
			switch (InitialStopMode)
			{
				case InitialStopCalculationMode.FixedTicks: return InitialStop;
				case InitialStopCalculationMode.ATR:
					if (ATR1 != null && ATR1.IsValidDataPoint(0)) return Math.Max(1, (ATR1[0] * StopLoss_ATR_Mult) / TickSize);
					return InitialStop;
				default: return InitialStop;
			}
		}

		private double GetInitialStopPrice(OrderAction direction, double entryPrice)
		{
			if (InitialStopMode == InitialStopCalculationMode.RangeFiltered)
			{
				if (botRangeFiltered != null && botRangeFiltered.IsValidDataPoint(0))
				{
					var bands = new Series<double>[]
					{
						botRangeFiltered.Values[18], botRangeFiltered.Values[16], botRangeFiltered.Values[14], 
						botRangeFiltered.Values[12], botRangeFiltered.Values[10], botRangeFiltered.Values[8],
						botRangeFiltered.Values[0], 
						botRangeFiltered.Values[9], botRangeFiltered.Values[11], botRangeFiltered.Values[13], 
						botRangeFiltered.Values[15], botRangeFiltered.Values[17], botRangeFiltered.Values[19]
					};

					int closestBandIndex = -1; double minDistance = double.MaxValue;
					for (int i = 0; i < bands.Length; i++)
					{
						double distance = Math.Abs(entryPrice - bands[i][0]);
						if (distance < minDistance) { minDistance = distance; closestBandIndex = i; }
					}
					if (closestBandIndex != -1)
					{
						int stopBandIndex = (direction == OrderAction.Buy) ? closestBandIndex - RangeFilteredStopLossBand : closestBandIndex + RangeFilteredStopLossBand;
						stopBandIndex = Math.Max(0, Math.Min(bands.Length - 1, stopBandIndex));
						double calculatedStopPrice = bands[stopBandIndex][0];
						if ((direction == OrderAction.Buy && calculatedStopPrice >= entryPrice) || (direction == OrderAction.Sell && calculatedStopPrice <= entryPrice))
							return direction == OrderAction.Buy ? entryPrice - (InitialStop * TickSize) : entryPrice + (InitialStop * TickSize);
						return Instrument.MasterInstrument.RoundToTickSize(calculatedStopPrice);
					}
				}
			}
			
		    if (InitialStopMode == InitialStopCalculationMode.DeviationTrendProfile)
		    {
		        if (botDeviationTrendProfile != null && botDeviationTrendProfile.IsValidDataPoint(0))
		        {
		            var bands = new Series<double>[]
		            {
		                botDeviationTrendProfile.Stdv_4, botDeviationTrendProfile.Stdv_3, botDeviationTrendProfile.Stdv_2, botDeviationTrendProfile.Stdv_1,
		                botDeviationTrendProfile.Avg,
		                botDeviationTrendProfile.Stdv1, botDeviationTrendProfile.Stdv2, botDeviationTrendProfile.Stdv3, botDeviationTrendProfile.Stdv4
		            };
		            int closestBandIndex = -1; double minDistance = double.MaxValue;
		            for (int i = 0; i < bands.Length; i++)
		            {
		                double distance = Math.Abs(entryPrice - bands[i][0]);
		                if (distance < minDistance) { minDistance = distance; closestBandIndex = i; }
		            }
		            if (closestBandIndex != -1)
		            {
		                int stopBandIndex = (direction == OrderAction.Buy) ? closestBandIndex - DTPStopLossBand : closestBandIndex + DTPStopLossBand;
		                stopBandIndex = Math.Max(0, Math.Min(bands.Length - 1, stopBandIndex));
		                double calculatedStopPrice = bands[stopBandIndex][0];
		                if ((direction == OrderAction.Buy && calculatedStopPrice >= entryPrice) || (direction == OrderAction.Sell && calculatedStopPrice <= entryPrice))
		                    return entryPrice - (InitialStop * (direction == OrderAction.Buy ? 1 : -1) * TickSize);
		                return Instrument.MasterInstrument.RoundToTickSize(calculatedStopPrice);
		            }
		        }
		    }
		
		    double stopTicks = GetInitialStopInTicks();
		    return direction == OrderAction.Buy ? entryPrice - (stopTicks * TickSize) : entryPrice + (stopTicks * TickSize);
		}

		private double GetProfitTargetPrice(OrderAction direction, double entryPrice)
		{
            if (TargetMode == TradeManagementMode.Dynamic)
            {
                return direction == OrderAction.Buy ? entryPrice + (ProfitTarget * TickSize) : entryPrice - (ProfitTarget * TickSize);
            }

		    if (ProfitTargetMode == ProfitTargetCalculationMode.RangeFiltered)
		    {
		        if (botRangeFiltered != null && botRangeFiltered.IsValidDataPoint(0))
		        {
		            var bands = new Series<double>[]
		            {
		                botRangeFiltered.Values[18], botRangeFiltered.Values[16], botRangeFiltered.Values[14], 
		                botRangeFiltered.Values[12], botRangeFiltered.Values[10], botRangeFiltered.Values[8],
		                botRangeFiltered.Values[0], 
		                botRangeFiltered.Values[9], botRangeFiltered.Values[11], botRangeFiltered.Values[13], 
		                botRangeFiltered.Values[15], botRangeFiltered.Values[17], botRangeFiltered.Values[19]
		            };
		            int closestBandIndex = -1; double minDistance = double.MaxValue;
		            for (int i = 0; i < bands.Length; i++)
		            {
		                double distance = Math.Abs(entryPrice - bands[i][0]);
		                if (distance < minDistance) { minDistance = distance; closestBandIndex = i; }
		            }
		            if (closestBandIndex != -1)
		            {
		                int targetBandIndex = (direction == OrderAction.Buy) ? closestBandIndex + RangeFilteredTakeProfitBand : closestBandIndex - RangeFilteredTakeProfitBand;
		                targetBandIndex = Math.Max(0, Math.Min(bands.Length - 1, targetBandIndex));
		                double calculatedTargetPrice = bands[targetBandIndex][0];
		                if ((direction == OrderAction.Buy && calculatedTargetPrice > entryPrice + (2 * TickSize)) || (direction == OrderAction.Sell && calculatedTargetPrice < entryPrice - (2 * TickSize)))
		                    return Instrument.MasterInstrument.RoundToTickSize(calculatedTargetPrice);
		            }
		        }
		    }
		
		    if (ProfitTargetMode == ProfitTargetCalculationMode.DeviationTrendProfile)
		    {
		        if (botDeviationTrendProfile != null && botDeviationTrendProfile.IsValidDataPoint(0))
		        {
		            var bands = new Series<double>[]
		            {
		                botDeviationTrendProfile.Stdv_4, botDeviationTrendProfile.Stdv_3, botDeviationTrendProfile.Stdv_2, botDeviationTrendProfile.Stdv_1,
		                botDeviationTrendProfile.Avg,
		                botDeviationTrendProfile.Stdv1, botDeviationTrendProfile.Stdv2, botDeviationTrendProfile.Stdv3, botDeviationTrendProfile.Stdv4
		            };
		            int closestBandIndex = -1; double minDistance = double.MaxValue;
		            for (int i = 0; i < bands.Length; i++)
		            {
		                double distance = Math.Abs(entryPrice - bands[i][0]);
		                if (distance < minDistance) { minDistance = distance; closestBandIndex = i; }
		            }
		            if (closestBandIndex != -1)
		            {
		                int targetBandIndex = (direction == OrderAction.Buy) ? closestBandIndex + DTPTakeProfitBand : closestBandIndex - DTPTakeProfitBand;
		                targetBandIndex = Math.Max(0, Math.Min(bands.Length - 1, targetBandIndex));
		                double calculatedTargetPrice = bands[targetBandIndex][0];
		                if ((direction == OrderAction.Buy && calculatedTargetPrice >= entryPrice + (2 * TickSize)) || (direction == OrderAction.Sell && calculatedTargetPrice <= entryPrice - (2 * TickSize)))
		                    return Instrument.MasterInstrument.RoundToTickSize(calculatedTargetPrice);
		            }
		        }
		    }
		    
		    double standardTargetTicks = (ProfitTargetMode == ProfitTargetCalculationMode.ATR && ATR1 != null) ? (ATR1[0] * ProfitTarget_ATR_Mult) / TickSize :
		        (ProfitTargetMode == ProfitTargetCalculationMode.RiskRewardRatio) ? GetInitialStopInTicks() * RiskRewardRatio : ProfitTarget;
		    
		    return direction == OrderAction.Buy ? entryPrice + (standardTargetTicks * TickSize) : entryPrice - (standardTargetTicks * TickSize);
		}
		
		private void ManageInTradeStops_OnMarketData(double currentPrice)
		{
			if (Position.MarketPosition == MarketPosition.Flat || stopOrder == null) return;
		
			if (BESetAuto && !BE_Realized)
			{
				bool isBETriggered = (Position.MarketPosition == MarketPosition.Long && currentPrice >= bePriceTrigger) || 
                                     (Position.MarketPosition == MarketPosition.Short && currentPrice <= bePriceTrigger);
				if (isBETriggered)
				{
					double side = (Position.MarketPosition == MarketPosition.Long ? 1 : -1);
					double beLevel = Instrument.MasterInstrument.RoundToTickSize(Position.AveragePrice + (side * BE_Offset * TickSize));
					UpdateStopOrderPrice(beLevel); BE_Realized = true;
				}
			}

			if (!startTrail && Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks, Close[0]) >= Trail_Trigger) startTrail = true;
			if (!startTrail || StopType != StopManagementType.RegularTrail) return;
		
			if (Position.MarketPosition == MarketPosition.Long && currentPrice >= Stop_Trigger)
			{
				UpdateStopOrderPrice(currentPrice - (Trail_Size * TickSize)); Stop_Trigger = currentPrice + (Trail_frequency * TickSize);
			}
			else if (Position.MarketPosition == MarketPosition.Short && currentPrice <= Stop_Trigger)
			{
				UpdateStopOrderPrice(currentPrice + (Trail_Size * TickSize)); Stop_Trigger = currentPrice - (Trail_frequency * TickSize);
			}
		}
		
		private void ManageInTradeStops_OnBarUpdate()
		{
			if (Position.MarketPosition == MarketPosition.Flat || stopOrder == null) return;
			double newStopPrice = 0;
			double profitTargetTicks = GetProfitTargetInTicks();
			double currentProfitTicks = Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks, Close[0]);
			
			switch (StopType)
			{
				case StopManagementType.ATR:
					if (TrailStop_ATR != null && TrailStop_ATR.IsValidDataPoint(0))
						newStopPrice = (Position.MarketPosition == MarketPosition.Long) ? TrailStop_ATR.TrailingStopLow[0] : TrailStop_ATR.TrailingStopHigh[0];
					break;
				case StopManagementType.HighLow:
					if (CurrentBar > highLowTrailCurrentLookback)
						newStopPrice = (Position.MarketPosition == MarketPosition.Long) ? MIN(Low, highLowTrailCurrentLookback)[0] : MAX(High, highLowTrailCurrentLookback)[0];
					break;
				case StopManagementType.ParabolicSAR:
					if (botPSAR != null && botPSAR.IsValidDataPoint(0)) newStopPrice = botPSAR[0];
					break;
				case StopManagementType.StagedTrail:
					if (!BE_Realized) break;
					if (profitTargetTicks > 0 && currentProfitTicks >= (profitTargetTicks * (FinalTrailTriggerPercent / 100.0)))
						newStopPrice = (Position.MarketPosition == MarketPosition.Long) ? Close[0] - (FinalTrailTicks * TickSize) : Close[0] + (FinalTrailTicks * TickSize);
					else
						newStopPrice = (Position.MarketPosition == MarketPosition.Long) ? Close[0] - (InitialTrailTicks * TickSize) : Close[0] + (InitialTrailTicks * TickSize);
					break;
				case StopManagementType.HybridAtrPsar:
					if (!isHybridPsarActive && profitTargetTicks > 0 && currentProfitTicks >= (profitTargetTicks * (HybridTrailTriggerPercent / 100.0))) isHybridPsarActive = true;
					if (isHybridPsarActive && botPSAR != null) newStopPrice = botPSAR[0];
					else if (TrailStop_ATR != null) newStopPrice = (Position.MarketPosition == MarketPosition.Long) ? TrailStop_ATR.TrailingStopLow[0] : TrailStop_ATR.TrailingStopHigh[0];
					break;
			}
			UpdateStopOrderPrice(newStopPrice);
		}

		private void UpdateStopOrderPrice(double newStopPrice)
		{
			if (newStopPrice.ApproxCompare(0) == 0 || stopOrder == null) return;
			newStopPrice = Instrument.MasterInstrument.RoundToTickSize(newStopPrice);
			if (Position.MarketPosition == MarketPosition.Long && newStopPrice > stopOrder.StopPrice && chkLongPositionStop(newStopPrice))
				ChangeOrder(stopOrder, stopOrder.Quantity, 0, newStopPrice);
			else if (Position.MarketPosition == MarketPosition.Short && newStopPrice < stopOrder.StopPrice && chkShortPositionStop(newStopPrice))
				ChangeOrder(stopOrder, stopOrder.Quantity, 0, newStopPrice);
		}
		#endregion
		
		#region Unmanaged Core Methods

        protected void FlattenAllPositions()
        {
			if (Account == null) return;
			Account.Flatten(new List<Instrument> { Instrument });
        }
		
		private bool chkLongPositionStop(double Price) { return (Price < GetCurrentAsk()) && (Price < GetCurrentBid()); }	
		private bool chkLongPositionProfit(double Price) { return (Price > GetCurrentAsk()) && (Price > GetCurrentBid()); }			
		private bool chkShortPositionStop(double Price) { return (Price > GetCurrentAsk()) && (Price > GetCurrentBid()); }			
		private bool chkShortPositionProfit(double Price) { return (Price < GetCurrentAsk()) && (Price < GetCurrentBid()); }		
		
		private void InitializeOperation()
		{
			entryOrder = stopOrder = targetOrder = null; currTargetPrice = 0; currStopPrice = 0; isHybridPsarActive = false;
			ResetOperation(); return;
		}
		
		private void ResetOperation() { iCurrentPosition = 0; activeOrder = false; orderBarNumber = 0; return; }	

		private void SubmitLongOrder(int quantity, string signalName, string myOrderType)
		{	
            if (State == State.Historical) oco = DateTime.Now.ToString() + CurrentBar + "HLE";
            else oco = GetAtmStrategyUniqueId() + "LE";
            SubmitOrderUnmanaged(0, OrderAction.Buy, (myOrderType == "Market" ? OrderType.Market : OrderType.Limit), quantity, (myOrderType == "Market" ? 0 : GetCurrentBid(0)-LimitOffset*TickSize), 0, oco, signalName);
            if (signalName != Add1 && signalName != CP1C) iCurrentPosition += quantity;
			orderBarNumber = CurrentBar;
        }  

		private void SubmitShortOrder(int quantity, string signalName, string myOrderType)
		{	
            if (State == State.Historical) oco = DateTime.Now.ToString() + CurrentBar + "HSE";
            else oco = GetAtmStrategyUniqueId() + "SE";
            SubmitOrderUnmanaged(0, OrderAction.SellShort, (myOrderType == "Market" ? OrderType.Market : OrderType.Limit), quantity, (myOrderType == "Market" ? 0 : GetCurrentAsk(0)+LimitOffset*TickSize), 0, oco, signalName);
            if (signalName != Add1 && signalName != CP1C) iCurrentPosition += quantity;
			orderBarNumber = CurrentBar;
        }
		
		protected void AddOneEntry()
		{
			if (Position.MarketPosition != MarketPosition.Flat && Position.Quantity + 1 <= EntriesPerDirection)
			{
				if (Position.MarketPosition == MarketPosition.Long) SubmitLongOrder(1, Add1, "Market");
				else SubmitShortOrder(1, Add1, "Market");
			}
		}
		
		protected void CloseOneExit(int quantityToClose)
		{
			if (stopOrder != null && Position.Quantity > quantityToClose) 
			{
				if (Position.MarketPosition == MarketPosition.Long) SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, quantityToClose, 0, 0, "", CP1C);
				else SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, quantityToClose, 0, 0, "", CP1C);
			}
            else if (stopOrder != null && Position.Quantity <= quantityToClose) CloseAllOperation("Close All (Partial)");
		}

		protected void CloseAllOperation(string txtSignal)
		{	
            if (stopOrder != null) CancelOrder(stopOrder);
            if (targetOrder != null) CancelOrder(targetOrder);
            stopOrder = targetOrder = null;
        	if (Position.MarketPosition != MarketPosition.Flat)
        	{
				if (Position.MarketPosition == MarketPosition.Long) SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Market, Position.Quantity, 0, 0, "", txtSignal);
				else SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Market, Position.Quantity, 0, 0, "", txtSignal);
         	}
			ResetOperation();
		}

		protected void checkOrder()
		{
			if (activeOrder && Position.MarketPosition == MarketPosition.Flat && entryOrder != null) 
			{	
				CancelOrder(entryOrder); orderBarNumber = 0; activeOrder = false; entryOrder = null;
			}
		}
		
		protected void checkPositions() { if (Position.MarketPosition == MarketPosition.Flat) { foreach (var order in Orders) { if (order != null) CancelOrder(order); } } }	
		
        private void ReconcileAccountOrders()
		{
		    lock (orderLock)
		    {
				if (Account == null) return;
				List<Order> accountOrders = Orders.Where(o => o.Instrument == Instrument && o.Account == Account).ToList();
				if (accountOrders.Count == 0) return;
				HashSet<string> strategyOrderIds = new HashSet<string>();
				if(entryOrder != null) strategyOrderIds.Add(entryOrder.OrderId);
				if(stopOrder != null) strategyOrderIds.Add(stopOrder.OrderId);
				if(targetOrder != null) strategyOrderIds.Add(targetOrder.OrderId);
				foreach (Order o in accountOrders) if (o.OrderState == OrderState.Working && !strategyOrderIds.Contains(o.OrderId)) { try { CancelOrder(o); } catch { } }
		    }
		}

		protected void MoveToBreakeven()
		{
		    if (isFlat || stopOrder == null || Position.AveragePrice == 0) return;
		    double beStopPrice = Instrument.MasterInstrument.RoundToTickSize(Position.AveragePrice + (Position.MarketPosition == MarketPosition.Long ? 1 : -1) * Math.Abs(BE_Offset) * TickSize);
			if ((isLong && chkLongPositionStop(beStopPrice)) || (!isLong && chkShortPositionStop(beStopPrice)))
			{
				ChangeOrder(stopOrder, stopOrder.Quantity, 0, beStopPrice); BE_Realized = true;
			}
		}
		
		protected void MoveStopToSwingPoint()
		{
		    if (isFlat || stopOrder == null || CurrentBar < ManualMoveStopLookback) return;
		    double newStopPrice = Instrument.MasterInstrument.RoundToTickSize(isLong ? Low[ManualMoveStopLookback] - LimitOffset*TickSize : High[ManualMoveStopLookback] + LimitOffset*TickSize);
		    if ((isLong && newStopPrice > stopOrder.StopPrice && newStopPrice < GetCurrentBid()) || (!isLong && newStopPrice < stopOrder.StopPrice && newStopPrice > GetCurrentAsk()))
			    ChangeOrder(stopOrder, stopOrder.Quantity, 0, newStopPrice);
		}
		
		protected void MoveTrailingStopByPercentage(double percentage)
		{
		    if (percentage <= 0 || percentage >= 1 || isFlat || stopOrder == null) return;
            double dist = Math.Abs(Close[0] - stopOrder.StopPrice); if (dist < TickSize) return;
            double newStopPrice = Instrument.MasterInstrument.RoundToTickSize(isLong ? stopOrder.StopPrice + (dist * percentage) : stopOrder.StopPrice - (dist * percentage));
			if ((isLong && chkLongPositionStop(newStopPrice)) || (!isLong && chkShortPositionStop(newStopPrice)))
				ChangeOrder(stopOrder, stopOrder.Quantity, 0, newStopPrice);
		}
		#endregion
    }
}