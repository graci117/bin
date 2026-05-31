#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This work is licensed under Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International
// https://creativecommons.org/licenses/by-nc-sa/4.0/
// © BigBeluga (NinjaTrader port)

namespace NinjaTrader.NinjaScript.Indicators
{
    public class TwoPoleOscillator_BigBeluga : Indicator
    {
        private Series<double> smooth1;
        private Series<double> smooth2;
        private Series<double> smaN1;
        private Series<double> twoP;
        private Series<double> twoPP;
        private SMA sma25;
        private SMA smaDeviation;
        private StdDev stdDeviation;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Two-Pole Oscillator [BigBeluga] - Normalized momentum oscillator with two-pole filtering";
                Name = "TwoPoleOscillator_BigBeluga";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                Length = 20;
                BuyDotColor = Brushes.Aqua;
                SellDotColor = Brushes.MediumPurple;
                
                AddPlot(Brushes.Cyan, "TwoPole");
                AddPlot(Brushes.DarkGray, "TwoPoleLag");
                
                AddLine(Brushes.Gray, 1.0, "UpperZone");
                AddLine(Brushes.Gray, 0.5, "MidUpper");
                AddLine(Brushes.Gray, 0, "ZeroLine");
                AddLine(Brushes.Gray, -0.5, "MidLower");
                AddLine(Brushes.Gray, -1.0, "LowerZone");
            }
            else if (State == State.Configure)
            {
            }
            else if (State == State.DataLoaded)
            {
                smooth1 = new Series<double>(this);
                smooth2 = new Series<double>(this);
                smaN1 = new Series<double>(this);
                twoP = new Series<double>(this);
                twoPP = new Series<double>(this);
                
                sma25 = SMA(Close, 25);
                smaDeviation = SMA(25);
                stdDeviation = StdDev(25);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 25)
            {
                Values[0][0] = 0;
                Values[1][0] = 0;
                smooth1[0] = 0;
                smooth2[0] = 0;
                smaN1[0] = 0;
                twoP[0] = 0;
                twoPP[0] = 0;
                return;
            }

            // 1. Base detrending + normalization
            double sma1 = sma25[0];
            double deviation = Close[0] - sma1;
            
            // Calculate SMA and StdDev of the deviation
            double sumDev = 0;
            double sumSqDev = 0;
            for (int i = 0; i < 25; i++)
            {
                double dev = Close[i] - SMA(Close, 25)[i];
                sumDev += dev;
                sumSqDev += dev * dev;
            }
            double avgDev = sumDev / 25.0;
            double variance = (sumSqDev / 25.0) - (avgDev * avgDev);
            double stdDev = Math.Sqrt(Math.Max(0, variance));
            
            smaN1[0] = (stdDev != 0) ? ((deviation - avgDev) / stdDev) : 0;

            // 2. Two-Pole Filter (EMA-like smoothing applied twice)
            double alpha = 2.0 / (Length + 1.0);
            
            if (CurrentBar == 25)
            {
                smooth1[0] = smaN1[0];
                smooth2[0] = smooth1[0];
            }
            else
            {
                smooth1[0] = (1 - alpha) * smooth1[1] + alpha * smaN1[0];
                smooth2[0] = (1 - alpha) * smooth2[1] + alpha * smooth1[0];
            }
            
            twoP[0] = smooth2[0];
            twoPP[0] = (CurrentBar >= 4) ? twoP[4] : twoP[0];

            // 3. Buy/Sell signal detection
            if (CurrentBar > 0)
            {
                bool buy = CrossAbove(twoP, twoPP, 1) && twoP[0] < 0;
                bool sell = CrossBelow(twoP, twoPP, 1) && twoP[0] > 0;
                
                if (buy)
                {
                    Draw.Dot(this, "buy_" + CurrentBar, false, 0, twoP[0], BuyDotColor);
                }
                else if (sell)
                {
                    Draw.Dot(this, "sell_" + CurrentBar, false, 0, twoP[0], SellDotColor);
                }
            }

            // 4. Plot values
            Values[0][0] = twoP[0];
            Values[1][0] = twoPP[0];
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Filter Length", Description = "Length of the two-pole filter", Order = 1, GroupName = "Parameters")]
        public int Length { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Buy Signal Color", Description = "Color for buy signal dots", Order = 2, GroupName = "Parameters")]
        public Brush BuyDotColor { get; set; }

        [Browsable(false)]
        public string BuyDotColorSerializable
        {
            get { return Serialize.BrushToString(BuyDotColor); }
            set { BuyDotColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Sell Signal Color", Description = "Color for sell signal dots", Order = 3, GroupName = "Parameters")]
        public Brush SellDotColor { get; set; }

        [Browsable(false)]
        public string SellDotColorSerializable
        {
            get { return Serialize.BrushToString(SellDotColor); }
            set { SellDotColor = Serialize.StringToBrush(value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TwoPoleOscillator_BigBeluga[] cacheTwoPoleOscillator_BigBeluga;
		public TwoPoleOscillator_BigBeluga TwoPoleOscillator_BigBeluga(int length, Brush buyDotColor, Brush sellDotColor)
		{
			return TwoPoleOscillator_BigBeluga(Input, length, buyDotColor, sellDotColor);
		}

		public TwoPoleOscillator_BigBeluga TwoPoleOscillator_BigBeluga(ISeries<double> input, int length, Brush buyDotColor, Brush sellDotColor)
		{
			if (cacheTwoPoleOscillator_BigBeluga != null)
				for (int idx = 0; idx < cacheTwoPoleOscillator_BigBeluga.Length; idx++)
					if (cacheTwoPoleOscillator_BigBeluga[idx] != null && cacheTwoPoleOscillator_BigBeluga[idx].Length == length && cacheTwoPoleOscillator_BigBeluga[idx].BuyDotColor == buyDotColor && cacheTwoPoleOscillator_BigBeluga[idx].SellDotColor == sellDotColor && cacheTwoPoleOscillator_BigBeluga[idx].EqualsInput(input))
						return cacheTwoPoleOscillator_BigBeluga[idx];
			return CacheIndicator<TwoPoleOscillator_BigBeluga>(new TwoPoleOscillator_BigBeluga(){ Length = length, BuyDotColor = buyDotColor, SellDotColor = sellDotColor }, input, ref cacheTwoPoleOscillator_BigBeluga);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TwoPoleOscillator_BigBeluga TwoPoleOscillator_BigBeluga(int length, Brush buyDotColor, Brush sellDotColor)
		{
			return indicator.TwoPoleOscillator_BigBeluga(Input, length, buyDotColor, sellDotColor);
		}

		public Indicators.TwoPoleOscillator_BigBeluga TwoPoleOscillator_BigBeluga(ISeries<double> input , int length, Brush buyDotColor, Brush sellDotColor)
		{
			return indicator.TwoPoleOscillator_BigBeluga(input, length, buyDotColor, sellDotColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TwoPoleOscillator_BigBeluga TwoPoleOscillator_BigBeluga(int length, Brush buyDotColor, Brush sellDotColor)
		{
			return indicator.TwoPoleOscillator_BigBeluga(Input, length, buyDotColor, sellDotColor);
		}

		public Indicators.TwoPoleOscillator_BigBeluga TwoPoleOscillator_BigBeluga(ISeries<double> input , int length, Brush buyDotColor, Brush sellDotColor)
		{
			return indicator.TwoPoleOscillator_BigBeluga(input, length, buyDotColor, sellDotColor);
		}
	}
}

#endregion
