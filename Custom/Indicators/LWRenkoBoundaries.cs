
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Xml.Serialization;


namespace NinjaTrader.NinjaScript.Indicators
{
  
  public class LWRenkoBoundaries : Indicator
  {
    private double continuationTickSize;
    private double reversalTickSize;
    private const int LIVEWIRE_RENKO_ID = 62025;
    private bool isCompatible = true;
    private SimpleFont textFont;
    private SimpleFont errorFont;
    private Canvas logoCanvas;
    private bool isLogoRendered;
    private Canvas brandingCanvas;
    private bool isBrandingRendered;

    protected override void OnStateChange()
    {
      if (State == State.SetDefaults)
      {
        Description = "This is an exclusive companion to the 'LivewireRenko' Bars Type. It automatically detects the chart's settings and draws the continuation and reversal boundaries for the current bar.";
        Name = "Livewire Renko Boundaries";
        Calculate = (Calculate) 2;
        IsOverlay = true;
        DisplayInDataBox = false;
        DrawOnPricePanel = true;
        DrawHorizontalGridLines = true;
        DrawVerticalGridLines = true;
        PaintPriceMarkers = false;
        ScaleJustification = ScaleJustification.Right;
        IsSuspendedWhileInactive = true;
        this.ContinuationColor = Brushes.DodgerBlue;
        this.ReversalColor = Brushes.Yellow;
        this.LineLength = 4;
        this.LineWidth = 2;
        this.TextVerticalOffset = 0;
      }
      else if (State == State.DataLoaded)
      {
        this.textFont = new SimpleFont("Arial", 12);
        this.errorFont = new SimpleFont("Arial", 24);
        
          double tickSize = (this).TickSize;
          this.continuationTickSize = (double) Bars.BarsPeriod.Value * tickSize;
          this.reversalTickSize = (double) Bars.BarsPeriod.Value2 * tickSize;
        
      }
      
    }

    protected override void OnBarUpdate()
    {
      if (!this.isCompatible || CurrentBar < 1)
        return;
      this.RemoveDrawObject("ContinuationLine");
      this.RemoveDrawObject("ContinuationText");
      this.RemoveDrawObject("ReversalLine");
      this.RemoveDrawObject("ReversalText");
      double num1 = Close[1];
      double num2 = Open[1];
      double num3;
      double num4;
      if (num1 > num2)
      {
        num3 = num1 + this.continuationTickSize;
        num4 = num1 - this.reversalTickSize;
      }
      else
      {
        num3 = num1 + this.reversalTickSize;
        num4 = num1 - this.continuationTickSize;
      }
      int endBarsAgo = -this.LineLength;
      Draw.Line(this, "ContinuationLine", false, 0, num3, endBarsAgo, num3, this.ContinuationColor, (DashStyleHelper) 0, this.LineWidth);
      Draw.Text(this, "ContinuationText", false, string.Format("\uD83D\uDDF2 {0}", (object) (this).Instrument.MasterInstrument.FormatPrice(num3, true)), endBarsAgo - 1, num3, this.TextVerticalOffset, this.ContinuationColor, this.textFont, TextAlignment.Left, (Brush) null, (Brush) null, 0);
      Draw.Line(this, "ReversalLine", false, 0, num4, endBarsAgo, num4, this.ReversalColor, (DashStyleHelper) 0, this.LineWidth);
      Draw.Text(this, "ReversalText", false, string.Format("\uD83D\uDDF2 {0}", (object) (this).Instrument.MasterInstrument.FormatPrice(num4, true)), endBarsAgo - 1, num4, this.TextVerticalOffset, this.ReversalColor, this.textFont, TextAlignment.Left, (Brush) null, (Brush) null, 0);
    }

   

    [Display(Name = "Line Length", Order = 1, GroupName = "Visual")]
    [Range(1, 20)]
    public int LineLength { get; set; }

    [Display(Name = "Line Width", Order = 2, GroupName = "Visual")]
    [Range(1, 10)]
    public int LineWidth { get; set; }

    [Range(-20, 20)]
    [Display(Name = "Text Vertical Offset", Description = "Pixel offset to fine-tune text alignment.", Order = 3, GroupName = "Visual")]
    public int TextVerticalOffset { get; set; }

    [Display(Name = "Continuation Color", Order = 4, GroupName = "Visual")]
    [XmlIgnore]
    public Brush ContinuationColor { get; set; }

    [Browsable(false)]
    public string ContinuationColorSerializable
    {
      get => Serialize.BrushToString(this.ContinuationColor);
      set => this.ContinuationColor = Serialize.StringToBrush(value);
    }

    [Display(Name = "Reversal Color", Order = 5, GroupName = "Visual")]
    [XmlIgnore]
    public Brush ReversalColor { get; set; }

    [Browsable(false)]
    public string ReversalColorSerializable
    {
      get => Serialize.BrushToString(this.ReversalColor);
      set => this.ReversalColor = Serialize.StringToBrush(value);
    }

  
  }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LWRenkoBoundaries[] cacheLWRenkoBoundaries;
		public LWRenkoBoundaries LWRenkoBoundaries()
		{
			return LWRenkoBoundaries(Input);
		}

		public LWRenkoBoundaries LWRenkoBoundaries(ISeries<double> input)
		{
			if (cacheLWRenkoBoundaries != null)
				for (int idx = 0; idx < cacheLWRenkoBoundaries.Length; idx++)
					if (cacheLWRenkoBoundaries[idx] != null &&  cacheLWRenkoBoundaries[idx].EqualsInput(input))
						return cacheLWRenkoBoundaries[idx];
			return CacheIndicator<LWRenkoBoundaries>(new LWRenkoBoundaries(), input, ref cacheLWRenkoBoundaries);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LWRenkoBoundaries LWRenkoBoundaries()
		{
			return indicator.LWRenkoBoundaries(Input);
		}

		public Indicators.LWRenkoBoundaries LWRenkoBoundaries(ISeries<double> input )
		{
			return indicator.LWRenkoBoundaries(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LWRenkoBoundaries LWRenkoBoundaries()
		{
			return indicator.LWRenkoBoundaries(Input);
		}

		public Indicators.LWRenkoBoundaries LWRenkoBoundaries(ISeries<double> input )
		{
			return indicator.LWRenkoBoundaries(input);
		}
	}
}

#endregion
