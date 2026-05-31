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
	public class ChandesTrendScore : Indicator
	{
		private double score = 0.0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"TrendScore was described by Tushar Chande in the September 1993 edition of 'Stocks & Commodities' magazine.The TrendScore attempts to make a quantative and qualiative determination of the direction and strength of a market trend by comparing the current close price to previous close prices over the last 20 periods. Interpretation: The TrendScore indicator oscillates between +1 and -1. A +1 reading indicates a strong uptrend is underway. A -1 reading indicates a strong downtrend is underway. Readings above 0 suggest an upward trend of varying strength and readings below 0 suggest a downward trend of varying strength.";
				Name										= "ChandesTrendScore";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				LookBack									= 20;
				LookBackLength								= 20;
				AddPlot(new Stroke(Brushes.CornflowerBlue, 2), PlotStyle.Line, "TrendScore");
				AddLine(Brushes.DimGray, 0, "Zero");
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar<(LookBack+LookBackLength)) 
				return;
			
			score = 0;  // reset to 0 for new bar
			
			for (int k = 0; k < LookBackLength; k++)
				{
					if (Close[0] >= Close[k+LookBack])
					{
						score++; 
					}
					else 
					{
						score--;
					}
				}
			
            TrendScore[0] = (score/LookBackLength);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LookBack", Order=1, GroupName="Parameters")]
		public int LookBack
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LookBackLength", Order=2, GroupName="Parameters")]
		public int LookBackLength
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrendScore
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
		private ChandesTrendScore[] cacheChandesTrendScore;
		public ChandesTrendScore ChandesTrendScore(int lookBack, int lookBackLength)
		{
			return ChandesTrendScore(Input, lookBack, lookBackLength);
		}

		public ChandesTrendScore ChandesTrendScore(ISeries<double> input, int lookBack, int lookBackLength)
		{
			if (cacheChandesTrendScore != null)
				for (int idx = 0; idx < cacheChandesTrendScore.Length; idx++)
					if (cacheChandesTrendScore[idx] != null && cacheChandesTrendScore[idx].LookBack == lookBack && cacheChandesTrendScore[idx].LookBackLength == lookBackLength && cacheChandesTrendScore[idx].EqualsInput(input))
						return cacheChandesTrendScore[idx];
			return CacheIndicator<ChandesTrendScore>(new ChandesTrendScore(){ LookBack = lookBack, LookBackLength = lookBackLength }, input, ref cacheChandesTrendScore);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChandesTrendScore ChandesTrendScore(int lookBack, int lookBackLength)
		{
			return indicator.ChandesTrendScore(Input, lookBack, lookBackLength);
		}

		public Indicators.ChandesTrendScore ChandesTrendScore(ISeries<double> input , int lookBack, int lookBackLength)
		{
			return indicator.ChandesTrendScore(input, lookBack, lookBackLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChandesTrendScore ChandesTrendScore(int lookBack, int lookBackLength)
		{
			return indicator.ChandesTrendScore(Input, lookBack, lookBackLength);
		}

		public Indicators.ChandesTrendScore ChandesTrendScore(ISeries<double> input , int lookBack, int lookBackLength)
		{
			return indicator.ChandesTrendScore(input, lookBack, lookBackLength);
		}
	}
}

#endregion
