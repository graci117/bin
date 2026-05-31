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

	public enum CDMAtype
	{
		DEMA,
		EMA,
		HMA,
		LinReg,
		SMA,
		TEMA,
		Tillson,
		TMA,
		VWMA,
		WMA,
		ZLEMA	
	}	

	public enum DrawSelection
	{
		Triangle,
    	Arrow,
    	Dot,
		Diamond,
	}	
	


//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
		[Gui.CategoryOrder("Crossing Moving Average (fast)", 1)] 
		[Gui.CategoryOrder("Crossed Moving Average (slow)", 2)] 
		[Gui.CategoryOrder("Cross Detection", 3)] 
		[Gui.CategoryOrder("Cross Detection Actions", 4)] 

	[TypeConverter("NinjaTrader.NinjaScript.Indicators.MACrossBuilderConverter")]
	
	public class MACrossBuilder : Indicator
	{
		private Series<double> ma0Series;
		private Series<double> ma1Series;	
		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		private Brush UpColor;
		private Brush DownColor;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				// NT8 MACrossBuilder by NinjaTrader_Paul,  Version 1.0, released 6/27/17 NT8 8.0.7.1
				// version 1.01 6/28/17, removed [NinjascriptProperty] from both opacity properties.
				// Version 1.02 10/31/17 - fixed trace error of duplicate properties, added support for indicator panel usage
				// Version 1.03 01/11/18 - added PriceType selectors and function. Enabled drawmarkers true as default
				// Version 1.04 01/29/18 - Fixed slow EMA (was left as DEMA in error).
				// Version 1.05 01/30/18 - found/fixed an error when loading in market analyzer related to 1.02
				// Version 1.06 05/30/18 - Added Linear Regression as a selectable moving average.
				
				Description									= @"Provide on chart indication of two MAs crossing.  Provide input to MarketAnalyzer of same.";
				Name										= "MACrossBuilderv1.06";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ArePlotsConfigurable 						= false;
				
				AddPlot(Brushes.CornflowerBlue, "FstMA");
				AddPlot(Brushes.Magenta, 		"SlwMA");
				AddPlot(Brushes.Transparent, 	"CrossDetect");
				
				CrossLookBack								= 1;
				ShowCrossAbove								= true;
				ShowCrossBelow								= true;
				
				FastMAPeriod								= 5;
				SlowMAPeriod								= 14;
				Ma0Type										= CDMAtype.EMA;
				Ma1Type										= CDMAtype.EMA;
				
				fastPriceType								= PriceType.Close;
				slowPriceType								= PriceType.Close;

				SoundsOn									= false;				
				UpSoundFile									= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert2.Wav";
				DownSoundFile 								= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert3.Wav";	

				DrawMarker									= true;
				DrawType									= DrawSelection.Diamond;				
				Offset										= 3;
				MarkerUpColor								= Brushes.Green;
				MarkerDownColor								= Brushes.Red;				
				
				AlertsOn									= false;
				ReArmTime									= 30;	
				
				Email										= false;
				EmailTo										= @"";
				EmailBody									= @"";
								
				ColorBars									= false;		
				BarColorCrossAbove							= Brushes.Blue;
				BarColorCrossBelow							= Brushes.Orange;	
				
				ColorBarOutline								= false;				
				OutlineColorCrossAbove						= Brushes.Blue;
				OutlineColorCrossBelow						= Brushes.Orange;	
				
				PanelColorCrossAbove						= Brushes.LightGreen;
				PanelColorCrossBelow						= Brushes.Tomato;
				BackgroundOpacity							= 40;				
				
				MA0LineWidth								= 2;
				MA0PlotStyle								= PlotStyle.Line;
				MA0DashStyle								= DashStyleHelper.Solid;				
				Single0Color								= Brushes.CornflowerBlue;
				
				Color0RF									= false;				
				MA0RisingColor								= Brushes.Green;
				MA0FlatColor								= Brushes.Gold;
				MA0FallingColor								= Brushes.Red;	
				
				MA1LineWidth								= 2;
				MA1PlotStyle								= PlotStyle.Line;
				MA1DashStyle								= DashStyleHelper.Solid;								
				Single1Color								= Brushes.Magenta;
				
				Color1RF									= false;				
				MA1RisingColor								= Brushes.SkyBlue;
				MA1FlatColor								= Brushes.Goldenrod;
				MA1FallingColor								= Brushes.Tomato;
				
				ColorRegions								= false;
				ColorRegionAbove							= Brushes.Blue;
				ColorRegionBelow							= Brushes.Orange;
				RegionOpacity								= 20;
				ColorBackgroundAll							= true;
				
				DownColor  = new SolidColorBrush(Color.FromArgb(50, 25, 175,0));
				UpColor  = new SolidColorBrush(Color.FromArgb(50, 220, 20,0));
				
			}
			else if (State == State.Configure)
			{
				ma0Series = new Series<double>(this);
				ma1Series = new Series<double>(this);			
				
				Plots[0].Width 				= MA0LineWidth;
				Plots[0].PlotStyle			= MA0PlotStyle;
				Plots[0].DashStyleHelper	= MA0DashStyle;
				Plots[0].Brush				= Single0Color;
				
				Plots[1].Width 				= MA1LineWidth;
				Plots[1].PlotStyle			= MA1PlotStyle;
				Plots[1].DashStyleHelper	= MA1DashStyle;
				Plots[1].Brush 				= Single1Color;
				
			

				if (ColorBackground )
				{
					Brush temp = PanelColorCrossAbove.Clone();
					temp.Opacity = BackgroundOpacity / 100.0;
					temp.Freeze();
					PanelColorCrossAbove = temp;
					
					Brush temp1 = PanelColorCrossBelow.Clone();
					temp1.Opacity = BackgroundOpacity / 100.0;
					temp1.Freeze();
					PanelColorCrossBelow = temp1;	
					
					
				}			
				if (ColorBackgroundAll)
				{
					Brush temp3 = UpColor.Clone();
					temp3.Opacity = 70 / 100.0;
					temp3.Freeze();
					UpColor = temp3;
					
					Brush temp4 = DownColor.Clone();
					temp4.Opacity = 70 / 100.0;
					temp4.Freeze();
					DownColor = temp4;	
				}
			}
			else if (State == State.DataLoaded)
			{			
				#region MA assignments
				switch (Ma0Type)
				{
					case CDMAtype.DEMA:						
						if (fastPriceType == PriceType.Close)
							ma0Series = DEMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = DEMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = DEMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = DEMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = DEMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = DEMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = DEMA(Weighted, FastMAPeriod).Value;	
					break;
						
					case CDMAtype.EMA:
						if (fastPriceType == PriceType.Close)
							ma0Series = EMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = EMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = EMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = EMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = EMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = EMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = EMA(Weighted, FastMAPeriod).Value;	
					break;	
						
					case CDMAtype.HMA:
						if (fastPriceType == PriceType.Close)
							ma0Series = HMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = HMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = HMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = HMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = HMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = HMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = HMA(Weighted, FastMAPeriod).Value;	
					break;	
						
					case CDMAtype.LinReg:
						if (fastPriceType == PriceType.Close)
							ma0Series = LinReg(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = LinReg(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = LinReg(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = LinReg(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = LinReg(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = LinReg(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = LinReg(Weighted, FastMAPeriod).Value;	
					break;							
						
					case CDMAtype.SMA:
						if (fastPriceType == PriceType.Close)
							ma0Series = SMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = SMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = SMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = SMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = SMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = SMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = SMA(Weighted, FastMAPeriod).Value;	
					break;	
						
					case CDMAtype.TEMA:
						if (fastPriceType == PriceType.Close)
							ma0Series = TEMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = TEMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = TEMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = TEMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = TEMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = TEMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = TEMA(Weighted, FastMAPeriod).Value;	
					break;	
						
					case CDMAtype.TMA:	
						if (fastPriceType == PriceType.Close)
							ma0Series = TMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = TMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = TMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = TMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = TMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = TMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = TMA(Weighted, FastMAPeriod).Value;	
					break;	
					
					case CDMAtype.VWMA:
						if (fastPriceType == PriceType.Close)
							ma0Series = VWMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = VWMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = VWMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = VWMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = VWMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = VWMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = VWMA(Weighted, FastMAPeriod).Value;	
					break;	
						
					case CDMAtype.WMA:
						if (fastPriceType == PriceType.Close)
							ma0Series = WMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = WMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = WMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = WMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = WMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = WMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = WMA(Weighted, FastMAPeriod).Value;	
					break;
						
					case CDMAtype.ZLEMA:
						if (fastPriceType == PriceType.Close)
							ma0Series = ZLEMA(Close, FastMAPeriod).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = ZLEMA(High, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = ZLEMA(Low, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = ZLEMA(Median, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = ZLEMA(Open, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = ZLEMA(Typical, FastMAPeriod).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = ZLEMA(Weighted, FastMAPeriod).Value;	
					break;		
						
					case CDMAtype.Tillson:
						if (fastPriceType == PriceType.Close)
							ma0Series = TillsonT3(Close,FastMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.High)
							ma0Series = TillsonT3(High, FastMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Low)
							ma0Series = TillsonT3(Low, FastMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Median)
							ma0Series = TillsonT3(Median, FastMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Open)
							ma0Series = TillsonT3(Open, FastMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Typical)
							ma0Series = TillsonT3(Typical, FastMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Weighted)
							ma0Series = TillsonT3(Weighted, FastMAPeriod, 0.7).Value;	
					break;						
				}					
				
				switch (Ma1Type)
				{
					case CDMAtype.DEMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = DEMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = DEMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = DEMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = DEMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = DEMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = DEMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = DEMA(Weighted, SlowMAPeriod).Value;								
					break;
						
					case CDMAtype.EMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = EMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = EMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = EMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = EMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = EMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = EMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = EMA(Weighted, SlowMAPeriod).Value;							
					break;	
						
					case CDMAtype.HMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = HMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = HMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = HMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = HMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = HMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = HMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = HMA(Weighted, SlowMAPeriod).Value;								
					break;	
						
					case CDMAtype.LinReg:
						if (slowPriceType == PriceType.Close)
							ma1Series = LinReg(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = LinReg(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = LinReg(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = LinReg(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = LinReg(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = LinReg(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = LinReg(Weighted, SlowMAPeriod).Value;								
					break;							
						
					case CDMAtype.SMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = SMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = SMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = SMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = SMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = SMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = SMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = SMA(Weighted, SlowMAPeriod).Value;	
					break;	
						
					case CDMAtype.TEMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = TEMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = TEMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = TEMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = TEMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = TEMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = TEMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = TEMA(Weighted, SlowMAPeriod).Value;	
					break;	
						
					case CDMAtype.TMA:	
						if (slowPriceType == PriceType.Close)
							ma1Series = TMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = TMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = TMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = TMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = TMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = TMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = TMA(Weighted, SlowMAPeriod).Value;							
					break;	
					
					case CDMAtype.VWMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = VWMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = VWMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = VWMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = VWMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = VWMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = VWMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = VWMA(Weighted, SlowMAPeriod).Value;	
					break;	
						
					case CDMAtype.WMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = WMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = WMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = WMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = WMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = WMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = WMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = WMA(Weighted, SlowMAPeriod).Value;	
					break;
						
					case CDMAtype.ZLEMA:
						if (slowPriceType == PriceType.Close)
							ma1Series = ZLEMA(Close, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.High)
							ma1Series = ZLEMA(High, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Low)
							ma1Series = ZLEMA(Low, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Median)
							ma1Series = ZLEMA(Median, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Open)
							ma1Series = ZLEMA(Open, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Typical)
							ma1Series = ZLEMA(Typical, SlowMAPeriod).Value;
						if (slowPriceType == PriceType.Weighted)
							ma1Series = ZLEMA(Weighted, SlowMAPeriod).Value;	
					break;		
						
					case CDMAtype.Tillson:
						if (fastPriceType == PriceType.Close)
							ma1Series = TillsonT3(Close,SlowMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.High)
							ma1Series = TillsonT3(High, SlowMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Low)
							ma1Series = TillsonT3(Low, SlowMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Median)
							ma1Series = TillsonT3(Median, SlowMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Open)
							ma1Series = TillsonT3(Open, SlowMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Typical)
							ma1Series = TillsonT3(Typical, SlowMAPeriod, 0.7).Value;
						if (fastPriceType == PriceType.Weighted)
							ma1Series = TillsonT3(Weighted, SlowMAPeriod, 0.7).Value;	
					break;						
				}
				
				#endregion
			}
		}

		protected override void OnBarUpdate()
		{
			
			if (CurrentBar == 0 && Parent is Chart) // added check of Parent for Market analyzer error v1.05
			{
				if (ChartPanel.PanelIndex != 0)  // support for placing indicator into indicator panel V1.02
				{
					DrawOnPricePanel = false;  // move draw objects to indicator panel
				}
			}
			
			
			FstMA[0] 		= ma0Series[0];		// Plot the selected MA fast
			SlwMA[0] 		= ma1Series[0];		// Plot the selected MA slow
			CrossDetect[0] 	= 0;				// Reset the cross detection
			
			
			if (Color0RF)
			{
				PlotBrushes[0][0] = IsRising(FstMA) ? MA0RisingColor : IsFalling(FstMA) ? MA0FallingColor : MA0FlatColor;
			}
			
			if (Color1RF)
			{
				PlotBrushes[1][0] = IsRising(SlwMA) ? MA1RisingColor : IsFalling(SlwMA) ? MA1FallingColor : MA1FlatColor;
			}
			
			if (CrossAbove (FstMA, SlwMA, CrossLookBack) && CurrentBar != savedUBar && ShowCrossAbove)
			{
				savedUBar = CurrentBar;  		// once per bar only
				CrossDetect[0] =  1;
				DoActions();	
			}
			
			if (CrossBelow (FstMA, SlwMA, CrossLookBack) && CurrentBar != savedDBar && ShowCrossBelow)
			{
				savedDBar = CurrentBar;			// once per bar only
				CrossDetect[0] = -1;
				DoActions();		
			}
						
			if (ColorRegions)
			{
				if (CrossDetect[0] == 1 || (savedUBar > savedDBar))
				{
					Draw.Region(this, "Up" + savedUBar, CurrentBar - savedUBar + 1, 0, FstMA, SlwMA, Brushes.Transparent, ColorRegionAbove, RegionOpacity, 0);
				}
				
				if (CrossDetect[0] == -1 || (savedDBar > savedUBar))
				{
					Draw.Region(this, "Dwn" + savedDBar, CurrentBar - savedDBar + 1, 0, FstMA, SlwMA, Brushes.Transparent, ColorRegionBelow, RegionOpacity, 0);
				}							
			}		
			if (ColorBackgroundAll)
			{
				//BackBrushAll = CrossDetect[0] == 1 ? PanelColorCrossAbove :  PanelColorCrossBelow;
				BackBrush = FstMA[0] < SlwMA[0] ? UpColor : DownColor;
			}
				
		}
		
		
		private void DoActions()
		{			
			if (AlertsOn)
			{
					Alert("CrossDetect1", Priority.Low,(CrossDetect[0] == 1 ? "CrossAbove detected on " : "CrossBelow detected on ")
					+ Instrument.MasterInstrument.Name+ " "+BarsPeriod.Value+" "+BarsPeriod.BarsPeriodType, "", ReArmTime, 
					(CrossDetect[0] ==1 ? Brushes.LightGreen : Brushes.Tomato), Brushes.Black);
			}
					
			if (ColorBackground)
			{				
				
				BackBrush = CrossDetect[0] == 1 ? PanelColorCrossAbove :  PanelColorCrossBelow;
				
			}	
			
			if (ColorBars)
			{
				BarBrush = CrossDetect[0] ==1 ? BarColorCrossAbove : BarColorCrossBelow;
			}	
				
			if (ColorBarOutline)
			{
				CandleOutlineBrush = CrossDetect[0] == 1 ? OutlineColorCrossAbove : OutlineColorCrossBelow;
			}
						
			if (DrawMarker)
			{
				switch (DrawType)
				{
					case DrawSelection.Arrow:
					{
						if (CrossDetect[0] == 1)
						{
							Draw.ArrowUp (this, "UpArrow"+CurrentBar, true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize : FstMA[0]), MarkerUpColor);
						}
						else
						{
							Draw.ArrowDown (this, "DwnArrow"+CurrentBar, true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize : FstMA[0]), MarkerDownColor);
						}
						break;						
					}
						
					case DrawSelection.Triangle:
					{
						if (CrossDetect[0] == 1)
						{
							Draw.TriangleUp (this, "UpTriangle"+CurrentBar, true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize: FstMA[0]), MarkerUpColor);
						}
						else
						{
							Draw.TriangleDown (this, "DwnArrow"+CurrentBar, true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize: FstMA[0]), MarkerDownColor);
						}							
						break;
					}
						
					case DrawSelection.Dot:
					{
						if (CrossDetect[0] == 1)
						{
							Draw.Dot (this, "UpDot"+CurrentBar, true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize : FstMA[0]), MarkerUpColor);
						}
						else
						{
							Draw.Dot (this, "DwnDot"+CurrentBar, true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize: FstMA[0]), MarkerDownColor);
						}
						break;
					}
						
					case DrawSelection.Diamond:
					{
						if (CrossDetect[0] == 1)
						{
							Draw.Diamond (this, "UpDiamond"+CurrentBar, true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize : FstMA[0]), MarkerUpColor);
						}
						else
						{
							Draw.Diamond (this, "DwnDiamond"+CurrentBar, true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize : FstMA[0]) , MarkerDownColor);
						}
						break;
					}
				}
			}

			
			if (Email)
			{

				if (CrossDetect[0] == 1 && (savedUBar - CurrentBar == 0))
				{
					SendMail (EmailTo, "CrossAbove on "+Instrument.FullName+" "+BarsPeriod.Value+" "+BarsPeriod.BarsPeriodType, EmailBody);
				}
				else if (CrossDetect[0] == -1 && (savedDBar - CurrentBar == 0))
				{
					SendMail (EmailTo, "CrossBelow on "+Instrument.FullName+" "+BarsPeriod.Value+" "+BarsPeriod.BarsPeriodType, EmailBody);
				}
			}
			
			if (SoundsOn && (savedUBar - CurrentBar == 0))
			{
				PlaySound(UpSoundFile);
			}
			else if (SoundsOn && (savedDBar - CurrentBar == 0))
			{
				PlaySound(DownSoundFile);
			}
		}
				

		#region Properties
		
		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> FstMA
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SlwMA
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CrossDetect
		{
			get { return Values[2]; }
		}
		
		#endregion
		
		#region FastMA

// Fast MA ************************

		[NinjaScriptProperty]
		[Display(Name=" Crossing MA Type", Description="Select Fast Moving Average Type", Order=1, GroupName="Crossing Moving Average (fast)")]
		public CDMAtype Ma0Type
        { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name=" Crossing MA PriceType", Description="Select price Type (Close, high, Low, etc.)", Order=2, GroupName="Crossing Moving Average (fast)")]
		public PriceType fastPriceType
        { get; set; }		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name=" Crossing MA Period", Description="Fast MA period", Order=3, GroupName="Crossing Moving Average (fast)")]
		public int FastMAPeriod
		{ get; set; }

		[Range(1, 19)]
		[Display(Name=" MA Plot Line width", Description="Set thickness of plot line", Order=4, GroupName="Crossing Moving Average (fast)")]
		public int MA0LineWidth
		{ get; set; }			
		
		[Display(Name=" MA Line PlotStyle", Description="Set type of line/cross/bar/square/etc", Order=5, GroupName="Crossing Moving Average (fast)")]
		public PlotStyle MA0PlotStyle
		{ get; set; }			
		
		[Display(Name=" MA Line DashStyle", Description="Set dash style of line", Order=6, GroupName="Crossing Moving Average (fast)")]
		public DashStyleHelper MA0DashStyle
		{ get; set; }	
		
		[Display(Name="Color Rising/falling ", Description="Set true to enable using plot rising/flat/falling colors\nSet false for single color plot", Order=7, GroupName="Crossing Moving Average (fast)")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Color0RF
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Plot Color", Description="Color of plot line", Order=8, GroupName="Crossing Moving Average (fast)")]
		public Brush Single0Color
		{ get; set; }		

		[Browsable(false)]
		public string Single0ColorSerializable
		{
			get { return Serialize.BrushToString(Single0Color); }
			set { Single0Color = Serialize.StringToBrush(value); }
		}			
		
		[XmlIgnore]
		[Display(Name=" Fast MA Rising Color", Description="Color of MA when rising", Order=10, GroupName="Crossing Moving Average (fast)")]
		public Brush MA0RisingColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA0RisingColorSerializable
		{
			get { return Serialize.BrushToString(MA0RisingColor); }
			set { MA0RisingColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name=" Fast MA Flat Color", Description="Color of MA when flat", Order=11, GroupName="Crossing Moving Average (fast)")]
		public Brush MA0FlatColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA0FlatColorSerializable
		{
			get { return Serialize.BrushToString(MA0FlatColor); }
			set { MA0FlatColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name=" Fast MA Falling Color", Description="Color of MA when falling", Order=12, GroupName="Crossing Moving Average (fast)")]
		public Brush MA0FallingColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA0FallingColorSerializable
		{
			get { return Serialize.BrushToString(MA0FallingColor); }
			set { MA0FallingColor = Serialize.StringToBrush(value); }
		}	
		
		#endregion
		
		#region SlowMA
		
// Slow MA *********************************************		
		
		[NinjaScriptProperty]
		[Display(Name="Crossed MA Type", Description="Select Slow Moving Average Type", Order=1, GroupName="Crossed Moving Average (slow)")]
		public CDMAtype Ma1Type
        { get; set; }	

		[NinjaScriptProperty]
		[Display(Name=" Crossed MA PriceType", Description="Select price Type (Close, high, Low, etc.)", Order=2, GroupName="Crossed Moving Average (slow)")]
		public PriceType slowPriceType
        { get; set; }			

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Crossed (slower) MA Period", Description="Slower MA period", Order=3, GroupName="Crossed Moving Average (slow)")]
		public int SlowMAPeriod
		{ get; set; }		
		
		[Range(1, 19)]
		[Display(Name="MA Plot Line width", Description="Set thickness of plot line", Order=4, GroupName="Crossed Moving Average (slow)")]
		public int MA1LineWidth
		{ get; set; }			
		
		[Display(Name="MA Line PlotStyle", Description="Set type of line/cross/bar/square/etc", Order=5, GroupName="Crossed Moving Average (slow)")]
		public PlotStyle MA1PlotStyle
		{ get; set; }			
		
		[Display(Name="MA Line DashStyle", Description="Set dash style of line", Order=6, GroupName="Crossed Moving Average (slow)")]
		public DashStyleHelper MA1DashStyle
		{ get; set; }
		
		[Display(Name="Color Rising/falling ", Description="Set true to enable using plot rising/flat/falling colors\nSet false for single color plot", Order=7, GroupName="Crossed Moving Average (slow)")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Color1RF
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Plot Color", Description="Color of plot line", Order=8, GroupName="Crossed Moving Average (slow)")]
		public Brush Single1Color
		{ get; set; }
		
		[Browsable(false)]
		public string Single1ColorSerializable
		{
			get { return Serialize.BrushToString(Single1Color); }
			set { Single1Color = Serialize.StringToBrush(value); }
		}			
		
		[XmlIgnore]
		[Display(Name="Slow MA Rising Color", Description="Color of MA when rising", Order=10, GroupName="Crossed Moving Average (slow)")]
		public Brush MA1RisingColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA1RisingColorSerializable
		{
			get { return Serialize.BrushToString(MA1RisingColor); }
			set { MA1RisingColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Slow MA Flat Color", Description="Color of MA when flat", Order=11, GroupName="Crossed Moving Average (slow)")]
		public Brush MA1FlatColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA1FlatColorSerializable
		{
			get { return Serialize.BrushToString(MA1FlatColor); }
			set { MA1FlatColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Slow MA Falling Color", Description="Color of MA when falling", Order=12, GroupName="Crossed Moving Average (slow)")]
		public Brush MA1FallingColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA1FallingColorSerializable
		{
			get { return Serialize.BrushToString(MA1FallingColor); }
			set { MA1FallingColor = Serialize.StringToBrush(value); }
		}	
		
		#endregion
		
		#region Detection Settings
		
		[Range(1, 100)]
		[NinjaScriptProperty]
		[Display(Name="Cross Look Back period", Description="Integer number of bars to look back for cross, 1 is typical", Order=1, GroupName="Cross Detection")]
		public int CrossLookBack
		{ get; set; }	
				
		[NinjaScriptProperty]
		[Display(Name="Identify cross above", Description="Show on Cross above conditions", Order=2, GroupName="Cross Detection")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ShowCrossAbove
		{ get; set; }	
		
		[NinjaScriptProperty]
		[Display(Name="Identify cross below", Description="Show on Cross below conditions", Order=3, GroupName="Cross Detection")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ShowCrossBelow
		{ get; set; }				
		
		#endregion
		
		#region Actions
		
		[Display(Name="Sounds On Cross", Description="Play sounds on cross conditions", Order=1, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool SoundsOn
		{ get; set; }			
		
		[Display(Name="CrossAbove sound", Description="Enter Up sound file path/name", Order=2, GroupName="Cross Detection Actions")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		[RefreshProperties(RefreshProperties.All)]
		public string UpSoundFile
		{ get; set; }

		[Display(Name="CrossBelow sound", Description="Enter Down sound file path/name", Order=3, GroupName="Cross Detection Actions")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		[RefreshProperties(RefreshProperties.All)]
		public string DownSoundFile
		{ get; set; }
	
		[Display(Name="Alerts to Alert Panel", Description="Send alert messages to alerts panel", Order=10, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool AlertsOn
		{ get; set; }	
		
		[Range(1, 1000)]
		[Display(Name="Alert re-arm time in seconds", Description="Rearm time in seconds, if alarm condition remains on it will resend alert", Order=11, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public int ReArmTime
		{ get; set; }
		
		[Display(Name="Send Email", Description="Send Email when cross detected (must have share service pre set)", Order=16, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Email
		{ get; set; }	
		
		[Display(Name="Email To:", Description="Destination complete e-mail address ", Order=17, GroupName="Cross Detection Actions")]
		public string EmailTo
		{ get; set; }
		
		[Display(Name="Email Body:", Description="Te=xt to display in the body of the e-mail", Order=19, GroupName="Cross Detection Actions")]
		public string EmailBody
		{ get; set; }	
		
		
		[Display(Name="Draw at Cross", Description="Draw a symbol to mark cross location", Order=20, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool DrawMarker
		{ get; set; }			
				
		[Display(Name="Marker to use", Description="Choose a marker to show at Cross", Order=21, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public DrawSelection DrawType
		{ get; set; }		
		
		[Display(Name="Marker offset (Ticks)", Description="Ticks above or beow price high or low to display selected marker", Order=22, GroupName="Cross Detection Actions")]
		public int Offset
		{ get; set; }			

		
		[XmlIgnore]
		[Display(Name="CrossAbove Marker color", Description="Color of marker to show croass above", Order=23, GroupName="Cross Detection Actions")]
		public Brush MarkerUpColor
		{ get; set; }
		
		[Browsable(false)]
		public string MarkerUpColorSerializable
		{
			get { return Serialize.BrushToString(MarkerUpColor); }
			set { MarkerUpColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="CrossBelow Marker color", Description="Color of marker to show cross below", Order=24, GroupName="Cross Detection Actions")]
		public Brush MarkerDownColor
		{ get; set; }
		
		[Browsable(false)]
		public string MarkerDowColorSerializable
		{
			get { return Serialize.BrushToString(MarkerDownColor); }
			set { MarkerDownColor = Serialize.StringToBrush(value); }
		}
		
		[Display(Name="Color Price Panel Background", Description="Color Price panel background when Crossabove/below", Order=30, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBackground
		{ get; set; }		
		
		
		[Display(Name="Extend Background to all Panels", Description="Extend background coloring all Panel on chart", Order=34, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBackgroundAll
		{ get; set; }	
		

		[Range(1, 99)]
		[Display(Name=" % Opacity of background", Description="Sets the amount of opacity of background colors ", Order=31, GroupName="Cross Detection Actions")]
		public int BackgroundOpacity
		{ get; set; }			

		[XmlIgnore]
		[Display(Name="Panel Color for Cross above", Description="Panel background color when crossing above", Order=32, GroupName="Cross Detection Actions")]
		public Brush PanelColorCrossAbove
		{ get; set; }

		[Browsable(false)]
		public string PanelColorCrossAboveSerializable
		{
			get { return Serialize.BrushToString(PanelColorCrossAbove); }
			set { PanelColorCrossAbove = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Panel Color for Cross below", Description="Panel background coloe when croissing below", Order=33, GroupName="Cross Detection Actions")]
		public Brush PanelColorCrossBelow
		{ get; set; }

		[Browsable(false)]
		public string PanelColorCrossBelowSerializable
		{
			get { return Serialize.BrushToString(PanelColorCrossBelow); }
			set { PanelColorCrossBelow = Serialize.StringToBrush(value); }
		}			

		
		
		[Display(Name="Color Price Bar on cross", Description="Color the bar where cross occured", Order=40, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBars
		{ get; set; }			
		
		[XmlIgnore]
		[Display(Name="Bar Color for Cross above", Description="Price bar color when crossing above", Order=41, GroupName="Cross Detection Actions")]
		public Brush BarColorCrossAbove
		{ get; set; }

		[Browsable(false)]
		public string BarColorCrossAboveSerializable
		{
			get { return Serialize.BrushToString(BarColorCrossAbove); }
			set { BarColorCrossAbove = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Bar Color for Cross below", Description="Price bar color when crossing below", Order=42, GroupName="Cross Detection Actions")]
		public Brush BarColorCrossBelow
		{ get; set; }

		[Browsable(false)]
		public string BarColorCrossBelowSerializable
		{
			get { return Serialize.BrushToString(BarColorCrossBelow); }
			set { BarColorCrossBelow = Serialize.StringToBrush(value); }
		}	
		
		[Display(Name="Color Price Bar Outline on cross", Description="Color the bar outline where cross occured", Order=50, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBarOutline
		{ get; set; }			
		
		[XmlIgnore]
		[Display(Name="Bar outline color for Cross above", Description="Price bar outline color when crossing above", Order=51, GroupName="Cross Detection Actions")]
		public Brush OutlineColorCrossAbove
		{ get; set; }

		[Browsable(false)]
		public string OutlineColorCrossAboveSerializable
		{
			get { return Serialize.BrushToString(OutlineColorCrossAbove); }
			set { OutlineColorCrossAbove = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Bar outline color for Cross below", Description="Price bar outline color when croissing below", Order=52, GroupName="Cross Detection Actions")]
		public Brush OutlineColorCrossBelow
		{ get; set; }

		[Browsable(false)]
		public string OutlineColorCrossBelowSerializable
		{
			get { return Serialize.BrushToString(OutlineColorCrossBelow); }
			set { OutlineColorCrossBelow = Serialize.StringToBrush(value); }
		}	
		
		[Display(Name="Color region between MAs", Description="Color area between the moving averages", Order=60, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorRegions
		{ get; set; }			
		
		[XmlIgnore]
		[Display(Name="Region color for fast above slow", Description="Color of area when fast ma above slow ma", Order=61, GroupName="Cross Detection Actions")]
		public Brush ColorRegionAbove
		{ get; set; }

		[Browsable(false)]
		public string ColorRegionAboveSerializable
		{
			get { return Serialize.BrushToString(ColorRegionAbove); }
			set { ColorRegionAbove = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Region color for fast below slow ", Description="Color of area when fast ma below slow ma", Order=62, GroupName="Cross Detection Actions")]
		public Brush ColorRegionBelow
		{ get; set; }

		[Browsable(false)]
		public string ColorRegionBelowSerializable
		{
			get { return Serialize.BrushToString(ColorRegionBelow); }
			set { ColorRegionBelow = Serialize.StringToBrush(value); }
		}

		[Range(1, 99)]
		[Display(Name=" % Opacity of region", Description="Sets the amount of opacity of region colors ", Order=63, GroupName="Cross Detection Actions")]
		public int RegionOpacity
		{ get; set; }			
		
		
		#endregion
		
		#endregion
			
	}
	#region ConverterStuff
	
	public class MACrossBuilderConverter : IndicatorBaseConverter 
	{
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			MACrossBuilder indicator = component as MACrossBuilder;

			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
				? base.GetProperties(context, component, attrs) : TypeDescriptor.GetProperties(component, attrs);

			if (indicator == null || propertyDescriptorCollection == null)
				return propertyDescriptorCollection;
			
			PropertyDescriptor Single0Color 		= propertyDescriptorCollection["Single0Color"];
			PropertyDescriptor MA0RisingColor 		= propertyDescriptorCollection["MA0RisingColor"];
			PropertyDescriptor MA0FlatColor 		= propertyDescriptorCollection["MA0FlatColor"];
			PropertyDescriptor MA0FallingColor 		= propertyDescriptorCollection["MA0FallingColor"];
			
			PropertyDescriptor Single1Color 		= propertyDescriptorCollection["Single1Color"];
			PropertyDescriptor MA1RisingColor 		= propertyDescriptorCollection["MA1RisingColor"];
			PropertyDescriptor MA1FlatColor 		= propertyDescriptorCollection["MA1FlatColor"];
			PropertyDescriptor MA1FallingColor 		= propertyDescriptorCollection["MA1FallingColor"];			
			
			PropertyDescriptor DrawMarker			= propertyDescriptorCollection["DrawMarker"];			
			PropertyDescriptor Offset				= propertyDescriptorCollection["Offset"];
			PropertyDescriptor MarkerUpColor		= propertyDescriptorCollection["MarkerUpColor"];
			PropertyDescriptor MarkerDownColor		= propertyDescriptorCollection["MarkerDownColor"];
			PropertyDescriptor DrawType				= propertyDescriptorCollection["DrawType"];			
			
			PropertyDescriptor ColorBars			= propertyDescriptorCollection["ColorBars"];			
			PropertyDescriptor BarColorCrossAbove	= propertyDescriptorCollection["BarColorCrossAbove"];
			PropertyDescriptor BarColorCrossBelow	= propertyDescriptorCollection["BarColorCrossBelow"];			

			PropertyDescriptor ColorBarOutline		= propertyDescriptorCollection["ColorBarOutline"];			
			PropertyDescriptor OutlineColorCrossAbove=propertyDescriptorCollection["OutlineColorCrossAbove"];
			PropertyDescriptor OutlineColorCrossBelow=propertyDescriptorCollection["OutlineColorCrossBelow"];			

			PropertyDescriptor ColorBackground		= propertyDescriptorCollection["ColorBackground"];			
			PropertyDescriptor PanelColorCrossAbove	= propertyDescriptorCollection["PanelColorCrossAbove"];
			PropertyDescriptor PanelColorCrossBelow	= propertyDescriptorCollection["PanelColorCrossBelow"];
			PropertyDescriptor ColorBackgroundAll	= propertyDescriptorCollection["ColorBackgroundAll"];
			PropertyDescriptor BackgroundOpacity	= propertyDescriptorCollection["BackgroundOpacity"];				
			
			PropertyDescriptor SoundsOn				= propertyDescriptorCollection["SoundsOn"];			
			PropertyDescriptor UpSoundFile			= propertyDescriptorCollection["UpSoundFile"];
			PropertyDescriptor DownSoundFile		= propertyDescriptorCollection["DownSoundFile"];				

			PropertyDescriptor AlertsOn				= propertyDescriptorCollection["AlertsOn"];			
			PropertyDescriptor ReArmTime			= propertyDescriptorCollection["ReArmTime"];

			PropertyDescriptor Email				= propertyDescriptorCollection["Email"];			
			PropertyDescriptor EmailTo				= propertyDescriptorCollection["EmailTo"];
			PropertyDescriptor EmailBody			= propertyDescriptorCollection["EmailBody"];	
			
			PropertyDescriptor ColorRegion			= propertyDescriptorCollection["ColorRegion"];			
			PropertyDescriptor ColorRegionAbove		= propertyDescriptorCollection["ColorRegionAbove"];
			PropertyDescriptor ColorRegionBelow		= propertyDescriptorCollection["ColorRegionBelow"];
			PropertyDescriptor RegionOpacity		= propertyDescriptorCollection["RegionOpacity"];			
			

			// remove removable properties first
			propertyDescriptorCollection.Remove(MA0RisingColor);
			propertyDescriptorCollection.Remove(MA0FlatColor);
			propertyDescriptorCollection.Remove(MA0FallingColor);				
			

			propertyDescriptorCollection.Remove(MA1RisingColor);
			propertyDescriptorCollection.Remove(MA1FlatColor);
			propertyDescriptorCollection.Remove(MA1FallingColor);	
			
			propertyDescriptorCollection.Remove(UpSoundFile);
			propertyDescriptorCollection.Remove(DownSoundFile);		
			
			propertyDescriptorCollection.Remove(ReArmTime);				
			
			propertyDescriptorCollection.Remove(EmailTo);	
			propertyDescriptorCollection.Remove(EmailBody);					
			
			propertyDescriptorCollection.Remove(DrawType);
			propertyDescriptorCollection.Remove(Offset);
			propertyDescriptorCollection.Remove(MarkerUpColor);
			propertyDescriptorCollection.Remove(MarkerDownColor);
			
			propertyDescriptorCollection.Remove(PanelColorCrossAbove);
			propertyDescriptorCollection.Remove(PanelColorCrossBelow);
			//propertyDescriptorCollection.Remove(ColorBackgroundAll);
			propertyDescriptorCollection.Remove(BackgroundOpacity);	
			
			propertyDescriptorCollection.Remove(BarColorCrossAbove);
			propertyDescriptorCollection.Remove(BarColorCrossBelow);			
			
			propertyDescriptorCollection.Remove(OutlineColorCrossAbove);
			propertyDescriptorCollection.Remove(OutlineColorCrossBelow);	
			
			propertyDescriptorCollection.Remove(ColorRegionAbove);
			propertyDescriptorCollection.Remove(ColorRegionBelow);
			propertyDescriptorCollection.Remove(RegionOpacity);					
			
			
			// Add backj in if...
			
			if (indicator.Color0RF)
			{
				propertyDescriptorCollection.Remove(Single0Color);
				propertyDescriptorCollection.Add(MA0RisingColor);
				propertyDescriptorCollection.Add(MA0FlatColor);
				propertyDescriptorCollection.Add(MA0FallingColor);				
			}

			if (indicator.Color1RF)
			{
				propertyDescriptorCollection.Remove(Single1Color);
				propertyDescriptorCollection.Add(MA1RisingColor);
				propertyDescriptorCollection.Add(MA1FlatColor);
				propertyDescriptorCollection.Add(MA1FallingColor);				
			}
			
			if (indicator.SoundsOn)
			{
				propertyDescriptorCollection.Add(UpSoundFile);
				propertyDescriptorCollection.Add(DownSoundFile);								
			}

			if (indicator.AlertsOn)
			{
				propertyDescriptorCollection.Add(ReArmTime);							
			}
	
			if (indicator.Email)
			{
				propertyDescriptorCollection.Add(EmailTo);	
				propertyDescriptorCollection.Add(EmailBody);					
			}
						
			if (indicator.DrawMarker)
			{
				propertyDescriptorCollection.Add(DrawType);
				propertyDescriptorCollection.Add(Offset);
				propertyDescriptorCollection.Add(MarkerUpColor);
				propertyDescriptorCollection.Add(MarkerDownColor);				
			}

			if (indicator.ColorBackground)
			{
				propertyDescriptorCollection.Add(PanelColorCrossAbove);
				propertyDescriptorCollection.Add(PanelColorCrossBelow);
				//propertyDescriptorCollection.Add(ColorBackgroundAll);
				propertyDescriptorCollection.Add(BackgroundOpacity);				
			}
		
			if (indicator.ColorBars)
			{
				propertyDescriptorCollection.Add(BarColorCrossAbove);
				propertyDescriptorCollection.Add(BarColorCrossBelow);								
			}
			
			if (indicator.ColorBarOutline)
			{
				propertyDescriptorCollection.Add(OutlineColorCrossAbove);
				propertyDescriptorCollection.Add(OutlineColorCrossBelow);								
			}
			
			if (indicator.ColorRegions)
			{
				propertyDescriptorCollection.Add(ColorRegionAbove);
				propertyDescriptorCollection.Add(ColorRegionBelow);
				propertyDescriptorCollection.Add(RegionOpacity);				
			}
		
			return propertyDescriptorCollection;
		}
		
		// Important:  This must return true otherwise the type convetor will not be called
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{ return true; }
	}		
	#endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACrossBuilder[] cacheMACrossBuilder;
		public MACrossBuilder MACrossBuilder(CDMAtype ma0Type, PriceType fastPriceType, int fastMAPeriod, CDMAtype ma1Type, PriceType slowPriceType, int slowMAPeriod, int crossLookBack, bool showCrossAbove, bool showCrossBelow)
		{
			return MACrossBuilder(Input, ma0Type, fastPriceType, fastMAPeriod, ma1Type, slowPriceType, slowMAPeriod, crossLookBack, showCrossAbove, showCrossBelow);
		}

		public MACrossBuilder MACrossBuilder(ISeries<double> input, CDMAtype ma0Type, PriceType fastPriceType, int fastMAPeriod, CDMAtype ma1Type, PriceType slowPriceType, int slowMAPeriod, int crossLookBack, bool showCrossAbove, bool showCrossBelow)
		{
			if (cacheMACrossBuilder != null)
				for (int idx = 0; idx < cacheMACrossBuilder.Length; idx++)
					if (cacheMACrossBuilder[idx] != null && cacheMACrossBuilder[idx].Ma0Type == ma0Type && cacheMACrossBuilder[idx].fastPriceType == fastPriceType && cacheMACrossBuilder[idx].FastMAPeriod == fastMAPeriod && cacheMACrossBuilder[idx].Ma1Type == ma1Type && cacheMACrossBuilder[idx].slowPriceType == slowPriceType && cacheMACrossBuilder[idx].SlowMAPeriod == slowMAPeriod && cacheMACrossBuilder[idx].CrossLookBack == crossLookBack && cacheMACrossBuilder[idx].ShowCrossAbove == showCrossAbove && cacheMACrossBuilder[idx].ShowCrossBelow == showCrossBelow && cacheMACrossBuilder[idx].EqualsInput(input))
						return cacheMACrossBuilder[idx];
			return CacheIndicator<MACrossBuilder>(new MACrossBuilder(){ Ma0Type = ma0Type, fastPriceType = fastPriceType, FastMAPeriod = fastMAPeriod, Ma1Type = ma1Type, slowPriceType = slowPriceType, SlowMAPeriod = slowMAPeriod, CrossLookBack = crossLookBack, ShowCrossAbove = showCrossAbove, ShowCrossBelow = showCrossBelow }, input, ref cacheMACrossBuilder);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACrossBuilder MACrossBuilder(CDMAtype ma0Type, PriceType fastPriceType, int fastMAPeriod, CDMAtype ma1Type, PriceType slowPriceType, int slowMAPeriod, int crossLookBack, bool showCrossAbove, bool showCrossBelow)
		{
			return indicator.MACrossBuilder(Input, ma0Type, fastPriceType, fastMAPeriod, ma1Type, slowPriceType, slowMAPeriod, crossLookBack, showCrossAbove, showCrossBelow);
		}

		public Indicators.MACrossBuilder MACrossBuilder(ISeries<double> input , CDMAtype ma0Type, PriceType fastPriceType, int fastMAPeriod, CDMAtype ma1Type, PriceType slowPriceType, int slowMAPeriod, int crossLookBack, bool showCrossAbove, bool showCrossBelow)
		{
			return indicator.MACrossBuilder(input, ma0Type, fastPriceType, fastMAPeriod, ma1Type, slowPriceType, slowMAPeriod, crossLookBack, showCrossAbove, showCrossBelow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACrossBuilder MACrossBuilder(CDMAtype ma0Type, PriceType fastPriceType, int fastMAPeriod, CDMAtype ma1Type, PriceType slowPriceType, int slowMAPeriod, int crossLookBack, bool showCrossAbove, bool showCrossBelow)
		{
			return indicator.MACrossBuilder(Input, ma0Type, fastPriceType, fastMAPeriod, ma1Type, slowPriceType, slowMAPeriod, crossLookBack, showCrossAbove, showCrossBelow);
		}

		public Indicators.MACrossBuilder MACrossBuilder(ISeries<double> input , CDMAtype ma0Type, PriceType fastPriceType, int fastMAPeriod, CDMAtype ma1Type, PriceType slowPriceType, int slowMAPeriod, int crossLookBack, bool showCrossAbove, bool showCrossBelow)
		{
			return indicator.MACrossBuilder(input, ma0Type, fastPriceType, fastMAPeriod, ma1Type, slowPriceType, slowMAPeriod, crossLookBack, showCrossAbove, showCrossBelow);
		}
	}
}

#endregion
