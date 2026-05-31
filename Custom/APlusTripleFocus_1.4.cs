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
		
		private A_Plus.APlusTripleFocus[] cacheAPlusTripleFocus;

		
		public A_Plus.APlusTripleFocus APlusTripleFocus(int fast, int slow, int smooth, PriceOBandPreset bandPreset, double customBand, Brush lowerLineC, double customRegion, int rOpacity, Brush regionColour, bool colourOsc, bool colourOscZone, bool colourZeroZone, Brush oscColourUp, Brush oscColourDn)
		{
			return APlusTripleFocus(Input, fast, slow, smooth, bandPreset, customBand, lowerLineC, customRegion, rOpacity, regionColour, colourOsc, colourOscZone, colourZeroZone, oscColourUp, oscColourDn);
		}


		
		public A_Plus.APlusTripleFocus APlusTripleFocus(ISeries<double> input, int fast, int slow, int smooth, PriceOBandPreset bandPreset, double customBand, Brush lowerLineC, double customRegion, int rOpacity, Brush regionColour, bool colourOsc, bool colourOscZone, bool colourZeroZone, Brush oscColourUp, Brush oscColourDn)
		{
			if (cacheAPlusTripleFocus != null)
				for (int idx = 0; idx < cacheAPlusTripleFocus.Length; idx++)
					if (cacheAPlusTripleFocus[idx].Fast == fast && cacheAPlusTripleFocus[idx].Slow == slow && cacheAPlusTripleFocus[idx].Smooth == smooth && cacheAPlusTripleFocus[idx].BandPreset == bandPreset && cacheAPlusTripleFocus[idx].CustomBand == customBand && cacheAPlusTripleFocus[idx].LowerLineC == lowerLineC && cacheAPlusTripleFocus[idx].CustomRegion == customRegion && cacheAPlusTripleFocus[idx].ROpacity == rOpacity && cacheAPlusTripleFocus[idx].RegionColour == regionColour && cacheAPlusTripleFocus[idx].colourOsc == colourOsc && cacheAPlusTripleFocus[idx].colourOscZone == colourOscZone && cacheAPlusTripleFocus[idx].colourZeroZone == colourZeroZone && cacheAPlusTripleFocus[idx].OscColourUp == oscColourUp && cacheAPlusTripleFocus[idx].OscColourDn == oscColourDn && cacheAPlusTripleFocus[idx].EqualsInput(input))
						return cacheAPlusTripleFocus[idx];
			return CacheIndicator<A_Plus.APlusTripleFocus>(new A_Plus.APlusTripleFocus(){ Fast = fast, Slow = slow, Smooth = smooth, BandPreset = bandPreset, CustomBand = customBand, LowerLineC = lowerLineC, CustomRegion = customRegion, ROpacity = rOpacity, RegionColour = regionColour, colourOsc = colourOsc, colourOscZone = colourOscZone, colourZeroZone = colourZeroZone, OscColourUp = oscColourUp, OscColourDn = oscColourDn }, input, ref cacheAPlusTripleFocus);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.A_Plus.APlusTripleFocus APlusTripleFocus(int fast, int slow, int smooth, PriceOBandPreset bandPreset, double customBand, Brush lowerLineC, double customRegion, int rOpacity, Brush regionColour, bool colourOsc, bool colourOscZone, bool colourZeroZone, Brush oscColourUp, Brush oscColourDn)
		{
			return indicator.APlusTripleFocus(Input, fast, slow, smooth, bandPreset, customBand, lowerLineC, customRegion, rOpacity, regionColour, colourOsc, colourOscZone, colourZeroZone, oscColourUp, oscColourDn);
		}


		
		public Indicators.A_Plus.APlusTripleFocus APlusTripleFocus(ISeries<double> input , int fast, int slow, int smooth, PriceOBandPreset bandPreset, double customBand, Brush lowerLineC, double customRegion, int rOpacity, Brush regionColour, bool colourOsc, bool colourOscZone, bool colourZeroZone, Brush oscColourUp, Brush oscColourDn)
		{
			return indicator.APlusTripleFocus(input, fast, slow, smooth, bandPreset, customBand, lowerLineC, customRegion, rOpacity, regionColour, colourOsc, colourOscZone, colourZeroZone, oscColourUp, oscColourDn);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.A_Plus.APlusTripleFocus APlusTripleFocus(int fast, int slow, int smooth, PriceOBandPreset bandPreset, double customBand, Brush lowerLineC, double customRegion, int rOpacity, Brush regionColour, bool colourOsc, bool colourOscZone, bool colourZeroZone, Brush oscColourUp, Brush oscColourDn)
		{
			return indicator.APlusTripleFocus(Input, fast, slow, smooth, bandPreset, customBand, lowerLineC, customRegion, rOpacity, regionColour, colourOsc, colourOscZone, colourZeroZone, oscColourUp, oscColourDn);
		}


		
		public Indicators.A_Plus.APlusTripleFocus APlusTripleFocus(ISeries<double> input , int fast, int slow, int smooth, PriceOBandPreset bandPreset, double customBand, Brush lowerLineC, double customRegion, int rOpacity, Brush regionColour, bool colourOsc, bool colourOscZone, bool colourZeroZone, Brush oscColourUp, Brush oscColourDn)
		{
			return indicator.APlusTripleFocus(input, fast, slow, smooth, bandPreset, customBand, lowerLineC, customRegion, rOpacity, regionColour, colourOsc, colourOscZone, colourZeroZone, oscColourUp, oscColourDn);
		}

	}
}

#endregion
