// 
// Copyright (C) 2025, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
namespace NinjaTrader.NinjaScript.ImportTypes
{
	public class TextImportTypeBeginningOfBar : TextImportType
	{ 
		protected override void OnStateChange()
		{			
			if (State == State.SetDefaults)
			{
				EndOfBarTimestamps	= false;
				Name				= Custom.Resource.ImportTypeNinjaTraderBeginningOfBar;
			}
			else
				base.OnStateChange();
		}
	}
}
