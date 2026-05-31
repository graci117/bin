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
		
		private ninZaPinocchioBarPro[] cacheninZaPinocchioBarPro;

		
		public ninZaPinocchioBarPro ninZaPinocchioBarPro(int neutralRangePercentage, bool minimumPinBarEnabled, double minimumPinBarMultiplier, ninZaPinocchioBarPro_Unit minimumPinBarUnit, int minimumPinBarATRPeriod)
		{
			return ninZaPinocchioBarPro(Input, neutralRangePercentage, minimumPinBarEnabled, minimumPinBarMultiplier, minimumPinBarUnit, minimumPinBarATRPeriod);
		}


		
		public ninZaPinocchioBarPro ninZaPinocchioBarPro(ISeries<double> input, int neutralRangePercentage, bool minimumPinBarEnabled, double minimumPinBarMultiplier, ninZaPinocchioBarPro_Unit minimumPinBarUnit, int minimumPinBarATRPeriod)
		{
			if (cacheninZaPinocchioBarPro != null)
				for (int idx = 0; idx < cacheninZaPinocchioBarPro.Length; idx++)
					if (cacheninZaPinocchioBarPro[idx].NeutralRangePercentage == neutralRangePercentage && cacheninZaPinocchioBarPro[idx].MinimumPinBarEnabled == minimumPinBarEnabled && cacheninZaPinocchioBarPro[idx].MinimumPinBarMultiplier == minimumPinBarMultiplier && cacheninZaPinocchioBarPro[idx].MinimumPinBarUnit == minimumPinBarUnit && cacheninZaPinocchioBarPro[idx].MinimumPinBarATRPeriod == minimumPinBarATRPeriod && cacheninZaPinocchioBarPro[idx].EqualsInput(input))
						return cacheninZaPinocchioBarPro[idx];
			return CacheIndicator<ninZaPinocchioBarPro>(new ninZaPinocchioBarPro(){ NeutralRangePercentage = neutralRangePercentage, MinimumPinBarEnabled = minimumPinBarEnabled, MinimumPinBarMultiplier = minimumPinBarMultiplier, MinimumPinBarUnit = minimumPinBarUnit, MinimumPinBarATRPeriod = minimumPinBarATRPeriod }, input, ref cacheninZaPinocchioBarPro);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaPinocchioBarPro ninZaPinocchioBarPro(int neutralRangePercentage, bool minimumPinBarEnabled, double minimumPinBarMultiplier, ninZaPinocchioBarPro_Unit minimumPinBarUnit, int minimumPinBarATRPeriod)
		{
			return indicator.ninZaPinocchioBarPro(Input, neutralRangePercentage, minimumPinBarEnabled, minimumPinBarMultiplier, minimumPinBarUnit, minimumPinBarATRPeriod);
		}


		
		public Indicators.ninZaPinocchioBarPro ninZaPinocchioBarPro(ISeries<double> input , int neutralRangePercentage, bool minimumPinBarEnabled, double minimumPinBarMultiplier, ninZaPinocchioBarPro_Unit minimumPinBarUnit, int minimumPinBarATRPeriod)
		{
			return indicator.ninZaPinocchioBarPro(input, neutralRangePercentage, minimumPinBarEnabled, minimumPinBarMultiplier, minimumPinBarUnit, minimumPinBarATRPeriod);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaPinocchioBarPro ninZaPinocchioBarPro(int neutralRangePercentage, bool minimumPinBarEnabled, double minimumPinBarMultiplier, ninZaPinocchioBarPro_Unit minimumPinBarUnit, int minimumPinBarATRPeriod)
		{
			return indicator.ninZaPinocchioBarPro(Input, neutralRangePercentage, minimumPinBarEnabled, minimumPinBarMultiplier, minimumPinBarUnit, minimumPinBarATRPeriod);
		}


		
		public Indicators.ninZaPinocchioBarPro ninZaPinocchioBarPro(ISeries<double> input , int neutralRangePercentage, bool minimumPinBarEnabled, double minimumPinBarMultiplier, ninZaPinocchioBarPro_Unit minimumPinBarUnit, int minimumPinBarATRPeriod)
		{
			return indicator.ninZaPinocchioBarPro(input, neutralRangePercentage, minimumPinBarEnabled, minimumPinBarMultiplier, minimumPinBarUnit, minimumPinBarATRPeriod);
		}

	}
}

#endregion
