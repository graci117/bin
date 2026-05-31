#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.NinjaScript;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.NinjaScript.Indicators.FxStill.SmartMoney;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.KCStrategies
{
    public class SmartMoney : KCAlgoBase
    {
        // Parameters
		private NinjaTrader.NinjaScript.Indicators.FxStill.SmartMoney.MarketStructuresLite MarketStructuresLite1;
		private Brush Brush1;
		private Brush Brush2;
		private Brush Brush3;
        private int marketStructurePeriod = 10; // MarketStructuresLite period

        // State Management
        private bool longSignal = false;  // Long signal flag
        private bool shortSignal = false; // Short signal flag
		private bool longEntry = false;
        private bool shortEntry = false;
		private bool longExit = false;
		private bool shortExit = false;

		public override string DisplayName { get { return Name; } }
		
        protected override void OnStateChange()
        {
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = "Market Structure Strategy.";
                Name = "SmartMoney v4.0";
                StrategyName = "SmartMoney";
                Version = "4.0 Feb. 2025";
                Credits = "Strategy by Khanh Nguyen";
                ChartType = "TDU Renko Backtest 70-2";	
				
                marketStructurePeriod 	= 10;
				enableBos				= true;
				
                InitialStop		= 140;
				ProfitTarget	= 32;
				
                activeOrder 	= false;
            }
            else if (State == State.DataLoaded)
            {
                InitializeIndicators();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade) return;

            // Generate signals
            longSignal = IsSignalPresent("LE") && IsValidTrend(true);
            shortSignal = IsSignalPresent("SE") && IsValidTrend(false);
			
			if (enableBos)
			{
				longEntry = !longSignal && (IsSignalPresent("HL") || (IsSignalPresent("BoS") && Close[0] > Open[0] && Close[0] > Close[1] && High[0] > High[1]));
				shortEntry = !shortSignal && (IsSignalPresent("LH") || (IsSignalPresent("BoS") && Close[0] < Open[0] && Close[0] < Close[1] && Low[0] < Low[1]));
			}
			else
			{
				longEntry = !longSignal && IsSignalPresent("HL");
				shortEntry = !shortSignal && IsSignalPresent("LH");
			}
			
			// Optimized exit logic
            if (Position.MarketPosition == MarketPosition.Long)
            {
                if (IsSignalPresent("LH") || !IsValidTrend(true))
                {
                    longExit = true;
                }
            }
            else if (Position.MarketPosition == MarketPosition.Short)
            {
                if (IsSignalPresent("HL") || !IsValidTrend(false))
                {
                    shortExit = true;
                }
            }

            base.OnBarUpdate();
        }

        private bool IsSignalPresent(string signalTag)
        {
            // Check if a specific signal tag exists in the draw objects
            foreach (var drawObject in DrawObjects)
            {
                if (drawObject.Tag.ToString().Contains(signalTag))
                    return true;
            }
            return false;
        }
		
		private bool IsValidTrend(bool isLong)
        {
            // Example trend validation logic
            return isLong ? Close[0] > Open[0] : Close[0] < Open[0];
        }
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price,
                                                  int quantity, MarketPosition marketPosition, string orderId,
                                                  DateTime time)
        {
            // Reset entry flags upon exit
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                longSignal = false;
                shortSignal = false;
            }
        }
		
        protected override bool ValidateEntryLong()
        {
            // Logic for validating long entries
			if (longEntry) return true;
			else return false;
        }

        protected override bool ValidateEntryShort()
        {
            // Logic for validating short entries
			if (shortEntry) return true;
            else return false;
        }

        protected override bool ValidateExitLong()
        {
            // Logic for validating long exits
            return enableExit? longExit : false;
        }

        protected override bool ValidateExitShort()
        {
			// Logic for validating short exits
			return enableExit? shortExit : false;
        }


        #region Indicators
        protected override void InitializeIndicators()
        {
			// Add the MarketStructuresLite indicator
            MarketStructuresLite1				= MarketStructuresLite(Close, 5, true, 10, 14, Brushes.DodgerBlue, Brushes.Crimson, 2, NinjaTrader.Gui.DashStyleHelper.Solid, true, Brush1, Brush2, Brush3, 50, true, @"LE ", @"SE ", false, true);
			MarketStructuresLite1.Plots[0].Brush = Brushes.BlueViolet;
			AddChartIndicator(MarketStructuresLite1);

        }
        #endregion

        #region Properties
		[NinjaScriptProperty]
        [Display(Name = "Enable BoS Entry?", Order = 0, GroupName = "06b. Filter Settings")]
        public bool enableBos { get; set; }	
        #endregion
    }
}
