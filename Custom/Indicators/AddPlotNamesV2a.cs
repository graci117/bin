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
using SharpDX.DirectWrite;
using NinjaTrader.Core;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AddPlotNamesV2a : Indicator
	{		
		private List<NinjaTrader.Gui.NinjaScript.IndicatorRenderBase> indicatorCollection;
		private int	areaOpacity;
		bool myshowTicks = false;
		private Brush HistoricalBrush = Brushes.Gray;
		private bool HistoricalView = false;
		private double barClose;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Will cycle through indicators on panel and add their plot names on right side of chart";
				Name										= "AddPlotNamesV2a";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				TextFontta 									= new Gui.Tools.SimpleFont("Arial", 11);
				textLength 									= 80;
				RsMarginColor								= System.Windows.Media.Brushes.DodgerBlue;
				ColorOpacity								= 10;
				LEoffset									= 4;
				RSmargin									= 100;
				showTicks									= false;
				showLabels									= true;
			}
			else if (State == State.Historical)
			{
				// create brushes once
				
				Brush temp1 = RsMarginColor.Clone();   // configure background brush just once
				temp1.Opacity = ColorOpacity / 100.0;
				temp1.Freeze();
				RsMarginColor = temp1;
				
				Brush temp2 = HistoricalBrush.Clone();
				temp2.Opacity = ColorOpacity / 100.0;
				temp2.Freeze();
				HistoricalBrush = temp2;
				
				SetZOrder(-1); // default here is go below the bars and called in State.Historical
			}
			else if (State == State.Terminated)
			{
				indicatorCollection = null; 	
			}
		}

		protected override void OnBarUpdate()
		{
			barClose = Close[0];		 // get the current price
			indicatorCollection = null; 
			if (ChartControl != null && ChartControl.Indicators != null)
				lock (ChartControl.Indicators)
					indicatorCollection = ChartControl.Indicators.ToList();	
		}
		
		public override void OnRenderTargetChanged()
		{
			indicatorCollection = null; 
			if (ChartControl != null && ChartControl.Indicators != null)
				lock (ChartControl.Indicators)
					indicatorCollection = ChartControl.Indicators.ToList();
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale); 
			
			if(IsInHitTest) return;
			
			int myTicks = 0;
			
			bool hideTicks = true;
			
			// all of this for right side shading...
			ChartControlProperties myProperties = chartControl.Properties;			
			myProperties.BarMarginRight = RSmargin;    // write the new right side margin property
			SharpDX.Vector2 startPoint1;
			SharpDX.Vector2 endPoint;
			SharpDX.Direct2D1.Brush RsMarginColordx;	
			SharpDX.Direct2D1.Brush HistoricalBrushdx;
			
			RsMarginColordx = RsMarginColor.ToDxBrush(RenderTarget);
			HistoricalBrushdx = HistoricalBrush.ToDxBrush(RenderTarget);
 
			startPoint1 = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - RSmargin , ChartPanel.Y);
			endPoint = new SharpDX.Vector2(ChartPanel.W, ChartPanel.Y + ChartPanel.H);
			float width = endPoint.X - startPoint1.X;
			float height = endPoint.Y - startPoint1.Y;			
			
			// draw the rectangle for background shading
			if (chartControl.IsScrollArrowVisible && ChartBars.ToIndex != ChartBars.Count -1)
			{
				SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint1.X, startPoint1.Y, width, height);
				RenderTarget.FillRectangle(rect, HistoricalBrushdx);
			}
			else
			{
				SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint1.X, startPoint1.Y, width, height);
				RenderTarget.FillRectangle(rect, RsMarginColordx);	
			}
			
			RsMarginColordx.Dispose();
			HistoricalBrushdx.Dispose();
			// end rightside shading
			
			// all of this for adding plot names
			try
			{			
				if(indicatorCollection == null) return; 
					
					TextFormat	textFormat	= TextFontta.ToDirectWriteTextFormat();
					
  					foreach (NinjaTrader.Gui.NinjaScript.IndicatorRenderBase indicator in indicatorCollection)			
  					{				
						if (indicator.Panel == this.Panel || indicator.Panel == 0)  // process each indicator in this panel
						{
							if (indicator.Name != "AddPlotNamesV2a")  				// don't need to label this indicator...
							{							
								if (indicator.State == State.Realtime)
								{
									for (int seriesCount = 0; seriesCount <  indicator.Values.Length ; seriesCount++)  // process each plot of the indicator
									{
										double	y					= -1;
										double	startX				= -1;
										int		lastBarPainted		= ChartBars.ToIndex - (indicator.Calculate == Calculate.OnBarClose ? 1:0); 
										Plot	plot				= indicator.Plots[seriesCount];
										startX						= ChartPanel.W - RSmargin +LEoffset;
										double val					= indicator.Values[seriesCount].GetValueAt(lastBarPainted);

										if (showTicks) 
										{
											if (ChartBars.ToIndex == ChartBars.Count -1) // live data
											{
												myTicks = Convert.ToInt32((val - barClose) / TickSize);  // Calculate using live value of close
												hideTicks = false;
											}
											else
											{
												myTicks = Convert.ToInt32((val - Bars.GetClose(lastBarPainted+1)) / TickSize);  // calculate using last historical bar shown
												hideTicks = true;  					// set bool to show "H"
											}
											
										}
										
										y							= chartScale.GetYByValue(val) - (textFormat.FontSize / 2)-1;  // try to center text on plot's last location	
										Point startPoint			= new Point(startX, y);	
										TextLayout textLayout 		= new TextLayout(Globals.DirectWriteFactory, (showLabels ? plot.Name+" " : "")+(showTicks ? (hideTicks ? "H["+myTicks+"]" : "["+myTicks+"]"):""), textFormat, textFormat.FontSize+textLength, textFormat.FontSize);
										RenderTarget.DrawTextLayout(startPoint.ToVector2(), textLayout, plot.BrushDX);
										textLayout.Dispose();
									} 
								}
							}
						}	
  					}// foreach
					textFormat.Dispose();
				} // try
				catch (Exception e)
				{
					if (State >= State.Terminated)
						return;
					Log("AddPlotNamesV2a error: ", NinjaTrader.Cbi.LogLevel.Warning);
					Print (Time[0]+ " "+ e.ToString()); // send exception to the output 
				}

		} //override end
		
		#region Properties
		
		[Display(Name	= "Plot Label Font",
		Description		= "select font, style, size to display on chart",
		GroupName		= "Display",
		Order			= 1)]
		public Gui.Tools.SimpleFont TextFontta
		{ get; set; }	
		
		[Display(Name="Display plot labels", Order=2, GroupName="Display")]
		public bool showLabels
		{ get; set; }		
		
		[Display(Name	= "Text length",
		Description		= "shorten to create multi line, lenthen to put on one line",
		GroupName		= "Display",
		Order			= 3)]
		public int textLength
		{ get; set; }

		[Range(40, 200)]
		[Display(Name	= "Right Side Margin",
		Description		= "Charts right side magin value",
		GroupName		= "Display",
		Order			= 4)]
		public int RSmargin
		{ get; set; }			
		
		
		[Range(1, 100)]
		[Display(Name	= "Left edge offset",
		Description		= "Pixels to space from left edge to label",
		GroupName		= "Display",
		Order			= 5)]
		public int LEoffset
		{ get; set; }			
		
		
		[XmlIgnore]
		[Display(Name="Text margin color", Order=6, GroupName="Display")]
		public Brush RsMarginColor
		{ get; set; }

		[Browsable(false)]
		public string RsMarginColorSerializable
		{
			get { return Serialize.BrushToString(RsMarginColor); }
			set { RsMarginColor = Serialize.StringToBrush(value); }
		}			

		[Range(1, 100)]
		[Display(Name="Margin color opacity", Order=7, GroupName="Display")]
		public int ColorOpacity
		{ get; set; }		
		
		[Display(Name="Display ticks away from current price", Order=8, GroupName="Display")]
		public bool showTicks
		{ get; set; }
		
		#endregion		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AddPlotNamesV2a[] cacheAddPlotNamesV2a;
		public AddPlotNamesV2a AddPlotNamesV2a()
		{
			return AddPlotNamesV2a(Input);
		}

		public AddPlotNamesV2a AddPlotNamesV2a(ISeries<double> input)
		{
			if (cacheAddPlotNamesV2a != null)
				for (int idx = 0; idx < cacheAddPlotNamesV2a.Length; idx++)
					if (cacheAddPlotNamesV2a[idx] != null &&  cacheAddPlotNamesV2a[idx].EqualsInput(input))
						return cacheAddPlotNamesV2a[idx];
			return CacheIndicator<AddPlotNamesV2a>(new AddPlotNamesV2a(), input, ref cacheAddPlotNamesV2a);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AddPlotNamesV2a AddPlotNamesV2a()
		{
			return indicator.AddPlotNamesV2a(Input);
		}

		public Indicators.AddPlotNamesV2a AddPlotNamesV2a(ISeries<double> input )
		{
			return indicator.AddPlotNamesV2a(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AddPlotNamesV2a AddPlotNamesV2a()
		{
			return indicator.AddPlotNamesV2a(Input);
		}

		public Indicators.AddPlotNamesV2a AddPlotNamesV2a(ISeries<double> input )
		{
			return indicator.AddPlotNamesV2a(input);
		}
	}
}

#endregion
