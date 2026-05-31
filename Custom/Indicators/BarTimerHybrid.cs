
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
using System.Globalization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
//using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;
//using System.Net.NetworkInformation;
using System.Net.Security;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Timers;
//using System.Drawing;

#endregion

#region Version notes
/*

v1.0 January 2024
v2.0 APRIL 2024
Complete rewrite and some bug fixes and general tidying. I am not a professional coder so there are some nasty workarounds in this 
code but it works. Any help refining the code is always appreciated. I learnt a lot writing this and am very happy for better coders
to assist in this or any other indicator's development.
Added Range Bars finally!
Added ability to display two types of clock for BarTypes other than Time,Tick,Volume or Range based
Added ability to run Timer as an elapsed value ie up or down
RE arranged properties to be more logical - and collapsed all the menus so it looks tidier and is easier to navigate
Added Separate  Options reveal for Number vs Percentage Alert Levels.
Removed need to specify BarType - code recognizes BarType automatically.
Added the Key Combo Ctrl + the number 0 - top row of a normal laptop. For people who 
don't have NumPad's - so that they can move the counter / clock around the Panel.
There is now a hack so that your computer clock is always correctly synced (and it's free).
It also has the added benefit of revealing your data lag with NT8. See you tube video for link in the 
menu dialog.
v3 Aug 2024
Added Timer and synchronization to full second
Added text Outline for easier reading
Added serialization for gradient colors - somehow missed that in v1 and v2!!
*/
#endregion

#region IMPORTANT NOTES PLEASE READ  *****  Remove all previous versions otherwise this indicator will produce errors ********

/*
Notes PLEASE READ

Tiny Bug - Key Combo to move indicator around charts will move ALL iterations of this indicator as it's an Enum

Special thanks goes to jeronmymite for the code to get user friendly gradient colours.
This is an updated version of BarTimerFlashFusion which I coded back in NT7 and Patrick_H kindly 
translated when we moved over to NT8 in 2022

ONBARTICK	
Can run on OnBarClose but when adding indicator needs the last bar to close before it plots - 
error message will warn you.
This indicator runs on it's own timer anyway which is once per second. 
For Speed the default is tick by tick but that is unnecessary- EXCEPT FOR TICK OR VOLUME COUNTERS WHERE IT IS COMPULSORY

MOVE CLOCK AROUND THE CHART
Added a couple of different positions to the standard bottom and top.
Allows user to move Clock around screen a little better -  goes clockwise from top left - see Enum
Additionally User can use  Control && NumPad.0  or Control + Zero KeyCode to move clock position.

SET Alert Settings
Alert needs to be set to N seconds before bar close eg on a 2 minute chart you might
set it to say 10 seconds before the end. So the setting would be 110 seconds
Flash Duration is how long you want the Alert or 'Flashing'to go on for so 4 would be for 4 seconds.
NB if you get the timings wrong eg 120s on a 1m chart the flashing and Alert Settings won't work

Volume, Range  and Tick Alert Settings are set numerically. So 40 would be 40 ticks before the end of the tick bar or 40% before the end 
of the tick Bar. That's 40% not 0.4.

ALTERNATIVE CLOCK
Default is New YorkDST - I don't know whether this automatically adapts to winter/summer changes but I assume it does!

If you want another timeZone I have included the option to print out available timezones on your computer ( prints once only)
(PrintTimeZones option)
I have added a spare in the Enum so you can just search for the word spare and follow the existing code lines for syntax
The spelling must be EXACT when you input your desired Alternative Clock

It is quite difficult to work out eg GMT has 3 different versions of itself and getting sensible info
is not easy. If anyone does know their way around this please message me @user Mindset in NT8
SO BE AWARE OF DAYLIGHT SAVINGS CHANGES AND DO NOT RELY ON THIS TO BE "ACCURATE"

GRADIENT COLOURS
I finally found out a way to allow the user to  set gradient colours.Kudos to jeronymite.
Base colour default is white however just to keep things simple.
Feel free to change the base colour as there are some really nice variations out there.

************************  WHEN IT GOES WRONG  ******************************************

1.Check that your colours are not the same as the background!!
2.MOST COMMON ISSUE - Resync your computer time - occasionally microsoft may skip out ( it's not a code issue) 
so do that before you send me a message. If your computer keeps losing time after shutdown, 
this indicates a faulty CMOS battery.
**There is now a free fix for this which automatically updates and syncs your computer clock AND helps sort out a lot 
of lag issues. See youtube clip in Help & Guides.

https://www.youtube.com/watch?v=L29IqEpS74I

3. Check if you're using % values that it's not > 100 ie Flashing/Alerting all the time

4.If it changes summer to winter and vice versa your opening times will change on your exchange- 
this is not an error. I beleive EDT will automatically change but this is untested.
4. If the alloted Alternative Time Zone is not on your computer and you get 
an error messge there is nothing I can do - try amending 
the indicator to a different time zone - requires a little coding.

Finally, any comments,suggestions, bugs, please DM me on NinjaTrader- @Mindset.
Happy Trading.
*/
#endregion


namespace NinjaTrader.NinjaScript.Indicators.Mindset
{
#region Enums & Categories & Reference Types
public enum TheDisplayType
{
	Alternative_Clock,Counter_Only,Chart_Time//,Alt_Time_Only,Chart_Time_Only
}

public enum TheFlashType
{
	None,
	WholePanel,
	ClockFaceOnly,
	TextOnly
}
		public enum MyObjectPosition 
{
			BottomRight,
			BottomLeft,
			TopRight,
			TopLeft,
			Centre,
			TopCentre,
			BottomCentre,
			Close,			
}
			public enum MyTimeZones 
{	
			Beijing,	
			CentralEurope,	
			HongKong,
			London,
			EST,
			EDT,
			NZ,	
			Singapore,
			Sydney,
			Tokyo,			
			Spare
}
#region Categories
	[Gui.CategoryOrder("Expand Hidden Options", 1)]
	[Gui.CategoryOrder("Counter Setup", 2)]
	[Gui.CategoryOrder("Alert Settings", 3)]
	[Gui.CategoryOrder("Flash_Settings", 4)]
	[Gui.CategoryOrder("Audio Settings", 5)]
	[Gui.CategoryOrder("Guides", 6)]
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.Mindset.BarTimerHybridConverter")]
//[CategoryExpanded(typeof(Custom.Resource), "Counter Setup", false)]
//[CategoryExpanded(typeof(Custom.Resource), "Alert Settings", false)]
[CategoryExpanded(typeof(Custom.Resource), "Audio Settings", false)]
[CategoryExpanded(typeof(Custom.Resource), "Flash_Settings", false)]
[CategoryExpanded(typeof(Custom.Resource), "Setup", false)]
[CategoryExpanded(typeof(Custom.Resource), "Visual", false)]
[CategoryExpanded(typeof(Custom.Resource), "Data Series", false)]
#endregion
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]

#endregion

///*****  Remove all previous versions otherwise this indicator may produce errors ********
	public class BarTimerHybrid : Indicator
	
		#region Variables
	{
		private string					  Clock 					= string.Empty;
		private string					  CTS						= String.Empty;
		private string				      counterstr				= string.Empty;
		private string 					altClockZone				= string.Empty;
	
		private double 	AlertProxy 									= 1;
		private double TextSize,rangeCount,tickCount,
						timePercentage,volume,Trigger 				= 0;
		
		private int activeBar, priorRange, enumint, FlashDuration,
					FlashStartTime, barType, lastBar,
					lastVBar, ChkLevel   							= 0;
		
		private 	DateTime			now		 					= Core.Globals.Now;
		private     DateTime 			Alternative_Clock_Now;
		
		private TheDisplayType			myDisplayType				= TheDisplayType.Chart_Time;
		private TheFlashType			myFlashType					= TheFlashType.WholePanel;	
		private MyObjectPosition		myClockPosition				= MyObjectPosition.Centre;
		private MyTimeZones 	        altTimeZone 				= MyTimeZones.EST;
		private SharpDX.Direct2D1.Brush dxBrush 					= null; //used for rendering the counter face
		private SharpDX.Direct2D1.Brush outlineBrush				= null; //used for rendering the counter face
		private static string youtube								= "https://youtu.be/YMwfxeSMx9k";
		private static string youtube1								= "https://www.youtube.com/watch?v=L29IqEpS74I";
		private TimeZoneInfo  			alternateZone;
	
		private TimeSpan 				barTimeLeft;
		private SimpleFont 				textFont;
		
		private Chart  					chartWindow;
				
		private bool			showPercent, showNumLevels,
								showGuide,countDown,connected,hasRealtimeData,
			 					isVolume, isVolumeBase;
		
		private System.Windows.Threading.DispatcherTimer timer;	
		#endregion
		
		#region States

		protected override void OnStateChange()
		{
			#region Defaults
			if (State == State.SetDefaults)
			{
				Description 				= @"Displays Time/Tick/Volume count of bar series.";
				Name        				= "BarTimerHybrid";
				Calculate 					= Calculate.OnEachTick;
				DisplayInDataBox 			= false;
				SoundsOn					= false;	
				Flash_Duration 				= 10;
				FlashStart 					= 10;	
				PaintPriceMarkers			= false;
				PrintTimeZones 				= false;
				IsOverlay 					= true;
				IsChartOnly					= true;
				TextBrush1 					= Brushes.Goldenrod;
                TextBrush1Outline           = Brushes.Black;
                TextBrushForSound			= Brushes.White;
				Gradient_Mixer_Colour 		= Brushes.DarkBlue;
				Gradient_Mixer_Colour1 		= Brushes.Cyan;
				TextFont 					= new SimpleFont("Impact", 30);
				VolumeAlertLevel			= 50;
				TickAlertLevel				= 50;
				RangeAlertLevel				= 2;
				ShowMaxRange				= false;
				CountDown					= true;
				PercentageTrigger			= 10;
				PercentageTriggerTick    	= 20;
				PercentageTriggerVolume    	= 20;
				PercentageTriggerRange    	= 20;
				ShowPercent 				= false;
				ShowNumLevels				= true;
				AlterFontColour				= true;

			}
			#endregion
			
			#region Data Loaded
			else if (State == State.DataLoaded)
			{

				if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Tick 
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Tick)
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Tick)))
				{
					barType = 1;/// tick based

					if (myFlashType != TheFlashType.None)
					{
						ChkLevel = (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi
								|| BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak
								|| BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric)
								? BarsPeriod.BaseBarsPeriodValue : BarsPeriod.Value;	
	
						if (TickAlertLevel > ChkLevel)
						{
							TickAlertLevel = ChkLevel;
							Draw.TextFixed(this,"warning","Alert Exceeds Bar value",TextPosition.Center);
						}	
					}
				}
				
				else if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Volume 
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Volume)
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Volume)))
				{
					barType = 2;  /// Volume based
					{
					
						ChkLevel = (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Volume
									|| BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Volume) 
									? BarsPeriod.BaseBarsPeriodValue : BarsPeriod.Value;	
					
						if (VolumeAlertLevel > ChkLevel)
						{
							VolumeAlertLevel = ChkLevel;
							Draw.TextFixed(this,"warning","Alert Exceeds Volume value",TextPosition.Center);
						}
					}
				}
				else if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Minute 
					|| BarsPeriod.BarsPeriodType == BarsPeriodType.Second 
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Minute) 
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Second)
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Minute) 
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Second)
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Minute)
					|| (BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Second)))
				{
					barType = 3;  /// time based
				//	if (myFlashType != TheFlashType.None)//  too many issues with eg Volumetric bars where BarsPeriod.Value = 2????
					{
//					if(myDisplayType == TheDisplayType.Alternative_Clock//Exclude 2 types of DisplayType from Showing error message
//						|| myDisplayType == TheDisplayType.Chart_Time
//						|| myDisplayType == TheDisplayType.Counter_Only);
					
//					if(BarsPeriod.BarsPeriodType != BarsPeriodType.Volumetric)// strange values 
//					{
//						if( Flash_Duration > BarsPeriod.Value)	
//						Draw.TextFixed(this,"warning","Alert duration set too high"+"\n"+" Max = BarsPeriod.Value which is "+BarsPeriod.Value.ToString(),TextPosition.Center,Brushes.Goldenrod,TextFont,TextBrush1,Brushes.Transparent,100);
//						if( FlashStart > BarsPeriod.Value)
//						Draw.TextFixed(this,"warning","Alert set at too high a value."+"\n"+" Max = BarsPeriod.Value which is "+BarsPeriod.Value.ToString(),TextPosition.Center,Brushes.Goldenrod,TextFont,TextBrush1,Brushes.Transparent,100);
//					}
					}
					ChkLevel = (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi 
								|| BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak
								|| BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric)  
								? BarsPeriod.BaseBarsPeriodValue : BarsPeriod.Value;
					
					if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Minute 
						|| BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Minute
						|| BarsPeriod.BarsPeriodType == BarsPeriodType.LineBreak && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Minute)
						|| BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Minute)
						ChkLevel = ChkLevel * 60;
					
				}
				else if (BarsPeriod.BarsPeriodType == BarsPeriodType.Range)	
				{
					barType = 4;// Range
				}	
				else
					barType = 5;
				
				FlashStartTime = FlashStart;
				FlashDuration = FlashStartTime - Flash_Duration;		
				isVolume 		= BarsPeriod.BarsPeriodType == BarsPeriodType.Volume;
				isVolumeBase 	= (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Volume;

			}//Data loaded
			#endregion
			
			#region Historical
			else if (State == State.Historical)
			{	
				 ChartControl.Dispatcher.InvokeAsync(() =>
                 {
				chartWindow = Window.GetWindow(this.ChartControl.Parent) as Chart;

				   if (chartWindow != null)
                {
					chartWindow.KeyDown += OnKeyDown;
                }
				});
							SetZOrder(-1);// bars on top of flashing. don't move to setDefaults
			}
			else if (State == State.Realtime)
			{
				
				if (timer == null)
				{
					//if (Bars.BarsType.IsTimeBased && Bars.BarsType.IsIntraday)
					{
						lock (Connection.Connections)
						{
							if (Connection.Connections.ToList().FirstOrDefault(c => c.Status == ConnectionStatus.Connected && c.InstrumentTypes.Contains(Instrument.MasterInstrument.InstrumentType)) == null)
								Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.BarTimerDisconnectedError, TextPosition.BottomRight);
							else
							{
//								if (!SessionIterator.IsInSession(Now, false, true))
//									Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.BarTimerSessionTimeError, TextPosition.BottomRight);
//								else
//									Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.BarTimerWaitingOnDataError, TextPosition.BottomRight);
							}
						}
					}
				}
			}
			#endregion
			
			#region Terminated
			else if (State == State.Terminated)
			{
				if (timer == null)
					return;
				timer.IsEnabled = false;
				timer = null;			
			}
			    	if (chartWindow != null)
					chartWindow.KeyDown -= OnKeyDown; 
			#endregion		
		}	
		#endregion
		
		#region OnKeyDown
		public void OnKeyDown(object sender, KeyEventArgs e)
	{
			if(Keyboard.Modifiers == ModifierKeys.Control)
		{
			if (e.Key == Key.D0 || e.Key == Key.NumPad0 )
			{
			///Scroll through Clock Positions Clockwise 
				if(enumint < 7)
				enumint += 1;
				else
				enumint = 0;
				
			switch (enumint)
			{		
						case 0:
						Clock_Position = MyObjectPosition.TopLeft;
						enumint = 0;			
						break;
					
						case 1:	
						Clock_Position = MyObjectPosition.TopCentre;
						enumint = 1;
						break;	
					
						case 2:
						Clock_Position = MyObjectPosition.TopRight;				
						enumint = 2;
						break;
					
						case 3:
						Clock_Position = MyObjectPosition.Close;
						enumint = 3;
						break;
					
						case 4:
						Clock_Position = MyObjectPosition.BottomRight;
						enumint = 4;
						break;
					
						case 5:
						Clock_Position = MyObjectPosition.BottomCentre;	
						enumint = 5;
						break;
					
						case 6:
						Clock_Position = MyObjectPosition.BottomLeft;									
						enumint = 6;
						break;
					
						case 7:
						Clock_Position = MyObjectPosition.Centre;
						enumint = 7;
				break;									
		break;			
			}
		}
	}		
}
	#endregion
	
		#region DisplayTime()
		private bool DisplayTime()
		{
			return ChartControl != null
					&& Bars != null
					&& Bars.Instrument.MarketData != null;
		}
		#endregion
		
		#region OnTimerTick

		private void OnTimerTick(object sender, EventArgs e)
		{
			this.OnBarUpdate();
			ForceRefresh();

			if (DisplayTime())
			{
				if (timer != null && !timer.IsEnabled)
					timer.IsEnabled = true;

				if (connected)
				{				
					//if (SessionIterator.IsInSession(Now, false, true))
					{
						if (hasRealtimeData)
						{
						if(CountDown)
								
							barTimeLeft = Bars.GetTime(Bars.Count - 1).Subtract(Now);
							else
							barTimeLeft = (Bars.GetTime(Bars.Count - 2).Subtract(Now)).Duration();
								 
						
				if (!ShowPercent  && !CountDown)
				{	
					if(BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
					Trigger = barTimeLeft.TotalSeconds - (BarsPeriod.Value*60 - FlashStartTime);
				else
					Trigger = barTimeLeft.TotalSeconds -( BarsPeriod.Value- FlashStartTime);				
				}
				if(ShowPercent && !CountDown)	 							
					Trigger = Math.Round((Bars.PercentComplete*100)-(100-PercentageTrigger),MidpointRounding.AwayFromZero);	
							
				if(!ShowPercent && CountDown)	
					Trigger =  FlashStartTime -  barTimeLeft.TotalSeconds;
				
				if(ShowPercent && CountDown)
					Trigger = Math.Round((Bars.PercentComplete*100)-(100-PercentageTrigger),MidpointRounding.AwayFromZero);	
					
							if(altTimeZone == MyTimeZones.EST)
								 altClockZone = "Eastern Standard Time";
							if(altTimeZone == MyTimeZones.EDT)
								 altClockZone = "US Eastern Standard Time";// allegedly will auto change for Daylight Savings - untested
						    if(altTimeZone == MyTimeZones.Sydney)
								 altClockZone = "AUS Eastern Standard Time";
							if(altTimeZone == MyTimeZones.CentralEurope)
								 altClockZone = "Central Europe Standard Time";
							if(altTimeZone == MyTimeZones.Beijing)
								 altClockZone = "China Standard Time";
 							if(altTimeZone == MyTimeZones.NZ)
								 altClockZone = "New Zealand Standard Time";
							if(altTimeZone == MyTimeZones.Singapore)
								 altClockZone = "Singapore Standard Time";
							if(altTimeZone == MyTimeZones.Tokyo)
								 altClockZone = "Tokyo Standard Time";
							if(altTimeZone == MyTimeZones.London)
								 altClockZone = "GMT Standard Time";
							if(altTimeZone == MyTimeZones.HongKong)
								 altClockZone = "Hong Kong Standard Time";
							
							
///nb dont'forget to change Enum name "spare" to your new zone abbreviation here AND in the Enum itself above		
							if(altTimeZone == MyTimeZones.Spare)
								 altClockZone = "Spare Time";
			alternateZone = TimeZoneInfo.FindSystemTimeZoneById(altClockZone);//"GMT Standard Time");										
			Alternative_Clock_Now = TimeZoneInfo.ConvertTime(Now, TimeZoneInfo.Local,alternateZone);
			timePercentage = CountDown ? (1 - Bars.PercentComplete)*100 : Bars.PercentComplete*100 ;
				
							RemoveDrawObject("NinjaScriptInfo");
						}
						else
							Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.BarTimerWaitingOnDataError, TextPosition.BottomRight);
					}
				}///if connected
				else
				{	
					Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.BarTimerDisconnectedError, TextPosition.BottomRight);

					if (timer != null)
						timer.IsEnabled = false;
				}
					ForceRefresh();		
			}///Display Time		
		}

		#region Session Iterator - removed
//		private SessionIterator SessionIterator
//		{
//			get
//			{
//				if (sessionIterator == null)
//					sessionIterator = new SessionIterator(Bars);
//				return sessionIterator;
//			}
//		}
#endregion
		
		#region DateTime Now
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
		#endregion
		
		#endregion
	
		#region OnBarUpdate
		protected override void OnBarUpdate()
		{					
			if(PrintTimeZones && CurrentBar == 0)
			ShowTimeZones();
	
			if (State == State.Realtime)
			{
				hasRealtimeData = true;
				connected = true;

				string[] newCTS =new String[2]{"[ "+Alternative_Clock_Now.ToString("HH:mm:ss")+" ]","[ "+Now.ToString("HH:mm:ss")+" ]"};//.Empty;
			
					if(CurrentBar != activeBar && SoundsOn)
				{
				PlaySound(UpSoundFile);
					activeBar = CurrentBar;
				}
		#region BarType = Time	
			
			if ( barType == 3 ) //Seconds
			{
				
								#region Display Layouts For Time Based Intervals		
				
				if (myDisplayType == TheDisplayType.Counter_Only)
					{
						{
						if(barTimeLeft.Hours > 0)
						{					
							Clock =  ShowPercent ? 
							timePercentage.ToString("N0")+"%":
							barTimeLeft.Hours.ToString("00") + ":"						
									 + barTimeLeft.Minutes.ToString("00") + ":"
									 + barTimeLeft.Seconds.ToString("00");
							
						}
	 					if(barTimeLeft.Hours < 1 && barTimeLeft.Minutes > 0)
						{
							Clock =  ShowPercent ? 
							timePercentage.ToString("N0")+"%":
							 barTimeLeft.Minutes.ToString("00") + ":" 
									 + barTimeLeft.Seconds.ToString("00");
						}	
						if(barTimeLeft.Minutes < 1)
							Clock =  ShowPercent ? 
							timePercentage.ToString("N0")+"%":	
							   barTimeLeft.Seconds.ToString("00");
		
						}//else
					}///lapsed time only
					
					if (myDisplayType == TheDisplayType.Alternative_Clock)
					{
	
					 	CTS = newCTS[0];// attempt at changing string colours - not quite there yet
						//"[ "+Alternative_Clock_Now.ToString("HH:mm:ss")+" ]";//Added [] to sort out justification issue with Times of  >1 hour,Padding didn't work for some reason
//CTS = CTS[0].Split(new string[] { CTS }, StringSplitOptions.None);
//						string [] strlist  = CTS.Split('[');
//    for (int i = 0; i < CTS.Length; i++)
//    {
//       //TextBrush1.ResetColor();
//       // TextBrush1 = TextBrush1;
//      //  if (i == CTS.Length - 1)
//			foreach(String s in strlist)
//        {
//           TextBrush1 = Brushes.Red;
//            //Write(coloredWord);
//        }
//    }
						
					
						
						if(barTimeLeft.Hours > 0)
								{
							 if(ShowPercent)
							 {
						Clock = CTS+"\n";
								// if(barType == 3)
						 counterstr = (timePercentage.ToString("N0") + "%").ToString();
							//Draw.TextFixed(this,";;DKDK",timePercentage.ToString	("N0")+"%",TextPosition.TopRight);
							 }
						 if(ShowNumLevels)
						 	{	
							Clock  = CTS+"\n";
								if(barType == 3)
								{
							counterstr =  barTimeLeft.Hours.ToString("00") + ":"+
								barTimeLeft.Minutes.ToString("00") +
							(barTimeLeft.Ticks < 0 ?"00:00":  ":" + barTimeLeft.Seconds.ToString("00"));
								}
							}
								}
															
						 if(barTimeLeft.Hours < 1 && barTimeLeft.Minutes > 0)
						 {								 
							 if(ShowPercent)
							 {
						Clock = CTS +"\n";
						 counterstr = (timePercentage.ToString("N0") + "%").ToString();
							 }
						 if(ShowNumLevels)
						 	{
							Clock  = CTS+"\n";
							counterstr =  barTimeLeft.Minutes.ToString("00") +
							(barTimeLeft.Ticks < 0 ?"00:00":  ":" + barTimeLeft.Seconds.ToString("00"));
							}
						 } 	 					 
						if(barTimeLeft.Minutes < 1) 
						  {
							  if(ShowPercent)
							  {
							Clock = CTS+"\n";
								  if(barType == 3)
							 counterstr = (timePercentage.ToString("N0") + "%").ToString();
							  }
								if(ShowNumLevels)
							  {
							Clock = CTS+"\n";
								 // if(barType == 3)
							counterstr = (barTimeLeft.Ticks < 0 ?"00:00": barTimeLeft.Seconds.ToString("00")+"\n");
								 // if(barType == 1)
									 // counterstr =tickCount.ToString();
							  }
						 }	
					}///alternative clock
			
					
				 if (myDisplayType == TheDisplayType.Chart_Time)
					{
						CTS = newCTS[1];
						if(barTimeLeft.Hours > 0)
						{
						 if(ShowPercent)
							 {
						Clock = CTS+"\n";
						 counterstr = (timePercentage.ToString("N0") + "%").ToString();
							 }
						 if(ShowNumLevels)
						 	{	
							Clock  = CTS+"\n";
							counterstr =  barTimeLeft.Hours.ToString("00") + ":"+
								barTimeLeft.Minutes.ToString("00") +
							(barTimeLeft.Ticks < 0 ?"00:00":  ":" + barTimeLeft.Seconds.ToString("00"));	
							}
							
						}
						 if(barTimeLeft.Hours < 1 && barTimeLeft.Minutes > 0)
								{
								 if(ShowPercent)
							 {
						Clock = CTS +"\n";
						 counterstr = (timePercentage.ToString("N0") + "%").ToString();
							 }
						 if(ShowNumLevels)
						 	{
							Clock  = CTS+"\n";
							counterstr =  barTimeLeft.Minutes.ToString("00") +
							(barTimeLeft.Ticks < 0 ?"00:00":  ":" + barTimeLeft.Seconds.ToString("00"));
							}
								}
						if(barTimeLeft.Minutes < 1)	
							
							
							 if(ShowPercent)
							  {
							Clock = CTS+"\n";
							 counterstr = (timePercentage.ToString("N0") + "%").ToString();// too far right need to account for "%" character
							  }
								if(ShowNumLevels)
							  {
							Clock = CTS+"\n";//room for second line
							counterstr = (barTimeLeft.Ticks < 0 ?"00:00": barTimeLeft.Seconds.ToString("00")+"\n");
							  }
								
								
					}///local clock only
					#endregion
	return;
			}
			#endregion
			
			
		#region BarType = Volume
///			============================================== Volume ===================================================	
			if (barType == 2 )///NB Different Calcs to Tick Charts
			{
			
				DisplayTime();
				
				volume = Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency ? Core.Globals.ToCryptocurrencyVolume((long)Volume[0]) : Volume[0];
				double periodValuev = (BarsPeriod.BarsPeriodType == BarsPeriodType.Volume) ? BarsPeriod.Value : BarsPeriod.BaseBarsPeriodValue;

				double volumeCount =  ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric)
					? BarsPeriod.BaseBarsPeriodValue - volume : BarsPeriod.Value - volume) ;    //BarsPeriod.BaseBarsPeriodValue

				string volume1 = (false ? NinjaTrader.Custom.Resource.VolumeCounterVolumeRemaining + volumeCount +  "": volumeCount +  ""); 
			 volumeCount = ShowPercent
				? CountDown
					? (1 - Bars.PercentComplete) * 100
					: Bars.PercentComplete * 100
				: CountDown
					? (isVolumeBase
						? BarsPeriod.BaseBarsPeriodValue
						: BarsPeriod.Value) - volume
					: volume;
				
if(myDisplayType == TheDisplayType.Alternative_Clock)			
					Clock = newCTS[0]+"\n";
			if(myDisplayType == TheDisplayType.Chart_Time)			
					Clock = newCTS[1]+"\n";
			
			counterstr= (isVolume || isVolumeBase)
				? ((CountDown
					?  volumeCount.ToString("N0")
					: volumeCount.ToString("N0")) + (ShowPercent ? "%" : ""))
				: NinjaTrader.Custom.Resource.VolumeCounterBarError;
	
				if (!ShowPercent  && !CountDown)///1
				{	
					Trigger =  volumeCount -(periodValuev-VolumeAlertLevel);
				}
				if(ShowPercent && !CountDown)///2										
				{
					Trigger =  volumeCount -(100-PercentageTriggerVolume );
				}
								
				if(!ShowPercent && CountDown)///3
				{			
					Trigger = VolumeAlertLevel - volumeCount;	
				}	
					if(ShowPercent && CountDown)///4
				{
				Trigger = PercentageTriggerVolume  - volumeCount;	

				}	
		
				
				if (volumeCount <= VolumeAlertLevel )
				{
					if (CurrentBar != lastVBar)  // playsound once per bar
					{
						lastVBar = CurrentBar;
					}
					RemoveDrawObject ("NinjaScriptInfo");
				}
				else
				{
					RemoveDrawObject ("NinjaScriptInfo1");
				}					
			}
			#endregion
			
		#region BarType = Tick	
			
			if (barType == 1)///==========================TICK=============================

			{
				double periodValue = (BarsPeriod.BarsPeriodType == BarsPeriodType.Tick) ? BarsPeriod.Value : BarsPeriod.BaseBarsPeriodValue;
				
				tickCount  = ShowPercent ? CountDown ? (1 - Bars.PercentComplete) : Bars.PercentComplete : 
				CountDown ? periodValue - Bars.TickCount : Bars.TickCount;				

				
				if(ShowPercent && TickAlertLevel > 100)
				Draw.TextFixed(this,"Warning","Tick alert level is > 100%",TextPosition.Center);///2

				if (!ShowPercent && !CountDown)///1
				{	
					Trigger = tickCount - (periodValue-TickAlertLevel);
				}
														
				if(ShowPercent && !CountDown)///2
				{
					Trigger = tickCount*100 - (100-PercentageTriggerTick);
				}
								
				if(!ShowPercent && CountDown)///3
				{	
					Trigger = TickAlertLevel - tickCount;	
				}
				
				if(ShowPercent && CountDown)///4	
				{
					Trigger = (PercentageTriggerTick)-(tickCount*100);
				}
				
					if(myDisplayType == TheDisplayType.Alternative_Clock)			
					Clock =newCTS[0]+"\n";
					if(myDisplayType == TheDisplayType.Chart_Time)			
					Clock =newCTS[1]+"\n";
				
				counterstr = ShowPercent ? tickCount.ToString("P0") : tickCount.ToString();	//don't move
		
			
				if (tickCount <= TickAlertLevel)
				{
					RemoveDrawObject ("NinjaScriptInfo");
				}
				else
				{
					RemoveDrawObject ("NinjaScriptInfo1");
				}	
				//Print(Trigger);			
			}  // tick counter	
	#endregion
			
			
		#region BarType = Range
		
	if( barType == 4)
	{
				{
					double high = High.GetValueAt(Bars.Count - 1 - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 1 : 0));
					double low = Low.GetValueAt(Bars.Count - 1 - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 1 : 0));
					double close = Close.GetValueAt(Bars.Count - 1 - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 1 : 0));
					int trueRange = (int)Math.Round(Math.Max(close - low, high - close) / Bars.Instrument.MasterInstrument.TickSize);
					
				    priorRange = Math.Max(priorRange,trueRange);// regardless of countdown or countup
					if(IsFirstTickOfBar)
						priorRange = 1;
					
					if(ShowMaxRange)
						rangeCount = CountDown ? BarsPeriod.Value - priorRange : priorRange;
						else
						rangeCount = CountDown ? BarsPeriod.Value - trueRange : trueRange;
					
					double rangePercentage = rangeCount / BarsPeriod.Value;
						   rangePercentage = Math.Round(rangePercentage, 2, MidpointRounding.AwayFromZero);
					
					if(myDisplayType == TheDisplayType.Alternative_Clock)			
					Clock =newCTS[0]+"\n";
					if(myDisplayType == TheDisplayType.Chart_Time)			
					Clock =newCTS[1]+"\n";	
					
					counterstr = ShowPercent ? rangePercentage.ToString("P0") : rangeCount.ToString();	
			
					if (!ShowPercent  && !CountDown)///1	
					Trigger = rangeCount -(BarsPeriod.Value -RangeAlertLevel);
				
					if(ShowPercent && !CountDown)///2											
					Trigger = ((rangePercentage*100)-(100-PercentageTriggerRange));
							
					if(!ShowPercent && CountDown)///3
					Trigger = RangeAlertLevel - rangeCount;			
					
					if(ShowPercent && CountDown)///4
					Trigger = ((PercentageTriggerRange- rangePercentage*100) );				
			}
	}
	#endregion
	
		#region BarType = 5
	if( barType >= 5 )///Other bar types that are not time,tick,volume or range based
	{
		if(myDisplayType == TheDisplayType.Chart_Time)
			Clock = Now.ToString("HH:mm:ss");
		else
			Clock = Alternative_Clock_Now.ToString("HH:mm:ss");
	Trigger = -1;// prevent Flashing
	}
	#endregion
			
		}/// real time
	}
		#endregion

		#region  OnConnected Status Update
		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Connected
				&& connectionStatusUpdate.Connection.InstrumentTypes.Contains(Instrument.MasterInstrument.InstrumentType)) ;
			//&& Bars.BarsType.IsTimeBased 
			//&& Bars.BarsType.IsIntraday)
			{
				connected = true;

				if (DisplayTime() && timer == null)
				{
					ChartControl.Dispatcher.InvokeAsync(() =>
					{
						timer = new System.Windows.Threading.DispatcherTimer { Interval = new TimeSpan(0, 0, 1), IsEnabled = false };
						timer.Tick += OnTimerTick;
						timer.Stop();
                        SynchronizeTimerAtFullSecond(timer);


                    });
				}

			}
			//else if (connectionStatusUpdate.PriceStatus == ConnectionStatus.Disconnected)
			//connected = false;
		}
		#endregion
		
		#region Synchronize
		
		protected void SynchronizeTimerAtFullSecond(System.Windows.Threading.DispatcherTimer timer)
        {
            if (timer != null)
            {	
				DispatcherTimer dispatcherTimer;
				Timer startTimer;

                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
                dispatcherTimer.Tick += DispatcherTimer_Tick;

				// Specify the target start time Now +1 s and no milliseconds
				DateTime targetTime = DateTime.Now;
				if (timer.IsEnabled)
	            {
                    targetTime = targetTime.AddSeconds(5);
                }
				else
				{
                    targetTime = targetTime.AddSeconds(1);
                }
				
				targetTime = targetTime.AddMilliseconds(-targetTime.Millisecond);

                // Calculate the interval to wait until the target time
                TimeSpan waitTime = targetTime - DateTime.Now;

                if (waitTime > TimeSpan.Zero)
                {
                    // Set up the timer to start the DispatcherTimer at the specified time
                    startTimer = new Timer(waitTime.TotalMilliseconds);
                    startTimer.Elapsed += StartTimer_Elapsed;
                    startTimer.AutoReset = false; // Only fire the event once
                    startTimer.Start();
                }
                else
                {
                  //  Console.WriteLine("Target time is in the past. Please specify a future time.");
                }

                void StartTimer_Elapsed(object sender, ElapsedEventArgs e)
                {
                    // Start the DispatcherTimer
					timer.IsEnabled = true;
					timer.Start();
                    dispatcherTimer.Start();
                    Console.WriteLine("DispatcherTimer started at: " + DateTime.Now);
                };
				void DispatcherTimer_Tick(object sender, EventArgs e)
				{
					//Console.WriteLine("DispatcherTimer tick at: " + DateTime.Now);
				};
            }

        }
#endregion
		
        #region OnRenderTargetChanged

        public override void OnRenderTargetChanged()
		{
			// if dxBrush exists on first render target change, dispose of it
			if (dxBrush != null)
			{
				dxBrush.Dispose();
			}
			// recalculate dxBrush from user defined brush when RenderTarget is recreated
			if (RenderTarget != null)
			try
				{
						dxBrush = TextBrush1.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
			
			// if outlineBrush exists on first render target change, dispose of it
			if (outlineBrush != null)
			{
				outlineBrush.Dispose();
			}
			// recalculate outlineBrush from user defined brush when RenderTarget is recreated
			if (RenderTarget != null)
				try
				{
					outlineBrush = TextBrush1Outline.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
        }
        #endregion

        #region OnRender
        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			#region Vars
			base.OnRender(chartControl, chartScale);
			
			// this.OnTimerTick(this, null); //ensure that the calculation is running
					
			SharpDX.Direct2D1.Brush dxBrush = TextBrush1.ToDxBrush(RenderTarget);
            SharpDX.Direct2D1.Brush outlineBrush = TextBrush1Outline.ToDxBrush(RenderTarget);
            if (SoundsOn && AlterFontColour)
				dxBrush =   TextBrushForSound.ToDxBrush(RenderTarget);
			SharpDX.Color sharpColor = new SharpDX.Color(Gradient_Mixer_Colour.Color.R,Gradient_Mixer_Colour.Color.G,Gradient_Mixer_Colour.Color.B);//gradient colour​
			SharpDX.Color sharpColorbase = new SharpDX.Color(Gradient_Mixer_Colour1.Color.R,Gradient_Mixer_Colour1.Color.G,Gradient_Mixer_Colour1.Color.B);//gradient colour​
			NinjaTrader.Gui.Tools.SimpleFont simpleFont 				= TextFont;

			SharpDX.DirectWrite.TextFormat textFormat1 					= simpleFont.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextFormat textFormat2 					= simpleFont.ToDirectWriteTextFormat();

			SharpDX.DirectWrite.TextLayout textLayout1 	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
			Clock, textFormat1, ChartPanel.X + ChartPanel.W, textFormat1.FontSize);
 			textFormat1.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;


			SharpDX.DirectWrite.TextLayout textLayout2 	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
			counterstr, textFormat2, textLayout1.Metrics.Width ,textFormat2.FontSize);

//			SharpDX.DirectWrite.TextLayout textLayout3 	= new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
//			nontimeLeft, textFormat1, ChartPanel.X + ChartPanel.W, textFormat1.FontSize);
// 			textFormat1.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
		
			//Initialize two Text Points for our two lines of text (these are just dummy co-ordinates and are not used)
			SharpDX.Vector2 textPoint2 	= new SharpDX.Vector2(ChartPanel.X, ChartPanel.Y);
			SharpDX.Vector2 textPoint1 	= new SharpDX.Vector2(ChartPanel.X, ChartPanel.Y);
				{
					#endregion
		
					
			#region Rotate Clock Positions					
			switch(myClockPosition)
			{			
			case MyObjectPosition.TopLeft:
				textPoint1 = new SharpDX.Vector2(ChartPanel.X + 10, ChartPanel.Y);
				textPoint2 	= new SharpDX.Vector2(textPoint1.X+textLayout1.Metrics.Width/2- textLayout2.Metrics.Width/2, ChartPanel.Y + textLayout1.Metrics.Height/2);

 				enumint = 0;
				break;
					
				case MyObjectPosition.TopCentre:
				textPoint1 = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W/2 - textLayout1.Metrics.Width/2, ChartPanel.Y);
				textPoint2 	= new SharpDX.Vector2(textPoint1.X+textLayout1.Metrics.Width/2- textLayout2.Metrics.Width/2, ChartPanel.Y + textLayout1.Metrics.Height/2);

					enumint = 1;
				break;		
				
				case MyObjectPosition.TopRight:
					
				textPoint1 = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W - textLayout1.Metrics.Width, ChartPanel.Y );
				textPoint2 	= new SharpDX.Vector2(textPoint1.X+textLayout1.Metrics.Width/2- textLayout2.Metrics.Width/2, ChartPanel.Y+ textLayout1.Metrics.Height/2);
					//textPoint2 	= new SharpDX.Vector2(textPoint1.X+textLayout1.Metrics.Width/2- textLayout2.Metrics.Width/2, ChartPanel.Y+ textLayout1.Metrics.Height/2);

				enumint = 2;
				break;	
					
				case MyObjectPosition.Close:
				if (ChartBars != null)
				{
				double close_px = Close.GetValueAt(ChartBars.ToIndex-1);
				float y_Close = chartScale.GetYByValue(close_px)- textLayout1.Metrics.Height;
				float y_CloseClock = chartScale.GetYByValue(close_px)-textLayout1.Metrics.Height/2;

		        float xpos = chartControl.GetXByBarIndex(ChartBars,ChartBars.ToIndex)+(ChartPanel.W - chartControl.GetXByBarIndex(ChartBars,ChartBars.ToIndex)-textLayout1.Metrics.Width)-5;
			
				textPoint1 	= new SharpDX.Vector2(xpos,y_Close);
				textPoint2 	= new SharpDX.Vector2(textPoint1.X+textLayout1.Metrics.Width/2- textLayout2.Metrics.Width/2, y_Close + textLayout1.Metrics.Height/2);

	
				enumint = 3;
				}
				break;
				
				case MyObjectPosition.BottomRight:
				textPoint1 	= new SharpDX.Vector2(ChartPanel.W- textLayout1.Metrics.Width, ChartPanel.Y + ChartPanel.H -textLayout1.Metrics.Height);
					textPoint2 	= new SharpDX.Vector2(ChartPanel.W- textLayout1.Metrics.Width/2-textLayout2.Metrics.Width/2, ChartPanel.Y + ChartPanel.H -textLayout1.Metrics.Height/2);
		
					enumint = 4;
				break;
					
				case MyObjectPosition.BottomCentre:
					textPoint1 	= new SharpDX.Vector2(ChartPanel.X +ChartPanel.W/2-textLayout1.Metrics.Width/2,
					ChartPanel.Y + ChartPanel.H -textLayout1.Metrics.Height);	
					textPoint2 	= new SharpDX.Vector2(textPoint1.X+
					textLayout1.Metrics.Width/2-textLayout2.Metrics.Width/2,
					ChartPanel.Y + ChartPanel.H - textLayout1.Metrics.Height/2);			
				enumint = 5;
				break;
					
				case MyObjectPosition.BottomLeft:
			 	textPoint1 	= new SharpDX.Vector2(ChartPanel.X+5 , ChartPanel.Y + ChartPanel.H -textLayout1.Metrics.Height);
			 	textPoint2 	= new SharpDX.Vector2(textPoint1.X+textLayout1.Metrics.Width/2- textLayout2.Metrics.Width/2, ChartPanel.Y + ChartPanel.H -textLayout1.Metrics.Height/2);
					
					enumint = 6;
				break;
		
				case MyObjectPosition.Centre:
				textPoint1  = new SharpDX.Vector2(ChartPanel.X + ChartPanel.W/2-textLayout1.Metrics.Width/2,
					ChartPanel.Y + ChartPanel.H/2 -(textLayout1.Metrics.Height));
 				textPoint2 	= new SharpDX.Vector2(ChartPanel.X + ChartPanel.W/2-textLayout2.Metrics.Width/2,
					ChartPanel.Y + ChartPanel.H/2 -(textLayout1.Metrics.Height/2));

					enumint = 7;
				break;							
				break;			
			}
#endregion

		
			#region Gradients
			SharpDX.Vector2 startPoint 									= new SharpDX.Vector2(ChartPanel.X, ChartPanel.Y); 
			SharpDX.Vector2 endPoint 									= new SharpDX.Vector2(ChartPanel.X + ChartPanel.W, ChartPanel.Y + ChartPanel.H);
			SharpDX.Direct2D1.LinearGradientBrush linearGradientBrush 	= new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, new SharpDX.Direct2D1.LinearGradientBrushProperties()
			{
				StartPoint = new SharpDX.Vector2(0, startPoint.Y),
				EndPoint = new SharpDX.Vector2(0, endPoint.Y),		
			},
			
			new SharpDX.Direct2D1.GradientStopCollection(RenderTarget, new SharpDX.Direct2D1.GradientStop[]
			{
				new	SharpDX.Direct2D1. GradientStop()
				{
					/// blue/cyan is nice, green yellow is sunset
					Color =  sharpColor,
					Position = 0,
				},
				new SharpDX.Direct2D1. GradientStop()
				{
					Color =  sharpColorbase,
					Position = 1,
				}
			}));
			if(myFlashType != TheFlashType.WholePanel)// text disappears under Whole Panel flashing so render it elsewhere
			{
			textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;// moves text to far right of panel
            // Draw text outline
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx != 0 || dy != 0) // Skip the center point
                    {
                        var outlinePosition = new SharpDX.Vector2(textPoint1.X + dx, textPoint1.Y + dy);
                        RenderTarget.DrawTextLayout(outlinePosition, textLayout1, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                        outlinePosition = new SharpDX.Vector2(textPoint2.X + dx, textPoint2.Y + dy);
						RenderTarget.DrawTextLayout(outlinePosition, textLayout2, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                    }
                }
            }
            // Draw the main text
            RenderTarget.DrawTextLayout(textPoint1, textLayout1, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);	
			RenderTarget.DrawTextLayout(textPoint2, textLayout2, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			}		
			#endregion

			
			#region FlashTypes
			if(barType == 3)//Time
			{				
			if(myFlashType != TheFlashType.None)
			{
				if(Trigger >= 0)
				{
				if((int)barTimeLeft.TotalSeconds % 2 < 1 )///alternate colors every other second
				{					
					if(myFlashType == TheFlashType.WholePanel)
					{						
						SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint.X, startPoint.Y, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
						RenderTarget.FillRectangle(rect, linearGradientBrush);
					}
					else if(myFlashType == TheFlashType.ClockFaceOnly)
					{						
						SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.X + 5, ChartPanel.Y + (ChartPanel.H - textLayout1.Metrics.Height));
						SharpDX.RectangleF rect = new SharpDX.RectangleF(textPoint1.X, textPoint1.Y, textLayout1.Metrics.Width, textLayout1.Metrics.Height);
						
						RenderTarget.FillRectangle(rect, linearGradientBrush);
                            // Draw text outline
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                for (int dy = -1; dy <= 1; dy++)
                                {
                                    if (dx != 0 || dy != 0) // Skip the center point
                                    {
                                        var outlinePosition = new SharpDX.Vector2(textPoint1.X + dx, textPoint1.Y + dy);
                                        RenderTarget.DrawTextLayout(outlinePosition, textLayout1, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                        outlinePosition = new SharpDX.Vector2(textPoint2.X + dx, textPoint2.Y + dy);
                                        RenderTarget.DrawTextLayout(outlinePosition, textLayout2, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                    }
                                }
                            }
						// Draw the main text
                        RenderTarget.DrawTextLayout(textPoint1, textLayout1, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						RenderTarget.DrawTextLayout(textPoint2, textLayout2, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

					}
					else if(myFlashType == TheFlashType.TextOnly)
					{
                        // Draw text outline
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx != 0 || dy != 0) // Skip the center point
                                {
                                    var outlinePosition = new SharpDX.Vector2(textPoint1.X + dx, textPoint1.Y + dy);
                                    RenderTarget.DrawTextLayout(outlinePosition, textLayout1, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                    outlinePosition = new SharpDX.Vector2(textPoint2.X + dx, textPoint2.Y + dy);
                                    RenderTarget.DrawTextLayout(outlinePosition, textLayout2, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                }
                            }
                        }
						// Draw the main text
                        RenderTarget.DrawTextLayout(textPoint1, textLayout1, linearGradientBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						RenderTarget.DrawTextLayout(textPoint2, textLayout2, linearGradientBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
	
					}
				}
			}
			}
			}//bartype == 3
			else/// ========== Tick,Range and Volume based Bars ===============
				if(myFlashType != TheFlashType.None)
				{
						if(Trigger >= 0)
					{
					
				if(Now.Second % 2 < 1 )///alternate colors every other second but using Now instead of bartimeleft
					
				{					
					if(myFlashType == TheFlashType.WholePanel)
					{						
						SharpDX.RectangleF rect = new SharpDX.RectangleF(startPoint.X, startPoint.Y, endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);
						RenderTarget.FillRectangle(rect, linearGradientBrush);
					}
					else if(myFlashType == TheFlashType.ClockFaceOnly)
					{						
						SharpDX.Vector2 lowerTextPoint = new SharpDX.Vector2(ChartPanel.X + 5, ChartPanel.Y + (ChartPanel.H - textLayout1.Metrics.Height));
						SharpDX.RectangleF rect = new SharpDX.RectangleF(textPoint1.X, textPoint1.Y, textLayout1.Metrics.Width, textLayout1.Metrics.Height);
						
						RenderTarget.FillRectangle(rect, linearGradientBrush);
                        // Draw text outline
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx != 0 || dy != 0) // Skip the center point
                                {
                                    var outlinePosition = new SharpDX.Vector2(textPoint1.X + dx, textPoint1.Y + dy);
                                    RenderTarget.DrawTextLayout(outlinePosition, textLayout1, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                    outlinePosition = new SharpDX.Vector2(textPoint2.X + dx, textPoint2.Y + dy);
                                    RenderTarget.DrawTextLayout(outlinePosition, textLayout2, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                }
                            }
                        }
						// Draw the main text
                        RenderTarget.DrawTextLayout(textPoint1, textLayout1, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						RenderTarget.DrawTextLayout(textPoint2, textLayout2, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
	
					}
					else if(myFlashType == TheFlashType.TextOnly)
					{
                        // Draw text outline
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx != 0 || dy != 0) // Skip the center point
                                {
                                    var outlinePosition = new SharpDX.Vector2(textPoint1.X + dx, textPoint1.Y + dy);
                                    RenderTarget.DrawTextLayout(outlinePosition, textLayout1, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                    outlinePosition = new SharpDX.Vector2(textPoint2.X + dx, textPoint2.Y + dy);
                                    RenderTarget.DrawTextLayout(outlinePosition, textLayout2, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                                }
                            }
                        }
                        // Draw the main text
                        RenderTarget.DrawTextLayout(textPoint1, textLayout1, linearGradientBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						RenderTarget.DrawTextLayout(textPoint2, textLayout2, linearGradientBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);

					}
				}//}//alternate colours			
			}//Flash Type != None
		}//end Tick Chart Flashing code
				/// Counter/Timer text output here so it renders 'on top' of any flashing
			if(myFlashType == TheFlashType.WholePanel)
				{
			textFormat2.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;// moves text to far right of panel
            // Draw text outline
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx != 0 || dy != 0) // Skip the center point
                    {
                        var outlinePosition = new SharpDX.Vector2(textPoint1.X + dx, textPoint1.Y + dy);
                        RenderTarget.DrawTextLayout(outlinePosition, textLayout1, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                        outlinePosition = new SharpDX.Vector2(textPoint2.X + dx, textPoint2.Y + dy);
                        RenderTarget.DrawTextLayout(outlinePosition, textLayout2, outlineBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
                    }
                }
            }
            // Draw the main text
            RenderTarget.DrawTextLayout(textPoint1, textLayout1, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);	
			RenderTarget.DrawTextLayout(textPoint2, textLayout2, dxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
				}	
			#endregion
		
			linearGradientBrush.Dispose();
			dxBrush.Dispose();
			outlineBrush.Dispose();
			textFormat1.Dispose();
			textLayout1.Dispose();
			textLayout2.Dispose();
			//textLayout3.Dispose();
		}
	}
		#endregion
					
		#region ShowTimeZones - only use if you need to find another alternative time zone
		private void ShowTimeZones()
{
	   	ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones(); 
      		foreach (TimeZoneInfo timeZone in timeZones)
   		 Print(timeZone.StandardName+ Environment.NewLine);		
}	
#endregion
	
		#region Debugging Text
				
//		Draw.TextFixed(this,"12ëueie","Alert Proxy =  "
//		+AlertProxy .ToString ("N2") +"\nTrigger = "
//		+Trigger.ToString("N2")+"\n AL "
//		+TickAlertLevel.ToString()
//		+"\nTickCount "
//		+tickCount.ToString("N0"),
		
//		TextPosition.TopRight);
				
				
				
//				Draw.TextFixed(this,"ëoepeo1",
//			"\n Percent "
//			+ ShowPercent.ToString()
//			+"\n Countdown "
//			+ CountDown.ToString()
//					+" Trigger" + AlertProxy.ToString()
//			+"\n 1 = "+ Condition1.ToString()+
//			 "\n 2 = "+ Condition2.ToString()+
//			 "\n 3 = "+ Condition3.ToString()+
//			 "\n 4 = "+ Condition4.ToString(),
//			 TextPosition.BottomRight);

	#endregion

		#region Properties

			#region Guides
					
					[NinjaScriptProperty]
					[Display(Name="Print available time zones", Description = "Make sure NinjaTrader Output is open",Order=5, GroupName="Guides")]
					public bool PrintTimeZones
					{ get; set; }
					
					[NinjaScriptProperty]
					[Display(Name="Indicator Guide", Order=1,Description="Copy and paste into browser", GroupName="Guides")]
					public  string Youtube
					{
					 	get{return youtube;} 
						set{youtube = (value);} 
					}
					
					[NinjaScriptProperty]
					[Display(Name="Sync Clock"+"\nData Lag", Order=1,Description="Copy and paste into browser", GroupName="Guides")]
					public  string Youtube1
					{
					 	get{return youtube1;} 
						set{youtube1 = (value);} 
					}
					
					#endregion
				
			#region Expand Hidden Options

				[RefreshProperties(RefreshProperties.All)]
				[NinjaScriptProperty]
				[Display(ResourceType = typeof (Custom.Resource), Name = "Show Guides and Aids?",
				Order = 5, GroupName = "Expand Hidden Options")]
				public bool ShowGuide
				{ get; set; }		
			#endregion		
			
			#region Counter Setup
					
			[RefreshProperties(RefreshProperties.All)]		
			[NinjaScriptProperty]
			[Display(ResourceType = typeof (Custom.Resource), Name = "Count Down", 
			Order = 1,Description="", GroupName = "Counter Setup")]
			public bool CountDown
			{ get; set; }		
					
			
			[Display(Name="Display Type", GroupName="Counter Setup",Description="", Order=2)]		
			public TheDisplayType MyDisplayType
			{
				get { return myDisplayType; }
				set { myDisplayType = value; }
			}	
			
			[Display(Name="Alt Time Zones", Description="", GroupName="Counter Setup", Order=3)]		
			public MyTimeZones AltTimeZone
			{
				get { return altTimeZone; }
				set { altTimeZone = value; }
			}
			
			[Display(GroupName = "Counter Setup", Description="Counter Font.",Order = 6)]
			public SimpleFont TextFont
			{
				get { return textFont; }
				set { textFont = value; }
			}
			
			[Display(Name = "Counter Position", GroupName = "Counter Setup", 
			Description="Position can be moved around screen by Ctrl + Zero",Order = 4)]
			public MyObjectPosition Clock_Position
			{
				get { return myClockPosition; }
			    set { myClockPosition = value; }
			}	
					
			[XmlIgnore]
			[Display(Name = "Counter Text Colour", GroupName = "Counter Setup", Order = 5)]	
			public System.Windows.Media.Brush TextBrush1
			{ get; set; } 
			
			[Browsable(false)]
			public string TextBrush1Serialize 
			{
			  get { return Serialize.BrushToString(TextBrush1); }
			  set { TextBrush1 = Serialize.StringToBrush(value); } 
			}

			//TextBrush1Outline
			[XmlIgnore]
			[Display(Name = "ClockOutline Text Colour", GroupName = "Counter Setup", Order = 6)]
			public System.Windows.Media.Brush TextBrush1Outline
			{ get; set; }

			[Browsable(false)]
			public string TextBrush1OutlineSerialize
			{
				get { return Serialize.BrushToString(TextBrush1Outline); }
				set { TextBrush1Outline = Serialize.StringToBrush(value); }
			}

        //				[Display(Name="Hide Display Name", Description="Blank on Chart Panel.", Order=1, GroupName="Counter Setup")]
        //					public bool ShowDispName
        //					{ get; set; }	

        #endregion

     	    #region Flashing

        [Display(Name="Flash Type", Description="", Order=3,GroupName = "Flash_Settings")]
			public TheFlashType MyFlashType
			{
				get { return myFlashType; }
				set { myFlashType = value; }
			}
					
			[XmlIgnore]
			[Display(GroupName = "Flash_Settings", Order = 4, Name = "Flash Gradient"+"\n"+"Mix base",
			Description = "Mix Colour for Gradient.")]
			public SolidColorBrush Gradient_Mixer_Colour1 { get; set; } 
			
			[Browsable(false)]
			public string gradient_Mixer_Colour1_Serialize
        {
				get { return Serialize.BrushToString(Gradient_Mixer_Colour1); }
				set {if (Gradient_Mixer_Colour1 != null)
							{
								if (Gradient_Mixer_Colour1.IsFrozen)
									Gradient_Mixer_Colour1 = Gradient_Mixer_Colour1.Clone();
									Gradient_Mixer_Colour1.Freeze();
							}
					Gradient_Mixer_Colour1 = (SolidColorBrush)Serialize.StringToBrush(value);
				  }
			}
			
			[XmlIgnore]
		[Display(GroupName = "Flash_Settings", Order = 5, Name = "Flash Gradient" + "\n" + "Mix",
			Description = "Mix Colour for Gradient.")]
			public SolidColorBrush Gradient_Mixer_Colour {get; set; }
			
			[Browsable(false)]
			public string gradient_Mixer_Colour_Serialize
			{
				get { return Serialize.BrushToString(Gradient_Mixer_Colour); }
				set {if (Gradient_Mixer_Colour != null)
							{
								if (Gradient_Mixer_Colour.IsFrozen)
									Gradient_Mixer_Colour = Gradient_Mixer_Colour.Clone();
									Gradient_Mixer_Colour.Freeze();
							}
					Gradient_Mixer_Colour = (SolidColorBrush)Serialize.StringToBrush(value);
				}
			}
			
			#endregion	
						
			#region Sounds
				
					[Display(Name="Sound Alert", Description="Play sounds on first alert.", Order=1, GroupName="Audio Settings")]
					public bool SoundsOn
					{ get; set; }	
					
					[Display(Name="Alter Text Colour", Description="Change Text Colour.", Order=3, GroupName="Audio Settings")]
					public bool AlterFontColour
					{ get; set; }	
							
					
					[Display(Name="Alert Sound file", Description="Enter sound file path/name. Alert2 is default sound", Order=2, GroupName="Audio Settings")]
					[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
					public string UpSoundFile
					{ get; set; }
					
					[XmlIgnore]
					[Display(Name = "Alternative Text Colour", GroupName = "Audio Settings", Order = 4)]	
					public System.Windows.Media.Brush TextBrushForSound
					{ get; set; } 
					
					[Browsable(false)]
					public string TextBrushForSoundSerialize 
					{
					  get { return Serialize.BrushToString(TextBrushForSound); }
					  set { TextBrushForSound = Serialize.StringToBrush(value); } 
					}	
					
					#endregion
		
			#region Alert levels

		
			[Range (1, int.MaxValue)]
			[Display(ResourceType = typeof (Custom.Resource), Name = "Range Num Level", 
			Description = "10 = last 10% or last 10 as volume number", Order = 9, GroupName = "Alert Settings")]
			public int RangeAlertLevel
			{ get; set; }
			
			//[RefreshProperties(RefreshProperties.All)]
			[NinjaScriptProperty]
			[Display(ResourceType = typeof (Custom.Resource), Name = "Show MAX Range"+"\n"+"(Range Bars only)",
			Order = 12, GroupName = "Alert Settings")]
			public bool ShowMaxRange
			{ get; set; }
			
			
			[Range(1, int.MaxValue), NinjaScriptProperty]
			[Display(Name = "Range % Level",GroupName = "Alert Settings",
			Description=" Alert start in percentage from END of bar eg 10 for 10% to commencement of next bar.",Order=11)]
			public int PercentageTriggerRange
			{ get; set; }
			
			[Range (1, int.MaxValue)]
			[Display(ResourceType = typeof (Custom.Resource), Name = "Volume Num Level",
			Description = "10 = last 10% or last 10 as volume number", Order = 7, GroupName = "Alert Settings")]
			public int VolumeAlertLevel
			{ get; set; }
			
			[Range(1, int.MaxValue), NinjaScriptProperty]
			[Display(Name = "Volume % Level",GroupName = "Alert Settings",
			Description=" Alert start in percentage from END of bar eg 10 for 10% to commencement of next bar.",Order=8)]
			public int PercentageTriggerVolume
			{ get; set; }
	
			[Range (1, double.MaxValue)]
			[Display(ResourceType = typeof (Custom.Resource), Name = "Tick Num Level", Order = 5,
			Description="10 = last 10% or last 10 Ticks as a number",GroupName = "Alert Settings")]
			public double TickAlertLevel
			{ get; set; }
			
			[Range(1, int.MaxValue), NinjaScriptProperty]
			[Display(Name = "Tick % Level",GroupName = "Alert Settings",
			Description=" Alert start in percentage from END of bar eg 10 for 10% to commencement of next bar.",Order=6)]
			public int PercentageTriggerTick
			{ get; set; }
			
			[Range(1, int.MaxValue), NinjaScriptProperty]
			[Display(Name = "Time(Seconds)",GroupName = "Alert Settings",
			Description=" Alert start in seconds from END of bar eg 10 for 10s to commencement of next bar.",Order=4)]
			public int FlashStart
			{ get; set; }
			
			[Range(1, int.MaxValue), NinjaScriptProperty]
			[Display(Name = "Time % Level",GroupName = "Alert Settings",
			Description=" Alert start in percentage from END of bar eg 10 for 10% to commencement of next bar.",Order=5)]
			public int PercentageTrigger
			{ get; set; }
	
			[Range(0, int.MaxValue), NinjaScriptProperty]
			[Display(Name ="Time Duration"+"\n"+"(Min 3s)",
			Description="Flash Time Duration",Order=3,GroupName = "Alert Settings")]
			public int Flash_Duration
			{ get; set; }
			
			[RefreshProperties(RefreshProperties.All)]
			[NinjaScriptProperty]
			[Display(ResourceType = typeof (Custom.Resource), Name = "Use Percentages", Order = 2, GroupName = "Alert Settings")]
			public bool ShowPercent
			{
			get { return showPercent;} 
				set{ 
					showPercent = true;
					showNumLevels = false;
				if(!value)
				{
					showNumLevels = true;
					showPercent = false;
				}
				}
			}
			
			[RefreshProperties(RefreshProperties.All)]
			[NinjaScriptProperty]
			[Display(ResourceType = typeof (Custom.Resource), Name = "Use Number Levels", Order = 1, GroupName = "Alert Settings")]
			public bool ShowNumLevels
			{
			get { return showNumLevels;} 
				set{ 
					showNumLevels = true;
					showPercent = false;
				if(!value)
				{
					showPercent  = true;
					showNumLevels = false;
				}
				}
			}
			

		
		#endregion

#endregion
	}
	
	//	code derived from the SampleIndicatorTypeConverter indicator
	//	https://ninjatrader.com/support/forum/showthread.php?t=97919
	#region TypeConverter to hide properties in the PropertyGrid
	public class BarTimerHybridConverter : IndicatorBaseConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
        {
            BarTimerHybrid indicator = component as BarTimerHybrid;

            PropertyDescriptorCollection propertyDescriptorCollection = 
			base.GetPropertiesSupported(context) ? base.GetProperties(context, component, attrs): 
				TypeDescriptor.GetProperties(component, attrs);

            if (indicator == null || propertyDescriptorCollection == null)
                return propertyDescriptorCollection;


			#region add/remove property based on other property's value
			
			#region Percentage Atrributes

			PropertyDescriptor showpercent 				= propertyDescriptorCollection["ShowPercent"];
			PropertyDescriptor percentagetrigger		= propertyDescriptorCollection["PercentageTrigger"];
			PropertyDescriptor percentagetriggertick  	= propertyDescriptorCollection["PercentageTriggerTick"];
			PropertyDescriptor percentagetriggervolume 	= propertyDescriptorCollection["PercentageTriggerVolume"];
			PropertyDescriptor percentagetriggerrange 	= propertyDescriptorCollection["PercentageTriggerRange"];

			//propertyDescriptorCollection.Remove(shownumlevels);

			propertyDescriptorCollection.Remove(percentagetrigger);
			propertyDescriptorCollection.Remove(percentagetriggertick);
            propertyDescriptorCollection.Remove(percentagetriggervolume);
		    propertyDescriptorCollection.Remove(percentagetriggerrange);
			
			if (indicator.ShowPercent)
            {
				propertyDescriptorCollection.Add(percentagetrigger);
				propertyDescriptorCollection.Add(percentagetriggertick);
				propertyDescriptorCollection.Add(percentagetriggervolume);
				propertyDescriptorCollection.Add(percentagetriggerrange);
					propertyDescriptorCollection.Add(showpercent);

			}	
			#endregion
			
			#region Num level Atrributes
			 PropertyDescriptor shownumlevels 		= propertyDescriptorCollection["ShowNumLevels"];

		 PropertyDescriptor flashstart			= propertyDescriptorCollection["FlashStart"];
		 PropertyDescriptor flash_duration		= propertyDescriptorCollection["Flash_Duration"];
		 PropertyDescriptor tickalertlevel  	= propertyDescriptorCollection["TickAlertLevel"];
		 PropertyDescriptor volumealertlevel 	= propertyDescriptorCollection["VolumeAlertLevel"];
		 PropertyDescriptor rangealertlevel 	= propertyDescriptorCollection["RangeAlertLevel"];
			
			propertyDescriptorCollection.Remove(flashstart);
			propertyDescriptorCollection.Remove(tickalertlevel);
			propertyDescriptorCollection.Remove(volumealertlevel);
			propertyDescriptorCollection.Remove(rangealertlevel);
			propertyDescriptorCollection.Remove(showpercent);
					propertyDescriptorCollection.Remove(shownumlevels);
	

					if (indicator.ShowNumLevels)
	            {
	                propertyDescriptorCollection.Add(flashstart);
	                propertyDescriptorCollection.Add(tickalertlevel);
					propertyDescriptorCollection.Add(volumealertlevel);
					propertyDescriptorCollection.Add(rangealertlevel);
					propertyDescriptorCollection.Add(shownumlevels);
	
	            }
				#endregion
			
			#region ShowGuide Attributes
			PropertyDescriptor showguide 		= propertyDescriptorCollection["ShowGuide"];	
			PropertyDescriptor printtimezones 	= propertyDescriptorCollection["PrintTimeZones"];
            PropertyDescriptor youtube 			= propertyDescriptorCollection["Youtube"];
		    PropertyDescriptor youtube1 		= propertyDescriptorCollection["Youtube1"];

			propertyDescriptorCollection.Remove(printtimezones);
			propertyDescriptorCollection.Remove(youtube);
		    propertyDescriptorCollection.Remove(youtube1);
			
				if (indicator.ShowGuide)
            {
               propertyDescriptorCollection.Add(printtimezones);
               propertyDescriptorCollection.Add(youtube);
			   propertyDescriptorCollection.Add(youtube1);

            }
				#endregion
						
			#region Show Base Attributes	
//			PropertyDescriptor showbasicattributes 		= propertyDescriptorCollection["ShowBasicAttributes"];
//			PropertyDescriptor paintpricemarkers 		= propertyDescriptorCollection["PaintPriceMarkers"];
//			PropertyDescriptor displayindatabox 		= propertyDescriptorCollection["DisplayInDataBox"];
//			PropertyDescriptor displacement 	= propertyDescriptorCollection["Displacement"];
//			PropertyDescriptor isautoscale 	= propertyDescriptorCollection["IsAutoScale"];
//			PropertyDescriptor calculate 	= propertyDescriptorCollection["Calculate"];
//			PropertyDescriptor scalejustification 	= propertyDescriptorCollection["ScaleJustification"];
//			PropertyDescriptor panel 	= propertyDescriptorCollection["Panel"];
//			PropertyDescriptor isvisible 	= propertyDescriptorCollection["IsVisible"];
//			PropertyDescriptor pdex_inputui 	= propertyDescriptorCollection["PDEX_InputUI"];
//			PropertyDescriptor maximumbarslookback	= propertyDescriptorCollection["MaximumBarsLookBack"];
//			PropertyDescriptor label	= propertyDescriptorCollection["Name"];
			
//			propertyDescriptorCollection.Remove(paintpricemarkers);
//			propertyDescriptorCollection.Remove(panel);
//			propertyDescriptorCollection.Remove(displayindatabox);
//			propertyDescriptorCollection.Remove(displacement);
//			propertyDescriptorCollection.Remove(isautoscale);
//			propertyDescriptorCollection.Remove(calculate);
//			propertyDescriptorCollection.Remove(scalejustification);
//			propertyDescriptorCollection.Remove(isvisible);
//			propertyDescriptorCollection.Remove(maximumbarslookback);
//			propertyDescriptorCollection.Remove(label);
//			propertyDescriptorCollection.Remove(pdex_inputui);

//					if(indicator.ShowBasicAttributes)
//				{
//						               // propertyDescriptorCollection.Add(paintpricemarkers);
//					  propertyDescriptorCollection.Add(paintpricemarkers); 
//					propertyDescriptorCollection.Add(panel);
//					  propertyDescriptorCollection.Add(isautoscale);
//					  propertyDescriptorCollection.Add(calculate);
//					  propertyDescriptorCollection.Add(scalejustification);
//					  propertyDescriptorCollection.Add(displayindatabox);
//					  propertyDescriptorCollection.Add(displacement);
//					  propertyDescriptorCollection.Add(isvisible);
//					  propertyDescriptorCollection.Add(pdex_inputui);
//					  propertyDescriptorCollection.Add(maximumbarslookback);
//					  propertyDescriptorCollection.Add(label);

//				}
				#endregion
									           
			#region Show Sound Options
//		PropertyDescriptor showsoundoptions 	= propertyDescriptorCollection["ShowSoundOptions"];		
//		PropertyDescriptor soundson				= propertyDescriptorCollection["SoundsOn"];
//		PropertyDescriptor upsoundfile			= propertyDescriptorCollection["UpSoundFile"];
//		PropertyDescriptor alterfontcolour		= propertyDescriptorCollection["AlterFontColour"];
//		PropertyDescriptor textbrushforsound	= propertyDescriptorCollection["TextBrushForSound"];
		
				
//		propertyDescriptorCollection.Remove(soundson);
//		propertyDescriptorCollection.Remove(upsoundfile);
//		propertyDescriptorCollection.Remove(alterfontcolour);
//		propertyDescriptorCollection.Remove(textbrushforsound);
		
//		if(indicator.ShowSoundOptions)
//				{
//					 propertyDescriptorCollection.Add(soundson); 
//					 propertyDescriptorCollection.Add(upsoundfile); 
//					 propertyDescriptorCollection.Add(textbrushforsound); 
//				     propertyDescriptorCollection.Add(alterfontcolour); 
					

//				}
#endregion//

		
			#endregion			

            return propertyDescriptorCollection;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        { return true; }
    }
	#endregion

	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Mindset.BarTimerHybrid[] cacheBarTimerHybrid;
		public Mindset.BarTimerHybrid BarTimerHybrid(bool printTimeZones, string youtube, string youtube1, bool showGuide, bool countDown, bool showMaxRange, int percentageTriggerRange, int percentageTriggerVolume, int percentageTriggerTick, int flashStart, int percentageTrigger, int flash_Duration, bool showPercent, bool showNumLevels)
		{
			return BarTimerHybrid(Input, printTimeZones, youtube, youtube1, showGuide, countDown, showMaxRange, percentageTriggerRange, percentageTriggerVolume, percentageTriggerTick, flashStart, percentageTrigger, flash_Duration, showPercent, showNumLevels);
		}

		public Mindset.BarTimerHybrid BarTimerHybrid(ISeries<double> input, bool printTimeZones, string youtube, string youtube1, bool showGuide, bool countDown, bool showMaxRange, int percentageTriggerRange, int percentageTriggerVolume, int percentageTriggerTick, int flashStart, int percentageTrigger, int flash_Duration, bool showPercent, bool showNumLevels)
		{
			if (cacheBarTimerHybrid != null)
				for (int idx = 0; idx < cacheBarTimerHybrid.Length; idx++)
					if (cacheBarTimerHybrid[idx] != null && cacheBarTimerHybrid[idx].PrintTimeZones == printTimeZones && cacheBarTimerHybrid[idx].Youtube == youtube && cacheBarTimerHybrid[idx].Youtube1 == youtube1 && cacheBarTimerHybrid[idx].ShowGuide == showGuide && cacheBarTimerHybrid[idx].CountDown == countDown && cacheBarTimerHybrid[idx].ShowMaxRange == showMaxRange && cacheBarTimerHybrid[idx].PercentageTriggerRange == percentageTriggerRange && cacheBarTimerHybrid[idx].PercentageTriggerVolume == percentageTriggerVolume && cacheBarTimerHybrid[idx].PercentageTriggerTick == percentageTriggerTick && cacheBarTimerHybrid[idx].FlashStart == flashStart && cacheBarTimerHybrid[idx].PercentageTrigger == percentageTrigger && cacheBarTimerHybrid[idx].Flash_Duration == flash_Duration && cacheBarTimerHybrid[idx].ShowPercent == showPercent && cacheBarTimerHybrid[idx].ShowNumLevels == showNumLevels && cacheBarTimerHybrid[idx].EqualsInput(input))
						return cacheBarTimerHybrid[idx];
			return CacheIndicator<Mindset.BarTimerHybrid>(new Mindset.BarTimerHybrid(){ PrintTimeZones = printTimeZones, Youtube = youtube, Youtube1 = youtube1, ShowGuide = showGuide, CountDown = countDown, ShowMaxRange = showMaxRange, PercentageTriggerRange = percentageTriggerRange, PercentageTriggerVolume = percentageTriggerVolume, PercentageTriggerTick = percentageTriggerTick, FlashStart = flashStart, PercentageTrigger = percentageTrigger, Flash_Duration = flash_Duration, ShowPercent = showPercent, ShowNumLevels = showNumLevels }, input, ref cacheBarTimerHybrid);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Mindset.BarTimerHybrid BarTimerHybrid(bool printTimeZones, string youtube, string youtube1, bool showGuide, bool countDown, bool showMaxRange, int percentageTriggerRange, int percentageTriggerVolume, int percentageTriggerTick, int flashStart, int percentageTrigger, int flash_Duration, bool showPercent, bool showNumLevels)
		{
			return indicator.BarTimerHybrid(Input, printTimeZones, youtube, youtube1, showGuide, countDown, showMaxRange, percentageTriggerRange, percentageTriggerVolume, percentageTriggerTick, flashStart, percentageTrigger, flash_Duration, showPercent, showNumLevels);
		}

		public Indicators.Mindset.BarTimerHybrid BarTimerHybrid(ISeries<double> input , bool printTimeZones, string youtube, string youtube1, bool showGuide, bool countDown, bool showMaxRange, int percentageTriggerRange, int percentageTriggerVolume, int percentageTriggerTick, int flashStart, int percentageTrigger, int flash_Duration, bool showPercent, bool showNumLevels)
		{
			return indicator.BarTimerHybrid(input, printTimeZones, youtube, youtube1, showGuide, countDown, showMaxRange, percentageTriggerRange, percentageTriggerVolume, percentageTriggerTick, flashStart, percentageTrigger, flash_Duration, showPercent, showNumLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Mindset.BarTimerHybrid BarTimerHybrid(bool printTimeZones, string youtube, string youtube1, bool showGuide, bool countDown, bool showMaxRange, int percentageTriggerRange, int percentageTriggerVolume, int percentageTriggerTick, int flashStart, int percentageTrigger, int flash_Duration, bool showPercent, bool showNumLevels)
		{
			return indicator.BarTimerHybrid(Input, printTimeZones, youtube, youtube1, showGuide, countDown, showMaxRange, percentageTriggerRange, percentageTriggerVolume, percentageTriggerTick, flashStart, percentageTrigger, flash_Duration, showPercent, showNumLevels);
		}

		public Indicators.Mindset.BarTimerHybrid BarTimerHybrid(ISeries<double> input , bool printTimeZones, string youtube, string youtube1, bool showGuide, bool countDown, bool showMaxRange, int percentageTriggerRange, int percentageTriggerVolume, int percentageTriggerTick, int flashStart, int percentageTrigger, int flash_Duration, bool showPercent, bool showNumLevels)
		{
			return indicator.BarTimerHybrid(input, printTimeZones, youtube, youtube1, showGuide, countDown, showMaxRange, percentageTriggerRange, percentageTriggerVolume, percentageTriggerTick, flashStart, percentageTrigger, flash_Duration, showPercent, showNumLevels);
		}
	}
}

#endregion
