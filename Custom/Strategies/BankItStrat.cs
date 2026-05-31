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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
//using NinjaTrader.NinjaScript.Indicators.BuySideGlobal;

#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class BankItStrat : Strategy
	{
		private BankItSystem BankItSystem1;
		private EMA EMA1;
		private VisEMA vEMA1;
		private string  atmStrategyId			= string.Empty;
		private string  orderId					= string.Empty;
		private bool	isAtmStrategyCreated	= false;
		private bool isLong = false;
		private bool isShort = false;
		private MTFMAStatusPanel mtf;
		private Series<double> trendSignal;
		//private NinjaTrader.NinjaScript.Indicators.BuySideGlobal.BSGBlueChip chip;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "BankItStrat";
				
				
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;
				BankMAType = BankItType.ALMA;
				AllowTrades = true;
				  // plot definition
            AddPlot(Brushes.Gold, "trend_signal");   // numeric plot [-1,0,1][web:14]
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, 1);
			}
			else if (State == State.DataLoaded)
			{				
				// BankItSystem1				= BankItSystem(Close, @"BankItSystem - v1.0", BankItType.Tillson, 6, 0.1, 8, 0.5, 6, 0.35, 5, 0.3, 4, 0.3, 42, 14, 10);
				BankItSystem1					= BankItSystem(
            "BankItSystem",          // indicatorName
            BankItType.Tillson,        // ribbonMAType

            8,    0.3,              // t1Length, t1VolumeFactor
            7,    0.3,              // t2Length, t2VolumeFactor
            6,    0.3,             // t3Length, t3VolumeFactor
            5,    0.3,              // t4Length, t4VolumeFactor
            4,    0.3,              // t5Length, t5VolumeFactor
            5,    0.35,             // t6Length, t6VolumeFactor

            21,   6.0,   0.95,      // aWindowSize1, aSigma1, aSample1
            31,   6.35,  0.95,      // aWindowSize2, aSigma2, aSample2
            34,   6.0,   0.95,      // aWindowSize3, aSigma3, aSample3
            37,   6.35,  0.95,      // aWindowSize4, aSigma4, aSample4
            42,   6.35,  0.95,      // aWindowSize5, aSigma5, aSample5
            55,   6.35,  0.95,      // aWindowSize6, aSigma6, aSample6

            42,                     // zombiePeriod
            100,                    // eMALength
            10                      // regionOpacity
        );
				vEMA1 = VisEMA(1,14);
				EMA1				= EMA(Closes[1], 14);
				AddChartIndicator(BankItSystem1);
				//mtf	=  MTFMAStatusPanel(14,34,50,100,1,1,1,1,AverageCalcMode.Exponential,AverageCalcMode.Exponential,AverageCalcMode.Exponential,AverageCalcMode.Exponential);
				
				mtf = MTFMAStatusPanel(
    BarsArray[1],
    14, 34, 50, 100,
    1, 1, 1, 1,
    AverageCalcMode.Exponential,
    AverageCalcMode.Exponential,
    AverageCalcMode.Exponential,
    AverageCalcMode.Exponential);
				     trendSignal = new Series<double>(this);
				//chip.BSGBlueChip.
			}
		}
private const string DefaultAtmTemplate = "BanksyNQ5b";

private string GetAtmTemplateName()
{
    try
    {
        if (ChartControl == null)
            return DefaultAtmTemplate;

        string template = DefaultAtmTemplate;

        ChartControl.Dispatcher.Invoke(() =>
        {
            try
            {
                var chartWindow = System.Windows.Window.GetWindow(ChartControl.Parent)
                                  as NinjaTrader.Gui.Chart.Chart;
                if (chartWindow == null) return;

                var combo = chartWindow.FindFirst("ChartTraderControlATMStrategySelector")
                            as System.Windows.Controls.ComboBox;

                if (combo?.SelectedItem == null) return;

                // SelectedItem is an AtmStrategy object — read Template property directly
                var selectedAtm = combo.SelectedItem as NinjaTrader.NinjaScript.AtmStrategy;
                if (selectedAtm != null && !string.IsNullOrWhiteSpace(selectedAtm.Template))
                {
                    template = selectedAtm.Template;
                    Print("GetAtmTemplateName: resolved template = " + template);
                }
            }
            catch (Exception ex) { Print("GetAtmTemplateName inner: " + ex.Message); }
        });

        return string.IsNullOrWhiteSpace(template) ? DefaultAtmTemplate : template;
    }
    catch (Exception ex)
    {
        Print("GetAtmTemplateName outer: " + ex.Message);
        return DefaultAtmTemplate;
    }
}

// Recursively dumps all named WPF elements to Output window
private void DumpNamedElements(System.Windows.DependencyObject parent, int depth = 0)
{
    if (depth > 6) return; // don't go too deep
    int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
    for (int i = 0; i < count; i++)
    {
        var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
        if (child is System.Windows.FrameworkElement fe && !string.IsNullOrEmpty(fe.Name))
            Print("ELEMENT [depth=" + depth + "]: " + fe.Name + " | Type: " + fe.GetType().Name);
        DumpNamedElements(child, depth + 1);
    }
}
		
		
		
		
		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;

			if (BarsInProgress != 0) 
				return;

			if (CurrentBars[0] < 1
			|| CurrentBars[1] < 0)
				return;
			
			// Make sure this strategy does not execute against historical data
			if(State == State.Historical)
				return;
			
			 double ts = 0;

    if (BankItSystem1.CloudBullish[0] == 1)
        ts = 1;
    else if (BankItSystem1.CloudBearish[0] == 1)
        ts = -1;
    else
        ts = 0;

    trendSignal[0] = ts;      // Series
    Values[0][0]   = ts;      // bind to plot
			
//	 		double highestMA;
//			double lowestMA;
//			highestMA = Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(BankItSystem1.MA1Values[0], BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]);
//	        lowestMA = Math.Min(Math.Min(Math.Min(Math.Min(Math.Min(BankItSystem1.MA1Values[0], BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]), BankItSystem1.MA1Values[0]);
			
					
//					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
//					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
//					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
				
//			Print ("Close[0] less Close[1]" + (Close[0] < Close[1]).ToString() + "-----" + Time[0]);
//			Print ("Closes[1][0] less EMA1[0])" + (Closes[1][0] < EMA1[0]).ToString() + "-----" + Time[0]);
//			Print ("CrossBelow Close, BankItSystem1.LowestValue" + (CrossBelow(Close, BankItSystem1.LowestValue, 1)).ToString() + "-----" + Time[0]);
//			Print ("BankItSystem1.ZombieMeanChangeValues greater	BankItSystem1.HighestValue	" + (BankItSystem1.ZombieMeanChangeValues[0] >	BankItSystem1.HighestValue[0]).ToString()	+ "-----" + Time[0]);
			// Time stamp
//Print(Time[0] + " ====== LONG ENTRY CONDITION CHECK ======");

//// Order and ATM status
//Print(Time[0] + " orderId.Length: " + orderId.Length);
//Print(Time[0] + " atmStrategyId.Length: " + atmStrategyId.Length);

//// Indicator conditions
//Print(Time[0] + " mtf.EMA1Status[0]: " + mtf.EMA1Status[0]);
Print(Time[0] + " BankItSystem1.CloudBullish[0]: " + BankItSystem1.CloudBullish[0]);

//// Price and indicator values for cross conditions
//Print(Time[0] + " Close[0]: " + Close[0]);
//Print(Time[0] + " Low[0]: " + Low[0]);
//Print(Time[0] + " BankItSystem1.HighestValue[0]: " + BankItSystem1.HighestValue[0]);
				//Print("MAX:" +Math.Max(Math.Max(Math.Max(Math.Max(BankItSystem1.MA1Values[0], BankItSystem1.MA2Values[0]), BankItSystem1.MA3Values[0]),BankItSystem1.MA4Values[0]), BankItSystem1.MA5Values[0]), BankItSystem1.MA6Values[0]);
//			Print("MAX: " 
//    + Math.Max(
//        Math.Max(
//            Math.Max(
//                Math.Max(
//                    Math.Max(
//                        BankItSystem1.MA1Values[0],
//                        BankItSystem1.MA2Values[0]),
//                    BankItSystem1.MA3Values[0]),
//                BankItSystem1.MA4Values[0]),
//            BankItSystem1.MA5Values[0]),
//        BankItSystem1.MA6Values[0])
//);
//Print("MA5:"+BankItSystem1.MA5Values[0]);
//	Print("MA5:"+BankItSystem1.MA6Values[0]);
		
//Print(Time[0] + " BankItSystem1.ZombieMeanChangeValues[0]: " + BankItSystem1.ZombieMeanChangeValues[0]);
//Print(Time[0] + " BankItSystem1.LowestValue[0]: " + BankItSystem1.LowestValue[0]);

//// CrossAbove conditions (these return bool)
Print(Time[0] + " CrossAbove(Close, BankItSystem1.HighestValue, 3): " + CrossAbove(Close, BankItSystem1.HighestValue, 3));
Print(Time[0] + " CrossAbove(Close, BankItSystem1.ZombieMeanChangeValues, 1): " + CrossAbove(Close, BankItSystem1.ZombieMeanChangeValues, 1));

// Individual condition evaluations
Print(Time[0] + " (Low[0],2)less HighestValue[0]: " + (Low[0] < BankItSystem1.HighestValue[0]));
//			Print(  " MIN(Low[0],2): " + MIN(Low,2)[0]);
//			Print(  "  HighestValue[0]: " + BankItSystem1.HighestValue[0]);
			
Print(Time[0] + " Close[0] > HighestValue[0]: " + (Close[0] > BankItSystem1.HighestValue[0]));
Print(Time[0] + " ZombieMeanChangeValues[0] < LowestValue[0]: " + (BankItSystem1.ZombieMeanChangeValues[0] < BankItSystem1.LowestValue[0]));
Print(Time[0] + " Close[0] > ZombieMeanChangeValues[0]: " + (Close[0] > BankItSystem1.ZombieMeanChangeValues[0]));

// Combined nested OR conditions
//Print(Time[0] + " First OR block (CrossAbove OR Low/Close condition OR CrossAbove Zombie): " + 
//    (CrossAbove(Close, BankItSystem1.HighestValue, 3) 
//    || (Low[0] < BankItSystem1.HighestValue[0] && Close[0] > BankItSystem1.HighestValue[0])
//    || CrossAbove(Close, BankItSystem1.ZombieMeanChangeValues, 1)));

//Print(Time[0] + " Second condition (Zombie < Lowest OR Close > Zombie): " + 
//    (BankItSystem1.ZombieMeanChangeValues[0] < BankItSystem1.LowestValue[0] 
//    || Close[0] > BankItSystem1.ZombieMeanChangeValues[0]));

//// Final overall condition
//Print(Time[0] + " Overall IF condition result: " + 
//    (orderId.Length == 0 && atmStrategyId.Length == 0 
//    && mtf.EMA1Status[0] == 1 
//    && BankItSystem1.CloudBullish[0] == 1
//    && (CrossAbove(Close, BankItSystem1.HighestValue, 3) 
//        ||(MIN(Low,2)[0] < BankItSystem1.HighestValue[0] && Close[0] > BankItSystem1.HighestValue[0])
//        || CrossAbove(Close, BankItSystem1.ZombieMeanChangeValues, 1))
//    && (BankItSystem1.ZombieMeanChangeValues[0] < BankItSystem1.LowestValue[0] 
//        || Close[0] > BankItSystem1.ZombieMeanChangeValues[0])));

//Print(Time[0] + " ======================================");
			
			//Print(Time[0] + " =================EMA1Status[0]====================="+mtf.MA1Status[0]);


			 // Set 1
			if (
				 orderId.Length == 0 && atmStrategyId.Length == 0 
				//&& mtf.MA1Status[0] == 1
				&& BankItSystem1.CloudBullish[0] == 1
				 //&& Close[0] > Close[1]
				 //&& (Closes[1][0] > EMA1[0]  ==1
					//|| Math.Abs((Close[0] - vEMA1[0])) > 40)
				  && ((CrossAbove(Close, BankItSystem1.HighestValue, 3)
						|| (Low[0] < BankItSystem1.HighestValue[0] 
								&& Close[0] > BankItSystem1.HighestValue[0]
							)
						|| CrossAbove(Close,BankItSystem1.ZombieMeanChangeValues,1)
				     ))
				   && 	(BankItSystem1.ZombieMeanChangeValues[0] <	BankItSystem1.LowestValue[0]
						|| Close[0] > BankItSystem1.ZombieMeanChangeValues[0]
					
					)
					&& !isLong
					&& !isShort
				)			
		
				
				
			{
				//Print(Time[0] + " =================EMA1Status[0]yay====================="+mtf.MA1Status[0]);
				Print(Time[0] + " =================notthereyetAllowedyay=====================");
				
				if (AllowTrades)
				{
					
				Print(Time[0] + " =================Allowedyay=====================");
							isAtmStrategyCreated = false;
			        atmStrategyId = GetAtmStrategyUniqueId();
			        orderId = GetAtmStrategyUniqueId();
			
			        string atmTemplate = GetAtmTemplateName();
					Print(Time[0] + " =================Atmname=====================" + atmTemplate);
					AtmStrategyCreate(OrderAction.Buy, OrderType.Market, Low[0], 0, TimeInForce.Day, orderId, atmTemplate, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
						//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
						if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						{
							Print(Time[0] + " =================ATM-Damn=====================");
							isAtmStrategyCreated = true;
						}
						
					});
//					isLong = true;
//					isShort = false;
				}
				else
				{
					Draw.Text(this, Convert.ToString("Long") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
//					Print ("BankItSystem1 CloudBullish" + BankItSystem1.CloudBullish[0] + "-----" + Time[0]);
					isLong = true;
					isShort = false;
				}
				//isShort = false;
				//isLong = true;
			}
			
				if (((Close[1] > Open[1] && Close[0] < Open[0]
				||  (BankItSystem1.CloudBullish[0] != 1))			
				)
				&& isLong)
			{
				Draw.Text(this, Convert.ToString("ExitLong") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ExitLong", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
				isLong = false;
								
			}
			
			
			
			Print ("BankItSystem1 CloudBearish" + BankItSystem1.CloudBearish[0] + "-----" + Time[0]);
			Print ("Close[0] less Close[1]" + (Close[0] < Close[1]).ToString() + "-----" + Time[0]);
			Print ("Closes[1][0] less EMA1[0])" + (Closes[1][0] < EMA1[0]).ToString() + "-----" + Time[0]);
			Print ("CrossBelow Close, BankItSystem1.LowestValue" + (CrossBelow(Close, BankItSystem1.LowestValue, 1)).ToString() + "-----" + Time[0]);
			Print ("BankItSystem1.ZombieMeanChangeValues greater	BankItSystem1.HighestValue	" + (BankItSystem1.ZombieMeanChangeValues[0] >	BankItSystem1.HighestValue[0]).ToString()	+ "-----" + Time[0]);
			Print ("orderId.Length	" + orderId.Length.ToString()	+ "-----" + Time[0]);
			Print ("atmStrategyId.Length	" + atmStrategyId.Length.ToString()	+ "-----" + Time[0]);
			
			
//			if (
//				  orderId.Length == 0 && atmStrategyId.Length == 0 
//				 && (BankItSystem1.CloudBearish[0] == 1)
//				 && (Close[0] < Close[1])
//				 && ((Closes[1][0] < EMA1[0]) 
//					|| (Math.Abs((Close[0] - vEMA1[0])) > 40))
//				 && (CrossBelow(Close, BankItSystem1.LowestValue, 2)
//						|| CrossBelow(Close, BankItSystem1.ZombieMeanChangeValues, 1))
//				 && 	BankItSystem1.ZombieMeanChangeValues[0] >	BankItSystem1.HighestValue[0]		
				
//				)
				
			if (
				 orderId.Length == 0 && atmStrategyId.Length == 0 
				// && mtf.MA1Status[0] == 0
				
				&& BankItSystem1.CloudBearish[0] == 1
				 && Close[0] < Close[1]
				// && (Closes[1][0] < EMA1[0] 
					//|| Math.Abs((Close[0] - vEMA1[0])) > 40)
				  && ((CrossBelow(Close, BankItSystem1.LowestValue, 3)
						|| (High[0] > BankItSystem1.LowestValue[0] 
								&& Close[0] < BankItSystem1.LowestValue[0]
							)
						|| CrossBelow(Close,BankItSystem1.ZombieMeanChangeValues,1)
				     ))
				   && 	(BankItSystem1.ZombieMeanChangeValues[0] >	BankItSystem1.HighestValue[0]
						|| Close[0] < BankItSystem1.ZombieMeanChangeValues[0]
					&& !isLong
					&& !isShort)
				)			
			{
								Print(Time[0] + " =================Short-notthereyetAllowedyay=====================");

				if (AllowTrades)
				{
									Print(Time[0] + " =================Shott-Allowedyay=====================");

					isAtmStrategyCreated = false;
			        atmStrategyId = GetAtmStrategyUniqueId();
			        orderId = GetAtmStrategyUniqueId();
			
			        string atmTemplate = GetAtmTemplateName();
					AtmStrategyCreate(OrderAction.Sell, OrderType.Limit, Low[0], 0, TimeInForce.Day, orderId, atmTemplate, atmStrategyId, (atmCallbackErrorCode, atmCallBackId) => {
						//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
						if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyId)
						{
							isAtmStrategyCreated = true;
							Print(Time[0] + " =================Shott-noerror=====================");
						}
							
					});
				}
				else
				{
					Draw.Text(this, Convert.ToString("Short") + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );
					Print ("BankItSystem1 CloudBearish" + BankItSystem1.CloudBearish[0] + "-----" + Time[0]);
					
				}
				isShort = true;
				isLong = false;
			}
			
			if (((Close[1] < Open[1] && Close[0] > Open[0]
				||  (BankItSystem1.CloudBearish[0] != 1))			
				)
				&& isShort)
			{
				Draw.Text(this, Convert.ToString("ExitShort") + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "ExitShort", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );	
				isShort = false;
								
			}
			
			if (AllowTrades)
			{
			
				if (!isAtmStrategyCreated)
					return;
				
				// Check for a pending entry order
				if (orderId.Length > 0)
				{
					string[] status = GetAtmStrategyEntryOrderStatus(orderId);
	
					// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
					if (status.GetLength(0) > 0)
					{
						// Print out some information about the order to the output window
//						Print("The entry order average fill price is: " + status[0]);
//						Print("The entry order filled amount is: " + status[1]);
//						Print("The entry order order state is: " + status[2]);
	
						// If the order state is terminal, reset the order id value
						if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
							orderId = string.Empty;
					}
				} // If the strategy has terminated reset the strategy id
				else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == Cbi.MarketPosition.Flat)
					atmStrategyId = string.Empty;
	
				if (atmStrategyId.Length > 0)
				{
					// You can change the stop price
					if (GetAtmStrategyMarketPosition(atmStrategyId) != MarketPosition.Flat)
						AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP1", atmStrategyId);
	
					// Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
					// the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
//					Print("The current ATM Strategy market position is: " + GetAtmStrategyMarketPosition(atmStrategyId));
//					Print("The current ATM Strategy position quantity is: " + GetAtmStrategyPositionQuantity(atmStrategyId));
//					Print("The current ATM Strategy average price is: " + GetAtmStrategyPositionAveragePrice(atmStrategyId));
//					Print("The current ATM Strategy Unrealized PnL is: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyId));
				}
			}
			
		}
		
		 #region Properties
		
		[NinjaScriptProperty]
		[Display(Name="MA Type", Description="MA Type", Order=1)]
		public BankItType BankMAType
        { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name=" AllowTrades", Description="AllowTrades", Order=2)]
		public bool AllowTrades
        { get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Trend_Signal
		{
		    get { return trendSignal; }
}
			
		 #endregion;
	}
}
