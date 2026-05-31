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
	public class TillsonArrowForPredator : Indicator
	{
		private TillsonT3 TillsonT31;
		private EMA EMA1;
		private EMA EMA2;
		private RSI RSI1;
		private RSI RSI2;
		private EMA EMA3;
		private RSIMA rsiMA;
		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		private int savedLExitBar 		= 0;
		private int	savedSExitBar		= 0;
		private int cd = 0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TillsonArrowForPredator";
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
				Test1					= 1;
				ShortMALength					= 5;
				LongMALength					= 14;
				RSILength					= 14;
				RSIMALength					= 50;
				RSIMAType					= "EMA";
				T3Length					= 8;
				T3VolumeFactor					= 0.7;
				AddPlot(Brushes.Transparent, 	"CrossDetect");
				MAType							= 1;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				TillsonT31				= TillsonT3(Close, T3Length, T3VolumeFactor);
				EMA1				= EMA(Close, Convert.ToInt32(ShortMALength));
				EMA2				= EMA(Close, Convert.ToInt32(LongMALength));
				rsiMA				= RSIMA(RSILength,RSIMALength,RSIMAType);		
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] <50)
				return;
			
		
			
			if (ToTime(Time[0]) < 093000  || ToTime(Time[0]) > 160000)
			{
				//maxLossHit = false;
				//Print("TEST-------------------------" + "-----maxLossHit " + maxLossHit);
				return;
				
			}
			
			CrossDetect[0] 	= 0;				// Reset the cross detection
		
			
			bool lowerSignal =  false;
			bool upperSignal =  false;
			bool lowerExitSignal =  false;
			bool upperExitSignal =  false;
			
			
			if (
				 // T3ChangeLong
				
				(((CrossAbove(Close, TillsonT31, 1))
				 && (EMA1[0] > EMA2[0])
				 && (rsiMA.RSIAvgPlot[0] > rsiMA.RSIEMAPlot[0]))
				 // EMACrossLong
				 || ((Close[0] > TillsonT31[0])
				 && (CrossAbove(EMA1, EMA2, 1))
				 &&  (rsiMA.RSIAvgPlot[0] > rsiMA.RSIEMAPlot[0]))
				 // RSIMACrossLong
				 || ((Close[0] > TillsonT31[0])
				 && (EMA1[0] > EMA2[0])
				 && (Close[0] > Open[0])
				 && (CrossAbove(rsiMA.RSIAvgPlot, rsiMA.RSIEMAPlot, 1))))	
				&& CurrentBar != savedUBar
				&& (cd  == 0 || cd == 2 || cd == -2)
				)
			{
				savedUBar = CurrentBar; 
				CrossDetect[0] =  1;
				Print("Time" + ToTime(Time[0]) + "1-----" + CurrentBar);
				Draw.ArrowUp (this, "UpArrowLong"+CurrentBar, true, 0,  Low[0] - 5 * TickSize , Brushes.Green);
				cd = 1;
			}
			
			if (CrossDetect[0] == 1 )
			{
				//Print ("cntUp[0]---------" + cntUp[0] + "-----Time-----" + ToTime(Time[0]));
				
				Print("Time" + ToTime(Time[0]) + "1 ArrowUP-----" + CurrentBar);
				
			}
			
			 // Set 2
			if (
				 // T3ChangeShort
				(((CrossBelow(Close, TillsonT31, 1))
				 && (EMA1[0] < EMA2[0])
				 && (rsiMA.RSIAvgPlot[0] < rsiMA.RSIEMAPlot[0]))
				 // EMACrossShort
				 || ((Close[0] < TillsonT31[0])
				 && (CrossBelow(EMA1, EMA2, 1))
				 && (rsiMA.RSIAvgPlot[0] < rsiMA.RSIEMAPlot[0]))
				 // RSIMACrossShort
				 || ((Close[0] < TillsonT31[0])
				 && (EMA1[0] < EMA2[0])
				&& (Close[0] < Open[0])
				 && (CrossBelow(rsiMA.RSIAvgPlot, rsiMA.RSIEMAPlot, 1)))
				)
				&& CurrentBar != savedDBar
				&& (cd  == 0 || cd == 2 || cd == -2)
				)
			{
				savedDBar = CurrentBar; 
				CrossDetect[0] =  -1;
				Draw.ArrowDown (this, "DwnArrowShort"+CurrentBar, true, 0, High[0] + 5 * TickSize, Brushes.Red);
				cd = -1;
				Print("Time" + ToTime(Time[0]) + "-1 -----" + CurrentBar);
				
			}		
			
			//if (CrossDetect[0] == 1 )
			if (CrossDetect[0] == -1 )
			{
				
				Print("Time" + ToTime(Time[0]) + "-1 ArrowDown-----" + CurrentBar);
				
			}	
	 
		
			
			if (((CrossBelow(Close, TillsonT31, 1))
				 || CrossBelow(EMA1 , EMA2,1)
				 || CrossBelow(rsiMA.RSIAvgPlot, rsiMA.RSIEMAPlot,1))
				//&& (CurrentBar > savedUBar)
				&&  cd == 1 
				)
			{
				 CrossDetect[0] =  -2;
				savedDBar = CurrentBar;
				Draw.ArrowDown (this, "ArrowLongExit"+CurrentBar, true, 0, High[0] + 5 * TickSize, Brushes.Purple);
				cd = -2;
				Print("Time" + ToTime(Time[0]) + "-2 -----" + CurrentBar);
			}
			
			//if (CrossDetect[0] == -2 )
			if (CrossDetect[0] == -2 )
			{
				
				Print("Time" + ToTime(Time[0]) + "-2 ExitArrowDown-----" + CurrentBar);
				
			}	
			
		
			if (((CrossAbove(Close, TillsonT31, 1))
				 || CrossAbove(EMA1 , EMA2,1)
				 || CrossAbove(rsiMA.RSIAvgPlot, rsiMA.RSIEMAPlot,1))
				//&& (CurrentBar > savedDBar )
				&&  cd == -1
				)
			{
				CrossDetect[0] =  2;
				savedUBar = CurrentBar;
				Print("Time" + ToTime(Time[0]) + "2 -----" + CurrentBar);
				Draw.ArrowUp (this, "ArrowShortExit"+CurrentBar, true, 0,  Low[0] - 5 * TickSize , Brushes.Yellow);
				cd = 2;
				
			}
			
			
			
			//short exit
			//if (CrossDetect[0] == 2)
			if (CrossDetect[0] == 2 )
			{
				//Print ("cntUp[0]---------" + cntUp[0] + "-----Time-----" + ToTime(Time[0]));
				
				Print("Time" + ToTime(Time[0]) + "2 ExitArrowUP-----" + CurrentBar);
				
			}
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Test1", Order=1, GroupName="Parameters")]
		public int Test1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MAType", Order=1, GroupName="Parameters")]
		public int MAType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortMALength", Order=2, GroupName="Parameters")]
		public int ShortMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongMALength", Order=3, GroupName="Parameters")]
		public int LongMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSILength", Order=4, GroupName="Parameters")]
		public int RSILength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSIMALength", Order=5, GroupName="Parameters")]
		public int RSIMALength
		{ get; set; }

		[NinjaScriptProperty]		
		[Display(Name="RSIMAType", Order=6, GroupName="Parameters")]
		public string RSIMAType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="T3Length", Order=7, GroupName="Parameters")]
		public int T3Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="T3VolumeFactor", Order=8, GroupName="Parameters")]
		public double T3VolumeFactor
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CrossDetect
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
		private TillsonArrowForPredator[] cacheTillsonArrowForPredator;
		public TillsonArrowForPredator TillsonArrowForPredator(int test1, int mAType, int shortMALength, int longMALength, int rSILength, int rSIMALength, string rSIMAType, int t3Length, double t3VolumeFactor)
		{
			return TillsonArrowForPredator(Input, test1, mAType, shortMALength, longMALength, rSILength, rSIMALength, rSIMAType, t3Length, t3VolumeFactor);
		}

		public TillsonArrowForPredator TillsonArrowForPredator(ISeries<double> input, int test1, int mAType, int shortMALength, int longMALength, int rSILength, int rSIMALength, string rSIMAType, int t3Length, double t3VolumeFactor)
		{
			if (cacheTillsonArrowForPredator != null)
				for (int idx = 0; idx < cacheTillsonArrowForPredator.Length; idx++)
					if (cacheTillsonArrowForPredator[idx] != null && cacheTillsonArrowForPredator[idx].Test1 == test1 && cacheTillsonArrowForPredator[idx].MAType == mAType && cacheTillsonArrowForPredator[idx].ShortMALength == shortMALength && cacheTillsonArrowForPredator[idx].LongMALength == longMALength && cacheTillsonArrowForPredator[idx].RSILength == rSILength && cacheTillsonArrowForPredator[idx].RSIMALength == rSIMALength && cacheTillsonArrowForPredator[idx].RSIMAType == rSIMAType && cacheTillsonArrowForPredator[idx].T3Length == t3Length && cacheTillsonArrowForPredator[idx].T3VolumeFactor == t3VolumeFactor && cacheTillsonArrowForPredator[idx].EqualsInput(input))
						return cacheTillsonArrowForPredator[idx];
			return CacheIndicator<TillsonArrowForPredator>(new TillsonArrowForPredator(){ Test1 = test1, MAType = mAType, ShortMALength = shortMALength, LongMALength = longMALength, RSILength = rSILength, RSIMALength = rSIMALength, RSIMAType = rSIMAType, T3Length = t3Length, T3VolumeFactor = t3VolumeFactor }, input, ref cacheTillsonArrowForPredator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TillsonArrowForPredator TillsonArrowForPredator(int test1, int mAType, int shortMALength, int longMALength, int rSILength, int rSIMALength, string rSIMAType, int t3Length, double t3VolumeFactor)
		{
			return indicator.TillsonArrowForPredator(Input, test1, mAType, shortMALength, longMALength, rSILength, rSIMALength, rSIMAType, t3Length, t3VolumeFactor);
		}

		public Indicators.TillsonArrowForPredator TillsonArrowForPredator(ISeries<double> input , int test1, int mAType, int shortMALength, int longMALength, int rSILength, int rSIMALength, string rSIMAType, int t3Length, double t3VolumeFactor)
		{
			return indicator.TillsonArrowForPredator(input, test1, mAType, shortMALength, longMALength, rSILength, rSIMALength, rSIMAType, t3Length, t3VolumeFactor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TillsonArrowForPredator TillsonArrowForPredator(int test1, int mAType, int shortMALength, int longMALength, int rSILength, int rSIMALength, string rSIMAType, int t3Length, double t3VolumeFactor)
		{
			return indicator.TillsonArrowForPredator(Input, test1, mAType, shortMALength, longMALength, rSILength, rSIMALength, rSIMAType, t3Length, t3VolumeFactor);
		}

		public Indicators.TillsonArrowForPredator TillsonArrowForPredator(ISeries<double> input , int test1, int mAType, int shortMALength, int longMALength, int rSILength, int rSIMALength, string rSIMAType, int t3Length, double t3VolumeFactor)
		{
			return indicator.TillsonArrowForPredator(input, test1, mAType, shortMALength, longMALength, rSILength, rSIMALength, rSIMAType, t3Length, t3VolumeFactor);
		}
	}
}

#endregion
