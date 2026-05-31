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
	public class JurbolMultiMAColorSlope : Indicator
	{
		
		#region Variables
		// Create a variable that stores the user's selection for a moving average 
		private JTSUniversalMovingAverage	matype	= JTSUniversalMovingAverage.SMA;
		
		private int			period					= 14;
		private Brush		risingMA        		= Brushes.Chartreuse;
		private Brush		fallingMA        		= Brushes.Red;
		private Brush		flatMA          		= Brushes.Yellow;
		private bool		SlopeColor				= true;			
		
		#endregion
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "JurbolMultiMAColorSlope";
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
				MAType = JTSUniversalMovingAverage.EMA;
				Period = 14;
			}
			else if (State == State.Configure)
			{
				//AddPlot(Brushes.Orange, "MA");
				AddPlot(new Stroke(Brushes.Orange, 3), PlotStyle.Line, "MA");
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
			if (CurrentBar < 20) 
				return;
			double EmaH = EMA(High, period)[0];

			
			switch (matype)
			{
				case JTSUniversalMovingAverage.EMA:
				{
					Value[0] = (EMA(period)[0]);
					
					break;
				}
				
				case JTSUniversalMovingAverage.HMA:
				{
					Value[0] = (HMA(period)[0]);
					break;
				}
				
				case JTSUniversalMovingAverage.SMA:
				{
					Value[0] = (SMA(period)[0]);
					break;
				}
				
				case JTSUniversalMovingAverage.WMA:
				{
					Value[0] = (WMA(period)[0]);
					break;
				}
				case JTSUniversalMovingAverage.DEMA:
				{
					Value[0] = (DEMA(period)[0]);
					break;
				}
				case JTSUniversalMovingAverage.TEMA:
				{
					Value[0] = (TEMA(period)[0]);
					break;
				}
				
			
			}
			
			//*******************************************************
			// SLOPE COLORS
			//*******************************************************
			if(SlopeColor == true)
			if(IsRising(Value)) {PlotBrushes[0][0] = risingMA;}
			else if(IsFalling(Value)) {PlotBrushes[0][0] = fallingMA;}
			else {PlotBrushes[0][0] = flatMA;}
			
		}
		
		#region Properties
		// Creates the user definable parameter for the moving average type.
		[NinjaScriptProperty]
		[Display(Name="01. Select MA Type", Description="Choose a Moving Average type.", GroupName="01. Parameters", Order = 1)]
		public JTSUniversalMovingAverage MAType
		{
			get { return matype; }
			set { matype = value; }
		}
		
		[NinjaScriptProperty]
		[Display(Name="02. MA Period", Description="Numbers of bars used for calculations", GroupName="01. Parameters", Order = 2)]
		public int Period
		{
			get { return period; }
			set { period = Math.Max(1, value); }
		}
		
		
		[Display(Name="01. Colored Slope?", Description="Use colored slope", GroupName="02.Slope", Order = 1)]
        public bool slopeColor
        {
            get { return SlopeColor; }
            set { SlopeColor = value; }
        }
		
		[XmlIgnore()]
		[Display(Name="02. Rising MA Color", Description="Color for rising MA", GroupName="02.Slope", Order = 2)]
        public Brush RisingMA
        {
            get { return risingMA; }
            set { risingMA = value; }
        }
		
		
		[Browsable(false)]
		public string RisingMASerialize
		{
			get { return Serialize.BrushToString(RisingMA); }
			set { RisingMA = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[Display(Name="03. Falling MA Color", Description="Color for falling MA", GroupName="02.Slope", Order = 3)]
        public Brush FallingMA
        {
            get { return fallingMA; }
            set { fallingMA = value; }
        }
		
		
		[Browsable(false)]
		public string FallingMASerialize
		{
			get { return Serialize.BrushToString(FallingMA); }
			set { FallingMA = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore()]
		[Display(Name="04. Flat MA Color", Description="Color for flat MA", GroupName="02.Slope", Order = 4)]
        public Brush FlatMA
        {
            get { return flatMA; }
            set { flatMA = value; }
        }
		
		[Browsable(false)]
		public string FlatMASerialize
		{
			get { return Serialize.BrushToString(FlatMA); }
			set { FlatMA = Serialize.StringToBrush(value); }
		}
		
		
        #endregion
		
	}
}


public enum JTSUniversalMovingAverage
{
	EMA,
	HMA,
	SMA,
	WMA,
	DEMA,
	TEMA
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private JurbolMultiMAColorSlope[] cacheJurbolMultiMAColorSlope;
		public JurbolMultiMAColorSlope JurbolMultiMAColorSlope(JTSUniversalMovingAverage mAType, int period)
		{
			return JurbolMultiMAColorSlope(Input, mAType, period);
		}

		public JurbolMultiMAColorSlope JurbolMultiMAColorSlope(ISeries<double> input, JTSUniversalMovingAverage mAType, int period)
		{
			if (cacheJurbolMultiMAColorSlope != null)
				for (int idx = 0; idx < cacheJurbolMultiMAColorSlope.Length; idx++)
					if (cacheJurbolMultiMAColorSlope[idx] != null && cacheJurbolMultiMAColorSlope[idx].MAType == mAType && cacheJurbolMultiMAColorSlope[idx].Period == period && cacheJurbolMultiMAColorSlope[idx].EqualsInput(input))
						return cacheJurbolMultiMAColorSlope[idx];
			return CacheIndicator<JurbolMultiMAColorSlope>(new JurbolMultiMAColorSlope(){ MAType = mAType, Period = period }, input, ref cacheJurbolMultiMAColorSlope);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.JurbolMultiMAColorSlope JurbolMultiMAColorSlope(JTSUniversalMovingAverage mAType, int period)
		{
			return indicator.JurbolMultiMAColorSlope(Input, mAType, period);
		}

		public Indicators.JurbolMultiMAColorSlope JurbolMultiMAColorSlope(ISeries<double> input , JTSUniversalMovingAverage mAType, int period)
		{
			return indicator.JurbolMultiMAColorSlope(input, mAType, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.JurbolMultiMAColorSlope JurbolMultiMAColorSlope(JTSUniversalMovingAverage mAType, int period)
		{
			return indicator.JurbolMultiMAColorSlope(Input, mAType, period);
		}

		public Indicators.JurbolMultiMAColorSlope JurbolMultiMAColorSlope(ISeries<double> input , JTSUniversalMovingAverage mAType, int period)
		{
			return indicator.JurbolMultiMAColorSlope(input, mAType, period);
		}
	}
}

#endregion
