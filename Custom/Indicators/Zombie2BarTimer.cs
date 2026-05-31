//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using System.Windows.Forms;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
	public class Zombie2BarTimer : Indicator
	{
		private const string SystemVersion = "v1.027";
		private const string SystemName = "Zombie2BarTimer";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		private string			timeLeft	= string.Empty;
		private DateTime		now		 	= Core.Globals.Now;
		private bool			connected,
								hasRealtimeData;
		private SessionIterator sessionIterator;

		private System.Windows.Threading.DispatcherTimer timer;

		public override string DisplayName
		{
			get { return FullSystemName; }
		}
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = SystemName;
				Description = FullSystemName;
				Calculate			= Calculate.OnEachTick;
				DrawOnPricePanel	= false;
				IsChartOnly			= true;
				IsOverlay			= true;
				DisplayInDataBox	= false;
				BasisTextLocation = TextPosition.BottomLeft;

				TextPrefix = "               ";
				TextSuffix = "";
			}
			else if (State == State.Realtime)
			{
				if (timer == null)
				{
					if (Bars.BarsType.IsTimeBased && Bars.BarsType.IsIntraday)
					{
						lock (Connection.Connections)
						{
							if (Connection.Connections.ToList().FirstOrDefault(c => c.Status == ConnectionStatus.Connected && c.InstrumentTypes.Contains(Instrument.MasterInstrument.InstrumentType)) == null)
								Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerDisconnectedError + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
							else
							{
								if (!SessionIterator.IsInSession(Now, false, true))
									Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerSessionTimeError + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
								else
									Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerWaitingOnDataError + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
							}
						}
					}
					else
						Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerTimeBasedError + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
				}
			}
			else if (State == State.Terminated)
			{
				if (timer == null)
					return;

				timer.IsEnabled = false;
				timer = null;
			}
		}

		protected override void OnBarUpdate()
		{
			if (State == State.Realtime)
			{
				hasRealtimeData = true;
				connected = true;
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Connected
				&& connectionStatusUpdate.Connection.InstrumentTypes.Contains(Instrument.MasterInstrument.InstrumentType)
				&& Bars.BarsType.IsTimeBased
				&& Bars.BarsType.IsIntraday)
			{
				connected = true;

				if (DisplayTime() && timer == null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						timer			= new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 1), IsEnabled = true };
						timer.Tick		+= OnTimerTick;
					});
				}
			}
			else if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Disconnected)
				connected = false;
		}

		private bool DisplayTime()
		{
			return ChartControl != null
					&& Bars != null
					&& Bars.Instrument.MarketData != null;
		}

		private void OnTimerTick(object sender, EventArgs e)
		{
			ForceRefresh();

			if (DisplayTime())
			{
				if (timer != null && !timer.IsEnabled)
					timer.IsEnabled = true;

				if (connected)
				{
					if (SessionIterator.IsInSession(Now, false, true))
					{
						if (hasRealtimeData)
						{
							TimeSpan barTimeLeft = Bars.GetTime(Bars.Count - 1).Subtract(Now);

							timeLeft = (barTimeLeft.Ticks < 0
								? "00:00:00"
								: barTimeLeft.Hours.ToString("00") + ":" + barTimeLeft.Minutes.ToString("00") + ":" + barTimeLeft.Seconds.ToString("00"));

							Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerTimeRemaining + timeLeft + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
						}
						else
							Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerWaitingOnDataError + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
					}
					else
						Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerSessionTimeError + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);
				}
				else
				{
					Draw.TextFixed(this, "Z2BT", this.TextPrefix + NinjaTrader.Custom.Resource.BarTimerDisconnectedError + this.TextSuffix, BasisTextLocation, ChartControl.Properties.ChartText, ChartControl.Properties.LabelFont, Brushes.Transparent, Brushes.Transparent, 0);

					if (timer != null)
						timer.IsEnabled = false;
				}
			}
		}

		private SessionIterator SessionIterator
		{
			get
			{
				if (sessionIterator == null)
					sessionIterator = new SessionIterator(Bars);
				return sessionIterator;
			}
		}

		private DateTime Now
		{
			get
			{
				now = (Cbi.Connection.PlaybackConnection != null ? Cbi.Connection.PlaybackConnection.Now : Core.Globals.Now);

				if (now.Millisecond > 0)
					now = Core.Globals.MinDate.AddSeconds((long)Math.Floor(now.Subtract(Core.Globals.MinDate).TotalSeconds));

				return now;
			}
		}

		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TextPrefix", GroupName = "Parameters", Order = 1)]
		public string TextPrefix { get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TextSuffix", GroupName = "Parameters", Order = 2)]
		public string TextSuffix { get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Chart location for basis data/info", Description = "", Order = 7, GroupName = "Parameters")]
		public NinjaTrader.NinjaScript.DrawingTools.TextPosition BasisTextLocation
		{ get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2BarTimer[] cacheZombie2BarTimer;
		public Zombie2BarTimer Zombie2BarTimer(string indicatorName, string textPrefix, string textSuffix, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return Zombie2BarTimer(Input, indicatorName, textPrefix, textSuffix, basisTextLocation);
		}

		public Zombie2BarTimer Zombie2BarTimer(ISeries<double> input, string indicatorName, string textPrefix, string textSuffix, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			if (cacheZombie2BarTimer != null)
				for (int idx = 0; idx < cacheZombie2BarTimer.Length; idx++)
					if (cacheZombie2BarTimer[idx] != null && cacheZombie2BarTimer[idx].IndicatorName == indicatorName && cacheZombie2BarTimer[idx].TextPrefix == textPrefix && cacheZombie2BarTimer[idx].TextSuffix == textSuffix && cacheZombie2BarTimer[idx].BasisTextLocation == basisTextLocation && cacheZombie2BarTimer[idx].EqualsInput(input))
						return cacheZombie2BarTimer[idx];
			return CacheIndicator<Zombie2BarTimer>(new Zombie2BarTimer(){ IndicatorName = indicatorName, TextPrefix = textPrefix, TextSuffix = textSuffix, BasisTextLocation = basisTextLocation }, input, ref cacheZombie2BarTimer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2BarTimer Zombie2BarTimer(string indicatorName, string textPrefix, string textSuffix, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2BarTimer(Input, indicatorName, textPrefix, textSuffix, basisTextLocation);
		}

		public Indicators.Zombie2BarTimer Zombie2BarTimer(ISeries<double> input , string indicatorName, string textPrefix, string textSuffix, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2BarTimer(input, indicatorName, textPrefix, textSuffix, basisTextLocation);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2BarTimer Zombie2BarTimer(string indicatorName, string textPrefix, string textSuffix, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2BarTimer(Input, indicatorName, textPrefix, textSuffix, basisTextLocation);
		}

		public Indicators.Zombie2BarTimer Zombie2BarTimer(ISeries<double> input , string indicatorName, string textPrefix, string textSuffix, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2BarTimer(input, indicatorName, textPrefix, textSuffix, basisTextLocation);
		}
	}
}

#endregion
