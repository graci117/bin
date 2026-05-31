

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
using NinjaTrader.Code;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{ 
   
public class Zombie2Candles : Indicator
    {

        private const string SystemVersion = "v1.028";
        private const string SystemName = "Zombie2Candles";
        private const string FullSystemName = SystemName + " - " + SystemVersion;
        private int lastPrintOutputHashCode = 0;

        private Instrument attachedInstrument = null;

        private bool 		colorBars 		= false;

        private bool 		colorOutline 	= false;

        private EMA emafastValue;
        private EMA emaMiddleValue;
        private EMA emaSlowValue;

        private Brush barColorCondition1		= Brushes.Chartreuse;
        private Brush barColorCondition2		= Brushes.RoyalBlue;
        private Brush barColorCondition3		= Brushes.WhiteSmoke;
        private Brush barColorCondition4		= Brushes.Gray;
        private Brush barColorCondition5		= Brushes.DarkOrange;  //Brushes.DarkOrange;
        private Brush barColorCondition6		= Brushes.Red;
		
        private Brush candleOutlineCondition1	= Brushes.Chartreuse;
        private Brush candleOutlineCondition2	= Brushes.RoyalBlue;
        private Brush candleOutlineCondition3	= Brushes.WhiteSmoke;
        private Brush candleOutlineCondition4	= Brushes.Gray;
        private Brush candleOutlineCondition5	= Brushes.DarkOrange; //Brushes.DarkOrange;
        private Brush candleOutlineCondition6	= Brushes.Red;

        public override string DisplayName
        {
            get { return FullSystemName; }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = SystemName;
                Description = FullSystemName;

                PaintPriceMarkers = false;
                Calculate = Calculate.OnPriceChange;
                IsOverlay				= true;
                EMAFastPeriod = 8;
                EMAMiddlePeriod = 21;
                EMASlowPeriod = 89;
            

                this.ColorBars = true;
                this.ColorOutline = true;
            }
			
			else if (State == State.Configure)
			{
                attachedInstrument = this.Instrument;

            }
            else if (State == State.DataLoaded)
            {
                PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
                PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

                emafastValue = EMA(Close, EMAFastPeriod);
                emaMiddleValue = EMA(Close, EMAMiddlePeriod);
                emaSlowValue = EMA(Close, EMASlowPeriod);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < EMASlowPeriod)
                return;

            
            bool bullishTrend = (emaMiddleValue[0] >= emaSlowValue[0]);
            bool bullishMicroTrend = (emafastValue[0] >= emafastValue[1]);
            bool bullishCandle = Close[0] >= Close[1];

            if (bullishTrend) 
            {
                // Condition set 1
                if (bullishCandle && bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition1;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition1;
                    }
                }

                // Condition set 2
                if (!bullishCandle && bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition2;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition2;
                    }
                }

                // Condition set 3
                if (bullishCandle && !bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition3;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition3;
                    }
                }

                // Condition set 4
                if (!bullishCandle && !bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition4;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition4;
                    }
                }
            }
            else
            {
                // Condition set 5
                if (bullishCandle && !bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition5;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition5;
                    }
                }

                // Condition set 6
                if (!bullishCandle && !bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition6;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition6;
                    }
                }

                // Condition set 3
                if (bullishCandle && bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition3;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition3;
                    }
                }

                // Condition set 4
                if (!bullishCandle && bullishMicroTrend)
                {
                    if (colorBars)
                    {
                        BarBrush = barColorCondition4;
                    }
                    if (colorOutline)
                    {
                        CandleOutlineBrush = candleOutlineCondition4;
                    }
                }
            }
            
                    
                
        }

        private void PrintOutput(string output, PrintTo outputTab = PrintTo.OutputTab1, bool blockDuplicateMessages = false)
        {
            this.PrintTo = outputTab;
            if (blockDuplicateMessages)
            {
                int tempHashCode = output.GetHashCode();
                if (tempHashCode != lastPrintOutputHashCode)
                {
                    Print(DateTime.Now + " " + SystemName + ": " + output);
                }
                lastPrintOutputHashCode = tempHashCode;
            }
            else
                Print(DateTime.Now + " " + SystemName + ": " + output);
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
        public string IndicatorName
        {
            get { return FullSystemName; }
            set { }
        }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 1)]
        public int EMAFastPeriod
        { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 2)]
        public int EMAMiddlePeriod
        { get; set; }
        

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), GroupName = "Parameters", Order = 3)]
        public int EMASlowPeriod
        { get; set; }

        

        [XmlIgnore]	
		[Display(Name = "BarCondition1", Description = "Color of BarCondition1.", GroupName = "Visual", Order = 1)]
        public Brush BarCondition1
        {
            get { return barColorCondition1; }
            set { barColorCondition1 = value; }
        }

        [Browsable(false)]
        public string BarCondition1Serialize
        {
            get { return Serialize.BrushToString(barColorCondition1); }
            set { barColorCondition1 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "BarCondition2", Description = "Color of BarCondition2.", GroupName = "Visual", Order = 2)]
        public Brush BarCondition2
        {
            get { return barColorCondition2; }
            set { barColorCondition2 = value; }
        }

        [Browsable(false)]
        public string BarCondition2Serialize
        {
            get { return Serialize.BrushToString(barColorCondition2); }
            set { barColorCondition2 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "BarCondition3", Description = "Color of BarCondition3.", GroupName = "Visual", Order = 3)]
        public Brush BarCondition3
        {
            get { return barColorCondition3; }
            set { barColorCondition3 = value; }
        }

        [Browsable(false)]
        public string BarCondition3Serialize
        {
            get { return Serialize.BrushToString(barColorCondition3); }
            set { barColorCondition3 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "BarCondition4", Description = "Color of BarCondition4.", GroupName = "Visual", Order = 4)]
        public Brush BarCondition4
        {
            get { return barColorCondition4; }
            set { barColorCondition4 = value; }
        }

        [Browsable(false)]
        public string BarCondition4Serialize
        {
            get { return Serialize.BrushToString(barColorCondition4); }
            set { barColorCondition4 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]		
		[Display(Name = "BarCondition5", Description = "Color of BarCondition5.", GroupName = "Visual", Order = 5)]
        public Brush BarCondition5
        {
            get { return barColorCondition5; }
            set { barColorCondition5 = value; }
        }

        [Browsable(false)]
        public string BarCondition5Serialize
        {
            get { return Serialize.BrushToString(barColorCondition5); }
            set { barColorCondition5 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "BarCondition6", Description = "Color of BarCondition6.", GroupName = "Visual", Order = 6)]
        public Brush BarCondition6
        {
            get { return barColorCondition6; }
            set { barColorCondition6 = value; }
        }

        [Browsable(false)]
        public string BarCondition6Serialize
        {
            get { return Serialize.BrushToString(barColorCondition6); }
            set { barColorCondition6 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "CandleOutlineCondition1", Description = "Color of CandleOutlineCondition1.", GroupName = "Visual", Order = 1)]
        public Brush CandleOutlineCondition1
        {
            get { return candleOutlineCondition1; }
            set { candleOutlineCondition1 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition1Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition1); }
            set { candleOutlineCondition1 = Serialize.StringToBrush(value); }
        }
				
		[XmlIgnore]				
		[Display(Name = "CandleOutlineCondition2", Description = "Color of CandleOutlineCondition2.", GroupName = "Visual", Order = 2)]
        public Brush CandleOutlineCondition2
        {
            get { return candleOutlineCondition2; }
            set { candleOutlineCondition2 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition2Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition2); }
            set { candleOutlineCondition2 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]		
		[Display(Name = "CandleOutlineCondition3", Description = "Color of CandleOutlineCondition3.", GroupName = "Visual", Order = 3)]
        public Brush CandleOutlineCondition3
        {
            get { return candleOutlineCondition3; }
            set { candleOutlineCondition3 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition3Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition3); }
            set { candleOutlineCondition3 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]	
		[Display(Name = "CandleOutlineCondition4", Description = "Color of CandleOutlineCondition4.", GroupName = "Visual", Order = 4)]
        public Brush CandleOutlineCondition4
        {
            get { return candleOutlineCondition4; }
            set { candleOutlineCondition4 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition4Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition4); }
            set { candleOutlineCondition4 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]		
		[Display(Name = "CandleOutlineCondition5", Description = "Color of CandleOutlineCondition5.", GroupName = "Visual", Order = 5)]
        public Brush CandleOutlineCondition5
        {
            get { return candleOutlineCondition5; }
            set { candleOutlineCondition5 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition5Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition5); }
            set { candleOutlineCondition5 = Serialize.StringToBrush(value); }
        }
		
		[XmlIgnore]
	
		[Display(Name = "CandleOutlineCondition6", Description = "Color of CandleOutlineCondition6.", GroupName = "Visual", Order = 6)]
        public Brush CandleOutlineCondition6
        {
            get { return candleOutlineCondition6; }
            set { candleOutlineCondition6 = value; }
        }

        [Browsable(false)]
        public string CandleOutlineCondition6Serialize
        {
            get { return Serialize.BrushToString(candleOutlineCondition6); }
            set { candleOutlineCondition6 = Serialize.StringToBrush(value); }
        }
	
		
		[Display(Name = "2. Color Bars", Description = "Color Bars", GroupName = "Colors", Order = 1)]		
		public bool ColorBars
		{
			get { return colorBars; }
			set { colorBars = value; }
		}
	
		[Display(Name = "3. Color Bar Outline", Description = "Color Bars Ouline", GroupName = "Colors", Order = 2)]		
		public bool ColorOutline
		{
			get { return colorOutline; }
			set { colorOutline = value; }
		}		

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2Candles[] cacheZombie2Candles;
		public Zombie2Candles Zombie2Candles(string indicatorName, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod)
		{
			return Zombie2Candles(Input, indicatorName, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod);
		}

		public Zombie2Candles Zombie2Candles(ISeries<double> input, string indicatorName, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod)
		{
			if (cacheZombie2Candles != null)
				for (int idx = 0; idx < cacheZombie2Candles.Length; idx++)
					if (cacheZombie2Candles[idx] != null && cacheZombie2Candles[idx].IndicatorName == indicatorName && cacheZombie2Candles[idx].EMAFastPeriod == eMAFastPeriod && cacheZombie2Candles[idx].EMAMiddlePeriod == eMAMiddlePeriod && cacheZombie2Candles[idx].EMASlowPeriod == eMASlowPeriod && cacheZombie2Candles[idx].EqualsInput(input))
						return cacheZombie2Candles[idx];
			return CacheIndicator<Zombie2Candles>(new Zombie2Candles(){ IndicatorName = indicatorName, EMAFastPeriod = eMAFastPeriod, EMAMiddlePeriod = eMAMiddlePeriod, EMASlowPeriod = eMASlowPeriod }, input, ref cacheZombie2Candles);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2Candles Zombie2Candles(string indicatorName, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod)
		{
			return indicator.Zombie2Candles(Input, indicatorName, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod);
		}

		public Indicators.Zombie2Candles Zombie2Candles(ISeries<double> input , string indicatorName, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod)
		{
			return indicator.Zombie2Candles(input, indicatorName, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2Candles Zombie2Candles(string indicatorName, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod)
		{
			return indicator.Zombie2Candles(Input, indicatorName, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod);
		}

		public Indicators.Zombie2Candles Zombie2Candles(ISeries<double> input , string indicatorName, int eMAFastPeriod, int eMAMiddlePeriod, int eMASlowPeriod)
		{
			return indicator.Zombie2Candles(input, indicatorName, eMAFastPeriod, eMAMiddlePeriod, eMASlowPeriod);
		}
	}
}

#endregion
