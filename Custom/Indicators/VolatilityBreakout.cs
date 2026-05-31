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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class VolatilityBreakout : Indicator
	{
		public bool blnFlag;
		private int barSensitivity;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Determine the periods of extremes of low volatility which usually followed by big moves. Indicator does also shows  direction of the trade based on Momentum indicator";
				Name						= "VolatilityBreakout";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				Period						= 20;
				KCDeviation					= 1.5;
				BBDeviation					= 2;
				MomentumPeriod				= 12;
				barSensitivity 				= 3;
				blnFlag 					= false;
				AddPlot(new Stroke(Brushes.OrangeRed, 2), PlotStyle.Bar, "Plot0");
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			/* The squeeze takes advantage of quiet periods in the market when the volatility 
			has decreased significantly and the market is building up energy for its next major 
			move higher or lower. Period of low volatility are identified as the times when the bands 
			"move closer together". How do we know that the current narrowness is really narrow enough 
			to qualify as low volatility? By adding Keltner Channels and momentum index oscillator 
			as per mentioned in John Carter's book Mastering the Trade.
			
			While Bolling Bands expand and contract as the markets alter between periods of high and 
			low volatility, the Keltner Channels stay in more of a steady range. The momentum index 
			oscillator is used to estimate the direction.
			
			How does this Setup work?
			The quite period is identified whent he Bollinger Bands narrow in width to the point that 
			they are actually trading inside of the Keltner Channels. This marks a period of reduced 
			volatility and signals that the market is taking a significant breather, building up steam
			for its next move. The trade signal occurs when the Bollinger Bands then move back outside 
			the Keltner Channels. Use 12 period momentum index oscillator to determine whether to go 
			long or short. If the oscillator is above 0 when this happens, GO LONG; if it id below 0 then
			GO SHORT.
			
			Usually the moves are explosive when the BB Width is lowest over past 6 months which comes to 
			across 126 days and hence we need more than 126 price data bars.
			
			*/
			
			if (CurrentBar > 127) 
			{
				
				/* if (bBandWidth < 0)
				{
					bBandWidth=0;
				}*/
				
				if (KeltnerChannel(KCDeviation, Period).Upper[0] >  Bollinger(BBDeviation, Period).Upper[0] &&
					KeltnerChannel(KCDeviation, Period).Lower[0] <  Bollinger(BBDeviation, Period).Lower[0] &&
					(BollingerBandWidth(Period, BBDeviation)[0] == MIN(BollingerBandWidth(Period, BBDeviation), 126)[0]) ) //BollingerBandWidth(bBdeviation,Period)[0] > bollingerWidth)
				{
					Plot0[0] = 1; //This is just a warning signal to be ready
					blnFlag = true;
				}
				else if (blnFlag && 
					KeltnerChannel(KCDeviation, Period).Upper[0] <=  Bollinger(BBDeviation, Period).Upper[0] &&
					KeltnerChannel(KCDeviation, Period).Lower[0] >= Bollinger(BBDeviation, Period).Lower[0])
				{
					if(Momentum(MomentumPeriod)[0] > 0)
						Plot0[0] = 2;
					else
						Plot0[0] = -2;
					//Plot0[0] = Momentum(MomentumPeriod)[0] >0? 2:-2; // +ve buy & -ve short
					blnFlag = false;
						
				}
				else
				{
					Plot0[0] = 0;
				}
			}
			else
			{		
				Plot0[0] = 0;
			}
		}

		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Period", Description="Used to calculate band/channel", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Range(1, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="KCDeviation", Description="Used to calculate Keltner Channel", Order=2, GroupName="Parameters")]
		public double KCDeviation
		{ get; set; }

		[Range(1, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BBDeviation", Description="Used to calculate Bollinger Bands", Order=3, GroupName="Parameters")]
		public double BBDeviation
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="MomentumPeriod", Description="Used to calculate Momentum", Order=4, GroupName="Parameters")]
		public int MomentumPeriod
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Plot0
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
		private VolatilityBreakout[] cacheVolatilityBreakout;
		public VolatilityBreakout VolatilityBreakout(int period, double kCDeviation, double bBDeviation, int momentumPeriod)
		{
			return VolatilityBreakout(Input, period, kCDeviation, bBDeviation, momentumPeriod);
		}

		public VolatilityBreakout VolatilityBreakout(ISeries<double> input, int period, double kCDeviation, double bBDeviation, int momentumPeriod)
		{
			if (cacheVolatilityBreakout != null)
				for (int idx = 0; idx < cacheVolatilityBreakout.Length; idx++)
					if (cacheVolatilityBreakout[idx] != null && cacheVolatilityBreakout[idx].Period == period && cacheVolatilityBreakout[idx].KCDeviation == kCDeviation && cacheVolatilityBreakout[idx].BBDeviation == bBDeviation && cacheVolatilityBreakout[idx].MomentumPeriod == momentumPeriod && cacheVolatilityBreakout[idx].EqualsInput(input))
						return cacheVolatilityBreakout[idx];
			return CacheIndicator<VolatilityBreakout>(new VolatilityBreakout(){ Period = period, KCDeviation = kCDeviation, BBDeviation = bBDeviation, MomentumPeriod = momentumPeriod }, input, ref cacheVolatilityBreakout);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolatilityBreakout VolatilityBreakout(int period, double kCDeviation, double bBDeviation, int momentumPeriod)
		{
			return indicator.VolatilityBreakout(Input, period, kCDeviation, bBDeviation, momentumPeriod);
		}

		public Indicators.VolatilityBreakout VolatilityBreakout(ISeries<double> input , int period, double kCDeviation, double bBDeviation, int momentumPeriod)
		{
			return indicator.VolatilityBreakout(input, period, kCDeviation, bBDeviation, momentumPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolatilityBreakout VolatilityBreakout(int period, double kCDeviation, double bBDeviation, int momentumPeriod)
		{
			return indicator.VolatilityBreakout(Input, period, kCDeviation, bBDeviation, momentumPeriod);
		}

		public Indicators.VolatilityBreakout VolatilityBreakout(ISeries<double> input , int period, double kCDeviation, double bBDeviation, int momentumPeriod)
		{
			return indicator.VolatilityBreakout(input, period, kCDeviation, bBDeviation, momentumPeriod);
		}
	}
}

#endregion
