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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

 public enum MadridMAType
    {
        SMA,
        EMA,
        WMA,
        TEMA,
        HMA,
        DEMA,
        TMA
    }

// ╔══════════════════════════════════════════════════════════════════════════════╗
// ║                                                                              ║
// ║ © Madrid : 141017TH2251                                                      ║
// ║                                                                              ║
// ║ Madrid Moving Average Ribbon                                                 ║
// ║                                                                              ║
// ║ This plots a moving average ribbon with configurable MA types.               ║
// ║ This study is best viewed with a dark background.  It provides an easy       ║
// ║ and fast way to determine the trend direction and possible reversals.        ║
// ║                                                                              ║
// ║ Lime : Uptrend. Long trading                                                 ║
// ║ Green : Reentry (buy the dip) or downtrend reversal warning                  ║
// ║ Red : Downtrend. Short trading                                               ║
// ║ Maroon : Short Reentry (sell the peak) or uptrend reversal warning           ║
// ║                                                                              ║
// ╚══════════════════════════════════════════════════════════════════════════════╝

namespace NinjaTrader.NinjaScript.Indicators
{
   

    public class MadridMovingAverageRibbon : Indicator
    {
        private Series<double> ma1;
        private Series<double> ma2;
        private Series<double> ma3;
        private Series<double> ma4;
        private Series<double> ma5;
        private Series<double> ma6;
        private Series<double> maRef;

        // Colors
        private Brush LIME = Brushes.Lime;
        private Brush GREEN = Brushes.Green;
        private Brush RUBI = Brushes.Red;
        private Brush MAROON = Brushes.Maroon;
        private Brush GRAY = Brushes.Gray;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Madrid Moving Average Ribbon";
                Name = "MadridRibbon";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                
                // Default input parameters
                MAType = MadridMAType.EMA;
                Length1 = 5;
                Length2 = 20;
                Length3 = 40;
                Length4 = 60;
                Length5 = 80;
                Length6 = 100;
                RefLength = 100;
            }
            else if (State == State.Configure)
            {
                // Create the series
                ma1 = new Series<double>(this);
                ma2 = new Series<double>(this);
                ma3 = new Series<double>(this);
                ma4 = new Series<double>(this);
                ma5 = new Series<double>(this);
                ma6 = new Series<double>(this);
                maRef = new Series<double>(this);
                
                // Add plots
                AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "MA1");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "MA2");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "MA3");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "MA4");
                AddPlot(new Stroke(Brushes.Lime, 1), PlotStyle.Line, "MA5");
                AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Line, "MA6");
            }
        }

        protected override void OnBarUpdate()
        {
            // Calculate MAs
            ma1[0] = CalculateMA(Close, Length1, MAType);
            ma2[0] = CalculateMA(Close, Length2, MAType);
            ma3[0] = CalculateMA(Close, Length3, MAType);
            ma4[0] = CalculateMA(Close, Length4, MAType);
            ma5[0] = CalculateMA(Close, Length5, MAType);
            ma6[0] = CalculateMA(Close, Length6, MAType);
            maRef[0] = CalculateMA(Close, RefLength, MAType);

            // Set plot colors
            PlotBrushes[0][0] = GetMAColor(ma1, maRef);
            PlotBrushes[1][0] = GetMAColor(ma2, maRef);
            PlotBrushes[2][0] = GetMAColor(ma3, maRef);
            PlotBrushes[3][0] = GetMAColor(ma4, maRef);
            PlotBrushes[4][0] = GetMAColor(ma5, maRef);
            PlotBrushes[5][0] = GetMAColor(ma6, maRef);

            // Set plot values
            Values[0][0] = ma1[0];
            Values[1][0] = ma2[0];
            Values[2][0] = ma3[0];
            Values[3][0] = ma4[0];
            Values[4][0] = ma5[0];
            Values[5][0] = ma6[0];
        }

        private double CalculateMA(ISeries<double> input, int period, MadridMAType maType)
        {
            switch (maType)
            {
                case MadridMAType.SMA:
                    return SMA(input, period)[0];
                case MadridMAType.EMA:
                    return EMA(input, period)[0];
                case MadridMAType.WMA:
                    return WMA(input, period)[0];
                case MadridMAType.TEMA:
                    return TEMA(input, period)[0];
                case MadridMAType.HMA:
                    return HMA(input, period)[0];
                case MadridMAType.DEMA:
                    return DEMA(input, period)[0];
                case MadridMAType.TMA:
                    return TMA(input, period)[0];
                default:
                    return SMA(input, period)[0];
            }
        }

        private Brush GetMAColor(Series<double> ma, Series<double> maRef)
        {
            if (CurrentBar < 1)
                return GRAY;

            double diffMA = ma[0] - ma[1];
            
            if (diffMA >= 0 && ma[0] > maRef[0])
                return LIME;
            else if (diffMA < 0 && ma[0] > maRef[0])
                return MAROON;
            else if (diffMA <= 0 && ma[0] < maRef[0])
                return RUBI;
            else if (diffMA >= 0 && ma[0] < maRef[0])
                return GREEN;
            else
                return GRAY;
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Moving Average Type", Description = "Type of moving average to use", Order = 1, GroupName = "Parameters")]
        public MadridMAType MAType { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MA Length 1", Description = "Length for MA 1", Order = 2, GroupName = "MA Lengths")]
        public int Length1 { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MA Length 2", Description = "Length for MA 2", Order = 3, GroupName = "MA Lengths")]
        public int Length2 { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MA Length 3", Description = "Length for MA 3", Order = 4, GroupName = "MA Lengths")]
        public int Length3 { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MA Length 4", Description = "Length for MA 4", Order = 5, GroupName = "MA Lengths")]
        public int Length4 { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MA Length 5", Description = "Length for MA 5", Order = 6, GroupName = "MA Lengths")]
        public int Length5 { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "MA Length 6", Description = "Length for MA 6", Order = 7, GroupName = "MA Lengths")]
        public int Length6 { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Reference MA Length", Description = "Length for reference MA used for coloring", Order = 8, GroupName = "MA Lengths")]
        public int RefLength { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MadridMovingAverageRibbon[] cacheMadridMovingAverageRibbon;
		public MadridMovingAverageRibbon MadridMovingAverageRibbon(MadridMAType mAType, int length1, int length2, int length3, int length4, int length5, int length6, int refLength)
		{
			return MadridMovingAverageRibbon(Input, mAType, length1, length2, length3, length4, length5, length6, refLength);
		}

		public MadridMovingAverageRibbon MadridMovingAverageRibbon(ISeries<double> input, MadridMAType mAType, int length1, int length2, int length3, int length4, int length5, int length6, int refLength)
		{
			if (cacheMadridMovingAverageRibbon != null)
				for (int idx = 0; idx < cacheMadridMovingAverageRibbon.Length; idx++)
					if (cacheMadridMovingAverageRibbon[idx] != null && cacheMadridMovingAverageRibbon[idx].MAType == mAType && cacheMadridMovingAverageRibbon[idx].Length1 == length1 && cacheMadridMovingAverageRibbon[idx].Length2 == length2 && cacheMadridMovingAverageRibbon[idx].Length3 == length3 && cacheMadridMovingAverageRibbon[idx].Length4 == length4 && cacheMadridMovingAverageRibbon[idx].Length5 == length5 && cacheMadridMovingAverageRibbon[idx].Length6 == length6 && cacheMadridMovingAverageRibbon[idx].RefLength == refLength && cacheMadridMovingAverageRibbon[idx].EqualsInput(input))
						return cacheMadridMovingAverageRibbon[idx];
			return CacheIndicator<MadridMovingAverageRibbon>(new MadridMovingAverageRibbon(){ MAType = mAType, Length1 = length1, Length2 = length2, Length3 = length3, Length4 = length4, Length5 = length5, Length6 = length6, RefLength = refLength }, input, ref cacheMadridMovingAverageRibbon);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MadridMovingAverageRibbon MadridMovingAverageRibbon(MadridMAType mAType, int length1, int length2, int length3, int length4, int length5, int length6, int refLength)
		{
			return indicator.MadridMovingAverageRibbon(Input, mAType, length1, length2, length3, length4, length5, length6, refLength);
		}

		public Indicators.MadridMovingAverageRibbon MadridMovingAverageRibbon(ISeries<double> input , MadridMAType mAType, int length1, int length2, int length3, int length4, int length5, int length6, int refLength)
		{
			return indicator.MadridMovingAverageRibbon(input, mAType, length1, length2, length3, length4, length5, length6, refLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MadridMovingAverageRibbon MadridMovingAverageRibbon(MadridMAType mAType, int length1, int length2, int length3, int length4, int length5, int length6, int refLength)
		{
			return indicator.MadridMovingAverageRibbon(Input, mAType, length1, length2, length3, length4, length5, length6, refLength);
		}

		public Indicators.MadridMovingAverageRibbon MadridMovingAverageRibbon(ISeries<double> input , MadridMAType mAType, int length1, int length2, int length3, int length4, int length5, int length6, int refLength)
		{
			return indicator.MadridMovingAverageRibbon(input, mAType, length1, length2, length3, length4, length5, length6, refLength);
		}
	}
}

#endregion
