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
		
		private QMomentum[] cacheQMomentum;
		private RMA[] cacheRMA;

		
		public QMomentum QMomentum(int length, QMomentumMAType mAType, bool alerts, bool plotBull, bool plotBear, bool plotBullH, bool plotBearH, bool divlabels, Brush upColor, Brush downColor, Brush upperThresholdColor, Brush zeroThresholdColor, Brush lowerThresholdColor, Brush blend1Color, Brush blend2Color, Brush blend3Color)
		{
			return QMomentum(Input, length, mAType, alerts, plotBull, plotBear, plotBullH, plotBearH, divlabels, upColor, downColor, upperThresholdColor, zeroThresholdColor, lowerThresholdColor, blend1Color, blend2Color, blend3Color);
		}

		public RMA RMA(int period)
		{
			return RMA(Input, period);
		}


		
		public QMomentum QMomentum(ISeries<double> input, int length, QMomentumMAType mAType, bool alerts, bool plotBull, bool plotBear, bool plotBullH, bool plotBearH, bool divlabels, Brush upColor, Brush downColor, Brush upperThresholdColor, Brush zeroThresholdColor, Brush lowerThresholdColor, Brush blend1Color, Brush blend2Color, Brush blend3Color)
		{
			if (cacheQMomentum != null)
				for (int idx = 0; idx < cacheQMomentum.Length; idx++)
					if (cacheQMomentum[idx].Length == length && cacheQMomentum[idx].MAType == mAType && cacheQMomentum[idx].Alerts == alerts && cacheQMomentum[idx].PlotBull == plotBull && cacheQMomentum[idx].PlotBear == plotBear && cacheQMomentum[idx].PlotBullH == plotBullH && cacheQMomentum[idx].PlotBearH == plotBearH && cacheQMomentum[idx].Divlabels == divlabels && cacheQMomentum[idx].UpColor == upColor && cacheQMomentum[idx].DownColor == downColor && cacheQMomentum[idx].UpperThresholdColor == upperThresholdColor && cacheQMomentum[idx].ZeroThresholdColor == zeroThresholdColor && cacheQMomentum[idx].LowerThresholdColor == lowerThresholdColor && cacheQMomentum[idx].Blend1Color == blend1Color && cacheQMomentum[idx].Blend2Color == blend2Color && cacheQMomentum[idx].Blend3Color == blend3Color && cacheQMomentum[idx].EqualsInput(input))
						return cacheQMomentum[idx];
			return CacheIndicator<QMomentum>(new QMomentum(){ Length = length, MAType = mAType, Alerts = alerts, PlotBull = plotBull, PlotBear = plotBear, PlotBullH = plotBullH, PlotBearH = plotBearH, Divlabels = divlabels, UpColor = upColor, DownColor = downColor, UpperThresholdColor = upperThresholdColor, ZeroThresholdColor = zeroThresholdColor, LowerThresholdColor = lowerThresholdColor, Blend1Color = blend1Color, Blend2Color = blend2Color, Blend3Color = blend3Color }, input, ref cacheQMomentum);
		}

		public RMA RMA(ISeries<double> input, int period)
		{
			if (cacheRMA != null)
				for (int idx = 0; idx < cacheRMA.Length; idx++)
					if (cacheRMA[idx].Period == period && cacheRMA[idx].EqualsInput(input))
						return cacheRMA[idx];
			return CacheIndicator<RMA>(new RMA(){ Period = period }, input, ref cacheRMA);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.QMomentum QMomentum(int length, QMomentumMAType mAType, bool alerts, bool plotBull, bool plotBear, bool plotBullH, bool plotBearH, bool divlabels, Brush upColor, Brush downColor, Brush upperThresholdColor, Brush zeroThresholdColor, Brush lowerThresholdColor, Brush blend1Color, Brush blend2Color, Brush blend3Color)
		{
			return indicator.QMomentum(Input, length, mAType, alerts, plotBull, plotBear, plotBullH, plotBearH, divlabels, upColor, downColor, upperThresholdColor, zeroThresholdColor, lowerThresholdColor, blend1Color, blend2Color, blend3Color);
		}

		public Indicators.RMA RMA(int period)
		{
			return indicator.RMA(Input, period);
		}


		
		public Indicators.QMomentum QMomentum(ISeries<double> input , int length, QMomentumMAType mAType, bool alerts, bool plotBull, bool plotBear, bool plotBullH, bool plotBearH, bool divlabels, Brush upColor, Brush downColor, Brush upperThresholdColor, Brush zeroThresholdColor, Brush lowerThresholdColor, Brush blend1Color, Brush blend2Color, Brush blend3Color)
		{
			return indicator.QMomentum(input, length, mAType, alerts, plotBull, plotBear, plotBullH, plotBearH, divlabels, upColor, downColor, upperThresholdColor, zeroThresholdColor, lowerThresholdColor, blend1Color, blend2Color, blend3Color);
		}

		public Indicators.RMA RMA(ISeries<double> input , int period)
		{
			return indicator.RMA(input, period);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.QMomentum QMomentum(int length, QMomentumMAType mAType, bool alerts, bool plotBull, bool plotBear, bool plotBullH, bool plotBearH, bool divlabels, Brush upColor, Brush downColor, Brush upperThresholdColor, Brush zeroThresholdColor, Brush lowerThresholdColor, Brush blend1Color, Brush blend2Color, Brush blend3Color)
		{
			return indicator.QMomentum(Input, length, mAType, alerts, plotBull, plotBear, plotBullH, plotBearH, divlabels, upColor, downColor, upperThresholdColor, zeroThresholdColor, lowerThresholdColor, blend1Color, blend2Color, blend3Color);
		}

		public Indicators.RMA RMA(int period)
		{
			return indicator.RMA(Input, period);
		}


		
		public Indicators.QMomentum QMomentum(ISeries<double> input , int length, QMomentumMAType mAType, bool alerts, bool plotBull, bool plotBear, bool plotBullH, bool plotBearH, bool divlabels, Brush upColor, Brush downColor, Brush upperThresholdColor, Brush zeroThresholdColor, Brush lowerThresholdColor, Brush blend1Color, Brush blend2Color, Brush blend3Color)
		{
			return indicator.QMomentum(input, length, mAType, alerts, plotBull, plotBear, plotBullH, plotBearH, divlabels, upColor, downColor, upperThresholdColor, zeroThresholdColor, lowerThresholdColor, blend1Color, blend2Color, blend3Color);
		}

		public Indicators.RMA RMA(ISeries<double> input , int period)
		{
			return indicator.RMA(input, period);
		}

	}
}

#endregion
