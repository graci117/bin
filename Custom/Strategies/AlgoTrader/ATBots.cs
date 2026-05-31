#region Using declarations
using System;
using System.Collections.Generic;
using System.Linq;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Indicators.AlgoTrader;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.NinjaScript.Indicators.FxStill.SmartMoney;
using NinjaTrader.NinjaScript.Indicators.ZombiePack9;
using NinjaTrader.NinjaScript.Indicators.TradeSaber;
using NinjaTrader.NinjaScript.Indicators.TradeSaber_SignalMod;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{
	public abstract partial class AlgoBase : Strategy
    {
	    #region Bot Framework Interface and Enum
	    public enum SignalDirection { NoSignal, Long, Short }
	
	    public interface ISignalBot
	    {
	        string Name { get; }
	        AlgoBase.MarketRegime Regime { get; }
	        void Initialize(AlgoBase strategy);
	        SignalDirection CheckSignal(int barsAgo);
	    }
	    #endregion
		
		private void InitializeBotFramework()
		{
			allBots = new List<ISignalBot>
			{
				// --- Universal Bots (Adapted to KingKhanh Momentum Logic) ---
				new PivotImpulseBot(),
				new KingKhanhBot(),
				new MomentumExtremesBot(),
				new RangeFilteredBot(),
				new DeviationTrendProfileBot(),
				new TMOBot(),
                new MomentumVmaBot(),
				new HiLoBandsBot(),
				new LinRegBandsBot(),
				new HookerBot(),
				new SuperTrendBot(),
				new Johny5Bot(),
				new MagicTrendyBot(),
				new CoralBot(),
				new BalaBot(),
				new TrendSniperBot(),
				
				// --- Trend Bots ---
				new MomoBot(),
				new SwingStructureBot(),
				new TrendArchitectBot(),

				// --- Range Bots ---
				new VolumeThrustBot(),
				new WillyBot(),
				new SmartMoneyBot(),
					
				// --- Breakout Bots ---				
				new BollingerBot(),
				new KeltnerBot(), 
			};
			
			foreach(var bot in allBots)
			{
				bot.Initialize(this);
			}
			
			trendBots = allBots.Where(b => b.Regime == MarketRegime.Trending).ToList();
			rangeBots = allBots.Where(b => b.Regime == MarketRegime.Ranging).ToList();
			breakoutBots = allBots.Where(b => b.Regime == MarketRegime.Breakout).ToList();
            universalBots = allBots.Where(b => b.Regime == MarketRegime.Undefined || b.Regime == MarketRegime.Universal).ToList();
		}
	
	    #region --- UNIVERSAL BOTS (MOMENTUM LOGIC) ---
		
		public class PivotImpulseBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "PivotImpulse";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnablePivotImpulseBot || s.botPivotImpulse == null) return SignalDirection.NoSignal;
		        if (s.botPivotImpulse.IsImpulseUp[0] && !s.botPivotImpulse.IsImpulseUp[1]) return SignalDirection.Long;
		        if (!s.botPivotImpulse.IsImpulseUp[0] && s.botPivotImpulse.IsImpulseUp[1]) return SignalDirection.Short;
		        return SignalDirection.NoSignal;
		    }
		}
		
	    public class KingKhanhBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "KingKhanh";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		    
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        // ADDED NULL CHECK HERE
		        if (!s.EnableKingKhanh || s.botRegressionBands == null || !s.botRegressionBands.IsValidDataPoint(1))
		            return SignalDirection.NoSignal;
		
		        Series<double>[] longLines = { s.botRegressionBands.UpperBand2, s.botRegressionBands.UpperBand1, s.botRegressionBands.RegressionLine, s.botRegressionBands.LowerBand1, s.botRegressionBands.LowerBand2, s.botRegressionBands.LowerBand3 };
		        foreach (var line in longLines) 
		            if (line != null && s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;
		
		        Series<double>[] shortLines = { s.botRegressionBands.UpperBand3, s.botRegressionBands.UpperBand2, s.botRegressionBands.UpperBand1, s.botRegressionBands.RegressionLine, s.botRegressionBands.LowerBand1, s.botRegressionBands.LowerBand2 };
		        foreach (var line in shortLines) 
		            if (line != null && s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
		        
		        return SignalDirection.NoSignal;
		    }
		}

		public class MomentumExtremesBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "MomExtremes (Universal)";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	
	        public SignalDirection CheckSignal(int barsAgo)
	        {
	            if (!s.EnableMomentumExtremesBot || s.botMomentumExtremesBands == null || !s.botMomentumExtremesBands.IsValidDataPoint(1))
	                return SignalDirection.NoSignal;
	
				Series<double>[] longLines = { s.botMomentumExtremesBands.UpperBand2, s.botMomentumExtremesBands.UpperBand1, s.botMomentumExtremesBands.MEDLine, s.botMomentumExtremesBands.LowerBand1, s.botMomentumExtremesBands.LowerBand2, s.botMomentumExtremesBands.LowerBand3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botMomentumExtremesBands.UpperBand3, s.botMomentumExtremesBands.UpperBand2, s.botMomentumExtremesBands.UpperBand1, s.botMomentumExtremesBands.MEDLine, s.botMomentumExtremesBands.LowerBand1, s.botMomentumExtremesBands.LowerBand2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
	
	            return SignalDirection.NoSignal;
	        }
	    }
		
		public class RangeFilteredBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "RangeFiltered (Optimized)";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnableRangeFilteredBot || s.botRangeFiltered == null || !s.botRangeFiltered.IsValidDataPoint(1))
		            return SignalDirection.NoSignal;
		
		        int[] longIndices = { 0, 9, 11, 13, 15, 17, 19 };
		        foreach (int i in longIndices) if (s.CrossAbove(s.Close, s.botRangeFiltered.Values[i], 1)) return SignalDirection.Long;
		
		        int[] shortIndices = { 0, 8, 10, 12, 14, 16, 18 };
		        foreach (int i in shortIndices) if (s.CrossBelow(s.Close, s.botRangeFiltered.Values[i], 1)) return SignalDirection.Short;
		
		        return SignalDirection.NoSignal;
		    }
		}

		public class DeviationTrendProfileBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "DevTrendProfile (Universal)";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        // ADDED NULL CHECK HERE
		        if (!s.EnableDeviationTrendProfileBot || s.botDeviationTrendProfile == null || !s.botDeviationTrendProfile.IsValidDataPoint(1))
		            return SignalDirection.NoSignal;
		
		        Series<double>[] longLines = { s.botDeviationTrendProfile.Stdv2, s.botDeviationTrendProfile.Stdv1, s.botDeviationTrendProfile.Avg, s.botDeviationTrendProfile.Stdv_1, s.botDeviationTrendProfile.Stdv_2, s.botDeviationTrendProfile.Stdv_3 };
		        foreach (var line in longLines) 
		            if (line != null && s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;
		
		        Series<double>[] shortLines = { s.botDeviationTrendProfile.Stdv3, s.botDeviationTrendProfile.Stdv2, s.botDeviationTrendProfile.Stdv1, s.botDeviationTrendProfile.Avg, s.botDeviationTrendProfile.Stdv_1, s.botDeviationTrendProfile.Stdv_2 };
		        foreach (var line in shortLines) 
		            if (line != null && s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
		
		        return SignalDirection.NoSignal;
		    }
		}
		
        public class TMOBot : ISignalBot
        {
            private AlgoBase s;
            public string Name => "TMO Bot";
            public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
            public void Initialize(AlgoBase strategy) { s = strategy; }

            public SignalDirection CheckSignal(int barsAgo)
            {
                if (!s.EnableTMOBot || s.botTMO == null || !s.botTMO.IsValidDataPoint(1)) return SignalDirection.NoSignal;
                if (s.CrossAbove(s.botTMO.Main, s.botTMO.Signal, 1) || s.CrossAbove(s.botTMO.Main, s.TMOOversoldLevel, 1)) return SignalDirection.Long;
                if (s.CrossBelow(s.botTMO.Main, s.botTMO.Signal, 1) || s.CrossBelow(s.botTMO.Main, s.TMOOverboughtLevel, 1)) return SignalDirection.Short;
                return SignalDirection.NoSignal;
            }
        }
		
        public class MomentumVmaBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "MomVMA (Universal)";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	
	        public SignalDirection CheckSignal(int barsAgo)
	        {
	            if (!s.EnableMomentumVmaBot || s.botMomentumVmaBands == null || !s.botMomentumVmaBands.IsValidDataPoint(1))
	                return SignalDirection.NoSignal;
	
				Series<double>[] longLines = { s.botMomentumVmaBands.UpperBand2, s.botMomentumVmaBands.UpperBand1, s.botMomentumVmaBands.VmaLine, s.botMomentumVmaBands.LowerBand1, s.botMomentumVmaBands.LowerBand2, s.botMomentumVmaBands.LowerBand3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botMomentumVmaBands.UpperBand3, s.botMomentumVmaBands.UpperBand2, s.botMomentumVmaBands.UpperBand1, s.botMomentumVmaBands.VmaLine, s.botMomentumVmaBands.LowerBand1, s.botMomentumVmaBands.LowerBand2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
	
	            return SignalDirection.NoSignal;
	        }
	    }
		
		public class HiLoBandsBot : ISignalBot
		{
			private AlgoBase s;
			public string Name => "HiLoBands";
			public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
			public void Initialize(AlgoBase strategy) { s = strategy; }

			public SignalDirection CheckSignal(int barsAgo)
			{
				if (!s.EnableHiLoBandsBot || s.botHiLoBands == null || !s.botHiLoBands.IsValidDataPoint(1))
					return SignalDirection.NoSignal;

				Series<double>[] longLines = { s.botHiLoBands.Values[4], s.botHiLoBands.Values[3], s.botHiLoBands.Values[2], s.botHiLoBands.Values[6], s.botHiLoBands.Values[7], s.botHiLoBands.Values[8] };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botHiLoBands.Values[0], s.botHiLoBands.Values[5], s.botHiLoBands.Values[4], s.botHiLoBands.Values[3], s.botHiLoBands.Values[2], s.botHiLoBands.Values[6] };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
				
				return SignalDirection.NoSignal;
			}
		}

		public class LinRegBandsBot : ISignalBot
		{
			private AlgoBase s;
			public string Name => "LinRegBands (Universal)";
			public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
			public void Initialize(AlgoBase strategy) { s = strategy; }

			public SignalDirection CheckSignal(int barsAgo)
			{
				if (!s.EnableLinRegBandsBot || s.botLinRegBands == null || !s.botLinRegBands.IsValidDataPoint(1))
					return SignalDirection.NoSignal;

				Series<double>[] longLines = { s.botLinRegBands.Upper2, s.botLinRegBands.Upper1, s.botLinRegBands.MainLinReg, s.botLinRegBands.Lower1, s.botLinRegBands.Lower2, s.botLinRegBands.Lower3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botLinRegBands.Upper3, s.botLinRegBands.Upper2, s.botLinRegBands.Upper1, s.botLinRegBands.MainLinReg, s.botLinRegBands.Lower1, s.botLinRegBands.Lower2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;

				return SignalDirection.NoSignal;
			}
		}

		public class HookerBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "HMA Hooks (Universal)";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	
	        public SignalDirection CheckSignal(int barsAgo)
	        {
	            if (!s.EnableHooker || s.botHmaAtrBands == null || !s.botHmaAtrBands.IsValidDataPoint(1))
	                return SignalDirection.NoSignal;
	
				Series<double>[] longLines = { s.botHmaAtrBands.Upper2, s.botHmaAtrBands.Upper1, s.botHmaAtrBands.MainHMA, s.botHmaAtrBands.Lower1, s.botHmaAtrBands.Lower2, s.botHmaAtrBands.Lower3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botHmaAtrBands.Upper3, s.botHmaAtrBands.Upper2, s.botHmaAtrBands.Upper1, s.botHmaAtrBands.MainHMA, s.botHmaAtrBands.Lower1, s.botHmaAtrBands.Lower2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
	
	            return SignalDirection.NoSignal;
	        }
	    }

	    public class CoralBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "Coral (Universal)";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnableCoralBot || s.botCoralBands == null || !s.botCoralBands.IsValidDataPoint(1))
		            return SignalDirection.NoSignal;
		
				Series<double>[] longLines = { s.botCoralBands.Upper2, s.botCoralBands.Upper1, s.botCoralBands.CoralTrendValue, s.botCoralBands.Lower1, s.botCoralBands.Lower2, s.botCoralBands.Lower3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botCoralBands.Upper3, s.botCoralBands.Upper2, s.botCoralBands.Upper1, s.botCoralBands.CoralTrendValue, s.botCoralBands.Lower1, s.botCoralBands.Lower2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
		
		        return SignalDirection.NoSignal;
		    }
		}
	
		public class Johny5Bot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "Johny5 (Universal)";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	
	        public SignalDirection CheckSignal(int barsAgo)
	        {
	            if (!s.EnableJohny5 || s.botJbSignalBands == null || !s.botJbSignalBands.IsValidDataPoint(1))
	                return SignalDirection.NoSignal;
	
	            double internalMomentum = s.botJbSignalBands.StrategySignal[0];
	
				Series<double>[] longLines = { s.botJbSignalBands.Upper2, s.botJbSignalBands.Upper1, s.botJbSignalBands.Midline, s.botJbSignalBands.Lower1, s.botJbSignalBands.Lower2, s.botJbSignalBands.Lower3 };
				foreach (var line in longLines) 
					if (s.CrossAbove(s.Close, line, 1) && internalMomentum >= 0) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botJbSignalBands.Upper3, s.botJbSignalBands.Upper2, s.botJbSignalBands.Upper1, s.botJbSignalBands.Midline, s.botJbSignalBands.Lower1, s.botJbSignalBands.Lower2 };
				foreach (var line in shortLines) 
					if (s.CrossBelow(s.Close, line, 1) && internalMomentum <= 0) return SignalDirection.Short;
	
	            return SignalDirection.NoSignal;
	        }
	    }
		
		 public class BalaBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "Bala (Universal)";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	
	        public SignalDirection CheckSignal(int barsAgo)
	        {
	            if (!s.EnableBalaBot || s.botBalaBands == null || !s.botBalaBands.IsValidDataPoint(1))
	                return SignalDirection.NoSignal;
	
				Series<double>[] longLines = { s.botBalaBands.Upper2, s.botBalaBands.Upper1, s.botBalaBands.BalaValue, s.botBalaBands.Lower1, s.botBalaBands.Lower2, s.botBalaBands.Lower3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botBalaBands.Upper3, s.botBalaBands.Upper2, s.botBalaBands.Upper1, s.botBalaBands.BalaValue, s.botBalaBands.Lower1, s.botBalaBands.Lower2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
	
	            return SignalDirection.NoSignal;
	        }
	    }
		
		public class MagicTrendyBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "TrendMagic (Universal)";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnableMagicTrendy || s.botTrendMagicBands == null || !s.botTrendMagicBands.IsValidDataPoint(1))
		            return SignalDirection.NoSignal;
		
				Series<double>[] longLines = { s.botTrendMagicBands.Upper2, s.botTrendMagicBands.Upper1, s.botTrendMagicBands.TrendMagicValue, s.botTrendMagicBands.Lower1, s.botTrendMagicBands.Lower2, s.botTrendMagicBands.Lower3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botTrendMagicBands.Upper3, s.botTrendMagicBands.Upper2, s.botTrendMagicBands.Upper1, s.botTrendMagicBands.TrendMagicValue, s.botTrendMagicBands.Lower1, s.botTrendMagicBands.Lower2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
		
		        return SignalDirection.NoSignal;
		    }
		}
		
		public class SuperTrendBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "SuperTrend (Universal)";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	
	        public SignalDirection CheckSignal(int barsAgo)
	        {
	            if (!s.EnableSuperTrendBot || s.botSuperTrendBands == null || !s.botSuperTrendBands.IsValidDataPoint(1))
	                return SignalDirection.NoSignal;
	
				Series<double>[] longLines = { s.botSuperTrendBands.Upper2, s.botSuperTrendBands.Upper1, s.botSuperTrendBands.SuperSmoother, s.botSuperTrendBands.Lower1, s.botSuperTrendBands.Lower2, s.botSuperTrendBands.Lower3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botSuperTrendBands.Upper3, s.botSuperTrendBands.Upper2, s.botSuperTrendBands.Upper1, s.botSuperTrendBands.SuperSmoother, s.botSuperTrendBands.Lower1, s.botSuperTrendBands.Lower2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
	
	            return SignalDirection.NoSignal;
	        }
	    }
		
		public class TrendSniperBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "TrendSniper (Universal)";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Universal;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnableTrendSniperBot || s.botTrendSniper == null || !s.botTrendSniper.IsValidDataPoint(1))
		            return SignalDirection.NoSignal;
		
				Series<double>[] longLines = { s.botTrendSniper.Upper2, s.botTrendSniper.Upper1, s.botTrendSniper.Midline, s.botTrendSniper.Lower1, s.botTrendSniper.Lower2, s.botTrendSniper.Lower3 };
				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

				Series<double>[] shortLines = { s.botTrendSniper.Upper3, s.botTrendSniper.Upper2, s.botTrendSniper.Upper1, s.botTrendSniper.Midline, s.botTrendSniper.Lower1, s.botTrendSniper.Lower2 };
				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
		
		        return SignalDirection.NoSignal;
		    }
		}
		
//		public class BollingerBot : ISignalBot
//		{
//		    private AlgoBase s;
//		    public string Name => "Bollinger (Breakout)";
//		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Breakout; 
//		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
//		    public SignalDirection CheckSignal(int barsAgo)
//		    {
//		        if (!s.EnableBollingerBot || s.botBollingerUniversal == null || !s.botBollingerUniversal.IsValidDataPoint(1))
//		            return SignalDirection.NoSignal;
		
//				Series<double>[] longLines = { s.botBollingerUniversal.Upper3, s.botBollingerUniversal.Upper2, s.botBollingerUniversal.Upper1, s.botBollingerUniversal.MainSMA, s.botBollingerUniversal.Lower1, s.botBollingerUniversal.Lower2 };
//				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

//				Series<double>[] shortLines = { s.botBollingerUniversal.Upper2, s.botBollingerUniversal.Upper1, s.botBollingerUniversal.MainSMA, s.botBollingerUniversal.Lower1, s.botBollingerUniversal.Lower2, s.botBollingerUniversal.Lower3 };
//				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
		
//		        return SignalDirection.NoSignal;
//		    }
//		}

//		public class KeltnerBot : ISignalBot
//		{
//		    private AlgoBase s;
//		    public string Name => "Keltner (Breakout)";
//		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Breakout;
//		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
//		    public SignalDirection CheckSignal(int barsAgo)
//		    {
//		        if (!s.EnableKeltnerBot || s.botKeltner == null || !s.botKeltner.IsValidDataPoint(1))
//		            return SignalDirection.NoSignal;
		
//				Series<double>[] longLines = { s.botKeltner.Upper3, s.botKeltner.Upper2, s.botKeltner.Upper1, s.botKeltner.MainMidline, s.botKeltner.Lower1, s.botKeltner.Lower2 };
//				foreach (var line in longLines) if (s.CrossAbove(s.Close, line, 1)) return SignalDirection.Long;

//				Series<double>[] shortLines = { s.botKeltner.Upper2, s.botKeltner.Upper1, s.botKeltner.MainMidline, s.botKeltner.Lower1, s.botKeltner.Lower2, s.botKeltner.Lower3 };
//				foreach (var line in shortLines) if (s.CrossBelow(s.Close, line, 1)) return SignalDirection.Short;
		
//		        return SignalDirection.NoSignal;
//		    }
//		}

	    #endregion
	
	    #region --- TREND BOTS ---
		
	    public class MomoBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "Momo";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Trending;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	        public SignalDirection CheckSignal(int barsAgo)
	        {
				if (!s.EnableMomo || s.botMomentum == null) return SignalDirection.NoSignal;
	            if (s.CrossAbove(s.botMomentum, s.MomoThreshold, 1)) return SignalDirection.Long;
	            if (s.CrossBelow(s.botMomentum, -s.MomoThreshold, 1)) return SignalDirection.Short;
	            return SignalDirection.NoSignal;
	        }
	    }
	    
	    public class SwingStructureBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "SwingStructure";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Trending;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		    
		    public SignalDirection CheckSignal(int barsAgo)
		    {
				if (!s.EnableSwingStructureBot || s.botSwing == null) return SignalDirection.NoSignal;
		
		        if (s.IsTrendingUp())
		        {
		            int barsAgoOfRecentLow = s.botSwing.SwingLowBar(1, 1, 200);
		            int barsAgoOfPriorLow  = s.botSwing.SwingLowBar(1, 2, 200);
		            if (barsAgoOfRecentLow > 0 && barsAgoOfPriorLow > barsAgoOfRecentLow)
		            {
		                if (s.botSwing.SwingLow[barsAgoOfRecentLow] > s.botSwing.SwingLow[barsAgoOfPriorLow])
		                    if (s.Close[0] > s.High[barsAgoOfRecentLow]) return SignalDirection.Long;
		            }
		        }
		
		        if (s.IsTrendingDown())
		        {
		            int barsAgoOfRecentHigh = s.botSwing.SwingHighBar(1, 1, 200);
		            int barsAgoOfPriorHigh  = s.botSwing.SwingHighBar(1, 2, 200);
		            if (barsAgoOfRecentHigh > 0 && barsAgoOfPriorHigh > barsAgoOfRecentHigh)
		            {
		                if (s.botSwing.SwingHigh[barsAgoOfRecentHigh] < s.botSwing.SwingHigh[barsAgoOfPriorHigh])
		                    if (s.Close[0] < s.Low[barsAgoOfRecentHigh]) return SignalDirection.Short;
		            }
		        }
		        return SignalDirection.NoSignal;
		    }
		}
	
		public class TrendArchitectBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "TrendArchitect";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Trending;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		    
		    public SignalDirection CheckSignal(int barsAgo)
		    {
				if (!s.EnableTrendArchitectBot || s.botTrendArchitect == null) return SignalDirection.NoSignal;
		        if (s.botTrendArchitect.BuySignalScore[0] >= 0.65 && s.botTrendArchitect.BuySignalScore[1] < 0.65) return SignalDirection.Long;
		        if (s.botTrendArchitect.SellSignalScore[0] >= 0.65 && s.botTrendArchitect.SellSignalScore[1] < 0.65) return SignalDirection.Short;
		        return SignalDirection.NoSignal;
		    }
		}

	    #endregion
		
		#region --- BREAKOUT BOTS ---
		
	    public class BollingerBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "BollingerBot";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Breakout; 
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnableBollingerBot || s.botBollingerUniversal == null || !s.botBollingerUniversal.IsValidDataPoint(1))
		            return SignalDirection.NoSignal;
		
                // FIXED: Changed UpperBand2 to Upper2 and LowerBand2 to Lower2
                // TRIGGER: Price CLOSES above Level 2 (Standard Deviation 2.0)
                bool longTrigger  = s.Close[0] > s.botBollingerUniversal.Upper2[0] && s.Close[1] <= s.botBollingerUniversal.Upper2[0];
                bool shortTrigger = s.Close[0] < s.botBollingerUniversal.Lower2[0] && s.Close[1] >= s.botBollingerUniversal.Lower2[0];

                // FIXED: Changed UpperBand4 to Upper4 and LowerBand4 to Lower4
                // CONFIRMATION: Volatility Expansion via Level 4 Slope
                bool upperExpanding = s.botBollingerUniversal.Upper4[0] > s.botBollingerUniversal.Upper4[1];
                bool lowerExpanding = s.botBollingerUniversal.Lower4[0] < s.botBollingerUniversal.Lower4[1];

                if (longTrigger && upperExpanding) 
                    return SignalDirection.Long;

                if (shortTrigger && lowerExpanding) 
                    return SignalDirection.Short;
		
		        return SignalDirection.NoSignal;
		    }
		}

		public class KeltnerBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "KeltnerBot";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Breakout;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnableKeltnerBot || s.botKeltner == null || !s.botKeltner.IsValidDataPoint(2))
		            return SignalDirection.NoSignal;
		
                // FIXED: Changed UpperBand3 to Upper3 and LowerBand3 to Lower3
                // TRIGGER: Close above Keltner Level 3 (typically 2.0 ATR based on your multiplier)
                bool longTrigger  = s.Close[0] > s.botKeltner.Upper3[0] && s.Close[1] <= s.botKeltner.Upper3[0];
                bool shortTrigger = s.Close[0] < s.botKeltner.Lower3[0] && s.Close[1] >= s.botKeltner.Lower3[0];

                // CONFIRMATION 1: "Walking the Bands"
                bool isWalkingLong  = s.High[0] > s.High[1] && s.High[1] > s.High[2];
                bool isWalkingShort = s.Low[0] < s.Low[1] && s.Low[1] < s.Low[2];

                // CONFIRMATION 2: Velocity Check (Power Bar)
                double currentBody = Math.Abs(s.Close[0] - s.Open[0]);
                double prevBody    = Math.Abs(s.Close[1] - s.Open[1]);

                if (longTrigger && isWalkingLong && currentBody >= prevBody) 
                    return SignalDirection.Long;

                if (shortTrigger && isWalkingShort && currentBody >= prevBody) 
                    return SignalDirection.Short;
		
		        return SignalDirection.NoSignal;
		    }
		}
		#endregion
		
	    #region --- RANGE BOTS ---
	
		public class VolumeThrustBot : ISignalBot
		{
		    private AlgoBase s;
		    public string Name => "VolumeThrust";
		    public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Ranging;
		    public void Initialize(AlgoBase strategy) { s = strategy; }
		
		    public SignalDirection CheckSignal(int barsAgo)
		    {
		        if (!s.EnableVolumeThrustBot || s.botVolumeUpDown == null) return SignalDirection.NoSignal;
		        double upVol = s.botVolumeUpDown.UpVolume[barsAgo];
		        double downVol = s.botVolumeUpDown.DownVolume[barsAgo];
		        if (upVol > 0 && downVol > 0)
		        {
		            if (s.botVolumeUpDown.UpVolume[0] > s.botVolumeUpDown.DownVolume[0] && s.botVolumeUpDown.DownVolume[1] > s.botVolumeUpDown.UpVolume[1]) return SignalDirection.Long;
		            if (s.botVolumeUpDown.DownVolume[0] > s.botVolumeUpDown.UpVolume[0] && s.botVolumeUpDown.UpVolume[1] > s.botVolumeUpDown.DownVolume[1]) return SignalDirection.Short;
		        }
		        else
		        {
		            if (upVol > 0 && downVol == 0) return SignalDirection.Long;
		            if (downVol > 0 && upVol == 0) return SignalDirection.Short;
		        }
		        return SignalDirection.NoSignal;
		    }
		}
		
	    public class WillyBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "Willy";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Ranging;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	        public SignalDirection CheckSignal(int barsAgo)
	        {
				if (!s.EnableWilly || s.botWilly == null) return SignalDirection.NoSignal;
	            if (s.CrossAbove(s.botWilly, s.wrDown, 1)) return SignalDirection.Long;
	            if (s.CrossBelow(s.botWilly, s.wrUp, 1)) return SignalDirection.Short;
	            return SignalDirection.NoSignal;
	        }
	    }
	
	    public class SmartMoneyBot : ISignalBot
	    {
	        private AlgoBase s;
	        public string Name => "SmartMoney";
	        public AlgoBase.MarketRegime Regime => AlgoBase.MarketRegime.Ranging;
	        public void Initialize(AlgoBase strategy) { s = strategy; }
	        public SignalDirection CheckSignal(int barsAgo)
	        {
				if (!s.EnableSmartMoneyBot || s.botMarketStructure == null || s.CurrentBar < s.MarketStructurePeriod) return SignalDirection.NoSignal;
	            if (s.botMarketStructure.Signal[0] == 1 && s.botMarketStructure.Signal[1] != 1) return SignalDirection.Long;
	            if (s.botMarketStructure.Signal[0] == -1 && s.botMarketStructure.Signal[1] != -1) return SignalDirection.Short;
	            return SignalDirection.NoSignal;
	        }
	    }
	    #endregion
	}
}