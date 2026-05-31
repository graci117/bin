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
	public class Zombie2PowerLine : Indicator
	{
		private const string SystemVersion = "v1.028";
		private const string SystemName = "Zombie2PowerLine";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		private MAX max;
		private MIN min;
		private int lastPrintOutputHashCode = 0;

		private Instrument attachedInstrument = null;

		const int MeanChangePlotIndex = 0;
		const int MeanBullishPlotIndex = 1;
		const int MeanBearishPlotIndex = 2;
		const int UpperPlotIndex = 3;
		const int LowerPlotIndex = 4;

		private Brush meanChangeColor;
		private Brush meanBullishColor;
		private Brush meanBearishColor;

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

				Calculate = Calculate.OnPriceChange;
				IsOverlay = true;
				IsSuspendedWhileInactive = true;
				PaintPriceMarkers = false;

				Period = 42;

				AddPlot(new Stroke(Brushes.Gray, 5), PlotStyle.Line, "MeanChange");
				AddPlot(new Stroke(Brushes.ForestGreen, 1), PlotStyle.Line, "MeanBullish");
				AddPlot(new Stroke(Brushes.Maroon, 1), PlotStyle.Line, "MeanBearish");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Upper");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Lower");

			}
			else if (State == State.Configure)
			{
				attachedInstrument = this.Instrument;
			}
			else if (State == State.DataLoaded)
			{
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

				max = MAX(High, Period);
				min = MIN(Low, Period);

				meanChangeColor = Plots[MeanChangePlotIndex].Brush;
				meanBullishColor = Plots[MeanBullishPlotIndex].Brush;
				meanBearishColor = Plots[MeanBearishPlotIndex].Brush;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Period)
				return;

			double max0 = max[0];
			double min0	= min[0];

			MeanChange[0]	= (max0 + min0) / 2;

			Upper[0] = max0;
			Lower[0] = min0;

			if (Close[0] >= MeanChange[0])
			{
				PlotBrushes[MeanChangePlotIndex][0] = meanBullishColor;
				
				this.MeanBullish[0] = 1;
				this.MeanBearish[0] = 0;
				//MeanBullish[0] = MeanChange[0];

				//if (MeanBullish[1] > 0) PlotBrushes[MeanChangePlotIndex][0] = Brushes.Transparent;
			}
			else //if (Close[0] < MeanChange[0])
			{
				PlotBrushes[MeanChangePlotIndex][0] = meanBearishColor;
				this.MeanBullish[0] = 0;
				this.MeanBearish[0] = 1;
				//MeanBearish[0] = MeanChange[0];
				//if (MeanBearish[1] > 0) PlotBrushes[MeanChangePlotIndex][0] = Brushes.Transparent;
			}

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
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MeanChange
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MeanBullish
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> MeanBearish
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[4]; }
		}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2PowerLine[] cacheZombie2PowerLine;
		public Zombie2PowerLine Zombie2PowerLine(string indicatorName, int period)
		{
			return Zombie2PowerLine(Input, indicatorName, period);
		}

		public Zombie2PowerLine Zombie2PowerLine(ISeries<double> input, string indicatorName, int period)
		{
			if (cacheZombie2PowerLine != null)
				for (int idx = 0; idx < cacheZombie2PowerLine.Length; idx++)
					if (cacheZombie2PowerLine[idx] != null && cacheZombie2PowerLine[idx].IndicatorName == indicatorName && cacheZombie2PowerLine[idx].Period == period && cacheZombie2PowerLine[idx].EqualsInput(input))
						return cacheZombie2PowerLine[idx];
			return CacheIndicator<Zombie2PowerLine>(new Zombie2PowerLine(){ IndicatorName = indicatorName, Period = period }, input, ref cacheZombie2PowerLine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2PowerLine Zombie2PowerLine(string indicatorName, int period)
		{
			return indicator.Zombie2PowerLine(Input, indicatorName, period);
		}

		public Indicators.Zombie2PowerLine Zombie2PowerLine(ISeries<double> input , string indicatorName, int period)
		{
			return indicator.Zombie2PowerLine(input, indicatorName, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2PowerLine Zombie2PowerLine(string indicatorName, int period)
		{
			return indicator.Zombie2PowerLine(Input, indicatorName, period);
		}

		public Indicators.Zombie2PowerLine Zombie2PowerLine(ISeries<double> input , string indicatorName, int period)
		{
			return indicator.Zombie2PowerLine(input, indicatorName, period);
		}
	}
}

#endregion
