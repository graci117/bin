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
using NinjaTrader.Gui.Tools;
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
	public class Zombie21EMA : Indicator
	{
		private const string SystemVersion = "v1.028";
		private const string SystemName = "Zombie21EMA";
		private const string FullSystemName = SystemName + " - " + SystemVersion;

		private int lastPrintOutputHashCode = 0;

		private Instrument attachedInstrument = null;

		private EMA shortEMA1Value;
		private EMA shortEMA2Value;
		private EMA shortEMA3Value;
		private EMA shortEMA4Value;
		private EMA shortEMA5Value;
		private EMA shortEMA6Value;

		private EMA longEMA1Value;
		private EMA longEMA2Value;
		private EMA longEMA3Value;
		private EMA longEMA4Value;
		private EMA longEMA5Value;
		private EMA longEMA6Value;

		private EMA hugeEMA1Value;

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

				IsOverlay = true;
				IsSuspendedWhileInactive = true;
				Calculate = Calculate.OnPriceChange;
				PaintPriceMarkers = false;

				ShortEMA1Period = 3;
				ShortEMA2Period = 5;
				ShortEMA3Period = 8;
				ShortEMA4Period = 10;
				ShortEMA5Period = 12;
				ShortEMA6Period = 15;

				LongEMA1Period = 30;
				LongEMA2Period = 35;
				LongEMA3Period = 40;
				LongEMA4Period = 45;
				LongEMA5Period = 50;
				LongEMA6Period = 55;

				HugeEMA1Period = 200;


				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "ShortEMA1");
				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "ShortEMA2");
				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "ShortEMA3");
				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "ShortEMA4");
				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "ShortEMA5");
				AddPlot(new Stroke(Brushes.Gray, 1), PlotStyle.Line, "ShortEMA6");

				AddPlot(new Stroke(Brushes.BlueViolet, 1), PlotStyle.Line, "LongEMA1");
				AddPlot(new Stroke(Brushes.BlueViolet, 1), PlotStyle.Line, "LongEMA2");
				AddPlot(new Stroke(Brushes.BlueViolet, 1), PlotStyle.Line, "LongEMA3");
				AddPlot(new Stroke(Brushes.BlueViolet, 1), PlotStyle.Line, "LongEMA4");
				AddPlot(new Stroke(Brushes.BlueViolet, 1), PlotStyle.Line, "LongEMA5");
				AddPlot(new Stroke(Brushes.BlueViolet, 1), PlotStyle.Line, "LongEMA6");

				AddPlot(new Stroke(Brushes.DimGray, DashStyleHelper.DashDot, 1), PlotStyle.Hash, "HugeEMA1");
			}
			else if (State == State.Configure)
			{
				attachedInstrument = this.Instrument;


			}
			else if (State == State.DataLoaded)
			{
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

				shortEMA1Value = EMA(Close, ShortEMA1Period);
				shortEMA2Value = EMA(Close, ShortEMA2Period);
				shortEMA3Value = EMA(Close, ShortEMA3Period);
				shortEMA4Value = EMA(Close, ShortEMA4Period);
				shortEMA5Value = EMA(Close, ShortEMA5Period);
				shortEMA6Value = EMA(Close, ShortEMA6Period);

				longEMA1Value = EMA(Close, LongEMA1Period);
				longEMA2Value = EMA(Close, LongEMA2Period);
				longEMA3Value = EMA(Close, LongEMA3Period);
				longEMA4Value = EMA(Close, LongEMA4Period);
				longEMA5Value = EMA(Close, LongEMA5Period);
				longEMA6Value = EMA(Close, LongEMA6Period);

				hugeEMA1Value = EMA(Close, HugeEMA1Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < LongEMA6Period)
				return;

			ShortEMA1[0] = shortEMA1Value[0];
			ShortEMA2[0] = shortEMA2Value[0];
			ShortEMA3[0] = shortEMA3Value[0];
			ShortEMA4[0] = shortEMA4Value[0];
			ShortEMA5[0] = shortEMA5Value[0];
			ShortEMA6[0] = shortEMA6Value[0];

			LongEMA1[0] = longEMA1Value[0];
			LongEMA2[0] = longEMA2Value[0];
			LongEMA3[0] = longEMA3Value[0];
			LongEMA4[0] = longEMA4Value[0];
			LongEMA5[0] = longEMA5Value[0];
			LongEMA6[0] = longEMA6Value[0];

			HugeEMA1[0] = hugeEMA1Value[0];
		}

		private void PrintOutput(string output, PrintTo outputTab = PrintTo.OutputTab1, bool blockDuplicateMessages = false)
		{
			this.PrintTo = outputTab;
			if (blockDuplicateMessages)
			{
				int tempHashCode = output.GetHashCode();
				if (tempHashCode != lastPrintOutputHashCode)
				{
					Print(DateTime.Now + " " + SystemName + ": " + output);
				}
				lastPrintOutputHashCode = tempHashCode;
			}
			else
				Print(DateTime.Now + " " + SystemName + ": " + output);
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
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 0)]
		public int ShortEMA1Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 1)]
		public int ShortEMA2Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 2)]
		public int ShortEMA3Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 3)]
		public int ShortEMA4Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 4)]
		public int ShortEMA5Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 5)]
		public int ShortEMA6Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 6)]
		public int LongEMA1Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 7)]
		public int LongEMA2Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 8)]
		public int LongEMA3Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 9)]
		public int LongEMA4Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 10)]
		public int LongEMA5Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 11)]
		public int LongEMA6Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 12)]
		public int HugeEMA1Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ShortEMA1
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ShortEMA2
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ShortEMA3
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ShortEMA4
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ShortEMA5
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ShortEMA6
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LongEMA1
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LongEMA2
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LongEMA3
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LongEMA4
		{
			get { return Values[9]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LongEMA5
		{
			get { return Values[10]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LongEMA6
		{
			get { return Values[11]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HugeEMA1
		{
			get { return Values[12]; }
		}


		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie21EMA[] cacheZombie21EMA;
		public Zombie21EMA Zombie21EMA(string indicatorName, int shortEMA1Period, int shortEMA2Period, int shortEMA3Period, int shortEMA4Period, int shortEMA5Period, int shortEMA6Period, int longEMA1Period, int longEMA2Period, int longEMA3Period, int longEMA4Period, int longEMA5Period, int longEMA6Period, int hugeEMA1Period)
		{
			return Zombie21EMA(Input, indicatorName, shortEMA1Period, shortEMA2Period, shortEMA3Period, shortEMA4Period, shortEMA5Period, shortEMA6Period, longEMA1Period, longEMA2Period, longEMA3Period, longEMA4Period, longEMA5Period, longEMA6Period, hugeEMA1Period);
		}

		public Zombie21EMA Zombie21EMA(ISeries<double> input, string indicatorName, int shortEMA1Period, int shortEMA2Period, int shortEMA3Period, int shortEMA4Period, int shortEMA5Period, int shortEMA6Period, int longEMA1Period, int longEMA2Period, int longEMA3Period, int longEMA4Period, int longEMA5Period, int longEMA6Period, int hugeEMA1Period)
		{
			if (cacheZombie21EMA != null)
				for (int idx = 0; idx < cacheZombie21EMA.Length; idx++)
					if (cacheZombie21EMA[idx] != null && cacheZombie21EMA[idx].IndicatorName == indicatorName && cacheZombie21EMA[idx].ShortEMA1Period == shortEMA1Period && cacheZombie21EMA[idx].ShortEMA2Period == shortEMA2Period && cacheZombie21EMA[idx].ShortEMA3Period == shortEMA3Period && cacheZombie21EMA[idx].ShortEMA4Period == shortEMA4Period && cacheZombie21EMA[idx].ShortEMA5Period == shortEMA5Period && cacheZombie21EMA[idx].ShortEMA6Period == shortEMA6Period && cacheZombie21EMA[idx].LongEMA1Period == longEMA1Period && cacheZombie21EMA[idx].LongEMA2Period == longEMA2Period && cacheZombie21EMA[idx].LongEMA3Period == longEMA3Period && cacheZombie21EMA[idx].LongEMA4Period == longEMA4Period && cacheZombie21EMA[idx].LongEMA5Period == longEMA5Period && cacheZombie21EMA[idx].LongEMA6Period == longEMA6Period && cacheZombie21EMA[idx].HugeEMA1Period == hugeEMA1Period && cacheZombie21EMA[idx].EqualsInput(input))
						return cacheZombie21EMA[idx];
			return CacheIndicator<Zombie21EMA>(new Zombie21EMA(){ IndicatorName = indicatorName, ShortEMA1Period = shortEMA1Period, ShortEMA2Period = shortEMA2Period, ShortEMA3Period = shortEMA3Period, ShortEMA4Period = shortEMA4Period, ShortEMA5Period = shortEMA5Period, ShortEMA6Period = shortEMA6Period, LongEMA1Period = longEMA1Period, LongEMA2Period = longEMA2Period, LongEMA3Period = longEMA3Period, LongEMA4Period = longEMA4Period, LongEMA5Period = longEMA5Period, LongEMA6Period = longEMA6Period, HugeEMA1Period = hugeEMA1Period }, input, ref cacheZombie21EMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie21EMA Zombie21EMA(string indicatorName, int shortEMA1Period, int shortEMA2Period, int shortEMA3Period, int shortEMA4Period, int shortEMA5Period, int shortEMA6Period, int longEMA1Period, int longEMA2Period, int longEMA3Period, int longEMA4Period, int longEMA5Period, int longEMA6Period, int hugeEMA1Period)
		{
			return indicator.Zombie21EMA(Input, indicatorName, shortEMA1Period, shortEMA2Period, shortEMA3Period, shortEMA4Period, shortEMA5Period, shortEMA6Period, longEMA1Period, longEMA2Period, longEMA3Period, longEMA4Period, longEMA5Period, longEMA6Period, hugeEMA1Period);
		}

		public Indicators.Zombie21EMA Zombie21EMA(ISeries<double> input , string indicatorName, int shortEMA1Period, int shortEMA2Period, int shortEMA3Period, int shortEMA4Period, int shortEMA5Period, int shortEMA6Period, int longEMA1Period, int longEMA2Period, int longEMA3Period, int longEMA4Period, int longEMA5Period, int longEMA6Period, int hugeEMA1Period)
		{
			return indicator.Zombie21EMA(input, indicatorName, shortEMA1Period, shortEMA2Period, shortEMA3Period, shortEMA4Period, shortEMA5Period, shortEMA6Period, longEMA1Period, longEMA2Period, longEMA3Period, longEMA4Period, longEMA5Period, longEMA6Period, hugeEMA1Period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie21EMA Zombie21EMA(string indicatorName, int shortEMA1Period, int shortEMA2Period, int shortEMA3Period, int shortEMA4Period, int shortEMA5Period, int shortEMA6Period, int longEMA1Period, int longEMA2Period, int longEMA3Period, int longEMA4Period, int longEMA5Period, int longEMA6Period, int hugeEMA1Period)
		{
			return indicator.Zombie21EMA(Input, indicatorName, shortEMA1Period, shortEMA2Period, shortEMA3Period, shortEMA4Period, shortEMA5Period, shortEMA6Period, longEMA1Period, longEMA2Period, longEMA3Period, longEMA4Period, longEMA5Period, longEMA6Period, hugeEMA1Period);
		}

		public Indicators.Zombie21EMA Zombie21EMA(ISeries<double> input , string indicatorName, int shortEMA1Period, int shortEMA2Period, int shortEMA3Period, int shortEMA4Period, int shortEMA5Period, int shortEMA6Period, int longEMA1Period, int longEMA2Period, int longEMA3Period, int longEMA4Period, int longEMA5Period, int longEMA6Period, int hugeEMA1Period)
		{
			return indicator.Zombie21EMA(input, indicatorName, shortEMA1Period, shortEMA2Period, shortEMA3Period, shortEMA4Period, shortEMA5Period, shortEMA6Period, longEMA1Period, longEMA2Period, longEMA3Period, longEMA4Period, longEMA5Period, longEMA6Period, hugeEMA1Period);
		}
	}
}

#endregion
