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
	// Coverted to Nt8b5  10/17/15
	// Improved/reduced calls to draw region
	
	public class BollingerTripleState_V4NT8 : Indicator
	{
		
		private	double		numStdDev				= 2;
		private int			period					= 14;
		private int			opacity					= 15;
		private int			wide_Tick_Threashold 	= 8;
		private int			narrow_Tick_Threashold 	= 4;
		
		private int			sb1 					= 0;		// Holds start bar for region 1
		private int			sb2						= 0;		// Holds start ber for region 2
		private int			sb3						= 0;		// Holds start bar for region 3
		
		private bool		s1						= false; 	// flag to capture start bar of region 1
		private bool		s2						= false; 	// flag to capture start bar of region 2
		private bool		s3						= false; 	// flag to capture start bar of region 3
		
		private bool 		myLines 				= true;
		private bool		shading					= true;
		private bool		displayData 			= false;			

		private Brush		wideColor 				= Brushes.Red;
		private Brush		narrowColor 			= Brushes.CornflowerBlue;
		private Brush 		neutralColor 			= Brushes.Gold;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Bollinger_TripleState_v4NT8 Bands are plotted at standard deviation levels above and below a moving average. Since standard deviation is a measure of volatility, the bands are self-adjusting: widening during volatile markets and contracting during calmer periods.";
				Name								= "BollingerTripleState_V4NT8";
				Calculate							= Calculate.OnBarClose;
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
				AddPlot(new Stroke (Brushes.Red, 2), PlotStyle.Line, "Lower");
				AddPlot(new Stroke (Brushes.Red, 2), PlotStyle.Line, "Upper");
				AddPlot(new Stroke (Brushes.Orange, 2), PlotStyle.Line, "Middle");

			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < Period) return;
			
			double line_upper = SMA(Period)[0] + NumStdDev * StdDev(Period)[0];		//get upper line value
			double line_lower = SMA(Period)[0] - NumStdDev * StdDev(Period)[0];		//get lower line value
			
			Upper[0] 	= line_upper;		//set the ds values to lines above
			Lower[0] 	= line_lower;
			Middle[0] 	= SMA(Period)[0];
			
			if (!myLines)
				PlotBrushes[2][0] = Brushes.Transparent;		// Hide the SMA line
		
			double diff 	= 	Upper[0] - Lower[0];	//calc diff btw line	
			double wtt 		= 	wide_Tick_Threashold * TickSize;
			double ntt 		= 	narrow_Tick_Threashold * TickSize;
			
			if( diff >= wtt)
			{
				if (!s1)
				{
					sb1	=	CurrentBar; 	// save the start bar for the region
					s1	= 	true;
					s2	=	false;
					s3	= 	false;
				}
					
				if (myLines)
				{
					PlotBrushes[0][0] = WideColor;
					PlotBrushes[1][0] = WideColor;
				}
				else
				{
					PlotBrushes[0][0] = Brushes.Transparent;	// Hide the upper/lower line
					PlotBrushes[1][0] = Brushes.Transparent;
				}
				
				if(shading)
				{
					Draw.Region(this, "reg1" + sb1, CurrentBar - sb1 + 1, 0, Upper, Lower, Brushes.Transparent, wideColor, opacity, Displacement);
				}
			}
			else if(diff < wtt && diff > ntt)
			{
				
				if (!s2)
				{
					sb2	=	CurrentBar; 	// save the start bar for the region
					s1	= 	false;
					s2	=	true;
					s3	= 	false;
				}
				
				if (myLines)
				{
					PlotBrushes[0][0] = NeutralColor;
					PlotBrushes[1][0] = NeutralColor;	
				}
				else
				{
					PlotBrushes[0][0] = Brushes.Transparent;
					PlotBrushes[1][0] = Brushes.Transparent;					
				}
				
				if(shading)
				{	
					Draw.Region(this, "reg2" + sb2, CurrentBar - sb2 + 1, 0,  Upper , Lower, Brushes.Transparent, neutralColor, opacity, Displacement);
				}
			}
			else if(diff <= ntt)
			{
				if (!s3)
				{
					sb3	=	CurrentBar; 	// save the start bar for the region
					s1	= 	false;
					s2	=	false;
					s3	= 	true;
				}				
				if (myLines)
				{
					PlotBrushes[0][0] = NarrowColor;
					PlotBrushes[1][0] = NarrowColor;
				}
				else
				{
					PlotBrushes[0][0] = Brushes.Transparent;
					PlotBrushes[1][0] = Brushes.Transparent;					
				}
							
				if(shading)
				{	
					Draw.Region(this, "reg3" + sb3,  CurrentBar - sb3 + 1, 0, Upper , Lower, Brushes.Transparent, narrowColor, opacity, Displacement);
				}
			}

			if (displayData)
			{
				Draw.TextFixed(this, "Threashold Determiner", "Actual Upper/Lower Band Spread: " + Math.Round(diff,4) 
					+ "\nUser Defined Threasholds:  Narrow: " + ntt + "  Wide: " + wtt , TextPosition.TopLeft);
			}
		
		}

		#region Properties
		[Range(0, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BB Standard deviations", Description="Number of Standard Deviations to spread the bands", Order=1, GroupName="Parameters")]
		public double NumStdDev
		{ 
			get {return numStdDev;}
			set {numStdDev = value;}
		}
		
		[Range(2, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BB period", Description="Number of bars used for calculations", Order=2, GroupName="Parameters")]
		public int Period
		{ 
			get {return period;}
			set {period = value;}
		}	
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Narrow Threashold (ticks)", Description="Number of ticks width to draw narrow color", Order=3, GroupName="Parameters")]
		public int Narrow_Tick_Threashold
		{ 
			get {return narrow_Tick_Threashold;}
			set {narrow_Tick_Threashold = value;}
		}		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Wide Theashold (ticks)", Description="Number of ticks to draw wide color", Order=4, GroupName="Parameters")]
		public int Wide_Tick_Threashold
		{ 
			get {return wide_Tick_Threashold;}
			set {wide_Tick_Threashold = value;}
		}
		
		[NinjaScriptProperty]
		[Display(Name="Display BB text data", Description="Shows current width of BB and user settings", Order=5, GroupName="Parameters")]
		public bool DisplayData
		{ 
			get {return displayData;}
			set {displayData = value;}
		}		
		
		[NinjaScriptProperty]
		[Display(Name="Show BB lines", Description="Show the Bollinger lines", Order=6, GroupName="Parameters")]
		public bool MyLines
		{ 
			get {
					if (!shading) myLines = true;
					return myLines;
				}
			set {myLines = value;}
		}

		[NinjaScriptProperty]
		[Display(Name="Color the BB region", Description="Color Bollinger bands regions", Order=7, GroupName="Parameters")]
		public bool Shading
		{ 
			get {
					if (!myLines) shading = true;
					return shading;
				}
			set {shading = value;}
		}
		[Range(0, 100)]
		[NinjaScriptProperty]
		[Display(Name="Region opacity", Description="Opacity of region color 0 - 100", Order=8, GroupName="Parameters")]
		public int Opacity
		{ 
			get {return opacity;}
			set {opacity = value;}
		}	
			
		
		[XmlIgnore]
		[Display(Name="BB Narrow Color", Description="Color when Bollinger bands meet narrow tick level", Order=9, GroupName="Parameters")]
		public Brush NarrowColor
		{ 
			get {return narrowColor;}
			set {narrowColor = value;}
		}

		[Browsable(false)]
		public string NarrowColorSerializable
		{
			get { return Serialize.BrushToString(narrowColor); }
			set { narrowColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="BB Neutral Color", Description="Region/Line Color when bollinger bands > narrow and < wide ", Order=10, GroupName="Parameters")]
		public Brush NeutralColor
		{ 
			get {return neutralColor;}
			set {neutralColor = value;}
		}

		[Browsable(false)]
		public string NeutralColorSerializable
		{
			get { return Serialize.BrushToString(neutralColor); }
			set { neutralColor = Serialize.StringToBrush(value); }
		}					

		[XmlIgnore]
		[Display(Name="BB Wide Color", Description="Region/Line Color when wide threashold met", Order=11, GroupName="Parameters")]
		public Brush WideColor
		{ 
			get {return wideColor;}
			set {wideColor = value;}
		}

		[Browsable(false)]
		public string WideColorSerializable
		{
			get { return Serialize.BrushToString(wideColor); }
			set { wideColor = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Middle
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerTripleState_V4NT8[] cacheBollingerTripleState_V4NT8;
		public BollingerTripleState_V4NT8 BollingerTripleState_V4NT8(double numStdDev, int period, int narrow_Tick_Threashold, int wide_Tick_Threashold, bool displayData, bool myLines, bool shading, int opacity)
		{
			return BollingerTripleState_V4NT8(Input, numStdDev, period, narrow_Tick_Threashold, wide_Tick_Threashold, displayData, myLines, shading, opacity);
		}

		public BollingerTripleState_V4NT8 BollingerTripleState_V4NT8(ISeries<double> input, double numStdDev, int period, int narrow_Tick_Threashold, int wide_Tick_Threashold, bool displayData, bool myLines, bool shading, int opacity)
		{
			if (cacheBollingerTripleState_V4NT8 != null)
				for (int idx = 0; idx < cacheBollingerTripleState_V4NT8.Length; idx++)
					if (cacheBollingerTripleState_V4NT8[idx] != null && cacheBollingerTripleState_V4NT8[idx].NumStdDev == numStdDev && cacheBollingerTripleState_V4NT8[idx].Period == period && cacheBollingerTripleState_V4NT8[idx].Narrow_Tick_Threashold == narrow_Tick_Threashold && cacheBollingerTripleState_V4NT8[idx].Wide_Tick_Threashold == wide_Tick_Threashold && cacheBollingerTripleState_V4NT8[idx].DisplayData == displayData && cacheBollingerTripleState_V4NT8[idx].MyLines == myLines && cacheBollingerTripleState_V4NT8[idx].Shading == shading && cacheBollingerTripleState_V4NT8[idx].Opacity == opacity && cacheBollingerTripleState_V4NT8[idx].EqualsInput(input))
						return cacheBollingerTripleState_V4NT8[idx];
			return CacheIndicator<BollingerTripleState_V4NT8>(new BollingerTripleState_V4NT8(){ NumStdDev = numStdDev, Period = period, Narrow_Tick_Threashold = narrow_Tick_Threashold, Wide_Tick_Threashold = wide_Tick_Threashold, DisplayData = displayData, MyLines = myLines, Shading = shading, Opacity = opacity }, input, ref cacheBollingerTripleState_V4NT8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerTripleState_V4NT8 BollingerTripleState_V4NT8(double numStdDev, int period, int narrow_Tick_Threashold, int wide_Tick_Threashold, bool displayData, bool myLines, bool shading, int opacity)
		{
			return indicator.BollingerTripleState_V4NT8(Input, numStdDev, period, narrow_Tick_Threashold, wide_Tick_Threashold, displayData, myLines, shading, opacity);
		}

		public Indicators.BollingerTripleState_V4NT8 BollingerTripleState_V4NT8(ISeries<double> input , double numStdDev, int period, int narrow_Tick_Threashold, int wide_Tick_Threashold, bool displayData, bool myLines, bool shading, int opacity)
		{
			return indicator.BollingerTripleState_V4NT8(input, numStdDev, period, narrow_Tick_Threashold, wide_Tick_Threashold, displayData, myLines, shading, opacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerTripleState_V4NT8 BollingerTripleState_V4NT8(double numStdDev, int period, int narrow_Tick_Threashold, int wide_Tick_Threashold, bool displayData, bool myLines, bool shading, int opacity)
		{
			return indicator.BollingerTripleState_V4NT8(Input, numStdDev, period, narrow_Tick_Threashold, wide_Tick_Threashold, displayData, myLines, shading, opacity);
		}

		public Indicators.BollingerTripleState_V4NT8 BollingerTripleState_V4NT8(ISeries<double> input , double numStdDev, int period, int narrow_Tick_Threashold, int wide_Tick_Threashold, bool displayData, bool myLines, bool shading, int opacity)
		{
			return indicator.BollingerTripleState_V4NT8(input, numStdDev, period, narrow_Tick_Threashold, wide_Tick_Threashold, displayData, myLines, shading, opacity);
		}
	}
}

#endregion
