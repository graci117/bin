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
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class KNearestEMA : Indicator
    {
        #region Variables
        private EMA[] emas;
        private int[] emaPeriods;
        private List<EMADistance> emaDistances;
        private int currentNearestPeriod = 0;
        #endregion

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Single EMA line that dynamically shows the nearest EMA to current price";
                Name = "KNearestEMA";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                
                // Default parameters
                EMAPeriodsString = "10,20,50,100,200";
                
                // Add only ONE plot for the nearest EMA
                AddPlot(Brushes.Red, "Nearest EMA");
            }
            else if (State == State.Configure)
            {
                ParseEMAPeriods();
                
                emas = new EMA[emaPeriods.Length];
                emaDistances = new List<EMADistance>();
                
                // Create EMA indicators
                for (int i = 0; i < emaPeriods.Length; i++)
                {
                    emas[i] = EMA(emaPeriods[i]);
                }
            }
            else if (State == State.DataLoaded)
            {
                // Set plot properties for the single nearest EMA line
                Plots[0].Width = 2;
                Plots[0].PlotStyle = PlotStyle.Line;
                Plots[0].Brush = Brushes.Red;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < GetMaxPeriod())
            {
                Values[0][0] = double.NaN;
                return;
            }
                
            // Clear previous distances
            emaDistances.Clear();
            
            // Calculate distances from current price to each EMA
            double currentPrice = Close[0];
            
            for (int i = 0; i < emas.Length; i++)
            {
                if (emas[i] == null || !emas[i].IsValidDataPoint(0))
                    continue;
                    
                double emaValue = emas[i][0];
                double distance = Math.Abs(currentPrice - emaValue);
                
                emaDistances.Add(new EMADistance
                {
                    Index = i,
                    Period = emaPeriods[i],
                    Value = emaValue,
                    Distance = distance
                });
            }
            
            // Find the single nearest EMA
            if (emaDistances.Count > 0)
            {
                var nearestEMA = emaDistances.OrderBy(x => x.Distance).First();
                
                // Plot the nearest EMA value
                Values[0][0] = nearestEMA.Value;
                
                // Update plot name to show current EMA period
                if (currentNearestPeriod != nearestEMA.Period)
                {
                    currentNearestPeriod = nearestEMA.Period;
                    Plots[0].Name = $"EMA{nearestEMA.Period}";
                }
                
                // Optional: Draw label showing current EMA period
                if (CurrentBar % 50 == 0) // Show label every 50 bars
                {
                    DrawLabel(nearestEMA);
                }
            }
            else
            {
                Values[0][0] = double.NaN;
            }
        }
        
        private void DrawLabel(EMADistance nearestEMA)
        {
            // Remove old label
            RemoveDrawObject($"Label_EMA_{CurrentBar - 50}");
            
            // Draw new label showing current EMA period
            string tag = $"Label_EMA_{CurrentBar}";
            Draw.Text(this, tag, $"EMA{nearestEMA.Period}", 0, nearestEMA.Value, Brushes.White);
        }
        
        private void ParseEMAPeriods()
        {
            try
            {
                string[] periodStrings = EMAPeriodsString.Split(',');
                List<int> validPeriods = new List<int>();
                
                foreach (string periodStr in periodStrings)
                {
                    if (int.TryParse(periodStr.Trim(), out int period) && period > 0)
                    {
                        validPeriods.Add(period);
                    }
                }
                
                if (validPeriods.Count == 0)
                {
                    validPeriods.AddRange(new int[] { 10, 20, 50, 100, 200 });
                }
                
                emaPeriods = validPeriods.ToArray();
            }
            catch
            {
                emaPeriods = new int[] { 10, 20, 50, 100, 200 };
            }
        }
        
        private int GetMaxPeriod()
        {
            return emaPeriods != null && emaPeriods.Length > 0 ? emaPeriods.Max() : 200;
        }

        #region Properties
        [Display(Name = "EMA Periods", Description = "Comma-separated EMA periods (e.g., 10,20,50,100,200)", Order = 1, GroupName = "Parameters")]
        public string EMAPeriodsString { get; set; }
        #endregion
    }

    public class EMADistance
    {
        public int Index { get; set; }
        public int Period { get; set; }
        public double Value { get; set; }
        public double Distance { get; set; }
    }
}




#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private KNearestEMA[] cacheKNearestEMA;
		public KNearestEMA KNearestEMA()
		{
			return KNearestEMA(Input);
		}

		public KNearestEMA KNearestEMA(ISeries<double> input)
		{
			if (cacheKNearestEMA != null)
				for (int idx = 0; idx < cacheKNearestEMA.Length; idx++)
					if (cacheKNearestEMA[idx] != null &&  cacheKNearestEMA[idx].EqualsInput(input))
						return cacheKNearestEMA[idx];
			return CacheIndicator<KNearestEMA>(new KNearestEMA(), input, ref cacheKNearestEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.KNearestEMA KNearestEMA()
		{
			return indicator.KNearestEMA(Input);
		}

		public Indicators.KNearestEMA KNearestEMA(ISeries<double> input )
		{
			return indicator.KNearestEMA(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.KNearestEMA KNearestEMA()
		{
			return indicator.KNearestEMA(Input);
		}

		public Indicators.KNearestEMA KNearestEMA(ISeries<double> input )
		{
			return indicator.KNearestEMA(input);
		}
	}
}

#endregion
