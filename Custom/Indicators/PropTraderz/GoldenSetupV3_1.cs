
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
using System.Windows.Media;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
using SharpDX;
#endregion
/*
// GoldenSetupV2 Indicator
Developed by: PropTraderz
 * This code is for personal use only and cannot be sold, redistributed, 
 * or used for commercial purposes without explicit permission from the author.
 * Can be found also at WWW.PROPTRADERZ.COM
// Created by PropTraderz
// The GoldenSetupV2 indicator is designed to identify and display key price levels
// within 100-handle price ranges, highlighting significant support and resistance zones. 
// These levels include 12, 33, 88, and others such as 26, 50, and 77, all calculated 
// dynamically based on the current market price. The indicator uses colored horizontal 
// lines for easy visual identification, helping traders spot important price inflection 
// points. 
// Customization options such as text size, position, and opacity allow traders to tailor 
// the appearance of the levels to fit their charts. The indicator is especially useful 
// for intraday trading, providing insights into potential trade entries or exits at key 
// price levels. GoldenSetupV2 is a vital tool for traders looking to identify reaction 
// levels where price is likely to reverse or consolidate.
// Vendor License: 2927434660904228A1F7C70197E90968
// www.proptraderz.com
// Discord : https://discord.gg/NAekcZXbam
*/

namespace NinjaTrader.NinjaScript.Indicators.PropTraderz
{
    public class GoldenSetupV3_1 : Indicator
    {
        // Variables for HorizontalLabels
        public enum SideEnum { Left, Right }
        public enum PositionEnum { Below, Above }

        public SideEnum Side { get; set; }
        public PositionEnum Position { get; set; }
        public int Size { get; set; }
        public bool Bold { get; set; }
        public byte Opacity { get; set; }

        // Variables for the Levels 		
        public int LevelA { get; set; }
        public int LevelB { get; set; }
        public int LevelC { get; set; }
        public int LevelD { get; set; }
        public int LevelE { get; set; } 
        public int LevelF { get; set; } 
        public int LevelG { get; set; }
        public int LevelH { get; set; } 

        public Stroke LevelALine { get; set; }
        public Stroke LevelBLine { get; set; }
        public Stroke LevelCLine { get; set; }
        public Stroke LevelDLine { get; set; }
        public Stroke LevelELine { get; set; }
        public Stroke LevelFLine { get; set; }
        public Stroke LevelGLine { get; set; } 
        public Stroke LevelHLine { get; set; } 
		
        private string discordInfo = "https://discord.gg/NAekcZXbam";
		
		private ptzGLManager glManager = new ptzGLManager(); // Instance of GLManager
        private double LevelAValue;
        private double LevelBValue;
        private double LevelCValue;
        private double LevelDValue;
        private double LevelEValue;
        private double LevelFValue;
        private double LevelGValue;
        private double LevelHValue;		
		
		// Watermark variables
        private readonly string watermarkText = "PropTraderz";
        private readonly System.Windows.Media.Brush watermarkBrush = System.Windows.Media.Brushes.SeaGreen;		
		
    //	Signals related variables
		private bool isLong, isShort;
        private bool longTargetReached, shortTargetReached;		

       

		
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Levels ndicator for the Golden Set up of Tony Rago with signals";
                Name = "GoldenSetupV3.1";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;

                // HorzLabel Settings
                Side = SideEnum.Right;
                Size = 15;
                Position = PositionEnum.Above;
                Bold = true;
                Opacity = 180;

                // TheoTrade Levels Settings (Added the new levels)
                LevelA = 0;  // Roundy
                LevelB = 26; // 26 Level
                LevelC = 50; // 50 Level
                LevelD = 77; // 77 Level
                LevelE = 12; // target 12 Level
                LevelF = 33; // target 33 Level
                LevelG = 62; // target 33 Level                
                LevelH = 88; // target 88 Level

                LevelALine = new Stroke(Brushes.DarkCyan, DashStyleHelper.Solid, 2);
		        LevelBLine = new Stroke(Brushes.Gold, DashStyleHelper.Dash, 4); // Dash style and thicker for 26
		        LevelCLine = new Stroke(Brushes.DarkCyan, DashStyleHelper.Solid, 2);
		        LevelDLine = new Stroke(Brushes.Gold, DashStyleHelper.Dash, 4); // Dash style and thicker for 77
		        LevelELine = new Stroke(Brushes.Silver, DashStyleHelper.Solid, 2);
		        LevelFLine = new Stroke(Brushes.Silver, DashStyleHelper.Solid, 2);
		        LevelHLine = new Stroke(Brushes.Silver, DashStyleHelper.Solid, 2);
		        LevelGLine = new Stroke(Brushes.Silver, DashStyleHelper.Solid, 2); //62 level

                // Default settings for watermark
                ShowWatermark = true;
                ShowNearestLevel = true;
                NearestLevelPosition = TextPosition.TopRight;

                // --------------------------------------------------------------------
                // SET DEFAULTS FOR NEW COLOR PROPERTIES
                // --------------------------------------------------------------------
                BuySignalColor = Brushes.LimeGreen;
                SellSignalColor = Brushes.Red;
                TargetSignalColor = Brushes.Cyan;
                // --------------------------------------------------------------------

                AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "InvisiblePlot");
            }
			if (State == State.Configure)
			{
				glManager = new ptzGLManager();
			}			
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1) return;

			if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
			{
				glManager.Clear();
			}			
			
			if (IsFirstTickOfBar)
			{	
	            // Get the current price and determine the nearest 100-handle block
	            double price = Close[0];
	            double priceInt = Math.Floor(price / 100);

	            // Calculate levels within the current 100-handle block
	            LevelAValue = (priceInt * 100) + LevelA;
				ptzGL gl = new ptzGL(LevelAValue.ToString(), "SILVER", LevelAValue, Time[0], Close[0]);
				glManager.AddGL(gl);
	            LevelBValue = (priceInt * 100) + LevelB;
				gl = new ptzGL(LevelBValue.ToString(), "GOLDEN", LevelBValue, Time[0], Close[0]);
				glManager.AddGL(gl);
				LevelCValue = (priceInt * 100) + LevelC;
				gl = new ptzGL(LevelCValue.ToString(), "SILVER", LevelCValue, Time[0], Close[0]);
				glManager.AddGL(gl);            
				LevelDValue = (priceInt * 100) + LevelD;
				gl = new ptzGL(LevelDValue.ToString(), "GOLDEN", LevelDValue, Time[0], Close[0]);
				glManager.AddGL(gl);            
				LevelEValue = (priceInt * 100) + LevelE;
				gl = new ptzGL(LevelEValue.ToString(), "SILVER", LevelEValue, Time[0], Close[0]);
				glManager.AddGL(gl);            
				LevelFValue = (priceInt * 100) + LevelF;
				gl = new ptzGL(LevelFValue.ToString(), "SILVER", LevelFValue, Time[0], Close[0]);
				glManager.AddGL(gl);            
				LevelGValue = (priceInt * 100) + LevelG;
				gl = new ptzGL(LevelGValue.ToString(), "SILVER", LevelGValue, Time[0], Close[0]);
				glManager.AddGL(gl);      
				LevelHValue = (priceInt * 100) + LevelH;
				gl = new ptzGL(LevelHValue.ToString(), "SILVER", LevelHValue, Time[0], Close[0]);
				glManager.AddGL(gl);
				
	            // Adjust if the price is below the current block's lowest level
	            if (price < LevelAValue)
	            {
	                LevelAValue -= 100;
					gl = new ptzGL(LevelAValue.ToString(), "SILVER", LevelAValue, Time[0], Close[0]);
					glManager.AddGL(gl);
	                LevelBValue -= 100;
					gl = new ptzGL(LevelBValue.ToString(), "GOLDEN", LevelBValue, Time[0], Close[0]);
					glManager.AddGL(gl);				
	                LevelCValue -= 100;
					gl = new ptzGL(LevelCValue.ToString(), "SILVER", LevelCValue, Time[0], Close[0]);
					glManager.AddGL(gl);                
					LevelDValue -= 100;
					gl = new ptzGL(LevelDValue.ToString(), "GOLDEN", LevelDValue, Time[0], Close[0]);				
					glManager.AddGL(gl);               
					LevelEValue -= 100;
					gl = new ptzGL(LevelEValue.ToString(), "SILVER", LevelEValue, Time[0], Close[0]);				
					glManager.AddGL(gl);  				         
					LevelFValue -= 100;
					gl = new ptzGL(LevelFValue.ToString(), "SILVER", LevelFValue, Time[0], Close[0]);				
					glManager.AddGL(gl);              
					LevelGValue -= 100;
					gl = new ptzGL(LevelGValue.ToString(), "SILVER", LevelGValue, Time[0], Close[0]);				
					glManager.AddGL(gl); 
					LevelHValue -= 100;
					gl = new ptzGL(LevelHValue.ToString(), "SILVER", LevelHValue, Time[0], Close[0]);				
					glManager.AddGL(gl); 
	            }		
				
	            // Draw the horizontal lines for each level (including new ones)
	            Draw.HorizontalLine(this, "LevelA" + LevelAValue, LevelAValue, LevelALine.Brush, LevelALine.DashStyleHelper, (int)LevelALine.Width);
	            Draw.HorizontalLine(this, "LevelB" + LevelBValue, LevelBValue, LevelBLine.Brush, LevelBLine.DashStyleHelper, (int)LevelBLine.Width);
	            Draw.HorizontalLine(this, "LevelC" + LevelCValue, LevelCValue, LevelCLine.Brush, LevelCLine.DashStyleHelper, (int)LevelCLine.Width);
	            Draw.HorizontalLine(this, "LevelD" + LevelDValue, LevelDValue, LevelDLine.Brush, LevelDLine.DashStyleHelper, (int)LevelDLine.Width);
	            Draw.HorizontalLine(this, "LevelE" + LevelEValue, LevelEValue, LevelELine.Brush, LevelELine.DashStyleHelper, (int)LevelELine.Width); // 12 Level
	            Draw.HorizontalLine(this, "LevelF" + LevelFValue, LevelFValue, LevelFLine.Brush, LevelFLine.DashStyleHelper, (int)LevelFLine.Width); // 33 Level
	            Draw.HorizontalLine(this, "LevelG" + LevelGValue, LevelGValue, LevelGLine.Brush, LevelGLine.DashStyleHelper, (int)LevelGLine.Width); // 33 Level
				Draw.HorizontalLine(this, "LevelH" + LevelHValue, LevelHValue, LevelHLine.Brush, LevelHLine.DashStyleHelper, (int)LevelHLine.Width); // 88 Level				
				
				ptzGL myGL = glManager.GetNearestLevel(price);
				if (myGL != null)
				{
				    if (ShowNearestLevel) Draw.TextFixed(this, "nstGL", "\n" + "Nearest Golden Spot: " + myGL.spotPrice.ToString(), NearestLevelPosition);
				}
				else
				{
				    if (ShowNearestLevel) Draw.TextFixed(this, "nstGL", "\n" + "No Golden Spot Found", NearestLevelPosition);
				}			
			
				if (ShowEntrySignal)
				{
		            // Long Signal Logic
		            if (CrossAbove(Close, LevelEValue, 1)) 
		            {
		                isLong = true;
		                isShort = false; // Reset short flag
		                longTargetReached = false; // Reset long target flag

		                // Replaced Brushes.White with BuySignalColor
		                Draw.ArrowUp(this, "LongEntry" + CurrentBar, false, 0, Low[0] - (4 * TickSize), BuySignalColor);
						Draw.Text(this, "LongEntryText" + CurrentBar, false, "L", 0, Low[0] - ((textSignalOffset + 14) * TickSize), 0, 
						          BuySignalColor, new SimpleFont("Arial", textSignalSize), System.Windows.TextAlignment.Center, 
						          Brushes.Transparent, Brushes.Transparent, textSignalOpacity);
		            }
		
		            // Short Signal Logic
		            if (CrossBelow(Close, LevelHValue, 1)) 
		            {
		                isShort = true;
		                isLong = false; // Reset long flag
		                shortTargetReached = false; // Reset short target flag

		                // Replaced Brushes.White with SellSignalColor
		                Draw.ArrowDown(this, "ShortEntry" + CurrentBar, false, 0, High[0] + (4 * TickSize), SellSignalColor);
						Draw.Text(this, "ShortEntryText" + CurrentBar, false, "S", 0, High[0] + ((textSignalOffset + 14) * TickSize), 0, 
						          SellSignalColor, new SimpleFont("Arial", textSignalSize), System.Windows.TextAlignment.Center, 
						          Brushes.Transparent, Brushes.Transparent, textSignalOpacity);
		            }
		
		            // Targets for Long Position
		            if (isLong && !longTargetReached)
		            {
		                if (CrossAbove(Close, LevelFValue, 1)) 
		                {
		                    // Replaced Brushes.Cyan with TargetSignalColor
		                    Draw.Dot(this, "LT1" + CurrentBar, false, 0, LevelFValue, TargetSignalColor);
							Draw.Text(this, "T1" + CurrentBar, false, "T1", 0, High[0] + (textSignalOffset * TickSize), 0, 
							          TargetSignalColor, new SimpleFont("Arial", textSignalSize), System.Windows.TextAlignment.Center, 
							          Brushes.Transparent, Brushes.Transparent, textSignalOpacity);
		                }
		                if (CrossAbove(Close, LevelCValue, 1)) 
		                {
		                    Draw.Dot(this, "LT2" + CurrentBar, false, 0, LevelCValue, TargetSignalColor);
							Draw.Text(this, "T2" + CurrentBar, false, "T2", 0, High[0] + (textSignalOffset * TickSize), 0, 
							          TargetSignalColor, new SimpleFont("Arial", textSignalSize), System.Windows.TextAlignment.Center, 
							          Brushes.Transparent, Brushes.Transparent, textSignalOpacity);							
		                    longTargetReached = true; // Mark that both targets for this long position have been hit
		                }
		            }
		
		            // Targets for Short Position
		            if (isShort && !shortTargetReached)
		            {
		                if (CrossBelow(Close, LevelDValue, 1)) 
		                {
		                    Draw.Dot(this, "ST1" + CurrentBar, false, 0, LevelDValue, TargetSignalColor);
							Draw.Text(this, "T1" + CurrentBar, false, "T1", 0, Low[0] - (textSignalOffset * TickSize), 0, 
							          TargetSignalColor, new SimpleFont("Arial", textSignalSize), System.Windows.TextAlignment.Center, 
							          Brushes.Transparent, Brushes.Transparent, textSignalOpacity);
		                }
		                if (CrossBelow(Close, LevelCValue, 1)) 
		                {
		                    Draw.Dot(this, "ST2" + CurrentBar, false, 0, LevelCValue, TargetSignalColor);
							Draw.Text(this, "T2" + CurrentBar, false, "T2", 0, Low[0] - (textSignalOffset * TickSize), 0, 
							          TargetSignalColor, new SimpleFont("Arial", textSignalSize), System.Windows.TextAlignment.Center, 
							          Brushes.Transparent, Brushes.Transparent, textSignalOpacity);
							
		                    shortTargetReached = true; // Mark that both targets for this short position have been hit
		                }
		            }					
				}	
			}			
        }
		
		
		// Add properties to control the nearest level visibility and position
		[NinjaScriptProperty]
		[Display(Name = "Show Nearest Level", Order = 0, GroupName = "Info", Description = "Show or hide the nearest level text")]
		public bool ShowNearestLevel { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Nearest Level Position", Order = 1, GroupName = "Info", Description = "Position of the nearest level text")]
		public TextPosition NearestLevelPosition { get; set; } = TextPosition.TopLeft;
		
		[NinjaScriptProperty]
		[Display(Name = "Show Entries signals?", Order = 2, GroupName = "Info", Description = "Show or hide entrries signals")]
		public bool ShowEntrySignal { get; set; }			
		
		
		
		// Add properties to visual control of signal text 
		[NinjaScriptProperty]
		[Display(Name = "Text Size", Order = 3, GroupName = "Info", Description = "Text size for signals info")]
		public int textSignalSize { get; set; }	= 12;		

		[NinjaScriptProperty]
		[Display(Name = "Text Offset", Order = 4, GroupName = "Info", Description = "Text offset")]
		public int textSignalOffset { get; set; }	= 6;			
		
		// Add properties to visual control of signal text 
		[NinjaScriptProperty]
		[Display(Name = "Text Opacity", Order = 5, GroupName = "Info", Description = "Text opacity")]
		public int textSignalOpacity { get; set; }	= 30;				
		
		 // --------------------------------------------------------------------
        // NEW PROPERTIES FOR CUSTOM SIGNAL COLORS
        // --------------------------------------------------------------------
        [NinjaScriptProperty]
        [TypeConverter(typeof(BrushConverter))]
        [Display(Name = "Buy Signal Color", Order = 6, GroupName = "Info", Description = "Color used for Buy signals")]
        public System.Windows.Media.Brush BuySignalColor { get; set; }

        [NinjaScriptProperty]
        [TypeConverter(typeof(BrushConverter))]
        [Display(Name = "Sell Signal Color", Order = 7, GroupName = "Info", Description = "Color used for Sell signals")]
        public System.Windows.Media.Brush SellSignalColor { get; set; }

        [NinjaScriptProperty]
        [TypeConverter(typeof(BrushConverter))]
        [Display(Name = "Target Signal Color", Order = 8, GroupName = "Info", Description = "Color used for Targets (T1, T2)")]
        public System.Windows.Media.Brush TargetSignalColor { get; set; }
        // --------------------------------------------------------------------
		
		[NinjaScriptProperty]
		[Display(Name = "Show Watermark", Order = 9, GroupName = "Info", Description = "Show or hide the watermark")]
		public bool ShowWatermark { get; set; } = true;
		
        [Display(Name = "Discord Info", Order = 10, GroupName = "Info", Description = "The Discord information")]
        public string DiscordInfo
        {
            get { return discordInfo; }
            set { discordInfo = value; }
        }

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            base.OnRender(chartControl, chartScale);

            if (IsInHitTest || Bars == null || ChartPanel == null)
                return;

            // Render the watermark only if ShowWatermark is true
            if (ShowWatermark)
            {
                Draw.TextFixed(
                    this,
                    "Watermark",
                    watermarkText,
                    TextPosition.BottomLeft,
                    Brushes.SeaGreen,
                    new SimpleFont("Arial", 25) { Bold = true }, // Set the font to bold
                    Brushes.Transparent,
                    Brushes.Transparent,
                    0);
            }

            foreach (var co in ChartControl.ChartObjects)
            {
                if (co is HorizontalLine)
                {
                    HorizontalLine l1 = (co as HorizontalLine);
                    string priceformat = Core.Globals.GetTickFormatString(TickSize);

                    float x, y;
                    System.Windows.Media.Color col = ((System.Windows.Media.SolidColorBrush)ChartControl.Properties.ChartBackground).Color;
                    using (SharpDX.Direct2D1.Brush backbr = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, new SharpDX.Color() { A = Opacity, R = col.R, G = col.G, B = col.B }))
                    using (SharpDX.Direct2D1.Brush br = l1.Stroke.Brush.ToDxBrush(RenderTarget))
                    using (var factory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared))
                    using (var textFormat = new SharpDX.DirectWrite.TextFormat(factory, "Arial", Bold ? SharpDX.DirectWrite.FontWeight.Bold : SharpDX.DirectWrite.FontWeight.Normal, SharpDX.DirectWrite.FontStyle.Normal, Size * 96 / 72))
                    {
                        string text = l1.Anchors.First().Price.ToString(priceformat);
                        using (var textLayout = new SharpDX.DirectWrite.TextLayout(factory, text, textFormat, float.MaxValue, float.MaxValue))
                        {
                            x = (Side == SideEnum.Right) ? ChartPanel.W - textLayout.Metrics.Width : 0;
                            y = chartScale.GetYByValue(l1.Anchors.First().Price);

                            if (Position == PositionEnum.Above)
                            {
                                y -= textLayout.Metrics.Height + l1.Stroke.Width / 2.0f;
                            }
                            else
                            {
                                y += l1.Stroke.Width / 2.0f;
                            }

                            RenderTarget.FillRectangle(new SharpDX.RectangleF(x, y, textLayout.Metrics.Width, textLayout.Metrics.Height), backbr);
                            RenderTarget.DrawText(text, textFormat, new SharpDX.RectangleF(x, y, textLayout.Metrics.Width, textLayout.Metrics.Height), br);
                        }
                    }
                }
            }
        }

        private double CalculatePriceLevelFromCurrentPrice(double currentPrice)
        {
            return Math.Floor(currentPrice / 100) * 100 + 26;
        }					
		
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PropTraderz.GoldenSetupV3_1[] cacheGoldenSetupV3_1;
		public PropTraderz.GoldenSetupV3_1 GoldenSetupV3_1(bool showNearestLevel, TextPosition nearestLevelPosition, bool showEntrySignal, int textSignalSize, int textSignalOffset, int textSignalOpacity, System.Windows.Media.Brush buySignalColor, System.Windows.Media.Brush sellSignalColor, System.Windows.Media.Brush targetSignalColor, bool showWatermark)
		{
			return GoldenSetupV3_1(Input, showNearestLevel, nearestLevelPosition, showEntrySignal, textSignalSize, textSignalOffset, textSignalOpacity, buySignalColor, sellSignalColor, targetSignalColor, showWatermark);
		}

		public PropTraderz.GoldenSetupV3_1 GoldenSetupV3_1(ISeries<double> input, bool showNearestLevel, TextPosition nearestLevelPosition, bool showEntrySignal, int textSignalSize, int textSignalOffset, int textSignalOpacity, System.Windows.Media.Brush buySignalColor, System.Windows.Media.Brush sellSignalColor, System.Windows.Media.Brush targetSignalColor, bool showWatermark)
		{
			if (cacheGoldenSetupV3_1 != null)
				for (int idx = 0; idx < cacheGoldenSetupV3_1.Length; idx++)
					if (cacheGoldenSetupV3_1[idx] != null && cacheGoldenSetupV3_1[idx].ShowNearestLevel == showNearestLevel && cacheGoldenSetupV3_1[idx].NearestLevelPosition == nearestLevelPosition && cacheGoldenSetupV3_1[idx].ShowEntrySignal == showEntrySignal && cacheGoldenSetupV3_1[idx].textSignalSize == textSignalSize && cacheGoldenSetupV3_1[idx].textSignalOffset == textSignalOffset && cacheGoldenSetupV3_1[idx].textSignalOpacity == textSignalOpacity && cacheGoldenSetupV3_1[idx].BuySignalColor == buySignalColor && cacheGoldenSetupV3_1[idx].SellSignalColor == sellSignalColor && cacheGoldenSetupV3_1[idx].TargetSignalColor == targetSignalColor && cacheGoldenSetupV3_1[idx].ShowWatermark == showWatermark && cacheGoldenSetupV3_1[idx].EqualsInput(input))
						return cacheGoldenSetupV3_1[idx];
			return CacheIndicator<PropTraderz.GoldenSetupV3_1>(new PropTraderz.GoldenSetupV3_1(){ ShowNearestLevel = showNearestLevel, NearestLevelPosition = nearestLevelPosition, ShowEntrySignal = showEntrySignal, textSignalSize = textSignalSize, textSignalOffset = textSignalOffset, textSignalOpacity = textSignalOpacity, BuySignalColor = buySignalColor, SellSignalColor = sellSignalColor, TargetSignalColor = targetSignalColor, ShowWatermark = showWatermark }, input, ref cacheGoldenSetupV3_1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PropTraderz.GoldenSetupV3_1 GoldenSetupV3_1(bool showNearestLevel, TextPosition nearestLevelPosition, bool showEntrySignal, int textSignalSize, int textSignalOffset, int textSignalOpacity, System.Windows.Media.Brush buySignalColor, System.Windows.Media.Brush sellSignalColor, System.Windows.Media.Brush targetSignalColor, bool showWatermark)
		{
			return indicator.GoldenSetupV3_1(Input, showNearestLevel, nearestLevelPosition, showEntrySignal, textSignalSize, textSignalOffset, textSignalOpacity, buySignalColor, sellSignalColor, targetSignalColor, showWatermark);
		}

		public Indicators.PropTraderz.GoldenSetupV3_1 GoldenSetupV3_1(ISeries<double> input , bool showNearestLevel, TextPosition nearestLevelPosition, bool showEntrySignal, int textSignalSize, int textSignalOffset, int textSignalOpacity, System.Windows.Media.Brush buySignalColor, System.Windows.Media.Brush sellSignalColor, System.Windows.Media.Brush targetSignalColor, bool showWatermark)
		{
			return indicator.GoldenSetupV3_1(input, showNearestLevel, nearestLevelPosition, showEntrySignal, textSignalSize, textSignalOffset, textSignalOpacity, buySignalColor, sellSignalColor, targetSignalColor, showWatermark);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PropTraderz.GoldenSetupV3_1 GoldenSetupV3_1(bool showNearestLevel, TextPosition nearestLevelPosition, bool showEntrySignal, int textSignalSize, int textSignalOffset, int textSignalOpacity, System.Windows.Media.Brush buySignalColor, System.Windows.Media.Brush sellSignalColor, System.Windows.Media.Brush targetSignalColor, bool showWatermark)
		{
			return indicator.GoldenSetupV3_1(Input, showNearestLevel, nearestLevelPosition, showEntrySignal, textSignalSize, textSignalOffset, textSignalOpacity, buySignalColor, sellSignalColor, targetSignalColor, showWatermark);
		}

		public Indicators.PropTraderz.GoldenSetupV3_1 GoldenSetupV3_1(ISeries<double> input , bool showNearestLevel, TextPosition nearestLevelPosition, bool showEntrySignal, int textSignalSize, int textSignalOffset, int textSignalOpacity, System.Windows.Media.Brush buySignalColor, System.Windows.Media.Brush sellSignalColor, System.Windows.Media.Brush targetSignalColor, bool showWatermark)
		{
			return indicator.GoldenSetupV3_1(input, showNearestLevel, nearestLevelPosition, showEntrySignal, textSignalSize, textSignalOffset, textSignalOpacity, buySignalColor, sellSignalColor, targetSignalColor, showWatermark);
		}
	}
}

#endregion
