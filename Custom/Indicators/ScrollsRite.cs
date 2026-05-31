//
// Copyright (C) 2018, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// 
	///  	ScrollsRite  For NT8 -- "TikDaddy" futures.io
	/// 
	/// 	Enables NinjaTrader 8 user to scroll charts any direction with a mouse.  
	/// 
	/// 	Based on NinjaTrader 7 code "ScrollsRite" fully rewritten for NinjaTrader 8.
	/// 	Originally based on code "ScrollsLite" by user "devdas" 	
	/// 
	/// 	Note: If the Price Scale has been moved to the left side of the chart 
	/// 	then Scrolls8 must also be configured for the left side also which is not intuitive.
	/// 
	/// 	V1.0 Jan 18 2019	Initial release.
	/// 	V1.1 Jan 22 2019	-Cursor HashCode is not dependable across installations
	/// 						-this.ChartPanel.Cursor can become null.  surprise surprise
	/// 	V1.2 Jan 23 2019 	-Forgot to test for "Hand" cursor.
	/// 						-Test for bars present
	/// 						-revamp _chartScale useage
	/// 
	/// </summary>

	
	public class ScrollsRite : Indicator
	{
		private ChartScale				_chartScale;
		private double      			old_Y,new_Y;
		private double     				scale;
		private bool 					currentlyScrolling = false;
		private bool					scrollingEnabled;
		private double					offset;
		
		protected override void OnStateChange()
		{		
			
			if (State == State.SetDefaults)
			{
				Description					= @"Scroll Chart in any direction with your mouse.  V1.2";
				Name						= "ScrollsRite";
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				IsVisible 					= true;				
			}
		//	else if (State == State.Historical)

			else if (State == State.Terminated)
			{
				// discontinue mouse event handling
				if(scrollingEnabled) EnableScrolling(false);	
			}

		}
				
		protected override void OnBarUpdate() { }

		public override string DisplayName
		{
			// slightly cleaner
			get { return Name;}
		}
	
		protected void OurMouseUp(object sender, RoutedEventArgs rea)
		{		
			currentlyScrolling = false;	
		}

		protected void EnableScrolling(bool enable )
		{
			if(enable)
			{					
				ChartControl.MouseMove += new MouseEventHandler(OurMouseMove);
				ChartControl.MouseUp += new MouseButtonEventHandler(OurMouseUp);
				scrollingEnabled = true;				
			}
			else
			{
				ChartControl.MouseMove -= OurMouseMove;
				ChartControl.MouseUp -= OurMouseUp;	
				scrollingEnabled = false;
			}
			
		}	
		
		
		protected void OurMouseMove(object sender, RoutedEventArgs rea)
		{
			if( Mouse.LeftButton != MouseButtonState.Pressed || this.ChartPanel.Cursor == null || _chartScale == null) return;
			if ( this.ChartPanel.Cursor.ToString() == "Hand")
			{	
				if(!currentlyScrolling )
				{				
					//	gives price change per displayed pixel						
					if(_chartScale.Height == 0) return;				
					scale = _chartScale.MaxMinusMin / _chartScale.Height;	
					old_Y = Mouse.GetPosition(ChartPanel).Y;
					currentlyScrolling = true;					
				}	
				
				new_Y = Mouse.GetPosition(ChartPanel).Y;			
				bool inBounds = new_Y > 0 && new_Y < this.ChartPanel.ActualHeight;
				if(inBounds)
				{	
					offset = ((new_Y - old_Y) * scale);
					old_Y = new_Y;					
				} 
				else currentlyScrolling = false;

			}	
		}
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{     
			_chartScale = chartScale;	
			if (chartControl == null || chartScale == null || Bars == null) 
			{
				// if something suddenly becomes null
				if(scrollingEnabled) EnableScrolling(false);	
				return;
			}
						
			if(currentlyScrolling)
			{
				chartScale.Properties.FixedScaleMax += offset;
				chartScale.Properties.FixedScaleMin += offset;		
			}
			
			base.OnRender(chartControl,chartScale);	
			
			if( !scrollingEnabled && Bars.Count != 0 && chartScale.IsVisible) 
			{
				chartScale.Properties.YAxisRangeType = YAxisRangeType.Automatic;
				EnableScrolling(true);
			}			
		}			
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ScrollsRite[] cacheScrollsRite;
		public ScrollsRite ScrollsRite()
		{
			return ScrollsRite(Input);
		}

		public ScrollsRite ScrollsRite(ISeries<double> input)
		{
			if (cacheScrollsRite != null)
				for (int idx = 0; idx < cacheScrollsRite.Length; idx++)
					if (cacheScrollsRite[idx] != null &&  cacheScrollsRite[idx].EqualsInput(input))
						return cacheScrollsRite[idx];
			return CacheIndicator<ScrollsRite>(new ScrollsRite(), input, ref cacheScrollsRite);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ScrollsRite ScrollsRite()
		{
			return indicator.ScrollsRite(Input);
		}

		public Indicators.ScrollsRite ScrollsRite(ISeries<double> input )
		{
			return indicator.ScrollsRite(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ScrollsRite ScrollsRite()
		{
			return indicator.ScrollsRite(Input);
		}

		public Indicators.ScrollsRite ScrollsRite(ISeries<double> input )
		{
			return indicator.ScrollsRite(input);
		}
	}
}

#endregion
