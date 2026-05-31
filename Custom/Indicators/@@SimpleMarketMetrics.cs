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
    public class SimpleMarketMetrics : Indicator
{
    private List<IDrawingTool> resistanceLines;
    private List<IDrawingTool> supportLines;
    private List<int> resistanceTouchBars;
    private List<int> supportTouchBars;
    private List<double> resistancePrices;
    private List<double> supportPrices;
	private Stack<Ray> resistanceRays;
	private Series<double> profitWaveEmaFast;
	private Series<double> profitWaveEmaMedium;
	private Series<double> profitWaveEmaSlow;
	private int savedUBar 		= 0;
	private int	savedDBar		= 0;
	private    DashboardOscillatorArrows dba;
private Stack<Ray> supportRays;
	
    protected override void OnStateChange()
    {
        if (State == State.SetDefaults)
        {
            Description = "Simple Market Metrics";
            Name = "Simple Market Metrics";
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
			EnableProfitWave = true;
            ProfitWaveUpperBullishColor = Brushes.Green;
            ProfitWaveLowerBullishColor = Brushes.Green;
            ProfitWaveUpperBearishColor = Brushes.Red;
            ProfitWaveLowerBearishColor = Brushes.Red;
			PanelColorCrossAbove						= Brushes.Lime;
			PanelColorCrossBelow						= Brushes.Red;
			BackgroundOpacity							= 20;		
			
			
						
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
			isBullish = new Series<bool>(this);
			
            AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "ATR");
			AddPlot(new Stroke(Brushes.White, 1), PlotStyle.Dot, "dot");
			
			Brush temp = PanelColorCrossAbove.Clone();
			temp.Opacity = BackgroundOpacity / 100.0;
			temp.Freeze();
			PanelColorCrossAbove = temp;
			
			Brush temp1 = PanelColorCrossBelow.Clone();
			temp1.Opacity = BackgroundOpacity / 100.0;
			temp1.Freeze();
			PanelColorCrossBelow = temp1;	
//			AddPlot(new Stroke(Brushes.Cyan), PlotStyle.Line, "profitWaveEmaFast");
//            AddPlot(new Stroke(Brushes.Green), PlotStyle.Line, "profitWaveEmaMedium");
//            AddPlot(new Stroke(Brushes.Yellow), PlotStyle.Line, "profitWaveEmaFast");
        }
		else if (State == State.DataLoaded)
			{	
				profitWaveEmaFast = EMA(Close, 8).Value;
	        profitWaveEmaMedium = EMA(Close, 13).Value;
	        profitWaveEmaSlow = EMA(Close, 21).Value;
			dba =  DashboardOscillatorArrows(true,true,ColoringType.ProfitWave);
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
	if (EnableProfitWave)
	{
		
	    Value[1] = profitWaveEmaFast[0];
	    Value[2] = profitWaveEmaMedium[0];
	    Value[3] = profitWaveEmaSlow[0];
	
	    // Set colors based on trend - match PineScript logic
	    isBullish[0] = Close[0] > profitWaveEmaSlow[0];
		
		if (isBullish[0] && !isBullish[1] && CurrentBar != savedUBar)
			{
				savedUBar = CurrentBar;  		// once per bar only
				
			}
			
			if (!isBullish[0] && isBullish[1] && CurrentBar != savedDBar )
			{
				savedDBar = CurrentBar;			// once per bar only
					
			}
	
//	    PlotBrushes[1][0] = isBullish ? ProfitWaveUpperBullishColor : ProfitWaveUpperBearishColor;
//	    PlotBrushes[2][0] = isBullish ? ProfitWaveLowerBullishColor : ProfitWaveLowerBearishColor;
//	    PlotBrushes[3][0] = isBullish ? ProfitWaveLowerBullishColor : ProfitWaveLowerBearishColor;
	
	    // Fill between Fast and Medium EMAs
		//Print("Close[0] is more profitWaveEmaSlow[0]---" + isBullish.ToString() + "----" + Time[0]);
		
		if ((isBullish[0] && !isBullish[1])  || (savedUBar > savedDBar))
		{
			Print("Test2---" + isBullish[0].ToString() + "----" + Time[0]);
		    Draw.Region(
		        this, 
		        "UpperRegion" + savedUBar, 
		        CurrentBar - savedUBar + 1, 
		        0, 
		        profitWaveEmaFast, 
		        profitWaveEmaMedium, 
		        Brushes.Transparent,  // Use transparent for background
		         ProfitWaveUpperBullishColor,
		        60  // Match PineScript's opacity
		    );
			Draw.Region(
		        this, 
		        "LowerRegion" + savedUBar, 
		        CurrentBar - savedUBar + 1, 
		        0, 
		        profitWaveEmaMedium, 
		        profitWaveEmaSlow, 
		        Brushes.Transparent,  // Use transparent for background
		         ProfitWaveLowerBullishColor,
		        40  // Match PineScript's opacity
		    );
		}
		if ((!isBullish[0] && isBullish[1])  || (savedDBar > savedUBar))
		{
			//Print("Test---" + isBullish[0].ToString() + "----" + Time[0]);
			 Draw.Region(
		        this, 
		        "UpperRegion" + savedDBar, 
		        CurrentBar - savedDBar + 1, 
		        0, 
		        profitWaveEmaFast, 
		        profitWaveEmaMedium, 
		        Brushes.Transparent,  // Use transparent for background
		         ProfitWaveUpperBearishColor,
		        20  // Match PineScript's opacity
		    );
			Draw.Region(
		        this, 
		        "LowerRegion" + savedDBar, 
		        CurrentBar - savedDBar + 1, 
		        0, 
		        profitWaveEmaMedium, 
		        profitWaveEmaSlow, 
		        Brushes.Transparent,  // Use transparent for background
		        ProfitWaveLowerBearishColor,
		        20  // Match PineScript's opacity
		    );
		}
	
	  BackBrush = dba.IsTrendBullish ? PanelColorCrossAbove :  PanelColorCrossBelow;	
		//Draw.Dot(this, "CloseDot" + CurrentBar, true, 0, Close[0], Brushes.White);
		 Values[1][0] = Close[0];
		 //BackBrush = PanelColorCrossAbove;	
	  
	}
}

        #region Properties
        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Pivot Sensitivity", Description = "Number of bars to look back for pivot points", Order = 1, GroupName = "Parameters")]
        public int PivotSensitivity { get; set; }
		
		 

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
	    [Display(Name = "Max Lines", Description = "Maximum number of SR lines", Order = 2, GroupName = "Parameters")]
	    public int MaxLines { get; set; }  // Changed from maxSRLines
		
		[Range(1, 99)]
		[Display(Name=" % Opacity of background", Description="Sets the amount of opacity of background colors ", Order=31, GroupName="Cross Detection Actions")]
		public int BackgroundOpacity
		{ get; set; }	
		
		[XmlIgnore]
		[Display(Name="Panel Color for Cross above", Description="Panel background color when crossing above", Order=32, GroupName="Cross Detection Actions")]
		public Brush PanelColorCrossAbove
		{ get; set; }

		[Browsable(false)]
		public string PanelColorCrossAboveSerializable
		{
			get { return Serialize.BrushToString(PanelColorCrossAbove); }
			set { PanelColorCrossAbove = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Panel Color for Cross below", Description="Panel background coloe when croissing below", Order=33, GroupName="Cross Detection Actions")]
		public Brush PanelColorCrossBelow
		{ get; set; }

		[Browsable(false)]
		public string PanelColorCrossBelowSerializable
		{
			get { return Serialize.BrushToString(PanelColorCrossBelow); }
			set { PanelColorCrossBelow = Serialize.StringToBrush(value); }
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
		
		 [NinjaScriptProperty]
    [Display(Name = "Enable Profit Wave", Description = "Show Profit Wave EMAs", Order = 1, GroupName = "Profit Wave")]
    public bool EnableProfitWave { get; set; }

    [XmlIgnore]
    [Display(Name = "Upper Bullish Color", Order = 2, GroupName = "Profit Wave")]
    public Brush ProfitWaveUpperBullishColor { get; set; }

    [XmlIgnore]
    [Display(Name = "Lower Bullish Color", Order = 3, GroupName = "Profit Wave")]
    public Brush ProfitWaveLowerBullishColor { get; set; }

    [XmlIgnore]
    [Display(Name = "Upper Bearish Color", Order = 4, GroupName = "Profit Wave")]
    public Brush ProfitWaveUpperBearishColor { get; set; }

    [XmlIgnore]
    [Display(Name = "Lower Bearish Color", Order = 5, GroupName = "Profit Wave")]
    public Brush ProfitWaveLowerBearishColor { get; set; }

    // Color serialization properties
    [Browsable(false)]
    public string ProfitWaveUpperBullishColorSerializable
    {
        get { return Serialize.BrushToString(ProfitWaveUpperBullishColor); }
        set { ProfitWaveUpperBullishColor = Serialize.StringToBrush(value); }
    }

    [Browsable(false)]
    public string ProfitWaveLowerBullishColorSerializable
    {
        get { return Serialize.BrushToString(ProfitWaveLowerBullishColor); }
        set { ProfitWaveLowerBullishColor = Serialize.StringToBrush(value); }
    }

    [Browsable(false)]
    public string ProfitWaveUpperBearishColorSerializable
    {
        get { return Serialize.BrushToString(ProfitWaveUpperBearishColor); }
        set { ProfitWaveUpperBearishColor = Serialize.StringToBrush(value); }
    }

    [Browsable(false)]
    public string ProfitWaveLowerBearishColorSerializable
    {
        get { return Serialize.BrushToString(ProfitWaveLowerBearishColor); }
        set { ProfitWaveLowerBearishColor = Serialize.StringToBrush(value); }
    }
	
	[Browsable(false)]
		[XmlIgnore]
		public Series<bool> isBullish{ get; set; }
		
	
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SimpleMarketMetrics[] cacheSimpleMarketMetrics;
		public SimpleMarketMetrics SimpleMarketMetrics(int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines, bool enableProfitWave)
		{
			return SimpleMarketMetrics(Input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines, enableProfitWave);
		}

		public SimpleMarketMetrics SimpleMarketMetrics(ISeries<double> input, int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines, bool enableProfitWave)
		{
			if (cacheSimpleMarketMetrics != null)
				for (int idx = 0; idx < cacheSimpleMarketMetrics.Length; idx++)
					if (cacheSimpleMarketMetrics[idx] != null && cacheSimpleMarketMetrics[idx].PivotSensitivity == pivotSensitivity && cacheSimpleMarketMetrics[idx].MaxLines == maxLines && cacheSimpleMarketMetrics[idx].AtrPeriod == atrPeriod && cacheSimpleMarketMetrics[idx].AtrMultiplier == atrMultiplier && cacheSimpleMarketMetrics[idx].KeepBrokenLines == keepBrokenLines && cacheSimpleMarketMetrics[idx].MaxSRLines == maxSRLines && cacheSimpleMarketMetrics[idx].EnableProfitWave == enableProfitWave && cacheSimpleMarketMetrics[idx].EqualsInput(input))
						return cacheSimpleMarketMetrics[idx];
			return CacheIndicator<SimpleMarketMetrics>(new SimpleMarketMetrics(){ PivotSensitivity = pivotSensitivity, MaxLines = maxLines, AtrPeriod = atrPeriod, AtrMultiplier = atrMultiplier, KeepBrokenLines = keepBrokenLines, MaxSRLines = maxSRLines, EnableProfitWave = enableProfitWave }, input, ref cacheSimpleMarketMetrics);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleMarketMetrics SimpleMarketMetrics(int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines, bool enableProfitWave)
		{
			return indicator.SimpleMarketMetrics(Input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines, enableProfitWave);
		}

		public Indicators.SimpleMarketMetrics SimpleMarketMetrics(ISeries<double> input , int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines, bool enableProfitWave)
		{
			return indicator.SimpleMarketMetrics(input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines, enableProfitWave);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleMarketMetrics SimpleMarketMetrics(int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines, bool enableProfitWave)
		{
			return indicator.SimpleMarketMetrics(Input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines, enableProfitWave);
		}

		public Indicators.SimpleMarketMetrics SimpleMarketMetrics(ISeries<double> input , int pivotSensitivity, int maxLines, int atrPeriod, double atrMultiplier, bool keepBrokenLines, int maxSRLines, bool enableProfitWave)
		{
			return indicator.SimpleMarketMetrics(input, pivotSensitivity, maxLines, atrPeriod, atrMultiplier, keepBrokenLines, maxSRLines, enableProfitWave);
		}
	}
}

#endregion
