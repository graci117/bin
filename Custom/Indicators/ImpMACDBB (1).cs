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
	/// <summary>
    /// BLUEZ MACD BB Indicator
	/// 
	/// Code Written 9/19/2024
	/// 
	/// 
	/// 
	/// 	
    /// </summary>
	public class BluezMACDBB : Indicator
	{
		#region Variables
		
		private Brush risingMACD = Brushes.Green;
		private Brush fallingMACD = Brushes.Red;
		
		// User defined variables (add any user defined variables below)
		private MACD macd;
		private StdDev std;
		private SMA avg, avgDiff;
		private Series<double> diff;
		private int barsBetweenBands = 0, totalBars = 0;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BluezMACDBB";
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
				
				Fast = 7;
				Slow = 24;
				Smooth = 12;
				BandPeriod = 20;
				NumDevs = 0;
				
				AddLine(Brushes.Black, 0, "Zero");
				
				AddPlot(new Stroke(Brushes.Black, 2), PlotStyle.Dot, "MACDFrame");
				AddPlot(new Stroke(Brushes.Yellow, 2), PlotStyle.Dot, "MACD");
				
				AddPlot(Brushes.Aqua, "Average");
				AddPlot(Brushes.Aqua, "Upper");
				AddPlot(Brushes.Aqua, "Lower");
				
				Plots[0].DashStyleHelper = DashStyleHelper.Dot;

				Plots[1].DashStyleHelper = DashStyleHelper.Dot;
				Plots[2].DashStyleHelper = DashStyleHelper.Dot; 
				
				Plots[0].Width = 3;
				Plots[1].Width = 2;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				diff = new Series<double>(this);
						
				macd = MACD(Fast, Slow, Smooth);
				avg = SMA(macd, BandPeriod);
				std = StdDev(diff, BandPeriod);
				avgDiff = SMA(diff, BandPeriod);
			}
		}

		protected override void OnBarUpdate()
		{
			double hDiff = macd[0] - avg[0];
			double lDiff = avg[0] - macd[0];			
            diff[0] = Math.Max(hDiff, lDiff);			
			
			MACDPlot[0] = macd[0];
			MACDFrame[0] = macd[0];
			Average[0] = avg[0];
			Upper[0] = avg[0] + avgDiff[0] + NumDevs * std[0];
			Lower[0] = avg[0] - avgDiff[0] - NumDevs * std[0];
			
			if (IsRising(MACDPlot))
				PlotBrushes[1][0] = risingMACD;
			else if (IsFalling(MACDPlot))
				PlotBrushes[1][0] = fallingMACD;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast", Order=1, GroupName="Parameters")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow", Order=2, GroupName="Parameters")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smooth", Order=3, GroupName="Parameters")]
		public int Smooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BandPeriod", Order=4, GroupName="Parameters")]
		public int BandPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="NumDevs", Order=5, GroupName="Parameters")]
		public double NumDevs
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="RisingMACD", Order=6, GroupName="Parameters")]
		public Brush RisingMACD
		{ 
			get { return risingMACD; } 
			set 
			{
				risingMACD = value;
				if (risingMACD != null)
				{
					if (risingMACD.IsFrozen)
						risingMACD = risingMACD.Clone();							
					risingMACD.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string RisingMACDSerializable
		{
			get { return Serialize.BrushToString(RisingMACD); }
			set { RisingMACD = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="FallingMACD", Order=7, GroupName="Parameters")]
		public Brush FallingMACD
		{ 
			get { return fallingMACD; } 
			set 
			{
				fallingMACD = value;
				if (fallingMACD != null)
				{
					if (fallingMACD.IsFrozen)
						fallingMACD = fallingMACD.Clone();							
					fallingMACD.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string FallingMACDSerializable
		{
			get { return Serialize.BrushToString(FallingMACD); }
			set { FallingMACD = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MACDFrame
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MACDPlot
		{
			get { return Values[1]; }
		}	

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Average
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { return Values[4]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BluezMACDBB[] cacheBluezMACDBB;
		public BluezMACDBB BluezMACDBB(int fast, int slow, int smooth, int bandPeriod, double numDevs, Brush risingMACD, Brush fallingMACD)
		{
			return BluezMACDBB(Input, fast, slow, smooth, bandPeriod, numDevs, risingMACD, fallingMACD);
		}

		public BluezMACDBB BluezMACDBB(ISeries<double> input, int fast, int slow, int smooth, int bandPeriod, double numDevs, Brush risingMACD, Brush fallingMACD)
		{
			if (cacheBluezMACDBB != null)
				for (int idx = 0; idx < cacheBluezMACDBB.Length; idx++)
					if (cacheBluezMACDBB[idx] != null && cacheBluezMACDBB[idx].Fast == fast && cacheBluezMACDBB[idx].Slow == slow && cacheBluezMACDBB[idx].Smooth == smooth && cacheBluezMACDBB[idx].BandPeriod == bandPeriod && cacheBluezMACDBB[idx].NumDevs == numDevs && cacheBluezMACDBB[idx].RisingMACD == risingMACD && cacheBluezMACDBB[idx].FallingMACD == fallingMACD && cacheBluezMACDBB[idx].EqualsInput(input))
						return cacheBluezMACDBB[idx];
			return CacheIndicator<BluezMACDBB>(new BluezMACDBB(){ Fast = fast, Slow = slow, Smooth = smooth, BandPeriod = bandPeriod, NumDevs = numDevs, RisingMACD = risingMACD, FallingMACD = fallingMACD }, input, ref cacheBluezMACDBB);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BluezMACDBB BluezMACDBB(int fast, int slow, int smooth, int bandPeriod, double numDevs, Brush risingMACD, Brush fallingMACD)
		{
			return indicator.BluezMACDBB(Input, fast, slow, smooth, bandPeriod, numDevs, risingMACD, fallingMACD);
		}

		public Indicators.BluezMACDBB BluezMACDBB(ISeries<double> input , int fast, int slow, int smooth, int bandPeriod, double numDevs, Brush risingMACD, Brush fallingMACD)
		{
			return indicator.BluezMACDBB(input, fast, slow, smooth, bandPeriod, numDevs, risingMACD, fallingMACD);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BluezMACDBB BluezMACDBB(int fast, int slow, int smooth, int bandPeriod, double numDevs, Brush risingMACD, Brush fallingMACD)
		{
			return indicator.BluezMACDBB(Input, fast, slow, smooth, bandPeriod, numDevs, risingMACD, fallingMACD);
		}

		public Indicators.BluezMACDBB BluezMACDBB(ISeries<double> input , int fast, int slow, int smooth, int bandPeriod, double numDevs, Brush risingMACD, Brush fallingMACD)
		{
			return indicator.BluezMACDBB(input, fast, slow, smooth, bandPeriod, numDevs, risingMACD, fallingMACD);
		}
	}
}

#endregion
