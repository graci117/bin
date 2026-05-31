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
	public class Zombie21MACD : Indicator
	{
		private const string SystemVersion = "v1.029";
		private const string SystemName = "Zombie21MACD";
		private const string FullSystemName = SystemName + " - " + SystemVersion;

		private	Series<double>		fastEma;
		private	Series<double>		slowEma;
		private double				constant1;
		private double				constant2;
		private double				constant3;
		private double				constant4;
		private double				constant5;
		private double				constant6;
		
		public override string DisplayName
		{
			get { return FullSystemName; }
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = SystemName;
				Description = SystemName + " " + SystemVersion;
				Calculate = Calculate.OnPriceChange;
				PaintPriceMarkers = false;
				BarsToLoad = 2;
				Fast						= 12;
				IsSuspendedWhileInactive	= true;
				Slow						= 26;
				Smooth						= 9;
				HistoMultiplier = 3;
				PaintBars = false;
				SpecialColorsForMixBars = false;

				//levels
				AddLine(Brushes.DarkGray, 0, NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);

				//histo
				AddPlot(new Stroke(Brushes.Chartreuse, 5), PlotStyle.Bar, "HistoBullStrong"); 
				AddPlot(new Stroke(Brushes.Red, 5),	PlotStyle.Bar,	"HistoBullWeak");
				AddPlot(new Stroke(Brushes.Red, 5), PlotStyle.Bar, "HistoBearStrong");
				AddPlot(new Stroke(Brushes.Chartreuse, 5), PlotStyle.Bar, "HistoBearWeak");

				AddPlot(new Stroke(Brushes.SkyBlue, 5), PlotStyle.Bar, "HistoBullMixedStrong");
				AddPlot(new Stroke(Brushes.RoyalBlue, 5), PlotStyle.Bar, "HistoBullMixedWeak");
				AddPlot(new Stroke(Brushes.Plum, 5), PlotStyle.Bar, "HistoBearMixedStrong");
				AddPlot(new Stroke(Brushes.DarkOrchid, 5), PlotStyle.Bar, "HistoBearMixedWeak");

				//lines
				AddPlot(new Stroke(Brushes.Gray, DashStyleHelper.Solid, 2), PlotStyle.Line, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameMACD);
				AddPlot(new Stroke(Brushes.BlueViolet, DashStyleHelper.Solid, 2), PlotStyle.Line, NinjaTrader.Custom.Resource.NinjaScriptIndicatorAvg);

				//diff histogram
				AddPlot(new Stroke(Brushes.DarkGray, 1), PlotStyle.Line, "Diff");

				Plots[0].AutoWidth = true;
				Plots[1].AutoWidth = true;
				Plots[2].AutoWidth = true;
				Plots[3].AutoWidth = true;
				Plots[4].AutoWidth = true;
				Plots[5].AutoWidth = true;
				Plots[6].AutoWidth = true;
				Plots[7].AutoWidth = true;
				Plots[10].AutoWidth = true;
			}
			else if (State == State.Configure)
			{
				constant1	= 2.0 / (1 + Fast);
				constant2	= (1 - (2.0 / (1 + Fast)));
				constant3	= 2.0 / (1 + Slow);
				constant4	= (1 - (2.0 / (1 + Slow)));
				constant5	= 2.0 / (1 + Smooth);
				constant6	= (1 - (2.0 / (1 + Smooth)));
			}
			else if (State == State.DataLoaded)
			{
				fastEma = new Series<double>(this);
				slowEma = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			double input0	= Input[0];

			if (CurrentBar == 0)
			{
				fastEma[0]		= input0;
				slowEma[0]		= input0;
				Default[0]		= 0;
				Avg[0]			= 0;
				Diff[0]			= 0;
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
				double fastEma0	= constant1 * input0 + constant2 * fastEma[1];
				double slowEma0	= constant3 * input0 + constant4 * slowEma[1];
				double macd		= fastEma0 - slowEma0;
				double macdAvg	= constant5 * macd + constant6 * Avg[1];
				double diff = (macd - macdAvg) * HistoMultiplier;
				double previousMacd = 0;

				DiffBullStrong[0] = 0;
				DiffBullWeak[0] = 0;
				DiffBearStrong[0] = 0;
				DiffBearWeak[0] = 0;
				DiffBullMixedStrong[0] = 0;
				DiffBullMixedWeak[0] = 0;
				DiffBearMixedStrong[0] = 0;
				DiffBearMixedWeak[0] = 0;

				if (DiffBullStrong[1] != 0)
					previousMacd = DiffBullStrong[1];
				else if (DiffBullWeak[1] != 0)
					previousMacd = DiffBullWeak[1];
				else if (DiffBearStrong[1] != 0)
					previousMacd = DiffBearStrong[1];
				else if (DiffBearWeak[1] != 0)
					previousMacd = DiffBearWeak[1];
				else if (DiffBullMixedStrong[1] != 0)
					previousMacd = DiffBullMixedStrong[1];
				else if (DiffBullMixedWeak[1] != 0)
					previousMacd = DiffBullMixedWeak[1];
				else if (DiffBearMixedStrong[1] != 0)
					previousMacd = DiffBearMixedStrong[1];
				else if (DiffBearMixedWeak[1] != 0)
					previousMacd = DiffBearMixedWeak[1];

				fastEma[0]		= fastEma0;
				slowEma[0]		= slowEma0;
				Default[0]		= macd;
				Avg[0]			= macdAvg;
				Diff[0]			= diff;

				if (diff >= 0)
				{
					if (diff > previousMacd)
					{
						if (macdAvg <= 0 && this.SpecialColorsForMixBars)
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
						if (macdAvg <= 0 && this.SpecialColorsForMixBars)
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
					if (diff < previousMacd)
					{
						if (macdAvg >= 0 && this.SpecialColorsForMixBars)
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
						if (macdAvg >= 0 && this.SpecialColorsForMixBars)
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
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Default
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
		{
			get { return Values[9]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Diff
		{
			get { return Values[10]; }
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

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Fast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Slow
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "NinjaScriptParameters", Order = 2)]
		public int Smooth
		{ get; set; }

		[Range(0.7, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HistoMultiplier", GroupName = "NinjaScriptParameters", Order = 3)]
		public double HistoMultiplier
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "PaintBars", GroupName = "NinjaScriptParameters", Order = 4)]
		public bool PaintBars
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "SpecialColorsForMixBars", GroupName = "NinjaScriptParameters", Order = 5)]
		public bool SpecialColorsForMixBars
		{ get; set; }

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie21MACD[] cacheZombie21MACD;
		public Zombie21MACD Zombie21MACD(string indicatorName, int fast, int slow, int smooth, double histoMultiplier)
		{
			return Zombie21MACD(Input, indicatorName, fast, slow, smooth, histoMultiplier);
		}

		public Zombie21MACD Zombie21MACD(ISeries<double> input, string indicatorName, int fast, int slow, int smooth, double histoMultiplier)
		{
			if (cacheZombie21MACD != null)
				for (int idx = 0; idx < cacheZombie21MACD.Length; idx++)
					if (cacheZombie21MACD[idx] != null && cacheZombie21MACD[idx].IndicatorName == indicatorName && cacheZombie21MACD[idx].Fast == fast && cacheZombie21MACD[idx].Slow == slow && cacheZombie21MACD[idx].Smooth == smooth && cacheZombie21MACD[idx].HistoMultiplier == histoMultiplier && cacheZombie21MACD[idx].EqualsInput(input))
						return cacheZombie21MACD[idx];
			return CacheIndicator<Zombie21MACD>(new Zombie21MACD(){ IndicatorName = indicatorName, Fast = fast, Slow = slow, Smooth = smooth, HistoMultiplier = histoMultiplier }, input, ref cacheZombie21MACD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie21MACD Zombie21MACD(string indicatorName, int fast, int slow, int smooth, double histoMultiplier)
		{
			return indicator.Zombie21MACD(Input, indicatorName, fast, slow, smooth, histoMultiplier);
		}

		public Indicators.Zombie21MACD Zombie21MACD(ISeries<double> input , string indicatorName, int fast, int slow, int smooth, double histoMultiplier)
		{
			return indicator.Zombie21MACD(input, indicatorName, fast, slow, smooth, histoMultiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie21MACD Zombie21MACD(string indicatorName, int fast, int slow, int smooth, double histoMultiplier)
		{
			return indicator.Zombie21MACD(Input, indicatorName, fast, slow, smooth, histoMultiplier);
		}

		public Indicators.Zombie21MACD Zombie21MACD(ISeries<double> input , string indicatorName, int fast, int slow, int smooth, double histoMultiplier)
		{
			return indicator.Zombie21MACD(input, indicatorName, fast, slow, smooth, histoMultiplier);
		}
	}
}

#endregion
