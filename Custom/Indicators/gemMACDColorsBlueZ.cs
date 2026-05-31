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
/*Colorized MACD indicator with one plot and time configurable alerts

    G. Eric Morgan (aka radi8 in Big Mike Trading forum)
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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.BlueZ
{
	public class gemMACDColorsBlueZ : Indicator
	{
		private Series<double>						fastMA;
		private Series<double>						slowMA;
		private	Series<double>						Average;
		private	Series<double>						diff;
		private Series<double> 						signal;
		private Series<bool> 						OB;
		private Series<bool> 						OS;
		
		
		private Series<double> _directionBlueZ;
		private Series<double> _signalBlueZ;

		
		private bool ArrowPrintedUP = false;
		private bool ArrowPrintedDOWN = false;
		SimpleFont textFontSymbolWing	= new SimpleFont("Wingdings", 10);//, FontStyle.Bold);
		private	SimpleFont		textFont;
		private	SimpleFont		textFont1;
		private	SimpleFont		textFont2;
		private	SimpleFont		textFont3;
		private int markersize = 30;
		
		#region Alerts
		
		#region Signals
		[NinjaScriptProperty]
		[Display(Name="Generate NinjaTrader Alerts", Order=1, GroupName="Alert Time Management - Signals")]
		public bool generateAlerts
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Generate Email Alerts", Order=2, GroupName="Alert Time Management - Signals")]
		public bool generateEmailSignals
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Recipient email address", Order=3, GroupName="Alert Time Management - Signals")]
		public string recipientEmail
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Audio file for Long signal alerts", Order=4, GroupName="Alert Time Management - Signals")]
		public string audioFileLong
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Audio file for Short signal alerts", Order=5, GroupName="Alert Time Management - Signals")]
		public string audioFileShort
		{ get; set; }
		
		
		
		#endregion
		
		#region Alert Times
		private bool isTimeToTrade				= false;
		
		#region Trade days of week
		[NinjaScriptProperty]
		[Display(Name="Trade on Monday", Order=1, GroupName="Alert Time Management - Trading Days")]
		public bool iTradeMonday
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trade on Tuesday", Order=2, GroupName="Alert Time Management - Trading Days")]
		public bool iTradeTuesday
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trade on Wednesday", Order=3, GroupName="Alert Time Management - Trading Days")]
		public bool iTradeWednesday
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade on Thursday", Order=4, GroupName="Alert Time Management - Trading Days")]
		public bool iTradeThursday
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade on Friday", Order=5, GroupName="Alert Time Management - Trading Days")]
		public bool iTradeFriday
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trade on Saturday", Order=6, GroupName="Alert Time Management - Trading Days")]
		public bool iTradeSaturday
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Trade on Sunday", Order=7, GroupName="Alert Time Management - Trading Days")]
		public bool iTradeSunday
		{ get; set; }
		#endregion

		#region Session Times
		[NinjaScriptProperty]
		[Range(-1, int.MaxValue)]
		[Display(Name="Session1 Start (-1 to disable)", Order=1, GroupName="Alert Time Management - Session Times")]
		public int session1Start
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-1, int.MaxValue)]
		[Display(Name="Session1 End (-1 to disable)", Order=2, GroupName="Alert Time Management - Session Times")]
		public int session1End
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-1, int.MaxValue)]
		[Display(Name="Session2 Start (-1 to disable)", Order=3, GroupName="Alert Time Management - Session Times")]
		public int session2Start
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-1, int.MaxValue)]
		[Display(Name="Session2 End (-1 to disable)", Order=4, GroupName="Alert Time Management - Session Times")]
		public int session2End
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-1, int.MaxValue)]
		[Display(Name="Session3 Start (-1 to disable)", Order=5, GroupName="Alert Time Management - Session Times")]
		public int session3Start
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-1, int.MaxValue)]
		[Display(Name="Session3 End (-1 to disable)", Order=6, GroupName="Alert Time Management - Session Times")]
		public int session3End
		{ get; set; }
		
		#endregion
		
		#endregion
		#endregion
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Colorized MACD indicator with one plot, user definable OB/OS levels and time configurable alerts.";
				Name										= "gemMACDColorsBlueZ";
				Calculate									= Calculate.OnPriceChange;//OnBarClose;
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
				fast 										= 15;//12;
				slow 										= 22;//26;
				Smooth 										= 10;//9;
				thresholdOB 								= 0.26;//10;
				thresholdOS 								= -0.69;//-10;
				requireOBOS									= true;
				MACDtype									=eMACDMABlueZ.EMA;
				
		
				upColor 									= Brushes.Lime;//Green;
				downColor 									= Brushes.Red;
				thresholdColorOB  							= Brushes.Blue;//Yellow;
				thresholdColorOS 							= Brushes.DeepPink;//Yellow;
				
				//Alert Defaults
				iTradeMonday								= true;
				iTradeTuesday								= true;
				iTradeWednesday								= true;
				iTradeThursday								= true;
				iTradeFriday								= true;
				iTradeSaturday								= false;
				iTradeSunday								= true;//false;
				session1Start 								= -1;
				session1End 								= -1;
				session2Start 								= -1;
				session2End 								= -1;
				session3Start 								= -1;
				session3End 								= -1;
				
				generateEmailSignals						= false;
				generateAlerts								= false;
				recipientEmail								= "";
				audioFileLong								= "Alert1.wav";
				audioFileShort								= "Alert1.wav";
				
				DrawArrows					= true;//false;//
				ArrowDisplacement 			= 30;
				ArrowUpColor 				= Brushes.DodgerBlue;
				ArrowDownColor 				= Brushes.Crimson;
				
//				AddPlot(Brushes.Blue, "MACDPLOT");
				AddPlot(new Stroke(Brushes.Blue, DashStyleHelper.Solid, 3), PlotStyle.Dot,  "MACDPlot");//0
//				AddLine(this.thresholdColorOB, thresholdOB, "Upper");
				AddLine(new Stroke(Brushes.Blue, DashStyleHelper.Solid, 2), thresholdOB, "Upper");
//				AddLine(this.thresholdColorOS, thresholdOS, "Lower");
				AddLine(new Stroke(Brushes.DeepPink, DashStyleHelper.Solid, 2), thresholdOS, "Lower");
//				AddLine(Brushes.LightGray, 0, "Zero");
				AddLine(new Stroke(Brushes.Yellow, DashStyleHelper.Solid, 2), 0, "Zero");
				
				
			}
			else if (State == State.Configure)
			{
//				Plots[0].Width = 3;
//				Plots[0].DashStyleHelper = DashStyleHelper.Dash;
				
				
				
			}
			else if (State == State.DataLoaded)
			{
				fastMA = new Series<double>(this);
				slowMA = new Series<double>(this);
				Average = new Series<double>(this);
				diff = new Series<double>(this);
				signal = new Series<double>(this);
				OB = new Series<bool>(this);
				OS = new Series<bool>(this);
				
				
				_directionBlueZ		= new Series<double>(this);
				_signalBlueZ			= new Series<double>(this);
			}
			
		}

		protected override void OnBarUpdate()
		{
			
			//ARROWS
			if(CurrentBar == 0)
			{
				textFont = new SimpleFont("Wingdings 3",markersize);
				textFont1 = new SimpleFont("Wingdings 3",markersize *0.8);//0.75
				textFont2 = new SimpleFont("Wingdings 2",markersize *1.5);//t or u are diamonds
				textFont3 = new SimpleFont("Wingdings 3",markersize);//t or u are diamonds
			}
			// Checks to make sure we have at least 1 bar before continuing
			if (CurrentBar < slow)
				return;
			
			bool canTrade = TimeToTrade();
			
			Lines[0].Value = thresholdOB;
			Lines[1].Value = thresholdOS;
			
			
				#region MACD Type Value assignment
					switch (MACDtype)
					{
						#region DEMA
						case eMACDMABlueZ.DEMA:
						{
							fastMA[0] = DEMA(fast)[0];
							slowMA[0] = DEMA(slow)[0];
							break;
						}
						#endregion
						
						#region EMA
						case eMACDMABlueZ.EMA:
						{
							fastMA[0] = EMA(fast)[0];
							slowMA[0] = EMA(slow)[0];
							break;
						}
						#endregion

						
						#region HMA
						case eMACDMABlueZ.HMA:
						{
							fastMA[0] = HMA(fast)[0];
							slowMA[0] = HMA(slow)[0];
							break;
						}
						#endregion
						
						#region SMA
						case eMACDMABlueZ.SMA:
						{
							fastMA[0] = SMA(fast)[0];
							slowMA[0] = SMA(slow)[0];
							break;
						}
						#endregion
						
						
						#region TEMA
						case eMACDMABlueZ.TEMA:
						{
							fastMA[0] = TEMA(fast)[0];
							slowMA[0] = TEMA(slow)[0];
							break;
						}
						#endregion
						
						#region TMA
						case eMACDMABlueZ.TMA:
						{
							fastMA[0] = TMA(fast)[0];
							slowMA[0] = TMA(slow)[0];
							break;
						}
						#endregion
						

						#region VWMA
						case eMACDMABlueZ.VWMA:
						{
							fastMA[0] = VWMA(fast)[0];
							slowMA[0] = VWMA(slow)[0];
							break;
						}
						#endregion
						
						#region WMA
						case eMACDMABlueZ.WMA:
						{
							fastMA[0] = WMA(fast)[0];
							slowMA[0] = WMA(slow)[0];
							break;
						}
						#endregion
						
						
						#region ZLEMA
						case eMACDMABlueZ.ZLEMA:
						{
							fastMA[0] = ZLEMA(fast)[0];
							slowMA[0] = ZLEMA(slow)[0];
							break;
						}
						#endregion
					}
				#endregion

				double macd		= (fastMA[0] - slowMA[0])/(TickSize*10);
				double macdAvg	= ((2.0 / (1 + Smooth)) * macd + (1 - (2.0 / (1 + Smooth))) * Avg[1]);
				
				MACDPLOT[0] = macd;
				Average[0] = macdAvg;
					
				if (macd > macdAvg) 
				{
					PlotBrushes[0][0] = upColor;
					signal[0] = 1;
				}
				else if (macd < macdAvg) 
				{
					PlotBrushes[0][0] = downColor;
					signal[0] = -1;
				}
				else if (macd == macdAvg) 
				{
					PlotBrushes[0][0] = Brushes.Yellow;
					signal[0] = 0;
				}
				
				diff[0] = (macd - macdAvg);
				if (MACDPLOT[0] > thresholdOB) OB[0] = true;
				else if (MACDPLOT[0] < thresholdOS) OS[0] = true;
			
			//Alerts
			if (
					signal[0] == 1 
					&& signal[1] == - 1
					&& (OS[0]  || !requireOBOS)
					&& canTrade
					)
				{
					if (this.generateAlerts)			
						Alert("MACD", Priority.High, "MACD Long Signal", this.audioFileLong, 30, Brushes.Black, Brushes.Green);
					if (this.generateEmailSignals)
						SendMail(this.recipientEmail, 
								"MACD Long Signal: " + Instrument.MasterInstrument.Name, 
								"Long " + Instrument.MasterInstrument.Name + " at: " + Time[0].ToString());
				}
				else if (
					signal[0] == -1 
					&& signal[1] == 1 
					&& (OB[0]  || !requireOBOS)
					&& canTrade
					
					)
				{
					if (this.generateAlerts)			
						Alert("MACD", Priority.High, "MACD Short Signal", this.audioFileShort, 30, Brushes.Black, Brushes.Red);
					if (this.generateEmailSignals)
						SendMail(this.recipientEmail, 
								"MACD Short Signal: " + Instrument.MasterInstrument.Name, 
								"Short " + Instrument.MasterInstrument.Name + " at: " + Time[0].ToString());
				}
				
			_directionBlueZ[0]=(MACDPLOT[0] > Average[0] ? 1 :  
							MACDPLOT[0] < Average[0] ? -1 : 0);
			
			_signalBlueZ[0]=((_directionBlueZ[1]<1 && _directionBlueZ[0]>0) || (CrossAbove(_directionBlueZ,0.5,1) && _directionBlueZ[0]>0)) ? 1 :	
						((_directionBlueZ[1]>-1 && _directionBlueZ[0]<0) || (CrossBelow(_directionBlueZ,-0.5,1) && _directionBlueZ[0]<0)) ? -1 : 0;
			
			if (_signalBlueZ[0] == 0)
			{
				ArrowPrintedUP = false;
				ArrowPrintedDOWN = false;
			}
			
			double val = 0;
			double spot = 0;
			
			if(DrawArrows)
			{
				if (!ArrowPrintedUP && _signalBlueZ[0]>0)
				{
					val = Low[0]-ArrowDisplacement*TickSize;
					spot = val;//Math.Min(Low[0],MACDPLOT[0])-ArrowDisplacement*TickSize;
					
//					Draw.ArrowUp(this,"sigup"+ CurrentBar, true, 0, Low[0]	-	ArrowDisplacement	* TickSize, ArrowUpColor);
					Draw.Text(this, "sigup" + (CurrentBar),true, "h"/*"l"*/, 0, spot,0, ArrowUpColor, textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 5);
					ArrowPrintedUP = true;
//						if (SoundOn) PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\ding.wav");//new
					
				}
				else 
				if (!ArrowPrintedDOWN && _signalBlueZ[0]<0)
				{
					val = High[0]+ArrowDisplacement*TickSize;
					spot = val;//Math.Max(High[0],MACDPLOT[0])+ArrowDisplacement*TickSize;
					
					//Draw.ArrowDown(this,"sigdown"+ CurrentBar, true, 0, High[0]	+	ArrowDisplacement	* TickSize, ArrowDownColor);
					Draw.Text(this, "sigdown" + (CurrentBar),true, "i"/*"l"*/, 0, spot,0, ArrowDownColor, textFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 5);
					ArrowPrintedDOWN = true;
//						if (SoundOn) PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\Chimes.wav");//new
				}
			}//end of if(DrawArrows)
				
		}//end of OnBarUpdate()
		
		public override string DisplayName
		{
			 
				get { if  (State == State.SetDefaults) return "gemMACDColorsBlueZ"; else  return "";  }
		  
		}

		#region Properties
		
		
		[Browsable(false)] [XmlIgnore()] public Series<double> DirectionBlueZ { get { Update(); return _directionBlueZ; } }
		[Browsable(false)] [XmlIgnore()] public Series<double> SignalBlueZ { get { Update(); return _signalBlueZ; } }
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast MA Period", Order=1, GroupName="Parameters")]
		public int fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow MA Period", Order=2, GroupName="Parameters")]
		public int slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smooth", Order=3, GroupName="Parameters")]
		public int Smooth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Require OB/OS Crossover to generate signal", Order=4, GroupName="Parameters")]
		public bool requireOBOS
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Threshold value to Determine overbought", Order=5, GroupName="Parameters")]
		public double thresholdOB
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Threshold value to Determine oversold", Order=6, GroupName="Parameters")]
		public double thresholdOS
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Select the type of MA to use for the MACD", Order=7, GroupName="Parameters")]
		public eMACDMABlueZ MACDtype
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Plot up color", Order=1, GroupName="Parameters - Visual")]
		public Brush upColor
		{ get; set; }
		

		[Browsable(false)]
		public string upColorSerializable
		{
			get { return Serialize.BrushToString(upColor); }
			set { upColor = Serialize.StringToBrush(value); }
		}		
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Plot down color", Order=2, GroupName="Parameters - Visual")]
		public Brush downColor
		{ get; set; }
		

		[Browsable(false)]
		public string downColorSerializable
		{
			get { return Serialize.BrushToString(downColor); }
			set { downColor = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Overbought Threshold color", Order=3, GroupName="Parameters - Visual")]
		public Brush thresholdColorOB
		{ get; set; }
		

		[Browsable(false)]
		public string thresholdColorOBSerializable
		{
			get { return Serialize.BrushToString(thresholdColorOB); }
			set { thresholdColorOB = Serialize.StringToBrush(value); }
		}		
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Oversold Threshold color", Order=4, GroupName="Parameters - Visual")]
		public Brush thresholdColorOS
		{ get; set; }
		

		[Browsable(false)]
		public string thresholdColorOSSerializable
		{
			get { return Serialize.BrushToString(thresholdColorOS); }
			set { thresholdColorOS = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MACDPLOT
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
		{
			get { return Average; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Difference
		{
			get { return diff; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
		{
			get { return signal; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<bool> StochOB
		{
			get { return OB; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<bool> StochOS
		{
			get { return OS; }
		}


		#endregion
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "01. Draw Arrows?", GroupName = "Drawing Objects", Order = 1)]	
		public bool DrawArrows
		{ get; set; }
		
		[Display(ResourceType = typeof(Custom.Resource), Name = "02. Arrow Displacement", GroupName = "Drawing Objects", Order = 2)]
		public int ArrowDisplacement
		{ get; set; }
		
		[XmlIgnore()]
		[Display(Name = "03. Color for Up Arrows?", Description = "Color for Up Arrow", GroupName = "Drawing Objects", Order = 3)]
        public Brush ArrowUpColor
		{ get; set; }
		
		[Browsable(false)]
		public string ArrowUpColorSerialize
		{ get; set; }
		
		[XmlIgnore()]
		[Display(Name = "04. Color for Down Arrows?", Description = "Color for Down Arrows", GroupName = "Drawing Objects", Order = 4)]
        public Brush ArrowDownColor
		{ get; set; }
		
		[Browsable(false)]
		public string ArrowDownColorSerialize
		{ get; set; }
		
		private bool TimeToTrade()
		{
			int block1 = 0;
			int block2 = 0;
			int block3 = 0;
			
			if (session1Start == -1 || session1End == -1)
				block1 = -1;
			if (session2Start == -1 || session2End == -1)
				block2 = -1;
			if (session3Start == -1 || session3End == -1)
				block3 = -1;
			
			if (
				(((ToTime(Time[0]) >= session1Start &&  ToTime(Time[0]) < session1End) && block1 != -1)
				||((ToTime(Time[0]) >= session2Start &&  ToTime(Time[0]) < session2End) && block2 != -1)
				||((ToTime(Time[0]) >= session3Start &&  ToTime(Time[0]) < session3End) && block3 != -1)
				|| (block1 == -1 && block2 == -1 && block3 == -1))
				&& (Time[0].DayOfWeek == DayOfWeek.Monday && this.iTradeMonday)
				)
				this.isTimeToTrade = true;
			else if (
				(((ToTime(Time[0]) >= session1Start &&  ToTime(Time[0]) < session1End) && block1 != -1)
				||((ToTime(Time[0]) >= session2Start &&  ToTime(Time[0]) < session2End) && block2 != -1)
				||((ToTime(Time[0]) >= session3Start &&  ToTime(Time[0]) < session3End) && block3 != -1)
				|| (block1 == -1 && block2 == -1 && block3 == -1))
				&& (Time[0].DayOfWeek == DayOfWeek.Tuesday && this.iTradeTuesday)
				)
				this.isTimeToTrade = true;
			else if (
				(((ToTime(Time[0]) >= session1Start &&  ToTime(Time[0]) < session1End) && block1 != -1)
				||((ToTime(Time[0]) >= session2Start &&  ToTime(Time[0]) < session2End) && block2 != -1)
				||((ToTime(Time[0]) >= session3Start &&  ToTime(Time[0]) < session3End) && block3 != -1)
				|| (block1 == -1 && block2 == -1 && block3 == -1))
				&& (Time[0].DayOfWeek == DayOfWeek.Wednesday && this.iTradeWednesday)
				)
				this.isTimeToTrade = true;
			else if (
				(((ToTime(Time[0]) >= session1Start &&  ToTime(Time[0]) < session1End) && block1 != -1)
				||((ToTime(Time[0]) >= session2Start &&  ToTime(Time[0]) < session2End) && block2 != -1)
				||((ToTime(Time[0]) >= session3Start &&  ToTime(Time[0]) < session3End) && block3 != -1)
				|| (block1 == -1 && block2 == -1 && block3 == -1))
				&& (Time[0].DayOfWeek == DayOfWeek.Thursday && this.iTradeThursday)
				)
				this.isTimeToTrade = true;
			else if (
				(((ToTime(Time[0]) >= session1Start &&  ToTime(Time[0]) < session1End) && block1 != -1)
				||((ToTime(Time[0]) >= session2Start &&  ToTime(Time[0]) < session2End) && block2 != -1)
				||((ToTime(Time[0]) >= session3Start &&  ToTime(Time[0]) < session3End) && block3 != -1)
				|| (block1 == -1 && block2 == -1 && block3 == -1))
				&& (Time[0].DayOfWeek == DayOfWeek.Friday && this.iTradeFriday)
				)
				this.isTimeToTrade = true;
			else if (
				(((ToTime(Time[0]) >= session1Start &&  ToTime(Time[0]) < session1End) && block1 != -1)
				||((ToTime(Time[0]) >= session2Start &&  ToTime(Time[0]) < session2End) && block2 != -1)
				||((ToTime(Time[0]) >= session3Start &&  ToTime(Time[0]) < session3End) && block3 != -1)
				|| (block1 == -1 && block2 == -1 && block3 == -1))
				&& (Time[0].DayOfWeek == DayOfWeek.Saturday && this.iTradeSaturday)
				)
				this.isTimeToTrade = true;
			else if (
				(((ToTime(Time[0]) >= session1Start &&  ToTime(Time[0]) < session1End) && block1 != -1)
				||((ToTime(Time[0]) >= session2Start &&  ToTime(Time[0]) < session2End) && block2 != -1)
				||((ToTime(Time[0]) >= session3Start &&  ToTime(Time[0]) < session3End) && block3 != -1)
				|| (block1 == -1 && block2 == -1 && block3 == -1))
				&& (Time[0].DayOfWeek == DayOfWeek.Sunday && this.iTradeSunday)
				)
				this.isTimeToTrade = true;
			else
				this.isTimeToTrade = false;
			
			return this.isTimeToTrade;
		}
	
        private double RoundPrice(double value)
        {
            return Bars.Instrument.MasterInstrument.RoundToTickSize(value);
        }

	}
}
	public enum eMACDMABlueZ
	{
		DEMA,
		EMA,
		HMA,
		SMA,
		TEMA,
		TMA,
		VWMA,
		WMA,
		ZLEMA
	}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BlueZ.gemMACDColorsBlueZ[] cachegemMACDColorsBlueZ;
		public BlueZ.gemMACDColorsBlueZ gemMACDColorsBlueZ(bool generateAlerts, bool generateEmailSignals, string recipientEmail, string audioFileLong, string audioFileShort, bool iTradeMonday, bool iTradeTuesday, bool iTradeWednesday, bool iTradeThursday, bool iTradeFriday, bool iTradeSaturday, bool iTradeSunday, int session1Start, int session1End, int session2Start, int session2End, int session3Start, int session3End, int fast, int slow, int smooth, bool requireOBOS, double thresholdOB, double thresholdOS, eMACDMABlueZ mACDtype, Brush upColor, Brush downColor, Brush thresholdColorOB, Brush thresholdColorOS)
		{
			return gemMACDColorsBlueZ(Input, generateAlerts, generateEmailSignals, recipientEmail, audioFileLong, audioFileShort, iTradeMonday, iTradeTuesday, iTradeWednesday, iTradeThursday, iTradeFriday, iTradeSaturday, iTradeSunday, session1Start, session1End, session2Start, session2End, session3Start, session3End, fast, slow, smooth, requireOBOS, thresholdOB, thresholdOS, mACDtype, upColor, downColor, thresholdColorOB, thresholdColorOS);
		}

		public BlueZ.gemMACDColorsBlueZ gemMACDColorsBlueZ(ISeries<double> input, bool generateAlerts, bool generateEmailSignals, string recipientEmail, string audioFileLong, string audioFileShort, bool iTradeMonday, bool iTradeTuesday, bool iTradeWednesday, bool iTradeThursday, bool iTradeFriday, bool iTradeSaturday, bool iTradeSunday, int session1Start, int session1End, int session2Start, int session2End, int session3Start, int session3End, int fast, int slow, int smooth, bool requireOBOS, double thresholdOB, double thresholdOS, eMACDMABlueZ mACDtype, Brush upColor, Brush downColor, Brush thresholdColorOB, Brush thresholdColorOS)
		{
			if (cachegemMACDColorsBlueZ != null)
				for (int idx = 0; idx < cachegemMACDColorsBlueZ.Length; idx++)
					if (cachegemMACDColorsBlueZ[idx] != null && cachegemMACDColorsBlueZ[idx].generateAlerts == generateAlerts && cachegemMACDColorsBlueZ[idx].generateEmailSignals == generateEmailSignals && cachegemMACDColorsBlueZ[idx].recipientEmail == recipientEmail && cachegemMACDColorsBlueZ[idx].audioFileLong == audioFileLong && cachegemMACDColorsBlueZ[idx].audioFileShort == audioFileShort && cachegemMACDColorsBlueZ[idx].iTradeMonday == iTradeMonday && cachegemMACDColorsBlueZ[idx].iTradeTuesday == iTradeTuesday && cachegemMACDColorsBlueZ[idx].iTradeWednesday == iTradeWednesday && cachegemMACDColorsBlueZ[idx].iTradeThursday == iTradeThursday && cachegemMACDColorsBlueZ[idx].iTradeFriday == iTradeFriday && cachegemMACDColorsBlueZ[idx].iTradeSaturday == iTradeSaturday && cachegemMACDColorsBlueZ[idx].iTradeSunday == iTradeSunday && cachegemMACDColorsBlueZ[idx].session1Start == session1Start && cachegemMACDColorsBlueZ[idx].session1End == session1End && cachegemMACDColorsBlueZ[idx].session2Start == session2Start && cachegemMACDColorsBlueZ[idx].session2End == session2End && cachegemMACDColorsBlueZ[idx].session3Start == session3Start && cachegemMACDColorsBlueZ[idx].session3End == session3End && cachegemMACDColorsBlueZ[idx].fast == fast && cachegemMACDColorsBlueZ[idx].slow == slow && cachegemMACDColorsBlueZ[idx].Smooth == smooth && cachegemMACDColorsBlueZ[idx].requireOBOS == requireOBOS && cachegemMACDColorsBlueZ[idx].thresholdOB == thresholdOB && cachegemMACDColorsBlueZ[idx].thresholdOS == thresholdOS && cachegemMACDColorsBlueZ[idx].MACDtype == mACDtype && cachegemMACDColorsBlueZ[idx].upColor == upColor && cachegemMACDColorsBlueZ[idx].downColor == downColor && cachegemMACDColorsBlueZ[idx].thresholdColorOB == thresholdColorOB && cachegemMACDColorsBlueZ[idx].thresholdColorOS == thresholdColorOS && cachegemMACDColorsBlueZ[idx].EqualsInput(input))
						return cachegemMACDColorsBlueZ[idx];
			return CacheIndicator<BlueZ.gemMACDColorsBlueZ>(new BlueZ.gemMACDColorsBlueZ(){ generateAlerts = generateAlerts, generateEmailSignals = generateEmailSignals, recipientEmail = recipientEmail, audioFileLong = audioFileLong, audioFileShort = audioFileShort, iTradeMonday = iTradeMonday, iTradeTuesday = iTradeTuesday, iTradeWednesday = iTradeWednesday, iTradeThursday = iTradeThursday, iTradeFriday = iTradeFriday, iTradeSaturday = iTradeSaturday, iTradeSunday = iTradeSunday, session1Start = session1Start, session1End = session1End, session2Start = session2Start, session2End = session2End, session3Start = session3Start, session3End = session3End, fast = fast, slow = slow, Smooth = smooth, requireOBOS = requireOBOS, thresholdOB = thresholdOB, thresholdOS = thresholdOS, MACDtype = mACDtype, upColor = upColor, downColor = downColor, thresholdColorOB = thresholdColorOB, thresholdColorOS = thresholdColorOS }, input, ref cachegemMACDColorsBlueZ);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BlueZ.gemMACDColorsBlueZ gemMACDColorsBlueZ(bool generateAlerts, bool generateEmailSignals, string recipientEmail, string audioFileLong, string audioFileShort, bool iTradeMonday, bool iTradeTuesday, bool iTradeWednesday, bool iTradeThursday, bool iTradeFriday, bool iTradeSaturday, bool iTradeSunday, int session1Start, int session1End, int session2Start, int session2End, int session3Start, int session3End, int fast, int slow, int smooth, bool requireOBOS, double thresholdOB, double thresholdOS, eMACDMABlueZ mACDtype, Brush upColor, Brush downColor, Brush thresholdColorOB, Brush thresholdColorOS)
		{
			return indicator.gemMACDColorsBlueZ(Input, generateAlerts, generateEmailSignals, recipientEmail, audioFileLong, audioFileShort, iTradeMonday, iTradeTuesday, iTradeWednesday, iTradeThursday, iTradeFriday, iTradeSaturday, iTradeSunday, session1Start, session1End, session2Start, session2End, session3Start, session3End, fast, slow, smooth, requireOBOS, thresholdOB, thresholdOS, mACDtype, upColor, downColor, thresholdColorOB, thresholdColorOS);
		}

		public Indicators.BlueZ.gemMACDColorsBlueZ gemMACDColorsBlueZ(ISeries<double> input , bool generateAlerts, bool generateEmailSignals, string recipientEmail, string audioFileLong, string audioFileShort, bool iTradeMonday, bool iTradeTuesday, bool iTradeWednesday, bool iTradeThursday, bool iTradeFriday, bool iTradeSaturday, bool iTradeSunday, int session1Start, int session1End, int session2Start, int session2End, int session3Start, int session3End, int fast, int slow, int smooth, bool requireOBOS, double thresholdOB, double thresholdOS, eMACDMABlueZ mACDtype, Brush upColor, Brush downColor, Brush thresholdColorOB, Brush thresholdColorOS)
		{
			return indicator.gemMACDColorsBlueZ(input, generateAlerts, generateEmailSignals, recipientEmail, audioFileLong, audioFileShort, iTradeMonday, iTradeTuesday, iTradeWednesday, iTradeThursday, iTradeFriday, iTradeSaturday, iTradeSunday, session1Start, session1End, session2Start, session2End, session3Start, session3End, fast, slow, smooth, requireOBOS, thresholdOB, thresholdOS, mACDtype, upColor, downColor, thresholdColorOB, thresholdColorOS);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BlueZ.gemMACDColorsBlueZ gemMACDColorsBlueZ(bool generateAlerts, bool generateEmailSignals, string recipientEmail, string audioFileLong, string audioFileShort, bool iTradeMonday, bool iTradeTuesday, bool iTradeWednesday, bool iTradeThursday, bool iTradeFriday, bool iTradeSaturday, bool iTradeSunday, int session1Start, int session1End, int session2Start, int session2End, int session3Start, int session3End, int fast, int slow, int smooth, bool requireOBOS, double thresholdOB, double thresholdOS, eMACDMABlueZ mACDtype, Brush upColor, Brush downColor, Brush thresholdColorOB, Brush thresholdColorOS)
		{
			return indicator.gemMACDColorsBlueZ(Input, generateAlerts, generateEmailSignals, recipientEmail, audioFileLong, audioFileShort, iTradeMonday, iTradeTuesday, iTradeWednesday, iTradeThursday, iTradeFriday, iTradeSaturday, iTradeSunday, session1Start, session1End, session2Start, session2End, session3Start, session3End, fast, slow, smooth, requireOBOS, thresholdOB, thresholdOS, mACDtype, upColor, downColor, thresholdColorOB, thresholdColorOS);
		}

		public Indicators.BlueZ.gemMACDColorsBlueZ gemMACDColorsBlueZ(ISeries<double> input , bool generateAlerts, bool generateEmailSignals, string recipientEmail, string audioFileLong, string audioFileShort, bool iTradeMonday, bool iTradeTuesday, bool iTradeWednesday, bool iTradeThursday, bool iTradeFriday, bool iTradeSaturday, bool iTradeSunday, int session1Start, int session1End, int session2Start, int session2End, int session3Start, int session3End, int fast, int slow, int smooth, bool requireOBOS, double thresholdOB, double thresholdOS, eMACDMABlueZ mACDtype, Brush upColor, Brush downColor, Brush thresholdColorOB, Brush thresholdColorOS)
		{
			return indicator.gemMACDColorsBlueZ(input, generateAlerts, generateEmailSignals, recipientEmail, audioFileLong, audioFileShort, iTradeMonday, iTradeTuesday, iTradeWednesday, iTradeThursday, iTradeFriday, iTradeSaturday, iTradeSunday, session1Start, session1End, session2Start, session2End, session3Start, session3End, fast, slow, smooth, requireOBOS, thresholdOB, thresholdOS, mACDtype, upColor, downColor, thresholdColorOB, thresholdColorOS);
		}
	}
}

#endregion
