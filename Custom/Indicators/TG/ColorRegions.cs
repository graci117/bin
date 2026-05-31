#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
#endregion





#region NOTES:
/*

07/25/2018
 - bug fix:  NT8 crashes during the process of resetting chart background color back to null (when local time reaches the "End Time").


05/07/2018
 - If you had a previous version installed, please remove it from the "Documents\NinjaTrader 8\bin\Custom\Indicators\downloaded\" folder
	as this version now resides in the "TG" folder/namespace along with the other indicator scripts I've coded.
 - Added an additional "warning" color region to give visual aid to let you know the original time range is about to start/end
 - Optimized some code to reduce overhead


02/28/17
A quick and easy way to conditionally change chart background colors based on the time of day.

This version uses wpf rendering

- "Start Time" can be set to a time that is later in the day than the "End Time".
- Toggle between coloring only the price panel or all panels.
- Manually set the color opacity to your liking.
	0.0 = fully transparent
	1.0 = fully filled


Written by Gubbar924 at NinjaTrader support forum.
http://ninjatrader.com/support/forum/member.php?u=30178
*/
#endregion





namespace NinjaTrader.NinjaScript.Indicators.TG
{



    public class ColorRegions : Indicator
	{
		
		
		#region class variables
		private Brush			warnColor, regColor;
		private TimeSpan 		checkNow, rts1, rts2, wts1, wts2;
		private bool 			RegionCondition 					= false;
		private bool 			WarningCondition 					= false;
        #endregion


		#region OnStateChange()
        protected override void OnStateChange()
		{
			#region State.SetDefaults
			if(State == State.SetDefaults)
			{
				#region properties
				Description					= String.Format(@"A sample script for changing the chart background color based on the time of day.{0}{0}This version includes a secondary color to visually warn you that the time is getting close to your selected start/end time.{0}{0}This script was Written by ""Gubbar924""{0}http://ninjatrader.com/support/forum/member.php?u=30178", Environment.NewLine);
				Name						= "ColorRegions";
				
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				MaximumBarsLookBack 		= MaximumBarsLookBack.TwoHundredFiftySix;
				Calculate					= Calculate.OnEachTick;
				
				IsSuspendedWhileInactive	= false;
				IsOverlay					= true;
				IsAutoScale				 	= false;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= true;
				PaintPriceMarkers			= false;
				#endregion
				
				#region inputs
				PricePanelOnly				= false;
				RegionColor   				= Brushes.DodgerBlue;
				WarningColor   				= Brushes.Yellow;
				RegionOpacity				= .100;
				WarningOpacity				= .100;
				GoTime						= DateTime.Parse("3:00 PM");
				EndTime						= DateTime.Parse("8:30 AM");
				WarningTime					= 15;
				#endregion
			}
			#endregion
			
			#region State.DataLoaded
			else if(State == State.DataLoaded)
			{
				#region prevent indicator name & parameters from showing on chart
				Name 						= "";
				#endregion
				
				#region color instances with opacity
				regColor 		 			= RegionColor.Clone();
				regColor.Opacity 			= this.RegionOpacity;
				
				warnColor 		 			= WarningColor.Clone();
				warnColor.Opacity 			= this.WarningOpacity;
				#endregion
				
				#region give values to timespans
				checkNow			  		= DateTime.Now.TimeOfDay;	//	temporary initial value
				rts1  				  		= GoTime.TimeOfDay;
				rts2  				  		= EndTime.TimeOfDay;
				wts1						= GoTime.AddMinutes(-WarningTime).TimeOfDay;
				wts2						= EndTime.AddMinutes(-WarningTime).TimeOfDay;
				#endregion
			}
			#endregion
			
			#region State.Terminated
			else if(State == State.Terminated)
			{
				#region change background back to normal
				if(BackBrushAll 		   != null)
					BackBrushAll  			= null;
				#endregion
          	}
			#endregion
		}
		#endregion
		
		
		#region OnBarUpdate()
		protected override void OnBarUpdate()
		{
			try
			{
				#region check our resources
				if(ChartControl == null || Bars == null || 
					warnColor == null || regColor == null || checkNow == null || 
					rts1 == null || rts2 == null || wts1 == null || wts2 == null)
				{
					return;
				}
				#endregion
				
				#region update TimeSpan
				checkNow			  			= Time[0].TimeOfDay;
				#endregion
				
				#region timing conditions
				if(rts1 < rts2)
				{
					RegionCondition	  			= ((checkNow >= rts1) && (checkNow <= rts2));
				}
				else
				{
					RegionCondition	  			= ((checkNow >= rts1) || (checkNow <= rts2));
				}
				
				WarningCondition				= (((checkNow >= wts1) && (checkNow < rts1)) || ((checkNow > wts2) && (checkNow <= rts2)));
				#endregion
				
				#region select panels to fill & apply background colors
				if(WarningCondition)
				{
					RegionCondition  = false;
					
					if(PricePanelOnly)
					{
						BackBrush 	 = warnColor;
						BackBrush.Freeze();
					}
					else
					{
						BackBrushAll = warnColor;
						BackBrushAll.Freeze();
					}
				}
				else if(RegionCondition)
				{
					WarningCondition = false;
					
					if(PricePanelOnly)
					{
						BackBrush 	 = regColor;
						BackBrush.Freeze();
					}
					else
					{
						BackBrushAll = regColor;
						BackBrushAll.Freeze();
					}
				}
				else
				{
					if(BackBrush != null)
					{
						BackBrush = null;
//						BackBrush.Freeze();
					}
					
					if(BackBrushAll != null)
					{
						BackBrushAll = null;
//						BackBrushAll.Freeze();
					}
				}
				#endregion
			}
			catch(Exception ex)
			{
				Print(String.Format("{0}.{1} threw an [{2}] \n\t {3} \n\t {4} \r\n ", this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex.GetType(), ex.Message, ex.StackTrace));
			}
		}
		#endregion

		
		#region Properties
		
		#region inputs
		[NinjaScriptProperty]
		[Display(Name="Price Panel Only", Description="     Checked = changes background color on price panel only\r\nUnChecked = changes background color on all panels", Order=0, GroupName="Options")]
		public bool PricePanelOnly
		{ get; set; }
		
		[XmlIgnore]
		[Display(Name = "Region Color", Order=1, GroupName="Options")]
		public Brush RegionColor
		{ get; set; }

		[Browsable(false)]
		public string RegionColorSerialize
		{
			get { return Serialize.BrushToString(RegionColor); }
			set { RegionColor = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(Name = "Warning Color", Order=2, GroupName="Options")]
		public Brush WarningColor
		{ get; set; }

		[Browsable(false)]
		public string WarningColorSerialize
		{
			get { return Serialize.BrushToString(WarningColor); }
			set { WarningColor = Serialize.StringToBrush(value); }
		}
		
		[Range(0.0, 1.0)]
		[NinjaScriptProperty]
		[Display(Name="Region Opacity", Order=3, GroupName="Options")]
		public double RegionOpacity
		{ get; set; }
		
		[Range(0.0, 1.0)]
		[NinjaScriptProperty]
		[Display(Name="Warning Opacity", Order=4, GroupName="Options")]
		public double WarningOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="Start Time", Order=5, GroupName="Options")]
		public DateTime GoTime
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="End Time", Order=6, GroupName="Options")]
		public DateTime EndTime
		{ get; set; }
		
		[Range(0, 60)]
		[NinjaScriptProperty]
		[Display(Name="Warning Time in Minutes", Order=7, GroupName="Options")]
		public double WarningTime
		{ get; set; }
		#endregion
		
		#endregion
		
		
	}
	
	
	
}


























//	This line is just to keep the Ninjascript generated code separate

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TG.ColorRegions[] cacheColorRegions;
		public TG.ColorRegions ColorRegions(bool pricePanelOnly, double regionOpacity, double warningOpacity, DateTime goTime, DateTime endTime, double warningTime)
		{
			return ColorRegions(Input, pricePanelOnly, regionOpacity, warningOpacity, goTime, endTime, warningTime);
		}

		public TG.ColorRegions ColorRegions(ISeries<double> input, bool pricePanelOnly, double regionOpacity, double warningOpacity, DateTime goTime, DateTime endTime, double warningTime)
		{
			if (cacheColorRegions != null)
				for (int idx = 0; idx < cacheColorRegions.Length; idx++)
					if (cacheColorRegions[idx] != null && cacheColorRegions[idx].PricePanelOnly == pricePanelOnly && cacheColorRegions[idx].RegionOpacity == regionOpacity && cacheColorRegions[idx].WarningOpacity == warningOpacity && cacheColorRegions[idx].GoTime == goTime && cacheColorRegions[idx].EndTime == endTime && cacheColorRegions[idx].WarningTime == warningTime && cacheColorRegions[idx].EqualsInput(input))
						return cacheColorRegions[idx];
			return CacheIndicator<TG.ColorRegions>(new TG.ColorRegions(){ PricePanelOnly = pricePanelOnly, RegionOpacity = regionOpacity, WarningOpacity = warningOpacity, GoTime = goTime, EndTime = endTime, WarningTime = warningTime }, input, ref cacheColorRegions);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TG.ColorRegions ColorRegions(bool pricePanelOnly, double regionOpacity, double warningOpacity, DateTime goTime, DateTime endTime, double warningTime)
		{
			return indicator.ColorRegions(Input, pricePanelOnly, regionOpacity, warningOpacity, goTime, endTime, warningTime);
		}

		public Indicators.TG.ColorRegions ColorRegions(ISeries<double> input , bool pricePanelOnly, double regionOpacity, double warningOpacity, DateTime goTime, DateTime endTime, double warningTime)
		{
			return indicator.ColorRegions(input, pricePanelOnly, regionOpacity, warningOpacity, goTime, endTime, warningTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TG.ColorRegions ColorRegions(bool pricePanelOnly, double regionOpacity, double warningOpacity, DateTime goTime, DateTime endTime, double warningTime)
		{
			return indicator.ColorRegions(Input, pricePanelOnly, regionOpacity, warningOpacity, goTime, endTime, warningTime);
		}

		public Indicators.TG.ColorRegions ColorRegions(ISeries<double> input , bool pricePanelOnly, double regionOpacity, double warningOpacity, DateTime goTime, DateTime endTime, double warningTime)
		{
			return indicator.ColorRegions(input, pricePanelOnly, regionOpacity, warningOpacity, goTime, endTime, warningTime);
		}
	}
}

#endregion
