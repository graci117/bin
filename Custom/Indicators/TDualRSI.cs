// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The TDualRSI is plot aove and below 50 line.
	/// </summary>
	[Gui.CategoryOrder("Misc:", 80)]
	public class TDualRSI : Indicator
	{
		private MAX max;
		private MIN min;
		private RSI rsi;
		private string				miscString				= "DualRSI = +/-RSI on Zero line";
  private bool		usePrice	= true;	
			private Series<double>		ema1;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "This is not RSI of RSI, Dual = -RSI and +RSI";
				Name						= "TDualRSI";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= false;
				Period						= 50;

				AddPlot(new Stroke(Brushes.Yellow, 1), PlotStyle.Line, "Middle");
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Bar, "Upper");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Lower");
	
				AddLine(Brushes.Black,	0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorNeutral);

			}
			else if (State == State.Configure)
			{
				rsi = RSI(Inputs[0], Period, 1);
			}
			else if (State == State.DataLoaded)
			{
				ema1 = new Series<double>(this);

			}
		}
		
		protected override void OnBarUpdate()
		{
			double rsi0 = rsi[0];
			
			if (usePrice)
			{
	          ema1[0] = ( Open[0] + High[0] + Low[0] + Close[0] ) / 4.0 ;	
			}
//     RSIFast.Set(RSI(temp,PeriodFast,1)[0] - 50);
if (CurrentBar < Period)
	return;
			if (usePrice)
			    Middle[0] = RSI(ema1,Period,1)[0] - 50;
			else
				Middle[0] = (rsi0 - 50);
			
			if (Middle[0] > 0)
				Upper[0] = Middle[0];
			 else
				Lower[0] = Middle[0];
		

		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[0]; }
		}
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Use O+H+L+C/4 instead ", GroupName = "OHLC Options", Order = 2)]
        public bool UsePrice
        {
            get { return usePrice; }
            set { usePrice = value; }
        }
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Note", Description = "misc note", GroupName = "Note", Order = 0)]
		public string MiscString
		{	
            get { return miscString; }
            set { ; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TDualRSI[] cacheTDualRSI;
		public TDualRSI TDualRSI(int period)
		{
			return TDualRSI(Input, period);
		}

		public TDualRSI TDualRSI(ISeries<double> input, int period)
		{
			if (cacheTDualRSI != null)
				for (int idx = 0; idx < cacheTDualRSI.Length; idx++)
					if (cacheTDualRSI[idx] != null && cacheTDualRSI[idx].Period == period && cacheTDualRSI[idx].EqualsInput(input))
						return cacheTDualRSI[idx];
			return CacheIndicator<TDualRSI>(new TDualRSI(){ Period = period }, input, ref cacheTDualRSI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TDualRSI TDualRSI(int period)
		{
			return indicator.TDualRSI(Input, period);
		}

		public Indicators.TDualRSI TDualRSI(ISeries<double> input , int period)
		{
			return indicator.TDualRSI(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TDualRSI TDualRSI(int period)
		{
			return indicator.TDualRSI(Input, period);
		}

		public Indicators.TDualRSI TDualRSI(ISeries<double> input , int period)
		{
			return indicator.TDualRSI(input, period);
		}
	}
}

#endregion
