// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Bollinger Bands are plotted at standard deviation levels above and below a moving average. 
	/// Since standard deviation is a measure of volatility, the bands are self-adjusting: 
	/// widening during volatile markets and contracting during calmer periods.
	/// </summary>
	public class BollingerNadaryaSmoothed : Indicator
	{
		private SMA		sma;
		private StdDev	stdDev;
		
		private double b;
        private double c1;
        private double c2;
        private double c3;
        private double c4;
		
		private Series<double> upperSeries;        
        private Series<double> lowerSeries;
       
		
		
		 private Series<double> smoothed_bolu_1;
        private Series<double> smoothed_bold_1;
        private Series<double> smoothed_bolu_2;
        private Series<double> smoothed_bold_2;
        private Series<double> smoothed_bolu_3;
        private Series<double> smoothed_bold_3;
        private Series<double> smoothed_bolu_4;
        private Series<double> smoothed_bold_4;
		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBollinger;
				Name						= "BollingerNadaryaSmoothed";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				
	
				SmoothingFactor = 6.0;		
				ShortPeriod = 20;
				
				ShortStdev = 3.0;
				
		       MediumPeriod = 75;
				
		       MediumStdev = 4.0;
				
				LongPeriod = 100;
				
		        LongStdev = 4.25;
				
				ShowSignals= true;
				
				SignalLookback  = 3;
				
		        PlotsLines = true;
				
				ShowBand1 = true;
				
				ShowBand2 = true;
			
				
			
				
				AddPlot(new Stroke (Brushes.Red, 3), PlotStyle.Line, "Upper");
				AddPlot(new Stroke (Brushes.Green, 3), PlotStyle.Line, "Lower");
				
				
		
			}
			else if (State == State.Configure)
			{
				upperSeries = new Series<double>(this);
                lowerSeries = new Series<double>(this);
               
//				smoothed_bolu_1 = new Series<double>(this);
//				smoothed_bold_1 = new Series<double>(this);
//				smoothed_bolu_2 = new Series<double>(this);
//				smoothed_bold_2 = new Series<double>(this);
//				smoothed_bolu_3 = new Series<double>(this);
//				smoothed_bold_3 = new Series<double>(this);
//				smoothed_bolu_4 = new Series<double>(this);
//				smoothed_bold_4 = new Series<double>(this);
				
				
				
			
				
            }
        }
				
				
				
		
		protected override void OnBarUpdate()
		{
			if (CurrentBar < 501)
				return;		
			
			//
		
//			smoothed_bolu_1 = NadarayaWatsonEstimator(Bollinger (Close, ShortStdev, ShortPeriod).Upper,500, SmoothingFactor).Value;
//            smoothed_bold_1 = NadarayaWatsonEstimator(Bollinger(Close, ShortStdev, ShortPeriod).Lower,  500, SmoothingFactor).Value;

//            smoothed_bolu_2 = NadarayaWatsonEstimator(Bollinger(Close, ShortStdev, MediumPeriod).Upper, 500, SmoothingFactor).Value;
//            smoothed_bold_2 = NadarayaWatsonEstimator(Bollinger(Close, ShortStdev, MediumPeriod).Lower,  500, SmoothingFactor).Value;

//            smoothed_bolu_3 = NadarayaWatsonEstimator(Bollinger(Close, MediumStdev, LongPeriod).Upper, 500, SmoothingFactor).Value;
//            smoothed_bold_3 = NadarayaWatsonEstimator(Bollinger(Close, MediumStdev, LongPeriod).Lower, 500, SmoothingFactor).Value;

//            smoothed_bolu_4 = NadarayaWatsonEstimator(Bollinger(Close, LongStdev, LongPeriod).Upper, 500, SmoothingFactor).Value;
//            smoothed_bold_4 = NadarayaWatsonEstimator(Bollinger(Close, LongStdev, LongPeriod).Lower, 500, SmoothingFactor).Value;
			
//			smoothed_bolu_1 = NadarayaSmoothed(Bollinger (Close, ShortStdev, ShortPeriod).Upper,SmoothingFactor);
//            smoothed_bold_1 = NadarayaSmoothed(Bollinger(Close, ShortStdev, ShortPeriod).Lower,  SmoothingFactor);

//            smoothed_bolu_2 = NadarayaSmoothed(Bollinger(Close, ShortStdev, MediumPeriod).Upper, SmoothingFactor);
//            smoothed_bold_2 = NadarayaSmoothed(Bollinger(Close, ShortStdev, MediumPeriod).Lower,  SmoothingFactor);

//            smoothed_bolu_3 = NadarayaSmoothed(Bollinger(Close, MediumStdev, LongPeriod).Upper, SmoothingFactor);
//            smoothed_bold_3 = NadarayaSmoothed(Bollinger(Close, MediumStdev, LongPeriod).Lower, SmoothingFactor);

//            smoothed_bolu_4 = NadarayaSmoothed(Bollinger(Close, LongStdev, LongPeriod).Upper,  SmoothingFactor);
//            smoothed_bold_4 = NadarayaSmoothed(Bollinger(Close, LongStdev, LongPeriod).Lower,  SmoothingFactor);
			
			smoothed_bolu_1 = NadarayaSmoothed(Bollinger (Close, ShortStdev, ShortPeriod).Upper,SmoothingFactor);
            	smoothed_bold_1 = NadarayaSmoothed(Bollinger(Close, ShortStdev, ShortPeriod).Lower,  SmoothingFactor);
			
//			foreach(var val in smoothed_bolu_1)
//            {
//                upperSeries[0] = val;
//            }
			
//			foreach(var val in smoothed_bold_1)
//            {
//                lowerSeries[0] = val;
//            }

			
            double upper = smoothed_bolu_1[0];
            double lower = smoothed_bold_1[0];
			
//			//Print((Bollinger (Close, ShortStdev, ShortPeriod).Upper)[0]);
//			Print(upper);

//            // Update series values
            upperSeries[0] = upper;   // Index 0
            lowerSeries[0] = lower;   // Index 2
			

            // Set plot values
            Upper[0] = upperSeries[0];  // Index 0
            Lower[0] = lowerSeries[0];  // Index 2
			

			
		}
		
 private Series<double> NadarayaSmoothed(Series<double> src, double h)
        {
            int n = 500;
            double curBar = Math.Abs(CurrentBar);
           	double den = 2 * Math.Pow(h, 2);
			double[] gk_sum = new double[n];
			double p = 0.0; // Initialize p here
           // List<double> smoothed = new List<double>();
			
			Series<double> smoothed = new Series<double>(this, MaximumBarsLookBack.Infinite);
			double q = 0.0; // Initialize q here
			
			//Print(curBar + "---curbar---");
			
				
			for (int i = 0; i < n; i++)
			{
				
//				if (i == 0)
//				{
					
//			        p += Math.Exp(-Math.Pow(i, 2) / den);
					
					
//			        gk_sum[i] = p;
//					//Print( gk_sum[i] + "--cusssr");
					
					
//				}
//				else
//				{
//					//Print(curBar + "--cusssr");
//				   gk_sum[i] = gk_sum[i - 1];	
//					//Print(i + "--cur1");
//					//Print( gk_sum[i] + "--ssss");
//				}
				
				  gk_sum[i] = (i == 0) ?
                    (gk_sum[i] + Math.Exp(-Math.Pow(i, 2) / den)) :
                    (gk_sum[1] + Math.Exp(-Math.Pow(i, 2) / den));
				
				
				
//				q += src[i] * Math.Exp(-Math.Pow(i, 2) / den) / gk_sum[i];
//				//Print("test1");
//				smoothed[i] = q;
				
				 smoothed[i] = (smoothed[i] + src[i] * Math.Exp(-Math.Pow(i, 2) / den) / gk_sum[i]);
			}
		

            return smoothed;
        }

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[1]; }
		}
		
		
		
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[0]; }
		}
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SmoothingFactor", GroupName = "NinjaScriptParameters", Order = 20)]
		public double SmoothingFactor { get; set; }
		
		[Range(5, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShortPeriod", GroupName = "NinjaScriptParameters", Order = 21)]
        public int ShortPeriod { get; set; } 
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShortStdev", GroupName = "NinjaScriptParameters", Order = 22)]
        public double ShortStdev { get; set; }
		
        [Range(10, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MediumPeriod", GroupName = "NinjaScriptParameters", Order = 23)]
		public int MediumPeriod { get; set; } 
		
        [Range(0.0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MediumStdev", GroupName = "NinjaScriptParameters", Order = 24)]
		public double MediumStdev { get; set; } 
		
        [Range(50, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "LongPeriod", GroupName = "NinjaScriptParameters", Order = 25)]
		public int LongPeriod { get; set; }
		
        [Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "LongStdev", GroupName = "NinjaScriptParameters", Order = 26)]
		public double LongStdev { get; set; } 
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowSignals", GroupName = "NinjaScriptParameters", Order = 27)]
		public bool ShowSignals { get; set; } 
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SignalLookback", GroupName = "NinjaScriptParameters", Order = 29)]
        public int SignalLookback { get; set; }
		
        [Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PlotsLines", GroupName = "NinjaScriptParameters", Order = 30)]
		public bool PlotsLines { get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowBand1", GroupName = "NinjaScriptParameters", Order = 31)]
		public bool ShowBand1 { get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowBand2", GroupName = "NinjaScriptParameters", Order = 32)]
		public bool ShowBand2 { get; set; }
	

	
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerNadaryaSmoothed[] cacheBollingerNadaryaSmoothed;
		public BollingerNadaryaSmoothed BollingerNadaryaSmoothed(double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return BollingerNadaryaSmoothed(Input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}

		public BollingerNadaryaSmoothed BollingerNadaryaSmoothed(ISeries<double> input, double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			if (cacheBollingerNadaryaSmoothed != null)
				for (int idx = 0; idx < cacheBollingerNadaryaSmoothed.Length; idx++)
					if (cacheBollingerNadaryaSmoothed[idx] != null && cacheBollingerNadaryaSmoothed[idx].SmoothingFactor == smoothingFactor && cacheBollingerNadaryaSmoothed[idx].ShortPeriod == shortPeriod && cacheBollingerNadaryaSmoothed[idx].ShortStdev == shortStdev && cacheBollingerNadaryaSmoothed[idx].MediumPeriod == mediumPeriod && cacheBollingerNadaryaSmoothed[idx].MediumStdev == mediumStdev && cacheBollingerNadaryaSmoothed[idx].LongPeriod == longPeriod && cacheBollingerNadaryaSmoothed[idx].LongStdev == longStdev && cacheBollingerNadaryaSmoothed[idx].SignalLookback == signalLookback && cacheBollingerNadaryaSmoothed[idx].PlotsLines == plotsLines && cacheBollingerNadaryaSmoothed[idx].EqualsInput(input))
						return cacheBollingerNadaryaSmoothed[idx];
			return CacheIndicator<BollingerNadaryaSmoothed>(new BollingerNadaryaSmoothed(){ SmoothingFactor = smoothingFactor, ShortPeriod = shortPeriod, ShortStdev = shortStdev, MediumPeriod = mediumPeriod, MediumStdev = mediumStdev, LongPeriod = longPeriod, LongStdev = longStdev, SignalLookback = signalLookback, PlotsLines = plotsLines }, input, ref cacheBollingerNadaryaSmoothed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerNadaryaSmoothed BollingerNadaryaSmoothed(double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadaryaSmoothed(Input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}

		public Indicators.BollingerNadaryaSmoothed BollingerNadaryaSmoothed(ISeries<double> input , double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadaryaSmoothed(input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerNadaryaSmoothed BollingerNadaryaSmoothed(double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadaryaSmoothed(Input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}

		public Indicators.BollingerNadaryaSmoothed BollingerNadaryaSmoothed(ISeries<double> input , double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadaryaSmoothed(input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}
	}
}

#endregion
