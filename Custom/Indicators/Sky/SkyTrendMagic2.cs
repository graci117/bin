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
namespace NinjaTrader.NinjaScript.Indicators.Sky
{
	public class SkyTrendMagic2 : Indicator
	{
		#region Private
		
		private const string SystemName = "SkyTrendMagic2";
        private const string FullSystemName = SystemName;
		
		public override string DisplayName
		{
			get
			{
				if (State == State.SetDefaults)
					return FullSystemName;
				else if (ShowIndicatorName)
					return FullSystemName;
				else
					return "";
			}
		}
		
		private double cciVal 					= 0.0;
		private double atrVal 					= 0.0;
		private double upTrend 					= 0.0;
		private double downTrend 				= 0.0;
		
		private double atrValAlt;
	    private double upTrendAlt;
	    private double downTrendAlt;

		private Series<double> lineColor;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description						= @"An ATR based trendline regulated by the position of CCI and its zero line.";
				Name							= "SkyTrendMagic2";
				IsOverlay						= true; 
				IsSuspendedWhileInactive		= true;
				PaintPriceMarkers				= false;
				IsAutoScale 					= false;
				///-----------------------------------------------------------------------------------------------
				cciPeriod 						= 19;
				atrPeriod 						= 12;
				atrMult							= 2.0;
				atrMultAlt						= 0.5;
				useCciTrendColor				= false;
				BullBrush						= Brushes.Cyan;
				BearBrush						= Brushes.DeepPink;
				///--------------------------------------------------
				HighlightChart					= false;
				BullChart						= Brushes.DodgerBlue;
				BearChart						= Brushes.Red;
				BullOpacity						= 20;
				BearOpacity						= 20;
				///--------------------------------------------------
				EnableShading = true;
                ShadingOpacity = 20;
				///--------------------------------------------------
				AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Line, "Trend");
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "TrendAlt");
				AddPlot(Brushes.Transparent, "Direction");		// provides +1 while in up direction or -1 while in down
				///--------------------------------------------------
				
				
			}
			
			else if (State == State.Configure)
			{
			///https://ninjatrader.com/support/forum/forum/ninjatrader-8/indicator-development/1090657-issue-with-backbrush
				
				Brush tempB = BullChart.Clone(); //Copy the brush into a temporary brush
				tempB.Opacity = BullOpacity	 / 100.0; // set the opacity
				tempB.Freeze(); // freeze the temp brush
				BullChart = tempB; // assign the temp brush value to BullChart.
				
				Brush tempS = BearChart.Clone(); //Copy the brush into a temporary brush
				tempS.Opacity = BearOpacity / 100.0; // set the opacity
				tempS.Freeze(); // freeze the temp brush
				BearChart = tempS; // assign the temp brush value to BearChart.
			}
			
			else if (State == State.DataLoaded)
			{
				Trend[0] = 0;
				TrendAlt[0] = 0;
				lineColor = new Series<double>(this);
			}
		}
		

		protected override void OnBarUpdate()
		{
		    if (CurrentBar < cciPeriod || CurrentBar < atrPeriod)
		        return;
		
		    cciVal = CCI(Close, cciPeriod)[0];
		    atrVal = ATR(Close, atrPeriod)[0];
		    atrValAlt = ATR(Close, atrPeriod)[0];  // Using the same ATR period for alt
		
		    upTrend = Low[0] - atrVal * atrMult;
		    downTrend = High[0] + atrVal * atrMult;
		
		    upTrendAlt = Low[0] - atrValAlt * atrMultAlt;
		    downTrendAlt = High[0] + atrValAlt * atrMultAlt;
		
		    // Main Trend calculation
		    if (cciVal >= 0)
		        Trend[0] = (upTrend < Trend[1]) ? Trend[1] : upTrend;
		    else
		        Trend[0] = (downTrend > Trend[1]) ? Trend[1] : downTrend;
		
		    // Alternate Trend calculation
		    if (cciVal >= 0)
		        TrendAlt[0] = (upTrendAlt < TrendAlt[1]) ? TrendAlt[1] : upTrendAlt;
		    else
		        TrendAlt[0] = (downTrendAlt > TrendAlt[1]) ? TrendAlt[1] : downTrendAlt;
		
		    // Compare Trend and TrendAlt to determine color
		    if (TrendAlt[0] > Trend[0])
		    {
		        PlotBrushes[0][0] = BullBrush;  // For Trend
		        PlotBrushes[1][0] = BullBrush;  // For TrendAlt
		    }
		    else if (TrendAlt[0] < Trend[0])
		    {
		        PlotBrushes[0][0] = BearBrush;  // For Trend
		        PlotBrushes[1][0] = BearBrush;  // For TrendAlt
		    }
		    else
		    {
		        // Optionally handle when TrendAlt is equal to Trend (e.g., keep previous color or make neutral)
		        PlotBrushes[0][0] = Brushes.Transparent;  // Example: no color when equal
		        PlotBrushes[1][0] = Brushes.Transparent;
		    }
		
		    // Direction logic (optional if needed)
		    Direction[0] = Trend[0] > Trend[1] ? 1 : -1;
		
		    // Highlight chart background if enabled
		    if (HighlightChart)
		    {
		        if (TrendAlt[0] > Trend[0])
		            BackBrush = BullChart;
		        else if (TrendAlt[0] < Trend[0])
		            BackBrush = BearChart;
		    }
			
			if (EnableShading)
	        {
	            if (TrendAlt[0] > Trend[0])
			    {
			        Draw.Region(this, "BullRegion" + CurrentBar, 0, 1, Trend, TrendAlt, null, BullBrush, ShadingOpacity, 0);
			    }
			    else if (TrendAlt[0] < Trend[0])
			    {
			        Draw.Region(this, "BearRegion" + CurrentBar, 0, 1, Trend, TrendAlt, null, BearBrush, ShadingOpacity, 0);
			    }
	        }
			
		}

		#region Properties.
		
		#region Name
		
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[NinjaScriptProperty]
		[Display(Name = "ShowIndicatorName", GroupName = "0) Indicator Information", Order = 1)]
		public bool ShowIndicatorName
		{ get; set; }
		
		#endregion
		
		#region MainProperties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CCI Period", Order=1, GroupName="Parameters")]
		public int cciPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATR Period", Order=2, GroupName="Parameters")]
		public int atrPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-20, int.MaxValue)]
		[Display(Name="ATR Multiplier", Order=3, GroupName="Parameters")]
		public double atrMult
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-20, int.MaxValue)]
		[Display(Name="ATR Multiplier 2", Order=4, GroupName="Parameters")]
		public double atrMultAlt
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use CCI Trend for Line Color", Order=5, GroupName="Parameters")]
		public bool useCciTrendColor
		{ get; set; }
		
		[XmlIgnore()]
		[Display(Name = "Bull Line Color", GroupName="2. BarColor", Order=0)]
		public Brush BullBrush
		{ get; set; }

		[Browsable(false)]
		public string BullBrushSerialize
		{
			get { return Serialize.BrushToString(BullBrush); }
   			set { BullBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[Display(Name = "Bear Line Color", GroupName="2. BarColor", Order=1)]
		public Brush BearBrush
		{ get; set; }

		[Browsable(false)]
		public string BearBrushSerialize
		{
			get { return Serialize.BrushToString(BearBrush); }
   			set { BearBrush = Serialize.StringToBrush(value); }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrendAlt
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Direction
		{
			get { return Values[2]; }
		}
		//This code has been modified from the original. All credit goes to original author
		
		#endregion
		
		#region TradeSimple Added
	
		[NinjaScriptProperty]
		[Display(Name="Highlight Chart", Order=0, GroupName="1. Highlight Chart")]
		public bool HighlightChart
		{ get; set; }
		
		[XmlIgnore()]
		[Display(Name = "Bull Color ", GroupName="1. Highlight Chart", Order=1)]
		public Brush BullChart
		{ get; set; }

		[Browsable(false)]
		public string BullChartSerialize
		{
			get { return Serialize.BrushToString(BullChart); }
   			set { BullChart = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Bull Opacity", Order=2, GroupName="1. Highlight Chart")]
		public int BullOpacity
		{ get; set; }
		
		
		
		
		[XmlIgnore()]
		[Display(Name = "Bear Color", GroupName="1. Highlight Chart", Order=3)]
		public Brush BearChart
		{ get; set; }

		[Browsable(false)]
		public string BearChartSerialize
		{
			get { return Serialize.BrushToString(BearChart); }
   			set { BearChart = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name="Bear Opacity", Order=4, GroupName="1. Highlight Chart")]
		public int BearOpacity
		{ get; set; }
		
		#endregion
		
		#region Shading
		
		[NinjaScriptProperty]
	    [Display(Name = "Enable Shading", GroupName = "Shading", Order = 0)]
	    public bool EnableShading
	    { get; set; }
	
	    [NinjaScriptProperty]
	    [Range(0, 100)]
	    [Display(Name = "Shading Opacity", GroupName = "Shading", Order = 1)]
	    public int ShadingOpacity
	    { get; set; }
		
		#endregion
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Sky.SkyTrendMagic2[] cacheSkyTrendMagic2;
		public Sky.SkyTrendMagic2 SkyTrendMagic2(string indicatorName, bool showIndicatorName, int cciPeriod, int atrPeriod, double atrMult, double atrMultAlt, bool useCciTrendColor, bool highlightChart, int bullOpacity, int bearOpacity, bool enableShading, int shadingOpacity)
		{
			return SkyTrendMagic2(Input, indicatorName, showIndicatorName, cciPeriod, atrPeriod, atrMult, atrMultAlt, useCciTrendColor, highlightChart, bullOpacity, bearOpacity, enableShading, shadingOpacity);
		}

		public Sky.SkyTrendMagic2 SkyTrendMagic2(ISeries<double> input, string indicatorName, bool showIndicatorName, int cciPeriod, int atrPeriod, double atrMult, double atrMultAlt, bool useCciTrendColor, bool highlightChart, int bullOpacity, int bearOpacity, bool enableShading, int shadingOpacity)
		{
			if (cacheSkyTrendMagic2 != null)
				for (int idx = 0; idx < cacheSkyTrendMagic2.Length; idx++)
					if (cacheSkyTrendMagic2[idx] != null && cacheSkyTrendMagic2[idx].IndicatorName == indicatorName && cacheSkyTrendMagic2[idx].ShowIndicatorName == showIndicatorName && cacheSkyTrendMagic2[idx].cciPeriod == cciPeriod && cacheSkyTrendMagic2[idx].atrPeriod == atrPeriod && cacheSkyTrendMagic2[idx].atrMult == atrMult && cacheSkyTrendMagic2[idx].atrMultAlt == atrMultAlt && cacheSkyTrendMagic2[idx].useCciTrendColor == useCciTrendColor && cacheSkyTrendMagic2[idx].HighlightChart == highlightChart && cacheSkyTrendMagic2[idx].BullOpacity == bullOpacity && cacheSkyTrendMagic2[idx].BearOpacity == bearOpacity && cacheSkyTrendMagic2[idx].EnableShading == enableShading && cacheSkyTrendMagic2[idx].ShadingOpacity == shadingOpacity && cacheSkyTrendMagic2[idx].EqualsInput(input))
						return cacheSkyTrendMagic2[idx];
			return CacheIndicator<Sky.SkyTrendMagic2>(new Sky.SkyTrendMagic2(){ IndicatorName = indicatorName, ShowIndicatorName = showIndicatorName, cciPeriod = cciPeriod, atrPeriod = atrPeriod, atrMult = atrMult, atrMultAlt = atrMultAlt, useCciTrendColor = useCciTrendColor, HighlightChart = highlightChart, BullOpacity = bullOpacity, BearOpacity = bearOpacity, EnableShading = enableShading, ShadingOpacity = shadingOpacity }, input, ref cacheSkyTrendMagic2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Sky.SkyTrendMagic2 SkyTrendMagic2(string indicatorName, bool showIndicatorName, int cciPeriod, int atrPeriod, double atrMult, double atrMultAlt, bool useCciTrendColor, bool highlightChart, int bullOpacity, int bearOpacity, bool enableShading, int shadingOpacity)
		{
			return indicator.SkyTrendMagic2(Input, indicatorName, showIndicatorName, cciPeriod, atrPeriod, atrMult, atrMultAlt, useCciTrendColor, highlightChart, bullOpacity, bearOpacity, enableShading, shadingOpacity);
		}

		public Indicators.Sky.SkyTrendMagic2 SkyTrendMagic2(ISeries<double> input , string indicatorName, bool showIndicatorName, int cciPeriod, int atrPeriod, double atrMult, double atrMultAlt, bool useCciTrendColor, bool highlightChart, int bullOpacity, int bearOpacity, bool enableShading, int shadingOpacity)
		{
			return indicator.SkyTrendMagic2(input, indicatorName, showIndicatorName, cciPeriod, atrPeriod, atrMult, atrMultAlt, useCciTrendColor, highlightChart, bullOpacity, bearOpacity, enableShading, shadingOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Sky.SkyTrendMagic2 SkyTrendMagic2(string indicatorName, bool showIndicatorName, int cciPeriod, int atrPeriod, double atrMult, double atrMultAlt, bool useCciTrendColor, bool highlightChart, int bullOpacity, int bearOpacity, bool enableShading, int shadingOpacity)
		{
			return indicator.SkyTrendMagic2(Input, indicatorName, showIndicatorName, cciPeriod, atrPeriod, atrMult, atrMultAlt, useCciTrendColor, highlightChart, bullOpacity, bearOpacity, enableShading, shadingOpacity);
		}

		public Indicators.Sky.SkyTrendMagic2 SkyTrendMagic2(ISeries<double> input , string indicatorName, bool showIndicatorName, int cciPeriod, int atrPeriod, double atrMult, double atrMultAlt, bool useCciTrendColor, bool highlightChart, int bullOpacity, int bearOpacity, bool enableShading, int shadingOpacity)
		{
			return indicator.SkyTrendMagic2(input, indicatorName, showIndicatorName, cciPeriod, atrPeriod, atrMult, atrMultAlt, useCciTrendColor, highlightChart, bullOpacity, bearOpacity, enableShading, shadingOpacity);
		}
	}
}

#endregion
