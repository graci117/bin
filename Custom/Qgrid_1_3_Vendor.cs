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
		
		private iGRID_EVO[] cacheiGRID_EVO;

		
		public iGRID_EVO iGRID_EVO(int hASmoothPeriod1, int omaLength, double omaSpeed, bool omaAdaptive, double sensitivity, double stepSize, int hASmoothPeriod2)
		{
			return iGRID_EVO(Input, hASmoothPeriod1, omaLength, omaSpeed, omaAdaptive, sensitivity, stepSize, hASmoothPeriod2);
		}


		
		public iGRID_EVO iGRID_EVO(ISeries<double> input, int hASmoothPeriod1, int omaLength, double omaSpeed, bool omaAdaptive, double sensitivity, double stepSize, int hASmoothPeriod2)
		{
			if (cacheiGRID_EVO != null)
				for (int idx = 0; idx < cacheiGRID_EVO.Length; idx++)
					if (cacheiGRID_EVO[idx].HASmoothPeriod1 == hASmoothPeriod1 && cacheiGRID_EVO[idx].OmaLength == omaLength && cacheiGRID_EVO[idx].OmaSpeed == omaSpeed && cacheiGRID_EVO[idx].OmaAdaptive == omaAdaptive && cacheiGRID_EVO[idx].Sensitivity == sensitivity && cacheiGRID_EVO[idx].StepSize == stepSize && cacheiGRID_EVO[idx].HASmoothPeriod2 == hASmoothPeriod2 && cacheiGRID_EVO[idx].EqualsInput(input))
						return cacheiGRID_EVO[idx];
			return CacheIndicator<iGRID_EVO>(new iGRID_EVO(){ HASmoothPeriod1 = hASmoothPeriod1, OmaLength = omaLength, OmaSpeed = omaSpeed, OmaAdaptive = omaAdaptive, Sensitivity = sensitivity, StepSize = stepSize, HASmoothPeriod2 = hASmoothPeriod2 }, input, ref cacheiGRID_EVO);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.iGRID_EVO iGRID_EVO(int hASmoothPeriod1, int omaLength, double omaSpeed, bool omaAdaptive, double sensitivity, double stepSize, int hASmoothPeriod2)
		{
			return indicator.iGRID_EVO(Input, hASmoothPeriod1, omaLength, omaSpeed, omaAdaptive, sensitivity, stepSize, hASmoothPeriod2);
		}


		
		public Indicators.iGRID_EVO iGRID_EVO(ISeries<double> input , int hASmoothPeriod1, int omaLength, double omaSpeed, bool omaAdaptive, double sensitivity, double stepSize, int hASmoothPeriod2)
		{
			return indicator.iGRID_EVO(input, hASmoothPeriod1, omaLength, omaSpeed, omaAdaptive, sensitivity, stepSize, hASmoothPeriod2);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.iGRID_EVO iGRID_EVO(int hASmoothPeriod1, int omaLength, double omaSpeed, bool omaAdaptive, double sensitivity, double stepSize, int hASmoothPeriod2)
		{
			return indicator.iGRID_EVO(Input, hASmoothPeriod1, omaLength, omaSpeed, omaAdaptive, sensitivity, stepSize, hASmoothPeriod2);
		}


		
		public Indicators.iGRID_EVO iGRID_EVO(ISeries<double> input , int hASmoothPeriod1, int omaLength, double omaSpeed, bool omaAdaptive, double sensitivity, double stepSize, int hASmoothPeriod2)
		{
			return indicator.iGRID_EVO(input, hASmoothPeriod1, omaLength, omaSpeed, omaAdaptive, sensitivity, stepSize, hASmoothPeriod2);
		}

	}
}

#endregion
