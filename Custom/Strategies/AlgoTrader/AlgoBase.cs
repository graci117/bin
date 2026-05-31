#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.AlgoTrader;
using IOPath = System.IO.Path;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{
    public abstract partial class AlgoBase : Strategy
    {
		#region Enums
        public enum orderSelector { Limit, Market }	
		public enum MasterTrendFilterMode { RangeFiltered, DeviationTrendProfile, Disabled }
		public enum StopManagementType { HighLow, StagedTrail, HybridAtrPsar, Fixed, ParabolicSAR, ATR, RegularTrail }		
		public enum InitialStopCalculationMode { DeviationTrendProfile, RangeFiltered, FixedTicks, ATR }
		public enum ProfitTargetCalculationMode { DeviationTrendProfile, RangeFiltered, FixedTicks, ATR, RiskRewardRatio }
        public enum BETriggerMode { FixedTicks, ProfitTargetPercentage }
		public enum ChopDetectionMode { RangeFiltered, DeviationTrendProfile } 
        public enum TradingMode { Auto, Disabled, DisabledByChop }	
		public enum MarketRegime { Undefined, Trending, Ranging, Breakout, Universal } 
		public enum VolatilityRegime { Undefined, Low, Normal, High }
		public enum MarketStructureRegime { Undefined, Uptrend, Downtrend, Ranging }
		public enum TradeManagementMode { Static, Dynamic }
		public enum BreakevenManagementMode { Static, Dynamic }
		public enum DynamicCalculationMode { Average, Median, Percentile }
		#endregion

		#region Property Backing Fields
		private bool isEnableTime2;
        private bool isEnableTime3;
        private bool isEnableTime4;
        private bool isEnableTime5;
        private bool isEnableTime6 = false; 
		private orderSelector showOrder;
		protected InitialStopCalculationMode initialStopMode;
		protected ProfitTargetCalculationMode profitTargetMode;
		private bool beSetAuto;
		#endregion
		
		#region Variables
		
		#region Unmanaged Order Variables
		private bool isBuySellMarketOrder;
		private int orderBarNumber = 0;
		protected int iCurrentPosition = 0;	
        private string oco;
		private bool activeLE = false;
		private bool activeSE = false;
		public bool activeOrder = false;
		protected Order entryOrder = null;
        protected Order stopOrder = null;
        protected Order targetOrder = null;
		private double currTargetPrice = 0;
		private double currStopPrice = 0;
		public double Target;
		private double Stop;
		protected double Stop_Price = 0;
		protected double Stop_Trigger = 0;
		protected double Target_Price = 0;
		protected double BE_Price = 0;
		protected double bePriceTrigger = 0;
        protected bool startTrail;
		private double newPrice = 0;
		protected bool isInLong = false;
		protected bool isInShort = false;
		#endregion
		
		private bool scaleOut1Triggered = false;
		private bool scaleOut2Triggered = false;
		
        private double lastEntryConfluenceScore = 0; 	
		private long uniqueTradeCounter = 0;
		private bool exitedThisBar = false;
		
		public MarketRegime currentRegime = MarketRegime.Undefined;	
		public MarketRegime currentTradeRegime = MarketRegime.Undefined;
		private MarketRegime signalSourceRegime = MarketRegime.Undefined;
		public VolatilityRegime currentVolatility = VolatilityRegime.Undefined;
		public MarketStructureRegime currentMarketStructure = MarketStructureRegime.Undefined;
		private bool isMasterTrendTemporarilyDisabled = false;
		public bool isTrendOverrideSuggested = false;

		protected double dynamicStopLossTicks = 0;
		protected double dynamicProfitTargetTicks = 0;
		protected double dynamicBreakevenTicks = 0;
		
		protected double currentTradeInitialSLTicks = 0;
		protected double currentTradeInitialTPTicks = 0;		
		protected string currentTradeSignalSource = "---";
        protected string currentTradeStopType = "---";
        protected string currentTradeProfitType = "---";
        protected double currentTradeBETriggerFixedTicks = 0;
        private double entryAtr = 0;
        protected double currentTradeSlippageTicks = 0;
		protected double currentTradeEntryUpVolume = 0;
		protected double currentTradeEntryDownVolume = 0;

        // --- Regime Performance Tracking Variables ---
        protected double RangeMaxMAE = 0;
        protected double RangeMaxMFE = 0;
        protected double TrendMaxMAE = 0;
        protected double TrendMaxMFE = 0;
		
		private Dictionary<MarketRegime, List<double>> recentMfeTicksByRegime = new Dictionary<MarketRegime, List<double>>();
		private Dictionary<MarketRegime, List<double>> recentMaeTicksByRegime = new Dictionary<MarketRegime, List<double>>();
		private Dictionary<MarketRegime, List<double>> recentMfeOfLosersByRegime = new Dictionary<MarketRegime, List<double>>();
				
		[Browsable(false)][XmlIgnore]
		public TradingMode CurrentMode { get; private set; }
		public bool isLongEnabled;
		public bool isShortEnabled;		
		
		private DateTime			currentDate			=	NinjaTrader.Core.Globals.MinDate;
		private double				currentOpen			=	double.MinValue;
		private double				highOfDay			=	double.MinValue;
		private double				lowOfDay			=	double.MaxValue;
		private double				range;
		private double 				currRange;		
		private DateTime			lastDate			= 	NinjaTrader.Core.Globals.MinDate;
		private SessionIterator		sessionIterator;

		protected double currentTradeMaxDrawdownTicks = 0;
		protected double currentTradeMaxProfitTicks = 0;
        protected double highSinceEntry = 0;
        protected double lowSinceEntry = 0;
		
        private DateTime lastEntryTime;
        private readonly TimeSpan tradeDelay = TimeSpan.FromSeconds(5);
		
		private List<(Func<DateTime> Start, Func<DateTime> End, Func<bool> IsEnabled)> tradingSessions;		
		private List<string> activeTradeSignalNames = new List<string>();
		private bool isManualTradeActive = false; 
		private bool isLongManuallyEnabled = false;
		private bool isShortManuallyEnabled = false;
		private bool wasTrendingUpLastBar = false;
		private bool wasTrendingDownLastBar = false;

	    [XmlIgnore]
	    protected bool profitTargetsSet = false;
		
	    private string LogFilePath;
	    private static readonly object LogLock = new object();
	    private bool loggerInitialized = false;		
		
		private TradeLogger tradeLogger;		
		private TradeJsonLogger tradeJsonLogger;

		protected double currentTradeEntryAdx = 0;
		protected double currentTradeEntryMomentum = 0;
		
        private Dictionary<string, int> printedMessages = new Dictionary<string, int>();
	    private string calculatedChartTypeDisplay = string.Empty;
		
        protected bool dtpUp;
        protected bool dtpDown;
        protected bool momoUp;
        protected bool momoDown;
        protected bool adxUp;
		
		protected double currentTMO;
		protected double currentMomentum;
		protected double currentAdx;
		protected double currentAtr;
		
		private double normalizedAtr;
		private SMA longTermAtrSma;

		protected bool priceUp;
		protected bool priceDown;
			
        protected bool longSignal;
        protected bool shortSignal;
        protected bool isLong;
        protected bool isShort;
        protected bool isFlat;
        protected double trailValueInTicks;
		
        protected bool exitLong;
        protected bool exitShort;
		
        protected bool BE_Realized;
        private bool trailingDrawdownReached = false;
        private bool isFinalTrailActive = false; 
		private int highLowTrailCurrentLookback; 
		
		protected int barsInCurrentTrade = 0;
		protected int entryBarNumberOfTrade = -1;
		
        protected double entryPrice;
        private double currentPrice;
        private bool additionalContractExists; 

        private int counterLong;
        private int counterShort;

        private readonly object orderLock = new object();
        private Dictionary<string, Order> activeOrders = new Dictionary<string, Order>();
        protected bool orderErrorOccurred = false;

        private DateTime lastAccountReconciliationTime = DateTime.MinValue;
        private readonly TimeSpan accountReconciliationInterval = TimeSpan.FromMinutes(5);

		private double initialAccountSizeForBacktest;
		private bool initialCapitalCaptured = false;
		
        protected double maxProfit;
		private double dailyMaxProfit;

        private int rejectedOrderCount = 0;
        private DateTime lastTickTime = DateTime.MinValue;
        private bool isHealthCheckDisabled = false;
		
		private int dataLossCounter = 0;
		
        protected double totalPnL;
        protected double cumPnL;
        protected double dailyPnL;
		
		private int sessionWins = 0;
		private int sessionLosses = 0;

        // --- Calmar Calculation Variables ---
        private double globalMaxPnL = double.MinValue;
        private double globalMaxDrawdown = 0;
        private DateTime strategyStartTime = DateTime.MinValue;
		
        [Browsable(false)][XmlIgnore]
        public string a_trendStatus = "---";
        [Browsable(false)][XmlIgnore]
        public string a_signalStatus = "---";
		[Browsable(false)][XmlIgnore]
		public string a_lastSignalSource = "---";
		[Browsable(false)][XmlIgnore]
		public string a_volatilityStatus = "---";
		[Browsable(false)][XmlIgnore]
		public string a_structureStatus = "---";
		[Browsable(false)][XmlIgnore]
		public string a_vetoReason = "---";

		private Brush backgroundUpBrush;
		private Brush backgroundDownBrush;
		
		private List<ISignalBot> allBots;
        private List<ISignalBot> trendBots;
        private List<ISignalBot> rangeBots;
        private List<ISignalBot> breakoutBots;
        private List<ISignalBot> universalBots;
		
		private LinReg rangeVetoLinReg;
		private ATR rangeVetoAtr;
		
		private class BotPerformanceTracker
		{
			public List<bool> RecentTrades { get; private set; } = new List<bool>();
			public int Lookback { get; private set; }
			public double WinRate => RecentTrades.Count > 0 ? (double)RecentTrades.Count(t => t) / RecentTrades.Count : 0.0;
			
			public BotPerformanceTracker(int lookback)
			{
				Lookback = lookback;
			}
			
			public void AddTrade(bool isWin)
			{
				RecentTrades.Add(isWin);
				if(RecentTrades.Count > Lookback)
				{
					RecentTrades.RemoveAt(0);
				}
			}
		}
		private Dictionary<string, BotPerformanceTracker> botPerformanceTrackers;
        #endregion

        public override string DisplayName { get { return Name; } }

        #region OnStateChange
		
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
				Description									= @"Base Strategy with Calmar Ratio Logic v7.1.1";
				Name										= "AlgoBase";
				BaseAlgoVersion								= "AlgoBase v7.1.1";
				Author										= "indiVGA, Khanh Nguyen";
				StrategyVersion								= "7.1.1 Jan 2026 - Calmar Implementation";
				Credits										= "";
				StrategyName 								= "";
				ChartType									= "Tbars 28";	
				paypal 										= "https://www.paypal.com/signin"; 		

                EntriesPerDirection 						= 10;
                Calculate									= Calculate.OnEachTick;
				EntryHandling 								= EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy 				= true;
                ExitOnSessionCloseSeconds 					= 30;
                IsFillLimitOnTouch 							= false;
                MaximumBarsLookBack 						= MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution 						= OrderFillResolution.Standard;
                Slippage 									= 0;
                StartBehavior 								= StartBehavior.WaitUntilFlat;
                TimeInForce 								= TimeInForce.Gtc;
                TraceOrders 								= false;
                RealtimeErrorHandling 						= RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling 							= StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade 						= 250; 
				IsInstantiatedOnEachOptimizationIteration 	= false;
				
                IsUnmanaged                                 = true;
                IsAdoptAccountPositionAware                 = true;
				                	
				IsAutoScale                 = false;
		        DrawOnPricePanel            = false;
		        IsOverlay                   = true;
				
				showOrder						= orderSelector.Limit;
				isBuySellMarketOrder 			= true;
				LimitOffset						= 4;
	
				Contracts						= 1;
				MaxPositionPerTrade				= 10;
				
	            EnableAutoRegimeDetection 		= true;
				FilterMode 						= MasterTrendFilterMode.DeviationTrendProfile;
				ManualRegimeOverride 			= MarketRegime.Undefined;
				
				ChopDetectionMethod 			= ChopDetectionMode.DeviationTrendProfile;
				DtpChopUpperThreshold 			= 0.55;
				DtpChopLowerThreshold 			= 0.45;
				VolatilityPeriod 				= 14;
				VolatilityLongTermLookback		= 100;
				VolatilityThresholdPercent		= 20;	            
				
				EnableTrendBots 				= true;
				EnableRangeBots 				= true;
				EnableBreakoutBots 				= true;
				AutoDisableCounterTrend			= true;
				
				EnableBotPerformanceFilter 		= true;
				BotPerformanceLookback 			= 20;
				MinTradesForFilter 				= 5;
				MinBotWinRatePercent 			= 40;
				
				EnableDynamicSizing 			= false;			
				DynamicInitialSL 				= 50;
				DynamicInitialTP 				= 200;
				DynamicBurnInTrades				= 30;
				DynamicAvgLookback				= 20;			
				DynamicSLPadding 				= 4;
				RiskPerTradePercent 			= 2;
				DynamicRiskMode					= DynamicCalculationMode.Percentile;
				DynamicRiskPercentile			= 80;
				dailyLossProfit					= false;
				DailyProfitLimit				= 1000;
				DailyLossLimit					= 1000;				
				enableTrailingDrawdown 			= false;
				TrailingDrawdown				= 1000;
	
				StopMode 						= TradeManagementMode.Static;
				TargetMode 						= TradeManagementMode.Static;
				initialStopMode 				= InitialStopCalculationMode.FixedTicks;
				InitialStop						= 40;
	            DTPStopLossBand					= 2;
				RangeFilteredStopLossBand		= 2;
				StopLoss_ATR_Period 			= 14;
				StopLoss_ATR_Mult				= 1.6;
				
				profitTargetMode 				= ProfitTargetCalculationMode.FixedTicks;
				ProfitTarget					= 112;
	            DTPTakeProfitBand				= 3;
				RangeFilteredTakeProfitBand		= 3;
				ProfitTarget_ATR_Period 		= 14;
				ProfitTarget_ATR_Mult			= 2.4;
				RiskRewardRatio					= 1.6;
				
				BESetAuto						= true;
				beSetAuto						= true;
				BreakevenMode					= BreakevenManagementMode.Static;
				DynamicInitialBE				= 35;
				DynamicBETargetPercent			= 80;
				BreakevenTriggerMode 			= BETriggerMode.FixedTicks;
				BETriggerPercent				= 20;
				BE_Trigger						= 35;
				BE_Offset						= 4;
				
				EnableScaleOutExecution = true;
				ScaleOutLevel1Percent = 65;
				ScaleOutQty1 = 1;
				ScaleOutLevel2Percent = 75;
				ScaleOutQty2 = 2;
				
				StopType 						= StopManagementType.StagedTrail;
				StagedTrailTriggerPercent 		= 20;
			    InitialTrailTicks           	= 40;     
				FinalTrailTriggerPercent        = 80;        
				FinalTrailTicks                 = 20; 
				
				Trail_Trigger					= 40;
				Trail_Size						= 40;
				Trail_frequency					= 1;
				
				TrailStop_ATR_Period 			= 14;
				TrailStop_ATR_Mult 				= 2;	
				
				HighLowTrailInitialLookback		= 10;
				
	            PSARAcceleration            	= 0.01;
	            PSARAccelerationMax         	= 0.2;
				
				HybridTrailTriggerPercent   	= 50;
				   
				ManualMoveStopLookback          = 1;
				
				EnableDM						= true;
				DmPeriod						= 14;
				AdxThreshold1					= 28.02;				
				AdxThreshold2					= 39.78;
				
	            EnableConfluenceScoring   		= false;
	            MinConfluenceScore        		= 70;
	            ConfluenceAdxThreshold    		= 28;
	
	            EnableOverboughtOversoldFilter 	= false; 
	            TMOOverboughtLevel          	= 10.0; 
	            TMOOversoldLevel            	= -10.0;
	
				ExhaustionBBPeriod				= 20;
				ExhaustionBBStdDev				= 2;
				ShowVolumeUpDown				= false;
				
				EnableRangeVetoFilter 			= true;
				RangeVetoPeriod 				= 100;
				RangeVetoBandMultiplier 		= 1.6;

				AtrPeriod						= 100;
				AtrMultiplier					= 1.25;
				StdDevMultiplier				= 1.0;
				
	            EnableTradingDaysFilter 		= true;
	            TradeOnMonday    				= true;
	            TradeOnTuesday   				= true;
	            TradeOnWednesday 				= true;
	            TradeOnThursday 				= true;
	            TradeOnFriday    				= true;
				Start							= DateTime.Parse("09:32", System.Globalization.CultureInfo.InvariantCulture);
				End								= DateTime.Parse("11:30", System.Globalization.CultureInfo.InvariantCulture);
				Start2							= DateTime.Parse("03:00", System.Globalization.CultureInfo.InvariantCulture);
				End2							= DateTime.Parse("05:00", System.Globalization.CultureInfo.InvariantCulture);
				Start3							= DateTime.Parse("11:30", System.Globalization.CultureInfo.InvariantCulture);
				End3							= DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
				Start4							= DateTime.Parse("12:00", System.Globalization.CultureInfo.InvariantCulture);
				End4							= DateTime.Parse("14:00", System.Globalization.CultureInfo.InvariantCulture);
				Start5							= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End5							= DateTime.Parse("02:00", System.Globalization.CultureInfo.InvariantCulture);
				Start6							= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End6							= DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
				TradesPerDirection				= false;
				longPerDirection				= 5;
				shortPerDirection				= 5;
				
				MomentumPeriod				= 5;
				MomoThreshold				= 2;
				
				ShowTMO						= true;
				TMOLength 					= 14;
				TMOCalcLength 				= 5;
				TMOSmoothLength 			= 3;
				
				DTPAverageType 				= MovingAverageType.Exponential;
				ShowDTPPlot 				= true;
				DTPLength 					= 77;
				DTPNormalizationLookback	= 200;
				DTPVolatilityPeriod			= 14;
	
				KalmanAlpha 				= 0.01;
				KalmanBeta  				= 0.1;
				KalmanPeriod 				= 30;
				DevMultiplier				= 1.2;
				SupertrendFactor 			= 0.7;
				SupertrendAtrPeriod			= 7; 
	
				HiLoLookbackPeriod			= 20;
				HiLoSmoothingPeriod			= 6;
				HiLoAtrMultiplier			= 0.8;
	
				LinRegPeriod    			= 100;
				
				MedBandsHmaPeriod			= 100;
				MedBandsExtremesLookback	= 20;
				MedBandsDriverWidth			= 2;
				MedBandsUpColor				= Brushes.Lime;
				MedBandsDownColor			= Brushes.Red;
				
				VmaBandsPeriod				= 30;
				VmaBandsVolatilityPeriod	= 30;
				VmaBandsExtremesLookback	= 14;
				VmaBandsDriverWidth			= 2;
				VmaBandsUpColor				= Brushes.Lime;
				VmaBandsDownColor			= Brushes.Red;
				
				RegBandsPeriod				= 100;
				
	            RegimeAdxPeriod 			= 14;
	            RegimeAdxTrendThreshold 	= 25;
				RegimeAdxTrendThreshold2 	= 50;
	            RegimeAdxRangeThreshold 	= 25;
	            RegimeBBPeriod 				= 20;
	            RegimeBBStdDev 				= 2;
	            RegimeSqueezeLookback 		= 50;
				
				CoralSmoothingPeriod 		= 20;
				CoralConstantD 				= 1.0; 
				
				JbSignalMacdFast 			= 5; 
				JbSignalMacdSlow 			= 8; 
				JbSignalMacdSmooth 			= 5; 
				JbSignalWrPeriod 			= 21; 
				JbSignalWrEmaPeriod 		= 13; 
				JbSignalAlmaFastLen 		= 19; 
				JbSignalAlmaSlowLen 		= 20;
				
				TrendMagicCciPeriod     	= 20; 
				TrendMagicAtrMult       	= 1.0;
				
				SuperTrendPeriod    		= 40; 
				SuperTrendPoles     		= 2;
				
				TrendSniperLength 			= 30;
				
				BalaUseSMMA     			= true;
				BalaEMAPeriod   			= 21;
				
				HmaHooksPeriod 				= 100;
				
				ShowOpen = true; ShowHigh = true; ShowLow = true;
				wrPeriod = 14; wrUp = -20; wrDown = -80;							
				showDailyPnl = true; EnableTrendBackground = true; FontSize = 15; Transparency = 50; PositionDailyPNL = TextPosition.BottomLeft; colorDailyProfitLoss = Brushes.Cyan;
				showPnl = false; PositionPnl = TextPosition.TopLeft; colorPnl = Brushes.Yellow; ShowHistorical = true; EnableLogging = false; EnableTradeLogging = true;
				TradeLogFileName = "AlgoTradeLog.csv"; EnableJsonLogging = true; JsonLogFileName = "AlgoTradeLog.jsonl";                
		        EnableHealthChecks = true; DataLossTimeoutSeconds = 15; MaxConsecutiveRejections = 4; 
				ShowMomentumExtremesPlot = false; ShowMomentumVmaPlot = false; ShowPSARPlot = false; ShowDM = false; 
				ShowExhaustionBB = false; ShowPivotImpulseLines = false; PIS_SwingStrength = 34; PIS_PivotLookback = 200;
				ShowRegChan = false;
				
                CurrentMode = TradingMode.Auto;
                isLongEnabled = true;
                isShortEnabled = true;				
				isManualTradeActive = false;
				isLongManuallyEnabled = false;
				isShortManuallyEnabled = false;
				
				BaseAlgoVersion = "AlgoBase v7.1.1"; Author = "indiVGA, Khanh Nguyen"; StrategyName = "AlgoBase";
				StrategyVersion = "7.1.1 Jan 2026 - Calmar Implementation"; Credits	= ""; ChartType = "Tbars 28"; paypal = "https://www.paypal.com/signin"; 		
				
				BE_Realized = false; counterLong = 0; counterShort = 0; maxProfit = double.MinValue;
				EnableAutoExit = true;
				
				SwingStrength = 5; 	
				BollingerPeriod = 20; BollingerStdDev = 2;MarketStructurePeriod = 5; MarketStructureUseReversals = true; MarketStructureUseContinuations = false;
				RFSamplingPeriod = 10; RFRangeMultiplier = 1.0; 
				KeltnerOffsetMultiplier = 1.5; KeltnerPeriod = 10;

                RangeMaxMAE = 0; RangeMaxMFE = 0; TrendMaxMAE = 0; TrendMaxMFE = 0;
			}
            else if (State == State.Configure)
            {
				RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                AddDataSeries(BarsPeriodType.Tick, 1);
            }
            else if (State == State.DataLoaded)
            {			
				sessionIterator = new SessionIterator(Bars);
		        InitializeLoggers();
		        InitializeChartDisplayHelpers();
				
		        InitializeAllIndicators();
				
				longTermAtrSma = SMA(ATR(VolatilityPeriod), VolatilityLongTermLookback);
				
                InitializeBotFramework();
				
				botPerformanceTrackers = new Dictionary<string, BotPerformanceTracker>();
				foreach (var bot in allBots)
				{
					botPerformanceTrackers[bot.Name] = new BotPerformanceTracker(BotPerformanceLookback);
				}

				if (EnableRangeVetoFilter)
				{
					rangeVetoLinReg = LinReg(RangeVetoPeriod);
					rangeVetoAtr = ATR(AtrPeriod);
				}
				
				foreach (MarketRegime regime in Enum.GetValues(typeof(MarketRegime)))
				{
					recentMfeTicksByRegime[regime] = new List<double>();
					recentMaeTicksByRegime[regime] = new List<double>();
					recentMfeOfLosersByRegime[regime] = new List<double>();
				}

                tradingSessions = new List<(Func<DateTime> Start, Func<DateTime> End, Func<bool> IsEnabled)>
                {
                    (() => Start,  () => End,  () => true),
                    (() => Start2, () => End2, () => Time2),
                    (() => Start3, () => End3, () => Time3),
                    (() => Start4, () => End4, () => Time4),
                    (() => Start5, () => End5, () => Time5),
                    (() => Start6, () => End6, () => Time6)
                };
		        
				if (State >= State.Historical)
				{
				    if (Account != null)
				        cumPnL = Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
				    else
				        cumPnL = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;

				    dailyPnL = 0;
				    dailyMaxProfit = 0;
				    maxProfit = totalPnL;
                    
                    strategyStartTime = Bars.GetTime(0);
				}
				
				backgroundUpBrush = new SolidColorBrush(Color.FromArgb(32, Colors.Lime.R, Colors.Lime.G, Colors.Lime.B));
				backgroundUpBrush.Freeze();
				
				backgroundDownBrush = new SolidColorBrush(Color.FromArgb(32, Colors.Crimson.R, Colors.Crimson.G, Colors.Crimson.B));
				backgroundDownBrush.Freeze();							
            }
			else if (State == State.Historical)
			{
				if (ChartControl != null) Dispatcher.InvokeAsync((() => { CreateWPFControls(); }));	
				
				if (!Bars.BarsType.IsIntraday) Draw.TextFixed(this, "NinjaScriptInfo", "Error: Daily OHLC requires intra-day data.", TextPosition.BottomRight);
			}
            else if (State == State.Realtime)
            {
                if (strategyStartTime == DateTime.MinValue || strategyStartTime == NinjaTrader.Core.Globals.MinDate)
                    strategyStartTime = DateTime.Now;

                if (entryOrder != null) entryOrder = GetRealtimeOrder(entryOrder);
                if (stopOrder != null) stopOrder = GetRealtimeOrder(stopOrder);
                if (targetOrder != null) targetOrder = GetRealtimeOrder(targetOrder);
            }
			else if (State == State.Terminated)
			{
				if(ChartControl != null) ChartControl.Dispatcher.InvokeAsync(() => { DisposeWPFControls(); });
			}
        }
		#endregion

		#region Initialization
		
		private void InitializeLoggers()
		{
		    if (EnableTradeLogging && !string.IsNullOrEmpty(TradeLogFileName))
		    {
		        try
		        {
		            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NinjaTrader 8", TradeLogFileName);
		            tradeLogger = new TradeLogger(path);
		        }
		        catch (Exception ex) { Print($"Error initializing CSV Logger: {ex.Message}"); }
		    }
		    if (EnableJsonLogging && !string.IsNullOrEmpty(JsonLogFileName))
		    {
		        try
		        {
		            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NinjaTrader 8", JsonLogFileName);
		            tradeJsonLogger = new TradeJsonLogger(path);
		        }
		        catch (Exception ex) { Print($"Error initializing JSON Logger: {ex.Message}"); }
		    }
		}

		private void InitializeChartDisplayHelpers()
		{
		     if (ShowOpen || ShowHigh || ShowLow)
		     {
		        dailyOHLIndicator = CurrentDayOHL();
		        dailyOHLIndicator.ShowOpen = ShowOpen;
		        dailyOHLIndicator.Plots[0].Brush = Brushes.White;
		        dailyOHLIndicator.ShowHigh = ShowHigh;
		        dailyOHLIndicator.Plots[1].Brush = Brushes.LightGreen;
		        dailyOHLIndicator.ShowLow  = ShowLow;
		        dailyOHLIndicator.Plots[2].Brush = Brushes.LightPink;
		        AddChartIndicator(dailyOHLIndicator);
		     }
		     
	        try
	        {
	            string barTypeName = "Unknown";
	
	            Type[] types = NinjaTrader.Core.Globals.AssemblyRegistry.GetDerivedTypes(typeof(BarsType));
	            for (int i = 0; i < types.Length; i++)
	            {
	                Type type = types[i];
	                if (type == null || type.FullName.IsNullOrEmpty()) continue;
	                var type2 = NinjaTrader.Core.Globals.AssemblyRegistry.GetType(type.FullName);
	                if (type2 == null) continue;
	                BarsType bar = Activator.CreateInstance(type2) as BarsType;
	                if (bar != null)
	                {
	                    bar.SetState(State.SetDefaults);
	                    int id = (int)bar.BarsPeriod.BarsPeriodType;
	                    if (id == (int)Bars.BarsPeriod.BarsPeriodType)
	                    {
	                        barTypeName = bar.Name;
	                        bar.SetState(State.Terminated);
	                        break;
	                    }
	                    bar.SetState(State.Terminated);
	                }
	            }
	
	            string displayString = $"{barTypeName} {Bars.BarsPeriod.Value}";
	
	            if (Bars.BarsPeriod.BaseBarsPeriodValue > 0 && Bars.BarsPeriod.BaseBarsPeriodValue != Bars.BarsPeriod.Value)
	                displayString += $"-{Bars.BarsPeriod.BaseBarsPeriodValue}";
	            
	            if (Bars.BarsPeriod.GetType().GetProperty("Value3") != null)
	            {
	                var value3Property = Bars.BarsPeriod.GetType().GetProperty("Value3");
	                if (value3Property != null)
	                {
	                    var value3 = (int)value3Property.GetValue(Bars.BarsPeriod);
	                    if (value3 > 0) displayString += $"-{value3}";
	                }
	            }

	            if (Bars.BarsPeriod.MarketDataType != MarketDataType.Unknown)
	                displayString += $" ({Bars.BarsPeriod.MarketDataType})";
	
	            calculatedChartTypeDisplay = displayString;
	        }
	        catch (Exception ex)
	        {
	            LogError("Failed to calculate chart type display", ex);
	            calculatedChartTypeDisplay = "Unknown Chart Type";
	        }	
		}

		#endregion
		
        #region OnBarUpdate
		
		protected override void OnBarUpdate()			
		{	
			if (CurrentBars[0] < BarsRequiredToTrade || CurrentBars[1] < BarsRequiredToTrade)
				return;		
		
			if (!initialCapitalCaptured && Account.Get(AccountItem.NetLiquidation, Currency.UsDollar) > 0)
			{
				initialAccountSizeForBacktest = Account.Get(AccountItem.NetLiquidation, Currency.UsDollar);
				initialCapitalCaptured = true;
			}
			
			if (BarsInProgress != 0 || orderErrorOccurred) return;
			if(Time[0] - lastEntryTime < tradeDelay) return;
			
			if (entryOrder != null && OrderSelector == orderSelector.Limit && orderBarNumber > 0 && (CurrentBar - orderBarNumber > 5)) 
				checkOrder();
					
			ManageIndicatorVisibility();
			ProcessTickBasedLogic();

			if (IsFirstTickOfBar)
			{
				ProcessBarBasedLogic();
			}
		}
		
		private void ProcessTickBasedLogic()
		{
			UpdatePnL();
			CheckHealth();
			CheckTrailingDrawdown();
			
            bool inChopZone = IsInChopZone();
            SetDisplayStatus(inChopZone);

			if (showDailyPnl) 
				DrawStrategyPnL();
		}
		
		private void ProcessBarBasedLogic()
		{
            if (BarsInProgress != 0)
                return;

			if (!isFlat && !checkTimers())
			{
				CloseAllOperation("Time Slot End");
			}

			if (!isFlat)
			{
				if (High[0] > highSinceEntry) highSinceEntry = High[0];
				if (Low[0] < lowSinceEntry) lowSinceEntry = Low[0];
                ManageInTradeStops_OnBarUpdate(); 
			}

			priceUp = Low[0] > Low[1];
			priceDown = High[0] < High[1];
			
			exitedThisBar = false;
			longSignal = false;
			shortSignal = false;
			signalSourceRegime = MarketRegime.Undefined;
			a_vetoReason = "---";
			
			UpdatePositionState();
			UpdateSessionAndBarMetrics();
			
			if (State == State.Realtime && DateTime.Now - lastAccountReconciliationTime > accountReconciliationInterval)
			{
				ReconcileAccountOrders();
				lastAccountReconciliationTime = DateTime.Now;
			}
	
			UpdateIndicatorValues();

			UpdateMarketState();
			
			if (CurrentMode == TradingMode.Auto)
			{
				ExecuteAutoTradingLogic();
			}
			
			ExecuteManualTradingSignals();
			
			UpdateBackgroundColor();

			if (showPnl) ShowPNLStatus();
			
			UpdateTradeCounters();
			ResetTradeStateIfFlat();

			if (ValidateExitLong()) 
				CloseAllOperation("Auto LX");
			if (ValidateExitShort())
				CloseAllOperation("Auto SX");

			KillSwitch();
		}
		
		private void CheckHealth()
		{
			if (State != State.Realtime || !EnableHealthChecks || isHealthCheckDisabled)
			{
				lastTickTime = DateTime.Now; 
				dataLossCounter = 0;
				return;
			}

			if (lastTickTime != DateTime.MinValue && (DateTime.Now - lastTickTime).TotalSeconds > DataLossTimeoutSeconds)
			{
				dataLossCounter++;
				if(EnableLogging) LogMessage($"HEALTH WARNING: No new tick in {DataLossTimeoutSeconds}s. Strike {dataLossCounter} of 3.");
				
				if (dataLossCounter >= 3)
				{
					if(EnableLogging) LogError($"HEALTH ALERT: 3 consecutive data loss timeouts. Disabling strategy.");
					isHealthCheckDisabled = true;
					SetTradingMode(TradingMode.Disabled);
				}
				lastTickTime = DateTime.Now;
			}
			else
			{
				dataLossCounter = 0;
				lastTickTime = DateTime.Now;
			}
	
			if (rejectedOrderCount >= MaxConsecutiveRejections)
			{
				if(EnableLogging) LogError($"HEALTH ALERT: {rejectedOrderCount} consecutive order rejections. Disabling strategy.");
				isHealthCheckDisabled = true;
				SetTradingMode(TradingMode.Disabled);
			}
		}

		private void UpdateSessionAndBarMetrics()
		{
			lastDate = currentDate;
			currentDate = sessionIterator.GetTradingDay(Time[0]);
			
			if (Bars.IsFirstBarOfSession)
			{
				ResetSessionMetrics();
			}

			if (currentOpen <= double.MinValue)
			{
				currentOpen = Open[0]; highOfDay = High[0]; lowOfDay = Low[0];
			}
			
			highOfDay = Math.Max(highOfDay, High[0]);
			lowOfDay = Math.Min(lowOfDay, Low[0]);
			range = (highOfDay - lowOfDay) / TickSize;
			currRange = (High[0] - Low[0]) / TickSize;
	
			if (!isFlat && entryBarNumberOfTrade > -1) { barsInCurrentTrade = CurrentBar - entryBarNumberOfTrade; } else { barsInCurrentTrade = 0; }
		}
		
		private void UpdateIndicatorValues()
		{
			if (ATR1 != null) { currentAtr = ATR1[0]; }
			if (DM1 != null) { currentAdx = DM1[0]; }
			if (botMomentum != null) { currentMomentum = botMomentum[0]; }
			
            if (botDeviationTrendProfile != null && botDeviationTrendProfile.IsValidDataPoint(0))
            {
                dtpUp = Close[0] > botDeviationTrendProfile.Avg[0];
                dtpDown = Close[0] < botDeviationTrendProfile.Avg[0];
            }

			momoUp = botMomentum != null && currentMomentum > MomoThreshold && currentMomentum > botMomentum[1];
			momoDown = botMomentum != null && currentMomentum < -MomoThreshold && currentMomentum < botMomentum[1];

			if (botTMO != null && CurrentBar > TMOLength) 
			{ 
				currentTMO = botTMO.Main[0];
			}
			
			if (Close[0] > 0)
			{
				normalizedAtr = ATR(VolatilityPeriod)[0] / Close[0];
			}
			
			if (EnableAutoRegimeDetection && regimeBollinger != null && CurrentBar > RegimeBBPeriod)
			{
				regimeBBWidth[0] = regimeBollinger.Upper[0] - regimeBollinger.Lower[0];
			}
		}
		
		private void UpdateMarketState()
		{
		    bool inChopZone = IsInChopZone();
		    if (inChopZone)
		    {
		        currentRegime = MarketRegime.Ranging;
		        if (CurrentMode == TradingMode.Auto) SetTradingMode(TradingMode.DisabledByChop);
		    }
		    else
		    {
		        if (CurrentMode == TradingMode.DisabledByChop) SetTradingMode(TradingMode.Auto);
		        DetectMarketRegime();
		    }
		
		    bool trendDetected = regimeAdx != null && CurrentBar > RegimeAdxPeriod && regimeAdx[0] > RegimeAdxTrendThreshold;
		    bool trendFaded = regimeAdx != null && CurrentBar > RegimeAdxPeriod && regimeAdx[0] < RegimeAdxRangeThreshold;
		    
		    if (FilterMode == MasterTrendFilterMode.Disabled)
		    {
		        if (trendDetected && !isTrendOverrideSuggested)
		        {
		            isTrendOverrideSuggested = true;
		            if (ChartControl != null)
		            {
		                ChartControl.Dispatcher.InvokeAsync(() => UpdateTrendAlertPanel());
		            }
		        }
		        else if (trendFaded && isTrendOverrideSuggested)
		        {
		            isTrendOverrideSuggested = false;
		            if (ChartControl != null)
		            {
		                ChartControl.Dispatcher.InvokeAsync(() => UpdateTrendAlertPanel());
		            }
		        }
		    }
		    else if (isTrendOverrideSuggested)
		    {
		        isTrendOverrideSuggested = false;
		        if (ChartControl != null)
		        {
		            ChartControl.Dispatcher.InvokeAsync(() => UpdateTrendAlertPanel());
		        }
		    }
		
		    SetDisplayStatus(inChopZone);
		    ManageAutoDirectionButtons();
		}
		
		private void ExecuteAutoTradingLogic()
		{
		    CheckForBotSignals();
		
		    if (!longSignal && !shortSignal)
		        return;
		
		    switch (currentRegime)
		    {
		        case MarketRegime.Trending:
		            if (isLongEnabled && IsLongEntryConditionMet())
		                EnterLongPosition();
		            
		            if (isShortEnabled && IsShortEntryConditionMet())
		                EnterShortPosition();
		            break;

		        case MarketRegime.Ranging:
		            if (isLongEnabled && IsRangingLongEntryConditionMet())
		                EnterLongPosition();

		            if (isShortEnabled && IsRangingShortEntryConditionMet())
		                EnterShortPosition();
		            break;

		        case MarketRegime.Breakout:
		            if (isLongEnabled && IsBreakoutLongEntryConditionMet())
		                EnterLongPosition();

		            if (isShortEnabled && IsBreakoutShortEntryConditionMet())
		                EnterShortPosition();
		            break;

		        case MarketRegime.Universal:
		        default:
		            if (isLongEnabled && IsLongEntryConditionMet())
		                EnterLongPosition();
		            
		            if (isShortEnabled && IsShortEntryConditionMet())
		                EnterShortPosition();
		            break;
		    }
		}
		
		private void ExecuteManualTradingSignals()
		{
			if (isManualTradeActive && (IsLongEntryConditionMet() || IsRangingLongEntryConditionMet() || IsBreakoutLongEntryConditionMet()))
			{				
			    Draw.ArrowUp(this, "EntryArrow" + CurrentBar, false, 0, Low[0] - 10 * TickSize, Brushes.Cyan);
			}
			
			if (isManualTradeActive && (IsShortEntryConditionMet() || IsRangingShortEntryConditionMet() || IsBreakoutShortEntryConditionMet()))
			{				
			    Draw.ArrowDown(this, "EntryArrow" + CurrentBar, false, 0, High[0] + 10 * TickSize, Brushes.Yellow);
			}
		}
		
		private void UpdateBackgroundColor()
		{
			if (!EnableTrendBackground) return;
			BackBrush = null;
		}
		
		private void UpdateTradeCounters()
		{
		}
		
		private void ResetTradeStateIfFlat()
		{
			if (!isFlat) return;
			trailValueInTicks = InitialStop;
		}
		
		private void KillSwitch()
		{
		    Action closeAllAndDisable = () =>
		    {
				if (!isFlat)
				{
					CloseAllOperation("Session Limit Exit");
				}
				SetTradingMode(TradingMode.Disabled);
		    };
		
		    if (dailyLossProfit && dailyPnL <= -DailyLossLimit)
		    {
		        closeAllAndDisable();
				Print($"DAILY LOSS LIMIT HIT! Daily PnL is {dailyPnL:C}.");
		    }
		
		    if (dailyLossProfit && dailyPnL >= DailyProfitLimit)
		    {
		        closeAllAndDisable();
				Print($"DAILY PROFIT LIMIT HIT! Daily PnL is {dailyPnL:C}.");
		    }
		}
		
		private void ResetSessionMetrics()
		{
			currentOpen = Open[0]; highOfDay = High[0]; lowOfDay = Low[0];
			SetTradingMode(TradingMode.Auto);
			isManualTradeActive = false; 
            EnableExhaustionFilter = false;
			
			if (State == State.Realtime && Account != null)
				cumPnL = Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar);
			else
				cumPnL = SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;

			dailyPnL = 0;
			maxProfit = totalPnL;
			dailyMaxProfit = 0;
			trailingDrawdownReached = false;
			
			sessionWins = 0;
			sessionLosses = 0;

            RangeMaxMAE = 0;
            RangeMaxMFE = 0;
            TrendMaxMAE = 0;
            TrendMaxMFE = 0;
		}
		
		#endregion
		
		#region Manage Indicator Visibilty
		private void ManageIndicatorVisibility()
		{
			if (!panelActive)
				return;
		
			bool showDTP = false;
			bool showRF = false;
			
			switch (FilterMode)
			{
				case MasterTrendFilterMode.DeviationTrendProfile: showDTP = ShowDTPPlot;						break;
				case MasterTrendFilterMode.RangeFiltered:		showRF = ShowRangeFilteredPlot;					break;
			}
			
			if(botDeviationTrendProfile != null)
				botDeviationTrendProfile.IsVisible = showDTP;
			
			if(botRangeFiltered != null)
				botRangeFiltered.IsVisible = showRF;
		}
		#endregion
		
		#region Calmar and Risk Helper methods

        private double CalculateCalmar()
		{
		    // 1. Safety Check: If we haven't made a profit, Calmar is not a useful metric
		    if (totalPnL <= 0 || globalMaxDrawdown <= 0)
		        return 0;
		
		    if (strategyStartTime == DateTime.MinValue || strategyStartTime == NinjaTrader.Core.Globals.MinDate)
		        return 0;
		    
		    // 2. Calculate Time Elapsed
		    double daysElapsed = (DateTime.Now - strategyStartTime).TotalDays;
		    
		    // 3. Minimum Data Rule: Don't show a valid ratio until at least 1 full trading day has passed
		    // This prevents the "annualization error" seen in your screenshot.
		    if (daysElapsed < 1) 
		        return 0;
		
		    // 4. Calculate Annualized Return (CAGR)
		    double yearsElapsed = daysElapsed / 365.0;
		    double annualizedReturn = totalPnL / yearsElapsed;
		
		    // 5. Final Ratio
		    return annualizedReturn / globalMaxDrawdown;
		}

		private double GetProfitTargetInTicks()
		{
		    if (TargetMode == TradeManagementMode.Dynamic) return ProfitTarget;
		    return ProfitTarget;
		}
		
		protected int CalculatePositionSize()
		{
		    if (!EnableDynamicSizing || Account == null) 
		        return Contracts;
		
		    double stopLossTicks;
		
		    if (InitialStopMode == InitialStopCalculationMode.ATR)
		    {
		        if (ATR1 == null) return 1;
		        stopLossTicks = Math.Max(4, (ATR(StopLoss_ATR_Period)[0] * StopLoss_ATR_Mult) / TickSize);
		    }
		    else
		    {
		        stopLossTicks = GetInitialStopInTicks();
		    }
		
		    if (stopLossTicks <= 0) return 1;
		    
		    double stopLossCurrency = stopLossTicks * Instrument.MasterInstrument.PointValue / Instrument.MasterInstrument.TickSize;
		    if (stopLossCurrency <= 0) return 1;
		
		    double accountValue;
		    if (State == State.Realtime) accountValue = Account.Get(AccountItem.NetLiquidation, Currency.UsDollar);
		    else accountValue = initialAccountSizeForBacktest + SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
		
		    double riskAmount = accountValue * (RiskPerTradePercent / 100.0);
		    int positionSize = (int)Math.Max(1.0, Math.Floor(riskAmount / stopLossCurrency));
		    
		    return positionSize;
		}
		
		private double CalculateDynamicValue(List<double> data, double padding)
		{
			if (data == null || !data.Any()) return 0;
			
			switch(DynamicRiskMode)
			{
				case DynamicCalculationMode.Median:
					var sortedData = data.OrderBy(n => n).ToList();
					int mid = sortedData.Count / 2;
					double median = (sortedData.Count % 2 != 0) ? sortedData[mid] : (sortedData[mid-1] + sortedData[mid]) / 2.0;
					return median + padding;
					
				case DynamicCalculationMode.Percentile:
					var sortedPData = data.OrderBy(n => n).ToList();
					if (!sortedPData.Any()) return 0;
					double index = (DynamicRiskPercentile / 100.0) * (sortedPData.Count - 1);
					if (index <= 0) return sortedPData.First() + padding;
					if (index >= sortedPData.Count - 1) return sortedPData.Last() + padding;
					
					int lowerIndex = (int)Math.Floor(index);
					double fraction = index - lowerIndex;
					double percentile = sortedPData[lowerIndex] + fraction * (sortedPData[lowerIndex + 1] - sortedPData[lowerIndex]);
					return percentile + padding;
					
				case DynamicCalculationMode.Average:
				default:
					return data.Average() + padding;
			}
		}
		#endregion
		
		#region Market Regime and State Management
		
		private void DetectVolatilityRegime()
		{
			if (CurrentBar < VolatilityLongTermLookback || normalizedAtr == 0)
			{
				currentVolatility = VolatilityRegime.Undefined;
				return;
			}

			double longTermAvg = longTermAtrSma[0];
			double highThreshold = longTermAvg * (1 + VolatilityThresholdPercent / 100.0);
			double longTermAvgValue = longTermAvg * (1 - VolatilityThresholdPercent / 100.0);

			if (normalizedAtr > highThreshold)
				currentVolatility = VolatilityRegime.High;
			else if (normalizedAtr < longTermAvgValue)
				currentVolatility = VolatilityRegime.Low;
			else
				currentVolatility = VolatilityRegime.Normal;
		}

		private void DetectMarketStructureRegime()
		{
			if (botSwing == null || CurrentBar < 20) 
			{
				currentMarketStructure = MarketStructureRegime.Undefined;
				return;
			}
		
			int lastHighBar = botSwing.SwingHighBar(0, 1, 200);
			int prevHighBar = botSwing.SwingHighBar(0, 2, 200);
			int lastLowBar  = botSwing.SwingLowBar(0, 1, 200);
			int prevLowBar  = botSwing.SwingLowBar(0, 2, 200);
		
			if (lastHighBar < 0 || prevHighBar < 0 || lastLowBar < 0 || prevLowBar < 0)
			{
				currentMarketStructure = MarketStructureRegime.Undefined;
				return;
			}
			
			double lastHigh = High[lastHighBar];
			double prevHigh = High[prevHighBar];
			double lastLow  = Low[lastLowBar];
			double prevLow  = Low[prevLowBar];
		
			bool isHigherHigh = lastHigh > prevHigh;
			bool isHigherLow  = lastLow > prevLow;
			bool isLowerHigh  = lastHigh < prevHigh;
			bool isLowerLow   = lastLow < prevLow;
		
			if(isHigherHigh && isHigherLow)
				currentMarketStructure = MarketStructureRegime.Uptrend;
			else if (isLowerHigh && isLowerLow)
				currentMarketStructure = MarketStructureRegime.Downtrend;
			else
				currentMarketStructure = MarketStructureRegime.Ranging;
		}
		
		private void DetectMarketRegime()
		{
			DetectVolatilityRegime();
			DetectMarketStructureRegime();

		    if (ManualRegimeOverride != MarketRegime.Undefined)
		    {
		        currentRegime = ManualRegimeOverride;
		    }
		    else if (EnableAutoRegimeDetection)
		    {
		        if (currentMarketStructure == MarketStructureRegime.Uptrend || currentMarketStructure == MarketStructureRegime.Downtrend)
		            currentRegime = MarketRegime.Trending;
		        else
		            currentRegime = MarketRegime.Ranging;

		        if (regimeMinBBW != null && regimeBBWidth != null && CurrentBar > RegimeSqueezeLookback)
		        {
		            double currentBBW = regimeBBWidth[0];
		            double minBBW = regimeMinBBW[0];
		            if (currentVolatility == VolatilityRegime.Low && (currentBBW / minBBW) < 1.1)
		                currentRegime = MarketRegime.Breakout;
		        }

		        if (regimeAdx != null && CurrentBar > RegimeAdxPeriod && regimeAdx[0] < RegimeAdxRangeThreshold)
		        {
		            if (currentRegime == MarketRegime.Trending) currentRegime = MarketRegime.Ranging;
		            EnableExhaustionFilter = true;
		        }
		        else
		        {
		            EnableExhaustionFilter = false;
		        }
		    }
		    else
		    {
		        currentRegime = MarketRegime.Undefined;
		    }

		    if (currentRegime == MarketRegime.Ranging) isMasterTrendTemporarilyDisabled = true;
		    else isMasterTrendTemporarilyDisabled = false;
		}
		
        private bool IsInChopZone()
        {
			switch(ChopDetectionMethod)
			{
				case ChopDetectionMode.DeviationTrendProfile:
					if (botDeviationTrendProfile == null || CurrentBar < DTPNormalizationLookback)
						return false; 
					double normalizedSlope = botDeviationTrendProfile.AvgColNorm[0];
					return normalizedSlope >= DtpChopLowerThreshold && normalizedSlope <= DtpChopUpperThreshold;

				case ChopDetectionMode.RangeFiltered:
				default:
					if (botRangeFiltered == null || CurrentBar < KalmanPeriod)
						return false; 
					return botRangeFiltered.IsRanging[0] == 1;
			}
        }

		#region Trend Checking 
		public bool IsTrendingUp()
		{
			if (isMasterTrendTemporarilyDisabled) return true;
			bool isNotOverbought = !EnableOverboughtOversoldFilter || (botTMO != null && CurrentBar > TMOLength && botTMO.Main[0] < TMOOverboughtLevel);
			if (!isNotOverbought) return false;
			
			switch (FilterMode)
			{
				case MasterTrendFilterMode.DeviationTrendProfile:
					if (botDeviationTrendProfile == null || CurrentBar < DTPNormalizationLookback) return false;
					return botDeviationTrendProfile.Trend[0] == 1;
				case MasterTrendFilterMode.RangeFiltered:
					if (botRangeFiltered == null || CurrentBar < KalmanPeriod) return false;
					return botRangeFiltered.TrendState[0] == 1 && botRangeFiltered.IsRanging[0] == 0;
				case MasterTrendFilterMode.Disabled:
				default: return true; 
			}
		}

		public bool IsTrendingDown()
		{
			if (isMasterTrendTemporarilyDisabled) return true;
			bool isNotOversold = !EnableOverboughtOversoldFilter || (botTMO != null && CurrentBar > TMOLength && botTMO.Main[0] > TMOOversoldLevel);
			if (!isNotOversold) return false;
			
			switch (FilterMode)
			{
				case MasterTrendFilterMode.DeviationTrendProfile:
					if (botDeviationTrendProfile == null || CurrentBar < DTPNormalizationLookback) return false;
					return botDeviationTrendProfile.Trend[0] == -1; 
				case MasterTrendFilterMode.RangeFiltered:
					if (botRangeFiltered == null || CurrentBar < KalmanPeriod) return false;
					return botRangeFiltered.TrendState[0] == -1 && botRangeFiltered.IsRanging[0] == 0;
				case MasterTrendFilterMode.Disabled:
				default: return true;
			}
		}
		#endregion
		
		private void UpdatePositionState()
		{
			isLong = Position.MarketPosition == MarketPosition.Long;
			isShort = Position.MarketPosition == MarketPosition.Short;
			isFlat = Position.MarketPosition == MarketPosition.Flat;
			entryPrice = Position.AveragePrice;
			currentPrice = Close[0];
		    additionalContractExists = Position.Quantity > 1;			
		}
		
		public void SetTradingMode(TradingMode newMode)
		{
		    CurrentMode = newMode;
		    if (State >= State.Historical && ChartControl != null)
            {
                ChartControl.Dispatcher.InvokeAsync(() => UpdateManualAutoButtonStates());
            }
		}
		
        public void SetStopManagementMode(TradeManagementMode newMode)
        {
            if (StopMode == newMode) return;
            StopMode = newMode;
            if (Position.MarketPosition != MarketPosition.Flat && stopOrder != null)
            {
                double newStopPrice = GetInitialStopPrice(Position.MarketPosition == MarketPosition.Long ? OrderAction.Buy : OrderAction.Sell, Position.AveragePrice);
                ChangeOrder(stopOrder, stopOrder.Quantity, 0, newStopPrice);
            }
            if (State >= State.Historical && ChartControl != null) ChartControl.Dispatcher.InvokeAsync(() => UpdateStopModeButtonStates());
        }

        public void SetTargetManagementMode(TradeManagementMode newMode)
        {
            if (TargetMode == newMode) return;
            TargetMode = newMode;
            if (Position.MarketPosition != MarketPosition.Flat && targetOrder != null)
            {
                double newTargetPrice = GetProfitTargetPrice(Position.MarketPosition == MarketPosition.Long ? OrderAction.Buy : OrderAction.Sell, Position.AveragePrice);
                ChangeOrder(targetOrder, targetOrder.Quantity, newTargetPrice, 0);
            }
            if (State >= State.Historical && ChartControl != null) ChartControl.Dispatcher.InvokeAsync(() => UpdateTargetModeButtonStates());
        }
		
        public void ToggleStopManagementMode() { SetStopManagementMode(StopMode == TradeManagementMode.Static ? TradeManagementMode.Dynamic : TradeManagementMode.Static); }
        public void ToggleTargetManagementMode() { SetTargetManagementMode(TargetMode == TradeManagementMode.Static ? TradeManagementMode.Dynamic : TradeManagementMode.Static); }
		#endregion

        #region UI, Entry, and Helper methods
        
        protected bool IsLongEntryConditionMet()
		{
		    if (currentAdx <= AdxThreshold1 || currentAdx >= AdxThreshold2) { a_vetoReason = $"ADX not within range ({currentAdx:F1})"; return false; }		
		    if (!longSignal || activeOrder || isFlat == false || exitedThisBar == true || CurrentMode != TradingMode.Auto) return false;
		    if (!checkTimers() || !IsValidTradingDay()) return false;
		    if (dailyLossProfit && (dailyPnL <= -DailyLossLimit || dailyPnL >= DailyProfitLimit)) return false;
		    if (trailingDrawdownReached) return false;
		    if (TradesPerDirection && counterLong >= longPerDirection) return false;
		    if (IsInChopZone()) return false;
		    if (!IsTrendingUp()) return false;
		    
			if (botTMO == null || CurrentBar < TMOLength || (botTMO.Main[0] > botTMO.Signal[0] && botTMO.Main[0] < TMOOversoldLevel)) return false;
		    if (priceUp && longSignal) return true;
		    return false;
		}
		
		protected bool IsShortEntryConditionMet()
		{
		    if (currentAdx <= AdxThreshold1 || currentAdx >= AdxThreshold2) { a_vetoReason = $"ADX not within range ({currentAdx:F1})"; return false; }		
		    if (!shortSignal || activeOrder || isFlat == false || exitedThisBar == true || CurrentMode != TradingMode.Auto) return false;
		    if (!checkTimers() || !IsValidTradingDay()) return false;
		    if (dailyLossProfit && (dailyPnL <= -DailyLossLimit || dailyPnL >= DailyProfitLimit)) return false;
		    if (trailingDrawdownReached) return false;
		    if (TradesPerDirection && counterShort >= shortPerDirection) return false;
		    if (IsInChopZone()) return false;
		    if (!IsTrendingDown()) return false;
		    
			if (botTMO == null || CurrentBar < TMOLength || (botTMO.Main[0] > botTMO.Signal[0] && botTMO.Main[0] > TMOOverboughtLevel)) return false;
		    if (priceDown && shortSignal) return true;
		    return false;
		}
		
		#region Regime-Based Entry Condition Methods

		protected bool IsRangingLongEntryConditionMet()
		{
			if (currentAdx <= AdxThreshold1 || currentAdx >= AdxThreshold2) return false;
			if (!longSignal || activeOrder) return false;
			if (IsInChopZone()) return false;
			if (botTMO == null || CurrentBar < TMOLength || (botTMO.Main[0] > botTMO.Signal[0] && botTMO.Main[0] < TMOOversoldLevel)) return false;
		    return isFlat && !exitedThisBar && CurrentMode == TradingMode.Auto && priceUp;
		}
		
		protected bool IsRangingShortEntryConditionMet()
		{
			if (currentAdx <= AdxThreshold1 || currentAdx >= AdxThreshold2) return false;
		    if (!shortSignal || activeOrder) return false;
			if (IsInChopZone()) return false;
			if (botTMO == null || CurrentBar < TMOLength || (botTMO.Main[0] > botTMO.Signal[0] && botTMO.Main[0] > TMOOverboughtLevel)) return false;
		    return isFlat && !exitedThisBar && CurrentMode == TradingMode.Auto && priceDown;
		}
		
		protected bool IsBreakoutLongEntryConditionMet()
		{
			if (currentAdx <= AdxThreshold1 || currentAdx >= AdxThreshold2) return false;
		    if (!longSignal || activeOrder) return false;
			if (IsInChopZone()) return false;
			if (botTMO == null || CurrentBar < TMOLength || (botTMO.Main[0] > botTMO.Signal[0] && botTMO.Main[0] < TMOOversoldLevel)) return false;
		    return isFlat && !exitedThisBar && CurrentMode == TradingMode.Auto && priceUp;
		}
		
		protected bool IsBreakoutShortEntryConditionMet()
		{			
			if (currentAdx <= AdxThreshold1 || currentAdx >= AdxThreshold2) return false;
		    if (!shortSignal || activeOrder) return false;
			if (IsInChopZone()) return false;
			if (botTMO == null || CurrentBar < TMOLength || (botTMO.Main[0] > botTMO.Signal[0] && botTMO.Main[0] > TMOOverboughtLevel)) return false;
		    return isFlat && !exitedThisBar && CurrentMode == TradingMode.Auto && priceDown;
		}
		
		#endregion
		
        private bool IsValidTradingDay()
        {
            if (!EnableTradingDaysFilter) return true;
            switch (Time[0].DayOfWeek)
            {
                case DayOfWeek.Monday:    return TradeOnMonday;
                case DayOfWeek.Tuesday:   return TradeOnTuesday;
                case DayOfWeek.Wednesday: return TradeOnWednesday;
                case DayOfWeek.Thursday:  return TradeOnThursday;
                case DayOfWeek.Friday:    return TradeOnFriday;
                default:                  return false;
            }
        }
		
        protected void PrintOnce(string key, string message)
        {
            if (Bars == null || CurrentBar < 0) return;
            if (!printedMessages.TryGetValue(key, out int lastPrintedBar) || lastPrintedBar != CurrentBar) { Print(message); printedMessages[key] = CurrentBar; }
        }		
		
		protected bool checkTimers()
		{
            if (tradingSessions == null) return false;
            TimeSpan currentTime = Times[0][0].TimeOfDay;
            foreach (var session in tradingSessions)
            {
				if (!session.IsEnabled()) continue;
				TimeSpan s = session.Start().TimeOfDay;
				TimeSpan e = session.End().TimeOfDay;
				bool inRange = (s <= e) ? (currentTime >= s && currentTime <= e) : (currentTime >= s || currentTime <= e);
                if (inRange) return true;
            }
            return false;
		}

		protected string GetActiveTimer()
		{
            if (tradingSessions == null) return "No active timer";
            TimeSpan currentTime = Times[0][0].TimeOfDay;
            foreach (var session in tradingSessions)
            {
                if (session.IsEnabled())
				{
					TimeSpan s = session.Start().TimeOfDay; TimeSpan e = session.End().TimeOfDay;
					bool inRange = (s <= e) ? (currentTime >= s && currentTime <= e) : (currentTime >= s || currentTime <= e);
					if (inRange) return $"{session.Start():HH\\:mm} - {session.End():HH\\:mm}";
				}
            }
            return "No active timer";
		}
		
	    protected void InitializeLogger()
	    {
	      if (!EnableLogging || loggerInitialized) return;
	      try
	      {
	          string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
	          LogFilePath = IOPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AlgoTrader", $"AlgoTrader{timestamp}.log");
	          string logDir = IOPath.GetDirectoryName(LogFilePath);
	          if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
	          if (!File.Exists(LogFilePath)) File.WriteAllText(LogFilePath, $"=== AlgoTrader Log Started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
	          loggerInitialized = true;
	      }
	      catch (Exception ex) { Print($"Error initializing logger: {ex.Message}"); }
	    }

	    protected void LogMessage(string message, string level = "INFO")
	    {
	      if (!EnableLogging || !loggerInitialized || (State != State.Realtime && level != "ERROR")) return;
	      try
	      {
	          string log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] Bar {CurrentBar} @ {Time[0]}: {message}";
	          lock (LogLock) { File.AppendAllText(LogFilePath, log + Environment.NewLine); }
	      }
	      catch (Exception ex) { Print($"Error writing to log file: {ex.Message}"); }
	    }
	
	    protected void LogError(string message, Exception ex = null)
	    {
	      if (!EnableLogging) return;
	      string fullMessage = ex != null ? $"{message} Error: {ex.Message}\nStack Trace: {ex.StackTrace}" : message;
	      LogMessage(fullMessage, "ERROR");
	    }
		#endregion
		
		#region PNL, Drawing
		
		private void UpdatePnL()
		{
			if (Account == null && State != State.Historical) return;
			
		    double accountRealized = (State == State.Realtime && Account != null) ? Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) : SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
		    double accountUnrealized = (State == State.Realtime && Account != null) ? Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) : (isFlat ? 0 : Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]));
		    
			dailyPnL = accountRealized - cumPnL + accountUnrealized;
			totalPnL = accountRealized + accountUnrealized;
		
		    if (totalPnL > globalMaxPnL) globalMaxPnL = totalPnL;
            double currentDD = globalMaxPnL - totalPnL;
            if (currentDD > globalMaxDrawdown) globalMaxDrawdown = currentDD;

			if (dailyPnL > dailyMaxProfit) dailyMaxProfit = dailyPnL;
		}

        private void SetDisplayStatus(bool inChopZone)
        {
            if (FilterMode == MasterTrendFilterMode.Disabled && !isTrendOverrideSuggested) a_trendStatus = "Disabled";
			else if (isMasterTrendTemporarilyDisabled) a_trendStatus = "Ranging (Trend Disabled)";
			else if (inChopZone) a_trendStatus = "Choppy";
            else
            {
				if(IsTrendingUp()) a_trendStatus = "Trending Up";
				else if (IsTrendingDown()) a_trendStatus = "Trending Down";
				else a_trendStatus = "Neutral";
            }
			
			a_volatilityStatus = currentVolatility.ToString();
			a_structureStatus = currentMarketStructure.ToString();

            a_signalStatus = "No Signal";
            switch (CurrentMode)
            {
                case TradingMode.Disabled: a_signalStatus = "Manual Mode"; break;
                case TradingMode.DisabledByChop: a_signalStatus = "Auto OFF (Chop)"; break;
                case TradingMode.Auto:
                    if (!checkTimers()) a_signalStatus = "Outside Hours"; 
                    else if (orderErrorOccurred) a_signalStatus = "Order Error!";
                    else if (enableTrailingDrawdown && trailingDrawdownReached) a_signalStatus = "Drawdown Limit Hit";
                    else if (dailyLossProfit && dailyPnL <= -DailyLossLimit) a_signalStatus = "Loss Limit Hit";
                    else if (dailyLossProfit && dailyPnL >= DailyProfitLimit) a_signalStatus = "Profit Limit Hit";
                    else if (!isFlat) a_signalStatus = "In Position";
					else a_signalStatus = "Armed";
                    break;
            }
        }
		
		private void DrawStrategyPnL()
		{
		    double accountNetLiquidation = (Account != null) ? Account.Get(AccountItem.NetLiquidation, Currency.UsDollar) : 0;
		    double realizedPnL = (Account != null) ? Account.Get(AccountItem.RealizedProfitLoss, Currency.UsDollar) : SystemPerformance.AllTrades.TradesPerformance.Currency.CumProfit;
		    double unrealizedPnL = (Account != null) ? Account.Get(AccountItem.UnrealizedProfitLoss, Currency.UsDollar) : (Position.MarketPosition != MarketPosition.Flat ? Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) : 0);
		
			int totalW = SystemPerformance.AllTrades.WinningTrades.Count;
			int totalL = SystemPerformance.AllTrades.LosingTrades.Count;
			int totalTrades = totalW + totalL;
			double winRate = (totalTrades > 0) ? ((double)totalW / totalTrades) : 0;

            double calmar = CalculateCalmar();
			// If it's 0 (meaning negative PnL or < 1 day of data), show "N/A" or "0.00"
			string calmarDisplay = (calmar <= 0) ? "N/A" : calmar.ToString("F2");

		    double currentDrawdown = Math.Max(0, dailyMaxProfit - dailyPnL);
		    double remainingDrawdown = TrailingDrawdown - currentDrawdown;
		    
		    string adxStatus = (currentAdx > AdxThreshold1) ? "Up" : "Choppy";
		    string tmoStatus = (botTMO != null && CurrentBar > TMOLength) ? (botTMO.Main[0] > botTMO.Signal[0] ? "Up" : "Down") : "---";
		    
			string tradeInfoText;
			if (isFlat) tradeInfoText = $"Next SL/TP/BE:\t{InitialStop:F0} / {ProfitTarget:F0} / {BE_Trigger:F0} Ticks\n";
			else tradeInfoText = $"MAE: {Math.Max(0, (highSinceEntry - lowSinceEntry) / TickSize):F0} | MFE: {Math.Max(0, (highSinceEntry - lowSinceEntry) / TickSize):F0}\n" +
								$"Current SL/TP:\t{currentTradeInitialSLTicks:F0} / {currentTradeInitialTPTicks:F0} Ticks\n";
			
            Brush panelColor = (totalPnL >= 0 ? Brushes.Lime : Brushes.Pink);
            if (a_signalStatus == "Outside Hours") panelColor = Brushes.LightGray;

		    string realTimeTradeText =
			$"{(Account?.Name ?? "N/A")}\n" +
		    $"Chart: {calculatedChartTypeDisplay}\n" +
		    $"Account Value:\t{accountNetLiquidation:C}\n" +
		    $"-------------\n" +
		    $"Realized:\t{realizedPnL:C}\n" +
		    $"Unrealized:\t{unrealizedPnL:C}\n" +
		    $"Total PnL:\t{totalPnL:C}\n" +
			$"Calmar Ratio:\t{calmarDisplay}\n" +
		    $"Win Rate:\t{winRate:P1} ({totalW}W / {totalL}L)\n" + 
		    $"-------------\n" +
		    $"Max Profit:\t{dailyMaxProfit:C}\n" +
		    $"Current DD:\t{currentDrawdown:C}\n" +
		    $"Remaining:\t{remainingDrawdown:C}\n" +
		    $"-------------\n" +
		    tradeInfoText +	
            $"RANGE Peak:\tMAE {RangeMaxMAE:F0} | MFE {RangeMaxMFE:F0}\n" +
            $"TREND Peak:\tMAE {TrendMaxMAE:F0} | MFE {TrendMaxMFE:F0}\n" +
		    $"-------------\n" +
		    $"High:\t\t{highOfDay:F2}\n" +				
		    $"Low:\t\t{lowOfDay:F2}\n" +
		    $"Range:\t\t{range:F0} Ticks\n" +
		    $"-------------\n" +
		    $"ADX:\t\t{adxStatus} ({currentAdx:F1})\n" +
		    $"TMO:\t\t{tmoStatus} ({currentTMO:F2})\n" +
		    $"ATR:\t\t{currentAtr:F2}\n" +
		    $"-------------\n" +
			$"Regime:\t\t{currentRegime}\n" +
			$"Master Trend:\t{a_trendStatus}\n" +
		    $"-------------\n" +
		    $"Bot Signal:\t{a_lastSignalSource}\n" +
		    $"Signal:\t\t{a_signalStatus}";
		    
		    Draw.TextFixed(this, "realTimeTradeText", realTimeTradeText, PositionDailyPNL, panelColor, new SimpleFont("Arial", FontSize), null, Brushes.Black, Transparency, DashStyleHelper.Solid, Transparency, false, "");
		}

		protected void ShowPNLStatus() {
			string statusPnlText = $"Active Timer: {GetActiveTimer()}\n" + $"Longs: {counterLong}/{longPerDirection} | Shorts: {counterShort}/{shortPerDirection}\n";
			Draw.TextFixed(this, "statusPnl", statusPnlText, PositionPnl, colorPnl, new SimpleFont("Arial", 16), Brushes.Transparent, Brushes.Transparent, 0);
		}
		
		private void CheckTrailingDrawdown()
		{
			if (enableTrailingDrawdown && !trailingDrawdownReached && (dailyMaxProfit - dailyPnL) >= TrailingDrawdown)
			{
				if (!isFlat) CloseAllOperation("Trailing DD Exit");
				SetTradingMode(TradingMode.Disabled);
				trailingDrawdownReached = true;
			}
		}
		#endregion
		
		#region Virtual Overrides & Child Strategy Hooks
		protected virtual void addDataSeries()       {}
		protected virtual void CheckForCustomSignals() {}
		
        protected virtual bool ValidateExitLong() { return isLong && botTMO != null && CurrentBar > 1 && CrossBelow(botTMO.Main, botTMO.Signal, 1); }
        protected virtual bool ValidateExitShort() { return isShort && botTMO != null && CurrentBar > 1 && CrossAbove(botTMO.Main, botTMO.Signal, 1); }
		#endregion
		
		#region Bot Framework
		
		private void CheckForBotSignals()
		{
		    a_lastSignalSource = "---"; lastEntryConfluenceScore = 0; signalSourceRegime = MarketRegime.Undefined;

            if (!EnableConfluenceScoring)
            {
                if (FindSignal(universalBots)) return;
                if (EnableAutoRegimeDetection)
                {
                    if (EnableTrendBots && currentRegime == MarketRegime.Trending) FindSignal(trendBots);
                    else if (EnableRangeBots && currentRegime == MarketRegime.Ranging) FindSignal(rangeBots);
                    else if (EnableBreakoutBots && currentRegime == MarketRegime.Breakout) FindSignal(breakoutBots);
                }
                else
                {
                    if (EnableTrendBots && FindSignal(trendBots)) return;
                    if (EnableRangeBots && FindSignal(rangeBots)) return;
                    if (EnableBreakoutBots && FindSignal(breakoutBots)) return;
                }
            }
            else
            {
                List<ISignalBot> firedBots = new List<ISignalBot>();
                firedBots.AddRange(FindAllSignals(universalBots));
                if (EnableAutoRegimeDetection)
                {
                    if (EnableTrendBots && currentRegime == MarketRegime.Trending) firedBots.AddRange(FindAllSignals(trendBots));
                    else if (EnableRangeBots && currentRegime == MarketRegime.Ranging) firedBots.AddRange(FindAllSignals(rangeBots));
                    else if (EnableBreakoutBots && currentRegime == MarketRegime.Breakout) firedBots.AddRange(FindAllSignals(breakoutBots));
                }
                else
                {
                    if (EnableTrendBots) firedBots.AddRange(FindAllSignals(trendBots));
                    if (EnableRangeBots) firedBots.AddRange(FindAllSignals(rangeBots));
                    if (EnableBreakoutBots) firedBots.AddRange(FindAllSignals(breakoutBots));
                }
                if (firedBots.Count == 0) return;
                ISignalBot bestBot = null; SignalDirection bestDirection = SignalDirection.NoSignal; double bestScore = -1;
                foreach (var bot in firedBots.Distinct())
                {
                    SignalDirection direction = bot.CheckSignal(0);
                    if (direction != SignalDirection.NoSignal)
                    {
                        double score = CalculateConfluenceScore(direction);
                        if (score > bestScore) { bestScore = score; bestBot = bot; bestDirection = direction; }
                    }
                }
                if (bestBot != null && bestScore >= MinConfluenceScore)
                {
                    a_lastSignalSource = $"{bestBot.Name} ({bestScore:F0})"; lastEntryConfluenceScore = bestScore;
					signalSourceRegime = bestBot.Regime;
                    if (bestDirection == SignalDirection.Long) longSignal = true; else shortSignal = true;
                }
            }
		}
		
        private double CalculateConfluenceScore(SignalDirection direction)
        {
            double score = 0;
            if (direction == SignalDirection.Long && dtpUp) score += 40;
            else if (direction == SignalDirection.Short && dtpDown) score += 40;
			if (CurrentBar < TMOLength) return 0;
            if (direction == SignalDirection.Long && botTMO.Main[0] > botTMO.Signal[0]) score += 30;
            else if (direction == SignalDirection.Short && botTMO.Main[0] < botTMO.Signal[0]) score += 30;
            if (currentAdx > ConfluenceAdxThreshold) score += 20;
            return Math.Max(0, score);
        }
		
		private bool FindSignal(List<ISignalBot> botCollection)
		{
			foreach (var bot in botCollection)
			{
				if (EnableBotPerformanceFilter && botPerformanceTrackers.TryGetValue(bot.Name, out BotPerformanceTracker tracker) && tracker.RecentTrades.Count >= MinTradesForFilter && tracker.WinRate < (MinBotWinRatePercent / 100.0)) continue;
				SignalDirection direction = bot.CheckSignal(0);
				if (direction != SignalDirection.NoSignal && bot.Regime == MarketRegime.Ranging && EnableRangeVetoFilter)
				{
					if (rangeVetoLinReg == null || rangeVetoAtr == null || CurrentBar < RangeVetoPeriod) continue; 
					double upperVetoBand = rangeVetoLinReg[0] + (rangeVetoAtr[0] * RangeVetoBandMultiplier);
					double lowerVetoBand = rangeVetoLinReg[0] - (rangeVetoAtr[0] * RangeVetoBandMultiplier);
					if (direction == SignalDirection.Long && Close[0] > lowerVetoBand) direction = SignalDirection.NoSignal; 
					else if (direction == SignalDirection.Short && Close[0] < upperVetoBand) direction = SignalDirection.NoSignal; 
				}
				if (direction == SignalDirection.Long) { a_lastSignalSource = bot.Name; longSignal = true; signalSourceRegime = bot.Regime; return true; }
				if (direction == SignalDirection.Short) { a_lastSignalSource = bot.Name; shortSignal = true; signalSourceRegime = bot.Regime; return true; }
			}
			return false;
		}

        private List<ISignalBot> FindAllSignals(List<ISignalBot> botCollection)
        {
            List<ISignalBot> firedBots = new List<ISignalBot>();
            foreach (var bot in botCollection)
            {
				if (EnableBotPerformanceFilter && botPerformanceTrackers.TryGetValue(bot.Name, out BotPerformanceTracker tracker) && tracker.RecentTrades.Count >= MinTradesForFilter && tracker.WinRate < (MinBotWinRatePercent / 100.0)) continue; 
				SignalDirection direction = bot.CheckSignal(0);
                if (direction != SignalDirection.NoSignal) firedBots.Add(bot);
            }
            return firedBots;
        }

		#endregion
		
		#region Automatic Direction Control

		private void ManageAutoDirectionButtons()
		{
		    if (!AutoDisableCounterTrend || FilterMode == MasterTrendFilterMode.Disabled) return;
		    bool isCurrentlyTrendingUp = IsTrendingUp(); bool isCurrentlyTrendingDown = IsTrendingDown();
		    if (wasTrendingUpLastBar && !isCurrentlyTrendingUp) isShortManuallyEnabled = false;
		    if (wasTrendingDownLastBar && !isCurrentlyTrendingUp) isLongManuallyEnabled = false;
		    if (isCurrentlyTrendingUp) { isLongEnabled = true; if (!isShortManuallyEnabled) isShortEnabled = false; }
		    else if (isCurrentlyTrendingDown) { isShortEnabled = true; if (!isLongManuallyEnabled) isLongEnabled = false; }
		    else { if (!isLongManuallyEnabled) isLongEnabled = true; if (!isShortManuallyEnabled) isShortEnabled = true; }
		    wasTrendingUpLastBar = isCurrentlyTrendingUp; wasTrendingDownLastBar = isCurrentlyTrendingDown;
		    if (ChartControl != null && State >= State.Historical) ChartControl.Dispatcher.InvokeAsync(() => UpdateDirectionButtonStates());
		}
		
		#endregion
    }
}