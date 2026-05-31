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
	public class TillsonT3 : Indicator
	{
		private EMA E1;
		private EMA E2;
		private EMA E3;
		private EMA E4;
		private EMA E5;
		private EMA E6;
		private double C1;
		private double C2;
		private double C3;
		private double C4;
		private Series<double> T3;
		private bool Col1;
		private bool Col3;
		private Brush Color1;

		
		
		
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TillsonT3";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				T3Length					= 8;
				VolumeFactor					= 0.7;
				AddPlot(new Stroke(Brushes.Yellow, 3), PlotStyle.Line, "T3Plot");
			}
			else if (State == State.Configure)
			{
				T3 = new Series<double>(this);
			}
			
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			E1 = EMA(Close, T3Length);
		    E2 = EMA(E1, T3Length);
		    E3 = EMA(E2, T3Length);
		    E4 = EMA(E3, T3Length);
		    E5 = EMA(E4, T3Length);
		    E6 = EMA(E5, T3Length);

		    C1 = -VolumeFactor * VolumeFactor * VolumeFactor;
		    C2 = 3 * VolumeFactor * VolumeFactor + 3 * VolumeFactor * VolumeFactor * VolumeFactor;
		    C3 = -6 * VolumeFactor * VolumeFactor - 3 * VolumeFactor - 3 * VolumeFactor * VolumeFactor * VolumeFactor;
		    C4 = 1 + 3 * VolumeFactor + VolumeFactor * VolumeFactor * VolumeFactor + 3 * VolumeFactor * VolumeFactor;

		    Col1 = false;
		    Col3 = false;
		    Color1 = Brushes.Yellow;

			 Value[0] = C1 * E6[0] + C2 * E5[0] + C3 * E4[0] + C4 * E3[0];

//		    if (T3[0] > T3[1])
//		    {
//		        Col1 = true;
//		        Col3 = false;
//		        Color1 = Brushes.Green;
//		    }
//		    else if (T3[0] < T3[1])
//		    {
//		        Col1 = false;
//		        Col3 = true;
//		        Color1 = Brushes.Red;
//		    }
			
			if(IsRising(Value)) {PlotBrushes[0][0] = Brushes.Green;}
			else if(IsFalling(Value)) {PlotBrushes[0][0] = Brushes.Red;}
			else {PlotBrushes[0][0] = Brushes.Yellow;}
			
			//T3Plot[0] =  T3[0] ;
		    //PlotBrushes[0][0] = Col1 ? Brushes.Green : Col3 ? Brushes.Red : Brushes.Yellow;
		    
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="T3Length", Order=1, GroupName="Parameters")]
		public int T3Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="VolumeFactor", Order=2, GroupName="Parameters")]
		public double VolumeFactor
		{ get; set; }
		
//		[Browsable(false)]
//		[XmlIgnore]
//		public Series<double> T3Plot
//		{
//			get { return Values[0]; }
//		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TillsonT3[] cacheTillsonT3;
		public TillsonT3 TillsonT3(int t3Length, double volumeFactor)
		{
			return TillsonT3(Input, t3Length, volumeFactor);
		}

		public TillsonT3 TillsonT3(ISeries<double> input, int t3Length, double volumeFactor)
		{
			if (cacheTillsonT3 != null)
				for (int idx = 0; idx < cacheTillsonT3.Length; idx++)
					if (cacheTillsonT3[idx] != null && cacheTillsonT3[idx].T3Length == t3Length && cacheTillsonT3[idx].VolumeFactor == volumeFactor && cacheTillsonT3[idx].EqualsInput(input))
						return cacheTillsonT3[idx];
			return CacheIndicator<TillsonT3>(new TillsonT3(){ T3Length = t3Length, VolumeFactor = volumeFactor }, input, ref cacheTillsonT3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TillsonT3 TillsonT3(int t3Length, double volumeFactor)
		{
			return indicator.TillsonT3(Input, t3Length, volumeFactor);
		}

		public Indicators.TillsonT3 TillsonT3(ISeries<double> input , int t3Length, double volumeFactor)
		{
			return indicator.TillsonT3(input, t3Length, volumeFactor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TillsonT3 TillsonT3(int t3Length, double volumeFactor)
		{
			return indicator.TillsonT3(Input, t3Length, volumeFactor);
		}

		public Indicators.TillsonT3 TillsonT3(ISeries<double> input , int t3Length, double volumeFactor)
		{
			return indicator.TillsonT3(input, t3Length, volumeFactor);
		}
	}
}

#endregion
