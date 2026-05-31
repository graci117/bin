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
	public class EMAWithBraid : Indicator
	{
		private EMA EMA1;
		private BraidFilter BraidFilter1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "EMAWithBraid";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				EMALength					= 14;
				IsLong = false;
				IsShort = false;
				LongSignal					= @"nLong";
				ShortSignal					= @"nShort";
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				EMA1				= EMA(Close, EMALength);
				BraidFilter1				= BraidFilter(3,7,14,14,MAtypeBraid.EMA,40);
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			
			
			if ((CrossAbove(Close, EMA1, 1))
				&& ((IsLong == false)) 
				&& (BraidFilter1.Values[2][0] == 1))
			{
				Draw.Text(this, Convert.ToString(LongSignal) + Convert.ToString(CurrentBars[0]), @"🢁" + System.Environment.NewLine + "Long", 0, (Low[0] + (-12 * TickSize)), Brushes.Lime );				
				IsLong = true;
				IsShort = false;
			}
			
			if ((CrossBelow(Close, EMA1, 1))
				&& ((IsShort == false))
				&& BraidFilter1.Values[3][0] == 1)
			{
				Draw.Text(this, Convert.ToString(ShortSignal) + Convert.ToString(CurrentBars[0]), "Short" + System.Environment.NewLine + @"🢃", 0, (High[0] + (12 * TickSize)), Brushes.Red );
					IsShort = true;
					IsLong = false;
					
			}
		
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EMALength", Order=1, GroupName="Parameters")]
		public int EMALength
		{ get; set; }
		
			[NinjaScriptProperty]
		//[Display(Name="IsLong", Order=18, GroupName="Parameters")]
		public bool IsLong
		{ get; set; }

		[NinjaScriptProperty]
		//[Display(Name="IsShort", Order=19, GroupName="Parameters")]
		public bool IsShort
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="LongSignal", Order=14, GroupName="Bollinger Reversal Pro")]
		public string LongSignal
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShortSignal", Order=15, GroupName="Bollinger Reversal Pro")]
		public string ShortSignal
		{ get; set; }
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EMAWithBraid[] cacheEMAWithBraid;
		public EMAWithBraid EMAWithBraid(int eMALength, bool isLong, bool isShort, string longSignal, string shortSignal)
		{
			return EMAWithBraid(Input, eMALength, isLong, isShort, longSignal, shortSignal);
		}

		public EMAWithBraid EMAWithBraid(ISeries<double> input, int eMALength, bool isLong, bool isShort, string longSignal, string shortSignal)
		{
			if (cacheEMAWithBraid != null)
				for (int idx = 0; idx < cacheEMAWithBraid.Length; idx++)
					if (cacheEMAWithBraid[idx] != null && cacheEMAWithBraid[idx].EMALength == eMALength && cacheEMAWithBraid[idx].IsLong == isLong && cacheEMAWithBraid[idx].IsShort == isShort && cacheEMAWithBraid[idx].LongSignal == longSignal && cacheEMAWithBraid[idx].ShortSignal == shortSignal && cacheEMAWithBraid[idx].EqualsInput(input))
						return cacheEMAWithBraid[idx];
			return CacheIndicator<EMAWithBraid>(new EMAWithBraid(){ EMALength = eMALength, IsLong = isLong, IsShort = isShort, LongSignal = longSignal, ShortSignal = shortSignal }, input, ref cacheEMAWithBraid);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EMAWithBraid EMAWithBraid(int eMALength, bool isLong, bool isShort, string longSignal, string shortSignal)
		{
			return indicator.EMAWithBraid(Input, eMALength, isLong, isShort, longSignal, shortSignal);
		}

		public Indicators.EMAWithBraid EMAWithBraid(ISeries<double> input , int eMALength, bool isLong, bool isShort, string longSignal, string shortSignal)
		{
			return indicator.EMAWithBraid(input, eMALength, isLong, isShort, longSignal, shortSignal);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EMAWithBraid EMAWithBraid(int eMALength, bool isLong, bool isShort, string longSignal, string shortSignal)
		{
			return indicator.EMAWithBraid(Input, eMALength, isLong, isShort, longSignal, shortSignal);
		}

		public Indicators.EMAWithBraid EMAWithBraid(ISeries<double> input , int eMALength, bool isLong, bool isShort, string longSignal, string shortSignal)
		{
			return indicator.EMAWithBraid(input, eMALength, isLong, isShort, longSignal, shortSignal);
		}
	}
}

#endregion
