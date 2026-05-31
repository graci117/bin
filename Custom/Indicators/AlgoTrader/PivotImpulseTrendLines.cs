// --- PivotImpulseTrendLines.cs ---
// Version 1.3 - ADD MISSING PIVOTLOOKBACK PARAMETER
// Key Changes:
// 1. FIX (COMPILE ERROR): Added the public 'PivotLookback' property that was missing, which caused the CS1061 error.
// 2. REFACTOR (LOGIC): The indicator now uses this new parameter for its main trend calculation, making it
//    configurable from the strategy.

#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace is used to contain indicators that are compiled into NinjaTrader.exe
namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class PivotImpulseTrendLines : Indicator
	{
		private Swing swing;
		
		private Brush upBrush;
		private Brush downBrush;
		private string creator;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Draws two lines: one for the main visible trend (pivots) and one for the current impulse wave. Colors the background based on the main trend.";
				Name										= "Pivot Impulse Trend Lines";
				creator 									= "Khanh Nguyen";
				IsOverlay									= true;
				DisplayInDataBox							= false;
				DrawOnPricePanel							= true;
				IsSuspendedWhileInactive					= true;
				PaintPriceMarkers							= false;
				Calculate 									= Calculate.OnBarClose;

				// Default Parameters
				SwingStrength								= 40;
				PivotLookback								= 200; // Added default value
				PivotLineColor								= Brushes.Yellow;
				PivotLineWidth								= 2;
				ImpulseUpColor								= Brushes.Lime;
				ImpulseDownColor							= Brushes.Red;
				ImpulseLineWidth							= 2;
				UpBgColor									= Color.FromArgb(30, 0, 128, 0);
				DownBgColor									= Color.FromArgb(30, 139, 0, 0);
				
				// Initialize public properties
				IsPivotTrendUp 		= new Series<bool>(this);
				IsImpulseUp 		= new Series<bool>(this);
			}
			else if (State == State.Configure)
			{
				swing = Swing(SwingStrength);
				
				upBrush 	= new SolidColorBrush(UpBgColor);
				downBrush 	= new SolidColorBrush(DownBgColor);
				
				if(upBrush.CanFreeze) upBrush.Freeze();
				if(downBrush.CanFreeze) downBrush.Freeze();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(SwingStrength, PivotLookback))
				return;

			// --- 1. Calculate and Draw Main Pivot Trend & Background ---
			DrawMainPivotLineAndBackground();
			
			// --- 2. Calculate and Draw Impulse Line ---
			DrawImpulseLine();
		}

		private void DrawMainPivotLineAndBackground()
		{
			int highestHighBar 	= HighestBar(High, PivotLookback);
			double highestHigh 	= High[highestHighBar];
			int lowestLowBar 	= LowestBar(Low, PivotLookback);
			double lowestLow 	= Low[lowestLowBar];
			
			if (highestHighBar < lowestLowBar)
			{
				BackBrush = upBrush;
				IsPivotTrendUp[0] = true;
			}
			else if (lowestLowBar < highestHighBar)
			{
				BackBrush = downBrush;
				IsPivotTrendUp[0] = false;
			}
			else
			{
				BackBrush = null;
			}

			Draw.Line(this, "MainPivotLine", false, lowestLowBar, lowestLow, highestHighBar, highestHigh, PivotLineColor, DashStyleHelper.Solid, PivotLineWidth);
		}

		private void DrawImpulseLine()
		{
			int barsAgoHigh = swing.SwingHighBar(0, 1, CurrentBar);
			int barsAgoLow 	= swing.SwingLowBar(0, 1, CurrentBar);

			if (barsAgoHigh < 0 || barsAgoLow < 0 || barsAgoHigh >= CurrentBar || barsAgoLow >= CurrentBar)
				return;

			if (barsAgoLow < barsAgoHigh)
			{
				IsImpulseUp[0] = true;
				double startPrice 	= Low[barsAgoLow]; 
				double endPrice 	= High[0];
				Draw.Line(this, "ImpulseLine", false, barsAgoLow, startPrice, 0, endPrice, ImpulseUpColor, DashStyleHelper.Solid, ImpulseLineWidth);
			}
			else
			{
				IsImpulseUp[0] = false;
				double startPrice 	= High[barsAgoHigh];
				double endPrice 	= Low[0];
				Draw.Line(this, "ImpulseLine", false, barsAgoHigh, startPrice, 0, endPrice, ImpulseDownColor, DashStyleHelper.Solid, ImpulseLineWidth);
			}
		}

		#region Public Properties (for bot access)
		[Browsable(false)]
		[XmlIgnore]
		public Series<bool> IsPivotTrendUp { get; private set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<bool> IsImpulseUp { get; private set; }
		#endregion

		#region Parameters
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Swing Strength", Description="Determines the significance of the pivots for the Impulse Line.", Order=1, GroupName="Parameters")]
		public int SwingStrength
		{ get; set; }
		
		[NinjaScriptProperty] // *** THIS WAS THE MISSING PROPERTY ***
		[Range(10, int.MaxValue)]
		[Display(Name="Pivot Lookback", Description="Lookback period for the main trend line.", Order=2, GroupName="Parameters")]
		public int PivotLookback
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Pivot Line Color", Order=3, GroupName="Visuals")]
		public Brush PivotLineColor
		{ get; set; }

		[Browsable(false)]
		public string PivotLineColorSerializable
		{
			get { return Serialize.BrushToString(PivotLineColor); }
			set { PivotLineColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(1, 20)]
		[Display(Name="Pivot Line Width", Order=4, GroupName="Visuals")]
		public int PivotLineWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Impulse Up Color", Order=5, GroupName="Visuals")]
		public Brush ImpulseUpColor
		{ get; set; }

		[Browsable(false)]
		public string ImpulseUpColorSerializable
		{
			get { return Serialize.BrushToString(ImpulseUpColor); }
			set { ImpulseUpColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Impulse Down Color", Order=6, GroupName="Visuals")]
		public Brush ImpulseDownColor
		{ get; set; }

		[Browsable(false)]
		public string ImpulseDownColorSerializable
		{
			get { return Serialize.BrushToString(ImpulseDownColor); }
			set { ImpulseDownColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, 20)]
		[Display(Name="Impulse Line Width", Order=7, GroupName="Visuals")]
		public int ImpulseLineWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Up Trend BG Color", Order=8, GroupName="Visuals")]
		public Color UpBgColor
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Down Trend BG Color", Order=9, GroupName="Visuals")]
		public Color DownBgColor
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.PivotImpulseTrendLines[] cachePivotImpulseTrendLines;
		public AlgoTrader.PivotImpulseTrendLines PivotImpulseTrendLines(int swingStrength, int pivotLookback, Brush pivotLineColor, int pivotLineWidth, Brush impulseUpColor, Brush impulseDownColor, int impulseLineWidth, Color upBgColor, Color downBgColor)
		{
			return PivotImpulseTrendLines(Input, swingStrength, pivotLookback, pivotLineColor, pivotLineWidth, impulseUpColor, impulseDownColor, impulseLineWidth, upBgColor, downBgColor);
		}

		public AlgoTrader.PivotImpulseTrendLines PivotImpulseTrendLines(ISeries<double> input, int swingStrength, int pivotLookback, Brush pivotLineColor, int pivotLineWidth, Brush impulseUpColor, Brush impulseDownColor, int impulseLineWidth, Color upBgColor, Color downBgColor)
		{
			if (cachePivotImpulseTrendLines != null)
				for (int idx = 0; idx < cachePivotImpulseTrendLines.Length; idx++)
					if (cachePivotImpulseTrendLines[idx] != null && cachePivotImpulseTrendLines[idx].SwingStrength == swingStrength && cachePivotImpulseTrendLines[idx].PivotLookback == pivotLookback && cachePivotImpulseTrendLines[idx].PivotLineColor == pivotLineColor && cachePivotImpulseTrendLines[idx].PivotLineWidth == pivotLineWidth && cachePivotImpulseTrendLines[idx].ImpulseUpColor == impulseUpColor && cachePivotImpulseTrendLines[idx].ImpulseDownColor == impulseDownColor && cachePivotImpulseTrendLines[idx].ImpulseLineWidth == impulseLineWidth && cachePivotImpulseTrendLines[idx].UpBgColor == upBgColor && cachePivotImpulseTrendLines[idx].DownBgColor == downBgColor && cachePivotImpulseTrendLines[idx].EqualsInput(input))
						return cachePivotImpulseTrendLines[idx];
			return CacheIndicator<AlgoTrader.PivotImpulseTrendLines>(new AlgoTrader.PivotImpulseTrendLines(){ SwingStrength = swingStrength, PivotLookback = pivotLookback, PivotLineColor = pivotLineColor, PivotLineWidth = pivotLineWidth, ImpulseUpColor = impulseUpColor, ImpulseDownColor = impulseDownColor, ImpulseLineWidth = impulseLineWidth, UpBgColor = upBgColor, DownBgColor = downBgColor }, input, ref cachePivotImpulseTrendLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.PivotImpulseTrendLines PivotImpulseTrendLines(int swingStrength, int pivotLookback, Brush pivotLineColor, int pivotLineWidth, Brush impulseUpColor, Brush impulseDownColor, int impulseLineWidth, Color upBgColor, Color downBgColor)
		{
			return indicator.PivotImpulseTrendLines(Input, swingStrength, pivotLookback, pivotLineColor, pivotLineWidth, impulseUpColor, impulseDownColor, impulseLineWidth, upBgColor, downBgColor);
		}

		public Indicators.AlgoTrader.PivotImpulseTrendLines PivotImpulseTrendLines(ISeries<double> input , int swingStrength, int pivotLookback, Brush pivotLineColor, int pivotLineWidth, Brush impulseUpColor, Brush impulseDownColor, int impulseLineWidth, Color upBgColor, Color downBgColor)
		{
			return indicator.PivotImpulseTrendLines(input, swingStrength, pivotLookback, pivotLineColor, pivotLineWidth, impulseUpColor, impulseDownColor, impulseLineWidth, upBgColor, downBgColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.PivotImpulseTrendLines PivotImpulseTrendLines(int swingStrength, int pivotLookback, Brush pivotLineColor, int pivotLineWidth, Brush impulseUpColor, Brush impulseDownColor, int impulseLineWidth, Color upBgColor, Color downBgColor)
		{
			return indicator.PivotImpulseTrendLines(Input, swingStrength, pivotLookback, pivotLineColor, pivotLineWidth, impulseUpColor, impulseDownColor, impulseLineWidth, upBgColor, downBgColor);
		}

		public Indicators.AlgoTrader.PivotImpulseTrendLines PivotImpulseTrendLines(ISeries<double> input , int swingStrength, int pivotLookback, Brush pivotLineColor, int pivotLineWidth, Brush impulseUpColor, Brush impulseDownColor, int impulseLineWidth, Color upBgColor, Color downBgColor)
		{
			return indicator.PivotImpulseTrendLines(input, swingStrength, pivotLookback, pivotLineColor, pivotLineWidth, impulseUpColor, impulseDownColor, impulseLineWidth, upBgColor, downBgColor);
		}
	}
}

#endregion
