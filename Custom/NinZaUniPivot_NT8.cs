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
		
		private ninZaUniPivot[] cacheninZaUniPivot;

		
		public ninZaUniPivot ninZaUniPivot(ninZaPivotType pivotType, double classicFactorS2R2, double classicFactorS3R3, double classicFactorS4R4, double fibonacciFactorS1R1, double fibonacciFactorS2R2, double fibonacciFactorS3R3, double fibonacciFactorS4R4, double camarillaDividend, double camarillaDivisorS1R1, double camarillaDivisorS2R2, double camarillaDivisorS3R3, double camarillaDivisorS4R4, double deMARKFactorS1R1, double deMARKFactorS2R2, double deMARKFactorS3R3, double deMARKFactorS4R4)
		{
			return ninZaUniPivot(Input, pivotType, classicFactorS2R2, classicFactorS3R3, classicFactorS4R4, fibonacciFactorS1R1, fibonacciFactorS2R2, fibonacciFactorS3R3, fibonacciFactorS4R4, camarillaDividend, camarillaDivisorS1R1, camarillaDivisorS2R2, camarillaDivisorS3R3, camarillaDivisorS4R4, deMARKFactorS1R1, deMARKFactorS2R2, deMARKFactorS3R3, deMARKFactorS4R4);
		}


		
		public ninZaUniPivot ninZaUniPivot(ISeries<double> input, ninZaPivotType pivotType, double classicFactorS2R2, double classicFactorS3R3, double classicFactorS4R4, double fibonacciFactorS1R1, double fibonacciFactorS2R2, double fibonacciFactorS3R3, double fibonacciFactorS4R4, double camarillaDividend, double camarillaDivisorS1R1, double camarillaDivisorS2R2, double camarillaDivisorS3R3, double camarillaDivisorS4R4, double deMARKFactorS1R1, double deMARKFactorS2R2, double deMARKFactorS3R3, double deMARKFactorS4R4)
		{
			if (cacheninZaUniPivot != null)
				for (int idx = 0; idx < cacheninZaUniPivot.Length; idx++)
					if (cacheninZaUniPivot[idx].PivotType == pivotType && cacheninZaUniPivot[idx].ClassicFactorS2R2 == classicFactorS2R2 && cacheninZaUniPivot[idx].ClassicFactorS3R3 == classicFactorS3R3 && cacheninZaUniPivot[idx].ClassicFactorS4R4 == classicFactorS4R4 && cacheninZaUniPivot[idx].FibonacciFactorS1R1 == fibonacciFactorS1R1 && cacheninZaUniPivot[idx].FibonacciFactorS2R2 == fibonacciFactorS2R2 && cacheninZaUniPivot[idx].FibonacciFactorS3R3 == fibonacciFactorS3R3 && cacheninZaUniPivot[idx].FibonacciFactorS4R4 == fibonacciFactorS4R4 && cacheninZaUniPivot[idx].CamarillaDividend == camarillaDividend && cacheninZaUniPivot[idx].CamarillaDivisorS1R1 == camarillaDivisorS1R1 && cacheninZaUniPivot[idx].CamarillaDivisorS2R2 == camarillaDivisorS2R2 && cacheninZaUniPivot[idx].CamarillaDivisorS3R3 == camarillaDivisorS3R3 && cacheninZaUniPivot[idx].CamarillaDivisorS4R4 == camarillaDivisorS4R4 && cacheninZaUniPivot[idx].DeMARKFactorS1R1 == deMARKFactorS1R1 && cacheninZaUniPivot[idx].DeMARKFactorS2R2 == deMARKFactorS2R2 && cacheninZaUniPivot[idx].DeMARKFactorS3R3 == deMARKFactorS3R3 && cacheninZaUniPivot[idx].DeMARKFactorS4R4 == deMARKFactorS4R4 && cacheninZaUniPivot[idx].EqualsInput(input))
						return cacheninZaUniPivot[idx];
			return CacheIndicator<ninZaUniPivot>(new ninZaUniPivot(){ PivotType = pivotType, ClassicFactorS2R2 = classicFactorS2R2, ClassicFactorS3R3 = classicFactorS3R3, ClassicFactorS4R4 = classicFactorS4R4, FibonacciFactorS1R1 = fibonacciFactorS1R1, FibonacciFactorS2R2 = fibonacciFactorS2R2, FibonacciFactorS3R3 = fibonacciFactorS3R3, FibonacciFactorS4R4 = fibonacciFactorS4R4, CamarillaDividend = camarillaDividend, CamarillaDivisorS1R1 = camarillaDivisorS1R1, CamarillaDivisorS2R2 = camarillaDivisorS2R2, CamarillaDivisorS3R3 = camarillaDivisorS3R3, CamarillaDivisorS4R4 = camarillaDivisorS4R4, DeMARKFactorS1R1 = deMARKFactorS1R1, DeMARKFactorS2R2 = deMARKFactorS2R2, DeMARKFactorS3R3 = deMARKFactorS3R3, DeMARKFactorS4R4 = deMARKFactorS4R4 }, input, ref cacheninZaUniPivot);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.ninZaUniPivot ninZaUniPivot(ninZaPivotType pivotType, double classicFactorS2R2, double classicFactorS3R3, double classicFactorS4R4, double fibonacciFactorS1R1, double fibonacciFactorS2R2, double fibonacciFactorS3R3, double fibonacciFactorS4R4, double camarillaDividend, double camarillaDivisorS1R1, double camarillaDivisorS2R2, double camarillaDivisorS3R3, double camarillaDivisorS4R4, double deMARKFactorS1R1, double deMARKFactorS2R2, double deMARKFactorS3R3, double deMARKFactorS4R4)
		{
			return indicator.ninZaUniPivot(Input, pivotType, classicFactorS2R2, classicFactorS3R3, classicFactorS4R4, fibonacciFactorS1R1, fibonacciFactorS2R2, fibonacciFactorS3R3, fibonacciFactorS4R4, camarillaDividend, camarillaDivisorS1R1, camarillaDivisorS2R2, camarillaDivisorS3R3, camarillaDivisorS4R4, deMARKFactorS1R1, deMARKFactorS2R2, deMARKFactorS3R3, deMARKFactorS4R4);
		}


		
		public Indicators.ninZaUniPivot ninZaUniPivot(ISeries<double> input , ninZaPivotType pivotType, double classicFactorS2R2, double classicFactorS3R3, double classicFactorS4R4, double fibonacciFactorS1R1, double fibonacciFactorS2R2, double fibonacciFactorS3R3, double fibonacciFactorS4R4, double camarillaDividend, double camarillaDivisorS1R1, double camarillaDivisorS2R2, double camarillaDivisorS3R3, double camarillaDivisorS4R4, double deMARKFactorS1R1, double deMARKFactorS2R2, double deMARKFactorS3R3, double deMARKFactorS4R4)
		{
			return indicator.ninZaUniPivot(input, pivotType, classicFactorS2R2, classicFactorS3R3, classicFactorS4R4, fibonacciFactorS1R1, fibonacciFactorS2R2, fibonacciFactorS3R3, fibonacciFactorS4R4, camarillaDividend, camarillaDivisorS1R1, camarillaDivisorS2R2, camarillaDivisorS3R3, camarillaDivisorS4R4, deMARKFactorS1R1, deMARKFactorS2R2, deMARKFactorS3R3, deMARKFactorS4R4);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.ninZaUniPivot ninZaUniPivot(ninZaPivotType pivotType, double classicFactorS2R2, double classicFactorS3R3, double classicFactorS4R4, double fibonacciFactorS1R1, double fibonacciFactorS2R2, double fibonacciFactorS3R3, double fibonacciFactorS4R4, double camarillaDividend, double camarillaDivisorS1R1, double camarillaDivisorS2R2, double camarillaDivisorS3R3, double camarillaDivisorS4R4, double deMARKFactorS1R1, double deMARKFactorS2R2, double deMARKFactorS3R3, double deMARKFactorS4R4)
		{
			return indicator.ninZaUniPivot(Input, pivotType, classicFactorS2R2, classicFactorS3R3, classicFactorS4R4, fibonacciFactorS1R1, fibonacciFactorS2R2, fibonacciFactorS3R3, fibonacciFactorS4R4, camarillaDividend, camarillaDivisorS1R1, camarillaDivisorS2R2, camarillaDivisorS3R3, camarillaDivisorS4R4, deMARKFactorS1R1, deMARKFactorS2R2, deMARKFactorS3R3, deMARKFactorS4R4);
		}


		
		public Indicators.ninZaUniPivot ninZaUniPivot(ISeries<double> input , ninZaPivotType pivotType, double classicFactorS2R2, double classicFactorS3R3, double classicFactorS4R4, double fibonacciFactorS1R1, double fibonacciFactorS2R2, double fibonacciFactorS3R3, double fibonacciFactorS4R4, double camarillaDividend, double camarillaDivisorS1R1, double camarillaDivisorS2R2, double camarillaDivisorS3R3, double camarillaDivisorS4R4, double deMARKFactorS1R1, double deMARKFactorS2R2, double deMARKFactorS3R3, double deMARKFactorS4R4)
		{
			return indicator.ninZaUniPivot(input, pivotType, classicFactorS2R2, classicFactorS3R3, classicFactorS4R4, fibonacciFactorS1R1, fibonacciFactorS2R2, fibonacciFactorS3R3, fibonacciFactorS4R4, camarillaDividend, camarillaDivisorS1R1, camarillaDivisorS2R2, camarillaDivisorS3R3, camarillaDivisorS4R4, deMARKFactorS1R1, deMARKFactorS2R2, deMARKFactorS3R3, deMARKFactorS4R4);
		}

	}
}

#endregion
