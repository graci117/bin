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
	public class LinRegBands : Indicator
	{
		private Series<double> lsma;
        private Series<double> lsl;
        private Series<double> lsh;
        private Series<double> dhs;
        private Series<double> lireg;
        private Series<double> lire;
        private Series<double> hhi;
        private Series<double> llo;
        private Series<double> hi;
        private Series<double> lo;
        private Series<double> wr;
        private Series<double> wi;
		private Series<double> hl;
        private Series<double> dhdv;
		private Series<double> hlhl;
		
		
		
		private LinRegWithOffset lg1;
		private LinRegWithOffset lg2;
		private LinRegWithOffset lg3;
		
		private bool LongDirection;
		private bool ShortDirection;
		private double priceSd;

		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "LinRegBands";
				Calculate									= Calculate.OnEachTick;
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
				Length					= 11;
				Offset					= 7;
				MyInput					= PriceType.Low;
				LongDirection			= false;
				ShortDirection			= false;
				LongEntrySd			= 0.0;
				LongExitSd			= 2.5;
				ShortEntrySd		= 0.0;
				ShortExitSd			= -0.3;
				
				LongFilterOn					= @"LongRGOn";
				LongFilterOff					= @"LongRGOff";
				ShortFilterOn					= @"ShortRGOn";
				ShortFilterOff					= @"ShortRGOff";
				
				
		
				
				AddPlot(new Stroke(Brushes.Peru, 3), PlotStyle.Line, "SignalLine");
				AddPlot(new Stroke(Brushes.Red, 3), PlotStyle.Line, "TopBand");
				AddPlot(new Stroke(Brushes.Green, 3), PlotStyle.Line, "LowerBand");

			}
			else if (State == State.Configure)
			{
				lsma = new Series<double>(this);
                lsl = new Series<double>(this);
                lsh = new Series<double>(this);
                dhs = new Series<double>(this);
                lireg = new Series<double>(this);
                lire = new Series<double>(this);
                hhi = new Series<double>(this);
                llo = new Series<double>(this);
                hi = new Series<double>(this);
                lo = new Series<double>(this);
                wr = new Series<double>(this);
                wi = new Series<double>(this);
				hl = new Series<double>(this);
                dhdv = new Series<double>(this);
				hlhl = new Series<double>(this);

			}
			else if (State == State.DataLoaded)
			{	
				
				lg1 = GetLinRegMiddle();
            	lg2 = LinRegWithOffset(Low, Length, Offset);
            	lg3 = LinRegWithOffset(High, Length, Offset);
				
			}
			
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 70)
				return;
			
			lsma[0] = lg1[0];
            lsl[0] = lg2[0];;
            lsh[0] = lg3[0];;

			hlhl[0] = High[0] - Low[0];
          
			hl[0]  = StdDev(hlhl,Length)[0];
            double dv = SMA(
				hl, 5)[0] - SMA(hl, 10)[0];
            double dh = lsh[0] - lsl[0];
			dhdv[0] = dh + dv * 0.3;
            dhs[0] = SMA(dhdv, 3)[0];
            lireg[0] = lsma[0] + (dhs[0] / 2) / 2;
            lire[0] = lsma[0] - (dhs[0] / 2) / 2;

            double dhq = dhs[0] * 1.618;
            double thq = dhs[0] * 2.618;
            double whd = dhs[0] * 4.236;
            hhi[0] = lsma[0] + thq;
            llo[0] = lsma[0] - thq;
            hi[0] = lsma[0] + dhq;
            lo[0] = lsma[0] - dhq;
            wr[0] = lsma[0] + whd;
            wi[0] = lsma[0] - whd;
			
			priceSd = (Low[0] - lsma[0]) / (dhs[0] / 2);

			
			if (Low[0] <= wi[0] && priceSd <= LongEntrySd
				 && (LongDirection == false))
			{
				
				Draw.Text(this, Convert.ToString(LongFilterOn) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "RGLong", 0, (Low[0] + (-12 * TickSize)), Brushes.Green );	
				LongDirection = true;
			}
			else if (High[0] >= wr[0]  && priceSd >= ShortEntrySd
				 && (ShortDirection == false))
			{
				Draw.Text(this, Convert.ToString(ShortFilterOn) + Convert.ToString(CurrentBars[0]), "RGShort" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.DarkRed );
				ShortDirection = true;
			}
			else
			{
				if  (LongDirection == true && priceSd >= LongExitSd)
				{
					LongDirection = false;
					Draw.Text(this, Convert.ToString(LongFilterOff) + Convert.ToString(CurrentBars[0]), "RGExitLong" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Magenta );		
					
					//Draw.Text(this, Convert.ToString(LongFilterOff) + " " + Convert.ToString(CurrentBars[0]), @"LongOFF", 0, (Close[0] + (10 * TickSize)) );
				}
				else if (ShortDirection == true && priceSd <= ShortExitSd)
				{
					ShortDirection = false;
					Draw.Text(this, Convert.ToString(ShortFilterOff) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "RGExitShort", 0, (Low[0] + (-12 * TickSize)), Brushes.YellowGreen );	
					
				}
			}

            Values[0][0] = lsma[0];
            Values[1][0] = wr[0];
            Values[2][0] = wi[0];

		}
		
		private LinRegWithOffset GetLinRegMiddle()
		{
			//ISeries<double> inputPrice = null;
			switch (MyInput) 
            {
                case PriceType.High:
                    return LinRegWithOffset(High, Length, Offset);
                    break;
                case PriceType.Low:
                    return LinRegWithOffset(Low, Length, Offset);	
					break;
                case PriceType.Median:
                    return LinRegWithOffset(Median, Length, Offset);
                    break;
                case PriceType.Open:
                   return LinRegWithOffset(Open, Length, Offset);
                    break;
                case PriceType.Typical:
                   return LinRegWithOffset(Typical, Length, Offset);
                    break;
                case PriceType.Weighted:
                   return LinRegWithOffset(Weighted, Length, Offset);
                    break;
				 case PriceType.Close:
                   return LinRegWithOffset(Close, Length, Offset);
                    break;
                default:
					return LinRegWithOffset(Low, Length, Offset);
                    break;
               
            }
			//return inputPrice;
		}
		




		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Order=1, GroupName="Parameters")]
		public int Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Offset", Order=2, GroupName="Parameters")]
		public int Offset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="LinReg Source", Description="Select price Type (Close, high, Low, etc.)", Order=99)]
		public PriceType MyInput
        { get; set; }		
		
		
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="LongEntrySd", Order=1, GroupName="Standand deviation Entries")]
		public double LongEntrySd
		{ get; set; }
		
			[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="LongExitSd", Order=1, GroupName="Standand deviation Entries")]
		public double LongExitSd
		{ get; set; }
		
			[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ShortEntrySd", Order=1, GroupName="Standand deviation Entries")]
		public double ShortEntrySd
		{ get; set; }
		
			[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="ShortExitSd", Order=1, GroupName="Standand deviation Entries")]
		public double ShortExitSd
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SignalLine
		{
			get { return Values[0]; }
		}
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBand
		{
			get { return Values[1]; }
		}
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBand
		{
			get { return Values[2]; }
		}
		
		
		[NinjaScriptProperty]
		[Display(Name="LongFilterOn", Order=4, GroupName="Signals")]
		public string LongFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="LongFilterOff", Order=5, GroupName="Signals")]
		public string LongFilterOff
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOn", Order=6, GroupName="Signals")]
		public string ShortFilterOn
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortFilterOff", Order=7, GroupName="Signals")]
		public string ShortFilterOff
		{ get; set; }
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinRegBands[] cacheLinRegBands;
		public LinRegBands LinRegBands(int length, int offset, PriceType myInput, double longEntrySd, double longExitSd, double shortEntrySd, double shortExitSd, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return LinRegBands(Input, length, offset, myInput, longEntrySd, longExitSd, shortEntrySd, shortExitSd, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public LinRegBands LinRegBands(ISeries<double> input, int length, int offset, PriceType myInput, double longEntrySd, double longExitSd, double shortEntrySd, double shortExitSd, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			if (cacheLinRegBands != null)
				for (int idx = 0; idx < cacheLinRegBands.Length; idx++)
					if (cacheLinRegBands[idx] != null && cacheLinRegBands[idx].Length == length && cacheLinRegBands[idx].Offset == offset && cacheLinRegBands[idx].MyInput == myInput && cacheLinRegBands[idx].LongEntrySd == longEntrySd && cacheLinRegBands[idx].LongExitSd == longExitSd && cacheLinRegBands[idx].ShortEntrySd == shortEntrySd && cacheLinRegBands[idx].ShortExitSd == shortExitSd && cacheLinRegBands[idx].LongFilterOn == longFilterOn && cacheLinRegBands[idx].LongFilterOff == longFilterOff && cacheLinRegBands[idx].ShortFilterOn == shortFilterOn && cacheLinRegBands[idx].ShortFilterOff == shortFilterOff && cacheLinRegBands[idx].EqualsInput(input))
						return cacheLinRegBands[idx];
			return CacheIndicator<LinRegBands>(new LinRegBands(){ Length = length, Offset = offset, MyInput = myInput, LongEntrySd = longEntrySd, LongExitSd = longExitSd, ShortEntrySd = shortEntrySd, ShortExitSd = shortExitSd, LongFilterOn = longFilterOn, LongFilterOff = longFilterOff, ShortFilterOn = shortFilterOn, ShortFilterOff = shortFilterOff }, input, ref cacheLinRegBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegBands LinRegBands(int length, int offset, PriceType myInput, double longEntrySd, double longExitSd, double shortEntrySd, double shortExitSd, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.LinRegBands(Input, length, offset, myInput, longEntrySd, longExitSd, shortEntrySd, shortExitSd, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public Indicators.LinRegBands LinRegBands(ISeries<double> input , int length, int offset, PriceType myInput, double longEntrySd, double longExitSd, double shortEntrySd, double shortExitSd, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.LinRegBands(input, length, offset, myInput, longEntrySd, longExitSd, shortEntrySd, shortExitSd, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegBands LinRegBands(int length, int offset, PriceType myInput, double longEntrySd, double longExitSd, double shortEntrySd, double shortExitSd, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.LinRegBands(Input, length, offset, myInput, longEntrySd, longExitSd, shortEntrySd, shortExitSd, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}

		public Indicators.LinRegBands LinRegBands(ISeries<double> input , int length, int offset, PriceType myInput, double longEntrySd, double longExitSd, double shortEntrySd, double shortExitSd, string longFilterOn, string longFilterOff, string shortFilterOn, string shortFilterOff)
		{
			return indicator.LinRegBands(input, length, offset, myInput, longEntrySd, longExitSd, shortEntrySd, shortExitSd, longFilterOn, longFilterOff, shortFilterOn, shortFilterOff);
		}
	}
}

#endregion
