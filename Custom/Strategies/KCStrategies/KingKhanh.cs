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
    public class KingKhanh : KCAlgoBase
    {
        // Parameters
       	private NinjaTrader.NinjaScript.Indicators.RegressionChannel RegressionChannel1, RegressionChannel2;
		private RegressionChannelHighLow RegressionChannelHighLow1;	
		
		private bool longSignal = false;
        private bool shortSignal = false;

		public override string DisplayName { get { return Name; } }
		
        protected override void OnStateChange()
        {
            base.OnStateChange();

            if (State == State.SetDefaults)
            {
                Description = "Strategy based on the Linear Regression Channel indicator.";
                Name = "KingKhanh v4.3";
                StrategyName = "KingKhanh";
                Version = "4.3 Feb. 2025";
                Credits = "Strategy by Khanh Nguyen";
                ChartType = "Orenko 34-40-40";		

				RegChanPeriod	= 40;
				RegChanWidth	= 4;
				RegChanWidth2	= 3;
				
                InitialStop		= 89;
				ProfitTarget	= 40;
				
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
			
			longSignal = 
				// Condition group 1
				(((RegressionChannel1.Middle[1] > RegressionChannel1.Middle[2]) && (RegressionChannel1.Middle[2] <= RegressionChannel1.Middle[3]))
				
				// Condition group 2
				|| ((RegressionChannel1.Middle[0] > RegressionChannel1.Middle[1]) && (Low[0] > Low[2]) && (Low[2] <= RegressionChannel1.Lower[2]))
				
				// Condition group 3
				|| (Low[0] > RegressionChannelHighLow1.Lower[2]))
			
				&& (Close[0] > Close[1]) && (Close[0] < RegressionChannel2.Upper[0]);

            shortSignal = 
				 // Condition group 1
				 (((RegressionChannel1.Middle[1] < RegressionChannel1.Middle[2]) && (RegressionChannel1.Middle[2] >= RegressionChannel1.Middle[3]))
			
				 // Condition group 2
				 || ((RegressionChannel1.Middle[0] < RegressionChannel1.Middle[1])  && (High[0] < High[2]) && (High[2] >= RegressionChannel1.Upper[2]))
			
				 // Condition group 3
				 || (High[0] < RegressionChannelHighLow1.Upper[2]))
			
				 && (Close[0] < Close[1]) && (Close[0] > RegressionChannel2.Lower[0]); 
			
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
            RegressionChannel1 = RegressionChannel(Close, Convert.ToInt32(RegChanPeriod), RegChanWidth);			
			RegressionChannel2 = RegressionChannel(Close, Convert.ToInt32(RegChanPeriod), RegChanWidth2);
			RegressionChannelHighLow1 = RegressionChannelHighLow(Close, Convert.ToInt32(RegChanPeriod), RegChanWidth);
			AddChartIndicator(RegressionChannel1);
			AddChartIndicator(RegressionChannel2);
			AddChartIndicator(RegressionChannelHighLow1);
        }
        #endregion

        #region Properties

		[NinjaScriptProperty]
		[Display(Name="Regression Channel Period", Order=1, GroupName="08. Strategy Settings")]
		public int RegChanPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Regression Channel Width", Order=2, GroupName="08. Strategy Settings")]
		public double RegChanWidth
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Inner Regression Channel Width", Order=3, GroupName="08. Strategy Settings")]
		public double RegChanWidth2
		{ get; set; }

        #endregion

    }
}
