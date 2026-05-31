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

using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Automation.Provider;

using LevelDisplayMode = NinjaTrader.NinjaScript.Indicators.AlightenDynamicLevelsEnums.LevelDisplayMode;

#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	public class AlightenDynamicLevelsEnums
	{
	    public enum LevelDisplayMode
	    {
	        ClosestToPrice,
	        MostRecent
	    }
	}
}

namespace NinjaTrader.NinjaScript.Indicators
{
    public class AlightenDynamicLevels : Indicator
    {
		
        #region Variables
        private List<int>         				pivotBars;
        private List<double>      				pivotPrices;
        private List<bool>        				pivotIsHigh;
        private HashSet<string>   				previousLineTags = new HashSet<string>();

        private const string      				tagPrefix        = "ZZ_line_";
        private const string      				horizLinePrefix  = "ZZ_Horz_";
        private string            				instanceId;
		
		private System.Windows.Controls.Button 	exportButton;
		private Chart 							chartWindow;
		private bool 							buttonsCreated = false;

		private class ExportPivot
		{
		    public double Level;
		    public DateTime Time;
		    public string Status;
		}
		private List<ExportPivot> exportPivots = new List<ExportPivot>();



        #endregion

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "Enable Global Projections", GroupName = "Parameters", Order = 1)]
        public bool EnableGlobalProjections { get; set; } = false;

        [NinjaScriptProperty]
        [Range(1, 100)]
        [Display(Name = "Number of Levels (Recent or Above/Below)", GroupName = "Parameters", Order = 3)]
        public int NumberOfLevels { get; set; } = 10;
		
		[NinjaScriptProperty]
		[Display(Name = "Level Display Option", GroupName = "Parameters", Order = 4)]
		public LevelDisplayMode LevelDisplayOption { get; set; } = LevelDisplayMode.ClosestToPrice;
		
		[NinjaScriptProperty]
		[Display(Name = "Show All Levels", GroupName = "Parameters", Order = 5)]
		public bool ShowAllLevels { get; set; } = false;
		
		[XmlIgnore]
		[NinjaScriptProperty]
		[Display(Name = "ZigZag Color", Order = 6, GroupName = "Parameters")]
		public System.Windows.Media.Brush ZigZagColor { get; set; } = System.Windows.Media.Brushes.Yellow;
		[Browsable(false)]
		public string ZigZagColorSerialize
		{
		    get { return Serialize.BrushToString(ZigZagColor); }
		    set { ZigZagColor = Serialize.StringToBrush(value); }
		}
		
		#endregion
		
		#region Color Settings
		
		[NinjaScriptProperty]
        [Display(Name = "Line Template - Support", GroupName = "Color Settings", Order = 2)]
        [TypeConverter(typeof(DrawingTemplateListConverter))]
        public string LineTemplateGained { get; set; } = "";		
		
		[NinjaScriptProperty]
        [Display(Name = "Line Template - Resistance", GroupName = "Color Settings", Order = 3)]
        [TypeConverter(typeof(DrawingTemplateListConverter))]
        public string LineTemplateLost { get; set; } = "";	
		
		#endregion
		
		#region Debugging

        [NinjaScriptProperty]
        [Display(Name = "Enable Debug Output", GroupName = "Debugging", Order = 100)]
        public bool DebugPrints { get; set; } = false;
        #endregion

        #region OnStateChange
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description 					= "Plots change in structure levels at higher time frames - by Alighten";
                Name 							= "Alighten DynamicLevels";
                Calculate 						= Calculate.OnBarClose;
                IsOverlay 						= true;
                DrawOnPricePanel 				= true;
                DisplayInDataBox 				= true;
                PaintPriceMarkers 				= true;
                ScaleJustification 				= ScaleJustification.Right;
                IsSuspendedWhileInactive 		= true;
            }
            else if (State == State.Configure)
            {
                instanceId = $"ZigZag_{Instrument.FullName}_{BarsPeriod.BarsPeriodType}_{BarsPeriod.Value}_{Guid.NewGuid().ToString()}";
            }
            else if (State == State.DataLoaded)
            {
                pivotBars 		= new List<int>();
                pivotPrices 	= new List<double>();
                pivotIsHigh 	= new List<bool>();

                DPrint($"instanceID={instanceId}");

                foreach (var tag in previousLineTags.ToList())
                {
                    try { RemoveDrawObject(tag); } catch { }
                }
                previousLineTags.Clear();
				
				exportPivots.Clear();
				
				if (ChartControl != null && ChartControl.Dispatcher != null && !buttonsCreated)
			    {
			        ChartControl.Dispatcher.InvokeAsync(() =>
			        {
			            CreateWPFControls();
			        });
			    }
            }
            else if (State == State.Terminated)
            {
                if (EnableGlobalProjections)
                {
                    foreach (var tag in previousLineTags.ToList())
                    {
                        try { RemoveDrawObject(tag); } catch { }
                    }
                    previousLineTags.Clear();
                }
				
				exportPivots.Clear();
				
				if (ChartControl != null && ChartControl.Dispatcher != null && buttonsCreated)
		        {
		             ChartControl.Dispatcher.InvokeAsync(() =>
		             {
		                RemoveWPFControls(); // Call cleanup method
		             });
		        }
            }
        }
        #endregion

        #region OnBarUpdate
        protected override void OnBarUpdate()
        {
            if (CurrentBar < 2)
                return;
			
            DPrint($"[ZZX] Bar={CurrentBar} Time={Time[0]:HH:mm}  O={Open[0]:F2} H={High[0]:F2} L={Low[0]:F2} C={Close[0]:F2}");

            bool isHigh 			= High[0] > High[1];
            bool isLow 				= Low[0] < Low[1];
            bool previousGreen 		= Close[1] > Open[1];
            bool previousRed 		= Close[1] < Open[1];
            bool currentGreen 		= Close[0] > Open[0];
            bool currentRed 		= Close[0] < Open[0];

            DPrint($"[ZZX]   isHigh={isHigh}  isLow={isLow}");
            bool hasLast 		= pivotBars.Count > 0;
            bool lastIsHigh 	= hasLast && pivotIsHigh.Last();
            DPrint($"[ZZX]   hasLast={hasLast}  lastIsHigh={lastIsHigh}");

            if (previousGreen && currentGreen && isHigh) 	{ ProcessPivot(CurrentBar, High[0], true); DPrint($"time={Time[0]}, TEST 1"); }
            if (previousRed && currentRed && isLow) 		{ ProcessPivot(CurrentBar, Low[0], false); DPrint($"time={Time[0]}, TEST 2"); }
            if (previousGreen && currentRed && isHigh) 		{ ProcessPivot(CurrentBar, High[0], true); DPrint($"time={Time[0]}, TEST 3"); }
            if (previousRed && currentGreen && isLow) 		{ ProcessPivot(CurrentBar, Low[0], false); DPrint($"time={Time[0]}, TEST 4"); }
            if (previousRed && currentGreen && isHigh) 		{ ProcessPivot(CurrentBar, High[0], true); DPrint($"time={Time[0]}, TEST 5"); }
            if (previousGreen && currentRed && isLow) 		{ ProcessPivot(CurrentBar, Low[0], false); DPrint($"time={Time[0]}, TEST 6"); }

            if (pivotBars.Count > 0)
            {
                var dump = pivotBars.Select((b, i) => $"{(pivotIsHigh[i] ? "H" : "L")}@{b}:{pivotPrices[i]:F2}").Aggregate((a, b) => a + " | " + b);
                DPrint($"[ZZX]   STORED Pivots → {dump}");
            }
            else
                DPrint("[ZZX]   (no pivots stored yet)");
        }
        #endregion

        #region Pivot Processing
        private void ProcessPivot(int barIndex, double price, bool isHigh)
        {
            if (pivotBars.Count == 0)
            {
                AddPivot(barIndex, price, isHigh);
                DPrint($"[ZZ]     Added FIRST pivot (isHigh={isHigh}) at {barIndex}, price={price:F2}");
                return;
            }

            bool lastIsHigh 	= pivotIsHigh.Last();
            double lastPrice 	= pivotPrices.Last();

            if (isHigh == lastIsHigh)
            {
                if ((isHigh && price > lastPrice) || (!isHigh && price < lastPrice))
                {
                    DPrint($"[ZZ]     Updating last pivot: oldPrice={lastPrice:F2} → newPrice={price:F2}");
                    pivotPrices[pivotPrices.Count - 1] = price;
                    pivotBars[pivotBars.Count - 1] = barIndex;
                    RedrawZigZag();
                }
                else
                {
                    DPrint($"[ZZ]     SAME-DIR but no extremum change: price={price:F2} lastPrice={lastPrice:F2}");
                }
            }
            else
            {
                AddPivot(barIndex, price, isHigh);
                DPrint($"[ZZ]     ADDED alternating pivot (isHigh={isHigh}) at {barIndex}, price={price:F2}");
            }
        }

        private void AddPivot(int barIndex, double price, bool isHigh)
		{
		    pivotBars.Add(barIndex);
		    pivotPrices.Add(price);
		    pivotIsHigh.Add(isHigh);
		
		    RedrawZigZag();
		}
        #endregion

        #region Drawing Logic
        private void RedrawZigZag()
        {
            foreach (var tag in previousLineTags)
            {
                try { RemoveDrawObject(tag); } catch { }
            }
            previousLineTags.Clear();
			exportPivots.Clear();

            for (int i = 1; i < pivotBars.Count; i++)
            {
				string tag = $"{tagPrefix}{instanceId}_{i}";
                Draw.Line(this, tag, false,
                    CurrentBar - pivotBars[i - 1], pivotPrices[i - 1],
                    CurrentBar - pivotBars[i], pivotPrices[i],
                    ZigZagColor, DashStyleHelper.Solid, 2);
                previousLineTags.Add(tag);
            }

            double price = Close[0];
            var pivotsWithDistance = pivotBars.Select((barIdx, i) => new
            {
                Index 		= i,
                BarIndex 	= barIdx,
                IsHigh 		= pivotIsHigh[i],
                Level 		= pivotIsHigh[i] ? Math.Max(Open[CurrentBar - barIdx], Close[CurrentBar - barIdx]) : Math.Min(Open[CurrentBar - barIdx], Close[CurrentBar - barIdx]),
                Distance 	= pivotIsHigh[i] ? Math.Max(Open[CurrentBar - barIdx], Close[CurrentBar - barIdx]) - price : price - Math.Min(Open[CurrentBar - barIdx], Close[CurrentBar - barIdx])
            }).ToList();

            IEnumerable<dynamic> visiblePivots;

			if (ShowAllLevels)
			{
			    visiblePivots = pivotsWithDistance;
			}
			else if (LevelDisplayOption == LevelDisplayMode.ClosestToPrice)
			{
			    visiblePivots = pivotsWithDistance
			        .Where(x => x.Level != price)
			        .OrderBy(x => Math.Abs(x.Distance))
			        .Take(NumberOfLevels * 2);
			}
			else // MostRecent
			{
			    visiblePivots = pivotsWithDistance
			        .Where(x => x.Level != price)
			        .OrderByDescending(x => x.BarIndex)
			        .Take(NumberOfLevels);
			}
			foreach (var pivot in visiblePivots)
			{
			    string tag = $"{horizLinePrefix}{instanceId}_{pivot.BarIndex}_{(pivot.IsHigh ? "H" : "L")}";
			    Brush lineBrush = pivot.IsHigh ? Brushes.Red : Brushes.Blue;
				
				string status = Close[0] > pivot.Level ? "GAINED" : "LOST";
				bool isGained = Close[0] > pivot.Level;
				string templateToUse = isGained ? LineTemplateGained : LineTemplateLost;
				DateTime time = Time[CurrentBar - pivot.BarIndex];
			
			    Draw.Line(this, tag, false,
			        CurrentBar - pivot.BarIndex, pivot.Level,
			        -1, pivot.Level,
			        EnableGlobalProjections,
			        templateToUse);
								
				exportPivots.Add(new ExportPivot { Level = pivot.Level, Time = time, Status = status });
			
			    previousLineTags.Add(tag);
			}

        }
        #endregion
		
		#region Toolbar Button
		private void CreateWPFControls()
		{
			if (ChartControl == null || buttonsCreated)
				return;
		
			chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
			if (chartWindow == null)
				return;
		
			// Check for duplicate using AutomationId
			foreach (var item in chartWindow.MainMenu)
			{
				if (item is DependencyObject depObj &&
			        System.Windows.Automation.AutomationProperties.GetAutomationId(depObj) == "ExportZigZagLevelsButton")
			        return;
			}
			
			Style buttonStyle = new Style(typeof(System.Windows.Controls.Button));
			buttonStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontFamilyProperty, new FontFamily("Segoe UI")));
			buttonStyle.Setters.Add(new Setter(System.Windows.Controls.Button.FontSizeProperty, 12.0));
			buttonStyle.Setters.Add(new Setter(System.Windows.Controls.Button.ForegroundProperty, Brushes.White)); // <== Font color

		
			exportButton = new System.Windows.Controls.Button
			{
				Content 	= "Export ZigZag Levels",
				Style 		= buttonStyle,
				Margin 		= new Thickness(2, 0, 2, 0),
				Padding 	= new Thickness(4, 2, 4, 2),
				ToolTip 	= "Export AlightenDynamicLevels levels to CSV"
			};
		
			System.Windows.Automation.AutomationProperties.SetAutomationId(exportButton, "ExportZigZagLevelsButton");
			exportButton.Click += ExportButton_Click;
		
			chartWindow.MainMenu.Add(exportButton);
			buttonsCreated = true;
		}
		private void RemoveWPFControls()
		{
			if (chartWindow == null || exportButton == null)
				return;
		
			if (chartWindow.MainMenu.Contains(exportButton))
			{
				chartWindow.MainMenu.Remove(exportButton);
			}
		
			exportButton.Click -= ExportButton_Click;
			exportButton = null;
			buttonsCreated = false;
		}
		private void ExportButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				string instrumentName = Instrument.MasterInstrument.Name;
				string timeFrame = $"{BarsPeriod.Value}_{BarsPeriod.BarsPeriodType}";
				string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
				
				string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AlightenDynamicLevels", instrumentName);
				if (!System.IO.Directory.Exists(folderPath))
					System.IO.Directory.CreateDirectory(folderPath);
		
				string filename = $"{instrumentName}_{timeFrame}_{timestamp}.txt";
				string fullPath = System.IO.Path.Combine(folderPath, filename);
		
				using (var writer = new System.IO.StreamWriter(fullPath))
				{
				    foreach (var pivot in exportPivots)
				    {
				        writer.WriteLine($"{instrumentName}, {timeFrame}, {pivot.Level:F2}, {pivot.Time:MM/dd/yyyy HH:mm:ss tt}, {pivot.Status}");
				    }
				}
		
				System.Windows.MessageBox.Show($"Export complete:\n{fullPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show($"Export failed:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion
		
        #region Utilities
        private void DPrint(string message)
        {
            if (DebugPrints)
                Print(message);
        }
        #endregion

        #region Drawing Template Support
        public class DrawingTemplateListConverter : TypeConverter
        {
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var values = new List<string>();
                string path = System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "templates", "DrawingTool", "Line");
                if (System.IO.Directory.Exists(path))
                    values.AddRange(System.IO.Directory.GetFiles(path, "*.xml").Select(System.IO.Path.GetFileNameWithoutExtension));
                values.Insert(0, "");
                return new StandardValuesCollection(values);
            }
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) => value.ToString();
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) => value;
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => true;
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => true;
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => false;
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AlightenDynamicLevels[] cacheAlightenDynamicLevels;
		public AlightenDynamicLevels AlightenDynamicLevels(bool enableGlobalProjections, int numberOfLevels, LevelDisplayMode levelDisplayOption, bool showAllLevels, System.Windows.Media.Brush zigZagColor, string lineTemplateGained, string lineTemplateLost, bool debugPrints)
		{
			return AlightenDynamicLevels(Input, enableGlobalProjections, numberOfLevels, levelDisplayOption, showAllLevels, zigZagColor, lineTemplateGained, lineTemplateLost, debugPrints);
		}

		public AlightenDynamicLevels AlightenDynamicLevels(ISeries<double> input, bool enableGlobalProjections, int numberOfLevels, LevelDisplayMode levelDisplayOption, bool showAllLevels, System.Windows.Media.Brush zigZagColor, string lineTemplateGained, string lineTemplateLost, bool debugPrints)
		{
			if (cacheAlightenDynamicLevels != null)
				for (int idx = 0; idx < cacheAlightenDynamicLevels.Length; idx++)
					if (cacheAlightenDynamicLevels[idx] != null && cacheAlightenDynamicLevels[idx].EnableGlobalProjections == enableGlobalProjections && cacheAlightenDynamicLevels[idx].NumberOfLevels == numberOfLevels && cacheAlightenDynamicLevels[idx].LevelDisplayOption == levelDisplayOption && cacheAlightenDynamicLevels[idx].ShowAllLevels == showAllLevels && cacheAlightenDynamicLevels[idx].ZigZagColor == zigZagColor && cacheAlightenDynamicLevels[idx].LineTemplateGained == lineTemplateGained && cacheAlightenDynamicLevels[idx].LineTemplateLost == lineTemplateLost && cacheAlightenDynamicLevels[idx].DebugPrints == debugPrints && cacheAlightenDynamicLevels[idx].EqualsInput(input))
						return cacheAlightenDynamicLevels[idx];
			return CacheIndicator<AlightenDynamicLevels>(new AlightenDynamicLevels(){ EnableGlobalProjections = enableGlobalProjections, NumberOfLevels = numberOfLevels, LevelDisplayOption = levelDisplayOption, ShowAllLevels = showAllLevels, ZigZagColor = zigZagColor, LineTemplateGained = lineTemplateGained, LineTemplateLost = lineTemplateLost, DebugPrints = debugPrints }, input, ref cacheAlightenDynamicLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AlightenDynamicLevels AlightenDynamicLevels(bool enableGlobalProjections, int numberOfLevels, LevelDisplayMode levelDisplayOption, bool showAllLevels, System.Windows.Media.Brush zigZagColor, string lineTemplateGained, string lineTemplateLost, bool debugPrints)
		{
			return indicator.AlightenDynamicLevels(Input, enableGlobalProjections, numberOfLevels, levelDisplayOption, showAllLevels, zigZagColor, lineTemplateGained, lineTemplateLost, debugPrints);
		}

		public Indicators.AlightenDynamicLevels AlightenDynamicLevels(ISeries<double> input , bool enableGlobalProjections, int numberOfLevels, LevelDisplayMode levelDisplayOption, bool showAllLevels, System.Windows.Media.Brush zigZagColor, string lineTemplateGained, string lineTemplateLost, bool debugPrints)
		{
			return indicator.AlightenDynamicLevels(input, enableGlobalProjections, numberOfLevels, levelDisplayOption, showAllLevels, zigZagColor, lineTemplateGained, lineTemplateLost, debugPrints);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AlightenDynamicLevels AlightenDynamicLevels(bool enableGlobalProjections, int numberOfLevels, LevelDisplayMode levelDisplayOption, bool showAllLevels, System.Windows.Media.Brush zigZagColor, string lineTemplateGained, string lineTemplateLost, bool debugPrints)
		{
			return indicator.AlightenDynamicLevels(Input, enableGlobalProjections, numberOfLevels, levelDisplayOption, showAllLevels, zigZagColor, lineTemplateGained, lineTemplateLost, debugPrints);
		}

		public Indicators.AlightenDynamicLevels AlightenDynamicLevels(ISeries<double> input , bool enableGlobalProjections, int numberOfLevels, LevelDisplayMode levelDisplayOption, bool showAllLevels, System.Windows.Media.Brush zigZagColor, string lineTemplateGained, string lineTemplateLost, bool debugPrints)
		{
			return indicator.AlightenDynamicLevels(input, enableGlobalProjections, numberOfLevels, levelDisplayOption, showAllLevels, zigZagColor, lineTemplateGained, lineTemplateLost, debugPrints);
		}
	}
}

#endregion
