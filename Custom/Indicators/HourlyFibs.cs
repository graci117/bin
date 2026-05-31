#region Using declarations
using System;
using System.Windows.Media;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.Gui;
using NinjaTrader.NinjaScript.Indicators;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Globalization;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{


public class HourlyFibs : Indicator
{
    private int currentHour = -1;
    private double high, low, mid, highExt, lowExt;
    private DateTime startTime, endTime;

    protected override void OnStateChange()
    {
        if (State == State.SetDefaults)
        {
            Description = "Draws hourly Fibonacci levels with extensions.";
            Name = "HourlyFibs";
            Calculate = Calculate.OnBarClose;
            IsOverlay = true;
            DisplayInDataBox = true;
            //PaintPriceLevels = false;
			
			AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "HighLine");
            AddPlot(new Stroke(Brushes.White, 2), PlotStyle.Line, "LowLine");
            AddPlot(new Stroke(Brushes.Yellow, DashStyleHelper.Dash, 3), PlotStyle.Line, "MidLine");
            AddPlot(new Stroke(Brushes.Magenta, DashStyleHelper.Dash, 2), PlotStyle.Line, "UpperExt");
            AddPlot(new Stroke(Brushes.Magenta, DashStyleHelper.Dash, 2), PlotStyle.Line, "LowerExt");

    		//AddPlot(new Stroke(LineColor, DashStyleHelper.Dash , LineWidth), PlotStyle.Line, "UpperExt");
        }
    }

    protected override void OnBarUpdate()
    {
        if (CurrentBar < 10) return; // Ensure we have enough data

        // Check if it's time to update the lines
        if (Time[0].Minute == 50 )
        {
            if (Time[0].Hour != currentHour)
            {
                currentHour = Time[0].Hour;
                startTime = Time[0];
                endTime = Time[0].AddMinutes(20);                
                // Remove old lines
                //RemoveDrawObjects();
            }			
        }
		if (Time[0] >= startTime && Time[0] < endTime)
		{
			// Calculate high and low for the period
                high = MAX(High, 20)[0];
                low = MIN(Low, 20)[0];
                mid = (high + low) / 2;
                highExt =   high + (high - low)/4;
                lowExt = low - (high - low)/4;

		}
		else if (Time[0].Minute >= 10 && Time[0].Minute < 50)
			{
            // Draw new lines
//                DrawHorizontalLine("High" + CurrentBar, high, Brushes.White, DashStyleHelper.Solid,2, startTime, endTime.AddMinutes(40));
//                DrawHorizontalLine("Low"+ CurrentBar, low, Brushes.White, DashStyleHelper.Solid, 2, startTime, endTime.AddMinutes(40));
//                DrawHorizontalLine("Mid"+ CurrentBar, mid, Brushes.Yellow, DashStyleHelper.Dash, 3, startTime, endTime.AddMinutes(40));
//                DrawHorizontalLine("HighExt"+ CurrentBar, highExt, Brushes.Magenta, DashStyleHelper.DashDot, 2, startTime, endTime.AddMinutes(40));
//                DrawHorizontalLine("LowExt"+ CurrentBar, lowExt, Brushes.Magenta, DashStyleHelper.DashDot, 2, startTime, endTime.AddMinutes(40));
				
				Values[0][0] = high;
				Values[1][0] = low;
				Values[2][0] = mid;
				Values[3][0] = highExt;
				Values[4][0] = lowExt;
			}
    }

    private void DrawHorizontalLine(string tag, double price, Brush color, DashStyleHelper dashStyle, int width, DateTime start, DateTime end)
    {
        Draw.Line(this, tag, false, start, price, end, price, color, dashStyle, width);
    }

    private void RemoveDrawObjects()
    {
        RemoveDrawObject("HighLine");
        RemoveDrawObject("LowLine");
        RemoveDrawObject("MidLine");
        RemoveDrawObject("HighExt");
        RemoveDrawObject("LowExt");
    }
	
	#region Properties

        
        [XmlIgnore]
        public Series<double>  HighLine 
		{ 
			get {return Values[0];}
		}

  
      
        
        [XmlIgnore]
        public Series<double>  LowLine 
		{ 
			get {return Values[1];}
		}  
		
		
        [XmlIgnore]
        public Series<double>  MidLine 
		{ 
			get {return Values[2];}
		}
		
		 [Browsable(false)]
        [XmlIgnore]
        public Series<double>  HighExt 
		{ 
			get {return Values[3];}
		}
		
		 [Browsable(false)]
        [XmlIgnore]
        public Series<double>  LowExt 
		{ 
			get {return Values[4];}
		}

        // You could add additional properties here for further customization.
     #endregion
}   
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HourlyFibs[] cacheHourlyFibs;
		public HourlyFibs HourlyFibs()
		{
			return HourlyFibs(Input);
		}

		public HourlyFibs HourlyFibs(ISeries<double> input)
		{
			if (cacheHourlyFibs != null)
				for (int idx = 0; idx < cacheHourlyFibs.Length; idx++)
					if (cacheHourlyFibs[idx] != null &&  cacheHourlyFibs[idx].EqualsInput(input))
						return cacheHourlyFibs[idx];
			return CacheIndicator<HourlyFibs>(new HourlyFibs(), input, ref cacheHourlyFibs);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HourlyFibs HourlyFibs()
		{
			return indicator.HourlyFibs(Input);
		}

		public Indicators.HourlyFibs HourlyFibs(ISeries<double> input )
		{
			return indicator.HourlyFibs(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HourlyFibs HourlyFibs()
		{
			return indicator.HourlyFibs(Input);
		}

		public Indicators.HourlyFibs HourlyFibs(ISeries<double> input )
		{
			return indicator.HourlyFibs(input);
		}
	}
}

#endregion
