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
#endregion

namespace NinjaTrader.NinjaScript.Strategies.KCStrategies
{
    public class Rosie : KCAlgoBase
    {
        // Parameters
        private RSI RSI1;
		private bool longSignal = false;
        private bool shortSignal = false;

		public override string DisplayName { get { return Name; } }
		
        protected override void OnStateChange()
        {
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = "Strategy based on the RSI indicator.";
                Name = "Rosie v4.3";
                StrategyName = "Rosie";
                Version = "4.3 Feb. 2025";
                Credits = "Strategy by B&B Trader and Khanh Nguyen";
                ChartType = "Range 15";	

                // Filter Settings
                Overbought 		= 70;
                Oversold 		= 30;
				showRSI			= true;
				
                InitialStop		= 32;
				ProfitTarget	= 12;
				
                activeOrder 	= false;
            }
            else if (State == State.DataLoaded)
            {
                InitializeIndicators();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBars[0] < BarsRequiredToTrade)
                return;

            // Generate signals
            longSignal = (RSI1.Default[0] < Oversold) || (RSI1.Default[0] > Overbought);
            shortSignal = (RSI1.Default[0] > Overbought) || (RSI1.Default[0] < Oversold);
			
            base.OnBarUpdate();
        }

        protected override bool ValidateEntryLong()
        {
            // Logic for validating long entries
			if (longSignal) return true;
			else return false;
        }

        protected override bool ValidateEntryShort()
        {
            // Logic for validating short entries
			if (shortSignal) return true;
            else return false;
        }

       	protected override bool ValidateExitLong()
        {
            // Logic for validating long exits
            return enableExit? true : false;
        }

        protected override bool ValidateExitShort()
        {
			// Logic for validating short exits
			return enableExit? true : false;
        }

        #region Indicators
        protected override void InitializeIndicators()
        {
            // Initialize the RSI2 indicator
            RSI1 = RSI(Close, 14, 3);
            RSI1.Plots[0].Brush = Brushes.Lime;
            RSI1.Plots[1].Brush = Brushes.Red;
            if (showRSI) AddChartIndicator(RSI1);
        }
        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Display(Name = "Overbought", Order = 1, GroupName = "08. Strategy Settings")]
        public int Overbought { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Oversold", Order = 2, GroupName = "08. Strategy Settings")]
        public int Oversold { get; set; }

		[NinjaScriptProperty]
        [Display(Name = "Show RSI", Order = 1, GroupName = "06b. Filter Settings")]
        public bool showRSI { get; set; }
		
        #endregion
    }
}
