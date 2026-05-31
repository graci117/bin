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

namespace NinjaTrader.NinjaScript.Indicators.Infinity
{
	public class HalfTrend : Indicator
	{
		private Series<int>    currTrend;
		private Series<int>    nextTrend;
		private Series<double> maxLoPrice;
		private Series<double> minHiPrice;
		private Series<double> up;
		private Series<double> dn;
		
		double atrHi      = 0.0;
		double atrLo      = 0.0;
		double atrValue   = 0.0;
		double devValue   = 0.0;
		double loPrice    = 0.0;
		double hiPrice    = 0.0;
		double hiSmaValue = 0.0;
		double loSmaValue = 0.0;
		
		protected override void OnStateChange()
		{
			if(State == State.SetDefaults)
			{
				Description					= @"";
				Name						= "HalfTrend";
				Calculate					= Calculate.OnPriceChange;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				PaintPriceMarkers			= false;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= false;
				
				Amplitude					= 2;
				Deviation					= 2;
				AtrLength					= 100;
				ShowChannels				= true;
				ShowSignals					= true;
				AreaOpacity					= 10;
				UpBrush						= Brushes.LightGreen;
				DnBrush						= Brushes.LightCoral;
				
				AddPlot(new Stroke(Brushes.DimGray, 2), PlotStyle.Line, "Trend");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "LowerChannel");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Line, "UpperChannel");
				AddPlot(new Stroke(UpBrush, 5), PlotStyle.TriangleUp, "UpSignal");
				AddPlot(new Stroke(DnBrush, 5), PlotStyle.TriangleDown, "DnSignal");
				
				Plots[1].DashStyleHelper = DashStyleHelper.Dot;
				Plots[2].DashStyleHelper = DashStyleHelper.Dot;
			}
			else if(State == State.Configure)
			{
				currTrend  = new Series<int>(this, MaximumBarsLookBack.Infinite);
				nextTrend  = new Series<int>(this, MaximumBarsLookBack.Infinite);
				maxLoPrice = new Series<double>(this, MaximumBarsLookBack.Infinite);
				minHiPrice = new Series<double>(this, MaximumBarsLookBack.Infinite);
				up 		   = new Series<double>(this, MaximumBarsLookBack.Infinite);
				dn 		   = new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1)
			{
				return;
			}
			
			if(CurrentBar < AtrLength)
			{
				currTrend[0]  = 0;
				nextTrend[0]  = 0;
				maxLoPrice[0] = Low[1];
				minHiPrice[0] = High[1];
				up[0] 		  = Close[1];
				dn[0] 		  = Close[1];
					
				return;
			}
			
			up[0] 		  = up[1];
			dn[0] 		  = dn[1];
			currTrend[0]  = currTrend[1];
			nextTrend[0]  = nextTrend[1];
			minHiPrice[0] = minHiPrice[1];
			maxLoPrice[0] = maxLoPrice[1];
			
			atrHi      = 0.0;
			atrLo      = 0.0;
			atrValue   = ATR(AtrLength)[0] / 2.0;
			devValue   = Deviation * atrValue;
			loPrice    = MIN(Low, Amplitude)[0];
			hiPrice    = MAX(High, Amplitude)[0];
			hiSmaValue = SMA(High, Amplitude)[0];
			loSmaValue = SMA(Low, Amplitude)[0];
			
			if(nextTrend[0] == 1)
			{
				maxLoPrice[0] = Math.Max(loPrice, maxLoPrice[1]);
				
				if(hiSmaValue < maxLoPrice[1] && Close[0] < Low[1])
				{
					currTrend[0]  = 1;
					nextTrend[0]  = 0;
					minHiPrice[0] = hiPrice;
				}
				else
				{
					currTrend[0]  = 0;
					nextTrend[0]  = 1;
					minHiPrice[0] = minHiPrice[1];
				}
			}
			else if(nextTrend[0] == 0)
			{
				minHiPrice[0] = Math.Min(hiPrice, minHiPrice[1]);
				
				if(loSmaValue > minHiPrice[1] && Close[0] > High[1])
				{
					currTrend[0]  = 0;
					nextTrend[0]  = 1;
					maxLoPrice[0] = loPrice;
				}
				else
				{
					currTrend[0]  = 1;
					nextTrend[0]  = 0;
					maxLoPrice[0] = maxLoPrice[1];
				}
			}
			
			if(currTrend[0] == 0)
			{
				if(currTrend[1] != 0)
				{
					up[0] = dn[1];
				}
				else
				{
					up[0] = Math.Max(maxLoPrice[0], up[1]);
				}
				
				dn[0] = 0;
				atrHi = up[0] + devValue;
				atrLo = up[0] - devValue;
			}
			else if(currTrend[0] == 1)
			{
				if(currTrend[1] != 1)
				{
					dn[0] = up[1];
				}
				else
				{
					dn[0] = Math.Min(minHiPrice[0], dn[1]);
				}
				
				up[0] = 0;
				atrHi = dn[0] + devValue;
				atrLo = dn[0] - devValue;
			}
			
			Trend[0] = (currTrend[0] == 0) ? up[0] : dn[0];
			PlotBrushes[0][0] = (currTrend[0] == 0) ? UpBrush : DnBrush;
			
			if(ShowSignals)
			{
				UpSignal.Reset(0);
				DnSignal.Reset(0);
				
				if(currTrend[1] != 0 && currTrend[0] == 0)
				{
					UpSignal[0] = atrLo;
				}
				if(currTrend[1] == 0 && currTrend[0] != 0)
				{
					DnSignal[0] = atrHi;
				}
			}
			
			if(ShowChannels)
			{
				LowerChannel[0] = atrLo;
				UpperChannel[0] = atrHi;
				
				if(currTrend[0] == 0)
				{
					PlotBrushes[1][0] = UpBrush;
					PlotBrushes[2][0] = Brushes.Transparent;
					
					if(AreaOpacity > 0)
					{
						Draw.Region(this, "htArea"+CurrentBar, 1, 0, Trend, LowerChannel, null, UpBrush, AreaOpacity);
					}
				}
				else
				{
					PlotBrushes[1][0] = Brushes.Transparent;
					PlotBrushes[2][0] = DnBrush;
					
					if(AreaOpacity > 0)
					{
						Draw.Region(this, "htArea"+CurrentBar, 1, 0, Trend, UpperChannel, null, DnBrush, AreaOpacity);
					}
				}
			}
		}
		

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerChannel
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperChannel
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpSignal
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DnSignal
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<int> CurrTrend
		{
			get { return currTrend; }
		}
		
		/// ---
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amplitude", Order=1, GroupName="Parameters")]
		public int Amplitude
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Deviation", Order=2, GroupName="Parameters")]
		public double Deviation
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATR Length", Order=3, GroupName="Parameters")]
		public int AtrLength
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Channels", Order=4, GroupName="Parameters")]
		public bool ShowChannels
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Show Signals", Order=5, GroupName="Parameters")]
		public bool ShowSignals
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Area Opacity", Order=6, GroupName="Parameters")]
		public int AreaOpacity
		{ get; set; }
		
		[XmlIgnore()]
		[Display(Name = "Up Color", GroupName="Colors", Order=1)]
		public Brush UpBrush
		{ get; set; }

		[Browsable(false)]
		public string UpBrushSerialize
		{
			get { return Serialize.BrushToString(UpBrush); }
   			set { UpBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[Display(Name = "Down Color", GroupName="Colors", Order=2)]
		public Brush DnBrush
		{ get; set; }

		[Browsable(false)]
		public string DnBrushSerialize
		{
			get { return Serialize.BrushToString(DnBrush); }
   			set { DnBrush = Serialize.StringToBrush(value); }
		}
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Infinity.HalfTrend[] cacheHalfTrend;
		public Infinity.HalfTrend HalfTrend(int amplitude, double deviation, int atrLength, bool showChannels, bool showSignals, int areaOpacity)
		{
			return HalfTrend(Input, amplitude, deviation, atrLength, showChannels, showSignals, areaOpacity);
		}

		public Infinity.HalfTrend HalfTrend(ISeries<double> input, int amplitude, double deviation, int atrLength, bool showChannels, bool showSignals, int areaOpacity)
		{
			if (cacheHalfTrend != null)
				for (int idx = 0; idx < cacheHalfTrend.Length; idx++)
					if (cacheHalfTrend[idx] != null && cacheHalfTrend[idx].Amplitude == amplitude && cacheHalfTrend[idx].Deviation == deviation && cacheHalfTrend[idx].AtrLength == atrLength && cacheHalfTrend[idx].ShowChannels == showChannels && cacheHalfTrend[idx].ShowSignals == showSignals && cacheHalfTrend[idx].AreaOpacity == areaOpacity && cacheHalfTrend[idx].EqualsInput(input))
						return cacheHalfTrend[idx];
			return CacheIndicator<Infinity.HalfTrend>(new Infinity.HalfTrend(){ Amplitude = amplitude, Deviation = deviation, AtrLength = atrLength, ShowChannels = showChannels, ShowSignals = showSignals, AreaOpacity = areaOpacity }, input, ref cacheHalfTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Infinity.HalfTrend HalfTrend(int amplitude, double deviation, int atrLength, bool showChannels, bool showSignals, int areaOpacity)
		{
			return indicator.HalfTrend(Input, amplitude, deviation, atrLength, showChannels, showSignals, areaOpacity);
		}

		public Indicators.Infinity.HalfTrend HalfTrend(ISeries<double> input , int amplitude, double deviation, int atrLength, bool showChannels, bool showSignals, int areaOpacity)
		{
			return indicator.HalfTrend(input, amplitude, deviation, atrLength, showChannels, showSignals, areaOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Infinity.HalfTrend HalfTrend(int amplitude, double deviation, int atrLength, bool showChannels, bool showSignals, int areaOpacity)
		{
			return indicator.HalfTrend(Input, amplitude, deviation, atrLength, showChannels, showSignals, areaOpacity);
		}

		public Indicators.Infinity.HalfTrend HalfTrend(ISeries<double> input , int amplitude, double deviation, int atrLength, bool showChannels, bool showSignals, int areaOpacity)
		{
			return indicator.HalfTrend(input, amplitude, deviation, atrLength, showChannels, showSignals, areaOpacity);
		}
	}
}

#endregion
