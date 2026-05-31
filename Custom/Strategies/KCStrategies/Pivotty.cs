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
#endregion

namespace NinjaTrader.NinjaScript.Strategies.KCStrategies
{
    public class Pivotty : KCAlgoBase
    {
        // Parameters
        private int pivotStrength = 5;
        private int barsRequiredToTrade = 20;
        private double trailingStopDistance;

        // Indicators
        private NTSvePivots pivots;
        private double pivotPoint, s1, s2, s3, r1, r2, r3, s1m, s2m, s3m, r1m, r2m, r3m;

		public override string DisplayName { get { return Name; } }

        protected override void OnStateChange()
        {
            base.OnStateChange();
            if (State == State.SetDefaults)
            {
                Description 		= @"Strategy based on NTSvePivots indicator.";
                Name 				= "Pivotty v4.0";
                StrategyName 		= "Pivotty";
                Version 			= "4.0 Feb. 2025";
				ChartType	= "Ninzarenko 70-2";
				
				EnableDynamicProfit	= true;
                pivotLength 		= 250;
				
				InitialStop			= 120;
				ProfitTarget		= 120;
            }
            else if (State == State.DataLoaded)
            {
                InitializeIndicators();
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < barsRequiredToTrade || BarsInProgress != 0)
                return;

            // Get pivot points and support/resistance levels
            pivotPoint = pivots.Pp[0];
            s1 = pivots.S1[0];
            s2 = pivots.S2[0];
            s3 = pivots.S3[0];
            r1 = pivots.R1[0];
            r2 = pivots.R2[0];
            r3 = pivots.R3[0];
            s1m = pivots.S1M[0];
            s2m = pivots.S2M[0];
			s3m = pivots.S3M[0];
            r1m = pivots.R1M[0];
            r2m = pivots.R2M[0];
			r3m = pivots.R3M[0];

            base.OnBarUpdate();
        }

        protected override bool ValidateEntryLong()
        {
            bool[] longConditions = {
                Close[0] > s3 && Low[0] <= s3,
				Close[0] > s3m && Low[0] <= s3m,
                Close[0] > s2 && Low[0] <= s2,
                Close[0] > s2m && Low[0] <= s2m,
                Close[0] > s1 && Low[0] <= s1,
                Close[0] > s1m && Low[0] <= s1m,
                Close[0] > pivotPoint && Low[0] <= pivotPoint,
                Close[0] > r1m && Low[0] <= r1m,
                Close[0] > r1 && Low[0] <= r1,
                Close[0] > r2m && Low[0] <= r2m,
                Close[0] > r2 && Low[0] <= r2,
//				Close[0] > r3m && Low[0] <= r3m
            };

            bool isValidEntry = longConditions.Any(condition => condition);
            if (isValidEntry)
            {
                SetProfitTargetBasedOnLongConditions();
            }

            return isValidEntry;
        }

        protected override bool ValidateEntryShort()
        {
            bool[] shortConditions = {
                Close[0] < r3 && High[0] >= r3,				
				Close[0] < r3m && High[0] >= r3m,
                Close[0] < r2 && High[0] >= r2,
                Close[0] < r2m && High[0] >= r2m,
                Close[0] < r1 && High[0] >= r1,
                Close[0] < r1m && High[0] >= r1m,
                Close[0] < pivotPoint && High[0] >= pivotPoint,
                Close[0] < s1m && High[0] >= s1m,
                Close[0] < s1 && High[0] >= s1,
                Close[0] < s2m && High[0] >= s2m,
                Close[0] < s2 && High[0] >= s2,
//				Close[0] < s3m && High[0] >= s3m
            };

            bool isValidEntry = shortConditions.Any(condition => condition);
            if (isValidEntry)
            {
                SetProfitTargetBasedOnShortConditions();
            }

            return isValidEntry;
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

        private void SetProfitTargetBasedOnLongConditions()
        {
            if (Close[0] > s3 && Low[0] <= s3)
				SetProfitTarget("LE", CalculationMode.Price, s3m);
			else if (Close[0] > s3m && Low[0] <= s3m)
				SetProfitTarget("LE", CalculationMode.Price, s2);
			else if (Close[0] > s2 && Low[0] <= s2)
				SetProfitTarget("LE", CalculationMode.Price, s2m);
			else if (Close[0] > s2m && Low[0] <= s2m)	
				SetProfitTarget("LE", CalculationMode.Price, s1);
			else if (Close[0] > s1 && Low[0] <= s1)
				SetProfitTarget("LE", CalculationMode.Price, s1m);
			else if (Close[0] > s1m && Low[0] <= s1m)
				SetProfitTarget("LE", CalculationMode.Price, pivotPoint);
			else if (Close[0] > pivotPoint && Low[0] <= pivotPoint)
				SetProfitTarget("LE", CalculationMode.Price, r1m);
			else if (Close[0] > r1m && Low[0] <= r1m)
				SetProfitTarget("LE", CalculationMode.Price, r1);
			else if (Close[0] > r1 && Low[0] <= r1)
				SetProfitTarget("LE", CalculationMode.Price, r2m);
			else if (Close[0] > r2m && Low[0] <= r2m)
				SetProfitTarget("LE", CalculationMode.Price, r2);
			else if (Close[0] > r2 && Low[0] <= r2)
				SetProfitTarget("LE", CalculationMode.Price, r3m);
			else if (Close[0] > r3m && Low[0] <= r3m)
				SetProfitTarget("LE", CalculationMode.Price, r3);
			else if (Close[0] > r3 && Low[0] <= r3)
				SetProfitTarget(@"LE", CalculationMode.Ticks, ProfitTarget);
        }

        private void SetProfitTargetBasedOnShortConditions()
        {
            if (Close[0] < r3 && High[0] >= r3)
				SetProfitTarget("SE", CalculationMode.Price, r3m);
			else if (Close[0] < r3m && High[0] >= r3m)
				SetProfitTarget("SE", CalculationMode.Price, r2);
			else if (Close[0] < r2 && High[0] >= r2)
				SetProfitTarget("SE", CalculationMode.Price, r2m);
			else if (Close[0] < r2m && High[0] >= r2m)
				SetProfitTarget("SE", CalculationMode.Price, r1);
			else if (Close[0] < r1 && High[0] >= r1)
				SetProfitTarget("SE", CalculationMode.Price, r1m);
			else if (Close[0] < r1m && High[0] >= r1m)
				SetProfitTarget("SE", CalculationMode.Price, pivotPoint);
			else if (Close[0] < pivotPoint && High[0] >= pivotPoint)
				SetProfitTarget("SE", CalculationMode.Price, s1m);
			else if (Close[0] < s1m && High[0] >= s1m)
				SetProfitTarget("SE", CalculationMode.Price, s1);
			else if (Close[0] < s1 && High[0] >= s1)
				SetProfitTarget("SE", CalculationMode.Price, s2m);
			else if (Close[0] < s2m && High[0] >= s2m)
				SetProfitTarget("SE", CalculationMode.Price, s2);
			else if (Close[0] < s2 && High[0] >= s2)
				SetProfitTarget("SE", CalculationMode.Price, s3m);
			else if (Close[0] < s3m && High[0] >= s3m)
				SetProfitTarget("SE", CalculationMode.Price, s3);
			else if (Close[0] < s3 && High[0] >= s3)
				SetProfitTarget(@"SE", CalculationMode.Ticks, ProfitTarget);
        }

        protected override void InitializeIndicators()
        {
            pivots = NTSvePivots(Close, false, NTSvePivotRange.Daily, NTSveHLCCalculationMode.CalcFromIntradayData, 0, 0, 0, pivotLength);
			pivots.Plots[0].Width = 4;
            AddChartIndicator(pivots);
        }

        #endregion

        #region Properties

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Pivot Length", Order = 1, GroupName = "06. Filter Settings")]
        public int pivotLength { get; set; }

        #endregion
    }
}
