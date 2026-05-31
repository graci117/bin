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
    public class SimpleMarketMetricsSR : Indicator
{
    private Series<double> High;
    private Series<double> Low;
    private Series<double> Close;
    private Series<double> Open;
    private List<IDrawingTool> resistanceLines;
    private List<IDrawingTool> supportLines;
    private List<int> resistanceTouchBars;
    private List<int> supportTouchBars;

    protected override void OnStateChange()
    {
        if (State == State.SetDefaults)
        {
            Description = "Support and Resistance from Simple Market Metrics";
            Name = "SMM Support Resistance";
            PivotSensitivity = 20;
            MaxSRLines = 50;
            ResistanceColor = Brushes.Red;
            SupportColor = Brushes.Lime;
            Plots[0].Width = 2;  // Changed from LineWidth to Plots[0].Width
            Calculate = Calculate.OnBarClose;
        }
        else if (State == State.DataLoaded)
        {
            High = new Series<double>(this);
            Low = new Series<double>(this);
            Close = new Series<double>(this);
            Open = new Series<double>(this);
        }
        else if (State == State.Configure)
        {
            resistanceLines = new List<IDrawingTool>();
            supportLines = new List<IDrawingTool>();
            resistanceTouchBars = new List<int>();
            supportTouchBars = new List<int>();
            
            // Add plot for the indicator
            AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "SR Line");
        }
    }
		
		protected override void OnBarUpdate()
    {
        if (CurrentBar < PivotSensitivity * 2) return;

        // Check for pivot high using ta.pivothigh logic
        bool isPivotHigh = true;
        bool isPivotLow = true;

        for (int i = 1; i <= PivotSensitivity; i++)
        {
            if (High[PivotSensitivity] <= High[PivotSensitivity + i] || 
                High[PivotSensitivity] <= High[PivotSensitivity - i])
            {
                isPivotHigh = false;
            }
            
            if (Low[PivotSensitivity] >= Low[PivotSensitivity + i] || 
                Low[PivotSensitivity] >= Low[PivotSensitivity - i])
            {
                isPivotLow = false;
            }
        }

        // Draw resistance line
        if (isPivotHigh)
        {
            if (resistanceLines.Count >= MaxSRLines)
            {
                RemoveDrawObject(((IDrawingTool)resistanceLines[0]).Tag);
                resistanceLines.RemoveAt(0);
                resistanceTouchBars.RemoveAt(0);
            }

            string tag = "Resistance_" + CurrentBar;
            Draw.Line(
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
        }
    }
			

        #region Properties
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Pivot Sensitivity", GroupName = "Parameters")]
        public int PivotSensitivity { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Max SR Lines", GroupName = "Parameters")]
        public int MaxSRLines { get; set; }
		
		[XmlIgnore]
		[Display(Name="Support Color", Description="Color of support line", Order=8, GroupName="Parameters")]
		public Brush SupportColor
		{ get; set; }		

		[Browsable(false)]
		public string SupportColorSerializable
		{
			get { return Serialize.BrushToString(SupportColor); }
			set { SupportColor = Serialize.StringToBrush(value); }
		}			

      
        [XmlIgnore]
		[Display(Name = "Resistance Color", GroupName = "Parameters")]
		public Brush ResistanceColor { get; set; }
		
		[Browsable(false)]
		public string ResistanceColorSerializable
		{
		    get { return Serialize.BrushToString(ResistanceColor); }
		    set { ResistanceColor = Serialize.StringToBrush(value); }
		}
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SimpleMarketMetricsSR[] cacheSimpleMarketMetricsSR;
		public SimpleMarketMetricsSR SimpleMarketMetricsSR(int pivotSensitivity, int maxSRLines)
		{
			return SimpleMarketMetricsSR(Input, pivotSensitivity, maxSRLines);
		}

		public SimpleMarketMetricsSR SimpleMarketMetricsSR(ISeries<double> input, int pivotSensitivity, int maxSRLines)
		{
			if (cacheSimpleMarketMetricsSR != null)
				for (int idx = 0; idx < cacheSimpleMarketMetricsSR.Length; idx++)
					if (cacheSimpleMarketMetricsSR[idx] != null && cacheSimpleMarketMetricsSR[idx].PivotSensitivity == pivotSensitivity && cacheSimpleMarketMetricsSR[idx].MaxSRLines == maxSRLines && cacheSimpleMarketMetricsSR[idx].EqualsInput(input))
						return cacheSimpleMarketMetricsSR[idx];
			return CacheIndicator<SimpleMarketMetricsSR>(new SimpleMarketMetricsSR(){ PivotSensitivity = pivotSensitivity, MaxSRLines = maxSRLines }, input, ref cacheSimpleMarketMetricsSR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SimpleMarketMetricsSR SimpleMarketMetricsSR(int pivotSensitivity, int maxSRLines)
		{
			return indicator.SimpleMarketMetricsSR(Input, pivotSensitivity, maxSRLines);
		}

		public Indicators.SimpleMarketMetricsSR SimpleMarketMetricsSR(ISeries<double> input , int pivotSensitivity, int maxSRLines)
		{
			return indicator.SimpleMarketMetricsSR(input, pivotSensitivity, maxSRLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SimpleMarketMetricsSR SimpleMarketMetricsSR(int pivotSensitivity, int maxSRLines)
		{
			return indicator.SimpleMarketMetricsSR(Input, pivotSensitivity, maxSRLines);
		}

		public Indicators.SimpleMarketMetricsSR SimpleMarketMetricsSR(ISeries<double> input , int pivotSensitivity, int maxSRLines)
		{
			return indicator.SimpleMarketMetricsSR(input, pivotSensitivity, maxSRLines);
		}
	}
}

#endregion
