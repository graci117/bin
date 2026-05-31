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
	public class HACO : Indicator
	{
		private Series<double> haOpen;
		private Series<double> haC;
		private Series<double> tMA1;
		private Series<double> tMA2;
		private Series<double> tMA3;
		private Series<double> tMA4;
		private Series<double> tMA5;
		private Series<double> tMA6;
		private Series<double> tMA7;
		private Series<double> tMA8;
		private Series<bool> keeping;
		private Series<bool> keepall;
		private Series<bool> keeping2;
		private Series<bool> keepall2;
		private Series<bool> utr;
		private Series<bool> dtr;
		private Series<bool> result;
		
		private double zlHa;
		private double zlCl;
		private double zlDif;
		private double diff;
		
		private bool keep1 = false;
		private bool keep2 = false;
		private bool keep3 = false;		
		
		private bool upw = false;
		private bool dnw = false;		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Heikin-Ashi Candlestick Oscillator as described in the December 2008 issue of S&C.";
				Name										= "HACO";
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
				Avg											= 34;
				AvgDn										= 34;
				BackgroundOpacity							= 25;
				PanelColorRising							= Brushes.MediumSeaGreen;
				PanelColorFalling							= Brushes.Pink;
				
				AddPlot(Brushes.Gray, "Result");
				
			}
			else if (State == State.Configure)
			{
					Plots[0].Width = 2;
				
					Brush temp = PanelColorRising.Clone();
					temp.Opacity = BackgroundOpacity / 100.0;
					temp.Freeze();
					PanelColorRising = temp;
					
					Brush temp1 = PanelColorFalling.Clone();
					temp1.Opacity = BackgroundOpacity / 100.0;
					temp1.Freeze();
					PanelColorFalling = temp1;				
			}
			else if (State == State.DataLoaded)
			{				
				haOpen = new Series<double>(this);
				haC = new Series<double>(this);
				tMA1 = new Series<double>(this);
				tMA2 = new Series<double>(this);
				tMA3 = new Series<double>(this);
				tMA4 = new Series<double>(this);
				tMA5 = new Series<double>(this);
				tMA6 = new Series<double>(this);
				tMA7 = new Series<double>(this);
				tMA8 = new Series<double>(this);
				keeping = new Series<bool>(this);
				keepall = new Series<bool>(this);
				keeping2 = new Series<bool>(this);
				keepall2 = new Series<bool>(this);
				utr = new Series<bool>(this);
				dtr = new Series<bool>(this);
				result = new Series<bool>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2)
				return;
			
			// UPTREND
			haOpen[0]	= 	((((Open[1] + High[1] + Low[1] + Close[1]) / 4) + haOpen[1]) / 2);
			haC[0]		=	(((Open[0] + High[0] + Low[0] + Close[0]) / 4 + haOpen[0] + Math.Max(High[0], haOpen[0]) + Math.Min(Low[0], haOpen[0])) / 4);
			
			tMA1[0]		=	(TEMA(haC, Avg)[0]);
			tMA2[0]		=	(TEMA(tMA1, Avg)[0]);
			diff		= 	tMA1[0] - tMA2[0];
			zlHa		= 	tMA1[0] + diff;
			
			tMA3[0]		= 	(TEMA(Median, Avg)[0]);
			tMA4[0]		=	(TEMA(tMA3, Avg)[0]);
			diff 		= 	tMA3[0] - tMA4[0];
			zlCl 		= 	tMA3[0] + diff;
			zlDif 		= 	zlCl - zlHa;
			
			// Keep Bools			
			if (MRO(delegate {return haC[0] >= haOpen[0];}, 1, 2) > -1)
				keep1 = true;
			else
				keep1 = false;
			
			keep2 = (zlDif >= 0) ? true : false;
			
			if (keep1 || keep2)
				keeping[0] 	= 	(true);
			else
				keeping[0]	=	(false);
			
			if (keeping[0] || (keeping[1] && (Close[0] >= 0) || Close[0] >= Close[1]))
				keepall[0]	=	(true);
			else
				keepall[0]	=	(false);
			
			keep3 = ((Math.Abs(Close[0] - Open[0]) < (High[0] - Low[0]) * 0.35) && (High[0] >= Low[1])) ? true : false;
			
			if (keepall[0] || (keepall[1] && keep3))
				utr[0]		=	(true);
			else
				utr[0]		=	(false);
			
			// DOWNTREND
			tMA5[0]			=	(TEMA(haC, AvgDn)[0]);
			tMA6[0]			=	(TEMA(tMA5, AvgDn)[0]);
			diff 			= 	tMA5[0] - tMA6[0];
			zlHa 			= 	tMA5[0] + diff;
			
			tMA7[0]			=	(TEMA(Median, AvgDn)[0]);
			tMA8[0]			=	(TEMA(tMA7, AvgDn)[0]);
			diff 			= 	tMA7[0] - tMA8[0];
			zlCl 			= 	tMA7[0] + diff;			
			zlDif 			= 	zlCl - zlHa;
			
			// Keep Bools			
			if (MRO(delegate {return haC[0] < haOpen[0];}, 1, 2) > -1)
				keep1 = true;
			else
				keep1 = false;
			
			keep2 = (zlDif < 0) ? true : false;
			keep3 = ((Math.Abs(Close[0] - Open[0]) < (High[0] - Low[0]) * 0.35) && (Low[0] <= High[1])) ? true : false;
			
			if (keep1 || keep2)
				keeping2[0]	=	(true);
			else
				keeping2[0]	=	(false);
			
			if (keeping2[0] || (keeping2[1] && (Close[0] < Open[0]) || Close[0] < Close[1]))
				keepall2[0]	=	(true);
			else
				keepall2[0]	=	(false);
			
			
			// Output Routine
			if (keepall2[0] || (keepall2[1] && keep3))
				dtr[0]		=	(true);
			else
				dtr[0]		=	(false);
			
			if (dtr[0] == false && dtr[1] && utr[0])
				upw = true;
			else
				upw = false;
			
			if (utr[0] == false && utr[1] && dtr[0])
				dnw = true;
			else
				dnw = false;
			
			if (upw)
			{
				Result[0] 	=	(1);
				BackBrushAll = 	PanelColorRising;
			}
			else if (dnw)
			{
				Result[0]	=	(0);
				BackBrushAll = 	PanelColorFalling;
			}
			else if (dnw == false)
			{
				Result[0]	=	(Result[1]);
				if (Result[0] == 1)
					BackBrushAll = PanelColorRising;
				else
					BackBrushAll = PanelColorFalling;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Avg", Description="Up TEMA average", Order=1, GroupName="Parameters")]
		public int Avg
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="AvgDn", Description="Down TEMA average", Order=2, GroupName="Parameters")]
		public int AvgDn
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Result
		{
			get { return Values[0]; }
		}
		
		[Range(1, 99)]
		[NinjaScriptProperty]
		[Display(Name=" % Opacity of background", Description="Sets the amount of opacity of background colors ", Order=23, GroupName="Options")]
		public int BackgroundOpacity
		{ get; set; }			

		[XmlIgnore]
		[Display(Name="Panel Color HACO Rising", Description="Panel background when HACO Rising", Order=24, GroupName="Options")]
		public Brush PanelColorRising
		{ get; set; }

		[Browsable(false)]
		public string PanelColorRisingSerializable
		{
			get { return Serialize.BrushToString(PanelColorRising); }
			set { PanelColorRising = Serialize.StringToBrush(value); }
		}	

		[XmlIgnore]
		[Display(Name="Panel Color HACO Falling", Description="Panel background when HACO falling", Order=26, GroupName="Options")]
		public Brush PanelColorFalling
		{ get; set; }

		[Browsable(false)]
		public string PanelColorFallingSerializable
		{
			get { return Serialize.BrushToString(PanelColorFalling); }
			set { PanelColorFalling = Serialize.StringToBrush(value); }
		}			
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HACO[] cacheHACO;
		public HACO HACO(int avg, int avgDn, int backgroundOpacity)
		{
			return HACO(Input, avg, avgDn, backgroundOpacity);
		}

		public HACO HACO(ISeries<double> input, int avg, int avgDn, int backgroundOpacity)
		{
			if (cacheHACO != null)
				for (int idx = 0; idx < cacheHACO.Length; idx++)
					if (cacheHACO[idx] != null && cacheHACO[idx].Avg == avg && cacheHACO[idx].AvgDn == avgDn && cacheHACO[idx].BackgroundOpacity == backgroundOpacity && cacheHACO[idx].EqualsInput(input))
						return cacheHACO[idx];
			return CacheIndicator<HACO>(new HACO(){ Avg = avg, AvgDn = avgDn, BackgroundOpacity = backgroundOpacity }, input, ref cacheHACO);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HACO HACO(int avg, int avgDn, int backgroundOpacity)
		{
			return indicator.HACO(Input, avg, avgDn, backgroundOpacity);
		}

		public Indicators.HACO HACO(ISeries<double> input , int avg, int avgDn, int backgroundOpacity)
		{
			return indicator.HACO(input, avg, avgDn, backgroundOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HACO HACO(int avg, int avgDn, int backgroundOpacity)
		{
			return indicator.HACO(Input, avg, avgDn, backgroundOpacity);
		}

		public Indicators.HACO HACO(ISeries<double> input , int avg, int avgDn, int backgroundOpacity)
		{
			return indicator.HACO(input, avg, avgDn, backgroundOpacity);
		}
	}
}

#endregion
