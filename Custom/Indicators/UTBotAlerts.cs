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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class UTBotAlerts : Indicator
	{
		Series<double> xATRTrailingStop;
        int position = 0;
		ATR atr;
		double nLoss;
		EMA ema1;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "UTBotAlerts";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ATRMultiplier					= 1.0;
				ATRPeriod					= 10;
				HeikenAshiSignals					= false;
				MarkerUpColor								= Brushes.Green;
				MarkerDownColor								= Brushes.Red;	
				
				AddPlot(Brushes.Transparent, 	"CrossDetect");
				
			}
			else if (State == State.Configure)
			{
				xATRTrailingStop	= new Series<double>(this);
				 atr = ATR(ATRPeriod);
				ema1 = EMA(Close, 1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			//Add your custom indicator logic here.
			
			CrossDetect[0] 	= 0;
			
           	nLoss = ATRMultiplier * atr[0];
			
			xATRTrailingStop[0] = Close[0] > xATRTrailingStop[1] && Close[1] > xATRTrailingStop[1]
								?Math.Max(xATRTrailingStop[1], Close[0] - nLoss)
					                : Close[0] < xATRTrailingStop[1] && Close[1] < xATRTrailingStop[1]
					                    ? Math.Min(xATRTrailingStop[1], Close[0] + nLoss)
					                    : Close[0] > xATRTrailingStop[1] ? Close[0] - nLoss : Close[0] + nLoss;
			
			position = Close[1] < xATRTrailingStop[1] && Close[0] > xATRTrailingStop[1] ? 1 :
                       Close[1] > xATRTrailingStop[1] && Close[0] < xATRTrailingStop[1] ? -1 : position;	
			
			
			
			bool above = Close[0] > xATRTrailingStop[0] && CrossAbove(ema1, xATRTrailingStop, 1);
            bool below = Close[0] < xATRTrailingStop[0] && CrossAbove(xATRTrailingStop, ema1, 1);
			
			bool buy = Close[0]  > xATRTrailingStop[0] && above;
            bool sell = Close[0]  < xATRTrailingStop[0] && below;
			
			if (buy)
				CrossDetect[0] 	= 1;	
			if (sell)
				CrossDetect[0] 	= 2;	
			
			if (CrossDetect[0] == 1 && CrossDetect[1] !=1)
			{
				Draw.ArrowUp (this, "UpArrow"+CurrentBar, true, 0, Low[0] - 5 * TickSize, MarkerUpColor);
			}
			if (CrossDetect[0] == 2 && CrossDetect[1] !=2)
			{
				Draw.ArrowDown (this, "DwnArrow"+CurrentBar, true, 0, High[0] + 5 * TickSize, MarkerDownColor);
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATRMultiplier", Description="ATR Multiplier", Order=1, GroupName="Parameters")]
		public double ATRMultiplier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATRPeriod", Description="ATR Period", Order=2, GroupName="Parameters")]
		public int ATRPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="HeikenAshiSignals", Description="Heiken Ashi Signals", Order=3, GroupName="Parameters")]
		public bool HeikenAshiSignals
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CrossDetect
		{
			get { return Values[0]; }
		}
		
				[XmlIgnore]
		[Display(Name="CrossAbove Marker color", Description="Color of marker to show croass above", Order=23, GroupName="Cross Detection Actions")]
		public Brush MarkerUpColor
		{ get; set; }
		
		[Browsable(false)]
		public string MarkerUpColorSerializable
		{
			get { return Serialize.BrushToString(MarkerUpColor); }
			set { MarkerUpColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="CrossBelow Marker color", Description="Color of marker to show cross below", Order=24, GroupName="Cross Detection Actions")]
		public Brush MarkerDownColor
		{ get; set; }
		
		[Browsable(false)]
		public string MarkerDowColorSerializable
		{
			get { return Serialize.BrushToString(MarkerDownColor); }
			set { MarkerDownColor = Serialize.StringToBrush(value); }
		}

		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private UTBotAlerts[] cacheUTBotAlerts;
		public UTBotAlerts UTBotAlerts(double aTRMultiplier, int aTRPeriod, bool heikenAshiSignals)
		{
			return UTBotAlerts(Input, aTRMultiplier, aTRPeriod, heikenAshiSignals);
		}

		public UTBotAlerts UTBotAlerts(ISeries<double> input, double aTRMultiplier, int aTRPeriod, bool heikenAshiSignals)
		{
			if (cacheUTBotAlerts != null)
				for (int idx = 0; idx < cacheUTBotAlerts.Length; idx++)
					if (cacheUTBotAlerts[idx] != null && cacheUTBotAlerts[idx].ATRMultiplier == aTRMultiplier && cacheUTBotAlerts[idx].ATRPeriod == aTRPeriod && cacheUTBotAlerts[idx].HeikenAshiSignals == heikenAshiSignals && cacheUTBotAlerts[idx].EqualsInput(input))
						return cacheUTBotAlerts[idx];
			return CacheIndicator<UTBotAlerts>(new UTBotAlerts(){ ATRMultiplier = aTRMultiplier, ATRPeriod = aTRPeriod, HeikenAshiSignals = heikenAshiSignals }, input, ref cacheUTBotAlerts);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.UTBotAlerts UTBotAlerts(double aTRMultiplier, int aTRPeriod, bool heikenAshiSignals)
		{
			return indicator.UTBotAlerts(Input, aTRMultiplier, aTRPeriod, heikenAshiSignals);
		}

		public Indicators.UTBotAlerts UTBotAlerts(ISeries<double> input , double aTRMultiplier, int aTRPeriod, bool heikenAshiSignals)
		{
			return indicator.UTBotAlerts(input, aTRMultiplier, aTRPeriod, heikenAshiSignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.UTBotAlerts UTBotAlerts(double aTRMultiplier, int aTRPeriod, bool heikenAshiSignals)
		{
			return indicator.UTBotAlerts(Input, aTRMultiplier, aTRPeriod, heikenAshiSignals);
		}

		public Indicators.UTBotAlerts UTBotAlerts(ISeries<double> input , double aTRMultiplier, int aTRPeriod, bool heikenAshiSignals)
		{
			return indicator.UTBotAlerts(input, aTRMultiplier, aTRPeriod, heikenAshiSignals);
		}
	}
}

#endregion
