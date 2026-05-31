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
	/// Coded by bcomas July 2020. Email: bernardocomas@hotmail.com for improvements
    /// Displays the major transactions that have taken place during
    /// short period of time and at the same price (Limit buyer / seller)
    /// </summary>
	
    public class bcomasLargeTradesV3 : Indicator
    {
        #region Variables 
		private int vol                               = 50;
		private int fix                               = 1; 
		Brush color                                   = Brushes.Transparent;
		Brush colorText                               = Brushes.White;
		Brush shadowcolor                             = Brushes.Red;
		int opacity                                   = 1;
		private bool useAlerts                        = false;
		private bool useEllipse                       = true;
		private bool ShowTime                         = false;
		private string stringtoAlert                  = "Alert4.wav";
		private int direction                         = 0; 
        #endregion
        
        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
		/// 
        private void Initialize()
        {                
            Calculate	= Calculate.OnBarClose;
			IsOverlay	= true;		
        }

        protected override void OnStateChange()
        {
            switch (State)
            {
                    case State.SetDefaults:
                    Name        = "bcomasLargeTradesV3";
                    Description = "Displays the major transactions that have taken place during short period of time and at the same price (Limit buyer / seller)";
                    Initialize();
                    break;
            }
			
		if (State == State.Configure)
    	  {
		   AddDataSeries(BarsPeriodType.Second,1);
		  }
       }
       /// <summary>
       /// Called on each bar update event (incoming tick)
       /// </summary>
       protected override void OnBarUpdate()
       {
			if (CurrentBar < 100) 
			{
				return;
			}
			// session start 
			if (Bars.IsFirstBarOfSession) 
            {
                direction = 0;    
            }
	        if (BarsInProgress == 1)
			{			
				TimeSpan ts1 = Time[0] - new DateTime(1970,1,1,0,0,0);
			    TimeSpan ts2 = Time[1] - new DateTime(1970,1,1,0,0,0);

				int timeGap=(int)ts1.TotalSeconds-(int)ts2.TotalSeconds;

			    if (Volume[0]>=vol && Math.Abs(timeGap)<=fix)
			    {	
				//if(useEllipse)	
			    //Draw.Ellipse(this,"ellipse"+CurrentBar, true, 3, High[0], -3, Low[0], color, shadowcolor, opacity);
			    
				if(ShowTime == true)	
				{
				Draw.Text(this,"TFx"+CurrentBar, true, Convert.ToString(Time[0]),5, Median[0], 0,colorText, 
				new SimpleFont("Small Fonts", 9), TextAlignment.Right,	Brushes.Transparent,Brushes.Transparent, 0);
				}
				if (ShowTime == false)
				{
				Draw.Text(this,"TFx"+CurrentBar, true, Convert.ToString(Volume[0]), 0, Median[0], 0,colorText, 
				new SimpleFont("Small Fonts", 9), TextAlignment.Right,	Brushes.Transparent,Brushes.Transparent, 0);
				}
				if ((Volume[0]>=vol && Math.Abs(timeGap)<=fix) && (Close[0] >= Open[0]))
			    {	
					Print("Vol - " + Volume[0]);
			    direction = 1;
				Draw.Ellipse(this,"ellipse"+CurrentBar, true, 3, High[0], -3, Low[0], Brushes.Lime, shadowcolor, opacity);
		        }
			    if ((Volume[0]>=vol && Math.Abs(timeGap)<=fix) && (Close[0] <= Open[0]))
			    {	
					Print("VolS - " + Volume[0]);
			    direction = -1;
				Draw.Ellipse(this,"ellipse"+CurrentBar, true, 3, High[0], -3, Low[0], Brushes.Red, shadowcolor, opacity);
		        }
			  }
		    }
		  }
		
		#region Properties	
		
		[Browsable(false)]
		[XmlIgnore]	
		public int Direction  
		{
			get { Update(); return direction; }
		}
		[NinjaScriptProperty]		
		[Display(Name = "1.Transaction volume", Description = "Transaction volume", GroupName = "Parameters", Order = 1)]		
		public int Volume_
        {
            get { return vol; }
            set { vol = Math.Max(1, value); }
        }
        [NinjaScriptProperty]        
		[Display(Name = "2.Time", Description = "The time during which the transaction occurred", GroupName = "Parameters", Order = 2)]       
		public int Time_
        {
            get { return fix; }
            set { fix = Math.Max(1, value); }
        }
		[NinjaScriptProperty]		
		[Display(Name = "3.Ellipse color", Description = "Transaction Label Color", GroupName = "Parameters", Order = 3)]
		[XmlIgnore]	
		public Brush colour
		{
			get{return color;}
			set{color = value;}
		}
		[NinjaScriptProperty]		
		[Display(Name = "4.Text color", Description = "Text color", GroupName = "Parameters", Order = 4)]	
		[XmlIgnore]	
		public Brush Colour_Text
		{
			get{return colorText;}
			set{colorText = value;}
		}
        [NinjaScriptProperty]       
		[Display(Name = "1.Deal display", Description = "Deal display", GroupName = "Parameters-Other", Order = 1)]        
		public bool Ellipse
        {
            get { return useEllipse; }
            set {useEllipse = value; }
        }	
        [NinjaScriptProperty]        
		[Display(Name = "2.Time or volume", Description = "Display time true, volume false", GroupName = "Parameters-Other", Order = 2)]       
		public bool Volume_values
        {
            get { return ShowTime; }
            set {ShowTime = value; }
        }	
        [NinjaScriptProperty]        
		[Display(Name = "3.Sound signal", Description = "Sound signal", GroupName = "Parameters-Other", Order = 3)]        
		public bool Sound_signal
        {
            get { return useAlerts; }
            set {useAlerts = value; }
        }			
        [NinjaScriptProperty]        
		[Display(Name = "4.Sound file name", Description = "Sound file name", GroupName = "Parameters-Other", Order = 4)]        
		public string StringtoAlert
        {
            get { return stringtoAlert; }
            set {stringtoAlert = value; }
        }		
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private bcomasLargeTradesV3[] cachebcomasLargeTradesV3;
		public bcomasLargeTradesV3 bcomasLargeTradesV3(int volume_, int time_, Brush colour, Brush colour_Text, bool ellipse, bool volume_values, bool sound_signal, string stringtoAlert)
		{
			return bcomasLargeTradesV3(Input, volume_, time_, colour, colour_Text, ellipse, volume_values, sound_signal, stringtoAlert);
		}

		public bcomasLargeTradesV3 bcomasLargeTradesV3(ISeries<double> input, int volume_, int time_, Brush colour, Brush colour_Text, bool ellipse, bool volume_values, bool sound_signal, string stringtoAlert)
		{
			if (cachebcomasLargeTradesV3 != null)
				for (int idx = 0; idx < cachebcomasLargeTradesV3.Length; idx++)
					if (cachebcomasLargeTradesV3[idx] != null && cachebcomasLargeTradesV3[idx].Volume_ == volume_ && cachebcomasLargeTradesV3[idx].Time_ == time_ && cachebcomasLargeTradesV3[idx].colour == colour && cachebcomasLargeTradesV3[idx].Colour_Text == colour_Text && cachebcomasLargeTradesV3[idx].Ellipse == ellipse && cachebcomasLargeTradesV3[idx].Volume_values == volume_values && cachebcomasLargeTradesV3[idx].Sound_signal == sound_signal && cachebcomasLargeTradesV3[idx].StringtoAlert == stringtoAlert && cachebcomasLargeTradesV3[idx].EqualsInput(input))
						return cachebcomasLargeTradesV3[idx];
			return CacheIndicator<bcomasLargeTradesV3>(new bcomasLargeTradesV3(){ Volume_ = volume_, Time_ = time_, colour = colour, Colour_Text = colour_Text, Ellipse = ellipse, Volume_values = volume_values, Sound_signal = sound_signal, StringtoAlert = stringtoAlert }, input, ref cachebcomasLargeTradesV3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.bcomasLargeTradesV3 bcomasLargeTradesV3(int volume_, int time_, Brush colour, Brush colour_Text, bool ellipse, bool volume_values, bool sound_signal, string stringtoAlert)
		{
			return indicator.bcomasLargeTradesV3(Input, volume_, time_, colour, colour_Text, ellipse, volume_values, sound_signal, stringtoAlert);
		}

		public Indicators.bcomasLargeTradesV3 bcomasLargeTradesV3(ISeries<double> input , int volume_, int time_, Brush colour, Brush colour_Text, bool ellipse, bool volume_values, bool sound_signal, string stringtoAlert)
		{
			return indicator.bcomasLargeTradesV3(input, volume_, time_, colour, colour_Text, ellipse, volume_values, sound_signal, stringtoAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.bcomasLargeTradesV3 bcomasLargeTradesV3(int volume_, int time_, Brush colour, Brush colour_Text, bool ellipse, bool volume_values, bool sound_signal, string stringtoAlert)
		{
			return indicator.bcomasLargeTradesV3(Input, volume_, time_, colour, colour_Text, ellipse, volume_values, sound_signal, stringtoAlert);
		}

		public Indicators.bcomasLargeTradesV3 bcomasLargeTradesV3(ISeries<double> input , int volume_, int time_, Brush colour, Brush colour_Text, bool ellipse, bool volume_values, bool sound_signal, string stringtoAlert)
		{
			return indicator.bcomasLargeTradesV3(input, volume_, time_, colour, colour_Text, ellipse, volume_values, sound_signal, stringtoAlert);
		}
	}
}

#endregion
