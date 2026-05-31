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
using NinjaTrader.NinjaScript.Indicators.Pdt;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class BigFour : Indicator
	{
		private AKTrendOscillator akTrend;
		private AnchoredMomentumOscillator anchMom;
		private AuEhlersFilter ehlersFilter;
		private TrueMomentumOscillator tmo;
		private ZScoreV10 zScore;
		private int alphaBarClr = 0;
		//private Series<int> direction;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"The Big Four";
				Name										= "BigFour";
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
				Aktrend_input1					= 3;
				Aktrend_input2					= 8;
				Zscore_length					= 20;
				Zscore_ZavgLength					= 20;
				Ehlers_length					= 34;
				Amom_MomentumPeriod					= 10;
				Amom_SignalPeriod					= 8;
				Amom_SmoothMomentum					= false;
				Amom_SmoothingPeriod					= 7;
				TMO_Length								= 30;
				TMO_CalcLength								= 6;
				TMO_SmoothLength								= 6;
				Strategy_Confirmation_Factor					= 4;
				Strategy_FilterWithTMO					= false;
				Strategy_ColoredCandlesOn					= true;
				Strategy_VerticalLinesOn					= false;
				Strategy_HoldTrend					= false;
			}
			else if (State == State.Configure)
			{
				AddPlot(Brushes.Transparent, "Direction");
			}
			else if (State == State.DataLoaded)
			{
				akTrend = AKTrendOscillator(Aktrend_input1, Aktrend_input2);//1
				zScore = ZScoreV10(Zscore_length, Zscore_ZavgLength);
				anchMom = AnchoredMomentumOscillator(Amom_MomentumPeriod, Amom_SignalPeriod, Amom_SmoothMomentum, Amom_SmoothingPeriod);//3
				ehlersFilter = AuEhlersFilter(Ehlers_length);//4
				tmo = TrueMomentumOscillator(TMO_Length, TMO_CalcLength, TMO_SmoothLength);//5
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			if (CurrentBar < 35)
				return;
			if (CurrentBar ==35)
				alphaBarClr = 25 * 4;
			int cond1_UP = 0;
			int cond2_UP = 0;
			int cond3_UP = 0;
			int cond4_UP = 0;
			int cond5_UP = 0;
			
			int cond1_DN = 0;
			int cond2_DN = 0;
			int cond3_DN = 0;
			int cond4_DN = 0;
			int cond5_DN = 0;
			
			
			//AKTrend	
			cond1_UP =  (akTrend.Values[0])[0] > 0 ? 1 : 0;
			cond1_DN =  (akTrend.Values[0])[0] <= 0 ? -1 : 0;
			
			
			//ZScore
			cond2_UP =  zScore.Z[0]  > 0 ? 1 : 0;
			cond2_DN =  zScore.Z[0]  <= 0 ? -1 : 0;
			
			//Anch Momentum
			cond3_UP =  anchMom.AnchoredMomentum[0]  > 0 ? 1 : 0;
			cond3_DN =  anchMom.AnchoredMomentum[0] <= 0 ? -1 : 0;
			
			//Ehlers
			cond4_UP =  Close[0] > ehlersFilter.EhlersFilter[0]   ? 1 : 0;
			cond4_DN =  Close[0] <= ehlersFilter.EhlersFilter[0]  ? -1 : 0;
			
			//TMO
			cond5_UP =  tmo.Main[0]  <= 0 ? 1 : 0;
			cond5_DN =  tmo.Main[0] >= 0 ? 1 : 0;
			
			
			
			int cond_UP = cond1_UP + cond2_UP + cond3_UP + cond4_UP;
			int cond_DN = cond1_DN + cond2_DN + cond3_DN + cond4_DN;
			
			
		
				
//			if (cond_UP >= Strategy_Confirmation_Factor && (!Strategy_FilterWithTMO || cond5_UP == 1)) 
//				direction =1;
//			else if (cond_DN <= -Strategy_Confirmation_Factor && (!Strategy_FilterWithTMO || cond5_DN == 1) )
//				direction = -1;
//			else if (!Strategy_HoldTrend && direction[1] == 1 && cond_UP < Strategy_Confirmation_Factor && cond_DN > -Strategy_Confirmation_Factor)
//				direction = 0	;
//			else if (!Strategy_HoldTrend && direction[1] == -1 && cond_DN > -Strategy_Confirmation_Factor && cond_UP < Strategy_Confirmation_Factor)
//				direction = 0	;
//			else
//				direction = direction[1];
				
			int direction;
					
			if (cond_UP >= Strategy_Confirmation_Factor && (!Strategy_FilterWithTMO || cond5_UP == 1)) 
				direction =1;
			else if (cond_DN <= -Strategy_Confirmation_Factor && (!Strategy_FilterWithTMO || cond5_DN == 1) )
				direction = -1;			
			else
				direction = 0;
			
			Direction[0] = direction;
			
			if (Strategy_ColoredCandlesOn)
			{
				if(direction == 1)
				{
					 BarBrushes[0] = Brushes.Lime;
					CandleOutlineBrushes[0] = Brushes.Lime;
				}
				else if (direction == -1)
				{
					BarBrushes[0] = Brushes.Red;
					CandleOutlineBrushes[0] = Brushes.Red;
				}
				else
				{
					BarBrushes[0] = Brushes.Gray;
					CandleOutlineBrushes[0] = Brushes.Gray;
				}
				
				 if (Close[0] > Open[0])
                    {
                        byte g = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).G;
                        byte r = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).R;
                        byte b = ((Color)BarBrushes[0].GetValue(SolidColorBrush.ColorProperty)).B;

                        BarBrushes[0] = new SolidColorBrush(Color.FromArgb((byte)alphaBarClr, r, g, b));
                    }
			}
			
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Aktrend_input1", Order=1, GroupName="AKTrend")]
		public int Aktrend_input1
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Aktrend_input2", Order=2, GroupName="AKTrend")]
		public int Aktrend_input2
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Zscore_length", Order=3, GroupName="ZScore")]
		public int Zscore_length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Zscore_ZavgLength", Order=4, GroupName="ZScore")]
		public int Zscore_ZavgLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Ehlers_length", Order=5, GroupName="Ehlers Filter")]
		public int Ehlers_length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amom_MomentumPeriod", Order=6, GroupName="Anchored Momentum")]
		public int Amom_MomentumPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amom_SignalPeriod", Order=7, GroupName="Anchored Momentum")]
		public int Amom_SignalPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Amom_SmoothMomentum", Order=8, GroupName="Anchored Momentum")]
		public bool Amom_SmoothMomentum
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Amom_SmoothingPeriod", Order=9, GroupName="Anchored Momentum")]
		public int Amom_SmoothingPeriod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TMO_Length", Order=13, GroupName="TMO")]
		public int TMO_Length
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="TMO_CalcLength", Order=14, GroupName="TMO")]
		public int TMO_CalcLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TMO_SmoothLength", Order=15, GroupName="TMO")]
		public int TMO_SmoothLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Strategy_Confirmation_Factor", Order=21, GroupName="Main")]
		public int Strategy_Confirmation_Factor
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Strategy_FilterWithTMO", Order=22, GroupName="Main")]
		public bool Strategy_FilterWithTMO
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Strategy_ColoredCandlesOn", Order=23, GroupName="Main")]
		public bool Strategy_ColoredCandlesOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Strategy_VerticalLinesOn", Order=24, GroupName="Main")]
		public bool Strategy_VerticalLinesOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Strategy_HoldTrend", Order=25, GroupName="Main")]
		public bool Strategy_HoldTrend
		{ get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Direction
		{ get { return Values[0]; } }
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BigFour[] cacheBigFour;
		public BigFour BigFour(int aktrend_input1, int aktrend_input2, int zscore_length, int zscore_ZavgLength, int ehlers_length, int amom_MomentumPeriod, int amom_SignalPeriod, bool amom_SmoothMomentum, int amom_SmoothingPeriod, int tMO_Length, int tMO_CalcLength, int tMO_SmoothLength, int strategy_Confirmation_Factor, bool strategy_FilterWithTMO, bool strategy_ColoredCandlesOn, bool strategy_VerticalLinesOn, bool strategy_HoldTrend)
		{
			return BigFour(Input, aktrend_input1, aktrend_input2, zscore_length, zscore_ZavgLength, ehlers_length, amom_MomentumPeriod, amom_SignalPeriod, amom_SmoothMomentum, amom_SmoothingPeriod, tMO_Length, tMO_CalcLength, tMO_SmoothLength, strategy_Confirmation_Factor, strategy_FilterWithTMO, strategy_ColoredCandlesOn, strategy_VerticalLinesOn, strategy_HoldTrend);
		}

		public BigFour BigFour(ISeries<double> input, int aktrend_input1, int aktrend_input2, int zscore_length, int zscore_ZavgLength, int ehlers_length, int amom_MomentumPeriod, int amom_SignalPeriod, bool amom_SmoothMomentum, int amom_SmoothingPeriod, int tMO_Length, int tMO_CalcLength, int tMO_SmoothLength, int strategy_Confirmation_Factor, bool strategy_FilterWithTMO, bool strategy_ColoredCandlesOn, bool strategy_VerticalLinesOn, bool strategy_HoldTrend)
		{
			if (cacheBigFour != null)
				for (int idx = 0; idx < cacheBigFour.Length; idx++)
					if (cacheBigFour[idx] != null && cacheBigFour[idx].Aktrend_input1 == aktrend_input1 && cacheBigFour[idx].Aktrend_input2 == aktrend_input2 && cacheBigFour[idx].Zscore_length == zscore_length && cacheBigFour[idx].Zscore_ZavgLength == zscore_ZavgLength && cacheBigFour[idx].Ehlers_length == ehlers_length && cacheBigFour[idx].Amom_MomentumPeriod == amom_MomentumPeriod && cacheBigFour[idx].Amom_SignalPeriod == amom_SignalPeriod && cacheBigFour[idx].Amom_SmoothMomentum == amom_SmoothMomentum && cacheBigFour[idx].Amom_SmoothingPeriod == amom_SmoothingPeriod && cacheBigFour[idx].TMO_Length == tMO_Length && cacheBigFour[idx].TMO_CalcLength == tMO_CalcLength && cacheBigFour[idx].TMO_SmoothLength == tMO_SmoothLength && cacheBigFour[idx].Strategy_Confirmation_Factor == strategy_Confirmation_Factor && cacheBigFour[idx].Strategy_FilterWithTMO == strategy_FilterWithTMO && cacheBigFour[idx].Strategy_ColoredCandlesOn == strategy_ColoredCandlesOn && cacheBigFour[idx].Strategy_VerticalLinesOn == strategy_VerticalLinesOn && cacheBigFour[idx].Strategy_HoldTrend == strategy_HoldTrend && cacheBigFour[idx].EqualsInput(input))
						return cacheBigFour[idx];
			return CacheIndicator<BigFour>(new BigFour(){ Aktrend_input1 = aktrend_input1, Aktrend_input2 = aktrend_input2, Zscore_length = zscore_length, Zscore_ZavgLength = zscore_ZavgLength, Ehlers_length = ehlers_length, Amom_MomentumPeriod = amom_MomentumPeriod, Amom_SignalPeriod = amom_SignalPeriod, Amom_SmoothMomentum = amom_SmoothMomentum, Amom_SmoothingPeriod = amom_SmoothingPeriod, TMO_Length = tMO_Length, TMO_CalcLength = tMO_CalcLength, TMO_SmoothLength = tMO_SmoothLength, Strategy_Confirmation_Factor = strategy_Confirmation_Factor, Strategy_FilterWithTMO = strategy_FilterWithTMO, Strategy_ColoredCandlesOn = strategy_ColoredCandlesOn, Strategy_VerticalLinesOn = strategy_VerticalLinesOn, Strategy_HoldTrend = strategy_HoldTrend }, input, ref cacheBigFour);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BigFour BigFour(int aktrend_input1, int aktrend_input2, int zscore_length, int zscore_ZavgLength, int ehlers_length, int amom_MomentumPeriod, int amom_SignalPeriod, bool amom_SmoothMomentum, int amom_SmoothingPeriod, int tMO_Length, int tMO_CalcLength, int tMO_SmoothLength, int strategy_Confirmation_Factor, bool strategy_FilterWithTMO, bool strategy_ColoredCandlesOn, bool strategy_VerticalLinesOn, bool strategy_HoldTrend)
		{
			return indicator.BigFour(Input, aktrend_input1, aktrend_input2, zscore_length, zscore_ZavgLength, ehlers_length, amom_MomentumPeriod, amom_SignalPeriod, amom_SmoothMomentum, amom_SmoothingPeriod, tMO_Length, tMO_CalcLength, tMO_SmoothLength, strategy_Confirmation_Factor, strategy_FilterWithTMO, strategy_ColoredCandlesOn, strategy_VerticalLinesOn, strategy_HoldTrend);
		}

		public Indicators.BigFour BigFour(ISeries<double> input , int aktrend_input1, int aktrend_input2, int zscore_length, int zscore_ZavgLength, int ehlers_length, int amom_MomentumPeriod, int amom_SignalPeriod, bool amom_SmoothMomentum, int amom_SmoothingPeriod, int tMO_Length, int tMO_CalcLength, int tMO_SmoothLength, int strategy_Confirmation_Factor, bool strategy_FilterWithTMO, bool strategy_ColoredCandlesOn, bool strategy_VerticalLinesOn, bool strategy_HoldTrend)
		{
			return indicator.BigFour(input, aktrend_input1, aktrend_input2, zscore_length, zscore_ZavgLength, ehlers_length, amom_MomentumPeriod, amom_SignalPeriod, amom_SmoothMomentum, amom_SmoothingPeriod, tMO_Length, tMO_CalcLength, tMO_SmoothLength, strategy_Confirmation_Factor, strategy_FilterWithTMO, strategy_ColoredCandlesOn, strategy_VerticalLinesOn, strategy_HoldTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BigFour BigFour(int aktrend_input1, int aktrend_input2, int zscore_length, int zscore_ZavgLength, int ehlers_length, int amom_MomentumPeriod, int amom_SignalPeriod, bool amom_SmoothMomentum, int amom_SmoothingPeriod, int tMO_Length, int tMO_CalcLength, int tMO_SmoothLength, int strategy_Confirmation_Factor, bool strategy_FilterWithTMO, bool strategy_ColoredCandlesOn, bool strategy_VerticalLinesOn, bool strategy_HoldTrend)
		{
			return indicator.BigFour(Input, aktrend_input1, aktrend_input2, zscore_length, zscore_ZavgLength, ehlers_length, amom_MomentumPeriod, amom_SignalPeriod, amom_SmoothMomentum, amom_SmoothingPeriod, tMO_Length, tMO_CalcLength, tMO_SmoothLength, strategy_Confirmation_Factor, strategy_FilterWithTMO, strategy_ColoredCandlesOn, strategy_VerticalLinesOn, strategy_HoldTrend);
		}

		public Indicators.BigFour BigFour(ISeries<double> input , int aktrend_input1, int aktrend_input2, int zscore_length, int zscore_ZavgLength, int ehlers_length, int amom_MomentumPeriod, int amom_SignalPeriod, bool amom_SmoothMomentum, int amom_SmoothingPeriod, int tMO_Length, int tMO_CalcLength, int tMO_SmoothLength, int strategy_Confirmation_Factor, bool strategy_FilterWithTMO, bool strategy_ColoredCandlesOn, bool strategy_VerticalLinesOn, bool strategy_HoldTrend)
		{
			return indicator.BigFour(input, aktrend_input1, aktrend_input2, zscore_length, zscore_ZavgLength, ehlers_length, amom_MomentumPeriod, amom_SignalPeriod, amom_SmoothMomentum, amom_SmoothingPeriod, tMO_Length, tMO_CalcLength, tMO_SmoothLength, strategy_Confirmation_Factor, strategy_FilterWithTMO, strategy_ColoredCandlesOn, strategy_VerticalLinesOn, strategy_HoldTrend);
		}
	}
}

#endregion
