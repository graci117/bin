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
using System.Xml.Serialization;  // ADD THIS LINE
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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class DynamicSupportResistance : Indicator
    {
        private int bulgeLengthPrice = 75;
        private int squeezeLengthPrice = 75;
        private int bulgeLengthPrice2 = 20;
        private int squeezeLengthPrice2 = 20;
        
        // Keltner variables
        private double factorK = 2.0;
        private int lengthK = 20;
        private SMA smaKeltner;
        private ATR atrKeltner;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Dynamic Support and Resistance based on CC Candles";
                Name = "DynamicSupportResistance";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                BulgeLengthPrice = 75;
                SqueezeLengthPrice = 75;
                BulgeLengthPrice2 = 20;
                SqueezeLengthPrice2 = 20;
                
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "IntermResistance");
                AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "IntermSupport");
                AddPlot(new Stroke(Brushes.Gray, DashStyleHelper.Dash, 2), PlotStyle.Line, "NearTResistance");
                AddPlot(new Stroke(Brushes.Gray, DashStyleHelper.Dash, 2), PlotStyle.Line, "NearTSupport");
            }
            else if (State == State.DataLoaded)
            {
                smaKeltner = SMA(lengthK);
                atrKeltner = ATR(lengthK);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(Math.Max(bulgeLengthPrice, squeezeLengthPrice), 
                                       Math.Max(bulgeLengthPrice2, squeezeLengthPrice2)))
                return;
            
            // Calculate Keltner conditions for color
            double shift = factorK * atrKeltner[0];
            double upperBandK = smaKeltner[0] + shift;
            double lowerBandK = smaKeltner[0] - shift;
            
            bool conditionK2 = (upperBandK > smaKeltner[1] + factorK * atrKeltner[1]) && 
                               (lowerBandK > smaKeltner[1] - factorK * atrKeltner[1]);
            bool conditionK3 = (upperBandK < smaKeltner[1] + factorK * atrKeltner[1]) && 
                               (lowerBandK < smaKeltner[1] - factorK * atrKeltner[1]);
            
            // Calculate IntermResistance - Highest price over BulgeLengthPrice
            double intermResistance = MAX(Close, BulgeLengthPrice)[0];
            Values[0][0] = intermResistance;
            PlotBrushes[0][0] = conditionK2 ? Brushes.Green : (conditionK3 ? Brushes.Red : Brushes.Gray);
            
            // Calculate IntermSupport - Lowest price over SqueezeLengthPrice
            double intermSupport = MIN(Close, SqueezeLengthPrice)[0];
            Values[1][0] = intermSupport;
            PlotBrushes[1][0] = conditionK2 ? Brushes.Green : (conditionK3 ? Brushes.Red : Brushes.Gray);
            
            // Calculate NearTResistance - Highest price over BulgeLengthPrice2
            double nearTResistance = MAX(Close, BulgeLengthPrice2)[0];
            Values[2][0] = nearTResistance;
            PlotBrushes[2][0] = conditionK2 ? Brushes.Green : (conditionK3 ? Brushes.Red : Brushes.Gray);
            
            // Calculate NearTSupport - Lowest price over SqueezeLengthPrice2
            double nearTSupport = MIN(Close, SqueezeLengthPrice2)[0];
            Values[3][0] = nearTSupport;
            PlotBrushes[3][0] = conditionK2 ? Brushes.Green : (conditionK3 ? Brushes.Red : Brushes.Gray);
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="BulgeLengthPrice", Description="Lookback period for Interim Resistance", Order=1, GroupName="Parameters")]
        public int BulgeLengthPrice
        {
            get { return bulgeLengthPrice; }
            set { bulgeLengthPrice = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="SqueezeLengthPrice", Description="Lookback period for Interim Support", Order=2, GroupName="Parameters")]
        public int SqueezeLengthPrice
        {
            get { return squeezeLengthPrice; }
            set { squeezeLengthPrice = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="BulgeLengthPrice2", Description="Lookback period for Near-Term Resistance", Order=3, GroupName="Parameters")]
        public int BulgeLengthPrice2
        {
            get { return bulgeLengthPrice2; }
            set { bulgeLengthPrice2 = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="SqueezeLengthPrice2", Description="Lookback period for Near-Term Support", Order=4, GroupName="Parameters")]
        public int SqueezeLengthPrice2
        {
            get { return squeezeLengthPrice2; }
            set { squeezeLengthPrice2 = Math.Max(1, value); }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IntermResistance
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IntermSupport
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> NearTResistance
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> NearTSupport
        {
            get { return Values[3]; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DynamicSupportResistance[] cacheDynamicSupportResistance;
		public DynamicSupportResistance DynamicSupportResistance(int bulgeLengthPrice, int squeezeLengthPrice, int bulgeLengthPrice2, int squeezeLengthPrice2)
		{
			return DynamicSupportResistance(Input, bulgeLengthPrice, squeezeLengthPrice, bulgeLengthPrice2, squeezeLengthPrice2);
		}

		public DynamicSupportResistance DynamicSupportResistance(ISeries<double> input, int bulgeLengthPrice, int squeezeLengthPrice, int bulgeLengthPrice2, int squeezeLengthPrice2)
		{
			if (cacheDynamicSupportResistance != null)
				for (int idx = 0; idx < cacheDynamicSupportResistance.Length; idx++)
					if (cacheDynamicSupportResistance[idx] != null && cacheDynamicSupportResistance[idx].BulgeLengthPrice == bulgeLengthPrice && cacheDynamicSupportResistance[idx].SqueezeLengthPrice == squeezeLengthPrice && cacheDynamicSupportResistance[idx].BulgeLengthPrice2 == bulgeLengthPrice2 && cacheDynamicSupportResistance[idx].SqueezeLengthPrice2 == squeezeLengthPrice2 && cacheDynamicSupportResistance[idx].EqualsInput(input))
						return cacheDynamicSupportResistance[idx];
			return CacheIndicator<DynamicSupportResistance>(new DynamicSupportResistance(){ BulgeLengthPrice = bulgeLengthPrice, SqueezeLengthPrice = squeezeLengthPrice, BulgeLengthPrice2 = bulgeLengthPrice2, SqueezeLengthPrice2 = squeezeLengthPrice2 }, input, ref cacheDynamicSupportResistance);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DynamicSupportResistance DynamicSupportResistance(int bulgeLengthPrice, int squeezeLengthPrice, int bulgeLengthPrice2, int squeezeLengthPrice2)
		{
			return indicator.DynamicSupportResistance(Input, bulgeLengthPrice, squeezeLengthPrice, bulgeLengthPrice2, squeezeLengthPrice2);
		}

		public Indicators.DynamicSupportResistance DynamicSupportResistance(ISeries<double> input , int bulgeLengthPrice, int squeezeLengthPrice, int bulgeLengthPrice2, int squeezeLengthPrice2)
		{
			return indicator.DynamicSupportResistance(input, bulgeLengthPrice, squeezeLengthPrice, bulgeLengthPrice2, squeezeLengthPrice2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DynamicSupportResistance DynamicSupportResistance(int bulgeLengthPrice, int squeezeLengthPrice, int bulgeLengthPrice2, int squeezeLengthPrice2)
		{
			return indicator.DynamicSupportResistance(Input, bulgeLengthPrice, squeezeLengthPrice, bulgeLengthPrice2, squeezeLengthPrice2);
		}

		public Indicators.DynamicSupportResistance DynamicSupportResistance(ISeries<double> input , int bulgeLengthPrice, int squeezeLengthPrice, int bulgeLengthPrice2, int squeezeLengthPrice2)
		{
			return indicator.DynamicSupportResistance(input, bulgeLengthPrice, squeezeLengthPrice, bulgeLengthPrice2, squeezeLengthPrice2);
		}
	}
}

#endregion
