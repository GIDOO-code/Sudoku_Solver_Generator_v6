using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;

using static System.Math;
using static System.Diagnostics.Debug;

using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Windows.Shapes;

using GIDOO_space;
using System.Threading;

//Reverse sample code
//http://msdn.microsoft.com/ja-jp/ff363212#05

//[C#/XAML] Draw graphics with WPF
//http://code.msdn.microsoft.com/windowsdesktop/CVBXAML-WPF-Windows-WPF-0738a600

namespace GNPX_space{

    public class EColor{
        static private readonly Color _Black_= Colors.Black;
        static private readonly Color _ClrTracer_ = Color.FromArgb(0xFF,0x1,0x2,0x4);

        public Color ClrCellBkg   = _Black_;
        public int   noB;
        public Color ClrDigit     = _Black_;
        public Color ClrDigitBkg  = _Black_;
        public Color ClrCellFrame = _Black_;

        public EColor(){ }
        public EColor( Color ClrCellBkg, int noB, Color ClrDigit, Color ClrDigitBkg ){
            this.ClrCellBkg=ClrCellBkg;
            this.noB=noB; this.ClrDigit=ClrDigit; this.ClrDigitBkg=ClrDigitBkg;
        }

        public EColor( EColor E ){
            ClrCellBkg=E.ClrCellBkg; noB=E.noB; ClrDigit=E.ClrDigit; ClrDigitBkg=E.ClrDigitBkg; ClrCellFrame=E.ClrCellFrame;
        }

        public EColor( Color ClrCellFrame ){ this.ClrCellFrame=ClrCellFrame; }

        static public bool operator==( EColor A, EColor B ){
            try{
                if( B is null )  return false;
                bool eq = A.noB==B.noB && A.ClrDigit==B.ClrDigit && A.ClrDigitBkg==B.ClrDigitBkg && A.ClrCellBkg==B.ClrCellBkg;
                return eq;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }
        static public bool operator!=( EColor A, EColor B ){
            try{
                if( B is null )  return true;
                bool eq = A.noB!=B.noB || A.ClrDigit!=B.ClrDigit || A.ClrDigitBkg!=B.ClrDigitBkg || A.ClrCellBkg!=B.ClrCellBkg;
                return eq;
            }
            catch(NullReferenceException ex){ WriteLine(ex.Message+"\r"+ex.StackTrace); return true; }
        }
        public override string ToString(){
            string st = $" no:{noB.TBS()} CellBkg:{ClrCellBkg} DigClr:{ClrDigit} DigBkgClr:{ClrDigitBkg}";
            return st;
        }

    }
  
}
