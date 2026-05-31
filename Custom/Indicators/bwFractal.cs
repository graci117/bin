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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	// 09-13-2021 Added Update() into plot outputs
	
	public class bwFractal : Indicator
	{
		private int lineWidth		= 1;
		private int barcounter 		= 0;
		private int markersize 		= 10;
		private int offset 			= 2;
		private int lastlowbar 		= 0;
		private int lasthighbar 	= 0;
		private int highcount 		= 1;
		private int lowcount 		= 1;
		private int highdrawbar 	= 0;
		private int lowdrawbar 		= 0;
		private int history 		= 3;
		private bool showText		= false;
		private bool showRays		= true;
		private double lastlow 		= 0.0;
		private double lasthigh 	= 0.0;		
		private Brush upcolor		= Brushes.Green;
		private Brush downcolor 	= Brushes.Red;			
		private	Gui.Tools.SimpleFont	textFont;
	
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Fractal Technical Indicator it is a series of at least five successive bars, with the highest HIGH 
														in the middle, and two lower HIGHs on both sides. The reversing set is a series of at least five 
														successive bars, with the lowest LOW in the middle, and two higher LOWs on both sides, which correlates 
														to the sell fractal. The fractals are have High and Low values and are indicated with the up and down arrows.";
				Name								= "bwFractal";
				Calculate							= Calculate.OnPriceChange;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Block, "Upper");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Block, "Lower");
				textFont	= new Gui.Tools.SimpleFont("Arial", 12);
			}

		}

		protected override void OnBarUpdate()
		{

			if (CurrentBar < 10)
				return;
			
			if (IsFirstTickOfBar)
				barcounter++;
			
			isHighPivot(2,upcolor);
			
			isLowPivot(2,downcolor);
		}

		private void isHighPivot(int period, Brush color)
		{
			#region HighPivot
			int y = 0;
			int Lvls = 0;
	
			//Four Matching Highs
			if(High[period]==High[period+1] && High[period]==High[period+2] && High[period]==High[period+3])
			{
				y = 1;
				while (y<=period)
				{
					if (y!=period ? High[period+3]>High[period+3+y] : High[period+3]>High[period+3+y])
						Lvls++;
					if (y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Highs
			else if (High[period]==High[period+1] && High[period]==High[period+2])
			{
				y = 1;
				while (y<=period)
				{
					if (y!=period ? High[period+2]>High[period+2+y] : High[period+2]>High[period+2+y])
						Lvls++;
					if (y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Highs
			else if (High[period]==High[period+1])
			{
				y = 1;
				while (y<=period)
				{
					if (y!=period ? High[period+1]>High[period+1+y] : High[period+1]>High[period+1+y])
						Lvls++;
					if (y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			//Regular Pivot
			else
			{
				y = 1;
				while (y<=period)
				{
					if (y!=period ? High[period]>High[period+y] : High[period]>High[period+y])
						Lvls++;
					if (y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if (Lvls<period*2)
			{
				Lvls=0;
				//Four Highs - First and Last Matching - Middle 2 are lower
				if (High[period]>=High[period+1] && High[period]>=High[period+2] && High[period]==High[period+3])
				{
					y=1;
					while (y<=period)
					{
						if (y!=period ? High[period+3]>High[period+3+y] : High[period+3]>High[period+3+y])
							Lvls++;
						if (y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
							Lvls++;
						y++;
					}
				}
			}
			if (Lvls<period*2)
			{
				Lvls=0;
				//Three Highs - Middle is lower than two outside
				if(High[period]>=High[period+1] && High[period]==High[period+2])
				{
					y=1;
					while (y<=period)
					{
						if (y!=period ? High[period+2]>High[period+2+y] : High[period+2]>High[period+2+y])
						Lvls++;
					if (y!=period ? High[period]>High[period-y] : High[period]>High[period-y])
						Lvls++;
					y++;
					}
				}
			}
			if (Lvls>=period*2)
			{ 
				Upper[period] = High[period];		// Draw the Block
			
				if (highdrawbar != CurrentBar)
				{
					if (showText)
					{
						Draw.Text(this, "High"+highcount, true, High[period].ToString(), period, High[period], 20, color, textFont, 
							TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}
					if (showRays)
					{
						Draw.Ray (this, "Highray"+highcount, false,  CurrentBar - lasthighbar, lasthigh, period, High[period], 
							color, DashStyleHelper.Dash, lineWidth);
					}
					highcount++;
					
					if (highcount == history + 1)
					{
						highcount = 1;
					}
					highdrawbar = CurrentBar;
				}
				lasthigh = High[period];				// Save the last High value for next ray drawn
				lasthighbar = CurrentBar - period;		// Save the last High location for next ray drawn
			}
			#endregion
		}

		private void isLowPivot(int period, Brush color)
		{
			#region LowPivot
			int y = 0;
			int Lvls = 0;
			
			//Four Matching Lows
			if(Low[period]==Low[period+1] && Low[period]==Low[period+2] && Low[period]==Low[period+3])
			{
				y = 1;
				while(y<=period)
				{
					if(y!=period ? Low[period+3]<Low[period+3+y] : Low[period+3]<Low[period+3+y])
						Lvls++;
					if(y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			//Three Matching Lows
			else if(Low[period]==Low[period+1] && Low[period]==Low[period+2])
			{
				y=1;
				while (y<=period)
				{
					if (y!=period ? Low[period+2]<Low[period+2+y] : Low[period+2]<Low[period+2+y])
						Lvls++;
					if (y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			//Two Matching Lows
			else if (Low[period]==Low[period+1])
			{
				y=1;
				while(y<=period)
				{
					if (y!=period ? Low[period+1]<Low[period+1+y] : Low[period+1]<Low[period+1+y])
						Lvls++;
					if (y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			//Regular Pivot
			else
			{
				y=1;
				while (y<=period)
				{
					if (y!=period ? Low[period]<Low[period+y] : Low[period]<Low[period+y])
						Lvls++;
					if (y!=period ? Low[period]<Low[period-y] : Low[period]<Low[period-y])
						Lvls++;
					y++;
				}
			}
			
			//Auxiliary Checks
			if (Lvls<period*2)
			{
				Lvls = 0;
				//Four Lows - First and Last Matching - Middle 2 are lower
				if (Low[period] <= Low[period+1] && Low[period]<=Low[period+2] && Low[period]==Low[period+3])
				{
					y = 1;
					while (y <= period)
					{
						if (y != period ? Low[period + 3] < Low[period + 3 + y] : Low[period + 3] < Low[period + 3 + y])
							Lvls++;
						if (y != period ? Low[period] < Low[period - y] : Low[period] < Low[period - y])
							Lvls++;
						y++;
					}
				}
			}
			if(Lvls < period * 2)
			{
				Lvls = 0;
				//Three Lows - Middle is lower than two outside
				if (Low[period] <= Low[period + 1] && Low[period]==Low[period + 2])
				{
					y = 1;
					while (y <= period)
					{
						if (y != period ? Low[period + 2]<Low[period + 2 + y] : Low[period + 2] < Low[period + 2 + y])
						Lvls++;
					if (y != period ? Low[period] < Low[period-y] : Low[period]  <Low[period - y])
						Lvls++;
					y++;
					}
				}
			}
			if (Lvls >= period * 2)
			{
				Lower[period] = Low[period];		// draw the block				
				
				if (lowdrawbar != CurrentBar)
				{
					if (showText)
					{
						Draw.Text(this, "Low"+lowcount, true, Low[period].ToString(), period, Low[period], -20, color,
							textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}					
					if (showRays)
					{
						Draw.Ray (this, "lowray"+lowcount, false,  CurrentBar - lastlowbar, lastlow, period, Low[period],
							color, DashStyleHelper.Dash, lineWidth);
					}
					lowcount++;
					if (lowcount == history + 1) 
					{
						lowcount = 1;
					}
					lowdrawbar = CurrentBar;	
				}
				lastlow = Low[period];					// Save the low value for next ray drawn
				lastlowbar = CurrentBar - period;		// Save the low value location for next ray drawn
			}
			#endregion
		}		

		#region Properties

		[NinjaScriptProperty]
		[Display(Name="Display price", Description="Displays price level of Fractal", Order=1, GroupName="Parameters")]
		public bool ShowText
		{ 
			get {return showText;}
			set {showText = value;}
		}
		[NinjaScriptProperty]
		[Display(Name="Display Rays", Description="Draw Fractal rays", Order=2, GroupName="Parameters")]
		public bool ShowRays
		{ 
			get {return showRays;}
			set {showRays = value;}
		}
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Historical Text/Ray", Description="Number of rays/Text to show", Order=3, GroupName="Parameters")]
		public int History
		{ 
			get {return history;}
			set {history = value;}
		}
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Ray line width", Description="Line thickness ", Order=4, GroupName="Parameters")]
		public int LineWidth
		{ 
			get {return lineWidth;}
			set {lineWidth = value;}
		}		
	
		[XmlIgnore]
		[Display(Name="Top color", Description="Ray/text up color", Order=5, GroupName="Parameters")]
		public Brush Upcolor
		{ 
			get {return upcolor;} 
			set {upcolor = value;}
		}

		[Browsable(false)]
		public string UpcolorSerializable
		{
			get { return Serialize.BrushToString(upcolor); }
			set { upcolor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Bottom color", Description="Ray/Text down color", Order=6, GroupName="Parameters")]
		public Brush Downcolor
		{ 
			get {return downcolor;} 
			set {downcolor = value;}
		}

		[Browsable(false)]
		public string DowncolorSerializable
		{
			get { return Serialize.BrushToString(downcolor); }
			set { downcolor = Serialize.StringToBrush(value); }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { 
				Update();
				return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { 
				Update();
				return Values[1]; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private bwFractal[] cachebwFractal;
		public bwFractal bwFractal(bool showText, bool showRays, int history, int lineWidth)
		{
			return bwFractal(Input, showText, showRays, history, lineWidth);
		}

		public bwFractal bwFractal(ISeries<double> input, bool showText, bool showRays, int history, int lineWidth)
		{
			if (cachebwFractal != null)
				for (int idx = 0; idx < cachebwFractal.Length; idx++)
					if (cachebwFractal[idx] != null && cachebwFractal[idx].ShowText == showText && cachebwFractal[idx].ShowRays == showRays && cachebwFractal[idx].History == history && cachebwFractal[idx].LineWidth == lineWidth && cachebwFractal[idx].EqualsInput(input))
						return cachebwFractal[idx];
			return CacheIndicator<bwFractal>(new bwFractal(){ ShowText = showText, ShowRays = showRays, History = history, LineWidth = lineWidth }, input, ref cachebwFractal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.bwFractal bwFractal(bool showText, bool showRays, int history, int lineWidth)
		{
			return indicator.bwFractal(Input, showText, showRays, history, lineWidth);
		}

		public Indicators.bwFractal bwFractal(ISeries<double> input , bool showText, bool showRays, int history, int lineWidth)
		{
			return indicator.bwFractal(input, showText, showRays, history, lineWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.bwFractal bwFractal(bool showText, bool showRays, int history, int lineWidth)
		{
			return indicator.bwFractal(Input, showText, showRays, history, lineWidth);
		}

		public Indicators.bwFractal bwFractal(ISeries<double> input , bool showText, bool showRays, int history, int lineWidth)
		{
			return indicator.bwFractal(input, showText, showRays, history, lineWidth);
		}
	}
}

#endregion
