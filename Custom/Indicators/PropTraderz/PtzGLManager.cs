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
using NinjaTrader.NinjaScript.Indicators;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators.PropTraderz
{
//  GL Type
	public enum ptzGLFillType
	{
		GOLDEN,
		SILVER,
		ALL
	}
	


 // 1. Create the GLManager class:
    public class ptzGLManager
    {
        private List<ptzGL> glList = new List<ptzGL>();

		public void AddGL(ptzGL gl)
		{
		//	Check if a GL with the same spotPrice already exists
		    if (!ContainsValue(gl.spotPrice))
		    {
		        glList.Add(gl);
		    }
//		    else
//		    {
//		        // Optional: You can print a message or throw an exception if you wish
//		        Console.WriteLine($"El valor {gl.spotPrice} ya existe y no se añadirá.");
//		    }
		}

        public ptzGL? GetNearestAbove(double price)
        {
//            return glList.Where(gl => gl.spotPrice > price && gl => !gl.filled)
//                          .OrderBy(gl => gl.spotPrice - price).FirstOrDefault();
			
            return glList.Where(gl => gl.spotPrice > price)
                      .OrderBy(gl => gl.spotPrice - price).FirstOrDefault();
			
        }

        public ptzGL? GetNearestBelow(double price)
        {
//            return glList.Where(gl => !gl.filled && gl.spotPrice < price && gl.type == type)
//                          .OrderByDescending(gl => price - gl.spotPrice).FirstOrDefault();
			
            return glList.Where(gl => gl.spotPrice < price)
                          .OrderBy(gl => price - gl.spotPrice).FirstOrDefault();			
        }

		public ptzGL? GetNearestLevel(double price)
		{
		    ptzGL? nearestAbove = GetNearestAbove(price);
		    ptzGL? nearestBelow = GetNearestBelow(price);
		
		    if (nearestAbove == null && nearestBelow == null)
		    {
		        return null; // No hay niveles disponibles
		    }
		
		    if (nearestAbove == null)
		    {
		        return nearestBelow; // Solo hay un nivel por debajo
		    }
		
		    if (nearestBelow == null)
		    {
		        return nearestAbove; // Solo hay un nivel por encima
		    }
		
		    // Ambos niveles están disponibles, así que comparamos distancias
		    double distanceAbove = nearestAbove.spotPrice - price;
		    double distanceBelow = price - nearestBelow.spotPrice;
		
		    return distanceAbove < distanceBelow ? nearestAbove : nearestBelow; // Devuelve el más cercano
		}		
		
		
        public List<ptzGL> GetGLList()
        {
		//	Return a copy to prevent external modifications	
            return new List<ptzGL>(glList); 
        }

        public void RemoveGL(string tag)
        {
            var glToRemove = glList.FirstOrDefault(gl => gl.tag == tag);
            if (glToRemove != null)
            {
                glList.Remove(glToRemove);
            }
        }

        public void MarkGLArmed(string tag, DateTime myTime)
        {
            var glToMark = glList.FirstOrDefault(gl => gl.tag == tag);
            if (glToMark != null)
            {
                glToMark.armed = true;
                glToMark.updateTime = myTime;
            }
        }
		
        public void MarkGLIsRising(string tag, DateTime myTime)
        {
            var glToMark = glList.FirstOrDefault(gl => gl.tag == tag);
            if (glToMark != null)
            {
                glToMark.glIsRising = true;
                glToMark.updateTime = myTime;
            }
        }

        public void MarkGLIsFalling(string tag, DateTime myTime)
        {
            var glToMark = glList.FirstOrDefault(gl => gl.tag == tag);
            if (glToMark != null)
            {
                glToMark.glIsFalling = true;
                glToMark.updateTime = myTime;
            }
        }

        public void ResetGL(string tag, DateTime myTime)
        {
            var glToMark = glList.FirstOrDefault(gl => gl.tag == tag);
            if (glToMark != null)
            {
                glToMark.armed = false;
                glToMark.glIsRising = false;
                glToMark.glIsFalling = false;
                glToMark.updateTime = myTime;
            }
        }
		
		public bool ContainsValue(double price)
		{
		    return glList.Any(gl => gl.spotPrice == price);
		}		

        public ptzGL? GetFirstGL()
        {
            return glList.FirstOrDefault();
        }

        public ptzGL? GetLastGL()
        {
            return glList.LastOrDefault();
        }

        public int GetGLCount()
        {
            return glList.Count;
        }		 		
		
	    public void Clear()
	    {
		//	Remove all items from the list	
	        glList.Clear(); 
	    }		

    }

}


#region Class GL (Golden Levels)
public class ptzGL
{
    public double spotPrice;
//    public double consequentEncroachmentPrice;
    public string tag;
	public string type;
    public bool armed;
    public bool glIsRising;
    public bool glIsFalling;
    public double roundyPrice;
    public DateTime updateTime;

    public ptzGL(string tag, string type, double spotPrice, DateTime myTime, double currentPrice)
    {
        this.tag = tag;
        this.spotPrice = spotPrice;
//        this.consequentEncroachmentPrice = (this.lowerPrice + this.upperPrice) / 2.0;
        this.armed = false;
        this.glIsRising = false;
        this.glIsFalling = false;   
        this.roundyPrice = Math.Floor(currentPrice / 100) * 100;     
        this.updateTime = myTime;
    }
}
#endregion
