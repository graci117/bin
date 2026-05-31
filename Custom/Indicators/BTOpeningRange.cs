#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// Opening range indicator for S/R
	/// Author:  Jason Chan (bearishtrader)
    /// Description:
	/// 
	/// Draw the upper and lower levels of the opening range.  Set the StartTime and EndTime according to your own local
	/// time preferences.  The defaults are 8:30 AM to 8:45 AM CST
	/// 
	/// *** You may freely distribute this script, but if you make changes please add your comments below
	/// 
	/// Revision Author      Date	    Comments  
	/// ________ ______		 ____	    ________
	/// 1.0      Jason Chan  04/24/2009 Initial release
	///
    /// </summary>
    public class BTOpeningRange : Indicator
    {
       
        // Wizard generated variables
        // User defined variables (add any user defined variables below)
		
			private string startTime = @"8:30 AM";
            private string endTime = @"8:45 AM";
		
			private DateTime startDateTime;
			private DateTime endDateTime;
			private double highestHigh = 0;
			private double lowestLow = 0;
			private bool firstTime;
		
			//private Brush rangeColor = Brushes.DarkGoldenrod;
			private int rangeColorOpacity = 2;			
       

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        private void Initialize()
        {
            AddPlot(new Stroke(Brushes.DarkGoldenrod), PlotStyle.Line, "Range Top");
            AddPlot(new Stroke(Brushes.DarkGoldenrod), PlotStyle.Line, "Range Bottom");
			
			//Plots[0].DashStyleHelper = DashStyleHelper.Dash;
			//Plots[1].DashStyleHelper = DashStyleHelper.Dash;
			
            Calculate	= Calculate.OnBarClose;
            IsOverlay				= true;
            /* NT8 REMOVED: PriceTypeSupported	= false */;			
			firstTime = true;
					
			startTime = @"8:30 AM";
            endTime = @"8:45 AM";
					
			// rangeColor = Brushes.DarkGoldenrod;
			rangeColorOpacity = 2;				
			
			
        }

        protected override void OnStateChange()
        {
            switch (State)
            {
                case State.SetDefaults:
					
                    Name = "BTOpeningRange";
                    Description = "Opening range indicator for S/R";
                    Initialize();
					
					
					
					
					
                    break;
             }
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {	
			if ( ToTime(Time[0]) > ToTime(startDateTime) && ToTime(Time[0]) <= ToTime(endDateTime))
			{
				if (firstTime)
				{
					highestHigh = High[0]; lowestLow = Low[0]; firstTime = false;
				}
				if (High[0] >= highestHigh) highestHigh = High[0];				
				if (Low[0] <= lowestLow) lowestLow = Low[0];
			}
			else
			{
				firstTime = true;
			}
			
			if (highestHigh > 0 && lowestLow > 0 && ToTime(Time[0]) > ToTime(startDateTime) ) {
				RangeTop[0] = highestHigh;
				RangeBottom[0] = lowestLow;
				
				Draw.Region(this,"ColorRange", CurrentBar, 0, RangeTop, RangeBottom, Brushes.Transparent, pColorAreaBrush, rangeColorOpacity);
				
			}
        }

    
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> RangeTop
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> RangeBottom
        {
            get { return Values[1]; }
        }


		
		
		
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Start Time", Description = "Time of market open", GroupName = "Parameters", Order = 1)]
        public string AtartTime
        {
            get { return startTime; }
            set { startTime = value;
				  DateTime tmpDateTime = DateTime.Today.Date;
				  string sTmpDateTime = tmpDateTime.ToString("MM/dd/yyyy") + " " + startTime;
				  //Print("sTmpDateTime = " + sTmpDateTime + "\n");
			      startDateTime = Convert.ToDateTime(sTmpDateTime);
			}
        }

        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "End Time", Description = "End time of opening range", GroupName = "Parameters", Order = 2)]
        public string EndTime
        {
            get { return endTime; }
            set { endTime = value;
				  DateTime tmpDateTime = DateTime.Today.Date;
				  string sTmpDateTime = tmpDateTime.ToString("MM/dd/yyyy") + " " + endTime;
				  //Print("sTmpDateTime = " + sTmpDateTime + "\n");
				  endDateTime = Convert.ToDateTime(sTmpDateTime);
			}
        }

		private Brush pColorAreaBrush	= Brushes.DarkGoldenrod;
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Area Color", GroupName = "Plots", Order = 5)]
		public Brush ColorAreaBrush
		{
			get { return pColorAreaBrush; } set { pColorAreaBrush = value; }
		}
		[Browsable(false)]
		public string ColorAreaBrushS
		{
			get { return Serialize.BrushToString(pColorAreaBrush); } set { pColorAreaBrush = Serialize.StringToBrush(value); }
		}			

		//[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Area Opacity (%)", Description = "Area Opacity (%)", GroupName = "Plots", Order = 6)]
		public int RangeColorOpacity
		{
			get { return rangeColorOpacity; }
			set { rangeColorOpacity = Math.Max(value, 0); }
		}
     
		
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BTOpeningRange[] cacheBTOpeningRange;
		public BTOpeningRange BTOpeningRange(string atartTime, string endTime)
		{
			return BTOpeningRange(Input, atartTime, endTime);
		}

		public BTOpeningRange BTOpeningRange(ISeries<double> input, string atartTime, string endTime)
		{
			if (cacheBTOpeningRange != null)
				for (int idx = 0; idx < cacheBTOpeningRange.Length; idx++)
					if (cacheBTOpeningRange[idx] != null && cacheBTOpeningRange[idx].AtartTime == atartTime && cacheBTOpeningRange[idx].EndTime == endTime && cacheBTOpeningRange[idx].EqualsInput(input))
						return cacheBTOpeningRange[idx];
			return CacheIndicator<BTOpeningRange>(new BTOpeningRange(){ AtartTime = atartTime, EndTime = endTime }, input, ref cacheBTOpeningRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BTOpeningRange BTOpeningRange(string atartTime, string endTime)
		{
			return indicator.BTOpeningRange(Input, atartTime, endTime);
		}

		public Indicators.BTOpeningRange BTOpeningRange(ISeries<double> input , string atartTime, string endTime)
		{
			return indicator.BTOpeningRange(input, atartTime, endTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BTOpeningRange BTOpeningRange(string atartTime, string endTime)
		{
			return indicator.BTOpeningRange(Input, atartTime, endTime);
		}

		public Indicators.BTOpeningRange BTOpeningRange(ISeries<double> input , string atartTime, string endTime)
		{
			return indicator.BTOpeningRange(input, atartTime, endTime);
		}
	}
}

#endregion
