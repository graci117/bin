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
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber_SignalMod
{
	public class B4v3_TSMod : Indicator
	{
		private bool 				RSM_rsiGreen, RSM_rsiRed, RSM_stochGreen, RSM_stochRed, RSM_macdGreen, RSM_macdRed, RSM_Buy, RSM_Sell, Strategy_BuySignal, 
									Strategy_SellSignal, MACDBB_Buy, MACDBB_Sell, isSignalPlotted, STOCHSCALPER_squeeze, SQZ_Squeeze_Signal;
		private double 				RSM_RSI, RSM_StochSlowK, RSI_IFT_AvgRSI, MACDBB_Upper, MACDBB_Lower, MACDBB_Midline, STOCHSCALPER_oSS, STOCHSCALPER_hSS, 
									STOCHSCALPER_lSS, STOCHSCALPER_mean, STOCHSCALPER_sd, STOCHSCALPER_atr, SQZ_Squeeze_Low;
		private Series<double> 		MACDBB_Data, RSM_Stoch_Val, FST_minL, FST_maxH, FST_hh, FST_ll, FST_h, FST_l, FST_Direction, FST_trend, RSI_IFT_R, RSI_IFT_InverseRSI, 
									RSI_IFT_Direction, STOCHSCALPER_cSS, STOCHSCALPER_tr, SQZ_Bandwidth, candleColorOutput, verticalLineOutput, buySellOutput;
		private Series<bool>	 	Strategy_Buy, Strategy_Sell, SQZ_Squeeze, BullBear_Buy, BullBear_Sell;
		
		private Brush 				barColor					= Brushes.Gray;
		private Brush				bullBrush					= Brushes.Lime;
		private Brush				bearBrush					= Brushes.Red;
		private Brush				signalBackBrush				= Brushes.Black;
		private SimpleFont			triangleFont				= null;
		private int					triangleFontSize			= 7;
		private string				triangleStringUp			= "5";
		private string				triangleStringDown			= "6";
		private double				margin						= 0.0;
		private ATR					barVolatility;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Based on the v3.0.1 release of the B4 Indicator";
				Name										= "B4v3_TSMod";
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
				IsSuspendedWhileInactive				= true;
				
				ShowBullBearVerticalLines				= true;
				MACDLength								= 5;
				MACDBBfastLength						= 12;
				MACDBBslowLength						= 81;//26;
				MACDBBbandLength						= 33;//15;
				MACDBBnumOfDev							= 1.5;//1;
				SqueezeLength							= 30;
				FibSuperTrendLength						= 21;//11;
				FibSuperTrendRetrace					= 23.6;
				FibSuperTrendUseHighLow					= true;
				BullBear_Include_FST					= true;
				BullBear_Include_RSI_IFT 				= true;
				ColorBars								= false;//true;
				
//				AddPlot(Brushes.Orange, "MACDBB Line");
				AddPlot(new Stroke(Brushes.Firebrick, 2), PlotStyle.Line, "MACDBB Line");
				AddPlot(new Stroke(Brushes.DimGray, 2), PlotStyle.Dot, "MACDBB Dots");
				AddPlot(new Stroke(Brushes.Gray), PlotStyle.Bar, "MACD Diff");
				AddPlot(Brushes.White, "BB Upper");
				AddPlot(Brushes.White, "BB Lower");
				AddPlot(Brushes.White, "BB Midline");
				AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Dot, "Squeeze Dots");
				AddPlot(Brushes.Transparent, "Buy/Sell Output");
				//AddLine(Brushes.White,	0, "Squeeze Dots");
				//Lines[0].DashStyleHelper = DashStyleHelper.Dot;
				//Lines[0].Width = 1;
				Plots[2].PlotStyle = PlotStyle.Bar;
				Plots[2].AutoWidth = true;
			}
			else if (State == State.DataLoaded)
			{
				RSM_Stoch_Val = new Series<double>(this);
				FST_minL = new Series<double>(this);
				FST_maxH = new Series<double>(this);
				FST_h = new Series<double>(this);
				FST_l = new Series<double>(this);
				FST_hh = new Series<double>(this);
				FST_ll = new Series<double>(this);
				FST_trend = new Series<double>(this);
				FST_Direction = new Series<double>(this);
				RSI_IFT_R = new Series<double>(this);
				RSI_IFT_InverseRSI = new Series<double>(this);
				RSI_IFT_Direction = new Series<double>(this);
				Strategy_Buy = new Series<bool>(this);
				Strategy_Sell = new Series<bool>(this);
				barVolatility = ATR(Closes[0], 256);
				MACDBB_Data = new Series<double>(this);
				STOCHSCALPER_cSS = new Series<double>(this);
				STOCHSCALPER_tr = new Series<double>(this);
				SQZ_Bandwidth = new Series<double>(this);
				SQZ_Squeeze = new Series<bool>(this);
				BullBear_Buy = new Series<bool>(this);
				BullBear_Sell = new Series<bool>(this);
				candleColorOutput = new Series<double>(this);
				verticalLineOutput = new Series<double>(this);
				buySellOutput = new Series<double>(this);
			}
			else if(State == State.Historical)
			{
				triangleFont = new SimpleFont("Webdings", 3*triangleFontSize);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < MACDBBslowLength + 5)
				return;
			
			// MACDBB
			MACDBB_Data[0] = MACD(MACDBBfastLength, MACDBBslowLength, MACDLength)[0]; 

			MACDBB_Upper = Bollinger(MACDBB_Data, MACDBBnumOfDev, MACDBBbandLength).Upper[0];
			MACDBB_Lower = Bollinger(MACDBB_Data, MACDBBnumOfDev, MACDBBbandLength).Lower[0];
			MACDBB_Midline = Bollinger(MACDBB_Data, MACDBBnumOfDev, MACDBBbandLength).Middle[0];
	
			MACDBB_Line[0] = MACDBB_Data[0];
			MACDBB_Dots[0] = MACDBB_Data[0];
			
			PlotBrushes[0][0] = MACDBB_Line[0] > MACDBB_Line[1] && MACDBB_Line[0] >= MACDBB_Upper ? Brushes.Lime 
			    : MACDBB_Line[0] < MACDBB_Line[1] && MACDBB_Line[0] >= MACDBB_Upper ? Brushes.DarkGreen
			    : MACDBB_Line[0] < MACDBB_Line[1] && MACDBB_Line[0] <= MACDBB_Lower ? Brushes.Red 
				: MACDBB_Line[0] > MACDBB_Line[1] && MACDBB_Line[0] <= MACDBB_Lower ? Brushes.DarkRed 
			    : Brushes.Gray;
			
			PlotBrushes[1][0] = MACDBB_Line[0] > MACDBB_Line[1] && MACDBB_Line[0] >= MACDBB_Upper ? Brushes.Lime 
				: MACDBB_Line[0] < MACDBB_Line[1] && MACDBB_Line[0] >= MACDBB_Upper ? Brushes.DarkGreen
			    : MACDBB_Line[0] < MACDBB_Line[1] && MACDBB_Line[0] <= MACDBB_Lower ? Brushes.Red 
				: MACDBB_Line[0] > MACDBB_Line[1] && MACDBB_Line[0] <= MACDBB_Lower ? Brushes.DarkRed 
			    : Brushes.Gray;

			MACDBB_Buy = MACDBB_Data[0] > MACDBB_Upper;
			MACDBB_Sell = MACDBB_Data[0] <= MACDBB_Lower;
			
			BBupper[0] = MACDBB_Upper;
			BBlower[0] = MACDBB_Lower;
			BBmidline[0] = MACDBB_Midline;
			
			// RSM 
			RSM_MACD_Diff[0] = MACD(12, 26, 9).Diff[0];
			RSM_RSI = RSI(7,1)[0];
			RSM_Stoch_Val[0] = 100 * (Close[0] - MIN(Low, 14)[0]) / (MAX(High, 14)[0] - MIN(Low, 14)[0]);
			RSM_StochSlowK = SMA(SMA(RSM_Stoch_Val, 3), 3)[0];
			
			RSM_rsiGreen = RSM_RSI >= 50;
			RSM_rsiRed = RSM_RSI < 50;
			RSM_stochGreen = RSM_StochSlowK >= 50;
			RSM_stochRed = RSM_StochSlowK < 50;
			RSM_macdGreen = RSM_MACD_Diff[0] >= 0;
			RSM_macdRed = RSM_MACD_Diff[0] < 0;
			RSM_Buy = RSM_rsiGreen && RSM_stochGreen && RSM_macdGreen;
			RSM_Sell = RSM_rsiRed && RSM_stochRed && RSM_macdRed;
			
			if (ColorBars)
			{
				barColor = RSM_Buy ? Brushes.Lime : RSM_Sell ? Brushes.Red : Brushes.Gray ;	
				BarBrushes[0] = barColor;
				CandleOutlineBrushes[0] = barColor; 
			}
			
			candleColorOutput[0] = RSM_Buy ? 1 : RSM_Sell ? -1 : 0;	
			PlotBrushes[2][0] = RSM_MACD_Diff[0] > 0 && RSM_MACD_Diff[0] > RSM_MACD_Diff[1] ? Brushes.Lime 
				:  RSM_MACD_Diff[0] > 0 ? Brushes.DarkGreen 
				:  RSM_MACD_Diff[0] < 0 &&  RSM_MACD_Diff[0] <  RSM_MACD_Diff[1] ? Brushes.Red 
				: Brushes.DarkRed;
			
			// RSI IFT Inverse Fisher Transform
			RSI_IFT_R[0] = RSI(Close, 5, 1)[0] - 50;
			RSI_IFT_AvgRSI = EMA(RSI_IFT_R, 9)[0];
			RSI_IFT_InverseRSI[0] = (Math.Exp(2 * RSI_IFT_AvgRSI) - 1) / (Math.Exp(2 * RSI_IFT_AvgRSI) + 1); 
			RSI_IFT_Direction[0] = CurrentBar == 0 ? 0
	            : (RSI_IFT_InverseRSI[1] > 0) && (RSI_IFT_InverseRSI[0] < 0) ? -1
	            : (RSI_IFT_InverseRSI[0] > 0) && (RSI_IFT_InverseRSI[1] < 0) ? 1
	            : RSI_IFT_Direction[1];
			
			// SQUEEZE
			SQZ_Bandwidth[0] = MACDBB_Upper - MACDBB_Lower;
			SQZ_Squeeze_Low = MIN(SQZ_Bandwidth, SqueezeLength)[0];
			SQZ_Squeeze[0] = SQZ_Bandwidth[0] == SQZ_Squeeze_Low;
			SQZ_Squeeze_Signal = !SQZ_Squeeze[1] && SQZ_Squeeze[0];
			SqueezeDots[0] = 0;
			
			// STOCHASTIC SCALPER
			STOCHSCALPER_oSS = (Open[1] + Close[1]) / 2;
			STOCHSCALPER_hSS = Math.Max(High[0], Close[1]);
			STOCHSCALPER_lSS = Math.Min(Low[0], Close[1]);
			STOCHSCALPER_cSS[0] = (STOCHSCALPER_oSS + STOCHSCALPER_hSS + STOCHSCALPER_lSS + Close[0]) / 4;

			STOCHSCALPER_mean = SMA(STOCHSCALPER_cSS, 20)[0];
			STOCHSCALPER_sd = StdDev(STOCHSCALPER_cSS, 20)[0];
			STOCHSCALPER_tr[0] = STOCHSCALPER_hSS - STOCHSCALPER_lSS;
			STOCHSCALPER_atr = SMA(STOCHSCALPER_tr, 20)[0];
			STOCHSCALPER_squeeze = (STOCHSCALPER_mean + (2 * STOCHSCALPER_sd)) < (STOCHSCALPER_mean + (1.5 * STOCHSCALPER_atr)) ? true : false;
			
			// Fib SuperTrend
			FST_h[0] = FibSuperTrendUseHighLow ? High[0] : Close[0];
			FST_l[0] = FibSuperTrendUseHighLow ? Low[0] : Close[0];
			FST_minL[0] = MIN(FST_l, FibSuperTrendLength)[0];
			FST_maxH[0] = MAX(FST_h, FibSuperTrendLength)[0];

			FST_hh[0] = FST_h[0] > FST_maxH[1] ? FST_h[0] : FST_hh[1];
			FST_ll[0] = FST_l[0] < FST_minL[1] ? FST_l[0] : FST_ll[1];
			FST_trend[0] = FST_h[0] > FST_maxH[1] ? 1 : FST_l[0] < FST_minL[1] ? -1 : FST_trend[1];
			FST_Direction[0] = CurrentBar == 0 ? 0 : FST_trend[0] != 1 ? -1 : FST_trend[0] == 1 ? 1 : FST_Direction[1];
			
			// Buy/Sell Signals
			BullBear_Buy[0] = (!BullBear_Include_FST || FST_Direction[0] == 1) &&
                   (!BullBear_Include_RSI_IFT || RSI_IFT_Direction[0] == 1);
			BullBear_Sell[0] = (!BullBear_Include_FST || FST_Direction[0] == -1) &&
                    (!BullBear_Include_RSI_IFT || RSI_IFT_Direction[0] == -1);
			
			Strategy_Buy[0] = BullBear_Buy[0] && MACDBB_Buy && RSM_Buy;
			Strategy_Sell[0] = BullBear_Sell[0] && MACDBB_Sell && RSM_Sell;
			Strategy_BuySignal = Strategy_Buy[0] && !Strategy_Buy[1];
			Strategy_SellSignal = Strategy_Sell[0] && !Strategy_Sell[1];
			
			// Plotting vertical lines and Buy/Sell signals
			if(IsFirstTickOfBar)
			{	
				margin = barVolatility[1];
				isSignalPlotted = false;
			}	
			
			if (ShowBullBearVerticalLines && BullBear_Buy[0] && !BullBear_Buy[1]) 
			{
				BackBrush = Brushes.DarkGreen;
				verticalLineOutput[0] = 1;
			}
			else if (ShowBullBearVerticalLines && BullBear_Sell[0] && !BullBear_Sell[1]) 
			{
				BackBrush = Brushes.DarkRed;
				verticalLineOutput[0] = -1;
			}
			else
			{
				verticalLineOutput[0] = 0;
			}
			
			if (Strategy_BuySignal)
			{	
				Draw.Text(this, "isGreen" + CurrentBar, false, triangleStringUp, 0, Low[0] - 0.5*margin, - 3*triangleFontSize, bullBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, signalBackBrush, 50);
				isSignalPlotted = true;
				BuySellOutput[0] = 1;
			}	
			else if (Strategy_SellSignal)
			{	
				Draw.Text(this, "isRed" + CurrentBar, false, triangleStringDown, 0, High[0] + 0.5*margin, 6*triangleFontSize, bearBrush, triangleFont, TextAlignment.Center, Brushes.Transparent, signalBackBrush, 50);
				isSignalPlotted = true;
				BuySellOutput[0] = -1;
			}	
			else if (isSignalPlotted)
			{	
				    // You need to know what was the last signal to remove the correct object
				if (BuySellOutput[1] == 1) {
					RemoveDrawObject("isGreen" + CurrentBar);
				} else if (BuySellOutput[1] == -1) {
					RemoveDrawObject("isRed" + CurrentBar);
				}
				isSignalPlotted = false;
				BuySellOutput[0] = 0;
			}
			else
			{
				BuySellOutput[0] = 0;	
			}
			
			if (SQZ_Squeeze[0])
			{
				PlotBrushes[3][0] = Brushes.DarkOrange;
				PlotBrushes[4][0] = Brushes.DarkOrange;
				PlotBrushes[5][0] = Brushes.DarkOrange;
				PlotBrushes[6][0] = Brushes.White;
			}
			else
			{
				PlotBrushes[3][0] = Brushes.White;
				PlotBrushes[4][0] = Brushes.White;
				PlotBrushes[5][0] = Brushes.White;
				PlotBrushes[6][0] = Brushes.Transparent;
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ShowBullBearVerticalLines", Description="Show vertical lines for bullish or bearish direction", Order=1, GroupName="Plots & Appearance")]
		public bool ShowBullBearVerticalLines
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ColorBars", Order=2, GroupName="Plots & Appearance")]
		public bool ColorBars
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="BullBear_Include_FST", Description="Include Fibonacci SuperTrend in the vertical line strategy", Order=3, GroupName="Signals")]
		public bool BullBear_Include_FST
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="BullBear_Include_RSI_IFT", Description="Include RSI IFT in the vertical line strategy", Order=4, GroupName="Signals")]
		public bool BullBear_Include_RSI_IFT  
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACD Length", Order=5, GroupName="MACD")]
		public int MACDLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACDBBfastLength", Order=6, GroupName="MACD")]
		public int MACDBBfastLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACDBBslowLength", Order=7, GroupName="MACD")]
		public int MACDBBslowLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MACDBBbandLength", Order=8, GroupName="MACD")]
		public int MACDBBbandLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="MACDBBnumOfDev", Order=9, GroupName="MACD")]
		public double MACDBBnumOfDev
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FibSuperTrendLength", Order=10, GroupName="Fib SuperTrend")]
		public int FibSuperTrendLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="FibSuperTrendRetrace", Order=11, GroupName="Fib SuperTrend")]
		public double FibSuperTrendRetrace
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FibSuperTrendUseHighLow", Description="False uses Close", Order=12, GroupName="Fib SuperTrend")]
		public bool FibSuperTrendUseHighLow
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Squeeze Length", Order=13, GroupName="Misc")]
		public int SqueezeLength
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MACDBB_Line
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MACDBB_Dots
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RSM_MACD_Diff
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BBupper
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BBlower
		{
			get { return Values[4]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BBmidline
		{
			get { return Values[5]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SqueezeDots
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VerticalLineOutput
		{
			get { return verticalLineOutput; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySellOutput
		{
			get { return Values[7]; }
		}
				
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CandleColorOutput
		{
			get { return candleColorOutput; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber_SignalMod.B4v3_TSMod[] cacheB4v3_TSMod;
		public TradeSaber_SignalMod.B4v3_TSMod B4v3_TSMod(bool showBullBearVerticalLines, bool colorBars, bool bullBear_Include_FST, bool bullBear_Include_RSI_IFT, int mACDLength, int mACDBBfastLength, int mACDBBslowLength, int mACDBBbandLength, double mACDBBnumOfDev, int fibSuperTrendLength, double fibSuperTrendRetrace, bool fibSuperTrendUseHighLow, int squeezeLength)
		{
			return B4v3_TSMod(Input, showBullBearVerticalLines, colorBars, bullBear_Include_FST, bullBear_Include_RSI_IFT, mACDLength, mACDBBfastLength, mACDBBslowLength, mACDBBbandLength, mACDBBnumOfDev, fibSuperTrendLength, fibSuperTrendRetrace, fibSuperTrendUseHighLow, squeezeLength);
		}

		public TradeSaber_SignalMod.B4v3_TSMod B4v3_TSMod(ISeries<double> input, bool showBullBearVerticalLines, bool colorBars, bool bullBear_Include_FST, bool bullBear_Include_RSI_IFT, int mACDLength, int mACDBBfastLength, int mACDBBslowLength, int mACDBBbandLength, double mACDBBnumOfDev, int fibSuperTrendLength, double fibSuperTrendRetrace, bool fibSuperTrendUseHighLow, int squeezeLength)
		{
			if (cacheB4v3_TSMod != null)
				for (int idx = 0; idx < cacheB4v3_TSMod.Length; idx++)
					if (cacheB4v3_TSMod[idx] != null && cacheB4v3_TSMod[idx].ShowBullBearVerticalLines == showBullBearVerticalLines && cacheB4v3_TSMod[idx].ColorBars == colorBars && cacheB4v3_TSMod[idx].BullBear_Include_FST == bullBear_Include_FST && cacheB4v3_TSMod[idx].BullBear_Include_RSI_IFT == bullBear_Include_RSI_IFT && cacheB4v3_TSMod[idx].MACDLength == mACDLength && cacheB4v3_TSMod[idx].MACDBBfastLength == mACDBBfastLength && cacheB4v3_TSMod[idx].MACDBBslowLength == mACDBBslowLength && cacheB4v3_TSMod[idx].MACDBBbandLength == mACDBBbandLength && cacheB4v3_TSMod[idx].MACDBBnumOfDev == mACDBBnumOfDev && cacheB4v3_TSMod[idx].FibSuperTrendLength == fibSuperTrendLength && cacheB4v3_TSMod[idx].FibSuperTrendRetrace == fibSuperTrendRetrace && cacheB4v3_TSMod[idx].FibSuperTrendUseHighLow == fibSuperTrendUseHighLow && cacheB4v3_TSMod[idx].SqueezeLength == squeezeLength && cacheB4v3_TSMod[idx].EqualsInput(input))
						return cacheB4v3_TSMod[idx];
			return CacheIndicator<TradeSaber_SignalMod.B4v3_TSMod>(new TradeSaber_SignalMod.B4v3_TSMod(){ ShowBullBearVerticalLines = showBullBearVerticalLines, ColorBars = colorBars, BullBear_Include_FST = bullBear_Include_FST, BullBear_Include_RSI_IFT = bullBear_Include_RSI_IFT, MACDLength = mACDLength, MACDBBfastLength = mACDBBfastLength, MACDBBslowLength = mACDBBslowLength, MACDBBbandLength = mACDBBbandLength, MACDBBnumOfDev = mACDBBnumOfDev, FibSuperTrendLength = fibSuperTrendLength, FibSuperTrendRetrace = fibSuperTrendRetrace, FibSuperTrendUseHighLow = fibSuperTrendUseHighLow, SqueezeLength = squeezeLength }, input, ref cacheB4v3_TSMod);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber_SignalMod.B4v3_TSMod B4v3_TSMod(bool showBullBearVerticalLines, bool colorBars, bool bullBear_Include_FST, bool bullBear_Include_RSI_IFT, int mACDLength, int mACDBBfastLength, int mACDBBslowLength, int mACDBBbandLength, double mACDBBnumOfDev, int fibSuperTrendLength, double fibSuperTrendRetrace, bool fibSuperTrendUseHighLow, int squeezeLength)
		{
			return indicator.B4v3_TSMod(Input, showBullBearVerticalLines, colorBars, bullBear_Include_FST, bullBear_Include_RSI_IFT, mACDLength, mACDBBfastLength, mACDBBslowLength, mACDBBbandLength, mACDBBnumOfDev, fibSuperTrendLength, fibSuperTrendRetrace, fibSuperTrendUseHighLow, squeezeLength);
		}

		public Indicators.TradeSaber_SignalMod.B4v3_TSMod B4v3_TSMod(ISeries<double> input , bool showBullBearVerticalLines, bool colorBars, bool bullBear_Include_FST, bool bullBear_Include_RSI_IFT, int mACDLength, int mACDBBfastLength, int mACDBBslowLength, int mACDBBbandLength, double mACDBBnumOfDev, int fibSuperTrendLength, double fibSuperTrendRetrace, bool fibSuperTrendUseHighLow, int squeezeLength)
		{
			return indicator.B4v3_TSMod(input, showBullBearVerticalLines, colorBars, bullBear_Include_FST, bullBear_Include_RSI_IFT, mACDLength, mACDBBfastLength, mACDBBslowLength, mACDBBbandLength, mACDBBnumOfDev, fibSuperTrendLength, fibSuperTrendRetrace, fibSuperTrendUseHighLow, squeezeLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber_SignalMod.B4v3_TSMod B4v3_TSMod(bool showBullBearVerticalLines, bool colorBars, bool bullBear_Include_FST, bool bullBear_Include_RSI_IFT, int mACDLength, int mACDBBfastLength, int mACDBBslowLength, int mACDBBbandLength, double mACDBBnumOfDev, int fibSuperTrendLength, double fibSuperTrendRetrace, bool fibSuperTrendUseHighLow, int squeezeLength)
		{
			return indicator.B4v3_TSMod(Input, showBullBearVerticalLines, colorBars, bullBear_Include_FST, bullBear_Include_RSI_IFT, mACDLength, mACDBBfastLength, mACDBBslowLength, mACDBBbandLength, mACDBBnumOfDev, fibSuperTrendLength, fibSuperTrendRetrace, fibSuperTrendUseHighLow, squeezeLength);
		}

		public Indicators.TradeSaber_SignalMod.B4v3_TSMod B4v3_TSMod(ISeries<double> input , bool showBullBearVerticalLines, bool colorBars, bool bullBear_Include_FST, bool bullBear_Include_RSI_IFT, int mACDLength, int mACDBBfastLength, int mACDBBslowLength, int mACDBBbandLength, double mACDBBnumOfDev, int fibSuperTrendLength, double fibSuperTrendRetrace, bool fibSuperTrendUseHighLow, int squeezeLength)
		{
			return indicator.B4v3_TSMod(input, showBullBearVerticalLines, colorBars, bullBear_Include_FST, bullBear_Include_RSI_IFT, mACDLength, mACDBBfastLength, mACDBBslowLength, mACDBBbandLength, mACDBBnumOfDev, fibSuperTrendLength, fibSuperTrendRetrace, fibSuperTrendUseHighLow, squeezeLength);
		}
	}
}

#endregion
