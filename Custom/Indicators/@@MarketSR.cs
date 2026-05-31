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
    public class MarketSR : Indicator
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
		private MarketSR[] cacheMarketSR;
		public MarketSR MarketSR(int pivotSensitivity, int maxSRLines)
		{
			return MarketSR(Input, pivotSensitivity, maxSRLines);
		}

		public MarketSR MarketSR(ISeries<double> input, int pivotSensitivity, int maxSRLines)
		{
			if (cacheMarketSR != null)
				for (int idx = 0; idx < cacheMarketSR.Length; idx++)
					if (cacheMarketSR[idx] != null && cacheMarketSR[idx].PivotSensitivity == pivotSensitivity && cacheMarketSR[idx].MaxSRLines == maxSRLines && cacheMarketSR[idx].EqualsInput(input))
						return cacheMarketSR[idx];
			return CacheIndicator<MarketSR>(new MarketSR(){ PivotSensitivity = pivotSensitivity, MaxSRLines = maxSRLines }, input, ref cacheMarketSR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MarketSR MarketSR(int pivotSensitivity, int maxSRLines)
		{
			return indicator.MarketSR(Input, pivotSensitivity, maxSRLines);
		}

		public Indicators.MarketSR MarketSR(ISeries<double> input , int pivotSensitivity, int maxSRLines)
		{
			return indicator.MarketSR(input, pivotSensitivity, maxSRLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MarketSR MarketSR(int pivotSensitivity, int maxSRLines)
		{
			return indicator.MarketSR(Input, pivotSensitivity, maxSRLines);
		}

		public Indicators.MarketSR MarketSR(ISeries<double> input , int pivotSensitivity, int maxSRLines)
		{
			return indicator.MarketSR(input, pivotSensitivity, maxSRLines);
		}
	}
}

#endregion
