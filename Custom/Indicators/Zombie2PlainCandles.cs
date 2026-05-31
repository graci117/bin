

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
   
public class Zombie2PlainCandles : Indicator
    {

        private const string SystemVersion = "v1.028";
        private const string SystemName = "Zombie2PlainCandles";
        private const string FullSystemName = SystemName + " - " + SystemVersion;
        private int lastPrintOutputHashCode = 0;

        private Instrument attachedInstrument = null;

        private bool 		colorBars 		= false;

        private bool 		colorOutline 	= false;

        private EMA emafastValue;
        private EMA emaMiddleValue;
        private EMA emaSlowValue;

        private Brush barColorCondition1		= Brushes.Chartreuse;
        private Brush barColorCondition2		= Brushes.Red;
        
		
        private Brush candleOutlineCondition1	= Brushes.Chartreuse;
        private Brush candleOutlineCondition2	= Brushes.Red;
       

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

            }
			
			else if (State == State.Configure)
			{
                attachedInstrument = this.Instrument;
            }
            else if (State == State.DataLoaded)
            {
                PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
                PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);


            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 2)
                return;

            
            bool bullishCandle = Close[0] >= Close[1];

            
            if (bullishCandle)
            {
                BarBrush = barColorCondition1;
                CandleOutlineBrush = candleOutlineCondition1;
            }
            else
            {
                BarBrush = barColorCondition2;
                CandleOutlineBrush = candleOutlineCondition2;
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


        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2PlainCandles[] cacheZombie2PlainCandles;
		public Zombie2PlainCandles Zombie2PlainCandles(string indicatorName)
		{
			return Zombie2PlainCandles(Input, indicatorName);
		}

		public Zombie2PlainCandles Zombie2PlainCandles(ISeries<double> input, string indicatorName)
		{
			if (cacheZombie2PlainCandles != null)
				for (int idx = 0; idx < cacheZombie2PlainCandles.Length; idx++)
					if (cacheZombie2PlainCandles[idx] != null && cacheZombie2PlainCandles[idx].IndicatorName == indicatorName && cacheZombie2PlainCandles[idx].EqualsInput(input))
						return cacheZombie2PlainCandles[idx];
			return CacheIndicator<Zombie2PlainCandles>(new Zombie2PlainCandles(){ IndicatorName = indicatorName }, input, ref cacheZombie2PlainCandles);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2PlainCandles Zombie2PlainCandles(string indicatorName)
		{
			return indicator.Zombie2PlainCandles(Input, indicatorName);
		}

		public Indicators.Zombie2PlainCandles Zombie2PlainCandles(ISeries<double> input , string indicatorName)
		{
			return indicator.Zombie2PlainCandles(input, indicatorName);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2PlainCandles Zombie2PlainCandles(string indicatorName)
		{
			return indicator.Zombie2PlainCandles(Input, indicatorName);
		}

		public Indicators.Zombie2PlainCandles Zombie2PlainCandles(ISeries<double> input , string indicatorName)
		{
			return indicator.Zombie2PlainCandles(input, indicatorName);
		}
	}
}

#endregion
