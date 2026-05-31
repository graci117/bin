#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class MACDSupportResistance : Indicator
    {
        private EMA fastEMA;
        private EMA slowEMA;
        private EMA signalEMA;
        private SMA fastSMA;
        private SMA slowSMA;
        private SMA signalSMA;
        
        private Series<double> macdSeries;
        private List<SRLevel> srLevels = new List<SRLevel>();
         private int currentTrend;
        private int previousTrend;
		
        private class SRLevel
        {
            public bool IsSupport { get; set; }
            public double Level { get; set; }
            public int StartBar { get; set; }
        }
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"MACD Support and Resistance Indicator";
                Name = "MACDSupportResistance";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                FastLength = 12;
                SlowLength = 26;
                SignalLength = 9;
                UseEMAForOscillator = true;
                UseEMAForSignal = true;
                MaxLevels = 20;
                
                UpColor = Brushes.DodgerBlue;
                DownColor = Brushes.OrangeRed;
                
                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "MACD");
                AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Line, "Signal");
                AddLine(new Stroke(Brushes.Gray, 1), 0, "ZeroLine");
				AddPlot(Brushes.LightPink, "TrendValue");
            }
            else if (State == State.Configure)
            {
                macdSeries = new Series<double>(this);
            }
            else if (State == State.DataLoaded)
            {
                if (UseEMAForOscillator)
                {
                    fastEMA = EMA(Close, FastLength);
                    slowEMA = EMA(Close, SlowLength);
                }
                else
                {
                    fastSMA = SMA(Close, FastLength);
                    slowSMA = SMA(Close, SlowLength);
                }
                
                if (UseEMAForSignal)
                    signalEMA = EMA(macdSeries, SignalLength);
                else
                    signalSMA = SMA(macdSeries, SignalLength);
				
				 currentTrend = 0;
                previousTrend = 0;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(FastLength, SlowLength))
                return;
            
            // Calculate MACD
            double fastMA = UseEMAForOscillator ? fastEMA[0] : fastSMA[0];
            double slowMA = UseEMAForOscillator ? slowEMA[0] : slowSMA[0];
            double macdValue = fastMA - slowMA;
            
            macdSeries[0] = macdValue;
            
            if (CurrentBar < Math.Max(FastLength, SlowLength) + SignalLength)
                return;
			
			// Store previous trend
            previousTrend = currentTrend;
            
            // Calculate Signal
            double signalValue = UseEMAForSignal ? signalEMA[0] : signalSMA[0];
            
            Values[0][0] = macdValue;
            Values[1][0] = signalValue;
            
			if (macdSeries[0] > signalValue)
				currentTrend = 1;			
			else if (macdSeries[0] < signalValue)
				currentTrend = -1;
			else
				currentTrend = 0;
			
			Values[2][0] = currentTrend;
			
            // Set plot colors
            Brush currentColor = macdValue > signalValue ? UpColor : DownColor;
            PlotBrushes[0][0] = currentColor;
            PlotBrushes[1][0] = currentColor;
            
            if (CurrentBar < 10)
                return;
            
            // Check for bearish crossunder
            if (macdSeries[1] >= signalValue && macdSeries[0] < signalValue)
            {
                double maxHigh = High[0];
                int maxBarIndex = 0;
                
                for (int i = 1; i <= 5 && i <= CurrentBar; i++)
                {
                    if (High[i] > maxHigh)
                    {
                        maxHigh = High[i];
                        maxBarIndex = i;
                    }
                }
                
                SRLevel newLevel = new SRLevel
                {
                    IsSupport = false,
                    Level = maxHigh,
                    StartBar = CurrentBar - maxBarIndex
                };
                srLevels.Add(newLevel);
                
                Draw.Diamond(this, "BearishCross" + CurrentBar, false, 0, signalValue, DownColor);
            }
            
            // Check for bullish crossover
            if (macdSeries[1] <= signalValue && macdSeries[0] > signalValue)
            {
                double minLow = Low[0];
                int minBarIndex = 0;
                
                for (int i = 1; i <= 5 && i <= CurrentBar; i++)
                {
                    if (Low[i] < minLow)
                    {
                        minLow = Low[i];
                        minBarIndex = i;
                    }
                }
                
                SRLevel newLevel = new SRLevel
                {
                    IsSupport = true,
                    Level = minLow,
                    StartBar = CurrentBar - minBarIndex
                };
                srLevels.Add(newLevel);
                
                Draw.Diamond(this, "BullishCross" + CurrentBar, false, 0, signalValue, UpColor);
            }
            
            // Limit to max levels
            while (srLevels.Count > MaxLevels)
            {
                srLevels.RemoveAt(0);
            }
            
            // Remove crossed levels and draw active ones
            for (int i = srLevels.Count - 1; i >= 0; i--)
            {
                SRLevel sr = srLevels[i];
                int barsAgo = CurrentBar - sr.StartBar;
                
                if (barsAgo < 0)
                    continue;
                
                // Remove if crossed
                if (sr.IsSupport && Low[0] < sr.Level)
                {
                    srLevels.RemoveAt(i);
                    continue;
                }
                
                if (!sr.IsSupport && High[0] > sr.Level)
                {
                    srLevels.RemoveAt(i);
                    continue;
                }
                
                // Draw line and labels
                Brush lineColor = sr.IsSupport ? UpColor : DownColor;
                string tag = "SR_" + sr.StartBar;
                
                Draw.Line(this, tag, false, barsAgo, sr.Level, 0, sr.Level, lineColor, DashStyleHelper.Solid, 1);
                Draw.Diamond(this, tag + "_diamond", false, barsAgo, sr.Level, lineColor);
                Draw.Text(this, tag + "_text", false, sr.Level.ToString("F2"), 0, sr.Level, 5, lineColor, 
                    new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10), System.Windows.TextAlignment.Left, Brushes.Transparent, Brushes.Transparent, 0);
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Fast Length", Description="Fast EMA/SMA Length", Order=1, GroupName="Parameters")]
        public int FastLength { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="Slow Length", Description="Slow EMA/SMA Length", Order=2, GroupName="Parameters")]
        public int SlowLength { get; set; }

        [NinjaScriptProperty]
        [Range(1, 50)]
        [Display(Name="Signal Length", Description="Signal Line Length", Order=3, GroupName="Parameters")]
        public int SignalLength { get; set; }

        [NinjaScriptProperty]
        [Display(Name="Use EMA for Oscillator", Description="Use EMA (true) or SMA (false)", Order=4, GroupName="Parameters")]
        public bool UseEMAForOscillator { get; set; }

        [NinjaScriptProperty]
        [Display(Name="Use EMA for Signal", Description="Use EMA (true) or SMA (false)", Order=5, GroupName="Parameters")]
        public bool UseEMAForSignal { get; set; }

        [Range(1, 50)]
        [Display(Name="Max Levels", Description="Maximum S/R levels to display", Order=6, GroupName="Parameters")]
        public int MaxLevels { get; set; }

        [XmlIgnore]
        [Display(Name="Up Color", Description="Bullish color", Order=1, GroupName="Visual")]
        public Brush UpColor { get; set; }
        
        [Browsable(false)]
        public string UpColorSerializable
        {
            get { return Serialize.BrushToString(UpColor); }
            set { UpColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name="Down Color", Description="Bearish color", Order=2, GroupName="Visual")]
        public Brush DownColor { get; set; }
        
        [Browsable(false)]
        public string DownColorSerializable
        {
            get { return Serialize.BrushToString(DownColor); }
            set { DownColor = Serialize.StringToBrush(value); }
        }
		
		 [Browsable(false)]
        [XmlIgnore]
        public Series<double> TrendValue
        {
            get { return Values[2]; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDSupportResistance[] cacheMACDSupportResistance;
		public MACDSupportResistance MACDSupportResistance(int fastLength, int slowLength, int signalLength, bool useEMAForOscillator, bool useEMAForSignal)
		{
			return MACDSupportResistance(Input, fastLength, slowLength, signalLength, useEMAForOscillator, useEMAForSignal);
		}

		public MACDSupportResistance MACDSupportResistance(ISeries<double> input, int fastLength, int slowLength, int signalLength, bool useEMAForOscillator, bool useEMAForSignal)
		{
			if (cacheMACDSupportResistance != null)
				for (int idx = 0; idx < cacheMACDSupportResistance.Length; idx++)
					if (cacheMACDSupportResistance[idx] != null && cacheMACDSupportResistance[idx].FastLength == fastLength && cacheMACDSupportResistance[idx].SlowLength == slowLength && cacheMACDSupportResistance[idx].SignalLength == signalLength && cacheMACDSupportResistance[idx].UseEMAForOscillator == useEMAForOscillator && cacheMACDSupportResistance[idx].UseEMAForSignal == useEMAForSignal && cacheMACDSupportResistance[idx].EqualsInput(input))
						return cacheMACDSupportResistance[idx];
			return CacheIndicator<MACDSupportResistance>(new MACDSupportResistance(){ FastLength = fastLength, SlowLength = slowLength, SignalLength = signalLength, UseEMAForOscillator = useEMAForOscillator, UseEMAForSignal = useEMAForSignal }, input, ref cacheMACDSupportResistance);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDSupportResistance MACDSupportResistance(int fastLength, int slowLength, int signalLength, bool useEMAForOscillator, bool useEMAForSignal)
		{
			return indicator.MACDSupportResistance(Input, fastLength, slowLength, signalLength, useEMAForOscillator, useEMAForSignal);
		}

		public Indicators.MACDSupportResistance MACDSupportResistance(ISeries<double> input , int fastLength, int slowLength, int signalLength, bool useEMAForOscillator, bool useEMAForSignal)
		{
			return indicator.MACDSupportResistance(input, fastLength, slowLength, signalLength, useEMAForOscillator, useEMAForSignal);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDSupportResistance MACDSupportResistance(int fastLength, int slowLength, int signalLength, bool useEMAForOscillator, bool useEMAForSignal)
		{
			return indicator.MACDSupportResistance(Input, fastLength, slowLength, signalLength, useEMAForOscillator, useEMAForSignal);
		}

		public Indicators.MACDSupportResistance MACDSupportResistance(ISeries<double> input , int fastLength, int slowLength, int signalLength, bool useEMAForOscillator, bool useEMAForSignal)
		{
			return indicator.MACDSupportResistance(input, fastLength, slowLength, signalLength, useEMAForOscillator, useEMAForSignal);
		}
	}
}

#endregion
