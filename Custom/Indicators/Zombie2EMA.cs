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
	public class Zombie2EMA : Indicator
	{
		private const string SystemVersion = "v1.028";
		private const string SystemName = "Zombie2EMA";
		private const string FullSystemName = SystemName + " - " + SystemVersion;

		private const string ObjectPrefix = "ze_";

		private int lastPrintOutputHashCode = 0;

		private Instrument attachedInstrument = null;

		const int EMA1ChangePlotIndex = 0;
		const int EMA1BullishPlotIndex = 1;
		const int EMA1BearishPlotIndex = 2;

		const int EMA2PlotIndex = 3;

		const int EMA3ChangePlotIndex = 4;
		const int EMA3BullishPlotIndex = 5;
		const int EMA3BearishPlotIndex = 6;

		const int EMA4ChangePlotIndex = 7;
		const int EMA4BullishPlotIndex = 8;
		const int EMA4BearishPlotIndex = 9;

		const int EMA5ChangePlotIndex = 10;
		const int EMA5BullishPlotIndex = 11;
		const int EMA5BearishPlotIndex = 12;

		private Brush ema1ChangeColor;
		private Brush ema1BullishColor;
		private Brush ema1BearishColor;

		private Brush ema3ChangeColor;
		private Brush ema3BullishColor;
		private Brush ema3BearishColor;

		private Brush ema4ChangeColor;
		private Brush ema4BullishColor;
		private Brush ema4BearishColor;

		private Brush ema5ChangeColor;
		private Brush ema5BullishColor;
		private Brush ema5BearishColor;

		private	EMA	ema1Value;
		private EMA ema2Value;
		private EMA ema3Value;
		private EMA ema4Value;
		private EMA ema5Value;
		private EMA ema6Value;

		private Brush maStackBullishColor = Brushes.Transparent;
		private Brush maStackBearishColor = Brushes.Transparent;
		private SimpleFont defaultFont = null;

		string m_spot1LastText = "";
		string m_spot2LastText = "";
		string m_spot3LastText = "";
		string m_spot4LastText = "";
		string m_spot5LastText = "";

		Brush m_spot1LastColor = Brushes.Transparent;
		Brush m_spot2LastColor = Brushes.Transparent;
		Brush m_spot3LastColor = Brushes.Transparent;
		Brush m_spot4LastColor = Brushes.Transparent;
		Brush m_spot5LastColor = Brushes.Transparent;

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

				EMA1Period = 8;
				EMA2Period = 21;
				EMA3Period = 34; //50
				EMA4Period = 55; //100
				EMA5Period = 89; //200
				EMA6Period = 200; //200
				ShowMAStack = true;
				MAStackClassicRules = false;
				MAStackBullishColor = Brushes.ForestGreen;
				MAStackBearishColor = Brushes.Maroon;

				PaintPriceMarkers = false;

				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "EMA1Change"); //SkyBlue
				AddPlot(new Stroke(Brushes.ForestGreen, 2), PlotStyle.Line, "EMA1Bullish");
				AddPlot(new Stroke(Brushes.Maroon, 2), PlotStyle.Line, "EMA1Bearish");

				AddPlot(new Stroke(Brushes.WhiteSmoke, 2), PlotStyle.Line, "EMA2");

				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "EMA3Change"); //LightSeaGreen
				AddPlot(new Stroke(Brushes.ForestGreen, 2), PlotStyle.Line, "EMA3Bullish");
				AddPlot(new Stroke(Brushes.Maroon, 2), PlotStyle.Line, "EMA3Bearish");

				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "EMA4Change"); //LightSeaGreen
				AddPlot(new Stroke(Brushes.ForestGreen, 2), PlotStyle.Line, "EM43Bullish");
				AddPlot(new Stroke(Brushes.Maroon, 2), PlotStyle.Line, "EM43Bearish");

				AddPlot(new Stroke(Brushes.Gray, 5), PlotStyle.Line, "EMA5Change"); //DodgerBlue
				AddPlot(new Stroke(Brushes.ForestGreen, 5), PlotStyle.Line, "EMA5Bullish");
				AddPlot(new Stroke(Brushes.Maroon, 5), PlotStyle.Line, "EMA5Bearish");

				AddPlot(new Stroke(Brushes.Gray, DashStyleHelper.Dash, 2), PlotStyle.Line, "EMA6");
			}
			else if (State == State.Configure)
			{
				attachedInstrument = this.Instrument;

				maStackBullishColor.Freeze();
				maStackBearishColor.Freeze();
			}
			else if (State == State.DataLoaded)
			{
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

				ema1Value = EMA(Close, EMA1Period);
				ema2Value = EMA(Close, EMA2Period);
				ema3Value = EMA(Close, EMA3Period);
				ema4Value = EMA(Close, EMA4Period);
				ema5Value = EMA(Close, EMA5Period);
				ema6Value = EMA(Close, EMA6Period);

				ema1ChangeColor = Plots[EMA1ChangePlotIndex].Brush;
				ema1BullishColor = Plots[EMA1BullishPlotIndex].Brush;
				ema1BearishColor = Plots[EMA1BearishPlotIndex].Brush;

				ema3ChangeColor = Plots[EMA3ChangePlotIndex].Brush;
				ema3BullishColor = Plots[EMA3BullishPlotIndex].Brush;
				ema3BearishColor = Plots[EMA3BearishPlotIndex].Brush;

				ema4ChangeColor = Plots[EMA4ChangePlotIndex].Brush;
				ema4BullishColor = Plots[EMA4BullishPlotIndex].Brush;
				ema4BearishColor = Plots[EMA4BearishPlotIndex].Brush;

				ema5ChangeColor = Plots[EMA5ChangePlotIndex].Brush;
				ema5BullishColor = Plots[EMA5BullishPlotIndex].Brush;
				ema5BearishColor = Plots[EMA5BearishPlotIndex].Brush;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < EMA6Period)
				return;

			EMA6[0] = ema6Value[0];

			double currentEMA1Value = ema1Value[0];
			double previousEMA1Value = ema1Value[1];
			bool ema1SlopeBullish = (currentEMA1Value >= previousEMA1Value);

			EMA1Change[0] = currentEMA1Value;

			if (ema1SlopeBullish)
			{
				PlotBrushes[EMA1ChangePlotIndex][0] = ema1BullishColor;
				//EMA1Bullish[0] = currentEMA1Value;
			}
			else
			{
				PlotBrushes[EMA1ChangePlotIndex][0] = ema1BearishColor;
				//EMA1Bearish[0] = currentEMA1Value;
			}

			double currentEMA2Value = ema2Value[0];
			double previousEMA2Value = ema2Value[1];
			bool ema2SlopeBullish = (currentEMA2Value >= previousEMA2Value);

			EMA2[0] = ema2Value[0];


			double currentEMA3Value = ema3Value[0];
			double previousEMA3Value = ema3Value[1];
			bool ema3SlopeBullish = (currentEMA3Value >= previousEMA3Value);

			EMA3Change[0] = currentEMA3Value;

			if (ema3SlopeBullish)
			{
				PlotBrushes[EMA3ChangePlotIndex][0] = ema3BullishColor;
				//EMA3Bullish[0] = currentEMA3Value;
			}
			else
			{
				PlotBrushes[EMA3ChangePlotIndex][0] = ema3BearishColor;
				//EMA3Bearish[0] = currentEMA3Value;
			}

			double currentEMA4Value = ema4Value[0];
			double previousEMA4Value = ema4Value[1];
			bool ema4SlopeBullish = (currentEMA4Value >= previousEMA4Value);

			EMA4Change[0] = currentEMA4Value;

			if (ema4SlopeBullish)
			{
				PlotBrushes[EMA4ChangePlotIndex][0] = ema4BullishColor;
				//EMA4Bullish[0] = currentEMA4Value;
			}
			else
			{
				PlotBrushes[EMA4ChangePlotIndex][0] = ema4BearishColor;
				//EMA4Bearish[0] = currentEMA4Value;
			}

			double currentEMA5Value = ema5Value[0];
			double ema89Value = ema5Value[0];
			double ema21Value = ema2Value[0];
			bool ema5RuleBullish = (ema21Value >= ema89Value);

			EMA5Change[0] = currentEMA5Value;
			

			if (ema5RuleBullish)
			{
				PlotBrushes[EMA5ChangePlotIndex][0] = ema5BullishColor;
				//EMA5Bullish[0] = currentEMA5Value;
			}
			else
			{
				PlotBrushes[EMA5ChangePlotIndex][0] = ema5BearishColor;
				//EMA5Bearish[0] = currentEMA5Value;
			}


			if (this.ChartControl != null && ShowMAStack)
			{
				if (defaultFont == null)
				{
					int fontSize = 10;
					defaultFont = new SimpleFont(this.ChartControl.Properties.LabelFont.Family.ToString(), fontSize) { Bold = true };
				}

				if (ema5RuleBullish)
				{

					if ((!MAStackClassicRules && ema1SlopeBullish) || (MAStackClassicRules && (ema1Value[0] >= ema2Value[0] && ema2Value[0] >= ema3Value[0] && ema3Value[0] >= ema4Value[0] && ema4Value[0] >= ema5Value[0])))
					{
						DrawSpot1(this.ChartControl, "1", MAStackBullishColor);
					}
					else
					{
						DrawSpot1(this.ChartControl, "1", MAStackBearishColor);
					}

					if ((!MAStackClassicRules && ema2SlopeBullish) || (MAStackClassicRules && (ema2Value[0] >= ema3Value[0] && ema3Value[0] >= ema4Value[0] && ema4Value[0] >= ema5Value[0])))
					{
						DrawSpot2(this.ChartControl, "2", MAStackBullishColor);
					}
					else
					{
						DrawSpot2(this.ChartControl, "2", MAStackBearishColor);
					}

					if ((!MAStackClassicRules && ema3SlopeBullish) || (MAStackClassicRules && (ema3Value[0] >= ema4Value[0] && ema4Value[0] >= ema5Value[0])))
					{
						DrawSpot3(this.ChartControl, "3", MAStackBullishColor);
					}
					else
					{
						DrawSpot3(this.ChartControl, "3", MAStackBearishColor);
					}

					if ((!MAStackClassicRules && ema4SlopeBullish) || (MAStackClassicRules && (ema4Value[0] >= ema5Value[0])))
					{
						DrawSpot4(this.ChartControl, "4", MAStackBullishColor);
					}
					else
					{
						DrawSpot4(this.ChartControl, "4", MAStackBearishColor);
					}


					DrawSpot5(this.ChartControl, "5", MAStackBullishColor);
				}
				else
				{
					DrawSpot1(this.ChartControl, "5", MAStackBearishColor);

					if ((!MAStackClassicRules && !ema4SlopeBullish) || (MAStackClassicRules && (ema4Value[0] < ema5Value[0])))
					{
						DrawSpot2(this.ChartControl, "4", MAStackBearishColor);
					}
					else
					{
						DrawSpot2(this.ChartControl, "4", MAStackBullishColor);
					}

					if ((!MAStackClassicRules && !ema3SlopeBullish) || (MAStackClassicRules && (ema3Value[0] < ema4Value[0] && ema4Value[0] < ema5Value[0])))
					{
						DrawSpot3(this.ChartControl, "3", MAStackBearishColor);
					}
					else
					{
						DrawSpot3(this.ChartControl, "3", MAStackBullishColor);
					}

					if ((!MAStackClassicRules && !ema2SlopeBullish) || (MAStackClassicRules && (ema2Value[0] < ema3Value[0] && ema3Value[0] < ema4Value[0] && ema4Value[0] < ema5Value[0])))
					{
						DrawSpot4(this.ChartControl, "2", MAStackBearishColor);
					}
					else
					{
						DrawSpot4(this.ChartControl, "2", MAStackBullishColor);
					}

					if ((!MAStackClassicRules && !ema1SlopeBullish) || (MAStackClassicRules && (ema1Value[0] < ema2Value[0] && ema2Value[0] < ema3Value[0] && ema3Value[0] < ema4Value[0] && ema4Value[0] < ema5Value[0])))
					{
						DrawSpot5(this.ChartControl, "1", MAStackBearishColor);
					}
					else
					{
						DrawSpot5(this.ChartControl, "1", MAStackBullishColor);
					}

				}

			}

		}


		private string BuildObjectFullName(string name)
        {
			string fullName = ObjectPrefix + name;
			return fullName;
		}

		private void DrawSpot1(ChartControl chartControl, string text, Brush backgroundColor)
		{
			if (text != m_spot1LastText || backgroundColor != m_spot1LastColor)
			{
				m_spot1LastText = text;
				m_spot1LastColor = backgroundColor;

				string formatedText = "* " + text + " *" + "\n\n\n\n\n\n\n\n\n";
				Draw.TextFixed(this, BuildObjectFullName("spot1"), formatedText, TextPosition.BottomLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
			}
		}

		private void DrawSpot2(ChartControl chartControl, string text, Brush backgroundColor)
		{
			if (text != m_spot2LastText || backgroundColor != m_spot2LastColor)
			{
				m_spot2LastText = text;
				m_spot2LastColor = backgroundColor;

				string formatedText = "* " + text + " *" + "\n\n\n\n\n\n\n";
				Draw.TextFixed(this, BuildObjectFullName("spot2"), formatedText, TextPosition.BottomLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
			}
		}

		private void DrawSpot3(ChartControl chartControl, string text, Brush backgroundColor)
		{
			if (text != m_spot3LastText || backgroundColor != m_spot3LastColor)
			{
				m_spot3LastText = text;
				m_spot3LastColor = backgroundColor;

				string formatedText = "* " + text + " *" + "\n\n\n\n\n";
				Draw.TextFixed(this, BuildObjectFullName("spot3"), formatedText, TextPosition.BottomLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
			}
		}

		private void DrawSpot4(ChartControl chartControl, string text, Brush backgroundColor)
		{
			if (text != m_spot4LastText || backgroundColor != m_spot4LastColor)
			{
				m_spot4LastText = text;
				m_spot4LastColor = backgroundColor;

				string formatedText = "* " + text + " *" + "\n\n\n";
				Draw.TextFixed(this, BuildObjectFullName("spot4"), formatedText, TextPosition.BottomLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
			}
		}
		private void DrawSpot5(ChartControl chartControl, string text, Brush backgroundColor)
		{
			if (text != m_spot5LastText || backgroundColor != m_spot5LastColor)
			{
				m_spot5LastText = text;
				m_spot5LastColor = backgroundColor;

				string formatedText = "* " + text + " *" + "\n";
				Draw.TextFixed(this, BuildObjectFullName("spot5"), formatedText, TextPosition.BottomLeft, Brushes.WhiteSmoke, defaultFont, Brushes.Black, backgroundColor, 100, DashStyleHelper.Solid, 2, false, "");
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
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 0)]
		public int EMA1Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 2)]
		public int EMA2Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 3)]
		public int EMA3Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 4)]
		public int EMA4Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 5)]
		public int EMA5Period
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 6)]
		public int EMA6Period
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 7)]
		public bool ShowMAStack
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 8)]
		public bool MAStackClassicRules
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 9)]
		[XmlIgnore]
		public Brush MAStackBullishColor
		{
			get { return maStackBullishColor; }
			set { maStackBullishColor = value; }
		}

		[Browsable(false)]
		public string MAStackBullishColorSerialize
		{
			get { return Serialize.BrushToString(MAStackBullishColor); }
			set { MAStackBullishColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "NinjaScriptParameters", Order = 10)]
		[XmlIgnore]
		public Brush MAStackBearishColor
		{
			get { return maStackBearishColor; }
			set { maStackBearishColor = value; }
		}

		[Browsable(false)]
		public string MAStackBearishColorSerialize
		{
			get { return Serialize.BrushToString(MAStackBearishColor); }
			set { MAStackBearishColor = Serialize.StringToBrush(value); }
		}




		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA1Change
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA1Bullish
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA1Bearish
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA2
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA3Change
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA3Bullish
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA3Bearish
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA4Change
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA4Bullish
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA4Bearish
		{
			get { return Values[9]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA5Change
		{
			get { return Values[10]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA5Bullish
		{
			get { return Values[11]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA5Bearish
		{
			get { return Values[12]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> EMA6
		{
			get { return Values[13]; }
		}


		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2EMA[] cacheZombie2EMA;
		public Zombie2EMA Zombie2EMA(string indicatorName, int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period, int eMA5Period, int eMA6Period, Brush mAStackBullishColor, Brush mAStackBearishColor)
		{
			return Zombie2EMA(Input, indicatorName, eMA1Period, eMA2Period, eMA3Period, eMA4Period, eMA5Period, eMA6Period, mAStackBullishColor, mAStackBearishColor);
		}

		public Zombie2EMA Zombie2EMA(ISeries<double> input, string indicatorName, int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period, int eMA5Period, int eMA6Period, Brush mAStackBullishColor, Brush mAStackBearishColor)
		{
			if (cacheZombie2EMA != null)
				for (int idx = 0; idx < cacheZombie2EMA.Length; idx++)
					if (cacheZombie2EMA[idx] != null && cacheZombie2EMA[idx].IndicatorName == indicatorName && cacheZombie2EMA[idx].EMA1Period == eMA1Period && cacheZombie2EMA[idx].EMA2Period == eMA2Period && cacheZombie2EMA[idx].EMA3Period == eMA3Period && cacheZombie2EMA[idx].EMA4Period == eMA4Period && cacheZombie2EMA[idx].EMA5Period == eMA5Period && cacheZombie2EMA[idx].EMA6Period == eMA6Period && cacheZombie2EMA[idx].MAStackBullishColor == mAStackBullishColor && cacheZombie2EMA[idx].MAStackBearishColor == mAStackBearishColor && cacheZombie2EMA[idx].EqualsInput(input))
						return cacheZombie2EMA[idx];
			return CacheIndicator<Zombie2EMA>(new Zombie2EMA(){ IndicatorName = indicatorName, EMA1Period = eMA1Period, EMA2Period = eMA2Period, EMA3Period = eMA3Period, EMA4Period = eMA4Period, EMA5Period = eMA5Period, EMA6Period = eMA6Period, MAStackBullishColor = mAStackBullishColor, MAStackBearishColor = mAStackBearishColor }, input, ref cacheZombie2EMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2EMA Zombie2EMA(string indicatorName, int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period, int eMA5Period, int eMA6Period, Brush mAStackBullishColor, Brush mAStackBearishColor)
		{
			return indicator.Zombie2EMA(Input, indicatorName, eMA1Period, eMA2Period, eMA3Period, eMA4Period, eMA5Period, eMA6Period, mAStackBullishColor, mAStackBearishColor);
		}

		public Indicators.Zombie2EMA Zombie2EMA(ISeries<double> input , string indicatorName, int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period, int eMA5Period, int eMA6Period, Brush mAStackBullishColor, Brush mAStackBearishColor)
		{
			return indicator.Zombie2EMA(input, indicatorName, eMA1Period, eMA2Period, eMA3Period, eMA4Period, eMA5Period, eMA6Period, mAStackBullishColor, mAStackBearishColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2EMA Zombie2EMA(string indicatorName, int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period, int eMA5Period, int eMA6Period, Brush mAStackBullishColor, Brush mAStackBearishColor)
		{
			return indicator.Zombie2EMA(Input, indicatorName, eMA1Period, eMA2Period, eMA3Period, eMA4Period, eMA5Period, eMA6Period, mAStackBullishColor, mAStackBearishColor);
		}

		public Indicators.Zombie2EMA Zombie2EMA(ISeries<double> input , string indicatorName, int eMA1Period, int eMA2Period, int eMA3Period, int eMA4Period, int eMA5Period, int eMA6Period, Brush mAStackBullishColor, Brush mAStackBearishColor)
		{
			return indicator.Zombie2EMA(input, indicatorName, eMA1Period, eMA2Period, eMA3Period, eMA4Period, eMA5Period, eMA6Period, mAStackBullishColor, mAStackBearishColor);
		}
	}
}

#endregion
