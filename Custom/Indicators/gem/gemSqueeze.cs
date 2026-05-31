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
	/*Copyright 2020 G. Eric Morgan (aka radi8 in Big Mike Trading forum).

	This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/gpl.txt


	*/
    /// Squeeze indicator that also brackets the price bars during the squeeze to help identify breakout trades

	/// 
	/// Trading futures, currencies, and equities is inherently high risk.  Use of this indicator is at your own risk
	/// and in so doing you accept personal responsibility to any potential financial gains or losses as a result.
	/// This indicator was created with the intent of helping identify potential entry signals for trades based on rules
	/// established in the Team PippEdge group.  These signals should not be construed as a recommendation for trading, rather
	/// they should be evalued on a case by case basis in the context of your personal risk management and trading style.  The
	/// decision to enter a trade is your own.

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.gem
{
	public class gemSqueeze : Indicator
	{
        private Series<double>  					_myValue2;
		private Series<double>						_bbsInd;
		private int 								_startbar;
		private double								_pricehigh;
		private double								_pricelow;
		private bool								_squeezeon;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Squeeze indicator that also brackets the price bars during the squeeze to help identify breakout trades.";
				Name										= "gemSqueeze";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= false;
        		_length 									= 20;
        		_nBb 										= 2;
        		_nK 										= 1.5;
        		_mLength 									= 20;
        		_alertcolor 								= Brushes.Red;
        		_normalcolor 								= Brushes.Blue;
        		_pmup 										= Brushes.Lime;
        		_pmdown 									= Brushes.DarkGreen;
        		_nmdown 									= Brushes.Red;
        		_nmup 										= Brushes.Maroon;
        		_zerolinewidth 								= 2;
        		_histogramwidth 							= 5;
				_vbshow										= true;
				_vb											= Brushes.Yellow;
				_vbopacity									= 20;
				
				AddPlot(_alertcolor, "Squeeze");
				AddPlot(_pmup, "Momentum");
				
				
			}
			else if (State == State.Configure)
			{
				Plots[0].Width = _zerolinewidth;
				Plots[1].Width = _histogramwidth;
				Plots[0].PlotStyle = PlotStyle.Dot;
				Plots[1].PlotStyle = PlotStyle.Bar;
				
				_myValue2 = new Series<double>(this);
				_bbsInd = new Series<double>(this);
				
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			
			Squeeze[0] = 0;
			// Checks to make sure we have at least 1 bar before continuing
			if (CurrentBar < 1)
				return;
			

            double avtrrg = ATR(_length)[0];
            double sd = StdDev(Close, _length)[0];
            _bbsInd[0] = (_nK * avtrrg) != 0 ? _nBb * sd / (_nK * avtrrg) : 1;
			
			
			PlotBrushes[0][0] = _bbsInd[0] <= 1 ? _alertcolor : _normalcolor;
			
			if (_bbsInd[0] <= 1 && _bbsInd[1] > 1)
			{
				_squeezeon = true;
				_startbar = CurrentBar;
			}
			else if (_bbsInd[0] > 1 && _bbsInd[1] <= 1)
			{
				_squeezeon = false;
				_startbar = CurrentBar;
			}
			
			if (_squeezeon && (CurrentBar - _startbar > 1) && _vbshow)
			{
				_pricehigh = MAX(High, CurrentBar - _startbar)[0];
				_pricelow = MIN(Low, CurrentBar - _startbar)[0];
				Draw.Rectangle(this, "Squeeze" + _startbar.ToString(), false, CurrentBar - _startbar, _pricehigh, 0, _pricelow, Brushes.Transparent, _vb, _vbopacity, true);
			}
			
			
            _myValue2[0] = (Input[0] - (((DonchianChannel(Input, _mLength).Mean.GetValueAt(CurrentBar)) + (EMA(Input, _mLength)[0])) / 2));
            Mom[0] = (LinReg(_myValue2, _mLength)[0]);
			if(CurrentBar > 0) 
				PlotBrushes[1][0] = Mom[0] > 0 ? (Mom[0] > Mom[1] ? _pmup : _pmdown) : (Mom[0] > Mom[1] ? _nmup : _nmdown);
					
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATR and StdDev Lenght", Order=1, GroupName="Parameters - Squeeze")]
		public int _length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(.1, double.MaxValue)]
		[Display(Name="Bolinger Bands Std. Devs from Average", Order=2, GroupName="Parameters - Squeeze")]
		public double _nBb
		{ get; set; }

		[NinjaScriptProperty]
		[Range(.1, double.MaxValue)]
		[Display(Name="Keltner Channel ATRs from Average", Order=3, GroupName="Parameters - Squeeze")]
		public double _nK
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Momentum Period Length", Order=4, GroupName="Parameters - Squeeze")]
		public int _mLength
		{ get; set; }
		

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="ZeroLine AlertColor", Order=5, GroupName="Parameters - Squeeze")]
		public Brush _alertcolor
		{ get; set; }
		

		[Browsable(false)]
		public string _alertcolorSerializable
		{
			get { return Serialize.BrushToString(_alertcolor); }
			set { _alertcolor = Serialize.StringToBrush(value); }
		}		

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="ZeroLine NormalColor", Order=6, GroupName="Parameters - Squeeze")]
		public Brush _normalcolor
		{ get; set; }
		

		[Browsable(false)]
		public string _normalcolorSerializable
		{
			get { return Serialize.BrushToString(_normalcolor); }
			set { _normalcolor = Serialize.StringToBrush(value); }
		}		
				[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Zero line width", Order=1, GroupName="Parameters - Visual")]
		public int _zerolinewidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Histogram (Momentum) Width", Order=2, GroupName="Parameters - Visual")]
		public int _histogramwidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Positive Momentum Rising", Order=3, GroupName="Parameters - Visual")]
		public Brush _pmup
		{ get; set; }
		

		[Browsable(false)]
		public string _pmupSerializable
		{
			get { return Serialize.BrushToString(_pmup); }
			set { _pmup = Serialize.StringToBrush(value); }
		}	
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Positive Momentum Falling", Order=4, GroupName="Parameters - Visual")]
		public Brush _pmdown
		{ get; set; }
		

		[Browsable(false)]
		public string _pmdownSerializable
		{
			get { return Serialize.BrushToString(_pmdown); }
			set { _pmdown = Serialize.StringToBrush(value); }
		}	
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Negative Momentum Falling", Order=5, GroupName="Parameters - Visual")]
		public Brush _nmdown
		{ get; set; }
		

		[Browsable(false)]
		public string _nmdownSerializable
		{
			get { return Serialize.BrushToString(_nmdown); }
			set { _nmdown = Serialize.StringToBrush(value); }
		}	
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Negative Momentum Rising", Order=6, GroupName="Parameters - Visual")]
		public Brush _nmup
		{ get; set; }
		

		[Browsable(false)]
		public string _nmupSerializable
		{
			get { return Serialize.BrushToString(_nmup); }
			set { _nmup = Serialize.StringToBrush(value); }
		}	
		
		[NinjaScriptProperty]
		[Display(Name="Show Volatility Box?", Order=7, GroupName="Parameters - Visual")]
		public bool _vbshow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name="Volatility Box Opacity", Order=8, GroupName="Parameters - Visual")]
		public int _vbopacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Volatility Box Color", Order=9, GroupName="Parameters - Visual")]
		public Brush _vb
		{ get; set; }
		

		[Browsable(false)]
		public string _vbSerializable
		{
			get { return Serialize.BrushToString(_vb); }
			set { _vb = Serialize.StringToBrush(value); }
		}	
		
		

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Squeeze
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Mom
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public bool SqueezeOn
		{
			get { return _squeezeon; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public double SqueezeHighPrice
		{
			get { return _pricehigh; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public double SqueezeLowPrice
		{
			get { return _pricelow; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public int SqueezeStartBar
		{
			get { return _startbar; }
		}
		
		#endregion
		

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private gem.gemSqueeze[] cachegemSqueeze;
		public gem.gemSqueeze gemSqueeze(int _length, double _nBb, double _nK, int _mLength, Brush _alertcolor, Brush _normalcolor, int _zerolinewidth, int _histogramwidth, Brush _pmup, Brush _pmdown, Brush _nmdown, Brush _nmup, bool _vbshow, int _vbopacity, Brush _vb)
		{
			return gemSqueeze(Input, _length, _nBb, _nK, _mLength, _alertcolor, _normalcolor, _zerolinewidth, _histogramwidth, _pmup, _pmdown, _nmdown, _nmup, _vbshow, _vbopacity, _vb);
		}

		public gem.gemSqueeze gemSqueeze(ISeries<double> input, int _length, double _nBb, double _nK, int _mLength, Brush _alertcolor, Brush _normalcolor, int _zerolinewidth, int _histogramwidth, Brush _pmup, Brush _pmdown, Brush _nmdown, Brush _nmup, bool _vbshow, int _vbopacity, Brush _vb)
		{
			if (cachegemSqueeze != null)
				for (int idx = 0; idx < cachegemSqueeze.Length; idx++)
					if (cachegemSqueeze[idx] != null && cachegemSqueeze[idx]._length == _length && cachegemSqueeze[idx]._nBb == _nBb && cachegemSqueeze[idx]._nK == _nK && cachegemSqueeze[idx]._mLength == _mLength && cachegemSqueeze[idx]._alertcolor == _alertcolor && cachegemSqueeze[idx]._normalcolor == _normalcolor && cachegemSqueeze[idx]._zerolinewidth == _zerolinewidth && cachegemSqueeze[idx]._histogramwidth == _histogramwidth && cachegemSqueeze[idx]._pmup == _pmup && cachegemSqueeze[idx]._pmdown == _pmdown && cachegemSqueeze[idx]._nmdown == _nmdown && cachegemSqueeze[idx]._nmup == _nmup && cachegemSqueeze[idx]._vbshow == _vbshow && cachegemSqueeze[idx]._vbopacity == _vbopacity && cachegemSqueeze[idx]._vb == _vb && cachegemSqueeze[idx].EqualsInput(input))
						return cachegemSqueeze[idx];
			return CacheIndicator<gem.gemSqueeze>(new gem.gemSqueeze(){ _length = _length, _nBb = _nBb, _nK = _nK, _mLength = _mLength, _alertcolor = _alertcolor, _normalcolor = _normalcolor, _zerolinewidth = _zerolinewidth, _histogramwidth = _histogramwidth, _pmup = _pmup, _pmdown = _pmdown, _nmdown = _nmdown, _nmup = _nmup, _vbshow = _vbshow, _vbopacity = _vbopacity, _vb = _vb }, input, ref cachegemSqueeze);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.gem.gemSqueeze gemSqueeze(int _length, double _nBb, double _nK, int _mLength, Brush _alertcolor, Brush _normalcolor, int _zerolinewidth, int _histogramwidth, Brush _pmup, Brush _pmdown, Brush _nmdown, Brush _nmup, bool _vbshow, int _vbopacity, Brush _vb)
		{
			return indicator.gemSqueeze(Input, _length, _nBb, _nK, _mLength, _alertcolor, _normalcolor, _zerolinewidth, _histogramwidth, _pmup, _pmdown, _nmdown, _nmup, _vbshow, _vbopacity, _vb);
		}

		public Indicators.gem.gemSqueeze gemSqueeze(ISeries<double> input , int _length, double _nBb, double _nK, int _mLength, Brush _alertcolor, Brush _normalcolor, int _zerolinewidth, int _histogramwidth, Brush _pmup, Brush _pmdown, Brush _nmdown, Brush _nmup, bool _vbshow, int _vbopacity, Brush _vb)
		{
			return indicator.gemSqueeze(input, _length, _nBb, _nK, _mLength, _alertcolor, _normalcolor, _zerolinewidth, _histogramwidth, _pmup, _pmdown, _nmdown, _nmup, _vbshow, _vbopacity, _vb);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.gem.gemSqueeze gemSqueeze(int _length, double _nBb, double _nK, int _mLength, Brush _alertcolor, Brush _normalcolor, int _zerolinewidth, int _histogramwidth, Brush _pmup, Brush _pmdown, Brush _nmdown, Brush _nmup, bool _vbshow, int _vbopacity, Brush _vb)
		{
			return indicator.gemSqueeze(Input, _length, _nBb, _nK, _mLength, _alertcolor, _normalcolor, _zerolinewidth, _histogramwidth, _pmup, _pmdown, _nmdown, _nmup, _vbshow, _vbopacity, _vb);
		}

		public Indicators.gem.gemSqueeze gemSqueeze(ISeries<double> input , int _length, double _nBb, double _nK, int _mLength, Brush _alertcolor, Brush _normalcolor, int _zerolinewidth, int _histogramwidth, Brush _pmup, Brush _pmdown, Brush _nmdown, Brush _nmup, bool _vbshow, int _vbopacity, Brush _vb)
		{
			return indicator.gemSqueeze(input, _length, _nBb, _nK, _mLength, _alertcolor, _normalcolor, _zerolinewidth, _histogramwidth, _pmup, _pmdown, _nmdown, _nmup, _vbshow, _vbopacity, _vb);
		}
	}
}

#endregion
