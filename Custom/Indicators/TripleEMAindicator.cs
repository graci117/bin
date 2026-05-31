#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.MarketAnalyzerColumns;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Triple EMA Indicator.
	/// </summary>
	public class TripleEMAindicator : Indicator
	{
        private EMA fastEMA;
        private EMA mediumEMA;
        private EMA slowEMA;
		private int StartIndex = 1;
        private int PriorIndex = 0;
		private Series<bool> Trendup = null;
		private int _arrowsOffset = 5;
		private NinjaTrader.Gui.Tools.SimpleFont myFont;
		private bool buycondition, sellcondition;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Triple EMA indicator";
                Name = "TripleEMAindicator";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
				fastEMALength = 8;
				showfastEMAPlot = false;
				mediumEMALength = 20;
				showmediumEMAPlot = false;
				slowEMALength = 200;
				showslowEMAPlot = true;
				colorEMAs = false;
				colorBars = false;
				showTrendArrows = false;
				ColorRegion    = false;
				RegionOpacity       = 20;
				showText            = false;
				TextPositionOffset  = 10;
				UpTrend = Brushes.Green;
				DownTrend = Brushes.Red;
				NeuTrend = Brushes.Gray;
				_arrowUpColor = Brushes.Blue;
				_arrowDownColor = Brushes.Red;
				myFont = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 12) { Size = 12, Bold = true };

				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "fastEMA"); 
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "mediumEMA"); 
				AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Line, "slowEMA"); 
            }
            else if (State == State.DataLoaded)
            {
                // Initializing EMAs
                fastEMA = EMA(Close, fastEMALength);
                mediumEMA = EMA(Close, mediumEMALength);
                slowEMA = EMA(Close, slowEMALength);
				Trendup = new Series<bool>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            // Ensure enough bars exist to compute EMA
            if (CurrentBar < Math.Max(Math.Max(fastEMALength, mediumEMALength), slowEMALength))
                return;

            // Calculate EMA values
            double fastEmaValue = fastEMA[0];
            double mediumEmaValue = mediumEMA[0];
            double slowEmaValue = slowEMA[0];

            // Bar color logic
            if (Close[0] > slowEmaValue && Close[0] > mediumEmaValue && colorBars)
			{
                BarBrush = UpTrend; // Bullish
				CandleOutlineBrush = UpTrend;
			}
            else if (Close[0] < slowEmaValue && Close[0] < mediumEmaValue && colorBars)
			{
                BarBrush = DownTrend; // Bearish
				CandleOutlineBrush = DownTrend;
			}
            else if (colorBars)
			{
                BarBrush = NeuTrend; // Neutral
				CandleOutlineBrush = NeuTrend;
			}

            // EMA color logic
            Values[0][0] = fastEMA[0];
			Values[1][0] = mediumEMA[0];
			Values[2][0] = slowEMA[0];
			if (showfastEMAPlot) PlotBrushes[0][0] = fastEMA[0] > fastEMA[1] ? UpTrend : DownTrend;
            if (showmediumEMAPlot) PlotBrushes[1][0] = mediumEMA[0] > mediumEMA[1] ? UpTrend : DownTrend;
			if (showslowEMAPlot) PlotBrushes[2][0] = IsRising(slowEMA) ? UpTrend : DownTrend;

            // Fill between upper and lower EMA lines
			if (fastEMA[0] > mediumEMA[0])
            {
                if (ColorRegion && RegionOpacity != 0)
                {
                    if (IsFirstTickOfBar)
                        PriorIndex = StartIndex;
                    int CountBars = CurrentBar - PriorIndex + 1 - Displacement;
                    if (Trendup[1])
                    {
                        if (StartIndex == CurrentBar)
                            RemoveDrawObject("Region"+CurrentBar);
                        if (CountBars <= CurrentBar)
                            Draw.Region(this, "Region"+PriorIndex, CountBars, -Displacement, fastEMA, mediumEMA, null, UpTrend, RegionOpacity);
                        StartIndex = PriorIndex;
                    }
                    else
                    {
                        if (CountBars <= CurrentBar && StartIndex == PriorIndex)
                            Draw.Region(this, "Region"+PriorIndex, CountBars, 1-Displacement, fastEMA, mediumEMA, null, DownTrend, RegionOpacity);
                        Draw.Region(this, "Region"+CurrentBar, 1-Displacement, -Displacement, fastEMA, mediumEMA, null, UpTrend, RegionOpacity);
                        StartIndex = CurrentBar;
                    }
                }
                Trendup[0] = true;
            }
            else if (fastEMA[0] < mediumEMA[0])
            {
                if (ColorRegion && RegionOpacity != 0)
                {
                    if (IsFirstTickOfBar)
                        PriorIndex = StartIndex;
                    int CountBars = CurrentBar - PriorIndex + 1 - Displacement;
                    if (!Trendup[1])
                    {
                        if (StartIndex == CurrentBar)
                            RemoveDrawObject("Region"+CurrentBar);
                        if (CountBars <= CurrentBar)
                            Draw.Region(this, "Region"+PriorIndex, CurrentBar-PriorIndex+1-Displacement, -Displacement, fastEMA, mediumEMA, null, DownTrend, RegionOpacity);
                        StartIndex = PriorIndex;
                    }
                    else
                    {
                        if (CountBars <= CurrentBar && StartIndex == PriorIndex)
                            Draw.Region(this, "Region"+PriorIndex, CurrentBar-PriorIndex+1-Displacement, 1-Displacement, fastEMA, mediumEMA, null, UpTrend, RegionOpacity);
                        Draw.Region(this, "Region"+CurrentBar, 1-Displacement, -Displacement, fastEMA, mediumEMA, null, DownTrend, RegionOpacity);
                        StartIndex = CurrentBar;
                    }
                }
                Trendup[0] = false;
            }
            else
            {
                if (ColorRegion && RegionOpacity != 0)
                {
                    if (IsFirstTickOfBar)
                        PriorIndex = StartIndex;
                    int CountBars = CurrentBar - PriorIndex + 1 - Displacement;
                    if (StartIndex == CurrentBar)
                        RemoveDrawObject("Region"+CurrentBar);
                    if (CountBars <= CurrentBar)
                        Draw.Region(this, "Region"+PriorIndex, CountBars, -Displacement, fastEMA, mediumEMA, null, Trendup[1] ? UpTrend : DownTrend, RegionOpacity);
                    StartIndex = PriorIndex;
                }
                Trendup[0] = Trendup[1];
            }
            // Alert Condition
            if (CrossAbove(Close, fastEMA, 1) && CrossAbove(Close, mediumEMA, 1) && slowEMA[0] < mediumEMA[0])
            {
                if (showText) Draw.Text(this, "Bull" + CurrentBar, false, "Buy", 0, Low[0]- (_arrowsOffset+TextPositionOffset) * TickSize, 0, _arrowUpColor, myFont, TextAlignment.Center, Brushes.Transparent, null, 1);
				if (showTrendArrows) Draw.ArrowUp(this, "UpArrow" + CurrentBar, false, 0, Low[0] - _arrowsOffset * TickSize, _arrowUpColor);
				buycondition = true;
				sellcondition = false;
            }
            else if (CrossBelow(Close, fastEMA, 1) && CrossBelow(Close, mediumEMA, 1) && slowEMA[0] > mediumEMA[0])
            {
                if (showText) Draw.Text(this, "Bear" + CurrentBar, false, "Sell", 0, High[0] + (_arrowsOffset+TextPositionOffset) * TickSize, 0, _arrowDownColor, myFont, TextAlignment.Center, Brushes.Transparent, null, 1);
				if (showTrendArrows) Draw.ArrowDown(this, "DownArrow" + CurrentBar, false, 0, High[0] + _arrowsOffset * TickSize, _arrowDownColor);
				buycondition = false;
				sellcondition = true;
            }
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> UpperEMACloud
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> LowerEMACloud
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> AlphaTrend
        {
            get { return Values[2]; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Fast EMA Length", GroupName = "Parameters", Order = 1)]
        public int fastEMALength { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show Fast EMA Plot?", GroupName = "Parameters", Order = 2)]
		public bool showfastEMAPlot
		{ get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Medium EMA Length", GroupName = "Parameters", Order = 3)]
        public int mediumEMALength { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show Medium EMA Plot?", GroupName = "Parameters", Order = 4)]
		public bool showmediumEMAPlot
		{ get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Slow EMA Length", GroupName = "Parameters", Order = 5)]
        public int slowEMALength { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show Slow EMA Plot?", GroupName = "Parameters", Order = 6)]
		public bool showslowEMAPlot
		{ get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Color EMA Lines", Description = "Coloring based on Cross Over between Slow & Fast/Medium EMAs", GroupName = "Parameters", Order = 7)]
        public bool colorEMAs { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Color Bars", GroupName = "Parameters", Order = 8)]
        public bool colorBars { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Color Region?", GroupName = "Parameters", Order = 9)]
        public bool ColorRegion
		{ get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "Region Opacity", GroupName = "Parameters", Order = 10)]
        public int RegionOpacity
        { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show Buy/Sell Text?", GroupName = "Parameters", Order = 11)]
		public bool showText
		{ get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show Trend style arrows?", GroupName = "Parameters", Order = 12)]
        public bool showTrendArrows { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Arrows' Position Offset in Ticks", GroupName = "Parameters", Order = 13)]
        public int ArrowsPositionOffset
        {
            get { return _arrowsOffset; }
            set { _arrowsOffset = value; }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Text Position Offset in Ticks", GroupName = "Parameters", Order = 14)]
        public int TextPositionOffset { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Text Font", GroupName = "Parameters", Order = 15)]
        public SimpleFont MyFont
        {
            get { return myFont; }
            set { myFont = value; }
        }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="UpTrend Color", Order=1, GroupName="Colors")]
		public Brush UpTrend
		{ get; set; }

		[Browsable(false)]
		public string UpTrendSerializable
		{
			get { return Serialize.BrushToString(UpTrend); }
			set { UpTrend = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DownTrend Color", Order=2, GroupName="Colors")]
		public Brush DownTrend
		{ get; set; }

		[Browsable(false)]
		public string DownTrendSerializable
		{
			get { return Serialize.BrushToString(DownTrend); }
			set { DownTrend = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Neutral Color", Order=3, GroupName="Colors")]
		public Brush NeuTrend
		{ get; set; }

		[Browsable(false)]
		public string NeuTrendSerializable
		{
			get { return Serialize.BrushToString(NeuTrend); }
			set { NeuTrend = Serialize.StringToBrush(value); }
		}

        [NinjaScriptProperty]
        [Display(Name = "Arrow Up Color", Description = "Up Arrow color", GroupName = "Colors", Order = 4)]
		[XmlIgnore()]
        public Brush _arrowUpColor
		{ get; set; }

		[Browsable(false)]
        public string ArrowUpColorSerialize
        {
            get { return Serialize.BrushToString(_arrowUpColor); }
            set { _arrowUpColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Arrow Down Color", Description = "Down Arrow color", GroupName = "Colors", Order = 5)]
		[XmlIgnore()]
        public Brush _arrowDownColor
		{ get; set; }

		[Browsable(false)]
        public string ArrowDownColorSerialize
        {
            get { return Serialize.BrushToString(_arrowDownColor); }
            set { _arrowDownColor = Serialize.StringToBrush(value); }
        }

		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TripleEMAindicator[] cacheTripleEMAindicator;
		public TripleEMAindicator TripleEMAindicator(int fastEMALength, bool showfastEMAPlot, int mediumEMALength, bool showmediumEMAPlot, int slowEMALength, bool showslowEMAPlot, bool colorEMAs, bool colorBars, bool colorRegion, int regionOpacity, bool showText, bool showTrendArrows, int arrowsPositionOffset, int textPositionOffset, SimpleFont myFont, Brush upTrend, Brush downTrend, Brush neuTrend, Brush _arrowUpColor, Brush _arrowDownColor)
		{
			return TripleEMAindicator(Input, fastEMALength, showfastEMAPlot, mediumEMALength, showmediumEMAPlot, slowEMALength, showslowEMAPlot, colorEMAs, colorBars, colorRegion, regionOpacity, showText, showTrendArrows, arrowsPositionOffset, textPositionOffset, myFont, upTrend, downTrend, neuTrend, _arrowUpColor, _arrowDownColor);
		}

		public TripleEMAindicator TripleEMAindicator(ISeries<double> input, int fastEMALength, bool showfastEMAPlot, int mediumEMALength, bool showmediumEMAPlot, int slowEMALength, bool showslowEMAPlot, bool colorEMAs, bool colorBars, bool colorRegion, int regionOpacity, bool showText, bool showTrendArrows, int arrowsPositionOffset, int textPositionOffset, SimpleFont myFont, Brush upTrend, Brush downTrend, Brush neuTrend, Brush _arrowUpColor, Brush _arrowDownColor)
		{
			if (cacheTripleEMAindicator != null)
				for (int idx = 0; idx < cacheTripleEMAindicator.Length; idx++)
					if (cacheTripleEMAindicator[idx] != null && cacheTripleEMAindicator[idx].fastEMALength == fastEMALength && cacheTripleEMAindicator[idx].showfastEMAPlot == showfastEMAPlot && cacheTripleEMAindicator[idx].mediumEMALength == mediumEMALength && cacheTripleEMAindicator[idx].showmediumEMAPlot == showmediumEMAPlot && cacheTripleEMAindicator[idx].slowEMALength == slowEMALength && cacheTripleEMAindicator[idx].showslowEMAPlot == showslowEMAPlot && cacheTripleEMAindicator[idx].colorEMAs == colorEMAs && cacheTripleEMAindicator[idx].colorBars == colorBars && cacheTripleEMAindicator[idx].ColorRegion == colorRegion && cacheTripleEMAindicator[idx].RegionOpacity == regionOpacity && cacheTripleEMAindicator[idx].showText == showText && cacheTripleEMAindicator[idx].showTrendArrows == showTrendArrows && cacheTripleEMAindicator[idx].ArrowsPositionOffset == arrowsPositionOffset && cacheTripleEMAindicator[idx].TextPositionOffset == textPositionOffset && cacheTripleEMAindicator[idx].MyFont == myFont && cacheTripleEMAindicator[idx].UpTrend == upTrend && cacheTripleEMAindicator[idx].DownTrend == downTrend && cacheTripleEMAindicator[idx].NeuTrend == neuTrend && cacheTripleEMAindicator[idx]._arrowUpColor == _arrowUpColor && cacheTripleEMAindicator[idx]._arrowDownColor == _arrowDownColor && cacheTripleEMAindicator[idx].EqualsInput(input))
						return cacheTripleEMAindicator[idx];
			return CacheIndicator<TripleEMAindicator>(new TripleEMAindicator(){ fastEMALength = fastEMALength, showfastEMAPlot = showfastEMAPlot, mediumEMALength = mediumEMALength, showmediumEMAPlot = showmediumEMAPlot, slowEMALength = slowEMALength, showslowEMAPlot = showslowEMAPlot, colorEMAs = colorEMAs, colorBars = colorBars, ColorRegion = colorRegion, RegionOpacity = regionOpacity, showText = showText, showTrendArrows = showTrendArrows, ArrowsPositionOffset = arrowsPositionOffset, TextPositionOffset = textPositionOffset, MyFont = myFont, UpTrend = upTrend, DownTrend = downTrend, NeuTrend = neuTrend, _arrowUpColor = _arrowUpColor, _arrowDownColor = _arrowDownColor }, input, ref cacheTripleEMAindicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TripleEMAindicator TripleEMAindicator(int fastEMALength, bool showfastEMAPlot, int mediumEMALength, bool showmediumEMAPlot, int slowEMALength, bool showslowEMAPlot, bool colorEMAs, bool colorBars, bool colorRegion, int regionOpacity, bool showText, bool showTrendArrows, int arrowsPositionOffset, int textPositionOffset, SimpleFont myFont, Brush upTrend, Brush downTrend, Brush neuTrend, Brush _arrowUpColor, Brush _arrowDownColor)
		{
			return indicator.TripleEMAindicator(Input, fastEMALength, showfastEMAPlot, mediumEMALength, showmediumEMAPlot, slowEMALength, showslowEMAPlot, colorEMAs, colorBars, colorRegion, regionOpacity, showText, showTrendArrows, arrowsPositionOffset, textPositionOffset, myFont, upTrend, downTrend, neuTrend, _arrowUpColor, _arrowDownColor);
		}

		public Indicators.TripleEMAindicator TripleEMAindicator(ISeries<double> input , int fastEMALength, bool showfastEMAPlot, int mediumEMALength, bool showmediumEMAPlot, int slowEMALength, bool showslowEMAPlot, bool colorEMAs, bool colorBars, bool colorRegion, int regionOpacity, bool showText, bool showTrendArrows, int arrowsPositionOffset, int textPositionOffset, SimpleFont myFont, Brush upTrend, Brush downTrend, Brush neuTrend, Brush _arrowUpColor, Brush _arrowDownColor)
		{
			return indicator.TripleEMAindicator(input, fastEMALength, showfastEMAPlot, mediumEMALength, showmediumEMAPlot, slowEMALength, showslowEMAPlot, colorEMAs, colorBars, colorRegion, regionOpacity, showText, showTrendArrows, arrowsPositionOffset, textPositionOffset, myFont, upTrend, downTrend, neuTrend, _arrowUpColor, _arrowDownColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TripleEMAindicator TripleEMAindicator(int fastEMALength, bool showfastEMAPlot, int mediumEMALength, bool showmediumEMAPlot, int slowEMALength, bool showslowEMAPlot, bool colorEMAs, bool colorBars, bool colorRegion, int regionOpacity, bool showText, bool showTrendArrows, int arrowsPositionOffset, int textPositionOffset, SimpleFont myFont, Brush upTrend, Brush downTrend, Brush neuTrend, Brush _arrowUpColor, Brush _arrowDownColor)
		{
			return indicator.TripleEMAindicator(Input, fastEMALength, showfastEMAPlot, mediumEMALength, showmediumEMAPlot, slowEMALength, showslowEMAPlot, colorEMAs, colorBars, colorRegion, regionOpacity, showText, showTrendArrows, arrowsPositionOffset, textPositionOffset, myFont, upTrend, downTrend, neuTrend, _arrowUpColor, _arrowDownColor);
		}

		public Indicators.TripleEMAindicator TripleEMAindicator(ISeries<double> input , int fastEMALength, bool showfastEMAPlot, int mediumEMALength, bool showmediumEMAPlot, int slowEMALength, bool showslowEMAPlot, bool colorEMAs, bool colorBars, bool colorRegion, int regionOpacity, bool showText, bool showTrendArrows, int arrowsPositionOffset, int textPositionOffset, SimpleFont myFont, Brush upTrend, Brush downTrend, Brush neuTrend, Brush _arrowUpColor, Brush _arrowDownColor)
		{
			return indicator.TripleEMAindicator(input, fastEMALength, showfastEMAPlot, mediumEMALength, showmediumEMAPlot, slowEMALength, showslowEMAPlot, colorEMAs, colorBars, colorRegion, regionOpacity, showText, showTrendArrows, arrowsPositionOffset, textPositionOffset, myFont, upTrend, downTrend, neuTrend, _arrowUpColor, _arrowDownColor);
		}
	}
}

#endregion
