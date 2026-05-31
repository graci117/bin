// Decompiled with JetBrains decompiler
// Type: NinjaTrader.NinjaScript.Indicators.pjsReversalDetector
// Assembly: PJSReversalDetector, Version=1.0.0.3, Culture=neutral, PublicKeyToken=null
// MVID: FCF32674-45FD-477E-AE4A-D24BAD3E534D
// Assembly location: C:\Users\ngrac\Downloads\PJSReversalDetector\PJSReversalDetector.dll

using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
//using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace NinjaTrader.NinjaScript.Indicators
{
    public class pjsReversalDetector : Indicator
    {
        private double a;
        private double b;
        private int c;
        private int d;
        private NinjaTrader.NinjaScript.Indicators.Stochastics e;
        //private SpeechSynthesizer f;
        private Brush g = (Brush)Brushes.Red.Clone();
        private Brush h = (Brush)Brushes.LimeGreen.Clone();
        private int i;
        private int j;
        private int k;
        private int l;

        protected override void OnStateChange()
        {
            if (((NinjaTrader.NinjaScript.NinjaScript)this).State == State.SetDefaults)
            {
                Description = "Indicates posible reversal points";
                ((NinjaScriptBase)this).Name = "PJS Reversal Detector";
                ((NinjaScriptBase)this).Calculate = (Calculate)0;
                ((NinjaScriptBase)this).IsOverlay = true;
                ((NinjaScriptBase)this).DisplayInDataBox = false;
                ((IndicatorBase)this).DrawOnPricePanel = true;
                ((IndicatorBase)this).DrawHorizontalGridLines = true;
                ((IndicatorBase)this).DrawVerticalGridLines = true;
                ((IndicatorBase)this).PaintPriceMarkers = true;
                ((NinjaScriptBase)this).IsAutoScale = false;
                ((NinjaScriptBase)this).ScaleJustification = (ScaleJustification)1;
                IsSuspendedWhileInactive = true;
                this.Period = 50;
                this.K = 8;
                this.AlertsGoTime = DateTime.Parse("1:00 PM");
                this.AlertsEndTime = DateTime.Parse("8:45 PM");
                this.enableVoiceAlerts = true;
                this.alert_suffix = "";
                this.alert_message = "pjs Reversal signal";
                this.Paintbackground = true;
                this.RealTimeOnly = false;
                AddPlot((Brush)Brushes.Transparent, "Signal");
            }
            else if (((NinjaTrader.NinjaScript.NinjaScript)this).State == State.Configure)
            {
                //this.f = new SpeechSynthesizer();
                //this.f.SetOutputToDefaultAudioDevice();
                this.g.Opacity = this.h.Opacity = 0.2;
                this.g.Freeze();
                this.h.Freeze();
            }
            else if (((NinjaTrader.NinjaScript.NinjaScript)this).State == State.DataLoaded)
            {
                this.e = this.Stochastics(this.K, this.K * 2, 3);
            }
            else
            {
                if (((NinjaTrader.NinjaScript.NinjaScript)this).State != State.Terminated )
                    return;
               // this.f.Dispose();
            }
        }

       

        //private bool a(DateTime A_0) => A_0.TimeOfDay > this.AlertsGoTime.TimeOfDay && A_0.TimeOfDay < this.AlertsEndTime.TimeOfDay;

        protected override void OnBarUpdate()
        {
            if (((NinjaScriptBase)this).CurrentBar < this.Period)
                return;
            if (((NinjaScriptBase)this).IsFirstTickOfBar)
            {
                this.k = NinjaScriptBase.LowestBar(((NinjaScriptBase)this).Low, this.Period);
                this.l = NinjaScriptBase.HighestBar(((NinjaScriptBase)this).High, this.Period);
            }
            if (((NinjaScriptBase)this).Close[1] < ((NinjaScriptBase)this).Open[1] && ((NinjaScriptBase)this).Close[0] > ((NinjaScriptBase)this).Open[0] && this.a > 0.0 && ((NinjaScriptBase)this).Close[0] > this.a && this.d > 0 && ((NinjaScriptBase)this).CurrentBar - 3 <= this.d && this.k <= 3)
            {
                ArrowUp arrowUp = Draw.ArrowUp((NinjaScriptBase)this, "MyArrowUp" + (object)((NinjaScriptBase)this).CurrentBar, false, 0, ((NinjaScriptBase)this).Low[0] - ((NinjaScriptBase)this).TickSize, (Brush)Brushes.Green);
               
                if (this.Paintbackground)
                    ((NinjaScriptBase)this).BackBrush = this.h;
                this.Signal[0] = 1.0;
                if (this.i > ((NinjaScriptBase)this).CurrentBar - this.Period && this.i > 0 && this.e.K[0] > this.e.K[((NinjaScriptBase)this).CurrentBar - this.i])
                    arrowUp.AreaBrush = (Brush)Brushes.Lime;
                this.i = ((NinjaScriptBase)this).CurrentBar;
            }
            else if (((NinjaScriptBase)this).Close[1] > ((NinjaScriptBase)this).Open[1] && ((NinjaScriptBase)this).Close[0] < ((NinjaScriptBase)this).Open[0] && this.b > 0.0 && ((NinjaScriptBase)this).Close[0] < this.b && this.c > 0 && ((NinjaScriptBase)this).CurrentBar - 3 <= this.c && this.l <= 3)
            {
                if (!this.RealTimeOnly || ((NinjaTrader.NinjaScript.NinjaScript)this).State == State.Realtime && this.RealTimeOnly)
                {
                    ArrowDown arrowDown = Draw.ArrowDown((NinjaScriptBase)this, "MyArrowDn" + (object)((NinjaScriptBase)this).CurrentBar, false, 0, ((NinjaScriptBase)this).High[0] + ((NinjaScriptBase)this).TickSize, (Brush)Brushes.Red);
                    
                    if (this.Paintbackground)
                        ((NinjaScriptBase)this).BackBrush = this.g;
                    if (this.j > ((NinjaScriptBase)this).CurrentBar - this.Period && this.j > 0 && this.e.K[0] < this.e.K[((NinjaScriptBase)this).CurrentBar - this.j])
                        arrowDown.AreaBrush = (Brush)Brushes.Pink;
                }
                this.Signal[0] = -1.0;
                this.j = ((NinjaScriptBase)this).CurrentBar;
            }
            if (this.e.K[0] <= 20.0)
                this.d = ((NinjaScriptBase)this).CurrentBar;
            else if (this.e.K[0] >= 80.0)
                this.c = ((NinjaScriptBase)this).CurrentBar;
            if (((NinjaScriptBase)this).Close[0] > ((NinjaScriptBase)this).Open[0])
                this.b = ((NinjaScriptBase)this).Open[0];
            if (((NinjaScriptBase)this).Close[0] >= ((NinjaScriptBase)this).Open[0])
                return;
            this.a = ((NinjaScriptBase)this).Open[0];
        }

      
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get 
			{return Values[0];}
		}
		
        [NinjaScriptProperty]
        [Range(1, 2147483647)]
        [Display(Name = "Period", Order = 1, GroupName = "Parameters")]
        public int Period { get; set; }

        [Range(1, 2147483647)]
        [Display(Name = "K", Order = 10, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public int K { get; set; }

        [Display(Name = "Only place markers in realtime", Order = 30, GroupName = "Parameters")]
        [NinjaScriptProperty]
        public bool RealTimeOnly { get; set; }       


        [Display(Name = "Enable voice Alerts", GroupName = "Alerts", Order = 0)]
        public bool enableVoiceAlerts { get; set; }

        [Display(Name = "Announcement suffix", GroupName = "Alerts", Order = 3)]
        public string alert_suffix { get; set; }

        [Display(Name = "Message", GroupName = "Alerts", Order = 1)]
        public string alert_message { get; set; }

        [Display(Name = "paint background", GroupName = "Alerts", Order = 50)]
        public bool Paintbackground { get; set; }

        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Start Time", Order = 5, GroupName = "Alerts")]
        public DateTime AlertsGoTime { get; set; }

        [Display(Name = "End Time", Order = 6, GroupName = "Alerts")]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [NinjaScriptProperty]
        public DateTime AlertsEndTime { get; set; }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private pjsReversalDetector[] cachepjsReversalDetector;
		public pjsReversalDetector pjsReversalDetector(int period, int k, bool realTimeOnly, DateTime alertsEndTime)
		{
			return pjsReversalDetector(Input, period, k, realTimeOnly, alertsEndTime);
		}

		public pjsReversalDetector pjsReversalDetector(ISeries<double> input, int period, int k, bool realTimeOnly, DateTime alertsEndTime)
		{
			if (cachepjsReversalDetector != null)
				for (int idx = 0; idx < cachepjsReversalDetector.Length; idx++)
					if (cachepjsReversalDetector[idx] != null && cachepjsReversalDetector[idx].Period == period && cachepjsReversalDetector[idx].K == k && cachepjsReversalDetector[idx].RealTimeOnly == realTimeOnly && cachepjsReversalDetector[idx].AlertsEndTime == alertsEndTime && cachepjsReversalDetector[idx].EqualsInput(input))
						return cachepjsReversalDetector[idx];
			return CacheIndicator<pjsReversalDetector>(new pjsReversalDetector(){ Period = period, K = k, RealTimeOnly = realTimeOnly, AlertsEndTime = alertsEndTime }, input, ref cachepjsReversalDetector);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.pjsReversalDetector pjsReversalDetector(int period, int k, bool realTimeOnly, DateTime alertsEndTime)
		{
			return indicator.pjsReversalDetector(Input, period, k, realTimeOnly, alertsEndTime);
		}

		public Indicators.pjsReversalDetector pjsReversalDetector(ISeries<double> input , int period, int k, bool realTimeOnly, DateTime alertsEndTime)
		{
			return indicator.pjsReversalDetector(input, period, k, realTimeOnly, alertsEndTime);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.pjsReversalDetector pjsReversalDetector(int period, int k, bool realTimeOnly, DateTime alertsEndTime)
		{
			return indicator.pjsReversalDetector(Input, period, k, realTimeOnly, alertsEndTime);
		}

		public Indicators.pjsReversalDetector pjsReversalDetector(ISeries<double> input , int period, int k, bool realTimeOnly, DateTime alertsEndTime)
		{
			return indicator.pjsReversalDetector(input, period, k, realTimeOnly, alertsEndTime);
		}
	}
}

#endregion
