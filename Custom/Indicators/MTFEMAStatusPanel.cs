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
    public class MTFEMAStatusPanel : Indicator
    {
        private EMA ema1;
        private EMA ema2;
        private EMA ema3;
        private EMA ema4;
        
        private int oneMinuteBarsIndex = -1;
        private Series<double> ema1Status;
        private Series<double> ema2Status;
        private Series<double> ema3Status;
        private Series<double> ema4Status;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                                 = @"Displays 1-minute bar close status relative to 4 EMAs with colored dots";
                Name                                        = "MTF EMA Status Panel";
                Calculate                                   = Calculate.OnBarClose;
                IsOverlay                                   = false;
                DisplayInDataBox                           = true;
                DrawOnPricePanel                           = false;
                ScaleJustification                         = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive                   = true;
                
                // EMA Periods
                EMA1Period                                  = 9;
                EMA2Period                                  = 20;
                EMA3Period                                  = 50;
                EMA4Period                                  = 200;
                
                // Colors
                BullishColor                                = Brushes.Lime;
                BearishColor                                = Brushes.Red;
            }
            else if (State == State.Configure)
            {
                // Add 1-minute data series if not already on 1-minute chart
                if (BarsPeriod.BarsPeriodType != BarsPeriodType.Minute || BarsPeriod.Value != 1)
                {
                    AddDataSeries(BarsPeriodType.Minute, 1);
                    oneMinuteBarsIndex = 1;
                }
                else
                {
                    oneMinuteBarsIndex = 0; // Already on 1-minute
                }
                
                // Add plots for each EMA - using PlotStyle.Dot for visual representation
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "EMA" + EMA1Period);
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "EMA" + EMA2Period);
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "EMA" + EMA3Period);
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "EMA" + EMA4Period);
            }
            else if (State == State.DataLoaded)
            {
                // Initialize EMAs on the 1-minute data series
                ema1 = EMA(BarsArray[oneMinuteBarsIndex], EMA1Period);
                ema2 = EMA(BarsArray[oneMinuteBarsIndex], EMA2Period);
                ema3 = EMA(BarsArray[oneMinuteBarsIndex], EMA3Period);
                ema4 = EMA(BarsArray[oneMinuteBarsIndex], EMA4Period);
                
                // Initialize status series
                ema1Status = new Series<double>(this);
                ema2Status = new Series<double>(this);
                ema3Status = new Series<double>(this);
                ema4Status = new Series<double>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            // Only process on the 1-minute bars
            if (BarsInProgress != oneMinuteBarsIndex)
                return;
                
            // Ensure we have enough bars for all EMAs
            if (CurrentBars[oneMinuteBarsIndex] < Math.Max(Math.Max(EMA1Period, EMA2Period), Math.Max(EMA3Period, EMA4Period)))
                return;
            
            // Get the close price of the 1-minute bar
            double closePrice = Closes[oneMinuteBarsIndex][0];
            
            // Get current EMA values
            double ema1Value = ema1[0];
            double ema2Value = ema2[0];
            double ema3Value = ema3[0];
            double ema4Value = ema4[0];
            
            // Determine status for each EMA (store as 1 for above, 0 for below)
            ema1Status[0] = closePrice > ema1Value ? 1 : 0;
            ema2Status[0] = closePrice > ema2Value ? 1 : 0;
            ema3Status[0] = closePrice > ema3Value ? 1 : 0;
            ema4Status[0] = closePrice > ema4Value ? 1 : 0;
            
            // Set plot values at different Y levels (0, 1, 2, 3)
            Values[0][0] = 3; // EMA 1 - Top row
            Values[1][0] = 2; // EMA 2 - Second row
            Values[2][0] = 1; // EMA 3 - Third row
            Values[3][0] = 0; // EMA 4 - Bottom row
            
            // Color the plots based on status
            PlotBrushes[0][0] = ema1Status[0] == 1 ? BullishColor : BearishColor;
            PlotBrushes[1][0] = ema2Status[0] == 1 ? BullishColor : BearishColor;
            PlotBrushes[2][0] = ema3Status[0] == 1 ? BullishColor : BearishColor;
            PlotBrushes[3][0] = ema4Status[0] == 1 ? BullishColor : BearishColor;
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="EMA 1 Period", Description="Period for first EMA (top row)", Order=1, GroupName="EMA Periods")]
        public int EMA1Period
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="EMA 2 Period", Description="Period for second EMA", Order=2, GroupName="EMA Periods")]
        public int EMA2Period
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="EMA 3 Period", Description="Period for third EMA", Order=3, GroupName="EMA Periods")]
        public int EMA3Period
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="EMA 4 Period", Description="Period for fourth EMA (bottom row)", Order=4, GroupName="EMA Periods")]
        public int EMA4Period
        { get; set; }

        [XmlIgnore]
        [Display(Name="Bullish Color", Description="Color when price is above EMA", Order=1, GroupName="Visual")]
        public Brush BullishColor
        { get; set; }

        [Browsable(false)]
        public string BullishColorSerializable
        {
            get { return Serialize.BrushToString(BullishColor); }
            set { BullishColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name="Bearish Color", Description="Color when price is below EMA", Order=2, GroupName="Visual")]
        public Brush BearishColor
        { get; set; }

        [Browsable(false)]
        public string BearishColorSerializable
        {
            get { return Serialize.BrushToString(BearishColor); }
            set { BearishColor = Serialize.StringToBrush(value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EMA1Plot
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EMA2Plot
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EMA3Plot
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> EMA1Status
        {
            get { return ema1Status; }
        }
		
        
		
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MTFEMAStatusPanel[] cacheMTFEMAStatusPanel;
		public MTFEMAStatusPanel MTFEMAStatusPanel(int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period)
		{
			return MTFEMAStatusPanel(Input, eMA1Period, eMA2Period, eMA3Period, eMA4Period);
		}

		public MTFEMAStatusPanel MTFEMAStatusPanel(ISeries<double> input, int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period)
		{
			if (cacheMTFEMAStatusPanel != null)
				for (int idx = 0; idx < cacheMTFEMAStatusPanel.Length; idx++)
					if (cacheMTFEMAStatusPanel[idx] != null && cacheMTFEMAStatusPanel[idx].EMA1Period == eMA1Period && cacheMTFEMAStatusPanel[idx].EMA2Period == eMA2Period && cacheMTFEMAStatusPanel[idx].EMA3Period == eMA3Period && cacheMTFEMAStatusPanel[idx].EMA4Period == eMA4Period && cacheMTFEMAStatusPanel[idx].EqualsInput(input))
						return cacheMTFEMAStatusPanel[idx];
			return CacheIndicator<MTFEMAStatusPanel>(new MTFEMAStatusPanel(){ EMA1Period = eMA1Period, EMA2Period = eMA2Period, EMA3Period = eMA3Period, EMA4Period = eMA4Period }, input, ref cacheMTFEMAStatusPanel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MTFEMAStatusPanel MTFEMAStatusPanel(int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period)
		{
			return indicator.MTFEMAStatusPanel(Input, eMA1Period, eMA2Period, eMA3Period, eMA4Period);
		}

		public Indicators.MTFEMAStatusPanel MTFEMAStatusPanel(ISeries<double> input , int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period)
		{
			return indicator.MTFEMAStatusPanel(input, eMA1Period, eMA2Period, eMA3Period, eMA4Period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MTFEMAStatusPanel MTFEMAStatusPanel(int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period)
		{
			return indicator.MTFEMAStatusPanel(Input, eMA1Period, eMA2Period, eMA3Period, eMA4Period);
		}

		public Indicators.MTFEMAStatusPanel MTFEMAStatusPanel(ISeries<double> input , int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period)
		{
			return indicator.MTFEMAStatusPanel(input, eMA1Period, eMA2Period, eMA3Period, eMA4Period);
		}
	}
}

#endregion
