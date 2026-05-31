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
	/// The MACD (Moving Average Convergence/Divergence) is a trend following momentum indicator
	/// that shows the relationship between two moving averages of prices.
	/// </summary>
	public class Zombie2MACD : Indicator
	{
		private const string SystemVersion = "v1.028";
		private const string SystemName = "Zombie2MACD";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		private	Series<double>		macdEMAFast;
		private	Series<double>		macdEMASlow;
		private EMA emafastValue;
		private EMA emaMiddleValue;
		private EMA emaSlowValue;
		private double				constant1;
		private double				constant2;
		private double				constant3;
		private double				constant4;
		private double				constant5;
		private double				constant6;

		const int HistoBullStrongPlotIndex = 0;
		const int HistoBullWeakPlotIndex = 1;
		const int HistoBearStrongPlotIndex = 2;
		const int HistoBearWeakPlotIndex = 3;
		const int HistoBullMixedStrongPlotIndex = 4;
		const int HistoBullMixedWeakPlotIndex = 5;
		const int HistoBearMixedStrongPlotIndex = 6;
		const int HistoBearMixedWeakPlotIndex = 7;

		const int LineChangePlotIndex = 8;
		const int LineBullishPlotIndex = 9;
		const int LineBearishPlotIndex = 10;

		const int AvgChangePlotIndex = 11;
		const int AvgBullishPlotIndex = 12;
		const int AvgBearishPlotIndex = 13;

		private Brush lineChangeColor;
		private Brush lineBullishColor;
		private Brush lineBearishColor;

		private Brush avgChangeColor;
		private Brush avgBullishColor;
		private Brush avgBearishColor;

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
				PaintPriceMarkers = false; 
				IsSuspendedWhileInactive = true;
				BarsToLoad = 2;
				MACDFastPeriod						= 12;
				MACDSlowPeriod						= 26;
				MACDSmoothPeriod = 9;

				EMAFastPeriod = 8;
				EMAMiddlePeriod = 21;
				EMASlowPeriod = 89;

				HistoMultiplier = 3;
				PaintBars = false;


				//levels
				AddLine(Brushes.DarkGray, 0, NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);

				//histo
				AddPlot(new Stroke(Brushes.Chartreuse, 5), PlotStyle.Bar, "HistoBullStrong");  
				AddPlot(new Stroke(Brushes.RoyalBlue, 5),	PlotStyle.Bar,	"HistoBullWeak"); 
				AddPlot(new Stroke(Brushes.Red, 5), PlotStyle.Bar, "HistoBearStrong"); 
				AddPlot(new Stroke(Brushes.DarkOrange, 5), PlotStyle.Bar, "HistoBearWeak"); 

				AddPlot(new Stroke(Brushes.WhiteSmoke, 5), PlotStyle.Bar, "HistoBullMixedStrong"); 
				AddPlot(new Stroke(Brushes.Gray, 5), PlotStyle.Bar, "HistoBullMixedWeak"); 
				AddPlot(new Stroke(Brushes.WhiteSmoke, 5), PlotStyle.Bar, "HistoBearMixedStrong"); 
				AddPlot(new Stroke(Brushes.Gray, 5), PlotStyle.Bar, "HistoBearMixedWeak"); 

				//lines
				AddPlot(new Stroke(Brushes.Gray, 3), PlotStyle.Line, "LineChange"); 
				AddPlot(new Stroke(Brushes.Chartreuse, 3), PlotStyle.Line, "LineBullish"); 
				AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Line, "LineBearish"); 

				AddPlot(new Stroke(Brushes.Gray, 3), PlotStyle.Line, "AvgChange");
				AddPlot(new Stroke(Brushes.WhiteSmoke, 3), PlotStyle.Line, "AvgBullish"); 
				AddPlot(new Stroke(Brushes.WhiteSmoke, 3), PlotStyle.Line, "AvgBearish");


				Plots[0].AutoWidth = true;
				Plots[1].AutoWidth = true;
				Plots[2].AutoWidth = true;
				Plots[3].AutoWidth = true;
				Plots[4].AutoWidth = true;
				Plots[5].AutoWidth = true;
				Plots[6].AutoWidth = true;
				Plots[7].AutoWidth = true;


			}
			else if (State == State.Configure)
			{
				constant1	= 2.0 / (1 + MACDFastPeriod);
				constant2	= (1 - (2.0 / (1 + MACDFastPeriod)));
				constant3	= 2.0 / (1 + MACDSlowPeriod);
				constant4	= (1 - (2.0 / (1 + MACDSlowPeriod)));
				constant5	= 2.0 / (1 + MACDSmoothPeriod);
				constant6	= (1 - (2.0 / (1 + MACDSmoothPeriod)));
			}
			else if (State == State.DataLoaded)
			{
				macdEMAFast = new Series<double>(this);
				macdEMASlow = new Series<double>(this);

				emafastValue = EMA(Close, EMAFastPeriod);
				emaMiddleValue = EMA(Close, EMAMiddlePeriod);
				emaSlowValue = EMA(Close, EMASlowPeriod);

				lineChangeColor = Plots[LineChangePlotIndex].Brush;
				lineBullishColor = Plots[LineBullishPlotIndex].Brush;
				lineBearishColor = Plots[LineBearishPlotIndex].Brush;

				avgChangeColor = Plots[AvgChangePlotIndex].Brush;
				avgBullishColor = Plots[AvgBullishPlotIndex].Brush;
				avgBearishColor = Plots[AvgBearishPlotIndex].Brush;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < EMASlowPeriod)
				return;

			double input0	= Input[0];
			double input1 = Input[1];


			if (CurrentBar == 0)
			{
				macdEMAFast[0]		= input0;
				macdEMASlow[0]		= input0;
				LineChange[0]		= 0;
				LineBullish[0] = 0;
				LineBearish[0] = 0;
				AvgChange[0] = 0;
				AvgBullish[0] = 0;
				AvgBearish[0] = 0;
				DiffBullStrong[0] = 0;
				DiffBullWeak[0] = 0;
				DiffBearStrong[0] = 0;
				DiffBearWeak[0] = 0;
				DiffBullMixedStrong[0] = 0;
				DiffBullMixedWeak[0] = 0;
				DiffBearMixedStrong[0] = 0;
				DiffBearMixedWeak[0] = 0;
			}
			else
			{

				bool bullishTrend = (emaMiddleValue[0] >= emaSlowValue[0]);
				bool bullishMicroTrend = (emafastValue[0] >= emafastValue[1]);


				double fastEma0	= constant1 * input0 + constant2 * macdEMAFast[1];
				double slowEma0	= constant3 * input0 + constant4 * macdEMASlow[1];
				double fastEma1 = constant1 * input1 + constant2 * macdEMAFast[2];
				double slowEma1 = constant3 * input1 + constant4 * macdEMASlow[2];
				double macd		= fastEma0 - slowEma0;
				double previousMACD = fastEma1 - slowEma1;
				double macdAvg	= constant5 * macd + constant6 * AvgChange[1];
				double previousMACDAvg = constant5 * previousMACD + constant6 * AvgChange[2];
				double diff = (macd - macdAvg) * HistoMultiplier;
				double previousHisto = 0;

				DiffBullStrong[0] = 0;
				DiffBullWeak[0] = 0;
				DiffBearStrong[0] = 0;
				DiffBearWeak[0] = 0;
				DiffBullMixedStrong[0] = 0;
				DiffBullMixedWeak[0] = 0;
				DiffBearMixedStrong[0] = 0;
				DiffBearMixedWeak[0] = 0;

				if (DiffBullStrong[1] != 0)
					previousHisto = DiffBullStrong[1];
				else if (DiffBullWeak[1] != 0)
					previousHisto = DiffBullWeak[1];
				else if (DiffBearStrong[1] != 0)
					previousHisto = DiffBearStrong[1];
				else if (DiffBearWeak[1] != 0)
					previousHisto = DiffBearWeak[1];
				else if (DiffBullMixedStrong[1] != 0)
					previousHisto = DiffBullMixedStrong[1];
				else if (DiffBullMixedWeak[1] != 0)
					previousHisto = DiffBullMixedWeak[1];
				else if (DiffBearMixedStrong[1] != 0)
					previousHisto = DiffBearMixedStrong[1];
				else if (DiffBearMixedWeak[1] != 0)
					previousHisto = DiffBearMixedWeak[1];

				macdEMAFast[0]		= fastEma0;
				macdEMASlow[0]		= slowEma0;
				LineChange[0]		= macd;
				AvgChange[0]			= macdAvg;

				bool bullishMACDSlope = (macd >= previousMACD);

				if (bullishMACDSlope)
                {
					PlotBrushes[LineChangePlotIndex][0] = lineBullishColor;
					//LineBullish[0] = macd;
				}
				else
                {
					PlotBrushes[LineChangePlotIndex][0] = lineBearishColor;
					//LineBearish[0] = macd;
				}
				

				bool bullishAvgSlope =  (macdAvg >= previousMACDAvg);

				if (bullishAvgSlope)
				{
					PlotBrushes[AvgChangePlotIndex][0] = avgBullishColor;
					//AvgBullish[0] = macdAvg;
				}
				else
				{
					PlotBrushes[AvgChangePlotIndex][0] = avgBearishColor;
					//AvgBearish[0] = macdAvg;
				}
				

				if (diff >= 0)
				{
					if (diff > previousHisto)
					{
						if (!bullishTrend) //macdAvg <= 0 && this.SpecialColorsForMixBars)
						{
							DiffBearMixedStrong[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[6].Brush; //this.BearMixedStrong;
								CandleOutlineBrush = Plots[6].Brush; //
							}
						}
						else
						{
							DiffBullStrong[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[0].Brush;
								CandleOutlineBrush = Plots[0].Brush;
							}
						}
					}
					else
					{
						if (!bullishTrend) //macdAvg <= 0 && this.SpecialColorsForMixBars)
						{
							
							DiffBearMixedWeak[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[7].Brush; // this.BearMixedWeak;
								CandleOutlineBrush = Plots[7].Brush; //this.BearMixedWeak;
							}
						}
						else
						{
							

							DiffBullWeak[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[1].Brush;
								CandleOutlineBrush = Plots[1].Brush;
							}
						}
					}
				}
				else
				{
					if (diff < previousHisto)
					{
						if (bullishTrend) //macdAvg >= 0 && this.SpecialColorsForMixBars)
						{
							DiffBullMixedWeak[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[5].Brush; // this.BullMixedWeak;
								CandleOutlineBrush = Plots[5].Brush; // this.BullMixedWeak;
							}
						}
						else
						{
							DiffBearStrong[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[2].Brush;
								CandleOutlineBrush = Plots[2].Brush;
							}
						}
					}
					else
					{
						if (bullishTrend) //macdAvg >= 0 && this.SpecialColorsForMixBars)
						{
							DiffBullMixedStrong[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[4].Brush; //this.BullMixedStrong;
								CandleOutlineBrush = Plots[4].Brush; //this.BullMixedStrong;
							}
						}
						else
						{
							DiffBearWeak[0] = diff;

							if (PaintBars)
							{
								BarBrush = Plots[3].Brush;
								CandleOutlineBrush = Plots[3].Brush;
							}
						}
					}
				}
			}
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LineChange
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LineBullish
		{
			get { return Values[9]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LineBearish
		{
			get { return Values[10]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgChange
		{
			get { return Values[11]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgBullish
		{
			get { return Values[12]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> AvgBearish
		{
			get { return Values[13]; }
		}


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBullStrong
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBullWeak
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBearStrong
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBearWeak
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBullMixedStrong
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBullMixedWeak
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBearMixedStrong
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DiffBearMixedWeak
		{
			get { return Values[7]; }
		}

		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 0)]
		public int MACDFastPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 1)]
		public int MACDSlowPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 2)]
		public int MACDSmoothPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 3)]
		public int EMAFastPeriod
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 4)]
		public int EMAMiddlePeriod
		{ get; set; }


		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 5)]
		public int EMASlowPeriod
		{ get; set; }

		[Range(1, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 6)]
		public double HistoMultiplier
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 7)]
		public bool PaintBars
		{ get; set; }

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2MACD[] cacheZombie2MACD;
		public Zombie2MACD Zombie2MACD(string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, double histoMultiplier)
		{
			return Zombie2MACD(Input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, histoMultiplier);
		}

		public Zombie2MACD Zombie2MACD(ISeries<double> input, string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, double histoMultiplier)
		{
			if (cacheZombie2MACD != null)
				for (int idx = 0; idx < cacheZombie2MACD.Length; idx++)
					if (cacheZombie2MACD[idx] != null && cacheZombie2MACD[idx].IndicatorName == indicatorName && cacheZombie2MACD[idx].MACDFastPeriod == mACDFastPeriod && cacheZombie2MACD[idx].MACDSlowPeriod == mACDSlowPeriod && cacheZombie2MACD[idx].MACDSmoothPeriod == mACDSmoothPeriod && cacheZombie2MACD[idx].EMAFastPeriod == eMAFastPeriod && cacheZombie2MACD[idx].EMAMiddlePeriod == eMAMiddlePeriod && cacheZombie2MACD[idx].EMASlowPeriod == eMASlowPeriod && cacheZombie2MACD[idx].HistoMultiplier == histoMultiplier && cacheZombie2MACD[idx].EqualsInput(input))
						return cacheZombie2MACD[idx];
			return CacheIndicator<Zombie2MACD>(new Zombie2MACD(){ IndicatorName = indicatorName, MACDFastPeriod = mACDFastPeriod, MACDSlowPeriod = mACDSlowPeriod, MACDSmoothPeriod = mACDSmoothPeriod, EMAFastPeriod = eMAFastPeriod, EMAMiddlePeriod = eMAMiddlePeriod, EMASlowPeriod = eMASlowPeriod, HistoMultiplier = histoMultiplier }, input, ref cacheZombie2MACD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2MACD Zombie2MACD(string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, double histoMultiplier)
		{
			return indicator.Zombie2MACD(Input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, histoMultiplier);
		}

		public Indicators.Zombie2MACD Zombie2MACD(ISeries<double> input , string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, double histoMultiplier)
		{
			return indicator.Zombie2MACD(input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, histoMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2MACD Zombie2MACD(string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, double histoMultiplier)
		{
			return indicator.Zombie2MACD(Input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, histoMultiplier);
		}

		public Indicators.Zombie2MACD Zombie2MACD(ISeries<double> input , string indicatorName, int mACDFastPeriod, int mACDSlowPeriod, int mACDSmoothPeriod, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod, double histoMultiplier)
		{
			return indicator.Zombie2MACD(input, indicatorName, mACDFastPeriod, mACDSlowPeriod, mACDSmoothPeriod, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod, histoMultiplier);
		}
	}
}

#endregion
