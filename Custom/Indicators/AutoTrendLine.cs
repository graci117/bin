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
	// Converted from NT7 6/5/2016
	// Added transparenty plot for Market analyzer input
	
	public class AutoTrendLine : Indicator
	{
		private int signal; // Provided for output to strategy:   0 = no signal, 1 = buy signal on down trend break, 2 = sell signal on up trend break
		private int triggerBarIndex = 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Automatically draws a line representing the current trend and generates an alert if the trend line is broken.";
				Name										= "AutoTrendLine";
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
				
				ArePlotsConfigurable 						= false; // Plots are not configurable in the indicator dialog
				AlertOnBreak								= true;
				LineWidth									= 1;
				Strength									= 5;
				DownTrendColor								= Brushes.Red;
				UpTrendColor								= Brushes.Lime;
				SoundFile									= @"C:\Program Files (x86)\NinjaTrader 8\sounds\Alert2.wav";
				AddPlot(Brushes.Transparent, "AutoTrendLineBreak");		// Provided for signal input to Market analyzer, 0 = no signal, 1 = buy signal on down trend break, 2 = sell signal on up trend break
			}
		}

		protected override void OnBarUpdate()
		{
			if (IsFirstTickOfBar)  // maintain signal for duration of bar (is using calculate.OET or OPC)
			{
				signal					= 0;  // signal (used in strategy)
				AutoTrendLineBreak[0] 	= 0;  // can be used in strategy or Market analyzer
			}
			
			// Calculate up trend line
			int upTrendStartBarsAgo		= 0;
			int upTrendEndBarsAgo 		= 0;
			int upTrendOccurence 		= 1;
			
			while (Low[upTrendEndBarsAgo] <= Low[upTrendStartBarsAgo])
			{
				upTrendStartBarsAgo 	= Swing(Strength).SwingLowBar(0, upTrendOccurence + 1, CurrentBar);
				upTrendEndBarsAgo 		= Swing(Strength).SwingLowBar(0, upTrendOccurence, CurrentBar);
					
				if (upTrendStartBarsAgo < 0 || upTrendEndBarsAgo < 0)
					break;

				upTrendOccurence++;
			}
			
			
			// Calculate down trend line	
			int downTrendStartBarsAgo	= 0;
			int downTrendEndBarsAgo 	= 0;
			int downTrendOccurence 		= 1;
			
			while (High[downTrendEndBarsAgo] >= High[downTrendStartBarsAgo])
			{
				downTrendStartBarsAgo 		= Swing(Strength).SwingHighBar(0, downTrendOccurence + 1, CurrentBar);
				downTrendEndBarsAgo 		= Swing(Strength).SwingHighBar(0, downTrendOccurence, CurrentBar);
					
				if (downTrendStartBarsAgo < 0 || downTrendEndBarsAgo < 0)
					break;
					
				downTrendOccurence++;
			}
			
			
			// Always clear out arrows that mark trend line breaks
			RemoveDrawObject("DownTrendBreak");							
			RemoveDrawObject("UpTrendBreak");
			
			
			// We have found an uptrend and the uptrend is the current trend
			if (upTrendStartBarsAgo > 0 && upTrendEndBarsAgo > 0 && upTrendStartBarsAgo < downTrendStartBarsAgo)
			{
				RemoveDrawObject("DownTrendLine");
				
				// Reset the alert if required
				if (triggerBarIndex != CurrentBar - upTrendEndBarsAgo)
				{
					triggerBarIndex = 0;
					RearmAlert("Alert");
				}
				
				double startBarPrice 	= Low[upTrendStartBarsAgo];
				double endBarPrice 		= Low[upTrendEndBarsAgo];
				double changePerBar 	= (endBarPrice - startBarPrice) / (Math.Abs(upTrendEndBarsAgo - upTrendStartBarsAgo));
				
				// Draw the up trend line
				Draw.Ray(this, "UpTrendLine", true, upTrendStartBarsAgo, startBarPrice, upTrendEndBarsAgo, endBarPrice, UpTrendColor, DashStyleHelper.Solid, LineWidth);

				// Check for an uptrend line break
				for (int barsAgo = upTrendEndBarsAgo - 1; barsAgo >= 0; barsAgo--) 
				{
					if (Close[barsAgo] < endBarPrice + (Math.Abs(upTrendEndBarsAgo - barsAgo) * changePerBar))
					{
						Draw.ArrowDown(this, "UpTrendBreak", true,  barsAgo, High[barsAgo] + TickSize, Brushes.Blue);
					
						// Set the signal only if the break is on the right most bar
						if (barsAgo == 0)
						{
							signal 					= 2;
							AutoTrendLineBreak[0]   = 2;
						}
						
						// Alert will only trigger in real-time
						if (AlertOnBreak && triggerBarIndex == 0)
						{
							triggerBarIndex = CurrentBar - upTrendEndBarsAgo;
							Alert("Alert", Priority.High, "Up trend line broken", SoundFile, 100000, Brushes.Black, Brushes.Red);
						}
						
						break;
					}
				}
			}
			// We have found a downtrend and the downtrend is the current trend
			else if (downTrendStartBarsAgo > 0 && downTrendEndBarsAgo > 0  && upTrendStartBarsAgo > downTrendStartBarsAgo)
			{
				RemoveDrawObject("UpTrendLine");
				
				// Reset the alert if required
				if (triggerBarIndex != CurrentBar - downTrendEndBarsAgo)
				{
					triggerBarIndex = 0;
					RearmAlert("Alert");
				}
				
				double startBarPrice 	= High[downTrendStartBarsAgo];
				double endBarPrice 		= High[downTrendEndBarsAgo];
				double changePerBar 	= (endBarPrice - startBarPrice) / (Math.Abs(downTrendEndBarsAgo - downTrendStartBarsAgo));
				
				// Draw the down trend line
				Draw.Ray(this, "DownTrendLine", true, downTrendStartBarsAgo, startBarPrice, downTrendEndBarsAgo, endBarPrice, DownTrendColor, DashStyleHelper.Solid, LineWidth);

				// Check for a down trend line break
				for (int barsAgo = downTrendEndBarsAgo - 1; barsAgo >= 0; barsAgo--) 
				{
					if (Close[barsAgo] > endBarPrice + (Math.Abs(downTrendEndBarsAgo - barsAgo) * changePerBar))
					{
						Draw.ArrowUp(this, "DownTrendBreak", true, barsAgo, Low[barsAgo] - TickSize, Brushes.Blue);
						
						// Set the signal only if the break is on the right most bar
						if (barsAgo == 0)
						{
							signal					= 1;
							AutoTrendLineBreak[0]  	= 1;
						}
						
						// Alert will only trigger in real-time
						if (AlertOnBreak && triggerBarIndex == 0)
						{
							triggerBarIndex = CurrentBar - downTrendEndBarsAgo;
							Alert("Alert", Priority.High, "Down trend line broken", SoundFile, 100000, Brushes.Black, Brushes.Green);
						}
						
						break;
					}
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Alert On Break", Order=1, GroupName="Parameters")]
		public bool AlertOnBreak
		{ get; set; }
		
		//[NinjaScriptProperty]
		[Display(Name="Alert sound file", Order=2, GroupName="Parameters")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string SoundFile
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Line width", Description="Trend line width", Order=3, GroupName="Parameters")]
		public int LineWidth
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Strength", Description="Number of bars for swing strength", Order=4, GroupName="Parameters")]
		public int Strength
		{ get; set; }

		[XmlIgnore]
		[Display(Name="DownTrend Color", Order=5, GroupName="Parameters")]
		public Brush DownTrendColor
		{ get; set; }

		[Browsable(false)]
		public string DownTrendColorSerializable
		{
			get { return Serialize.BrushToString(DownTrendColor); }
			set { DownTrendColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="UpTrend Color", Order=6, GroupName="Parameters")]
		public Brush UpTrendColor
		{ get; set; }

		[Browsable(false)]
		public string UpTrendColorSerializable
		{
			get { return Serialize.BrushToString(UpTrendColor); }
			set { UpTrendColor = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AutoTrendLineBreak
		{
			get { return Values[0]; }
		}
		[Browsable(false)]
		[XmlIgnore]		
		public int Signal
		{
			get { Update(); return signal; }
		}		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AutoTrendLine[] cacheAutoTrendLine;
		public AutoTrendLine AutoTrendLine(bool alertOnBreak, int lineWidth, int strength)
		{
			return AutoTrendLine(Input, alertOnBreak, lineWidth, strength);
		}

		public AutoTrendLine AutoTrendLine(ISeries<double> input, bool alertOnBreak, int lineWidth, int strength)
		{
			if (cacheAutoTrendLine != null)
				for (int idx = 0; idx < cacheAutoTrendLine.Length; idx++)
					if (cacheAutoTrendLine[idx] != null && cacheAutoTrendLine[idx].AlertOnBreak == alertOnBreak && cacheAutoTrendLine[idx].LineWidth == lineWidth && cacheAutoTrendLine[idx].Strength == strength && cacheAutoTrendLine[idx].EqualsInput(input))
						return cacheAutoTrendLine[idx];
			return CacheIndicator<AutoTrendLine>(new AutoTrendLine(){ AlertOnBreak = alertOnBreak, LineWidth = lineWidth, Strength = strength }, input, ref cacheAutoTrendLine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AutoTrendLine AutoTrendLine(bool alertOnBreak, int lineWidth, int strength)
		{
			return indicator.AutoTrendLine(Input, alertOnBreak, lineWidth, strength);
		}

		public Indicators.AutoTrendLine AutoTrendLine(ISeries<double> input , bool alertOnBreak, int lineWidth, int strength)
		{
			return indicator.AutoTrendLine(input, alertOnBreak, lineWidth, strength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AutoTrendLine AutoTrendLine(bool alertOnBreak, int lineWidth, int strength)
		{
			return indicator.AutoTrendLine(Input, alertOnBreak, lineWidth, strength);
		}

		public Indicators.AutoTrendLine AutoTrendLine(ISeries<double> input , bool alertOnBreak, int lineWidth, int strength)
		{
			return indicator.AutoTrendLine(input, alertOnBreak, lineWidth, strength);
		}
	}
}

#endregion
