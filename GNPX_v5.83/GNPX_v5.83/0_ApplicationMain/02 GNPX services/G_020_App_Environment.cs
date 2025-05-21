using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static System.Diagnostics.Debug;
using static System.Math;
//using GIDOO_Lib;
//using OpenCvSharp;
using GNPX_space;
using System.Globalization;

namespace GIDOO_space{
	static public class App_Environment{
		static public string GNPXvX = "GNPXv6 2025";  //"GNPXv " + DateTime.Now.Year;
		static public int    VersionNo = 580;

		static public double pixelsPerDip;
		static public int    lineWidth = 1;
        static public int    cellSize  = 36;
        static public int    cellSizeP = cellSize+lineWidth;

        static public GFont  gsFont    = new GFont( "Times New Romaon", 22 );

		static public string culture   = CultureInfo.CurrentCulture.Name;
	}
	
}
