// #############################################################
// #														   #
// #                   WeisWaveDelta by Gill 				   #
// #						16.07.2020						   #
// #                    gsbi619znw@pomail.net                  #
// #														   #
// #			   adapted from PriceActionSwing	           #
// #     19.12.2016 by dorschden, die.unendlichkeit@gmx.de     #
// #													       #
// #        Thanks and comments are highly appreciated         #
// # Paypal thanks to dorschden work "die.unendlichkeit@gmx.de"#
// #														   #
// #############################################################


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
using WeisWaveDelta.Base;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// WeisWaveDelta calculates swings, visualizes them in different ways
    /// and displays several information about the swings.
    /// </summary>
	public class WeisWaveDelta : Indicator
	{
        
		#region Variables
		
			
			private VolumeDelta VolDelta;
			private Series<double> deltaseries;
			int DtbStrength = 20;

        //#########################################################################################
        #region Display
        //=========================================================================================
     
        private bool swingVolumeType = true;
		/// <summary>
        /// Indicates if the swing volume delta is shown for the swings.
        /// </summary>
        private bool swingDeltaType = true;
		/// <summary>
		/// Represents the name of the indicator.
		/// </summary>
		private string displayName = null;
        //=========================================================================================
        #endregion

        #region Visualize Volume
        //=========================================================================================
 
        /// <summary>
        /// Indicates if AutoScale is used for the swings. 
        /// </summary>
        private bool useAutoScale = true;
        /// <summary>
        /// Indicates if the swings are drawn on the price panel. 
        /// </summary>
        private bool drawSwingsOnPricePanel = true;
        /// <summary>
        /// Represents the color of the zig-zag up lines for the swings.
        /// </summary>
        private Brush zigZagColorUp = Brushes.Gray;
        /// <summary>
        /// Represents the color of the zig-zag down lines for the swings.
        /// </summary>
        private Brush zigZagColorDn = Brushes.Gray;
        /// <summary>
        /// Represents the line style of the zig-zag lines for the swings.
        /// </summary>
        private DashStyleHelper zigZagStyle = DashStyleHelper.Solid;
        /// <summary>
        /// Represents the line width of the zig-zag lines for the swings.
        /// </summary>
        private int zigZagWidth = 4;
        /// <summary>
        /// Represents the color of the swing value output for higher highs for the swings.
        /// </summary>
        private Brush textColorVolumeHigh = Brushes.DodgerBlue;
        /// <summary>
        /// Represents the color of the swing value output for lower highs for the swings.
        /// </summary>
        private Brush textColorVolumeLow = Brushes.Maroon;
        /// <summary>
        /// Represents the color of the swing value output for double tops for the swings.
        /// </summary>
        /// <summary>
        /// Represents the text font for the swing volume value output for the swings.
        /// </summary>
        private SimpleFont textFont = new SimpleFont("Arial", 12);
		/// <summary>
        /// Represents the color of the swing value output for higher lows for the swings.
        /// </summary>
        private Brush textColorPositiveDelta = Brushes.DodgerBlue;
        /// <summary>
        /// Represents the color of the swing value output for lower lows swings for the swings.
        /// </summary>
        private Brush textColorNegativeDelta = Brushes.Maroon;
		 /// <summary>
        /// Represents the text font for the swing volume value output for the swings.
        /// </summary>
        private SimpleFont textFontDelta = new SimpleFont("Arial", 10);
        
        /// <summary>*/
        /// Represents the text offset in pixel for the swing volume delta for the swings.
        /// </summary>
        private int textOffsetDelta = 17;
 
        /// Represents the text offset in pixel for the swing volume for the swings.
        /// </summary>
        private int textOffsetVolume = 3;

        /// <summary>
        /// Indicates if the Gann swings are updated if the last swing high/low is broken. 
        /// </summary>
        private bool useBreakouts = true;
        /// <summary>
        /// Indicates if inside bars are ignored for the Gann swing calculation. If set to 
        /// true it is possible that between consecutive up/down bars are inside bars.
        /// </summary>
        private bool ignoreInsideBars = true;
        /// <summary>
        /// Represents the number of decimal places for the instrument
        /// </summary>
        private int decimalPlaces;
        //=========================================================================================
        #endregion

        #region Class objects and DataSeries
        //=========================================================================================
        /// <summary>
        /// Represents the properties for the swing.
        /// </summary>
        private SwingProperties swingProperties;
        /// <summary>
        /// Represents the values for the current swing.
        /// </summary>
        private SwingCurrent swingCurrent = new SwingCurrent();
        /// <summary>
        /// Represents the swing high values.
        /// </summary>
        private Swings swingHigh = new Swings();
        /// <summary>
        /// Represents the swing low values.
        /// </summary>
        private Swings swingLow = new Swings();
        /// <summary>
        /// Indicates if the swing direction changed form down to up swing for the swings.
        /// </summary>
        private Series<bool> upFlip;
        /// <summary>
        /// Indicates if the swing direction changed form up to down swing for the swings.
        /// </summary>
        private Series<bool> dnFlip;
        /// <summary>
        /// Represents a list of all up swings for the swings.
        /// </summary>
        private List<SwingStruct> swingHighs = new List<SwingStruct>();
        /// <summary>
        /// Represents a list of all down swings for the swings.
        /// </summary>
        private List<SwingStruct> swingLows = new List<SwingStruct>();
        //=========================================================================================
        #endregion
        //#########################################################################################
		#endregion
		
        #region OnStateChange()
        //=========================================================================================
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"WeisWaveDelta calculates swings and visualizes swing volume and volume delta";
				Name								= "WeisWaveDelta";
				Calculate							= Calculate.OnEachTick;
		//		Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= false;
				
		        #region Parameters
		        //=========================================================================================
	
				SwingSize = 2;
				UseCloseValues = false;
		        //=========================================================================================
		        #endregion	        
				
				AddPlot(new Stroke(Brushes.Gold, DashStyleHelper.Solid, 3), PlotStyle.Dot, "DoubleBottom");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 3), PlotStyle.Dot, "LowerLow");
				AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 3), PlotStyle.Dot, "HigherLow");

				AddPlot(new Stroke(Brushes.Gold, DashStyleHelper.Solid, 3), PlotStyle.Dot, "DoubleTop");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 3), PlotStyle.Dot, "LowerHigh");
				AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 3), PlotStyle.Dot, "HigherHigh");
				
				AddPlot(new Stroke(Brushes.Blue, DashStyleHelper.Solid, 1), PlotStyle.Square, "GannSwing");
	
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1), PlotStyle.Square, "Swingdelta");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1), PlotStyle.Square, "Swinglength");
				AddPlot(new Stroke(Brushes.Transparent, DashStyleHelper.Solid, 1), PlotStyle.Square, "Swingslope");
			}
			else if (State == State.DataLoaded)
			{
	            // Calculate decimal places
	            decimal increment = Convert.ToDecimal(Instrument.MasterInstrument.TickSize);
	            int incrementLength = increment.ToString().Length;
	            decimalPlaces = 0;
	            if (incrementLength == 1)
	                decimalPlaces = 0;
	            else if (incrementLength > 2)
	                decimalPlaces = incrementLength - 2;
				
				
					SwingSize = SwingSize < 1 ? 1 : Math.Round(SwingSize, MidpointRounding.AwayFromZero);
				
				
				displayName = Name +  "(" + Instrument.FullName + " (" + BarsPeriod.Value + " " + BarsPeriod.BarsPeriodType + "), " + SwingSize +")";

	            swingProperties = new SwingProperties(SwingSize, DtbStrength,
	                swingVolumeType, swingDeltaType, useBreakouts, ignoreInsideBars, useAutoScale,
	                zigZagColorUp, zigZagColorDn, zigZagStyle, zigZagWidth, textColorVolumeHigh,
	                textColorVolumeLow, textFont, textColorPositiveDelta, textColorNegativeDelta, textFontDelta, textOffsetDelta, 
	                textOffsetVolume, UseCloseValues, drawSwingsOnPricePanel);
				
				deltaseries = new Series<double>(BarsArray[0], MaximumBarsLookBack.Infinite);
	
			}
			else if (State == State.Configure)
			{
				dnFlip = new Series<bool>(this);
				upFlip = new Series<bool>(this);
				VolDelta = VolumeDelta(Brushes.DarkOrange,Brushes.DarkOrange,Brushes.DarkOrange,1,false,0,false);
			}
		}
        //=========================================================================================
		#endregion
		
		public override string DisplayName
		{
		  get { return "WeisWaveDelta by Gill"; }
		}

		protected override void OnBarUpdate()
		{
			Print("Size: " + Convert.ToInt32(textFont.Size));
			
            // Checks to ensure there are enough bars before beginning
            if (CurrentBars[BarsInProgress] <= 1 
                || CurrentBars[BarsInProgress] < SwingSize)
                return;
			if (VolDelta.DeltaClose.IsValidDataPoint(0))
			deltaseries[0] = VolDelta.DeltaClose[0];

            #region Swing calculation
            //=====================================================================================
            InitializeSwingCalculation(swingHigh, swingLow, swingCurrent, upFlip, swingHighs, 
                dnFlip, swingLows);

           
                    CalculateSwingGann(swingHigh, swingLow, swingCurrent, swingProperties, upFlip,
                        swingHighs, dnFlip, swingLows, decimalPlaces, DoubleBottom, LowerLow, 
                        HigherLow, DoubleTop, LowerHigh, HigherHigh, GannSwing);
           //         break;
      
            
            //=====================================================================================
            #endregion
			
		}
		
        #region Initialize swing calculation
        //#########################################################################################
        public void InitializeSwingCalculation(Swings swingHigh, Swings swingLow,
            SwingCurrent swingCur, Series<bool> upFlip, List<SwingStruct> swingHighs,
            Series<bool> dnFlip, List<SwingStruct> swingLows)
        {
            if (IsFirstTickOfBar)
            {
                swingCur.StopOutsideBarCalc = false;

                // Initialize first swing
                if (swingHighs.Count == 0)
                {
                    swingHigh.CurBar = CurrentBars[BarsInProgress];
                    swingHigh.CurPrice = Highs[BarsInProgress][CurrentBars[BarsInProgress]];
                    swingHigh.CurDateTime = swingHigh.LastDateTime =
                        Times[BarsInProgress][CurrentBars[BarsInProgress]];
                    SwingStruct up = new SwingStruct(swingHigh.CurPrice, swingHigh.CurBar,
                        Times[BarsInProgress][CurrentBars[BarsInProgress] - 1], 0, 0, -1,
                        Convert.ToInt64(Volumes[BarsInProgress][0]),
                        deltaseries[0]);
                    swingHighs.Add(up);
                    swingHigh.ListCount = swingHighs.Count;
                }
                if (swingLows.Count == 0)
                {
                    swingLow.CurBar = CurrentBars[BarsInProgress];
                    swingLow.CurPrice = Lows[BarsInProgress][CurrentBars[BarsInProgress]];
                    swingLow.CurDateTime = swingLow.LastDateTime =
                        Times[BarsInProgress][CurrentBars[BarsInProgress]];
                    SwingStruct dn = new SwingStruct(swingLow.CurPrice, swingLow.CurBar,
                        Times[BarsInProgress][CurrentBars[BarsInProgress] - 1], 0, 0, -1,
                        Convert.ToInt64(Volumes[BarsInProgress][0]),
                        deltaseries[0]);
                    swingLows.Add(dn);
                    swingLow.ListCount = swingLows.Count;
                }
            }
            // Set new/update high/low back to false, to avoid function calls which depends on
            // them
            dnFlip[0] = false;
            upFlip[0] = false;
            swingHigh.New = swingLow.New = swingHigh.Update = swingLow.Update = false;
        }
        //#########################################################################################
        #endregion


        #region Calculate Swing Gann
        //#########################################################################################
        protected void CalculateSwingGann(Swings swingHigh, Swings swingLow, SwingCurrent swingCur,
            SwingProperties swingProp, Series<bool> upFlip, List<SwingStruct> swingHighs,
            Series<bool> dnFlip, List<SwingStruct> swingLows, int decimalPlaces, 
            Series<double> doubleBottom, Series<double> lowerLow, Series<double> higherLow, 
            Series<double> doubleTop, Series<double> lowerHigh, Series<double> higherHigh, 
            Series<double> gannSwing)
        {
            #region Set bar property
            //=================================================================================
            // Represents the bar type. -1 = Down | 0 = Inside | 1 = Up | 2 = Outside
            int barType = 0;
            if (Highs[BarsInProgress][0] > Highs[BarsInProgress][1])
            {
                if (Lows[BarsInProgress][0] < Lows[BarsInProgress][1])
                    barType = 2;
                else
                    barType = 1;
            }
            else
            {
                if (Lows[BarsInProgress][0] < Lows[BarsInProgress][1])
                    barType = -1;
                else
                    barType = 0;
            }
            //=================================================================================
            #endregion

            #region Up swing
            //=================================================================================
            if (swingCur.SwingSlope == 1)
            {
                switch (barType)
                {
                    // Up bar
                    case 1:
                        swingCur.ConsecutiveBars = 0;
                        swingCur.ConsecutiveBarValue = 0.0;
                        if (Highs[BarsInProgress][0] > swingHigh.CurPrice)
                        {
                            swingHigh.New = true;
                            swingHigh.Update = true;
                            CalcUpSwing(CurrentBars[BarsInProgress],
                                Highs[BarsInProgress][0], swingHigh.Update, swingHigh,
                                swingLow, swingCur, swingProp, upFlip, swingHighs,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop, 
                                lowerHigh, higherHigh, gannSwing);
                            if ((swingCur.ConsecutiveBars + 1) == swingProp.SwingSize)
                                swingCur.StopOutsideBarCalc = true;
                        }
                        break;
                    // Down bar
                    case -1:
                        if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                        {
                            if (swingCur.ConsecutiveBarValue == 0.0)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                            }
                            else if (Lows[BarsInProgress][0] < swingCur.ConsecutiveBarValue)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                            }
                        }
                        else if (Lows[BarsInProgress][0] < swingCur.ConsecutiveBarValue)
                            swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                        if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                            (swingProp.UseBreakouts && Lows[BarsInProgress][0] <
                            swingLow.CurPrice))
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                            swingLow.New = true;
                            swingLow.Update = false;
                            int bar = CurrentBars[BarsInProgress] -
                                LowestBar(Lows[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingHigh.CurBar);
                            double price =
                                Lows[BarsInProgress][LowestBar(Lows[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingHigh.CurBar)];
                            CalcDnSwing(bar, price, swingLow.Update, swingHigh, swingLow,
                                swingCur, swingProp, dnFlip, swingLows, decimalPlaces, 
                                doubleBottom, lowerLow, higherLow, doubleTop, lowerHigh, 
                                higherHigh, gannSwing);
                        }
                        break;
                    // Inside bar
				
                    case 0:
                        if (!swingProp.IgnoreInsideBars)
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                        }
                        break;
                    // Outside bar
                    case 2:
                        if (Highs[BarsInProgress][0] > swingHigh.CurPrice)
                        {
                            swingHigh.New = true;
                            swingHigh.Update = true;
                            CalcUpSwing(CurrentBars[BarsInProgress],
                                Highs[BarsInProgress][0], swingHigh.Update, swingHigh,
                                swingLow, swingCur, swingProp, upFlip, swingHighs,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop,
                                lowerHigh, higherHigh, gannSwing);
                        }
                        else if (!swingCur.StopOutsideBarCalc)
                        {
                            if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                            {
                                if (swingCur.ConsecutiveBarValue == 0.0)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                                }
                                else if (Lows[BarsInProgress][0] <
                                    swingCur.ConsecutiveBarValue)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                                }
                            }
                            else if (Lows[BarsInProgress][0] < swingCur.ConsecutiveBarValue)
                                swingCur.ConsecutiveBarValue = Lows[BarsInProgress][0];
                            if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                                (swingProp.UseBreakouts && Lows[BarsInProgress][0] <
                                swingLow.CurPrice))
                            {
                                swingCur.ConsecutiveBars = 0;
                                swingCur.ConsecutiveBarValue = 0.0;
                                swingLow.New = true;
                                swingLow.Update = false;
                                int bar = CurrentBars[BarsInProgress] -
                                    LowestBar(Lows[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingHigh.CurBar);
                                double price =
                                    Lows[BarsInProgress][LowestBar(Lows[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingHigh.CurBar)];
                                CalcDnSwing(bar, price, swingLow.Update, swingHigh, swingLow,
                                    swingCur, swingProp, dnFlip, swingLows, decimalPlaces, 
                                    doubleBottom, lowerLow, higherLow, doubleTop, lowerHigh,
                                    higherHigh, gannSwing);
                            }
                        }
                        break;
                }
            }
            //=================================================================================
            #endregion

            #region Down swing
            //=================================================================================
            else
            {
                switch (barType)
                {
                    // Dwon bar
                    case -1:
                        swingCur.ConsecutiveBars = 0;
                        swingCur.ConsecutiveBarValue = 0.0;
                        if (Lows[BarsInProgress][0] < swingLow.CurPrice)
                        {
                            swingLow.New = true;
                            swingLow.Update = true;
                            CalcDnSwing(CurrentBars[BarsInProgress],
                                Lows[BarsInProgress][0], swingLow.Update, swingHigh,
                                swingLow, swingCur, swingProp, dnFlip, swingLows,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop,
                                lowerHigh, higherHigh, gannSwing);
                            if ((swingCur.ConsecutiveBars + 1) == swingProp.SwingSize)
                                swingCur.StopOutsideBarCalc = true;
                        }
                        break;
                    // Up bar
                    case 1:
                        if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                        {
                            if (swingCur.ConsecutiveBarValue == 0.0)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                            }
                            else if (Highs[BarsInProgress][0] > swingCur.ConsecutiveBarValue)
                            {
                                swingCur.ConsecutiveBars++;
                                swingCur.ConsecutiveBarNumber = CurrentBars[BarsInProgress];
                                swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                            }
                        }
                        else if (Highs[BarsInProgress][0] > swingCur.ConsecutiveBarValue)
                            swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                        if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                            (swingProp.UseBreakouts && Highs[BarsInProgress][0] >
                            swingHigh.CurPrice))
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                            swingHigh.New = true;
                            swingHigh.Update = false;
                            int bar = CurrentBars[BarsInProgress] -
                                HighestBar(Highs[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingLow.CurBar);
                            double price =
                                Highs[BarsInProgress][HighestBar(Highs[BarsInProgress],
                                CurrentBars[BarsInProgress] - swingLow.CurBar)];
                            CalcUpSwing(bar, price, swingHigh.Update, swingHigh, swingLow,
                                swingCur, swingProp, upFlip, swingHighs, decimalPlaces, 
                                doubleBottom, lowerLow, higherLow, doubleTop, lowerHigh,
                                higherHigh, gannSwing);
                        }
                        break;
                    // Inside bar
                    case 0:
                        if (!swingProp.IgnoreInsideBars)
                        {
                            swingCur.ConsecutiveBars = 0;
                            swingCur.ConsecutiveBarValue = 0.0;
                        }
                        break;
                    // Outside bar
                    case 2:
                        if (Lows[BarsInProgress][0] < swingLow.CurPrice)
                        {
                            swingLow.New = true;
                            swingLow.Update = true;
                            CalcDnSwing(CurrentBars[BarsInProgress],
                                Lows[BarsInProgress][0], swingLow.Update, swingHigh,
                                swingLow, swingCur, swingProp, dnFlip, swingLows,
                                decimalPlaces, doubleBottom, lowerLow, higherLow, doubleTop,
                                lowerHigh, higherHigh, gannSwing);
                        }
                        else if (!swingCur.StopOutsideBarCalc)
                        {
                            if (swingCur.ConsecutiveBarNumber != CurrentBars[BarsInProgress])
                            {
                                if (swingCur.ConsecutiveBarValue == 0.0)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                                }
                                else if (Highs[BarsInProgress][0] >
                                    swingCur.ConsecutiveBarValue)
                                {
                                    swingCur.ConsecutiveBars++;
                                    swingCur.ConsecutiveBarNumber =
                                        CurrentBars[BarsInProgress];
                                    swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                                }
                            }
                            else if (Highs[BarsInProgress][0] > swingCur.ConsecutiveBarValue)
                                swingCur.ConsecutiveBarValue = Highs[BarsInProgress][0];
                            if (swingCur.ConsecutiveBars == swingProp.SwingSize ||
                                (swingProp.UseBreakouts && Highs[BarsInProgress][0] >
                                swingHigh.CurPrice))
                            {
                                swingCur.ConsecutiveBars = 0;
                                swingCur.ConsecutiveBarValue = 0.0;
                                swingHigh.New = true;
                                swingHigh.Update = false;
                                int bar = CurrentBars[BarsInProgress] -
                                    HighestBar(Highs[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingLow.CurBar);
                                double price =
                                    Highs[BarsInProgress][HighestBar(Highs[BarsInProgress],
                                    CurrentBars[BarsInProgress] - swingLow.CurBar)];
                                CalcUpSwing(bar, price, swingHigh.Update, swingHigh,
                                    swingLow, swingCur, swingProp, upFlip, swingHighs,
                                    decimalPlaces, doubleBottom, lowerLow, higherLow, 
                                    doubleTop, lowerHigh, higherHigh, gannSwing);
                            }
                        }
                        break;
                }
            }
            //=================================================================================
            #endregion
        }
        //#########################################################################################
        #endregion

        #region Calculate down swing
        //#########################################################################################
        protected void CalcDnSwing(int bar, double low, bool updateLow, Swings swingHigh,
			Swings swingLow, SwingCurrent swingCur, SwingProperties swingProp, Series<bool> dnFlip,
            List<SwingStruct> swingLows, int decimalPlaces, Series<double> doubleBottom,
            Series<double> lowerLow, Series<double> higherLow, Series<double> doubleTop, Series<double> lowerHigh,
            Series<double> higherHigh, Series<double> gannSwing)
        {
            #region New and update Swing values
            //=====================================================================================
            if (!updateLow)
            {
   
                swingLow.LastPrice = swingLow.CurPrice;
                swingLow.LastBar = swingLow.CurBar;
                swingLow.LastDateTime = swingLow.CurDateTime;
                swingLow.LastDuration = swingLow.CurDuration;
                swingLow.LastLength = swingLow.CurLength;
                swingLow.LastTime = swingLow.CurTime;
                swingLow.LastPercent = swingLow.CurPercent;
                swingLow.LastRelation = swingLow.CurRelation;
                swingLow.LastVolume = swingLow.CurVolume;
				swingLow.LastDelta = swingLow.CurDelta;
                swingLow.Counter++;
                swingCur.SwingSlope = -1;
                swingCur.SwingSlopeChangeBar = bar;
                dnFlip[0] = true;
            }
            else
            {
    
                swingLows.RemoveAt(swingLows.Count - 1);
            }
            swingLow.CurBar = bar;
            swingLow.CurPrice = Math.Round(low, decimalPlaces, MidpointRounding.AwayFromZero);
            swingLow.CurTime = ToTime(Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingLow.CurBar]);
            swingLow.CurDateTime = Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingLow.CurBar];
            swingLow.CurLength = Convert.ToInt32(Math.Round((swingLow.CurPrice -
                swingHigh.CurPrice) / TickSize, 0, MidpointRounding.AwayFromZero));
            if (swingHigh.CurLength != 0)
                swingLow.CurPercent = Math.Round(100.0 / swingHigh.CurLength *
                    Math.Abs(swingLow.CurLength), 1);
            swingLow.CurDuration = swingLow.CurBar - swingHigh.CurBar;
            double dtbOffset = ATR(BarsArray[BarsInProgress], 14)[CurrentBars[BarsInProgress] -
                swingLow.CurBar] * swingProp.DtbStrength / 100;
            if (swingLow.CurPrice > swingLow.LastPrice - dtbOffset && swingLow.CurPrice <
                swingLow.LastPrice + dtbOffset)
                swingLow.CurRelation = 0;
            else if (swingLow.CurPrice < swingLow.LastPrice)
                swingLow.CurRelation = -1;
            else
                swingLow.CurRelation = 1;
            if (Calculate != Calculate.OnBarClose)
				{
                swingHigh.SignalBarVolume = Volumes[BarsInProgress][0];
				swingHigh.SignalBarDelta = deltaseries[0];
				}
            double swingVolume = 0.0;
			double swingDelta = 0.0;
			for (int i = 0; i < swingLow.CurDuration; i++)
				{
                swingVolume = swingVolume + Volumes[BarsInProgress][i];
				swingDelta = swingDelta + deltaseries[i];
				}
		    if (Calculate != Calculate.OnBarClose)
				{
                swingVolume = swingVolume + (Volumes[BarsInProgress][CurrentBars[BarsInProgress] -
                    swingHigh.CurBar] - swingLow.SignalBarVolume);
				swingDelta = swingDelta + (deltaseries[CurrentBars[BarsInProgress] -
                    swingHigh.CurBar] - swingLow.SignalBarDelta);
				}
    
            swingLow.CurVolume = Convert.ToInt64(swingVolume);
			swingLow.CurDelta = Convert.ToInt64(swingDelta);
	
			Swingdelta[0] = Convert.ToInt64(swingDelta);
			Swinglength[0] = swingLow.CurDuration;
			Swingslope[0] = swingCur.SwingSlope;
				
            //=====================================================================================
            #endregion

           #region Visualize swing
            //=====================================================================================
 //           switch (swingProp.VisualizationType)
  //          {
 
  //              case VisualizationStyle.ZigZagVolume:
                    if (swingLow.CurVolume > swingHigh.CurVolume)
                        Draw.Line(this, "ZigZagDown" + swingLow.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingHigh.CurBar, 
                            swingHigh.CurPrice, CurrentBar - swingLow.CurBar, swingLow.CurPrice, 
                            swingProp.ZigZagColorDn, swingProp.ZigZagStyle, swingProp.ZigZagWidth,
							swingProp.DrawSwingsOnPricePanel);
                    else
                        Draw.Line(this, "ZigZagDown" + swingLow.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingHigh.CurBar,
                            swingHigh.CurPrice, CurrentBar - swingLow.CurBar, swingLow.CurPrice,
                            swingProp.ZigZagColorUp, swingProp.ZigZagStyle, swingProp.ZigZagWidth, 
							swingProp.DrawSwingsOnPricePanel);
  //                  break;
   //         }
            //=====================================================================================
            #endregion		

  

            string swingLabel = null;
            Brush textColor = Brushes.Transparent;
			Brush textColorDelta = Brushes.Transparent;
            switch (swingLow.CurRelation)
            {
                case 1:
                    swingLabel = "HL";
                    textColor = swingProp.TextColorVolumeLow;
                    break;
                case -1:
                    swingLabel = "LL";
                    textColor = swingProp.TextColorVolumeLow;
                    break;
                case 0:
                    swingLabel = "DB";
                    textColor = swingProp.TextColorVolumeLow;
                    break;
            }
			if (swingLow.CurDelta >= 0) textColorDelta = swingProp.TextColorPositiveDelta;
				else textColorDelta = swingProp.TextColorNegativeDelta;
			
    
            if (swingVolumeType)
                Draw.Text(this, "DnVolume" + swingLow.Counter, swingProp.UseAutoScale,
                    TruncIntToStr(swingLow.CurVolume), CurrentBar - swingLow.CurBar,
                    swingLow.CurPrice, -swingProp.TextOffsetVolume - Convert.ToInt32(textFont.Size), textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			if (swingDeltaType)
                Draw.Text(this, "DnDelta" + swingLow.Counter, swingProp.UseAutoScale,
                    TruncIntToStr(swingLow.CurDelta), CurrentBar - swingLow.CurBar,
          	        swingLow.CurPrice, -swingProp.TextOffsetDelta - Convert.ToInt32(textFontDelta.Size), textColorDelta, swingProp.TextFontDelta,
			//		swingLow.CurPrice, -swingProp.TextOffsetVolume - swingProp.TextOffsetDelta, textColorDelta, swingProp.TextFontDelta,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            //=====================================================================================
            #endregion

            SwingStruct dn = new SwingStruct(swingLow.CurPrice, swingLow.CurBar,
                swingLow.CurDateTime, swingLow.CurDuration, swingLow.CurLength,
                swingLow.CurRelation, swingLow.CurVolume, swingLow.CurDelta);
            swingLows.Add(dn);
            swingLow.ListCount = swingLows.Count - 1;
        }
        //#########################################################################################
 //       #endregion

        #region Calculate up swing
        //#########################################################################################
        private void CalcUpSwing(int bar, double high, bool updateHigh, Swings swingHigh,
            Swings swingLow, SwingCurrent swingCur, SwingProperties swingProp, Series<bool> upFlip,
            List<SwingStruct> swingHighs, int decimalPlaces, Series<double> doubleBottom, 
            Series<double> lowerLow, Series<double> higherLow, Series<double> doubleTop, Series<double> lowerHigh, 
            Series<double> higherHigh, Series<double> gannSwing)
        {
            #region New and update swing values
            //=====================================================================================
            if (!updateHigh)
            {
   
                swingHigh.LastPrice = swingHigh.CurPrice;
                swingHigh.LastBar = swingHigh.CurBar;
                swingHigh.LastDateTime = swingHigh.CurDateTime;
                swingHigh.LastDuration = swingHigh.CurDuration;
                swingHigh.LastLength = swingHigh.CurLength;
                swingHigh.LastTime = swingHigh.CurTime;
                swingHigh.LastPercent = swingHigh.CurPercent;
                swingHigh.LastRelation = swingHigh.CurRelation;
                swingHigh.LastVolume = swingHigh.CurVolume;
				swingHigh.LastDelta = swingHigh.CurDelta;
                swingHigh.Counter++;
                swingCur.SwingSlope = 1;
                swingCur.SwingSlopeChangeBar = bar;
                upFlip[0] = true;
            }
            else
            {
      
                swingHighs.RemoveAt(swingHighs.Count - 1);
            }
            swingHigh.CurBar = bar;
            swingHigh.CurPrice = Math.Round(high, decimalPlaces, MidpointRounding.AwayFromZero);
            swingHigh.CurTime = ToTime(Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingHigh.CurBar]);
            swingHigh.CurDateTime = Times[BarsInProgress][CurrentBars[BarsInProgress] -
                swingHigh.CurBar];
            swingHigh.CurLength = Convert.ToInt32(Math.Round((swingHigh.CurPrice -
                swingLow.CurPrice) / TickSize, 0, MidpointRounding.AwayFromZero));
            if (swingLow.CurLength != 0)
                swingHigh.CurPercent = Math.Round(100.0 / Math.Abs(swingLow.CurLength) *
                    swingHigh.CurLength, 1);
            swingHigh.CurDuration = swingHigh.CurBar - swingLow.CurBar;
            double dtbOffset = ATR(BarsArray[BarsInProgress], 14)[CurrentBars[BarsInProgress] -
                swingHigh.CurBar] * swingProp.DtbStrength / 100;
            if (swingHigh.CurPrice > swingHigh.LastPrice - dtbOffset && swingHigh.CurPrice <
                swingHigh.LastPrice + dtbOffset)
                swingHigh.CurRelation = 0;
            else if (swingHigh.CurPrice < swingHigh.LastPrice)
                swingHigh.CurRelation = -1;
            else
                swingHigh.CurRelation = 1;
            if (Calculate != Calculate.OnBarClose)
				{
                swingLow.SignalBarVolume = Volumes[BarsInProgress][0];
				swingLow.SignalBarDelta = deltaseries[0];
				}
            double swingVolume = 0.0;
			double swingDelta = 0.0;
            for (int i = 0; i < swingHigh.CurDuration; i++)
				{
                swingVolume = swingVolume + Volumes[BarsInProgress][i];
				swingDelta = swingDelta + deltaseries[i];
				}
            if (Calculate != Calculate.OnBarClose)
				{
                swingVolume = swingVolume + (Volumes[BarsInProgress][CurrentBars[BarsInProgress] -
                    swingLow.CurBar] - swingHigh.SignalBarVolume);
				swingDelta = swingDelta + (deltaseries[CurrentBars[BarsInProgress] -
                    swingLow.CurBar] - swingHigh.SignalBarDelta);
				}
  
            swingHigh.CurVolume = Convert.ToInt64(swingVolume);
			swingHigh.CurDelta = Convert.ToInt64(swingDelta);
	
			Swingdelta[0] = Convert.ToInt64(swingDelta);
			Swinglength[0] = swingHigh.CurDuration;
			Swingslope[0] = swingCur.SwingSlope;
            //=====================================================================================
            #endregion

            #region Visualize swing
            //=====================================================================================
 
                    if (swingHigh.CurVolume > swingLow.CurVolume)
                        Draw.Line(this, "ZigZagUp" + swingHigh.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingLow.CurBar, 
                            swingLow.CurPrice, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice, 
                            swingProp.ZigZagColorUp, swingProp.ZigZagStyle, swingProp.ZigZagWidth);
                    else
                        Draw.Line(this, "ZigZagUp" + swingHigh.Counter,
                            swingProp.UseAutoScale, CurrentBar - swingLow.CurBar,
                            swingLow.CurPrice, CurrentBar - swingHigh.CurBar, swingHigh.CurPrice,
                            swingProp.ZigZagColorDn, swingProp.ZigZagStyle, swingProp.ZigZagWidth);

            //=====================================================================================
            #endregion

  

            string swingLabel = null;
            Brush textColor = Brushes.Transparent;
			Brush textColorDelta = Brushes.Transparent;
            switch (swingHigh.CurRelation)
            {
                case 1:
                    swingLabel = "HH";
                    textColor = swingProp.TextColorVolumeHigh;
                    break;
                case -1:
                    swingLabel = "LH";
                    textColor = swingProp.TextColorVolumeHigh;
                    break;
                case 0:
                    swingLabel = "DT";
                    textColor = swingProp.TextColorVolumeHigh;
                    break;
            }
			if (swingHigh.CurDelta <= 0) textColorDelta = swingProp.TextColorNegativeDelta;
				else textColorDelta = swingProp.TextColorPositiveDelta;
     
            if (swingVolumeType)
                Draw.Text(this, "UpVolume" + swingHigh.Counter, swingProp.UseAutoScale,
                    TruncIntToStr(swingHigh.CurVolume), CurrentBar - swingHigh.CurBar,
                    swingHigh.CurPrice, swingProp.TextOffsetVolume + Convert.ToInt32(textFont.Size), textColor, swingProp.TextFont,
                    TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			if (swingDeltaType)
                Draw.Text(this, "UpDelta" + swingHigh.Counter, swingProp.UseAutoScale,
                    TruncIntToStr(swingHigh.CurDelta), CurrentBar - swingHigh.CurBar,
                    swingHigh.CurPrice, swingProp.TextOffsetDelta + Convert.ToInt32(textFontDelta.Size), textColorDelta, swingProp.TextFontDelta,
					TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
            //=========================================================================================
            #endregion

            SwingStruct up = new SwingStruct(swingHigh.CurPrice, swingHigh.CurBar,
                swingHigh.CurDateTime, swingHigh.CurDuration, swingHigh.CurLength,
                swingHigh.CurRelation, swingHigh.CurVolume, swingHigh.CurDelta);
            swingHighs.Add(up);
            swingHigh.ListCount = swingHighs.Count - 1;
        }
        //#########################################################################################
  //      #endregion

        #region Trunc integer to string
        //#########################################################################################
        /// <summary>
        /// Converts long integer numbers in a number-string format.
        /// </summary>
        protected string TruncIntToStr(long number)
        {
            long numberAbs = Math.Abs(number);
            string output = "";
            double convertedNumber = 0.0;
            if (numberAbs > 1000000000)
            {
                convertedNumber = Math.Round(number / 1000000000.0, 1,
                    MidpointRounding.AwayFromZero);
                output = convertedNumber.ToString() + "B";
            }
            else if (numberAbs > 1000000)
            {
                convertedNumber = Math.Round(number / 1000000.0, 1,
                    MidpointRounding.AwayFromZero);
                output = convertedNumber.ToString() + "M";
            }

            else
                output = number.ToString();

            return output;
        }
        //#########################################################################################
        #endregion
		
		#region Properties
		
		#region Plots
        // Plots ==================================================================================
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> DoubleBottom
        {
            get { return Values[0]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> LowerLow
        {
            get { return Values[1]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> HigherLow
        {
            get { return Values[2]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> DoubleTop
        {
            get { return Values[3]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> LowerHigh
        {
            get { return Values[4]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> HigherHigh
        {
            get { return Values[5]; }
        }
		[Browsable(false)]
        [XmlIgnore]
        public Series<double> GannSwing
        {
            get { return Values[6]; }
        }
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Swingdelta
		{
			get { return Values[7]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Swinglength
		{
			get { return Values[8]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Swingslope
		{
			get { return Values[9]; }
		}
		//=========================================================================================
        #endregion

        #region Parameters
        //=========================================================================================

		
		[Range(0.00000001, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Swing size", Description = "Represents the swing size. e.g. 1 = small swings and 5 = bigger swings.", Order = 2, GroupName = "Parameters")]
		public double SwingSize
		{ get; set; }

		
		[NinjaScriptProperty]
		[Display(Name = "Use close values", Description = "Indicates if high and low prices are used for the swing calculations or close values.", Order = 4, GroupName = "Parameters")]
		public bool UseCloseValues
		{ get; set; }
        //=========================================================================================
        #endregion

		#region Swings Values
		//=========================================================================================

		
		//=========================================================================================
		#endregion

		#region Visualize Volume
		//=========================================================================================


		[NinjaScriptProperty]
		[Display(Name = "Text Font", Description = "Represents the text font for the swing volume value output.", Order = 2, GroupName = "1. Visualize Volume")]
		public NinjaTrader.Gui.Tools.SimpleFont TextFont
		{
			get { return textFont; }
			set { textFont = value; }
		}
		
	
		[XmlIgnore]
		[Display(Name = "Text Color Volume High", Description = "Represents the color of the swing value output for highs.", Order = 4, GroupName = "1. Visualize Volume")]
		public Brush TextColorVolumeHigh
		{
			get { return textColorVolumeHigh; }
			set { textColorVolumeHigh = value; }
		}

		[Browsable(false)]
		public string TextColorVolumeHighSerializable
		{
			get { return Serialize.BrushToString(textColorVolumeHigh); }
			set { textColorVolumeHigh = Serialize.StringToBrush(value); }
		}



		[XmlIgnore]
		[Display(Name = "Text Color Volume Low", Description = "Represents the color of the swing value output for volume lows.", Order = 7, GroupName = "1. Visualize Volume")]
		public Brush TextColorVolumeLow
		{
			get { return textColorVolumeLow; }
			set { textColorVolumeLow = value; }
		}
		[Browsable(false)]
		public string TextColorVolumeLowSerializable
		{
			get { return Serialize.BrushToString(textColorVolumeLow); }
			set { textColorVolumeLow = Serialize.StringToBrush(value); }
		}





		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Volume", Description = "Represents the text offset in pixel for the swing volume.", Order = 10, GroupName = "1. Visualize Volume")]
		public int TextOffsetVolume
		{
			get { return textOffsetVolume; }
			set { textOffsetVolume = Math.Max(1, value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Text Offset Delta", Description = "Represents the text offset in pixel for the swing delta for the swings.", Order = 1, GroupName = "2. Visualize Delta")]
		public int TextOffsetDelta
		{
			get { return textOffsetDelta; }
			set { textOffsetDelta = Math.Max(1, value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name = "Text Font Delta", Description = "Represents the text font for the swing volume delta value output.", Order = 2, GroupName = "2. Visualize Delta")]
		public NinjaTrader.Gui.Tools.SimpleFont TextFontDelta
		{
			get { return textFontDelta; }
			set { textFontDelta = value; }
		}
		
		[XmlIgnore]
		[Display(Name = "Text Color Positive Delta", Description = "Represents the color of the swing volume delta positive output.", Order = 3, GroupName = "2. Visualize Delta")]
		public Brush TextColorPositiveDelta
		{
			get { return textColorPositiveDelta; }
			set { textColorPositiveDelta = value; }
		}

		[Browsable(false)]
		public string TextColorPositiveDeltaSerializable
		{
			get { return Serialize.BrushToString(textColorPositiveDelta); }
			set { textColorPositiveDelta = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Text Color Negative Delta", Description = "Represents the color of the swing volume delta negative output.", Order = 4, GroupName = "2. Visualize Delta")]
		public Brush TextColorNegativeDelta
		{
			get { return textColorNegativeDelta; }
			set { textColorNegativeDelta = value; }
		}
		[Browsable(false)]
		public string TextColorNegativeDeltaSerializable
		{
			get { return Serialize.BrushToString(textColorNegativeDelta); }
			set { textColorNegativeDelta = Serialize.StringToBrush(value); }
		}


		[XmlIgnore]
		[Display(Name = "Zig-Zag Color Up", Description = "Represents the color of the zig-zag up lines.", Order = 1, GroupName = "3. Zig-Zag Parameters")]
		public Brush ZigZagColorUp
		{
			get { return zigZagColorUp; }
			set { zigZagColorUp = value; }
		}
		[Browsable(false)]
		public string ZigZagColorUpSerializable
		{
			get { return Serialize.BrushToString(zigZagColorUp); }
			set { zigZagColorUp = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(Name = "Zig-Zag Color Down", Description = "Represents the color of the zig-zag down lines.", Order = 2, GroupName = "3. Zig-Zag Parameters")]
		public Brush ZigZagColorDn
		{
			get { return zigZagColorDn; }
			set { zigZagColorDn = value; }
		}
		[Browsable(false)]
		public string ZigZagColorDnSerializable
		{
			get { return Serialize.BrushToString(zigZagColorDn); }
			set { zigZagColorDn = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "Zig-Zag Style", Description = "Represents the line style of the zig-zag lines.", Order = 3, GroupName = "3. Zig-Zag Parameters")]
		public DashStyleHelper ZigZagStyle
		{
			get { return zigZagStyle; }
			set { zigZagStyle = value; }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name = "Zig-Zag Width", Description = "Represents the line width of the zig-zag lines.", Order = 4, GroupName = "3. Zig-Zag Parameters")]
		public int ZigZagWidth
		{
			get { return zigZagWidth; }
			set { zigZagWidth = Math.Max(1, value); }
		}
		

		//=========================================================================================
		#endregion
		
		#region Gann Swings
		//=========================================================================================
		[NinjaScriptProperty]
		[Display(Name = "Ignore Inside Bars", Description = "Indicates if inside bars are ignored. If set to true it is possible that between consecutive up/down bars are inside bars. Only used if calculationSize > 1.", Order = 1, GroupName = "4. Gann Swings")]
		public bool IgnoreInsideBars
		{
			get { return ignoreInsideBars; }
			set { ignoreInsideBars = value; }
		}

		[NinjaScriptProperty]
		[Display(Name = "Use Breakouts", Description = "Indicates if the swings are updated if the last swing high/low is broken. Only used if calculationSize > 1.", Order = 2, GroupName = "4. Gann Swings")]
		public bool UseBreakouts
		{
			get { return useBreakouts; }
			set { useBreakouts = value; }
		}
		//=========================================================================================
		#endregion
		#endregion
		
     
	}
}

namespace WeisWaveDelta.Base
{
	    #region public class SwingValues
	    //=============================================================================================
	    public class Swings
	    {
	        #region Current values
	        //-----------------------------------------------------------------------------------------
	        /// <summary>
	        /// Represents the price of the current swing.
	        /// </summary>
	        public double CurPrice { get; set; }
	        /// <summary>
	        /// Represents the bar number of the highest/lowest bar of the current swing.
	        /// </summary>
	        public int CurBar { get; set; }
	        /// <summary>
	        /// Represents the duration as time values of the current swing.
	        /// </summary>
	        public DateTime CurDateTime { get; set; }
	        /// <summary>
	        /// Represents the duration in bars of the current swing.
	        /// </summary>
	        public int CurDuration { get; set; }
	        /// <summary>
	        /// Represents the swing length in ticks of the current swing.
	        /// </summary>
	        public int CurLength { get; set; }
	        /// <summary>
	        /// Represents the percentage in relation between the last swing and the current swing. 
	        /// E. g. 61.8% fib retracement.
	        /// </summary>
	        public double CurPercent { get; set; }
	        /// <summary>
	        /// Represents the duration as integer in HHMMSS of the current swing.
	        /// </summary>
	        public int CurTime { get; set; }
	        /// <summary>
	        /// Represents the entire volume of the current swing.
	        /// </summary>
	        public long CurVolume { get; set; }
			/// <summary>
	        /// Represents the entire volume delta of the current swing.
	        /// </summary>
	        public long CurDelta { get; set; }
	        /// <summary>
	        /// Represents the relation to the previous swing.
	        /// -1 = Lower High | 0 = Double Top | 1 = Higher High
	        /// </summary>
	        public int CurRelation { get; set; }
	        //-----------------------------------------------------------------------------------------
	        #endregion

	        #region Last values
	        //-----------------------------------------------------------------------------------------
	        /// <summary>
	        /// Represents the price of the last swing.
	        /// </summary>
	        public double LastPrice { get; set; }
	        /// <summary>
	        /// Represents the bar number of the highest/lowest bar of the last swing.
	        /// </summary>
	        public int LastBar { get; set; }
	        /// <summary>
	        /// Represents the duration as time values of the last swing.
	        /// </summary>
	        public DateTime LastDateTime { get; set; }
	        /// <summary>
	        /// Represents the duration in bars of the last swing.
	        /// </summary>
	        public int LastDuration { get; set; }
	        /// <summary>
	        /// Represents the swing length in ticks of the last swing.
	        /// </summary>
	        public int LastLength { get; set; }
	        /// <summary>
	        /// Represents the percentage in relation between the previous swing and the last swing. 
	        /// E. g. 61.8% fib retracement.
	        /// </summary>
	        public double LastPercent { get; set; }
	        /// <summary>
	        /// Represents the duration as integer in HHMMSS of the last swing.
	        /// </summary>
	        public int LastTime { get; set; }
	        /// <summary>
	        /// Represents the entire volume of the last swing.
	        /// </summary>
	        public long LastVolume { get; set; }
			/// <summary>
	        /// Represents the entire volume delta of the last swing.
	        /// </summary>
	        public long LastDelta { get; set; }
	        /// <summary>
	        /// Represents the relation to the previous swing.
	        /// -1 = Lower High | 0 = Double Top | 1 = Higher High
	        /// </summary>
	        public int LastRelation { get; set; }
	        //-----------------------------------------------------------------------------------------
	        #endregion

	        #region Other values
	        //-----------------------------------------------------------------------------------------
	        /// <summary>
	        /// Represents the number of swings.
	        /// </summary>
	        public int Counter { get; set; }
	        /// <summary>
	        /// Indicates if a new swing is found.
	        /// </summary>
	        public bool New { get; set; }
	        /// <summary>
	        /// Indicates if a the current swing is updated.
	        /// </summary>
	        public bool Update { get; set; }
	        /// <summary>
	        /// Represents the volume of the signal bar for the swing.
	        /// </summary>
	        public double SignalBarVolume { get; set; }
			/// <summary>
	        /// Represents the volume delta of the signal bar for the swing.
	        /// </summary>
	        public double SignalBarDelta { get; set; }
	        /// <summary>
	        /// Represents the number of the last swing in the swing list.
	        /// </summary>
	        public int ListCount { get; set; }
	        //-----------------------------------------------------------------------------------------
	        #endregion
	    }
	    //=============================================================================================
	    #endregion

	    #region public class CurrentSwing
	    //=============================================================================================
	    public class SwingCurrent
	    {
	        /// <summary>
	        /// Represents the swing slope direction. -1 = down | 0 = init | 1 = up.
	        /// </summary>
	        public int SwingSlope { get; set; }
	        /// <summary>
	        /// Represents the bar number of the swing slope change bar.
	        /// </summary>
	        public int SwingSlopeChangeBar { get; set; }
	        /// <summary>
	        /// Indicates if a new swing is found. And whether it is a swing high or a swing low.
	        /// Used to control, that either a swing high or a swing low is set for each bar.
	        /// 0 = no swing | -1 = down swing | 1 = up swing
	        /// </summary>
	        public int NewSwing { get; set; }
	        /// <summary>
	        /// Represents the number of consecutives up/down bars.
	        /// </summary>
	        public int ConsecutiveBars { get; set; }
	        /// <summary>
	        /// Represents the bar number of the last bar which was counted to the 
	        /// consecutives up/down bars.
	        /// </summary>
	        public int ConsecutiveBarNumber { get; set; }
	        /// <summary>
	        /// Represents the high/low of the last consecutive bar.
	        /// </summary>
	        public double ConsecutiveBarValue { get; set; }
	        /// <summary>
	        /// Indicates if the outside bar calculation is stopped. Used to avoid an up swing and 
	        /// a down swing in one bar.
	        /// </summary>
	        public bool StopOutsideBarCalc { get; set; }
	    }
	    //=============================================================================================
	    #endregion

		
	    #region public class SwingProperties
	    //=============================================================================================
	    public class SwingProperties
	    {
	        public SwingProperties(double swingSize, int dtbStrength)
	        {
	            SwingSize = swingSize;
	            DtbStrength = dtbStrength;
	        }

	        public SwingProperties(double swingSize, int dtbStrength,
	//	        SwingVolumeStyle swingVolumeType, SwingDeltaStyle swingDeltaType, 
				bool swingVolumeType, bool swingDeltaType, 
	            bool useBreakouts, bool ignoreInsideBars, 
	            bool useAutoScale, Brush zigZagColorUp, Brush zigZagColorDn,
	            DashStyleHelper zigZagStyle, int zigZagWidth, Brush textColorVolumeHigh,
	            Brush textColorVolumeLow, SimpleFont textFont,
				Brush textColorPositiveDelta, Brush textColorNegativeDelta, SimpleFont textFontDelta,
				int textOffsetDelta, int textOffsetVolume, bool useCloseValues, 
				bool drawSwingsOnPricePanel)
	        {
	 
	            SwingSize = swingSize;
	            DtbStrength = dtbStrength;
	            SwingVolumeType = swingVolumeType;
				SwingDeltaType = swingDeltaType;
	            UseBreakouts = useBreakouts;
	            IgnoreInsideBars = ignoreInsideBars;
	            UseAutoScale = useAutoScale;
	            ZigZagColorUp = zigZagColorUp;
	            ZigZagColorDn = zigZagColorDn;
	            ZigZagStyle = zigZagStyle;
	            ZigZagWidth = zigZagWidth;
	            TextColorVolumeHigh = textColorVolumeHigh;
	            TextColorVolumeLow = textColorVolumeLow;
	            TextFont = textFont;
				TextColorPositiveDelta = textColorPositiveDelta;
				TextColorNegativeDelta = textColorNegativeDelta;
				TextFontDelta = textFontDelta;
	            TextOffsetDelta = textOffsetDelta;
	            TextOffsetVolume = textOffsetVolume;
	            UseCloseValues = useCloseValues;
				DrawSwingsOnPricePanel = drawSwingsOnPricePanel;
	        }


	        /// <summary>
	        /// Represents the swing size. e.g. 1 = small swings and 5 = bigger swings.
	        /// </summary>
	        public double SwingSize { get; set; }
	        /// <summary>
	        /// Represents the double top and double bottom strength.
	        /// </summary>
	        public int DtbStrength { get; set; }

	        /// <summary>
	        /// Indicates if the swing price is shown.
	        /// </summary>

	        /// Represents the swing time visualization type.
	        /// </summary>
	
	        /// <summary>
	        /// Represents the swing volume visualization type.
	        /// </summary>
	        public bool SwingVolumeType { get; set; }
			/// <summary>
	        /// Represents the swing delta visualization type.
	        /// </summary>
	        public bool SwingDeltaType { get; set; }
	        /// <summary>
	        /// Represents the swing visualization type. 
	        /// </summary>
	
	        /// <summary>
	        /// Indicates if the Gann swings are updated if the last swing high/low is broken.
	        /// </summary>
	        public bool UseBreakouts { get; set; }
	        /// <summary>
	        /// Indicates if inside bars are ignored for the Gann swing calculation. If set to true 
	        /// it is possible that between consecutive up/down bars are inside bars.
	        /// </summary>
	        public bool IgnoreInsideBars { get; set; }
	        /// <summary>
	        /// Indicates if AutoScale is used. 
	        /// </summary>
	        public bool UseAutoScale { get; set; }
	        /// <summary>
	        /// Represents the colour of the zig-zag up lines.
	        /// </summary>
	        public Brush ZigZagColorUp { get; set; }
	        /// <summary>
	        /// Represents the colour of the zig-zag down lines.
	        /// </summary>
	        public Brush ZigZagColorDn { get; set; }
	        /// <summary>
	        /// Represents the line style of the zig-zag lines.
	        /// </summary>
	        public DashStyleHelper ZigZagStyle { get; set; }
	        /// <summary>
	        /// Represents the line width of the zig-zag lines.
	        /// </summary>
	        public int ZigZagWidth { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for higher highs.
	        /// </summary>
	        public Brush TextColorVolumeHigh { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for lower highs.
	        /// </summary>
//	        public Brush TextColorLowerHigh { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for double tops.
	        /// </summary>
//	        public Brush TextColorDoubleTop { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for higher lows.
	        /// </summary>
	        public Brush TextColorVolumeLow { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for lower lows.
	        /// </summary>
//	        public Brush TextColorLowerLow { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for double bottems.
	        /// </summary>
//	        public Brush TextColorDoubleBottom { get; set; }
	        /// <summary>
	        /// Represents the text font for the swing value output.
	        /// </summary>
			/// <summary>
	        public SimpleFont TextFont { get; set; }
			/// Represents the colour of the swing value output for higher lows.
	        /// </summary>
	        public Brush TextColorPositiveDelta { get; set; }
	        /// <summary>
	        /// Represents the colour of the swing value output for lower lows.
	        /// </summary>
	        public Brush TextColorNegativeDelta { get; set; }
			/// <summary>
	        /// Represents the text font for the swing value output.
	        /// </summary>
	        public SimpleFont TextFontDelta { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the swing length.
	        /// </summary>

	        /// Represents the text offset in pixel for the swing price.
	        /// </summary>
	        public int TextOffsetDelta { get; set; }
	        /// <summary>
	        /// Represents the text offset in pixel for the swing labels.
	        /// </summary>

	        /// Represents the text offset in pixel for the swing volume.
	        /// </summary>
	        public int TextOffsetVolume { get; set; }
	        /// <summary>
	        /// Indicates if high and low prices are used for the swing calculations or close values.
	        /// </summary>
	        public bool UseCloseValues { get; set; }
	        /// <summary>
	        /// Indicates if the swings are drawn on the price panel.
	        /// </summary>
	        public bool DrawSwingsOnPricePanel { get; set; }
	    }
	    //=============================================================================================
	    #endregion

	    #region public struct SwingStruct
	    //=============================================================================================
	    public struct SwingStruct
	    {
	        /// <summary>
	        /// Swing price.
	        /// </summary>
	        public double price;
	        /// <summary>
	        /// Swing bar number.
	        /// </summary>
	        public int barNumber;
	        /// <summary>
	        /// Swing time.
	        /// </summary>
	        public DateTime time;
	        /// <summary>
	        /// Swing duration in bars.
	        /// </summary>
	        public int duration;
	        /// <summary>
	        /// Swing length in ticks.
	        /// </summary>
	        public int length;
	        /// <summary>
	        /// Swing relation.
	        /// -1 = Lower | 0 = Double | 1 = Higher
	        /// </summary>
	        public int relation;
	        /// <summary>
	        /// Swing volume.
	        /// </summary>
	        public long volume;
			 /// <summary>
	        /// Swing volume delta.
	        /// </summary>
	        public double delta;

	        public SwingStruct(double swingPrice, int swingBarNumber, DateTime swingTime,
	                int swingDuration, int swingLength, int swingRelation, long swingVolume, double swingDelta)
	        {
	            price = swingPrice;
	            barNumber = swingBarNumber;
	            time = swingTime;
	            duration = swingDuration;
	            length = swingLength;
	            relation = swingRelation;
	            volume = swingVolume;
				delta = swingDelta;
	        }
	    }
	    //=============================================================================================
	    #endregion

	    #region Enums
	    //=============================================================================================

	

	    //=============================================================================================
	    #endregion
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WeisWaveDelta[] cacheWeisWaveDelta;
		public WeisWaveDelta WeisWaveDelta(double swingSize, bool useCloseValues, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetVolume, int textOffsetDelta, NinjaTrader.Gui.Tools.SimpleFont textFontDelta, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return WeisWaveDelta(Input, swingSize, useCloseValues, textFont, textOffsetVolume, textOffsetDelta, textFontDelta, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}

		public WeisWaveDelta WeisWaveDelta(ISeries<double> input, double swingSize, bool useCloseValues, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetVolume, int textOffsetDelta, NinjaTrader.Gui.Tools.SimpleFont textFontDelta, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			if (cacheWeisWaveDelta != null)
				for (int idx = 0; idx < cacheWeisWaveDelta.Length; idx++)
					if (cacheWeisWaveDelta[idx] != null && cacheWeisWaveDelta[idx].SwingSize == swingSize && cacheWeisWaveDelta[idx].UseCloseValues == useCloseValues && cacheWeisWaveDelta[idx].TextFont == textFont && cacheWeisWaveDelta[idx].TextOffsetVolume == textOffsetVolume && cacheWeisWaveDelta[idx].TextOffsetDelta == textOffsetDelta && cacheWeisWaveDelta[idx].TextFontDelta == textFontDelta && cacheWeisWaveDelta[idx].ZigZagStyle == zigZagStyle && cacheWeisWaveDelta[idx].ZigZagWidth == zigZagWidth && cacheWeisWaveDelta[idx].IgnoreInsideBars == ignoreInsideBars && cacheWeisWaveDelta[idx].UseBreakouts == useBreakouts && cacheWeisWaveDelta[idx].EqualsInput(input))
						return cacheWeisWaveDelta[idx];
			return CacheIndicator<WeisWaveDelta>(new WeisWaveDelta(){ SwingSize = swingSize, UseCloseValues = useCloseValues, TextFont = textFont, TextOffsetVolume = textOffsetVolume, TextOffsetDelta = textOffsetDelta, TextFontDelta = textFontDelta, ZigZagStyle = zigZagStyle, ZigZagWidth = zigZagWidth, IgnoreInsideBars = ignoreInsideBars, UseBreakouts = useBreakouts }, input, ref cacheWeisWaveDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WeisWaveDelta WeisWaveDelta(double swingSize, bool useCloseValues, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetVolume, int textOffsetDelta, NinjaTrader.Gui.Tools.SimpleFont textFontDelta, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.WeisWaveDelta(Input, swingSize, useCloseValues, textFont, textOffsetVolume, textOffsetDelta, textFontDelta, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}

		public Indicators.WeisWaveDelta WeisWaveDelta(ISeries<double> input , double swingSize, bool useCloseValues, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetVolume, int textOffsetDelta, NinjaTrader.Gui.Tools.SimpleFont textFontDelta, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.WeisWaveDelta(input, swingSize, useCloseValues, textFont, textOffsetVolume, textOffsetDelta, textFontDelta, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WeisWaveDelta WeisWaveDelta(double swingSize, bool useCloseValues, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetVolume, int textOffsetDelta, NinjaTrader.Gui.Tools.SimpleFont textFontDelta, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.WeisWaveDelta(Input, swingSize, useCloseValues, textFont, textOffsetVolume, textOffsetDelta, textFontDelta, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}

		public Indicators.WeisWaveDelta WeisWaveDelta(ISeries<double> input , double swingSize, bool useCloseValues, NinjaTrader.Gui.Tools.SimpleFont textFont, int textOffsetVolume, int textOffsetDelta, NinjaTrader.Gui.Tools.SimpleFont textFontDelta, DashStyleHelper zigZagStyle, int zigZagWidth, bool ignoreInsideBars, bool useBreakouts)
		{
			return indicator.WeisWaveDelta(input, swingSize, useCloseValues, textFont, textOffsetVolume, textOffsetDelta, textFontDelta, zigZagStyle, zigZagWidth, ignoreInsideBars, useBreakouts);
		}
	}
}

#endregion
