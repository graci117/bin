#region Using declarations
using System;
using System.IO;
using System.Linq;
using System.Text;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
#endregion

namespace NinjaTrader.NinjaScript.Strategies.AlgoTrader
{ 
	public abstract partial class AlgoBase : Strategy
	{
	    // --- JSON Logger Components ---
	    public class JsonTradeEntry
	    {
	        public string EntryType => "Trade";
	        public string Strategy { get; set; }
	        public string Instrument { get; set; }
	        public string Account { get; set; }
	        public int TradeNumber { get; set; }
	        public DateTime EntryTime { get; set; }
	        public MarketPosition Direction { get; set; }
	        public double EntryPrice { get; set; }
	        public long Quantity { get; set; }
	        public DateTime ExitTime { get; set; }
	        public double ExitPrice { get; set; }
	        public string ExitName { get; set; }
	        public double ProfitTicks { get; set; }
	        public double ProfitCurrency { get; set; }
	        public double Commission { get; set; }
	        public double MfeTicks { get; set; }
	        public double MaeTicks { get; set; }
	        public string MarketRegime { get; set; }
	        public string SignalSource { get; set; }
	        public string MasterFilter { get; set; }
	        public string StopType { get; set; }
	        public string ProfitType { get; set; }
	        public double ConfluenceScore { get; set; }
			public double ADX_at_Entry { get; set; }
	        public double TMO_at_Entry { get; set; }
			public double UpVolume_at_Entry { get; set; }
			public double DownVolume_at_Entry { get; set; }
	        public double AdxAtExit { get; set; }
	        public double AtrAtExit { get; set; }
	        public double TMO_at_Exit { get; set; }
	        public int BarsInTrade { get; set; }
	        public double InitialSLTicks { get; set; }
	        public double InitialTPTicks { get; set; }
	        public double BeTriggerTicks { get; set; }
			public double DynamicBeTriggerTicks { get; set; }
	        public double SlippageTicks { get; set; }
            public double CalmarRatio { get; set; } // ADDED FOR CALMAR
	    }
	
	    public class TradeJsonLogger
	    {
	        private readonly string filePath;
	        private static readonly object fileLock = new object();
	
	        public TradeJsonLogger(string fullFilePath)
	        {
	            filePath = fullFilePath;
	            string directory = Path.GetDirectoryName(filePath);
	            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
	            {
	                Directory.CreateDirectory(directory);
	            }
	        }
	
	        private string ToJsonString(JsonTradeEntry entry)
	        {
	            string sanitizedExitName = entry.ExitName?.Replace("\"", "\\\"") ?? "";
	            string sanitizedSignalSource = entry.SignalSource?.Replace("\"", "\\\"") ?? "";
	
	            return "{"
	                + $"\"EntryType\":\"{entry.EntryType}\","
	                + $"\"Strategy\":\"{entry.Strategy}\","
	                + $"\"Instrument\":\"{entry.Instrument}\","
	                + $"\"Account\":\"{entry.Account}\","
	                + $"\"TradeNumber\":{entry.TradeNumber},"
	                + $"\"EntryTime\":\"{entry.EntryTime:o}\","
	                + $"\"Direction\":\"{entry.Direction}\","
	                + $"\"EntryPrice\":{entry.EntryPrice},"
	                + $"\"Quantity\":{entry.Quantity},"
	                + $"\"ExitTime\":\"{entry.ExitTime:o}\","
	                + $"\"ExitPrice\":{entry.ExitPrice},"
	                + $"\"ExitName\":\"{sanitizedExitName}\","
	                + $"\"ProfitTicks\":{entry.ProfitTicks},"
	                + $"\"ProfitCurrency\":{entry.ProfitCurrency},"
	                + $"\"Commission\":{entry.Commission},"
	                + $"\"MfeTicks\":{entry.MfeTicks},"
	                + $"\"MaeTicks\":{entry.MaeTicks},"
	                + $"\"MarketRegime\":\"{entry.MarketRegime}\","
	                + $"\"SignalSource\":\"{sanitizedSignalSource}\","
	                + $"\"MasterFilter\":\"{entry.MasterFilter}\","
	                + $"\"StopType\":\"{entry.StopType}\","
	                + $"\"ProfitType\":\"{entry.ProfitType}\","
	                + $"\"ConfluenceScore\":{entry.ConfluenceScore:F0},"
					+ $"\"ADX_at_Entry\":{entry.ADX_at_Entry:F2},"
	                + $"\"TMO_at_Entry\":{entry.TMO_at_Entry:F2},"
					+ $"\"UpVolume_at_Entry\":{entry.UpVolume_at_Entry:F0},"
					+ $"\"DownVolume_at_Entry\":{entry.DownVolume_at_Entry:F0},"
	                + $"\"AdxAtExit\":{entry.AdxAtExit:F2},"
	                + $"\"AtrAtExit\":{entry.AtrAtExit:F2},"
	                + $"\"TMO_at_Exit\":{entry.TMO_at_Exit:F2},"
	                + $"\"BarsInTrade\":{entry.BarsInTrade},"
	                + $"\"InitialSLTicks\":{entry.InitialSLTicks:F2},"
	                + $"\"InitialTPTicks\":{entry.InitialTPTicks:F2},"
	                + $"\"BeTriggerTicks\":{entry.BeTriggerTicks:F2},"
					+ $"\"DynamicBeTriggerTicks\":{entry.DynamicBeTriggerTicks:F2},"
	                + $"\"SlippageTicks\":{entry.SlippageTicks:F2},"
                    + $"\"CalmarRatio\":{entry.CalmarRatio:F4}" // ADDED FOR CALMAR
	                + "}";
	        }
	        
	        public bool LogTrade(string strategyName, Trade trade, double mfeTicks, double maeTicks, 
	                             string marketRegime, string signalSource, string masterFilterUsed, string stopTypeUsed, string profitTypeUsed, 
								 double confluenceScore, double adxAtEntry, double momentumAtEntry, double adxValue, double atrValue, double momentumValue, int barsInTrade, 
								 double initialSL, double initialTP, double beTrigger, double dynamicBeTrigger, double slippageTicks,
								 double upVolumeAtEntry, double downVolumeAtEntry, double currentCalmar, // ADDED PARAMETER
	                             out string errorMessage)
	        {
	            errorMessage = string.Empty;
	            if (trade?.Entry == null || trade.Exit == null) return false;
	
	            try
	            {
	                var logEntry = new JsonTradeEntry
	                {
	                    Strategy = strategyName, Instrument = trade.Entry.Instrument.FullName, Account = trade.Entry.Account.Name,
	                    TradeNumber = trade.TradeNumber, EntryTime = trade.Entry.Time, Direction = trade.Entry.MarketPosition,
	                    EntryPrice = trade.Entry.Price, Quantity = trade.Quantity, ExitTime = trade.Exit.Time,
	                    ExitPrice = trade.Exit.Price, ExitName = trade.Exit.Name, ProfitTicks = trade.ProfitTicks,
	                    ProfitCurrency = trade.ProfitCurrency, Commission = trade.Commission, MfeTicks = mfeTicks, MaeTicks = maeTicks,
	                    MarketRegime = marketRegime, SignalSource = signalSource, MasterFilter = masterFilterUsed, StopType = stopTypeUsed, ProfitType = profitTypeUsed, 
						ConfluenceScore = confluenceScore, ADX_at_Entry = adxAtEntry, TMO_at_Entry = momentumAtEntry,
						UpVolume_at_Entry = upVolumeAtEntry, DownVolume_at_Entry = downVolumeAtEntry,
						AdxAtExit = adxValue, AtrAtExit = atrValue, TMO_at_Exit = momentumValue, BarsInTrade = barsInTrade, 
                        InitialSLTicks = initialSL, InitialTPTicks = initialTP, BeTriggerTicks = beTrigger,
						DynamicBeTriggerTicks = dynamicBeTrigger, SlippageTicks = slippageTicks,
                        CalmarRatio = currentCalmar // ADDED FOR CALMAR
	                };
	
	                string jsonLine = ToJsonString(logEntry);
	                lock (fileLock) { File.AppendAllText(filePath, jsonLine + Environment.NewLine); }
	                return true;
	            }
	            catch (Exception ex)
	            {
	                errorMessage = $"Error logging trade to JSON: {ex.Message}";
	                return false;
	            }
	        }
	    }
	
	    // --- CSV Logger Component ---
	    public class TradeLogger
	    {
	        private readonly string filePath;
	        private static readonly object fileLock = new object();
	
	        public TradeLogger(string fullFilePath)
	        {
	            filePath = fullFilePath;
	            InitializeFile();
	        }
	
	        private void InitializeFile()
	        {
	            lock (fileLock)
	            {
	                try
	                {
	                    string directory = Path.GetDirectoryName(filePath);
	                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
	
	                    if (!File.Exists(filePath))
	                    {
	                        string header = "Strategy,Instrument,Account,TradeNum,EntryTime,Direction,EntryPrice,Quantity,ExitTime,ExitPrice,ExitName,ProfitTicks,ProfitCurrency,Commission,MFE_Ticks,MAE_Ticks,"
	                                      + "MarketRegime,SignalSource,MasterFilter,StopType,ProfitType,ConfluenceScore,ADX_at_Entry,TMO_at_Entry,UpVolume_at_Entry,DownVolume_at_Entry,ADX_at_Exit,ATR_at_Exit,TMO_at_Exit,BarsInTrade,InitialSL_Ticks,InitialTP_Ticks,BE_Trigger_Ticks,DynamicBE_Trigger_Ticks,SlippageTicks,CalmarRatio" 
	                                      + Environment.NewLine; // ADDED CALMAR TO HEADER
	                        File.WriteAllText(filePath, header);
	                    }
	                }
	                catch { }
	            }
	        }
	
	        public bool LogTrade(string strategyName, Trade trade, double mfeTicks, double maeTicks, 
	                             string marketRegime, string signalSource, string masterFilterUsed, string stopTypeUsed, string profitTypeUsed, 
								 double confluenceScore, double adxAtEntry, double momentumAtEntry, double adxValue, double atrValue, double momentumValue, int barsInTrade, 
								 double initialSL, double initialTP, double beTrigger, double dynamicBeTrigger, double slippageTicks,
								 double upVolumeAtEntry, double downVolumeAtEntry, double currentCalmar, // ADDED PARAMETER
	                             out string errorMessage)
	        {
	            errorMessage = string.Empty;
	            if (trade?.Entry == null || trade.Exit == null) return false;
	
	            try
	            {
	                var sb = new StringBuilder();
	                sb.Append($"{strategyName},");
	                sb.Append($"{trade.Entry.Instrument.FullName},");
	                sb.Append($"{trade.Entry.Account.Name},");
	                sb.Append($"{trade.TradeNumber},");
	                sb.Append($"{trade.Entry.Time:yyyy-MM-dd HH:mm:ss},");
	                sb.Append($"{trade.Entry.MarketPosition},");
	                sb.Append($"{trade.Entry.Price:F2},");
	                sb.Append($"{trade.Quantity},");
	                sb.Append($"{trade.Exit.Time:yyyy-MM-dd HH:mm:ss},");
	                sb.Append($"{trade.Exit.Price:F2},");
	                sb.Append($"{trade.Exit.Name.Replace(",", ";")},");
	                sb.Append($"{trade.ProfitTicks},");
	                sb.Append($"{trade.ProfitCurrency:F2},");
	                sb.Append($"{trade.Commission:F2},");
	                sb.Append($"{mfeTicks:F0},");
	                sb.Append($"{maeTicks:F0},");
	                sb.Append($"{marketRegime},");
	                sb.Append($"{signalSource.Replace(",", ";")},");
	                sb.Append($"{masterFilterUsed},");
	                sb.Append($"{stopTypeUsed},");
	                sb.Append($"{profitTypeUsed},");
	                sb.Append($"{confluenceScore:F0},");
					sb.Append($"{adxAtEntry:F2},");
	                sb.Append($"{momentumAtEntry:F2},");
					sb.Append($"{upVolumeAtEntry:F0},");
					sb.Append($"{downVolumeAtEntry:F0},");
	                sb.Append($"{adxValue:F2},");
	                sb.Append($"{atrValue:F2},");
	                sb.Append($"{momentumValue:F2},");
	                sb.Append($"{barsInTrade},");
	                sb.Append($"{initialSL:F2},");
	                sb.Append($"{initialTP:F2},");
	                sb.Append($"{beTrigger:F2},");
					sb.Append($"{dynamicBeTrigger:F2},");
	                sb.Append($"{slippageTicks:F2},");
                    sb.Append($"{currentCalmar:F4}"); // ADDED FOR CALMAR
	                sb.Append(Environment.NewLine);
	
	                lock (fileLock) { File.AppendAllText(filePath, sb.ToString()); }
	                return true;
	            }
	            catch (IOException ex)
	            {
	                errorMessage = $"Error logging trade to CSV: {ex.Message}";
	                return false;
	            }
	        }
	    }
	}
}