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

namespace NinjaTrader.NinjaScript.Indicators
{
	public class ChannelSSL : Indicator
	{
		private int trend;
		private ISeries<double> maHigh, maLow;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "Channel SSL";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;

				Period					= 20;
				
				AddPlot(Brushes.DodgerBlue, "SslUp");
				AddPlot(Brushes.Firebrick, "SslDown");
			}
			else if (State == State.DataLoaded)
			{
				maHigh = SMA(High, Period);
				maLow = SMA(Low, Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar <= Period) return;
			
			if(Close[0] > maHigh[0]) 
			{
				if(trend != 1)
					Draw.ArrowUp(this, "Up " + CurrentBar.ToString(), false, 0, Low[0] - 2 * TickSize, Brushes.Gold);
				
				trend = 1;
			}
			else if(Close[0] < maLow[0]) 
			{
				if(trend != -1)
					Draw.ArrowDown(this, "Down " + CurrentBar.ToString(), false, 0, High[0] + 2 * TickSize, Brushes.Gold);
				
				trend = -1;
			}
			
			if(trend == 1) 
			{
				SslDown[0] = maLow[0];
				SslUp[0] = maHigh[0];
			}
			else if(trend == -1) 
			{
				SslDown[0] = maHigh[0];
				SslUp[0] = maLow[0];
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SslUp
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SslDown
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChannelSSL[] cacheChannelSSL;
		public ChannelSSL ChannelSSL(int period)
		{
			return ChannelSSL(Input, period);
		}

		public ChannelSSL ChannelSSL(ISeries<double> input, int period)
		{
			if (cacheChannelSSL != null)
				for (int idx = 0; idx < cacheChannelSSL.Length; idx++)
					if (cacheChannelSSL[idx] != null && cacheChannelSSL[idx].Period == period && cacheChannelSSL[idx].EqualsInput(input))
						return cacheChannelSSL[idx];
			return CacheIndicator<ChannelSSL>(new ChannelSSL(){ Period = period }, input, ref cacheChannelSSL);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChannelSSL ChannelSSL(int period)
		{
			return indicator.ChannelSSL(Input, period);
		}

		public Indicators.ChannelSSL ChannelSSL(ISeries<double> input , int period)
		{
			return indicator.ChannelSSL(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChannelSSL ChannelSSL(int period)
		{
			return indicator.ChannelSSL(Input, period);
		}

		public Indicators.ChannelSSL ChannelSSL(ISeries<double> input , int period)
		{
			return indicator.ChannelSSL(input, period);
		}
	}
}

#endregion
