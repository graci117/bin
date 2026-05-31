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

using _nsHeikenAshi_Smoothed;

namespace _nsHeikenAshi_Smoothed
{
public enum PaintingStyle
	{
	PaintVisibleOnly,
	PaintToLast,
	PaintAll
	}		
}
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class HeikenAshiSmoothed : Indicator
	{
		#region Variables
		private int BodyWidth;		
		private SharpDX.Direct2D1.Brush barBrushUp, barBrushDown, shadowBrush, priceLineBrush, priceTextBrush, priceAreaBrush;
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"HeikenAshi technique discussed in the article 'Using Heiken-Ashi Technique' in February 2004 issue of TASC magazine. "
																+ "This version has been smoothed to better filter out noise that is present even in the HeikenAshi chart.";
				Name										= "HeikenAshiSmoothed";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
				
				BarColorDown							= Brushes.Red;
				BarColorUp								= Brushes.LightGreen;
				ShadowColor								= Brushes.Transparent;
				PriceLineColor							= Brushes.Blue;
				ShowPriceBars							= false;
				ShowPriceLine							= true;
				ShadowWidth								= 2;
				SmoothingPeriod							= 4;
				PriceLineLength							= 34;
				PriceLineWidth							= 2;
				PriceLineStyle							= DashStyleHelper.Solid;
				Font									= new SimpleFont("Arial", 10);
				BodyWidth								= 1;
				
				AddPlot(Brushes.Transparent, "HAOpen");
				AddPlot(Brushes.Transparent, "HAHigh");
				AddPlot(Brushes.Transparent, "HALow");
				AddPlot(Brushes.Transparent, "HAClose");
				
			}
		}

		protected override void OnBarUpdate()
		{	
			// make normal price bars invisible
			if (!ShowPriceBars)
			{
				BarBrushes[0] = Brushes.Transparent;
				CandleOutlineBrushes[0] = Brushes.Transparent;
			}
			
			if (CurrentBar == 0)
			{
				HAOpen[0] 	=	Open[0];
                HAHigh[0] 	=	High[0];
                HALow[0]	=	Low[0];
                HAClose[0]	=	Close[0];
				return;
			}

			// Draw HeikenAshi bars as specified by user
			double _dblAverageOpen 		= HMA(Open, SmoothingPeriod)[0];
			double _dblAverageHigh 		= HMA(High, SmoothingPeriod)[0];
			double _dblAverageLow 		= HMA(Low, SmoothingPeriod)[0];
			double _dblAverageClose 	= HMA(Close, SmoothingPeriod)[0];
			double _dblAverageHAOpen1 	= HMA(HAOpen, SmoothingPeriod)[1];
			double _dblAverageHAClose1 	= HMA(HAClose, SmoothingPeriod)[1];
			
			HAClose[0] = ((_dblAverageOpen + _dblAverageHigh + _dblAverageLow + _dblAverageClose) / 4); // Calculate the close
			HAOpen[0] = ((_dblAverageHAOpen1 + _dblAverageHAClose1) / 2); // Calculate the open
			
			double _dblAverageHAOpen0 = HMA(HAOpen, SmoothingPeriod)[0]; // Calculate the high
			HAHigh[0] = (Math.Max(_dblAverageHigh, _dblAverageHAOpen0));
			
			HALow[0] = (Math.Min(_dblAverageLow, _dblAverageHAOpen0)); // Calculate the low
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			BodyWidth = (int)chartControl.BarWidth + Math.Min(ShadowWidth * 2, 4);

			// loop through all of the viewable range of the chart
			for (int barIndex = ChartBars.FromIndex; barIndex <= ChartBars.ToIndex; barIndex++)
			{
				if(ShadowColor.ToString() == Brushes.Transparent.ToString())
				{
					if(HAClose.GetValueAt(barIndex) > HAOpen.GetValueAt(barIndex))
					{
						DrawLineNT(barBrushUp,
							barIndex,
							HAHigh.GetValueAt(barIndex),
							barIndex,
							Math.Max(HAClose.GetValueAt(barIndex),HAOpen.GetValueAt(barIndex)),
							ShadowWidth,
							chartScale);
						
						DrawLineNT(barBrushUp,
							barIndex,
							HALow.GetValueAt(barIndex),
							barIndex,
							Math.Min(HAClose.GetValueAt(barIndex),HAOpen.GetValueAt(barIndex)),
							ShadowWidth,
							chartScale);
					}
					else
					{
						DrawLineNT(barBrushDown,
							barIndex,
							HAHigh.GetValueAt(barIndex),
							barIndex,
							Math.Max(HAClose.GetValueAt(barIndex),HAOpen.GetValueAt(barIndex)),
							ShadowWidth,
							chartScale);
						
						DrawLineNT(barBrushDown,
							barIndex,
							HALow.GetValueAt(barIndex),
							barIndex,
							Math.Min(HAClose.GetValueAt(barIndex),HAOpen.GetValueAt(barIndex)),
							ShadowWidth,
							chartScale);
					}
				}
				else
				{
					DrawLineNT(shadowBrush,
						barIndex,
						HAHigh.GetValueAt(barIndex),
						barIndex,
						Math.Max(HAClose.GetValueAt(barIndex),HAOpen.GetValueAt(barIndex)),
						ShadowWidth,
						chartScale);
					
					DrawLineNT(shadowBrush,
						barIndex,
						HALow.GetValueAt(barIndex),
						barIndex,
						Math.Min(HAClose.GetValueAt(barIndex),HAOpen.GetValueAt(barIndex)),
						ShadowWidth,
						chartScale);
				}
						
				if(HAClose.GetValueAt(barIndex) > HAOpen.GetValueAt(barIndex))
				{
					DrawLineNT(barBrushUp,
						barIndex,
						HAOpen.GetValueAt(barIndex),
						barIndex,
						HAClose.GetValueAt(barIndex),
						BodyWidth,
						chartScale);
				}
				else
				{
					DrawLineNT(barBrushDown,
						barIndex,
						HAOpen.GetValueAt(barIndex),
						barIndex,
						HAClose.GetValueAt(barIndex),
						BodyWidth,
						chartScale);
				}
				
			}
			
			// Draw price line if wanted		
			if (ShowPriceLine)
			{
				DrawLineNT(priceLineBrush,
					Math.Max(PriceLineLength, CurrentBar - 15),
					Close.GetValueAt(CurrentBar),
					CurrentBar,
					Close.GetValueAt(CurrentBar),
					PriceLineWidth,
					PriceLineStyle,
					chartScale);
				
				DrawStringNT("  << " + Close.GetValueAt(CurrentBar).ToString() + " = Last Price",
					Font,
					priceTextBrush,
					CurrentBar,
					Close.GetValueAt(CurrentBar),
					priceAreaBrush,
					chartScale);
			}
		}
		
		public override void OnRenderTargetChanged()
        {
            if (barBrushUp != null)
				barBrushUp.Dispose();
			if (barBrushDown != null)
				barBrushDown.Dispose();
			if (shadowBrush != null)
				shadowBrush.Dispose();
			if (priceLineBrush != null)
				priceLineBrush.Dispose();
			if (priceTextBrush != null)
				priceTextBrush.Dispose();
			if (priceAreaBrush != null)
				priceAreaBrush.Dispose();
 
			if (RenderTarget != null)
			{
				barBrushUp = BarColorUp.ToDxBrush(RenderTarget);
				barBrushDown = BarColorDown.ToDxBrush(RenderTarget);
				shadowBrush = ShadowColor.ToDxBrush(RenderTarget);
				priceLineBrush = PriceLineColor.ToDxBrush(RenderTarget);
				priceTextBrush = Brushes.White.ToDxBrush(RenderTarget);
				priceAreaBrush = Brushes.Black.ToDxBrush(RenderTarget);
			}
        }
		
		public override void OnCalculateMinMax()
        {
            base.OnCalculateMinMax();
			
            if (Bars == null || ChartControl == null)
                return;

            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
                double tmpHigh 	= 	HAHigh.GetValueAt(idx);
                double tmpLow 	= 	HALow.GetValueAt(idx);
				
                if (tmpHigh != 0 && tmpHigh > MaxValue)
                    MaxValue = tmpHigh;
                if (tmpLow != 0 && tmpLow < MinValue)
                    MinValue = tmpLow;										
            }
        }
		
		#region SharpDX Helper Classes/Methods
		private void DrawString(string text, SimpleFont font, SharpDX.Direct2D1.Brush dxBrush, double pointX, double pointY, SharpDX.Direct2D1.Brush dxAreaBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, ChartPanel.X + ChartPanel.W,
				textFormat.FontSize);
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX, pointY-textLayout.Metrics.Height/2).ToVector2();
			
			float newW = textLayout.Metrics.Width; 
            float newH = textLayout.Metrics.Height;
            SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF((float)pointX+2, (float)pointY-textLayout.Metrics.Height/2, newW+5, newH+2);
            RenderTarget.FillRectangle(PLBoundRect, dxAreaBrush);
			
			RenderTarget.DrawTextLayout(TextPlotPoint, textLayout, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textLayout.Dispose();
			textFormat.Dispose();
		}
		
		private void DrawStringNT(string text, SimpleFont font, SharpDX.Direct2D1.Brush dxBrush, double pointX, double pointY, SharpDX.Direct2D1.Brush dxAreaBrush, ChartScale chartScale)
		{
			DrawString(text, font, dxBrush, ChartControl.GetXByBarIndex(ChartBars,(int)pointX), chartScale.GetYByValue(pointY), dxAreaBrush);
		}
		
		private void DrawLine(SharpDX.Direct2D1.Brush dxBrush, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			if (dashStyle == DashStyleHelper.Dash)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash;
			if (dashStyle == DashStyleHelper.DashDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot;
			if (dashStyle == DashStyleHelper.DashDotDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;
			if (dashStyle == DashStyleHelper.Dot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;
			if (dashStyle == DashStyleHelper.Solid)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			SharpDX.Vector2 startPoint = new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint = new System.Windows.Point(x2, y2).ToVector2();
			RenderTarget.DrawLine(startPoint, endPoint, dxBrush, width, strokeStyle);
		}
		
		private void DrawLine(SharpDX.Direct2D1.Brush dxBrush, double x1, double y1, double x2, double y2, float width)
		{
			DrawLine(dxBrush, x1, y1, x2, y2, width, DashStyleHelper.Solid);
		}
		
		private void DrawLineNT(SharpDX.Direct2D1.Brush dxBrush, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle, ChartScale chartScale)
		{
			DrawLine(dxBrush, ChartControl.GetXByBarIndex(ChartBars,(int)x1), chartScale.GetYByValue(y1), ChartControl.GetXByBarIndex(ChartBars,(int)x2), chartScale.GetYByValue(y2), width, dashStyle);
		}
		
		private void DrawLineNT(SharpDX.Direct2D1.Brush dxBrush, double x1, double y1, double x2, double y2, float width, ChartScale chartScale)
		{
			DrawLine(dxBrush, ChartControl.GetXByBarIndex(ChartBars,(int)x1), chartScale.GetYByValue(y1), ChartControl.GetXByBarIndex(ChartBars,(int)x2), chartScale.GetYByValue(y2), width, DashStyleHelper.Solid);
		}
    	#endregion

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Down color", Description="Color of down bars.", Order=1, GroupName="Parameters")]
		public Brush BarColorDown
		{ get; set; }

		[Browsable(false)]
		public string BarColorDownSerializable
		{
			get { return Serialize.BrushToString(BarColorDown); }
			set { BarColorDown = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Up color", Description="Color of up bars.", Order=2, GroupName="Parameters")]
		public Brush BarColorUp
		{ get; set; }

		[Browsable(false)]
		public string BarColorUpSerializable
		{
			get { return Serialize.BrushToString(BarColorUp); }
			set { BarColorUp = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Shadow color", Description="Color of shadow line. If not specified, will use the 'Up color' and 'Down color' colors.", Order=3, GroupName="Parameters")]
		public Brush ShadowColor
		{ get; set; }

		[Browsable(false)]
		public string ShadowColorSerializable
		{
			get { return Serialize.BrushToString(ShadowColor); }
			set { ShadowColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Shadow width", Description="Width of shadow line.", Order=4, GroupName="Parameters")]
		public int ShadowWidth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smoothing period", Description="Period for smoothing the indicator", Order=5, GroupName="Parameters")]
		public int SmoothingPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Price Bars", Description="Show/hide the price bars on the chart.", Order=6, GroupName="Parameters")]
		public bool ShowPriceBars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show price line", Description="Draws a line showing the price of the underlying equity.", Order=7, GroupName="Parameters")]
		public bool ShowPriceLine
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Price Line length", Description="Length of price line.", Order=8, GroupName="Parameters")]
		public int PriceLineLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Price Line width", Description="Width of price line.", Order=9, GroupName="Parameters")]
		public int PriceLineWidth
		{ get; set; }	

		[NinjaScriptProperty]
		[Display(Name="Price Line style", Description="Style of price line.", Order=10, GroupName="Parameters")]
		public DashStyleHelper PriceLineStyle
        { get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Price Line color", Description="Color of price line", Order=11, GroupName="Parameters")]
		public Brush PriceLineColor
		{ get; set; }

		[Browsable(false)]
		public string PriceLineColorSerializable
		{
			get { return Serialize.BrushToString(PriceLineColor); }
			set { PriceLineColor = Serialize.StringToBrush(value); }
		}
		
		[Display(Name="Price Marker Font", Description="Font for Price Marker", Order=12, GroupName="Parameters")]
		public SimpleFont Font
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAOpen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAHigh
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HALow
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HAClose
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HeikenAshiSmoothed[] cacheHeikenAshiSmoothed;
		public HeikenAshiSmoothed HeikenAshiSmoothed(Brush barColorDown, Brush barColorUp, Brush shadowColor, int shadowWidth, int smoothingPeriod, bool showPriceBars, bool showPriceLine, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, Brush priceLineColor)
		{
			return HeikenAshiSmoothed(Input, barColorDown, barColorUp, shadowColor, shadowWidth, smoothingPeriod, showPriceBars, showPriceLine, priceLineLength, priceLineWidth, priceLineStyle, priceLineColor);
		}

		public HeikenAshiSmoothed HeikenAshiSmoothed(ISeries<double> input, Brush barColorDown, Brush barColorUp, Brush shadowColor, int shadowWidth, int smoothingPeriod, bool showPriceBars, bool showPriceLine, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, Brush priceLineColor)
		{
			if (cacheHeikenAshiSmoothed != null)
				for (int idx = 0; idx < cacheHeikenAshiSmoothed.Length; idx++)
					if (cacheHeikenAshiSmoothed[idx] != null && cacheHeikenAshiSmoothed[idx].BarColorDown == barColorDown && cacheHeikenAshiSmoothed[idx].BarColorUp == barColorUp && cacheHeikenAshiSmoothed[idx].ShadowColor == shadowColor && cacheHeikenAshiSmoothed[idx].ShadowWidth == shadowWidth && cacheHeikenAshiSmoothed[idx].SmoothingPeriod == smoothingPeriod && cacheHeikenAshiSmoothed[idx].ShowPriceBars == showPriceBars && cacheHeikenAshiSmoothed[idx].ShowPriceLine == showPriceLine && cacheHeikenAshiSmoothed[idx].PriceLineLength == priceLineLength && cacheHeikenAshiSmoothed[idx].PriceLineWidth == priceLineWidth && cacheHeikenAshiSmoothed[idx].PriceLineStyle == priceLineStyle && cacheHeikenAshiSmoothed[idx].PriceLineColor == priceLineColor && cacheHeikenAshiSmoothed[idx].EqualsInput(input))
						return cacheHeikenAshiSmoothed[idx];
			return CacheIndicator<HeikenAshiSmoothed>(new HeikenAshiSmoothed(){ BarColorDown = barColorDown, BarColorUp = barColorUp, ShadowColor = shadowColor, ShadowWidth = shadowWidth, SmoothingPeriod = smoothingPeriod, ShowPriceBars = showPriceBars, ShowPriceLine = showPriceLine, PriceLineLength = priceLineLength, PriceLineWidth = priceLineWidth, PriceLineStyle = priceLineStyle, PriceLineColor = priceLineColor }, input, ref cacheHeikenAshiSmoothed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HeikenAshiSmoothed HeikenAshiSmoothed(Brush barColorDown, Brush barColorUp, Brush shadowColor, int shadowWidth, int smoothingPeriod, bool showPriceBars, bool showPriceLine, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, Brush priceLineColor)
		{
			return indicator.HeikenAshiSmoothed(Input, barColorDown, barColorUp, shadowColor, shadowWidth, smoothingPeriod, showPriceBars, showPriceLine, priceLineLength, priceLineWidth, priceLineStyle, priceLineColor);
		}

		public Indicators.HeikenAshiSmoothed HeikenAshiSmoothed(ISeries<double> input , Brush barColorDown, Brush barColorUp, Brush shadowColor, int shadowWidth, int smoothingPeriod, bool showPriceBars, bool showPriceLine, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, Brush priceLineColor)
		{
			return indicator.HeikenAshiSmoothed(input, barColorDown, barColorUp, shadowColor, shadowWidth, smoothingPeriod, showPriceBars, showPriceLine, priceLineLength, priceLineWidth, priceLineStyle, priceLineColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HeikenAshiSmoothed HeikenAshiSmoothed(Brush barColorDown, Brush barColorUp, Brush shadowColor, int shadowWidth, int smoothingPeriod, bool showPriceBars, bool showPriceLine, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, Brush priceLineColor)
		{
			return indicator.HeikenAshiSmoothed(Input, barColorDown, barColorUp, shadowColor, shadowWidth, smoothingPeriod, showPriceBars, showPriceLine, priceLineLength, priceLineWidth, priceLineStyle, priceLineColor);
		}

		public Indicators.HeikenAshiSmoothed HeikenAshiSmoothed(ISeries<double> input , Brush barColorDown, Brush barColorUp, Brush shadowColor, int shadowWidth, int smoothingPeriod, bool showPriceBars, bool showPriceLine, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, Brush priceLineColor)
		{
			return indicator.HeikenAshiSmoothed(input, barColorDown, barColorUp, shadowColor, shadowWidth, smoothingPeriod, showPriceBars, showPriceLine, priceLineLength, priceLineWidth, priceLineStyle, priceLineColor);
		}
	}
}

#endregion
