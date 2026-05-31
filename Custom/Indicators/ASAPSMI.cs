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
	
	public class ASAPSMI : Indicator
	{
		private int	smiemaperiod		= 4;

		
		private Series<double>	sms;
		private Series<double>	hls;
		private Series<double>	smis;
		private Series<double>	rng;
		private Series<double>	avgRel;
		private Series<double>	avgDiff;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"The Stochastic Momentum Index is made up of two lines that oscillate between a vertical scale of -100 to 100.";
				Name						= "ASAPSMI";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
				ASAPSMIEMAPeriod			= 4;
				
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "ASAPSMI");
				//AddPlot(Brushes.Orange, "ASAPSMI");
				AddLine(Brushes.DarkGray, 1, "ZeroLine");
				//AddPlot(new Stroke(Brushes.Crimson, 3), PlotStyle.Bar, "ASAPSMI");
				
						//stochastic momentums
			
			}
			else if (State == State.Configure)
			{
				//high low diffs
				hls		= new Series<double>(this);
				//stochastic momentum indexes
				
				
				rng    = new Series<double>(this);
				
				avgRel    = new Series<double>(this);
				
				avgDiff    = new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{	
			
				
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;
			
			
			//Stochastic Momentum = SM {distance of close - midpoint}
			
			//High low diffs
			
			//sms[0] = (Close[0] - 0.5 * ((MAX(High, range)[0] + MIN(Low, range)[0])));
			
			//High low diffs
			//hls[0] = (MAX(High, range)[0] - MIN(Low, range)[0]);
			
			
//			Stochastic Momentum Index = SMI
//			double denom = 0.5*EMA(EMA(hls,emaperiod1),emaperiod2)[0];
// 			smis[0] = (100*(EMA(EMA(sms,emaperiod1),emaperiod2))[0] / (denom ==0 ? 1 : denom  ));
			
//			Set the current SMI line value
//			smi[0] = (smis[0]);
			
//			Set the line value for the SMIEMA by taking the EMA of the SMI
//			SMIEMA[0] = (EMA(smis, smiemaperiod)[0]);
			
//			SMIDiff[0] = smi[0] - SMIEMA[0];
			
			
			hls[0] = (Close[0] - 0.5 * ((MAX(High, smiemaperiod)[0] + MIN(Low, smiemaperiod)[0])));
			
			rng[0] = (MAX(High, smiemaperiod)[0] - MIN(Low, smiemaperiod)[0]);
			
			avgRel[0] = (EMA(EMA(hls,smiemaperiod),smiemaperiod))[0];
			
			avgDiff[0] = (EMA(EMA(rng,smiemaperiod),smiemaperiod))[0];

		
			
			//Set the current SMI line value
			smi[0] = avgDiff[0] != 0 ? avgRel[0]/(avgDiff[0]/2) *100 : 0;
			
			if(IsRising(Value)) {PlotBrushes[0][0] = Brushes.Cyan;}
			else if(IsFalling(Value)) {PlotBrushes[0][0] = Brushes.Magenta;}
			else {PlotBrushes[0][0] = Brushes.Yellow;}

		}

		#region Properties
		
		
	

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="ASAPSMI", Description="ASAPSMI", Order=4, GroupName="Parameters")]
		public int ASAPSMIEMAPeriod
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

	

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ASAPSMI[] cacheASAPSMI;
		public ASAPSMI ASAPSMI(int aSAPSMIEMAPeriod)
		{
			return ASAPSMI(Input, aSAPSMIEMAPeriod);
		}

		public ASAPSMI ASAPSMI(ISeries<double> input, int aSAPSMIEMAPeriod)
		{
			if (cacheASAPSMI != null)
				for (int idx = 0; idx < cacheASAPSMI.Length; idx++)
					if (cacheASAPSMI[idx] != null && cacheASAPSMI[idx].ASAPSMIEMAPeriod == aSAPSMIEMAPeriod && cacheASAPSMI[idx].EqualsInput(input))
						return cacheASAPSMI[idx];
			return CacheIndicator<ASAPSMI>(new ASAPSMI(){ ASAPSMIEMAPeriod = aSAPSMIEMAPeriod }, input, ref cacheASAPSMI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ASAPSMI ASAPSMI(int aSAPSMIEMAPeriod)
		{
			return indicator.ASAPSMI(Input, aSAPSMIEMAPeriod);
		}

		public Indicators.ASAPSMI ASAPSMI(ISeries<double> input , int aSAPSMIEMAPeriod)
		{
			return indicator.ASAPSMI(input, aSAPSMIEMAPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ASAPSMI ASAPSMI(int aSAPSMIEMAPeriod)
		{
			return indicator.ASAPSMI(Input, aSAPSMIEMAPeriod);
		}

		public Indicators.ASAPSMI ASAPSMI(ISeries<double> input , int aSAPSMIEMAPeriod)
		{
			return indicator.ASAPSMI(input, aSAPSMIEMAPeriod);
		}
	}
}

#endregion
