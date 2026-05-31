//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
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
	/// Keltner Channel. The Keltner Channel is a similar indicator to Bollinger Bands.
	/// Here the midline is a standard moving average with the upper and lower bands offset
	/// by the SMA of the difference between the high and low of the previous bars.
	/// The offset multiplier as well as the SMA period is configurable.
	/// </summary>
	public class Zombie2KeltnerATR : Indicator
	{
		private const string SystemVersion = "v1.028";
		private const string SystemName = "Zombie2KeltnerATR";
        private const string FullSystemName = SystemName + " - " + SystemVersion;
		//private Series<double>		diff;
		private	EMA					emaDiff;
		private	EMA					emaValue;
		private ATR atrValue;

		public override string DisplayName
		{
			get { return FullSystemName; }
		}
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = SystemName;
				Description = FullSystemName;

				
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Calculate = Calculate.OnPriceChange;

				EMAPeriod = 21;
				ATRPeriod = 21;
				OffsetMultiplier1			= 1.0;
				OffsetMultiplier2 = 2.0;
				OffsetMultiplier3 = 3.0;

				PaintPriceMarkers = false;

				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "KeltnerMidline");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "KeltnerUpperLevel1");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "KeltnerLowerLevel1");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "KeltnerUpperLevel2");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "KeltnerLowerLevel2");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "KeltnerUpperLevel3");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "KeltnerLowerLevel3");
			}
			else if (State == State.DataLoaded)
			{
				//diff				= new Series<double>(this);
				//emaDiff				= EMA(diff, Period);
				emaValue			= EMA(Close, EMAPeriod);
				atrValue = ATR(EMAPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < EMAPeriod)
				return;

			//diff[0]			= High[0] - Low[0];

			double middle	= emaValue[0];
			double offset1 = atrValue[0] * OffsetMultiplier1; //emaDiff[0] * OffsetMultiplier;
			double offset2 = atrValue[0] * OffsetMultiplier2;
			double offset3 = atrValue[0] * OffsetMultiplier3;

			double upperLevel1	= middle + offset1;
			double lowerLevel1	= middle - offset1;
			double upperLevel2 = middle + offset2;
			double lowerLevel2 = middle - offset2;
			double upperLevel3 = middle + offset3;
			double lowerLevel3 = middle - offset3;

			Midline[0]		= middle;
			UpperLevel1[0]		= upperLevel1;
			LowerLevel1[0]		= lowerLevel1;
			UpperLevel2[0] = upperLevel2;
			LowerLevel2[0] = lowerLevel2;
			UpperLevel3[0] = upperLevel3;
			LowerLevel3[0] = lowerLevel3;

		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMAPeriod", GroupName = "NinjaScriptParameters", Order = 0)]
		public int EMAPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ATRPeriod", GroupName = "NinjaScriptParameters", Order = 1)]
		public int ATRPeriod
		{ get; set; }

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier1", GroupName = "NinjaScriptParameters", Order = 3)]
		public double OffsetMultiplier1
		{ get; set; }

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier2", GroupName = "NinjaScriptParameters", Order = 4)]
		public double OffsetMultiplier2
		{ get; set; }

		[Range(0.01, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "OffsetMultiplier3", GroupName = "NinjaScriptParameters", Order = 5)]
		public double OffsetMultiplier3
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Midline
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperLevel1
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerLevel1
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperLevel2
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerLevel2
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> UpperLevel3
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowerLevel3
		{
			get { return Values[6]; }
		}



		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2KeltnerATR[] cacheZombie2KeltnerATR;
		public Zombie2KeltnerATR Zombie2KeltnerATR(string indicatorName, int eMAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3)
		{
			return Zombie2KeltnerATR(Input, indicatorName, eMAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3);
		}

		public Zombie2KeltnerATR Zombie2KeltnerATR(ISeries<double> input, string indicatorName, int eMAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3)
		{
			if (cacheZombie2KeltnerATR != null)
				for (int idx = 0; idx < cacheZombie2KeltnerATR.Length; idx++)
					if (cacheZombie2KeltnerATR[idx] != null && cacheZombie2KeltnerATR[idx].IndicatorName == indicatorName && cacheZombie2KeltnerATR[idx].EMAPeriod == eMAPeriod && cacheZombie2KeltnerATR[idx].ATRPeriod == aTRPeriod && cacheZombie2KeltnerATR[idx].OffsetMultiplier1 == offsetMultiplier1 && cacheZombie2KeltnerATR[idx].OffsetMultiplier2 == offsetMultiplier2 && cacheZombie2KeltnerATR[idx].OffsetMultiplier3 == offsetMultiplier3 && cacheZombie2KeltnerATR[idx].EqualsInput(input))
						return cacheZombie2KeltnerATR[idx];
			return CacheIndicator<Zombie2KeltnerATR>(new Zombie2KeltnerATR(){ IndicatorName = indicatorName, EMAPeriod = eMAPeriod, ATRPeriod = aTRPeriod, OffsetMultiplier1 = offsetMultiplier1, OffsetMultiplier2 = offsetMultiplier2, OffsetMultiplier3 = offsetMultiplier3 }, input, ref cacheZombie2KeltnerATR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2KeltnerATR Zombie2KeltnerATR(string indicatorName, int eMAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3)
		{
			return indicator.Zombie2KeltnerATR(Input, indicatorName, eMAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3);
		}

		public Indicators.Zombie2KeltnerATR Zombie2KeltnerATR(ISeries<double> input , string indicatorName, int eMAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3)
		{
			return indicator.Zombie2KeltnerATR(input, indicatorName, eMAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2KeltnerATR Zombie2KeltnerATR(string indicatorName, int eMAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3)
		{
			return indicator.Zombie2KeltnerATR(Input, indicatorName, eMAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3);
		}

		public Indicators.Zombie2KeltnerATR Zombie2KeltnerATR(ISeries<double> input , string indicatorName, int eMAPeriod, int aTRPeriod, double offsetMultiplier1, double offsetMultiplier2, double offsetMultiplier3)
		{
			return indicator.Zombie2KeltnerATR(input, indicatorName, eMAPeriod, aTRPeriod, offsetMultiplier1, offsetMultiplier2, offsetMultiplier3);
		}
	}
}

#endregion
