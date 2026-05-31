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
	public class HMAHooks : Indicator
	{
		
		private HMA hma;
		public Series<double> trend;
		
		public override string DisplayName
		{			 
			get { if  (State == State.SetDefaults) return "HMAHooks"; else if (!ShowLabel) return ""; else return "HMAHooks";  }
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "HMAHooks";
				Calculate									= Calculate.OnPriceChange;
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
				
				Period			= 14;
				ArrowOffset		= 0;
				AudioAlert		= false;
				ShowLabel		= false;
				ColorChange		= true;
				UpBrush			= Brushes.Lime;
				DownBrush		= Brushes.Red;
				
				AddPlot(Brushes.White, "HMA");
			}
			else if (State == State.Configure)
			{
				trend = new Series<double>(this);
			}
			else if (State == State.DataLoaded)
			{
				hma = HMA(Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < Period)
				return;
			
			Value[0] = hma[0];
			
			trend[0] = IsRising(hma) ? 1 : -1;
			
			if(Close[0] > hma[0] && trend[1] == -1 && trend[0] == 1)
			{
				int candleOffset = Close[1] > hma[1] ? 1 : 0;
				Draw.ArrowUp(this, "LE " + CurrentBar, false, candleOffset, Low[candleOffset] - ArrowOffset*TickSize, Brushes.Lime);
				if(AudioAlert)
					PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav");	
			}
			if(Close[0] < hma[0] && trend[1] == 1 && trend[0] == -1)
			{
				int candleOffset = Close[1] < hma[1] ? 1 : 0;
				Draw.ArrowDown(this, "SE " + CurrentBar, false, candleOffset, High[candleOffset] + ArrowOffset*TickSize, Brushes.Red);
				if(AudioAlert)
					PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav");	
			}
			
			if(ColorChange)
				PlotBrushes[0][0] = IsRising(hma) ? UpBrush : DownBrush;
		}
		
		/*
			2. With the option to plot an arrow with a number of ticks offset option.
			3. To have the option to turn on or off an audio alert. 
			6. The arrow to plot above or below the FIRST candle that closes above or below the HMA hook. (See chart example) 
		*/
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(int.MinValue, int.MaxValue)]
		[Display(Name="ArrowOffset", Order=2, GroupName="Parameters")]
		public int ArrowOffset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="AudioAlert", Order=3, GroupName="Parameters")]
		public bool AudioAlert
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ShowLabel", Order=4, GroupName="Parameters")]
		public bool ShowLabel
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ColorChange", Order=5, GroupName="Parameters")]
		public bool ColorChange
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="UpBrush", Order=6, GroupName="Parameters")]
		public Brush UpBrush
		{ get; set; }

		[Browsable(false)]
		public string UpBrushSerializable
		{
			get { return Serialize.BrushToString(UpBrush); }
			set { UpBrush = Serialize.StringToBrush(value); }
		}	
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DownBrush", Order=7, GroupName="Parameters")]
		public Brush DownBrush
		{ get; set; }

		[Browsable(false)]
		public string DownBrushSerializable
		{
			get { return Serialize.BrushToString(DownBrush); }
			set { DownBrush = Serialize.StringToBrush(value); }
		}	
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HMAHooks[] cacheHMAHooks;
		public HMAHooks HMAHooks(int period, int arrowOffset, bool audioAlert, bool showLabel, bool colorChange, Brush upBrush, Brush downBrush)
		{
			return HMAHooks(Input, period, arrowOffset, audioAlert, showLabel, colorChange, upBrush, downBrush);
		}

		public HMAHooks HMAHooks(ISeries<double> input, int period, int arrowOffset, bool audioAlert, bool showLabel, bool colorChange, Brush upBrush, Brush downBrush)
		{
			if (cacheHMAHooks != null)
				for (int idx = 0; idx < cacheHMAHooks.Length; idx++)
					if (cacheHMAHooks[idx] != null && cacheHMAHooks[idx].Period == period && cacheHMAHooks[idx].ArrowOffset == arrowOffset && cacheHMAHooks[idx].AudioAlert == audioAlert && cacheHMAHooks[idx].ShowLabel == showLabel && cacheHMAHooks[idx].ColorChange == colorChange && cacheHMAHooks[idx].UpBrush == upBrush && cacheHMAHooks[idx].DownBrush == downBrush && cacheHMAHooks[idx].EqualsInput(input))
						return cacheHMAHooks[idx];
			return CacheIndicator<HMAHooks>(new HMAHooks(){ Period = period, ArrowOffset = arrowOffset, AudioAlert = audioAlert, ShowLabel = showLabel, ColorChange = colorChange, UpBrush = upBrush, DownBrush = downBrush }, input, ref cacheHMAHooks);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HMAHooks HMAHooks(int period, int arrowOffset, bool audioAlert, bool showLabel, bool colorChange, Brush upBrush, Brush downBrush)
		{
			return indicator.HMAHooks(Input, period, arrowOffset, audioAlert, showLabel, colorChange, upBrush, downBrush);
		}

		public Indicators.HMAHooks HMAHooks(ISeries<double> input , int period, int arrowOffset, bool audioAlert, bool showLabel, bool colorChange, Brush upBrush, Brush downBrush)
		{
			return indicator.HMAHooks(input, period, arrowOffset, audioAlert, showLabel, colorChange, upBrush, downBrush);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HMAHooks HMAHooks(int period, int arrowOffset, bool audioAlert, bool showLabel, bool colorChange, Brush upBrush, Brush downBrush)
		{
			return indicator.HMAHooks(Input, period, arrowOffset, audioAlert, showLabel, colorChange, upBrush, downBrush);
		}

		public Indicators.HMAHooks HMAHooks(ISeries<double> input , int period, int arrowOffset, bool audioAlert, bool showLabel, bool colorChange, Brush upBrush, Brush downBrush)
		{
			return indicator.HMAHooks(input, period, arrowOffset, audioAlert, showLabel, colorChange, upBrush, downBrush);
		}
	}
}

#endregion
