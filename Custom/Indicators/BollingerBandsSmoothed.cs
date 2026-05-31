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
	public class BollingerBandsSmoothed : Indicator
	{
		private EMA		EMA;
		private StdDev	stdDev;
		
		private double b;
        private double c1;
        private double c2;
        private double c3;
        private double c4;
		private double middle;
		private double middleDev;
		private Series<double> upperSeries;
        private Series<double> middleSeries;
        private Series<double> lowerSeries;
        private Series<double> devSeries;
        private Series<double> middleDevSeries;
		
		private Series<bool> bearIndication;
		private Series<bool> bullIndication;
		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBollinger;
				Name						= "BollingerBandsSmoothed";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				NumStdDev					= 2.5;
				Period						= 20;
	
				
			
				
			
				
				AddPlot(new Stroke (Brushes.Green, 2), PlotStyle.Line, "Upper");
				AddPlot(new Stroke (Brushes.Orange, 2), PlotStyle.Line, "Middle");
					AddPlot(new Stroke (Brushes.Green, 2), PlotStyle.Line, "Lower");
				
				
		
			}
			else if (State == State.Configure)
			{
				bearIndication			= new Series<bool>(this);
				bullIndication			= new Series<bool>(this);

				upperSeries = new Series<double>(this);
                middleSeries = new Series<double>(this);
                lowerSeries = new Series<double>(this);
                devSeries = new Series<double>(this);
                middleDevSeries = new Series<double>(this);
				b = 0.7;
		        c1 = -b * b * b;
		        c2 = 3 * b * b + 3 * b * b * b;
		        c3 = -6 * b * b - 3 * b - 3 * b * b * b;
		        c4 = 1 + 3 * b + b * b * b + 3 * b * b;
				
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 5)
				return;
			
			double middle = T3(Period, Close);
            double middleDev = T3(Period / 10, Close);
			
			middleDevSeries[0] = middleDev;
			
			Series<double> middleDevStdDev = StdDev(middleDevSeries, Period).Value;
			
			double dev = NumStdDev * middleDevStdDev[0];
            double upper = middle + dev;
            double lower = middle - dev;

            // Update series values
            upperSeries[0] = upper;   // Index 0
            middleSeries[0] = middle; // Index 1
            lowerSeries[0] = lower;   // Index 2

            // Set plot values
            Upper[0] = upperSeries[0];  // Index 0
            Middle[0] = middleSeries[0]; // Index 1
            Lower[0] = lowerSeries[0];  // Index 2
			
			if ((Low[1] < lowerSeries[1] || Low[0] < lowerSeries[0]) && (Close[0] > lowerSeries[0]) && Close[0] > Close[1])
			{
				BarBrushes[0]		= Brushes.Yellow;
				
				/* This crossover condition is considered bullish so we set the "bullIndication" Series<bool> object to true.
				We also set the "bearIndication" object to false so it does not take on a null value. */
				bullIndication[0]	= (false);
				bearIndication[0]	= (true);
			}
			
			if ((High[1] > upperSeries[1] || High[0] > upperSeries[0]) && (Close[0] < upperSeries[0]) && Close[0] < Close[1])
			{
				BarBrushes[0]		= Brushes.Magenta;
				
				/* This crossover condition is considered bullish so we set the "bullIndication" Series<bool> object to true.
				We also set the "bearIndication" object to false so it does not take on a null value. */
				bullIndication[0]	= (false);
				bearIndication[0]	= (true);
			}			
			// MACD Crossover: No cross
			else
			{
				/* Since no crosses occured we are not receiving any bullish or bearish signals so we
				set our Series<bool> objects both to false. */
				bullIndication[0] = (false);
				bearIndication[0] = (false);
			}

			

			
		}
		
		 private double T3(int len, ISeries<double> close)
        {
            return c1 * ((EMA(EMA(EMA(EMA(EMA(EMA(close, len), len), len), len), len), len)).Value)[0] +
                   c2 * ((EMA(EMA(EMA(EMA(EMA(close, len), len), len), len), len)).Value)[0] +
                   c3 * ((EMA(EMA(EMA(EMA(close, len), len), len), len)).Value)[0] +
                   c4 * ((EMA(EMA(EMA(close, len), len), len)).Value)[0];
        }

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[1]; }
		}

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDev", GroupName = "NinjaScriptParameters", Order = 0)]
		public double NumStdDev
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Period
		{ get; set; }
		
	

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[0]; }
		}
		
			// Creating public properties that access our internal Series<bool> allows external access to this indicator's Series<bool>
		[Browsable(false)]
		[XmlIgnore]
        public Series<bool> BearIndication
        {
            get { return bearIndication; }	// Allows our public BearIndication Series<bool> to access and expose our interal bearIndication Series<bool>
        }
		
		[Browsable(false)]
		[XmlIgnore]		
        public Series<bool>  BullIndication
        {
            get { return bullIndication; }	// Allows our public BullIndication Series<bool> to access and expose our interal bullIndication Series<bool>
        }


		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerBandsSmoothed[] cacheBollingerBandsSmoothed;
		public BollingerBandsSmoothed BollingerBandsSmoothed(double numStdDev, int period)
		{
			return BollingerBandsSmoothed(Input, numStdDev, period);
		}

		public BollingerBandsSmoothed BollingerBandsSmoothed(ISeries<double> input, double numStdDev, int period)
		{
			if (cacheBollingerBandsSmoothed != null)
				for (int idx = 0; idx < cacheBollingerBandsSmoothed.Length; idx++)
					if (cacheBollingerBandsSmoothed[idx] != null && cacheBollingerBandsSmoothed[idx].NumStdDev == numStdDev && cacheBollingerBandsSmoothed[idx].Period == period && cacheBollingerBandsSmoothed[idx].EqualsInput(input))
						return cacheBollingerBandsSmoothed[idx];
			return CacheIndicator<BollingerBandsSmoothed>(new BollingerBandsSmoothed(){ NumStdDev = numStdDev, Period = period }, input, ref cacheBollingerBandsSmoothed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerBandsSmoothed BollingerBandsSmoothed(double numStdDev, int period)
		{
			return indicator.BollingerBandsSmoothed(Input, numStdDev, period);
		}

		public Indicators.BollingerBandsSmoothed BollingerBandsSmoothed(ISeries<double> input , double numStdDev, int period)
		{
			return indicator.BollingerBandsSmoothed(input, numStdDev, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerBandsSmoothed BollingerBandsSmoothed(double numStdDev, int period)
		{
			return indicator.BollingerBandsSmoothed(Input, numStdDev, period);
		}

		public Indicators.BollingerBandsSmoothed BollingerBandsSmoothed(ISeries<double> input , double numStdDev, int period)
		{
			return indicator.BollingerBandsSmoothed(input, numStdDev, period);
		}
	}
}

#endregion
