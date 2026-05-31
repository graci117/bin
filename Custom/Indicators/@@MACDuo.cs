
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
  
  public class MACDuo : Indicator
  {
    private MACD fastMacdEngine;
    private MACD slowMacdEngine;
    private bool isAlignedBullish;
    private bool isAlignedBearish;
    private bool isMisaligned;
    private bool prev_isAlignedBullish;
    private bool prev_isAlignedBearish;
    private bool prev_isMisaligned;

    protected override void OnStateChange()
    {
      if (((NinjaTrader.NinjaScript.NinjaScript) this).State == State.SetDefaults)
      {
       Description = "A unified indicator combining a fast and slow MACD to generate alignment signals based on MACD crossovers. (Version 1.05)";
       Name = "MACDuo";       
        IsOverlay = true;
         DisplayInDataBox = false;
        DrawOnPricePanel = true;
        PaintPriceMarkers = false;
        //ScaleJustification = (ScaleJustification) 1;
		  ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
        IsSuspendedWhileInactive = true;
        this.Fast_MACDFastPeriod = 12;
        this.Fast_MACDSlowPeriod = 26;
        this.Fast_MACDSmoothPeriod = 9;
        this.Slow_MACDFastPeriod = 50;
        this.Slow_MACDSlowPeriod = 100;
        this.Slow_MACDSmoothPeriod = 50;
        this.ShowAlignmentArrows = true;
        this.ShowMisalignmentX = true;
        this.ShowDashboard = true;
        this.EnableAudioAlerts = true;
        this.BullishArrowColor = (Brush) Brushes.DodgerBlue;
        this.BearishArrowColor = (Brush) Brushes.Yellow;
        this.SignalOffset = 4;
        this.MisalignmentXColor = (Brush) Brushes.Red;
        this.MisalignmentXSize = 12;
        this.DashboardPosition = DashboardPositionEnum.UpperRight;
        this.DashboardFontSize = 16;
        
      }
      else if (((NinjaTrader.NinjaScript.NinjaScript) this).State == State.DataLoaded)
      {
        this.fastMacdEngine = this.MACD(this.Fast_MACDFastPeriod, this.Fast_MACDSlowPeriod, this.Fast_MACDSmoothPeriod);
        this.slowMacdEngine = this.MACD(this.Slow_MACDFastPeriod, this.Slow_MACDSlowPeriod, this.Slow_MACDSmoothPeriod);
        this.prev_isAlignedBullish = false;
        this.prev_isAlignedBearish = false;
        this.prev_isMisaligned = true;
      }
     
    
    }

    protected override void OnBarUpdate()
    {
      if (CurrentBar < Math.Max(this.Fast_MACDSlowPeriod, this.Slow_MACDSlowPeriod) + Math.Max(this.Fast_MACDSmoothPeriod, this.Slow_MACDSmoothPeriod))
        return;
      bool flag1 = this.fastMacdEngine.Default[0] > this.fastMacdEngine.Avg[0];
      bool flag2 = this.slowMacdEngine.Default[0] > this.slowMacdEngine.Avg[0];
      this.isAlignedBullish = flag1 & flag2;
      this.isAlignedBearish = !flag1 && !flag2;
      this.isMisaligned = !this.isAlignedBullish && !this.isAlignedBearish;
      bool flag3 = this.isAlignedBullish && !this.prev_isAlignedBullish;
      bool flag4 = this.isAlignedBearish && !this.prev_isAlignedBearish;
      bool flag5 = this.isMisaligned && !this.prev_isMisaligned;
      double y1 = ((NinjaScriptBase) this).Low[0] - (double) this.SignalOffset * ((NinjaScriptBase) this).TickSize;
      double y2 = ((NinjaScriptBase) this).High[0] + (double) this.SignalOffset * ((NinjaScriptBase) this).TickSize;
      if (flag3)
      {
        if (this.ShowAlignmentArrows)
          Draw.ArrowUp((NinjaScriptBase) this, "LMD_Signal" + ((NinjaScriptBase) this).CurrentBar.ToString(), true, 0, y1, this.BullishArrowColor);
        if (this.EnableAudioAlerts)
         PlaySound(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NinjaTrader 8\\templates\\Sounds\\MACDuoBullish.wav"));
      }
      else if (flag4)
      {
        if (this.ShowAlignmentArrows)
          Draw.ArrowDown(this, "LMD_Signal" + ((NinjaScriptBase) this).CurrentBar.ToString(), true, 0, y2, this.BearishArrowColor);
        if (this.EnableAudioAlerts)
          this.PlaySound(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NinjaTrader 8\\templates\\Sounds\\MACDuoBearish.wav"));
      }
      else if (flag5)
      {
        if (this.ShowMisalignmentX)
        {
          SimpleFont font = new SimpleFont("Arial", this.MisalignmentXSize)
          {
            Bold = true
          };
          Draw.Text(this, "LMD_Signal" + ((NinjaScriptBase) this).CurrentBar.ToString(), true, "X", 0, y2, 0, this.MisalignmentXColor, font, TextAlignment.Center, (Brush) Brushes.Transparent, (Brush) Brushes.Transparent, 0);
        }
        if (this.EnableAudioAlerts)
          this.PlaySound(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "NinjaTrader 8\\templates\\Sounds\\MACDuoNeutral.wav"));
      }
      if (this.ShowDashboard && flag3 | flag4 | flag5)
        this.UpdateDashboard();
      this.prev_isAlignedBullish = this.isAlignedBullish;
      this.prev_isAlignedBearish = this.isAlignedBearish;
      this.prev_isMisaligned = this.isMisaligned;
    }

    private void UpdateDashboard()
    {
      string text = "MISALIGNED / NEUTRAL";
      Brush textBrush = (Brush) Brushes.Gray;
      if (this.isAlignedBullish)
      {
        text = "BULLISH ALIGNMENT";
        textBrush = this.BullishArrowColor;
      }
      else if (this.isAlignedBearish)
      {
        text = "BEARISH ALIGNMENT";
        textBrush = this.BearishArrowColor;
      }
      TextPosition textPosition;
      switch (this.DashboardPosition)
      {
        case DashboardPositionEnum.UpperLeft:
          textPosition = TextPosition.TopLeft;
          break;
        case DashboardPositionEnum.UpperRight:
          textPosition = TextPosition.TopRight;
          break;
        default:
          textPosition = TextPosition.TopRight;
          break;
      }
      this.RemoveDrawObject("DashboardText");
      SimpleFont font = new SimpleFont("Calibri", this.DashboardFontSize)
      {
        Bold = true
      };
      Draw.TextFixed((NinjaScriptBase) this, "DashboardText", text, textPosition, textBrush, font, (Brush) Brushes.Transparent, (Brush) Brushes.Transparent, 0);
    }
   

    [NinjaScriptProperty]
    [Display(Name = "MACD Fast", Order = 1, GroupName = "1. Fast Signal Settings")]
    public int Fast_MACDFastPeriod { get; set; }

    [NinjaScriptProperty]
    [Display(Name = "MACD Slow", Order = 2, GroupName = "1. Fast Signal Settings")]
    public int Fast_MACDSlowPeriod { get; set; }

    [NinjaScriptProperty]
    [Display(Name = "MACD Smooth", Order = 3, GroupName = "1. Fast Signal Settings")]
    public int Fast_MACDSmoothPeriod { get; set; }

    [Display(Name = "MACD Fast", Order = 1, GroupName = "2. Slow Signal Settings")]
    [NinjaScriptProperty]
    public int Slow_MACDFastPeriod { get; set; }

    [Display(Name = "MACD Slow", Order = 2, GroupName = "2. Slow Signal Settings")]
    [NinjaScriptProperty]
    public int Slow_MACDSlowPeriod { get; set; }

    [NinjaScriptProperty]
    [Display(Name = "MACD Smooth", Order = 3, GroupName = "2. Slow Signal Settings")]
    public int Slow_MACDSmoothPeriod { get; set; }

    [Display(Name = "Enable Audio Alerts", Order = 1, GroupName = "3. Visuals & Alerts")]
    [NinjaScriptProperty]
    public bool EnableAudioAlerts { get; set; }

    [Display(Name = "Show Alignment Arrows", Order = 2, GroupName = "3. Visuals & Alerts")]
    [NinjaScriptProperty]
    public bool ShowAlignmentArrows { get; set; }

    [Display(Name = "Show Misalignment 'X'", Order = 3, GroupName = "3. Visuals & Alerts")]
    [NinjaScriptProperty]
    public bool ShowMisalignmentX { get; set; }

    [NinjaScriptProperty]
    [Display(Name = "Show Dashboard Text", Order = 4, GroupName = "3. Visuals & Alerts")]
    public bool ShowDashboard { get; set; }

    [NinjaScriptProperty]
    [Display(Name = "Dashboard Position", Order = 5, GroupName = "3. Visuals & Alerts")]
    public DashboardPositionEnum DashboardPosition { get; set; }

    [Display(Name = "Dashboard Font Size", Order = 6, GroupName = "3. Visuals & Alerts")]
    [NinjaScriptProperty]
    public int DashboardFontSize { get; set; }

    [Display(Name = "Signal Offset", Order = 7, GroupName = "3. Visuals & Alerts")]
    [NinjaScriptProperty]
    public int SignalOffset { get; set; }

    [XmlIgnore]
    [Display(Name = "Bullish Arrow Color", Order = 8, GroupName = "3. Visuals & Alerts")]
    [NinjaScriptProperty]
    public Brush BullishArrowColor { get; set; }

    [Browsable(false)]
    public string BullishArrowColorSerializable
    {
      get => Serialize.BrushToString(this.BullishArrowColor);
      set => this.BullishArrowColor = Serialize.StringToBrush(value);
    }

    [NinjaScriptProperty]
    [Display(Name = "Bearish Arrow Color", Order = 9, GroupName = "3. Visuals & Alerts")]
    [XmlIgnore]
    public Brush BearishArrowColor { get; set; }

    [Browsable(false)]
    public string BearishArrowColorSerializable
    {
      get => Serialize.BrushToString(this.BearishArrowColor);
      set => this.BearishArrowColor = Serialize.StringToBrush(value);
    }

    [XmlIgnore]
    [NinjaScriptProperty]
    [Display(Name = "Misalignment 'X' Color", Order = 10, GroupName = "3. Visuals & Alerts")]
    public Brush MisalignmentXColor { get; set; }

    [Browsable(false)]
    public string MisalignmentXColorSerializable
    {
      get => Serialize.BrushToString(this.MisalignmentXColor);
      set => this.MisalignmentXColor = Serialize.StringToBrush(value);
    }

    [Range(1, 100)]
    [NinjaScriptProperty]
    [Display(Name = "Misalignment 'X' Size", Order = 11, GroupName = "3. Visuals & Alerts")]
    public int MisalignmentXSize { get; set; }

  }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDuo[] cacheMACDuo;
		public MACDuo MACDuo(int fast_MACDFastPeriod, int fast_MACDSlowPeriod, int fast_MACDSmoothPeriod, int slow_MACDFastPeriod, int slow_MACDSlowPeriod, int slow_MACDSmoothPeriod, bool enableAudioAlerts, bool showAlignmentArrows, bool showMisalignmentX, bool showDashboard, DashboardPositionEnum dashboardPosition, int dashboardFontSize, int signalOffset, Brush bullishArrowColor, Brush bearishArrowColor, Brush misalignmentXColor, int misalignmentXSize)
		{
			return MACDuo(Input, fast_MACDFastPeriod, fast_MACDSlowPeriod, fast_MACDSmoothPeriod, slow_MACDFastPeriod, slow_MACDSlowPeriod, slow_MACDSmoothPeriod, enableAudioAlerts, showAlignmentArrows, showMisalignmentX, showDashboard, dashboardPosition, dashboardFontSize, signalOffset, bullishArrowColor, bearishArrowColor, misalignmentXColor, misalignmentXSize);
		}

		public MACDuo MACDuo(ISeries<double> input, int fast_MACDFastPeriod, int fast_MACDSlowPeriod, int fast_MACDSmoothPeriod, int slow_MACDFastPeriod, int slow_MACDSlowPeriod, int slow_MACDSmoothPeriod, bool enableAudioAlerts, bool showAlignmentArrows, bool showMisalignmentX, bool showDashboard, DashboardPositionEnum dashboardPosition, int dashboardFontSize, int signalOffset, Brush bullishArrowColor, Brush bearishArrowColor, Brush misalignmentXColor, int misalignmentXSize)
		{
			if (cacheMACDuo != null)
				for (int idx = 0; idx < cacheMACDuo.Length; idx++)
					if (cacheMACDuo[idx] != null && cacheMACDuo[idx].Fast_MACDFastPeriod == fast_MACDFastPeriod && cacheMACDuo[idx].Fast_MACDSlowPeriod == fast_MACDSlowPeriod && cacheMACDuo[idx].Fast_MACDSmoothPeriod == fast_MACDSmoothPeriod && cacheMACDuo[idx].Slow_MACDFastPeriod == slow_MACDFastPeriod && cacheMACDuo[idx].Slow_MACDSlowPeriod == slow_MACDSlowPeriod && cacheMACDuo[idx].Slow_MACDSmoothPeriod == slow_MACDSmoothPeriod && cacheMACDuo[idx].EnableAudioAlerts == enableAudioAlerts && cacheMACDuo[idx].ShowAlignmentArrows == showAlignmentArrows && cacheMACDuo[idx].ShowMisalignmentX == showMisalignmentX && cacheMACDuo[idx].ShowDashboard == showDashboard && cacheMACDuo[idx].DashboardPosition == dashboardPosition && cacheMACDuo[idx].DashboardFontSize == dashboardFontSize && cacheMACDuo[idx].SignalOffset == signalOffset && cacheMACDuo[idx].BullishArrowColor == bullishArrowColor && cacheMACDuo[idx].BearishArrowColor == bearishArrowColor && cacheMACDuo[idx].MisalignmentXColor == misalignmentXColor && cacheMACDuo[idx].MisalignmentXSize == misalignmentXSize && cacheMACDuo[idx].EqualsInput(input))
						return cacheMACDuo[idx];
			return CacheIndicator<MACDuo>(new MACDuo(){ Fast_MACDFastPeriod = fast_MACDFastPeriod, Fast_MACDSlowPeriod = fast_MACDSlowPeriod, Fast_MACDSmoothPeriod = fast_MACDSmoothPeriod, Slow_MACDFastPeriod = slow_MACDFastPeriod, Slow_MACDSlowPeriod = slow_MACDSlowPeriod, Slow_MACDSmoothPeriod = slow_MACDSmoothPeriod, EnableAudioAlerts = enableAudioAlerts, ShowAlignmentArrows = showAlignmentArrows, ShowMisalignmentX = showMisalignmentX, ShowDashboard = showDashboard, DashboardPosition = dashboardPosition, DashboardFontSize = dashboardFontSize, SignalOffset = signalOffset, BullishArrowColor = bullishArrowColor, BearishArrowColor = bearishArrowColor, MisalignmentXColor = misalignmentXColor, MisalignmentXSize = misalignmentXSize }, input, ref cacheMACDuo);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDuo MACDuo(int fast_MACDFastPeriod, int fast_MACDSlowPeriod, int fast_MACDSmoothPeriod, int slow_MACDFastPeriod, int slow_MACDSlowPeriod, int slow_MACDSmoothPeriod, bool enableAudioAlerts, bool showAlignmentArrows, bool showMisalignmentX, bool showDashboard, DashboardPositionEnum dashboardPosition, int dashboardFontSize, int signalOffset, Brush bullishArrowColor, Brush bearishArrowColor, Brush misalignmentXColor, int misalignmentXSize)
		{
			return indicator.MACDuo(Input, fast_MACDFastPeriod, fast_MACDSlowPeriod, fast_MACDSmoothPeriod, slow_MACDFastPeriod, slow_MACDSlowPeriod, slow_MACDSmoothPeriod, enableAudioAlerts, showAlignmentArrows, showMisalignmentX, showDashboard, dashboardPosition, dashboardFontSize, signalOffset, bullishArrowColor, bearishArrowColor, misalignmentXColor, misalignmentXSize);
		}

		public Indicators.MACDuo MACDuo(ISeries<double> input , int fast_MACDFastPeriod, int fast_MACDSlowPeriod, int fast_MACDSmoothPeriod, int slow_MACDFastPeriod, int slow_MACDSlowPeriod, int slow_MACDSmoothPeriod, bool enableAudioAlerts, bool showAlignmentArrows, bool showMisalignmentX, bool showDashboard, DashboardPositionEnum dashboardPosition, int dashboardFontSize, int signalOffset, Brush bullishArrowColor, Brush bearishArrowColor, Brush misalignmentXColor, int misalignmentXSize)
		{
			return indicator.MACDuo(input, fast_MACDFastPeriod, fast_MACDSlowPeriod, fast_MACDSmoothPeriod, slow_MACDFastPeriod, slow_MACDSlowPeriod, slow_MACDSmoothPeriod, enableAudioAlerts, showAlignmentArrows, showMisalignmentX, showDashboard, dashboardPosition, dashboardFontSize, signalOffset, bullishArrowColor, bearishArrowColor, misalignmentXColor, misalignmentXSize);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDuo MACDuo(int fast_MACDFastPeriod, int fast_MACDSlowPeriod, int fast_MACDSmoothPeriod, int slow_MACDFastPeriod, int slow_MACDSlowPeriod, int slow_MACDSmoothPeriod, bool enableAudioAlerts, bool showAlignmentArrows, bool showMisalignmentX, bool showDashboard, DashboardPositionEnum dashboardPosition, int dashboardFontSize, int signalOffset, Brush bullishArrowColor, Brush bearishArrowColor, Brush misalignmentXColor, int misalignmentXSize)
		{
			return indicator.MACDuo(Input, fast_MACDFastPeriod, fast_MACDSlowPeriod, fast_MACDSmoothPeriod, slow_MACDFastPeriod, slow_MACDSlowPeriod, slow_MACDSmoothPeriod, enableAudioAlerts, showAlignmentArrows, showMisalignmentX, showDashboard, dashboardPosition, dashboardFontSize, signalOffset, bullishArrowColor, bearishArrowColor, misalignmentXColor, misalignmentXSize);
		}

		public Indicators.MACDuo MACDuo(ISeries<double> input , int fast_MACDFastPeriod, int fast_MACDSlowPeriod, int fast_MACDSmoothPeriod, int slow_MACDFastPeriod, int slow_MACDSlowPeriod, int slow_MACDSmoothPeriod, bool enableAudioAlerts, bool showAlignmentArrows, bool showMisalignmentX, bool showDashboard, DashboardPositionEnum dashboardPosition, int dashboardFontSize, int signalOffset, Brush bullishArrowColor, Brush bearishArrowColor, Brush misalignmentXColor, int misalignmentXSize)
		{
			return indicator.MACDuo(input, fast_MACDFastPeriod, fast_MACDSlowPeriod, fast_MACDSmoothPeriod, slow_MACDFastPeriod, slow_MACDSlowPeriod, slow_MACDSmoothPeriod, enableAudioAlerts, showAlignmentArrows, showMisalignmentX, showDashboard, dashboardPosition, dashboardFontSize, signalOffset, bullishArrowColor, bearishArrowColor, misalignmentXColor, misalignmentXSize);
		}
	}
}

#endregion
