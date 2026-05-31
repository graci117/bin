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

using SharpDX.DirectWrite;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

using NinjaTrader.Core;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.BobC
{
	public class ADXVMAButtonBand : Indicator
	{   private int 		Period 			= 5;
		private double		k				= 0.0;
		private double		hhp				= 0.0;
		private double		llp				= 0.0;
		private double		hhv				= 0.0;
		private double		llv				= 0.0;
		private double 		epsilon			= 0.0;
		private bool		showPlot		= true;
		private bool		showPaintBars	= true;
		private bool		candles			= true;
		private int			opacity			= 4;
		private int			alpha			= 0;
		private int 		plot0Width 		= 4;
		
		private double[] 	up;
		private double[]  	down;
		private double[]  	ups;
		private double[]  	downs;
		
		private Series<double> index;
		public double[] _ADXVMAColorBand2;
		private int[]	trend;
		//private ATR volatility; 
		private double[] Volatility;
		private double prevInput=0;
		private int vPeriod=200;
		private double rawRange, trueRange,P1;
		private bool ShowPlot =true;
		private bool LoadFlag=true;
		private DateTime Start;
	    private TimeSpan BackfillTime;
		private int cbr=0;
		//for keltner
		private Series<double>		diff;
		private	SMA					smaDiff;
		private	SMA					smaTypical;
		
		NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 11) { Size = 11, Bold = false };
		
		///button
		private bool 			        installGrid;
		private ChartControl 			cc;
		private SolidColorBrush 		textBrush;
		private Grid					myGrid;
		private							SessionIterator		sessionIterator;
		private int						dataTypeInt			= 0;
		
		
		protected override void OnStateChange()
		{
		#region	Set Defaults and Plots
			if (State == State.SetDefaults)
			{
				Description							= @"adapted from anaADXVMA for NT7";
				Name								= "ADXVMAButtonBand";
		//		AddPlot(Brushes.Red, "adx vma 0");
				_Period                             = 5;
				Textoffset 							= 1;
				Calculate							= Calculate.OnPriceChange;
				IsOverlay							= true;
				DisplayInDataBox					= false;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive			= true;
				OffsetMultiplier					= 1.618;
				OuterOffsetMultiplier				= 3.1415926;

				AddPlot(Brushes.DarkGray,		NinjaTrader.Custom.Resource.KeltnerChannelMidline);
				AddPlot(Brushes.Yellow,			NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddPlot(Brushes.Yellow,			"OuterUpper");
				AddPlot(Brushes.Yellow,			NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddPlot(Brushes.Yellow,			"OuterLower");
				Plots[0].Width = 5;				//adxvma Main
				Plots[1].Width = 2;				//adxvma Upper
				Plots[2].Width = 2;				//adxvma UpperOuter
				Plots[3].Width = 2;				//adxvma Lower
				Plots[4].Width = 2;				//adxvma LowerOuter	
				adxvmaRisingBrush = Brushes.Blue;
				adxvmaFallingBrush = Brushes.Red;
				adxvmaNeutralBrush = Brushes.Yellow;
				
				ColumnNum					= 3;
				
				//ButtonText					= priorType.ToString();
				installGrid					= true;
				ShowButton					= true;
				ButtonBrush					= Brushes.Yellow;
			}
			else if (State == State.Configure)
			{       Period=_Period;
				    k = Math.Round(1.0/(double)Period,6);
				    up = new double[2];
					down = new double[2];
					ups = new double[2];
					downs = new double[2];
				    trend = new int[2];
					index = new Series<double>(this);
					
					Volatility= new double[2];
					_ADXVMAColorBand2=new double[2];
				   
				    Start=Core.Globals.Now;
				    LoadFlag=true;
				    cbr=ChartBars.ToIndex;
				
				diff				= new Series<double>(this);
				if (diff.Count > 0)
				{
				smaDiff				= SMA(diff, Period);
				smaTypical			= SMA(Typical, Period);
				}
				
				adxvmaRisingBrush.Freeze();				//added 3/16/17 after Zondor comments
				adxvmaFallingBrush.Freeze();
				adxvmaNeutralBrush.Freeze();
			}
			else if (State == State.DataLoaded)
            {

            }
				else if (State == State.Historical)
			{
				if (ShowButton)
					InstallButton();
				
				if (sessionIterator == null)
					sessionIterator = new SessionIterator(BarsArray[dataTypeInt]);
			}
			else if (State == State.Terminated)
			{
				if (ShowButton)
					RemoveButton();
			}
			
		#endregion	
		}
		
			#region	Button
		
		protected void	InstallButton()
		{
			try
			{
				cc = this.ChartControl;
				
				if (cc != null)
				{
					this.Dispatcher.BeginInvoke((Action)(() => { 
						
						
						// find the chart window and subscribe to the tab control selection change event
						Chart myChart = Window.GetWindow(cc.Parent) as Chart;
						if (myChart != null) myChart.MainTabControl.SelectionChanged += MySelectionChangedHandler;
					
						if (installGrid)
						{
						
						foreach (DependencyObject item in myChart.MainMenu)
								{
									if (AutomationProperties.GetAutomationId(item) == "addGrid")
									{
										installGrid = false;
									}
								}
						}
						if (installGrid)
						{
							InstallGrid();
						}
					}));
				}
			}
			catch(Exception ex)
			{
				Print(ex.Message.ToString() + ": ADXVMAButtonBand");
			}
		}
		protected void	RemoveButton()
		{
			this.Dispatcher.InvokeAsync((Action)(() => {
				try
				{
		            if (this.cc != null)
					{
						if (this.UserControlCollection != null)
						{
							this.cc.Children.Remove(myGrid); //???
							this.UserControlCollection.Remove(myGrid);
							
		                    this.myGrid.Children.Clear();
							this.myGrid = null;
							// unsubscribe
							Chart myChart = Window.GetWindow(cc.Parent) as Chart;
							if (myChart != null) myChart.MainTabControl.SelectionChanged -= MySelectionChangedHandler;
						}
						this.cc = null;
					}
				}
				catch(Exception ex)
				{
					Print(ex.Message.ToString() + ": ADXVMAButtonBand");
				}
			}));
		}
		
		private void InstallGrid()
		{
			try
			{	
				// Create a grid to house the buttons
				myGrid = new System.Windows.Controls.Grid
		        {
		            Name = "buttonGrid",
		            // Align the control to the top left corner of the chart.
		            HorizontalAlignment = HorizontalAlignment.Left,
		            VerticalAlignment = VerticalAlignment.Bottom,								//Top, Middle, Bottom
		        };
				
				 // Make only as many columns as needed in the grid
				ColumnDefinition[] colDef	= new ColumnDefinition[ColumnNum];
				
				for (int i = 0; i <= colDef.Length - 1; i++)
				{
					colDef[i]	= new ColumnDefinition();
					colDef[i].Width = new GridLength(60); //60
					myGrid.ColumnDefinitions.Add(colDef[i]);
				}
				
				 
				// Create Rows   
				RowDefinition gridRow1 = new RowDefinition();  
				gridRow1.Height = new GridLength(37);  //37
				
				// Set the text color to white or black depending on the darkness/lightness of the button color
				textBrush = (SolidColorBrush) ButtonBrush;
				textBrush = (textBrush.Color.R + textBrush.Color.G + textBrush.Color.B) > 382f ? Brushes.Black : Brushes.White;
				textBrush.Freeze();
				 
				// Format cell text
			    TextBlock txtBlock1 = new TextBlock();  
			    txtBlock1.Text = "ADXVMA".ToString(); //.Remove(1,4); //ButtonText;
			    txtBlock1.FontSize = 10;
			    txtBlock1.FontWeight = FontWeights.Bold;  
			    txtBlock1.Foreground = textBrush;
			    txtBlock1.VerticalAlignment = VerticalAlignment.Center; 
				txtBlock1.HorizontalAlignment = HorizontalAlignment.Center;
			
				// Create button 
				Button btn1 = new Button
		         {
		            Name = "Button1",
		            Content = txtBlock1,
		            Foreground = textBrush,
		            Background = ButtonBrush,
					HorizontalAlignment = HorizontalAlignment.Center,
					HorizontalContentAlignment = HorizontalAlignment.Center,
					VerticalContentAlignment = VerticalAlignment.Center,
					//Margin = new Thickness(1.0,1.0,1.0,1.0),
					Padding	= new Thickness(1.0,1.0,1.0,1.0),
					MinWidth	= 60d, //column width
					//MinHeight	= RowHeight
		         };
				 btn1.Click += btn_Click;
				 
				// Define where the button should appear in the grid
		        System.Windows.Controls.Grid.SetColumn(btn1, Math.Max(0, colDef.Length));
		        //System.Windows.Controls.Grid.SetRow(btn1, Math.Max(0, RowNum - 1)); // set to max 1 at this stage
				
				// Add the button to the grid
				myGrid.Children.Add(btn1);
				// Add the grid to the chart panel
				UserControlCollection.Add(myGrid);
				//cc.Children.Add(myGrid);
				 
				AutomationProperties.SetAutomationId(myGrid, "addGrid");
	           
				this.cc.InvalidateVisual();
			}
			catch(Exception ex)
			{
				Print(ex.Message.ToString());
			}
		}
		/// <summary>
		/// When we click on a new tab, make sure the grid/buttons do not appear on the new tabbed chart
		/// </summary>
		private void MySelectionChangedHandler(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;
						
			ChartTab temp = (e.AddedItems[0] as TabItem).Content as ChartTab;
			if (temp != null)
			{	
				// set grid/button visiblity based on selected tab
				if(myGrid != null)
					myGrid.Visibility = temp.ChartControl == ChartControl ? Visibility.Visible : Visibility.Collapsed;
			}
		}	
		/// <summary>
		/// When the button is clicked, make the indicator visible or invisible and refresh the chart
		/// </summary>
        protected void btn_Click(object sender, RoutedEventArgs rea)
        {
			IsVisible = IsVisible == true ? false : true;
			ForceRefresh();
        }
		
		#endregion
		protected override void OnBarUpdate()
		{
			if (BarsArray[dataTypeInt] == null || CurrentBars[dataTypeInt] <= 0 || CurrentBars[0] <= 0)
					return;
			
			if(!IsFirstTickOfBar && prevInput==Input[0])return;prevInput=Input[0];
		      if(CurrentBar==cbr && LoadFlag)
		    {   LoadFlag=false;	
				BackfillTime=Core.Globals.Now-Start;
				int s=BackfillTime.Seconds;
				int f=BackfillTime.Milliseconds;
				string F=f.ToString("000");
				cbr=CurrentBar;
				string replay="";
				if(IsTickReplays[0]==true) replay=" (Tick Replay Mode) "; else replay =" (Non Tick Replay Mode) ";
			//	Print("ADXVMAColorBand2, efficiently coded for NT8 by Zondor, has backfilled "
			//	+CurrentBar.ToString("#,000")	+" bars, on "); 
			//	Print("   "+Bars.BarsPeriod+" chart of "+Instrument.FullName+" "+replay+", in "+s+"."+F
			//	+" seconds."); 
			//	Print(" Thank you for using the ADXVMAColorBand2. You are sooo awesome :) .");
				
		    }
		    	
			if( CurrentBar < 1 )
			{
				up[0]=(0.0);
				down[0]=(0.0);
				ups[0]=(0.0);
				downs[0]=(0.0);
				index[0]=(0.0);
				trend[0]=(0);
				return;
					
				
			}
			if (CurrentBar <Period)
				{
					Values[0][0]=Median[0]; _ADXVMAColorBand2[0]=Median[0]; _ADXVMAColorBand2[1]=Median[1];
				}
			else
			{
			//	double trueRange = High[0] - Low[0];
				if(IsFirstTickOfBar) 
				 {   up[1]=up[0];       ups[1]=ups[0]; 
				     down[1]=down[0];   downs[1]=downs[0];
					
					 _ADXVMAColorBand2[1]         = _ADXVMAColorBand2[0];
					 Volatility[1]      = Volatility[0];
					 trend[1]           = trend[0];
				     P1=(vPeriod - 1 ) * Values[0][1];
				     rawRange=High[0]-Low[0];
				     trueRange = Math.Max(Math.Abs(Low[0] - Close[1]), Math.Max(rawRange, Math.Abs(High[0] - Close[1])));
				     Volatility[0]=(P1 + trueRange) / vPeriod;
				 }
				 
		    }
		
		
		
			#region Calculate ADXVMA
			{
				double currentUp = Math.Max(Input[0] - Input[1], 0);
             	double currentDown = Math.Max(Input[1] - Input[0], 0);
			    up[0]= ((1-k)*up[1] + k*currentUp);
				down[0]=((1-k)*down[1] + k*currentDown);
				
				double sum = up[0] + down[0];
				double fractionUp = 0.0;
				double fractionDown = 0.0;
				if(sum > double.Epsilon)
				{
					fractionUp = up[0]/sum;
					fractionDown = down[0]/sum;
				}
				ups[0]=((1-k)*ups[1] + k*fractionUp);
				downs[0]=((1-k)*downs[1] + k*fractionDown);
				
				double normDiff = Math.Abs(ups[0] - downs[0]);
				double normSum = ups[0] + downs[0];
				double normFraction = 0.0;
				if(normSum > double.Epsilon)
					normFraction = normDiff/normSum;
				index[0]=((1-k)*index[1] + k*normFraction);
				
            	if(IsFirstTickOfBar)
				{
					epsilon = 0.1 * Volatility[1];
					hhp = MAX(index,Period)[1];
					llp = MIN(index,Period)[1];
				}	
				hhv = Math.Max(index[0],hhp);
				llv = Math.Min(index[0],llp);
				
				double vDiff = hhv-llv;
				double vIndex = 0;
				if(vDiff > double.Epsilon)
					vIndex = (index[0] - llv)/vDiff;
				
				_ADXVMAColorBand2[0]=((1 - k*vIndex)*_ADXVMAColorBand2[1] + k*vIndex*Input[0]);
	//		Values[0][0]=_ADXVMAColorBand2[0];
			#endregion
			#region Apply Volatility Bands and Outer Bands to ADXVMA	 
			diff[0]				= High[0] - Low[0];
		//	diff.Set(High[0] - Low[0]);

			double middle		= _ADXVMAColorBand2[0];
			double offset		= amaADXVMA(diff, Period)[0] * OffsetMultiplier;
			double outeroffset	= amaADXVMA(diff, Period)[0] * OuterOffsetMultiplier;   
			double upper		= middle + offset;
			double outerupper	= middle + outeroffset;	 
			double lower		= middle - offset;
			double outerlower	= middle - outeroffset;
				 
			Midline[0]		= middle;
			Upper[0]		= upper;
			OuterUpper[0]	= outerupper;	 
			Lower[0]		= lower;
			OuterLower[0]	= outerlower;
			#endregion	 
	//Print("vDiff" + vDiff);
				
		 	#region Adjust ADXVMA Line Color and Plot
			
			if (_ADXVMAColorBand2[0] > _ADXVMAColorBand2[1])
				{PlotBrushes[0][0] = adxvmaRisingBrush;
				PlotBrushes[1][0] = adxvmaRisingBrush;	
				PlotBrushes[2][0] = adxvmaRisingBrush;	
				PlotBrushes[3][0] = adxvmaRisingBrush;	
				PlotBrushes[4][0] = adxvmaRisingBrush;		
				Draw.Text(this, "tag1", false, "ADXVMA"+(Period), -Textoffset, _ADXVMAColorBand2[0], 0, PlotBrushes[0][0], myFont, System.Windows.TextAlignment.Center, Brushes.Transparent, null, 1);
			//	CellConditionPlot [0] = 1;
				}
			else if (_ADXVMAColorBand2[0] < _ADXVMAColorBand2[1])
				{PlotBrushes[0][0] = adxvmaFallingBrush;
				PlotBrushes[1][0] = adxvmaFallingBrush;	
				PlotBrushes[2][0] = adxvmaFallingBrush;	
				PlotBrushes[3][0] = adxvmaFallingBrush;	
				PlotBrushes[4][0] = adxvmaFallingBrush;		
			//	CellConditionPlot [0] = -1;
				Draw.Text(this, "tag1", false, "ADXVMA"+(Period), -Textoffset, _ADXVMAColorBand2[0], 0, PlotBrushes[0][0], myFont, System.Windows.TextAlignment.Center, Brushes.Transparent, null, 1);
				}	
			else
				{PlotBrushes[0][0] = adxvmaNeutralBrush;
				Draw.Text(this, "tag1", false, "ADXVMA"+(Period), -Textoffset, _ADXVMAColorBand2[0], 0, PlotBrushes[0][0], myFont, System.Windows.TextAlignment.Center, Brushes.Transparent, null, 1);
			//	CellConditionPlot [0] = 0;
			//	Draw.Region("Region2", CurrentBar , 0, Upper, OuterUpper, Brushes.Transparent, Brushes.Yellow,1);
			//	Draw.Region(this, "tag1", CurrentBar, 0, ADXVMAColorBand2(8,1.618,3.14).OuterUpper, ADXVMAColorBand2(8,1.618,3.14).Upper, null, Brushes.Blue, 50);
			//	Draw.Text(this, "tag1","ADXVMA"+( Period), -Textoffset, _ADXVMAColorBand2[0],PlotBrushes[0][0]);
				}		
			#endregion	
			}
			
		}
			// In order to trim the indicator's label we need to override the ToString() method.
			public override string DisplayName
				{
		            get { return Name + "(" + _Period +"_"+OffsetMultiplier +"_"+ OuterOffsetMultiplier+")";}
				}	
		
	#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CellConditionPlot
		{
			get { return Values[0]; }
		}				
				
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int _Period
		{ get; set; }		
	
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OuterLower
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Midline
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> OuterUpper
		{
			get { return Values[2]; }
		}
		
		[Range(1, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier", GroupName = "NinjaScriptParameters", Order = 0)]
		public double OffsetMultiplier
		{ get; set; }

		[Range(1, double.MaxValue), NinjaScriptProperty]	
		[Display(ResourceType = typeof(Custom.Resource), Name = "OuterOffsetMultiplier", GroupName = "NinjaScriptParameters", Order = 0)]
		public double OuterOffsetMultiplier
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="ADXVMA Rising Color", Description="ADXVMA Line Rising Color", Order=1, GroupName="Colors")]
		public Brush adxvmaRisingBrush
		{ get; set; }

		[Browsable(false)]
		public string adxvmaRisingBrushSerializable
		{
			get { return Serialize.BrushToString(adxvmaRisingBrush); }
			set { adxvmaRisingBrush = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="ADXVMA Falling Color", Description="ADXVMA Line Falling Color", Order=2, GroupName="Colors")]
		public Brush adxvmaFallingBrush
		{ get; set; }

		[Browsable(false)]
		public string adxvmaFallingBrushSerializable
		{
			get { return Serialize.BrushToString(adxvmaFallingBrush); }
			set { adxvmaFallingBrush = Serialize.StringToBrush(value); }					
		}			
				
		
		[XmlIgnore]
		[Display(Name="ADXVMA Neutral Color", Description="ADXVMA Line Neutral Color", Order=2, GroupName="Colors")]
		public Brush adxvmaNeutralBrush
		{ get; set; }

		[Browsable(false)]
		public string adxvmaNeutralBrushSerializable
		{
			get { return Serialize.BrushToString(adxvmaNeutralBrush); }
			set { adxvmaNeutralBrush = Serialize.StringToBrush(value); }					
		}			
			
		[Range(1, int.MaxValue)]
		[Display(Name="TextOffset", Description="Chart Text Offset", Order=1, GroupName="Parameters")]
		public int Textoffset
		{ get; set; }
		
	#region Button
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show button", GroupName = "Button", Order = 10)]
		public bool ShowButton
		{ get; set; }	
		
		[Range(1, 10), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Column #", GroupName = "Button", Order = 1)]
		public int ColumnNum
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name="Button color", Description="Color of button", Order=2, GroupName= "Button")]
		public Brush ButtonBrush
		{ get; set; }
		
		[Browsable(false)]
		public string ButtonBrushSerializable
		{
			get { return Serialize.BrushToString(ButtonBrush); }
			set { ButtonBrush = Serialize.StringToBrush(value); }
		}	
	#endregion
		
	#endregion 
		
    
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BobC.ADXVMAButtonBand[] cacheADXVMAButtonBand;
		public BobC.ADXVMAButtonBand ADXVMAButtonBand(int _period, double offsetMultiplier, double outerOffsetMultiplier, bool showButton, int columnNum)
		{
			return ADXVMAButtonBand(Input, _period, offsetMultiplier, outerOffsetMultiplier, showButton, columnNum);
		}

		public BobC.ADXVMAButtonBand ADXVMAButtonBand(ISeries<double> input, int _period, double offsetMultiplier, double outerOffsetMultiplier, bool showButton, int columnNum)
		{
			if (cacheADXVMAButtonBand != null)
				for (int idx = 0; idx < cacheADXVMAButtonBand.Length; idx++)
					if (cacheADXVMAButtonBand[idx] != null && cacheADXVMAButtonBand[idx]._Period == _period && cacheADXVMAButtonBand[idx].OffsetMultiplier == offsetMultiplier && cacheADXVMAButtonBand[idx].OuterOffsetMultiplier == outerOffsetMultiplier && cacheADXVMAButtonBand[idx].ShowButton == showButton && cacheADXVMAButtonBand[idx].ColumnNum == columnNum && cacheADXVMAButtonBand[idx].EqualsInput(input))
						return cacheADXVMAButtonBand[idx];
			return CacheIndicator<BobC.ADXVMAButtonBand>(new BobC.ADXVMAButtonBand(){ _Period = _period, OffsetMultiplier = offsetMultiplier, OuterOffsetMultiplier = outerOffsetMultiplier, ShowButton = showButton, ColumnNum = columnNum }, input, ref cacheADXVMAButtonBand);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BobC.ADXVMAButtonBand ADXVMAButtonBand(int _period, double offsetMultiplier, double outerOffsetMultiplier, bool showButton, int columnNum)
		{
			return indicator.ADXVMAButtonBand(Input, _period, offsetMultiplier, outerOffsetMultiplier, showButton, columnNum);
		}

		public Indicators.BobC.ADXVMAButtonBand ADXVMAButtonBand(ISeries<double> input , int _period, double offsetMultiplier, double outerOffsetMultiplier, bool showButton, int columnNum)
		{
			return indicator.ADXVMAButtonBand(input, _period, offsetMultiplier, outerOffsetMultiplier, showButton, columnNum);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BobC.ADXVMAButtonBand ADXVMAButtonBand(int _period, double offsetMultiplier, double outerOffsetMultiplier, bool showButton, int columnNum)
		{
			return indicator.ADXVMAButtonBand(Input, _period, offsetMultiplier, outerOffsetMultiplier, showButton, columnNum);
		}

		public Indicators.BobC.ADXVMAButtonBand ADXVMAButtonBand(ISeries<double> input , int _period, double offsetMultiplier, double outerOffsetMultiplier, bool showButton, int columnNum)
		{
			return indicator.ADXVMAButtonBand(input, _period, offsetMultiplier, outerOffsetMultiplier, showButton, columnNum);
		}
	}
}

#endregion
