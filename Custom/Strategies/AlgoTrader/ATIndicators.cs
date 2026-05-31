#region Using declarations
using System;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui; 
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.AlgoTrader;
using NinjaTrader.NinjaScript.Indicators.FxStill.SmartMoney;
using NinjaTrader.NinjaScript.Indicators.ZombiePack9;
using NinjaTrader.NinjaScript.Indicators.TradeSaber;
using NinjaTrader.NinjaScript.Indicators.TradeSaber_SignalMod;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{
    public abstract partial class AlgoBase : Strategy
    {
        #region Indicator Declarations
        
        [Browsable(false)][XmlIgnore] public DM DM1 { get; private set; }
        [Browsable(false)][XmlIgnore] public ATR ATR1 { get; private set; }  
		
        protected ADX regimeAdx;
        protected Bollinger regimeBollinger;
        protected Series<double> regimeBBWidth;
        protected MIN regimeMinBBW;        
		[Browsable(false)][XmlIgnore] public CurrentDayOHL dailyOHLIndicator { get; private set; }		
		
		[Browsable(false)][XmlIgnore] public ATRTrailBands TrailStop_ATR { get; private set; }
        [Browsable(false)][XmlIgnore] public Momentum botMomentum { get; private set; }	
		[Browsable(false)][XmlIgnore] public BollingerMultiBands botBollingerUniversal { get; set; }
        [Browsable(false)][XmlIgnore] public HiLoATRBands botHiLoBands { get; set; }
		[Browsable(false)][XmlIgnore] public LinRegATRBands botLinRegBands { get; set; }
        [Browsable(false)][XmlIgnore] public HMAHooksATRBands botHmaAtrBands { get; set; }		
        [Browsable(false)][XmlIgnore] public AuSuperSmoothFilterATRBands botSuperTrendBands { get; set; }
        [Browsable(false)][XmlIgnore] public TrendMagicATRBands botTrendMagicBands { get; set; }
		[Browsable(false)][XmlIgnore] public WilliamsR botWilly { get; set; }      
        [Browsable(false)][XmlIgnore] public JBSignalATRBands botJbSignalBands { get; set; }		
		[Browsable(false)][XmlIgnore] public BalaATRBands botBalaBands { get; set; }
		[Browsable(false)][XmlIgnore] public CoralTrendATRBands botCoralBands { get; set; }
		[Browsable(false)][XmlIgnore] public MarketStructuresLite botMarketStructure { get; set; }
		[Browsable(false)][XmlIgnore] public KeltnerMultiBands botKeltner { get; set; }
        [Browsable(false)][XmlIgnore] public ParabolicSAR2 botPSAR { get; set; }
        [Browsable(false)][XmlIgnore] public TrendArchitectLite botTrendArchitect { get; set; }
        [Browsable(false)][XmlIgnore] public Swing botSwing { get; set; }
		[Browsable(false)][XmlIgnore] public PivotImpulseTrendLines botPivotImpulse { get; private set; }
		[Browsable(false)][XmlIgnore] public TrendSniperATRBands botTrendSniper { get; private set; }
		[Browsable(false)][XmlIgnore] public DeviationTrendProfile botDeviationTrendProfile { get; private set; }
		[Browsable(false)][XmlIgnore] public RangeFilteredTrendSignalsATRBands botRangeFiltered { get; private set; }	
		[Browsable(false)][XmlIgnore] public MomentumExtremesATRBands botMomentumExtremesBands { get; private set; }
		[Browsable(false)][XmlIgnore] public VMAATRBands botMomentumVmaBands { get; private set; }
		[Browsable(false)][XmlIgnore] public RegressionATRBands botRegressionBands { get; private set; }		
		[Browsable(false)][XmlIgnore] public VolumeUpDown botVolumeUpDown { get; private set; }
		[Browsable(false)][XmlIgnore] public TMO botTMO { get; private set; }
        #endregion

        #region Indicator Initialization and Calculation

        private void InitializeAllIndicators()
        {
			ATR1 = ATR(14);
			DM1 = DM(DmPeriod);
			
			botMomentum = Momentum(MomentumPeriod);
			if (ShowMomo) 
			{
				botMomentum.Plots[0].Width = 2;
				AddChartIndicator(botMomentum);
			}
			
			botVolumeUpDown = VolumeUpDown();
			if (ShowVolumeUpDown) 
			{
				botVolumeUpDown.Plots[0].Width = 5;
				botVolumeUpDown.Plots[1].Width = 5;
				AddChartIndicator(botVolumeUpDown);
			}
			
			botTMO = TMO(TMOLength, TMOCalcLength, TMOSmoothLength);
			if (ShowTMO) AddChartIndicator(botTMO);
			
			if (StopType == StopManagementType.ATR || StopType == StopManagementType.HybridAtrPsar)
			{
				TrailStop_ATR = ATRTrailBands(TrailStop_ATR_Period, TrailStop_ATR_Mult);
				if (ShowAtrTrailPlot) AddChartIndicator(TrailStop_ATR);
			}
            
			if(StopType == StopManagementType.ParabolicSAR || StopType == StopManagementType.HybridAtrPsar)
			{
				botPSAR = ParabolicSAR2(PSARAcceleration, PSARAccelerationMax, PSARAcceleration);
				if (ShowPSARPlot) AddChartIndicator(botPSAR);
			}
			
			if(EnableMomentumExtremesBot)
			{
			    botMomentumExtremesBands = MomentumExtremesATRBands(MedBandsHmaPeriod, MedBandsExtremesLookback, 0, AtrPeriod, AtrMultiplier, MedBandsUpColor, MedBandsDownColor, MedBandsDriverWidth);
			    botMomentumExtremesBands.IsAutoScale = false;
			    if(ShowMomentumExtremesPlot) AddChartIndicator(botMomentumExtremesBands);
			}
			
			if(EnableMomentumVmaBot)
			{
			    botMomentumVmaBands = VMAATRBands(VmaBandsPeriod, VmaBandsVolatilityPeriod, VmaBandsExtremesLookback, AtrPeriod, AtrMultiplier, VmaBandsUpColor, VmaBandsDownColor, VmaBandsDriverWidth);
			    if(ShowMomentumVmaPlot) AddChartIndicator(botMomentumVmaBands);
			}
			
			if (EnableKingKhanh) 
			{
			    botRegressionBands = RegressionATRBands(RegBandsPeriod, AtrPeriod, AtrMultiplier);
			    if (ShowRegChan) AddChartIndicator(botRegressionBands);
			}
					               
			if (EnableLinRegBandsBot)
			{
			    botLinRegBands = LinRegATRBands(LinRegPeriod, AtrPeriod, AtrMultiplier);
			    if (ShowLinRegBandsPlot) AddChartIndicator(botLinRegBands); 
			}

			if (ShowDM) 
			{
				DM1.Plots[0].Brush = Brushes.Yellow; 
				DM1.Plots[1].Brush = Brushes.Lime; DM1.Plots[2].Brush = Brushes.Red;
				DM1.Plots[0].Width = 2; DM1.Plots[1].Width = 2; DM1.Plots[2].Width = 2;
				AddChartIndicator(DM1);
			}

			if (EnableAutoRegimeDetection)
			{
				regimeAdx = ADX(RegimeAdxPeriod);
                regimeBollinger = Bollinger(RegimeBBStdDev, RegimeBBPeriod);
                regimeBBWidth = new Series<double>(this);
                regimeMinBBW = MIN(regimeBBWidth, RegimeSqueezeLookback);
			}
			
			if (EnablePivotImpulseBot)
			{
				botPivotImpulse = PivotImpulseTrendLines(PIS_SwingStrength, PIS_PivotLookback, Brushes.Yellow, 2, Brushes.Lime, Brushes.Red, 2, Color.FromArgb(30, 0, 128, 0), Color.FromArgb(30, 139, 0, 0));
				AddChartIndicator(botPivotImpulse);
			}
			
			if (EnableTrendSniperBot)
			{
			    botTrendSniper = TrendSniperATRBands(TrendSniperLength, 2, AtrPeriod, AtrMultiplier);
			    if(ShowTrendSniperPlot) AddChartIndicator(botTrendSniper);
			}
			
			if (FilterMode == MasterTrendFilterMode.DeviationTrendProfile || 
			    EnableDeviationTrendProfileBot || 
			    InitialStopMode == InitialStopCalculationMode.DeviationTrendProfile ||
			    ProfitTargetMode == ProfitTargetCalculationMode.DeviationTrendProfile ||
				ChopDetectionMethod == ChopDetectionMode.DeviationTrendProfile)
			{				
			    botDeviationTrendProfile = DeviationTrendProfile(false, DTPLength, DTPVolatilityPeriod, AtrMultiplier, AtrPeriod, DTPNormalizationLookback, false, DTPAverageType);
			    if (ShowDTPPlot) AddChartIndicator(botDeviationTrendProfile);
			}
			
			if (FilterMode == MasterTrendFilterMode.RangeFiltered || 
			    EnableRangeFilteredBot || 
			    ChopDetectionMethod == ChopDetectionMode.RangeFiltered ||
			    InitialStopMode == InitialStopCalculationMode.RangeFiltered ||
			    ProfitTargetMode == ProfitTargetCalculationMode.RangeFiltered)
			{
			    // Updated constructor call with only AtrMultiplier and global AtrPeriod (Length)
			    // Mult 2-6 are now handled internally by the indicator logic.
			    botRangeFiltered = RangeFilteredTrendSignalsATRBands(true, KalmanAlpha, KalmanBeta, KalmanPeriod, DevMultiplier, SupertrendFactor, SupertrendAtrPeriod, AtrMultiplier, AtrPeriod);
			    
			    if(ShowRangeFilteredPlot)
			        AddChartIndicator(botRangeFiltered);
			}		
		
			if (EnableSwingStructureBot || EnableAutoRegimeDetection)
			{
				botSwing = Swing(SwingStrength);
				if (ShowSwingPlot) AddChartIndicator(botSwing);
			}

			if (EnableHooker)
			{
			    botHmaAtrBands = HMAHooksATRBands(HmaHooksPeriod, AtrPeriod, AtrMultiplier);
			    if (ShowHmaHooks) AddChartIndicator(botHmaAtrBands);
			}
			
			if (EnableCoralBot)
			{
			    botCoralBands = CoralTrendATRBands(CoralSmoothingPeriod, CoralConstantD, 3, AtrPeriod, AtrMultiplier);
			    if (ShowCoralPlot) AddChartIndicator(botCoralBands);
			}
			
			if (EnableJohny5)
			{
			    botJbSignalBands = JBSignalATRBands(JbSignalMacdFast, JbSignalMacdSlow, JbSignalMacdSmooth, JbSignalWrPeriod, JbSignalWrEmaPeriod, JbSignalAlmaFastLen, JbSignalAlmaSlowLen, AtrPeriod, AtrMultiplier);
			    if (ShowJohny5) AddChartIndicator(botJbSignalBands);
			}
			if (EnableMagicTrendy)
			{
			    botTrendMagicBands = TrendMagicATRBands(TrendMagicCciPeriod, AtrPeriod, TrendMagicAtrMult, AtrPeriod, AtrMultiplier);
			    if (ShowTrendMagic) AddChartIndicator(botTrendMagicBands);
			}
				
			if (EnableHiLoBandsBot)
			{
				botHiLoBands = HiLoATRBands(HiLoLookbackPeriod, HiLoSmoothingPeriod, AtrPeriod, HiLoAtrMultiplier, 2);
				if (ShowMultiLevelHiLoBandsPlot) AddChartIndicator(botHiLoBands);
			}
			
			if (EnableTrendBots)
			{		
				if (EnableTrendArchitectBot)
				{
				    botTrendArchitect = TrendArchitectLite(true, 50, 30, true, ShowTrendArchitectPlot, 18, 0.5, 50, 3.0, 20, 0.85, 6.0, 10, 0.5, 20, 30, 25);
				    if (ShowTrendArchitectPlot) AddChartIndicator(botTrendArchitect);
				}
				if (EnableSuperTrendBot)
				{
					botSuperTrendBands = AuSuperSmoothFilterATRBands(SuperTrendPoles, SuperTrendPeriod, AtrPeriod, AtrMultiplier);
				    if (ShowSuperTrend) AddChartIndicator(botSuperTrendBands);
				}
			}
			
			if (EnableRangeBots)
			{
				if (EnableBalaBot) 
				{
				    botBalaBands = BalaATRBands(BalaUseSMMA, BalaEMAPeriod, AtrPeriod, AtrMultiplier);
				    if (ShowBalaBot) AddChartIndicator(botBalaBands);
				}
				if (EnableSmartMoneyBot)
				{
					botMarketStructure = MarketStructuresLite(MarketStructurePeriod, true, 10, 14, Brushes.DodgerBlue, Brushes.Crimson, 2, NinjaTrader.Gui.DashStyleHelper.Solid, true, Brushes.Green, Brushes.Red, Brushes.Gray, 50, true, "LE", "SE", MarketStructureUseContinuations, MarketStructureUseReversals);
					if (ShowMarketStructurePlot) AddChartIndicator(botMarketStructure);
				}
				if (EnableWilly)
				{
					botWilly = WilliamsR(wrPeriod);
					if (ShowWilly) AddChartIndicator(botWilly);
				}
			}
			
			if (EnableBreakoutBots)
			{
				if (EnableBollingerBot)
			    {
			        // Signature updated from 5 parameters to 2
			        // Uses the global BollingerPeriod and the global consolidated AtrMultiplier
			        botBollingerUniversal = BollingerMultiBands(BollingerPeriod, StdDevMultiplier);
			        
			        if (ShowExhaustionBB)
			        {
			            AddChartIndicator(botBollingerUniversal);
			        }			
			    }
				if (EnableBollingerBot)
			    {
			        // Signature updated from 5 parameters to 2
			        // Uses the global BollingerPeriod and the global consolidated AtrMultiplier
			        botBollingerUniversal = BollingerMultiBands(BollingerPeriod, StdDevMultiplier);
			        
			        if (ShowExhaustionBB)
			        {
			            AddChartIndicator(botBollingerUniversal);
			        }			
			    }
			}			
        }		
        #endregion
    }
}