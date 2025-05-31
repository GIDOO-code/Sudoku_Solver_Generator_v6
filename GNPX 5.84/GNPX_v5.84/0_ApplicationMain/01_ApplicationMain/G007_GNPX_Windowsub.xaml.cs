using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Globalization;

using static System.Math;
using static System.Diagnostics.Debug;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Windows.Threading;
using System.Runtime.InteropServices;

using GIDOO_space;
using System.Windows.Media.Imaging;

namespace GNPX_space{

    using pRes=Properties.Resources;
    using sysWin=System.Windows;

	using pGPGC = GNPX_Puzzle_Global_Control;

    public partial class GNPX_winSub: sysWin.Window{
		private  GNPX_AnalyzerMan    AnMan => pGPGC.pGNPX_Eng.AnMan; 	
		
		public  GNPX_App_Man		 pGNPX_App;

        private  GNPX_Engin		 	 pGNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
		private G6_Base			     G6 => GNPX_App_Man.G6;
		private  GNPX_Graphics       gnpxGrp => pGNPX_App.gnpxGrp;
		private  RenderTargetBitmap  bmpGZeroA;
		public GNPX_winSub( GNPX_App_Man  pGNPX_App ){		
			InitializeComponent();  
			this.pGNPX_App = pGNPX_App;
		}

		private void Window_MouseDown( object sender, MouseButtonEventArgs e ){
            this.DragMove();
        }

		private void GNPXwinSub_Loaded(object sender, RoutedEventArgs e){ }


		public void Set_imgPB_GBoard( List<UCell> board, bool whiteBack=true ){
			bmpGZeroA = new ( (int)imgPB_GBoard.Width, (int)imgPB_GBoard.Height, 96,96, PixelFormats.Default );
			imgPB_GBoard.Source = gnpxGrp.GBoardPaint( bmpGZeroA, board,  sNoAssist:G6.sNoAssist, whiteBack:G6.sWhiteBack, finalStateB:true );
		}
				
	}

/*
bool test_Error( UCell p ) => (p.FixedNo>0 && p.FixedNo!=sol_int81[p.rc]) | (p.CancelB>0 && p.CancelB.IsHit(sol_int81[p.rc]-1) );
string stUnmatch = pBOARD.Aggregate( "", (a,p) => a + (test_Error(p)? "X": "@") ). AddSpace9_81(). Replace("@", " ") ;
*/

}	