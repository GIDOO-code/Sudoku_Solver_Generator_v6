	using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;
//using GIDOO_Lib;
//using OpenCvSharp;

namespace GIDOO_space{
    public static class App_ColorBrush{
        static public readonly Color[] _ColorsLst2 = new Color[]{
            Colors.Red, Colors.Blue, Colors.Tomato,
            Colors.DarkViolet, Colors.Lime, Colors.Magenta,
            Colors.DarkGreen, Colors.DodgerBlue, Colors.DarkOrange,
        };

		static public Color cr_Board        = Color.FromArgb(255,220,220,220);
		static public Color cr_BoardLine    = Colors.Navy;	
		static public Color cr_CellForeNo   = Colors.Navy;
		static public Color cr_CellBkgdPNo  = Color.FromArgb(255,160,160,160);
		static public Color cr_CellBkgdMNo  = Color.FromArgb(255,190,190,200);
		static public Color cr_CellBkgdZNo  = Colors.White;
		static public Color cr_CellBkgdZNo2 = Color.FromArgb(255,150,150,250);
		static public Color cr_CellBkgdFix  = Colors.LightGreen;
		static public Color cr_CellFixed    = Colors.Red;
		static public Color cr_CellFixedNo  = Colors.Red;

		static public Brush br_Board        = new SolidColorBrush( cr_Board );
		static public Brush br_BoardLine    = new SolidColorBrush( cr_BoardLine );
		static public Brush br_CellForeNo   = new SolidColorBrush( cr_CellForeNo );
		static public Brush br_CellBkgdPNo  = new SolidColorBrush( cr_CellBkgdPNo );
		static public Brush br_CellBkgdMNo  = new SolidColorBrush( cr_CellBkgdMNo );
		static public Brush br_CellBkgdZNo  = new SolidColorBrush( cr_CellBkgdZNo );
		static public Brush br_CellBkgdZNo2 = new SolidColorBrush( cr_CellBkgdZNo2 );
		static public Brush br_CellBkgdFix  = new SolidColorBrush( cr_CellBkgdFix );
		static public Brush br_CellFixed    = new SolidColorBrush( cr_CellFixed );
		static public Brush br_CellFixedNo  = new SolidColorBrush( cr_CellFixedNo );

		static public void GNPX_DebugTracerWithTime( string fName, string message, bool append=true ){
			using (var fpW=new StreamWriter(fName,append,Encoding.UTF8)){ // message for debug 
                fpW.WriteLine($"{DateTime.Now}  {message}");
            }
		}
    }
}
