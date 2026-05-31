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
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber
{
	public class NinzaPulback : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "NinzaPulback";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ShowTransparentPlotsInDataBox				= true;
				
				TriggerDistanceTicks						= 10;
				AddPlot(Brushes.Transparent, "SignalPlot");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2)
				return;
			
			if (IsFirstTickOfBar)
				SignalPlot[0]	= 0;
			
			if (Close[1] > Open[1] && Low[0] <= Low[1] - TriggerDistanceTicks * TickSize)
			{
				SignalPlot[0] 	= 1;
				BarBrush		= Brushes.SteelBlue;
			}
			
			else if (Close[1] < Open[1] && High[0] >= High[1] + TriggerDistanceTicks * TickSize)
			{
				SignalPlot[0] 	= -1;
				BarBrush		= Brushes.DarkOrange;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TriggerDistanceTicks", Order=1, GroupName="Parameters")]
		public int TriggerDistanceTicks
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SignalPlot
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
		private TradeSaber.NinzaPulback[] cacheNinzaPulback;
		public TradeSaber.NinzaPulback NinzaPulback(int triggerDistanceTicks)
		{
			return NinzaPulback(Input, triggerDistanceTicks);
		}

		public TradeSaber.NinzaPulback NinzaPulback(ISeries<double> input, int triggerDistanceTicks)
		{
			if (cacheNinzaPulback != null)
				for (int idx = 0; idx < cacheNinzaPulback.Length; idx++)
					if (cacheNinzaPulback[idx] != null && cacheNinzaPulback[idx].TriggerDistanceTicks == triggerDistanceTicks && cacheNinzaPulback[idx].EqualsInput(input))
						return cacheNinzaPulback[idx];
			return CacheIndicator<TradeSaber.NinzaPulback>(new TradeSaber.NinzaPulback(){ TriggerDistanceTicks = triggerDistanceTicks }, input, ref cacheNinzaPulback);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.NinzaPulback NinzaPulback(int triggerDistanceTicks)
		{
			return indicator.NinzaPulback(Input, triggerDistanceTicks);
		}

		public Indicators.TradeSaber.NinzaPulback NinzaPulback(ISeries<double> input , int triggerDistanceTicks)
		{
			return indicator.NinzaPulback(input, triggerDistanceTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.NinzaPulback NinzaPulback(int triggerDistanceTicks)
		{
			return indicator.NinzaPulback(Input, triggerDistanceTicks);
		}

		public Indicators.TradeSaber.NinzaPulback NinzaPulback(ISeries<double> input , int triggerDistanceTicks)
		{
			return indicator.NinzaPulback(input, triggerDistanceTicks);
		}
	}
}

#endregion
