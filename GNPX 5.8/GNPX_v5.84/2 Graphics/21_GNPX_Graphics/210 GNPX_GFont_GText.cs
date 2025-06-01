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

    public class GFont{
        public FontFamily FontFamily;
        public int        FontSize;
        public FontWeight FontWeight;
        public FontStyle  FontStyle;

        public string Name{ get=> FontFamily.ToString(); }

        public GFont( string FontName, int FontSize=10 ){
            this.FontFamily =new FontFamily(FontName);
            this.FontSize   = FontSize;
            this.FontWeight = FontWeights.Normal;
            this.FontStyle  = FontStyles.Normal;
        }   

        public GFont( string  FontName, int FontSize, FontWeight FontWeight, FontStyle FontStyle){
            this.FontFamily = new FontFamily(FontName);
            this.FontSize   = FontSize;
            this.FontWeight = FontWeight;
            this.FontStyle  = FontStyle;
        }
    }



	public class GFormattedText{
        private CultureInfo CulInfoJpn=CultureInfo.GetCultureInfo("ja-JP");
        private Typeface TFace; //##
        private GFont    _GFont;
        public GFont gFnt{
            set{
                _GFont = value; 
                TFace  = new Typeface( _GFont.FontFamily, _GFont.FontStyle, _GFont.FontWeight, FontStretches.Medium );
            }
        }

        public GFormattedText( GFont gf ){
            gFnt = gf;
        }

        public FormattedText GFText( string st, Brush br ){
            FormattedText FT = new( st, CulInfoJpn,FlowDirection.LeftToRight, TFace, _GFont.FontSize,br, App_Environment.pixelsPerDip );                             
            return FT;
        }
    }
}
