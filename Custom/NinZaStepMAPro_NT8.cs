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
		
		private ninZaStepMAPro[] cacheninZaStepMAPro;

		
		public ninZaStepMAPro ninZaStepMAPro(ninZaStepMAPro_BaseMode baseMode, bool baseHighLowEnabled, ninZa_MAType mAType, int mAPeriod, double sensitivityFactor, ninZaStepMAPro_StepSizeMode stepSizeMode, int stepSizeTicks, int stepSizeAutoLookback)
		{
			return ninZaStepMAPro(Input, baseMode, baseHighLowEnabled, mAType, mAPeriod, sensitivityFactor, stepSizeMode, stepSizeTicks, stepSizeAutoLookback);
		}


		
		public ninZaStepMAPro ninZaStepMAPro(ISeries<double> input, ninZaStepMAPro_BaseMode baseMode, bool baseHighLowEnabled, ninZa_MAType mAType, int mAPeriod, double sensitivityFactor, ninZaStepMAPro_StepSizeMode stepSizeMode, int stepSizeTicks, int stepSizeAutoLookback)
		{
			if (cacheninZaStepMAPro != null)
				for (int idx = 0; idx < cacheninZaStepMAPro.Length; idx++)
					if (cacheninZaStepMAPro[idx].BaseMode == baseMode && cacheninZaStepMAPro[idx].BaseHighLowEnabled == baseHighLowEnabled && cacheninZaStepMAPro[idx].MAType == mAType && cacheninZaStepMAPro[idx].MAPeriod == mAPeriod && cacheninZaStepMAPro[idx].SensitivityFactor == sensitivityFactor && cacheninZaStepMAPro[idx].StepSizeMode == stepSizeMode && cacheninZaStepMAPro[idx].StepSizeTicks == stepSizeTicks && cacheninZaStepMAPro[idx].StepSizeAutoLookback == stepSizeAutoLookback && cacheninZaStepMAPro[idx].EqualsInput(input))
						return cacheninZaStepMAPro[idx];
			return CacheIndicator<ninZaStepMAPro>(new ninZaStepMAPro(){ BaseMode = baseMode, BaseHighLowEnabled = baseHighLowEnabled, MAType = mAType, MAPeriod = mAPeriod, SensitivityFactor = sensitivityFactor, StepSizeMode = stepSizeMode, StepSizeTicks = stepSizeTicks, StepSizeAutoLookback = stepSizeAutoLookback }, input, ref cacheninZaStepMAPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaStepMAPro ninZaStepMAPro(ninZaStepMAPro_BaseMode baseMode, bool baseHighLowEnabled, ninZa_MAType mAType, int mAPeriod, double sensitivityFactor, ninZaStepMAPro_StepSizeMode stepSizeMode, int stepSizeTicks, int stepSizeAutoLookback)
		{
			return indicator.ninZaStepMAPro(Input, baseMode, baseHighLowEnabled, mAType, mAPeriod, sensitivityFactor, stepSizeMode, stepSizeTicks, stepSizeAutoLookback);
		}


		
		public Indicators.ninZaStepMAPro ninZaStepMAPro(ISeries<double> input , ninZaStepMAPro_BaseMode baseMode, bool baseHighLowEnabled, ninZa_MAType mAType, int mAPeriod, double sensitivityFactor, ninZaStepMAPro_StepSizeMode stepSizeMode, int stepSizeTicks, int stepSizeAutoLookback)
		{
			return indicator.ninZaStepMAPro(input, baseMode, baseHighLowEnabled, mAType, mAPeriod, sensitivityFactor, stepSizeMode, stepSizeTicks, stepSizeAutoLookback);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaStepMAPro ninZaStepMAPro(ninZaStepMAPro_BaseMode baseMode, bool baseHighLowEnabled, ninZa_MAType mAType, int mAPeriod, double sensitivityFactor, ninZaStepMAPro_StepSizeMode stepSizeMode, int stepSizeTicks, int stepSizeAutoLookback)
		{
			return indicator.ninZaStepMAPro(Input, baseMode, baseHighLowEnabled, mAType, mAPeriod, sensitivityFactor, stepSizeMode, stepSizeTicks, stepSizeAutoLookback);
		}


		
		public Indicators.ninZaStepMAPro ninZaStepMAPro(ISeries<double> input , ninZaStepMAPro_BaseMode baseMode, bool baseHighLowEnabled, ninZa_MAType mAType, int mAPeriod, double sensitivityFactor, ninZaStepMAPro_StepSizeMode stepSizeMode, int stepSizeTicks, int stepSizeAutoLookback)
		{
			return indicator.ninZaStepMAPro(input, baseMode, baseHighLowEnabled, mAType, mAPeriod, sensitivityFactor, stepSizeMode, stepSizeTicks, stepSizeAutoLookback);
		}

	}
}

#endregion
