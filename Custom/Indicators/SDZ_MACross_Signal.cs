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
	public class SDZ_MACross_Signal  : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SDZ_MACross_Signal";
				Calculate									= Calculate.OnBarClose;
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
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SDZ_MACross_Signal[] cacheSDZ_MACross_Signal;
		public SDZ_MACross_Signal SDZ_MACross_Signal()
		{
			return SDZ_MACross_Signal(Input);
		}

		public SDZ_MACross_Signal SDZ_MACross_Signal(ISeries<double> input)
		{
			if (cacheSDZ_MACross_Signal != null)
				for (int idx = 0; idx < cacheSDZ_MACross_Signal.Length; idx++)
					if (cacheSDZ_MACross_Signal[idx] != null &&  cacheSDZ_MACross_Signal[idx].EqualsInput(input))
						return cacheSDZ_MACross_Signal[idx];
			return CacheIndicator<SDZ_MACross_Signal>(new SDZ_MACross_Signal(), input, ref cacheSDZ_MACross_Signal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SDZ_MACross_Signal SDZ_MACross_Signal()
		{
			return indicator.SDZ_MACross_Signal(Input);
		}

		public Indicators.SDZ_MACross_Signal SDZ_MACross_Signal(ISeries<double> input )
		{
			return indicator.SDZ_MACross_Signal(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SDZ_MACross_Signal SDZ_MACross_Signal()
		{
			return indicator.SDZ_MACross_Signal(Input);
		}

		public Indicators.SDZ_MACross_Signal SDZ_MACross_Signal(ISeries<double> input )
		{
			return indicator.SDZ_MACross_Signal(input);
		}
	}
}

#endregion
