//+----------------------------------------------------------------------------------------------+
//| Copyright © <2022>  <lawfpenn@outlook.com - >
//| Created by 'Forrestang' :https://futures.io/trading_member/5692-forrestang.html
//| For bugs, feature requests or other info, see thread below:
//| https://futures.io/ninjatrader/56493-my-indicator-thread-just-placeholder.html#post830676
//| This indicator was ported from TradeView, and the indicator was originally
//| produced by JayRogers.  For more information on his original work, see link below:
//| https://www.tradingview.com/script/1o4oWbEx-Heikin-Ashi-RSI-Oscillator/
//+----------------------------------------------------------------------------------------------+

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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.BTMM
{
	public class HeikinAshiRSIOscillator : Indicator
	{
		// -- Basic 
		private bool displayIndicatorName 	= true;	
		private int yChartMax 				= +45;
		private int yChartMin 				= -45;
		
		// -- Candle config
		//string GROUP_CAND = "Config » HARSI Candles"
		private int 							i_lenHARSI  = 14;
		private int 							i_smoothing = 7;
						
		//string INLINE_COL = "Colour Pallette"
		private System.Windows.Media.Brush		i_colUp     = Brushes.Teal;
		private System.Windows.Media.Brush		i_colDown   = Brushes.Red;
		private System.Windows.Media.Brush		i_colWick   = Brushes.DimGray;

		// -- RSI plot config
		//string GROUP_PLOT = "Config » RSI Plot"
//		private PriceType 						i_source    = PriceType.Close;
		private int 							i_lenRSI    = 7;
		private bool 							i_mode      = true;
		private bool 							i_showPlot  = true;
		private bool 							i_showHist  = true;

		// -- Stochastic RSI plots config
		//string GROUP_STOCH = "Config » Stochastic RSI Plot"
		//string INLINE_STDS = "Stoch Draw States"
		private bool 							i_showStoch = false;
		private int 							i_smoothK   = 3;
		private int 							i_smoothD   = 3;
		private int 							i_stochLen  = 14;
		private int 							i_stochFit  = 90;
		
		private bool 							i_ribbon    = false;
		private System.Windows.Media.Brush		ribbonClrUp   = Brushes.DodgerBlue;
		private System.Windows.Media.Brush		ribbonClrDn   = Brushes.OrangeRed;
		private int 							ribbonOpacity = 50;

		// -- Channel OB/OS config
		//string GROUP_CHAN = "Config » OB/OS Boundaries"
		private double 							i_upper     = 20;
		private double 							i_upperx    = 30;
		private double 							i_lower     = -20;
		private double 							i_lowerx    = -30;
		private int								boxOpacityTop	= 5;
		private int								boxOpacityMdl 	= 5;
		private int								boxOpacityLwr	= 5;
		
		//  channel fill
		private System.Windows.Media.Brush		uprZone   = Brushes.Red;
		private System.Windows.Media.Brush		mdlZone   = Brushes.Blue;
		private System.Windows.Media.Brush		lwrZone   = Brushes.Green;

		//Series needed 
		private Series<double>  _smoothed;	//for RSI
		private Series<double>  _openRSI;	//for HA calc
		private Series<double>  _closeRSI;	//for HA calc
		private Series<double>  _open;		//for HA calc
		private Series<double>  _close;		//for HA calc
		private Series<double>  _zstoch;	//for stochastics calc
		
		//Data Series to hold HA OHLC vals
		private Series<double>  openHA;		
		private Series<double>  highHA;		
		private Series<double>  lowHA;		
		private Series<double>  closeHA;	
		
		//use for MinMax calc
		private double haHighest =0;
		private double haLowest =0;
		
		//Shading
		private Series<double> UpTopLine;
		private Series<double> UpBotLine;
		private Series<double> DnTopLine;
		private Series<double> DnBotLine;
		
//		private System.Windows.Media.Brush		tempK   = Brushes.Red;
//		private System.Windows.Media.Brush		tempD   = Brushes.Blue;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Code Refactor";
				Name										= "HeikinAshiRSIOscillator";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ArePlotsConfigurable						= true;
				AreLinesConfigurable 						= true;
//				IgnoresUserInput 							= true;
				IsAutoScale									= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				AddPlot(new Stroke(Brushes.Yellow, 1), PlotStyle.Line, "MyRSI");	
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Bar, "MyRSI_histogram");
				AddPlot(new Stroke(Brushes.CornflowerBlue, 1), PlotStyle.Line, "MyStochK");
				AddPlot(new Stroke(Brushes.OrangeRed, 1), PlotStyle.Line, "MyStochD");
				
				AddLine(Brushes.DimGray,	i_upperx,	"UprTop"); //+30
				AddLine(Brushes.DimGray,	i_upper,	"UprMid"); //+20
				AddLine(Brushes.DimGray,	0,			"MyZero"); //0
				AddLine(Brushes.DimGray,	i_lower,	"LwrMid"); //-20
				AddLine(Brushes.DimGray,	i_lowerx,	"LwrBtm"); //-30
			}
			else if (State == State.Configure)
			{
//				Lines[2].DashStyleHelper = DashStyleHelper.Dot;
				i_upperx    = Lines[0].Value;
				i_upper 	= Lines[1].Value;
				i_lower     = Lines[3].Value;
				i_lowerx    = Lines[4].Value;
				
//				tempK =Plots[2].Brush;
//				tempD =Plots[3].Brush;
			}
			else if (State == State.DataLoaded)
			{			
//				ClearOutputWindow();
				_smoothed	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				_openRSI	= new Series<double>(this, MaximumBarsLookBack.Infinite);	
				_closeRSI	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				_open 		= new Series<double>(this, MaximumBarsLookBack.Infinite);
				_close 		= new Series<double>(this, MaximumBarsLookBack.Infinite);
				_zstoch 	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				openHA	= new Series<double>(this, MaximumBarsLookBack.Infinite);		
				highHA	= new Series<double>(this, MaximumBarsLookBack.Infinite);		
				lowHA	= new Series<double>(this, MaximumBarsLookBack.Infinite);		
				closeHA	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				UpTopLine= new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpBotLine= new Series<double>(this, MaximumBarsLookBack.Infinite);
				DnTopLine= new Series<double>(this, MaximumBarsLookBack.Infinite);
				DnBotLine= new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar <= i_lenHARSI+1 || CurrentBar <= i_lenRSI+1 || CurrentBar <= i_stochLen+1)
				return;
			
			//  standard, or ha smoothed rsi for the line plot and/or histogram
			MyRSI[0] = f_rsi(i_lenRSI, i_mode);
			MyRSI_histogram[0] = f_rsi(i_lenRSI, i_mode);
			
			//  stoch stuff			
			MyStochK[0] =f_zstoch( i_stochLen, i_smoothK, i_stochFit );
			MyStochD[0] =SMA(MyStochK, i_smoothD)[0];
			
			//  stoch ribbon
			if(MyStochK[0] > MyStochD[0])	{	UpTopLine[0] =MyStochK[0]; UpBotLine[0] =MyStochD[0];	}
			else							{	UpTopLine[0] =MyStochK[0]; UpBotLine[0] =MyStochK[0];	}
			
			if(MyStochK[0] < MyStochD[0])	{	DnTopLine[0] =MyStochD[0]; DnBotLine[0] =MyStochK[0];	}
			else							{	DnTopLine[0] =MyStochD[0]; DnBotLine[0] =MyStochD[0];	}
			
			if(i_ribbon)
			{
				Draw.Region(this, "isUPStoch", CurrentBar, 0, UpBotLine, UpTopLine, null, ribbonClrUp, ribbonOpacity);	
				Draw.Region(this, "isDNStoch", CurrentBar, 0, DnBotLine, DnTopLine, null, ribbonClrDn, ribbonOpacity);	
//				Plots[2].Brush = Brushes.Transparent;	//Set lines transparent
//				Plots[3].Brush = Brushes.Transparent;
			}
			else
			{
//				Plots[2].Brush = tempK;	//Reinitialize to original temp color if not using ribbon
//				Plots[3].Brush = tempD;					
			}

			//  get OHLC values to use in the plotcandle()
			double []myArray = f_rsiHeikinAshi(i_lenHARSI);  
			
			//  put HA OHLC values into data series
			openHA[0] =myArray[0];	highHA[0] =myArray[1];	lowHA[0] =myArray[2];	closeHA[0] =myArray[3];	
		}	//end onBarUpdate()

		#region onRenderFunction() / MinMax
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (Bars == null || chartControl == null)
				return;   
			
		//DRAW BACKGROUND COLORS //lwrZone mdlZone  uprZone
			// top box
			if(true)
			{
				// create two vectors to position the rectangle
				SharpDX.Vector2 startPoint = new SharpDX.Vector2(ChartPanel.X, chartScale.GetYByValue( i_upperx )); //upr left
				SharpDX.Vector2 endPoint = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, chartScale.GetYByValue( i_upper )); //lwr right
				
				// calculate the desired width and heigh of the rectangle
				float width = endPoint.X - startPoint.X;
				float height = endPoint.Y - startPoint.Y;
				
				// initialize Brush and add color from UI, add opacity and draw box
				SharpDX.Direct2D1.Brush boxBrush = uprZone.ToDxBrush(RenderTarget);	
				boxBrush.Opacity =  Convert.ToSingle(boxOpacityTop*0.01);    					//Convert Opacity int to float
				
				// construct the rectangleF struct to describe the with position and size the drawing
				SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint.X, startPoint.Y, width, height);
				RenderTarget.FillRectangle(rect, boxBrush);

				// always dispose of a brush when finished
				boxBrush.Dispose();
			}
			
			// middle box
			if(true)
			{
				// create two vectors to position the rectangle
				SharpDX.Vector2 startPoint = new SharpDX.Vector2(ChartPanel.X, chartScale.GetYByValue( i_upper )); //upr left
				SharpDX.Vector2 endPoint = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, chartScale.GetYByValue( i_lower )); //lwr right
				
				// calculate the desired width and heigh of the rectangle
				float width = endPoint.X - startPoint.X;
				float height = endPoint.Y - startPoint.Y;
				
				// initialize Brush and add color from UI, add opacity and draw box
				SharpDX.Direct2D1.Brush boxBrush = mdlZone.ToDxBrush(RenderTarget);	
				boxBrush.Opacity =  Convert.ToSingle(boxOpacityMdl*0.01);    					//Convert Opacity int to float
				
				// construct the rectangleF struct to describe the with position and size the drawing
				SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint.X, startPoint.Y, width, height);
				RenderTarget.FillRectangle(rect, boxBrush);
				
				// always dispose of a brush when finished
				boxBrush.Dispose();
			}
	
			// lower box
			if(true)
			{
				// create two vectors to position the rectangle
				SharpDX.Vector2 startPoint = new SharpDX.Vector2(ChartPanel.X, chartScale.GetYByValue( i_lower )); //upr left
				SharpDX.Vector2 endPoint = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, chartScale.GetYByValue( i_lowerx )); //lwr right
				
				// calculate the desired width and heigh of the rectangle
				float width = endPoint.X - startPoint.X;
				float height = endPoint.Y - startPoint.Y;
				
				// initialize Brush and add color from UI, add opacity and draw box
				SharpDX.Direct2D1.Brush boxBrush = lwrZone.ToDxBrush(RenderTarget);	
				boxBrush.Opacity =  Convert.ToSingle(boxOpacityLwr*0.01);    					//Convert Opacity int to float
				
				// construct the rectangleF struct to describe the with position and size the drawing
				SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint.X, startPoint.Y, width, height);
				RenderTarget.FillRectangle(rect, boxBrush);
				
				// always dispose of a brush when finished
				boxBrush.Dispose();
			}

			
			
		//CANDLES
			int rightMostBar = ChartBars.ToIndex; 					//Note:First bar on chart is 0 
			int leftMostBar = ChartBars.FromIndex; 					//Note:First bar on chart is 0 
			
			haHighest =0;
			haLowest =0;

			for (int i=leftMostBar; i <= rightMostBar; i++) //Start at necessary List position
			{
				//Calcs for MinMax
				haHighest =highHA.GetValueAt(i) >haHighest ? highHA.GetValueAt(i) : haHighest;
				haLowest =lowHA.GetValueAt(i) <haLowest ? lowHA.GetValueAt(i) : haLowest;
				
				float xPosition = chartControl.GetXByBarIndex(ChartBars, i);
				float myOpen = chartScale.GetYByValue( openHA.GetValueAt(i) );
				float myHigh = chartScale.GetYByValue( highHA.GetValueAt(i) );
				float myLow = chartScale.GetYByValue( lowHA.GetValueAt(i) );
				float myClose = chartScale.GetYByValue( closeHA.GetValueAt(i) );
				float barWidth = 0.80f*(chartControl.GetXByBarIndex(ChartBars,rightMostBar) - chartControl.GetXByBarIndex(ChartBars,(rightMostBar-1)));
				float barOCheight = (myClose - myOpen);

			//Body-------------------------------------------------------
				SharpDX.Direct2D1.Brush customDXBrush;	//initialize Brush
				
				// define the brush used in the rectangle
				if(myClose < myOpen) //OPPOSITE DIRECTION
					customDXBrush = i_colUp.ToDxBrush(RenderTarget); 
				else
					customDXBrush = i_colDown.ToDxBrush(RenderTarget); 

				// construct the rectangleF struct to describe the with position and size the drawing
				SharpDX.RectangleF rect = new SharpDX.RectangleF((xPosition-0.5f*barWidth), myOpen, barWidth, barOCheight);
				
				// execute the render target fill rectangle with desired values
				RenderTarget.FillRectangle(rect, customDXBrush);

				// always dispose of a brush when finished
				customDXBrush.Dispose();
				
			//Wicks-------------------------------------------------------
				float barTop, barBot;
				if(myClose < myOpen)		{	barTop =myClose;	barBot =myOpen;		}//OPPOSITE DIRECTION
				else if(myClose > myOpen)	{	barTop =myOpen;		barBot =myClose;	}//OPPOSITE DIRECTION
				else						{	barTop =myOpen;		barBot =myOpen;		}//OPPOSITE DIRECTION

				// will draw wicks from Bottom to TOP in both scenarios
				// t1,t2    b1,b2  t1=top of body, t2=bar high..... b1=bar low, b2 bottom of body
				SharpDX.Vector2 t1 = new SharpDX.Vector2(xPosition, barTop); //<---this is BROKEN for some reason
				SharpDX.Vector2 t2 = new SharpDX.Vector2(xPosition, myHigh);
				SharpDX.Vector2 b1 = new SharpDX.Vector2(xPosition, barBot);
				SharpDX.Vector2 b2 = new SharpDX.Vector2(xPosition, myLow);
				
				// define the brush used in the line
				SharpDX.Direct2D1.Brush customDXBrush2;	//initialize Brush
				customDXBrush2 = i_colWick.ToDxBrush(RenderTarget); 
//				customDXBrush2 = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.DimGray);
				
				// execute the render target draw line with desired values
				RenderTarget.DrawLine(t1, t2, customDXBrush2, 2);
				RenderTarget.DrawLine(b1, b2, customDXBrush2, 2);
				
				// always dispose of a brush when finished
				customDXBrush2.Dispose();
			}
			
			//Allow onRender+
			base.OnRender(chartControl, chartScale);
		}
		
		public override void OnCalculateMinMax()
		{
//			if(!IsAutoScale)
//			{	
			  	MaxValue = yChartMax;
				MinValue = yChartMin;
//			}
//			else
//			{
//				MaxValue =haHighest;
//				MinValue =haLowest;
//			}
		}
		
		
		#endregion

		#region User Created Functions
		
		//  zero median rsi helper function, just subtracts 50.
		public double f_zrsi(  int _length )	{	return RSI(Input, _length, 1)[0]-50;	}
		
		//  zero median stoch helper function, subtracts 50 and includes % scaling
		public double f_zstoch( int _length, int _smooth, int _scale ) 
		{			
			double daClose =MyRSI[0];
			double daLL =MyRSI.IsValidDataPoint(1) ? MIN(MyRSI, _length)[0] : MIN(MyRSI, _length)[0];
			double daHH =MyRSI.IsValidDataPoint(1) ? MAX(MyRSI, _length)[0] : MAX(MyRSI, _length)[0];
			_zstoch[0] =100*(daClose-daLL)/(daHH-daLL)-50;
			
			double _smoothedx =SMA(_zstoch, _smooth)[0];
			double _scaledx =(_smoothedx / 100)*_scale;
			return _scaledx;
		}

		//  mode selectable rsi function for standard, or smoothed output
		public double f_rsi( int _length, bool _mode ) 
		{
		    //  get base rsi
		    double _zrsi = f_zrsi( _length );

		    //  smoothing in a manner similar to HA open, but rather using the realtime rsi in place of the prior close value.
			_smoothed[0]  = _smoothed.IsValidDataPoint(1) ? ( _smoothed[1] + _zrsi ) / 2 : _zrsi;

		    //  return the requested mode
		    return _mode ? _smoothed[0] : _zrsi;
		}
		
		//  RSI Heikin-Ashi generation function
		public double[] f_rsiHeikinAshi(  int _length )	
		{		
			double []myOHLC = new double[4];
			
		    //  get close rsi
			_closeRSI[0] = RSI(Close, _length, 1)[0]-50;

		    //  emulate "open" simply by taking the previous close rsi value
			_openRSI[0]  = _closeRSI.IsValidDataPoint(1) ? _closeRSI[1] : _closeRSI[0];		//			var result = x > y ? "x is greater than y" : "x is less than y";

		    //  the high and low are tricky, because unlike "high" and "low" by
		    //  themselves, the RSI results can overlap each other. So first we just go
		    //  ahead and get the raw results for high and low, and then..
		    double _highRSI_raw = RSI(High, _length, 1)[0]-50;
		    double _lowRSI_raw  = RSI(Low, _length, 1)[0]-50;
			
//		    //  ..make sure we use the highest for high, and lowest for low
		    double _highRSI  = Math.Max( _highRSI_raw, _lowRSI_raw );
		    double _lowRSI   = Math.Min( _highRSI_raw, _lowRSI_raw );
			
//		    //  ha calculation for close
		    _close[0]    = ( _openRSI[0] + _highRSI + _lowRSI + _closeRSI[0] ) / 4;

//			//  ha calculation for open, standard, and smoothed/lagged
			if( _open[ i_smoothing ] ==0)
				_open[0] = (_openRSI[0] + _closeRSI[0] ) / 2;	
			else
				_open[0] =( ( _open[1] * i_smoothing ) + _close[1] ) / ( i_smoothing + 1 );	

//		    //  ha high and low min-max selections
		    double _high     = Math.Max( _highRSI, Math.Max( _open[0], _close[0] ) );
		    double _low      = Math.Min( _lowRSI,  Math.Min( _open[0], _close[0] ) );
			
			myOHLC[0] =_open[0];
			myOHLC[1] =_high;
			myOHLC[2] =_low;
			myOHLC[3] =_close[0];

			return myOHLC;
		}

		public override string DisplayName
		{
			get { if  (State == State.SetDefaults) 
					return Name; 	
					else  if (displayIndicatorName)
					return "HARSI("+i_lenHARSI+","+i_smoothing+","+i_lenRSI+","+i_smoothK+","+i_smoothD+","+i_stochLen+""+ ")";
					else return "";  }	
		}
		
		#endregion
		
		#region Plots
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MyRSI
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MyRSI_histogram
		{
			get { return Values[1]; }
		}		
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MyStochK
		{
			get { return Values[2]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MyStochD
		{
			get { return Values[3]; }
		}
		#endregion
		
		#region User Input-Basic
	// -- Basic 	
		[Display(ResourceType = typeof(Custom.Resource), Name="Show Label on-chart", Description="", GroupName="1)BASIC", Order=0)]
		public bool DisplayIndicatorName
		{ 
			get { return displayIndicatorName; } 
			set { displayIndicatorName = value; } 
		}		

//		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Y-MAX", Description = "", GroupName = "1)BASIC", Order = 1)]
		public int YChartMax
		{	
            get { return yChartMax; }
            set { yChartMax = value; }
		}
//		[Range(-100, int.MinValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Y-MIN", Description = "", GroupName = "1)BASIC", Order = 2)]
		public int YChartMin
		{	
            get { return yChartMin; }
            set { yChartMin = value; }
		}
		
		#endregion
		
		#region User Input-Candle Config	
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Length HARSI", Description = "", GroupName = "2)CANDLE CONFIG", Order = 0)]
		public int I_lenHARSI
		{	
            get { return i_lenHARSI; }
            set { i_lenHARSI = value; }
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Smoothing HARSI", Description = "", GroupName = "2)CANDLE CONFIG", Order = 1)]
		public int I_smoothing
		{	
            get { return i_smoothing; }
            set { i_smoothing = value; }
		}		
		
	//------------	
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Candle Color UP", Description = "", GroupName = "2)CANDLE CONFIG", Order = 3)]
		public System.Windows.Media.Brush I_colUp
		{ 
			get {return i_colUp;}
			set {i_colUp = value;}
		}
		[Browsable(false)]
		public string I_colUpSerializable
		{
			get { return Serialize.BrushToString(i_colUp); }
			set { i_colUp = Serialize.StringToBrush(value); }
		}
	//------------	
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Candle Color DN", Description = "", GroupName = "2)CANDLE CONFIG", Order = 4)]
		public System.Windows.Media.Brush I_colDown
		{ 
			get {return i_colDown;}
			set {i_colDown = value;}
		}
		[Browsable(false)]
		public string I_colDownSerializable
		{
			get { return Serialize.BrushToString(i_colDown); }
			set { i_colDown = Serialize.StringToBrush(value); }
		}
	//------------	
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Wick Color", Description = "", GroupName = "2)CANDLE CONFIG", Order = 5)]
		public System.Windows.Media.Brush I_colWick
		{ 
			get {return i_colWick;}
			set {i_colWick = value;}
		}
		[Browsable(false)]
		public string I_colWickSerializable
		{
			get { return Serialize.BrushToString(i_colWick); }
			set { i_colWick = Serialize.StringToBrush(value); }
		}
		
		
		#endregion
		
		#region User Input - RSI
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Length RSI", Description = "", GroupName = "2)RSI CONFIG", Order = 0)]
		public int I_lenRSI
		{	
            get { return i_lenRSI; }
            set { i_lenRSI = value; }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Smooth RSI", Description="", GroupName="2)RSI CONFIG", Order=1)]
		public bool I_mode
		{ 
			get { return i_mode; } 
			set { i_mode = value; } 
		}
		
		#endregion
		
		#region User Input - Stochastic RSI
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Length", Description = "", GroupName = "3)Stoch RSI CONFIG", Order = 0)]
		public int I_stochLen
		{	
            get { return i_stochLen; }
            set { i_stochLen = value; }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Smooth K", Description = "", GroupName = "3)Stoch RSI CONFIG", Order = 1)]
		public int I_smoothK
		{	
            get { return i_smoothK; }
            set { i_smoothK = value; }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Smooth D", Description = "", GroupName = "3)Stoch RSI CONFIG", Order = 2)]
		public int I_smoothD
		{	
            get { return i_smoothD; }
            set { i_smoothD = value; }
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name ="Stoch Scaler", Description = "", GroupName = "3)Stoch RSI CONFIG", Order = 3)]
		public int I_stochFit
		{	
            get { return i_stochFit; }
            set { i_stochFit = value; }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show Ribbon", Description="", GroupName="3)Stoch RSI CONFIG", Order=4)]
		public bool I_ribbon
		{ 
			get { return i_ribbon; } 
			set { i_ribbon = value; } 
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name="Ribbon Color UP", Description="", GroupName="3)Stoch RSI CONFIG", Order=5)]
		public System.Windows.Media.Brush RibbonClrUp
		{ 
			get {return ribbonClrUp;}
			set {ribbonClrUp = value;}
		}
		[Browsable(false)]
		public string RibbonClrUpSerializable
		{
			get { return Serialize.BrushToString(ribbonClrUp); }
			set { ribbonClrUp = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name="Ribbon Color DN", Description="", GroupName="3)Stoch RSI CONFIG", Order=6)]
		public System.Windows.Media.Brush RibbonClrDn
		{ 
			get {return ribbonClrDn;}
			set {ribbonClrDn = value;}
		}
		[Browsable(false)]
		public string RibbonClrDnSerializable
		{
			get { return Serialize.BrushToString(ribbonClrDn); }
			set { ribbonClrDn = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Ribbon Opacity", Description="", GroupName="3)Stoch RSI CONFIG", Order=7)]
		public int RibbonOpacity
		{	
            get { return ribbonOpacity; }
            set { ribbonOpacity = value; }
		}
		
		#endregion
		
		#region User Input - Region Fill Colors
	//------------	
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper Zone", Description = "", GroupName = "4)Region Fill Colors", Order = 0)]
		public System.Windows.Media.Brush UprZone
		{ 
			get {return uprZone;}
			set {uprZone = value;}
		}
		[Browsable(false)]
		public string UprZoneSerializable
		{
			get { return Serialize.BrushToString(uprZone); }
			set { uprZone = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Top Opacity", Description = "", GroupName = "4)Region Fill Colors", Order = 1)]
		public int BoxOpacityTop
		{	
            get { return boxOpacityTop; }
            set { boxOpacityTop = value; }
		}
		
	//------------	
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Middle Zone", Description = "", GroupName = "4)Region Fill Colors", Order = 2)]
		public System.Windows.Media.Brush MdlZone
		{ 
			get {return mdlZone;}
			set {mdlZone = value;}
		}
		[Browsable(false)]
		public string MdlZoneSerializable
		{
			get { return Serialize.BrushToString(mdlZone); }
			set { mdlZone = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Middle Opacity", Description = "", GroupName = "4)Region Fill Colors", Order = 3)]
		public int BoxOpacityMdl
		{	
            get { return boxOpacityMdl; }
            set { boxOpacityMdl = value; }
		}
		
	//------------	
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lower Zone", Description = "", GroupName = "4)Region Fill Colors", Order = 4)]
		public System.Windows.Media.Brush LwrZone
		{ 
			get {return lwrZone;}
			set {lwrZone = value;}
		}
		[Browsable(false)]
		public string LwrZoneSerializable
		{
			get { return Serialize.BrushToString(lwrZone); }
			set { lwrZone = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lower Opacity", Description = "", GroupName = "4)Region Fill Colors", Order = 5)]
		public int BoxOpacityLwr
		{	
            get { return boxOpacityLwr; }
            set { boxOpacityLwr = value; }
		}
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BTMM.HeikinAshiRSIOscillator[] cacheHeikinAshiRSIOscillator;
		public BTMM.HeikinAshiRSIOscillator HeikinAshiRSIOscillator(int i_lenHARSI, int i_smoothing, int i_lenRSI, bool i_mode, int i_stochLen, int i_smoothK, int i_smoothD, int i_stochFit, bool i_ribbon, int ribbonOpacity, int boxOpacityTop, int boxOpacityMdl, int boxOpacityLwr)
		{
			return HeikinAshiRSIOscillator(Input, i_lenHARSI, i_smoothing, i_lenRSI, i_mode, i_stochLen, i_smoothK, i_smoothD, i_stochFit, i_ribbon, ribbonOpacity, boxOpacityTop, boxOpacityMdl, boxOpacityLwr);
		}

		public BTMM.HeikinAshiRSIOscillator HeikinAshiRSIOscillator(ISeries<double> input, int i_lenHARSI, int i_smoothing, int i_lenRSI, bool i_mode, int i_stochLen, int i_smoothK, int i_smoothD, int i_stochFit, bool i_ribbon, int ribbonOpacity, int boxOpacityTop, int boxOpacityMdl, int boxOpacityLwr)
		{
			if (cacheHeikinAshiRSIOscillator != null)
				for (int idx = 0; idx < cacheHeikinAshiRSIOscillator.Length; idx++)
					if (cacheHeikinAshiRSIOscillator[idx] != null && cacheHeikinAshiRSIOscillator[idx].I_lenHARSI == i_lenHARSI && cacheHeikinAshiRSIOscillator[idx].I_smoothing == i_smoothing && cacheHeikinAshiRSIOscillator[idx].I_lenRSI == i_lenRSI && cacheHeikinAshiRSIOscillator[idx].I_mode == i_mode && cacheHeikinAshiRSIOscillator[idx].I_stochLen == i_stochLen && cacheHeikinAshiRSIOscillator[idx].I_smoothK == i_smoothK && cacheHeikinAshiRSIOscillator[idx].I_smoothD == i_smoothD && cacheHeikinAshiRSIOscillator[idx].I_stochFit == i_stochFit && cacheHeikinAshiRSIOscillator[idx].I_ribbon == i_ribbon && cacheHeikinAshiRSIOscillator[idx].RibbonOpacity == ribbonOpacity && cacheHeikinAshiRSIOscillator[idx].BoxOpacityTop == boxOpacityTop && cacheHeikinAshiRSIOscillator[idx].BoxOpacityMdl == boxOpacityMdl && cacheHeikinAshiRSIOscillator[idx].BoxOpacityLwr == boxOpacityLwr && cacheHeikinAshiRSIOscillator[idx].EqualsInput(input))
						return cacheHeikinAshiRSIOscillator[idx];
			return CacheIndicator<BTMM.HeikinAshiRSIOscillator>(new BTMM.HeikinAshiRSIOscillator(){ I_lenHARSI = i_lenHARSI, I_smoothing = i_smoothing, I_lenRSI = i_lenRSI, I_mode = i_mode, I_stochLen = i_stochLen, I_smoothK = i_smoothK, I_smoothD = i_smoothD, I_stochFit = i_stochFit, I_ribbon = i_ribbon, RibbonOpacity = ribbonOpacity, BoxOpacityTop = boxOpacityTop, BoxOpacityMdl = boxOpacityMdl, BoxOpacityLwr = boxOpacityLwr }, input, ref cacheHeikinAshiRSIOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BTMM.HeikinAshiRSIOscillator HeikinAshiRSIOscillator(int i_lenHARSI, int i_smoothing, int i_lenRSI, bool i_mode, int i_stochLen, int i_smoothK, int i_smoothD, int i_stochFit, bool i_ribbon, int ribbonOpacity, int boxOpacityTop, int boxOpacityMdl, int boxOpacityLwr)
		{
			return indicator.HeikinAshiRSIOscillator(Input, i_lenHARSI, i_smoothing, i_lenRSI, i_mode, i_stochLen, i_smoothK, i_smoothD, i_stochFit, i_ribbon, ribbonOpacity, boxOpacityTop, boxOpacityMdl, boxOpacityLwr);
		}

		public Indicators.BTMM.HeikinAshiRSIOscillator HeikinAshiRSIOscillator(ISeries<double> input , int i_lenHARSI, int i_smoothing, int i_lenRSI, bool i_mode, int i_stochLen, int i_smoothK, int i_smoothD, int i_stochFit, bool i_ribbon, int ribbonOpacity, int boxOpacityTop, int boxOpacityMdl, int boxOpacityLwr)
		{
			return indicator.HeikinAshiRSIOscillator(input, i_lenHARSI, i_smoothing, i_lenRSI, i_mode, i_stochLen, i_smoothK, i_smoothD, i_stochFit, i_ribbon, ribbonOpacity, boxOpacityTop, boxOpacityMdl, boxOpacityLwr);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BTMM.HeikinAshiRSIOscillator HeikinAshiRSIOscillator(int i_lenHARSI, int i_smoothing, int i_lenRSI, bool i_mode, int i_stochLen, int i_smoothK, int i_smoothD, int i_stochFit, bool i_ribbon, int ribbonOpacity, int boxOpacityTop, int boxOpacityMdl, int boxOpacityLwr)
		{
			return indicator.HeikinAshiRSIOscillator(Input, i_lenHARSI, i_smoothing, i_lenRSI, i_mode, i_stochLen, i_smoothK, i_smoothD, i_stochFit, i_ribbon, ribbonOpacity, boxOpacityTop, boxOpacityMdl, boxOpacityLwr);
		}

		public Indicators.BTMM.HeikinAshiRSIOscillator HeikinAshiRSIOscillator(ISeries<double> input , int i_lenHARSI, int i_smoothing, int i_lenRSI, bool i_mode, int i_stochLen, int i_smoothK, int i_smoothD, int i_stochFit, bool i_ribbon, int ribbonOpacity, int boxOpacityTop, int boxOpacityMdl, int boxOpacityLwr)
		{
			return indicator.HeikinAshiRSIOscillator(input, i_lenHARSI, i_smoothing, i_lenRSI, i_mode, i_stochLen, i_smoothK, i_smoothD, i_stochFit, i_ribbon, ribbonOpacity, boxOpacityTop, boxOpacityMdl, boxOpacityLwr);
		}
	}
}

#endregion
