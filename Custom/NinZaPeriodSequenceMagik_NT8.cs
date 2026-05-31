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
		
		private ninZaPeriodSequenceMagik[] cacheninZaPeriodSequenceMagik;

		
		public ninZaPeriodSequenceMagik ninZaPeriodSequenceMagik(int limitNumberOfMAs, int limitPeriodValue, int minimumConsensus, ninZa_MAType mAType, ninZaPeriodSequenceMagik_SequenceMode sequenceMode, int sequenceInitialPeriod, int sequenceStep, double sequenceMultiplier, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow, bool slowestMAFilterEnabled, double slowestMAFilterMultiplier, ninZaPeriodSequenceMagik_FilterUnit slowestMAFilterUnit, double priceMAOffset, int aTRPeriod)
		{
			return ninZaPeriodSequenceMagik(Input, limitNumberOfMAs, limitPeriodValue, minimumConsensus, mAType, sequenceMode, sequenceInitialPeriod, sequenceStep, sequenceMultiplier, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow, slowestMAFilterEnabled, slowestMAFilterMultiplier, slowestMAFilterUnit, priceMAOffset, aTRPeriod);
		}


		
		public ninZaPeriodSequenceMagik ninZaPeriodSequenceMagik(ISeries<double> input, int limitNumberOfMAs, int limitPeriodValue, int minimumConsensus, ninZa_MAType mAType, ninZaPeriodSequenceMagik_SequenceMode sequenceMode, int sequenceInitialPeriod, int sequenceStep, double sequenceMultiplier, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow, bool slowestMAFilterEnabled, double slowestMAFilterMultiplier, ninZaPeriodSequenceMagik_FilterUnit slowestMAFilterUnit, double priceMAOffset, int aTRPeriod)
		{
			if (cacheninZaPeriodSequenceMagik != null)
				for (int idx = 0; idx < cacheninZaPeriodSequenceMagik.Length; idx++)
					if (cacheninZaPeriodSequenceMagik[idx].LimitNumberOfMAs == limitNumberOfMAs && cacheninZaPeriodSequenceMagik[idx].LimitPeriodValue == limitPeriodValue && cacheninZaPeriodSequenceMagik[idx].MinimumConsensus == minimumConsensus && cacheninZaPeriodSequenceMagik[idx].MAType == mAType && cacheninZaPeriodSequenceMagik[idx].SequenceMode == sequenceMode && cacheninZaPeriodSequenceMagik[idx].SequenceInitialPeriod == sequenceInitialPeriod && cacheninZaPeriodSequenceMagik[idx].SequenceStep == sequenceStep && cacheninZaPeriodSequenceMagik[idx].SequenceMultiplier == sequenceMultiplier && cacheninZaPeriodSequenceMagik[idx].SmoothingEnabled == smoothingEnabled && cacheninZaPeriodSequenceMagik[idx].SmoothingMethod == smoothingMethod && cacheninZaPeriodSequenceMagik[idx].SmoothingPeriod == smoothingPeriod && cacheninZaPeriodSequenceMagik[idx].ThresholdHigh == thresholdHigh && cacheninZaPeriodSequenceMagik[idx].ThresholdLow == thresholdLow && cacheninZaPeriodSequenceMagik[idx].SlowestMAFilterEnabled == slowestMAFilterEnabled && cacheninZaPeriodSequenceMagik[idx].SlowestMAFilterMultiplier == slowestMAFilterMultiplier && cacheninZaPeriodSequenceMagik[idx].SlowestMAFilterUnit == slowestMAFilterUnit && cacheninZaPeriodSequenceMagik[idx].PriceMAOffset == priceMAOffset && cacheninZaPeriodSequenceMagik[idx].ATRPeriod == aTRPeriod && cacheninZaPeriodSequenceMagik[idx].EqualsInput(input))
						return cacheninZaPeriodSequenceMagik[idx];
			return CacheIndicator<ninZaPeriodSequenceMagik>(new ninZaPeriodSequenceMagik(){ LimitNumberOfMAs = limitNumberOfMAs, LimitPeriodValue = limitPeriodValue, MinimumConsensus = minimumConsensus, MAType = mAType, SequenceMode = sequenceMode, SequenceInitialPeriod = sequenceInitialPeriod, SequenceStep = sequenceStep, SequenceMultiplier = sequenceMultiplier, SmoothingEnabled = smoothingEnabled, SmoothingMethod = smoothingMethod, SmoothingPeriod = smoothingPeriod, ThresholdHigh = thresholdHigh, ThresholdLow = thresholdLow, SlowestMAFilterEnabled = slowestMAFilterEnabled, SlowestMAFilterMultiplier = slowestMAFilterMultiplier, SlowestMAFilterUnit = slowestMAFilterUnit, PriceMAOffset = priceMAOffset, ATRPeriod = aTRPeriod }, input, ref cacheninZaPeriodSequenceMagik);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaPeriodSequenceMagik ninZaPeriodSequenceMagik(int limitNumberOfMAs, int limitPeriodValue, int minimumConsensus, ninZa_MAType mAType, ninZaPeriodSequenceMagik_SequenceMode sequenceMode, int sequenceInitialPeriod, int sequenceStep, double sequenceMultiplier, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow, bool slowestMAFilterEnabled, double slowestMAFilterMultiplier, ninZaPeriodSequenceMagik_FilterUnit slowestMAFilterUnit, double priceMAOffset, int aTRPeriod)
		{
			return indicator.ninZaPeriodSequenceMagik(Input, limitNumberOfMAs, limitPeriodValue, minimumConsensus, mAType, sequenceMode, sequenceInitialPeriod, sequenceStep, sequenceMultiplier, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow, slowestMAFilterEnabled, slowestMAFilterMultiplier, slowestMAFilterUnit, priceMAOffset, aTRPeriod);
		}


		
		public Indicators.ninZaPeriodSequenceMagik ninZaPeriodSequenceMagik(ISeries<double> input , int limitNumberOfMAs, int limitPeriodValue, int minimumConsensus, ninZa_MAType mAType, ninZaPeriodSequenceMagik_SequenceMode sequenceMode, int sequenceInitialPeriod, int sequenceStep, double sequenceMultiplier, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow, bool slowestMAFilterEnabled, double slowestMAFilterMultiplier, ninZaPeriodSequenceMagik_FilterUnit slowestMAFilterUnit, double priceMAOffset, int aTRPeriod)
		{
			return indicator.ninZaPeriodSequenceMagik(input, limitNumberOfMAs, limitPeriodValue, minimumConsensus, mAType, sequenceMode, sequenceInitialPeriod, sequenceStep, sequenceMultiplier, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow, slowestMAFilterEnabled, slowestMAFilterMultiplier, slowestMAFilterUnit, priceMAOffset, aTRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaPeriodSequenceMagik ninZaPeriodSequenceMagik(int limitNumberOfMAs, int limitPeriodValue, int minimumConsensus, ninZa_MAType mAType, ninZaPeriodSequenceMagik_SequenceMode sequenceMode, int sequenceInitialPeriod, int sequenceStep, double sequenceMultiplier, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow, bool slowestMAFilterEnabled, double slowestMAFilterMultiplier, ninZaPeriodSequenceMagik_FilterUnit slowestMAFilterUnit, double priceMAOffset, int aTRPeriod)
		{
			return indicator.ninZaPeriodSequenceMagik(Input, limitNumberOfMAs, limitPeriodValue, minimumConsensus, mAType, sequenceMode, sequenceInitialPeriod, sequenceStep, sequenceMultiplier, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow, slowestMAFilterEnabled, slowestMAFilterMultiplier, slowestMAFilterUnit, priceMAOffset, aTRPeriod);
		}


		
		public Indicators.ninZaPeriodSequenceMagik ninZaPeriodSequenceMagik(ISeries<double> input , int limitNumberOfMAs, int limitPeriodValue, int minimumConsensus, ninZa_MAType mAType, ninZaPeriodSequenceMagik_SequenceMode sequenceMode, int sequenceInitialPeriod, int sequenceStep, double sequenceMultiplier, bool smoothingEnabled, ninZa_MAType smoothingMethod, int smoothingPeriod, double thresholdHigh, double thresholdLow, bool slowestMAFilterEnabled, double slowestMAFilterMultiplier, ninZaPeriodSequenceMagik_FilterUnit slowestMAFilterUnit, double priceMAOffset, int aTRPeriod)
		{
			return indicator.ninZaPeriodSequenceMagik(input, limitNumberOfMAs, limitPeriodValue, minimumConsensus, mAType, sequenceMode, sequenceInitialPeriod, sequenceStep, sequenceMultiplier, smoothingEnabled, smoothingMethod, smoothingPeriod, thresholdHigh, thresholdLow, slowestMAFilterEnabled, slowestMAFilterMultiplier, slowestMAFilterUnit, priceMAOffset, aTRPeriod);
		}

	}
}

#endregion
