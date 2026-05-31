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
	public class RSIOMA : Indicator
	{
		private Series<double> avgUp;
		private Series<double> avgDown;
		private Series<double> down;
		private Series<double> up;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"The RSIOMA is a price-following oscillator that ranges between 0 and 100. this indicator takes two moving averages, calculates their RSI (Relative Strength Index) and then also adds a moving average of the calculated RSI. These two lines now can accurately signal the trend changes.";
				Name										= "RSIOMA";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Left;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period					= 14;
				EmaPeriod				= 21;
				Smooth					= 14;
				AddPlot(Brushes.Transparent, "RsiOMAPlot");
				AddPlot(new Stroke(Brushes.RoyalBlue, 3),PlotStyle.Line, "Avg");
				AddPlot(new Stroke(Brushes.Magenta, 1),PlotStyle.Line, "EMAORSI");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Histogram");
				AddLine(new Stroke(Brushes.Green,DashStyleHelper.Dash, 1f), 20, "Lower");
				AddLine(new Stroke(Brushes.Red,DashStyleHelper.Dash, 1f), 80, "Upper");
				AddLine(new Stroke(Brushes.Orange,DashStyleHelper.Dash, 1f), 50, "Middle");
				AddLine(new Stroke(Brushes.White, DashStyleHelper.Dash, 1f), 0, "Zero");

			}
			else if (State == State.Configure)
			{
//				Plots[0].Brush = Brushes.DarkViolet;
//				Plots[0].DashStyleHelper = DashStyleHelper.DashDotDot;
				
//				Plots[1].Brush = Brushes.YellowGreen;
//				Plots[1].DashStyleHelper = DashStyleHelper.DashDotDot;
				
//				Plots[2].Brush = Brushes.DarkGreen;
//				Plots[2].DashStyleHelper = DashStyleHelper.DashDotDot;
				
			}
			else if (State == State.DataLoaded)
			{				
				avgUp = new Series<double>(this);
				avgDown = new Series<double>(this);
				down = new Series<double>(this);
				up = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
			{
				down[0]= (0);
				up[0]= (0);

                if (Period < 3)
                    Avg[0]= (50);
				return;
			}

		    double cma = EMA(Input, Period)[0];
            double pma = EMA(Input, Period)[1];

            down[0]= (Math.Max(pma - cma, 0));
            up[0]= (Math.Max(cma - pma, 0));

			if ((CurrentBar + 1) < Period) 
			{
				if ((CurrentBar + 1) == (Period - 1))
					Avg[0]= (50);
				return;
			}

			if ((CurrentBar + 1) == Period) 
			{
				// First averages 
				avgDown[0]= (EMA(down, Period)[0]);
				avgUp[0]= (EMA(up, Period)[0]);
			}  
			else 
			{
				// Rest of averages are smoothed
				avgDown[0]= ((avgDown[1] * (Period - 1) + down[0]) / Period);
				avgUp[0]= ((avgUp[1] * (Period - 1) + up[0]) / Period);
			}

			//double rsi	  = avgDown[0] == 0 ? 100 : 100 - 100 / (1 + avgUp[0] / avgDown[0]);
			RSI rsi = RSI(Period, Smooth);
			double rsiAvg = (2.0 / (1 + Smooth)) * rsi[0] + (1 - (2.0 / (1 + Smooth))) * Avg[1];

			Avg[0]= (rsiAvg);
			Value[0]= (rsi[0]);
			
			
			EMAORSI[0]= ( EMA(Avg,EmaPeriod)[0]);

		    double val = Avg[0] - 50;

			Histogram[0]= (-10);
			if (val < 0 )
				PlotBrushes[3][0] = Brushes.Red;
   			 else
               PlotBrushes[3][0] = Brushes.Green;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="Numbers of bars used for calculations", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EmaPeriod", Description="EMA of RSI Period", Order=2, GroupName="Parameters")]
		public int EmaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smooth", Description="Number of bars for smoothing", Order=3, GroupName="Parameters")]
		public int Smooth
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RsiOMAPlot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMAORSI
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Histogram
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
		private RSIOMA[] cacheRSIOMA;
		public RSIOMA RSIOMA(int period, int emaPeriod, int smooth)
		{
			return RSIOMA(Input, period, emaPeriod, smooth);
		}

		public RSIOMA RSIOMA(ISeries<double> input, int period, int emaPeriod, int smooth)
		{
			if (cacheRSIOMA != null)
				for (int idx = 0; idx < cacheRSIOMA.Length; idx++)
					if (cacheRSIOMA[idx] != null && cacheRSIOMA[idx].Period == period && cacheRSIOMA[idx].EmaPeriod == emaPeriod && cacheRSIOMA[idx].Smooth == smooth && cacheRSIOMA[idx].EqualsInput(input))
						return cacheRSIOMA[idx];
			return CacheIndicator<RSIOMA>(new RSIOMA(){ Period = period, EmaPeriod = emaPeriod, Smooth = smooth }, input, ref cacheRSIOMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSIOMA RSIOMA(int period, int emaPeriod, int smooth)
		{
			return indicator.RSIOMA(Input, period, emaPeriod, smooth);
		}

		public Indicators.RSIOMA RSIOMA(ISeries<double> input , int period, int emaPeriod, int smooth)
		{
			return indicator.RSIOMA(input, period, emaPeriod, smooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSIOMA RSIOMA(int period, int emaPeriod, int smooth)
		{
			return indicator.RSIOMA(Input, period, emaPeriod, smooth);
		}

		public Indicators.RSIOMA RSIOMA(ISeries<double> input , int period, int emaPeriod, int smooth)
		{
			return indicator.RSIOMA(input, period, emaPeriod, smooth);
		}
	}
}

#endregion
