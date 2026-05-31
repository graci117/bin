//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class MACDWithCustomHistogramColors : Indicator
    {
        private Series<double> fastEma;
        private Series<double> slowEma;
        private double constant1;
        private double constant2;
        private double constant3;
        private double constant4;
        private double constant5;
        private double constant6;

        [XmlIgnore]
        [Display(ResourceType = typeof(NinjaTrader.Custom.Resource), Name = "AscendingAboveZeroColor", GroupName = "Colors", Order = 0)]
        public Brush AscendingAboveZeroColor { get; set; }

        [Browsable(false)]
        public string AscendingAboveZeroColorSerializable
        {
            get { return Serialize.BrushToString(AscendingAboveZeroColor); }
            set { AscendingAboveZeroColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(ResourceType = typeof(NinjaTrader.Custom.Resource), Name = "DescendingAboveZeroColor", GroupName = "Colors", Order = 1)]
        public Brush DescendingAboveZeroColor { get; set; }

        [Browsable(false)]
        public string DescendingAboveZeroColorSerializable
        {
            get { return Serialize.BrushToString(DescendingAboveZeroColor); }
            set { DescendingAboveZeroColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(ResourceType = typeof(NinjaTrader.Custom.Resource), Name = "AscendingBelowZeroColor", GroupName = "Colors", Order = 2)]
        public Brush AscendingBelowZeroColor { get; set; }

        [Browsable(false)]
        public string AscendingBelowZeroColorSerializable
        {
            get { return Serialize.BrushToString(AscendingBelowZeroColor); }
            set { AscendingBelowZeroColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(ResourceType = typeof(NinjaTrader.Custom.Resource), Name = "DescendingBelowZeroColor", GroupName = "Colors", Order = 3)]
        public Brush DescendingBelowZeroColor { get; set; }

        [Browsable(false)]
        public string DescendingBelowZeroColorSerializable
        {
            get { return Serialize.BrushToString(DescendingBelowZeroColor); }
            set { DescendingBelowZeroColor = Serialize.StringToBrush(value); }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "MACD with customizable histogram colors.";
                Name = "MACDWithCustomHistogramColors";
                Fast = 12;
                Slow = 26;
                Smooth = 9;

                AscendingAboveZeroColor = Brushes.Lime;
                DescendingAboveZeroColor = Brushes.DarkGreen;
                AscendingBelowZeroColor = Brushes.Orange;
                DescendingBelowZeroColor = Brushes.Red;

                AddPlot(Brushes.DarkCyan, "MACD");
                AddPlot(Brushes.Crimson, "Signal");
                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "Histogram");
                AddLine(Brushes.DarkGray, 0, "ZeroLine");
            }
            else if (State == State.Configure)
            {
                constant1 = 2.0 / (1 + Fast);
                constant2 = (1 - (2.0 / (1 + Fast)));
                constant3 = 2.0 / (1 + Slow);
                constant4 = (1 - (2.0 / (1 + Slow)));
                constant5 = 2.0 / (1 + Smooth);
                constant6 = (1 - (2.0 / (1 + Smooth)));
            }
            else if (State == State.DataLoaded)
            {
                fastEma = new Series<double>(this);
                slowEma = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar == 0)
            {
                fastEma[0] = Input[0];
                slowEma[0] = Input[0];
                Value[0] = 0;
                Avg[0] = 0;
                Diff[0] = 0;
                return;
            }

            fastEma[0] = constant1 * Input[0] + constant2 * fastEma[1];
            slowEma[0] = constant3 * Input[0] + constant4 * slowEma[1];
            Value[0] = fastEma[0] - slowEma[0];
            Avg[0] = constant5 * Value[0] + constant6 * Avg[1];
            Diff[0] = Value[0] - Avg[0];

            // Set the histogram bar colors
            if (Diff[0] > 0)
                PlotBrushes[2][0] = Diff[0] > Diff[1] ? AscendingAboveZeroColor : DescendingAboveZeroColor;
            else
                PlotBrushes[2][0] = Diff[0] > Diff[1] ? AscendingBelowZeroColor : DescendingBelowZeroColor;
        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Avg
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Diff
        {
            get { return Values[2]; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Fast", GroupName = "Parameters", Order = 0)]
        public int Fast { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Slow", GroupName = "Parameters", Order = 1)]
        public int Slow { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Smooth", GroupName = "Parameters", Order = 2)]
        public int Smooth { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDWithCustomHistogramColors[] cacheMACDWithCustomHistogramColors;
		public MACDWithCustomHistogramColors MACDWithCustomHistogramColors(int fast, int slow, int smooth)
		{
			return MACDWithCustomHistogramColors(Input, fast, slow, smooth);
		}

		public MACDWithCustomHistogramColors MACDWithCustomHistogramColors(ISeries<double> input, int fast, int slow, int smooth)
		{
			if (cacheMACDWithCustomHistogramColors != null)
				for (int idx = 0; idx < cacheMACDWithCustomHistogramColors.Length; idx++)
					if (cacheMACDWithCustomHistogramColors[idx] != null && cacheMACDWithCustomHistogramColors[idx].Fast == fast && cacheMACDWithCustomHistogramColors[idx].Slow == slow && cacheMACDWithCustomHistogramColors[idx].Smooth == smooth && cacheMACDWithCustomHistogramColors[idx].EqualsInput(input))
						return cacheMACDWithCustomHistogramColors[idx];
			return CacheIndicator<MACDWithCustomHistogramColors>(new MACDWithCustomHistogramColors(){ Fast = fast, Slow = slow, Smooth = smooth }, input, ref cacheMACDWithCustomHistogramColors);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDWithCustomHistogramColors MACDWithCustomHistogramColors(int fast, int slow, int smooth)
		{
			return indicator.MACDWithCustomHistogramColors(Input, fast, slow, smooth);
		}

		public Indicators.MACDWithCustomHistogramColors MACDWithCustomHistogramColors(ISeries<double> input , int fast, int slow, int smooth)
		{
			return indicator.MACDWithCustomHistogramColors(input, fast, slow, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDWithCustomHistogramColors MACDWithCustomHistogramColors(int fast, int slow, int smooth)
		{
			return indicator.MACDWithCustomHistogramColors(Input, fast, slow, smooth);
		}

		public Indicators.MACDWithCustomHistogramColors MACDWithCustomHistogramColors(ISeries<double> input , int fast, int slow, int smooth)
		{
			return indicator.MACDWithCustomHistogramColors(input, fast, slow, smooth);
		}
	}
}

#endregion
