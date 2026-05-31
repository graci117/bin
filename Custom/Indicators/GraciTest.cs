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
	public class GraciTest : Indicator
	{
		private RSI RSI1;
		private EMA EMA1;
		private Series<double> RSI1Avg;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "GraciTest";
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
				AddPlot(Brushes.Lime, "EMA1Plot");
				AddPlot(Brushes.Magenta, "RSIAvgPlot");
				RSILength = 12;
				RSIMALength = 43;
			}
			else if (State == State.DataLoaded)
			{
				RSI1 = RSI(Close, Convert.ToInt32(RSILength), 3);
				EMA1 = EMA(RSI1, RSIMALength);
				RSI1Avg = RSI1.Avg;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 10)
				return;
			
			Print(Time[0] + "  RSI1: " + RSI1[0]);
			Print(Time[0] + "  EMA1: " + EMA1[0]);
			
			Values[0][0] = RSI1[0];
			Values[1][0] = EMA1[0];
			Values[2][0] = RSI1Avg[0];
			
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

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RSI1Plot
		{
			get { return Values[0]; }
		}
		


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA1Plot
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
		private GraciTest[] cacheGraciTest;
		public GraciTest GraciTest(int rSILength, int rSIMALength)
		{
			return GraciTest(Input, rSILength, rSIMALength);
		}

		public GraciTest GraciTest(ISeries<double> input, int rSILength, int rSIMALength)
		{
			if (cacheGraciTest != null)
				for (int idx = 0; idx < cacheGraciTest.Length; idx++)
					if (cacheGraciTest[idx] != null && cacheGraciTest[idx].RSILength == rSILength && cacheGraciTest[idx].RSIMALength == rSIMALength && cacheGraciTest[idx].EqualsInput(input))
						return cacheGraciTest[idx];
			return CacheIndicator<GraciTest>(new GraciTest(){ RSILength = rSILength, RSIMALength = rSIMALength }, input, ref cacheGraciTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GraciTest GraciTest(int rSILength, int rSIMALength)
		{
			return indicator.GraciTest(Input, rSILength, rSIMALength);
		}

		public Indicators.GraciTest GraciTest(ISeries<double> input , int rSILength, int rSIMALength)
		{
			return indicator.GraciTest(input, rSILength, rSIMALength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GraciTest GraciTest(int rSILength, int rSIMALength)
		{
			return indicator.GraciTest(Input, rSILength, rSIMALength);
		}

		public Indicators.GraciTest GraciTest(ISeries<double> input , int rSILength, int rSIMALength)
		{
			return indicator.GraciTest(input, rSILength, rSIMALength);
		}
	}
}

#endregion
