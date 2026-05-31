//
// Copyright (C) 2020, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Displays net change on the chart.
	/// </summary>
	public class Zombie2NetPriceChange : Indicator
	{
		private const string SystemVersion = "v1.028";
		private const string SystemName = "Zombie2NetPriceChange";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		private Cbi.Account 	account;
		private Cbi.Instrument 	instrument;
		
		private double 			currentValue;
		private double 			lastValue;

		private DateTime currentDate = Core.Globals.MinDate;
		private double currentClose = 0;
		private double currentOpen = 0;
		private double priorDayClose = 0;
		private double priorDayOpen = 0;
		private Data.SessionIterator sessionIterator;

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
				Calculate					= Calculate.OnPriceChange;
				IsOverlay					= true;
				DrawOnPricePanel			= true;
				IsSuspendedWhileInactive	= true;
				Unit 						= Cbi.PerformanceUnit.Percent;
				PositiveBrush 				= Brushes.LimeGreen;
				NegativeBrush 				= Brushes.Red;
				Location 					= NetChangePosition.TopLeft;
				Font 						= new SimpleFont("Arial", 18);
				TextPrefix = "               ";
				TextSuffix = "";
			}
			else if (State == State.Configure)
			{
				instrument = Instruments[0];

				currentDate = Core.Globals.MinDate;
				currentClose = 0;
				currentOpen = 0;
				priorDayClose = 0;
				priorDayOpen = 0;
				sessionIterator = null;
			}
			else if (State == State.DataLoaded)
			{
				sessionIterator = new Data.SessionIterator(Bars);
			}
		}

		private TextPosition GetTextPosition(NetChangePosition ncp)
		{
			switch(ncp)
			{
				case NetChangePosition.BottomLeft:
					return TextPosition.BottomLeft;
				case NetChangePosition.BottomRight:
					return TextPosition.BottomRight;
				case NetChangePosition.TopLeft:
					return TextPosition.TopLeft;
				case NetChangePosition.TopRight:
					return TextPosition.TopRight;
			}

			return TextPosition.TopRight;
		}

		protected override void OnBarUpdate()
		{
			if (!Bars.BarsType.IsIntraday)
				return;

			// If the current data is not the same date as the current bar then its a new session
			if (currentDate != sessionIterator.GetTradingDay(Time[0]) || currentOpen == 0)
			{
				// The current day OHLC values are now the prior days value so set
				// them to their respect indicator series for plotting
				priorDayOpen = currentOpen;
				
				priorDayClose = currentClose;

				

				// Initilize the current day settings to the new days data
				currentOpen = Open[0];
				
				currentClose = Close[0];

				currentDate = sessionIterator.GetTradingDay(Time[0]);
			}
			else // The current day is the same day
			{
				// Set the current day OHLC values
				
				currentClose = Close[0];

				
			}
		}
		
		protected override void OnConnectionStatusUpdate(Cbi.ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.PriceStatus == Cbi.ConnectionStatus.Connected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Connecting
					&& connectionStatusUpdate.Connection.Accounts.Count > 0
					&& account == null)
				account = connectionStatusUpdate.Connection.Accounts[0];
			else if (connectionStatusUpdate.Status == Cbi.ConnectionStatus.Disconnected && connectionStatusUpdate.PreviousStatus == Cbi.ConnectionStatus.Disconnecting
					&& account != null && account.Connection == connectionStatusUpdate.Connection)
				account = null;
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
			{
				currentValue = double.MinValue;
				
				if (lastValue != currentValue)
				{
					Draw.TextFixed(this, "Z2NPC", "\n" + this.TextPrefix + FormatValue(currentValue) + this.TextSuffix, GetTextPosition(Location), currentValue >= 0 ? PositiveBrush : NegativeBrush, Font, Brushes.Transparent, Brushes.Transparent, 0);
					lastValue = currentValue;
				}
				return;
			}

			if (marketDataUpdate.MarketDataType != Data.MarketDataType.Last || marketDataUpdate.Instrument.MarketData.LastClose == null)
				return;

			bool 	tryAgainLater 	= false;
			double 	rate 			= 0;
			
			if (account != null)
				rate = marketDataUpdate.Instrument.GetConversionRate(Data.MarketDataType.Bid, account.Denomination, out tryAgainLater);

			double previousClosePrice = priorDayClose; //marketDataUpdate.Instrument.MarketData.LastClose.Price

			switch (Unit)
			{
				case Cbi.PerformanceUnit.Percent:
					currentValue = (marketDataUpdate.Price - previousClosePrice) / previousClosePrice;
					break;
				case Cbi.PerformanceUnit.Pips:
					currentValue = ((marketDataUpdate.Price - previousClosePrice) / Instrument.MasterInstrument.TickSize) * (Instrument.MasterInstrument.InstrumentType == Cbi.InstrumentType.Forex ? 0.1 : 1);
					break;
				case Cbi.PerformanceUnit.Ticks:
					currentValue = (marketDataUpdate.Price - previousClosePrice) / Instrument.MasterInstrument.TickSize;
					break;
				case Cbi.PerformanceUnit.Currency:
					currentValue = (marketDataUpdate.Price - previousClosePrice) * Instrument.MasterInstrument.PointValue * rate * (Instrument.MasterInstrument.InstrumentType == Cbi.InstrumentType.Forex ? (account != null ? account.ForexLotSize : Cbi.Account.DefaultLotSize) : 1);
					break;
				case Cbi.PerformanceUnit.Points:
					currentValue = (marketDataUpdate.Price - previousClosePrice);
					break;
			}

			//Print(DateTime.Now + ": " + "lastPrice=" + previousClosePrice.ToString() + " price=" + marketDataUpdate.Price.ToString());

			if (lastValue != currentValue)
			{
				Draw.TextFixed(this, "Z2NPC", "\n" + this.TextPrefix + FormatValue(currentValue) + this.TextSuffix, GetTextPosition(Location), currentValue >= 0 ? PositiveBrush : NegativeBrush, Font, Brushes.Transparent, Brushes.Transparent, 0);
				lastValue = currentValue;
			}
		}
		
		public string FormatValue(double value)
		{
			if (value == double.MinValue)
				return string.Empty;

			switch (Unit)
			{
				case Cbi.PerformanceUnit.Currency:
					{
                        Cbi.Currency formatCurrency;
                        if (account != null)
                            formatCurrency = account.Denomination;
                        else
                            formatCurrency = Instrument.MasterInstrument.Currency;
                        return Core.Globals.FormatCurrency(value, formatCurrency);
                    }
				case Cbi.PerformanceUnit.Points: return value.ToString(Core.Globals.GetTickFormatString(Instrument.MasterInstrument.TickSize), Core.Globals.GeneralOptions.CurrentCulture);
				case Cbi.PerformanceUnit.Percent: return (value).ToString("P", Core.Globals.GeneralOptions.CurrentCulture);
				case Cbi.PerformanceUnit.Pips:
					{
						System.Globalization.CultureInfo forexCulture = Core.Globals.GeneralOptions.CurrentCulture.Clone() as System.Globalization.CultureInfo;
						if (forexCulture != null)
							forexCulture.NumberFormat.NumberDecimalSeparator = "'";
						return (Math.Round(value * 10) / 10.0).ToString("0.0", forexCulture);
					}
				case Cbi.PerformanceUnit.Ticks: return Math.Round(value).ToString(Core.Globals.GeneralOptions.CurrentCulture);
				default: return "0";
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[XmlIgnore]
		[Browsable(false)]
		public double NetChangeValue {  get { return currentValue; } }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Unit", GroupName = "Parameters", Order = 0)]
		public Cbi.PerformanceUnit Unit { get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TextPrefix", GroupName = "Parameters", Order = 1)]
		public string TextPrefix { get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "TextSuffix", GroupName = "Parameters", Order = 2)]
		public string TextSuffix { get; set; }

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PositiveColor", GroupName = "Parameters", Order = 1810)]
		public Brush PositiveBrush { get; set; }
		
		[Browsable(false)]
		public string PositiveBrushSerialize
		{
			get { return Serialize.BrushToString(PositiveBrush); }
		   	set { PositiveBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NegativeColor", GroupName = "Parameters", Order = 1820)]
		public Brush NegativeBrush { get; set; }
		
		[Browsable(false)]
		public string NegativeBrushSerialize
		{
			get { return Serialize.BrushToString(NegativeBrush); }
		   	set { NegativeBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Location", GroupName = "Parameters", Order = 1830)]
		public NetChangePosition Location { get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Font", GroupName = "Parameters", Order = 1800)]
		public SimpleFont Font { get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2NetPriceChange[] cacheZombie2NetPriceChange;
		public Zombie2NetPriceChange Zombie2NetPriceChange(string indicatorName, Cbi.PerformanceUnit unit, string textPrefix, string textSuffix, NetChangePosition location)
		{
			return Zombie2NetPriceChange(Input, indicatorName, unit, textPrefix, textSuffix, location);
		}

		public Zombie2NetPriceChange Zombie2NetPriceChange(ISeries<double> input, string indicatorName, Cbi.PerformanceUnit unit, string textPrefix, string textSuffix, NetChangePosition location)
		{
			if (cacheZombie2NetPriceChange != null)
				for (int idx = 0; idx < cacheZombie2NetPriceChange.Length; idx++)
					if (cacheZombie2NetPriceChange[idx] != null && cacheZombie2NetPriceChange[idx].IndicatorName == indicatorName && cacheZombie2NetPriceChange[idx].Unit == unit && cacheZombie2NetPriceChange[idx].TextPrefix == textPrefix && cacheZombie2NetPriceChange[idx].TextSuffix == textSuffix && cacheZombie2NetPriceChange[idx].Location == location && cacheZombie2NetPriceChange[idx].EqualsInput(input))
						return cacheZombie2NetPriceChange[idx];
			return CacheIndicator<Zombie2NetPriceChange>(new Zombie2NetPriceChange(){ IndicatorName = indicatorName, Unit = unit, TextPrefix = textPrefix, TextSuffix = textSuffix, Location = location }, input, ref cacheZombie2NetPriceChange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2NetPriceChange Zombie2NetPriceChange(string indicatorName, Cbi.PerformanceUnit unit, string textPrefix, string textSuffix, NetChangePosition location)
		{
			return indicator.Zombie2NetPriceChange(Input, indicatorName, unit, textPrefix, textSuffix, location);
		}

		public Indicators.Zombie2NetPriceChange Zombie2NetPriceChange(ISeries<double> input , string indicatorName, Cbi.PerformanceUnit unit, string textPrefix, string textSuffix, NetChangePosition location)
		{
			return indicator.Zombie2NetPriceChange(input, indicatorName, unit, textPrefix, textSuffix, location);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2NetPriceChange Zombie2NetPriceChange(string indicatorName, Cbi.PerformanceUnit unit, string textPrefix, string textSuffix, NetChangePosition location)
		{
			return indicator.Zombie2NetPriceChange(Input, indicatorName, unit, textPrefix, textSuffix, location);
		}

		public Indicators.Zombie2NetPriceChange Zombie2NetPriceChange(ISeries<double> input , string indicatorName, Cbi.PerformanceUnit unit, string textPrefix, string textSuffix, NetChangePosition location)
		{
			return indicator.Zombie2NetPriceChange(input, indicatorName, unit, textPrefix, textSuffix, location);
		}
	}
}

#endregion
