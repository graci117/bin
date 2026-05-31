// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
using NinjaTrader.Gui.SuperDom;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
#endregion

namespace NinjaTrader.NinjaScript.SuperDomColumns
{
	public class PullingStacking : SuperDomColumn
	{
		private				Dictionary<double, Tuple<long, long>>	askPriceDepthMap;
		private				Dictionary<double, Tuple<long, long>>	bidPriceDepthMap;
		private readonly	object									collectionSync				= new();
		private				FontFamily								fontFamily;
		private				FontStyle								fontStyle;
		private				FontWeight								fontWeight;
		private				Pen										gridPen;			
		private				double									halfPenWidth;
		private				bool									heightUpdateNeeded;
		private				double									previousAsk					= double.MinValue;
		private				double									previousBid					= double.MinValue;
		private				Timer									resetTimer;
		private				double									textHeight;
		private				Typeface								typeFace;

		public override void OnColumnLabelClicked(object sender, MouseButtonEventArgs e)
		{
			lock (collectionSync)
			{
				askPriceDepthMap.Clear();
				bidPriceDepthMap.Clear();

				if (SuperDom.MarketDepth == null)
					return;

				lock (SuperDom.MarketDepth.Instrument.SyncMarketDepth)
				{
					for (int i = 0; i < SuperDom.MarketDepth.Asks.Count; i++)
						askPriceDepthMap.Add(SuperDom.MarketDepth.Asks[i].Price, Tuple.Create(SuperDom.MarketDepth.Asks[i].Volume, 0L));

					for (int i = 0; i < SuperDom.MarketDepth.Bids.Count; i++)
						bidPriceDepthMap.Add(SuperDom.MarketDepth.Bids[i].Price, Tuple.Create(SuperDom.MarketDepth.Bids[i].Volume, 0L));
				}
				OnPropertyChanged();
			}
		}

		protected override void OnMarketData(MarketDataEventArgs marketData)
		{
			if (marketData.MarketDataType == MarketDataType.Last)
			{
				lock (collectionSync)
				{
					if (askPriceDepthMap.TryGetValue(marketData.Price, out Tuple<long, long> depthTuple))
						askPriceDepthMap[marketData.Price] = Tuple.Create(depthTuple.Item1 - marketData.Volume, depthTuple.Item2 - marketData.Volume);
					if (bidPriceDepthMap.TryGetValue(marketData.Price, out depthTuple))
						bidPriceDepthMap[marketData.Price] = Tuple.Create(depthTuple.Item1 - marketData.Volume, depthTuple.Item2 - marketData.Volume);
				}
			}
			else if (ResetWhen == PullingStackingResetWhen.BidAskChange) 
			{
				if (marketData.MarketDataType == MarketDataType.Ask
					&& (previousAsk <= double.MinValue || previousAsk.ApproxCompare(marketData.Price) != 0))
				{
					if (previousAsk > double.MinValue)
					{
						resetTimer?.Dispose();

						resetTimer = new(ResetTimerCallback, Tuple.Create(marketData.Price, marketData.MarketDataType), ResetTolerance, Timeout.Infinite);
					}
					previousAsk = marketData.Price;
				}
				else if (marketData.MarketDataType == MarketDataType.Bid
					&& (previousBid <= double.MinValue || previousBid.ApproxCompare(marketData.Price) != 0))
				{
					if (previousBid > double.MinValue)
					{
						resetTimer?.Dispose();

						resetTimer = new(ResetTimerCallback, Tuple.Create(marketData.Price, marketData.MarketDataType), ResetTolerance, Timeout.Infinite);
					}
					previousBid = marketData.Price;
				}
			}
		}

		protected override void OnMarketDepth(MarketDepthEventArgs marketDepth)
		{
			if (marketDepth.IsReset)
			{
				lock (collectionSync)
				{
					askPriceDepthMap.Clear();
					bidPriceDepthMap.Clear();
					OnPropertyChanged();
					return;
				}
			}

			if (marketDepth.Position >= SuperDom.DepthLevels)
				return;

			lock (collectionSync)
				if (marketDepth.MarketDataType == MarketDataType.Ask)
				{
					if (marketDepth.Operation == Cbi.Operation.Add)
						askPriceDepthMap[marketDepth.Price] = Tuple.Create(marketDepth.Volume, 0L);
					else if (marketDepth.Operation == Cbi.Operation.Update)
					{
						if (askPriceDepthMap.TryGetValue(marketDepth.Price, out Tuple<long, long> depthTuple))
							askPriceDepthMap[marketDepth.Price] = Tuple.Create(depthTuple.Item1, marketDepth.Volume - depthTuple.Item1);
						else
							askPriceDepthMap[marketDepth.Price] = Tuple.Create(marketDepth.Volume, 0L);
					}
					else if (marketDepth.Operation == Cbi.Operation.Remove && ResetWhen == PullingStackingResetWhen.NoMoreData)
						askPriceDepthMap.Remove(marketDepth.Price);
				}
				else if (marketDepth.MarketDataType == MarketDataType.Bid)
				{
					if (marketDepth.Operation == Cbi.Operation.Add)
						bidPriceDepthMap[marketDepth.Price] = Tuple.Create(marketDepth.Volume, 0L);
					else if (marketDepth.Operation == Cbi.Operation.Update)
					{
						if (bidPriceDepthMap.TryGetValue(marketDepth.Price, out Tuple<long, long> depthTuple))
							bidPriceDepthMap[marketDepth.Price] = Tuple.Create(depthTuple.Item1, marketDepth.Volume - depthTuple.Item1);
						else
							bidPriceDepthMap[marketDepth.Price] = Tuple.Create(marketDepth.Volume, 0L);
					}
					else if (marketDepth.Operation == Cbi.Operation.Remove && ResetWhen == PullingStackingResetWhen.NoMoreData)
						bidPriceDepthMap.Remove(marketDepth.Price);
				}
		}

		protected override void OnRender(DrawingContext dc, double renderWidth)
		{
			// This may be true if the UI for a column hasn't been loaded yet (e.g., restoring multiple tabs from workspace won't load each tab until it's clicked by the user)
			if (gridPen == null)
			{
				if (UiWrapper != null && PresentationSource.FromVisual(UiWrapper)?.CompositionTarget is { } target)
				{
					Matrix m			= target.TransformToDevice;
					double dpiFactor	= 1 / m.M11;
					gridPen				= new(Application.Current.TryFindResource("BorderThinBrush") as Brush,  1 * dpiFactor);
					halfPenWidth		= gridPen.Thickness * 0.5;
				}
			}

			if (!Equals(fontFamily, SuperDom.Font.Family)
				|| (SuperDom.Font.Italic && fontStyle != FontStyles.Italic)
				|| (!SuperDom.Font.Italic && fontStyle == FontStyles.Italic)
				|| (SuperDom.Font.Bold && fontWeight != FontWeights.Bold)
				|| (!SuperDom.Font.Bold && fontWeight == FontWeights.Bold))
			{
				// Only update this if something has changed
				fontFamily			= SuperDom.Font.Family;
				fontStyle			= SuperDom.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
				fontWeight			= SuperDom.Font.Bold ? FontWeights.Bold : FontWeights.Normal;
				typeFace			= new(fontFamily, fontStyle, fontWeight, FontStretches.Normal);
				heightUpdateNeeded	= true;
			}
			double verticalOffset	= -gridPen?.Thickness ?? 0;
			double pixelsPerDip		= VisualTreeHelper.GetDpi(UiWrapper).PixelsPerDip;

			lock (SuperDom.Rows)
				foreach (PriceRow row in SuperDom.Rows)
				{
					if (renderWidth - halfPenWidth >= 0)
					{
						// Draw cell
						Rect rect = new(-halfPenWidth, verticalOffset, renderWidth - halfPenWidth, SuperDom.ActualRowHeight);

						// Create a guidelines set
						GuidelineSet guidelines = new();
						guidelines.GuidelinesX.Add(rect.Left	+ halfPenWidth);
						guidelines.GuidelinesX.Add(rect.Right	+ halfPenWidth);
						guidelines.GuidelinesY.Add(rect.Top		+ halfPenWidth);
						guidelines.GuidelinesY.Add(rect.Bottom	+ halfPenWidth);

						dc.PushGuidelineSet(guidelines);

						// Draw the Ask and Bid rectangles
						if (DisplayType == PullingStackingDisplayType.BidAsk)
						{
							Rect bidRect = new(-halfPenWidth, verticalOffset, renderWidth / 2 - halfPenWidth, SuperDom.ActualRowHeight);
							Rect askRect = new(renderWidth / 2 - halfPenWidth, verticalOffset, renderWidth / 2 - halfPenWidth, SuperDom.ActualRowHeight);
							dc.DrawRectangle(BidBackColor, null, bidRect);
							dc.DrawRectangle(AskBackColor, null, askRect);
						}
						else if (DisplayType == PullingStackingDisplayType.Ask)
						{
							dc.DrawRectangle(AskBackColor, null, rect);
						}
						else if (DisplayType == PullingStackingDisplayType.Bid)
						{
							dc.DrawRectangle(BidBackColor, null, rect);
						}

						dc.DrawLine(gridPen, new(-gridPen?.Thickness ?? 0, rect.Bottom), new(renderWidth - halfPenWidth, rect.Bottom));
						dc.DrawLine(gridPen, new(rect.Right, verticalOffset), new(rect.Right, rect.Bottom));

						// Write bid/ask pulling/stacking values
						if (SuperDom.IsConnected
							&& !SuperDom.IsReloading
							&& State == State.Active)
						{
							lock (collectionSync)
							{
								if (DisplayType == PullingStackingDisplayType.BidAsk || DisplayType == PullingStackingDisplayType.Bid)
								{
									if (bidPriceDepthMap.TryGetValue(row.Price, out Tuple<long, long> bidVolume) && row.BidVolume > 0)
									{
										fontFamily	= SuperDom.Font.Family;
										typeFace	= new(fontFamily, SuperDom.Font.Italic ? FontStyles.Italic : FontStyles.Normal, SuperDom.Font.Bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);

										if (renderWidth - 6 > 0)
										{
											FormattedText bidText = new(bidVolume.Item2.ToString(Core.Globals.GeneralOptions.CurrentCulture), Core.Globals.GeneralOptions.CurrentCulture, FlowDirection.LeftToRight, typeFace, SuperDom.Font.Size, BidForeColor, pixelsPerDip) { MaxLineCount = 1, MaxTextWidth = renderWidth / 2 - 6, Trimming = TextTrimming.CharacterEllipsis };
											// Getting the text height is expensive, so only update it if something's changed
											if (heightUpdateNeeded)
											{
												textHeight = bidText.Height;
												heightUpdateNeeded = false;
											}

											dc.DrawText(bidText, new(0 + 4, verticalOffset + (SuperDom.ActualRowHeight - textHeight) / 2));
										}
									}
								}

								if (DisplayType == PullingStackingDisplayType.BidAsk || DisplayType == PullingStackingDisplayType.Ask)
								{
									if (askPriceDepthMap.TryGetValue(row.Price, out Tuple<long, long> askVolume) && row.AskVolume > 0)
									{
										fontFamily	= SuperDom.Font.Family;
										typeFace	= new(fontFamily, SuperDom.Font.Italic ? FontStyles.Italic : FontStyles.Normal, SuperDom.Font.Bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);

										if (renderWidth - 6 > 0)
										{
											FormattedText askText = new(askVolume.Item2.ToString(Core.Globals.GeneralOptions.CurrentCulture), Core.Globals.GeneralOptions.CurrentCulture, FlowDirection.LeftToRight, typeFace, SuperDom.Font.Size, AskForeColor, pixelsPerDip) { MaxLineCount = 1, MaxTextWidth = renderWidth / 2 - 6, Trimming = TextTrimming.CharacterEllipsis };
											// Getting the text height is expensive, so only update it if something's changed
											if (heightUpdateNeeded)
											{
												textHeight = askText.Height;
												heightUpdateNeeded = false;
											}

											dc.DrawText(askText, new(renderWidth / 2 + 4, verticalOffset + (SuperDom.ActualRowHeight - textHeight) / 2));
										}
									}
								}
							}
						}

						dc.Pop();
						verticalOffset += SuperDom.ActualRowHeight;
					}
				}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name							= Gui.Resource.NinjaScriptSuperDomColumnPullingStackingLabel;
				Description						= Gui.Resource.NinjaScriptSuperdomColumnPullingStackingDescription;
				DefaultWidth					= 100;
				PreviousWidth					= -1;
				IsDataSeriesRequired			= false;
				AskBackColor					= Application.Current.TryFindResource("brushPriceColumnBackground") as Brush;
				AskForeColor					= Application.Current.TryFindResource("brushVolumeColumnForeground") as SolidColorBrush;
				BidBackColor					= Application.Current.TryFindResource("brushPriceColumnBackground") as Brush;
				BidForeColor					= Application.Current.TryFindResource("brushVolumeColumnForeground") as SolidColorBrush;
				askPriceDepthMap				= new();
				bidPriceDepthMap				= new();
				DisplayType						= PullingStackingDisplayType.BidAsk;
				ResetWhen						= PullingStackingResetWhen.BidAskChange;
				ResetTolerance					= 2500;
			}
			else if (State == State.Configure)
			{
				if (UiWrapper != null && PresentationSource.FromVisual(UiWrapper)?.CompositionTarget is { } target)
				{ 
					Matrix m			= target.TransformToDevice;
					double dpiFactor	= 1 / m.M11;
					gridPen				= new(Application.Current.TryFindResource("BorderThinBrush") as Brush,  1 * dpiFactor);
					halfPenWidth		= gridPen.Thickness * 0.5;
				}
			}
			else if (State == State.Terminated) { }
		}

		private void ResetTimerCallback(object state)
		{
			lock (collectionSync)
			{
				if (SuperDom.Instrument == null || SuperDom.Instrument.MarketData.Ask == null || SuperDom.Instrument.MarketData.Bid == null)
				{
					askPriceDepthMap.Clear();
					bidPriceDepthMap.Clear();
					return;
				}

				Tuple<double, MarketDataType> priorPrice = (Tuple<double, MarketDataType>)state;
				if (priorPrice.Item2 == MarketDataType.Ask)
				{
					if (priorPrice.Item1.ApproxCompare(SuperDom.Instrument.MarketData.Ask.Price) == 0)
						return;
				} 
				else if (priorPrice.Item2 == MarketDataType.Bid)
				{
					if (priorPrice.Item1.ApproxCompare(SuperDom.Instrument.MarketData.Bid.Price) == 0)
						return;
				}

				askPriceDepthMap.Clear();
				bidPriceDepthMap.Clear();

				if (SuperDom.MarketDepth == null)
					return;

				lock (SuperDom.MarketDepth.Instrument.SyncMarketDepth)
				{
					for (int i = 0; i < SuperDom.MarketDepth.Asks.Count; i++)
						askPriceDepthMap[SuperDom.MarketDepth.Asks[i].Price] = Tuple.Create(SuperDom.MarketDepth.Asks[i].Volume, 0L);

					for (int i = 0; i < SuperDom.MarketDepth.Bids.Count; i++)
						bidPriceDepthMap[SuperDom.MarketDepth.Bids[i].Price] = Tuple.Create(SuperDom.MarketDepth.Bids[i].Volume, 0L);
				}
			}
		}

		#region Properties

		#region Setup
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptRecentColumnDiplay", GroupName = "NinjaScriptSetup", Order = 100)]
		public PullingStackingDisplayType DisplayType { get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptRecentColumnResetWhen", GroupName = "NinjaScriptSetup", Order = 110)]
		public PullingStackingResetWhen ResetWhen { get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptRecentColumnResetTolerance", GroupName = "NinjaScriptSetup", Order = 115)]
		[Range(1, int.MaxValue)]
		public int ResetTolerance { get; set; }
		#endregion

		#region Colors
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptRecentColumnAskBackground", GroupName = "PropertyCategoryVisual", Order = 105)]
		public Brush AskBackColor { get; set; }

		[Browsable(false)]
		public string AskBackgroundBrushSerialize
		{
			get => Gui.Serialize.BrushToString(AskBackColor, "brushAskPriceColumnBackground");
			set => AskBackColor = Gui.Serialize.StringToBrush(value, "brushAskPriceColumnBackground");
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptRecentColumnAskForeground", GroupName = "PropertyCategoryVisual", Order = 111)]
		public Brush AskForeColor { get; set; }

		[Browsable(false)]
		public string AskForeColorSerialize
		{
			get => Gui.Serialize.BrushToString(AskForeColor);
			set => AskForeColor = Gui.Serialize.StringToBrush(value);
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptRecentColumnBidBackground", GroupName = "PropertyCategoryVisual", Order = 116)]
		public Brush BidBackColor { get; set; }

		[Browsable(false)]
		public string BidBackgroundBrushSerialize
		{
			get => Gui.Serialize.BrushToString(BidBackColor, "brushBidPriceColumnBackground");
			set => BidBackColor = Gui.Serialize.StringToBrush(value, "brushBidPriceColumnBackground");
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptRecentColumnBidForeground", GroupName = "PropertyCategoryVisual", Order = 121)]
		public Brush BidForeColor { get; set; }

		[Browsable(false)]
		public string BidForeColorSerialize
		{
			get => Gui.Serialize.BrushToString(BidForeColor);
			set => BidForeColor = Gui.Serialize.StringToBrush(value);
		}
		#endregion
		#endregion

		[TypeConverter("NinjaTrader.Custom.ResourceEnumConverter")]
		public enum PullingStackingDisplayType
		{
			Ask,
			Bid,
			BidAsk
		}

		[TypeConverter("NinjaTrader.Custom.ResourceEnumConverter")]
		public enum PullingStackingResetWhen
		{
			NoMoreData,
			BidAskChange
		}
	}
}
