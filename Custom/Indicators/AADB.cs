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



namespace NinjaTrader.NinjaScript.Indicators
{
	

	public class AADB : Indicator
	{
	    private Series<double> mfiSeries;
		private Series<double> profitWaveEmaFast;
		private Series<double> profitWaveEmaMedium;
		private Series<double> profitWaveEmaSlow;
		private Series<bool> isBullish;
		private Series<double> trendUp;
		private Series<double> trendDown;
		private Series<int> trendSwitch;
		private Series<double> trendDirection;
		private Series<bool> bullishTrend;
		private Series<double> obline, osline, upperline, lowerline, obline2, osline2;
		
		
		 private enum Mode { Buy, Sell, None }
        private Mode currentMode = Mode.None;
		 
		 
	
	   protected override void OnStateChange()
		{
		    if (State == State.SetDefaults)
		    {
		       Description = "aaDashboard Oscillator Arrows";
		        Name = "aaDashboard Oscillator Arrows";
		        Calculate = Calculate.OnBarClose;
		        IsOverlay = false;
		        IsSuspendedWhileInactive = true;
				
				BuySignalColor =  Brushes.Lime;
				SellSignalColor = Brushes.Red;
		        
				DrawOnPricePanel = false;
		        MfiBullishColor = Brushes.Lime;
		        MfiBearishColor = Brushes.Red;
				EnableChopFilter = false;
				ColorBackground							= true;
		        // Add plots
		        AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "MFI");
		        AddPlot(new Stroke(Brushes.Lime, 3), PlotStyle.Dot, "Buy Signal");
		        AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Dot, "Sell Signal");
		
		        // Add middle line with dots
		        AddPlot(new Stroke(Brushes.White,1), PlotStyle.Dot, "Trend Dots");
				AddPlot(new Stroke(Brushes.White, 7), PlotStyle.TriangleUp, "Signal Dots"); 
				
				AddPlot(Brushes.Red, "OS1");
				AddPlot(Brushes.Green, "OB1");
				
				ColorBars									= true;		
				BarColorCrossAbove							= Brushes.Blue;
				BarColorCrossBelow							= Brushes.Orange;			
				
				
				PanelColorCrossAbove						= Brushes.Lime;
				PanelColorCrossBelow						= Brushes.Red;
				BackgroundOpacity							= 50;				
							
						
				EnableTrendCandleColoring = true;
			    CandleColoringType = ColoringType.ProfitWave;
			    BullishCandleColor = Brushes.Green;
			    BearishCandleColor = Brushes.Red;
				
				AddLine(new Stroke(Brushes.Red), 100, "Line100");
		        AddLine(new Stroke(Brushes.Red), 90, "Line90");
		        AddLine(new Stroke(Brushes.Red), 80, "Line80");
		        AddLine(new Stroke(Brushes.Green), 20, "Line20");
		        AddLine(new Stroke(Brushes.Green), 10, "Line10");
		        AddLine(new Stroke(Brushes.Green), 0, "Line0");
				
				
		
		       
		    }
		    else if (State == State.Configure)
		    {
		        mfiSeries = MFI(10).Value;
				isBullish = new Series<bool>(this);
				
				  	
				
				if (ColorBackground )
				{
					Brush temp = PanelColorCrossAbove.Clone();
					temp.Opacity = BackgroundOpacity / 100.0;
					temp.Freeze();
					PanelColorCrossAbove = temp;
					
					Brush temp1 = PanelColorCrossBelow.Clone();
					temp1.Opacity = BackgroundOpacity / 100.0;
					temp1.Freeze();
					PanelColorCrossBelow = temp1;	
				}		
				
				
		    }
			 else if (State == State.DataLoaded)
		    {
		        profitWaveEmaFast = EMA(Close,8).Value;
		        profitWaveEmaMedium = EMA(Close,14).Value;
		        profitWaveEmaSlow = EMA(Close,21).Value;
				 trendUp = new Series<double>(this);
		        trendDown = new Series<double>(this);
		        trendSwitch = new Series<int>(this);
		        trendDirection = new Series<double>(this);
		        bullishTrend = new Series<bool>(this);
				obline = new Series<double>(this, MaximumBarsLookBack.Infinite);
				osline = new Series<double>(this, MaximumBarsLookBack.Infinite);
				upperline = new Series<double>(this, MaximumBarsLookBack.Infinite);
				lowerline = new Series<double>(this, MaximumBarsLookBack.Infinite);
				obline2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				osline2 = new Series<double>(this, MaximumBarsLookBack.Infinite);
		    }
		    
		}

		protected override void OnBarUpdate()
		{
		    if (CurrentBar < 20) return;
			//this.DrawOnPricePanel = false;
		
		    // Calculate MFI value
		    double mfi = mfiSeries[0];
			
			bool mfiBuy = mfi > 52;
		    bool mfiSell = mfi < 48;
		    bool canBuy = true;
		    bool canSell = true;
			
			double up = ((High[0] + Low[0]) / 2) - (1.3 * ATR(8)[0]);
		    double dn = ((High[0] + Low[0]) / 2) + (1.3 * ATR(8)[0]);
		
		    // Calculate trend values
		    trendUp[0] = Close[1] > trendUp[1] ? Math.Max(up, trendUp[1]) : up;
		    trendDown[0] = Close[1] < trendDown[1] ? Math.Min(dn, trendDown[1]) : dn;
			
			 OS1[0] = 20.0;     //OS.HideBubble(); OS.HideTitle();
            OB1[0] = 80.0;     //OB.HideBubble(); OB.HideTitle();
			
 			//Draw.Region(this, "OS", CurrentBar, 0, OS1, 0, Brushes.Red, 50);
            //Draw.Region(this, "OB", CurrentBar, 0, OB1, 100, Brushes.Green, 50);
			
			//Draw.RegionHighlightY(this,"OS1",false, 80,100,Brushes.Red,Brushes.Red,50);
		    
		    // Calculate trend switch
		    if (Close[0] > trendDown[1])
		        trendSwitch[0] = 1;
		    else if (Close[0] < trendUp[1])
		        trendSwitch[0] = -1;
		    else
		        trendSwitch[0] = trendSwitch[1];
			
			if (EnableChopFilter)
		    {
		        double diPlus = DM(14).DiPlus[0];
		        double diMinus = DM(14).DiMinus[0];
		        canBuy = Math.Floor(diPlus) > Math.Floor(diMinus) && Math.Floor(diPlus) >= 45;
		        canSell = Math.Floor(diMinus) > Math.Floor(diPlus) && Math.Floor(diMinus) >= 45;
		    }
		    
		
		    // Set trend direction and bullish/bearish state
		    trendDirection[0] = trendSwitch[0] == 1 ? trendUp[0] : trendDown[0];
		    bullishTrend[0] = trendDirection[0] == trendUp[0] ; // this bullishTrend is used for Background also
			
			 if (EnableTrendCandleColoring)
			    {
			        Brush candleColor = Brushes.Gray;
			
			        if (CandleColoringType == ColoringType.Trend)
			        {
			            candleColor = bullishTrend[0] ? BullishCandleColor : BearishCandleColor;
			        }
			        else if (CandleColoringType == ColoringType.ProfitWave)
			        {
			            candleColor = Close[0] > profitWaveEmaSlow[0] ? BullishCandleColor : BearishCandleColor;
			        }
			
			        BarBrush = candleColor;
			        CandleOutlineBrush = candleColor;
			    }
			 if (Close[0] > Open[0])
                    {
                        byte g = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).G;
                        byte r = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).R;
                        byte b = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).B;

                        BarBrushes[0] = new SolidColorBrush(Color.FromArgb((byte)0, r, g, b));
                    }
				
			if ( bullishTrend[0] != bullishTrend[1])
				currentMode = Mode.None;
			
			bool buySignal = bullishTrend[0];
            bool sellSignal = !bullishTrend[0];
			
			bool strongBullishCandle = Close[0] > Open[0] && Open[0] == Low[0] && Close[0] > High[1] && Close[0] > profitWaveEmaFast[0];
			bool strongBearishCandle = Close[0] < Open[0] && Open[0] == High[0] && Close[0] < Low[1] && Close[0] < profitWaveEmaSlow[0];
		    //bool buySignal  = trendDirection[0] == trendUp[0] ;
			bool buy_con = buySignal && strongBullishCandle && mfiBuy && canBuy  && currentMode != Mode.Buy;
			bool sell_con = sellSignal && strongBearishCandle && mfiSell && canSell && currentMode != Mode.Sell;
			
			 // Plot arrows on price panel
		    if (buy_con)
		    {
				currentMode = Mode.Buy;
		        Draw.ArrowUp(this, "Buy" + CurrentBar, true, 0, Low[0] - TickSize * 2, BuySignalColor);
		    }
		
		    if (sell_con)
		    {
				currentMode = Mode.Sell;
		        Draw.ArrowDown(this, "Sell" + CurrentBar, true, 0, High[0] + TickSize * 2, SellSignalColor);
		    }
			
			if (buy_con) //bigger dots
			{
				Draw.Dot(this, "signalDot" + CurrentBar, true, 0, 50, Brushes.Yellow, false);
			}
			if (sell_con)
			{
				
				Draw.Dot(this, "signalDot" + CurrentBar, true, 0, 50, Brushes.White, false);
			}
			
			
			isBullish[0] = Close[0] > profitWaveEmaSlow[0];			
		    if (double.IsNaN(mfi)) return;
			Brush mfiColor = mfi > 50 ? MfiBullishColor : MfiBearishColor;
		    
		    // Plot MFI line
		    Value[0] = mfi;
		    PlotBrushes[0][0] = mfiColor;
			
		
		    // Use bullishTrend for dot coloring
		    Values[3][0] = 50;  // Middle line value
		    PlotBrushes[3][0] = bullishTrend[0] ? MfiBullishColor : MfiBearishColor;	
			
			if (currentMode == Mode.Buy && Close[0] < profitWaveEmaSlow[0] )
			    currentMode = Mode.None;
			
			if (currentMode == Mode.Sell && Close[0] > profitWaveEmaSlow[0])
			    currentMode = Mode.None;
			
			upperline[0] = 100;
			lowerline[0] = 0;
			obline[0] = 90;
			osline[0] = 10;
			obline2[0] = 80;
			osline2[0] = 20;
			
//			Draw.Region(this, "obToUpper" + CurrentBar, CurrentBar, 0, upperline, obline, Brushes.LightGreen, 10);
//			Draw.Region(this, "osToLower" + CurrentBar, CurrentBar, 0, lowerline, osline, Brushes.IndianRed, 10);
			
			
			//for this to work DrawOnPricePanel Should be false. But if I do that then arrows don't appear and rendering is very slow
//			Draw.Region(this, "tag1" + CurrentBar , CurrentBar, 0, upperline, obline, null, Brushes.DarkRed, 60);
//			Draw.Region(this, "tag2" + CurrentBar , CurrentBar, 0, obline, obline2, null, Brushes.Red, 30);
//			Draw.Region(this, "tag3" + CurrentBar , CurrentBar, 0, lowerline, osline, null, Brushes.Lime, 60);
//			Draw.Region(this, "tag4" + CurrentBar , CurrentBar, 0, osline, osline2, null, Brushes.Green, 30);
			
			//Draw.RegionHighlightY(this, "tag1", 60, 0, Brushes.Blue);
			
			//Draw.RegionHighlightY(this, "tag1", true, 100, 90, Brushes.Blue, Brushes.Green, 20);
						
		}
	
	    #region Properties
	    
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OS1
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OB1
		{
			get { return Values[6]; }
		}
		
	    [XmlIgnore]
	    [Display(Name = "MFI Bullish Color", Description = "Color for bullish MFI", Order = 2, GroupName = "Colors")]
	    public Brush MfiBullishColor { get; set; }
	
	    [XmlIgnore]
	    [Display(Name = "MFI Bearish Color", Description = "Color for bearish MFI", Order = 3, GroupName = "Colors")]
	    public Brush MfiBearishColor { get; set; }
	
	    [Browsable(false)]
	    public string MfiBullishColorSerializable
	    {
	        get { return Serialize.BrushToString(MfiBullishColor); }
	        set { MfiBullishColor = Serialize.StringToBrush(value); }
	    }
	
	    [Browsable(false)]
	    public string MfiBearishColorSerializable
	    {
	        get { return Serialize.BrushToString(MfiBearishColor); }
	        set { MfiBearishColor = Serialize.StringToBrush(value); }
	    }
		
		[XmlIgnore]
		[Display(Name = "Buy Signal Color", Order = 1, GroupName = "Colors")]
		public Brush BuySignalColor { get; set; }
		
		[XmlIgnore]
		[Display(Name = "Sell Signal Color", Order = 2, GroupName = "Colors")]
		public Brush SellSignalColor { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Chop Filter", Order = 1, GroupName = "Parameters")]
		public bool EnableChopFilter { get; set; }
		
		[Display(Name="Color Price Panel Background", Description="Color Price panel background when Crossabove/below", Order=30, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBackground
		{ get; set; }
		
		[Range(1, 99)]
		[Display(Name=" % Opacity of background", Description="Sets the amount of opacity of background colors ", Order=31, GroupName="Cross Detection Actions")]
		public int BackgroundOpacity
		{ get; set; }			

		[XmlIgnore]
		[Display(Name="Panel Color for Cross above", Description="Panel background color when crossing above", Order=32, GroupName="Cross Detection Actions")]
		public Brush PanelColorCrossAbove
		{ get; set; }

		[Browsable(false)]
		public string PanelColorCrossAboveSerializable
		{
			get { return Serialize.BrushToString(PanelColorCrossAbove); }
			set { PanelColorCrossAbove = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Panel Color for Cross below", Description="Panel background coloe when croissing below", Order=33, GroupName="Cross Detection Actions")]
		public Brush PanelColorCrossBelow
		{ get; set; }

		[Browsable(false)]
		public string PanelColorCrossBelowSerializable
		{
			get { return Serialize.BrushToString(PanelColorCrossBelow); }
			set { PanelColorCrossBelow = Serialize.StringToBrush(value); }
		}			

		
		
		[Display(Name="Color Price Bar on cross", Description="Color the bar where cross occured", Order=40, GroupName="Cross Detection Actions")]
		[RefreshProperties(RefreshProperties.All)]
		public bool ColorBars
		{ get; set; }			
		
		[XmlIgnore]
		[Display(Name="Bar Color for Cross above", Description="Price bar color when crossing above", Order=41, GroupName="Cross Detection Actions")]
		public Brush BarColorCrossAbove
		{ get; set; }

		[Browsable(false)]
		public string BarColorCrossAboveSerializable
		{
			get { return Serialize.BrushToString(BarColorCrossAbove); }
			set { BarColorCrossAbove = Serialize.StringToBrush(value); }
		}	
		
		[XmlIgnore]
		[Display(Name="Bar Color for Cross below", Description="Price bar color when crossing below", Order=42, GroupName="Cross Detection Actions")]
		public Brush BarColorCrossBelow
		{ get; set; }

		[Browsable(false)]
		public string BarColorCrossBelowSerializable
		{
			get { return Serialize.BrushToString(BarColorCrossBelow); }
			set { BarColorCrossBelow = Serialize.StringToBrush(value); }
		}	
		
		public bool IsTrendBullish
		{
			get
			{
				return bullishTrend[0];
			}
		}
		
		[NinjaScriptProperty]
		[Display(Name="Enable Candle Color Matching", Description="Enable coloring of candles based on trend or profit wave", Order=1, GroupName="Candle Settings")]
		public bool EnableTrendCandleColoring { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Candle Coloring Type", Description="Choose the method for coloring candles", Order=2, GroupName="Candle Settings")]
		public ColoringType CandleColoringType { get; set; }
		
		[XmlIgnore]
		[Display(Name="Bullish Candle Color", Description="Color for bullish candles", Order=3, GroupName="Candle Settings")]
		public Brush BullishCandleColor { get; set; }
		
		[XmlIgnore]
		[Display(Name="Bearish Candle Color", Description="Color for bearish candles", Order=4, GroupName="Candle Settings")]
		public Brush BearishCandleColor { get; set; }


	    #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AADB[] cacheAADB;
		public AADB AADB(bool enableChopFilter, bool enableTrendCandleColoring, ColoringType candleColoringType)
		{
			return AADB(Input, enableChopFilter, enableTrendCandleColoring, candleColoringType);
		}

		public AADB AADB(ISeries<double> input, bool enableChopFilter, bool enableTrendCandleColoring, ColoringType candleColoringType)
		{
			if (cacheAADB != null)
				for (int idx = 0; idx < cacheAADB.Length; idx++)
					if (cacheAADB[idx] != null && cacheAADB[idx].EnableChopFilter == enableChopFilter && cacheAADB[idx].EnableTrendCandleColoring == enableTrendCandleColoring && cacheAADB[idx].CandleColoringType == candleColoringType && cacheAADB[idx].EqualsInput(input))
						return cacheAADB[idx];
			return CacheIndicator<AADB>(new AADB(){ EnableChopFilter = enableChopFilter, EnableTrendCandleColoring = enableTrendCandleColoring, CandleColoringType = candleColoringType }, input, ref cacheAADB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AADB AADB(bool enableChopFilter, bool enableTrendCandleColoring, ColoringType candleColoringType)
		{
			return indicator.AADB(Input, enableChopFilter, enableTrendCandleColoring, candleColoringType);
		}

		public Indicators.AADB AADB(ISeries<double> input , bool enableChopFilter, bool enableTrendCandleColoring, ColoringType candleColoringType)
		{
			return indicator.AADB(input, enableChopFilter, enableTrendCandleColoring, candleColoringType);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AADB AADB(bool enableChopFilter, bool enableTrendCandleColoring, ColoringType candleColoringType)
		{
			return indicator.AADB(Input, enableChopFilter, enableTrendCandleColoring, candleColoringType);
		}

		public Indicators.AADB AADB(ISeries<double> input , bool enableChopFilter, bool enableTrendCandleColoring, ColoringType candleColoringType)
		{
			return indicator.AADB(input, enableChopFilter, enableTrendCandleColoring, candleColoringType);
		}
	}
}

#endregion
