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
  public class BuySellVolumePressure_MOD : Indicator
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

      
      if (UsePercentageSignal)
      {
        if (buyVolumeAvg[0] >= SignalThreshold && buyVolPercent[0] >= SignalPctThreshold)
        {       
          Draw.Text(this, "Buy" + CurrentBar, "º", 0, Low[0] - TickSize * 10, Brushes.Green);
        }
        if (sellVolumeAvg[0] >= SignalThreshold && sellVolPercent[0] >= SignalPctThreshold)
        {
          Draw.Text(this, "Sell" + CurrentBar, "º", 0, High[0] + TickSize * 10, Brushes.Red);
        }
      }
      else
      {
        if (buyVolumeAvg[0] >= SignalThreshold)
        {       
          Draw.Text(this, "Buy" + CurrentBar, "º", 0, Low[0] - TickSize * 10, Brushes.Green);
        }
        if (sellVolumeAvg[0] >= SignalThreshold)
        {
          Draw.Text(this, "Sell" + CurrentBar, "º", 0, High[0] + TickSize * 10, Brushes.Red);
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

    #region Properties
    [Range(1, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Buy Volume Avg Length", Order = 1, GroupName = "Parameters")]
    public int BuyVolumeAvgLength { get; set; } = 17;

    [Range(1, int.MaxValue), NinjaScriptProperty]
    [Display(Name = "Sell Volume Avg Length", Order = 2, GroupName = "Parameters")]
    public int SellVolumeAvgLength { get; set; } = 17;
	
    
    [Range(0, 10000), NinjaScriptProperty]
    [Display(Name = "Signal Threshold (Volume)", Order = 3, GroupName = "Parameters")]
    public double SignalThreshold { get; set; } = 300;
	  
    [NinjaScriptProperty]
    [Display(Name = "Use Percentage Threshold", Order = 4, GroupName = "Parameters")]
    public bool UsePercentageSignal { get; set; } = false;
	   
    [Range(0, 100), NinjaScriptProperty]
    [Display(Name = "Signal Threshold (%)", Order = 5, GroupName = "Parameters")]
    public double SignalPctThreshold { get; set; } = 50;
    #endregion
  }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Emerald.BuySellVolumePressure_MOD[] cacheBuySellVolumePressure_MOD;
		public Emerald.BuySellVolumePressure_MOD BuySellVolumePressure_MOD(int buyVolumeAvgLength, int sellVolumeAvgLength, double signalThreshold, bool usePercentageSignal, double signalPctThreshold)
		{
			return BuySellVolumePressure_MOD(Input, buyVolumeAvgLength, sellVolumeAvgLength, signalThreshold, usePercentageSignal, signalPctThreshold);
		}

		public Emerald.BuySellVolumePressure_MOD BuySellVolumePressure_MOD(ISeries<double> input, int buyVolumeAvgLength, int sellVolumeAvgLength, double signalThreshold, bool usePercentageSignal, double signalPctThreshold)
		{
			if (cacheBuySellVolumePressure_MOD != null)
				for (int idx = 0; idx < cacheBuySellVolumePressure_MOD.Length; idx++)
					if (cacheBuySellVolumePressure_MOD[idx] != null && cacheBuySellVolumePressure_MOD[idx].BuyVolumeAvgLength == buyVolumeAvgLength && cacheBuySellVolumePressure_MOD[idx].SellVolumeAvgLength == sellVolumeAvgLength && cacheBuySellVolumePressure_MOD[idx].SignalThreshold == signalThreshold && cacheBuySellVolumePressure_MOD[idx].UsePercentageSignal == usePercentageSignal && cacheBuySellVolumePressure_MOD[idx].SignalPctThreshold == signalPctThreshold && cacheBuySellVolumePressure_MOD[idx].EqualsInput(input))
						return cacheBuySellVolumePressure_MOD[idx];
			return CacheIndicator<Emerald.BuySellVolumePressure_MOD>(new Emerald.BuySellVolumePressure_MOD(){ BuyVolumeAvgLength = buyVolumeAvgLength, SellVolumeAvgLength = sellVolumeAvgLength, SignalThreshold = signalThreshold, UsePercentageSignal = usePercentageSignal, SignalPctThreshold = signalPctThreshold }, input, ref cacheBuySellVolumePressure_MOD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Emerald.BuySellVolumePressure_MOD BuySellVolumePressure_MOD(int buyVolumeAvgLength, int sellVolumeAvgLength, double signalThreshold, bool usePercentageSignal, double signalPctThreshold)
		{
			return indicator.BuySellVolumePressure_MOD(Input, buyVolumeAvgLength, sellVolumeAvgLength, signalThreshold, usePercentageSignal, signalPctThreshold);
		}

		public Indicators.Emerald.BuySellVolumePressure_MOD BuySellVolumePressure_MOD(ISeries<double> input , int buyVolumeAvgLength, int sellVolumeAvgLength, double signalThreshold, bool usePercentageSignal, double signalPctThreshold)
		{
			return indicator.BuySellVolumePressure_MOD(input, buyVolumeAvgLength, sellVolumeAvgLength, signalThreshold, usePercentageSignal, signalPctThreshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Emerald.BuySellVolumePressure_MOD BuySellVolumePressure_MOD(int buyVolumeAvgLength, int sellVolumeAvgLength, double signalThreshold, bool usePercentageSignal, double signalPctThreshold)
		{
			return indicator.BuySellVolumePressure_MOD(Input, buyVolumeAvgLength, sellVolumeAvgLength, signalThreshold, usePercentageSignal, signalPctThreshold);
		}

		public Indicators.Emerald.BuySellVolumePressure_MOD BuySellVolumePressure_MOD(ISeries<double> input , int buyVolumeAvgLength, int sellVolumeAvgLength, double signalThreshold, bool usePercentageSignal, double signalPctThreshold)
		{
			return indicator.BuySellVolumePressure_MOD(input, buyVolumeAvgLength, sellVolumeAvgLength, signalThreshold, usePercentageSignal, signalPctThreshold);
		}
	}
}

#endregion
