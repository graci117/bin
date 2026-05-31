// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using	System.Windows.Controls;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.BobC
{
	/// <summary>
	/// CongestionBoxNT8wPOC - by jabeztrading
	/// This indicator was designed for Range Bars but been used by minute bars by others.
	/// The POC is a Volume POC based on the closing price of the bar with the highest volume within the box.
	/// Click the button to make the lines transparent and click again to color the lines.
	/// The button was modeled after the methods shown in the NinjaTrader help guide @
	/// http://ninjatrader.com/support/helpGuides/nt8/en-us/?using_bitmapimage_objects_with_buttons.htm
	/// Thanks to all those who improved, modified, revised, and/or gave input/ideas for this indicator.
	/// Special thanks to Sirkin01 (futures.io member) for his ideas and testing that helped shaped this version of the indicator.
	/// Added code handle the tabs to prevent the button from displaying on other tabs than the intended tab. Code taken from HTToolBarButton.cs (by -=Edge=- of https://www.htech.net/) 
	/// a modification of Ninjatraders SampleAddChartButton.cs. See https://ninjatrader.com/support/forum/forum/historical-beta-archive/version-8-beta/75475-buttons-stackpanels-dockpanels/page3?postcount=21
	/// </summary>
	public class CongestionBoxNTBCrev : Indicator
	{
	
		private System.Windows.Media.Brush boxColor 	= Brushes.Transparent;
		private System.Windows.Media.Brush demandColor	= Brushes.LimeGreen;
		private System.Windows.Media.Brush supplyColor 	= Brushes.Crimson;	
		private System.Windows.Media.Brush pocLineColor = Brushes.DarkBlue;
		
        #region Variables
        // Wizard generated variables
        // User defined variables (add any user defined variables below)
		private double      Upperboundary;
		private double      Lowerboundary;
		private bool        UpDownpattern;
		private bool        DownUppattern;

		private double      buyentry;
		private double      sellentry;
		
		private double      HighofThree;
		private double      LowofThree;	
		private int         firstbar;
		private int         lastbar;
		private int         totalbars;	

		private bool        Inthebox;		
		private bool        breakup;
		private bool        breakdown;
		private bool		switchPocLineOn;
		
		private double      breakupbarvalue;
		private double      breakdownbarvalue;


	
		private Brush 		pocLineColorSwitch;
		
		// Included series to store level values to be used in strageties
		private Series<double>	upperBoundarySeries; 
		private Series<double>	lowerBoundarySeries;
		private Series<double>	boxBarNumSeries;
		private Series<double>	pocLineColorSeries;
		
		//Enable Sound Alerts
		private bool	soundAlert		= false;
		
		
		// Define a Chart object to refer to the chart on which the indicator resides
		private Chart chartWindow;

		// Define a Button
		private System.Windows.Controls.Button pocLineButton = null;
		
		private bool IsToolBarButtonAdded;
		
        #endregion	
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionADL;
				Name						= "CongestionBoxNTBCrev";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				IsAutoScale					= false;
				HighofThree 		= 0;
				LowofThree 			= 0;	
				firstbar 			= 0;
				lastbar 			= 0;
				totalbars 			= 0;
				trade= 0;

				Inthebox            = false;				
				breakup             = false;
				breakdown           = false;				
				
				Opacity1            = 70;
				Opacity2            = 70;
				Opacity3            = 70; 								
		
				
				StartHour			= 6;
				EndHour				= 13;

				AddPlot(Brushes.Transparent, "VPOCLine");
				switchPocLineOn 	= true;

				
			}
			else if (State == State.Configure)
   			{ 
				upperBoundarySeries	= new Series<double>(this);
				lowerBoundarySeries	= new Series<double>(this);
				boxBarNumSeries		= new Series<double>(this);   
   			}
			else if (State == State.Historical)
		      {
		          //Call the custom addButtonToToolbar method in State.Historical to ensure it is only done when applied to a chart
		           // -- not when loaded in the Indicators window
		          if (!IsToolBarButtonAdded) AddButtonToToolbar();
		      }
		      else if (State == State.Terminated)
		      {
		          //Call a custom method to dispose of any leftover objects in State.Terminated
		          DisposeCleanUp();
		      }				
			
			
		}
		
  private void AddButtonToToolbar()
  {
      // Use this.Dispatcher to ensure code is executed on the proper thread
      ChartControl.Dispatcher.InvokeAsync((Action)(() =>
      {
 
          //Obtain the Chart on which the indicator is configured
          chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;
          if (chartWindow == null)
          {
              Print("chartWindow == null");
              return;
          }
		  //Set Event Handle for Tab Change // added
		  else chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		  
          // Create a style to apply to the button
          Style s = new Style();
          s.TargetType = typeof(System.Windows.Controls.Button);
          s.Setters.Add(new Setter(System.Windows.Controls.Button.FontSizeProperty, 11.0));
          s.Setters.Add(new Setter(System.Windows.Controls.Button.BackgroundProperty, Brushes.Green));
          s.Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, Brushes.White));
          s.Setters.Add(new Setter(System.Windows.Controls.Button.FontFamilyProperty, new FontFamily("Arial")));
          s.Setters.Add(new Setter(System.Windows.Controls.Button.FontWeightProperty, FontWeights.Bold));
 
          // Instantiate the Button
          pocLineButton = new System.Windows.Controls.Button();
 
          //Set Button Style             
          pocLineButton.Style = s;
 
 
          pocLineButton.Content = "POC Line On";
          //pocLineButton.IsEnabled = true;
          pocLineButton.HorizontalAlignment = HorizontalAlignment.Left;
		  
		  // Subscribe to each buttons click event to execute the logic we defined in OnPocLineButtonClick()
 		  pocLineButton.Click += OnPocLineButtonClick;
		  
		  
          // Add the Button to the Chart's Toolbar
          chartWindow.MainMenu.Add(pocLineButton);
		  
          //Prevent the Button From Displaying when WorkSpace Opens if it is not in an active tab
          pocLineButton.Visibility = Visibility.Collapsed;
          foreach (TabItem tab in this.chartWindow.MainTabControl.Items)
          {
              if ((tab.Content as ChartTab).ChartControl == this.ChartControl
                   && tab == this.chartWindow.MainTabControl.SelectedItem)
              {
                  pocLineButton.Visibility = Visibility.Visible;
              }
          }
          IsToolBarButtonAdded = true;
      }));
  }		


//Added ** Tab Changed Event Handler
private void TabChangedHandler(object sender, SelectionChangedEventArgs e)
{
	//Null Check of Sorts
	if (e.AddedItems.Count <= 0) return;
	//Declare Temp Chart Tab from Event Args
	ChartTab tmpCT = (e.AddedItems[0] as TabItem).Content as ChartTab;
	//Temp ChartTab Null Check
	if (tmpCT != null)
	{	
		// Set Button Visiblity Based on Selected Tab
		if(pocLineButton != null)	pocLineButton.Visibility = tmpCT.ChartControl == this.ChartControl ? Visibility.Visible : Visibility.Collapsed;
	}
}	  
  
  private void DisposeCleanUp()
  {
      //ChartWindow Null Check
      if (chartWindow != null)
      {
          //Dispatcher used to Assure Executed on UI Thread
          ChartControl.Dispatcher.InvokeAsync((Action)(() =>
          {
              //Button Null Check
              if (pocLineButton != null)
              {
                  //Remove Button from Indicator's Chart ToolBar
                  chartWindow.MainMenu.Remove(pocLineButton);
				  pocLineButton.Click -= OnPocLineButtonClick;
				  pocLineButton = null;  // added
              }
			//Remove Tab Changed Event Handler  ** Added
			chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;
			//Set ChartWindow to Null - Not Needed - Again Just Done out of Habit
			chartWindow = null;			  
          }));
      }
  }	  
  
		protected override void OnBarUpdate()
		{
			if (CurrentBar < 30)		//was 2
			{
				upperBoundarySeries[0] 	= 0;
				lowerBoundarySeries[0] 	= 0;
				boxBarNumSeries[0] 		= 0;
				return;
			}
			
		if ((ToTime(Time[0]) >= (StartHour)*10000) && (ToTime(Time[0]) <(EndHour)*10000) )
			{
			
			//Determine if a two bar pattern exist which could be the beginning of some indecision. Checking
			//for an up bar and then a down bar or a down bar and then an up bar
	        UpDownpattern =  ( (Close[1] > Open[1]) || (Close[1] >= Open[1] && Close[1]>(((High[1]-Low[1])*0.5)+Low[1]))  ) && Close[0] < Open[0]; 
			DownUppattern =  ( (Close[1] < Open[1]) || (Close[1] <= Open[1] && Close[1]<(((High[1]-Low[1])*0.5)+Low[1]))  ) && Close[0] > Open[0];
		    
			//If a pattern exist and box is not in progress then we set new upper and lower boundaries.
			if(UpDownpattern && Inthebox==false ) //
			{
				
				Upperboundary = MAX(High, 2)[0];
				Lowerboundary = MIN(Low, 2)[0]; 

				Inthebox = true;
				firstbar = CurrentBar - 1;				
			}
			else
			if(DownUppattern && Inthebox==false ) //
			{

		
				Upperboundary = MAX(High, 2)[0];
				Lowerboundary = MIN(Low, 2)[0]; 		
				
				Inthebox = true;	
				firstbar = CurrentBar - 1;				
			}	
			else
			{
				upperBoundarySeries[0] 	= upperBoundarySeries[1]; // The upper and lower levels of the box are being stored in a data series for future acces i.e. strategy
				lowerBoundarySeries[0] 	= lowerBoundarySeries[1];				
				boxBarNumSeries[0] 		= boxBarNumSeries[1];	// stores the bar number of the first bar of the box.
			}
			
			// If a box is already in progress we are checking to see if it was broken. If not we continue drawing the box according to the previous upper and lower level values.
			if(Inthebox==true ) //
			{
				breakup = false; breakdown = false;

				Draw.Rectangle(this,"Box"+CurrentBar,false, 0, Upperboundary,1,Lowerboundary, Brushes.Transparent, BoxColor, Opacity1);
			
				// VPOC routine
				double MaxVolume = 0;
				double MaxPrice = 0.0;
				for (int x = 0; x <= (CurrentBar - firstbar); x++)   //totalbars = CurrentBar - firstbar;	
				{
					if (Volume[x] > MaxVolume)
					{
						MaxVolume = Volume[x];
						MaxPrice = Close[x];
					}
				}
				VPOCLine[0] = MaxPrice;				
				// end VPOC routine
				
				if(Close[0] > Upperboundary)
				{
				    Inthebox = false;
					breakupbarvalue = Close[0];
					lastbar = CurrentBar;
					totalbars = lastbar - firstbar;	
					
					upperBoundarySeries[0] 	= Upperboundary;
					lowerBoundarySeries[0] 	= Lowerboundary;
					boxBarNumSeries[0] 		= CurrentBar;					
					
				    breakup = true;
				}else
				if(Close[0] < Lowerboundary)
				{
	
				    Inthebox = false;
					breakdownbarvalue = Close[0];
					lastbar = CurrentBar;
					totalbars = lastbar - firstbar;	
					
					upperBoundarySeries[0] 	= Upperboundary;
					lowerBoundarySeries[0] 	= Lowerboundary;
					boxBarNumSeries[0] 		= CurrentBar;	
					
					breakdown = true;
				}
				else
				{
					upperBoundarySeries[0] 	= upperBoundarySeries[1];
					lowerBoundarySeries[0] 	= lowerBoundarySeries[1];					
					boxBarNumSeries[0] 		= boxBarNumSeries[1];
				}
				
			}
			if(switchPocLineOn)
				pocLineColorSwitch = PocLineColor;
			else
				pocLineColorSwitch =  Brushes.Transparent;
			
			//Demand Zone Maybe?? The idea is that two consecutive up bars, plus one in the box making it three together, and having a break out of that box, might mean something significant as far as demand goes
			// 
			if(breakup && Close[0]> breakupbarvalue && (CurrentBar == lastbar+1) )
			{
				for(int i=firstbar+1; i< lastbar+1;i++)
					  RemoveDrawObject("Box"+i);
					
				Draw.Rectangle(this,"Box2"+CurrentBar,false, totalbars+1, Upperboundary,1,Lowerboundary, Brushes.Transparent, demandColor, Opacity2);

				Draw.Line(this,"POCLine"+CurrentBar, false, totalbars+1, VPOCLine[2],1, VPOCLine[2], pocLineColorSwitch , DashStyleHelper.Solid, 2);
				
				trade = 2;  //Buy Signal
		//		Print("Buy Signal    "+(trade));
		//		Print("POC Line    "+(VPOCLine[2]));
			if(SoundAlert==true)	
					{
					Alert("myAlert", Priority.High, "Reached threshold", this.audioFileAlert, armtime, Brushes.Black, Brushes.Green);
					}	
			} else
			//Supply Zone Maybe?? The idea is that two consecutive down bars, plus one in the box making it three together, and having a break out of that box, might mean something significant as far as supply goes
			//
			if(breakdown && Close[0]< breakdownbarvalue && (CurrentBar == lastbar+1) )
			{			
				for(int i=firstbar+1; i< lastbar+1;i++)
					  RemoveDrawObject("Box"+i);					

				Draw.Rectangle(this,"Box3"+CurrentBar,false, totalbars+1, Upperboundary,1,Lowerboundary, Brushes.Transparent, supplyColor, Opacity3);		
			
				Draw.Line(this,"POCLine"+CurrentBar, false, totalbars+1, VPOCLine[2],1, VPOCLine[2], pocLineColorSwitch , DashStyleHelper.Solid, 2);
				
				trade = 1;   //Sell Signal
		//		Print("Sell Signal    "+(trade));
		//		Print("POC Line    "+(VPOCLine[2]));
			if(SoundAlert==true)	
					{
					Alert("myAlert", Priority.High, "Reached threshold", this.audioFileAlert, armtime, Brushes.Black, Brushes.Green);
					}	
			}

		} //end OnBarUpdate
		}
		
		// Define a custom event method to handle our custom task when the button is clicked
		private void OnPocLineButtonClick(object sender, RoutedEventArgs rea)
		{
		  System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;

			if(pocLineButton.Background == Brushes.Red)
			{	
				pocLineButton.Background = Brushes.Green;
				pocLineButton.Content = "POC Line On";

				switchPocLineOn = true;
				
				// loops through each draw object on the chart
				  foreach (DrawingTool draw in DrawObjects)
				  {
					    if (draw is DrawingTools.Line)
					    {               
							DrawingTools.Line drawnLines = draw as DrawingTools.Line;
							
					       // Loop through each bar for a tag match and change color PocLineColor
							for(int i = 0; i <= CurrentBar; i++)
							{
								if(draw.Tag == "POCLine"+i) 
									drawnLines.Stroke.Brush = PocLineColor;
							} //end for loop
					    } //end if					  
				  } //end foreach loop
				  //chartWindow.ActiveChartControl.InvalidateVisual();
				  ForceRefresh();
			}
			else
			{
				pocLineButton.Background = Brushes.Red;
				pocLineButton.Content = "POC Line OFF";

				switchPocLineOn = false;
				
				// loops through each draw object on the chart
				  foreach (DrawingTool draw in DrawObjects)
				  {
					    if (draw is DrawingTools.Line)
					    {               
							DrawingTools.Line drawnLines = draw as DrawingTools.Line;
							
					       // Loop through each bar for a tag match and change color to Transparent
							for(int i = 0; i <= CurrentBar; i++)
							{
								if(draw.Tag == "POCLine"+i) 
									drawnLines.Stroke.Brush = Brushes.Transparent;
							} //end for loop
					    } //end if					  
				  } //end foreach loop	
				 ForceRefresh(); 
			} // end else
		} //Close PocLineButtonClick			


		
		#region Properties
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource),Name = "Opacity1", Description="Opacity for Gray Box",GroupName = "Parameters",Order=1)]
		public int Opacity1
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource),Name = "Opacity2",Description="Opacity for Demand Zone",GroupName = "Parameters",Order=2)]
		public int Opacity2
		{ get; set; }
		
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource),Name = "Opacity3",Description="Opacity for Supply Zone",GroupName = "Parameters",Order=3)]
		public int Opacity3
		{ get; set; }
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource),Name = "StartHour",Description="Hour to Start Display",GroupName = "Parameters",Order=4)]
		public int StartHour
		{ get; set; }
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource),Name = "EndHour",Description="Hour to End Display",GroupName = "Parameters",Order=5)]
		public int EndHour
		{ get; set; }
		

		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Box Color", Description="Select color for Congestion Box",GroupName = "Plot Colors",Order=1)]
		public System.Windows.Media.Brush BoxColor
		{
            get { return boxColor; }
            set { boxColor = value; }			
		}		
		
		[Browsable(false)]
		public string boxColorSerialize
		{
			get { return Serialize.BrushToString(boxColor); }
			set { boxColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Demand Box Color", Description="Select color for Demand Zone",GroupName = "Plot Colors",Order=2)]
		public System.Windows.Media.Brush DemandColor
		{
            get { return demandColor; }
            set { demandColor = value; }	
		}
		
		[Browsable(false)]
		public string demandColorSerialize
		{
			get { return Serialize.BrushToString(demandColor); }
			set { demandColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Supply Box Color", Description="Select color for Supply Zone",GroupName = "Plot Colors",Order=3)]
		public System.Windows.Media.Brush SupplyColor
		{
            get { return supplyColor; }
            set { supplyColor = value; }
		}
		
		[Browsable(false)]
		public string supplyColorSerialize
		{
			get { return Serialize.BrushToString(supplyColor); }
			set { supplyColor = Serialize.StringToBrush(value); }
		}

		[XmlIgnore()]
		[Display(ResourceType = typeof(Custom.Resource), Name = "POC Line Color", Description="Select color for POC Line",GroupName = "Plot Colors",Order=4)]
		public System.Windows.Media.Brush PocLineColor
		{
            get { return pocLineColor; }
            set { pocLineColor = value; }
		}
		
		[Browsable(false)]
		public string pocLineColorSerialize
		{
			get { return Serialize.BrushToString(pocLineColor); }
			set { pocLineColor = Serialize.StringToBrush(value); }
		}		
		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBoundarySeries
		{
			get { return upperBoundarySeries; }
		}			
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBoundarySeries
		{
			get { return lowerBoundarySeries; }
		}	
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BoxBarNumSeries
		{
			get { return boxBarNumSeries; }
		}			
		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VPOCLine
		{
			get { return Values[0]; }
		}		
		
		[Browsable(false)]
		[Range(0, 2), NinjaScriptProperty]  //1= short, 2= Long
		[XmlIgnore]
		public int trade
		{
			 get; set; 
		}		
		
		[Display(Name="Play Sound Alert", Description="Play wav file",Order=11, GroupName="Audible")]
		public bool SoundAlert
		{ 
			get { return soundAlert;}
			set { soundAlert = value;}
		}
			
		
		[Display(Name="Audio file for transition alerts", Order=12, GroupName="Audible")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string audioFileAlert
		{ get; set; }
		
		
		[Display(Name="Audio Re-Arm Time", Order=13, GroupName="Audible")]
		public int armtime
		{ get; set; }
		
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BobC.CongestionBoxNTBCrev[] cacheCongestionBoxNTBCrev;
		public BobC.CongestionBoxNTBCrev CongestionBoxNTBCrev(int opacity1, int opacity2, int opacity3, int startHour, int endHour, int trade)
		{
			return CongestionBoxNTBCrev(Input, opacity1, opacity2, opacity3, startHour, endHour, trade);
		}

		public BobC.CongestionBoxNTBCrev CongestionBoxNTBCrev(ISeries<double> input, int opacity1, int opacity2, int opacity3, int startHour, int endHour, int trade)
		{
			if (cacheCongestionBoxNTBCrev != null)
				for (int idx = 0; idx < cacheCongestionBoxNTBCrev.Length; idx++)
					if (cacheCongestionBoxNTBCrev[idx] != null && cacheCongestionBoxNTBCrev[idx].Opacity1 == opacity1 && cacheCongestionBoxNTBCrev[idx].Opacity2 == opacity2 && cacheCongestionBoxNTBCrev[idx].Opacity3 == opacity3 && cacheCongestionBoxNTBCrev[idx].StartHour == startHour && cacheCongestionBoxNTBCrev[idx].EndHour == endHour && cacheCongestionBoxNTBCrev[idx].trade == trade && cacheCongestionBoxNTBCrev[idx].EqualsInput(input))
						return cacheCongestionBoxNTBCrev[idx];
			return CacheIndicator<BobC.CongestionBoxNTBCrev>(new BobC.CongestionBoxNTBCrev(){ Opacity1 = opacity1, Opacity2 = opacity2, Opacity3 = opacity3, StartHour = startHour, EndHour = endHour, trade = trade }, input, ref cacheCongestionBoxNTBCrev);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BobC.CongestionBoxNTBCrev CongestionBoxNTBCrev(int opacity1, int opacity2, int opacity3, int startHour, int endHour, int trade)
		{
			return indicator.CongestionBoxNTBCrev(Input, opacity1, opacity2, opacity3, startHour, endHour, trade);
		}

		public Indicators.BobC.CongestionBoxNTBCrev CongestionBoxNTBCrev(ISeries<double> input , int opacity1, int opacity2, int opacity3, int startHour, int endHour, int trade)
		{
			return indicator.CongestionBoxNTBCrev(input, opacity1, opacity2, opacity3, startHour, endHour, trade);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BobC.CongestionBoxNTBCrev CongestionBoxNTBCrev(int opacity1, int opacity2, int opacity3, int startHour, int endHour, int trade)
		{
			return indicator.CongestionBoxNTBCrev(Input, opacity1, opacity2, opacity3, startHour, endHour, trade);
		}

		public Indicators.BobC.CongestionBoxNTBCrev CongestionBoxNTBCrev(ISeries<double> input , int opacity1, int opacity2, int opacity3, int startHour, int endHour, int trade)
		{
			return indicator.CongestionBoxNTBCrev(input, opacity1, opacity2, opacity3, startHour, endHour, trade);
		}
	}
}

#endregion
