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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.Emerald
{
  /// <summary>
  /// Buy/Sell Volume Pressure Indicator.
  /// </summary>
  public class BSVP : Indicator
  {
    #region Variables
    private Series<double> buyVolume;
    private Series<double> sellVolume;
    private Series<double> buyVolumeAvg;
    private Series<double> sellVolumeAvg;
    private Series<double> buyVolPercent;
    private Series<double> sellVolPercent;

    private double latestBuyPressure = 0;
    private double latestSellPressure = 0;

    private SharpDX.Direct2D1.Brush buyPressureBrush;
    private SharpDX.Direct2D1.Brush sellPressureBrush;
    private SharpDX.DirectWrite.TextFormat textFormat;
    #endregion
	
    protected override void OnStateChange()
    {
      if (State == State.SetDefaults)
      {
        Description = "Buy/Sell Volume Pressure";
        Name = "BuySellVolumePressure_MOD";
        Calculate = Calculate.OnEachTick;
        IsOverlay = false;

        AddPlot(Brushes.Gray, "Zero Line");
        AddPlot(new Stroke(Brushes.Green, 3), PlotStyle.Bar, "Buy Volume");
        AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Bar, "Sell Volume");
		AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Bar, "Buy Pressure %");
        AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Bar, "Sell Pressure %");


        Plots[1].AutoWidth = true;
      }
      else if (State == State.DataLoaded)
      {
        buyVolume = new Series<double>(this);
        sellVolume = new Series<double>(this);
        buyVolumeAvg = new Series<double>(this);
        sellVolumeAvg = new Series<double>(this);
        buyVolPercent = new Series<double>(this);
        sellVolPercent = new Series<double>(this);
      }
    }

    protected override void OnBarUpdate()
    {
      if (CurrentBar < 1 || Volume[0] == 0 || (High[0] - Low[0]) == 0)
      {
        buyVolume[0] = 0;
        sellVolume[0] = 0;
        buyVolumeAvg[0] = 0;
        sellVolumeAvg[0] = 0;
        buyVolPercent[0] = 0;
        sellVolPercent[0] = 0;
        return;
      }

      double rawBuyVolume = Math.Round(((High[0] - Open[0]) + (Close[0] - Low[0])) / 2 / (High[0] - Low[0]) * Volume[0], 0);
      double rawSellVolume = Math.Round(((Low[0] - Open[0]) + (Close[0] - High[0])) / 2 / (High[0] - Low[0]) * Volume[0], 0);

      buyVolume[0] = rawBuyVolume;
      sellVolume[0] = Math.Abs(rawSellVolume);

      buyVolumeAvg[0] = EMA(buyVolume, BuyVolumeAvgLength)[0];
      sellVolumeAvg[0] = EMA(sellVolume, SellVolumeAvgLength)[0];

      double totalSmoothedVolume = buyVolumeAvg[0] + sellVolumeAvg[0];
      if (totalSmoothedVolume > 0)
      {
        buyVolPercent[0] = (buyVolumeAvg[0] / totalSmoothedVolume) * 100;
        sellVolPercent[0] = (sellVolumeAvg[0] / totalSmoothedVolume) * 100;
      }
      else
      {
        buyVolPercent[0] = 0;
        sellVolPercent[0] = 0;
      }

      latestBuyPressure = buyVolPercent[0];
      latestSellPressure = sellVolPercent[0];

      Values[0][0] = 0;
      Values[1][0] = buyVolumeAvg[0];
      Values[2][0] = sellVolumeAvg[0];
	  Values[3][0] = buyVolPercent[0];
      Values[4][0] = sellVolPercent[0];
	  
	  /// Dominance Factor
      bool strongBuySignal = buyVolumeAvg[0] >= sellVolumeAvg[0] * BuySellDominanceRatio;
      bool strongSellSignal = sellVolumeAvg[0] >= buyVolumeAvg[0] * BuySellDominanceRatio;

      if (strongBuySignal)
     {
       Draw.Dot(this, "BuyDom" + CurrentBar, false, 0, Low[0] - TickSize * 10, Brushes.LimeGreen);
     }
      if (strongSellSignal)
     {
       Draw.Dot(this, "SellDom" + CurrentBar, false, 0, High[0] + TickSize * 10, Brushes.OrangeRed);
     }

      
      if (EnableLongShortSignals)
      {
        if (buyVolumeAvg[0] >= LongBuyVolumeMin && sellVolumeAvg[0] < LongSellVolumeMax)
        {       
          Draw.Dot(this, "Buy" + CurrentBar, false, 0, Low[0] - TickSize * 10, Brushes.Green);
        }
        if (sellVolumeAvg[0] >= ShortSellVolumeMin && buyVolumeAvg[0] < ShortBuyVolumeMax)
        {
          Draw.Dot(this, "Sell" + CurrentBar, false, 0, High[0] + TickSize * 10, Brushes.Red);
        }
      }
      else
      {  
        if (UsePercentageSignal)
        {
          if (buyVolumeAvg[0] >= LongBuyVolumeMin && buyVolPercent[0] >= BuySignalPctThreshold)
          {       
            Draw.Text(this, "Buy" + CurrentBar, "º", 0, Low[0] - TickSize * 10, Brushes.Green);
          }
          if (sellVolumeAvg[0] >= ShortSellVolumeMin && sellVolPercent[0] >= SellSignalPctThreshold)
          {
            Draw.Text(this, "Sell" + CurrentBar, "º", 0, High[0] + TickSize * 10, Brushes.Red);
          }       
        }
      }
    }

    public override void OnRenderTargetChanged()
    {
      if (RenderTarget != null)
      {
        buyPressureBrush?.Dispose();
        buyPressureBrush = null;
        sellPressureBrush?.Dispose();
        sellPressureBrush = null;
        textFormat?.Dispose();
        textFormat = null;
      }
    }

    protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
    {
      base.OnRender(chartControl, chartScale);

      if (buyPressureBrush == null)
        buyPressureBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Green);
      if (sellPressureBrush == null)
        sellPressureBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Red);
      if (textFormat == null)
        textFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Arial", SharpDX.DirectWrite.FontWeight.Bold,
          SharpDX.DirectWrite.FontStyle.Normal, 14);

      string buyPressureText = $"Buy Pressure: {latestBuyPressure:F2}%";
      string sellPressureText = $"Sell Pressure: {latestSellPressure:F2}%";

      float x = ChartPanel.X + ChartPanel.W - 150;
      float yBuy = ChartPanel.Y + 20;
      float ySell = ChartPanel.Y + 40;

      RenderTarget.DrawText(buyPressureText, textFormat, new SharpDX.RectangleF(x, yBuy, 200, 20), buyPressureBrush);
      RenderTarget.DrawText(sellPressureText, textFormat, new SharpDX.RectangleF(x, ySell, 200, 20), sellPressureBrush);
    }

	#region Plots
	      [Browsable(false)]
	      [XmlIgnore]
	      public Series < double > BuyPressure => Values[1];

	      [Browsable(false)]
	      [XmlIgnore]
	      public Series < double > SellPressure => Values[2];
	
		  [Browsable(false)]
	      [XmlIgnore]
	      public Series < double > BuyPercent => Values[3];

	      [Browsable(false)]
	      [XmlIgnore]
	      public Series < double > SellPercent => Values[4];
	#endregion
    #region Properties
    [Range(1, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Buy Volume Avg Length", Order = 1, GroupName = "Parameters")]
    public int BuyVolumeAvgLength { get; set; } = 17;

    [Range(1, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Sell Volume Avg Length", Order = 2, GroupName = "Parameters")]
    public int SellVolumeAvgLength { get; set; } = 17;
		  
    [NinjaScriptProperty]
    [Display(Name = "Use Percentage Threshold", Order = 5, GroupName = "Parameters")]
    public bool UsePercentageSignal { get; set; } = false;
	   
    [Range(0, 100), NinjaScriptProperty]
    [Display(Name = "Buy Signal Threshold (%)", Order = 6, GroupName = "Parameters")]
    public double BuySignalPctThreshold { get; set; } = 50;
	  
    [Range(0, 100), NinjaScriptProperty]
    [Display(Name = "Sell Signal Threshold (%)", Order = 7, GroupName = "Parameters")]
    public double SellSignalPctThreshold { get; set; } = 50;
    
    [NinjaScriptProperty]
    [Display(Name = "Enable Long/Short Signals", Order = 8, GroupName = "Parameters")]
    public bool EnableLongShortSignals { get; set; } = false;
	  
    [Range(0, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Long Buy Volume Minimum", Order = 9, GroupName = "Parameters")]
    public double LongBuyVolumeMin { get; set; } = 300;
	  
    [Range(0, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Long Sell Volume Maximum", Order = 10, GroupName = "Parameters")]
    public double LongSellVolumeMax { get; set; } = 300;
	  
    [Range(0, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Short Sell Volume Minimum", Order = 11, GroupName = "Parameters")]
    public double ShortSellVolumeMin { get; set; } = 300;
	  
    [Range(0, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Short Buy Volume Maximum", Order = 12, GroupName = "Parameters")]
    public double ShortBuyVolumeMax { get; set; } = 300;
	
	/// Dominance Factor
	[Range(0.0, 10.0), NinjaScriptProperty]
    [Display(Name = "Buy/Sell Dominance Ratio", Order = 13, GroupName = "Parameters")]
    public double BuySellDominanceRatio { get; set; } = 1.75;
    #endregion
  }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Emerald.BSVP[] cacheBSVP;
		public Emerald.BSVP BSVP(int buyVolumeAvgLength, int sellVolumeAvgLength, bool usePercentageSignal, double buySignalPctThreshold, double sellSignalPctThreshold, bool enableLongShortSignals, double longBuyVolumeMin, double longSellVolumeMax, double shortSellVolumeMin, double shortBuyVolumeMax, double buySellDominanceRatio)
		{
			return BSVP(Input, buyVolumeAvgLength, sellVolumeAvgLength, usePercentageSignal, buySignalPctThreshold, sellSignalPctThreshold, enableLongShortSignals, longBuyVolumeMin, longSellVolumeMax, shortSellVolumeMin, shortBuyVolumeMax, buySellDominanceRatio);
		}

		public Emerald.BSVP BSVP(ISeries<double> input, int buyVolumeAvgLength, int sellVolumeAvgLength, bool usePercentageSignal, double buySignalPctThreshold, double sellSignalPctThreshold, bool enableLongShortSignals, double longBuyVolumeMin, double longSellVolumeMax, double shortSellVolumeMin, double shortBuyVolumeMax, double buySellDominanceRatio)
		{
			if (cacheBSVP != null)
				for (int idx = 0; idx < cacheBSVP.Length; idx++)
					if (cacheBSVP[idx] != null && cacheBSVP[idx].BuyVolumeAvgLength == buyVolumeAvgLength && cacheBSVP[idx].SellVolumeAvgLength == sellVolumeAvgLength && cacheBSVP[idx].UsePercentageSignal == usePercentageSignal && cacheBSVP[idx].BuySignalPctThreshold == buySignalPctThreshold && cacheBSVP[idx].SellSignalPctThreshold == sellSignalPctThreshold && cacheBSVP[idx].EnableLongShortSignals == enableLongShortSignals && cacheBSVP[idx].LongBuyVolumeMin == longBuyVolumeMin && cacheBSVP[idx].LongSellVolumeMax == longSellVolumeMax && cacheBSVP[idx].ShortSellVolumeMin == shortSellVolumeMin && cacheBSVP[idx].ShortBuyVolumeMax == shortBuyVolumeMax && cacheBSVP[idx].BuySellDominanceRatio == buySellDominanceRatio && cacheBSVP[idx].EqualsInput(input))
						return cacheBSVP[idx];
			return CacheIndicator<Emerald.BSVP>(new Emerald.BSVP(){ BuyVolumeAvgLength = buyVolumeAvgLength, SellVolumeAvgLength = sellVolumeAvgLength, UsePercentageSignal = usePercentageSignal, BuySignalPctThreshold = buySignalPctThreshold, SellSignalPctThreshold = sellSignalPctThreshold, EnableLongShortSignals = enableLongShortSignals, LongBuyVolumeMin = longBuyVolumeMin, LongSellVolumeMax = longSellVolumeMax, ShortSellVolumeMin = shortSellVolumeMin, ShortBuyVolumeMax = shortBuyVolumeMax, BuySellDominanceRatio = buySellDominanceRatio }, input, ref cacheBSVP);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Emerald.BSVP BSVP(int buyVolumeAvgLength, int sellVolumeAvgLength, bool usePercentageSignal, double buySignalPctThreshold, double sellSignalPctThreshold, bool enableLongShortSignals, double longBuyVolumeMin, double longSellVolumeMax, double shortSellVolumeMin, double shortBuyVolumeMax, double buySellDominanceRatio)
		{
			return indicator.BSVP(Input, buyVolumeAvgLength, sellVolumeAvgLength, usePercentageSignal, buySignalPctThreshold, sellSignalPctThreshold, enableLongShortSignals, longBuyVolumeMin, longSellVolumeMax, shortSellVolumeMin, shortBuyVolumeMax, buySellDominanceRatio);
		}

		public Indicators.Emerald.BSVP BSVP(ISeries<double> input , int buyVolumeAvgLength, int sellVolumeAvgLength, bool usePercentageSignal, double buySignalPctThreshold, double sellSignalPctThreshold, bool enableLongShortSignals, double longBuyVolumeMin, double longSellVolumeMax, double shortSellVolumeMin, double shortBuyVolumeMax, double buySellDominanceRatio)
		{
			return indicator.BSVP(input, buyVolumeAvgLength, sellVolumeAvgLength, usePercentageSignal, buySignalPctThreshold, sellSignalPctThreshold, enableLongShortSignals, longBuyVolumeMin, longSellVolumeMax, shortSellVolumeMin, shortBuyVolumeMax, buySellDominanceRatio);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Emerald.BSVP BSVP(int buyVolumeAvgLength, int sellVolumeAvgLength, bool usePercentageSignal, double buySignalPctThreshold, double sellSignalPctThreshold, bool enableLongShortSignals, double longBuyVolumeMin, double longSellVolumeMax, double shortSellVolumeMin, double shortBuyVolumeMax, double buySellDominanceRatio)
		{
			return indicator.BSVP(Input, buyVolumeAvgLength, sellVolumeAvgLength, usePercentageSignal, buySignalPctThreshold, sellSignalPctThreshold, enableLongShortSignals, longBuyVolumeMin, longSellVolumeMax, shortSellVolumeMin, shortBuyVolumeMax, buySellDominanceRatio);
		}

		public Indicators.Emerald.BSVP BSVP(ISeries<double> input , int buyVolumeAvgLength, int sellVolumeAvgLength, bool usePercentageSignal, double buySignalPctThreshold, double sellSignalPctThreshold, bool enableLongShortSignals, double longBuyVolumeMin, double longSellVolumeMax, double shortSellVolumeMin, double shortBuyVolumeMax, double buySellDominanceRatio)
		{
			return indicator.BSVP(input, buyVolumeAvgLength, sellVolumeAvgLength, usePercentageSignal, buySignalPctThreshold, sellSignalPctThreshold, enableLongShortSignals, longBuyVolumeMin, longSellVolumeMax, shortSellVolumeMin, shortBuyVolumeMax, buySellDominanceRatio);
		}
	}
}

#endregion
