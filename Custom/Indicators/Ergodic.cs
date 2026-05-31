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





namespace NinjaTrader.NinjaScript.Indicators
{



    public class Ergodic : Indicator
    {


        #region class variables
        private TSI myTSI;
        private EMA myEMA;
        #endregion


        #region OnStateChange()
        protected override void OnStateChange()
        {
            #region State.SetDefaults
            if (State == State.SetDefaults)
            {
				#region properties
                Description                 = "";
                Name                        = "Ergodic";

                ScaleJustification          = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                Calculate                   = Calculate.OnPriceChange;

                IsOverlay                   = false;
                DisplayInDataBox            = true;
                DrawOnPricePanel            = false;
                DrawHorizontalGridLines     = false;
                DrawVerticalGridLines       = false;
                PaintPriceMarkers           = true;
                IsSuspendedWhileInactive    = true;
				#endregion

				#region plots
                AddPlot(new Stroke(Brushes.Magenta, DashStyleHelper.Solid, 2), PlotStyle.Line, "Signal");
                AddPlot(new Stroke(Brushes.Black,   DashStyleHelper.Solid, 2), PlotStyle.Line, "Main");
                AddPlot(new Stroke(Brushes.Green,   DashStyleHelper.Solid, 4), PlotStyle.Bar,  "MainUp");
                AddPlot(new Stroke(Brushes.Crimson, DashStyleHelper.Solid, 4), PlotStyle.Bar,  "MainDown");
                AddPlot(new Stroke(Brushes.Red,     DashStyleHelper.Solid, 4), PlotStyle.Dot,  "CrossDown");
                AddPlot(new Stroke(Brushes.Lime,    DashStyleHelper.Solid, 4), PlotStyle.Dot,  "CrossUp");

                Plots[2].Min 				= 0;
                Plots[3].Max 				= 0;
				#endregion

				#region lines
                AddLine(Brushes.DarkGray,   0, "Zero");
                AddLine(Brushes.DarkGray,  25, "UpperLevel");
                AddLine(Brushes.DarkGray, -25, "LowerLevel");
				#endregion

				#region plots
                Fast 						= 3;
                Slow 						= 14;
                SignalLen 					= 5;
				#endregion
            }
            #endregion

            #region State.DataLoaded
            else if (State == State.DataLoaded)
            {
                myTSI 						= TSI(Close, Fast, Slow);
                myEMA 						= EMA(myTSI, SignalLen);
            }
            #endregion

            #region State.Terminated
            else if (State == State.Terminated)
            {
				myTSI						= null;
				myEMA						= null;
            }
            #endregion
        }
        #endregion


        #region OnBarUpdate()
        protected override void OnBarUpdate()
        {
            if (CurrentBar < 25)
            {
                return;
            }

            double ergo         = myTSI[0];
            double prev_ergo    = myTSI[1];
            double signal       = myEMA[0];
            double prev_signal  = myEMA[1];

            Main[0]             = ergo;
            MainUp[0]           = prev_ergo;
            MainDown[0]         = ergo;
            Signal[0]           = signal;

            // Draw a dot where the signal and main lines cross.  Set a plot to +/- 1 to indicate long/short
            if (signal > ergo && prev_signal <= prev_ergo && ergo > 20)
            {
                CrossDown[0]    = signal;
            }
            if (signal < ergo && prev_signal >= prev_ergo && ergo < -20)
            {
                CrossUp[0]      = signal;
            }
        }
        #endregion


        #region Properties

        #region inputs

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Fast", Order = 0, GroupName = "Parameters")]
        public int Fast
        { get; set; }

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Slow", Order = 1, GroupName = "Parameters")]
        public int Slow
        { get; set; }

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "Signal Length", Order = 2, GroupName = "Parameters")]
        public int SignalLen
        { get; set; }

        #endregion

        #region plots

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Signal
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Main
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MainUp
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MainDown
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> CrossDown
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> CrossUp
        {
            get { return Values[5]; }
        }

        #endregion

        #endregion


    }



}























//	Keep separate

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Ergodic[] cacheErgodic;
		public Ergodic Ergodic(int fast, int slow, int signalLen)
		{
			return Ergodic(Input, fast, slow, signalLen);
		}

		public Ergodic Ergodic(ISeries<double> input, int fast, int slow, int signalLen)
		{
			if (cacheErgodic != null)
				for (int idx = 0; idx < cacheErgodic.Length; idx++)
					if (cacheErgodic[idx] != null && cacheErgodic[idx].Fast == fast && cacheErgodic[idx].Slow == slow && cacheErgodic[idx].SignalLen == signalLen && cacheErgodic[idx].EqualsInput(input))
						return cacheErgodic[idx];
			return CacheIndicator<Ergodic>(new Ergodic(){ Fast = fast, Slow = slow, SignalLen = signalLen }, input, ref cacheErgodic);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Ergodic Ergodic(int fast, int slow, int signalLen)
		{
			return indicator.Ergodic(Input, fast, slow, signalLen);
		}

		public Indicators.Ergodic Ergodic(ISeries<double> input , int fast, int slow, int signalLen)
		{
			return indicator.Ergodic(input, fast, slow, signalLen);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Ergodic Ergodic(int fast, int slow, int signalLen)
		{
			return indicator.Ergodic(Input, fast, slow, signalLen);
		}

		public Indicators.Ergodic Ergodic(ISeries<double> input , int fast, int slow, int signalLen)
		{
			return indicator.Ergodic(input, fast, slow, signalLen);
		}
	}
}

#endregion
