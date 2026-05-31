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

namespace NinjaTrader.NinjaScript.Indicators.Infinity
{
	public class AccumulationSwingIndex : Indicator
	{
		private Series<double> AbsHighClose;
		private Series<double> AbsLowClose;
		private Series<double> AbsCloseOpen;
		
		private Series<double> K;
		private Series<double> R;
		private Series<double> nRes;
		private SMA sma;
		private Series<double> asi;
	
		
		
		
		protected override void OnStateChange()
		{
			if(State == State.SetDefaults)
			{
				Description					= @"";
				Name						= "AccumulationSwingIndex";
				Calculate					= Calculate.OnPriceChange;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				PaintPriceMarkers			= false;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= false;
				
				SMALength					= 2;
				Limit					= 2;
				ASI_crossesbelow		= false;
				
				AddPlot(new Stroke(Brushes.DimGray, 2), PlotStyle.Line, "ASI");
				AddPlot(new Stroke(Brushes.Cyan, 2), PlotStyle.Line, "SMAASI");
				
			
				
		
			}
			else if(State == State.Configure)
			{
				AbsHighClose  = new Series<double>(this, MaximumBarsLookBack.Infinite);
				AbsLowClose = new Series<double>(this, MaximumBarsLookBack.Infinite);
				AbsCloseOpen = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				K = new Series<double>(this, MaximumBarsLookBack.Infinite);
				R = new Series<double>(this, MaximumBarsLookBack.Infinite);
				nRes = new Series<double>(this, MaximumBarsLookBack.Infinite);
				asi = new Series<double>(this, MaximumBarsLookBack.Infinite);	
			
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 1)
			{
				return;
			}
			
			AbsHighClose[0] = Math.Abs(High[0] - Close[1]);
            AbsLowClose[0] = Math.Abs(Low[0] - Close[1]);
            AbsCloseOpen[0] = Math.Abs(Close[1] - Open[1]);
            K[0] = (AbsHighClose[0] >= AbsLowClose[0]) ? AbsHighClose[0] : AbsLowClose[0];
            R[0] = (AbsHighClose[0] >= AbsLowClose[0]) ?
                        ((AbsHighClose[0] >= (High[0] - Low[0])) ? AbsHighClose[0] - 0.5 * AbsLowClose[0] + 0.25 * AbsCloseOpen[0] : (High[0] - Low[0]) + 0.25 * AbsCloseOpen[0]) :
                        ((AbsLowClose[0] >= (High[0] - Low[0])) ? AbsLowClose[0] - 0.5 * AbsHighClose[0] + 0.25 * AbsCloseOpen[0] : (High[0] - Low[0]) + 0.25 * AbsCloseOpen[0]);
            nRes[0] = (R[0] != 0) ?
                        (50 * (((Close[0] - Close[1]) + 0.50 * (Close[0] - Open[0]) + 0.25 * (Close[1] - Open[1])) / R[0]) * K[0] / Limit) + ((!double.IsNaN(nRes[1])) ? nRes[1] : 0) :
                        0 + ((!double.IsNaN(nRes[1])) ? nRes[1] : 0);

            asi = nRes;

            sma = SMA(asi, SMALength);
			
			ASI[0]  = asi[0] ;
			SMAASI[0]  = sma[0] ;

            ASI_crossesbelow = CrossAbove(asi, sma, 1);

            PlotBrushes[0][0] = Brushes.DodgerBlue;
            PlotBrushes[1][0] = Brushes.Red;
            //Plots[0].Values[0] = asi;
            //Plots[1].Values[0] = sma;

            
		
		}
		

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ASI
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SMAASI
		{
			get { return Values[1]; }
		}
		
		
		
	
		
		/// ---
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SMALength", Order=1, GroupName="Parameters")]
		public int SMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Limit", Order=2, GroupName="Parameters")]
		public double Limit
		{ get; set; }
			

		[NinjaScriptProperty]
		public bool ASI_crossesbelow
		{ get; set; }
		
	
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Infinity.AccumulationSwingIndex[] cacheAccumulationSwingIndex;
		public Infinity.AccumulationSwingIndex AccumulationSwingIndex(int sMALength, double limit, bool aSI_crossesbelow)
		{
			return AccumulationSwingIndex(Input, sMALength, limit, aSI_crossesbelow);
		}

		public Infinity.AccumulationSwingIndex AccumulationSwingIndex(ISeries<double> input, int sMALength, double limit, bool aSI_crossesbelow)
		{
			if (cacheAccumulationSwingIndex != null)
				for (int idx = 0; idx < cacheAccumulationSwingIndex.Length; idx++)
					if (cacheAccumulationSwingIndex[idx] != null && cacheAccumulationSwingIndex[idx].SMALength == sMALength && cacheAccumulationSwingIndex[idx].Limit == limit && cacheAccumulationSwingIndex[idx].ASI_crossesbelow == aSI_crossesbelow && cacheAccumulationSwingIndex[idx].EqualsInput(input))
						return cacheAccumulationSwingIndex[idx];
			return CacheIndicator<Infinity.AccumulationSwingIndex>(new Infinity.AccumulationSwingIndex(){ SMALength = sMALength, Limit = limit, ASI_crossesbelow = aSI_crossesbelow }, input, ref cacheAccumulationSwingIndex);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Infinity.AccumulationSwingIndex AccumulationSwingIndex(int sMALength, double limit, bool aSI_crossesbelow)
		{
			return indicator.AccumulationSwingIndex(Input, sMALength, limit, aSI_crossesbelow);
		}

		public Indicators.Infinity.AccumulationSwingIndex AccumulationSwingIndex(ISeries<double> input , int sMALength, double limit, bool aSI_crossesbelow)
		{
			return indicator.AccumulationSwingIndex(input, sMALength, limit, aSI_crossesbelow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Infinity.AccumulationSwingIndex AccumulationSwingIndex(int sMALength, double limit, bool aSI_crossesbelow)
		{
			return indicator.AccumulationSwingIndex(Input, sMALength, limit, aSI_crossesbelow);
		}

		public Indicators.Infinity.AccumulationSwingIndex AccumulationSwingIndex(ISeries<double> input , int sMALength, double limit, bool aSI_crossesbelow)
		{
			return indicator.AccumulationSwingIndex(input, sMALength, limit, aSI_crossesbelow);
		}
	}
}

#endregion
