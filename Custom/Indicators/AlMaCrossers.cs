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
	public class AlMaCrossers : Indicator
	{
		private SMA EMA4;
		private SMA EMA8;
		private SMA EMA15;
		private MACD MACD1;
		private PPO PPO1;

		private bool canLong1 = false; 
		private bool canLong2 = false; 
		private bool canLong3 = false; //unused
		private bool canLong4 = false; //unused 
		private bool canShort1 = false;
		private bool canShort2 = false;
		private bool canShort3 = false; //unused
		private bool canShort4 = false; //unused
		
		// Controls the number of entries per bar.
		// Need to make attempts in the same direction
		// Aparently, the code is forcing trade cancellation in the same bar
		private bool allowTrade = true;
		private int tradesCounter = 0;
		private int MAX_TRADES_BAR = 2;
		
		private bool longPos = false;
		private bool shortPos = false;
		
		private double entryAvgPrice = 0.0;
		private double totalPNL = 0.0;
		
		private double tickSize = 0.0;
        private double pointValue =  0.0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Signals possible entry and exit for trades based on MACD and 3 SMAs (4, 8, 15";
				Name										= "AlMaCrossers";
				Calculate									= Calculate.OnEachTick;
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
				PpoRange					= 0.001;
				MACDRange					= 0.25;
				EntryTolerance				= 4;
				lTrade						= 0;
				emasDelta					= 0.012;
				macdsDelta					= 0.002;
				StopLoss					= 50;
				
			}
//			else if (State == State.Configure)
//			{
//				AddDataSeries(Data.BarsPeriodType.Minute, 5);
//				AddDataSeries(Data.BarsPeriodType.Tick, 2000);
//			}
			else if (State == State.DataLoaded)
			{				
				EMA4				= SMA(Close, 4);
				EMA8				= SMA(Close, 8);
				EMA15				= SMA(Close, 15);
				
				MACD1				= MACD(Close, 12, 26, 9);
				PPO1				= PPO(Close, 12, 26, 9); 
				
				EMA4.Plots[0].Brush = Brushes.Fuchsia;
				EMA8.Plots[0].Brush = Brushes.Snow;
				EMA15.Plots[0].Brush = Brushes.Green;
				
				MACD1.Plots[0].Brush = Brushes.Green;
				MACD1.Plots[1].Brush = Brushes.Red;
				MACD1.Plots[2].Brush = Brushes.DodgerBlue;
				
				tickSize = BarsArray[0].Instrument.MasterInstrument.TickSize;
				pointValue =  BarsArray[0].Instrument.MasterInstrument.PointValue;
				
				totalPNL = 0.0;
			}
			
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnFundamentalData(FundamentalDataEventArgs fundamentalDataUpdate)
		{
			
		}

		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			
		}

		protected override void OnMarketDepth(MarketDepthEventArgs marketDepthUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1)
				return;
			
			if (IsFirstTickOfBar)
            {
				allowTrade = true;
				tradesCounter = 0;
            } 
			
			double pieceL = ((EMA4[1]-EMA8[1])/EntryTolerance);
			double pieceS = ((EMA8[1]-EMA4[1])/EntryTolerance);
			
			
			

			if (CrossBelow(EMA4, EMA8, 1) && longPos)
			{
				longPos = false;
				
				Draw.TriangleDown(this, @"AlMa2 Diamond down_L"+CurrentBar, true, 0, High[1] , Brushes.White);

				double finalPrice = Close[0];
				
				string isWin = finalPrice>entryAvgPrice? "Win": "Loss";
				
				double pnl = (finalPrice-entryAvgPrice)*(pointValue);
				
				totalPNL = totalPNL + pnl;
				
				Draw.Text(this, "t1"+CurrentBar, "CL "+ finalPrice +"\n"+isWin+" \n"+pnl, 0, High[1]+1);
				entryAvgPrice = 0.0;
			}
			
			if (CrossAbove(EMA4, EMA8, 1) && shortPos)
			{
				shortPos = false;
				
				Draw.TriangleUp(this, @"AlMa2 Diamond up_S"+CurrentBar, true, 0, Low[1] , Brushes.White);
				
				double finalPrice = Close[0];
				
				string isWin = finalPrice<entryAvgPrice? "Win": "Loss";
				
				double pnl = (entryAvgPrice-finalPrice)*(pointValue);
				
				totalPNL = totalPNL + pnl;
				
				Draw.Text(this, "t2"+CurrentBar, "CS "+ finalPrice +"\n"+isWin+" \n"+pnl, 0, Low[1]-1);
				entryAvgPrice = 0.0;
			}
			
			Draw.TextFixed(this, "tag1", "PnL: " + totalPNL, TextPosition.TopRight);
			
			double ppoHist = Math.Abs((PPO1.Default[0]) - (PPO1.Smoothed[0]));
			double mHist = Math.Abs(MACD1.Diff[0]);

			 // Set 1
			if (CrossAbove(EMA4, EMA15, 1))
			{
				canLong1=true;
				canShort1=false;
			}
			
//			if ( (Close[0] < EMA8[1] || Close[0] < EMA15[1]) )
//			{
//				entryAvgPrice = 0.0;
//			}
			
			// Set 2
			if (CrossBelow(EMA4, EMA15, 1) )
			{
				canShort1=true;
				canLong1=false;
			}
			
//			if ( (Close[0] > EMA8[1] || Close[0] > EMA15[1]) )
//			{
//				entryAvgPrice = 0.0;
//			}
			
			bool goodVol = Volume[0]>Volume[1] && Volume[1]>Volume[2];
						
			 // Set 3
			if (CrossAbove(MACD1.Default, MACD1.Avg, 1))
			{
				canLong2=true;
				canShort2=false;
			}
			else
			 // Set 4
			if (CrossBelow(MACD1.Default, MACD1.Avg, 1))
			{
				canShort2=true;
				canLong2=false;
			}
			
			bool deltaEma4Ema8 = EMA4[0]>EMA8[0]?(isEMAsDeltaOK(EMA4) )://&& isEMAsDeltaOK(EMA8)): 
				(isEMAsDeltaOK2(EMA4));// && isEMAsDeltaOK2(EMA8));
			
			if (!longPos && !shortPos)
			{
				if ((Open[0]>EMA8[1]||Open[0]>EMA15[1]) && (EMA4[0]>EMA8[0] && EMA8[0]>EMA15[0]) && //deltaEma4Ema8 &&
					canLong1 && canLong2 && mHist>MACDRange &&  
					allowTrade && tradesCounter<MAX_TRADES_BAR //&& isMACDsDeltaOK(MACD1) 
					)
				{
					
					Alert("ALong", Priority.High, "Pot Long, Vol OK?[" + goodVol + "]", 
						NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", 120, 
						Brushes.Black, Brushes.Green);
					
					Draw.TriangleUp(this, @"AlMa2 Diamond up_1"+CurrentBar, true, 0, Close[0] , Brushes.DarkGreen);
					Draw.Text(this, "t2"+CurrentBar, "B "+ Close[0], 0, Low[1]-1);
					longPos = true;
					entryAvgPrice = Close[0];
					
				}
				else
				if ((Open[0]<EMA8[1]||Open[0]<EMA15[1]) && (EMA4[0]<EMA8[0] && EMA8[0]<EMA15[0]) && //deltaEma4Ema8 &&
					canShort1 && canShort2 && mHist>MACDRange && 
					allowTrade && tradesCounter<MAX_TRADES_BAR //&& isMACDsDeltaOK(MACD1)
					)
				{

					Alert("AShort", Priority.High, "Pot Short, Vol OK?[" + goodVol + "]", 
						NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", 120, 
						Brushes.Black, Brushes.Orange);
					
					Draw.TriangleDown(this, @"AlMa2 Diamond down_1"+CurrentBar, true, 0, Close[0] , Brushes.DarkRed);
					Draw.Text(this, "t1"+CurrentBar, "S "+Close[0], 0, High[1]+1);
					shortPos = true;
					entryAvgPrice = Close[0];
					
				}
			}
		}
		
		private bool isEMAsDeltaOK(SMA ema)
		{
			double s = Slope(ema,2,0);
			return s>emasDelta;
		}
		
		private bool isEMAsDeltaOK2(SMA ema)
		{			
			double s = Slope(ema,2,0);
			return Math.Abs(s)>emasDelta;
		}
		
		private bool isMACDsDeltaOK(MACD macd1)
		{
			Double defValue = Math.Abs(Slope(macd1.Default,2,0));
			Double defAvg = Math.Abs(Slope(macd1.Avg,2,0));
						
			return defValue>macdsDelta;
		}
		
		#region Properties
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="PpoRange", Order=1, GroupName="Parameters")]
		public double PpoRange
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MACDRange", Order=2, GroupName="Parameters")]
		public double MACDRange
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="EntryTolerance", Order=3, GroupName="Parameters")]
		public int EntryTolerance
		{ get; set; }

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="StopLoss", Order=4, GroupName="Parameters")]
		public int StopLoss
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, 1)]
		[Display(Name="Live Trade?", Order=5, GroupName="Parameters")]
		public int lTrade
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.001, 200.00)]
		[Display(Name="EMAs variation", Order=6, GroupName="Parameters")]
		public double emasDelta
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.001, 10.00)]
		[Display(Name="MACDs variation", Order=7, GroupName="Parameters")]
		public double macdsDelta
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlMaCrossers[] cacheAlMaCrossers;
		public AlMaCrossers AlMaCrossers(double ppoRange, double mACDRange, int entryTolerance, int stopLoss, int lTrade, double emasDelta, double macdsDelta)
		{
			return AlMaCrossers(Input, ppoRange, mACDRange, entryTolerance, stopLoss, lTrade, emasDelta, macdsDelta);
		}

		public AlMaCrossers AlMaCrossers(ISeries<double> input, double ppoRange, double mACDRange, int entryTolerance, int stopLoss, int lTrade, double emasDelta, double macdsDelta)
		{
			if (cacheAlMaCrossers != null)
				for (int idx = 0; idx < cacheAlMaCrossers.Length; idx++)
					if (cacheAlMaCrossers[idx] != null && cacheAlMaCrossers[idx].PpoRange == ppoRange && cacheAlMaCrossers[idx].MACDRange == mACDRange && cacheAlMaCrossers[idx].EntryTolerance == entryTolerance && cacheAlMaCrossers[idx].StopLoss == stopLoss && cacheAlMaCrossers[idx].lTrade == lTrade && cacheAlMaCrossers[idx].emasDelta == emasDelta && cacheAlMaCrossers[idx].macdsDelta == macdsDelta && cacheAlMaCrossers[idx].EqualsInput(input))
						return cacheAlMaCrossers[idx];
			return CacheIndicator<AlMaCrossers>(new AlMaCrossers(){ PpoRange = ppoRange, MACDRange = mACDRange, EntryTolerance = entryTolerance, StopLoss = stopLoss, lTrade = lTrade, emasDelta = emasDelta, macdsDelta = macdsDelta }, input, ref cacheAlMaCrossers);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlMaCrossers AlMaCrossers(double ppoRange, double mACDRange, int entryTolerance, int stopLoss, int lTrade, double emasDelta, double macdsDelta)
		{
			return indicator.AlMaCrossers(Input, ppoRange, mACDRange, entryTolerance, stopLoss, lTrade, emasDelta, macdsDelta);
		}

		public Indicators.AlMaCrossers AlMaCrossers(ISeries<double> input , double ppoRange, double mACDRange, int entryTolerance, int stopLoss, int lTrade, double emasDelta, double macdsDelta)
		{
			return indicator.AlMaCrossers(input, ppoRange, mACDRange, entryTolerance, stopLoss, lTrade, emasDelta, macdsDelta);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlMaCrossers AlMaCrossers(double ppoRange, double mACDRange, int entryTolerance, int stopLoss, int lTrade, double emasDelta, double macdsDelta)
		{
			return indicator.AlMaCrossers(Input, ppoRange, mACDRange, entryTolerance, stopLoss, lTrade, emasDelta, macdsDelta);
		}

		public Indicators.AlMaCrossers AlMaCrossers(ISeries<double> input , double ppoRange, double mACDRange, int entryTolerance, int stopLoss, int lTrade, double emasDelta, double macdsDelta)
		{
			return indicator.AlMaCrossers(input, ppoRange, mACDRange, entryTolerance, stopLoss, lTrade, emasDelta, macdsDelta);
		}
	}
}

#endregion
