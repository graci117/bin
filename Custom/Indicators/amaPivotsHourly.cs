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
	/// The Hourly Pivots indicator displays floor pivots, wide pivots or Fibonacci pivots as calculated from selected minute bars.
	/// </summary>
	/// 
	[Gui.CategoryOrder("Algorithmic Options", 0)]
	[Gui.CategoryOrder("Input Parameters", 10)]
	[Gui.CategoryOrder("Display Options", 20)]
	[Gui.CategoryOrder("Data Series", 30)]
	[Gui.CategoryOrder("Set up", 40)]
	[Gui.CategoryOrder("Visual", 50)]
	[Gui.CategoryOrder("Plot Colors", 60)]
	[Gui.CategoryOrder("Plot Parameters", 70)]
	[Gui.CategoryOrder("Shaded Areas", 80)]
	[Gui.CategoryOrder("Version", 90)]
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.amaPivotsHourlyTypeConverter")]
	public class amaPivotsHourly : Indicator
	{
		private DateTime							sessionBegin					= Globals.MinDate;
		private DateTime							sessionEnd						= Globals.MinDate;
		private DateTime							nextBarTime						= Globals.MinDate;
		private DateTime							currentTime						= Globals.MinDate;
		private TimeSpan							interval						= new TimeSpan(0,60,0);
		private double								currentHigh						= 0.0;
		private double								currentLow						= 0.0;
		private double								currentClose					= 0.0;
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
		private	double								cp								= 0.0;
		private	double								dp								= 0.0;
		private double								displaySize						= 0.0;
		private int									barPeriod						= 60;
		private int									displacement					= 0;
		private bool								showPP							= true;
		private bool								showCP							= true;
		private bool								showDP							= true;
		private bool								showPivots						= true;
		private bool								showPriorHLC					= true;
		private bool								showPivotRange					= true;
		private bool								showLabels						= true;
		private bool								isIntraday0						= true;
		private bool								calcOpen						= false;
		private bool								timeBased0						= false;
		private bool								breakAtEOD						= true;
		private bool								errorMessage					= true;
		private bool								firstLoop						= false;
		private bool								calculateFromPriceData			= true;
		private amaPivotFormulaPH					pivotFormula					= amaPivotFormulaPH.Floor_Trader_Pivots;
		private readonly List<int>					newSessionBarIdxArr				= new List<int>();
		private SessionIterator						sessionIterator0				= null;
		private System.Windows.Media.Brush			pivotBrush						= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			centralPivotBrush				= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			directionalPivotBrush			= Brushes.MidnightBlue;
		private System.Windows.Media.Brush  		resistanceBrush					= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			supportBrush					= Brushes.MidnightBlue;
		private System.Windows.Media.Brush			priorHighBrush					= Brushes.DarkGreen;
		private System.Windows.Media.Brush			priorLowBrush 					= Brushes.Firebrick;
		private System.Windows.Media.Brush			priorCloseBrush 				= Brushes.DarkOrange;
		private System.Windows.Media.Brush			pivotRangeBrushS				= Brushes.SandyBrown;
		private System.Windows.Media.Brush			centralRangeBrushS				= Brushes.LightSlateGray;
		private System.Windows.Media.Brush			pivotRangeBrush					= null;
		private System.Windows.Media.Brush			centralRangeBrush				= null;
		private System.Windows.Media.Brush			errorBrush						= Brushes.Black;
		private SharpDX.Direct2D1.Brush 			pivotRangeBrushDX 				= null;
		private	SharpDX.Direct2D1.Brush 			centralRangeBrushDX 			= null;
		private	SharpDX.Direct2D1.SolidColorBrush 	transparentBrushDX				= null;
		private	SharpDX.Direct2D1.Brush[] 			brushesDX						= null;
		private SimpleFont							errorFont						= null;
		private string								errorText1						= "The amaPivotsHourly indicator only works on price data.";
		private string								errorText2						= "The amaPivotsHourly indicator can only be used on intra-day charts.";
		private string								errorText3						= "The amaPivotsHourly indicator cannot be used with a displacement.";
		private int									pivotRangeOpacity				= 60;
		private int									centralRangeOpacity				= 60;
		private int									plot0Width						= 2;
		private int									plot1Width						= 3;
		private int									textFontSize					= 14;
		private int									shiftLabelOffset				= 10;
		private PlotStyle							plot0Style						= PlotStyle.Line;
		private DashStyleHelper						dash0Style						= DashStyleHelper.Solid;
		private PlotStyle							plot1Style						= PlotStyle.Line;
		private DashStyleHelper						dash1Style						= DashStyleHelper.Dot;
		private string								versionString					= "v 1.0  -  January 14, 2018";
	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Hourly Pivots indicator displays floor pivots, wide pivots or Fibonacci pivots as calculated from selected minute bars.";
				Name						= "amaPivotsHourly";
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				IsAutoScale					= false;
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
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"P-High");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"P-Low");
				AddPlot(new Stroke(Brushes.Gray,1), PlotStyle.Hash,"P-Close");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Minute, barPeriod);
				BarsRequiredToPlot = 3;
				displacement = Displacement;
				pivotRangeBrush = pivotRangeBrushS.Clone();
				pivotRangeBrush.Opacity = (float) pivotRangeOpacity/100.0;
				pivotRangeBrush.Freeze();
				centralRangeBrush = centralRangeBrushS.Clone();
				centralRangeBrush.Opacity = (float) centralRangeOpacity/100.0;
				centralRangeBrush.Freeze();
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
				brushesDX = new SharpDX.Direct2D1.Brush[Values.Length];
			}
		  	else if (State == State.DataLoaded)
		 	{
				if(BarsArray[0].BarsType.IsIntraday)
					isIntraday0 = true;
				else
					isIntraday0 = false;
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
		    	sessionIterator0 = new SessionIterator(BarsArray[0]);
		  	}
			else if (State == State.Historical)
			{
				interval = new TimeSpan(barPeriod/60, barPeriod%60, 0);
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
				}	
				else if(!isIntraday0)
				{
					Draw.TextFixed(this, "error text 2", errorText2, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
				}
				else if(displacement != 0)
				{
					Draw.TextFixed(this, "error text 3", errorText3, TextPosition.Center, errorBrush, errorFont, Brushes.Transparent, Brushes.Transparent, 0);  
					errorMessage = true;
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
						return;
				}
				
				if(CurrentBars[0] == 0)
					currentTime = Times[0][0];
				
				if(IsFirstTickOfBar && BarsArray[0].IsFirstBarOfSession)
				{
					sessionIterator0.CalculateTradingDay(Times[0][0], timeBased0);
					sessionBegin = sessionIterator0.ActualSessionBegin;
					sessionEnd = sessionIterator0.ActualSessionEnd;
					nextBarTime = sessionBegin.Add(interval);
					newSessionBarIdxArr.Add(CurrentBars[0]);
					priorHigh = currentHigh;
					priorLow = currentLow;
					priorClose = currentClose;
				}
				else
				{	
					firstLoop = true;
					while (Times[0][0] > nextBarTime)
					{	
						nextBarTime = nextBarTime.Add(interval);
						if (firstLoop) 
						{	
							if (newSessionBarIdxArr.Count == 0 || (newSessionBarIdxArr.Count > 0 && CurrentBars[0] > (int) newSessionBarIdxArr[newSessionBarIdxArr.Count - 1]))
								newSessionBarIdxArr.Add(CurrentBars[0]);
							priorHigh = currentHigh;
							priorLow = currentLow;
							priorClose = currentClose;
						}	
						firstLoop = false;
					}
				}
				
				if(Times[0][0] > currentTime)
				{	
					currentTime = Times[0][0];
					pp				= (priorHigh + priorLow + priorClose)/3.0;
					cp				= (priorHigh + priorLow) / 2.0;
					priorRange 		= priorHigh - priorLow;						
					upside  		= pp - priorLow;
					downside		= priorHigh - pp;
					if(pivotFormula == amaPivotFormulaPH.Floor_Trader_Pivots)
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
					else if (pivotFormula == amaPivotFormulaPH.Wide_Pivots)
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
					else if (pivotFormula == amaPivotFormulaPH.Fibonacci_Pivots)
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
				}
			}
			
			if(BarsInProgress == 1)
			{
				currentHigh		= Highs[1][0];
				currentLow	 	= Lows[1][0];
				currentClose	= Closes[1][0];
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
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Pivot formula", Description = "Select formula for calculating the pivots", GroupName = "Algorithmic Options", Order = 0)]
 		[RefreshProperties(RefreshProperties.All)] 
		public amaPivotFormulaPH PivotFormula
		{	
            get { return pivotFormula; }
            set { pivotFormula = value; }
		}
			
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Bar period", Description = "Select bar period in minutes for calculating intraday pivots", GroupName = "Input Parameters", Order = 0)]
		public int BarPeriod
		{	
            get { return barPeriod; }
            set { barPeriod = value; }
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

		public override void OnRenderTargetChanged()
		{
			if (pivotRangeBrushDX != null)
				pivotRangeBrushDX.Dispose();
			if (centralRangeBrushDX != null)
				centralRangeBrushDX.Dispose();
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
			SharpDX.Vector2 startPointDX;
			SharpDX.Vector2 endPointDX;
			SharpDX.Vector2 textPointDX;
			SharpDX.RectangleF rect;
			
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
		}
		#endregion
	}
}

namespace NinjaTrader.NinjaScript.Indicators
{
	public class amaPivotsHourlyTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			amaPivotsHourly		thisPivotsInstance					= (amaPivotsHourly) value;
			bool				showPPFromInstance					= thisPivotsInstance.ShowPP;
			bool				showCPFromInstance					= thisPivotsInstance.ShowCP;
			bool				showDPFromInstance					= thisPivotsInstance.ShowDP;
			bool				showPriorHLCFromInstance			= thisPivotsInstance.ShowPriorHLC;
			bool				showPivotsFromInstance				= thisPivotsInstance.ShowPivots;
			bool				showPivotRangeFromInstance			= thisPivotsInstance.ShowPivotRange;
			bool				showLabelsFromInstance				= thisPivotsInstance.ShowLabels;
			
			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				if (!showPPFromInstance && thisDescriptor.Name == "PivotBrush")
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
				else if (!showPPFromInstance && !showCPFromInstance && !showDPFromInstance && !showPivotsFromInstance && (thisDescriptor.Name == "Dash0Style" || thisDescriptor.Name == "Plot0Width"))
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else if (!showPivotRangeFromInstance && (thisDescriptor.Name == "PivotRangeBrushS" || thisDescriptor.Name == "PivotRangeOpacity"
					|| thisDescriptor.Name == "CentralRangeBrushS" || thisDescriptor.Name == "CentralRangeOpacity" ))
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
public enum amaPivotFormulaPH 
{
	Floor_Trader_Pivots, 
	Wide_Pivots,
	Fibonacci_Pivots,
}
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaPivotsHourly[] cacheamaPivotsHourly;
		public LizardIndicators.amaPivotsHourly amaPivotsHourly(amaPivotFormulaPH pivotFormula, int barPeriod)
		{
			return amaPivotsHourly(Input, pivotFormula, barPeriod);
		}

		public LizardIndicators.amaPivotsHourly amaPivotsHourly(ISeries<double> input, amaPivotFormulaPH pivotFormula, int barPeriod)
		{
			if (cacheamaPivotsHourly != null)
				for (int idx = 0; idx < cacheamaPivotsHourly.Length; idx++)
					if (cacheamaPivotsHourly[idx] != null && cacheamaPivotsHourly[idx].PivotFormula == pivotFormula && cacheamaPivotsHourly[idx].BarPeriod == barPeriod && cacheamaPivotsHourly[idx].EqualsInput(input))
						return cacheamaPivotsHourly[idx];
			return CacheIndicator<LizardIndicators.amaPivotsHourly>(new LizardIndicators.amaPivotsHourly(){ PivotFormula = pivotFormula, BarPeriod = barPeriod }, input, ref cacheamaPivotsHourly);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaPivotsHourly amaPivotsHourly(amaPivotFormulaPH pivotFormula, int barPeriod)
		{
			return indicator.amaPivotsHourly(Input, pivotFormula, barPeriod);
		}

		public Indicators.LizardIndicators.amaPivotsHourly amaPivotsHourly(ISeries<double> input , amaPivotFormulaPH pivotFormula, int barPeriod)
		{
			return indicator.amaPivotsHourly(input, pivotFormula, barPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaPivotsHourly amaPivotsHourly(amaPivotFormulaPH pivotFormula, int barPeriod)
		{
			return indicator.amaPivotsHourly(Input, pivotFormula, barPeriod);
		}

		public Indicators.LizardIndicators.amaPivotsHourly amaPivotsHourly(ISeries<double> input , amaPivotFormulaPH pivotFormula, int barPeriod)
		{
			return indicator.amaPivotsHourly(input, pivotFormula, barPeriod);
		}
	}
}

#endregion
