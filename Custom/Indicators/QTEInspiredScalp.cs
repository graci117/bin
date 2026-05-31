//
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// This indicator returns 1 when we have n of consecutive bars up, otherwise returns 0.
	/// An up bar is defined as a bar where the close is above the open and the bars makes a higher
	/// high and a higher low. You can adjust the specific requirements with the indicator options.
	/// </summary>
	public class QTEInspiredScalp : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionNBarsUp;
				Name						= "QTEInspiredScalp";
				BarCount					= 3;
				BarUp						= true;
				HigherHigh					= true;
				HigherLow					= true;
				RedCandle					= true;
				BelowEMA					= true;
				ShowShort					= false;
				LowerHigh					= true;
				LowerLow					= true;
				GreenCandle					= true;
				AboveEMA					= true;
				ShowLong					= false;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;

				AddPlot(new Stroke(Brushes.DarkCyan, 2), PlotStyle.Bar, NinjaTrader.Custom.Resource.NinjaScriptIndicatorDiff);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarCount)
			{
				Value[0] = 0;
			}
			else
			{
				//bool gotBars = false;

				HigherHigh = High[0] > High[1];
				HigherLow = Low[0] > Low[1];
				RedCandle = Close[0] < Open[0];
				//BelowEMA = close < ema10;
				ShowShort = HigherHigh && HigherLow && RedCandle;
				
				
				LowerHigh = High[0] < High[1];
				LowerLow = Low[0] < Low[1];
				GreenCandle = Close[0] > Open[0];
				//BelowEMA = close < ema10;
				ShowLong = LowerHigh && LowerLow && GreenCandle;

				if (ShowLong)
					Draw.ArrowUp (this, "UpArrow"+CurrentBar, true, 0, Low[0] - 3 * TickSize, Brushes.Blue);
				if (ShowShort)
					Draw.ArrowDown (this, "DwnArrow"+CurrentBar, true, 0,  High[0] + 3 * TickSize, Brushes.Orange);
			}
		}


		#region Properties
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarCount", GroupName = "NinjaScriptParameters", Order = 0)]
		public int BarCount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "BarUp", GroupName = "NinjaScriptParameters", Order = 1)]
		public bool BarUp
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public bool HigherHigh
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public bool HigherLow
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public bool RedCandle
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public bool BelowEMA
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public bool ShowLong
		{ get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore]
		public bool LowerHigh
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public bool LowerLow
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public bool GreenCandle
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public bool AboveEMA
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public bool ShowShort
		{ get; set; }
		
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private QTEInspiredScalp[] cacheQTEInspiredScalp;
		public QTEInspiredScalp QTEInspiredScalp(int barCount, bool barUp)
		{
			return QTEInspiredScalp(Input, barCount, barUp);
		}

		public QTEInspiredScalp QTEInspiredScalp(ISeries<double> input, int barCount, bool barUp)
		{
			if (cacheQTEInspiredScalp != null)
				for (int idx = 0; idx < cacheQTEInspiredScalp.Length; idx++)
					if (cacheQTEInspiredScalp[idx] != null && cacheQTEInspiredScalp[idx].BarCount == barCount && cacheQTEInspiredScalp[idx].BarUp == barUp && cacheQTEInspiredScalp[idx].EqualsInput(input))
						return cacheQTEInspiredScalp[idx];
			return CacheIndicator<QTEInspiredScalp>(new QTEInspiredScalp(){ BarCount = barCount, BarUp = barUp }, input, ref cacheQTEInspiredScalp);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.QTEInspiredScalp QTEInspiredScalp(int barCount, bool barUp)
		{
			return indicator.QTEInspiredScalp(Input, barCount, barUp);
		}

		public Indicators.QTEInspiredScalp QTEInspiredScalp(ISeries<double> input , int barCount, bool barUp)
		{
			return indicator.QTEInspiredScalp(input, barCount, barUp);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.QTEInspiredScalp QTEInspiredScalp(int barCount, bool barUp)
		{
			return indicator.QTEInspiredScalp(Input, barCount, barUp);
		}

		public Indicators.QTEInspiredScalp QTEInspiredScalp(ISeries<double> input , int barCount, bool barUp)
		{
			return indicator.QTEInspiredScalp(input, barCount, barUp);
		}
	}
}

#endregion
