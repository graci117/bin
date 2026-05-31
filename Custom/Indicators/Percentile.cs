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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
// Percentile - Fast percentile calculation
// by Alex Matulich, October 2014
//
// By default, this indicator plots the 50th percentile (median) of the input data.
// Percentiles are insensitive to outliers, which is good, although lag is significant.
// It is useful for plotting against statistically random oscillators like volume,
// bar ranges, or absolute price movements.
//
// This is a fast, exact calculation of median or other percentile, taking advantage
// of the C# List methods and using a one-time binary search plus insertion on each
// bar to avoid re-sorting the entire list of data. The number of comparisons
// required per bar is O(log n) + O(n/2), on average.
//
// This indicator maintains an internal sorted list of Input values and their
// corresponding bar numbers. As each new data value comes in, the oldest value (with
// the smallest bar number) is dropped from the list, and the new value is inserted
// into the list to maintain the sorted-by-value order.
//
// The median value (50th percentile) is simply the middle value in the list, or an
// interpolation between two middle values if the list is even-length. Similarly, any
// other percentile is easily found by interpolating the correct position in the list.
//
// A public method GetPercentile(int p) is available to call from other indicators.
	
// Converted to NT8 12/29/15 

	public class Percentile : Indicator
	{
		public struct inputdata 
		{ // <input value, bar number> pairs
			public double v; // value
			public int bar;  // bar number
			public inputdata(double value, int barnum) 
			{
				v 	= value;
				bar = barnum;
			}
		}
		public class InputdataComparer: IComparer<inputdata> 
		{ // comparer for binary search
			public int Compare(inputdata x, inputdata y) 
			{
				return x.v.CompareTo(y.v);
			}
		}	
		
		private int period ;
		private int percentil;
		List<inputdata> pvalues;        // this will be the sorted list of inputdata objects
		InputdataComparer vc;           // declaration of binary search comparer	
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Returns P percentile value of last N bars";
				Name								= "Percentile";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				MyInput						= PriceType.Close;
				Period						= 50;
				AddPlot(new Stroke (Brushes.Fuchsia, 2),PlotStyle.Line, "Pctile");
			}
			else if (State == State.Configure)
			{
				pvalues = new List<inputdata>();
				vc 		= new InputdataComparer();
				pvalues.Clear(); 							// empty the list when re-starting calculations
				Plots[0].Name 	= percentil.ToString()+"%"; // clarify name in data box
			}
		}
		
		

		protected override void OnBarUpdate()
		{
			// remove oldest data from sorted list of values
			period = Period;
			percentil = Percentil;
			int rmbar = CurrentBar - period;
			if (rmbar >= 0) 
			{ // nothing will be removed unless the list is full
				foreach (inputdata vdt in pvalues) 
				{ // O(n/2) search
					if (vdt.bar == rmbar) 
					{
						pvalues.Remove(vdt);
						break;
					}
				}
			}

			// insert new input data into sorted list
			inputdata vd	= new inputdata(GetIndexedPriceType(MyInput,0), CurrentBar); // make new data object
			int bsrch 		= pvalues.BinarySearch(vd, vc);           // find insert position
			int insertAt	= (bsrch < 0) ? ~bsrch : bsrch;        // binary complement if needed
			pvalues.Insert(insertAt, vd);                       // insert the new object

			// update plot
			Pctile[0] 	= GetPercentile(MyInput,period,percentil);
				
		}
		
		/// <summary>
		/// Public method, accessible from other indicators, to return specified percentile.
		/// Returned value will be correctly interpolated if needed.
		/// </summary>
		/// <param name="p">Integer between 0 and 100; desired percentile value to return</param>
		/// <returns></returns>
		public double GetPercentile(PriceType myInput, int myPeriod, int p) 
		{
			MyInput = myInput;
			period = myPeriod;
			int n = pvalues.Count - 1;          // max array index
			if (n < 0) return 0.0;              // for safety; this should never happen
			if (n == 0) return pvalues[0].v;    // array has only 1 value, return it
			double r = 0.01 * p * n;            // fractional array index needed
			int indx1 = (int)Math.Floor(r);     // integer value of fractional index
			int indx2 = Math.Min(indx1 + 1, n); // next higher index
			r -= (double)indx1;                 // fractional remainder, for interpolation
			return pvalues[indx1].v * (1.0-r) + pvalues[indx2].v * r;
		}		
		
		private double GetIndexedPriceType(PriceType myInput, int index)
		{
			myInput = MyInput;
			
			switch (MyInput) 
            {
                case PriceType.High:
                    return High[index];
                    break;
                case PriceType.Low:
                    return Low[index];
                    break;
                case PriceType.Median:
                    return Median[index];
                    break;
                case PriceType.Open:
                   return Open[index];
                    break;
                case PriceType.Typical:
                   return Typical[index];
                    break;
                case PriceType.Weighted:
                   return Weighted[index];
                    break;
                default:
					return Close[index];
                    break;
                case PriceType.Close:
                   return Close[index];
                    break;
            }
			
			return 1;
		}

		#region Properties
		[Range(4, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Look Back Period", Description="Lookback window size", Order=2, GroupName="Parameters")]
		public int Period
		{ 
			get {return period;}
		 	set {period = value; }
		}

		[Range(0, 100)]
		[NinjaScriptProperty]
		[Display(Name="Percentile", Description="Percentile of data to plot (0-100)", Order=1, GroupName="Parameters")]
		public int Percentil
		{ 
			get {return percentil;}
		 	set {percentil = value; }
		}
		
		[NinjaScriptProperty]
		[Display(Name="  MA PriceType", Description="Select price Type (Close, high, Low, etc.)", Order=99)]
		public PriceType MyInput
        { get; set; }
		
		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Pctile
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
		private Percentile[] cachePercentile;
		public Percentile Percentile(int period, int percentil, PriceType myInput)
		{
			return Percentile(Input, period, percentil, myInput);
		}

		public Percentile Percentile(ISeries<double> input, int period, int percentil, PriceType myInput)
		{
			if (cachePercentile != null)
				for (int idx = 0; idx < cachePercentile.Length; idx++)
					if (cachePercentile[idx] != null && cachePercentile[idx].Period == period && cachePercentile[idx].Percentil == percentil && cachePercentile[idx].MyInput == myInput && cachePercentile[idx].EqualsInput(input))
						return cachePercentile[idx];
			return CacheIndicator<Percentile>(new Percentile(){ Period = period, Percentil = percentil, MyInput = myInput }, input, ref cachePercentile);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Percentile Percentile(int period, int percentil, PriceType myInput)
		{
			return indicator.Percentile(Input, period, percentil, myInput);
		}

		public Indicators.Percentile Percentile(ISeries<double> input , int period, int percentil, PriceType myInput)
		{
			return indicator.Percentile(input, period, percentil, myInput);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Percentile Percentile(int period, int percentil, PriceType myInput)
		{
			return indicator.Percentile(Input, period, percentil, myInput);
		}

		public Indicators.Percentile Percentile(ISeries<double> input , int period, int percentil, PriceType myInput)
		{
			return indicator.Percentile(input, period, percentil, myInput);
		}
	}
}

#endregion
