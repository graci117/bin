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
		
		private QuantVue.QstepMAIndicator[] cacheQstepMAIndicator;

		
		public QuantVue.QstepMAIndicator QstepMAIndicator(int grid1Period1, int grid1omaL, double grid1omaS, bool grid1omaA, double grid1Sensitivity, double grid1StepSize, int grid1Period2, int stepMAOffsetTick)
		{
			return QstepMAIndicator(Input, grid1Period1, grid1omaL, grid1omaS, grid1omaA, grid1Sensitivity, grid1StepSize, grid1Period2, stepMAOffsetTick);
		}


		
		public QuantVue.QstepMAIndicator QstepMAIndicator(ISeries<double> input, int grid1Period1, int grid1omaL, double grid1omaS, bool grid1omaA, double grid1Sensitivity, double grid1StepSize, int grid1Period2, int stepMAOffsetTick)
		{
			if (cacheQstepMAIndicator != null)
				for (int idx = 0; idx < cacheQstepMAIndicator.Length; idx++)
					if (cacheQstepMAIndicator[idx].grid1Period1 == grid1Period1 && cacheQstepMAIndicator[idx].grid1omaL == grid1omaL && cacheQstepMAIndicator[idx].grid1omaS == grid1omaS && cacheQstepMAIndicator[idx].grid1omaA == grid1omaA && cacheQstepMAIndicator[idx].grid1Sensitivity == grid1Sensitivity && cacheQstepMAIndicator[idx].grid1StepSize == grid1StepSize && cacheQstepMAIndicator[idx].grid1Period2 == grid1Period2 && cacheQstepMAIndicator[idx].stepMAOffsetTick == stepMAOffsetTick && cacheQstepMAIndicator[idx].EqualsInput(input))
						return cacheQstepMAIndicator[idx];
			return CacheIndicator<QuantVue.QstepMAIndicator>(new QuantVue.QstepMAIndicator(){ grid1Period1 = grid1Period1, grid1omaL = grid1omaL, grid1omaS = grid1omaS, grid1omaA = grid1omaA, grid1Sensitivity = grid1Sensitivity, grid1StepSize = grid1StepSize, grid1Period2 = grid1Period2, stepMAOffsetTick = stepMAOffsetTick }, input, ref cacheQstepMAIndicator);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.QuantVue.QstepMAIndicator QstepMAIndicator(int grid1Period1, int grid1omaL, double grid1omaS, bool grid1omaA, double grid1Sensitivity, double grid1StepSize, int grid1Period2, int stepMAOffsetTick)
		{
			return indicator.QstepMAIndicator(Input, grid1Period1, grid1omaL, grid1omaS, grid1omaA, grid1Sensitivity, grid1StepSize, grid1Period2, stepMAOffsetTick);
		}


		
		public Indicators.QuantVue.QstepMAIndicator QstepMAIndicator(ISeries<double> input , int grid1Period1, int grid1omaL, double grid1omaS, bool grid1omaA, double grid1Sensitivity, double grid1StepSize, int grid1Period2, int stepMAOffsetTick)
		{
			return indicator.QstepMAIndicator(input, grid1Period1, grid1omaL, grid1omaS, grid1omaA, grid1Sensitivity, grid1StepSize, grid1Period2, stepMAOffsetTick);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.QuantVue.QstepMAIndicator QstepMAIndicator(int grid1Period1, int grid1omaL, double grid1omaS, bool grid1omaA, double grid1Sensitivity, double grid1StepSize, int grid1Period2, int stepMAOffsetTick)
		{
			return indicator.QstepMAIndicator(Input, grid1Period1, grid1omaL, grid1omaS, grid1omaA, grid1Sensitivity, grid1StepSize, grid1Period2, stepMAOffsetTick);
		}


		
		public Indicators.QuantVue.QstepMAIndicator QstepMAIndicator(ISeries<double> input , int grid1Period1, int grid1omaL, double grid1omaS, bool grid1omaA, double grid1Sensitivity, double grid1StepSize, int grid1Period2, int stepMAOffsetTick)
		{
			return indicator.QstepMAIndicator(input, grid1Period1, grid1omaL, grid1omaS, grid1omaA, grid1Sensitivity, grid1StepSize, grid1Period2, stepMAOffsetTick);
		}

	}
}

#endregion
