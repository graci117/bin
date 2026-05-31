
#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

/// <summary>
/// 
/// Originally Written by @aligator for NT7 and published free on Futures.IO (Later it was pirated by a 3rd party vendor and sold as part of a package)
/// Version 1.0, October 07, 2011
/// 
/// Credit is given to Raghee Horner (IBFX) for intoducing the RGaB setup.
/// Refer to Raghee's webinars on YouTube to better understand how this setup is used.
/*
    I wrote this indicator based on a chart setup used by Raghee Horner for trading currency pairs but it can be
    applied to any instrument. So, credit for the idea goes to Ms. Horner.
 
    Although this indicator is very simple, it is very powerful when used correctly. The indicator is a trend 
    following setup that will keep one out of choppy markets and on the right side of the trend.
 
 	It is helpful to watch one of Raghee's old Youtube seminars on IBFX (if still available) to understand how this indicator is applied.
*/
/// NOTES:
/// Version 2.0, September 06, 2018 added:
/// 
/// - Optional shading of the EMA's Region.
/// - Optional removal of EMA High and Low bands.
/// - Optional candle outline to show up/down candles - Although not really needed.
/// - Draw Buy/Sell arrows filtered based on direction of the RMI indicator for best signals.
/// - Optional Historical Buy/Sell arrows.
/// - Publically exposed Buy/Sell signals for Market Analyzer and for use in Strategies.

/// Exported using: 8.0.24.2 64-bit

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.mah_Indicators
{
    /// <summary>
    /// This is an adaptation of the GRAB (GRB) setup used by Raghee Horner of InterbankFX for trading currency pairs.
    /// </summary>
    public class mahTrendGRaBer2 : Indicator
    {
        #region Variables
       		private bool 		colorbars 		= true;
			private bool 		colorzone 		= true;
       		private bool 		colorOutline 	= true;
       		private bool 		removeMABands	= false;
       		private bool 		drawArrows		= true;		
		
			private int 		zopacity	= 3;
			private Brush 		zoneColor	= Brushes.Gray;
			private Brush 		maUpColor	= Brushes.Lime;
			private Brush 		maDownColor	= Brushes.Red;
		
            private int emah = 34; // Default setting for Emah
            private int emac = 34; // Default setting for Emac
            private int emal = 34; // Default setting for Emal
		
			private Brush barColorCondition1		= Brushes.Chartreuse;
			private Brush barColorCondition2		= Brushes.Green;
			private Brush barColorCondition3		= Brushes.LightBlue;
			private Brush barColorCondition4		= Brushes.RoyalBlue;
			private Brush barColorCondition5		= Brushes.DarkOrange;
			private Brush barColorCondition6		= Brushes.Red;
		
			private Brush candleOutlineCondition1	= Brushes.Chartreuse;
			private Brush candleOutlineCondition2	= Brushes.Green;
			private Brush candleOutlineCondition3	= Brushes.LightBlue;
			private Brush candleOutlineCondition4	= Brushes.RoyalBlue;
			private Brush candleOutlineCondition5	= Brushes.DarkOrange;
			private Brush candleOutlineCondition6	= Brushes.Red;
		
		
        // User defined variables (add any user defined variables below)
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "mahTrendGRaBer2";
                Description = "This is an adaptation of the GRAB (GRB) setup used by Raghee Horner of InterbankFX for trading currency pairs.";
		        AddPlot(new Stroke(Brushes.ForestGreen), PlotStyle.Line, "EmaHigh");
		        AddPlot(new Stroke(Brushes.MediumBlue), PlotStyle.Line, "EmaClose");
		        AddPlot(new Stroke(Brushes.Red), PlotStyle.Line, "EmaLow");
				AddPlot(new Stroke(Brushes.Transparent), PlotStyle.Line, "MAnalizer");
				IsOverlay				 = true;
				ShowAll					 = false;				
            }
			
			else if (State == State.Configure)
			{
	            Plots[0].Width = 1;
				Plots[1].Width = 2;
				Plots[2].Width = 1;
				Plots[0].DashStyleHelper = DashStyleHelper.Dot;
				Plots[1].DashStyleHelper = DashStyleHelper.Dot;
				Plots[2].DashStyleHelper = DashStyleHelper.Dot;
			}
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if (CurrentBar < Emac)
				return;
			
			// Condition set 1
            if (Open[0] <= Close[0]
                && Close[0] > EMA(High, Emah)[0])
            {
				if (colorbars)
				{
				BarBrush = barColorCondition1;
				}
				if (colorOutline)
				{
                CandleOutlineBrush = candleOutlineCondition1;
				}
            }

            // Condition set 2
            if (Open[0] >= Close[0]
                && Close[0] > EMA(High, Emah)[0])
            {
				if (colorbars)
				{
				BarBrush = barColorCondition2;
				}
				if (colorOutline)
				{
                CandleOutlineBrush = candleOutlineCondition2;
				}
            }

            // Condition set 3
            if (Open[0] <= Close[0]
                && Close[0] < EMA(High, Emah)[0]
                && Close[0] > EMA(Low, Emal)[0])
            {
				if (colorbars)
				{
				BarBrush = barColorCondition3;
				}
				if (colorOutline)
				{
                CandleOutlineBrush = candleOutlineCondition3;
				}
            }

            // Condition set 4
            if (Open[0] >= Close[0]
                && Close[0] < EMA(High, Emah)[0]
                && Close[0] > EMA(Low, Emal)[0])
            {
				if (colorbars)
				{
				BarBrush = barColorCondition4;
				}
				if (colorOutline)
				{
                CandleOutlineBrush = candleOutlineCondition4;
				}
            }

            // Condition set 5
            if (Open[0] <= Close[0]
                && Close[0] < EMA(Low, Emal)[0])
            {
				if (colorbars)
				{
				BarBrush = barColorCondition5;
				}
				if (colorOutline)
				{
                CandleOutlineBrush = candleOutlineCondition5;
				}
            }

            // Condition set 6
            if (Open[0] >= Close[0]
                && Close[0] < EMA(Low, Emal)[0])
            {
				if (colorbars)
				{
				BarBrush = barColorCondition6;
				}
				if (colorOutline)
				{
                CandleOutlineBrush = candleOutlineCondition6;
				}
            }

            // Plot EMA for High, Low, and Close.
            {  EmaHigh[0] = EMA(High, Emah)[0];
               EmaClose[0] = EMA(Close, Emac)[0];
               EmaLow[0] = EMA(Low, Emal)[0];
			}
			
			if(IsRising(EmaClose))
			PlotBrushes[1][0] = MaUpColor;
			
			if(IsFalling(EmaClose))
			PlotBrushes[1][0] = MaDownColor;
			
			if(RemoveMABands)
			{
			PlotBrushes[0][0] = Brushes.Transparent;
			PlotBrushes[2][0] = Brushes.Transparent;				
			}
			
			if (Colorzone)
			{
				Draw.Region(this,"MABands",CurrentBar, 0, EmaHigh, EmaLow, Brushes.Transparent, ZoneColor, Zopacity);
			}
			
        #region Signals
			
//			if(Close[2] > Open[2] && Close[1] < Open[1] &&  Close[0] < EmaLow[0]) // Use for original signals - uncomment and comment nest line
			if (IsFalling(RMI(14,3)) && Close[1] < Open[1] && Close[0] < Open[0] && CrossBelow(EMA(1), EmaLow[0], 1)) // Use for RMI Momentum filtered signals			
			{
				if (drawArrows)
				Draw.ArrowDown(this, "ARROWDOWN" +(ShowAll ? CurrentBar.ToString() : ToString()), false, 0, High[0] + TickSize*1, Brushes.White);
				
				MAnalyzer[0] = -1;
			}
			
//			if(Close[2] < Open[2] && Close[1] > Open[1] && Close[0] > EmaHigh[0]) // Use for original signals - uncomment and comment next line
			if (IsRising(RMI(14,3)) && Close[1] > Open[1] && Close[0] > Open[0] && CrossAbove(EMA(1), EmaHigh[0], 1)) // Use for RMI Momentum filtered signals			
			{
				if (drawArrows)					
				Draw.ArrowUp(this, "ARROWUP" +(ShowAll ? CurrentBar.ToString() : ToString()), false, 0, Low[0] - TickSize*1, Brushes.White);
									
				MAnalyzer[0] = 1;
			}				
			
		#endregion
			
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> EmaHigh
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> EmaClose
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> EmaLow
        {
            get { return Values[2]; }
        }
		
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> MAnalyzer
        {
            get { return Values[3]; }
        }		

//        [NinjaScriptProperty]
        [Display(Description = "Default value for EMA of High", GroupName = "Parameters", Order = 1)]
        public int Emah
        {
            get { return emah; }
            set { emah = Math.Max(1, value); }
        }

 //       [NinjaScriptProperty]
        [Display(Description = "Default value for EMA of Close", GroupName = "Parameters", Order = 1)]
        public int Emac
        {
            get { return emac; }
            set { emac = Math.Max(1, value); }
        }

//        [NinjaScriptProperty]
        [Display(Description = "Default value for EMA of Low", GroupName = "Parameters", Order = 1)]
        public int Emal
        {
            get { return emal; }
            set { emal = Math.Max(1, value); }
        }
		
		[XmlIgnore]
//		[NinjaScriptProperty]	//Not needed for color serializatio 	
		[Display(Name = "BarCondition1", Description = "Color of BarCondition1.", GroupName = "Visual", Order = 1)]
        public Brush BarCondition1
        {
            get { return barColorCondition1; }
            set { barColorCondition1 = value; }
        }

        [Browsable(false)]
        public string BarCondition1Serialize
        {
            get { return Serialize.BrushToString(barColorCondition1); }
            set { barColorCondition1 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "BarCondition2", Description = "Color of BarCondition2.", GroupName = "Visual", Order = 2)]
        public Brush BarCondition2
        {
            get { return barColorCondition2; }
            set { barColorCondition2 = value; }
        }

        [Browsable(false)]
        public string BarCondition2Serialize
        {
            get { return Serialize.BrushToString(barColorCondition2); }
            set { barColorCondition2 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "BarCondition3", Description = "Color of BarCondition3.", GroupName = "Visual", Order = 3)]
        public Brush BarCondition3
        {
            get { return barColorCondition3; }
            set { barColorCondition3 = value; }
        }

        [Browsable(false)]
        public string BarCondition3Serialize
        {
            get { return Serialize.BrushToString(barColorCondition3); }
            set { barColorCondition3 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "BarCondition4", Description = "Color of BarCondition4.", GroupName = "Visual", Order = 4)]
        public Brush BarCondition4
        {
            get { return barColorCondition4; }
            set { barColorCondition4 = value; }
        }

        [Browsable(false)]
        public string BarCondition4Serialize
        {
            get { return Serialize.BrushToString(barColorCondition4); }
            set { barColorCondition4 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "BarCondition5", Description = "Color of BarCondition5.", GroupName = "Visual", Order = 5)]
        public Brush BarCondition5
        {
            get { return barColorCondition5; }
            set { barColorCondition5 = value; }
        }

        [Browsable(false)]
        public string BarCondition5Serialize
        {
            get { return Serialize.BrushToString(barColorCondition5); }
            set { barColorCondition5 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "BarCondition6", Description = "Color of BarCondition6.", GroupName = "Visual", Order = 6)]
        public Brush BarCondition6
        {
            get { return barColorCondition6; }
            set { barColorCondition6 = value; }
        }

        [Browsable(false)]
        public string BarCondition6Serialize
        {
            get { return Serialize.BrushToString(barColorCondition6); }
            set { barColorCondition6 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "CandleOutlineCondition1", Description = "Color of CandleOutlineCondition1.", GroupName = "Visual", Order = 1)]
        public Brush CandleOutlineCondition1
        {
            get { return candleOutlineCondition1; }
            set { candleOutlineCondition1 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition1Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition1); }
            set { candleOutlineCondition1 = Serialize.StringToBrush(value); }
        }
				
		[XmlIgnore]
				
//		[NinjaScriptProperty]				
		[Display(Name = "CandleOutlineCondition2", Description = "Color of CandleOutlineCondition2.", GroupName = "Visual", Order = 2)]
        public Brush CandleOutlineCondition2
        {
            get { return candleOutlineCondition2; }
            set { candleOutlineCondition2 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition2Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition2); }
            set { candleOutlineCondition2 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "CandleOutlineCondition3", Description = "Color of CandleOutlineCondition3.", GroupName = "Visual", Order = 3)]
        public Brush CandleOutlineCondition3
        {
            get { return candleOutlineCondition3; }
            set { candleOutlineCondition3 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition3Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition3); }
            set { candleOutlineCondition3 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "CandleOutlineCondition4", Description = "Color of CandleOutlineCondition4.", GroupName = "Visual", Order = 4)]
        public Brush CandleOutlineCondition4
        {
            get { return candleOutlineCondition4; }
            set { candleOutlineCondition4 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition4Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition4); }
            set { candleOutlineCondition4 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
//		[NinjaScriptProperty]		
		[Display(Name = "CandleOutlineCondition5", Description = "Color of CandleOutlineCondition5.", GroupName = "Visual", Order = 5)]
        public Brush CandleOutlineCondition5
        {
            get { return candleOutlineCondition5; }
            set { candleOutlineCondition5 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition5Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition5); }
            set { candleOutlineCondition5 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
		
		[Display(Name = "CandleOutlineCondition6", Description = "Color of CandleOutlineCondition6.", GroupName = "Visual", Order = 6)]
        public Brush CandleOutlineCondition6
        {
            get { return candleOutlineCondition6; }
            set { candleOutlineCondition6 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition6Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition6); }
            set { candleOutlineCondition6 = Serialize.StringToBrush(value); }
        }
		
		[NinjaScriptProperty]	
		[Display(Name = "1. Draw Arrows?", Description = "Draw Suy/Sell Arrows?", GroupName = "Colors", Order = 0)]		
		public bool DrawArrows
		{
			get { return drawArrows; }
			set { drawArrows = value; }
		}
		
		
//		[NinjaScriptProperty]		
		[Display(Name = "2. ShowAll?", Description = "Show All Arrows?", GroupName = "Colors", Order = 1)]		
		public bool ShowAll
		{get; set;} 		

		[Display(Name = "3. Remove MA Band?", Description = "Remove MA H/L Band?", GroupName = "Colors", Order = 2)]		
		public bool RemoveMABands
		{
			get { return removeMABands; }
			set { removeMABands = value; }
		}		
		
//		[NinjaScriptProperty]		
		[Display(Name = "4. Color Zone?", Description = "Color Zone?", GroupName = "Colors", Order = 3)]		
		public bool Colorzone
		{
			get { return colorzone; }
			set { colorzone = value; }
		}
		
//		[NinjaScriptProperty]		
		[Display(Name = "5. Color Bars?", Description = "Color Bars?", GroupName = "Colors", Order = 4)]		
		public bool Colorbars
		{
			get { return colorbars; }
			set { colorbars = value; }
		}
		
//		[NinjaScriptProperty]		
		[Display(Name = "6. Color Bar Outline?", Description = "Color Bars Ouline?", GroupName = "Colors", Order = 5)]		
		public bool ColorOutline
		{
			get { return colorOutline; }
			set { colorOutline = value; }
		}		
		
//		[NinjaScriptProperty]		
		[Display(Name = "7. Color Zone Opacity", Description = "Zone Opacity 1-9", GroupName = "Colors", Order = 6)]		
		public int Zopacity
		{
			get { return zopacity; }
			set { zopacity = value; }
		}
		
		[XmlIgnore()]
		
//		[NinjaScriptProperty]		
		[Display(Name = "8. Color for Rising MA?", Description = "Color for Rising MA", GroupName = "Colors", Order = 7)]
        public Brush MaUpColor
        {
            get { return maUpColor; }
            set { maUpColor = value; }
        }
		
		[Browsable(false)]
		public string MaUpColorSerialize
		{
			get { return Serialize.BrushToString(MaUpColor); }
			set { MaUpColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		
//		[NinjaScriptProperty]		
		[Display(Name = "9. Color for Falling MA?", Description = "Color for Falling MA", GroupName = "Colors", Order = 7)]
        public Brush MaDownColor
        {
            get { return maDownColor; }
            set { maDownColor = value; }
        }
		
		[Browsable(false)]
		public string MaDownColorSerialize
		{
			get { return Serialize.BrushToString(MaDownColor); }
			set { MaDownColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		
//		[NinjaScriptProperty]		
		[Display(Name = "10. Color for Zone?", Description = "Color for Zone", GroupName = "Colors", Order = 7)]
        public Brush ZoneColor
        {
            get { return zoneColor; }
            set { zoneColor = value; }
        }
		
		[Browsable(false)]
		public string ZoneColorSerialize
		{
			get { return Serialize.BrushToString(ZoneColor); }
			set { ZoneColor = Serialize.StringToBrush(value); }
		}
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private mah_Indicators.mahTrendGRaBer2[] cachemahTrendGRaBer2;
		public mah_Indicators.mahTrendGRaBer2 mahTrendGRaBer2(bool drawArrows)
		{
			return mahTrendGRaBer2(Input, drawArrows);
		}

		public mah_Indicators.mahTrendGRaBer2 mahTrendGRaBer2(ISeries<double> input, bool drawArrows)
		{
			if (cachemahTrendGRaBer2 != null)
				for (int idx = 0; idx < cachemahTrendGRaBer2.Length; idx++)
					if (cachemahTrendGRaBer2[idx] != null && cachemahTrendGRaBer2[idx].DrawArrows == drawArrows && cachemahTrendGRaBer2[idx].EqualsInput(input))
						return cachemahTrendGRaBer2[idx];
			return CacheIndicator<mah_Indicators.mahTrendGRaBer2>(new mah_Indicators.mahTrendGRaBer2(){ DrawArrows = drawArrows }, input, ref cachemahTrendGRaBer2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.mah_Indicators.mahTrendGRaBer2 mahTrendGRaBer2(bool drawArrows)
		{
			return indicator.mahTrendGRaBer2(Input, drawArrows);
		}

		public Indicators.mah_Indicators.mahTrendGRaBer2 mahTrendGRaBer2(ISeries<double> input , bool drawArrows)
		{
			return indicator.mahTrendGRaBer2(input, drawArrows);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.mah_Indicators.mahTrendGRaBer2 mahTrendGRaBer2(bool drawArrows)
		{
			return indicator.mahTrendGRaBer2(Input, drawArrows);
		}

		public Indicators.mah_Indicators.mahTrendGRaBer2 mahTrendGRaBer2(ISeries<double> input , bool drawArrows)
		{
			return indicator.mahTrendGRaBer2(input, drawArrows);
		}
	}
}

#endregion
