#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Xml.Serialization;
#endregion



namespace NinjaTrader.NinjaScript.Indicators
{
    public class BollingerKeltnerSignal : Indicator
    {
        private ninZaBollingerReversal bollingerReversal;
        private KeltnerChannel keltner;

        // Exposed Series so other strategies/indicators can read Signal_Trade
        private Series<int> signalTrade;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description    = "Signal_Trade = 1 when BollingerReversal.UpperBand < Keltner.Upper; -1 when BollingerReversal.UpperBand > Keltner.Lower";
                Name           = "BollingerKeltnerSignal";
                Calculate      = Calculate.OnBarClose;
                IsOverlay      = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
				this.ShowTransparentPlotsInDataBox = true;
				
				
				 BRMaType                      = ninZa_MAType.EMA;
                BRPeriod                      = 30;
                BRStdDevOffset                = 2.0;
                BRMinProtrusion               = 1.0;

                BRSmoothingEnabled            = true;
                BRSmoothingMaType             = ninZa_MAType.WilderMA;
                BRSmoothingPeriod             = 6;
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "Signal_Trade");
				
            }
            else if (State == State.DataLoaded)
            {
                // Initialize ninZaBollingerReversal: (EMA, 14, True, EMA, 5, 2, 1, 1)
                bollingerReversal = ninZaBollingerReversal(
                    Close,
                    BRMaType,  // MA type
                    BRPeriod,       // Period
                    true,     // Bool param
                    BRSmoothingMaType,  // 2nd MA type
                    BRSmoothingPeriod,        // 2nd Period
                    BRStdDevOffset,        // Multiplier (double → 2)
                    BRMinProtrusion,        // Param 7
                    1         // Param 8
                );

                // Initialize KeltnerChannel: (1.1, 20)
                keltner = KeltnerChannel(1.1, 20);

                signalTrade = new Series<int>(this);

               
                // Add Signal_Trade plot
                
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 20) return;

            double bbUpper     = bollingerReversal.UpperBand[0];
			double bbLower     = bollingerReversal.LowerBand[0];
            double keltnerUpper = keltner.Upper[0];
            double keltnerLower = keltner.Lower[0];

            int signal = 0;

            // Condition 1: UpperBand < Keltner.Upper  → Signal = 1
            if (bbUpper > keltnerUpper && bbLower < keltnerLower)
                signal = -1;

            // Condition 2: UpperBand > Keltner.Lower  → Signal = -1
            // (if both are true simultaneously, condition 2 overrides — adjust priority as needed)
            if (bbLower > keltnerLower && (bbUpper < keltnerUpper))
                signal = 1;

            signalTrade[0] = signal;
            Values[0][0]   = signal;  // Plot value

            // Color the bar for easy visual reading
          
        }

        #region Properties

        [Browsable(false)]
        [XmlIgnore]
        public Series<int> SignalTrade => signalTrade;

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SignalPlot => Values[0];
		
		
		       [NinjaScriptProperty]
        [Display(Name = "MA Type", Order = 1, GroupName = "Bollinger Reversal")]
        public ninZa_MAType BRMaType { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Period", Order = 2, GroupName = "Bollinger Reversal")]
        public int BRPeriod { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Smoothing: Enabled", Order = 3, GroupName = "Bollinger Reversal")]
        public bool BRSmoothingEnabled { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Smoothing: Method", Order = 4, GroupName = "Bollinger Reversal")]
        public ninZa_MAType BRSmoothingMaType { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Smoothing: Period", Order = 5, GroupName = "Bollinger Reversal")]
        public int BRSmoothingPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Offset (StdDev)", Order = 6, GroupName = "Bollinger Reversal")]
        public double BRStdDevOffset { get; set; }

        [NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Minimum Protrusion", Order = 7, GroupName = "Bollinger Reversal")]
        public double BRMinProtrusion { get; set; }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerKeltnerSignal[] cacheBollingerKeltnerSignal;
		public BollingerKeltnerSignal BollingerKeltnerSignal(ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return BollingerKeltnerSignal(Input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}

		public BollingerKeltnerSignal BollingerKeltnerSignal(ISeries<double> input, ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			if (cacheBollingerKeltnerSignal != null)
				for (int idx = 0; idx < cacheBollingerKeltnerSignal.Length; idx++)
					if (cacheBollingerKeltnerSignal[idx] != null && cacheBollingerKeltnerSignal[idx].BRMaType == bRMaType && cacheBollingerKeltnerSignal[idx].BRPeriod == bRPeriod && cacheBollingerKeltnerSignal[idx].BRSmoothingEnabled == bRSmoothingEnabled && cacheBollingerKeltnerSignal[idx].BRSmoothingMaType == bRSmoothingMaType && cacheBollingerKeltnerSignal[idx].BRSmoothingPeriod == bRSmoothingPeriod && cacheBollingerKeltnerSignal[idx].BRStdDevOffset == bRStdDevOffset && cacheBollingerKeltnerSignal[idx].BRMinProtrusion == bRMinProtrusion && cacheBollingerKeltnerSignal[idx].EqualsInput(input))
						return cacheBollingerKeltnerSignal[idx];
			return CacheIndicator<BollingerKeltnerSignal>(new BollingerKeltnerSignal(){ BRMaType = bRMaType, BRPeriod = bRPeriod, BRSmoothingEnabled = bRSmoothingEnabled, BRSmoothingMaType = bRSmoothingMaType, BRSmoothingPeriod = bRSmoothingPeriod, BRStdDevOffset = bRStdDevOffset, BRMinProtrusion = bRMinProtrusion }, input, ref cacheBollingerKeltnerSignal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerKeltnerSignal BollingerKeltnerSignal(ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.BollingerKeltnerSignal(Input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}

		public Indicators.BollingerKeltnerSignal BollingerKeltnerSignal(ISeries<double> input , ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.BollingerKeltnerSignal(input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerKeltnerSignal BollingerKeltnerSignal(ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.BollingerKeltnerSignal(Input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}

		public Indicators.BollingerKeltnerSignal BollingerKeltnerSignal(ISeries<double> input , ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.BollingerKeltnerSignal(input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}
	}
}

#endregion
