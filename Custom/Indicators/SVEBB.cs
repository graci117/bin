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
	public class SVEBB : Indicator
	{
		private Series<double> haOpen;
		private Series<double> haC;
		private Series<double> TMA1;
		private Series<double> TMA2;
		private Series<double> ZLHA;
		private double diff;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"This indicator is a modification of the Bollinger Band indicator as described in the May 2010 issue of Stocks & Commodities.";
				Name										= "SVEBB";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				Period										= 18;
				TeAv										= 8;
				Afwh										= 1.6;
				Afwl										= 1.6;
				Afwper										= 63;
				AddPlot(Brushes.Red, "B_Upper");
				AddPlot(Brushes.Red, "B_Lower");
				AddPlot(Brushes.DodgerBlue, "B_Plot");
				AddLine(Brushes.Red, 50, "MiddleLine");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				haOpen = new Series<double>(this);
				haC = new Series<double>(this);
				TMA1 = new Series<double>(this);
				TMA2 = new Series<double>(this);
				ZLHA = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 1)
				return;

			if (CurrentBar == 2)
				haOpen[0] = (Open[1] + Close[1] + High[1] + Low[1]) / 4;
			else
				haOpen[0] = (((Open[1] + Close[1] + High[1] + Low[1]) / 4) + haOpen[1]) / 2;
			
			haC[0] = ((Open[0] + Close[0] + High[0] + Low[0]) / 4 + haOpen[0] + Math.Max(High[0], haOpen[0]) + Math.Min(Low[0], haOpen[0])) / 4;
            TMA1[0] = TEMA(haC, TeAv)[0];
			TMA2[0] = TEMA(TMA1, TeAv)[0];
			diff = TMA1[0] - TMA2[0];
			ZLHA[0] = TMA1[0] + diff;
			
			B_Plot[0] = (TEMA(ZLHA, TeAv)[0] + 2 * StdDev(TEMA(ZLHA, TeAv), Period)[0] - WMA(TEMA(ZLHA, TeAv), Period)[0]) / (4 * StdDev(TEMA(ZLHA, TeAv), Period)[0]) * 100;
			B_Upper[0] = 50 + Afwh * StdDev(B_Plot, Afwper)[0];
            B_Lower[0] = 50 - Afwh * StdDev(B_Plot, Afwper)[0];
            
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="%b period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TeAv", Description="Tema Average", Order=2, GroupName="Parameters")]
		public int TeAv
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Afwh", Description="Standard deviation high", Order=3, GroupName="Parameters")]
		public double Afwh
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Afwl", Description="Standard deviation low", Order=4, GroupName="Parameters")]
		public double Afwl
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Afwper", Description="Standard deviation period", Order=5, GroupName="Parameters")]
		public int Afwper
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> B_Upper
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> B_Lower
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> B_Plot
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
		private SVEBB[] cacheSVEBB;
		public SVEBB SVEBB(int period, int teAv, double afwh, double afwl, int afwper)
		{
			return SVEBB(Input, period, teAv, afwh, afwl, afwper);
		}

		public SVEBB SVEBB(ISeries<double> input, int period, int teAv, double afwh, double afwl, int afwper)
		{
			if (cacheSVEBB != null)
				for (int idx = 0; idx < cacheSVEBB.Length; idx++)
					if (cacheSVEBB[idx] != null && cacheSVEBB[idx].Period == period && cacheSVEBB[idx].TeAv == teAv && cacheSVEBB[idx].Afwh == afwh && cacheSVEBB[idx].Afwl == afwl && cacheSVEBB[idx].Afwper == afwper && cacheSVEBB[idx].EqualsInput(input))
						return cacheSVEBB[idx];
			return CacheIndicator<SVEBB>(new SVEBB(){ Period = period, TeAv = teAv, Afwh = afwh, Afwl = afwl, Afwper = afwper }, input, ref cacheSVEBB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SVEBB SVEBB(int period, int teAv, double afwh, double afwl, int afwper)
		{
			return indicator.SVEBB(Input, period, teAv, afwh, afwl, afwper);
		}

		public Indicators.SVEBB SVEBB(ISeries<double> input , int period, int teAv, double afwh, double afwl, int afwper)
		{
			return indicator.SVEBB(input, period, teAv, afwh, afwl, afwper);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SVEBB SVEBB(int period, int teAv, double afwh, double afwl, int afwper)
		{
			return indicator.SVEBB(Input, period, teAv, afwh, afwl, afwper);
		}

		public Indicators.SVEBB SVEBB(ISeries<double> input , int period, int teAv, double afwh, double afwl, int afwper)
		{
			return indicator.SVEBB(input, period, teAv, afwh, afwl, afwper);
		}
	}
}

#endregion
