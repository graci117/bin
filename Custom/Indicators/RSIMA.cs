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
	public class RSIMA : Indicator
	{
		private RSI RSI1;
		private Series<double> EMA1;
		private Series<double> RSI1Avg;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RSIMA";
				Calculate									= Calculate.OnBarClose;
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
				AddPlot(Brushes.Blue, "RSI1Plot");
				AddPlot(Brushes.Lime, "RSIEMAPlot");
				AddPlot(Brushes.Magenta, "RSIAvgPlot");
				RSILength = 12;
				RSIMALength = 43;
				RSIMAType = "EMA";
			}
			else if (State == State.DataLoaded)
			{
				RSI1 = RSI(Close, Convert.ToInt32(RSILength), 3);
				if (RSIMAType  ==  "EMA")
					EMA1 = EMA(RSI1, RSIMALength).Value;
				else
					EMA1 = SMA(RSI1, RSIMALength).Value;
				RSI1Avg = RSI1.Avg;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 10)
				return;
			
//			Print(Time[0] + "  RSI1: " + RSI1[0]);
//			Print(Time[0] + "  EMA1: " + EMA1[0]);
			
			Values[0][0] = RSI1[0];
			Values[1][0] = EMA1[0];
			Values[2][0] = RSI1.Avg[0];
			
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSILength", Order=1, GroupName="Parameters")]
		public int RSILength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSIMALength", Order=2, GroupName="Parameters")]
		public int RSIMALength
		{ get; set; }
		
		[NinjaScriptProperty]		
		[Display(Name="RSIMAType", Order=3, GroupName="Parameters")]
		public string RSIMAType
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RSI1Plot
		{
			get { return Values[0]; }
		}
		


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RSIEMAPlot
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RSIAvgPlot
		{
			get { return Values[2]; }
		}
		
		
		#endregion

	}

	
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RSIMA[] cacheRSIMA;
		public RSIMA RSIMA(int rSILength, int rSIMALength, string rSIMAType)
		{
			return RSIMA(Input, rSILength, rSIMALength, rSIMAType);
		}

		public RSIMA RSIMA(ISeries<double> input, int rSILength, int rSIMALength, string rSIMAType)
		{
			if (cacheRSIMA != null)
				for (int idx = 0; idx < cacheRSIMA.Length; idx++)
					if (cacheRSIMA[idx] != null && cacheRSIMA[idx].RSILength == rSILength && cacheRSIMA[idx].RSIMALength == rSIMALength && cacheRSIMA[idx].RSIMAType == rSIMAType && cacheRSIMA[idx].EqualsInput(input))
						return cacheRSIMA[idx];
			return CacheIndicator<RSIMA>(new RSIMA(){ RSILength = rSILength, RSIMALength = rSIMALength, RSIMAType = rSIMAType }, input, ref cacheRSIMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSIMA RSIMA(int rSILength, int rSIMALength, string rSIMAType)
		{
			return indicator.RSIMA(Input, rSILength, rSIMALength, rSIMAType);
		}

		public Indicators.RSIMA RSIMA(ISeries<double> input , int rSILength, int rSIMALength, string rSIMAType)
		{
			return indicator.RSIMA(input, rSILength, rSIMALength, rSIMAType);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSIMA RSIMA(int rSILength, int rSIMALength, string rSIMAType)
		{
			return indicator.RSIMA(Input, rSILength, rSIMALength, rSIMAType);
		}

		public Indicators.RSIMA RSIMA(ISeries<double> input , int rSILength, int rSIMALength, string rSIMAType)
		{
			return indicator.RSIMA(input, rSILength, rSIMALength, rSIMAType);
		}
	}
}

#endregion
