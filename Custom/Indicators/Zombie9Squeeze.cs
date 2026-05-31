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
namespace NinjaTrader.NinjaScript.Indicators.ZombiePack9
{
	/// <summary>
	/// Translate from Pine: Squeeze Momentum Indicator Author: [LazyBear]. Author Description:
	/// "This is a derivative of John Carter's "TTM Squeeze" volatility indicator, as discussed in his book 
	/// "Mastering the Trade" (chapter 11).	Black cicles on the midline show that the market just entered
	/// a squeeze ( Bollinger Bands are with in Keltner Channel). This signifies low volatility , market
	/// reparing itself for an explosive move (up or down). Gray cicles signify "Squeeze release".
	/// Mr.Carter suggests waiting till the first gray after a black cicle, and taking a position in the
	/// direction of the momentum (for ex., if momentum value is above zero, go long). Exit the position
	/// when the momentum changes (increase or decrease --- signified by a color change). My (limited)
	/// experience with this shows, an additional indicator like ADX / WaveTrend, is needed to not miss
	/// good entry points. Also, Mr.Carter uses simple momentum indicator , while I have used a
	/// different method (linreg based) to plot the histogram." 
	/// </summary>
	//This namespace holds Indicators in this folder and is required. Do not change it. 

//This namespace holds Indicators in this folder and is required. Do not change it. 
public class Zombie9Squeeze : Indicator
    {
        private const string SystemVersion = "v1.074";
        private const string SystemName = "Zombie9Squeeze";
        private const string FullSystemName = SystemName + " - " + SystemVersion;

        private int iMinBar; 
        private Series<double> data;
        private Series<double> chopValue;
        private Series<double> significantVolume;
        private int squeezeStartBar = -1;
        private bool squeezeInProgress = false;

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VolumeHistogram { get; private set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> isInSqueeze { get; private set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> isMomentumPositive { get; private set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> isMomentumNegative { get; private set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> isNeutral { get; private set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<bool> isNoSqueeze { get; private set; }

        public override string DisplayName
        {
            get
            {
                if (State == State.SetDefaults)
                    return FullSystemName;
                else if (ShowIndicatorName)
                    return FullSystemName;
                else
                    return "";
            }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = SystemName;
                Description = SystemName + " " + SystemVersion;
                Calculate = Calculate.OnPriceChange;
                IsOverlay = false;
                DisplayInDataBox = true;
                DrawOnPricePanel = false;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = false;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
                LengthBB = 20;
                MultBB = 0.38;
                LengthKC = 20;
                MultKC = 0.38;
                BrushUpBegin = Brushes.LightGreen;
                BrushUpEnd = Brushes.ForestGreen;
                BrushDownBegin = Brushes.Red;
                BrushDownEnd = Brushes.LightCoral;
                IsSqueeze = Brushes.DeepSkyBlue;
                NoSqueeze = Brushes.Transparent;

                AddPlot(new Stroke(Brushes.Transparent, 5), PlotStyle.Bar, "Histo");
                AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "InSqueeze");
                AddPlot(Brushes.Yellow, "ChopValue");
                AddPlot(Brushes.Magenta, "SignificantVolume");
                AddPlot(Brushes.Blue, "VolumeHistogram");
                AddPlot(Brushes.Green, "MomentumPositivePlot");
                AddPlot(Brushes.Red, "MomentumNegativePlot");
                AddPlot(Brushes.Gray, "NoSqueezePlot");
                AddPlot(Brushes.Black, "IsSqueezePlot");

                isInSqueeze = new Series<bool>(this);
                isMomentumPositive = new Series<bool>(this);
                isMomentumNegative = new Series<bool>(this);
                isNeutral = new Series<bool>(this);
                isNoSqueeze = new Series<bool>(this);

                Print("Indicator initialized.");
            }
            else if (State == State.Configure)
            {
                iMinBar = Math.Max(LengthBB, LengthKC) + 1;
                data = new Series<double>(this);
                chopValue = new Series<double>(this);
                significantVolume = new Series<double>(this);
                VolumeHistogram = new Series<double>(this);
                Print("Configuration complete. Minimum bar count: " + iMinBar.ToString());
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < iMinBar)
                return;

            double bbt = Bollinger(MultBB, LengthBB).Upper[0];
            double bbb = Bollinger(MultBB, LengthBB).Lower[0];
            double kct = KeltnerChannel(MultKC, LengthKC).Upper[0];
            double kcb = KeltnerChannel(MultKC, LengthKC).Lower[0];

            bool sqzOn = (bbb > kcb) && (bbt < kct);
            bool sqzOff = (bbb < kcb) && (bbt > kct);
            bool noSqz = !sqzOn && !sqzOff;

            double h = High[HighestBar(High, LengthKC)];
            double l = Low[LowestBar(Low, LengthKC)];
            double avg = (h + l) / 2;
            avg = (avg + (kct + kcb) / 2) / 2;
            data[0] = Close[0] - avg;

            InSqueeze[0] = 0.0;
            Histo[0] = LinReg(data, LengthKC)[0];

            if (Histo[0] > 0)
            {
                chopValue[0] = Histo[0];
                significantVolume[0] = 1;
                VolumeHistogram[0] = CalculateVolumeHistogram();
            }
            else
            {
                chopValue[0] = Histo[0];
                significantVolume[0] = 1;
                VolumeHistogram[0] = CalculateVolumeHistogram();
            }

            ChopValue[0] = chopValue[0];
            SignificantVolume[0] = significantVolume[0];

            if (Histo[0] > 0)
            {
                if (Histo[0] < Histo[1])
                    PlotBrushes[0][0] = BrushUpEnd;
                else
                    PlotBrushes[0][0] = BrushUpBegin;
            }
            else
            {
                if (Histo[0] > Histo[1])
                    PlotBrushes[0][0] = BrushDownEnd;
                else
                    PlotBrushes[0][0] = BrushDownBegin;
            }

            PlotBrushes[1][0] = (sqzOn) ? IsSqueeze : NoSqueeze;

            isInSqueeze[0] = sqzOn;
            isMomentumPositive[0] = Histo[0] > 0;
            isMomentumNegative[0] = Histo[0] < 0;
            isNeutral[0] = Histo[0] == 0;
            isNoSqueeze[0] = noSqz;

            NoSqueezePlot[0] = noSqz ? 1 : 0;
            IsSqueezePlot[0] = sqzOn ? 1 : 0;
            MomentumPositivePlot[0] = isMomentumPositive[0] ? Histo[0] : 0;
            MomentumNegativePlot[0] = isMomentumNegative[0] ? Histo[0] : 0;

            Print("Bar: " + CurrentBar.ToString() + " Histo: " + Histo[0].ToString() +
                " Squeeze: " + sqzOn.ToString() + " sqzOff: " + sqzOff.ToString() +
                " noSqz: " + noSqz.ToString() + " BBB: " + bbb.ToString() + " KCB: " + kcb.ToString() +
                " BBT: " + bbt.ToString() + " KCT: " + kct.ToString() +
                " Momentum Positive: " + isMomentumPositive[0].ToString());

            if (sqzOn && !squeezeInProgress)
            {
                squeezeStartBar = CurrentBar;
                squeezeInProgress = true;
            }
            else if (!sqzOn && squeezeInProgress)
            {
                Draw.RegionHighlightX(this, "squeezeRegion" + squeezeStartBar, squeezeStartBar, CurrentBar, Brushes.LightBlue, Brushes.LightBlue, 50);
                squeezeInProgress = false;
            }

            if (!sqzOn && squeezeInProgress)
            {
                squeezeInProgress = false;
            }
        }

        private double CalculateVolumeHistogram()
        {
            double volume = Volume[0];
            if (Histo[0] < 0)
                volume = -volume;
            return volume;
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
        public string IndicatorName
        {
            get { return FullSystemName; }
            set { }
        }

        [NinjaScriptProperty]
        [Display(Name = "ShowIndicatorName", GroupName = "0) Indicator Information", Order = 1)]
        public bool ShowIndicatorName
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="LengthBB", Description="Bollinger Bands Period", Order=1, GroupName="Parameters")]
        public int LengthBB
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name="MultBB", Description="Bollinger Bands MultFactor", Order=2, GroupName="Parameters")]
        public double MultBB
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="LengthKC", Description="Keltner Channel Period", Order=3, GroupName="Parameters")]
        public int LengthKC
        { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, double.MaxValue)]
        [Display(Name="MultKC", Description="Keltner Channel MultFactor", Order=4, GroupName="Parameters")]
        public double MultKC
        { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> Histo
        {
            get { return Values[0]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> InSqueeze
        {
            get { return Values[1]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> ChopValue
        {
            get { return Values[2]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> SignificantVolume
        {
            get { return Values[3]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> VolumeHistogramPlot
        {
            get { return Values[4]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MomentumPositivePlot
        {
            get { return Values[5]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> MomentumNegativePlot
        {
            get { return Values[6]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> NoSqueezePlot
        {
            get { return Values[7]; }
        }

        [Browsable(false)]
        [XmlIgnore]
        public Series<double> IsSqueezePlot
        {
            get { return Values[8]; }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "BrushUpBegin", Description = "Begin Bull Brush", Order = 5, GroupName = "Parameters")]
        public Brush BrushUpBegin
        { get; set; }

        [Browsable(false)]
        public string BrushUpBeginSerializable
        {
            get { return Serialize.BrushToString(BrushUpBegin); }
            set { BrushUpBegin = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BrushUpEnd", Description="End Bull Brush", Order=6, GroupName="Parameters")]
        public Brush BrushUpEnd
        { get; set; }

        [Browsable(false)]
        public string BrushUpEndSerializable
        {
            get { return Serialize.BrushToString(BrushUpEnd); }
            set { BrushUpEnd = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BrushDownBegin", Description="Begin Bear Brush", Order=7, GroupName="Parameters")]
        public Brush BrushDownBegin
        { get; set; }

        [Browsable(false)]
        public string BrushDownBeginSerializable
        {
            get { return Serialize.BrushToString(BrushDownBegin); }
            set { BrushDownBegin = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="BrushDownEnd", Description="End Bear Brush", Order=8, GroupName="Parameters")]
        public Brush BrushDownEnd
        { get; set; }

        [Browsable(false)]
        public string BrushDownEndSerializable
        {
            get { return Serialize.BrushToString(BrushDownEnd); }
            set { BrushDownEnd = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="IsSqueeze", Description="Is Squeeze", Order=10, GroupName="Parameters")]
        public Brush IsSqueeze
        { get; set; }

        [Browsable(false)]
        public string IsSqueezeSerializable
        {
            get { return Serialize.BrushToString(IsSqueeze); }
            set { IsSqueeze = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name="NoSqueeze", Description="No Squeeze", Order=11, GroupName="Parameters")]
        public Brush NoSqueeze
        { get; set; }

        [Browsable(false)]
        public string NoSqueezeSerializable
        {
            get { return Serialize.BrushToString(NoSqueeze); }
            set { NoSqueeze = Serialize.StringToBrush(value); }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZombiePack9.Zombie9Squeeze[] cacheZombie9Squeeze;
		public ZombiePack9.Zombie9Squeeze Zombie9Squeeze(string indicatorName, bool showIndicatorName, int lengthBB, double multBB, int lengthKC, double multKC, Brush brushUpBegin, Brush brushUpEnd, Brush brushDownBegin, Brush brushDownEnd, Brush isSqueeze, Brush noSqueeze)
		{
			return Zombie9Squeeze(Input, indicatorName, showIndicatorName, lengthBB, multBB, lengthKC, multKC, brushUpBegin, brushUpEnd, brushDownBegin, brushDownEnd, isSqueeze, noSqueeze);
		}

		public ZombiePack9.Zombie9Squeeze Zombie9Squeeze(ISeries<double> input, string indicatorName, bool showIndicatorName, int lengthBB, double multBB, int lengthKC, double multKC, Brush brushUpBegin, Brush brushUpEnd, Brush brushDownBegin, Brush brushDownEnd, Brush isSqueeze, Brush noSqueeze)
		{
			if (cacheZombie9Squeeze != null)
				for (int idx = 0; idx < cacheZombie9Squeeze.Length; idx++)
					if (cacheZombie9Squeeze[idx] != null && cacheZombie9Squeeze[idx].IndicatorName == indicatorName && cacheZombie9Squeeze[idx].ShowIndicatorName == showIndicatorName && cacheZombie9Squeeze[idx].LengthBB == lengthBB && cacheZombie9Squeeze[idx].MultBB == multBB && cacheZombie9Squeeze[idx].LengthKC == lengthKC && cacheZombie9Squeeze[idx].MultKC == multKC && cacheZombie9Squeeze[idx].BrushUpBegin == brushUpBegin && cacheZombie9Squeeze[idx].BrushUpEnd == brushUpEnd && cacheZombie9Squeeze[idx].BrushDownBegin == brushDownBegin && cacheZombie9Squeeze[idx].BrushDownEnd == brushDownEnd && cacheZombie9Squeeze[idx].IsSqueeze == isSqueeze && cacheZombie9Squeeze[idx].NoSqueeze == noSqueeze && cacheZombie9Squeeze[idx].EqualsInput(input))
						return cacheZombie9Squeeze[idx];
			return CacheIndicator<ZombiePack9.Zombie9Squeeze>(new ZombiePack9.Zombie9Squeeze(){ IndicatorName = indicatorName, ShowIndicatorName = showIndicatorName, LengthBB = lengthBB, MultBB = multBB, LengthKC = lengthKC, MultKC = multKC, BrushUpBegin = brushUpBegin, BrushUpEnd = brushUpEnd, BrushDownBegin = brushDownBegin, BrushDownEnd = brushDownEnd, IsSqueeze = isSqueeze, NoSqueeze = noSqueeze }, input, ref cacheZombie9Squeeze);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZombiePack9.Zombie9Squeeze Zombie9Squeeze(string indicatorName, bool showIndicatorName, int lengthBB, double multBB, int lengthKC, double multKC, Brush brushUpBegin, Brush brushUpEnd, Brush brushDownBegin, Brush brushDownEnd, Brush isSqueeze, Brush noSqueeze)
		{
			return indicator.Zombie9Squeeze(Input, indicatorName, showIndicatorName, lengthBB, multBB, lengthKC, multKC, brushUpBegin, brushUpEnd, brushDownBegin, brushDownEnd, isSqueeze, noSqueeze);
		}

		public Indicators.ZombiePack9.Zombie9Squeeze Zombie9Squeeze(ISeries<double> input , string indicatorName, bool showIndicatorName, int lengthBB, double multBB, int lengthKC, double multKC, Brush brushUpBegin, Brush brushUpEnd, Brush brushDownBegin, Brush brushDownEnd, Brush isSqueeze, Brush noSqueeze)
		{
			return indicator.Zombie9Squeeze(input, indicatorName, showIndicatorName, lengthBB, multBB, lengthKC, multKC, brushUpBegin, brushUpEnd, brushDownBegin, brushDownEnd, isSqueeze, noSqueeze);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZombiePack9.Zombie9Squeeze Zombie9Squeeze(string indicatorName, bool showIndicatorName, int lengthBB, double multBB, int lengthKC, double multKC, Brush brushUpBegin, Brush brushUpEnd, Brush brushDownBegin, Brush brushDownEnd, Brush isSqueeze, Brush noSqueeze)
		{
			return indicator.Zombie9Squeeze(Input, indicatorName, showIndicatorName, lengthBB, multBB, lengthKC, multKC, brushUpBegin, brushUpEnd, brushDownBegin, brushDownEnd, isSqueeze, noSqueeze);
		}

		public Indicators.ZombiePack9.Zombie9Squeeze Zombie9Squeeze(ISeries<double> input , string indicatorName, bool showIndicatorName, int lengthBB, double multBB, int lengthKC, double multKC, Brush brushUpBegin, Brush brushUpEnd, Brush brushDownBegin, Brush brushDownEnd, Brush isSqueeze, Brush noSqueeze)
		{
			return indicator.Zombie9Squeeze(input, indicatorName, showIndicatorName, lengthBB, multBB, lengthKC, multKC, brushUpBegin, brushUpEnd, brushDownBegin, brushDownEnd, isSqueeze, noSqueeze);
		}
	}
}

#endregion
