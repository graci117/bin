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
	public class MACDImpulse : Indicator
	{
		Series<double> hi;
	    Series<double> lo;
	    Series<double> mi;
		Series<double> smma;
		EMA EMA1;
		EMA EMA2;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MACDImpulse";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				LengthMA					= 34;
				LengthSignal					= 9;
				AddPlot(Brushes.Coral, "Mid");
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Bar, "ImpulseMACD");
				AddPlot(new Stroke(Brushes.Chartreuse, 2), PlotStyle.Bar, "ImpulseHist");
				AddPlot(Brushes.PaleVioletRed, "ImpulseMACDSignal");
			}
			else if (State == State.Configure)
			{
			
				
				hi = new Series<double>(this);
			    lo = new Series<double>(this);
			    mi = new Series<double>(this);
				smma = new Series<double>(this);
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 100)
				return;
			//Print(High[0]);
	     	hi[0] = calc_smma(PriceType.High,LengthMA);
			
			lo[0] = calc_smma(PriceType.Low,LengthMA);
			
			mi[0] = calc_zlema(PriceType.Typical, LengthMA);
			

	        ImpulseMACD[0] = (mi[0] > hi[0]) ? (mi[0] - hi[0]) : (mi[0] < lo[0]) ? (mi[0] - lo[0]) : 0;
			//Print("2l"+ImpulseMACD[0]);
			
	        ImpulseMACDSignal[0] = SMA(ImpulseMACD, LengthSignal)[0];
	        ImpulseHist[0] = ImpulseMACD[0] - ImpulseMACDSignal[0];

	        // Plot
	        Values[0][0] = 0; // MidLine
	        Values[1][0] = ImpulseMACD[0]; // ImpulseMACD
	        Values[2][0] = ImpulseHist[0]; // ImpulseHisto
	        Values[3][0] = ImpulseMACDSignal[0]; // ImpulseMACDCDSignal
			
			
			PlotBrushes[2][0] = Brushes.Blue;
			PlotBrushes[1][0] = Typical[0]>mi[0]?Typical[0]>hi[0]?Brushes.Lime:Brushes.Green:Typical[0]<lo[0]?Brushes.Red:Brushes.Orange;
			

        // Bar colors
        
		}
		
		private double calc_zlema(PriceType src, int length)
	    {
			Series<double> ema1;
			if (src == PriceType.Typical)
	        	ema1 = EMA(Typical, length).Value;
			else if (src == PriceType.High)
				ema1 = EMA(High, length).Value;
			else if (src == PriceType.Low)
				ema1 = EMA(Low, length).Value;
			else if (src == PriceType.Close)
				ema1 = EMA(Close, length).Value;
			else if (src == PriceType.Median)
				ema1 = EMA(Median, length).Value;
			else if (src == PriceType.Weighted)
				ema1 = EMA(Weighted, length).Value;
			else
				ema1 = EMA(Typical, length).Value;
	        Series<double> ema2 = EMA(ema1, length).Value;
	        double d = ema1[0] - ema2[0];
	        return ema1[0] + d;
	    }
		
		private double calc_smma(PriceType src, int length)
	    {
	        double smmaValue;
			
			
			
	        if (smma[1] == 0)
	        {
				
	            // Initialize smma on the first bar or if it's null/out of range
				
					if (src == PriceType.Typical)
					{
						smma = SMA(Typical, length).Value;
			        	smmaValue = smma[0];
					}
					else if (src == PriceType.High)
					{
						smma = SMA(High, length).Value;
			        	smmaValue = smma[0];
					}
						else if (src == PriceType.Low)
					{
						smma = SMA(Low, length).Value;
			        	smmaValue = smma[0];
					}
						else if (src == PriceType.Close)
					{
						smma = SMA(Close, length).Value;
			        	smmaValue = smma[0];
					}
						else if (src == PriceType.Median)
					{
						smma = SMA(Median, length).Value;
			        	smmaValue = smma[0];
					}
						else if (src == PriceType.Weighted)
					{
						smma = SMA(Weighted, length).Value;
			        	smmaValue = smma[0];
					}
						else 
					{
						smma = SMA(Close, length).Value;
			        	smmaValue = smma[0];
					}				
					
				
	        }
	        else
	        {
	            // Calculate smma
				
				if (src == PriceType.Typical)
		        	smmaValue = (smma[1] * (length - 1) + Typical[0]) / length;
				else if (src == PriceType.High)
				{
					//Print(smma[1]);
					smmaValue = (smma[1] * (length - 1) + High[0]) / length;
					
				}
				else if (src == PriceType.Low)
					smmaValue = (smma[1] * (length - 1) + Low[0]) / length;
				else if (src == PriceType.Close)
					smmaValue = (smma[1] * (length - 1) + Close[0]) / length;
				else if (src == PriceType.Median)
					smmaValue = (smma[1] * (length - 1) + Median[0]) / length;
				else if (src == PriceType.Weighted)
					smmaValue = (smma[1] * (length - 1) + Weighted[0]) / length;
				else
					smmaValue = (smma[1] * (length - 1) + Typical[0]) / length;
	        }

	        return smmaValue;
	    }

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LengthMA", Order=1, GroupName="Parameters")]
		public int LengthMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LengthSignal", Order=2, GroupName="Parameters")]
		public int LengthSignal
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Mid
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ImpulseMACD
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ImpulseHist
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ImpulseMACDSignal
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDImpulse[] cacheMACDImpulse;
		public MACDImpulse MACDImpulse(int lengthMA, int lengthSignal)
		{
			return MACDImpulse(Input, lengthMA, lengthSignal);
		}

		public MACDImpulse MACDImpulse(ISeries<double> input, int lengthMA, int lengthSignal)
		{
			if (cacheMACDImpulse != null)
				for (int idx = 0; idx < cacheMACDImpulse.Length; idx++)
					if (cacheMACDImpulse[idx] != null && cacheMACDImpulse[idx].LengthMA == lengthMA && cacheMACDImpulse[idx].LengthSignal == lengthSignal && cacheMACDImpulse[idx].EqualsInput(input))
						return cacheMACDImpulse[idx];
			return CacheIndicator<MACDImpulse>(new MACDImpulse(){ LengthMA = lengthMA, LengthSignal = lengthSignal }, input, ref cacheMACDImpulse);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDImpulse MACDImpulse(int lengthMA, int lengthSignal)
		{
			return indicator.MACDImpulse(Input, lengthMA, lengthSignal);
		}

		public Indicators.MACDImpulse MACDImpulse(ISeries<double> input , int lengthMA, int lengthSignal)
		{
			return indicator.MACDImpulse(input, lengthMA, lengthSignal);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDImpulse MACDImpulse(int lengthMA, int lengthSignal)
		{
			return indicator.MACDImpulse(Input, lengthMA, lengthSignal);
		}

		public Indicators.MACDImpulse MACDImpulse(ISeries<double> input , int lengthMA, int lengthSignal)
		{
			return indicator.MACDImpulse(input, lengthMA, lengthSignal);
		}
	}
}

#endregion
