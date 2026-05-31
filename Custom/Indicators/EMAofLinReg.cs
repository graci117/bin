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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
    public class EMAofLinReg : Indicator
    {
        private EMA ema;
        private LinReg linReg;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Plots a 34 EMA of a 27-period Linear Regression (of Close).";
                Name = "EMA of LinReg";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                IsSuspendedWhileInactive = true;
                
                EmaPeriod = 34;
                LinRegPeriod = 27;
                
                AddPlot(Brushes.DeepSkyBlue, "EMAofLinReg");
            }
            else if (State == State.DataLoaded)
            {
                linReg = LinReg(Close, LinRegPeriod);
                ema = EMA(linReg, EmaPeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(EmaPeriod, LinRegPeriod))
                return;

            Value[0] = ema[0];
        }

        #region Properties
        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "EMA Period", Order = 1, GroupName = "Parameters")]
        public int EmaPeriod { get; set; }

        [Range(1, int.MaxValue)]
        [NinjaScriptProperty]
        [Display(Name = "LinReg Period", Order = 2, GroupName = "Parameters")]
        public int LinRegPeriod { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EMAofLinReg[] cacheEMAofLinReg;
		public EMAofLinReg EMAofLinReg(int emaPeriod, int linRegPeriod)
		{
			return EMAofLinReg(Input, emaPeriod, linRegPeriod);
		}

		public EMAofLinReg EMAofLinReg(ISeries<double> input, int emaPeriod, int linRegPeriod)
		{
			if (cacheEMAofLinReg != null)
				for (int idx = 0; idx < cacheEMAofLinReg.Length; idx++)
					if (cacheEMAofLinReg[idx] != null && cacheEMAofLinReg[idx].EmaPeriod == emaPeriod && cacheEMAofLinReg[idx].LinRegPeriod == linRegPeriod && cacheEMAofLinReg[idx].EqualsInput(input))
						return cacheEMAofLinReg[idx];
			return CacheIndicator<EMAofLinReg>(new EMAofLinReg(){ EmaPeriod = emaPeriod, LinRegPeriod = linRegPeriod }, input, ref cacheEMAofLinReg);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EMAofLinReg EMAofLinReg(int emaPeriod, int linRegPeriod)
		{
			return indicator.EMAofLinReg(Input, emaPeriod, linRegPeriod);
		}

		public Indicators.EMAofLinReg EMAofLinReg(ISeries<double> input , int emaPeriod, int linRegPeriod)
		{
			return indicator.EMAofLinReg(input, emaPeriod, linRegPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EMAofLinReg EMAofLinReg(int emaPeriod, int linRegPeriod)
		{
			return indicator.EMAofLinReg(Input, emaPeriod, linRegPeriod);
		}

		public Indicators.EMAofLinReg EMAofLinReg(ISeries<double> input , int emaPeriod, int linRegPeriod)
		{
			return indicator.EMAofLinReg(input, emaPeriod, linRegPeriod);
		}
	}
}

#endregion
