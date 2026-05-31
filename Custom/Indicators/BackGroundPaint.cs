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
	public class BackGroundPaint : Indicator
	{
		private BltTriggerLines BltTriggerLines1;
		private BltTriggerLines BltTriggerLines2;
		private Brush UpColor;
		private Brush DownColor;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BackGroundPaint";
				Calculate									= Calculate.OnBarClose;
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
					UpColor  = new SolidColorBrush(Color.FromArgb(50, 25, 175,0));
				DownColor  = new SolidColorBrush(Color.FromArgb(50, 220, 20,0));
			}
			else if (State == State.Configure)
			{
					Brush temp3 = UpColor.Clone();
					temp3.Opacity = 40 / 100.0;
					temp3.Freeze();
					UpColor = temp3;
					
					Brush temp4 = DownColor.Clone();
					temp4.Opacity = 40 / 100.0;
					temp4.Freeze();
					DownColor = temp4;	
			}
			else if (State == State.DataLoaded)
			{
			    BltTriggerLines1				= BltTriggerLines(Close, bltTriggerLines.Common.MAType.LinReg, 20, bltTriggerLines.Common.MAType.EMA, 6, bltTriggerLines.Common.ColorStyle.RegionColors, false, 30, Brushes.Blue, Brushes.Red, false, 30, Brushes.Lime, Brushes.DarkRed, false, @"Alert4.wav", Brushes.Lime, Brushes.Yellow, Brushes.Aqua, Brushes.Red);
				BltTriggerLines2				= BltTriggerLines(Close, bltTriggerLines.Common.MAType.LinReg, 38, bltTriggerLines.Common.MAType.EMA, 8, bltTriggerLines.Common.ColorStyle.RegionColors, false, 30, Brushes.Blue, Brushes.Red, false, 30, Brushes.Aqua, Brushes.White, false, @"Alert4.wav", Brushes.Lime, Brushes.Yellow, Brushes.Aqua, Brushes.Red);
				BltTriggerLines1.Plots[0].Brush = Brushes.Green;
				BltTriggerLines1.Plots[1].Brush = Brushes.Red;
				BltTriggerLines2.Plots[0].Brush = Brushes.Green;
				BltTriggerLines2.Plots[1].Brush = Brushes.Red;
				
		
				
			
			}
		}
		
		

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 6)
				return;

			 // Set 1
			if (BltTriggerLines1.Trigger[1] > BltTriggerLines2.Trigger[6])
			{
				BackBrushAll = UpColor;
				//BackBrushAll.Opacity = 25 / 100.0;
			}
			
			 // Set 2
			if (BltTriggerLines1.Trigger[1] < BltTriggerLines2.Trigger[6])
			{
				BackBrushAll = DownColor;
				//BackBrushAll.Opacity = 70 / 100.0;
			}
			
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BackGroundPaint[] cacheBackGroundPaint;
		public BackGroundPaint BackGroundPaint()
		{
			return BackGroundPaint(Input);
		}

		public BackGroundPaint BackGroundPaint(ISeries<double> input)
		{
			if (cacheBackGroundPaint != null)
				for (int idx = 0; idx < cacheBackGroundPaint.Length; idx++)
					if (cacheBackGroundPaint[idx] != null &&  cacheBackGroundPaint[idx].EqualsInput(input))
						return cacheBackGroundPaint[idx];
			return CacheIndicator<BackGroundPaint>(new BackGroundPaint(), input, ref cacheBackGroundPaint);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BackGroundPaint BackGroundPaint()
		{
			return indicator.BackGroundPaint(Input);
		}

		public Indicators.BackGroundPaint BackGroundPaint(ISeries<double> input )
		{
			return indicator.BackGroundPaint(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BackGroundPaint BackGroundPaint()
		{
			return indicator.BackGroundPaint(Input);
		}

		public Indicators.BackGroundPaint BackGroundPaint(ISeries<double> input )
		{
			return indicator.BackGroundPaint(input);
		}
	}
}

#endregion
