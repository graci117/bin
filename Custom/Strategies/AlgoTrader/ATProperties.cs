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
    public abstract partial class AlgoBase : Strategy, ICustomTypeDescriptor
    {
		#region Custom Property Manipulation (ICustomTypeDescriptor)
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection col = TypeDescriptor.GetProperties(GetType(), attributes);
            PropertyDescriptorCollection filteredCol = new PropertyDescriptorCollection(null);

			foreach(PropertyDescriptor d in col) filteredCol.Add(d);
			
			if (OrderSelector == orderSelector.Market)
			{
				filteredCol.Remove(filteredCol.Find("LimitOffset", true));
			}

			if (InitialStopMode == InitialStopCalculationMode.FixedTicks)
			{
				filteredCol.Remove(filteredCol.Find("StopLoss_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("StopLoss_ATR_Mult", true));
				filteredCol.Remove(filteredCol.Find("DTPStopLossBand", true));
				filteredCol.Remove(filteredCol.Find("RangeFilteredStopLossBand", true));
			}
			else if (InitialStopMode == InitialStopCalculationMode.ATR)
			{
				filteredCol.Remove(filteredCol.Find("InitialStop", true));
				filteredCol.Remove(filteredCol.Find("DTPStopLossBand", true));
				filteredCol.Remove(filteredCol.Find("RangeFilteredStopLossBand", true));
			}
			else if (InitialStopMode == InitialStopCalculationMode.DeviationTrendProfile)
			{
				filteredCol.Remove(filteredCol.Find("InitialStop", true));
				filteredCol.Remove(filteredCol.Find("StopLoss_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("StopLoss_ATR_Mult", true));
				filteredCol.Remove(filteredCol.Find("RangeFilteredStopLossBand", true));
			}
			else // RangeFiltered
			{
				filteredCol.Remove(filteredCol.Find("InitialStop", true));
				filteredCol.Remove(filteredCol.Find("StopLoss_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("StopLoss_ATR_Mult", true));
				filteredCol.Remove(filteredCol.Find("DTPStopLossBand", true));
			}

			if (ProfitTargetMode == ProfitTargetCalculationMode.FixedTicks)
			{
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Mult", true));
				filteredCol.Remove(filteredCol.Find("RiskRewardRatio", true));
				filteredCol.Remove(filteredCol.Find("DTPTakeProfitBand", true));
				filteredCol.Remove(filteredCol.Find("RangeFilteredTakeProfitBand", true));
			}
			else if (ProfitTargetMode == ProfitTargetCalculationMode.ATR)
			{
				filteredCol.Remove(filteredCol.Find("ProfitTarget", true));
				filteredCol.Remove(filteredCol.Find("RiskRewardRatio", true));
				filteredCol.Remove(filteredCol.Find("DTPTakeProfitBand", true));
				filteredCol.Remove(filteredCol.Find("RangeFilteredTakeProfitBand", true));
			}
			else if (ProfitTargetMode == ProfitTargetCalculationMode.RiskRewardRatio)
			{
				filteredCol.Remove(filteredCol.Find("ProfitTarget", true));
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Mult", true));
				filteredCol.Remove(filteredCol.Find("DTPTakeProfitBand", true));
				filteredCol.Remove(filteredCol.Find("RangeFilteredTakeProfitBand", true));
			}
			else if (ProfitTargetMode == ProfitTargetCalculationMode.DeviationTrendProfile)
			{
				filteredCol.Remove(filteredCol.Find("ProfitTarget", true));
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Mult", true));
				filteredCol.Remove(filteredCol.Find("RiskRewardRatio", true));
				filteredCol.Remove(filteredCol.Find("RangeFilteredTakeProfitBand", true));
			}
			else // RangeFiltered
			{
				filteredCol.Remove(filteredCol.Find("ProfitTarget", true));
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("ProfitTarget_ATR_Mult", true));
				filteredCol.Remove(filteredCol.Find("RiskRewardRatio", true));
				filteredCol.Remove(filteredCol.Find("DTPTakeProfitBand", true));
			}
			
			if (StopType != StopManagementType.RegularTrail)
			{
				filteredCol.Remove(filteredCol.Find("Trail_Trigger", true));
				filteredCol.Remove(filteredCol.Find("Trail_Size", true));
				filteredCol.Remove(filteredCol.Find("Trail_frequency", true));
			}
			if (StopType != StopManagementType.ATR && StopType != StopManagementType.HybridAtrPsar)
			{
				filteredCol.Remove(filteredCol.Find("TrailStop_ATR_Period", true));
				filteredCol.Remove(filteredCol.Find("TrailStop_ATR_Mult", true));
			}
			if (StopType != StopManagementType.HighLow)
			{
				filteredCol.Remove(filteredCol.Find("HighLowTrailInitialLookback", true));
			}
			if (StopType != StopManagementType.ParabolicSAR && StopType != StopManagementType.HybridAtrPsar)
			{
				filteredCol.Remove(filteredCol.Find("PSARAcceleration", true));
				filteredCol.Remove(filteredCol.Find("PSARAccelerationMax", true));
			}
			if (StopType != StopManagementType.HybridAtrPsar)
			{
				filteredCol.Remove(filteredCol.Find("HybridTrailTriggerPercent", true));
			}
			
			if (!BESetAuto)
			{
				filteredCol.Remove(filteredCol.Find("BreakevenMode", true));
				filteredCol.Remove(filteredCol.Find("DynamicInitialBE", true));
				filteredCol.Remove(filteredCol.Find("DynamicBETargetPercent", true));
				filteredCol.Remove(filteredCol.Find("BreakevenTriggerMode", true));
				filteredCol.Remove(filteredCol.Find("BE_Trigger", true));
				filteredCol.Remove(filteredCol.Find("BETriggerPercent", true));
				filteredCol.Remove(filteredCol.Find("BE_Offset", true));
			}
			else if(BreakevenMode == BreakevenManagementMode.Dynamic)
			{
				filteredCol.Remove(filteredCol.Find("BreakevenTriggerMode", true));
				filteredCol.Remove(filteredCol.Find("BE_Trigger", true));
				filteredCol.Remove(filteredCol.Find("BETriggerPercent", true));
			}
			else // Static Mode
			{
				filteredCol.Remove(filteredCol.Find("DynamicInitialBE", true));
				filteredCol.Remove(filteredCol.Find("DynamicBETargetPercent", true));
				if (BreakevenTriggerMode == BETriggerMode.FixedTicks)
				{
					filteredCol.Remove(filteredCol.Find("BETriggerPercent", true));
				}
				else // Percentage Mode
				{
					filteredCol.Remove(filteredCol.Find("BE_Trigger", true));
				}
			}

			if (ChopDetectionMethod == ChopDetectionMode.RangeFiltered)
			{
				filteredCol.Remove(filteredCol.Find("DtpChopUpperThreshold", true));
				filteredCol.Remove(filteredCol.Find("DtpChopLowerThreshold", true));
			}
			
			if (!EnableRangeVetoFilter)
			{
				filteredCol.Remove(filteredCol.Find("RangeVetoPeriod", true));
				filteredCol.Remove(filteredCol.Find("RangeVetoBandMultiplier", true));
			}
			
			if (!EnableBotPerformanceFilter)
			{
				filteredCol.Remove(filteredCol.Find("BotPerformanceLookback", true));
				filteredCol.Remove(filteredCol.Find("MinTradesForFilter", true));
				filteredCol.Remove(filteredCol.Find("MinBotWinRatePercent", true));
			}
			
			if (!EnableTMOBot)
			{
				filteredCol.Remove(filteredCol.Find("ShowTMO", true));
				filteredCol.Remove(filteredCol.Find("TMOLength", true));
				filteredCol.Remove(filteredCol.Find("TMOCalcLength", true));
				filteredCol.Remove(filteredCol.Find("TMOSmoothLength", true));
			}
			
			if (!EnableOverboughtOversoldFilter)
			{
				filteredCol.Remove(filteredCol.Find("TMOOverboughtLevel", true));
				filteredCol.Remove(filteredCol.Find("TMOOversoldLevel", true));
			}

            if (!EnableScaleOutExecution)
            {
                filteredCol.Remove(filteredCol.Find("ScaleOutLevel1Percent", true));
                filteredCol.Remove(filteredCol.Find("ScaleOutQty1", true));
                filteredCol.Remove(filteredCol.Find("ScaleOutLevel2Percent", true));
                filteredCol.Remove(filteredCol.Find("ScaleOutQty2", true));
            }
			
            if (!EnableTradingDaysFilter)
            {
                filteredCol.Remove(filteredCol.Find("TradeOnMonday", true)); filteredCol.Remove(filteredCol.Find("TradeOnTuesday", true)); 
				filteredCol.Remove(filteredCol.Find("TradeOnWednesday", true)); filteredCol.Remove(filteredCol.Find("TradeOnThursday", true)); 
				filteredCol.Remove(filteredCol.Find("TradeOnFriday", true));
            }
			if (!TradesPerDirection) { filteredCol.Remove(filteredCol.Find("longPerDirection", true)); filteredCol.Remove(filteredCol.Find("shortPerDirection", true)); }
			if (!Time2) { filteredCol.Remove(filteredCol.Find("Start2", true)); filteredCol.Remove(filteredCol.Find("End2", true)); }
			if (!Time3) { filteredCol.Remove(filteredCol.Find("Start3", true)); filteredCol.Remove(filteredCol.Find("End3", true)); }
			if (!Time4) { filteredCol.Remove(filteredCol.Find("Start4", true)); filteredCol.Remove(filteredCol.Find("End4", true)); }
			if (!Time5) { filteredCol.Remove(filteredCol.Find("Start5", true)); filteredCol.Remove(filteredCol.Find("End5", true)); }
			if (!Time6) { filteredCol.Remove(filteredCol.Find("Start6", true)); filteredCol.Remove(filteredCol.Find("End6", true)); }
			
			if (StopMode == TradeManagementMode.Static)
			{
			    filteredCol.Remove(filteredCol.Find("DynamicInitialSL", true));
			    filteredCol.Remove(filteredCol.Find("DynamicSLPadding", true)); filteredCol.Remove(filteredCol.Find("DynamicAvgLookback", true));
				filteredCol.Remove(filteredCol.Find("DynamicBurnInTrades", true)); filteredCol.Remove(filteredCol.Find("DynamicRiskMode", true));
				filteredCol.Remove(filteredCol.Find("DynamicRiskPercentile", true));
			}
			if (TargetMode == TradeManagementMode.Static)
			{
				filteredCol.Remove(filteredCol.Find("DynamicInitialTP", true));
			}
			if (DynamicRiskMode != DynamicCalculationMode.Percentile)
			{
				filteredCol.Remove(filteredCol.Find("DynamicRiskPercentile", true));
			}
			if (!EnableDynamicSizing)
			{
				filteredCol.Remove(filteredCol.Find("RiskPerTradePercent", true));
			}
			
			if (FilterMode != MasterTrendFilterMode.RangeFiltered)
			{
				filteredCol.Remove(filteredCol.Find("ShowRangeFilteredPlot", true));
				filteredCol.Remove(filteredCol.Find("KalmanAlpha", true));
				filteredCol.Remove(filteredCol.Find("KalmanBeta", true));
				filteredCol.Remove(filteredCol.Find("KalmanPeriod", true));
				filteredCol.Remove(filteredCol.Find("DevMultiplier", true));
				filteredCol.Remove(filteredCol.Find("SupertrendFactor", true));
				filteredCol.Remove(filteredCol.Find("SupertrendAtrPeriod", true));
//				filteredCol.Remove(filteredCol.Find("RftsAtrMult5", true));
//				filteredCol.Remove(filteredCol.Find("RftsAtrMult6", true));
			}			
			if (FilterMode != MasterTrendFilterMode.DeviationTrendProfile)
			{
				filteredCol.Remove(filteredCol.Find("ShowDTPPlot", true));
				filteredCol.Remove(filteredCol.Find("DTPLength", true));
//				filteredCol.Remove(filteredCol.Find("DTPAtrLength", true));
				filteredCol.Remove(filteredCol.Find("DTPVolatilityPeriod", true));
				filteredCol.Remove(filteredCol.Find("DTPNormalizationLookback", true));
				filteredCol.Remove(filteredCol.Find("DTPAverageType", true));
				filteredCol.Remove(filteredCol.Find("DTPMult1", true));
				filteredCol.Remove(filteredCol.Find("DTPMult2", true));
				filteredCol.Remove(filteredCol.Find("DTPMult3", true));
				filteredCol.Remove(filteredCol.Find("DTPMult4", true));
			}
            if (!EnableLinRegBandsBot)
			{
			    filteredCol.Remove(filteredCol.Find("LinRegPeriod", true));
			}
            return filteredCol;
        }

        public AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(GetType()); }
        public string GetClassName() { return TypeDescriptor.GetClassName(GetType()); }
        public string GetComponentName() { return TypeDescriptor.GetComponentName(GetType()); }
        public TypeConverter GetConverter() { return TypeDescriptor.GetConverter(GetType()); }
        public EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(GetType()); }
        public PropertyDescriptor GetDefaultProperty() { return TypeDescriptor.GetDefaultProperty(GetType()); }
        public object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(GetType(), editorBaseType); }
        public EventDescriptorCollection GetEvents(Attribute[] attributes) { return TypeDescriptor.GetEvents(GetType(), attributes); }
        public EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(GetType()); }
        public PropertyDescriptorCollection GetProperties() { return GetProperties(null); }
        public object GetPropertyOwner(PropertyDescriptor pd) { return this; }
		#endregion	
	
		#region 01. Core Strategy Settings
		[NinjaScriptProperty, Display(Name = "Contracts (Default)", Order = 0, GroupName = "01. Core Strategy Settings", Description="Default number of contracts to trade. Can be overridden by Dynamic Sizing.")]
		[Range(1, int.MaxValue)]
		public int Contracts { get; set; }	
		
		[NinjaScriptProperty, Display(Name = "Master Trend Filter", Order = 1, GroupName = "01. Core Strategy Settings", Description="Selects the master trend filter logic to apply.")]
        [RefreshProperties(RefreshProperties.All)] 
		public MasterTrendFilterMode FilterMode { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Enable Auto Regime Detection", Order = 2, GroupName = "01. Core Strategy Settings", Description="If true, the strategy will attempt to automatically classify the market state (Trending, Ranging, Breakout).")]
        public bool EnableAutoRegimeDetection { get; set; }

        [NinjaScriptProperty, Display(Name = "Manual Regime Override", Order = 3, GroupName = "01. Core Strategy Settings", Description="Force a specific regime to trade, ignoring the auto-detector. Set to 'Undefined' to use auto-detection.")]
        public MarketRegime ManualRegimeOverride { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable Trend Bots (Master)", GroupName = "01. Core Strategy Settings", Order = 4)] public bool EnableTrendBots { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable Range Bots (Master)", GroupName = "01. Core Strategy Settings", Order = 5)] public bool EnableRangeBots { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable Breakout Bots (Master)", GroupName = "01. Core Strategy Settings", Order = 6)] public bool EnableBreakoutBots { get; set; }
		[NinjaScriptProperty, Display(Name = "Auto-Disable Counter-Trend", Order = 7, GroupName = "01. Core Strategy Settings", Description="If true, automatically disables the SHORT button in an uptrend and the LONG button in a downtrend.")]
		public bool AutoDisableCounterTrend { get; set; }
		#endregion

		#region 01a. Master Filter (RangeFiltered)
		[NinjaScriptProperty, Display(Name = "Show RangeFiltered Plot", GroupName = "01a. Master Filter (RangeFiltered)", Order = 1)] public bool ShowRangeFilteredPlot { get; set; }
		[NinjaScriptProperty, Range(0.001, 1), Display(Name="Kalman Alpha", Order=2, GroupName="01a. Master Filter (RangeFiltered)")] public double KalmanAlpha { get; set; }
		[NinjaScriptProperty, Range(0.001, 1), Display(Name="Kalman Beta", Order=3, GroupName="01a. Master Filter (RangeFiltered)")] public double KalmanBeta { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="Kalman Period", Order=4, GroupName="01a. Master Filter (RangeFiltered)")] public int KalmanPeriod { get; set; }
		[NinjaScriptProperty, Range(0.1, 10), Display(Name="Deviation Multiplier", Order=5, GroupName="01a. Master Filter (RangeFiltered)")] public double DevMultiplier { get; set; }
		[NinjaScriptProperty, Range(0.1, 10), Display(Name="Supertrend Factor", Order=6, GroupName="01a. Master Filter (RangeFiltered)")] public double SupertrendFactor { get; set; }
		[NinjaScriptProperty, Range(1, 200), Display(Name="Supertrend ATR Peiord", Order=7, GroupName="01a. Master Filter (RangeFiltered)")] public int SupertrendAtrPeriod { get; set; }
		#endregion
		
		#region 01b. Master Filter (DTP)
		[NinjaScriptProperty, Display(Name = "Show DTP Plot", GroupName = "01b. Master Filter (DTP)", Order = 1)] public bool ShowDTPPlot { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "Length", GroupName = "01b. Master Filter (DTP)", Order = 2)] public int DTPLength { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "VMA Volatility Period", GroupName = "01b. Master Filter (DTP)", Order = 4)] public int DTPVolatilityPeriod { get; set; }
		[NinjaScriptProperty, Range(1, 250), Display(Name="Normalization Lookback", Order=5, GroupName="01b. Master Filter (DTP)")] public int DTPNormalizationLookback { get; set; }
		[NinjaScriptProperty, Display(Name = "Average Type", GroupName = "01b. Master Filter (DTP)", Order = 6)] public MovingAverageType DTPAverageType { get; set; }
		#endregion
		
		#region 01c. Adaptive Filtering
		[NinjaScriptProperty, Display(Name="Enable Bot Performance Filter", GroupName="01c. Adaptive Filtering", Order=0, Description="If true, signals from bots with poor recent performance will be temporarily ignored.")]
		[RefreshProperties(RefreshProperties.All)]
		public bool EnableBotPerformanceFilter { get; set; }
		
		[NinjaScriptProperty, Range(5, 100), Display(Name="Bot Performance Lookback", GroupName="01c. Adaptive Filtering", Order=1, Description="The number of recent trades to track for each bot to determine its win rate.")]
		public int BotPerformanceLookback { get; set; }
		
		[NinjaScriptProperty, Range(1, 20), Display(Name="Min Trades for Filter", GroupName="01c. Adaptive Filtering", Order=2, Description="A bot will not be filtered until it has taken at least this many trades.")]
		public int MinTradesForFilter { get; set; }
		
		[NinjaScriptProperty, Range(0, 100), Display(Name="Min Bot Win Rate (%)", GroupName="01c. Adaptive Filtering", Order=3, Description="The win rate threshold. Bots performing below this will be disabled.")]
		public double MinBotWinRatePercent { get; set; }
		#endregion

		#region 02. Position Sizing & Risk
		[NinjaScriptProperty, RefreshProperties(RefreshProperties.All), Display(Name = "Enable Dynamic Sizing", GroupName = "02. Position Sizing & Risk", Order = 1, Description = "Automatically calculate position size based on account risk.")]
		public bool EnableDynamicSizing { get; set; }
		
		[NinjaScriptProperty, Display(Name = "Risk Per Trade (%)", GroupName = "02. Position Sizing & Risk", Order = 2, Description = "The percentage of account net liquidation to risk on each trade."), Range(0.1, 100.0)]
		public double RiskPerTradePercent { get; set; }
		
		[NinjaScriptProperty, Display(Name = "Enable Daily P/L Limit", Order = 3, GroupName = "02. Position Sizing & Risk"), RefreshProperties(RefreshProperties.All)]
		public bool dailyLossProfit { get; set; }
		
		[NinjaScriptProperty, Range(0, double.MaxValue), Display(Name="Daily Profit Limit ($)", Order=4, GroupName="02. Position Sizing & Risk")]
		public double DailyProfitLimit { get; set; }
		
		[NinjaScriptProperty, Range(0, double.MaxValue), Display(Name="Daily Loss Limit ($)", Order=5, GroupName="02. Position Sizing & Risk")]
		public double DailyLossLimit { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable Trailing Drawdown", Order = 6, GroupName = "02. Position Sizing & Risk", Description="Enables a trailing drawdown based on the peak profit reached during the session.")]
		public bool enableTrailingDrawdown { get; set; }
		
		[NinjaScriptProperty, Range(0, double.MaxValue), Display(Name="Trailing Drawdown ($)", Order=7, GroupName="02. Position Sizing & Risk", Description="The maximum allowed drawdown from the session's peak PnL. If hit, auto-trading is disabled.")]
		public double TrailingDrawdown { get; set; }
		#endregion

		#region 03. Stop & Target Strategy
		[NinjaScriptProperty, Display(Name="Initial Stop Mode", Order=1, GroupName="03a. Initial Stop Loss", Description="How the initial stop loss value is determined.")]
        [RefreshProperties(RefreshProperties.All)]
		public InitialStopCalculationMode InitialStopMode
        { 
			get { return initialStopMode; } 
			set { initialStopMode = value; }
		}				
		[NinjaScriptProperty, Display(Name="Initial Stop (Ticks)", Order=2, GroupName="03a. Initial Stop Loss")]
		[Range(1, double.MaxValue)]
		public double InitialStop { get; set; }
		
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="StopLoss ATR Period", Order=3, GroupName="03a. Initial Stop Loss")]
		public int StopLoss_ATR_Period { get; set; }
		
		[NinjaScriptProperty, Range(0.1, double.MaxValue), Display(Name="StopLoss ATR Multiplier", Order=4, GroupName="03a. Initial Stop Loss")]
		public double StopLoss_ATR_Mult { get; set; }

		[NinjaScriptProperty, Range(1, 4), Display(Name = "DTP Stop Band", Order = 6, GroupName = "03a. Initial Stop Loss", Description = "The DTP Band (1-4) to set the stop loss.")]
		public int DTPStopLossBand { get; set; }
		
		[NinjaScriptProperty, Range(1, 4), Display(Name = "RangeFiltered Stop Band", Order = 7, GroupName = "03a. Initial Stop Loss", Description = "The RangeFiltered ATR Band (1-4) to set the stop loss.")]
		public int RangeFilteredStopLossBand { get; set; }

		[NinjaScriptProperty, RefreshProperties(RefreshProperties.All), Display(Name="Enable Auto Exit", Order=8, GroupName="03a. Auto Exit")]	
		public bool EnableAutoExit { get; set; }
		
		[NinjaScriptProperty, Display(Name = "Profit Target Mode", Order = 1, GroupName = "03b. Profit Target", Description="How the profit target value is determined.")]
        [RefreshProperties(RefreshProperties.All)]
		public ProfitTargetCalculationMode ProfitTargetMode
        { 
			get { return profitTargetMode; } 
			set { profitTargetMode = value; }
		}
		[NinjaScriptProperty, Display(Name="Profit Target (Ticks)", Order=2, GroupName="03b. Profit Target")]
		[Range(1, double.MaxValue)]
		public double ProfitTarget { get; set; }			
		
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="ProfitTarget ATR Period", Order=3, GroupName="03b. Profit Target")]
		public int ProfitTarget_ATR_Period { get; set; }
		
		[NinjaScriptProperty, Range(0.1, double.MaxValue), Display(Name="ProfitTarget ATR Multiplier", Order=4, GroupName="03b. Profit Target")]
		public double ProfitTarget_ATR_Mult { get; set; }
		
		[NinjaScriptProperty, Display(Name="Risk/Reward Ratio", Order= 5, GroupName="03b. Profit Target", Description="Profit target will be this multiple of the initial stop loss value.")] 
		[Range(0.1, double.MaxValue)]
		public double RiskRewardRatio { get; set; }
		
		[NinjaScriptProperty, Range(1, 4), Display(Name = "DTP Target Band", Order = 7, GroupName = "03b. Profit Target", Description = "The DTP Band (1-4) to set the profit target.")]
		public int DTPTakeProfitBand { get; set; }
		
		[NinjaScriptProperty, Range(1, 4), Display(Name = "RangeFiltered Target Band", Order = 8, GroupName = "03b. Profit Target", Description = "The RangeFiltered ATR Band (1-4) to set the profit target.")]
		public int RangeFilteredTakeProfitBand { get; set; }
		
		[NinjaScriptProperty, RefreshProperties(RefreshProperties.All), Display(Name="Enable Auto Breakeven", Order=1, GroupName="03c. Breakeven")]	
		public bool BESetAuto
		{	get{ return beSetAuto; } set { beSetAuto = value; } }
		
		[NinjaScriptProperty, Display(Name="Breakeven Mode", Order=2, GroupName="03c. Breakeven"), RefreshProperties(RefreshProperties.All)]
		public BreakevenManagementMode BreakevenMode { get; set; }
		
		[NinjaScriptProperty, Display(Name = "Dynamic: Initial BE Ticks (Fallback)", Description="The Breakeven Trigger to use until the system has learned a value.", GroupName = "03c. Breakeven", Order = 3)]
		public double DynamicInitialBE { get; set; }
		
		[NinjaScriptProperty, Range(1, 200), Display(Name="Dynamic: BE Target Percent", GroupName="03c. Breakeven", Order=4, Description="Sets the dynamic BE trigger as a percentage of the calculated MFE of losers. e.g., 80%")]
		public double DynamicBETargetPercent { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Breakeven Trigger Mode", Order = 2, GroupName = "03c. Breakeven"), RefreshProperties(RefreshProperties.All)]
        public BETriggerMode BreakevenTriggerMode { get; set; }

		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="BE Trigger (Ticks)", Order=3, Description="Profit in Ticks required to move stop to breakeven.", GroupName="03c. Breakeven")]
		public int BE_Trigger { get; set; }

		[NinjaScriptProperty, Range(1, 100), Display(Name="BE Trigger (% of Profit Target)", Order=4, Description="Percentage of profit target reached to trigger breakeven.", GroupName="03c. Breakeven")]
		public int BETriggerPercent { get; set; }

		[NinjaScriptProperty, Display(Name="BE Offset (Ticks)", Order=5, Description="Offset from entry price for the breakeven stop.", GroupName="03c. Breakeven")]
		public int BE_Offset { get; set; }		

        [NinjaScriptProperty, Display(Name = "In-Trade Stop Management", Description = "The stop-loss trailing methodology to use once in a trade.", Order = 1, GroupName = "03d. Trailing Stop"), RefreshProperties(RefreshProperties.All)]
        public StopManagementType StopType { get; set; }
		
		[NinjaScriptProperty, Display(Name="Trail Trigger (Ticks)", Order=2, GroupName="03d. Trailing Stop", Description="Profit needed to activate the trailing stop.")]
		public double Trail_Trigger	{ get; set; }
		
		[NinjaScriptProperty, Display(Name="Regular Trail: Trail Size (Ticks)", Order=3, GroupName="03d. Trailing Stop", Description="Distance the stop will trail behind price.")]
		public int Trail_Size { get; set; }
		
		[NinjaScriptProperty, Display(Name="Regular Trail: Frequency (Ticks)", Order=4, GroupName="03d. Trailing Stop", Description="How often (in ticks of profit) the stop will be moved.")]
		public int Trail_frequency { get; set; }
		
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="ATR Trail: Period", Order=5, GroupName="03d. Trailing Stop")]
		public int TrailStop_ATR_Period { get; set; }
		
		[NinjaScriptProperty, Range(1, double.MaxValue), Display(Name="ATR Trail: Multiplier", Order=6, GroupName="03d. Trailing Stop")]
		public double TrailStop_ATR_Mult { get; set; }
		
		[NinjaScriptProperty, Display(Name = "ATR Trail: Show Plot", Order = 7, GroupName = "03d. Trailing Stop")] 
		public bool ShowAtrTrailPlot { get; set; }
		
        [NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "High/Low Trail: Lookback", Order = 8, GroupName = "03d. Trailing Stop")]
        public int HighLowTrailInitialLookback { get; set; }
		
		[NinjaScriptProperty, Display(Name = "Show Parabolic SAR", Order = 9, GroupName = "03d. Trailing Stop")] public bool ShowPSARPlot { get; set; }
		
        [NinjaScriptProperty, Range(0.001, 1.0), Display(Name = "Parabolic Trail: Acceleration", GroupName = "03d. Trailing Stop", Order = 10)]
        public double PSARAcceleration { get; set; }
		
        [NinjaScriptProperty, Range(0.01, 1.0), Display(Name = "Parabolic Trail: Accel. Max", GroupName = "03d. Trailing Stop", Order = 11)]
        public double PSARAccelerationMax { get; set; }

		[NinjaScriptProperty, Range(1, 99), Display(Name = "Hybrid Trail Trigger (%)", Order = 12, GroupName = "03d. Trailing Stop", Description = "The percentage of the profit target that must be reached to switch from ATR to PSAR trail.")]
		public int HybridTrailTriggerPercent { get; set; }
		
		[NinjaScriptProperty, Range(1, 99), Display(Name = "Staged Trail: Start Trigger (%)", Order = 13, GroupName = "03d. Trailing Stop", Description = "The percentage of the profit target that must be reached to activate the Staged Trail.")]
		public int StagedTrailTriggerPercent { get; set; }

		[NinjaScriptProperty, Display(Name = "Staged Trail: Initial Distance (Ticks)", Order = 14, GroupName = "03d. Trailing Stop", Description = "The initial trailing distance in ticks, active after breakeven is reached.")]
		public int InitialTrailTicks { get; set; }
		
		[NinjaScriptProperty, Range(1, 99), Display(Name = "Staged Trail: Final Trigger (%)", Order = 15, GroupName = "03d. Trailing Stop", Description = "The percentage of the profit target that must be reached to activate the Final Stage Trail.")]
		public int FinalTrailTriggerPercent { get; set; }
		
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "Staged Trail: Final Distance (Ticks)", Order = 16, GroupName = "03d. Trailing Stop", Description = "The tight trailing distance in ticks to use after the trigger is met.")]
		public int FinalTrailTicks { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Manual Move Stop Lookback", Order = 17, GroupName = "03d. Trailing Stop", Description = "The lookback period for the 'Move Trailstop' UI button.")]
        public int ManualMoveStopLookback { get; set; }
		#endregion

		#region 04. Order Execution
		[NinjaScriptProperty, Display(Name = "Order Entry Type", Description= "Select order type to enter the market.", Order = 0, GroupName = "04. Order Execution")]
		[RefreshProperties(RefreshProperties.All)]
		public orderSelector OrderSelector
		{
			get { return showOrder; }
			set { showOrder = value; isBuySellMarketOrder = (showOrder == orderSelector.Market); }			
		}

		[NinjaScriptProperty, Range(0, int.MaxValue), Display(Name="Limit Order Offset (Ticks)", Description="Distance to place Limit Orders away from the market.", Order=1, GroupName="04. Order Execution")]
		public int LimitOffset { get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty, Display(Name="Max Contracts Per Trade", Order=2, GroupName="04. Order Execution", Description= "Max allowed contracts for a single trade.")]
		public int MaxPositionPerTrade { get; set; }
		#endregion
		
		#region 05. Chop Detection
		[NinjaScriptProperty, Display(Name = "Chop Detection Method", Order = 1, GroupName = "05. Chop Detection", Description="Selects the algorithm used to detect and avoid choppy market conditions.")] 
		[RefreshProperties(RefreshProperties.All)]
		public ChopDetectionMode ChopDetectionMethod { get; set; } 
		
		[NinjaScriptProperty, Range(0.0, 1.0), Display(Name = "DTP Chop: Upper Threshold", Order = 7, GroupName = "05. Chop Detection", Description = "The upper bound of the normalized slope for DTP chop detection.")]
		public double DtpChopUpperThreshold { get; set; }
		[NinjaScriptProperty, Range(0.0, 1.0), Display(Name = "DTP Chop: Lower Threshold", Order = 8, GroupName = "05. Chop Detection", Description = "The lower bound of the normalized slope for DTP chop detection.")]
		public double DtpChopLowerThreshold { get; set; }
		
		[NinjaScriptProperty, Range(1, 200), Display(Name = "Volatility: ATR Period", Order = 9, GroupName = "05. Chop Detection", Description = "The lookback period for the short-term ATR used to measure current volatility.")]
        public int VolatilityPeriod { get; set; }
		[NinjaScriptProperty, Range(10, 1000), Display(Name = "Volatility: Long-Term Lookback", Order = 10, GroupName = "05. Chop Detection", Description = "The lookback for the moving average of ATR, used to establish a 'normal' volatility baseline.")]
        public int VolatilityLongTermLookback { get; set; }
		[NinjaScriptProperty, Range(1, 100), Display(Name = "Volatility: Threshold (%)", Order = 11, GroupName = "05. Chop Detection", Description = "Defines the percentage band around the long-term average to classify volatility as High or Low.")]
        public double VolatilityThresholdPercent { get; set; }
		#endregion
		
		#region 05a. Market Condition Filters - TMO
		[NinjaScriptProperty, Display(Name = "Show TMO Plot", GroupName = "05a. Market Condition Filters - TMO", Order = 1)] 
		public bool ShowTMO { get; set; }
		
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="Lookback Length", Description="Main lookback period for the momentum calculation.", Order=2, GroupName="05a. Market Condition Filters - TMO")]
		public int TMOLength { get; set; }

		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="Calculation Length", Description="Period for the first EMA smoothing.", Order=3, GroupName="05a. Market Condition Filters - TMO")]
		public int TMOCalcLength { get; set; }

		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="Smoothing Length", Description="Period for the second (Main) and third (Signal) EMA smoothing.", Order=4, GroupName="05a. Market Condition Filters - TMO")]
		public int TMOSmoothLength { get; set; }
		
		[NinjaScriptProperty, Display(Name = "Enable OB/OS Filter (TMO)", Order = 5, GroupName = "05a. Market Condition Filters - TMO", Description = "If true, prevents long range entries when TMO is Overbought and short when Oversold.")]
        [RefreshProperties(RefreshProperties.All)]
		public bool EnableOverboughtOversoldFilter { get; set; }
        
        [NinjaScriptProperty, Display(Name = "TMO Overbought Level", Order = 6, GroupName = "05a. Market Condition Filters - TMO", Description = "The positive TMO value above which the market is considered overbought for filtering.")]
        public double TMOOverboughtLevel { get; set; }
        
        [NinjaScriptProperty, Display(Name = "TMO Oversold Level", Order = 7, GroupName = "05a. Market Condition Filters - TMO", Description = "The negative TMO value below which the market is considered overbought for filtering.")]
        public double TMOOversoldLevel { get; set; }			
		#endregion
		
		#region 05c. Market Condition Filters - Bollinger
        [NinjaScriptProperty, Display(Name = "Enable Exhaustion Filter (BBands)", Order = 12, GroupName = "05c. Market Condition Filters - Bollinger", Description = "If true, prevents long entries when price is above the upper Bollinger Band and short entries when price is below the lower Bollinger Band.")]
        public bool EnableExhaustionFilter { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Exhaustion BB Plot", Order = 13, GroupName = "05c. Market Condition Filters - Bollinger")] public bool ShowExhaustionBB { get; set; }
        [NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "Exhaustion BB Period", Order = 14, GroupName = "05c. Market Condition Filters - Bollinger")]
        public int ExhaustionBBPeriod { get; set; }
        [NinjaScriptProperty, Range(0.1, 5), Display(Name = "Exhaustion BB StdDev", Order = 15, GroupName = "05c. Market Condition Filters - Bollinger")]
        public double ExhaustionBBStdDev { get; set; }
		#endregion
        
		#region 05d. Range Veto Filter
		[NinjaScriptProperty, Display(Name="Enable Range Veto Filter", GroupName="05d. Range Veto Filter", Order=0, Description="If true, uses a Regression/ATR channel to veto ranging bot signals that are not at statistical extremes.")]
		[RefreshProperties(RefreshProperties.All)]
		public bool EnableRangeVetoFilter { get; set; }
		
		[NinjaScriptProperty, Range(5, 200), Display(Name="Veto: Regression Period", GroupName="05d. Range Veto Filter", Order=1)]
		public int RangeVetoPeriod { get; set; }
		
		[NinjaScriptProperty, Range(0.5, 6), Display(Name="Veto: ATR Band Multiplier", GroupName="05d. Range Veto Filter", Order=3, Description="The ATR multiplier to define the extreme band. Shorts are vetoed below this band, longs are vetoed above it.")]
		public double RangeVetoBandMultiplier { get; set; }
		#endregion
		
		#region 06. Session & Time Controls
        [NinjaScriptProperty, RefreshProperties(RefreshProperties.All), Display(Name = "Enable Trading Days Filter", GroupName = "06. Session & Time Controls", Order = 1)] public bool EnableTradingDaysFilter { get; set; }
        [NinjaScriptProperty, Display(Name = "Trade on Monday", GroupName = "06. Session & Time Controls", Order = 2)] public bool TradeOnMonday { get; set; }
        [NinjaScriptProperty, Display(Name = "Trade on Tuesday", GroupName = "06. Session & Time Controls", Order = 3)] public bool TradeOnTuesday { get; set; }
        [NinjaScriptProperty, Display(Name = "Trade on Wednesday", GroupName = "06. Session & Time Controls", Order = 4)] public bool TradeOnWednesday { get; set; }
        [NinjaScriptProperty, Display(Name = "Trade on Thursday", GroupName = "06. Session & Time Controls", Order = 5)] public bool TradeOnThursday { get; set; }
        [NinjaScriptProperty, Display(Name = "Trade on Friday", GroupName = "06. Session & Time Controls", Order = 6)] public bool TradeOnFriday { get; set; }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="Start Trades", Order=7, GroupName="06. Session & Time Controls")]
		public DateTime Start { get; set; }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="End Trades", Order=8, GroupName="06. Session & Time Controls")]
		public DateTime End { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable Time 2", Description = "Enable 2 times.", Order=9, GroupName = "06. Session & Time Controls"), RefreshProperties(RefreshProperties.All)]
		public bool Time2 { get{return isEnableTime2;} set{isEnableTime2 = (value);} }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="Start Time 2", Order=10, GroupName="06. Session & Time Controls")]
		public DateTime Start2 { get; set; }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="End Time 2", Order=11, GroupName="06. Session & Time Controls")]
		public DateTime End2 { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable Time 3", Description = "Enable 3 times.", Order=12, GroupName = "06. Session & Time Controls"), RefreshProperties(RefreshProperties.All)]
		public bool Time3 { get{return isEnableTime3;} set{isEnableTime3 = (value);} }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="Start Time 3", Order=13, GroupName="06. Session & Time Controls")]
		public DateTime Start3 { get; set; }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="End Time 3", Order=14, GroupName="06. Session & Time Controls")]
		public DateTime End3 { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable Time 4", Description = "Enable 4 times.", Order=15, GroupName = "06. Session & Time Controls"), RefreshProperties(RefreshProperties.All)]
		public bool Time4 { get{return isEnableTime4;} set{isEnableTime4 = (value);} }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="Start Time 4", Order=16, GroupName="06. Session & Time Controls")]
		public DateTime Start4 { get; set; }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="End Time 4", Order=17, GroupName="06. Session & Time Controls")]
		public DateTime End4 { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable Time 5", Description = "Enable 5 times.", Order=18, GroupName = "06. Session & Time Controls"), RefreshProperties(RefreshProperties.All)]
		public bool Time5 { get{return isEnableTime5;} set{isEnableTime5 = (value);} }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="Start Time 5", Order=19, GroupName="06. Session & Time Controls")]
		public DateTime Start5 { get; set; }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="End Time 5", Order=20, GroupName="06. Session & Time Controls")]
		public DateTime End5 { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable Time 6", Description = "Enable 6 times.", Order =21, GroupName = "06. Session & Time Controls"), RefreshProperties(RefreshProperties.All)]
		public bool Time6 { get{return isEnableTime6;} set{isEnableTime6 = (value);} }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="Start Time 6", Order=22, GroupName="06. Session & Time Controls")]
		public DateTime Start6 { get; set; }
		[NinjaScriptProperty, PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey"), Display(Name="End Time 6", Order=23, GroupName="06. Session & Time Controls")]
		public DateTime End6 { get; set; }
		[NinjaScriptProperty, RefreshProperties(RefreshProperties.All), Display(Name = "Enable Trades Per Direction", Order = 24, GroupName = "06. Session & Time Controls")] public bool TradesPerDirection { get; set; }
		[NinjaScriptProperty, Display(Name="Longs Per Direction", Order = 25, GroupName = "06. Session & Time Controls")] public int longPerDirection { get; set; }
		[NinjaScriptProperty, Display(Name="Shorts Per Direction", Order = 26, GroupName = "06. Session & Time Controls")] public int shortPerDirection { get; set; }
		#endregion

		#region 07. Core Indicator & Filter Settings
		[NinjaScriptProperty, Display(Name = "Enable Confluence Scoring", Order = 1, GroupName = "07. Core Indicator & Filter Settings", Description = "If true, it finds all signals on a bar and scores them based on market conditions to find the highest-probability trade.")]
        [RefreshProperties(RefreshProperties.All)] public bool EnableConfluenceScoring { get; set; }
        [NinjaScriptProperty, Range(0, 100), Display(Name = "Min Confluence Score", Order = 2, GroupName = "07. Core Indicator & Filter Settings", Description = "The minimum score (0-100) a signal must achieve to be considered for an entry.")]
        public int MinConfluenceScore { get; set; }
        [NinjaScriptProperty, Range(10, 50), Display(Name = "Confluence ADX Threshold", Order = 3, GroupName = "07. Core Indicator & Filter Settings", Description = "The ADX value used by the scoring engine to determine if the market is 'trending' vs 'choppy'.")]
        public int ConfluenceAdxThreshold { get; set; }
		[NinjaScriptProperty, Display(Name = "Enable DM", Order = 8, GroupName = "07. Core Indicator & Filter Settings")]
		public bool EnableDM { get; set; }
		[NinjaScriptProperty, Display(Name = "Show DM Plot", GroupName = "07. Core Indicator & Filter Settings", Order = 9)] public bool ShowDM { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "DM Period", Order = 10, GroupName = "07. Core Indicator & Filter Settings")]
		public int DmPeriod { get; set; }
		[NinjaScriptProperty, Range(1, double.MaxValue), Display(Name = "ADX Trend Threshold 1", Order = 11, GroupName = "07. Core Indicator & Filter Settings", Description="The ADX value above which the market is considered to be trending.")]
		public double AdxThreshold1 { get; set; }
		[NinjaScriptProperty, Range(1, double.MaxValue), Display(Name = "ADX Trend Threshold 2", Order = 12, GroupName = "07. Core Indicator & Filter Settings", Description="The ADX value above which the market is considered to be trending.")]
		public double AdxThreshold2 { get; set; }
		[NinjaScriptProperty, Display(Name = "Regime ADX Period", Order = 13, GroupName = "07. Core Indicator & Filter Settings")]
        public int RegimeAdxPeriod { get; set; }
        [NinjaScriptProperty, Display(Name = "Regime ADX Min Trend Threshold", Order = 14, GroupName = "07. Core Indicator & Filter Settings", Description="ADX value BELOW which the market is considered TRENDING.")]
        public int RegimeAdxTrendThreshold { get; set; }
        [NinjaScriptProperty, Display(Name = "Regime ADX Max Trend Threshold", Order = 15, GroupName = "07. Core Indicator & Filter Settings", Description="ADX value BELOW which the trade is likely profitable.")]
        public int RegimeAdxTrendThreshold2 { get; set; }
        [NinjaScriptProperty, Display(Name = "Regime ADX Range Threshold", Order = 16, GroupName = "07. Core Indicator & Filter Settings", Description="ADX value BELOW which the market is considered RANGING.")]
        public int RegimeAdxRangeThreshold { get; set; }
        [NinjaScriptProperty, Display(Name = "Regime BB Period", Order = 17, GroupName = "07. Core Indicator & Filter Settings")]
        public int RegimeBBPeriod { get; set; }
        [NinjaScriptProperty, Display(Name = "Regime BB StdDev", Order = 18, GroupName = "07. Core Indicator & Filter Settings")]
        public double RegimeBBStdDev { get; set; }
        [NinjaScriptProperty, Display(Name = "Regime Squeeze Lookback", Order = 19, GroupName = "07. Core Indicator & Filter Settings", Description="Looks for the tightest Bollinger Band Width over this many bars to identify a Breakout setup.")]
        public int RegimeSqueezeLookback { get; set; }
		#endregion

		#region 07a. Global ATR Settings
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "ATR Period (Smoothing)", GroupName = "07a. Global ATR Settings", Order = 1)] public int AtrPeriod { get; set; }
		[NinjaScriptProperty, Range(0.1, double.MaxValue), Display(Name = "ATR Multiplier", GroupName = "07a. Global ATR Settings", Order = 2)] public double AtrMultiplier { get; set; }
		[NinjaScriptProperty, Range(0.1, double.MaxValue), Display(Name = "Std. Dev. Multiplier", GroupName = "07a. Global ATR Settings", Order = 3)] public double StdDevMultiplier { get; set; }
		#endregion
		
		#region 08. Visuals & Diagnostics
		[NinjaScriptProperty, Display(Name = "Show Daily PnL", Order = 1, GroupName = "08. Visuals & Diagnostics")] public bool showDailyPnl { get; set; }			
		[NinjaScriptProperty, Display(Name = "Enable Trend Background", Order = 2, GroupName = "08. Visuals & Diagnostics")] public bool EnableTrendBackground { get; set; }
		[NinjaScriptProperty, Display(Name = "Font Size", Order = 3, GroupName = "08. Visuals & Diagnostics")] public int FontSize { get; set; }
		[NinjaScriptProperty, Display(Name = "Text Panel Transparency", Order = 4, GroupName = "08. Visuals & Diagnostics")] public int Transparency { get; set; }
		[XmlIgnore, Display(Name = "Daily PnL Color", Order = 5, GroupName = "08. Visuals & Diagnostics")] public Brush colorDailyProfitLoss { get; set; }	
		[Browsable(false)] public string colorDailyProfitLossSerialize { get { return Serialize.BrushToString(colorDailyProfitLoss); } set { colorDailyProfitLoss = Serialize.StringToBrush(value); } }
		[NinjaScriptProperty, Display(Name="Daily PnL Position", Order = 6, GroupName = "08. Visuals & Diagnostics")] public TextPosition PositionDailyPNL { get; set; }
        [NinjaScriptProperty, Display(Name = "Show STATUS PANEL", Order = 7, GroupName = "08. Visuals & Diagnostics")] public bool showPnl { get; set; }		
		[XmlIgnore, Display(Name = "STATUS PANEL Color", Order = 8, GroupName = "08. Visuals & Diagnostics")] public Brush colorPnl { get; set; }				
		[Browsable(false)] public string colorPnlSerialize { get { return Serialize.BrushToString(colorPnl); } set { colorPnl = Serialize.StringToBrush(value); } }
		[NinjaScriptProperty, Display(Name="STATUS PANEL Position", Order = 9, GroupName = "08. Visuals & Diagnostics")] public TextPosition PositionPnl { get; set; }	
		[NinjaScriptProperty, Display(Name="Show Historical Trades", Order= 10, GroupName="08. Visuals & Diagnostics")] public bool ShowHistorical { get; set; }
	    [NinjaScriptProperty, Display(Name = "Enable Debug Logging", Order = 11, GroupName = "08. Visuals & Diagnostics", Description="Enables detailed logging to a file in the MyDocuments/AlgoTrader folder. Use for debugging only.")] public bool EnableLogging { get; set; }
		[NinjaScriptProperty, Display(Name="Enable JSON Trade Logging", Order=12, GroupName="08. Visuals & Diagnostics")] public bool EnableJsonLogging { get; set; }
		[Display(Name="JSON Log File Name", Description="e.g., 'MyPerformanceLog.jsonl'. File will be saved in Documents/NinjaTrader 8/", Order=13, GroupName="08. Visuals & Diagnostics")]
		public string JsonLogFileName { get; set; }
		[NinjaScriptProperty, Display(Name="Enable CSV Trade Logging", Order=14, GroupName="08. Visuals & Diagnostics")] public bool EnableTradeLogging { get; set; }
		[Display(Name="CSV Log File Name", Description="e.g., 'MyPerformanceLog.csv'. File will be saved in Documents/NinjaTrader 8/", Order=15, GroupName="08. Visuals & Diagnostics")]
		public string TradeLogFileName { get; set; }
        [NinjaScriptProperty, Display(Name = "Enable Health Checks (Live)", Description = "Enables safety features like data loss and rejection detection.", Order = 16, GroupName = "08. Visuals & Diagnostics")] public bool EnableHealthChecks { get; set; }
        [NinjaScriptProperty, Range(5, 60), Display(Name = "Data Loss Timeout (Sec)", Description = "Disables the strategy if no new tick is received.", Order = 17, GroupName = "08. Visuals & Diagnostics")] public int DataLossTimeoutSeconds { get; set; }
        [NinjaScriptProperty, Range(2, 10), Display(Name = "Max Order Rejections", Description = "Disables the strategy if this many consecutive orders are rejected.", Order = 18, GroupName = "08. Visuals & Diagnostics")] public int MaxConsecutiveRejections { get; set; }
		[Display(Name = "Show Open", GroupName = "08. Visuals & Diagnostics", Order = 19)] public bool ShowOpen { get; set; }
		[Display(Name = "Show High", GroupName = "08. Visuals & Diagnostics", Order = 20)] public bool ShowHigh { get; set; }
		[Display(Name = "Show Low", GroupName = "08. Visuals & Diagnostics", Order = 21)] public bool ShowLow { get; set; }
		#endregion
		
		#region 09. Advanced Execution
        [NinjaScriptProperty, Display(Name = "Enable Scale-Out Execution", Description = "If true, the strategy will automatically take partial profits at predefined levels.", Order = 1, GroupName = "09. Advanced Execution"), RefreshProperties(RefreshProperties.All)]
        public bool EnableScaleOutExecution { get; set; }

        [NinjaScriptProperty, Range(1, 99), Display(Name = "Scale-Out Lvl 1 (%)", Description = "The percentage of the profit target at which to take the first partial profit.", Order = 2, GroupName = "09. Advanced Execution")]
        public int ScaleOutLevel1Percent { get; set; }

        [NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "Scale-Out Qty 1", Description = "The number of contracts to close for the first partial profit.", Order = 3, GroupName = "09. Advanced Execution")]
        public int ScaleOutQty1 { get; set; }

        [NinjaScriptProperty, Range(1, 100), Display(Name = "Scale-Out Lvl 2 (%)", Description = "The percentage of the profit target at which to take the second partial profit.", Order = 4, GroupName = "09. Advanced Execution")]
        public int ScaleOutLevel2Percent { get; set; }

        [NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "Scale-Out Qty 2", Description = "The number of contracts to close for the second partial profit.", Order = 5, GroupName = "09. Advanced Execution")]
        public int ScaleOutQty2 { get; set; }
        
		[NinjaScriptProperty, Display(Name = "Stop Loss Mode", GroupName = "09. Advanced Execution", Order = 6, Description = "Static: Uses fixed SL values set in parameters. Dynamic: Learns from recent trade performance to set SL."), RefreshProperties(RefreshProperties.All)]
		public TradeManagementMode StopMode { get; set; }
		[NinjaScriptProperty, Display(Name = "Profit Target Mode", GroupName = "09. Advanced Execution", Order = 7, Description = "Static: Uses fixed TP values set in parameters. Dynamic: Learns from recent trade performance to set TP."), RefreshProperties(RefreshProperties.All)]
		public TradeManagementMode TargetMode { get; set; }
		[NinjaScriptProperty, Display(Name = "Dynamic: Initial SL Ticks (Fallback)", Description="The Stop Loss to use until the system has learned a value.", GroupName = "09. Advanced Execution", Order = 8)]
		public double DynamicInitialSL { get; set; }
		[NinjaScriptProperty, Display(Name = "Dynamic: Initial TP Ticks (Fallback)", Description="The Profit Target to use until the system has learned a value.", GroupName = "09. Advanced Execution", Order = 9)]
		public double DynamicInitialTP { get; set; }
		[NinjaScriptProperty, Display(Name = "Dynamic: SL Padding (Ticks)", Description="Extra ticks to add to the learned Max Drawdown for the stop.", GroupName = "09. Advanced Execution", Order = 10)]
		public double DynamicSLPadding { get; set; }
		[NinjaScriptProperty, Display(Name = "Dynamic: Burn-In Trades", Description="The number of trades to execute before the dynamic SL/TP adjustments become active.", GroupName = "09. Advanced Execution", Order = 11), Range(1, 100)]
		public int DynamicBurnInTrades { get; set; }
		[NinjaScriptProperty, Display(Name = "Dynamic: Averaging Lookback", Description="The number of recent trades to average for the dynamic SL/TP calculation.", GroupName = "09. Advanced Execution", Order = 12), Range(1, 100)]
		public int DynamicAvgLookback { get; set; }
		[NinjaScriptProperty, Display(Name = "Dynamic: Calculation Mode", Description="The statistical method used for dynamic risk calculation.", GroupName = "09. Advanced Execution", Order = 13), RefreshProperties(RefreshProperties.All)]
		public DynamicCalculationMode DynamicRiskMode { get; set; }
		[NinjaScriptProperty, Display(Name = "Dynamic: Percentile", Description="The percentile (1-99) to use for calculation. Higher values are more conservative.", GroupName = "09. Advanced Execution", Order = 14), Range(1, 99)]
		public int DynamicRiskPercentile { get; set; }
		#endregion
		
		#region 10. About
		[NinjaScriptProperty, Display(Name="BaseAlgoVersion", Order=1, GroupName="10. About")] public string BaseAlgoVersion { get; set; }
		[NinjaScriptProperty, Display(Name="Author", Order=2, GroupName="10. About")] public string Author { get; set; }		
		[NinjaScriptProperty, Display(Name="StrategyName", Order=3, GroupName="10. About")] public string StrategyName { get; set; }
		[NinjaScriptProperty, Display(Name="StrategyVersion", Order =4, GroupName="10. About")] public string StrategyVersion { get; set; }
		[NinjaScriptProperty, Display(Name="Credits", Order=5, GroupName="10. About")] public string Credits { get; set; }
		[NinjaScriptProperty, Display(Name="Chart Type", Order=6, GroupName="10. About")] public string ChartType { get; set; }
		[NinjaScriptProperty, Display(Name = "PayPal Donation URL", Order = 7, GroupName = "10. About")] public string paypal { get; set; }
		#endregion
		
		#region 11. Bot Parameters
		
        #region 11a. Bot Parameters - Breakout
        [NinjaScriptProperty, Display(Name = "Enable Bollinger Breakout Bot", GroupName = "11a. Bot Parameters - Breakout", Order = 1)] 
        public bool EnableBollingerBot { get; set; }
        [NinjaScriptProperty, Display(Name = "Show Bollinger Plot", Order = 2, GroupName = "11a. Bot Parameters - Breakout")] 
        public bool ShowBollinger { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Enable Keltner Breakout Bot", GroupName = "11a. Bot Parameters - Breakout", Order = 60)] public bool EnableKeltnerBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Keltner Plot", Order = 61, GroupName = "11a. Bot Parameters - Breakout")] public bool ShowKeltnerPlot { get; set; }
        [NinjaScriptProperty, Display(Name = "Keltner Period", GroupName = "11a. Bot Parameters - Breakout", Order = 62)] public int KeltnerPeriod { get; set; }
		[NinjaScriptProperty, Range(0.1, 10.0), Display(Name = "Keltner Offset Multiplier", GroupName = "11a. Bot Parameters - Breakout", Order = 63, Description = "Multiplier for the outer bands (Band 4) used for signal logic.")] 
		public double KeltnerOffsetMultiplier { get; set; }
		
        #endregion

        #region 11b. Bot Parameters - Range
        
        [NinjaScriptProperty, Display(Name = "Enable SmartMoney", GroupName = "11b. Bot Parameters - Range", Order = 40)] public bool EnableSmartMoneyBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Market Structure", Order = 41, GroupName = "11b. Bot Parameters - Range")] public bool ShowMarketStructurePlot { get; set; }
        [NinjaScriptProperty, Display(Name = "Market Structure Period", GroupName = "11b. Bot Parameters - Range", Order = 42)] public int MarketStructurePeriod { get; set; }
		[NinjaScriptProperty, Display(Name = "Use Reversals", GroupName = "11b. Bot Parameters - Range", Order = 43)] public bool MarketStructureUseReversals { get; set; }
		[NinjaScriptProperty, Display(Name = "Use Continuations", GroupName = "11b. Bot Parameters - Range", Order = 44)] public bool MarketStructureUseContinuations { get; set; }

        [NinjaScriptProperty, Display(Name = "Enable Willy", Order = 60, GroupName = "11b. Bot Parameters - Range")] public bool EnableWilly { get; set; }
		[NinjaScriptProperty, Display(Name = "Show William %R Plot", Order = 61, GroupName = "11b. Bot Parameters - Range")] public bool ShowWilly { get; set; }
        [NinjaScriptProperty, Display(Name = "Willy Range Period", GroupName = "11b. Bot Parameters - Range", Order = 62)] public int wrPeriod { get; set; }
		[NinjaScriptProperty, Display(Name = "Willy Up Level", GroupName = "11b. Bot Parameters - Range", Order = 63)] [Range(-100, 0)] public int wrUp { get; set; }
		[NinjaScriptProperty, Display(Name = "Willy Down Level", GroupName = "11b. Bot Parameters - Range", Order = 64)] [Range(-100, 0)] public int wrDown { get; set; }

		#endregion

        #region 11c. Bot Parameters - Trend
        
        [NinjaScriptProperty, Display(Name = "Enable Range Filtered Bot", GroupName = "11c. Bot Parameters - Trend", Order = 30)] public bool EnableRangeFilteredBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Range Filtered Plot", Order = 31, GroupName = "11c. Bot Parameters - Trend")] public bool ShowRangeFilterPlot { get; set; }
        [NinjaScriptProperty, Display(Name = "RF Sampling Period", GroupName = "11c. Bot Parameters - Trend", Order = 32)] public int RFSamplingPeriod { get; set; }
		[NinjaScriptProperty, Display(Name = "RF Range Multiplier", GroupName = "11c. Bot Parameters - Trend", Order = 33)] [Range(0.1, double.MaxValue)] public double RFRangeMultiplier { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable DeviationTrendProfile Bot", GroupName = "11c. Bot Parameters - Trend", Order = 60)] public bool EnableDeviationTrendProfileBot { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable Momentum Filter", Order = 61, GroupName = "11c. Bot Parameters - Trend")]
		public bool EnableMomo { get; set; }
        [NinjaScriptProperty, Display(Name = "Show Momentum Plot", GroupName = "11c. Bot Parameters - Trend", Order = 62)] public bool ShowMomo { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="Momentum Period", Order = 63, GroupName="11c. Bot Parameters - Trend")]
		public int MomentumPeriod { get; set; }
		[NinjaScriptProperty, Display(Name="Momentum Threshold", Description="The minimum Momentum value required to confirm a strong trend.", Order = 64, GroupName="11c. Bot Parameters - Trend")]
		public double MomoThreshold { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable Momentum Extremes Bot", GroupName = "11c. Bot Parameters - Trend", Order = 65)] public bool EnableMomentumExtremesBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Momentum Extremes Plot", GroupName = "11c. Bot Parameters - Trend", Order = 66)] public bool ShowMomentumExtremesPlot { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "HMA Period", GroupName = "11c. Bot Parameters - Trend", Order = 67)] public int MedBandsHmaPeriod { get; set; }
		[NinjaScriptProperty, Range(2, int.MaxValue), Display(Name = "Extremes Lookback", GroupName = "11c. Bot Parameters - Trend", Order = 68)] public int MedBandsExtremesLookback { get; set; }
		[NinjaScriptProperty, Range(1, 20), Display(Name = "Driver Width", GroupName = "11c. Bot Parameters - Trend", Order = 74)] public int MedBandsDriverWidth { get; set; }
		[XmlIgnore, NinjaScriptProperty, Display(Name = "Up Color", GroupName = "11c. Bot Parameters - Trend", Order = 75)] public Brush MedBandsUpColor { get; set; }
		[Browsable(false)] public string MedBandsUpColorSerialize { get { return Serialize.BrushToString(MedBandsUpColor); } set { MedBandsUpColor = Serialize.StringToBrush(value); } }
		[XmlIgnore, NinjaScriptProperty, Display(Name = "Down Color", GroupName = "11c. Bot Parameters - Trend", Order = 76)] public Brush MedBandsDownColor { get; set; }
		[Browsable(false)] public string MedBandsDownColorSerialize { get { return Serialize.BrushToString(MedBandsDownColor); } set { MedBandsDownColor = Serialize.StringToBrush(value); } }

		[NinjaScriptProperty, Display(Name = "Enable Momentum VMA Bot", GroupName = "11c. Bot Parameters - Trend", Order = 77)] public bool EnableMomentumVmaBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show VMA Bands Plot", GroupName = "11c. Bot Parameters - Trend", Order = 78)] public bool ShowMomentumVmaPlot { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "VMA Period", GroupName = "11c. Bot Parameters - Trend", Order = 79)] public int VmaBandsPeriod { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "VMA Volatility Period", GroupName = "11c. Bot Parameters - Trend", Order = 80)] public int VmaBandsVolatilityPeriod { get; set; }
		[NinjaScriptProperty, Range(2, int.MaxValue), Display(Name = "Extremes Lookback", GroupName = "11c. Bot Parameters - Trend", Order = 81)] public int VmaBandsExtremesLookback { get; set; }
		[XmlIgnore, NinjaScriptProperty, Display(Name = "Up Color", GroupName = "11c. Bot Parameters - Trend", Order = 87)] public Brush VmaBandsUpColor { get; set; }
		[Browsable(false)] public string VmaBandsUpColorSerialize { get { return Serialize.BrushToString(VmaBandsUpColor); } set { VmaBandsUpColor = Serialize.StringToBrush(value); } }
		[XmlIgnore, NinjaScriptProperty, Display(Name = "Down Color", GroupName = "11c. Bot Parameters - Trend", Order = 88)] public Brush VmaBandsDownColor { get; set; }
		[Browsable(false)] public string VmaBandsDownColorSerialize { get { return Serialize.BrushToString(MedBandsDownColor); } set { MedBandsDownColor = Serialize.StringToBrush(value); } }
		[NinjaScriptProperty, Range(1, 20), Display(Name = "Driver Width", GroupName = "11c. Bot Parameters - Trend", Order = 89)] public int VmaBandsDriverWidth { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable PSAR Bot", GroupName = "11c. Bot Parameters - Trend", Order = 90)] public bool EnablePSARBot { get; set; }
		
		[NinjaScriptProperty, Display(Name = "Enable Swing Structure Bot", GroupName = "11c. Bot Parameters - Trend", Order = 94)] public bool EnableSwingStructureBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Swing Structure Plot", Order = 95, GroupName = "11c. Bot Parameters - Trend")] public bool ShowSwingPlot { get; set; }
        [NinjaScriptProperty, Display(Name = "Swing Strength", GroupName = "11c. Bot Parameters - Trend", Order = 96)] [Range(1, int.MaxValue)] public int SwingStrength { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable Trend Architect Bot", GroupName = "11c. Bot Parameters - Trend", Order = 97)] public bool EnableTrendArchitectBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Trend Architect Plot", Order = 98, GroupName = "11c. Bot Parameters - Trend")] public bool ShowTrendArchitectPlot { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable Volume Thrust Bot", GroupName = "11c. Bot Parameters - Trend", Order = 102, Description = "Enables trade signals based on a surge in Up or Down volume.")]
		public bool EnableVolumeThrustBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Volume Up Down", Order = 103, GroupName = "11c. Bot Parameters - Trend")] public bool ShowVolumeUpDown { get; set; }
        #endregion

        #region 11d. Bot Parameters - Universal

        [NinjaScriptProperty, Display(Name = "Enable LinRegBands Bot", GroupName = "11d. Bot Parameters - Universal", Order = 1)] public bool EnableLinRegBandsBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show LinReg Plot", GroupName = "11d. Bot Parameters - Universal", Order = 2)] public bool ShowLinRegBandsPlot { get; set; }
        [NinjaScriptProperty, Display(Name = "LinReg Period", GroupName = "11d. Bot Parameters - Universal", Order = 3)] public int LinRegPeriod { get; set; }

		[NinjaScriptProperty, Display(Name = "Enable KingKhanh", Order = 14, GroupName = "11d. Bot Parameters - Universal")] public bool EnableKingKhanh { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Regression Bands", Order = 15, GroupName = "11d. Bot Parameters - Universal")] public bool ShowRegChan { get; set; }
		[NinjaScriptProperty, Range(2, int.MaxValue), Display(Name = "Regression Period", GroupName = "11d. Bot Parameters - Universal", Order = 16)] public int RegBandsPeriod { get; set; }
			
        [NinjaScriptProperty, Display(Name = "Enable Hooker (HMA ATR)", Order = 25, GroupName = "11d. Bot Parameters - Universal")] public bool EnableHooker { get; set; }
		[NinjaScriptProperty, Display(Name = "Show HMA Hooks Plot", Order = 26, GroupName = "11d. Bot Parameters - Universal")] public bool ShowHmaHooks { get; set; }
        [NinjaScriptProperty, Display(Name = "HMA Period", GroupName = "11d. Bot Parameters - Universal", Order = 27)] public int HmaHooksPeriod { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Enable HiLoBands Bot", GroupName = "11d. Bot Parameters - Universal", Order = 35)] public bool EnableHiLoBandsBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show HiLoBands Plot", GroupName = "11d. Bot Parameters - Universal", Order = 36)] public bool ShowMultiLevelHiLoBandsPlot { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="Lookback Period", Order = 37, GroupName="11d. Bot Parameters - Universal")] public int HiLoLookbackPeriod { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name="Smoothing Period", Order = 38, GroupName="11d. Bot Parameters - Universal")] public int HiLoSmoothingPeriod { get; set; }
		[NinjaScriptProperty, Range(0.1, double.MaxValue), Display(Name="HiLo ATR Multiplier", Order = 40, GroupName="11d. Bot Parameters - Universal")] public double HiLoAtrMultiplier { get; set; }

        [NinjaScriptProperty, Display(Name = "Enable Pivot Impulse Bot", GroupName = "11d. Bot Parameters - Universal", Order = 45)] public bool EnablePivotImpulseBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Pivot Impulse Lines", GroupName = "11d. Bot Parameters - Universal", Order = 46)] public bool ShowPivotImpulseLines { get; set; }
        [NinjaScriptProperty, Display(Name = "PIS Swing Strength", GroupName = "11d. Bot Parameters - Universal", Order = 47)] public int PIS_SwingStrength { get; set; }
		[NinjaScriptProperty, Display(Name = "PIS Pivot Lookback", GroupName = "11d. Bot Parameters - Universal", Order = 48)] public int PIS_PivotLookback { get; set; }

        [NinjaScriptProperty, Display(Name = "Enable MagicTrendy", Order = 50, GroupName = "11d. Bot Parameters - Universal")] public bool EnableMagicTrendy { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Magic Trend Plot", Order = 51, GroupName = "11d. Bot Parameters - Universal")] public bool ShowTrendMagic { get; set; }
        [NinjaScriptProperty, Display(Name = "Magic CCI Period", GroupName = "11d. Bot Parameters - Universal", Order = 52)] public int TrendMagicCciPeriod { get; set; }
		[NinjaScriptProperty, Display(Name = "Magic ATR Mult", GroupName = "11d. Bot Parameters - Universal", Order = 54)] [Range(0.0001, double.MaxValue)] public double TrendMagicAtrMult { get; set; }

        [NinjaScriptProperty, Display(Name = "Enable Johny5", GroupName = "11d. Bot Parameters - Universal", Order = 70)] public bool EnableJohny5 { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Johny5 Plot", Order = 71, GroupName = "11d. Bot Parameters - Universal")] public bool ShowJohny5 { get; set; }
        [NinjaScriptProperty, Display(Name = "J5 MACD Fast", GroupName = "11d. Bot Parameters - Universal", Order = 72)] public int JbSignalMacdFast { get; set; }
        [NinjaScriptProperty, Display(Name = "J5 MACD Slow", GroupName = "11d. Bot Parameters - Universal", Order = 73)] public int JbSignalMacdSlow { get; set; }
		[NinjaScriptProperty, Display(Name = "J5 MACD Smooth", GroupName = "11d. Bot Parameters - Universal", Order = 74)] [Range(1, int.MaxValue)] public int JbSignalMacdSmooth { get; set; }
		[NinjaScriptProperty, Display(Name = "J5 WR Period", GroupName = "11d. Bot Parameters - Universal", Order = 75)] [Range(1, int.MaxValue)] public int JbSignalWrPeriod { get; set; }
		[NinjaScriptProperty, Display(Name = "J5 WR EMA Period", GroupName = "11d. Bot Parameters - Universal", Order = 76)] [Range(1, int.MaxValue)] public int JbSignalWrEmaPeriod { get; set; }
		[NinjaScriptProperty, Display(Name = "J5 ALMA Fast Len", GroupName = "11d. Bot Parameters - Universal", Order = 77)] public int JbSignalAlmaFastLen { get; set; }
		[NinjaScriptProperty, Display(Name = "J5 ALMA Slow Len", GroupName = "11d. Bot Parameters - Universal", Order = 88)] public int JbSignalAlmaSlowLen { get; set; }

        [NinjaScriptProperty, Display(Name = "Bollinger Universal: StdDev", GroupName = "11d. Bot Parameters - Universal", Order = 90)] public double BollingerStdDev { get; set; }
        [NinjaScriptProperty, Display(Name = "Bollinger Universal: Period", GroupName = "11d. Bot Parameters - Universal", Order = 91)] public int BollingerPeriod { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Enable TMO Bot", GroupName = "11d. Bot Parameters - Universal", Order = 100)] public bool EnableTMOBot { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Enable SuperTrend Bot", GroupName = "11d. Bot Parameters - Universal", Order = 110)] public bool EnableSuperTrendBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Super Trend Plot", Order = 111, GroupName = "11d. Bot Parameters - Universal")] public bool ShowSuperTrend { get; set; }
        [NinjaScriptProperty, Display(Name = "SuperTrend Period", GroupName = "11d. Bot Parameters - Universal", Order = 112)] public int SuperTrendPeriod { get; set; }
		[NinjaScriptProperty, Range(2, 3), Display(Name = "SuperTrend Poles", GroupName = "11d. Bot Parameters - Universal", Order = 113)] public int SuperTrendPoles { get; set; }
        
        [NinjaScriptProperty, Display(Name = "Enable Coral Bot", GroupName = "11d. Bot Parameters - Universal", Order = 120)] public bool EnableCoralBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Coral Trend Plot", Order = 121, GroupName = "11d. Bot Parameters - Universal")] public bool ShowCoralPlot { get; set; }
        [NinjaScriptProperty, Display(Name = "Coral Smoothing", GroupName = "11d. Bot Parameters - Universal", Order = 122)] public int CoralSmoothingPeriod { get; set; }
		[NinjaScriptProperty, Display(Name = "Coral Constant D", GroupName = "11d. Bot Parameters - Universal", Order = 123)] [Range(0.0001, double.MaxValue)] public double CoralConstantD { get; set; }
		
        [NinjaScriptProperty, Display(Name = "Enable BalaBot", Order = 130, GroupName = "11d. Bot Parameters - Universal")] public bool EnableBalaBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show Bala Plot", Order = 131, GroupName = "11d. Bot Parameters - Universal")] public bool ShowBalaBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Bala Use SMMA", GroupName = "11d. Bot Parameters - Universal", Order = 132)] public bool BalaUseSMMA { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "Bala Midline Period", GroupName = "11d. Bot Parameters - Universal", Order = 133)] public int BalaEMAPeriod { get; set; }
				
		[NinjaScriptProperty, Display(Name = "Enable TrendSniper Bot", GroupName = "11d. Bot Parameters - Universal", Order = 140)] public bool EnableTrendSniperBot { get; set; }
		[NinjaScriptProperty, Display(Name = "Show TrendSniper Plot", GroupName = "11d. Bot Parameters - Universal", Order = 141)] public bool ShowTrendSniperPlot { get; set; }
		[NinjaScriptProperty, Range(1, int.MaxValue), Display(Name = "TrendSniper Length", GroupName = "11d. Bot Parameters - Universal", Order = 142)] public int TrendSniperLength { get; set; }
		#endregion
		
		#endregion			
    }
}