using GIDOO_space;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Diagnostics.Debug;
using static System.Math;

namespace GNPX_space{
    using pRes=Properties.Resources;
	using pGPGC = GNPX_Puzzle_Global_Control;
    public partial class Func_DevelopWHS: Page{
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 


        private GNPX_AnalyzerMan    pAnMan => GNPX_Eng.AnMan;

		static public GNPX_App_Man 	pGNPX_App;
		static private G6_Base		G6 => GNPX_App_Man.G6;
		static public GNPX_win		pGNP00win => pGNPX_App.pGNP00win;
        private GNPX_Engin			GNPX_Eng => pGNPX_App.GNPX_Eng;                      //Analysis Engine




		static private string       pathName = "develop/";
        public  GNPX_Graphics       gnpxGrp;
        private RenderTargetBitmap  bmpGZero;
        private List<UCell>         pBDLMemo;
        private bool                sNoAssist=true;


		public object Culture{ get=> pRes.Culture; }

        private UPuzzle             P0_dev;
        private UPuzzle             P1_sol;
        private DispatcherTimer     onWorkingTimer;
        private int                 onWorkingCounter=0;

//		private int					OnWork = 0;
        private bool				ErrorStopB;

		private Stopwatch           AnalyzerLap = new Stopwatch();

        private CancellationTokenSource cts;
        private string				taskCompInfo => G6.taskCompInfo;
        private string              fName_Transformed_1Removed        => pathName+"hardSuDoKu_Transformed_1Removed.txt";
        private string              fName_Transformed_12Added         => pathName+"hardSuDoKu_Transformed_12Added.txt";
        private string              fName_Transformed_X = null;
        private string              fName_btn_CharacteristicAnalysis      => pathName+"hardSuDoKu_btn_CharacteristicAnalysis.txt";

        private string              fName_GNPX_Solution               => pathName+ "hardSuDoKu_GNPX_Solution.txt";
        private string              fName_TryAndError_Solution        => pathName+ "hardSuDoKu_TryAndError_Solution.txt";
        private string              fName_TryAndError_SolutionSummary => pathName+ "hardSuDoKu_TryAndError_SolutionSummary.txt";        
        private ALSLinkMan          ALSMan;

		private string				sortMode="ID";






        public Func_DevelopWHS( GNPX_App_Man GNPX_App){
			pGNPX_App = GNPX_App;

            InitializeComponent();

			Send_Command_to_GNPXwin += new GNPX_EventHandler( pGNP00win.GNPX_Event_Handling_man );  
        }

		private void Page_Loaded(object sender, RoutedEventArgs e) {
			G6.PG6Mode = GetType().Name;
		}


 		private void Send_Board( List<UCell> board ){
			Gidoo_EventHandler se = new Gidoo_EventHandler( eName:"boardToDisp", Board:board, bPara:sNoAssist );  //Report
			Send_Command_to_GNPXwin( this, se );
		}



        private void btn_ReadPuzzle_Click(object sender, RoutedEventArgs e){
//            pathName = pGNP00win.Read_Puzzle()+"/";
            P0_dev   = pGPGC.GNPX_Get_Puzzle(0);
            lbl_Messafg.Content = (P0_dev==null)?"Puzzle undefined.": "";
            if( P0_dev != null ) Set_dev_GBoard(P0_dev.BOARD, dispOn:sNoAssist);
        }

        private void chbAnalyze00_Checked(object sender, RoutedEventArgs e){
            if( bmpGZero == null ) return;
            sNoAssist = (bool)chbx_ShowCandidate.IsChecked;//chbx_ShowNoUsedDigits
            if( P0_dev !=null )  Set_dev_GBoard(P0_dev.BOARD, dispOn:sNoAssist );
        }
        public void Set_dev_GBoard( List<UCell> pBOARD, bool dispOn=false ){
            pBDLMemo = pBOARD;
            pAnMan.Update_CellsState(pBDLMemo, setAllCandidates:true);

            //dev_GBoard.Source = gnpxGrp.GBoardPaint( bmpGZero, pBOARD, sNoAssist:sNoAssist);
			Send_Board( pBOARD );
        }
        private void devWin_IsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e ){
            if(pBDLMemo==null)  return;
            //dev_GBoard.Source = gnpxGrp.GBoardPaint( bmpGZero, pBDLMemo, sNoAssist:true );
			Send_Board( pBDLMemo );
        }


        private void btn_SolveContinuously_Click(object sender, RoutedEventArgs e){
            onWorkingTimer.Start();
//#@            pGNPX_App.GNPX_Eng.Set_Methods_for_Solving(AllMthd: false);  //true:All Method 
            string stR;
            int nc = 0;
            UPuzzle UP = new UPuzzle();
            using( var fpR=new StreamReader(fName_Transformed_X) )
            using( var fpW=new StreamWriter(fName_GNPX_Solution, false, Encoding.UTF8) ){
                while( (stR= fpR.ReadLine()) != null ){
                    stR = stR.Substring(0,81);
                    UP.BOARD = pGNPX_App._stringToBDL(stR);
                    string stSolM= pGNPX_App.SetSolution( UP, SolSet2:true, SolAll:true );
                    string stSol="";
                    GNPX_Eng.ePZL.BOARD.ForEach(q => { stSol += Abs(q.No).ToString(); });

                    stSol = stSol.Replace("0",".");
                    string st = $"{stR} {stSol} {stSolM}";
                    fpW.WriteLine( st );                   
                    this.Dispatcher.Invoke(() => { lblDone.Content = $"{(++nc)}"; });
                }
            }
            onWorkingTimer.Stop();
       //     elp_OnWork.Visibility = Visibility.Hidden;
            return;
        }

        private void chbx_ShowSolution_Checked(object sender, RoutedEventArgs e ){
            if( P0_dev==null )  P0_dev = new UPuzzle( GNPX_Eng.ePZL.BOARD, resetB:true );
//##            P1_sol = pGNP00win.TransSolverA("Checked", true);
            if( P1_sol ==null ){
                lbl_Messafg.Content = "This is an unsolvable puzzle.";
                return;
            }

            UPuzzle PX = ((bool)chbx_ShowSolution.IsChecked)? P1_sol: P0_dev;
            //dev_GBoard.Source = gnpxGrp.GBoardPaint(bmpGZero, PX.BOARD, sNoAssist: sNoAssist);
			Send_Board( PX.BOARD );
            pGNPX_App._Develop_Puzzle = PX.BOARD;

            Output_Develop_SuDoKuName(PX.BOARD);
            return;


            // ----- inner function -----
            void Output_Develop_SuDoKuName( List<UCell> pBOARD ){
                string st = "";
                int rc=0;
                foreach( var P in  pBOARD ){
                    st += (P.No==0)? ".": $"{Math.Abs(P.No)}";
                    if( (++rc)%9 == 0 ) st += " ";
                }

                if( rc>=81 ){
                    using( var fpW=new StreamWriter(pGNPX_App._Develop_SuDoKuName, false, Encoding.UTF8) ){
                        fpW.WriteLine(st);
                    }
                }
            }
        }


        private void btn_Generate_Puzzle_1Removed_Click(object sender, RoutedEventArgs e) {
            if( !Directory.Exists(pathName) )  Directory.CreateDirectory(pathName);

            var rcNotZero = P0_dev.BOARD.FindAll(p=>p.No!=0);
            int nSize = rcNotZero.Count;
            using(var fpW = new StreamWriter(fName_Transformed_1Removed, false, Encoding.UTF8)){
                for( int n=0; n<nSize; n++ ){       
                    var solCan = P0_dev.BOARD.ConvertAll(p=>p.No);
                    int rcRem = rcNotZero[n].rc;
                    solCan[rcRem] = 0;

                    string record = string.Join("",solCan).Replace("0",".");
                    fpW.WriteLine(record);
                }
            }
            fName_Transformed_X = fName_Transformed_1Removed;
            lbl_fNmae.Content = fName_Transformed_X;
        }

        private void btnGenerate_Puzzle_12Add_Click_1(object sender, RoutedEventArgs e){
            // Add 1-2 numbers to Hard SuDoKu
        
            P0_dev = pGPGC.GNPX_Get_Puzzle(0);    // Puzzle to be analyzed
            P1_sol = pGPGC.GNPX_Get_Puzzle(1);    // Solution
            if( P0_dev==null || P1_sol==null ){
                lbl_Messafg.Content = $"P0_dev:{((P0_dev==null)?"null":"ok")}  P1_sol:{((P0_dev==null)?"null":"ok")}";
                return;
            }

            if( !Directory.Exists(pathName) )  Directory.CreateDirectory(pathName);
            var Q = P0_dev.BOARD.ConvertAll(p=>p.No.ToString());
            List<UCell> SelZero = new List<UCell>();
            foreach( var P in P0_dev.BOARD.Where(p=>p.No==0) ) SelZero.Add(P);
            List<int> solCan = new List<int>();
            foreach (var P in P0_dev.BOARD ) solCan.Add(P.No);

            // Generate a puzzle file with 1-2 cells returned to blank.
            using(var fpW = new StreamWriter(fName_Transformed_12Added, false, Encoding.UTF8)){
                for( int n=1; n<=2; n++ ){ 
                    int[] indx = new int[n]; 
                    Combination cmb = new Combination(SelZero.Count,n);
                    int nxt = 1;
                    while (cmb.Successor(skip: nxt)){
                        for( int k=0; k<n; k++ ){
                            int mx = cmb.Index[k];
                            int rc = SelZero[mx].rc;
                            solCan[rc] = P1_sol.BOARD[rc].No;
                            indx[k] = rc;
                        }
                        string record = string.Join("",solCan).Replace("0",".");
                        fpW.WriteLine(record);

                        for( int k=0; k<n; k++ ) solCan[indx[k]] = 0;
                    }
                }
            }
            fName_Transformed_X = fName_Transformed_12Added;
            lbl_fNmae.Content = fName_Transformed_X;

        }

    #region Solve
        private void btn_Solve_TryAndError_cont_Click( object sender,RoutedEventArgs e ){
            // solve the problem by trial and error.
//z            fName_reducedPuzzle = pathName+"hardSuDoKu_reduced.txt";
            if( !File.Exists(fName_Transformed_X) ){
                lbl_Messafg.Content = $"file:{fName_Transformed_X} is not exist.";
                return;
            }

            onWorkingTimer.Start();
         // pGNPX_App.GNPX_Eng.Set_Methods_for_Solving(AllMthd: false);  //true:All Method 

            string stR;
            int nc = 0;
            UPuzzle UP = new UPuzzle();

            using( var fpR=new StreamReader(fName_Transformed_X) )

            using( var fpW=new StreamWriter(   fName_TryAndError_Solution, false, Encoding.UTF8) )
            using( var fpWSum=new StreamWriter(fName_TryAndError_SolutionSummary, false, Encoding.UTF8) ){
                while( (stR= fpR.ReadLine()) != null ){
                    stR = stR.Substring(0,81);
                    UP.BOARD = pGNPX_App._stringToBDL(stR);
		                    WriteLine( $"\r{stR}");

                    Research_trial RT = new( pAnMan );
                    pGNPX_App.SetSolution( UP, false, SolAll:true ); //Solver
                    var SolList =  RT.RT_Get_SolList;

                    fpWSum.WriteLine( $"{stR} solutions:{SolList.Count}" );
                    fpW.WriteLine( $"{stR} solutions:{SolList.Count}" );
                    foreach( var sol in SolList ){
                        string stSolM = sol.Aggregate("", (s,p)=> s+Abs(p).ToString() );
                        fpW.WriteLine(stSolM);
                        WriteLine(stSolM);
                    }
                    fpW.WriteLine( "" );
                    this.Dispatcher.Invoke(() => { lblDone_TaE.Content = $"{(++nc)}"; });
                }
            }
            onWorkingTimer.Stop();
            elp_OnWork.Visibility = Visibility.Hidden;
            return;
        }
		#endregion Solve



    #region btn_SaveBitMap
        private void btn_SaveBitMap_Click( object sender, RoutedEventArgs e ){
            BitmapEncoder enc = new PngBitmapEncoder(); // JpegBitmapEncoder(); BmpBitmapEncoder();
            BitmapFrame bmf = BitmapFrame.Create(bmpGZero);
            enc.Frames.Add(bmf);
            try{
                Clipboard.SetData(DataFormats.Bitmap,bmf);
            }
            catch(System.Runtime.InteropServices.COMException){ /* NOP */ }

            if( !Directory.Exists(pRes.fldSuDoKuImages) ){ Directory.CreateDirectory(pRes.fldSuDoKuImages); }
            string fName=DateTime.Now.ToString("yyyyMMdd HHmmss Develop")+".png";
            using( Stream stream = File.Create(pRes.fldSuDoKuImages+"/"+fName) ){
                enc.Save(stream);
            }      
            
            lbl_Messafg.Content = "saved:"+ fName;
        }
		#endregion btn_SaveBitMap

        private void btn_CharacteristicAnalysis_Click(object sender, RoutedEventArgs e) {

            using( var fpW=new StreamWriter(fName_btn_CharacteristicAnalysis, false, Encoding.UTF8) ){
                int[] numLst = new int[10];
                
                foreach( var P in P0_dev.BOARD ){
                    int n = P.FreeBC;
                    numLst[n]++;
                }
                string st = "number of elements:";
                for( int n=0; n<10; n++ )  st += $" {n}:{numLst[n]}";
                fpW.WriteLine( st ); 
                WriteLine( st );
            }
        }
        private void btn_AnalysisALS_Click( object sender, RoutedEventArgs e ){
            if( ALSMan is null ) ALSMan = new ALSLinkMan(pAnMan);

            ALSMan.Initialize();
            int minSize = nUD_ALS_minSize.Value;
            int nPlus   = nUD_ALS_plus.Value;
            ALSMan.Prepare_ALSLink_Man( nPlus, 1, setCondInfo:true, debugPrintB:true );

            if( ALSMan.ALSList==null ) return;
            ALSMan.QSearch_Cell2ALS_Link();  //LinkCeAlsLst[rc]

            string fName_ALS_Analyzing = pathName+"hardSuDoKu_ALS_Analyzing.txt";
            using( var fpW=new StreamWriter(fName_ALS_Analyzing, false, Encoding.UTF8) ){

                string st = $" Cell -> ALS";
                WriteLine(st); fpW.WriteLine(st);

                foreach( var P in ALSMan.LinkCeAlsLst.Where(p=>p!=null && p.Count>1) ){
                    st = $"rc:{P[0].UC.rc} count:{P.Count}";
                    WriteLine(st); fpW.WriteLine(st);

                    for( int k=0; k<P.Count; k++ ){ 
                        st = $"k:{k} :{P[k]}";
                        WriteLine(st); fpW.WriteLine(st);
                    }

                    Combination cmb = new Combination(P.Count,2);
                    while( cmb.Successor() ){
                        var UL = P[cmb.Index[0]];
                        var UA = P[cmb.Index[0]].ALS;
                        var UB = P[cmb.Index[1]].ALS;

                        int RCC = ALSMan.Get_AlsAlsRcc(UA, UB);
                        if( RCC == 0 )  continue;
                        st = $"\r\r rc:{UL.UC.rc} RCC:{RCC.ToBitString(9)}\r 1*{UA}\r 2*{UB}";
                        WriteLine(st); fpW.WriteLine(st);
                    }
                }
            }
        }
	}
}
