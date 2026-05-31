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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class DamianiVolatmeter : Indicator
    {
        private int visAtr = 13; // Default setting for Viscosity ATR
        private int visStd = 20; // Default setting for Viscosity StdDev.
        private int sedAtr = 40; // Default setting for Sedimentation ATR
        private int sedStd = 100; // Default setting for Sedimentation StdDev.
        private double thresholdLevel = 1.4; // Default setting for Threshold Level
        private double underhline = -0.35; // Default setting for Charging Level
        private bool lagSupressor = true; // Default setting for Lag Suppressor
        private int dvsigfast = 12; // Default setting for Fast Length
        private int dvsigslow = 26; // Default setting for Slow Length
        private int dvsigsignal = 9; // Default setting for Signal Length
        private bool showChargeExtreme = true; // Default setting for Show Charging Extremes
        private bool colorbars = true; // Default setting for Color Bars
        private bool showsignals = true; // Default setting for Show Signals

        private Series<double> DVval;
        private Series<double> DVema26;
        private Series<double> DVsig;
        private Series<double> diffSeries; // New series to hold differences
		
		private Series<double> zeroLineSeries; // Series for Zero Line
        private Series<double> chargingLineSeries; // Series for Charging Line
		
		private bool isChop = false;
		private bool isVolatile = false;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Damiani Volatmeter";
                Name = "DamianiVolatmeter";
                Calculate = Calculate.OnBarClose;
                IsOverlay = false;
				DrawOnPricePanel = false;
                AddPlot(Brushes.White, "Zero Line");
                AddPlot(Brushes.White, "Charging Line");
                AddPlot(new Stroke(Brushes.Transparent, 3), PlotStyle.Bar, "DVMeter Histo");
                AddPlot(Brushes.White, "Signal Line");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Line, "DVMeter Line");
				
            }
            else if (State == State.Configure)
            {
                // No need to add data series here unless necessary
				
            }
            else if (State == State.DataLoaded)
            {
                DVval = new Series<double>(this);
                DVema26 = new Series<double>(this);
                DVsig = new Series<double>(this);
                diffSeries = new Series<double>(this); // Initialize the new series
				 zeroLineSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
                chargingLineSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress == 0 && CurrentBar > sedStd)
            {
                // Calculate DVval
                double lag_s_K = 0.5;
                double s1 = DVval[1];
                double s3 = DVval[3];

                double vol = lagSupressor ? ATR(visAtr)[0] / ATR(sedAtr)[0] + lag_s_K * (s1 - s3) : ATR(visAtr)[0] / ATR(sedAtr)[0];
                double anti_thres = StdDev(Close, visStd)[0] / StdDev(Close, sedStd)[0];
                double t = thresholdLevel - anti_thres;

                DVval[0] = -t + vol;

                // Calculate DVema26
                DVema26[0] = HMA(DVval, dvsigslow)[0];

                // Calculate the difference series
                diffSeries[0] = LinReg(DVval, dvsigfast)[0] - DVema26[0];

                // Calculate DVsig using SMA on the difference series
                DVsig[0] = LinReg(diffSeries, dvsigsignal)[0] + DVema26[0];

                // Assign values to plots
                Values[0][0] = 0; // Zero Line
                Values[1][0] = underhline; // Charging Line
                Values[2][0] = DVval[0]; // DVMeter Line
                Values[3][0] = DVsig[0]; // Signal Line
				Values[4][0] = DVval[0]; // Signal Line

//                // Color bars based on conditions
//                if (colorbars)
//                {
//                    if (DVval[0] > 0 && DVval[0] > DVsig[0])
//                    {
//                        BarBrush = Brushes.Lime;
//                    }
//                    else if (DVval[0] > 0)
//                    {
//                        BarBrush = Brushes.LightGreen;
//                    }
//                    else if (DVval[0] < 0 && DVval[0] < DVsig[0])
//                    {
//                        BarBrush = Brushes.Red;
//                    }
//                    else if (DVval[0] < 0)
//                    {
//                        BarBrush = Brushes.Pink;
//                    }
//                }

                // Plot shapes for signals
				
				
                if (showsignals)
                {
//                    if (DVval[0] > 0 && this.isVolatile == false)
//                    {
//                        //Draw.ArrowUp(this, "RisingVolatility" + CurrentBar, true, 0, -1.25, Brushes.Yellow);
//						if (DVval[0] > DVval[1]  )
//						{
//							Draw.Text(this, "RisingVolatility" + CurrentBar, "🔹", 0, -0.80, Brushes.Yellow);
//							this.isVolatile = true;
//							this.isChop = false;
//						}
						
//                    }
//                    else if (DVval[0] < 0 && this.isChop == false)
//                    {
//						if (DVval[0] < DVval[1]  )
//						{
//                        Draw.Text(this, "VolatilityDump" + CurrentBar , "🔸", 0,  0.80, Brushes.Fuchsia);
//						this.isVolatile = false;
//						this.isChop = true;
//						}
//                    }
					
					if(CrossAbove(DVval,DVsig,1) && this.isVolatile == false)
					{
						Draw.Text(this, "RisingVolatility" + CurrentBar, "🔹", 0, -0.80, Brushes.Yellow);
							this.isVolatile = true;
							this.isChop = false;
					}
					if(CrossBelow(DVval,DVsig,1) && this.isChop == false)
					{
						 Draw.Text(this, "VolatilityDump" + CurrentBar , "🔸", 0,  0.80, Brushes.Fuchsia);
						this.isVolatile = false;
						this.isChop = true;
					}
                }

                // Plot charging extremes
                if (showChargeExtreme && DVval[0] < underhline && DVval[0] < DVsig[0])
                {
                    // Use a different shape since Draw.Circle is not available
                    Draw.ArrowDown(this, "ChargingExtreme", true, 0, Low[0], Brushes.Yellow);
                }
				
				  // Draw region between zero line and DVMeter Line
                if (DVval[0] > 0)
                {
                      PlotBrushes[2][0] = Brushes.DarkGreen;
					PlotBrushes[4][0] = Brushes.DarkGreen;
					  if (DVval[0] < DVsig[0])
					  {
						  PlotBrushes[2][0] = Brushes.LightGreen;
						  PlotBrushes[4][0] = Brushes.DarkGreen;
					  }
                }
                else
                {
                     PlotBrushes[2][0] = Brushes.Red;
					 PlotBrushes[4][0] = Brushes.Red;
						 if (DVval[0] > DVsig[0])
						 {
						  PlotBrushes[2][0] = Brushes.Salmon;
							 PlotBrushes[4][0] = Brushes.Salmon;
						 }
                }
            }
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Viscosity ATR", Order = 0, GroupName = "Parameters")]
        public int VisAtr
        {
            get { return visAtr; }
            set { visAtr = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Viscosity StdDev.", Order = 1, GroupName = "Parameters")]
        public int VisStd
        {
            get { return visStd; }
            set { visStd = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Sedimentation ATR", Order = 2, GroupName = "Parameters")]
        public int SedAtr
        {
            get { return sedAtr; }
            set { sedAtr = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Sedimentation StdDev.", Order = 3, GroupName = "Parameters")]
        public int SedStd
        {
            get { return sedStd; }
            set { sedStd = value; }
        }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name = "Threshold Level", Order = 4, GroupName = "Parameters")]
        public double ThresholdLevel
        {
            get { return thresholdLevel; }
            set { thresholdLevel = value; }
        }

        [NinjaScriptProperty]
        [Range(-10, 10)]
        [Display(Name = "Charging Level", Order = 5, GroupName = "Parameters")]
        public double Underhline
        {
            get { return underhline; }
            set { underhline = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Lag Suppressor", Order = 6, GroupName = "Parameters")]
        public bool LagSupressor
        {
            get { return lagSupressor; }
            set { lagSupressor = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Fast Length", Order = 7, GroupName = "Signal")]
        public int Dvsigfast
        {
            get { return dvsigfast; }
            set { dvsigfast = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Slow Length", Order = 8, GroupName = "Signal")]
        public int Dvsigslow
        {
            get { return dvsigslow; }
            set { dvsigslow = value; }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Signal Length", Order = 9, GroupName = "Signal")]
        public int Dvsigsignal
        {
            get { return dvsigsignal; }
            set { dvsigsignal = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Show Charging Extremes", Order = 10, GroupName = "UI Options")]
        public bool ShowChargeExtreme
        {
            get { return showChargeExtreme; }
            set { showChargeExtreme = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Color Bars", Order = 11, GroupName = "UI Options")]
        public bool Colorbars
        {
            get { return colorbars; }
            set { colorbars = value; }
        }

        [NinjaScriptProperty]
        [Display(Name = "Show Signals", Order = 12, GroupName = "UI Options")]
        public bool Showsignals
        {
            get { return showsignals; }
            set { showsignals = value; }
        }

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DamianiVolatmeter[] cacheDamianiVolatmeter;
		public DamianiVolatmeter DamianiVolatmeter(int visAtr, int visStd, int sedAtr, int sedStd, double thresholdLevel, double underhline, bool lagSupressor, int dvsigfast, int dvsigslow, int dvsigsignal, bool showChargeExtreme, bool colorbars, bool showsignals)
		{
			return DamianiVolatmeter(Input, visAtr, visStd, sedAtr, sedStd, thresholdLevel, underhline, lagSupressor, dvsigfast, dvsigslow, dvsigsignal, showChargeExtreme, colorbars, showsignals);
		}

		public DamianiVolatmeter DamianiVolatmeter(ISeries<double> input, int visAtr, int visStd, int sedAtr, int sedStd, double thresholdLevel, double underhline, bool lagSupressor, int dvsigfast, int dvsigslow, int dvsigsignal, bool showChargeExtreme, bool colorbars, bool showsignals)
		{
			if (cacheDamianiVolatmeter != null)
				for (int idx = 0; idx < cacheDamianiVolatmeter.Length; idx++)
					if (cacheDamianiVolatmeter[idx] != null && cacheDamianiVolatmeter[idx].VisAtr == visAtr && cacheDamianiVolatmeter[idx].VisStd == visStd && cacheDamianiVolatmeter[idx].SedAtr == sedAtr && cacheDamianiVolatmeter[idx].SedStd == sedStd && cacheDamianiVolatmeter[idx].ThresholdLevel == thresholdLevel && cacheDamianiVolatmeter[idx].Underhline == underhline && cacheDamianiVolatmeter[idx].LagSupressor == lagSupressor && cacheDamianiVolatmeter[idx].Dvsigfast == dvsigfast && cacheDamianiVolatmeter[idx].Dvsigslow == dvsigslow && cacheDamianiVolatmeter[idx].Dvsigsignal == dvsigsignal && cacheDamianiVolatmeter[idx].ShowChargeExtreme == showChargeExtreme && cacheDamianiVolatmeter[idx].Colorbars == colorbars && cacheDamianiVolatmeter[idx].Showsignals == showsignals && cacheDamianiVolatmeter[idx].EqualsInput(input))
						return cacheDamianiVolatmeter[idx];
			return CacheIndicator<DamianiVolatmeter>(new DamianiVolatmeter(){ VisAtr = visAtr, VisStd = visStd, SedAtr = sedAtr, SedStd = sedStd, ThresholdLevel = thresholdLevel, Underhline = underhline, LagSupressor = lagSupressor, Dvsigfast = dvsigfast, Dvsigslow = dvsigslow, Dvsigsignal = dvsigsignal, ShowChargeExtreme = showChargeExtreme, Colorbars = colorbars, Showsignals = showsignals }, input, ref cacheDamianiVolatmeter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DamianiVolatmeter DamianiVolatmeter(int visAtr, int visStd, int sedAtr, int sedStd, double thresholdLevel, double underhline, bool lagSupressor, int dvsigfast, int dvsigslow, int dvsigsignal, bool showChargeExtreme, bool colorbars, bool showsignals)
		{
			return indicator.DamianiVolatmeter(Input, visAtr, visStd, sedAtr, sedStd, thresholdLevel, underhline, lagSupressor, dvsigfast, dvsigslow, dvsigsignal, showChargeExtreme, colorbars, showsignals);
		}

		public Indicators.DamianiVolatmeter DamianiVolatmeter(ISeries<double> input , int visAtr, int visStd, int sedAtr, int sedStd, double thresholdLevel, double underhline, bool lagSupressor, int dvsigfast, int dvsigslow, int dvsigsignal, bool showChargeExtreme, bool colorbars, bool showsignals)
		{
			return indicator.DamianiVolatmeter(input, visAtr, visStd, sedAtr, sedStd, thresholdLevel, underhline, lagSupressor, dvsigfast, dvsigslow, dvsigsignal, showChargeExtreme, colorbars, showsignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DamianiVolatmeter DamianiVolatmeter(int visAtr, int visStd, int sedAtr, int sedStd, double thresholdLevel, double underhline, bool lagSupressor, int dvsigfast, int dvsigslow, int dvsigsignal, bool showChargeExtreme, bool colorbars, bool showsignals)
		{
			return indicator.DamianiVolatmeter(Input, visAtr, visStd, sedAtr, sedStd, thresholdLevel, underhline, lagSupressor, dvsigfast, dvsigslow, dvsigsignal, showChargeExtreme, colorbars, showsignals);
		}

		public Indicators.DamianiVolatmeter DamianiVolatmeter(ISeries<double> input , int visAtr, int visStd, int sedAtr, int sedStd, double thresholdLevel, double underhline, bool lagSupressor, int dvsigfast, int dvsigslow, int dvsigsignal, bool showChargeExtreme, bool colorbars, bool showsignals)
		{
			return indicator.DamianiVolatmeter(input, visAtr, visStd, sedAtr, sedStd, thresholdLevel, underhline, lagSupressor, dvsigfast, dvsigslow, dvsigsignal, showChargeExtreme, colorbars, showsignals);
		}
	}
}

#endregion
