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

	public class DashboardOscillator : Indicator
	{
	    private Series<double> mfiSeries;
	
	   protected override void OnStateChange()
{
    if (State == State.SetDefaults)
    {
       Description = "Dashboard Oscillator";
        Name = "Dashboard Oscillator";
        Calculate = Calculate.OnBarClose;
        IsOverlay = false;
        IsSuspendedWhileInactive = true;
        ShowLevelLines = true;
        MfiBullishColor = Brushes.Lime;
        MfiBearishColor = Brushes.Red;

        // Add plots
        AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "MFI");
        AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Dot, "Buy Signal");
        AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Dot, "Sell Signal");

        // Add middle line with dots
        AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Dot, "Middle Line");
		
		AddLine(new Stroke(Brushes.Red), 100, "Line100");
        AddLine(new Stroke(Brushes.Red), 90, "Line90");
        AddLine(new Stroke(Brushes.Red), 80, "Line80");
        AddLine(new Stroke(Brushes.Green), 20, "Line20");
        AddLine(new Stroke(Brushes.Green), 10, "Line10");
        AddLine(new Stroke(Brushes.Green), 0, "Line0");

        if (ShowLevelLines)
        {
            AddLine(new Stroke(Brushes.Red,DashStyleHelper.Dot,2), 70, "Line70");
            AddLine(new Stroke(Brushes.Red, DashStyleHelper.Dot,2), 60, "Line60");
            AddLine(new Stroke(Brushes.Gray, DashStyleHelper.Dot,2), 50, "Line50");
            AddLine(new Stroke(Brushes.Green, DashStyleHelper.Dot,2), 40, "Line40");
            AddLine(new Stroke(Brushes.Green, DashStyleHelper.Dot,2), 30, "Line30");
        }
    }
    else if (State == State.Configure)
    {
        mfiSeries = MFI(10).Value;
    }
    
}

protected override void OnBarUpdate()
{
    
    if (CurrentBar < 20) return;

    // Calculate MFI value
    double mfi = mfiSeries[0];
    
    if (double.IsNaN(mfi)) return;
    
    // Set MFI line color based on value
    Brush mfiColor = mfi > 50 ? MfiBullishColor : MfiBearishColor;
    
    // Plot MFI line
    Value[0] = mfi;
    PlotBrushes[0][0] = mfiColor;

    // Plot middle line dots with trend color
    Values[3][0] = 50;  // Middle line value at 50
    PlotBrushes[3][0] = mfi > 50 ? MfiBullishColor : MfiBearishColor;

    // Plot buy/sell signals
    bool buySignal = mfi < 20 && mfiSeries[1] >= 20;
    bool sellSignal = mfi > 80 && mfiSeries[1] <= 80;

    Values[1][0] = buySignal ? 50 : double.NaN;
    Values[2][0] = sellSignal ? 50 : double.NaN;
}


	
	    #region Properties
	    [NinjaScriptProperty]
	    [Display(Name = "Show Level Lines", Description = "Show additional level lines", Order = 1, GroupName = "Parameters")]
	    public bool ShowLevelLines { get; set; }
	
	    [XmlIgnore]
	    [Display(Name = "MFI Bullish Color", Description = "Color for bullish MFI", Order = 2, GroupName = "Colors")]
	    public Brush MfiBullishColor { get; set; }
	
	    [XmlIgnore]
	    [Display(Name = "MFI Bearish Color", Description = "Color for bearish MFI", Order = 3, GroupName = "Colors")]
	    public Brush MfiBearishColor { get; set; }
	
	    [Browsable(false)]
	    public string MfiBullishColorSerializable
	    {
	        get { return Serialize.BrushToString(MfiBullishColor); }
	        set { MfiBullishColor = Serialize.StringToBrush(value); }
	    }
	
	    [Browsable(false)]
	    public string MfiBearishColorSerializable
	    {
	        get { return Serialize.BrushToString(MfiBearishColor); }
	        set { MfiBearishColor = Serialize.StringToBrush(value); }
	    }
	    #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DashboardOscillator[] cacheDashboardOscillator;
		public DashboardOscillator DashboardOscillator(bool showLevelLines)
		{
			return DashboardOscillator(Input, showLevelLines);
		}

		public DashboardOscillator DashboardOscillator(ISeries<double> input, bool showLevelLines)
		{
			if (cacheDashboardOscillator != null)
				for (int idx = 0; idx < cacheDashboardOscillator.Length; idx++)
					if (cacheDashboardOscillator[idx] != null && cacheDashboardOscillator[idx].ShowLevelLines == showLevelLines && cacheDashboardOscillator[idx].EqualsInput(input))
						return cacheDashboardOscillator[idx];
			return CacheIndicator<DashboardOscillator>(new DashboardOscillator(){ ShowLevelLines = showLevelLines }, input, ref cacheDashboardOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DashboardOscillator DashboardOscillator(bool showLevelLines)
		{
			return indicator.DashboardOscillator(Input, showLevelLines);
		}

		public Indicators.DashboardOscillator DashboardOscillator(ISeries<double> input , bool showLevelLines)
		{
			return indicator.DashboardOscillator(input, showLevelLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DashboardOscillator DashboardOscillator(bool showLevelLines)
		{
			return indicator.DashboardOscillator(Input, showLevelLines);
		}

		public Indicators.DashboardOscillator DashboardOscillator(ISeries<double> input , bool showLevelLines)
		{
			return indicator.DashboardOscillator(input, showLevelLines);
		}
	}
}

#endregion
