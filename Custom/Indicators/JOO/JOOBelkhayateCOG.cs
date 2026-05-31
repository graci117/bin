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
namespace NinjaTrader.NinjaScript.Indicators.JOO
{
	public class JOOBelkhayateCOG : Indicator
	{
		
        #region Variables
        private double[,] ai = new double[10, 10];
        private double[] b = new double[10];
        private double[] x = new double[10];
        private double[] sx = new double[10];
        private double sum;
        private int ip;
        private int p;
        private int n;
        private int f;
        private double qq;
        private double mm;
        private double tt;
        private int ii;
        private int jj;
        private int kk;
        private int ll;
        private int nn;
        private double sq;
        private double sq2;
        private double sq3;
        private int i0 = 0;
        private int mi;
        #endregion
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "JOOBelkhayateCOG";
				Calculate									= Calculate.OnEachTick;
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
				

				
				AddPlot(new Stroke(Brushes.Gray, DashStyleHelper.Dash, 1), PlotStyle.Line, "PRC");								
	            AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "SQH");//Plot0
	            AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "SQL");//Plot1
	            AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "SQL2");//Plot2
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "SQH2");//Plot3
	            AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "SQL3");//Plot4
	            AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "SQH3");//Plot4
				
				
				
				degree = 3;
				period = 120;
				strdDev = 1.4;
				strdDev2 = 2.4;
				strdDev3 = 3.4;
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if( CurrentBars[0] < period){
				return;
			}
			
            //int i = index;
            ip = period;
            p = ip;
            sx[1] = p + 1;
            nn = degree + 1;
            //----------------------sx-------------------------------------------------------------------
            // 
 
            for (mi = 1; mi <= nn * 2 - 2; mi++)
            {
                sum = 0;
                for (n = i0; n <= i0 + p; n++)
                {
                    sum += Math.Pow(n, mi);
                }
                sx[mi + 1] = sum;
            }
            //----------------------syx-----------
 
            for (mi = 1; mi <= nn; mi++)
            {
                sum = 0.0;
                for (n = i0; n <= i0 + p; n++)
                {
                    if (mi == 1)
                        sum += Close[n];
                    else
                        sum += Close[n] * Math.Pow(n, mi - 1);
                }
                b[mi] = sum;
            }
            //===============Matrix=======================================================================================================
 
            for (jj = 1; jj <= nn; jj++)
            {
                for (ii = 1; ii <= nn; ii++)
                {
                    kk = ii + jj - 1;
                    ai[ii, jj] = sx[kk];
                }
            }
 
            //===============Gauss========================================================================================================
            for (kk = 1; kk <= nn - 1; kk++)
            {
                ll = 0;
                mm = 0;
                for (ii = kk; ii <= nn; ii++)
                {
                    if (Math.Abs(ai[ii, kk]) > mm)
                    {
                        mm = Math.Abs(ai[ii, kk]);
                        ll = ii;
                    }
                }
                if (ll == 0)
                    return;
                if (ll != kk)
                {
                    for (jj = 1; jj <= nn; jj++)
                    {
                        tt = ai[kk, jj];
                        ai[kk, jj] = ai[ll, jj];
                        ai[ll, jj] = tt;
                    }
                    tt = b[kk];
                    b[kk] = b[ll];
                    b[ll] = tt;
                }
                for (ii = kk + 1; ii <= nn; ii++)
                {
                    qq = ai[ii, kk] / ai[kk, kk];
                    for (jj = 1; jj <= nn; jj++)
                    {
                        if (jj == kk)
                            ai[ii, jj] = 0;
                        else
                            ai[ii, jj] = ai[ii, jj] - qq * ai[kk, jj];
                    }
                    b[ii] = b[ii] - qq * b[kk];
                }
            }
 
            x[nn] = b[nn] / ai[nn, nn];
            for (ii = nn - 1; ii >= 1; ii--)
            {
                tt = 0;
                for (jj = 1; jj <= nn - ii; jj++)
                {
                    tt = tt + ai[ii, ii + jj] * x[ii + jj];
                    x[ii] = (1 / ai[ii, ii]) * (b[ii] - tt);
                }
            }
            sq = 0.0;
            sq2 = 0.0;
            sq3 = 0.0;
            for (n = i0; n <= i0 + p; n++)
            {
                sum = 0;
                for (kk = 1; kk <= degree; kk++)
                {
                    sum += x[kk + 1] * Math.Pow(n, kk);
                }
 
                prc[n] = (x[1] + sum);
                sq += Math.Pow(Close[n] - prc[n], 2);
                sq2 = sq;
                sq3 = sq;
            }
 
            sq = Math.Sqrt(sq / (p + 1)) * strdDev;
            sq2 = Math.Sqrt(sq2 / (p + 1)) * strdDev2;
            sq3 = Math.Sqrt(sq3 / (p + 1)) * strdDev3;
            for (n = 0; n <= period; n++)
            {
 
                sqh[n] = prc[n] + sq;
                sqh2[n] = prc[n] + sq2;
                sqh3[n] = prc[n] + sq3;
                sql[n] = prc[n] - sq;
                sql2[n] = prc[n] - sq2;
                sql3[n] = prc[n] - sq3;
            }
 
            //prc[period] = double.NaN;
            //sqh[period] = double.NaN;
            //sqh2[period] = double.NaN;
            //sqh3[period] = double.NaN;
            //sql[period] = double.NaN;
            //sql2[period] = double.NaN;
            //sql3[period] = double.NaN;
        
			

		}
		
        #region Properties
		
        [Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> prc
        {
            get { return Values[0]; }
        }

		[Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> sqh
        {
            get { return Values[1]; }
        }
		
		[Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> sql
        {
            get { return Values[2]; }
        }
		
		[Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> sql2
        {
            get { return Values[3]; }
        }
		
		[Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> sqh2
        {
            get { return Values[4]; }
        }
		
		[Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> sql3
        {
            get { return Values[5]; }
        }
		
		[Browsable(false)]	
        [XmlIgnore()]		
        public Series<double> sqh3
        {
            get { return Values[6]; }
        }
	
		
		
		[NinjaScriptProperty]
		[Range(1, 4)]
        [Description("type of polynomial 1, 2, 3 or 4")]
        [Category("Parameters")]		
		[Display(Name="degree", Order=1, GroupName="Parameters")]
		public int degree
		{ get; set; }		

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
        [Description("Periods Back for Polynomial")]
        [Category("Parameters")]		
		[Display(Name="period", Order=2, GroupName="Parameters")]
		public int period
		{ get; set; }		

		[NinjaScriptProperty]
        [Description("strdDev")]
        [Category("Parameters")]		
		[Display(Name="strdDev", Order=3, GroupName="Parameters")]
		public double strdDev
		{ get; set; }		

		[NinjaScriptProperty]
        [Description("strdDev2")]
        [Category("Parameters")]		
		[Display(Name="strdDev2", Order=4, GroupName="Parameters")]
		public double strdDev2
		{ get; set; }		

		[NinjaScriptProperty]
        [Description("strdDev3")]
        [Category("Parameters")]		
		[Display(Name="strdDev3", Order=5, GroupName="Parameters")]
		public double strdDev3
		{ get; set; }		

		
		
		
		
		#endregion		
		
		
		
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private JOO.JOOBelkhayateCOG[] cacheJOOBelkhayateCOG;
		public JOO.JOOBelkhayateCOG JOOBelkhayateCOG(int degree, int period, double strdDev, double strdDev2, double strdDev3)
		{
			return JOOBelkhayateCOG(Input, degree, period, strdDev, strdDev2, strdDev3);
		}

		public JOO.JOOBelkhayateCOG JOOBelkhayateCOG(ISeries<double> input, int degree, int period, double strdDev, double strdDev2, double strdDev3)
		{
			if (cacheJOOBelkhayateCOG != null)
				for (int idx = 0; idx < cacheJOOBelkhayateCOG.Length; idx++)
					if (cacheJOOBelkhayateCOG[idx] != null && cacheJOOBelkhayateCOG[idx].degree == degree && cacheJOOBelkhayateCOG[idx].period == period && cacheJOOBelkhayateCOG[idx].strdDev == strdDev && cacheJOOBelkhayateCOG[idx].strdDev2 == strdDev2 && cacheJOOBelkhayateCOG[idx].strdDev3 == strdDev3 && cacheJOOBelkhayateCOG[idx].EqualsInput(input))
						return cacheJOOBelkhayateCOG[idx];
			return CacheIndicator<JOO.JOOBelkhayateCOG>(new JOO.JOOBelkhayateCOG(){ degree = degree, period = period, strdDev = strdDev, strdDev2 = strdDev2, strdDev3 = strdDev3 }, input, ref cacheJOOBelkhayateCOG);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.JOO.JOOBelkhayateCOG JOOBelkhayateCOG(int degree, int period, double strdDev, double strdDev2, double strdDev3)
		{
			return indicator.JOOBelkhayateCOG(Input, degree, period, strdDev, strdDev2, strdDev3);
		}

		public Indicators.JOO.JOOBelkhayateCOG JOOBelkhayateCOG(ISeries<double> input , int degree, int period, double strdDev, double strdDev2, double strdDev3)
		{
			return indicator.JOOBelkhayateCOG(input, degree, period, strdDev, strdDev2, strdDev3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.JOO.JOOBelkhayateCOG JOOBelkhayateCOG(int degree, int period, double strdDev, double strdDev2, double strdDev3)
		{
			return indicator.JOOBelkhayateCOG(Input, degree, period, strdDev, strdDev2, strdDev3);
		}

		public Indicators.JOO.JOOBelkhayateCOG JOOBelkhayateCOG(ISeries<double> input , int degree, int period, double strdDev, double strdDev2, double strdDev3)
		{
			return indicator.JOOBelkhayateCOG(input, degree, period, strdDev, strdDev2, strdDev3);
		}
	}
}

#endregion
