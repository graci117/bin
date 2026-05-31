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
		
		private RenkoKings.RenkoKings_NobleCloud[] cacheRenkoKings_NobleCloud;

		
		public RenkoKings.RenkoKings_NobleCloud RenkoKings_NobleCloud(double sensitivity, int smoothness, ninZa_MAType baselineMAType, int baselinePeriod, bool baselineSmoothingEnabled, ninZa_MAType baselineSmoothingMethod, int baselineSmoothingPeriod, ninZa_MAType kernelMAType, int kernelPeriod, bool kernelSmoothingEnabled, ninZa_MAType kernelSmoothingMethod, int kernelSmoothingPeriod, int signalSplit, bool filterEnabled, int filterBarMin, int filterBarMax)
		{
			return RenkoKings_NobleCloud(Input, sensitivity, smoothness, baselineMAType, baselinePeriod, baselineSmoothingEnabled, baselineSmoothingMethod, baselineSmoothingPeriod, kernelMAType, kernelPeriod, kernelSmoothingEnabled, kernelSmoothingMethod, kernelSmoothingPeriod, signalSplit, filterEnabled, filterBarMin, filterBarMax);
		}


		
		public RenkoKings.RenkoKings_NobleCloud RenkoKings_NobleCloud(ISeries<double> input, double sensitivity, int smoothness, ninZa_MAType baselineMAType, int baselinePeriod, bool baselineSmoothingEnabled, ninZa_MAType baselineSmoothingMethod, int baselineSmoothingPeriod, ninZa_MAType kernelMAType, int kernelPeriod, bool kernelSmoothingEnabled, ninZa_MAType kernelSmoothingMethod, int kernelSmoothingPeriod, int signalSplit, bool filterEnabled, int filterBarMin, int filterBarMax)
		{
			if (cacheRenkoKings_NobleCloud != null)
				for (int idx = 0; idx < cacheRenkoKings_NobleCloud.Length; idx++)
					if (cacheRenkoKings_NobleCloud[idx].Sensitivity == sensitivity && cacheRenkoKings_NobleCloud[idx].Smoothness == smoothness && cacheRenkoKings_NobleCloud[idx].BaselineMAType == baselineMAType && cacheRenkoKings_NobleCloud[idx].BaselinePeriod == baselinePeriod && cacheRenkoKings_NobleCloud[idx].BaselineSmoothingEnabled == baselineSmoothingEnabled && cacheRenkoKings_NobleCloud[idx].BaselineSmoothingMethod == baselineSmoothingMethod && cacheRenkoKings_NobleCloud[idx].BaselineSmoothingPeriod == baselineSmoothingPeriod && cacheRenkoKings_NobleCloud[idx].KernelMAType == kernelMAType && cacheRenkoKings_NobleCloud[idx].KernelPeriod == kernelPeriod && cacheRenkoKings_NobleCloud[idx].KernelSmoothingEnabled == kernelSmoothingEnabled && cacheRenkoKings_NobleCloud[idx].KernelSmoothingMethod == kernelSmoothingMethod && cacheRenkoKings_NobleCloud[idx].KernelSmoothingPeriod == kernelSmoothingPeriod && cacheRenkoKings_NobleCloud[idx].SignalSplit == signalSplit && cacheRenkoKings_NobleCloud[idx].FilterEnabled == filterEnabled && cacheRenkoKings_NobleCloud[idx].FilterBarMin == filterBarMin && cacheRenkoKings_NobleCloud[idx].FilterBarMax == filterBarMax && cacheRenkoKings_NobleCloud[idx].EqualsInput(input))
						return cacheRenkoKings_NobleCloud[idx];
			return CacheIndicator<RenkoKings.RenkoKings_NobleCloud>(new RenkoKings.RenkoKings_NobleCloud(){ Sensitivity = sensitivity, Smoothness = smoothness, BaselineMAType = baselineMAType, BaselinePeriod = baselinePeriod, BaselineSmoothingEnabled = baselineSmoothingEnabled, BaselineSmoothingMethod = baselineSmoothingMethod, BaselineSmoothingPeriod = baselineSmoothingPeriod, KernelMAType = kernelMAType, KernelPeriod = kernelPeriod, KernelSmoothingEnabled = kernelSmoothingEnabled, KernelSmoothingMethod = kernelSmoothingMethod, KernelSmoothingPeriod = kernelSmoothingPeriod, SignalSplit = signalSplit, FilterEnabled = filterEnabled, FilterBarMin = filterBarMin, FilterBarMax = filterBarMax }, input, ref cacheRenkoKings_NobleCloud);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.RenkoKings.RenkoKings_NobleCloud RenkoKings_NobleCloud(double sensitivity, int smoothness, ninZa_MAType baselineMAType, int baselinePeriod, bool baselineSmoothingEnabled, ninZa_MAType baselineSmoothingMethod, int baselineSmoothingPeriod, ninZa_MAType kernelMAType, int kernelPeriod, bool kernelSmoothingEnabled, ninZa_MAType kernelSmoothingMethod, int kernelSmoothingPeriod, int signalSplit, bool filterEnabled, int filterBarMin, int filterBarMax)
		{
			return indicator.RenkoKings_NobleCloud(Input, sensitivity, smoothness, baselineMAType, baselinePeriod, baselineSmoothingEnabled, baselineSmoothingMethod, baselineSmoothingPeriod, kernelMAType, kernelPeriod, kernelSmoothingEnabled, kernelSmoothingMethod, kernelSmoothingPeriod, signalSplit, filterEnabled, filterBarMin, filterBarMax);
		}


		
		public Indicators.RenkoKings.RenkoKings_NobleCloud RenkoKings_NobleCloud(ISeries<double> input , double sensitivity, int smoothness, ninZa_MAType baselineMAType, int baselinePeriod, bool baselineSmoothingEnabled, ninZa_MAType baselineSmoothingMethod, int baselineSmoothingPeriod, ninZa_MAType kernelMAType, int kernelPeriod, bool kernelSmoothingEnabled, ninZa_MAType kernelSmoothingMethod, int kernelSmoothingPeriod, int signalSplit, bool filterEnabled, int filterBarMin, int filterBarMax)
		{
			return indicator.RenkoKings_NobleCloud(input, sensitivity, smoothness, baselineMAType, baselinePeriod, baselineSmoothingEnabled, baselineSmoothingMethod, baselineSmoothingPeriod, kernelMAType, kernelPeriod, kernelSmoothingEnabled, kernelSmoothingMethod, kernelSmoothingPeriod, signalSplit, filterEnabled, filterBarMin, filterBarMax);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.RenkoKings.RenkoKings_NobleCloud RenkoKings_NobleCloud(double sensitivity, int smoothness, ninZa_MAType baselineMAType, int baselinePeriod, bool baselineSmoothingEnabled, ninZa_MAType baselineSmoothingMethod, int baselineSmoothingPeriod, ninZa_MAType kernelMAType, int kernelPeriod, bool kernelSmoothingEnabled, ninZa_MAType kernelSmoothingMethod, int kernelSmoothingPeriod, int signalSplit, bool filterEnabled, int filterBarMin, int filterBarMax)
		{
			return indicator.RenkoKings_NobleCloud(Input, sensitivity, smoothness, baselineMAType, baselinePeriod, baselineSmoothingEnabled, baselineSmoothingMethod, baselineSmoothingPeriod, kernelMAType, kernelPeriod, kernelSmoothingEnabled, kernelSmoothingMethod, kernelSmoothingPeriod, signalSplit, filterEnabled, filterBarMin, filterBarMax);
		}


		
		public Indicators.RenkoKings.RenkoKings_NobleCloud RenkoKings_NobleCloud(ISeries<double> input , double sensitivity, int smoothness, ninZa_MAType baselineMAType, int baselinePeriod, bool baselineSmoothingEnabled, ninZa_MAType baselineSmoothingMethod, int baselineSmoothingPeriod, ninZa_MAType kernelMAType, int kernelPeriod, bool kernelSmoothingEnabled, ninZa_MAType kernelSmoothingMethod, int kernelSmoothingPeriod, int signalSplit, bool filterEnabled, int filterBarMin, int filterBarMax)
		{
			return indicator.RenkoKings_NobleCloud(input, sensitivity, smoothness, baselineMAType, baselinePeriod, baselineSmoothingEnabled, baselineSmoothingMethod, baselineSmoothingPeriod, kernelMAType, kernelPeriod, kernelSmoothingEnabled, kernelSmoothingMethod, kernelSmoothingPeriod, signalSplit, filterEnabled, filterBarMin, filterBarMax);
		}

	}
}

#endregion
