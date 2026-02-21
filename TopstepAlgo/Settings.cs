using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopstepAlgo
{
   internal static class Settings
    {
        static public string AppName = "PAIR TRADING";
        static public long CompteId=19185015;
        static public string Exchange = "CME";
        static public string Symbol1 = "MES";
        static public string Exchange2 = "";
        static public string Symbol2 = "MNQ";
        static public int Quantity = 1;
        static public int Quantity2 = 1;
        static public double PROFIT = 50;
        static public double PERTE = 50;
        static public int ECART = 10;
        static public double DEPLACE = 5;
        static public double TickSize1 = 0.25;
        static public double TickSize2 = 0.25;
        static public double TickValue1 = 12.5;
        static public double TickValue2 = 5.0;
        static public int BUY = 0;
        static public int SELL = 1;
        static public bool Correlation = true;

    }
}
