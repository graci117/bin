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

	public class ArrowsDB2 : Indicator
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
		
		 private enum Mode { Buy, Sell, None }
        private Mode currentMode = Mode.None;
	
	   protected override void OnStateChange()
		{
		    if (State == State.SetDefaults)
		    {
		       Description = "Arrows DB2";
		        Name = "Arrows DB2";
		        Calculate = Calculate.OnBarClose;
		        IsOverlay = false;
		        IsSuspendedWhileInactive = true;
				
				BuySignalColor =  Brushes.Lime;
				SellSignalColor = Brushes.Red;
		        
				//DrawOnPricePanel = true;
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
				
				ColorBars									= false;		
				BarColorCrossAbove							= Brushes.Blue;
				BarColorCrossBelow							= Brushes.Orange;			
				
				
				PanelColorCrossAbove						= Brushes.Lime;
				PanelColorCrossBelow						= Brushes.Red;
				BackgroundOpacity							= 50;				
							
						
				
				
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
		    
		    // Calculate trend switch
		    if (Close[0] > trendDown[1])
		        trendSwitch[0] = 1;
		    else if (Close[0] < trendUp[1])
		        trendSwitch[0] = -1;
		    else
		        trendSwitch[0] = trendSwitch[1];
		
		    // Set trend direction and bullish/bearish state
		    trendDirection[0] = trendSwitch[0] == 1 ? trendUp[0] : trendDown[0];
		    bullishTrend[0] = trendDirection[0] == trendUp[0] ; // this bullishTrend is used for Background also
			
			bool strongBullishCandle = Close[0] > Open[0] && Open[0] == Low[0] && Close[0] > High[1] && Close[0] > profitWaveEmaFast[0];
		    bool buySignal  = trendDirection[0] == trendUp[0] ;
			bool buy_con = buySignal && strongBullishCandle && mfiBuy && canBuy;
		
		    bool strongBearishCandle = Close[0] < Open[0] && Open[0] == High[0] && Close[0] < Low[1] && Close[0] < profitWaveEmaSlow[0];
		    bool sellSignal = trendDirection[0] == trendDown[0] ;
			bool sell_con = sellSignal && strongBearishCandle && mfiSell && canSell;
			
			isBullish[0] = Close[0] > profitWaveEmaSlow[0];
			
			 // Calculate buy/sell conditions
		    
		
		    if (EnableChopFilter)
		    {
		        double diPlus = DM(14).DiPlus[0];
		        double diMinus = DM(14).DiMinus[0];
		        canBuy = Math.Floor(diPlus) > Math.Floor(diMinus) && Math.Floor(diPlus) >= 45;
		        canSell = Math.Floor(diMinus) > Math.Floor(diPlus) && Math.Floor(diMinus) >= 45;
		    }
		    
			
		    if (double.IsNaN(mfi)) return;
		    
			
			
			Brush mfiColor = mfi > 50 ? MfiBullishColor : MfiBearishColor;
		    
		    // Plot MFI line
		    Value[0] = mfi;
		    PlotBrushes[0][0] = mfiColor;
			
		
		    // Use bullishTrend for dot coloring
		    Values[3][0] = 50;  // Middle line value
		    PlotBrushes[3][0] = bullishTrend[0] ? MfiBullishColor : MfiBearishColor;	
						
			
			if (buy_con) //bigger dots
			{
				Draw.Dot(this, "signalDot" + CurrentBar, true, 0, 50, Brushes.Yellow, false);
			}
			if (sell_con)
			{
				
				Draw.Dot(this, "signalDot" + CurrentBar, true, 0, 50, Brushes.White, false);
			}
			
		
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
			
			//this.DrawOnPricePanel = false;
			//BackBrush = bullishTrend[0] ? PanelColorCrossAbove :  PanelColorCrossBelow;	
			//this.DrawOnPricePanel = true;
			
			
			
		}
	
	    #region Properties
	    
	
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
	    #endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ArrowsDB2[] cacheArrowsDB2;
		public ArrowsDB2 ArrowsDB2(bool enableChopFilter)
		{
			return ArrowsDB2(Input, enableChopFilter);
		}

		public ArrowsDB2 ArrowsDB2(ISeries<double> input, bool enableChopFilter)
		{
			if (cacheArrowsDB2 != null)
				for (int idx = 0; idx < cacheArrowsDB2.Length; idx++)
					if (cacheArrowsDB2[idx] != null && cacheArrowsDB2[idx].EnableChopFilter == enableChopFilter && cacheArrowsDB2[idx].EqualsInput(input))
						return cacheArrowsDB2[idx];
			return CacheIndicator<ArrowsDB2>(new ArrowsDB2(){ EnableChopFilter = enableChopFilter }, input, ref cacheArrowsDB2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ArrowsDB2 ArrowsDB2(bool enableChopFilter)
		{
			return indicator.ArrowsDB2(Input, enableChopFilter);
		}

		public Indicators.ArrowsDB2 ArrowsDB2(ISeries<double> input , bool enableChopFilter)
		{
			return indicator.ArrowsDB2(input, enableChopFilter);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ArrowsDB2 ArrowsDB2(bool enableChopFilter)
		{
			return indicator.ArrowsDB2(Input, enableChopFilter);
		}

		public Indicators.ArrowsDB2 ArrowsDB2(ISeries<double> input , bool enableChopFilter)
		{
			return indicator.ArrowsDB2(input, enableChopFilter);
		}
	}
}

#endregion
