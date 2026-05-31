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
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber
{
	public class ATRTrailBands : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ATRTrailBands";
				Calculate									= Calculate.OnBarClose;
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
				
				Period					= 5;
				Multi					= 2;
//				AddPlot(Brushes.Red, "TrailingStopHigh");
//				AddPlot(Brushes.Blue, "TrailingStopLow");
				
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Dash, 2), PlotStyle.Line, "TrailingStopHigh");
				AddPlot(new Stroke(Brushes.Blue, DashStyleHelper.Dash, 2), PlotStyle.Line, "TrailingStopLow");
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Period)
				return;

			double loss = ATR(Input, Period)[0] * Multi;
			
	
			TrailingStopHigh[0] = Close[0] + loss;
			
			TrailingStopLow[0] = Close[0] - loss;
			
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="ATR period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Multi", Description="ATR multiplication", Order=2, GroupName="Parameters")]
		public double Multi
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrailingStopHigh
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrailingStopLow
		{
			get { return Values[1]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber.ATRTrailBands[] cacheATRTrailBands;
		public TradeSaber.ATRTrailBands ATRTrailBands(int period, double multi)
		{
			return ATRTrailBands(Input, period, multi);
		}

		public TradeSaber.ATRTrailBands ATRTrailBands(ISeries<double> input, int period, double multi)
		{
			if (cacheATRTrailBands != null)
				for (int idx = 0; idx < cacheATRTrailBands.Length; idx++)
					if (cacheATRTrailBands[idx] != null && cacheATRTrailBands[idx].Period == period && cacheATRTrailBands[idx].Multi == multi && cacheATRTrailBands[idx].EqualsInput(input))
						return cacheATRTrailBands[idx];
			return CacheIndicator<TradeSaber.ATRTrailBands>(new TradeSaber.ATRTrailBands(){ Period = period, Multi = multi }, input, ref cacheATRTrailBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.ATRTrailBands ATRTrailBands(int period, double multi)
		{
			return indicator.ATRTrailBands(Input, period, multi);
		}

		public Indicators.TradeSaber.ATRTrailBands ATRTrailBands(ISeries<double> input , int period, double multi)
		{
			return indicator.ATRTrailBands(input, period, multi);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.ATRTrailBands ATRTrailBands(int period, double multi)
		{
			return indicator.ATRTrailBands(Input, period, multi);
		}

		public Indicators.TradeSaber.ATRTrailBands ATRTrailBands(ISeries<double> input , int period, double multi)
		{
			return indicator.ATRTrailBands(input, period, multi);
		}
	}
}

#endregion
