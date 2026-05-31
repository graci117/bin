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
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ElderImpulseToolforNt8 : Indicator
	{
		// v1 = initial version (29-NOV-2022)
		// v2 = without license check (1-DEC-2022)

		#region Variables
		private string codeAuthor = "Author: www.fiverr.com/codernt8";
		private string codeFor = "For: thesuperrookies";
		private string codeOrder = "Order Ref: # FO81FD2068FC4";
		private string codeProductName = "Product Name: ElderImpulseToolforNt8";
		private string codeUpdated = "Last updated: 1-DEC-2022";
		private string codeVers = "v2";
		private EMA EMA1;
		private MACD MACD1;
		#endregion

		#region Config
		public override string DisplayName
		{
			get { return Name; }
		}
		#endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "Elder Impulse Tool for Nt8";
				Calculate = Calculate.OnPriceChange;
				IsOverlay = true;
				versionInfo = codeVers + " " + codeUpdated;
				emaPeriod = 13;
				macdFast = 12;
				macdSlow = 26;
				macdSmooth = 9;
				colorUp = Brushes.Green;
				colorDown = Brushes.Red;
				colorOther = Brushes.Blue;
			}
			else if (State == State.DataLoaded)
			{
				EMA1 = EMA(emaPeriod);
				MACD1 = MACD(macdFast, macdSlow, macdSmooth);
				#region Print Settings
				if (showOutput)
				{
					Print("#########################################################");
					Print(codeAuthor);
					Print(codeFor);
					Print(codeOrder);
					Print(codeProductName);
					Print(codeUpdated);
					Print(codeVers);
					Print("#########################################################");
					Print("Indicator enabled: " + DateTime.Now);
					Print("Indicator Settings...");
					Print(this.ChartBars);
					Print("Timeframe: " + BarsPeriod.BarsPeriodTypeName + " Type #" + BarsPeriod.BarsPeriodTypeSerialize + " Value: " + BarsPeriod.BaseBarsPeriodValue + " Time Period: " + BarsPeriod.Value);
					Print("Bars on chart: " + Count);
					Print("NinjaScript Output: " + showOutput);
					Print("#########################################################");
				}
				#endregion
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToPlot)
				return;

			if (EMA1[0] > EMA1[1] && MACD1.Diff[0] > MACD1.Diff[1])
            {
				BarBrush = colorUp;
			}

			else if (EMA1[0] < EMA1[1] && MACD1.Diff[0] < MACD1.Diff[1])
            {
				BarBrush = colorDown;
            }

            else
            {
				BarBrush = colorOther;
            }
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name = "NinjaScript Output", Description = "", Order = 1, GroupName = "Settings")]
		public bool showOutput
		{ get; set; }

		[NinjaScriptProperty]
		[ReadOnly(true)]
		[Display(Name = "Version:", Description = "", Order = 2, GroupName = "Settings")]
		public string versionInfo
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Period", Description = "", Order = 1, GroupName = "Options - EMA")]
		public int emaPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Fast", Description = "", Order = 1, GroupName = "Options - MACD")]
		public int macdFast
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Slow", Description = "", Order = 20, GroupName = "Options - MACD")]
		public int macdSlow
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name = "Smooth", Description = "", Order = 30, GroupName = "Options - MACD")]
		public int macdSmooth
		{ get; set; }

		[XmlIgnore()]
		[Display(Name = "Up", Order = 10, GroupName = "Options - Bars")]
		public Brush colorUp
		{ get; set; }
		[Browsable(false)]
		public string colorUpT
		{
			get { return Serialize.BrushToString(colorUp); }
			set { colorUp = Serialize.StringToBrush(value); }
		}

		[XmlIgnore()]
		[Display(Name = "Down", Order = 20, GroupName = "Options - Bars")]
		public Brush colorDown
		{ get; set; }
		[Browsable(false)]
		public string colorDownT
		{
			get { return Serialize.BrushToString(colorDown); }
			set { colorDown = Serialize.StringToBrush(value); }
		}

		[XmlIgnore()]
		[Display(Name = "Other", Order = 30, GroupName = "Options - Bars")]
		public Brush colorOther
		{ get; set; }
		[Browsable(false)]
		public string colorOtherT
		{
			get { return Serialize.BrushToString(colorOther); }
			set { colorOther = Serialize.StringToBrush(value); }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ElderImpulseToolforNt8[] cacheElderImpulseToolforNt8;
		public ElderImpulseToolforNt8 ElderImpulseToolforNt8(bool showOutput, string versionInfo, int emaPeriod, int macdFast, int macdSlow, int macdSmooth)
		{
			return ElderImpulseToolforNt8(Input, showOutput, versionInfo, emaPeriod, macdFast, macdSlow, macdSmooth);
		}

		public ElderImpulseToolforNt8 ElderImpulseToolforNt8(ISeries<double> input, bool showOutput, string versionInfo, int emaPeriod, int macdFast, int macdSlow, int macdSmooth)
		{
			if (cacheElderImpulseToolforNt8 != null)
				for (int idx = 0; idx < cacheElderImpulseToolforNt8.Length; idx++)
					if (cacheElderImpulseToolforNt8[idx] != null && cacheElderImpulseToolforNt8[idx].showOutput == showOutput && cacheElderImpulseToolforNt8[idx].versionInfo == versionInfo && cacheElderImpulseToolforNt8[idx].emaPeriod == emaPeriod && cacheElderImpulseToolforNt8[idx].macdFast == macdFast && cacheElderImpulseToolforNt8[idx].macdSlow == macdSlow && cacheElderImpulseToolforNt8[idx].macdSmooth == macdSmooth && cacheElderImpulseToolforNt8[idx].EqualsInput(input))
						return cacheElderImpulseToolforNt8[idx];
			return CacheIndicator<ElderImpulseToolforNt8>(new ElderImpulseToolforNt8(){ showOutput = showOutput, versionInfo = versionInfo, emaPeriod = emaPeriod, macdFast = macdFast, macdSlow = macdSlow, macdSmooth = macdSmooth }, input, ref cacheElderImpulseToolforNt8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ElderImpulseToolforNt8 ElderImpulseToolforNt8(bool showOutput, string versionInfo, int emaPeriod, int macdFast, int macdSlow, int macdSmooth)
		{
			return indicator.ElderImpulseToolforNt8(Input, showOutput, versionInfo, emaPeriod, macdFast, macdSlow, macdSmooth);
		}

		public Indicators.ElderImpulseToolforNt8 ElderImpulseToolforNt8(ISeries<double> input , bool showOutput, string versionInfo, int emaPeriod, int macdFast, int macdSlow, int macdSmooth)
		{
			return indicator.ElderImpulseToolforNt8(input, showOutput, versionInfo, emaPeriod, macdFast, macdSlow, macdSmooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ElderImpulseToolforNt8 ElderImpulseToolforNt8(bool showOutput, string versionInfo, int emaPeriod, int macdFast, int macdSlow, int macdSmooth)
		{
			return indicator.ElderImpulseToolforNt8(Input, showOutput, versionInfo, emaPeriod, macdFast, macdSlow, macdSmooth);
		}

		public Indicators.ElderImpulseToolforNt8 ElderImpulseToolforNt8(ISeries<double> input , bool showOutput, string versionInfo, int emaPeriod, int macdFast, int macdSlow, int macdSmooth)
		{
			return indicator.ElderImpulseToolforNt8(input, showOutput, versionInfo, emaPeriod, macdFast, macdSlow, macdSmooth);
		}
	}
}

#endregion
