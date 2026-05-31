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
        public class VolBuySellMomentum_v3 : Indicator
        {
                private double                  xROC;
			    private Series<double>          nRes1, nRes2, nRes3, nResEMA3, PNVI_PEMA_Diff; 
			    Brush							lightUpColor, lightDownColor;
                protected override void OnStateChange()
                {
                        if (State == State.SetDefaults)
                        {
                                Description                                                                     = @"Volume Based Buy and Sell Momentum by 2tm, based on Siyeon's TV indicator. Converted by NPtechs.";
                                Name                                                                            = "VolBuySellMomentum_v3";
                                Calculate                                                                       = Calculate.OnBarClose;
                                IsOverlay                                                                       = false;
                                DisplayInDataBox                                                        		= true;
                                DrawOnPricePanel                                                       	 		= false;
                                DrawHorizontalGridLines                                         				= true;
                                DrawVerticalGridLines                                           				= true;
                                PaintPriceMarkers                                                       		= true;
                                ScaleJustification                                                      		= NinjaTrader.Gui.Chart.ScaleJustification.Right;
                                //Disable this property if your indicator requires custom values that cumulate with each new market data event.
                                //See Help Guide for additional information.
                                IsSuspendedWhileInactive                                        				= true;
                                ROC_MA                                      									= 25;
							    Delta_MA																		= 14;
								Avg_Delta_MA																	= 8;
							    bullBrush																		= Brushes.Lime;
								bearBrush																		= Brushes.Red;
							    noTrendBrush																	= Brushes.Gray;
							    ColorBars																		= true;
                                AddPlot(new Stroke(Brushes.Gray), PlotStyle.Bar, "PVI PEMA Diff Histogram");
								AddPlot(Brushes.Cyan, "PVI_NVI");
                                AddPlot(Brushes.White, "PEMA");
								Plots[0].AutoWidth = true;
                        }
                        else if (State == State.Configure)
                        {
                        }
                        else if (State == State.DataLoaded)
                        {
                                nRes1           = new Series<double>(this);
                                nRes2           = new Series<double>(this);
                                nRes3           = new Series<double>(this);
                                nResEMA3        = new Series<double>(this);
							    PNVI_PEMA_Diff  = new Series<double>(this);
                        }
                }

                protected override void OnBarUpdate()
                {
                        //Add your custom indicator logic here.
						if (CurrentBar < ROC_MA) return;
						if (lightUpColor == null)
						{
							lightUpColor = bullBrush.Clone();
							lightUpColor.Opacity = 0.4;
							lightUpColor.Freeze();
				
							lightDownColor = bearBrush.Clone();
							lightDownColor.Opacity = 0.4;
							lightDownColor.Freeze();
						}
						
                        xROC        = ROC(Close, 1)[0];
                        nRes1[0]    = (Bars.GetVolume(0) < Bars.GetVolume(1)) ? nRes1[1] + xROC : nRes1[1];
                        nRes2[0]    = (Bars.GetVolume(0) > Bars.GetVolume(1)) ? nRes2[1] + xROC : nRes2[1];
            			nRes3[0]    = nRes1[0] + nRes2[0];
                        nResEMA3[0] = SMA(nRes1, ROC_MA)[0] + SMA(nRes2, ROC_MA)[0];
						PNVI_PEMA_Diff[0] = nRes3[0] - nResEMA3[0];
						PNVI_PEMA_Diff_Hist[0] = PNVI_PEMA_Diff[0] * 0.5;
                        PVI_NVI[0]  = EMA(PNVI_PEMA_Diff,Delta_MA)[0];
                        PEMA[0]     = EMA(PVI_NVI,Avg_Delta_MA)[0];
						PlotBrushes[0][0] = PNVI_PEMA_Diff_Hist[0] > 0 && PNVI_PEMA_Diff_Hist[0] > PNVI_PEMA_Diff_Hist[1] ? Brushes.Lime 
											:  PNVI_PEMA_Diff_Hist[0] > 0 ? Brushes.DarkGreen 
											:  PNVI_PEMA_Diff_Hist[0] < 0 &&  PNVI_PEMA_Diff_Hist[0] <  PNVI_PEMA_Diff_Hist[1] ? Brushes.Red 
											: Brushes.DarkRed;
							
						if (ColorBars)
						{
							if (PNVI_PEMA_Diff[0] > 0 && PVI_NVI[0] > PEMA[0])
							{
								BarBrush = bullBrush;
								CandleOutlineBrush = bullBrush;
								if (Close[0]>Open[0]) BarBrush = lightUpColor;
							}
							else if (PNVI_PEMA_Diff[0] < 0 && PVI_NVI[0] < PEMA[0])
							{
								BarBrush = lightDownColor;
								CandleOutlineBrush =  bearBrush;
								if (Close[0] < Open[0]) BarBrush = bearBrush;
							}
							else
							{
								BarBrush = noTrendBrush;
								CandleOutlineBrush =  noTrendBrush;
							}
						}
                }

                #region Properties
                [NinjaScriptProperty]
                [Range(1, int.MaxValue)]
                [Display(Name="ROC MA Length", Order=1, GroupName="Parameters")]
                public int ROC_MA
                { get; set; }
				
				[Range(1, int.MaxValue)]
                [Display(Name="Delta MA Length", Order=2, GroupName="Parameters")]
                public int Delta_MA
                { get; set; }
				
				[Range(1, int.MaxValue)]
                [Display(Name="Avg Delta MA Length", Order=3, GroupName="Parameters")]
                public int Avg_Delta_MA
                { get; set; }

				[NinjaScriptProperty]
                [Display(Name = "ColorBars", GroupName = "Display", Order=1)]
                public bool ColorBars
                { get; set; }
				
				[XmlIgnore()]
				[NinjaScriptProperty]
                [Display(Name = "Candle UpColor", GroupName = "Display", Order=2)]
                public Brush bullBrush
                { get; set; }
				
				[XmlIgnore()]
				[NinjaScriptProperty]
                [Display(Name = "Candle DownColor", GroupName = "Display", Order=3)]
                public Brush bearBrush
                { get; set; }
				
				[XmlIgnore()]
				[NinjaScriptProperty]
                [Display(Name = "No Trend Candle Color", GroupName = "Display", Order=4)]
                public Brush noTrendBrush
                { get; set; }


				[Browsable(false)]
                [XmlIgnore()]
                public Series<double> PNVI_PEMA_Diff_Hist
                {
                        get { return Values[0]; }
                }
				
                [Browsable(false)]
                [XmlIgnore()]
                public Series<double> PVI_NVI
                {
                        get { return Values[1]; }
                }

                [Browsable(false)]
                [XmlIgnore()]
                public Series<double> PEMA
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
		private VolBuySellMomentum_v3[] cacheVolBuySellMomentum_v3;
		public VolBuySellMomentum_v3 VolBuySellMomentum_v3(int rOC_MA, bool colorBars, Brush bullBrush, Brush bearBrush, Brush noTrendBrush)
		{
			return VolBuySellMomentum_v3(Input, rOC_MA, colorBars, bullBrush, bearBrush, noTrendBrush);
		}

		public VolBuySellMomentum_v3 VolBuySellMomentum_v3(ISeries<double> input, int rOC_MA, bool colorBars, Brush bullBrush, Brush bearBrush, Brush noTrendBrush)
		{
			if (cacheVolBuySellMomentum_v3 != null)
				for (int idx = 0; idx < cacheVolBuySellMomentum_v3.Length; idx++)
					if (cacheVolBuySellMomentum_v3[idx] != null && cacheVolBuySellMomentum_v3[idx].ROC_MA == rOC_MA && cacheVolBuySellMomentum_v3[idx].ColorBars == colorBars && cacheVolBuySellMomentum_v3[idx].bullBrush == bullBrush && cacheVolBuySellMomentum_v3[idx].bearBrush == bearBrush && cacheVolBuySellMomentum_v3[idx].noTrendBrush == noTrendBrush && cacheVolBuySellMomentum_v3[idx].EqualsInput(input))
						return cacheVolBuySellMomentum_v3[idx];
			return CacheIndicator<VolBuySellMomentum_v3>(new VolBuySellMomentum_v3(){ ROC_MA = rOC_MA, ColorBars = colorBars, bullBrush = bullBrush, bearBrush = bearBrush, noTrendBrush = noTrendBrush }, input, ref cacheVolBuySellMomentum_v3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolBuySellMomentum_v3 VolBuySellMomentum_v3(int rOC_MA, bool colorBars, Brush bullBrush, Brush bearBrush, Brush noTrendBrush)
		{
			return indicator.VolBuySellMomentum_v3(Input, rOC_MA, colorBars, bullBrush, bearBrush, noTrendBrush);
		}

		public Indicators.VolBuySellMomentum_v3 VolBuySellMomentum_v3(ISeries<double> input , int rOC_MA, bool colorBars, Brush bullBrush, Brush bearBrush, Brush noTrendBrush)
		{
			return indicator.VolBuySellMomentum_v3(input, rOC_MA, colorBars, bullBrush, bearBrush, noTrendBrush);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolBuySellMomentum_v3 VolBuySellMomentum_v3(int rOC_MA, bool colorBars, Brush bullBrush, Brush bearBrush, Brush noTrendBrush)
		{
			return indicator.VolBuySellMomentum_v3(Input, rOC_MA, colorBars, bullBrush, bearBrush, noTrendBrush);
		}

		public Indicators.VolBuySellMomentum_v3 VolBuySellMomentum_v3(ISeries<double> input , int rOC_MA, bool colorBars, Brush bullBrush, Brush bearBrush, Brush noTrendBrush)
		{
			return indicator.VolBuySellMomentum_v3(input, rOC_MA, colorBars, bullBrush, bearBrush, noTrendBrush);
		}
	}
}

#endregion
