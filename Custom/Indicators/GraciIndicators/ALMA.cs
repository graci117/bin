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

namespace NinjaTrader.NinjaScript.Indicators.GraciIndicators
{
    public class ALMA : Indicator
    {

        
        private double[] aALMA;        

        protected override void OnStateChange()
        {



            if (State == State.SetDefaults)
            {
                Description                                    = @"Enter the description for your new custom Indicator here.";
                Name                                        = "ALMA";
                Calculate                                    = Calculate.OnBarClose;
                IsOverlay                                    = true;
                DisplayInDataBox                            = true;
                DrawOnPricePanel                            = false;
                DrawHorizontalGridLines                        = true;
                DrawVerticalGridLines                        = true;
                PaintPriceMarkers                            = true;
                ScaleJustification                            = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                //Disable this property if your indicator requires custom values that cumulate with each new market data event. 
                //See Help Guide for additional information.
                IsSuspendedWhileInactive                    = true;
                AddPlot(Brushes.Orange, "ALMA_Plot");
				
				WindowSize = 30; 
				Sigma = 6.0;
				Sample = 0.9;
            }
            else if (State == State.Configure)
            {
                aALMA = new double[WindowSize];
                ResetWindow();                
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < WindowSize)
                return;

            int pt = 0;

            double agr  = 0;
            double norm = 0;

            for (int i = 0; i < WindowSize; i++) 
            {
                if (i < WindowSize - pt) 
                {
                    agr += aALMA[i] * Close[WindowSize - pt - 1 - i];
                    norm += aALMA[i];
                }
            }

            // Normalize the result
            if (norm != 0) agr /= norm;

            // Set the approrpiate bar.
            //ALMA_Plot.Set(agr);    
            ALMA_Plot[0] = agr;
			
			
			if(IsRising(Value)) {PlotBrushes[0][0] = Brushes.Green;}
			else if(IsFalling(Value)) {PlotBrushes[0][0] = Brushes.Red;}
			else {PlotBrushes[0][0] = Brushes.Yellow;}
        }


        #region helper

        private void ResetWindow() 
        {


            double m = (int)Math.Floor(Sample * (double)(WindowSize - 1));


            double s = WindowSize;
            s /= Sigma;

            for (int i = 0; i < WindowSize; i++) 
            {
                aALMA[i] = Math.Exp(-((((double)i)-m)*(((double)i)-m))/(2*s*s));
            }
        }    

        #endregion

        #region Properties

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ALMA_Plot
        {
            get { return Values[0]; }
        }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="WindowSize", Order=1, GroupName="Parameters")]
		public int WindowSize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Sigma", Order=2, GroupName="Parameters")]
		public double Sigma
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Sample", Order=2, GroupName="Parameters")]
		public double Sample
		{ get; set; }

	


        #endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GraciIndicators.ALMA[] cacheALMA;
		public GraciIndicators.ALMA ALMA(int windowSize, double sigma, double sample)
		{
			return ALMA(Input, windowSize, sigma, sample);
		}

		public GraciIndicators.ALMA ALMA(ISeries<double> input, int windowSize, double sigma, double sample)
		{
			if (cacheALMA != null)
				for (int idx = 0; idx < cacheALMA.Length; idx++)
					if (cacheALMA[idx] != null && cacheALMA[idx].WindowSize == windowSize && cacheALMA[idx].Sigma == sigma && cacheALMA[idx].Sample == sample && cacheALMA[idx].EqualsInput(input))
						return cacheALMA[idx];
			return CacheIndicator<GraciIndicators.ALMA>(new GraciIndicators.ALMA(){ WindowSize = windowSize, Sigma = sigma, Sample = sample }, input, ref cacheALMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GraciIndicators.ALMA ALMA(int windowSize, double sigma, double sample)
		{
			return indicator.ALMA(Input, windowSize, sigma, sample);
		}

		public Indicators.GraciIndicators.ALMA ALMA(ISeries<double> input , int windowSize, double sigma, double sample)
		{
			return indicator.ALMA(input, windowSize, sigma, sample);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GraciIndicators.ALMA ALMA(int windowSize, double sigma, double sample)
		{
			return indicator.ALMA(Input, windowSize, sigma, sample);
		}

		public Indicators.GraciIndicators.ALMA ALMA(ISeries<double> input , int windowSize, double sigma, double sample)
		{
			return indicator.ALMA(input, windowSize, sigma, sample);
		}
	}
}

#endregion
