// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion

//This namespace holds Drawing tools in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.DrawingTools
{
	/// <summary>
	/// Represents an interface that exposes information regarding a Time Cycles IDrawingTool.
	/// </summary>
	public class TimeCycles : DrawingTool
	{
		#region Variables
		private				Brush			areaBrush;
		private readonly	DeviceBrush		areaBrushDevice			= new();
		private				int				areaOpacity;
		private				List<int>		anchorBars;
		private const		int				cursorSensitivity		= 15;
		private				int				diameter;
		private				int				radius;
		#endregion

		#region Properties
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolShapesAreaBrush", GroupName = "NinjaScriptGeneral", Order = 0)]
		public Brush AreaBrush
		{
			get => areaBrush;
			set
			{
				areaBrush = value;
				if (areaBrush != null)
				{
					if (areaBrush.IsFrozen)
						areaBrush = areaBrush.Clone();
					areaBrush.Freeze();
				}
				areaBrushDevice.Brush = null;
			}
		}

		[Browsable(false)]
		public string AreaBrushSerialize
		{
			get => Serialize.BrushToString(AreaBrush);
			set => AreaBrush = Serialize.StringToBrush(value);
		}

		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolAreaOpacity", GroupName = "NinjaScriptGeneral", Order = 1)]
		public int AreaOpacity
		{
			get => areaOpacity;
			set
			{
				areaOpacity = Math.Max(0, Math.Min(100, value));
				areaBrushDevice.Brush = null;
			}
		}

		public override object Icon => Gui.Tools.Icons.DrawTimeCycles;

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolTextOutlineStroke", GroupName = "NinjaScriptGeneral", Order = 2)]
		public Stroke								OutlineStroke	{ get; set; }

		[Browsable(false)]
		public ChartAnchor							StartAnchor		{ get; set; }

		[Browsable(false)]
		public ChartAnchor							EndAnchor		{ get; set; }

		[PropertyEditor("NinjaTrader.Gui.Tools.ChartAnchorTimeEditor")]
		[Display(ResourceType = typeof(NTRes.NinjaTrader.Gui.Chart.ChartResources), GroupName = "GuiChartsCategoryData", Name = "GuiChartsChartAnchorStartTime", Order = 0)]
		public DateTime								StartTime		{ get => StartAnchor.Time; set => StartAnchor.Time = value; }

		[PropertyEditor("NinjaTrader.Gui.Tools.ChartAnchorTimeEditor")]
		[Display(ResourceType = typeof(NTRes.NinjaTrader.Gui.Chart.ChartResources), GroupName = "GuiChartsCategoryData", Name = "GuiChartsChartAnchorEndTime", Order = 1)]
		public DateTime								EndTime			{ get => EndAnchor.Time; set => EndAnchor.Time = value; }

		public override IEnumerable<ChartAnchor>	Anchors => new[] { StartAnchor, EndAnchor };

		public override bool						SupportsAlerts => true;

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (areaBrushDevice != null)
				areaBrushDevice.RenderTarget = null;
		}
		
		public override IEnumerable<AlertConditionItem> GetAlertConditionItems()
		{
			yield return new AlertConditionItem 
			{
				Name					= "Time cycles",
				ShouldOnlyDisplayName	= true
			};
		}

		private ChartBars GetChartBars()
		{
			if (AttachedTo != null)
			{
				ChartBars chartBars = AttachedTo.ChartObject as ChartBars;
				if (chartBars == null && AttachedTo.ChartObject is Gui.NinjaScript.IChartBars iChartBars)
					chartBars = iChartBars.ChartBars;

				return chartBars;
			}

			return null;
		}

		private int GetClosestBarAnchor(ChartControl chartControl, Point p, bool ignoreHitTest)
		{
			if (!ignoreHitTest && p.Y < ChartPanel.Y + ChartPanel.H - cursorSensitivity)
				return int.MinValue;

			int leftX = chartControl.GetXByTime(GetChartBars().GetTimeByBarIdx(chartControl, 0)) - diameter;

			if (anchorBars != null)
				for (int i = 0; i < anchorBars.Count - 1; i++)
					if (anchorBars[i] > leftX && (!ignoreHitTest && anchorBars[i] > p.X - cursorSensitivity && anchorBars[i] < p.X + cursorSensitivity && anchorBars[i] > leftX || ignoreHitTest && i > 0 && anchorBars[i] > p.X && anchorBars[i - 1] < p.X))
						return anchorBars[i];

			return int.MinValue;
		}

		public override Cursor GetCursor(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, Point point)
		{
			if (DrawingState == DrawingState.Building)
				return Cursors.Pen;

			if (DrawingState == DrawingState.Moving)
				return IsLocked ? Cursors.No : Cursors.SizeAll;

			if (DrawingState == DrawingState.Editing)
				return IsLocked ? Cursors.No : Cursors.SizeWE;

			if (GetClosestBarAnchor(chartControl, point, false) != int.MinValue)
				return Cursors.SizeWE;
			if (IsPointOnTimeCyclesOutline(chartControl, chartPanel, point))
				return Cursors.SizeAll;

			return Cursors.Arrow;
		}

		public override IEnumerable<Condition> GetValidAlertConditions()
		{
			return new[] { Condition.CrossInside, Condition.CrossOutside };
		}

		public override Point[] GetSelectionPoints(ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel chartPanel = chartControl.ChartPanels[PanelIndex];
			int leftX = chartControl.GetXByTime(GetChartBars().GetTimeByBarIdx(chartControl, 0)) - diameter;
			List<Point> selectionPoints = new();

			if (anchorBars != null)
				for (int i = 0; i < anchorBars.Count - 1; i++)
					if (anchorBars[i] > leftX)
						selectionPoints.Add(new Point(anchorBars[i], chartPanel.Y + chartPanel.H));

			return selectionPoints.ToArray();
		}

		public override bool IsAlertConditionTrue(AlertConditionItem conditionItem, Condition condition, ChartAlertValue[] values, ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel						chartPanel	= chartControl.ChartPanels[PanelIndex];
			Point GetBarPoint(ChartAlertValue v) => v.ValueType != ChartAlertValueType.StaticValue ? new Point(chartControl.GetXByTime(v.Time), chartScale.GetYByValue(v.Value)) : new Point(0, 0);

			bool Predicate(ChartAlertValue v)
			{
				bool isInside = IsPointInsideTimeCycles(chartPanel, GetBarPoint(v));
				return condition == Condition.CrossInside ? isInside : !isInside;
			}

			return MathHelper.DidPredicateCross(values, Predicate);
		}

		private bool IsPointInsideTimeCycles(ChartPanel chartPanel, Point p)
		{
			if (radius < 0)
				return false;

			for (int i = 0; i < anchorBars.Count - 1; i++)
				if (MathHelper.IsPointInsideEllipse(new Point(anchorBars[i] + radius, chartPanel.Y + chartPanel.H), p, radius, radius))
					return true;

			return false;
		}

		private bool IsPointOnTimeCyclesOutline(ChartControl chartControl, ChartPanel chartPanel, Point p)
		{
			if (radius < 0)
				return false;

			int leftX = chartControl.GetXByTime(GetChartBars().GetTimeByBarIdx(chartControl, 0)) - diameter;

			for (int i = 0; i < anchorBars.Count - 1; i++)
			{
				if (anchorBars[i] < leftX)
					continue;

				// We get the angle from the center of ellipse to point p, then calculate the expected point on the ellipse and compare to that
				double startX	= anchorBars[i] + radius;
				double startY	= chartPanel.Y + chartPanel.H;
				double rad		= Math.Atan2(p.Y - startY, p.X - startX);
				double deg		= rad * (180.0 / Math.PI);
				double t		= Math.Atan(radius * Math.Tan(rad) / radius) + (deg > 90 ? Math.PI : deg < -90 ? 0 - Math.PI : 0);
				double checkX	= startX + radius * Math.Cos(t);
				double checkY	= startY + radius * Math.Sin(t);

				if (p.X < checkX + cursorSensitivity && p.X > checkX - cursorSensitivity && p.Y < checkY + cursorSensitivity && p.Y > checkY - cursorSensitivity)
					return true;
			}

			return false;
		}

		public override bool IsVisibleOnChart(ChartControl chartControl, ChartScale chartScale, DateTime firstTimeOnChart, DateTime lastTimeOnChart)
		{
			return true;
		}

		public override void OnCalculateMinMax()
		{
			MaxValue = double.MinValue;
			MinValue = double.MaxValue;
		}

		public override void OnMouseDown(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			switch (DrawingState)
			{
				case DrawingState.Building:
					if (StartAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(StartAnchor);
						dataPoint.CopyDataValues(EndAnchor);
						StartAnchor.IsEditing	= false;
					}
					else if (EndAnchor.IsEditing)
					{
						dataPoint.CopyDataValues(EndAnchor);
						EndAnchor.IsEditing		= false;
						DrawingState			= DrawingState.Normal;
						IsSelected				= false;
					}
					break;
				case DrawingState.Normal:
					Point p		= dataPoint.GetPoint(chartControl, chartPanel, chartScale);
					Cursor c	= GetCursor(chartControl, chartPanel, chartScale, p);

					if (c == Cursors.SizeWE)
					{
						int editingAnchor	= GetClosestBarAnchor(chartControl, p, false);
						int i				= anchorBars.IndexOf(editingAnchor);

						if (editingAnchor != int.MinValue && i > -1)
						{
							StartAnchor.UpdateXFromPoint(new Point(anchorBars[i == 0 ? i : i - 1], chartPanel.Y + chartPanel.H), chartControl, chartScale);
							EndAnchor.UpdateXFromPoint(new Point(anchorBars[i == 0 ? 1 : i], chartPanel.Y + chartPanel.H), chartControl, chartScale);

							EndAnchor.IsEditing = true;
							DrawingState = DrawingState.Editing;
						}
					}
					else if (c == Cursors.SizeAll)
					{
						int editingAnchor	= GetClosestBarAnchor(chartControl, p, true);
						int i				= anchorBars.IndexOf(editingAnchor);

						if (editingAnchor != int.MinValue && i > -1)
						{
							StartAnchor.UpdateXFromPoint(new Point(anchorBars[i - 1], chartPanel.Y + chartPanel.H), chartControl, chartScale);
							EndAnchor.UpdateXFromPoint(new Point(anchorBars[i], chartPanel.Y + chartPanel.H), chartControl, chartScale);

							// We have to update the InitialMouseDownAnchor here because we moved our Start/End anchors
							InitialMouseDownAnchor = dataPoint.Clone() as ChartAnchor;
							DrawingState = DrawingState.Moving;
						}
					}
					else
						IsSelected = false;
					break;
			}
		}

		public override void OnMouseMove(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			if (IsLocked && DrawingState != DrawingState.Building)
				return;

			switch (DrawingState)
			{
				case DrawingState.Building:
					if (StartAnchor.IsEditing)
						dataPoint.CopyDataValues(StartAnchor);
					if (EndAnchor.IsEditing)
						dataPoint.CopyDataValues(EndAnchor);
					break;
				case DrawingState.Editing:
					dataPoint.CopyDataValues(EndAnchor);
					break;
				case DrawingState.Moving:
					StartAnchor.MoveAnchor(InitialMouseDownAnchor, dataPoint, chartControl, chartPanel, chartScale, this);
					EndAnchor.MoveAnchor(InitialMouseDownAnchor, dataPoint, chartControl, chartPanel, chartScale, this);
					break;
			}
		}

		public override void OnMouseUp(ChartControl chartControl, ChartPanel chartPanel, ChartScale chartScale, ChartAnchor dataPoint)
		{
			if (DrawingState == DrawingState.Building)
				return;

			if (DrawingState == DrawingState.Editing)
				EndAnchor.IsEditing = false;

			DrawingState = DrawingState.Normal;
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			ChartPanel	chartPanel	= chartControl.ChartPanels[PanelIndex];
			int			startX		= Convert.ToInt32(StartAnchor.GetPoint(chartControl, chartPanel, chartScale).X);

			diameter	= Math.Abs(startX - Convert.ToInt32(EndAnchor.GetPoint(chartControl, chartPanel, chartScale).X));
			radius		= Convert.ToInt32(diameter / 2.0);

			if (radius <= 0)
				return;

			UpdateAnchors(chartControl, startX);

			if (anchorBars.Count <= 2)
				return;

			RenderTarget.AntialiasMode	= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			Stroke outlineStroke		= OutlineStroke;
			outlineStroke.RenderTarget	= RenderTarget;

			// dont bother with an area brush if we're doing a hit test (software) render pass. we do not render area then.
			// this allows us to select something behind our area brush (like NT7)
			bool renderArea = false;

			if (!IsInHitTest && AreaBrush != null)
			{
				if (areaBrushDevice.Brush == null)
				{
					Brush brushCopy			= areaBrush.Clone();
					brushCopy.Opacity		= areaOpacity / 100d;
					areaBrushDevice.Brush	= brushCopy;
				}

				areaBrushDevice.RenderTarget	= RenderTarget;
				renderArea						= true;
			}
			else
			{
				areaBrushDevice.RenderTarget	= null;
				areaBrushDevice.Brush			= null;
			}

			for (int i = 0; i < anchorBars.Count - 1; i++)
			{
				SharpDX.Direct2D1.Ellipse ellipse = new(new SharpDX.Vector2(anchorBars[i] + radius, chartPanel.Y + chartPanel.H), radius, radius);

				if (renderArea)
					RenderTarget.FillEllipse(ellipse, areaBrushDevice.BrushDX);

				SharpDX.Direct2D1.Brush tmpBrush = IsInHitTest ? chartControl.SelectionBrush : outlineStroke.BrushDX;
				RenderTarget.DrawEllipse(ellipse, tmpBrush);
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				AreaBrush		= Brushes.CornflowerBlue;
				AreaOpacity		= 40;
				DrawingState	= DrawingState.Building;
				Name			= Custom.Resource.NinjaScriptDrawingToolTimeCycles;
				OutlineStroke	= new Stroke(Brushes.CornflowerBlue, DashStyleHelper.Solid, 2, 100);
				StartAnchor		= new ChartAnchor { DisplayName = Custom.Resource.NinjaScriptDrawingToolAnchorStart, IsEditing = true, DrawingTool = this, IsYPropertyVisible = false };
				EndAnchor		= new ChartAnchor { DisplayName = Custom.Resource.NinjaScriptDrawingToolAnchorEnd, IsEditing = true, DrawingTool = this, IsYPropertyVisible = false };
			}
			else if (State == State.Terminated)
				Dispose();
		}

		private void UpdateAnchors(ChartControl chartControl, int startX)
		{
			List<int> bars = new();
			if (!StartAnchor.IsEditing && diameter > 0)
			{
				int minX = chartControl.GetXByTime(chartControl.FirstTimePainted) - diameter;
				int maxX = chartControl.GetXByTime(chartControl.LastTimePainted) + diameter;

				// Add our start anchor if it is visible
				if (startX <= maxX && startX >= minX)
					bars.Add(startX);

				int leftX = startX;

				while (true)
				{
					leftX -= diameter;

					if (leftX <= maxX && leftX >= minX)
						bars.Add(leftX);

					if (leftX < minX)
					{
						bars.Add(leftX);
						break;
					}
				}

				int count		= bars.Count;
				leftX			= count == 0 ? int.MinValue : bars[bars.Count - 1];
				int rightX		= leftX == int.MinValue ? startX : bars[0];

				while (true)
				{
					rightX += diameter;

					if (rightX <= maxX && rightX >= minX)
						bars.Add(rightX);

					if (rightX > maxX)
					{
						bars.Add(rightX);
						break;
					}
				}
			}
			else
				bars.Add(startX);

			anchorBars = bars.OrderBy(x => x).ToList();
		}
	}

	public static partial class Draw
	{
		private static TimeCycles TimeCyclesCore(NinjaScriptBase owner, string tag, int startBarsAgo, int endBarsAgo,
			DateTime startTime, DateTime endTime, Brush brush, Brush areaBrush, int areaOpacity, bool isGlobal, string templateName)
		{
			if (owner == null)
				throw new ArgumentException("owner");

			if (string.IsNullOrWhiteSpace(tag))
				throw new ArgumentException("tag cant be null or empty");

			if (isGlobal && tag[0] != GlobalDrawingToolManager.GlobalDrawingToolTagPrefix)
				tag = GlobalDrawingToolManager.GlobalDrawingToolTagPrefix + tag;

			if (DrawingTool.GetByTagOrNew(owner, typeof(TimeCycles), tag, templateName) is not TimeCycles drawingTool)
				return null;

			if (startTime < Core.Globals.MinDate)
				throw new ArgumentException($"{drawingTool} startTime must be greater than the minimum Date but was {startTime}");
			if (endTime < Core.Globals.MinDate)
				throw new ArgumentException($"{drawingTool} endTime must be greater than the minimum Date but was {endTime}");

			DrawingTool.SetDrawingToolCommonValues(drawingTool, tag, false, owner, isGlobal);

			// dont overwrite existing anchor references
			ChartAnchor startAnchor		= DrawingTool.CreateChartAnchor(owner, startBarsAgo, startTime, 0);
			ChartAnchor endAnchor		= DrawingTool.CreateChartAnchor(owner, endBarsAgo, endTime, 0);

			startAnchor.CopyDataValues(drawingTool.StartAnchor);
			endAnchor.CopyDataValues(drawingTool.EndAnchor);

			// these can be null when using a templateName so mind not overwriting them
			if (brush != null)
				drawingTool.OutlineStroke = new Stroke(brush, DashStyleHelper.Solid, 2f) { RenderTarget = drawingTool.OutlineStroke.RenderTarget };

			if (areaOpacity >= 0)
				drawingTool.AreaOpacity = areaOpacity;

			if (areaBrush != null)
			{
				drawingTool.AreaBrush = areaBrush.Clone();
				if (drawingTool.AreaBrush.CanFreeze)
					drawingTool.AreaBrush.Freeze();
			}

			drawingTool.SetState(State.Active);
			return drawingTool;
		}

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, int startBarsAgo, int endBarsAgo, Brush brush)
			=> TimeCyclesCore(owner, tag, startBarsAgo, endBarsAgo, Core.Globals.MinDate, Core.Globals.MinDate, brush, Brushes.CornflowerBlue, 40, false, null);

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="areaBrush">The brush used to color the fill region area of the draw object</param>
		/// <param name="areaOpacity"> Sets the level of transparency for the fill color. Valid values between 0 - 100. (0 = completely transparent, 100 = no opacity)</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, int startBarsAgo, int endBarsAgo, Brush brush, Brush areaBrush, int areaOpacity)
			=> TimeCyclesCore(owner, tag, startBarsAgo, endBarsAgo, Core.Globals.MinDate, Core.Globals.MinDate, brush, areaBrush, areaOpacity, false, null);

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, DateTime startTime, DateTime endTime, Brush brush)
			=> TimeCyclesCore(owner, tag, int.MinValue, int.MinValue, startTime, endTime, brush, Brushes.CornflowerBlue, 40, false, null);

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="areaBrush">The brush used to color the fill region area of the draw object</param>
		/// <param name="areaOpacity"> Sets the level of transparency for the fill color. Valid values between 0 - 100. (0 = completely transparent, 100 = no opacity)</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, DateTime startTime, DateTime endTime, Brush brush, Brush areaBrush, int areaOpacity)
			=> TimeCyclesCore(owner, tag, int.MinValue, int.MinValue, startTime, endTime, brush, areaBrush, areaOpacity, false, null);

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, int startBarsAgo, int endBarsAgo, Brush brush, bool drawOnPricePanel) =>
			DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				TimeCyclesCore(owner, tag, startBarsAgo, endBarsAgo, Core.Globals.MinDate, Core.Globals.MinDate, brush, Brushes.CornflowerBlue, 40, false, null));

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="areaBrush">The brush used to color the fill region area of the draw object</param>
		/// <param name="areaOpacity"> Sets the level of transparency for the fill color. Valid values between 0 - 100. (0 = completely transparent, 100 = no opacity)</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, int startBarsAgo, int endBarsAgo, Brush brush, Brush areaBrush, int areaOpacity, bool drawOnPricePanel) =>
			DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				TimeCyclesCore(owner, tag, startBarsAgo, endBarsAgo, Core.Globals.MinDate, Core.Globals.MinDate, brush, areaBrush, areaOpacity, false, null));

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, DateTime startTime, DateTime endTime, Brush brush, bool drawOnPricePanel) =>
			DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				TimeCyclesCore(owner, tag, int.MinValue, int.MinValue, startTime, endTime, brush, Brushes.CornflowerBlue, 40, false, null));

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="brush">The brush used to color draw object</param>
		/// <param name="areaBrush">The brush used to color the fill region area of the draw object</param>
		/// <param name="areaOpacity"> Sets the level of transparency for the fill color. Valid values between 0 - 100. (0 = completely transparent, 100 = no opacity)</param>
		/// <param name="drawOnPricePanel">Determines if the draw-object should be on the price panel or a separate panel</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, DateTime startTime, DateTime endTime, Brush brush, Brush areaBrush, int areaOpacity, bool drawOnPricePanel) =>
			DrawingTool.DrawToggledPricePanel(owner, drawOnPricePanel, () =>
				TimeCyclesCore(owner, tag, int.MinValue, int.MinValue, startTime, endTime, brush, areaBrush, areaOpacity, false, null));

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startBarsAgo">The starting bar (x axis coordinate) where the draw object will be drawn. For example, a value of 10 would paint the draw object 10 bars back.</param>
		/// <param name="endBarsAgo">The end bar (x axis coordinate) where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, int startBarsAgo, int endBarsAgo, bool isGlobal, string templateName)
			=> TimeCyclesCore(owner, tag, startBarsAgo, endBarsAgo, Core.Globals.MinDate, Core.Globals.MinDate, null, null, -1, isGlobal, templateName);

		/// <summary>
		/// Draws time cycles.
		/// </summary>
		/// <param name="owner">The hosting NinjaScript object which is calling the draw method</param>
		/// <param name="tag">A user defined unique id used to reference the draw object</param>
		/// <param name="startTime">The starting time where the draw object will be drawn.</param>
		/// <param name="endTime">The end time where the draw object will terminate</param>
		/// <param name="isGlobal">Determines if the draw object will be global across all charts which match the instrument</param>
		/// <param name="templateName">The name of the drawing tool template the object will use to determine various visual properties</param>
		/// <returns></returns>
		public static TimeCycles TimeCycles(NinjaScriptBase owner, string tag, DateTime startTime, DateTime endTime, bool isGlobal, string templateName)
			=> TimeCyclesCore(owner, tag, int.MinValue, int.MinValue, startTime, endTime, null, null, -1, isGlobal, templateName);
	}
}