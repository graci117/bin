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
using NinjaTrader.NinjaScript.Indicators.GraciIndicators;

#endregion

public enum BankItType
{
	ALMA,
	Tillson
}

namespace NinjaTrader.NinjaScript.Indicators
{
    

	public class BankItSystem : Indicator
    {
        private TillsonT3 tillsonT1;
        private TillsonT3 tillsonT2;
        private TillsonT3 tillsonT3;
        private TillsonT3 tillsonT4;
        private TillsonT3 tillsonT5;
		private TillsonT3 tillsonT6;
		
		private ALMA alma1;
		private ALMA alma2;
		private ALMA alma3;
		private ALMA alma4;
		private ALMA alma5;
		private ALMA alma6;
		
		
		
       		
        private Zombie2PowerLine zombiePowerLine;
        private const string SystemVersion = "v1.0";
        private const string SystemName = "BankItSystem";
        private const string FullSystemName = SystemName + " - " + SystemVersion;
		private EMA emaIndicator; 
		private int savedUBar 		= 0;
		private int	savedDBar		= 0;
		private Series<double> highestValue;
		private Series<double> lowestValue;
		private int bullishCloudCount = 0;
		private int bearishCloudCount = 0;
		private int neutralCloudCount = 0;

        public override string DisplayName
        {
            get { return FullSystemName; }
        }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"BankItSystem with 5 Tillson T3 indicators and Zombie2PowerLine";
                Name = "BankItSystem";
                Calculate = Calculate.OnPriceChange;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
				this.MaximumBarsLookBack = MaximumBarsLookBack.Infinite;
				this.ShowTransparentPlotsInDataBox = true;
				
				this.RibbonMAType = BankItType.ALMA;
                // Default T3 settings
                T1Length = 6;
                T1VolumeFactor = 0.1;
                T2Length = 8;
                T2VolumeFactor = 0.5;
                T3Length = 6;
                T3VolumeFactor = 0.35;
                T4Length = 5;
                T4VolumeFactor = 0.3;
                T5Length = 4;
                T5VolumeFactor = 0.3;
				T6Length = 5;
                T6VolumeFactor = 0.35;
				
				AWindowSize1 = 21;
				ASigma1 = 6;
				ASample1 = 0.95;
				
				AWindowSize2 = 31;
				ASigma2 = 6.35;
				ASample2 = 0.95;
				
				AWindowSize3 = 34;
				ASigma3 = 6;
				ASample3 = 0.95;
				
				
				AWindowSize4 = 37;
				ASigma4 = 6.35;
				ASample4 = 0.95;
				
				
				AWindowSize5 = 42;
				ASigma5 = 6.35;
				ASample5 = 0.95;
				
				
				AWindowSize6 = 55;
				ASigma6 = 6.35;
				ASample6 = 0.95;
				
				
                // Zombie2PowerLine settings
                ZombiePeriod = 42;
				EMALength = 100;
				this.RegionOpacity = 10;
				
				

                // Add plots for Tillson T3 indicators
                AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Line, "MA1");
                AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Line, "MA2");
                AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Line, "MA3");
                AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Line, "MA4");
                AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Line, "MA5");
				AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Line, "MA6");
                
                // Add plots for Zombie2PowerLine
                AddPlot(new Stroke(Brushes.Gray, 5), PlotStyle.Line, "ZombieMeanChange");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "ZombieUpper");
                AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "ZombieLower");
				
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "CloudBullish");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "CloudBearish");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "CloudNeutral");
				
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "Signal_Trade");
				
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "HighestValue");
				AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "LowestestValue");
				
				
				
				 //AddPlot(new Stroke(Brushes.Transparent, 1), PlotStyle.Line, "EMA");
            }
            else if (State == State.Configure)
            {
				
                // Initialize Tillson T3 indicators
                
                
                // Initialize Zombie2PowerLine
               zombiePowerLine = Zombie2PowerLine(FullSystemName, ZombiePeriod);
				highestValue = new Series<double>(this, MaximumBarsLookBack.Infinite);
				lowestValue = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				emaIndicator = EMA(Close, EMALength);
            }
			else if (State == State.DataLoaded)
			{
				if (RibbonMAType == BankItType.ALMA)
				{
					alma1 = ALMA(AWindowSize1, ASigma1, ASample1);
						alma2 = ALMA(AWindowSize2, ASigma2, ASample2);
						alma3 = ALMA(AWindowSize3, ASigma3, ASample3);
						alma4 = ALMA(AWindowSize4, ASigma4, ASample4);
						alma5 = ALMA(AWindowSize5, ASigma5, ASample5);
						alma6 = ALMA(AWindowSize6, ASigma6, ASample6);
				}
				else 
				{
					tillsonT1 = TillsonT3(T1Length, T1VolumeFactor);
		                tillsonT2 = TillsonT3(T2Length, T2VolumeFactor);
		                tillsonT3 = TillsonT3(T3Length, T3VolumeFactor);
		                tillsonT4 = TillsonT3(T4Length, T4VolumeFactor);
		                tillsonT5 = TillsonT3(T5Length, T5VolumeFactor);
						tillsonT6 = TillsonT3(T6Length, T6VolumeFactor);
				}
				
			
				 
			}
        }

        protected override void OnBarUpdate()
        {
			
				
			
            // Make sure we have enough bars before proceeding
            if (CurrentBar < this.BarsRequiredToPlot)
                return;
			
			
			
			bullishCloudCount = 0;
			bearishCloudCount = 0;
			neutralCloudCount = 0;
			
			
			
			
			if (RibbonMAType == BankItType.ALMA)
			{
				 Values[0][0] = alma1.Value[0];
			            Values[1][0] = alma2.Value[0];
			            Values[2][0] = alma3.Value[0];
			            Values[3][0] = alma4.Value[0];
			            Values[4][0] = alma5.Value[0];
						Values[5][0] = alma6.Value[0];
						//return;
			}			
			else 
			{
				 		Values[0][0] = tillsonT1.Value[0];
			            Values[1][0] = tillsonT2.Value[0];
			            Values[2][0] = tillsonT3.Value[0];
			            Values[3][0] = tillsonT4.Value[0];
			            Values[4][0] = tillsonT5.Value[0];
						Values[5][0] = tillsonT6.Value[0];
			}
			
              
           
            //Print("Zombie    " + zombiePowerLine.MeanChange[0]);
				
            // Set values for Zombie2PowerLine plots
            Values[6][0] = zombiePowerLine.MeanChange[0];
            Values[7][0] = zombiePowerLine.Upper[0];
            Values[8][0] = zombiePowerLine.Lower[0];
            
            // Optional: Color changes for Tillson T3 lines based on direction
            if (CurrentBar >= 1)
            {
				//Print(RibbonMAType);
                // T1 color change
                if (Values[0][0] > Values[0][1])
				{
					
                    PlotBrushes[0][0] = Brushes.LimeGreen;
					bullishCloudCount += 1;
					//Print ("BulllishCloudCount0   " + bullishCloudCount + "-----" + Time[0]);
				}
                else if (Values[0][0] < Values[0][1])
                 {
                    PlotBrushes[0][0] = Brushes.Red;
					bearishCloudCount += 1;
					 //Print ("bearishCloudCount0   " + bearishCloudCount + "-----" + Time[0]);
				}
                else
				{
                    PlotBrushes[0][0] = Brushes.Gray;
					neutralCloudCount += 1;
				}
                
                // T2 color change
                if (Values[1][0] > Values[1][1])
                {
                    PlotBrushes[1][0] = Brushes.LimeGreen;
					bullishCloudCount += 1;
					//Print ("BulllishCloudCount1   " + bullishCloudCount + "-----" + Time[0]);
				}
                else if (Values[1][0] < Values[1][1])
				{
                    PlotBrushes[1][0] = Brushes.Red;
					bearishCloudCount += 1;
					//Print ("bearishCloudCount1   " + bearishCloudCount + "-----" + Time[0]);
				}
                else
				{
                    PlotBrushes[1][0] = Brushes.Gray;
					neutralCloudCount += 1;
				}
                
                // T3 color change
                if (Values[2][0] > Values[2][1])
                {
                    PlotBrushes[2][0] = Brushes.LimeGreen;
					bullishCloudCount += 1;
					//Print ("BulllishCloudCount2   " + bullishCloudCount + "-----" + Time[0]);
				}
                else if (Values[2][0] < Values[2][1])
                 {
                    PlotBrushes[2][0] = Brushes.Red;
					bearishCloudCount += 1;
					 //Print ("bearishCloudCount2   " + bearishCloudCount + "-----" + Time[0]);
				}
                else
				{
                    PlotBrushes[2][0] = Brushes.Gray;
					neutralCloudCount += 1;
				}
                
                // T4 color change
                if (Values[3][0] > Values[3][1])
				{
                    PlotBrushes[3][0] = Brushes.LimeGreen;
					bullishCloudCount += 1;
					//Print ("BulllishCloudCount3   " + bullishCloudCount + "-----" + Time[0]);
				}
                else if (Values[3][0] < Values[3][1])
                {
                    PlotBrushes[3][0] = Brushes.Red;
					bearishCloudCount += 1;
					//Print ("bearishCloudCount3   " + bearishCloudCount + "-----" + Time[0]);
				}
                else
				{
                    PlotBrushes[3][0] = Brushes.Gray;
					neutralCloudCount += 1;
				}
                
                // T5 color change
                if (Values[4][0] > Values[4][1])
                {
                    PlotBrushes[4][0] = Brushes.LimeGreen;
					bullishCloudCount += 1;
					//Print ("BulllishCloudCount4   " + bullishCloudCount + "-----" + Time[0]);
				}
                else if (Values[4][0] < Values[4][1])
                {
                    PlotBrushes[4][0] = Brushes.Red;
					bearishCloudCount += 1;
					//Print ("bearishCloudCount4   " + bearishCloudCount + "-----" + Time[0]);
				}
                else
				{
                    PlotBrushes[4][0] = Brushes.Gray;
					neutralCloudCount += 1;
				}
				
				if (Values[5][0] > Values[5][1])
                {
					
                    PlotBrushes[5][0] = Brushes.LimeGreen;
					bullishCloudCount += 1;
					//Print ("Bulllish Values[5][0]   " + Values[5][0]+ "-----" + Time[0]);
					//Print ("Bulllish Values[5][1]   " + Values[5][1]+ "-----" + Time[0]);
					//Print ("BulllishCloudCount5   " + bullishCloudCount + "-----" + Time[0]);
				}
                else if (Values[5][0] < Values[5][1])
                {
                    PlotBrushes[5][0] = Brushes.Red;
					bearishCloudCount += 1;
					//Print ("Bearish Values[5][0]   " + Values[5][0]+ "-----" + Time[0]);
					//Print ("Bearish Values[5][1]   " + Values[5][1]+ "-----" + Time[0]);
					//Print ("bearishCloudCount5   " + bearishCloudCount + "-----" + Time[0]);
				}
                else
				{
                    PlotBrushes[5][0] = Brushes.Gray;
					neutralCloudCount += 1;
				}
				
				
			
				
				
				  // Zombie MeanChange color based on price position
               if (Close[0] >= zombiePowerLine.MeanChange[0])
			   {
                    PlotBrushes[6][0] = Brushes.ForestGreen;
				   	Values[7][0] = 1;
			   }
                else
			   {
                   PlotBrushes[6][0] = Brushes.Maroon;
				   Values[8][0] = 0;
			   }
				
				//cloud bullish or bearish
				if (bullishCloudCount == 6)
				{
					Values[9][0] = 1;
					//Print ("Bullish cloud Values9   " + Values[9][0]  + "-----" + Time[0]);
				}
				else if (bearishCloudCount == 6)
				{
					Values[10][0] = 1;
					//Print ("Bearish cloud Values10   " + Values[10][0]  + "-----" + Time[0]);
				}
				else
					Values[11][0] = 1;
				
				 Values[13][0] = Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Values[0][0], Values[1][0]), Values[2][0]), Values[3][0]), Values[4][0]), Values[5][0]);
	             Values[14][0] = Math.Min(Math.Min(Math.Min(Math.Min(Math.Min(Values[0][0], Values[1][0]), Values[2][0]), Values[3][0]), Values[4][0]), Values[5][0]);
				//Print(Time[0] + " Low[0]: " + Low[0]);
//Print(Time[0] + "bs BankItSystem1.HighestValue[0]: " + HighestValue[0]);
			    bool emaRising = emaIndicator[0] >= emaIndicator[1];
				bool prevRising = emaIndicator[1] >= emaIndicator[2];;
				
				if ( (!prevRising && emaRising) && CurrentBar != savedUBar)
				{
					savedUBar = CurrentBar;  	
				}
				
				if ((prevRising && !emaRising) && CurrentBar != savedDBar)
				{
					savedDBar = CurrentBar;  	
				}
			//savedUBar = CurrentBar;  
				
					
				//RemoveDrawObject("Dwn" + (CurrentBar-1));
	           // RemoveDrawObject("BearishRegion" + (CurrentBar-1));
					
				SolidColorBrush bullishBrush = new SolidColorBrush(Colors.LimeGreen) { Opacity = RegionOpacity / 100.0 };
	            SolidColorBrush bearishBrush = new SolidColorBrush(Colors.Red) { Opacity = RegionOpacity / 100.0 };
				
				//Draw.Region(this, "Dwn" + CurrentBar, CurrentBar, 0, lowestValue, highestValue,  Brushes.Transparent, bearishBrush, RegionOpacity, 0);
	
				
				// Check conditions and draw regions
	            if (zombiePowerLine.MeanChange[0] < lowestValue[0] && emaRising)
	            {
	                // Zombie below lowest Tillson and EMA rising
	                // Color region between highest Tillson and Zombie with LimeGreen
					Draw.Region(this, "Up" + savedUBar, CurrentBar - savedUBar , 0, highestValue, zombiePowerLine.MeanChange,  Brushes.Transparent, bullishBrush, RegionOpacity, 0);
					Values[12][0]  = 1;
	            }
	            else if (zombiePowerLine.MeanChange[0] > highestValue[0] && emaRising)
	            {
	                // Zombie above highest Tillson and EMA rising
	                // Color region between lowest Tillson and Zombie with LimeGreen
					Draw.Region(this, "Up" + savedUBar, CurrentBar - savedUBar, 0, lowestValue, zombiePowerLine.MeanChange,  Brushes.Transparent, bullishBrush, RegionOpacity, 0);
					//Values[12][0]  = 1;
	            }
	            else if (zombiePowerLine.MeanChange[0] > highestValue[0] && !emaRising)
	            {
	                // Zombie above highest Tillson and EMA falling
	                // Color region between lowest Tillson and Zombie with Red
					Draw.Region(this, "Dwn" + savedDBar, CurrentBar - savedDBar, 0, zombiePowerLine.MeanChange, lowestValue, Brushes.Transparent, bearishBrush, RegionOpacity, 0);
					//Values[12][0]  = -1;
	            }
	            else if (zombiePowerLine.MeanChange[0] < lowestValue[0] && !emaRising)
	            {
	                // Zombie below lowest Tillson and EMA falling
	                // Color region between highest Tillson and Zombie with Red
	                Draw.Region(this, "Dwn" + savedUBar, CurrentBar - savedDBar, 0, lowestValue, zombiePowerLine.MeanChange,  Brushes.Transparent, bearishBrush, RegionOpacity, 0);
					//Values[12][0] = -1;
	            }
				
				
				// Check conditions and draw regions
	            if ((zombiePowerLine.MeanChange[0] < lowestValue[0]|| Close[0] > zombiePowerLine.MeanChange[0]) && CloudBullish[0]==1)
				//if(CloudBullish[0]==1)
	            {
	                // Zombie below lowest Tillson and EMA rising
	                // Color region between highest Tillson and Zombie with LimeGreen
					//Draw.Text(this, Convert.ToString("Long") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + " ", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
					Values[12][0]  = 1;
	            }	            
	            else if ((zombiePowerLine.MeanChange[0] > highestValue[0] || Close[0] < zombiePowerLine.MeanChange[0])&& CloudBearish[0]==1)
				//else if (CloudBearish[0]==1)
	            {
	                // Zombie above highest Tillson and EMA falling
	                // Color region between lowest Tillson and Zombie with Red
					//Draw.Text(this, Convert.ToString("Short") + Convert.ToString(CurrentBars[0]), " " + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );
					Values[12][0]  = -1;
	            }	           
				else
				{
					Values[12][0] = 0;
				}
				
				
				
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Display(Name = "IndicatorName", GroupName = "0) Indicator Information", Order = 0)]
        public string IndicatorName
        {
            get { return FullSystemName; }
            set { }
        }
        
		// Tillson T3 Properties
        [NinjaScriptProperty]
        [Display(Name="Ribbon MA Type", Order=1, GroupName="Ribbon Moving Average Type")]
        public BankItType RibbonMAType { get; set; }
		
        // Tillson T3 Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T1 Length", Order=1, GroupName="Tillson T3 Parameters")]
        public int T1Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T1 Volume Factor", Order=2, GroupName="Tillson T3 Parameters")]
        public double T1VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T2 Length", Order=3, GroupName="Tillson T3 Parameters")]
        public int T2Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T2 Volume Factor", Order=4, GroupName="Tillson T3 Parameters")]
        public double T2VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T3 Length", Order=5, GroupName="Tillson T3 Parameters")]
        public int T3Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T3 Volume Factor", Order=6, GroupName="Tillson T3 Parameters")]
        public double T3VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T4 Length", Order=7, GroupName="Tillson T3 Parameters")]
        public int T4Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T4 Volume Factor", Order=8, GroupName="Tillson T3 Parameters")]
        public double T4VolumeFactor { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T5 Length", Order=9, GroupName="Tillson T3 Parameters")]
        public int T5Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T5 Volume Factor", Order=10, GroupName="Tillson T3 Parameters")]
        public double T5VolumeFactor { get; set; }
		
		 [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="T6 Length", Order=11, GroupName="Tillson T3 Parameters")]
        public int T6Length { get; set; }

        [NinjaScriptProperty]
        [Range(0.1, 1.0)]
        [Display(Name="T6 Volume Factor", Order=12, GroupName="Tillson T3 Parameters")]
        public double T6VolumeFactor { get; set; }
		
		/// <summary>
		/// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// </summary>
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="ALMA WindowSize 1", Order=1, GroupName="ALMA Parameters")]
        public int AWindowSize1 { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, 20.0)]
        [Display(Name="ALMA Sigma 1", Order=2, GroupName="ALMA Parameters")]
        public double ASigma1 { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="ALMA Sample 1", Order=3, GroupName="ALMA Parameters")]
        public double ASample1 { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="ALMA WindowSize 2", Order=4, GroupName="ALMA Parameters")]
        public int AWindowSize2 { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, 20.0)]
        [Display(Name="ALMA Sigma 2", Order=5, GroupName="ALMA Parameters")]
        public double ASigma2 { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="ALMA Sample 2", Order=6, GroupName="ALMA Parameters")]
        public double ASample2 { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="ALMA WindowSize 3", Order=7, GroupName="ALMA Parameters")]
        public int AWindowSize3 { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, 20.0)]
        [Display(Name="ALMA Sigma 3", Order=8, GroupName="ALMA Parameters")]
        public double ASigma3 { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="ALMA Sample 3", Order=9, GroupName="ALMA Parameters")]
        public double ASample3 { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="ALMA WindowSize 4", Order=10, GroupName="ALMA Parameters")]
        public int AWindowSize4 { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, 20.0)]
        [Display(Name="ALMA Sigma 4", Order=11, GroupName="ALMA Parameters")]
        public double ASigma4 { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="ALMA Sample 4", Order=12, GroupName="ALMA Parameters")]
        public double ASample4 { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="ALMA WindowSize 5", Order=13, GroupName="ALMA Parameters")]
        public int AWindowSize5 { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, 20.0)]
        [Display(Name="ALMA Sigma 5", Order=14, GroupName="ALMA Parameters")]
        public double ASigma5 { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="ALMA Sample 5", Order=15, GroupName="ALMA Parameters")]
        public double ASample5{ get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name="ALMA WindowSize 6", Order=16, GroupName="ALMA Parameters")]
        public int AWindowSize6 { get; set; }

        [NinjaScriptProperty]
        [Range(0.01, 20.0)]
        [Display(Name="ALMA Sigma 6", Order=17, GroupName="ALMA Parameters")]
        public double ASigma6 { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name="ALMA Sample 6", Order=18, GroupName="ALMA Parameters")]
        public double ASample6 { get; set; }
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// 
		
		
        
        // Zombie2PowerLine Properties
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "Zombie Period", GroupName = "Zombie2PowerLine Parameters", Order = 1)]
        public int ZombiePeriod { get; set; }
        
        // Value Series Properties
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MA1Values
        {
            get { return Values[0]; }
        }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MA2Values
        {
            get { return Values[1]; }
        }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MA3Values
        {
            get { return Values[2]; }
        }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MA4Values
        {
            get { return Values[3]; }
        }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> MA5Values
        {
            get { return Values[4]; }
        }
        
		[Browsable(false)]
        [XmlIgnore()]
        public Series<double> MA6Values
        {
            get { return Values[5]; }
        }
		
		
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> ZombieMeanChangeValues
        {
            get { return Values[6]; }
        }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> ZombieUpperValues
        {
            get { return Values[7]; }
        }
        
        [Browsable(false)]
        [XmlIgnore()]
        public Series<double> ZombieLowerValues
        {
            get { return Values[8]; }
        }
		
		// EMA Properties
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(Name = "EMA Length", GroupName = "EMA Parameters", Order = 1)]
        public int EMALength { get; set; }
		
		[Range(1, 100), NinjaScriptProperty]
        [Display(Name = "Region Opacity (%)", GroupName = "Region Parameters", Order = 1)]
        public int RegionOpacity { get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CloudBullish
		{
			get { return Values[9]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CloudBearish
		{
			get { return Values[10]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> CloudNeutral
		{
			get { return Values[11]; }
		}
		
				
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Signal_Trade
		{
			get { return Values[12]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> HighestValue
		{
			get { return Values[13]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> LowestValue
		{
			get { return Values[14]; }
		}
		
		
		
		
		
		
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BankItSystem[] cacheBankItSystem;
		public BankItSystem BankItSystem(string indicatorName, BankItType ribbonMAType, int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor, int t6Length, double t6VolumeFactor, int aWindowSize1, double aSigma1, double aSample1, int aWindowSize2, double aSigma2, double aSample2, int aWindowSize3, double aSigma3, double aSample3, int aWindowSize4, double aSigma4, double aSample4, int aWindowSize5, double aSigma5, double aSample5, int aWindowSize6, double aSigma6, double aSample6, int zombiePeriod, int eMALength, int regionOpacity)
		{
			return BankItSystem(Input, indicatorName, ribbonMAType, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor, t6Length, t6VolumeFactor, aWindowSize1, aSigma1, aSample1, aWindowSize2, aSigma2, aSample2, aWindowSize3, aSigma3, aSample3, aWindowSize4, aSigma4, aSample4, aWindowSize5, aSigma5, aSample5, aWindowSize6, aSigma6, aSample6, zombiePeriod, eMALength, regionOpacity);
		}

		public BankItSystem BankItSystem(ISeries<double> input, string indicatorName, BankItType ribbonMAType, int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor, int t6Length, double t6VolumeFactor, int aWindowSize1, double aSigma1, double aSample1, int aWindowSize2, double aSigma2, double aSample2, int aWindowSize3, double aSigma3, double aSample3, int aWindowSize4, double aSigma4, double aSample4, int aWindowSize5, double aSigma5, double aSample5, int aWindowSize6, double aSigma6, double aSample6, int zombiePeriod, int eMALength, int regionOpacity)
		{
			if (cacheBankItSystem != null)
				for (int idx = 0; idx < cacheBankItSystem.Length; idx++)
					if (cacheBankItSystem[idx] != null && cacheBankItSystem[idx].IndicatorName == indicatorName && cacheBankItSystem[idx].RibbonMAType == ribbonMAType && cacheBankItSystem[idx].T1Length == t1Length && cacheBankItSystem[idx].T1VolumeFactor == t1VolumeFactor && cacheBankItSystem[idx].T2Length == t2Length && cacheBankItSystem[idx].T2VolumeFactor == t2VolumeFactor && cacheBankItSystem[idx].T3Length == t3Length && cacheBankItSystem[idx].T3VolumeFactor == t3VolumeFactor && cacheBankItSystem[idx].T4Length == t4Length && cacheBankItSystem[idx].T4VolumeFactor == t4VolumeFactor && cacheBankItSystem[idx].T5Length == t5Length && cacheBankItSystem[idx].T5VolumeFactor == t5VolumeFactor && cacheBankItSystem[idx].T6Length == t6Length && cacheBankItSystem[idx].T6VolumeFactor == t6VolumeFactor && cacheBankItSystem[idx].AWindowSize1 == aWindowSize1 && cacheBankItSystem[idx].ASigma1 == aSigma1 && cacheBankItSystem[idx].ASample1 == aSample1 && cacheBankItSystem[idx].AWindowSize2 == aWindowSize2 && cacheBankItSystem[idx].ASigma2 == aSigma2 && cacheBankItSystem[idx].ASample2 == aSample2 && cacheBankItSystem[idx].AWindowSize3 == aWindowSize3 && cacheBankItSystem[idx].ASigma3 == aSigma3 && cacheBankItSystem[idx].ASample3 == aSample3 && cacheBankItSystem[idx].AWindowSize4 == aWindowSize4 && cacheBankItSystem[idx].ASigma4 == aSigma4 && cacheBankItSystem[idx].ASample4 == aSample4 && cacheBankItSystem[idx].AWindowSize5 == aWindowSize5 && cacheBankItSystem[idx].ASigma5 == aSigma5 && cacheBankItSystem[idx].ASample5 == aSample5 && cacheBankItSystem[idx].AWindowSize6 == aWindowSize6 && cacheBankItSystem[idx].ASigma6 == aSigma6 && cacheBankItSystem[idx].ASample6 == aSample6 && cacheBankItSystem[idx].ZombiePeriod == zombiePeriod && cacheBankItSystem[idx].EMALength == eMALength && cacheBankItSystem[idx].RegionOpacity == regionOpacity && cacheBankItSystem[idx].EqualsInput(input))
						return cacheBankItSystem[idx];
			return CacheIndicator<BankItSystem>(new BankItSystem(){ IndicatorName = indicatorName, RibbonMAType = ribbonMAType, T1Length = t1Length, T1VolumeFactor = t1VolumeFactor, T2Length = t2Length, T2VolumeFactor = t2VolumeFactor, T3Length = t3Length, T3VolumeFactor = t3VolumeFactor, T4Length = t4Length, T4VolumeFactor = t4VolumeFactor, T5Length = t5Length, T5VolumeFactor = t5VolumeFactor, T6Length = t6Length, T6VolumeFactor = t6VolumeFactor, AWindowSize1 = aWindowSize1, ASigma1 = aSigma1, ASample1 = aSample1, AWindowSize2 = aWindowSize2, ASigma2 = aSigma2, ASample2 = aSample2, AWindowSize3 = aWindowSize3, ASigma3 = aSigma3, ASample3 = aSample3, AWindowSize4 = aWindowSize4, ASigma4 = aSigma4, ASample4 = aSample4, AWindowSize5 = aWindowSize5, ASigma5 = aSigma5, ASample5 = aSample5, AWindowSize6 = aWindowSize6, ASigma6 = aSigma6, ASample6 = aSample6, ZombiePeriod = zombiePeriod, EMALength = eMALength, RegionOpacity = regionOpacity }, input, ref cacheBankItSystem);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BankItSystem BankItSystem(string indicatorName, BankItType ribbonMAType, int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor, int t6Length, double t6VolumeFactor, int aWindowSize1, double aSigma1, double aSample1, int aWindowSize2, double aSigma2, double aSample2, int aWindowSize3, double aSigma3, double aSample3, int aWindowSize4, double aSigma4, double aSample4, int aWindowSize5, double aSigma5, double aSample5, int aWindowSize6, double aSigma6, double aSample6, int zombiePeriod, int eMALength, int regionOpacity)
		{
			return indicator.BankItSystem(Input, indicatorName, ribbonMAType, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor, t6Length, t6VolumeFactor, aWindowSize1, aSigma1, aSample1, aWindowSize2, aSigma2, aSample2, aWindowSize3, aSigma3, aSample3, aWindowSize4, aSigma4, aSample4, aWindowSize5, aSigma5, aSample5, aWindowSize6, aSigma6, aSample6, zombiePeriod, eMALength, regionOpacity);
		}

		public Indicators.BankItSystem BankItSystem(ISeries<double> input , string indicatorName, BankItType ribbonMAType, int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor, int t6Length, double t6VolumeFactor, int aWindowSize1, double aSigma1, double aSample1, int aWindowSize2, double aSigma2, double aSample2, int aWindowSize3, double aSigma3, double aSample3, int aWindowSize4, double aSigma4, double aSample4, int aWindowSize5, double aSigma5, double aSample5, int aWindowSize6, double aSigma6, double aSample6, int zombiePeriod, int eMALength, int regionOpacity)
		{
			return indicator.BankItSystem(input, indicatorName, ribbonMAType, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor, t6Length, t6VolumeFactor, aWindowSize1, aSigma1, aSample1, aWindowSize2, aSigma2, aSample2, aWindowSize3, aSigma3, aSample3, aWindowSize4, aSigma4, aSample4, aWindowSize5, aSigma5, aSample5, aWindowSize6, aSigma6, aSample6, zombiePeriod, eMALength, regionOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BankItSystem BankItSystem(string indicatorName, BankItType ribbonMAType, int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor, int t6Length, double t6VolumeFactor, int aWindowSize1, double aSigma1, double aSample1, int aWindowSize2, double aSigma2, double aSample2, int aWindowSize3, double aSigma3, double aSample3, int aWindowSize4, double aSigma4, double aSample4, int aWindowSize5, double aSigma5, double aSample5, int aWindowSize6, double aSigma6, double aSample6, int zombiePeriod, int eMALength, int regionOpacity)
		{
			return indicator.BankItSystem(Input, indicatorName, ribbonMAType, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor, t6Length, t6VolumeFactor, aWindowSize1, aSigma1, aSample1, aWindowSize2, aSigma2, aSample2, aWindowSize3, aSigma3, aSample3, aWindowSize4, aSigma4, aSample4, aWindowSize5, aSigma5, aSample5, aWindowSize6, aSigma6, aSample6, zombiePeriod, eMALength, regionOpacity);
		}

		public Indicators.BankItSystem BankItSystem(ISeries<double> input , string indicatorName, BankItType ribbonMAType, int t1Length, double t1VolumeFactor, int t2Length, double t2VolumeFactor, int t3Length, double t3VolumeFactor, int t4Length, double t4VolumeFactor, int t5Length, double t5VolumeFactor, int t6Length, double t6VolumeFactor, int aWindowSize1, double aSigma1, double aSample1, int aWindowSize2, double aSigma2, double aSample2, int aWindowSize3, double aSigma3, double aSample3, int aWindowSize4, double aSigma4, double aSample4, int aWindowSize5, double aSigma5, double aSample5, int aWindowSize6, double aSigma6, double aSample6, int zombiePeriod, int eMALength, int regionOpacity)
		{
			return indicator.BankItSystem(input, indicatorName, ribbonMAType, t1Length, t1VolumeFactor, t2Length, t2VolumeFactor, t3Length, t3VolumeFactor, t4Length, t4VolumeFactor, t5Length, t5VolumeFactor, t6Length, t6VolumeFactor, aWindowSize1, aSigma1, aSample1, aWindowSize2, aSigma2, aSample2, aWindowSize3, aSigma3, aSample3, aWindowSize4, aSigma4, aSample4, aWindowSize5, aSigma5, aSample5, aWindowSize6, aSigma6, aSample6, zombiePeriod, eMALength, regionOpacity);
		}
	}
}

#endregion
