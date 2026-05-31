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
	public class VisEMA : Indicator
	{
		private EMA EMA1; 
		private int 				period 						= 5;
		private int					periodEMA					= 20;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "VisEMA";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(Brushes.LightCoral, "EMAPlot");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, period); //add our secondary 5 minute data series for calculating the EMA
			}
			else if (State == State.DataLoaded)
			{
				EMA1 = EMA(BarsArray[1], periodEMA);  //set EMA here so we make sure it's calculated on the secondary data series
			}
			
		}
		protected override void OnBarUpdate()
		{
			
			if (CurrentBars[0] < 1 || CurrentBars[1] < periodEMA) // make sure there's at least one bar for primary series and at least 20 of the secondary series prior to processing
				return;
			if(BarsInProgress == 0) // if OnBarUpdate was called from the primary bar series, then set the current value to the latest EMA1 value
				Value[0] = EMA1[0];	
		}
		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMAPlot
		{
			get { return Values[0]; }
		}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", Description = "Serial period", GroupName = "Input Parameters", Order = 1)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
		}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period EMA", Description = "Serial period EMA", GroupName = "Input Parameters", Order = 2)]
		public int PeriodEMA
		{	
            get { return periodEMA; }
            set { periodEMA = value; }
		}			
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VisEMA[] cacheVisEMA;
		public VisEMA VisEMA(int period, int periodEMA)
		{
			return VisEMA(Input, period, periodEMA);
		}

		public VisEMA VisEMA(ISeries<double> input, int period, int periodEMA)
		{
			if (cacheVisEMA != null)
				for (int idx = 0; idx < cacheVisEMA.Length; idx++)
					if (cacheVisEMA[idx] != null && cacheVisEMA[idx].Period == period && cacheVisEMA[idx].PeriodEMA == periodEMA && cacheVisEMA[idx].EqualsInput(input))
						return cacheVisEMA[idx];
			return CacheIndicator<VisEMA>(new VisEMA(){ Period = period, PeriodEMA = periodEMA }, input, ref cacheVisEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VisEMA VisEMA(int period, int periodEMA)
		{
			return indicator.VisEMA(Input, period, periodEMA);
		}

		public Indicators.VisEMA VisEMA(ISeries<double> input , int period, int periodEMA)
		{
			return indicator.VisEMA(input, period, periodEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VisEMA VisEMA(int period, int periodEMA)
		{
			return indicator.VisEMA(Input, period, periodEMA);
		}

		public Indicators.VisEMA VisEMA(ISeries<double> input , int period, int periodEMA)
		{
			return indicator.VisEMA(input, period, periodEMA);
		}
	}
}

#endregion
