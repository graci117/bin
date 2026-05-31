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

	public enum DrawMarkerSelection
	{
		Triangle,
    	Arrow,
    	Dot,
		Diamond,
	}	
	
//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
		[Gui.CategoryOrder("Plot Options", 1)] 
		[Gui.CategoryOrder("Direction Change", 3)] 
		[Gui.CategoryOrder("Actions", 4)] 

	[TypeConverter("NinjaTrader.NinjaScript.Indicators.ColorThePlotConverter")]

	
	// Change Log:
	// 11-20-2020 V1.0 - Release
	// 11-01-2021 V1.1 - Added Options to limit number of draw markers for general NS performance inmprovemnt, changed ChartPanel check in case Chart bars are not panel 0
	
	
	public class ColorThePlot : Indicator
	{

		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		private int marker_Count	= 0;		// added v1.1
		private bool OffsetPercent = false;
		private bool UpDir;
		private Brush hollowBar = Brushes.Transparent;
		private string IndiName = "";
		private bool addPanel = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Use the input series to select the indicator to color.";
				Name										= "_ColorThePlot";
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
				ShowTransparentPlotsInDataBox 				= true;
				AddPlot(Brushes.CornflowerBlue, "CTPplot");
				AddPlot(Brushes.Transparent, 	"Signal");     	// signal change of directiopn (for duration of bar)
				AddPlot(Brushes.Transparent, "Direction");		// provides +1 while in up direction or -1 while in down (Flat is part of direction until changed)
				
				#region SetDefaults	
				
				DisplayIndicatorName 						= true;
				SlopePeriod									= 3;
				
				SoundsOn									= false;				
				UpSoundFile									= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert2.Wav";
				DownSoundFile 								= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert3.Wav";	

				DrawMarker									= false;
				Number_Of_Markers							= 50;							// added v1.1
				DrawType									= DrawMarkerSelection.Arrow;				
				Offset										= 3;
				MarkerUpColor								= Brushes.Green;
				MarkerDownColor								= Brushes.Red;				
				
				AlertsOn									= false;
				ReArmTime									= 30;	
				
				Email										= false;
				EmailTo										= @"";
				EmailBody									= @"";
								
				ColorBars									= false;		
				BarColorUp									= Brushes.CornflowerBlue;
				BarColorDown								= Brushes.Orange;	
				
				ColorBarOutline								= false;				
				OutlineColorUp								= Brushes.CornflowerBlue;
				OutlineColorDown							= Brushes.Orange;	
				
				PanelColorUp								= Brushes.LightGreen;
				PanelColorDown								= Brushes.Tomato;
				BackgroundOpacity							= 20;				
				
				MA0LineWidth								= 2;
				MA0PlotStyle								= PlotStyle.Line;
				MA0DashStyle								= DashStyleHelper.Solid;				
			
				MA0RisingColor								= Brushes.Green;
				MA0FlatColor								= Brushes.Gold;
				MA0FallingColor								= Brushes.Red;			
				#endregion
			}
			else if (State == State.Configure)
			{
				#region Configure
				
				if (Input is Indicator)
				{
					if ((Input as Indicator).IsOverlay == false)
					{
						addPanel = true;  // cannot dynamically add a panel so have to ask user to add this in the UI, this bool lets us know.
						IsOverlay = false;
					}
				}
				
				Plots[0].Width 				= MA0LineWidth;
				Plots[0].PlotStyle			= MA0PlotStyle;
				Plots[0].DashStyleHelper	= MA0DashStyle;


				if (ColorBackground || ColorBackgroundAll)
				{
					Brush temp = PanelColorUp.Clone();
					temp.Opacity = BackgroundOpacity / 100.0;
					temp.Freeze();
					PanelColorUp = temp;
					
					Brush temp1 = PanelColorDown.Clone();
					temp1.Opacity = BackgroundOpacity / 100.0;
					temp1.Freeze();
					PanelColorDown = temp1;	
				}	
				#endregion
			}
		}
		
		public override string DisplayName
		{
    			get { if  (State == State.SetDefaults) 
						return Name; 	
					else  if (DisplayIndicatorName)
						return Name;
					else return "";  }	
		}			

		protected override void OnBarUpdate()
		{			
			if  (CurrentBar == 0 && ChartControl != null ) // do once on  chart only
			{
				if (Input is Indicator)
				{
					Name = "CTP+"+Input.ToString(); // To show on chart "CTP+indicator" used
				}
				
				
				
				if (ChartPanel.PanelIndex != 0)  // support for placing indicator into indicator panel 
				{
					DrawOnPricePanel = false;  // move draw objects to indicator panel
					OffsetPercent	= true;
				}
				
				if (ChartPanel.PanelIndex == ChartBars.Panel && addPanel) // if user has not select seperate panel when indicator needs its own panel.
				{
					Draw.TextFixed(this, "test", " Please re-open the ColotThePlot indicator, locate the 'Panel' and select 'New Panel', thank-you.", TextPosition.Center);
				}
	
			}			
							
			CTPplot[0] = Input[0];  			// bring the user selected plot into this indicator for output			
			Signal[0] 	= 0;					// Reset the cross detection on each run
			
			if (CurrentBar < SlopePeriod) 
				return;  						// wait for enough bars to check the slope
			
			if (Slope(CTPplot, SlopePeriod, 0) > 0.0) 
			{
				if (!UpDir)
				{
					Signal[0] = 1; 					// set signal to plus 1 for up direction, once only per change of direction.
					UpDir = true;
					savedUBar = CurrentBar;			// once per bar only
					DoActions();					// process action the first time through indicating change
				}
				
				PlotBrushes[0][0] = MA0RisingColor;
			}		
			else if (Slope(CTPplot, SlopePeriod, 0) < 0.0) 
			{
				if (UpDir)
				{
					Signal[0] = -1; 				// set signal to minus 1 for up direction, once only, per change of direction.					
					UpDir = false;
					savedDBar = CurrentBar;			// once per bar only
					DoActions();					// process action the first time through ind icating change
				}
				
				PlotBrushes[0][0] = MA0FallingColor;
			}	
			
			else	
			{											// if flat process color but do not change direction
				PlotBrushes[0][0] = MA0FlatColor;
			}
			
			if (ColorBackground)
			{
				if (ColorBackgroundAll)
				{
					BackBrushAll = UpDir ? PanelColorUp :  PanelColorDown;
				}
				else
				{
					BackBrush = UpDir ? PanelColorUp :  PanelColorDown;
				}
			}			
				
			if (ColorBars)
			{
				BarBrush = UpDir ? (Close[0] > Open[0] ? hollowBar : BarColorUp) : Close[0] < Open[0] ? BarColorDown : hollowBar;  // down is solid in all cases
			}	
				
			if (ColorBarOutline)
			{
				CandleOutlineBrush = UpDir ? OutlineColorUp : OutlineColorDown;
			}	
			
			Direction[0] = UpDir ? 1 : -1;  // set the direction plot, we are either rising or falling, flat is included with either.
		} // end of OnBarUpdate()
		
		
		#region DoActions
		
		private void DoActions()
		{			
			if (AlertsOn)
			{
					Alert("DirectionChange", Priority.Low,(Signal[0] == 1 ? "Change to up direction detected on " : "Change to down direction detected on ")
					+ Instrument.MasterInstrument.Name+ " "+BarsPeriod.Value+" "+BarsPeriod.BarsPeriodType, "", ReArmTime, 
					(Signal[0] ==1 ? Brushes.LightGreen : Brushes.Tomato), Brushes.Black);
			}
						
			if (DrawMarker)
			{
				marker_Count++;			// Increment the number of draw objects couter (not used if Number_Of_Markers = 99) v1.1
				
				if (Number_Of_Markers != 99 && Number_Of_Markers <= marker_Count)  //Reset the counter when over the limit  v1.1
					marker_Count = 1;
				
				switch (DrawType)
				{
					case DrawMarkerSelection.Arrow:
					{
						if (Signal[0] == 1)
						{
							Draw.ArrowUp (this, "UpArrow"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize : Input[0]), MarkerUpColor);
						}
						else
						{
							Draw.ArrowDown (this, "DwnArrow"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize : Input[0]), MarkerDownColor);
						}
						break;						
					}
						
					case DrawMarkerSelection.Triangle:
					{
						if (Signal[0] == 1)
						{
							Draw.TriangleUp (this, "UpTriangle"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize: Input[0]), MarkerUpColor);
						}
						else
						{
							Draw.TriangleDown (this, "DwnArrow"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize: Input[0]), MarkerDownColor);
						}							
						break;
					}
						
					case DrawMarkerSelection.Dot:
					{
						if (Signal[0] == 1)
						{
							Draw.Dot (this, "UpDot"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize : Input[0]), MarkerUpColor);
						}
						else
						{
							Draw.Dot (this, "DwnDot"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize: Input[0]), MarkerDownColor);
						}
						break;
					}
						
					case DrawMarkerSelection.Diamond:
					{
						if (Signal[0] == 1)
						{
							Draw.Diamond (this, "UpDiamond"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? Low[0] - Offset * TickSize : Input[0]), MarkerUpColor);
						}
						else
						{
							Draw.Diamond (this, "DwnDiamond"+(Number_Of_Markers == 99 ? CurrentBar : marker_Count), true, 0, (DrawOnPricePanel ? High[0] + Offset * TickSize : Input[0]) , MarkerDownColor);
						}
						break;
					}
				}
			}

			
			if (Email)
			{
				if (Signal[0] == 1 && (savedUBar - CurrentBar == 0))
				{
					SendMail (EmailTo, "Direction change to up on "+Instrument.FullName+" "+BarsPeriod.Value+" "+BarsPeriod.BarsPeriodType, EmailBody);
				}
				else if (Signal[0] == -1 && (savedDBar - CurrentBar == 0))
				{
					SendMail (EmailTo, "Direction change to down on "+Instrument.FullName+" "+BarsPeriod.Value+" "+BarsPeriod.BarsPeriodType, EmailBody);
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
			#endregion
		}
		
				
		
		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CTPplot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Direction
		{
			get { return Values[2]; }
		}		
		
		#endregion
		[NinjaScriptProperty]
		[Display(Name="Show Indicator Name", Description="Draw a symbol to mark change in direction", Order=1, GroupName="Display")]
		public bool DisplayIndicatorName
		{ get; set; }		
		
		#region Plot choices
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Rising Color", Description="Color when rising", Order=10, GroupName="Plot Options")]
		public Brush MA0RisingColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA0RisingColorSerializable
		{
			get { return Serialize.BrushToString(MA0RisingColor); }
			set { MA0RisingColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Flat Color", Description="Color when flat", Order=11, GroupName="Plot Options")]
		public Brush MA0FlatColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA0FlatColorSerializable
		{
			get { return Serialize.BrushToString(MA0FlatColor); }
			set { MA0FlatColor = Serialize.StringToBrush(value); }
		}	
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Falling Color", Description="Color when falling", Order=12, GroupName="Plot Options")]
		public Brush MA0FallingColor
		{ get; set; }
		
		[Browsable(false)]
		public string MA0FallingColorSerializable
		{
			get { return Serialize.BrushToString(MA0FallingColor); }
			set { MA0FallingColor = Serialize.StringToBrush(value); }
		}
		[NinjaScriptProperty]
		[Range(1, 19)]
		[Display(Name="Plot Line width", Description="Set thickness of plot line", Order=4, GroupName="Plot Options")]
		public int MA0LineWidth
		{ get; set; }	
		[NinjaScriptProperty]
		[Display(Name="Plot Line PlotStyle", Description="Set type of line/cross/bar/square/etc", Order=5, GroupName="Plot Options")]
		public PlotStyle MA0PlotStyle
		{ get; set; }			
		[NinjaScriptProperty]
		[Display(Name="Plot Line DashStyle", Description="Set dash style of line", Order=6, GroupName="Plot Options")]
		public DashStyleHelper MA0DashStyle
		{ get; set; }	
		
		
		#endregion
		
		#region Detection Settings
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slope period", Description="Number of bars for slope check", Order=3, GroupName="Parameters")]
		public int SlopePeriod
		{ get; set; }		
		
		#endregion
		
		#region Actions
		
		[Display(Name="Sound on direction change", Description="Play sounds on diection change", Order=1, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool SoundsOn
		{ get; set; }			
		
		[Display(Name="Up direction sound", Description="Enter Up sound file path/name", Order=2, GroupName="Actions")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter = "Wav Files (*.wav)|*.wav")]
		[RefreshProperties(RefreshProperties.All)]
		public string UpSoundFile
		{ get; set; }

		[Display(Name="Down direction sound", Description="Enter Down sound file path/name", Order=3, GroupName="Actions")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		[RefreshProperties(RefreshProperties.All)]
		public string DownSoundFile
		{ get; set; }
	
		[Display(Name="Alerts to Alert Panel", Description="Send alert messages to alerts panel", Order=10, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool AlertsOn
		{ get; set; }	
		
		[Range(1, 1000)]
		[Display(Name="Alert re-arm time in seconds", Description="Rearm time in seconds, if alarm condition remains on it will resend alert", Order=11, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public int ReArmTime
		{ get; set; }
		
		[Display(Name="Send Email", Description="Send Email when cross detected (must have share service pre set)", Order=16, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool Email
		{ get; set; }	
		
		[Display(Name="Email To:", Description="Destination complete e-mail address ", Order=17, GroupName="Actions")]
		public string EmailTo
		{ get; set; }
		
		[Display(Name="Email Body:", Description="Text to display in the body of the e-mail", Order=19, GroupName="Actions")]
		public string EmailBody
		{ get; set; }			
		
		[Display(Name="Mark direction change", Description="Draw a symbol to mark change in direction", Order=20, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool DrawMarker
		{ get; set; }	
		
		//added v1.1
		[Display(Name="Show how many markers?", Description="specify the number of drawing markers to show, 99 = show all", Order=21, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public int Number_Of_Markers
		{ get; set; }			
				
		[Display(Name="Marker to use", Description="Choose a marker to show at each direction change", Order=22, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public DrawMarkerSelection DrawType
		{ get; set; }		
		
		[Display(Name="Marker offset (Ticks)", Description="Ticks above or below price high or low to display selected marker", Order=23, GroupName="Actions")]
		public int Offset
		{ get; set; }			


		[XmlIgnore]
		[Display(Name="Up Marker color", Description="Color of marker to show ", Order=24, GroupName="Actions")]
		public Brush MarkerUpColor
		{ get; set; }
		
		[Browsable(false)]
		public string MarkerUpColorSerializable
		{
			get { return Serialize.BrushToString(MarkerUpColor); }
			set { MarkerUpColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Down Marker color", Description="Color of marker to show direction change to down", Order=25, GroupName="Actions")]
		public Brush MarkerDownColor
		{ get; set; }
		
		[Browsable(false)]
		public string MarkerDowColorSerializable
		{
			get { return Serialize.BrushToString(MarkerDownColor); }
			set { MarkerDownColor = Serialize.StringToBrush(value); }
		}
		
		[Display(Name="Color Panel Background", Description="Color the panel background when direction changes", Order=30, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBackground
		{ get; set; }		

		[Range(1, 99)]
		[Display(Name=" % Opacity of background", Description="Sets the amount of opacity of background colors ", Order=31, GroupName="Actions")]
		public int BackgroundOpacity
		{ get; set; }			

		[XmlIgnore]
		[Display(Name="Up Panel Color ", Description="Panel background color for up direction", Order=32, GroupName="Actions")]
		public Brush PanelColorUp
		{ get; set; }

		[Browsable(false)]
		public string PanelColorUpSerializable
		{
			get { return Serialize.BrushToString(PanelColorUp); }
			set { PanelColorUp = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Down Panel Color ", Description="Panel background color for down direction", Order=33, GroupName="Actions")]
		public Brush PanelColorDown
		{ get; set; }

		[Browsable(false)]
		public string PanelColorDownSerializable
		{
			get { return Serialize.BrushToString(PanelColorDown); }
			set { PanelColorDown = Serialize.StringToBrush(value); }
		}			

		[Display(Name="Extend Background to all Panels", Description="Extend background coloring all Panel on chart", Order=34, GroupName="Actions")]
		public bool ColorBackgroundAll
		{ get; set; }	
		
		[Display(Name="Color Price Bars", Description="Color the bars according to direction", Order=40, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBars
		{ get; set; }			
		
		[XmlIgnore]
		[Display(Name="Up direction Bar Color", Description="Price bar color when indicator is up", Order=41, GroupName="Actions")]
		public Brush BarColorUp
		{ get; set; }

		[Browsable(false)]
		public string BarColorUpSerializable
		{
			get { return Serialize.BrushToString(BarColorUp); }
			set { BarColorUp = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Down direction Bar Color", Description="Price bar color when indicator is down", Order=42, GroupName="Actions")]
		public Brush BarColorDown
		{ get; set; }

		[Browsable(false)]
		public string BarColorDownSerializable
		{
			get { return Serialize.BrushToString(BarColorDown); }
			set { BarColorDown = Serialize.StringToBrush(value); }
		}	
		
		[Display(Name="Color Price Bar Outline", Description="Color the bar outline when direction changes", Order=50, GroupName="Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBarOutline
		{ get; set; }			
		
		[XmlIgnore]
		[Display(Name="Up direction Bar outline Color", Description="Price bar outline color for indicator up direction", Order=51, GroupName="Actions")]
		public Brush OutlineColorUp
		{ get; set; }

		[Browsable(false)]
		public string OutlineColorUpSerializable
		{
			get { return Serialize.BrushToString(OutlineColorUp); }
			set { OutlineColorUp = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Down direction Bar outline Color", Description="Price bar outline color for indicator down direction", Order=52, GroupName="Actions")]
		public Brush OutlineColorDown
		{ get; set; }

		[Browsable(false)]
		public string OutlineColorDownSerializable
		{
			get { return Serialize.BrushToString(OutlineColorDown); }
			set { OutlineColorDown = Serialize.StringToBrush(value); }
		}		
		#endregion
		

			
	}
	#region ConverterStuff
	
	public class ColorThePlotConverter : IndicatorBaseConverter 
	{
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			ColorThePlot indicator = component as ColorThePlot;

			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context)
				? base.GetProperties(context, component, attrs) : TypeDescriptor.GetProperties(component, attrs);

			if (indicator == null || propertyDescriptorCollection == null)
				return propertyDescriptorCollection;
			

			PropertyDescriptor MA0RisingColor 		= propertyDescriptorCollection["MA0RisingColor"];
			PropertyDescriptor MA0FlatColor 		= propertyDescriptorCollection["MA0FlatColor"];
			PropertyDescriptor MA0FallingColor 		= propertyDescriptorCollection["MA0FallingColor"];
			
			PropertyDescriptor DrawMarker			= propertyDescriptorCollection["DrawMarker"];
			PropertyDescriptor Number_Of_Markers	= propertyDescriptorCollection["Number_Of_Markers"];
			PropertyDescriptor Offset				= propertyDescriptorCollection["Offset"];
			PropertyDescriptor MarkerUpColor		= propertyDescriptorCollection["MarkerUpColor"];
			PropertyDescriptor MarkerDownColor		= propertyDescriptorCollection["MarkerDownColor"];
			PropertyDescriptor DrawType				= propertyDescriptorCollection["DrawType"];			
			
			PropertyDescriptor ColorBars			= propertyDescriptorCollection["ColorBars"];			
			PropertyDescriptor BarColorUp	= propertyDescriptorCollection["BarColorUp"];
			PropertyDescriptor BarColorDown	= propertyDescriptorCollection["BarColorDown"];			

			PropertyDescriptor ColorBarOutline		= propertyDescriptorCollection["ColorBarOutline"];			
			PropertyDescriptor OutlineColorUp=propertyDescriptorCollection["OutlineColorUp"];
			PropertyDescriptor OutlineColorDown=propertyDescriptorCollection["OutlineColorDown"];			

			PropertyDescriptor ColorBackground		= propertyDescriptorCollection["ColorBackground"];			
			PropertyDescriptor PanelColorUp	= propertyDescriptorCollection["PanelColorUp"];
			PropertyDescriptor PanelColorDown	= propertyDescriptorCollection["PanelColorDown"];
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
			
				
		// remove removable properties first
			
			propertyDescriptorCollection.Remove(UpSoundFile);
			propertyDescriptorCollection.Remove(DownSoundFile);		
			
			propertyDescriptorCollection.Remove(ReArmTime);				
			
			propertyDescriptorCollection.Remove(EmailTo);	
			propertyDescriptorCollection.Remove(EmailBody);	
			
			propertyDescriptorCollection.Remove(Number_Of_Markers);
			propertyDescriptorCollection.Remove(DrawType);
			propertyDescriptorCollection.Remove(Offset);
			propertyDescriptorCollection.Remove(MarkerUpColor);
			propertyDescriptorCollection.Remove(MarkerDownColor);
			
			propertyDescriptorCollection.Remove(PanelColorUp);
			propertyDescriptorCollection.Remove(PanelColorDown);
			propertyDescriptorCollection.Remove(ColorBackgroundAll);
			propertyDescriptorCollection.Remove(BackgroundOpacity);	
			
			propertyDescriptorCollection.Remove(BarColorUp);
			propertyDescriptorCollection.Remove(BarColorDown);			
			
			propertyDescriptorCollection.Remove(OutlineColorUp);
			propertyDescriptorCollection.Remove(OutlineColorDown);	
									
			// Add backj in if...
						
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
				propertyDescriptorCollection.Add(Number_Of_Markers);
				propertyDescriptorCollection.Add(DrawType);
				propertyDescriptorCollection.Add(Offset);
				propertyDescriptorCollection.Add(MarkerUpColor);
				propertyDescriptorCollection.Add(MarkerDownColor);				
			}

			if (indicator.ColorBackground)
			{
				propertyDescriptorCollection.Add(PanelColorUp);
				propertyDescriptorCollection.Add(PanelColorDown);
				propertyDescriptorCollection.Add(ColorBackgroundAll);
				propertyDescriptorCollection.Add(BackgroundOpacity);				
			}
		
			if (indicator.ColorBars)
			{
				propertyDescriptorCollection.Add(BarColorUp);
				propertyDescriptorCollection.Add(BarColorDown);								
			}
			
			if (indicator.ColorBarOutline)
			{
				propertyDescriptorCollection.Add(OutlineColorUp);
				propertyDescriptorCollection.Add(OutlineColorDown);								
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
		private ColorThePlot[] cacheColorThePlot;
		public ColorThePlot ColorThePlot(bool displayIndicatorName, Brush mA0RisingColor, Brush mA0FlatColor, Brush mA0FallingColor, int mA0LineWidth, PlotStyle mA0PlotStyle, DashStyleHelper mA0DashStyle, int slopePeriod)
		{
			return ColorThePlot(Input, displayIndicatorName, mA0RisingColor, mA0FlatColor, mA0FallingColor, mA0LineWidth, mA0PlotStyle, mA0DashStyle, slopePeriod);
		}

		public ColorThePlot ColorThePlot(ISeries<double> input, bool displayIndicatorName, Brush mA0RisingColor, Brush mA0FlatColor, Brush mA0FallingColor, int mA0LineWidth, PlotStyle mA0PlotStyle, DashStyleHelper mA0DashStyle, int slopePeriod)
		{
			if (cacheColorThePlot != null)
				for (int idx = 0; idx < cacheColorThePlot.Length; idx++)
					if (cacheColorThePlot[idx] != null && cacheColorThePlot[idx].DisplayIndicatorName == displayIndicatorName && cacheColorThePlot[idx].MA0RisingColor == mA0RisingColor && cacheColorThePlot[idx].MA0FlatColor == mA0FlatColor && cacheColorThePlot[idx].MA0FallingColor == mA0FallingColor && cacheColorThePlot[idx].MA0LineWidth == mA0LineWidth && cacheColorThePlot[idx].MA0PlotStyle == mA0PlotStyle && cacheColorThePlot[idx].MA0DashStyle == mA0DashStyle && cacheColorThePlot[idx].SlopePeriod == slopePeriod && cacheColorThePlot[idx].EqualsInput(input))
						return cacheColorThePlot[idx];
			return CacheIndicator<ColorThePlot>(new ColorThePlot(){ DisplayIndicatorName = displayIndicatorName, MA0RisingColor = mA0RisingColor, MA0FlatColor = mA0FlatColor, MA0FallingColor = mA0FallingColor, MA0LineWidth = mA0LineWidth, MA0PlotStyle = mA0PlotStyle, MA0DashStyle = mA0DashStyle, SlopePeriod = slopePeriod }, input, ref cacheColorThePlot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ColorThePlot ColorThePlot(bool displayIndicatorName, Brush mA0RisingColor, Brush mA0FlatColor, Brush mA0FallingColor, int mA0LineWidth, PlotStyle mA0PlotStyle, DashStyleHelper mA0DashStyle, int slopePeriod)
		{
			return indicator.ColorThePlot(Input, displayIndicatorName, mA0RisingColor, mA0FlatColor, mA0FallingColor, mA0LineWidth, mA0PlotStyle, mA0DashStyle, slopePeriod);
		}

		public Indicators.ColorThePlot ColorThePlot(ISeries<double> input , bool displayIndicatorName, Brush mA0RisingColor, Brush mA0FlatColor, Brush mA0FallingColor, int mA0LineWidth, PlotStyle mA0PlotStyle, DashStyleHelper mA0DashStyle, int slopePeriod)
		{
			return indicator.ColorThePlot(input, displayIndicatorName, mA0RisingColor, mA0FlatColor, mA0FallingColor, mA0LineWidth, mA0PlotStyle, mA0DashStyle, slopePeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ColorThePlot ColorThePlot(bool displayIndicatorName, Brush mA0RisingColor, Brush mA0FlatColor, Brush mA0FallingColor, int mA0LineWidth, PlotStyle mA0PlotStyle, DashStyleHelper mA0DashStyle, int slopePeriod)
		{
			return indicator.ColorThePlot(Input, displayIndicatorName, mA0RisingColor, mA0FlatColor, mA0FallingColor, mA0LineWidth, mA0PlotStyle, mA0DashStyle, slopePeriod);
		}

		public Indicators.ColorThePlot ColorThePlot(ISeries<double> input , bool displayIndicatorName, Brush mA0RisingColor, Brush mA0FlatColor, Brush mA0FallingColor, int mA0LineWidth, PlotStyle mA0PlotStyle, DashStyleHelper mA0DashStyle, int slopePeriod)
		{
			return indicator.ColorThePlot(input, displayIndicatorName, mA0RisingColor, mA0FlatColor, mA0FallingColor, mA0LineWidth, mA0PlotStyle, mA0DashStyle, slopePeriod);
		}
	}
}

#endregion
