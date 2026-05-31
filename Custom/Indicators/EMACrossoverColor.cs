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
	public class EMACrossoverColor : Indicator
	{
		private EMA EMA1;
		private EMA EMA2;
		private EMA EMA3;
		//private Brush fastBrush;
		//private Brush slowBrush;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "EMACrossoverColor";
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
				FastEMALength					= 5;
				SlowEMALength					= 8;
				//fastBrush						= Brushes.LightGreen;
				//slowBrush						= Brushes.Tomato;
			}
			else if (State == State.Configure)
			{
				//Brush temp = fastBrush.Clone();
					//temp.Opacity = 0.35;
					//temp.Freeze();
					//fastBrush = temp;
					
					//Brush temp1 = slowBrush.Clone();
					//temp1.Opacity = 0.35;
					//temp1.Freeze();
					//slowBrush = temp1;	
			}
			else if (State == State.DataLoaded)
			{			
				
				EMA1				= EMA(Close, Convert.ToInt32(8));
				EMA2				= EMA(Close, Convert.ToInt32(5));
				EMA3				= EMA(Close, Convert.ToInt32(5));
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 0)
				return;

			if (EMA1[0] < EMA2[0])
			{
				
				//BackBrush = Brushes.DarkGreen;
//				BackBrush.Opacity = 35/100;
//				BackBrush.Freeze();
				//new SolidColorBrush(Colors.DarkGreen) {Opacity = 0.25};
				//BackBrush = fastBrush;
				BackBrush = new SolidColorBrush(Color.FromArgb(50, 25, 175, 185));
			}
			
			 // Set 2
			if (EMA1[0] > EMA2[0])
			{
				//BackBrush = Brushes.Crimson;
//				BackBrush.Opacity = 35/100;
//				BackBrush.Freeze();
				//new SolidColorBrush(Colors.Crimson) {Opacity = 0.25};
				//BackBrush = slowBrush;
				BackBrush = new SolidColorBrush(Color.FromArgb(50, 220, 20, 60));
			}
			
			 // Set 3
			if (EMA1[0] == EMA2[0])
			{
				BackBrush = Brushes.Transparent;
//				BackBrush.Opacity = 35/100;
//				BackBrush.Freeze();
				//new SolidColorBrush(Colors.Transparent) {Opacity = 0.25};
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastEMALength", Order=1, GroupName="Parameters")]
		public int FastEMALength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowEMALength", Order=2, GroupName="Parameters")]
		public int SlowEMALength
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EMACrossoverColor[] cacheEMACrossoverColor;
		public EMACrossoverColor EMACrossoverColor(int fastEMALength, int slowEMALength)
		{
			return EMACrossoverColor(Input, fastEMALength, slowEMALength);
		}

		public EMACrossoverColor EMACrossoverColor(ISeries<double> input, int fastEMALength, int slowEMALength)
		{
			if (cacheEMACrossoverColor != null)
				for (int idx = 0; idx < cacheEMACrossoverColor.Length; idx++)
					if (cacheEMACrossoverColor[idx] != null && cacheEMACrossoverColor[idx].FastEMALength == fastEMALength && cacheEMACrossoverColor[idx].SlowEMALength == slowEMALength && cacheEMACrossoverColor[idx].EqualsInput(input))
						return cacheEMACrossoverColor[idx];
			return CacheIndicator<EMACrossoverColor>(new EMACrossoverColor(){ FastEMALength = fastEMALength, SlowEMALength = slowEMALength }, input, ref cacheEMACrossoverColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EMACrossoverColor EMACrossoverColor(int fastEMALength, int slowEMALength)
		{
			return indicator.EMACrossoverColor(Input, fastEMALength, slowEMALength);
		}

		public Indicators.EMACrossoverColor EMACrossoverColor(ISeries<double> input , int fastEMALength, int slowEMALength)
		{
			return indicator.EMACrossoverColor(input, fastEMALength, slowEMALength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EMACrossoverColor EMACrossoverColor(int fastEMALength, int slowEMALength)
		{
			return indicator.EMACrossoverColor(Input, fastEMALength, slowEMALength);
		}

		public Indicators.EMACrossoverColor EMACrossoverColor(ISeries<double> input , int fastEMALength, int slowEMALength)
		{
			return indicator.EMACrossoverColor(input, fastEMALength, slowEMALength);
		}
	}
}

#endregion
