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
    public class ECO2New2 : Indicator
    {
        #region Variables
        // Wizard generated variables
        private int fast = 7; // Default setting for Fast 3
        private int slow = 21; // Default setting for Slow 13
        private int signalLen = 4; // Default setting for SignalLen 5
                                   // User defined variables (add any user defined variables below)
        Series<double> fastEma;
        Series<double> fastAbsEma;
        Series<double> slowEma;
        Series<double> slowAbsEma;
        Series<double> closeopen;
        Series<double> highlow;
        Series<double> eco;
        Series<double> average;
        bool histogramType = true;
        private bool showText = true;
        private bool textOnLeft = false;
        private TextPosition bPosition = TextPosition.BottomLeft;
        private TextPosition tPosition = TextPosition.TopLeft;
        //		double average;
        //		double eco;
        private bool showEntries = true;
        private bool alerts = false;
        private int edownbar = 0;
        private int edownplot = 0;
        private int triangleDn = 0;
        private int eupbar = 0;
        private int eupplot = 0;
        private int triangleUp = 0;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"ECO2New2(Ergodic Candlestick Oscilator.  The ECO2New2 is a Momentum Oscillator that is not affected by opening gaps.  See William Blau's Momentum/Direction/Divergence book";
                Name = "ECO2New2";
                Calculate = Calculate.OnEachTick;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive = true;
                Fast = fast;
                Slow = slow;
                SignalLen = signalLen;
                HistogramType = false;
                ShowText = true;
                TextOnLeft = true;
                ShowEntries = true;
                Alerts = true;
                /* 
                V1.1 - Fixed Plot Overflow problem.  Code was not skipping enough warm-up bars and was getting an overflow on some charts
                                                        - now skips 10 bars
                Adapted from Blau's Momentum/Direction/Divergence by zoltran (Wes S.) 

                                // Formula for Candlestick oscilator from Blaus book is 
                            //                      EMA(EMA(close-open),r),s)
                            //    CSI(r,s)= 100 x  --------------------------
                            //                        EMA(EMA(high-low),r),s)
                            //
                            // The 'Ergodic' version adds signal line, wich is a 5 bar ema of the CSI
                            //
                Trading Ergodics with the Trend - Rules

                1. Enter or hold position only when slope of ECO2New2 Signal line has the same direction as the trend
                2. Stand aside when slope of ECO2New2 Signal Line is in the opposite direction of trend
                3. Enter or exit position when ECO2New2 and its Signal Line cross
                4. Take note when the ECO2New2 is 'inside' it's signal line.  This indicates failing momemntum
                   This is what the magenta dot signifies
                */

                // The add statements are in this odd order so that 'z' order of the plots works right... 
                // Fading dot plots on top of the Signal 
                // Signal plot on top of the Main indicator

                AddPlot(new Stroke(Brushes.Magenta, 2), PlotStyle.Dot, "Fading");          // Values[0]
                AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "Signal");           // Values[1]			
                AddPlot(new Stroke(Brushes.SpringGreen, 3), PlotStyle.Line, "Rising");     // Values[2]
                AddPlot(new Stroke(Brushes.MediumVioletRed, 3), PlotStyle.Line, "Falling");        // Values[3]
                AddPlot(new Stroke(Brushes.Yellow, 1), PlotStyle.Line, "Main");                // Values[4]			
                AddPlot(new Stroke(Brushes.DarkGreen, 4), PlotStyle.Bar, "RisingHistogram");       // Values[5]			
                AddPlot(new Stroke(Brushes.Crimson, 4), PlotStyle.Bar, "FallingHistogram");        // Values[6]

                AddLine(Brushes.DarkGray, 0, "Zero");
                AddLine(Brushes.DarkGray, 20, "OverBought");
                AddLine(Brushes.DarkGray, -20, "OverSold");


                // Init user data series
                closeopen = new Series<double>(this);
                highlow = new Series<double>(this); // Init the data series
                fastEma = new Series<double>(this);
                fastAbsEma = new Series<double>(this);
                slowEma = new Series<double>(this);
                slowAbsEma = new Series<double>(this);
                eco = new Series<double>(this);
                average = new Series<double>(this);
                this.BarsRequiredToPlot = 1; //Slow+SignalLen+2;
            }
            else if (State == State.Configure)
            {
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 3) return;
            //replace DataSeries for Series<double>
            // and closeopen.Set for closeopen[0]
            closeopen[0] = (Close[0] - Open[0]);                  // Store the result of close - open
            highlow[0] = (High[0] - Low[0]);                      // Store the result of high - low            

            if (CurrentBar < Slow + Fast + 1)
            {
                for (int index = 0; index <= 6; index++)
                {
                    Values[index].Reset();
                }
                fastEma.Reset();
                fastAbsEma.Reset();
                slowEma.Reset();
                slowAbsEma.Reset();
                eco.Reset();
                average.Reset();

                return;     // Skip number of bars used for the ECO2New2 and it's EMA. .. warm up time  Overkill, but safe
            }
            else
            {

                // Formula for Candlestick oscilator from Blaus book is 
                //                      EMA(EMA(close-open),r),s)
                //    CSI(r,s)= 100 x  --------------------------
                //                        EMA(EMA(high-low),r),s)
                //
                // The 'Ergodic' version adds signal line, wich is a 5 bar ema of the CSI
                //

                eco[0] = (100 * EMA(EMA(closeopen, slow), fast)[0] / EMA(EMA(highlow, slow), fast)[0]);

                if (CurrentBar > Slow + Fast + SignalLen + 2)
                    average[0] = (EMA(eco, SignalLen)[0]);        // Take a 5 bar EMA of the ECO2New2 (stored in collection Values[4]				
                Signal[0] = average[0];                      // Plot the Signal Line Value[1]
                Main[0] = eco[0];                          // Plot main ECO2New2 line  Value[4]

                // The following code does the multicolor paints using extra plots for each color				
                Values[2].Reset();                                  // Reset other lines to 0
                Values[3].Reset();
                Values[5].Reset();
                Values[6].Reset();

                if (eco[0] > eco[1])    // Plot green bars for rising eco, red for falling		
                {
                    // if (eco[1]<eco[2]) Values[2].Set(1,eco[1]);//   connect prior secgment Paint falling eco  if (CurrentBar > Slow+Fast+SignalLen)
                    ECORising[0] = eco[0];                          //  Value[2]  Paint Rising ECO2New2 Line
                }
                else if (eco[0] < eco[1])
                {
                    //if (eco[1]> eco[2])Values[3].Set(1,eco[1]);	//   connect prior secgment Paint falling eco
                    ECOFalling[0] = eco[0];                      //    Value[3] Paint falling eco			 
                }
                if (HistogramType)
                {
                    if (eco[0] > 0)
                        RisingHistogram[0] = eco[0];                      //    Values[5] Paint histogram above zero
                    else
                        FallingHistogram[0] = eco[0];                         //    Values[6] Paint histogram below zero	
                }

                else
                {
                    if (IsRising(eco))
                        RisingHistogram[0] = eco[0];                     //    Paint histogram Rising
                    else
                        FallingHistogram[0] = eco[0];                     //    Paint histogram Falling
                }

                if (eco[0] < 0 && average[0] < eco[0])
                    Fading[0] = average[0]; // Plot the 'Fading' dots if signal is outside of the ECO2New2
                if (eco[0] > 0 && average[0] > eco[0])
                    Fading[0] = average[0]; // This indicates declining momentum
                                            //// else Values[1].Set(eco[0]);							//defautls to same as eco 

                //--Text Box Info--//

                double ECOminus = Math.Abs(eco[1] - eco[0]);
                double ECOplus = Math.Abs(eco[0] - eco[1]);

                if (textOnLeft)
                {
                    bPosition = TextPosition.BottomLeft;
                    tPosition = TextPosition.TopLeft;
                }
                else
                {
                    bPosition = TextPosition.BottomRight;
                    tPosition = TextPosition.TopRight;
                }

                if (showText)
                {
                    if (eco[0] < eco[1])
                        //Draw.TextFixed("EcoDown", " ECO Diff -" + ECOminus.ToString("0.00"), bPosition, Colors.Black, new SimpleFont ("Arial Narrow", 12), Colors.Crimson, Colors.Red, 10);
                        Draw.TextFixed(this, "EcoDown", " ECO Diff -" + ECOminus.ToString("0.00"), bPosition, Brushes.White, new SimpleFont("Arial Narrow", 12), Brushes.Crimson, Brushes.Red, 10);
                    else
                        RemoveDrawObject("EcoDown");

                    if (eco[0] > eco[1])
                        Draw.TextFixed(this, "EcoUp", " ECO Diff +" + ECOplus.ToString("0.00"), bPosition, Brushes.White, new SimpleFont("Arial Narrow", 12), Brushes.DarkGreen, Brushes.LimeGreen, 10);
                    else
                        RemoveDrawObject("EcoUp");

                    if (eco[0] < 0 && average[0] < 0 && eco[0] < average[0])
                        Draw.TextFixed(this, "Short", " SHORT", tPosition, Brushes.White, new SimpleFont("Arial Narrow", 12), Brushes.Crimson, Brushes.Orange, 10);
                    else
                        RemoveDrawObject("Short");

                    if (eco[0] > 0 && average[0] > 0 && eco[0] > average[0])
                        Draw.TextFixed(this, "Long", " LONG", tPosition, Brushes.White, new SimpleFont("Arial Narrow", 12), Brushes.Crimson, Brushes.Orange, 10);
                    else
                        RemoveDrawObject("Long");

                    if (eco[0] > 0 && average[0] > 0 && eco[0] < average[0] || eco[0] < 0 && average[0] < 0 && eco[0] > average[0])
                        Draw.TextFixed(this, "Fading", " FADING", tPosition, Brushes.White, new SimpleFont("Arial Narrow", 12), Brushes.Crimson, Brushes.Orange, 10);
                    else
                        RemoveDrawObject("Fading");
                }

                //Triangle Plotting//

                if (average[0] > 0 || average[0] < eco[0] || eco[0] > 0)
                    triangleDn = 0;
                if (average[0] < 0 || average[0] > eco[0] || eco[0] < 0)
                    triangleUp = 0;

                if (showEntries)
                {

                    if (CrossBelow(average, 0, 1) && eco[0] < average[0] && triangleDn == 0 || CrossAbove(average, eco, 1) && average[0] < 0 && triangleDn == 0)
                    {
                        Draw.TriangleDown(this, "EcoDT", true, 0, 0, Brushes.Transparent);
                        if (alerts == true)
                            Alert("DnTrend", Priority.Medium, "Dn Trend Pending", "Drip.wav", 6, Brushes.White, Brushes.Yellow);
                        edownbar = CurrentBar;
                        edownplot = 1;
                    }
                    if (edownplot == 1 && CurrentBar == edownbar + 1)
                    {
                        RemoveDrawObject("EcoDT");
                        edownplot = 0;
                        if (eco[1] < average[1] && average[1] < 0 && triangleDn == 0)
                        {
                            Draw.TriangleDown(this, CurrentBar.ToString() + "EcoDTSolid", true, 1, 0, Brushes.Red);
                            if (eco[1] > eco[2] || average[1] > average[2])
                                Draw.TriangleDown(this, CurrentBar.ToString() + "EcoDTSolidDark", true, 1, 0, Brushes.LightSalmon);
                            triangleDn = 1;
                            if (alerts == true)
                                Alert("DnTrendFinal", Priority.Medium, "Dn Trend", "Alert2.wav", 6, Brushes.Black, Brushes.Yellow);
                        }
                    }

                    if (CrossAbove(average, 0, 1) && eco[0] > average[0] && triangleUp == 0 || CrossBelow(average, eco, 1) && average[0] > 0 && triangleUp == 0)
                    {
                        Draw.TriangleUp(this, "EcoUT", true, 0, 0, Brushes.Transparent);
                        if (alerts == true)
                            Alert("UpTrend", Priority.Medium, "Up Trend Pending", "Drip.wav", 6, Brushes.Black, Brushes.Yellow);
                        eupbar = CurrentBar;
                        eupplot = 1;
                    }

                    if (eupplot == 1 && CurrentBar == eupbar + 1)
                    {
                        RemoveDrawObject("EcoUT");
                        eupplot = 0;
                        if (eco[1] > average[1] && average[1] > 0 && triangleUp == 0)
                        {
                            Draw.TriangleUp(this, CurrentBar.ToString() + "EcoUTSolid", true, 1, 0, Brushes.Green);
                            if (eco[1] < eco[2] || average[1] < average[2])
                                Draw.TriangleUp(this, CurrentBar.ToString() + "EcoUTSolidDark", true, 1, 0, Brushes.GreenYellow);
                            triangleUp = 1;
                            if (alerts == true)
                                Alert("UpTrendFinal", Priority.Medium, "Up Trend", "Alert2.wav", 6, Brushes.White, Brushes.Yellow);
                        }
                    }
                }//ShowEntries
            } //else from ocillator
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast", Description = "Fast Ema", Order = 1, GroupName = "Parameters")]
        public int Fast
        {
            get { return fast; }
            set { fast = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Slow", Description = "Slow EMA", Order = 2, GroupName = "Parameters")]
        public int Slow
        {
            get { return slow; }
            set { slow = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "SignalEma", Description = "Signal EMA", Order = 3, GroupName = "Parameters")]
        public int SignalLen
        {
            get { return signalLen; }
            set { signalLen = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "HistogramType", Description = "Histogram Paint Type. True = Green/Red Above/Below 0.  False = Histogram Color shows Rising/Falling ECO", Order = 4, GroupName = "Parameters")]
        public bool HistogramType
        {
            get { return histogramType; }
            set { histogramType = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "ShowText", Description = "Show Text Info.", Order = 5, GroupName = "Parameters")]
        public bool ShowText
        {
            get { return showText; }
            set { showText = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "TextOnLeft", Description = "Text on left of chart? For those with no right margin.", Order = 6, GroupName = "Parameters")]
        public bool TextOnLeft
        {
            get { return textOnLeft; }
            set { textOnLeft = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "ShowEntries", Description = "Show Entry Triangles", Order = 7, GroupName = "Parameters")]
        public bool ShowEntries
        {
            get { return showEntries; }
            set { showEntries = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Alerts", Description = "Sound alert warning.", Order = 8, GroupName = "Parameters")]
        public bool Alerts
        {
            get { return alerts; }
            set { alerts = value; }
        }


        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Signal
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ECORising
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ECOFalling
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Main
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> RisingHistogram
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> FallingHistogram
        {
            get { return Values[6]; }
        }

        [Browsable(false)]  // this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]       // this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> Fading
        {
            get { return Values[0]; }
        }
        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ECO2New2[] cacheECO2New2;
		public ECO2New2 ECO2New2(int fast, int slow, int signalLen, bool histogramType, bool showText, bool textOnLeft, bool showEntries, bool alerts)
		{
			return ECO2New2(Input, fast, slow, signalLen, histogramType, showText, textOnLeft, showEntries, alerts);
		}

		public ECO2New2 ECO2New2(ISeries<double> input, int fast, int slow, int signalLen, bool histogramType, bool showText, bool textOnLeft, bool showEntries, bool alerts)
		{
			if (cacheECO2New2 != null)
				for (int idx = 0; idx < cacheECO2New2.Length; idx++)
					if (cacheECO2New2[idx] != null && cacheECO2New2[idx].Fast == fast && cacheECO2New2[idx].Slow == slow && cacheECO2New2[idx].SignalLen == signalLen && cacheECO2New2[idx].HistogramType == histogramType && cacheECO2New2[idx].ShowText == showText && cacheECO2New2[idx].TextOnLeft == textOnLeft && cacheECO2New2[idx].ShowEntries == showEntries && cacheECO2New2[idx].Alerts == alerts && cacheECO2New2[idx].EqualsInput(input))
						return cacheECO2New2[idx];
			return CacheIndicator<ECO2New2>(new ECO2New2(){ Fast = fast, Slow = slow, SignalLen = signalLen, HistogramType = histogramType, ShowText = showText, TextOnLeft = textOnLeft, ShowEntries = showEntries, Alerts = alerts }, input, ref cacheECO2New2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ECO2New2 ECO2New2(int fast, int slow, int signalLen, bool histogramType, bool showText, bool textOnLeft, bool showEntries, bool alerts)
		{
			return indicator.ECO2New2(Input, fast, slow, signalLen, histogramType, showText, textOnLeft, showEntries, alerts);
		}

		public Indicators.ECO2New2 ECO2New2(ISeries<double> input , int fast, int slow, int signalLen, bool histogramType, bool showText, bool textOnLeft, bool showEntries, bool alerts)
		{
			return indicator.ECO2New2(input, fast, slow, signalLen, histogramType, showText, textOnLeft, showEntries, alerts);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ECO2New2 ECO2New2(int fast, int slow, int signalLen, bool histogramType, bool showText, bool textOnLeft, bool showEntries, bool alerts)
		{
			return indicator.ECO2New2(Input, fast, slow, signalLen, histogramType, showText, textOnLeft, showEntries, alerts);
		}

		public Indicators.ECO2New2 ECO2New2(ISeries<double> input , int fast, int slow, int signalLen, bool histogramType, bool showText, bool textOnLeft, bool showEntries, bool alerts)
		{
			return indicator.ECO2New2(input, fast, slow, signalLen, histogramType, showText, textOnLeft, showEntries, alerts);
		}
	}
}

#endregion
