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
	public class TPR : Indicator
	{
		#region Variables
			private double MULT, point, tpr, sma1, sma2, smadiff, thrs, jj, ctrT, ctrP, ctrM;
			private Series<double> TPRSeries;
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"TPR indicator as detailed in the February 2021 Technical Analysis Stocks and Commodities article ‘Trend Strength: Measuring The Duration Of A Trend’  by Richard Poster, PhD.";
				Name										= "TPR";
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
				AddPlot(Brushes.Red, "TPRPlot");
				TPRPer 										= 15;
				SMAPer 										= 5;
				ThrshFixed 									= 1.0;
				MULT 										= 10;
				Joff										= 0;
				OverridePoint								= false;
				Point										= 0;
				Smooth										= false;
				SmoothPer									= 5;
				RangeMulti									= 100;
			}
			else if (State == State.Configure)
			{
				TPRSeries = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
			}
			else if (State == State.DataLoaded)
			{
				if (OverridePoint == false)
					point = Bars.Instrument.MasterInstrument.TickSize;
				else if (OverridePoint == true)
					point = Point;
			}
		}
		
		public double GetTPR(int Joff)
		{
			ctrT = ctrP = ctrM = 0;
			
			for (jj = 0; jj < TPRPer; jj++)
			{
				sma1 = SMA(Close, SMAPer)[(int)jj + 1 + (int)Joff];
				sma2 = SMA(Close, SMAPer)[(int)jj + 2 + (int)Joff];
				smadiff = ((sma1 - sma2)/((int)MULT * point));
				ctrT += 1;
				thrs = ThrshFixed;
				//up trend counter
				if (smadiff > thrs)
					ctrP += 1;
				//down trend counter
				if (smadiff < -thrs)
					ctrM += 1;
			}
			tpr = Math.Abs(RangeMulti *(ctrP - ctrM) / ctrT);
			return(tpr);
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] <= TPRPer || CurrentBars[0] <= SMAPer)
				return;
			
			TPRSeries[0] = GetTPR(Joff);
			
			if (Smooth == false)
				TPRPlot[0] = TPRSeries[0];
			else if (Smooth == true)
				TPRPlot[0] = SMA(TPRSeries, SmoothPer)[0];
		}
		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TPRPlot
		{
			get { return Values[0]; }
		}
		[NinjaScriptProperty]
		[Display(Name="TPR Period", Description="TPR Period", Order=1, GroupName="Parameters")]
		public int TPRPer
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="SMA Period", Description="SMA Period", Order=2, GroupName="Parameters")]
		public int SMAPer
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Bar Offset", Description="Bar Offset", Order=3, GroupName="Parameters")]
		public int Joff
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Threshold", Description="Threshold", Order=4, GroupName="Parameters")]
		public double ThrshFixed
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Override Point", Description="Override Point", Order=5, GroupName="Parameters")]
		public bool OverridePoint
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Point", Description="Point value", Order=6, GroupName="Parameters")]
		public double Point
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Smooth", Description="Applies Smooth", Order=7, GroupName="Parameters")]
		public bool Smooth
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Smooth Period", Description="Smooth period", Order=8, GroupName="Parameters")]
		public int SmoothPer
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Range Multiplier", Description="Range multiplier", Order=9, GroupName="Parameters")]
		public int RangeMulti
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TPR[] cacheTPR;
		public TPR TPR(int tPRPer, int sMAPer, int joff, double thrshFixed, bool overridePoint, double point, bool smooth, int smoothPer, int rangeMulti)
		{
			return TPR(Input, tPRPer, sMAPer, joff, thrshFixed, overridePoint, point, smooth, smoothPer, rangeMulti);
		}

		public TPR TPR(ISeries<double> input, int tPRPer, int sMAPer, int joff, double thrshFixed, bool overridePoint, double point, bool smooth, int smoothPer, int rangeMulti)
		{
			if (cacheTPR != null)
				for (int idx = 0; idx < cacheTPR.Length; idx++)
					if (cacheTPR[idx] != null && cacheTPR[idx].TPRPer == tPRPer && cacheTPR[idx].SMAPer == sMAPer && cacheTPR[idx].Joff == joff && cacheTPR[idx].ThrshFixed == thrshFixed && cacheTPR[idx].OverridePoint == overridePoint && cacheTPR[idx].Point == point && cacheTPR[idx].Smooth == smooth && cacheTPR[idx].SmoothPer == smoothPer && cacheTPR[idx].RangeMulti == rangeMulti && cacheTPR[idx].EqualsInput(input))
						return cacheTPR[idx];
			return CacheIndicator<TPR>(new TPR(){ TPRPer = tPRPer, SMAPer = sMAPer, Joff = joff, ThrshFixed = thrshFixed, OverridePoint = overridePoint, Point = point, Smooth = smooth, SmoothPer = smoothPer, RangeMulti = rangeMulti }, input, ref cacheTPR);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TPR TPR(int tPRPer, int sMAPer, int joff, double thrshFixed, bool overridePoint, double point, bool smooth, int smoothPer, int rangeMulti)
		{
			return indicator.TPR(Input, tPRPer, sMAPer, joff, thrshFixed, overridePoint, point, smooth, smoothPer, rangeMulti);
		}

		public Indicators.TPR TPR(ISeries<double> input , int tPRPer, int sMAPer, int joff, double thrshFixed, bool overridePoint, double point, bool smooth, int smoothPer, int rangeMulti)
		{
			return indicator.TPR(input, tPRPer, sMAPer, joff, thrshFixed, overridePoint, point, smooth, smoothPer, rangeMulti);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TPR TPR(int tPRPer, int sMAPer, int joff, double thrshFixed, bool overridePoint, double point, bool smooth, int smoothPer, int rangeMulti)
		{
			return indicator.TPR(Input, tPRPer, sMAPer, joff, thrshFixed, overridePoint, point, smooth, smoothPer, rangeMulti);
		}

		public Indicators.TPR TPR(ISeries<double> input , int tPRPer, int sMAPer, int joff, double thrshFixed, bool overridePoint, double point, bool smooth, int smoothPer, int rangeMulti)
		{
			return indicator.TPR(input, tPRPer, sMAPer, joff, thrshFixed, overridePoint, point, smooth, smoothPer, rangeMulti);
		}
	}
}

#endregion
