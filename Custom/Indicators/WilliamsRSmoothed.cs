// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The Williams %R is a momentum indicator that is designed to identify overbought and oversold areas in a nontrending market.
	/// </summary>
	public class WilliamsRSmoothed : Indicator
	{
		private MAX max;
		private MIN min;
		SMA s_percentR;
		Series<double> williamsR;
		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionWilliamsR;
				Name						= "WilliamsRSmoothed";
				IsSuspendedWhileInactive	= true;
				Period						= 14;
				SmoothLength				= 3;
				AddLine(Brushes.DarkGray,	-20,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddLine(Brushes.DarkGray,	-80,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddPlot(Brushes.Goldenrod,				NinjaTrader.Custom.Resource.WilliamsPercentR);
			}
			else if (State == State.Configure)
			{
				max = MAX(High, Period);
				min	= MIN(Low, Period);
			}
			
			else if (State == State.DataLoaded)
			{				
				williamsR = new Series<double>(this);
				
			}
		}

		protected override void OnBarUpdate()
		{
				if (CurrentBar < 1)
				return;
			double max0	= max[0];
			double min0	= min[0];
			
				Print("Made it to line 5");
			williamsR[0]	= -100 * (max0 - Close[0]) / (max0 - min0 == 0 ? 1 : max0 - min0);
				Print("Made it to line 7");
			s_percentR = SMA(williamsR,SmoothLength);
				Print("Made it to line 9");
			Value[0] = s_percentR[0];
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.1, double.MaxValue)]
		[Display(Name="Smoothing Length", Order=2, GroupName="Parameters")]
		public int SmoothLength
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WilliamsRSmoothed[] cacheWilliamsRSmoothed;
		public WilliamsRSmoothed WilliamsRSmoothed(int period, int smoothLength)
		{
			return WilliamsRSmoothed(Input, period, smoothLength);
		}

		public WilliamsRSmoothed WilliamsRSmoothed(ISeries<double> input, int period, int smoothLength)
		{
			if (cacheWilliamsRSmoothed != null)
				for (int idx = 0; idx < cacheWilliamsRSmoothed.Length; idx++)
					if (cacheWilliamsRSmoothed[idx] != null && cacheWilliamsRSmoothed[idx].Period == period && cacheWilliamsRSmoothed[idx].SmoothLength == smoothLength && cacheWilliamsRSmoothed[idx].EqualsInput(input))
						return cacheWilliamsRSmoothed[idx];
			return CacheIndicator<WilliamsRSmoothed>(new WilliamsRSmoothed(){ Period = period, SmoothLength = smoothLength }, input, ref cacheWilliamsRSmoothed);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WilliamsRSmoothed WilliamsRSmoothed(int period, int smoothLength)
		{
			return indicator.WilliamsRSmoothed(Input, period, smoothLength);
		}

		public Indicators.WilliamsRSmoothed WilliamsRSmoothed(ISeries<double> input , int period, int smoothLength)
		{
			return indicator.WilliamsRSmoothed(input, period, smoothLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WilliamsRSmoothed WilliamsRSmoothed(int period, int smoothLength)
		{
			return indicator.WilliamsRSmoothed(Input, period, smoothLength);
		}

		public Indicators.WilliamsRSmoothed WilliamsRSmoothed(ISeries<double> input , int period, int smoothLength)
		{
			return indicator.WilliamsRSmoothed(input, period, smoothLength);
		}
	}
}

#endregion
