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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class JurbolBBmacd : Indicator
	{
		#region Variables
		private int fast 				= 12;
		private int slow 				= 26;
		private int smooth 				= 1;
		private double numStdDev 		= 0.882;
		private int period 				= 10;
		private int opacity 			= 3;
		private int arrowSize 			= 14;
		private double arrowOffset 		= 0.5;
		private Brush bullColor 		= Brushes.Blue;
		private Brush bearColor 		= Brushes.Maroon;
		private Brush neutralColor 		= Brushes.Yellow;
		private Brush fillColor 		= Brushes.CadetBlue;
		private Brush bullArrowColor 	= Brushes.Cyan;
		private Brush bearArrowColor 	= Brushes.Magenta;
		
		#endregion
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Jurbol BBMacd. Combination of Bollinger Bands and Macd. Programmed for NT 8.";
				Name						= "JurbolBBmacd";
				Calculate					= Calculate.OnEachTick;
				IsOverlay					= false;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= false;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				AddPlot(new Stroke(Brushes.Orange, 4), PlotStyle.Dot, "Macd");
				AddPlot(new Stroke(Brushes.Black, 1), PlotStyle.Line, "Avg");
				AddPlot(new Stroke(Brushes.LimeGreen, 2), PlotStyle.Line, "BUpper");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "BLower");
				AddLine(Brushes.DarkGray, 0, "Zero line");
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1)
				return;
			Draw.Region(this, "Fill_Band", CurrentBar, 0, BollingerUpper, BollingerLower, Brushes.Transparent, fillColor, opacity);
			Macd[0] = MACD(fast, slow, smooth)[0];
			Avg[0] = MACD(fast, slow, smooth)[0];
			BollingerUpper[0] = Bollinger(MACD(fast, slow, smooth), numStdDev, period).Upper[0];
			BollingerLower[0] = Bollinger(MACD(fast, slow, smooth), numStdDev, period).Lower[0];
			if(IsRising(Macd))
			{
				PlotBrushes[0][0] = bullColor;
			}
			else if(IsFalling(Macd))
			{
				PlotBrushes[0][0] = bearColor;
			}
			else
			{
				PlotBrushes[0][0] = neutralColor;
			}
			NinjaTrader.Gui.Tools.SimpleFont arrowFont = new NinjaTrader.Gui.Tools.SimpleFont("Wingdings", arrowSize){ Bold = true };
			if(CrossAbove(MACD(fast, slow, smooth), 0, 1))
			{
				Draw.Text(this, "CrossingUp"+CurrentBar, false, "é", 0, Macd[0]+arrowOffset*TickSize,0, bullArrowColor, arrowFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
			else if(CrossBelow(MACD(fast, slow, smooth), 0, 1))
			{
				Draw.Text(this, "CrossingDn"+CurrentBar, false, "ê", 0, Macd[0]-arrowOffset*TickSize,0, bearArrowColor, arrowFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
			
		}
        #region Properties
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "01. Fast", Description="Number of bars for fast EMA", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Fast
		{
			get { return fast; }
			set { fast = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "01. Slow", Description="Number of bars for slow EMA", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Slow
		{
			get { return slow; }
			set { slow = Math.Max(1, value); }
		}

		/// <summary>
		/// </summary>
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "03. Smooth", Description="Number of bars for smoothing", GroupName = "NinjaScriptParameters", Order = 2)]
		public int Smooth
		{
			get { return smooth; }
			set { smooth = Math.Max(1, value); }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "04. BB std. dev.", Description="Number of standard deviations", GroupName = "NinjaScriptParameters", Order = 3)]
		public double NumStdDev
		{
			get { return numStdDev; }
			set { numStdDev = Math.Max(0, value); }
		}

		/// <summary>
		/// </summary>
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "05. BB Period", Description="Numbers of bars used for calculations", GroupName = "NinjaScriptParameters", Order = 4)]
		public int Period
		{
			get { return period; }
			set { period = Math.Max(1, value); }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "06. FillColor Opacity", Description="Fill Opacity", GroupName = "NinjaScriptParameters", Order = 5)]
        public int Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "06. Arrow Size", Description="ArrowSize", GroupName = "NinjaScriptParameters", Order = 6)]
        public int ArrowSize
        {
            get { return arrowSize; }
            set { arrowSize = value; }
        }
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "07. Arrow Offset from MacdLine", Description="ArrowOffset", GroupName = "NinjaScriptParameters", Order = 7)]
        public double ArrowOffset
        {
            get { return arrowOffset; }
            set { arrowOffset = value; }
        }
		
		
        [Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> Macd
        {
            get { return Values[0]; }
        }

         [Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> Avg
        {
            get { return Values[1]; }
        }
		  [Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> BollingerUpper
        {
            get { return Values[2]; }
        }
		
		  [Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> BollingerLower
        {
            get { return Values[3]; }
        }
		//----Visual---------------------------------------------------
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "01. Color Bull Dots", Description="Color for Up Bars", GroupName = "Visual", Order = 0)]
        public Brush BullColor
        {
            get { return bullColor; }
            set { bullColor = value; }
        }
		[Browsable(false)]
		public string BullColorSerialize
		{
			get { return Serialize.BrushToString(BullColor); }
			set { BullColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "02. Color Bear Dots", Description="Color for Down Bars", GroupName = "Visual", Order = 1)]
        public Brush BearColor
        {
            get { return bearColor; }
            set { bearColor = value; }
        }
		[Browsable(false)]
		public string BearColorSerialize
		{
			get { return Serialize.BrushToString(BearColor); }
			set { BearColor = Serialize.StringToBrush(value); }
		}
		//START HERE
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "03. Color Neutral Dots", Description="Color for NN Bars", GroupName = "Visual", Order = 2)]
        public Brush NeutralColor
        {
            get { return neutralColor; }
            set { neutralColor = value; }
        }
		[Browsable(false)]
		public string NeutralColorSerialize
		{
			get { return Serialize.BrushToString(NeutralColor); }
			set { NeutralColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "04. Fill Color", Description="Color for fill", GroupName = "Visual", Order = 3)]
        public Brush FillColor
        {
            get { return fillColor; }
            set { fillColor = value; }
        }
		[Browsable(false)]
		public string FillColorSerialize
		{
			get { return Serialize.BrushToString(FillColor); }
			set { FillColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "05. Color Up Arrows", Description="Arrow Crossing up Color", GroupName = "Visual", Order = 4)]
        public Brush BullArrowColor
        {
            get { return bullArrowColor; }
            set { bullArrowColor = value; }
        }
		[Browsable(false)]
		public string BullArrowColorSerialize
		{
			get { return Serialize.BrushToString(BullArrowColor); }
			set { BullArrowColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "06. Color Dn Arrows", Description="Arrow Crossing udn Color", GroupName = "Visual", Order = 5)]
        public Brush BearArrowColor
        {
            get { return bearArrowColor; }
            set { bearArrowColor = value; }
        }
		[Browsable(false)]
		public string BearArrowColorSerialize
		{
			get { return Serialize.BrushToString(BearArrowColor); }
			set { BearArrowColor = Serialize.StringToBrush(value); }
		}
		
        #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private JurbolBBmacd[] cacheJurbolBBmacd;
		public JurbolBBmacd JurbolBBmacd(int fast, int slow, int smooth, double numStdDev, int period, int opacity, int arrowSize, double arrowOffset, Brush bullColor, Brush bearColor, Brush neutralColor, Brush fillColor, Brush bullArrowColor, Brush bearArrowColor)
		{
			return JurbolBBmacd(Input, fast, slow, smooth, numStdDev, period, opacity, arrowSize, arrowOffset, bullColor, bearColor, neutralColor, fillColor, bullArrowColor, bearArrowColor);
		}

		public JurbolBBmacd JurbolBBmacd(ISeries<double> input, int fast, int slow, int smooth, double numStdDev, int period, int opacity, int arrowSize, double arrowOffset, Brush bullColor, Brush bearColor, Brush neutralColor, Brush fillColor, Brush bullArrowColor, Brush bearArrowColor)
		{
			if (cacheJurbolBBmacd != null)
				for (int idx = 0; idx < cacheJurbolBBmacd.Length; idx++)
					if (cacheJurbolBBmacd[idx] != null && cacheJurbolBBmacd[idx].Fast == fast && cacheJurbolBBmacd[idx].Slow == slow && cacheJurbolBBmacd[idx].Smooth == smooth && cacheJurbolBBmacd[idx].NumStdDev == numStdDev && cacheJurbolBBmacd[idx].Period == period && cacheJurbolBBmacd[idx].Opacity == opacity && cacheJurbolBBmacd[idx].ArrowSize == arrowSize && cacheJurbolBBmacd[idx].ArrowOffset == arrowOffset && cacheJurbolBBmacd[idx].BullColor == bullColor && cacheJurbolBBmacd[idx].BearColor == bearColor && cacheJurbolBBmacd[idx].NeutralColor == neutralColor && cacheJurbolBBmacd[idx].FillColor == fillColor && cacheJurbolBBmacd[idx].BullArrowColor == bullArrowColor && cacheJurbolBBmacd[idx].BearArrowColor == bearArrowColor && cacheJurbolBBmacd[idx].EqualsInput(input))
						return cacheJurbolBBmacd[idx];
			return CacheIndicator<JurbolBBmacd>(new JurbolBBmacd(){ Fast = fast, Slow = slow, Smooth = smooth, NumStdDev = numStdDev, Period = period, Opacity = opacity, ArrowSize = arrowSize, ArrowOffset = arrowOffset, BullColor = bullColor, BearColor = bearColor, NeutralColor = neutralColor, FillColor = fillColor, BullArrowColor = bullArrowColor, BearArrowColor = bearArrowColor }, input, ref cacheJurbolBBmacd);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.JurbolBBmacd JurbolBBmacd(int fast, int slow, int smooth, double numStdDev, int period, int opacity, int arrowSize, double arrowOffset, Brush bullColor, Brush bearColor, Brush neutralColor, Brush fillColor, Brush bullArrowColor, Brush bearArrowColor)
		{
			return indicator.JurbolBBmacd(Input, fast, slow, smooth, numStdDev, period, opacity, arrowSize, arrowOffset, bullColor, bearColor, neutralColor, fillColor, bullArrowColor, bearArrowColor);
		}

		public Indicators.JurbolBBmacd JurbolBBmacd(ISeries<double> input , int fast, int slow, int smooth, double numStdDev, int period, int opacity, int arrowSize, double arrowOffset, Brush bullColor, Brush bearColor, Brush neutralColor, Brush fillColor, Brush bullArrowColor, Brush bearArrowColor)
		{
			return indicator.JurbolBBmacd(input, fast, slow, smooth, numStdDev, period, opacity, arrowSize, arrowOffset, bullColor, bearColor, neutralColor, fillColor, bullArrowColor, bearArrowColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.JurbolBBmacd JurbolBBmacd(int fast, int slow, int smooth, double numStdDev, int period, int opacity, int arrowSize, double arrowOffset, Brush bullColor, Brush bearColor, Brush neutralColor, Brush fillColor, Brush bullArrowColor, Brush bearArrowColor)
		{
			return indicator.JurbolBBmacd(Input, fast, slow, smooth, numStdDev, period, opacity, arrowSize, arrowOffset, bullColor, bearColor, neutralColor, fillColor, bullArrowColor, bearArrowColor);
		}

		public Indicators.JurbolBBmacd JurbolBBmacd(ISeries<double> input , int fast, int slow, int smooth, double numStdDev, int period, int opacity, int arrowSize, double arrowOffset, Brush bullColor, Brush bearColor, Brush neutralColor, Brush fillColor, Brush bullArrowColor, Brush bearArrowColor)
		{
			return indicator.JurbolBBmacd(input, fast, slow, smooth, numStdDev, period, opacity, arrowSize, arrowOffset, bullColor, bearColor, neutralColor, fillColor, bullArrowColor, bearArrowColor);
		}
	}
}

#endregion
