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
	public class AnchoredMomentumOscillator : Indicator
	{
		
		private Series<double> t_amom;
        private Series<double> amom;
        private Series<double> amoms;
        private bool enableBarcolors;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Anchored Momentum";
				Name										= "AnchoredMomentumOscillator";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				MomentumPeriod					= 10;
				SignalPeriod					= 8;
				SmoothMomentum					= false;
				SmoothingPeriod					= 7;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Histogram");
				AddLine(Brushes.Gray,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
			}
			else if (State == State.Configure)
			{
				
				t_amom = new Series<double>(this);
                amom = new Series<double>(this);
                amoms = new Series<double>(this);
				AnchoredMomentum = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			 if (CurrentBar < 35)
                return;
			 
			 
			int  amom_p = 2 * MomentumPeriod + 1;
            double t_amomValue = SmoothMomentum ? EMA(SmoothingPeriod)[0] : Close[0];
            double amomValue = 100 * ((t_amomValue / SMA(amom_p)[0]) - 1);

            t_amom[0] = t_amomValue;
            amom[0] = amomValue;
            amoms[0] = SMA(amom, SignalPeriod)[0];
			 
			 
			double hlValue = 0;

            if (amoms[0] < 0 && amom[0] < 0)
                hlValue = Math.Max(amoms[0], amom[0]);
            else 
			{
				if (amoms[0] > 0 && amom[0] > 0)
                	hlValue = Math.Min(amoms[0], amom[0]);
				else
					hlValue = 0;
			}
			
			AnchoredMomentum[0] = amom[0];

            Histogram[0] = hlValue;
			
			if (amom[0] > amoms[0])
			{
				if (amom[0] < 0) 
				{ 
					PlotBrushes[0][0] = Brushes.Orange;
				}
				else 
				{
					PlotBrushes[0][0] = Brushes.Green;
				}
			}
			else
			{
				if (amom[0] < 0)
				{
					PlotBrushes[0][0] = Brushes.Red;
				
				}
				else 
				{
					PlotBrushes[0][0] = Brushes.Orange;
				}
			}

           
			
		
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MomentumPeriod", Description="MomentumPeriod", Order=1, GroupName="Parameters")]
		public int MomentumPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SignalPeriod", Description="SignalPeriod", Order=2, GroupName="Parameters")]
		public int SignalPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SmoothMomentum", Description="SmoothMomentum?", Order=3, GroupName="Parameters")]
		public bool SmoothMomentum
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmoothingPeriod", Description="SmoothingPeriod", Order=4, GroupName="Parameters")]
		public int SmoothingPeriod
		{ get; set; }
		
		[Browsable(false)]		
		[Display(Name="AnchoredMomentum", Description="AnchoredMomentum", Order=4, GroupName="Parameters")]
		public Series<double> AnchoredMomentum
		{ get; set; }
		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Histogram
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AnchoredMomentumOscillator[] cacheAnchoredMomentumOscillator;
		public AnchoredMomentumOscillator AnchoredMomentumOscillator(int momentumPeriod, int signalPeriod, bool smoothMomentum, int smoothingPeriod)
		{
			return AnchoredMomentumOscillator(Input, momentumPeriod, signalPeriod, smoothMomentum, smoothingPeriod);
		}

		public AnchoredMomentumOscillator AnchoredMomentumOscillator(ISeries<double> input, int momentumPeriod, int signalPeriod, bool smoothMomentum, int smoothingPeriod)
		{
			if (cacheAnchoredMomentumOscillator != null)
				for (int idx = 0; idx < cacheAnchoredMomentumOscillator.Length; idx++)
					if (cacheAnchoredMomentumOscillator[idx] != null && cacheAnchoredMomentumOscillator[idx].MomentumPeriod == momentumPeriod && cacheAnchoredMomentumOscillator[idx].SignalPeriod == signalPeriod && cacheAnchoredMomentumOscillator[idx].SmoothMomentum == smoothMomentum && cacheAnchoredMomentumOscillator[idx].SmoothingPeriod == smoothingPeriod && cacheAnchoredMomentumOscillator[idx].EqualsInput(input))
						return cacheAnchoredMomentumOscillator[idx];
			return CacheIndicator<AnchoredMomentumOscillator>(new AnchoredMomentumOscillator(){ MomentumPeriod = momentumPeriod, SignalPeriod = signalPeriod, SmoothMomentum = smoothMomentum, SmoothingPeriod = smoothingPeriod }, input, ref cacheAnchoredMomentumOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AnchoredMomentumOscillator AnchoredMomentumOscillator(int momentumPeriod, int signalPeriod, bool smoothMomentum, int smoothingPeriod)
		{
			return indicator.AnchoredMomentumOscillator(Input, momentumPeriod, signalPeriod, smoothMomentum, smoothingPeriod);
		}

		public Indicators.AnchoredMomentumOscillator AnchoredMomentumOscillator(ISeries<double> input , int momentumPeriod, int signalPeriod, bool smoothMomentum, int smoothingPeriod)
		{
			return indicator.AnchoredMomentumOscillator(input, momentumPeriod, signalPeriod, smoothMomentum, smoothingPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AnchoredMomentumOscillator AnchoredMomentumOscillator(int momentumPeriod, int signalPeriod, bool smoothMomentum, int smoothingPeriod)
		{
			return indicator.AnchoredMomentumOscillator(Input, momentumPeriod, signalPeriod, smoothMomentum, smoothingPeriod);
		}

		public Indicators.AnchoredMomentumOscillator AnchoredMomentumOscillator(ISeries<double> input , int momentumPeriod, int signalPeriod, bool smoothMomentum, int smoothingPeriod)
		{
			return indicator.AnchoredMomentumOscillator(input, momentumPeriod, signalPeriod, smoothMomentum, smoothingPeriod);
		}
	}
}

#endregion
