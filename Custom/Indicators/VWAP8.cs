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
	public class VWAP8 : Indicator
	{
		double	iCumVolume			= 0;
		double	iCumTypicalVolume	= 0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Volume Weighted Average Price";
				Name								= "VWAP8";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				AddPlot(Brushes.Black, "PlotVWAP");
			}
		}

		protected override void OnBarUpdate()
		{
			if (Bars.IsFirstBarOfSession)
			{
				iCumVolume = VOL()[0];
				iCumTypicalVolume = VOL()[0] * ((High[0] + Low[0] + Close[0]) / 3);
			}
			else
			{
				iCumVolume = iCumVolume + VOL()[0];
				iCumTypicalVolume = iCumTypicalVolume + (VOL()[0] * ((High[0] + Low[0] + Close[0]) / 3));
			}

			PlotVWAP[0] = (iCumTypicalVolume / iCumVolume);
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotVWAP
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
		private VWAP8[] cacheVWAP8;
		public VWAP8 VWAP8()
		{
			return VWAP8(Input);
		}

		public VWAP8 VWAP8(ISeries<double> input)
		{
			if (cacheVWAP8 != null)
				for (int idx = 0; idx < cacheVWAP8.Length; idx++)
					if (cacheVWAP8[idx] != null &&  cacheVWAP8[idx].EqualsInput(input))
						return cacheVWAP8[idx];
			return CacheIndicator<VWAP8>(new VWAP8(), input, ref cacheVWAP8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VWAP8 VWAP8()
		{
			return indicator.VWAP8(Input);
		}

		public Indicators.VWAP8 VWAP8(ISeries<double> input )
		{
			return indicator.VWAP8(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VWAP8 VWAP8()
		{
			return indicator.VWAP8(Input);
		}

		public Indicators.VWAP8 VWAP8(ISeries<double> input )
		{
			return indicator.VWAP8(input);
		}
	}
}

#endregion
