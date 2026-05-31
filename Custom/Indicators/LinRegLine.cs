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
namespace NinjaTrader.NinjaScript.Indicators
{
	public class LinRegLine : Indicator
	{
		Series<double> BClose;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "LinRegLine";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Linreg_length					= 11;
				Signal_length					= 7;
				
				//AddPlot(Brushes.Peru, "SignalLine");
				AddPlot(new Stroke(Brushes.Peru, 3), PlotStyle.Line, "SignalLine");
			}
			else if (State == State.Configure)
			{
				BClose	= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			//Add your custom indicator logic here.
			BClose[0] = LinReg(Close, Linreg_length)[0];
			SignalLine[0] = SMA(BClose,Signal_length).Value[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Linreg_length", Order=1, GroupName="Parameters")]
		public int Linreg_length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Signal_length", Order=2, GroupName="Parameters")]
		public int Signal_length
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SignalLine
		{
			get { return Values[0]; }
		}
		
	
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinRegLine[] cacheLinRegLine;
		public LinRegLine LinRegLine(int linreg_length, int signal_length)
		{
			return LinRegLine(Input, linreg_length, signal_length);
		}

		public LinRegLine LinRegLine(ISeries<double> input, int linreg_length, int signal_length)
		{
			if (cacheLinRegLine != null)
				for (int idx = 0; idx < cacheLinRegLine.Length; idx++)
					if (cacheLinRegLine[idx] != null && cacheLinRegLine[idx].Linreg_length == linreg_length && cacheLinRegLine[idx].Signal_length == signal_length && cacheLinRegLine[idx].EqualsInput(input))
						return cacheLinRegLine[idx];
			return CacheIndicator<LinRegLine>(new LinRegLine(){ Linreg_length = linreg_length, Signal_length = signal_length }, input, ref cacheLinRegLine);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegLine LinRegLine(int linreg_length, int signal_length)
		{
			return indicator.LinRegLine(Input, linreg_length, signal_length);
		}

		public Indicators.LinRegLine LinRegLine(ISeries<double> input , int linreg_length, int signal_length)
		{
			return indicator.LinRegLine(input, linreg_length, signal_length);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegLine LinRegLine(int linreg_length, int signal_length)
		{
			return indicator.LinRegLine(Input, linreg_length, signal_length);
		}

		public Indicators.LinRegLine LinRegLine(ISeries<double> input , int linreg_length, int signal_length)
		{
			return indicator.LinRegLine(input, linreg_length, signal_length);
		}
	}
}

#endregion
