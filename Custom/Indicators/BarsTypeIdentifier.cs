#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
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
	public class BarsTypeIdentifier : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"A simple script to print all bar types out with their information";
				Name										= "BarsTypeIdentifier";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
				Type[] types = NinjaTrader.Core.Globals.AssemblyRegistry.GetDerivedTypes(typeof(BarsType));
                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];
                    if (type == null || type.FullName.IsNullOrEmpty()) continue;
	                var type2 = NinjaTrader.Core.Globals.AssemblyRegistry.GetType(type.FullName);
	                if (type2 == null) continue;
					BarsType bar = Activator.CreateInstance(type2) as BarsType;
                    if (bar != null)
                    {
                 		bar.SetState(State.SetDefaults);
						int id = (int)bar.BarsPeriod.BarsPeriodType;
						Print(string.Format("{0} - {1}", bar.Name, id,id));
						bar.SetState(State.Terminated);
                    }
                }
			}
		}

		protected override void OnBarUpdate(){}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BarsTypeIdentifier[] cacheBarsTypeIdentifier;
		public BarsTypeIdentifier BarsTypeIdentifier()
		{
			return BarsTypeIdentifier(Input);
		}

		public BarsTypeIdentifier BarsTypeIdentifier(ISeries<double> input)
		{
			if (cacheBarsTypeIdentifier != null)
				for (int idx = 0; idx < cacheBarsTypeIdentifier.Length; idx++)
					if (cacheBarsTypeIdentifier[idx] != null &&  cacheBarsTypeIdentifier[idx].EqualsInput(input))
						return cacheBarsTypeIdentifier[idx];
			return CacheIndicator<BarsTypeIdentifier>(new BarsTypeIdentifier(), input, ref cacheBarsTypeIdentifier);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BarsTypeIdentifier BarsTypeIdentifier()
		{
			return indicator.BarsTypeIdentifier(Input);
		}

		public Indicators.BarsTypeIdentifier BarsTypeIdentifier(ISeries<double> input )
		{
			return indicator.BarsTypeIdentifier(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BarsTypeIdentifier BarsTypeIdentifier()
		{
			return indicator.BarsTypeIdentifier(Input);
		}

		public Indicators.BarsTypeIdentifier BarsTypeIdentifier(ISeries<double> input )
		{
			return indicator.BarsTypeIdentifier(input);
		}
	}
}

#endregion
