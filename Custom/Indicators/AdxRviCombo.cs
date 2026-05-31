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
	public class AdxRviCombo : Indicator
	{
		private ADX adx;
		private RVI rvi;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"ADX and RVI Trend Filter Combo";
				Name										= "AdxRviCombo";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
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
				
				AddPlot(Brushes.Transparent, "ADXValue");
				AddPlot(Brushes.Transparent, "RVIValue");
				AddPlot(Brushes.Transparent, "RVISignal");
				AddPlot(Brushes.Transparent, "ADXSignal");
				
				AddLine(Brushes.Transparent, 25, "LowerLine");
				AddLine(Brushes.Transparent, 75, "UpperLine");
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
			
			ADXValue[0] = adx[0];
			RVIValue[0] = rvi[0];
			
			// RVI DOTS - plotted at horizontal level 3
			// LIME DOT: RVI > 60 (value = +2)
			if (rvi[0] > RVIBullishThreshold)
			{
				RVISignal[0] = 2;
				Draw.Dot(this, "RviLime" + CurrentBar, false, 0, 3, Brushes.Lime);
			}
			// RED DOT: RVI < 40 (value = -2)
			else if (rvi[0] < RVIBearishThreshold)
			{
				RVISignal[0] = -2;
				Draw.Dot(this, "RviRed" + CurrentBar, false, 0, 3, Brushes.Red);
			}
			// BLACK CROSS: RVI between 40-60 (no signal)
			else
			{
				RVISignal[0] = 0;
				Draw.Text(this, "RviCross" + CurrentBar, false, "X", 0, 3, 0, Brushes.Black, new SimpleFont("Arial", 10), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}
			
			// ADX DOT - plotted at horizontal level 0
			// LIME DOT: ADX between LowerThreshold-UpperThreshold (value = +3)
			bool adxInRange = adx[0] >= LowerThreshold && adx[0] <= UpperThreshold;
			if (adxInRange)
			{
				ADXSignal[0] = 3;
				Draw.Dot(this, "AdxLime" + CurrentBar, false, 0, 0, Brushes.Lime);
			}
			// BLACK CROSS: ADX outside range (value = -3)
			else
			{
				ADXSignal[0] = -3;
				Draw.Text(this, "AdxCross" + CurrentBar, false, "X", 0, 0, 0, Brushes.Black, new SimpleFont("Arial", 10), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
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

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ADXValue
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RVIValue
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RVISignal
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ADXSignal
		{
			get { return Values[3]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber.AdxRviCombo[] cacheAdxRviCombo;
		public TradeSaber.AdxRviCombo AdxRviCombo(int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold)
		{
			return AdxRviCombo(Input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold);
		}

		public TradeSaber.AdxRviCombo AdxRviCombo(ISeries<double> input, int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold)
		{
			if (cacheAdxRviCombo != null)
				for (int idx = 0; idx < cacheAdxRviCombo.Length; idx++)
					if (cacheAdxRviCombo[idx] != null && cacheAdxRviCombo[idx].ADXPeriod == aDXPeriod && cacheAdxRviCombo[idx].LowerThreshold == lowerThreshold && cacheAdxRviCombo[idx].UpperThreshold == upperThreshold && cacheAdxRviCombo[idx].RVIPeriod == rVIPeriod && cacheAdxRviCombo[idx].RVIBullishThreshold == rVIBullishThreshold && cacheAdxRviCombo[idx].RVIBearishThreshold == rVIBearishThreshold && cacheAdxRviCombo[idx].EqualsInput(input))
						return cacheAdxRviCombo[idx];
			return CacheIndicator<TradeSaber.AdxRviCombo>(new TradeSaber.AdxRviCombo(){ ADXPeriod = aDXPeriod, LowerThreshold = lowerThreshold, UpperThreshold = upperThreshold, RVIPeriod = rVIPeriod, RVIBullishThreshold = rVIBullishThreshold, RVIBearishThreshold = rVIBearishThreshold }, input, ref cacheAdxRviCombo);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.AdxRviCombo AdxRviCombo(int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold)
		{
			return indicator.AdxRviCombo(Input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold);
		}

		public Indicators.TradeSaber.AdxRviCombo AdxRviCombo(ISeries<double> input , int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold)
		{
			return indicator.AdxRviCombo(input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.AdxRviCombo AdxRviCombo(int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold)
		{
			return indicator.AdxRviCombo(Input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold);
		}

		public Indicators.TradeSaber.AdxRviCombo AdxRviCombo(ISeries<double> input , int aDXPeriod, int lowerThreshold, int upperThreshold, int rVIPeriod, int rVIBullishThreshold, int rVIBearishThreshold)
		{
			return indicator.AdxRviCombo(input, aDXPeriod, lowerThreshold, upperThreshold, rVIPeriod, rVIBullishThreshold, rVIBearishThreshold);
		}
	}
}

#endregion
