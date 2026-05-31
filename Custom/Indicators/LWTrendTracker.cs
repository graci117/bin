#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;          // BarsPeriodType, BarsPeriod
using NinjaTrader.NinjaScript.DrawingTools;

#endregion

// Enums in global scope (works with NT codegen reliably)
public enum OttMaTypes { VAR, SMA, EMA, WMA, DEMA, HMA, TMA, ZLEMA, TSF, HULL, WWMA }
public enum lwCustomTimeFrame { Minute, Day, Week, Month, Second, Tick, Volume, Range }
public enum lwTableSize { Tiny, Small, Normal, Large }
public enum lwTablePosition { TopLeft, TopRight, BottomLeft, BottomRight }




namespace NinjaTrader.NinjaScript.Indicators
{
    public class LWTrendTracker : Indicator
    {
		
		// DX resources (dispose in OnRenderTargetChanged)
		private SharpDX.Direct2D1.SolidColorBrush tableTextDx;
		private SharpDX.Direct2D1.SolidColorBrush tableBullishDx;
		private SharpDX.Direct2D1.SolidColorBrush tableBearishDx;
		private SharpDX.Direct2D1.SolidColorBrush tableRangingDx;
		private SharpDX.Direct2D1.SolidColorBrush debugFillDx;
		
		private SharpDX.DirectWrite.TextFormat tableTextFormat;
		
		private bool lastUp;
		private bool lastDown;
		private string lastPriorText = "n/a";
		private SharpDX.DirectWrite.TextFormat rightAlignFormat;
		
		private int[] tfBarsIndex = new int[6];          // map row -> BarsArray index
		private bool[] rowUp = new bool[6];
		private bool[] rowDown = new bool[6];
		private string[] rowPrior = new string[6];
		
		private NinjaTrader.NinjaScript.Indicators.ADX[] seriesADX = new NinjaTrader.NinjaScript.Indicators.ADX[6];
		private double[] rowADX = new double[6];
		
		// Parser support
		private struct TfSpec { public BarsPeriodType Type; public int Value; }
		
		private NinjaTrader.NinjaScript.Indicators.KeltnerChannel[] seriesOTT = new NinjaTrader.NinjaScript.Indicators.KeltnerChannel[6]; // OTT uses KC or ATR-based bands
		private int[] rowCloudStatus = new int[6];  // 1 = above, 0 = inside, -1 = below
		private double[] rowCloudUpper = new double[6];
		private double[] rowCloudLower = new double[6];
		private NinjaTrader.NinjaScript.Indicators.ATR primaryATR;
		
		private double cachedCloudUpper = 0;
		private double cachedCloudLower = 0;

		Series<double> upperBand;
        Series<double> lowerBand;
		
		
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
			{
			    Name = "LWTrendTracker";
			    Description = "LWTrendTracker properties and defaults";
			    Calculate = Calculate.OnBarClose;
			    IsOverlay = true;
			    DrawOnPricePanel = true;
			    DisplayInDataBox = true;
			    IsSuspendedWhileInactive = true;
			
			    // ========== Range Filter (OTT) ==========
			    OttPeriod = 2;                               // [1, +inf)
			    OttCoeff = 0.6;                              // (0, +inf)
			    OttMaType = OttMaTypes.VAR;
			    ShowRangeFilter = false;
			    RangeFilterCloudOpacity = 20;                // [0..100]
			
			    // Colors (serialized via paired string properties)
			    OttUpColor = Brushes.DeepSkyBlue;
			    OttDownColor = Brushes.Goldenrod;
			
			    // ========== ADX Filter ==========
			    UseADXFilter = true;
			    AdxPeriod = 18;                              // [1, +inf)
			    AdxThreshold = 20;                           // [1, +inf)
			
			    // ========== Trend Magic ==========
			    CciPeriod = 30;                              // [1, +inf)
			    AtrPeriod = 12;                              // [1, +inf)
			    AtrMult = 1.5;                               // (0, +inf)
			
			    // ========== Multi-Timeframe ==========
			    MtfTimeframeType = lwCustomTimeFrame.Minute;
			    MtfTimeframeValue = 15;                      // [1, +inf)
			
			    // ========== Trend Table (Basics) ==========
			    ShowTrendTable = true;
			    TableSize = lwTableSize.Normal;
			    TablePosition = lwTablePosition.TopLeft;
			    TableYOffset = 20;
			
			    Timeframe1 = "1D";
			    Timeframe2 = "60";
			    Timeframe3 = "30";
			    Timeframe4 = "15";
			    Timeframe5 = "5";
			    Timeframe6 = "1";
			
			    // ========== Trend Table (Colors) ==========
			    TableBullishColor = Brushes.DeepSkyBlue;
			    TableBearishColor = Brushes.Goldenrod;
			    TableRangingColor = Brushes.Gray;
			    TableTextColor = Brushes.White;
			
			    // ========== Trend Table (Advanced) ==========
			    TablePeriod = 15;                            // [1, +inf)
			    TablePoles = 3;                              // [1, +inf)
			    TableSmoothLen = 22;                         // [1, +inf)
			    TableSmoothOffset = 7;                       // [0, +inf)
				
				
			
			    // Optional: clear Output window during dev
			    ClearOutputWindow();
			}
			if (State == State.DataLoaded)
		    {
		        upperBand = new Series<double>(this);
        		lowerBand = new Series<double>(this);
		    }
			else if (State == State.Configure)
			{
				
				primaryATR = ATR(OttPeriod);
			    // Reset maps; 0 is always primary Bars (current panel)
			    for (int i = 0; i < 6; i++) tfBarsIndex[i] = -1;
			
			    string[] tfs = new[] { Timeframe1, Timeframe2, Timeframe3, Timeframe4, Timeframe5, Timeframe6 };
			
			    // Keep track of which (Type,Value) already added
			    var added = new System.Collections.Generic.Dictionary<string, int>();
			
			    // Row 0..5
			    for (int i = 0; i < 6; i++)
			    {
			        if (!TryParseTf(tfs[i], out var spec))
			            continue;
					
					if (tfBarsIndex[i] >= 0)
				    {
				        // Use Keltner Channel as OTT approximation (ATR-based cloud)
				        seriesOTT[i] = KeltnerChannel(Closes[tfBarsIndex[i]], OttCoeff, OttPeriod);
				    }
			
			        string key = spec.Type + ":" + spec.Value;
			
			        if (spec.Type == BarsPeriodType.Minute && spec.Value == BarsPeriod.Value && BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
			        {
			            tfBarsIndex[i] = 0; // primary series matches
			        }
			        else if (spec.Type == BarsPeriodType.Day && BarsPeriod.BarsPeriodType == BarsPeriodType.Day && spec.Value == BarsPeriod.Value)
			        {
			            tfBarsIndex[i] = 0; // primary series matches
			        }
			        else if (added.ContainsKey(key))
			        {
			            tfBarsIndex[i] = added[key];
			        }
			        else
			        {
			            AddDataSeries(spec.Type, spec.Value);
			            int newIndex = BarsArray.Length - 1;
			            added[key] = newIndex;
			            tfBarsIndex[i] = newIndex;
			        }
					if (tfBarsIndex[i] >= 0)
            			seriesADX[i] = ADX(BarsArray[tfBarsIndex[i]], 18); // 18-period ADX
			    }
			}


        }
		
		protected override void OnBarUpdate()
		{
			



		    for (int i = 0; i < 6; i++)
		    {
		        int idx = tfBarsIndex[i];
		        if (idx < 0 || BarsInProgress != idx) continue;
				
					// Cloud status now shows: Upper if close > upper, Lower if close < lower, or Neutral if inside
					// Cloud status: Align with trend (bullish if above MA, bearish if below)
				if (CurrentBars[idx] >= OttPeriod)
				{
				    double close = Closes[idx][0];
				    double ma = 0;
				    for (int j = 0; j < OttPeriod && j < CurrentBars[idx]; j++)
				        ma += Closes[idx][j];
				    ma /= OttPeriod;
				
				    // Status: 1=above MA (bullish), -1=below MA (bearish), 0=on MA
				    if (close > ma * 1.0005)  // small buffer to avoid chop
				        rowCloudStatus[i] = 1;
				    else if (close < ma * 0.9995)
				        rowCloudStatus[i] = -1;
				    else
				        rowCloudStatus[i] = 0;
				}
				else
				{
				    rowCloudStatus[i] = 0;
				}



		
		        // Existing trend/prior calcs
		        if (CurrentBars[idx] < 1)
		        {
		            rowUp[i] = false;
		            rowDown[i] = false;
		            rowPrior[i] = "n/a";
		            rowADX[i] = 0;
		            continue;
		        }
		
		        double c0 = Closes[idx][0];
		        double c1 = Closes[idx][1];
		        rowUp[i] = c0 > c1;
		        rowDown[i] = c0 < c1;
		        rowPrior[i] = string.Format("O:{0:F2}  C:{1:F2}", Opens[idx][1], Closes[idx][1]);
		
		        // NEW: Fetch ADX value
		        if (seriesADX[i] != null && seriesADX[i].Value.Count > 0)
		            rowADX[i] = seriesADX[i].Value[0];
		        else
		            rowADX[i] = 0;
		    }
			
			
		}




		
		// Convert WPF Brush -> DX Color4
		private static SharpDX.Color4 ToColor4(Brush brush)
		{
		    var scb = brush as SolidColorBrush ?? Brushes.White;
		    var c = scb.Color;
		    // Preserve alpha
		    return new SharpDX.Color4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
		}
		
		// Create a DX brush from a WPF brush (RenderTarget must be non-null)
		private SharpDX.Direct2D1.SolidColorBrush MakeDxBrush(Brush wpfBrush)
		{
		    return new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, ToColor4(wpfBrush));
		}
		
		// Compute base layout and font size from TableSize and panel
		private void GetTableLayout(out float x, out float y, out float rowH, out float col1W, out float col2W, out float col3W)
		{
		    float panelLeft   = (float)ChartPanel.X;
		    float panelTop    = (float)ChartPanel.Y;
		    float panelRight  = (float)(ChartPanel.X + ChartPanel.W);
		
		    rowH = TableSize == lwTableSize.Tiny ? 14f
		         : TableSize == lwTableSize.Small ? 16f
		         : TableSize == lwTableSize.Normal ? 20f
		         : 24f;
		
		    col1W = 80f; col2W = 80f; col3W = 120f;
		    float totalW = col1W + col2W + col3W;
		
		    x = TablePosition == lwTablePosition.TopLeft
		        ? panelLeft + 20f
		        : panelRight - totalW - 20f;
		
		    y = panelTop + Math.Max(0, TableYOffset);
		}
		
		// Compute font size from TableSize
		private float GetFontSize()
		{
		    return TableSize == lwTableSize.Tiny ? 10f
		         : TableSize == lwTableSize.Small ? 12f
		         : TableSize == lwTableSize.Normal ? 16f
		         : 20f;
		}
		
		private bool TryParseTf(string tf, out TfSpec spec)
		{
		    spec = new TfSpec { Type = BarsPeriodType.Minute, Value = 1 };
		    if (string.IsNullOrWhiteSpace(tf)) return false;
		
		    tf = tf.Trim().ToUpperInvariant();
		
		    // Day
		    if (tf.EndsWith("D"))
		    {
		        if (int.TryParse(tf.Substring(0, tf.Length - 1), out int d) && d >= 1)
		        {
		            spec.Type = BarsPeriodType.Day;
		            spec.Value = d;
		            return true;
		        }
		        if (tf == "1D") { spec.Type = BarsPeriodType.Day; spec.Value = 1; return true; }
		        return false;
		    }
		
		    // Minute
		    if (int.TryParse(tf, out int m) && m >= 1)
		    {
		        spec.Type = BarsPeriodType.Minute;
		        spec.Value = m;
		        return true;
		    }
		
		    return false;
		}


		
		public override void OnRenderTargetChanged()
		{
		    // Dispose previous resources
		    tableTextDx?.Dispose();    tableTextDx = null;
		    tableBullishDx?.Dispose(); tableBullishDx = null;
		    tableBearishDx?.Dispose(); tableBearishDx = null;
		    tableRangingDx?.Dispose(); tableRangingDx = null;
		    debugFillDx?.Dispose();    debugFillDx = null;
		    tableTextFormat?.Dispose(); tableTextFormat = null;
		
		    if (RenderTarget == null)
		        return;
		
		    // Recreate DX brushes from current WPF brushes
		    tableTextDx    = MakeDxBrush(TableTextColor ?? Brushes.White);
		    tableBullishDx = MakeDxBrush(TableBullishColor ?? Brushes.DeepSkyBlue);
		    tableBearishDx = MakeDxBrush(TableBearishColor ?? Brushes.Goldenrod);
		    tableRangingDx = MakeDxBrush(TableRangingColor ?? Brushes.Gray);
		
		    // Semi-transparent debug fill
		    debugFillDx = MakeDxBrush(new SolidColorBrush(Color.FromArgb(96, 30, 144, 255)));
		
		    // Text format
		    float fontSize = GetFontSize();
		    tableTextFormat = new SharpDX.DirectWrite.TextFormat(
		        NinjaTrader.Core.Globals.DirectWriteFactory,
		        "Calibri",
		        SharpDX.DirectWrite.FontWeight.SemiBold,
		        SharpDX.DirectWrite.FontStyle.Normal,
		        SharpDX.DirectWrite.FontStretch.Medium,
		        fontSize);
			tableTextFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
		    tableTextFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Center;
			
			rightAlignFormat?.Dispose();
			rightAlignFormat = new SharpDX.DirectWrite.TextFormat(
			    NinjaTrader.Core.Globals.DirectWriteFactory,
			    "Calibri",
			    SharpDX.DirectWrite.FontWeight.SemiBold,
			    SharpDX.DirectWrite.FontStyle.Normal,
			    SharpDX.DirectWrite.FontStretch.Medium,
			    14f);
			rightAlignFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing;
			rightAlignFormat.ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center;
			rightAlignFormat.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
				
			
		}
		
		private bool TryGetClose(int barsAgo, out double value)
		{
		    value = 0;
		    if (CurrentBar >= barsAgo)
		    {
		        value = Close[barsAgo];
		        return true;
		    }
		    return false;
		}
		
		private bool TryGetOpen(int barsAgo, out double value)
		{
		    value = 0;
		    if (CurrentBar >= barsAgo)
		    {
		        value = Open[barsAgo];
		        return true;
		    }
		    return false;
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
{
    base.OnRender(chartControl, chartScale);

    if (chartControl == null || ChartPanel == null || RenderTarget == null) return;

    try
    {
          // ========== TREND TABLE ==========
       

        if (!ShowTrendTable) return;  // NOW it's here, after cloud

        // Recreate brushes
        tableTextDx?.Dispose();    tableTextDx    = MakeDxBrush(TableTextColor ?? Brushes.White);
        tableBullishDx?.Dispose(); tableBullishDx = MakeDxBrush(TableBullishColor ?? Brushes.DeepSkyBlue);
        tableBearishDx?.Dispose(); tableBearishDx = MakeDxBrush(TableBearishColor ?? Brushes.Goldenrod);
        tableRangingDx?.Dispose(); tableRangingDx = MakeDxBrush(TableRangingColor ?? Brushes.Gray);
        debugFillDx?.Dispose();    debugFillDx    = MakeDxBrush(new SolidColorBrush(Color.FromArgb(96, 30, 144, 255)));

        // Fixed layout
        float x = (float)ChartPanel.X + 20f;
        float y = (float)ChartPanel.Y + 100f;
        float rowH = 18f;
        float col1W = 80f, col2W = 80f, col3W = 140f, col4W = 70f;
        float totalW = col1W + col2W + col3W + col4W;
        float pad = 4f;

        // Table background (header + 6 rows)
        var fullArea = new SharpDX.RectangleF(x, y, totalW, rowH * 7f);
        RenderTarget.FillRectangle(fullArea, debugFillDx);

        // Header cells with background color
        var h1 = new SharpDX.RectangleF(x, y, col1W, rowH);
        var h2 = new SharpDX.RectangleF(x + col1W, y, col2W, rowH);
        var h3 = new SharpDX.RectangleF(x + col1W + col2W, y, col3W, rowH);
        var h4 = new SharpDX.RectangleF(x + col1W + col2W + col3W, y, col4W, rowH);
        
        // Fill header cells with alternating background
        var headerAlt = tableRangingDx;
        RenderTarget.FillRectangle(h1, headerAlt);
        RenderTarget.FillRectangle(h2, headerAlt);
        RenderTarget.FillRectangle(h3, headerAlt);
        RenderTarget.FillRectangle(h4, headerAlt);
        
        // Draw header text
        RenderTarget.DrawText("Timeframe", tableTextFormat, h1, tableTextDx);
        RenderTarget.DrawText("Trend",     tableTextFormat, h2, tableTextDx);
        RenderTarget.DrawText("Prior Bar", tableTextFormat, h3, tableTextDx);
        RenderTarget.DrawText("Align",     tableTextFormat, h4, tableTextDx);

        // Separator under header
        var sep = new SharpDX.RectangleF(x, y + rowH - 1f, totalW, 1f);
        RenderTarget.FillRectangle(sep, tableTextDx);

        // Timeframe labels
        string[] tfs = new[] { Timeframe1, Timeframe2, Timeframe3, Timeframe4, Timeframe5, Timeframe6 };

        // Rows
        for (int i = 0; i < 6; i++)
        {
            float rowY = y + (i + 1) * rowH;

            var tfRectPad    = new SharpDX.RectangleF(x + pad, rowY + 0.5f, col1W - 2 * pad, rowH - 1f);
            var trendRectPad = new SharpDX.RectangleF(x + col1W + pad, rowY + 0.5f, col2W - 2 * pad, rowH - 1f);
            var priorRectPad = new SharpDX.RectangleF(x + col1W + col2W + pad, rowY + 0.5f, col3W - 2 * pad, rowH - 1f);
            var cloudRectPad = new SharpDX.RectangleF(x + col1W + col2W + col3W + pad, rowY + 0.5f, col4W - 2 * pad, rowH - 1f);

            // Alternating row background
            var alt = (i % 2 == 0) ? tableRangingDx : tableBearishDx;
            RenderTarget.FillRectangle(new SharpDX.RectangleF(x, rowY, col1W, rowH), alt);
            RenderTarget.FillRectangle(new SharpDX.RectangleF(x + col1W, rowY, col2W, rowH), alt);
            RenderTarget.FillRectangle(new SharpDX.RectangleF(x + col1W + col2W, rowY, col3W, rowH), alt);

            // Timeframe
            string tfLabel = string.IsNullOrWhiteSpace(tfs[i]) ? "-" : tfs[i];
            RenderTarget.DrawText(tfLabel, tableTextFormat, tfRectPad, tableTextDx);

            // Trend + ADX combined
            string trendLabel = "—";
            if (rowADX[i] > 0)
            {
                string direction = rowUp[i] ? "UP" : rowDown[i] ? "DOWN" : "—";
                trendLabel = string.Format("{0}({1:F0})", direction, rowADX[i]);
            }
            var trendBrush = rowUp[i] ? tableBullishDx : rowDown[i] ? tableBearishDx : tableRangingDx;
            RenderTarget.FillRectangle(trendRectPad, trendBrush);
            RenderTarget.DrawText(trendLabel, tableTextFormat, trendRectPad, tableTextDx);

            // Prior from cached per-row
            RenderTarget.DrawText(string.IsNullOrEmpty(rowPrior[i]) ? "n/a" : rowPrior[i], rightAlignFormat, priorRectPad, tableTextDx);

            // Cloud/MA alignment - color-coded background
            var alignBrush = tableRangingDx;  // Default neutral (gray)
            string alignLabel = "=";

            if (rowCloudStatus[i] == 1)  // Price above MA
            {
                if (rowUp[i])  // UP trend + price above = aligned
                {
                    alignLabel = "OK";
                    alignBrush = tableBullishDx;  // Green
                }
                else  // DOWN trend + price above = divergence
                {
                    alignLabel = "DIV";
                    alignBrush = tableBearishDx;  // Red
                }
            }
            else if (rowCloudStatus[i] == -1)  // Price below MA
            {
                if (rowDown[i])  // DOWN trend + price below = aligned
                {
                    alignLabel = "OK";
                    alignBrush = tableBearishDx;  // Red (bearish OK)
                }
                else  // UP trend + price below = divergence
                {
                    alignLabel = "DIV";
                    alignBrush = tableBullishDx;  // Green
                }
            }

            // Draw colored background for alignment column
            RenderTarget.FillRectangle(new SharpDX.RectangleF(x + col1W + col2W + col3W, rowY, col4W, rowH), alignBrush);

            // Draw alignment text
            RenderTarget.DrawText(alignLabel, tableTextFormat, cloudRectPad, tableTextDx);
        }
    
    }
    catch (Exception ex)
    {
        Print($"OnRender exception: {ex.Message}");
    }
}


		
		
















       #region Properties

		// ========== Range Filter (OTT) ==========
		[NinjaScriptProperty]
		[Display(Name = "OTT Period", Order = 1, GroupName = "Range Filter (OTT) Settings")]
		[Range(1, int.MaxValue)]
		public int OttPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Optimization Coeff", Order = 2, GroupName = "Range Filter (OTT) Settings")]
		[Range(0.0001, double.MaxValue)]
		public double OttCoeff { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "OTT MA Type", Order = 3, GroupName = "Range Filter (OTT) Settings")]
		public OttMaTypes OttMaType { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Show Range Filter", Order = 4, GroupName = "Range Filter (OTT) Settings")]
		public bool ShowRangeFilter { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Range Cloud Opacity", Order = 5, GroupName = "Range Filter (OTT) Settings")]
		[Range(0, 100)]
		public int RangeFilterCloudOpacity { get; set; }
		
		[XmlIgnore]
		[Display(Name = "OTT Up Color", Order = 6, GroupName = "Range Filter (OTT) Settings")]
		public Brush OttUpColor { get; set; }
		
		[Browsable(false)]
		public string OttUpColorSerialize
		{
		    get => Serialize.BrushToString(OttUpColor);
		    set => OttUpColor = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[Display(Name = "OTT Down Color", Order = 7, GroupName = "Range Filter (OTT) Settings")]
		public Brush OttDownColor { get; set; }
		
		[Browsable(false)]
		public string OttDownColorSerialize
		{
		    get => Serialize.BrushToString(OttDownColor);
		    set => OttDownColor = Serialize.StringToBrush(value);
		}
		
		// ========== ADX Filter ==========
		[NinjaScriptProperty]
		[Display(Name = "Use ADX Filter", Order = 1, GroupName = "ADX Filter")]
		public bool UseADXFilter { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "ADX Period", Order = 2, GroupName = "ADX Filter")]
		[Range(1, int.MaxValue)]
		public int AdxPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "ADX Threshold", Order = 3, GroupName = "ADX Filter")]
		[Range(1, int.MaxValue)]
		public int AdxThreshold { get; set; }
		
		// ========== Trend Magic ==========
		[NinjaScriptProperty]
		[Display(Name = "CCI Period", Order = 1, GroupName = "Trend Magic")]
		[Range(1, int.MaxValue)]
		public int CciPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "ATR Period", Order = 2, GroupName = "Trend Magic")]
		[Range(1, int.MaxValue)]
		public int AtrPeriod { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "ATR Multiplier", Order = 3, GroupName = "Trend Magic")]
		[Range(0.0001, double.MaxValue)]
		public double AtrMult { get; set; }
		
		// ========== Multi-Timeframe ==========
		[NinjaScriptProperty]
		[Display(Name = "MTF Type", Order = 1, GroupName = "Multi-Timeframe")]
		public lwCustomTimeFrame MtfTimeframeType { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "MTF Value", Order = 2, GroupName = "Multi-Timeframe")]
		[Range(1, int.MaxValue)]
		public int MtfTimeframeValue { get; set; }
		
		// ========== Trend Table (Basics) ==========
		[NinjaScriptProperty]
		[Display(Name = "Show Trend Table", Order = 1, GroupName = "Trend Table")]
		public bool ShowTrendTable { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Table Size", Order = 2, GroupName = "Trend Table")]
		public lwTableSize TableSize { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Table Position", Order = 3, GroupName = "Trend Table")]
		public lwTablePosition TablePosition { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Table Y Offset", Order = 4, GroupName = "Trend Table")]
		public int TableYOffset { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "TF1", Order = 10, GroupName = "Trend Table")]
		public string Timeframe1 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "TF2", Order = 11, GroupName = "Trend Table")]
		public string Timeframe2 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "TF3", Order = 12, GroupName = "Trend Table")]
		public string Timeframe3 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "TF4", Order = 13, GroupName = "Trend Table")]
		public string Timeframe4 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "TF5", Order = 14, GroupName = "Trend Table")]
		public string Timeframe5 { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "TF6", Order = 15, GroupName = "Trend Table")]
		public string Timeframe6 { get; set; }
		
		// ========== Trend Table (Colors) ==========
		[XmlIgnore]
		[Display(Name = "Table Bullish", Order = 20, GroupName = "Trend Table")]
		public Brush TableBullishColor { get; set; }
		
		[Browsable(false)]
		public string TableBullishColorSerialize
		{
		    get => Serialize.BrushToString(TableBullishColor);
		    set => TableBullishColor = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[Display(Name = "Table Bearish", Order = 21, GroupName = "Trend Table")]
		public Brush TableBearishColor { get; set; }
		
		[Browsable(false)]
		public string TableBearishColorSerialize
		{
		    get => Serialize.BrushToString(TableBearishColor);
		    set => TableBearishColor = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[Display(Name = "Table Ranging", Order = 22, GroupName = "Trend Table")]
		public Brush TableRangingColor { get; set; }
		
		[Browsable(false)]
		public string TableRangingColorSerialize
		{
		    get => Serialize.BrushToString(TableRangingColor);
		    set => TableRangingColor = Serialize.StringToBrush(value);
		}
		
		[XmlIgnore]
		[Display(Name = "Table Text", Order = 23, GroupName = "Trend Table")]
		public Brush TableTextColor { get; set; }
		
		[Browsable(false)]
		public string TableTextColorSerialize
		{
		    get => Serialize.BrushToString(TableTextColor);
		    set => TableTextColor = Serialize.StringToBrush(value);
		}
		
		// ========== Trend Table (Advanced) ==========
		[NinjaScriptProperty]
		[Display(Name = "Table Period", Order = 30, GroupName = "Trend Table")]
		[Range(1, int.MaxValue)]
		public int TablePeriod { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Table Poles", Order = 31, GroupName = "Trend Table")]
		[Range(1, int.MaxValue)]
		public int TablePoles { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Smooth Len", Order = 32, GroupName = "Trend Table")]
		[Range(1, int.MaxValue)]
		public int TableSmoothLen { get; set; }
		
		[NinjaScriptProperty]
		[Display(Name = "Smooth Offset", Order = 33, GroupName = "Trend Table")]
		[Range(0, int.MaxValue)]
		public int TableSmoothOffset { get; set; }
		
		#endregion

    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LWTrendTracker[] cacheLWTrendTracker;
		public LWTrendTracker LWTrendTracker(int ottPeriod, double ottCoeff, OttMaTypes ottMaType, bool showRangeFilter, int rangeFilterCloudOpacity, bool useADXFilter, int adxPeriod, int adxThreshold, int cciPeriod, int atrPeriod, double atrMult, lwCustomTimeFrame mtfTimeframeType, int mtfTimeframeValue, bool showTrendTable, lwTableSize tableSize, lwTablePosition tablePosition, int tableYOffset, string timeframe1, string timeframe2, string timeframe3, string timeframe4, string timeframe5, string timeframe6, int tablePeriod, int tablePoles, int tableSmoothLen, int tableSmoothOffset)
		{
			return LWTrendTracker(Input, ottPeriod, ottCoeff, ottMaType, showRangeFilter, rangeFilterCloudOpacity, useADXFilter, adxPeriod, adxThreshold, cciPeriod, atrPeriod, atrMult, mtfTimeframeType, mtfTimeframeValue, showTrendTable, tableSize, tablePosition, tableYOffset, timeframe1, timeframe2, timeframe3, timeframe4, timeframe5, timeframe6, tablePeriod, tablePoles, tableSmoothLen, tableSmoothOffset);
		}

		public LWTrendTracker LWTrendTracker(ISeries<double> input, int ottPeriod, double ottCoeff, OttMaTypes ottMaType, bool showRangeFilter, int rangeFilterCloudOpacity, bool useADXFilter, int adxPeriod, int adxThreshold, int cciPeriod, int atrPeriod, double atrMult, lwCustomTimeFrame mtfTimeframeType, int mtfTimeframeValue, bool showTrendTable, lwTableSize tableSize, lwTablePosition tablePosition, int tableYOffset, string timeframe1, string timeframe2, string timeframe3, string timeframe4, string timeframe5, string timeframe6, int tablePeriod, int tablePoles, int tableSmoothLen, int tableSmoothOffset)
		{
			if (cacheLWTrendTracker != null)
				for (int idx = 0; idx < cacheLWTrendTracker.Length; idx++)
					if (cacheLWTrendTracker[idx] != null && cacheLWTrendTracker[idx].OttPeriod == ottPeriod && cacheLWTrendTracker[idx].OttCoeff == ottCoeff && cacheLWTrendTracker[idx].OttMaType == ottMaType && cacheLWTrendTracker[idx].ShowRangeFilter == showRangeFilter && cacheLWTrendTracker[idx].RangeFilterCloudOpacity == rangeFilterCloudOpacity && cacheLWTrendTracker[idx].UseADXFilter == useADXFilter && cacheLWTrendTracker[idx].AdxPeriod == adxPeriod && cacheLWTrendTracker[idx].AdxThreshold == adxThreshold && cacheLWTrendTracker[idx].CciPeriod == cciPeriod && cacheLWTrendTracker[idx].AtrPeriod == atrPeriod && cacheLWTrendTracker[idx].AtrMult == atrMult && cacheLWTrendTracker[idx].MtfTimeframeType == mtfTimeframeType && cacheLWTrendTracker[idx].MtfTimeframeValue == mtfTimeframeValue && cacheLWTrendTracker[idx].ShowTrendTable == showTrendTable && cacheLWTrendTracker[idx].TableSize == tableSize && cacheLWTrendTracker[idx].TablePosition == tablePosition && cacheLWTrendTracker[idx].TableYOffset == tableYOffset && cacheLWTrendTracker[idx].Timeframe1 == timeframe1 && cacheLWTrendTracker[idx].Timeframe2 == timeframe2 && cacheLWTrendTracker[idx].Timeframe3 == timeframe3 && cacheLWTrendTracker[idx].Timeframe4 == timeframe4 && cacheLWTrendTracker[idx].Timeframe5 == timeframe5 && cacheLWTrendTracker[idx].Timeframe6 == timeframe6 && cacheLWTrendTracker[idx].TablePeriod == tablePeriod && cacheLWTrendTracker[idx].TablePoles == tablePoles && cacheLWTrendTracker[idx].TableSmoothLen == tableSmoothLen && cacheLWTrendTracker[idx].TableSmoothOffset == tableSmoothOffset && cacheLWTrendTracker[idx].EqualsInput(input))
						return cacheLWTrendTracker[idx];
			return CacheIndicator<LWTrendTracker>(new LWTrendTracker(){ OttPeriod = ottPeriod, OttCoeff = ottCoeff, OttMaType = ottMaType, ShowRangeFilter = showRangeFilter, RangeFilterCloudOpacity = rangeFilterCloudOpacity, UseADXFilter = useADXFilter, AdxPeriod = adxPeriod, AdxThreshold = adxThreshold, CciPeriod = cciPeriod, AtrPeriod = atrPeriod, AtrMult = atrMult, MtfTimeframeType = mtfTimeframeType, MtfTimeframeValue = mtfTimeframeValue, ShowTrendTable = showTrendTable, TableSize = tableSize, TablePosition = tablePosition, TableYOffset = tableYOffset, Timeframe1 = timeframe1, Timeframe2 = timeframe2, Timeframe3 = timeframe3, Timeframe4 = timeframe4, Timeframe5 = timeframe5, Timeframe6 = timeframe6, TablePeriod = tablePeriod, TablePoles = tablePoles, TableSmoothLen = tableSmoothLen, TableSmoothOffset = tableSmoothOffset }, input, ref cacheLWTrendTracker);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LWTrendTracker LWTrendTracker(int ottPeriod, double ottCoeff, OttMaTypes ottMaType, bool showRangeFilter, int rangeFilterCloudOpacity, bool useADXFilter, int adxPeriod, int adxThreshold, int cciPeriod, int atrPeriod, double atrMult, lwCustomTimeFrame mtfTimeframeType, int mtfTimeframeValue, bool showTrendTable, lwTableSize tableSize, lwTablePosition tablePosition, int tableYOffset, string timeframe1, string timeframe2, string timeframe3, string timeframe4, string timeframe5, string timeframe6, int tablePeriod, int tablePoles, int tableSmoothLen, int tableSmoothOffset)
		{
			return indicator.LWTrendTracker(Input, ottPeriod, ottCoeff, ottMaType, showRangeFilter, rangeFilterCloudOpacity, useADXFilter, adxPeriod, adxThreshold, cciPeriod, atrPeriod, atrMult, mtfTimeframeType, mtfTimeframeValue, showTrendTable, tableSize, tablePosition, tableYOffset, timeframe1, timeframe2, timeframe3, timeframe4, timeframe5, timeframe6, tablePeriod, tablePoles, tableSmoothLen, tableSmoothOffset);
		}

		public Indicators.LWTrendTracker LWTrendTracker(ISeries<double> input , int ottPeriod, double ottCoeff, OttMaTypes ottMaType, bool showRangeFilter, int rangeFilterCloudOpacity, bool useADXFilter, int adxPeriod, int adxThreshold, int cciPeriod, int atrPeriod, double atrMult, lwCustomTimeFrame mtfTimeframeType, int mtfTimeframeValue, bool showTrendTable, lwTableSize tableSize, lwTablePosition tablePosition, int tableYOffset, string timeframe1, string timeframe2, string timeframe3, string timeframe4, string timeframe5, string timeframe6, int tablePeriod, int tablePoles, int tableSmoothLen, int tableSmoothOffset)
		{
			return indicator.LWTrendTracker(input, ottPeriod, ottCoeff, ottMaType, showRangeFilter, rangeFilterCloudOpacity, useADXFilter, adxPeriod, adxThreshold, cciPeriod, atrPeriod, atrMult, mtfTimeframeType, mtfTimeframeValue, showTrendTable, tableSize, tablePosition, tableYOffset, timeframe1, timeframe2, timeframe3, timeframe4, timeframe5, timeframe6, tablePeriod, tablePoles, tableSmoothLen, tableSmoothOffset);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LWTrendTracker LWTrendTracker(int ottPeriod, double ottCoeff, OttMaTypes ottMaType, bool showRangeFilter, int rangeFilterCloudOpacity, bool useADXFilter, int adxPeriod, int adxThreshold, int cciPeriod, int atrPeriod, double atrMult, lwCustomTimeFrame mtfTimeframeType, int mtfTimeframeValue, bool showTrendTable, lwTableSize tableSize, lwTablePosition tablePosition, int tableYOffset, string timeframe1, string timeframe2, string timeframe3, string timeframe4, string timeframe5, string timeframe6, int tablePeriod, int tablePoles, int tableSmoothLen, int tableSmoothOffset)
		{
			return indicator.LWTrendTracker(Input, ottPeriod, ottCoeff, ottMaType, showRangeFilter, rangeFilterCloudOpacity, useADXFilter, adxPeriod, adxThreshold, cciPeriod, atrPeriod, atrMult, mtfTimeframeType, mtfTimeframeValue, showTrendTable, tableSize, tablePosition, tableYOffset, timeframe1, timeframe2, timeframe3, timeframe4, timeframe5, timeframe6, tablePeriod, tablePoles, tableSmoothLen, tableSmoothOffset);
		}

		public Indicators.LWTrendTracker LWTrendTracker(ISeries<double> input , int ottPeriod, double ottCoeff, OttMaTypes ottMaType, bool showRangeFilter, int rangeFilterCloudOpacity, bool useADXFilter, int adxPeriod, int adxThreshold, int cciPeriod, int atrPeriod, double atrMult, lwCustomTimeFrame mtfTimeframeType, int mtfTimeframeValue, bool showTrendTable, lwTableSize tableSize, lwTablePosition tablePosition, int tableYOffset, string timeframe1, string timeframe2, string timeframe3, string timeframe4, string timeframe5, string timeframe6, int tablePeriod, int tablePoles, int tableSmoothLen, int tableSmoothOffset)
		{
			return indicator.LWTrendTracker(input, ottPeriod, ottCoeff, ottMaType, showRangeFilter, rangeFilterCloudOpacity, useADXFilter, adxPeriod, adxThreshold, cciPeriod, atrPeriod, atrMult, mtfTimeframeType, mtfTimeframeValue, showTrendTable, tableSize, tablePosition, tableYOffset, timeframe1, timeframe2, timeframe3, timeframe4, timeframe5, timeframe6, tablePeriod, tablePoles, tableSmoothLen, tableSmoothOffset);
		}
	}
}

#endregion
