using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading;
using System.Diagnostics;

using static System.Diagnostics.Debug;
using static System.Math;
using GIDOO_space;


namespace GNPX_space{
    public partial class GNPX_Engin{                        // This class-object is unique in the system.
		static public event GNPX_EventHandler Send_Command_to_GNPXwin; 

		static public bool		 MltAnsSearch;
        static public bool       SolInfoB;    
        static public int        eng_retCode;						// result
        static public UAlgMethod AnalyzingMethod;
		static public string     AdditionalMessage;
        static public TimeSpan   SdkExecTime;  
        static public bool       SolverBusy=false;
		static public UAlgMethod method_maxDiffcult0;
		static private int		 __GCC__=0;

        public  GNPX_App_Man     pGNPX_App;							// main control     
		private G6_Base			 G6 => GNPX_App_Man.G6;


        public  GNPX_AnalyzerMan AnMan;								// analyzer(methods)
		public  Research_trial   RTrial;

        private bool             __ChkPritnt_SolverMethod = false;	// for debug	//#####

		
		public  UPuzzle          PZL_Initial;

        public  UPuzzle          ePZL = new(); // Puzzle to analyze
        private int              stageNo{ get  => ePZL.stageNo; }
        private  int             stageNo100; 
        public  int              Add_stageNo() => stageNo100 +=100;    

        public int               stageNoP{ get => stageNo100 + stageNo; }
     
		private Stopwatch		 AnalyzerLap;
		private string			 stNotSolved="", stNotSolvedJP="";

		public  bool			 AnalysisResult = false;
        private List<UAlgMethod> SolverList_App{ get{ return pGNPX_App.SolverList_App; } }  // List of algorithms to be applied.


        public GNPX_Engin( GNPX_App_Man  pGNPX_App ){           //Execute once at GNPX startup.
            this.pGNPX_App = pGNPX_App; 
            AnMan = new GNPX_AnalyzerMan(this);
			RTrial = new(AnMan);	
			
			this.ePZL = new UPuzzle();
		
			AnalyzerLap = new Stopwatch();

			stNotSolved = "\n This is a Sudoku Puzzle,\n There is no algorithm that can solve this.";
			stNotSolved += "\n\n It is necessary to adjust the difficulty level or\n develop a new algorithm.";

			stNotSolvedJP = "\n これは数独の問題です。\n 指定のアルゴリズムでは解けません。";
			stNotSolvedJP += "\n\n アルゴリズムの難度を調整するか、\n 新たなアルゴリズムの開発が必要です。";
        }


      #region Puzzle management
        public void Clear_0(){
            ePZL.ToInitial( ); // Clear all.
            AnMan.Update_CellsState( ePZL.BOARD, setAllCandidates:true );
        }
        public bool IsSolved() => ePZL.BOARD.All(p=> p.No!=0);

		// <<< Use TandE to find the solution >>>
		public (bool,string) IsValidPuzzle(){
			List<int>  _intBoard = ePZL.BOARD.ConvertAll(P=>Max(P.No,0));
			bool ret = false;
			try{
				ret = RTrial.TrialAndErrorApp( _intBoard, filePutB:true, upperLimit:2 );
					this.sol_int81 = RTrial.RT_Get_Solution_iAbs;;
				ePZL.SolCode = 0;	// Cancel the“SolCode=1”setted by TrialAndErrorApp.
			}
			catch(Exception ex){
				// ... not a latin square
				//    Index was out of range. at "991 GNPZ_An99_Research_trial.cs:line 83"
				ret = false;
				RTrial.Result = $"Invalid Puzzle\n  not a latin square";
				WriteLine( $"Invalid Puzzle\n{ex.Message}\n{ex.StackTrace}" );
			}
			return (ret,RTrial.Result);
		}

        public void Set_NewPuzzle( UPuzzle aPZL ){   
            this.PZL_Initial = aPZL;
			if( aPZL.BOARD == null )  return;

				//WriteLine( $"##### Set_NewPuzzle ePZL:{ePZL.ToString_check(aPZL)}" );
				//WriteLine( $"##### Set_NewPuzzle aPZL:{aPZL.ToString_check(aPZL)}" );

            ePZL = aPZL.Copy();
            ePZL.selectedIX = -1;
        }



        public void Set_selectedChild( int selX ){
            int cnt = ePZL.Child_PZLs.Count;
            if( selX<0  || cnt<=0 || selX>=cnt ) return;
            ePZL = ePZL.Child_PZLs[ selX ];
        }
		public bool Adjust_Candidates(  ){   //Cell true/false setting processing  
			if( ePZL.SolCode<0 ) return true;				// ... Already initialized. No Adjust process is required.

			var (codeX,eNChk) = AnMan.Execute_Fix_Eliminate( ePZL.BOARD );
						// codeX = 0 : Complete. Go to next stage.
						//         1 : Solved. 
						//        -1 : Error. Conditions are broken.

				if( codeX==-1 && ePZL.SolCode==-9119 ){
					string st="";
					for(int h=0; h<27; h++ ){
						if(eNChk[h]!=0x1FF){
							st+= "Candidate #"+(eNChk[h]^0x1ff).ToBitStringNZ(9)+" disappeared in "+_ToHouseName(h)+"\r";
							AnMan.SetBG_OnError(h);
						}
					}

					//txbxAnalyzerResult.Text=st;
					ePZL.SolCode = ePZL.SolCode;
					return false;
				}
				if( ePZL.SolCode==-999 )  ePZL.SolCode = -1;


			var (confirmedAll,nP,nZ,nM) = AnMan.Aggregate_CellsPZM( ePZL.BOARD );
			if( nZ==0 ){ AnMan.SolCode=0; return true; }
			return true;

						// ---------------------------------------------------
						string _ToHouseName( int h ){
							string st="";
							switch(h/9){
								case 0: st="row";    break;
								case 1: st="Column"; break;
								case 2: st="block";  break;
							}
							st += ((h%9)+1).ToString();
							return st;
						}
		}


		// <<< Stage Management >>>
/*
		public bool Set_NextStage_Origin( int _stageNo=-1, bool skip_valid_check=false ){      // update the state and create the next stage.
            if( AnMan is null )  return false;
			if( ePZL is null || ePZL.BOARD.All(p=>p.No!=0) )  return false; //solved

					if( stageNo == 0 ){ AnMan.Update_CellsState( ePZL.BOARD, setAllCandidates:true ); }
					else{ 
						int nIX = ePZL.selectedIX;
						if( nIX<0 || ePZL.Child_PZLs==null || nIX>=ePZL.Child_PZLs.Count )  return true;  //#@
						ePZL = ePZL.Child_PZLs[nIX];
					}

			UPuzzle PZL_nextStage   = ePZL.Copy();
			PZL_nextStage.pre_PZL   = ePZL;
			PZL_nextStage.stageNo   = ePZL.stageNo+1;	
			//PZL_nextStage.BOARD.ForEach( P=> P.ECrLst=null );

					if( stageNo > 0 ){
						var (codeX,_) = AnMan.Execute_Fix_Eliminate( PZL_nextStage.BOARD );
							// codeX  0:Complete. Go to next stage.  1:Solved.   -1:Error. Conditions are broken.
						if( codeX<0 ){  eng_retCode = -1; return false; }
					}

            PZL_nextStage.Child_PZLs = new List<UPuzzle>();
			ePZL = PZL_nextStage;

            return true;            //check_pGP(aPZL,"Set_NextStage");   
        }
*/
		public string Set_NextStage( int _stageNo=-1, bool skip_valid_check=false ){      // update the state and create the next stage.
      
			if( AnMan is null )  return "AnMan is null";
			if( ePZL  is null )  return "ePZL is null";
			if( ePZL.BOARD.All( p=> p.No!=0 ) )  return "solved";


			if( stageNo == 0 ){ AnMan.Update_CellsState( ePZL.BOARD, setAllCandidates:true ); }
			else{ 
				int nIX = ePZL.selectedIX;
				if( nIX<0 || ePZL.Child_PZLs==null || nIX>=ePZL.Child_PZLs.Count )  return "solved";
				ePZL = ePZL.Child_PZLs[nIX];
			}


			UPuzzle PZL_nextStage   = ePZL.Copy();
			PZL_nextStage.pre_PZL   = ePZL;
			PZL_nextStage.stageNo   = ePZL.stageNo+1;	
			//PZL_nextStage.BOARD.ForEach( P=> P.ECrLst=null );


			if( stageNo > 0 ){
				var (codeX,_) = AnMan.Execute_Fix_Eliminate( PZL_nextStage.BOARD );
					// codeX  0:Complete. Go to next stage.  1:Solved.   -1:Error. Conditions are broken.
				if( codeX == 1 )  return "solved";
				if( codeX < 0 ){  eng_retCode = -1; return "Error. Conditions are broken."; }
			}


            PZL_nextStage.Child_PZLs = new List<UPuzzle>();
			ePZL = PZL_nextStage;

            return "";            //"No problem, continue"  
        }


        public void ReturnToInitial(){
            if( PZL_Initial is null )  PZL_Initial = ePZL;
            
            this.ePZL = PZL_Initial.Copy();
            this.ePZL.stageNo = 0;
            this.Add_stageNo();
			this.ePZL.ToInitial( ); //to initial stage
		}
      #endregion Puzzle management


      #region Methods_for_Solving functions
        public void MethodLst_Run__Reset_UsedCC(){
            Add_stageNo();

            SolverList_App.ForEach(P=>P.UsedCC=0);
			GNPX_Engin.method_maxDiffcult0 = null;
        } 

        public string DGView_MethodCounterToString(){
            var Q = SolverList_App.Where(p=>p.UsedCC>0).ToList();
            return Q.Aggregate("",(a,q) => a+$" {q.MethodName}[{q.UsedCC}]" );
        }

        public void AnalyzerCounterReset(){
			if( SolverList_App==null )  return;
			SolverList_App.ForEach(P=>P.UsedCC=0);  //Clear the algorithm counter. 
		}

        public (int,string)  Get_difficultyLevel( ){
            int DifL=0;
            string pzlName="";
            if( SolverList_App.Any(P=>(P.UsedCC>0) )){
                DifL = SolverList_App.Where(P=>P.UsedCC>0).Max(P=>Abs(P.difficultyLevel));
                var R = SolverList_App.FindLast(Q=>(Q.UsedCC>0)&&(Abs(Q.difficultyLevel)==DifL));
                pzlName = (R!=null)? R.MethodName: "";
            }
            return (DifL,pzlName);
        }
      #endregion Methods_for_Solving functions






	  // ######################################################
      #region Analyzer


		
        public string GNPX_Solver_SolveUp( CancellationToken cts ){

			string stRet = "";
            eng_retCode =0;

            var (_,nP,nZ,nM) = AnMan.Aggregate_CellsPZM(ePZL.BOARD);
            eng_retCode = nZ;
			if( nZ==0 ){ stRet="SolveUp"; return stRet; }		// <<< Already solved >>>

			stRet = "--";
            try{
				ePZL = PZL_Initial.Copy();
                ePZL.stageNo = 0;

				AnalyzerLap.Reset();
				AnalyzerLap.Start();    

					// <<<<< Repeatedly apply the SingleStage solver. >>>>> 
					int RestFreeBC=1;
					while( RestFreeBC>0 ){  
								if( cts.IsCancellationRequested ){ stRet="Canceled"; break; }

						string retST = Set_NextStage(PZL_Initial.stageNo);
						if( retST == "solved" && ePZL.BOARD.Sum(P=>P.FreeBC) == 0 ){ stRet="SolveUp"; break; }			
						if( retST != "" ){ eng_retCode=-3; stRet="Not Sudoku"; break; }	

						// ================================================
						var (ret,ret2) = GNPX_Solver_SingleStage( cts:cts, SolInfoB:false ); // <<< 1-step solver >>>
						if( ret2 == "SolveUp" ){ stRet="SolveUp"; break; }
						if( !ret ){ eng_retCode=-3; stRet=ret2; break; }
					}
					// -----------------------------------------------------

                AnalyzerLap.Stop();     
				SdkExecTime = AnalyzerLap.Elapsed;
            }
			catch(Exception e){
				string message = $"{e.Message} \r{e.StackTrace}";
				WriteLine( $"---{DateTime.Now} {message}" );
				if( !message.Contains("canceled") ){
					Utility_Display.GNPX_StreamWriter_WithTime( fName:"Exception_GNPX_Solver_SolveUp.txt", message:message );//, append:true );
				}
			}

			// If the solution is successful, return "SolveUp".
			return stRet;
        }



		private int debugCC=0;

        // 1-step solver
        public (bool,string) GNPX_Solver_SingleStage( CancellationToken cts, bool SolInfoB ){
				//::::::::::::::::::::::: GC ::::::::::::::::::::::::::::
				if( ((++__GCC__)%10000)==0 ){ GC.Collect(); __GCC__=0; }     //Is it necessary? Is it effective?
																			//-------------------------------------------------------
				//__ChkPritnt_SolverMethod = true;  //######### // ===== SE_Nxt Debug =====

				string    stResult="";
				int       nonFixed = ePZL.BOARD.Count(p=>(p.FreeB!=0));
				//bool	  AnalysisResult = false;

				DateTime  MltAnsTimer = DateTime.Now;

				#region Prepare
								if(__ChkPritnt_SolverMethod){
									WriteLine( $"\n=== {G6.PatternCC}  stageNo:{stageNo}  unsolved:{nonFixed} cells" );
									if( nonFixed==0 ){ WriteLine( $"zeroCC:{nonFixed}" ); stResult="SolveUp"; }
								}
					if( nonFixed==0 ){ stResult="SolveUp"; goto LProcessEnd; }

					#region Initialize, set search conditions
					if( stageNo == 0 ){
						ePZL.method_MostDiffcult = null;
						PZL_Initial.pMethod		 = null;
						PZL_Initial.difficultyLevel = 0;
					}

					// --- Initialize global ---
					G6.stResult			= "...";
					G6.Canceled			= "";
					G6.Found_level_Solution = false;
					ePZL.Sol_ResultLong = "";
					ePZL.extResult		= "";
					AdditionalMessage   = "";
					ePZL.SolCode		= -1;

					AnalysisResult		= false;

					// --- set search conditions ---
					
					var (_,Puzzle_LevelHigh) = _Set_AcceptableLevel( );
					#endregion

  					// ---  Verify Puzzle ---
					if( ePZL.BOARD.All(p=>(p.FreeB==0)) ){ AnalysisResult=true;  stResult="SolveUp"; goto LProcessEnd; }				//break; //  Is solved. (All cells confirmed.)
					if( !AnMan.Verify_Puzzle_Roule() ){ AnalysisResult=false; stResult="no solution"; goto LProcessEnd; }  
				#endregion Prepare
				

			// <<< Search for one stage >>>
						if(__ChkPritnt_SolverMethod)  WriteLine( $" <<< SolverList_App:{SolverList_App.Count()}" );

			AnalyzerLap.Reset();
			AnalyzerLap.Start();
			
			double  ts0=0, ts1;
			foreach( var (MTHD,MTHD_idx) in SolverList_App.WithIndex() ){					   // Sequentially execute analysis algorithms
				if( cts.IsCancellationRequested ){ _PostProcessing(); return (false,"canceled"); } // Was the task canceled? 

				#region  Determining the suitability of a method
					//WriteLine( MTHD );
					if( G6.GeneralLogic_on && !MTHD.ID_caseGL )  continue;
					if( !G6.GeneralLogic_on && MTHD.GenLogB )	 continue;

					// Execution/interruption of analysis method by difficulty 
					if( G6.Found_level_Solution && Abs(MTHD.difficultyLevel)>2 )  break;	//Break if there is a solution with a difficulty level of 2 or less.

					int lvl = MTHD.difficultyLevel;
					if( G6.PG6Mode!="Func_SolveMulti" && lvl<0 )  continue;     // The negative level algorithm is used only with multiple soluving. 				
					
					// In "Func_CreateAuto" or "Func_SolveMulti",
					// Puzzles that require the specified difficulty or higher method are invalid.
					// Therefore, the method is excluded.

					if( G6.PG6Mode=="Func_CreateAuto" || G6.PG6Mode=="Func_SolveMulti" ){
						if( Abs(lvl)>Puzzle_LevelHigh )  continue;
					}
					//if( G6.AnalyzerMode == "SolveUpDev" ){ if( !MTHD.method_valid ) continue; }	// In future versions, change to "method_valid" judgment.
				#endregion
					
					//if(__ChkPritnt_SolverMethod)  WriteLine( $"   --- {MTHD_idx} {MTHD.NameM}" );  //###@@@

				#region Algorithm execution
							if(__ChkPritnt_SolverMethod){
								ts1 = AnalyzerLap.Elapsed.TotalMilliseconds;
								string stStep = $"--->GNPX_Solver_SingleStage/debuCC:{debugCC++:00} stageNo:{stageNo}";
									   stStep += $"  difficultyLevel:{MTHD.difficultyLevel}";
									   stStep += $"  (lap time: {(ts1-ts0): 0.0} msec)";
									   stStep += $"  method: {(MTHD_idx)} - {MTHD.MethodName}";
									   stStep += $" @@:{G6.stopped_StatusMessage}  "; 
								WriteLine( stStep );
								ts0 = ts1;
							}

					if( stageNo==4 && ePZL.BOARD[0].ECrLst!=null )  WriteLine( "ECrLst check" );

					try{	
						// *==* Execute *==*==*==*==*==*==*==*==*==*==*
						GNPX_Engin.AnalyzingMethod = MTHD;
						AnalysisResult = MTHD.Method();		// <<< excute method >>>
						if( G6.stopped_StatusMessage!="" )  break; // "time over"...
						// *==*==*==*==*==*==*==*==*==*==*==*==*==*==*
					}
				    
					catch(Exception e){
						string stException = e.Message + "\r" + e.StackTrace;
								WriteLine( stException );
						G6.stopped_StatusMessage = stException;
						Utility_Display.GNPX_StreamWriter_WithTime(fName:"Solver_Exception.txt", message:stException, append:true );
						_PostProcessing();
						return (false,"Exception occurred");
					}
				
				#endregion  Algorithm execution

				if( ePZL.Child_PZLs.Count>0 ){
					int diffLevel = Abs(MTHD.difficultyLevel);			// method difficult level
					if( diffLevel <= 2 )  G6.Found_level_Solution = true;
				}
				if( AnalysisResult ){ // Solved
							if(__ChkPritnt_SolverMethod) WriteLine($"                  ========================> solved  @@{MTHD}");
					_AnalysisSuccessful(MTHD);

					if( GNPX_Engin.MltAnsSearch is false )  break;		// *** In simple search, end here. ***

					// continue Searching for multiple solutions.--> continue ***
				}
			}

		  LProcessEnd:

			{
				var ePZL_Child_PZLz = ePZL.Child_PZLs;
				if(  stResult=="SolveUp" ){ AnalysisResult=true; }
				else if( ePZL_Child_PZLz!=null && ePZL_Child_PZLz.Count>0 ){  // solution found
					ePZL.selectedIX=0; ePZL=ePZL_Child_PZLz[0]; 
					AnalysisResult=true;
					if( stResult != "SolveUp" )  stResult="Solved";
				}
				else{
					AnalysisResult=false; stResult="Unsolved";
					string culture = App_Environment.culture;
					ePZL.Sol_Result = (culture=="ja-JP")? stNotSolvedJP: stNotSolved;
				}
			}

			// ======================================================================================================		


			_PostProcessing();
		
			return (AnalysisResult,stResult);

					// ---------------------- inner functions ----------------------
						(int,int) _Set_AcceptableLevel( ){
							int _lvlLow=1, _lvlHgh=15;
							switch(G6.PG6Mode){	
								case "Func_CreateAuto":	 _lvlLow=G6.Puzzle_LevelLow; _lvlHgh=G6.Puzzle_LevelHigh; break;	
								case "Func_SolveMulti":  _lvlLow=1;					 _lvlHgh=G6.MSlvr_MaxLevel; break;	
							}
								//WriteLine( $" * * * Set_AcceptableLevel {G6.PG6Mode} : {_lvlLow} - {_lvlHgh}" );
							return (_lvlLow,_lvlHgh);
						}



						void _AnalysisSuccessful( UAlgMethod MTHD ){			// ###... This code needs to be rebuilt
							// @@ Basic solutions were applied. No advanced solutions were applied to the multiple solution search.
							int diffLevel = Abs(MTHD.difficultyLevel);			// method difficult level
							if( diffLevel <= 2 )  G6.Found_level_Solution = true;

							// for Create Puzzle
							// @@ Record the difficulty of the puzzle
							if( diffLevel > PZL_Initial.difficultyLevel ){
								PZL_Initial.difficultyLevel = diffLevel;
								PZL_Initial.pMethod = MTHD;
							}

							// for SolveUp
							// @@ Record the number of times the solution is applied.
							MTHD.UsedCC++;  // Counter for the number of times the algorithm has been applied.


							// for Multiple Analysis
							ePZL.pMethod = MTHD;

							// for development new Algorithm
							if( MTHD.markA_dev ){    // for development(ver.5.2-)
								PZL_Initial.g7MarkA = true;		//##@@ Needs organization and confirmation

								G6.g7MarkA0 = true;                // Record what was solved using the marked method
								G6.g7MarkA_MLst0.Add($"{stageNo}-{MTHD.MethodName}");
								//if(__ChkPritnt_SolverMethod) WriteLine( $" +++@@@ GNPX_Solver_SingleStage : {MTHD}" );
							}
						}
		 
									// <<< PostProcessing >>>
						void _PostProcessing(){
							G6.AnalysisResult = AnalysisResult;
							G6.stResult = stResult;

							var ePZL_Child_PZLz = ePZL.Child_PZLs;
							if( ePZL_Child_PZLz!=null && ePZL_Child_PZLz.Count>0 ){
								ePZL.selectedIX=0; ePZL=ePZL_Child_PZLz[0]; // solution found
							}
							SdkExecTime = AnalyzerLap.Elapsed;	
							AnalyzerLap.Stop();
						}     

		}
		#endregion Analyzer
	}
}