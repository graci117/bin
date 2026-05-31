// 
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>.
// =============================================================================================================================== //
// 	Central Pivot Range:
//		The Central Pivot Range (CPR) is an indicator to identify key price points to set up trades. CPR is beneficial for intraday trading.
//		The CPR consists of three components:
//
//		1. Pivot (CPR)
//		2. Bottom Central Pivot (BC)
//		3. Top Cetral Pivot (TC)
//
//		Formula:
//      	CPR = (PreviousHigh + PreviosLow + PreviousClose) / 3
//			BC = (PreviosHigh + PreviousLow) / 2
//			TC = (CPR - BC) + CPR
// ================================================================================================================================ //
//###
//###  User	 	Date
//###  ------   ------
//###  SQuant   Nov 21 2021
//###
// =============================================================================================================================== //

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
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SQuantLevels : Indicator
	{	
#region Class Variables
	
		private DateTime				currentDate			=	Core.Globals.MinDate;		
		private DateTime				lastDate			= 	Core.Globals.MinDate;
		
		private double curC;
		private double curH;
		private double curL;
		private double curO;
		private double hgap;
		
		private double r;
		
		private double r6;
		private double r3;
		private double r4;
		private double r5;
		private double r35;
		private double s3;
		private double s4;
		private double s6;
		private double s35;
		private double s5;
		
		//private double					userDefinedClose;
		
		private double curTC;
		private double curBC;
		private double curCPR;
		
		private Brush ColorAreaBear;
		private Brush ColorAreaBull;
		
		int opacity = 15;
	
		
		NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Arial", 11) { /*Size = 0, */Bold = false};
		
#endregion
		
#region OnStateChange
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description										= @"By: Nick Scott 1999";
				Name												= "SQuantLevels";
				Calculate											= Calculate.OnBarClose;
				IsOverlay											= true;
				DisplayInDataBox						= true;
				DrawOnPricePanel						= true;
				DrawHorizontalGridLines					= true;
				DrawVerticalGridLines					= true;
				PaintPriceMarkers						= true;
				ScaleJustification						= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive				= true;
				
				ShowClose = true;
				ShowHigh = true;
				ShowLow = true;
				ShowCPR = true;	
				ShowHGap = true;
				
			
				
				AddPlot(new Stroke(Brushes.MediumSeaGreen, DashStyleHelper.Solid, 1), PlotStyle.Hash, "Tgt2");
				AddPlot(new Stroke(Brushes.MediumSeaGreen, DashStyleHelper.Solid, 1), PlotStyle.Hash, "Tgt1");
				AddPlot(new Stroke(Brushes.ForestGreen, DashStyleHelper.Solid, 2), PlotStyle.Hash, "BearLastStand");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 1), PlotStyle.Hash, "BearishReversalZone1");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 1), PlotStyle.Hash, "BearishReversalZone2");
				AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 1), PlotStyle.Hash,"BullishReversalZone1");
				AddPlot(new Stroke(Brushes.Green, DashStyleHelper.Solid, 1), PlotStyle.Hash,"BullishRerversalZone2");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 2), PlotStyle.Hash,"BullLastStand");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 1), PlotStyle.Hash, "TgtB1");
				AddPlot(new Stroke(Brushes.Red, DashStyleHelper.Solid, 1), PlotStyle.Hash,"TgtB2");
				
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Hash,"PClose");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Hash, "PHigh");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Hash, "PLow");
				AddPlot(new Stroke(Brushes.DimGray, 1), PlotStyle.Hash, "HGap");
				
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Hash, "TC");
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Hash, "BC");
				AddPlot(new Stroke(Brushes.Magenta, 2), PlotStyle.Hash, "CPR");
			
			}
			else if (State == State.Configure)
			{
				
			}
			else if (State == State.DataLoaded)
			{
				
			}
		}
#endregion
		
#region OnBarUpdate
		protected override void OnBarUpdate()
		{	
			//Comprueba si hay mas de X barras en el grafico
			if(CurrentBar < 10) return;
			//Import Method
			PriorDayOHLC valueSource = PriorDayOHLC(); 
			
				
			curH = PriorDayOHLC().PriorHigh[0];
			curL = PriorDayOHLC().PriorLow[0];
			//curC = userDefinedClose;
			curC = PriorDayOHLC().PriorClose[0];
			curO = PriorDayOHLC().PriorOpen[0];		
			//Range
			r = curH - curL;
			hgap = (curC + curO) / 2;	
			
			
			#region Calculate
			r6 = (curH / curL) * curC; // Bull Target 2 
			r3 = curC + r * (1.1 / 4);//Bear Reversal High
			r4 = curC + r * (1.1 / 2); // Bear Last Stand
			r5 = r4 + 1.168 * (r4 - r3); //Bull Target 1
			r35 = r4 - ( r4 - r3) / 2; // Bear Reversal Low
					
			s3 = curC - r * (1.1 / 4); // Bull Reversal High
			s4 = curC - r * (1.1 / 2); // Bull Last Stand
			s6 = curC - (r6 - curC); //Bear Target 2
			s35 = s3 - (s3 - s4) / 2; // Bull Reversal Low
			s5 = s4 - 1.168 * (s3 -s4); //Bear Target 1
					
			curCPR = ( curH + curL + curC) / 3;
			curBC = (curH + curL) / 2;
			curTC = curCPR - curBC + curCPR;
			
			
			 
			 //Visuals
			Tgt2[0] = r6;
			Tgt1[0] = r5;
			BearLastStand[0] = r4;
			BearishReversalZone1[0] = r3;
			BearishReversalZone2[0] = r35;
			BullishReversalZone1[0] = s3;
			BullishRerversalZone2[0] = s35;
			BullLastStand[0] = s4;
			TgtB1[0] = s5;
			TgtB2[0] = s6;

			
		
			//Draw.Rectangle(this, "CentralPivots" + CurrentBar, false, 1, BC[0], 0, TC[0], Brushes.Transparent, Brushes.SlateGray, opacity);
			Draw.Text(this, "D TC", false, "	-> D TC", 0, TC[0], 0, Brushes.DodgerBlue, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
			Draw.Text(this, "D BC", false, "	-> D BC", 0, BC[0], 0, Brushes.DodgerBlue, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
			
			
			if(ShowHGap){
				HGap[0] = hgap;
				//Draw.Rectangle(this, "HGap" + CurrentBar, false, 1, HGap[0], 0, HGap[0], Brushes.Goldenrod, Brushes.Aqua, opacity);
				Draw.Text(this, "HGap", false, "	-> HGap", 0, HGap[0], 0, Brushes.DimGray, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
			}
			//if(ShowTC) TC[0] = curBC;
			if(ShowCPR){
				
				TC[0] = curTC;
				BC[0] = curBC;
				CPR[0] = curCPR;
				
				Draw.Text(this, "D CPR", false, "	-> D CPR", 0, CPR[0], 0, Brushes.Magenta, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);				
				Draw.Text(this, "D TC", false, "	-> D TC", 0, TC[0], 0, Brushes.DodgerBlue, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "D BC", false, "	-> D BC", 0, BC[0], 0, Brushes.DodgerBlue, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
			
			}
			if(ShowHigh){
				PHigh[0] = curH;
				Draw.Text(this, "D High", false, "	-> D High", 0, PHigh[0], 0, Brushes.DimGray, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);	
			}
			
			if(ShowLow){
				PLow[0] = curL;
				Draw.Text(this, "D Low", false, "	-> D Low", 0, PLow[0], 0, Brushes.DimGray, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
			}
			
			if(ShowClose){
				PClose[0] = curC;
				Draw.Text(this, "D Close", false, "	-> D Close", 0, PClose[0], 0, Brushes.DimGray, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);	
			}
			#endregion

			#region Visual
			if(CurrentBar>10){
				
				Draw.Rectangle(this, "BearZone" + CurrentBar, false, 1, BearishReversalZone2[0], 0, BearishReversalZone1[0], Brushes.Transparent, Brushes.Red, opacity);
				Draw.Rectangle(this, "BullZone" + CurrentBar, false, 1, BullishReversalZone1[0], 0, BullishRerversalZone2[0], Brushes.Transparent, Brushes.LimeGreen, opacity);
				//Draw.Rectangle(this, "CentralPivots" + CurrentBar, false, 1, BC[0], 0, TC[0], Brushes.Transparent, Brushes.SlateGray, opacity);
				
				
				/*Draw.Text(NinjaScriptBase owner, string tag, bool isAutoScale, string text, int barsAgo, double y, int yPixelOffset, Brush textBrush, SimpleFont font, TextAlignment alignment, Brush outlineBrush, Brush areaBrush, int areaOpacity)*/
				Draw.Text(this, "Target 2", false, "	-> Target 2", 0, Tgt2[0], 0, Brushes.ForestGreen, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "Target 1", false, "	-> Target 1", 0, Tgt1[0], 0, Brushes.ForestGreen, myFont, TextAlignment.Justify, Brushes.Transparent, null, 30);
				Draw.Text(this, "BearLastStand", false, "	-> BearLastStand", 0, BearLastStand[0], 0, Brushes.ForestGreen, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "BearishReversalZoneL", false, "	-> BearishReversalZoneL", 0, BearishReversalZone1[0], 0, Brushes.Red, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "BearishReversalZoneH", false, "	-> BearishReversalZoneH", 0, BearishReversalZone2[0], 0, Brushes.Red, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				
				Draw.Text(this, "BullishReversalZoneL", false, "	-> BullReversalZoneL", 0, BullishReversalZone1[0], 0, Brushes.LimeGreen, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "BullishReversalZoneH", false, "	-> BullReversalZoneH", 0, BullishRerversalZone2[0], 0, Brushes.LimeGreen, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "BullLastStand", false, "	-> BullLastStand", 0, BullLastStand[0], 0, Brushes.Red, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "Target -1", false, "	-> Target -1", 0, TgtB1[0], 0, Brushes.Red, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				Draw.Text(this, "Target -2", false, "	-> Target -2", 0, TgtB2[0], 0, Brushes.Red, myFont, TextAlignment.Justify, Brushes.Transparent, null, 1);
				 
			}
			
			
			
			
			#endregion
		}
		#endregion

#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Tgt2
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Tgt1
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BearLastStand
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BearishReversalZone1
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BearishReversalZone2
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BullishReversalZone1
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BullishRerversalZone2
		{
			get { return Values[6]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BullLastStand
		{
			get { return Values[7]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TgtB1
		{
			get { return Values[8]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TgtB2
		{
			get { return Values[9]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PClose
		{
			get { return Values[10]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PHigh
		{
			get { return Values[11]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PLow
		{
			get { return Values[12]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HGap
		{
			get { return Values[13]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TC
		{
			get { return Values[14]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BC
		{
			get { return Values[15]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CPR
		{
			get { return Values[16]; }
		}
		
		[NinjaScriptProperty]
		[Display(Description = "Opacity Min - 1; Max - 10;", GroupName = "Parameters", Order = 6)]
		public int Opacity
		{
			get{return opacity;}
			set{opacity = Math.Max(1,value);
				opacity = Math.Min(30,opacity);}
		}
	
#endregion

#region Shows

		[Display(ResourceType = typeof(Custom.Resource), Name = "Close", GroupName = "NinjaScriptParameters", Order = 1)]
		public bool ShowClose
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "High", GroupName = "NinjaScriptParameters", Order = 1)]
		public bool ShowHigh
		{ get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "Low", GroupName = "NinjaScriptParameters", Order = 2)]
		public bool ShowLow
		{ get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "CPR", GroupName = "NinjaScriptParameters", Order = 3)]
		public bool ShowCPR
		{ get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "HGap", GroupName = "NinjaScriptParameters", Order = 4)]
		public bool ShowHGap
		{ get; set; }

	}
}
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SQuantLevels[] cacheSQuantLevels;
		public SQuantLevels SQuantLevels(int opacity)
		{
			return SQuantLevels(Input, opacity);
		}

		public SQuantLevels SQuantLevels(ISeries<double> input, int opacity)
		{
			if (cacheSQuantLevels != null)
				for (int idx = 0; idx < cacheSQuantLevels.Length; idx++)
					if (cacheSQuantLevels[idx] != null && cacheSQuantLevels[idx].Opacity == opacity && cacheSQuantLevels[idx].EqualsInput(input))
						return cacheSQuantLevels[idx];
			return CacheIndicator<SQuantLevels>(new SQuantLevels(){ Opacity = opacity }, input, ref cacheSQuantLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SQuantLevels SQuantLevels(int opacity)
		{
			return indicator.SQuantLevels(Input, opacity);
		}

		public Indicators.SQuantLevels SQuantLevels(ISeries<double> input , int opacity)
		{
			return indicator.SQuantLevels(input, opacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SQuantLevels SQuantLevels(int opacity)
		{
			return indicator.SQuantLevels(Input, opacity);
		}

		public Indicators.SQuantLevels SQuantLevels(ISeries<double> input , int opacity)
		{
			return indicator.SQuantLevels(input, opacity);
		}
	}
}

#endregion
