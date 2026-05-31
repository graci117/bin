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
    public class RayngeFilter : Indicator
    {
        #region Variables
        // Series for indicator calculations
        private Series<double> smrngSeries;
        private Series<double> filtSeries;
        private Series<double> upwardSeries;
        private Series<double> downwardSeries;
        private Series<int> condIniSeries;
        private Series<double> buySellSignalSeries;
        private Series<double> absDiffSeries;
        private EMA emaAbsDiff;
        private EMA emaSmoothAvrng;

        // Hard-coded visual settings
        private SimpleFont signalFont;
        private SimpleFont pointerFont;
        private const int signalVerticalOffset = 60;
        private const int buyPointerOffset = 16;
        private const int sellPointerOffset = 14;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Range Filter with Buy/Sell Signals. Original by @DonovanWall, adapted by @guikroth, @tvenn.";
                Name = "RayngeFilter";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;                 // <- stays overlay to keep filter line on price panel
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                IsSuspendedWhileInactive = true;
                ScaleJustification = ScaleJustification.Right;

                // --- DEFAULTS ---
                SamplingPeriod = 5;
                RangeMultiplier = 2;
                ColorPriceBars = true;
                
                UpColor = new SolidColorBrush(Colors.Transparent); UpColor.Freeze();
                MidColor = new SolidColorBrush(Colors.Transparent); MidColor.Freeze();
                DownColor = new SolidColorBrush(Colors.Transparent); DownColor.Freeze();

                UpBarColor = new SolidColorBrush(Colors.White); UpBarColor.Freeze();
                MidBarColor = new SolidColorBrush(Colors.SkyBlue); MidBarColor.Freeze();
                DownBarColor = new SolidColorBrush(Colors.Blue); DownBarColor.Freeze();

                BuySignalTextColor = new SolidColorBrush(Colors.White); BuySignalTextColor.Freeze();
                BuySignalBackgroundColor = new SolidColorBrush(Color.FromRgb(105, 105, 105)); BuySignalBackgroundColor.Freeze();

                SellSignalTextColor = new SolidColorBrush(Colors.White); SellSignalTextColor.Freeze();
                SellSignalBackgroundColor = new SolidColorBrush(Colors.RoyalBlue); SellSignalBackgroundColor.Freeze();
                
                signalFont = new SimpleFont("Verdana", 14) { Bold = true };
                pointerFont = new SimpleFont("Arial", 16) { Bold = true };

                AlertOnBuy = false;
                AlertOnSell = false;
                AlertOnBuyOrSell = true;

                AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "RangeFilterLine");
                AddPlot(Brushes.Transparent, "HighTargetBand");
                AddPlot(Brushes.Transparent, "LowTargetBand");

                // Make the signal plot visible; we’ll also color it per-bar in OnBarUpdate
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Dot, "BuySellSignalSeries");
            }
            else if (State == State.Configure)
            {
                smrngSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                filtSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                upwardSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                downwardSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                condIniSeries = new Series<int>(this, MaximumBarsLookBack.Infinite);
                absDiffSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                buySellSignalSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
            }
            else if (State == State.DataLoaded)
            {
                emaAbsDiff = EMA(absDiffSeries, SamplingPeriod);
                int wper = SamplingPeriod * 2 - 1;
                if (wper < 1) wper = 1;
                emaSmoothAvrng = EMA(emaAbsDiff, wper);
            }
        }

        private double Nz(ISeries<double> series, int barsAgo = 0)
        {
            if (series == null || CurrentBar - barsAgo < 0 || CurrentBar - barsAgo >= series.Count || !series.IsValidDataPoint(barsAgo) || double.IsNaN(series[barsAgo]))
                return 0.0;
            return series[barsAgo];
        }

        private int Nz(ISeries<int> series, int barsAgo = 0)
        {
            if (series == null || CurrentBar - barsAgo < 0 || CurrentBar - barsAgo >= series.Count)
                return 0;
            return series[barsAgo];
        }

        // --- Predator tag utilities ---
        [NinjaScriptProperty]
        [Display(Name = "Tag Prefix", GroupName = "Predator", Order = 0, Description = "Text prepended to all Predator tags")]
        public string TagPrefix { get; set; } = "RF_";
        
        [NinjaScriptProperty]
        [Display(Name = "Long Tag Base", GroupName = "Predator", Order = 1)]
        public string LongTagBase { get; set; } = "BuySignal";
        
        [NinjaScriptProperty]
        [Display(Name = "Short Tag Base", GroupName = "Predator", Order = 2)]
        public string ShortTagBase { get; set; } = "SellSignal";
        
        [NinjaScriptProperty]
        [Display(Name = "Emit Draw Signals", GroupName = "Predator", Order = 3, Description = "If false, draw objects are not created (use plot instead)")]
        public bool EmitDrawSignals { get; set; } = true;
        
        [NinjaScriptProperty]
        [Display(Name = "Emit Pointer Labels", GroupName = "Predator", Order = 4, Description = "Also draw BuyPointer/SellPointer labels")]
        public bool EmitPointerLabels { get; set; } = false;
        
        [NinjaScriptProperty]
        [Display(Name = "Use Time Ticks Suffix", GroupName = "Predator", Order = 5, Description = "Use Time[0].Ticks instead of CurrentBar as tag suffix")]
        public bool UseTimeTicksSuffix { get; set; } = false;
        
        private string Tag(string baseName)
        {
            return (TagPrefix ?? string.Empty) + baseName + (UseTimeTicksSuffix ? Time[0].Ticks.ToString() : CurrentBar.ToString());
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar == 0)
            {
                absDiffSeries[0] = 0; smrngSeries[0] = 0; filtSeries[0] = Input[0]; upwardSeries[0] = 0; downwardSeries[0] = 0;
                condIniSeries[0] = 0; buySellSignalSeries[0] = 0; RangeFilterLine[0] = Input[0]; HighTargetBand[0] = Input[0]; LowTargetBand[0] = Input[0];
                BuySellSignalSeries[0] = 0;
                return;
            }

            absDiffSeries[0] = Math.Abs(Input[0] - Input[1]);

            if (!emaSmoothAvrng.IsValidDataPoint(0)) smrngSeries[0] = Nz(smrngSeries, 1);
            else smrngSeries[0] = emaSmoothAvrng[0] * RangeMultiplier;

            double prevFilt = Nz(filtSeries, 1);
            double currentSrc = Input[0];
            double currentSmrng = smrngSeries[0];

            if (currentSrc > prevFilt) filtSeries[0] = (currentSrc - currentSmrng < prevFilt) ? prevFilt : currentSrc - currentSmrng;
            else filtSeries[0] = (currentSrc + currentSmrng > prevFilt) ? prevFilt : currentSrc + currentSmrng;

            if (filtSeries[0] > Nz(filtSeries, 1)) { upwardSeries[0] = Nz(upwardSeries, 1) + 1; downwardSeries[0] = 0; }
            else if (filtSeries[0] < Nz(filtSeries, 1)) { downwardSeries[0] = Nz(downwardSeries, 1) + 1; upwardSeries[0] = 0; }
            else { upwardSeries[0] = Nz(upwardSeries, 1); downwardSeries[0] = Nz(downwardSeries, 1); }

            HighTargetBand[0] = filtSeries[0] + smrngSeries[0];
            LowTargetBand[0] = filtSeries[0] - smrngSeries[0];
            RangeFilterLine[0] = filtSeries[0];

            PlotBrushes[0][0] = (upwardSeries[0] > 0) ? UpColor : (downwardSeries[0] > 0) ? DownColor : MidColor;

            if (ColorPriceBars)
            {
                if (Input[0] > filtSeries[0] && upwardSeries[0] > 0) BarBrush = UpBarColor;
                else if (Input[0] < filtSeries[0] && downwardSeries[0] > 0) BarBrush = DownBarColor;
                else BarBrush = MidBarColor;
            }

            if (UpColor is SolidColorBrush) PlotBrushes[1][0] = new SolidColorBrush((UpColor as SolidColorBrush).Color) { Opacity = 0.3 };
            if (DownColor is SolidColorBrush) PlotBrushes[2][0] = new SolidColorBrush((DownColor as SolidColorBrush).Color) { Opacity = 0.3 };

            bool longCond = (Input[0] > filtSeries[0] && upwardSeries[0] > 0);
            bool shortCond = (Input[0] < filtSeries[0] && downwardSeries[0] > 0);

            if (longCond) condIniSeries[0] = 1; 
            else if (shortCond) condIniSeries[0] = -1; 
            else condIniSeries[0] = Nz(condIniSeries, 1);

            bool finalLongCondition  = longCond && Nz(condIniSeries, 1) == -1;
            bool finalShortCondition = shortCond && Nz(condIniSeries, 1) == 1;

            // reset for this bar
            buySellSignalSeries[0] = 0;
            BuySellSignalSeries[0] = 0;
            PlotBrushes[3][0] = Brushes.Transparent;   // hide when 0

            if (finalLongCondition)
            {
                buySellSignalSeries[0] = 1;
                BuySellSignalSeries[0] = 1;           // <- visible plot value (1)
                PlotBrushes[3][0] = Brushes.LimeGreen; // color the dot

                if (EmitDrawSignals) Draw.Text(this, Tag(LongTagBase), true, "Buy", 0, Low[0], -signalVerticalOffset, BuySignalTextColor, signalFont, TextAlignment.Center, BuySignalBackgroundColor, BuySignalBackgroundColor, 100);
                if (EmitDrawSignals && EmitPointerLabels) Draw.Text(this, Tag("BuyPointer"), true, "▴", 0, Low[0], -signalVerticalOffset + buyPointerOffset, BuySignalBackgroundColor, pointerFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 100);

                if (AlertOnBuy) Alert("RangeFilterBuyAlert", Priority.High, "Buy alert on Range Filter", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav", 0, Brushes.Green, Brushes.White);
            }
            else if (finalShortCondition)
            {
                buySellSignalSeries[0] = -1;
                BuySellSignalSeries[0] = -1;          // <- visible plot value (-1)
                PlotBrushes[3][0] = Brushes.Red;       // color the dot

                if (EmitDrawSignals) Draw.Text(this, Tag(ShortTagBase), true, "Sell", 0, High[0], signalVerticalOffset, SellSignalTextColor, signalFont, TextAlignment.Center, SellSignalBackgroundColor, SellSignalBackgroundColor, 100);
                if (EmitDrawSignals && EmitPointerLabels) Draw.Text(this, Tag("SellPointer"), true, "▾", 0, High[0], signalVerticalOffset - sellPointerOffset, SellSignalBackgroundColor, pointerFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 100);
                
                if (AlertOnSell) Alert("RangeFilterSellAlert", Priority.High, "Sell alert on Range Filter", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert2.wav", 0, Brushes.Red, Brushes.White);
            }

            if (AlertOnBuyOrSell && (finalLongCondition || finalShortCondition)) 
                Alert("RangeFilterBuySellAlert", Priority.High, "Buy or Sell alert on Range Filter", NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert3.wav", 0, Brushes.Orange, Brushes.White);
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "1. Sampling Period", Order = 1, GroupName = "Parameters")]
        public int SamplingPeriod { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "2. Range Multiplier", Order = 2, GroupName = "Parameters")]
        public double RangeMultiplier { get; set; }

        [XmlIgnore]
        [Display(Name = "Up Trend Line Color", Description = "Color for upward trend filter line.", Order = 3, GroupName = "Visuals - Lines & Bars")]
        public Brush UpColor { get; set; }
        [Browsable(false)] public string UpColorSerializable { get { return Serialize.BrushToString(UpColor); } set { UpColor = Serialize.StringToBrush(value); } }

        [XmlIgnore]
        [Display(Name = "Mid Trend Line Color", Description = "Color for neutral filter line.", Order = 4, GroupName = "Visuals - Lines & Bars")]
        public Brush MidColor { get; set; }
        [Browsable(false)] public string MidColorSerializable { get { return Serialize.BrushToString(MidColor); } set { MidColor = Serialize.StringToBrush(value); } }

        [XmlIgnore]
        [Display(Name = "Down Trend Line Color", Description = "Color for downward trend filter line.", Order = 5, GroupName = "Visuals - Lines & Bars")]
        public Brush DownColor { get; set; }
        [Browsable(false)] public string DownColorSerializable { get { return Serialize.BrushToString(DownColor); } set { DownColor = Serialize.StringToBrush(value); } }

        [NinjaScriptProperty]
        [Display(Name = "Color Price Bars", Description = "If true, colors the price bars according to the trend.", Order = 6, GroupName = "Visuals - Lines & Bars")]
        public bool ColorPriceBars { get; set; }
        
        [XmlIgnore]
        [Display(Name = "Up Trend Bar Color", Description = "Color for upward trend bars.", Order = 7, GroupName = "Visuals - Lines & Bars")]
        public Brush UpBarColor { get; set; }
        [Browsable(false)] public string UpBarColorSerializable { get { return Serialize.BrushToString(UpBarColor); } set { UpBarColor = Serialize.StringToBrush(value); } }

        [XmlIgnore]
        [Display(Name = "Mid Trend Bar Color", Description = "Color for neutral trend bars.", Order = 8, GroupName = "Visuals - Lines & Bars")]
        public Brush MidBarColor { get; set; }
        [Browsable(false)] public string MidBarColorSerializable { get { return Serialize.BrushToString(MidBarColor); } set { MidBarColor = Serialize.StringToBrush(value); } }

        [XmlIgnore]
        [Display(Name = "Down Trend Bar Color", Description = "Color for downward trend bars.", Order = 9, GroupName = "Visuals - Lines & Bars")]
        public Brush DownBarColor { get; set; }
        [Browsable(false)] public string DownBarColorSerializable { get { return Serialize.BrushToString(DownBarColor); } set { DownBarColor = Serialize.StringToBrush(value); } } // <- fixed bug
        
        [XmlIgnore]
        [Display(Name = "Buy Signal Text", Description = "Color for the 'Buy' text.", Order = 10, GroupName = "Visuals - Signals")]
        public Brush BuySignalTextColor { get; set; }
        [Browsable(false)] public string BuySignalTextColorSerializable { get { return Serialize.BrushToString(BuySignalTextColor); } set { BuySignalTextColor = Serialize.StringToBrush(value); } }

        [XmlIgnore]
        [Display(Name = "Buy Signal Background", Description = "Background color for the 'Buy' tag.", Order = 11, GroupName = "Visuals - Signals")]
        public Brush BuySignalBackgroundColor { get; set; }
        [Browsable(false)] public string BuySignalBackgroundColorSerializable { get { return Serialize.BrushToString(BuySignalBackgroundColor); } set { BuySignalBackgroundColor = Serialize.StringToBrush(value); } }
        
        [XmlIgnore]
        [Display(Name = "Sell Signal Text", Description = "Color for the 'Sell' text.", Order = 12, GroupName = "Visuals - Signals")]
        public Brush SellSignalTextColor { get; set; }
        [Browsable(false)] public string SellSignalTextColorSerializable { get { return Serialize.BrushToString(SellSignalTextColor); } set { SellSignalTextColor = Serialize.StringToBrush(value); } }

        [XmlIgnore]
        [Display(Name = "Sell Signal Background", Description = "Background color for the 'Sell' tag.", Order = 13, GroupName = "Visuals - Signals")]
        public Brush SellSignalBackgroundColor { get; set; }
        [Browsable(false)] public string SellSignalBackgroundColorSerializable { get { return Serialize.BrushToString(SellSignalBackgroundColor); } set { SellSignalBackgroundColor = Serialize.StringToBrush(value); } }
        
        [NinjaScriptProperty]
        [Display(Name = "Alert on Buy Signal", Order = 20, GroupName = "Alerts")]
        public bool AlertOnBuy { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Alert on Sell Signal", Order = 21, GroupName = "Alerts")]
        public bool AlertOnSell { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Alert on Any Signal (Buy/Sell)", Order = 22, GroupName = "Alerts")]
        public bool AlertOnBuyOrSell { get; set; }

        // --- PLOT & SERIES ACCESSORS ---

        [Browsable(false)] [XmlIgnore] public Series<double> RangeFilterLine { get { return Values[0]; } }
        [Browsable(false)] [XmlIgnore] public Series<double> HighTargetBand { get { return Values[1]; } }
        [Browsable(false)] [XmlIgnore] public Series<double> LowTargetBand { get { return Values[2]; } }
        [Browsable(false)] [XmlIgnore] public Series<double> BuySellSignalSeries { get { return Values[3]; } }

        // --- RESTORED: Critical accessors for external scripts (strategies, other indicators) ---
        [Browsable(false)] [XmlIgnore] public Series<double> Smrng { get { return smrngSeries; } }
        [Browsable(false)] [XmlIgnore] public Series<double> Upward { get { return upwardSeries; } }
        [Browsable(false)] [XmlIgnore] public Series<double> Downward { get { return downwardSeries; } }

        // --- RESTORED & IMPROVED: Makes signal visible in Data Box and usable in strategies ---
        [Browsable(true)]
        [XmlIgnore]
        [Display(Name = "Buy/Sell Signal", Description = "Returns 1 for Buy, -1 for Sell, 0 otherwise.", GroupName = "Output", Order = 1)]
        public double BuySellSignal { get { return buySellSignalSeries != null && buySellSignalSeries.Count > 0 ? buySellSignalSeries[0] : 0; } }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RayngeFilter[] cacheRayngeFilter;
		public RayngeFilter RayngeFilter(string tagPrefix, string longTagBase, string shortTagBase, bool emitDrawSignals, bool emitPointerLabels, bool useTimeTicksSuffix, int samplingPeriod, double rangeMultiplier, bool colorPriceBars, bool alertOnBuy, bool alertOnSell, bool alertOnBuyOrSell)
		{
			return RayngeFilter(Input, tagPrefix, longTagBase, shortTagBase, emitDrawSignals, emitPointerLabels, useTimeTicksSuffix, samplingPeriod, rangeMultiplier, colorPriceBars, alertOnBuy, alertOnSell, alertOnBuyOrSell);
		}

		public RayngeFilter RayngeFilter(ISeries<double> input, string tagPrefix, string longTagBase, string shortTagBase, bool emitDrawSignals, bool emitPointerLabels, bool useTimeTicksSuffix, int samplingPeriod, double rangeMultiplier, bool colorPriceBars, bool alertOnBuy, bool alertOnSell, bool alertOnBuyOrSell)
		{
			if (cacheRayngeFilter != null)
				for (int idx = 0; idx < cacheRayngeFilter.Length; idx++)
					if (cacheRayngeFilter[idx] != null && cacheRayngeFilter[idx].TagPrefix == tagPrefix && cacheRayngeFilter[idx].LongTagBase == longTagBase && cacheRayngeFilter[idx].ShortTagBase == shortTagBase && cacheRayngeFilter[idx].EmitDrawSignals == emitDrawSignals && cacheRayngeFilter[idx].EmitPointerLabels == emitPointerLabels && cacheRayngeFilter[idx].UseTimeTicksSuffix == useTimeTicksSuffix && cacheRayngeFilter[idx].SamplingPeriod == samplingPeriod && cacheRayngeFilter[idx].RangeMultiplier == rangeMultiplier && cacheRayngeFilter[idx].ColorPriceBars == colorPriceBars && cacheRayngeFilter[idx].AlertOnBuy == alertOnBuy && cacheRayngeFilter[idx].AlertOnSell == alertOnSell && cacheRayngeFilter[idx].AlertOnBuyOrSell == alertOnBuyOrSell && cacheRayngeFilter[idx].EqualsInput(input))
						return cacheRayngeFilter[idx];
			return CacheIndicator<RayngeFilter>(new RayngeFilter(){ TagPrefix = tagPrefix, LongTagBase = longTagBase, ShortTagBase = shortTagBase, EmitDrawSignals = emitDrawSignals, EmitPointerLabels = emitPointerLabels, UseTimeTicksSuffix = useTimeTicksSuffix, SamplingPeriod = samplingPeriod, RangeMultiplier = rangeMultiplier, ColorPriceBars = colorPriceBars, AlertOnBuy = alertOnBuy, AlertOnSell = alertOnSell, AlertOnBuyOrSell = alertOnBuyOrSell }, input, ref cacheRayngeFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RayngeFilter RayngeFilter(string tagPrefix, string longTagBase, string shortTagBase, bool emitDrawSignals, bool emitPointerLabels, bool useTimeTicksSuffix, int samplingPeriod, double rangeMultiplier, bool colorPriceBars, bool alertOnBuy, bool alertOnSell, bool alertOnBuyOrSell)
		{
			return indicator.RayngeFilter(Input, tagPrefix, longTagBase, shortTagBase, emitDrawSignals, emitPointerLabels, useTimeTicksSuffix, samplingPeriod, rangeMultiplier, colorPriceBars, alertOnBuy, alertOnSell, alertOnBuyOrSell);
		}

		public Indicators.RayngeFilter RayngeFilter(ISeries<double> input , string tagPrefix, string longTagBase, string shortTagBase, bool emitDrawSignals, bool emitPointerLabels, bool useTimeTicksSuffix, int samplingPeriod, double rangeMultiplier, bool colorPriceBars, bool alertOnBuy, bool alertOnSell, bool alertOnBuyOrSell)
		{
			return indicator.RayngeFilter(input, tagPrefix, longTagBase, shortTagBase, emitDrawSignals, emitPointerLabels, useTimeTicksSuffix, samplingPeriod, rangeMultiplier, colorPriceBars, alertOnBuy, alertOnSell, alertOnBuyOrSell);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RayngeFilter RayngeFilter(string tagPrefix, string longTagBase, string shortTagBase, bool emitDrawSignals, bool emitPointerLabels, bool useTimeTicksSuffix, int samplingPeriod, double rangeMultiplier, bool colorPriceBars, bool alertOnBuy, bool alertOnSell, bool alertOnBuyOrSell)
		{
			return indicator.RayngeFilter(Input, tagPrefix, longTagBase, shortTagBase, emitDrawSignals, emitPointerLabels, useTimeTicksSuffix, samplingPeriod, rangeMultiplier, colorPriceBars, alertOnBuy, alertOnSell, alertOnBuyOrSell);
		}

		public Indicators.RayngeFilter RayngeFilter(ISeries<double> input , string tagPrefix, string longTagBase, string shortTagBase, bool emitDrawSignals, bool emitPointerLabels, bool useTimeTicksSuffix, int samplingPeriod, double rangeMultiplier, bool colorPriceBars, bool alertOnBuy, bool alertOnSell, bool alertOnBuyOrSell)
		{
			return indicator.RayngeFilter(input, tagPrefix, longTagBase, shortTagBase, emitDrawSignals, emitPointerLabels, useTimeTicksSuffix, samplingPeriod, rangeMultiplier, colorPriceBars, alertOnBuy, alertOnSell, alertOnBuyOrSell);
		}
	}
}

#endregion
