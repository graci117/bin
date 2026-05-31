
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
namespace NinjaTrader.NinjaScript.Indicators.CampervanSeth
{
	
	[Gui.CategoryOrder("Williams %R", 1)]
    [Gui.CategoryOrder("Colors", 2)]
    [Gui.CategoryOrder("SMA Parameters", 3)]
	
	
	
	
	
	public class WilliamsRColorBars : Indicator
	{
		private MAX max;
		private MIN min;
		
		private SMA sma50;  // 50-period SMA
		private Series<double> signal;
		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "The Williams %R is a momentum indicator that is designed to identify overbought and oversold areas in a nontrending market. When combined with the 50 SMA, this prints entrys by coloring the bars based on the user's threshold";
				Name						= "Williams R Color Bars";
				IsSuspendedWhileInactive	= true; 
				Period						= 14;
				IsOverlay					= false; // Williams %R should be in a separate panel
				LongThresh					= 40;
				ShortThresh					= 60;

				SmaPeriod					= 50; // Default SMA period
				SMAThick					= 4;
				
				LongColor					= Brushes.Lime;
				ShortColor					= Brushes.Red;
				NeutralColor				= Brushes.DarkGray;
				
				PlotSMA						= true;
				
				AddLine(Brushes.DarkGreen,	20,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddLine(Brushes.Maroon,	80,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddLine(Brushes.DarkGray,	50,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorMiddle);
				AddPlot(new Stroke(Brushes.Gold, 3), PlotStyle.Bar,NinjaTrader.Custom.Resource.WilliamsPercentR);
				Plots[0].AutoWidth = true;
				//AddPlot(Brushes.Transparent, "SMA50"); // Invisible by default, SMA will be drawn manually
				
				
			}
			else if (State == State.DataLoaded)
			{
				max = MAX(High, Period);
				min	= MIN(Low, Period);
				sma50 = SMA(Close, SmaPeriod);
				
				
			} else if (State == State.Configure) {
				signal = new Series<double>(this);
			}
		}

        private void DrawText(float x, float y, string text, int fontSize, SolidColorBrush br)
        {
        }

        protected override void OnBarUpdate()
		{
			if (CurrentBar < SmaPeriod) return;  // Ensure enough bars for SMA calculation

			// Williams %R Calculation
			double max0 = max[0];
			double min0 = min[0];
			Values[0][0] = 100 * (max0 - Close[0]) / (max0 - min0 == 0 ? 1 : max0 - min0); // Williams %R
			Plots[0].Width = 3;
			
			// Determine SMA direction (rising or falling)
			double smaCurrent = sma50[0];
			double smaPrevious = sma50[1];
			bool isSmaRising = smaCurrent > smaPrevious;
			Brush smaColor = isSmaRising ? LongColor : ShortColor;

			// Draw SMA on the main price panel (Panel 1)
			if (PlotSMA)
				{
				DrawOnPricePanel = true;
				Draw.Line(this, "SMA50_" + CurrentBar, false, 1, smaPrevious, 0, smaCurrent, smaColor, DashStyleHelper.Solid, SMAThick);
					}

			// Enhanced Bar Coloring Logic
			if (Values[0][0] < LongThresh && isSmaRising)  // Williams %R is bullish & SMA is rising
				
			
			{
				BarBrushes[0] = LongColor;
				CandleOutlineBrushes[0] = LongColor;
				PlotBrushes[0][0] = LongColor;
				signal[0] = 1;
			}
			else if (Values[0][0] > ShortThresh && !isSmaRising)  // Williams %R is bearish & SMA is falling
			{
				BarBrushes[0] = ShortColor;
				CandleOutlineBrushes[0] = ShortColor;
				PlotBrushes[0][0] = ShortColor;
				signal[0] = -1;
			}
			else
			{
				BarBrushes[0] = NeutralColor; // Neutral color when conditions aren’t met
				PlotBrushes[0][0] = NeutralColor;
				signal[0] = 0;
			}
			
			
			
			
			
			
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Williams %R", Order = 0)]
		public int Period
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Long Threshold", GroupName = "Williams %R", Order = 1)]
		public int LongThresh
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Short Threshold", GroupName = "Williams %R", Order = 2)]
		public int ShortThresh
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Plot the SMA", GroupName = "SMA Parameters", Order = 2)]
		public bool PlotSMA { get; set; }			
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "SMA Period", GroupName = "SMA Parameters", Order = 3)]
		public int SmaPeriod { get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(Name = "SMA Thickness", GroupName = "SMA Parameters", Order = 4)]
		public int SMAThick { get; set; }
		
		[Browsable(false)] [XmlIgnore] public Series<double> Signal				{ get { return signal; }}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Long Color", Description="Long Color", Order=1, GroupName="Colors")]
		public Brush LongColor
		{ get; set; }
		
		[Browsable(false)]
		public string LongColorSerializable
				{
			get { return Serialize.BrushToString(LongColor); }

			set { LongColor = Serialize.StringToBrush(value); }
				}	
				
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Short Color", Description="Short Color", Order=2, GroupName="Colors")]
		public Brush ShortColor
		{ get; set; }
		
		[Browsable(false)]
		public string ShortColorSerializable
				{
			get { return Serialize.BrushToString(ShortColor); }

			set { ShortColor = Serialize.StringToBrush(value); }
				}	
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Neutral Color", Description="Neutral Color", Order=3, GroupName="Colors")]
		public Brush NeutralColor
		{ get; set; }
		
		[Browsable(false)]
		public string NeutralColorSerializable
				{
			get { return Serialize.BrushToString(NeutralColor); }

			set { NeutralColor = Serialize.StringToBrush(value); }
				}	
		
		#endregion
				
				
				// In order to trim the indicator's label on the chart we need to override the ToString() method.
			public override string DisplayName
				{
		            get { return Name ;}
				}		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CampervanSeth.WilliamsRColorBars[] cacheWilliamsRColorBars;
		public CampervanSeth.WilliamsRColorBars WilliamsRColorBars(int period, int longThresh, int shortThresh, bool plotSMA, int smaPeriod, int sMAThick, Brush longColor, Brush shortColor, Brush neutralColor)
		{
			return WilliamsRColorBars(Input, period, longThresh, shortThresh, plotSMA, smaPeriod, sMAThick, longColor, shortColor, neutralColor);
		}

		public CampervanSeth.WilliamsRColorBars WilliamsRColorBars(ISeries<double> input, int period, int longThresh, int shortThresh, bool plotSMA, int smaPeriod, int sMAThick, Brush longColor, Brush shortColor, Brush neutralColor)
		{
			if (cacheWilliamsRColorBars != null)
				for (int idx = 0; idx < cacheWilliamsRColorBars.Length; idx++)
					if (cacheWilliamsRColorBars[idx] != null && cacheWilliamsRColorBars[idx].Period == period && cacheWilliamsRColorBars[idx].LongThresh == longThresh && cacheWilliamsRColorBars[idx].ShortThresh == shortThresh && cacheWilliamsRColorBars[idx].PlotSMA == plotSMA && cacheWilliamsRColorBars[idx].SmaPeriod == smaPeriod && cacheWilliamsRColorBars[idx].SMAThick == sMAThick && cacheWilliamsRColorBars[idx].LongColor == longColor && cacheWilliamsRColorBars[idx].ShortColor == shortColor && cacheWilliamsRColorBars[idx].NeutralColor == neutralColor && cacheWilliamsRColorBars[idx].EqualsInput(input))
						return cacheWilliamsRColorBars[idx];
			return CacheIndicator<CampervanSeth.WilliamsRColorBars>(new CampervanSeth.WilliamsRColorBars(){ Period = period, LongThresh = longThresh, ShortThresh = shortThresh, PlotSMA = plotSMA, SmaPeriod = smaPeriod, SMAThick = sMAThick, LongColor = longColor, ShortColor = shortColor, NeutralColor = neutralColor }, input, ref cacheWilliamsRColorBars);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CampervanSeth.WilliamsRColorBars WilliamsRColorBars(int period, int longThresh, int shortThresh, bool plotSMA, int smaPeriod, int sMAThick, Brush longColor, Brush shortColor, Brush neutralColor)
		{
			return indicator.WilliamsRColorBars(Input, period, longThresh, shortThresh, plotSMA, smaPeriod, sMAThick, longColor, shortColor, neutralColor);
		}

		public Indicators.CampervanSeth.WilliamsRColorBars WilliamsRColorBars(ISeries<double> input , int period, int longThresh, int shortThresh, bool plotSMA, int smaPeriod, int sMAThick, Brush longColor, Brush shortColor, Brush neutralColor)
		{
			return indicator.WilliamsRColorBars(input, period, longThresh, shortThresh, plotSMA, smaPeriod, sMAThick, longColor, shortColor, neutralColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CampervanSeth.WilliamsRColorBars WilliamsRColorBars(int period, int longThresh, int shortThresh, bool plotSMA, int smaPeriod, int sMAThick, Brush longColor, Brush shortColor, Brush neutralColor)
		{
			return indicator.WilliamsRColorBars(Input, period, longThresh, shortThresh, plotSMA, smaPeriod, sMAThick, longColor, shortColor, neutralColor);
		}

		public Indicators.CampervanSeth.WilliamsRColorBars WilliamsRColorBars(ISeries<double> input , int period, int longThresh, int shortThresh, bool plotSMA, int smaPeriod, int sMAThick, Brush longColor, Brush shortColor, Brush neutralColor)
		{
			return indicator.WilliamsRColorBars(input, period, longThresh, shortThresh, plotSMA, smaPeriod, sMAThick, longColor, shortColor, neutralColor);
		}
	}
}

#endregion
