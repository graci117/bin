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



//	The "Leledc" indicator was originally introduced by a user named "glaz" on TradingView. 

//	It's designed to identify potential trend exhaustion or reversals by highlighting a specific
//	type of candle that forms after a sequence of bars moving in the same direction.

//	The indicator looks for a candle where the price action has reached a new high or low within
//	a specified period, after a series of bars closing in the same direction, which could signal
//	the end of a strong trend or a pause before a continuation. 

//	In an uptrend, a Leledc bar might form after a series of consecutive bars closing higher,
//	indicating that the upward momentum might be slowing or reversing.

//	In a downtrend, a Leledc bar might form after a series of consecutive bars closing lower,
//	indicating that the downward momentum might be slowing or reversing




//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.CampervanSeth
{
	
	[Gui.CategoryOrder("Time Contraints", 1)]
    [Gui.CategoryOrder("Colors", 2)]
    [Gui.CategoryOrder("Major Signal", 3)]
	[Gui.CategoryOrder("Minor Signal", 4)]
	
	
	public class LeledcExhaustionPro : Indicator
	{
		
		private string drawLong = "[▲]";
		private string drawShort = "[▼]";
		
		private string drawLongMinor = "(●)";
		private string drawShortMinor = "(●)";
		
		

        private int bindex = 0;
        private int sindex = 0;
		
		private Brush Brush1;
		private Brush Brush2;
		
		
		private Dictionary<string, double> bullishRectangles = new Dictionary<string, double>();
		private Dictionary<string, double> bearishRectangles = new Dictionary<string, double>();
		
		
		private int signalState = 0; // 1 = Long, -1 = Short, 0 = No signal
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Leledc refers to a price action indicator that highlights a candle after a series of bars in the same direction result in the greatest high/low over a period, often indicating a potential trend pause or exhaustion";
				Name										= "Leledc Exhaustion Pro";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= false;
				IsAutoScale									= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				Start_Time									= DateTime.Parse("00:00", System.Globalization.CultureInfo.InvariantCulture);
				End_Time									= DateTime.Parse("23:59", System.Globalization.CultureInfo.InvariantCulture);
				
                // Input parameters
                ShowMajor 									= true;
                ShowMinor 									= false;
                MajorBarCount 								= 6;
                MajorBarLen 								= 30;
                MinorBarCount 								= 5;
                MinorBarLen 								= 5;
				
				playsound									= false;
				
				ExhaustSound 								= NinjaTrader.Core.Globals.InstallDir + @"\sounds\j2PriceRejection.wav"; //decleared the wavsound variable 
				
				AddPlot(Brushes.Blue, "SignalState");
				
			}
			else if (State == State.Configure)
			{
				
			}
		}

		protected override void OnBarUpdate()
		{
		
			 // Reset to no signal by default
            signalState = 0;
			
			
		 // Ensure enough bars are available for calculations
            if (CurrentBar < Math.Max(MajorBarLen, MinorBarLen))
                return;

            // Calculate Major and Minor exhaustion levels
            int majorSignal = CalculateLeledc(MajorBarCount, MajorBarLen);
            int minorSignal = CalculateLeledc(MinorBarCount, MinorBarLen);

            // Plot markers for Major signals
            if (ShowMajor && (Times[0][0].TimeOfDay >= Start_Time.TimeOfDay)
				 && (Times[0][0].TimeOfDay < End_Time.TimeOfDay))
            {
                if (majorSignal == 1) // Major Bullish
                {
                    Draw.Text(this, $"MajorBullish{CurrentBar}", drawLong, 0, (Low[0] + (-SymbolDistance * TickSize)), SignalLColor);
					BarBrush = SignalLColor;
					CandleOutlineBrush = SignalLColor;
					Alert(@"Exhaustion Long", Priority.High, @"Exhaustion Long", null, 120, Brushes.Lime, Brushes.Black);
					signalState = 1; // Long Signal
					
					
					if
							(playsound)
							PlaySound(ExhaustSound); //playing sound	
                }
                else if (majorSignal == -1) // Major Bearish
                {
                    Draw.Text(this, $"MajorBearish{CurrentBar}", drawShort, 0, (High[0] + (SymbolDistance * TickSize)), SignalSColor);
					BarBrush = SignalSColor;
					CandleOutlineBrush = SignalSColor;
					Alert(@"Exhaustion Short", Priority.High, @"Exhaustion Short", null, 120, Brushes.Red, Brushes.Black);
					signalState = -1; // Short Signal
					

					if
							(playsound)
							PlaySound(ExhaustSound); //playing sound
				}
			}
            // Plot markers for Minor signals
            if (ShowMinor&& (Times[0][0].TimeOfDay >= Start_Time.TimeOfDay)
				 && (Times[0][0].TimeOfDay < End_Time.TimeOfDay))
            {
                if (minorSignal == 1) // Minor Bullish
                {
                    Draw.Text(this, $"MinorBullish{CurrentBar}", drawLongMinor, 0, (Low[0] + (-SymbolDistance * TickSize)), SignalLColor);
					CandleOutlineBrush = SignalLColor;
					
                }
                else if (minorSignal == -1) // Minor Bearish
                {
                    Draw.Text(this, $"MinorBearish{CurrentBar}", drawShortMinor, 0, (High[0] + (SymbolDistance * TickSize)), SignalSColor);
					CandleOutlineBrush = SignalSColor;
					
                }
            }
			
				
			#region Rectangles
			
 // Plot markers for Major signals
            if (ShowMajor && (Times[0][0].TimeOfDay >= Start_Time.TimeOfDay)
				 && (Times[0][0].TimeOfDay < End_Time.TimeOfDay))			
			if (ShowRectangles) // Only draw if user enabled it
    {
        // Define high/low for exhaustion bars
        double bullrectHigh = High[0];
        double bullrectLow = Low[0];
        double bullrectMid = (bullrectHigh + bullrectLow) / 2;

        double bearrectHigh = High[0];
        double bearrectLow = Low[0];
        double bearrectMid = (bearrectHigh + bearrectLow) / 2;

        if (majorSignal == 1) // Bullish Exhaustion (Green)
        {
            string bullrectTag = $"BullishRect_{CurrentBar}";
            string bullmidLineTag = $"BullishMidLine_{CurrentBar}";

            Draw.Rectangle(this, bullrectTag, false, 0, bullrectHigh, RectLength * (-1), bullrectLow, Brushes.Transparent, SignalLColor, (RectOpacity));
            Draw.Line(this, bullmidLineTag, false, 0, bullrectMid, RectLength * (-1), bullrectMid, SignalLColor, DashStyleHelper.Dot, 1);

            // Store rectangle for tracking
            if (!bullishRectangles.ContainsKey(bullrectTag))
            {
                bullishRectangles.Add(bullrectTag, bullrectLow);
            }
        }

        if (majorSignal == -1) // Bearish Exhaustion (Red)
        {
            string bearrectTag = $"BearishRect_{CurrentBar}";
            string bearmidLineTag = $"BearishMidLine_{CurrentBar}";

            Draw.Rectangle(this, bearrectTag, false, 0, bearrectHigh, RectLength * (-1), bearrectLow, Brushes.Transparent, SignalSColor, (RectOpacity));
            Draw.Line(this, bearmidLineTag, false, 0, bearrectMid, RectLength * (-1), bearrectMid, SignalSColor, DashStyleHelper.Dot, 1);

            // Store rectangle for tracking
            if (!bearishRectangles.ContainsKey(bearrectTag))
            {
                bearishRectangles.Add(bearrectTag, bearrectHigh);
            }
        }

        // **Check and remove bullish rectangles if price drops below their low**
        List<string> bullToRemove = new List<string>();
        foreach (var rect in bullishRectangles)
        {
            if (Close[0] < rect.Value) // Price dropped below stored rectangle low
            {
                RemoveDrawObject(rect.Key); // Remove rectangle
                RemoveDrawObject(rect.Key.Replace("BullishRect_", "BullishMidLine_")); // Remove mid-line
                bullToRemove.Add(rect.Key); // Mark for removal
            }
        }

        // Remove breached bullish rectangles from dictionary
        foreach (var key in bullToRemove)
        {
            bullishRectangles.Remove(key);
        }

        // **Check and remove bearish rectangles if price rises above their high**
        List<string> bearToRemove = new List<string>();
        foreach (var rect in bearishRectangles)
        {
            if (Close[0] > rect.Value) // Price moved above stored rectangle high
            {
                RemoveDrawObject(rect.Key); // Remove rectangle
                RemoveDrawObject(rect.Key.Replace("BearishRect_", "BearishMidLine_")); // Remove mid-line
                bearToRemove.Add(rect.Key); // Mark for removal
            }
        }

        // Remove breached bearish rectangles from dictionary
        foreach (var key in bearToRemove)
        {
            bearishRectangles.Remove(key);
        }
    }
#endregion
			
			
			
			 // Output the signal state
            Values[0][0] = signalState;
	 
	} // End On Bar Update
			

        private int CalculateLeledc(int qual, int len)
        {
            // Increment bindex and sindex based on price movement
            if (Close[0] > Close[CloseValInput])
                bindex++;
            if (Close[0] < Close[CloseValInput])
                sindex++;

            // Check for exhaustion conditions
            if (bindex > qual && Close[0] < Open[0] && High[0] >= MAX(High, len)[0])
            {
                bindex = 0; // Reset counter
                return -1; // Major Bearish signal
            }

            if (sindex > qual && Close[0] > Open[0] && Low[0] <= MIN(Low, len)[0])
            {
                sindex = 0; // Reset counter
                return 1; // Major Bullish signal
            }

            return 0; // No signal	
		
        }
		
				
	// In order to trim the indicator's label on the chart we need to override the ToString() method.
			public override string DisplayName
				{
		            get { return Name ;}
				}		
		
				
		#region Properties // User-configurable properties	
				
		[NinjaScriptProperty]
		[Display(Name = "Show Exhaustion Zone", Description="Draws a rectangle with midline from High to Low of Leledc bar", Order = 20, GroupName = "Major Signal")]
		public bool ShowRectangles { get; set; } = true;			
	
		[NinjaScriptProperty]
    	[Range(1, int.MaxValue)]
    	[Display(Name = "Zone Length (Bars)", Order = 30, GroupName = "Major Signal")]
    	public int RectLength
		{ get; set; } = 500;
	
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Zone Opacity", Description="Opacity of Zone", Order=40, GroupName= "Major Signal")]
		public int RectOpacity
		{ get; set; } = 25;	
	
        
		/// ------ Major Long Signal ------
		// [Browsable(false)] 
		[NinjaScriptProperty]
        [Display(Name = "Show Major", Order = 1, GroupName = "Major Signal")]
        public bool ShowMajor 
		{ get; set; } = true;
		
		[NinjaScriptProperty]
		[Display(Name="Major Long Signal", Order=4, GroupName= "Major Signal")]
		public string DrawLong
		{ get { return drawLong; } set { drawLong = value; } }
		
		// [NinjaScriptProperty]
       // [Display(Name = "Show Highlight", Order = 2, GroupName = "Major Signal")]
       // public bool highlight 
		//{ get; set; } = true;
		
		//[NinjaScriptProperty]
		//[Range(0, int.MaxValue)]
		//[Display(Name="Highlight Opacity", Description="Opacity highlight", Order=3, GroupName= "Major Signal")]
		//public double Opacity
		//{ get; set; } = 100;	
		
		/// ------ Major Short Signal ------
		[NinjaScriptProperty]
		[Display(Name="Major Short Signal", Order=5, GroupName= "Major Signal")]
		public string DrawShort
		{ get { return drawShort; } set { drawShort = value; } }
		
		[NinjaScriptProperty]
		[Display(Name="Play Alerts", Description="Play Alerts Sounds", Order=6, GroupName="Major Signal")]
		public bool playsound
		{ get; set; }
		
		[NinjaScriptProperty]
		[Description("The name of the sound file. NT will look for this file in Documents\\NinjaTrader 8\\sounds.")]
        [Display(Name = "Exhaustion Sound", GroupName = "Major Signal", Order = 7)]
		[ PropertyEditor ("NinjaTrader.Gui.Tools.FilePathPicker" , Filter= "Any Files (*.*)|*.*" )]
        public string ExhaustSound
        { get ; set ; }
				
		/// ------ Signal Distance ------
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Signal Offset Distance", Order = 3, GroupName = "Major Signal")]
		public int SymbolDistance { get; set; } = 4;
		
		/// ------ Minor Long Signal ------
		[NinjaScriptProperty]
        [Display(Name = "Show Minor", Order = 1, GroupName ="Minor Signal")]
        public bool ShowMinor
		{ get; set; } = false;
		
		[NinjaScriptProperty]
		[Display(Name="Minor Long Signal", Order=2, GroupName ="Minor Signal")]
		public string DrawLongMinor
		{ get { return drawLongMinor; } set { drawLongMinor = value; } }
		
		/// ------ Minor Short Signal ------
		[NinjaScriptProperty]
		[Display(Name="Minor Short Signal", Order=3, GroupName ="Minor Signal")]
		public string DrawShortMinor
		{ get { return drawShortMinor; } set { drawShortMinor = value; } }
		
		/// ------ Long Coloring ------
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Long Color ", GroupName= "Colors", Order=1)]
		public Brush SignalLColor { get; set; } = Brushes.Lime;

		[Browsable(false)]
		public string SignalLColorSerialize
		{
			get { return Serialize.BrushToString(SignalLColor); }
   			set { SignalLColor = Serialize.StringToBrush(value); }
		}
		/// ------ Short Coloring ------
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Short Color ", GroupName= "Colors", Order=2)]
		public Brush SignalSColor { get; set; } = Brushes.Red;

		[Browsable(false)]
		public string SignalSColorSerialize
		{
			get { return Serialize.BrushToString(SignalSColor); }
   			set { SignalSColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time", Description="Start Time", Order = 8, GroupName = "Time Contraints")]
		public DateTime Start_Time
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time", Description="End Time",Order = 9, GroupName = "Time Contraints")]
		public DateTime End_Time
		{ get; set; }
		
		#region Hidden Properties
		
		[Browsable(false)]
        [Range(1, int.MaxValue)]
        [Display(Name = "Major Bar Count", Order = 3, GroupName = "Parameters")]
        public int MajorBarCount
		{ get; set; } = 6;

       [Browsable(false)]
        [Range(1, int.MaxValue)]
        [Display(Name = "Major Bar Length", Order = 4, GroupName = "Parameters")]
        public int MajorBarLen
		{ get; set; } = 30;

        [Browsable(false)]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minor Bar Count", Order = 5, GroupName = "Parameters")]
        public int MinorBarCount
		{ get; set; } = 5;

        [Browsable(false)]
        [Range(1, int.MaxValue)]
        [Display(Name = "Minor Bar Length", Order = 6, GroupName = "Parameters")]
        public int MinorBarLen
		{ get; set; } = 5;

        [Browsable(false)]
        [Range(0, int.MaxValue)]
        [Display(Name = "Close Val Input", Order = 7, GroupName = "Parameters")]
        public int CloseValInput
		{ get; set; }  = 4;
	
		
		#endregion
		
		
	#endregion	
	

		
		// Public property to expose Signal State in UI (READ-ONLY)
        [Browsable(true)] // Show in properties panel
        [Display(Name = "-1 = Short, 0 = No Signal, 1 = Long", Description = "Displays the current signal state (-1 = Short, 0 = No Signal, 1 = Long)", Order = 1, GroupName = "Signal State Info")]
        public int SignalState
        {
            get { return signalState; } // Read-only property
        }
	
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CampervanSeth.LeledcExhaustionPro[] cacheLeledcExhaustionPro;
		public CampervanSeth.LeledcExhaustionPro LeledcExhaustionPro(bool showRectangles, int rectLength, int rectOpacity, bool showMajor, string drawLong, string drawShort, bool playsound, string exhaustSound, int symbolDistance, bool showMinor, string drawLongMinor, string drawShortMinor, Brush signalLColor, Brush signalSColor, DateTime start_Time, DateTime end_Time)
		{
			return LeledcExhaustionPro(Input, showRectangles, rectLength, rectOpacity, showMajor, drawLong, drawShort, playsound, exhaustSound, symbolDistance, showMinor, drawLongMinor, drawShortMinor, signalLColor, signalSColor, start_Time, end_Time);
		}

		public CampervanSeth.LeledcExhaustionPro LeledcExhaustionPro(ISeries<double> input, bool showRectangles, int rectLength, int rectOpacity, bool showMajor, string drawLong, string drawShort, bool playsound, string exhaustSound, int symbolDistance, bool showMinor, string drawLongMinor, string drawShortMinor, Brush signalLColor, Brush signalSColor, DateTime start_Time, DateTime end_Time)
		{
			if (cacheLeledcExhaustionPro != null)
				for (int idx = 0; idx < cacheLeledcExhaustionPro.Length; idx++)
					if (cacheLeledcExhaustionPro[idx] != null && cacheLeledcExhaustionPro[idx].ShowRectangles == showRectangles && cacheLeledcExhaustionPro[idx].RectLength == rectLength && cacheLeledcExhaustionPro[idx].RectOpacity == rectOpacity && cacheLeledcExhaustionPro[idx].ShowMajor == showMajor && cacheLeledcExhaustionPro[idx].DrawLong == drawLong && cacheLeledcExhaustionPro[idx].DrawShort == drawShort && cacheLeledcExhaustionPro[idx].playsound == playsound && cacheLeledcExhaustionPro[idx].ExhaustSound == exhaustSound && cacheLeledcExhaustionPro[idx].SymbolDistance == symbolDistance && cacheLeledcExhaustionPro[idx].ShowMinor == showMinor && cacheLeledcExhaustionPro[idx].DrawLongMinor == drawLongMinor && cacheLeledcExhaustionPro[idx].DrawShortMinor == drawShortMinor && cacheLeledcExhaustionPro[idx].SignalLColor == signalLColor && cacheLeledcExhaustionPro[idx].SignalSColor == signalSColor && cacheLeledcExhaustionPro[idx].Start_Time == start_Time && cacheLeledcExhaustionPro[idx].End_Time == end_Time && cacheLeledcExhaustionPro[idx].EqualsInput(input))
						return cacheLeledcExhaustionPro[idx];
			return CacheIndicator<CampervanSeth.LeledcExhaustionPro>(new CampervanSeth.LeledcExhaustionPro(){ ShowRectangles = showRectangles, RectLength = rectLength, RectOpacity = rectOpacity, ShowMajor = showMajor, DrawLong = drawLong, DrawShort = drawShort, playsound = playsound, ExhaustSound = exhaustSound, SymbolDistance = symbolDistance, ShowMinor = showMinor, DrawLongMinor = drawLongMinor, DrawShortMinor = drawShortMinor, SignalLColor = signalLColor, SignalSColor = signalSColor, Start_Time = start_Time, End_Time = end_Time }, input, ref cacheLeledcExhaustionPro);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CampervanSeth.LeledcExhaustionPro LeledcExhaustionPro(bool showRectangles, int rectLength, int rectOpacity, bool showMajor, string drawLong, string drawShort, bool playsound, string exhaustSound, int symbolDistance, bool showMinor, string drawLongMinor, string drawShortMinor, Brush signalLColor, Brush signalSColor, DateTime start_Time, DateTime end_Time)
		{
			return indicator.LeledcExhaustionPro(Input, showRectangles, rectLength, rectOpacity, showMajor, drawLong, drawShort, playsound, exhaustSound, symbolDistance, showMinor, drawLongMinor, drawShortMinor, signalLColor, signalSColor, start_Time, end_Time);
		}

		public Indicators.CampervanSeth.LeledcExhaustionPro LeledcExhaustionPro(ISeries<double> input , bool showRectangles, int rectLength, int rectOpacity, bool showMajor, string drawLong, string drawShort, bool playsound, string exhaustSound, int symbolDistance, bool showMinor, string drawLongMinor, string drawShortMinor, Brush signalLColor, Brush signalSColor, DateTime start_Time, DateTime end_Time)
		{
			return indicator.LeledcExhaustionPro(input, showRectangles, rectLength, rectOpacity, showMajor, drawLong, drawShort, playsound, exhaustSound, symbolDistance, showMinor, drawLongMinor, drawShortMinor, signalLColor, signalSColor, start_Time, end_Time);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CampervanSeth.LeledcExhaustionPro LeledcExhaustionPro(bool showRectangles, int rectLength, int rectOpacity, bool showMajor, string drawLong, string drawShort, bool playsound, string exhaustSound, int symbolDistance, bool showMinor, string drawLongMinor, string drawShortMinor, Brush signalLColor, Brush signalSColor, DateTime start_Time, DateTime end_Time)
		{
			return indicator.LeledcExhaustionPro(Input, showRectangles, rectLength, rectOpacity, showMajor, drawLong, drawShort, playsound, exhaustSound, symbolDistance, showMinor, drawLongMinor, drawShortMinor, signalLColor, signalSColor, start_Time, end_Time);
		}

		public Indicators.CampervanSeth.LeledcExhaustionPro LeledcExhaustionPro(ISeries<double> input , bool showRectangles, int rectLength, int rectOpacity, bool showMajor, string drawLong, string drawShort, bool playsound, string exhaustSound, int symbolDistance, bool showMinor, string drawLongMinor, string drawShortMinor, Brush signalLColor, Brush signalSColor, DateTime start_Time, DateTime end_Time)
		{
			return indicator.LeledcExhaustionPro(input, showRectangles, rectLength, rectOpacity, showMajor, drawLong, drawShort, playsound, exhaustSound, symbolDistance, showMinor, drawLongMinor, drawShortMinor, signalLColor, signalSColor, start_Time, end_Time);
		}
	}
}

#endregion
