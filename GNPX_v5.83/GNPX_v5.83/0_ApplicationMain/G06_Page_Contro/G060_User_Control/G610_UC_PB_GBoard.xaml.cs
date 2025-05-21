using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Diagnostics.Debug;
using static System.Math;
using GNPX_space;

namespace GNPX_space{
	using static System.Net.Mime.MediaTypeNames;
	using sysWin = System.Windows;
	using pGPGC  = GNPX_Puzzle_Global_Control;
	using pApEnv = App_Environment;	

    public partial class UC_PB_GBoard: UserControl{
		private  GNPX_AnalyzerMan    AnMan => pGPGC.pGNPX_Eng.AnMan; 	
		
		static public  GNPX_App_Man  pGNPX_App;

        private  GNPX_Engin		 	 pGNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine
		private G6_Base			     G6 => GNPX_App_Man.G6;
		private  GNPX_Graphics       gnpxGrp => pGNPX_App.gnpxGrp;

		private  UPuzzle			 ePZL => pGNPX_Eng.ePZL;
		private  RenderTargetBitmap  bmpGZeroA;

	    // ----- Extend -----
        private ExtendResultWin	 	 ExtResultWin;


		public   int rc;

        public UC_PB_GBoard(){
            InitializeComponent();	          
			bmpGZeroA = new ( (int)imgPB_GBoard.Width, (int)imgPB_GBoard.Height, 96,96, PixelFormats.Default );
        }



		public void Set_Puzzle( UPuzzle aPZL, bool setImage=true ){
//			if( aPZL == null )  return;
//			this.ePZL = aPZL;

				//WriteLine( $"Set_Puzzle ePZL:{ePZL.ToString_check(aPZL)}" );
				//WriteLine( $"Set_Puzzle aPZL:{aPZL.ToString_check(aPZL)}" );

			if( setImage )  Set_imgPB_GBoard( aPZL );
		}

		public void Set_imgPB_GBoard( UPuzzle aPZL ){
			
			imgPB_GBoard.Source = gnpxGrp.GBoardPaint( bmpGZeroA, aPZL.BOARD,  sNoAssist:G6.sNoAssist, whiteBack:G6.sWhiteBack );
			var (_,nP,nZ,nM) = AnMan.Aggregate_CellsPZM(aPZL.BOARD);

			txb_Puzzle.Text   = nP.ToString();
			txb_Solved.Text   = nZ.ToString();
			txb_Unsolved.Text = nM.ToString();
			nud_difficultyLevel.Text = aPZL.difficultyLevel.ToString();

			grd_overLap.Visibility = (GNPX_Graphics.overlaped)? Visibility.Visible : Visibility.Collapsed;
		}


	  #region Mouse operation
        private int     rowMemo; 
        private int     colMemo;
        private int     noPMemo;

		private void imgPB_GBoard_MouseLeftButtonDown( object sender, MouseButtonEventArgs e ){				// --- Down (first)
            var (noP,r,c) = _Get_PB_GBoardRCNumSmall( );
			if( noP <= 0 )  return;			

			rc = r*9+c;
            noPMemo=noP; rowMemo=r; colMemo=c;

		  //if( G6.P01_mode == "Create#CreateManual")  _img_PadManager( r, c, noP); 
			if( G6.PG6Mode == "Func_CreateManual" )	_img_PadManager( r, c, noP); 
			if( G6.PG6Mode == "Func_Transform"    ) G6.Cell_rc = rc;
        }
		

		// {Att] While specifying the digits, the iPad will be displayed at the top of the board,
		//       so the "Move" and "Up" events are set to the iPad.
        private void img_Pad_MouseMove( object sender, MouseEventArgs e ){								// --- Move (change)
//		    if( G6.P01_mode != "Create#CreateManual") return;
			if( G6.PG6Mode != "Func_CreateManual" )  return;

			try{
				var (noP,r,c) = _Get_PB_GBoardRCNumSmall( );
				if( noP < 0 ) return;

				if( noP!=noPMemo || r!=rowMemo || c!=colMemo ){ 
					noPMemo=noP; rowMemo=r; colMemo=c;
					_img_PadManager(r,c,noP);	//Redraw img_Pad when selection changes
						//WriteLine( $"+++ img_Pad_MouseMove (noP,r,c):({noP},{r},{c})" );
				}
			}
			catch(Exception e2){ WriteLine( $"{e2.Message}\n{e2.StackTrace}" );}
        }  

		private void img_Pad_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ){				// --- Up (Confirm)	
//			if( G6.P01_mode != "Create#CreateManual") return;
			if( G6.PG6Mode != "Func_CreateManual" )  return;

			var (noP,r,c) = _Get_PB_GBoardRCNumSmall( );
			if( noP < 0 ) return;
            if( noP!=noPMemo ){ noPMemo=noP; _img_PadManager(r,c,noP); }
                    //WriteLine( $"img_Pad_MouseLeftButtonUp (noP,r,c):({noP},{r},{c})" );

            if(noP<=0){
                img_Pad.Visibility = Visibility.Hidden;
                rowMemo=-1; colMemo=-1;
                return;
            }
            if( r!=rowMemo || c!=colMemo ) return;

            UCell BDX = ePZL.BOARD[rowMemo*9+colMemo];
            if( Abs(BDX.No) == noP )  BDX.No=0;			//Cancel selected digit.	
            else{
                BDX.No=0; 
                pGNPX_Eng.AnMan.Update_CellsState( pGNPX_Eng.ePZL.BOARD );
                if( ((BDX.FreeB>>(noP-1))&1)!=0) BDX.No=noP;
            }
          
            pGNPX_Eng.AnMan.Update_CellsState( pGNPX_Eng.ePZL.BOARD );
            img_Pad.Visibility = Visibility.Hidden;
            rowMemo=-1; colMemo=-1;

            Set_Puzzle( ePZL );
        }

        private void _img_PadManager( int r, int c, int noP ){              //Draw img_Pad
            noPMemo = noP;
			img_Pad.Source = null;
            img_Pad.Source = gnpxGrp.CreateCellImageLight( ePZL.BOARD[r*9+c], noP );
            int PosX = (int)imgPB_GBoard.Margin.Left +2 +37*c +(int)c/3;
            int PosY = (int)imgPB_GBoard.Margin.Top  +2 +37*r +(int)r/3;        
            img_Pad.Margin = new Thickness(PosX, PosY, 0,0 );        
            img_Pad.Visibility = Visibility.Visible;           
        }    

        private (int,int,int)   _Get_PB_GBoardRCNumSmall( ){
            int cSz = pApEnv.cellSize, LWid = pApEnv.lineWidth;

            int r0=-1, c0=-1;
            sysWin.Point pt = Mouse.GetPosition(imgPB_GBoard);
            int cn = (int)pt.X-2,  rn = (int)pt.Y-2;

            cn = cn - cn/cSz - cn/(cSz*3+LWid)*LWid;
            c0 = cn/cSz;
            cn = (cn%cSz)/12;
            if( cn<0 || cn>=3 || c0<0 || c0>=9 ){ r0=c0=-1;  return (-1,-1,-1); }
            
            rn = rn - rn/cSz - rn/(cSz*3+LWid)*LWid;
            r0 = rn/cSz;
            rn = (rn%cSz)/12;
            if(rn<0 || rn>=3 || r0<0 || r0>=9){ r0=c0=-1;  return (-1,-1,-1); }

			int n0 = rn*3+cn+1; 
            return ( n0, r0, c0);
        }

		#endregion Mouse operation


		private void imgPB_GBoard_PreviewKeyDown(object sender, KeyEventArgs e) {
			WriteLine( "imgPB_GBoard_PreviewKeyDown" );
		}
	}
}