//add plot color rename T3Color
//+----------------------------------------------------------------------------------------------+
//| Copyright © <2017>  <LizardIndicators.com - powered by AlderLab UG>
//
//| This program is free software: you can redistribute it and/or modify
//| it under the terms of the GNU General Public License as published by
//| the Free Software Foundation, either version 3 of the License, or
//| any later version.
//|
//| This program is distributed in the hope that it will be useful,
//| but WITHOUT ANY WARRANTY; without even the implied warranty of
//| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//| GNU General Public License for more details.
//|
//| By installing this software you confirm acceptance of the GNU
//| General Public License terms. You may find a copy of the license
//| here; http://www.gnu.org/licenses/
//+----------------------------------------------------------------------------------------------+

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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The Tillson-T3 is a non-linear 6-pole Kaiman filter. Tim Tillson had published the formula in the article 'Smoothing Techniques for More Accurate Signals', 
	/// which was published in the January 1998 issue of Technical Analysis of Stocks and Commodities. The Fulks-Matulich version of the Tillson T3 has the lookback period
	/// adjusted to make it comparable with a standard exponential moving average.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 0)]
	[Gui.CategoryOrder("Data Series", 20)]
	[Gui.CategoryOrder("Set up", 30)]
	[Gui.CategoryOrder("Visual", 40)]
	[Gui.CategoryOrder("Plots", 50)]
	[Gui.CategoryOrder("Version", 80)]
	public class T3Color : Indicator
	{
        private int 				period 						= 21;
		private int					smooth						= 21;
		private double				vFactor						= 0.05;
		private double				coef1						= 0.0;
		private double				coef2						= 0.0;
		private double				coef3						= 0.0;
		private double				coef4						= 0.0;
	//   	private amaT3CalcMode	 	calcMode 					= amaT3CalcMode.Tillson;
		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1.0  -  Jan 13, 2018";
		private	EMA					ema1;
		private	EMA					ema2;
		private	EMA					ema3;
		private	EMA					ema4;
		private	EMA					ema5;
		private	EMA					ema6;
private bool		showPaintBars	= true;				
			private Brush region1Color 	= Brushes.LimeGreen;
			private Brush region2Color 	= Brushes.Red;
			private Brush region3Color 	= Brushes.Yellow;
			
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\nThe Tillson-T3 is a non-linear 6-pole Kaiman filter. Tim Tillson had published the formula in the article 'Smoothing Techniques for More Accurate Signals', "
												+ "which was published in the January 1998 issue of Technical Analysis of Stocks and Commodities. The Fulks-Matulich version of the Tillson T3 has the lookback period "
												+ " adjusted to make it comparable with a standard exponential moving average.";
				Name						= "T3Color";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.MediumBlue, 2), PlotStyle.Line, "Tillson T3");	
			}
			else if (State == State.Configure)
			{
//				BarsRequiredToPlot	= 6 * smooth;
			}
			else if (State == State.DataLoaded)
			{

			smooth = period;
				ema1 = EMA(Input, smooth);
				ema2 = EMA(ema1, smooth);
				ema3 = EMA(ema2, smooth);
				ema4 = EMA(ema3, smooth);
				ema5 = EMA(ema4, smooth);
				ema6 = EMA(ema5, smooth);
			}	
			else if (State == State.Historical)
			{
				coef1 = - Math.Pow(vFactor, 3);
				coef2 = 3 * Math.Pow(vFactor, 2) + 3 * Math.Pow(vFactor, 3);
				coef3 = - 3 * vFactor - 6 * Math.Pow(vFactor, 2) - 3 * Math.Pow(vFactor, 3);
				coef4 = 1 + 3 * vFactor + 3 * Math.Pow(vFactor, 2) + Math.Pow(vFactor, 3);			
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar <= smooth)
				T3[0] = (Input[0]);
			else
			    T3[0] = coef1 * ema6[0] + coef2 * ema5[0] + coef3 * ema4[0] + coef4 * ema3[0];
			
if (CurrentBar > 1)
{
//	Print (CurrentBar+ " "+ T3[1]+ " "+ T3[0]);
		if (T3[1] < T3[0])
		{
			PlotBrushes[0][0] = region1Color;
			if(showPaintBars)
			{
			BarBrushes[0] = region1Color;
			CandleOutlineBrushes[0] = region1Color;
			}
		}
		else if (T3[1] > T3[0])
		{
			PlotBrushes[0][0] = region2Color;
			if(showPaintBars)
			{
			BarBrushes[0] = region2Color;
			CandleOutlineBrushes[0] = region2Color;
			}
		}
		else
		{
			PlotBrushes[0][0] = region3Color;
			if(showPaintBars)
			{
			BarBrushes[0] = region3Color;
			CandleOutlineBrushes[0] = region3Color;
			}
		}
}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> T3
		{
			get { return Values[0]; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show paint bars", GroupName = "Display Options", Order = 2)]
        public bool ShowPaintBars
        {
            get { return showPaintBars; }
            set { showPaintBars = value; }
        }
			
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", Description = "Tillson cacl, smooth = period", GroupName = "Input Parameters", Order = 1)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
		}
			
		[Range(0,2), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "V-Factor", Description = "Please select values between 0 and 2", GroupName = "Input Parameters", Order = 2)]
		public double VFactor
		{	
            get { return vFactor; }
            set { vFactor = value; }
		}
			
				
		[XmlIgnore]	
		[Display(Name = "Raising Color", Description = "Color for 1st region", GroupName = "Plot Color", Order = 1)]
        public Brush Region1Color
        {
            get { return region1Color; }
            set { region1Color = value; }
        }
		
		[Browsable(false)]
		public string Region1ColorSerialize
		{
			get { return Serialize.BrushToString(region1Color); }
			set { region1Color = Serialize.StringToBrush(value); }
		}
			
		[XmlIgnore]
		[Display(Name = "Falling Color", Description = "Color for 2nd region", GroupName = "Plot Color", Order = 2)]
        public Brush Region2Color
        {
            get { return region2Color; }
            set { region2Color = value; }
        }
		
		[Browsable(false)]
		public string Region2ColorSerialize
		{
			get { return Serialize.BrushToString(region2Color); }
			set { region2Color = Serialize.StringToBrush(value); }
		}
			
		[XmlIgnore]
		[Display(Name = "Neutral Color", Description = "Color for 3rd region", GroupName = "Plot Color", Order = 3)]
        public Brush Region3Color
        {
            get { return region3Color; }
            set { region3Color = value; }
        }
		
		[Browsable(false)]
		public string Region3ColorSerialize
		{
			get { return Serialize.BrushToString(region3Color); }
			set { region3Color = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}		
		#endregion
		
		#region Miscellaneous

		public override string FormatPriceMarker(double price)
		{
			if(indicatorIsOnPricePanel)
				return Instrument.MasterInstrument.FormatPrice(Instrument.MasterInstrument.RoundToTickSize(price));
			else
				return base.FormatPriceMarker(price);
		}			
		#endregion
	}
}

//#region Global Enums

//public enum amaT3CalcMode {Tillson, Fulks_Matulich}

//#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private T3Color[] cacheT3Color;
		public T3Color T3Color(int period, double vFactor)
		{
			return T3Color(Input, period, vFactor);
		}

		public T3Color T3Color(ISeries<double> input, int period, double vFactor)
		{
			if (cacheT3Color != null)
				for (int idx = 0; idx < cacheT3Color.Length; idx++)
					if (cacheT3Color[idx] != null && cacheT3Color[idx].Period == period && cacheT3Color[idx].VFactor == vFactor && cacheT3Color[idx].EqualsInput(input))
						return cacheT3Color[idx];
			return CacheIndicator<T3Color>(new T3Color(){ Period = period, VFactor = vFactor }, input, ref cacheT3Color);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.T3Color T3Color(int period, double vFactor)
		{
			return indicator.T3Color(Input, period, vFactor);
		}

		public Indicators.T3Color T3Color(ISeries<double> input , int period, double vFactor)
		{
			return indicator.T3Color(input, period, vFactor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.T3Color T3Color(int period, double vFactor)
		{
			return indicator.T3Color(Input, period, vFactor);
		}

		public Indicators.T3Color T3Color(ISeries<double> input , int period, double vFactor)
		{
			return indicator.T3Color(input, period, vFactor);
		}
	}
}

#endregion
