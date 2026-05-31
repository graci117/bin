#region Using declarations
using System;
using System.IO;
using System.Globalization;
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

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.TrendIsYourFriend
{
	[Gui.CategoryOrder("General", 0)] 
	[Gui.CategoryOrder("Highlighted rectangles - settings", 1)] 
	[Gui.CategoryOrder("Dimmed rectangles - settings", 2)] 
	[Gui.CategoryOrder("Trade plan (TP) - Save settings", 3)] 
	[Gui.CategoryOrder("Trade plan (TP) - Load settings", 4)] 
	[Gui.CategoryOrder("Toolbar - settings", 5)] 

    public class tiyfTradePlanFactory : Indicator
    {
		/// <summary> 
		/// indicator name : tiyfTradePlanFactory 
		/// compiled with Ninjatrader version 8.0.24.3 64-bit - September 2021
		/// author : trendisyourfriend (futures.io)
		/// trendisyourfriend's discussion thread : https://futures.io/elite-circle/57090-tiyftradeplanfactory-indicator.html#post841548 
		/// ChangeLog:
		/// 2021-05-03 Version 1: first time released on Futures.io
		/// 2021-05-04 Version 1a: added the [NinjaScriptProperty] attribute to the 'Indicator_Version' property to expose it to the user
		/// 2021-05-05 Version 1b: - the highlighted or dimmed settings will no more be applied to a user drawn rectangle
		///                        - modified the default list of prefix "S&D, Fib, MM, MP, Vwap"
		///                        - changed the location of the Indicator_Version value. It is now part of the parameter name to avoid
		///                          problems when the user has created templates with prior version of the indicator
		/// 2021-05-07 Version 1c: when the user click on a rectangle the indicator displays its Tag name at the upper left of the chart 
		/// 2021-05-08 Version 1d: a selected rectangle will also display the distance it is from the last bar close 
		/// 2021-05-11 Version 1e: new button added to forecast levels 
		/// 2021-05-12 Version 1f: forgot to reset the count of forecasted areas when a new category of Rectangles was Highlighted.
		///                        - changed the method for evaluating the average gap between rectangles. New method is more logical.
		/// 2021-05-13 Version 1g: The Highlight and Forecast functions have been reworked to ease the job of forecasting
		///                        - it is no more necessary to load a trade plan to use the forecast option
		///                        - If there is no trade plan loaded, The highlight button will display the "Rect" prefix only
		/// 2021-05-15 Version 1h: Added the ability to save multiple forecasts into a file
		/// 2021-05-16 Version 1i: forgot to dispose of the new button Forecast in the State.Terminated event.
		/// - Modified the tooltip message associated to the Add prefix button when the save button content = Save Forecast.
		/// 2021-05-17 Version 1j: Removed the required step to reset/erase a previous forecast before generating a new one
		/// - the indicator can now sniff out if a series of user drawn rectangles have been modified (Rect. moved/added/removed)
		/// - any selected rectangle (no more restricted to rectangles from a loaded TP) will display the distance it is from the last bar close 
		/// 2021-08-09 Version 1k: I have modified the method to assign a unique name or version number to the indicator (see method DisplayName)
	/// </summary>
		
		#region CLASS_WIDE_VARIABLES
		//-
		private const string	SystemVersion = "v1k";
		private const string	SystemName = "tiyfTradePlanFactory";
		private const string	FullSystemName = SystemName + " - " + SystemVersion;
		
		private List<string> listOfTagNamesLoaded; //used to hold the Tag names from the most recent loaded file (tradeplan)
		private List<string> listOfTagNamesForecasted; //used to hold the Tag names of forecast rectangles
		private List<Tuple<string, double, double>> listOfPriorDrawnRectanglesByUser; //used to hold the attributes of all user drawn rectangles when the Forecast button is pressed
		private List<string> listOfRect; //used to temporarily hold all manually drawn rectangles attributes (5 per rectangle) before saving to disk
		private List<string> listOfRect2; //used to temporarily hold all previously saved rectangles attributes. Used to avoid duplicate Tag names and keep the most recent drawn rectangle.
		private List<string> listOfFilters; //used to hold the filters as defined by the user in parameter 'Apply visual settings if tag name starts with...'
		private List<string> listOfPrefixes; //used to hold the prefixes as defined by the user in parameter 'Tag name - list of prefixes (comma separated)'
		private int currentFilterIdx; //holds the current idx of listOfFilters[?] to keep track where we are when the user presses the filterButton
		private int currentPrefixIdx; //holds the current idx of listOfPrefixes[?] to keep track where we are when the user presses the addPrefixButton
		private int nbAreasForecasted; //counter representing the nb of areas or zones forecasted
		
		private DrawingTools.HorizontalLine dummyHorizontalLine;
		private string rectTagPrefix; //was a property in older version. Now it is converted into a class wide variable
		
		//Buttons and toolbar variables
		private System.Windows.Controls.Button forecastButton;
		private System.Windows.Controls.Button filterButton;
		private System.Windows.Controls.Button saveButton;
		private System.Windows.Controls.Button loadButton;
		private System.Windows.Controls.Button addPrefixButton;
		private System.Windows.Controls.Button hideToolBarButton;
		private System.Windows.Controls.Grid myGrid;
		private VerticalAlignment vAlignment; //toolbar VerticalAlignment
		private HorizontalAlignment hAlignment; //toolbar HorizontalAlignment
		//-
		#endregion
		
		protected override void OnStateChange()
		{
		    if (State == State.SetDefaults)
		    {
				#region STATE_SETDEFAULTS
				//-
		        Description                                    = @"Extend all manually drawn rectangles to the right side.";
		        Name                                        = "tiyfTradePlanFactory";
		        Calculate                                   = Calculate.OnBarClose;
		        IsOverlay                                   = true;
		        DisplayInDataBox                            = true;
		        DrawOnPricePanel                            = true;
		        DrawHorizontalGridLines                     = true;
		        DrawVerticalGridLines                       = true;
		        PaintPriceMarkers                           = true;
		        ScaleJustification                          = NinjaTrader.Gui.Chart.ScaleJustification.Right;
		        //Disable this property if your indicator requires custom values that cumulate with each new market data event.
		        //See Help Guide for additional information.
		        IsSuspendedWhileInactive                    = true;
				
				///DEFAULT> General
				
				//path to Ninjatrader\export folder
				pathToTradePlan = NinjaTrader.Core.Globals.UserDataDir + "export" +"\\";
				rectTagFilterSkip = "x"; //ignore rectangle if tag name starts with...
				
				///DEFAULT> Highlighted rectangles - settings
//				rectTagFilter = "m1, m5, m15, m30, m60, tik, Rng, PnF, Rnk, LineB, Day, ON, MP, IB, VP, VA, hvn, lvn, vwap, MM, Fib, FibRet, FibExt, DD%";
				rectTagFilter = "S&D, FCst, Fib, MM, MP, Vwap"; // <-- Highlight rectangles if tag name starts with
				rectAreaColor = Brushes.PaleTurquoise;
				rectAreaOpacity = 10;
				rectOutlineStroke = new Stroke(Brushes.Wheat, DashStyleHelper.Dash, 1, 100);
				
				///DEFAULT> Dimmed rectangles - settings
				rectAreaColor2 = Brushes.LightGray;
				rectAreaOpacity2 = 7;
				rectOutlineStroke2 = new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1, 50);
				
				///DEFAULT> Trade plan (TP) - Save settings
				tradePlanFileName = "TradePlan";
				forecastingFileName = "Forecast";
				prefixFileNameWithInstrumentName = true; //This option if checked (true) tells the indicator to add the Instrument name to the left of the trade plan & Forecasting filenames before saving
				
				///DEFAULT> Trade plan (TP) - Load settings
				tradePlanFileName2 = "Forecast"; //if empty then nothing to load
				prefixFileNameWithInstrumentName2 = true; //This option if checked (true) tells the indicator to add the Instrument name to the left of the trade plan filename before loading

				///DEFAULT> Toolbar - settings
				buttonForegroundColor = Brushes.White;
				buttonBackgroundColor = Brushes.Black;
				vAlignment = VerticalAlignment.Bottom;
				hAlignment = HorizontalAlignment.Center;
				
				///DEFAULT> Class wide variable(s)
				listOfTagNamesLoaded = new List<string>(); //Required variable to know if a trade plan has been loaded and to know which rectangle to remove based on their Tag name
				//-
				#endregion
		    }
		    else if (State == State.Configure)
		    {
				// REQUIRED: dummy plot necessary so strategies using this indicator
				// the OnBarUpdate will be called in this indicator.
				base.AddPlot(System.Windows.Media.Brushes.Transparent, "dummyPlot");
				rectTagPrefix = rectTagFilter; //list of prefixes. One of them will be added to the left of all Tag names at save time
				nbAreasForecasted = 0; //keep track of the count of forecasted areas
				listOfTagNamesForecasted = new List<string>();
				listOfPriorDrawnRectanglesByUser = new List<Tuple<string, double, double>>();
			}
		    else if (State == State.Realtime)
		    {
				// create a dummy object (such as a HorizontalLine)
				// this is necessary to populate/refresh the DrawObjects collection when no live data is coming in
				if ( DrawObjects["tiyfDummyObject"] as DrawingTools.HorizontalLine == null ) {
					dummyHorizontalLine = Draw.HorizontalLine(this, "tiyfDummyObject", Close[0], Brushes.Transparent);
				}
				
				displayMsg("\nReady to extend all rectangles on the chart ! \n\n-> Rectangles will extend automatically at the next bar close. \n-> Or you may Select or Unselect any object on the chart to extend all rectangles right away.");
			}
			else if (State == State.Historical)
			{
				#region STATE_HISTORICAL
				//-
				ClearOutputWindow();
				// local variable(s)
				List<string> listTempo;
				
				#region SplitStringParametersIntoAList
					// split the string parameter 'rectTagFilter' into a useable list
					listTempo = new List<string>();
					listTempo = rectTagFilter.Split(',').ToList();
					
					listOfFilters = new List<string>();
					listOfFilters.Add("Rect"); // Add filter 'Rect' automatically to avoid the user a required entry for filtering user drawn rectangles.
					for (int i = 0; i < listTempo.Count; i++) 
					{
						listOfFilters.Add( listTempo[i].Trim() );
					}
//					listOfFilters.Add("None"); // None = don't highlight any rectangle
					currentFilterIdx = 0;

					// split the string parameter 'rectTagPrefix' into a useable list
					listTempo = new List<string>();
					listTempo = rectTagPrefix.Split(',').ToList();

					listOfPrefixes = new List<string>();
					for (int i = 0; i < listTempo.Count; i++) 
					{
						listOfPrefixes.Add( listTempo[i].Trim() );
					}
					listOfPrefixes.Add("None"); // None = don't add a prefix at save time
					currentPrefixIdx = 0;				
				//-
				#endregion
				
				if (UserControlCollection.Contains(myGrid))
					return;
				
				Dispatcher.InvokeAsync((() =>
				{
					myGrid = new System.Windows.Controls.Grid
					{
						Name = "MyCustomGrid", HorizontalAlignment = hAlignment, VerticalAlignment = vAlignment
					};
					
					System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column3 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column4 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column5 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column6 = new System.Windows.Controls.ColumnDefinition();
					
					myGrid.ColumnDefinitions.Add(column1);
					myGrid.ColumnDefinitions.Add(column2);
					myGrid.ColumnDefinitions.Add(column3);
					myGrid.ColumnDefinitions.Add(column4);
					myGrid.ColumnDefinitions.Add(column5);
					myGrid.ColumnDefinitions.Add(column6);
					
					///Buttons attributes
					forecastButton = new System.Windows.Controls.Button
					{
						Name = "ForecastButton", Content = "Forecast(0) > " + listOfFilters[0].Trim(), ToolTip = "Estimate probable areas above and below the highlighted category of rectangles.", Foreground = buttonForegroundColor, Background = buttonBackgroundColor
					};

					filterButton = new System.Windows.Controls.Button
					{
						Name = "FilterButton", Content = "Highlight: " + listOfFilters[0].Trim(), ToolTip = "Highlight all rectangles whose Tag name starts with the selected prefix.\n\nClick this button repeatedly to select a different filter.", Foreground = buttonForegroundColor, Background = buttonBackgroundColor
					};
					
					loadButton = new System.Windows.Controls.Button
					{
						Name = "LoadButton", Content = "Load TP", ToolTip = "Load a Trade Plan from the specified file as defined in the 'Load settings' section", Foreground = buttonForegroundColor, Background = buttonBackgroundColor
					};

					saveButton = new System.Windows.Controls.Button
					{
						Name = "SaveButton", Content = "Save TP", ToolTip = "Save all manually drawn rectangles into the specified file as defined in the 'Save settings' section", Foreground = buttonForegroundColor, Background = buttonBackgroundColor
					};
					
					addPrefixButton = new System.Windows.Controls.Button
					{
						Name = "AddPrefixButton", Content = "Add prefix: " + listOfPrefixes[0].Trim(), ToolTip = "The selected prefix will be added to the Tag name of all manually drawn rectangles\nnext time you press the 'Save TP' button.\n\nClick this button repeatedly to select a different prefix.", Foreground = buttonForegroundColor, Background = buttonBackgroundColor
					};

					hideToolBarButton = new System.Windows.Controls.Button
					{
						Name = "HideToolBarButton", Content = "Hide Toolbar", ToolTip = "Hide all 5 buttons.\n\nTo display the Toolbar again just press F5.", Foreground = buttonForegroundColor, Background = buttonBackgroundColor
					};

					forecastButton.Click += OnButtonClick;
					filterButton.Click += OnButtonClick;
					saveButton.Click += OnButtonClick;
					loadButton.Click += OnButtonClick;
					addPrefixButton.Click  += OnButtonClick;
					hideToolBarButton.Click  += OnButtonClick;
					
					System.Windows.Controls.Grid.SetColumn(forecastButton, 5);
					System.Windows.Controls.Grid.SetColumn(filterButton, 4);
					System.Windows.Controls.Grid.SetColumn(loadButton, 3);
					System.Windows.Controls.Grid.SetColumn(saveButton, 2);
					System.Windows.Controls.Grid.SetColumn(addPrefixButton, 1);
					System.Windows.Controls.Grid.SetColumn(hideToolBarButton, 0);
					
					myGrid.Children.Add(forecastButton);
					myGrid.Children.Add(filterButton);
					myGrid.Children.Add(loadButton);
					myGrid.Children.Add(saveButton);
					myGrid.Children.Add(addPrefixButton);
					myGrid.Children.Add(hideToolBarButton);

					UserControlCollection.Add(myGrid);
				}));
				//-
				#endregion
			}
			else if (State == State.Terminated)
			{
				#region STATE_TERMINATED
				//-
				RemoveDrawObject("tiyfDummyObject"); //DrawingTools.HorizontalLine
				dummyHorizontalLine = null;
				Dispatcher.InvokeAsync((() =>
				{
					if (myGrid != null)
					{
						if (forecastButton != null)
						{
							myGrid.Children.Remove(forecastButton);
							forecastButton.Click -= OnButtonClick;
							forecastButton = null;
						}
						if (filterButton != null)
						{
							myGrid.Children.Remove(filterButton);
							filterButton.Click -= OnButtonClick;
							filterButton = null;
						}
						if (saveButton != null)
						{
							myGrid.Children.Remove(saveButton);
							saveButton.Click -= OnButtonClick;
							saveButton = null;
						}
						if (loadButton != null)
						{
							myGrid.Children.Remove(loadButton);
							loadButton.Click -= OnButtonClick;
							loadButton = null;
						}
						if (addPrefixButton != null)
						{
							myGrid.Children.Remove(addPrefixButton);
							addPrefixButton.Click -= OnButtonClick;
							addPrefixButton = null;
						}
						if (hideToolBarButton != null)
						{
							myGrid.Children.Remove(hideToolBarButton);
							hideToolBarButton.Click -= OnButtonClick;
							hideToolBarButton = null;
						}
					}
				}));
				//-
				#endregion
			}
		} //end OnStateChange

		public override string DisplayName
        {
            get { return FullSystemName; }
        }
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			#region ON_BUTTON_CLICK
			//-
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			
			if ( button == forecastButton )
			{
//				myGrid.Children[0].Visibility = Visibility.Visible;			
				if (DrawObjects[ "tiyfDummyObject" ] as DrawingTools.HorizontalLine != null) {
					
					if ( listOfFilters[ currentFilterIdx ] == "Rect" ) {
						// Forecasting based on user drawn rectangles whose tag name starts with the prefix "Rect"
						foreCastingAreas( false );
					} else {
						// Forecasting based on the Highlighted category of rectangles from a loaded trade plan
						if ( listOfTagNamesLoaded.Count > 0 ) {
							foreCastingAreas( true );
						} else {
							displayMsg("\nForecasting future areas will work if a trade plan has been loaded.\nYou must first load a trade plan and Highlight a category of rectangles\nto forecast future areas.");
						}
					}
					
					int lastBarIdx = Bars.Count-1;
					ExtendAllRectangles( Bars.GetTime(lastBarIdx) );
					ForceRefresh();
				}
				return;
			}

			if ( button == filterButton ) // << - filterButton represents the Highlight button
			{
//				myGrid.Children[1].Visibility = Visibility.Visible;
				if (DrawObjects[ "tiyfDummyObject" ] as DrawingTools.HorizontalLine != null) {
					displayMsg("");
					if (listOfTagNamesLoaded.Count > 0)
					{
						currentFilterIdx++;
						if (currentFilterIdx == listOfFilters.Count) currentFilterIdx = 0;
						
						filterButton.Content = "Highlight: " + listOfFilters[ currentFilterIdx ];
					}
					
					resetForecasting();
					
					int lastBarIdx = Bars.Count-1;
					ExtendAllRectangles( Bars.GetTime(lastBarIdx) );
					ForceRefresh();
				}
				return;
			}
			
			if (button == loadButton)
			{
//				myGrid.Children[2].Visibility = Visibility.Hidden;
				currentFilterIdx = 0;
				filterButton.Content = "Highlight: " + listOfFilters[0].Trim();
				resetForecasting();
				if ( tradePlanFileName2.Trim() != "" ) {
					ReadTradePlanFromFile();
					int lastBarIdx = Bars.Count-1;
					ExtendAllRectangles( Bars.GetTime(lastBarIdx) );
				}
				else
				{
					displayMsg("\nWARNING ! The parameter 'File name' in the 'Trade plan (TP) - Load settings' section is empty ! \n\n-> Select any object on the chart to continue.");
				}
				ForceRefresh();
				return;
			}
			
			if (button == saveButton)
			{
//				myGrid.Children[3].Visibility = Visibility.Visible;
				if (DrawObjects[ "tiyfDummyObject" ] as DrawingTools.HorizontalLine != null) {
					displayMsg("");
					int lastBarIdx = Bars.Count-1;
					ExtendAllRectangles( Bars.GetTime(lastBarIdx) );
					if ( saveButton.Content == "Save TP" )
					{
						if ( tradePlanFileName.Trim() != "" ) WriteTradePlanToFile( "inTradePlanFile" );
					}
					else // saveButton.Content == "Save Forecast"
					{
						if (addPrefixButton.Content.ToString().Contains("None")) {
							displayMsg("\nWARNING ! You must select a prefix other than \"None\". \n\nPress the \"Add prefix\" button to make a valid selection.");
						} else {
							if ( forecastingFileName.Trim() != "" ) WriteTradePlanToFile( "inForecastFile" );
							currentFilterIdx = 0;
							filterButton.Content = "Highlight: " + listOfFilters[0].Trim();
							resetForecasting();
						}
					}
				}
				ForceRefresh();
				return;
			}
			
			if (button == addPrefixButton)
			{
//				myGrid.Children[4].Visibility = Visibility.Visible;
				if (DrawObjects[ "tiyfDummyObject" ] as DrawingTools.HorizontalLine != null) {
					displayMsg("\nATTENTION !\n\nThe 'Add prefix' button allows you to specify what prefix\nwill be added to each rectangle 'Tag name' \nnext time you press the 'Save ??' button.");
					currentPrefixIdx++;
					if (currentPrefixIdx == listOfPrefixes.Count) currentPrefixIdx = 0;
					addPrefixButton.Content = "Add prefix: " + listOfPrefixes[ currentPrefixIdx ];
					
					ForceRefresh();
				}
				return;
			}
			
			if (button == hideToolBarButton)
			{
//				myGrid.Children[5].Visibility = Visibility.Visible;
				if (DrawObjects[ "tiyfDummyObject" ] as DrawingTools.HorizontalLine != null) {
					displayMsg("\nATTENTION ! The toolbar is hidden.\n\nIf you want to show it again then press the key F5\nOr right click on the chart background and select 'Reload NinjaScript'");
					myGrid.Visibility = Visibility.Hidden;
					ForceRefresh();
				}
				return;
			}
			//-
			#endregion
		}
		
		#region LOAD_TRADE_PLAN
		//-
		private void ReadTradePlanFromFile()
		{
			string path, instrumentName, filenameToLoad;
			List<string> lineElements; //holds the rectangle attributes splitted
			string aLine;
			int pos;
			
			filenameToLoad = CleanName( tradePlanFileName2.Trim(), "_" ) + ".txt";
			if (prefixFileNameWithInstrumentName2) {
				instrumentName = Bars.ToChartString();
				//keep the instrument name only
				pos = instrumentName.IndexOf(" ");
				if (pos > -1) instrumentName = instrumentName.Substring(0, pos);
			    filenameToLoad = instrumentName + "_" + filenameToLoad;
			}
			path = pathToTradePlan + filenameToLoad;
			
			DateTime rectStartTime;
			double rectStartY;
			DateTime rectEndTime;
			double rectEndY;
			string rectTagName;
			
			if ( listOfTagNamesLoaded.Count > 0 ) {
				// if a trade plan has already been loaded and recreated then remove all rectangles before loading a trade plan again
				for(int i = 0; i < listOfTagNamesLoaded.Count; i++)
				{
					if ( DrawObjects[ listOfTagNamesLoaded[i] ] as DrawingTools.Rectangle != null ) {
						RemoveDrawObject(listOfTagNamesLoaded[i]);
					}
				}
				// Reset our list
				listOfTagNamesLoaded = new List<string>();
			}
			
			try
			{
		        if (File.Exists(path))
		        {
			        using (StreamReader sr = new StreamReader(path))
			        {
			            while (sr.Peek() >= 0)
			            {
							aLine = sr.ReadLine();
							//make sure this one line has something in it
							if ( aLine.Length > 0 ) {
								lineElements = new List<string>();
								//get all the elements using TAB as delimiter
								lineElements = aLine.Split('\t').ToList();
								//make sure we have 5 elements Tag, StartAnchor.Time, StartAnchor.Price, EndAnchor.Time, EndAnchor.Price 
								if( lineElements.Count == 5 ) {
//									rectTagName = "_" + lineElements[0].TrimStart('_');
									try
									{
									rectTagName = lineElements[0];
									listOfTagNamesLoaded.Add( rectTagName );
									rectStartTime = Convert.ToDateTime( lineElements[1] );
									rectStartY = Convert.ToDouble( lineElements[2], CultureInfo.InvariantCulture );
									rectEndTime = Convert.ToDateTime( lineElements[3] );
									rectEndY = Convert.ToDouble( lineElements[4], CultureInfo.InvariantCulture );
									//let's recreate the rectangle
									DrawingTools.Rectangle myRect = Draw.Rectangle(this, rectTagName, rectStartTime, rectStartY, rectEndTime, rectEndY, rectAreaColor);
									//it is necessary to set AreaBrush again even though it is done in the Draw.Rectangle(...) command
									myRect.AreaBrush = rectAreaColor2;
									myRect.AreaOpacity = rectAreaOpacity2;
									myRect.OutlineStroke.Brush = rectOutlineStroke2.Brush;
									myRect.OutlineStroke.Width = rectOutlineStroke2.Width;
									myRect.OutlineStroke.DashStyleHelper = rectOutlineStroke2.DashStyleHelper;
									myRect.OutlineStroke.Opacity = rectOutlineStroke2.Opacity;

//			  	             		Print( sr.ReadLine() );
									}
									catch (Exception e)
									{
//										displayMsg(String.Format( "\nOne item was found in wrong format, the entry was skipped :-( \n\n {0}", e.ToString() ));
//										Print("one item was found in wrong format, the entry was skipped");
									}
								}
							}
			            }
						sr.Close();
			        }
					displayMsg("\nTrade plan '" + filenameToLoad + "' loaded successfully ! \n\n-> Select any object on the chart to continue.");
		        } else {
					displayMsg("\nWARNING ! The trade plan to load '" + filenameToLoad + "' was not found,\n\n-> check for a typo in the 'path' or 'file name' parameters. \n\nSelect or unselect any object on the chart to continue.");
				}
			}
			catch (Exception e)
			{
				displayMsg(String.Format( "\nSomething went wrong! unable to load the trade plan: {0} :-(\n\n", e.ToString() ));
			}
		}
		//-
		#endregion
		
		#region SAVE_TRADE_PLAN
		//-
		#region SAVE_TRADE_PLAN_misc_functions
		//-
		private void populateList_listOfRect(string inWhichFile)
		{
			string prefixToAdd = listOfPrefixes[ currentPrefixIdx ];
			string prefixHighlighted = listOfFilters[ currentFilterIdx ];
			List<string> tagElements;
			
			// used to filter if a rectangle has been re-created from a loaded file
			tagElements = new List<string>();
			
			//this list must be reset/emptied before the save operation
			listOfRect = new List<string>();
			
			if ( inWhichFile == "inTradePlanFile" )
			{	// saved button content == "Save TP"
		
				foreach ( DrawingTool draw in DrawObjects.ToList() )
				{
					if ( draw is DrawingTools.Rectangle )
					{
						if ( draw.IsUserDrawn )
						{
							// create a variable to hold a ref to the rectangle object beeing processed
							DrawingTools.Rectangle myRect = draw as DrawingTools.Rectangle;
							// if it is not a rectangle to skip
							if ( myRect.Tag.Trim('@').StartsWith( rectTagFilterSkip ) == false ) {
								//---------------
								string aLine = "";
								// if the left part of Tag name has already the prefixToAdd value then don't double it
								aLine += prefixToAdd + removeTxtToTheLeft( myRect.Tag, prefixToAdd ) + "\t";
								if (aLine == "\t") aLine = "Rectangle nameless\t";
								aLine += myRect.StartAnchor.Time.ToString("yyyy-MM-dd HH:mm:ss") + "\t";
								aLine += myRect.StartAnchor.Price.ToString("G", CultureInfo.InvariantCulture) + "\t";
								aLine += myRect.EndAnchor.Time.ToString("yyyy-MM-dd HH:mm:ss") + "\t";
								aLine += myRect.EndAnchor.Price.ToString("G", CultureInfo.InvariantCulture);
								listOfRect.Add( aLine );
		//						Print( aLine );
								//---------------
							}
						}
					} //end if ( draw is ...
					
				} //end foreach
				
			}
			else
			{ // saved button content == "Save Forecast"
				foreach ( DrawingTool draw in DrawObjects.ToList() )
				{
					if ( draw is DrawingTools.Rectangle )
					{
						if ( !draw.IsUserDrawn )
							// a forecast is not drawn by the user but generated automatically
							// to differentiate between new forecast Rectangles and old forecast Rectangles (from a loaded file)
							// both types are not user drawn and contains "Forecast" in their Tag so as a convention new forecast rectangles have the prefix "Rect"
						{
							// create a variable to hold a ref to the rectangle object beeing processed
							DrawingTools.Rectangle myRect = draw as DrawingTools.Rectangle;
							
							// if the rectangle starts with the prefixHighlighted and also contains the word "Forecast"
							if ( myRect.Tag.Trim('@').StartsWith( prefixHighlighted ) && myRect.Tag.Trim('@').Contains( "Forecast" ) ) {
								//---------------
								tagElements = myRect.Tag.Split('_').ToList(); //MPUpperForecast_1_637565968801105954
								if (tagElements.Count < 3) {
									string aLine = "";
									//Replace space in the Tag name with the underscore to stay consistent
									aLine = myRect.Tag.Replace(" ", "_");
									// Remove the prefixHighlighted in the Tag name: Ex. "RectUpperForecast 1" => "UpperForecast 1" before saving
									aLine = removeTxtToTheLeft( aLine, prefixHighlighted );
									//Add selected prefix and unique ID to the Tag name
									aLine = prefixToAdd + aLine + "_" + DateTime.Now.Ticks.ToString() + "\t";
									if (aLine == "\t") aLine = "Rectangle nameless\t";
									aLine += myRect.StartAnchor.Time.ToString("yyyy-MM-dd HH:mm:ss") + "\t";
									aLine += myRect.StartAnchor.Price.ToString("G", CultureInfo.InvariantCulture) + "\t";
									aLine += myRect.EndAnchor.Time.ToString("yyyy-MM-dd HH:mm:ss") + "\t";
									aLine += myRect.EndAnchor.Price.ToString("G", CultureInfo.InvariantCulture);
									listOfRect.Add( aLine );
			//						Print( aLine );
								}
								//---------------
							}
							
						}
					} //end if ( draw is ...
					
				} //end foreach
			}
			
		}

		private bool checkIfRectangleExists(string thisRect)
		{
			List<string> lineElements, lineElements2;
			lineElements = new List<string>();
			lineElements = thisRect.Split('\t').ToList();
			
			for(int i = 0; i < listOfRect2.Count; i++)
			{
				lineElements2 = new List<string>();
				//get all the elements using TAB as delimiter
				lineElements2 = listOfRect2[i].Split('\t').ToList();
				if (lineElements[0] == lineElements2[0]) {
					listOfRect2[i] = thisRect;
					return true;
				}
			}
			return false;
		}
		
		private bool populateList_listOfRect2(string path)
		{
			/// Read the previously saved content and populate the list 'listOfRect2' with it

			string aLine;
			List<string> lineElements;
			listOfRect2 = new List<string>();
			
			try
			{
		        if (File.Exists(path))
		        {
			        using (StreamReader sr = new StreamReader(path))
			        {
			            while (sr.Peek() >= 0)
			            {
							aLine = sr.ReadLine();
							//make sure this one line has something in it
							if ( aLine.Length > 0 ) {
								lineElements = new List<string>();
								//get all the elements using TAB as delimiter
								lineElements = aLine.Split('\t').ToList();
								//make sure we have 5 elements Tag, StartAnchor.Time, StartAnchor.Price, EndAnchor.Time, EndAnchor.Price 
								if( lineElements.Count == 5 ) {
									listOfRect2.Add(aLine);
								}
							}
			            }
						sr.Close();
			        }
					// did we find any valid rectangles
					if (listOfRect2.Count > 0)  { return true; } else { return false; }
		        } else {
					// file not found
					return false;
				}
			}
			catch (Exception e)
			{
				// something went wrong
				return false;
			}
		}
		
		private string removeTxtToTheLeft(string textSource, string searchItem)
		//utility function to remove some text at the beginning of a string
		{
			int pos;
			if ( (textSource != "") && (searchItem != "") ) {
				pos = textSource.IndexOf(searchItem);
				if (pos == 0) {
					return textSource.Remove(pos, searchItem.Length);
				}
			}
			return textSource;
		}
		
		private string CleanName(string aName, string replaceWith)
		//utility function to remove/replace problematic chars
		{
			string charsToRemove = " <>:\"/\\?*!@$%&()=+#'|{}[]-";
			aName = aName.Trim();
			foreach (char c in charsToRemove) 
			{
				aName = aName.Replace( "" + c, replaceWith );
			}
			return aName;
		}
		//-
		#endregion
		
		private void WriteTradePlanToFile(string inWhichFile)
		{
			string instrumentName, fileName;
			bool fileExists = false;
			bool oldContentFound;
			int pos;
			
			populateList_listOfRect( inWhichFile );
			if (listOfRect.Count == 0) {
				displayMsg("\nWARNING ! There is nothing to save. No manually drawn rectangles were found !!!\n\n-> If it is not the case, press F5 to reload the script\n-> and press the Save button again.\n\nSelect any object to continue...");
				return;
			}

			if (inWhichFile == "inTradePlanFile" )
				fileName = tradePlanFileName.Trim();
			else
				fileName = forecastingFileName.Trim();
			
			fileName = CleanName( fileName, "_" ) + ".txt";
			if (prefixFileNameWithInstrumentName) {
				instrumentName = Bars.ToChartString();
				//keep the instrument name only
				pos = instrumentName.IndexOf(" ");
				if (pos > -1) instrumentName = instrumentName.Substring(0, pos);
			    fileName = instrumentName + "_" + fileName;
			}
		
			string path = pathToTradePlan + fileName;
			try
			{
		        if (File.Exists(path))
		        {
					fileExists = true;
					oldContentFound = populateList_listOfRect2(path);
					if (oldContentFound) {
						//Replace old content with updated values if we have rectangles with the same Tag name
						for(int i = 0; i < listOfRect.Count; i++)
						{
							oldContentFound = checkIfRectangleExists( listOfRect[i] );
							if (oldContentFound) listOfRect[i] = "";
						}
					}
		            File.Delete(path);
		        }
					
		        using (StreamWriter sw = new StreamWriter(path))
		        {
					sw.WriteLine( "You can edit this file. However you must respect this structure:" );
					sw.WriteLine( "---" );
					sw.WriteLine( "1) One line per rectangle specification" );
					sw.WriteLine( "2) Each line must contain 5 items: " );
					sw.WriteLine( "-> A unique tag name" );
					sw.WriteLine( "-> The time of the starting anchor using this format yyyy-MM-dd HH:mm:ss" );
					sw.WriteLine( "-> The price of the starting anchor" );
					sw.WriteLine( "-> The time of the ending anchor using this format yyyy-MM-dd HH:mm:ss" );
					sw.WriteLine( "-> The price of the ending anchor" );
					sw.WriteLine( "3) You must separate each item by pressing the TAB key on your keyboard" );
					sw.WriteLine( "---" );
					
					if (fileExists) {
						//re-save the updated old content first
						for(int i = 0; i < listOfRect2.Count; i++)
						{
				            sw.WriteLine( listOfRect2[i] );
						}
					}
					for(int i = 0; i < listOfRect.Count; i++)
					{
						//save new content second
			            if (listOfRect[i] != "") sw.WriteLine( listOfRect[i] );
					}
					
					sw.Close();
		        }
				string strTemp = String.Format("\nTrade plan saved successfully in file: \n( {0} ).", fileName);
				if (fileExists) {
					if (inWhichFile == "inTradePlanFile" ) {
						strTemp = strTemp + "\n\n-> *Note: As the file was already created then all manually drawn rectangles were added to it.\n";
						strTemp = strTemp + "-> To remove previous content in the trade plan file, you will need to edit or delete the file manually.";
					} else {
						strTemp = strTemp + "\n\n-> *Note: As the file was already created then all forecast rectangles were added to it.\n";
						strTemp = strTemp + "-> To remove previous content in the forecast file, you will need to edit or delete the file manually.";
					}
				}
				strTemp = strTemp + "\n\nSelect and/or unselect any object on the chart to continue.";
				displayMsg(strTemp);
			}
			catch (Exception e)
			{
				displayMsg(String.Format( "\nSomething went wrong! unable to save the trade plan: {0} :-(\n\n", e.ToString() ));
			}
		}
		//-
		#endregion
		
		#region EXTEND_RECTANGLES
		//-
		private void ExtendAllRectangles(DateTime whatTime)
		{
			foreach ( DrawingTool draw in DrawObjects.ToList() )
			{
//				if ( (draw is DrawingTools.Rectangle) && draw.IsUserDrawn ) {
				if ( draw is DrawingTools.Rectangle ) {
					
					// create a variable to hold a ref to the rectangle object beeing processed
					DrawingTools.Rectangle myRect = draw as DrawingTools.Rectangle;
					// extend the rectangle to the specified time (right side of the chart)
					if ( myRect.Tag.Trim('@').StartsWith( rectTagFilterSkip ) == false ) {

						// extend to the specified time
						myRect.EndAnchor.Time = whatTime;

						int idx = 0;
						if (currentFilterIdx > -1) idx = currentFilterIdx;
						
						if ( myRect.Tag.Trim('@').StartsWith( listOfFilters[ idx ].Trim() ) )
						{	// apply the (Highlighted rectangles - settings) if Tag name starts with the current filter
							if ( !draw.IsUserDrawn ) { //and it is not a user drawn rectangle
								myRect.AreaBrush = rectAreaColor;
								myRect.AreaOpacity = rectAreaOpacity;
								myRect.OutlineStroke.Brush = rectOutlineStroke.Brush;
								myRect.OutlineStroke.Width = rectOutlineStroke.Width;
								myRect.OutlineStroke.DashStyleHelper = rectOutlineStroke.DashStyleHelper;
								myRect.OutlineStroke.Opacity = rectOutlineStroke.Opacity;
							}
						}
						else // otherwise apply the (Dimmed rectangles - settings) if Tag name does not match the current filter
						{
							if ( !draw.IsUserDrawn ) { //and it is not a user drawn rectangle
								myRect.AreaBrush = rectAreaColor2;
								myRect.AreaOpacity = rectAreaOpacity2;
								myRect.OutlineStroke.Brush = rectOutlineStroke2.Brush;
								myRect.OutlineStroke.Width = rectOutlineStroke2.Width;
								myRect.OutlineStroke.DashStyleHelper = rectOutlineStroke2.DashStyleHelper;
								myRect.OutlineStroke.Opacity = rectOutlineStroke2.Opacity;
							}
						}
					}
				} //end if ( draw is ...
			} //end foreach
			ForceRefresh();
		} //end ExtendAllRectangles
		//-
		#endregion
		
		#region FORECASTING_AREAS
		//-
		private void resetForecasting()
		{
			for(int i = 0; i < listOfTagNamesForecasted.Count; i++)
			{
				if ( DrawObjects[ listOfTagNamesForecasted[i] ] as DrawingTools.Rectangle != null ) {
					RemoveDrawObject(listOfTagNamesForecasted[i]);
				}
			}
			nbAreasForecasted = 0; //reset the count of forecasted areas when a new category of Rectangles is Highlighted
			forecastButton.Content = "Forecast(" + nbAreasForecasted + ") > " + listOfFilters[ currentFilterIdx ];
			saveButton.Content = "Save TP";
			saveButton.ToolTip = "Save all manually drawn rectangles into the specified file as defined in the 'Save settings' section";
			addPrefixButton.ToolTip = "The selected prefix will be added to the Tag name of all manually drawn rectangles\nnext time you press the 'Save TP' button.\n\nClick this button repeatedly to select a different prefix.";
			listOfPriorDrawnRectanglesByUser = new List<Tuple<string, double, double>>();
		}

		private void foreCastingAreas( bool fromLoadedTP )
		{
			//Tuple< item1=Tag, item2=LowestAnchor.Price, item3=HighestAnchor.Price, item4=rectMidPoint, item5=rectStartTime, item6=rectEndTime>
			var listOfRectanglesToSort = new List<Tuple<string, double, double, double, DateTime, DateTime>>();
			double AllRectAverageSize = 0;
			double thisRectAverageSize;
			double rangeBetweenRect;
			double averageRangeBetweenRect = 0;
			double rectMidPoint;
			DateTime rectStartTime;
			double rectStartY;
			DateTime rectEndTime;
			double rectEndY;
			string rectTagName;
			string highlightedRectToForecast = listOfFilters[ currentFilterIdx ];
			int x;
			bool differenceFound;
			
			
			displayMsg("\nForecast based on Highlighted prefix > " + listOfFilters[ currentFilterIdx ] );
			
			// create a list to record all highlighted rectangles and put the lowest anchor price first (this will be referred to by item2)
			if ( fromLoadedTP ) {
				// Deal with rectangles from a loaded TP
				for(int i = 0; i < listOfTagNamesLoaded.Count; i++)
				{
					if ( DrawObjects[ listOfTagNamesLoaded[i] ] as DrawingTools.Rectangle != null ) {
						dynamic myRect = DrawObjects[ listOfTagNamesLoaded[i] ];
						//add highlighted rectangle to the list
						
						if ( myRect.Tag.StartsWith( highlightedRectToForecast) ) {
							if (myRect.StartAnchor.Price < myRect.EndAnchor.Price) {
								rectMidPoint = myRect.StartAnchor.Price + Math.Abs(myRect.StartAnchor.Price - myRect.EndAnchor.Price) / 2 ;
								listOfRectanglesToSort.Add( Tuple.Create( listOfTagNamesLoaded[i], myRect.StartAnchor.Price, myRect.EndAnchor.Price, rectMidPoint, myRect.StartAnchor.Time, myRect.EndAnchor.Time ) );
							} else {
								rectMidPoint = myRect.EndAnchor.Price + Math.Abs(myRect.StartAnchor.Price - myRect.EndAnchor.Price) / 2 ;
								listOfRectanglesToSort.Add( Tuple.Create( listOfTagNamesLoaded[i], myRect.EndAnchor.Price, myRect.StartAnchor.Price, rectMidPoint, myRect.StartAnchor.Time, myRect.EndAnchor.Time ) );
							}
						}
					}
				}
				//
			}
			else
			{
				// Deal with user drawn rectangles
				foreach(DrawingTool drawTool in DrawObjects)
				{
					// only apply logic below to drawing objects of type "Rectangle")
					if(drawTool.GetType().ToString().Contains("Rectangle"))
					{
					    if( drawTool.IsUserDrawn )
					    {
					        // safely cast as dynamic type at run-time
					        dynamic myRect = drawTool;
							
							if ( myRect.Tag.StartsWith( "Rect") ) {
								if (myRect.StartAnchor.Price < myRect.EndAnchor.Price) {
									rectMidPoint = myRect.StartAnchor.Price + Math.Abs(myRect.StartAnchor.Price - myRect.EndAnchor.Price) / 2 ;
									listOfRectanglesToSort.Add( Tuple.Create( myRect.Tag, myRect.StartAnchor.Price, myRect.EndAnchor.Price, rectMidPoint, myRect.StartAnchor.Time, myRect.EndAnchor.Time ) );
								} else {
									rectMidPoint = myRect.EndAnchor.Price + Math.Abs(myRect.StartAnchor.Price - myRect.EndAnchor.Price) / 2 ;
									listOfRectanglesToSort.Add( Tuple.Create( myRect.Tag, myRect.EndAnchor.Price, myRect.StartAnchor.Price, rectMidPoint, myRect.StartAnchor.Time, myRect.EndAnchor.Time ) );
								}
							}
					    }
					}
				}
				//
			}
			
			if (listOfRectanglesToSort.Count > 1)
			{
				//sort rectangles based on item2 of the Tuple (item2 = lowest anchor price)
				listOfRectanglesToSort = listOfRectanglesToSort.OrderBy(i => i.Item2).ToList();
				
				if (nbAreasForecasted == 0) {
					listOfTagNamesForecasted = new List<string>();
				} else {
					//listOfPriorDrawnRectanglesByUser = new List<Tuple<string, double, double>>();
					//if there is a change between the previous series of user drawn rectangles used to forecast and the new series of user drawn rectangles
					//then we must reset the Forecast function
					if ( !fromLoadedTP ) {
						differenceFound = false;
						if ( listOfRectanglesToSort.Count != listOfPriorDrawnRectanglesByUser.Count ) {
							differenceFound = true;
						} else {
							x = 0;
							for (int i = 0; i < listOfPriorDrawnRectanglesByUser.Count; i++)
							{
								if ( listOfRectanglesToSort[i].Item1 == listOfPriorDrawnRectanglesByUser[i].Item1 )
								{
									x++;
									if ( listOfRectanglesToSort[i].Item2 != listOfPriorDrawnRectanglesByUser[i].Item2 )
									{
										differenceFound = true;
										break;
									}
									if ( listOfRectanglesToSort[i].Item3 != listOfPriorDrawnRectanglesByUser[i].Item3 )
									{
										differenceFound = true;
										break;
									}
								}
							}
							if (x != listOfPriorDrawnRectanglesByUser.Count) differenceFound = true;
						}
						if (differenceFound) resetForecasting();
					}
				}
				if ( !fromLoadedTP ) listOfPriorDrawnRectanglesByUser = new List<Tuple<string, double, double>>();
				for (int i = 0; i < listOfRectanglesToSort.Count; i++) 
				{
					/// item2=LowestAnchor.Price, item3=HighestAnchor.Price
					
					thisRectAverageSize = listOfRectanglesToSort[i].Item3 - listOfRectanglesToSort[i].Item2;
					AllRectAverageSize = AllRectAverageSize + thisRectAverageSize;
					
					if (i > 0) {
						rangeBetweenRect = listOfRectanglesToSort[i].Item2 - listOfRectanglesToSort[i-1].Item3;
						averageRangeBetweenRect = averageRangeBetweenRect + rangeBetweenRect;
					}
					if ( !fromLoadedTP ) listOfPriorDrawnRectanglesByUser.Add( Tuple.Create( listOfRectanglesToSort[i].Item1, listOfRectanglesToSort[i].Item2, listOfRectanglesToSort[i].Item3) );
				}
				
				AllRectAverageSize = AllRectAverageSize / listOfRectanglesToSort.Count;
				averageRangeBetweenRect = averageRangeBetweenRect / (listOfRectanglesToSort.Count-1);
				
				nbAreasForecasted++; // increase the count (nb of times the Forecast button has been pressed)
				forecastButton.Content = "Forecast(" + nbAreasForecasted + ") > " + listOfFilters[ currentFilterIdx ];
				saveButton.Content = "Save Forecast";
				saveButton.ToolTip = "Save all forecast rectangles into the specified file as defined in the 'Save settings' section";
				addPrefixButton.ToolTip = "The selected prefix will be added to the Tag name of all forecast rectangles\nnext time you press the 'Save Forecast' button.\n\nClick this button repeatedly to select a different prefix.";
				
				//zones created above the top most rectangle of the category to forecast
				x = listOfRectanglesToSort.Count-1;
				rectStartTime = listOfRectanglesToSort[x].Item5;
				rectEndTime = listOfRectanglesToSort[x].Item6;
				rectStartY = listOfRectanglesToSort[x].Item3 + ((averageRangeBetweenRect * nbAreasForecasted) + (AllRectAverageSize * (nbAreasForecasted-1)));
				rectEndY = rectStartY + AllRectAverageSize;
				rectTagName = listOfFilters[ currentFilterIdx ] + "UpperForecast " + nbAreasForecasted;
				listOfTagNamesForecasted.Add(rectTagName);
				DrawingTools.Rectangle topForecastRect = Draw.Rectangle(this, rectTagName, rectStartTime, rectStartY, rectEndTime, rectEndY, rectAreaColor);
				
				//zones created below the rock bottom rectangle of the category to forecast
				x = 0;
				rectStartTime = listOfRectanglesToSort[x].Item5;
				rectEndTime = listOfRectanglesToSort[x].Item6;
				rectStartY = listOfRectanglesToSort[x].Item2 - ((averageRangeBetweenRect * nbAreasForecasted) + (AllRectAverageSize * (nbAreasForecasted-1)));
				rectEndY = rectStartY - AllRectAverageSize;
				rectTagName = listOfFilters[ currentFilterIdx ] + "LowerForecast " + nbAreasForecasted;
				listOfTagNamesForecasted.Add(rectTagName);
				DrawingTools.Rectangle bottomForecastRect = Draw.Rectangle(this, rectTagName, rectStartTime, rectStartY, rectEndTime, rectEndY, rectAreaColor);
			}
			else
			{
				displayMsg("\nWARNING ! Nothing to forecast.\n\nTo forecast future areas, we need at least 2 rectangles of the same category (i.e. tag name starts with the same prefix).\n\nPress the \"Highlight\" button repeatedly to find an existing category.\n\nPlease note that the prefix \"Rect\" identifies user drawn rectangles. \nConversely, all other prefixes apply to rectangles re-created from a loaded file.");
			}
		}// end foreCastingAreas()
		//-
		#endregion
		
		private void displayMsg( string txtSource )
		{
			Draw.TextFixed(this, "tiyf_MessageTextToUser2", txtSource, TextPosition.TopLeft);
		}

        protected override void OnBarUpdate()
        {
			try
			{
				if (State == State.Historical) {
					return;
				}

				ExtendAllRectangles( Time[0] );
				
				// REQUIRED: necessary to assign any value so strategies using this indicator the
	            // OnBarUpdate will be called in this indicator.
	 			base.Value[0] = double.NaN;
			}
			catch (Exception e)
			{
				displayMsg(String.Format( "\nSomething went wrong! :-( \n\n {0}", e.ToString() ));
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			string strTemp;
			int lastBarIdx;
			double lastBarClose, startYDistToClose, endYDistToClose;
			
			// call the base.OnRender() to ensure standard Plots work as designed
			base.OnRender(chartControl, chartScale);
			
			foreach(DrawingTool drawTool in DrawObjects)
			{
				// only apply logic below to types of "Rectangle")
				if(drawTool.GetType().ToString().Contains("Rectangle"))
				{ // if the user click on a rectangle from a loaded trade plan then display its Tag name
//				    if( !drawTool.IsUserDrawn && drawTool.IsSelected ) // version 1d was restricted to Rect from a loaded TP
				    if( drawTool.IsSelected )
				    {
				        // safely cast as dynamic type at run-time
				        dynamic myRect = drawTool;
						
						lastBarIdx = Bars.Count-1;
						lastBarClose = Bars.GetClose(lastBarIdx);
						
						startYDistToClose = Math.Abs( myRect.StartAnchor.Price - lastBarClose ); //distance of anchor 1 to last bar close
						endYDistToClose = Math.Abs( myRect.EndAnchor.Price - lastBarClose ); //distance of anchor 2 to last bar close
						startYDistToClose = Instrument.MasterInstrument.RoundToTickSize( startYDistToClose );
						endYDistToClose = Instrument.MasterInstrument.RoundToTickSize( endYDistToClose );
						
						strTemp =  "StartY to " + lastBarClose.ToString() + "-> " + startYDistToClose.ToString() + "\n";
						strTemp =  strTemp + "EndY to " + lastBarClose.ToString() + "-> " + endYDistToClose.ToString() + "\n";
						
						displayMsg( "\n" + drawTool.Tag + "\n" + strTemp );
				    }
				}
			}
			
			if(!IsInHitTest) return;

			if (State == State.Realtime) {
				displayMsg("");
				if (DrawObjects[ "tiyfDummyObject" ] as DrawingTools.HorizontalLine != null) {
					lastBarIdx = Bars.Count-1;
					ExtendAllRectangles( Bars.GetTime(lastBarIdx) );
				}
			}
		}
		
		#region Properties
///				=== General
		
				[ReadOnly(true)]
				[Display(Name = "Indicator info", GroupName = "General", Order = 0)]
				public string Indicator_info
				{
					get { return FullSystemName; }
					set { }
				}

				[Display( Name = "Path to trade plan (abbrv. TP)", GroupName = "General", Description = "Path to the directory where the Trade Plan will be saved to/loaded from", Order = 1 )]
				public string pathToTradePlan
				{ get; set; }

				[Display(Name="Ignore rectangle if Tag name starts with", GroupName="General", Description = "This option allows a rectangle to be free from the control of the indicator", Order = 2)]
				public String rectTagFilterSkip
				{ get; set; }

///				=== Highlighted rectangles - settings				

				[Display(Name="Highlight if Tag name starts with (comma separated)", GroupName="Highlighted rectangles - settings", Description = "List of prefixes (comma separated). Used as filter to highlight specific rectangles", Order = 0)]
				public String rectTagFilter
				{ get; set; }

				[XmlIgnore()]
				[Display(Name = "Color - Area", GroupName = "Highlighted rectangles - settings", Order = 1)]
				public Brush rectAreaColor
				{ get; set; }
				[Browsable(false)] //prevents this property from showing up on the UI
		        public string rectAreaColorSerializable
		        {
		            get { return Serialize.BrushToString(rectAreaColor); }
		            set { rectAreaColor = Serialize.StringToBrush(value); }
		        }
				
				[Range(0, 100)]
				[Display(Name="Opacity - Area %", GroupName="Highlighted rectangles - settings",  Order = 2)]
				public int rectAreaOpacity
				{ get; set; }

				[Display(Name="Outline", GroupName="Highlighted rectangles - settings",  Order = 3)]
				public NinjaTrader.Gui.Stroke rectOutlineStroke
				{ get; set; }

///				=== Dimmed rectangles - settings	

				[XmlIgnore()]
				[Display(Name = "Color - Area", GroupName = "Dimmed rectangles - settings", Order = 0)]
				public Brush rectAreaColor2
				{ get; set; }
				[Browsable(false)] //prevents this property from showing up on the UI
		        public string rectAreaColor2Serializable
		        {
		            get { return Serialize.BrushToString(rectAreaColor2); }
		            set { rectAreaColor2 = Serialize.StringToBrush(value); }
		        }
				
				[Range(0, 100)]
				[Display(Name="Opacity - Area %", GroupName="Dimmed rectangles - settings",  Order = 1)]
				public int rectAreaOpacity2
				{ get; set; }

				[Display(Name="Outline", GroupName="Dimmed rectangles - settings",  Order = 2)]
				public NinjaTrader.Gui.Stroke rectOutlineStroke2
				{ get; set; }
				
///				=== Trade plan (TP) - Save settings				
				
				[Display( Name = "Trade Plan filename (without the .txt suffix file type)", GroupName = "Trade plan (TP) - Save settings", Description = "Specify a file name without the .txt suffix file type for your Trade Plan. ", Order = 0 )]
				public string tradePlanFileName
				{ get; set; }

				[Display( Name = "Forecasting filename (without the .txt suffix file type)", GroupName = "Trade plan (TP) - Save settings", Description = "Specify a file name without the .txt suffix file type for your Forecasts. ", Order = 1 )]
				public string forecastingFileName
				{ get; set; }

				[Display( Name = "Add the Instrument name to the file name at save time", GroupName = "Trade plan (TP) - Save settings", Description = "If checked, the Chart's instrument name will be added as prefix to the file name before saving", Order = 2 )]
				public bool prefixFileNameWithInstrumentName
				{ get; set; }
				
///				=== Trade plan (TP) - Load settings
				
				[Display( Name = "File name of TP to load (without the .txt suffix file type)", GroupName = "Trade plan (TP) - Load settings", Description = "Specify a file name without the .txt suffix file type.", Order = 0 )]
				public string tradePlanFileName2
				{ get; set; }

				[Display( Name = "Add the Instrument name to the file name at load time", GroupName = "Trade plan (TP) - Load settings", Description = "If checked, the Chart's instrument name will be added as prefix to the file name before loading", Order = 1 )]
				public bool prefixFileNameWithInstrumentName2
				{ get; set; }
				
///				=== Toolbar - settings
				
				[XmlIgnore()]
				[Display(Name = "button foreground color", GroupName = "Toolbar - settings", Order = 0)]
				public Brush buttonForegroundColor
				{ get; set; }
				[Browsable(false)] //prevents this property from showing up on the UI
		        public string buttonForegroundColorSerializable
		        {
		            get { return Serialize.BrushToString(buttonForegroundColor); }
		            set { buttonForegroundColor = Serialize.StringToBrush(value); }
		        }

				[XmlIgnore()]
				[Display(Name = "button Background color", GroupName = "Toolbar - settings", Order = 1)]
				public Brush buttonBackgroundColor
				{ get; set; }
				[Browsable(false)] //prevents this property from showing up on the UI
		        public string buttonBackgroundColorSerializable
		        {
		            get { return Serialize.BrushToString(buttonBackgroundColor); }
		            set { buttonBackgroundColor = Serialize.StringToBrush(value); }
		        }

				[Display(Name = "Toolbar's vertical aligment", GroupName = "Toolbar - settings", Description="Choose the Vertical Alignment", Order = 2)]
				public VerticalAlignment VAlignment
				{
					get { return vAlignment; }
					set { vAlignment = value; }
				}

				[Display(Name = "Toolbar's horizontal aligment", GroupName = "Toolbar - settings", Description="Choose the horizontal Alignment", Order = 3)]
				public HorizontalAlignment HAlignment
				{
					get { return hAlignment; }
					set { hAlignment = value; }
				}

		#endregion
				
    } //end public class tiyfTradePlanFactory : Indicator
} //end NinjaTrader.NinjaScript.Indicators.TrendIsYourFriend

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TrendIsYourFriend.tiyfTradePlanFactory[] cachetiyfTradePlanFactory;
		public TrendIsYourFriend.tiyfTradePlanFactory tiyfTradePlanFactory()
		{
			return tiyfTradePlanFactory(Input);
		}

		public TrendIsYourFriend.tiyfTradePlanFactory tiyfTradePlanFactory(ISeries<double> input)
		{
			if (cachetiyfTradePlanFactory != null)
				for (int idx = 0; idx < cachetiyfTradePlanFactory.Length; idx++)
					if (cachetiyfTradePlanFactory[idx] != null &&  cachetiyfTradePlanFactory[idx].EqualsInput(input))
						return cachetiyfTradePlanFactory[idx];
			return CacheIndicator<TrendIsYourFriend.tiyfTradePlanFactory>(new TrendIsYourFriend.tiyfTradePlanFactory(), input, ref cachetiyfTradePlanFactory);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TrendIsYourFriend.tiyfTradePlanFactory tiyfTradePlanFactory()
		{
			return indicator.tiyfTradePlanFactory(Input);
		}

		public Indicators.TrendIsYourFriend.tiyfTradePlanFactory tiyfTradePlanFactory(ISeries<double> input )
		{
			return indicator.tiyfTradePlanFactory(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TrendIsYourFriend.tiyfTradePlanFactory tiyfTradePlanFactory()
		{
			return indicator.tiyfTradePlanFactory(Input);
		}

		public Indicators.TrendIsYourFriend.tiyfTradePlanFactory tiyfTradePlanFactory(ISeries<double> input )
		{
			return indicator.tiyfTradePlanFactory(input);
		}
	}
}

#endregion
