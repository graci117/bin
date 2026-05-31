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
namespace NinjaTrader.NinjaScript.Indicators
{
	public class FibonacciClusterV16D : Indicator
	{	
		private DateTime				currentDate;				
		private DateTime  				sessionBegin;
		private DateTime  				sessionEnd;			
		
		private TimeSpan				sessionLength;
		private	double					currentHigh;			
		private	double					currentLow;			
		private double					currentClose;
		private double					currentLowAfterHigh;
		private double					currentHighAfterLow;	
		private SimpleFont				textFont;	
		private string					errorData;
		private bool					dailyChart;				
		private bool					firstPeriod;				
		private bool					plotPivots;				
		private bool					periodOpen;				
		private int						sessionNumber;			
		
		private int						shortperiod;
		private int						longperiod;			
		private double[]				priorHigh;			
		private double[]				priorLow;				
		private double[]				priorClose;				
		private double					priorLowAfterHigh;
		private double					priorHighAfterLow;
		private DateTime				currentHighTime;		
		private DateTime				currentLowTime;		
		private DateTime				currentLowAfterHighTime;
		private DateTime				currentHighAfterLowTime;
		private DateTime[]				priorHighTime;
		private DateTime[]				priorLowTime;
		private DateTime				priorLowAfterHighTime;
		private DateTime				priorHighAfterLowTime;
		private double					newPrimaryHigh;	
		private double					newSecondaryHigh;
		private double					newPrimaryLow;
		private double					newSecondaryLow;			
		private bool					existsNewPrimaryHigh;
		private bool					existsNewSecondaryHigh;
		private bool					existsNewPrimaryLow;
		private bool					existsNewSecondaryLow;
		private bool					existsOldSecondaryHigh;
		private bool					existsOldSecondaryLow;
		private int						swingIndex;	
		private int						rIndex;			
		private double[]				swingHigh;		
		private double[]				swingLow;		
		private DateTime[]				swingHighTime;		
		private DateTime[]				swingLowTime;		
		private double[]				coveredHigh;		
		private double[]				coveredLow;			
		private DateTime[]				coveredHighTime;			
		private DateTime[]				coveredLowTime;			
		private int						highFibIndex;			
		private int						lowFibIndex;			
		private int[]					highPairIndex;	
		private int[]					lowPairIndex;		
		private double[]				highFib;		
		private double[]				coupledLowFib;	
		private DateTime[]				highFibTime;			
		private double[]				lowFib;			
		private double[]				coupledHighFib;			
		private DateTime[]				lowFibTime;		
		private double					coupledLowFibTemp;
		private double					coupledHighFibTemp;		
		private int						highExtIndex;	
		private int						lowExtIndex;			
		private int[]					highExtPairIndex;
		private int[]					lowExtPairIndex;		
		private double[]				highExtFib;		
		private double[]				coupledLowExtFib;
		private DateTime[]				highExtFibTime;		
		private double[]				lowExtFib;		
		private double[]				coupledHighExtFib;
		private DateTime[]				lowExtFibTime;		
		private bool					noextensions;
		private double					fib000;			
		private double					fib1000;
		private int[]					fibCounter;
		private Series<DateTime>[]		highDate;
		private Series<DateTime>[]		lowDate;
		
		private Series<DateTime>[]		hiddenHighDate;
		private Series<DateTime>[]		hiddenLowDate;
		private Series<DateTime>[]		highExtDate;
		private Series<DateTime>[]		lowExtDate;
		private double					runningHigh;
		private double					runningLow;			
		private DateTime				runningHighTime;
		private DateTime				runningLowTime;		
		private int						runningHighIndex;
		private int 					runningLowIndex;		
		private double					lastHighFib;		
		private double					lastLowFib;			
		private double					lastCoupledLowFib;
		private double					lastCoupledHighFib;
		private DateTime				lastHighFibTime;	
		private DateTime				lastLowFibTime;		
		private Series<DateTime>		lastHighDate;
		private Series<DateTime>		lastLowDate;
		private bool					recentDown;
		private bool					recentUp;				
		private bool					zExtensionDown;
		private bool					zExtensionUp;		
		private double					recentHigh;			
		private double					recentLow;			
		private DateTime				recentHighTime;
		private DateTime				recentLowTime;		
		private double					recentCoupledLow;
		private double					recentCoupledHigh;
		private Series<DateTime>		recentHighDate;
		private Series<DateTime>		recentLowDate;
		private double					rememberHigh;		
		private double					rememberLow;			
		private DateTime				rememberTime;
		private string[] 				plotlabel;
		private string[] 				plotlabelfull;
		private string[] 				plotlabelused;
		
		private SessionIterator sessionIterator;
		int BarsRequired;
		
		private SharpDX.DirectWrite.TextFormat[] textFormats;
		private SharpDX.DirectWrite.TextLayout[] textLayouts;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Based on NinjaTrader 7's anaFibonacciClusters indicator";
				Name										= "FibonacciClusterV16D";
				Calculate									= Calculate.OnPriceChange;
				IsAutoScale 								= false;
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
				
				string[] PlotName = new string[]
				{
					"X+ 127,2 "		, "X+ 161,8 "	, "X+ 200,0 "	, "X+ 127,2 1 "	, "X+ 161,8 1 "	, "X+ 200,0 1 "	, "X+ 127,2 2 "	,
					"X+ 161,8 2 "	, "X+ 200,0 2 "	, "X+ 127,2 3 "	, "X+ 161,8 3 "	, "X+ 200,0 3 "	, "X+ 127,2 4 "	, "X+ 161,8 4 "	,
					"X+ 200,0 4 "	, "X+ 127,2 5 "	, "X+ 161,8 5 "	, "X+ 200,0 6 "	,
					
					"X- 127,2 "		, "X- 161,8 "	, "X- 200,0 "	, "X- 127,2 1 "	, "X- 161,8 1 "	, "X- 200,0 1 "	, "X- 127,2 2 "	,
					"X- 161,8 2 "	, "X- 200,0 2 "	, "X- 127,2 3 "	, "X- 161,8 3 "	, "X- 200,0 3 "	, "X- 127,2 4 "	, "X- 161,8 4 "	,
					"X- 200,0 4 "	, "X- 127,2 5 "	, "X- 161,8 5 "	, "X- 200,0 5 "	,
					
					"Z+   0,0 "		, "Z+  23,6 "	, "Z+  38,2 "	,	"Z+  50,0 "	, "Z+  61,8 "	, "Z+  76,4 "	, "Z+ HIGH "	,
					"Z+ 127,2 "		, "Z+ 161,8 "	,
					
					"Z-   0,0 "		, "Z-  23,6 "	, "Z-  38,2 "	, "Z-  50,0 "	, "Z-  61,8 "	, "Z-  76,4 "	, "Z-  LOW "	,
					"Z- 127,2 "		, "Z- 161,8 "	,
					
					"Y+   0,0 "		, "Y+  23,6 "	, "Y+  38,2 "	, "Y+  50,0 "	, "Y+  61,8 "	, "Y+  76,4 "	, "Y+ HIGH "	,
					"Y+ 127,2 "		, "Y+ 161,8 "	,
					
					"Y-   0,0 "		, "Y-  23,6 "	, "Y-  38,2 "	, "Y-  50,0 "	, "Y-  61,8 "	, "Y-  76,4 "	, "Y-  LOW "	,
					"Y- 127,2 "		, "Y- 161,8 "	,
					
					"S+   0,0 "		, "S+  23,6 "	, "S+  38,2 "	, "S+  50,0 "	, "S+  61,8 "	, "S+  76,4 "	, "S+ HIGH "	,
					"S+ 127,2 "		, "S+ 161,8 "	,
					
					"S-   0,0 "		, "S-  23,6 "	, "S-  38,2 "	, "S-  50,0 "	, "S-  61,8 "	, "S-  76,4 "	, "S-  LOW "	,
					"S- 127,2 "		, "S- 161,8 "	,
					
					"E+   0,0 "		, "E+  23,6 "	, "E+  38,2 "	, "E+  50,0 "	, "E+  61,8 "	, "E+  76,4 "	, "E+ HIGH "	,
					"E+ 127,2 "		, "E+ 161,8 "	,
					
					"E-   0,0 "		, "E-  23,6 "	, "E-  38,2 "	, "E-  50,0 "	, "E-  61,8 "	, "E-  76,4 "	, "E-  LOW "	,
					"E- 127,2 "		, "E- 161,8 "	,
					
					"D+   0,0 "		, "D+  23,6 "	, "D+  38,2 "	, "D+  50,0 "	, "D+  61,8 "	, "D+  76,4 "	, "D+ HIGH "	,
					"D+ 127,2 "		, "D+ 161,8 "	,
					
					"D-   0,0 "		, "D-  23,6 "	, "D-  38,2 "	, "D-  50,0 "	, "D-  61,8 "	, "D-  76,4 "	, "D- HIGH "	,
					"D- 127,2 "		, "D- 161,8 "	,
					
					"C+   0,0 "		, "C+  23,6 "	, "C+  38,2 "	, "C+  50,0 "	, "C+  61,8 "	, "C+  76,4 "	, "C+ HIGH "	,
					"C+ 127,2 "		, "C+ 161,8 "	,
					
					"C-   0,0 "		, "C-  23,6 "	, "C-  38,2 "	, "C-  50,0 "	, "C-  61,8 "	, "C-  76,4 "	, "C- HIGH "	,
					"C- 127,2 "		, "C- 161,8 "	,
					
					"B+   0,0 "		, "B+  23,6 "	, "B+  38,2 "	, "B+  50,0 "	, "B+  61,8 "	, "B+  76,4 "	, "B+ HIGH "	,
					"B+ 127,2 "		, "B+ 161,8 "	,
					
					"B-   0,0 "		, "B-  23,6 "	, "B-  38,2 "	, "B-  50,0 "	, "B-  61,8 "	, "B-  76,4 "	, "B- HIGH "	,
					"B- 127,2 "		, "B- 161,8 "	,
					
					"A+   0,0 "		, "A+  23,6 "	, "A+  38,2 "	, "A+  50,0 "	, "A+  61,8 "	, "A+  76,4 "	, "A+ HIGH "	,
					"A+ 127,2 "		, "A+ 161,8 "	,
					
					"A-   0,0 "		, "A-  23,6 "	, "A-  38,2 "	, "A-  50,0 "	, "A-  61,8 "	, "A-  76,4 "	, "A- HIGH "	,
					"A- 127,2 "		, "A- 161,8 "	,
					
					"HIGH0      "	, "HIGH1      "	, "HIGH2      "	, "HIGH3      "	, "HIGH4      "	, "HIGH5      "	,
					"LOW0      "	, "LOW1      "	, "LOW2      "	, "LOW3      "	, "LOW4      "	, "LOW5      "	,
				};
				
				int BrushCount = -1;
				for (int i = 0; i < 192; i++)
				{
					if (PlotName[i].Contains("HIGH") || PlotName[i].Contains("LOW"))
						AddPlot(new Stroke(Brushes.LightSteelBlue,1), PlotStyle.Line, PlotName[i]);
					else if (PlotName[i].Contains("X"))
					{
						if (i % 3 == 0)
							BrushCount++;
						switch (BrushCount)
						{
							case 0:
							case 6:
								AddPlot(new Stroke(Brushes.Gold,1), PlotStyle.Line, PlotName[i]);
								break;
							case 1:
							case 7:
								AddPlot(new Stroke(Brushes.Orange,1), PlotStyle.Line, PlotName[i]);
								break;
							case 2:
							case 8:
								AddPlot(new Stroke(Brushes.LightCoral,1), PlotStyle.Line, PlotName[i]);
								break;
							case 3:
							case 9:
								AddPlot(new Stroke(Brushes.MediumSpringGreen,1), PlotStyle.Line, PlotName[i]);
								break;
							case 4:
							case 10:
								AddPlot(new Stroke(Brushes.DeepSkyBlue,1), PlotStyle.Line, PlotName[i]);
								break;
							case 5:
							case 11:
								AddPlot(new Stroke(Brushes.Fuchsia,1), PlotStyle.Line, PlotName[i]);
								break;
						}
					}
					else if (PlotName[i].Contains("Z"))
						AddPlot(new Stroke(Brushes.LightGoldenrodYellow,1), PlotStyle.Line, PlotName[i]);
					else if (PlotName[i].Contains("Y"))
						AddPlot(new Stroke(Brushes.PeachPuff,1), PlotStyle.Line, PlotName[i]);	
					else if (PlotName[i].Contains("S"))
						AddPlot(new Stroke(Brushes.Yellow,1), PlotStyle.Line, PlotName[i]);
					else if (PlotName[i].Contains("E"))
						AddPlot(new Stroke(Brushes.Orange,1), PlotStyle.Line, PlotName[i]);		
					else if (PlotName[i].Contains("D"))
						AddPlot(new Stroke(Brushes.LightCoral,1), PlotStyle.Line, PlotName[i]);				
					else if (PlotName[i].Contains("C"))
						AddPlot(new Stroke(Brushes.MediumSpringGreen,1), PlotStyle.Line, PlotName[i]);		
					else if (PlotName[i].Contains("B"))
						AddPlot(new Stroke(Brushes.DeepSkyBlue,1), PlotStyle.Line, PlotName[i]);				
					else if (PlotName[i].Contains("A"))
						AddPlot(new Stroke(Brushes.Fuchsia,1), PlotStyle.Line, PlotName[i]);
				}
					
				// User Defined Inputs
				LabelPosition			= 10;
				Width					= 15;
				LookBack				= 100;
				Filter					= 85;
				// Offset?
				Show_S_Plus				= true;
				Show_S_Minus			= true;
				Show_E_Plus				= true;
				Show_E_Minus			= true;		
				Show_D_Plus				= true;
				Show_D_Minus			= true;		
				Show_C_Plus				= true;
				Show_C_Minus			= true;		
				Show_B_Plus				= true;
				Show_B_Minus			= true;
				Show_A_Plus				= true;
				Show_A_Minus			= true;
				Show_Xtensions			= true;
				Show_Highs_Lows			= true;
				Show_Y_Plus				= true;
				Show_Y_Minus			= true;
				Show_Z_Plus				= true;
				Show_Z_Minus			= true;
				
			}
			else if (State == State.DataLoaded)
			{
				currentDate				= DateTime.MinValue;
				sessionBegin			= DateTime.MinValue;
				sessionEnd				= DateTime.MinValue;

				sessionLength			= new TimeSpan(0,0,0);
				currentHigh				= double.MinValue;
				currentLow				= double.MaxValue;
				currentClose			= 0;
				currentLowAfterHigh		= double.MaxValue;
				currentHighAfterLow		= double.MinValue;
				textFont				= new SimpleFont("Arial", 12);
				errorData				= "Please increase chart lookback period to match lookback period of Fibonacci indicator.";
				dailyChart				= false;
				firstPeriod				= true;
				plotPivots				= false;
				periodOpen				= false;
				sessionNumber			= 0;
				shortperiod				= 0;
				longperiod				= 0;
				priorHigh				= new double[5];
				priorLow				= new double[5];
				priorClose				= new double[5];
				priorLowAfterHigh		= double.MinValue;
				priorHighAfterLow		= double.MaxValue;
				currentHighTime			= DateTime.MinValue;
				currentLowTime			= DateTime.MinValue;
				currentLowAfterHighTime	= DateTime.MinValue;
				currentHighAfterLowTime	= DateTime.MinValue;
				priorHighTime			= new DateTime[5];
				priorLowTime			= new DateTime[5];
				priorLowAfterHighTime	= DateTime.MinValue;
				priorHighAfterLowTime	= DateTime.MinValue;
				newPrimaryHigh			= double.MaxValue;
				newSecondaryHigh		= double.MaxValue;
				newPrimaryLow			= double.MinValue;
				newSecondaryLow			= double.MinValue;
				existsNewPrimaryHigh	= false;
				existsNewSecondaryHigh	= false;
				existsNewPrimaryLow		= false;
				existsNewSecondaryLow	= false;
				existsOldSecondaryHigh	= false;
				existsOldSecondaryLow	= false;
				swingIndex				= -1;
				rIndex					= 0;
				swingHigh				= new double[10];
				swingLow				= new double[10];
				swingHighTime			= new DateTime[10];
				swingLowTime			= new DateTime[10];
				coveredHigh				= new double[10];
				coveredLow				= new double[10];
				coveredHighTime			= new DateTime[10];
				coveredLowTime			= new DateTime[10];
				highFibIndex			= -1;
				lowFibIndex				= -1;
				highPairIndex			= new int[10];
				lowPairIndex			= new int[10];
				highFib					= new double[10];
				coupledLowFib			= new double[10];
				highFibTime				= new DateTime[10];
				lowFib					= new double[10];
				coupledHighFib			= new double[10];
				lowFibTime				= new DateTime[10];
				coupledLowFibTemp		= double.MaxValue;
				coupledHighFibTemp		= double.MinValue;
				highExtIndex			= -1;
				lowExtIndex				= -1;
				highExtPairIndex		= new int[10];
				lowExtPairIndex			= new int[10];
				highExtFib				= new double[10];
				coupledLowExtFib		= new double[10];
				highExtFibTime			= new DateTime[10];
				lowExtFib				= new double[10];
				coupledHighExtFib		= new double[10];
				lowExtFibTime			= new DateTime[10];
				noextensions			= false;
				fib000					= 0;
				fib1000					= 0;	

				runningHigh				= double.MinValue;
				runningLow				= double.MaxValue;
				runningHighTime			= DateTime.MinValue;
				runningLowTime			= DateTime.MinValue;
				runningHighIndex		=-1;
				runningLowIndex			=-1;
				lastHighFib				= double.MaxValue;
				lastLowFib				= double.MinValue;
				lastCoupledLowFib		= double.MinValue;
				lastCoupledHighFib		= double.MaxValue;
				lastHighFibTime			= DateTime.MinValue;
				lastLowFibTime			= DateTime.MinValue;

				recentDown				= false;
				recentUp				= false;
				zExtensionDown			= false;
				zExtensionUp			= false;
				recentHigh				= double.MaxValue;
				recentLow				= double.MinValue;
				recentHighTime			= DateTime.MinValue;
				recentLowTime			= DateTime.MinValue;
				recentCoupledLow		= double.MinValue;
				recentCoupledHigh		= double.MaxValue;

				rememberHigh			= 0;
				rememberLow				= 0;
				rememberTime			= DateTime.MinValue;
				
				fibCounter				= new int[192];
				plotlabel				= new string[192];
				plotlabelfull			= new string[192];
				plotlabelused			= new string[192];
				
				highDate = new Series<DateTime>[6];
				lowDate = new Series<DateTime>[6];
				hiddenHighDate = new Series<DateTime>[6];
				hiddenLowDate = new Series<DateTime>[6];
				highExtDate = new Series<DateTime>[6];
				lowExtDate = new Series<DateTime>[6];
				
				for (int i=0; i<6; i++)
				{
					highDate[i] = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
					lowDate[i] = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
					hiddenHighDate[i]= new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
					hiddenLowDate[i]= new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
					highExtDate[i] = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
					lowExtDate[i] = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
				}
				
				lastHighDate = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
				lastLowDate = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
				recentHighDate = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
				recentLowDate = new Series<DateTime>(this, MaximumBarsLookBack.Infinite);
				
				// Defining all instruments for which this indicator can be used. Indicator will not show on any instruments not listed here!
			
				if(Instrument == null)
					return;
			
				for (int i=0; i<5; i++)
				{
					priorHigh[i]=double.MaxValue;
					priorLow[i]= double.MinValue;
					priorClose[i] = 0;
					priorHighTime[i] = DateTime.MinValue;
					priorLowTime[i] = DateTime.MinValue;
				}
				for (int i=0; i<10; i++)
				{
					swingHigh[i] = double.MaxValue;
					swingLow[i] = double.MinValue;	
					swingHighTime[i] = DateTime.MinValue;
					swingLowTime[i] = DateTime.MinValue;	
					coveredHigh[i] = double.MaxValue;
					coveredLow[i] = double.MinValue;	
					coveredHighTime[i] = DateTime.MinValue;
					coveredLowTime[i] = DateTime.MinValue;					
					highPairIndex[i]= -1;
					lowPairIndex[i]= -1;		
					highFib[i]= double.MaxValue;	
					coupledLowFib[i]= double.MinValue;
					highFibTime[i]=DateTime.MinValue;	
					lowFib[i] = double.MinValue;
					coupledHighFib[i] = double.MaxValue;
					lowFibTime[i]=DateTime.MinValue;	
					highExtPairIndex[i]= -1;
					lowExtPairIndex[i]= -1;		
					highExtFib[i]= double.MaxValue;	
					coupledLowExtFib[i]= double.MinValue;
					highExtFibTime[i]=DateTime.MinValue;	
					lowExtFib[i] = double.MinValue;
					coupledHighExtFib[i] = double.MaxValue;
					lowExtFibTime[i]=DateTime.MinValue;	
				}
				
				for (int i=0; i<192; i++)
				{	
					fibCounter[i] = 0;
					plotlabel[i]= "empty";
				}
				
				sessionIterator = new SessionIterator(Bars);
			}
		}

		protected override void OnBarUpdate()
		{
			if (Bars == null)
				return;
			
			if (!Bars.BarsType.IsIntraday)
			{	
				if(BarsPeriod.BarsPeriodType != BarsPeriodType.Day)
					return;
				else if (BarsPeriod.Value > 1)
					return;
				else 
					dailyChart = true;
			}
			
			DateTime firstBarTime = Bars.GetTime(0);
			if (firstBarTime.DayOfWeek == DayOfWeek.Monday)
				firstBarTime = firstBarTime.AddDays(-2);
			if (firstBarTime.Date > DateTime.Now.AddDays(2-LookBack).Date )
			{
				Draw.TextFixed(this,"errortag", errorData, TextPosition.Center, ChartControl.Properties.AxisPen.Brush, textFont, Brushes.Transparent,Brushes.Transparent,0);
				return;
			}
			
			int extendedLookBack = 5*LookBack/4 + 5;

			if (Time[0]< DateTime.Now.AddDays(-extendedLookBack))
				return;

			if (!dailyChart)
			{
				sessionIterator.GetNextSession(Time[0], true);

				if (Offset == new TimeSpan(0,0,0) || Offset > sessionIterator.ActualSessionEnd.Subtract(sessionIterator.ActualSessionBegin))
					sessionLength = sessionEnd.Subtract(sessionBegin);
				else
					sessionLength = Offset;
			}
			
			double	high	=	High[0];
			double	low		=	Low[0];
			double	close	=	Close[0];
			
			if (Bars.IsFirstBarOfSession || dailyChart)
			{
				periodOpen = false;
				if (!firstPeriod)
				{
					sessionNumber = sessionNumber+1;
					for (int i=4;i>0;i--)
					{		
						priorHigh[i]=priorHigh[i-1];
						priorLow[i]=priorLow[i-1];
						priorClose[i]= priorClose[i-1];
						priorHighTime[i]=priorHighTime[i-1];
						priorLowTime[i]=priorLowTime[i-1];
					}
					priorHigh[0]		= currentHigh;
					priorLow[0]			= currentLow;
					priorClose[0]		= currentClose;
					priorHighTime[0]	= currentHighTime;
					priorLowTime[0] 	= currentLowTime;
					priorLowAfterHigh 	= currentLowAfterHigh;
					priorHighAfterLow 	= currentHighAfterLow;
					priorLowAfterHighTime = currentLowAfterHighTime;
					priorHighAfterLowTime = currentHighAfterLowTime;
					
					//Identifiying Swing Highs for Fibonacci Retracements
					existsNewPrimaryHigh=false;
					existsNewSecondaryHigh=false;
					coupledLowFibTemp =	Math.Min(coupledLowFibTemp, priorLow[0]);
					swingIndex=-1;

					if (priorHigh[2] > priorHigh[3] && priorHigh[2] > priorHigh[4] && // CHANGED HERE
						priorHigh[2] > priorHigh[1] && priorHigh[2] > priorHigh[0])	
					{	
						newPrimaryHigh = priorHigh[2];
						existsNewPrimaryHigh = true;
						if (priorHigh[0] >= priorHigh[1])
						{	
							newSecondaryHigh = priorHigh[0];
							existsNewSecondaryHigh = true;
							coupledLowFibTemp = priorLowAfterHigh;
							swingIndex=0;
						}
					}
					else if (priorHigh[1] > priorHigh[2] && priorHigh[1] > priorHigh[0])  // CHANGED HERE
					{	
						newSecondaryHigh = priorHigh[1];
						existsNewSecondaryHigh = true;
						swingIndex=1;
					}
					else if (priorHigh[0] > priorHigh[1]) // CHANGED HERE
					{
						newSecondaryHigh = priorHigh[0];
						existsNewSecondaryHigh = true;
						coupledLowFibTemp = priorLowAfterHigh;
						swingIndex=0;
					}
					
					// Filtering out insignificant Primary Highs occuring shortly after a higher Primary High
					for (int i=1; i<9; i++) 
					if (swingHighTime[i+1] != DateTime.MinValue)
					{
							shortperiod = (swingHighTime[0]-swingHighTime[i]).Days;
							longperiod = (swingHighTime[0]-swingHighTime[i+1]).Days;
							if (Convert.ToDouble(shortperiod)/Convert.ToDouble(longperiod) > Convert.ToDouble(Filter)/Convert.ToDouble(100))
						{
							for (int j=i; j<9; j++)
							{	
								swingHigh[j]=swingHigh[j+1];
								swingHighTime[j]=swingHighTime[j+1];
							}
							swingHigh[9]=double.MaxValue;
							swingHighTime[9]=DateTime.MinValue;
						}	
					}
					
					//Deleting old Secondary High
					if ((existsNewPrimaryHigh || existsNewSecondaryHigh) && existsOldSecondaryHigh)	
					{
						for (int i=0; i<9; i++)
						{
							swingHigh[i]=swingHigh[i+1];
							swingHighTime[i]=swingHighTime[i+1];
						}
						swingHigh[9]=double.MaxValue;
						swingHighTime[9]=DateTime.MinValue;
						existsOldSecondaryHigh=false;
					}
					
					//Adding new Primary High to Array
					if (existsNewPrimaryHigh)
					{
						if (newPrimaryHigh < swingHigh[0])
						{
							for (int i=9; i>0; i--)
							{
								swingHigh[i]=swingHigh[i-1];
								swingHighTime[i]=swingHighTime[i-1];
							}
						}
						else
						{
							rIndex = 1;
							for (int i=1; i<10; i++)
							{
								if (newPrimaryHigh >= swingHigh[i])
									rIndex = i+1;
							}
							// rIndex defines the number of Swing Highs to be deleted and transferred to Covered High Array
						    for (int i=9; i>=rIndex; i--)
							{
								coveredHigh[i]=coveredHigh[i-rIndex];
								coveredHighTime[i]=coveredHighTime[i-rIndex];
							}
							for (int i=0; i<rIndex; i++)
							{
								coveredHigh[i]=swingHigh[i];
								coveredHighTime[i]=swingHighTime[i];
							}
							// Covered Highs need to be sorted chronologically
							for (int i = 9; i>0; i--)
								for (int j=i; j>0; j--)
									if (coveredHighTime[j]>coveredHighTime[j-1])
									{
										rememberHigh = coveredHigh[j];
										coveredHigh[j] = coveredHigh[j-1];
										coveredHigh[j-1] = rememberHigh;
										rememberTime = coveredHighTime[j];
										coveredHighTime[j]= coveredHighTime[j-1];
										coveredHighTime[j-1]= rememberTime;
									}
							for (int i=0; i<10-rIndex; i++)
								if (rIndex>1)	
								{
									swingHigh[i+1]=swingHigh[i+rIndex];	
									swingHighTime[i+1]=swingHighTime[i+rIndex];
								}
							for (int i=9; i>10-rIndex; i--)
							{
								swingHigh[i]=double.MaxValue;	
								swingHighTime[i]=DateTime.MinValue;	
							}
						}
						swingHigh[0]=newPrimaryHigh;
						swingHighTime[0]= priorHighTime[2];
					}
					
					//Adding new Secondary High to Array
					if (existsNewSecondaryHigh)
					{	
						if (newSecondaryHigh < swingHigh[0])
						{
							for (int i=9; i>0; i--)
							{
								swingHigh[i]=swingHigh[i-1];
								swingHighTime[i]=swingHighTime[i-1];
							}
						}
						else
						{	
							rIndex = 1;
							for (int i=1; i<10; i++)
							{
								if (newSecondaryHigh >= swingHigh[i])
									rIndex = i+1;
							}
							// rIndex defines the number of Swing Highs to be deleted and transferred to Covered High Array
						    for (int i=9; i>=rIndex; i--)
							{
								coveredHigh[i]=coveredHigh[i-rIndex];
								coveredHighTime[i]=coveredHighTime[i-rIndex];
							}
							for (int i=0; i<rIndex; i++)
							{
								coveredHigh[i]=swingHigh[i];
								coveredHighTime[i]=swingHighTime[i];
							}
							// Covered Highs need to be sorted chronologically
							for (int i = 9; i>0; i--)
								for (int j=i; j>0; j--)
									if (coveredHighTime[j]>coveredHighTime[j-1])
									{
										rememberHigh = coveredHigh[j];
										coveredHigh[j] = coveredHigh[j-1];
										coveredHigh[j-1] = rememberHigh;
										rememberTime = coveredHighTime[j];
										coveredHighTime[j]= coveredHighTime[j-1];
										coveredHighTime[j-1]= rememberTime;
									}
							for (int i=0; i<10-rIndex; i++)
								if(rIndex>1)
								{
									swingHigh[i+1]=swingHigh[i+rIndex];	
									swingHighTime[i+1]=swingHighTime[i+rIndex];
								}
							for (int i=9; i>10-rIndex; i--)
							{
								swingHigh[i]=double.MaxValue;	
								swingHighTime[i]=DateTime.MinValue;	
							}
						}	
						swingHigh[0]=newSecondaryHigh;
						swingHighTime[0]= priorHighTime[swingIndex];
						existsOldSecondaryHigh = true;
					}	
			
					//Identifiying Swing Lows for Fibonacci Retracements
					existsNewPrimaryLow=false;
					existsNewSecondaryLow=false;
					coupledHighFibTemp = Math.Max(coupledHighFibTemp, priorHigh[0]);
					swingIndex=-1;
					if (priorLow[2] < priorLow[3] && priorLow[2]<priorLow[4] &&  //CHANGED HERE
						priorLow[2] < priorLow[1] && priorLow[2]<priorLow[0])	
					{	
						newPrimaryLow = priorLow[2];
						existsNewPrimaryLow = true;
						if (priorLow[0] <= priorLow[1])
						{	
							newSecondaryLow = priorLow[0];
							existsNewSecondaryLow = true;
							coupledHighFibTemp = priorHighAfterLow;
							swingIndex=0;
						}
					}
					else if (priorLow[1] < priorLow[2] && priorLow[1] < priorLow[0])  // CHANGED HERE
					{	
						newSecondaryLow = priorLow[1];
						existsNewSecondaryLow = true;
						swingIndex=1;
					}
					else if (priorLow[0] < priorLow[1]) // CHANGED HERE
					{
						newSecondaryLow = priorLow[0];
						existsNewSecondaryLow = true;
						coupledHighFibTemp = priorHighAfterLow;
						swingIndex=0;
					}
					
					// Filtering out insignificant Primary Lows occuring shortly after a lower Primary Low
					for (int i=1; i<9; i++) 
					if (swingLowTime[i+1] != DateTime.MinValue)
					{
							shortperiod = (swingLowTime[0]-swingLowTime[i]).Days;
							longperiod = (swingLowTime[0]-swingLowTime[i+1]).Days;
							if (Convert.ToDouble(shortperiod)/Convert.ToDouble(longperiod) > Convert.ToDouble(Filter)/Convert.ToDouble(100))
						{
							for (int j=i; j<9; j++)
							{	
								swingLow[j]=swingLow[j+1];
								swingLowTime[j]=swingLowTime[j+1];
							}
							swingLow[9]=double.MinValue;
							swingLowTime[9]=DateTime.MinValue;	
						}	
					}
					
					//Deleting old Secondary Low
					if ((existsNewPrimaryLow == true || existsNewSecondaryLow == true) && existsOldSecondaryLow == true )	
					{
						for (int i=0; i<9; i++)
						{
							swingLow[i]=swingLow[i+1];
							swingLowTime[i]=swingLowTime[i+1];
						}
						swingLow[9]=double.MinValue;
						swingLowTime[9]=DateTime.MinValue;
						existsOldSecondaryLow = false;
					}
					
					//Adding new Primary Low to Array
					if (existsNewPrimaryLow)
					{
						if (newPrimaryLow > swingLow[0])
						{
							for (int i=9; i>0; i--)
							{
								swingLow[i]=swingLow[i-1];
								swingLowTime[i]=swingLowTime[i-1];
							}
						}
						else
						{
							rIndex = 1;
							for (int i=1; i<10; i++)
							{
								if (newPrimaryLow <= swingLow[i])
									rIndex = i+1;
							}
							// rIndex defines the number of Swing Lows to be deleted amd transferred to Covered Low Array
						    for (int i=9; i>=rIndex; i--)
							{
								coveredLow[i]=coveredLow[i-rIndex];
								coveredLowTime[i]=coveredLowTime[i-rIndex];
							}
							for (int i=0; i<rIndex; i++)
							{
								coveredLow[i]=swingLow[i];
								coveredLowTime[i]=swingLowTime[i];
							}
							// Covered Lows need to be sorted chronologically
							for (int i = 9; i>0; i--)
								for (int j=i; j>0; j--)
									if (coveredLowTime[j]>coveredLowTime[j-1])
									{
										rememberLow = coveredLow[j];
										coveredLow[j] = coveredLow[j-1];
										coveredLow[j-1] = rememberLow;
										rememberTime = coveredLowTime[j];
										coveredLowTime[j]= coveredLowTime[j-1];
										coveredLowTime[j-1]= rememberTime;
									}
							for (int i=0; i<10-rIndex; i++)
								if (rIndex>1)
								{
									swingLow[i+1]=swingLow[i+rIndex];	
									swingLowTime[i+1]=swingLowTime[i+rIndex];
								}
							for (int i=9; i>10-rIndex; i--)
							{
								swingLow[i]=double.MinValue;	
								swingLowTime[i]=DateTime.MinValue;	
							}
						}
						swingLow[0]=newPrimaryLow;
						swingLowTime[0]= priorLowTime[2];
					}
					
					//Adding new Secondary Low to Array
					if (existsNewSecondaryLow)
					{	
						if (newSecondaryLow > swingLow[0])
						{
							for (int i=9; i>0; i--)
							{
								swingLow[i]=swingLow[i-1];
								swingLowTime[i]=swingLowTime[i-1];
							}
						}
						else
						{	
							rIndex = 1;
							for (int i=1; i<10; i++)
							{
								if (newSecondaryLow <= swingLow[i])
									rIndex = i+1;
							}
							// rIndex defines the number of Swing Lows to be deleted amd transferred to Covered Low Array
						    for (int i=9; i>=rIndex; i--)
							{
								coveredLow[i]=coveredLow[i-rIndex];
								coveredLowTime[i]=coveredLowTime[i-rIndex];
							}
							for (int i=0; i<rIndex; i++)
							{
								coveredLow[i]=swingLow[i];
								coveredLowTime[i]=swingLowTime[i];
							}
							// Covered Lows need to be sorted chronologically
							for (int i = 9; i>0; i--)
								for (int j=i; j>0; j--)
									if (coveredLowTime[j]>coveredLowTime[j-1])
									{
										rememberLow = coveredLow[j];
										coveredLow[j] = coveredLow[j-1];
										coveredLow[j-1] = rememberLow;
										rememberTime = coveredLowTime[j];
										coveredLowTime[j]= coveredLowTime[j-1];
										coveredLowTime[j-1]= rememberTime;
									}
							for (int i=0; i<10-rIndex; i++)
							{
								if (rIndex>1)
								{
									swingLow[i+1]=swingLow[i+rIndex];	
									swingLowTime[i+1]=swingLowTime[i+rIndex];
								}
							}
							for (int i=9; i>10-rIndex; i--)
							{
								swingLow[i]=double.MinValue;	
								swingLowTime[i]=DateTime.MinValue;	
							}
						}
						swingLow[0]=newSecondaryLow;
						swingLowTime[0]= priorLowTime[swingIndex];;
						existsOldSecondaryLow = true;
					}	
		
					//Creating Fibonacci Retracements from Swing Highs and Lows Arrays		
					if (existsNewPrimaryHigh || existsNewSecondaryHigh || existsNewPrimaryLow || existsNewSecondaryLow)
					{
						
						//Selecting Approriate Lows for Highs
						highFibIndex=-1;
						for (int i=0; i<10; i++)
						{	
							if (swingHighTime[i]== DateTime.MinValue)
							break; // High number i and following numbers cannot be used for Fib retracements
							
							highPairIndex[i]=-1;
							for (int j=0; j<10; j++)
							{	
							if (swingHighTime[i]<swingLowTime[j])
							highPairIndex[i]=j;
							}
							if (highPairIndex[i] > -1)
							{
								highFibIndex = highFibIndex+1;
								highFib[highFibIndex]=swingHigh[i];
								coupledLowFib[highFibIndex]= swingLow[highPairIndex[i]];
								highFibTime[highFibIndex] = swingHighTime[i];
							}
						}
	
						//Selecting Fibonacci Extensions for Covered Highs
						highExtIndex=-1;
						for (int i=0; i<10; i++)
						{	
							for (int j=0; j<10; j++)
							if (coveredHighTime[i] <= swingLowTime[j])
								highExtPairIndex[i] = j;
							if (coveredHighTime[i] == DateTime.MinValue)
								highExtPairIndex[i] = -1;
						}
						for (int i=1; i<10; i++)
							for (int j = 1; j<=i; j++)
								if (coveredHigh[i] < coveredHigh[i-j] && highExtPairIndex[i] == highExtPairIndex[i-j])
									highExtPairIndex[i] = -1;
						
						for (int i=0; i<10; i++)
						{	
							if (highExtPairIndex[i] > -1)
							{
								highExtIndex = highExtIndex+1;
								highExtFib[highExtIndex]=coveredHigh[i];
								coupledLowExtFib[highExtIndex]= swingLow[highExtPairIndex[i]];
								highExtFibTime[highExtIndex]= coveredHighTime[i];
							}
						}
						
						//Selecting Appropriate Highs for Lows
						lowFibIndex=-1;
						for (int i=0; i<10; i++)
						{	
							if (swingLowTime[i]== DateTime.MinValue)
							break; // Low number i and following numbers cannot be used for Fib retracements
							lowPairIndex[i]=-1;
							for (int j=0; j<10; j++)
							{	
							if (swingLowTime[i]<=swingHighTime[j])
							lowPairIndex[i]=j;
							}
							if (lowPairIndex[i] > -1)
							{
								lowFibIndex = lowFibIndex+1;
								lowFib[lowFibIndex]=swingLow[i];
								coupledHighFib[lowFibIndex]= swingHigh[lowPairIndex[i]];
								lowFibTime[lowFibIndex]= swingLowTime[i];
							}
						}
						
						//Selecting Fibonacci Extensions for Covered Lows
						lowExtIndex=-1;
						for (int i=0; i<10; i++)
						{	
							for (int j=0; j<10; j++)
							if (coveredLowTime[i] <= swingHighTime[j])
								lowExtPairIndex[i] = j;
							if (coveredLowTime[i] == DateTime.MinValue)
								lowExtPairIndex[i] = -1;
						}
						for (int i=1; i<10; i++)
							for (int j = 1; j<=i; j++)
								if (coveredLow[i] > coveredLow[i-j] && lowExtPairIndex[i] == lowExtPairIndex[i-j])
									lowExtPairIndex[i] = -1;
						
						for (int i=0; i<10; i++)
						{	
							if (lowExtPairIndex[i] > -1)
							{
								lowExtIndex = lowExtIndex+1;
								lowExtFib[lowExtIndex]=coveredLow[i];
								coupledHighExtFib[lowExtIndex]= swingHigh[lowExtPairIndex[i]];
								lowExtFibTime[lowExtIndex]= coveredLowTime[i];
							}
						}
					}
				}
				firstPeriod = false;
				if (Time[0] > sessionEnd.Subtract(sessionLength)|| dailyChart)				
				{	
					currentHighTime = Time[0];
					currentLowTime = Time[0];
					currentLowAfterHighTime = Time[0];
					currentHighAfterLowTime = Time[0];
					runningHighTime = Time[0];
					runningLowTime = Time[0];
					currentHigh		= high;
					currentLow		= low;
					currentClose	= close;
					currentLowAfterHigh = close;
					currentHighAfterLow = close;
					runningHigh		= High[0];
					runningLow		= Low[0];
					periodOpen		= true;
				}
			}
			else if (!periodOpen && Time[0] > sessionEnd.Subtract(sessionLength))
			{
				currentHighTime 		= Time[0]; 
				currentLowTime 			= Time[0];
				currentLowAfterHighTime = Time[0];
				currentHighAfterLowTime = Time[0];
				runningHighTime 		= Time[0];
				runningLowTime 			= Time[0];
				currentHigh			= high;
				currentLow			= low;
				currentClose		= close;
				currentLowAfterHigh = close;
				currentHighAfterLow = close;
				runningHigh			= High[0];
				runningLow			= Low[0];
				periodOpen			=  true;
			}
			else if (periodOpen && Time[0] > sessionEnd.Subtract(sessionLength))	
			{
				if (high > currentHigh)
				{	
					currentHighTime = Time[0];
					currentLowAfterHigh = close;
					currentLowAfterHighTime = Time[0];
				}
				if (low < currentLow)
				{
					currentLowTime = Time[0];
					currentHighAfterLow = close;
					currentHighAfterLowTime = Time[0];
				}
				if (high <= currentHigh)
				{
					if (low < currentLowAfterHigh)
						currentLowAfterHighTime = Time[0];
					currentLowAfterHigh = Math.Min(currentLowAfterHigh, low);
				}
				if (low >= currentLow)
				{
					if (high > currentHighAfterLow)
						currentHighAfterLowTime = Time[0];
					currentHighAfterLow = Math.Max(currentHighAfterLow, high);
				}
				currentHigh		= Math.Max(currentHigh, high);
				currentLow		= Math.Min(currentLow, low);
				currentClose	= close;
				if(High[0] > runningHigh)
					runningHighTime = Time[0];
				if(Low[0] < runningLow)
					runningLowTime = Time[0];
				runningHigh		= Math.Max(runningHigh, High[0]);
				runningLow		= Math.Min(runningLow, Low[0]);
			}
			if (!dailyChart)
			{
				//Creating Fibonacci Retracement from last Swing High, Swing Low in both directions 
				runningHighIndex = -1;
				runningLowIndex = -1;
				if (runningLow < swingLow[0] && runningHigh <= swingHigh[0] && priorHigh[0]!=swingHigh[0]) // Trend Down
				{
					if (swingHighTime[0] > swingLowTime[0]) 
					{
						lastHighFib = swingHigh[0];
						lastHighFibTime = swingHighTime[0];
						lastCoupledLowFib = runningLow;
						runningHighIndex = 0;
					}
				}
				else if (runningHigh > swingHigh[0] && runningLow >= swingLow[0] && priorLow[0]!=swingLow[0]) // Trend Up
				{
					if (swingLowTime[0] > swingHighTime[0])
					{
						lastLowFib = swingLow[0];
						lastLowFibTime = swingLowTime[0];
						lastCoupledHighFib = runningHigh;
						runningLowIndex = 0;
					}
				}
				else if (runningLow >= swingLow[0] && runningHigh <= swingHigh[0]) // Inside Range
				{
					if (swingLowTime[0] >= swingHighTime[0]) 
					{	
						if (swingLow[0] != priorLow[0]) // Creating Fib Retracement from Swing Low to the Right 
						{
							lastLowFib = swingLow[0];
							lastLowFibTime = swingLowTime[0];
							lastCoupledHighFib = Math.Max(coupledHighFibTemp, runningHigh);
							runningLowIndex = 1;	
						}	
						if (priorHighTime[1]>swingHighTime[0] && swingLowTime[0]>priorHighTime[1] && priorHigh[1]>priorHigh[0] // Creating Fib Retracement backwards 
							&& (priorHigh[1]-priorClose[2] > 0.20*(priorHigh[1]-priorLow[1]) || priorHigh[1]-priorClose[2] > 0.20*(priorHigh[2]-priorLow[2])
							|| priorHighTime[1]>priorLowTime[1]))
						{
								lastHighFib = priorHigh[1];
								lastHighFibTime = priorHighTime[1];
								lastCoupledLowFib = Math.Min(swingLow[0],runningLow);
								runningHighIndex = 1;
						}
					}
					else if (swingHighTime[0] >= swingLowTime[0])
					{	
						if (swingHigh[0] != priorHigh[0]) // Creating Fib Retracement from Swing High to the Right 
						{
							lastHighFib = swingHigh[0];
							lastHighFibTime = swingHighTime[0];
							lastCoupledLowFib = Math.Min(coupledLowFibTemp, runningLow);
							runningHighIndex = 1;
						}	
						if (priorLowTime[1]>swingLowTime[0] && swingHighTime[0]>priorLowTime[1] && priorLow[1]<priorLow[0] //Creating Fib Retracement backwards
							&& (priorClose[2]-priorLow[1] > 0.20*(priorHigh[1]-priorLow[1])|| priorClose[2]-priorLow[1] > 0.20*(priorHigh[2]-priorLow[2])
							|| priorLowTime[1]> priorHighTime[1]))
						{
								lastLowFib = priorLow[1];
								lastLowFibTime = priorLowTime[1];
								lastCoupledHighFib = Math.Max(swingHigh[0],runningHigh);
								runningLowIndex = 1;
						}
					}
				}			
				// Creating Fib retracement from yesterday's (or from the day before yesterday) and today's Highs and Lows
				recentDown = false;
				recentUp = false;
				zExtensionDown = false;
				zExtensionUp = false;
				if (runningHigh > priorHigh[0] && runningLow >= priorLow[0] )// Trend Up
				{
					if (priorHigh[0]!=swingHigh[0] || priorLow[0]!=swingLow[0] || priorLowTime[0]>priorHighTime[0])
					{
						if (priorLow[0]==swingLow[0] || priorClose[1]-priorLow[0] > 0.2*(priorHigh[0]-priorLow[0]) 
							|| priorClose[1]-priorLow[0] > 0.2*(priorHigh[1]-priorLow[1]) || priorLowTime[0]>priorHighTime[0] ) // this is to exclude insignificant Lows
						{
								recentLow = priorLow[0];
								recentLowTime = priorLowTime[0];	
								recentCoupledHigh = runningHigh;
								recentDown = true;
						}
						else if ((priorLow[1] != swingLow[0])&& (priorClose[2]-priorLow[1] > 0.2*(priorHigh[1]-priorLow[1]) 
							|| priorClose[2]-priorLow[1] > 0.2*(priorHigh[2]-priorLow[2]) || priorLowTime[1]>priorHighTime[1])) // if yesterday's Low ain't no good, take the day before
						{
								recentLow = priorLow[1];
								recentLowTime = priorLowTime[1];	
								recentCoupledHigh = runningHigh;
								recentDown = true;
						}	
					}
					//the following section only is needed to display upward extensions after prior high has been taken out
					if (priorHighTime[0]>=priorLowTime[0]) 
					{
						recentHigh = priorHigh[0];
						recentHighTime = priorHighTime[0]; 
						recentCoupledLow = Math.Min(runningLow, priorLowAfterHigh);
						recentUp = true;
						zExtensionUp = true;
					}
					if (priorLowTime[0]>priorHighTime[0])
					{
						if ((priorHigh[0]==swingHigh[0] || priorHigh[0]-priorClose[1] > 0.2*(priorHigh[0]-priorLow[0])
							|| priorHigh[0]-priorClose[1] > 0.2*(priorHigh[1]-priorLow[1]) || priorHighTime[0]>priorLowTime[0])
							&& (priorHigh[0]!=swingHigh[0] || priorLow[0]!=swingLow[0]))
						{
							recentHigh = priorHigh[0];
							recentHighTime = priorHighTime[0];
							recentCoupledLow = priorLow[0];
							recentUp = true;
							zExtensionUp = true;
						}	
					}	
					// end of section for display of upward extensions 
				}
				else if (runningHigh <= priorHigh[0] && runningLow < priorLow[0] )// Trend Down
				{
					if (priorHigh[0]!=swingHigh[0] || priorLow[0]!=swingLow[0] || priorHighTime[0]>priorLowTime[0])
					{	
						
						if (priorHigh[0]==swingHigh[0] || priorHigh[0]-priorClose[1] > 0.2*(priorHigh[0]-priorLow[0])
							|| priorHigh[0]-priorClose[1] > 0.2*(priorHigh[1]-priorLow[1]) || priorHighTime[0] > priorLowTime[0]) // this is to exclude insignificant Highs
						{
								recentHigh = priorHigh[0];
								recentHighTime = priorHighTime[0];
								recentCoupledLow = runningLow;
								recentUp = true;
						}
						else if ((priorHigh[1] != swingHigh[0])&& (priorHigh[1]-priorClose[2] > 0.2*(priorHigh[1]-priorLow[1]) 
							|| priorHigh[1]-priorClose[2] > 0.2*(priorHigh[2]-priorLow[2])|| priorHighTime[1]>priorLowTime[1])) // if yesterday's High ain't no good take the day before
						{
								recentHigh = priorHigh[1];
								recentHighTime = priorHighTime[1];	
								recentCoupledLow = runningLow;
								recentUp = true;
						}		
					}
					//the following section only is needed to display downward extensions after prior low has been taken out
					if (priorHighTime[0]>=priorLowTime[0])
					{
						if ((priorLow[0]==swingLow[0] || priorClose[1]-priorLow[0] > 0.2*(priorHigh[0]-priorLow[0]) 
							|| priorClose[1]-priorLow[0] > 0.2*(priorHigh[1]-priorLow[1]) || priorLowTime[0]>priorHighTime[0])
							&& (priorHigh[0]!=swingHigh[0] || priorLow[0]!=swingLow[0]))
						{
							recentLow = priorLow[0];
							recentLowTime = priorLowTime[0];
							recentCoupledHigh = priorHigh[0];
							recentDown = true;
							zExtensionDown = true;
						}
					}
					if (priorLowTime[0]>priorHighTime[0])
					{
						recentLow = priorLow[0];
						recentLowTime = priorLowTime[0];
						recentCoupledHigh = Math.Max(runningHigh,priorHighAfterLow);
						recentDown = true;
						zExtensionDown = true;
					}
					// end of section for display of downward extensions 
				}
				else if (runningHigh <= priorHigh[0] && runningLow >= priorLow[0]) //Inside Bar
				{
					if (priorHighTime[0]>=priorLowTime[0]) // this will be displayed with Daily Bars as both times are identical
					{
						if ((priorLow[0]==swingLow[0] || priorClose[1]-priorLow[0] > 0.2*(priorHigh[0]-priorLow[0]) 
							|| priorClose[1]-priorLow[0] > 0.2*(priorHigh[1]-priorLow[1]) || priorLowTime[0]>priorHighTime[0])
							&& (priorHigh[0]!=swingHigh[0] || priorLow[0]!=swingLow[0]))
						{
							recentLow = priorLow[0];
							recentLowTime = priorLowTime[0];
							recentCoupledHigh = priorHigh[0];
							recentDown = true;
						}
						recentHigh = priorHigh[0];
						recentHighTime = priorHighTime[0]; 
						recentCoupledLow = Math.Min(runningLow, priorLowAfterHigh);
						recentUp = true;
					}
					if (priorLowTime[0]>priorHighTime[0])	// this will never occur with Daily Bars
					{
						if ((priorHigh[0]==swingHigh[0] || priorHigh[0]-priorClose[1] > 0.2*(priorHigh[0]-priorLow[0])
							|| priorHigh[0]-priorClose[1] > 0.2*(priorHigh[1]-priorLow[1]) || priorHighTime[0]>priorLowTime[0])
							&& (priorHigh[0]!=swingHigh[0] || priorLow[0]!=swingLow[0]))
						{
							recentHigh = priorHigh[0];
							recentHighTime = priorHighTime[0];
							recentCoupledLow = priorLow[0];
							recentUp = true;
						}	
						recentLow = priorLow[0];
						recentLowTime = priorLowTime[0];
						recentCoupledHigh = Math.Max(runningHigh,priorHighAfterLow);
						recentDown = true;
					}	
				}	
				else if (runningHigh > priorHigh[0] && runningLow < priorLow[0]) // Outside Bar
				{
					if (runningLowTime >= runningHighTime)
					{
						recentHigh = runningHigh;
						recentHighTime = runningHighTime;
						recentCoupledLow = runningLow;
						recentUp = true;
					}
				
					if (runningHighTime > runningLowTime)	
					{
						recentLow = runningLow;
						recentLowTime = runningLowTime;
						recentCoupledHigh = runningHigh;
						recentDown = true;
					}		
				}
			}
			
			if (highExtIndex>-1 && Show_Xtensions)
			{
				fib1000 = highExtFib[0]/TickSize;
				fib000 = coupledLowExtFib[0]/TickSize;
				Extension0[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension1[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension2[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				highExtDate[0][0] = (highExtFibTime[0]);
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[0] && swingHigh[i]>Extension1[0])
				{
					Extension0[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[0] && swingHigh[i]>Extension2[0])
				{
					Extension1[0] = 0;
					break;
				}
			}
			else
			{
				Extension0[0] = 0;
				Extension1[0] = 0;
				Extension2[0] = 0;
				highExtDate[0][0] = (DateTime.MinValue);
			}
			
			if (highExtIndex>0 && Show_Xtensions)
			{
				fib1000 = highExtFib[1]/TickSize;
				fib000 = coupledLowExtFib[1]/TickSize;
				Extension3[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension4[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension5[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				highExtDate[1][0] = (highExtFibTime[1]);
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[1] && swingHigh[i]>Extension4[0])
				{
					Extension3[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[1] && swingHigh[i]>Extension5[0])
				{
					Extension4[0] = 0;
					break;
				}
			}
			else
			{
				Extension3[0] = 0;
				Extension4[0] = 0;
				Extension5[0] = 0;
				highExtDate[1][0] = (DateTime.MinValue);
			}
			
			if (highExtIndex>1 && Show_Xtensions)
			{
				fib1000 = highExtFib[2]/TickSize;
				fib000 = coupledLowExtFib[2]/TickSize;
				Extension6[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension7[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension8[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				highExtDate[2][0] = (highExtFibTime[2]);
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[2] && swingHigh[i]>Extension7[0])
				{
					Extension6[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[2] && swingHigh[i]>Extension8[0])
				{
					Extension7[0] = 0;
					break;
				}
			}
			else
			{
				Extension6[0] = 0;
				Extension7[0] = 0;
				Extension8[0] = 0;
				highExtDate[2][0] = (DateTime.MinValue);
			}
			
			if (highExtIndex>2 && Show_Xtensions)
			{
				fib1000 = highExtFib[3]/TickSize;
				fib000 = coupledLowExtFib[3]/TickSize;
				Extension9[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension10[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension11[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				highExtDate[3][0] = (highExtFibTime[3]);
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[3] && swingHigh[i]>Extension10[0])
				{
					Extension9[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[3] && swingHigh[i]>Extension11[0])
				{
					Extension10[0] = 0;
					break;
				}
			}
			else
			{
				Extension9[0] = 0;
				Extension10[0] = 0;
				Extension11[0] = 0;
				highExtDate[3][0] = (DateTime.MinValue);
			}
			
			if (highExtIndex>3 && Show_Xtensions)
			{
				fib1000 = highExtFib[4]/TickSize;
				fib000 = coupledLowExtFib[4]/TickSize;
				Extension12[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension13[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension14[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				highExtDate[4][0] = (highExtFibTime[4]);
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[4] && swingHigh[i]>Extension13[0])
				{
					Extension12[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[4] && swingHigh[i]>Extension14[0])
				{
					Extension13[0] = 0;
					break;
				}
			}
			else
			{
				Extension12[0] = 0;
				Extension13[0] = 0;
				Extension14[0] = 0;
				highExtDate[4][0] = (DateTime.MinValue);
			}
			
			if (highExtIndex>4 && Show_Xtensions)
			{
				fib1000 = highExtFib[5]/TickSize;
				fib000 = coupledLowExtFib[5]/TickSize;
				Extension15[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension16[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension17[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				highExtDate[5][0] = (highExtFibTime[5]);
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[5] && swingHigh[i]>Extension16[0])
				{
					Extension15[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingHighTime[i]>highExtFibTime[5] && swingHigh[i]>Extension17[0])
				{
					Extension16[0] = 0;
					break;
				}
			}
			else
			{
				Extension15[0] = 0;
				Extension16[0] = 0;
				Extension17[0] = 0;
				highExtDate[5][0] = (DateTime.MinValue);
			}

			if (lowExtIndex>-1)
			{
				fib1000 = lowExtFib[0]/TickSize;
				fib000 = coupledHighExtFib[0]/TickSize;
				Extension18[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension19[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension20[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				lowExtDate[0][0] = (lowExtFibTime[0]);
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[0] && swingLow[i]<Extension19[0])
				{
					Extension18[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[0] && swingLow[i]<Extension20[0])
				{
					Extension19[0] = 0;
					break;
				}
			}
			else
			{
				Extension18[0] = 0;
				Extension19[0] = 0;
				Extension20[0] = 0;
				lowExtDate[0][0] = DateTime.MinValue;
			}
			
			if (lowExtIndex>0 && Show_Xtensions)
			{
				fib1000 = lowExtFib[1]/TickSize;
				fib000 = coupledHighExtFib[1]/TickSize;
				Extension21[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension22[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension23[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				lowExtDate[1][0] = (lowExtFibTime[1]);
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[1] && swingLow[i]<Extension22[0])
				{
					Extension21[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[1] && swingLow[i]<Extension23[0])
				{
					Extension22[0] = 0;
					break;
				}
			}
			else
			{
				Extension21[0] = 0;
				Extension22[0] = 0;
				Extension23[0] = 0;
				lowExtDate[1][0] = (DateTime.MinValue);
			}

			if (lowExtIndex>1 && Show_Xtensions)
			{
				fib1000 = lowExtFib[2]/TickSize;
				fib000 = coupledHighExtFib[2]/TickSize;
				Extension24[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension25[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension26[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				lowExtDate[2][0] = (lowExtFibTime[2]);
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[2] && swingLow[i]<Extension25[0])
				{
					Extension24[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[2] && swingLow[i]<Extension26[0])
				{
					Extension25[0] = 0;
					break;
				}
			}
			else
			{
				Extension24[0] = 0;
				Extension25[0] = 0;
				Extension26[0] = 0;
				lowExtDate[2][0] = (DateTime.MinValue);
			}
			
			if (lowExtIndex>2 && Show_Xtensions)
			{
				fib1000 = lowExtFib[3]/TickSize;
				fib000 = coupledHighExtFib[3]/TickSize;
				Extension27[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension28[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension29[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				lowExtDate[3][0] = (lowExtFibTime[3]);
			
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[3] && swingLow[i]<Extension28[0])
				{
					Extension27[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[3] && swingLow[i]<Extension29[0])
				{
					Extension28[0] = 0;
					break;
				}
			}
			else
			{
				Extension27[0] = 0;
				Extension28[0] = 0;
				Extension29[0] = 0;
				lowExtDate[3][0] = (DateTime.MinValue);
			}
			
			if (lowExtIndex>3 && Show_Xtensions)
			{
				fib1000 = lowExtFib[4]/TickSize;
				fib000 = coupledHighExtFib[4]/TickSize;
				Extension30[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension31[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension32[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				lowExtDate[4][0] = (lowExtFibTime[4]);
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[4] && swingLow[i]<Extension31[0])
				{
					Extension30[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[4] && swingLow[i]<Extension32[0])
				{
					Extension31[0] = 0;
					break;
				}
			}
			else
			{
				Extension30[0] = 0;
				Extension31[0] = 0;
				Extension32[0] = 0;
				lowExtDate[4][0] = (DateTime.MinValue);
			}
			
			if (lowExtIndex>4 && Show_Xtensions)
			{
				fib1000 = lowExtFib[5]/TickSize;
				fib000 = coupledHighExtFib[5]/TickSize;
				Extension33[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				Extension34[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				Extension35[0] = (TickSize*Math.Round(fib000+2.0*(fib1000-fib000)));
				lowExtDate[5][0] = (lowExtFibTime[5]);
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[5] && swingLow[i]<Extension34[0])
				{
					Extension33[0] = 0;
					break;
				}
				for (int i=0;i<=9;i++)
				if (swingLowTime[i]>lowExtFibTime[5] && swingLow[i]<Extension35[0])
				{
					Extension34[0] = 0;
					break;
				}
			}
			else
			{
				Extension33[0] = 0;
				Extension34[0] = 0;
				Extension35[0] = 0;
				lowExtDate[5][0] = (DateTime.MinValue);
			}
	
			if (sessionNumber > 0 && recentUp == true && Show_Z_Plus && Bars.BarsType.IsIntraday)
			{
				fib1000 = recentHigh/TickSize;
				fib000 = recentCoupledLow/TickSize;
				if (runningHigh<=recentHigh+0.618*(recentHigh-recentCoupledLow))
					ZplusFib000[0] = (TickSize*Math.Round(fib000));
				else
					ZplusFib000[0] = 0;
				if (!zExtensionUp)
				{	
					ZplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					ZplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					ZplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					ZplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					ZplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					ZplusFib236[0] = 0;
					ZplusFib382[0] = 0;
					ZplusFib500[0] = 0;
					ZplusFib618[0] = 0;
					ZplusFib764[0] = 0;
				}
				ZplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh<=recentHigh+0.618*(recentHigh-recentCoupledLow))  // CHANGED HERE
					ZplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				else
					ZplusFib1272[0] = 0;
				if (runningHigh<=recentHigh+(recentHigh-recentCoupledLow)) // CHANGED HERE
					ZplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				else
					ZplusFib1618[0] = 0;
				recentHighDate[0] = (recentHighTime);
			}
			else
			{
				ZplusFib000[0] = 0;
				ZplusFib236[0] = 0;
				ZplusFib382[0] = 0;
				ZplusFib500[0] = 0;
				ZplusFib618[0] = 0;
				ZplusFib764[0] = 0;
				ZplusFib1000[0] = 0;
				ZplusFib1272[0] = 0;
				ZplusFib1618[0] = 0;
				recentHighDate[0] = (DateTime.MinValue);
			}
			
			if (sessionNumber > 0 && recentDown == true && Show_Z_Minus && Bars.BarsType.IsIntraday)
			{
				fib1000 = recentLow/TickSize;
				fib000 = recentCoupledHigh/TickSize;
				if (runningLow >= recentLow-0.618*(recentCoupledHigh-recentLow))
					ZminusFib000[0] = (TickSize*Math.Round(fib000));
				else
					ZminusFib000[0] = 0;
				if (!zExtensionDown)
				{	
					ZminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					ZminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					ZminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					ZminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					ZminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					ZminusFib236[0] = 0;
					ZminusFib382[0] = 0;
					ZminusFib500[0] = 0;
					ZminusFib618[0] = 0;
					ZminusFib764[0] = 0;
				}
				ZminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow >= recentLow-0.618*(recentCoupledHigh-recentLow)) // CHANGED HERE
					ZminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				else
					ZminusFib1272[0] = 0;
				if (runningLow >= recentLow-(recentCoupledHigh-recentLow))  // CHANGED HERE
					ZminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				else
					ZminusFib1618[0] = 0;
				recentLowDate[0] = (recentLowTime);
			}
			else
			{
				ZminusFib000[0] = 0;
				ZminusFib236[0] = 0;
				ZminusFib382[0] = 0;
				ZminusFib500[0] = 0;
				ZminusFib618[0] = 0;
				ZminusFib764[0] = 0;
				ZminusFib1000[0] = 0;
				ZminusFib1272[0] = 0;
				ZminusFib1618[0] = 0;
				recentLowDate[0] = (DateTime.MinValue);
			}

			if (sessionNumber > 2 && runningHighIndex > -1 && Show_Y_Plus && Bars.BarsType.IsIntraday)
			{
				fib1000 = lastHighFib/TickSize;
				fib000 = lastCoupledLowFib/TickSize;
				YplusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningHigh < lastHighFib)
				{
					YplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					YplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					YplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					YplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					YplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					YplusFib236[0] = 0;
					YplusFib382[0] = 0;
					YplusFib500[0] = 0;
					YplusFib618[0] = 0;
					YplusFib764[0] = 0;
				}
				YplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh < lastHighFib || runningLow > lastCoupledLowFib || runningLowTime<runningHighTime)
				{
					YplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
					YplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					YplusFib1272[0] = 0;
					YplusFib1618[0] = 0;
				}						
				lastHighDate[0] = (lastHighFibTime);
			}
			else
			{
				YplusFib000[0] = 0;
				YplusFib236[0] = 0;
				YplusFib382[0] = 0;
				YplusFib500[0] = 0;
				YplusFib618[0] = 0;
				YplusFib764[0] = 0;
				YplusFib1000[0] = 0;
				YplusFib1272[0] = 0;
				YplusFib1618[0] = 0;
				lastHighDate[0] = (DateTime.MinValue);
			}	
			
			if (sessionNumber > 2 && runningLowIndex > -1 && Show_Y_Minus && Bars.BarsType.IsIntraday)
			{
				fib1000 = lastLowFib/TickSize;
				fib000 = lastCoupledHighFib/TickSize;
				YminusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningLow > lastLowFib)
				{
					YminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					YminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					YminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					YminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					YminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					YminusFib236[0] = 0;
					YminusFib382[0] = 0;
					YminusFib500[0] = 0;
					YminusFib618[0] = 0;
					YminusFib764[0] = 0;
				}
	
				YminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow > lastLowFib || runningHigh < lastCoupledHighFib || runningHighTime<runningLowTime)
				{
				YminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				YminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					YminusFib1272[0] = 0;
					YminusFib1618[0] = 0;
				}	
				lastLowDate[0] = (lastLowFibTime);
			}
			else
			{
				YminusFib000[0] = 0;
				YminusFib236[0] = 0;
				YminusFib382[0] = 0;
				YminusFib500[0] = 0;
				YminusFib618[0] = 0;
				YminusFib764[0] = 0;
				YminusFib1000[0] = 0;
				YminusFib1272[0] = 0;
				YminusFib1618[0] = 0;
				lastLowDate[0] = (DateTime.MinValue);
			}

			if(highFibIndex > -1 && Show_S_Plus && Bars.BarsType.IsIntraday)
			{
				fib1000 = highFib[0]/TickSize;
				fib000 = Math.Min(coupledLowFib[0],runningLow)/TickSize;
				SplusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningHigh < highFib[0])
				{	
					SplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					SplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					SplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					SplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					SplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					SplusFib236[0] = 0;
					SplusFib382[0] = 0;
					SplusFib500[0] = 0;
					SplusFib618[0] = 0;
					SplusFib764[0] = 0;
				}
				SplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh < highFib[0] || runningLow > coupledLowFib[0] || runningLowTime<runningHighTime)
				{
					SplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
					SplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					SplusFib1272[0] = 0;
					SplusFib1618[0] = 0;
				}						
				highDate[0][0] = (highFibTime[0]);
			}
			else
			{
				SplusFib000[0] = 0;
				SplusFib236[0] = 0;
				SplusFib382[0] = 0;
				SplusFib500[0] = 0;
				SplusFib618[0] = 0;
				SplusFib764[0] = 0;
				SplusFib1000[0] = 0;
				SplusFib1272[0] = 0;
				SplusFib1618[0] = 0;
				highDate[0][0] = (DateTime.MinValue);
			}	

			if(lowFibIndex > -1 && Show_S_Minus && Bars.BarsType.IsIntraday)
			{
				fib1000 = lowFib[0]/TickSize;
				fib000 = Math.Max(coupledHighFib[0],runningHigh)/TickSize;
				
				SminusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningLow > lowFib[0])
				{	
					SminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					SminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					SminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					SminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					SminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					SminusFib236[0] = 0;
					SminusFib382[0] = 0;
					SminusFib500[0] = 0;
					SminusFib618[0] = 0;
					SminusFib764[0] = 0;
				}
				SminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow > lowFib[0] || runningHigh < coupledHighFib[0] || runningHighTime<runningLowTime)
				{
				SminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				SminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					SminusFib1272[0] = 0;
					SminusFib1618[0] = 0;
				}						
				lowDate[0][0] = (lowFibTime[0]);
			}
			else
			{
				SminusFib000[0] = 0;
				SminusFib236[0] = 0;
				SminusFib382[0] = 0;
				SminusFib500[0] = 0;
				SminusFib618[0] = 0;
				SminusFib764[0] = 0;
				SminusFib1000[0] = 0;
				SminusFib1272[0] = 0;
				SminusFib1618[0] = 0;
				lowDate[0][0] = (DateTime.MinValue);
			}
			
			if(highFibIndex > 0 && Show_E_Plus)
			{
				fib1000 = highFib[1]/TickSize;
				fib000 = Math.Min(coupledLowFib[1],runningLow)/TickSize;
				EplusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningHigh < highFib[1])
				{	
					EplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					EplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					EplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					EplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					EplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					EplusFib236[0] = 0;
					EplusFib382[0] = 0;
					EplusFib500[0] = 0;
					EplusFib618[0] = 0;
					EplusFib764[0] = 0;
				}
				EplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh < highFib[1] || runningLow > coupledLowFib[1] || runningLowTime<runningHighTime)
				{
					EplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
					EplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					EplusFib1272[0] = 0;
					EplusFib1618[0] = 0;
				}						
				highDate[1][0] = (highFibTime[1]);
			}				
			else
			{
				EplusFib000[0] = 0;
				EplusFib236[0] = 0;
				EplusFib382[0] = 0;
				EplusFib500[0] = 0;
				EplusFib618[0] = 0;
				EplusFib764[0] = 0;
				EplusFib1000[0] = 0;
				EplusFib1272[0] = 0;
				EplusFib1618[0] = 0;
				highDate[1][0] = (DateTime.MinValue);
			}	

			if(lowFibIndex > 0 && Show_E_Minus)
			{
				fib1000 = lowFib[1]/TickSize;
				fib000 = Math.Max(coupledHighFib[1],runningHigh)/TickSize;
				EminusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningLow > lowFib[1])
				{	
					EminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					EminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					EminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					EminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					EminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					EminusFib236[0] = 0;
					EminusFib382[0] = 0;
					EminusFib500[0] = 0;
					EminusFib618[0] = 0;
					EminusFib764[0] = 0;
				}
				EminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow > lowFib[1] || runningHigh < coupledHighFib[1] || runningHighTime<runningLowTime)
				{
				EminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				EminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					EminusFib1272[0] = 0;
					EminusFib1618[0] = 0;
				}						
				lowDate[1][0] = (lowFibTime[1]);
			}
			else
			{
				EminusFib000[0] = 0;
				EminusFib236[0] = 0;
				EminusFib382[0] = 0;
				EminusFib500[0] = 0;
				EminusFib618[0] = 0;
				EminusFib764[0] = 0;
				EminusFib1000[0] = 0;
				EminusFib1272[0] = 0;
				EminusFib1618[0] = 0;
				lowDate[1][0] = (DateTime.MinValue);
			}				
			
			if(highFibIndex > 1 && Show_D_Plus)
			{
				fib1000 = highFib[2]/TickSize;
				fib000 = Math.Min(coupledLowFib[2],runningLow)/TickSize;
				DplusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningHigh < highFib[2])
				{	
					DplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					DplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					DplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					DplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					DplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					DplusFib236[0] = 0;
					DplusFib382[0] = 0;
					DplusFib500[0] = 0;
					DplusFib618[0] = 0;
					DplusFib764[0] = 0;
				}
				DplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh < highFib[2] || runningLow > coupledLowFib[2] || runningLowTime<runningHighTime)
				{
					DplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
					DplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					DplusFib1272[0] = 0;
					DplusFib1618[0] = 0;
				}						
				highDate[2][0] = (highFibTime[2]);
			}
			else
			{
				DplusFib000[0] = 0;
				DplusFib236[0] = 0;
				DplusFib382[0] = 0;
				DplusFib500[0] = 0;
				DplusFib618[0] = 0;
				DplusFib764[0] = 0;
				DplusFib1000[0] = 0;
				DplusFib1272[0] = 0;
				DplusFib1618[0] = 0;
				highDate[2][0] = (DateTime.MinValue);
			}	
			
			if(lowFibIndex > 1 && Show_D_Minus)
			{
				fib1000 = lowFib[2]/TickSize;
				fib000 = Math.Max(coupledHighFib[2],runningHigh)/TickSize;
				DminusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningLow > lowFib[2])
				{	
					DminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					DminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					DminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					DminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					DminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					DminusFib236[0] = 0;
					DminusFib382[0] = 0;
					DminusFib500[0] = 0;
					DminusFib618[0] = 0;
					DminusFib764[0] = 0;
				}
				DminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow > lowFib[2] || runningHigh < coupledHighFib[2] || runningHighTime<runningLowTime)
				{
				DminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				DminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					DminusFib1272[0] = 0;
					DminusFib1618[0] = 0;
				}						
				lowDate[2][0] = (lowFibTime[2]);
			}
			else
			{
				DminusFib000[0] = 0;
				DminusFib236[0] = 0;
				DminusFib382[0] = 0;
				DminusFib500[0] = 0;
				DminusFib618[0] = 0;
				DminusFib764[0] = 0;
				DminusFib1000[0] = 0;
				DminusFib1272[0] = 0;
				DminusFib1618[0] = 0;
				lowDate[2][0] = (DateTime.MinValue);
			}	
			
			if(highFibIndex > 2 && Show_C_Plus)
			{
				fib1000 = highFib[3]/TickSize;
				fib000 = Math.Min(coupledLowFib[3],runningLow)/TickSize;
				CplusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningHigh < highFib[3])
				{	
					CplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					CplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					CplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					CplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					CplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					CplusFib236[0] = 0;
					CplusFib382[0] = 0;
					CplusFib500[0] = 0;
					CplusFib618[0] = 0;
					CplusFib764[0] = 0;
				}
				CplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh < highFib[3] || runningLow > coupledLowFib[3] || runningLowTime<runningHighTime)
				{
					CplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
					CplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					CplusFib1272[0] = 0;
					CplusFib1618[0] = 0;
				}						
				highDate[3][0] = (highFibTime[3]);
			}
			else
			{
				CplusFib000[0] = 0;
				CplusFib236[0] = 0;
				CplusFib382[0] = 0;
				CplusFib500[0] = 0;
				CplusFib618[0] = 0;
				CplusFib764[0] = 0;
				CplusFib1000[0] = 0;
				CplusFib1272[0] = 0;
				CplusFib1618[0] = 0;
				highDate[3][0] = (DateTime.MinValue);
			}	

			if(lowFibIndex > 2 && Show_C_Minus)
			{
				fib1000 = lowFib[3]/TickSize;
				fib000 = Math.Max(coupledHighFib[3],runningHigh)/TickSize;
				CminusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningLow > lowFib[3])
				{	
					CminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					CminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					CminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					CminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					CminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					CminusFib236[0] = 0;
					CminusFib382[0] = 0;
					CminusFib500[0] = 0;
					CminusFib618[0] = 0;
					CminusFib764[0] = 0;
				}
				CminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow > lowFib[3] || runningHigh < coupledHighFib[3] || runningHighTime<runningLowTime)
				{
				CminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				CminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					CminusFib1272[0] = 0;
					CminusFib1618[0] = 0;
				}						
				lowDate[3][0] = (lowFibTime[3]);
			}
			else
			{
				CminusFib000[0] = 0;
				CminusFib236[0] = 0;
				CminusFib382[0] = 0;
				CminusFib500[0] = 0;
				CminusFib618[0] = 0;
				CminusFib764[0] = 0;
				CminusFib1000[0] = 0;
				CminusFib1272[0] = 0;
				CminusFib1618[0] = 0;
				lowDate[3][0] = (DateTime.MinValue);
			}	
		
			if(highFibIndex > 3 && Show_B_Plus)
			{
				fib1000 = highFib[4]/TickSize;
				fib000 = Math.Min(coupledLowFib[4],runningLow)/TickSize;
				BplusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningHigh < highFib[4])
				{	
					BplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					BplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					BplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					BplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					BplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					BplusFib236[0] = 0;
					BplusFib382[0] = 0;
					BplusFib500[0] = 0;
					BplusFib618[0] = 0;
					BplusFib764[0] = 0;
				}
				BplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh < highFib[4] || runningLow > coupledLowFib[4] || runningLowTime<runningHighTime)
				{
					BplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
					BplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					BplusFib1272[0] = 0;
					BplusFib1618[0] = 0;
				}						
				highDate[4][0] = (highFibTime[4]);
			}			
			else
			{
				BplusFib000[0] = 0;
				BplusFib236[0] = 0;
				BplusFib382[0] = 0;
				BplusFib500[0] = 0;
				BplusFib618[0] = 0;
				BplusFib764[0] = 0;
				BplusFib1000[0] = 0;
				BplusFib1272[0] = 0;
				BplusFib1618[0] = 0; // Why do you error???
				highDate[4][0] = (DateTime.MinValue);
			}	
	
			if(lowFibIndex > 3 && Show_B_Minus)
			{
				fib1000 = lowFib[4]/TickSize;
				fib000 = Math.Max(coupledHighFib[4],runningHigh)/TickSize;
				BminusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningLow > lowFib[4])
				{	
					BminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					BminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					BminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					BminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					BminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					BminusFib236[0] = 0;
					BminusFib382[0] = 0;
					BminusFib500[0] = 0;
					BminusFib618[0] = 0;
					BminusFib764[0] = 0;
				}
				BminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow > lowFib[4] || runningHigh < coupledHighFib[4] || runningHighTime<runningLowTime)
				{
				BminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				BminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					BminusFib1272[0] = 0;
					BminusFib1618[0] = 0;
				}						
				lowDate[4][0] = (lowFibTime[4]);
			}			
			else
			{
				BminusFib000[0] = 0;
				BminusFib236[0] = 0;
				BminusFib382[0] = 0;
				BminusFib500[0] = 0;
				BminusFib618[0] = 0;
				BminusFib764[0] = 0;
				BminusFib1000[0] = 0;
				BminusFib1272[0] = 0;
				BminusFib1618[0] = 0;
				lowDate[4][0] = (DateTime.MinValue);
			}	

			if(highFibIndex > 4 && Show_A_Plus)
			{
				fib1000 = highFib[5]/TickSize;
				fib000 = Math.Min(coupledLowFib[5],runningLow)/TickSize;
				AplusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningHigh < highFib[5])
				{	
					AplusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					AplusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					AplusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					AplusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					AplusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					AplusFib236[0] = 0;
					AplusFib382[0] = 0;
					AplusFib500[0] = 0;
					AplusFib618[0] = 0;
					AplusFib764[0] = 0;
				}
				AplusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningHigh < highFib[5] || runningLow > coupledLowFib[5] || runningLowTime<runningHighTime)
				{
					AplusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
					AplusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					AplusFib1272[0] = 0;
					AplusFib1618[0] = 0;
				}						
				highDate[5][0] = (highFibTime[5]);
			}						
			else
			{
				AplusFib000[0] = 0;
				AplusFib236[0] = 0;
				AplusFib382[0] = 0;
				AplusFib500[0] = 0;
				AplusFib618[0] = 0;
				AplusFib764[0] = 0;
				AplusFib1000[0] = 0;
				AplusFib1272[0] = 0;
				AplusFib1618[0] = 0;
				highDate[5][0] = (DateTime.MinValue);
			}	
		
			if(lowFibIndex > 4 && Show_A_Minus)
			{
				fib1000 = lowFib[5]/TickSize;
				fib000 = Math.Max(coupledHighFib[5],runningHigh)/TickSize;
				AminusFib000[0] = (TickSize*Math.Round(fib000));
				if (runningLow > lowFib[5])
				{	
					AminusFib236[0] = (TickSize*Math.Round(fib000+0.236*(fib1000-fib000)));
					AminusFib382[0] = (TickSize*Math.Round(fib000+0.382*(fib1000-fib000)));
					AminusFib500[0] = (TickSize*Math.Round(fib000+0.500*(fib1000-fib000)));
					AminusFib618[0] = (TickSize*Math.Round(fib000+0.618*(fib1000-fib000)));
					AminusFib764[0] = (TickSize*Math.Round(fib000+0.764*(fib1000-fib000)));
				}
				else
				{	
					AminusFib236[0] = 0;
					AminusFib382[0] = 0;
					AminusFib500[0] = 0;
					AminusFib618[0] = 0;
					AminusFib764[0] = 0;
				}
				AminusFib1000[0] = (TickSize*Math.Round(fib1000));
				if (runningLow > lowFib[5] || runningHigh < coupledHighFib[5] || runningHighTime<runningLowTime)
				{
				AminusFib1272[0] = (TickSize*Math.Round(fib000+1.272*(fib1000-fib000)));
				AminusFib1618[0] = (TickSize*Math.Round(fib000+1.618*(fib1000-fib000)));
				}
				else
				{	
					AminusFib1272[0] = 0;
					AminusFib1618[0] = 0;
				}						
				lowDate[5][0] = (lowFibTime[5]);
			}
			else
			{
				AminusFib000[0] = 0;
				AminusFib236[0] = 0;
				AminusFib382[0] = 0;
				AminusFib500[0] = 0;
				AminusFib618[0] = 0;
				AminusFib764[0] = 0;
				AminusFib1000[0] = 0;
				AminusFib1272[0] = 0;
				AminusFib1618[0] = 0;
				lowDate[5][0] = (DateTime.MinValue);
			}	
		
			if (coveredHighTime[0] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenHigh0[0] = (TickSize*Math.Round(coveredHigh[0]/TickSize));
				hiddenHighDate[0][0] = (coveredHighTime[0]);
			}
			else
			{
				HiddenHigh0[0] = 0;
				hiddenHighDate[0][0] = (DateTime.MinValue);
			}

			if (coveredHighTime[1] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenHigh1[0] = (TickSize*Math.Round(coveredHigh[1]/TickSize));
				hiddenHighDate[1][0] = (coveredHighTime[1]);
			}
			else
			{
				HiddenHigh1[0] = 0;
				hiddenHighDate[1][0] = (DateTime.MinValue);
			}

			if (coveredHighTime[2] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenHigh2[0] = (TickSize*Math.Round(coveredHigh[2]/TickSize));
				hiddenHighDate[2][0] = (coveredHighTime[2]);
			}
			else
			{
				HiddenHigh2[0] = 0;
				hiddenHighDate[2][0] = (DateTime.MinValue);
			}
							
			if (coveredHighTime[3] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenHigh3[0] = (TickSize*Math.Round(coveredHigh[3]/TickSize));
				hiddenHighDate[3][0] = (coveredHighTime[3]);
			}
			else
			{
				HiddenHigh3[0] = 0;
				hiddenHighDate[3][0] = (DateTime.MinValue);
			}
							
			if (coveredHighTime[4] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenHigh4[0] = (TickSize*Math.Round(coveredHigh[4]/TickSize));
				hiddenHighDate[4][0] = (coveredHighTime[4]);
			}
			else
			{
				HiddenHigh4[0] = 0;
				hiddenHighDate[4][0] = (DateTime.MinValue);
			}
							
			if (coveredHighTime[5] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenHigh5[0] = (TickSize*Math.Round(coveredHigh[5]/TickSize));
				hiddenHighDate[5][0] = (coveredHighTime[5]);
			}
			else
			{
				HiddenHigh5[0] = 0;
				hiddenHighDate[5][0] = (DateTime.MinValue);
			}
							
			if (coveredLowTime[0] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenLow0[0] = (TickSize*Math.Round(coveredLow[0]/TickSize));
				hiddenLowDate[0][0] = (coveredLowTime[0]);
			}
			else
			{
				HiddenLow0[0] = 0;
				hiddenLowDate[0][0] = (DateTime.MinValue);
			}
			
			if (coveredLowTime[1] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenLow1[0] = (TickSize*Math.Round(coveredLow[1]/TickSize));
				hiddenLowDate[1][0] = (coveredLowTime[1]);
			}
			else
			{
				HiddenLow1[0] = 0;
				hiddenLowDate[1][0] = (DateTime.MinValue);
			}
							
			if (coveredLowTime[2] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenLow2[0] = (TickSize*Math.Round(coveredLow[2]/TickSize));
				hiddenLowDate[2][0] = (coveredLowTime[2]);
			}
			else
			{
				HiddenLow2[0] = 0;
				hiddenLowDate[2][0] = (DateTime.MinValue);
			}
							
			if (coveredLowTime[3] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenLow3[0] = (TickSize*Math.Round(coveredLow[3]/TickSize));
				hiddenLowDate[3][0] = (coveredLowTime[3]);
			}
			else
			{
				HiddenLow3[0] = 0;
				hiddenLowDate[3][0] = (DateTime.MinValue);
			}
							
			if (coveredLowTime[4] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenLow4[0] = (TickSize*Math.Round(coveredLow[4]/TickSize));
				hiddenLowDate[4][0] = (coveredLowTime[4]);
			}
			else
			{
				HiddenLow4[0] = 0;
				hiddenLowDate[4][0] = (DateTime.MinValue);
			}
							
			if (coveredLowTime[5] > DateTime.MinValue && Show_Highs_Lows == true)
			{
				HiddenLow5[0] = (TickSize*Math.Round(coveredLow[5]/TickSize));
				hiddenLowDate[5][0] = (coveredLowTime[5]);
			}
			else
			{
				HiddenLow5[0] = 0;
				hiddenLowDate[5][0] = (DateTime.MinValue);
			}
			
			if(State == State.Realtime || (State == State.Historical && CurrentBar == Bars.Count - 2))
				UpdateLabels();
		}
		
		private void UpdateLabels()
		{
			//Counting for multiple Fib and S/R levels	
			for (int seriesCount=0; seriesCount<Values.Length; seriesCount++)
				fibCounter[seriesCount] = 1;
			for (int seriesCount=0; seriesCount<Values.Length ; seriesCount++)
			{
				for(int j=seriesCount+1; j<Values.Length; j++)
				{
					if (Values[seriesCount].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex )) == Values[j].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex )))
					{
						fibCounter.SetValue((int)fibCounter.GetValue(seriesCount) + 1, seriesCount);
						fibCounter.SetValue((int)fibCounter.GetValue(j) + 1, j);
					}
				}
			}
			
			// Preparing Plot Labels		
			string highDateLabel = "no date";
			string lowDateLabel = "no date";

			for (int i=0; i<6; i++)
			{	
				plotlabel.SetValue(highExtDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), 3*i);
				plotlabel.SetValue(highExtDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), 3*i+1);
				plotlabel.SetValue(highExtDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), 3*i+2);
				plotlabel.SetValue(lowExtDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), 3*i+18);
				plotlabel.SetValue(lowExtDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), 3*i+19);
				plotlabel.SetValue(lowExtDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), 3*i+20);
			}
			for (int i=0;i<9;i++)
			{
				plotlabel.SetValue(recentHighDate.GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), i+36);
				plotlabel.SetValue(recentLowDate.GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), i+45);
			}			
			for (int i=0;i<9;i++)
			{	
				plotlabel.SetValue(lastHighDate.GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), i+54);
				plotlabel.SetValue(lastLowDate.GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), i+63);
			}
			for (int i=0; i<6; i++)
			{
				highDateLabel = highDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM ");
				lowDateLabel = lowDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM ");
				for (int j=0; j<9; j++) 
				{
					plotlabel.SetValue(highDateLabel, 72+18*i+j);
					plotlabel.SetValue(lowDateLabel, 81+18*i+j);
				}
			}
			for (int i=0; i<6; i++)
			{	
				plotlabel.SetValue(hiddenHighDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), i+180);
				plotlabel.SetValue(hiddenLowDate[i].GetValueAt(Math.Min(Bars.Count - 2, ChartBars.ToIndex)).ToString("dd MMM "), i+186);
			}
			
			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
			{
				if ((int)fibCounter.GetValue(seriesCount)==1)
					plotlabelfull[seriesCount] = Plots[seriesCount].Name + "  " + plotlabel.GetValue(seriesCount);
				else
					plotlabelfull[seriesCount] = "Multiple S/R (" + Convert.ToString(fibCounter.GetValue(seriesCount)) + ")" ;
				
				if (textLayouts[seriesCount] != null)
				{
					if (plotlabelfull[seriesCount] != plotlabelused[seriesCount])
					{
						textLayouts[seriesCount].Dispose();
						textLayouts[seriesCount] = null;
					}
				}	
			}
		}
		
		public override void OnRenderTargetChanged()
		{				
			if (textLayouts == null)
				textLayouts = new SharpDX.DirectWrite.TextLayout[192];
			
			if (textFormats == null)
				textFormats = new SharpDX.DirectWrite.TextFormat[192];
				
			for (int seriesCount=0; seriesCount<Values.Length; seriesCount++)
			{
				if (textLayouts[seriesCount] != null)
				{
					textLayouts[seriesCount].Dispose();
					textLayouts[seriesCount] = null;
				}
				
				if (textFormats[seriesCount] != null)
				{
					textFormats[seriesCount].Dispose();
					textFormats[seriesCount] = null;
				}
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (Bars == null || ChartControl == null)
				return;
			
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode 	= RenderTarget.AntialiasMode;
			
			// Drawing Lines and Labels
			int	barWidth = chartControl.GetBarPaintWidth(chartControl.BarsArray[0]);
			
			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
			{
				int								lastX				= -1;
				int								lastY				= -1;
				int 							firstX				= -1;
				Plot							plot				= Plots[seriesCount];
				Series<double>					series				= (Series<double>) Values[seriesCount];
				
				for (int idx = ChartBars.ToIndex; idx >= Math.Max(ChartBars.FromIndex, ChartBars.ToIndex - Width); idx--)
				{
					if (idx - Displacement < 0 || idx - Displacement >= Bars.Count || (idx - Displacement < BarsRequired))
						continue;
					else if (!series.IsValidDataPointAt(idx))
						continue;
					double val = series.GetValueAt(idx);
					
					int		x	= ChartControl.GetXByBarIndex(ChartBars, idx);	
					int		y	= chartScale.GetYByValue(val);

					if (lastX >= 0)
					{
						if (y != lastY)
							y = lastY;
						SharpDX.Vector2 startPoint1 = new System.Windows.Point(lastX - plot.Width / 2, lastY).ToVector2();
						SharpDX.Vector2 endPoint1 = new System.Windows.Point(x - plot.Width / 2,  y).ToVector2();
						
						RenderTarget.DrawLine(startPoint1, endPoint1, Plots[seriesCount].BrushDX, 1);

					}
					lastX	= x;
					lastY	= y;
					
					if (idx == ChartBars.ToIndex || idx == ChartBars.ToIndex-1)
						firstX	= x;
				}			
				
				if (textFormats[seriesCount] == null)
				{
					NinjaTrader.Gui.Tools.SimpleFont simpleFont = chartControl.Properties.LabelFont ??  new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12);
					textFormats[seriesCount] = simpleFont.ToDirectWriteTextFormat();
				}
				
				if (textLayouts[seriesCount] == null)
				{
					textLayouts[seriesCount] = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
								plotlabelfull[seriesCount], textFormats[seriesCount], ChartPanel.X + ChartPanel.W,
								textFormats[seriesCount].FontSize);
					plotlabelused[seriesCount] = plotlabelfull[seriesCount];
				}
				
				SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(firstX + LabelPosition + 20,  lastY - textFormats[seriesCount].FontSize / 2).ToVector2();
				
				RenderTarget.DrawTextLayout(TextPlotPoint, textLayouts[seriesCount], Plots[seriesCount].BrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			}
			
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}
		
		
#region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="LabelPosition", Description="Distance of label from line.", Order=1, GroupName="NinjaScriptStrategyParameters")]
		public int LabelPosition
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(ResourceType = typeof(Custom.Resource), Name="Offset", Description="For RTH pivots enter RTH session length. RTH session end is taken from session template. Only with CalcFromIntradayData.", Order=2, GroupName="NinjaScriptStrategyParameters")]
		public TimeSpan Offset
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_S_Plus", Description="Option to show S+ Fibs", Order=3, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_S_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_S_Minus", Description="Option to show S- Fibs", Order=4, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_S_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_E_Plus", Description="Option to show E+ Fibs", Order=5, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_E_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_E_Minus", Description="Option to show E- Fibs", Order=6, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_E_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_D_Plus", Description="Option to show D+ Fibs", Order=7, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_D_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_D_Minus", Description="Option to show D- Fibs", Order=8, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_D_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_C_Plus", Description="Option to show C+ Fibs", Order=9, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_C_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_C_Minus", Description="Option to show C- Fibs", Order=10, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_C_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_B_Plus", Description="Option to show B+ Fibs", Order=11, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_B_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_B_Minus", Description="Option to show B- Fibs", Order=12, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_B_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_A_Plus", Description="Option to show A+ Fibs", Order=13, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_A_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_A_Minus", Description="Option to show A- Fibs", Order=14, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_A_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_Xtensions", Description="Option to show Fib Extensions of Covered Highs and Lows", Order=15, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_Xtensions
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_Highs_Lows", Description="Option to show Swing Highs and Lows", Order=16, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_Highs_Lows
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_Y_Plus", Description="Option to show last Fib restracements", Order=17, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_Y_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_Y_Minus", Description="Option to show last Fib restracements", Order=18, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_Y_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_Z_Plus", Description="Option to show last Fib restracements", Order=19, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_Z_Plus
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name="Show_Z_Minus", Description="Option to show last Fib restracements", Order=20, GroupName="NinjaScriptStrategyParameters")]
		public bool Show_Z_Minus
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Width", Description="Width of the pivot lines as # of bars.", Order=21, GroupName="NinjaScriptStrategyParameters")]
		public int Width
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="LookBack", Description="Lookback Period of Indicator", Order=22, GroupName="NinjaScriptStrategyParameters")]
		public int LookBack
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Filter", Description="Filter, no filter = 100, standard value = 85", Order=23, GroupName="NinjaScriptStrategyParameters")]
		public int Filter
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension0
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension1
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension2
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension3
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension4
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension5
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension6
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension7
		{
			get { return Values[7]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension8
		{
			get { return Values[8]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension9
		{
			get { return Values[9]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension10
		{
			get { return Values[10]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension11
		{
			get { return Values[11]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension12
		{
			get { return Values[12]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension13
		{
			get { return Values[13]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension14
		{
			get { return Values[14]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension15
		{
			get { return Values[15]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension16
		{
			get { return Values[16]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension17
		{
			get { return Values[17]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension18
		{
			get { return Values[18]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension19
		{
			get { return Values[19]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension20
		{
			get { return Values[20]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension21
		{
			get { return Values[21]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension22
		{
			get { return Values[22]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension23
		{
			get { return Values[23]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension24
		{
			get { return Values[24]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension25
		{
			get { return Values[25]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension26
		{
			get { return Values[26]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension27
		{
			get { return Values[27]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension28
		{
			get { return Values[28]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension29
		{
			get { return Values[29]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension30
		{
			get { return Values[30]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension31
		{
			get { return Values[31]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension32
		{
			get { return Values[32]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension33
		{
			get { return Values[33]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension34
		{
			get { return Values[34]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Extension35
		{
			get { return Values[35]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib000
		{
			get { return Values[36]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib236
		{
			get { return Values[37]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib382
		{
			get { return Values[38]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib500
		{
			get { return Values[39]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib618
		{
			get { return Values[40]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib764
		{
			get { return Values[41]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib1000
		{
			get { return Values[42]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib1272
		{
			get { return Values[43]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZplusFib1618
		{
			get { return Values[44]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib000
		{
			get { return Values[45]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib236
		{
			get { return Values[46]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib382
		{
			get { return Values[47]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib500
		{
			get { return Values[48]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib618
		{
			get { return Values[49]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib764
		{
			get { return Values[50]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib1000
		{
			get { return Values[51]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib1272
		{
			get { return Values[52]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZminusFib1618
		{
			get { return Values[53]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib000
		{
			get { return Values[54]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib236
		{
			get { return Values[55]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib382
		{
			get { return Values[56]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib500
		{
			get { return Values[57]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib618
		{
			get { return Values[58]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib764
		{
			get { return Values[59]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib1000
		{
			get { return Values[60]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib1272
		{
			get { return Values[61]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YplusFib1618
		{
			get { return Values[62]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib000
		{
			get { return Values[63]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib236
		{
			get { return Values[64]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib382
		{
			get { return Values[65]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib500
		{
			get { return Values[66]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib618
		{
			get { return Values[67]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib764
		{
			get { return Values[68]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib1000
		{
			get { return Values[69]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib1272
		{
			get { return Values[70]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> YminusFib1618
		{
			get { return Values[71]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib000
		{
			get { return Values[72]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib236
		{
			get { return Values[73]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib382
		{
			get { return Values[74]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib500
		{
			get { return Values[75]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib618
		{
			get { return Values[76]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib764
		{
			get { return Values[77]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib1000
		{
			get { return Values[78]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib1272
		{
			get { return Values[79]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SplusFib1618
		{
			get { return Values[80]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib000
		{
			get { return Values[81]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib236
		{
			get { return Values[82]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib382
		{
			get { return Values[83]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib500
		{
			get { return Values[84]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib618
		{
			get { return Values[85]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib764
		{
			get { return Values[86]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib1000
		{
			get { return Values[87]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib1272
		{
			get { return Values[88]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SminusFib1618
		{
			get { return Values[89]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib000
		{
			get { return Values[90]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib236
		{
			get { return Values[91]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib382
		{
			get { return Values[92]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib500
		{
			get { return Values[93]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib618
		{
			get { return Values[94]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib764
		{
			get { return Values[95]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib1000
		{
			get { return Values[96]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib1272
		{
			get { return Values[97]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EplusFib1618
		{
			get { return Values[98]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib000
		{
			get { return Values[99]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib236
		{
			get { return Values[100]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib382
		{
			get { return Values[101]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib500
		{
			get { return Values[102]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib618
		{
			get { return Values[103]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib764
		{
			get { return Values[104]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib1000
		{
			get { return Values[105]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib1272
		{
			get { return Values[106]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EminusFib1618
		{
			get { return Values[107]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib000
		{
			get { return Values[108]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib236
		{
			get { return Values[109]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib382
		{
			get { return Values[110]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib500
		{
			get { return Values[111]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib618
		{
			get { return Values[112]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib764
		{
			get { return Values[113]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib1000
		{
			get { return Values[114]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib1272
		{
			get { return Values[115]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DplusFib1618
		{
			get { return Values[116]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib000
		{
			get { return Values[117]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib236
		{
			get { return Values[118]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib382
		{
			get { return Values[119]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib500
		{
			get { return Values[120]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib618
		{
			get { return Values[121]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib764
		{
			get { return Values[122]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib1000
		{
			get { return Values[123]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib1272
		{
			get { return Values[124]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DminusFib1618
		{
			get { return Values[125]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib000
		{
			get { return Values[126]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib236
		{
			get { return Values[127]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib382
		{
			get { return Values[128]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib500
		{
			get { return Values[129]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib618
		{
			get { return Values[130]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib764
		{
			get { return Values[131]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib1000
		{
			get { return Values[132]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib1272
		{
			get { return Values[133]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CplusFib1618
		{
			get { return Values[134]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib000
		{
			get { return Values[135]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib236
		{
			get { return Values[136]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib382
		{
			get { return Values[137]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib500
		{
			get { return Values[138]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib618
		{
			get { return Values[139]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib764
		{
			get { return Values[140]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib1000
		{
			get { return Values[141]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib1272
		{
			get { return Values[142]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CminusFib1618
		{
			get { return Values[143]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib000
		{
			get { return Values[144]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib236
		{
			get { return Values[145]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib382
		{
			get { return Values[146]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib500
		{
			get { return Values[147]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib618
		{
			get { return Values[148]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib764
		{
			get { return Values[149]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib1000
		{
			get { return Values[150]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib1272
		{
			get { return Values[151]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BplusFib1618
		{
			get { return Values[152]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib000
		{
			get { return Values[153]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib236
		{
			get { return Values[154]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib382
		{
			get { return Values[155]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib500
		{
			get { return Values[156]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib618
		{
			get { return Values[157]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib764
		{
			get { return Values[158]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib1000
		{
			get { return Values[159]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib1272
		{
			get { return Values[160]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BminusFib1618
		{
			get { return Values[161]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib000
		{
			get { return Values[162]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib236
		{
			get { return Values[163]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib382
		{
			get { return Values[164]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib500
		{
			get { return Values[165]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib618
		{
			get { return Values[166]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib764
		{
			get { return Values[167]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib1000
		{
			get { return Values[168]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib1272
		{
			get { return Values[169]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AplusFib1618
		{
			get { return Values[170]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib000
		{
			get { return Values[171]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib236
		{
			get { return Values[172]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib382
		{
			get { return Values[173]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib500
		{
			get { return Values[174]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib618
		{
			get { return Values[175]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib764
		{
			get { return Values[176]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib1000
		{
			get { return Values[177]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib1272
		{
			get { return Values[178]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AminusFib1618
		{
			get { return Values[179]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenHigh0
		{
			get { return Values[180]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenHigh1
		{
			get { return Values[181]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenHigh2
		{
			get { return Values[182]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenHigh3
		{
			get { return Values[183]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenHigh4
		{
			get { return Values[184]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenHigh5
		{
			get { return Values[185]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenLow0
		{
			get { return Values[186]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenLow1
		{
			get { return Values[187]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenLow2
		{
			get { return Values[188]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenLow3
		{
			get { return Values[189]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenLow4
		{
			get { return Values[190]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HiddenLow5
		{
			get { return Values[191]; }
		}
		
		
		
		#endregion		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FibonacciClusterV16D[] cacheFibonacciClusterV16D;
		public FibonacciClusterV16D FibonacciClusterV16D(int labelPosition, TimeSpan offset, bool show_S_Plus, bool show_S_Minus, bool show_E_Plus, bool show_E_Minus, bool show_D_Plus, bool show_D_Minus, bool show_C_Plus, bool show_C_Minus, bool show_B_Plus, bool show_B_Minus, bool show_A_Plus, bool show_A_Minus, bool show_Xtensions, bool show_Highs_Lows, bool show_Y_Plus, bool show_Y_Minus, bool show_Z_Plus, bool show_Z_Minus, int width, int lookBack, int filter)
		{
			return FibonacciClusterV16D(Input, labelPosition, offset, show_S_Plus, show_S_Minus, show_E_Plus, show_E_Minus, show_D_Plus, show_D_Minus, show_C_Plus, show_C_Minus, show_B_Plus, show_B_Minus, show_A_Plus, show_A_Minus, show_Xtensions, show_Highs_Lows, show_Y_Plus, show_Y_Minus, show_Z_Plus, show_Z_Minus, width, lookBack, filter);
		}

		public FibonacciClusterV16D FibonacciClusterV16D(ISeries<double> input, int labelPosition, TimeSpan offset, bool show_S_Plus, bool show_S_Minus, bool show_E_Plus, bool show_E_Minus, bool show_D_Plus, bool show_D_Minus, bool show_C_Plus, bool show_C_Minus, bool show_B_Plus, bool show_B_Minus, bool show_A_Plus, bool show_A_Minus, bool show_Xtensions, bool show_Highs_Lows, bool show_Y_Plus, bool show_Y_Minus, bool show_Z_Plus, bool show_Z_Minus, int width, int lookBack, int filter)
		{
			if (cacheFibonacciClusterV16D != null)
				for (int idx = 0; idx < cacheFibonacciClusterV16D.Length; idx++)
					if (cacheFibonacciClusterV16D[idx] != null && cacheFibonacciClusterV16D[idx].LabelPosition == labelPosition && cacheFibonacciClusterV16D[idx].Offset == offset && cacheFibonacciClusterV16D[idx].Show_S_Plus == show_S_Plus && cacheFibonacciClusterV16D[idx].Show_S_Minus == show_S_Minus && cacheFibonacciClusterV16D[idx].Show_E_Plus == show_E_Plus && cacheFibonacciClusterV16D[idx].Show_E_Minus == show_E_Minus && cacheFibonacciClusterV16D[idx].Show_D_Plus == show_D_Plus && cacheFibonacciClusterV16D[idx].Show_D_Minus == show_D_Minus && cacheFibonacciClusterV16D[idx].Show_C_Plus == show_C_Plus && cacheFibonacciClusterV16D[idx].Show_C_Minus == show_C_Minus && cacheFibonacciClusterV16D[idx].Show_B_Plus == show_B_Plus && cacheFibonacciClusterV16D[idx].Show_B_Minus == show_B_Minus && cacheFibonacciClusterV16D[idx].Show_A_Plus == show_A_Plus && cacheFibonacciClusterV16D[idx].Show_A_Minus == show_A_Minus && cacheFibonacciClusterV16D[idx].Show_Xtensions == show_Xtensions && cacheFibonacciClusterV16D[idx].Show_Highs_Lows == show_Highs_Lows && cacheFibonacciClusterV16D[idx].Show_Y_Plus == show_Y_Plus && cacheFibonacciClusterV16D[idx].Show_Y_Minus == show_Y_Minus && cacheFibonacciClusterV16D[idx].Show_Z_Plus == show_Z_Plus && cacheFibonacciClusterV16D[idx].Show_Z_Minus == show_Z_Minus && cacheFibonacciClusterV16D[idx].Width == width && cacheFibonacciClusterV16D[idx].LookBack == lookBack && cacheFibonacciClusterV16D[idx].Filter == filter && cacheFibonacciClusterV16D[idx].EqualsInput(input))
						return cacheFibonacciClusterV16D[idx];
			return CacheIndicator<FibonacciClusterV16D>(new FibonacciClusterV16D(){ LabelPosition = labelPosition, Offset = offset, Show_S_Plus = show_S_Plus, Show_S_Minus = show_S_Minus, Show_E_Plus = show_E_Plus, Show_E_Minus = show_E_Minus, Show_D_Plus = show_D_Plus, Show_D_Minus = show_D_Minus, Show_C_Plus = show_C_Plus, Show_C_Minus = show_C_Minus, Show_B_Plus = show_B_Plus, Show_B_Minus = show_B_Minus, Show_A_Plus = show_A_Plus, Show_A_Minus = show_A_Minus, Show_Xtensions = show_Xtensions, Show_Highs_Lows = show_Highs_Lows, Show_Y_Plus = show_Y_Plus, Show_Y_Minus = show_Y_Minus, Show_Z_Plus = show_Z_Plus, Show_Z_Minus = show_Z_Minus, Width = width, LookBack = lookBack, Filter = filter }, input, ref cacheFibonacciClusterV16D);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FibonacciClusterV16D FibonacciClusterV16D(int labelPosition, TimeSpan offset, bool show_S_Plus, bool show_S_Minus, bool show_E_Plus, bool show_E_Minus, bool show_D_Plus, bool show_D_Minus, bool show_C_Plus, bool show_C_Minus, bool show_B_Plus, bool show_B_Minus, bool show_A_Plus, bool show_A_Minus, bool show_Xtensions, bool show_Highs_Lows, bool show_Y_Plus, bool show_Y_Minus, bool show_Z_Plus, bool show_Z_Minus, int width, int lookBack, int filter)
		{
			return indicator.FibonacciClusterV16D(Input, labelPosition, offset, show_S_Plus, show_S_Minus, show_E_Plus, show_E_Minus, show_D_Plus, show_D_Minus, show_C_Plus, show_C_Minus, show_B_Plus, show_B_Minus, show_A_Plus, show_A_Minus, show_Xtensions, show_Highs_Lows, show_Y_Plus, show_Y_Minus, show_Z_Plus, show_Z_Minus, width, lookBack, filter);
		}

		public Indicators.FibonacciClusterV16D FibonacciClusterV16D(ISeries<double> input , int labelPosition, TimeSpan offset, bool show_S_Plus, bool show_S_Minus, bool show_E_Plus, bool show_E_Minus, bool show_D_Plus, bool show_D_Minus, bool show_C_Plus, bool show_C_Minus, bool show_B_Plus, bool show_B_Minus, bool show_A_Plus, bool show_A_Minus, bool show_Xtensions, bool show_Highs_Lows, bool show_Y_Plus, bool show_Y_Minus, bool show_Z_Plus, bool show_Z_Minus, int width, int lookBack, int filter)
		{
			return indicator.FibonacciClusterV16D(input, labelPosition, offset, show_S_Plus, show_S_Minus, show_E_Plus, show_E_Minus, show_D_Plus, show_D_Minus, show_C_Plus, show_C_Minus, show_B_Plus, show_B_Minus, show_A_Plus, show_A_Minus, show_Xtensions, show_Highs_Lows, show_Y_Plus, show_Y_Minus, show_Z_Plus, show_Z_Minus, width, lookBack, filter);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FibonacciClusterV16D FibonacciClusterV16D(int labelPosition, TimeSpan offset, bool show_S_Plus, bool show_S_Minus, bool show_E_Plus, bool show_E_Minus, bool show_D_Plus, bool show_D_Minus, bool show_C_Plus, bool show_C_Minus, bool show_B_Plus, bool show_B_Minus, bool show_A_Plus, bool show_A_Minus, bool show_Xtensions, bool show_Highs_Lows, bool show_Y_Plus, bool show_Y_Minus, bool show_Z_Plus, bool show_Z_Minus, int width, int lookBack, int filter)
		{
			return indicator.FibonacciClusterV16D(Input, labelPosition, offset, show_S_Plus, show_S_Minus, show_E_Plus, show_E_Minus, show_D_Plus, show_D_Minus, show_C_Plus, show_C_Minus, show_B_Plus, show_B_Minus, show_A_Plus, show_A_Minus, show_Xtensions, show_Highs_Lows, show_Y_Plus, show_Y_Minus, show_Z_Plus, show_Z_Minus, width, lookBack, filter);
		}

		public Indicators.FibonacciClusterV16D FibonacciClusterV16D(ISeries<double> input , int labelPosition, TimeSpan offset, bool show_S_Plus, bool show_S_Minus, bool show_E_Plus, bool show_E_Minus, bool show_D_Plus, bool show_D_Minus, bool show_C_Plus, bool show_C_Minus, bool show_B_Plus, bool show_B_Minus, bool show_A_Plus, bool show_A_Minus, bool show_Xtensions, bool show_Highs_Lows, bool show_Y_Plus, bool show_Y_Minus, bool show_Z_Plus, bool show_Z_Minus, int width, int lookBack, int filter)
		{
			return indicator.FibonacciClusterV16D(input, labelPosition, offset, show_S_Plus, show_S_Minus, show_E_Plus, show_E_Minus, show_D_Plus, show_D_Minus, show_C_Plus, show_C_Minus, show_B_Plus, show_B_Minus, show_A_Plus, show_A_Minus, show_Xtensions, show_Highs_Lows, show_Y_Plus, show_Y_Minus, show_Z_Plus, show_Z_Minus, width, lookBack, filter);
		}
	}
}

#endregion
