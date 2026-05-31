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
    /// Plots the dynamic support/resistance
    /// </summary>
    public class DynamicSRColorBandNT8 : Indicator
    {
        #region Variables
        // Wizard generated variables
        private int			period				= 21;     
        private double 		HH					=0;
		private double			LL					=0;
		// User defined variables (add any user defined variables below)
		private double		DynamicR			= 0;
		private double		DynamicS			= 0;
		
		private int			barcounter			= 0;
		
		private bool bandArea  = true;
		private Brush bandAreaColor = Brushes.Turquoise;
		
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        private void Initialize()
        {
            AddPlot(new Stroke(Brushes.Blue,1), PlotStyle.Line,"Resistance");
			AddPlot(new Stroke(Brushes.Magenta,1), PlotStyle.Line, "Support");
			AddPlot(new Stroke(Brushes.Transparent,1), PlotStyle.Line, "Average");
			
            Calculate	= Calculate.OnEachTick;
            IsOverlay				= true;
            /* NT8 REMOVED: PriceTypeSupported	= false */;
			PaintPriceMarkers	= true;
			IsAutoScale			= false;
			BarsRequiredToPlot		= 1;
        }

        protected override void OnStateChange()
        {
            switch (State)
            {
                case State.SetDefaults:
                    Name = "Dynamic S/R ColorBand NT8";
                    Description = "Plots the dynamic support/resistance";
                    Initialize();
                    break;
             }
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {	Resistance[0] = High[0];
			Support[0] = Low[0];
            if ( CurrentBar < Period )	return;
			Average[0] = HMA(period)[0];
			
			if (Average[0]>Average[1])
				
			{
				HH = Math.Max(High[0],HH);
				Resistance[0] = Resistance[1];
				Support[0] = Support[1];
			}
			if (Average[0]<Average[1])
				
			{	
				LL=Math.Min(Low[0],LL);
				Support[0] = Support[1];
				Resistance[0] = Resistance[1];
			}
			
			if (Average[0] <Average[1] && Average[1]>Average[2])
				
			{
				Resistance[0] = HH;
				LL=Low[0];
			}

			if (Average[0]>Average[1] && Average[1]<Average[2])
				
			{
				Support[0] = LL;
				HH=High[0];
				
			}
		
			//Support.Set(LL);
			//Resistance.Set(HH);
			
			if(BandArea){
				//DrawRegion("Bollinger Upper Region", CurrentBar, 0, DynamicSR_Color(55).Resistance, DynamicSR_Color(55).Support, Color.Black, Color.Blue, 1);
				//DrawRegion("tag1", CurrentBar, 0, Resistance, Support, Color.Empty, Color.LimeGreen,2);
				Draw.Region(this,"tag1", CurrentBar, 0, Resistance, Support, Brushes.Transparent, bandAreaColor,30);
			}
		}
	

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> Resistance
        {
            get { return Values[0]; }
        }
		
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> Support
        {
            get { return Values[1]; }
        }
		
		[Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public Series<double> Average
        {
            get { return Values[2]; }
        }
		
		[NinjaScriptProperty]		
		[Display(Description = "Numbers of bars used for calculations", GroupName = "Parameters", Order = 1)]		
		public int Period
		{
			get { return period; }
			set { period = Math.Max(1, value); }
		}
		
		[NinjaScriptProperty]		
		[Display(Description = "Have ColorBand between support and resistance", GroupName = "Parameters", Order = 2)]		
		public bool BandArea
		{
			get { return bandArea; }
			set { bandArea = value; }
		}
        		
		[XmlIgnore]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove   		
		[Display(Description = "Color of Band between support and resistance", GroupName = "Band Color", Order = 1)]
        public Brush BandAreaColor
		{
			get { return bandAreaColor; }
			set { bandAreaColor = value; }
		}
		[Browsable(false)]
		public string BandAreaColorSerialize
		{
			get { return Serialize.BrushToString(BandAreaColor); }
			set { BandAreaColor = Serialize.StringToBrush(value); }
		}
		

		
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DynamicSRColorBandNT8[] cacheDynamicSRColorBandNT8;
		public DynamicSRColorBandNT8 DynamicSRColorBandNT8(int period, bool bandArea)
		{
			return DynamicSRColorBandNT8(Input, period, bandArea);
		}

		public DynamicSRColorBandNT8 DynamicSRColorBandNT8(ISeries<double> input, int period, bool bandArea)
		{
			if (cacheDynamicSRColorBandNT8 != null)
				for (int idx = 0; idx < cacheDynamicSRColorBandNT8.Length; idx++)
					if (cacheDynamicSRColorBandNT8[idx] != null && cacheDynamicSRColorBandNT8[idx].Period == period && cacheDynamicSRColorBandNT8[idx].BandArea == bandArea && cacheDynamicSRColorBandNT8[idx].EqualsInput(input))
						return cacheDynamicSRColorBandNT8[idx];
			return CacheIndicator<DynamicSRColorBandNT8>(new DynamicSRColorBandNT8(){ Period = period, BandArea = bandArea }, input, ref cacheDynamicSRColorBandNT8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DynamicSRColorBandNT8 DynamicSRColorBandNT8(int period, bool bandArea)
		{
			return indicator.DynamicSRColorBandNT8(Input, period, bandArea);
		}

		public Indicators.DynamicSRColorBandNT8 DynamicSRColorBandNT8(ISeries<double> input , int period, bool bandArea)
		{
			return indicator.DynamicSRColorBandNT8(input, period, bandArea);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DynamicSRColorBandNT8 DynamicSRColorBandNT8(int period, bool bandArea)
		{
			return indicator.DynamicSRColorBandNT8(Input, period, bandArea);
		}

		public Indicators.DynamicSRColorBandNT8 DynamicSRColorBandNT8(ISeries<double> input , int period, bool bandArea)
		{
			return indicator.DynamicSRColorBandNT8(input, period, bandArea);
		}
	}
}

#endregion
