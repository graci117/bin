// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using SharpDX;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows;
using System.Xml.Serialization;

#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class CandleStyle : ChartStyle
	{

		private System.Windows.Media.Brush	dojiBrush;
		private SharpDX.Direct2D1.Brush		dojiBrushDX;
		private object						icon;

		public override int GetBarPaintWidth(int barWidth) => 1 + 2 * (barWidth - 1) + 2 * (int) Math.Round(Stroke.Width);

		public override object Icon => icon ??= Gui.Tools.Icons.ChartChartStyle;

		public override void OnRender(Gui.Chart.ChartControl chartControl, Gui.Chart.ChartScale chartScale, Gui.Chart.ChartBars chartBars)
		{
			Data.Bars		bars			= chartBars.Bars;
			float			barWidth		= GetBarPaintWidth(BarWidthUI);
			Vector2			point0			= new();
			Vector2			point1			= new();
			RectangleF		rect			= new();

			Gui.Stroke		stroke			= !WickMatchesBody ? Stroke		: new Gui.Stroke(Stroke.Brush, WickStyle, WickWidth);
			Gui.Stroke		stroke2			= !WickMatchesBody ? Stroke2	: new Gui.Stroke(Stroke2.Brush, WickStyle, WickWidth);

			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				SharpDX.Direct2D1.Brush		overriddenBarBrush		= chartControl.GetBarOverrideBrush(chartBars, idx);
				SharpDX.Direct2D1.Brush		overriddenOutlineBrush	= chartControl.GetCandleOutlineOverrideBrush(chartBars, idx);
				double						closeValue				= bars.GetClose(idx);
				double						highValue				= bars.GetHigh(idx);
				double						lowValue				= bars.GetLow(idx);
				double						openValue				= bars.GetOpen(idx);
				int							close					= chartScale.GetYByValue(closeValue);
				int							high					= chartScale.GetYByValue(highValue);
				int							low						= chartScale.GetYByValue(lowValue);
				int							open					= chartScale.GetYByValue(openValue);
				int							x						= chartControl.GetXByBarIndex(chartBars, idx);
				bool						isDoji					= Math.Abs(openValue - closeValue) < 0.0000000001;

				if (isDoji)
				{
					// Line 
					point0.X					= x - barWidth * 0.5f;
					point0.Y					= close;
					point1.X					= x + barWidth * 0.5f;
					point1.Y					= close;
					SharpDX.Direct2D1.Brush b	= overriddenOutlineBrush ?? (WickMatchesBody ? DojiBrushDX: stroke.BrushDX);
					if (b is not SharpDX.Direct2D1.SolidColorBrush)
						TransformBrush(b, new RectangleF(point0.X, point0.Y - stroke.Width, barWidth, stroke.Width));
					RenderTarget.DrawLine(point0, point1, b, stroke.Width, stroke.StrokeStyle);
				}
				else
				{
					// Candle
					rect.X		= x - barWidth * 0.5f + 0.5f;
					rect.Y		= Math.Min(close, open);
					rect.Width	= barWidth - 1;
					rect.Height	= Math.Max(open, close) - Math.Min(close, open);

					// Rectangle fill
					SharpDX.Direct2D1.Brush brush	= overriddenBarBrush ?? (closeValue >= openValue ? UpBrushDX : DownBrushDX);
					if (brush is not SharpDX.Direct2D1.SolidColorBrush)
						TransformBrush(brush, rect);
					RenderTarget.FillRectangle(rect, brush);

					// Rectangle border
					SharpDX.Direct2D1.Brush brush2 = overriddenOutlineBrush ?? (WickMatchesBody ? closeValue >= openValue ? UpBrushDX : DownBrushDX: stroke.BrushDX);
					if (brush2 is not SharpDX.Direct2D1.SolidColorBrush)
						TransformBrush(brush2, rect);
					RenderTarget.DrawRectangle(rect, brush2 ?? stroke.BrushDX, stroke.Width, stroke.StrokeStyle);
				}

				SharpDX.Direct2D1.Brush br = overriddenOutlineBrush ?? (WickMatchesBody
					? isDoji ? DojiBrushDX : closeValue >= openValue ? UpBrushDX : DownBrushDX
					: stroke2.BrushDX);

				// High wick
				if (highValue > Math.Max(openValue, closeValue))
				{
					point0.X	= x;
					point0.Y	= high;
					point1.X	= x;
					point1.Y	= openValue > closeValue ? open : close;
					if (br is not SharpDX.Direct2D1.SolidColorBrush)
						TransformBrush(br, new RectangleF(point0.X - stroke2.Width, point0.Y, stroke2.Width, point1.Y - point0.Y));
					RenderTarget.DrawLine(point0, point1, br, stroke2.Width, stroke2.StrokeStyle);
				}

				// Low wick
				if (lowValue < Math.Min(openValue, closeValue))
				{
					point0.X = x;
					point0.Y = low;
					point1.X = x;
					point1.Y = openValue < closeValue ? open : close;
					if (br is not SharpDX.Direct2D1.SolidColorBrush)
						TransformBrush(br, new RectangleF(point1.X - stroke2.Width, point1.Y, stroke2.Width, point0.Y - point1.Y));
					RenderTarget.DrawLine(point0, point1, br, stroke2.Width, stroke2.StrokeStyle);
				}
			}
		}

		public override void OnRenderTargetChanged()
		{
			base.OnRenderTargetChanged();
			dojiBrushDX?.Dispose();
			dojiBrushDX = null;
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleCandlestick;
				ChartStyleType	= Gui.Chart.ChartStyleType.CandleStick;
				DojiBrush		= (Application.Current.FindResource("ChartControl.Stroke") as System.Windows.Media.Pen)?.Brush;
			}
			else if (State == State.Configure)
			{
				SetPropertyName(nameof(BarWidth),	Custom.Resource.NinjaScriptChartStyleBarWidth);
				SetPropertyName(nameof(DownBrush),	Custom.Resource.NinjaScriptChartStyleCandleDownBarsColor);
				SetPropertyName(nameof(UpBrush),	Custom.Resource.NinjaScriptChartStyleCandleUpBarsColor);
				SetPropertyName(nameof(Stroke),		Custom.Resource.NinjaScriptChartStyleCandleOutline);
				SetPropertyName(nameof(Stroke2),	Custom.Resource.NinjaScriptChartStyleCandleWick);

				SetPropertyOrder(nameof(BarWidth),			1);
				SetPropertyOrder(nameof(UpBrush),			3);
				SetPropertyOrder(nameof(DownBrush),			4);
				SetPropertyOrder(nameof(Stroke),			5);
				SetPropertyOrder(nameof(Stroke2),			6);
			}
		}

		#region Additional Properties
		[RefreshProperties(RefreshProperties.All)]
		[Display (ResourceType = typeof(NTRes.NinjaTrader.Gui.Chart.ChartResources), Name = "GuiChartStyleWickMatchesBody", Order = 2)]
		[DefaultIfMissing(false)]
		public bool WickMatchesBody { get; set; } = true;

		[Display (ResourceType = typeof(NTRes.NinjaTrader.Gui.Chart.ChartResources), Name = "GuiChartStyleDojiBrush", Order = 5)]
		[XmlIgnore]
		public System.Windows.Media.Brush DojiBrush
		{
			get => dojiBrush ??= (Application.Current.FindResource("ChartControl.Stroke") as System.Windows.Media.Pen)?.Brush;
			set
			{
				dojiBrush = value;
				if (dojiBrush is { CanFreeze: true })
					dojiBrush.Freeze();
				dojiBrushDX = null;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		[CLSCompliant(false)]
		public SharpDX.Direct2D1.Brush DojiBrushDX
		{
			get
			{
				if (dojiBrushDX?.IsDisposed != false)
					dojiBrushDX = Gui.DxExtensions.ToDxBrush(DojiBrush, RenderTarget);
				return dojiBrushDX;
			}
		}

		[Browsable(false)]
		public string DojiBrushSerialize
		{
			get => Gui.Serialize.BrushToString(DojiBrush);
			set => DojiBrush = Gui.Serialize.StringToBrush(value);
		}

		[Display (ResourceType = typeof(NTRes.NinjaTrader.Gui.Chart.ChartResources), Name = "GuiChartStyleWickStyle", Order = 6)]
		public Gui.DashStyleHelper WickStyle { get; set; } = Gui.DashStyleHelper.Solid;

		[Display (ResourceType = typeof(NTRes.NinjaTrader.Gui.Chart.ChartResources), Name = "GuiChartStyleWickWidth", Order = 7)]
		public int WickWidth { get; set; } = 1;

		public override object Clone()
		{
			CandleStyle ret = base.Clone() as CandleStyle;
			if (ret != null)
			{
				ret.WickMatchesBody	= WickMatchesBody;
				ret.DojiBrush		= DojiBrush?.Clone();
				ret.WickStyle		= WickStyle;
				ret.WickWidth		= WickWidth;
			}
			return ret ?? new CandleStyle();
		}
		#endregion
	}
}
