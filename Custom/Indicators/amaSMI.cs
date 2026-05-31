//+----------------------------------------------------------------------------------------------+
//| Copyright © <2023>  <LizardIndicators.com - powered by AlderLab UG>
//
//| This program is free software: you can redistribute it and/or modify
//| it under the terms of the GNU General Public License as published by
//| the Free Software Foundation, either version 3 of the License, or
//| any later version.
//|
//| This program is distributed in the hope that it will be useful,
//| but WITHOUT ANY WARRANTY; without even the implied warranty of
//| MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//| GNU General Public License for more details.
//|
//| By installing this software you confirm acceptance of the GNU
//| General Public License terms. You may find a copy of the license
//| here; http://www.gnu.org/licenses/
//+----------------------------------------------------------------------------------------------+

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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.LizardIndicators
{
	/// <summary>
	/// The Stochastics Momentum Index (SMI) was first presented by  William Blau in the January 1993 issue of Stocks & Commodities. 
	/// This momentum oscillator is a modified version of the Double Smoothed Stochastics (DSS Blau), which plots on a scale from -100 to + 100.
	/// The signal line that comes with the oscillator is slightly modified when compared to the original version.
	/// </summary>
	[Gui.CategoryOrder("Input Parameters", 1000100)]
	[Gui.CategoryOrder("Threshold Values", 1000200)]
	[Gui.CategoryOrder("Display Options", 1000300)]
	[Gui.CategoryOrder("SMI Plot", 8000100)]
	[Gui.CategoryOrder("Average Plot", 8000200)]
	[Gui.CategoryOrder("Version", 8000300)]
	public class amaSMI : Indicator
	{
		private int								periodK							= 8;
		private int								smooth1							= 3;
		private int								smooth2							= 5;
		private int								signalPeriod					= 7;
		private int								displacement					= 0;
		private int								totalBarsRequiredToPlot			= 0;
		private double							k1								= 0.0;
		private double							oneMinusK1						= 0.0;
		private double							k2								= 0.0;
		private double							oneMinusK2						= 0.0;
		private double							k3								= 0.0;
		private double							oneMinusK3						= 0.0;
		private double							minLow0							= 0.0;
		private double							maxHigh0						= 0.0;
		private double							minLow1							= 0.0;
		private double							maxHigh1						= 0.0;
		private double							midpoint0						= 0.0;
		private double							smi								= 0.0;
		private double							smiAvg							= 0.0;
		private double							upperlineValue					= 50;
		private double							midlineValue					= 0;
		private double							lowerlineValue					= -50;
		private bool							showSMI							= true;
		private bool							showAvg							= true;
		private bool							showSMIShading					= true;
		private bool							showAvgShading					= true;
		private bool							applySMIShading					= true;
		private bool							applyAvgShading					= true;
		private bool							calculateFromPriceData			= true;
		private System.Windows.Media.Brush		smiOverboughtBrush				= Brushes.MediumBlue;
		private System.Windows.Media.Brush		smiOversoldBrush				= Brushes.MediumBlue;
		private System.Windows.Media.Brush		smiNeutralBrush					= Brushes.MediumBlue;
		private System.Windows.Media.Brush		avgOverboughtBrush				= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		avgOversoldBrush				= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		avgNeutralBrush					= Brushes.DarkSlateGray;
		private System.Windows.Media.Brush		smiOverboughtAreaBrush			= Brushes.Lime;
		private System.Windows.Media.Brush		smiOversoldAreaBrush			= Brushes.Red;
		private System.Windows.Media.Brush		avgOverboughtAreaBrush			= Brushes.DarkGreen;
		private System.Windows.Media.Brush		avgOversoldAreaBrush			= Brushes.DarkRed;
		private System.Windows.Media.Brush		smiOverboughtAreaBrushOpaque	= null;
		private System.Windows.Media.Brush		smiOversoldAreaBrushOpaque		= null;
		private System.Windows.Media.Brush		avgOverboughtAreaBrushOpaque	= null;
		private System.Windows.Media.Brush		avgOversoldAreaBrushOpaque		= null;
		private System.Windows.Media.Brush		upperlineBrush					= Brushes.DarkGreen;
		private System.Windows.Media.Brush		midlineBrush					= Brushes.Navy;
		private System.Windows.Media.Brush		lowerlineBrush					= Brushes.DarkRed;
		private	SharpDX.Direct2D1.Brush 		avgOverboughtBrushDX;
		private	SharpDX.Direct2D1.Brush 		avgOversoldBrushDX;
		private	SharpDX.Direct2D1.Brush 		avgNeutralBrushDX;
		private	SharpDX.Direct2D1.Brush 		smiOverboughtBrushDX;
		private	SharpDX.Direct2D1.Brush 		smiOversoldBrushDX;
		private	SharpDX.Direct2D1.Brush 		smiNeutralBrushDX;
		private	SharpDX.Direct2D1.Brush 		avgOverboughtAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		avgOversoldAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		smiOverboughtAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		smiOversoldAreaBrushDX;
		private	SharpDX.Direct2D1.Brush 		upperlineBrushDX;
		private	SharpDX.Direct2D1.Brush 		midlineBrushDX;
		private	SharpDX.Direct2D1.Brush 		lowerlineBrushDX;
		private PlotStyle						plot0Style						= PlotStyle.Line;
		private DashStyleHelper					dash0Style						= DashStyleHelper.Solid;
		private int								plot0Width						= 2;
		private PlotStyle						plot1Style						= PlotStyle.Line;
		private DashStyleHelper					dash1Style						= DashStyleHelper.Dash;
		private int								plot1Width						= 2;
		private DashStyleHelper					upperlineStyle					= DashStyleHelper.Solid;
		private int								upperlineWidth					= 1;
		private DashStyleHelper					midlineStyle					= DashStyleHelper.Solid;
		private int								midlineWidth					= 1;
		private DashStyleHelper					lowerlineStyle					= DashStyleHelper.Solid;
		private int								lowerlineWidth					= 1;
		private int								avgAreaOpacity					= 100;
		private int								smiAreaOpacity					= 80;
		private string							versionString					= "v 1.5  -  August 24, 2023";
		private Series<double>					num;
		private Series<double>					den;
		private Series<double>					smoothedNum;
		private Series<double>					smoothedDen;
		private Series<double>					doubleSmoothedNum;
		private Series<double>					doubleSmoothedDen;
		private Series<double>					smiAverage;
		private	MAX								maxHigh;
		private	MIN								minLow;
		private Series<double>					smiTrend;
		private Series<double>					avgTrend;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "\r\n The Stochastics Momentum Index (SMI) was first presented by  William Blau in the January 1993 issue of Stocks & Commodities. "
												+ " This momentum oscillator is a modified version of the Double Smoothed Stochastics (DSS Blau), which plots on a scale from -100 to + 100."
												+ " The signal line that comes with the oscillator is slightly modified when compared to the original version.";
				Name						= "amaSMI";
				IsSuspendedWhileInactive 	= true;
				ArePlotsConfigurable		= false;
				AreLinesConfigurable		= false;
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "SMI");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.Line, "SMI Average");
				AddLine(new Stroke(Brushes.Gray, 1), 50, "Upper");
				AddLine(new Stroke(Brushes.Gray, 1), 0, "Midline");
				AddLine(new Stroke(Brushes.Gray, 1), -50, "Lower");
			}
			else if (State == State.Configure)
			{
				BarsRequiredToPlot = periodK + 3*smooth1 + 3*smooth2 + 2*signalPeriod;
				totalBarsRequiredToPlot = Math.Max(0, BarsRequiredToPlot + displacement);
				if(showSMI)
				{	
					Plots[0].PlotStyle = plot0Style;
					Plots[0].DashStyleHelper = dash0Style;
					Plots[0].Width = plot0Width;
				}	
				if(showAvg)
				{	
					Plots[1].PlotStyle = plot1Style;
					Plots[1].DashStyleHelper = dash1Style;
					Plots[1].Width = plot1Width;
				}	
				Lines[0].Value = upperlineValue;
				Lines[0].Brush = upperlineBrush;
				Lines[0].DashStyleHelper = upperlineStyle;
				Lines[0].Width = upperlineWidth;
				Lines[1].Value = midlineValue;
				Lines[1].Brush = midlineBrush;
				Lines[1].DashStyleHelper = midlineStyle;
				Lines[1].Width = midlineWidth;
				Lines[2].Value = lowerlineValue;
				Lines[2].Brush = lowerlineBrush;
				Lines[2].DashStyleHelper = lowerlineStyle;
				Lines[2].Width = lowerlineWidth;
				avgOverboughtAreaBrushOpaque = avgOverboughtAreaBrush.Clone(); 
				avgOverboughtAreaBrushOpaque.Opacity = (float)avgAreaOpacity/100.0;
				avgOverboughtAreaBrushOpaque.Freeze();
				avgOversoldAreaBrushOpaque = avgOversoldAreaBrush.Clone(); 
				avgOversoldAreaBrushOpaque.Opacity = (float) avgAreaOpacity/100.0;
				avgOversoldAreaBrushOpaque.Freeze();
				smiOverboughtAreaBrushOpaque = smiOverboughtAreaBrush.Clone(); 
				smiOverboughtAreaBrushOpaque.Opacity = (float) smiAreaOpacity/100.0;
				smiOverboughtAreaBrushOpaque.Freeze();
				smiOversoldAreaBrushOpaque = smiOversoldAreaBrush.Clone(); 
				smiOversoldAreaBrushOpaque.Opacity = (float) smiAreaOpacity/100.0;
				smiOversoldAreaBrushOpaque.Freeze();
			}
			else if (State == State.DataLoaded)
			{
				num = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				den = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				smoothedNum = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				smoothedDen = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				doubleSmoothedNum = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				doubleSmoothedDen = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				smiAverage = new Series<double>(this, MaximumBarsLookBack.TwoHundredFiftySix);
				smiTrend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				avgTrend = new Series<double>(this, MaximumBarsLookBack.Infinite);
				if(Input is PriceSeries)
				{	
					calculateFromPriceData = true;
					if(periodK > 1)
					{	
						minLow = MIN(Low, periodK - 1);
						maxHigh = MAX(High, periodK - 1);
					}	
				}	
				else
				{	
					calculateFromPriceData = false;
					if(periodK > 1)
					{	
						minLow = MIN(Input, periodK - 1);
						maxHigh = MAX(Input, periodK - 1);
					}	
				}
			}
			else if (State == State.Historical)
			{
				k1 = 2.0 / (1 + smooth1);
				oneMinusK1 = 1 - k1;
				k2 = 2.0 / (1 + smooth2);
				oneMinusK2 = 1 - k2;
				k3 = 2.0 / (1 + signalPeriod);
				oneMinusK3 = 1 - k3;
				avgOverboughtAreaBrushOpaque = avgOverboughtAreaBrush.Clone(); 
				avgOverboughtAreaBrushOpaque.Opacity = (float)avgAreaOpacity/100.0;
				avgOverboughtAreaBrushOpaque.Freeze();
				avgOversoldAreaBrushOpaque = avgOversoldAreaBrush.Clone(); 
				avgOversoldAreaBrushOpaque.Opacity = (float) avgAreaOpacity/100.0;
				avgOversoldAreaBrushOpaque.Freeze();
				smiOverboughtAreaBrushOpaque = smiOverboughtAreaBrush.Clone(); 
				smiOverboughtAreaBrushOpaque.Opacity = (float) smiAreaOpacity/100.0;
				smiOverboughtAreaBrushOpaque.Freeze();
				smiOversoldAreaBrushOpaque = smiOversoldAreaBrush.Clone(); 
				smiOversoldAreaBrushOpaque.Opacity = (float) smiAreaOpacity/100.0;
				smiOversoldAreaBrushOpaque.Freeze();
			}	
			
		}

		protected override void OnBarUpdate()
		{
			if (calculateFromPriceData)
			{
				if(CurrentBar == 0 || periodK == 1)
				{
					maxHigh0 = High[0];
					minLow0 = Low[0];
					midpoint0 = 0.5 * (maxHigh0 + minLow0);
					num[0]	= Close[0] - midpoint0;
				}
				else 
				{	
					if(IsFirstTickOfBar)
					{
						maxHigh1 = maxHigh[1]; 
						minLow1 = minLow[1];
					}
					maxHigh0 = Math.Max(maxHigh1, High[0]); 
					minLow0 = Math.Min(minLow1, Low[0]);
					midpoint0 = 0.5 * (maxHigh0 + minLow0);
					num[0]	= Close[0] - midpoint0;
				}
			}	
			else
			{
				if(CurrentBar == 0 || periodK == 1)
				{
					maxHigh0 = Input[0];
					minLow0 = Input[0];
					midpoint0 = 0.5 * (maxHigh0 + minLow0);
					num[0]	= Input[0] - midpoint0;
				}
				else
				{	
					if(IsFirstTickOfBar)
					{
						maxHigh1 = maxHigh[1]; 
						minLow1 = minLow[1];
					}
					maxHigh0 = Math.Max(maxHigh1, Input[0]); 
					minLow0 = Math.Min(minLow1, Input[0]);
					midpoint0 = 0.5 * (maxHigh0 + minLow0);
					num[0]	= Input[0] - midpoint0;
				}
			}	
			den[0] = maxHigh0 - minLow0;
			
			if(CurrentBar == 0)
			{
				smoothedNum[0] = num[0];
				doubleSmoothedNum[0] = smoothedNum[0];
				smoothedDen[0] = den[0];
				doubleSmoothedDen[0] = smoothedDen[0];
				if(Math.Abs(den[0]).ApproxCompare(0) == 0)
					smi = 50;
				else
					smi = num[0]/den[0];
				SMI[0] = smi;
				smiAvg = smi;
				Average[0] = smiAvg;
			}	
			else
			{	
				smoothedNum[0] = k1 * num[0] + oneMinusK1 * smoothedNum[1];
				doubleSmoothedNum[0] = k2 * smoothedNum[0] + oneMinusK2 * doubleSmoothedNum[1];
				smoothedDen[0] = k1 * den[0] + oneMinusK1 * smoothedDen[1];
				doubleSmoothedDen[0] = k2 * smoothedDen[0] + oneMinusK2 * doubleSmoothedDen[1];
				if(Math.Abs(den[0]).ApproxCompare(0) == 0)
					smi = SMI[1];
				else
					smi = Math.Min(100.0, Math.Max (-100.0, 200.0 * doubleSmoothedNum[0]/doubleSmoothedDen[0]));  
				SMI[0] = smi;
				smiAvg = Math.Min(100.0, Math.Max (-100.0, k3 * smi + oneMinusK3 * Average[1]));
				Average[0] = smiAvg;
			}
			
			if(showSMI)
			{
				if(smi > upperlineValue)
				{	
					smiTrend[0] = 1.0;
					PlotBrushes[0][0] = smiOverboughtBrush;
				}	
				else if(smi < lowerlineValue)
				{	
					smiTrend[0] = -1.0;
					PlotBrushes[0][0] = smiOversoldBrush;
				}	
				else
				{	
					smiTrend[0] = 0.0;
					PlotBrushes[0][0] = smiNeutralBrush;
				}	
			}
			else
			{	
				if(smi > upperlineValue)
					smiTrend[0] = 1.0;
				else if(smi < lowerlineValue)
					smiTrend[0] = -1.0;
				else
					smiTrend[0] = 0.0;
				PlotBrushes[0][0] = Brushes.Transparent;
			}
			
			if(showAvg)
			{
				if(smiAvg > upperlineValue)
				{	
					avgTrend[0] = 1.0;
					PlotBrushes[1][0] = avgOverboughtBrush;
				}	
				else if(smiAvg < lowerlineValue)
				{	
					avgTrend[0] = -1.0;
					PlotBrushes[1][0] = avgOversoldBrush;
				}	
				else
				{	
					avgTrend[0] = 0.0;
					PlotBrushes[1][0] = avgNeutralBrush;
				}	
			}
			else
			{	
				if(smiAvg > upperlineValue)
					avgTrend[0] = 1.0;
				else if(smiAvg < lowerlineValue)
					avgTrend[0] = -1.0;
				else
					avgTrend[0] = 0.0;
				PlotBrushes[1][0] = Brushes.Transparent;
			}
		}
		
		#region Properties
		[Browsable(false)]	
		[XmlIgnore()]	
		public Series<double> SMI
		{
			get { return Values[0]; }
		}

		[Browsable(false)]	
		[XmlIgnore()]	
		public Series<double> Average
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SMITrend
		{
			get { return smiTrend; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> AvgTrend
		{
			get { return avgTrend; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Stochastic period", Description = "Reference period used for determining high and low", GroupName = "Parameters", Order = 0)]
		public int PeriodK
		{	
            get { return periodK; }
            set { periodK = value; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smoothing period 1", Description = "First lookback period used for smoothing numerator and denominator", GroupName = "Parameters", Order = 1)]
		public int Smooth1
		{	
            get { return smooth1; }
            set { smooth1 = value; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Smoothing period 2", Description = "Second lookback period used for smoothing numerator and denominator", GroupName = "Parameters", Order = 2)]
		public int Smooth2
		{	
            get { return smooth2; }
            set { smooth2 = value; }
		}
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Signal period", Description = "Lookback period used for calculating the signal line", GroupName = "Parameters", Order = 3)]
		public int SignalPeriod
		{	
            get { return signalPeriod; }
            set { signalPeriod = value; }
		}
		
		[Range(0, 100), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Overbought line", Description = "Sets the overbought level ffor the two oscillators", GroupName = "Threshold Values", Order = 0)]
		public double UpperlineValue
		{	
            get { return upperlineValue; }
            set { upperlineValue = value; }
		}
		
		[Range(-100, 0), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Oversold line", Description = "Sets the oversold level for the two oscillators", GroupName = "Threshold Values", Order = 1)]
		public double LowerlineValue
		{	
            get { return lowerlineValue; }
            set { lowerlineValue = value; }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show SMI", Description = "Displays SMI", GroupName = "Display Options", Order = 0)]
      	public bool ShowSMI
        {
            get { return showSMI; }
            set { showSMI = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show SMI shading", Description = "Displays SMI shading", GroupName = "Display Options", Order = 1)]
      	public bool ShowSMIShading
        {
            get { return showSMIShading; }
            set { showSMIShading = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show average", Description = "Displays average", GroupName = "Display Options", Order = 2)]
      	public bool ShowAvg
        {
            get { return showAvg; }
            set { showAvg = value; }
        }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Show average shading", Description = "Displays average shading", GroupName = "Display Options", Order = 3)]
      	public bool ShowAvgShading
        {
            get { return showAvgShading; }
            set { showAvgShading = value; }
        }
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SMI overbought", Description = "Sets the color for the SMI when overbought", GroupName = "SMI Plot", Order = 0)]
		public System.Windows.Media.Brush SMIOverboughtBrush
		{ 
			get {return smiOverboughtBrush;}
			set {smiOverboughtBrush = value;}
		}

		[Browsable(false)]
		public string SMIOverboughtBrushSerializable
		{
			get { return Serialize.BrushToString(smiOverboughtBrush); }
			set { smiOverboughtBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SMI neutral", Description = "Sets the color for the SMI when neutral", GroupName = "SMI Plot", Order = 1)]
		public System.Windows.Media.Brush SMINeutralBrush
		{ 
			get {return smiNeutralBrush;}
			set {smiNeutralBrush = value;}
		}

		[Browsable(false)]
		public string SMINeutralBrushSerializable
		{
			get { return Serialize.BrushToString(smiNeutralBrush); }
			set { smiNeutralBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SMI oversold", Description = "Sets the color for the SMI when oversold", GroupName = "SMI Plot", Order = 2)]
		public System.Windows.Media.Brush SMIOversoldBrush
		{ 
			get {return smiOversoldBrush;}
			set {smiOversoldBrush = value;}
		}

		[Browsable(false)]
		public string SMIOversoldBrushSerializable
		{
			get { return Serialize.BrushToString(smiOversoldBrush); }
			set { smiOversoldBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style SMI", Description = "Sets the dash style for the SMI", GroupName = "SMI Plot", Order = 3)]
		public DashStyleHelper Dash0Style
		{
			get { return dash0Style; }
			set { dash0Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width SMI", Description = "Sets the plot width for the SMI", GroupName = "SMI Plot", Order = 4)]
		public int Plot0Width
		{	
            get { return plot0Width; }
            set { plot0Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SMI overbought area", Description = "Sets the color for the SMI area when overbought", GroupName = "SMI Plot", Order = 5)]
		public System.Windows.Media.Brush SMIOverboughtAreaBrush
		{ 
			get {return smiOverboughtAreaBrush;}
			set {smiOverboughtAreaBrush = value;}
		}

		[Browsable(false)]
		public string SMIOverboughtAreaBrushSerializable
		{
			get { return Serialize.BrushToString(smiOverboughtAreaBrush); }
			set { smiOverboughtAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SMI oversold area", Description = "Sets the color for the SMI area when oversold", GroupName = "SMI Plot", Order = 6)]
		public System.Windows.Media.Brush SMIOversoldAreaBrush
		{ 
			get {return smiOversoldAreaBrush;}
			set {smiOversoldAreaBrush = value;}
		}

		[Browsable(false)]
		public string SMIOversoldAreaBrushSerializable
		{
			get { return Serialize.BrushToString(smiOversoldAreaBrush); }
			set { smiOversoldAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Area opacity", Description = "Sets the opacity for the overbought and oversold areas", GroupName = "SMI Plot", Order = 7)]
		public int SMIAreaOpacity
		{	
            get { return smiAreaOpacity; }
            set { smiAreaOpacity = value; }
		}		
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Average overbought", Description = "Sets the color for the average when overbought", GroupName = "Average Plot", Order = 0)]
		public System.Windows.Media.Brush AvgOverboughtBrush
		{ 
			get {return avgOverboughtBrush;}
			set {avgOverboughtBrush = value;}
		}

		[Browsable(false)]
		public string AvgOverboughtBrushSerializable
		{
			get { return Serialize.BrushToString(avgOverboughtBrush); }
			set { avgOverboughtBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Average neutral", Description = "Sets the color for the average when neutral", GroupName = "Average Plot", Order = 1)]
		public System.Windows.Media.Brush AvgNeutralBrush
		{ 
			get {return avgNeutralBrush;}
			set {avgNeutralBrush = value;}
		}

		[Browsable(false)]
		public string AvgNeutralBrushSerializable
		{
			get { return Serialize.BrushToString(avgNeutralBrush); }
			set { avgNeutralBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Average oversold", Description = "Sets the color for the average when oversold", GroupName = "Average Plot", Order = 2)]
		public System.Windows.Media.Brush AvgOversoldBrush
		{ 
			get {return avgOversoldBrush;}
			set {avgOversoldBrush = value;}
		}

		[Browsable(false)]
		public string AvgOversoldBrushSerializable
		{
			get { return Serialize.BrushToString(avgOversoldBrush); }
			set { avgOversoldBrush = Serialize.StringToBrush(value); }
		}

		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style average", Description = "Sets the dash style for the average", GroupName = "Average Plot", Order = 3)]
		public DashStyleHelper Dash1Style
		{
			get { return dash1Style; }
			set { dash1Style = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width average", Description = "Sets the plot width for the average", GroupName = "Average Plot", Order = 4)]
		public int Plot1Width
		{	
            get { return plot1Width; }
            set { plot1Width = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Average overbought area", Description = "Sets the color for the average area when overbought", GroupName = "Average Plot", Order = 5)]
		public System.Windows.Media.Brush AvgOverboughtAreaBrush
		{ 
			get {return avgOverboughtAreaBrush;}
			set {avgOverboughtAreaBrush = value;}
		}

		[Browsable(false)]
		public string AvgOverboughtAreaBrushSerializable
		{
			get { return Serialize.BrushToString(avgOverboughtAreaBrush); }
			set { avgOverboughtAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Average oversold area", Description = "Sets the color for the average area when oversold", GroupName = "Average Plot", Order = 6)]
		public System.Windows.Media.Brush AvgOversoldAreaBrush
		{ 
			get {return avgOversoldAreaBrush;}
			set {avgOversoldAreaBrush = value;}
		}

		[Browsable(false)]
		public string AvgOversoldAreaBrushSerializable
		{
			get { return Serialize.BrushToString(avgOversoldAreaBrush); }
			set { avgOversoldAreaBrush = Serialize.StringToBrush(value); }
		}
		
		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Area opacity", Description = "Sets the opacity for the overbought and oversold areas", GroupName = "Average Plot", Order = 7)]
		public int AvgAreaOpacity
		{	
            get { return avgAreaOpacity; }
            set { avgAreaOpacity = value; }
		}
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Upper line", Description = "Sets the color for the average area when oversold", GroupName = "Lines", Order = 0)]
		public System.Windows.Media.Brush UpperlineBrush
		{ 
			get {return upperlineBrush;}
			set {upperlineBrush = value;}
		}

		[Browsable(false)]
		public string UpperlineBrushSerializable
		{
			get { return Serialize.BrushToString(upperlineBrush); }
			set { upperlineBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style upper line", Description = "Sets the dash style for the upper line", GroupName = "Lines", Order = 1)]
		public DashStyleHelper UpperlineStyle
		{
			get { return upperlineStyle; }
			set { upperlineStyle = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width upper line", Description = "Sets the plot width for the upper line", GroupName = "Lines", Order = 2)]
		public int UpperlineWidth
		{	
            get { return upperlineWidth; }
            set { upperlineWidth = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Midline", Description = "Sets the color for the average area when oversold", GroupName = "Lines", Order = 3)]
		public System.Windows.Media.Brush MidlineBrush
		{ 
			get {return midlineBrush;}
			set {midlineBrush = value;}
		}

		[Browsable(false)]
		public string MidlineBrushSerializable
		{
			get { return Serialize.BrushToString(midlineBrush); }
			set { midlineBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style midline", Description = "Sets the dash style for the midline", GroupName = "Lines", Order = 4)]
		public DashStyleHelper MidlineStyle
		{
			get { return midlineStyle; }
			set { midlineStyle = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width midline", Description = "Sets the plot width for the midline", GroupName = "Lines", Order = 5)]
		public int MidlineWidth
		{	
            get { return midlineWidth; }
            set { midlineWidth = value; }
		}
			
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Lower line", Description = "Sets the color for the average area when oversold", GroupName = "Lines", Order = 6)]
		public System.Windows.Media.Brush LowerlineBrush
		{ 
			get {return lowerlineBrush;}
			set {lowerlineBrush = value;}
		}

		[Browsable(false)]
		public string LowerlineBrushSerializable
		{
			get { return Serialize.BrushToString(lowerlineBrush); }
			set { lowerlineBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "Dash style lower line", Description = "Sets the dash style for the lower line", GroupName = "Lines", Order = 7)]
		public DashStyleHelper LowerlineStyle
		{
			get { return lowerlineStyle; }
			set { lowerlineStyle = value; }
		}
		
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Plot width lower line", Description = "Sets the plot width for the lower line", GroupName = "Lines", Order = 8)]
		public int LowerlineWidth
		{	
            get { return lowerlineWidth; }
            set { lowerlineWidth = value; }
		}
				[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Release and date", Description = "Release and date", GroupName = "Version", Order = 0)]
		public string VersionString
		{	
            get { return versionString; }
            set { ; }
		}
		#endregion
		
		#region Miscellaneous
		
		public override void OnRenderTargetChanged()
		{
			if (avgOverboughtBrushDX != null)
				avgOverboughtBrushDX.Dispose();
			if (avgOversoldBrushDX != null)
				avgOversoldBrushDX.Dispose();
			if (avgNeutralBrushDX != null)
				avgNeutralBrushDX.Dispose();
			if (smiOverboughtBrushDX != null)
				smiOverboughtBrushDX.Dispose();
			if (smiOversoldBrushDX != null)
				smiOversoldBrushDX.Dispose();
			if (smiNeutralBrushDX != null)
				smiNeutralBrushDX.Dispose();
			if (avgOverboughtAreaBrushDX != null)
				avgOverboughtAreaBrushDX.Dispose();
			if (avgOversoldAreaBrushDX != null)
				avgOversoldAreaBrushDX.Dispose();
			if (smiOverboughtAreaBrushDX != null)
				smiOverboughtAreaBrushDX.Dispose();
			if (smiOversoldAreaBrushDX != null)
				smiOversoldAreaBrushDX.Dispose();
			if (upperlineBrushDX != null)
				upperlineBrushDX.Dispose();
			if (midlineBrushDX != null)
				midlineBrushDX.Dispose();
			if (lowerlineBrushDX != null)
				lowerlineBrushDX.Dispose();
			
			if (RenderTarget != null)
			{
				try
				{
					avgOverboughtBrushDX = avgOverboughtBrush.ToDxBrush(RenderTarget);
					avgOversoldBrushDX = avgOversoldBrush.ToDxBrush(RenderTarget);
					avgNeutralBrushDX = avgNeutralBrush.ToDxBrush(RenderTarget);
					smiOverboughtBrushDX = smiOverboughtBrush.ToDxBrush(RenderTarget);
					smiOversoldBrushDX = smiOversoldBrush.ToDxBrush(RenderTarget);
					smiNeutralBrushDX = smiNeutralBrush.ToDxBrush(RenderTarget);
					avgOverboughtAreaBrushDX = avgOverboughtAreaBrushOpaque.ToDxBrush(RenderTarget);
					avgOversoldAreaBrushDX = avgOversoldAreaBrushOpaque.ToDxBrush(RenderTarget);
					smiOverboughtAreaBrushDX = smiOverboughtAreaBrushOpaque.ToDxBrush(RenderTarget);
					smiOversoldAreaBrushDX = smiOversoldAreaBrushOpaque.ToDxBrush(RenderTarget);
					upperlineBrushDX = upperlineBrush.ToDxBrush(RenderTarget);
					midlineBrushDX = midlineBrush.ToDxBrush(RenderTarget);
					lowerlineBrushDX = lowerlineBrush.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (Bars == null || ChartControl == null || ChartBars.ToIndex < BarsRequiredToPlot || !IsVisible) return;
			int	lastBarPainted = ChartBars.ToIndex;
			if(lastBarPainted  < 0 || BarsArray[0].Count < lastBarPainted) return;
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
	        ChartPanel panel = chartControl.ChartPanels[ChartPanel.PanelIndex];
			
			bool nonEquidistant 	= (chartControl.BarSpacingType == BarSpacingType.TimeBased || chartControl.BarSpacingType == BarSpacingType.EquidistantMulti);
			int lastBarCounted		= Close.Count - 1;
			int	lastBarOnUpdate		= lastBarCounted - (Calculate == Calculate.OnBarClose ? 1 : 0);
			int	lastBarIndex		= Math.Min(lastBarPainted, lastBarOnUpdate);
			int firstBarPainted	 	= ChartBars.FromIndex;
			int firstBarIndex  	 	= Math.Max(totalBarsRequiredToPlot, firstBarPainted); 
			int lastIndex			= 0;
			int x 					= 0;
			int y0	 				= 0;
			int y1 					= 0;
			int yUpper 				= 0;
			int yMid 				= 0;
			int yLower 				= 0;
			int y 					= 0;
			int t					= 0;
			int lastX 				= 0;
			int lastY1 				= 0;
			int lastY2 				= 0;
			int lastY 				= 0;
			int sign 				= 0;
			int lastSign 			= 0;
			int startBar 			= 0;
			int returnBar 			= 0;
			int count	 			= 0;
			double barWidth			= 0;
			bool firstLoop 			= true;
			SharpDX.Vector2 startPointDX;
			SharpDX.Vector2 endPointDX;
			Vector2[] plotArray 	= new Vector2[2 * (lastBarIndex - firstBarIndex + Math.Max(0, displacement) + 1)]; 
			
			if(lastBarIndex + displacement > firstBarIndex)
			{	
				if (displacement >= 0)
					lastIndex = lastBarIndex + displacement;
				else
					lastIndex = Math.Min(lastBarIndex, lastBarOnUpdate + displacement);
				if(nonEquidistant && lastIndex > lastBarOnUpdate)
					barWidth = Convert.ToDouble(ChartControl.GetXByBarIndex(ChartBars, lastBarPainted) - ChartControl.GetXByBarIndex(ChartBars, lastBarPainted - displacement))/displacement;
				lastY1	= chartScale.GetYByValue(Average.GetValueAt(lastIndex - displacement));
				lastY2	= chartScale.GetYByValue(SMI.GetValueAt(lastIndex - displacement));
				yUpper  = chartScale.GetYByValue(upperlineValue);
				yMid  = chartScale.GetYByValue(midlineValue);
				yLower  = chartScale.GetYByValue(lowerlineValue);
				
				//Lines
				startPointDX = new SharpDX.Vector2((float)ChartPanel.X, (float)yUpper);
				endPointDX	= new SharpDX.Vector2((float)ChartPanel.X + ChartPanel.W, (float)yUpper);
				RenderTarget.DrawLine(startPointDX, endPointDX, upperlineBrushDX, upperlineWidth, Lines[0].StrokeStyle);
				startPointDX = new SharpDX.Vector2((float)ChartPanel.X, (float)yMid);
				endPointDX	= new SharpDX.Vector2((float)ChartPanel.X + ChartPanel.W, (float)yMid);
				RenderTarget.DrawLine(startPointDX, endPointDX, midlineBrushDX, midlineWidth, Lines[1].StrokeStyle);
				startPointDX = new SharpDX.Vector2((float)ChartPanel.X, (float)yLower);
				endPointDX	= new SharpDX.Vector2((float)ChartPanel.X + ChartPanel.W, (float)yLower);
				RenderTarget.DrawLine(startPointDX, endPointDX, lowerlineBrushDX, lowerlineWidth, Lines[2].StrokeStyle);
				
				// Average Shading
				if(applyAvgShading)
				{	
					lastY = lastY1;
					lastSign = (int) AvgTrend.GetValueAt(lastIndex - displacement);
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathF;
						SharpDX.Direct2D1.GeometrySink 	sinkF;
						pathF = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathF)
						{
							if(firstLoop)
							{	
								count = 0;						
								if(nonEquidistant && displacement > 0 && startBar > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((startBar - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, startBar);
								if (lastSign == 1)
									plotArray[count] = new Vector2(x, yUpper);
								else if(lastSign == -1)
									plotArray[count] = new Vector2(x, yLower);
							}	
							else
							{	
								count = 0;
								plotArray[count] = new Vector2(lastX, lastY);
							}
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Average.GetValueAt(idx - displacement));   
								t = (int) AvgTrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = t;
									lastX = x;
									lastY = y;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							if(sign == 0)
							{	
								firstLoop = false;
								continue;
							}	
							if(startBar == firstBarIndex)
							{
								count = count + 1;
								if(sign == 1)
									plotArray[count] = new Vector2(lastX, yUpper);
								else if(sign == -1)
									plotArray[count] = new Vector2(lastX, yLower);
							}	
							sinkF = pathF.Open();
							sinkF.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkF.AddLine(plotArray[i]);
							sinkF.EndFigure(FigureEnd.Closed);
			        		sinkF.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.FillGeometry(pathF, avgOverboughtAreaBrushDX);
							else if(sign == -1) 
		 						RenderTarget.FillGeometry(pathF, avgOversoldAreaBrushDX);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathF.Dispose();
						sinkF.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && Average.IsValidDataPointAt(startBar - displacement));
				}
				
				// SMI Shading
				if(applySMIShading)
				{	
					lastY = lastY2;
					lastSign = (int) SMITrend.GetValueAt(lastIndex - displacement);
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathF;
						SharpDX.Direct2D1.GeometrySink 	sinkF;
						pathF = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathF)
						{
							if(firstLoop)
							{	
								count = 0;						
								if(nonEquidistant && displacement > 0 && startBar > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((startBar - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, startBar);
								if (lastSign == 1)
									plotArray[count] = new Vector2(x, yUpper);
								else if(lastSign == -1)
									plotArray[count] = new Vector2(x, yLower);
							}	
							else
							{	
								count = 0;
								plotArray[count] = new Vector2(lastX, lastY);
							}
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(SMI.GetValueAt(idx - displacement));   
								t = (int) SMITrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = t;
									lastX = x;
									lastY = y;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									startBar = idx;
									sign = lastSign;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							if(sign == 0)
							{	
								firstLoop = false;
								continue;
							}	
							if(startBar == firstBarIndex)
							{
								count = count + 1;
								if(sign == 1)
									plotArray[count] = new Vector2(lastX, yUpper);
								else if(sign == -1)
									plotArray[count] = new Vector2(lastX, yLower);
							}	
							sinkF = pathF.Open();
							sinkF.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkF.AddLine(plotArray[i]);
							sinkF.EndFigure(FigureEnd.Closed);
			        		sinkF.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.FillGeometry(pathF, smiOverboughtAreaBrushDX);
							else if(sign == -1) 
		 						RenderTarget.FillGeometry(pathF, smiOversoldAreaBrushDX);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathF.Dispose();
						sinkF.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && SMI.IsValidDataPointAt(startBar - displacement));
				}
				
				// SMI Average
				if(showAvg)
				{	
					lastY = lastY1;
					lastSign = 5;
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathE;
						SharpDX.Direct2D1.GeometrySink 	sinkE;
						pathE = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathE)
						{
							count = 0;
							plotArray[count] = new Vector2(lastX, lastY);
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(Average.GetValueAt(idx - displacement));   
								t = (int) AvgTrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									sign = t;
									startBar = idx;
									lastX = x;
									lastY = y;
								}	
								else if(lastSign == 5)
								{	
									plotArray[count] = new Vector2(x,y);
									sign = 5;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							sinkE = pathE.Open();
							if(firstLoop)
								sinkE.BeginFigure(plotArray[1], FigureBegin.Filled);
							else
								sinkE.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkE.AddLine(plotArray[i]);
							sinkE.EndFigure(FigureEnd.Open);
			        		sinkE.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.DrawGeometry(pathE, avgOverboughtBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == 0) 
		 						RenderTarget.DrawGeometry(pathE, avgNeutralBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							else if(sign == -1) 
		 						RenderTarget.DrawGeometry(pathE, avgOversoldBrushDX, Plots[1].Width, Plots[1].StrokeStyle);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathE.Dispose();
						sinkE.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && Average.IsValidDataPointAt(startBar - displacement));
				}

				// SMI
				if(showSMI)
				{	
					lastY = lastY2;
					lastSign = 5;
					startBar = lastIndex;
					firstLoop = true;
					do
					{
						SharpDX.Direct2D1.PathGeometry 	pathE;
						SharpDX.Direct2D1.GeometrySink 	sinkE;
						pathE = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						using (pathE)
						{
							count = 0;
							plotArray[count] = new Vector2(lastX, lastY);
							for (int idx = startBar; idx >= firstBarIndex; idx --)	
							{
								if(nonEquidistant && displacement > 0 && idx > lastBarCounted)
									x = ChartControl.GetXByBarIndex(ChartBars, lastBarCounted) + (int)((idx - lastBarCounted) * barWidth);
								else
									x = ChartControl.GetXByBarIndex(ChartBars, idx);
								y = chartScale.GetYByValue(SMI.GetValueAt(idx - displacement));   
								t = (int) SMITrend.GetValueAt(idx - displacement);
								count = count + 1;
								if(t == lastSign)
								{
									plotArray[count] = new Vector2(x,y);
									sign = t;
									startBar = idx;
									lastX = x;
									lastY = y;
								}	
								else if(lastSign == 5)
								{	
									plotArray[count] = new Vector2(x,y);
									sign = 5;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}	
								else if (t == 0 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 0 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == 1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 0)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = t;
									break;
								}
								else if (t == -1 && lastSign == 1)
								{	
									double lastDiff = Convert.ToDouble(yUpper - lastY);
									double diff = Convert.ToDouble(yUpper - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yUpper;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
								else if (t == 1 && lastSign == -1)
								{	
									double lastDiff = Convert.ToDouble(yLower - lastY);
									double diff = Convert.ToDouble(yLower - y);
									double denominator = lastDiff - diff;
									if(denominator.ApproxCompare(0) == 0)
										x = lastX - Convert.ToInt32((lastX - x) * 0.5);
									else	
										x = lastX - Convert.ToInt32((lastX - x) * lastDiff / denominator);
									y = yLower;
									plotArray[count] = new Vector2(x,y);
									sign = lastSign;
									startBar = idx;
									lastX = x;
									lastY = y;
									lastSign = 0;
									break;
								}
							}
							sinkE = pathE.Open();
							if(firstLoop)
								sinkE.BeginFigure(plotArray[1], FigureBegin.Filled);
							else
								sinkE.BeginFigure(plotArray[0], FigureBegin.Filled);
							for (int i = 1; i <= count; i++)
								sinkE.AddLine(plotArray[i]);
							sinkE.EndFigure(FigureEnd.Open);
			        		sinkE.Close();
							RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
							if(sign == 1)
		 						RenderTarget.DrawGeometry(pathE, smiOverboughtBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							else if(sign == 0) 
		 						RenderTarget.DrawGeometry(pathE, smiNeutralBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							else if(sign == -1) 
		 						RenderTarget.DrawGeometry(pathE, smiOversoldBrushDX, Plots[0].Width, Plots[0].StrokeStyle);
							RenderTarget.AntialiasMode = oldAntialiasMode;
						}
						pathE.Dispose();
						sinkE.Dispose();
						firstLoop = false;
					}	
					while (startBar > firstBarIndex && SMI.IsValidDataPointAt(startBar - displacement));
				}			
			}
		}		
		#endregion		

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LizardIndicators.amaSMI[] cacheamaSMI;
		public LizardIndicators.amaSMI amaSMI(int periodK, int smooth1, int smooth2, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return amaSMI(Input, periodK, smooth1, smooth2, signalPeriod, upperlineValue, lowerlineValue);
		}

		public LizardIndicators.amaSMI amaSMI(ISeries<double> input, int periodK, int smooth1, int smooth2, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			if (cacheamaSMI != null)
				for (int idx = 0; idx < cacheamaSMI.Length; idx++)
					if (cacheamaSMI[idx] != null && cacheamaSMI[idx].PeriodK == periodK && cacheamaSMI[idx].Smooth1 == smooth1 && cacheamaSMI[idx].Smooth2 == smooth2 && cacheamaSMI[idx].SignalPeriod == signalPeriod && cacheamaSMI[idx].UpperlineValue == upperlineValue && cacheamaSMI[idx].LowerlineValue == lowerlineValue && cacheamaSMI[idx].EqualsInput(input))
						return cacheamaSMI[idx];
			return CacheIndicator<LizardIndicators.amaSMI>(new LizardIndicators.amaSMI(){ PeriodK = periodK, Smooth1 = smooth1, Smooth2 = smooth2, SignalPeriod = signalPeriod, UpperlineValue = upperlineValue, LowerlineValue = lowerlineValue }, input, ref cacheamaSMI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LizardIndicators.amaSMI amaSMI(int periodK, int smooth1, int smooth2, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaSMI(Input, periodK, smooth1, smooth2, signalPeriod, upperlineValue, lowerlineValue);
		}

		public Indicators.LizardIndicators.amaSMI amaSMI(ISeries<double> input , int periodK, int smooth1, int smooth2, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaSMI(input, periodK, smooth1, smooth2, signalPeriod, upperlineValue, lowerlineValue);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LizardIndicators.amaSMI amaSMI(int periodK, int smooth1, int smooth2, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaSMI(Input, periodK, smooth1, smooth2, signalPeriod, upperlineValue, lowerlineValue);
		}

		public Indicators.LizardIndicators.amaSMI amaSMI(ISeries<double> input , int periodK, int smooth1, int smooth2, int signalPeriod, double upperlineValue, double lowerlineValue)
		{
			return indicator.amaSMI(input, periodK, smooth1, smooth2, signalPeriod, upperlineValue, lowerlineValue);
		}
	}
}

#endregion
