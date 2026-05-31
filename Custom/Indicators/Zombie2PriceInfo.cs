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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Globalization;
using System.Collections.Concurrent;
using NTDrawingTools = NinjaTrader.NinjaScript.DrawingTools;
using Infragistics.Windows.DataPresenter;
using SharpDX.DirectWrite;
using SharpDX;
using Direct2D1 = SharpDX.Direct2D1;
using NinjaTrader.NinjaScript.Indicators.ZPI;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class Zombie2PriceInfo : Indicator
	{
		private const string SystemVersion = "v1.028";
		private const string SystemName = "Zombie2PriceInfo";
		private const string FullSystemName = SystemName + " - " + SystemVersion;
		private const string LabelInitializeText = "*";
		private const string LabelEmptyText = " ";
		private bool hasRanOnceFirstCycle = false;
		private int lastPrintOutputHashCode = 0;

		private Dictionary<string, DXMediaMap> dxmBrushes;
		private SharpDX.Direct2D1.RenderTarget myRenderTarget = null;

		private RealInstrumentService RealInstrumentService = new RealInstrumentService();

		private string lastPriceQuote = "0";
		private string lastSpreadTicksText = "0";
		private double lastIntradayMargin = 0;
		private double lastMaintenanceMargin = 0;
		private double lastInstrumentTotalPositionPnL = 0;
		private int lastInstrumentTotalPositionTicks = 0;
		private string instrumentTotalPositionTicksText = "";
		private string instrumentTotalPositionATRRatioText = "";

		private Account account = null;
		string lastTextLineTop1 = "";
		string lastTextLineTop2 = "";
		string lastTextLineTop3 = "";
		string lastTextLineBottom1 = "";
		string lastTextLineBottom2 = "";
		string lastTextLineBottom3 = "";
		private SimpleFont defaultFont = null;
		private SimpleFont priceQuoteFont = null;
		//private NTDrawingTools.TextFixed field1 = null;
		//private NTDrawingTools.TextFixed field2 = null;
		private Brush transparentBrush = Brushes.Transparent;
		private readonly object renderLock = new object();
		//private SharpDX.Direct2D1.Brush dxBrush = null;
		private System.Windows.Threading.DispatcherTimer timer;
		private Instrument attachedInstrument = null;
		private bool attachedInstrumentIsFuture = false;
		private bool attachedInstrumentServerSupported = false;
		private double attachedInstrumentTickValue = 0;
		private double attachedInstrumentTickSize = 0;
		private Instrument micro1Instrument = null;
		private Instrument micro2Instrument = null;
		private Instrument micro3Instrument = null;
		private Instrument micro4Instrument = null;
		private Instrument emini1Instrument = null;
		private Instrument emini2Instrument = null;
		private Instrument emini3Instrument = null;
		private Instrument emini4Instrument = null;
		private bool instrumentsSubscribed = false;
		
		private const string MYMPrefix = "MYM";
		private const string MESPrefix = "MES";
		private const string M2KPrefix = "M2K";
		private const string MNQPrefix = "MNQ";

		
		private const string YMPrefix = "YM";
		private const string ESPrefix = "ES";
		private const string RTYPrefix = "RTY";
		private const string NQPrefix = "NQ";


		private bool hasPositions = false;
		private double intradayMargin = 0;
		private double maintenanceMargin = 0;
		private string priceQuote = "";
		private string spreadTicksText = "";
		private string instrumentTotalPositionPnLText = "";
		private double accountTotalPositionPnL = 0;
		private string accountTotalPositionPnLText = "";
		private bool accountHasNegativePnL = false;

		private ATR atrBuffer;
		private double atrValue = 0;
		private double previousHigh = 0;
		private double previousLow = 0;

		string m_spot1LastText = "";
		string m_spot2LastText = "";
		string m_spot3LastText = "";

		Brush priceTextColor = Brushes.Silver;
		Brush inProfitColor = Brushes.LimeGreen;
		Brush inLossColor = Brushes.Red;

		Brush m_spot1LastColor = Brushes.Transparent;
		Brush m_spot2LastColor = Brushes.Transparent;
		Brush m_spot3LastColor = Brushes.Transparent;

		public override string DisplayName
		{
			get { return FullSystemName; }
		}
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = FullSystemName;
				Name = SystemName;
				Calculate = Calculate.OnPriceChange;
				IsChartOnly = true;
				IsSuspendedWhileInactive = true;
				DisplayInDataBox = false;
				DrawOnPricePanel = false;
				DrawHorizontalGridLines = false;
				DrawVerticalGridLines = false;
				PaintPriceMarkers = false;
				MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;

				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;

				PriceTextColor = Brushes.Silver;
				InProfitColor = Brushes.LimeGreen;
				InLossColor = Brushes.Red; 
				ATRPeriod = 21;
				
				BasisTextLocation = TextPosition.BottomRight;

				dxmBrushes = new Dictionary<string, DXMediaMap>();

			}
			else if (State == State.Configure)
			{
				attachedInstrument = this.Instrument;
				attachedInstrumentIsFuture = IsFutureInstrumentType(this.attachedInstrument);
				attachedInstrumentServerSupported = this.Instrument.MasterInstrument.IsServerSupported;

				if (attachedInstrumentServerSupported)
				{
					priceTextColor.Freeze();
					inProfitColor.Freeze();
					inLossColor.Freeze();
					//transparentBrush.Freeze();


					if (attachedInstrumentIsFuture)
					{
						attachedInstrumentTickValue = GetTickValue(this.Instrument);
						attachedInstrumentTickSize = RealInstrumentService.GetTickSize(attachedInstrument);

						string micro1FullName = MYMPrefix + GetCurrentFuturesMonthYearPrefix();
						string micro2FullName = MESPrefix + GetCurrentFuturesMonthYearPrefix();
						string micro3FullName = M2KPrefix + GetCurrentFuturesMonthYearPrefix();
						string micro4FullName = MNQPrefix + GetCurrentFuturesMonthYearPrefix();

						string emini1FullName = YMPrefix + GetCurrentFuturesMonthYearPrefix();
						string emini2FullName = ESPrefix + GetCurrentFuturesMonthYearPrefix();
						string emini3FullName = RTYPrefix + GetCurrentFuturesMonthYearPrefix();
						string emini4FullName = NQPrefix + GetCurrentFuturesMonthYearPrefix();
						// BarsArray[0] is default of chart we are on



						if (this.Instrument.FullName != micro1FullName)
						{
							ValidateInstrument(micro1FullName);
							//AddDataSeries(micro1FullName, BarsPeriodType.Minute, 5);
						}

						if (this.Instrument.FullName != micro2FullName)
						{
							ValidateInstrument(micro2FullName);
							//AddDataSeries(micro2FullName, BarsPeriodType.Minute, 5);
						}

						if (this.Instrument.FullName != micro3FullName)
						{
							ValidateInstrument(micro3FullName);
							//AddDataSeries(micro3FullName, BarsPeriodType.Minute, 5);
						}

						if (this.Instrument.FullName != micro4FullName)
						{
							ValidateInstrument(micro4FullName);
							//AddDataSeries(micro4FullName, BarsPeriodType.Minute, 5);
						}

						if (this.Instrument.FullName != emini1FullName)
						{
							ValidateInstrument(emini1FullName);
							//AddDataSeries(micro1FullName, BarsPeriodType.Minute, 5);
						}

						if (this.Instrument.FullName != emini2FullName)
						{
							ValidateInstrument(emini2FullName);
							//AddDataSeries(emini2FullName, BarsPeriodType.Minute, 5);
						}

						if (this.Instrument.FullName != emini3FullName)
						{
							ValidateInstrument(emini3FullName);
							//AddDataSeries(emini3FullName, BarsPeriodType.Minute, 5);
						}

						if (this.Instrument.FullName != emini4FullName)
						{
							ValidateInstrument(emini4FullName);
							//AddDataSeries(emini4FullName, BarsPeriodType.Minute, 5);
						}

						micro1Instrument = Instrument.GetInstrument(micro1FullName);
						micro2Instrument = Instrument.GetInstrument(micro2FullName);
						micro3Instrument = Instrument.GetInstrument(micro3FullName);
						micro4Instrument = Instrument.GetInstrument(micro4FullName);

						emini1Instrument = Instrument.GetInstrument(emini1FullName);
						emini2Instrument = Instrument.GetInstrument(emini2FullName);
						emini3Instrument = Instrument.GetInstrument(emini3FullName);
						emini4Instrument = Instrument.GetInstrument(emini4FullName);

						if (!instrumentsSubscribed)
						{
							if (micro1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro1Instrument, "MarketDataUpdate", MarketData_Update);
							if (micro2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro2Instrument, "MarketDataUpdate", MarketData_Update);
							if (micro3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro3Instrument, "MarketDataUpdate", MarketData_Update);
							if (micro4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(micro4Instrument, "MarketDataUpdate", MarketData_Update);

							if (emini1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini1Instrument, "MarketDataUpdate", MarketData_Update);
							if (emini2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini2Instrument, "MarketDataUpdate", MarketData_Update);
							if (emini3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini3Instrument, "MarketDataUpdate", MarketData_Update);
							if (emini4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.AddHandler(emini4Instrument, "MarketDataUpdate", MarketData_Update);
							instrumentsSubscribed = true;
						}
					}

					atrValue = 0;
					previousHigh = 0;
					previousLow = 0;
				}
			}
			else if (State == State.DataLoaded)
			{
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab1);
				PrintOutput("Loading " + SystemVersion + " on " + this.attachedInstrument.FullName + " (" + BarsPeriod + ")", PrintTo.OutputTab2);

				if (!attachedInstrumentServerSupported)
                {
					PrintOutput("Detecting instrument not enabled for real-time quotes and trading.", PrintTo.OutputTab1);
					PrintOutput("Detecting instrument not enabled for real-time quotes and trading.", PrintTo.OutputTab2);
				}

				if (attachedInstrumentServerSupported)
				{
					atrBuffer = ATR(ATRPeriod);

					if (BarsInProgress == 0 && ChartControl != null && timer == null)
					{
						ChartControl.Dispatcher.InvokeAsync(() =>
						{
							timer = new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50), IsEnabled = true };
							WeakEventManager<System.Windows.Threading.DispatcherTimer, EventArgs>.AddHandler(timer, "Tick", OnTimerTick);
						});
					}
				}
			}
			else if (State == State.Terminated)
			{
				if (attachedInstrumentServerSupported)
				{
					hasRanOnceFirstCycle = false;

					foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
						if (item.Value.DxBrush != null)
							item.Value.DxBrush.Dispose();

					if (ChartControl != null && timer != null)
					{
						ChartControl.Dispatcher.InvokeAsync(() =>
						{
							WeakEventManager<System.Windows.Threading.DispatcherTimer, EventArgs>.RemoveHandler(timer, "Tick", OnTimerTick);
							timer = null;
						});
					}

					if (attachedInstrumentIsFuture)
					{
						if (instrumentsSubscribed)
						{
							if (micro1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro1Instrument, "MarketDataUpdate", MarketData_Update);
							if (micro2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro2Instrument, "MarketDataUpdate", MarketData_Update);
							if (micro3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro3Instrument, "MarketDataUpdate", MarketData_Update);
							if (micro4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(micro4Instrument, "MarketDataUpdate", MarketData_Update);

							if (emini1Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini1Instrument, "MarketDataUpdate", MarketData_Update);
							if (emini2Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini2Instrument, "MarketDataUpdate", MarketData_Update);
							if (emini3Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini3Instrument, "MarketDataUpdate", MarketData_Update);
							if (emini4Instrument != null) WeakEventManager<Instrument, MarketDataEventArgs>.RemoveHandler(emini4Instrument, "MarketDataUpdate", MarketData_Update);
							instrumentsSubscribed = false;
						}

					}
					//if (field1 != null)
					//{
					//	field1.Dispose();
					//	field1 = null;
					//}

					//if (field2 != null)
					//{
					//	field2.Dispose();
					//	field2 = null;
					//}
				}
			}

		}

        private void MarketData_Update(object sender, MarketDataEventArgs e)
        {
			if (e.Instrument != null)
			{
				double lastPrice = 0;
				double newPrice = 0;
				if (e.MarketDataType == MarketDataType.Ask)
				{
					lastPrice = RealInstrumentService.GetAskPrice(e.Instrument);
					newPrice = e.Ask;

					if (lastPrice != newPrice)
					{
						
						RealInstrumentService.SetAskPrice(e.Instrument, newPrice);
					}
				}
				else if (e.MarketDataType == MarketDataType.Bid)
				{
					lastPrice = RealInstrumentService.GetBidPrice(e.Instrument);
					newPrice = e.Bid;

					if (lastPrice != newPrice)
					{
						
						RealInstrumentService.SetBidPrice(e.Instrument, newPrice);
					}
				}
				else if (e.MarketDataType == MarketDataType.Last)
				{
					lastPrice = RealInstrumentService.GetLastPrice(e.Instrument);
					newPrice = e.Last;

					if (lastPrice != newPrice)
					{
						
						RealInstrumentService.SetLastPrice(e.Instrument, newPrice);
					}
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (attachedInstrumentServerSupported)
			{
				RefreshAccount();

				if (CurrentBar > ATRPeriod)
				{
					atrValue = atrBuffer[0];
					previousHigh = High[1];
					previousLow = Low[1];
				}
			}
		}

		private void RefreshAccount()
        {
			if (hasRanOnceFirstCycle)
			{
				Account tempAccount = GetAccount();
				if (account != null & tempAccount != account)
				{
					hasRanOnceFirstCycle = false;
				}
			}
		}

		private void OnTimerTick(object sender, EventArgs e)
		{
			//try
			//{
				if (HasRanOnceFirstCycle())
				{
					//PrintOutput(this.attachedInstrument.FullName + ": askprice = " + askPrice.ToString() + " bidPrice= " + bidPrice.ToString());

					PrepToDrawOnChart();
					ForceRefresh();
				}
				
			//}
			//catch (Exception ex)
			//{
			//	Log("Exception in OnTimerTick:" + ex.Message + " " + ex.StackTrace, LogLevel.Error);
				//PrintOutput("Exception in OnTimerTick:" + ex.Message + " " + ex.StackTrace);  //log and ignore exception
			//}
		}
		

		private string GetPriceQuote()
		{
			double bidPrice = RealInstrumentService.GetBidPrice(this.attachedInstrument);
			if ( bidPrice > 0)
			{
				lastPriceQuote = ConvertToFixedPoint(bidPrice, this.attachedInstrument);
			}

			return lastPriceQuote;
		}

		private string GetSpreadTicksText()
		{
			
			lastSpreadTicksText = Convert.ToString(GetTicks(this.attachedInstrument, GetSpread(this.attachedInstrument))) + " spread";

			return lastSpreadTicksText;
		}

		private double GetInstrumentTotalPositionPnL()
		{
			bool tempHasPositions = false;
			lastInstrumentTotalPositionTicks = 0;
			lastInstrumentTotalPositionPnL = GetTotalPositionPnL(true, out tempHasPositions, out lastInstrumentTotalPositionTicks);

			return lastInstrumentTotalPositionPnL;
		}

		private double GetIntradayMargin()
		{
			if (account != null)
				lastIntradayMargin = Math.Round(account.Get(AccountItem.IntradayMargin, Currency.UsDollar), 2);

			return lastIntradayMargin;
		}

		private double GetMaintenanceMargin()
		{
			if (account != null)
				lastMaintenanceMargin = Math.Round(account.Get(AccountItem.MaintenanceMargin, Currency.UsDollar), 2);
			
			return lastMaintenanceMargin;
		}

		private void PrepToDrawOnChart()
		{
			//PrintOutput("PrepToDrawOnChart");
			
			priceQuote = GetPriceQuote();
			spreadTicksText = GetSpreadTicksText();
			double instrumentTotalPositionPnL = GetInstrumentTotalPositionPnL();
			int atrTicks = GetTicks(attachedInstrument, atrValue);
			double tempATRValue = (atrTicks > 0) ? atrTicks : 1;

			if (instrumentTotalPositionPnL < 0)
			{
				instrumentTotalPositionPnLText = "-$" + instrumentTotalPositionPnL.ToString("N2").Replace("-", "") + " pnl";
				instrumentTotalPositionTicksText = "(" + lastInstrumentTotalPositionTicks.ToString("N0") + ") ticks";
				instrumentTotalPositionATRRatioText = "(" + ((double)lastInstrumentTotalPositionTicks / tempATRValue).ToString("N1") + ") ratio";
			}
			else
            {
				instrumentTotalPositionPnLText = "$" + instrumentTotalPositionPnL.ToString("N2") + " pnl";
				instrumentTotalPositionTicksText = lastInstrumentTotalPositionTicks.ToString("N0") + " ticks";
				instrumentTotalPositionATRRatioText = ((double)lastInstrumentTotalPositionTicks / tempATRValue).ToString("N1") + " ratio";
			}
			

			int tickCount = 0;
			accountTotalPositionPnL = GetTotalPositionPnL(false, out hasPositions, out tickCount);
			accountTotalPositionPnLText = "$" + accountTotalPositionPnL.ToString("N2");
			accountHasNegativePnL = false;
			if (accountTotalPositionPnL < 0)
			{
				accountHasNegativePnL = true;
				accountTotalPositionPnLText = "-$" + accountTotalPositionPnL.ToString("N2").Replace("-", "");

			}
				
			if (account != null)
			{
				intradayMargin = GetIntradayMargin();
				maintenanceMargin = GetMaintenanceMargin();
			}

		}

		string GetBarTickText()
        {
			string tempText = "";

			int previousBarSize = 0;
			if (previousHigh > 0 && previousLow > 0) previousBarSize = GetTicks(attachedInstrument, previousHigh - previousLow);

			tempText = previousBarSize.ToString("N0") + " bar";

			return tempText;
		}

		string GetATRTickText()
        {
			string tempText = "";
			
			int previousBarSize = 0;
			if (previousHigh > 0 && previousLow > 0) previousBarSize = GetTicks(attachedInstrument, previousHigh - previousLow);

			int atrTicks = GetTicks(attachedInstrument, atrValue);
			double atrTicksValue = atrTicks * attachedInstrumentTickValue;

			tempText = atrTicks.ToString("N0") + " atr / $" + atrTicksValue.ToString("N0") + " value";
			

			return tempText;
		}

		private void DrawTextOnChart(ChartControl chartControl)
        {
			//PrintOutput("DrawTextOnChart x" + Convert.ToString(hasRanOnceFirstCycle));
			Brush pnlBrush;

			if (!hasPositions)
			{
				pnlBrush = this.priceTextColor;
			}
			else if (accountHasNegativePnL)
			{
				pnlBrush = this.inLossColor;
			}
			else
			{
				pnlBrush = this.inProfitColor;
			}

			if (BasisTextLocation == TextPosition.Center)
            {
				string textLineTop1 = "Only TopLeft / TopRight / BottomLeft / BottomRight locations supported by PriceInfo";

				int fontSize = 13;
				if (defaultFont == null)
				{
					defaultFont = new SimpleFont(chartControl.Properties.LabelFont.Family.ToString(), fontSize);
				}

				DrawSpot1(chartControl, textLineTop1, defaultFont, this.priceTextColor, Brushes.Transparent);
			}
			else if (BasisTextLocation == TextPosition.TopRight || BasisTextLocation == TextPosition.TopLeft)
			{
				string textLine1 = priceQuote + "";
				string textLine2 = "\n\n" + spreadTicksText + " / " + GetBarTickText() + "\n" + GetATRTickText();
				if (hasPositions) textLine2 += "\n---------------------";
				string textLine3 = "\n\n\n\n\n\n" + accountTotalPositionPnLText + " / " + instrumentTotalPositionPnLText + " " + "\n " + instrumentTotalPositionTicksText + " / " + instrumentTotalPositionATRRatioText + " \n " + "$" + maintenanceMargin.ToString("N0") + " / $" + intradayMargin.ToString("N0") + " margin ";


				int fontSize = 13;
				if (defaultFont == null)
				{
					defaultFont = new SimpleFont(chartControl.Properties.LabelFont.Family.ToString(), fontSize);
				}
				if (priceQuoteFont == null)
				{
					priceQuoteFont = new SimpleFont(chartControl.Properties.LabelFont.Family.ToString(), fontSize + 8) { Bold = true };
				}

				

				if (hasPositions)
				{
					DrawSpot3(chartControl, textLine3, defaultFont, pnlBrush, Brushes.Transparent);
				}
				else
				{
					DrawSpot3(chartControl, LabelEmptyText, defaultFont, this.priceTextColor, Brushes.Transparent);
				}


				DrawSpot2(chartControl, textLine2, defaultFont, this.priceTextColor, Brushes.Transparent);

				DrawSpot1(chartControl, textLine1, priceQuoteFont, this.priceTextColor, Brushes.Transparent);

			}
			else if (BasisTextLocation == TextPosition.BottomLeft || BasisTextLocation == TextPosition.BottomRight)
			{


				string textLine1 = priceQuote + "\n\n\n\n";
				string textLine2 = spreadTicksText + " / " + GetBarTickText() + "\n" + GetATRTickText() + "\n\n\n\n";
				string textLine3 = " " + accountTotalPositionPnLText + " / " + instrumentTotalPositionPnLText + " " + "\n " + instrumentTotalPositionTicksText + " / " + instrumentTotalPositionATRRatioText + " \n " + "$" + maintenanceMargin.ToString("N0") + " / $" + intradayMargin.ToString("N0") + " margin ";

				
				int fontSize = 13;
				if (defaultFont == null)
				{
					defaultFont = new SimpleFont(chartControl.Properties.LabelFont.Family.ToString(), fontSize);
				}
				if (priceQuoteFont == null)
				{
					priceQuoteFont = new SimpleFont(chartControl.Properties.LabelFont.Family.ToString(), fontSize + 8) { Bold = true };
				}

				//DrawString("Test", priceQuoteFont, PriceTextColor, 0, 0);

				//Draw.TextFixed(this, "testa", lastTextLineBottom1, BasisTextLocation, chartControl.Properties.ChartText, priceQuoteFont, Brushes.Transparent, chartControl.Properties.ChartBackground, 50, DashStyleHelper.Solid, 2, false, "");

				DrawSpot1(chartControl, textLine1, priceQuoteFont, this.priceTextColor, Brushes.Transparent);

				DrawSpot2(chartControl, textLine2, defaultFont, this.priceTextColor, Brushes.Transparent);

				if (hasPositions)
				{
					DrawSpot3(chartControl, textLine3, defaultFont, pnlBrush, pnlBrush);
				}
				else
				{
					DrawSpot3(chartControl, LabelEmptyText, defaultFont, this.priceTextColor, Brushes.Transparent);
				}
			}
		}

		#region SharpDX Helper Classes/Methods

		[Browsable(false)]
		public class DXMediaMap
		{
			public SharpDX.Direct2D1.Brush DxBrush;
			public System.Windows.Media.Brush MediaBrush;
		}

		private void SetOpacity(string brushName)
		{
			if (dxmBrushes[brushName].MediaBrush == null)
				return;

			if (dxmBrushes[brushName].MediaBrush.IsFrozen)
				dxmBrushes[brushName].MediaBrush = dxmBrushes[brushName].MediaBrush.Clone();

			dxmBrushes[brushName].MediaBrush.Opacity = 100.0 / 100.0;
			dxmBrushes[brushName].MediaBrush.Freeze();
		}

		private void UpdateBrush(Brush mediaBrush, string brushName)
		{
			dxmBrushes[brushName].MediaBrush = mediaBrush;
			SetOpacity(brushName);
			if (dxmBrushes[brushName].DxBrush != null)
				dxmBrushes[brushName].DxBrush.Dispose();
			if (RenderTarget != null && !RenderTarget.IsDisposed)
			{
				dxmBrushes[brushName].DxBrush = dxmBrushes[brushName].MediaBrush.ToDxBrush(RenderTarget);
			}

		}

		private void DrawString(string text, SimpleFont font, string brushName, double pointX, double pointY, string areaBrushName)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX, pointY).ToVector2();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, ChartPanel.X + ChartPanel.W,
				textFormat.FontSize);

			float newW = textLayout.Metrics.Width;
			float newH = textLayout.Metrics.Height;
			SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF((float)pointX + 2, (float)pointY, newW + 5, newH + 2);
			RenderTarget.FillRectangle(PLBoundRect, dxmBrushes[areaBrushName].DxBrush);

			RenderTarget.DrawTextLayout(TextPlotPoint, textLayout, dxmBrushes[brushName].DxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textLayout.Dispose();
			textFormat.Dispose();
		}

		private void DrawString(string text, SimpleFont font, string brushName, double pointX, double pointY)
		{
			DrawString(text, font, brushName, pointX, pointY, "TransparentBrush");
		}
		#endregion



		private void DrawSpot1(ChartControl chartControl, string text, SimpleFont textFont, Brush textColor, Brush outlineColor)
		{
			if (text != m_spot1LastText || textColor != m_spot1LastColor)
			{
				m_spot1LastText = text;
				m_spot1LastColor = textColor;

				Draw.TextFixed(this, "pl_spot1", text, BasisTextLocation, textColor, textFont, outlineColor, chartControl.Properties.ChartBackground, 50, DashStyleHelper.Solid, 2, false, "");
			}
		}

		private void DrawSpot2(ChartControl chartControl, string text, SimpleFont textFont, Brush textColor, Brush outlineColor)
		{
			if (text != m_spot2LastText || textColor != m_spot2LastColor)
			{
				m_spot2LastText = text;
				m_spot2LastColor = textColor;

				Draw.TextFixed(this, "pl_spot2", text, BasisTextLocation, textColor, textFont, outlineColor, chartControl.Properties.ChartBackground, 50, DashStyleHelper.Solid, 2, false, "");
			}
		}

		private void DrawSpot3(ChartControl chartControl, string text, SimpleFont textFont, Brush textColor, Brush outlineColor)
		{
			if (text != m_spot3LastText || textColor != m_spot3LastColor)
			{
				m_spot3LastText = text;
				m_spot3LastColor = textColor;

				Draw.TextFixed(this, "pl_spot3", text, BasisTextLocation, textColor, textFont, outlineColor, chartControl.Properties.ChartBackground, 50, DashStyleHelper.Solid, 2, false, "");
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);

			if (hasRanOnceFirstCycle)
			{
				DrawTextOnChart(chartControl);
			}
		}

		private string ConvertToFixedPoint(double source, Instrument instrument)
		{
			return source.ToString("F" + RealInstrumentService.GetTickSizeDecimalPlaces(instrument.MasterInstrument.TickSize).ToString());
		}

		private int GetTicks(Instrument instrument, double value)
		{
			int ticks = 0;

			if (instrument.MarketData != null)
			{
				try
				{
					ticks = Convert.ToInt32(value / instrument.MasterInstrument.TickSize);
				}
				catch
				{
					//this can happen when price is invalid
				}
			}

			return ticks;
		}
		private double GetSpread(Instrument instrument)
		{
			double spread = 0;

			if (instrument.MarketData != null)
			{
				int decimalPlaces = RealInstrumentService.GetTickSizeDecimalPlaces(instrument.MasterInstrument.TickSize);

				double askPrice = RealInstrumentService.GetAskPrice(instrument);
				double bidPrice = RealInstrumentService.GetBidPrice(instrument);

				if (askPrice > 0 && bidPrice > 0 && askPrice >= bidPrice)
				{
					spread = Math.Round((askPrice - bidPrice), decimalPlaces);
				}
			}

			return spread;
		}

		

		private double GetTotalPositionPnL(bool limitToAttachedInstrument, out bool hasPositions, out int tickCount)
		{
			double totalUnrealizedProfitLoss = 0;
			double unrealizedProfitLoss = 0;
			tickCount = 0;
			hasPositions = false;
			
			if (account != null && account.Positions != null)
			{
				if (account.Positions.Count > 0)
				{
					lock (account.Positions)
					{
						if (account.Positions.Count > 0)
						{
							foreach (Position position in account.Positions)
							{
								if (position.MarketPosition != MarketPosition.Flat && ((limitToAttachedInstrument && this.attachedInstrument.Id == position.Instrument.Id) || !limitToAttachedInstrument))
								{
									hasPositions = true;
									unrealizedProfitLoss = GetPositionProfit(position);
									tickCount = (int)Math.Round(((unrealizedProfitLoss / position.Quantity) / GetTickValue(position.Instrument)),MidpointRounding.ToEven);
									totalUnrealizedProfitLoss += Math.Round(unrealizedProfitLoss, 2);
								}
							}
						}
					}
				}
			}
			
			return totalUnrealizedProfitLoss;
		}

		private double GetPositionProfit(Position position)
		{
			double positionProfit = 0;
			double totalVolume = position.Quantity;
			double averagePrice = position.AveragePrice;
			double tickValue = GetTickValue(position.Instrument);
			double tickSize = position.Instrument.MasterInstrument.TickSize;

			if (position.MarketPosition == MarketPosition.Long)
			{
				double bid = RealInstrumentService.GetBidPrice(position.Instrument);

				positionProfit = (bid - averagePrice) * ((tickValue * totalVolume) / tickSize);
			}
			else if (position.MarketPosition == MarketPosition.Short)
			{
				double ask = RealInstrumentService.GetAskPrice(position.Instrument);

				positionProfit = (averagePrice - ask) * ((tickValue * totalVolume) / tickSize);
			}

			double commissionPerSide = GetCommissionPerSide(position.Instrument);
			bool includeCommissions = (commissionPerSide > 0);

			if (includeCommissions)
			{
				positionProfit = positionProfit - (totalVolume * commissionPerSide * 2);
			}


			return (Math.Round(positionProfit, 2, MidpointRounding.ToEven));

		}


		private bool IsMicroInstrument(Instrument instrument)
		{
			bool returnFlag = false;

			if (instrument.FullName.StartsWith(MYMPrefix) || instrument.FullName.StartsWith(MESPrefix) || instrument.FullName.StartsWith(M2KPrefix) || instrument.FullName.StartsWith(MNQPrefix))
			{
				returnFlag = true;
			}

			return returnFlag;
		}

		private bool IsEminiInstrument(Instrument instrument)
		{
			bool returnFlag = false;

			if (instrument.FullName.StartsWith(YMPrefix) || instrument.FullName.StartsWith(ESPrefix) || instrument.FullName.StartsWith(RTYPrefix) || instrument.FullName.StartsWith(NQPrefix))
			{
				returnFlag = true;
			}

			return returnFlag;
		}

		private double GetCommissionPerSide(Instrument instrument)
		{
			double commissionPerSide = 0;

			if (account != null && account.Commission != null && account.Commission.ByMasterInstrument.ContainsKey(instrument.MasterInstrument))
			{
				commissionPerSide = account.Commission.ByMasterInstrument[instrument.MasterInstrument].PerUnit;
			}
			else
			{
				//PrintOutput("ERROR: Missing commission per side for instrument '" + instrument.FullName + "'");
			}
			/*
			if (IsMicroInstrument(instrument))
			{
				commissionPerSide = MicroCommissionPerSide;
			}
			else if (IsEminiInstrument(instrument))
			{
				commissionPerSide = EminiCommissionPerSide;
			}
			else
			{
				PrintOutput("ERROR: Missing commission per side for instrument '" + instrument.FullName + "'");
			}
			*/

			return commissionPerSide;
		}

		private double GetTickValue(Instrument instrumnet)
		{
			double tickValue = instrumnet.MasterInstrument.PointValue * instrumnet.MasterInstrument.TickSize;

			return tickValue;
		}

		private bool IsFutureInstrumentType(Instrument instrument)
        {
			bool isFuture = (instrument.MasterInstrument.InstrumentType == InstrumentType.Future);
			return isFuture;

		}
		private string GetCurrentFuturesMonthYearPrefix()
		{
			string tempText = null;

			if (attachedInstrumentIsFuture)
			{
				tempText = this.attachedInstrument.FullName.Substring(this.attachedInstrument.MasterInstrument.Name.Length, 6);
			}
			return tempText;
		}

		private bool HasRanOnceFirstCycle()
		{
			if (!hasRanOnceFirstCycle && attachedInstrumentServerSupported && BarsInProgress == 0 && CurrentBar > 0)// && this.State == State.Realtime)
			{
				LoadAccount();

				if (account != null)
				{
					PrintOutput("Detected commission per side: " + GetCommissionPerSide(attachedInstrument).ToString("N2") + " for " + attachedInstrument.FullName + " (see account commission template per-unit commission value)", PrintTo.OutputTab1);

					PrintOutput("Detected commission per side: " + GetCommissionPerSide(attachedInstrument).ToString("N2") + " for " + attachedInstrument.FullName + " (see account commission template per-unit commission value)", PrintTo.OutputTab2);

					hasRanOnceFirstCycle = true;
				}
			}

			/*
				if (ChartControl.Dispatcher.CheckAccess())
				{
					LoadAccount();
				}
				else
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						LoadAccount();
					}));
				}
				*/

			return hasRanOnceFirstCycle;
		}


		private Account GetAccount()
		{
			Account tempAccount = null;

			try
			{
				tempAccount = this.ChartControl.OwnerChart.ChartTrader.Account;
			}
			catch
			{
				//stuff exception
			}

			return tempAccount;
		}
		private void LoadAccount(Account newAccount = null)
		{
			Account tempAccount = newAccount;

			if (newAccount == null)
			{
				tempAccount = GetAccount();
			}
			else
			{
				tempAccount = newAccount;
			}

			if (tempAccount != null && tempAccount != account)
			{
				account = tempAccount;

				PrintOutput("Found account name (" + Convert.ToString(account.DisplayName) + ")", PrintTo.OutputTab2);
				
			}
			else if (tempAccount == null)
            {
					PrintOutput("Account name not found.", PrintTo.OutputTab2);
			}
			/*
			Window currentWindow = Window.GetWindow(ChartControl.Parent);
			if (currentWindow != null)
			{
				NinjaTrader.Gui.Tools.AccountSelector accountSelector = currentWindow.FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;
				if (accountSelector != null)
				{
					if (accountSelector.SelectedAccount != null)
					{
						PrintOutput("Found account name (" + Convert.ToString(accountSelector.SelectedAccount.DisplayName) + ")", PrintTo.OutputTab2);
						account = accountSelector.SelectedAccount;
					}
				}
				else
					PrintOutput("Account name not found.", PrintTo.OutputTab2);

			}
			else
				PrintOutput("Account name not found and no window.", PrintTo.OutputTab2);
			*/
		}
		private void ValidateInstrument(string instrumentName)
		{
			Instrument instrument = Instrument.GetInstrument(instrumentName);
			if (instrument != null)
			{
				PrintOutput("Instrument =" + instrument.FullName + " Tick Size=" + Convert.ToString(instrument.MasterInstrument.TickSize) + " Tick Value=" + Convert.ToString(GetTickValue(instrument)), PrintTo.OutputTab2);
			}
		}


		private void PrintOutput(string output, PrintTo outputTab = PrintTo.OutputTab1, bool blockDuplicateMessages = false)
		{
			this.PrintTo = outputTab;
			if (blockDuplicateMessages)
			{
				int tempHashCode = output.GetHashCode();
				if (tempHashCode != lastPrintOutputHashCode)
				{
					Print(DateTime.Now + " " + SystemName + ": " + output);
				}
				lastPrintOutputHashCode = tempHashCode;
			}
			else
				Print(DateTime.Now + " " + SystemName + ": " + output);
		}

		[NinjaScriptProperty]
		[Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
		public string IndicatorName
		{
			get { return FullSystemName; }
			set { }
		}

		[NinjaScriptProperty]
		[Display(Name = "PriceTextColor", Description = "", Order = 1, GroupName = "Parameters")]
		[XmlIgnore]
		public Brush PriceTextColor
		{
			get { return this.priceTextColor; }
			set { this.priceTextColor = value; }
		}

		[Browsable(false)]
		public string PriceTextColorSerialize
		{
			get { return Serialize.BrushToString(this.priceTextColor); }
			set { this.priceTextColor = Serialize.StringToBrush(value); }
		}


		[NinjaScriptProperty]
		[Display(Name = "InProfitColor", Description = "", Order = 2, GroupName = "Parameters")]
		[XmlIgnore]
		public Brush InProfitColor
		{
			get { return this.inProfitColor; }
			set {this.inProfitColor = value; }
		}

		[Browsable(false)]
		public string InProfitColorSerialize
		{
			get { return Serialize.BrushToString(this.inProfitColor); }
			set { this.inProfitColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Display(Name = "InLossColor", Description = "", Order = 3, GroupName = "Parameters")]
		[XmlIgnore]
		public Brush InLossColor
		{
			get { return this.inLossColor; }
			set { this.inLossColor = value; }
		}

		[Browsable(false)]
		public string InLossColorSerialize
		{
			get { return Serialize.BrushToString(this.inLossColor); }
			set { this.inLossColor = Serialize.StringToBrush(value); }
		}

		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name = "ATRPeriod", Order = 4, GroupName = "Parameters")]
		public int ATRPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Chart location for basis data/info", Description = "", Order = 5, GroupName = "Parameters")]
		public NinjaTrader.NinjaScript.DrawingTools.TextPosition BasisTextLocation
		{ get; set; }
	}
}
namespace NinjaTrader.NinjaScript.Indicators.ZPI
{
	public class RealInstrumentService
	{
		private readonly ConcurrentDictionary<string, double> askPriceCache = new ConcurrentDictionary<string, double>();
		private readonly ConcurrentDictionary<string, double> bidPriceCache = new ConcurrentDictionary<string, double>();
		private readonly ConcurrentDictionary<string, double> lastPriceCache = new ConcurrentDictionary<string, double>();
		private readonly Dictionary<double, int> tickSizeDecimalPlaceCountCache = new Dictionary<double, int>();

		private string BuildKeyName(Instrument instrument)
		{
			string keyName = instrument.FullName;

			return keyName;
		}

		public double NormalizePrice(Instrument instrument, double price)
        {
			double newPrice = 0;
			int decimalPlaces = GetTickSizeDecimalPlaces(instrument.MasterInstrument.TickSize);

			string formatText = string.Concat("N", decimalPlaces);
			string stringPriceValue = price.ToString(formatText);
			newPrice = double.Parse(stringPriceValue);

			return newPrice;
        }
		public int GetTickSizeDecimalPlaces(double tickSize)
		{
			int decimalPlaceCount = 0;

			if (tickSize < 0) return decimalPlaceCount;

			if (tickSizeDecimalPlaceCountCache.ContainsKey(tickSize))
			{
				decimalPlaceCount = tickSizeDecimalPlaceCountCache[tickSize];
			}
			else
			{
				var parts = tickSize.ToString(CultureInfo.InvariantCulture).Split('.');

				if (parts.Length < 2)
					decimalPlaceCount = 0;
				else
					decimalPlaceCount = parts[1].TrimEnd('0').Length;

				tickSizeDecimalPlaceCountCache.Add(tickSize, decimalPlaceCount);
			}

			return decimalPlaceCount;
		}

		public static double GetTickValue(Instrument instrument)
		{
			double tickValue = instrument.MasterInstrument.PointValue * instrument.MasterInstrument.TickSize;

			return tickValue;
		}

		public static int GetTicksPerPoint(double tickSize)
		{
			int tickPoint = 1;

			if (tickSize < 1)
			{
				tickPoint = (int)(1.0 / tickSize);
			}

			return (tickPoint);
		}

		public static double GetTickSize(Instrument instrument)
		{
			double tickSize = instrument.MasterInstrument.TickSize;

			return (tickSize);
		}

		public double GetLastPrice(Instrument instrument)
		{
			double lastPrice = 0;
			string keyName = BuildKeyName(instrument);

			if (!lastPriceCache.TryGetValue(keyName, out lastPrice))
			{
				if (instrument.MarketData != null && instrument.MarketData.Last != null)
				{
					lastPrice = instrument.MarketData.Last.Price;
				}
				else if (instrument.MarketData != null && instrument.MarketData.Bid != null)
				{
					lastPrice = instrument.MarketData.Bid.Price;
				}
				else
				{
					lastPrice = 0;
				}
			}

			return lastPrice;
		}

		public double GetAskPrice(Instrument instrument)
		{
			double askPrice = 0;
			string keyName = BuildKeyName(instrument);

			if (!askPriceCache.TryGetValue(keyName, out askPrice))
			{
				if (instrument.MarketData != null && instrument.MarketData.Last != null)
				{
					askPrice = instrument.MarketData.Last.Ask;
				}
				else if (instrument.MarketData != null && instrument.MarketData.Ask != null)
				{
					askPrice = instrument.MarketData.Ask.Price;
				}
				else
				{
					askPrice = 0;
				}
			}

			return askPrice;
		}

		public double GetBidPrice(Instrument instrument)
		{
			double bidPrice = 0;
			string keyName = BuildKeyName(instrument);

			if (!bidPriceCache.TryGetValue(keyName, out bidPrice))
			{
				if (instrument.MarketData != null && instrument.MarketData.Last != null)
				{
					bidPrice = instrument.MarketData.Last.Bid;
				}
				else if (instrument.MarketData != null && instrument.MarketData.Bid != null)
				{
					bidPrice = instrument.MarketData.Bid.Price;
				}
				else
				{
					bidPrice = 0;
				}
			}

			return bidPrice;
		}

		public double SetAskPrice(Instrument instrument, double askPrice)
		{
			string keyName = BuildKeyName(instrument);

			double newPrice = askPriceCache.AddOrUpdate(String.Copy(keyName), askPrice, (oldkey, oldvalue) => askPrice);

			return newPrice;
		}

		public double SetBidPrice(Instrument instrument, double bidPrice)
		{
			string keyName = BuildKeyName(instrument);

			double newPrice = bidPriceCache.AddOrUpdate(String.Copy(keyName), bidPrice, (oldkey, oldvalue) => bidPrice);

			return newPrice;
		}

		public double SetLastPrice(Instrument instrument, double lastPrice)
		{
			string keyName = BuildKeyName(instrument);

			double newPrice = lastPriceCache.AddOrUpdate(String.Copy(keyName), lastPrice, (oldkey, oldvalue) => lastPrice);

			return newPrice;
		}
	}
}

	

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Zombie2PriceInfo[] cacheZombie2PriceInfo;
		public Zombie2PriceInfo Zombie2PriceInfo(string indicatorName, Brush priceTextColor, Brush inProfitColor, Brush inLossColor, int aTRPeriod, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return Zombie2PriceInfo(Input, indicatorName, priceTextColor, inProfitColor, inLossColor, aTRPeriod, basisTextLocation);
		}

		public Zombie2PriceInfo Zombie2PriceInfo(ISeries<double> input, string indicatorName, Brush priceTextColor, Brush inProfitColor, Brush inLossColor, int aTRPeriod, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			if (cacheZombie2PriceInfo != null)
				for (int idx = 0; idx < cacheZombie2PriceInfo.Length; idx++)
					if (cacheZombie2PriceInfo[idx] != null && cacheZombie2PriceInfo[idx].IndicatorName == indicatorName && cacheZombie2PriceInfo[idx].PriceTextColor == priceTextColor && cacheZombie2PriceInfo[idx].InProfitColor == inProfitColor && cacheZombie2PriceInfo[idx].InLossColor == inLossColor && cacheZombie2PriceInfo[idx].ATRPeriod == aTRPeriod && cacheZombie2PriceInfo[idx].BasisTextLocation == basisTextLocation && cacheZombie2PriceInfo[idx].EqualsInput(input))
						return cacheZombie2PriceInfo[idx];
			return CacheIndicator<Zombie2PriceInfo>(new Zombie2PriceInfo(){ IndicatorName = indicatorName, PriceTextColor = priceTextColor, InProfitColor = inProfitColor, InLossColor = inLossColor, ATRPeriod = aTRPeriod, BasisTextLocation = basisTextLocation }, input, ref cacheZombie2PriceInfo);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Zombie2PriceInfo Zombie2PriceInfo(string indicatorName, Brush priceTextColor, Brush inProfitColor, Brush inLossColor, int aTRPeriod, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2PriceInfo(Input, indicatorName, priceTextColor, inProfitColor, inLossColor, aTRPeriod, basisTextLocation);
		}

		public Indicators.Zombie2PriceInfo Zombie2PriceInfo(ISeries<double> input , string indicatorName, Brush priceTextColor, Brush inProfitColor, Brush inLossColor, int aTRPeriod, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2PriceInfo(input, indicatorName, priceTextColor, inProfitColor, inLossColor, aTRPeriod, basisTextLocation);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Zombie2PriceInfo Zombie2PriceInfo(string indicatorName, Brush priceTextColor, Brush inProfitColor, Brush inLossColor, int aTRPeriod, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2PriceInfo(Input, indicatorName, priceTextColor, inProfitColor, inLossColor, aTRPeriod, basisTextLocation);
		}

		public Indicators.Zombie2PriceInfo Zombie2PriceInfo(ISeries<double> input , string indicatorName, Brush priceTextColor, Brush inProfitColor, Brush inLossColor, int aTRPeriod, NinjaTrader.NinjaScript.DrawingTools.TextPosition basisTextLocation)
		{
			return indicator.Zombie2PriceInfo(input, indicatorName, priceTextColor, inProfitColor, inLossColor, aTRPeriod, basisTextLocation);
		}
	}
}

#endregion
