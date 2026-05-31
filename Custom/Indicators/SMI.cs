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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	// Converted to NT8-b3 by Ninjscript team 8/9/15.
	
	public class SMI : Indicator
	{
		private int	range		= 13;
		private int	emaperiod1	= 25;
		private int	emaperiod2	= 1;
		private int smiemaperiod= 25;
		
		private Series<double>	sms;
		private Series<double>	hls;
		private Series<double>	smis;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"The Stochastic Momentum Index is made up of two lines that oscillate between a vertical scale of -100 to 100.";
				Name						= "SMI";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;

				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "SMI");
				AddPlot(Brushes.Orange, "SMIEMA");
				AddLine(Brushes.DarkGray, 1, "ZeroLine");
				AddPlot(new Stroke(Brushes.Crimson, 3), PlotStyle.Bar, "SMIDiff");
			}
			else if (State == State.Configure)
			{
				//stochastic momentums
				sms		= new Series<double>(this);
				//high low diffs
				hls		= new Series<double>(this);
				//stochastic momentum indexes
				smis	= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (( CurrentBar < emaperiod2) || ( CurrentBar < emaperiod1)) 
			{
				return;
			}
			
			//Stochastic Momentum = SM {distance of close - midpoint}
		 	sms[0] = (Close[0] - 0.5 * ((MAX(High, range)[0] + MIN(Low, range)[0])));
			
			//High low diffs
			hls[0] = (MAX(High, range)[0] - MIN(Low, range)[0]);

			//Stochastic Momentum Index = SMI
			double denom = 0.5*EMA(EMA(hls,emaperiod1),emaperiod2)[0];
 			smis[0] = (100*(EMA(EMA(sms,emaperiod1),emaperiod2))[0] / (denom ==0 ? 1 : denom  ));
			
			//Set the current SMI line value
			smi[0] = (smis[0]);
			
			//Set the line value for the SMIEMA by taking the EMA of the SMI
			SMIEMA[0] = (EMA(smis, smiemaperiod)[0]);
			
			SMIDiff[0] = smi[0] - SMIEMA[0];

		}

		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EMAPeriod1", Description="1st ema smothing period. ( R )", Order=1, GroupName="Parameters")]
		public int EMAPeriod1
		{
			get { return emaperiod1; }
			set { emaperiod1 = Math.Max(1, value); }
		}

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EMAPeriod2", Description="2nd ema smoothing period. ( S )", Order=2, GroupName="Parameters")]
		public int EMAPeriod2
		{
			get { return emaperiod2; }
			set { emaperiod2 = Math.Max(1, value); }
		}
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Range", Description="Range for momentum Calculation ( Q )", Order=3, GroupName="Parameters")]
		public int Range
		{
			get { return range; }
			set { range = Math.Max(1, value); }
		}		

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="SMIEMAPeriod", Description="SMI EMA smoothing period", Order=4, GroupName="Parameters")]
		public int SMIEMAPeriod
		{
			get { return smiemaperiod; }
			set { smiemaperiod = Math.Max(1, value); }
		}


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> smi
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SMIEMA
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SMIDiff
		{
			get { return Values[2]; }
		}		

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SMI[] cacheSMI;
		public SMI SMI(int eMAPeriod1, int eMAPeriod2, int range, int sMIEMAPeriod)
		{
			return SMI(Input, eMAPeriod1, eMAPeriod2, range, sMIEMAPeriod);
		}

		public SMI SMI(ISeries<double> input, int eMAPeriod1, int eMAPeriod2, int range, int sMIEMAPeriod)
		{
			if (cacheSMI != null)
				for (int idx = 0; idx < cacheSMI.Length; idx++)
					if (cacheSMI[idx] != null && cacheSMI[idx].EMAPeriod1 == eMAPeriod1 && cacheSMI[idx].EMAPeriod2 == eMAPeriod2 && cacheSMI[idx].Range == range && cacheSMI[idx].SMIEMAPeriod == sMIEMAPeriod && cacheSMI[idx].EqualsInput(input))
						return cacheSMI[idx];
			return CacheIndicator<SMI>(new SMI(){ EMAPeriod1 = eMAPeriod1, EMAPeriod2 = eMAPeriod2, Range = range, SMIEMAPeriod = sMIEMAPeriod }, input, ref cacheSMI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMI SMI(int eMAPeriod1, int eMAPeriod2, int range, int sMIEMAPeriod)
		{
			return indicator.SMI(Input, eMAPeriod1, eMAPeriod2, range, sMIEMAPeriod);
		}

		public Indicators.SMI SMI(ISeries<double> input , int eMAPeriod1, int eMAPeriod2, int range, int sMIEMAPeriod)
		{
			return indicator.SMI(input, eMAPeriod1, eMAPeriod2, range, sMIEMAPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMI SMI(int eMAPeriod1, int eMAPeriod2, int range, int sMIEMAPeriod)
		{
			return indicator.SMI(Input, eMAPeriod1, eMAPeriod2, range, sMIEMAPeriod);
		}

		public Indicators.SMI SMI(ISeries<double> input , int eMAPeriod1, int eMAPeriod2, int range, int sMIEMAPeriod)
		{
			return indicator.SMI(input, eMAPeriod1, eMAPeriod2, range, sMIEMAPeriod);
		}
	}
}

#endregion
