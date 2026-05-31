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

#endregion



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		
		private QuantVue.QbankIndicator[] cacheQbankIndicator;

		
		public QuantVue.QbankIndicator QbankIndicator(int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool qcloudAlerts, int hdsma1RenkoSize, int hdsma2RenkoSize, int hdsma3RenkoSize, int qWave1TMAPB, int qWave1ATRPB, double qWave1ATRM, double qWave1Coeff, int qWave1AP, bool qWave1NVD, bool qWave1CIB, Brush barsInsideColor, bool qWave1Alerts, SimpleFont font, int verticalOffset, int horizontalOffset, Brush labelTextBrush, Brush areaBrush, Brush borderBrush, Brush positiveBrush, Brush negativeBrush, Brush neutralBrush, Brush bBrush, bool paintBackground, double backgroundOp, bool classColorBars, Brush barsClassColor)
		{
			return QbankIndicator(Input, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, qcloudAlerts, hdsma1RenkoSize, hdsma2RenkoSize, hdsma3RenkoSize, qWave1TMAPB, qWave1ATRPB, qWave1ATRM, qWave1Coeff, qWave1AP, qWave1NVD, qWave1CIB, barsInsideColor, qWave1Alerts, font, verticalOffset, horizontalOffset, labelTextBrush, areaBrush, borderBrush, positiveBrush, negativeBrush, neutralBrush, bBrush, paintBackground, backgroundOp, classColorBars, barsClassColor);
		}


		
		public QuantVue.QbankIndicator QbankIndicator(ISeries<double> input, int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool qcloudAlerts, int hdsma1RenkoSize, int hdsma2RenkoSize, int hdsma3RenkoSize, int qWave1TMAPB, int qWave1ATRPB, double qWave1ATRM, double qWave1Coeff, int qWave1AP, bool qWave1NVD, bool qWave1CIB, Brush barsInsideColor, bool qWave1Alerts, SimpleFont font, int verticalOffset, int horizontalOffset, Brush labelTextBrush, Brush areaBrush, Brush borderBrush, Brush positiveBrush, Brush negativeBrush, Brush neutralBrush, Brush bBrush, bool paintBackground, double backgroundOp, bool classColorBars, Brush barsClassColor)
		{
			if (cacheQbankIndicator != null)
				for (int idx = 0; idx < cacheQbankIndicator.Length; idx++)
					if (cacheQbankIndicator[idx].InpPeriod3 == inpPeriod3 && cacheQbankIndicator[idx].InpPeriod4 == inpPeriod4 && cacheQbankIndicator[idx].InpPeriod5 == inpPeriod5 && cacheQbankIndicator[idx].InpPeriod6 == inpPeriod6 && cacheQbankIndicator[idx].qcloudAlerts == qcloudAlerts && cacheQbankIndicator[idx].hdsma1RenkoSize == hdsma1RenkoSize && cacheQbankIndicator[idx].hdsma2RenkoSize == hdsma2RenkoSize && cacheQbankIndicator[idx].hdsma3RenkoSize == hdsma3RenkoSize && cacheQbankIndicator[idx].qWave1TMAPB == qWave1TMAPB && cacheQbankIndicator[idx].qWave1ATRPB == qWave1ATRPB && cacheQbankIndicator[idx].qWave1ATRM == qWave1ATRM && cacheQbankIndicator[idx].qWave1Coeff == qWave1Coeff && cacheQbankIndicator[idx].qWave1AP == qWave1AP && cacheQbankIndicator[idx].qWave1NVD == qWave1NVD && cacheQbankIndicator[idx].qWave1CIB == qWave1CIB && cacheQbankIndicator[idx].barsInsideColor == barsInsideColor && cacheQbankIndicator[idx].qWave1Alerts == qWave1Alerts && cacheQbankIndicator[idx].Font == font && cacheQbankIndicator[idx].VerticalOffset == verticalOffset && cacheQbankIndicator[idx].HorizontalOffset == horizontalOffset && cacheQbankIndicator[idx].LabelTextBrush == labelTextBrush && cacheQbankIndicator[idx].AreaBrush == areaBrush && cacheQbankIndicator[idx].BorderBrush == borderBrush && cacheQbankIndicator[idx].PositiveBrush == positiveBrush && cacheQbankIndicator[idx].NegativeBrush == negativeBrush && cacheQbankIndicator[idx].NeutralBrush == neutralBrush && cacheQbankIndicator[idx].BBrush == bBrush && cacheQbankIndicator[idx].paintBackground == paintBackground && cacheQbankIndicator[idx].backgroundOp == backgroundOp && cacheQbankIndicator[idx].classColorBars == classColorBars && cacheQbankIndicator[idx].barsClassColor == barsClassColor && cacheQbankIndicator[idx].EqualsInput(input))
						return cacheQbankIndicator[idx];
			return CacheIndicator<QuantVue.QbankIndicator>(new QuantVue.QbankIndicator(){ InpPeriod3 = inpPeriod3, InpPeriod4 = inpPeriod4, InpPeriod5 = inpPeriod5, InpPeriod6 = inpPeriod6, qcloudAlerts = qcloudAlerts, hdsma1RenkoSize = hdsma1RenkoSize, hdsma2RenkoSize = hdsma2RenkoSize, hdsma3RenkoSize = hdsma3RenkoSize, qWave1TMAPB = qWave1TMAPB, qWave1ATRPB = qWave1ATRPB, qWave1ATRM = qWave1ATRM, qWave1Coeff = qWave1Coeff, qWave1AP = qWave1AP, qWave1NVD = qWave1NVD, qWave1CIB = qWave1CIB, barsInsideColor = barsInsideColor, qWave1Alerts = qWave1Alerts, Font = font, VerticalOffset = verticalOffset, HorizontalOffset = horizontalOffset, LabelTextBrush = labelTextBrush, AreaBrush = areaBrush, BorderBrush = borderBrush, PositiveBrush = positiveBrush, NegativeBrush = negativeBrush, NeutralBrush = neutralBrush, BBrush = bBrush, paintBackground = paintBackground, backgroundOp = backgroundOp, classColorBars = classColorBars, barsClassColor = barsClassColor }, input, ref cacheQbankIndicator);
		}

	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		
		public Indicators.QuantVue.QbankIndicator QbankIndicator(int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool qcloudAlerts, int hdsma1RenkoSize, int hdsma2RenkoSize, int hdsma3RenkoSize, int qWave1TMAPB, int qWave1ATRPB, double qWave1ATRM, double qWave1Coeff, int qWave1AP, bool qWave1NVD, bool qWave1CIB, Brush barsInsideColor, bool qWave1Alerts, SimpleFont font, int verticalOffset, int horizontalOffset, Brush labelTextBrush, Brush areaBrush, Brush borderBrush, Brush positiveBrush, Brush negativeBrush, Brush neutralBrush, Brush bBrush, bool paintBackground, double backgroundOp, bool classColorBars, Brush barsClassColor)
		{
			return indicator.QbankIndicator(Input, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, qcloudAlerts, hdsma1RenkoSize, hdsma2RenkoSize, hdsma3RenkoSize, qWave1TMAPB, qWave1ATRPB, qWave1ATRM, qWave1Coeff, qWave1AP, qWave1NVD, qWave1CIB, barsInsideColor, qWave1Alerts, font, verticalOffset, horizontalOffset, labelTextBrush, areaBrush, borderBrush, positiveBrush, negativeBrush, neutralBrush, bBrush, paintBackground, backgroundOp, classColorBars, barsClassColor);
		}


		
		public Indicators.QuantVue.QbankIndicator QbankIndicator(ISeries<double> input , int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool qcloudAlerts, int hdsma1RenkoSize, int hdsma2RenkoSize, int hdsma3RenkoSize, int qWave1TMAPB, int qWave1ATRPB, double qWave1ATRM, double qWave1Coeff, int qWave1AP, bool qWave1NVD, bool qWave1CIB, Brush barsInsideColor, bool qWave1Alerts, SimpleFont font, int verticalOffset, int horizontalOffset, Brush labelTextBrush, Brush areaBrush, Brush borderBrush, Brush positiveBrush, Brush negativeBrush, Brush neutralBrush, Brush bBrush, bool paintBackground, double backgroundOp, bool classColorBars, Brush barsClassColor)
		{
			return indicator.QbankIndicator(input, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, qcloudAlerts, hdsma1RenkoSize, hdsma2RenkoSize, hdsma3RenkoSize, qWave1TMAPB, qWave1ATRPB, qWave1ATRM, qWave1Coeff, qWave1AP, qWave1NVD, qWave1CIB, barsInsideColor, qWave1Alerts, font, verticalOffset, horizontalOffset, labelTextBrush, areaBrush, borderBrush, positiveBrush, negativeBrush, neutralBrush, bBrush, paintBackground, backgroundOp, classColorBars, barsClassColor);
		}
	
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		
		public Indicators.QuantVue.QbankIndicator QbankIndicator(int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool qcloudAlerts, int hdsma1RenkoSize, int hdsma2RenkoSize, int hdsma3RenkoSize, int qWave1TMAPB, int qWave1ATRPB, double qWave1ATRM, double qWave1Coeff, int qWave1AP, bool qWave1NVD, bool qWave1CIB, Brush barsInsideColor, bool qWave1Alerts, SimpleFont font, int verticalOffset, int horizontalOffset, Brush labelTextBrush, Brush areaBrush, Brush borderBrush, Brush positiveBrush, Brush negativeBrush, Brush neutralBrush, Brush bBrush, bool paintBackground, double backgroundOp, bool classColorBars, Brush barsClassColor)
		{
			return indicator.QbankIndicator(Input, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, qcloudAlerts, hdsma1RenkoSize, hdsma2RenkoSize, hdsma3RenkoSize, qWave1TMAPB, qWave1ATRPB, qWave1ATRM, qWave1Coeff, qWave1AP, qWave1NVD, qWave1CIB, barsInsideColor, qWave1Alerts, font, verticalOffset, horizontalOffset, labelTextBrush, areaBrush, borderBrush, positiveBrush, negativeBrush, neutralBrush, bBrush, paintBackground, backgroundOp, classColorBars, barsClassColor);
		}


		
		public Indicators.QuantVue.QbankIndicator QbankIndicator(ISeries<double> input , int inpPeriod3, int inpPeriod4, int inpPeriod5, int inpPeriod6, bool qcloudAlerts, int hdsma1RenkoSize, int hdsma2RenkoSize, int hdsma3RenkoSize, int qWave1TMAPB, int qWave1ATRPB, double qWave1ATRM, double qWave1Coeff, int qWave1AP, bool qWave1NVD, bool qWave1CIB, Brush barsInsideColor, bool qWave1Alerts, SimpleFont font, int verticalOffset, int horizontalOffset, Brush labelTextBrush, Brush areaBrush, Brush borderBrush, Brush positiveBrush, Brush negativeBrush, Brush neutralBrush, Brush bBrush, bool paintBackground, double backgroundOp, bool classColorBars, Brush barsClassColor)
		{
			return indicator.QbankIndicator(input, inpPeriod3, inpPeriod4, inpPeriod5, inpPeriod6, qcloudAlerts, hdsma1RenkoSize, hdsma2RenkoSize, hdsma3RenkoSize, qWave1TMAPB, qWave1ATRPB, qWave1ATRM, qWave1Coeff, qWave1AP, qWave1NVD, qWave1CIB, barsInsideColor, qWave1Alerts, font, verticalOffset, horizontalOffset, labelTextBrush, areaBrush, borderBrush, positiveBrush, negativeBrush, neutralBrush, bBrush, paintBackground, backgroundOp, classColorBars, barsClassColor);
		}

	}
}

#endregion
