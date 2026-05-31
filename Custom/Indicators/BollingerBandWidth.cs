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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BollingerBandWidth : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Calculates Width of Bollinger Bands based on period and std. deviation.";
				Name						= "BollingerBandWidth";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				Period						= 20;
				Deviation					= 2;
				AddPlot(Brushes.OrangeRed, "Plot0");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar > Period) 
			{
				Plot0[0] = (Bollinger(Deviation,Period).Upper[0] - Bollinger(Deviation,Period).Lower[0])/SMA(20)[0];
				
			}
			else
			{		
				Plot0[0] = 0;
			}
		}

		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Description="Used to calculates Bollinger Bands for these # of days", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Range(1, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Deviation", Description="Used to calculates Bollinger Bands for this standard deviation", Order=2, GroupName="Parameters")]
		public double Deviation
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Plot0
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
		private BollingerBandWidth[] cacheBollingerBandWidth;
		public BollingerBandWidth BollingerBandWidth(int period, double deviation)
		{
			return BollingerBandWidth(Input, period, deviation);
		}

		public BollingerBandWidth BollingerBandWidth(ISeries<double> input, int period, double deviation)
		{
			if (cacheBollingerBandWidth != null)
				for (int idx = 0; idx < cacheBollingerBandWidth.Length; idx++)
					if (cacheBollingerBandWidth[idx] != null && cacheBollingerBandWidth[idx].Period == period && cacheBollingerBandWidth[idx].Deviation == deviation && cacheBollingerBandWidth[idx].EqualsInput(input))
						return cacheBollingerBandWidth[idx];
			return CacheIndicator<BollingerBandWidth>(new BollingerBandWidth(){ Period = period, Deviation = deviation }, input, ref cacheBollingerBandWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerBandWidth BollingerBandWidth(int period, double deviation)
		{
			return indicator.BollingerBandWidth(Input, period, deviation);
		}

		public Indicators.BollingerBandWidth BollingerBandWidth(ISeries<double> input , int period, double deviation)
		{
			return indicator.BollingerBandWidth(input, period, deviation);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerBandWidth BollingerBandWidth(int period, double deviation)
		{
			return indicator.BollingerBandWidth(Input, period, deviation);
		}

		public Indicators.BollingerBandWidth BollingerBandWidth(ISeries<double> input , int period, double deviation)
		{
			return indicator.BollingerBandWidth(input, period, deviation);
		}
	}
}

#endregion
