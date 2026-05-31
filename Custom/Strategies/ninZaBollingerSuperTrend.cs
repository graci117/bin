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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class ninZaBollingerSuperTrend : Strategy
	{
		private ninZaBollingerReversal ninZaBollingerReversal1;
		private KeltnerChannel KeltnerChannel1;
		private ninZaSuperTrendPro ninZaSuperTrendPro1;
		private ninZaPANAKanal ninZaPANAKanal1;
		private Series<double> tradeSignal;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "ninZaBollingerSuperTrend";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				//Signal_Trade					= 0;
				Signal_Trend					= 0;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				ninZaBollingerReversal1				= ninZaBollingerReversal(Close, ninZa_MAType.EMA, 14, true, ninZa_MAType.EMA, 5, 2, 1, 1);
				KeltnerChannel1				= KeltnerChannel(Close, 1.1, 20);
				ninZaSuperTrendPro1				= ninZaSuperTrendPro(Close, ninZa_MAType.EMA, NinjaTrader.Data.PriceType.Median, NinjaTrader.Data.PriceType.Median, 14, true, ninZa_MAType.SMA, 5, 1.5, 50);
				ninZaPANAKanal1				=  ninZaPANAKanal(Close, 20, 4, 14, 20, 10);
				ninZaBollingerReversal1.Plots[0].Brush = Brushes.HotPink;
				ninZaBollingerReversal1.Plots[1].Brush = Brushes.Orange;
				ninZaBollingerReversal1.Plots[2].Brush = Brushes.DodgerBlue;
				ninZaBollingerReversal1.Plots[3].Brush = Brushes.Transparent;
				ninZaBollingerReversal1.Plots[4].Brush = Brushes.Transparent;
				KeltnerChannel1.Plots[0].Brush = Brushes.DarkGray;
				KeltnerChannel1.Plots[1].Brush = Brushes.Turquoise;
				KeltnerChannel1.Plots[2].Brush = Brushes.Turquoise;
				ninZaSuperTrendPro1.Plots[0].Brush = Brushes.Yellow;
				ninZaSuperTrendPro1.Plots[1].Brush = Brushes.Transparent;
				ninZaPANAKanal1.Plots[0].Brush = Brushes.Yellow;
				ninZaPANAKanal1.Plots[1].Brush = Brushes.Goldenrod;
				ninZaPANAKanal1.Plots[2].Brush = Brushes.Goldenrod;
				ninZaPANAKanal1.Plots[3].Brush = Brushes.Transparent;
				ninZaPANAKanal1.Plots[4].Brush = Brushes.Transparent;
				//AddChartIndicator(ninZaBollingerReversal1);
				//AddChartIndicator(KeltnerChannel1);
				//AddChartIndicator(ninZaSuperTrendPro1);
				//AddChartIndicator(ninZaPANAKanal1);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			tradeSignal[0] = 0;

			 // Set 1
			if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] > ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] > 0)
				 && (High[0] > High[1])
				 && (ninZaSuperTrendPro1.Signal_State[1] < 0)
				 && (ninZaSuperTrendPro1.Signal_State[0] > 0)
				&& tradeSignal[0] == 0)
			{
				//EnterLongLimit(Convert.ToInt32(DefaultQuantity), 0, @"STLong");
				tradeSignal[0] = 1;
			}			
			 // Set 2
			else if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] < ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] < 0)
				 && (High[0] > High[1])
				 && (ninZaSuperTrendPro1.Signal_State[1] >= 0)
				 && (ninZaSuperTrendPro1.Signal_State[0] < 0)
				 && tradeSignal[0] == 0)
			{
				//EnterShortLimit(Convert.ToInt32(DefaultQuantity), 0, @"1");
				tradeSignal[0] = -1;
			}			
			 // Set 3
			 else if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] > ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] > 0)
				 && (High[0] > High[1])
				 && (ninZaPANAKanal1.Signal_Trade[0] > 0)
				 && (ninZaSuperTrendPro1.Signal_State[0] > 0)
				 && tradeSignal[0] == 0)
			{
				//EnterLongLimit(Convert.ToInt32(DefaultQuantity), 0, @"STLong");
				tradeSignal[0] = 1;
			}
			
			 // Set 4
			else if ((ninZaBollingerReversal1.UpperBand[0] < KeltnerChannel1.Upper[0])
				 && (ninZaBollingerReversal1.LowerBand[0] > KeltnerChannel1.Lower[0])
				 && (Close[0] < ninZaSuperTrendPro1.SuperTrend[0])
				 && (ninZaPANAKanal1.Signal_Trend[0] < 0)
				 && (High[0] > High[1])
				 && (ninZaSuperTrendPro1.Signal_State[0] < 0)
				 && (ninZaPANAKanal1.Signal_Trade[0] < 0)
				&& tradeSignal[0] == 0)
			{
				//EnterShortLimit(Convert.ToInt32(DefaultQuantity), 0, @"1");
				tradeSignal[0] = -1;
			}
			
		}

		#region Properties
		
		
		
		

		[NinjaScriptProperty]
		[Range(-5, int.MaxValue)]
		[Display(Name="Signal_Trend", Order=2, GroupName="Parameters")]
		public int Signal_Trend
		{ get; set; }
		
				[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trade_Signal
		{
		    get { return tradeSignal; }
			}
		#endregion

	}
}
