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

#endregion



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private FisherTransformMod[] cacheFisherTransformMod;

		
		public FisherTransformMod FisherTransformMod(int period, double upperValue, double lowerValue, bool colourRegion, int highlightBarsCount, bool shadeExtremeFlipSimple)
		{
			return FisherTransformMod(Input, period, upperValue, lowerValue, colourRegion, highlightBarsCount, shadeExtremeFlipSimple);
		}


		
		public FisherTransformMod FisherTransformMod(ISeries<double> input, int period, double upperValue, double lowerValue, bool colourRegion, int highlightBarsCount, bool shadeExtremeFlipSimple)
		{
			if (cacheFisherTransformMod != null)
				for (int idx = 0; idx < cacheFisherTransformMod.Length; idx++)
					if (cacheFisherTransformMod[idx].Period == period && cacheFisherTransformMod[idx].UpperValue == upperValue && cacheFisherTransformMod[idx].LowerValue == lowerValue && cacheFisherTransformMod[idx].ColourRegion == colourRegion && cacheFisherTransformMod[idx].HighlightBarsCount == highlightBarsCount && cacheFisherTransformMod[idx].ShadeExtremeFlipSimple == shadeExtremeFlipSimple && cacheFisherTransformMod[idx].EqualsInput(input))
						return cacheFisherTransformMod[idx];
			return CacheIndicator<FisherTransformMod>(new FisherTransformMod(){ Period = period, UpperValue = upperValue, LowerValue = lowerValue, ColourRegion = colourRegion, HighlightBarsCount = highlightBarsCount, ShadeExtremeFlipSimple = shadeExtremeFlipSimple }, input, ref cacheFisherTransformMod);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.FisherTransformMod FisherTransformMod(int period, double upperValue, double lowerValue, bool colourRegion, int highlightBarsCount, bool shadeExtremeFlipSimple)
		{
			return indicator.FisherTransformMod(Input, period, upperValue, lowerValue, colourRegion, highlightBarsCount, shadeExtremeFlipSimple);
		}


		
		public Indicators.FisherTransformMod FisherTransformMod(ISeries<double> input , int period, double upperValue, double lowerValue, bool colourRegion, int highlightBarsCount, bool shadeExtremeFlipSimple)
		{
			return indicator.FisherTransformMod(input, period, upperValue, lowerValue, colourRegion, highlightBarsCount, shadeExtremeFlipSimple);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.FisherTransformMod FisherTransformMod(int period, double upperValue, double lowerValue, bool colourRegion, int highlightBarsCount, bool shadeExtremeFlipSimple)
		{
			return indicator.FisherTransformMod(Input, period, upperValue, lowerValue, colourRegion, highlightBarsCount, shadeExtremeFlipSimple);
		}


		
		public Indicators.FisherTransformMod FisherTransformMod(ISeries<double> input , int period, double upperValue, double lowerValue, bool colourRegion, int highlightBarsCount, bool shadeExtremeFlipSimple)
		{
			return indicator.FisherTransformMod(input, period, upperValue, lowerValue, colourRegion, highlightBarsCount, shadeExtremeFlipSimple);
		}

	}
}

#endregion
