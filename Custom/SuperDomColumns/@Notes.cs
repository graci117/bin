// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using NinjaTrader.Gui.SuperDom;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.SuperDomColumns
{
	public class Notes : SuperDomColumn
	{
		private double		columnWidth;
		private double		currentEditingPrice	= -1.0;
		private FontFamily	fontFamily;
		private double		gridHeight;
		private int			gridIndex;
		private Pen			gridPen;
		private double		halfPenWidth;
		private TextBox		tbNotes;
		private Typeface	typeFace;

		#region Mouse Input Handling
		private CommandBinding			displayTextBoxCommandBinding;
		private MouseBinding			doubleClickMouseBinding;

		public static ICommand			DisplayTextBox	= new RoutedCommand("DisplayTextBox", typeof(Notes));
		public static void				DisplayTextBoxExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is not Notes notesCol) return;

			Point mousePos = Mouse.GetPosition(e.Source as IInputElement);

			// Use the mouse position Y coordinate and maths to determine where in the grid of notes cells we are,
			// then update the position of the textbox and display it
			if (notesCol.gridHeight > 0 && notesCol.SuperDom.IsConnected)
			{
				if (notesCol.tbNotes.Visibility == Visibility.Visible)
				{
					// Commit value if the user double clicks away from the text box
					notesCol.SetAndSaveNote();
					notesCol.tbNotes.Text = string.Empty;
				}

				notesCol.gridIndex					= (int)Math.Floor(mousePos.Y / notesCol.SuperDom.ActualRowHeight);
				notesCol.currentEditingPrice		= notesCol.SuperDom.Rows[notesCol.gridIndex].Price;
				
				double	tbOffset					= notesCol.gridIndex * notesCol.SuperDom.ActualRowHeight;
				
				notesCol.tbNotes.Height				= notesCol.SuperDom.ActualRowHeight;
				notesCol.tbNotes.Margin				= new Thickness(0, tbOffset, 0, 0);
				notesCol.tbNotes.Text				= notesCol.PriceStringValues[notesCol.currentEditingPrice];
				notesCol.tbNotes.Width				= notesCol.columnWidth;
				notesCol.tbNotes.Visibility			= Visibility.Visible;
				notesCol.tbNotes.SetValue(Panel.ZIndexProperty, 100);
				notesCol.tbNotes.BringIntoView();
				notesCol.tbNotes.Focus();

				notesCol.OnPropertyChanged();
			}
		}
		#endregion

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptColumnBaseBackground", GroupName = "PropertyCategoryVisual", Order = 110)]
		public Brush BackColor
		{ get; set; }

		[Browsable(false)]
		public string BackBrushSerialize
		{
			get => Gui.Serialize.BrushToString(BackColor, "brushPriceColumnBackground");
			set => BackColor = Gui.Serialize.StringToBrush(value, "brushPriceColumnBackground");
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptColumnBaseForeground", GroupName = "PropertyCategoryVisual", Order = 111)]
		public Brush ForeColor
		{ get; set; }

		[Browsable(false)]
		public string ForeColorSerialize
		{
			get => Gui.Serialize.BrushToString(ForeColor);
			set => ForeColor = Gui.Serialize.StringToBrush(value);
		}

		public override void CopyCustomData(SuperDomColumn newInstance)
		{
			if (newInstance is not Notes newNotes) return;

			newNotes.PriceStringValues = new ConcurrentDictionary<double, string>(PriceStringValues);
		}

		[Browsable(false)]
		public List<string> NotesSerializable { get; set; }

		protected override void OnRender(DrawingContext dc, double renderWidth)
		{
			// This may be true if the UI for a column hasn't been loaded yet (e.g., restoring multiple tabs from workspace won't load each tab until it's clicked by the user)
			if (gridPen == null)
			{
				if (UiWrapper != null && PresentationSource.FromVisual(UiWrapper)?.CompositionTarget is { } target)
				{
					Matrix m			= target.TransformToDevice;
					double dpiFactor	= 1 / m.M11;
					gridPen				= new Pen(Application.Current.TryFindResource("BorderThinBrush") as Brush,  1 * dpiFactor);
					halfPenWidth		= gridPen.Thickness * 0.5;
				}
			}

			if (gridPen == null)
				return;

			columnWidth				= renderWidth;
			gridHeight				= -gridPen.Thickness;
			double verticalOffset	= -gridPen.Thickness;
			double pixelsPerDip		= VisualTreeHelper.GetDpi(UiWrapper).PixelsPerDip;

			// If SuperDom scrolls so that editing price goes off the grid, hide the textbox until the editing price is visible again
			if (SuperDom.IsConnected)
			{
				if (tbNotes.Visibility == Visibility.Visible && SuperDom.Rows.All(r => Math.Abs(r.Price - currentEditingPrice) > 0.000000000000001))
					tbNotes.Visibility = Visibility.Hidden;
				if (tbNotes.Visibility == Visibility.Hidden && SuperDom.Rows.Any(r => Math.Abs(r.Price - currentEditingPrice) < 0.000000000000001))
					tbNotes.Visibility = Visibility.Visible;
			}

			lock (SuperDom.Rows)
				foreach (PriceRow row in SuperDom.Rows)
				{
					// Add new prices if needed to the dictionary as the user scrolls
					PriceStringValues.AddOrUpdate(row.Price, string.Empty, (_, oldValue) => oldValue);
					// If textbox is open, move it when the SuperDom scrolls
					if (tbNotes.Visibility == Visibility.Visible && Math.Abs(row.Price - currentEditingPrice) < 0.000000000000001)
					{
						if (SuperDom.Rows.IndexOf(row) != gridIndex)
						{
							gridIndex			= SuperDom.Rows.IndexOf(row);
							double tbOffset		= gridIndex * SuperDom.ActualRowHeight;
							tbNotes.Margin		= new Thickness(0, tbOffset, 0, 0);
						}
					}

					// Draw cell
					if (renderWidth - halfPenWidth >= 0)
					{
						Rect rect = new(-halfPenWidth, verticalOffset, renderWidth - halfPenWidth, SuperDom.ActualRowHeight);

						// Create a guidelines set
						GuidelineSet guidelines = new();
						guidelines.GuidelinesX.Add(rect.Left	+ halfPenWidth);
						guidelines.GuidelinesX.Add(rect.Right	+ halfPenWidth);
						guidelines.GuidelinesY.Add(rect.Top		+ halfPenWidth);
						guidelines.GuidelinesY.Add(rect.Bottom	+ halfPenWidth);

						dc.PushGuidelineSet(guidelines);
						dc.DrawRectangle(BackColor, null, rect);
						dc.DrawLine(gridPen, new Point(-gridPen.Thickness, rect.Bottom), new Point(renderWidth - halfPenWidth, rect.Bottom));
						dc.DrawLine(gridPen, new Point(rect.Right, verticalOffset), new Point(rect.Right, rect.Bottom));
						// Print note value - remember to set MaxTextWidth so text doesn't spill into another column
						if (PriceStringValues.TryGetValue(row.Price, out string note) && !string.IsNullOrEmpty(PriceStringValues[row.Price]))
						{
							fontFamily				= SuperDom.Font.Family;
							typeFace				= new Typeface(fontFamily, SuperDom.Font.Italic ? FontStyles.Italic : FontStyles.Normal, SuperDom.Font.Bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);

							if (renderWidth - 6 > 0)
							{
								FormattedText noteText = new(note, Core.Globals.GeneralOptions.CurrentCulture, FlowDirection.LeftToRight, typeFace, SuperDom.Font.Size, ForeColor, pixelsPerDip) { MaxLineCount = 1, MaxTextWidth = renderWidth - 6, Trimming = TextTrimming.CharacterEllipsis };
								dc.DrawText(noteText, new Point(0 + 4, verticalOffset + (SuperDom.ActualRowHeight - noteText.Height) / 2));
							}
						}

						dc.Pop();
						verticalOffset	+= SuperDom.ActualRowHeight;
						gridHeight		+= SuperDom.ActualRowHeight;
					}
				}
		}

		public override void OnRestoreValues()
		{
			bool			restored		= false;

			if (NotesSerializable != null)
				foreach (string note in NotesSerializable)
				{
					string[]	noteVal		= note.Split(';');
					if (double.TryParse(noteVal[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double price))
					{
						PriceStringValues.AddOrUpdate(price, noteVal[1], (_, _) => noteVal[1]);
						restored = true;
					}
				}

			if (restored) OnPropertyChanged();
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name							= Custom.Resource.NinjaScriptSuperDomColumnNotes;
				Description						= Custom.Resource.NinjaScriptSuperDomColumnDescriptionNotes;
				DefaultWidth					= 160;
				PreviousWidth					= -1;
				IsDataSeriesRequired			= false;
				BackColor						= Application.Current.TryFindResource("brushPriceColumnBackground") as Brush;
				ForeColor						= Application.Current.TryFindResource("FontControlBrush") as Brush;

				NotesSerializable				= new List<string>();
				PriceStringValues				= new ConcurrentDictionary<double, string>();
			}
			else if (State == State.Configure)
			{
				if (UiWrapper != null && PresentationSource.FromVisual(UiWrapper)?.CompositionTarget is { } target)
				{
					Matrix m					= target.TransformToDevice;
					double dpiFactor			= 1 / m.M11;
					gridPen						= new Pen(Application.Current.TryFindResource("BorderThinBrush") as Brush,  1 * dpiFactor);
					halfPenWidth				= gridPen.Thickness * 0.5;
				}

				tbNotes	= new TextBox	{
											Margin				= new Thickness(0), 
											VerticalAlignment	= VerticalAlignment.Top, 
											Visibility			= Visibility.Hidden
										};

				SetBindings();

				tbNotes.LostKeyboardFocus += (_, _) =>
					{
						if (Math.Abs(currentEditingPrice - -1.0) > 0.000000000000001 && tbNotes.Visibility == Visibility.Visible)
						{
							SetAndSaveNote();
	
							tbNotes.Text		= string.Empty;
							currentEditingPrice = -1.0;
							tbNotes.Visibility	= Visibility.Hidden;
							OnPropertyChanged();
						}
					};

				tbNotes.KeyDown += (_, args) =>
					{
						if (args.Key is Key.Enter or Key.Tab)
						{
							SetAndSaveNote();

							tbNotes.Text		= string.Empty;
							currentEditingPrice	= -1.0;
							tbNotes.Visibility	= Visibility.Hidden;
							OnPropertyChanged();
						}
						else if (args.Key == Key.Escape)
						{
							currentEditingPrice	= -1.0;
							tbNotes.Visibility	= Visibility.Hidden;
							OnPropertyChanged();
						}
					};
			}
			else if (State == State.Active)
			{
				foreach (PriceRow row in SuperDom.Rows)
					PriceStringValues.AddOrUpdate(row.Price, string.Empty, (_, oldValue) => oldValue);
			}
			else if (State == State.Terminated)
			{
				if (UiWrapper != null)
				{
					UiWrapper.Children.Remove(tbNotes);
					UiWrapper.InputBindings.Remove(doubleClickMouseBinding);
					UiWrapper.CommandBindings.Remove(displayTextBoxCommandBinding);
				}
			}
		}

		[XmlIgnore]
		[Browsable(false)]
		public ConcurrentDictionary<double, string> PriceStringValues { get; set; }

		public override void SetBindings()
		{
			//Use InputBindings to handle mouse interactions
			//	MouseAction.LeftClick
			//	MouseAction.LeftDoubleClick
			//	MouseAction.MiddleClick
			//	MouseAction.MiddleDoubleClick
			//	MouseAction.None
			//	MouseAction.RightClick
			//	MouseAction.RightDoubleClick
			//	MouseAction.WheelClick
			doubleClickMouseBinding			= new MouseBinding(DisplayTextBox, new MouseGesture(MouseAction.LeftDoubleClick)) { CommandParameter = this };
			displayTextBoxCommandBinding	= new CommandBinding(DisplayTextBox, DisplayTextBoxExecuted);

			if (UiWrapper != null)
			{
				UiWrapper.InputBindings.Add(doubleClickMouseBinding);
				UiWrapper.CommandBindings.Add(displayTextBoxCommandBinding);
				UiWrapper.Children.Add(tbNotes);
			}
		}

		private void SetAndSaveNote()
		{
			string updatedValue = PriceStringValues.AddOrUpdate(currentEditingPrice, tbNotes.Text, (_, _) => tbNotes.Text);
			lock (NotesSerializable)
			{
				if (NotesSerializable.Any(n => n.StartsWith(currentEditingPrice.ToString("N2", CultureInfo.InvariantCulture))))
				{
					int index = NotesSerializable.IndexOf(NotesSerializable.SingleOrDefault(n => n.StartsWith(currentEditingPrice.ToString("N2", CultureInfo.InvariantCulture))));
					NotesSerializable[index] = $"{currentEditingPrice.ToString("N2", CultureInfo.InvariantCulture)};{updatedValue}";
				}
				else
					NotesSerializable.Add($"{currentEditingPrice.ToString("N2", CultureInfo.InvariantCulture)};{tbNotes.Text}");
			}
		}
	}
}