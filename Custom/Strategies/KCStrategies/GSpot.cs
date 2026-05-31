#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.KCStrategies
{
    public class GSpot : KCAlgoBase
    {
        // Parameters
		private HiLoBands HiLoBands1; 
        private Series<double> highestHigh;
        private Series<double> lowestLow;
		private Series<double> highestHigh2;
        private Series<double> lowestLow2;
		private Series<double> midline;
		
	    private double target1Ticks = 100; // Profit Target 1 in ticks
	    private double target2Ticks = 200; // Profit Target 2 in ticks
	    private double stopLossTicks = 100; // Stop-loss in ticks
		private bool enableDrawLines = true; // Enable line drawing
	
	    // Session range tracking
	    private double dailyHigh = 0;
	    private double dailyLow = 0;
		
		// Properties for text rendering
	    public bool TextBold { get; set; }
	    public int TextSize { get; set; }
		public int LineWidth { get; set; }
	    public byte TextOpacity { get; set; }
		
		private bool longSignal = false;
        private bool shortSignal = false;

		public override string DisplayName { get { return Name; } }
		
        protected override void OnStateChange()
        {
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = "This strategy is based on the Golden Level Setup and HiLo indicator.";
                Name = "GSpot v4.0";
                StrategyName = "GSpot";
                Version = "4.0 Jan. 2025";
                Credits = "Strategy by Khanh Nguyen";
                ChartType = "TDU Renko Backtest 70-2";	
				
				// Text properties
	            TextBold 		= true;
	            TextSize 		= 15;
				LineWidth 		= 3;
	            TextOpacity 	= 180;
				
				HiLoFastPeriod	= 4;
				HiLoSlowPeriod	= 6;
				Width			= 2;
				showHiLo		= false;
				
                InitialStop		= 144;
				ProfitTarget	= 52;
				
                activeOrder 	= false;
            }
			else if (State == State.Configure)
			{
				highestHigh = new Series<double> (this);
				lowestLow = new Series<double> (this);
				highestHigh2 = new Series<double> (this);
				lowestLow2 = new Series<double> (this);
				midline = new Series<double> (this);
			}
            else if (State == State.DataLoaded)
            {
                InitializeIndicators();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade)
                return;
            
			 // Update the daily high and low dynamically
		    if (Time[0].Date != Time[1].Date) // New session
		    {
		        dailyHigh = 0;
		        dailyLow = double.MaxValue;
		        RemoveDrawObjects(); // Remove old lines for a new session
		    }
		
		    dailyHigh = Math.Max(dailyHigh, High[0]);
		    dailyLow = Math.Min(dailyLow, Low[0]);	
			
			highestHigh[0] = HiLoBands1.Values[0][0];
			lowestLow[0] = HiLoBands1.Values[1][0];
			
			highestHigh2[0] = HiLoBands1.Values[2][0];
			lowestLow2[0] = HiLoBands1.Values[3][0];
			
		    uptrend =  Close[0] > Close[1];
		    downtrend = Close[0] < Close[1];
			
			// Verifica si hay alguna posición abierta en la cuenta
			bool hasOpenPosition = false;
			
			foreach (var position in Account.Positions)
			{
			    if (position.MarketPosition != MarketPosition.Flat)
			    {
			        hasOpenPosition = true;
			        break; // Si encontramos una posición abierta, salimos del bucle
			    }
			}
			
			if (hasOpenPosition && isLong) isLong = false;
			if (hasOpenPosition && isShort) isShort = false;
		
		    // Calculate all valid "Spot" levels
		    double spotIncrement = 100; // Increment for MNQ levels (e.g., 21526, 21626)
		    double firstSpot = Math.Floor(dailyLow / spotIncrement) * spotIncrement + 26;
		    double lastSpot = Math.Ceiling(dailyHigh / spotIncrement) * spotIncrement + 26;
		
		    // Draw lines dynamically for each level
		    for (double spot = firstSpot; spot <= lastSpot; spot += spotIncrement)
		    {
		        if (enableDrawLines)
		        {
		            Draw.HorizontalLine(this, $"HorizontalLine_26_{spot}", spot, Brushes.Yellow, DashStyleHelper.Solid, LineWidth);
		            Draw.HorizontalLine(this, $"HorizontalLine_50_{spot + 24}", spot + 24, Brushes.Cyan, DashStyleHelper.Solid, LineWidth);
		            Draw.HorizontalLine(this, $"HorizontalLine_77_{spot + 51}", spot + 51, Brushes.Yellow, DashStyleHelper.Solid, LineWidth);
					Draw.HorizontalLine(this, $"HorizontalLine_100_{spot + 74}", spot + 74, Brushes.Cyan, DashStyleHelper.Solid, LineWidth);
		        }
		    }
		
		    // Iterate through all possible spots to find a match
		    for (double spot = firstSpot; spot <= lastSpot; spot += spotIncrement)
		    {
		        // Long entry logic for 26 Spot
		        if (Close[0] >= spot && Low[0] <= spot && uptrend) // Long entry for 26 Spot
		        {
					longSignal = true;
		        }
				
				// Short entry logic for 26 Spot
		        if (Close[0] <= spot && High[0] >= spot && downtrend) // Short entry for 26 Spot
		        {
					shortSignal = true;
		        }
		
		        // Long entry logic for 77 Spot
		        if (Close[0] >= (spot + 51) && Low[0] <= (spot + 51) && uptrend) // Long entry for 77 Spot
		        {
					longSignal = true;
		        }
				
				// Short entry logic for 77 spot
		        if (Close[0] <= (spot + 51) && High[0] >= (spot + 51) && downtrend) // Short entry for 77 Spot
		        {
					shortSignal = true;
		        }
		    }
			
			longSignal = ((highestHigh[0] == highestHigh2[0] && highestHigh[1] != highestHigh2[1] && highestHigh[0] > highestHigh[1])
				|| (highestHigh2[0] > highestHigh2[1] && highestHigh2[1] == highestHigh2[2])
				|| (highestHigh[0] > highestHigh[1] && highestHigh[1] > highestHigh[2])
				|| (midline[0] > midline[1] && midline[1] < midline[2])
				|| (lowestLow[0] == lowestLow[1] && lowestLow[1] < lowestLow[2]))
				&& (uptrend);
			
            shortSignal = ((lowestLow[0] == lowestLow2[0] && lowestLow[1] != lowestLow2[1] && lowestLow[0] < lowestLow[1])
				|| (lowestLow2[0] < lowestLow2[1] && lowestLow2[1] == lowestLow2[2])
				|| (lowestLow[0] < lowestLow[1] && lowestLow[1] < lowestLow[2])
				|| (midline[0] < midline[1] && midline[1] > midline[2])
				|| (highestHigh[0] == highestHigh[1] && highestHigh[1] > highestHigh[2]))
				&& (downtrend);
			
			
			exitLong = isLong && highestHigh[0] < highestHigh[1] && highestHigh[1] == highestHigh[2];
			exitShort = isShort && lowestLow[0] > lowestLow[1] && lowestLow[1] == lowestLow[2];					
			
			base.OnBarUpdate();
        }

		protected override void OnExecutionUpdate(Execution execution, string executionId, double price,
                                                  int quantity, MarketPosition marketPosition, string orderId,
                                                  DateTime time)
        {
            // Reset entry flags upon exit
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                longSignal = false;
                shortSignal = false;
            }
        }
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
	    {
	        // Loop through all horizontal lines
	        foreach (var co in ChartControl.ChartObjects)
	        {
	            if (co is HorizontalLine horizontalLine)
	            {
	                string priceFormat = Core.Globals.GetTickFormatString(TickSize);
	                float x, y;
	                System.Windows.Media.Color chartBackgroundColor = ((System.Windows.Media.SolidColorBrush)ChartControl.Properties.ChartBackground).Color;
	
	                using (var backBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color(chartBackgroundColor.R, chartBackgroundColor.G, chartBackgroundColor.B, TextOpacity / 255.0f)))
	                using (var textBrush = horizontalLine.Stroke.Brush.ToDxBrush(RenderTarget))
	                using (var factory = new SharpDX.DirectWrite.Factory())
	                using (var textFormat = new TextFormat(factory, "Arial", TextBold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, TextSize * 96 / 72))
	                {
	                    string text = horizontalLine.Anchors.First().Price.ToString(priceFormat);
	                    using (var textLayout = new SharpDX.DirectWrite.TextLayout(factory, text, textFormat, float.MaxValue, float.MaxValue))
	                    {
	                        // Position the text above the line
	                        x = 5; // Slight padding from the left
	                        y = chartScale.GetYByValue(horizontalLine.Anchors.First().Price) - textLayout.Metrics.Height - 2;
	
	                        // Render background and text
	                        RenderTarget.FillRectangle(new RectangleF(x, y, textLayout.Metrics.Width, textLayout.Metrics.Height), backBrush);
	                        RenderTarget.DrawText(text, textFormat, new SharpDX.RectangleF(x, y, textLayout.Metrics.Width, textLayout.Metrics.Height), textBrush);
	                    }
	                }
	            }
	        }
	    }

        protected override bool ValidateEntryLong()
        {
            // Logic for validating long entries
			if (longSignal) return true;
			else return false;
        }

        protected override bool ValidateEntryShort()
        {
            // Logic for validating short entries
			if (shortSignal) return true;
            else return false;
        }

       	protected override bool ValidateExitLong()
        {
            // Logic for validating long exits
            return enableExit? true : false;
        }

        protected override bool ValidateExitShort()
        {
			// Logic for validating short exits
			return enableExit? true : false;
        }

        #region Indicators
        protected override void InitializeIndicators()
        {
					
				dailyHigh = 0;
	            dailyLow = double.MaxValue;
				
				HiLoBands1				= HiLoBands(HiLoFastPeriod, HiLoSlowPeriod, Width);
				HiLoBands1.Plots[0].Brush = Brushes.Cyan;
				HiLoBands1.Plots[1].Brush = Brushes.Magenta;
				HiLoBands1.Plots[2].Brush = Brushes.Lime;
				HiLoBands1.Plots[3].Brush = Brushes.Red;
				if (showHiLo) AddChartIndicator(HiLoBands1);
        }
        #endregion

        #region Properties
		
		[NinjaScriptProperty]
        [Display(Name = "HiLo Fast Period", Order = 1, GroupName="Filter Settings")]
        public int HiLoFastPeriod { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "HiLo Slow Period", Order = 2, GroupName="Filter Settings")]
        public int HiLoSlowPeriod { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Line Width", Order = 3, GroupName="Filter Settings")]
        public int Width { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Show High Low", Order = 4, GroupName = "Filter Settings")]
        public bool showHiLo { get; set; }
		

        #endregion
    }
}
