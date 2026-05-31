// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;

//added
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Gui;

#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	/// <summary>
		/*  This Chartstyle is a 2nd modification of the Candlesticks Chartstyle With Outline + Opacity one from Sim22 previous share
		 *	https://ninjatraderecosystem.com/user-app-share-download/candlesticks-chartstyle-with-outline-opacity/
		 *	and it's 1st modification by rafaelcoisa from this other share
		 *	https://ninjatrader.com/support/forum/forum/ninjascript-file-sharing/version-8-indicators/1172418-change-the-wick-color-of-the-candles?p=1172791#post1172791
		 *
		 *	It allows for coloring the Upper Wick separately from the Lower Wick of both Long and Short candles.
	     *
		 *	Just simply click on the chartstyle menu on your chart and select 'ccc' or set as the default chartstyle instead of candlesticks.
		 *	Then right Click on the Chart and select DataSeries or press Ctrl+F to customize the Wicks colors.
		 */
		
		/* For more details on the modification applied please refere to posts
		 * https://ninjatrader.com/support/forum/forum/ninjatrader-8/add-on-development/1188345-control-ohlc-upper-wicks-color-and-lower-wicks-color-independently-of-one-another?p=1189057#post1189057
		 * https://ninjatrader.com/support/forum/forum/ninjatrader-8/add-on-development/1188345-control-ohlc-upper-wicks-color-and-lower-wicks-color-independently-of-one-another?p=1189059#post1189059
		 * https://ninjatrader.com/support/forum/forum/ninjatrader-8/add-on-development/1188345-control-ohlc-upper-wicks-color-and-lower-wicks-color-independently-of-one-another?p=1189063#post1189063
		*/
	/// </summary>
	public class UpperWickvsLoweWick : ChartStyle
	{
		private object icon;

		public override int GetBarPaintWidth(int barWidth)
		{
			return 1 + 2 * (barWidth - 1) + 2 * (int) WidthOutline;
		}

		public override object Icon
		{
			get
			{
				if (icon == null)
				{
					icon = NinjaTrader.Gui.Tools.Icons.ChartChartStyle;
				}
				return icon;
			}
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars			bars			= chartBars.Bars;
			float			barWidth		= GetBarPaintWidth(BarWidthUI);
			Vector2			point0			= new Vector2();
			Vector2			point1			= new Vector2();
			RectangleF		rect			= new RectangleF();

			SharpDX.Direct2D1.StrokeStyleProperties ssPropsWick = new SharpDX.Direct2D1.StrokeStyleProperties();
			ssPropsWick.DashStyle								= DashStyleWick;
			SharpDX.Direct2D1.StrokeStyle styleWick 			= new SharpDX.Direct2D1.StrokeStyle(RenderTarget.Factory, ssPropsWick);
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode	= RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode							= SharpDX.Direct2D1.AntialiasMode.Aliased;
			
			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				Brush		overriddenBarBrush		= chartControl.GetBarOverrideBrush(chartBars, idx);
				Brush		overriddenOutlineBrush	= chartControl.GetCandleOutlineOverrideBrush(chartBars, idx);
				Brush		upOutBrushDX			= UpOutBrush.ToDxBrush(RenderTarget);
				Brush		dnOutBrushDX			= DnOutBrush.ToDxBrush(RenderTarget);
				Brush		dojiBrushDX				= DojiBrush.ToDxBrush(RenderTarget);
				Brush upOutBrushWickDX = UpOutBrushWick.ToDxBrush(RenderTarget);
				Brush dnOutBrushWickDX = DnOutBrushWick.ToDxBrush(RenderTarget);
				Brush dojiBrushWickDX = DojiBrushWick.ToDxBrush(RenderTarget);
				
				Brush upOutBrushWickDX1 = UpOutBrushWick1.ToDxBrush(RenderTarget);
				Brush dnOutBrushWickDX1 = DnOutBrushWick1.ToDxBrush(RenderTarget);


				double		closeValue				= bars.GetClose(idx);
				float		closeY					= chartScale.GetYByValue(closeValue);
				float		highY					= chartScale.GetYByValue(bars.GetHigh(idx));
				float		lowY					= chartScale.GetYByValue(bars.GetLow(idx));
				double		openValue				= bars.GetOpen(idx);
				float		openY					= chartScale.GetYByValue(openValue);
				float		x						= chartControl.GetXByBarIndex(chartBars, idx);

				
				if (Math.Abs(openY - closeY) < 0.0000001)
				{
					// Doji Line 
					point0.X = x - barWidth * 0.5f;
					point0.Y = closeY;
					point1.X = x + barWidth * 0.5f;
					point1.Y = closeY;

					TransformBrush(overriddenOutlineBrush ?? dojiBrushDX, new RectangleF(point0.X, point0.Y - WidthOutline, barWidth, WidthOutline));
					RenderTarget.DrawLine(point0, point1, overriddenOutlineBrush ?? dojiBrushDX, WidthOutline);
				}
				else
				{
					// Candle
					Brush brush	= closeValue >= openValue ? UpBrushDX : DownBrushDX;
					brush.Opacity	= Opacity * 0.01f;
					rect.X		= x - barWidth * 0.5f + 0.5f;
					rect.Y		= Math.Min(closeY, openY);
					rect.Width	= barWidth - 1;
					rect.Height	= Math.Max(openY, closeY) - Math.Min(closeY, openY);
					TransformBrush(overriddenBarBrush ?? brush, rect);
					TransformBrush(overriddenOutlineBrush ?? (closeValue > openValue ? upOutBrushDX : dnOutBrushDX), rect);
					RenderTarget.FillRectangle(rect, overriddenBarBrush ?? brush);
					RenderTarget.DrawRectangle(rect, overriddenOutlineBrush ?? (closeValue > openValue ? upOutBrushDX : dnOutBrushDX), WidthOutline);

				}

				// Up Bar Upper Wick
				if (highY < Math.Min(openY, closeY))
				{
					point0.X = x;
					point0.Y = highY;
					point1.X = x;
					point1.Y = Math.Min(openY, closeY);
					TransformBrush(overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX : dnOutBrushWickDX), new RectangleF(point0.X - WidthWick, point0.Y, WidthWick, point1.Y - point0.Y));
					RenderTarget.DrawLine(point0, point1, overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX : dnOutBrushWickDX), WidthWick, styleWick);
				}
				
				// Up Bar Lower Wick
				if (highY < Math.Min(openY, closeY))
				{
					point0.X = x;
					point0.Y = lowY;
					point1.X = x;
					point1.Y = Math.Max(openY, closeY);
					TransformBrush(overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX1 : dnOutBrushWickDX1), new RectangleF(point0.X - WidthWick, point0.Y, WidthWick, point1.Y - point0.Y));
					RenderTarget.DrawLine(point0, point1, overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX1 : dnOutBrushWickDX1), WidthWick, styleWick);
				}

				// Down Bar Upper Wick
				if (lowY > Math.Max(openY, closeY))
				{
					point0.X = x;
					point0.Y = highY;
					point1.X = x;
					point1.Y = Math.Min(openY, closeY);
					TransformBrush(overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX : dnOutBrushWickDX), new RectangleF(point1.X - WidthWick, point1.Y, WidthWick, point0.Y - point1.Y));
					RenderTarget.DrawLine(point0, point1, overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX : dnOutBrushWickDX), WidthWick, styleWick);
				}

				// Down Bar Lower Wick
				if (lowY > Math.Max(openY, closeY))
				{
					point0.X = x;
					point0.Y = lowY;
					point1.X = x;
					point1.Y = Math.Max(openY, closeY);
					TransformBrush(overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX1 : dnOutBrushWickDX1), new RectangleF(point1.X - WidthWick, point1.Y, WidthWick, point0.Y - point1.Y));
					RenderTarget.DrawLine(point0, point1, overriddenOutlineBrush ?? (closeValue == openValue ? dojiBrushWickDX : closeValue > openValue ? upOutBrushWickDX1 : dnOutBrushWickDX1), WidthWick, styleWick);
				}
				
				upOutBrushDX.Dispose();
				dnOutBrushDX.Dispose();
				dojiBrushDX.Dispose();
				upOutBrushWickDX.Dispose();
				dnOutBrushWickDX.Dispose();
				dojiBrushWickDX.Dispose();
				
				upOutBrushWickDX1.Dispose();
				dnOutBrushWickDX1.Dispose();
			}
			styleWick.Dispose();
			RenderTarget.AntialiasMode = oldAntialiasMode;

		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name				= @"UpperWickvsLoweWick";
				Description			= "Plots customizable outline, wicks, separately colors wicks and bar opacity Candlesticks";
				ChartStyleType		= (ChartStyleType) 11227;
				
				this.UpBrush		= System.Windows.Media.Brushes.Pink;       // Colors - Up Bar 
				this.DownBrush 		= System.Windows.Media.Brushes.Yellow;     // Colors - Down Bar
				
				this.UpOutBrush 	= System.Windows.Media.Brushes.Pink;       // Colors - Up Bar Outline
				this.DnOutBrush 	= System.Windows.Media.Brushes.Cyan;       // Colors - Down Bar Outline
				
				this.UpOutBrushWick = System.Windows.Media.Brushes.Lime;       // Colors - Up Bar Upper Wick
				this.UpOutBrushWick1 = System.Windows.Media.Brushes.Beige;     // Colors - Up Bar Lower Wick
				
				this.DnOutBrushWick = System.Windows.Media.Brushes.Red;        // Colors - Down Bar Upper Wick
				this.DnOutBrushWick1 = System.Windows.Media.Brushes.Black;     // Colors - Down Bar Lower Wick
				
				this.DojiBrush 		= System.Windows.Media.Brushes.Gray;       // Colors - Doji Outline
				this.DojiBrushWick = System.Windows.Media.Brushes.Gray;        // Colors - Doji Wick
				
				WidthOutline		= 1f;
				WidthWick			= 3f;
				Opacity				= 85;
			}
			else if (State == State.Configure)
			{
				SetPropertyName("BarWidth",		Custom.Resource.NinjaScriptChartStyleBarWidth);
				
				SetPropertyName("UpBrush",		"Colors - Up Bar");
				SetPropertyName("DownBrush",	"Colors - Down Bar");
				
				Properties.Remove(Properties.Find("Stroke", true));
				Properties.Remove(Properties.Find("Stroke2", true));
				
				UpBrush.Freeze();
				DownBrush.Freeze();
				UpOutBrush.Freeze();
				DnOutBrush.Freeze();
				DojiBrush.Freeze();
				UpOutBrushWick.Freeze();
				DnOutBrushWick.Freeze();
				DojiBrushWick.Freeze();
				
				UpOutBrushWick1.Freeze();
				DnOutBrushWick1.Freeze();
			}
		}
		#region Properties

		[XmlIgnore]
		[Display(Name="Colors - Up Bar Outline", Description="Outline color of up bar", Order=1, GroupName="Chart Style")]
		public System.Windows.Media.Brush UpOutBrush
		{ get; set; }

		[Browsable(false)]
		public string UpOutBrushSerializable
		{
			get { return Serialize.BrushToString(UpOutBrush); }
			set { UpOutBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name="Colors - Down Bar Outline", Description="Outline color of down bar", Order=2, GroupName="Chart Style")]
		public System.Windows.Media.Brush DnOutBrush
		{ get; set; }

		[Browsable(false)]
		public string DnOutBrushSerializable
		{
			get { return Serialize.BrushToString(DnOutBrush); }
			set { DnOutBrush = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Colors - Up Bar Upper Wick", Description = "Wick color of up bar upper wick", Order = 3, GroupName = "Chart Style")]
		public System.Windows.Media.Brush UpOutBrushWick
		{ get; set; }

		[Browsable(false)]
		public string UpOutBrushWickSerializable
		{
			get { return Serialize.BrushToString(UpOutBrushWick); }
			set { UpOutBrushWick = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Colors - Up Bar Lower Wick", Description = "Wick color of up bar lower wick", Order = 4, GroupName = "Chart Style")]
		public System.Windows.Media.Brush UpOutBrushWick1
		{ get; set; }

		[Browsable(false)]
		public string UpOutBrushWick1Serializable
		{
			get { return Serialize.BrushToString(UpOutBrushWick1); }
			set { UpOutBrushWick1 = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Colors - Down Bar Upper Wick", Description = "Wick color of down bar upper wick", Order = 5, GroupName = "Chart Style")]
		public System.Windows.Media.Brush DnOutBrushWick
		{ get; set; }

		[Browsable(false)]
		public string DnOutBrushWickSerializable
		{
			get { return Serialize.BrushToString(DnOutBrushWick); }
			set { DnOutBrushWick = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Colors - Down Bar Lower Wick", Description = "Wick color of down bar lower wick", Order = 6, GroupName = "Chart Style")]
		public System.Windows.Media.Brush DnOutBrushWick1
		{ get; set; }

		[Browsable(false)]
		public string DnOutBrushWick1Serializable
		{
			get { return Serialize.BrushToString(DnOutBrushWick1); }
			set { DnOutBrushWick1 = Serialize.StringToBrush(value); }
		}
	
		[XmlIgnore]
		[Display(Name="Colors - Doji Outline", Description="Color of doji bar outline", Order=7, GroupName="Chart Style")]
		public System.Windows.Media.Brush DojiBrush
		{ get; set; }

		[Browsable(false)]
		public string DojiBrushSerializable
		{
			get { return Serialize.BrushToString(DojiBrush); }
			set { DojiBrush = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Colors - Doji Wick", Description = "Color of doji bar wicks", Order = 8, GroupName = "Chart Style")]
		public System.Windows.Media.Brush DojiBrushWick
		{ get; set; }

		[Browsable(false)]
		public string DojiBrushWickSerializable
		{
			get { return Serialize.BrushToString(DojiBrushWick); }
			set { DojiBrushWick = Serialize.StringToBrush(value); }
		}

		[Range(0, 100),NinjaScriptProperty]
		[Display(Name="Opacity % (0-100)", Description="Opacity Of Bar.", Order=9, GroupName="Chart Style")]
		public int Opacity
		{ get; set; }
		
		[Range(1.0f, 20.0f),NinjaScriptProperty]
		[Display(Name="Outline Width", Description="Outline Width.", Order=10, GroupName="Chart Style")]
		public float WidthOutline
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Wick Dashstyle", Description="Wick Dashstyle.", Order=11, GroupName="Chart Style")]
		public SharpDX.Direct2D1.DashStyle DashStyleWick
		{ get; set; }
		
		[Range(1.0f, 20.0f),NinjaScriptProperty]
		[Display(Name="Wick Width", Description="Wick Width.", Order=12, GroupName="Chart Style")]
		public float WidthWick
		{ get; set; }
		
		#endregion
	}
}
