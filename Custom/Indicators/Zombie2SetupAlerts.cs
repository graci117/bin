
#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.Code;
using NinjaTrader.NinjaScript.Indicators.Z2SA;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{ 
   
public class Zombie2SetupAlerts : Indicator
{

        private const string SystemVersion = "v1.028";
        private const string SystemName = "Zombie2SetupAlerts";
        private const string FullSystemName = SystemName + " - " + SystemVersion;
        private const string ObjectPrefix = "zsa_";

        private int lastPrintOutputHashCode = 0;
        private Instrument attachedInstrument = null;
        private double attachedInstrumentTickSize = 0;
        private int attachedInstrumentTicksPerPoint = 0;
        private double attachedInstrumentTickValue = 0;

        private Brush barColorZombieBuy = Brushes.Chartreuse;
        private Brush barColorHalfLifeBuy = Brushes.Chartreuse; 
		private Brush barColorZombieSell = Brushes.Red;
		private Brush barColorHalfLifeSell = Brushes.Red;  

        private EMA emafastValue;
        private EMA emaMiddleValue;
        private EMA emaSlowValue;

        private Series<bool> buyZombieSetupSeries;
        private Series<bool> sellZombieSetupSeries;
        private Series<bool> buyHalfLifeSetupSeries;
        private Series<bool> sellHalfLifeSetupSeries;

        private SignalCache signalCache = null;

        private RealRunOncePerBar MomentumStackRunOncePerBar = new RealRunOncePerBar();


        private Brush momentumStackBullishColor = Brushes.Transparent;
        private Brush momentumStackBearishColor = Brushes.Transparent;
        private Brush momentumStackLossColor = Brushes.Transparent;
        private SimpleFont defaultFont = null;

        string m_spot1LastText = "";
        string m_spot2LastText = "";
        string m_spot3LastText = "";
        string m_spot4LastText = "";
        string m_spot5LastText = "";
        string m_spot6LastText = "";
        string m_spot7LastText = "";
        string m_spot8LastText = "";
        string m_spot9LastText = "";
        string m_spot10LastText = "";
        string m_spot11LastText = "";

        Brush m_spot1LastColor = Brushes.Transparent;
        Brush m_spot2LastColor = Brushes.Transparent;
        Brush m_spot3LastColor = Brushes.Transparent;
        Brush m_spot4LastColor = Brushes.Transparent;
        Brush m_spot5LastColor = Brushes.Transparent;
        Brush m_spot6LastColor = Brushes.Transparent;
        Brush m_spot7LastColor = Brushes.Transparent;
        Brush m_spot8LastColor = Brushes.Transparent;
        Brush m_spot9LastColor = Brushes.Transparent;
        Brush m_spot10LastColor = Brushes.Transparent;
        Brush m_spot11LastColor = Brushes.Transparent;

        bool isSpotsInitialized = false;

        public enum SignalTypes
        {
            None = 0,
            ZombieBuy = 1,
            ZombieSell = 2,
            HalfLifeBuy = 3,
            HalfLifeSell = 4
        };

        public override string DisplayName
        {
            get { return FullSystemName; }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = SystemName;
                Description = FullSystemName;

                PaintPriceMarkers = false;
                Calculate = Calculate.OnPriceChange;
                IsOverlay				= true;

                ShowZombieEntrySetups = true;
                ShowHalfLifeEntrySetups = true;
                ShowMomentumStack = true;

                EMAFastPeriod = 8;
                EMAMiddlePeriod = 21;
                EMASlowPeriod = 89;
                SignalBarPaddingTicks = 8;

                MomentumStackBullishColor = Brushes.ForestGreen;
                MomentumStackBearishColor = Brushes.Maroon;
                MomentumStackLossColor = Brushes.DimGray;

                MomentumStackFontSize = 10;

            }
			
			else if (State == State.Configure)
			{
                attachedInstrument = this.Instrument;
                attachedInstrumentTickSize = GetTickSize(attachedInstrument);
                attachedInstrumentTicksPerPoint = GetTicksPerPoint(attachedInstrumentTickSize);
                attachedInstrumentTickValue = GetTickValue(attachedInstrument);

                momentumStackBullishColor.Freeze();
                momentumStackBearishColor.Freeze();
                momentumStackLossColor.Freeze();

                buyZombieSetupSeries = new Series<bool>(this);
                sellZombieSetupSeries = new Series<bool>(this);
                buyHalfLifeSetupSeries = new Series<bool>(this);
                sellHalfLifeSetupSeries = new Series<bool>(this);
            }
            else if (State == State.DataLoaded)
            {
                PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
                PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

                Series<double> signalSeries = new Series<double>(this, this.MaximumBarsLookBack);
                signalCache = new SignalCache(signalSeries);

                emafastValue = EMA(Close, EMAFastPeriod);
                emaMiddleValue = EMA(Close, EMAMiddlePeriod);
                emaSlowValue = EMA(Close, EMASlowPeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < EMASlowPeriod)
                return;

            MomentumStackRunOncePerBar.UpdateBarTime(Time[0]);

            bool bullishTrend = (emaMiddleValue[0] >= emaSlowValue[0]);
            bool bullishMicroTrend = (emafastValue[0] >= emafastValue[1]);
            bool bullishMicroTrendPrevious = (emafastValue[1] >= emafastValue[2]);
            bool bullishCandle = Close[0] >= Close[1];


            if (bullishTrend)
            {
                bool buyZombieSetup = bullishCandle && bullishMicroTrend && !bullishMicroTrendPrevious;
                bool sellHalfLifeSetup = !bullishCandle && !bullishMicroTrend && bullishMicroTrendPrevious;

                if (buyZombieSetup)
                {
                    buyZombieSetupSeries[0] = (true);
                    sellZombieSetupSeries[0] = (false);
                    buyHalfLifeSetupSeries[0] = (false);
                    sellHalfLifeSetupSeries[0] = (false);

                    if (ShowZombieEntrySetups)
                    {
                        ClearAllSignalsFromBar(CurrentBar);
                        DrawBuyZombieSignal(CurrentBar, Low[0], barColorZombieBuy);
                    }
                }
                else if (sellHalfLifeSetup)
                {
                    buyZombieSetupSeries[0] = (false);
                    sellZombieSetupSeries[0] = (false);
                    buyHalfLifeSetupSeries[0] = (false);
                    sellHalfLifeSetupSeries[0] = (true);

                    if (ShowHalfLifeEntrySetups)
                    {
                        ClearAllSignalsFromBar(CurrentBar);
                        DrawSellHalfLifeSignal(CurrentBar, High[0], barColorHalfLifeSell);
                    }
                }
                else
                {
                    buyZombieSetupSeries[0] = (false);
                    sellZombieSetupSeries[0] = (false);
                    buyHalfLifeSetupSeries[0] = (false);
                    sellHalfLifeSetupSeries[0] = (false);

                    ClearAllSignalsFromBar(CurrentBar);
                }
            }
            else
            {
                bool sellZombieSetup = !bullishCandle && !bullishMicroTrend && bullishMicroTrendPrevious;
                bool buyHalfLifeSetup = bullishCandle && bullishMicroTrend && !bullishMicroTrendPrevious;

                if (sellZombieSetup)
                {
                    buyZombieSetupSeries[0] = (false);
                    sellZombieSetupSeries[0] = (true);
                    buyHalfLifeSetupSeries[0] = (false);
                    sellHalfLifeSetupSeries[0] = (false);

                    if (ShowZombieEntrySetups)
                    {
                        ClearAllSignalsFromBar(CurrentBar);
                        DrawSellZombieSignal(CurrentBar, High[0], barColorZombieSell);
                    }
                }
                else if (buyHalfLifeSetup)
                {
                    buyZombieSetupSeries[0] = (false);
                    sellZombieSetupSeries[0] = (false);
                    buyHalfLifeSetupSeries[0] = (true);
                    sellHalfLifeSetupSeries[0] = (false);

                    if (ShowHalfLifeEntrySetups)
                    {
                        ClearAllSignalsFromBar(CurrentBar);
                        DrawBuyHalfLifeSignal(CurrentBar, Low[0], barColorHalfLifeBuy);
                    }
                }
                else
                {
                    buyZombieSetupSeries[0] = (false);
                    sellZombieSetupSeries[0] = (false);
                    buyHalfLifeSetupSeries[0] = (false);
                    sellHalfLifeSetupSeries[0] = (false);

                    ClearAllSignalsFromBar(CurrentBar);
                }
            }


            //Setup Wave
            if (this.ChartControl != null && ShowMomentumStack)
            {
                if (defaultFont == null)
                {
                    int fontSize = this.MomentumStackFontSize;
                    //defaultFont = new SimpleFont(this.ChartControl.Properties.LabelFont.Family.ToString(), fontSize) { Bold = true };
                    defaultFont = new SimpleFont("Courier New", fontSize) { Bold = true };
                }

                if (!isSpotsInitialized)
                {
                    DrawSpot11(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot10(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot9(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot8(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot7(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot6(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot5(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot4(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot3(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot2(this.ChartControl, "?.000", Brushes.Transparent);
                    DrawSpot1(this.ChartControl, "?.000", Brushes.Transparent);
                    isSpotsInitialized = true;
                }


                bool bullishTrend2 = (emaMiddleValue[1] >= emaSlowValue[1]);
                bool bullishMicroTrend2 = (emafastValue[1] >= emafastValue[2]);
                bool bullishMicroTrendPrevious2 = (emafastValue[2] >= emafastValue[3]);
                bool bullishCandle2 = Close[1] >= Close[2];

                if (bullishTrend2)
                {
                    bool buyZombieSetup2 = bullishCandle2 && bullishMicroTrend2 && !bullishMicroTrendPrevious2;
                    bool sellHalfLifeSetup2 = !bullishCandle2 && !bullishMicroTrend2 && bullishMicroTrendPrevious2;

                    if (buyZombieSetup2)
                    {
                        CollectPerformanceMetrics(SignalTypes.ZombieBuy, Close[1], Time[0]);
                    }
                    else if (sellHalfLifeSetup2)
                    {
                        CollectPerformanceMetrics(SignalTypes.HalfLifeSell, Close[1], Time[0]);
                    }

                }
                else
                {
                    bool sellZombieSetup2 = !bullishCandle2 && !bullishMicroTrend2 && bullishMicroTrendPrevious2;
                    bool buyHalfLifeSetup2 = bullishCandle2 && bullishMicroTrend2 && !bullishMicroTrendPrevious2;

                    if (sellZombieSetup2)
                    {
                        CollectPerformanceMetrics(SignalTypes.ZombieSell, Close[1], Time[0]);
                    }
                    else if (buyHalfLifeSetup2)
                    {
                        CollectPerformanceMetrics(SignalTypes.HalfLifeBuy, Close[1], Time[0]);
                    }

                }

                if (setupResults.Count > 10)
                {
                    setupResults.RemoveAt(0);
                }

                string pnlStringFormater = "D3";

                if (setupResults.Count >= 1)
                {
                    int resultIndex = 0;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot11(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 2)
                {
                    int resultIndex = 1;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot10(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 3)
                {
                    int resultIndex = 2;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot9(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 4)
                {
                    int resultIndex = 3;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot8(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 5)
                {
                    int resultIndex = 4;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot7(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 6)
                {
                    int resultIndex = 5;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;
                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot6(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 7)
                {
                    int resultIndex = 6;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot5(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 8)
                {
                    int resultIndex = 7;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot4(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 9)
                {
                    int resultIndex = 8;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot3(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 10)
                {
                    int resultIndex = 9;
                    int pnlTicks = setupResults[resultIndex].PnLTicks;

                    Brush tempBrush = GetMomentumStackCloseColor(setupResults[resultIndex].SignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupCloseDirectionLetter(setupResults[resultIndex].SignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot2(this.ChartControl, textValue, tempBrush);
                }

                if (setupResults.Count >= 1)
                {
                    bool tempSignalTypeBullish = !(lastSignalType == SignalTypes.HalfLifeBuy || lastSignalType == SignalTypes.ZombieBuy);
                    double tempLastSignalPrice = lastSignalPrice;
                    int tempPerformanceTicks = 0;

                    double currentPrice = Close[0];

                    if (tempSignalTypeBullish)
                    {
                        tempPerformanceTicks = (int)((tempLastSignalPrice - currentPrice) / attachedInstrumentTickSize);
                        if (tempPerformanceTicks >= 0)
                        {
                            tempPerformanceTicks -= roundTripCommissionTicks;
                        }
                        else
                        {
                            tempPerformanceTicks += (roundTripCommissionTicks * -1);
                        }
                    }
                    else
                    {
                        tempPerformanceTicks = (int)((currentPrice - tempLastSignalPrice) / attachedInstrumentTickSize);
                        if (tempPerformanceTicks >= 0)
                        {
                            tempPerformanceTicks -= roundTripCommissionTicks;
                        }
                        else
                        {
                            tempPerformanceTicks += (roundTripCommissionTicks * -1);
                        }
                    }

                    int pnlTicks = tempPerformanceTicks;

                    Brush tempBrush = GetMomentumStackColor(lastSignalType, pnlTicks);

                    if (pnlTicks < 0) pnlTicks *= -1;

                    string dirLetter = GetSetupDirectionLetter(lastSignalType);
                    string textValue = dirLetter + pnlTicks.ToString(pnlStringFormater);

                    DrawSpot1(this.ChartControl, textValue, tempBrush);
                }

            }
        }


        Brush GetMomentumStackColor(SignalTypes signalType, int pnlTicks)
        {
            Brush tempBrush = MomentumStackLossColor;
            if (pnlTicks >= 0)
            {
                if (signalType == SignalTypes.ZombieBuy || signalType == SignalTypes.HalfLifeBuy)
                {
                    tempBrush = MomentumStackBullishColor;
                }
                else if (signalType == SignalTypes.ZombieSell || signalType == SignalTypes.HalfLifeSell)
                {
                    tempBrush = MomentumStackBearishColor;
                }
            }

            return tempBrush;
        }

        Brush GetMomentumStackCloseColor(SignalTypes signalType, int pnlTicks)
        {
            Brush tempBrush = MomentumStackLossColor;
            if (pnlTicks > 0)
            {
                if (signalType == SignalTypes.ZombieBuy || signalType == SignalTypes.HalfLifeBuy)
                {
                    tempBrush = MomentumStackBearishColor;
                }
                else if (signalType == SignalTypes.ZombieSell || signalType == SignalTypes.HalfLifeSell)
                {
                    tempBrush = MomentumStackBullishColor;
                }
            }

            return tempBrush;
        }

        private string GetSetupDirectionLetter(SignalTypes signalType)
        {
            string directionLetter = "";

            if (signalType == SignalTypes.ZombieBuy || signalType == SignalTypes.HalfLifeBuy)
            {
                directionLetter = "B.";
            }
            else if (signalType == SignalTypes.ZombieSell || signalType == SignalTypes.HalfLifeSell)
            {
                directionLetter = "S.";
            }

            return directionLetter;
        }

        private string GetSetupCloseDirectionLetter(SignalTypes signalType)
        {
            string directionLetter = "";

            if (signalType == SignalTypes.ZombieBuy || signalType == SignalTypes.HalfLifeBuy)
            {
                directionLetter = "S.";
            }
            else if (signalType == SignalTypes.ZombieSell || signalType == SignalTypes.HalfLifeSell)
            {
                directionLetter = "B.";
            }

            return directionLetter;
        }

        private SignalTypes lastSignalType = SignalTypes.None;
        private double lastSignalPrice = 0;
        private const int roundTripCommissionTicks = 2;

        public class SetupResult
        {
            public int PnLTicks
            { get; set; }
            public SignalTypes SignalType
            { get; set; }
        }

        List<SetupResult> setupResults = new List<SetupResult>();

        private void CollectPerformanceMetrics(SignalTypes signalType, double signalPrice, DateTime barTime)
        {
            if (MomentumStackRunOncePerBar.IsFirstRunThisBar)
            {
                MomentumStackRunOncePerBar.SetRunCompletedThisBar();

                if (lastSignalType != signalType)
                {
                    bool tempSignalTypeBullish = (signalType == SignalTypes.HalfLifeBuy || signalType == SignalTypes.ZombieBuy);
                    double tempLastSignalPrice = lastSignalPrice;
                    int tempPerformanceTicks = 0;

                    if (tempSignalTypeBullish)
                    {
                        tempPerformanceTicks = (int)((tempLastSignalPrice - signalPrice) / attachedInstrumentTickSize);
                        if (tempPerformanceTicks >= 0)
                        {
                            tempPerformanceTicks -= roundTripCommissionTicks;
                        }
                        else
                        {
                            tempPerformanceTicks += (roundTripCommissionTicks * -1);
                        }
                    }
                    else
                    {
                        tempPerformanceTicks = (int)((signalPrice - tempLastSignalPrice) / attachedInstrumentTickSize);
                        if (tempPerformanceTicks >= 0)
                        {
                            tempPerformanceTicks -= roundTripCommissionTicks;
                        }
                        else
                        {
                            tempPerformanceTicks += (roundTripCommissionTicks * -1);
                        }
                    }

                    SetupResult setupResult = new SetupResult();
                    setupResult.SignalType = signalType;
                    setupResult.PnLTicks = tempPerformanceTicks;

                    setupResults.Add(setupResult);

                    //PrintOutput("count=" + setupResults.Count + " signal=" + signalType.ToString() + " ticks=" + tempPerformanceTicks);

                    lastSignalType = signalType;
                    lastSignalPrice = signalPrice;
                }
            }
        }

        private void ClearAllSignalsFromBar(int currentBar)
        {
            SignalTypes signalValue = signalCache.CurrentSignal;

            if (signalValue == SignalTypes.ZombieBuy)
                RemoveBuyZombieSignal(currentBar);
            else if (signalValue == SignalTypes.ZombieSell)
                RemoveSellZombieSignal(currentBar);
            else if (signalValue == SignalTypes.HalfLifeBuy)
                RemoveBuyHalfLifeSignal(currentBar);
            else if (signalValue == SignalTypes.HalfLifeSell)
                RemoveSellHalfLifeSignal(currentBar);
        }


        private void DrawBuyZombieSignal(int currentBar, double price, Brush brush)
        {
            string key = BuildBuyZombieKey(currentBar);
            Dot tempDot = Draw.Dot(this, key, true, 0, price - GetSignalBarPadding(), brush);
            tempDot.OutlineBrush = brush;
            tempDot.Dispose();

            signalCache.AddSignal(SignalTypes.ZombieBuy);
        }
        void DrawBuyHalfLifeSignal(int currentBar, double price, Brush brush)
        {
            string key = BuildBuyHalfLifeKey(currentBar);
            Diamond tempDiamond = Draw.Diamond(this, key, true, 0, price - GetSignalBarPadding(), brush);
            tempDiamond.OutlineBrush = brush;
            tempDiamond.Dispose();

            signalCache.AddSignal(SignalTypes.HalfLifeBuy);
        }

        void DrawSellZombieSignal(int currentBar, double price, Brush brush)
        {
            string key = BuildSellZombieKey(currentBar);
            Dot tempDot = Draw.Dot(this, key, true, 0, price + GetSignalBarPadding(), brush);
            tempDot.OutlineBrush = brush;
            tempDot.Dispose();

            signalCache.AddSignal(SignalTypes.ZombieSell);
        }

        void DrawSellHalfLifeSignal(int currentBar, double price, Brush brush)
        {
            string key = BuildSellHalfLifeKey(currentBar);
            Diamond tempDiamond = Draw.Diamond(this, key, true, 0, price + GetSignalBarPadding(), brush);
            tempDiamond.OutlineBrush = brush;
            tempDiamond.Dispose();

            signalCache.AddSignal(SignalTypes.HalfLifeSell);
        }

            

        string BuildBuyZombieKey(int currentBar)
        {
            string key = BuildObjectFullName("zbuy_" + currentBar);

            return key;
        }

        string BuildSellZombieKey(int currentBar)
        {
            string key = BuildObjectFullName("zsell_" + currentBar);

            return key;
        }

        string BuildBuyHalfLifeKey(int currentBar)
        {
            string key = BuildObjectFullName("hbuy_" + currentBar);

            return key;
        }

        string BuildSellHalfLifeKey(int currentBar)
        {
            string key = BuildObjectFullName("hsell_" + currentBar);

            return key;
        }

        private string BuildObjectFullName(string name)
        {
            string fullName = ObjectPrefix + name;
            return fullName;
        }

        private void RemoveBuyZombieSignal(int currentBar)
        {
            string key = BuildBuyZombieKey(currentBar);
            RemoveDrawObject(key);

            signalCache.ClearSignal();
        }

        private void RemoveSellZombieSignal(int currentBar)
        {
            string key = BuildSellZombieKey(currentBar);
            RemoveDrawObject(key);

            signalCache.ClearSignal();
        }

        private void RemoveBuyHalfLifeSignal(int currentBar)
        {
            string key = BuildBuyHalfLifeKey(currentBar);
            RemoveDrawObject(key);

            signalCache.ClearSignal();
        }

        private void RemoveSellHalfLifeSignal(int currentBar)
        {
            string key = BuildSellHalfLifeKey(currentBar);
            RemoveDrawObject(key);

            signalCache.ClearSignal();
        }

        double GetSignalBarPadding()
        {
            return (this.TickSize * (double)this.SignalBarPaddingTicks);
        }

        private void DrawSpot1(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot1LastText || backgroundColor != m_spot1LastColor)
            {
                m_spot1LastText = text;
                m_spot1LastColor = backgroundColor;

                string formatedText = "\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot1"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot2(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot2LastText || backgroundColor != m_spot2LastColor)
            {
                m_spot2LastText = text;
                m_spot2LastColor = backgroundColor;

                string formatedText = "\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot2"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot3(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot3LastText || backgroundColor != m_spot3LastColor)
            {
                m_spot3LastText = text;
                m_spot3LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot3"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot4(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot4LastText || backgroundColor != m_spot4LastColor)
            {
                m_spot4LastText = text;
                m_spot4LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot4"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }
        private void DrawSpot5(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot5LastText || backgroundColor != m_spot5LastColor)
            {
                m_spot5LastText = text;
                m_spot5LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot5"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot6(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot6LastText || backgroundColor != m_spot6LastColor)
            {
                m_spot6LastText = text;
                m_spot6LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot6"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot7(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot7LastText || backgroundColor != m_spot7LastColor)
            {
                m_spot7LastText = text;
                m_spot7LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot7"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot8(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot8LastText || backgroundColor != m_spot8LastColor)
            {
                m_spot8LastText = text;
                m_spot8LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot8"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot9(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot9LastText || backgroundColor != m_spot9LastColor)
            {
                m_spot9LastText = text;
                m_spot9LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot9"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot10(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot10LastText || backgroundColor != m_spot10LastColor)
            {
                m_spot10LastText = text;
                m_spot10LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot10"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        private void DrawSpot11(ChartControl chartControl, string text, Brush backgroundColor)
        {
            if (text != m_spot11LastText || backgroundColor != m_spot11LastColor)
            {
                m_spot11LastText = text;
                m_spot11LastColor = backgroundColor;

                string formatedText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n" + " " + text + ".";
                Draw.TextFixed(this, BuildObjectFullName("spot11"), formatedText, TextPosition.TopLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
            }
        }

        public static double GetTickSize(Instrument instrument)
        {
            double tickSize = instrument.MasterInstrument.TickSize;

            return (tickSize);
        }

        public double ConvertTicksToDollars(Instrument instrument, int ticks, int contracts)
        {
            double dollarValue = 0;

            if (ticks > 0 && contracts > 0)
            {
                double tickValue = GetTickValue(instrument);
                double tickSize = GetTickSize(instrument);

                dollarValue = tickValue * ticks * contracts;
            }

            return dollarValue;
        }
        public double GetTickValue(Instrument instrument)
        {
            double tickValue = instrument.MasterInstrument.PointValue * instrument.MasterInstrument.TickSize;

            return tickValue;
        }

        public int GetTicksPerPoint(double tickSize)
        {
            int tickPoint = 1;

            if (tickSize < 1)
            {
                tickPoint = (int)(1.0 / tickSize);
            }

            return (tickPoint);
        }

        private void PrintOutput(string output, PrintTo outputTab = PrintTo.OutputTab1, bool blockDuplicateMessages = false)
        {
            this.PrintTo = outputTab;
            if (blockDuplicateMessages)
            {
                int tempHashCode = output.GetHashCode();
                if (tempHashCode != lastPrintOutputHashCode)
                {
                    Print(DateTime.Now + " " + SystemName + ": " + output);
                }
                lastPrintOutputHashCode = tempHashCode;
            }
            else
                Print(DateTime.Now + " " + SystemName + ": " + output);
        }

        public class SignalCache
        {
            private Series<double> signalCache = null;

            public SignalTypes CurrentSignal
            {
                get
                {
                    return (SignalTypes)this.signalCache[0];
                }
            }
            public SignalCache(Series<double> series)
            {
                this.signalCache = series;
            }

            public void AddSignal(SignalTypes signalType)
            {
                this.signalCache[0] = (double)signalType;
            }

            public void ClearSignal()
            {
                this.signalCache[0] = (double)SignalTypes.None;
            }

        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
        public string IndicatorName
        {
            get { return FullSystemName; }
            set { }
        }

        [NinjaScriptProperty]
        [Display(Name = "ShowZombieEntrySetups", Order = 0, GroupName = "Setups")]
        public bool ShowZombieEntrySetups
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ShowHalfLifeEntrySetups", Order = 1, GroupName = "Setups")]
        public bool ShowHalfLifeEntrySetups
        { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "ShowMomentumStack", Order = 2, GroupName = "Setups")]
        public bool ShowMomentumStack
        { get; set; }


        [Display(Name = "EMAFastPeriod", Order = 3, GroupName = "Parameters")]
        [Range(1, int.MaxValue), NinjaScriptProperty]
        public int EMAFastPeriod
        {
            get;
            set;
        }

        [Display(Name = "EMAMiddlePeriod", Order = 4, GroupName = "Parameters")]
        [Range(1, int.MaxValue), NinjaScriptProperty]
        public int EMAMiddlePeriod
        {
            get;
            set;
        }

        [Display(Name = "EMASlowPeriod", Order = 5, GroupName = "Parameters")]
        [Range(1, int.MaxValue), NinjaScriptProperty]
        public int EMASlowPeriod
        {
            get;
            set;
        }

        [Display(ResourceType = typeof(Custom.Resource), Name = "SignalBarPaddingTicks", Order = 6, GroupName = "Parameters")]
        [Range(1, int.MaxValue), NinjaScriptProperty]
        public int SignalBarPaddingTicks
        {
            get;
            set;
        }

    

        [XmlIgnore]	
		[Display(Name = "Zombie Buy", Description = "Zombie Buy", Order = 7, GroupName = "Parameters")]
        public Brush BarColorCondition1y
        {
            get { return barColorZombieBuy; }
            set { barColorZombieBuy = value; }
        }

        [Browsable(false)]
        public string BarColorCondition1Serialize
        {
            get { return Serialize.BrushToString(barColorZombieBuy); }
            set { barColorZombieBuy = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "HalfLife Buy", Description = "HalfLife Buy", Order = 8, GroupName = "Parameters")]
        public Brush BarColorCondition2
        {
            get { return barColorHalfLifeBuy; }
            set { barColorHalfLifeBuy = value; }
        }

        [Browsable(false)]
        public string BarColorCondition2Serialize
        {
            get { return Serialize.BrushToString(barColorHalfLifeBuy); }
            set { barColorHalfLifeBuy = Serialize.StringToBrush(value); }
        }
		
		
		
		[XmlIgnore]		
		[Display(Name = "Zombie Sell", Description = "Zombie Sell", Order = 9, GroupName = "Parameters")]
        public Brush BarColorCondition3
        {
            get { return barColorZombieSell; }
            set { barColorZombieSell = value; }
        }

        [Browsable(false)]
        public string BarColorCondition3Serialize
        {
            get { return Serialize.BrushToString(barColorZombieSell); }
            set { barColorZombieSell = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "HalfLife Sell", Description = "HalfLife Sell", Order = 10, GroupName = "Parameters")]
        public Brush BarColorCondition4
        {
            get { return barColorHalfLifeSell; }
            set { barColorHalfLifeSell = value; }
        }

        [Browsable(false)]
        public string BarColorCondition4Serialize
        {
            get { return Serialize.BrushToString(barColorHalfLifeSell); }
            set { barColorHalfLifeSell = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 11)]
        [XmlIgnore]
        public Brush MomentumStackBullishColor
        {
            get { return momentumStackBullishColor; }
            set { momentumStackBullishColor = value; }
        }

        [Browsable(false)]
        public string MomentumStackBullishColorSerialize
        {
            get { return Serialize.BrushToString(MomentumStackBullishColor); }
            set { MomentumStackBullishColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 12)]
        [XmlIgnore]
        public Brush MomentumStackBearishColor
        {
            get { return momentumStackBearishColor; }
            set { momentumStackBearishColor = value; }
        }

        [Browsable(false)]
        public string MomentumStackBearishColorSerialize
        {
            get { return Serialize.BrushToString(MomentumStackBearishColor); }
            set { MomentumStackBearishColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 13)]
        [XmlIgnore]
        public Brush MomentumStackLossColor
        {
            get { return momentumStackLossColor; }
            set { momentumStackLossColor = value; }
        }

        [Browsable(false)]
        public string MomentumStackLossColorSerialize
        {
            get { return Serialize.BrushToString(MomentumStackLossColor); }
            set { MomentumStackLossColor = Serialize.StringToBrush(value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> BuyZombieSetup
        {
            get { return this.buyZombieSetupSeries; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> SellZombieSetup
        {
            get { return this.sellZombieSetupSeries; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> BuyHalfLifeSetup
        {
            get { return this.buyHalfLifeSetupSeries; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> SellHalfLifeSetup
        {
            get { return this.sellHalfLifeSetupSeries; }
        }

        [Display(Name = "MomentumStackFontSize", Order = 14, GroupName = "Parameters")]
        [Range(1, int.MaxValue), NinjaScriptProperty]
        public int MomentumStackFontSize
        {
            get;
            set;
        }

        #endregion
    }
}

namespace NinjaTrader.NinjaScript.Indicators.Z2SA
{
    public class RealRunOncePerBar
    {
        public DateTime runOncePerBarLastNewBarTime = DateTime.MinValue;
        public DateTime runOncePerBarLastRunBarTime = DateTime.MinValue;

        public bool IsFirstRunThisBar
        {
            get
            {
                return (runOncePerBarLastRunBarTime != runOncePerBarLastNewBarTime);
            }
        }

        public void SetRunCompletedThisBar()
        {
            runOncePerBarLastRunBarTime = runOncePerBarLastNewBarTime;
        }

        public void UpdateBarTime(DateTime currentNewBarTime)
        {
            DateTime tempNewBarTime = currentNewBarTime;

            if (tempNewBarTime != runOncePerBarLastNewBarTime)
            {
                runOncePerBarLastNewBarTime = tempNewBarTime;
            }
        }
    }

}  

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2SetupAlerts[] cacheZombie2SetupAlerts;
		public Zombie2SetupAlerts Zombie2SetupAlerts(string indicatorName, bool showZombieEntrySetups, bool showHalfLifeEntrySetups, bool showMomentumStack, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, int signalBarPaddingTicks, Brush momentumStackBullishColor, Brush momentumStackBearishColor, Brush momentumStackLossColor, int momentumStackFontSize)
		{
			return Zombie2SetupAlerts(Input, indicatorName, showZombieEntrySetups, showHalfLifeEntrySetups, showMomentumStack, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, signalBarPaddingTicks, momentumStackBullishColor, momentumStackBearishColor, momentumStackLossColor, momentumStackFontSize);
		}

		public Zombie2SetupAlerts Zombie2SetupAlerts(ISeries<double> input, string indicatorName, bool showZombieEntrySetups, bool showHalfLifeEntrySetups, bool showMomentumStack, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, int signalBarPaddingTicks, Brush momentumStackBullishColor, Brush momentumStackBearishColor, Brush momentumStackLossColor, int momentumStackFontSize)
		{
			if (cacheZombie2SetupAlerts != null)
				for (int idx = 0; idx < cacheZombie2SetupAlerts.Length; idx++)
					if (cacheZombie2SetupAlerts[idx] != null && cacheZombie2SetupAlerts[idx].IndicatorName == indicatorName && cacheZombie2SetupAlerts[idx].ShowZombieEntrySetups == showZombieEntrySetups && cacheZombie2SetupAlerts[idx].ShowHalfLifeEntrySetups == showHalfLifeEntrySetups && cacheZombie2SetupAlerts[idx].ShowMomentumStack == showMomentumStack && cacheZombie2SetupAlerts[idx].EMAFastPeriod == eMAFastPeriod && cacheZombie2SetupAlerts[idx].EMAMiddlePeriod == eMAMiddlePeriod && cacheZombie2SetupAlerts[idx].EMASlowPeriod == eMASlowPeriod && cacheZombie2SetupAlerts[idx].SignalBarPaddingTicks == signalBarPaddingTicks && cacheZombie2SetupAlerts[idx].MomentumStackBullishColor == momentumStackBullishColor && cacheZombie2SetupAlerts[idx].MomentumStackBearishColor == momentumStackBearishColor && cacheZombie2SetupAlerts[idx].MomentumStackLossColor == momentumStackLossColor && cacheZombie2SetupAlerts[idx].MomentumStackFontSize == momentumStackFontSize && cacheZombie2SetupAlerts[idx].EqualsInput(input))
						return cacheZombie2SetupAlerts[idx];
			return CacheIndicator<Zombie2SetupAlerts>(new Zombie2SetupAlerts(){ IndicatorName = indicatorName, ShowZombieEntrySetups = showZombieEntrySetups, ShowHalfLifeEntrySetups = showHalfLifeEntrySetups, ShowMomentumStack = showMomentumStack, EMAFastPeriod = eMAFastPeriod, EMAMiddlePeriod = eMAMiddlePeriod, EMASlowPeriod = eMASlowPeriod, SignalBarPaddingTicks = signalBarPaddingTicks, MomentumStackBullishColor = momentumStackBullishColor, MomentumStackBearishColor = momentumStackBearishColor, MomentumStackLossColor = momentumStackLossColor, MomentumStackFontSize = momentumStackFontSize }, input, ref cacheZombie2SetupAlerts);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2SetupAlerts Zombie2SetupAlerts(string indicatorName, bool showZombieEntrySetups, bool showHalfLifeEntrySetups, bool showMomentumStack, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, int signalBarPaddingTicks, Brush momentumStackBullishColor, Brush momentumStackBearishColor, Brush momentumStackLossColor, int momentumStackFontSize)
		{
			return indicator.Zombie2SetupAlerts(Input, indicatorName, showZombieEntrySetups, showHalfLifeEntrySetups, showMomentumStack, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, signalBarPaddingTicks, momentumStackBullishColor, momentumStackBearishColor, momentumStackLossColor, momentumStackFontSize);
		}

		public Indicators.Zombie2SetupAlerts Zombie2SetupAlerts(ISeries<double> input , string indicatorName, bool showZombieEntrySetups, bool showHalfLifeEntrySetups, bool showMomentumStack, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, int signalBarPaddingTicks, Brush momentumStackBullishColor, Brush momentumStackBearishColor, Brush momentumStackLossColor, int momentumStackFontSize)
		{
			return indicator.Zombie2SetupAlerts(input, indicatorName, showZombieEntrySetups, showHalfLifeEntrySetups, showMomentumStack, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, signalBarPaddingTicks, momentumStackBullishColor, momentumStackBearishColor, momentumStackLossColor, momentumStackFontSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2SetupAlerts Zombie2SetupAlerts(string indicatorName, bool showZombieEntrySetups, bool showHalfLifeEntrySetups, bool showMomentumStack, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, int signalBarPaddingTicks, Brush momentumStackBullishColor, Brush momentumStackBearishColor, Brush momentumStackLossColor, int momentumStackFontSize)
		{
			return indicator.Zombie2SetupAlerts(Input, indicatorName, showZombieEntrySetups, showHalfLifeEntrySetups, showMomentumStack, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, signalBarPaddingTicks, momentumStackBullishColor, momentumStackBearishColor, momentumStackLossColor, momentumStackFontSize);
		}

		public Indicators.Zombie2SetupAlerts Zombie2SetupAlerts(ISeries<double> input , string indicatorName, bool showZombieEntrySetups, bool showHalfLifeEntrySetups, bool showMomentumStack, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, int signalBarPaddingTicks, Brush momentumStackBullishColor, Brush momentumStackBearishColor, Brush momentumStackLossColor, int momentumStackFontSize)
		{
			return indicator.Zombie2SetupAlerts(input, indicatorName, showZombieEntrySetups, showHalfLifeEntrySetups, showMomentumStack, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, signalBarPaddingTicks, momentumStackBullishColor, momentumStackBearishColor, momentumStackLossColor, momentumStackFontSize);
		}
	}
}

#endregion
