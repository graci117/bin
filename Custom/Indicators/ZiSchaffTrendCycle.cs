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
	public class ZiSchaffTrendCycle : Indicator
	{
		private Series<double> sFrac1;
		private Series<double> sFrac2;
		private Series<double> sPF;
		private Series<double> sPFF;

		private MACD sMACD;
		private MIN sMIN;
		private MAX sMAX;
		private MIN pMIN;
		private MAX pMAX;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Schaff Trend Cycle";
				Name										= "ZiSchaffTrendCycle";
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

				MACDFast = 12;
				MACDSlow = 26;
				MACDSmooth = 9;
				Periodo = 14;
				Factor = 0.5;

				AddPlot(new Stroke(Brushes.Orange), PlotStyle.Line, "Plot0");
				AddLine(Brushes.LightBlue, 100.00, "banda100");
				AddLine(Brushes.LightBlue, 70.00, "bandaUpper");
				AddLine(Brushes.LightBlue, 30.00, "bandaLower");
				AddLine(Brushes.LightBlue, 0.00, "banda0");

			}
			else if (State == State.DataLoaded)
			{
				sFrac1 = new Series<double>(this);
				sFrac2 = new Series<double>(this);
				sPF = new Series<double>(this);
				sPFF = new Series<double>(this);

				sMACD = MACD(Close, MACDFast, MACDSlow, MACDSmooth);
				sMIN = MIN(sMACD, Periodo);
				sMAX = MAX(sMACD, Periodo);
				pMIN = MIN(sPF, Periodo);
				pMAX = MAX(sPF, Periodo);
			}
		}

		protected override void OnBarUpdate()
		{
			sFrac1[0]	= sMAX[0] - sMIN[0] > 0 ? 100 * (sMACD[0] - sMIN[0]) / (sMAX[0] - sMIN[0]) : sFrac1[1];
			sPF[0]		= CurrentBar <= 1 ? sFrac1[0] : sPF[1] + (Factor * (sFrac1[0] - sPF[1]));
			sFrac2[0]	= pMAX[0] - pMIN[0] > 0 ? 100 * (sPF[0] - pMIN[0]) / (pMAX[0] - pMIN[0]) : sFrac2[1];
			sPFF[0]		= CurrentBar <= 1 ? sFrac2[0] : sPFF[1] + (Factor * (sFrac2[0] - sPFF[1]));

			Values[0][0] = sPFF[0];
			
			if(IsRising(Value)) {PlotBrushes[0][0] = Brushes.Green;}
			else if(IsFalling(Value)) {PlotBrushes[0][0] = Brushes.Red;}
			else {PlotBrushes[0][0] = Brushes.Yellow;}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MACDFast", GroupName = "NinjaScriptParameters", Order = 0)]
		public int MACDFast
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MACDSlow", GroupName = "NinjaScriptParameters", Order = 1)]
		public int MACDSlow
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "MACDSmooth", GroupName = "NinjaScriptParameters", Order = 2)]
		public int MACDSmooth
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Periodo", GroupName = "NinjaScriptParameters", Order = 3)]
		public int Periodo
		{ get; set; }

		[Range(0, double.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Factor", GroupName = "NinjaScriptParameters", Order = 4)]
		public double Factor
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZiSchaffTrendCycle[] cacheZiSchaffTrendCycle;
		public ZiSchaffTrendCycle ZiSchaffTrendCycle(int mACDFast, int mACDSlow, int mACDSmooth, int periodo, double factor)
		{
			return ZiSchaffTrendCycle(Input, mACDFast, mACDSlow, mACDSmooth, periodo, factor);
		}

		public ZiSchaffTrendCycle ZiSchaffTrendCycle(ISeries<double> input, int mACDFast, int mACDSlow, int mACDSmooth, int periodo, double factor)
		{
			if (cacheZiSchaffTrendCycle != null)
				for (int idx = 0; idx < cacheZiSchaffTrendCycle.Length; idx++)
					if (cacheZiSchaffTrendCycle[idx] != null && cacheZiSchaffTrendCycle[idx].MACDFast == mACDFast && cacheZiSchaffTrendCycle[idx].MACDSlow == mACDSlow && cacheZiSchaffTrendCycle[idx].MACDSmooth == mACDSmooth && cacheZiSchaffTrendCycle[idx].Periodo == periodo && cacheZiSchaffTrendCycle[idx].Factor == factor && cacheZiSchaffTrendCycle[idx].EqualsInput(input))
						return cacheZiSchaffTrendCycle[idx];
			return CacheIndicator<ZiSchaffTrendCycle>(new ZiSchaffTrendCycle(){ MACDFast = mACDFast, MACDSlow = mACDSlow, MACDSmooth = mACDSmooth, Periodo = periodo, Factor = factor }, input, ref cacheZiSchaffTrendCycle);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZiSchaffTrendCycle ZiSchaffTrendCycle(int mACDFast, int mACDSlow, int mACDSmooth, int periodo, double factor)
		{
			return indicator.ZiSchaffTrendCycle(Input, mACDFast, mACDSlow, mACDSmooth, periodo, factor);
		}

		public Indicators.ZiSchaffTrendCycle ZiSchaffTrendCycle(ISeries<double> input , int mACDFast, int mACDSlow, int mACDSmooth, int periodo, double factor)
		{
			return indicator.ZiSchaffTrendCycle(input, mACDFast, mACDSlow, mACDSmooth, periodo, factor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZiSchaffTrendCycle ZiSchaffTrendCycle(int mACDFast, int mACDSlow, int mACDSmooth, int periodo, double factor)
		{
			return indicator.ZiSchaffTrendCycle(Input, mACDFast, mACDSlow, mACDSmooth, periodo, factor);
		}

		public Indicators.ZiSchaffTrendCycle ZiSchaffTrendCycle(ISeries<double> input , int mACDFast, int mACDSlow, int mACDSmooth, int periodo, double factor)
		{
			return indicator.ZiSchaffTrendCycle(input, mACDFast, mACDSlow, mACDSmooth, periodo, factor);
		}
	}
}

#endregion
