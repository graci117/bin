// --- TrendArchitectLite.cs ---
// Version 1.8.1 - Complete Code
// This version restores the helper methods that were accidentally omitted in the previous update.

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Windows;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.Indicators.GraciIndicators;

#endregion

namespace NinjaTrader.NinjaScript.Indicators.AlgoTrader
{
	public class TrendArchitectLite : Indicator
	{
		#region Indicator Variables
		private const double h1_amf_weight = 1.2, h1_alma_weight = 0.4, h1_sr_weight = 0.5, h1_swing_weight = 0.4, h1_regime_weight = 0.7, h1_bias = 0.2;
		private const double h2_amf_weight = 1.4, h2_alma_weight = 0.3, h2_sr_weight = 0.6, h2_swing_weight = 0.3, h2_regime_weight = 0.8, h2_bias = 0.15;
		private const double h3_amf_weight = 1.6, h3_alma_weight = 0.4, h3_sr_weight = 0.4, h3_swing_weight = 0.5, h3_regime_weight = 0.6, h3_bias = 0.25;
		private const double out_h1_weight = 0.4, out_h2_weight = 0.4, out_h3_weight = 0.4, out_bias = 0.3;

		private ATR atr14;
		private ALMA alma200, alma2;
		private Swing swing;
		private DM dm;
		private HMA hma10;
		private VMA masterVma;

		private Series<double> amf_line;
		private Series<double> amf_signal;
		private Series<double> vzo_value_series;
		private Series<double> amf_context_series;
		private Series<double> alma_context_series;
		private Series<double> sr_context_series;
		private Series<double> swing_context_series;
		private Series<double> regime_context_series;
		private Series<double> normalized_amf_score_series;
		#endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description	= @"NinjaScript conversion of the Pine Script 'Trend Architect Lite' indicator. It uses a neural network model to score and grade buy/sell signals.";
				Name		= "TrendArchitectLite";
				Calculate	= Calculate.OnBarClose;
				IsOverlay	= true;
				DisplayInDataBox	= true;
				DrawOnPricePanel	= true;
				DrawHorizontalGridLines	= true;
				DrawVerticalGridLines	= true;
				PaintPriceMarkers	= true;
				ScaleJustification	= ScaleJustification.Right;
				IsSuspendedWhileInactive = true;
				
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "BuySignalScore");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "SellSignalScore");
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "LastSignalDirection");

				EnableMasterTrendFilter = true;
				MasterTrendPeriod 		= 50;
				MasterVolatilityPeriod  = 30;

				EnableScalpHelper 		= true;
				
				SignalDistanceFromCandles = 0.5;
				SignalLabelSize			  = 18; 
				ShowFilteredLabels		  = true;

				NormalizationLookback 	= 50;
				ClippingThreshold 		= 3.0;

				ALMALength 				= 20;
				ALMAOffset 				= 0.85;
				ALMASigma 				= 6.0;
				PivotLookback 			= 10;
				SR_ProximityThreshold 	= 0.5;
				SwingStructureLength 	= 20;
				RegimeLookback 			= 30;
				TrendingThreshold 		= 25;
			}
			else if (State == State.DataLoaded)
			{
				atr14 		= ATR(14);
				alma200 	= ALMA(Close, ALMALength, ALMAOffset, ALMASigma);
				alma2 		= ALMA(Close, ALMALength, ALMAOffset - 0.08, ALMASigma);
				swing		= Swing(SwingStructureLength);
				dm 			= DM(RegimeLookback);
				hma10		= HMA(10);
				masterVma 	= VMA(Close, MasterTrendPeriod, MasterVolatilityPeriod);
				
				amf_line 				= new Series<double>(this);
				amf_signal 				= new Series<double>(this);
				vzo_value_series		= new Series<double>(this);
				amf_context_series 		= new Series<double>(this);
				alma_context_series		= new Series<double>(this);
				sr_context_series		= new Series<double>(this);
				swing_context_series 	= new Series<double>(this);
				regime_context_series	= new Series<double>(this);
				normalized_amf_score_series = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < Math.Max(NormalizationLookback, RegimeLookback))
				return;
			
			BuySignalScore[0] 		= 0;
			SellSignalScore[0] 		= 0;
			LastSignalDirection[0]  = LastSignalDirection[1];

			bool buy_signal_buySignal 	= CrossAbove(Close, hma10, 1);
			bool sell_signal_sellSignal = CrossBelow(Close, hma10, 1);

			if (EnableScalpHelper && (buy_signal_buySignal || sell_signal_sellSignal))
			{
				amf_line[0] = CalculateAMF(Close, 14, 5, 21, 3);
				amf_signal[0] = EMA(amf_line, 9)[0];

				double amf_context 		= AnalyzeAMF(amf_line, amf_signal, buy_signal_buySignal, sell_signal_sellSignal, atr14);
				double alma_context 	= AnalyzeALMA(alma200, alma2, buy_signal_buySignal, sell_signal_sellSignal, atr14);
				double sr_context 		= AnalyzeSupportResistance(buy_signal_buySignal, sell_signal_sellSignal, atr14, High, Low, Close);
				double swing_context 	= AnalyzeSwingStructure(buy_signal_buySignal, sell_signal_sellSignal);
				double regime_context 	= AnalyzeMarketRegime(buy_signal_buySignal, sell_signal_sellSignal);
				
				double amf_norm 	= NormalizeInput(amf_context, amf_context_series, NormalizationLookback);
				double alma_norm 	= NormalizeInput(alma_context, alma_context_series, NormalizationLookback);
				double sr_norm 		= NormalizeInput(sr_context, sr_context_series, NormalizationLookback);
				double swing_norm 	= NormalizeInput(swing_context, swing_context_series, NormalizationLookback);
				double regime_norm 	= NormalizeInput(regime_context, regime_context_series, NormalizationLookback);
				
				double signal_score = ScoreSignalNormalized(amf_norm, alma_norm, sr_norm, swing_norm, regime_norm);
				
				bool masterTrendFilterVeto = false;
				if(EnableMasterTrendFilter)
				{
					if(buy_signal_buySignal && Close[0] < masterVma[0])
					{
						signal_score = 0.30;
						masterTrendFilterVeto = true;
					}
					else if(sell_signal_sellSignal && Close[0] > masterVma[0])
					{
						signal_score = 0.30;
						masterTrendFilterVeto = true;
					}
				}

				string signal_grade = GetSignalGrade(signal_score);
				
				if (buy_signal_buySignal)
				{
					BuySignalScore[0] = signal_score;
					if(!masterTrendFilterVeto) LastSignalDirection[0] = 1;
					
					if (ShowFilteredLabels && (signal_grade == "A+" || signal_grade == "A" || signal_grade == "B"))
					{
						Brush score_color = GetScoreColor(signal_score, true);
						double atr_offset = SignalDistanceFromCandles * atr14[0];
						Draw.Text(this, "BuySignal" + CurrentBar, true, signal_grade, 0, Low[0] - atr_offset, 0, score_color, new SimpleFont("Arial", SignalLabelSize), System.Windows.TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}
				}
				
				if (sell_signal_sellSignal)
				{
					SellSignalScore[0] = signal_score;
					if(!masterTrendFilterVeto) LastSignalDirection[0] = -1;
					
					if (ShowFilteredLabels && (signal_grade == "A+" || signal_grade == "A" || signal_grade == "B"))
					{
						Brush score_color = GetScoreColor(signal_score, false);
						double atr_offset = SignalDistanceFromCandles * atr14[0];
						Draw.Text(this, "SellSignal" + CurrentBar, true, signal_grade, 0, High[0] + atr_offset, 0, score_color, new SimpleFont("Arial", SignalLabelSize), System.Windows.TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}
				}
			}
		}

		#region Analysis Functions
		
		private double CalculateAMF(ISeries<double> src, int length, int fast_len, int slow_len, int smooth_len)
		{
			SMA smaAtr = SMA(ATR(14), 50);
			if (smaAtr[0].ApproxCompare(0) == 0) return 0;
			
			double volatility_ratio = ATR(14)[0] / smaAtr[0];
			double raw_adaptive_lookback = length / Math.Pow(volatility_ratio, 2.0);
			int adaptive_lookback = (int)Math.Max(5, Math.Min(50, raw_adaptive_lookback));
			
			double zlema_fast = ZLEMA(src, adaptive_lookback)[0];
			double dema_medium = DEMA(src, (int)(adaptive_lookback * 1.5))[0];
			double dema_slow = DEMA(src, (int)(adaptive_lookback * 2.5))[0];
			
			Series<double> pvSeries = new Series<double>(this);
			pvSeries[0] = src[0] > src[1] ? Volume[0] : (src[0] < src[1] ? -Volume[0] : 0);
			
			SUM sumPv = SUM(pvSeries, length);
			SUM sumVol = SUM(Volume, length);
			vzo_value_series[0] = sumVol[0] > 0 ? (sumPv[0] / sumVol[0] * 100) : 0;
			double smoothed_vzo = SMA(vzo_value_series, 3)[0];
			
			double oscillator_score = 0.0;
			oscillator_score += (src[0] > zlema_fast) ? 30.0 : -30.0;
			oscillator_score += (zlema_fast > dema_medium) ? 30.0 : -30.0;
			oscillator_score += (dema_medium > dema_slow) ? 40.0 : -40.0;
			oscillator_score += (smoothed_vzo / 100.0) * 20.0;
			
			normalized_amf_score_series[0] = oscillator_score / 120.0;
			return EMA(normalized_amf_score_series, smooth_len)[0];
		}
		
		private double AnalyzeAMF(Series<double> amf, Series<double> signal, bool is_buy, bool is_sell, ISeries<double> atr)
		{
			double amf_vs_signal = amf[0] - signal[0];
			bool crossing_up = CrossAbove(amf, signal, 1);
			bool crossing_down = CrossBelow(amf, signal, 1);
			double amf_score = 0.0;

			if (is_buy)
			{
				if (crossing_up || amf[0] > signal[0]) amf_score = 1.0;
				else if (amf[0] < signal[0]) amf_score = (atr[0] > 0 && Math.Abs(amf_vs_signal) / atr[0] > 0.3) ? -0.8 : -0.4;
			}
			else if (is_sell)
			{
				if (crossing_down || amf[0] < signal[0]) amf_score = 1.0;
				else if (amf[0] > signal[0]) amf_score = (atr[0] > 0 && Math.Abs(amf_vs_signal) / atr[0] > 0.3) ? -0.8 : -0.4;
			}
			return Math.Max(-1.0, Math.Min(1.0, amf_score));
		}
		
		private double AnalyzeALMA(ALMA fast_alma, ALMA slow_alma, bool is_buy, bool is_sell, ISeries<double> atr)
		{
			double alma_gap = fast_alma[0] - slow_alma[0];
			bool is_wide_gap = atr[0] > 0 && Math.Abs(alma_gap) / atr[0] > 0.1;
			bool crossing_up = CrossAbove(fast_alma, slow_alma, 1);
			bool crossing_down = CrossBelow(fast_alma, slow_alma, 1);
			double alma_score = 0.0;
			
			if(is_buy)
			{
				if(crossing_up || fast_alma[0] > slow_alma[0]) alma_score = 0.5;
				else if (fast_alma[0] < slow_alma[0]) alma_score = is_wide_gap ? -0.8 : -0.2;
			}
			else if (is_sell)
			{
				if(crossing_down || fast_alma[0] < slow_alma[0]) alma_score = 0.5;
				else if (fast_alma[0] > slow_alma[0]) alma_score = is_wide_gap ? -0.8 : -0.2;
			}
			return alma_score;
		}
		
		private double AnalyzeSupportResistance(bool is_buy, bool is_sell, ISeries<double> atr, ISeries<double> high, ISeries<double> low, ISeries<double> close)
		{
			double pivot_high = MAX(high, PivotLookback)[PivotLookback];
			double pivot_low = MIN(low, PivotLookback)[PivotLookback];
			
			double resistance_distance = pivot_high > 0 && atr[0] > 0 ? Math.Abs(close[0] - pivot_high) / atr[0] : 999;
			double support_distance = pivot_low > 0 && atr[0] > 0 ? Math.Abs(close[0] - pivot_low) / atr[0] : 999;
			
			bool near_resistance = resistance_distance <= SR_ProximityThreshold;
			bool near_support = support_distance <= SR_ProximityThreshold;
			
			double sr_score = 0.0;
			if(is_buy)
			{
				if(near_support) sr_score = 0.5;
				else if(near_resistance) sr_score = -0.6;
			}
			else if (is_sell)
			{
				if(near_resistance) sr_score = 0.5;
				else if(near_support) sr_score = -0.6;
			}
			return sr_score;
		}

		private double AnalyzeSwingStructure(bool is_buy, bool is_sell)
		{
			bool bullish_structure = swing.SwingHigh[0] > swing.SwingHigh[1] && swing.SwingLow[0] > swing.SwingLow[1];
			bool bearish_structure = swing.SwingHigh[0] < swing.SwingHigh[1] && swing.SwingLow[0] < swing.SwingLow[1];
			
			double swing_score = 0.0;
			if(is_buy)
			{
				if(bullish_structure) swing_score = 0.6;
				else if(bearish_structure) swing_score = -0.7;
				else swing_score = -0.1;
			}
			else if (is_sell)
			{
				if(bearish_structure) swing_score = 0.6;
				else if(bullish_structure) swing_score = -0.7;
				else swing_score = -0.1;
			}
			return swing_score;
		}
		
		private double AnalyzeMarketRegime(bool is_buy, bool is_sell)
		{
			bool trending_market = dm.ADXPlot[0] >= TrendingThreshold;
			bool bullish_trend = trending_market && dm.DiPlus[0] > dm.DiMinus[0];
			bool bearish_trend = trending_market && dm.DiMinus[0] > dm.DiPlus[0];
			
			double regime_score = 0.0;
			if(is_buy)
			{
				if(bullish_trend) regime_score = 0.7;
				else if (bearish_trend) regime_score = -0.8;
				else regime_score = -0.2;
			}
			else if (is_sell)
			{
				if(bearish_trend) regime_score = 0.7;
				else if (bullish_trend) regime_score = -0.8;
				else regime_score = -0.2;
			}
			return regime_score;
		}
		
		#endregion

		#region Normalization & Scoring Functions
		
		private double NormalizeInput(double value, Series<double> contextSeries, int lookback_period)
		{
			contextSeries[0] = value;
			if (CurrentBar < lookback_period) return 0;

			double mean_val = SMA(contextSeries, lookback_period)[0];
			double std_val = StdDev(contextSeries, lookback_period)[0];
			
			double normalized = std_val > 0 ? (value - mean_val) / std_val : 0.0;
			
			return Math.Max(-ClippingThreshold, Math.Min(ClippingThreshold, normalized));
		}
		
		private double Tanh(double x)
		{
			return Math.Tanh(x);
		}

		private double ScoreSignalNormalized(double amf_norm, double alma_norm, double sr_norm, double swing_norm, double regime_norm)
		{
			double input_scale = 0.8;
			
			double hidden1 = Tanh(h1_amf_weight*amf_norm*input_scale + h1_alma_weight*alma_norm*input_scale + h1_sr_weight*sr_norm*input_scale + h1_swing_weight*swing_norm*input_scale + h1_regime_weight*regime_norm*input_scale + h1_bias);
			double hidden2 = Tanh(h2_amf_weight*amf_norm*input_scale + h2_alma_weight*alma_norm*input_scale + h2_sr_weight*sr_norm*input_scale + h2_swing_weight*swing_norm*input_scale + h2_regime_weight*regime_norm*input_scale + h2_bias);
			double hidden3 = Tanh(h3_amf_weight*amf_norm*input_scale + h3_alma_weight*alma_norm*input_scale + h3_sr_weight*sr_norm*input_scale + h3_swing_weight*swing_norm*input_scale + h3_regime_weight*regime_norm*input_scale + h3_bias);
			
			double raw_output = out_h1_weight*hidden1 + out_h2_weight*hidden2 + out_h3_weight*hidden3 + out_bias;
			
			double temperature = 1.2;
			return 1.0 / (1.0 + Math.Exp(-raw_output / temperature));
		}
		
		private string GetSignalGrade(double score)
		{
			if (score >= 0.80) return "A+";  
			else if (score >= 0.76) return "A";   
			else if (score >= 0.65) return "B";   
			else if (score >= 0.40) return "C";   
			else if (score >= 0.32) return "D";   
			else return "F";
		}
		
		private Brush GetScoreColor(double score, bool isBuy)
		{
			if(isBuy)
			{
				if (score >= 0.76) return Brushes.Lime;
				else if (score >= 0.65) return Brushes.DarkTurquoise;
				else if (score >= 0.40) return Brushes.White;
				else return Brushes.Gold; 
			}
			else // Is Sell
			{
				if (score >= 0.76) return Brushes.Red;
				else if (score >= 0.65) return Brushes.Fuchsia;
				else if (score >= 0.40) return Brushes.White;
				else return Brushes.Gold;
			}
		}

		#endregion

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySignalScore { get { return Values[0]; } }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellSignalScore { get { return Values[1]; } }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LastSignalDirection { get { return Values[2]; } }
		
		[NinjaScriptProperty]
		[Display(Name="Enable Master Trend Filter", Order=1, GroupName="General Settings")]
		public bool EnableMasterTrendFilter { get; set; }

		[NinjaScriptProperty]
		[Display(Name="Master Trend Period", Order=2, GroupName="General Settings")]
		public int MasterTrendPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Master Volatility Period", Order=3, GroupName="General Settings")]
		public int MasterVolatilityPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Enable Scalp Helper Signals", Order=4, GroupName="General Settings")]
		public bool EnableScalpHelper { get; set; }

		[NinjaScriptProperty]
		[Display(Name="Show Filtered Labels (A/B)", Order=1, GroupName="Display Settings")]
		public bool ShowFilteredLabels { get; set; }

		[NinjaScriptProperty]
		[Display(Name="Signal Label Size", Order=2, GroupName="Display Settings")]
		public int SignalLabelSize { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Signal Distance from Candles", Order=3, GroupName="Display Settings")]
		public double SignalDistanceFromCandles { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Normalization Lookback", Order=1, GroupName="Neural Network Settings")]
		public int NormalizationLookback { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Outlier Clipping Threshold", Order=2, GroupName="Neural Network Settings")]
		public double ClippingThreshold { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ALMA Length", Order=1, GroupName="Indicator Parameters")]
		public int ALMALength { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ALMA Offset", Order=2, GroupName="Indicator Parameters")]
		public double ALMAOffset { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ALMA Sigma", Order=3, GroupName="Indicator Parameters")]
		public double ALMASigma { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Pivot Lookback", Order=4, GroupName="Indicator Parameters")]
		public int PivotLookback { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="S/R Proximity Threshold (ATR)", Order=5, GroupName="Indicator Parameters")]
		public double SR_ProximityThreshold { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Swing Structure Length", Order=6, GroupName="Indicator Parameters")]
		public int SwingStructureLength { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Regime Lookback", Order=7, GroupName="Indicator Parameters")]
		public int RegimeLookback { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trending Threshold (ADX)", Order=8, GroupName="Indicator Parameters")]
		public int TrendingThreshold { get; set; }

        [Browsable(false)] public double SignalTriggerSensitivity { get; set; }
        [Browsable(false)] public double StopLossATRMultiplier { get; set; }

		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlgoTrader.TrendArchitectLite[] cacheTrendArchitectLite;
		public AlgoTrader.TrendArchitectLite TrendArchitectLite(bool enableMasterTrendFilter, int masterTrendPeriod, int masterVolatilityPeriod, bool enableScalpHelper, bool showFilteredLabels, int signalLabelSize, double signalDistanceFromCandles, int normalizationLookback, double clippingThreshold, int aLMALength, double aLMAOffset, double aLMASigma, int pivotLookback, double sR_ProximityThreshold, int swingStructureLength, int regimeLookback, int trendingThreshold)
		{
			return TrendArchitectLite(Input, enableMasterTrendFilter, masterTrendPeriod, masterVolatilityPeriod, enableScalpHelper, showFilteredLabels, signalLabelSize, signalDistanceFromCandles, normalizationLookback, clippingThreshold, aLMALength, aLMAOffset, aLMASigma, pivotLookback, sR_ProximityThreshold, swingStructureLength, regimeLookback, trendingThreshold);
		}

		public AlgoTrader.TrendArchitectLite TrendArchitectLite(ISeries<double> input, bool enableMasterTrendFilter, int masterTrendPeriod, int masterVolatilityPeriod, bool enableScalpHelper, bool showFilteredLabels, int signalLabelSize, double signalDistanceFromCandles, int normalizationLookback, double clippingThreshold, int aLMALength, double aLMAOffset, double aLMASigma, int pivotLookback, double sR_ProximityThreshold, int swingStructureLength, int regimeLookback, int trendingThreshold)
		{
			if (cacheTrendArchitectLite != null)
				for (int idx = 0; idx < cacheTrendArchitectLite.Length; idx++)
					if (cacheTrendArchitectLite[idx] != null && cacheTrendArchitectLite[idx].EnableMasterTrendFilter == enableMasterTrendFilter && cacheTrendArchitectLite[idx].MasterTrendPeriod == masterTrendPeriod && cacheTrendArchitectLite[idx].MasterVolatilityPeriod == masterVolatilityPeriod && cacheTrendArchitectLite[idx].EnableScalpHelper == enableScalpHelper && cacheTrendArchitectLite[idx].ShowFilteredLabels == showFilteredLabels && cacheTrendArchitectLite[idx].SignalLabelSize == signalLabelSize && cacheTrendArchitectLite[idx].SignalDistanceFromCandles == signalDistanceFromCandles && cacheTrendArchitectLite[idx].NormalizationLookback == normalizationLookback && cacheTrendArchitectLite[idx].ClippingThreshold == clippingThreshold && cacheTrendArchitectLite[idx].ALMALength == aLMALength && cacheTrendArchitectLite[idx].ALMAOffset == aLMAOffset && cacheTrendArchitectLite[idx].ALMASigma == aLMASigma && cacheTrendArchitectLite[idx].PivotLookback == pivotLookback && cacheTrendArchitectLite[idx].SR_ProximityThreshold == sR_ProximityThreshold && cacheTrendArchitectLite[idx].SwingStructureLength == swingStructureLength && cacheTrendArchitectLite[idx].RegimeLookback == regimeLookback && cacheTrendArchitectLite[idx].TrendingThreshold == trendingThreshold && cacheTrendArchitectLite[idx].EqualsInput(input))
						return cacheTrendArchitectLite[idx];
			return CacheIndicator<AlgoTrader.TrendArchitectLite>(new AlgoTrader.TrendArchitectLite(){ EnableMasterTrendFilter = enableMasterTrendFilter, MasterTrendPeriod = masterTrendPeriod, MasterVolatilityPeriod = masterVolatilityPeriod, EnableScalpHelper = enableScalpHelper, ShowFilteredLabels = showFilteredLabels, SignalLabelSize = signalLabelSize, SignalDistanceFromCandles = signalDistanceFromCandles, NormalizationLookback = normalizationLookback, ClippingThreshold = clippingThreshold, ALMALength = aLMALength, ALMAOffset = aLMAOffset, ALMASigma = aLMASigma, PivotLookback = pivotLookback, SR_ProximityThreshold = sR_ProximityThreshold, SwingStructureLength = swingStructureLength, RegimeLookback = regimeLookback, TrendingThreshold = trendingThreshold }, input, ref cacheTrendArchitectLite);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlgoTrader.TrendArchitectLite TrendArchitectLite(bool enableMasterTrendFilter, int masterTrendPeriod, int masterVolatilityPeriod, bool enableScalpHelper, bool showFilteredLabels, int signalLabelSize, double signalDistanceFromCandles, int normalizationLookback, double clippingThreshold, int aLMALength, double aLMAOffset, double aLMASigma, int pivotLookback, double sR_ProximityThreshold, int swingStructureLength, int regimeLookback, int trendingThreshold)
		{
			return indicator.TrendArchitectLite(Input, enableMasterTrendFilter, masterTrendPeriod, masterVolatilityPeriod, enableScalpHelper, showFilteredLabels, signalLabelSize, signalDistanceFromCandles, normalizationLookback, clippingThreshold, aLMALength, aLMAOffset, aLMASigma, pivotLookback, sR_ProximityThreshold, swingStructureLength, regimeLookback, trendingThreshold);
		}

		public Indicators.AlgoTrader.TrendArchitectLite TrendArchitectLite(ISeries<double> input , bool enableMasterTrendFilter, int masterTrendPeriod, int masterVolatilityPeriod, bool enableScalpHelper, bool showFilteredLabels, int signalLabelSize, double signalDistanceFromCandles, int normalizationLookback, double clippingThreshold, int aLMALength, double aLMAOffset, double aLMASigma, int pivotLookback, double sR_ProximityThreshold, int swingStructureLength, int regimeLookback, int trendingThreshold)
		{
			return indicator.TrendArchitectLite(input, enableMasterTrendFilter, masterTrendPeriod, masterVolatilityPeriod, enableScalpHelper, showFilteredLabels, signalLabelSize, signalDistanceFromCandles, normalizationLookback, clippingThreshold, aLMALength, aLMAOffset, aLMASigma, pivotLookback, sR_ProximityThreshold, swingStructureLength, regimeLookback, trendingThreshold);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlgoTrader.TrendArchitectLite TrendArchitectLite(bool enableMasterTrendFilter, int masterTrendPeriod, int masterVolatilityPeriod, bool enableScalpHelper, bool showFilteredLabels, int signalLabelSize, double signalDistanceFromCandles, int normalizationLookback, double clippingThreshold, int aLMALength, double aLMAOffset, double aLMASigma, int pivotLookback, double sR_ProximityThreshold, int swingStructureLength, int regimeLookback, int trendingThreshold)
		{
			return indicator.TrendArchitectLite(Input, enableMasterTrendFilter, masterTrendPeriod, masterVolatilityPeriod, enableScalpHelper, showFilteredLabels, signalLabelSize, signalDistanceFromCandles, normalizationLookback, clippingThreshold, aLMALength, aLMAOffset, aLMASigma, pivotLookback, sR_ProximityThreshold, swingStructureLength, regimeLookback, trendingThreshold);
		}

		public Indicators.AlgoTrader.TrendArchitectLite TrendArchitectLite(ISeries<double> input , bool enableMasterTrendFilter, int masterTrendPeriod, int masterVolatilityPeriod, bool enableScalpHelper, bool showFilteredLabels, int signalLabelSize, double signalDistanceFromCandles, int normalizationLookback, double clippingThreshold, int aLMALength, double aLMAOffset, double aLMASigma, int pivotLookback, double sR_ProximityThreshold, int swingStructureLength, int regimeLookback, int trendingThreshold)
		{
			return indicator.TrendArchitectLite(input, enableMasterTrendFilter, masterTrendPeriod, masterVolatilityPeriod, enableScalpHelper, showFilteredLabels, signalLabelSize, signalDistanceFromCandles, normalizationLookback, clippingThreshold, aLMALength, aLMAOffset, aLMASigma, pivotLookback, sR_ProximityThreshold, swingStructureLength, regimeLookback, trendingThreshold);
		}
	}
}

#endregion
