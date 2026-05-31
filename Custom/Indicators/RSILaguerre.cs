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
	public class RSILaguerre : Indicator
	{
	     private Series<double> L0;
	    private Series<double> L1;
	    private Series<double> L2;
	    private Series<double> L3;



    

    protected override void OnStateChange()
    {
        if (State == State.SetDefaults)
        {
            Description = @"Laguerre RSI";
            Name = "RSILaguerre";
            Calculate = Calculate.OnPriceChange;
            IsOverlay = false;
            DisplayInDataBox = true;
            DrawOnPricePanel = false;
            DrawHorizontalGridLines = true;
            DrawVerticalGridLines = true;
            PaintPriceMarkers = true;
            ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
            IsSuspendedWhileInactive = true;

            Alpha = 0.2;

//            AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "LaRSI");
//            AddPlot(new Stroke(Brushes.Maroon, 1), PlotStyle.Line, "Upper Line");
//            AddPlot(new Stroke(Brushes.Maroon, 1), PlotStyle.Line, "Lower Line");
			
			AddPlot(Brushes.Blue, "LaRSI");
			AddPlot(Brushes.Maroon, 		"UpperLine");
			AddPlot(Brushes.Maroon, 	"LowerLine");
        }
        else if (State == State.DataLoaded)
        {
            L0 = new Series<double>(this);
            L1 = new Series<double>(this);
            L2 = new Series<double>(this);
            L3 = new Series<double>(this);
            
//            LaRSI = Values[0];
//            UpperLine = Values[1];
//            LowerLine = Values[2];
        }
		else if (State == State.Configure)
			{
				Plots[0].Width 				= 2;
				Plots[0].PlotStyle			= PlotStyle.Line;
				//Plots[0].DashStyleHelper	= DashStyle.;
				Plots[0].Brush				= Brushes.Blue;
				
				Plots[1].Width 				= 1;
				Plots[1].PlotStyle			= PlotStyle.Line;
				//Plots[1].DashStyleHelper	= MA1DashStyle;
				Plots[1].Brush 				= Brushes.Maroon;
				
				Plots[2].Width 				= 1;
				Plots[2].PlotStyle			= PlotStyle.Line;
				//Plots[1].DashStyleHelper	= MA1DashStyle;
				Plots[2].Brush 				= Brushes.Maroon;
			}
	}
	
	    protected override void OnBarUpdate()
	    {
	        double src = Close[0];
	        bool colorChange = false;
	
	        double gamma = 1 - Alpha;
	
	        L0[0] = (1 - gamma) * src + gamma * L0[1];
	        L1[0] = -gamma * L0[0] + L0[1] + gamma * L1[1];
	        L2[0] = -gamma * L1[0] + L1[1] + gamma * L2[1];
	        L3[0] = -gamma * L2[0] + L2[1] + gamma * L3[1];
	
	        double cu = (L0[0] > L1[0] ? L0[0] - L1[0] : 0) +
	                    (L1[0] > L2[0] ? L1[0] - L2[0] : 0) +
	                    (L2[0] > L3[0] ? L2[0] - L3[0] : 0);
	
	        double cd = (L0[0] < L1[0] ? L1[0] - L0[0] : 0) +
	                    (L1[0] < L2[0] ? L2[0] - L1[0] : 0) +
	                    (L2[0] < L3[0] ? L3[0] - L2[0] : 0);
	
	        double temp = cu + cd == 0 ? -1 : cu + cd;
	        double lrsi = temp == -1 ? 0 : cu / temp;
	
	        Brush color = colorChange ? (lrsi > Values[0][1] ? Brushes.Green : Brushes.Red) : Brushes.Blue;
	
	        LaRSI[0] = 100 * lrsi;
	        UpperLine[0] = 80;
	        LowerLine[0] = 20;
			
			Draw.Region(this, "UpperLine", CurrentBar, 0, LowerLine, 0, Brushes.Red, 30);
			Draw.Region(this, "LowerLine", CurrentBar, 0, UpperLine, 100, Brushes.Green, 30);
			
			
			
			
	
	       
	
	        if (State == State.Historical) return;
	
	        
	    }
		
		#region Properties
	
		
		[NinjaScriptProperty]
	    [Range(0.01, 1.0)]
		[Display(Name="Alpha", Order=1, GroupName="Parameters")]
	    public double Alpha
	    { get; set; }

		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LaRSI
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperLine
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerLine
		{
			get { return Values[2]; }
		}
		
		

		
	
		
		
		#endregion
	}

}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RSILaguerre[] cacheRSILaguerre;
		public RSILaguerre RSILaguerre(double alpha)
		{
			return RSILaguerre(Input, alpha);
		}

		public RSILaguerre RSILaguerre(ISeries<double> input, double alpha)
		{
			if (cacheRSILaguerre != null)
				for (int idx = 0; idx < cacheRSILaguerre.Length; idx++)
					if (cacheRSILaguerre[idx] != null && cacheRSILaguerre[idx].Alpha == alpha && cacheRSILaguerre[idx].EqualsInput(input))
						return cacheRSILaguerre[idx];
			return CacheIndicator<RSILaguerre>(new RSILaguerre(){ Alpha = alpha }, input, ref cacheRSILaguerre);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSILaguerre RSILaguerre(double alpha)
		{
			return indicator.RSILaguerre(Input, alpha);
		}

		public Indicators.RSILaguerre RSILaguerre(ISeries<double> input , double alpha)
		{
			return indicator.RSILaguerre(input, alpha);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSILaguerre RSILaguerre(double alpha)
		{
			return indicator.RSILaguerre(Input, alpha);
		}

		public Indicators.RSILaguerre RSILaguerre(ISeries<double> input , double alpha)
		{
			return indicator.RSILaguerre(input, alpha);
		}
	}
}

#endregion
