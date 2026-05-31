#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class SupplyDemandZones : Indicator
    {
        private class Zone
        {
            public double   Top;
            public double   Bottom;
            public bool     IsDemand;
            public string   Tag;
            public int      StartBar;
            public DateTime StartTime;
            public bool     PriceTouchedZone;
            public int      BarsOutsideCount;
			
        }
		private Swing _swing;
        private List<Zone>   _zones;
        private int          _zoneCounter;
        private HashSet<int> _zoneCreatedAtBar;

        // ── Public signal — readable by strategy ──────────────────────────────
        public int SignalState { get; private set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description              = "Supply & Demand Zone Indicator";
                Name                     = "SupplyDemandZones";
                Calculate                = Calculate.OnBarClose;
                IsOverlay                = true;
                DisplayInDataBox         = true;
                DrawOnPricePanel         = true;
                ScaleJustification       = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;

                ConsecutiveBars        = 3;
                BreakTicks             = 10;
                ReversalBars           = 1;
                OppositeZoneClearTicks = 1;
                DrawZoneArrows         = false;
                DemandColor            = Brushes.LimeGreen;
                SupplyColor            = Brushes.Red;
                BuySignalColor         = Brushes.Cyan;
                SellSignalColor        = Brushes.Magenta;
                ZoneOpacity            = 40;
				AddPlot(Brushes.Pink, "SignalState");
				DisplayInDataBox = true;
				SwingStrength = 3;
				ZoneMergeTicks = 12;

            }
            else if (State == State.Configure)
            {
                BarsRequiredToPlot = 5;
            }
            else if (State == State.DataLoaded)
            {
                _zones            = new List<Zone>();
                _zoneCounter      = 0;
                _zoneCreatedAtBar = new HashSet<int>();
                SignalState       = 0;
				_swing = Swing(SwingStrength);
            }
        }

      protected override void OnBarUpdate()
{
    Values[0][0] = CurrentBar == 0 ? 0 : Values[0][1];

    if (CurrentBar < Math.Max(ConsecutiveBars, SwingStrength * 2 + 1))
        return;

    TryCreateZone();
    CheckZoneLogic();

    Values[0][0] = SignalState;
}


        private int CountRun(int startBarsAgo)
        {
            int dir   = Close[startBarsAgo] >= Open[startBarsAgo] ? 1 : -1;
            int count = 1;
            int limit = Math.Min(CurrentBar, startBarsAgo + ConsecutiveBars + 5);

            for (int i = startBarsAgo + 1; i <= limit; i++)
            {
                if ((Close[i] >= Open[i] ? 1 : -1) == dir)
                    count++;
                else
                    break;
            }
            return count;
        }

      private bool ZoneAlreadyExists(double top, double bottom, bool isDemand)
{
    foreach (Zone z in _zones)
    {
        bool sameType   = z.IsDemand == isDemand;
        bool similarTop = Math.Abs(z.Top - top) < TickSize * 2;
        bool similarBot = Math.Abs(z.Bottom - bottom) < TickSize * 2;

        if (sameType && similarTop && similarBot)
            return true;
    }

    return false;
}


private bool TryMergeZone(double newTop, double newBottom, bool isDemand, DateTime startTime)
{
    double mergeDist = ZoneMergeTicks * TickSize;

    foreach (Zone z in _zones)
    {
        if (z.IsDemand != isDemand)
            continue;

        bool nearTop    = Math.Abs(z.Top - newTop) <= mergeDist;
        bool nearBottom = Math.Abs(z.Bottom - newBottom) <= mergeDist;
        bool overlap    = newBottom <= z.Top && newTop >= z.Bottom;

        if (nearTop || nearBottom || overlap)
        {
            z.Top       = Math.Max(z.Top, newTop);
            z.Bottom    = Math.Min(z.Bottom, newBottom);
            z.StartTime = startTime < z.StartTime ? startTime : z.StartTime;

            RemoveDrawObject(z.Tag);
            DrawZone(z);
            return true;
        }
    }

    return false;
}



	private void TryCreateZone()
{
    if (_zoneCreatedAtBar.Contains(CurrentBar))
        return;

    if (CurrentBar < Math.Max(ConsecutiveBars, SwingStrength * 2 + 1))
        return;

    int runDir = Close[0] >= Open[0] ? 1 : -1;
    int runLen = CountRunFromBarsAgo(0);

    if (runLen < ConsecutiveBars)
        return;

    if (runDir == 1)
    {
        int swingLowBarsAgo = _swing.SwingLowBar(0, 1, CurrentBar);

        if (swingLowBarsAgo >= 0 && swingLowBarsAgo >= runLen - 1)
        {
            double zoneTop    = High[swingLowBarsAgo];
            double zoneBottom = Low[swingLowBarsAgo];
            DateTime startTime = Time[swingLowBarsAgo];

            if (TryMergeZone(zoneTop, zoneBottom, true, startTime))
            {
                _zoneCreatedAtBar.Add(CurrentBar);
                return;
            }

            string tag = "SDZ_" + _zoneCounter++;

            var zone = new Zone
            {
                Top              = zoneTop,
                Bottom           = zoneBottom,
                IsDemand         = true,
                Tag              = tag,
                StartBar         = CurrentBar,
                StartTime        = startTime,
                PriceTouchedZone = false,
                BarsOutsideCount = 0
            };

            _zones.Add(zone);
            _zoneCreatedAtBar.Add(CurrentBar);
            DrawZone(zone);
        }
    }
    else
    {
        int swingHighBarsAgo = _swing.SwingHighBar(0, 1, CurrentBar);

        if (swingHighBarsAgo >= 0 && swingHighBarsAgo >= runLen - 1)
        {
            double zoneTop     = High[swingHighBarsAgo];
            double zoneBottom  = Low[swingHighBarsAgo];
            DateTime startTime = Time[swingHighBarsAgo];

            if (TryMergeZone(zoneTop, zoneBottom, false, startTime))
            {
                _zoneCreatedAtBar.Add(CurrentBar);
                return;
            }

            string tag = "SDZ_" + _zoneCounter++;

            var zone = new Zone
            {
                Top              = zoneTop,
                Bottom           = zoneBottom,
                IsDemand         = false,
                Tag              = tag,
                StartBar         = CurrentBar,
                StartTime        = startTime,
                PriceTouchedZone = false,
                BarsOutsideCount = 0
            };

            _zones.Add(zone);
            _zoneCreatedAtBar.Add(CurrentBar);
            DrawZone(zone);
        }
    }
}



		
		private bool IsInsideAnyZone(double price)
{
    foreach (Zone z in _zones)
    {
        if (price >= z.Bottom && price <= z.Top)
            return true;
    }
    return false;
}


private int CountRunFromBarsAgo(int startBarsAgo)
{
    int dir   = Close[startBarsAgo] >= Open[startBarsAgo] ? 1 : -1;
    int count = 1;
    int maxBarsAgo = CurrentBar;

    for (int i = startBarsAgo + 1; i <= maxBarsAgo; i++)
    {
        int dirI = Close[i] >= Open[i] ? 1 : -1;

        if (dirI == dir)
            count++;
        else
            break;
    }

    return count;
}

		       


private void CheckZoneLogic()
{
    double breakDist = BreakTicks * TickSize;
    double closeNow  = Close[0];
    Zone   firedZone = null;

    for (int i = _zones.Count - 1; i >= 0; i--)
    {
        Zone z = _zones[i];

        bool broken = z.IsDemand
            ? closeNow < z.Bottom - breakDist
            : closeNow > z.Top + breakDist;

        if (broken)
        {
            RemoveDrawObject(z.Tag);
            _zones.RemoveAt(i);
            continue;
        }

        bool insideThisZone = closeNow >= z.Bottom && closeNow <= z.Top;

        if (insideThisZone)
        {
            z.PriceTouchedZone = true;
            z.BarsOutsideCount = 0;
            continue;
        }

        if (z.PriceTouchedZone)
        {
            bool exitedCorrectSide = z.IsDemand
                ? closeNow > z.Top
                : closeNow < z.Bottom;

            if (exitedCorrectSide)
            {
                z.BarsOutsideCount++;

                if (z.BarsOutsideCount >= ReversalBars)
                {
                    firedZone          = z;
                    z.PriceTouchedZone = false;
                    z.BarsOutsideCount = 0;
                }
            }
            else
            {
                z.BarsOutsideCount = 0;
                z.PriceTouchedZone = false;
            }
        }
    }

    // Hard rule: if price is inside ANY zone, signal must be zero
    if (IsInsideAnyZone(closeNow))
    {
        SignalState = 0;
        return;
    }

    // Only after clearing ALL zones can a new signal be stamped in
    if (firedZone != null)
    {
        SignalState = firedZone.IsDemand ? 1 : -1;

        if (DrawZoneArrows)
            FireSignalArrow(firedZone);
    }

    // If no new signal fired, keep the old persistent state,
    // but still clear it if now near opposite zone
    CheckOppositeZoneClear();
}



			private void FireSignalArrow(Zone zone)
			{
			    string sigTag = "SIG_" + zone.Tag + "_" + CurrentBar;
			
			    if (zone.IsDemand && SignalState == 1)
			        Draw.ArrowUp(this, sigTag, false, 0,
			            Low[0] - 2 * TickSize, BuySignalColor);
			    else if (!zone.IsDemand && SignalState == -1)
			        Draw.ArrowDown(this, sigTag, false, 0,
			            High[0] + 2 * TickSize, SellSignalColor);
			}


      private void FireSignal(Zone zone)
			{	
			    SignalState = zone.IsDemand ? 1 : -1;
			
			    CheckOppositeZoneClear();
			
			    if (SignalState == 0)
			        return;
			
			    string sigTag = "SIG_" + zone.Tag + "_" + CurrentBar;
			
			    if (SignalState == 1)
			    {
			        Draw.ArrowUp(this, sigTag, false, 0,
			            Low[0] - 2 * TickSize, BuySignalColor);
			
			        Values[0][0] = 1;   // stamp the signal onto this bar
			    }
			    else if (SignalState == -1)
			    {
			        Draw.ArrowDown(this, sigTag, false, 0,
			            High[0] + 2 * TickSize, SellSignalColor);
			
			        Values[0][0] = -1;  // stamp the signal onto this bar
			    }
			}


        private void CheckOppositeZoneClear()
        {
            if (SignalState == 0)
                return;

            double clearDist = OppositeZoneClearTicks * TickSize;
            double price     = Close[0];

            foreach (Zone z in _zones)
            {
                bool isOpposite = (SignalState == 1  && !z.IsDemand)
                               || (SignalState == -1 &&  z.IsDemand);
                if (!isOpposite)
                    continue;

                if (price >= z.Bottom - clearDist && price <= z.Top + clearDist)
                {
                    SignalState = 0;
                    return;
                }
            }
        }

        private void DrawZone(Zone zone)
        {
            Color fillColor = zone.IsDemand
                ? ((SolidColorBrush)DemandColor).Color
                : ((SolidColorBrush)SupplyColor).Color;

            byte opacity = (byte)(ZoneOpacity / 100.0 * 255);

            var fillBrush = new SolidColorBrush(
                Color.FromArgb(opacity, fillColor.R, fillColor.G, fillColor.B));

            Draw.Rectangle(this, zone.Tag, true,
                zone.StartTime,
                zone.Top,
                DateTime.Now.AddYears(10),
                zone.Bottom,
                zone.IsDemand ? DemandColor : SupplyColor,
                fillBrush,
                ZoneOpacity);
        }

        #region Properties

        [NinjaScriptProperty]
        [Range(2, 20)]
        [Display(Name = "Consecutive Bars", Order = 1, GroupName = "Zone Settings")]
        public int ConsecutiveBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, 500)]
        [Display(Name = "Break Ticks", Order = 2, GroupName = "Zone Settings")]
        public int BreakTicks { get; set; }

        [NinjaScriptProperty]
        [Range(1, 20)]
        [Display(Name = "Reversal Bars", Order = 3, GroupName = "Zone Settings")]
        public int ReversalBars { get; set; }

        [NinjaScriptProperty]
        [Range(1, 500)]
        [Display(Name = "Opposite Zone Clear Ticks", Order = 4, GroupName = "Zone Settings")]
        public int OppositeZoneClearTicks { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Draw Zone Arrows", Order = 5, GroupName = "Zone Settings")]
        public bool DrawZoneArrows { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Demand Color", Order = 1, GroupName = "Visual Settings")]
        public Brush DemandColor { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Supply Color", Order = 2, GroupName = "Visual Settings")]
        public Brush SupplyColor { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Buy Signal Color", Order = 3, GroupName = "Visual Settings")]
        public Brush BuySignalColor { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Sell Signal Color", Order = 4, GroupName = "Visual Settings")]
        public Brush SellSignalColor { get; set; }

        [NinjaScriptProperty]
        [Range(5, 100)]
        [Display(Name = "Zone Opacity %", Order = 5, GroupName = "Visual Settings")]
        public int ZoneOpacity { get; set; }
		
		[Browsable(false)]
		[System.Xml.Serialization.XmlIgnore]
		public Series<double> SignalStatePlot
		{
		    get { return Values[0]; }
		}
		
		
		[NinjaScriptProperty]
[Range(1, 20)]
[Display(Name = "Swing Strength", Order = 1, GroupName = "Zone Settings")]
public int SwingStrength { get; set; }

[NinjaScriptProperty]
[Range(1, 100)]
[Display(Name = "Zone Merge Ticks", Order = 6, GroupName = "Zone Settings")]
public int ZoneMergeTicks { get; set; }



        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SupplyDemandZones[] cacheSupplyDemandZones;
		public SupplyDemandZones SupplyDemandZones(int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength, int zoneMergeTicks)
		{
			return SupplyDemandZones(Input, consecutiveBars, breakTicks, reversalBars, oppositeZoneClearTicks, drawZoneArrows, demandColor, supplyColor, buySignalColor, sellSignalColor, zoneOpacity, swingStrength, zoneMergeTicks);
		}

		public SupplyDemandZones SupplyDemandZones(ISeries<double> input, int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength, int zoneMergeTicks)
		{
			if (cacheSupplyDemandZones != null)
				for (int idx = 0; idx < cacheSupplyDemandZones.Length; idx++)
					if (cacheSupplyDemandZones[idx] != null && cacheSupplyDemandZones[idx].ConsecutiveBars == consecutiveBars && cacheSupplyDemandZones[idx].BreakTicks == breakTicks && cacheSupplyDemandZones[idx].ReversalBars == reversalBars && cacheSupplyDemandZones[idx].OppositeZoneClearTicks == oppositeZoneClearTicks && cacheSupplyDemandZones[idx].DrawZoneArrows == drawZoneArrows && cacheSupplyDemandZones[idx].DemandColor == demandColor && cacheSupplyDemandZones[idx].SupplyColor == supplyColor && cacheSupplyDemandZones[idx].BuySignalColor == buySignalColor && cacheSupplyDemandZones[idx].SellSignalColor == sellSignalColor && cacheSupplyDemandZones[idx].ZoneOpacity == zoneOpacity && cacheSupplyDemandZones[idx].SwingStrength == swingStrength && cacheSupplyDemandZones[idx].ZoneMergeTicks == zoneMergeTicks && cacheSupplyDemandZones[idx].EqualsInput(input))
						return cacheSupplyDemandZones[idx];
			return CacheIndicator<SupplyDemandZones>(new SupplyDemandZones(){ ConsecutiveBars = consecutiveBars, BreakTicks = breakTicks, ReversalBars = reversalBars, OppositeZoneClearTicks = oppositeZoneClearTicks, DrawZoneArrows = drawZoneArrows, DemandColor = demandColor, SupplyColor = supplyColor, BuySignalColor = buySignalColor, SellSignalColor = sellSignalColor, ZoneOpacity = zoneOpacity, SwingStrength = swingStrength, ZoneMergeTicks = zoneMergeTicks }, input, ref cacheSupplyDemandZones);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SupplyDemandZones SupplyDemandZones(int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength, int zoneMergeTicks)
		{
			return indicator.SupplyDemandZones(Input, consecutiveBars, breakTicks, reversalBars, oppositeZoneClearTicks, drawZoneArrows, demandColor, supplyColor, buySignalColor, sellSignalColor, zoneOpacity, swingStrength, zoneMergeTicks);
		}

		public Indicators.SupplyDemandZones SupplyDemandZones(ISeries<double> input , int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength, int zoneMergeTicks)
		{
			return indicator.SupplyDemandZones(input, consecutiveBars, breakTicks, reversalBars, oppositeZoneClearTicks, drawZoneArrows, demandColor, supplyColor, buySignalColor, sellSignalColor, zoneOpacity, swingStrength, zoneMergeTicks);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SupplyDemandZones SupplyDemandZones(int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength, int zoneMergeTicks)
		{
			return indicator.SupplyDemandZones(Input, consecutiveBars, breakTicks, reversalBars, oppositeZoneClearTicks, drawZoneArrows, demandColor, supplyColor, buySignalColor, sellSignalColor, zoneOpacity, swingStrength, zoneMergeTicks);
		}

		public Indicators.SupplyDemandZones SupplyDemandZones(ISeries<double> input , int consecutiveBars, int breakTicks, int reversalBars, int oppositeZoneClearTicks, bool drawZoneArrows, Brush demandColor, Brush supplyColor, Brush buySignalColor, Brush sellSignalColor, int zoneOpacity, int swingStrength, int zoneMergeTicks)
		{
			return indicator.SupplyDemandZones(input, consecutiveBars, breakTicks, reversalBars, oppositeZoneClearTicks, drawZoneArrows, demandColor, supplyColor, buySignalColor, sellSignalColor, zoneOpacity, swingStrength, zoneMergeTicks);
		}
	}
}

#endregion
