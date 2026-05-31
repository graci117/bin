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
	public enum MAtypeADX
	{
		DEMA,
		EMA,
		HMA,
		LinReg,
		SMA,
		TEMA,		
		TMA,
		VWMA,
		WMA,
		ZLEMA	
	}	

namespace NinjaTrader.NinjaScript.Indicators
{
	
	public class ADXMA : Indicator
	{
		
		private Series<double> dmPlus;
        private Series<double> dmMinus;
        private Series<double> tr;
        private Series<double> maPlusDI;
        private Series<double> maMinusDI;
		private Series<double> adx;
		private Series<double>  MA1;

		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ADXMA";
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
				MAType					= MAtypeADX.SMA;

				ADXPeriod					= 14;
				AddPlot(Brushes.Green, "ADX");
                //AddPlot(Brushes.Blue, "PlusDI");
                //AddPlot(Brushes.Red, "MinusDI");
			}
			else if (State == State.Configure)
			{
			}
			 else if (State == State.DataLoaded)
            {
                dmPlus = new Series<double>(this);
                dmMinus = new Series<double>(this);
                tr = new Series<double>(this);
                maPlusDI = new Series<double>(this);
                maMinusDI = new Series<double>(this);
                adx = new Series<double>(this);
				MA1 = new Series<double>(this);
            }
		}

		protected override void OnBarUpdate()
		{
			 if (CurrentBar < ADXPeriod)
                return;

            double upMove = High[0] - High[1];
            double downMove = Low[1] - Low[0];

            dmPlus[0] = (upMove > downMove && upMove > 0) ? upMove : 0;
            dmMinus[0] = (downMove > upMove && downMove > 0) ? downMove : 0;

            tr[0] = Math.Max(High[0] - Low[0], Math.Max(Math.Abs(High[0] - Close[1]), Math.Abs(Low[0] - Close[1])));

            maPlusDI[0] = GetMA(dmPlus, ADXPeriod)[0];
            maMinusDI[0] = GetMA(dmMinus, ADXPeriod)[0];
            double emaTR = GetMA(tr, ADXPeriod)[0];

            double plusDI = 100 * (maPlusDI[0] / emaTR);
            double minusDI = 100 * (maMinusDI[0] / emaTR);

            double diDiff = Math.Abs(plusDI - minusDI);
            double diSum = plusDI + minusDI;
			 
			adx[0] 		= diSum.ApproxCompare(0) == 0 ? 50 : ((ADXPeriod - 1) * Value[1] + 100 * diDiff / diSum) / ADXPeriod;

            //adx[0] = EMA(100 * (diDiff / (diSum == 0 ? 1 : diSum)), ADXPeriod)[0];

            Values[0][0] = adx[0];
            //Values[1][0] = plusDI;
            //Values[2][0] = minusDI;
		}
		
		private Series<double> GetMA(Series<double> myInput, int period)
		{
			
			
			
			switch (MAType)
				{
					case MAtypeADX.DEMA:						
						
						MA1 = DEMA(myInput, period).Value;
						
						break;
						
					case MAtypeADX.EMA:
							MA1 = EMA(myInput, period).Value;
						
					break;	
						
					case MAtypeADX.HMA:
							MA1 = HMA(myInput, period).Value;
					
					break;	
						
					case MAtypeADX.LinReg:
							MA1 = LinReg(myInput, period).Value;

					break;							
						
					case MAtypeADX.SMA:
							MA1 = SMA(myInput, period).Value;
					
					break;	
						
					case MAtypeADX.TEMA:
							MA1 = TEMA(myInput, period).Value;

					break;	
						
					case MAtypeADX.TMA:	
							MA1 = TMA(myInput, period).Value;
					
					break;	
					
					case MAtypeADX.VWMA:
							MA1 = VWMA(myInput, period).Value;

					break;	
						
					case MAtypeADX.WMA:
							MA1 = WMA(myInput, period).Value;
							
					break;
						
					case MAtypeADX.ZLEMA:
							MA1 = ZLEMA(myInput, period).Value;
					break;
																
				}	
				return MA1;
		}


		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int ADXPeriod
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name=" MA Type", Description="MA Type", Order=1)]
		public MAtypeADX MAType
        { get; set; }

		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ADXMA[] cacheADXMA;
		public ADXMA ADXMA(int aDXPeriod, MAtypeADX mAType)
		{
			return ADXMA(Input, aDXPeriod, mAType);
		}

		public ADXMA ADXMA(ISeries<double> input, int aDXPeriod, MAtypeADX mAType)
		{
			if (cacheADXMA != null)
				for (int idx = 0; idx < cacheADXMA.Length; idx++)
					if (cacheADXMA[idx] != null && cacheADXMA[idx].ADXPeriod == aDXPeriod && cacheADXMA[idx].MAType == mAType && cacheADXMA[idx].EqualsInput(input))
						return cacheADXMA[idx];
			return CacheIndicator<ADXMA>(new ADXMA(){ ADXPeriod = aDXPeriod, MAType = mAType }, input, ref cacheADXMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ADXMA ADXMA(int aDXPeriod, MAtypeADX mAType)
		{
			return indicator.ADXMA(Input, aDXPeriod, mAType);
		}

		public Indicators.ADXMA ADXMA(ISeries<double> input , int aDXPeriod, MAtypeADX mAType)
		{
			return indicator.ADXMA(input, aDXPeriod, mAType);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ADXMA ADXMA(int aDXPeriod, MAtypeADX mAType)
		{
			return indicator.ADXMA(Input, aDXPeriod, mAType);
		}

		public Indicators.ADXMA ADXMA(ISeries<double> input , int aDXPeriod, MAtypeADX mAType)
		{
			return indicator.ADXMA(input, aDXPeriod, mAType);
		}
	}
}

#endregion
