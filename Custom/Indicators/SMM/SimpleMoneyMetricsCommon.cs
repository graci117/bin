//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// 	File			:	SimpleMoneyMetricsCommon																	//
//	Description		:	Common enumerations, functions and objects used in the SMM indicators.						//
//	Author			:	Adheer Pai (adheer@gmail.com, firstlanetech@gmail.com)										//
//	History																											//
//		18-Nov-2024		: 	1.00	Initial version.																//
//		24-Nov-2024		:	1.01	Added CustomPriceSeries and indicators.											//
//		08-Feb-2025		:	1.02	Added EMovingAverageType and EDataSource enumerations.							//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// #property version "1.02"

#region References

using NinjaTrader.NinjaScript;
using System.Collections.Generic;
using System;

#endregion

//////////////////////////////////////////////////////////////////////////////////////////////////////
// SimpleMoneyMetricsCommon - Namespace of custom enumerations of this indicator.
//////////////////////////////////////////////////////////////////////////////////////////////////////
namespace SimpleMoneyMetricsCommon
{
	public enum ECandleColoringType { ProfitWave, Trend };
	public enum EExtendMethod 		{ Touch, Close };
	public enum ERealPriceLineWidth { Thin, Thick };
	public enum ERealCloseSize		{ Auto, Small, Large };
	public enum EMode				{ None, Buy, Sell };
	public enum EMovingAverageType  { SMA, EMA };
	public enum EDataSource			{ Price, HeikenAshi };

	//////////////////////////////////////////////////////////////////////////////////////////////////////
	// CustomPriceSeries - A class to represent OHLC series.
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	public class CustomPriceSeries
	{
		public Series<double> Open 	 = null;
		public Series<double> High 	 = null;
		public Series<double> Low 	 = null;
		public Series<double> Close  = null;
		public Series<double> HLC3 	 = null;

		public Series<double> Volume = null;
		public NinjaScriptBase Base  = null;

		private CustomPriceSeries() {}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public CustomPriceSeries(NinjaScriptBase baseObj)
		{
			Base = baseObj;
			Open   = new Series<double>(baseObj);
			High   = new Series<double>(baseObj);
			Low    = new Series<double>(baseObj);
			Close  = new Series<double>(baseObj);
			HLC3   = new Series<double>(baseObj);
			Volume = new Series<double>(baseObj);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public void Update()
		{
			Open[0]   = Base.Open[0];
			High[0]   = Base.High[0];
			Low[0]    = Base.Low[0];
			Close[0]  = Base.Close[0];
			HLC3[0]   = (Base.High[0] + Base.Low[0] + Base.Close[0]) / 3.0f;
			Volume[0] = Base.Volume[0];
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public void Update(ref CustomPriceSeries fromSeries)
		{
			Open[0]   = fromSeries.Open[0];
			High[0]   = fromSeries.High[0];
			Low[0]    = fromSeries.Low[0];
			Close[0]  = fromSeries.Close[0];
			HLC3[0]   = (fromSeries.High[0] + fromSeries.Low[0] + fromSeries.Close[0]) / 3.0f;
			Volume[0] = fromSeries.Volume[0];
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public void UpdateHeikinAshi(ref CustomPriceSeries fromSeries)
		{
			Volume[0] = fromSeries.Volume[0];
			if (Base.CurrentBar == 0)
            {
                Open[0]	 = fromSeries.Open[0];
                High[0]	 = fromSeries.High[0];
                Low[0]	 = fromSeries.Low[0];
                Close[0] = fromSeries.Close[0];
				HLC3[0]  = (High[0] + Low[0] + Close[0]) / 3.0;
                return;
            }

            Close[0]  = (fromSeries.Open[0] + fromSeries.High[0] + fromSeries.Low[0] + fromSeries.Close[0]) * 0.25;
            Open[0]	  = (Open[1] + Close[1]) * 0.5;
            High[0]	  = Math.Max(fromSeries.High[0], Open[0]);
            Low[0]	  = Math.Min(fromSeries.Low[0], Open[0]);
			HLC3[0]   = (High[0] + Low[0] + Close[0]) / 3.0;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////
	// AverageTrueRange - Average True Range custom indicator.
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	public class AverageTrueRange
	{
		public Series<double> ATR = null;
		private CustomPriceSeries _series = null;
		private int _period = 10;

		private AverageTrueRange() {}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public AverageTrueRange(ref CustomPriceSeries series, int period)
		{
			_series = series;
			_period = period;
			ATR = new Series<double>(_series.Base);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public double Calculate()
		{
			double atr = 0.0;

			double high0	= _series.High[0];
			double low0		= _series.Low[0];
			if( _series.Base.CurrentBar == 0 )
				atr = high0 - low0;
			else
			{
				double close1		= _series.Close[1];
				double trueRange	= Math.Max(Math.Abs(low0 - close1), Math.Max(high0 - low0, Math.Abs(high0 - close1)));
				atr					= ((Math.Min(_series.Base.CurrentBar + 1, _period) - 1 ) * ATR[1] + trueRange) / Math.Min(_series.Base.CurrentBar + 1, _period);
			}
			ATR[0] = atr;
			return atr;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////
	// ExponentialMA - Exponential Moving Average custom indicator.
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	public class ExponentialMA
	{
		public Series<double> EMA = null;

		private Series<double> _source = null;
		private NinjaScriptBase _base = null;
		private int _period = 10;

		private double _constant1 = 0.0;
		private double _constant2 = 0.0;

		private ExponentialMA() {}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public ExponentialMA(NinjaScriptBase baseObj, Series<double> series, int period)
		{
			_base = baseObj;
			_source = series;
			_period = period;
			EMA = new Series<double>(_base);
			_constant1 = 2.0 / (1 + _period);
			_constant2 = 1 - (2.0 / (1 + _period));
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public double Calculate()
		{
			EMA[0] = (_base.CurrentBar == 0 ? _source[0] : _source[0] * _constant1 + _constant2 * EMA[1]);
			return EMA[0];
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////
	// MoneyFlowIndex - Money Flow Index custom indicator.
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	public class MoneyFlowIndex
	{
		public Series<double> MFI = null;

		private NinjaScriptBase _base = null;
		private Series<double> _field = null;
		private ISeries<double> _volume = null;
		private int _period = 10;

		private MoneyFlowIndex() {}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public MoneyFlowIndex(NinjaScriptBase baseObj, ref Series<double> field, ISeries<double> volume, int period)
		{
			_base = baseObj;
			_field = field;
			_volume = volume;
			_period = period;
			MFI = new Series<double>(baseObj);
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////
		public double Calculate()
		{
			MFI[0] = double.NaN;
			if( _base.CurrentBar < _period ) return MFI[0];
			double positiveMoneyFlow = 0.0, negativeMoneyFlow = 0.0;
			for(int i = 0; i < _period; i++)
			{
				if( _field[i] > _field[i+1] )
					positiveMoneyFlow += (_volume[i] * _field[i]);
				else if( _field[i] < _field[i+1] )
					negativeMoneyFlow += (_volume[i] * _field[i]);
			}
			if( negativeMoneyFlow != 0.0 )
				MFI[0] = 100.0 - (100.0 / (1.0 + positiveMoneyFlow / negativeMoneyFlow));
			else
				MFI[0] = 0.0;
			return MFI[0];
		}
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	// SupportResistanceLevel
	//////////////////////////////////////////////////////////////////////////////////////////////////////
	public class SupportResistanceLevel
	{
		public bool		IsSupport;
		public int 		FromBar;
		public double	Level;
		public int		UptoBar;
		public bool		IsActive;

		public SupportResistanceLevel(bool isSupport, int fromBar, double level, int uptoBar)
		{
			IsSupport = isSupport;
			FromBar = fromBar;
			Level = level;
			UptoBar = uptoBar;
			IsActive = true;
		}
		private SupportResistanceLevel() {}
	}

}
