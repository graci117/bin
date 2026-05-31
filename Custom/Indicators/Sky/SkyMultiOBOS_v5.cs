#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;	//Sky
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
namespace NinjaTrader.NinjaScript.Indicators.Sky
{
	
	public class SkyMultiOBOS_v5 : Indicator
	{

		private MFI MFI1;
		private RSI RSI1;
		private Stochastics Stochastics1;

		private double sumC = 0.0;	//TMA Bands
		private double sumW = 0.0;	//TMA Bands
		private double rngV = 0.0;	//TMA Bands
		
		private Brush bullBrush;
        private Brush bearBrush;
		private Brush longBrush;
		private Brush shortBrush;
		private Brush SymbolSBrush;
		private Brush SymbolLBrush;
		
		private string drawLong = "֍";	// Customize Long Symbol
		private string drawShort = "֍";	// Customize Short Symbol
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Sky Multi OBOS_v5";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
			#region SkyMultiOBOS Periods
				
				MFIPeriod				= 34;
				RSIPeriod				= 10;
				RSISmooth				= 3;
				StochKPeriod			= 29;
				StochDPeriod			= 7;
				StochSmooth				= 3;
				MFIOB					= 62;
				MFIOS					= 23;
				STOCHOB					= 63;
				STOCHOS					= 37;
				RSIOB					= 73;
				RSIOS					= 34;
				
				// Set default colors
				bearColor = Brushes.DodgerBlue;
				bullColor = Brushes.MediumVioletRed;
				longColor = Brushes.DodgerBlue;
				shortColor = Brushes.MediumVioletRed;
				SymbolLColor = Brushes.Aqua;
				SymbolSColor = Brushes.MediumVioletRed;
				
				ShowOBOSZones = true; //Enable & Disable Zones
				showEntryLine = true; //Enable & Disable Entry Line
				showSymbol = true; //Enable & Disable Symbol
				
				SymbolDistance = 11; // Set symbol Distance
				
			#endregion
				
			#region TMA Bands Periods
				
				atrPeriod  = 100;
				atrFactor  = 2.6;
				halfLength = 80;
				
				showBands = true; //Enable & Disable Bands
				
				AddPlot(new Stroke(new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)), 2), PlotStyle.Line, "UpperBand");
				AddPlot(new Stroke(new SolidColorBrush(Color.FromArgb(0, 125, 125, 125)), 1), PlotStyle.Line, "MiddlBand");
				AddPlot(new Stroke(new SolidColorBrush(Color.FromArgb(255, 0, 255, 255)), 2), PlotStyle.Line, "LowerBand");
				
			#endregion
			
			}
			else if (State == State.Configure)
			{
				if(ChartBars != null)
				{
					ZOrder = ChartBars.ZOrder - 1;
				}
			}
			else if (State == State.DataLoaded)
			{				
				MFI1				= MFI(Close, Convert.ToInt32(MFIPeriod));
				RSI1				= RSI(Close, Convert.ToInt32(RSIPeriod), RSISmooth);
				Stochastics1		= Stochastics(Close, Convert.ToInt32(StochDPeriod), Convert.ToInt32(StochKPeriod), Convert.ToInt32(StochSmooth));
			}
		}
		
		protected override void OnBarUpdate()
		{
			
		#region SkyMultiOBOS
			
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			
			// -----------------------------------------------------------------------------------------
			
			if (ShowOBOSZones)
			 // OverBought Zone
			if ((MFI1[0] > MFIOB)
				&& (Stochastics1.D[0] > STOCHOB)
				&& (RSI1.Default[0] > RSIOB))
			{
                bullBrush = Brushes.MediumVioletRed;
				bullBrush = bullColor.Clone();
				bullBrush.Opacity = 0.25;
				bullBrush.Freeze();
				BackBrushAll = bullBrush;
			}
			
			if (ShowOBOSZones)
			 // OverSold Zone
			if ((MFI1[0] < MFIOS)
				&& (Stochastics1.D[0] < STOCHOS)
				&& (RSI1.Default[0] < RSIOS))
			{
                bearBrush = Brushes.DodgerBlue;		
				bearBrush = bearColor.Clone();
				bearBrush.Opacity = 0.25;
				bearBrush.Freeze();
				BackBrushAll = bearBrush;
			}
			
			// -----------------------------------------------------------------------------------------
			
			if (showEntryLine)
			// Sell Line
			if ((MFI1[1] > MFIOB)
				 && (RSI1.Default[1] > RSIOB)
				 && (Stochastics1.D[1] > STOCHOB)
				 && (Close[0] < Close[1]))
			{
				shortBrush = Brushes.MediumVioletRed;
				shortBrush = shortColor.Clone();
				shortBrush.Opacity = 0.9;
				shortBrush.Freeze();
				BackBrushAll = shortBrush;
				
			}
			if (showEntryLine)
			 // Buy Line
			if ((MFI1[1] < MFIOS)
				 && (RSI1.Default[1] < RSIOS)
				 && (Stochastics1.D[1] < STOCHOS)
				 && (Close[0] > Close[1]))
			{
				longBrush = Brushes.DodgerBlue;		
				longBrush = longColor.Clone();
				longBrush.Opacity = 0.9;
				longBrush.Freeze();
				BackBrushAll = longBrush;
			}
			
			// -----------------------------------------------------------------------------------------
			
			if (showSymbol)
			// Symbol Long
			if ((MFI1[1] < MFIOS)
				 && (RSI1.Default[1] < RSIOS)
				 && (Stochastics1.D[1] < STOCHOS)
				 && (Close[0] > Close[1]))
			{
				//Draw.TriangleUp(this, @"AAAAAAAASkyMultiOBOSO Triangle up_1 " + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] + (-6 * TickSize)) , Brushes.Aqua);

				Draw.Text(this, @"Sky Symbol Long " + Convert.ToString(CurrentBars[0]), DrawLong, 0, (Low[0] + (-SymbolDistance * TickSize)), SymbolLColor);

			}
			if (showSymbol)
			// Symbol Short
			if ((MFI1[1] > MFIOB)
				 && (RSI1.Default[1] > RSIOB)
				 && (Stochastics1.D[1] > STOCHOB)
				 && (Close[0] < Close[1]))
			{
				//Draw.TriangleDown(this, @"AAAAAAAASkyMultiOBOSO Triangle down_1 " + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (6 * TickSize)) , Brushes.MediumVioletRed);

				Draw.Text(this, @"Sky Symbol Short " + Convert.ToString(CurrentBars[0]), DrawShort, 0, (High[0] + (SymbolDistance * TickSize)), SymbolSColor);
				
			}
			
		#endregion
		
		#region TMA Bands
			
			if(CurrentBar < halfLength + 1) return;
			if(State != State.Realtime && !IsFirstTickOfBar) return;
			
			if (showBands)
			for(int i=halfLength; i>=0; i--)
      		{
         		sumC = (halfLength+1) * Close[i];
         		sumW = (halfLength+1);
				 
				int k = halfLength;
				
				for(int j=1; j<=halfLength; j++)
				{
					if(i+j > CurrentBar) break;
					
					sumC += k * Close[i+j];
					sumW += k;
					
					if(j<=i)
					{
						sumC += k * Close[i-j];
						sumW += k;
					}
					
					k--;
				}
 				
         		rngV = ATR(atrPeriod)[i] * atrFactor;
				
            	MiddlBand[i] = sumC / sumW;
            	UpperBand[i] = MiddlBand[i] + rngV;
            	LowerBand[i] = MiddlBand[i] - rngV;
				
      		}
			
		#endregion
			
		}

		#region Properties
		
		#region Period Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MFI Period", Order=1, GroupName="MFI Parameters")]
		public int MFIPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI Period", Order=2, GroupName="RSI Parameters")]
		public int RSIPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI Smooth Period", Order=3, GroupName="RSI Parameters")]
		public int RSISmooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stoch D Period", Order=3, GroupName="Stochastic Parameters")]
		public int StochDPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stoch K Period", Order=4, GroupName="Stochastic Parameters")]
		public int StochKPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Stoch Smooth Period", Order=5, GroupName="Stochastic Parameters")]
		public int StochSmooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MFI Over Bought", Order=4, GroupName="MFI Parameters")]
		public int MFIOB
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MFI Over Sold", Order=5, GroupName="MFI Parameters")]
		public int MFIOS
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI Over Bought", Order=6, GroupName="RSI Parameters")]
		public int RSIOB
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSI Over Sold", Order=7, GroupName="RSI Parameters")]
		public int RSIOS
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="STOCH Over Bought", Order=8, GroupName="Stochastic Parameters")]
		public int STOCHOB
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="STOCH Over Sold", Order=9, GroupName="Stochastic Parameters")]
		public int STOCHOS
		{ get; set; }
		#endregion
		
		#region Symbol Properties
		
		// Define the user-defined input parameter (Long Symbol)
		[NinjaScriptProperty]
		[Display(Name="・ Long Signal", Order=1, GroupName="Symbol")]
		public string DrawLong
		{
    		get { return drawLong; }
    		set { drawLong = value; }
		}
		
		// Define the user-defined input parameter (Short Symbol)
		[NinjaScriptProperty]
		[Display(Name="・ Short Signal", Order=2, GroupName="Symbol")]
		public string DrawShort
		{
    		get { return drawShort; }
    		set { drawShort = value; }
		}

		//Symbol Size
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "・ Symbol Distance", Order = 4, GroupName = "Symbol")]
		public int SymbolDistance { get; set; }
		
		//Show Symbol
		[NinjaScriptProperty]
        [Display(Name = "Show Symbol", Order = 3, GroupName = "Symbol")]
        public bool showSymbol { get; set; }
		
		//Symbol Long Color
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "・ Symbol L Color", GroupName = "Symbol", Order = 5)]
		[DefaultValue(typeof(Brush), "DodgerBlue")] // Set default color to DodgerBlue
		public Brush SymbolLColor { get; set; }

		[Browsable(false)]
		public string SymbolLColorSerializable
		{
   			get { return Serialize.BrushToString(SymbolLColor); }
    		set { SymbolLColor = Serialize.StringToBrush(value); }
		}
		
		//Symbol Short Color
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "・ Symbol S Color", GroupName = "Symbol", Order = 6)]
		[DefaultValue(typeof(Brush), "MediumVioletRed")] // Set default color to MediumVioletRed
		public Brush SymbolSColor { get; set; }

		[Browsable(false)]
		public string SymbolSColorSerializable
		{
   			get { return Serialize.BrushToString(SymbolSColor); }
    		set { SymbolSColor = Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		#region Signal Properties
		
		
		
		//Show OBOS Zones
		[NinjaScriptProperty]
        [Display(Name = "Show Zones", Order = 4, GroupName = "Symbol 2")]
        public bool ShowOBOSZones { get; set; }
		
		//Show Entry Lines
		[NinjaScriptProperty]
        [Display(Name = "Show Entry Lines", Order = 7, GroupName = "Symbol 2")]
        public bool showEntryLine { get; set; }
		
		//Over Bought Zone
		[NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="・ OB Zone", Description="OverBought Zone.", Order=6, GroupName="Symbol 2")]
		[DefaultValue(typeof(Brush), "MediumVioletRed")] // Set default color to MediumVioletRed
        public Brush bullColor
        { get; set; }

        [Browsable(false)]
        public string bullColorSerializable
        {
            get { return Serialize.BrushToString(bullColor); }
            set { bullColor = Serialize.StringToBrush(value); }
        }
		
		//Over Sold Zone
		[NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="・ OS Zone", Description="OverSold Zone.", Order=5, GroupName="Symbol 2")]
		[DefaultValue(typeof(Brush), "DodgerBlue")] // Set default color to DodgerBlue
        public Brush bearColor
        { get; set; }

        [Browsable(false)]
        public string bearColorSerializable
        {
            get { return Serialize.BrushToString(bearColor); }
            set { bearColor = Serialize.StringToBrush(value); }
        }
		
		//Line Signal Short
		[NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="・ Short Color", Description="OverSold Zone.", Order=9, GroupName="Symbol 2")]
		[DefaultValue(typeof(Brush), "MediumVioletRed")] // Set default color to MediumVioletRed
        public Brush shortColor
        { get; set; }

        [Browsable(false)]
        public string shortColorSerializable
        {
            get { return Serialize.BrushToString(shortColor); }
            set { shortColor = Serialize.StringToBrush(value); }
        }
		
		//Line Signal Long
		[NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="・ Long Color", Description="OverBought Zone.", Order=8, GroupName="Symbol 2")]
		[DefaultValue(typeof(Brush), "Cyan")] // Set default color to Cyan
        public Brush longColor
        { get; set; }

        [Browsable(false)]
        public string longColorSerializable
        {
            get { return Serialize.BrushToString(longColor); }
            set { longColor = Serialize.StringToBrush(value); }
        }
		#endregion
		
		#region TMA Bands Properties
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperBand
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MiddlBand
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerBand
		{
			get { return Values[2]; }
		}
		
		// ---
		
		[NinjaScriptProperty]
		[Display(Name = "Show Bands", GroupName = "TMA Bands", Order = 0)]
		public bool showBands { get; set; }
		
		// ---
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "ATR Period", GroupName = "TMA Bands", Order = 1)]
		public int atrPeriod
		{ get; set; }
		
		// ---
		
		[Range(0.0, double.MaxValue), NinjaScriptProperty]
		[Display(Name = "Band Deviation", GroupName = "TMA Bands", Order = 2)]
		public double atrFactor
		{ get; set; }
		
		// ---
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "Half Length", GroupName = "TMA Bands", Order = 3)]
		public int halfLength
		{ get; set; }
		
		#endregion
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Sky.SkyMultiOBOS_v5[] cacheSkyMultiOBOS_v5;
		public Sky.SkyMultiOBOS_v5 SkyMultiOBOS_v5(int mFIPeriod, int rSIPeriod, int rSISmooth, int stochDPeriod, int stochKPeriod, int stochSmooth, int mFIOB, int mFIOS, int rSIOB, int rSIOS, int sTOCHOB, int sTOCHOS, string drawLong, string drawShort, int symbolDistance, bool showSymbol, Brush symbolLColor, Brush symbolSColor, bool showOBOSZones, bool showEntryLine, Brush bullColor, Brush bearColor, Brush shortColor, Brush longColor, bool showBands, int atrPeriod, double atrFactor, int halfLength)
		{
			return SkyMultiOBOS_v5(Input, mFIPeriod, rSIPeriod, rSISmooth, stochDPeriod, stochKPeriod, stochSmooth, mFIOB, mFIOS, rSIOB, rSIOS, sTOCHOB, sTOCHOS, drawLong, drawShort, symbolDistance, showSymbol, symbolLColor, symbolSColor, showOBOSZones, showEntryLine, bullColor, bearColor, shortColor, longColor, showBands, atrPeriod, atrFactor, halfLength);
		}

		public Sky.SkyMultiOBOS_v5 SkyMultiOBOS_v5(ISeries<double> input, int mFIPeriod, int rSIPeriod, int rSISmooth, int stochDPeriod, int stochKPeriod, int stochSmooth, int mFIOB, int mFIOS, int rSIOB, int rSIOS, int sTOCHOB, int sTOCHOS, string drawLong, string drawShort, int symbolDistance, bool showSymbol, Brush symbolLColor, Brush symbolSColor, bool showOBOSZones, bool showEntryLine, Brush bullColor, Brush bearColor, Brush shortColor, Brush longColor, bool showBands, int atrPeriod, double atrFactor, int halfLength)
		{
			if (cacheSkyMultiOBOS_v5 != null)
				for (int idx = 0; idx < cacheSkyMultiOBOS_v5.Length; idx++)
					if (cacheSkyMultiOBOS_v5[idx] != null && cacheSkyMultiOBOS_v5[idx].MFIPeriod == mFIPeriod && cacheSkyMultiOBOS_v5[idx].RSIPeriod == rSIPeriod && cacheSkyMultiOBOS_v5[idx].RSISmooth == rSISmooth && cacheSkyMultiOBOS_v5[idx].StochDPeriod == stochDPeriod && cacheSkyMultiOBOS_v5[idx].StochKPeriod == stochKPeriod && cacheSkyMultiOBOS_v5[idx].StochSmooth == stochSmooth && cacheSkyMultiOBOS_v5[idx].MFIOB == mFIOB && cacheSkyMultiOBOS_v5[idx].MFIOS == mFIOS && cacheSkyMultiOBOS_v5[idx].RSIOB == rSIOB && cacheSkyMultiOBOS_v5[idx].RSIOS == rSIOS && cacheSkyMultiOBOS_v5[idx].STOCHOB == sTOCHOB && cacheSkyMultiOBOS_v5[idx].STOCHOS == sTOCHOS && cacheSkyMultiOBOS_v5[idx].DrawLong == drawLong && cacheSkyMultiOBOS_v5[idx].DrawShort == drawShort && cacheSkyMultiOBOS_v5[idx].SymbolDistance == symbolDistance && cacheSkyMultiOBOS_v5[idx].showSymbol == showSymbol && cacheSkyMultiOBOS_v5[idx].SymbolLColor == symbolLColor && cacheSkyMultiOBOS_v5[idx].SymbolSColor == symbolSColor && cacheSkyMultiOBOS_v5[idx].ShowOBOSZones == showOBOSZones && cacheSkyMultiOBOS_v5[idx].showEntryLine == showEntryLine && cacheSkyMultiOBOS_v5[idx].bullColor == bullColor && cacheSkyMultiOBOS_v5[idx].bearColor == bearColor && cacheSkyMultiOBOS_v5[idx].shortColor == shortColor && cacheSkyMultiOBOS_v5[idx].longColor == longColor && cacheSkyMultiOBOS_v5[idx].showBands == showBands && cacheSkyMultiOBOS_v5[idx].atrPeriod == atrPeriod && cacheSkyMultiOBOS_v5[idx].atrFactor == atrFactor && cacheSkyMultiOBOS_v5[idx].halfLength == halfLength && cacheSkyMultiOBOS_v5[idx].EqualsInput(input))
						return cacheSkyMultiOBOS_v5[idx];
			return CacheIndicator<Sky.SkyMultiOBOS_v5>(new Sky.SkyMultiOBOS_v5(){ MFIPeriod = mFIPeriod, RSIPeriod = rSIPeriod, RSISmooth = rSISmooth, StochDPeriod = stochDPeriod, StochKPeriod = stochKPeriod, StochSmooth = stochSmooth, MFIOB = mFIOB, MFIOS = mFIOS, RSIOB = rSIOB, RSIOS = rSIOS, STOCHOB = sTOCHOB, STOCHOS = sTOCHOS, DrawLong = drawLong, DrawShort = drawShort, SymbolDistance = symbolDistance, showSymbol = showSymbol, SymbolLColor = symbolLColor, SymbolSColor = symbolSColor, ShowOBOSZones = showOBOSZones, showEntryLine = showEntryLine, bullColor = bullColor, bearColor = bearColor, shortColor = shortColor, longColor = longColor, showBands = showBands, atrPeriod = atrPeriod, atrFactor = atrFactor, halfLength = halfLength }, input, ref cacheSkyMultiOBOS_v5);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Sky.SkyMultiOBOS_v5 SkyMultiOBOS_v5(int mFIPeriod, int rSIPeriod, int rSISmooth, int stochDPeriod, int stochKPeriod, int stochSmooth, int mFIOB, int mFIOS, int rSIOB, int rSIOS, int sTOCHOB, int sTOCHOS, string drawLong, string drawShort, int symbolDistance, bool showSymbol, Brush symbolLColor, Brush symbolSColor, bool showOBOSZones, bool showEntryLine, Brush bullColor, Brush bearColor, Brush shortColor, Brush longColor, bool showBands, int atrPeriod, double atrFactor, int halfLength)
		{
			return indicator.SkyMultiOBOS_v5(Input, mFIPeriod, rSIPeriod, rSISmooth, stochDPeriod, stochKPeriod, stochSmooth, mFIOB, mFIOS, rSIOB, rSIOS, sTOCHOB, sTOCHOS, drawLong, drawShort, symbolDistance, showSymbol, symbolLColor, symbolSColor, showOBOSZones, showEntryLine, bullColor, bearColor, shortColor, longColor, showBands, atrPeriod, atrFactor, halfLength);
		}

		public Indicators.Sky.SkyMultiOBOS_v5 SkyMultiOBOS_v5(ISeries<double> input , int mFIPeriod, int rSIPeriod, int rSISmooth, int stochDPeriod, int stochKPeriod, int stochSmooth, int mFIOB, int mFIOS, int rSIOB, int rSIOS, int sTOCHOB, int sTOCHOS, string drawLong, string drawShort, int symbolDistance, bool showSymbol, Brush symbolLColor, Brush symbolSColor, bool showOBOSZones, bool showEntryLine, Brush bullColor, Brush bearColor, Brush shortColor, Brush longColor, bool showBands, int atrPeriod, double atrFactor, int halfLength)
		{
			return indicator.SkyMultiOBOS_v5(input, mFIPeriod, rSIPeriod, rSISmooth, stochDPeriod, stochKPeriod, stochSmooth, mFIOB, mFIOS, rSIOB, rSIOS, sTOCHOB, sTOCHOS, drawLong, drawShort, symbolDistance, showSymbol, symbolLColor, symbolSColor, showOBOSZones, showEntryLine, bullColor, bearColor, shortColor, longColor, showBands, atrPeriod, atrFactor, halfLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Sky.SkyMultiOBOS_v5 SkyMultiOBOS_v5(int mFIPeriod, int rSIPeriod, int rSISmooth, int stochDPeriod, int stochKPeriod, int stochSmooth, int mFIOB, int mFIOS, int rSIOB, int rSIOS, int sTOCHOB, int sTOCHOS, string drawLong, string drawShort, int symbolDistance, bool showSymbol, Brush symbolLColor, Brush symbolSColor, bool showOBOSZones, bool showEntryLine, Brush bullColor, Brush bearColor, Brush shortColor, Brush longColor, bool showBands, int atrPeriod, double atrFactor, int halfLength)
		{
			return indicator.SkyMultiOBOS_v5(Input, mFIPeriod, rSIPeriod, rSISmooth, stochDPeriod, stochKPeriod, stochSmooth, mFIOB, mFIOS, rSIOB, rSIOS, sTOCHOB, sTOCHOS, drawLong, drawShort, symbolDistance, showSymbol, symbolLColor, symbolSColor, showOBOSZones, showEntryLine, bullColor, bearColor, shortColor, longColor, showBands, atrPeriod, atrFactor, halfLength);
		}

		public Indicators.Sky.SkyMultiOBOS_v5 SkyMultiOBOS_v5(ISeries<double> input , int mFIPeriod, int rSIPeriod, int rSISmooth, int stochDPeriod, int stochKPeriod, int stochSmooth, int mFIOB, int mFIOS, int rSIOB, int rSIOS, int sTOCHOB, int sTOCHOS, string drawLong, string drawShort, int symbolDistance, bool showSymbol, Brush symbolLColor, Brush symbolSColor, bool showOBOSZones, bool showEntryLine, Brush bullColor, Brush bearColor, Brush shortColor, Brush longColor, bool showBands, int atrPeriod, double atrFactor, int halfLength)
		{
			return indicator.SkyMultiOBOS_v5(input, mFIPeriod, rSIPeriod, rSISmooth, stochDPeriod, stochKPeriod, stochSmooth, mFIOB, mFIOS, rSIOB, rSIOS, sTOCHOB, sTOCHOS, drawLong, drawShort, symbolDistance, showSymbol, symbolLColor, symbolSColor, showOBOSZones, showEntryLine, bullColor, bearColor, shortColor, longColor, showBands, atrPeriod, atrFactor, halfLength);
		}
	}
}

#endregion
