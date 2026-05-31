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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class T3Cloud : Indicator
    {
        private TillsonT3 T1;
        private TillsonT3 T2;
        private TillsonT3 T3;
        private TillsonT3 T4;
        private TillsonT3 T5;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"T3Cloud with 5 Tillson T3 indicators";
                Name = "T3Cloud";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                T1Length = 6;
                T1VolumeFactor = 0.1;
                T2Length = 8;
                T2VolumeFactor = 0.5;
                T3Length = 6;
                T3VolumeFactor = 0.35;
                T4Length = 5;
                T4VolumeFactor = 0.3;
                T5Length = 4;
                T5VolumeFactor = 0.3;

                AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "T1");
                AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "T2");
                AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "T3");
                AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Line, "T4");
                AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Line, "T5");
            }
            else if (State == State.Configure)
            {
                T1 = TillsonT3(T1Length, T1VolumeFactor);
                T2 = TillsonT3(T2Length, T2VolumeFactor);
                T3 = TillsonT3(T3Length, T3VolumeFactor);
                T4 = TillsonT3(T4Length, T4VolumeFactor);
                T5 = TillsonT3(T5Length, T5VolumeFactor);
            }
        }

        protected override void OnBarUpdate()
        {
			  if (CurrentBar < 1)
        		return;
			
            // Use Values[] to set the values for each plot
            Values[0][0] = T1.Value[0];
            Values[1][0] = T2.Value[0];
            Values[2][0] = T3.Value[0];
            Values[3][0] = T4.Value[0];
            Values[4][0] = T5.Value[0];
            
            // Optional: Add color changes based on direction
            if (IsFirstTickOfBar)
            {
                // T1 color change
                if (Values[0][0] > Values[0][1])
                    PlotBrushes[0][0] = Brushes.LimeGreen;
                else if (Values[0][0] < Values[0][1])
                    PlotBrushes[0][0] = Brushes.Red;
                else
                    PlotBrushes[0][0] = Brushes.Gray;
                
                // T2 color change
                if (Values[1][0] > Values[1][1])
                    PlotBrushes[1][0] = Brushes.LimeGreen;
                else if (Values[1][0] < Values[1][1])
                    PlotBrushes[1][0] = Brushes.Red;
                else
                    PlotBrushes[1][0] = Brushes.Blue;
                
                // T3 color change
                if (Values[2][0] > Values[2][1])
                    PlotBrushes[2][0] = Brushes.LimeGreen;
                else if (Values[2][0] < Values[2][1])
                    PlotBrushes[2][0] = Brushes.Red;
                else
                    PlotBrushes[2][0] = Brushes.Green;
                
                // T4 color change
                if (Values[3][0] > Values[3][1])
                    PlotBrushes[3][0] = Brushes.LimeGreen;
                else if (Values[3][0] < Values[3][1])
                    PlotBrushes[3][0] = Brushes.Red;
                else
                    PlotBrushes[3][0] = Brushes.Orange;
                
                // T5 color change
                if (Values[4][0] > Values[4][1])
                    PlotBrushes[4][0] = Brushes.LimeGreen;
                else if (Values[4][0] < Values[4][1])
                    PlotBrushes[4][0] = Brushes.Red;
                else
                    PlotBrushes[4][0] = Brushes.Purple;
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T1 Length", Order=1, GroupName="Parameters")]
        public int T1Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T1 Volume Factor", Order=2, GroupName="Parameters")]
        public double T1VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T2 Length", Order=3, GroupName="Parameters")]
        public int T2Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T2 Volume Factor", Order=4, GroupName="Parameters")]
        public double T2VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T3 Length", Order=5, GroupName="Parameters")]
        public int T3Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T3 Volume Factor", Order=6, GroupName="Parameters")]
        public double T3VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T4 Length", Order=7, GroupName="Parameters")]
        public int T4Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T4 Volume Factor", Order=8, GroupName="Parameters")]
        public double T4VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T5 Length", Order=9, GroupName="Parameters")]
        public int T5Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T5 Volume Factor", Order=10, GroupName="Parameters")]
        public double T5VolumeFactor { get; set; }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private T3Cloud[] cacheT3Cloud;
		public T3Cloud T3Cloud(int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor)
		{
			return T3Cloud(Input, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor);
		}

		public T3Cloud T3Cloud(ISeries<double> input, int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor)
		{
			if (cacheT3Cloud != null)
				for (int idx = 0; idx < cacheT3Cloud.Length; idx++)
					if (cacheT3Cloud[idx] != null && cacheT3Cloud[idx].T1Length == t1Length && cacheT3Cloud[idx].T1VolumeFactor == t1VolumeFactor && cacheT3Cloud[idx].T2Length == t2Length && cacheT3Cloud[idx].T2VolumeFactor == t2VolumeFactor && cacheT3Cloud[idx].T3Length == t3Length && cacheT3Cloud[idx].T3VolumeFactor == t3VolumeFactor && cacheT3Cloud[idx].T4Length == t4Length && cacheT3Cloud[idx].T4VolumeFactor == t4VolumeFactor && cacheT3Cloud[idx].T5Length == t5Length && cacheT3Cloud[idx].T5VolumeFactor == t5VolumeFactor && cacheT3Cloud[idx].EqualsInput(input))
						return cacheT3Cloud[idx];
			return CacheIndicator<T3Cloud>(new T3Cloud(){ T1Length = t1Length, T1VolumeFactor = t1VolumeFactor, T2Length = t2Length, T2VolumeFactor = t2VolumeFactor, T3Length = t3Length, T3VolumeFactor = t3VolumeFactor, T4Length = t4Length, T4VolumeFactor = t4VolumeFactor, T5Length = t5Length, T5VolumeFactor = t5VolumeFactor }, input, ref cacheT3Cloud);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.T3Cloud T3Cloud(int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor)
		{
			return indicator.T3Cloud(Input, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor);
		}

		public Indicators.T3Cloud T3Cloud(ISeries<double> input , int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor)
		{
			return indicator.T3Cloud(input, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.T3Cloud T3Cloud(int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor)
		{
			return indicator.T3Cloud(Input, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor);
		}

		public Indicators.T3Cloud T3Cloud(ISeries<double> input , int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor)
		{
			return indicator.T3Cloud(input, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor);
		}
	}
}

#endregion
