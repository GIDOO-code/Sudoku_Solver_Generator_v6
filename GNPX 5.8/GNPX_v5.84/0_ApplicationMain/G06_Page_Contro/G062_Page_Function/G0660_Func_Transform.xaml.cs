using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Diagnostics.Debug;

using static System.Math;

namespace GNPX_space{
    using sysWin = System.Windows;
    using pApEnv = App_Environment;
    public partial class Func_Transform: Page{

		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 
		public event GNPX_EventHandler Send_Command_to_Page;

		static public GNPX_App_Man 	pGNPX_App;
		static private G6_Base		G6 => GNPX_App_Man.G6;
		//static public GNPX_win		pGNP00win => pGNPX_App.pGNP00win;
		static public GNPX_win		pGNP00win;
        private GNPX_Engin			pGNPX_Eng => pGNPX_App.GNPX_Eng;       //Analysis Engine
		private GNPX_AnalyzerMan    pAnMan    => pGNPX_Eng.AnMan;  // pGNPX_App.pGNPX_Eng.AnMan;

		public  UPuzzle				ePZL{ get=>pGNPX_Eng.ePZL; set=>pGNPX_Eng.ePZL=value; }

        private GPuzzleTrans		pPTrans;
		private int					noPChg = -1;
		private UPuzzle				ePZL_Initial;


		// The Random initial value of the random number is fixed.
		// ->  The same random number is always used.
		// ->  Reproducibility is maintained even when debugging.
		private Random				rnd_Transform = new Random(6);



		// :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        public Func_Transform( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;
			pGNP00win = pGNPX_App.pGNP00win;
			pPTrans   = new( pGNPX_App );

            InitializeComponent();

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );      
			pGNP00win.Send_Command_to_Func_Transform += new GNPX_EventHandler( Event_Reciever );
        }

		private void Page_Loaded(object sender, RoutedEventArgs e ){ // Executes when transitioning to this page.
			G6.PG6Mode = GetType().Name; 
			G6.DigitsChangeMode = false;
			ePZL_Initial = ePZL.Copy();
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e ){ // Executes when leave this page.
			G6.DigitsChangeMode = false;
        }

 		private void Send_SetPuzzleOnBoard(){	// ( General purpose. )
			Send_Command_to_GNPXwin( this, new Gidoo_EventHandler( eName:"Show_AnalysisStatus" ) );
		}
		// :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::






        private void btn_NumChange_Click( object sender, RoutedEventArgs e ){
			G6.DigitsChangeMode = true;

            btn_NumChange.IsEnabled=false;
            btn_NumChangeRandom.Visibility=Visibility.Hidden;
            TransSolverA("NumChange",true); //display solution

            txb_NumChange.Text = "1";
            txb_NumChange.Visibility = Visibility.Visible;
            btn_NumChangeDone.Visibility = Visibility.Visible;
            noPChg = 1;            


            Send_SetPuzzleOnBoard( DevelopB:true );		//ボードクリック受信状態を設定
        }

        private void btn_NumChangeDone_Click( object sender, RoutedEventArgs e ){
            btn_NumChange.IsEnabled=true;
            btn_NumChangeRandom.Visibility=Visibility.Visible;
            txb_NumChange.Visibility = Visibility.Hidden;
            btn_NumChangeDone.Visibility = Visibility.Hidden;
            noPChg = -1;

			G6.DigitsChangeMode = false;

            Send_SetPuzzleOnBoard();
        }
		
		public void Event_Reciever( object sender, Gidoo_EventHandler e ){  //Board click receiver
			if( G6.DigitsChangeMode is false )  return;		//It's excessive, but.
			int rc = e.ePara0;		//WriteLine( $" --- Func_Transform / Reciever rc:{e.ePara0}" );
			_Change_PB_GBoardNum( rcX:e.ePara0 ) ;
			return;

					// ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- 
					void _Change_PB_GBoardNum( int rcX ){
						if(rcX<0) return;
						int noP=Abs(ePZL.BOARD[rcX].No);
						if( noP==0 )  return;
						if( noP>noPChg){
							foreach( var q in ePZL.BOARD.Where(r=>r.No!=0) ){                 
								int nm=q.No, nmAbs=Abs(nm), nmSgn=Sign(nm);
								if( nmAbs<noPChg ) continue;
								else if( nmAbs==noP ) q.No = nmSgn * noPChg;
								else if( nmAbs<noP )  q.No = nmSgn * (nmAbs+1);
							}
						}
						else if( noP<noPChg ){          
							foreach( var q in ePZL.BOARD.Where(r=>r.No!=0) ){                 
								int nm=q.No, nmAbs=Abs(nm), nmSgn=Sign(nm);
								if( nmAbs<noP ) continue;
								else if( nmAbs==noP )    q.No = nmSgn * noPChg;
								else if( nmAbs<=noPChg ) q.No = nmSgn * (nmAbs-1);
							}           
						}

						Send_SetPuzzleOnBoard();

						noPChg++;
						txb_NumChange.Text = noPChg.ToString();
						if(noPChg>9) btn_NumChangeDone_Click(this,new RoutedEventArgs());
						return;
					}
		}

        private void btn_NumChangeRandom_Click(object sender,RoutedEventArgs e){
            TransSolverA("random",true);

            List<int> ranNum = new List<int>();
            for(int r=0; r<9; r++)  ranNum.Add( rnd_Transform.Next(0,9)*10+r );
            ranNum.Sort((x,y)=>(x-y));
            for(int r=0; r<9; r++) ranNum[r] %= 10;

            foreach( var q in ePZL.BOARD.Where(p=>p.No!=0) ){
                int nm=q.No, nm2=ranNum[Abs(nm)-1]+1;
                q.No = (nm>0)? nm2: -nm2;
            }
            Send_SetPuzzleOnBoard();
        }



		// :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
		private void btn_PatCVRCg_Click( object sender, RoutedEventArgs e ){
            Button btn = sender as Button;
            TransSolverA( btn.Name, (bool)chbx_ShowSolution.IsChecked );
			Send_SetPuzzleOnBoard( DevelopB:true );
        }
        public UPuzzle TransSolverA( string Name, bool DspSol ){
            G6.NoOfPuzzlesToCreate = 1;
            G6.Puzzle_LevelLow	  = 0;
            G6.Puzzle_LevelHigh   = 999;
            G6.Digits_Randomize   = false;
          //App_Man_000.App_Ctrl.CbxDspNumRandmize=false;
            G6.GenLStyp = 1;
            GNPX_Engin.SolInfoB = true;
            var aP = pPTrans.G_TransProbG( Name, DspSol );
            return aP;
        }
        
		private void chbx_ShowSolution_Checked( object sender, RoutedEventArgs e ){
			// ePZL has a solution(AnsNum). However, the matrix exchange result is different from AnsNum.
			// It is also possible to convert using G_TransProbG, but here i used "Research_trial".
			bool DspSol = (bool)chbx_ShowSolution.IsChecked;
			if( DspSol ) ePZL.BOARD = Get_Solution();
			else		 ePZL.BOARD.ForEach( P=> P.No=Max(P.No,0) );
			
			Send_Command_to_GNPXwin( this, new Gidoo_EventHandler( eName:"Show_AnalysisStatus" ) );
			return;
			
						// <<< Solve using "Research_trial". >>>
						List<UCell> Get_Solution(){
							Research_trial RTrial = new( pAnMan );
							List<int> intBoard = ePZL.BOARD.ConvertAll( P=> P.No ); 
							bool ret = RTrial.TrialAndErrorApp( intBoard, filePutB:true, upperLimit:2 );	
							
							List<UCell>  board = ret? RTrial.RT_Get_Board(): null;
							return board;
						}
        }


        private void btn_TransEst_Click( object sender, RoutedEventArgs e ){
            pPTrans.btn_TransEst();
            Send_SetPuzzleOnBoard( DevelopB:true );
        }
        private void btn_TransRes_Click(object sender, RoutedEventArgs e ){
            pPTrans.btn_TransRes();
            if(!(bool)chbx_ShowSolution.IsChecked) ePZL.BOARD.ForEach(P=>{P.No=Max(P.No,0);});
            Send_SetPuzzleOnBoard( DevelopB:true );
        }

        private void btn_Nomalize_Click( object sender, RoutedEventArgs e ){
			this.Dispatcher.Invoke(() => tbx_TransReport.Text = "\nThis will take a few seconds."  );

			pPTrans = new( pGNPX_App );
			pPTrans.Initialize( StartF:true );

            if(ePZL.AnsNum==null){
				TrialAndError_Solver();
				TransSolverA("Checked",true);
			}
			this.Cursor = Cursors.Wait;
            string st = pPTrans.G_Nomalize( (bool)chbx_ShowSolution.IsChecked, (bool)chbx_NrmlNum.IsChecked );
            tbx_TransReport.Text=st;
            Send_SetPuzzleOnBoard( DevelopB:true );
			this.Cursor = Cursors.Arrow;
			return;

						// <<< Solve using Research_trial >>>
						void TrialAndError_Solver(){
							Research_trial RTrial = new( pAnMan );
							List<int> intBoard = ePZL.BOARD.ConvertAll( P=> P.No ); 
							RTrial.TrialAndErrorApp( intBoard, filePutB:true, upperLimit:2 );

							ePZL.AnsNum = RTrial.RT_Get_Solution_iArray;
						}    


        }

 		private void Send_SetPuzzleOnBoard( bool DevelopB=false ){
			var se = new Gidoo_EventHandler( eName:"Show_AnalysisStatus" );  //Report
			Send_Command_to_GNPXwin( this, se );

				//  ---- set Engin


			if( DevelopB )	_Display_PuzzleTransform();
			return;

						void _Display_PuzzleTransform(){
							int[] TrPara = pPTrans.TrPara;
							if( TrPara == null )  return;
							Lbl_Rg.Content    = TrPara[0].ToString();      
							Lbl_R123g.Content = TrPara[1].ToString();
							Lbl_R456g.Content = TrPara[2].ToString();
							Lbl_R789g.Content = TrPara[3].ToString();

							Lbl_Cg.Content    = TrPara[4].ToString();
							Lbl_C123g.Content = TrPara[5].ToString();
							Lbl_C456g.Content = TrPara[6].ToString();
							Lbl_C789g.Content = TrPara[7].ToString(); 
							Lbl_RC7g.Content  = TrPara[8].ToString();
						}
		}
    }
}