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
	public class HMASuite : Indicator
	{
		private Series<double> maSeriesHull;
			
		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "HMASuite";
				Calculate									= Calculate.OnEachTick;
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
				Length					= 60;
				Multiplier					= 3;
				
				AddPlot(new Stroke(Brushes.LightSalmon, 3), PlotStyle.Line, "hullMAMult");
				AddPlot(new Stroke(Brushes.LightSalmon, 3), PlotStyle.Line, "hullMAMult2");
				AddPlot(Brushes.Transparent, 	"CrossDetect");
			}
			else if (State == State.Configure)
			{
				
			}
			else if (State == State.DataLoaded)
			{
				maSeriesHull = 	 HMA(Close, Length * Multiplier).Value;
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
				if (CurrentBar < 10)
				return;
			
				hullMAMult[0]  = maSeriesHull[0];
				hullMAMult2[0]  = maSeriesHull[2];
				CrossDetect[0] 	= 0;	
				//Print("V0:" + Value[0] + "---------V1:" + Value[1]);
				
				
					//= hullMAMult[0];
				if(Value[0] > Value[1])
					
					{PlotBrushes[0][0] = Brushes.Green;}
				else if(Value[0] < Value[1]) {PlotBrushes[0][0] = Brushes.Red;}
				else {PlotBrushes[0][0] = Brushes.Yellow;}
				
				if(Value[0] > Value[1]) {PlotBrushes[1][0] = Brushes.Green;}
				else if(Value[0] < Value[1]) {PlotBrushes[1][0] = Brushes.Red;}
				else {PlotBrushes[1][0] = Brushes.Yellow;}
				
				
				if (CrossAbove (hullMAMult, hullMAMult2, 1) && CurrentBar != savedUBar)
				{
					savedUBar = CurrentBar;  		// once per bar only
					CrossDetect[0] =  1;
					//DoActions();	
				}
				
				if (CrossBelow (hullMAMult, hullMAMult2, 1) && CurrentBar != savedDBar)
				{
					savedDBar = CurrentBar;			// once per bar only
					CrossDetect[0] = -1;
					//DoActions();		
				}
				
				if (CrossDetect[0] == 1 || (savedUBar > savedDBar))
				{
					Draw.Region(this, "Up" + savedUBar, CurrentBar - savedUBar + 1, 0, hullMAMult, hullMAMult2, Brushes.Transparent, Brushes.Green, 40, 0);
				}
				
				if (CrossDetect[0] == -1 || (savedDBar > savedUBar))
				{
					Draw.Region(this, "Dwn" + savedDBar, CurrentBar - savedDBar + 1, 0, hullMAMult, hullMAMult2, Brushes.Transparent, Brushes.Red, 40, 0);
				}	
				
				//Brush temp = Value[0] > Value[1] ? Brushes.Green:Brushes.Red;
				
				
		
			
		
		}		
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Order=1, GroupName="Parameters")]
		public int Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Multiplier", Order=2, GroupName="Parameters")]
		public int Multiplier
		{ get; set; }
		
		#region Plots
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> hullMAMult
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> hullMAMult2
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CrossDetect
		{
			get { return Values[2]; }
		}

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HMASuite[] cacheHMASuite;
		public HMASuite HMASuite(int length, int multiplier)
		{
			return HMASuite(Input, length, multiplier);
		}

		public HMASuite HMASuite(ISeries<double> input, int length, int multiplier)
		{
			if (cacheHMASuite != null)
				for (int idx = 0; idx < cacheHMASuite.Length; idx++)
					if (cacheHMASuite[idx] != null && cacheHMASuite[idx].Length == length && cacheHMASuite[idx].Multiplier == multiplier && cacheHMASuite[idx].EqualsInput(input))
						return cacheHMASuite[idx];
			return CacheIndicator<HMASuite>(new HMASuite(){ Length = length, Multiplier = multiplier }, input, ref cacheHMASuite);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HMASuite HMASuite(int length, int multiplier)
		{
			return indicator.HMASuite(Input, length, multiplier);
		}

		public Indicators.HMASuite HMASuite(ISeries<double> input , int length, int multiplier)
		{
			return indicator.HMASuite(input, length, multiplier);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HMASuite HMASuite(int length, int multiplier)
		{
			return indicator.HMASuite(Input, length, multiplier);
		}

		public Indicators.HMASuite HMASuite(ISeries<double> input , int length, int multiplier)
		{
			return indicator.HMASuite(input, length, multiplier);
		}
	}
}

#endregion
