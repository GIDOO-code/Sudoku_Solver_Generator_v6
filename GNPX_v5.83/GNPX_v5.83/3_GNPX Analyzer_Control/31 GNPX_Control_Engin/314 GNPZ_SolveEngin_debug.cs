using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media;

using static System.Diagnostics.Debug;
using static System.Math;

using GIDOO_space;
using System.IO;
using System.Text;
using static GNPX_space.GNPX_App_Ctrl;
using GNPX_space;



namespace GNPX_space{
    public partial class GNPX_Engin{                        // This class-object is unique in the system.
		static public  UInt128 b081_all  = (UInt128.One<<81)-1;
        private UInt128[]   pConnectedCells81 => AnalyzerBaseV2.ConnectedCells81;
		private UInt128[]   pHouseCells81	   => AnalyzerBaseV2.HouseCells81; 

		public  string      TandE_st_Puzzle="";

        public int[]		sol_int81;
		public  string      TandE_st_sol_int81="";
        public string       Result{   get=>ePZL.Sol_Result; set=>ePZL.Sol_Result=value; }
        public string       ResultLong{   get=>ePZL.Sol_ResultLong; set=>ePZL.Sol_ResultLong=value; }

		public string       extResult{   get=>ePZL.extResult; set=>ePZL.extResult=value; }

		private string		fName_BugInApplication;

		private object obj = new object();
		private string dirDebugStr = "Debug";



        public bool  IsSudokuPuzzle_TE_Check( bool debugPrint=false ){
	 		//if( IsCreatePuzzle ){ sol_int81=null; return true; }   // This will not work in @CreatePuzzle.

			List<int> intBoard = ePZL.BOARD.ConvertAll( P=> Max(P.No,0) );
			TandE_st_Puzzle = string.Join("",intBoard);

			Stopwatch sw = new Stopwatch();
			sw.Start();
				
					fName_BugInApplication = $"{DateTime.Now.ToString("yyyyMMdd_hhMM")}.txt";

					RTrial = new Research_trial(AnMan);

					LatinSquare_9x9 Q = new(  intBoard );
					bool validF = RTrial.TrialAndErrorApp( Q.SolX.ToList(), filePutB:false, upperLimit:2 );   
					sol_int81 = RTrial.RT_Get_Solution_iAbs;

					if( !validF ){
						if( !Directory.Exists(dirDebugStr) ){ Directory.CreateDirectory(dirDebugStr); }
						string stFname = $"dirDebugStr/{fName_BugInApplication}";
					}

				
			var elps = sw.ElapsedMilliseconds;
			if(debugPrint){
				if( sol_int81 !=null ){	WriteLine( $"{string.Join("",sol_int81)} ... elapsed time:{elps}ms" ); }
				else{					WriteLine( $"There are multiple solutions ... elapsed time:{elps}ms" ); }
			}

			return validF;
        }




		public bool IsSolutionValid( ){

			bool test_Error( UCell p ) => (p.FixedNo>0 && p.FixedNo!=sol_int81[p.rc]) | (p.CancelB>0 && p.CancelB.IsHit(sol_int81[p.rc]-1) );

			bool errorB = false;
			List <UCell> pBOARD = ePZL.BOARD;

			try{
				// This will not work in @CreatePuzzle.
				if( !G6.PG6Mode.Contains("Func_Create") ){
					errorB = pBOARD.Any( p=> (p.FixedNo>0 && p.FixedNo!=sol_int81[p.rc] ) )
						   | pBOARD.Any( p=> (p.CancelB>0 && (p.CancelB & (1<<(sol_int81[p.rc]-1)))>0 ) );
				}
			}
			catch(Exception e){ WriteLine( $"{e.Message}\n{e.StackTrace}" ); }

			if( !errorB ){
				UInt128 B81Free = pBOARD.Create_Free_BitExp128();
				UInt128 B81Fixed = B81Free ^ b081_all;

				for( int h=0; h<27; h++ ){
					UInt128 b81 = B81Free & pHouseCells81[h];
					int n9 = b81.BitCount();
					if( n9 == 0 )  continue;
					int freeB = b81.IEGet_UCell( pBOARD ).Aggregate(0, (p,q)=> p | q.FreeB );
					if( n9 == freeB.BitCount() )  continue;
					errorB = true;
				}
			}

			if( !errorB ) return (!errorB);

			string dirError = "Algorithm_Error";
			if( !Directory.Exists(dirError) )  Directory.CreateDirectory(dirError);

			fName_BugInApplication = $"{dirError}/{DateTime.Now.ToString("yyyyMMdd_hhMM")}.txt";
            Result += "  ... error ...";
            string stError = $"\n\n ... error ...\n  file:{fName_BugInApplication}"; 	
			ResultLong += stError; 			
			
			if( extResult == "" )  extResult = ResultLong; 
			extResult += stError;

		//	G6.g7CurrentState = pBOARD.Copy();  // .... ver. 7.0

			G6.g7CurrentState = new();
			pBOARD.ForEach( Q => G6.g7CurrentState.Add(Q.Copy()) ); // .... ver. 7.0

			string st0 = string.Join("",pBOARD.ConvertAll(p=>Max(p.No,0))).Replace("0","."). AddSpace9_81();
			string st1 = string.Join("",pBOARD.ConvertAll(p=> Abs(p.No))).Replace("0","."). AddSpace9_81();
			string st2 = string.Join("",sol_int81.ToList()). AddSpace9_81();

				
				string[]  Marks = new string[81];
				string stLT2 = $"Puzzle sq:{ePZL.ID+1} stage:{ePZL.stageNo} Method:{AnalyzingMethod.NameM}  IsSolutionValid ... erroe ...";
				pBOARD._Dynmic_CellsPrint_withFrame( Marks, stLT2 );


			string stLT = $"Puzzle sq:{ePZL.ID+1} stage:{ePZL.stageNo} Method:{AnalyzingMethod.NameM}  IsSolutionValid ... erroe ...\n         < Current >                   < Puzzle >";
			string stL  = Utility_Display.__DBUG_Print2( pBOARD, sqF:true, stLT );

			string stUnmatch = pBOARD.Aggregate( "", (a,p) => a + (test_Error(p)? "X": "@") ). AddSpace9_81(). Replace("@", " ") ;

			foreach( var P in pBOARD.Where( p=> (p.FixedNo>0 && p.FixedNo!=sol_int81[p.rc]) ) ){
				string stF = $"\n error P.FixedNo: {P.rc.ToRCString()}#{P.FixedNo} is false, #{sol_int81[P.rc]} is true";
				stL += stF; 
				WriteLine( stF );
			}
			foreach( var P in pBOARD.Where( p=> (p.CancelB>0 && (p.CancelB & (1<<(sol_int81[p.rc]-1)))>0)) ){
				string stC = $"\n error CancelB: {P.rc.ToRCString()}#{P.CancelB.ToBitStringN(9)} is false, #{sol_int81[P.rc]} is true";
				stL += stC; 
				WriteLine( stC );
			}

			string st012 = $"\n\n  {st0}   Puzzle\n  {st1}   Current\n  {st2}   Solution\n  {stUnmatch}   Error";
			stL += st012;
			extResult += stL;
			WriteLine( st012 );

            using( var fpW=new StreamWriter( fName_BugInApplication, append:true, encoding:Encoding.UTF8) ){
				fpW.WriteLine( $"\n ... error ... {DateTime.Now} MethodName : {AnalyzingMethod.MethodName}" );					
				fpW.WriteLine( stL );
			}
            return (!errorB);
        }
    }
}