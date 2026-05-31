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

//This namespace holds Indicators in this folder and is required. Do not change it. 

//Credit goes to original Author. Was just modified to be automated with the Predator X Order Entry
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber_SignalMod
{
	public class TOWilliamsTraderOracleSignalMOD : Indicator
	{
		private MAX max;
		private MIN min;
		
		private bool SignalOnce = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "TOWilliamsTraderOracleSignalMOD";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				Period										= 14;
				
				LongEntry									= "LongEntry";
				ShortEntry									= "ShortEntry";

				AddLine(Brushes.DarkGray,	-25,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddLine(Brushes.DarkGray,	-75,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
				AddPlot(Brushes.Goldenrod,			NinjaTrader.Custom.Resource.WilliamsPercentR);
			}
			else if (State == State.DataLoaded)
			{
				max = MAX(High, Period);
				min	= MIN(Low, Period);
			}
		}

        private void DrawText(float x, float y, string text, int fontSize, SolidColorBrush br)
        {
        }

        protected override void OnBarUpdate()
		{
			double max0	= max[0];
			double min0	= min[0];
			Value[0]	= -100 * (max0 - Close[0]) / (max0 - min0 == 0 ? 1 : max0 - min0);
			Plots[0].Width = 5;
            if (Value[0] > -2)
			{
                BarBrushes[0] = Brushes.Lime;
                PlotBrushes[0][0] = Brushes.Lime;
                DrawText(130, 130, "Willims LONG", 14, Brushes.Lime);
				
				if (!SignalOnce)
				{
					Draw.ArrowUp(this, LongEntry + CurrentBar, true, 0, Low[0] - TickSize, Brushes.Green);
					SignalOnce	= true;
				}
				
            }
            else if (Value[0] < -90)
            {
                BarBrushes[0] = Brushes.Red;
                PlotBrushes[0][0] = Brushes.Red;
                DrawText(130, 130, "Willims SHORT", 14, Brushes.Red);
				
				if (!SignalOnce)
				{
					Draw.ArrowDown(this, ShortEntry + CurrentBar, true, 0, High[0] + TickSize, Brushes.DodgerBlue);
					SignalOnce	= true;
				}
				
            }
			else
            {
                BarBrushes[0] = Brushes.DarkGray;
                PlotBrushes[0][0] = Brushes.DarkSlateGray;
				SignalOnce	= false;
            }
        }

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="Long Entry", Description="Signals to Automate this indicator with the Predator X Order Entry", Order=1, GroupName="Predator Signals")]
		public string LongEntry
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Short Entry", Description="Signals to Automate this indicator with the Predator X Order Entry", Order=2, GroupName="Predator Signals")]
		public string ShortEntry
		{ get; set; }
		
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD[] cacheTOWilliamsTraderOracleSignalMOD;
		public TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD TOWilliamsTraderOracleSignalMOD(int period, string longEntry, string shortEntry)
		{
			return TOWilliamsTraderOracleSignalMOD(Input, period, longEntry, shortEntry);
		}

		public TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD TOWilliamsTraderOracleSignalMOD(ISeries<double> input, int period, string longEntry, string shortEntry)
		{
			if (cacheTOWilliamsTraderOracleSignalMOD != null)
				for (int idx = 0; idx < cacheTOWilliamsTraderOracleSignalMOD.Length; idx++)
					if (cacheTOWilliamsTraderOracleSignalMOD[idx] != null && cacheTOWilliamsTraderOracleSignalMOD[idx].Period == period && cacheTOWilliamsTraderOracleSignalMOD[idx].LongEntry == longEntry && cacheTOWilliamsTraderOracleSignalMOD[idx].ShortEntry == shortEntry && cacheTOWilliamsTraderOracleSignalMOD[idx].EqualsInput(input))
						return cacheTOWilliamsTraderOracleSignalMOD[idx];
			return CacheIndicator<TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD>(new TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD(){ Period = period, LongEntry = longEntry, ShortEntry = shortEntry }, input, ref cacheTOWilliamsTraderOracleSignalMOD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD TOWilliamsTraderOracleSignalMOD(int period, string longEntry, string shortEntry)
		{
			return indicator.TOWilliamsTraderOracleSignalMOD(Input, period, longEntry, shortEntry);
		}

		public Indicators.TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD TOWilliamsTraderOracleSignalMOD(ISeries<double> input , int period, string longEntry, string shortEntry)
		{
			return indicator.TOWilliamsTraderOracleSignalMOD(input, period, longEntry, shortEntry);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD TOWilliamsTraderOracleSignalMOD(int period, string longEntry, string shortEntry)
		{
			return indicator.TOWilliamsTraderOracleSignalMOD(Input, period, longEntry, shortEntry);
		}

		public Indicators.TradeSaber_SignalMod.TOWilliamsTraderOracleSignalMOD TOWilliamsTraderOracleSignalMOD(ISeries<double> input , int period, string longEntry, string shortEntry)
		{
			return indicator.TOWilliamsTraderOracleSignalMOD(input, period, longEntry, shortEntry);
		}
	}
}

#endregion
