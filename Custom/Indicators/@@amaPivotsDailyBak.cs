 //+----------------------------------------------------------------------------------------------+
//| Copyright © <2018>  <LizardIndicators.com - powered by AlderLab UG>
//
//| This program is free software: you can redistribute it and/or modify
//| it under the terms of the GNU General Public License as published by
//| the Free Software Foundation, either version 3 of the License, or
//| any later version.
//|
//| This program is distributed in the hope that it will be useful,
//| but WITHOUT ANY WARRANTY; without even the implied warranty of
//| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//| GNU General Public License for more details.
//|
//| By installing this software you confirm acceptance of the GNU
//| General Public License terms. You may find a copy of the license
//| here; http://www.gnu.org/licenses/
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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators.LizardIndicators;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
#endregion


// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Daily Pivots indicator can be used to display floor pivots, Globex pivots and the high, low and close of the prior day.
	/// </summary>
	/// 
	[Gui.CategoryOrder("Algorithmic Options", 0)]
	[Gui.CategoryOrder("Display Options", 20)]
	[Gui.CategoryOrder("Data Series", 30)]
	[Gui.CategoryOrder("Set up", 40)]
	[Gui.CategoryOrder("Visual", 50)]
	[Gui.CategoryOrder("Plot Colors", 60)]
	[Gui.CategoryOrder("Plot Parameters", 70)]
	[Gui.CategoryOrder("Shaded Areas", 80)]
	[Gui.CategoryOrder("Version", 90)]
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.amaPivotsDailyTypeConverter")]
	public class amaPivotsDailyBak : Indicator
	{
		private DateTime							sessionDateTmp0					= Globals.MinDate;
		private DateTime							cacheSessionDate0				= Globals.MinDate;
		private DateTime							sessionDateTmp1					= Globals.MinDate;
		private DateTime							dailyBarDate					= Globals.MinDate;
		private DateTime							priorDailyBarDate				= Globals.MinDate;
		private double								currentDailyHigh				= 0.0;
		private double								currentDailyLow					= 0.0;
		private double								currentDailyClose				= 0.0;
		private double								priorDailyHigh					= 0.0;
		private double								priorDailyLow					= 0.0;
		private double								priorDailyClose					= 0.0;
		private double								priorHigh						= 0.0;
		private double								priorLow						= 0.0;
		private double								priorClose						= 0.0;
		private double								priorRange						= 0.0;
		private	double								pp								= 0.0;
		private	double								upside							= 0.0;
		private	double								downside						= 0.0;
		private	double								r1								= 0.0;
		private	double								s1								= 0.0;
		private	double								r2								= 0.0;
		private	double								s2								= 0.0;
		private	double								r3								= 0.0;
		private	double								s3								= 0.0;
		private	double								r4								= 0.0;
		private	double								s4								= 0.0;
		private	double								rmid							= 0.0;
		private	double								smid							= 0.0;
		private	double								r12mid							= 0.0;
		private	double								s12mid							= 0.0;
		private	double								r23mid							= 0.0;
		private	double								s23mid							= 0.0;
		private	double								r34mid							= 0.0;
		private	double								s34mid							= 0.0;
		private	double								cp								= 0.0;
		private	double								dp								= 0.0;
		private double								displaySize						= 0.0;
		private int									displacement					= 0;
		private int									count							= 0;
		private int									cacheLastBarPainted				= 0;
		private bool								showPP							= true;
		private bool								showCP							= true;
		private bool								showDP							= true;
		private bool								showPivots						= true;
		private bool								showPriorHLC					= true;
		private bool								showMidpivots					= false;
		private bool								showPivotRange					= true;
		private bool								showJacksonZones				= true;
		private bool								showLabels						= true;
		private bool								isIntraday0						= true;
		private bool								calcOpen						= false;
		private bool								continuationTick				= false;
		private bool								timeBased0						= false;
		private bool								breakAtEOD						= true;
		private bool								errorMessage					= true;
		private bool								basicError						= false;
		private bool								sundaySessionError				= false;
		private bool								dailyBarsError					= false;
		private bool								initBarSeries0					= false;
		private bool								calculateFromPriceData			= true;
		private bool								calculateFromIntradayData		= false;
		private bool								lastBarStartsNewPeriod			= false;
		private amaPivotFormulaPD					pivotFormula					= amaPivotFormulaPD.Floor_Trader_Pivots;
		private amaSessionTypePD					sessionType						= amaSessionTypePD.Daily_Bars;
		private amaCalcModePD						calcMode						= amaCalcModePD.Daily_Data;
		private readonly List<int>					newSessionBarIdxArr				= new List<int>();
		private SessionIterator						sessionIterator0				= null;
		private System.Windows.Media.Brush			pivotBrush						= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			centralPivotBrush				= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			directionalPivotBrush			= Brushes.MidnightBlue;
		private System.Windows.Media.Brush  		resistanceBrush					= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			supportBrush					= Brushes.MidnightBlue;
		private System.Windows.Media.Brush  		resistanceMidBrush				= Brushes.MidnightBlue;
		private System.Windows.Media.Brush  		supportMidBrush					= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			priorHighBrush					= Brushes.DarkGreen;
		private System.Windows.Media.Brush			priorLowBrush 					= Brushes.Firebrick;
		private System.Windows.Media.Brush			priorCloseBrush 				= Brushes.DarkOrange;
		private System.Windows.Media.Brush			pivotRangeBrushS				= Brushes.RoyalBlue;
		private System.Windows.Media.Brush			centralRangeBrushS				= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush			jacksonZonesBrushS				= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush			pivotRangeBrush					= null;
		private System.Windows.Media.Brush			centralRangeBrush				= null;
		private System.Windows.Media.Brush			jacksonZonesBrush				= null;
		private System.Windows.Media.Brush			errorBrush						= Brushes.Black;
		private SharpDX.Direct2D1.Brush 			pivotRangeBrushDX 				= null;
		private	SharpDX.Direct2D1.Brush 			centralRangeBrushDX 			= null;
		private	SharpDX.Direct2D1.Brush 			jacksonZonesBrushDX				= null;
		private	SharpDX.Direct2D1.SolidColorBrush 	transparentBrushDX				= null;
		private	SharpDX.Direct2D1.Brush[] 			brushesDX						= null;
		private SimpleFont							errorFont						= null;
		private string								errorText1						= "The amaPivotsDailyBak indicator only works on price data.";
		private string								errorText2						= "The amaPivotsDailyBak indicator cannot be used on weekly or monthly charts.";
		private string								errorText3						= "The amaPivotsDailyBak indicator cannot be used with a displacement.";
		private string								errorText4						= "The amaPivotsDailyBak indicator cannot be used when the Break EOD data series property is unselected.";
		private string								errorText5						= "The amaPivotsDailyBak indicator requires the setting 'DailyBars' when it is used on daily charts.";
		private string								errorText6						= "amaPivotsDailyBak";
		private string								errorText7						= "amaPivotsDailyBak";
		private string								errorText8						= "amaPivotsDailyBak";
		private int									pivotRangeOpacity				= 60;
		private int									centralRangeOpacity				= 60;
		private int									jacksonZonesOpacity				= 60;
		private int									plot0Width						= 2;
		private int									plot1Width						= 3;
		private int									plot2Width						= 1;
		private int									textFontSize					= 14;
		private int									shiftLabelOffset				= 10;
		private PlotStyle							plot0Style						= PlotStyle.Line;
		private DashStyleHelper						dash0Style						= DashStyleHelper.Solid;
		private PlotStyle							plot1Style						= PlotStyle.Line;
		private DashStyleHelper						dash1Style						= DashStyleHelper.Dot;
		private PlotStyle							plot2Style						= PlotStyle.Line;
		private DashStyleHelper						dash2Style						= DashStyleHelper.Dash;
		private string								versionString					= "v 2.6  -  July 4, 2018";
		private Series<DateTime>					tradingDate0;
		private Series<DateTime>					tradingDate1;
		private Series<int>							countDown;
		private Series<double>						currentHistoricalHigh;
		private Series<double>						currentHistoricalLow;
		private Series<double>						currentHistoricalClose;
		private Series<double>						currentPrimaryBarsHigh;
		private Series<double>						currentPrimaryBarsLow;
		private Series<double>						currentPrimaryBarsClose;
		
		private DateTime			currentDate			=	Core.Globals.MinDate;
		private double				currentOpen			=	double.MinValue;
		private double				currentHigh			=	double.MinValue;
		private double				currentLow			=	double.MaxValue;
		private DateTime			lastDate			= 	Core.Globals.MinDate;
		private SessionIterator		sessionIterator;
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Daily Pivots indicator can be used to display floor pivots, Globex pivots and the high, low and close of the prior day.";
				Name						= "amaPivotsDailyBak";
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				IsAutoScale					= false;
				
				ShowLow						= true;
				ShowHigh					= true;
				ShowOpen					= true;
				
				ArePlotsConfigurable		= false;
				if(Calculate == Calculate.OnEachTick)
					Calculate = Calculate.OnPriceChange;
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"PP");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"CP");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"DP");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"R1");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"S1");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"R2");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"S2");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"R3");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"S3");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"R4");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"S4");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"Y-High");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"Y-Low");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"Y-Close");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"RMid");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"SMid");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"R12Mid");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"S12Mid");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"R23Mid");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"S23Mid");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"R34Mid");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"S34Mid");
				
				AddPlot(new Stroke(Brushes.Goldenrod,	DashStyleHelper.Dash, 2), PlotStyle.Square, "COpen");
				AddPlot(new Stroke(Brushes.SeaGreen,	DashStyleHelper.Dash, 2), PlotStyle.Square, "CHigh");
				AddPlot(new Stroke(Brushes.Red,			DashStyleHelper.Dash, 2), PlotStyle.Square, "CLow");
			}
			else if (State == State.Configure)
			{
				if(sessionType == amaSessionTypePD.Daily_Bars || calcMode == amaCalcModePD.Daily_Data)
					AddDataSeries(BarsPeriodType.Day, 1);
				BarsRequiredToPlot = 3;
				displacement = Displacement;
				pivotRangeBrush = pivotRangeBrushS.Clone();
				pivotRangeBrush.Opacity = (float) pivotRangeOpacity/100.0;
				pivotRangeBrush.Freeze();
				centralRangeBrush = centralRangeBrushS.Clone();
				centralRangeBrush.Opacity = (float) centralRangeOpacity/100.0;
				centralRangeBrush.Freeze();
				jacksonZonesBrush = jacksonZonesBrushS.Clone();
				jacksonZonesBrush.Opacity = (float) jacksonZonesOpacity/100.0;
				jacksonZonesBrush.Freeze();
				if(showPP)
					Plots[0].Brush = pivotBrush;
				else
					Plots[0].Brush = Brushes.Transparent;
				if(showCP)
					Plots[1].Brush = centralPivotBrush;
				else
					Plots[1].Brush = Brushes.Transparent;
				if(showDP)
					Plots[2].Brush = directionalPivotBrush;
				else
					Plots[2].Brush = Brushes.Transparent;
				if(showPivots)
				{	
					Plots[3].Brush = resistanceBrush;
					Plots[4].Brush = supportBrush;
					Plots[5].Brush = resistanceBrush;
					Plots[6].Brush = supportBrush;
					Plots[7].Brush = resistanceBrush;
					Plots[8].Brush = supportBrush;
					Plots[9].Brush = resistanceBrush;
					Plots[10].Brush = supportBrush;
				}	
				else
				{	
					Plots[3].Brush = Brushes.Transparent;
					Plots[4].Brush = Brushes.Transparent;
					Plots[5].Brush = Brushes.Transparent;
					Plots[6].Brush = Brushes.Transparent;
					Plots[7].Brush = Brushes.Transparent;
					Plots[8].Brush = Brushes.Transparent;
					Plots[9].Brush = Brushes.Transparent;
					Plots[10].Brush = Brushes.Transparent;
				}
				if(showPriorHLC)
				{	
					Plots[11].Brush = priorHighBrush;
					Plots[12].Brush = priorLowBrush;
					Plots[13].Brush = priorCloseBrush;
				}	
				else
				{	
					Plots[11].Brush = Brushes.Transparent;
					Plots[12].Brush = Brushes.Transparent;
					Plots[13].Brush = Brushes.Transparent;
				}
				if(showMidpivots && pivotFormula != amaPivotFormulaPD.Jackson_Zones && pivotFormula != amaPivotFormulaPD.Fibonacci_Pivots)
				{	
					Plots[14].Brush = resistanceMidBrush;
					Plots[15].Brush = supportMidBrush;
					Plots[16].Brush = resistanceMidBrush;
					Plots[17].Brush = supportMidBrush;
					Plots[18].Brush = resistanceMidBrush;
					Plots[19].Brush = supportMidBrush;
					Plots[20].Brush = resistanceMidBrush;
					Plots[21].Brush = supportMidBrush;
				}	
				else
				{	
					Plots[14].Brush = Brushes.Transparent;
					Plots[15].Brush = Brushes.Transparent;
					Plots[16].Brush = Brushes.Transparent;
					Plots[17].Brush = Brushes.Transparent;
					Plots[18].Brush = Brushes.Transparent;
					Plots[19].Brush = Brushes.Transparent;
					Plots[20].Brush = Brushes.Transparent;
					Plots[21].Brush = Brushes.Transparent;
				}
				for (int i = 0; i < 11; i++)
				{
					Plots[i].Width = plot0Width;
					Plots[i].DashStyleHelper = dash0Style;
				}
				for (int i = 11; i < 14; i++)
				{
					Plots[i].Width = plot1Width;
					Plots[i].DashStyleHelper = dash1Style;
				}
				for (int i = 14; i < 22; i++)
				{
					Plots[i].Width = plot2Width;
					Plots[i].DashStyleHelper = dash2Style;
				}
				if (pivotFormula == amaPivotFormulaPD.Jackson_Zones)
				{
					Plots[3].Name = "R1";
					Plots[4].Name = "S1";
					Plots[5].Name = "RB1";
					Plots[6].Name = "SB1";
					Plots[7].Name = "R2";
					Plots[8].Name = "S2";
					Plots[9].Name = "RB2";
					Plots[10].Name = "SB2";
				}
				else
				{
					Plots[3].Name = "R1";
					Plots[4].Name = "S1";
					Plots[5].Name = "R2";
					Plots[6].Name = "S2";
					Plots[7].Name = "R3";
					Plots[8].Name = "S3";
					Plots[9].Name = "R4";
					Plots[10].Name = "S4";
				}
				brushesDX = new SharpDX.Direct2D1.Brush[Values.Length];
				
				currentDate			= Core.Globals.MinDate;
				currentOpen			= double.MinValue;
				currentHigh			= double.MinValue;
				currentLow			= double.MaxValue;
				lastDate			= Core.Globals.MinDate;
			}
		  	else if (State == State.DataLoaded)
		 	{
				if(BarsArray[0].BarsType.IsIntraday)
					isIntraday0 = true;
				else
					isIntraday0 = false;
				tradingDate0 = new Series<DateTime>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				countDown = new Series<int>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				if(isIntraday0 && (sessionType == amaSessionTypePD.Daily_Bars || calcMode == amaCalcModePD.Daily_Data))
				{
					tradingDate1 = new Series<DateTime>(BarsArray[1], MaximumBarsLookBack.TwoHundredFiftySix);
					currentHistoricalHigh = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
					currentHistoricalLow = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
					currentHistoricalClose = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				}
				if((isIntraday0 && sessionType != amaSessionTypePD.Daily_Bars) || !isIntraday0)
				{	
					currentPrimaryBarsHigh = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
					currentPrimaryBarsLow = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
					currentPrimaryBarsClose = new Series<double>(BarsArray[0], MaximumBarsLookBack.TwoHundredFiftySix);
				}	
				if (Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex && (TickSize == 0.00001 || TickSize == 0.001))
					displaySize = 5 * TickSize;
				else
					displaySize = TickSize;
				if (BarsArray[0].BarsType.IsTimeBased) 
					timeBased0 = true;
				else
					timeBased0 = false;
				if(Input is PriceSeries)
					calculateFromPriceData = true;
				else
					calculateFromPriceData = false;
				if(calcMode	!= amaCalcModePD.Daily_Data && sessionType != amaSessionTypePD.Daily_Bars)
					calculateFromIntradayData = true;
				else
					calculateFromIntradayData = false;
		    	sessionIterator0 = new SessionIterator(BarsArray[0]);
		  	}
			else if (State == State.Historical)
			{
				if(ChartBars != null)
				{	
					breakAtEOD = ChartBars.Bars.IsResetOnNewTradingDay;
					errorBrush = ChartControl.Properties.AxisPen.Brush;
					errorBrush.Freeze();
					errorFont = new SimpleFont("Arial", 24);
				}
				this.ZOrder = -2; //SetZOrder(-2);
				errorMessage = false;
				if(!calculateFromPriceData)
				{
					Draw.TextFixed(this, "error text 1", errorText1, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}	
				else if(BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Week || BarsArray[0].BarsPeriod.BarsPeriodType == BarsPeriodType.Month)
				{
					Draw.TextFixed(this, "error text 2", errorText2, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if(displacement != 0)
				{
					Draw.TextFixed(this, "error text 3", errorText3, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if(!breakAtEOD)
				{
					Draw.TextFixed(this, "error text 4", errorText4, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
				else if (!isIntraday0 && sessionType != amaSessionTypePD.Daily_Bars)
				{
					Draw.TextFixed(this, "error text 5", errorText5, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
					basicError = true;
				}
			}	
		}

		protected override void OnBarUpdate()
		{
			if(BarsInProgress == 0)
			{
				if(IsFirstTickOfBar)
				{	
					if(errorMessage)
					{	
						if(basicError)
							return;
						else if(sundaySessionError)
						{	
							Draw.TextFixed(this, "error text 6", errorText6, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);
							return;
						}	
						else if(dailyBarsError)
						{	
							Draw.TextFixed(this, "error text 7", errorText7, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);
							return;
						}
					}	
				}	
					
				if (!initBarSeries0 || CurrentBars[0] == 0)
				{	
					if(!initBarSeries0)
					{
						tradingDate0[0] = GetLastBarSessionDate0(Times[0][0]);
						countDown[0] = 2;
						if(isIntraday0 && (sessionType == amaSessionTypePD.Daily_Bars || calcMode == amaCalcModePD.Daily_Data))
						{	
							currentHistoricalHigh.Reset();
							currentHistoricalLow.Reset();
							currentHistoricalClose.Reset();
						}	
						if((isIntraday0 && sessionType != amaSessionTypePD.Daily_Bars) || !isIntraday0)
						{	
							currentPrimaryBarsHigh.Reset();
							currentPrimaryBarsLow.Reset();
							currentPrimaryBarsClose.Reset();
						}	
						PP.Reset();
						CP.Reset();
						DP.Reset();
						R1.Reset();
						S1.Reset();
						R2.Reset();
						S2.Reset();
						R3.Reset();
						S3.Reset();
						R4.Reset();
						S4.Reset();
						PriorHigh.Reset();
						PriorLow.Reset();
						PriorClose.Reset();
						RMid.Reset();
						SMid.Reset();
						R12Mid.Reset();
						S12Mid.Reset();
						R23Mid.Reset();
						S23Mid.Reset();
						R34Mid.Reset();
						S34Mid.Reset();
						initBarSeries0 = true;
					}	
					return;
				}
				
				if(IsFirstTickOfBar)
				{	
					if(BarsArray[0].IsFirstBarOfSession || !isIntraday0)
					{	
						tradingDate0[0] = GetLastBarSessionDate0(Times[0][0]);
						if(tradingDate0[0].DayOfWeek == DayOfWeek.Sunday)
						{
							sundaySessionError = true;
							errorMessage = true;
							return;
						}
						if(tradingDate0[0] != tradingDate0[1])
						{
							if((sessionType == amaSessionTypePD.Daily_Bars || calcMode == amaCalcModePD.Daily_Data) && CurrentBars[1] < 0)
								dailyBarsError = true;
							else
								dailyBarsError = false;
							countDown[0] = countDown[1] - 1; 
							if(countDown[0] <= 0) // calculations are only performed for the third day with intraday data (first day maybe incomplete, second day is needed for collecting data)
							{	
								if (dailyBarsError)
								{
									errorMessage = true;
									return;
								}	
								if(!isIntraday0) // daily pivots displayed on a daily chart
								{
									priorHigh = currentPrimaryBarsHigh[1];
									priorLow = currentPrimaryBarsLow[1];
									priorClose = currentPrimaryBarsClose[1];
								}
								else if(sessionType == amaSessionTypePD.Daily_Bars) // daily pivots calculated from daily data  
								{	
									if(IsConnected())
									{
										if(tradingDate0[0] > tradingDate1[0]) 
										{ 
											priorHigh = currentDailyHigh;	
											priorLow = currentDailyLow;
											priorClose = currentDailyClose;
										}
										else
										{
											priorHigh = priorDailyHigh;	
											priorLow = priorDailyLow;
											priorClose = priorDailyClose;
										}	
									}
									else // workaround needed because of NinjaTrader bar processing bug
									{
										priorHigh = currentHistoricalHigh[1];
										priorLow = currentHistoricalLow[1];
										priorClose = currentHistoricalClose[1];
										count = 0;
										priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1]));
										dailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][0]));
										if(priorDailyBarDate == dailyBarDate && BarsArray[1].Count - 2 > CurrentBars[1])
										{	
											while (priorDailyBarDate >= dailyBarDate)
											{	
												count = count - 1;
												priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
												if (count == -5)
													break;
											}	
											priorHigh = BarsArray[1].GetHigh(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
											priorLow = BarsArray[1].GetLow(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
											priorClose = BarsArray[1].GetClose(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
										}	
									}	
								}
								else if(calcMode == amaCalcModePD.Daily_Data) // daily pivots calculated with highs and low taken from intraday data, but daily close taken from daily data
								{
									priorHigh = currentPrimaryBarsHigh[1];
									priorLow = currentPrimaryBarsLow[1];
									if(IsConnected())
									{	
										if(tradingDate0[0] > tradingDate1[0]) 
											priorClose = currentDailyClose;
										else
											priorClose = priorDailyClose;
									}	
									else // workaround needed because of NinjaTrader bar processing bug
									{
										priorClose = currentHistoricalClose[1];
										count = 0;
										priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1]));
										dailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][0]));
										if(priorDailyBarDate == dailyBarDate && BarsArray[1].Count - 2 > CurrentBars[1])
										{	
											while (priorDailyBarDate >= dailyBarDate)
											{	
												count = count - 1;
												priorDailyBarDate = BarsArray[1].GetTime(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
												if (count == -5)
													break;
											}	
											priorClose = BarsArray[1].GetClose(BarsArray[1].GetBar(Times[0][1].AddDays(count)));
										}	
									}	
								}	
								else // daily pivots calculated from intraday data
								{
									priorHigh = currentPrimaryBarsHigh[1];	
									priorLow = currentPrimaryBarsLow[1];
									priorClose = currentPrimaryBarsClose[1];
								}
								pp				= (priorHigh + priorLow + priorClose)/3.0;
								cp				= (priorHigh + priorLow) / 2.0;
								priorRange 		= priorHigh - priorLow;						
								upside  		= pp - priorLow;
								downside		= priorHigh - pp;
								if(pivotFormula == amaPivotFormulaPD.Floor_Trader_Pivots)
								{
									r1		= pp + upside;
									s1		= pp - downside;
									r2		= pp + priorRange;
									s2		= pp - priorRange;
									r3		= r1 + priorRange;
									s3		= s1 - priorRange;
									r4		= r3 + upside;
									s4		= s3 - downside;
								}
								else if (pivotFormula == amaPivotFormulaPD.Wide_Pivots)
								{
									r1		= pp + upside;
									s1		= pp - downside;
									r2		= pp + priorRange;
									s2		= pp - priorRange;
									r3		= r2 + priorRange;
									s3		= s2 - priorRange;
									r4		= r3 + priorRange;
									s4		= s3 - priorRange;
								}
								else if (pivotFormula == amaPivotFormulaPD.Jackson_Zones)
								{
									r1		= pp + 0.5*priorRange;
									s1		= pp - 0.5*priorRange;
									r2		= pp + 0.618*priorRange;
									s2		= pp - 0.618*priorRange;
									r3		= pp + priorRange;
									s3		= pp - priorRange;
									r4		= pp + 1.382*priorRange;
									s4		= pp - 1.382*priorRange;
								}
								else if (pivotFormula == amaPivotFormulaPD.Fibonacci_Pivots)
								{
									r1		= pp + 0.382*priorRange;
									s1		= pp - 0.382*priorRange;
									r2		= pp + 0.618*priorRange;
									s2		= pp - 0.618*priorRange;
									r3		= pp + priorRange;
									s3		= pp - priorRange;
									r4		= pp + 1.382*priorRange;
									s4		= pp - 1.382*priorRange;
								}
								rmid		= 0.5*(pp+r1);
								smid		= 0.5*(pp+s1);
								r12mid		= 0.5*(r1+r2);
								s12mid		= 0.5*(s1+s2);
								r23mid		= 0.5*(r2+r3);
								s23mid		= 0.5*(s2+s3);
								r34mid		= 0.5*(r3+r4);
								s34mid		= 0.5*(s3+s4);
								if(countDown[0] <= 0)
								{	
									PP[0] = Math.Round(pp/displaySize) * displaySize;
									CP[0] = Math.Round((cp + 0.01*displaySize)/displaySize)*displaySize;
									DP[0] = 2.0*PP[0] - CP[0];
									R1[0] = Math.Round(r1/displaySize) * displaySize;
									S1[0] = Math.Round(s1/displaySize) * displaySize;
									R2[0] = Math.Round(r2/displaySize) * displaySize;
									S2[0] = Math.Round(s2/displaySize) * displaySize;
									R3[0] = Math.Round(r3/displaySize) * displaySize;
									S3[0] = Math.Round(s3/displaySize) * displaySize;
									R4[0] = Math.Round(r4/displaySize) * displaySize;
									S4[0] = Math.Round(s4/displaySize) * displaySize;
									PriorHigh[0] = priorHigh;
									PriorLow[0] = priorLow;
									PriorClose[0] = priorClose;
									RMid[0] = Math.Round(rmid/displaySize)* displaySize;
									SMid[0] = Math.Round(smid/displaySize)* displaySize;
									R12Mid[0] = Math.Round(r12mid/displaySize)* displaySize;
									S12Mid[0] = Math.Round(s12mid/displaySize)* displaySize;
									R23Mid[0] = Math.Round(r23mid/displaySize)* displaySize;
									S23Mid[0] = Math.Round(s23mid/displaySize)* displaySize;
									R34Mid[0] = Math.Round(r34mid/displaySize)* displaySize;
									S34Mid[0] = Math.Round(s34mid/displaySize)* displaySize;
								}	
							}
						}	
						else // case where Bars.IsFirstBarOfSession is true inside the trading day
						{	
							tradingDate0[0] = tradingDate0[1];
							countDown[0] = countDown[1];
							if (countDown[0] <= 0)
							{
								PP[0] = PP[1];
								CP[0] = CP[1];
								DP[0] = DP[1];
								R1[0] = R1[1];
								S1[0] = S1[1];
								R2[0] = R2[1];
								S2[0] = S2[1];
								R3[0] = R3[1];
								S3[0] = S3[1];
								R4[0] = R4[1];
								S4[0] = S4[1];
								PriorHigh[0] = PriorHigh[1];
								PriorLow[0] = PriorLow[1];
								PriorClose[0] = PriorClose[1];
								RMid[0] = RMid[1];
								SMid[0] = SMid[1];
								R12Mid[0] = R12Mid[1];
								S12Mid[0] = S12Mid[1];
								R23Mid[0] = R23Mid[1];
								S23Mid[0] = S23Mid[1];
								R34Mid[0] = R34Mid[1];
								S34Mid[0] = S34Mid[1];
							}	
						}	
					}
					else // case where Bars.IsFirstBarOfSession is false
					{	
						tradingDate0[0] = tradingDate0[1];
						countDown[0] = countDown[1];
						if (countDown[0] <= 0)
						{
							PP[0] = PP[1];
							CP[0] = CP[1];
							DP[0] = DP[1];
							R1[0] = R1[1];
							S1[0] = S1[1];
							R2[0] = R2[1];
							S2[0] = S2[1];
							R3[0] = R3[1];
							S3[0] = S3[1];
							R4[0] = R4[1];
							S4[0] = S4[1];
							PriorHigh[0] = PriorHigh[1];
							PriorLow[0] = PriorLow[1];
							PriorClose[0] = PriorClose[1];
							RMid[0] = RMid[1];
							SMid[0] = SMid[1];
							R12Mid[0] = R12Mid[1];
							S12Mid[0] = S12Mid[1];
							R23Mid[0] = R23Mid[1];
							S23Mid[0] = S23Mid[1];
							R34Mid[0] = R34Mid[1];
							S34Mid[0] = S34Mid[1];
						}	
					}
					
					if(countDown[0] > 0)
					{
						PP.Reset();
						CP.Reset();
						DP.Reset();
						R1.Reset();
						S1.Reset();
						R2.Reset();
						S2.Reset();
						R3.Reset();
						S3.Reset();
						R4.Reset();
						S4.Reset();
						PriorHigh.Reset();
						PriorLow.Reset();
						PriorClose.Reset();
						RMid.Reset();
						SMid.Reset();
						R12Mid.Reset();
						S12Mid.Reset();
						R23Mid.Reset();
						S23Mid.Reset();
						R34Mid.Reset();
						S34Mid.Reset();
					}
					
					// needed as a workaround for NinjaTrader bar processing bug
					if(isIntraday0 && (sessionType == amaSessionTypePD.Daily_Bars || calcMode == amaCalcModePD.Daily_Data) && CurrentBars[1] >= 0)  
					{	
						if(CurrentBars[1] >= 0)
						{	
							currentHistoricalHigh[0] = BarsArray[1].GetHigh(BarsArray[1].GetBar(Times[0][0]));
							currentHistoricalLow[0] = BarsArray[1].GetLow(BarsArray[1].GetBar(Times[0][0]));
							currentHistoricalClose[0] = BarsArray[1].GetClose(BarsArray[1].GetBar(Times[0][0]));
						}
						else
						{
							currentHistoricalHigh.Reset();
							currentHistoricalLow.Reset();
							currentHistoricalClose.Reset();
						}	
					}
				}	
				
				if((isIntraday0 && sessionType != amaSessionTypePD.Daily_Bars) || !isIntraday0)
				{	
					if (tradingDate0[0] != tradingDate0[1])
					{	
						currentPrimaryBarsHigh[0]	= Highs[0][0];
						currentPrimaryBarsLow[0] 	= Lows[0][0];
						currentPrimaryBarsClose[0]	= Closes[0][0];
					}
					else 
					{	
						currentPrimaryBarsHigh[0]	= Math.Max(currentPrimaryBarsHigh[1], Highs[0][0]);
						currentPrimaryBarsLow[0] 	= Math.Min(currentPrimaryBarsLow[1], Lows[0][0]);
						currentPrimaryBarsClose[0]	= Closes[0][0];
					}
				}
			}	
				
			if(BarsInProgress == 1 && isIntraday0)
			{
				if(CurrentBars[1] == 0)
				{	
					if(IsFirstTickOfBar)
						tradingDate1[0] = GetLastBarSessionDate1(Times[1][0]);
					currentDailyHigh	= Highs[1][0];
					currentDailyLow	 	= Lows[1][0];
					currentDailyClose	= Closes[1][0];
					return;
				}	
				
				if(IsFirstTickOfBar)
				{	
					tradingDate1[0] = GetLastBarSessionDate1(Times[1][0]);
					if(tradingDate1[0] != tradingDate1[1])
					{	
						priorDailyHigh 		= currentDailyHigh;
						priorDailyLow  		= currentDailyLow;
						priorDailyClose 	= currentDailyClose;
						currentDailyHigh	= Highs[1][0];
						currentDailyLow	 	= Lows[1][0];
						currentDailyClose	= Closes[1][0];
					}
					else
					{
						currentDailyHigh	= Math.Max(currentDailyHigh, Highs[1][0]);
						currentDailyLow	 	= Math.Min(currentDailyLow, Lows[1][0]);
						currentDailyClose	= Closes[1][0];
					}
				}	
				else
				{
					currentDailyHigh	= Math.Max(currentDailyHigh, Highs[1][0]);
					currentDailyLow	 	= Math.Min(currentDailyLow, Lows[1][0]);
					currentDailyClose	= Closes[1][0];
				}
			}
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PP
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CP
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> DP
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> R1
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> S1
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> R2
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> S2
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> R3
		{
			get { return Values[7]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> S3
		{
			get { return Values[8]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> R4
		{
			get { return Values[9]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> S4
		{
			get { return Values[10]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PriorHigh
		{
			get { return Values[11]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PriorLow
		{
			get { return Values[12]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> PriorClose
		{
			get { return Values[13]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> RMid
		{
			get { return Values[14]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SMid
		{
			get { return Values[15]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> R12Mid
		{
			get { return Values[16]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> S12Mid
		{
			get { return Values[17]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> R23Mid
		{
			get { return Values[18]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> S23Mid
		{
			get { return Values[19]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> R34Mid
		{
			get { return Values[20]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> S34Mid
		{
			get { return Values[21]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public int CacheLastBarPainted
		{
			get { return cacheLastBarPainted; }
			set { cacheLastBarPainted = value; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public bool LastBarStartsNewPeriod
		{
			get { return lastBarStartsNewPeriod; }
			set { lastBarStartsNewPeriod = value; }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Pivot formula", Description = "Select formula for calculating the pivots", GroupName = "Algorithmic Options", Order = 0)]
 		[RefreshProperties(RefreshProperties.All)] 
		public amaPivotFormulaPD PivotFormula
		{	
            get { return pivotFormula; }
            set { pivotFormula = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Input data", Description = "Select between taking high, low and close from intraday data or from daily data", GroupName = "Algorithmic Options", Order = 1)]
 		[RefreshProperties(RefreshProperties.All)] 
		public amaSessionTypePD SessionType
		{	
            get { return sessionType; }
            set { sessionType = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Input data for close", Description = "Select between taking close from intraday data or from daily data", GroupName = "Algorithmic Options", Order = 2)]
		public amaCalcModePD CalcMode
		{	
            get { return calcMode; }
            set { calcMode = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show main pivot PP", Description = "Shows main pivot PP", GroupName = "Display Options", Order = 0)]
    	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowPP
        {
            get { return showPP; }
            set { showPP = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show central pivot CP", Description = "Shows central pivot CP", GroupName = "Display Options", Order = 1)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowCP
        {
            get { return showCP; }
            set { showCP = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show directional pivot DP", Description = "Shows directional pivot DP", GroupName = "Display Options", Order = 2)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowDP
        {
            get { return showDP; }
            set { showDP = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show pivot range", Description = "Shows central and directional pivot", GroupName = "Display Options", Order = 3)]
      	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowPivotRange
        {
            get { return showPivotRange; }
            set { showPivotRange = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show prior HLC", Description = "Shows prior day high, low and close (settlement)", GroupName = "Display Options", Order = 4)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowPriorHLC
        {
            get { return showPriorHLC; }
            set { showPriorHLC = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show S/R pivots", Description = "Shows all support and resistance pivots", GroupName = "Display Options", Order = 5)]
    	[RefreshProperties(RefreshProperties.All)] 
        public bool ShowPivots
        {
            get { return showPivots; }
            set { showPivots = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show Jackson Zones", Description = "Shows Jackson Zones", GroupName = "Display Options", Order = 6)]
      	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowJacksonZones
        {
            get { return showJacksonZones; }
            set { showJacksonZones = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show midpivots", Description = "Shows midpivot levels", GroupName = "Display Options", Order = 7)]
     	[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowMidpivots
        {
            get { return showMidpivots; }
            set { showMidpivots = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show labels", Description = "Shows labels for all pivot lines", GroupName = "Display Options", Order = 8)]
  		[RefreshProperties(RefreshProperties.All)] 
       	public bool ShowLabels
        {
            get { return showLabels; }
            set { showLabels = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Main pivot", Description = "Sets the color for the main pivot PP", GroupName = "Plot Colors", Order = 0)]
		public System.Windows.Media.Brush PivotBrush
		{ 
			get {return pivotBrush;}
			set {pivotBrush = value;}
		}

		[Browsable(false)]
		public string PivotBrushSerializable
		{
			get { return Serialize.BrushToString(pivotBrush); }
			set { pivotBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Central pivot", Description = "Sets the color for the central pivot CP", GroupName = "Plot Colors", Order = 1)]
		public System.Windows.Media.Brush CentralPivotBrush
		{ 
			get {return centralPivotBrush;}
			set {centralPivotBrush = value;}
		}

		[Browsable(false)]
		public string CentralPivotBrushSerializable
		{
			get { return Serialize.BrushToString(centralPivotBrush); }
			set { centralPivotBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Directional pivot", Description = "Sets the color for the directional pivot DP", GroupName = "Plot Colors", Order = 2)]
		public System.Windows.Media.Brush DirectionalPivotBrush
		{ 
			get {return directionalPivotBrush;}
			set {directionalPivotBrush = value;}
		}

		[Browsable(false)]
		public string DirectionalPivotBrushSerializable
		{
			get { return Serialize.BrushToString(directionalPivotBrush); }
			set { directionalPivotBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Prior day high", Description = "Sets the color for the prior day high", GroupName = "Plot Colors", Order = 3)]
		public System.Windows.Media.Brush PriorHighBrush
		{ 
			get {return priorHighBrush;}
			set {priorHighBrush = value;}
		}

		[Browsable(false)]
		public string PriorHighBrushSerializable
		{
			get { return Serialize.BrushToString(priorHighBrush); }
			set { priorHighBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Prior day low", Description = "Sets the color for the prior day low", GroupName = "Plot Colors", Order = 4)]
		public System.Windows.Media.Brush PriorLowBrush
		{ 
			get {return priorLowBrush;}
			set {priorLowBrush = value;}
		}

		[Browsable(false)]
		public string PriorLowBrushSerializable
		{
			get { return Serialize.BrushToString(priorLowBrush); }
			set { priorLowBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Prior day close", Description = "Sets the color for the prior day close", GroupName = "Plot Colors", Order = 5)]
		public System.Windows.Media.Brush PriorCloseBrush
		{ 
			get {return priorCloseBrush;}
			set {priorCloseBrush = value;}
		}

		[Browsable(false)]
		public string PriorCloseBrushSerializable
		{
			get { return Serialize.BrushToString(priorCloseBrush); }
			set { priorCloseBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Resistance pivots", Description = "Sets the color for the resistance levels R1, R2, R3 and R4", GroupName = "Plot Colors", Order = 6)]
		public System.Windows.Media.Brush ResistanceBrush
		{ 
			get {return resistanceBrush;}
			set {resistanceBrush = value;}
		}

		[Browsable(false)]
		public string ResistanceBrushSerializable
		{
			get { return Serialize.BrushToString(resistanceBrush); }
			set { resistanceBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Support pivots", Description = "Sets the color for the support levels S1, S2, S3 and S4", GroupName = "Plot Colors", Order = 7)]
		public System.Windows.Media.Brush SupportBrush
		{ 
			get {return supportBrush;}
			set {supportBrush = value;}
		}

		[Browsable(false)]
		public string SupportBrushSerializable
		{
			get { return Serialize.BrushToString(supportBrush); }
			set { supportBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Resistance midpivots", Description = "Sets the color for the resistance midpivots", GroupName = "Plot Colors", Order = 8)]
		public System.Windows.Media.Brush ResistanceMidBrush
		{ 
			get {return resistanceMidBrush;}
			set {resistanceMidBrush = value;}
		}

		[Browsable(false)]
		public string ResistanceMidBrushSerializable
		{
			get { return Serialize.BrushToString(resistanceMidBrush); }
			set { resistanceMidBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Support midpivots", Description = "Sets the color for the support midpivots", GroupName = "Plot Colors", Order = 9)]
		public System.Windows.Media.Brush SupportMidBrush
		{ 
			get {return supportMidBrush;}
			set {supportMidBrush = value;}
		}

		[Browsable(false)]
		public string SupportMidBrushSerializable
		{
			get { return Serialize.BrushToString(supportMidBrush); }
			set { supportMidBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style pivots", Description = "Sets the dash style for all pivot lines", GroupName = "Plot Parameters", Order = 0)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width pivots", Description = "Sets the plot width for all pivots", GroupName = "Plot Parameters", Order = 1)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style prior HLC", Description = "Sets the dash style for the prior day high, low and close", GroupName = "Plot Parameters", Order = 2)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width prior HLC", Description = "Sets the plot width for the prior day high, low and close", GroupName = "Plot Parameters", Order = 3)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style midpivots", Description = "Sets the dash style for the midpivots", GroupName = "Plot Parameters", Order = 4)]
		public DashStyleHelper Dash2Style
		{
			get { return dash2Style; }
			set { dash2Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width midpivots", Description = "Sets the plot width for the midpivots", GroupName = "Plot Parameters", Order = 5)]
		public int Plot2Width
		{	
            get { return plot2Width; }
            set { plot2Width = value; }
		}
		
		[Range(10, 40)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Label font size", Description = "Sets the size for all pivot labels", GroupName = "Plot Parameters", Order = 6)]
		public int TextFontSize
		{	
            get { return textFontSize; }
            set { textFontSize = value; }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Label offset", Description = "Allows for shifting the labels further to the right", GroupName = "Plot Parameters", Order = 7)]
		public int ShiftLabelOffset
		{	
            get { return shiftLabelOffset; }
            set { shiftLabelOffset = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Pivot range", Description = "Sets the color for the pivot range", GroupName = "Shaded Areas", Order = 0)]
		public System.Windows.Media.Brush PivotRangeBrushS
		{ 
			get {return pivotRangeBrushS;}
			set {pivotRangeBrushS = value;}
		}

		[Browsable(false)]
		public string PivotRangeBrushSSerializable
		{
			get { return Serialize.BrushToString(pivotRangeBrushS); }
			set { pivotRangeBrushS = Serialize.StringToBrush(value); }
		}
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Pivot range opacity", Description = "Sets the opacity for the pivot range", GroupName = "Shaded Areas", Order = 1)]
        public int PivotRangeOpacity
        {
            get { return pivotRangeOpacity; }
            set { pivotRangeOpacity = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Central range", Description = "Sets the color for the central range", GroupName = "Shaded Areas", Order = 2)]
		public System.Windows.Media.Brush CentralRangeBrushS
		{ 
			get {return centralRangeBrushS;}
			set {centralRangeBrushS = value;}
		}

		[Browsable(false)]
		public string CentralRangeBrushSSerializable
		{
			get { return Serialize.BrushToString(centralRangeBrushS); }
			set { centralRangeBrushS = Serialize.StringToBrush(value); }
		}
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Central range opacity", Description = "Sets the opacity for the central range", GroupName = "Shaded Areas", Order = 3)]
        public int CentralRangeOpacity
        {
            get { return centralRangeOpacity; }
            set { centralRangeOpacity = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Jackson Zones", Description = "Sets the color for the Jackson Zones", GroupName = "Shaded Areas", Order = 4)]
		public System.Windows.Media.Brush JacksonZonesBrushS
		{ 
			get {return jacksonZonesBrushS;}
			set {jacksonZonesBrushS = value;}
		}

		[Browsable(false)]
		public string JacksonZonesBrushSSerializable
		{
			get { return Serialize.BrushToString(jacksonZonesBrushS); }
			set { jacksonZonesBrushS = Serialize.StringToBrush(value); }
		}
		
		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Jackson Zones opacity", Description = "Sets the opacity for the Jackson Zones", GroupName = "Shaded Areas", Order = 5)]
        public int JacksonZonesOpacity
        {
            get { return jacksonZonesOpacity; }
            set { jacksonZonesOpacity = value; }
        }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}
		#endregion
		
		#region Miscellaneous
		
		public override string FormatPriceMarker(double price)
		{
			return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
		}			

		private bool IsConnected()
        {
			try
			{
				if (BarsArray[0] != null && BarsArray[0].Instrument.GetMarketDataConnection().PriceStatus == NinjaTrader.Cbi.ConnectionStatus.Connected)
					return true;
				else
					return false;
			}
			catch
			{
				return false;
			}	
		}
		
		private DateTime GetLastBarSessionDate0(DateTime time)
		{
			sessionIterator0.CalculateTradingDay(time, timeBased0);
			sessionDateTmp0 = sessionIterator0.ActualTradingDayExchange;
			if(cacheSessionDate0 != sessionDateTmp0) 
			{
				cacheSessionDate0 = sessionDateTmp0;
				if (newSessionBarIdxArr.Count == 0 || (newSessionBarIdxArr.Count > 0 && CurrentBars[0] > (int) newSessionBarIdxArr[newSessionBarIdxArr.Count - 1]))
						newSessionBarIdxArr.Add(CurrentBars[0]);
			}
			return sessionDateTmp0;			
		}
		
		private DateTime GetLastBarSessionDate1(DateTime time)
		{
			sessionIterator0.CalculateTradingDay(time, true);
			sessionDateTmp1 = sessionIterator0.ActualTradingDayExchange;
			return sessionDateTmp1;			
		}
		
		public override void OnRenderTargetChanged()
		{
			if (pivotRangeBrushDX != null)
				pivotRangeBrushDX.Dispose();
			if (centralRangeBrushDX != null)
				centralRangeBrushDX.Dispose();
			if (jacksonZonesBrushDX != null)
				jacksonZonesBrushDX.Dispose();
			if (transparentBrushDX != null)
				transparentBrushDX.Dispose();
			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
				if(brushesDX[seriesCount] != null)
					brushesDX[seriesCount].Dispose();

			if (RenderTarget != null)
			{
				try
				{
					pivotRangeBrushDX 	= pivotRangeBrush.ToDxBrush(RenderTarget);
					centralRangeBrushDX = centralRangeBrush.ToDxBrush(RenderTarget);
					jacksonZonesBrushDX = jacksonZonesBrush.ToDxBrush(RenderTarget);
					transparentBrushDX = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Transparent);
					for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
						brushesDX[seriesCount] 	= Plots[seriesCount].BrushDX;
				}
				catch (Exception e) { }
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (ChartBars == null || errorMessage || !IsVisible) return;
			int	lastBarPainted = ChartBars.ToIndex;
			if(lastBarPainted  < 0 || BarsArray[0].Count < lastBarPainted) return;

			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
			TextFormat textFormat1 = new TextFormat(Globals.DirectWriteFactory, "Arial", (float)this.TextFontSize);		
			
			int lastBarCounted				= Inputs[0].Count - 1;
			int	lastBarOnUpdate				= lastBarCounted - (Calculate == Calculate.OnBarClose ? 1 : 0);
			int	lastBarIndex				= Math.Min(lastBarPainted, lastBarOnUpdate);
			int firstBarPainted	 			= ChartBars.FromIndex;
			int firstBarIndex  	 			= Math.Max(BarsRequiredToPlot, firstBarPainted);
			int firstBarIdxToPaint  		= 0;
			int lastPlotIndex				= lastBarPainted;
			int lastPlotCalcIndex			= lastBarIndex;
			int firstPlotIndex				= 0;
			int	resolution					= 1 + this.TextFontSize/2;
			double barWidth					= chartControl.GetBarPaintWidth(ChartBars);
			double smallGap					= 2.0;
			double labelOffset				= (this.TextFontSize + barWidth)/2.0 + shiftLabelOffset;
			double firstX					= 0.0;
			double lastX					= 0.0;
			double val						= 0.0;
			double valC						= 0.0;
			double valD						= 0.0;
			double valH						= 0.0;
			double valL						= 0.0;
			double y						= 0.0;
			double yC						= 0.0;
			double yD						= 0.0;
			double yH						= 0.0;
			double yL						= 0.0;
			double[] yArr					= new double[Values.Length];
			float width						= 0.0f;
			float height					= 0.0f;
			float textFontSize				= textFormat1.FontSize;
			string[] plotLabels				= new string[Values.Length];
			bool firstLoop					= true;
			DateTime lastBarPaintedTime		= ChartBars.Bars.GetTime(lastBarPainted);
			DateTime lastBarIndexTime		= ChartBars.Bars.GetTime(lastBarIndex);
			DateTime tradingDayEndPainted;
			DateTime tradingDayEndByIndex;
			SharpDX.Vector2 startPointDX;
			SharpDX.Vector2 endPointDX;
			SharpDX.Vector2 textPointDX;
			SharpDX.RectangleF rect;
			
			if(lastBarPainted != CacheLastBarPainted)
			{
				sessionIterator0.GetNextSession(lastBarPaintedTime, timeBased0);
				tradingDayEndPainted = sessionIterator0.ActualTradingDayEndLocal;
				sessionIterator0.GetNextSession(lastBarIndexTime, timeBased0);
				tradingDayEndByIndex = sessionIterator0.ActualTradingDayEndLocal;
				CacheLastBarPainted = lastBarPainted;
				if(tradingDayEndByIndex < tradingDayEndPainted)
					LastBarStartsNewPeriod = true;
				else
					LastBarStartsNewPeriod = false;
			}
			if(LastBarStartsNewPeriod)
				lastPlotIndex = lastBarIndex;
			
			if(!Values[0].IsValidDataPointAt(lastPlotCalcIndex))
			{
				if(!errorMessage)
				{	
					SharpDX.Direct2D1.Brush errorBrushDX = errorBrush.ToDxBrush(RenderTarget);
					TextFormat textFormat2 = new TextFormat(Globals.DirectWriteFactory, "Arial", 20.0f);		
					SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory, errorText8, textFormat2, 1000, 20.0f);
					SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.W - textLayout.Metrics.Width - 5, ChartPanel.Y + (ChartPanel.H - textLayout.Metrics.Height));
					RenderTarget.DrawTextLayout(lowerTextPoint, textLayout, errorBrushDX, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					errorBrushDX.Dispose();
					textFormat2.Dispose();
					textLayout.Dispose();
				}	
				RenderTarget.AntialiasMode = oldAntialiasMode;
				textFormat1.Dispose();
				return;
			}
			
			do
			{
				//check whether lastBarIndex contains a pivot value
				for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
				{
					int prevSessionBreakIdx = newSessionBarIdxArr[i];
					if (prevSessionBreakIdx <= lastPlotIndex)
					{
						firstBarIdxToPaint = prevSessionBreakIdx;
						break;
					}
				}
				firstPlotIndex = Math.Max(firstBarIndex, firstBarIdxToPaint);
				if(!Values[0].IsValidDataPointAt(lastPlotCalcIndex))
					break;
				if(firstPlotIndex > firstBarPainted)
					firstX	= chartControl.GetXByBarIndex(ChartBars, firstPlotIndex - 1) + smallGap;
				else
					firstX	= smallGap;
				lastX	= chartControl.GetXByBarIndex(ChartBars, lastPlotIndex);
				
				for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
				{
					val						= Values[seriesCount].GetValueAt(lastPlotCalcIndex);
					yArr[seriesCount] 		= chartScale.GetYByValue(val);
					plotLabels[seriesCount] = Plots[seriesCount].Name;
					brushesDX[seriesCount] 	= Plots[seriesCount].BrushDX;
				}
				
				// adjust labels in case that two pivots have near-identical prices
				if(ShowMidpivots && pivotFormula != amaPivotFormulaPD.Jackson_Zones && pivotFormula != amaPivotFormulaPD.Fibonacci_Pivots)
				{	
					for (int i = 7; i < 9; i++)
					{
						if(ShowPriorHLC && Math.Abs(yArr[2*i] - yArr[13]) < resolution)
						{
							plotLabels[13] = plotLabels[13] + "/" + plotLabels[2*i]; 
							plotLabels[2*i] = "";
							brushesDX[2*i] = transparentBrushDX;
						}
						else if(ShowPriorHLC && Math.Abs(yArr[2*i] - yArr[11]) < resolution)
						{
							plotLabels[11] = plotLabels[11] + "/" + plotLabels[2*i]; 
							plotLabels[2*i] = "";
							brushesDX[2*i] = transparentBrushDX;
						}
						else if (ShowDP && Math.Abs(yArr[2*i] - yArr[2]) < resolution)
						{
							plotLabels[2] = plotLabels[2] + "/" + plotLabels[2*i]; 
							plotLabels[2*i] = "";
							brushesDX[2*i] = transparentBrushDX;
						}	
						else if (ShowCP && Math.Abs(yArr[2*i] - yArr[1]) < resolution)
						{
							plotLabels[1] = plotLabels[1] + "/" + plotLabels[2*i]; 
							plotLabels[2*i] = "";
							brushesDX[2*i] = transparentBrushDX;
						}
					}	
					for (int i = 7; i < 9; i++)
					{
						if(ShowPriorHLC && Math.Abs(yArr[2*i+1] - yArr[13]) < resolution)
						{
							plotLabels[13] = plotLabels[13] + "/" + plotLabels[2*i+1]; 
							plotLabels[2*i+1] = "";
							brushesDX[2*i+1] = transparentBrushDX;
						}
						else if(ShowPriorHLC && Math.Abs(yArr[2*i+1] - yArr[12]) < resolution)
						{
							plotLabels[12] = plotLabels[12] + "/" + plotLabels[2*i+1]; 
							plotLabels[2*i+1] = "";
							brushesDX[2*i+1] = transparentBrushDX;
						}
						else if (ShowDP && Math.Abs(yArr[2*i+1] - yArr[2]) < resolution)
						{
							plotLabels[2] = plotLabels[2] + "/" + plotLabels[2*i+1]; 
							plotLabels[2*i+1] = "";
							brushesDX[2*i+1] = transparentBrushDX;
						}	
						else if (ShowCP && Math.Abs(yArr[2*i+1] - yArr[1]) < resolution)
						{
							plotLabels[1] = plotLabels[1] + "/" + plotLabels[2*i+1]; 
							plotLabels[2*i+1] = "";
							brushesDX[2*i+1] = transparentBrushDX;
						}
					}
				}
				
				if(ShowPriorHLC)
				{	
					if (Math.Abs(yArr[13] - yArr[11]) < resolution)
					{
						plotLabels[11] = plotLabels[11] + "/" + plotLabels[13]; 
						plotLabels[13] = "";
						brushesDX[13] = transparentBrushDX;
					}
					else if (Math.Abs(yArr[13] - yArr[12]) < resolution)
					{
						plotLabels[12] = plotLabels[12] + "/" + plotLabels[13]; 
						plotLabels[13] = "";
						brushesDX[13] = transparentBrushDX;
					}
					else if (ShowDP && Math.Abs(yArr[13] - yArr[2]) < resolution)
					{
						plotLabels[2] = plotLabels[2] + "/" + plotLabels[13]; 
						brushesDX[2] = brushesDX[13];
						plotLabels[13] = "";
						brushesDX[13] = transparentBrushDX;
					}
					else if (ShowCP && Math.Abs(yArr[13] - yArr[1]) < resolution)
					{
						plotLabels[1] = plotLabels[1] + "/" + plotLabels[13]; 
						brushesDX[1] = brushesDX[13];
						plotLabels[13] = "";
						brushesDX[13] = transparentBrushDX;
					}
					else if (ShowPP && Math.Abs(yArr[13] - yArr[0]) < resolution)
					{
						plotLabels[0] = plotLabels[0] + "/" + plotLabels[13]; 
						brushesDX[0] = brushesDX[13];
						plotLabels[13] = "";
						brushesDX[13] = transparentBrushDX;
					}
					if(ShowPivots)
					{	
						if (Math.Abs(yArr[11] - yArr[3]) < resolution)
						{
							plotLabels[3] = plotLabels[3] + "/" + plotLabels[11]; 
							brushesDX[3] = brushesDX[11];
							plotLabels[11] = "";
							brushesDX[11] = transparentBrushDX;
						}	
						if (Math.Abs(yArr[12] - yArr[4]) < resolution)
						{
							plotLabels[4] = plotLabels[4] + "/" + plotLabels[12]; 
							brushesDX[4] = brushesDX[12];
							plotLabels[12] = "";
							brushesDX[12] = transparentBrushDX;
						}	
					}
				}
				
				if (ShowPP && ShowCP && ShowDP && Math.Min(Math.Abs(yArr[1] - yArr[0]), Math.Abs(yArr[2] - yArr[0])) < resolution)
				{
					plotLabels[0] = plotLabels[0] + "/" + plotLabels[1] + "/" + plotLabels[2]; 
					brushesDX[0] = brushesDX[2];
					plotLabels[1] = "";
					plotLabels[2] = "";
					brushesDX[1] = transparentBrushDX;
					brushesDX[2] = transparentBrushDX;
				}
				else if (ShowPP)
				{	
					if (ShowCP && Math.Abs(yArr[1] - yArr[0]) < resolution)
					{
						plotLabels[0] = plotLabels[0] + "/" + plotLabels[1]; 
						brushesDX[0] = brushesDX[1];
						plotLabels[1] = "";
						brushesDX[1] = transparentBrushDX;
					}
					else if (ShowDP && Math.Abs(yArr[2] - yArr[0]) < resolution)
					{
						plotLabels[0] = plotLabels[0] + "/" + plotLabels[2]; 
						brushesDX[0] = brushesDX[2];
						plotLabels[2] = "";
						brushesDX[2] = transparentBrushDX;
					}
				}
				else if (ShowCP && ShowDP && Math.Abs(yArr[2] - yArr[1]) < resolution)
				{
					plotLabels[1] = plotLabels[1] + "/" + plotLabels[2]; 
					brushesDX[1] = brushesDX[2];
					plotLabels[2] = "";
					brushesDX[2] = transparentBrushDX;
				}
				
				//Draw pivot range
				if(ShowPivotRange)
				{	
					val = Values[0].GetValueAt(lastPlotCalcIndex);
					valC = Values[1].GetValueAt(lastPlotCalcIndex);
					valD = Values[2].GetValueAt(lastPlotCalcIndex);
					y = chartScale.GetYByValue(val);
					yC = chartScale.GetYByValue(valC);
					yD = chartScale.GetYByValue(valD);
					
					if(y < yC)
					{
						width = (float)(lastX - firstX);
						height = (float)(yC - y);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)y);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, centralRangeBrushDX);
					}
					else if (y > yC)
					{
						width = (float)(lastX - firstX);
						height = (float)(y - yC);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yC);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, centralRangeBrushDX);
					}
					else
					{
						width = (float)(lastX - firstX);
						height = 10.0f;
						startPointDX = new SharpDX.Vector2((float)firstX, (float)y);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y -5.0f, width, height);
						RenderTarget.FillRectangle(rect, centralRangeBrushDX);
					}
					
					if(y < yD)
					{
						width = (float)(lastX - firstX);
						height = (float)(yD - y);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)y);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, pivotRangeBrushDX);
					}
					else if (y > yD)
					{
						width = (float)(lastX - firstX);
						height = (float)(y - yD);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yD);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, pivotRangeBrushDX);
					}
				}
				
				//Draw Jackson Zones
				if(ShowJacksonZones && pivotFormula == amaPivotFormulaPD.Jackson_Zones)
				{	
					valH = Values[3].GetValueAt(lastPlotCalcIndex);
					valL = Values[5].GetValueAt(lastPlotCalcIndex);
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					if(yL < yH)
					{
						width = (float)(lastX - firstX);
						height = (float)(yH - yL);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
					else
					{
						width = (float)(lastX - firstX);
						height = 10.0f;
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y -5.0f, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
					
					valH = Values[6].GetValueAt(lastPlotCalcIndex);
					valL = Values[4].GetValueAt(lastPlotCalcIndex);
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					if(yL < yH)
					{
						width = (float)(lastX - firstX);
						height = (float)(yH - yL);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
					else
					{
						width = (float)(lastX - firstX);
						height = 10.0f;
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y -5.0f, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
					
					valH = Values[7].GetValueAt(lastPlotCalcIndex);
					valL = Values[9].GetValueAt(lastPlotCalcIndex);
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					if(yL < yH)
					{
						width = (float)(lastX - firstX);
						height = (float)(yH - yL);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
					else
					{
						width = (float)(lastX - firstX);
						height = 10.0f;
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y -5.0f, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
					
					valH = Values[10].GetValueAt(lastPlotCalcIndex);
					valL = Values[8].GetValueAt(lastPlotCalcIndex);
					yH = chartScale.GetYByValue(valH);
					yL = chartScale.GetYByValue(valL);
					
					if(yL < yH)
					{
						width = (float)(lastX - firstX);
						height = (float)(yH - yL);
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
					else
					{
						width = (float)(lastX - firstX);
						height = 10.0f;
						startPointDX = new SharpDX.Vector2((float)firstX, (float)yL);
						rect = new SharpDX.RectangleF(startPointDX.X, startPointDX.Y -5.0f, width, height);
						RenderTarget.FillRectangle(rect, jacksonZonesBrushDX);
					}
				}
				
				// Loop through all plot values on the chart
				for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
				{
					Plot	plot = Plots[seriesCount];
					y = yArr[seriesCount];
					
					// Draw pivot lines
					startPointDX = new SharpDX.Vector2((float)firstX, (float)y);
					endPointDX = new SharpDX.Vector2((float)lastX, (float)y);
					RenderTarget.DrawLine(startPointDX, endPointDX, plot.BrushDX, plot.Width, plot.StrokeStyle);
					
					// Draw pivot text
					if(ShowLabels && firstLoop)
					{	
							textPointDX = new SharpDX.Vector2((float)(lastX + labelOffset), (float)(y - textFontSize/2));
							TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, plotLabels[seriesCount], textFormat1, Math.Max(150, ChartControl.Properties.BarMarginRight - (float)labelOffset - 20), textFontSize);
							RenderTarget.DrawTextLayout(textPointDX, textLayout, brushesDX[seriesCount]);
							textLayout.Dispose();
					}	
				}
				
				if(lastPlotIndex < firstPlotIndex)
					lastPlotIndex = 0;
				else
					lastPlotIndex = firstPlotIndex - 1;
				lastPlotCalcIndex = lastPlotIndex;
				firstLoop = false;
			}
			while (lastPlotIndex >= firstBarIndex);
				
			RenderTarget.AntialiasMode = oldAntialiasMode;
			textFormat1.Dispose();
			base.OnRender(chartControl, chartScale);
		}
		#endregion
	}
}

namespace NinjaTrader.NinjaScript.Indicators
{
	public class amaPivotsDailyTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			amaPivotsDailyBak		thisPivotsInstance					= (amaPivotsDaily) value;
			amaPivotFormulaPD	pivotFormulaFromInstance			= thisPivotsInstance.PivotFormula;
			amaSessionTypePD	sessionTypeFromInstance				= thisPivotsInstance.SessionType;
			bool				showPPFromInstance					= thisPivotsInstance.ShowPP;
			bool				showCPFromInstance					= thisPivotsInstance.ShowCP;
			bool				showDPFromInstance					= thisPivotsInstance.ShowDP;
			bool				showPriorHLCFromInstance			= thisPivotsInstance.ShowPriorHLC;
			bool				showPivotsFromInstance				= thisPivotsInstance.ShowPivots;
			bool				showMidpivotsFromInstance			= thisPivotsInstance.ShowMidpivots;
			bool				showPivotRangeFromInstance			= thisPivotsInstance.ShowPivotRange;
			bool				showJacksonZonesFromInstance		= thisPivotsInstance.ShowJacksonZones;
			bool				showLabelsFromInstance				= thisPivotsInstance.ShowLabels;
			
			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				if ((pivotFormulaFromInstance == amaPivotFormulaPD.Jackson_Zones || pivotFormulaFromInstance == amaPivotFormulaPD.Fibonacci_Pivots) && 
					(thisDescriptor.Name == "ShowMidpivots" || thisDescriptor.Name == "ResistanceMidBrush" || thisDescriptor.Name == "SupportMidBrush"
					|| thisDescriptor.Name == "Dash2Style" || thisDescriptor.Name == "Plot2Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (pivotFormulaFromInstance != amaPivotFormulaPD.Jackson_Zones && (thisDescriptor.Name == "ShowJacksonZones" ||
					thisDescriptor.Name == "JacksonZonesBrushS" || thisDescriptor.Name == "JacksonZonesOpacity"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (sessionTypeFromInstance == amaSessionTypePD.Daily_Bars && thisDescriptor.Name == "CalcMode")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPPFromInstance && thisDescriptor.Name == "PivotBrush")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showCPFromInstance && thisDescriptor.Name == "CentralPivotBrush")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showDPFromInstance && thisDescriptor.Name == "DirectionalPivotBrush")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPriorHLCFromInstance && (thisDescriptor.Name == "PriorHighBrush" || thisDescriptor.Name == "PriorLowBrush" 
					|| thisDescriptor.Name == "PriorCloseBrush" || thisDescriptor.Name == "Dash1Style" || thisDescriptor.Name == "Plot1Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPivotsFromInstance && (thisDescriptor.Name == "ResistanceBrush" || thisDescriptor.Name == "SupportBrush"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showMidpivotsFromInstance && (thisDescriptor.Name == "ResistanceMidBrush" || thisDescriptor.Name == "SupportMidBrush" 
					|| thisDescriptor.Name == "Dash2Style" || thisDescriptor.Name == "Plot2Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPPFromInstance && !showCPFromInstance && !showDPFromInstance && !showPivotsFromInstance && (thisDescriptor.Name == "Dash0Style" || thisDescriptor.Name == "Plot0Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPivotRangeFromInstance && (thisDescriptor.Name == "PivotRangeBrushS" || thisDescriptor.Name == "PivotRangeOpacity"
					|| thisDescriptor.Name == "CentralRangeBrushS" || thisDescriptor.Name == "CentralRangeOpacity" ))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showJacksonZonesFromInstance && (thisDescriptor.Name == "JacksonZonesBrushS" || thisDescriptor.Name == "JacksonZonesOpacity"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showLabelsFromInstance && (thisDescriptor.Name == "TextFontSize" || thisDescriptor.Name == "ShiftLabelOffset"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else	
					adjusted.Add(thisDescriptor);
			}
			return adjusted;		
		}
	}
}

#region Public Enums
public enum amaPivotFormulaPD 
{
	Floor_Trader_Pivots, 
	Wide_Pivots,
	Jackson_Zones,
	Fibonacci_Pivots
}

public enum amaSessionTypePD 
{
	Trading_Hours, 
	Daily_Bars
}

public enum amaCalcModePD 
{
	Intraday_Data,
	Daily_Data
}
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaPivotsDailyBak[] cacheamaPivotsDailyBak;
		public LizardIndicators.amaPivotsDailyBak amaPivotsDailyBak(amaPivotFormulaPD pivotFormula, amaSessionTypePD sessionType, amaCalcModePD calcMode)
		{
			return amaPivotsDailyBak(Input, pivotFormula, sessionType, calcMode);
		}

		public LizardIndicators.amaPivotsDailyBak amaPivotsDailyBak(ISeries<double> input, amaPivotFormulaPD pivotFormula, amaSessionTypePD sessionType, amaCalcModePD calcMode)
		{
			if (cacheamaPivotsDailyBak != null)
				for (int idx = 0; idx < cacheamaPivotsDailyBak.Length; idx++)
					if (cacheamaPivotsDailyBak[idx] != null && cacheamaPivotsDailyBak[idx].PivotFormula == pivotFormula && cacheamaPivotsDailyBak[idx].SessionType == sessionType && cacheamaPivotsDailyBak[idx].CalcMode == calcMode && cacheamaPivotsDailyBak[idx].EqualsInput(input))
						return cacheamaPivotsDailyBak[idx];
			return CacheIndicator<LizardIndicators.amaPivotsDailyBak>(new LizardIndicators.amaPivotsDailyBak(){ PivotFormula = pivotFormula, SessionType = sessionType, CalcMode = calcMode }, input, ref cacheamaPivotsDailyBak);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaPivotsDailyBak amaPivotsDailyBak(amaPivotFormulaPD pivotFormula, amaSessionTypePD sessionType, amaCalcModePD calcMode)
		{
			return indicator.amaPivotsDailyBak(Input, pivotFormula, sessionType, calcMode);
		}

		public Indicators.LizardIndicators.amaPivotsDailyBak amaPivotsDailyBak(ISeries<double> input , amaPivotFormulaPD pivotFormula, amaSessionTypePD sessionType, amaCalcModePD calcMode)
		{
			return indicator.amaPivotsDailyBak(input, pivotFormula, sessionType, calcMode);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaPivotsDailyBak amaPivotsDailyBak(amaPivotFormulaPD pivotFormula, amaSessionTypePD sessionType, amaCalcModePD calcMode)
		{
			return indicator.amaPivotsDailyBak(Input, pivotFormula, sessionType, calcMode);
		}

		public Indicators.LizardIndicators.amaPivotsDailyBak amaPivotsDailyBak(ISeries<double> input , amaPivotFormulaPD pivotFormula, amaSessionTypePD sessionType, amaCalcModePD calcMode)
		{
			return indicator.amaPivotsDailyBak(input, pivotFormula, sessionType, calcMode);
		}
	}
}

#endregion
