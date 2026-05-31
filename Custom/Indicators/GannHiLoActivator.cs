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
	public class GannHiLoActivator : Indicator
	{
		private double swing;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"GannHiLoActivator";
				Name										= "GannHiLoActivator";
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
				Period					= 10;
				AddPlot(Brushes.Indigo, "HiLo");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			double val1_high	= SMA(High, Period)[0];
			double val2_low		= SMA(Low, Period)[0];
			
			if (Close[0] < val2_low)	swing = -1;
			if (Close[0] > val1_high)	swing = 1;
			
			if (swing == 1)
			{
				Values[0][0]  = val2_low;
			}
			else if (swing == -1)
			{
				Values[0][0] = val1_high;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HiLo
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
		private GannHiLoActivator[] cacheGannHiLoActivator;
		public GannHiLoActivator GannHiLoActivator(int period)
		{
			return GannHiLoActivator(Input, period);
		}

		public GannHiLoActivator GannHiLoActivator(ISeries<double> input, int period)
		{
			if (cacheGannHiLoActivator != null)
				for (int idx = 0; idx < cacheGannHiLoActivator.Length; idx++)
					if (cacheGannHiLoActivator[idx] != null && cacheGannHiLoActivator[idx].Period == period && cacheGannHiLoActivator[idx].EqualsInput(input))
						return cacheGannHiLoActivator[idx];
			return CacheIndicator<GannHiLoActivator>(new GannHiLoActivator(){ Period = period }, input, ref cacheGannHiLoActivator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GannHiLoActivator GannHiLoActivator(int period)
		{
			return indicator.GannHiLoActivator(Input, period);
		}

		public Indicators.GannHiLoActivator GannHiLoActivator(ISeries<double> input , int period)
		{
			return indicator.GannHiLoActivator(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GannHiLoActivator GannHiLoActivator(int period)
		{
			return indicator.GannHiLoActivator(Input, period);
		}

		public Indicators.GannHiLoActivator GannHiLoActivator(ISeries<double> input , int period)
		{
			return indicator.GannHiLoActivator(input, period);
		}
	}
}

#endregion
