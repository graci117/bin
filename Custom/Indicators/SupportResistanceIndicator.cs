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
    public class SupportResistanceIndicator : Indicator
{
    private List<IDrawingTool> resistanceLines;
    private List<IDrawingTool> supportLines;
    private List<int> resistanceTouchBars;
    private List<int> supportTouchBars;
    private List<double> resistancePrices;
    private List<double> supportPrices;
	private Stack<Ray> resistanceRays;
private Stack<Ray> supportRays;
	
    protected override void OnStateChange()
    {
        if (State == State.SetDefaults)
        {
            Description = "Support and Resistance with ATR Filter";
            Name = "Support Resistance ATR";
            Calculate = Calculate.OnBarClose;
            IsOverlay = true;
            PivotSensitivity = 5;
            MaxLines = 50;
            AtrPeriod = 14;
            AtrMultiplier = 0.5;
            ResistanceColor = Brushes.Red;
            SupportColor = Brushes.Green;
			this.KeepBrokenLines = true;
			MaxSRLines = 50;
        }
        else if (State == State.Configure)
        {
            resistanceLines = new List<IDrawingTool>();
            supportLines = new List<IDrawingTool>();
            resistanceTouchBars = new List<int>();
            supportTouchBars = new List<int>();
            resistancePrices = new List<double>();
            supportPrices = new List<double>();
             resistanceRays = new Stack<Ray>();
    supportRays = new Stack<Ray>();
            AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "ATR");
        }
    }

 protected override void OnBarUpdate()
{
    if (CurrentBar < PivotSensitivity * 2) return;

    // Check for pivot high/low with stronger ATR filter
    bool isPivotHigh = true;
    bool isPivotLow = true;
    double atr = ATR(14)[0];

    // More stringent pivot detection
    for (int i = 1; i <= PivotSensitivity; i++)
    {
        if (High[PivotSensitivity] <= High[PivotSensitivity + i] || 
            High[PivotSensitivity] <= High[PivotSensitivity - i])
        {
            isPivotHigh = false;
            break;
        }
    }

    for (int i = 1; i <= PivotSensitivity; i++)
    {
        if (Low[PivotSensitivity] >= Low[PivotSensitivity + i] || 
            Low[PivotSensitivity] >= Low[PivotSensitivity - i])
        {
            isPivotLow = false;
            break;
        }
    }

    // Draw new resistance ray with stronger ATR filter
    if (isPivotHigh && High[PivotSensitivity] - Low[PivotSensitivity] > atr)
    {
        string tag = "Resistance_" + CurrentBar;
        Ray newRay = Draw.Ray(
            this,
            tag,
            false,
            PivotSensitivity,
            High[PivotSensitivity],
            0,
            High[PivotSensitivity],
            ResistanceColor,
            DashStyleHelper.Solid,
            2
        );
        resistanceRays.Push(newRay);
    }

    // Draw new support ray with stronger ATR filter
    if (isPivotLow && High[PivotSensitivity] - Low[PivotSensitivity] > atr)
    {
        string tag = "Support_" + CurrentBar;
        Ray newRay = Draw.Ray(
            this,
            tag,
            false,
            PivotSensitivity,
            Low[PivotSensitivity],
            0,
            Low[PivotSensitivity],
            SupportColor,
            DashStyleHelper.Solid,
            2
        );
        supportRays.Push(newRay);
    }

    // Check broken resistance rays
    Ray tmpRay = null;
    if (resistanceRays.Count > 0)
    {
        tmpRay = (Ray)resistanceRays.Peek();
        while (resistanceRays.Count > 0 && Close[0] > tmpRay.StartAnchor.Price)
        {
            int barsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar + PivotSensitivity;
            Draw.Line(
                this,
                "BrokenResistance_" + barsAgo,
                false,
                barsAgo,
                tmpRay.StartAnchor.Price,
                0,
                tmpRay.StartAnchor.Price,
                ResistanceColor,
                DashStyleHelper.Dot,
                2
            );
            RemoveDrawObject(tmpRay.Tag);
            resistanceRays.Pop();
            if (resistanceRays.Count > 0)
                tmpRay = (Ray)resistanceRays.Peek();
        }
    }

    // Check broken support rays
    if (supportRays.Count > 0)
    {
        tmpRay = (Ray)supportRays.Peek();
        while (supportRays.Count > 0 && Close[0] < tmpRay.StartAnchor.Price)
        {
            int barsAgo = CurrentBar - tmpRay.StartAnchor.DrawnOnBar + PivotSensitivity;
            Draw.Line(
                this,
                "BrokenSupport_" + barsAgo,
                false,
                barsAgo,
                tmpRay.StartAnchor.Price,
                0,
                tmpRay.StartAnchor.Price,
                SupportColor,
                DashStyleHelper.Dot,
                2
            );
            RemoveDrawObject(tmpRay.Tag);
            supportRays.Pop();
            if (supportRays.Count > 0)
                tmpRay = (Ray)supportRays.Peek();
        }
    }
}

        #region Properties
        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Pivot Sensitivity", Description = "Number of bars to look back for pivot points", Order = 1, GroupName = "Parameters")]
        public int PivotSensitivity { get; set; }
		
		 [Range(1, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Max Lines", Description = "Maximum number of SR lines", Order = 2, GroupName = "Parameters")]
    public int MaxLines { get; set; }  // Changed from maxSRLines

        [XmlIgnore]
        [Display(Name = "Resistance Color", Description = "Color for resistance lines", Order = 2, GroupName = "Parameters")]
        public Brush ResistanceColor { get; set; }

        [Browsable(false)]
        public string ResistanceColorSerializable
        {
            get { return Serialize.BrushToString(ResistanceColor); }
            set { ResistanceColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Support Color", Description = "Color for support lines", Order = 3, GroupName = "Parameters")]
        public Brush SupportColor { get; set; }

        [Browsable(false)]
        public string SupportColorSerializable
        {
            get { return Serialize.BrushToString(SupportColor); }
            set { SupportColor = Serialize.StringToBrush(value); }
        }
		
		 [Range(1, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "ATR Period", Description = "ATR calculation period", Order = 1, GroupName = "Parameters")]
    public int AtrPeriod { get; set; }

    [Range(0.1, 10.0), NinjaScriptProperty]
    [Display(Name = "ATR Multiplier", Description = "Multiplier for ATR filtering", Order = 2, GroupName = "Parameters")]
    public double AtrMultiplier { get; set; }
	
	[NinjaScriptProperty]
    [Display(Name="Keep Broken Lines", Description="Show broken support/resistance lines", Order=2, GroupName="Parameters")]
    public bool KeepBrokenLines { get; set; }
	
	 [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Max SR Lines", GroupName = "Parameters")]
        public int MaxSRLines { get; set; }
	
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SupportResistanceIndicator[] cacheSupportResistanceIndicator;
		public SupportResistanceIndicator SupportResistanceIndicator(int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines)
		{
			return SupportResistanceIndicator(Input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines);
		}

		public SupportResistanceIndicator SupportResistanceIndicator(ISeries<double> input, int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines)
		{
			if (cacheSupportResistanceIndicator != null)
				for (int idx = 0; idx < cacheSupportResistanceIndicator.Length; idx++)
					if (cacheSupportResistanceIndicator[idx] != null && cacheSupportResistanceIndicator[idx].PivotSensitivity == pivotSensitivity && cacheSupportResistanceIndicator[idx].MaxLines == maxLines && cacheSupportResistanceIndicator[idx].AtrPeriod == atrPeriod && cacheSupportResistanceIndicator[idx].AtrMultiplier == atrMultiplier && cacheSupportResistanceIndicator[idx].KeepBrokenLines == keepBrokenLines && cacheSupportResistanceIndicator[idx].MaxSRLines == maxSRLines && cacheSupportResistanceIndicator[idx].EqualsInput(input))
						return cacheSupportResistanceIndicator[idx];
			return CacheIndicator<SupportResistanceIndicator>(new SupportResistanceIndicator(){ PivotSensitivity = pivotSensitivity, MaxLines = maxLines, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier, KeepBrokenLines = keepBrokenLines, MaxSRLines = maxSRLines }, input, ref cacheSupportResistanceIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SupportResistanceIndicator SupportResistanceIndicator(int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines)
		{
			return indicator.SupportResistanceIndicator(Input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines);
		}

		public Indicators.SupportResistanceIndicator SupportResistanceIndicator(ISeries<double> input , int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines)
		{
			return indicator.SupportResistanceIndicator(input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SupportResistanceIndicator SupportResistanceIndicator(int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines)
		{
			return indicator.SupportResistanceIndicator(Input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines);
		}

		public Indicators.SupportResistanceIndicator SupportResistanceIndicator(ISeries<double> input , int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines)
		{
			return indicator.SupportResistanceIndicator(input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines);
		}
	}
}

#endregion
