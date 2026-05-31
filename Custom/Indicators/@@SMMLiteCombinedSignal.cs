#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators.SMM; 
using SmmCustom = SimpleMoneyMetricsCommon; 
using NinjaTrader.Gui.Tools;
using System.Windows;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    [Gui.CategoryOrder("SMM Main Settings", 1)]
    public class SMMLiteCombinedSignal : Indicator
    {
        private SimpleMoneyMetricsLite  smmLite;
        private SMMDots                 smmDots;
        private int armedSignal = 0;
        private int trend_state = 0;

        #region User-configurable parameters for SMM - Lite

        [NinjaScriptProperty]
        [Display(Name="SMM Profit Target (Ticks)", Order=1, GroupName="SMM Main Settings")]
        [Range(1, int.MaxValue)]
        public int PS_ProfitTarget { get; set; }

        [NinjaScriptProperty]
        [Display(Name="SMM Enable Chop Filter", Order=2, GroupName="SMM Main Settings")]
        public bool SS_EnableChopFilter { get; set; }

        [NinjaScriptProperty]
        [Display(Name="SMM MA Enabled", Order=3, GroupName="SMM Main Settings")]
        public bool IN_MaEnabled { get; set; }

        [NinjaScriptProperty]
        [Display(Name="SMM MA Period", Order=4, GroupName="SMM Main Settings")]
        [Range(2, 1000)]
        public int IN_MaPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name="SMM MA Type", Order=5, GroupName="SMM Main Settings")]
        public SmmCustom.EMovingAverageType IN_MaMethod { get; set; }
        
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description             = @"Combines SMM Lite and SMM Dots with state logic for lagging signals.";
                Name                    = "SMM Lite Combined Signal";
                Calculate               = Calculate.OnBarClose;
                IsOverlay               = false;
                DisplayInDataBox        = true;
                DrawOnPricePanel        = false;
                PaintPriceMarkers       = true;
                ScaleJustification      = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                PS_ProfitTarget     = 40;
                SS_EnableChopFilter = true;
                IN_MaEnabled        = true;
                IN_MaPeriod         = 10;
                IN_MaMethod         = SmmCustom.EMovingAverageType.SMA;

                AddPlot(new Stroke(Brushes.Goldenrod, 2), PlotStyle.Bar, "CombinedSignal");
            }
            else if (State == State.DataLoaded)
			{
			    smmDots = SMMDots(Close);
			    
			    // Call SimpleMoneyMetricsLite using Input parameter
			    smmLite = SimpleMoneyMetricsLite(Input);
			    
			    if (smmLite != null)
			    {
			        // Apply user settings to the indicator
			        smmLite.PS_ProfitTarget = PS_ProfitTarget;
			        smmLite.SS_EnableChopFilter = SS_EnableChopFilter;
			        smmLite.IN_MaEnabled = IN_MaEnabled;
			        smmLite.IN_MaPeriod = IN_MaPeriod;
			        smmLite.IN_MaMethod = IN_MaMethod;
			    }
			    else
			    {
			        Print("ERROR: Could not create SimpleMoneyMetricsLite instance");
			    }
			}

        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 21) 
                return;
            
            // Check if indicators are ready
            if (smmLite == null || smmDots == null)
            {
                Print(string.Format("Bar {0}: Indicators not ready. smmLite={1}, smmDots={2}", 
                    CurrentBar, (smmLite == null ? "null" : "ok"), (smmDots == null ? "null" : "ok")));
                return;
            }
            
            // SimpleMoneyMetricsLite plot indices:
            // [4] = Signal (buy/sell signal)
            // [6] = Trend State (1 = bullish, -1 = bearish, 0 = neutral)
            double mainSignal = smmLite.Values[4][0];
            double trend_signal = smmLite.Values[6][0];
            double dotsSignal = smmDots.Values[1][0]; 
            
            // Debug output on first few bars
            if (CurrentBar < 25)
            {
                Print(string.Format("Bar {0}: mainSignal={1}, trend_signal={2}, dotsSignal={3}", 
                    CurrentBar, mainSignal, trend_signal, dotsSignal));
            }
            
            // Update trend_state based on trend_signal changes
            if (trend_signal == 1 && (CurrentBar == 21 || smmLite.Values[6][1] != 1))
            {
                trend_state = 1;
            }
            else if (trend_signal == -1 && (CurrentBar == 21 || smmLite.Values[6][1] != -1))
            {
                trend_state = -1;
            }
            
            // Armed signal logic using trend_signal
            if (mainSignal == 1 && trend_signal == 1)
                armedSignal = 1;
            else if (mainSignal == -1 && trend_signal == -1)
                armedSignal = -1;
            
            if (armedSignal == 1 && trend_signal != 1)
                armedSignal = 0;
            else if (armedSignal == -1 && trend_signal != -1)
                armedSignal = 0;

            Value[0] = 0;
            PlotBrushes[0][0] = Brushes.Transparent;
            
            // Fire signals and draw arrows on price panel
            if (armedSignal == 1 && dotsSignal == 1)
            {
                Value[0] = 1;
                PlotBrushes[0][0] = Brushes.LimeGreen;
                
                Print(string.Format("Bar {0}: BUY SIGNAL FIRED!", CurrentBar));
                
                NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont() { Size = 16, Bold = true };
                DrawOnPricePanel = true;	
                Draw.Text(this, Convert.ToString("Long") + Convert.ToString(CurrentBars[0]), false, @"🢁" + System.Environment.NewLine + "Long", 0, Low[0] + (-50 * TickSize), -8, Brushes.Green, myFont, TextAlignment.Center, Brushes.Green, Brushes.Pink, 10);
                DrawOnPricePanel = false;
                armedSignal = 0;
            }
            else if (armedSignal == -1 && dotsSignal == -1)
            {
                Value[0] = -1;
                PlotBrushes[0][0] = Brushes.Red;
                
                Print(string.Format("Bar {0}: SELL SIGNAL FIRED!", CurrentBar));
                
                NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont() { Size = 16, Bold = true };
                DrawOnPricePanel = true;
                Draw.Text(this, Convert.ToString("Short") + Convert.ToString(CurrentBars[0]), false, "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (50 * TickSize)), 8, Brushes.Red, myFont, TextAlignment.Center, Brushes.Red, Brushes.Pink, 10);
                DrawOnPricePanel = false;
                armedSignal = 0;
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SMMLiteCombinedSignal[] cacheSMMLiteCombinedSignal;
		public SMMLiteCombinedSignal SMMLiteCombinedSignal(int pS_ProfitTarget, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod)
		{
			return SMMLiteCombinedSignal(Input, pS_ProfitTarget, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod);
		}

		public SMMLiteCombinedSignal SMMLiteCombinedSignal(ISeries<double> input, int pS_ProfitTarget, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod)
		{
			if (cacheSMMLiteCombinedSignal != null)
				for (int idx = 0; idx < cacheSMMLiteCombinedSignal.Length; idx++)
					if (cacheSMMLiteCombinedSignal[idx] != null && cacheSMMLiteCombinedSignal[idx].PS_ProfitTarget == pS_ProfitTarget && cacheSMMLiteCombinedSignal[idx].SS_EnableChopFilter == sS_EnableChopFilter && cacheSMMLiteCombinedSignal[idx].IN_MaEnabled == iN_MaEnabled && cacheSMMLiteCombinedSignal[idx].IN_MaPeriod == iN_MaPeriod && cacheSMMLiteCombinedSignal[idx].IN_MaMethod == iN_MaMethod && cacheSMMLiteCombinedSignal[idx].EqualsInput(input))
						return cacheSMMLiteCombinedSignal[idx];
			return CacheIndicator<SMMLiteCombinedSignal>(new SMMLiteCombinedSignal(){ PS_ProfitTarget = pS_ProfitTarget, SS_EnableChopFilter = sS_EnableChopFilter, IN_MaEnabled = iN_MaEnabled, IN_MaPeriod = iN_MaPeriod, IN_MaMethod = iN_MaMethod }, input, ref cacheSMMLiteCombinedSignal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMMLiteCombinedSignal SMMLiteCombinedSignal(int pS_ProfitTarget, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod)
		{
			return indicator.SMMLiteCombinedSignal(Input, pS_ProfitTarget, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod);
		}

		public Indicators.SMMLiteCombinedSignal SMMLiteCombinedSignal(ISeries<double> input , int pS_ProfitTarget, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod)
		{
			return indicator.SMMLiteCombinedSignal(input, pS_ProfitTarget, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMMLiteCombinedSignal SMMLiteCombinedSignal(int pS_ProfitTarget, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod)
		{
			return indicator.SMMLiteCombinedSignal(Input, pS_ProfitTarget, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod);
		}

		public Indicators.SMMLiteCombinedSignal SMMLiteCombinedSignal(ISeries<double> input , int pS_ProfitTarget, bool sS_EnableChopFilter, bool iN_MaEnabled, int iN_MaPeriod, SmmCustom.EMovingAverageType iN_MaMethod)
		{
			return indicator.SMMLiteCombinedSignal(input, pS_ProfitTarget, sS_EnableChopFilter, iN_MaEnabled, iN_MaPeriod, iN_MaMethod);
		}
	}
}

#endregion
