//
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.ComponentModel;             // For BrowsableAttribute, [CategoryAttribute implicitly if used by NT]
using System.ComponentModel.DataAnnotations; // For RangeAttribute, DisplayAttribute
using System.Windows.Media;           // For Brush, Brushes
using System.Xml.Serialization;       // For XmlIgnoreAttribute
using NinjaTrader.Cbi;                // For DashStyleHelper
using NinjaTrader.Gui;                // For various GUI elements if needed
using NinjaTrader.Gui.Chart;          // For PlotStyle, ScaleJustification
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript;        // For Indicator, NinjaScriptProperty, Custom.Resource, Serialize, AddPlot, Values, PlotBrushes etc.
using NinjaTrader.Core.FloatingPoint; // For ApproxCompare, Globals (though not directly used in simplified PSAR)
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
    /// <summary>
    /// Parabolic SAR with color-changing dots based on trend direction.
    /// </summary>
    public class ParabolicSAR2 : Indicator
    {
        // --- Member Variables ---
        private double af;
        private bool longPosition;
        private double xp;
        // Removed prevSAR as a class member for the OnBarClose version, will use Value[1]

        // Color Properties (defined below in Properties region)

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = Custom.Resource.NinjaScriptIndicatorDescriptionParabolicSAR + " (Color Changing)";
                Name = "ParabolicSAR2"; // Keep this name consistent
                Acceleration = 0.02;
                AccelerationStep = 0.02;
                AccelerationMax = 0.2;
                Calculate = Calculate.OnBarClose; // Crucial for this simplified logic
                IsSuspendedWhileInactive = true;
                IsOverlay = true;
                DisplayInDataBox = true;      // Standard defaults
                DrawOnPricePanel = true;      // Standard defaults
                DrawHorizontalGridLines = true; // Standard defaults
                DrawVerticalGridLines = true;   // Standard defaults
                PaintPriceMarkers = false;      // Usually false for SAR as it is the price marker

                AddPlot(new Stroke(Brushes.DimGray, 2), PlotStyle.Dot, "SAR"); // Name of plot

                // Initialize public color properties
                UpTrendColor = Brushes.Lime;
                DownTrendColor = Brushes.Red;
            }
            else if (State == State.Configure)
            {
                // Minimal configure for OnBarClose version, state set in OnBarUpdate or DataLoaded
                af = 0; // Will be set on first bar
                xp = 0; // Will be set on first bar
                longPosition = true; // Initial guess
            }
            else if (State == State.DataLoaded)
            {
                // No complex series needed for this simplified OnBarClose version
                // highSeries and lowSeries are directly accessed via High[0], Low[0] etc.
            }
        }

        protected override void OnBarUpdate()
        {
            // Initial Bar (CurrentBar == 0)
            if (CurrentBar == 0)
            {
                longPosition = Close[0] > Open[0]; // Initial trend guess
                xp = longPosition ? High[0] : Low[0];
                af = Acceleration; // Use the public property for initial AF
                Value[0] = longPosition ? Low[0] - TickSize * 2 : High[0] + TickSize * 2;
                PlotBrushes[0][0] = longPosition ? UpTrendColor : DownTrendColor;
                //Print($"Bar: {CurrentBar} INIT SAR: {Value[0]:F2} Long: {longPosition} XP: {xp:F2} AF: {af:F4}");
                return;
            }

            // Subsequent Bars for Calculate.OnBarClose
            double sarPrev = Value[1]; // SAR from the fully completed previous bar
            bool longPosPrev = (PlotBrushes[0][1] == UpTrendColor); // Infer from previous bar's dot color
                                                                    // This works for OnBarClose. For OnPriceChange, need Series.
            double xpPrev = xp; // This was set on the previous bar's calculation
            double afPrev = af; // This was set on the previous bar's calculation

            // If trend had just flipped on the previous bar, af should be initial Acceleration
            // More robust check: if (longPosition != longPosPrev) afPrev = Acceleration; (requires storing longPosition)
            // For now, this simplified carry-over will work for OnBarClose mostly
            // but a full PSAR might reset AF on every reversal explicitly.

            double currentSAR;

            if (longPosPrev) // Assumed to be in a long trend from previous bar
            {
                currentSAR = sarPrev + afPrev * (xpPrev - sarPrev);
                // SAR cannot be higher than the low of the previous OR second previous bar
                currentSAR = Math.Min(currentSAR, Low[1]);
                if (CurrentBar > 1) currentSAR = Math.Min(currentSAR, Low[2]);

                if (Low[0] < currentSAR) // Reversal to short
                {
                    currentSAR = xpPrev;    // SAR becomes the previous extreme high point
                    longPosition = false; // Flip trend state
                    xp = Low[0];          // New extreme point is the current low
                    af = Acceleration;    // Reset acceleration factor
                }
                else // No reversal, continue long
                {
                    longPosition = true; // Confirm current trend state
                    // Update SAR to not penetrate current low if needed for strictness, usually PSAR can be penetrated intraday and flips on next bar
                    // currentSAR = Math.Min(currentSAR, Low[0]);
                    if (High[0] > xpPrev) // New extreme high made
                    {
                        xp = High[0];
                        af = Math.Min(AccelerationMax, afPrev + AccelerationStep);
                    }
                    // else: xp and af remain as xpPrev, afPrev
                }
            }
            else // Assumed to be in a short trend from previous bar
            {
                currentSAR = sarPrev + afPrev * (xpPrev - sarPrev);
                // SAR cannot be lower than the high of the previous OR second previous bar
                currentSAR = Math.Max(currentSAR, High[1]);
                if (CurrentBar > 1) currentSAR = Math.Max(currentSAR, High[2]);

                if (High[0] > currentSAR) // Reversal to long
                {
                    currentSAR = xpPrev;
                    longPosition = true; // Flip trend state
                    xp = High[0];
                    af = Acceleration;
                }
                else // No reversal, continue short
                {
                    longPosition = false; // Confirm current trend state
                    // currentSAR = Math.Max(currentSAR, High[0]);
                    if (Low[0] < xpPrev) // New extreme low made
                    {
                        xp = Low[0];
                        af = Math.Min(AccelerationMax, afPrev + AccelerationStep);
                    }
                    // else: xp and af remain as xpPrev, afPrev
                }
            }

            Value[0] = currentSAR;
            PlotBrushes[0][0] = longPosition ? UpTrendColor : DownTrendColor;

            // Class members 'xp', 'af', and 'longPosition' are now updated for the next bar.
            // Print($"Bar: {CurrentBar} SAR: {Value[0]:F2} Long: {longPosition} XP: {xp:F2} AF: {af:F4} PrevSAR: {sarPrev:F2}");
        }


        #region Properties
        [Range(0.00, double.MaxValue)]
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Acceleration", GroupName = "NinjaScriptParameters", Order = 0)]
        public double Acceleration { get; set; }

        [Range(0.001, double.MaxValue)]
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "AccelerationMax", GroupName = "NinjaScriptParameters", Order = 1)]
        public double AccelerationMax { get; set; }

        [Range(0.001, double.MaxValue)]
        [NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "AccelerationStep", GroupName = "NinjaScriptParameters", Order = 2)]
        public double AccelerationStep { get; set; }

        // Color Properties
        [XmlIgnore]
        [Display(Name = "Up Trend Color", GroupName = "Visual", Order = 3)]
        public Brush UpTrendColor { get; set; }

        [Browsable(false)]
        public string UpTrendColorSerializable
        {
            get { return Serialize.BrushToString(UpTrendColor); }
            set { UpTrendColor = Serialize.StringToBrush(value); }
        }

        [XmlIgnore]
        [Display(Name = "Down Trend Color", GroupName = "Visual", Order = 4)]
        public Brush DownTrendColor { get; set; }

        [Browsable(false)]
        public string DownTrendColorSerializable
        {
            get { return Serialize.BrushToString(DownTrendColor); }
            set { DownTrendColor = Serialize.StringToBrush(value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.ParabolicSAR2[] cacheParabolicSAR2;
		public AlgoTrader.ParabolicSAR2 ParabolicSAR2(double acceleration, double accelerationMax, double accelerationStep)
		{
			return ParabolicSAR2(Input, acceleration, accelerationMax, accelerationStep);
		}

		public AlgoTrader.ParabolicSAR2 ParabolicSAR2(ISeries<double> input, double acceleration, double accelerationMax, double accelerationStep)
		{
			if (cacheParabolicSAR2 != null)
				for (int idx = 0; idx < cacheParabolicSAR2.Length; idx++)
					if (cacheParabolicSAR2[idx] != null && cacheParabolicSAR2[idx].Acceleration == acceleration && cacheParabolicSAR2[idx].AccelerationMax == accelerationMax && cacheParabolicSAR2[idx].AccelerationStep == accelerationStep && cacheParabolicSAR2[idx].EqualsInput(input))
						return cacheParabolicSAR2[idx];
			return CacheIndicator<AlgoTrader.ParabolicSAR2>(new AlgoTrader.ParabolicSAR2(){ Acceleration = acceleration, AccelerationMax = accelerationMax, AccelerationStep = accelerationStep }, input, ref cacheParabolicSAR2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.ParabolicSAR2 ParabolicSAR2(double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.ParabolicSAR2(Input, acceleration, accelerationMax, accelerationStep);
		}

		public Indicators.AlgoTrader.ParabolicSAR2 ParabolicSAR2(ISeries<double> input , double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.ParabolicSAR2(input, acceleration, accelerationMax, accelerationStep);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.ParabolicSAR2 ParabolicSAR2(double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.ParabolicSAR2(Input, acceleration, accelerationMax, accelerationStep);
		}

		public Indicators.AlgoTrader.ParabolicSAR2 ParabolicSAR2(ISeries<double> input , double acceleration, double accelerationMax, double accelerationStep)
		{
			return indicator.ParabolicSAR2(input, acceleration, accelerationMax, accelerationStep);
		}
	}
}

#endregion
