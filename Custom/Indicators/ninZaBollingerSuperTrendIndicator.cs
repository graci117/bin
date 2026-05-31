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
    public class ninZaBollingerSuperTrendIndicator : Indicator
    {
        private ninZaBollingerReversal ninZaBollingerReversal1;
		private KeltnerChannel KeltnerChannel1;
		private Series<int> tradeSignal;
		private ninZaSuperTrendPro ninZaSuperTrendPro1;
		private ninZaPANAKanal ninZaPANAKanal1;

        // Exposed Series so other strategies/indicators can read Signal_Trade
        private Series<int> signalTrade;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description    = "Signal_Trade = 1 when BollingerReversal.UpperBand < Keltner.Upper; -1 when BollingerReversal.UpperBand > Keltner.Lower";
                Name           = "ninZaBollingerSuperTrendIndicator";
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
                ninZaBollingerReversal1 = ninZaBollingerReversal(
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
				
				//ninZaBollingerReversal1				= ninZaBollingerReversal(Close, ninZa_MAType.EMA, 14, true, ninZa_MAType.EMA, 5, 2, 1, 1);
				KeltnerChannel1				= KeltnerChannel(Close, 1.1, 20);
				ninZaSuperTrendPro1				= ninZaSuperTrendPro(Close, ninZa_MAType.EMA, NinjaTrader.Data.PriceType.Median, NinjaTrader.Data.PriceType.Median, 14, true, ninZa_MAType.SMA, 5, 1.5, 50);
				ninZaPANAKanal1				=  ninZaPANAKanal(Close, 20, 4, 14, 20, 10);
//				ninZaBollingerReversal1.Plots[0].Brush = Brushes.HotPink;
//				ninZaBollingerReversal1.Plots[1].Brush = Brushes.Orange;
//				ninZaBollingerReversal1.Plots[2].Brush = Brushes.DodgerBlue;
//				ninZaBollingerReversal1.Plots[3].Brush = Brushes.Transparent;
//				ninZaBollingerReversal1.Plots[4].Brush = Brushes.Transparent;
//				KeltnerChannel1.Plots[0].Brush = Brushes.DarkGray;
//				KeltnerChannel1.Plots[1].Brush = Brushes.Turquoise;
//				KeltnerChannel1.Plots[2].Brush = Brushes.Turquoise;
//				ninZaSuperTrendPro1.Plots[0].Brush = Brushes.Yellow;
//				ninZaSuperTrendPro1.Plots[1].Brush = Brushes.Transparent;
//				ninZaPANAKanal1.Plots[0].Brush = Brushes.Yellow;
//				ninZaPANAKanal1.Plots[1].Brush = Brushes.Goldenrod;
//				ninZaPANAKanal1.Plots[2].Brush = Brushes.Goldenrod;
//				ninZaPANAKanal1.Plots[3].Brush = Brushes.Transparent;
//				ninZaPANAKanal1.Plots[4].Brush = Brushes.Transparent;

                // Initialize KeltnerChannel: (1.1, 20)
                //keltner = KeltnerChannel(1.1, 20);

                tradeSignal = new Series<int>(this);

               
                // Add Signal_Trade plot
                
            }
        }

        protected override void OnBarUpdate()
        {
           if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			tradeSignal[0] = 0;

			 // Set 1
			if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] > ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] > 0)	
				 && (High[0] > High[1])
				 && (ninZaSuperTrendPro1.Signal_State[1] < 0)
				 && (ninZaSuperTrendPro1.Signal_State[0] > 0)
				&& tradeSignal[0] == 0
				&& !(ToTime(Time[0]) >= 92900 && ToTime(Time[0]) <= 95100)
				&& !(ToTime(Time[0]) >= 13590 && ToTime(Time[0]) <= 14160))
			{
				//Print(ToTime(Time[0]));
			//(	!(ToTime(Time[0]) >= 09290 && ToTime(Time[0]) <= 09510));
				//EnterLongLimit(Convert.ToInt32(DefaultQuantity), 0, @"STLong");
				tradeSignal[0] = 1;
			}			
			 // Set 2
			else if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] < ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] < 0)
				 && (High[0] > High[1])
				 && (ninZaSuperTrendPro1.Signal_State[1] >= 0)
				 && (ninZaSuperTrendPro1.Signal_State[0] < 0)
				 && tradeSignal[0] == 0
				&& !(ToTime(Time[0]) >= 92900 && ToTime(Time[0]) <= 95100)
				&& !(ToTime(Time[0]) >= 13590 && ToTime(Time[0]) <= 14160))
			{
				//EnterShortLimit(Convert.ToInt32(DefaultQuantity), 0, @"1");
				tradeSignal[0] = -1;
			}			
			 // Set 3
			 else if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] > ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] > 0)
				 && (High[0] > High[1])
				 && (ninZaPANAKanal1.Signal_Trade[0] > 0)
				 && (ninZaSuperTrendPro1.Signal_State[0] > 0)
				 && tradeSignal[0] == 0
				 && !(ToTime(Time[0]) >= 92900 && ToTime(Time[0]) <= 95100)
				&& !(ToTime(Time[0]) >= 13590 && ToTime(Time[0]) <= 14160))
			{
				//EnterLongLimit(Convert.ToInt32(DefaultQuantity), 0, @"STLong");
					//Print("here2ndTime[0])" +  ToTime(Time[0]) );
			//Print(	!(ToTime(Time[0]) >= 09290 && ToTime(Time[0]) <= 09510));
				tradeSignal[0] = 1;
			}
			
			 // Set 4
			else if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] < ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] < 0)
				 && (High[0] > High[1])
				 && (ninZaSuperTrendPro1.Signal_State[0] < 0)
				 && (ninZaPANAKanal1.Signal_Trade[0] < 0)
				&& tradeSignal[0] == 0
				&& !(ToTime(Time[0]) >= 92900 && ToTime(Time[0]) <= 95100)
				&& !(ToTime(Time[0]) >= 13590 && ToTime(Time[0]) <= 14160))
			{
				//EnterShortLimit(Convert.ToInt32(DefaultQuantity), 0, @"1");
				tradeSignal[0] = -1;
			}
			
		
          
        }

        #region Properties


				[Browsable(false)]
		[XmlIgnore]
		public Series<int> Trade_Signal
		{
		    get { return tradeSignal; }
			}

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
		private ninZaBollingerSuperTrendIndicator[] cacheninZaBollingerSuperTrendIndicator;
		public ninZaBollingerSuperTrendIndicator ninZaBollingerSuperTrendIndicator(ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return ninZaBollingerSuperTrendIndicator(Input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}

		public ninZaBollingerSuperTrendIndicator ninZaBollingerSuperTrendIndicator(ISeries<double> input, ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			if (cacheninZaBollingerSuperTrendIndicator != null)
				for (int idx = 0; idx < cacheninZaBollingerSuperTrendIndicator.Length; idx++)
					if (cacheninZaBollingerSuperTrendIndicator[idx] != null && cacheninZaBollingerSuperTrendIndicator[idx].BRMaType == bRMaType && cacheninZaBollingerSuperTrendIndicator[idx].BRPeriod == bRPeriod && cacheninZaBollingerSuperTrendIndicator[idx].BRSmoothingEnabled == bRSmoothingEnabled && cacheninZaBollingerSuperTrendIndicator[idx].BRSmoothingMaType == bRSmoothingMaType && cacheninZaBollingerSuperTrendIndicator[idx].BRSmoothingPeriod == bRSmoothingPeriod && cacheninZaBollingerSuperTrendIndicator[idx].BRStdDevOffset == bRStdDevOffset && cacheninZaBollingerSuperTrendIndicator[idx].BRMinProtrusion == bRMinProtrusion && cacheninZaBollingerSuperTrendIndicator[idx].EqualsInput(input))
						return cacheninZaBollingerSuperTrendIndicator[idx];
			return CacheIndicator<ninZaBollingerSuperTrendIndicator>(new ninZaBollingerSuperTrendIndicator(){ BRMaType = bRMaType, BRPeriod = bRPeriod, BRSmoothingEnabled = bRSmoothingEnabled, BRSmoothingMaType = bRSmoothingMaType, BRSmoothingPeriod = bRSmoothingPeriod, BRStdDevOffset = bRStdDevOffset, BRMinProtrusion = bRMinProtrusion }, input, ref cacheninZaBollingerSuperTrendIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ninZaBollingerSuperTrendIndicator ninZaBollingerSuperTrendIndicator(ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.ninZaBollingerSuperTrendIndicator(Input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}

		public Indicators.ninZaBollingerSuperTrendIndicator ninZaBollingerSuperTrendIndicator(ISeries<double> input , ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.ninZaBollingerSuperTrendIndicator(input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ninZaBollingerSuperTrendIndicator ninZaBollingerSuperTrendIndicator(ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.ninZaBollingerSuperTrendIndicator(Input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}

		public Indicators.ninZaBollingerSuperTrendIndicator ninZaBollingerSuperTrendIndicator(ISeries<double> input , ninZa_MAType bRMaType, int bRPeriod, bool bRSmoothingEnabled, ninZa_MAType bRSmoothingMaType, int bRSmoothingPeriod, double bRStdDevOffset, double bRMinProtrusion)
		{
			return indicator.ninZaBollingerSuperTrendIndicator(input, bRMaType, bRPeriod, bRSmoothingEnabled, bRSmoothingMaType, bRSmoothingPeriod, bRStdDevOffset, bRMinProtrusion);
		}
	}
}

#endregion
