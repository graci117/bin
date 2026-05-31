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

///+--------------------------------------------------------------------------------------------------+
///|   Site:     https://fxstill.com                                                                  |
///|   Telegram: https://t.me/fxstill (Literature on cryptocurrencies, development and code. )        |
///|                                   Don't forget to subscribe!                                     |
///+--------------------------------------------------------------------------------------------------+


//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.FxStill.SmartMoney
{
	public class MarketStructuresLite : Indicator
	{
		private double dUp, dDw;
		private DateTime dtUp, dtDw;
		private bool bUp, bDw;
		private int    MinBar;
		private double delta;
		private NinjaTrader.Gui.Tools.SimpleFont myFont;
		private bool bFirstHH, bFirstLL;
		private bool bSecondHH, bSecondLL;
		private bool bBoSL, bBoSH;
		
		private double lastSwingHigh;
		private double lastSwingLow;
		private int barLastSwingHigh;
		private int barLastSwingLow;
		private bool hasLastSwingHigh;
		private bool hasLastSwingLow;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"This indicator will indicate a Breaking Structure (BOS). Development: https://fxstill.com and https://t.me/fxstill.";
				Name										= "MarketStructuresLite";
				Calculate									= Calculate.OnBarClose;
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
				IsAutoScale 								= false;	
				
				
				Period					        = 5;
				bLbl                            = true;
				iDist                           = 10;
				iFontSz                         = 14;
				BuyClr                          = Brushes.Cyan;
				SellClr                         = Brushes.Yellow;
				iWdth                           = 2;
				eStyle                          = DashStyleHelper.Solid;
				
				BackgroundColor 				= true;
                bCopacity 						= 50;
				bColorUp 						= new SolidColorBrush(Color.FromArgb((byte) bCopacity, (byte) 0, (byte) 80, (byte) 0));
		        bColorDn 						= new SolidColorBrush(Color.FromArgb((byte) bCopacity, (byte) 80, (byte) 0, (byte) 0));
				bColorFlat 						= new SolidColorBrush(Color.FromArgb((byte) bCopacity, (byte) 245, (byte) 242, (byte) 242));
				ShowEntrySignals				= true;
				ShowEntrySignalsBoS				= false;
				ShowEntrySignalsRev				= true;
				LongEntry						= "LE ";
				ShortEntry						= "SE ";	
				bFirstHH = bFirstLL 			= false;
				bSecondHH = bSecondLL 			= false;
				bBoSL = bBoSH 					= false;
				
				
				AddPlot(new Stroke(Brushes.BlueViolet, 1) ,PlotStyle.PriceBox, "Trend");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Dot, "Signal");
			}
			else if (State == State.Configure)
			{
				ClearOutputWindow(); 
				MinBar  = 2 * Period + 1;
				delta = iDist * TickSize;
				bUp = bDw = false;
				myFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", iFontSz);
				
        		bColorUp = new SolidColorBrush(Color.FromArgb((byte) bCopacity, (byte) 0, (byte) 80, (byte) 0));
        		bColorDn = new SolidColorBrush(Color.FromArgb((byte) bCopacity, (byte) 80, (byte) 0, (byte) 0));
				bColorFlat = new SolidColorBrush(Color.FromArgb((byte) bCopacity, (byte) 245, (byte) 242, (byte) 242));
        		bColorUp.Freeze();
        		bColorDn.Freeze();	
				bColorFlat.Freeze();
			}
		}

		protected override void OnBarUpdate() 		
		{		
			Signal[0] = 0;
			
			if (CurrentBar <= MinBar) {
				dDw = Low[0];
				dUp = High[0];
				return;
			}	
			
			FindBosLine();
			FindPointSwing();
			DrawFibonacciRetracement();
			
		} // protected override void OnBarUpdate()
		
		protected void FindBosLine() {
   			if (bUp) {
      			if (High[0] >= dUp) {
         			bUp = false;
					Draw.Line(this, "BosUp" + CurrentBar, true, dtUp, dUp, Time[0], dUp, BuyClr, eStyle, iWdth);
					Draw.Text(this,"BoSUpL" + CurrentBar, false, "BoS", 0, dUp + delta, 5, BuyClr, myFont, TextAlignment.Right, null, null, 1);
					if (ShowEntrySignals && ShowEntrySignalsBoS) Draw.TriangleUp(this, "LE " + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] + (-5 * TickSize)) , Brushes.Lime);
				    Signal[0] = 1;
					if (bFirstHH) bBoSH = true;			

      			}
   			}
   			if (bDw) {
      			if (Low[0] <= dDw) {
         			bDw = false;
					Draw.Line(this, "BosDw" + CurrentBar, true, dtDw, dDw, Time[0], dDw, SellClr, eStyle, iWdth);
					Draw.Text(this,"BoSDwL" + CurrentBar, false, "BoS", 0, dDw - delta, 5, SellClr, myFont, TextAlignment.Right, null, null, 1);
					if (ShowEntrySignals && ShowEntrySignalsBoS) Draw.TriangleDown(this, "SE " + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (5 * TickSize)) , Brushes.Red);
				    Signal[0] = -1;
					if (bFirstLL) bBoSL = true;
					
      			}
   			}
		}//void FindBosLine()		
		
		protected void FindPointSwing() {
 		    int z, y;
			bool   fr_up = true, fr_dw = true;
			bool   d_up = false, d_dw = false;
			
			for(int j = 1; j <= Period; j++) {
      			z = Period - j;
      			y = Period + j;
      			if (fr_up) {
         			if( (High[Period] <= High[z]) || (High[Period] < High[y]) ) {
            			fr_up = false;
	         		}
      			}
      			if (fr_dw) {
         			if( (Low[Period] >= Low[z]) || (Low[Period] > Low[y]) ) {
            			fr_dw = false;
         			}
      			}				
			}// for(int j = 1; j < Period; j++)
			
		    if(fr_up) {
				if (bLbl) {
					if (High[Period] >= dUp)
					{
            			lastSwingHigh = High[Period];
						barLastSwingHigh = CurrentBar - Period;
            			hasLastSwingHigh = true;						
						
						Draw.Text(this,"swHH" + CurrentBar, false, "HH", Period, High[Period] + delta, 5, BuyClr, myFont, TextAlignment.Center, null, null, 1);
						d_up = true;
						d_dw = false;
						if (bFirstHH) bSecondHH = true;
						if (!bFirstHH)
						{
							bFirstHH = true;
							bSecondHH = false;
							bBoSH = false;
						}							
						if (bSecondHH && bBoSH) 
						{
						//	bSecondHH = true;
							if (ShowEntrySignals && ShowEntrySignalsRev) Draw.TriangleDown(this, "SE " + Convert.ToString(CurrentBars[0]), false, 0, (High[0] + (5 * TickSize)) , Brushes.Indigo);	
					        Signal[0] = -1;
							bFirstHH = false;
							bSecondHH = false;
							bBoSH = false;
						}	
					}	
					else
					{	
						Draw.Text(this,"swLH" + CurrentBar, false, "LH", Period, High[Period] + delta, 5, BuyClr, myFont, TextAlignment.Center, null, null, 1);
						d_up = false;
						d_dw = true;
						bFirstHH = false;
						bSecondHH = false;
						bBoSH = false;
					}	
						
				}
				dUp   = High[Period];
				dtUp  = Time[Period];
				bUp = true;
			}
		    if(fr_dw) {
				if (bLbl) {
					if (Low[Period] <= dDw)
					{	
						lastSwingLow = Low[Period];
						barLastSwingLow = CurrentBar - Period;
            			hasLastSwingLow = true;
						
						Draw.Text(this,"swLL" + CurrentBar, false, "LL", Period, Low[Period] - delta, 5, SellClr, myFont, TextAlignment.Center, null, null, 1);
						d_up = false;
						d_dw = true;	
						if (bFirstLL) bSecondLL = true;
						if (!bFirstLL)
						{
							bFirstLL = true;
							bSecondLL = false;
							bBoSL = false;				
						}
						if (bSecondLL && bBoSL)
						{
						//	bSecondLL = true;
							if (ShowEntrySignals && ShowEntrySignalsRev) Draw.TriangleUp(this, "LE " + Convert.ToString(CurrentBars[0]), false, 0, (Low[0] + (-5 * TickSize)) , Brushes.DeepSkyBlue);	
					        Signal[0] = 1;
							bFirstLL = false;
							bSecondLL = false;
							bBoSL = false;
						}							
					}	
					else
					{
						Draw.Text(this,"swHL" + CurrentBar, false, "HL", Period, Low[Period] - delta, 5, SellClr, myFont, TextAlignment.Center, null, null, 1);
						d_up = true;
						d_dw = false;		
						bFirstLL = false;
						bSecondLL = false;
						bBoSL = false;
					}	
				}
				dDw   = Low[Period]; 
				dtDw  = Time[Period];
				bDw = true;
			}
			
			Trend[0] = 0;
			Trend[0] = (d_up || bDw ? 1 : (d_dw || bUp ? -1 : Trend[1]));
			if (BackgroundColor)
                BackBrushAll = Trend[0] == 1.0 ? bColorUp : (Trend[0] == -1.0 ? bColorDn : bColorFlat);
		}
		
		private void DrawFibonacciRetracement()
		{
		    if (hasLastSwingHigh && hasLastSwingLow)
		    {
		        double range = lastSwingHigh - lastSwingLow;
				if (barLastSwingHigh > barLastSwingLow) 	
					Draw.FibonacciRetracements(this, "Fibo", false, CurrentBar - barLastSwingHigh, lastSwingHigh, CurrentBar - barLastSwingLow, lastSwingLow, false, "Fibo_Leo");  
				if (barLastSwingLow > barLastSwingHigh)
					Draw.FibonacciRetracements(this, "Fibo", false, CurrentBar - barLastSwingLow, lastSwingLow, CurrentBar - barLastSwingHigh, lastSwingHigh, false, "Fibo_Leo");  
		    }
		}		
		

		private void InitializeSecuence()
		{
			bFirstHH = bFirstLL = bSecondHH = bSecondLL = false;
			bBoSL = bBoSH = false;			
			
		}	
			
		#region Properties MarketStructureLite
		[NinjaScriptProperty]
		[Range(3, int.MaxValue)]
		[Display(Name="Period", Description="Period swing calculation", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Draw Label", Description="Draw Swing Label", Order=1, GroupName="View")]
		public bool bLbl
		{ get; set; }			
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Label's Distance", Description="Label's Distance", Order=2, GroupName="View")]
		public int iDist
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Label's Font Size", Description="Label's Font Size", Order=3, GroupName="View")]
		public int iFontSz
		{ get; set; }	
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Long Color", Order=4, GroupName="View")]
		public Brush BuyClr
		{ get; set; }

		[Browsable(false)]
		public string BuyClrSerializable
		{
			get { return Serialize.BrushToString(BuyClr); }
			set { BuyClr = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Short Color", Order=5, GroupName="View")]
		public Brush SellClr
		{ get; set; }

		[Browsable(false)]
		public string SellClrSerializable
		{
			get { return Serialize.BrushToString(SellClr); }
			set { SellClr = Serialize.StringToBrush(value); }
		}		
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Width", Order=6, GroupName="View")]
		public int iWdth
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name="Line's Style", Order=7, GroupName="View")]
		public DashStyleHelper eStyle
		{ get; set; }				
		
        [NinjaScriptProperty]
        [Display(GroupName = "View", Name = "Background Color?", Order = 35)]
        public bool BackgroundColor { get; set; }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(GroupName = "View", Name = "Background-Color Up", Order = 37)]
        public Brush bColorUp { get; set; }

        [Browsable(false)]
        public string bColorUpSerializable
        {
            get => Serialize.BrushToString(bColorUp);
            set => bColorUp = Serialize.StringToBrush(value);
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(GroupName = "View", Name = "Background-Color Down", Order = 38)]
        public Brush bColorDn { get; set; }

        [Browsable(false)]
        public string bColorDnSerializable
        {
            get => Serialize.BrushToString(bColorDn);
            set => bColorDn = Serialize.StringToBrush(value);
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(GroupName = "View", Name = "Background-Color Flat", Order = 40)]
        public Brush bColorFlat { get; set; }

        [Browsable(false)]
        public string bColorFlatSerializable
        {
            get => Serialize.BrushToString(bColorFlat);
            set => bColorFlat = Serialize.StringToBrush(value);
        }		
		
        [NinjaScriptProperty]
        [Display(GroupName = "View", Name = "Background Opacity", Order = 41)]
        public int bCopacity { get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name="Show ENTRY PredatorX Signals", Description="Signals to Automate this indicator with the Predator X Order Entry", Order=0, GroupName="Predator Signals")]
		public bool ShowEntrySignals
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Long Entry", Description="Signals to Automate this indicator with the Predator X Order Entry", Order=1, GroupName="Predator Signals")]
		public string LongEntry
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Short Entry", Description="Signals to Automate this indicator with the Predator X Order Entry", Order=3, GroupName="Predator Signals")]
		public string ShortEntry
		{ get; set; }
				
		[NinjaScriptProperty]
		[Display(Name="Show ENTRY PredatorX Signals (BoS)", Description="Signals to Automate this indicator with the Predator X Order Entry", Order=0, GroupName="Predator Signals")]
		public bool ShowEntrySignalsBoS
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Display(Name="Show ENTRY PredatorX Signals (REV)", Description="Signals to Automate this indicator with the Predator X Order Entry", Order=0, GroupName="Predator Signals")]
		public bool ShowEntrySignalsRev
		{ get; set; }	
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return Values[0]; }
		}	
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
		    get { return Values[1]; } // Corresponds to the 2nd plot we added
		}
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FxStill.SmartMoney.MarketStructuresLite[] cacheMarketStructuresLite;
		public FxStill.SmartMoney.MarketStructuresLite MarketStructuresLite(int period, bool bLbl, int iDist, int iFontSz, Brush buyClr, Brush sellClr, int iWdth, DashStyleHelper eStyle, bool backgroundColor, Brush bColorUp, Brush bColorDn, Brush bColorFlat, int bCopacity, bool showEntrySignals, string longEntry, string shortEntry, bool showEntrySignalsBoS, bool showEntrySignalsRev)
		{
			return MarketStructuresLite(Input, period, bLbl, iDist, iFontSz, buyClr, sellClr, iWdth, eStyle, backgroundColor, bColorUp, bColorDn, bColorFlat, bCopacity, showEntrySignals, longEntry, shortEntry, showEntrySignalsBoS, showEntrySignalsRev);
		}

		public FxStill.SmartMoney.MarketStructuresLite MarketStructuresLite(ISeries<double> input, int period, bool bLbl, int iDist, int iFontSz, Brush buyClr, Brush sellClr, int iWdth, DashStyleHelper eStyle, bool backgroundColor, Brush bColorUp, Brush bColorDn, Brush bColorFlat, int bCopacity, bool showEntrySignals, string longEntry, string shortEntry, bool showEntrySignalsBoS, bool showEntrySignalsRev)
		{
			if (cacheMarketStructuresLite != null)
				for (int idx = 0; idx < cacheMarketStructuresLite.Length; idx++)
					if (cacheMarketStructuresLite[idx] != null && cacheMarketStructuresLite[idx].Period == period && cacheMarketStructuresLite[idx].bLbl == bLbl && cacheMarketStructuresLite[idx].iDist == iDist && cacheMarketStructuresLite[idx].iFontSz == iFontSz && cacheMarketStructuresLite[idx].BuyClr == buyClr && cacheMarketStructuresLite[idx].SellClr == sellClr && cacheMarketStructuresLite[idx].iWdth == iWdth && cacheMarketStructuresLite[idx].eStyle == eStyle && cacheMarketStructuresLite[idx].BackgroundColor == backgroundColor && cacheMarketStructuresLite[idx].bColorUp == bColorUp && cacheMarketStructuresLite[idx].bColorDn == bColorDn && cacheMarketStructuresLite[idx].bColorFlat == bColorFlat && cacheMarketStructuresLite[idx].bCopacity == bCopacity && cacheMarketStructuresLite[idx].ShowEntrySignals == showEntrySignals && cacheMarketStructuresLite[idx].LongEntry == longEntry && cacheMarketStructuresLite[idx].ShortEntry == shortEntry && cacheMarketStructuresLite[idx].ShowEntrySignalsBoS == showEntrySignalsBoS && cacheMarketStructuresLite[idx].ShowEntrySignalsRev == showEntrySignalsRev && cacheMarketStructuresLite[idx].EqualsInput(input))
						return cacheMarketStructuresLite[idx];
			return CacheIndicator<FxStill.SmartMoney.MarketStructuresLite>(new FxStill.SmartMoney.MarketStructuresLite(){ Period = period, bLbl = bLbl, iDist = iDist, iFontSz = iFontSz, BuyClr = buyClr, SellClr = sellClr, iWdth = iWdth, eStyle = eStyle, BackgroundColor = backgroundColor, bColorUp = bColorUp, bColorDn = bColorDn, bColorFlat = bColorFlat, bCopacity = bCopacity, ShowEntrySignals = showEntrySignals, LongEntry = longEntry, ShortEntry = shortEntry, ShowEntrySignalsBoS = showEntrySignalsBoS, ShowEntrySignalsRev = showEntrySignalsRev }, input, ref cacheMarketStructuresLite);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FxStill.SmartMoney.MarketStructuresLite MarketStructuresLite(int period, bool bLbl, int iDist, int iFontSz, Brush buyClr, Brush sellClr, int iWdth, DashStyleHelper eStyle, bool backgroundColor, Brush bColorUp, Brush bColorDn, Brush bColorFlat, int bCopacity, bool showEntrySignals, string longEntry, string shortEntry, bool showEntrySignalsBoS, bool showEntrySignalsRev)
		{
			return indicator.MarketStructuresLite(Input, period, bLbl, iDist, iFontSz, buyClr, sellClr, iWdth, eStyle, backgroundColor, bColorUp, bColorDn, bColorFlat, bCopacity, showEntrySignals, longEntry, shortEntry, showEntrySignalsBoS, showEntrySignalsRev);
		}

		public Indicators.FxStill.SmartMoney.MarketStructuresLite MarketStructuresLite(ISeries<double> input , int period, bool bLbl, int iDist, int iFontSz, Brush buyClr, Brush sellClr, int iWdth, DashStyleHelper eStyle, bool backgroundColor, Brush bColorUp, Brush bColorDn, Brush bColorFlat, int bCopacity, bool showEntrySignals, string longEntry, string shortEntry, bool showEntrySignalsBoS, bool showEntrySignalsRev)
		{
			return indicator.MarketStructuresLite(input, period, bLbl, iDist, iFontSz, buyClr, sellClr, iWdth, eStyle, backgroundColor, bColorUp, bColorDn, bColorFlat, bCopacity, showEntrySignals, longEntry, shortEntry, showEntrySignalsBoS, showEntrySignalsRev);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FxStill.SmartMoney.MarketStructuresLite MarketStructuresLite(int period, bool bLbl, int iDist, int iFontSz, Brush buyClr, Brush sellClr, int iWdth, DashStyleHelper eStyle, bool backgroundColor, Brush bColorUp, Brush bColorDn, Brush bColorFlat, int bCopacity, bool showEntrySignals, string longEntry, string shortEntry, bool showEntrySignalsBoS, bool showEntrySignalsRev)
		{
			return indicator.MarketStructuresLite(Input, period, bLbl, iDist, iFontSz, buyClr, sellClr, iWdth, eStyle, backgroundColor, bColorUp, bColorDn, bColorFlat, bCopacity, showEntrySignals, longEntry, shortEntry, showEntrySignalsBoS, showEntrySignalsRev);
		}

		public Indicators.FxStill.SmartMoney.MarketStructuresLite MarketStructuresLite(ISeries<double> input , int period, bool bLbl, int iDist, int iFontSz, Brush buyClr, Brush sellClr, int iWdth, DashStyleHelper eStyle, bool backgroundColor, Brush bColorUp, Brush bColorDn, Brush bColorFlat, int bCopacity, bool showEntrySignals, string longEntry, string shortEntry, bool showEntrySignalsBoS, bool showEntrySignalsRev)
		{
			return indicator.MarketStructuresLite(input, period, bLbl, iDist, iFontSz, buyClr, sellClr, iWdth, eStyle, backgroundColor, bColorUp, bColorDn, bColorFlat, bCopacity, showEntrySignals, longEntry, shortEntry, showEntrySignalsBoS, showEntrySignalsRev);
		}
	}
}

#endregion
