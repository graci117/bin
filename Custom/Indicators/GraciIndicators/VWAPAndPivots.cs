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
	public class VWAPAndPivots : Indicator
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

    
	   protected override void OnStateChange()
        {



            if (State == State.SetDefaults)
            {
            Description = "Converted from PineScript";
            Name = "VWAPAndPivots";
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
	            runningPh = new Series<double>(this);
	            runningPl = new Series<double>(this);
	            oh = new Series<double>(this);
	            ol = new Series<double>(this);
	            isStrongBuy = new Series<bool>(this);
	            isStrongSell = new Series<bool>(this);
	            source = new Series<double>(this);          
            }
        }

    protected override void OnBarUpdate()
    {
        if (CurrentBar < 40) return;

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
}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GraciIndicators.VWAPAndPivots[] cacheVWAPAndPivots;
		public GraciIndicators.VWAPAndPivots VWAPAndPivots()
		{
			return VWAPAndPivots(Input);
		}

		public GraciIndicators.VWAPAndPivots VWAPAndPivots(ISeries<double> input)
		{
			if (cacheVWAPAndPivots != null)
				for (int idx = 0; idx < cacheVWAPAndPivots.Length; idx++)
					if (cacheVWAPAndPivots[idx] != null &&  cacheVWAPAndPivots[idx].EqualsInput(input))
						return cacheVWAPAndPivots[idx];
			return CacheIndicator<GraciIndicators.VWAPAndPivots>(new GraciIndicators.VWAPAndPivots(), input, ref cacheVWAPAndPivots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GraciIndicators.VWAPAndPivots VWAPAndPivots()
		{
			return indicator.VWAPAndPivots(Input);
		}

		public Indicators.GraciIndicators.VWAPAndPivots VWAPAndPivots(ISeries<double> input )
		{
			return indicator.VWAPAndPivots(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GraciIndicators.VWAPAndPivots VWAPAndPivots()
		{
			return indicator.VWAPAndPivots(Input);
		}

		public Indicators.GraciIndicators.VWAPAndPivots VWAPAndPivots(ISeries<double> input )
		{
			return indicator.VWAPAndPivots(input);
		}
	}
}

#endregion
