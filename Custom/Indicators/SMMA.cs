//modify amaEHMA to SMMA -Cory 1/13/2018
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
	/// The Exponential Hull Moving Average is calculated in a similar way as the traditional Hull Moving Average, but has the weighted moving averages replaced with exponential moving averages.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 0)]
	[Gui.CategoryOrder("Data Series", 20)]
	[Gui.CategoryOrder("Set up", 30)]
	[Gui.CategoryOrder("Visual", 40)]
	[Gui.CategoryOrder("Plots", 50)]
	[Gui.CategoryOrder("Version", 80)]
	public class SMMA : Indicator
	{
		private int					period						= 14;
 		private double	smma1	= 0;
		private double	sum1	= 0;
		private double	prevsum1 = 0;
		private double	prevsmma1 = 0;

		private bool				indicatorIsOnPricePanel		= true;
		private string				versionString				= "v 1  -  Jan 13, 2018";

		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n SMMA (Smoothed Moving Average)"
												+ " has a diff calculation from SMA .";
				Name						= "SMMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DarkMagenta, 2), PlotStyle.Line, "SMAA");	
			}
			else if (State == State.Configure)
			{
	//			BarsRequiredToPlot = period;
			}

			else if (State == State.Historical)
			{  
				if(ChartBars != null)
					indicatorIsOnPricePanel = (ChartPanel.PanelIndex == ChartBars.Panel);
				else
					indicatorIsOnPricePanel = false;
			}	
		}
		
		protected override void OnBarUpdate()
		{

			
            if(CurrentBar <= Period)
			{
				sum1 = SUM(Input,Period)[0];
				smma1 = sum1/Period;
				Value[0] =(smma1);
			}
			else //if (CurrentBar > Period)
			{
				if (IsFirstTickOfBar)
				{
					prevsum1 = sum1;
					prevsmma1 = smma1;
				}
				SMAA[0] = ((prevsum1-prevsmma1+Input[0])/Period);
				sum1 = prevsum1-prevsmma1+Input[0];
				smma1 = (sum1-prevsmma1+Input[0])/Period;
			}	
			
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SMAA
		{
			get { return Values[0]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Input Parameters", Order = 0)]
		public int Period
		{	
            get { return period; }
            set { period = value; }
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

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SMMA[] cacheSMMA;
		public SMMA SMMA(int period)
		{
			return SMMA(Input, period);
		}

		public SMMA SMMA(ISeries<double> input, int period)
		{
			if (cacheSMMA != null)
				for (int idx = 0; idx < cacheSMMA.Length; idx++)
					if (cacheSMMA[idx] != null && cacheSMMA[idx].Period == period && cacheSMMA[idx].EqualsInput(input))
						return cacheSMMA[idx];
			return CacheIndicator<SMMA>(new SMMA(){ Period = period }, input, ref cacheSMMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMMA SMMA(int period)
		{
			return indicator.SMMA(Input, period);
		}

		public Indicators.SMMA SMMA(ISeries<double> input , int period)
		{
			return indicator.SMMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMMA SMMA(int period)
		{
			return indicator.SMMA(Input, period);
		}

		public Indicators.SMMA SMMA(ISeries<double> input , int period)
		{
			return indicator.SMMA(input, period);
		}
	}
}

#endregion
