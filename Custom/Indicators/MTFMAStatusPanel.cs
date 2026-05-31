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
	public class MTFMAStatusPanel : Indicator
	{
		private ISeries<double> ma1;
		private ISeries<double> ma2;
		private ISeries<double> ma3;
		private ISeries<double> ma4;

		private Dictionary<int, int> timeframeToIndex = new Dictionary<int, int>();
		private int ma1BarsIndex;
		private int ma2BarsIndex;
		private int ma3BarsIndex;
		private int ma4BarsIndex;

	private double ma1Status = 0;
private double ma2Status = 0;
private double ma3Status = 0;
private double ma4Status = 0;
		private Series<double> Signal_Trade;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description                         = @"Displays bar close status relative to 4 moving averages (EMA or SMA) with colored dots on different timeframes";
				Name                                = "MTF MA Status Panel";
				Calculate                           = Calculate.OnBarClose;
				IsOverlay                           = false;
				DisplayInDataBox                    = true;
				DrawOnPricePanel                    = false;
				ScaleJustification                  = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive            = true;
				// Show transparent Signal_Trade plots in the data box
				ShowTransparentPlotsInDataBox       = true;

				// MA Periods
				MA1Period   = 14;
				MA2Period   = 34;
				MA3Period   = 50;
				MA4Period   = 200;

				// MA Timeframes (in minutes)
				MA1Timeframe = 1;
				MA2Timeframe = 1;
				MA3Timeframe = 1;
				MA4Timeframe = 1;

				// MA Types
				MA1Indicator = AverageCalcMode.Exponential;
				MA2Indicator = AverageCalcMode.Exponential;
				MA3Indicator = AverageCalcMode.Exponential;
				MA4Indicator = AverageCalcMode.Exponential;

				// Colors
				BullishColor = Brushes.Lime;
				BearishColor = Brushes.Red;
			}
			else if (State == State.Configure)
			{
				// Validate timeframes
				ValidateTimeframe(MA1Timeframe, "MA1Timeframe");
				ValidateTimeframe(MA2Timeframe, "MA2Timeframe");
				ValidateTimeframe(MA3Timeframe, "MA3Timeframe");
				ValidateTimeframe(MA4Timeframe, "MA4Timeframe");

				// Collect unique timeframes
				List<int> uniqueTimeframes = new List<int>();
				if (!uniqueTimeframes.Contains(MA1Timeframe)) uniqueTimeframes.Add(MA1Timeframe);
				if (!uniqueTimeframes.Contains(MA2Timeframe)) uniqueTimeframes.Add(MA2Timeframe);
				if (!uniqueTimeframes.Contains(MA3Timeframe)) uniqueTimeframes.Add(MA3Timeframe);
				if (!uniqueTimeframes.Contains(MA4Timeframe)) uniqueTimeframes.Add(MA4Timeframe);

				// Sort timeframes
				uniqueTimeframes.Sort();

				// Add data series for each unique timeframe
				int seriesIndex = 1; // Start at 1 because 0 is the primary series
				foreach (int tf in uniqueTimeframes)
				{
					// Check if this timeframe matches the primary chart
					if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && BarsPeriod.Value == tf)
					{
						timeframeToIndex[tf] = 0; // Use primary series
					}
					else
					{
						AddDataSeries(BarsPeriodType.Minute, tf);
						timeframeToIndex[tf] = seriesIndex;
						seriesIndex++;
					}
				}

				// Map each MA to its bars index
				ma1BarsIndex = timeframeToIndex[MA1Timeframe];
				ma2BarsIndex = timeframeToIndex[MA2Timeframe];
				ma3BarsIndex = timeframeToIndex[MA3Timeframe];
				ma4BarsIndex = timeframeToIndex[MA4Timeframe];

				// Add visible dot plots for each MA (indices 0-3)
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, GetMALabel(MA1Indicator, MA1Period, MA1Timeframe));
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, GetMALabel(MA2Indicator, MA2Period, MA2Timeframe));
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, GetMALabel(MA3Indicator, MA3Period, MA3Timeframe));
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, GetMALabel(MA4Indicator, MA4Period, MA4Timeframe));

				// Add transparent Signal_Trade plots (indices 4-7)
				// Values: 1 = bullish (price above MA), -1 = bearish (price below MA)
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Signal_Trade1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Signal_Trade2");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Signal_Trade3");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Signal_Trade4");
			}
			else if (State == State.DataLoaded)
			{
				// Initialize MAs based on selected type and timeframe
				if (MA1Indicator == AverageCalcMode.Exponential)
					ma1 = EMA(BarsArray[ma1BarsIndex], MA1Period);
				else
					ma1 = SMA(BarsArray[ma1BarsIndex], MA1Period);

				if (MA2Indicator == AverageCalcMode.Exponential)
					ma2 = EMA(BarsArray[ma2BarsIndex], MA2Period);
				else
					ma2 = SMA(BarsArray[ma2BarsIndex], MA2Period);

				if (MA3Indicator == AverageCalcMode.Exponential)
					ma3 = EMA(BarsArray[ma3BarsIndex], MA3Period);
				else
					ma3 = SMA(BarsArray[ma3BarsIndex], MA3Period);

				if (MA4Indicator == AverageCalcMode.Exponential)
					ma4 = EMA(BarsArray[ma4BarsIndex], MA4Period);
				else
					ma4 = SMA(BarsArray[ma4BarsIndex], MA4Period);

				// Initialize status series
				
			}
		}

protected override void OnBarUpdate()
{
    // Carry-forward: runs every primary bar, fills Signal_Trade from last known status
    if (BarsInProgress == 0)
    {
        if (CurrentBars[0] < 1) return;

        if (CurrentBars[ma1BarsIndex] >= MA1Period)
            Values[4][0] = ma1Status == 1 ? 1 : -1;

        if (CurrentBars[ma2BarsIndex] >= MA2Period)
            Values[5][0] = ma2Status == 1 ? 1 : -1;

        if (CurrentBars[ma3BarsIndex] >= MA3Period)
            Values[6][0] = ma3Status == 1 ? 1 : -1;

        if (CurrentBars[ma4BarsIndex] >= MA4Period)
            Values[7][0] = ma4Status == 1 ? 1 : -1;
    }

    if (BarsInProgress == ma1BarsIndex)
    {
        if (CurrentBars[ma1BarsIndex] < MA1Period) return;

        double closePrice   = Closes[ma1BarsIndex][0];
        double maValue      = ma1[0];
        ma1Status           = closePrice > maValue ? 1 : 0;  // write to plain double

        Values[0][0]        = 3;
        PlotBrushes[0][0]   = ma1Status == 1 ? BullishColor : BearishColor;
        Values[4][0]        = ma1Status == 1 ? 1 : -1;
    }

    if (BarsInProgress == ma2BarsIndex)
    {
        if (CurrentBars[ma2BarsIndex] < MA2Period) return;

        double closePrice   = Closes[ma2BarsIndex][0];
        double maValue      = ma2[0];
        ma2Status           = closePrice > maValue ? 1 : 0;

        Values[1][0]        = 2;
        PlotBrushes[1][0]   = ma2Status == 1 ? BullishColor : BearishColor;
        Values[5][0]        = ma2Status == 1 ? 1 : -1;
    }

    if (BarsInProgress == ma3BarsIndex)
    {
        if (CurrentBars[ma3BarsIndex] < MA3Period) return;

        double closePrice   = Closes[ma3BarsIndex][0];
        double maValue      = ma3[0];
        ma3Status           = closePrice > maValue ? 1 : 0;

        Values[2][0]        = 1;
        PlotBrushes[2][0]   = ma3Status == 1 ? BullishColor : BearishColor;
        Values[6][0]        = ma3Status == 1 ? 1 : -1;
    }

    if (BarsInProgress == ma4BarsIndex)
    {
        if (CurrentBars[ma4BarsIndex] < MA4Period) return;

        double closePrice   = Closes[ma4BarsIndex][0];
        double maValue      = ma4[0];
        ma4Status           = closePrice > maValue ? 1 : 0;

        Values[3][0]        = 0;
        PlotBrushes[3][0]   = ma4Status == 1 ? BullishColor : BearishColor;
        Values[7][0]        = ma4Status == 1 ? 1 : -1;
    }
}

		private void ValidateTimeframe(int timeframe, string paramName)
		{
			int[] validTimeframes = { 1, 2, 3, 5, 10, 15, 30, 60 };
			if (!validTimeframes.Contains(timeframe))
			{
				throw new Exception(string.Format("{0} must be one of: 1, 2, 3, 5, 10, 15, 30, or 60 minutes", paramName));
			}
		}

		private string GetMALabel(AverageCalcMode mode, int period, int timeframe)
		{
			string maType = mode == AverageCalcMode.Exponential ? "EMA" : "SMA";
			return string.Format("{0}{1}({2}m)", maType, period, timeframe);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MA 1 Period", Description="Period for first moving average (top row)", Order=1, GroupName="MA Periods")]
		public int MA1Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MA 2 Period", Description="Period for second moving average", Order=2, GroupName="MA Periods")]
		public int MA2Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MA 3 Period", Description="Period for third moving average", Order=3, GroupName="MA Periods")]
		public int MA3Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MA 4 Period", Description="Period for fourth moving average (bottom row)", Order=4, GroupName="MA Periods")]
		public int MA4Period
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 1 Timeframe (minutes)", Description="Timeframe for first MA (1,2,3,5,10,15,30,60)", Order=1, GroupName="MA Timeframes")]
		public int MA1Timeframe
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 2 Timeframe (minutes)", Description="Timeframe for second MA (1,2,3,5,10,15,30,60)", Order=2, GroupName="MA Timeframes")]
		public int MA2Timeframe
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 3 Timeframe (minutes)", Description="Timeframe for third MA (1,2,3,5,10,15,30,60)", Order=3, GroupName="MA Timeframes")]
		public int MA3Timeframe
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 4 Timeframe (minutes)", Description="Timeframe for fourth MA (1,2,3,5,10,15,30,60)", Order=4, GroupName="MA Timeframes")]
		public int MA4Timeframe
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 1 Type", Description="Type of first moving average", Order=1, GroupName="MA Types")]
		public AverageCalcMode MA1Indicator
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 2 Type", Description="Type of second moving average", Order=2, GroupName="MA Types")]
		public AverageCalcMode MA2Indicator
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 3 Type", Description="Type of third moving average", Order=3, GroupName="MA Types")]
		public AverageCalcMode MA3Indicator
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="MA 4 Type", Description="Type of fourth moving average", Order=4, GroupName="MA Types")]
		public AverageCalcMode MA4Indicator
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Bullish Color", Description="Color when price is above MA", Order=1, GroupName="Visual")]
		public Brush BullishColor
		{ get; set; }

		[Browsable(false)]
		public string BullishColorSerializable
		{
			get { return Serialize.BrushToString(BullishColor); }
			set { BullishColor = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name="Bearish Color", Description="Color when price is below MA", Order=2, GroupName="Visual")]
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
		public Series<double> MA1Plot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MA2Plot
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MA3Plot
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MA4Plot
		{
			get { return Values[3]; }
		}

		/// <summary>Returns 1 when price is above MA1, -1 when below. Transparent on chart but visible in Data Box.</summary>
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal_Trade1
		{
			get { return Values[4]; }
		}

		/// <summary>Returns 1 when price is above MA2, -1 when below. Transparent on chart but visible in Data Box.</summary>
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal_Trade2
		{
			get { return Values[5]; }
		}

		/// <summary>Returns 1 when price is above MA3, -1 when below. Transparent on chart but visible in Data Box.</summary>
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal_Trade3
		{
			get { return Values[6]; }
		}

		/// <summary>Returns 1 when price is above MA4, -1 when below. Transparent on chart but visible in Data Box.</summary>
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal_Trade4
		{
			get { return Values[7]; }
		}
		#endregion
	}
}

public enum AverageCalcMode
{
	Exponential,
	Simple
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MTFMAStatusPanel[] cacheMTFMAStatusPanel;
		public MTFMAStatusPanel MTFMAStatusPanel(int mA1Period, int mA2Period, int mA3Period, int mA4Period, int mA1Timeframe, int mA2Timeframe, int mA3Timeframe, int mA4Timeframe, AverageCalcMode mA1Indicator, AverageCalcMode mA2Indicator, AverageCalcMode mA3Indicator, AverageCalcMode mA4Indicator)
		{
			return MTFMAStatusPanel(Input, mA1Period, mA2Period, mA3Period, mA4Period, mA1Timeframe, mA2Timeframe, mA3Timeframe, mA4Timeframe, mA1Indicator, mA2Indicator, mA3Indicator, mA4Indicator);
		}

		public MTFMAStatusPanel MTFMAStatusPanel(ISeries<double> input, int mA1Period, int mA2Period, int mA3Period, int mA4Period, int mA1Timeframe, int mA2Timeframe, int mA3Timeframe, int mA4Timeframe, AverageCalcMode mA1Indicator, AverageCalcMode mA2Indicator, AverageCalcMode mA3Indicator, AverageCalcMode mA4Indicator)
		{
			if (cacheMTFMAStatusPanel != null)
				for (int idx = 0; idx < cacheMTFMAStatusPanel.Length; idx++)
					if (cacheMTFMAStatusPanel[idx] != null && cacheMTFMAStatusPanel[idx].MA1Period == mA1Period && cacheMTFMAStatusPanel[idx].MA2Period == mA2Period && cacheMTFMAStatusPanel[idx].MA3Period == mA3Period && cacheMTFMAStatusPanel[idx].MA4Period == mA4Period && cacheMTFMAStatusPanel[idx].MA1Timeframe == mA1Timeframe && cacheMTFMAStatusPanel[idx].MA2Timeframe == mA2Timeframe && cacheMTFMAStatusPanel[idx].MA3Timeframe == mA3Timeframe && cacheMTFMAStatusPanel[idx].MA4Timeframe == mA4Timeframe && cacheMTFMAStatusPanel[idx].MA1Indicator == mA1Indicator && cacheMTFMAStatusPanel[idx].MA2Indicator == mA2Indicator && cacheMTFMAStatusPanel[idx].MA3Indicator == mA3Indicator && cacheMTFMAStatusPanel[idx].MA4Indicator == mA4Indicator && cacheMTFMAStatusPanel[idx].EqualsInput(input))
						return cacheMTFMAStatusPanel[idx];
			return CacheIndicator<MTFMAStatusPanel>(new MTFMAStatusPanel(){ MA1Period = mA1Period, MA2Period = mA2Period, MA3Period = mA3Period, MA4Period = mA4Period, MA1Timeframe = mA1Timeframe, MA2Timeframe = mA2Timeframe, MA3Timeframe = mA3Timeframe, MA4Timeframe = mA4Timeframe, MA1Indicator = mA1Indicator, MA2Indicator = mA2Indicator, MA3Indicator = mA3Indicator, MA4Indicator = mA4Indicator }, input, ref cacheMTFMAStatusPanel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MTFMAStatusPanel MTFMAStatusPanel(int mA1Period, int mA2Period, int mA3Period, int mA4Period, int mA1Timeframe, int mA2Timeframe, int mA3Timeframe, int mA4Timeframe, AverageCalcMode mA1Indicator, AverageCalcMode mA2Indicator, AverageCalcMode mA3Indicator, AverageCalcMode mA4Indicator)
		{
			return indicator.MTFMAStatusPanel(Input, mA1Period, mA2Period, mA3Period, mA4Period, mA1Timeframe, mA2Timeframe, mA3Timeframe, mA4Timeframe, mA1Indicator, mA2Indicator, mA3Indicator, mA4Indicator);
		}

		public Indicators.MTFMAStatusPanel MTFMAStatusPanel(ISeries<double> input , int mA1Period, int mA2Period, int mA3Period, int mA4Period, int mA1Timeframe, int mA2Timeframe, int mA3Timeframe, int mA4Timeframe, AverageCalcMode mA1Indicator, AverageCalcMode mA2Indicator, AverageCalcMode mA3Indicator, AverageCalcMode mA4Indicator)
		{
			return indicator.MTFMAStatusPanel(input, mA1Period, mA2Period, mA3Period, mA4Period, mA1Timeframe, mA2Timeframe, mA3Timeframe, mA4Timeframe, mA1Indicator, mA2Indicator, mA3Indicator, mA4Indicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MTFMAStatusPanel MTFMAStatusPanel(int mA1Period, int mA2Period, int mA3Period, int mA4Period, int mA1Timeframe, int mA2Timeframe, int mA3Timeframe, int mA4Timeframe, AverageCalcMode mA1Indicator, AverageCalcMode mA2Indicator, AverageCalcMode mA3Indicator, AverageCalcMode mA4Indicator)
		{
			return indicator.MTFMAStatusPanel(Input, mA1Period, mA2Period, mA3Period, mA4Period, mA1Timeframe, mA2Timeframe, mA3Timeframe, mA4Timeframe, mA1Indicator, mA2Indicator, mA3Indicator, mA4Indicator);
		}

		public Indicators.MTFMAStatusPanel MTFMAStatusPanel(ISeries<double> input , int mA1Period, int mA2Period, int mA3Period, int mA4Period, int mA1Timeframe, int mA2Timeframe, int mA3Timeframe, int mA4Timeframe, AverageCalcMode mA1Indicator, AverageCalcMode mA2Indicator, AverageCalcMode mA3Indicator, AverageCalcMode mA4Indicator)
		{
			return indicator.MTFMAStatusPanel(input, mA1Period, mA2Period, mA3Period, mA4Period, mA1Timeframe, mA2Timeframe, mA3Timeframe, mA4Timeframe, mA1Indicator, mA2Indicator, mA3Indicator, mA4Indicator);
		}
	}
}

#endregion
