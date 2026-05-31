// 
// Copyright (C) 2006, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//

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
    /// Plots the current hour open, high, and low values.
    /// </summary>
    public class HiLowChannel : Indicator
    {
        #region Variables

		private double		currentOpen			=	0;
        private double		currentHigh			=	0;
		private double		currentLow			=	0;
		private bool		plotCurrentValue	=	false;
		private bool		showOpen			=	true;
		private bool		showHigh			=	true;
		private bool		showLow				=	true;
		int                 barBreak = 0;
		int                 barProgress =0;
		int                 barAgo =0;  
		private DateTime    temp;
                bool        HourBreak = false;
		// When time interval wraps back to zero, draw a line.
		#endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        private void Initialize()
        {
            AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Hash, "Current Open");
			AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Square, "Current High");
			AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Square, "Current Low");
			
			Period						= 180;
			
			IsAutoScale 			= false;
            Calculate	= Calculate.OnEachTick;
            IsOverlay				= true;
			
			}

        protected override void OnStateChange()
        {
           if (State == State.SetDefaults)
			{
                    Name = "HiLowChannel";
                    Description = "Plots the current hour open, high, and low values.";
                    Initialize();
             }
			else if (State == State.Configure)
			{  // Add a minute Bars object - BarsInProgress index = 1 (Primary Chart Index always = 0)
        	//	AddDataSeries(BarsPeriodType.Minute,Period);
				AddDataSeries(BarsPeriodType.Second,Period);
			}
        }
		
        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			 if (BarsInProgress == 1 )
			 {
				 if (barProgress != CurrentBar)
				 {
				  HourBreak = true;
				  barProgress = CurrentBar;
		//	      Print("barinprogress# "+ CurrentBar + " barinprogress timer "+	 Bars.GetTime(CurrentBar));
			      temp = Bars.GetTime(CurrentBar);
			      }
			  }
			
			// working with primary bar
           if (BarsInProgress == 0 )
		   {
	
			if (CurrentBar < 5) { return; }
			
				// wrapped to a new hour!	
				if(HourBreak && Bars.GetTime(CurrentBar) > temp)
				{
				  HourBreak = false;				
				//plot HH, LL back from minute break point
				//Print("Inside From bar# "+ barBreak +" To bar# "+ CurrentBar	+ " bar timer "+ Bars.GetTime(CurrentBar));
					barAgo = CurrentBar - barBreak;
					for (int i=0; i <= (CurrentBar - barBreak); i++)
						{
							CurrentHigh[i] = currentHigh;
							CurrentLow[i] = currentLow;
						}
					
					currentOpen 	= 	Open[0];
					currentHigh 	= 	High[0];
					currentLow		=	Low[0];
					barBreak = CurrentBar;

	//		Print("inside Hi Lo = " + Time[0].Date +  " high "+High[0]+ " low "+Low[0]+ "\n");
	//		Print("inside bar # "+ CurrentBar + " bar timer "+	 Bars.GetTime(CurrentBar));
				}
	
				currentHigh 	= 	Math.Max(currentHigh, High[0]);
				currentLow		= 	Math.Min(currentLow, Low[0]);
				
				
	//		Print("outside From bar# "+ barBreak +" To bar# "+ CurrentBar) ;	
	//		Print("Highest Hi Lo = " + Time[0].Date +  " HH "+currentHigh+ " LL "+currentLow + "\n");
	//		Print("current bar # "+ CurrentBar + " bar timer "+	 Bars.GetTime(CurrentBar));
	//		Print("bar back# "+	barAgo);
    //      Print("bar break# "+barBreak);
	
				if (ShowOpen)
				{
					if (!plotCurrentValue || !HourBreak)
						CurrentOpen[0] = currentOpen;
					else
						for (int idx = 0; idx < CurrentOpen.Count; idx++)
							CurrentOpen[idx] = currentOpen;
				}
	
				if (ShowHigh)
				{
						for (int idx = 0; idx < (CurrentBar - barBreak)+1 ; idx++)
						    CurrentHigh[idx] = currentHigh;
				}
	
				if (ShowLow)
				{
						for (int idx = 0; idx < (CurrentBar - barBreak)+1; idx++)
						    CurrentLow[idx] = currentLow;
				}
				
		
			  }
        }

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> CurrentOpen
        {
            get { return Values[0]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> CurrentHigh
        {
            get { return Values[1]; }
        }

        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> CurrentLow
        {
            get { return Values[2]; }
        }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period in Second", Description="Make sure period > bar duration",GroupName = "Inputs", Order = 0)]
		public int Period
		{ get; set; }
		
		[Browsable(true)]

		[NinjaScriptProperty]
		[Display(Name = "Show open", Order = 1)]
        public bool ShowOpen
        {
            get { return showOpen; }
			set { showOpen = value; }
        }
		
		[Browsable(true)]
		
		[NinjaScriptProperty]		
		[Display(Name = "Show high", Order = 1)]
        public bool ShowHigh
        {
            get { return showHigh; }
			set { showHigh = value; }
        }
/*
		[Browsable(true)]

		[NinjaScriptProperty]
		[Display(Name = "Plot current value only", Order = 1)]
		public bool PlotCurrentValue
		{
			get { return plotCurrentValue; }
			set { plotCurrentValue = value; }
		}
*/		
		[Browsable(true)]
		
		[NinjaScriptProperty]		
		[Display(Name = "Show low", Order = 1)]
        public bool ShowLow
        {
            get { return showLow; }
			set { showLow = value; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HiLowChannel[] cacheHiLowChannel;
		public HiLowChannel HiLowChannel(int period, bool showOpen, bool showHigh, bool showLow)
		{
			return HiLowChannel(Input, period, showOpen, showHigh, showLow);
		}

		public HiLowChannel HiLowChannel(ISeries<double> input, int period, bool showOpen, bool showHigh, bool showLow)
		{
			if (cacheHiLowChannel != null)
				for (int idx = 0; idx < cacheHiLowChannel.Length; idx++)
					if (cacheHiLowChannel[idx] != null && cacheHiLowChannel[idx].Period == period && cacheHiLowChannel[idx].ShowOpen == showOpen && cacheHiLowChannel[idx].ShowHigh == showHigh && cacheHiLowChannel[idx].ShowLow == showLow && cacheHiLowChannel[idx].EqualsInput(input))
						return cacheHiLowChannel[idx];
			return CacheIndicator<HiLowChannel>(new HiLowChannel(){ Period = period, ShowOpen = showOpen, ShowHigh = showHigh, ShowLow = showLow }, input, ref cacheHiLowChannel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HiLowChannel HiLowChannel(int period, bool showOpen, bool showHigh, bool showLow)
		{
			return indicator.HiLowChannel(Input, period, showOpen, showHigh, showLow);
		}

		public Indicators.HiLowChannel HiLowChannel(ISeries<double> input , int period, bool showOpen, bool showHigh, bool showLow)
		{
			return indicator.HiLowChannel(input, period, showOpen, showHigh, showLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HiLowChannel HiLowChannel(int period, bool showOpen, bool showHigh, bool showLow)
		{
			return indicator.HiLowChannel(Input, period, showOpen, showHigh, showLow);
		}

		public Indicators.HiLowChannel HiLowChannel(ISeries<double> input , int period, bool showOpen, bool showHigh, bool showLow)
		{
			return indicator.HiLowChannel(input, period, showOpen, showHigh, showLow);
		}
	}
}

#endregion
