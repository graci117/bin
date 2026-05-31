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
namespace NinjaTrader.NinjaScript.Indicators.Pdt
{
	public class ZScoreV10 : Indicator
	{

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"ZScore by FutureDragon";
				Name										= "ZScoreV10";
				Calculate									= Calculate.OnPriceChange;
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
				
				Upper1_Offset					= 1;
				Upper2_Offset					= 2;
                Upper3_Offset					= 3;
                Lower1_Offset					= -1;
                Lower2_Offset					= -2;
                Lower3_Offset					= -3;

                AveragePeriod = 100;
                StdPeriod = 100;

                AddPlot(Brushes.Gold, "Z");

                AddLine(new Gui.Stroke(Brushes.White, Gui.DashStyleHelper.Dash, 1), 0, "Mid");

                AddLine(new Stroke(Brushes.OrangeRed),  Upper3_Offset,		"Upper3");
                AddLine(new Stroke(Brushes.Red),		Upper2_Offset,		"Upper2");
                AddLine(new Stroke(Brushes.DarkRed),	Upper1_Offset,		"Upper1");

                AddLine(new Stroke(Brushes.Cyan),		Lower3_Offset,		"Lower3");
                AddLine(new Stroke(Brushes.DarkCyan),	Lower2_Offset,		"Lower2");
                AddLine(new Stroke(Brushes.DarkBlue),   Lower1_Offset,		"Lower1");

            }
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar < 34)
                return;

            //ZScore
            double tempZ = SMA(Typical, AveragePeriod)[0];
            tempZ = (Typical[0] - tempZ) / StdDev(Typical, StdPeriod)[0];

			if (Math.Abs(tempZ) > 5)//Removing outlier data point.
			{
				tempZ = Z[1];
			}

			Z[0] = tempZ;


        }

		#region Properties
		//[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name= "Upper1Offset", Order= 10, GroupName="Parameters")]
		public double Upper1_Offset
		{ get; set; }

		//[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name= "Upper2Offset", Order= 20, GroupName="Parameters")]
		public double Upper2_Offset
		{ get; set; }

        //[NinjaScriptProperty]
        [Range(0, double.MaxValue)]
        [Display(Name = "Upper3Offset", Order = 30, GroupName = "Parameters")]
        public double Upper3_Offset
        { get; set; }

        //[NinjaScriptProperty]
		[Range(double.MinValue, 0)]
		[Display(Name= "Lower1Offset", Order= 50, GroupName="Parameters")]
		public double Lower1_Offset
		{ get; set; }

        //[NinjaScriptProperty]
        [Range(double.MinValue, 0)]
        [Display(Name = "Lower2Offset", Order = 60, GroupName = "Parameters")]
        public double Lower2_Offset
        { get; set; }

        //[NinjaScriptProperty]
        [Range(double.MinValue, 0)]
        [Display(Name = "Lower3_Offset", Order = 70, GroupName = "Parameters")]
        public double Lower3_Offset
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "AveragePeriod", Order = 80, GroupName = "Parameters")]
        public int AveragePeriod
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "StdPeriod", Order = 90, GroupName = "Parameters")]
        public int StdPeriod
        { get; set; }


        [Browsable(false)]
		[XmlIgnore]
		public Series<double> Z
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
		private Pdt.ZScoreV10[] cacheZScoreV10;
		public Pdt.ZScoreV10 ZScoreV10(int averagePeriod, int stdPeriod)
		{
			return ZScoreV10(Input, averagePeriod, stdPeriod);
		}

		public Pdt.ZScoreV10 ZScoreV10(ISeries<double> input, int averagePeriod, int stdPeriod)
		{
			if (cacheZScoreV10 != null)
				for (int idx = 0; idx < cacheZScoreV10.Length; idx++)
					if (cacheZScoreV10[idx] != null && cacheZScoreV10[idx].AveragePeriod == averagePeriod && cacheZScoreV10[idx].StdPeriod == stdPeriod && cacheZScoreV10[idx].EqualsInput(input))
						return cacheZScoreV10[idx];
			return CacheIndicator<Pdt.ZScoreV10>(new Pdt.ZScoreV10(){ AveragePeriod = averagePeriod, StdPeriod = stdPeriod }, input, ref cacheZScoreV10);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Pdt.ZScoreV10 ZScoreV10(int averagePeriod, int stdPeriod)
		{
			return indicator.ZScoreV10(Input, averagePeriod, stdPeriod);
		}

		public Indicators.Pdt.ZScoreV10 ZScoreV10(ISeries<double> input , int averagePeriod, int stdPeriod)
		{
			return indicator.ZScoreV10(input, averagePeriod, stdPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Pdt.ZScoreV10 ZScoreV10(int averagePeriod, int stdPeriod)
		{
			return indicator.ZScoreV10(Input, averagePeriod, stdPeriod);
		}

		public Indicators.Pdt.ZScoreV10 ZScoreV10(ISeries<double> input , int averagePeriod, int stdPeriod)
		{
			return indicator.ZScoreV10(input, averagePeriod, stdPeriod);
		}
	}
}

#endregion
