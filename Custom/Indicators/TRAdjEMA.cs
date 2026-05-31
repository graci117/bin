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
	public class TRAdjEMA : Indicator
	{
		private double tH, tL, tRAdj, mltp1, mltp2, rate;

		private Series<double> tR;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"The true range adjusted exponential moving average (TRAdj EMA) is designed to account for true range.";
				Name								= "TRAdjEMA";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				
				Period								= 40;
				Pds									= 40;
				Multiplier							= 10;

				AddPlot(Brushes.Blue, "TRAdjEMA");
			}
			else if (State == State.DataLoaded)
			{
				tR			= new Series<double>(this, MaximumBarsLookBack.Infinite);
				mltp1		= 2 / (double)(Period + 1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			
			tH		= (Close[1] > High[0]) ? Close[1] : High[0];
			tL		= (Close[1] < Low[0]) ? Close[1] : Low[0];

			tR[0]	= Math.Abs(tH - tL);
			tRAdj	= (tR[0] - MIN(tR, Pds)[0]) / (MAX(tR, Pds)[0] - MIN(tR, Pds)[0]);
			mltp2	= tRAdj * Multiplier;
			rate	= mltp1 * (1 + mltp2);	

			if (CurrentBar > Period + 1)
				Value[0] = Value[1] + (rate * (Close[0] - Value[1]));
			else
				Value[0] = Close[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Periods", Order = 1, GroupName = "Parameters")]
		public int Pds
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name = "Multiplier", Order = 1, GroupName = "Parameters")]
		public int Multiplier
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TRAdjEMAPlot
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TRAdjEMA[] cacheTRAdjEMA;
		public TRAdjEMA TRAdjEMA(int period, int pds, int multiplier)
		{
			return TRAdjEMA(Input, period, pds, multiplier);
		}

		public TRAdjEMA TRAdjEMA(ISeries<double> input, int period, int pds, int multiplier)
		{
			if (cacheTRAdjEMA != null)
				for (int idx = 0; idx < cacheTRAdjEMA.Length; idx++)
					if (cacheTRAdjEMA[idx] != null && cacheTRAdjEMA[idx].Period == period && cacheTRAdjEMA[idx].Pds == pds && cacheTRAdjEMA[idx].Multiplier == multiplier && cacheTRAdjEMA[idx].EqualsInput(input))
						return cacheTRAdjEMA[idx];
			return CacheIndicator<TRAdjEMA>(new TRAdjEMA(){ Period = period, Pds = pds, Multiplier = multiplier }, input, ref cacheTRAdjEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TRAdjEMA TRAdjEMA(int period, int pds, int multiplier)
		{
			return indicator.TRAdjEMA(Input, period, pds, multiplier);
		}

		public Indicators.TRAdjEMA TRAdjEMA(ISeries<double> input , int period, int pds, int multiplier)
		{
			return indicator.TRAdjEMA(input, period, pds, multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TRAdjEMA TRAdjEMA(int period, int pds, int multiplier)
		{
			return indicator.TRAdjEMA(Input, period, pds, multiplier);
		}

		public Indicators.TRAdjEMA TRAdjEMA(ISeries<double> input , int period, int pds, int multiplier)
		{
			return indicator.TRAdjEMA(input, period, pds, multiplier);
		}
	}
}

#endregion
