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
	public class BollingerNadarya : Indicator
	{
		private SMA		sma;
		private StdDev	stdDev;
		
		
		private Series<double> upperSeries;        
        private Series<double> lowerSeries;
       
		
		
		 private Series<double> smoothed_bolu_1;
        private Series<double> smoothed_bold_1;
		private Series<int> cntUp;
		private Series<int> cntDn;
      
		  private int signalLookback;
		
		
		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		private Brush UpColor;
		private Brush DownColor;
		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBollinger;
				Name						= "BollingerNadarya";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Calculate									= Calculate.OnBarClose;
				
	
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
				AddPlot(Brushes.Transparent, 	"CrossDetect");
				
				
		
			}
			else if (State == State.Configure)
			{
				upperSeries = new Series<double>(this);
                lowerSeries = new Series<double>(this);
				
				cntUp = new Series<int>(this);
                cntDn = new Series<int>(this);
               
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
			
			
			CrossDetect[0] 	= 0;	
			var BOLU_FIRST = Bollinger(Typical,  ShortStdev, ShortPeriod).Upper;
			var BOLD_FIRST = Bollinger(Typical,  ShortStdev, ShortPeriod).Lower;
          
			
			double smoothed_bolu_1 = Nadaraya(BOLU_FIRST, SmoothingFactor, 500);
            double smoothed_bold_1 = Nadaraya(BOLD_FIRST, SmoothingFactor, 500);
			
//			 cntUp[0] = 0;
//			cntDn[0]  = 0;
			
           // double upper = smoothed_bolu_1[0];
            //double lower = smoothed_bold_1[0];
			
//			//Print((Bollinger (Close, ShortStdev, ShortPeriod).Upper)[0]);
//			Print(upper);

//            // Update series values
            upperSeries[0] = smoothed_bolu_1;   // Index 0
            lowerSeries[0] = smoothed_bold_1;   // Index 2
			

            // Set plot values
            Upper[0] = upperSeries[0];  // Index 0
            Lower[0] = lowerSeries[0];  // Index 2
			
			
			var pivotHigh = High[0] < MAX(Typical, 3)[0];
            var pivotLow = Low[0] > MIN(Typical, 3)[0];
			
			bool upperBandTest =  false;
			bool lowerBandTest =  false;
			
			//band_test_upper_source  >= smoothed_bolu_1[offset]
			//High[0]> smoothed_bolu_1[0];
			
			//band_test_upper_source[1] <= smoothed_bolu_1[offset] or na(band_test_upper_source[1]))
			//High[1]> smoothed_bolu_1[0] || High[1] is null
			
			//upper_band_test = band_test_upper_source  >= smoothed_bolu_1[offset] and (band_test_upper_source[1] <= smoothed_bolu_1[offset] or na(band_test_upper_source[1])) and not repaint ? band_test_upper_source + spacing * 1.01  : na
            //lower_band_test = band_test_lower_source  <= smoothed_bold_1[offset] and (band_test_lower_source[1] >= smoothed_bold_1[offset] or na(band_test_upper_source[1])) and not repaint ? band_test_lower_source - spacing * 1.01  : na
			
			upperBandTest = High[0]>= smoothed_bolu_1 && (High[1] <= smoothed_bolu_1 || High[1] == null) ;
			lowerBandTest = Low[0]  <= smoothed_bold_1 && (Low[1] >= smoothed_bold_1 || Low[1] == null);
			
			
				if (upperBandTest && CurrentBar != savedUBar)
				{
					savedUBar = CurrentBar;  
				
					CrossDetect[0] = 1;
					
				}

				if (lowerBandTest && CurrentBar != savedDBar)
				{
					savedDBar =  CurrentBar;  
					
					CrossDetect[0] = -1;
				}

				
           
//			if (cntUp[0] == 2 )
//			{
//				Print ("cntUp[0]---------" + cntUp[0] + "-----Time-----" + ToTime(Time[0]));
//				CrossDetect[0] = 1;
				
//			}
//			else if (cntDn[0] == 2 )
//			{
//				Print ("cntDn[0]---------" + cntDn[0] + "-----Time-----" + ToTime(Time[0]));
//				CrossDetect[0] = -1;
//				savedDBar = CurrentBar;  	
//			}
			
			
			if (CrossDetect[0] == -1 )
			{
				//Print ("cntUp[0]---------" + cntUp[0] + "-----Time-----" + ToTime(Time[0]));
				Draw.ArrowUp (this, "UpArrow"+CurrentBar, true, 0,  Low[0] - 3 * TickSize , Brushes.Green);
			}
			
			if (CrossDetect[0] == 1 )
			{
				Draw.ArrowDown (this, "DwnArrow"+CurrentBar, true, 0, High[0] + 3 * TickSize, Brushes.Red);
			}			
			 
				

            // Signals
            

            // Plot Signals
            
//                Draw.ArrowUp(this, "UpperSignal", upperBandTest, Brushes.Red);
//                Draw.ArrowDown(this, "LowerSignal", lowerBandTest, Brushes.Green);
            
			

			
		}
		
 
		private double Nadaraya(ISeries<double> src, double h, int n)
        {
            var bar = CurrentBar >= 0 ? CurrentBar : -CurrentBar;
            var den = 2 * (h * h);
            var gk_sum = 0.0;
            for (var i = 0; i <= n; i++)
                gk_sum += Math.Pow(Math.E, -((i * i) / den));

            var smoothed = 0.0;
            for (var j = 0; j <= n; j++)
                smoothed += src[j] * Math.Pow(Math.E, -((j * j) / den)) / gk_sum;

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
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CrossDetect
		{
			get { return Values[2]; }
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
		private BollingerNadarya[] cacheBollingerNadarya;
		public BollingerNadarya BollingerNadarya(double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return BollingerNadarya(Input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}

		public BollingerNadarya BollingerNadarya(ISeries<double> input, double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			if (cacheBollingerNadarya != null)
				for (int idx = 0; idx < cacheBollingerNadarya.Length; idx++)
					if (cacheBollingerNadarya[idx] != null && cacheBollingerNadarya[idx].SmoothingFactor == smoothingFactor && cacheBollingerNadarya[idx].ShortPeriod == shortPeriod && cacheBollingerNadarya[idx].ShortStdev == shortStdev && cacheBollingerNadarya[idx].MediumPeriod == mediumPeriod && cacheBollingerNadarya[idx].MediumStdev == mediumStdev && cacheBollingerNadarya[idx].LongPeriod == longPeriod && cacheBollingerNadarya[idx].LongStdev == longStdev && cacheBollingerNadarya[idx].SignalLookback == signalLookback && cacheBollingerNadarya[idx].PlotsLines == plotsLines && cacheBollingerNadarya[idx].EqualsInput(input))
						return cacheBollingerNadarya[idx];
			return CacheIndicator<BollingerNadarya>(new BollingerNadarya(){ SmoothingFactor = smoothingFactor, ShortPeriod = shortPeriod, ShortStdev = shortStdev, MediumPeriod = mediumPeriod, MediumStdev = mediumStdev, LongPeriod = longPeriod, LongStdev = longStdev, SignalLookback = signalLookback, PlotsLines = plotsLines }, input, ref cacheBollingerNadarya);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerNadarya BollingerNadarya(double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadarya(Input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}

		public Indicators.BollingerNadarya BollingerNadarya(ISeries<double> input , double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadarya(input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerNadarya BollingerNadarya(double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadarya(Input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}

		public Indicators.BollingerNadarya BollingerNadarya(ISeries<double> input , double smoothingFactor, int shortPeriod, double shortStdev, int mediumPeriod, double mediumStdev, int longPeriod, double longStdev, int signalLookback, bool plotsLines)
		{
			return indicator.BollingerNadarya(input, smoothingFactor, shortPeriod, shortStdev, mediumPeriod, mediumStdev, longPeriod, longStdev, signalLookback, plotsLines);
		}
	}
}

#endregion
