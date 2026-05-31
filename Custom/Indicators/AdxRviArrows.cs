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

namespace NinjaTrader.NinjaScript.Indicators.TradeSaber
{
	public class AdxRviArrows : Indicator
	{
		private ADX adx;
		private RVI rvi;
		private int lastSignal = 0; // Track last signal: 0=none, 1=bullish, -1=bearish
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"ADX and RVI Combined Arrow Signals";
				Name										= "AdxRviArrows";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				ShowTransparentPlotsInDataBox 				= true;
				
				ADXPeriod									= 14;
				LowerThreshold								= 25;
				UpperThreshold								= 75;
				
				RVIPeriod									= 14;
				RVIBullishThreshold							= 60;
				RVIBearishThreshold							= 40;
				
				PriceOffset									= 3;
				
				AddPlot(Brushes.Transparent, "CombinedSignal");
			}
			else if (State == State.DataLoaded)
			{
				adx = ADX(ADXPeriod);
				rvi = RVI(RVIPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(ADXPeriod, RVIPeriod))
				return;
			
			double priceOffsetValue = PriceOffset * TickSize;
			
			bool adxInRange = adx[0] >= LowerThreshold && adx[0] <= UpperThreshold;
			bool rviBullish = rvi[0] > RVIBullishThreshold;
			bool rviBearish = rvi[0] < RVIBearishThreshold;
			
			// BULLISH SIGNAL: ADX in range AND RVI bullish
			if (adxInRange && rviBullish)
			{
				CombinedSignal[0] = 1;
				// Only draw arrow if this is a NEW bullish signal
				if (lastSignal != 1)
				{
					Draw.ArrowUp(this, "BullArrow" + CurrentBar, true, 0, Low[0] - priceOffsetValue, Brushes.Lime);
					lastSignal = 1;
				}
			}
			// BEARISH SIGNAL: ADX in range AND RVI bearish
			else if (adxInRange && rviBearish)
			{
				CombinedSignal[0] = -1;
				// Only draw arrow if this is a NEW bearish signal
				if (lastSignal != -1)
				{
					Draw.ArrowDown(this, "BearArrow" + CurrentBar, true, 0, High[0] + priceOffsetValue, Brushes.Red);
					lastSignal = -1;
				}
			}
			// NO SIGNAL
			else
			{
				CombinedSignal[0] = 0;
				// Reset signal when conditions no longer met
				lastSignal = 0;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXPeriod", Order=1, GroupName="ADX Parameters")]
		public int ADXPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LowerThreshold", Order=2, GroupName="ADX Parameters")]
		public int LowerThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="UpperThreshold", Order=3, GroupName="ADX Parameters")]
		public int UpperThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RVIPeriod", Order=1, GroupName="RVI Parameters")]
		public int RVIPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name="RVIBullishThreshold", Order=2, GroupName="RVI Parameters")]
		public int RVIBullishThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name="RVIBearishThreshold", Order=3, GroupName="RVI Parameters")]
		public int RVIBearishThreshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PriceOffset (ticks)", Order=1, GroupName="Display Parameters")]
		public int PriceOffset
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CombinedSignal
		{
			get { return Values[0]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber.AdxRviArrows[] cacheAdxRviArrows;
		public TradeSaber.AdxRviArrows AdxRviArrows(int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold, int priceOffset)
		{
			return AdxRviArrows(Input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold, priceOffset);
		}

		public TradeSaber.AdxRviArrows AdxRviArrows(ISeries<double> input, int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold, int priceOffset)
		{
			if (cacheAdxRviArrows != null)
				for (int idx = 0; idx < cacheAdxRviArrows.Length; idx++)
					if (cacheAdxRviArrows[idx] != null && cacheAdxRviArrows[idx].ADXPeriod == aDXPeriod && cacheAdxRviArrows[idx].LowerThreshold == lowerThreshold && cacheAdxRviArrows[idx].UpperThreshold == upperThreshold && cacheAdxRviArrows[idx].RVIPeriod == rVIPeriod && cacheAdxRviArrows[idx].RVIBullishThreshold == rVIBullishThreshold && cacheAdxRviArrows[idx].RVIBearishThreshold == rVIBearishThreshold && cacheAdxRviArrows[idx].PriceOffset == priceOffset && cacheAdxRviArrows[idx].EqualsInput(input))
						return cacheAdxRviArrows[idx];
			return CacheIndicator<TradeSaber.AdxRviArrows>(new TradeSaber.AdxRviArrows(){ ADXPeriod = aDXPeriod, LowerThreshold = lowerThreshold, UpperThreshold = upperThreshold, RVIPeriod = rVIPeriod, RVIBullishThreshold = rVIBullishThreshold, RVIBearishThreshold = rVIBearishThreshold, PriceOffset = priceOffset }, input, ref cacheAdxRviArrows);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.AdxRviArrows AdxRviArrows(int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold, int priceOffset)
		{
			return indicator.AdxRviArrows(Input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold, priceOffset);
		}

		public Indicators.TradeSaber.AdxRviArrows AdxRviArrows(ISeries<double> input , int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold, int priceOffset)
		{
			return indicator.AdxRviArrows(input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold, priceOffset);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.AdxRviArrows AdxRviArrows(int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold, int priceOffset)
		{
			return indicator.AdxRviArrows(Input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold, priceOffset);
		}

		public Indicators.TradeSaber.AdxRviArrows AdxRviArrows(ISeries<double> input , int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold, int priceOffset)
		{
			return indicator.AdxRviArrows(input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold, priceOffset);
		}
	}
}

#endregion
