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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class ChopToTrend : Indicator
    {
        #region Variables
        private int period = 14;
        private double chopThreshold = 1;  // Threshold to identify chop
        private double chopValue = 0.0;
        private Brush chopColor = Brushes.Yellow;
        private Brush uptrendColor = Brushes.Green;
        private Brush downtrendColor = Brushes.Red;
        private bool paintBars = true;  // Flag to enable or disable paint bars
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"UniRenko Chop Indicator to identify choppy market conditions.";
                Name = "ChopToTrend";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                AddPlot(new Stroke(Brushes.Transparent), PlotStyle.Bar, "ChopIndicator");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < period)
            {
                Values[0][0] = 0;
                return;
            }

            double highestHigh = High[HighestBar(High, period)];
            double lowestLow = Low[LowestBar(Low, period)];
            double range = highestHigh - lowestLow;
            double totalMovement = 0.0;

            for (int i = 0; i < period; i++)
            {
                totalMovement += Math.Abs(Close[i] - Close[i + 1]);
            }

            chopValue = totalMovement / range;

            Values[0][0] = chopValue;

            if (chopValue > chopThreshold)
            {
                PlotBrushes[0][0] = chopColor;
                if (paintBars)
                {
                    BarBrush = chopColor;
                    CandleOutlineBrush = chopColor;
                }
            }
            else
            {
                if (Close[0] > Open[0])
                {
                    PlotBrushes[0][0] = uptrendColor;
                    if (paintBars)
                    {
                        BarBrush = uptrendColor;
                        CandleOutlineBrush = uptrendColor;
                    }
                }
                else
                {
                    PlotBrushes[0][0] = downtrendColor;
                    if (paintBars)
                    {
                        BarBrush = downtrendColor;
                        CandleOutlineBrush = downtrendColor;
                    }
                }
            }
        }

        #region Properties
        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ChopIndicator
        {
            get { return Values[0]; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Period", Description = "Period for calculating the chop indicator", Order = 0, GroupName = "Parameters")]
        public int Period
        {
            get { return period; }
            set { period = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Chop Threshold", Description = "Threshold to identify chop conditions", Order = 1, GroupName = "Parameters")]
        public double ChopThreshold
        {
            get { return chopThreshold; }
            set { chopThreshold = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Paint Bars", Description = "Enable or disable painting bars based on chop conditions", Order = 2, GroupName = "Parameters")]
        public bool PaintBars
        {
            get { return paintBars; }
            set { paintBars = value; }
        }

        [XmlIgnore]
        [Display(Name = "Chop Color", Description = "Color for chop conditions", Order = 3, GroupName = "Plot Colors")]
        public Brush ChopColor
        {
            get { return chopColor; }
            set { chopColor = value; }
        }

        [Browsable(false)]
        public string ChopColorSerialize
        {
            get { return Serialize.BrushToString(chopColor); }
            set { chopColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Uptrend Color", Description = "Color for uptrend conditions", Order = 4, GroupName = "Plot Colors")]
        public Brush UptrendColor
        {
            get { return uptrendColor; }
            set { uptrendColor = value; }
        }

        [Browsable(false)]
        public string UptrendColorSerialize
        {
            get { return Serialize.BrushToString(uptrendColor); }
            set { uptrendColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Downtrend Color", Description = "Color for downtrend conditions", Order = 5, GroupName = "Plot Colors")]
        public Brush DowntrendColor
        {
            get { return downtrendColor; }
            set { downtrendColor = value; }
        }

        [Browsable(false)]
        public string DowntrendColorSerialize
        {
            get { return Serialize.BrushToString(downtrendColor); }
            set { downtrendColor = Serialize.StringToBrush(value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChopToTrend[] cacheChopToTrend;
		public ChopToTrend ChopToTrend(int period, double chopThreshold, bool paintBars)
		{
			return ChopToTrend(Input, period, chopThreshold, paintBars);
		}

		public ChopToTrend ChopToTrend(ISeries<double> input, int period, double chopThreshold, bool paintBars)
		{
			if (cacheChopToTrend != null)
				for (int idx = 0; idx < cacheChopToTrend.Length; idx++)
					if (cacheChopToTrend[idx] != null && cacheChopToTrend[idx].Period == period && cacheChopToTrend[idx].ChopThreshold == chopThreshold && cacheChopToTrend[idx].PaintBars == paintBars && cacheChopToTrend[idx].EqualsInput(input))
						return cacheChopToTrend[idx];
			return CacheIndicator<ChopToTrend>(new ChopToTrend(){ Period = period, ChopThreshold = chopThreshold, PaintBars = paintBars }, input, ref cacheChopToTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChopToTrend ChopToTrend(int period, double chopThreshold, bool paintBars)
		{
			return indicator.ChopToTrend(Input, period, chopThreshold, paintBars);
		}

		public Indicators.ChopToTrend ChopToTrend(ISeries<double> input , int period, double chopThreshold, bool paintBars)
		{
			return indicator.ChopToTrend(input, period, chopThreshold, paintBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChopToTrend ChopToTrend(int period, double chopThreshold, bool paintBars)
		{
			return indicator.ChopToTrend(Input, period, chopThreshold, paintBars);
		}

		public Indicators.ChopToTrend ChopToTrend(ISeries<double> input , int period, double chopThreshold, bool paintBars)
		{
			return indicator.ChopToTrend(input, period, chopThreshold, paintBars);
		}
	}
}

#endregion
