// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
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
    /// <summary>
	///  This is a modification to the excelent TChannel2 indicator
	/// 
	/// </summary>
	public class TChannel3Tony : Indicator
	{

  private bool		useSMMA	= true;	
private EMA		ema;
		private double avg;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "Trend Channel of StandardDeviation of Movingaverage relation to RSI and ATR";
				Name						= "TChannel3Tony";

		        IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				
				MALength = 21;  // Default setting for MALength
				Dev	   = 2; // Default setting for Const
				RSILength = 21;
				XL = 51;
			    XS = 49;
				
				AddPlot(Brushes.Transparent, NinjaTrader.Custom.Resource.BollingerMiddleBand);
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "UCrosses");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Dot, "LCrosses");
				AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "Upper");
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "Lower");
				Plots[3].DashStyleHelper = DashStyleHelper.Dot;
				Plots[4].DashStyleHelper = DashStyleHelper.Dot;


			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot	= MALength;
			}
			else if (State == State.DataLoaded)
			{
				ema = EMA(Input, MALength);
	
			}	
		}

		protected override void OnBarUpdate()
		{

			
//double ema0		= ema[0];
		Middle[0] = ema[0];
//		double avg = SMA(ATR(1),MALength)[0];
		if (!useSMMA)	
			avg = MAAverage(ATR(1),MALength)[0];
		else
			avg = SMMA(ATR(1),MALength)[0];
		
            Upper[0] = Middle[0] + avg*Dev;
            Lower[0] = Middle[0] - avg*Dev;
			
			double ttt = RSI(Input,RSILength,1)[0];
			if (ttt > XL) 
			{	PlotBrushes[2][0]=Plots[2].Brush;
				LC[0] = Lower[0];
	//			tradeTrigger = 1;
			}	
			else
				PlotBrushes[2][0] = Brushes.Transparent;
		
			if(ttt < XS) 
			{	PlotBrushes[1][0]=Plots[1].Brush;
				UC[0] =Upper[0];
//				tradeTrigger = -1;
			}
			else
				PlotBrushes[1][0]=Brushes.Transparent;
			
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[3]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[4]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UC
		{
			get { return Values[1]; }
		}		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LC
		{
			get { return Values[2]; }
		}	
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[0]; }
		}

		[Range(0.00001, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDev", GroupName = "NinjaScriptParameters", Order = 0)]
		public double Dev
		{ get; set; }
		
		[Range(0.00001, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OverSold XL", GroupName = "NinjaScriptParameters", Order = 0)]
		public double XL
		{ get; set; }
		
		[Range(0.00001, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OverBought XS", GroupName = "NinjaScriptParameters", Order = 0)]
		public double XS
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMA Period", GroupName = "NinjaScriptParameters", Order = 1)]
		public int MALength
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "RSI Period", GroupName = "NinjaScriptParameters", Order = 1)]
		public int RSILength
		{ get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Use SMMA instead ", GroupName = "SMMA Options", Order = 2)]
        public bool UseSMMA
        {
            get { return useSMMA; }
            set { useSMMA = value; }
        }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TChannel3Tony[] cacheTChannel3Tony;
		public TChannel3Tony TChannel3Tony(double dev, double xL, double xS, int mALength, int rSILength)
		{
			return TChannel3Tony(Input, dev, xL, xS, mALength, rSILength);
		}

		public TChannel3Tony TChannel3Tony(ISeries<double> input, double dev, double xL, double xS, int mALength, int rSILength)
		{
			if (cacheTChannel3Tony != null)
				for (int idx = 0; idx < cacheTChannel3Tony.Length; idx++)
					if (cacheTChannel3Tony[idx] != null && cacheTChannel3Tony[idx].Dev == dev && cacheTChannel3Tony[idx].XL == xL && cacheTChannel3Tony[idx].XS == xS && cacheTChannel3Tony[idx].MALength == mALength && cacheTChannel3Tony[idx].RSILength == rSILength && cacheTChannel3Tony[idx].EqualsInput(input))
						return cacheTChannel3Tony[idx];
			return CacheIndicator<TChannel3Tony>(new TChannel3Tony(){ Dev = dev, XL = xL, XS = xS, MALength = mALength, RSILength = rSILength }, input, ref cacheTChannel3Tony);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TChannel3Tony TChannel3Tony(double dev, double xL, double xS, int mALength, int rSILength)
		{
			return indicator.TChannel3Tony(Input, dev, xL, xS, mALength, rSILength);
		}

		public Indicators.TChannel3Tony TChannel3Tony(ISeries<double> input , double dev, double xL, double xS, int mALength, int rSILength)
		{
			return indicator.TChannel3Tony(input, dev, xL, xS, mALength, rSILength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TChannel3Tony TChannel3Tony(double dev, double xL, double xS, int mALength, int rSILength)
		{
			return indicator.TChannel3Tony(Input, dev, xL, xS, mALength, rSILength);
		}

		public Indicators.TChannel3Tony TChannel3Tony(ISeries<double> input , double dev, double xL, double xS, int mALength, int rSILength)
		{
			return indicator.TChannel3Tony(input, dev, xL, xS, mALength, rSILength);
		}
	}
}

#endregion
