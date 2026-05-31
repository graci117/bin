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
namespace NinjaTrader.NinjaScript.Indicators.BobC
{
	public class OpeningPrice : Indicator
	{NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 10) { Size = 10, Bold = false };
	#region States	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"A Line is drawn from the specified Open.";
				Name						= "OpeningPrice";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true; 
				DrawOnPricePanel			= true;
				PaintPriceMarkers			= true;
				IsSuspendedWhileInactive	= true;
						
		#region Variables        
				OpenHour = 8;
				OpenMinute = 0;
				OpenTextHorizontal=-4;
				OpenTextCenter=0;
				ExtendPlotHours =6;
				double o=0.0;
				  LineSize=4;		
				MyBrush = Brushes.DarkOrange;				
				PlotHistoricalOpen 			= true;				
			}
		#endregion	
			else if (State == State.Configure)
			{
				AddPlot(MyBrush, "MyBrush");
				AddDataSeries(BarsPeriodType.Minute, 1);	
			}
		}
	#endregion	
		protected override void OnBarUpdate()
		{	
			if (CurrentBars[0]<BarsRequiredToPlot || CurrentBars[1] <BarsRequiredToPlot)
			{return;}
		#region Trap Open Price			
			if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && BarsPeriod.Value == 1)   //Only look at 1Min bars regardless of chart
			{		
				int BarMinute = Time[1].Minute;
				int BarHour = Time[1].Hour;
				int BarDay = Time[1].Day;
				int BarMonth = Time[1].Month;
				int BarYear = Time[1].Year;
				int open = CurrentBars[1]-Bars.GetBar(new DateTime(BarYear, BarMonth, BarDay, OpenHour, OpenMinute,0));				//Open Time example 800 cst
				PlotBrushes[0][0]=MyBrush;
							
				if ( open == 0 ) //Bar Zero at the Open
				{DateTime xStartTime=  Times[0][1].Date.AddHours(OpenHour).AddMinutes(OpenMinute);  //Time of Open
				DateTime xEndTime= 	xStartTime.AddHours(ExtendPlotHours);   //Extend Line Hours
				o=Closes[1][0];	  //level of Open on 1Min timebase
			
				if (PlotHistoricalOpen==true)
				{	
					Draw.Line(this,"OpenLine"+Times[0][1].Date, false, xStartTime, o, xEndTime, o, PlotBrushes[0][0], MyDashStyle,LineSize, true);
				}
				
				if (PlotHistoricalOpen==false)
				{	
					Draw.Line(this,"OpenLine", false, xStartTime, o, xEndTime, o, PlotBrushes[0][0], MyDashStyle, LineSize, true);
				}
				
				}
				if ( open >= 0 ) //Time at or past the Open
				{
					Draw.Text(this, "tag1", userText, (OpenTextHorizontal), o+(OpenTextCenter*TickSize),PlotBrushes[0][0]);
				}
				if ( open <= 0 ) //Time at or past the Open
				{
					Draw.Text(this, "tag1", userText, (OpenTextHorizontal), o+(OpenTextCenter*TickSize),Brushes.Transparent);
				}	
		#endregion			
					
			}
			else return;
		}
		
		// In order to trim the indicator's label we need to override the ToString() method.
			public override string DisplayName
				{
		            get { return Name + "(" + OpenHour +":"+OpenMinute+ ")";}
				}	

		
		#region Properties
	
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="ExtendPlotHours", Description="Extend Open Line x hours", Order=5, GroupName="Parameters")]
		public int ExtendPlotHours
		{ get; set; }
			
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EventHour", Description="Hour of Event", Order=3, GroupName="Parameters")]
		public int OpenHour
		{ get; set; }
		
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EventMinute", Description="Minute of Event", Order=4, GroupName="Parameters")]
		public int OpenMinute
		{ get; set; }
	
		[Range(-100, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EventTextHorizontal", Description="Horizontal Text Offset from latest Bar", Order=6, GroupName="Parameters")]
		public int OpenTextHorizontal
		{ get; set; }

		[Range(-100, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="EventTextCenter", Description="Vertical Text Offset Ticks", Order=7, GroupName="Parameters")]
		public int OpenTextCenter
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Display(Name="PlotHistoryEvent", Description="False=Plot Current Only", Order=8, GroupName="Parameters")]
		public bool PlotHistoricalOpen
		{ get; set; }
			
		[Browsable(false)]	
		[XmlIgnore()]		
		public double TheOpen
        {
      	 get; set; 
        }
				
		[Browsable(false)]	
		[XmlIgnore()]		
		public double o
        {
      	 get; set; 
        }
		[XmlIgnore]
		[Display(Name="MyBrush", Description="Color of the Event Marker", Order=1, GroupName="Parameters")]
		public Brush MyBrush
		{ get; set; }

		[Browsable(false)]
		public string MyBrushSerializable
		{
			get { return Serialize.BrushToString(MyBrush); }
			set { MyBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(Name="Brush Style", Description="Style of the Event Marker", Order=3, GroupName="Parameters")]
		public DashStyleHelper MyDashStyle 
		{get;set;}
		
		[Range(0, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="BrushSize", Description="Width of the Open Line", Order=2, GroupName="Parameters")]
		public int LineSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="UserText", Description="Label for Line", Order=5, GroupName="Parameters")]
		public string userText
		{ get; set; }
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BobC.OpeningPrice[] cacheOpeningPrice;
		public BobC.OpeningPrice OpeningPrice(int extendPlotHours, int openHour, int openMinute, int openTextHorizontal, int openTextCenter, bool plotHistoricalOpen, int lineSize, string userText)
		{
			return OpeningPrice(Input, extendPlotHours, openHour, openMinute, openTextHorizontal, openTextCenter, plotHistoricalOpen, lineSize, userText);
		}

		public BobC.OpeningPrice OpeningPrice(ISeries<double> input, int extendPlotHours, int openHour, int openMinute, int openTextHorizontal, int openTextCenter, bool plotHistoricalOpen, int lineSize, string userText)
		{
			if (cacheOpeningPrice != null)
				for (int idx = 0; idx < cacheOpeningPrice.Length; idx++)
					if (cacheOpeningPrice[idx] != null && cacheOpeningPrice[idx].ExtendPlotHours == extendPlotHours && cacheOpeningPrice[idx].OpenHour == openHour && cacheOpeningPrice[idx].OpenMinute == openMinute && cacheOpeningPrice[idx].OpenTextHorizontal == openTextHorizontal && cacheOpeningPrice[idx].OpenTextCenter == openTextCenter && cacheOpeningPrice[idx].PlotHistoricalOpen == plotHistoricalOpen && cacheOpeningPrice[idx].LineSize == lineSize && cacheOpeningPrice[idx].userText == userText && cacheOpeningPrice[idx].EqualsInput(input))
						return cacheOpeningPrice[idx];
			return CacheIndicator<BobC.OpeningPrice>(new BobC.OpeningPrice(){ ExtendPlotHours = extendPlotHours, OpenHour = openHour, OpenMinute = openMinute, OpenTextHorizontal = openTextHorizontal, OpenTextCenter = openTextCenter, PlotHistoricalOpen = plotHistoricalOpen, LineSize = lineSize, userText = userText }, input, ref cacheOpeningPrice);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BobC.OpeningPrice OpeningPrice(int extendPlotHours, int openHour, int openMinute, int openTextHorizontal, int openTextCenter, bool plotHistoricalOpen, int lineSize, string userText)
		{
			return indicator.OpeningPrice(Input, extendPlotHours, openHour, openMinute, openTextHorizontal, openTextCenter, plotHistoricalOpen, lineSize, userText);
		}

		public Indicators.BobC.OpeningPrice OpeningPrice(ISeries<double> input , int extendPlotHours, int openHour, int openMinute, int openTextHorizontal, int openTextCenter, bool plotHistoricalOpen, int lineSize, string userText)
		{
			return indicator.OpeningPrice(input, extendPlotHours, openHour, openMinute, openTextHorizontal, openTextCenter, plotHistoricalOpen, lineSize, userText);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BobC.OpeningPrice OpeningPrice(int extendPlotHours, int openHour, int openMinute, int openTextHorizontal, int openTextCenter, bool plotHistoricalOpen, int lineSize, string userText)
		{
			return indicator.OpeningPrice(Input, extendPlotHours, openHour, openMinute, openTextHorizontal, openTextCenter, plotHistoricalOpen, lineSize, userText);
		}

		public Indicators.BobC.OpeningPrice OpeningPrice(ISeries<double> input , int extendPlotHours, int openHour, int openMinute, int openTextHorizontal, int openTextCenter, bool plotHistoricalOpen, int lineSize, string userText)
		{
			return indicator.OpeningPrice(input, extendPlotHours, openHour, openMinute, openTextHorizontal, openTextCenter, plotHistoricalOpen, lineSize, userText);
		}
	}
}

#endregion
