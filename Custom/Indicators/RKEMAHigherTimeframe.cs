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

namespace NinjaTrader.NinjaScript.Indicators
{
	public class RKEMAHigherTimeframe : Indicator
	{
		private double myValue1;
		private double myValue11;
		private double myValue2;
		private double myFactor;
		private double mySMATotal;
		
		private int myIntradayBar;
		private int myCurrentBar;
		private int myFrequency;
		private int mySMABars;
		
		private SessionIterator sessionIterator;
    	private DateTime endTime;
		
		private bool lastBarOfSession;
		private string myMsg;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Draw EMA of a higher Timeframe on a n-Minutes chart (by Ronny Keller)";
				Name										= "RKEMAHigherTimeframe";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				IsAutoScale									= false;
				DrawOnPricePanel							= true;
				IsSuspendedWhileInactive					= true;
				PaintPriceMarkers							= true;
				DisplayInDataBox							= true;
				Period										= 20;
				HigherTimeframe								= 60;
				AddPlot(new Stroke(Brushes.Navy, DashStyleHelper.Dash, 1), PlotStyle.Line,"EMA HTF");

			}
			else if (State == State.DataLoaded)
  			{
    			//stores the sessions once bars are ready, but before OnBarUpdate is called
    			sessionIterator = new SessionIterator(Bars);
  			}
			else if (State == State.Configure)
			{
				myMsg = "";
				mySMABars = 0;
				myIntradayBar = 0;
				myCurrentBar = 0;
				myFrequency = 0;
				lastBarOfSession = false;

				myFactor = (2.0 / (Period + 1.0));
				myFrequency = HigherTimeframe / BarsPeriod.Value;
				mySMABars = myFrequency * Period;

				mySMATotal = 0;
			}
		}

		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday || BarsPeriod.BarsPeriodType != BarsPeriodType.Minute || BarsPeriod.Value < 1)
			{
				myMsg = "RKEMAHigherTimeframe: Works on minute-based intraday-charts only.";
				Draw.TextFixed(this, "RKEmaHTF1", myMsg, TextPosition.BottomRight, ChartControl.Properties.ChartText, 
  								ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);				
				return;
			}

			if ((Convert.ToDouble(HigherTimeframe) / Convert.ToDouble(BarsPeriod.Value)) % 1 != 0 || HigherTimeframe == BarsPeriod.Value)
			{
				myMsg = "RKEMAHigherTimeframe: Higher Timeframe needs to be a multiple of the current Timeframe.";
				Draw.TextFixed(this, "RKEmaHTF1", myMsg, TextPosition.BottomRight, ChartControl.Properties.ChartText, 
  								ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);				
				return;
			}

			// on new bars session, find the next trading session
  			if (Bars.IsFirstBarOfSession)
  			{
    			sessionIterator.GetNextSession(Time[0], true);
    			endTime     = sessionIterator.ActualSessionEnd;
  			}

			if (myCurrentBar != CurrentBar)
			{
				myCurrentBar = CurrentBar;	
				if (Bars.IsFirstBarOfSession)
				{
					myIntradayBar = 0;
				}
				myIntradayBar += 1;

				if (myCurrentBar < mySMABars) 
				{
					if (((myCurrentBar) % myFrequency == 0) && myCurrentBar>1)
					{
						mySMATotal += Close[1];
					}
					return;
				}
				
				if (myCurrentBar == mySMABars)
				{
					mySMATotal += Close[1];
					myValue1 = mySMATotal / Convert.ToDouble(Period);
					return;
				}
				
				myValue11 = myValue1;	//previous Value1

				lastBarOfSession = false;
				if (Time[0] == endTime) {
					lastBarOfSession = true;
				}
				
			}
			

			if ((myIntradayBar % myFrequency == 0) || lastBarOfSession)
			{
				myValue1 = myValue11 + myFactor * (Close[0] - myValue11);
				myValue2 = myValue1;
				MyEmaHigher[0] = myValue1;
			} 
			else
			{
				myValue2 = myValue1 + myFactor * (Close[0] - myValue11);
				MyEmaHigher[0] = myValue2;
			}

		}

		#region Properties

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Higher Timeframe Minutes", GroupName = "Settings", Order = 0)]
		public int HigherTimeframe
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "EMA Period", GroupName = "Settings", Order = 1)]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MyEmaHigher
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RKEMAHigherTimeframe[] cacheRKEMAHigherTimeframe;
		public RKEMAHigherTimeframe RKEMAHigherTimeframe(int higherTimeframe, int period)
		{
			return RKEMAHigherTimeframe(Input, higherTimeframe, period);
		}

		public RKEMAHigherTimeframe RKEMAHigherTimeframe(ISeries<double> input, int higherTimeframe, int period)
		{
			if (cacheRKEMAHigherTimeframe != null)
				for (int idx = 0; idx < cacheRKEMAHigherTimeframe.Length; idx++)
					if (cacheRKEMAHigherTimeframe[idx] != null && cacheRKEMAHigherTimeframe[idx].HigherTimeframe == higherTimeframe && cacheRKEMAHigherTimeframe[idx].Period == period && cacheRKEMAHigherTimeframe[idx].EqualsInput(input))
						return cacheRKEMAHigherTimeframe[idx];
			return CacheIndicator<RKEMAHigherTimeframe>(new RKEMAHigherTimeframe(){ HigherTimeframe = higherTimeframe, Period = period }, input, ref cacheRKEMAHigherTimeframe);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RKEMAHigherTimeframe RKEMAHigherTimeframe(int higherTimeframe, int period)
		{
			return indicator.RKEMAHigherTimeframe(Input, higherTimeframe, period);
		}

		public Indicators.RKEMAHigherTimeframe RKEMAHigherTimeframe(ISeries<double> input , int higherTimeframe, int period)
		{
			return indicator.RKEMAHigherTimeframe(input, higherTimeframe, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RKEMAHigherTimeframe RKEMAHigherTimeframe(int higherTimeframe, int period)
		{
			return indicator.RKEMAHigherTimeframe(Input, higherTimeframe, period);
		}

		public Indicators.RKEMAHigherTimeframe RKEMAHigherTimeframe(ISeries<double> input , int higherTimeframe, int period)
		{
			return indicator.RKEMAHigherTimeframe(input, higherTimeframe, period);
		}
	}
}

#endregion
