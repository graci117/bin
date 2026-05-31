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
using System.Reflection;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.MA5
{
	public class RectangleExtender : Indicator
	{
        private NinjaTrader.Gui.Chart.Chart	chartWindow;
	    private new System.Windows.Controls.Button Expansion;	
		private bool IsToolBarButtonAdded;
		
		private TimeSpan timeEndS;
		private DateTime startTime;
        private DateTime endTime;
		private TimeSpan TimeEnd;
		
        private bool showAreaColor;
		private Brush rectColor;    

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Extend the selected rectangle to the current day, and to the Future.";
				Name										= "Rectangle Extender";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				startTime                                   = DateTime.Now;
                endTime                                     = startTime.Add(TimeEnd);
				
                TimeEnd                                     = new TimeSpan(23, 59, 59);
				ExtraExtended                               = false;
				ShowAreaColor                               = true;
                RectColor                                   = Brushes.LightCyan;
				ButtonColor                                 = Brushes.Cyan;
			}
			else if (State == State.Configure)
			{
				 endTime  = startTime.Add(TimeEnd);
			}
			else if (State == State.Realtime)
			{
				if (ChartControl != null && !IsToolBarButtonAdded)
				{
				    ChartControl.Dispatcher.InvokeAsync((Action)(() => 
				    {
						AddButtonToToolbar();
					}));
				}
			}
			else if (State == State.Terminated)
			{
				if (chartWindow != null)
				{
			        ChartControl.Dispatcher.InvokeAsync((Action)(() => 
			        {	
						DisposeCleanUp();
					}));
				}
			}	

		}
	
		#region AddButtonToToolbar
		private void AddButtonToToolbar()
		{
			 chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;
		     if (chartWindow == null)
		      {
		          Print("chartWindow == null");
		          return;
		      }
		     Style btnStyle = new Style();
		     btnStyle.TargetType = typeof(System.Windows.Controls.Button);
			 
		     btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontSizeProperty, 11.0));
		     btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontFamilyProperty, new FontFamily("Franklin Gothic Book")));
		     btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontWeightProperty, FontWeights.Bold));
			 btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.MarginProperty, new Thickness(2, 0, 2, 0)));
			 btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.PaddingProperty, new Thickness(4, 2, 4, 2)));
			 btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, ButtonColor));
			 btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.BackgroundProperty, Brushes.Transparent));
			 btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.IsEnabledProperty, true));
			 btnStyle.Setters.Add(new Setter(System.Windows.Controls.Button.HorizontalAlignmentProperty, HorizontalAlignment.Center));
             
			 Expansion = new System.Windows.Controls.Button();
			 Expansion.Content = "Extension"  ;
			 Expansion.Style = btnStyle;
			 chartWindow.MainMenu.Add(Expansion);
             
			 Expansion.Visibility = Visibility.Visible;
			 Expansion.Click += ExpansionClick;
		     IsToolBarButtonAdded = true;
		}	
		 #endregion
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{			
		}	
		private void ExpansionClick(object sender, RoutedEventArgs e)
		{	
			if (CurrentBar < 0)	return;
			
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;			
	
			if (button != null)
			{	
				foreach(IDrawingTool drawObject in DrawObjects)
				{					
			        if(drawObject.GetType().ToString().Contains("Rectangle"))
					  {	
						 if (ExtraExtended) { endTime = endTime.Add(TimeEnd);}
                          dynamic rectangle = drawObject;
						  if(drawObject.IsSelected)
                          {								   
                             if (ShowAreaColor){ rectangle.AreaBrush = RectColor; }							 
							    ((Rectangle) drawObject).EndAnchor = new ChartAnchor(endTime, ((Rectangle) drawObject).EndAnchor.Price, ChartControl);								 
                          }							   
					  }
				}									
				chartWindow.ActiveChartControl.InvalidateVisual();
				ForceRefresh();
			}
		}
		private void DisposeCleanUp()
		{
		  if (Expansion != null) chartWindow.MainMenu.Remove(Expansion);			
		      Expansion.Click -= ExpansionClick;
		}

		#region Properties
		[XmlIgnore]
		[Display(Name="Button Color",  Order = 00, GroupName="01 Parameters")]
		public Brush ButtonColor
		{ get; set; }

		[Browsable(false)]
		public string ButtonColorSerializable
		{
			get { return Serialize.BrushToString(ButtonColor); }
			set { ButtonColor = Serialize.StringToBrush(value); }
		}
		
       	[NinjaScriptProperty]
		[Display(Name="Extra Extended", Order = 01, GroupName="02 Parameters")]
		public bool ExtraExtended
		{ get; set; }
		
//        [XmlIgnore]   
//        [Browsable(false)] 
//        public TimeSpan TimeEnd
//        { get; set; }
 
       
//        [NinjaScriptProperty]
//        [Display(Name = "Time End",  Order = 02, GroupName = "Parameters")]
//        public string TimeEndSpanSerialize
//        {
//          get { return TimeEnd.ToString(); }
//          set { TimeEnd = TimeSpan.Parse(value); }
//        }
		
		[NinjaScriptProperty]
		[Display(Name="Show Other Color", Order = 03, GroupName="02 Parameters")]
		public bool ShowAreaColor
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Rectangle Color",  Order = 04, GroupName="02 Parameters")]
		public Brush RectColor
		{ get; set; }

		[Browsable(false)]
		public string RectColorSerializable
		{
			get { return Serialize.BrushToString(RectColor); }
			set { RectColor = Serialize.StringToBrush(value); }
		}	
		
		#endregion
		
     	#region  Hide names
	    public override string DisplayName
	    {
	    get	{ if  (State == State.SetDefaults){ return GetIndDisplay();}
	    else{ return ""; }}}
	    protected string GetIndDisplay() {
	    try { string[] split = Instrument.FullName.Split(new char[] {' '});
	    string CrSy = split[0];	return CrSy + "Rectangle Extender"; } catch (Exception ex) { return "";}
	    }
	    #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MA5.RectangleExtender[] cacheRectangleExtender;
		public MA5.RectangleExtender RectangleExtender(bool extraExtended, bool showAreaColor)
		{
			return RectangleExtender(Input, extraExtended, showAreaColor);
		}

		public MA5.RectangleExtender RectangleExtender(ISeries<double> input, bool extraExtended, bool showAreaColor)
		{
			if (cacheRectangleExtender != null)
				for (int idx = 0; idx < cacheRectangleExtender.Length; idx++)
					if (cacheRectangleExtender[idx] != null && cacheRectangleExtender[idx].ExtraExtended == extraExtended && cacheRectangleExtender[idx].ShowAreaColor == showAreaColor && cacheRectangleExtender[idx].EqualsInput(input))
						return cacheRectangleExtender[idx];
			return CacheIndicator<MA5.RectangleExtender>(new MA5.RectangleExtender(){ ExtraExtended = extraExtended, ShowAreaColor = showAreaColor }, input, ref cacheRectangleExtender);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MA5.RectangleExtender RectangleExtender(bool extraExtended, bool showAreaColor)
		{
			return indicator.RectangleExtender(Input, extraExtended, showAreaColor);
		}

		public Indicators.MA5.RectangleExtender RectangleExtender(ISeries<double> input , bool extraExtended, bool showAreaColor)
		{
			return indicator.RectangleExtender(input, extraExtended, showAreaColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MA5.RectangleExtender RectangleExtender(bool extraExtended, bool showAreaColor)
		{
			return indicator.RectangleExtender(Input, extraExtended, showAreaColor);
		}

		public Indicators.MA5.RectangleExtender RectangleExtender(ISeries<double> input , bool extraExtended, bool showAreaColor)
		{
			return indicator.RectangleExtender(input, extraExtended, showAreaColor);
		}
	}
}

#endregion
