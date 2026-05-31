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
namespace NinjaTrader.NinjaScript.Indicators.TradeSaber
{
	public class ReversalTS : Indicator, ICustomTypeDescriptor 
	{
		private double percentageCalc;
		private double priceCalc;
		private double tickCalc;
		
		//Picks Highest Offset
		private double candleBarOffset;							
		
		private bool colorReversalBar;
		private bool setRevFlash;
		private bool setVolumeAlert;
		
		private bool simpleReverse;
		
		private bool setChartRev;

		
		private int CalcInt;
		
		
		#region TradeSaber Social
		
		private string author 								= "TradeSaber(Dre)";
		private string version 								= "Version 2.0 // June 2023";
		
		private string youtube								= "https://youtu.be/IqMJCp8N0-4"; 
		private string discord								= "https://discord.gg/2YU9GDme8j";
		private string tradeSaber							= "https://tradesaber.com/";
		
		private bool showSocials;
		
		private bool youtubeButtonClicked;
		private bool discordButtonClicked;
		private bool tradeSaberButtonClicked;
		
		private System.Windows.Controls.Button youtubeButton;
		private System.Windows.Controls.Button discordButton;
		private System.Windows.Controls.Button tradeSaberButton;
		
		
		private System.Windows.Controls.Grid myGrid29;
		
		#endregion
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ReversalTS";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				///Default colors for Reversal Bars
				colorReversalBar							= true;
				GreenReversalBar 							= Brushes.DarkCyan;
				RedReversalBar								= Brushes.Indigo;
				
				simpleReverse								= true;
				
				priceOffset 								= 0.00;
				percentageOffset 							= 0.00;
				tickOffset									= 1;
				
				///Reversal Bar Alerts
				CustomSoundFile								= @"C:\Program Files\NinjaTrader 8\sounds\Alert2.wav";
				
				SetVolumeAlert								= false;
				SetRevFlash									= false;
				FlashBrush 									= Brushes.Green;
				
				setChartRev									= true;
				
			
				
				///Plot for Market Analyzer settings
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Dot, "CurrentReversalBar");
				
				///Will show buttons with links to Youtube/Discord.
				showSocials									= true;
				
				
				
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				if (Calculate == Calculate.OnBarClose)
				{
					CalcInt = 0;
				}
				
				if (Calculate == Calculate.OnPriceChange || Calculate == Calculate.OnEachTick)
				{
					CalcInt = 1;
				}
			}
			
		#region Add Buttons with Links
			
		else if (State == State.Historical)
		{
			#region TradeSaber Socials
			
			if (showSocials)
			{
				if (UserControlCollection.Contains(myGrid29))
					return;
				
				Dispatcher.InvokeAsync((() =>
				{
					myGrid29 = new System.Windows.Controls.Grid
					{
						Name = "MyCustomGrid", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Bottom
					};
					
					System.Windows.Controls.ColumnDefinition column1 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column2 = new System.Windows.Controls.ColumnDefinition();
					System.Windows.Controls.ColumnDefinition column3 = new System.Windows.Controls.ColumnDefinition();
					
					myGrid29.ColumnDefinitions.Add(column1);
					myGrid29.ColumnDefinitions.Add(column2);
					myGrid29.ColumnDefinitions.Add(column3);
					
					youtubeButton = new System.Windows.Controls.Button
					{
						Name = "YoutubeButton", Content = "Youtube", Foreground = Brushes.White, Background = Brushes.Red
					};
					
					discordButton = new System.Windows.Controls.Button
					{
						Name = "DiscordButton", Content = "Discord", Foreground = Brushes.White, Background = Brushes.RoyalBlue
					};
					
					tradeSaberButton = new System.Windows.Controls.Button
					{
						Name = "TradeSaberButton", Content = "TradeSaber", Foreground = Brushes.White, Background = Brushes.DarkOrange
					};
					
					youtubeButton.Click += OnButtonClick;
					discordButton.Click += OnButtonClick;
					tradeSaberButton.Click += OnButtonClick;
					
					System.Windows.Controls.Grid.SetColumn(youtubeButton, 0);
					System.Windows.Controls.Grid.SetColumn(discordButton, 1);
					System.Windows.Controls.Grid.SetColumn(tradeSaberButton, 2);
					
					myGrid29.Children.Add(youtubeButton);
					myGrid29.Children.Add(discordButton);
					myGrid29.Children.Add(tradeSaberButton);
					
					UserControlCollection.Add(myGrid29);
				}));
			}
		#endregion
			
			else if (State == State.Terminated)
			{
				#region Terminate TradeSaber Socials
			
			if (showSocials)
			{
				Dispatcher.InvokeAsync((() =>
				{
					if (myGrid29 != null)
					{
						if (youtubeButton != null)
						{
							myGrid29.Children.Remove(youtubeButton);
							youtubeButton.Click -= OnButtonClick;
							youtubeButton = null;
						}
						
						if (discordButton != null)
						{
							myGrid29.Children.Remove(discordButton);
							discordButton.Click -= OnButtonClick;
							discordButton = null;
						}
						
						if (tradeSaberButton != null)
						{
							myGrid29.Children.Remove(tradeSaberButton);
							tradeSaberButton.Click -= OnButtonClick;
							tradeSaberButton = null;
						}		
					}
				}));
			}
		#endregion
			}
		}
		#endregion	
			
		}
		
		
		public override string DisplayName
		{
        	get 
			{ 
				if  (State == State.SetDefaults) 
				return Name; 
		
				else  return "TradeSaber Reversal Bar"; 
			}

		}
		
		protected override void OnBarUpdate()
		{
			
			if(CurrentBars[0] < 2) 
				return;
			
			BarBrush	= null;
			
			
		#region offset Calculations
			
			//Percent for the current Rev forming
			percentageCalc = (High[1] - Low[1]) * percentageOffset;
			
			//These are the same for both because they do not rely on another candle
			priceCalc = 	priceOffset;
			tickCalc = TickSize * tickOffset;
			
			//Picks Highest Offset for the Current Rev
			candleBarOffset = Math.Max(percentageCalc, Math.Max(priceCalc, tickCalc));

		#endregion		
		
		#region Current Reversal Bar Logic
			
		if (simpleReverse == true)
		{
			if (Close[1] < Open[1] && Close[0] > Open[0])
			{
				CurrentReversalBar[0] = 1;
			}
			
			else if (Close[1] > Open[1] && Close[0] < Open[0])
			{
				CurrentReversalBar[0] = -1;
			}
			
			else 
			{
				CurrentReversalBar[0] = 0;
			}
			
		}
			
		if (simpleReverse == false)
		{
			//Currently a Bullish Reversal Pattern
				if (
					((Low[0] + (candleBarOffset)) <= Low[1]) 
						&& (Close[0] >= Close[1])
							&& (Open[0] < Close[0])
								&& (Open[1] > Close[1])
					)
			
			{
				CurrentReversalBar[0] = 1;
			}
		
			//Currently a Bearish Reversal Pattern	
				else if (
					((High[0] - (candleBarOffset)) >= High[1]) 
						&& (Close[0] <= Close[1])
							&& (Open[0] > Close[0])
								&& (Open[1] < Close[1])
					)
			
			{
				CurrentReversalBar[0] = -1;
			}
				else
			{
				CurrentReversalBar[0] = 0;
			}
		}

	#endregion
		
			
		if (setChartRev)
		{
			#region Color Reversal Bars
			
	///Reversal Bar Color
		if (colorReversalBar)
		{
			//Red Reversal Bar Logic
			if (CurrentReversalBar[0] == 1) 
			{
				BarBrush = GreenReversalBar;
			}
			
			//Green Reversal Bar Logic
			else if (CurrentReversalBar[0] == -1) 
			{
				BarBrush = RedReversalBar;
			}
				
			else if (CurrentReversalBar[0] == 0)
			{
				BarBrush = null;
			}
		}
	
	#endregion
		}
		
		
			#region Flash Alert
		
		///Flash Alert (Once Bar Closes)
		if ((CurrentReversalBar[CalcInt] == 1 || CurrentReversalBar[CalcInt] == -1) && IsFirstTickOfBar && SetRevFlash)
		{
			BackBrushes[CalcInt] = FlashBrush;
		}
		
		else if (CurrentReversalBar[CalcInt] == 0 && IsFirstTickOfBar && SetRevFlash)
		{
			BackBrushes = null;
		}
		
		
			
	#endregion 
			
			#region Sound Alert
			
		///Sound Alert (Once Bar Closes)	
		if ((CurrentReversalBar[CalcInt] == 1 || CurrentReversalBar[CalcInt] == -1) && IsFirstTickOfBar && SetVolumeAlert)
		{
			PlaySound(CustomSoundFile);	
		}

			
	#endregion
			
		}
			
		#region Button Click Event
		
		private void OnButtonClick(object sender, RoutedEventArgs rea)
		{
			System.Windows.Controls.Button button = sender as System.Windows.Controls.Button;
			
			#region TradeSaber Socials
			
			if (showSocials)
			{
				if (button == youtubeButton && button.Name == "YoutubeButton" && button.Content == "Youtube")
				{
					System.Diagnostics.Process.Start(youtube);
					return;
				}
				
				if (button == discordButton && button.Name == "DiscordButton" && button.Content == "Discord")
				{	
					System.Diagnostics.Process.Start(discord);
					return;
				}
				
				if (button == tradeSaberButton && button.Name == "TradeSaberButton" && button.Content == "TradeSaber")
				{	
					System.Diagnostics.Process.Start(tradeSaber);
					return;
				}
			}
			
			#endregion
		}
	
	#endregion	
	
		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CurrentReversalBar
		{ get { return Values[0]; } }
	
		#region 01. Offset Properties
	///Offset for Reversal Bars. Allows Price to come outside of the High/Low of the previous candle if the user chooses.
		
		[NinjaScriptProperty]
		[Display(Name = "Price Offset", GroupName = "01. Reversal Bar Offset", Order = 1)]
		public double priceOffset
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Percentage Offset", GroupName = "01. Reversal Bar Offset", Order = 2)]
		public double percentageOffset
		{ get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "Tick Offset", Description = "Sensetive Value", Order = 3, GroupName = "01. Reversal Bar Offset")]
        public int tickOffset 
		{ get; set; }
		
		
		
		#endregion
		
		#region 02. Color Reversal Bars
	///Change the color of the Green and Red Reversal Bars.
		
		[NinjaScriptProperty]
		[Display(Name = "Enable Color Reversal Bars", Description = "", Order = 0, GroupName = "02. Reversal Bar Custom Color")]
		public bool ColorReversalBar 
		{
		 	get{return colorReversalBar;} 
			set{colorReversalBar = (value);} 
		}
		
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Green Reversal Bar", GroupName = "02. Reversal Bar Custom Color", Order = 1)]
		public Brush GreenReversalBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string GreenReversalBarSerialize
		{
			get { return Serialize.BrushToString(GreenReversalBar); }
   			set { GreenReversalBar = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Red Reversal Bar", GroupName = "02. Reversal Bar Custom Color", Order = 2)]
		public Brush RedReversalBar
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string RedReversalBarSerialize
		{
			get { return Serialize.BrushToString(RedReversalBar); }
   			set { RedReversalBar = Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		#region 03. Flash Alert
	///Enable Flash Alert and Change color 	
		
		[NinjaScriptProperty]
		[Display(ResourceType = typeof (Custom.Resource), Name = "Enable Reversal Bar Flash", Order = 0, GroupName = "03. Flash Alert")]
		public bool SetRevFlash
		{ get; set; }	
		
		[NinjaScriptProperty]
		[XmlIgnore()]
		[Display(Name = "Flash Brush", GroupName = "03. Flash Alert", Order = 1)]
		public Brush FlashBrush
		{ get; set; }
		
		// Serialize our Color object
		[Browsable(false)]
		public string FlashBrushSerialize
		{
			get { return Serialize.BrushToString(FlashBrush); }
   			set { FlashBrush = Serialize.StringToBrush(value); }
		}
		#endregion
		
		#region 04. Sound Alerts
	///Enable Sound Alert	
		
		[Display(ResourceType = typeof (Custom.Resource), Name = "Enable Volume Alert", Order = 0, GroupName = "04. Audio Alert")]
		public bool SetVolumeAlert
		{ get; set; }
		
		[Display(Name="Alert sound file", Description="Enter sound file path/name", Order=1, GroupName="04. Audio Alert")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter="Wav Files (*.wav)|*.wav")]
		public string CustomSoundFile
		{ get; set; }
		
		#endregion
		
		#region 29. TradeSaber Socials
		
		[NinjaScriptProperty]
		[Display(Name = "Show Social Media Buttons", Description = "", Order = 0, GroupName = "29. TradeSaber Socials")]
		public bool ShowSocials 
		{
		 	get{return showSocials;} 
			set{showSocials = (value);} 
		}
		
		
		[Display(Name="Explanation Video", Order=1, GroupName="29. TradeSaber Socials")]
		public  string Youtube
		{
		 	get{return youtube;} 
			set{youtube = (value);} 
		}
		
		
		[Display(Name="Discord Link", Order=2, GroupName="29. TradeSaber Socials")]
		public  string Discord
		{
		 	get{return discord;} 
			set{discord = (value);} 
		}
		
		
		[Display(Name="TradeSaber Link", Order=3, GroupName="29. TradeSaber Socials")]
		public  string TradeSaber
		{
		 	get{return tradeSaber;} 
			set{tradeSaber = (value);} 
		}
		
		
		[ReadOnly(true)]
		[Display(Name = "Author", GroupName = "29. TradeSaber Socials", Order = 4)]
		public string Author
		{
		 	get{return author;} 
			set{author = (value);} 
		}
		
		
		[ReadOnly(true)]
		[Display(Name = "Version", GroupName = "29. TradeSaber Socials", Order = 5)]
		public string Version
		{
		 	get{return version;} 
			set{version = (value);} 
		}
		
		#endregion
		
		#endregion
		
		
		#region Custom Property Manipulation

        private void ModifyProperties(PropertyDescriptorCollection col)
        {	
			if (simpleReverse == true)
            {
				col.Remove(col.Find("priceOffset", true));
				col.Remove(col.Find("percentageOffset", true));
				col.Remove(col.Find("tickOffset", true));
            }
		}
		
		#endregion
		
		#region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(GetType());
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(GetType());
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(GetType());
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(GetType());
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(GetType());
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(GetType());
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(GetType(), editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(GetType(), attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(GetType());
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection orig = TypeDescriptor.GetProperties(GetType(), attributes);
            PropertyDescriptor[] arr = new PropertyDescriptor[orig.Count];
            orig.CopyTo(arr, 0);
            PropertyDescriptorCollection col = new PropertyDescriptorCollection(arr);

            ModifyProperties(col);
            return col;

        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(GetType());
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion
		
		#region Pattern Selector
		
		public enum patternSelector
		{
			SimpleReversal 	= 0,
			WickReversal	= 1,
		};
		
		patternSelector showPatternEnum;
		
		
		[Display(Name = "Reversal Selector", Order = 0, GroupName = "01. Reversal Bar Offset")]
		[RefreshProperties(RefreshProperties.All)]
		public patternSelector PatternSelector
		{
			get { return showPatternEnum; }
			set
			{
				showPatternEnum = value;
				if (showPatternEnum == patternSelector.SimpleReversal)
				{
					simpleReverse 	= true;
				}
				else if (showPatternEnum == patternSelector.WickReversal)
				{
					simpleReverse 	= false;
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
		private TradeSaber.ReversalTS[] cacheReversalTS;
		public TradeSaber.ReversalTS ReversalTS(double priceOffset, double percentageOffset, int tickOffset, bool colorReversalBar, Brush greenReversalBar, Brush redReversalBar, bool setRevFlash, Brush flashBrush, bool showSocials)
		{
			return ReversalTS(Input, priceOffset, percentageOffset, tickOffset, colorReversalBar, greenReversalBar, redReversalBar, setRevFlash, flashBrush, showSocials);
		}

		public TradeSaber.ReversalTS ReversalTS(ISeries<double> input, double priceOffset, double percentageOffset, int tickOffset, bool colorReversalBar, Brush greenReversalBar, Brush redReversalBar, bool setRevFlash, Brush flashBrush, bool showSocials)
		{
			if (cacheReversalTS != null)
				for (int idx = 0; idx < cacheReversalTS.Length; idx++)
					if (cacheReversalTS[idx] != null && cacheReversalTS[idx].priceOffset == priceOffset && cacheReversalTS[idx].percentageOffset == percentageOffset && cacheReversalTS[idx].tickOffset == tickOffset && cacheReversalTS[idx].ColorReversalBar == colorReversalBar && cacheReversalTS[idx].GreenReversalBar == greenReversalBar && cacheReversalTS[idx].RedReversalBar == redReversalBar && cacheReversalTS[idx].SetRevFlash == setRevFlash && cacheReversalTS[idx].FlashBrush == flashBrush && cacheReversalTS[idx].ShowSocials == showSocials && cacheReversalTS[idx].EqualsInput(input))
						return cacheReversalTS[idx];
			return CacheIndicator<TradeSaber.ReversalTS>(new TradeSaber.ReversalTS(){ priceOffset = priceOffset, percentageOffset = percentageOffset, tickOffset = tickOffset, ColorReversalBar = colorReversalBar, GreenReversalBar = greenReversalBar, RedReversalBar = redReversalBar, SetRevFlash = setRevFlash, FlashBrush = flashBrush, ShowSocials = showSocials }, input, ref cacheReversalTS);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TradeSaber.ReversalTS ReversalTS(double priceOffset, double percentageOffset, int tickOffset, bool colorReversalBar, Brush greenReversalBar, Brush redReversalBar, bool setRevFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.ReversalTS(Input, priceOffset, percentageOffset, tickOffset, colorReversalBar, greenReversalBar, redReversalBar, setRevFlash, flashBrush, showSocials);
		}

		public Indicators.TradeSaber.ReversalTS ReversalTS(ISeries<double> input , double priceOffset, double percentageOffset, int tickOffset, bool colorReversalBar, Brush greenReversalBar, Brush redReversalBar, bool setRevFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.ReversalTS(input, priceOffset, percentageOffset, tickOffset, colorReversalBar, greenReversalBar, redReversalBar, setRevFlash, flashBrush, showSocials);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TradeSaber.ReversalTS ReversalTS(double priceOffset, double percentageOffset, int tickOffset, bool colorReversalBar, Brush greenReversalBar, Brush redReversalBar, bool setRevFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.ReversalTS(Input, priceOffset, percentageOffset, tickOffset, colorReversalBar, greenReversalBar, redReversalBar, setRevFlash, flashBrush, showSocials);
		}

		public Indicators.TradeSaber.ReversalTS ReversalTS(ISeries<double> input , double priceOffset, double percentageOffset, int tickOffset, bool colorReversalBar, Brush greenReversalBar, Brush redReversalBar, bool setRevFlash, Brush flashBrush, bool showSocials)
		{
			return indicator.ReversalTS(input, priceOffset, percentageOffset, tickOffset, colorReversalBar, greenReversalBar, redReversalBar, setRevFlash, flashBrush, showSocials);
		}
	}
}

#endregion
