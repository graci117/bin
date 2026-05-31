//+----------------------------------------------------------------------------------------------+
//| Copyright © <2020>  <LizardIndicators.com - powered by AlderLab UG>
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
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Double Weighted Moving Average (DHMA) is calculated in a similar way as the Double Exponential Moving Average (DEMA), but has the exponential moving averages replaced with weighted moving averages.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Version", 8000100)]
	public class amaDHMA : Indicator
	{
		private Series<double> baseline;
        private Series<double> devUpper;
        private Series<double> devLower;
        private Series<double> cprice;
        private Series<double> priceMA;
        private Series<double> ZLMA;
        private bool up;
        private bool dn;
        private int length = 8; // Default values
        private int smooth = 5;
        private double mult = 0.3;
        private int sd_len = 5;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Double Weighted Moving Average (DHMA) is calculated in a similar way as the Double Exponential Moving Average (DEMA),"
												+ " but has the exponential moving averages replaced with weighted moving averages.";
				Name						= "amaDHMA";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				AddPlot(new Stroke(Brushes.DarkOrange, 3), PlotStyle.Line, "DHMA");	
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = 2 * length;
			}
			else if (State == State.DataLoaded)
			{
				baseline = new Series<double>(this);
                devUpper = new Series<double>(this);
                devLower = new Series<double>(this);
                cprice = new Series<double>(this);
                priceMA = new Series<double>(this);
                ZLMA = new Series<double>(this);
				
			}	
			else if (State == State.Historical)
			{
				
			}	
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar <20)
				return;
			
           	baseline[0] = WMA(Close, sd_len).Value[0];
            devUpper[0] = baseline[0] + mult * StdDev(Close, sd_len).Value[0];
            devLower[0] = baseline[0] - mult * StdDev(Close, sd_len).Value[0];
            cprice[0] = Close[0] > devUpper[0] ? devUpper[0] : Close[0] < devLower[0] ? devLower[0] : Close[0];
            priceMA[0] = HMA(HMA(cprice, length).Value, smooth).Value[0];
            Value[0] = 2 * priceMA[0] - HMA(priceMA, length).Value[0];
			
			//Value[0] = ZLMA[0];

            up = Value[0] > Value[1];
            dn = Value[0] < Value[1];

            PlotBrushes[0][0] = up ? Brushes.Blue : Brushes.Red;
          
		}

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DHMA
		{
			get { return Values[0]; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "Input Parameters", Order = 0)]
		public int Period
		{	
            get { return length; }
            set { length = value; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smooth", GroupName = "Input Parameters", Order = 0)]
		public int Smooth
		{	
            get { return smooth; }
            set { smooth = value; }
		}
		
		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Multiplier", GroupName = "Input Parameters", Order = 0)]
		public double Multiplier
		{	
            get { return mult; }
            set { mult = value; }
		}
		
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "StdDev Length", GroupName = "Input Parameters", Order = 0)]
		public int StdDevLength
		{	
            get { return sd_len; }
            set { sd_len = value; }
		}
			
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaDHMA[] cacheamaDHMA;
		public LizardIndicators.amaDHMA amaDHMA(int period, int smooth, double multiplier, int stdDevLength)
		{
			return amaDHMA(Input, period, smooth, multiplier, stdDevLength);
		}

		public LizardIndicators.amaDHMA amaDHMA(ISeries<double> input, int period, int smooth, double multiplier, int stdDevLength)
		{
			if (cacheamaDHMA != null)
				for (int idx = 0; idx < cacheamaDHMA.Length; idx++)
					if (cacheamaDHMA[idx] != null && cacheamaDHMA[idx].Period == period && cacheamaDHMA[idx].Smooth == smooth && cacheamaDHMA[idx].Multiplier == multiplier && cacheamaDHMA[idx].StdDevLength == stdDevLength && cacheamaDHMA[idx].EqualsInput(input))
						return cacheamaDHMA[idx];
			return CacheIndicator<LizardIndicators.amaDHMA>(new LizardIndicators.amaDHMA(){ Period = period, Smooth = smooth, Multiplier = multiplier, StdDevLength = stdDevLength }, input, ref cacheamaDHMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaDHMA amaDHMA(int period, int smooth, double multiplier, int stdDevLength)
		{
			return indicator.amaDHMA(Input, period, smooth, multiplier, stdDevLength);
		}

		public Indicators.LizardIndicators.amaDHMA amaDHMA(ISeries<double> input , int period, int smooth, double multiplier, int stdDevLength)
		{
			return indicator.amaDHMA(input, period, smooth, multiplier, stdDevLength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaDHMA amaDHMA(int period, int smooth, double multiplier, int stdDevLength)
		{
			return indicator.amaDHMA(Input, period, smooth, multiplier, stdDevLength);
		}

		public Indicators.LizardIndicators.amaDHMA amaDHMA(ISeries<double> input , int period, int smooth, double multiplier, int stdDevLength)
		{
			return indicator.amaDHMA(input, period, smooth, multiplier, stdDevLength);
		}
	}
}

#endregion
