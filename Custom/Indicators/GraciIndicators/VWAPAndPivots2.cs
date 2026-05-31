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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.GraciIndicators
{
	public class VWAPAndPivots2 : Indicator
	{
	    private int point_lb = 5;
	    private int point_rb = 5;
	    private Series<double> runningPh;
	    private Series<double> runningPl;
	    private Series<double> oh;
	    private Series<double> ol;
	    private Series<bool> isStrongBuy;
	    private Series<bool> isStrongSell;
	    private Series<double> source;
	
	

		private EMA ema3;
	    private Series<double> ema11;
	    private Series<double> ema48;
	    private Series<double> ema200;
	    private Series<double> wma13;
	    private Series<double> wma48;
	    private Series<double> wma200;
	    //private Series<double> rsi;
	    private Series<double> macd;
	    private Series<double> signal;
	    private Series<double> hist;
	    private Series<double> runningPivotHigh;
	    private Series<double> runningPivotLow;
	    private Series<double> cto;
	    private Series<double> bullPower;
	    private Series<double> bearPower;
	    private Series<double> bullVolume;
	    private Series<double> bearVolume;
	    private Series<double> delta;
	    private Series<double> cvd;
	    private Series<double> cvdMa;
		
		    // Multi-timeframe series
	    private Series<double> open3m;
	    private Series<double> open5m;
	    private Series<double> open8m;
	    private Series<double> open10m;
	    private Series<double> avg3m8;
	    private Series<double> avg3m;
	    private Series<double> avg5m;
	    private Series<double> avg8m;
	    private Series<double> avg10m;
	    private Series<double> close10;
	    private Series<double> wma10_11;
	    private Series<double> wma10_48;
	    private Series<double> close8;
	    private Series<double> wma8_11;
	    private Series<double> low1d;
		
		private double c1;
	    private double c2;
	    private double c3;
	    private double c4; 
	    private double c5;
	    
	    private Series<double> i1;
	    private Series<double> i2;
	    private Series<double> i3;
	    private Series<double> i4;
	    private Series<double> i5;
	    private Series<double> i6;
	    
	    private Series<double> Cto;
	    //private Series<double> ema3;
	    
	    private Series<bool> is_cross_up;
	    private Series<bool> is_cross_down;
	    
	    private SMA sma;
	    private RSI rsi;

	   protected override void OnStateChange()
        {



            if (State == State.SetDefaults)
            {
	            Description = "Converted from PineScript";
	            Name = "VWAPAndPivots2";
	            Calculate = Calculate.OnBarClose;
	            IsOverlay = true;
	            DisplayInDataBox = true;
	            DrawOnPricePanel = true;
	            DrawHorizontalGridLines = true;
	            DrawVerticalGridLines = true;
	            PaintPriceMarkers = true;
	            ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				
	
            }
            else if (State == State.DataLoaded)
            {	
				
				InitializeSeries();
				c1 = 2.0 / ((6 - 1.0) / 2.0 + 1.0 + 1.0);  
	            c2 = 1 - c1;
	            c3 = 3.0 * (0.4 * 0.4 + 0.4 * 0.4 * 0.4);
	            c4 = -3.0 * (2.0 * 0.4 * 0.4 + 0.4 + 0.4 * 0.4 * 0.4);
	            c5 = 3.0 * 0.4 + 1.0 + 0.4 * 0.4 * 0.4 + 3.0 * 0.4 * 0.4;
				sma = SMA(Close, 3);
            	rsi = RSI(Close, 3, 3);
            }
			else if (State == State.Configure)
            {
                // Add additional data series
                AddDataSeries(BarsPeriodType.Minute, 3);
                AddDataSeries(BarsPeriodType.Minute, 5);
                AddDataSeries(BarsPeriodType.Minute, 8);
                AddDataSeries(BarsPeriodType.Minute, 10);
                AddDataSeries(BarsPeriodType.Minute, 12);
                AddDataSeries(BarsPeriodType.Day, 1);
            }
        }
		
		 private void InitializeSeries()
        {
			//pivots
            runningPh = new Series<double>(this);
            runningPl = new Series<double>(this);
            oh = new Series<double>(this);
            ol = new Series<double>(this);
            isStrongBuy = new Series<bool>(this);
            isStrongSell = new Series<bool>(this);
            source = new Series<double>(this);      
		
            // Initialize main series
            //ema3 = new Series<double>(this);
            ema11 = new Series<double>(this);
            ema48 = new Series<double>(this);
            ema200 = new Series<double>(this);
            wma13 = new Series<double>(this);
            wma48 = new Series<double>(this);
            wma200 = new Series<double>(this);
            //rsi = new Series<double>(this);
            macd = new Series<double>(this);
            signal = new Series<double>(this);
            hist = new Series<double>(this);
            runningPivotHigh = new Series<double>(this);
            runningPivotLow = new Series<double>(this);
            cto = new Series<double>(this);
            bullPower = new Series<double>(this);
            bearPower = new Series<double>(this);
            bullVolume = new Series<double>(this);
            bearVolume = new Series<double>(this);
            delta = new Series<double>(this);
            cvd = new Series<double>(this);
            cvdMa = new Series<double>(this);

            // Initialize multi-timeframe series
            open3m = new Series<double>(this);
            open5m = new Series<double>(this);
            open8m = new Series<double>(this);
            open10m = new Series<double>(this);
            avg3m8 = new Series<double>(this);
            avg3m = new Series<double>(this);
            avg5m = new Series<double>(this);
            avg8m = new Series<double>(this);
            avg10m = new Series<double>(this);
            close10 = new Series<double>(this);
            wma10_11 = new Series<double>(this);
            wma10_48 = new Series<double>(this);
            close8 = new Series<double>(this);
            wma8_11 = new Series<double>(this);
            low1d = new Series<double>(this);
			
			
			 Cto = new Series<double>(this);
            //ema3 = new Series<double>(this);
            
            is_cross_up = new Series<bool>(this);
            is_cross_down = new Series<bool>(this);
        }

    protected override void OnBarUpdate()
    {
        if (CurrentBar < 40) return;
		
		open3m[0] = Opens[1][0];
    	open5m[0] = Opens[2][0];
    	open8m[0] = Opens[3][0];
    	open10m[0] = Opens[4][0];
		
		
		avg3m8[0] = (open3m[7] + open3m[6] + open3m[5] + open3m[4] + open3m[3] + open3m[2] + open3m[1] + open3m[0])/8;
		avg3m[0] = (open3m[2] + open3m[1] + open3m[0])/3;
		//avg_5m = (open5m[10] + open5m[9] + open5m[8] + open5m[7] + open5m[6] + open5m[5] + open5m[4] + open5m[3] + open5m[2] + open5m[1]+ open3m)/10
		avg5m[0] = (open5m[4] + open5m[3] + open5m[2] + open5m[1]+ open3m[0])/5;
		//avg_8m = (open8m[10] + open8m[9] + open8m[8] + open8m[7] + open8m[6] + open8m[5] + open8m[4] + open8m[3] + open8m[2] + open8m[1]+ open3m)/10
		avg8m[0] = (open8m[7] + open8m[6] + open8m[5] + open8m[4] + open8m[3] + open8m[2] + open8m[1]+ open3m[0])/8;
		//avg_10m = (open10m[10] + open10m[9] + open10m[8] + open10m[7] + open10m[6] + open10m[5] + open10m[4] + open10m[3] + open10m[2] + open10m[1]+ open3m)/10
		avg10m[0] = (open10m[9] + open10m[8] + open10m[7] + open10m[6] + open10m[5] + open10m[4] + open10m[3] + open10m[2] + open10m[1]+ open3m[0])/10;

		

        oh[0] = Math.Max(Math.Max(Open[2], Open[1]), Open[0]);
        ol[0] = Math.Min(Math.Min(Open[2], Open[1]), Open[0]);

        double? point_ph = CalculatePivotHigh(oh, point_lb, point_rb);
        double? point_pl = CalculatePivotLow(ol, point_lb, point_rb);

        if (point_ph.HasValue)
            runningPh[0] = point_ph.Value;
        else
            runningPh[0] = runningPh[1];

        if (point_pl.HasValue)
            runningPl[0] = point_pl.Value;
        else
            runningPl[0] = runningPl[1];

		
		
		
        Draw.Text(this, "R_" + CurrentBar, @"▪", 0, runningPh[0], Brushes.Red);
        Draw.Text(this, "R_Offset_" + CurrentBar, @"▪", point_rb, runningPh[0], Brushes.Red);
        Draw.Text(this, "S_" + CurrentBar, @"▪", 0, runningPl[0], Brushes.Blue);
        Draw.Text(this, "S_Offset_" + CurrentBar, @"▪", point_rb, runningPl[0], Brushes.Blue);
		
		//Draw.Text(this, Convert.ToString(LongFilterOn) + Convert.ToString(CurrentBars[0]), @"." + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Green );	

        source[0] = (Open[9] + Open[8] + Open[7] + Open[6] + Open[5] + Open[4] + Open[3] + Open[2] + Open[1] + Open[0]) / 10;

        if (runningPh[0] != 0 && CrossAbove(source, runningPh, 1))
        {
            isStrongBuy[0] = true;
            isStrongSell[0] = false;
        }
        else if (runningPl[0] != 0 && CrossBelow(source, runningPh, 1))
        {
            isStrongBuy[0] = false;
        }
        else if (runningPl[0] != 0 && CrossBelow(source, runningPl, 1))
        {
            isStrongSell[0] = true;
            isStrongBuy[0] = false;
        }
        else if (runningPl[0] != 0 && CrossAbove(source, runningPl, 1))
        {
            isStrongSell[0] = false;
        }
        else
        {
            isStrongBuy[0] = isStrongBuy[1];
            isStrongSell[0] = isStrongSell[1];
        }
		
		 // Assign variables
        i1[0] = c1 * source[0] + c2 * i1[1];
        i2[0] = c1 * i1[0] + c2 * i2[1];
        i3[0] = c1 * i2[0] + c2 * i3[1];
        i4[0] = c1 * i3[0] + c2 * i4[1];
        i5[0] = c1 * i4[0] + c2 * i5[1];
        i6[0] = c1 * i5[0] + c2 * i6[1];
        
        Cto[0] = -0.4 * 0.4 * 0.4 * i6[0] + c3 * i5[0] + c4 * i4[0] + c5 * i3[0];
        ema3 = EMA(source, 3);
        
        // Detect crosses
        if (CrossAbove(ema3, Cto,1)) 
            is_cross_up[0] = true;
        if (CrossBelow(ema3, Cto,1)) 
            is_cross_down[0] = true;
      
    }

    private double? CalculatePivotHigh(Series<double> series, int leftBars, int rightBars)
    {
        if (CurrentBar < leftBars + rightBars) return null;

        double currentValue = series[rightBars];
        for (int i = rightBars - 1; i <= rightBars + leftBars; i++)
        {
            if (series[i] > currentValue) return null;
        }
        return currentValue;
    }

    private double? CalculatePivotLow(Series<double> series, int leftBars, int rightBars)
    {
        if (CurrentBar < leftBars + rightBars) return null;

        double currentValue = series[rightBars];
        for (int i = rightBars - 1; i <= rightBars + leftBars; i++)
        {
            if (series[i] < currentValue) return null;
        }
        return currentValue;
    }
	
	private void UpdateMainTimeframe()
        {
            //ema3[0] = EMA(Close, 3)[0];
            ema11[0] = EMA(Close, 11)[0];
            ema48[0] = EMA(Close, 48)[0];
            ema200[0] = EMA(Close, 200)[0];
            wma13[0] = WMA(Close, 13)[0];
            wma48[0] = WMA(Close, 48)[0];
            wma200[0] = WMA(Close, 200)[0];
            //rsi[0] = RSI(3, 3)[0];

            // MACD calculation
            double fastMA = EMA(Close, 7)[0];
            double slowMA = EMA(Close, 20)[0];
            macd[0] = fastMA - slowMA;
            signal[0] = EMA(macd, 7)[0];
            hist[0] = macd[0] - signal[0];

            // Implement other main timeframe calculations here
        }

        private void UpdateMultiTimeframe()
        {
            // 3-minute data
            open3m[0] = Opens[1][0];  // BarsArray[1] is the 3-minute series

            // 5-minute data
            open5m[0] = Opens[2][0];  // BarsArray[2] is the 5-minute series

            // 8-minute data
            open8m[0] = Opens[3][0];  // BarsArray[3] is the 8-minute series

            // 10-minute data
            open10m[0] = Opens[4][0]; // BarsArray[4] is the 10-minute series

            // 12-minute data (for close10 and WMAs)
            close10[0] = Closes[5][0]; // BarsArray[5] is the 12-minute series
            wma10_11[0] = WMA(Closes[5], 4)[0];
            wma10_48[0] = WMA(Closes[5], 9)[0];

            // 11-minute data (assumed to be close8 and wma8_11)
            close8[0] = Closes[3][0];  // Using 8-minute data as an approximation
            wma8_11[0] = WMA(Closes[3], 3)[0];

            // Daily data
            low1d[0] = Lows[6][0];  // BarsArray[6] is the daily series

            // Calculate averages
            avg3m8[0] = CalculateAverage(open3m, 8);
            avg3m[0] = CalculateAverage(open3m, 3);
            avg5m[0] = CalculateAverage(open5m, 5);
            avg8m[0] = CalculateAverage(open8m, 8);
            avg10m[0] = CalculateAverage(open10m, 10);
        }

        private double CalculateAverage(Series<double> series, int period)
        {
            double sum = 0;
            for (int i = 0; i < period; i++)
            {
                if (CurrentBar >= i)
                    sum += series[i];
                else
                    return 0; // Not enough data
            }
            return sum / period;
        }
}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GraciIndicators.VWAPAndPivots2[] cacheVWAPAndPivots2;
		public GraciIndicators.VWAPAndPivots2 VWAPAndPivots2()
		{
			return VWAPAndPivots2(Input);
		}

		public GraciIndicators.VWAPAndPivots2 VWAPAndPivots2(ISeries<double> input)
		{
			if (cacheVWAPAndPivots2 != null)
				for (int idx = 0; idx < cacheVWAPAndPivots2.Length; idx++)
					if (cacheVWAPAndPivots2[idx] != null &&  cacheVWAPAndPivots2[idx].EqualsInput(input))
						return cacheVWAPAndPivots2[idx];
			return CacheIndicator<GraciIndicators.VWAPAndPivots2>(new GraciIndicators.VWAPAndPivots2(), input, ref cacheVWAPAndPivots2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GraciIndicators.VWAPAndPivots2 VWAPAndPivots2()
		{
			return indicator.VWAPAndPivots2(Input);
		}

		public Indicators.GraciIndicators.VWAPAndPivots2 VWAPAndPivots2(ISeries<double> input )
		{
			return indicator.VWAPAndPivots2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GraciIndicators.VWAPAndPivots2 VWAPAndPivots2()
		{
			return indicator.VWAPAndPivots2(Input);
		}

		public Indicators.GraciIndicators.VWAPAndPivots2 VWAPAndPivots2(ISeries<double> input )
		{
			return indicator.VWAPAndPivots2(input);
		}
	}
}

#endregion
